# 🎮 Game Scene Setup Guide

This guide explains how to set up the MainGameScene so cars spawn correctly with player control.

---

## Step 1: Create Spawn Points

Spawn points are where cars will appear in the game.

### In Unity:
1. **Create Empty GameObjects** for each spawn point
   - Right-click in Hierarchy → Create Empty
   - Rename to: `SpawnPoint_1`, `SpawnPoint_2`, `SpawnPoint_3`, `SpawnPoint_4`
   
2. **Position them** on your track/map:
   - Move each spawn point to where you want cars to start
   - Rotate them to face the direction cars should drive
   - Example positions:
     - SpawnPoint_1: Position (10, 0, 0), Rotation (0, 0, 0)
     - SpawnPoint_2: Position (-10, 0, 0), Rotation (0, 180, 0)
     - SpawnPoint_3: Position (0, 0, 10), Rotation (0, 90, 0)
     - SpawnPoint_4: Position (0, 0, -10), Rotation (0, -90, 0)

---

## Step 2: Create GameSpawnManager

This handles spawning cars when the game starts.

### In Unity:
1. **Create Empty GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Rename to: `GameSpawnManager`
   
2. **Add GameSpawnManager Script**:
   - Select `GameSpawnManager` object
   - In Inspector, click "Add Component"
   - Search for: `GameSpawnManager` (Script)
   - Click to add it

3. **Assign Car Prefabs**:
   - In Inspector, find "Car Prefabs" field
   - Set Size to match your number of cars (e.g., 4)
   - Drag your car prefabs from Project window into each slot:
     - Element 0: Car_Sedan prefab
     - Element 1: Car_Sports prefab
     - Element 2: Lamborghini prefab
     - Element 3: Nissan GTR prefab

4. **Assign Spawn Points**:
   - In Inspector, find "Spawn Points" field
   - Set Size to match number of spawn points (e.g., 4)
   - Drag your `SpawnPoint_1`, `SpawnPoint_2`, etc. into each slot

---

## Step 3: Setup CameraFollow

This makes the camera follow the player's car.

### In Unity:
1. **Select Main Camera**:
   - Click on `Main Camera` in Hierarchy
   
2. **Add CameraFollow Script**:
   - In Inspector, click "Add Component"
   - Search for: `CameraFollow` (Script)
   - Click to add it

3. **Configure Camera Settings** (optional):
   - **Offset**: Default is `(0, 6, -10)` - adjust height and distance
   - **Smooth Speed**: Default is `8` - higher = snappier following

---

## Step 4: Prepare Car Prefabs

Each car prefab MUST have these components:

### Required Components:
1. **NetworkIdentity** (Mirror component)
2. **CarPlayer** script (handles player control)
3. **CarController** script (handles driving physics)
4. **Car model/mesh** (visuals)

### How to Check:
1. Select your car prefab in Project window (e.g., `Assets/_Prefabs/Cars/Car_Sedan`)
2. In Inspector, verify you see:
   - ✅ NetworkIdentity
   - ✅ CarPlayer (Script)
   - ✅ CarController (Script)

### If Missing:
- **Missing NetworkIdentity**: Click "Add Component" → search "Network Identity"
- **Missing CarPlayer**: Click "Add Component" → search "CarPlayer"
- **Missing CarController**: Click "Add Component" → search "CarController"

---

## Step 5: Scene Setup Checklist

Before testing, verify:

| Component | Required | Location |
|-----------|----------|----------|
| GameSpawnManager GameObject | ✅ | Root of Hierarchy |
| GameSpawnManager Script | ✅ | On GameSpawnManager |
| Car Prefabs assigned | ✅ | In GameSpawnManager Inspector |
| Spawn Points assigned | ✅ | In GameSpawnManager Inspector |
| Spawn Point GameObjects | ✅ | Positioned on map |
| Main Camera | ✅ | Default camera |
| CameraFollow Script | * | On Main Camera |
| Car Prefabs with NetworkIdentity | ✅ | In Project window |
| Car Prefabs with CarPlayer | ✅ | In Project window |
| Car Prefabs with CarController | ✅ | In Project window |

---

## Step 6: Testing

### Test Flow:
1. **Build and Run** or **Play in Editor**
2. **Host** clicks "HOST" button
3. **Lobby Scene**: Select cars, mark ready
4. **Countdown**: Wait for countdown
5. **Game Scene**: Cars should spawn at spawn points
6. **Control**: Local player can drive with WASD/Arrow keys
7. **Camera**: Camera follows player's car

### Debug Tips:

**Cars not spawning?**
- Check Console for errors
- Verify `GameSpawnManager` has car prefabs assigned
- Verify car prefabs have `NetworkIdentity`

**Can drive but no camera?**
- Verify `CameraFollow` is on Main Camera
- Check Console for "CameraFollow component NOT FOUND" error

**Camera works but can't drive?**
- Verify car prefab has `CarController` script
- Check Console for "CarController component not found" error

**Wrong car spawned?**
- Verify `SaveAllPlayerData()` is being called (check Console logs)
- Check that lobby players have `selectedCarIndex` set correctly

---

## Quick Debug Commands (Console)

Add these to see what's happening:

```csharp
// In PlayerDataContainer.SaveAllPlayerData()
Debug.Log($"Saving {lobbyPlayers.Length} players");

// In GameSpawnManager.SpawnCarsFromSavedData()
Debug.Log($"Spawning {playerDataList.Count} cars");

// In CarPlayer.OnStartLocalPlayer()
Debug.Log($"Local player started: {gameObject.name}");
```

---

## File Locations Reference

| Script | Path |
|--------|------|
| GameSpawnManager | `Assets/resource/script/GameSpawnManager.cs` |
| CameraFollow | `Assets/resource/script/CameraFollow.cs` |
| CarPlayer | `Assets/resource/script/CarPlayer.cs` |
| PlayerDataContainer | `Assets/resource/MainMenuScene/PlayerDataContainer.cs` |

---

## Need Help?

Check the Console (Ctrl+Shift+C) for error messages. Common errors:

