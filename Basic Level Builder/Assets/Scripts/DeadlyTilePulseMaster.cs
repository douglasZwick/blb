using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyTilePulseMaster : MonoBehaviour
{
  public static DeadlyTilePulseMaster Instance;
  public static float s_Timer;

  public Color m_Color0 = Color.HSVToRGB(350 / 360.0f, 0.9f, 1);
  public Color m_Color1 = Color.white;
  public float m_Period = 1;
  public float m_Exponent = 4;


  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Debug.LogError($"Excuse me but wtf is {gameObject.name} doing with a DeadlyTilePulseMaster" +
        $"when {Instance.gameObject.name} already has a perfectly good one???");
  }


  void Update()
  {
    s_Timer = Mathf.Repeat(s_Timer + Time.deltaTime, m_Period);
  }


  static float GetCurrentInterpolant()
  {
    var fraction = s_Timer / Instance.m_Period;
    var innerPart = 2 * Mathf.Abs(fraction - 0.5f);

    return Mathf.Pow(innerPart, Instance.m_Exponent);
  }


  public static Color GetCurrentColor()
  {
    var interpolant = GetCurrentInterpolant();

    return Color.Lerp(Instance.m_Color0, Instance.m_Color1, interpolant);
  }
}
