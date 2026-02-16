# PUBG-Style Lobby System - Setup Guide

## ✅ Implemented Features

### 1. **Team Plates System**
- 4 player plates showing player info
- Player name, car selection, ready status
- Visual ready indicators (green = ready, gray = not ready)
- Highlight local player with gold border
- Empty slot indicators for waiting players

### 2. **Map Selection & Voting**
- MapData ScriptableObject for track configuration
- Map preview images, descriptions, difficulty levels
- Player voting system (vote for preferred map)
- Host can override map selection
- Vote counter showing current votes

### 3. **Countdown Timer**
- 5-second auto-start countdown when all players ready
- Cancel countdown if any player un-readies
- Visual countdown display with animations
- Color changes (white → orange → red)
- "GO!" animation at countdown end
- Sound effect support

### 4. **Bug Fixes**
- Fixed EditorBuildSettings scene paths
- Fixed scene name consistency
- Proper map scene switching

---

## 🎮 Unity Setup Instructions

### Step 1: Create MapData Asset

1. In Unity, right-click in **Project window**
2. Select **Create → Racing → Map Data**
3. Name it "DefaultTrack" or your track name
4. Configure:
   - **Map Name**: Display name (e.g., "City Circuit")
   - **Description**: Brief description
   - **Scene Name**: Must match your scene file (e.g., "MainGameScene")
   - **Difficulty**: Easy/Medium/Hard
   - **Laps**: Number of laps for this track
   - **Map Preview**: Assign a Sprite for the map image

### Step 2: Setup LobbyScene

#### Required GameObjects in LobbyScene:

**1. LobbyManager GameObject**
```
Create Empty → Name: "LobbyManager"
Add Components:
- LobbyManager script
- NetworkIdentity
```

Configure LobbyManager:
```
- Spawn Points: Assign 4 transforms for player plates
- Lobby Player Prefab: Assign your LobbyPlayer prefab
- Available Maps: Add your MapData asset(s)
```

**2. LobbyCountdown GameObject**
```
Create Empty → Name: "LobbyCountdown"
Add Components:
- LobbyCountdown script
- NetworkIdentity
```

**3. Canvas Setup (UI)**

Create UI structure:
```
Canvas (Screen Space - Overlay)
└── LobbyUI (GameObject with LobbyUI script)
    ├── PlayerPlatesContainer (Horizontal Layout Group)
    │   ├── PlayerPlate1 (with PlayerPlateUI script)
    │   ├── PlayerPlate2 (with PlayerPlateUI script)
    │   ├── PlayerPlate3 (with PlayerPlateUI script)
    │   └── PlayerPlate4 (with PlayerPlateUI script)
    ├── MapSelectionPanel (GameObject with MapSelectionPanel script)
    │   ├── MapPreview (Image)
    │   ├── MapNameText (TextMeshPro)
    │   ├── DifficultyText (TextMeshPro)
    │   ├── VoteCountText (TextMeshPro)
    │   ├── PreviousButton (Button)
    │   ├── NextButton (Button)
    │   └── VoteButton (Button)
    ├── CountdownDisplay (GameObject with CountdownDisplay script)
    │   ├── CountdownPanel
    │   └── CountdownText (TextMeshPro - size 100+)
    ├── ControlButtons
    │   ├── ReadyButton
    │   ├── StartButton (Host only)
    │   ├── NextCarButton
    │   ├── PrevCarButton
    │   └── LeaveButton
    └── StatusPanel (Background image + StatusText)
```

### Step 3: Configure LobbyUI Script

Assign references in LobbyUI component:
```
Player Plates:
- Player Plate 1-4: Assign your plate GameObjects

Map Selection:
- Map Selection Panel: Assign MapSelectionPanel GameObject

Countdown:
- Countdown Display: Assign CountdownDisplay GameObject

Control Buttons:
- Ready Button, Start Button, etc.

UI Text Elements:
- Ready Button Text, Status Text, etc.

Car Selection:
- Car Names: ["Sports Car", "Truck", "F1", "Muscle"]
- Car Preview Sprites: Assign sprite array
```

