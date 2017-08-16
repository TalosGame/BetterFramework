using System.Collections;
using UnityEngine;

public class LabelCount : MonoBehaviour 
{
    public float startTime = 0;
    public float stopTime = 0;
    public float speed = 0;
    bool increasing = false;
    public UILabel label;
    float curTIme = 0;

    void OnEnable ()
    {
        increasing = (startTime - stopTime) <= 0;
        curTIme = startTime;
        label.text = startTime.ToString();
        StartCoroutine("CountLabel");
    }

    IEnumerator CountLabel ()
    {
        while (true)
        {
            yield return new WaitForSeconds(speed);
            if (increasing)
            {
                curTIme += speed;
            }
            else
            {
                curTIme -= speed;
            }
            label.text = curTIme.ToString();
            if (curTIme == stopTime)
            {
                yield break;
            }
        }

    }

    void OnDisable ()
    {
        StopCoroutine("CountLabel");
    }
}
