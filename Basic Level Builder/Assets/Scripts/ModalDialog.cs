using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ModalDialog : MonoBehaviour
{
  public bool m_OverrideSize = false;
  public Vector2 m_SizeToOverrideWith = new Vector2(200, 200);
  protected RectTransform m_RectTransform;
  protected ModalDialogMaster m_Master;


  public virtual void Setup(ModalDialogMaster master, Vector2 rectPoint, string[] strings = null)
  {
    m_RectTransform = GetComponent<RectTransform>();
    m_Master = master;
    var size = m_OverrideSize ? m_SizeToOverrideWith : new Vector2(-1, -1);

    StringsSetup(strings);

    m_Master.Add(m_RectTransform, rectPoint, size);
  }


  public virtual void StringsSetup(string[] strings = null)
  {

  }


  public virtual void Open()
  {

  }


  public virtual void Close()
  {
    if (!enabled)
      return;

    enabled = false;
    m_Master.Remove();
  }
}
