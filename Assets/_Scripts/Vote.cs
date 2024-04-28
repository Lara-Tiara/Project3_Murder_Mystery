using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System;
using Photon.Realtime;

public class Vote : MonoBehaviourPunCallbacks
{
    public const string ROUND_ONE_KEY = "Round One";
    public const string ROUND_TWO_KEY = "Round Two";
    public TextMeshProUGUI timedownText;
    public float time;
    public const float MAXTIME = 10;
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
    public List<int> roundResults = new List<int>();
    private static int clickedButtonCount = 0;
    private bool hasSelect = false;
    private int votedOutIndex;
    private Dictionary<int, int> votedOutMap = new Dictionary<int, int>();
    public string masterClient;

    public IEnumerator VotingRoutine()
    {
        while (currentRound < totalRounds)
        {
            while (time >= 0)
            {
                photonView.RPC("SetTimeText", RpcTarget.All, time.ToString("f1"));
                yield return new WaitForSeconds(1);
                time--;
            }

            yield return new WaitForSeconds(5);
            CheckEndOfRound();
            ResetTimer();
        }
    }

    [PunRPC]
    private void SetTimeText(string text)
    {
        timedownText.text = text;
    }

    [PunRPC]
    private void ResetTimer()
    {
        time = MAXTIME;
        timedownText.text = time.ToString("f1");
    }

    private int GetRandomButtonIndex(int unVoterID)
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].interactable && i != unVoterID)
            {
                availableIndices.Add(i);
            }
        }
        if (availableIndices.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, availableIndices.Count);
            return availableIndices[index];
        }
        return 0;
    }

    private void Start()
    {
        buttons[GameDataManager.selectCharacter].interactable = false;
        ResetTimer();
        UpdateRoundTitle();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(VotingRoutine());
        }
        masterClient = PhotonNetwork.MasterClient.UserId;
        Debug.Log(masterClient);
        Debug.Log(PhotonNetwork.MasterClient);
        Debug.Log(PhotonNetwork.MasterClient.GetHashCode());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        StopAllCoroutines();
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex == -1 || hasSelect || (PhotonNetwork.IsMasterClient && clickedButtonCount >= buttons.Length))
        {
            return;
        }

        GameDataManager.voteCharacter = buttonIndex;
        photonView.RPC("PlayerVote", RpcTarget.All, GameDataManager.selectCharacter, buttonIndex);
        if (buttonIndex >= 0)
        {
            buttons[buttonIndex].interactable = false;
        }
        hasSelect = true;
    }

    [PunRPC]
    private void PlayerVote(int voterID, int buttonIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        if (votedOutMap == null)
        {
            votedOutMap = new Dictionary<int, int>();
        }

        if (votedOutMap.TryAdd(voterID, buttonIndex) == false)
        {
            votedOutMap[voterID] = buttonIndex;
        }
    }

    private void CheckEndOfRound()
    {
        DetermineRoundResult();

        if (currentRound + 1 < totalRounds)
        {
            photonView.RPC("PrepareNextRound", RpcTarget.All);
            
        }
        else
        {
            StopAllCoroutines();
            if (roundResults.Count >= totalRounds){
                photonView.RPC("ShowEndUI", RpcTarget.All);
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
            ResetTimer();
            UpdateRoundTitle();
        }
    }

    private void ResetVotingState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            votedOutMap = new Dictionary<int, int>();
        }

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
        Array.Clear(votes, 0, votes.Length);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (votedOutMap.TryGetValue(i, out int result))
            {
                votes[result]++;
            } else {
                votes[GetRandomButtonIndex(i)]++;
            }
        }

        for (int i = 0; i < votes.Length; i++)
        {
            if (votes[i] == 2)
            {
                votedOutIndex = i;
                break;
            }
        }
        
        if (votes.All(v => v == 1))
        {
            votedOutIndex = -1; 
        }

        roundResults.Add(votedOutIndex);
        if (roundResults.Count == 1)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { ROUND_ONE_KEY, votedOutIndex } });
        }
        else if (roundResults.Count == 2)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { ROUND_TWO_KEY, votedOutIndex } });
        }
    }

    [PunRPC]
    public void ShowEndUI()
    {
        roundResults.Clear();
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROUND_ONE_KEY, out object data1))
        {
            if (int.TryParse(data1.ToString(), out int round1))
            {
                roundResults.Add(round1);
            }
            else
            {
            return;
            }
        }
        else
        {
            return;
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROUND_TWO_KEY, out object data2))
        {
            if (int.TryParse(data2.ToString(), out int round2))
            {
                roundResults.Add(round2);
            }
            else
            {
            return;
            }
        }
        else
        {
            return;
        }
        
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
