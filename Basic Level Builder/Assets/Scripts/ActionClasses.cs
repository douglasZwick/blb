using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Action
{
  public string Name = "Base Action";
  public GameObject GO;
  public bool Started;
  public bool Completed;
  public bool Active = true;
  public float Duration;
  public float Remaining;
  public virtual float Completion
  {
    get
    {
      if (Duration == 0)
        return 1f;

      return 1f - Remaining / Duration;
    }
  }

  public virtual ActionState Update(float dt) { return ActionState.Completed; }
  public virtual void CancelOverride() { }

  public void Cancel()
  {
    CancelOverride();
    Active = false;
  }
}

public abstract class ActionSet : Action
{
  public List<Action> Actions = new List<Action>();
  public int Count { get { return Actions.Count; } }
  public bool Empty { get { return Actions.Count == 0; } }

  public uint CancelAllRegarding(GameObject go)
  {
    uint count = 0;

    foreach (var action in Actions)
    {
      if (action.GO == go)
      {
        action.Cancel();
        ++count;
      }
    }

    return count;
  }

  public uint CancelAllInTreeRegarding(GameObject go)
  {
    uint count = 0;

    foreach (var action in Actions)
    {
      if (action.GO == go)
      {
        action.Cancel();
        ++count;
      }

      if (action is ActionSet set)
      {
        count += set.CancelAllRegarding(go);
      }
    }

    return count;
  }

  public void Add(Action action)
  {
    Actions.Add(action);
  }

  public ActionSequence Sequence()
  {
    var sequence = new ActionSequence();
    Add(sequence);

    return sequence;
  }

  public ActionGroup Group()
  {
    var group = new ActionGroup();
    Add(group);

    return group;
  }

  public ActionDelay Delay(float duration)
  {
    var delay = new ActionDelay(duration);
    Add(delay);

    return delay;
  }

  public ActionCall Call(ActionCall.CallbackType callback, GameObject callbackOwner)
  {
    var call = new ActionCall(callback, callbackOwner);
    Add(call);

    return call;
  }

  public ActionDestroy Destroy(GameObject go)
  {
    var destroy = new ActionDestroy(go);
    Add(destroy);

    return destroy;
  }

