using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene_Base : MonoBehaviour
{
    protected List<Queue<Popup_Base>>               m_OpenPopupList = new List<Queue<Popup_Base>>();
    protected Dictionary<System.Type , Popup_Base>  m_Popups = new Dictionary<System.Type, Popup_Base>();

    protected Transform                             m_Content;
    protected Transform                             m_Transform;

    public virtual void Destroy()
    {
        //DestroyStub();
        //DestroyPlugin();

        //Util.UtilFunc.DestroyDictionary<System.Type, Popup_Base>(ref m_Popups);

        for (int j = 0; j < m_OpenPopupList.Count; ++j)
        {
            m_OpenPopupList[j].Clear();
            m_OpenPopupList[j] = null;
        }
        m_OpenPopupList.Clear();
        m_OpenPopupList = null;

        m_Content = null;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Popup_Base curPopup = PeekPopup();

            if (null != curPopup)
            {
                curPopup.OnAndroidCancel();
            }
            else
            {
                OnQuitBtn();
            }
        }
    }

    /// <summary>
    /// 에셋번들 로딩하고 바로 불리는 함수.. 유저에게 Scene이 노출된 상태은 아니다..
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator ReserveLoadScene()
    {
        m_Transform = this.transform;

        m_Content = m_Transform.Find("Content");
        m_Content.gameObject.SetActive(false);

        Popup_Base[] popups = m_Transform.GetComponentsInChildren<Popup_Base>(true);

        for(int i=0; i< popups.Length; ++i)
        {
            m_Popups.Add( popups[i].GetType(), popups[i]);
        }

        /*
        포폴을 위해 SceneMgr 제거 조치..
        Popup_Base popup_Common = SceneMgr.Instance.GetPopupCommon();

        if (null != popup_Common)
        {
            m_Popups.Add(popup_Common.GetType(), popup_Common);
        }
        */
        yield return CoAwake();
    }

    /// <summary>
    /// 무조건 ReserveLoadScene 이후에 들어온다..
    /// 유저에게 Scene이 노출..
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator MoveScene()
    {
        m_Content.gameObject.SetActive(true);

        yield return true;

        yield return CoStart();
    }

    private void OnApplicationFocus(bool focus)
    {
        if(true == focus)
        {
            Screen.fullScreen = true;
        }
    }

    protected abstract IEnumerator CoAwake();
    protected abstract IEnumerator CoStart();
    public abstract void RefreshLabel();

    protected virtual void OnQuitBtn()
    {
    }

    public void Call_Quit()
    {
        Application.Quit();
    }

    public T GetPopup<T>() where T : Popup_Base
    {
        Popup_Base popupBase;
        System.Type type = typeof(T);

        if ( m_Popups.TryGetValue(type, out popupBase) )
        {
            return (T)popupBase;
        }
        Debug.LogWarning("[" + gameObject.name + "] " + type.Name + " null");
        return null;
    }

    public bool IsPopupOpen<T>() where T : Popup_Base
    {
        Popup_Base popupBase = GetPopup<T>();

        if( null != popupBase )
        {
            return popupBase.IsPopupOpen();
        }

        return false;
    }

    public void Regist<T>( object data, bool registOnly = false) where T : Popup_Base
    {
        Popup_Base popup = GetPopup<T>();

        if (null == popup) return;

        /*
        포폴을 위해 SceneMgr 제거 조치..

        Scene_Base sceneBase = SceneMgr.Instance.GetCurScene();
        Popup_Base prevPopup = sceneBase.PeekPopup();
        sceneBase.AddPopupList(popup);
        */
        Popup_Base prevPopup = null;

        popup.Initialize(data);

        //상황에 따른 대기/열기...
        if (registOnly)
        {
            //0) 쌓아둔다. PopupShow를 받은 후 연다...
        }
        else if ( true == popup.IsSequencePopup() &&
                  null != prevPopup &&
                  true == prevPopup.IsSequencePopup() )
        {
            //1) 시퀀스 팝업은 앞에 팝업도 시퀀스면 오픈 안하고 대기한다...
        }
        else
        {
            popup.OnPopupOpen();
        }
    }

    public void AddPopupList(Popup_Base popup)
    {
        Queue<Popup_Base> queue;

        if (true == popup.IsSequencePopup() && 0 < m_OpenPopupList.Count)
        {
            int index = m_OpenPopupList.Count - 1;
            queue = m_OpenPopupList[index];

            index = queue.Count - 1;
            Popup_Base lastPopup = queue.Peek();

            if (true == lastPopup.IsSequencePopup())
            {
                queue.Enqueue(popup);
            }
            else
            {
                queue = new Queue<Popup_Base>();
                queue.Enqueue(popup);
                m_OpenPopupList.Add(queue);
            }
        }
        else
        {
            queue = new Queue<Popup_Base>();
            queue.Enqueue(popup);
            m_OpenPopupList.Add(queue);
        }
    }

    public Popup_Base PopPopup()
    {
        if (0 == m_OpenPopupList.Count) return null;

        int index = m_OpenPopupList.Count - 1;
        Queue<Popup_Base> queue = m_OpenPopupList[index];

        Popup_Base value = queue.Dequeue();

        if (0 == queue.Count)
        {
            queue = null;
            m_OpenPopupList.RemoveAt(index);
        }

        return value;
    }

    public Popup_Base PeekPopup()
    {
        if (0 == m_OpenPopupList.Count) return null;

        int index = m_OpenPopupList.Count - 1;
        Queue<Popup_Base> queue = m_OpenPopupList[index];

        return queue.Peek();
    }

    public int GetPopupDepth()
    {
        return m_OpenPopupList.Count;
    }

    public void NextSequencePopup()
    {
        Popup_Base lastPopup = PopPopup();

        if (0 == m_OpenPopupList.Count ||
            false == lastPopup.IsSequencePopup()) return;

        Popup_Base nextPopup = PeekPopup();

        if (false == nextPopup.IsSequencePopup()) return;

        nextPopup.OnPopupOpen();
    }

    public void PlayPopup() //그간 모은 것을 보여주자...
    {
        Popup_Base lastPopup = PopPopup();
        if (null != lastPopup)
        {
            lastPopup.OnPopupOpen();
        }
    }
}
