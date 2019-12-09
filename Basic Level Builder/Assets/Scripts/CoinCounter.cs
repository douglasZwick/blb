using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinCounter : MonoBehaviour
{
  public Image m_CoinIcon;
  public TextMeshProUGUI m_Text;

  int m_CurrentCount;
  int m_Total;


  private void Awake()
  {
    GlobalData.PlayModeToggled += OnPlayModeToggled;
    GlobalData.CoinCollected += OnCoinCollected;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_CurrentCount = 0;

      CountCoins();
      SetText();
      //m_Text.enabled = m_CoinIcon.enabled = m_Total > 0;
      gameObject.SetActive(m_Total > 0);
    }
  }


  void OnCoinCollected(int value)
  {
    m_CurrentCount += value;
    SetText();
  }


  void CountCoins()
  {
    m_Total = 0;

    var allCoins = FindObjectsOfType<Coin>();

    foreach (var coin in allCoins)
      m_Total += coin.m_Value;
  }


  void SetText()
  {
    m_Text.text = $"{m_CurrentCount} / {m_Total}";
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
    GlobalData.CoinCollected -= OnCoinCollected;
  }
}
