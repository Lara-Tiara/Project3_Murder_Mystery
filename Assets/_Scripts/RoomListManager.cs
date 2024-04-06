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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        roomList.RemoveAll(room => room.PlayerCount == 0);

        foreach (var room in roomList)
        {
            GameObject newRoomButton = Instantiate(roomNamePrefab, gridLayout);
            TextMeshProUGUI roomText = newRoomButton.GetComponentInChildren<TextMeshProUGUI>();
            roomText.text = $"Room Name: {room.Name} (Player Number: {room.PlayerCount})";

            Button button = newRoomButton.GetComponent<Button>();
            string roomName = room.Name;

            button.onClick.AddListener(() => JoinRoom(roomName));
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
            joinRoomTip.GetComponent<Text>().text = "Room joined! Wait for other players, 3 players needed," + 
                "\nnow " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)";
        }
    }
}
