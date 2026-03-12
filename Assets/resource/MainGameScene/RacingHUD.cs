using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main gameplay HUD overlay (Asphalt-style).
/// Attach to a Screen Space - Overlay Canvas in the game scene.
///
/// Hierarchy (create in Inspector):
///   RacingHUD_Canvas  (Screen Space - Overlay, Sort Order 1)
///   ├── TopBar  (Panel, anchored top-stretch, height ~80px)
///   │   ├── PositionText    (TMP)  "1/6 POS"   ← top-left
///   │   ├── SpeedText       (TMP)  "180"        ← top-center
///   │   ├── SpeedUnitText   (TMP)  "KM/H"       ← below speed
///   │   ├── LapText         (TMP)  "2/3 LAP"   ← top-right
///   │   └── PauseButton     (Button) ← top-right corner
///   ├── SpeedBarBG  (Image, spped bar)
///   │   └── SpeedBarFill  (Image, type=Filled, Radial180)
///   └── GearText (TMP) "3"  ← near speed bar
/// </summary>
public class RacingHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI positionText;   // e.g. "1/6 POS"
    public TextMeshProUGUI speedText;      // e.g. "180"
    public TextMeshProUGUI speedUnitText;  // e.g. "KM/H"
    public TextMeshProUGUI lapText;        // e.g. "2/3 LAP"
    public TextMeshProUGUI gearText;       // e.g. "3"
    public Button pauseButton;

    [Header("Speed Bar")]
    public Image speedBarFill;             // Filled image (Radial360 or Horizontal)
    public Gradient speedBarGradient;      // Color transitions: green → yellow → red

    [Header("Pause Menu")]
    public GameObject pausePanel;          // optional overlay panel to show when paused

    [Header("Settings")]
    public bool showGear = true;

    private PrometeoCarController localPrometeo;
    private CarController localCarController;
    private Rigidbody localCarRb;
    private CarPlayer localCarPlayer;
    private bool initialized;
    private float searchTimer;
    private bool isPaused;

    void Awake()
    {
        // BUG FIX #1: Validate Inspector references on startup so the user sees
        // clear error messages instead of silently blank HUD
        if (positionText == null) Debug.LogError("[RacingHUD] positionText is NOT assigned in Inspector!");
        if (speedText == null) Debug.LogError("[RacingHUD] speedText is NOT assigned in Inspector!");
        if (lapText == null) Debug.LogError("[RacingHUD] lapText is NOT assigned in Inspector!");

        // BUG FIX #2: Initialize Gradient with a default if user didn't set one.
        // An uninitialized Gradient is null and would skip speed bar coloring entirely.
        if (speedBarGradient == null)
        {
            speedBarGradient = new Gradient();
            speedBarGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.green, 0f),
                    new GradientColorKey(Color.yellow, 0.6f),
                    new GradientColorKey(Color.red, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }

        // BUG FIX #3: Show default placeholder text so HUD is visible immediately
        // and user knows the elements are working even before the car spawns
        if (speedText != null) speedText.text = "0";
        if (speedUnitText != null) speedUnitText.text = "KM/H";
        if (positionText != null) positionText.text = "-/- POS";
        if (lapText != null) lapText.text = "-/- LAP";
        if (gearText != null) gearText.text = "1";
    }

    void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (!initialized)
        {
            // BUG FIX #4: Don't search every single frame — it's wasteful.
            // Search every 0.5s and log warnings so user knows what's happening.
            searchTimer -= Time.unscaledDeltaTime;
            if (searchTimer <= 0f)
            {
                searchTimer = 0.5f;
                TryFindLocalCar();
            }
            return;
        }

        if (localPrometeo == null && localCarController == null)
        {
            Debug.LogWarning("[RacingHUD] Lost reference to local car controller — re-searching...");
            initialized = false;
            return;
        }

        UpdateSpeed();
        UpdateSpeedBar();
        UpdateGear();
        UpdatePosition();
        UpdateLap();
    }

    void TryFindLocalCar()
    {
        // BUG FIX #5: Use FindObjectsByType with FindObjectsInactive.Exclude
        // (explicit enum) and add debug logging when search fails.
        // Also search ALL matching cars — don't bail on the first non-local one.
#if UNITY_6000_0_OR_NEWER
        CarPlayer[] allCars = FindObjectsByType<CarPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        CarPlayer[] allCars = FindObjectsOfType<CarPlayer>();
#endif

        if (allCars.Length == 0)
        {
            Debug.Log("[RacingHUD] No CarPlayer objects found in scene yet — waiting for spawn...");
            return;
        }

        foreach (var cp in allCars)
        {
            if (cp.isOwned || cp.isLocalPlayer)
            {
                localCarPlayer = cp;
                localPrometeo = cp.GetComponent<PrometeoCarController>();
                localCarController = cp.GetComponent<CarController>();
                localCarRb = cp.GetComponent<Rigidbody>();

                if (localPrometeo == null && localCarController == null)
                {
                    Debug.LogError($"[RacingHUD] Found local CarPlayer '{cp.gameObject.name}' but it has NO car controller (PrometeoCarController or CarController)!");
                    return;
                }

                initialized = true;
                string controllerType = localPrometeo != null ? "PrometeoCarController" : "CarController";
                Debug.Log($"[RacingHUD] Local car found: {cp.gameObject.name} using {controllerType} — HUD active");
                return;
            }
        }

        Debug.Log($"[RacingHUD] Found {allCars.Length} CarPlayer(s) but none are owned by local player — waiting...");
    }

    float GetCurrentSpeed()
    {
        if (localPrometeo != null)
            return Mathf.Abs(localPrometeo.carSpeed);
        if (localCarRb != null)
            return localCarRb.linearVelocity.magnitude * 3.6f;
        return 0f;
    }

    float GetMaxSpeed()
    {
        if (localPrometeo != null)
            return localPrometeo.maxSpeed;
        if (localCarController != null)
            return localCarController.maxSpeed;
        return 180f;
    }

    void UpdateSpeed()
    {
        if (speedText == null) return;
        speedText.text = Mathf.RoundToInt(GetCurrentSpeed()).ToString();
    }

    void UpdateSpeedBar()
    {
        if (speedBarFill == null) return;

        // Ensure image is configured correctly for filled mode
        if (speedBarFill.type != Image.Type.Filled)
        {
            speedBarFill.type = Image.Type.Filled;
            speedBarFill.fillMethod = Image.FillMethod.Radial180;
            speedBarFill.fillOrigin = (int)Image.Origin180.Bottom;
            speedBarFill.fillClockwise = true;
        }

        float maxSpd = GetMaxSpeed();
        if (maxSpd <= 0f) maxSpd = 180f;

        float currentSpeed = GetCurrentSpeed();
        float targetFill = Mathf.Clamp01(currentSpeed / maxSpd);

        // Smooth the bar so it doesn't jitter
        speedBarFill.fillAmount = Mathf.Lerp(speedBarFill.fillAmount, targetFill, Time.deltaTime * 8f);

        if (speedBarGradient != null)
            speedBarFill.color = speedBarGradient.Evaluate(targetFill);
    }

    void UpdateGear()
    {
        if (!showGear || gearText == null) return;
        // PrometeoCarController has no gear system — hide gear text
        if (localPrometeo != null)
        {
            gearText.gameObject.SetActive(false);
            return;
        }
        if (localCarController != null)
            gearText.text = localCarController.currentGear.ToString();
    }

    void UpdatePosition()
    {
        if (positionText == null || localCarPlayer == null) return;

        int pos = localCarPlayer.racePosition;
        int total = localCarPlayer.totalRacers;
        if (pos <= 0) pos = 1;
        if (total <= 0) total = 1;

        positionText.text = $"{pos}/{total} POS";
    }

    void UpdateLap()
    {
        if (lapText == null || localCarPlayer == null) return;

        int currentLap = localCarPlayer.syncedLap + 1; // display as 1-based
        int totalLaps = localCarPlayer.syncedTotalLaps;
        if (totalLaps <= 0) totalLaps = 1;

        // Clamp so it doesn't show lap 4/3 after finishing
        currentLap = Mathf.Min(currentLap, totalLaps);

        lapText.text = $"{currentLap}/{totalLaps} LAP";
    }

    // BUG FIX #6: Pause button was setting Time.timeScale = 0 which BREAKS
    // Mirror networking (freezes coroutines, physics, FixedUpdate, network ticks).
    // In multiplayer, pause should show an overlay menu, not freeze time.
    void OnPauseClicked()
    {
        Debug.Log("[RacingHUD] Pause button clicked");
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // Lock/unlock cursor for menu interaction
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
}
