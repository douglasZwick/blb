/***************************************************
Authors:        Brenden Epp
Last Updated:   7/09/2025

Description:
  Contains functionality for modifying and comparing
  save versions

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FileSystemInternal;

public static class LevelVersioning
{
  public struct LevelVersion
  {
    public LevelVersion(int manual = 0, int Auto = 0)
    {
      m_ManualVersion = manual;
      m_AutoVersion = Auto;
    }

    public readonly bool IsManual()
    {
      return m_AutoVersion == 0;
    }

    public readonly void WriteBinary(System.IO.BinaryWriter writer)
    {
      writer.Write((ushort)m_ManualVersion);
      writer.Write((ushort)m_AutoVersion);
    }

    public static LevelVersion ReadBinary(System.IO.BinaryReader reader)
    {
      return new()
      {
        m_ManualVersion = reader.ReadUInt16(),
        m_AutoVersion = reader.ReadUInt16()
      };
    }

    public override readonly string ToString()
    {
      return $"Save version: Manual {m_ManualVersion}, Auto {m_AutoVersion}"; // Using string interpolation for a readable output
    }

    public readonly bool Equals(LevelVersion rhs)
    {
      return m_ManualVersion == rhs.m_ManualVersion && m_AutoVersion == rhs.m_AutoVersion;
    }

    public static bool operator ==(LevelVersion left, LevelVersion right)
    {
      return left.Equals(right); // Delegate to Equals method
    }

    public static bool operator !=(LevelVersion left, LevelVersion right)
    {
      return !(left == right);
    }

    public override bool Equals(object obj)
    {
      return obj is LevelVersion other && Equals(other);
    }

    // Override GetHashCode
    public override int GetHashCode()
    {
      return m_ManualVersion.GetHashCode() + m_AutoVersion.GetHashCode();
    }

    public readonly int CompareTo(LevelVersion other)
    {
      // Sorts Largest to Smallest/Top to Bottom
      // -# = This goes up
      // +# = This goes down
      // == This stays

      int diff = other.m_ManualVersion - m_ManualVersion;

      // If they are the same maunal save, one (or both) of them is an autosave.
      if (diff == 0)
      {
        // Sort the auto saves to have the newest on top
        diff = other.m_AutoVersion - m_AutoVersion;

        // If either werer a manaul save, we need to put that on top
        if (other.m_AutoVersion == 0)
          diff = 1;
        if (m_AutoVersion == 0)
          diff = -1;
      }

      return diff;
    }

    // The version of the manaul save, or maunal the auto is branched off of
    public int m_ManualVersion;
    // The autosave version, 0 if not an autosave
    public int m_AutoVersion;
  }

  public static bool IsCameraDifferent(FileData fileData, LevelVersion version)
  {
    // If this is the first manual save the camera has to be "different"
    if (version.m_ManualVersion <= 1 && version.m_AutoVersion == 0)
      return true;
    
    GetVersionLevelData(fileData, version, out LevelData targetData);

    // Find the previous versions number
    int v;
    // If we are a manual get the previouse manuals number, otherwise we will compare to this auto saves manual
    if (version.IsManual())
    {
      v = GetPreviousManualVersion(fileData, version.m_ManualVersion);
      // 0 means no prev version was found, so the camera is different by default
      if (v == 0)
        return true;
    }
    else
    {
      // If we are an auto, just compare againt the manual we are branched off of
      v = version.m_ManualVersion;
    }
    
    GetVersionLevelData(fileData, new LevelVersion(v, 0), out LevelData previousData);

    return targetData.m_CameraPos != previousData.m_CameraPos;
  }

  public static bool GetDifferences(out LevelData differences, FileInfo fileInfo, Dictionary<Vector2Int, TileGrid.Element> gridDictionary, LevelVersion? version = null)
  {
    Dictionary<Vector2Int, TileGrid.Element> oldGrid = GetGridDictionaryFromFileData(fileInfo, version);

    return GetDifferencesEx(out differences, oldGrid, gridDictionary);
  }

  public static bool GetVersionDifferences(out LevelData differences, FileInfo fileInfo, LevelVersion from, LevelVersion to)
  {
    Dictionary<Vector2Int, TileGrid.Element> oldGrid = GetGridDictionaryFromFileData(fileInfo, from);
    Dictionary<Vector2Int, TileGrid.Element> newGrid = GetGridDictionaryFromFileData(fileInfo, to);

    return GetDifferencesEx(out differences, oldGrid, newGrid);
  }

  private static bool GetDifferencesEx(out LevelData differences, Dictionary<Vector2Int, TileGrid.Element> oldGrid, Dictionary<Vector2Int, TileGrid.Element> newGrid)
  {
    differences = new();

    bool hasDifferences = false;

    foreach (var kvp in newGrid)
    {
      Vector2Int position = kvp.Key;
      TileGrid.Element currentElement = kvp.Value;

      if (oldGrid.TryGetValue(position, out TileGrid.Element oldElement))
      {
        bool same = currentElement.Equals(oldElement);

        // Removed element so we don't check it again in the next loop
        oldGrid.Remove(position);

        if (same)
          continue;
      }
      differences.m_AddedTiles.Add(currentElement);
      hasDifferences = true;
    }

    // Every tile left in the old grid will be removed
    foreach (var kvp in oldGrid)
    {
      differences.m_RemovedTiles.Add(kvp.Key);
      hasDifferences = true;
    }

    return hasDifferences;
  }

  // Will convert the level data to a Dictionary of elements up to the passed in version
  // If no version is passed in, we will flatten to the latest version
  public static Dictionary<Vector2Int, TileGrid.Element> GetGridDictionaryFromFileData(FileInfo fileInfo, LevelVersion? tempVersion = null)
  {
    // Sets the default value if no version is specified
    LevelVersion version = tempVersion ?? new(int.MaxValue, 0);

    Dictionary<Vector2Int, TileGrid.Element> tiles = new();

    // Load the version up the the specified manual save
    foreach (var levelData in fileInfo.m_FileData.m_ManualSaves)
    {
      // Stop flattening the level once we pass the version we want
      if (levelData.m_Version.m_ManualVersion > version.m_ManualVersion)
        break;

      AddLevelDeltasToGrid(ref tiles, levelData);
    }

    // If we are loading a autosave, load the branch now
    if (!version.IsManual())
    {
      try
      {
        // Find the level data from the auto save version
        GetVersionLevelData(fileInfo.m_FileData, version, out LevelData autoSaveData);

        AddLevelDeltasToGrid(ref tiles, autoSaveData);
      }
      catch (InvalidOperationException)
      {
        Debug.Log($"Couldn't find {version} in file `{fileInfo.m_SaveFilePath}");
        FileSystem.Instance.MainThreadDispatcherQueue(() => StatusBar.Print("Error, couldn't find the proper save to load. Loaded branched manual instead."));
        // Just return the tiles we've loaded so far (the manual save)
      }
    }

    return tiles;
  }

  // Combines two versions level data
  // Add the level data from "from" to "to"
  // Returns the combine data
  // Note: The passed in versions should be right after eachother or else the deltas might not be correct
  public static LevelData FlattenLevelData(LevelData to, LevelData from)
  {
    Dictionary<Vector2Int, TileGrid.Element> flattenedLevelAdd = new();
    HashSet<Vector2Int> flattenedLevelRemove = new();

    FlattenLevelDataAdder(ref flattenedLevelAdd, ref flattenedLevelRemove, from);
    FlattenLevelDataAdder(ref flattenedLevelAdd, ref flattenedLevelRemove, to);

    to.m_AddedTiles = flattenedLevelAdd.Values.ToList();
    to.m_RemovedTiles = flattenedLevelRemove.ToList();

    return to;
  }

  // Adds the level datas deltas to an add/removed tiles arrays
  private static void FlattenLevelDataAdder(ref Dictionary<Vector2Int, TileGrid.Element> flattenedLevelAdd, ref HashSet<Vector2Int> flattenedLevelRemove, LevelData addedData)
  {
    foreach (var tile in addedData.m_AddedTiles)
    {
      // Add the tile to the list
      // If we had record to remove it earlier, remove the record
      if (flattenedLevelRemove.Contains(tile.m_GridIndex))
        flattenedLevelRemove.Remove(tile.m_GridIndex);
      flattenedLevelAdd[tile.m_GridIndex] = tile;
    }
    foreach (var pos in addedData.m_RemovedTiles)
    {
      // Remove a tile if we have one
      if (flattenedLevelAdd.ContainsKey(pos))
        flattenedLevelAdd.Remove(pos);
      // Keep the remove in the list even if we deleted a tile, because the tile could be replacing a previously placed tile and we need to delete that too
      flattenedLevelRemove.Add(pos);
    }
  }

  // Promotes selected auto versions and removes all non selected manual/auto version from the data
  public static void ExtractSelectedVersions(ref FileData fileData, List<LevelVersion> versions)
  {
    // Promote all selected autos to manuals
    // This function will ignore passed in manual versions so we don't need to remove them from the list
    PromoteMultipleAutoSavesEx(ref fileData, versions, false);
    fileData.m_AutoSaves.Clear();

    Dictionary<Vector2Int, TileGrid.Element> flattenedLevelAdd = new();
    HashSet<Vector2Int> flattenedLevelRemove = new();

    int version = 1;
    fileData.m_LastId = 0;

    for (int i = 0; i < fileData.m_ManualSaves.Count; ++i)
    {
      FlattenLevelDataAdder(ref flattenedLevelAdd, ref flattenedLevelRemove, fileData.m_ManualSaves[i]);

      // Remove if we are not extracting this version
      if (!versions.Contains(fileData.m_ManualSaves[i].m_Version))
      {
        fileData.m_ManualSaves.RemoveAt(i);
        --i;
        continue;
      }

      // Update tiles just in case the detas got flattened
      fileData.m_ManualSaves[i].m_AddedTiles = flattenedLevelAdd.Values.ToList();
      fileData.m_ManualSaves[i].m_RemovedTiles = flattenedLevelRemove.ToList();
      fileData.m_ManualSaves[i].m_Version = new LevelVersion(version++, 0);
      fileData.m_ManualSaves[i].m_Id = ++fileData.m_LastId;

      flattenedLevelAdd.Clear();
      flattenedLevelRemove.Clear();
    }
  }

  // Promotes multiple autosaves to be a manual save, leaf to branch.
  // NOTE: Does not save file
  public static void PromoteMultipleAutoSavesEx(ref FileData fileData, List<LevelVersion> versions, bool updateVersions = true)
  {
    foreach (LevelData level in fileData.m_AutoSaves.AsEnumerable().Reverse())
    {
      // If this levels version doesn't match any versions we want to export, then skip it
      if (!versions.Any(item => item == level.m_Version))
        continue;

      PromoteAutoSaveEx(ref fileData, level, updateVersions);
    }
  }

  // Promotes an autosave to be a manual save, leaf to branch.
  // NOTE: Does not save file
  public static void PromoteAutoSaveEx(ref FileData fileData, LevelData level, bool updateVersions = true)
  {
    Dictionary<Vector2Int, TileGrid.Element> autosGrid = new();

    LevelVersion autoSaveVersion = level.m_Version;

    for (int i = 0; i < fileData.m_ManualSaves.Count; ++i)
    {
      LevelData currentManual = fileData.m_ManualSaves[i];

      AddLevelDeltasToGrid(ref autosGrid, currentManual);

      if (currentManual.m_Version.m_ManualVersion < autoSaveVersion.m_ManualVersion)
        continue;

      // We added the auto save, push forward the version numbers for all the future versions
      if (currentManual.m_Version.m_ManualVersion > autoSaveVersion.m_ManualVersion)
      {
        ++currentManual.m_Version.m_ManualVersion;
        continue;
      }

      // Remove the autosave we are promoting before modifieing it and adding it to the manual list
      fileData.m_AutoSaves.Remove(level);

      // We are on our autos manual version
      // If we have more manuals after this
      if (fileData.m_ManualSaves.Count > (i + 1))
      {
        // Add the new delta to the next manual so it effectivly removes the promoted autosaves deltas
        Dictionary<Vector2Int, TileGrid.Element> nextGrid = new(autosGrid);

        AddLevelDeltasToGrid(ref nextGrid, fileData.m_ManualSaves[i + 1]);
        AddLevelDeltasToGrid(ref autosGrid, level);

        GetDifferencesEx(out LevelData differences, autosGrid, nextGrid);

        fileData.m_ManualSaves[i + 1].m_AddedTiles = differences.m_AddedTiles;
        fileData.m_ManualSaves[i + 1].m_RemovedTiles = differences.m_RemovedTiles;

        // Promote auto save
        if (updateVersions)
        {
          level.m_Version = currentManual.m_Version;
          level.m_Version.m_ManualVersion += 1;
        }
        // Insert auto into the slot right after its manual
        fileData.m_ManualSaves.Insert(i + 1, level);

        // If we aren't updateing any of the levels versions we can stop here
        if (!updateVersions)
          return;

        // Add one to i so we skip the added auto when continueing the loop
        ++i;

        // Push forward the manual version for all the autosaves after this
        foreach (LevelData auto in fileData.m_AutoSaves)
        {
          if (auto.m_Version.m_ManualVersion > currentManual.m_Version.m_ManualVersion)
            ++auto.m_Version.m_ManualVersion;
        }
      }
      else
      {
        // Promote auto and we don't need to worry about anything else
        if (updateVersions)
        {
          level.m_Version = currentManual.m_Version;
          level.m_Version.m_ManualVersion += 1;
        }
        fileData.m_ManualSaves.Add(level);
        // We added one more manual so the loop will continue, but we are done, so return
        return;
      }
    }
  }

  private static void AddLevelDeltasToGrid(ref Dictionary<Vector2Int, TileGrid.Element> tiles, LevelData level)
  {
    foreach (var tile in level.m_AddedTiles)
    {
      tiles[tile.m_GridIndex] = tile;
    }
    foreach (var pos in level.m_RemovedTiles)
    {
      tiles.Remove(pos);
    }
  }

  public static void DeleteVersionEx(FileInfo fileInfo, LevelVersion version)
  {
    if (!FileDataExists(fileInfo.m_FileData))
      throw new Exception("No file data exists to delete version");

    if (version.IsManual())
    {
      // Loop to find our manual save
      for (int i = 0; i < fileInfo.m_FileData.m_ManualSaves.Count; ++i)
      {
        if (fileInfo.m_FileData.m_ManualSaves[i].m_Version != version)
          continue;

        // If this is the first manaul on the list, ie: no newer manual exists
        // We don't need to combine versions and can just delete this version
        if (i == fileInfo.m_FileData.m_ManualSaves.Count - 1)
        {
          fileInfo.m_FileData.m_ManualSaves.RemoveAt(i);
        }
        else
        {
          // Combine deltas and overwrite the newer version with the flattened data
          fileInfo.m_FileData.m_ManualSaves[i + 1] = FlattenLevelData(fileInfo.m_FileData.m_ManualSaves[i + 1], fileInfo.m_FileData.m_ManualSaves[i]);
          fileInfo.m_FileData.m_ManualSaves.RemoveAt(i);
        }

        DeleteBranchedAutoSaves(fileInfo, version.m_ManualVersion);
        return;
      }
    }
    else
    {
      // Find auto save version
      for (int i = 0; i < fileInfo.m_FileData.m_AutoSaves.Count; ++i)
      {
        if (fileInfo.m_FileData.m_AutoSaves[i].m_Version == version)
        {
          fileInfo.m_FileData.m_AutoSaves.RemoveAt(i);
          return;
        }
      }
    }

    throw new Exception($"Couldn't find {version} to delete");
  }

  // Deletes all autosave off a versions branch
  private static void DeleteBranchedAutoSaves(FileInfo fileInfo, int version)
  {
    if (!FileDataExists(fileInfo.m_FileData))
      throw new Exception("No file data exists to delete version");

    for (int i = 0; i < fileInfo.m_FileData.m_AutoSaves.Count; ++i)
    {
      if (fileInfo.m_FileData.m_AutoSaves[i].m_Version.m_ManualVersion == version)
      {
        fileInfo.m_FileData.m_AutoSaves.RemoveAt(i);
        --i;
      }
    }
  }

  public static FileInfo SetVersionNameEx(string fullFilePath, LevelVersion version, string name)
  {
    FileSystem.Instance.GetFileInfoFromFullFilePath(fullFilePath, out FileInfo fileInfo);
    List<LevelData> levelList = version.IsManual() ? fileInfo.m_FileData.m_ManualSaves : fileInfo.m_FileData.m_AutoSaves;

    var data = levelList.Find(d => d.m_Version == version);
    if (data != null)
    {
      data.m_Name = name;
      return fileInfo;
    }

    throw new InvalidOperationException($"{version} not found");
  }

  public static void GetVersionLevelData(FileData fileData, LevelVersion version, out LevelData levelData)
  {
    if (fileData == null)
      throw new InvalidOperationException("File data is null");

    levelData = new();

    List<LevelData> levelList = version.IsManual() ? fileData.m_ManualSaves : fileData.m_AutoSaves;
    foreach (var data in levelList)
    {
      if (data.m_Version == version)
      {
        levelData = data;
        return;
      }
    }

    throw new InvalidOperationException($"{version} can not found");
  }

  // Finds the newest autosave from a manual save version
  // Returns 0 if no versions were found
  public static int GetPreviousManualVersion(FileData fileData, int manualVersion)
  {
    int closestPrevVersion = 0;
    foreach (var data in fileData.m_ManualSaves)
    {
      if (data.m_Version.m_ManualVersion > closestPrevVersion && data.m_Version.m_ManualVersion < manualVersion)
        closestPrevVersion = data.m_Version.m_ManualVersion;
    }
    return closestPrevVersion;
  }

  // Finds the newest autosave from a manual save version
  // Returns 0 if no versions were found
  public static int GetLastAutoSaveVersion(FileData fileData, int manualVersion)
  {
    int lastVersion = 0;
    foreach (var data in fileData.m_AutoSaves)
    {
      if (data.m_Version.m_ManualVersion == manualVersion && data.m_Version.m_AutoVersion > lastVersion)
        lastVersion = data.m_Version.m_AutoVersion;
    }
    return lastVersion;
  }

  // Finds the newest manual save version
  // Returns 0 if no versions were found
  public static int GetLastManualSaveVersion(FileData fileData)
  {
    int lastVersion = 0;
    foreach (var data in fileData.m_ManualSaves)
    {
      if (data.m_Version.m_ManualVersion > lastVersion)
        lastVersion = data.m_Version.m_ManualVersion;
    }
    return lastVersion;
  }

  // Finds the newest manual save data
  // Returns null if no versions were found
  public static LevelData GetLastManualSaveData(FileData fileData)
  {
    if (fileData.m_ManualSaves.Count == 0)
      return null;
    
    int lastVersion = 0;
    int index = -1;
    foreach (var data in fileData.m_ManualSaves)
    {
      ++index;
      if (data.m_Version.m_ManualVersion > lastVersion)
        lastVersion = data.m_Version.m_ManualVersion;
    }

    return fileData.m_ManualSaves[index];
  }

  public static Sprite GetThumbnailSprite(LevelData levelData)
  {
    byte[] bytes = Convert.FromBase64String(levelData.m_Thumbnail);
    Texture2D tex = new(0, 0, TextureFormat.RGBA32, false) // No real reason for the width/height values in the constructor, they will be overwritten anyways in LoadImage
    {
      filterMode = FilterMode.Point
    };

    tex.LoadImage(bytes);

    return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f) // pivot in the center
        );
  }
}
