using UnityEngine;
using System;
using Spine.Unity;
using System.Collections.Generic;

/// <summary>
/// 실제 사용되는 스파인 오브젝트의 FX 클래스..
/// </summary>
[Serializable]
public class SpineFXObj_Ani_UnDel : SpineFXObjBase
{
    private float                       m_AnimationLength = 0f; 
    private float                       m_NotiTime = -1f; 
    private float                       m_EndTime = -1f;

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
        m_NotiTime = -1f;
        m_EndTime = -1f;
        m_IsInit = true;
    }

    private void Update()
    {
        if( false ==  m_IsInit )    return;

        FixedUpdate_Ani_UnDel();
        FixedUpdate_BoneChase();
    }
    
    private void FixedUpdate_Ani_UnDel()
    {
        AnimatorStateInfo aniState = m_Animator.GetCurrentAnimatorStateInfo(0);
        
        if (true == aniState.IsName(m_FXData.FXTypeData))
        {
            m_CurAniTime = aniState.normalizedTime * m_AnimationLength;

            if(m_CurAniTime > m_AnimationLength )
            {
                m_CurAniTime = m_CurAniTime % m_AnimationLength;
            }

            if (m_NotiTime == -1)
            {
                if( m_FXData.FXNotiTime > m_CurAniTime )   
                {
                    m_Transform_FX.gameObject.SetActive(false);
                    m_NotiTime = -1f;
                    m_EndTime = -1f;
                }
                else
                {
                    //애니가 달라져도..
                    //NotiTime으로 부터  m_FXData.FXDeleteTime 만큼 FX 지속시킴..
                    m_Transform_FX.gameObject.SetActive(true);
                    m_NotiTime = Time.realtimeSinceStartup - (m_CurAniTime - m_FXData.FXNotiTime);
                    m_EndTime = m_NotiTime + m_FXData.FXDeleteTime;
                }
            }
            else // [Feldt] 툴에선 애니가 loop 될 때 지워지지 않게 다시 시작하지만 실제FX는 시간지나면 지워져야하므로..
            {
                if (Time.realtimeSinceStartup > m_EndTime)
                {
                    m_Transform_FX.gameObject.SetActive(false);
                    m_NotiTime = -1f;
                    m_EndTime = -1f;
                }
            }
        }
        else
        {
            if (-1f == m_EndTime)
            {
                m_Transform_FX.gameObject.SetActive(false);
                m_NotiTime = -1f;
            }
            else
            {
                if ( Time.realtimeSinceStartup < m_EndTime )
                {
                    m_Transform_FX.gameObject.SetActive(true);
                }
                else
                {
                    m_Transform_FX.gameObject.SetActive(false);
                    m_NotiTime = -1f;
                    m_EndTime = -1f;
                }
            }
        }
    }
}