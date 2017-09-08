//  Author: dyjia
//  Created: 2017-6-20 11:50:12
//  Description: UI旋转固定角度动画
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenAngle : MonoBehaviour {

    public enum Direction
    {
        X,
        Y,
        Z,
    }

    public int preAngle = 0;
    public float preTime = 0;
    public Direction direction;
    Vector3 offset = Vector3.zero;

    void OnEnable ()
    {
        transform.localRotation = Quaternion.identity;
        if (direction == Direction.X)
        {
            offset = new Vector3(preAngle, 0, 0);
        }
        else if (direction == Direction.Y)
        {
            offset = new Vector3(0, preAngle, 0);
        }
        else
        {
            offset = new Vector3(0, 0, preAngle);
        }
        StartCoroutine(ShowEffect());
    }

    IEnumerator ShowEffect ()
    {
        while (this.enabled)
        {
            transform.Rotate(offset);
            yield return new WaitForSeconds(preTime);
        }
    }
}
