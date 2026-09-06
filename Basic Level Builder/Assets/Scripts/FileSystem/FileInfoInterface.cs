/***************************************************
Authors:        Brenden Epp
Last Updated:   9/5/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

public abstract class FileInfoInterface
{
    public abstract FileInfoInterface ConvertToNewest();
    public abstract FileInfoInterface ConvertToNextVersion();
    public abstract void Read(System.IO.BinaryReader reader);
    public static FileInfoInterface ReadAndCreate(System.IO.BinaryReader reader)
    {
        return null;
    }
}
