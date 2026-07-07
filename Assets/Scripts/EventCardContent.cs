using UnityEngine;

/// <summary>
/// Conteúdo de uma carta de evento (dados do tema legado)
/// </summary>
public class EventCardContent : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private int year;

    public Material Material => material;
    public int Year => year;
}
