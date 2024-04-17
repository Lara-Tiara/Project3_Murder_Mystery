using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICluesManager
{
    List<StoryClue> GetSharedClues();
    void AddSharedClue(StoryClue clue);
}