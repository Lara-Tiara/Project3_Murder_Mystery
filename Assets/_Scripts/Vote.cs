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
    public const string CURRENT_ROUND_KEY = "CurrentRound";

    public const string VOTE_OUT_MAP_KEY = "VotedOutMap";

    [Header("UI Elements")]
    public TextMeshProUGUI timedownText;
    public float time;
    public const float MAXTIME = 10;
    public TextMeshProUGUI roundTitle;
    public string[] titleText;
    public GameObject endUI;
    public TextMeshProUGUI endingText;

    [Header("Vote buttons")]
    public Button[] buttons;

    [Header("Story Nodes")]
    public StoryNode[] maxNodes;
    public StoryNode[] rachelNodes;
    public StoryNode[] chloeNodes;

    [Header("Vote Routine settings")]
    public int totalRounds = 2;
    [HideInInspector] public int currentRound = 0;
    [HideInInspector] public List<int> roundResults = new List<int>();
    private static int clickedButtonCount = 0;
    private bool hasSelect = false;
    private int votedOutIndex;
    public Dictionary<int, int> votedOutMap = new Dictionary<int, int>();
    public string masterClient;

    /// <summary>
    /// Voting routine is a coroutine that will run for the duration of the voting phase.
    /// To keep everything synchronized, this coroutine should be run by the master client only.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Set the time text. Called by VotingRoutine.
    /// </summary>
    /// <param name="text"></param>
    [PunRPC]
    private void SetTimeText(string text)
    {
        time = float.Parse(text); // do a backup in case of master client disconnection
        timedownText.text = text;
    }

    /// <summary>
    /// Reset the timer. Called by VotingRoutine.
    /// </summary>
    /// <returns></returns>
    private void ResetTimer()
    {
        time = MAXTIME;
        timedownText.text = time.ToString("f1");
    }

    /// <summary>
    /// Choose a random button index for the one who is not voting.
    /// </summary>
    /// <param name="unVoterID"> id for whoever is not voting. </param>
    /// <returns></returns>
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
        buttons[GameDataManager.selectCharacter].interactable = false; // a player cannot vote for themselves
        ResetTimer();
        UpdateRoundTitle();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(VotingRoutine());
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // if local becomes master client, start the voting routine
        if (newMasterClient.UserId == PhotonNetwork.LocalPlayer.UserId)
        {
            StartCoroutine(VotingRoutine());
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Called when a button is clicked. this is monted to the vote button's onClick event.
    /// This method will notify all other players of the vote.
    /// </summary>
    /// <param name="buttonIndex"></param>
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

    /// <summary>
    ///  Called when a player votes. This is called by OnButtonClick.
    /// </summary>
    /// <param name="voterID"></param>
    /// <param name="buttonIndex"></param>
    [PunRPC]
    private void PlayerVote(int voterID, int buttonIndex)
    {
        if (votedOutMap == null)
        {
            votedOutMap = new Dictionary<int, int>();
        }

        if (votedOutMap.TryAdd(voterID, buttonIndex) == false)
        {
            votedOutMap[voterID] = buttonIndex;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // update the current custom properties
            var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            roomProperties[VOTE_OUT_MAP_KEY] = votedOutMap;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    /// <summary>
    /// Determine the round result and check if the game is over.
    /// This funciton can only get called by the master client.
    /// </summary>
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
            if (roundResults.Count >= totalRounds)
            {
                photonView.RPC("ShowEndUI", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void IncreaseClickedButtonCount() // this seems usless
    {
        clickedButtonCount++;
    }

    /// <summary>
    /// Prepare for the next round. Called by CheckEndOfRound.
    /// Every player will be able to execute this function.
    /// </summary>
    [PunRPC]
    private void PrepareNextRound()
    {
        currentRound++;

        // set the round info to the room properties
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomProperties[CURRENT_ROUND_KEY] = currentRound;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        ResetVotingState();
        if (currentRound < totalRounds)
        {
            ResetTimer();
            UpdateRoundTitle();
        }
    }

    /// <summary>
    /// Reset the voting state for the next round.
    /// Every player will be able to execute this function.
    /// </summary>
    private void ResetVotingState()
    {
        votedOutMap = new Dictionary<int, int>();
        clickedButtonCount = 0;
        hasSelect = false;
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
        buttons[GameDataManager.selectCharacter].interactable = false;
    }

    /// <summary>
    /// Determine the round result.
    /// </summary>
    private void DetermineRoundResult()
    {
        int[] votes = new int[buttons.Length];
        Array.Clear(votes, 0, votes.Length);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (votedOutMap.TryGetValue(i, out int result))
            {
                votes[result]++;
            }
            else
            {
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
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (roundResults.Count == 1)
        {
            roomProperties[ROUND_ONE_KEY] = votedOutIndex;
        }
        else if (roundResults.Count == 2)
        {
            roomProperties[ROUND_TWO_KEY] = votedOutIndex;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
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
                Debug.LogError("Failed to parse round 1 result.");
                return;
            }
        }
        else
        {
            Debug.LogError("Failed to get round 1 result.");
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
                Debug.LogError("Failed to parse round 2 result.");
                return;
            }
        }
        else
        {
            Debug.LogError("Failed to get round 2 result.");
            return;
        }

        bool maxVotedOutTwice = roundResults.SequenceEqual(new int[] { 0, 0 });
        bool rachelVotedOutTwice = roundResults.SequenceEqual(new int[] { 1, 1 });
        bool chloeVotedOutTwice = roundResults.SequenceEqual(new int[] { 2, 2 });
        bool equalVote = roundResults.SequenceEqual(new int[] { -1, -1 });
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
        if (rachelVotedOutTwice)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes, 1);
            SetActiveNodes(rachelNodes, 3);
        }
        if (chloeVotedOutTwice)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes, 2);
            SetActiveNodes(rachelNodes, 1);
        }
        if (maxRachelVotedOut)
        {
            SetActiveNodes(maxNodes, 2);
            SetActiveNodes(chloeNodes, 1);
            SetActiveNodes(rachelNodes, 2);
        }
        if (maxChloeVotedOut)
        {
            SetActiveNodes(maxNodes, 2);
            SetActiveNodes(chloeNodes, 2);
            SetActiveNodes(rachelNodes, 1);
        }
        if (chloeRachelVotedOut)
        {
            SetActiveNodes(maxNodes, 3);
            SetActiveNodes(chloeNodes, 2);
            SetActiveNodes(rachelNodes, 2);
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
        if (equalVote)
        {
            SetActiveNodes(maxNodes, 4);
            SetActiveNodes(chloeNodes, 4);
            SetActiveNodes(rachelNodes, 4);
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
        foreach (var node in nodes)
        {
            if (node.nodeId == nodeID)
            {
                node.isActive = true;
            }
        }
    }

    public void UpdateRoundTitle()
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
