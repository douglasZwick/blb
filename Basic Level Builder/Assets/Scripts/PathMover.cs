using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour
{
  static Vector3 s_IconLocalPosition = Vector3.zero;

  public float m_Speed = 5.5f;

  Transform m_Transform;
  public List<Vector2Int> m_IndexList { get; private set; }
  int m_PathIndex = 0;
  Vector3 m_InitialPosition;
  Vector3 m_Start;
  Vector3 m_End;
  Vector3 m_Difference;
  bool m_Loop = false;
  float m_Duration;
  float m_Timer = 0;


  private void Awake()
  {
    m_Transform = transform;

    if (!GlobalData.IsTransitioning())
    {
      var icon = Instantiate(PathTool.s_PathIconPrefab, m_Transform);
      icon.transform.localPosition = s_IconLocalPosition;
    }

    enabled = false;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
    GlobalData.PreHeroReturn += OnPreHeroReturn;
  }


  void Update()
  {
    var interpolant = m_Timer / m_Duration;
    var newPosition = m_Start + m_Difference * interpolant;
    MoveTo(newPosition);

    m_Timer += Time.deltaTime;

    if (m_Timer >= m_Duration)
    {
      m_Timer -= m_Duration;
      GetNewDestination();
    }
  }


  public void Setup(List<Vector2Int> indexList)
  {
    m_IndexList = null;

    if (indexList == null)
      return;

    var count = indexList.Count;

    if (count <= 0)
      return;

    var allZero = true;
    foreach (var index in indexList)
      if (index != Vector2Int.zero)
        allZero = false;

    if (allZero)
      return;

    m_IndexList = indexList;
    m_InitialPosition = m_Transform.position;
    m_End = m_InitialPosition;

    m_Loop = indexList[count - 1] == Vector2Int.zero;

    SetDestination();
  }


  void ResetPath()
  {
    if (m_IndexList == null || m_IndexList.Count == 0)
      return;

    MoveTo(m_InitialPosition);
    m_Timer = 0;
    m_PathIndex = 0;
    m_End = m_InitialPosition;
    SetDestination();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      enabled = m_IndexList != null;

      if (enabled)
        ResetPath();
    }
    else
    {
      enabled = false;

      ResetPath();
    }
  }


  void OnPreHeroReturn()
  {
    if (m_IndexList != null)
      ResetPath();
  }


  void GetNewDestination()
  {
    var pathLength = m_IndexList.Count;

    for (var checkCounter = 0; checkCounter < pathLength; ++checkCounter)
    {
      ++m_PathIndex;

      if (m_PathIndex >= pathLength)
      {
        m_PathIndex = 0;

        if (!m_Loop)
        {
          enabled = false;
        }

        // Otherwise, the path is a closed loop
      }

      var currentNode = m_IndexList[m_PathIndex];
      var nextIndex = m_PathIndex + 1;

      if (nextIndex >= pathLength)
        nextIndex = 0;

      var nextNode = m_IndexList[nextIndex];

      if (currentNode != nextNode)
        break;
    }

    SetDestination();
  }


  void SetDestination()
  {
    var currentNode = m_IndexList[m_PathIndex];

    m_Start = m_End;
    m_End = new Vector3(currentNode.x, currentNode.y, 0) + m_InitialPosition;
    m_Difference = m_End - m_Start;
    var distance = m_Difference.magnitude;
    m_Duration = distance / m_Speed;
  }


  void MoveTo(Vector3 position)
  {
    m_Transform.position = position;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
    GlobalData.PreHeroReturn -= OnPreHeroReturn;
  }


  static public bool SamePath(PathMover a, PathMover b)
  {
    if (a == null)
      return b == null;
    if (b == null)
      return a == null;

    var listA = a.m_IndexList;
    var listB = b.m_IndexList;

    if (listA == null)
      return listB == null;
    if (listB == null)
      return listA == null;

    if (listA.Count != listB.Count)
      return false;

    for (var i = 0; i < listA.Count; ++i)
    {
      if (listA[i] != listB[i])
        return false;
    }

    return true;
  }
}
