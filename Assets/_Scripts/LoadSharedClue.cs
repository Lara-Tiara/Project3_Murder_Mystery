using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class LoadSharedClue : MonoBehaviourPunCallbacks
{
    public Transform gridLayout;
    public GameObject clueButtonPrefab;

    private void Start()
    {
        SharedCluesUpdate(CluesManager.Instance.GetSharedClues());
    }

    public void SharedCluesUpdate(List<StoryClue> clues)
    {
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        foreach (StoryClue clue in clues)
        {
            GameObject newClueButton = Instantiate(clueButtonPrefab, gridLayout);
            TextMeshProUGUI clueText = newClueButton.GetComponentInChildren<TextMeshProUGUI>();
            clueText.text = clue.clueText;
        }
    }
}

