using UnityEngine;

public abstract class UiTab : MonoBehaviour
{
  public GameObject m_TabButton;

  public abstract void InitLoad(string fullFilePath);

  public virtual void OpenTab()
  {

  }
}
