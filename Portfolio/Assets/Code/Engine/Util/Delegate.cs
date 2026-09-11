using System.Collections;
using UnityEngine;

//델리게이트 관련
namespace Method
{
    public delegate void FunctionVoid();
    public delegate void FunctionVoidCallback(FunctionVoid callback);
    //public delegate void FunctionBool(bool arg);
    public delegate void FunctionBool(params bool[] args);
    public delegate void FunctionInt(int arg);
    public delegate void FunctionFloat(float arg);
    public delegate void FunctionString(string arg);
    public delegate void FunctionObject(params object[] arg);
    public delegate void FunctionTemplate<T>(T arg);
    public delegate void FunctionTemplate<T1, T2>(T1 arg1, T2 arg2);
    public delegate void FunctionVector3(Vector3 arg);
    public delegate void FunctionGameObject(GameObject arg);
    public delegate IEnumerator FunctionCoroutine();
}
