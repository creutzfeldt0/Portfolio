using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
public class FXGameObject : MonoBehaviour
{
#if UNITY_EDITOR
    private Transform                           m_Transform;
    private float                               m_FXTimeRatio = 0f;
    private float                               m_TotalFXNotiTime = 0f;    
    private float                               m_WorldTime;
    private FXData                              m_FXData;
    private SkeletonAnimation                   m_SkeletonAnimation;
    private Transform                           m_SpineBackTrans;
    private Transform                           m_SpineFrontTrans;
    private Transform                           m_SpineTransform;
    private GameObject                          m_FXGameObject;
    private  Transform                          m_FXTransform;
    List<ParticleSystem>                        m_Particles;
    ParticleSystem                              m_MainParticle;
    private float                               m_SimulateTime_Prev = float.MinValue;  
    private float                               m_SimulateTime_Cur = 0f;  
    protected Quaternion                        m_CameraRotation;
    

    private void OnDestroy()
    {
        m_Transform             = null;
        m_SkeletonAnimation     = null;
        m_SpineBackTrans        = null;
        m_SpineFrontTrans       = null;
        m_SpineTransform        = null;

        m_Particles             = null;
        m_MainParticle          = null;
        
        if(null != m_FXGameObject)
        {
            m_FXTransform = null;
            GameObject.DestroyImmediate(m_FXGameObject);
        }

        m_FXTransform = null;
    }

    public void Init( FXData fxData )
    {
        m_FXData = fxData;
        m_Transform = this.transform;

        Scene_SpineTool scene = Manager_SpineFX.Instance.GetScene();
        m_SkeletonAnimation = Manager_SpineFX.Instance.GetSkeletonAnimation();
        m_SpineBackTrans = scene.GetTransform_FXs(true);
        m_SpineFrontTrans = scene.GetTransform_FXs(false);
        m_SpineTransform = m_SkeletonAnimation.transform;

        Instantiate_FXPrefab();

        RefreshFXObject();
    }

