using UnityEngine;
using UnityEditor;
using System.IO;

public class LabelLoader : LoaderBase
{
    [SerializeField] [HideInInspector] private bool         m_IsTrueTypeFont = true;
    [SerializeField] [HideInInspector] private int          m_FontIndex = 0;
    [SerializeField] [HideInInspector] private int          m_TextIndex = 0;
    [SerializeField] [HideInInspector] private UILabel      m_Label;

    private void OnDestroy()
    {
        m_Label = null;
    }

    private void Awake()
    {
        m_Label = gameObject.GetComponent<UILabel>();

        if (null == m_Label)
        {
#if ASSETBUNDLE
            Debug.LogError(string.Format("[LabelLoader] {0} m_Label == null", gameObject.name));
#endif
            return;
        }

        RefreshFont();
    }

    private void OnValidate()
    {
        if (null == m_Label)
            m_Label = gameObject.GetComponent<UILabel>();

        if (null == m_Label)
            return;

        RefreshText();
    }

    public void RefreshFont()
    {
        if (m_IsTrueTypeFont)
        {
            Font font = null; //LanguageMgr.Instance.GetFont( m_FontIndex ); 저장된 폰트 인덱스에 맞는 폰트를 가져온다.

            if (null == font)
            {
#if ASSETBUNDLE
                Debug.LogError(string.Format("[LabelLoader] {0} font == null", gameObject.name));
#endif
#if UNITY_EDITOR
                if (null != m_Label.trueTypeFont)
                {
                    font = m_Label.trueTypeFont;

                    string path = AssetDatabase.GetAssetPath(font);
                    m_Type = BUNDLE_TYPE.ASSET; // Util.AssetBundleFunc.GetBundleTypeToPath(path); 저장된 경로에 맞는 번들 타입을 가져온다.

                    m_FontIndex = int.Parse( path.Substring( path.Length - 1 ) );
                    m_ResourcePath = string.Empty;
                }
                else
                {
                    Debug.LogError(string.Format("[LabelLoader] {0} font == null", gameObject.name));
                    return;
                }
#endif
            }

            m_Label.trueTypeFont = font;
        }
        else
        {
            NGUIFont font = ResourceMgr.Instance.Load<NGUIFont>( m_Type, m_ResourcePath );

            if ( null == font )
            {
#if ASSETBUNDLE
                Debug.LogError(string.Format("[LabelLoader] {0} font == null", gameObject.name));
#endif
#if UNITY_EDITOR
                if (null != m_Label.bitmapFont)
                {
                    font = (NGUIFont)m_Label.bitmapFont;

                    m_ResourcePath = AssetDatabase.GetAssetPath(font).ToLower();
                    m_ResourcePath = m_ResourcePath.Replace("assets/resources/", "");

                    m_Type = BUNDLE_TYPE.ASSET; // Util.AssetBundleFunc.GetBundleTypeToPath(m_ResourcePath); 저장된 경로에 맞는 번들 타입을 가져온다.
                }
                else
                {
                    Debug.LogError(string.Format("[LabelLoader] {0} font == null", gameObject.name));
                }
#endif
                return;
            }

            m_Label.bitmapFont = font;
        }

        RefreshText();
    }

    public int GetTextIndex()
    {
        return m_TextIndex;
    }

    public void SetText(int index)
    {
        if (m_TextIndex != index)
        {
            m_TextIndex = index;
        }
        RefreshText();
    }

    private void RefreshText()
    {
        if (null == m_Label)
            m_Label = gameObject.GetComponent<UILabel>();

        if (m_TextIndex < 1)
        {
            //코드로 들어간 경우엔 패스한다.
        }
        else
        {
            m_Label.text = "";//LanguageMgr.Instance.Get( m_TextIndex ); 저장된 인덱스에 맞는 텍스트를 가져온다.
        }
    }

#if UNITY_EDITOR
#region 에셋번들 제작 전용 함수
    public override void RefreshResource()
    {
        if (null == m_Label)
        {
            m_Label = gameObject.GetComponent<UILabel>();

            if (null == m_Label)
            {
                Debug.LogError(string.Format("[LabelLoader.RefreshResource] {0} m_Label == null", gameObject.name));
                return;
            }
        }
        
        if (m_IsTrueTypeFont)
        {
            m_ResourcePath = string.Empty;
            m_Label.trueTypeFont = null;
        }
        else
        {
            UIFont font = ResourceMgr.Instance.Load<UIFont>(m_Type, m_ResourcePath);

            if (null == font)
            {
                Debug.LogError(string.Format("[LabelLoader.RefreshResource] {0} Bfont == null", gameObject.name));

                if (null != m_Label.bitmapFont)
                {
                    font = (UIFont)m_Label.bitmapFont;

                    m_ResourcePath = AssetDatabase.GetAssetPath(font).ToLower();
                    m_ResourcePath = m_ResourcePath.Replace("assets/resources/", "");

                    m_Type = BUNDLE_TYPE.ASSET; // Util.AssetBundleFunc.GetBundleTypeToPath(m_ResourcePath); // 저장된 경로에 맞는 번들 타입을 가져온다.
                }
                return;
            }

            m_Label.bitmapFont = font;
        }
    }

    public override void SaveResourcePath()
    {
        if (false == string.IsNullOrEmpty(m_ResourcePath)) return;

        if (null == m_Label)
        {
            m_Label = gameObject.GetComponent<UILabel>();

            if (null == m_Label)
            {
                Debug.LogError(string.Format("[LabelLoader.SaveResourcePath] {0} m_Label == null", gameObject.name));
                return;
            }
        }

        if (m_IsTrueTypeFont)
        {
            m_Label.trueTypeFont = null;
            m_ResourcePath = string.Empty;
            m_Type = BUNDLE_TYPE.NON_ASSET;
        }
        else
        {
            if (null == m_Label.bitmapFont)
            {
                Debug.LogWarning(string.Format("[LabelLoader.SaveResourcePath] {0} m_Label.bitmapFont == null", gameObject.name));
                return;
            }

            m_ResourcePath = AssetDatabase.GetAssetPath((Object)m_Label.bitmapFont).ToLower();
            m_ResourcePath = m_ResourcePath.Replace("assets/resources/", "");

            m_Type = BUNDLE_TYPE.ASSET; // Util.AssetBundleFunc.GetBundleTypeToPath(m_ResourcePath); // 저장된 경로에 맞는 번들 타입을 가져온다.
        }
    }

    public override void DeleteResource()
    {
        if (string.IsNullOrEmpty(m_ResourcePath))
        {
            return;
        }

        if (null == m_Label)
        {
            m_Label = gameObject.GetComponent<UILabel>();

            if (null == m_Label)
            {
                Debug.LogError(string.Format("[LabelLoader.DeleteResource] {0} m_Label == null", gameObject.name));
                return;
            }
        }

        m_Label.trueTypeFont = null;
        m_Label.bitmapFont = null;
        m_Label = null;
    }
#endregion
#endif
}
