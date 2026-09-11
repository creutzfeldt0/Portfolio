using UnityEngine;
using Spine.Unity;
using System;
using System.Collections.Generic;

[RequireComponent( typeof(SkeletonMecanim) )]
public class SpineController :  MonoBehaviour
{
    /// <summary>
    /// 메카님 애니의 이벤트 함수를 애니메이션처럼 받아서 특정 모노클래스에 넘겨주기 위해 만듦.. 
    /// </summary>
    [Serializable]
    public struct AniFuncData
    {
        public string                   funcName;
        public float                    funcTime;
        public string                   funcParam_String;
        public int                      funcParam_Int;
        public float                    funcParam_Float;
        public UnityEngine.Object       funcParam_Object;
    }

    protected SkeletonMecanim                       m_SkeletonMecanim;
    protected Animator                              m_Animator;
    protected const string                          m_ParameterName = "state";
    protected bool                                  m_IsParameter = false;
    protected Dictionary<string, int>               m_AnimationIndexs = new Dictionary<string, int>();
    protected MonoBehaviour                         m_EventReceiver;
    protected Dictionary<string, AniFuncData>       m_Functions = new Dictionary<string, AniFuncData>();
    protected Transform                             m_Transform_FXs;
    
    [SerializeField] protected List<SpineFXObjBase>  m_FXs = new List<SpineFXObjBase>();

    protected void OnDestroy()
    {
        m_SkeletonMecanim       = null;
        m_Animator              = null;

        m_AnimationIndexs.Clear();
        m_AnimationIndexs       = null;   

        m_EventReceiver         = null;

        m_Functions.Clear();
        m_Functions             = null;

        m_Transform_FXs         = null;
    }

    public void Awake()
    {
        m_SkeletonMecanim = gameObject.GetComponent<SkeletonMecanim>();
        m_Animator = gameObject.GetComponent<Animator>();
        m_IsParameter = false;
        m_Transform_FXs = transform.Find( "FXs" );
        
        if( null == GetAnimator() ) return;

        AnimatorControllerParameter param;

        for(int i = 0; i < GetAnimator().parameterCount; i++)
        {
            param = GetAnimator().GetParameter(i);

            if(param.name == m_ParameterName)
            {
                m_IsParameter = true;
                break;
            }
        }
        
        //메카님에 등록된 State들에 들어간 애니메이션 클립들, state들 이므로 클립들간 중복 가능하다..
        AnimationClip[] clips = GetAnimator().runtimeAnimatorController.animationClips;
        //스파인에 저장된 애니메이션들, 중복 불가능하다. AniType과 같아야함..
        Spine.Animation[] aniDatas = m_SkeletonMecanim.SkeletonDataAsset.GetSkeletonData(false).Animations.Items;
        
        AniFuncData aniFuncData;
        AnimationEvent animEvent;
        List<AnimationEvent> animEvents = new List<AnimationEvent>();
        
        m_Functions.Clear();
        m_AnimationIndexs.Clear();

        //일단 애니 순서에 맞춰서 인덱스 순서 정해둠..
        for (int i = 0; i < aniDatas.Length; i++)
        {
            m_AnimationIndexs.Add(aniDatas[i].Name, i);
        }
        
        //State에 들어간 설정에 맞춰서 데이터 입력..
        for (int i = 0; i < clips.Length; i++)
        {
            animEvents.Clear();

            for(int j = 0; j < clips[i].events.Length; j++)
            {
                if( "HandleEvent" == clips[i].events[j].functionName )
                {
                    animEvent = clips[i].events[j];
                    
                    aniFuncData = JsonUtility.FromJson<AniFuncData>( animEvent.stringParameter );
                    //safeCode..
                    if (0f == aniFuncData.funcTime)
                    {
                        aniFuncData.funcTime = clips[i].events[j].time;
                        animEvent.stringParameter = JsonUtility.ToJson( aniFuncData );
                    }
                }
                else
                {
                    aniFuncData = new AniFuncData
                    {   
                        funcName = new string( clips[i].events[j].functionName ),
                        funcTime = clips[i].events[j].time,
                        funcParam_String = new string( clips[i].events[j].stringParameter ),
                        funcParam_Int = clips[i].events[j].intParameter,
                        funcParam_Float = clips[i].events[j].floatParameter,
                        funcParam_Object = null,
                    };

                    if (true == m_Functions.ContainsKey(aniFuncData.funcName))
                    {
                        m_Functions[aniFuncData.funcName] = aniFuncData;
                    }
                    else
                    {
                        m_Functions.Add( aniFuncData.funcName, aniFuncData);
                    }

                    if(null != clips[i].events[j].objectReferenceParameter)
                    {
                        aniFuncData.funcParam_Object = ObjectCopier.Clone<UnityEngine.Object>( clips[i].events[j].objectReferenceParameter);
                    }

                    animEvent = new AnimationEvent();
                    animEvent.functionName = "HandleEvent";
                    animEvent.time = aniFuncData.funcTime;
                    animEvent.stringParameter = JsonUtility.ToJson( aniFuncData );
                }
                
                if( true == m_Functions.ContainsKey(aniFuncData.funcName) )
                {
                    m_Functions[aniFuncData.funcName] = aniFuncData;
                }
                else
                {
                    m_Functions.Add( aniFuncData.funcName, aniFuncData);
                }

                animEvents.Add(animEvent);
            }

            clips[i].events = animEvents.ToArray();
        }
    }

