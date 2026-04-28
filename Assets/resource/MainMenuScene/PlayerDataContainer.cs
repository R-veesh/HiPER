using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace resource.MainMenuScene
{
    /// <summary>
    /// Persistent data container that survives scene changes.
    /// Stores player info (car selection, ready state, etc.) when transitioning from lobby to game.
    /// This runs ONLY on server - clients receive spawned cars via NetworkServer.Spawn.
    /// </summary>
    public class PlayerDataContainer : MonoBehaviour
    {
        public static PlayerDataContainer Instance;
        
        /// <summary>
        /// Player game data struct - stored on server only
        /// </summary>
        [System.Serializable]
        public struct PlayerGameData
        {
            public int connectionId;
            public string playerName;
            public int selectedCarIndex;
            public int selectedMapIndex;
            public bool isReady;
            
            public PlayerGameData(int connId, string name, int carIdx, int mapIdx, bool ready)
            {
                connectionId = connId;
                playerName = name;
                selectedCarIndex = carIdx;
                selectedMapIndex = mapIdx;
                isReady = ready;
            }
        }
        
        // Server-side only list - not synced (we spawn cars on server, clients see via NetworkServer.Spawn)
        private List<PlayerGameData> playerDataList = new List<PlayerGameData>();
        
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
        
        /// <summary>
        /// Call this before changing to game scene to save all player data
        /// </summary>
        public void SaveAllPlayerData()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[PlayerDataContainer] SaveAllPlayerData called but not server!");
                return;
            }
            
            playerDataList.Clear();
            
            var lobbyPlayers = FindObjectsOfType<resource.LobbyScene.LobbyPlayer>();
            Debug.Log($"[PlayerDataContainer] Saving data for {lobbyPlayers.Length} players");
            
            foreach (var lobbyPlayer in lobbyPlayers)
            {
                if (lobbyPlayer != null && lobbyPlayer.connectionToClient != null)
                {
                    var data = new PlayerGameData(
                        lobbyPlayer.connectionToClient.connectionId,
                        lobbyPlayer.playerName,
                        lobbyPlayer.selectedCarIndex,
                        lobbyPlayer.selectedMapIndex,
                        lobbyPlayer.isReady
                    );
                    
                    playerDataList.Add(data);
                    Debug.Log($"[PlayerDataContainer] Saved data for player: {data.playerName} (Car: {data.selectedCarIndex})");
                }
            }
        }
        
        /// <summary>
        /// Get player data by connection ID
        /// </summary>
        public PlayerGameData? GetPlayerData(int connectionId)
        {
            foreach (var data in playerDataList)
            {
                if (data.connectionId == connectionId)
                    return data;
            }
            return null;
        }
        
        /// <summary>
        /// Remove player data by connection ID
        /// </summary>
        public void RemovePlayerData(int connectionId)
        {
            if (!NetworkServer.active) return;
            
            for (int i = playerDataList.Count - 1; i >= 0; i--)
            {
                if (playerDataList[i].connectionId == connectionId)
                {
                    Debug.Log($"[PlayerDataContainer] Removed data for connection {connectionId}");
                    playerDataList.RemoveAt(i);
                    return;
                }
            }
        }

        public void UpsertPlayerData(PlayerGameData playerData)
        {
            if (!NetworkServer.active) return;

            for (int i = 0; i < playerDataList.Count; i++)
            {
                if (playerDataList[i].connectionId == playerData.connectionId)
                {
                    playerDataList[i] = playerData;
                    Debug.Log($"[PlayerDataContainer] Updated data for player: {playerData.playerName} (Conn: {playerData.connectionId})");
                    return;
                }
            }

            playerDataList.Add(playerData);
            Debug.Log($"[PlayerDataContainer] Added data for player: {playerData.playerName} (Conn: {playerData.connectionId})");
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
        public void ClearAllData()
        {
            if (!NetworkServer.active) return;
            
            playerDataList.Clear();
            Debug.Log("[PlayerDataContainer] All player data cleared");
        }
    }
}
