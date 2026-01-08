using Photon.Pun;
using Photon.Realtime;
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

    private void RandomMaterialIdList(int materialListLenght)
    {

        randomNumber = UnityEngine.Random.Range(0, materialListLenght);

        if (randomNumberList.Contains(randomNumber))
        {
            //DebugHelper.Log("ja existe!!");
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

        //DebugHelper.Log("tamanho da lista de materiais: " + length);

        count = 0;
        while (count < 7)
        {
            RandomMaterialIdList(length);
        };

        for(int i = 0;i < randomNumberList.Length; i++)
        {
            //DebugHelper.Log("random number ["+i+"]: "+randomNumberList[i]);
        }

        photonView.RPC("SetAllValues", RpcTarget.All, theme, randomNumberList);

    }

    [PunRPC]
    public void SetAllValues(string theme, int[] randomNumberList)
    {
        timelineTearsList = timeline.GetComponentsInChildren<TextMeshPro>();
        eventCards = FindObjectsByType<EventCard>(FindObjectsSortMode.None);

        //DebugHelper.Log("Theme 3: " + theme.ToUpper());

        switch (theme.ToUpper())
        {

            case "WORLD HISTORY":

                whMaterialList = worldHistoryMaterials.GetComponentsInChildren<EventCardContent>();

                for (int i = 0; i < eventCards.Length; i++)
                {

                    SetMaterialsToEventCards(i, whMaterialList, randomNumberList);
                }

                Array.Sort(selectedYears);

                //photonView.RPC("SetSlotCounts", RpcTarget.All);
                SetSlotCounts();

                //photonView.RPC("SetTimelineYears", RpcTarget.All);
                SetTimelineYears();

                break;

            case "WORLD WAR 2":

                wwMaterialList = wordWar2Materials.GetComponentsInChildren<EventCardContent>();

                for (int i = 0; i < eventCardsLength; i++)
                {
                    SetMaterialsToEventCards(i, wwMaterialList, randomNumberList);
                }

                Array.Sort(selectedYears);

                //photonView.RPC("SetSlotCounts", RpcTarget.All);
                SetSlotCounts();

                //photonView.RPC("SetTimelineYears", RpcTarget.All);
                SetTimelineYears();


                break;

            case "WORLD SCIENCE":
                // code block
                break;
        }
    }

    //[PunRPC]
    public void SetMaterialsToEventCards(int i, EventCardContent[] materialList, int[] randomNumberList)
    {
        //DebugHelper.Log("carta [" + i + "] recebendo materia: " +materialList[randomNumberList[i]].material.name);
        //DebugHelper.Log("carta [" + i + "] recebendo ano: " + materialList[randomNumberList[i]].year);
        eventCards[i].GetComponent<Renderer>().material = materialList[randomNumberList[i]].material;
        eventCards[i].slotYear = materialList[randomNumberList[i]].year;
        selectedYears[i] = materialList[randomNumberList[i]].year;

    }

    //[PunRPC]
    public void SetSlotCounts()
    {
        for (int i = 0; i < eventCardsLength; i++)
        {

            for (int y = 0; y < selectedYears.Length; y++)
            {
                //DebugHelper.Log("eventCards.slotYear: "+ eventCards[i].slotYear+"  == selectedYears"+ selectedYears[y]);
                if (eventCards[i].slotYear == selectedYears[y])
                {
                    eventCards[i].slotCount = y + 1;
                    //DebugHelper.Log("carta index: " + i + " recebe valor: " + (y+1));
                }
            }

        }
    }

    //[PunRPC]
    public void SetTimelineYears()
    {

        timelineTearsList = timeline.GetComponentsInChildren<TextMeshPro>();

        //DebugHelper.Log("Entrou no SetTime");
        for (int i = 0; i < timelineTearsList.Length; i++)
        {
            //DebugHelper.Log("--- " + timelineTearsList[i].name);
            timelineTearsList[i].text = selectedYears[i].ToString();
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
            // Criar material com a textura
            var renderer = eventCard.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Clonar material para não afetar outros objetos
                var material = new Material(renderer.material);
                material.mainTexture = texture;
                renderer.material = material;
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
        // selectedCards já está ordenado por ano
        for (int i = 0; i < eventCards.Length; i++)
        {
            for (int y = 0; y < selectedCards.Count; y++)
            {
                if (eventCards[i].slotYear == selectedCards[y].year)
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
