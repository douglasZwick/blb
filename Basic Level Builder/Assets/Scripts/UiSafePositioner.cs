using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiSafePositioner : MonoBehaviour
{
  private class PositionerData
  {
    public RectTransform m_InputWidget;
    public Vector2 m_DesiredPosition;
    public Vector2 m_Size = new Vector2(-1, -1);
  }


  RectTransform m_RectTransform;


  private void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();
  }


  public void AttachAtSafePosition(RectTransform inputWidget, Vector2 desiredPosition)
  {
    var data = new PositionerData()
    {
      m_InputWidget = inputWidget,
      m_DesiredPosition = desiredPosition,
    };

    StartCoroutine("AttachAtSafePositionCoroutine", data);
  }


  public void AttachAtSafePosition(RectTransform inputWidget, Vector2 desiredPosition, Vector2 size)
  {
    var data = new PositionerData()
    {
      m_InputWidget = inputWidget,
      m_DesiredPosition = desiredPosition,
      m_Size = size,
    };

    StartCoroutine("AttachAtSafePositionCoroutine", data);
  }


  IEnumerator AttachAtSafePositionCoroutine(PositionerData data)
  {
    yield return new WaitForEndOfFrame();

    if (data.m_InputWidget != null)
      AttachAtSafePositionHelper(data.m_InputWidget, data.m_DesiredPosition, data.m_Size);
  }


  void AttachAtSafePositionHelper(RectTransform inputWidget, Vector2 desiredPosition, Vector2 size)
  {
    inputWidget.SetParent(m_RectTransform);
    inputWidget.position = desiredPosition;

    var rootCorners = new Vector3[4];
    var inputRectCorners = new Vector3[4];
    m_RectTransform.GetWorldCorners(rootCorners);

    // check em both just cuz
    if (size.x < 0 && size.y < 0)
    {
      inputWidget.GetWorldCorners(inputRectCorners);
    }
    else
    {
      var halfWidth = size.x / 2;
      var halfHeight = size.y / 2;

      inputRectCorners[0].x = desiredPosition.x - halfWidth;
      inputRectCorners[0].y = desiredPosition.y - halfHeight;
      inputRectCorners[2].x = desiredPosition.x + halfWidth;
      inputRectCorners[2].y = desiredPosition.y + halfHeight;
    }

    var rootXMin = rootCorners[0].x;
    var rootYMin = rootCorners[0].y;
    var rootXMax = rootCorners[2].x;
    var rootYMax = rootCorners[2].y;
    var inputRectXMin = inputRectCorners[0].x;
    var inputRectYMin = inputRectCorners[0].y;
    var inputRectXMax = inputRectCorners[2].x;
    var inputRectYMax = inputRectCorners[2].y;

    var xMaxDifference = rootXMax - inputRectXMax;
    var xMinDifference = rootXMin - inputRectXMin;
    var yMaxDifference = rootYMax - inputRectYMax;
    var yMinDifference = rootYMin - inputRectYMin;

    var adjustment = Vector2.zero;

    if (xMaxDifference < 0)
    {
      adjustment.x = xMaxDifference;
    }
    else if (xMinDifference > 0)
    {
      adjustment.x = xMinDifference;
    }

    if (yMaxDifference < 0)
    {
      adjustment.y = yMaxDifference;
    }
    else if (yMinDifference > 0)
    {
      adjustment.y = yMinDifference;
    }

    //var message0 = $"Desired position: {desiredPosition}";
    //var message1 = $"ROOT  | xmin {rootXMin} | xmax {rootXMax} | ymin {rootYMin} | ymax {rootYMax}";
    //var message2 = $"RECT  | xmin {inputRectXMin} | xmax {inputRectXMax} | ymin {inputRectYMin} | ymax {inputRectYMax}";
    //var message3 = $"DIFFS | xmin {xMinDifference} | xmax {xMaxDifference} | ymin {yMinDifference} | ymax {yMaxDifference}";
    //var message4 = $"Adjustment: {adjustment}";
    //Debug.Log(message0);
    //Debug.Log(message1);
    //Debug.Log(message2);
    //Debug.Log(message3);
    //Debug.Log(message4);

    inputWidget.position = desiredPosition + adjustment;
  }
}
