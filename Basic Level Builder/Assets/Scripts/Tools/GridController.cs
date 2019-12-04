using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridController : MonoBehaviour
{
  //Main camera of the scene.
  Camera m_cMainCamera;

  //Transform of the game object.
  Transform m_cTransform;

  //Sprite renderer of the game object.
  SpriteRenderer m_cSpriteRenderer;

  // Start is called before the first frame update
  void Start()
  {
    m_cMainCamera = Camera.main;
    m_cTransform = GetComponent<Transform>();
    m_cSpriteRenderer = GetComponent<SpriteRenderer>();
  }

  // Update is called once per frame
  void Update()
  {
    //Manage sprite scaling.
    Vector2 newSize = new Vector2();
    newSize.x += m_cMainCamera.orthographicSize * 5;
    newSize.y += m_cMainCamera.orthographicSize * 2.5f;
    m_cSpriteRenderer.size = newSize;

    //Manage position.
    Vector3 vecPlacePosition = m_cMainCamera.transform.position;
    vecPlacePosition.x = Mathf.RoundToInt(vecPlacePosition.x);
    vecPlacePosition.x += (0.5f * m_cMainCamera.orthographicSize / 10.0f);
    vecPlacePosition.y = Mathf.RoundToInt(vecPlacePosition.y);
    vecPlacePosition.z = 0.0f;

    m_cTransform.position = vecPlacePosition;
  }
}
