//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple sprite animation. Attach to a sprite and specify a common prefix such as "idle" and it will cycle through them.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Animation")]
public class UISpriteAnimation : MonoBehaviour
{
	/// <summary>
	/// Index of the current frame in the sprite animation.
	/// </summary>

	public int frameIndex = 0;

	[HideInInspector][SerializeField] protected int mFPS = 30;
	[HideInInspector][SerializeField] protected string mPrefix = "";
	[HideInInspector][SerializeField] protected bool mLoop = true;
	[HideInInspector][SerializeField] protected bool mSnap = true;
	[HideInInspector][SerializeField] protected bool mReverse = false;

    protected UISprite mSprite;
	protected float mDelta = 0f;
	protected bool mActive = true;
	protected List<string> mSpriteNames = new List<string>();

	/// <summary>
	/// Number of frames in the animation.
	/// </summary>

	public int frames { get { return mSpriteNames.Count; } }

	/// <summary>
	/// Animation framerate.
	/// </summary>

	public int framesPerSecond { get { return mFPS; } set { mFPS = value; } }

	/// <summary>
	/// Set the name prefix used to filter sprites from the atlas.
	/// </summary>

	public string namePrefix { get { return mPrefix; } set { if (mPrefix != value) { mPrefix = value; RebuildSpriteList(); } } }

	/// <summary>
	/// Set the animation to be looping or not
	/// </summary>

	public bool loop { get { return mLoop; } set { mLoop = value; } }

    /// <summary>
    /// Set the animation play reverse. always need to call RebuildSpriteList or set namePrefix
    /// </summary>
    /// 
    public bool reverse { get { return mReverse; } set { mReverse = value; } }

	/// <summary>
	/// Returns is the animation is still playing or not
	/// </summary>

	public bool isPlaying { get { return mActive; } }

	/// <summary>
	/// Rebuild the sprite list first thing.
	/// </summary>

	protected virtual void Start () { RebuildSpriteList(); }

	/// <summary>
	/// Advance the sprite animation process.
	/// </summary>

	protected virtual void Update ()
	{
		if (mActive && mSpriteNames.Count > 1 && Application.isPlaying && mFPS > 0)
		{
			mDelta += Mathf.Min(1f, RealTime.deltaTime);
			float rate = 1f / mFPS;

			while (rate < mDelta)
			{
				mDelta = (rate > 0f) ? mDelta - rate : 0f;

				if (++frameIndex >= mSpriteNames.Count)
				{
					frameIndex = 0;
					mActive = mLoop;
				}

				if (mActive)
				{
					mSprite.spriteName = mSpriteNames[frameIndex];
					if (mSnap) mSprite.MakePixelPerfect();
				}
			}
		}
	}

	/// <summary>
	/// Rebuild the sprite list after changing the sprite name.
	/// </summary>

	public void RebuildSpriteList ()
	{
		if (mSprite == null) mSprite = GetComponent<UISprite>();
		mSpriteNames.Clear();

		if (mSprite != null)
		{
			var atlas = mSprite.atlas;

			if (atlas != null)
			{
				var sprites = atlas.spriteList;

				for (int i = 0, imax = sprites.Count; i < imax; ++i)
				{
					var sprite = sprites[i];
					if (string.IsNullOrEmpty(mPrefix) || sprite.name.StartsWith(mPrefix)) mSpriteNames.Add(sprite.name);
				}
				mSpriteNames.Sort();

                if (mReverse == true)
                    mSpriteNames.Reverse();
			}
		}
	}

    /// <summary>
    /// SpriteList에서 특정 스프라이트 묶음을 특정 횟수만큼 더 추가합니다. RebuildSpriteList 후에 호출해야 합니다. 뒷 프레임부터 차례대로 Extension 해야 합니다.
    /// </summary>
    /// <param name="startIndexArr">추가할 스프라이트 인덱스 묶음</param>
    /// <param name="count">반복 시킬 횟수</param>
    public void AddExtensionSpriteFrame(int[] startIndexArr, int count)
    {
        // check start indice invalidate
        if (mSpriteNames.Count < 1)
            return;

        for (int i = 0, cnt = startIndexArr.Length; i < cnt; i++)
        {
            if (mSpriteNames.Count <= startIndexArr[i])
                return;
        }



        // 역방향이면 정방향으로 바꾼 뒤 프레임을 추가
        if (mReverse == true)
            mSpriteNames.Reverse();
        
        for (int i = 0; i < count; i++)
        {
            List<string> spriteNameList = new List<string>();
            for (int j = 0, cnt = startIndexArr.Length; j < cnt; j++)
            {
                spriteNameList.Add(mSpriteNames[startIndexArr[j]]);
            }
            mSpriteNames.InsertRange(startIndexArr[0], spriteNameList);
        }

        // 다시 역방향으로 변경
        if (mReverse == true)
            mSpriteNames.Reverse();
    }

    public void AddExtensionSpriteFrame(int startIndex, int count)
    {
        AddExtensionSpriteFrame(new int[1] { startIndex }, count);
    }

	public void AddExtensionSpriteFrameToLast(int[] startIndexArr, int count)
	{
        // check start indice invalidate
        if (mSpriteNames.Count < 1)
            return;

        for (int i = 0, cnt = startIndexArr.Length; i < cnt; i++)
        {
            if (mSpriteNames.Count <= startIndexArr[i])
                return;
        }



        // 역방향이면 정방향으로 바꾼 뒤 프레임을 추가
        if (mReverse == true)
            mSpriteNames.Reverse();

        for (int i = 0; i < count; i++)
        {
            List<string> spriteNameList = new List<string>();
            for (int j = 0, cnt = startIndexArr.Length; j < cnt; j++)
            {
                spriteNameList.Add(mSpriteNames[startIndexArr[j]]);
            }
            mSpriteNames.AddRange(spriteNameList);
        }

        // 다시 역방향으로 변경
        if (mReverse == true)
            mSpriteNames.Reverse();
    }

    public void AddExtensionSpriteFrameToLast(int startIndex, int count)
    {
        AddExtensionSpriteFrameToLast(new int[1] { startIndex }, count);
    }

    /// <summary>
    /// Reset the animation to the beginning.
    /// </summary>

    public void Play () { mActive = true; }

	/// <summary>
	/// Pause the animation.
	/// </summary>

	public void Pause () { mActive = false; }

	/// <summary>
	/// Reset the animation to frame 0 and activate it.
	/// </summary>

	public void ResetToBeginning ()
	{
		mActive = true;
		frameIndex = 0;

		if (mSprite != null && mSpriteNames.Count > 0)
		{
			mSprite.spriteName = mSpriteNames[frameIndex];
			if (mSnap) mSprite.MakePixelPerfect();
		}
	}
}
