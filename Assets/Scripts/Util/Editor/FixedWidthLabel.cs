// /****************************************************************
// *   FixedWidthLabel.cs
// *   作者：Miller 
// *   创建时间：2017/4/17 21:20
// *   
// *	QQ: 408310416 
// *	Email: wangquan84@126.com
// *
// *   描述说明：
// *		
// *   修改历史：
// *
// *
// *****************************************************************/
using System;
using UnityEngine;
using UnityEditor;

//FixedWidthLabel class. Extends IDisposable, so that it can be used with the "using" keyword.
public class FixedWidthLabel : IDisposable
{
	private readonly ZeroIndent indentReset; //helper class to reset and restore indentation

	public FixedWidthLabel(GUIContent label, float offsetW = 0)
	{
		// state changes are applied here.
		EditorGUILayout.BeginHorizontal();// create a new horizontal group

		EditorGUILayout.LabelField (label,
			GUILayout.Width (GUI.skin.label.CalcSize (label).x + // actual label width
				9 * EditorGUI.indentLevel + offsetW));//indentation from the left side. It's 9 pixels per indent level

		indentReset = new ZeroIndent();//helper class to have no indentation after the label
	}

	public FixedWidthLabel(string label, float offsetW = 0)
		: this(new GUIContent(label), offsetW)//alternative constructor, if we don't want to deal with GUIContents
	{
	}

	public void Dispose() //restore GUI state
	{
		//Debug.Log("Dispose!!!!!!!!!!");
		indentReset.Dispose();//restore indentation
		EditorGUILayout.EndHorizontal();//finish horizontal group
	}
}

class ZeroIndent : IDisposable //helper class to clear indentation
{
	private readonly int originalIndent;//the original indentation value before we change the GUI state
	public ZeroIndent()
	{
		originalIndent = EditorGUI.indentLevel;//save original indentation
		EditorGUI.indentLevel = 0;//clear indentation
	}

	public void Dispose()
	{
		EditorGUI.indentLevel = originalIndent;//restore original indentation
	}
}

