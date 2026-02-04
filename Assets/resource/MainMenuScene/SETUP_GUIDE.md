# MainMenuScene Setup Guide

## 🔧 UNITY EDITOR SETUP REQUIRED

### 1. NetworkManager Setup
- Select NetworkManager GameObject
- Add `CustomNetworkManager` script
- Assign references:
  - `mainMenuPanel`: Drag MainMenuPanel
  - `lobbyPanel`: Drag LobbyPanel
  - `playerSetupPanel`: Drag PlayerSetupPanel
  - `ipInputField`: Drag IPInputField
  - `addPlayerButton`: Drag AddPlayerButton (in PlayerSetupPanel)
  - `startLocalGameButton`: Drag StartLocalGameButton (in PlayerSetupPanel)
  - `lobbyUI`: Drag LobbyUI GameObject

### 2. MainMenuPanel Setup
- Select MainMenuPanel GameObject
- Add `MainMenuUI` script
- Assign references:
  - `ipInput`: Drag IPInputField
  - `hostButton`: Drag HostButton
  - `joinButton`: Drag JoinButton
  - `localMultiplayerButton`: Drag LocalMultiplayerButton
  - `quitButton`: Drag QuitButton

### 3. PlayerSetupPanel Setup
- Create PlayerSetupPanel GameObject (disabled by default)
- Add UI elements:
  - `AddPlayerButton`: Button to add local players
  - `StartLocalGameButton`: Button to start local game (initially disabled)
  - Player count display (optional)

### 4. LobbyPanel Setup
- Select LobbyPanel GameObject  
- Add `LobbyUI` script
- Assign references:
  - `playerListText`: Drag PlayerListText (TextMeshPro)
  - `readyButton`: Drag ReadyButton
  - `startGameButton`: Drag StartGameButton

### 5. LobbyPlayer Prefab
- Create LobbyPlayer GameObject
- Add `NetworkIdentity` (set as Server Authority)
- Add `LobbyPlayer` script
- Add to `CustomNetworkManager` -> `Registered Spawnable Prefabs`

### 6. Button Setup
**MainMenuPanel Buttons**:
- HostButton: OnClick -> MainMenuUI.OnClickHost()
- JoinButton: OnClick -> MainMenuUI.OnClickJoin()  
- LocalMultiplayerButton: OnClick -> MainMenuUI.OnClickLocalMultiplayer()
- QuitButton: OnClick -> MainMenuUI.OnClickQuit()

**PlayerSetupPanel Buttons**:
- AddPlayerButton: OnClick -> CustomNetworkManager.AddLocalPlayer()
- StartLocalGameButton: OnClick -> CustomNetworkManager.StartLocalGame()

**LobbyPanel Buttons**:
- ReadyButton: OnClick -> LobbyUI.OnClickReady()
- StartGameButton: OnClick -> LobbyUI.OnClickStartGame()

## 🎯 FIXED WORKFLOW

✅ **START STATE**
- MainMenuPanel = ACTIVE
- LobbyPanel = DISABLED
- PlayerSetupPanel = DISABLED

✅ **HOST FLOW**
1. Click Host → `MainMenuUI.OnClickHost()` → `CustomNetworkManager.OnClickHost()`
2. `StartHost()` → `OnStartHost()` → Switch to Lobby
3. `OnServerAddPlayer()` → Spawn LobbyPlayer
4. `LobbyUI.RegisterPlayer()` → Update UI

✅ **JOIN FLOW**  
1. Enter IP → Click Join → `MainMenuUI.OnClickJoin()` → `CustomNetworkManager.OnClickJoin()`
2. `StartClient()` → `OnStartClient()` → Switch to Lobby
3. Host spawns LobbyPlayer for client
4. `LobbyUI.RegisterPlayer()` → Update UI on all clients

✅ **LOCAL MULTIPLAYER FLOW**
1. Click Local Multiplayer → `MainMenuUI.OnClickLocalMultiplayer()` → `CustomNetworkManager.StartLocalMultiplayer()`
2. Switch to PlayerSetupPanel
3. Click Add Player → `CustomNetworkManager.AddLocalPlayer()` (max 4 players)
4. Start Local Game button enables when ≥2 players
5. Click Start Local Game → `CustomNetworkManager.StartLocalGame()` → `StartHost()`
6. Enter Lobby with local players

✅ **READY FLOW**
1. Click Ready → `LobbyUI.OnClickReady()`
2. `LobbyPlayer.CmdToggleReady()` → Server updates SyncVar
3. SyncVar hook → `LobbyUI.Refresh()` on all clients

✅ **START GAME FLOW**
1. All players ready → Start button enables (host only)
2. Host clicks Start → `LobbyUI.OnClickStartGame()`
3. `CustomNetworkManager.ServerChangeScene("GameScene")`
4. All clients move to GameScene

## 🐛 DEBUG TIPS

- Check Console for "LobbyUI:" debug messages
- Verify all UI references are assigned in Inspector
- Ensure LobbyPlayer prefab is in Registered Spawnable Prefabs
- Test with 2+ Unity instances for multiplayer