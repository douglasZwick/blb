using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionPickerDialog : ModalDialog
{
  public bool m_RightAllowed = true;
  public bool m_UpAllowed = true;
  public bool m_LeftAllowed = true;
  public bool m_DownAllowed = true;
  public float m_OpenDuration = 0.25f;
  public Vector3 m_InitialScale = new Vector3(0, 4, 1);

  bool m_AcceptingInput = false;

  void Update()
  {
    if (!m_AcceptingInput)
      return;

    HandleKeyboardInput();
  }


  void HandleKeyboardInput()
  {
    var horizontalInput = Input.GetAxis("Horizontal");
    var verticalInput = Input.GetAxis("Vertical");
    var r = horizontalInput > 0;
    var l = horizontalInput < 0;
    var u = verticalInput > 0;
    var d = verticalInput < 0;

    if      (r && !(u || d) && m_RightAllowed)
      SetAndClose(Direction.RIGHT);
    else if (u && !(r || l) && m_UpAllowed)
      SetAndClose(Direction.UP);
    else if (l && !(u || d) && m_LeftAllowed)
      SetAndClose(Direction.LEFT);
    else if (d && !(r || l) && m_DownAllowed)
      SetAndClose(Direction.DOWN);
  }


  public override void Open()
  {
    base.Open();

    m_AcceptingInput = true;

    m_RectTransform.localScale = m_InitialScale;
    ActionMaster.Actions.Scale(gameObject, Vector3.one, m_OpenDuration, new Ease(Ease.Back.Out));
  }


  public override void Close()
  {
    base.Close();

    m_AcceptingInput = false;

    var closeSeq = ActionMaster.Actions.Sequence();
    closeSeq.Scale(gameObject, Vector3.zero, m_OpenDuration, new Ease(Ease.Quad.In));
    closeSeq.Call(Destroy, gameObject);
  }


  void Destroy()
  {
    Destroy(gameObject);
  }


  public void Set(Direction direction)
  {
    if (!m_AcceptingInput)
      return;

    SetHelper(direction);
  }


  public void SetAndClose(Direction direction)
  {
    if (!m_AcceptingInput)
      return;

    SetHelper(direction);
    Close();
  }


  public void SetHelper(Direction direction)
  {
    var deltas = OperationSystem.s_CurrentOperation.m_Deltas;

    foreach (var delta in deltas)
    {
      var tileDirection = delta.Tile.GetComponent<TileDirection>();
      tileDirection.Set(direction);
      delta.NewState.Direction = direction;
    }
  }
}
