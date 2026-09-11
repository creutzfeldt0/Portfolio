using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderBase : MonoBehaviour
{
    [SerializeField] protected BUNDLE_TYPE m_Type = BUNDLE_TYPE.ASSET;
    [SerializeField] protected string m_ResourcePath = string.Empty;

#if UNITY_EDITOR
    public virtual void RefreshResource() { }
    public virtual void SaveResourcePath() { }
    public virtual void DeleteResource() { }
#endif
}