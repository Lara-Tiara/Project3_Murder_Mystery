using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject namePrefab;
    public Transform gridLayoutRoom;
    public GameObject joinRoomTip;
    public GameObject loginUI;
    public GameObject playerListUI;
    public GameObject readyButton;
    public GameObject leaveRoomButton;
    public Transform gridLayoutPlayer;
    private Player newPlayer;
    private List<Player> playerList = new List<Player>(); 
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<Player, GameObject> playerToGameObjectMap = new Dictionary<Player, GameObject>();
    private Dictionary<Player, bool> playerReadyMap = new Dictionary<Player, bool>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
        foreach (Transform child in gridLayoutRoom)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var room in cachedRoomList.Values)
        {
            GameObject newRoomButton = Instantiate(namePrefab, gridLayoutRoom);
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
        playerListUI.SetActive(true);
        joinRoomTip.SetActive(true);
        readyButton.SetActive(true);
        leaveRoomButton.SetActive(true);
    }

    public void UpdatePlayerList(List<Player> playerList)
    {
        foreach (Transform child in gridLayoutRoom)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in playerList)
        {
            GameObject newPlayerButton = Instantiate(namePrefab, gridLayoutPlayer);
            TextMeshProUGUI playerName = newPlayerButton.GetComponentInChildren<TextMeshProUGUI>();
            playerName.text = player.NickName;
            playerToGameObjectMap[newPlayer] = newPlayerButton;
            playerReadyMap[newPlayer] = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerList.Add(newPlayer);
        UpdatePlayerList(playerList);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerToGameObjectMap.TryGetValue(otherPlayer, out GameObject playerObject))
        {
            Destroy(playerObject);
            playerToGameObjectMap.Remove(otherPlayer);
            playerList.Remove(otherPlayer);
        }
    }

    public void PlayerReady(Player player)
    {
        if (playerReadyMap.ContainsKey(player) && playerToGameObjectMap.TryGetValue(player, out GameObject playerButton))
        {
            playerReadyMap[player] = true;
            HighlightPlayerButton(playerButton);
        }
    }

    private void HighlightPlayerButton(GameObject playerButton)
    {
        Button button = playerButton.GetComponent<Button>();
        if (button != null)
        {
            button.Select();
        }
    }

    public void OnReadyButtonPressed()
{
    PhotonHashtable properties = new PhotonHashtable { { "IsReady", true } };
    PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    PlayerReady(PhotonNetwork.LocalPlayer);
    photonView.RPC("MarkPlayerAsReady", RpcTarget.Others, PhotonNetwork.LocalPlayer);
}

    [PunRPC]
    public void MarkPlayerAsReady(Player player)
    {
        PlayerReady(player);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            joinRoomTip.GetComponentInChildren<TextMeshProUGUI>().text = "Room joined! Wait for other players, 3 players needed," + 
                "\nnow " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)";
        }
    }
}
