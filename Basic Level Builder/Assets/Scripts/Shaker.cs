using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
  [Range(0, 1)]
  public float m_IntensityPerShake = 0.25f;
  public float m_IntensityExponent = 2;
  public Vector3 m_TranslationalMagnitude = new Vector3(1, 1, 0);
  public Vector3 m_RotationalMagnitude = new Vector3(0, 0, 15);
  public float m_DecayRate = 2;

  Transform m_Transform;
  float m_CurrentIntensity = 0;
  Vector3 m_LatestTranslation = Vector3.zero;
  Quaternion m_LatestRotation = Quaternion.identity;


  private void Awake()
  {
    m_Transform = transform;
    enabled = false;
  }


  public void BeginShaking()
  {
    IncrementIntensity();
    enabled = true;
  }


  public void Cancel()
  {
    EndShaking();
    m_CurrentIntensity = 0;
  }


  void EndShaking()
  {
    ApplyShake(Vector3.zero, Quaternion.identity);

    enabled = false;
  }


  private void FixedUpdate()
  {
    var perceivedIntensity = Mathf.Pow(m_CurrentIntensity, m_IntensityExponent);

    var baseTranslation = Vector3.Scale(Random.insideUnitSphere, m_TranslationalMagnitude);
    var translation = baseTranslation * Random.Range(0, perceivedIntensity);

    var xAngle = Random.Range(-m_RotationalMagnitude.x, m_RotationalMagnitude.x);
    var yAngle = Random.Range(-m_RotationalMagnitude.y, m_RotationalMagnitude.y);
    var zAngle = Random.Range(-m_RotationalMagnitude.z, m_RotationalMagnitude.z);

    var baseEulerOffset = new Vector3(xAngle, yAngle, zAngle);
    var eulerOffset = baseEulerOffset * Random.Range(0, perceivedIntensity);
    var rotation = Quaternion.Euler(eulerOffset);

    ApplyShake(translation, rotation);

    m_CurrentIntensity -= m_DecayRate * Time.deltaTime;

    if (m_CurrentIntensity <= 0)
    {
      m_CurrentIntensity = 0;
      EndShaking();
    }
  }


  void IncrementIntensity()
  {
    m_CurrentIntensity = Mathf.Clamp01(m_CurrentIntensity + m_IntensityPerShake);
  }


  void ApplyShake(Vector3 translation, Quaternion rotation)
  {
    transform.position += translation - m_LatestTranslation;
    transform.rotation *= rotation * Quaternion.Inverse(m_LatestRotation);

    m_LatestTranslation = translation;
    m_LatestRotation = rotation;
  }
}
