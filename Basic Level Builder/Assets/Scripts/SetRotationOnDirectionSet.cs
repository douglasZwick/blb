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

    if (!m_TransformToRotate)
      m_TransformToRotate = transform;
  }


  void OnDirectionSet(Direction direction)
  {
    float angle = direction switch
    {
      Direction.UP => 90,
      Direction.LEFT => 180,
      Direction.DOWN => -90,
      // Direction.RIGHT
      _ => 0,
    };
    m_TransformToRotate.localEulerAngles = Vector3.forward * angle;
  }


  private void OnDestroy()
  {
    m_TileDirection.DirectionSet -= OnDirectionSet;
  }
}