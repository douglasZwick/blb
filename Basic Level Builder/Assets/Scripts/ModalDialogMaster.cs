using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(UiSafePositioner))]
public class ModalDialogMaster : MonoBehaviour
{
  class ModalDialogRequest
  {
    public ModalDialog m_DialogPrefab;
    public Vector2 m_SpawnRectPoint;
    public string[] m_Strings;
  }

  public Camera m_HudCamera;
  public float m_TintDuration = 0.25f;

  [HideInInspector]
  public bool m_Active = false;

  RectTransform m_RectTransform;
  Image m_Image;
  UiSafePositioner m_Positioner;
  Stack<ModalDialogRequest> m_PendingRequests = new Stack<ModalDialogRequest>();
  ActionSequence m_ActivationSequence;
  float m_ActiveAlpha;
  bool m_EndOperationWhenFinished = false;


  void Start()
  {
    m_RectTransform = GetComponent<RectTransform>();
    m_Image = GetComponent<Image>();
    m_Positioner = GetComponent<UiSafePositioner>();
    var color = m_Image.color;
    m_ActiveAlpha = color.a;
    color.a = 0;
    m_Image.color = color;

    m_ActivationSequence = ActionMaster.Actions.Sequence();
  }


  public void RequestDialogAtRectPoint(ModalDialog prefab, Vector2 rectPoint, params string[] strings)
  {
    var request = new ModalDialogRequest()
    {
      m_DialogPrefab = prefab,
      m_SpawnRectPoint = rectPoint,
      m_Strings = strings,
    };

    m_PendingRequests.Push(request);
  }


  public void RequestDialogAtWorldPoint(ModalDialog prefab, Vector3 worldPoint, params string[] strings)
  {
    var rectPoint = WorldPointToRectPoint(worldPoint);

    RequestDialogAtRectPoint(prefab, rectPoint, strings);
  }


  public void RequestDialogAtCenter(ModalDialog prefab, params string[] strings)
  {
    var x = Screen.width / 2f;
    var y = Screen.height / 2f;
    var rectPoint = new Vector2(x, y);

    RequestDialogAtRectPoint(prefab, rectPoint, strings);
  }


  public void Begin(bool endOperationWhenFinished = false)
  {
    m_EndOperationWhenFinished = endOperationWhenFinished;
    Next();
  }


  void Next()
  {
    var request = m_PendingRequests.Pop();
    var prefab = request.m_DialogPrefab;
    var rectPointToSpawnAt = request.m_SpawnRectPoint;
    var strings = request.m_Strings;
    var dialog = Instantiate(prefab);
    dialog.Setup(this, rectPointToSpawnAt, strings);
    dialog.Open();
  }


  void End()
  {
    if (m_EndOperationWhenFinished)
    {
      m_EndOperationWhenFinished = false;
      OperationSystem.EndOperation();
    }

    AttemptDeactivate();
  }


  public void Add(RectTransform inputWidget, Vector2 desiredPosition, Vector2 size)
  {
    m_Positioner.AttachAtSafePosition(inputWidget, desiredPosition, size);

    AttemptActivate();
  }


  public void Remove()
  {
    if (m_PendingRequests.Count > 0)
      Next();
    else
      End();
  }


  void AttemptActivate()
  {
    if (!m_Active)
      Activate();
  }


  void AttemptDeactivate()
  {
    if (m_Active)
      Deactivate();
  }


  void Activate()
  {
    HotkeyMaster.s_HotkeysEnabled = false;

    m_Active = true;
    m_Image.enabled = true;
    m_Image.raycastTarget = true;

    m_ActivationSequence.Cancel();
    m_ActivationSequence = ActionMaster.Actions.Sequence();
    m_ActivationSequence.UiAlpha(gameObject, m_ActiveAlpha, m_TintDuration);
  }


  void Deactivate()
  {
    m_Active = false;
    m_Image.raycastTarget = false;

    m_ActivationSequence.Cancel();
    m_ActivationSequence = ActionMaster.Actions.Sequence();
    m_ActivationSequence.UiAlpha(gameObject, 0, m_TintDuration);
    m_ActivationSequence.Call(EndDeactivate, gameObject);
  }


  void EndDeactivate()
  {
    HotkeyMaster.s_HotkeysEnabled = true;
    DisableImage();
  }


  public Vector2 ScreenPointToRectPoint(Vector3 screenPoint)
  {
    RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform,
      screenPoint, null, out Vector2 localPoint);
    return m_RectTransform.TransformPoint(localPoint);
  }


  public Vector2 WorldPointToRectPoint(Vector3 worldPoint)
  {
    var screenPoint = m_HudCamera.WorldToScreenPoint(worldPoint);
    return ScreenPointToRectPoint(screenPoint);
  }


  void DisableImage()
  {
    m_Image.enabled = false;
  }
}
