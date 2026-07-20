using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;

public class LegacyThemesData : MonoBehaviourPunCallbacks
{
    [Header("Legacy Theme Materials")]
    public GameObject discoveryOfAmericasMaterials;

    [Header("Timeline")]
    public GameObject timeline;

    // Legacy system
    public int randomNumber = -1;
    private int[] randomNumberList = new int[6] { -1, -1, -1, -1, -1, -1 };
    private int[] selectedYears = new int[6];
    private EventCard[] eventCards;
    private EventCardContent[] legacyMaterialList;

    [Header("Timeline Years")]
    [SerializeField] private TextMeshPro[] timelineYearsList = new TextMeshPro[6];
    private int eventCardsLength = 6;
    private int count = 0;

    // New Theme System
    private ThemeData currentTheme;
    private List<ThemeCard> selectedCards; // 6 cartas selecionadas aleatoriamente

    [Header("Theme Card Material")]
    [SerializeField] private Material eventCardBaseMaterial; // Material base com shader EventCardComposite
    [SerializeField] private Texture2D cardTemplateTexture; // Textura do template da carta

    private void LegacyThemesDataIdList(int materialListLenght)
    {
        do
        {
            randomNumber = UnityEngine.Random.Range(0, materialListLenght);
        }
        while (randomNumberList.Contains(randomNumber));

        randomNumberList[count] = randomNumber;
        count++;
    }

    public void GetLegacyThemesData(string theme)
    {

        int length = 0;

        switch (theme.ToUpper())
        {
            case "DISCOVERY OF THE AMERICAS":
                if (discoveryOfAmericasMaterials == null)
                {
                    Debug.LogError("[LegacyThemesData] 'discoveryOfAmericasMaterials' não está atribuído no Inspector.");
                    return;
                }
                length = discoveryOfAmericasMaterials.GetComponentsInChildren<EventCardContent>().Length;
                break;
        }

        if (length == 0)
        {
            Debug.LogError($"[LegacyThemesData] Tema legado não reconhecido ou sem cartas: '{theme}'. Partida não pode iniciar.");
            return;
        }

        count = 0;
        for (int i = 0; i < randomNumberList.Length; i++) randomNumberList[i] = -1;
        while (count < 6)
        {
            LegacyThemesDataIdList(length);
        }

        photonView.RPC("SetAllValues", RpcTarget.All, theme, randomNumberList);

    }

    [PunRPC]
    public void SetAllValues(string theme, int[] randomNumberList)
    {
        eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        if (theme.ToUpper() == "DISCOVERY OF THE AMERICAS")
        {
            legacyMaterialList = discoveryOfAmericasMaterials.GetComponentsInChildren<EventCardContent>();

            for (int i = 0; i < eventCards.Length; i++)
                SetMaterialsToEventCards(i, legacyMaterialList, randomNumberList);

            Array.Sort(selectedYears);
            SetSlotCounts();
            SetTimelineYears();
        }
    }

    public void SetMaterialsToEventCards(int i, EventCardContent[] materialList, int[] randomNumberList)
    {
        // Verificações de bounds
        if (eventCards == null || i >= eventCards.Length || eventCards[i] == null) return;
        if (materialList == null || randomNumberList == null) return;
        if (i >= randomNumberList.Length || randomNumberList[i] >= materialList.Length) return;
        if (i >= selectedYears.Length) return;

        var content = materialList[randomNumberList[i]];
        if (content == null) return;

        eventCards[i].GetComponent<Renderer>().material = content.Material;
        eventCards[i].slotYear = content.Year;
        selectedYears[i] = content.Year;

        // Alimentar o sistema de mini-games com os dados da carta legada
        var themeCard = new TimeCrax.Themes.ThemeCard
        {
            year    = content.Year,
            title   = content.Title,
            era     = content.Era,
            map     = content.Map,
            persons = content.Persons,
        };
        eventCards[i].SetThemeCard(themeCard);
    }

    public void SetSlotCounts()
    {
        if (eventCards == null) return;

        for (int i = 0; i < eventCardsLength && i < eventCards.Length; i++)
        {
            if (eventCards[i] == null) continue;

            for (int y = 0; y < selectedYears.Length; y++)
            {
                if (eventCards[i].slotYear == selectedYears[y])
                {
                    eventCards[i].slotCount = y + 1;
                    break;
                }
            }
        }
    }

    public void SetTimelineYears()
    {
        if (timelineYearsList == null || timelineYearsList.Length == 0)
        {
            return;
        }


        for (int i = 0; i < selectedYears.Length && i < timelineYearsList.Length; i++)
        {
            if (timelineYearsList[i] == null)
            {
                continue;
            }

            timelineYearsList[i].text = selectedYears[i].ToString();

        }
    }

    public bool IsLegacyThemeReady(string themeName)
    {
        return themeName.ToUpper() == "DISCOVERY OF THE AMERICAS" && discoveryOfAmericasMaterials != null;
    }

    #region New Theme System

