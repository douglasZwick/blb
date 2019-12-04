using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMover : MonoBehaviour
{
  Transform m_Transform;
  List<Vector3> m_Path;
  float m_Speed = 1;
  Vector3 m_CurrentDestination;


  private void Awake()
  {
    enabled = false;
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
      SetNextDestination();
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

    for (var i = 0; i < count; ++i)
    {
      var index = indexPath[i];
      var position = new Vector3(index.x, index.y, 0) + initialPosition;
      m_Path[i] = position;
    }

    enabled = true;

    SetDestinationFromNode(indexPath[0]);
  }


  void SetDestinationFromNode(Vector2Int node)
  {
    m_CurrentDestination = new Vector3(node.x, node.y, 0);
  }


  void SetNextDestination()
  {
    var zerothNode = m_Path[0];
    var lastNode = m_Path[m_Path.Count - 1];

    if (zerothNode == lastNode)
    {
      // the loop is closed
    }
    else
    {

    }
  }


  void MoveTo(Vector3 position)
  {
    m_Transform.position = position;
  }


  void MoveBy(Vector3 offset)
  {
    m_Transform.position += offset;
  }
}