  public ActionMove Move(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    var action = new ActionMove(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionMoveLocalY MoveLocalY(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionMoveLocalY(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionMoveRT MoveRT(GameObject go, Vector2 end, float duration, Ease ease = null)
  {
    var action = new ActionMoveRT(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionMoveRTX MoveRTX(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionMoveRTX(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionMoveRTY MoveRTY(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionMoveRTY(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionScale Scale(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    var action = new ActionScale(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionRectWidth RectWidth(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionRectWidth(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionRectHeight RectHeight(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionRectHeight(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionUiPreferredHeight UiPreferredHeight(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionUiPreferredHeight(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionRotate2DRT Rotate2DRT(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionRotate2DRT(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionUiColor UiColor(GameObject go, Color end, float duration, Ease ease = null)
  {
    var action = new ActionUiColor(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionUiColorRGB UiColorRGB(GameObject go, Color end, float duration, Ease ease = null)
  {
    var action = new ActionUiColorRGB(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionUiAlpha UiAlpha(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionUiAlpha(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionSpriteColor SpriteColor(GameObject go, Color end, float duration, Ease ease = null)
  {
    var action = new ActionSpriteColor(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionSpriteAlpha SpriteAlpha(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionSpriteAlpha(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionTurnFacing TurnFacing(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionTurnFacing(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionChangeCameraOffset ChangeCameraOffset(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    var action = new ActionChangeCameraOffset(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionOrthographicZoom OrthographicZoom(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionOrthographicZoom(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionOutlinerMin OutlinerMin(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    var action = new ActionOutlinerMin(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionOutlinerMax OutlinerMax(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    var action = new ActionOutlinerMax(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionRotate2D Rotate2D(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionRotate2D(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionParticleEmissionRate ParticleEmissionRate(GameObject go, float end, float duration, Ease ease = null)
  {
    var action = new ActionParticleEmissionRate(go, end, duration, ease);
    Add(action);

    return action;
  }

  public ActionState ProcessActions(float dt, bool blocking)
  {
    for (var i = 0; i < Count; ++i)
    {
      var action = Actions[i];

      if (!action.Active)
      {
        Actions.Remove(action);
        --i;

        continue;
      }

      var state = action.Update(dt);

      if (state != ActionState.Completed)
      {
        if (blocking)
          return ActionState.Running;
      }
      else
      {
        action.Completed = true;
        action.Active = false;
        Actions.Remove(action);
        --i;

        continue;
      }
    }

    if (Empty)
      return ActionState.Completed;
    else
      return ActionState.Running;
  }
}

public class ActionGroup : ActionSet
{
  public override ActionState Update(float dt)
  {
    return ProcessActions(dt, blocking: false);
  }
}

public class ActionSequence : ActionSet
{
  public override ActionState Update(float dt)
  {
    return ProcessActions(dt, blocking: true);
  }
}



public class ActionDelay : Action
{
  public ActionDelay(float duration)
  {
    Remaining = Duration = duration;
  }

  public override ActionState Update(float dt)
  {
    Started = true;
    Remaining -= dt;

    if (Remaining > 0)
      return ActionState.Running;
    else
      return ActionState.Completed;
  }
}

public class ActionCall : Action
{
  public delegate void CallbackType();
  public CallbackType Callback;

  public ActionCall(CallbackType callback, GameObject callbackOwner, string name = "Call")
  {
    Name = name;
    Callback = callback;
    GO = callbackOwner;
  }

  public override ActionState Update(float dt)
  {
    Started = true;
    Remaining = 0;

    if (GO != null)
      Callback();

    return ActionState.Completed;
  }
}

public class ActionDestroy : Action
{
  public ActionDestroy(GameObject go, string name = "Destroy")
  {
    GO = go;
    Name = name;
  }

  public override ActionState Update(float dt)
  {
    Started = true;
    Remaining = 0;

    Object.Destroy(GO);

    return ActionState.Completed;
  }
}

public abstract class ActionFloat : Action
{
  public float Start;
  public float End;
  public Ease Easer = new Ease();

  public ActionFloat(GameObject go, float end, float duration, Ease ease = null)
  {
    Name = "Float";
    GO = go;
    End = end;
    Remaining = Duration = duration;

    if (ease != null)
      Easer = ease;
  }

  public override ActionState Update(float dt)
  {
    if (GO == null)
      return ActionState.Completed;

    if (!Started)
    {
      Initialize();
      Started = true;
    }

    Remaining -= dt;
    var t = Mathf.Clamp01(Completion);

    if (t == 0)
      Set(Start);
    else if (t == 1)
      Set(End);
    else
      Set(Easer.Go(Start, End, t));

    if (Remaining > 0)
      return ActionState.Running;
    else
      return ActionState.Completed;
  }

  protected virtual void Initialize() { }
  protected virtual void Set(float value) { }
}

public abstract class ActionVector2 : Action
{
  public Vector2 Start;
  public Vector2 End;
  public Ease Easer = new Ease();

  public ActionVector2(GameObject go, Vector2 end, float duration, Ease ease = null)
  {
    Name = "Vector2";
    GO = go;
    End = end;
    Remaining = Duration = duration;

    if (ease != null)
      Easer = ease;
  }

  public override ActionState Update(float dt)
  {
    if (GO == null)
      return ActionState.Completed;

    if (!Started)
    {
      Initialize();
      Started = true;
    }

    Remaining -= dt;
    var t = Mathf.Clamp01(Completion);

    if (t == 0)
      Set(Start);
    else if (t == 1)
      Set(End);
    else
      Set(Easer.Go(Start, End, t));

    if (Remaining > 0)
      return ActionState.Running;
    else
      return ActionState.Completed;
  }

  protected virtual void Initialize() { }
  protected virtual void Set(Vector2 value) { }
}

public abstract class ActionVector3 : Action
{
  public Vector3 Start;
  public Vector3 End;
  public Ease Easer = new Ease();

  public ActionVector3(GameObject go, Vector3 end, float duration, Ease ease = null)
  {
    Name = "Vector3";
    GO = go;
    End = end;
    Remaining = Duration = duration;

    if (ease != null)
      Easer = ease;
  }

  public override ActionState Update(float dt)
  {
    if (GO == null)
      return ActionState.Completed;

    if (!Started)
    {
      Initialize();
      Started = true;
    }

    Remaining -= dt;
    var t = Mathf.Clamp01(Completion);

    if (t == 0)
      Set(Start);
    else if (t == 1)
      Set(End);
    else
      Set(Easer.Go(Start, End, t));

    if (Remaining > 0)
      return ActionState.Running;
    else
      return ActionState.Completed;
  }

  protected virtual void Initialize() {}
  protected virtual void Set(Vector3 value) {}
}

public abstract class ActionColor : Action
{
  public Color Start;
  public Color End;
  public Ease Easer = new Ease();

  public ActionColor(GameObject go, Color end, float duration, Ease ease = null)
  {
    Name = "Color";
    GO = go;
    End = end;
    Remaining = Duration = duration;

    if (ease != null)
      Easer = ease;
  }

  public override ActionState Update(float dt)
  {
    if (GO == null)
      return ActionState.Completed;

    if (!Started)
    {
      Initialize();
      Started = true;
    }

    Remaining -= dt;
    var t = Mathf.Clamp01(Completion);

    if (t == 0)
      Set(Start);
    else if (t == 1)
      Set(End);
    else
      Set(Easer.Go(Start, End, t));

    if (Remaining > 0)
      return ActionState.Running;
    else
      return ActionState.Completed;
  }

  protected virtual void Initialize() { }
  protected virtual void Set(Color value) { }
}

public class ActionMove : ActionVector3
{
  private Transform Tf;

  public ActionMove(GameObject go, Vector3 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "Move";
    Tf = go?.transform;
  }

  protected override void Initialize() { Start = Tf.position; }
  protected override void Set(Vector3 position) { Tf.position = position; }
}

public class ActionMoveRT : ActionVector2
{
  private RectTransform RT;

  public ActionMoveRT(GameObject go, Vector2 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "MoveRT";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.anchoredPosition; }
  protected override void Set(Vector2 value) { RT.anchoredPosition = value; }
}

public class ActionMoveRTX : ActionFloat
{
  private RectTransform RT;

  public ActionMoveRTX(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "MoveRTX";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.anchoredPosition.x; }
  protected override void Set(float x)
  {
    var anchoredPosition = RT.anchoredPosition;
    anchoredPosition.x = x;
    RT.anchoredPosition = anchoredPosition;
  }
}

public class ActionMoveRTY : ActionFloat
{
  private RectTransform RT;

  public ActionMoveRTY(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "MoveRTY";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.anchoredPosition.x; }
  protected override void Set(float y)
  {
    var anchoredPosition = RT.anchoredPosition;
    anchoredPosition.y = y;
    RT.anchoredPosition = anchoredPosition;
  }
}

public class ActionRotate2D : ActionFloat
{
  private Transform Tf;

  public ActionRotate2D(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "Rotate2D";
    Tf = go?.transform;
  }

  protected override void Initialize() { Start = Tf.localEulerAngles.z; }
  protected override void Set(float value)
  {
    var eulerAngles = Tf.localEulerAngles;
    eulerAngles.z = value;
    Tf.localEulerAngles = eulerAngles;
  }
}

public class ActionRotate2DRT : ActionFloat
{
  private RectTransform RT;

  public ActionRotate2DRT(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "Rotate2DRT";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.eulerAngles.z; }
  protected override void Set(float value)
  {
    var eulerAngles = RT.eulerAngles;
    eulerAngles.z = value;
    RT.eulerAngles = eulerAngles;
  }
}

public class ActionScale : ActionVector3
{
  private Transform Tf;

  public ActionScale(GameObject go, Vector3 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "Scale";
    Tf = go?.transform;
  }

  protected override void Initialize() { Start = Tf.localScale; }
  protected override void Set(Vector3 localScale) { Tf.localScale = localScale; }
}

public class ActionRectWidth : ActionFloat
{
  private RectTransform RT;

  public ActionRectWidth(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "RectWidth";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.rect.width; }
  protected override void Set(float width)
  { RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width); }
}

public class ActionRectHeight : ActionFloat
{
  private RectTransform RT;

  public ActionRectHeight(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "RectHeight";
    RT = go?.GetComponent<RectTransform>();
  }

  protected override void Initialize() { Start = RT.rect.height; }
  protected override void Set(float height)
  { RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height); }
}

public class ActionUiPreferredHeight : ActionFloat
{
  private LayoutElement LE;

  public ActionUiPreferredHeight(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "UiPreferredHeight";
    LE = go?.GetComponent<LayoutElement>();
  }

  protected override void Initialize() { Start = LE.preferredHeight; }
  protected override void Set(float preferredHeight) { LE.preferredHeight = preferredHeight; }
}

public class ActionUiColor : ActionColor
{
  private Graphic Graphic;

  public ActionUiColor(GameObject go, Color end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "UiColor";
    Graphic = go?.GetComponent<Graphic>();
  }

  protected override void Initialize() { Start = Graphic.color; }
  protected override void Set(Color value) { Graphic.color = value; }
}

public class ActionUiColorRGB : ActionColor
{
  private Graphic Graphic;

  public ActionUiColorRGB(GameObject go, Color end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "UiColorRGB";
    Graphic = go?.GetComponent<Graphic>();
  }

  protected override void Initialize() { Start = Graphic.color; }
  protected override void Set(Color color)
  {
    color.a = Graphic.color.a;
    Graphic.color = color;
  }
}

public class ActionUiAlpha : ActionFloat
{
  private Graphic Graphic;

  public ActionUiAlpha(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "UiAlpha";
    Graphic = go?.GetComponent<Graphic>();
  }

  protected override void Initialize() { Start = Graphic.color.a; }
  protected override void Set(float a)
  {
    var color = Graphic.color;
    color.a = a;
    Graphic.color = color;
  }
}

public class ActionSpriteColor : ActionColor
{
  private SpriteRenderer SR;

  public ActionSpriteColor(GameObject go, Color end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "SpriteColor";
    SR = go?.GetComponent<SpriteRenderer>();
  }

  protected override void Initialize() { Start = SR.color; }
  protected override void Set(Color color) { SR.color = color; }
}

public class ActionSpriteAlpha : ActionFloat
{
  private SpriteRenderer SR;

  public ActionSpriteAlpha(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "SpriteAlpha";
    SR = go?.GetComponent<SpriteRenderer>();
  }

  protected override void Initialize() { Start = SR.color.a; }
  protected override void Set(float a)
  {
    var color = SR.color;
    color.a = a;
    SR.color = color;
  }
}

public class ActionMoveLocalY : ActionFloat
{
  private Transform Tf;

  public ActionMoveLocalY(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "MoveLocalY";
    Tf = go?.transform;
  }

  protected override void Initialize() { Start = Tf.localPosition.y; }
  protected override void Set(float y)
  {
    var localPosition = Tf.localPosition;
    localPosition.y = y;
    Tf.localPosition = localPosition;
  }
}

public class ActionTurnFacing : ActionFloat
{
  private EyesController Eyes;

  public ActionTurnFacing(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "TurnFacing";
    Eyes = go?.GetComponent<EyesController>();
  }

  protected override void Initialize() { Start = Eyes.m_LookAngle; }
  protected override void Set(float angle) { Eyes.m_LookAngle = angle; }
}

public class ActionChangeCameraOffset : ActionVector3
{
  private CameraController mCameraController;

  public ActionChangeCameraOffset(GameObject go, Vector3 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "ChangeCameraOffset";
    mCameraController = go?.GetComponent<CameraController>();
  }

  protected override void Initialize() { Start = mCameraController.m_Offset; }
  protected override void Set(Vector3 offset) { mCameraController.m_Offset = offset; }
}

public class ActionOrthographicZoom : ActionFloat
{
  private Camera m_Camera;

  public ActionOrthographicZoom(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "OrthographicZoom";
    m_Camera = go?.GetComponent<Camera>();
  }

  protected override void Initialize() { Start = m_Camera.orthographicSize; }
  protected override void Set(float size) { m_Camera.orthographicSize = size; }
}

public class ActionOutlinerMin : ActionVector3
{
  private Outliner m_Outliner;

  public ActionOutlinerMin(GameObject go, Vector3 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "OutlinerMin";
    m_Outliner = go?.GetComponent<Outliner>();
  }

  protected override void Initialize() { Start = m_Outliner.m_DisplayMinBounds; }
  protected override void Set(Vector3 min) { m_Outliner.m_DisplayMinBounds = min; }
}

public class ActionOutlinerMax : ActionVector3
{
  private Outliner m_Outliner;

  public ActionOutlinerMax(GameObject go, Vector3 end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "OutlinerMax";
    m_Outliner = go?.GetComponent<Outliner>();
  }

  protected override void Initialize() { Start = m_Outliner.m_DisplayMaxBounds; }
  protected override void Set(Vector3 max)
  {
    m_Outliner.m_DisplayMaxBounds = max;
    m_Outliner.Outline();
  }
}

public class ActionParticleEmissionRate : ActionFloat
{
  private ParticleSystem m_PS;

  public ActionParticleEmissionRate(GameObject go, float end, float duration, Ease ease = null)
    : base(go, end, duration, ease)
  {
    Name = "ParticleEmissionRate";
    m_PS = go?.GetComponent<ParticleSystem>();
  }

  protected override void Initialize() { Start = m_PS.emission.rateOverTimeMultiplier; }
  protected override void Set(float value)
  {
    var emission = m_PS.emission;
    emission.rateOverTimeMultiplier = value;
  }
}

public enum ActionState
{
  Running,
  Completed,
}
