using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelStringEffect : MonoBehaviour 
{
    public List<string> str;
    public float time = 1;
    public float delayTime = 0;
    public bool loop = false;
    int index = 0;
    float preTime = 0;
    public UILabel label;

	// Use this for initialization
    void OnEnable () {
        if (str.Count != 0)
        {
            preTime = time / str.Count;
            if (time != -1)
            {
                StartCoroutine(WriteEffect());
            }
        }
	}
	
    IEnumerator WriteEffect ()
    {
        yield return new WaitForSeconds(delayTime);
        do
        {
            label.text = str[index];
            index ++;
            index = index%str.Count;
            yield return new WaitForSeconds(preTime);
        } while (loop);
    }
}
