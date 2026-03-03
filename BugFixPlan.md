

 I don't have file creation/editing tools available in this session. Here's the markdown content — you can save it manually as a `.md` file (e.g., `d:\unity_project\HYPER\BugFixPlan.md`):

```markdown
# Master Bug Fix Plan — Car Spawning & Camera Following

**TL;DR:** There are 7 interconnected bugs that prevent cars from spawning correctly and the camera from following the local player. The root cause chain is: LobbyPlayer objects survive into the game scene and spawn phantom preview cars → GameSpawnManager may have wrong prefabs assigned or spawns cars for disconnected players without authority → `CarPlayer.OnStartLocalPlayer()` never fires → camera never gets a target. The fix requires changes to 4 scripts and 1 Unity Inspector cleanup.

---

## Flow Overview

```
1. MainMenuManager.OnHostClicked()
   → NetworkManager.StartHost()
   → CustomNetworkManager.OnStartHost() → ServerChangeScene(lobbyScene)

2. LobbyScene loads
   → CustomNetworkManager.OnServerAddPlayer() → Instantiate(lobbyPlayerPrefab)
   → LobbyManager.OnPlayerAdded() → assigns spawn plates
   → LobbyPlayer.SpawnPreviewCar() → spawns car preview in lobby

3. Players ready up → LobbyManager.StartGame()
   → PlayerDataContainer.SaveAllPlayerData() → stores connectionId, carIndex, etc.
   → CustomNetworkManager.LoadGameScene() → ServerChangeScene(gameScene)

4. Game scene loads
   → CustomNetworkManager.OnServerSceneChanged() → StartCoroutine(SpawnCarsInGameScene)
   → (0.5s delay) → GameSpawnManager.OnSceneLoaded()
   → WaitAndSpawnCars() → waits for clients ready → SpawnCarsFromSavedData()
   → For each player: SpawnCarForPlayerData() → Instantiate(carPrefabs[carIndex])
   → NetworkServer.Spawn(car, conn) → gives authority to player

5. Client side: CarPlayer.OnStartLocalPlayer()
   → EnableControl() → enables CarController + FindObjectOfType<CameraFollow>().SetTarget()

