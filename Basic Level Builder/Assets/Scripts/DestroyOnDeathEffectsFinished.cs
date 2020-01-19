using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDeathEffectsFinished : MonoBehaviour
{
  public void OnDeathEffectsFinished(HealthEventData eventData)
  {
    Destroy(gameObject);
  }
}
