using UnityEngine;

namespace resource.MainMenuScene
{
    public class OfflineRaceConfig : MonoBehaviour
    {
        public static OfflineRaceConfig Instance { get; private set; }

        public bool IsOfflineMode { get; private set; }
        public bool HasAppliedResult { get; private set; }
        public int SelectedCarIndex { get; private set; }
        public int SelectedMapIndex { get; private set; }
        public string SelectedMapName { get; private set; }
        public string SelectedSceneName { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static OfflineRaceConfig EnsureExists()
        {
            if (Instance != null)
                return Instance;

            GameObject configObject = new GameObject("OfflineRaceConfig");
            return configObject.AddComponent<OfflineRaceConfig>();
        }

        public void Configure(int carIndex, int mapIndex, string mapName, string sceneName)
        {
            IsOfflineMode = true;
            HasAppliedResult = false;
            SelectedCarIndex = carIndex;
            SelectedMapIndex = mapIndex;
            SelectedMapName = mapName;
            SelectedSceneName = sceneName;
        }

        public void MarkResultApplied()
        {
            HasAppliedResult = true;
        }

        public void Clear()
        {
            IsOfflineMode = false;
            HasAppliedResult = false;
            SelectedCarIndex = 0;
            SelectedMapIndex = 0;
            SelectedMapName = null;
            SelectedSceneName = null;
        }
    }
}