6. CameraFollow.LateUpdate() → follows target via smooth lerp + LookAt
```

---

## Bugs Identified

| # | Issue | File | Severity |
|---|-------|------|----------|
| 1 | LobbyPlayer spawns preview cars in game scene | `Assets/resource/LobbyScene/LobbyPlayer.cs` | Critical |
| 2 | Wrong prefab in `carPrefabs[]` array (Inspector) | `Assets/resource/script/GameSpawnManager.cs` | Critical |
| 3 | Disconnected players still get cars spawned (no authority) | `Assets/resource/script/GameSpawnManager.cs` | Critical |
| 4 | Camera never finds target (chain failure) | `CarPlayer.cs` + `CameraFollow.cs` | Critical |
| 5 | Duplicate `CarSpawner` vs `GameSpawnManager` | `Assets/resource/script/CarSpawner.cs` | High |
| 6 | CameraFollow double-searches in Update + LateUpdate | `Assets/resource/script/CameraFollow.cs` | Medium |
| 7 | PlayerDataContainer not cleaned on disconnect | `Assets/resource/MainMenuScene/CustomNetworkManager.cs` | High |

---

## Steps

### Step 1: Guard `LobbyPlayer.SpawnPreviewCar()` — Lobby Scene Only

**File:** `Assets/resource/LobbyScene/LobbyPlayer.cs`

Mirror preserves player objects across scene changes. When the game scene loads, SyncVar hooks (`OnCarChanged`) re-fire which calls `SpawnPreviewCar()`, spawning unwanted preview cars in the game scene.

**Changes:**
- In `SpawnPreviewCar()` (line 89), add scene guard at the top:
  ```csharp
  if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "LobbyScene") return;
  ```
- Apply the same guard in `OnStartClient()` (line 31) and `OnCarChanged()` (line 136).
- Also destroy `_previewCar` when leaving the lobby scene (in `OnStopClient` or add cleanup in `OnServerSceneChanged`).

---

### Step 2: Skip Spawning Cars for Disconnected Players

**File:** `Assets/resource/script/GameSpawnManager.cs`

`SpawnCarForPlayerData()` (line 87) spawns a car even when the connection lookup fails (`conn == null`). This creates orphaned cars with no authority that nobody can control.

**Changes:**
- In `SpawnCarForPlayerData()`, if `conn` is `null`, **skip spawning entirely** — log a warning and return early instead of calling `NetworkServer.Spawn(car)` without an owner.
- Replace lines 130–136:
  ```csharp
  if (conn != null)
  {
      NetworkServer.Spawn(car, conn);
  }
  else
  {
      // DO NOT spawn — no owner means no one can control it
      Destroy(car);
      Debug.LogWarning($"[GameSpawnManager] Skipped spawn for {playerData.playerName} — connection {playerData.connectionId} not found (disconnected)");
      return;
  }
  ```
- Optionally, prune stale entries from `PlayerDataContainer` before spawning — in `WaitAndSpawnCars()` (line 276), after confirming clients are ready, remove any `PlayerGameData` entries whose `connectionId` is not in `NetworkServer.connections`.

---

### Step 3: Fix `carPrefabs[]` in Unity Inspector

**File:** Game Scene → `GameSpawnManager` component

The debug log shows `"Spawned car LobbyPlayer for player..."` — this means `carPrefabs[]` on `GameSpawnManager` is assigned the **LobbyPlayer prefab** instead of actual car prefabs.

**Changes (Inspector):**
- Open the game scene in Unity Editor.
- Select the GameObject with `GameSpawnManager`.
- Set `carPrefabs[]` to the same car prefabs used in `LobbyPlayer.carPrefabs[]` — the actual vehicle models that have `CarController`, `CarPlayer`, and `NetworkIdentity` components.
- Ensure each car prefab is registered in `CustomNetworkManager` → **Registered Spawnable Prefabs** list.

---

### Step 4: Remove or Disable `CarSpawner` from the Game Scene

**File:** `Assets/resource/script/CarSpawner.cs`

`CarSpawner.cs` is an older duplicate spawner. Its `OnStartServer()` (line 14) also tries to spawn cars using `LobbyPlayer` references on connections — which won't exist after scene change. This causes either double-spawns or errors.

**Changes:**
- Remove the `CarSpawner` component from any GameObject in the game scene.
- Optionally delete `CarSpawner.cs` entirely if no other scene uses it.

---

### Step 5: Fix `CameraFollow` Double-Search and Timing

**File:** `Assets/resource/script/CameraFollow.cs`

`CameraFollow` calls `FindLocalPlayerCar()` in both `Update()` (line 55) and `LateUpdate()` (line 93) when `target == null`, causing duplicate searches every frame.

**Changes:**
- Remove the `FindLocalPlayerCar()` call from inside `LateUpdate()`. Let `Update()` handle the periodic search (every 0.5s), and `LateUpdate()` only does following.
- Change initial `searchTimer` to `0.5f` so the first search happens immediately:
  ```csharp
  private float searchTimer = 0.5f; // Search immediately on first frame
  ```
- Add a `maxSearchTime` (e.g., 30s) to stop scanning after timeout and log an error:
  ```csharp
  private float totalSearchTime = 0f;
  private const float MAX_SEARCH_TIME = 30f;
  ```

---

### Step 6: Add Retry in `CarPlayer.OnStartLocalPlayer()`

**File:** `Assets/resource/script/CarPlayer.cs`

`OnStartLocalPlayer()` (line 23) calls `EnableControl()` which does `FindObjectOfType<CameraFollow>()`. If `CameraFollow` hasn't initialized yet (race condition), the camera won't be set.

**Changes:**
- Add a coroutine retry: if `FindObjectOfType<CameraFollow>()` returns `null`, retry after 0.5s for up to 5 attempts:
  ```csharp
  public override void OnStartLocalPlayer()
  {
      base.OnStartLocalPlayer();
      Debug.Log("[CarPlayer] OnStartLocalPlayer called for: " + gameObject.name);
      EnableControl();
      
      // Retry camera assignment if it failed
      if (FindObjectOfType<CameraFollow>()?.target != transform)
      {
          StartCoroutine(RetryCameraAssignment());
      }
  }

  private IEnumerator RetryCameraAssignment()
  {
      for (int i = 0; i < 5; i++)
      {
          yield return new WaitForSeconds(0.5f);
          CameraFollow cam = FindObjectOfType<CameraFollow>();
          if (cam != null)
          {
              cam.SetTarget(transform);
              Debug.Log("[CarPlayer] Camera target set on retry #" + (i + 1));
              yield break;
          }
      }
      Debug.LogError("[CarPlayer] Failed to find CameraFollow after 5 retries!");
  }
  ```

---

### Step 7: Clean `PlayerDataContainer` on Player Disconnect

**File:** `Assets/resource/MainMenuScene/CustomNetworkManager.cs` + `PlayerDataContainer.cs`

`OnServerDisconnect()` (line 226) notifies `LobbyManager` but does NOT remove the disconnected player's data from `PlayerDataContainer`. This causes stale data to persist and orphan cars to be spawned.

**Changes:**

In `PlayerDataContainer.cs`, add:
```csharp
[Server]
public void RemovePlayerData(int connectionId)
{
    for (int i = playerDataList.Count - 1; i >= 0; i--)
    {
        if (playerDataList[i].connectionId == connectionId)
        {
            Debug.Log($"[PlayerDataContainer] Removed data for connection {connectionId}");
            playerDataList.RemoveAt(i);
            return;
        }
    }
}
```

In `CustomNetworkManager.OnServerDisconnect()`, add:
```csharp
PlayerDataContainer.Instance?.RemovePlayerData(conn.connectionId);
```

---

## Verification

1. **Single-player test (Host only):** Start as host → lobby → select car → ready → game scene loads → car spawns → camera follows → WASD controls work.
2. **Multi-player test (Host + 1 Client):** Both join lobby → select different cars → ready up → game loads → each player controls their own car, camera follows their own car.
3. **Disconnect test:** Host + 2 clients in lobby → 1 client disconnects before game starts → game loads with only 2 cars (not 3). No orphaned cars.
4. **Console check:**
   - NO `[LobbyPlayer] SpawnPreviewCar` logs after game scene loads.
   - NO `"connection null"` warnings.
   - `[CarPlayer] OnStartLocalPlayer` and `[CameraFollow] Target set to` should appear for each player.

---

## Decisions

- **Skip spawning for disconnected players entirely** (rather than spawning AI cars) — simplest fix, avoids orphan cars.
- **Keep `CameraFollow` polling search as safety net** alongside `CarPlayer.SetTarget()` — belt-and-suspenders for reliability.
- **Remove CarSpawner.cs** in favor of GameSpawnManager.cs — only one spawn system should exist.
```

