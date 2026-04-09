/***************************************************
Authors:        Brenden Epp
Last Updated:   3/24/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.UI;

public class UiSaveFileInfoButton : MonoBehaviour
{
  [SerializeField]
  private UiFileInfo m_FileInfoPrefab;

  public void OnClick()
  {
    GameObject root = GameObject.FindGameObjectWithTag("FileInfoRoot");
    if (!root)
    {
      Debug.LogError("Could not find FileInfoRoot");
      return;
    }

    // Toggle on the black background
    root.GetComponent<Image>().enabled = true;

    UiFileInfo infoBox = Instantiate(m_FileInfoPrefab, root.GetComponent<RectTransform>());

    UiSaveFileItem parent = GetComponentInParent<UiSaveFileItem>();
    if (parent != null)
    {
      infoBox.InitLoad(parent.m_FullFilePath);
      GlobalData.IncrementUiPopup();
    }
    else
      Debug.LogError("Could not find parent of history button");
  }
}
