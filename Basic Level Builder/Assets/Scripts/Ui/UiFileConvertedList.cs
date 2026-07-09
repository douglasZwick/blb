/***************************************************
Authors:        Brenden Epp
Last Updated:   7/1/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using UnityEngine;

public class UiFileConvertedList : UiGenericModalDialog
{
    [SerializeField]
    private TMPro.TextMeshProUGUI m_ConvertedFilesText;
    [SerializeField]
    private RectTransform m_ConvertedFilesListContent;
    [SerializeField]
    private UiConvertedFilePreviewItem m_ConvertedFilesPreviewPrefab;
    [SerializeField]
    private GameObject m_UnconvertedFilesPreviewPrefab;

    public void InitializeConvertedFilesList(List<string> convertedFiles, List<string> corruptedFiles)
    {
        m_ConvertedFilesText.text = CreateTitleString(convertedFiles.Count, corruptedFiles.Count);

        foreach (string fullFilePath in convertedFiles)
        {
            Instantiate(m_ConvertedFilesPreviewPrefab, m_ConvertedFilesListContent).Init(fullFilePath);
        }

        foreach (string fullFilePath in corruptedFiles)
        {
            GameObject preview = Instantiate(m_UnconvertedFilesPreviewPrefab, m_ConvertedFilesListContent);
            preview.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = CreateUnconvertedFileString(fullFilePath);
        }
    }

    private string CreateTitleString(int convertedFilesCount, int corruptedFilesCount)
    {
        string titleString = "";
        if (convertedFilesCount > 0)
            titleString = convertedFilesCount + " outdated level file(s) were successfully converted" + System.Environment.NewLine;
        if (corruptedFilesCount > 0)
            titleString += "<color=red>" + corruptedFilesCount + " outdated level file(s) were unable to be converted</color>" + System.Environment.NewLine;
        titleString += System.Environment.NewLine + "<size=15>Original files were kept, with \".old\" added their file names.</size>";

        return titleString;
    }

    private string CreateUnconvertedFileString(string fileName)
    {
        return "<color=red><b>Not Converted</color></b>" + System.Environment.NewLine + fileName;
    }
}
