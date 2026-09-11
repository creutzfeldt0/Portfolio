using UnityEngine;
using UnityEditor;
public class MissingScriptFinder : EditorWindow
{
    [MenuItem("CustomTool/FindMissingScripts")]
    private static void FindMissingScripts()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        for (int i =0; i<roots.Length; ++i)
        {
            FindChildAndCheck( roots[i].transform );
        }
    }

    private static void FindChildAndCheck( Transform parent )
    {
        CheckScript(parent);

        for (int i = 0; i < parent.childCount; ++i)
        {
            FindChildAndCheck(parent.GetChild(i));
        }
    }

    private static void CheckScript( Transform trans )
    {
        Component[] components = trans.gameObject.GetComponents<Component>();

        for (int i = 0; i < components.Length; ++i)
        {
            if (null == components[i])
            {
                string s = trans.gameObject.name;
                Transform t = trans;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, trans.gameObject);
            }
        }
    }
}