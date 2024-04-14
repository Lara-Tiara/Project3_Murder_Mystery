using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class Chat : MonoBehaviourPunCallbacks
{
    public float time;
    public TextMeshProUGUI timedownText;
    public int nextScene;
    public InputField iptMessage;
    public TextMeshProUGUI chatText;
    public ScrollRect scrollRect;
    public GameObject noteCanvas;
    public Button sendButton;
    public Transform gridLayout;
    public GameObject clueButtonPrefab;
    private EventSystem eventSystem;
    private string[] characterNames = { "Max", "Rachel", "Chloe" };

    private void Start()
    {
        eventSystem = EventSystem.current;
        SetFocusOnInputField();
        SharedCluesUpdate(CluesManager.Instance.GetSharedClues());
    }

    private void Update()
    {
        time -= Time.deltaTime;
        timedownText.text = time.ToString("f1");
        if (time <= 0)
        {
            SceneManager.LoadScene(nextScene);
        }
        sendButton.onClick.AddListener(SendMessageButtonClicked);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (iptMessage.text.Length > 0)
            {
                SendMessage();
            }
        }

        if (!iptMessage.isFocused && !noteCanvas.activeSelf)
        {
            SetFocusOnInputField();
        }
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

    private void SendMessageButtonClicked()
    {
        if (iptMessage.text.Length > 0)
        {
            SendMessage();
        }
    }

    public void SendMessage()
    {
        string message = iptMessage.text;
        if (!string.IsNullOrEmpty(message))
        {
            string characterName = characterNames[GameDataManager.selectCharacter];

            photonView.RPC("ReceiveMessage", RpcTarget.All, characterName, message);
            iptMessage.text = "";

            SetFocusOnInputField();
        }
    }

    [PunRPC]
    private void ReceiveMessage(string sender, string message)
    {
        string characterName = characterNames[GameDataManager.selectCharacter];
        string formattedMessage = sender == characterName ? 
        $"<align=right>{sender}: {message}</align>\n" : 
        $"<align=left>{sender}: {message}</align>\n";
        chatText.text += formattedMessage;

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void SetFocusOnInputField()
    {
        eventSystem.SetSelectedGameObject(iptMessage.gameObject, null);
        iptMessage.OnPointerClick(new PointerEventData(eventSystem));
    }

    private void OnDestroy()
    {
        SaveChatLog();
    }

    private void SaveChatLog()
    {
        string directory = Application.dataPath + "/ChatLogs";
        string fileName = "ChatLog_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, fileName);

        string formattedText = chatText.text.Replace("<align=left>", "")
                                            .Replace("</align>", "")
                                            .Replace("<align=right>", "");

        File.WriteAllText(filePath, formattedText);

        Debug.Log("Chat log saved to " + filePath);
    }
}
