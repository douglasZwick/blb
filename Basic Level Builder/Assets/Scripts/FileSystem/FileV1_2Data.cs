/***************************************************
Authors:        Brenden Epp
Last Updated:   9/5/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FileV1_3Data = NewestFileData;

namespace FileV1_2Data
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

    public class FileInfo : FileInfoInterface
    {
        // This data format supports any versions after this untill the next specified version from another data fromats oldest version
        readonly static public Version s_OldestSupportedVersion = new(1, 2, 1, 0);

        public string m_SaveFilePath;
        public FileData m_FileData;
        public FileHeader m_FileHeader;
        public override FileInfoInterface ConvertToNewest()
        {
            FileInfoInterface v1_3FileInfo = ConvertToNextVersion();
            return v1_3FileInfo.ConvertToNewest();
        }

        // Converting to v1.3
        public override FileInfoInterface ConvertToNextVersion()
        {
            FileV1_3Data.FileInfo v1_3FileInfo = new()
            {
                // Convert data to the V1.3 data class
                m_SaveFilePath = m_SaveFilePath,
                m_FileHeader = new FileV1_3Data.FileHeader(
                  m_FileHeader.m_BlbVersion.ToString(),
                  m_FileHeader.m_IsTempFile),

                m_FileData = new FileV1_3Data.FileData
                {
                    m_Description = m_FileData.m_Description,
                    m_LastId = m_FileData.m_LastId,
                    m_ManualSaves = m_FileData.m_ManualSaves
                      .Select(ConvertLevelData)
                      .ToList(),
                    m_AutoSaves = m_FileData.m_AutoSaves
                      .Select(ConvertLevelData)
                      .ToList()
                }
            };

            // Convert the tileTypes and direction enums
            foreach (var save in v1_3FileInfo.m_FileData.m_ManualSaves)
            {
                foreach (var tile in save.m_AddedTiles)
                {
                    ConvertV1_2TileTypeAndDirVToV1_3(ref tile.m_Type, ref tile.m_Direction);
                }
            }

            foreach (var save in v1_3FileInfo.m_FileData.m_AutoSaves)
            {
                foreach (var tile in save.m_AddedTiles)
                {
                    ConvertV1_2TileTypeAndDirVToV1_3(ref tile.m_Type, ref tile.m_Direction);
                }
            }

            return v1_3FileInfo;
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            if (ReadBinaryStream(reader) is not FileInfo fileInfo)
                throw new InvalidOperationException("The file data could not be converted to the newest file format.");

            m_FileData = fileInfo.m_FileData;
            m_FileHeader = fileInfo.m_FileHeader;
        }

        public new static FileInfoInterface Create(string filePath = "")
        {
            return new FileInfo()
            {
                m_SaveFilePath = filePath,
                m_FileHeader = new(),
                m_FileData = new()
            };
        }

        public new static FileInfoInterface ReadAndCreate(System.IO.BinaryReader reader, string filePath = "")
        {
            return (Create(filePath) as FileInfo).ReadBinaryStream(reader);
        }

        private FileInfoInterface ReadBinaryStream(System.IO.BinaryReader reader)
        {
            // Check if we can read this file first before continuing with the read
            // If outdated, try to read using the previous file structure
            Version blbVersion = new(reader.ReadString());
            if (blbVersion < s_OldestSupportedVersion)
            {
                throw new Exception($"Attempted to read file with unsuported version: \"{blbVersion}\"");
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

        private static LevelData ReadLevelDataBinarySteam(System.IO.BinaryReader reader)
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

        private static FileV1_3Data.LevelData ConvertLevelData(LevelData source)
        {
            return new FileV1_3Data.LevelData
            {
                m_Version = new FileV1_3Data.LevelVersion(
                    source.m_Version.m_ManualVersion,
                    source.m_Version.m_AutoVersion),
                m_Name = source.m_Name,
                m_Id = source.m_Id,
                m_CameraPos = source.m_CameraPos,
                m_Thumbnail = source.m_Thumbnail,
                m_TimeStamp = source.m_TimeStamp,
                m_AddedTiles = source.m_AddedTiles
                    .Select(ConvertElement)
                    .ToList(),
                m_RemovedTiles = new List<Vector2Int>(source.m_RemovedTiles)
            };
        }

        private static FileV1_3Data.Element ConvertElement(Element source)
        {
            return new FileV1_3Data.Element
            {
                m_GridIndex = source.m_GridIndex,
                m_Type = (FileV1_3Data.TileType)(int)source.m_Type,
                m_TileColor = (FileV1_3Data.TileColor)(int)source.m_TileColor,
                m_Direction = (FileV1_3Data.Direction)(int)source.m_Direction,
                m_GameObject = source.m_GameObject,
                m_Path = source.m_Path == null
                    ? null
                    : new List<Vector2Int>(source.m_Path)
            };
        }

        // Convert legacy slope variants into a slope tile and its equivalent direction.
        private static void ConvertV1_2TileTypeAndDirVToV1_3(ref FileV1_3Data.TileType tileTypeV1_3, ref FileV1_3Data.Direction tileDirV1_3)
        {
            TileType tileType = (TileType)(int)tileTypeV1_3;
            Direction tileDir = (Direction)(int)tileDirV1_3;

            tileDir = tileType switch
            {
                TileType.SLOPE_LEFT or TileType.BG_LEFT => (Direction)(int)FileV1_3Data.Direction.UP,
                TileType.SLOPE_RIGHT or TileType.BG_RIGHT => (Direction)(int)FileV1_3Data.Direction.RIGHT,
                TileType.SLOPE_LEFT_INV or TileType.BG_LEFT_INV => (Direction)(int)FileV1_3Data.Direction.LEFT,
                TileType.SLOPE_RIGHT_INV or TileType.BG_RIGHT_INV => (Direction)(int)FileV1_3Data.Direction.DOWN,
                _ => tileDir switch
                {
                    Direction.RIGHT => (Direction)(int)FileV1_3Data.Direction.RIGHT,
                    Direction.LEFT => (Direction)(int)FileV1_3Data.Direction.LEFT,
                    Direction.UP => (Direction)(int)FileV1_3Data.Direction.UP,
                    Direction.DOWN => (Direction)(int)FileV1_3Data.Direction.DOWN,
                    _ => Direction.RIGHT
                },
            };
            if (tileType >= TileType.SLOPE_LEFT)
            {
                if (tileType <= TileType.SLOPE_RIGHT_INV)
                {
                    tileType = TileType.SLOPE_LEFT;
                }
                else
                {
                    if (tileType >= TileType.BG_LEFT)
                    {
                        if (tileType <= TileType.BG_RIGHT_INV)
                        {
                            tileType = TileType.BG_LEFT;
                        }
                        else
                        {
                            tileType -= 3;
                        }
                    }
                    tileType -= 3;
                }
            }

            tileTypeV1_3 = (FileV1_3Data.TileType)(int)tileType;
            tileDirV1_3 = (FileV1_3Data.Direction)(int)tileDir;
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
