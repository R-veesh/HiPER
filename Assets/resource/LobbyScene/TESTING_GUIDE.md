# 🧪 LOBBY SYSTEM TESTING GUIDE

## ✅ **Critical Fixes Applied**

### 1. Scene Name Mismatch - FIXED
- **Issue**: `CustomNetworkManager` referenced `"MainGameScene"` but actual scene is `"GameScene"`
- **Fix**: Updated `gameScene` variable in `CustomNetworkManager.cs:9`
- **Status**: ✅ COMPLETED

### 2. Host-Only Start Button - FIXED  
- **Issue**: Start button showed for any server client
- **Fix**: Added `NetworkServer.isHost` check in `LobbyUI.cs:92`
- **Status**: ✅ COMPLETED

### 3. Spawn Points Auto-Setup - FIXED
- **Issue**: `LobbyManager.spawnPoints` array was empty
- **Fix**: Created `LobbySetupHelper.cs` to auto-find and assign spawn points
- **Status**: ✅ COMPLETED

## 🎮 **How to Test**

### **Step 1: Scene Setup**
1. Open `LobbyScene.unity`
2. Add the `LobbySetupHelper.cs` component to any GameObject
3. Add the `LobbyTester.cs` component to any GameObject  
4. Press Play - auto-setup will run automatically

### **Step 2: Host Testing**
1. Build the project
2. Run 2+ instances of the built game
3. **Instance 1**: Click "Host"
4. **Verify**: LobbyScene loads, player spawns at Plate 1
5. **Check Console**: "✅ Lobby setup validation PASSED"

### **Step 3: Client Testing**  
1. **Instance 2**: Click "Join" → enter host's IP (usually "127.0.0.1")
2. **Verify**: Client joins lobby, spawns at next available plate
3. **Verify**: Both players see each other correctly

### **Step 4: Ready System Testing**
1. **Player 1**: Click "Ready" button
2. **Verify**: Button text changes to "NOT READY"
3. **Verify**: Car selection buttons disable
4. **Player 2**: Click "Ready" button  
5. **Verify**: Start button appears for HOST only

### **Step 5: Game Start Testing**
1. **Host**: Click "Start" button
2. **Verify**: GameScene loads for all clients
3. **Verify**: Smooth transition without errors

## 🔍 **Test Validation Points**

### **Console Should Show:**
```
✅ Found X spawn points
✅ Lobby player prefab assigned  
🎯 Lobby setup validation PASSED
🧪 Lobby Tester initialized
✅ TEST PASSED: Found X spawn points
✅ TEST PASSED: Network setup validated
✅ Ready button found
✅ Start button found
✅ Status text found
```

### **In-Game Should Show:**
- [ ] Host spawns at first available plate
- [ ] Clients spawn at subsequent plates  
- [ ] Ready button toggles correctly
- [ ] Car selection disables when ready
- [ ] Start button only appears for host
- [ ] All players see synchronized ready states

## 🚨 **Troubleshooting**

### **If "LobbySpawns not found":**
- Check that `LobbySpawns` GameObject exists in LobbyScene
- Verify it has child Transform objects for spawn points

### **If "LobbyManager not found":**
- Ensure LobbyManager prefab is in LobbyScene
- Check that LobbyManager component is active

### **If players don't spawn:**
- Verify `lobbyPlayerPrefab` is assigned in NetworkManager
- Check that prefab has LobbyPlayer component
- Ensure spawn points have valid positions

### **If Start button doesn't appear:**
- Verify all players are ready
- Check that `NetworkServer.isHost` returns true for host
- Ensure `LobbyUI` references are correct

## 🎯 **Success Criteria**

✅ **All tests pass when:**
- Host can create lobby successfully
- Multiple clients can join and spawn correctly  
- Ready system works for all players
- Only host sees Start button when all ready
- Game scene loads without errors
- No console errors or warnings

## 📝 **Next Steps**

1. **Run the test suite** using the built game instances
2. **Verify all validation points** pass
3. **Test edge cases** (disconnect/reconnect)
4. **Performance test** with max players

The lobby system is now ready for comprehensive testing! 🚀