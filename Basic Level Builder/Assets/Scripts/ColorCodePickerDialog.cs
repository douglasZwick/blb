/***************************************************
File:           ColorCodePickerDialog.cs
Authors:        Doug Zwick, Christopher Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Code for the color wheel of the color code modal.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class ColorCodePickerDialog : ModalDialog
{
  public float m_OpenDuration = 0.25f;

  //ColorCode m_ColorCode;
  //TileGrid.Element m_Element;

  //Flag to check if input is accepted for the wheel.
  bool m_AcceptingInput = false;

  /************************************************************************************/


  private void Update()
  {
    if (!m_AcceptingInput)
      return;

    HandleKeyboardInput();
  }

  void HandleKeyboardInput()
  {
    if      (Input.GetKeyDown(KeyCode.A))
      SetAndClose(ColorCode.KeyCodes[KeyCode.A]);
    else if (Input.GetKeyDown(KeyCode.B))
      SetAndClose(ColorCode.KeyCodes[KeyCode.B]);
    else if (Input.GetKeyDown(KeyCode.C))
      SetAndClose(ColorCode.KeyCodes[KeyCode.C]);
    else if (Input.GetKeyDown(KeyCode.D))
      SetAndClose(ColorCode.KeyCodes[KeyCode.D]);
    else if (Input.GetKeyDown(KeyCode.E))
      SetAndClose(ColorCode.KeyCodes[KeyCode.E]);
    else if (Input.GetKeyDown(KeyCode.F))
      SetAndClose(ColorCode.KeyCodes[KeyCode.F]);
    else if (Input.GetKeyDown(KeyCode.G))
      SetAndClose(ColorCode.KeyCodes[KeyCode.G]);
    else if (Input.GetKeyDown(KeyCode.H))
      SetAndClose(ColorCode.KeyCodes[KeyCode.H]);
  }


  //public void Setup(ToolEvent te)
  //{
  //  //var tileGameObject = te.Element.m_GameObject;
  //  //m_ColorCode = tileGameObject.GetComponent<ColorCode>();
  //  //m_Element = te.Element;

  //  Setup();

  //  var worldPosition = te.TileWorldPosition;
  //  var screenPoint = te.EventCamera.WorldToScreenPoint(worldPosition);
  //  m_RectTransform.position = m_Master.ScreenPointToRectPoint(screenPoint);

  //  Open();
  //}


  //public void Setup(Vector2 rectPoint)
  //{
  //  Setup();

  //  m_RectTransform.position = rectPoint;

  //  Open();
  //}


  public override void Open()
  {
    base.Open();

    m_AcceptingInput = true;

    m_RectTransform.localScale = Vector3.zero;
    ActionMaster.Actions.Scale(gameObject, Vector3.one, m_OpenDuration, new Ease(Ease.Back.Out));
    ActionMaster.Actions.Rotate2DRT(gameObject, 360, m_OpenDuration, new Ease(Ease.Back.Out));
  }


  public override void Close()
  {
    base.Close();

    m_AcceptingInput = false;

    var closeSeq = ActionMaster.Actions.Sequence();
    var closeGrp = closeSeq.Group();
    closeGrp.Scale(gameObject, Vector3.zero, m_OpenDuration, new Ease(Ease.Quad.In));
    closeGrp.Rotate2DRT(gameObject, 360, m_OpenDuration, new Ease(Ease.Quad.In));
    closeSeq.Call(Destroy, gameObject);

    //Disconnect all assigned tiles from responding to future color wheels.
    //if (OnColorCodeFinalized != null)
    //  OnColorCodeFinalized();
  }


  void Destroy()
  {
    Destroy(gameObject);
  }


  public void Set(TileColor tileColor)
  {
    if (!m_AcceptingInput)
      return;

    SetHelper(tileColor);
  }

  
  public void SetAndClose(TileColor tileColor)
  {
    if (!m_AcceptingInput)
      return;

    SetHelper(tileColor);
    Close();
  }


  public void SetHelper(TileColor tileColor)
  {
    var deltas = OperationSystem.s_CurrentOperation.m_Deltas;

    foreach (var delta in deltas)
    {
      var colorCode = delta.Tile.GetComponent<ColorCode>();
      colorCode.Set(tileColor);
      delta.NewState.Color = tileColor;
    }

    //if (OnColorCodeSet != null)
    //  OnColorCodeSet(tileColor);

    //m_Element.m_TileColor = tileColor;
  }
}
