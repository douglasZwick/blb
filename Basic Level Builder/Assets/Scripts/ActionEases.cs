using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ease
{
  public delegate float Easer(float t, float parameter = 1);
  public Easer EasingFunction = Linear;
  public float Parameter = 1;

  public Ease()
  {
    EasingFunction = Quad.InOut;
  }

  public Ease(Easer easer)
  {
    EasingFunction = easer;
  }

  public Ease(Easer easer, float parameter)
  {
    EasingFunction = easer;
    Parameter = parameter;
  }

  public float Go(float start, float end, float t)
  {
    return Mathf.LerpUnclamped(start, end, EasingFunction(t, Parameter));
  }

  public Vector2 Go(Vector2 start, Vector2 end, float t)
  {
    return Vector2.LerpUnclamped(start, end, EasingFunction(t, Parameter));
  }

  public Vector3 Go(Vector3 start, Vector3 end, float t)
  {
    return Vector3.LerpUnclamped(start, end, EasingFunction(t, Parameter));
  }

  public Color Go(Color start, Color end, float t)
  {
    return Color.LerpUnclamped(start, end, EasingFunction(t, Parameter));
  }

  public static float Linear(float t, float parameter)
  {
    return t;
  }

  public class Quad
  {
    public static float In(float t, float parameter)
    {
      return t * t;
    }

    public static float Out(float t, float parameter)
    {
      return -(t * (t - 2));
    }

    public static float InOut(float t, float parameter)
    {
      if (t < 0.5f)
        return 2 * t * t;
      else
        return (-2 * t * t) + (4 * t) - 1;
    }
  }

  public class Power
  {
    public static float In(float t, float parameter)
    {
      return Mathf.Pow(t, parameter);
    }

    public static float Out(float t, float parameter)
    {
      return 1 - Mathf.Pow(1 - t, parameter);
    }

    public static float InOut(float t, float parameter)
    {
      if (t < 0.5f)
      {
        t *= 2;
        return In(t, parameter) / 2;
      }
      else
      {
        t = 2 * t - 1;
        return Out(t, parameter) / 2 + 0.5f;
      }
    }
  }

  public class Sin
  {
    public static float In(float t, float parameter)
    {
      return 1 - Mathf.Cos(t * Mathf.PI / 2);
    }

    public static float Out(float t, float parameter)
    {
      return Mathf.Sin(t * Mathf.PI / 2);
    }

    public static float InOut(float t, float parameter)
    {
      return (1 - Mathf.Cos(t * Mathf.PI)) / 2;
    }
  }

  public class Warp
  {
    private static readonly float MinDifferenceFromAsymptote = 7.9579e-12f;

    public static float In(float t, float parameter)
    {
      if (t > 1 - MinDifferenceFromAsymptote)
        t = 1 - MinDifferenceFromAsymptote;

      return 1 - Mathf.Pow(1 / Mathf.Cos(Mathf.PI * t / 2), parameter);
    }

    public static float Out(float t, float parameter)
    {
      if (t < MinDifferenceFromAsymptote)
        t = MinDifferenceFromAsymptote;

      return Mathf.Pow(1 / Mathf.Cos(Mathf.PI * (1 - t) / 2), parameter);
    }

    public static float InOut(float t, float parameter)
    {
      if (t < 0.5f)
      {
        if (t > 0.5f - MinDifferenceFromAsymptote)
          t = 0.5f - MinDifferenceFromAsymptote;

        t *= 2;
        return In(t, parameter) / 2;
      }
      else
      {
        if (t < 0.5f + MinDifferenceFromAsymptote)
          t = 0.5f + MinDifferenceFromAsymptote;

        t = 2 * t - 1;
        return Out(t, parameter) / 2 + 0.5f;
      }
    }
  }

  public class Elastic
  {
    private static readonly float PowerTerm = 0.0009765625f; // used for default exponent of 10
    private static readonly float DefaultParameter = 0.3f;

    public static float In(float t, float parameter)
    {
      return InP(t, DefaultParameter);
    }

    public static float Out(float t, float parameter)
    {
      return OutP(t, DefaultParameter);
    }

    public static float InOut(float t, float parameter)
    {
      return InOutP(t, DefaultParameter);
    }

    public static float InP(float t, float parameter)
    {
      if (parameter == 0) return 1;

      var exponent = 10f;
      var valueAtX = Mathf.Pow(2, exponent * (t - 1)) * Mathf.Cos(2 * Mathf.PI * (t - 1) / parameter);
      var verticalShift = PowerTerm * Mathf.Cos(2 * Mathf.PI / parameter);
      var verticalScale = 1 - verticalShift;

      return (valueAtX - verticalShift) / verticalScale;
    }

    public static float OutP(float t, float parameter)
    {
      if (parameter == 0) return 1;

      var exponent = 10f;
      var verticalScale = 1 - PowerTerm * Mathf.Cos(2 * Mathf.PI / parameter);

      return (1 - Mathf.Pow(2, -exponent * t) * Mathf.Cos(2 * Mathf.PI * t / parameter)) / verticalScale;
    }

    public static float InOutP(float t, float parameter)
    {
      if (t < 0.5f)
      {
        t *= 2;
        return In(t, parameter) / 2;
      }
      else
      {
        t = 2 * t - 1;
        return Out(t, parameter) / 2 + 0.5f;
      }
    }
  }

  public class Back
  {
    private static readonly float DefaultParameter = 1.70158f;

    public static float In(float t, float parameter)
    {
      return t * t * ((DefaultParameter + 1) * t - DefaultParameter);
    }

    public static float Out(float t, float parameter)
    {
      t -= 1;
      return t * t * ((DefaultParameter + 1) * t + DefaultParameter) + 1;
    }

    public static float InOut(float t, float parameter)
    {
      if (t < 0.5f)
      {
        t *= 2;
        return In(t, 0) / 2;
      }
      else
      {
        t = 2 * t - 1;
        return Out(t, 0) / 2 + 0.5f;
      }
    }

    public static float InP(float t, float parameter)
    {
      return t * t * ((parameter + 1) * t - parameter);
    }

    public static float OutP(float t, float parameter)
    {
      t -= 1;
      return t * t * ((parameter + 1) * t * parameter) + 1;
    }

    public static float InOutP(float t, float parameter)
    {
      if (t < 0.5f)
      {
        t *= 2;
        return In(t, parameter) / 2;
      }
      else
      {
        t = 2 * t - 1;
        return Out(t, parameter) / 2 + 0.5f;
      }
    }
  }

  public class Bounce
  {
    public static float In(float t, float parameter)
    {
      return 1 - Out(1 - t, parameter);
    }

    public static float Out(float t, float parameter)
    {
      if (t < 1.0f / 2.75f)
      {
        return 7.5625f * t * t;
      }

      if (t < 2.0f / 2.75f)
      {
        t -= 1.5f / 2.75f;
        return 7.5625f * t * t + 0.75f;
      }

      if (t < 2.5f / 2.75f)
      {
        t -= 2.25f / 2.75f;
        return 7.5625f * t * t + 0.9375f;
      }

      t -= 2.625f / 2.75f;
      return 7.5625f * t * t + 0.984375f;
    }

    public static float InOut(float t, float parameter)
    {
      if (t < 0.5f)
      {
        t *= 2;
        return In(t, parameter) / 2;
      }
      else
      {
        t = 2 * t - 1;
        return Out(t, parameter) / 2 + 0.5f;
      }
    }
  }
}
