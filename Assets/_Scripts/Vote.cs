using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Vote : MonoBehaviourPunCallbacks
{
    public int targetChar;
    public GameObject endUI;
    public Text resultText;

    public Button[] buttons;
    private static int clickedButtonCount = 0;
    private static int clickedRightCount = 0;
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

        GameDataManager.voteCharacter = buttonIndex;

        photonView.RPC("IncreaseClickedButtonCount", RpcTarget.All);

        buttons[buttonIndex].interactable = false;

        if(buttonIndex == targetChar)
            photonView.RPC("IncreaseRightButtonCount", RpcTarget.All);

        //string playerName = PhotonNetwork.LocalPlayer.NickName;
        //photonView.RPC("ShowPlayerName", RpcTarget.All, buttonIndex, playerName);

        hasSelect = true;

        CheckAllPlayersClicked();
    }

    private void CheckAllPlayersClicked()
    {
        if (clickedButtonCount >= buttons.Length)
        {
            photonView.RPC("ShowEndUI", RpcTarget.All);
        }
    }

    [PunRPC]
    private void IncreaseClickedButtonCount()
    {
        clickedButtonCount++;
    }

    [PunRPC]
    private void IncreaseRightButtonCount()
    {
        clickedRightCount++;
    }

    [PunRPC]
    private void ShowPlayerName(int buttonIndex, string playerName)
    {
        buttons[buttonIndex].transform.GetChild(0).gameObject.SetActive(true);
        Text buttonText = buttons[buttonIndex].GetComponentInChildren<Text>();
        buttonText.text = playerName;
    }

    [PunRPC]
    private void ShowEndUI()
    {
        if(GameDataManager.selectCharacter == targetChar)
        {
            if (clickedRightCount >= 2)
                resultText.text = "You Lose";
            else
                resultText.text = "You Win";
        }
        else
        {
            if (clickedRightCount >= 2)
                resultText.text = "You Win";
            else
                resultText.text = "You Lose";
        }

        endUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
