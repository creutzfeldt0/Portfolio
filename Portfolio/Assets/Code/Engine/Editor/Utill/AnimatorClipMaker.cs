using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

public class AnimatorClipMaker : EditorWindow
{
    private GUIStyle                    m_Style;

    private Color                       orange = new Color(1f, 187f / 255f, 0f);

    private AnimatorController          m_AnimatorController;
    private List<AnimationClip>         m_AnimationClipFiles = new List<AnimationClip>();

    [MenuItem("CustomTool/AnimatorController")]
    static public void Make()
    {
        var window = EditorWindow.GetWindow<AnimatorClipMaker>(false, "Managing Animatior Controller", true);
        window.Show();
    }

    private void OnEnable()
    {
        m_Style = new GUIStyle();
        m_Style.normal.textColor = orange;

        ClearEditor();
    }

    private void OnGUI()
    {
        UI_FileLoader();
    }

    private void UI_FileLoader()
    {
        GUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();

        GUI.contentColor = orange;

        GUILayout.Label("AnimatorController : ", m_Style, GUILayout.Width(50f));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        AnimatorController loadController = (AnimatorController)EditorGUILayout.ObjectField(m_AnimatorController, typeof(AnimatorController), true);
        EditorGUILayout.EndHorizontal();

        if(null == loadController)  
        {
            ClearEditor();
            return;
        }
        
        if( loadController != m_AnimatorController )
        {
            m_AnimatorController = loadController;
            LoadAnimatorController();
            return;
        }
        else
        {
            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AnimatorClips : ", m_Style, GUILayout.Width(50f));
            EditorGUILayout.EndHorizontal();

            if( null != m_AnimationClipFiles[m_AnimationClipFiles.Count-1] )
            {
                m_AnimationClipFiles.Add(null);
            }
            else if( 1 < m_AnimationClipFiles.Count && null == m_AnimationClipFiles[m_AnimationClipFiles.Count-2] )
            {
                m_AnimationClipFiles.RemoveAt( m_AnimationClipFiles.Count-1 );
            }
        }

        for (int i = 0; i < m_AnimationClipFiles.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            m_AnimationClipFiles[i] = (AnimationClip)EditorGUILayout.ObjectField(m_AnimationClipFiles[i], typeof(AnimationClip), true);
            EditorGUILayout.EndHorizontal();
        }

        bool btnResult = GUILayout.Button("Submit", EditorStyles.toolbarButton);

        if (true == btnResult)
        {
            List<AnimationClip> saveClips = new List<AnimationClip>();

            for (int i = 0; i < m_AnimationClipFiles.Count; ++i)
            {
                if(null != m_AnimationClipFiles[i])
                {
                    saveClips.Add(m_AnimationClipFiles[i]);
                }
            }

            OnSubmit(saveClips);
            
            EditorUtility.DisplayDialog("애니메이션 컨트롤러", "애니메이션 컨트롤러가 변경되었습니다.", "확인");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LoadAnimatorController();
        }
    }

    public void OnSubmit(List<AnimationClip> clips)
    {
        string parentsPath = AssetDatabase.GetAssetPath( m_AnimatorController );
        AnimationClip copyClip;
        UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath( parentsPath );

        for(int i=0; i<objs.Length; i++)
        {
            if(objs[i] is AnimationClip)
            {
                AssetDatabase.RemoveObjectFromAsset( objs[i] );
            }
        }

        for (int i = 0; i < clips.Count; i++)
        {
            copyClip = UnityEngine.Object.Instantiate<AnimationClip>( clips[i] );
            copyClip.name = copyClip.name.Replace("(Clone)", "");
            AssetDatabase.AddObjectToAsset( copyClip, m_AnimatorController );
            AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(copyClip) );

            var state = m_AnimatorController.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name.Equals(clips[i].name)).state;

            if (state != null)
            {
                m_AnimatorController.SetStateEffectiveMotion(state, copyClip);
            }

            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( clips[i] ) );
        }

        /*objs = AssetDatabase.LoadAllAssetsAtPath( parentsPath );

        for(int i=0; i<objs.Length; i++)
        {
            if( objs[i] is AnimationState )
            {
                aniState = (AnimationState)objs[i];
            }
        }*/
    }

    private void LoadAnimatorController()
    {
        m_AnimationClipFiles.Clear();

        UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath( AssetDatabase.GetAssetPath( m_AnimatorController) );

        for(int i=0; i<objs.Length; i++)
        {
            if(objs[i] is AnimationClip)
            {
                m_AnimationClipFiles.Add( (AnimationClip)objs[i] );
            }
        }

        m_AnimationClipFiles.Add(null);
    }

    public void ClearEditor()
    {
        m_AnimatorController = null;
        m_AnimationClipFiles.Clear();
    }
}
