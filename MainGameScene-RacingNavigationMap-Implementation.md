# MainGameScene Racing Navigation Map - Implementation

## Step By Step Setup
1. Open Unity and load `Assets/Scenes/MainGameScene.unity`.
2. In Hierarchy, select your race HUD Canvas (the object that has `RacingHUD`).
3. Create minimap UI:
- Right click Canvas -> UI -> `Image` -> rename `MiniMapPanel`.
- Set anchor to top-right, size about `220 x 220`.
4. Create marker root:
- Right click `MiniMapPanel` -> Create Empty -> rename `MarkerRoot`.
- Add `RectTransform` (if not present), stretch to full panel.
5. Add script:
- Select Canvas (or `MiniMapPanel`) -> Add Component -> `RacingNavigationMap`.
6. Assign references in `RacingNavigationMap`:
- `Map Rect` = `MiniMapPanel` RectTransform
- `Marker Root` = `MarkerRoot` RectTransform
7. Create `PlayerMarker` prefab:
- Under `MarkerRoot`, create UI Image named `PlayerMarker`.
- Size `12 x 12`, color yellow.
- Drag to Project to make prefab, then delete scene instance.
- Assign to `playerMarkerPrefab`.
8. Create `LocalPlayerMarker` prefab:
- Duplicate `PlayerMarker`, rename `LocalPlayerMarker`.
- Change color to cyan/green and size `14 x 14`.
- Make prefab and assign to `localPlayerMarkerPrefab`.
9. Create `CheckpointMarker` prefab:
- Create UI Image named `CheckpointMarker`.
- Size `8 x 8`, white color.
- Make prefab and assign to `checkpointMarkerPrefab`.
10. Connect HUD:
- Select object with `RacingHUD`.
- In Inspector, set `navigationMap` to the object with `RacingNavigationMap`.
11. Configure map bounds in `RacingNavigationMap`:
- `worldMinX = -200`
- `worldMaxX = 200`
- `worldMinZ = -200`
- `worldMaxZ = 200`
12. Set behavior:
- `rotateMarkersWithCar = true`
- `refreshInterval = 0.15`
13. Ensure race checkpoints exist:
- Confirm `Checkpoint` components are on track trigger objects.
- Finish line checkpoint must have `isFinishLine = true`.
14. Press Play (Host mode) and verify:
- Minimap visible.
- Local car marker appears and moves.
- Checkpoint markers appear.
15. Multiplayer test:
- Run Host + Client.
- Confirm second car marker appears and updates.
16. Calibrate bounds:
- If markers hit edge too early, increase bounds.
- If markers stay clustered at center, reduce bounds.
17. Save scene once aligned.

## Runtime Notes
- Script file: `Assets/resource/MainGameScene/RacingNavigationMap.cs`
- HUD integration: `Assets/resource/MainGameScene/RacingHUD.cs`
- This feature depends on correct Inspector references.
You’re describing network movement jitter, usually called:
- rubberbanding (car snaps back/forward),
- replication lag (state arrives late),
- or interpolation delay (smooth but delayed remote motion).
In your project, this is very likely a replication setup issue, not PC performance.
I checked your current setup and found the main risk:
- Cars use Mirror.NetworkTransformUnreliable with client authority (syncDirection: 1) in car prefabs like Assets/resource/Car/car_list/2007 Lamborghini Reventon+online (1).prefab.
- Cars also have a non-kinematic Rigidbody, but no NetworkRigidbody component.
- This combo often causes visible jitter/rubberbanding for remote players because physics and transform sync fight each other.
Also, your game scene spawner currently has only one car prefab in Assets/Scenes/MainGameScene.unity:52417, so replication consistency across different selected cars can break if selections differ.
Plan to Fix (no edits yet, plan mode)
- 1) Stabilize physics replication (highest impact)
  - Replace NetworkTransformUnreliable on drivable car prefabs with NetworkRigidbodyUnreliable (same client authority direction).
  - Keep transform sync but let Mirror set non-owned rigidbodies kinematic automatically.
  - Apply to all online car prefabs:
    - Assets/resource/Car/2007-lamborghini-reventon/2007 Lamborghini Reventon+online (1).prefab
    - Assets/resource/Car/car_list/2007 Lamborghini Reventon+online (1).prefab
    - Assets/resource/Car/car_list/2008 Bugati+online.prefab
    - Assets/resource/Car/car_list/2009 toyora+online 1.prefab
- 2) Tune network smoothing for race feel
  - Keep syncInterval around 0.05 (20Hz) initially; optionally test 0.033 (30Hz) if bandwidth allows.
  - Keep interpolation enabled; tune bufferResetMultiplier (start 3, test 2.5–4) to balance smoothness vs delay.
  - Keep onlySyncOnChange on unless micro-stops cause stutter.
- 3) Fix prefab registration/spawn consistency
  - Ensure all selectable car prefabs are in NetworkManager spawnable prefabs (currently only one is listed in Assets/Scenes/MainMenuScene.unity:19982).
  - Ensure GameSpawnManager.carPrefabs in Assets/Scenes/MainGameScene.unity:52417 contains the same full set/order as lobby car selection.
- 4) Remove duplicate spawner risk
  - Verify CarSpawner script is not active as a second spawning path; keep GameSpawnManager as single source.
  - (Scene currently has object named CarSpawner with GameSpawnManager, which is fine naming-wise, but ensure no old CarSpawner component exists.)
- 5) Validation checklist (host + client)
  - Compare: owner car feel (should be immediate) vs remote car smoothness (should stop snapping).
  - Test under artificial latency (Unity transport simulator if available): 60–120ms RTT.
  - Watch logs for authority warnings/missing spawnable prefab warnings.
  - Confirm no duplicated cars and no null-authority spawns.
- 6) Build gate after fix
  - Run playtest matrix first (Host only, Host+1, Host+2 with disconnect case), then do final build.
If you want, next step I’ll execute this plan in order: prefab/network component updates first, then scene registrations, then multiplayer verification, then build.