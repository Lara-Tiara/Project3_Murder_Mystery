using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class Vote : MonoBehaviourPunCallbacks
{
    public int murder;
    public int suspect;
    public int[] murderEachRound;
    public TextMeshProUGUI timedownText;
    public float time;
    public TextMeshProUGUI roundTitle;
    public string[] titleText;
    public GameObject endUI;
    public TextMeshProUGUI endingText;
    public Button[] buttons;
    public StoryNode[] maxNodes;
    public StoryNode[] rachelNodes;
    public StoryNode[] chloeNodes;
    public int totalRounds = 2;
    private int currentRound = 0;
    private List<int> roundResults = new List<int>();
    private static int clickedButtonCount = 0;
    private bool hasSelect = false;

    private IEnumerator VotingRoutine()
    {
        while (currentRound < totalRounds)
        {
            ResetTimer();
            while (time > 0)
            {
                timedownText.text = time.ToString("f1");
                yield return new WaitForSeconds(1);
                time--;
            }
            
            photonView.RPC("CheckEndOfRound", RpcTarget.All);
            yield return new WaitForSeconds(5);
        }
    }

    [PunRPC]
    private void ResetTimer()
    {
        //time = 10;
        timedownText.text = time.ToString("f1");
    }

    private int GetRandomButtonIndex()
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].interactable && i != GameDataManager.selectCharacter)
            {
                availableIndices.Add(i);
            }
        }
        if (availableIndices.Count > 0)
        {
            int index = Random.Range(0, availableIndices.Count);
            return availableIndices[index];
        }
        return -1;
    }

    private void Start()
    {
        buttons[GameDataManager.selectCharacter].interactable = false;
        ResetTimer();
        UpdateRoundTitle();
        StartCoroutine(VotingRoutine());
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex == -1 || hasSelect || (PhotonNetwork.IsMasterClient && clickedButtonCount >= buttons.Length))
        {
            return;
        }

        GameDataManager.voteCharacter = buttonIndex;
        photonView.RPC("IncreaseClickedButtonCount", RpcTarget.All);
        if (buttonIndex >= 0)
        {
            buttons[buttonIndex].interactable = false;
        }
        hasSelect = true;
        CheckAllPlayersClicked();
    }

    private void CheckAllPlayersClicked()
    {
        if (clickedButtonCount >= buttons.Length)
        {
            photonView.RPC("PrepareNextRound", RpcTarget.All);
        }
    }

    [PunRPC]
    private void CheckEndOfRound()
    {
        if (clickedButtonCount < buttons.Length - 1)
        {
            SimulateVotesForNonVoters();
        }

        DetermineRoundResult();

        if (currentRound + 1 < totalRounds)
        {
            photonView.RPC("PrepareNextRound", RpcTarget.All);
        }
        else
        {
            photonView.RPC("ShowEndUI", RpcTarget.All);
        }
    }

    private void SimulateVotesForNonVoters()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].interactable && i != GameDataManager.selectCharacter)
            {
                OnButtonClick(GetRandomButtonIndex());
            }
        }
    }

    [PunRPC]
    private void IncreaseClickedButtonCount()
    {
        clickedButtonCount++;
    }

    [PunRPC]
    private void PrepareNextRound()
    {
        currentRound++;
        ResetVotingState();
        if (currentRound < totalRounds)
        {
            photonView.RPC("ResetTimer", RpcTarget.All);
            UpdateRoundTitle();
        }
    }

    private void ResetVotingState()
    {
        clickedButtonCount = 0;
        hasSelect = false;
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
        buttons[GameDataManager.selectCharacter].interactable = false;
    }

    private void DetermineRoundResult()
    {
        int[] votes = new int[buttons.Length];
        foreach (var vote in roundResults)
        {
            if (vote >= 0 && vote < buttons.Length)
            {
                votes[vote]++;
            }
        }

        // Check if any player got exactly two votes
        int votedOutIndex = -1; // -1 means no one was voted out with exactly two votes
        for (int i = 0; i < votes.Length; i++)
        {
            if (votes[i] == 2)
            {
                votedOutIndex = i;
                break;
            }
        }

        // If no one has two votes and each has one vote, we consider no one voted out
        if (votedOutIndex == -1 && votes.All(v => v == 1))
        {
            votedOutIndex = -1; // No one voted out
        }

        roundResults.Add(votedOutIndex);
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
        bool maxVotedOutTwice = roundResults.SequenceEqual(new int[] {0, 0});
        bool rachelVotedOutTwice = roundResults.SequenceEqual(new int[] {1, 1});
        bool chloeVotedOutTwice = roundResults.SequenceEqual(new int[] {2, 2});
        bool equalVote = roundResults.SequenceEqual(new int[] {-1, -1});
        bool maxRachelVotedOut = roundResults.Contains(0) && roundResults.Contains(1);
        bool maxChloeVotedOut = roundResults.Contains(0) && roundResults.Contains(2);
        bool chloeRachelVotedOut = roundResults.Contains(1) && roundResults.Contains(2);
        bool equalMaxVote = roundResults.Contains(0) && roundResults.Contains(-1);
        bool equalChloeVote = roundResults.Contains(2) && roundResults.Contains(-1);
        bool equalRachelVote = roundResults.Contains(1) && roundResults.Contains(-1);

        if (maxVotedOutTwice)
        {
            SetActiveNodes(maxNodes, 1);
            SetActiveNodes(chloeNodes, 1);
            SetActiveNodes(rachelNodes, 1);
        }
        if(rachelVotedOutTwice)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes,1);
            SetActiveNodes(rachelNodes,3);
        }
        if(chloeVotedOutTwice)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes,2);
            SetActiveNodes(rachelNodes,1);
        }
        if(maxRachelVotedOut)
        {
            SetActiveNodes(maxNodes, 2);
            SetActiveNodes(chloeNodes,1);
            SetActiveNodes(rachelNodes,2);
        }
        if(maxChloeVotedOut)
        {
            SetActiveNodes(maxNodes, 2);
            SetActiveNodes(chloeNodes,2);
            SetActiveNodes(rachelNodes,1);
        }
        if(chloeRachelVotedOut)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes,2);
            SetActiveNodes(rachelNodes,2);
        }
        if (equalMaxVote)
        {
            SetActiveNodes(maxNodes, 1);
            SetActiveNodes(chloeNodes, 1);
            SetActiveNodes(rachelNodes, 1);
        }
        if (equalChloeVote)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes, 2);
            SetActiveNodes(rachelNodes, 1);
        }
        if (equalRachelVote)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes, 1);
            SetActiveNodes(rachelNodes, 2);
        }
        if(equalVote)
        {
            SetActiveNodes(maxNodes, 4);
            SetActiveNodes(chloeNodes,4);
            SetActiveNodes(rachelNodes,4);
        }

        LoadActiveTextForCharacter();

        endUI.SetActive(true);
    }

    private void LoadActiveTextForCharacter()
    {
        List<StoryNode> activeNodes = LoadActiveCharacterStoryNodes(new List<StoryNode>(GetNodesArray(GameDataManager.selectCharacter)));
        endingText.text = CombineStoryText(activeNodes);
    }

    private StoryNode[] GetNodesArray(int characterIndex)
    {
        switch (characterIndex)
        {
            case 0:
                return maxNodes;
            case 1:
                return rachelNodes;
            case 2:
                return chloeNodes;
            default:
                return null;
        }
    }

    public List<StoryNode> LoadActiveCharacterStoryNodes(List<StoryNode> storyNodes)
    {
        List<StoryNode> activeStoryNodes = storyNodes.FindAll(node => node.isActive);
        return activeStoryNodes;
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

    public void SetActiveNodes(StoryNode[] nodes, int nodeID)
    {
        foreach (var node in nodes) {
            if(node.nodeId == nodeID) {
                node.isActive = true;
            }
        }
    }

    private void UpdateRoundTitle()
    {
        if (currentRound < titleText.Length)
        {
            roundTitle.text = titleText[currentRound];
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