    private void Instantiate_FXPrefab()
    {
        if (null != m_Particles) m_Particles.Clear();

        m_FXGameObject = GameObject.Instantiate(m_FXData.FXPrefab, Vector3.zero, Quaternion.identity, m_Transform);
        m_FXGameObject.name = m_FXData.FXPrefab.name;
        NGUITools.SetChildLayer(m_Transform, Manager_SpineFX.Instance.GetSpineLayer());

        DestroyHelperEffect destroyHelperFX;
        if (true == m_FXGameObject.TryGetComponent<DestroyHelperEffect>(out destroyHelperFX))
        {
            destroyHelperFX.OnDisableForEditor();
        }

        DestroyHelperParticle destroyHelperParticle;
        if (true == m_FXGameObject.TryGetComponent<DestroyHelperParticle>(out destroyHelperParticle))
        {
            destroyHelperParticle.OnDisableForEditor();
        }

        ParticleSystem[] particles = m_FXGameObject.transform.GetComponentsInChildren<ParticleSystem>(true);
        m_Particles = particles.ToList();

        if (0 < m_Particles.Count)
            m_MainParticle = m_Particles[0];

        Renderer[] renderers = m_FXGameObject.transform.GetComponentsInChildren<Renderer>(true);
        string sortOrderName = Manager_SpineFX.Instance.GetSpineSortOrderLayerName();
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].sortingLayerName = sortOrderName;
        }

        m_FXTransform = m_FXGameObject.transform;

        m_CameraRotation = Camera.main.transform.rotation;
    }

    private void FixedUpdate()
    {
        if(null != m_FXGameObject)
        {
            RefreshFXTime();
            RefreshBoneTracking();
        }
    }

    public void RefreshFXObject()
    {
        if( true == m_FXData.IsSpineBack )
        {
            m_Transform.parent = m_SpineBackTrans;
        }
        else
        {
            m_Transform.parent = m_SpineFrontTrans;
        }

        m_Transform.localPosition = Vector3.zero;
        m_Transform.localScale = Vector3.one;
        m_Transform.localRotation = Quaternion.identity;

        m_TotalFXNotiTime = m_FXData.FXDeleteTime - m_FXData.FXNotiTime;

        if(0 > m_TotalFXNotiTime)
            m_TotalFXNotiTime = 0f;   

        m_FXTransform.localScale = new Vector3( m_SpineTransform.localScale.x * m_FXData.FXScale.x,
                                                m_SpineTransform.localScale.y * m_FXData.FXScale.y,
                                                m_SpineTransform.localScale.z * m_FXData.FXScale.z );

        RefreshFXTime();
        RefreshBoneTracking();
    }

    private void RefreshFXTime()
    {
        switch ((Manager_SpineFX.FXTYPE) m_FXData.FXType)
        {
            case Manager_SpineFX.FXTYPE.상시FX:
            case Manager_SpineFX.FXTYPE.상시스킨:
                m_FXGameObject.SetActive(true);
                return;
            default:
                break;
        }
        
        if( null == m_SkeletonAnimation || 
            1 > m_SkeletonAnimation.AnimationState.Tracks.Count )    return;

        m_WorldTime = m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime;
        
        float m_MaxTime = m_SkeletonAnimation.AnimationState.Tracks.Items[0].AnimationEnd;
        
        if( m_WorldTime > m_MaxTime )     m_WorldTime = m_WorldTime % m_MaxTime;

        if( null == m_FXData.FXPrefab || null == m_FXGameObject)    return;

        CalculateRatio();

        if (m_FXTimeRatio < 0f)
        {
            m_FXGameObject.SetActive(false);
            return;
        }            

        if( (int)Manager_SpineFX.FXTYPE.안지워짐 != m_FXData.FXType &&  
            m_FXTimeRatio > 1f )
        {
            m_FXGameObject.SetActive(false);
            return;
        }

        m_FXGameObject.SetActive(true);

        m_SimulateTime_Cur = m_TotalFXNotiTime * m_FXTimeRatio;

        if ( m_SimulateTime_Cur == m_SimulateTime_Prev )
        {
        }
        else
        {
            m_Particles[0].Simulate(m_SimulateTime_Cur, true, true);
        }
        
        m_SimulateTime_Prev = m_SimulateTime_Cur;
        
        m_Particles[0].Pause(true);
    }

    private void RefreshBoneTracking()
    {
        if( null == m_FXTransform )   return;

        if( false == m_FXData.IsBoneChase ) 
        {
            m_FXTransform.localPosition = m_FXData.FXPosition;
            return;
        }

        if( null == m_FXData.ChasedBone )    return;

        m_FXTransform.position = m_SpineTransform.position;
        Vector3 bonePos = new Vector3( (m_FXData.ChasedBone.AppliedPose.WorldX * m_SpineTransform.localScale.x), 
                                        (m_FXData.ChasedBone.AppliedPose.WorldY * m_SpineTransform.localScale.y),//0f, 
                                        0f );
        
        bonePos = m_CameraRotation * bonePos;

        m_FXTransform.position = m_FXTransform.position + bonePos;
        
        m_FXTransform.localRotation = Quaternion.Euler( 0f, 
                                                        0f,
                                                        m_FXData.ChasedBone.AppliedPose.WorldRotationX);

        m_FXTransform.localScale = new Vector3( m_FXData.ChasedBone.Pose.WorldScaleX * m_SpineTransform.localScale.x * m_FXData.FXScale.x,
                                                m_FXData.ChasedBone.Pose.WorldScaleY * m_SpineTransform.localScale.y * m_FXData.FXScale.y,
                                                m_SpineTransform.localScale.z);

        m_FXTransform.localPosition += m_FXData.FXPosition; 
    }

    private void CalculateRatio()
    {
        m_FXTimeRatio = (m_WorldTime - m_FXData.FXDeleteTime) / m_TotalFXNotiTime;

        //지우고 난 후..
        if( 0f < m_FXTimeRatio )
        {
            m_FXTimeRatio += 1;
            return;
        }

        m_FXTimeRatio = (m_WorldTime - m_FXData.FXNotiTime) / m_TotalFXNotiTime;
    }

    public void ChangeSpineIsBack()
    {
        ParticleSystem[] particles = m_FXGameObject.transform.GetComponentsInChildren<ParticleSystem>(true);
        m_Particles = particles.ToList();
    }
#endif //UNITY_EDITOR
}