### Step 4: Configure PlayerPlateUI

Each player plate needs:
```
PlayerPlateUI script with:
- Player Name Text
- Player Status Text
- Car Name Text
- Plate Background (Image)
- Ready Indicator (Image)
- Car Preview Image (optional)
- Empty Slot Overlay
```

### Step 5: Configure MapSelectionPanel

```
MapSelectionPanel script with:
- Map Preview Image
- Map Name Text
- Map Description Text
- Difficulty Text
- Vote Count Text
- Laps Text
- Previous/Next/Vote Buttons
```

### Step 6: Configure CountdownDisplay

```
CountdownDisplay script with:
- Countdown Text (TextMeshPro - large font)
- Countdown Panel (GameObject)
- GO Effect (optional animation object)
- AudioSource component for sounds
```

### Step 7: Configure CustomNetworkManager

In MainMenuScene:
```
CustomNetworkManager already exists
Verify:
- Main Menu Scene: "MainMenuScene"
- Lobby Scene: "LobbyScene"
- Game Scene: "MainGameScene"
- Lobby Player Prefab: Assigned
```

---

## 🎨 PUBG-Style Visual Tips

### Color Scheme:
```
Background: Dark gray/blue (#1a1a2e or similar)
Ready: Bright green (#4CAF50)
Not Ready: Gray (#757575)
Highlight (Local Player): Gold (#FFD700)
Warning: Orange (#FF9800)
Danger: Red (#F44336)
```

### UI Layout (PUBG-Style):
```
┌─────────────────────────────────────┐
│  [Player1]  [Player2]  [Player3]  [Player4]  │  ← Top: Player plates
├─────────────────────────────────────┤
│                                     │
│         [MAP PREVIEW IMAGE]         │  ← Center: Map selection
│         City Circuit                │
│         Difficulty: Medium          │
│         Laps: 3                     │
│         Votes: 2/4                  │
│         [<] [Vote] [>]              │
│                                     │
├─────────────────────────────────────┤
│     Selected: Sports Car            │
│     [<<] [Ready] [>>]               │  ← Bottom: Controls
│     [Start Game] (Host Only)        │
└─────────────────────────────────────┘
```

---

## 🔧 Troubleshooting

### Issue: Players not spawning on plates
**Solution**: Check that:
- LobbyManager has 4 spawn point transforms assigned
- Spawn points are positioned in world space
- LobbyPlayer prefab has correct components

### Issue: Map voting not working
**Solution**: Check that:
- MapData asset is assigned to LobbyManager
- MapSelectionPanel has button listeners
- Map scene name matches actual scene file

### Issue: Countdown not starting
**Solution**: Check that:
- LobbyCountdown GameObject exists in scene
- All players are marked as ready
- Auto-start is enabled in LobbyCountdown settings

### Issue: Can't switch maps
**Solution**: Check that:
- Player is not marked as ready
- Multiple MapData assets exist for voting
- Map buttons have onClick listeners

---

## 🎮 Testing

1. **Start Host** from MainMenuScene
2. **Join** with another client
3. **Select cars** using arrow buttons
4. **Vote for map** in map selection panel
5. **Click Ready**
6. **Watch countdown** (5 seconds)
7. **Game starts** automatically

---

## 📁 New Files Created

```
Assets/resource/LobbyScene/
├── MapData.cs (ScriptableObject)
├── LobbyCountdown.cs
├── PlayerPlateUI.cs
├── MapSelectionPanel.cs
├── CountdownDisplay.cs
└── LobbyUI.cs (updated)
```

---

## 🚀 Next Steps (Optional Enhancements)

1. **Add sound effects** to CountdownDisplay
2. **Create animations** for PlayerPlateUI
3. **Add more tracks** by creating additional MapData assets
4. **Team mode**: Group players into teams (2v2)
5. **Chat system**: In-lobby text chat
6. **Player stats**: Show win/loss ratio on plates
7. **Customization**: Allow players to customize plate colors

---

**All systems are ready!** The PUBG-style lobby is fully functional with team plates, map voting, and countdown timer. 🎉
