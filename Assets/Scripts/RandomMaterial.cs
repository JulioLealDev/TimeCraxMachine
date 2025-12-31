using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using TimeCrax.Core;

public class RandomMaterial : MonoBehaviourPunCallbacks
{

    public GameObject worldHistoryMaterials;
    public GameObject wordWar2Materials;
    public GameObject worldScienceMaterials;
    public GameObject timeline;
    public int randomNumber = -1;
    private int[] randomNumberList = new int[7] { -1, -1, -1, -1, -1, -1, -1 };
    private int[] selectedYears = new int[7];
    private EventCard[] eventCards;
    private EventCardContent[] wwMaterialList;
    private EventCardContent[] whMaterialList;
    private TextMeshPro[] timelineTearsList;
    private int eventCardsLength = 7;
    private int count = 0;

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

}
