using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace resource.MainMenuScene
{
    /// <summary>
    /// Persistent data container that survives scene changes.
    /// Stores player info (car selection, ready state, etc.) when transitioning from lobby to game.
    /// </summary>
    public class PlayerDataContainer : NetworkBehaviour
    {
        public static PlayerDataContainer Instance;
        
        [System.Serializable]
        public class PlayerGameData
        {
            public int connectionId;
            public string playerName;
            public int selectedCarIndex;
            public int selectedMapIndex;
            public bool isReady;
        }
        
        // SyncList to store all players' data across network
        private readonly SyncList<PlayerGameData> playerDataList = new SyncList<PlayerGameData>();
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[PlayerDataContainer] Instance created and set to DontDestroyOnLoad");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            playerDataList.Callback += OnPlayerDataChanged;
        }
        
        void OnPlayerDataChanged(SyncList<PlayerGameData>.Operation op, int itemIndex, PlayerGameData oldItem, PlayerGameData newItem)
        {
            Debug.Log($"[PlayerDataContainer] Player data list changed: {op} at index {itemIndex}");
        }
        
        /// <summary>
        /// Call this before changing to game scene to save all player data
        /// </summary>
        [Server]
        public void SaveAllPlayerData()
        {
            playerDataList.Clear();
            
            var lobbyPlayers = FindObjectsOfType<resource.LobbyScene.LobbyPlayer>();
            Debug.Log($"[PlayerDataContainer] Saving data for {lobbyPlayers.Length} players");
            
            foreach (var lobbyPlayer in lobbyPlayers)
            {
                if (lobbyPlayer != null && lobbyPlayer.connectionToClient != null)
                {
                    var data = new PlayerGameData
                    {
                        connectionId = lobbyPlayer.connectionToClient.connectionId,
                        playerName = lobbyPlayer.playerName,
                        selectedCarIndex = lobbyPlayer.selectedCarIndex,
                        selectedMapIndex = lobbyPlayer.selectedMapIndex,
                        isReady = lobbyPlayer.isReady
                    };
                    
                    playerDataList.Add(data);
                    Debug.Log($"[PlayerDataContainer] Saved data for player: {data.playerName} (Car: {data.selectedCarIndex})");
                }
            }
        }
        
        /// <summary>
        /// Get player data by connection ID
        /// </summary>
        public PlayerGameData GetPlayerData(int connectionId)
        {
            foreach (var data in playerDataList)
            {
                if (data.connectionId == connectionId)
                    return data;
            }
            return null;
        }
        
        /// <summary>
        /// Get all player data
        /// </summary>
        public List<PlayerGameData> GetAllPlayerData()
        {
            return new List<PlayerGameData>(playerDataList);
        }
        
        /// <summary>
        /// Clear all stored data (call when returning to lobby)
        /// </summary>
        [Server]
        public void ClearAllData()
        {
            playerDataList.Clear();
            Debug.Log("[PlayerDataContainer] All player data cleared");
        }
    }
}
