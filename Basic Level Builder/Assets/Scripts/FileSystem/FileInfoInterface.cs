using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FileInfoInterface
{
    public abstract void ConvertToNext(ref FileInfoInterface nextFileInfo);

    public abstract void ReadAndConvertToNext();
}
