using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;

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
            Debug.Log("Nome do objeto encontrado: "+roomAlreadyExist?.name);

            if (!roomAlreadyExist)
            {
                GameObject Room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
                Room.name = roomList[i].Name;
                Room.GetComponent<Room>().buttonName.text = roomList[i].Name;
                Debug.Log("criando objeto com nome de: " + roomList[i].Name);
            }
            else
            {
                Debug.Log("Já existe uma sala com esse nome");
            }

        }

    }

}
