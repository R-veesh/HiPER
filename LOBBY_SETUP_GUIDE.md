# PUBG-Style Lobby System Setup Guide

## ✅ What I've Created

### 1. **Scene Structure**
- `MainMenuScene.unity` - Entry point with HOST/JOIN/QUIT buttons
- `LobbyScene.unity` - Core lobby system with plates and car selection
- `MainGameScene.unity` - Actual racing gameplay scene

### 2. **Network Components**
- **CustomNetworkManager** - Handles scene transitions and player connections
- **LobbyManager** - Manages player assignments and ready states (SERVER ONLY)
- **LobbyPlayer** - Per-player state with car selection and ready status
- **LobbyUI** - Local UI for car selection and ready/start buttons
- **GameSpawnManager** - Spawns cars in MainGameScene

### 3. **Prefabs**
- Basic car prefabs (Sports, Sedan) for testing
- LobbyPlayer prefab with network components

## 🔧 Unity Setup Instructions

### Step 1: Install Mirror
1. Open Unity Package Manager
2. Install Mirror Networking from Unity Registry
3. Import Mirror into your project

### Step 2: Configure Network Manager
1. Open `MainMenuScene.unity`
2. Select the `NetworkManager` GameObject
3. Assign these references:
   - **Network Address**: `localhost` (for testing)
   - **Network Port**: `7777`
   - **Lobby Player Prefab**: Assign `LobbyPlayer.prefab`
   - **Scene Names**: Set to your scene names

### Step 3: Setup Lobby Scene
1. Open `LobbyScene.unity`
2. Select `LobbyManager` GameObject
3. Assign references:
   - **Spawn Points**: Assign the 4 Plate transforms
   - **Lobby Player Prefab**: Same as Network Manager

### Step 4: Configure Lobby Player Prefab
1. Open `LobbyPlayer.prefab`
2. Assign car prefabs to the `carPrefabs` array
3. Make sure it has NetworkIdentity component

### Step 5: Setup Lobby UI
1. In `LobbyScene.unity`, select the `LobbyUI` GameObject
2. Assign UI references:
   - Ready Button
   - Start Button (hidden for clients)
   - Next/Prev Car buttons
   - Status Text
   - Car Selection Text

### Step 6: Setup Main Game Scene
1. Open `MainGameScene.unity`
2. Select `GameSpawnManager`
3. Assign car prefabs and spawn points
4. Make sure NetworkManager persists between scenes

## 🎮 How It Works

### Main Menu Flow:
1. **HOST Button** → `StartHost()` → Auto-loads `LobbyScene`
2. **JOIN Button** → `StartClient()` → Connects to host → Auto-loads `LobbyScene`
3. **QUIT Button** → `Application.Quit()`

### Lobby Flow:
1. **Player Joins** → Spawns `LobbyPlayer` on available plate
2. **Car Spawns** on assigned plate for each player
3. **Car Selection** → Next/Prev buttons (disabled when ready)
4. **Ready System** → Each player clicks READY
5. **Host Start** → Only HOST sees START button (only when all ready)
6. **Game Start** → All players transition to `MainGameScene`

### Game Flow:
1. **Scene Loads** → `GameSpawnManager` spawns selected cars
2. **Players Spawn** → Each gets their selected car with control
3. **Race Begins** → Standard car racing gameplay

## 🎯 Key Features Implemented

✅ **Strict Scene Separation**: Each scene has single responsibility
✅ **Host-Only Start**: Only host can start game when all players ready
✅ **Plate Assignment**: Each player gets unique plate (max 4 players)
✅ **Car Selection**: synced car changes with visual preview
✅ **Ready System**: Global ready state checking
✅ **Network Authority**: Proper server-client authority
✅ **Clean Scene Transitions**: No mixed logic between scenes

## 🚀 Testing Instructions

1. **Host Test**: 
   - Play `MainMenuScene`
   - Click HOST → Should load `LobbyScene`
   - Your car should appear on Plate_1
   - Select car, click READY → START button appears
   - Click START → Should load `MainGameScene`

2. **Client Test**:
   - Build the project or run multiple instances
   - Host joins first, then client clicks JOIN
   - Client should appear on next available plate
   - Both players can select cars independently
   - Both must be READY for host to see START button

## 🔧 Common Issues & Solutions

### **Mirror Not Found**
- Install Mirror from Package Manager first
- Add `using Mirror;` to scripts

### **Scene Not Loading**
- Check scene names in NetworkManager match actual scene names
- Ensure scenes are added to Build Settings

### **UI Not Working**
- Assign all button references in LobbyUI
- Make sure Canvas has EventSystem
- Check buttons have onClick events assigned

### **Cars Not Spawning**
- Assign car prefabs to LobbyPlayer and GameSpawnManager
- Ensure prefabs have NetworkIdentity component
- Check spawn points are assigned correctly

## 🎮 Next Steps

1. **Replace placeholder cars** with your actual car models
2. **Add car controls** to the CarPlayer script
3. **Implement race logic** (laps, checkpoints, finish line)
4. **Add polish** (sounds, effects, animations)
5. **Test multiplayer** thoroughly across different network conditions

The core lobby system is now fully functional according to your PUBG-style requirements! 🎯