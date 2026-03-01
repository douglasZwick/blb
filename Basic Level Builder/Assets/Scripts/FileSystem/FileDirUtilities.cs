/***************************************************
Authors:        Brenden Epp
Last Updated:   12/16/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class FileDirUtilities : MonoBehaviour
{
  readonly static public string s_RootDirectoryName = "Basic Level Builder";
  readonly static public string s_FilenameExtension = ".blb";
  readonly static public string s_TempFilePrefix = "backup_file_";

  public GameObject m_FileItemPrefab;
  public UiListView m_SaveList;

  protected string m_CurrentDirectoryPath;
  private System.IntPtr m_WindowPtr;
  private string m_AppName;

  private void Awake()
  {
    m_AppName = Application.productName;
    m_WindowPtr = FindWindow(null, m_AppName);
    if (m_WindowPtr == System.IntPtr.Zero)
    {
      Debug.LogWarning($"Error finding application window");
    }
  }

  public void SetDirectoryName(string name)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    if (!ValidateDirectoryName(name))
    {
      // modal: something's wrong with the file name
      return;
    }

    var documentsPath = GetDocumentsPath();
    // this will never throw as long as s_RootDirectoryName is valid
    var newDirectoryPath = Path.Combine(documentsPath, s_RootDirectoryName, name);

    m_CurrentDirectoryPath = newDirectoryPath;

    UpdateFilesList();
  }

  public void UpdateFilesList()
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    if (!Directory.Exists(m_CurrentDirectoryPath))
      Directory.CreateDirectory(m_CurrentDirectoryPath);

    try
    {
      var filePaths = Directory.GetFiles(m_CurrentDirectoryPath);
      var validFilePaths = filePaths.Where(path => IsFileValid(path)).ToArray();

      if (filePaths.Length == 0)
      {
        // modal: pointing to an existing empty folder
      }
      else if (validFilePaths.Length == 0)
      {
        // modal: this folder doesn't have any BLB level files in it
      }
      else
      {
        // modal: pointing to an existing folder with stuff in it
        m_SaveList.Clear();
        SortByDateModified(validFilePaths);

        // at this point, filePaths is already sorted chronologically
        AddFileItemsForFiles(validFilePaths);

        FileItemSetSelected(FileSystem.Instance.GetMountedFilePath());
      }
    }
    catch (Exception e)
    {
      // this probably can't happen, but....
      StatusBar.Print($"Error getting files in directory. {e.Message} ({e.GetType()})");
    }
  }

  public string[] GetTempFiles()
  {
    if (GlobalData.AreEffectsUnderway())
      return null;

    if (!Directory.Exists(m_CurrentDirectoryPath))
      return null;

    try
    {
      var filePaths = Directory.GetFiles(m_CurrentDirectoryPath);

      var validFilePaths = filePaths
          .Where(path => IsValidExtension(path))
          .ToArray();

      if (validFilePaths.Length == 0)
        return null;

      var results = new ConcurrentBag<string>();

      Parallel.ForEach(validFilePaths, fullFilePath =>
      {
        if (IsTempFile(fullFilePath))
          results.Add(fullFilePath);
      });

      return results.Count == 0 ? null : results.ToArray();
    }
    catch (Exception e)
    {
      StatusBar.Print($"Error getting files in directory. {e.Message} ({e.GetType()})");
      return null;
    }
  }

  public string GetCurrentDirectoryPath()
  {
    return m_CurrentDirectoryPath;
  }

  [DllImport("user32.dll", EntryPoint = "SetWindowText")]
  public static extern bool SetWindowText(System.IntPtr hwnd, string lpString);
  [DllImport("user32.dll", EntryPoint = "FindWindow")]
  public static extern System.IntPtr FindWindow(string className, string windowName);

  public void SetTitleBarFileName(string filePath)
  {
    // If the window is found, set the new title
    if (m_WindowPtr != System.IntPtr.Zero && !FileSystem.Instance.m_IsAppQuitting)
    {
      if (string.IsNullOrEmpty(filePath))
        SetWindowText(m_WindowPtr, m_AppName);
      else
        SetWindowText(m_WindowPtr, m_AppName + " - " + Path.GetFileNameWithoutExtension(filePath));
    }
  }

  public static bool IsFileNameValid(string name)
  {
    var emptyName = name == string.Empty;
    var whiteSpaceName = string.IsNullOrWhiteSpace(name);

    if (emptyName || whiteSpaceName)
    {
      StatusBar.Print("<color=#ffff00>Entered file name is invalid.</color>");

      return false;
    }
    return true;
  }

  public void FileItemSetSelected(string fullFilePath)
  {
    m_SaveList.ItemSetSelected(fullFilePath);
  }

  public void DeselectAll()
  {
    m_SaveList.DeselectAll();
  }

  public void MoveFileItemToTop(string fullFilePath)
  {
    var item = m_SaveList.GetItemByFullFilePath(fullFilePath);
    m_SaveList.MoveToTop(item);
  }

  public void RemoveFileItem(string fullFilePath)
  {
    var element = m_SaveList.GetItemByFullFilePath(fullFilePath);
    m_SaveList.Remove(element.GetComponent<RectTransform>());
  }

  public void AddFileItemForFile(string fullFilePath)
  {
    var fileName = Path.GetFileNameWithoutExtension(fullFilePath);
    var rt = AddHelper(fullFilePath, fileName);
    m_SaveList.Add(rt);
  }

  private void AddFileItemsForFiles(string[] fullFilePaths)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var listItems = new List<RectTransform>();

    foreach (var fullFilePath in fullFilePaths)
    {
      // Path.GetFileNameWithoutExtension can only throw ArgumentException
      // for the path having invalid characters, and AddHelper will only be
      // called after ValidateDirectoryName has cleared the path
      var fileName = Path.GetFileNameWithoutExtension(fullFilePath);

      var rt = AddHelper(fullFilePath, fileName);
      listItems.Add(rt);
    }

    m_SaveList.Add(listItems);
  }

  private RectTransform AddHelper(string fullFilePath, string fileName)
  {
    var listItem = Instantiate(m_FileItemPrefab);
    UiSaveFileItem fileItem = listItem.GetComponentInChildren<UiSaveFileItem>();
    fileItem.Setup(fullFilePath, fileName, File.GetLastWriteTime(fullFilePath).ToString("g"));
    var rt = listItem.GetComponent<RectTransform>();

    return rt;
  }

  static private bool IsFileValid(string fullFilePath)
  {
    if (!IsValidExtension(fullFilePath))
      return false;
    if (!HasHeader(fullFilePath))
      return false;
    if (IsTempFile(fullFilePath))
      return false;
    return true;
  }

  static public bool IsValidExtension(string fullFilePath)
  {
    return fullFilePath.EndsWith(s_FilenameExtension);
  }

  static public bool IsTempFile(string fullFilePath)
  {
    if (!TryGetHeader(fullFilePath, out FileSystemInternal.FileHeader header))
      return false;

    // If the save file was not read properly
    if (header.m_BlbVersion == null)
    {
      string errorStr = $"Error reading save file {Path.GetFileName(fullFilePath)}. It may have been made with a different BLB version or is corrupted.";
      Debug.Log(errorStr);
      return false;
    }

    return header.m_IsTempFile;
  }

  static public bool TryGetHeader(string fullFilePath, out FileSystemInternal.FileHeader header)
  {
    IEnumerable<string> lines;
    try
    {
      lines = File.ReadLines(fullFilePath);
    }
    catch (Exception e)
    {
      string errorStr = $"Error reading save file {Path.GetFileName(fullFilePath)}. {e.Message} ({e.GetType()})";
      Debug.Log(errorStr);

      header = new();
      return false;
    }

    header = JsonUtility.FromJson<FileSystemInternal.FileHeader>(lines.First());

    // Check if one of the header values are valid to confirm if the JSON was read correctly
    return header.m_BlbVersion != null;
  }

  static public bool HasHeader(string fullFilePath)
  {
    if (!TryGetHeader(fullFilePath, out FileSystemInternal.FileHeader header))
      return false;
    return header.m_BlbVersion != null;
  }

  // Skips rename if the file already exists
  // Returns new file path
  // Expexts UpdateFilesList to be called later to update file names
  public string RenameFile(string oldFilePath, string newFileName)
  {
    string newFilePath = CreateFilePath(newFileName);

    if (File.Exists(newFilePath))
    {
      StatusBar.Print("<color=#ffff00>Entered file name already exists</color>");
      return oldFilePath;
    }

    try
    {
      File.Move(oldFilePath, newFilePath);
    }
    catch (Exception e)
    {
      Debug.Log($"Error renaming save file {oldFilePath}. {e.Message} ({e.GetType()})");
      StatusBar.Print($"<color=#ffff00>Error renaming save file {oldFilePath}</color>");
      return oldFilePath;
    }

    return newFilePath;
  }

  private bool ValidateDirectoryName(string directoryName)
  {
    if (string.IsNullOrEmpty(directoryName) || string.IsNullOrWhiteSpace(directoryName))
      return false;

    var invalidChars = Path.GetInvalidPathChars();

    if (invalidChars.Length > 0)
      return directoryName.IndexOfAny(invalidChars) < 0;
    else
      return true;
  }

  private string GetDocumentsPath()
  {
    try
    {
      switch (Application.platform)
      {
        case RuntimePlatform.OSXEditor:
        case RuntimePlatform.OSXPlayer:
          // Documents folder can not be directly pathed with the SpecialFolder,
          // so we need to find the home path and navigate to the document from there
          string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
          return Path.Combine(homePath, "Documents");

        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.WindowsEditor:
        case RuntimePlatform.LinuxPlayer:
        case RuntimePlatform.LinuxEditor:
        case RuntimePlatform.WSAPlayerX86:
        case RuntimePlatform.WSAPlayerX64:
        case RuntimePlatform.WSAPlayerARM:
          return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        default:
          return Application.persistentDataPath;
      }
    }
    catch (Exception e)
    {
      Debug.LogError($"Error getting document path. Defaulting to persistent data path. {e.Message} ({e.GetType()})");

      return Application.persistentDataPath;
    }
  }

  public static void OpenInFinder(string path)
  {
    bool openInsidesOfFolder = false;

    // try mac
    string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

    if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
    {
      openInsidesOfFolder = true;
    }

    if (!macPath.StartsWith("\""))
    {
      macPath = "\"" + macPath;
    }

    if (!macPath.EndsWith("\""))
    {
      macPath += "\"";
    }

    string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
    try
    {
      System.Diagnostics.Process.Start("open", arguments);
    }
    catch (Exception e)
    {
      e.HelpLink = ""; // do anything with this variable to silence warning about not using it

#if UNITY_EDITOR
      // EditorUtility.RevealInFinder is sure to work, but for files, it doesn't allow us to pre-select the file specified.
      // For folders, it can't open the insides of a folder, instead it will open the parent folder.
      // Very strange behavior, so we use EditorUtility.RevealInFinder only as our last resort.
      UnityEditor.EditorUtility.RevealInFinder(path);
#endif
    }
  }

  /// <summary>
  /// Creates a temporary file name with a random GUID.
  /// </summary>
  /// <returns>The full path to the temporary file</returns>
  public string CreateTempFileName()
  {
    string tempFilePath = s_TempFilePrefix + DateTime.Now.ToString("yy-M-d-HH-mm");
    tempFilePath = Path.Combine(m_CurrentDirectoryPath, tempFilePath);
    int dup = 0;
    string tempFilePathDup = tempFilePath + s_FilenameExtension;
    while (File.Exists(tempFilePathDup))
    {
      ++dup;
      tempFilePathDup = tempFilePath + $" ({dup})" + s_FilenameExtension;
    }
    return tempFilePathDup;
  }

  public string CreateFilePath(string fileName)
  {
    return Path.Combine(m_CurrentDirectoryPath, fileName + s_FilenameExtension);
  }

  /// <summary>
  /// Sorts an array of file paths by their last modified date.
  /// </summary>
  /// <param name="files">The array of file paths to sort</param>
  private void SortByDateModified(string[] files)
  {
    Array.Sort(files, DateModifiedComparison);
  }

  /// <summary>
  /// Compares two file paths by their last modified date.
  /// </summary>
  /// <param name="a">The first file path</param>
  /// <param name="b">The second file path</param>
  /// <returns>A comparison value indicating the relative order of the files</returns>
  private int DateModifiedComparison(string a, string b)
  {
    var dateTimeA = File.GetLastWriteTime(a);
    var dateTimeB = File.GetLastWriteTime(b);

    return DateTime.Compare(dateTimeA, dateTimeB);
  }

  /// <summary>
  /// Utility class for compressing and decompressing string data using GZip.
  /// </summary>
  public static class StringCompression
  {
    /// <summary>
    /// Compresses the input string data using GZip.
    /// </summary>
    /// <param name="input">The string to compress</param>
    /// <returns>The compressed data as a byte array</returns>
    public static byte[] Compress(string input)
    {
      byte[] byteArray = System.Text.Encoding.Default.GetBytes(input);

      using MemoryStream ms = new();
      using (GZipStream sw = new(ms, CompressionMode.Compress))
      {
        sw.Write(byteArray, 0, byteArray.Length);
      }
      return ms.ToArray();
    }

    /// <summary>
    /// Decompresses the input data using GZip.
    /// </summary>
    /// <param name="compressedData">The compressed data as a byte array</param>
    /// <returns>The decompressed string</returns>
    public static string Decompress(byte[] compressedData)
    {
      using MemoryStream ms = new(compressedData);
      using GZipStream sr = new(ms, CompressionMode.Decompress);
      using StreamReader reader = new(sr);
      return reader.ReadToEnd();
    }
  }
}
