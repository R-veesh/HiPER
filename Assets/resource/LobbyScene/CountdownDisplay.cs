using UnityEngine;
using TMPro;
using System.Collections;

namespace resource.LobbyScene
{
    public class CountdownDisplay : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI countdownText;
        public GameObject countdownPanel;
        public GameObject goEffect;

        [Header("Visual Settings")]
        public Color normalColor = Color.white;
        public Color warningColor = new Color(1f, 0.5f, 0.2f, 1f); // Orange
        public Color finalColor = new Color(1f, 0.2f, 0.2f, 1f); // Red
        public Color goColor = new Color(0.2f, 0.9f, 0.2f, 1f); // Green

        [Header("Animation Settings")]
        public float pulseScale = 1.2f;
        public float pulseDuration = 0.5f;
        public float shakeIntensity = 10f;
        public AudioClip countdownSound;
        public AudioClip finalSound;
        public AudioClip goSound;

        [Header("Optional: Animator")]
        public Animator countdownAnimator;
        public string countdownTrigger = "Pulse";

        private AudioSource audioSource;
        private int lastDisplayedNumber = -1;
        private Coroutine currentAnimation;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Hide initially
            if (countdownPanel != null)
                countdownPanel.SetActive(false);
            if (goEffect != null)
                goEffect.SetActive(false);
        }

        void Update()
        {
            if (LobbyCountdown.Instance != null && LobbyCountdown.Instance.isCountingDown)
            {
                UpdateCountdownDisplay(LobbyCountdown.Instance.remainingTime);
            }
            else
            {
                HideCountdown();
            }
        }

        void UpdateCountdownDisplay(float time)
        {
            if (countdownPanel != null && !countdownPanel.activeSelf)
                countdownPanel.SetActive(true);

            int displayNumber = Mathf.CeilToInt(time);
            
            if (countdownText != null)
            {
                countdownText.text = displayNumber.ToString();

                // Color changes based on time
                if (time > 3f)
                    countdownText.color = normalColor;
                else if (time > 1f)
                    countdownText.color = warningColor;
                else
                    countdownText.color = finalColor;

                // Scale animation when number changes
                if (displayNumber != lastDisplayedNumber)
                {
                    AnimateNumberChange(displayNumber);
                    lastDisplayedNumber = displayNumber;
                }
            }
        }

        void AnimateNumberChange(int number)
        {
            if (countdownText == null) return;

            // Use Animator if available
            if (countdownAnimator != null)
            {
                countdownAnimator.SetTrigger(countdownTrigger);
            }
            else
            {
                // Stop previous animation
                if (currentAnimation != null)
                    StopCoroutine(currentAnimation);
                
                // Start new animation
                currentAnimation = StartCoroutine(PulseAnimation());
            }

            // Play sound
            PlayCountdownSound(number);
        }

        IEnumerator PulseAnimation()
        {
            if (countdownText == null) yield break;

            Transform textTransform = countdownText.transform;
            Vector3 originalScale = Vector3.one;
            float elapsed = 0f;

            // Scale up
            while (elapsed < pulseDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration * 0.5f);
                float scale = Mathf.Lerp(1f, pulseScale, Mathf.Sin(t * Mathf.PI * 0.5f));
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < pulseDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration * 0.5f);
                float scale = Mathf.Lerp(pulseScale, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            textTransform.localScale = originalScale;
        }

        void PlayCountdownSound(int number)
        {
            if (audioSource == null) return;

            if (number == 0)
            {
                // Show GO effect
                ShowGoEffect();
                if (goSound != null)
                    audioSource.PlayOneShot(goSound);
            }
            else if (number <= 3 && finalSound != null)
            {
                audioSource.PlayOneShot(finalSound);
            }
            else if (countdownSound != null)
            {
                audioSource.PlayOneShot(countdownSound);
            }
        }

        void ShowGoEffect()
        {
            if (goEffect != null)
            {
                goEffect.SetActive(true);
                
                // Animate GO text
                TextMeshProUGUI goText = goEffect.GetComponentInChildren<TextMeshProUGUI>();
                if (goText != null)
                {
                    goText.color = goColor;
                    StartCoroutine(AnimateGoText(goText));
                }
            }

            // Hide countdown
            if (countdownPanel != null)
            {
                countdownPanel.SetActive(false);
            }
        }

        IEnumerator AnimateGoText(TextMeshProUGUI goText)
        {
            Transform textTransform = goText.transform;
            
            // Scale up
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                float scale = Mathf.Lerp(0f, 1.5f, Mathf.Sin(t * Mathf.PI * 0.5f));
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            textTransform.localScale = Vector3.one * 1.5f;

            // Wait
            yield return new WaitForSeconds(0.5f);

            // Scale down
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                float scale = Mathf.Lerp(1.5f, 0f, t);
                textTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            textTransform.localScale = Vector3.zero;

            goEffect.SetActive(false);
        }

        void HideCountdown()
        {
            if (countdownPanel != null && countdownPanel.activeSelf)
            {
                countdownPanel.SetActive(false);
            }
            lastDisplayedNumber = -1;
        }

        public void ShowMessage(string message, float duration = 2f)
        {
            if (countdownText != null)
            {
                countdownText.text = message;
                countdownText.color = normalColor;
                
                if (countdownPanel != null)
                    countdownPanel.SetActive(true);

                CancelInvoke(nameof(HideCountdown));
                Invoke(nameof(HideCountdown), duration);
            }
        }
    }
}
