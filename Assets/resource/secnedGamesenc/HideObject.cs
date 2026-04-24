using UnityEngine;

public class HideVisuals : MonoBehaviour
{
    void Start()
    {
        // Get all MeshRenderer components in this object and children
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer r in renderers)
        {
            r.enabled = false; // Hide visual
        }
    }
}