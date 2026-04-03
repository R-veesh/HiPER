using UnityEngine;
using UnityEngine.UI;
using Mirror;
using resource.MainMenuScene;

namespace resource.MainMenuScene
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI Buttons")]
        public Button hostButton;
        public Button joinButton;
        public Button quitButton;

        [Header("Network Settings")]
        public string networkAddress = "127.0.0.1";
        
        [Header("UI - Optional")]
        public InputField addressInputField;
        public Text connectionStatusText;

        private void Start()
        {
            SetupButtonListeners();
            SetupNetworkManager();
            SetupAddressInput();
        }

        void SetupButtonListeners()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        void SetupAddressInput()
        {
            if (addressInputField != null)
            {
                addressInputField.text = networkAddress;
                addressInputField.onEndEdit.AddListener(OnAddressChanged);
            }
            UpdateConnectionStatus("");
        }
        
        void OnAddressChanged(string newAddress)
        {
            if (!string.IsNullOrWhiteSpace(newAddress))
            {
                networkAddress = newAddress.Trim();
                Debug.Log($"[MainMenuManager] Network address changed to: {networkAddress}");
            }
        }
        
        void UpdateConnectionStatus(string status)
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = status;
            }
        }

        void SetupNetworkManager()
        {
            var networkManager = NetworkManager.singleton as CustomNetworkManager;
            if (networkManager != null)
            {
                networkManager.networkAddress = networkAddress;
            }
        }

        void OnHostClicked()
        {
            if (!NetworkServer.active && !NetworkClient.active)
            {
                Debug.Log("Starting Host – will load LobbyScene");
                UpdateConnectionStatus("Starting host...");
                NetworkManager.singleton.StartHost();
            }
            else
            {
                Debug.LogWarning("Already connected as host or client");
                UpdateConnectionStatus("Already connected");
            }
        }

        void OnJoinClicked()
        {
            if (!NetworkClient.active)
            {
                // Use 127.0.0.1 if user enters "localhost" for better compatibility
                string connectAddress = networkAddress;
                if (connectAddress.Equals("localhost", System.StringComparison.OrdinalIgnoreCase))
                {
                    connectAddress = "127.0.0.1";
                    Debug.Log("[MainMenuManager] Converting 'localhost' to '127.0.0.1' for better compatibility");
                }
                
                Debug.Log($"[MainMenuManager] Attempting to join game at {connectAddress}:7777");
                UpdateConnectionStatus($"Connecting to {connectAddress}...");
                
                NetworkManager.singleton.networkAddress = connectAddress;
                NetworkManager.singleton.StartClient();
                
                // Start connection timeout check
                StartCoroutine(ConnectionTimeoutCheck());
            }
            else
            {
                Debug.LogWarning("Already connected as client");
                UpdateConnectionStatus("Already connected");
            }
        }
        
        System.Collections.IEnumerator ConnectionTimeoutCheck()
        {
            float timeout = 10f;
            float elapsed = 0f;
            
            while (elapsed < timeout && NetworkClient.active && !NetworkClient.isConnected)
            {
                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
            
            if (!NetworkClient.isConnected && NetworkClient.active)
            {
                Debug.LogWarning($"[MainMenuManager] Connection attempt timed out after {timeout}s");
                UpdateConnectionStatus("Connection failed - is server running?");
                NetworkManager.singleton.StopClient();
            }
        }

        void OnQuitClicked()
        {
            Debug.Log("Quitting game");
            Application.Quit();
        }
    }
}
