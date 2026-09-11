using Method;
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyHelperParticle : MonoBehaviour {

    private FunctionVoid    m_callbakEnd = null;

#if UNITY_EDITOR
    private bool            m_destroyPass = false;
#endif //UNITY_EDITOR

    IEnumerator Start()
    {
        while (true)
        {
            yield return YieldCache.WaitForSeconds(0.1f);

            if (!GetComponent<ParticleSystem>().IsAlive(true))
            {
                Destroy(gameObject);
                break;
            }
        }

        yield return null;
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if(true == m_destroyPass)    return;
#endif //UNITY_EDITOR

        m_callbakEnd?.Invoke();

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    public void OnDisableForEditor()
    {
        m_destroyPass = true;
    }
#endif //UNITY_EDITOR

    public void SetCallbackEnd(FunctionVoid _callbakEnd)
    {
        m_callbakEnd = _callbakEnd;
    }
}
