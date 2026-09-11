using UnityEngine;
using System;
using Spine.Unity;
using System.Collections.Generic;

/// <summary>
/// 실제 사용되는 스파인 오브젝트의 FX 클래스..
/// </summary>
[Serializable]
public class SpineFXObj_Ani : SpineFXObjBase
{
    private float                       m_AnimationLength = 0f; 
    
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

        AnimationClip[] aniClips = m_Animator.runtimeAnimatorController.animationClips;

        for(int i=0; i<aniClips.Length; ++i)
        {
            if( aniClips[i].name == m_FXData.FXTypeData )
            {
                m_AnimationLength = aniClips[i].length;
                break;
            }
        }
        
        Init_Depth();

        Init_BoneChase();
        
        m_Transform_FX.gameObject.SetActive(false);
        
        m_IsInit = true;
    }

    private void Update()
    {
        if( false ==  m_IsInit )    return;

        FixedUpdate_AniTime();

        FixedUpdate_BoneChase();
    }

    private void FixedUpdate_AniTime()
    {
        AnimatorStateInfo aniState = m_Animator.GetCurrentAnimatorStateInfo(0);
        
        if (false == aniState.IsName(m_FXData.FXTypeData))
        {
            m_Transform_FX.gameObject.SetActive(false);
            return;
        }
        
        m_CurAniTime = aniState.normalizedTime * m_AnimationLength;

        if(m_CurAniTime > m_AnimationLength )
        {
            m_CurAniTime = m_CurAniTime % m_AnimationLength;
        }

        if( m_FXData.FXNotiTime > m_CurAniTime || m_FXData.FXDeleteTime < m_CurAniTime )   
        {
            m_Transform_FX.gameObject.SetActive(false);
            return;
        }
        
        m_Transform_FX.gameObject.SetActive(true);
    }
}