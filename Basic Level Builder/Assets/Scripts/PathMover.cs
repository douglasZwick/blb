using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour
{
  public float m_Speed = 1;

  Transform m_Transform;
  List<Vector3> m_Path;
  int m_PathIndex = 0;
  Vector3 m_InitialPosition;
  Vector3 m_CurrentDestination;
  bool m_Loop = false;


  private void Awake()
  {
    m_Transform = transform;

    enabled = false;

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
    m_Path = null;

    if (indexPath == null)
      return;

    var count = indexPath.Count;

    if (count <= 0)
      return;

    var allZero = true;
    foreach (var index in indexPath)
      if (index != Vector2Int.zero)
        allZero = false;

    if (allZero)
      return;

    m_Path = new List<Vector3>(count);
    m_InitialPosition = m_Transform.position;

    foreach (var index in indexPath)
    {
      var position = new Vector3(index.x, index.y, 0) + m_InitialPosition;
      m_Path.Add(position);
    }

    m_Loop = indexPath[count - 1] == Vector2Int.zero;

    SetDestination();
  }


  void ResetPath()
  {
    MoveTo(m_InitialPosition);
    m_PathIndex = 0;
    SetDestination();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    ResetPath();

    if (isInPlayMode)
    {
      enabled = m_Path != null;
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
      m_PathIndex = 0;

      if (!m_Loop)
      {
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
