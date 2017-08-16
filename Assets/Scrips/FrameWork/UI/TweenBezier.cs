//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Bezier")]
public class TweenBezier : UITweener
{
	public Vector3 from;

    public Vector3 p1;

    public Vector3 p2;

	public Vector3 to;

    public bool showDebugLine = false;


	[HideInInspector]
	public bool worldSpace = false;

	Transform mTrans;
	UIRect mRect;

    private Vector3[] bezier = new Vector3[4];
	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	[System.Obsolete("Use 'value' instead")]
	public Vector3 position { get { return this.value; } set { this.value = value; } }

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public Vector3 value
	{
		get
		{
			return worldSpace ? cachedTransform.position : cachedTransform.localPosition;
		}
		set
		{
			if (mRect == null || !mRect.isAnchored || worldSpace)
			{
				if (worldSpace) cachedTransform.position = value;
				else cachedTransform.localPosition = value;
			}
			else
			{
				value -= cachedTransform.localPosition;
				NGUIMath.MoveRect(mRect, value.x, value.y);
			}
		}
	}

    void Awake() 
    { 
        mRect = GetComponent<UIRect>(); 
        initBezier(); 
    }

	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished) {

        Vector3[] vector3s = PathControlPointGenerator(bezier);
        value = Interp(vector3s, factor);
    }

    void OnDrawGizmos()
    {
        if (!showDebugLine)
        {
            return;
        }

        List<Vector3> nodes = new List<Vector3>();
        nodes.Add(from + transform.position);
        nodes.Add(p1 + transform.position);
        nodes.Add(p2 + transform.position);
        nodes.Add(to + transform.position);
        
        iTween.DrawPath(nodes.ToArray(),Color.blue);
        iTween.DrawLine(nodes.ToArray(), Color.green);
    }


    private static Vector3[] PathControlPointGenerator(Vector3[] path)
    {
        Vector3[] suppliedPath;
        Vector3[] vector3s;

        //create and store path points:
        suppliedPath = path;

        //populate calculate path;
        int offset = 2;
        vector3s = new Vector3[suppliedPath.Length + offset];
        Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);

        //populate start and end control points:
        //vector3s[0] = vector3s[1] - vector3s[2];
        vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
        vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);

        return (vector3s);
    }

    private static Vector3 Interp(Vector3[] pts, float t)
    {
        int numSections = pts.Length - 3;
        int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
        float u = t * (float)numSections - (float)currPt;

        Vector3 a = pts[currPt];
        Vector3 b = pts[currPt + 1];
        Vector3 c = pts[currPt + 2];
        Vector3 d = pts[currPt + 3];

        return .5f * (
            (-a + 3f * b - 3f * c + d) * (u * u * u)
            + (2f * a - 5f * b + 4f * c - d) * (u * u)
            + (-a + c) * u
            + 2f * b
        );
    }

    public void initBezier()
    {
        worldSpace = false;
        bezier[0] = from + transform.localPosition;
        bezier[1] = p1 + transform.localPosition;
        bezier[2] = p2 + transform.localPosition;
        bezier[3] = to + transform.localPosition;

    }

    public void initBezier2()
    {
        worldSpace = false;
        bezier[0] = from;
        bezier[1] = p1;
        bezier[2] = p2;
        bezier[3] = to;

    }
	/// <summary>
	/// Start the tweening operation.
	/// </summary>

    public static TweenBezier Begin(GameObject go, TweenBezier tween, float duration, EventDelegate.Callback callBack, bool isReverse = false)
	{
		TweenBezier comp = UITweener.Begin<TweenBezier>(go, duration);

        if(!isReverse)
        {
            comp.from = tween.from;
            comp.p1 = tween.p1;
            comp.p2 = tween.p2;
            comp.to = tween.to;

        }else
        {
            comp.from = tween.to;
            comp.p1 = tween.p2;
            comp.p2 = tween.p1;
            comp.to = tween.from;
        }

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}

        if (callBack != null)
            comp.AddOnFinished(callBack);

        comp.initBezier2();
		return comp;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue () { from = value; }

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue () { to = value; }

	[ContextMenu("Assume value of 'From'")]
	void SetCurrentValueToStart () { value = from; }

	[ContextMenu("Assume value of 'To'")]
	void SetCurrentValueToEnd () { value = to; }
}
