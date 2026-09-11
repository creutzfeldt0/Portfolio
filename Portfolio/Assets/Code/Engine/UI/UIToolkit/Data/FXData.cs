using UnityEngine;
using System;

[Serializable]
public class FXData
{
    public int                  Index;
    public string               FXName;
    public string               FXPath;
    public GameObject           FXPrefab;        
    public float                FXNotiTime;
    public float                FXDeleteTime;
    public bool                 IsSpineBack;
    public int                  FXType;
    public bool                 IsBoneChase;
    public Spine.Bone           ChasedBone;
    public  int                 SelectedBoneIndex;
    public Vector3              FXPosition = Vector3.zero;
    public Vector3              FXScale = Vector3.one;
    public string               FXTypeData;   

    public FXData(){}
        
    public FXData( FXData data )
    {
        Index               = data.Index;
        FXName              = data.FXName;
        FXPath              = data.FXPath;
        FXPrefab            = data.FXPrefab;
        FXNotiTime          = data.FXNotiTime;
        FXDeleteTime        = data.FXDeleteTime;
        IsSpineBack         = data.IsSpineBack;
        FXType              = data.FXType;
        IsBoneChase         = data.IsBoneChase;
        ChasedBone          = data.ChasedBone;
        SelectedBoneIndex   = data.SelectedBoneIndex;
        FXPosition          = data.FXPosition;
        FXScale             = data.FXScale;
        FXTypeData          = data.FXTypeData;
    }
}