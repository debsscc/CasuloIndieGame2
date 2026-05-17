//----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debora Carvalho
// DESCRIÇÃO: Componente genérico Singleton.
// Ele garante que apenas uma instância do tipo T exista na cena.
// ----------------------------------------------------------------

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
