using UnityEngine;

public class HideIfAttribute : PropertyAttribute
{
  public string boolField;
  public bool hideWhenTrue;

  public HideIfAttribute(string boolField, bool hideWhenTrue = true)
  {
    this.boolField = boolField;
    this.hideWhenTrue = hideWhenTrue;
  }
}
