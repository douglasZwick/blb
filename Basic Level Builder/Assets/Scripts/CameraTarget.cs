using UnityEngine;

[RequireComponent(typeof(TileDirection))]
public class CameraTarget : MonoBehaviour
{
  [HideInInspector]
  public Transform m_Transform;
  [HideInInspector]
  public Rigidbody2D m_Rigidbody2D;


  private void Awake()
  {
    m_Transform = transform;
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
  }
}
