using UnityEngine;
using TimeCrax.Themes;

/// <summary>
/// Conteúdo de uma carta de evento (dados do tema legado).
/// Espelha a estrutura de ThemeCard para compatibilidade com o sistema de mini-games.
/// </summary>
public class EventCardContent : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private int year;
    [SerializeField] private string title;
    [SerializeField] private string era;
    [SerializeField] private CardMapData map;
    [SerializeField] private CardPersonsData persons;

    public Material Material => material;
    public int Year => year;
    public string Title => title;
    public string Era => era;
    public CardMapData Map => map;
    public CardPersonsData Persons => persons;
}
