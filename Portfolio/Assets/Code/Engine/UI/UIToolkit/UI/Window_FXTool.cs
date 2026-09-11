
#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// 스파인툴 에디터 윈도우..
/// 이 위에 UIToolkit UI를 띄운다..
/// </summary>
public class Window_FXTool : EditorWindow
{
    private static Window_FXTool                m_Instance = null;

    private const string                        m_ScenePath = "Assets/1_Scene/Scene_SpineTool.unity";

    private Scene_SpineTool                     m_SceneSpineTool;
    private   VisualElement                     m_UIRoot;

    ~Window_FXTool()
    {
        m_SceneSpineTool = null;
        m_UIRoot = null;
    }

    public static void OnMenuSelectFXTool()
    {
        if( false == EditorApplication.isPlaying )
        {
            EditorSceneManager.OpenScene(m_ScenePath, OpenSceneMode.Single );

            EditorApplication.isPlaying = true;
        }
        else
        {
            Scene curScene = EditorSceneManager.GetActiveScene();

            if(  curScene.name != "Scene_SpineTool" )
            {
                EditorSceneManager.LoadScene( "Scene_SpineTool" );
            }
        }
    }

    public static Window_FXTool Instance
    {
        get
        {
            if(null == m_Instance)
            {
                m_Instance = EditorWindow.GetWindow(typeof(Window_FXTool), false, "FX", true) as Window_FXTool;
            }

            return m_Instance;
        }
    }

    public static bool IsNullInstance()
    {
        return (null == m_Instance);
    }

    public void Initialize( Scene_SpineTool spineScene )
    {
        m_SceneSpineTool = spineScene;

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Data/None/SpineFXEditor/UXML/UXML_Window_FXTool.uxml");
        m_UIRoot = visualTree.Instantiate();

        // A stylesheet can be added to a VisualElement.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Data/None/SpineFXEditor/USS/USS_Window_FXTool.uss");
        m_UIRoot.styleSheets.Add(styleSheet);

        root.Add(m_UIRoot);
    }

    public VisualElement GetUIRoot()
    {
        return m_UIRoot;
    }
}
#endif //UNITY_EDITOR