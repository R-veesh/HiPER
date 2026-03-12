using UnityEngine;
using TMPro;

/// <summary>
/// Floating name label above opponent vehicles.
/// Uses a world-space TextMeshPro object that billboards toward the camera.
///
/// Usage:
///   1. Create an empty child "NameLabel" on each car prefab at (0, 2.5, 0).
///   2. Add a TextMeshPro - Text (3D) component to it.
///   3. Attach this script to the same GameObject.
///   4. The script auto-hides on the local player's car.
///
/// Alternatively, call FloatingNameLabel.CreateForCar(carTransform, name) at runtime.
/// </summary>
public class FloatingNameLabel : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro nameText;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0f, 2.5f, 0f);
    public float maxVisibleDistance = 80f;
    public float fadeStartDistance = 60f;

    private Transform target;       // the car transform to follow
    private Transform cam;
    private bool isHidden;

    /// <summary>
    /// Initialize the label. Call after instantiation.
    /// </summary>
    public void Initialize(Transform carTransform, string playerName, bool isLocalPlayer)
    {
        target = carTransform;

        if (nameText != null)
        {
            nameText.text = playerName;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = 4f;
        }

        // Hide label above the local player's own car
        if (isLocalPlayer)
        {
            isHidden = true;
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        if (nameText == null)
            nameText = GetComponent<TextMeshPro>();
    }

    void LateUpdate()
    {
        if (isHidden) return;

        // Find camera if not cached
        if (cam == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null) cam = mainCam.transform;
            else return;
        }

        // Follow the car
        if (target != null)
            transform.position = target.position + offset;

        // Billboard: rotate to always face the camera
        Vector3 dirToCamera = cam.position - transform.position;
        // Only rotate on Y axis to keep text upright, or full billboard:
        transform.rotation = Quaternion.LookRotation(-dirToCamera, Vector3.up);

        // Distance-based fade
        float dist = dirToCamera.magnitude;
        if (dist > maxVisibleDistance)
        {
            SetAlpha(0f);
        }
        else if (dist > fadeStartDistance)
        {
            float t = 1f - (dist - fadeStartDistance) / (maxVisibleDistance - fadeStartDistance);
            SetAlpha(t);
        }
        else
        {
            SetAlpha(1f);
        }
    }

    void SetAlpha(float alpha)
    {
        if (nameText == null) return;
        Color c = nameText.color;
        c.a = alpha;
        nameText.color = c;
    }

    /// <summary>
    /// Factory: create a floating name label at runtime for a car.
    /// Call this from CarPlayer after spawn.
    /// </summary>
    public static FloatingNameLabel CreateForCar(Transform carTransform, string playerName, bool isLocalPlayer)
    {
        GameObject labelObj = new GameObject($"NameLabel_{playerName}");

        // Add TextMeshPro 3D component
        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = playerName;
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        tmp.rectTransform.sizeDelta = new Vector2(5f, 1f);

        // Add this component
        FloatingNameLabel label = labelObj.AddComponent<FloatingNameLabel>();
        label.nameText = tmp;
        label.Initialize(carTransform, playerName, isLocalPlayer);

        return label;
    }
}
