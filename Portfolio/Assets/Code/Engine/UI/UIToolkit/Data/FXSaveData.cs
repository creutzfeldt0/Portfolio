using UnityEngine;
using Spine.Unity;
using System;
using System.Collections.Generic;

[Serializable]
public class FXSaveData : ScriptableObject
{
#if UNITY_EDITOR
    public byte                                     m_SceneType;
    public SkeletonDataAsset                        m_SkeletonDataAsset;
    public float                                    m_SpineScale;
    public string                                   m_FXDatas_All;
    public List<GameObject>                         m_FXObjects = new List<GameObject>();
#endif // UNITY_EDITOR
}