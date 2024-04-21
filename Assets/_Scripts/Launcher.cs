using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using TMPro;
using System.Text.RegularExpressions;

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
    public GameObject inValidInputText;
    


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
        if (string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            nameUI.SetActive(true);
        }
        PhotonNetwork.JoinLobby();
    }

    public void StartButton()
    {
        string nickname = nickNameIpt.GetComponent<TMP_InputField>().text;
        Debug.Log("shdsiodyh");

        if (string.IsNullOrWhiteSpace(nickname) || !Regex.IsMatch(nickname, @"^[a-zA-Z0-9]+$"))
        {
            inValidInputText.GetComponent<TextMeshProUGUI>().text = "Nickname cannot be empty and must contain only letters and numbers.";
            StartCoroutine(InValidInputText());
            //Debug.LogError("Nickname cannot be empty and must contain only letters and numbers.");
            nameUI.SetActive(true);
            return;
        }

        nameUI.SetActive(false);
        PhotonNetwork.NickName = nickname;
        Debug.Log("User Nickname: " + PhotonNetwork.LocalPlayer.NickName);
        Debug.Log("User ID: " + PhotonNetwork.LocalPlayer.UserId);
        loginUI.SetActive(true);
        if (PhotonNetwork.InLobby)
        {
            roomListUI.SetActive(true);
        }
    }

    private IEnumerator InValidInputText()
    {
        inValidInputText.SetActive(true);
        yield return new WaitForSeconds(3);
        inValidInputText.SetActive(false);
    }
    
    public void JoinGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Not connected to Photon.");
            return;
        }

        string room = roomName.text;
        if (string.IsNullOrWhiteSpace(room) || room.Length < 3 || !Regex.IsMatch(room, @"^[a-zA-Z0-9]+$"))
        {
            inValidInputText.GetComponent<TextMeshProUGUI>().text = "Room name cannot be empty, must be at least three characters long, and contain only letters and numbers.";
            StartCoroutine(InValidInputText());
            //Debug.LogError("Room name cannot be empty, must be at least three characters long, and contain only letters and numbers.");
            loginUI.SetActive(true);
            return;
        }

        loginUI.SetActive(false);
        PhotonNetwork.JoinOrCreateRoom(roomName.text, new Photon.Realtime.RoomOptions { MaxPlayers = playerCount }, default);

        joinRoomTip.SetActive(true);
    }

    private void Update()
    {

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Successful Join the Room");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

}