You can copy this and save it as `BugFixPlan.md` in your project, or would you like me to suggest a specific location?You can copy this and save it as `BugFixPlan.md` in your project, or would you like me to suggest a specific location?


Kilo said

Now I have a clear picture of the "database" (player data system) in this project. Let me provide a comprehensive analysis:

/plane — Player Data "Database" Check
Here's a full analysis of the player data system (the in-memory "database") used across the project:

📦 Data Structure — PlayerDataContainer.PlayerGameData
PlayerGameData {
  connectionId      : int
  playerName        : string
  selectedCarIndex  : int
  selectedMapIndex  : int
  isReady           : bool
}
Stored in a SyncList<PlayerGameData> — synced across the Mirror network.

🔴 CRITICAL ISSUES FOUND
1. PlayerGameData is NOT serializable by Mirror's SyncList
PlayerDataContainer.cs:16 — The inner class PlayerGameData uses [System.Serializable] but Mirror's SyncList requires types to implement IEquatable<T> or be a struct. A plain class in a SyncList will not sync correctly and will throw runtime errors.

Fix: Convert PlayerGameData to a struct, or implement IEquatable<PlayerGameData>.

2. Missing file: Assets/resource/script/PlayerDataContainer.cs
The VSCode open tabs show Assets/resource/script/PlayerDataContainer.cs but this file does not exist in the directory. The actual file is at Assets/resource/MainMenuScene/PlayerDataContainer.cs. This will cause a compile error if any script in the resource.script namespace references it.

3. PlayerDataContainer is a NetworkBehaviour but spawned via new GameObject()
In CustomNetworkManager.cs:81:

GameObject containerObj = new GameObject("PlayerDataContainer");
containerObj.AddComponent<PlayerDataContainer>();
NetworkBehaviour components must be on a prefab registered with Mirror and spawned via NetworkServer.Spawn(). Creating them with new GameObject() means they will never have a NetworkIdentity and Mirror will never sync the SyncList to clients.

4. SaveAllPlayerData() is never called before scene transition
PlayerDataContainer.cs:57 — SaveAllPlayerData() must be called before ServerChangeScene() to capture lobby data. Searching the codebase, this call is missing from LobbyManager or CustomNetworkManager before the game scene loads.

5. GetPlayerData() returns null for a class — callers don't null-check
GameSpawnManager.cs:156:

var data = playerDataContainer.GetPlayerData(conn.connectionId);
if (data != null) { ... }
This one is handled, but if PlayerGameData becomes a struct (fix #1), returning null won't compile — needs to return PlayerGameData? (nullable struct).

✅ What Works Correctly
Component	Status
LobbyPlayer SyncVars (selectedCarIndex, isReady, playerName, plateIndex, selectedMapIndex)	✅ Correct
GameSpawnManager fallback to connected players	✅ Correct
CustomNetworkManager.OnServerDisconnect calling RemovePlayerData	✅ Correct
DontDestroyOnLoad on PlayerDataContainer	✅ Correct (but only if properly spawned)
🔧 Required Fixes Summary
Convert PlayerGameData class → struct in PlayerDataContainer.cs:16
Add NetworkIdentity to PlayerDataContainer prefab and register it — don't create via new GameObject()
Call PlayerDataContainer.Instance.SaveAllPlayerData() in LobbyManager before calling ServerChangeScene to game scene
Update GetPlayerData() return type to PlayerGameData? if converting to struct
Would you like me to apply these fixes?


Checkpoint
(Current)
