using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HoverMaterialAdder : MonoBehaviour
{
    [SerializeField] private Material hoverMaterial;

    private Renderer targetRenderer;
    private Material[] originalMaterials;
    private bool hoverActive = false;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
            originalMaterials = targetRenderer.materials;
    }

    /// <summary>
    /// Define o material de hover em runtime (útil quando o componente é adicionado via AddComponent).
    /// Também recaptura os materiais originais do renderer no momento da chamada.
    /// </summary>
    public void SetMaterial(Material mat)
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (targetRenderer == null) return;

        HideHover();
        hoverMaterial = mat;
        originalMaterials = targetRenderer.materials;
    }

    private void OnMouseEnter()
    {
        ShowHover();
    }

    private void OnMouseExit()
    {
        HideHover();
    }

    public void ShowHover()
    {
        if (hoverActive || hoverMaterial == null || targetRenderer == null) return;

        var mats = new Material[originalMaterials.Length];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = hoverMaterial;
        targetRenderer.materials = mats;
        hoverActive = true;
    }

    public void HideHover()
    {
        if (!hoverActive || targetRenderer == null) return;

        targetRenderer.materials = originalMaterials;
        hoverActive = false;
    }
}
