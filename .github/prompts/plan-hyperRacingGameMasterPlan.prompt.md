# HYPER Racing Game — Master Business Plan

## Overview
Multiplayer car racing game (Unity 6 + Mirror) with backend services (Express.js + Appwrite Cloud), web coin shop (React.js), user accounts, car gacha/spin system, and skin inventory.

## Architecture

```
Unity Game Client ←→ Express.js API (Render.com) ←→ Appwrite Cloud (DB + Auth + Storage)
                          ↑
React.js Web App ─────────┘  (Coin shop, profile, car spin)
```

## Current State (What exists)
- Mirror multiplayer: MainMenuScene → LobbyScene → MainGameScene
- Lobby: 4 spawn points, car selection, ready system, map voting, countdown
- Game: RaceManager + checkpoints + HUD + result screen
- PlayerDataContainer: DontDestroyOnLoad, carries car selection data
- NO: auth, login, profile, coins, shop, skins, inventory, persistent data

## User Answers
- Web: React.js
- Payment: Fake card payment only (test/dev mode, no real money)
- Gacha: Tiered rarity (Common/Rare/Epic/Legendary)
- Lobby camera: All players visible (zoom out when more join)
- Hosting: Appwrite Cloud (DB) + Express.js on Render.com

---

## Phase 1: Backend Foundation (Express.js + Appwrite)

### 1.1 — Express.js Server Setup
- Location: New folder `backend/` in project root (or separate repo)
- Stack: Express.js, Appwrite Node SDK, cors, helmet, express-validator
- Hosting: Render.com (free/starter tier)
- Files:
  - `backend/server.js` — Express entry point
  - `backend/routes/auth.js` — login/register endpoints
  - `backend/routes/profile.js` — CRUD profile
  - `backend/routes/coins.js` — coin balance, purchase, spend
  - `backend/routes/inventory.js` — owned cars/skins
  - `backend/routes/spin.js` — gacha spin endpoint
  - `backend/middleware/auth.js` — JWT/session token validation
  - `backend/config/appwrite.js` — Appwrite client init

### 1.2 — Appwrite Cloud Setup
- Project: Create "HYPER" project on cloud.appwrite.io
- Services used:
  - **Auth**: Email/password login (Appwrite Auth built-in)
  - **Database**: Collections for profiles, coin transactions, inventory, spin history
  - **Storage**: Profile picture uploads (Appwrite Storage bucket)
- Collections:
  - `profiles` — { userId, displayName, age, bio, profilePicUrl, createdAt }
  - `wallets` — { userId, coinBalance }
  - `inventory` — { userId, itemType("car"|"skin"), itemId, rarity, obtainedAt }
  - `transactions` — { userId, type("purchase"|"spin"|"reward"), amount, timestamp }
  - `spin_config` — { itemId, itemName, rarity, probability, spriteUrl }

### 1.3 — Auth Endpoints
- `POST /api/auth/register` — { email, password, displayName } → Appwrite createAccount + create profile + wallet(500 starter coins)
- `POST /api/auth/login` — { email, password } → Appwrite createEmailPasswordSession → return JWT
- `POST /api/auth/logout` — invalidate session
- `GET /api/auth/me` — return current user info

---

## Phase 2: User Profiles

### 2.1 — Profile API
- `GET /api/profile/:userId` — get profile (name, age, bio, pic)
- `PUT /api/profile` — update profile (auth required)
- `POST /api/profile/avatar` — upload profile picture → Appwrite Storage

### 2.2 — Profile in Unity (Login Scene)
- New scene: `LoginScene` (before MainMenuScene)
- New scripts:
  - `Assets/resource/LoginScene/LoginUI.cs` — email/password fields, login/register buttons
  - `Assets/resource/LoginScene/AuthManager.cs` — HTTP calls to Express API, stores JWT token
  - `Assets/resource/LoginScene/ProfileUI.cs` — edit name, age, bio, upload picture
  - `Assets/resource/LoginScene/UserSession.cs` — DontDestroyOnLoad singleton, holds userId, token, displayName, coinBalance

### 2.3 — Scene Flow Change
- `LoginScene` → `MainMenuScene` → `LobbyScene` → `MainGameScene`
- `UserSession` persists across all scenes

---

## Phase 3: Coin System

### 3.1 — Coin API
- `GET /api/coins/balance` — return current coin balance
- `POST /api/coins/purchase` — fake card purchase → add coins to wallet
  - Request: { amount, cardNumber(fake), expiry, cvv }
  - Validation: accept any 16-digit card (test mode)
  - Coin packages: 100/$0.99, 500/$3.99, 1200/$7.99, 3000/$14.99
- `POST /api/coins/spend` — deduct coins (for spins)
  - Validates sufficient balance server-side

### 3.2 — Coin Display in Unity Lobby
- Modify `LobbyUI.cs` — add coin balance TextMeshPro element
- `UserSession.coinBalance` synced on lobby enter via GET /api/coins/balance
- Coin display updates after any transaction

