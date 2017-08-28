using UnityEngine;
using System.Collections.Generic;

public class NotificationTest : MonoBehaviour
{
	private const string NOTIFY_HANDLE_POST_VALUE = "notify_handle_post_value";
	private const string NOTIFY_HANDLE_POST_USERINFO = "notify_handle_post_useinfo";

    private const string NOTIFY_KEY_NAME = "key_name";
    private const string NOTIFY_KEY_SCORE = "key_score";

	void Awake()
	{
		NotificationCenter.Instance.AddObserver(this, NOTIFY_HANDLE_POST_VALUE, HandleGetValue);
        NotificationCenter.Instance.AddObserver(this, NOTIFY_HANDLE_POST_USERINFO, HandleGetUserInfo);
	}

	private void HandleGetValue(Notification notify)
	{
		int val = (int)notify.Object;
        Debug.Log("GameObject name:" + gameObject.name + " Get value:" + val);
	}

	private void HandleGetUserInfo(Notification notify)
	{
        string name = (string)notify[NOTIFY_KEY_NAME];
        int score = (int)notify[NOTIFY_KEY_SCORE];

        Debug.Log("GameObject name:" + gameObject.name + " Get user name:" + name + " score:" + score);
	}

	void Update()
	{
		if (Input.GetKeyUp(KeyCode.A))
		{
			NotificationCenter.Instance.PostNotification(NOTIFY_HANDLE_POST_VALUE, 100);
		}

        if (Input.GetKeyUp(KeyCode.S))
        {
            Dictionary<string, object> userInfo = new Dictionary<string, object>();
            userInfo.Add(NOTIFY_KEY_NAME, "miller");
            userInfo.Add(NOTIFY_KEY_SCORE, 100);
            NotificationCenter.Instance.PostNotification(NOTIFY_HANDLE_POST_USERINFO, null, userInfo);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            NotificationCenter.Instance.RemoveObserver(this);
        }

        if (Input.GetKeyUp(KeyCode.F))
		{
            NotificationCenter.Instance.RemoveObserver(this, NOTIFY_HANDLE_POST_VALUE);
		}
	}
}
