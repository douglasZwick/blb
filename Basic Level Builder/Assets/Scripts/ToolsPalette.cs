using UnityEngine;

public enum Tools
{
  BRUSH,
  SELECTOR,
  EYE_DROPPER,
  BOX,
  HAND,
  PATH,
}

public class ToolsPalette : MonoBehaviour
{
  /************************************************************************************/

  public BlbTool m_DefaultTool;

  BlbTool m_ActiveTool;

  private void Awake()
  {
    SetToolActive(m_DefaultTool);
  }

  public void OnLeftPointerDown(ToolEvent te)
  {
    m_ActiveTool?.LeftPointerDown(te);
  }

  public void OnRightPointerDown(ToolEvent te)
  {
    m_ActiveTool?.RightPointerDown(te);
  }

  public void OnLeftPointerUp(ToolEvent te)
  {
    m_ActiveTool?.LeftPointerUp(te);
  }

  public void OnRightPointerUp(ToolEvent te)
  {
    m_ActiveTool?.RightPointerUp(te);
  }

  public void OnBeginLeftDrag(ToolEvent te)
  {
    m_ActiveTool?.BeginLeftDrag(te);
  }

  public void OnBeginRightDrag(ToolEvent te)
  {
    m_ActiveTool?.BeginRightDrag(te);
  }

  public void OnLeftDrag(ToolEvent te)
  {
    m_ActiveTool?.LeftDrag(te);
  }

  public void OnRightDrag(ToolEvent te)
  {
    m_ActiveTool?.RightDrag(te);
  }

  public void OnEndLeftDrag(ToolEvent te)
  {
    m_ActiveTool?.EndLeftDrag(te);
  }

  public void OnEndRightDrag(ToolEvent te)
  {
    m_ActiveTool?.EndRightDrag(te);
  }

  public void UpdateLeftDrag(ToolEvent te)
  {
    if (m_ActiveTool.m_ToolID == Tools.HAND)
      return;

    OnLeftDrag(te);
  }

  public void UpdateRightDrag(ToolEvent te)
  {
    if (m_ActiveTool.m_ToolID == Tools.HAND)
      return;

    OnRightDrag(te);
  }


  /**
  * FUNCTION NAME: SetToolActive
  * DESCRIPTION  : Sets a new tool as active.
  * INPUTS       : _tool - Tool to set as active.
  * OUTPUTS      : None
  **/
  public void SetToolActive(BlbTool _tool)
  {
    if (_tool == m_ActiveTool)
      return;

    m_ActiveTool?.Deactivate();
    m_ActiveTool = _tool;
    m_ActiveTool.Activate();
  }

  /**
  * FUNCTION NAME: IsHandActive
  * DESCRIPTION  : Checks if the hand tool is active.
  * INPUTS       : None
  * OUTPUTS      : Bool - True/false on if the hand tool is the active tool.
  **/
  public bool IsHandActive()
  {
    if (m_ActiveTool?.m_ToolID == Tools.HAND)
      return true;

    return false;
  }

  /**
  * FUNCTION NAME: IsBoxActive
  * DESCRIPTION  : Checks if the box tool is active.
  * INPUTS       : None
  * OUTPUTS      : Bool - True/false on if the box tool is the active tool.
  **/
  public bool IsBoxActive()
  {
    if (m_ActiveTool?.m_ToolID == Tools.BOX)
      return true;

    return false;
  }

  /**
* FUNCTION NAME: IsSelectorActive
* DESCRIPTION  : Checks if the selector tool is active.
* INPUTS       : None
* OUTPUTS      : Bool - True/false on if the selector tool is the active tool.
**/
  public bool IsSelectorActive()
  {
    if (m_ActiveTool?.m_ToolID == Tools.SELECTOR)
      return true;

    return false;
  }
}
