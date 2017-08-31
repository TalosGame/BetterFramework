using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected Transform cacheTransform;

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
			{
                _instance = GameObject.FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }

                _instance.Init();
            }

            return _instance;
        }
    }

    protected virtual void Init()
    {
        cacheTransform = this.transform;
    }

    protected void OnApplicationQuit()
    {
        if(_instance == null)
        {
            return;
        }

        GameObject.Destroy(_instance.gameObject);
        _instance = null;
    }
}

