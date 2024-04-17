using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject roomNamePrefab;
    public Transform gridLayout;
    public GameObject joinRoomTip;
    public GameObject loginUI;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var room in cachedRoomList.Values)
        {
            GameObject newRoomButton = Instantiate(roomNamePrefab, gridLayout);
            TextMeshProUGUI roomText = newRoomButton.GetComponentInChildren<TextMeshProUGUI>();
            roomText.text = $"Room Name: {room.Name} (Player Number: {room.PlayerCount})";

            Button button = newRoomButton.GetComponent<Button>();
            string roomName = room.Name;

            button.onClick.AddListener(() => JoinRoom(roomName));
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for(int i=0; i<roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }

            if (info.PlayerCount == 0)
            {
                cachedRoomList.Remove(info.Name);
            }
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        loginUI.SetActive(false);
        joinRoomTip.SetActive(true);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            joinRoomTip.GetComponentInChildren<TextMeshProUGUI>().text = "Room joined! Wait for other players, 3 players needed," + 
                "\nnow " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)";
        }
    }

    public int GetCachedRoomCount()
    {
        return cachedRoomList.Count;
    }

    public bool IsRoomCached(string roomName)
    {
        return cachedRoomList.ContainsKey(roomName);
    }
}
