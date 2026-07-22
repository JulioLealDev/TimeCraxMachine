using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using TimeCrax.Core;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject roomPrefab;

    public void GetRoomsList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            GameObject roomAlreadyExist = GameObject.Find(roomList[i].Name);

            if (!roomAlreadyExist)
            {
                var contentObj = GameObject.Find("Content");
                if (contentObj == null)
                {
                    continue;
                }

                GameObject roomObj = Instantiate(roomPrefab, contentObj.transform, false);
                roomObj.name = roomList[i].Name;
                string locked = roomList[i].CustomProperties["pass"].ToString();
                string themeName = roomList[i].CustomProperties["the"].ToString();
                string themeId = roomList[i].CustomProperties.ContainsKey("themeId")
                    ? roomList[i].CustomProperties["themeId"].ToString()
                    : "";


                // Configurar dados do tema no Room
                Room roomComponent = roomObj.GetComponent<Room>();
                if (roomComponent != null)
                {
                    roomComponent.SetThemeData(themeId, themeName);
                }

                // Extrair apenas o nome da sala (sem o tema)
                string fullRoomName = roomList[i].Name;
                string displayName = fullRoomName;

                // Se o nome contém " - ", pegar apenas a parte antes do tema
                int separatorIndex = fullRoomName.LastIndexOf(" - ");
                if (separatorIndex > 0)
                {
                    displayName = fullRoomName.Substring(0, separatorIndex);
                }

                foreach (Transform child in roomObj.GetComponentsInChildren<Transform>())
                {
                    if(child.name == "NameRoomText")
                    {
                        child.GetComponent<TMP_Text>().text = displayName;
                    }
                    else if(child.name == "PlayersText")
                    {
                        child.GetComponent<TMP_Text>().text = roomList[i].PlayerCount.ToString()+ "/"+ roomList[i].MaxPlayers.ToString();
                    }
                    else if (child.name == "ThemeText")
                    {
                        child.GetComponent<TMP_Text>().text = themeName;
                    }
                    else if (child.name == "DifficultyText")
                    {
                        child.GetComponent<TMP_Text>().text = roomList[i].CustomProperties["dif"].ToString();
                    }
                    else if (child.name == "LockedText")
                    {
                        child.GetComponent<TMP_Text>().text = string.IsNullOrWhiteSpace(locked) ? "No" : "Yes";
                    }
                }

            }
            else
            {
            }

        }

    }

}
