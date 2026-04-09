using UnityEngine;
using UnityEngine.EventSystems;

public class InputFieldUiHelper : MonoBehaviour
{

  [SerializeField]
  private TMPro.TMP_InputField m_InputField;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_ShownText;

  public void OnEnable()
  {
    var caret = m_InputField.GetComponentInChildren<TMPro.TMP_SelectionCaret>(true);
    if (caret != null)
      caret.raycastTarget = false;
  }

  public void OnInputFieldDeselect()
  {
    // Move text position back to the left
    m_InputField.textComponent.rectTransform.localPosition = Vector3.zero;
    m_InputField.caretPosition = 0;
    // Re-disable mouse event blocking
    m_InputField.GetComponentInChildren<TMPro.TMP_SelectionCaret>(true).raycastTarget = false;

    m_InputField.textComponent.overflowMode = TMPro.TextOverflowModes.Ellipsis;
    m_ShownText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
  }

  public void OnInputFieldSelect()
  {
    m_InputField.textComponent.overflowMode = TMPro.TextOverflowModes.Masking;
    m_ShownText.overflowMode = TMPro.TextOverflowModes.Masking;
  }

  public void StartEdit()
  {
    m_InputField.GetComponentInChildren<TMPro.TMP_SelectionCaret>(true).raycastTarget = true;
    m_InputField.ActivateInputField();
  }

  public void OnInputFieldValueChanged(string value)
  {
    m_ShownText.text = value + ".";
  }

  public void EndEdit()
  {
    // Deletect all ui so that the input field will be deselected and update its text
    var eventSystem = EventSystem.current;
    if (!eventSystem.alreadySelecting) eventSystem.SetSelectedGameObject(null);
  }

  public void SetText(string txt)
  {
    m_ShownText.text = txt + ".";
    m_InputField.text = txt;
  }

  public string GetText()
  {
    return m_InputField.text;
  }
}
