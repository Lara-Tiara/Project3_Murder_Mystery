using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

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
    public Transform gridLayoutClue;
    public GameObject clueButtonPrefab;
    [SerializeField]
    private int sharedClueNum;
    private int cluesPickedCount = 0;


    private void Awake() {

        InitializeStoryNodes();
    }

    private void Start()
    {
        SetupStoryEnvironment();
    }

    private void InitializeStoryNodes() 
    {
        SetInitialActiveNodes(maxNodes);
        SetInitialActiveNodes(rachelNodes);
        SetInitialActiveNodes(chloeNodes);
    }

    private void SetInitialActiveNodes(StoryNode[] nodes) 
    {
        foreach (var node in nodes) {
            if(node.nodeId == 0) {
                node.isActive = true;
            }
        }
    }

    private void SetupStoryEnvironment() 
    {
        i = 0;
        readOverCount = 0;

        List<StoryNode> activeMaxNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(maxNodes));
        List<StoryNode> activeRachelNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(rachelNodes));
        List<StoryNode> activeChloeNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(chloeNodes));

        maxStory = CombineStoryText(activeMaxNodes);
        rachelStory = CombineStoryText(activeRachelNodes);
        chloeStory = CombineStoryText(activeChloeNodes);

        maxStorySplit = maxStory.Split(new string[] { "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        rachelStorySplit = rachelStory.Split(new string[] { "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        chloeStorySplit = chloeStory.Split(new string[] { "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        switch (GameDataManager.selectCharacter) {
            case 0:
                currentStory = maxStorySplit;
                maxPhone.SetActive(true);
                LoadClues(activeMaxNodes.SelectMany(node => node.clues).ToList());
                break;
            case 1:
                currentStory = rachelStorySplit;
                rachelPhone.SetActive(true);
                LoadClues(activeRachelNodes.SelectMany(node => node.clues).ToList());
                break;
            case 2:
                currentStory = chloeStorySplit;
                chloePhone.SetActive(true);
                LoadClues(activeChloeNodes.SelectMany(node => node.clues).ToList());
                break;
            default:
                break;
        }

        content.text = currentStory.Length > 0 ? currentStory[i] : "";
    }

    public string CombineStoryText(List<StoryNode> storyNodes)
    {
        string combinedText = "";

        foreach (StoryNode node in storyNodes)
        {
            if (node != null && node.storyText != null)
            {
                combinedText += node.storyText.text + "\n\n";
            }
        }
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

            CheckReadOverActivation();
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

    public void LoadClues(List<StoryClue> clues)
    {
        foreach (Transform child in gridLayoutClue)
        {
            Destroy(child.gameObject);
        }

        foreach (StoryClue clue in clues)
        {
            GameObject newClueButton = Instantiate(clueButtonPrefab, gridLayoutClue);
            TextMeshProUGUI clueButtonText = newClueButton.GetComponentInChildren<TextMeshProUGUI>();
            clueButtonText.text = clue.clueText;

            Button button = newClueButton.GetComponent<Button>();
            button.onClick.AddListener(() => {
            ShareClue(clue);
            button.interactable = false;
            });
        }
    }

    public void ShareClue(StoryClue clue)
    {
        if (cluesPickedCount < sharedClueNum)
        {
            CluesManager.Instance.AddSharedClue(clue);
            cluesPickedCount++;
            CheckReadOverActivation();
        }
    }

    private void CheckReadOverActivation()
    {
        if (i == currentStory.Length - 1 && !hasReadOver && cluesPickedCount >= sharedClueNum)
        {
            readOverBtn.SetActive(true);
        }
    }

}
