using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Vote : MonoBehaviourPunCallbacks
{
    public int murder;
    public int suspect;
    public GameObject endUI;
    public TextMeshProUGUI resultText;

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

        if(buttonIndex == murder)
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
        bool rachelVotedOut = murder == 0 && clickedRightCount >= 2;
        bool chloeVotedOut = murder == 1 && clickedRightCount >= 2;

        if (GameDataManager.selectCharacter == 0)
        {
            resultText.text = rachelVotedOut ? "You Lose" : "You Win";
        }
        else if (GameDataManager.selectCharacter == 1)
        {
            resultText.text = chloeVotedOut ? "You Win" : "You Lose";
        }
        else if (GameDataManager.selectCharacter == 2)
        {
            if (rachelVotedOut)
            {
                resultText.text = "You Win";
            }
            else
            {
                if (GameDataManager.voteCharacter == 0)
                {
                    resultText.text = "You lose but you voted the true murder.";
                }
                else
                {
                    resultText.text = "You Lose";
                }
            }
        }

        endUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
