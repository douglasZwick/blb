using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;

public class SaveAsDialog : ModalDialog
{
  public static char[] s_InvalidFileNameChars = Path.GetInvalidFileNameChars();
  public static string s_InvalidCharsString;

  public TMP_InputField m_InputField;

  FileSystem m_FS;
  string m_CurrentValidName = string.Empty;
  bool m_FirstUpdate = true;


  private void Awake()
  {
    var printableInvalidChars = s_InvalidFileNameChars.Where(invalidChar =>
      !char.IsControl(invalidChar)).ToArray();
    s_InvalidCharsString = string.Join(" ", printableInvalidChars);

    m_FS = FindObjectOfType<FileSystem>();

    m_InputField.onSubmit.AddListener(OnSubmit);
  }


  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();

    // scaling effects
  }


  public override void Close()
  {
    base.Close();

    // scaling effects

    Destroy(gameObject);
  }


  void Update()
  {
    if (m_FirstUpdate)
    {
      m_FirstUpdate = false;
      m_InputField.ActivateInputField();
    }

    if (Input.GetButtonDown("Cancel"))
      Cancel();
  }


  public void Save()
  {
    var emptyName = m_CurrentValidName == string.Empty;
    var whiteSpaceName = string.IsNullOrWhiteSpace(m_CurrentValidName);

    if (emptyName || whiteSpaceName)
    {
      StatusBar.Print("<color=#ffff00>Please enter a valid file name.</color>");

      return;
    }

    Close();
    m_FS.SaveAs(m_CurrentValidName);
  }


  public void Cancel()
  {
    Close();
  }


  public void OnValueChanged(string value)
  {
    var invalidChars = s_InvalidFileNameChars;

    if (value.IndexOfAny(invalidChars) >= 0)
    {
      var message = $"<color=#ffff00>The string <b>{value}</b> contains " +
        $"one or more invalid characters: <b>{s_InvalidCharsString}</b></color>";
      StatusBar.Print(message);

      m_InputField.text = m_CurrentValidName;
    }
    else
    {
      m_CurrentValidName = value;
    }
  }


  void OnSubmit(string value)
  {
    // i'm pretty annoyed that i have to do this =|
    if (Input.GetButtonDown("Cancel"))
      return;

    Save();
  }


  private void OnDestroy()
  {
    m_InputField.onSubmit.RemoveListener(OnSubmit);
  }
}
