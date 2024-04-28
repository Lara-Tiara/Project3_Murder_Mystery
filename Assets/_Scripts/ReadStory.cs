using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;

public class ReadStory : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI content;
    public Slider maxSlider;
    public Slider rachelSlider;
    public Slider chloeSlider;
    public StoryNode[] maxNodes;
    public StoryNode[] rachelNodes;
    public StoryNode[] chloeNodes;
    public Clues clues;
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
    public GameObject cluesButton;
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
    [SerializeField]
    private int maxCheckClueNum;
    private int cluesPickedCount = 0;
    private Dictionary<string, GameObject> destroyClueButtons = new Dictionary<string, GameObject>();
    [HideInInspector]
    public List<Button> clueButtons = new List<Button>();

    private void Awake() {

        InitializeStoryNodes();
    }

    private void Start()
    {
        SetupStoryEnvironment();
        CluesManager.Instance.cluesCurrentRound.Clear();
        Debug.Log("Share Clue " + sharedClueNum);
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

        maxStorySplit = maxStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        rachelStorySplit = rachelStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        chloeStorySplit = chloeStory.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        LoadClues(clues);

        switch (GameDataManager.selectCharacter) {
            case 0:
                currentStory = maxStorySplit;
                maxPhone.SetActive(true);
                //LoadClues(activeMaxNodes.SelectMany(node => node.clues).ToList());
                //DisableButtonsMatchingRegex("(?i)Max.*");
                break;
            case 1:
                currentStory = rachelStorySplit;
                rachelPhone.SetActive(true);
                //LoadClues(activeRachelNodes.SelectMany(node => node.clues).ToList());
                //DisableButtonsMatchingRegex("(?i)Rachel.*");
                break;
            case 2:
                currentStory = chloeStorySplit;
                chloePhone.SetActive(true);
                //LoadClues(activeChloeNodes.SelectMany(node => node.clues).ToList());
                //DisableButtonsMatchingRegex("(?i)Chloe.*");
                break;
            default:
                break;
        }

        content.text = currentStory.Length > 0 ? currentStory[i] : "";
        
    }

    /*
    public void DisableButtonsMatchingRegex(string pattern)
    {
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        foreach (Transform child in gridLayoutClue)
        {
            Button button = child.GetComponent<Button>();

            TextMeshProUGUI textComponent = child.GetComponentsInChildren<TextMeshProUGUI>()
                                                 .FirstOrDefault(tmPro => tmPro.gameObject.name == "ButtonText");

            if (textComponent != null && regex.IsMatch(textComponent.text))
            {
                button.interactable = false;
            }
        }
    }
    */

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
            if (i == currentStory.Length - 1)
            {
                cluesButton.SetActive(true);
            }

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

    /*
    public void InitializeClueButtons()
    {
        ExitGames.Client.Photon.Hashtable cluesState = PhotonNetwork.CurrentRoom.CustomProperties;
        if (cluesState.ContainsKey("CluesState"))
        {
            var cluesStateDict = (Dictionary<string, bool>)cluesState["CluesState"];
            foreach (var clueState in cluesStateDict)
            {
                if (clueState.Value)
                {
                    photonView.RPC("DisableClueButton", RpcTarget.All, clueState.Key);
                }
            }
        }
    }
    */

    public void LoadClues(Clues newClues)
    {
        foreach (Transform child in gridLayoutClue)
        {
            Destroy(child.gameObject);
        }

        if (newClues.clues == null) return;

        foreach (Clue clue in newClues.clues)
        {
            GameObject newClueButton = Instantiate(clueButtonPrefab, gridLayoutClue);
            TextMeshProUGUI clueButtonText = newClueButton.GetComponentInChildren<TextMeshProUGUI>();
            clueButtonText.text = clue.clueKeyWord;

            Button button = newClueButton.GetComponent<Button>();
            clueButtons.Add(button);
            button.onClick.AddListener(() => {
            ShareClue(clue);
            clueButtonText.text = clue.clueText;
            button.interactable = false;
            ShowDestroyButton();
            });
            Transform destroyButtonTransform =  newClueButton.transform.Find("DestroyButton");
            Button destroyButton = destroyButtonTransform.GetComponent<Button>();
            destroyButton.onClick.AddListener(() => {
                CluesManager.Instance.DestroyClue(clue, sharedClueNum);
                newClueButton.SetActive(false);
                ShowDestroyButton();
                CheckReadOverActivation();
            });
            destroyClueButtons.TryAdd(clue.clueKeyWord, destroyButtonTransform.gameObject);
        }
    }

    public void ShowDestroyButton()
    {
        bool shouldDestroy = CluesManager.Instance.GetSharedCluesCount() > sharedClueNum;
        foreach (var clue in CluesManager.Instance.cluesCurrentRound)
        {
            if (destroyClueButtons.TryGetValue(clue.clueKeyWord, out GameObject destroyButtonGameObject))
            {
                destroyButtonGameObject.SetActive(shouldDestroy);
            } 
        } 
    }

    [PunRPC]
    public void DisableClueButton(string clueKeyWord)
    {
        foreach (Transform child in gridLayoutClue)
        {
            var btn = child.GetComponentInChildren<Button>();
            var textComponent = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent.text.Equals(clueKeyWord))
            {
                btn.interactable = false;
                break;
            }
        }
    }

    public void ShareClue(Clue clue)
    {
        CluesManager.Instance.AddSharedClue(clue);
        cluesPickedCount++;
        CheckReadOverActivation();
        photonView.RPC("DisableClueButton", RpcTarget.All, clue.clueKeyWord);
        CheckStopSelecting();
    }

    public void CheckStopSelecting()
    {
        if (cluesPickedCount < maxCheckClueNum)
        {
            return;
        }
        DisableClueSelection();
    }

    public void DisableClueSelection()
    {
        foreach (var button in clueButtons)
        {
            button.interactable = false;
        }
    }

    private void CheckReadOverActivation()
    {
        if (i == currentStory.Length - 1 && !hasReadOver && CluesManager.Instance.GetSharedCluesCount() == sharedClueNum)
        {
            readOverBtn.SetActive(true);
        } else {
            readOverBtn.SetActive(false);
        }
    }

}
