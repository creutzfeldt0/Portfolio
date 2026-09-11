using UnityEngine;
using System;
using Spine.Unity;
using System.Collections.Generic;

/// <summary>
/// 실제 사용되는 스파인 오브젝트의 FX 클래스..
/// </summary>
[Serializable]
public class SpineFXObj_Const : SpineFXObjBase
{
    /// <summary>
    /// FXData와 SkeletonAnimation, m_FXAnimationName 값 보장..
    /// </summary>
    /// <param name="fxData"></param>
    /// <param name="animation"></param>
    public override void Init( FXData fxData, SkeletonMecanim skeletonMecanim, Animator animator, Transform FXObject )
    {
        if( null == fxData || 
            null == skeletonMecanim ||
            null == animator || 
            null == FXObject ) 
                return;

        m_Transform_FX = FXObject.transform;
        m_FXData = new FXData(fxData);
        m_SkeletonMecanim = skeletonMecanim;
        m_Animator = animator;
        
        Init_Depth();

        Init_BoneChase();

        m_Transform_FX.gameObject.SetActive(false);
        
        m_IsInit = true;
    }

    private void Update()
    {
        if (false == m_IsInit) return;

        m_Transform_FX.gameObject.SetActive(true);

        FixedUpdate_BoneChase();
    }
}