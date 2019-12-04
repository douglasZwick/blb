using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collectable))]
public class CoinCountIncrementer : MonoBehaviour
{
  public int m_Value = 1;

  Collectable m_Collectable;


  public void OnCollected()
  {
    GlobalData.DispatchCoinCollected(m_Value);
  }
}
