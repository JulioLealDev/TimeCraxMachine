using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;
using TMPro;
using TimeCrax.Core;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject roomPrefab;
    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    for(int i = 0; i < roomList.Count; i ++ )
    //    {
    //        GameObject Room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
    //        Room.GetComponent<Room>().buttonName.text = roomList[i].Name;
    //    }
    //}

    public void GetRoomsList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            GameObject roomAlreadyExist = GameObject.Find(roomList[i].Name);
            DebugHelper.Log("Nome do objeto encontrado: "+roomAlreadyExist?.name);
            DebugHelper.Log("Nome do objeto a ser criado: " + roomList[i].Name +" que est� no index: "+i);

            if (!roomAlreadyExist)
            {
                GameObject Room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
                Room.name = roomList[i].Name;
                string locked = roomList[i].CustomProperties["pass"].ToString();
                DebugHelper.Log("locked value: "+locked);
                DebugHelper.Log("isnullorwhite: "+string.IsNullOrWhiteSpace(locked));
                foreach (Transform child in Room.GetComponentsInChildren<Transform>())
                {
                    if(child.name == "NameRoomText")
                    {
                        child.GetComponent<TMP_Text>().text = roomList[i].Name;
                    }
                    else if(child.name == "PlayersText")
                    {
                        child.GetComponent<TMP_Text>().text = roomList[i].PlayerCount.ToString()+ "/"+ roomList[i].MaxPlayers.ToString();
                    }
                    else if (child.name == "ThemeText")
                    {
                        child.GetComponent<TMP_Text>().text = roomList[i].CustomProperties["the"].ToString();
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

                DebugHelper.Log("criando objeto com nome de: " + roomList[i].Name);
            }
            else
            {
                DebugHelper.Log("J� existe uma sala com esse nome");
            }

        }

    }

}
