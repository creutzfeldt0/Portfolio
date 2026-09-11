using UnityEngine;
using System;
using Spine.Unity;
using System.Collections.Generic;

/// <summary>
/// 실제 사용되는 스파인 오브젝트의 FX 클래스..
/// </summary>
[Serializable]
public abstract class SpineFXObjBase : MonoBehaviour
{
    protected Transform                     m_Transform_FX;
    [SerializeField] protected FXData       m_FXData;
    protected Animator                      m_Animator;
    protected SkeletonMecanim               m_SkeletonMecanim;
    protected float                         m_CurAniTime = 0f;
    protected Transform                     m_Transform_Spine;
    protected Quaternion                    m_CameraRotation;
    protected ParticleSystemRenderer[]      m_ParticleRenderers;
        
    protected bool                          m_IsInit = false;

    protected void OnDestroy()
    {       
        m_Transform_FX                  = null;
        m_FXData                        = null;
        m_Animator                      = null;
        m_SkeletonMecanim               = null;
        m_Transform_Spine               = null;
        m_ParticleRenderers             = null;        
    }

    protected void Awake()
    {
        m_Transform_FX = transform.GetChild(0);

        if (null == m_Transform_FX) return;
        
        m_Transform_FX.gameObject.SetActive(false);
    }

    /// <summary>
    /// FXData와 SkeletonAnimation, m_FXAnimationName 값 보장..
    /// </summary>
    /// <param name="fxData"></param>
    /// <param name="animation"></param>
    public abstract void Init(FXData fxData, SkeletonMecanim skeletonMecanim, Animator animator, Transform FXObject);
    //protected abstract void FixedUpdate();

    protected void Init_Depth()
    {
        m_ParticleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
        Renderer spineRenderer = m_SkeletonMecanim.GetComponent<Renderer>();
        m_Transform_Spine = m_SkeletonMecanim.transform;
        
        int maxDepth = 0;

        if (m_ParticleRenderers != null)
        {
            for (int i = 0; i < m_ParticleRenderers.Length; ++i)
            {
                m_ParticleRenderers[i].sortingLayerID = spineRenderer.sortingLayerID;

                if (m_ParticleRenderers[i].sortingOrder > maxDepth)
                {
                    maxDepth = m_ParticleRenderers[i].sortingOrder;
                }
            }
        }

        int fxDepth;;

        if( true == m_FXData.IsSpineBack )
        {
            fxDepth = - (spineRenderer.sortingOrder - 1 + maxDepth);
            
            for (int i=0; i< m_ParticleRenderers.Length; ++i )
            {
                m_ParticleRenderers[i].sortingOrder = fxDepth + m_ParticleRenderers[i].sortingOrder;
            }
        }
        else
        {
            fxDepth = spineRenderer.sortingOrder + 1;
            
            for (int i=0; i< m_ParticleRenderers.Length; ++i )
            {
                m_ParticleRenderers[i].sortingOrder = fxDepth + m_ParticleRenderers[i].sortingOrder;
            }
        }
    }

    protected void Init_BoneChase()
    {
        if( true == m_FXData.IsBoneChase )
        {
            m_Transform_FX.localScale = m_FXData.FXScale;

            if (m_FXData.SelectedBoneIndex < m_SkeletonMecanim.skeleton.Bones.Items.Length)
            {
                m_FXData.ChasedBone = m_SkeletonMecanim.skeleton.Bones.Items[ m_FXData.SelectedBoneIndex ];
            }
        }
        else
        {
            m_Transform_FX.localPosition = m_FXData.FXPosition;
            m_Transform_FX.localScale = m_FXData.FXScale;
        }

        m_CameraRotation = Camera.main.transform.rotation;
    }
    
    protected void FixedUpdate_BoneChase()
    {
        if (false == m_Transform_FX.gameObject.activeInHierarchy) return;
        
        if (false == m_FXData.IsBoneChase || null == m_FXData.ChasedBone)
        {
            return;
        }

        //Vector3 yVector = Vector3.up * ( m_Transform_Spine.position.y + (m_FXData.ChasedBone.WorldY * m_Transform_Spine.localScale.y) );
        //yVector = m_CameraRotation * yVector;
        
        m_Transform_FX.position = m_Transform_Spine.position;
        Vector3 bonePos = new Vector3( (m_FXData.ChasedBone.AppliedPose.WorldX * m_Transform_Spine.localScale.x), 
                                        (m_FXData.ChasedBone.AppliedPose.WorldY * m_Transform_Spine.localScale.y),//0f, 
                                        0f );

        bonePos = m_CameraRotation * bonePos;
        m_Transform_FX.position = m_Transform_FX.position + bonePos;

        
        //Scale값이 - 가 되면 고려하여 로직 변경해야한다. 일단은 없으니 간단하게..
        m_Transform_FX.localRotation = Quaternion.Euler( 0f, 
                                                         0f,
                                                         m_FXData.ChasedBone.AppliedPose.WorldRotationX);

        m_Transform_FX.localScale = new Vector3( m_FXData.ChasedBone.Pose.ScaleX * m_FXData.FXScale.x,
                                                    m_FXData.ChasedBone.Pose.ScaleY * m_FXData.FXScale.y,
                                                      m_FXData.FXScale.z );

        m_Transform_FX.localPosition += m_FXData.FXPosition; 
    }

    public FXData GetFXData()
    {
        return m_FXData;
    }

    public string GetFXTypeData()
    {
        return m_FXData.FXTypeData;
    }

    public ParticleSystemRenderer[] GetParticleRenderers()
    {
        return m_ParticleRenderers;
    }
}