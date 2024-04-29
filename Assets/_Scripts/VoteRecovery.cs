using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class VoteRecovery : MonoBehaviourPunCallbacks
{
    public Vote vote;

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // set vote.currentRound according to room properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.CURRENT_ROUND_KEY, out object value))
        {
            if (int.TryParse(value.ToString(), out int currentRound))
            {
                vote.currentRound = currentRound;
            }
        }

        vote.UpdateRoundTitle();

        // load votedOutMap back to vote
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.VOTE_OUT_MAP_KEY, out value))
        {
            vote.votedOutMap = (Dictionary<int, int>)value;
        }

        vote.roundResults.Clear();

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.ROUND_ONE_KEY, out value))
        {   // we have finished round one
            if (int.TryParse(value.ToString(), out int round1))
            {
            vote.roundResults.Add(round1);
            }
        }
        else
        {   // we are still voting for round one
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.ROUND_TWO_KEY, out value))
        { 
            // we have finished round two
            if (int.TryParse(value.ToString(), out int round2))
            {
            vote.roundResults.Add(round2);
            }
        }
        else
        {   // we are still voting for round two
            vote.UpdateRoundTitle();
            return;
        }

        // we have finished both rounds
        vote.ShowEndUI();
    }
}
