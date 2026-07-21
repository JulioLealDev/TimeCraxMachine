using UnityEngine;
using TMPro;

/// <summary>
/// Enables ZWrite on TMP 3D materials so text is properly occluded by other 3D objects.
/// Attach to any GameObject in the scene (e.g., GameManager or a dedicated setup object).
/// </summary>
public class TMPDepthFix : MonoBehaviour
{
    [SerializeField] private TextMeshPro[] targets;

    private void Awake()
    {
        if (targets == null || targets.Length == 0)
            targets = FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);

        foreach (var tmp in targets)
        {
            if (tmp == null) continue;
            var mat = tmp.fontMaterial;
            mat.SetInt("_ZWrite", 1);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }
    }
}
