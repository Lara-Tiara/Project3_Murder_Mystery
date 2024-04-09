using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class ReadStory : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI content;
    public Slider maxSlider;
    public Slider rachelSlider;
    public Slider chloeSlider;

    public StoryNode[] maxNodes;
    public StoryNode[] rachelNodes;
    public StoryNode[] chloeNodes;
    private string[] maxStorySplit;
    private string[] rachelStorySplit;
    private string[] chloeStorySplit;
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
    private string maxStory;
    private string rachelStory;
    private string chloeStory;

    private void Awake() {

        foreach (var node in maxNodes)
        {
            if(node.nodeId == 0){
                node.isActive = true;
            }
        }

        foreach (var node in rachelNodes)
        {
            if(node.nodeId == 0){
                node.isActive = true;
            }
        }

        foreach (var node in chloeNodes)
        {
            if(node.nodeId == 0){
                node.isActive = true;
            }
        }
    }

    private void Start()
    {
        i = 0;
        readOverCount = 0;
        
        List<StoryNode> activeMaxNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(maxNodes));
        Debug.Log("Active Max Nodes Count: " + activeMaxNodes.Count);
        List<StoryNode> activeRachelNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(rachelNodes));
        Debug.Log("Active Rachel Nodes Count: " + activeRachelNodes.Count);
        List<StoryNode> activeChloeNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(chloeNodes));
        Debug.Log("Active Chloe Nodes Count: " + activeChloeNodes.Count);
    
        maxStory = CombineStoryText(activeMaxNodes);
        rachelStory = CombineStoryText(activeRachelNodes);
        chloeStory = CombineStoryText(activeChloeNodes);

        maxStorySplit = maxStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        rachelStorySplit = rachelStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        chloeStorySplit = chloeStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        switch (GameDataManager.selectCharacter)
        {
            case 0:
                currentStory = maxStorySplit;
                maxPhone.SetActive(true);
                break;
            case 1:
                currentStory = rachelStorySplit;
                rachelPhone.SetActive(true);
                break;
            case 2:
                currentStory = chloeStorySplit;
                chloePhone.SetActive(true);
                break;
            default:
                break;
        }

        content.text = currentStory.Length > 0 ? currentStory[i] : "";
    }

    public string CombineStoryText(List<StoryNode> storyNodes)
    {
        // Initialize an empty string to hold the combined text
        string combinedText = "";

        // Iterate through each StoryNode in the list
        foreach (StoryNode node in storyNodes)
        {
            // Check if the node and its storyText field are not null to avoid null reference exceptions
            if (node != null && node.storyText != null)
            {
                // Append this node's text to the combinedText string, adding a newline for separation
                combinedText += node.storyText.text + "\n\n";
            }
        }

        // Return the combined text
        return combinedText;
    }

    [PunRPC]
    private void UpdateSliderValue(int targetPlayer,int targetProgress)
    {
        maxSlider.maxValue = maxStorySplit.Length - 1;
        rachelSlider.maxValue = rachelStorySplit.Length - 1;
        chloeSlider.maxValue = chloeStorySplit.Length - 1;

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
        //StoryNode currentNode = GetCurrentStoryNode();
        //currentNode.hasBeenRead = true;

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

    public List<StoryNode> LoadActiveCharacterStoryNodes(List<StoryNode> storyNodes)
    {
        List<StoryNode> activeStoryNodes = storyNodes.FindAll(node => node.isActive);
        return activeStoryNodes;
    }
}
