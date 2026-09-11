
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
	
/// <summary>
/// 일반적인 팝업 윈도우 개선버전
/// 유니티 기본 팝업은 화면밖으로 넘어가게 되면 선택이 매우 불편해진다. 그것을 보안하기 위한 클래스이다.
/// 
/// GUILayout.Label(m_rgStrEffectKind[idx], GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
/// string Name = "";
/// if (m_rgEffectID[idx, m_nRenderSkillActionIdx] != -1)
///     Name = m_EffectNames[m_rgEffectID[idx, m_nRenderSkillActionIdx]];
/// if (GUILayout.Button(Name))
/// {
///     EditorApplication.ExecuteMenuItem("Tool/Popup/GUIPopupWindow");
///     if (GUIPopupWindow.s_EW != null)
///     {
///         Vector2 pos = Event.current.mousePosition;
///         pos.x += this.position.x;
///         pos.y += this.position.y;
/// 
///         int curIdx = idx;
///         int acIdx = m_nRenderSkillActionIdx;
///         GUIPopupWindow.s_EW.SetData(m_rgStrEffectKind[idx], m_EffectNames, m_rgEffectID[idx, m_nRenderSkillActionIdx], 0f, Color.yellow, 20, pos, 500f, delegate(int EffectIdx)
///         {
///             // 여기서 처리.
///         }
///     }
/// }
/// </summary>
public class GUIPopupWindow : EditorWindow
{
    string[]                        m_strPopup;
    Color                           m_defColor;
    int                             m_ViewCnt;
    float                           m_guiWidth;
    Action<int>                     m_CallBack;
    int                             m_selectIdx;

    static Vector2                  m_Scroll;    // 기존위치 저장하기 위해서 static으로 선언.
    float                           m_ItemHeight = 18f;

    /// <summary>
    /// 데이터를 셋팅한다.
    /// </summary>
    /// <param name="strPopup">팝업 리스트에 들어갈 string 배열</param>
    /// <param name="selectIdx">현재 선택된 인덱스</param>
    /// <param name="guiWidth">GUILayout.BeginScrollView의 Width값</param>
    /// <param name="defColor">선택되지 않은 리스트에 색깔</param>
    /// <param name="ViewCnt">팝업창에 보여질 세로측 개수</param>
    /// <param name="mousePos">마우스 위치. 이 위치를 기준으로 팝업창이 생성된다.</param>
    /// <param name="WindowWidth">팝업 윈도우의 넓이</param>
    /// <param name="CallBack">팝업 리스트를 선택했을 때 받을 콜백함수</param>
    public void SetData(string[] strPopup, int selectIdx, float guiWidth, Color defColor, int ViewCnt, Vector2 mousePos, float WindowWidth, Action<int> CallBack)
    {
        m_strPopup = strPopup;
        m_selectIdx = selectIdx;
        m_guiWidth  = guiWidth;
        m_defColor  = defColor;
        m_ViewCnt   = ViewCnt;
        m_CallBack  = CallBack;

        float height =  m_ViewCnt * m_ItemHeight + 40f;  // 40f (close버튼 + 스페이스 값)
        minSize = new Vector2 (WindowWidth, height);
    }
    
    void OnGUI()
    {
        if (m_strPopup == null || (m_strPopup!=null&& m_strPopup.Length==0))
        {
            if (GUILayout.Button("Close"))
                this.Close();
            return;
        }

        if( false == EditorApplication.isPlaying )
        {
            this.Close();
        }

        GUI.color = m_defColor;
        GUILayout.BeginHorizontal();
        {
            if (m_guiWidth == 0f)
                m_Scroll = GUILayout.BeginScrollView(m_Scroll, GUILayout.Height(m_ViewCnt * m_ItemHeight));
            else
                m_Scroll = GUILayout.BeginScrollView(m_Scroll, GUILayout.Width(m_guiWidth), GUILayout.Height(m_ViewCnt * m_ItemHeight));
            {
                int MaxCNT = m_strPopup.Length;
                int FirstIndex;

                FirstIndex = (int)(m_Scroll.y / m_ItemHeight);
                FirstIndex = Mathf.Clamp(FirstIndex, 0, Mathf.Max(0, MaxCNT - m_ViewCnt));

                GUILayout.Space(FirstIndex * m_ItemHeight);
                for (int i = FirstIndex; i < Mathf.Min(MaxCNT, FirstIndex + m_ViewCnt); ++i)
                {
                    if (i == m_selectIdx)
                        GUI.color = Color.green;

                    if (GUILayout.Button(m_strPopup[i]))
                    {
                        m_selectIdx = i;
                        m_CallBack(m_selectIdx);
                        this.Close();
                    }

                    if (i == m_selectIdx)
                    {
                        GUI.color = m_defColor;
                    }
                }
                GUILayout.Space(Mathf.Max(0, (MaxCNT - FirstIndex - m_ViewCnt) * m_ItemHeight));
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndHorizontal();

        GUI.color = Color.green;
        if (GUILayout.Button("Close"))
            this.Close();
        GUI.color = Color.white;
    }
}
#endif //UNITY_EDITOR