    /// <summary>
    /// Inicializa o sistema para um tema da API
    /// </summary>
    public void InitializeForTheme(ThemeData theme)
    {
        currentTheme = theme;
    }

    /// <summary>
    /// Seleciona 7 cartas aleatórias do tema e configura o jogo (chamado pelo Master Client)
    /// </summary>
    public void GetLegacyThemesDataFromTheme()
    {
        if (currentTheme == null)
        {
            return;
        }

        // Selecionar 7 cartas aleatórias do tema
        selectedCards = SelectRandomCards(currentTheme.cards, 7);

        // Criar array de índices das cartas selecionadas para sincronizar via RPC
        int[] selectedIndices = new int[selectedCards.Count];
        for (int i = 0; i < selectedCards.Count; i++)
        {
            selectedIndices[i] = currentTheme.cards.IndexOf(selectedCards[i]);
        }


        // Sincronizar com todos os jogadores
        photonView.RPC("SetAllValuesFromTheme", RpcTarget.All, currentTheme.id, selectedIndices);
    }

    /// <summary>
    /// Seleciona N cartas aleatórias de uma lista
    /// </summary>
    private List<ThemeCard> SelectRandomCards(List<ThemeCard> allCards, int count)
    {
        if (allCards.Count <= count)
        {
            // Se o tema tem 6 ou menos cartas, usar todas
            return new List<ThemeCard>(allCards);
        }

        // Embaralhar e pegar as primeiras N cartas
        return allCards.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
    }

    [PunRPC]
    public void SetAllValuesFromTheme(string themeId, int[] selectedIndices)
    {
        // Carregar tema do storage local
        var theme = ThemeStorage.GetTheme(themeId);
        if (theme == null)
        {
            return;
        }

        currentTheme = theme;

        // Reconstruir lista de cartas selecionadas a partir dos índices
        selectedCards = new List<ThemeCard>();
        for (int i = 0; i < selectedIndices.Length; i++)
        {
            if (selectedIndices[i] >= 0 && selectedIndices[i] < theme.cards.Count)
            {
                selectedCards.Add(theme.cards[selectedIndices[i]]);
            }
        }

        // Ordenar cartas por ano para definir slots
        selectedCards = selectedCards.OrderBy(c => c.year).ToList();

        // Preencher selectedYears
        for (int i = 0; i < selectedCards.Count && i < selectedYears.Length; i++)
        {
            selectedYears[i] = selectedCards[i].year;
        }

        // Buscar EventCards na cena
        eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        // Aplicar materiais e dados às cartas
        for (int i = 0; i < eventCards.Length && i < selectedCards.Count; i++)
        {
            SetMaterialsFromTheme(i);
        }

        // Definir slotCounts baseado na ordem dos anos
        SetSlotCountsFromTheme();

        // Atualizar timeline
        SetTimelineYears();

    }

    /// <summary>
    /// Aplica textura e dados de uma carta do tema a um EventCard
    /// </summary>
    private void SetMaterialsFromTheme(int index)
    {
        var themeCard = selectedCards[index];
        var eventCard = eventCards[index];

        // Carregar textura local
        var texture = ThemeStorage.LoadLocalImage(themeCard.localImagePath);
        if (texture != null)
        {
            var renderer = eventCard.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Verificar se temos o material base configurado
                if (eventCardBaseMaterial != null)
                {
                    // Usar o shader de composição
                    var material = new Material(eventCardBaseMaterial);

                    // Definir o template (frame da carta)
                    if (cardTemplateTexture != null)
                    {
                        material.SetTexture("_MainTex", cardTemplateTexture);
                    }

                    // Definir a imagem do tema
                    material.SetTexture("_ImageTex", texture);

                    renderer.material = material;
                }
                else
                {
                    // Fallback: usar material padrão (imagem ocupa toda a carta)
                    var material = new Material(Shader.Find("Standard"));
                    material.mainTexture = texture;
                    material.SetFloat("_Glossiness", 0.2f);
                    renderer.material = material;
                }
            }
        }
        else
        {
        }

        // Definir ano
        eventCard.slotYear = themeCard.year;

        eventCard.SetThemeCard(themeCard);

    }

    /// <summary>
    /// Define slotCount para cada EventCard baseado na ordem cronológica
    /// </summary>
    private void SetSlotCountsFromTheme()
    {
        if (eventCards == null || selectedCards == null) return;

        // selectedCards já está ordenado por ano
        for (int i = 0; i < eventCards.Length; i++)
        {
            if (eventCards[i] == null) continue;

            for (int y = 0; y < selectedCards.Count; y++)
            {
                if (selectedCards[y] != null && eventCards[i].slotYear == selectedCards[y].year)
                {
                    eventCards[i].slotCount = y + 1;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Retorna as cartas selecionadas para a partida atual
    /// </summary>
    public List<ThemeCard> GetSelectedCards()
    {
        return selectedCards;
    }

    /// <summary>
    /// Retorna o tema atual
    /// </summary>
    public ThemeData GetCurrentTheme()
    {
        return currentTheme;
    }

    #endregion
}
