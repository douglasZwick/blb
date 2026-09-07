using UnityEngine;
using UnityEngine.EventSystems;

public class TilePreviewHoverAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField]
  private TileIconUpdater m_TileIconUpdater;
  [SerializeField]
  private bool m_Clockwise;

  public void OnPointerEnter(PointerEventData eventData)
  {
    m_TileIconUpdater.JigglePrimaryTile(m_Clockwise ? -1 : 1);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    m_TileIconUpdater.CancelPrimaryTileJiggle();
  }

  private void OnDisable()
  {
    if (m_TileIconUpdater != null)
      m_TileIconUpdater.CancelPrimaryTileJiggle();
  }
}