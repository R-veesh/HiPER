using UnityEngine;

namespace resource.LobbyScene
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "Racing/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Map Info")]
        public string mapName = "Default Track";
        public string mapDescription = "A racing track";
        public Sprite mapPreview;
        public string sceneName = "MainGameScene";
        
        [Header("Difficulty")]
        public DifficultyLevel difficulty = DifficultyLevel.Medium;
        public int laps = 3;
        
        [Header("Game Settings")]
        public float raceTimeLimit = 600f; // 10 minutes default
        public bool enableCheckpoints = true;
        
        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }
    }
}
