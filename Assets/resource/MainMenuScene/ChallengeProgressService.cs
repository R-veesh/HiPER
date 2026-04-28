using UnityEngine;
using System;

namespace resource.MainMenuScene
{
    public class ChallengeProgressService : MonoBehaviour
    {
        public static ChallengeProgressService Instance { get; private set; }

        const string SaveKey = "challenge_progress_v1";

        [Serializable]
        public class ProgressSaveData
        {
            public int playerLevel = 1;
            public int totalMatches;
            public int matchesWon;
            public int currentChallengeIndex;
            public int selectedOfflineCarIndex;
            public int[] unlockedMapIndices = new[] { 0 };
            public int[] ownedCarIndices = new[] { 0 };
        }

        ProgressSaveData currentData;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static ChallengeProgressService EnsureExists()
        {
            if (Instance != null)
                return Instance;

            GameObject serviceObject = new GameObject("ChallengeProgressService");
            return serviceObject.AddComponent<ChallengeProgressService>();
        }

        public ProgressSaveData GetProgress()
        {
            if (currentData == null)
                LoadProgress();

            return currentData;
        }

        public void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                currentData = JsonUtility.FromJson<ProgressSaveData>(PlayerPrefs.GetString(SaveKey));
            }

            if (currentData == null)
                currentData = new ProgressSaveData();

            if (currentData.unlockedMapIndices == null || currentData.unlockedMapIndices.Length == 0)
                currentData.unlockedMapIndices = new[] { 0 };

            if (currentData.ownedCarIndices == null || currentData.ownedCarIndices.Length == 0)
                currentData.ownedCarIndices = new[] { 0 };

            currentData.playerLevel = Mathf.Max(1, currentData.playerLevel);
            SyncSession();
        }

        public void SaveProgress()
        {
            if (currentData == null)
                return;

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(currentData));
            PlayerPrefs.Save();
            SyncSession();
        }

        public bool IsMapUnlocked(int mapIndex)
        {
            return Contains(currentData.unlockedMapIndices, mapIndex);
        }

        public bool IsCarOwned(int carIndex)
        {
            return Contains(currentData.ownedCarIndices, carIndex);
        }

        public int GetSelectedOfflineCarIndex()
        {
            return currentData.selectedOfflineCarIndex;
        }

        public void SetSelectedOfflineCarIndex(int carIndex)
        {
            currentData.selectedOfflineCarIndex = carIndex;
            SaveProgress();
        }

        public int GetNextOwnedCarIndex(int currentIndex, int totalCars)
        {
            if (totalCars <= 0)
                return 0;

            for (int step = 1; step <= totalCars; step++)
            {
                int candidate = (currentIndex + step) % totalCars;
                if (IsCarOwned(candidate))
                    return candidate;
            }

            return Mathf.Clamp(currentIndex, 0, totalCars - 1);
        }

        public int GetPreviousOwnedCarIndex(int currentIndex, int totalCars)
        {
            if (totalCars <= 0)
                return 0;

            for (int step = 1; step <= totalCars; step++)
            {
                int candidate = currentIndex - step;
                if (candidate < 0)
                    candidate += totalCars;

                if (IsCarOwned(candidate))
                    return candidate;
            }

            return Mathf.Clamp(currentIndex, 0, totalCars - 1);
        }

        public void UnlockCar(int carIndex)
        {
            if (Contains(currentData.ownedCarIndices, carIndex))
                return;

            currentData.ownedCarIndices = AddUnique(currentData.ownedCarIndices, carIndex);
            SaveProgress();
        }

        public string ApplyOfflineRaceResult(int mapIndex, bool won)
        {
            currentData.totalMatches++;

            string resultMessage = won ? "Challenge complete" : "Challenge failed";

            if (won)
            {
                currentData.matchesWon++;
                currentData.playerLevel = Mathf.Max(1, currentData.playerLevel + 1);
                currentData.currentChallengeIndex = Mathf.Max(currentData.currentChallengeIndex, mapIndex + 1);

                int nextMapIndex = mapIndex + 1;
                if (!Contains(currentData.unlockedMapIndices, nextMapIndex))
                {
                    currentData.unlockedMapIndices = AddUnique(currentData.unlockedMapIndices, nextMapIndex);
                    resultMessage = nextMapIndex > mapIndex
                        ? $"Challenge complete - unlocked map {nextMapIndex + 1}"
                        : "Challenge complete";
                }

                if (UserSession.Instance != null)
                    UserSession.Instance.CoinBalance += 100;
            }

            SaveProgress();
            return resultMessage;
        }

        public bool TryGetNextChallengeIndex(int currentMapIndex, int totalMaps, out int nextMapIndex)
        {
            nextMapIndex = currentMapIndex + 1;
            return nextMapIndex >= 0 && nextMapIndex < totalMaps && IsMapUnlocked(nextMapIndex);
        }

        void SyncSession()
        {
            if (UserSession.Instance == null)
                return;

            UserSession.Instance.PlayerLevel = currentData.playerLevel;
            UserSession.Instance.TotalMatches = currentData.totalMatches;
            UserSession.Instance.MatchesWon = currentData.matchesWon;
            UserSession.Instance.SetOwnedCarIndices(currentData.ownedCarIndices);
            UserSession.Instance.PreferredCarIndex = currentData.selectedOfflineCarIndex;
        }

        static bool Contains(int[] values, int value)
        {
            if (values == null)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value)
                    return true;
            }

            return false;
        }

        static int[] AddUnique(int[] values, int value)
        {
            if (Contains(values, value))
                return values;

            int currentLength = values != null ? values.Length : 0;
            int[] next = new int[currentLength + 1];

            for (int i = 0; i < currentLength; i++)
                next[i] = values[i];

            next[currentLength] = value;
            Array.Sort(next);
            return next;
        }
    }
}
