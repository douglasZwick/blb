using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileV1_2Data
{
    public enum TileType
    {
        EMPTY,
        SOLID,
        SLOPE_LEFT,
        SLOPE_RIGHT,
        SLOPE_LEFT_INV,
        SLOPE_RIGHT_INV,
        START,
        DEADLY,
        GOAL,
        FALSE_SOLID,
        INVISIBLE_SOLID,
        CHECKPOINT,
        TELEPORTER,
        DOOR,
        KEY,
        COIN,
        SWITCH,
        BOOSTER,
        BG,
        BG_LEFT,
        BG_RIGHT,
        BG_LEFT_INV,
        BG_RIGHT_INV,
        MOVESTER,
        GOON,
    }

    public enum TileColor
    {
        RED,
        ORANGE,
        YELLOW,
        GREEN,
        CYAN,
        BLUE,
        PURPLE,
        MAGENTA,
    }

    public enum Direction
    {
        RIGHT,
        LEFT,
        UP,
        DOWN,
    }

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

    public class Element
    {
        public Vector2Int m_GridIndex;
        public TileType m_Type;
        public TileColor m_TileColor;
        public Direction m_Direction;
        public GameObject m_GameObject;
        public List<Vector2Int> m_Path;


        public Element() { }

        public void WriteBinary(System.IO.BinaryWriter writer)
        {
            writer.Write((short)m_GridIndex.x);
            writer.Write((short)m_GridIndex.y);
            writer.Write((byte)m_Type);
            // Write both enums as haf a byte (nibble)
            writer.Write((byte)(((byte)m_TileColor << 4) | (byte)m_Direction));

            // Make sure the path exists before trying to write it
            if (m_Path != null)
            {
                writer.Write((ushort)m_Path.Count);
                foreach (Vector2Int pos in m_Path)
                {
                    writer.Write((short)pos.x);
                    writer.Write((short)pos.y);
                }
            }
            else
            {
                writer.Write((ushort)0);
            }
        }

        public static Element ReadBinary(System.IO.BinaryReader reader)
        {
            Element element = new();

            element.m_GridIndex = new(reader.ReadInt16(), reader.ReadInt16());
            element.m_Type = (TileType)reader.ReadByte();

            byte combinedEnum = reader.ReadByte();
            element.m_TileColor = (TileColor)((combinedEnum >> 4) & 0x0F);
            element.m_Direction = (Direction)(combinedEnum & 0x0F);

            ushort pathLength = reader.ReadUInt16();
            element.m_Path = new(pathLength);
            for (ushort i = 0; i < pathLength; ++i)
            {
                element.m_Path.Add(new(reader.ReadInt16(), reader.ReadInt16()));
            }
            return element;
        }
    }


    #region FileStructure classes

    public struct FileInfo
    {
        public string m_SaveFilePath;
        // The version of the manual or autosave that is loaded
        public LevelVersion m_LoadedVersion;
        public FileData m_FileData;
        public FileHeader m_FileHeader;
    }

    public class FileHeader
    {
        public FileHeader(string ver = "", bool isTempFile = false)
        {
            m_BlbVersion = new(ver);
            m_IsTempFile = isTempFile;
        }

        public Version m_BlbVersion;
        public bool m_IsTempFile = false;
    }

    public class FileData
    {
        public FileData()
        {
            m_ManualSaves = new List<LevelData>();
            m_AutoSaves = new List<LevelData>();
            m_Description = "";
        }
        public List<LevelData> m_ManualSaves;
        public List<LevelData> m_AutoSaves;
        public uint m_LastId;
        public string m_Description;
    }

    public class LevelData
    {
        public LevelData()
        {
            m_AddedTiles = new List<Element>();
            m_RemovedTiles = new List<Vector2Int>();
            m_Name = "";
        }

        public LevelVersion m_Version;
        public string m_Name;
        public uint m_Id;
        public Vector2 m_CameraPos;
        public string m_Thumbnail;
        public DateTime m_TimeStamp;
        public List<Element> m_AddedTiles;
        public List<Vector2Int> m_RemovedTiles;
    }

    public class ThumbnailTile
    {
        public Color[] m_ColorData;

        public ThumbnailTile(TileType tileType, Color[] atlasBuffer,
          int atlasWidth, Vector2Int tileSize)
        {
            m_ColorData = new Color[tileSize.x * tileSize.y];
            // The buffer contains the pixel colors starting from the bottom-left
            //   corner of the texture. I believe it then goes to the right, and when
            //   it reaches the end of a row, it goes back to the start of the next
            //   row up from there.
            // startIndex is the index of the bottom-left pixel in the tile.
            var startIndex = (int)tileType * tileSize.x;
            // This is the index for writing to the thumbnail tile, incremented
            //   manually in the loop, so it's separate from the x and y indices that
            //   are only used to read from the atlas.
            var dataIndex = 0;

            for (var y = 0; y < tileSize.y; ++y)
            {
                for (var x = 0; x < tileSize.x; ++x)
                {
                    var color = atlasBuffer[startIndex + x + y * atlasWidth];
                    m_ColorData[dataIndex] = color;
                    ++dataIndex;
                }
            }
        }
    }
    #endregion

    public static FileInfo Read(BinaryReader reader, string filePath)
    {
        FileInfo fileInfo = new()
        {
            m_SaveFilePath = filePath,
            m_FileHeader = new(),
            m_FileData = new()
        };

        ReadBinaryStream(reader, ref fileInfo);

        return fileInfo;
    }

    private static void ReadBinaryStream(BinaryReader reader, ref FileInfo fileInfo)
    {
        // Write file version first so we can later check for future file changes and adapt
        fileInfo.m_FileHeader.m_BlbVersion = new(reader.ReadString());
        // TODO, from here add check to see if the file is new or old and how to proceed with the read
        fileInfo.m_FileHeader.m_IsTempFile = reader.ReadBoolean();
        fileInfo.m_FileData.m_Description = reader.ReadString();

        ushort count = reader.ReadUInt16();
        fileInfo.m_FileData.m_ManualSaves = new(count);
        for (ushort i = 0; i < count; ++i)
        {
            fileInfo.m_FileData.m_ManualSaves.Add(ReadLevelDataBinarySteam(reader));
            fileInfo.m_FileData.m_ManualSaves[i].m_Id = i;
        }

        // Start the id count at the last manual save id for the auto saves
        uint id = (uint)fileInfo.m_FileData.m_ManualSaves.Count;

        count = reader.ReadUInt16();
        fileInfo.m_FileData.m_AutoSaves = new(count);
        for (ushort i = 0; i < count; ++i)
        {
            fileInfo.m_FileData.m_AutoSaves.Add(ReadLevelDataBinarySteam(reader));
            fileInfo.m_FileData.m_AutoSaves[i].m_Id = id + i;
        }

        fileInfo.m_FileData.m_LastId = id + (uint)fileInfo.m_FileData.m_AutoSaves.Count - 1;
    }

    private static LevelData ReadLevelDataBinarySteam(BinaryReader reader)
    {
        LevelData levelData = new()
        {
            m_Version = LevelVersion.ReadBinary(reader),
            m_Name = reader.ReadString(),
            m_CameraPos = new(reader.ReadInt16(), reader.ReadInt16()),
            m_Thumbnail = reader.ReadString(),
            m_TimeStamp = new(reader.ReadInt64()),
        };

        ushort count = reader.ReadUInt16();
        levelData.m_AddedTiles = new(count);
        for (ushort i = 0; i < count; ++i)
        {
            Element element = Element.ReadBinary(reader);
            levelData.m_AddedTiles.Add(element);
        }

        count = reader.ReadUInt16();
        levelData.m_RemovedTiles = new(count);
        for (ushort i = 0; i < count; ++i)
        {
            levelData.m_RemovedTiles.Add(new(reader.ReadInt16(), reader.ReadInt16()));
        }

        return levelData;
    }

    public static FileSystemInternal.FileInfo ConvertToV1_3(FileInfo v1_2FileInfo)
    {
        foreach (var save in v1_2FileInfo.m_FileData.m_ManualSaves)
        {
            foreach (var tile in save.m_AddedTiles)
            {
                ConvertV1_2TileTypeAndDirVToV1_3(ref tile.m_Type, ref tile.m_Direction);
            }
        }

        foreach (var save in v1_2FileInfo.m_FileData.m_AutoSaves)
        {
            foreach (var tile in save.m_AddedTiles)
            {
                ConvertV1_2TileTypeAndDirVToV1_3(ref tile.m_Type, ref tile.m_Direction);
            }
        }

        // At this point both filedata types are exactly the same so, we can reinterpret the memory as the current versions data type
        return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<FileInfo, FileSystemInternal.FileInfo>(ref v1_2FileInfo); // Direct memory reinterpretation
    }

    static private void ConvertV1_2TileTypeAndDirVToV1_3(ref TileType tileType, ref Direction tileDir)
    {
        int tileTypeInt = (int)tileType;
        int tileDirInt = (int)tileDir;
        int oldRot = 0;

        // If the tile is a slope
        if (tileTypeInt >= 2 && tileTypeInt <= 5)
        {
            oldRot = tileTypeInt - 2;
            // Use magic number just incase the enum changes
            tileTypeInt = 2;
        }

        // If the tile is a bg slope
        if (tileTypeInt >= 19 && tileTypeInt <= 22)
        {
            oldRot = tileTypeInt - 19;
            // Use magic number just incase the enum changes
            tileTypeInt = 16;
        }

        tileDirInt = oldRot switch
        {
            0 => 0,
            1 => 3,
            2 => 1,
            3 => 2,
            _ => tileDirInt
        };

        tileType = (TileType)tileTypeInt;
        tileDir = (Direction)tileDirInt;
    }
}
