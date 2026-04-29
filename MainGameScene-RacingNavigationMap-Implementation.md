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
