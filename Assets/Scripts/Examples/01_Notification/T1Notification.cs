using UnityEngine;

public class T1Notification : MonoBehaviour, INotification
{
	private const string NOTIFY_HANDLE_POST_VALUE = "notify_handle_post_value";
	private const string NOTIFY_HANDLE_POST_USERINFO = "notify_handle_post_useinfo";

    private const string NOTIFY_KEY_NAME = "key_name";
    private const string NOTIFY_KEY_SCORE = "key_score";

	void Awake()
	{
		NotificationCenter.Instance.AddObserver(NOTIFY_HANDLE_POST_VALUE, this);
        NotificationCenter.Instance.AddObserver(NOTIFY_HANDLE_POST_USERINFO, this);
	}

    public void NotificationHandler(Notification notify) 
    {
        var evtName = notify.name;
        if (evtName == NOTIFY_HANDLE_POST_VALUE) {
            HandleGetValue(notify);
            return;
        }

        if (evtName == NOTIFY_HANDLE_POST_USERINFO) {
            HandleGetUserInfo(notify);
            return;
        }
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
}
