using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DisconnectCanvas : MonoBehaviourPunCallbacks
{
    public GameObject disconnectMask;

    public void Start()
    {
        disconnectMask.SetActive(false);
    }

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
        disconnectMask.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        disconnectMask.SetActive(true);
    }
}
