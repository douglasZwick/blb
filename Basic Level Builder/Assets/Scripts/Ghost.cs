using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
  public float m_BodyFadeDuration = 0.1f;
  public float m_EyesFadeDelay = 0.5f;
  public float m_EyesFadeDuration = 0.5f;
  public List<Transform> m_SquashNodes;

  public Transform m_HopNode;

  public Transform m_EyesRoot;
  public Transform m_LeftBlinkRoot;
  public Transform m_RightBlinkRoot;
  public Transform m_LeftRotateRoot;
  public Transform m_RightRotateRoot;

  public SpriteRenderer m_BodyRenderer;
  public SpriteRenderer m_LeftEyeRenderer;
  public SpriteRenderer m_RightEyeRenderer;
  public SpriteRenderer m_LeftShineRenderer;
  public SpriteRenderer m_RightShineRenderer;


  private void Awake()
  {
    GlobalData.GhostModeDisabled += OnGhostModeDisabled;
    GlobalData.GhostCleanup += OnGhostCleanup;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
      Cleanup();
  }


  void OnGhostModeDisabled()
  {
    Cleanup();
  }


  void OnGhostCleanup()
  {
    Cleanup();
  }


  public void Setup(List<SquashController> squashControllers,
    HopController hopController, EyesController eyesController)
  {
    for (var i = 0; i < m_SquashNodes.Count; ++i)
      m_SquashNodes[i].localScale = squashControllers[i].m_SquashNode.localScale;

    m_HopNode.localPosition = hopController.m_HopNode.localPosition;

    m_EyesRoot.localPosition = eyesController.m_EyesRoot.localPosition;
    m_LeftBlinkRoot.localScale = eyesController.m_LeftBlinkRoot.localScale;
    m_RightBlinkRoot.localScale = eyesController.m_RightBlinkRoot.localScale;
    m_LeftRotateRoot.localPosition = eyesController.m_LeftRotateRoot.localPosition;
    m_RightRotateRoot.localPosition = eyesController.m_RightRotateRoot.localPosition;
  }


  public void Cleanup()
  {
    var mainSeq = ActionMaster.Actions.Sequence();

      var mainGrp = mainSeq.Group();
      
        mainGrp.SpriteAlpha(m_BodyRenderer.gameObject, 0, m_BodyFadeDuration, new Ease(Ease.Quad.InOut));
        var eyesSeq = mainGrp.Sequence();
        
          eyesSeq.Delay(m_EyesFadeDelay);
          var eyesGrp = eyesSeq.Group();
          
            eyesGrp.SpriteAlpha(m_LeftEyeRenderer.gameObject, 0, m_EyesFadeDuration, new Ease(Ease.Quad.InOut));
            eyesGrp.SpriteAlpha(m_RightEyeRenderer.gameObject, 0, m_EyesFadeDuration, new Ease(Ease.Quad.InOut));
            eyesGrp.SpriteAlpha(m_LeftShineRenderer.gameObject, 0, m_EyesFadeDuration, new Ease(Ease.Quad.InOut));
            eyesGrp.SpriteAlpha(m_RightShineRenderer.gameObject, 0, m_EyesFadeDuration, new Ease(Ease.Quad.InOut));

      mainSeq.Call(EndCleanup, gameObject);
  }


  public void EndCleanup()
  {
    Destroy(gameObject);
  }


  private void OnDestroy()
  {
    GlobalData.GhostModeDisabled -= OnGhostModeDisabled;
    GlobalData.GhostCleanup -= OnGhostCleanup;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
