using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ClueListRecovery : MonoBehaviourPunCallbacks
{
    public ReadStory readStory;

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
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CluesManager.SHARED_CLUES_KEY, out object value))
        {
            string json = value as string;
            if (!string.IsNullOrEmpty(json))
            {
                SerializableCluesList cluesList = JsonUtility.FromJson<SerializableCluesList>(json);
                foreach ( var clueButton in readStory.clueButtons)
                {
                    TextMeshProUGUI clueButtonText = clueButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    string keyword = clueButtonText.text;
                    if (cluesList.Clues.FirstOrDefault(clue => clue.clueKeyWord == keyword) != null)
                    {
                        clueButton.interactable =  false;
                    }
                }
            }
        }
    }
}
