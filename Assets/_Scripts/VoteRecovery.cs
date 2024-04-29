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
        Debug.Log("OnJoinedRoom called in VoteRecovery.");
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.CURRENT_ROUND_KEY, out object value))
        {
            if (int.TryParse(value.ToString(), out int currentRound))
            {
                vote.currentRound = currentRound;
            }
        }

        vote.UpdateRoundTitle();

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.VOTE_OUT_MAP_KEY, out value))
        {
            vote.votedOutMap = (Dictionary<int, int>)value;
        }

        vote.roundResults.Clear();

        Debug.Log("Now let's try to show the end UI.");
        vote.ShowEndUI();
    }
}
