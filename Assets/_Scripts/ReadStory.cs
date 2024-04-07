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
    public Slider maxSlider;
    public Slider rachelSlider;
    public Slider chloeSlider;

    public TextAsset maxTextAsset;
    public TextAsset rachelTextAsset;
    public TextAsset chloeTextAsset;
    public string[] maxStory;
    public string[] rachelStory;
    public string[] chloeStory;
    public GameObject maxPhone;
    public GameObject rachelPhone;
    public GameObject chloePhone;

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

        maxStory = maxTextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        rachelStory = rachelTextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        chloeStory = chloeTextAsset.text.Split(new string[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        switch (GameDataManager.selectCharacter)
        {
            case 0:
                currentStory = maxStory;
                maxPhone.SetActive(true);
                break;
            case 1:
                currentStory = rachelStory;
                rachelPhone.SetActive(true);
                break;
            case 2:
                currentStory = chloeStory;
                chloePhone.SetActive(true);
                break;
            default:
                break;
        }

        content.text = currentStory.Length > 0 ? currentStory[i] : "";
    }

    [PunRPC]
    private void UpdateSliderValue(int targetPlayer,int targetProgress)
    {
        maxSlider.maxValue = maxStory.Length - 1;
        rachelSlider.maxValue = rachelStory.Length - 1;
        chloeSlider.maxValue = chloeStory.Length - 1;

        switch (targetPlayer)
        {
            case 0:
                maxSlider.value = targetProgress;
                break;
            case 1:
                rachelSlider.value = targetProgress;
                break;
            case 2:
                chloeSlider.value = targetProgress;
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
