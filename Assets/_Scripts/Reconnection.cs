using System;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Reconnection :  PersistentSingletonCallback<Reconnection>, IConnectionCallbacks
{
    private LoadBalancingClient loadBalancingClient;
    private AppSettings appSettings;
    public bool shouldReconnected = true;
    private bool reconnecting = false;

    public void Start()
    {
        loadBalancingClient = PhotonNetwork.NetworkingClient;
        appSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
        loadBalancingClient.AddCallbackTarget(this);
    }

    public void OnDestroy()
    {
        if(loadBalancingClient == null)
        {
            return;
        }
        loadBalancingClient.RemoveCallbackTarget(this);
    }

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
        if (!shouldReconnected)
        {
            return;
        }
        if (this.CanRecoverFromDisconnect(cause))
        {
            Debug.Log("Attempting recovery.");
            this.Recover();
        }
        else
        {
            Debug.Log("Could not attempt from disconnection");
        }
    }

    private bool CanRecoverFromDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // the list here may be non exhaustive and is subject to review
            case DisconnectCause.Exception:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        return false;
    }

    private void Recover()
    {
        if (!loadBalancingClient.ReconnectAndRejoin())
        {
            Debug.LogError("ReconnectAndRejoin failed, trying Reconnect");
            if (!loadBalancingClient.ReconnectToMaster())
            {
                Debug.LogError("Reconnect failed, trying ConnectUsingSettings");
                if (!loadBalancingClient.ConnectUsingSettings(appSettings))
                {
                    Debug.LogError("ConnectUsingSettings failed");
                }
                else
                {
                    Debug.Log("ConnectUsingSettings success");
                }
            }
            else
            {
                Debug.Log("Reconnect to master success");
            }
        }
        else
        {
            Debug.Log("ReconnectAndRejoin success");
            reconnecting = true;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string reason)
    {
        if( reconnecting)
        {
            Debug.LogError("Reconnection failed: " + reason);
        }
    }

    public override void OnJoinedRoom()
    {
        reconnecting = false;
        Debug.Log("Reconnected - OnJoinRoom");
    }

    #region Unused Methods

    void IConnectionCallbacks.OnConnected()
    {
    }

    void IConnectionCallbacks.OnConnectedToMaster()
    {
    }

    void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    #endregion
}