---

## Phase 4: Car Spin (Gacha) System

### 4.1 — Spin API
- `POST /api/spin` — deduct coins → weighted random pick → add to inventory → return result
- Cost: 100 coins per spin
- Rarity tiers + probabilities:
  - Common (60%) — basic cars/skins
  - Rare (25%) — mid-tier cars
  - Epic (12%) — premium cars
  - Legendary (3%) — exclusive cars
- Server-side random (not client-side) to prevent cheating
- Response: { itemId, itemName, rarity, spriteUrl, isNew }

### 4.2 — Spin in React Web App
- Spin wheel / card-flip animation
- Shows result with rarity glow effect
- "Spin Again" or "Back to Inventory" buttons

### 4.3 — Spin in Unity (optional later)
- Could add gacha UI in lobby or separate scene
- Lower priority — web version comes first

---

## Phase 5: Web Platform (React.js)

### 5.1 — React App Structure
- Location: `web/` folder in project root (or separate repo)
- Pages:
  - `/login` — email/password login
  - `/register` — create account
  - `/profile` — view/edit profile (name, age, bio, avatar)
  - `/shop` — buy coin packages (fake card form)
  - `/spin` — car gacha spinner
  - `/inventory` — view owned cars and skins
- Stack: React + React Router + Axios + TailwindCSS (or CSS modules)

### 5.2 — Coin Shop Page
- Display coin packages with prices
- Fake credit card form (card number, expiry, CVV)
- Accept any 16-digit number as valid (test mode)
- Show confirmation + updated balance after purchase

### 5.3 — Skin/Car Inventory Page
- Grid of owned cars with rarity badge
- Filter by rarity (Common/Rare/Epic/Legendary)
- Show "equipped" badge on selected car

---

## Phase 6: Lobby Camera System

### 6.1 — Dynamic Lobby Camera
- New script: `Assets/resource/LobbyScene/LobbyCameraController.cs`
- Camera positions array (1-4 presets matching player count)
- When player count changes → smooth lerp camera to new preset position
- Position presets:
  - 1 player: close-up on spawn point 1
  - 2 players: medium shot covering spawn points 1-2
  - 3 players: wider shot covering spawn points 1-3
  - 4 players: full wide shot covering all 4 spawn points
- Hook into `LobbyManager.OnPlayerAdded()` / `OnPlayerRemoved()` to trigger camera change
- Smooth transition using `Vector3.Lerp` + `Quaternion.Slerp` over ~1 second

### 6.2 — Camera Presets (set in Inspector)
- `cameraPositions: Transform[]` — 4 empty GameObjects in lobby scene marking camera positions
- `transitionSpeed: float = 3f`

---

## Phase 7: Unity-Backend Integration

### 7.1 — HTTP Client in Unity
- New utility: `Assets/resource/Common/ApiClient.cs` — UnityWebRequest wrapper
  - Base URL config (Render.com Express URL)
  - JWT token header injection
  - JSON serialize/deserialize
  - Async/await or coroutine-based

### 7.2 — Login Flow in Unity
1. LoginScene → user enters email/password
2. AuthManager calls POST /api/auth/login
3. On success → store token in UserSession → load MainMenuScene
4. On fail → show error message

### 7.3 — Lobby Coin Integration
1. On lobby enter → GET /api/coins/balance
2. Display in LobbyUI coin text
3. After spin/purchase on web → balance updates on next lobby refresh

### 7.4 — Car Selection from Inventory
- Future: car selection in lobby filtered by owned cars
- Current: all cars available (no restriction until inventory system mature)

---

## Implementation Order (Priority)

| Step | Phase | Depends On | Effort |
|------|-------|------------|--------|
| 1 | 1.1 Express setup | Nothing | Small |
| 2 | 1.2 Appwrite setup | Nothing | Small |
| 3 | 1.3 Auth endpoints | 1.1, 1.2 | Medium |
| 4 | 2.1 Profile API | 1.3 | Medium |
| 5 | 3.1 Coin API | 1.3 | Medium |
| 6 | 4.1 Spin API | 3.1 | Medium |
| 7 | 5.1-5.3 React web app | 1.3, 3.1, 4.1 | Large |
| 8 | 2.2-2.3 Unity login scene | 1.3 | Medium |
| 9 | 7.1-7.3 Unity-backend integration | 8 | Medium |
| 10 | 6.1-6.2 Lobby camera | Nothing | Small |
| 11 | 3.2 Coin in lobby | 7.3 | Small |

Step 10 (lobby camera) can run in parallel with backend work (Steps 1-9).

---

## Decisions
- Fake payments only — no real payment processor, accept any test card
- Appwrite handles auth (email/password) — Express proxies/validates
- Car spin is server-side randomization (anti-cheat)
- Web platform is primary for shop/spin, Unity integration follows
- Lobby camera uses preset positions (not dynamic framing algorithm)
- 4-player max maintained (existing lobby constraint)
