using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ScriptCanvasController : MonoBehaviour
{
    public ScrollRect script;
    public TextMeshProUGUI content;
    public TextAsset maxTextAsset;
    public TextAsset rachelTextAsset;
    public TextAsset chloeTextAsset;
    private string currentStory;

    // Start is called before the first frame update
    void Start()
    {
        switch (GameDataManager.selectCharacter)
        {
            case 0:
                currentStory = maxTextAsset.text;
                break;
            case 1:
                currentStory = rachelTextAsset.text;
                break;
            case 2:
                currentStory = chloeTextAsset.text;
                break;
            default:
                currentStory = "No story available.";
                break;
        }
        content.text = currentStory;

        script.verticalNormalizedPosition = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
