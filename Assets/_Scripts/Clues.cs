using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClueList", menuName = "Story/ClueList")]
public class Clues : ScriptableObject
{
    public List<Clue> clues;
}

[System.Serializable]
public class Clue
{
    public string clueKeyWord;
    public string clueText;
    public bool hasDestroyed;
}

