using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesController : MonoBehaviour
{
  public Transform m_EyesRoot;
  public Transform m_LeftBlinkRoot;
  public Transform m_RightBlinkRoot;
  public Transform m_LeftRotateRoot;
  public Transform m_RightRotateRoot;
  public Transform m_LeftX;
  public Transform m_RightX;
  public SpriteRenderer m_LeftEyeSprite;
  public SpriteRenderer m_RightEyeSprite;
  public SpriteRenderer m_LeftShineSprite;
  public SpriteRenderer m_RightShineSprite;
  public float m_LookHeight = 0.5f;
  public float m_LookDuration = 0.1f;
  public Vector3 m_BlinkScale = new Vector3(1.5f, 0.1f, 1.0f);
  public float m_BlinkDuration = 0.1f;
  public float m_BlinkDelayBase = 3;
  public float m_BlinkDelayVariance = 2;
  public float m_DoubleBlinkDelay = 0.1f;
  public float m_DoubleBlinkProbability = 0.1f;
  public float m_FaceDuration = 0.1f;

  ActionSequence m_LookSequence;
  float m_LookDefaultY;
  ActionSequence m_BlinkSequence;
  float m_MinDelay;
  float m_MaxDelay;
  float m_RightLookAngle; // in degrees
  float m_LeftLookAngle;  // in degrees
  float m_EyeAngleOffset; // in degrees
  ActionSequence m_FaceSequence;
  int m_EyeDefaultSortingOrder;
  int m_ShineDefaultSortingOrder;

  float m_LookAngle_; // in degrees
  public float m_LookAngle   // in degrees
  {
    get
    {
      return m_LookAngle_;
    }

    set
    {
      value = Mathf.Repeat(value + 180, 360) - 180;

      m_LookAngle_ = value;

      var rightEyeAngle = Mathf.Repeat(value + 180 - m_EyeAngleOffset, 360) - 180;
      var rightX = Mathf.Cos(rightEyeAngle * Mathf.Deg2Rad) / 2;
      var rightPos = m_RightRotateRoot.localPosition;
      rightPos.x = rightX;
      m_RightRotateRoot.localPosition = rightPos;
      var leftEyeAngle = Mathf.Repeat(value + 180 + m_EyeAngleOffset, 360) - 180;
      var leftX = Mathf.Cos(leftEyeAngle * Mathf.Deg2Rad) / 2;
      var leftPos = m_LeftRotateRoot.localPosition;
      leftPos.x = leftX;
      m_LeftRotateRoot.localPosition = leftPos;

      if (rightEyeAngle > 0)
      {
        m_RightEyeSprite.sortingOrder = m_EyeDefaultSortingOrder;
        m_RightShineSprite.sortingOrder = m_ShineDefaultSortingOrder;
      }
      else
      {
        m_RightEyeSprite.sortingOrder = -m_EyeDefaultSortingOrder;
        m_RightShineSprite.sortingOrder = -m_ShineDefaultSortingOrder;
      }

      if (leftEyeAngle > 0)
      {
        m_LeftEyeSprite.sortingOrder = m_EyeDefaultSortingOrder;
        m_LeftShineSprite.sortingOrder = m_ShineDefaultSortingOrder;
      }
      else
      {
        m_LeftEyeSprite.sortingOrder = -m_EyeDefaultSortingOrder;
        m_LeftShineSprite.sortingOrder = -m_ShineDefaultSortingOrder;
      }
    }
  }


  private void Awake()
  {
    m_LookDefaultY = m_EyesRoot.localPosition.y;
    m_MinDelay = m_BlinkDelayBase - m_BlinkDelayVariance;
    m_MaxDelay = m_BlinkDelayBase + m_BlinkDelayVariance;

    var rightEyeInitialX = m_RightRotateRoot.localPosition.x;
    var rightEyeCosine = 2 * rightEyeInitialX;
    var rightEyeAngle = Mathf.Acos(rightEyeCosine) * Mathf.Rad2Deg;
    var leftEyeInitialX = m_LeftRotateRoot.localPosition.x;
    var leftEyeCosine = 2 * leftEyeInitialX;
    var leftEyeAngle = Mathf.Acos(leftEyeCosine) * Mathf.Rad2Deg;
    var angleDifference = Mathf.Abs(rightEyeAngle - leftEyeAngle);
    m_EyeAngleOffset = angleDifference / 2;
    m_RightLookAngle = rightEyeAngle + m_EyeAngleOffset;
    m_LeftLookAngle = 180 - m_RightLookAngle;
    m_FaceSequence = ActionMaster.Actions.Sequence();
    m_LookAngle_ = m_RightLookAngle;

    m_EyeDefaultSortingOrder = m_RightEyeSprite.sortingOrder;
    m_ShineDefaultSortingOrder = m_RightShineSprite.sortingOrder;

    m_LookSequence = ActionMaster.Actions.Sequence();
    m_BlinkSequence = ActionMaster.Actions.Sequence();
    BlinkDelay();
  }


  public void LookUp()
  {
    m_LookSequence.Cancel();
    m_LookSequence = ActionMaster.Actions.Sequence();
    m_LookSequence.MoveLocalY(m_EyesRoot.gameObject, m_LookDefaultY + m_LookHeight, m_LookDuration, new Ease(Ease.Quad.InOut));
  }


  public void LookDown()
  {
    m_LookSequence.Cancel();
    m_LookSequence = ActionMaster.Actions.Sequence();
    m_LookSequence.MoveLocalY(m_EyesRoot.gameObject, m_LookDefaultY, m_LookDuration, new Ease(Ease.Quad.InOut));
  }


  public void FaceRight(bool snap = false)
  {
    if (snap)
    {
      m_LookAngle = m_RightLookAngle;
    }
    else
    {
      m_FaceSequence.Cancel();
      m_FaceSequence = ActionMaster.Actions.Sequence();
      m_FaceSequence.TurnFacing(gameObject, m_RightLookAngle, m_FaceDuration, new Ease(Ease.Quad.InOut));
    }
  }


  public void FaceLeft(bool snap = false)
  {
    if (snap)
    {
      m_LookAngle = m_LeftLookAngle;
    }
    else
    {
      m_FaceSequence.Cancel();
      m_FaceSequence = ActionMaster.Actions.Sequence();
      m_FaceSequence.TurnFacing(gameObject, m_LeftLookAngle, m_FaceDuration, new Ease(Ease.Quad.InOut));
    }
  }


  private void BlinkDelay()
  {
    var delay = Random.Range(m_MinDelay, m_MaxDelay);
    var shouldDoubleBlink = Random.value < m_DoubleBlinkProbability;
    m_BlinkSequence.Delay(delay);

    if (shouldDoubleBlink)
      m_BlinkSequence.Call(DoubleBlink, gameObject);
    else
      m_BlinkSequence.Call(Blink, gameObject);
  }


  private void Blink()
  {
    BlinkHelper();
    m_BlinkSequence.Call(BlinkDelay, gameObject);
  }


  private void DoubleBlink()
  {
    BlinkHelper();
    m_BlinkSequence.Delay(m_DoubleBlinkDelay);
    Blink();
  }


  private void BlinkHelper()
  {
    if (!enabled || m_LeftBlinkRoot == null || m_RightBlinkRoot == null)
      return;

    var duration = m_BlinkDuration / 2;
    var closeGrp = m_BlinkSequence.Group();
    closeGrp.Scale(m_LeftBlinkRoot.gameObject, m_BlinkScale, duration, new Ease(Ease.Quad.InOut));
    closeGrp.Scale(m_RightBlinkRoot.gameObject, m_BlinkScale, duration, new Ease(Ease.Quad.InOut));
    m_BlinkSequence.Delay(duration);
    var openGrp = m_BlinkSequence.Group();
    openGrp.Scale(m_LeftBlinkRoot.gameObject, Vector3.one, duration, new Ease(Ease.Quad.InOut));
    openGrp.Scale(m_RightBlinkRoot.gameObject, Vector3.one, duration, new Ease(Ease.Quad.InOut));
  }


  public void OnDied(HealthEventData eventData)
  {
    m_LeftEyeSprite.enabled = false;
    m_RightEyeSprite.enabled = false;
    m_LeftShineSprite.enabled = false;
    m_RightShineSprite.enabled = false;
    m_LeftX.gameObject.SetActive(true);
    m_RightX.gameObject.SetActive(true);
  }


  public void OnReturned(HealthEventData eventData)
  {
    m_LeftEyeSprite.enabled = true;
    m_RightEyeSprite.enabled = true;
    m_LeftShineSprite.enabled = true;
    m_RightShineSprite.enabled = true;
    m_LeftX.gameObject.SetActive(false);
    m_RightX.gameObject.SetActive(false);
  }


  public void OnFacedLeft(FacingEventData eventData)
  {
    FaceLeft(snap: false);
  }


  public void OnFacedRight(FacingEventData eventData)
  {
    FaceRight(snap: false);
  }


  public void OnDirectionInitialized(TileDirectionEventData eventData)
  {
    switch (eventData.m_Direction)
    {
      case Direction.LEFT:
        FaceLeft(snap: true);
        break;
      case Direction.RIGHT:
        FaceRight(snap: true);
        break;
      default:
        // eventually something may go here
        break;
    }
  }
}
