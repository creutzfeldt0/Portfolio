using System;

public abstract class Singleton<T> : Object where T : class, new()
{
    static protected T  m_instance = default(T);
    static private bool _disposed = true;

    static public T Instance
    {
        get
        {
            if (null == m_instance)
            {
                m_instance = new T();
                return m_instance;
            }

            return m_instance;
        }
    }

    protected Singleton()
    {
        //m_instance = (T)(object)this;
        _disposed = false;
        Awake();
    }

    ~Singleton()
    {
        Dispose(false);
    }

    public void Disinstance()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed == false)
        {
            _disposed = true;

            if (disposing == true)
            {
                OnDestroy();
                m_instance = default(T);
            }
            else
            {
                m_instance = default(T);
            }
        }
    }

    protected abstract void Awake();
    protected abstract void OnDestroy();

    #region StaticClass Ver Singleton..
    /*
    public static T Instance
    {
        get
        {
            return SingletonAllocator.ms_kInstance;
        }
    }

    internal static class SingletonAllocator
    {
        internal static T ms_kInstance = null;

        static SingletonAllocator()
        {
            CreateInstance(typeof(T));
            Initialize(typeof(T));
        }

        public static T CreateInstance(Type type)
        {
            ConstructorInfo ctorNonPublic = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], new ParameterModifier[0]);

            ConstructorInfo[] arCtorPublic = type.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public);

            if (arCtorPublic.Length > 0)
            {
                throw new Exception(
                    type.FullName + " has one more public constructors so the property cannot be enforced.");
            }

            if (null == ctorNonPublic)
            {
                throw new Exception(
                    type.FullName + " doesn't have a private/protected constructor so the property cannnot be enforced.");
            }

            try
            {
                return ms_kInstance = (T)ctorNonPublic.Invoke(new object[0]);
            }
            catch (Exception e)
            {
                throw new Exception(
                    "The singleton couldnt be constructed, check if " + type.FullName + " has a default constructor", e);
            }
        }

        public static bool Initialize(Type type)
        {
            MethodInfo kMethod = type.GetMethod("Initialize");
            if (null != kMethod)
            {
                bool bResult = (bool)kMethod.Invoke(ms_kInstance, null);
                return bResult;
            }

            return true;
        }
    }*/
    #endregion
}
