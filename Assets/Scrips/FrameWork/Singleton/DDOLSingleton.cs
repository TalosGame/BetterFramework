using UnityEngine;

public abstract class DDOLSingleton<T> : MonoBehaviour where T : DDOLSingleton<T>
{
	private static T _instance;

    public static T Instance
	{
		get 
		{
			if (_instance == null)
			{
                _instance = GameObject.FindObjectOfType<T>();

                if(_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();

                    go.AddToDDOLRoot();
                }

                _instance.Init();
			}
			return _instance;
		}
	}

    protected virtual void Init(){}

	void OnDestroy()
	{
		_instance = null;
		Destroy();
        Debug.Log(this.gameObject.name + " OnDestroy!");
	}

	protected virtual void Destroy() { }
}

