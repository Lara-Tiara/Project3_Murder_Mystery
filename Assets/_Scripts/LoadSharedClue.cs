using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class LoadSharedClue : MonoBehaviourPunCallbacks
{
    public Transform gridLayout;
    public GameObject clueButtonPrefab;
    private CluesManager cluesManager;

    private void Start()
    {
        cluesManager = CluesManager.Instance;
        SharedCluesUpdate(CluesManager.Instance.GetSharedClues());
    }


    public void SharedCluesUpdate(List<Clue> clues)

    {
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        foreach (Clue clue in clues)
        {
            GameObject newClueButton = Instantiate(clueButtonPrefab, gridLayout);
            TextMeshProUGUI clueText = newClueButton.GetComponentInChildren<TextMeshProUGUI>();
            clueText.text = clue.clueText;
        }
    }
}

