using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace resource.LobbyScene
{
    public class PlayerPlateUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerStatusText;
        public TextMeshProUGUI carNameText;
        public Image playerPlateBackground;
        public Image readyIndicator;
        public Image carPreviewImage;
        public GameObject emptySlotOverlay;

        [Header("Visual Settings")]
        public Color readyColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
        public Color notReadyColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Gray
        public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Dark gray
        public Color highlightColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold for local player

        [Header("Animation")]
        public Animator plateAnimator;
        public string readyAnimTrigger = "OnReady";
        public string unreadyAnimTrigger = "OnUnready";
        public string joinAnimTrigger = "OnJoin";
        public string leaveAnimTrigger = "OnLeave";

        private int plateIndex;
        private bool isOccupied = false;

        public void SetPlateIndex(int index)
        {
            plateIndex = index;
        }

        public void SetPlayerInfo(string playerName, string carName, bool isReady, bool isLocalPlayer)
        {
            isOccupied = true;
            
            if (playerNameText != null)
                playerNameText.text = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
            
            if (carNameText != null)
                carNameText.text = string.IsNullOrEmpty(carName) ? "No Car" : carName;

            UpdateReadyState(isReady);

            // Highlight local player
            if (isLocalPlayer && playerPlateBackground != null)
            {
                playerPlateBackground.color = highlightColor;
            }

            // Hide empty slot overlay
            if (emptySlotOverlay != null)
                emptySlotOverlay.SetActive(false);

            // Trigger join animation
            if (plateAnimator != null)
                plateAnimator.SetTrigger(joinAnimTrigger);
        }

        public void UpdateReadyState(bool isReady)
        {
            if (readyIndicator != null)
            {
                readyIndicator.color = isReady ? readyColor : notReadyColor;
            }

            if (playerStatusText != null)
            {
                playerStatusText.text = isReady ? "READY" : "NOT READY";
                playerStatusText.color = isReady ? readyColor : notReadyColor;
            }

            // Trigger animation
            if (plateAnimator != null)
            {
                if (isReady)
                    plateAnimator.SetTrigger(readyAnimTrigger);
                else
                    plateAnimator.SetTrigger(unreadyAnimTrigger);
            }
        }

        public void SetCarPreview(Sprite carSprite)
        {
            if (carPreviewImage != null && carSprite != null)
            {
                carPreviewImage.sprite = carSprite;
                carPreviewImage.gameObject.SetActive(true);
            }
        }

        public void ClearPlayer()
        {
            isOccupied = false;
            
            if (playerNameText != null)
                playerNameText.text = $"Player {plateIndex + 1}";
            
            if (carNameText != null)
                carNameText.text = "Waiting...";

            if (playerStatusText != null)
            {
                playerStatusText.text = "EMPTY";
                playerStatusText.color = emptySlotColor;
            }

            if (readyIndicator != null)
                readyIndicator.color = emptySlotColor;

            if (playerPlateBackground != null)
                playerPlateBackground.color = emptySlotColor;

            if (carPreviewImage != null)
                carPreviewImage.gameObject.SetActive(false);

            // Show empty slot overlay
            if (emptySlotOverlay != null)
                emptySlotOverlay.SetActive(true);

            // Trigger leave animation
            if (plateAnimator != null)
                plateAnimator.SetTrigger(leaveAnimTrigger);
        }

        public bool IsOccupied()
        {
            return isOccupied;
        }

        public void ShowPing(int ping)
        {
            // Optional: Show ping if needed
            // Could add a ping text element
        }
    }
}
