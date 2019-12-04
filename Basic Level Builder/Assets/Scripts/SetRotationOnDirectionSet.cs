using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileDirection))]
public class SetRotationOnDirectionSet : MonoBehaviour
{
  public Transform m_TransformToRotate;

  TileDirection m_TileDirection;


  private void Awake()
  {
    m_TileDirection = GetComponent<TileDirection>();

    m_TileDirection.DirectionSet += OnDirectionSet;
  }


  void OnDirectionSet(Direction direction)
  {
    float angle;

    switch (direction)
    {
      default:  // Direction.RIGHT
        angle = 0;
        break;

      case Direction.UP:
        angle = 90;
        break;

      case Direction.LEFT:
        angle = 180;
        break;

      case Direction.DOWN:
        angle = -90;
        break;
    }

    var angles = Vector3.forward * angle;
    m_TransformToRotate.localEulerAngles = angles;
  }


  private void OnDestroy()
  {
    m_TileDirection.DirectionSet -= OnDirectionSet;
  }
}
