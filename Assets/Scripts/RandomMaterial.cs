using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Themes;

public class RandomMaterial : MonoBehaviourPunCallbacks
{
    [Header("Legacy Theme Materials")]
    public GameObject worldHistoryMaterials;
    public GameObject wordWar2Materials;
    public GameObject worldScienceMaterials;

    [Header("Timeline")]
    public GameObject timeline;

    // Legacy system
    public int randomNumber = -1;
    private int[] randomNumberList = new int[7] { -1, -1, -1, -1, -1, -1, -1 };
    private int[] selectedYears = new int[7];
    private EventCard[] eventCards;
    private EventCardContent[] wwMaterialList;
    private EventCardContent[] whMaterialList;
    private TextMeshPro[] timelineTearsList;
    private int eventCardsLength = 7;
    private int count = 0;

    // New Theme System
    private ThemeData currentTheme;
    private List<ThemeCard> selectedCards; // 7 cartas selecionadas aleatoriamente

    [Header("Theme Card Material")]
    [SerializeField] private Material eventCardBaseMaterial; // Material base com shader EventCardComposite
    [SerializeField] private Texture2D cardTemplateTexture; // Textura do template da carta

    private void RandomMaterialIdList(int materialListLenght)
    {

        randomNumber = UnityEngine.Random.Range(0, materialListLenght);

        if (randomNumberList.Contains(randomNumber))
        {
            RandomMaterialIdList(materialListLenght);
        }
        else
        {
            randomNumberList[count] = randomNumber;
            count++;
        }

            
    }

    public void GetRandomMaterial(string theme)
    {

        int length = 0;

        switch (theme.ToUpper())
        {

            case "WORLD HISTORY":

                length = worldHistoryMaterials.GetComponentsInChildren<EventCardContent>().Length;

                break;

            case "WORLD WAR 2":

                length = wordWar2Materials.GetComponentsInChildren<EventCardContent>().Length;

                break;
        }

        count = 0;
        while (count < 7)
        {
            RandomMaterialIdList(length);
        };

        photonView.RPC("SetAllValues", RpcTarget.All, theme, randomNumberList);

    }

    [PunRPC]
    public void SetAllValues(string theme, int[] randomNumberList)
    {
        timelineTearsList = timeline.GetComponentsInChildren<TextMeshPro>();
        eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        switch (theme.ToUpper())
        {

            case "WORLD HISTORY":

                whMaterialList = worldHistoryMaterials.GetComponentsInChildren<EventCardContent>();

                for (int i = 0; i < eventCards.Length; i++)
                {
                    SetMaterialsToEventCards(i, whMaterialList, randomNumberList);
                }

                Array.Sort(selectedYears);
                SetSlotCounts();
                SetTimelineYears();
                break;

            case "WORLD WAR 2":

                wwMaterialList = wordWar2Materials.GetComponentsInChildren<EventCardContent>();

                for (int i = 0; i < eventCardsLength; i++)
                {
                    SetMaterialsToEventCards(i, wwMaterialList, randomNumberList);
                }

                Array.Sort(selectedYears);
                SetSlotCounts();
                SetTimelineYears();
                break;
        }
    }

    public void SetMaterialsToEventCards(int i, EventCardContent[] materialList, int[] randomNumberList)
    {
        // Verificações de bounds
        if (eventCards == null || i >= eventCards.Length || eventCards[i] == null) return;
        if (materialList == null || randomNumberList == null) return;
        if (i >= randomNumberList.Length || randomNumberList[i] >= materialList.Length) return;
        if (i >= selectedYears.Length) return;

        var material = materialList[randomNumberList[i]];
        if (material == null) return;

        eventCards[i].GetComponent<Renderer>().material = material.material;
        eventCards[i].slotYear = material.year;
        selectedYears[i] = material.year;
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
        if (timeline == null) return;

        timelineTearsList = timeline.GetComponentsInChildren<TextMeshPro>();

        for (int i = 0; i < timelineTearsList.Length && i < selectedYears.Length; i++)
        {
            if (timelineTearsList[i] != null)
            {
                timelineTearsList[i].text = selectedYears[i].ToString();
            }
        }
    }

    #region New Theme System

    /// <summary>
    /// Inicializa o sistema para um tema da API
    /// </summary>
    public void InitializeForTheme(ThemeData theme)
    {
        currentTheme = theme;
        DebugHelper.Log($"[RandomMaterial] Inicializando tema: {theme.name} ({theme.cards.Count} cartas disponíveis)");
    }

    /// <summary>
    /// Seleciona 7 cartas aleatórias do tema e configura o jogo (chamado pelo Master Client)
    /// </summary>
    public void GetRandomMaterialFromTheme()
    {
        if (currentTheme == null)
        {
            DebugHelper.Log("[RandomMaterial] ERRO: Tema não inicializado!");
            return;
        }

        // Selecionar 7 cartas aleatórias do tema
        selectedCards = SelectRandomCards(currentTheme.cards, 7);

        // Criar array de índices das cartas selecionadas para sincronizar via RPC
        int[] selectedIndices = new int[7];
        for (int i = 0; i < selectedCards.Count; i++)
        {
            selectedIndices[i] = currentTheme.cards.IndexOf(selectedCards[i]);
        }

        DebugHelper.Log($"[RandomMaterial] Cartas selecionadas: {string.Join(", ", selectedIndices)}");

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
            // Se o tema tem 7 ou menos cartas, usar todas
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
            DebugHelper.Log($"[RandomMaterial] ERRO: Tema {themeId} não encontrado no storage local!");
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
        timelineTearsList = timeline.GetComponentsInChildren<TextMeshPro>();
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

        DebugHelper.Log($"[RandomMaterial] Tema configurado: {theme.name}");
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
                    DebugHelper.Log($"[RandomMaterial] Carta {index}: Usando shader de composição");
                }
                else
                {
                    // Fallback: usar material padrão (imagem ocupa toda a carta)
                    var material = new Material(Shader.Find("Standard"));
                    material.mainTexture = texture;
                    material.SetFloat("_Glossiness", 0.2f);
                    renderer.material = material;
                    DebugHelper.Log($"[RandomMaterial] Carta {index}: Usando fallback (sem template)");
                }
            }
        }
        else
        {
            DebugHelper.Log($"[RandomMaterial] AVISO: Textura não encontrada para carta {themeCard.title}");
        }

        // Definir ano
        eventCard.slotYear = themeCard.year;

        // Associar ThemeCard ao EventCard para acesso aos dados do quiz
        eventCard.SetThemeCard(themeCard);

        DebugHelper.Log($"[RandomMaterial] Carta {index}: {themeCard.title} ({themeCard.year})");
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
