using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[AddComponentMenu("UI/TextExtension",0)]
public class UITextExtension : Text
{
    #region Localization
    public bool B_AutoLocalize = false;
    public string m_LocalizeKey;
    protected override void OnEnable()
    {
        base.OnEnable();
        TLocalization.OnLocaleChanged += OnKeyLocalize;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        TLocalization.OnLocaleChanged -= OnKeyLocalize;
    }
    protected override void Start()
    {
        base.Start();
        if (TLocalization.IsInit)
            OnKeyLocalize();
    }

    void OnKeyLocalize()
    {
        if (B_AutoLocalize)
            text = TLocalization.GetLocalizeValue(m_LocalizeKey);
    }

    public string formatText(string formatKey, params object[] subItems) => base.text = string.Format(TLocalization.GetLocalizeValue(formatKey), subItems);
    public string formatKey(string formatKey, string subKey) => base.text = string.Format(TLocalization.GetLocalizeValue(formatKey), TLocalization.GetLocalizeValue(subKey));

    public string localizeKey
    {
        set
        {
            m_LocalizeKey = value;
            B_AutoLocalize = true;
            OnKeyLocalize();
        }
    }
    #endregion
    #region CharacterSpacing
    private const string m_RichTextRegexPatterns = @"<b>|</b>|<i>|</i>|<size=.*?>|</size>|<Size=.*?>|</Size>|<color=.*?>|</color>|<Color=.*?>|</Color>|<material=.*?>|</material>";
    public override float preferredWidth
    {
        get
        {
            float preferredWidth= cachedTextGenerator.GetPreferredWidth(text, GetGenerationSettings( Vector2.zero));
            List<List<int>> linesVertexStartIndexes = GetLinesVertexStartIndexes();
            int maxLineCount = 0;
            for (int i=0;i<linesVertexStartIndexes.Count;i++)
                maxLineCount = Mathf.Max(maxLineCount, linesVertexStartIndexes[i].Count);
            return preferredWidth + m_CharacterSpacing * (maxLineCount - 1);
        }
    } 

    public int m_CharacterSpacing;
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        if (m_CharacterSpacing == 0)
            return;
        List<UIVertex> vertexes = new List<UIVertex>();
        toFill.GetUIVertexStream(vertexes);

        List<List<int>> linesVertexStartIndexes = GetLinesVertexStartIndexes();
        float alignmentFactor = GetAlignmentFactor();

        for (int i = 0; i < linesVertexStartIndexes.Count; i++)
        {
            float lineOffset = (linesVertexStartIndexes[i].Count - 1) * m_CharacterSpacing * alignmentFactor;
            for (int j = 0; j < linesVertexStartIndexes[i].Count; j++)
            {
                int vertexStartIndex = linesVertexStartIndexes[i][j];
                Vector3 offset = Vector3.right * ((m_CharacterSpacing * j) - lineOffset);
                AddVertexOffset(vertexes, vertexStartIndex + 0, offset);
                AddVertexOffset(vertexes, vertexStartIndex + 1, offset);
                AddVertexOffset(vertexes, vertexStartIndex + 2, offset);
                AddVertexOffset(vertexes, vertexStartIndex + 3, offset);
                AddVertexOffset(vertexes, vertexStartIndex + 4, offset);
                AddVertexOffset(vertexes, vertexStartIndex + 5, offset);
            }
        }

        toFill.Clear();
        toFill.AddUIVertexTriangleStream(vertexes);
    }

    void AddVertexOffset(List<UIVertex> vertexes,int index,Vector3 offset)
    {
        UIVertex vertex = vertexes[index];
        vertex.position += offset;
        vertexes[index] = vertex;
    }

    List<List<int>> GetLinesVertexStartIndexes()
    {
        List<List<int>> linesVertexIndexes = new List<List<int>>();
        IList<UILineInfo> lineInfos = cachedTextGenerator.lines;
        for(int i=0;i<lineInfos.Count;i++)
        {
            List<int> lineVertexStartIndex = new List<int>();
            int lineStart = lineInfos[i].startCharIdx;
            int lineLength = (i < lineInfos.Count - 1) ? lineInfos[i + 1].startCharIdx - lineInfos[i].startCharIdx:text.Length - lineInfos[i].startCharIdx;
            List<int> ignoreIndexes = new List<int>();
            if (supportRichText)
            {
                string line = text.Substring(lineStart, lineLength);
                foreach (Match matchTag in Regex.Matches(line, m_RichTextRegexPatterns))
                {
                    for(int j=0;j<matchTag.Length;j++)
                        ignoreIndexes.Add(matchTag.Index + j);
                }
            }

            for (int j = 0; j < lineLength; j++)
                if (!ignoreIndexes.Contains(j))
                    lineVertexStartIndex.Add((lineStart+j)*6);

            linesVertexIndexes.Add(lineVertexStartIndex);
        }
        return linesVertexIndexes;
    }

    float GetAlignmentFactor()
    {
        switch (alignment)
        {
            default:
                Debug.LogError("Invalid Convertions Here!");
                return 0;
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return 0;
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return .5f;
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return 1f;
        }

    }
    #endregion
}