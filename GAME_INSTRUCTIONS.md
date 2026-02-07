# HYPER Racing Game - Step by Step Instructions

## Overview
This document provides step-by-step instructions for understanding and working with the HYPER racing game's MainGameScene and MainMenuScene components.

## MainMenuScene

### 1. MainMenuManager.cs
**Location**: `Assets/resource/MainMenuScene/MainMenuManager.cs`

#### Purpose
Manages the main menu UI and network connections for hosting/joining races.

#### Key Components
- **UI Panels**: Main menu panel and lobby panel
- **Buttons**: Host, Join, and Quit buttons
- **Network Manager**: CustomNetworkManager for handling multiplayer

#### Step-by-Step Flow
1. **Initialization** (`Start()` method)
   - Sets up UI panels (main menu visible, lobby hidden)
   - Configures button click listeners
   - Sets offline mode to ensure clean network state

2. **Button Actions**
   - **Host Button**: Starts a host server and switches to lobby
   - **Join Button**: Starts a client and switches to lobby
   - **Quit Button**: Closes the application

3. **Panel Management**
   - `SwitchToLobby()`: Hides main menu, shows lobby
   - `ReturnToMainMenu()`: Returns to main menu from lobby

## MainGameScene

### 1. GameSceneManager.cs
**Location**: `Assets/resource/MainGameScene/Scripts/GameSceneManager.cs`

#### Purpose
Singleton manager that controls the overall race state and flow.

#### Key Features
- **Race States**: Waiting → Countdown → Racing → Finished
- **Network Sync**: Synchronizes race state across all clients
- **Server Authority**: Server controls race timing and state transitions

#### Step-by-Step Race Flow
1. **Race Initialization** (`InitializeRace()`)
   - Sets race state to "Waiting"
   - Sets up RaceManager and CheckpointManager
   - Notifies all clients race is ready

2. **Countdown Phase** (`StartRaceCountdown()`)
   - Transitions to "Countdown" state
   - Starts countdown coroutine (3 seconds default)
   - Updates countdown display on all clients

3. **Race Start** (`StartRace()`)
   - Transitions to "Racing" state
   - Records race start time
   - Enables player controls on all clients
   - Notifies RaceManager to begin race

4. **Race Finish** (`FinishRace()`)
   - Transitions to "Finished" state
   - Announces winner to all clients
   - Returns to lobby after 5 seconds

### 2. RaceManager.cs
**Location**: `Assets/resource/MainGameScene/Scripts/RaceManager.cs`

#### Purpose
Manages player positions, lap counting, and race progress tracking.

#### Key Features
- **Player Tracking**: Maintains race data for each player
- **Position Management**: Assigns starting positions and tracks current positions
- **Lap System**: Handles lap completion and validation

#### Step-by-Step Operations
1. **Race Setup** (`SetupRace()`)
   - Counts connected players
   - Initializes player race data
   - Positions players at starting grid

2. **Player Positioning** (`PositionPlayersAtStart()`)
   - Places each player at assigned starting position
   - Sets player rotation to match start direction
   - Disables player control until race starts

3. **Checkpoint Tracking** (`UpdatePlayerCheckpoint()`)
   - Validates checkpoint progression (must pass in order)
   - Updates player's last checkpoint
   - Detects lap completion when checkpoint 0 is passed

4. **Lap Completion** (`OnPlayerLapCompleted()`)
   - Increments player's lap count
   - Records lap time and best lap time
   - Checks for race completion

5. **Race Completion** (`OnPlayerFinished()`)
   - Marks player as finished
   - Records total race time
   - Assigns finishing position
   - Triggers race end for first finisher

### 3. RaceUI.cs
**Location**: `Assets/resource/MainGameScene/Scripts/RaceUI.cs`

#### Purpose
Manages all user interface elements during the race.

#### Key UI Components
- **Countdown Panel**: Shows race countdown
- **Race Panel**: Displays race information (laps, time, position)
- **Results Panel**: Shows race results and winner
- **Player List**: Real-time player status during race

#### Step-by-Step UI Flow
1. **Waiting State** (`ShowWaitingMessage()`)
   - Shows "Waiting for players" message
   - Hides all other panels

2. **Countdown State** (`UpdateCountdown()`)
   - Displays countdown numbers (3, 2, 1)
   - Shows "GO!" when countdown reaches 0

3. **Racing State** (`ShowRaceStarted()`)
   - Activates race information panel
   - Initializes player list
   - Starts updating race time and lap info

4. **Results State** (`ShowWinner()`)
   - Displays winner announcement
   - Shows complete race results
   - Lists all players by finishing position

## Supporting Components

### Checkpoint System
- **Checkpoint.cs**: Individual checkpoint trigger
- **CheckpointManager.cs**: Manages all checkpoints and validation

### Finish Line
- **FinishLine.cs**: Detects when players cross the finish line

## Network Architecture

### Mirror Integration
- All managers use Mirror's NetworkBehaviour
- Server-authoritative race state management
- Client RPC calls for UI updates
- SyncVar for synchronized data

### Data Flow
1. **Server** controls race logic and state
2. **Clients** send input and receive state updates
3. **UI** updates based on synchronized race data

## Setup Instructions

### For MainMenuScene
1. Create MainMenuManager GameObject
2. Assign UI panels and buttons in inspector
3. Link CustomNetworkManager reference
4. Configure button onClick events

### For MainGameScene
1. Create GameSceneManager GameObject (singleton)
2. Create RaceManager GameObject
3. Create RaceUI GameObject with all UI components
4. Set up race start positions array
5. Configure checkpoints and finish line
6. Link all manager references in inspector

## Testing Checklist

### MainMenuScene
- [ ] Host button starts server and shows lobby
- [ ] Join button connects to server and shows lobby
- [ ] Quit button closes application
- [ ] Return to menu works correctly

### MainGameScene
- [ ] Race initializes with all players
- [ ] Countdown displays correctly
- [ ] Race starts at "GO!"
- [ ] Player controls enable at race start
- [ ] Checkpoint progression works
- [ ] Lap counting functions properly
- [ ] Race ends when winner crosses finish line
- [ ] Results display correctly
- [ ] Return to lobby after race completion

## Common Issues & Solutions

### Network Issues
- Ensure CustomNetworkManager is properly configured
- Check that all NetworkIdentity components are set up
- Verify server/client connection establishment

### UI Issues
- Make sure all UI references are assigned in inspector
- Check that TextMeshPro components are properly configured
- Verify panel activation/deactivation logic

### Race Logic Issues
- Ensure race start positions array matches max players
- Check checkpoint ordering and validation
- Verify lap counting logic and finish conditions