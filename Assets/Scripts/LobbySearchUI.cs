using UnityEngine;
using TMPro;
using TimeCrax.Core;

/// <summary>
/// Componente de busca para filtrar salas no lobby.
/// Filtra por nome da sala ou tema.
/// </summary>
public class LobbySearchUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private GameObject roomListContent;

    private string currentSearchText = "";

    private void Awake()
    {
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    private void OnDestroy()
    {
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(OnSearchChanged);
    }

    private void OnSearchChanged(string searchText)
    {
        currentSearchText = searchText.ToLower().Trim();
        FilterRooms();
    }

    private void FilterRooms()
    {
        if (roomListContent == null) return;

        Room[] rooms = roomListContent.GetComponentsInChildren<Room>(true);

        foreach (Room room in rooms)
        {
            if (room.CompareTag("Undestructable")) continue;

            bool shouldShow = true;

            if (!string.IsNullOrEmpty(currentSearchText))
            {
                // Buscar no nome da sala
                string roomName = room.gameObject.name?.ToLower() ?? "";

                // Buscar no nome do tema
                string themeName = room.GetThemeName()?.ToLower() ?? "";

                // Buscar no texto exibido (NameRoomText)
                string displayName = "";
                foreach (Transform child in room.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "NameRoomText")
                    {
                        var tmp = child.GetComponent<TMP_Text>();
                        if (tmp != null)
                            displayName = tmp.text?.ToLower() ?? "";
                        break;
                    }
                }

                shouldShow = roomName.Contains(currentSearchText) ||
                            themeName.Contains(currentSearchText) ||
                            displayName.Contains(currentSearchText);
            }

            room.gameObject.SetActive(shouldShow);
        }

    }

    /// <summary>
    /// Limpa o campo de busca e mostra todas as salas
    /// </summary>
    public void ClearSearch()
    {
        if (searchInput != null)
            searchInput.text = "";

        currentSearchText = "";
        FilterRooms();
    }

    /// <summary>
    /// Reaplica o filtro atual (útil após refresh da lista)
    /// </summary>
    public void RefreshFilter()
    {
        FilterRooms();
    }
}
