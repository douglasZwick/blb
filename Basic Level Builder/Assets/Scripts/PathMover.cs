using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour
{
  static Vector3 s_IconLocalPosition = Vector3.zero;

  public float m_Speed = 5.5f;

  Transform m_Transform;
  public List<Vector3> m_Path { get; private set; }
  public List<Vector2Int> m_IndexList { get; private set; }
  int m_PathIndex = 0;
  Vector3 m_InitialPosition;
  Vector3 m_CurrentDestination;
  bool m_Loop = false;


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


  public void Setup(List<Vector2Int> indexList)
  {
    m_IndexList = indexList;
    m_Path = null;

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

    m_Path = new List<Vector3>(count);
    m_InitialPosition = m_Transform.position;

    foreach (var index in indexList)
    {
      var position = new Vector3(index.x, index.y, 0) + m_InitialPosition;
      m_Path.Add(position);
    }

    m_Loop = indexList[count - 1] == Vector2Int.zero;

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


  void OnPreHeroReturn()
  {
    if (m_Path != null)
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
