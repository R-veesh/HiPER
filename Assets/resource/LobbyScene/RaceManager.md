# Win / Defeat System — Setup Guide

> All scripts are in `Assets/resource/MainGameScene/`

---

## Scripts Overview

| Script | Type | Purpose |
|--------|------|---------|
| `RaceManager.cs` | NetworkBehaviour (singleton) | Server-authoritative race logic: tracks checkpoints, laps, finish order |
| `Checkpoint.cs` | MonoBehaviour | Attach to trigger colliders along the track |
| `RaceResultUI.cs` | MonoBehaviour | UI panel showing finish positions + return button |
| `CarPlayer.cs` | NetworkBehaviour | **Already has** `OnTriggerEnter` + `CmdReportCheckpoint` (added at bottom) |
| `GameSpawnManager.cs` | NetworkBehaviour | **Already calls** `RaceManager.RegisterPlayer()` after spawn |

---

## Step-by-Step Scene Setup

### 1. Create the RaceManager object

1. In your **game scene** (MainGameScene), create an **empty GameObject** → name it `RaceManager`
2. Add components:
   - **NetworkIdentity** (required for any networked object)
   - **RaceManager** script
3. In the Inspector, set:
   - **Total Laps** — how many laps to win (default `1`)
   - **Total Checkpoints** — number of Checkpoint objects you place (e.g., `3`)

> The RaceManager is a **scene object**, NOT a prefab. No need to register it in Spawnable Prefabs.

---

### 2. Place Checkpoints around the track

Create empty GameObjects along your track. Each one needs:

| Component | Setting |
|-----------|---------|
| **Box Collider** (or Sphere Collider) | ✅ `Is Trigger = true`, make it wide enough for cars |
| **Checkpoint** script | Set `checkpointIndex` and `isFinishLine` |

#### Checkpoint numbering

Checkpoints are numbered **starting from 0** in the order cars should drive through them.
The **last checkpoint** (highest index) must have `isFinishLine = true`.

```
Example with 3 checkpoints:

  [CP 0] ──→ [CP 1] ──→ [CP 2 (isFinishLine ✅)] = 1 lap
```

#### Rules
- Cars **must** hit checkpoints in order — skipping is ignored (anti-shortcut)
- A lap only completes when the car hits the **finish-line checkpoint** AND has passed all previous checkpoints
- After a lap completes, the checkpoint counter resets to 0

#### Sizing tips
- Make colliders **tall** (Y scale ~5) so cars can't jump over
- Make them **wide** enough to cover the full road width + a buffer
- The yellow/green gizmos in Scene view show their positions

---

### 3. Set up the Race Result UI

1. Create a **Canvas** in the game scene (or use an existing one)
2. Add the **RaceResultUI** script to the Canvas
3. Create these child UI elements and drag them into the RaceResultUI inspector slots:

| Inspector Slot | UI Element | Notes |
|---------------|------------|-------|
| `resultPanel` | Panel (GameObject) | Set **active = false** by default — it shows automatically |
| `resultText` | Text (UI) | Displays finish list: "1st - PlayerName", "2nd - ..." |
| `statusText` | Text (UI) | Shows "PlayerName Wins!" for 1st place |
| `returnButton` | Button (UI) | Takes player back to lobby. Can start hidden, shown on race complete |

> Uses legacy `UnityEngine.UI.Text`. If you prefer TextMeshPro, change the `Text` fields to `TMP_Text` in the script and add `using TMPro;`.

---

### 4. Verify Car Prefab

Your car prefab should already have:

- ✅ **Rigidbody** (or a non-trigger Collider) — needed for `OnTriggerEnter` to fire
- ✅ **CarPlayer** script — already contains checkpoint detection code
- ✅ **NetworkIdentity** — required for Mirror networking

**Nothing to add here** — the car code is already integrated.

---

## How It Works (Flow)

