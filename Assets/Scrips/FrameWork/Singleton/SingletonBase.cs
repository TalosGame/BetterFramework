using System;

public abstract class SingletonBase<T> : IDisposable where T : class, new()
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null) 
			{
				_instance = new T();
			}

			return _instance;
		}
	}

	protected SingletonBase()
	{
		if (_instance != null) 
		{
			throw new Exception ("This " + typeof(T).ToString() + " Singleton Instance is not null!!!");
			return;
		}

		Init ();
	}

	protected virtual void Init()
	{
		
	}

    public virtual void Dispose()
    {
        
    }
}
