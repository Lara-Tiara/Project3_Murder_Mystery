using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject connectTip;
    public GameObject nickNameIpt;
    public GameObject roomNamePrefab;
    public GameObject startBtn;
    public GameObject joinRoomTip;
    public GameObject roomListUI;
    public GameObject nameUI;
    public GameObject loginUI;
    public TMP_InputField roomName;
    public TMP_InputField playerName;
    public int playerCount;

    void Start()
    {
        GameDataManager.playerCount = playerCount;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected");

        connectTip.SetActive(false);
        nameUI.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public void StartButton()
    {
        nameUI.SetActive(false);
        PhotonNetwork.NickName = nickNameIpt.GetComponent<InputField>().text;
        loginUI.SetActive(true);
        if(PhotonNetwork.InLobby)
        {
            roomListUI.SetActive(true);
        }
    }
    
    public void JoinGame()
    {
        if(!PhotonNetwork.IsConnected)
            return;

        if(roomName.text.Length < 2)
            return;

        loginUI.SetActive(false);
        PhotonNetwork.JoinOrCreateRoom(roomName.text, new Photon.Realtime.RoomOptions { MaxPlayers = playerCount }, default);

        joinRoomTip.SetActive(true);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
            {
                SceneManager.LoadScene(1);
            }
            joinRoomTip.GetComponent<Text>().text = "Room joined! Wait for other players, 3 players needed," + 
                "\nnow " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)";
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Successful Join the Room");
    }
}
