using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    public static T instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            instance = _instance;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
