using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [Feldt] 코루틴 상에서 new로 생성하게 되면 가비지컬렉팅 증가...
/// </summary>
public static class YieldCache
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        WaitForSeconds wfs;
        if (!_timeInterval.TryGetValue(seconds, out wfs))
            _timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }
}

class FloatComparer : IEqualityComparer<float>
{
    bool IEqualityComparer<float>.Equals (float x, float y)
    {
        return x == y;
    }
    int IEqualityComparer<float>.GetHashCode (float obj)
    {
        return obj.GetHashCode();
    }
}