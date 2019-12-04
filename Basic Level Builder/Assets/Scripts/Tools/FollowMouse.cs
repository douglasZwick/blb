/***************************************************
File:           FollowMouse.cs
Authors:        Christopher Onorati
Last Updated:   6/5/2019
Last Version:   2019.1.4

Description:
  Script to control mouse following behavior.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.Events;

/**
* CLASS NAME  : FollowMouse
* DESCRIPTION : Class used to make a game object follow the mouse.
**/
public class FollowMouse : MonoBehaviour
{
  /************************************************************************************/

  public enum FollowMode
  {
    DIRECT,
    ROUNDED,
  };

  /************************************************************************************/

  [Tooltip("How the game object will follow the mouse.")]
  public FollowMode m_FollowMode;

  public Vector3Event Followed;

  /************************************************************************************/

  //SpriteRenderer of the game object.
  SpriteRenderer m_cSpriteRenderer;

  //Transform of the game object.
  Transform m_cTransform;

  //Main camera for getting mouse position.
  Camera m_cMainCamera;


  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches out components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    //Cache the main camera and transform of this game object.
    m_cMainCamera = Camera.main;
    m_cSpriteRenderer = GetComponent<SpriteRenderer>();
    m_cTransform = GetComponent<Transform>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }

  /**
  * FUNCTION NAME: Update
  * DESCRIPTION  : Manages game object movement.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Update()
  {
    if (m_FollowMode == FollowMode.DIRECT)
    {
      //Get mouse position with z axis of 0.
      Vector3 vecMousePosition = Input.mousePosition;
      vecMousePosition = m_cMainCamera.ScreenToWorldPoint(vecMousePosition);
      vecMousePosition.z = -9.0f;

      //Assign position.
      m_cTransform.position = vecMousePosition;
    }

    else if (m_FollowMode == FollowMode.ROUNDED)
    {
      //Get mouse position.
      Vector3 vecMousePosition = Input.mousePosition;
      vecMousePosition = m_cMainCamera.ScreenToWorldPoint(vecMousePosition);

      //Rounding time!
      vecMousePosition.x = Mathf.RoundToInt(vecMousePosition.x);
      vecMousePosition.y = Mathf.RoundToInt(vecMousePosition.y);
      vecMousePosition.z = -9.0f;

      //Assign it out.
      m_cTransform.position = vecMousePosition;
    }

    Followed.Invoke(m_cTransform.position);
  }

  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Hides the mouse sprite while in playmode.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool _IsInPlayMode)
  {
    //Show the sprite.
    if (!_IsInPlayMode)
    {
      if (m_cSpriteRenderer)
        m_cSpriteRenderer.enabled = true;
    }

    //Hide the sprite.
    else
    {
      if (m_cSpriteRenderer)
        m_cSpriteRenderer.enabled = false;
    }
  }

  /**
  * FUNCTION NAME: OnDestroy
  * DESCRIPTION  : Remove event connection.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}

[System.Serializable]
public class Vector3Event : UnityEvent<Vector3> { }
