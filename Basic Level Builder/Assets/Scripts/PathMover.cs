using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour
{
  public float m_Speed = 1;

  Transform m_Transform;
  List<Vector3> m_Path;
  int m_PathIndex = 1;  // the index starts at 1 because the zeroth node is always the starting point
  Vector3 m_CurrentDestination;


  private void Awake()
  {
    m_Transform = transform;

    enabled = false;

    // For now, for debugging purposes, we will test with
    // this canned, hard-coded path that all movers will use
    // ***
    var testPath = new List<Vector2Int>()
    {
      new Vector2Int(0, 0),
      new Vector2Int(3, 4),
      new Vector2Int(-1, 4),
      new Vector2Int(0, 0),
    };

    Setup(testPath);
    // ***
    // Remove this code when you're done testing!

    GlobalData.PlayModeToggled += OnPlayModeToggled;
    GlobalData.HeroReturned += OnHeroReturned;
  }


  void Update()
  {
    var currentPosition = m_Transform.position;
    var difference = m_CurrentDestination - currentPosition;
    var direction = difference.normalized;
    var distance = difference.magnitude;
    var dPosMagnitude = m_Speed * Time.deltaTime;

    if (dPosMagnitude >= distance)
    {
      MoveTo(m_CurrentDestination);
      GetNewDestination();
    }
    else
    {
      var dPos = direction * dPosMagnitude;
      MoveBy(dPos);
    }
  }


  public void Setup(List<Vector2Int> indexPath)
  {
    if (indexPath == null)
      return;

    var count = indexPath.Count;

    if (count <= 1)
      return;

    m_Path = new List<Vector3>(count);
    var initialPosition = m_Transform.position;

    foreach (var index in indexPath)
    {
      var position = new Vector3(index.x, index.y, 0) + initialPosition;
      m_Path.Add(position);
    }

    SetDestination();
  }


  void ResetPath()
  {
    MoveTo(m_Path[0]);
    m_PathIndex = 1;
    SetDestination();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    ResetPath();

    if (isInPlayMode)
    {
      enabled = m_Path != null && m_Path.Count > 1;
    }
    else
    {
      enabled = false;
    }
  }


  void OnHeroReturned()
  {
    if (enabled)
      ResetPath();
  }


  void GetNewDestination()
  {
    ++m_PathIndex;

    if (m_PathIndex >= m_Path.Count)
    {
      // The index is reset to 1 instead of 0 because a closed
      // loop is defined by having identical start and end points,
      // so it is pointless (PUN VERY MUCH INTENDED) to go to the
      // start point after reaching the end point, because in a
      // closed loop, you're already there, and in an open path,
      // you're done moving anyway

      m_PathIndex = 1;

      var zerothNode = m_Path[0];
      var lastNode = m_Path[m_Path.Count - 1];

      if (zerothNode != lastNode)
      {
        // If the zeroth and last node are in different places,
        // then the path is open, and the mover should stop
        // when it gets to the end

        enabled = false;
      }

      // Otherwise, the path is a closed loop
    }

    SetDestination();
  }


  void SetDestination()
  {
    m_CurrentDestination = m_Path[m_PathIndex];
  }


  void MoveTo(Vector3 position)
  {
    m_Transform.position = position;
  }


  void MoveBy(Vector3 offset)
  {
    m_Transform.position += offset;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
    GlobalData.HeroReturned -= OnHeroReturned;
  }
}
