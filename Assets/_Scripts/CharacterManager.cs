using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviourPunCallbacks
{
    public Button[] buttons;
    private static int clickedButtonCount = 0;
    private bool hasSelect = false;

    private void Start()
    {
        
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (PhotonNetwork.IsMasterClient && clickedButtonCount >= buttons.Length || hasSelect)
        {
            return;
        }

        GameDataManager.selectCharacter = buttonIndex;

        photonView.RPC("IncreaseClickedButtonCount", RpcTarget.All);

        photonView.RPC("DisableButton", RpcTarget.All, buttonIndex);

        string playerName = PhotonNetwork.LocalPlayer.NickName;
        photonView.RPC("ShowPlayerName", RpcTarget.All, buttonIndex, playerName);

        hasSelect = true;

        CheckAllPlayersClicked();
    }

    private void CheckAllPlayersClicked()
    {
        if (clickedButtonCount >= buttons.Length)
        {
            photonView.RPC("LoadNextScene", RpcTarget.All);
        }
    }

    [PunRPC]
    private void IncreaseClickedButtonCount()
    {
        clickedButtonCount++;
    }

    [PunRPC]
    private void DisableButton(int clickedButtonIndex)
    {
        buttons[clickedButtonIndex].interactable = false;
    }

    [PunRPC]
    private void ShowPlayerName(int buttonIndex, string playerName)
    {
        buttons[buttonIndex].transform.GetChild(0).gameObject.SetActive(true);
        Text buttonText = buttons[buttonIndex].GetComponentInChildren<Text>();
        buttonText.text = playerName;
    }

    [PunRPC]
    private void LoadNextScene()
    {
        //SceneManager.LoadScene(2);
        SceneManager.LoadScene(8);
    }
}