```
1. Game scene loads
   ↓
2. GameSpawnManager spawns cars with NetworkServer.Spawn(car, conn)
   → calls RaceManager.RegisterPlayer(netId, playerName) for each car
   ↓
3. Cars drive through Checkpoint trigger colliders
   ↓
4. CarPlayer.OnTriggerEnter() detects Checkpoint component
   → sends CmdReportCheckpoint(index, isFinishLine) to server
   ↓
5. RaceManager.ServerReportCheckpoint() validates:
   - Is this the NEXT expected checkpoint? (anti-cheat)
   - If yes → advance counter
   - If finish line + all checkpoints passed → complete lap
   - If all laps done → player FINISHED
   ↓
6. RpcPlayerFinished(playerName, position) → all clients
   → RaceResultUI.ShowPlayerFinished() displays "1st - PlayerName"
   ↓
7. When ALL players finish:
   → RpcRaceComplete() → all clients
   → RaceResultUI.ShowRaceComplete() shows return button
   ↓
8. Player clicks Return → disconnects and goes back to lobby
```

---

## Inspector Checklist

| Object | Component | Setting | Value |
|--------|-----------|---------|-------|
| RaceManager (GameObject) | NetworkIdentity | — | — |
| RaceManager (GameObject) | RaceManager | Total Laps | `1` (or more) |
| RaceManager (GameObject) | RaceManager | Total Checkpoints | Must equal number of CP objects |
| Checkpoint_0 (GameObject) | Collider | Is Trigger | ✅ |
| Checkpoint_0 (GameObject) | Checkpoint | checkpointIndex | `0` |
| Checkpoint_0 (GameObject) | Checkpoint | isFinishLine | ❌ |
| Checkpoint_1 (GameObject) | Collider | Is Trigger | ✅ |
| Checkpoint_1 (GameObject) | Checkpoint | checkpointIndex | `1` |
| Checkpoint_1 (GameObject) | Checkpoint | isFinishLine | ❌ |
| Checkpoint_2 (GameObject) | Collider | Is Trigger | ✅ |
| Checkpoint_2 (GameObject) | Checkpoint | checkpointIndex | `2` |
| Checkpoint_2 (GameObject) | Checkpoint | isFinishLine | ✅ |
| Canvas (GameObject) | RaceResultUI | resultPanel | → drag Panel |
| Canvas (GameObject) | RaceResultUI | resultText | → drag Text |
| Canvas (GameObject) | RaceResultUI | statusText | → drag Text |
| Canvas (GameObject) | RaceResultUI | returnButton | → drag Button |

---

## Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| Checkpoints not detected | Collider `Is Trigger` not checked | Enable `Is Trigger` on checkpoint colliders |
| Checkpoints not detected | Car has no Rigidbody | Add Rigidbody to car prefab |
| "hit checkpoint X but expected Y" in Console | Player drove through checkpoints out of order | Re-check `checkpointIndex` values — they must be sequential |
| Lap never completes | Last checkpoint missing `isFinishLine = true` | Check the `isFinishLine` box on the highest-index checkpoint |
| `totalCheckpoints` mismatch | Inspector value doesn't match actual checkpoint count | Count your Checkpoint objects and update the Inspector value |
| UI not showing | RaceResultUI references not assigned | Drag the Panel/Text/Button into the Inspector slots |
| Return button does nothing | Button not wired | Should auto-wire via `Awake()` — check RaceResultUI is on the Canvas |

---

## What Was Changed in Existing Files

### CarPlayer.cs (bottom of file — nothing above was touched)
```csharp
// ── Checkpoint / Race detection (additive) ──

void OnTriggerEnter(Collider other)
{
    if (!isOwned) return;
    Checkpoint cp = other.GetComponent<Checkpoint>();
    if (cp != null)
        CmdReportCheckpoint(cp.checkpointIndex, cp.isFinishLine);
}

[Command]
void CmdReportCheckpoint(int checkpointIndex, bool isFinishLine)
{
    if (RaceManager.Instance != null)
        RaceManager.Instance.ServerReportCheckpoint(netIdentity, checkpointIndex, isFinishLine);
}
```

### GameSpawnManager.cs (one line added after each spawn)
```csharp
// after spawnedCars.Add(car);
if (RaceManager.Instance != null)
    RaceManager.Instance.RegisterPlayer(car.GetComponent<NetworkIdentity>(), playerName);
```
