using UnityEngine;

public class SingletonDontDestroyMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }

    protected virtual void Awake()
    {
        InitSingleTon();
    }

    private void InitSingleTon()
    {
        if (instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            DestroyImmediate(this.gameObject);
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
