using System;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static protected T  m_instance = default(T);

    static public T Instance
    {
        get
        {
            if (null == m_instance)
            {
                System.Type type = typeof(T);

                GameObject obj = GameObject.Find( type.Name );

                if(null == obj)
                {
                    obj = new GameObject(type.Name);
                    m_instance = obj.AddComponent<T>();
                    obj.AddComponent<DontDestroy>();
                }
                else
                {
                    m_instance = obj.GetComponent<T>();
                }
            }

            return m_instance;
        }
    }

    public void Disinstance()
    {
        DestroyImmediate( m_instance.gameObject, true );
        m_instance = default(T);
        GC.SuppressFinalize(this);
    }
}
