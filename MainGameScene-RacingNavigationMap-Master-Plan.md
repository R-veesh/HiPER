# MainGameScene Racing Navigation Map - Master Plan

## Goal
Add a racing navigation map (minimap) to `MainGameScene` so players can see:
- their own car position and heading,
- other racers on the track,
- checkpoint and finish-line locations.

## Scope
In scope:
- Runtime minimap logic for mapping world XZ coordinates into HUD space.
- Local and remote car markers.
- Checkpoint/finish markers.
- Integration with existing `RacingHUD` local-car discovery flow.
- Unity setup guidance and verification steps.

Out of scope:
- Full GPS route line rendering.
- Dynamic zoom by speed.
- Animated path arrows between checkpoints.

## Design
1. Add `RacingNavigationMap` MonoBehaviour in `Assets/resource/MainGameScene/`.
2. Use configurable map world bounds (`worldMinX/worldMaxX/worldMinZ/worldMaxZ`) for coordinate conversion.
3. Spawn checkpoint markers from scene `Checkpoint` objects at startup.
4. Refresh player markers at a controlled interval for performance.
5. Mark local player with dedicated marker prefab (optional).
6. Rotate marker with car heading (optional toggle).
7. Reuse `RacingHUD` local-player detection and pass local target into minimap.

## Implementation Steps
1. Create `RacingNavigationMap.cs` with:
- Map references (`mapRect`, `markerRoot`, marker prefabs).
- World-to-map conversion method.
- Checkpoint marker build method.
- Player marker create/update/cleanup methods.
2. Update `RacingHUD.cs`:
- Add `navigationMap` serialized reference.
- On local car found, call `navigationMap.SetLocalTarget(cp.transform)`.
3. Add docs for scene wiring and testing.

## Validation Plan
1. Open `MainGameScene`, attach `RacingNavigationMap` to HUD canvas object.
2. Assign map panel and marker prefabs.
3. Set world bounds to track extents.
4. Run host + client:
- local marker appears and rotates,
- remote marker appears,
- checkpoint markers render,
- markers move smoothly while racing.
5. Verify no impact on race start/finish flow.

## Risks and Mitigations
- Incorrect world bounds -> markers appear offset.
  - Mitigation: expose bounds in Inspector and document calibration steps.
- Missing prefabs/references -> map appears empty.
  - Mitigation: null checks and setup checklist.
- Frequent object search overhead.
  - Mitigation: throttled refresh interval (`refreshInterval`).

## Future Enhancements
1. Next-checkpoint arrow with route hint.
2. Dynamic zoom based on speed/lap context.
3. Team colors or rank-based marker colors.
4. Route polyline overlay.
