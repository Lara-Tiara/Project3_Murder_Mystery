using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class LoadSharedClue : MonoBehaviourPunCallbacks
{
    public Transform gridLayout;
    public GameObject clueButtonPrefab;
    private ICluesManager cluesManager;

    private void Start()
    {
        cluesManager = CluesManager.Instance;
        SharedCluesUpdate(CluesManager.Instance.GetSharedClues());
    }

    public void SetCluesManager(ICluesManager manager)
    {
        cluesManager = manager;
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

