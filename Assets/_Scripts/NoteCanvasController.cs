using UnityEngine;
using TMPro;

public class NoteCanvasController : MonoBehaviour
{
    public TMP_InputField noteInputField;
    private NoteDataManager noteDataManager;

    private void Start()
    {
        noteDataManager = FindObjectOfType<NoteDataManager>();

        if (noteDataManager != null && noteInputField != null)
        {
            string note = noteDataManager.LoadNote();
            noteInputField.text = note;

            noteInputField.onValueChanged.AddListener(delegate { SaveNote(); });
        }
    }

    private void SaveNote()
    {
        if (noteDataManager != null)
        {
            noteDataManager.SaveNote(noteInputField.text);
        }
    }
}
