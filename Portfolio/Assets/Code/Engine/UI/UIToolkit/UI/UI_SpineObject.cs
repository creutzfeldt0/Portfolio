using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Spine.Unity;
using Spine.Unity.Editor;
#endif  // UNITY_EDITOR

public class UI_SpineObject : MonoBehaviour
{
#if UNITY_EDITOR
    private Transform                          m_SpineParents;

    private void OnDestroy() 
    {
        m_SpineParents                   = null;
    }

    private void Awake() 
    {
        m_SpineParents = this.transform;
    }
    
    public void OnRefreshSpine()
    {   
        if(null == m_SpineParents)   return;

        if( 0 != m_SpineParents.childCount )
        {
            //유니티 에디터가 play중일 시 DestroyImmediate 로 제거됨. 
            m_SpineParents.DestroyChildren();
        }

        SkeletonDataAsset dataAsset =  Manager_SpineFX.Instance.GetSkeletonDataAsset(); 


        if(null == dataAsset)   return;

        var spawnMenuData = new SpineEditorUtilities.DragAndDropInstantiation.SpawnMenuData
        {
            skeletonDataAsset = Manager_SpineFX.Instance.GetSkeletonDataAsset(),
            spawnPoint = Vector3.zero,
            parent = m_SpineParents,
            instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonAnimation(data),
            isUI = false
        };

        SpineEditorUtilities.DragAndDropInstantiation.HandleSkeletonComponentDrop( spawnMenuData );

        if( 0 == m_SpineParents.childCount ) return;

        Transform spineTransform = m_SpineParents.GetChild(0);

        if( null == spineTransform ) return;

        spineTransform.gameObject.name = dataAsset.name.Replace("_SkeletonData", "");

        spineTransform.localScale = Vector3.one * Manager_SpineFX.Instance.GetSpineScale();

        SkeletonAnimation skeletonAnimation = spineTransform.GetComponent<SkeletonAnimation>();

        Manager_SpineFX.Instance.SetSkeletonAni( skeletonAnimation );

        skeletonAnimation.skeleton.SetSkin( skeletonAnimation.skeleton.Data.DefaultSkin );

        //Layer변경..
        int layer = LayerMask.NameToLayer("Default");
        Manager_SpineFX.Instance.SetSpineLayer( layer );

        spineTransform.gameObject.layer = layer;
        var renderer = spineTransform.GetComponent<Renderer>();
        renderer.sortingLayerName = "GameMiddle";
        Manager_SpineFX.Instance.SetSpineSortOrderLayerName( renderer.sortingLayerName );

        NGUITools.SetChildLayer( spineTransform, layer );
       
        //선택된 오브젝트가 HandleSkeletonComponentDrop에서 바뀌므로 다시 되돌려준다.
        Selection.activeGameObject = gameObject;
    }
    
#endif  // UNITY_EDITOR
}
