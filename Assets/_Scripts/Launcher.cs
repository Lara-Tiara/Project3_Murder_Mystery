using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject connectTip;
    public GameObject nickNameInput;
    public GameObject createRoomBtn;
    public GameObject joinRoomBtn;
    public GameObject joinRandomQueueBtn;
    public GameObject joinRoomTip;
    public GameObject roomCodeInput;
    public int playerCount = 3; // Set according to your game's requirement

    private const string RoomNamePrefix = "Room_";

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon as soon as the game starts
    }

    public override void OnConnectedToMaster()
    {
        // Called when connected to the Photon master server
        connectTip.SetActive(false);
        createRoomBtn.SetActive(true);
        joinRoomBtn.SetActive(true);
        joinRandomQueueBtn.SetActive(true);
        nickNameInput.SetActive(true);

        Debug.Log("Connected");

        // Assign button listeners
        createRoomBtn.GetComponent<Button>().onClick.AddListener(CreateRoom);
        joinRoomBtn.GetComponent<Button>().onClick.AddListener(JoinRoom);
        joinRandomQueueBtn.GetComponent<Button>().onClick.AddListener(JoinQueue);
    }

    public void CreateRoom()
    {
        PhotonNetwork.NickName = nickNameInput.GetComponent<InputField>().text;
        string randomRoomName = RoomNamePrefix + Random.Range(1000, 9999); // Generate a unique room name

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = (byte)playerCount };
        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);

        UpdateUIForRoomJoining();
    }

    public void JoinRoom()
    {
        PhotonNetwork.NickName = nickNameInput.GetComponent<InputField>().text;
        string roomCode = roomCodeInput.GetComponent<InputField>().text.Trim();

        if (!string.IsNullOrEmpty(roomCode))
        {
            PhotonNetwork.JoinRoom(roomCode);
            UpdateUIForRoomJoining();
        }
        else
        {
            Debug.LogError("Room code is empty!");
            // Optionally, show an error message in the UI
        }
    }

    public void JoinQueue()
    {
        PhotonNetwork.NickName = nickNameInput.GetComponent<InputField>().text;
        PhotonNetwork.JoinRandomRoom(); // Attempts to join an existing room; if it fails, OnJoinRandomFailed is called
        UpdateUIForRoomJoining();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // No random room available, so create a new one with a unique name
        CreateRoom();
    }

    private void UpdateUIForRoomJoining()
    {
        // Common UI updates when attempting to join or create a room
        createRoomBtn.SetActive(false);
        joinRoomBtn.SetActive(false);
        joinRandomQueueBtn.SetActive(false);
        nickNameInput.SetActive(false);
        joinRoomTip.SetActive(true);
    }

    private void Update()
    {
        // Monitor the current room's status and load the game scene when enough players have joined
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
            // Load your game scene here
            // SceneManager.LoadScene("YourGameSceneName");
        }
    }

    // Callback when successfully joined any room
    public override void OnJoinedRoom()
    {
        Debug.Log($"Successfully joined the room: {PhotonNetwork.CurrentRoom.Name}");
        joinRoomTip.GetComponent<Text>().text = $"Joined room {PhotonNetwork.CurrentRoom.Name}. Waiting for other players...";
    }

    // Callback when room creation failed
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}, attempting again...");
        CreateRoom(); // Attempt to create another room with a new random name
    }
}