    public void InitData( MonoBehaviour eventReceiver )
    {
        m_EventReceiver = eventReceiver;

        for (int i = 0; i < m_FXs.Count; ++i)
        {
            if (null == m_FXs[i]) continue;
            if (0 == m_FXs[i].transform.childCount) continue;

            m_FXs[i].Init(m_FXs[i].GetFXData(),
                           m_SkeletonMecanim,
                           m_Animator,
                           m_FXs[i].transform.GetChild(0));
        }
    }

    public Animator GetAnimator()
    {
        return m_Animator;
    }

    public void SetFXParents(bool isActive)
    {
        if (null != m_Transform_FXs)
        {
            m_Transform_FXs.gameObject.SetActive(isActive);
        }
    }

    public List<SpineFXObjBase> GetFXs()
    {
        return m_FXs;
    }

    public float GetCurAniLength()
    {
        return GetAnimator().GetCurrentAnimatorStateInfo(0).length;
    }

    /// <summary>
    /// 현재 애니메이션 진행도 0~1 ..
    /// Mix 되는 애니메이션이면 Mix되는 값도 생각해야된다..
    /// </summary>
    /// <returns></returns>
    public float GetCurAniProgress()
    {
        AnimatorStateInfo info = GetAnimator().GetCurrentAnimatorStateInfo(0);
        
        return info.normalizedTime;
    }

    public bool IsCurAniEnd()
    {
        return 1f <= GetCurAniProgress();
    }

    public bool HasAni(string inAnimName)
    {
        if (null == m_AnimationIndexs) return false;

        foreach (string k in m_AnimationIndexs.Keys)
        {
            if (k.ToLower() == inAnimName.ToLower())
            {
                return true;
            }
        }

        return false;
    }
    public bool IsCurAni( string aniName )
    {
        if (GetAnimator() == null)
            return false;

        return GetAnimator().GetCurrentAnimatorStateInfo(0).IsName(aniName);
    }

    public void PlayAnimation( string aniName, float normalizedTime = 0f )
    {
        // 예약되어있는곳에 가지않게 초기화.
        GetAnimator().SetInteger( m_ParameterName, -1 );

        if (string.IsNullOrEmpty(aniName) == false)
        {
            GetAnimator().StopPlayback();
            GetAnimator().Play(aniName, 0, normalizedTime);
            GetAnimator().Update(normalizedTime);
        }
    }

    public void SetAniState( string aniName )
    {
        if( false == m_IsParameter )  return;
        if( null == m_AnimationIndexs )  return;

        int index;
        if( m_AnimationIndexs.TryGetValue(aniName, out index) )
        {
            GetAnimator().SetInteger( m_ParameterName, index );
        }
        else
        {
            GetAnimator().SetInteger( m_ParameterName, -1 );
        }
    }

    public void PauseAnimation(bool isPause = true)
    {
        if (isPause)
        {
            GetAnimator().StopPlayback();
        }
        else
        {
            GetAnimator().StartPlayback();
            GetAnimator().speed = 1f;
        }
    }

    public void ResetAniParameter()
    {
        if( false == m_IsParameter )  return;
        GetAnimator().SetInteger( m_ParameterName, -1 );
    }

    /// <summary>
    /// SpineMecanim도 SpineAnimation 처럼 함수 모아서 받을 수 있도록 바꿔봄.
    /// </summary>
    /// <param name="aniFuncData"></param>
    public void HandleEvent( string aniFuncData )
    {
        if(null == m_EventReceiver) return;

        AniFuncData funcData = JsonUtility.FromJson<AniFuncData>(aniFuncData);

        if( true == string.IsNullOrEmpty(funcData.funcName) ) return;

        m_EventReceiver.SendMessage( funcData.funcName, funcData, SendMessageOptions.DontRequireReceiver );
    }

    public float GetFunctionCallTime( string funcName )
    {
        AniFuncData aniFuncData;
        
        if (false == m_Functions.TryGetValue(funcName, out aniFuncData)) return -1f;

        return aniFuncData.funcTime;
    }
}