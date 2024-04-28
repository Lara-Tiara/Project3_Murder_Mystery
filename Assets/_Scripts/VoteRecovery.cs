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
        Debug.Log(PhotonNetwork.LocalPlayer.UserId);
        if (PhotonNetwork.LocalPlayer.UserId == vote.masterClient)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            vote.StartCoroutine(vote.VotingRoutine());  
        }

        base.OnJoinedRoom();
        vote.roundResults.Clear();
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.ROUND_ONE_KEY, out object value))
        {
            if (int.TryParse(value.ToString(), out int round1) == false)
            {
                return;
            }
            vote.roundResults.Add(round1);
        }
        else
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Vote.ROUND_TWO_KEY, out value))
        {
            if (int.TryParse(value.ToString(), out int round2) == false)
            {
                return;
            }
            vote.roundResults.Add(round2);
        }
        else
        {
            return;
        }

        vote.ShowEndUI();
    }
}
