using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;


public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject namePrefab;
    public GameObject playerListPrefab;
    public Transform gridLayoutRoom;
    public GameObject joinRoomTip;
    public GameObject loginUI;
    public GameObject playerListUI;
    public GameObject readyButton;
    public GameObject leaveRoomButton;
    public Transform gridLayoutPlayer;
    public Sprite notReadySprite;
    public Sprite isReadySprite;
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<Player, GameObject> playerToGameObjectMap = new Dictionary<Player, GameObject>();
    private Dictionary<Player, bool> playerReadyMap = new Dictionary<Player, bool>();
    private bool isLocalPlayerReady = false;

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
    }

    public void UpdatePlayerList()
    {
        foreach (Transform child in gridLayoutPlayer)
        {
            Destroy(child.gameObject);
        }

        playerReadyMap.Clear();

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            GameObject newPlayerButton = Instantiate(playerListPrefab, gridLayoutPlayer);
            TextMeshProUGUI playerName = newPlayerButton.GetComponentInChildren<TextMeshProUGUI>();
            playerName.text = player.NickName;
            playerToGameObjectMap[player] = newPlayerButton;
            playerReadyMap[player] = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log("Test " + newPlayer.NickName);
        photonView.RPC("PlayerReadyStateChanged", RpcTarget.All, PhotonNetwork.LocalPlayer, isLocalPlayerReady);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        OnPlayerEnteredRoom(PhotonNetwork.LocalPlayer);
        loginUI.SetActive(false);
        playerListUI.SetActive(true);
        joinRoomTip.SetActive(true);
        readyButton.SetActive(true);
        leaveRoomButton.SetActive(true);
        photonView.RPC("PlayerReadyStateChanged", RpcTarget.All, PhotonNetwork.LocalPlayer, false);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        loginUI.SetActive(true);
        playerListUI.SetActive(false);
        joinRoomTip.SetActive(false);
        readyButton.SetActive(false);
        leaveRoomButton.SetActive(false);

        isLocalPlayerReady = false;
        Toggle toggle = readyButton.GetComponent<Toggle>();
        toggle.isOn = false;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerToGameObjectMap.TryGetValue(otherPlayer, out GameObject playerObject))
        {
            Destroy(playerObject);
            playerToGameObjectMap.Remove(otherPlayer);
            playerReadyMap.Remove(otherPlayer);
        }
    }

    [PunRPC]
    public void PlayerReadyStateChanged(Player player, bool isReady)
    {
        if (playerReadyMap.ContainsKey(player) && playerToGameObjectMap.TryGetValue(player, out GameObject playerButton))
        {
            Debug.Log("Set Dictionary " + player.NickName + isReady);
            playerReadyMap[player] = isReady;
            HighlightPlayerButton(playerButton, isReady);

            Debug.Log($"Count {GameDataManager.playerCount}, {playerReadyMap.Count}");
            if (playerReadyMap.Count != GameDataManager.playerCount)
            {
                return;
            }

            bool allReady = true;

            foreach (var isReadyPlayer in playerReadyMap)
            {
                Debug.Log($"Ready {isReadyPlayer.Key}, {isReadyPlayer.Value}");
                allReady = allReady && isReadyPlayer.Value;
            }

            if (allReady && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("LoadGameLevel", RpcTarget.All);
            }
        }
    }

    private void HighlightPlayerButton(GameObject playerButton, bool isReady)
    {
        Image spriteRenderer = playerButton.GetComponent<Image>();
        if (spriteRenderer != null)
        {
            Debug.Log("Set Sprite " + spriteRenderer);
            if (isReady)
            {
                spriteRenderer.sprite = isReadySprite;
            } else {
                spriteRenderer.sprite = notReadySprite;
            }
        }
    }

    //On Ready Toggle
    public void OnReadyStateChanged(bool isReady)
    {
        Debug.Log("Ready State " + isReady);
        isLocalPlayerReady = isReady;
        photonView.RPC("PlayerReadyStateChanged", RpcTarget.All, PhotonNetwork.LocalPlayer, isReady);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            joinRoomTip.GetComponentInChildren<TextMeshProUGUI>().text = "Room joined! Wait for other players, 3 players needed," + 
                "\nnow " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)";
        }
    }


    [PunRPC]
    void LoadGameLevel()
    {
        SceneManager.LoadScene(1);
    }
}
