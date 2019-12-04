using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebOptions : MonoBehaviour
{
  public bool m_UseWebDataOptions = true;
  public Transform m_TileButtonsRoot;
  public FileSystem m_FileSystem;
  public bool m_ParseDebugWebData = false;
  public string m_DebugWebData = "";

  string m_WebData = "[ Uninitialized ]";
  TextAsset[] m_Levels;


  [DllImport("__Internal")]
  private static extern string GetUrl();
  [DllImport("__Internal")]
  private static extern void DispatchUnityStartedEvent();


  private void Awake()
  {
#if !UNITY_EDITOR
    m_ParseDebugWebData = false;
#endif

    m_Levels = Resources.LoadAll<TextAsset>("Levels");
  }


  private void Start()
  {
    if (m_UseWebDataOptions && Application.platform == RuntimePlatform.WebGLPlayer)
    {
      ParseUrl();
      DispatchUnityStartedEvent();
    }
    else if (m_ParseDebugWebData)
    {
      WebDataSetup(m_DebugWebData);
    }
  }


  void ParseUrl()
  {
    var url = GetUrl();
    var urlSections = url.Split(new char[] { '?' });
    var lastSection = urlSections[urlSections.Length - 1];

    if (lastSection.ToLower().StartsWith("http"))
      return;

    WebDataSetupHelper(lastSection);
  }


  void SetButtonActiveStates(string optionsString)
  {
    var splits = optionsString.Split(new char[] { '=' });
    var codeString = splits[1];
    var tileCode = long.Parse(codeString);

    var tileButtons = new List<GameObject>();
    foreach (Transform child in m_TileButtonsRoot)
      tileButtons.Add(child.gameObject);

    for (var i = 0; i < tileButtons.Count; ++i)
    {
      var indexBit = 1L << i;
      var buttonActiveState = ((tileCode & indexBit) != 0);
      var button = tileButtons[i];
      button.SetActive(buttonActiveState);
    }
  }


  void WebDataSetup(string data)
  {
    data = data.Replace("</p><p>", " ");
    data = data.Replace("<p>", "");
    data = data.Replace("</p>", "");

    WebDataSetupHelper(data);
  }


  void WebDataSetupHelper(string data)
  {
    m_WebData = data;
    var dataLength = data.Length;
    var blocks = new List<string>();
    string currentBlock = null;

    for (var i = 0; i < dataLength; ++i)
    {
      var ch = data[i];

      switch (ch)
      {
        case '{':
          currentBlock = "";
          break;
        case '}':
          if (!string.IsNullOrWhiteSpace(currentBlock) && currentBlock != "")
            blocks.Add(currentBlock);
          currentBlock = null;
          break;
        default:
          if (currentBlock != null)
            currentBlock += ch;
          break;
      }
    }

    var statusMessage = "Web options setup complete.";

    statusMessage += ProcessDataStrings(blocks);

    StatusBar.Print(statusMessage, highPriority: true);
  }


  string ProcessDataStrings(List<string> dataStrings)
  {
    var statusMessage = "";

    foreach (var dataString in dataStrings)
    {
      var splits = dataString.Split(new char[] { ':', '=' });

      if (splits.Length < 2)
        continue;

      var typeString = splits[0].ToLower().Trim();
      var data = splits[1];

      switch (typeString)
      {
        case "tiles":
          statusMessage += TilesSetup(data);
          break;
        case "level":
          statusMessage += LoadLevel(data);
          break;
        case "editing":
          SetEditing(data);
          break;
      }
    }

    return statusMessage;
  }


  string TilesSetup(string tileTypes)
  {
    tileTypes = tileTypes.ToLower().Replace(" ", "");
    var splits = tileTypes.Split(',');
    var enabledTileTypeNames = new Dictionary<string, bool>();

    foreach (var tileName in splits)
      enabledTileTypeNames[tileName] = false;

    var tileButtons = new List<GameObject>();
    foreach (Transform child in m_TileButtonsRoot)
      tileButtons.Add(child.gameObject);

    foreach (var button in tileButtons)
    {
      var tilePicker = button.GetComponent<TilePicker>();
      var tileType = tilePicker.m_TileType;
      var typeString = tileType.ToString().ToLower();

      if (enabledTileTypeNames.ContainsKey(typeString))
      {
        button.SetActive(true);
        enabledTileTypeNames[typeString] = true;
      }
      else
      {
        button.SetActive(false);
      }
    }

    var statusMessage = "";

    if (enabledTileTypeNames.ContainsValue(false))
    {
      var notFoundTypes = "{ ";

      foreach (var pair in enabledTileTypeNames)
      {
        if (!pair.Value)
          notFoundTypes += pair.Key + " ";
      }

      notFoundTypes += "}";
      statusMessage = " The following tile names were unrecognized: " + notFoundTypes;
    }

    return statusMessage;
  }


  string LoadLevel(string levelNameString)
  {
    levelNameString = levelNameString.Trim();
    var sanitizedLevelNameString = levelNameString.ToLower().Replace(" ", "");
    var found = false;

    foreach (var level in m_Levels)
    {
      var comparisonName = level.name.ToLower().Replace(" ", "");

      if (comparisonName == sanitizedLevelNameString)
      {
        found = true;
        m_FileSystem.LoadFromTextAsset(level);

        break;
      }
    }

    var statusMessage = "";

    if (found)
      statusMessage = $" Loaded level \"{levelNameString}\".";
    else
      statusMessage = $" Couldn't find a level called \"{levelNameString}.\"";

    return statusMessage;
  }


  void SetEditing(string data)
  {
    data = data.ToLower().Replace(" ", "");

    switch (data)
    {
      case "false":
      case "no":
      case "not":
      case "negative":
      case "nothankyou":
      case "disabled":
      case "inactive":
        GlobalData.DispatchEditingDisabled();
        break;
    }
  }
}
