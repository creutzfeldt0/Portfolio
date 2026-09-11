using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
using System.Collections;
#endif  // UNITY_EDITOR

/// <summary>
/// UI툴킷에서 쓰는 팝업을 정리하기 위해 만든 부모클래스..
/// 팝업닫을 때 버튼은 ClosePopup과 연결 하도록 짬..
/// </summary>
public abstract class UTPopup_Base : MonoBehaviour
{
#if UNITY_EDITOR
    protected VisualElement                     m_uiPopupVE;
    private Coroutine                           m_Coroutine;
    private bool                                m_isCreate = false;
    private bool                                m_isOpen = false;

    protected virtual void OnDestroy() 
    {
        m_uiPopupVE               = null;
        m_Coroutine               = null;
    }
    
    public void InitVisualElement( VisualElement uiPopupVE )
    {
        m_uiPopupVE = uiPopupVE; 
        if( null == m_uiPopupVE )return; 
        m_uiPopupVE.style.display = DisplayStyle.None; 
    }

    //Awake Implementation
    protected abstract void Initialize( object baseData );
    protected abstract void CreateElement();

    //Start Implementation
    protected virtual IEnumerator OpenAnimation() 
    {
        m_uiPopupVE.style.display = DisplayStyle.Flex; 
        m_Coroutine = null; 
        m_isOpen = true;
        yield break; 
    }

    //Open EnterCode
    public void OpenPopup( object baseData )
    {
        Initialize(baseData);
        
        if ( false == m_isCreate )
        {
            CreateElement();
            m_isCreate = true;
        }

        if(null != m_Coroutine) StopCoroutine( m_Coroutine );

        m_Coroutine = StartCoroutine( OpenAnimation() );
    }

    //UI Value Refresh, Language Change
    public abstract void RefreshUI();

    //Close Implementation
    protected virtual IEnumerator CloseAnimation() 
    {  
        m_uiPopupVE.style.display = DisplayStyle.None; 
        m_Coroutine = null; 
        m_isOpen = false;
        yield break; 
    }

    //Close EnterCode
    public void ClosePopup()
    {
        if(null != m_Coroutine) StopCoroutine( m_Coroutine );

        m_Coroutine = StartCoroutine( CloseAnimation() );
    }

    public bool IsOpen()
    {   
        return m_isOpen;
    }
#endif  // UNITY_EDITOR
}
