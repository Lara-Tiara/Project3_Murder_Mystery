using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Chat : MonoBehaviourPunCallbacks
{
    public float time;
    public TextMeshProUGUI timedownText;
    public int nextScene;
    public TMP_InputField iptMessage;
    public TextMeshProUGUI chatText;

    private void Update()
    {
        time -= Time.deltaTime;
        timedownText.text = time.ToString("f1");
        if (time <= 0)
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void SendMessage()
    {
        string message = iptMessage.text;
        if (!string.IsNullOrEmpty(message))
        {
            photonView.RPC("ReceiveMessage", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, message);
            iptMessage.text = "";
        }
    }

    [PunRPC]
    private void ReceiveMessage(string sender, string message)
    {
        string formattedMessage = "<align=left>" + sender + ": " + message + "</align>\n";
        if (sender == PhotonNetwork.LocalPlayer.NickName)
        {
            formattedMessage = "<align=right>" + sender + ": " + message + "</align>\n";
        }
        chatText.text += formattedMessage;
    }
}
