# MainMenuScene Master Plan

## Goal
Build a complete MainMenu flow with:
1) Mode selection (Offline Game / LAN Game),
2) Web-based Store integration,
3) Full Profile management,
4) Car progression + challenge map level system,
while keeping current online lobby flow stable.

---

## Current System (Observed)
- `Assets/resource/MainMenuScene/MainMenuManager.cs` currently supports Host/Join/Quit only.
- `Assets/resource/MainMenuScene/CustomNetworkManager.cs` handles LAN Host/Join + Lobby/Game scene transitions.
- `Assets/resource/LoginScene/UserSession.cs` has basic user data (id, email, displayName, token, coinBalance).
- `Assets/resource/LoginScene/ProfileUI.cs` already supports simple profile load/save (name/age/bio).
- `Assets/resource/LobbyScene/MapData.cs` exists and is reusable for map/challenge metadata.
- Lobby car select already exists (`LobbyUI`, `LobbyPlayer`) for online mode.

---

## Feature Breakdown

## 1) Main Menu UX Rework

### 1.1 New root menu buttons
- Add two main buttons:
  - `Offline Game`
  - `LAN Game`
- Keep `Quit` visible globally.

### 1.2 LAN panel (existing behavior retained)
- On `LAN Game` click, show current panel:
  - Host
  - Join
  - IP input
  - Quit/Back
- Reuse current `MainMenuManager` + `CustomNetworkManager` logic.

### 1.3 Offline panel
- On `Offline Game` click, show:
  - Challenge map list (ordered)
  - Selected map details (name, difficulty, reward)
  - Current selected car preview (Next/Prev)
  - Play button
- No lobby scene for offline.
- Direct load to race scene with selected map/car.

---

## 2) Offline Challenge + Level Progression

### 2.1 Challenge progression rules
- Map sequence unlock model:
  - First challenge map unlocked by default.
  - Win map N => unlock map N+1.
- Player level increases when challenge map is completed (or based on XP).
- Track:
  - `playerLevel`
  - `totalMatches`
  - `matchesWon`
  - `currentChallengeIndex`
  - `unlockedMaps[]`

### 2.2 Data source
- Use backend as source of truth.
- Local cache for offline fallback (PlayerPrefs / local JSON).

### 2.3 Offline race completion flow
- On race result:
  - if win => unlock next map, add coins, add XP/level progress.
  - save progress (API + local cache fallback).
  - show `Next Challenge` button.

---

## 3) Store Integration (Web-based)

### 3.1 Store button behavior
- Add `Store` button in main menu.
- On click:
  - `Application.OpenURL(storeUrl)` (optionally with token/user id query).
- Web store handles purchase/unlock.

### 3.2 Sync after returning to game
- Add `RefreshInventory` action:
  - Fetch latest coin balance.
  - Fetch owned cars / unlocks.
  - Update local UI immediately.

### 3.3 Car unlock dependency
- Next/Prev car list should only cycle through owned cars in offline mode.
- In online lobby:
  - either allow all,
  - or enforce owned-only (decision-based; recommend owned-only for consistency).

---

## 4) Profile Management (Main Menu Panel)

### 4.1 Profile display fields
- Show:
  - profile image
  - display name
  - email
  - level
  - total matches
  - matches won
  - coins
  - owned cars count/list

### 4.2 Edit profile
- Edit screen:
  - change display name
  - change profile image
- Save via API.
- Update `UserSession` and visible UI immediately after success.

### 4.3 Existing code reuse
- Reuse/extend `ProfileUI.cs` endpoints for profile data.
- Expand DTOs to include gameplay stats and image URL.

---

## 5) Car Selection Behavior (Critical Rule)

### Offline mode
- Car selection happens in MainMenu offline panel.
- Selected car is used directly when starting offline race.

### Online mode
- Keep current lobby car select (`NextCar/PrevCar`) in LobbyScene.
- Do not force offline selected car into lobby unless explicitly desired.
- Optional enhancement: preselect lobby car based on profile preference.

---

## Technical Implementation Plan (Execution Order)

## Phase A - UI/Navigation Foundation
1. Refactor `MainMenuManager` into panel-based state controller:
   - ModeSelectPanel
   - LANPanel
   - OfflinePanel
   - Store/Profile buttons
2. Wire button listeners and back navigation.

## Phase B - Data Models + Services
3. Create `PlayerProgress` model (level, wins, unlocks, ownedCars).
4. Create `ProgressService` for:
   - load from API,
   - fallback local cache,
   - save/update methods.
5. Extend `UserSession` to include profile stats fields.

## Phase C - Offline Challenge System
6. Build challenge map list UI using `MapData` assets.
7. Add unlock gating and next-map progression logic.
8. Integrate offline race start and pass selected car/map into game scene.

## Phase D - Store + Inventory Sync
9. Add store URL launcher (`Application.OpenURL`).
10. Add inventory refresh endpoint call and UI binding.
11. Restrict offline car selector to owned cars.

## Phase E - Profile Management Upgrade
12. Add profile summary panel in main menu.
13. Add edit modal for name + image.
14. Sync update to backend and session cache.

## Phase F - Game Result Hooks
15. Update race-complete path to call progression reward logic.
16. Apply coin/xp/level/unlock updates.
17. Show next challenge CTA after win.

## Phase G - QA/Validation
18. Test matrix:
   - Offline progression unlock flow
   - LAN host/join unchanged
   - Store open + sync
   - Profile edit persistence
   - Car ownership restrictions
19. Regression check on Lobby ready/start flow.

---

## Suggested New/Updated Scripts

### Update
- `Assets/resource/MainMenuScene/MainMenuManager.cs`
- `Assets/resource/LoginScene/UserSession.cs`
- `Assets/resource/LoginScene/ProfileUI.cs`
- `Assets/resource/Common/ApiClient.cs` (if new endpoints/helpers needed)

### New (suggested)
- `Assets/resource/MainMenuScene/MainMenuUIStateController.cs`
- `Assets/resource/MainMenuScene/OfflineModeController.cs`
- `Assets/resource/MainMenuScene/ChallengeProgressService.cs`
- `Assets/resource/MainMenuScene/StoreLauncher.cs`
- `Assets/resource/MainMenuScene/ProfileSummaryPanel.cs`
- `Assets/resource/MainMenuScene/OwnedCarFilter.cs`
- `Assets/resource/MainMenuScene/OfflineRaceConfig.cs` (DontDestroyOnLoad handoff data)

---

## API/Backend Requirements (Minimum)
- `GET /api/profile/me` -> profile + stats + image + level
- `PUT /api/profile` -> name/image update
- `GET /api/coins/balance`
- `GET /api/inventory/cars` -> owned cars
- `POST /api/progression/offline-result` -> map result, rewards, unlocks
- (Optional) `GET /api/challenges/state`

---

## Acceptance Criteria
- Main menu has Offline/LAN split UX.
- LAN flow still works like now (Host/Join/IP/Quit).
- Offline mode supports challenge list, sequential unlock, next challenge.
- Winning challenge updates level + coins.
- Store opens web page and unlocked cars/coins sync back.
- Profile panel shows all requested fields and supports edit (name/image).
- Offline uses main-menu-selected car; online uses lobby selection.
