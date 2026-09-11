using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Popup_Base : MonoBehaviour
{
    [SerializeField] private bool   m_IsSequence = false;
    protected Transform             m_Transform;
    protected Animation             m_Animation;
    protected GameObject            m_Base;

    protected virtual void OnDestroy()
    {
        m_Transform         = null;
        m_Animation         = null;
        m_Base              = null;
    }

    protected void Awake()
    {
        m_Transform = this.transform;
        m_Animation = m_Transform.GetComponent<Animation>();

        Transform tmpTrans = m_Transform.Find("Base");

        if (null == tmpTrans) return;

        m_Base = tmpTrans.gameObject;
    }

    public bool IsSequencePopup()
    {
        return m_IsSequence;
    }

    public bool IsPopupOpen()
    {
        return m_Base.activeSelf;
    }

    public virtual void OnAndroidCancel()
    {
    }

    public void OnPopupOpen()
    {
        Open();
    }

    public void OnPopupForceClose()
    {
        m_Base.SetActive(false);

        //SceneMgr.Instance.GetCurScene().NextSequencePopup();
    }

    public abstract void Initialize(object data);
    protected abstract void Open();
    protected abstract void Close();
    public abstract void RefreshLabel();
}
