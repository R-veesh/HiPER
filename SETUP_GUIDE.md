# HYPER Racing Game — Setup & Run Guide

## Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| Node.js | 18+ | `node --version` |
| npm | 9+ | `npm --version` (use `cmd /c "npm --version"` in PowerShell) |
| Unity | 6000.x | Open HYPER.sln |
| Browser | Chrome/Edge | For React web app |

---

## Part 1: Appwrite Cloud Setup

### 1.1 — Create Project
1. Go to [cloud.appwrite.io](https://cloud.appwrite.io)
2. Sign up / Log in
3. Click **Create Project** → Name it **HYPER**
4. Copy your **Project ID** (top of project settings)

### 1.2 — Create API Key
1. In your HYPER project → **Settings** → **API Keys**
2. Click **Create API Key**
3. Name: `backend-server`
4. Scopes: Select **ALL** scopes (or at minimum: `users.*`, `databases.*`, `storage.*`)
5. Copy the **API Key** value

### 1.3 — Create Database
1. Go to **Databases** → **Create Database**
2. Name: `hyper_db`
3. Copy the **Database ID**
4. Inside `hyper_db`, create these **5 collections**:

#### Collection: `profiles`
| Attribute | Type | Required | Size |
|-----------|------|----------|------|
| userId | String | Yes | 36 |
| displayName | String | Yes | 30 |
| age | Integer | No | — |
| bio | String | No | 500 |
| profilePicUrl | String | No | 500 |
| createdAt | String | Yes | 30 |

- Set **Document ID** permission: Use document-level permissions
- Add permission: `Any` → Read. `Users` → Read, Create, Update.

#### Collection: `wallets`
| Attribute | Type | Required |
|-----------|------|----------|
| userId | String | Yes |
| coinBalance | Integer | Yes |

- Permissions: same as profiles

#### Collection: `inventory`
| Attribute | Type | Required | Size |
|-----------|------|----------|------|
| userId | String | Yes | 36 |
| itemType | String | Yes | 10 |
| itemId | String | Yes | 50 |
| itemName | String | Yes | 100 |
| rarity | String | Yes | 20 |
| spriteUrl | String | No | 500 |
| obtainedAt | String | Yes | 30 |

#### Collection: `transactions`
| Attribute | Type | Required | Size |
|-----------|------|----------|------|
| userId | String | Yes | 36 |
| type | String | Yes | 20 |
| amount | Integer | Yes | — |
| description | String | No | 200 |
| timestamp | String | Yes | 30 |

#### Collection: `spin_config`
| Attribute | Type | Required | Size |
|-----------|------|----------|------|
| itemId | String | Yes | 50 |
| itemName | String | Yes | 100 |
| itemType | String | No | 10 |
| rarity | String | Yes | 20 |
| probability | Float | No | — |
| spriteUrl | String | No | 500 |

### 1.4 — Add Spin Items (Sample Data)
In the `spin_config` collection, click **Create Document** and add these sample cars:

```
{ itemId: "car_sedan",     itemName: "Street Sedan",     rarity: "Common",    itemType: "car" }
{ itemId: "car_hatch",     itemName: "Hot Hatch",        rarity: "Common",    itemType: "car" }
{ itemId: "car_coupe",     itemName: "Sport Coupe",      rarity: "Common",    itemType: "car" }
{ itemId: "car_muscle",    itemName: "Muscle Car",       rarity: "Rare",      itemType: "car" }
{ itemId: "car_gt",        itemName: "GT Racer",         rarity: "Rare",      itemType: "car" }
{ itemId: "car_turbo",     itemName: "Turbo Sprint",     rarity: "Epic",      itemType: "car" }
{ itemId: "car_hyper",     itemName: "Hyper X",          rarity: "Epic",      itemType: "car" }
{ itemId: "car_legend",    itemName: "Legend One",       rarity: "Legendary", itemType: "car" }
```

### 1.5 — Create Storage Bucket
1. Go to **Storage** → **Create Bucket**
2. Name: `avatars` — set the Bucket ID to `avatars`
3. Max file size: 2MB
4. Allowed extensions: `jpg, jpeg, png, webp`
5. Permissions: `Users` → Read, Create, Update, Delete

---

## Part 2: Backend Server Setup

### 2.1 — Configure Environment
Open `backend/.env` and fill in your Appwrite values:

```env
APPWRITE_ENDPOINT=https://cloud.appwrite.io/v1
APPWRITE_PROJECT_ID=<paste your Project ID>
APPWRITE_API_KEY=<paste your API Key>
APPWRITE_DATABASE_ID=<paste your Database ID>

JWT_SECRET=<make up a long random string, e.g. my-super-secret-key-12345>

PORT=3000
NODE_ENV=development

CORS_ORIGINS=http://localhost:5173,http://localhost:3001
```

> **IMPORTANT**: The `APPWRITE_DATABASE_ID` must match the ID shown in your Appwrite console for `hyper_db`. If you didn't set a custom ID, Appwrite auto-generated one — copy that.

### 2.2 — Install & Run

```bash
cd backend
npm install
npm run dev
```

You should see:
```
HYPER API server running on port 3000
```

### 2.3 — Test the Server
Open a browser or use curl:
```
http://localhost:3000/api/health
```
Expected response:
```json
{ "status": "ok", "timestamp": "2026-03-20T..." }
```

---

## Part 3: React Web App Setup

### 3.1 — Install & Run

```bash
cd web
npm install
npm run dev
```

You should see:
```
VITE v6.x  ready in XXms
➜  Local:   http://localhost:5173/
```

### 3.2 — Open in Browser
Go to `http://localhost:5173/`

You'll see the **Login page**. The web app proxies all `/api` requests to `localhost:3000` (backend), so both must be running simultaneously.

### 3.3 — Test Flow
1. Click **Register** → create an account (email, password 8+ chars, display name)
2. You'll land on the **Profile page** (500 starter coins shown)
3. Go to **Shop** → select a coin package → enter any 16-digit card number, any expiry (MM/YY), any 3-digit CVV → Purchase
4. Go to **Spin** → spend 100 coins for a gacha spin
5. Go to **Inventory** → see your owned cars, filter by rarity

---

## Part 4: Unity Setup

### 4.1 — Create the LoginScene

1. In Unity: **File → New Scene** → Save as `Assets/_Scenes/LoginScene.unity`
2. Add the scene to **Build Settings** (File → Build Settings → Add Open Scenes) — make it the **first scene** (index 0)

### 4.2 — Set Up LoginScene GameObjects

Create this hierarchy in the LoginScene:

```
LoginScene
├── Main Camera
├── EventSystem
├── --- Managers ---
│   ├── [ApiClient]          ← Add ApiClient.cs component
│   ├── [UserSession]        ← Add UserSession.cs component
│   └── [AuthManager]        ← Add AuthManager.cs component
│
└── Canvas (Screen Space - Overlay)
    ├── LoginPanel           ← Panel (Image background)
    │   ├── TitleText        ← TextMeshPro: "HYPER"
    │   ├── EmailField       ← TMP_InputField (Content Type: Email)
    │   ├── PasswordField    ← TMP_InputField (Content Type: Password)
    │   ├── LoginButton      ← Button
    │   ├── ErrorText        ← TextMeshPro (red, empty by default)
    │   └── SwitchToRegBtn   ← Button: "Create Account"
    │
    ├── RegisterPanel        ← Panel (start disabled)
    │   ├── NameField        ← TMP_InputField
    │   ├── EmailField       ← TMP_InputField
    │   ├── PasswordField    ← TMP_InputField (Password)
    │   ├── ConfirmPwField   ← TMP_InputField (Password)
    │   ├── RegisterButton   ← Button
    │   ├── ErrorText        ← TextMeshPro (red)
    │   └── SwitchToLoginBtn ← Button: "Back to Login"
    │
    └── LoadingOverlay       ← Panel with spinner (start disabled)
```

### 4.3 — Configure LoginUI Component

1. Add `LoginUI.cs` component to the Canvas (or a manager object)
2. In the Inspector, drag and assign:
   - `loginPanel` → LoginPanel
   - `loginEmailField` → LoginPanel/EmailField
   - `loginPasswordField` → LoginPanel/PasswordField
   - `loginButton` → LoginPanel/LoginButton
   - `switchToRegisterButton` → LoginPanel/SwitchToRegBtn
   - `loginErrorText` → LoginPanel/ErrorText
   - `registerPanel` → RegisterPanel
   - `registerNameField` → RegisterPanel/NameField
   - `registerEmailField` → RegisterPanel/EmailField
   - `registerPasswordField` → RegisterPanel/PasswordField
   - `registerConfirmPasswordField` → RegisterPanel/ConfirmPwField
   - `registerButton` → RegisterPanel/RegisterButton
   - `switchToLoginButton` → RegisterPanel/SwitchToLoginBtn
   - `registerErrorText` → RegisterPanel/ErrorText
   - `loadingOverlay` → LoadingOverlay
   - `mainMenuSceneName` → `MainMenuScene`

### 4.4 — Configure ApiClient

Select the `[ApiClient]` GameObject → in Inspector:
- **Base URL**: `http://localhost:3000` (for local dev)
- For deployed server: `https://your-app.onrender.com`

### 4.5 — Update Build Settings Scene Order

**File → Build Settings** → Arrange scenes:
```
0: LoginScene       ← First scene loaded
1: MainMenuScene
2: LobbyScene
3: MainGameScene
```

---

## Part 5: Lobby Camera Setup

### 5.1 — Create Camera Presets

In the **LobbyScene**:

1. Create 4 empty GameObjects:
   - `CamPreset_1Player` — position close to spawn point 1
   - `CamPreset_2Players` — medium shot covering spawn points 1-2
   - `CamPreset_3Players` — wider shot covering spawn points 1-3
   - `CamPreset_4Players` — full wide shot covering all 4 spawn points

2. Position each one where you want the camera to be for that player count. Set rotation to look at the players.

### 5.2 — Add LobbyCameraController

1. Select the **Main Camera** in LobbyScene
2. Add Component → `LobbyCameraController`
3. In Inspector:
   - `cameraPositions[0]` → drag `CamPreset_1Player`
   - `cameraPositions[1]` → drag `CamPreset_2Players`
   - `cameraPositions[2]` → drag `CamPreset_3Players`
   - `cameraPositions[3]` → drag `CamPreset_4Players`
   - `transitionSpeed` → `3` (adjust for feel)

### 5.3 — Coin Display in Lobby

1. In LobbyScene Canvas, create a new **TextMeshPro** text element (e.g. top-right corner)
2. Name it `CoinBalanceText`
3. Select the `LobbyUI` component → in Inspector, assign `coinBalanceText` → `CoinBalanceText`

---

## Part 6: Running Everything Together

### Local Development (3 terminals)

**Terminal 1 — Backend:**
```bash
cd backend
npm run dev
```

**Terminal 2 — Web App:**
```bash
cd web
npm run dev
```

**Terminal 3 — Unity:**
- Open Unity Editor
- Press **Play** in LoginScene

### Test Checklist

| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Web: Register account | Redirects to Profile, shows 500 coins |
| 2 | Web: Buy coins (Shop) | Any 16-digit card accepted, balance increases |
| 3 | Web: Spin gacha | Deducts 100 coins, shows car with rarity glow |
| 4 | Web: View inventory | Shows all owned cars with rarity badges |
| 5 | Unity: Login with same account | Loads MainMenuScene |
| 6 | Unity: Enter lobby | Coin balance shown in top corner |
| 7 | Unity: Multiple players join lobby | Camera smoothly zooms out |
| 8 | `localhost:3000/api/health` | `{ "status": "ok" }` |

---

## Part 7: Deploying to Production

### 7.1 — Deploy Backend to Render.com

1. Push `backend/` folder to a Git repo (GitHub/GitLab)
2. Go to [render.com](https://render.com) → **New Web Service**
3. Connect your repo, set:
   - **Root Directory**: `backend`
   - **Build Command**: `npm install`
   - **Start Command**: `npm start`
4. Add **Environment Variables** (same as `.env`):
   - `APPWRITE_ENDPOINT`, `APPWRITE_PROJECT_ID`, `APPWRITE_API_KEY`, `APPWRITE_DATABASE_ID`
   - `JWT_SECRET` (use a strong random value)
   - `PORT` = `3000`
   - `NODE_ENV` = `production`
   - `CORS_ORIGINS` = `https://your-web-app-url.vercel.app` (add your deployed web URL)
5. Deploy → note the URL (e.g. `https://hyper-api.onrender.com`)

### 7.2 — Deploy Web App to Vercel

1. Push `web/` folder to Git
2. Go to [vercel.com](https://vercel.com) → **Import Project**
3. Set **Root Directory**: `web`
4. Add Environment Variable:
   - `VITE_API_URL` = `https://hyper-api.onrender.com`
5. Update `web/src/api.js` to use the env var:
   ```js
   baseURL: import.meta.env.VITE_API_URL || '/api',
   ```
6. Deploy

### 7.3 — Update Unity for Production

In the `ApiClient` component in LoginScene:
- Change **Base URL** to `https://hyper-api.onrender.com`

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `npm` won't run in PowerShell | Use `cmd /c "npm run dev"` or set execution policy: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` |
| Backend: "Cannot connect to Appwrite" | Check `.env` values match your Appwrite console exactly |
| Web: Login fails with CORS error | Make sure backend `CORS_ORIGINS` includes `http://localhost:5173` |
| Web: 401 on all requests | Token expired or invalid — logout and login again |
| Unity: "Connection refused" | Make sure backend is running on port 3000 |
| Spin returns "No items configured" | Add sample data to `spin_config` collection (see Part 1.4) |
| Lobby camera doesn't move | Check `LobbyCameraController` has all 4 Transform presets assigned |
| Coin balance shows 0 in lobby | Ensure `UserSession` + `ApiClient` exist in LoginScene with DontDestroyOnLoad |

---

## File Map

```
HYPER/
├── backend/                    ← Express.js API
│   ├── server.js              ← Entry point (port 3000)
│   ├── .env                   ← Your secrets (not committed)
│   ├── .env.example           ← Template for .env
│   ├── config/appwrite.js     ← Appwrite client
│   ├── middleware/auth.js     ← JWT verification
│   └── routes/
│       ├── auth.js            ← Register/Login/Logout
│       ├── profile.js         ← Profile CRUD + avatar
│       ├── coins.js           ← Balance/Purchase/Spend
│       ├── inventory.js       ← Owned items list
│       └── spin.js            ← Gacha spin logic
│
├── web/                       ← React web app
│   ├── src/
│   │   ├── api.js             ← Axios + JWT interceptor
│   │   ├── App.jsx            ← Routes
│   │   ├── context/
│   │   │   └── AuthContext.jsx ← Auth state
│   │   └── pages/
│   │       ├── LoginPage.jsx
│   │       ├── RegisterPage.jsx
│   │       ├── ProfilePage.jsx
│   │       ├── ShopPage.jsx
│   │       ├── SpinPage.jsx
│   │       └── InventoryPage.jsx
│   └── vite.config.js         ← Dev proxy to backend
│
└── Assets/resource/
    ├── Common/
    │   └── ApiClient.cs        ← HTTP wrapper
    ├── LoginScene/
    │   ├── UserSession.cs      ← Session singleton
    │   ├── AuthManager.cs      ← Auth API calls
    │   ├── LoginUI.cs          ← Login/Register UI
    │   └── ProfileUI.cs        ← Profile edit UI
    └── LobbyScene/
        ├── LobbyCameraController.cs ← Dynamic camera
        └── LobbyUI.cs          ← (modified: coinBalanceText)
```
