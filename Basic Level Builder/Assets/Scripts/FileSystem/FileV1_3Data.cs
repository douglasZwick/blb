/***************************************************
Authors:        Brenden Epp
Last Updated:   9/5/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace NewestFileData
{
    public enum TileType
    {
        EMPTY,
        SOLID,
        SLOPE,
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
        BG_SLOPE,
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
        DOWN,
        LEFT,
        UP,
    }

    public enum DirectionType
    {
        ORTHOGONAL,
        UP_DOWN,
        LEFT_RIGHT,
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

    public class Element : ICloneable
    {
        public Vector2Int m_GridIndex;
        public TileType m_Type;
        public TileColor m_TileColor;
        public Direction m_Direction;
        public GameObject m_GameObject;
        public List<Vector2Int> m_Path;


        public Element() { }

        public Element(Vector2Int gridIndex, TileState state, GameObject gameObject)
        {
            m_GridIndex = gridIndex;
            m_Type = state.Type;
            m_TileColor = state.Color;
            m_Direction = state.Direction;
            m_Path = state.Path;
            m_GameObject = gameObject;

            if (gameObject.TryGetComponent<ColorCode>(out var colorCode))
                colorCode.m_Element = this;
        }

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

        public bool Equals(Element other)
        {
            if (m_GridIndex != other.m_GridIndex)
                return false;
            if (m_Type != other.m_Type)
                return false;
            if (m_TileColor != other.m_TileColor)
                return false;
            if (m_Direction != other.m_Direction)
                return false;
            if (!PathsEqual(other))
                return false;
            return true;
        }

        public bool PathsEqual(Element other)
        {
            // If both paths don't exist or the paths are equal
            if ((other.m_Path == null && m_Path == null) ||
              (other.m_Path != null && m_Path != null && m_Path.SequenceEqual(other.m_Path)))
            {
                return true;
            }
            return false;
        }

        public void SetState(TileState state)
        {
            m_Type = state.Type;
            m_TileColor = state.Color;
            m_Direction = state.Direction;
            m_Path = state.Path;
        }

        public TileState ToState()
        {
            return new TileState()
            {
                Type = m_Type,
                Color = m_TileColor,
                Direction = m_Direction,
                Path = m_Path,
            };
        }


        public T GetComponent<T>() where T : Component
        {
            if (m_GameObject == null)
                return null;

            return m_GameObject.GetComponent<T>();
        }

        public object Clone()
        {
            return new Element
            {
                m_GridIndex = m_GridIndex,
                m_Type = m_Type,
                m_TileColor = m_TileColor,
                m_Direction = m_Direction,
                m_Path = m_Path != null ? new List<Vector2Int>(m_Path) : null,
                m_GameObject = null // intentionally not cloned
            };
        }
    }

    #region FileStructure classes

    public class FileInfo : FileInfoInterface
    {
        // This data format supports any versions after this untill the next specified version from another data fromats oldest version
        readonly static private Version s_OldestSupportedVersion = new(1, 3, 0, 0);

        public string m_SaveFilePath;
        // The version of the manual or autosave that is loaded
        public LevelVersion m_LoadedVersion;
        public FileData m_FileData;
        public FileHeader m_FileHeader;

        public override FileInfoInterface ConvertToNewest()
        {
            return this;
        }

        public override FileInfoInterface ConvertToNextVersion()
        {
            return this;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            _ = ReadBinaryStream(reader);
        }

        public new static FileInfoInterface ReadAndCreate(System.IO.BinaryReader reader)
        {
            FileInfo fileInfo = new()
            {
                m_FileHeader = new(),
                m_FileData = new()
            };

            return fileInfo.ReadBinaryStream(reader);
        }

        private FileInfoInterface ReadBinaryStream(System.IO.BinaryReader reader)
        {
            // Check if we can read this file first before continuing with the read
            // If outdated, try to read using the previous file structure
            Version blbVersion = new(reader.ReadString());
            if (blbVersion < s_OldestSupportedVersion)
            {
                // Reset the reader so the other readers can reread the file
                reader.BaseStream.Position = 0;
                FileInfoInterface v1_2FileInfo = FileV1_2Data.FileInfo.ReadAndCreate(reader);
                return v1_2FileInfo.ConvertToNewest();
            }


            // Write file version first so we can later check for future file changes and adapt
            m_FileHeader.m_BlbVersion = blbVersion;
            // TODO, from here add check to see if the file is new or old and how to proceed with the read
            m_FileHeader.m_IsTempFile = reader.ReadBoolean();
            m_FileData.m_Description = reader.ReadString();

            ushort count = reader.ReadUInt16();
            m_FileData.m_ManualSaves = new(count);
            for (ushort i = 0; i < count; ++i)
            {
                m_FileData.m_ManualSaves.Add(ReadLevelDataBinarySteam(reader));
                m_FileData.m_ManualSaves[i].m_Id = i;
            }

            // Start the id count at the last manual save id for the auto saves
            uint id = (uint)m_FileData.m_ManualSaves.Count;

            count = reader.ReadUInt16();
            m_FileData.m_AutoSaves = new(count);
            for (ushort i = 0; i < count; ++i)
            {
                m_FileData.m_AutoSaves.Add(ReadLevelDataBinarySteam(reader));
                m_FileData.m_AutoSaves[i].m_Id = id + i;
            }

            m_FileData.m_LastId = id + (uint)m_FileData.m_AutoSaves.Count - 1;
            return this;
        }

        private LevelData ReadLevelDataBinarySteam(System.IO.BinaryReader reader)
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
    }

    public class FileHeader
    {
        public FileHeader(string ver = "0.0.0.0", bool isTempFile = false)
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
}
