using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class ReadStory : MonoBehaviourPunCallbacks
{
    public TextMeshPro content;
    public Slider player1Slider;
    public Slider player2Slider;
    public Slider player3Slider;

    public TextAsset player1TextAsset;
    public TextAsset player2TextAsset;
    public TextAsset player3TextAsset;
    public string[] player1Story;
    public string[] player2Story;
    public string[] player3Story;
    public GameObject player1Phone;
    public GameObject player2Phone;
    public GameObject player3Phone;

    public int nextScene;

    private string[] currentStory;
    private int i;

    public GameObject readOverBtn;
    public GameObject[] readOverTips;
    private static int readOverCount;
    private bool hasReadOver;

    private void Start()
    {
        i = 0;
        readOverCount = 0;

        player1Story = player1TextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        player2Story = player2TextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        player3Story = player3TextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        switch (GameDataManager.selectCharacter)
        {
            case 0:
                currentStory = player1Story;
                player1Phone.SetActive(true);
                break;
            case 1:
                currentStory = player2Story;
                player2Phone.SetActive(true);
                break;
            case 2:
                currentStory = player3Story;
                player3Phone.SetActive(true);
                break;
            default:
                break;
        }

        content.text = currentStory.Length > 0 ? currentStory[i] : "";
    }

    [PunRPC]
    private void UpdateSliderValue(int targetPlayer,int targetProgress)
    {
        player1Slider.maxValue = player1Story.Length - 1;
        player2Slider.maxValue = player2Story.Length - 1;
        player3Slider.maxValue = player3Story.Length - 1;

        switch (targetPlayer)
        {
            case 0:
                player1Slider.value = targetProgress;
                break;
            case 1:
                player2Slider.value = targetProgress;
                break;
            case 2:
                player3Slider.value = targetProgress;
                break;
            default:
                break;
        }
    }

    public void NextPage()
    {
        if (i < currentStory.Length - 1)
        {
            i++;
            content.text = currentStory[i];

            photonView.RPC("UpdateSliderValue", RpcTarget.All, GameDataManager.selectCharacter, i);

            if (i == currentStory.Length - 1 && !hasReadOver)
                readOverBtn.SetActive(true);
        }
    }

    public void LastPage()
    {
        if (i > 0)
        {
            i--;
            content.text = currentStory[i];

            photonView.RPC("UpdateSliderValue", RpcTarget.All, GameDataManager.selectCharacter, i);
        }
    }

    public void ReadOver()
    {
        if (hasReadOver)
            return;

        hasReadOver = true;

        photonView.RPC("AddReadOver", RpcTarget.All, GameDataManager.selectCharacter);

        readOverBtn.SetActive(false);
    }

    [PunRPC]
    private void AddReadOver(int i)
    {
        readOverCount++;

        readOverTips[i].SetActive(true);
    }

    private void Update()
    {
        if (readOverCount >= GameDataManager.playerCount)
        {
            readOverCount = 0;
            SceneManager.LoadScene(nextScene);
        }
    }
}
