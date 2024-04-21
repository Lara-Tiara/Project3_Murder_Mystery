using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public GameObject palyerNamePrefab;
    public Transform gridLayout;
    private Player newPlayer;
    
    public void SetPlayerInfo(Player player)
    {
        newPlayer = player;
        GameObject newPlayerButton = Instantiate(palyerNamePrefab, gridLayout);
        TextMeshProUGUI playerName = newPlayerButton.GetComponentInChildren<TextMeshProUGUI>();
        playerName.text = newPlayer.NickName;
    }
}
