using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateForWeb : MonoBehaviour
{
  private void Awake()
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
      gameObject.SetActive(false);
  }
}
