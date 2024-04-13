using System;
using UnityEditor;
using UnityEngine;

public class FixedWidthLabel : IDisposable
{
    private readonly ZeroIndent indentReset;

    public FixedWidthLabel(GUIContent label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label,
            GUILayout.Width(GUI.skin.label.CalcSize(label).x +
                9 * EditorGUI.indentLevel));

        indentReset = new ZeroIndent();
    }

    public FixedWidthLabel(string label)
        : this(new GUIContent(label))
    {
    }

    public void Dispose()
    {
        indentReset.Dispose();
        EditorGUILayout.EndHorizontal();
    }
}

class ZeroIndent : IDisposable
{
    private readonly int originalIndent;
    public ZeroIndent()
    {
        originalIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
    }

    public void Dispose()
    {
        EditorGUI.indentLevel = originalIndent;
    }
}