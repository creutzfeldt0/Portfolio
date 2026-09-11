using UnityEngine;
using System;

[Serializable]
public class FXData_SaveObj
{
#if UNITY_EDITOR
    public  int                 Index;
    public string               FXName;
    public string               FXPath;
    public float                FXNotiTime;
    public float                FXDeleteTime;
    public bool                 IsSpineBack;
    public bool                 IsBoneChase;
    public Spine.Bone           ChasedBone;
    public  int                 SelectedBoneIndex;
    public Vector3              FXPosition;
    public Vector3              FXScale;
    public int                  FXType;
    public string               FXTypeData;   

    public FXData_SaveObj( FXData data )
    {
        Index               = data.Index;
        FXName              = data.FXName;
        FXPath              = data.FXPath;
        FXNotiTime          = data.FXNotiTime;
        FXDeleteTime        = data.FXDeleteTime;
        IsSpineBack         = data.IsSpineBack;
        IsBoneChase         = data.IsBoneChase;
        ChasedBone          = data.ChasedBone;
        SelectedBoneIndex   = data.SelectedBoneIndex;
        FXPosition          = data.FXPosition;
        FXScale             = data.FXScale;
        FXType              = data.FXType;
        FXTypeData          = data.FXTypeData;
    }

    public FXData ConvertFXData( GameObject prefab )
    {
        FXData fxData = new FXData()
        {
            Index               = this.Index,
            FXName              = this.FXName,
            FXPath              = this.FXPath,
            FXPrefab            = prefab,
            FXNotiTime          = this.FXNotiTime,
            FXDeleteTime        = this.FXDeleteTime,    
            IsSpineBack         = this.IsSpineBack,
            IsBoneChase         = this.IsBoneChase,
            ChasedBone          = this.ChasedBone,
            SelectedBoneIndex   = this.SelectedBoneIndex,
            FXPosition          = this.FXPosition,
            FXScale             = this.FXScale,
            FXType              = this.FXType,
            FXTypeData          = this.FXTypeData,
        };

        return fxData;
    }
#endif // UNITY_EDITOR
}