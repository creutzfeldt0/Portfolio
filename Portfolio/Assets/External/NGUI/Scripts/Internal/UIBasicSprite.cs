using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Functionality common to both NGUI and 2D sprites brought out into a single common parent.
/// Mostly contains everything related to drawing the sprite.
/// </summary>

public abstract class UIBasicSprite : UIWidget
{
	[DoNotObfuscateNGUI] public enum Type
	{
		Simple,
		Sliced,
		Tiled,
		Filled,
		Advanced,
		Block,
        Rope,
        Environment,
        Lava,
        Water,
        SpiderWeb1,
        SpiderWeb2,

        // For Bomb Effect
        HitRB,
        HitLB,
        HitLT,
        HitRT,
    }

	[DoNotObfuscateNGUI] public enum FillDirection
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360,
	}

	[DoNotObfuscateNGUI] public enum AdvancedType
	{
		Invisible,
		Sliced,
		Tiled,
	}

	[DoNotObfuscateNGUI] public enum Flip
	{
		Nothing,
		Horizontally,
		Vertically,
		Both,
	}

	[HideInInspector][SerializeField] protected Type mType = Type.Simple;
	[HideInInspector][SerializeField] protected FillDirection mFillDirection = FillDirection.Radial360;
	[Range(0f, 1f)]
	[HideInInspector][SerializeField] protected float mFillAmount = 1f;
	[HideInInspector][SerializeField] protected bool mInvert = false;
	[HideInInspector][SerializeField] protected Flip mFlip = Flip.Nothing;
	[HideInInspector][SerializeField] protected bool mApplyGradient = false;
	[HideInInspector][SerializeField] protected Color mGradientTop = Color.white;
	[HideInInspector][SerializeField] protected Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	// Cached to avoid allocations
	[System.NonSerialized] protected Rect mInnerUV = new Rect();
	[System.NonSerialized] protected Rect mOuterUV = new Rect();

	/// <summary>
	/// When the sprite type is advanced, this determines whether the center is tiled or sliced.
	/// </summary>

	public AdvancedType centerType = AdvancedType.Sliced;

	/// <summary>
	/// When the sprite type is advanced, this determines whether the left edge is tiled or sliced.
	/// </summary>

	public AdvancedType leftType = AdvancedType.Sliced;

	/// <summary>
	/// When the sprite type is advanced, this determines whether the right edge is tiled or sliced.
	/// </summary>

	public AdvancedType rightType = AdvancedType.Sliced;

	/// <summary>
	/// When the sprite type is advanced, this determines whether the bottom edge is tiled or sliced.
	/// </summary>

	public AdvancedType bottomType = AdvancedType.Sliced;

	/// <summary>
	/// When the sprite type is advanced, this determines whether the top edge is tiled or sliced.
	/// </summary>

	public AdvancedType topType = AdvancedType.Sliced;

	/// <summary>
	/// How the sprite is drawn. It's virtual for legacy reasons (UISlicedSprite, UITiledSprite, UIFilledSprite).
	/// </summary>

	public virtual Type type
	{
		get
		{
			return mType;
		}
		set
		{
			if (mType != value)
			{
				mType = value;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Sprite flip setting.
	/// </summary>

	public Flip flip
	{
		get
		{
			return mFlip;
		}
		set
		{
			if (mFlip != value)
			{
				mFlip = value;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Direction of the cut procedure.
	/// </summary>

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Amount of the sprite shown. 0-1 range with 0 being nothing shown, and 1 being the full sprite.
	/// </summary>

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mFillAmount != val)
			{
				mFillAmount = val;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Minimum allowed width for this widget.
	/// </summary>

	override public int minWidth
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 b = border * pixelSize;
				int min = Mathf.RoundToInt(b.x + b.z);
				return Mathf.Max(base.minWidth, ((min & 1) == 1) ? min + 1 : min);
			}
			return base.minWidth;
		}
	}

	/// <summary>
	/// Minimum allowed height for this widget.
	/// </summary>

	override public int minHeight
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 b = border * pixelSize;
				int min = Mathf.RoundToInt(b.y + b.w);
				return Mathf.Max(base.minHeight, ((min & 1) == 1) ? min + 1 : min);
			}
			return base.minHeight;
		}
	}

	/// <summary>
	/// Whether the sprite should be filled in the opposite direction.
	/// </summary>

	public bool invert
	{
		get
		{
			return mInvert;
		}
		set
		{
			if (mInvert != value)
			{
				mInvert = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Whether the widget has a border for 9-slicing.
	/// </summary>

	public bool hasBorder
	{
		get
		{
			Vector4 br = border;
			return (br.x != 0f || br.y != 0f || br.z != 0f || br.w != 0f);
		}
	}

	/// <summary>
	/// Whether the sprite's material is using a pre-multiplied alpha shader.
	/// </summary>

	public virtual bool premultipliedAlpha { get { return false; } }

	/// <summary>
	/// Size of the pixel. Overwritten in the NGUI sprite to pull a value from the atlas.
	/// </summary>

	public virtual float pixelSize { get { return 1f; } }

	/// <summary>
	/// Trimmed space in the atlas around the sprite. X = left, Y = bottom, Z = right, W = top. Overridden in UISprite.
	/// </summary>
	protected virtual Vector4 padding
	{
		get { return new Vector4(0, 0, 0, 0); }
	}

#if UNITY_EDITOR
	/// <summary>
	/// Keep sane values.
	/// </summary>

	protected override void OnValidate ()
	{
		base.OnValidate();
		mFillAmount = Mathf.Clamp01(mFillAmount);
	}
#endif
    
	float _blockRatio = 0f;
    public float blockRatio
    {
        get
        {
            return _blockRatio;
        }
        set
        {
            if (_blockRatio != value)
            {
                _blockRatio = value;
                mChanged = true;
            }
        }
    }

    float _verticalRatio = 0f;
    public float verticalRatio
    {
        get
        {
            return _verticalRatio;
        }
        set
        {
            if (_verticalRatio != value)
            {
                _verticalRatio = value;
                mChanged = true;
            }
        }
    }
   

    float _RopeRatio = 0f;
    public float ropeRatio
    {
        get
        {
            return _RopeRatio;
        }
        set
        {
            if (_RopeRatio != value)
            {
                _RopeRatio = value;
                mChanged = true;
            }
        }
    }

    float _EnvironmentRatio = 0f;
    public float environmentRatio
    {
        get
        {
            return _EnvironmentRatio;
        }
        set
        {
            if (_EnvironmentRatio != value)
            {
                _EnvironmentRatio = value;
                mChanged = true;
            }
        }
    }

    float _LavaRatioV = 0f;
    public float LavaRatioV
    {
        get
        {
            return _LavaRatioV;
        }
        set
        {
            if (_LavaRatioV != value)
            {
                _LavaRatioV = value;
                mChanged = true;
            }
        }
    }

    float _LavaRatioH = 0f;
    public float LavaRatioH
    {
        get
        {
            return _LavaRatioH;
        }
        set
        {
            if (_LavaRatioH != value)
            {
                _LavaRatioH = value;
                mChanged = true;
            }
        }
    }


    float _WaterRatio = 0f;
    public float WaterRatio
    {
        get
        {
            return _WaterRatio;
        }
        set
        {
            if (_WaterRatio != value)
            {
                _WaterRatio = value;
                mChanged = true;
            }
        }
    }

    float _RandomValueForSpiderWeb = 0;
    public float RandomValueForSpiderWeb
    {
        get
        {
            return _RandomValueForSpiderWeb;
        }
        set
        {
            if (_RandomValueForSpiderWeb != value)
            {
                _RandomValueForSpiderWeb = value;
            }
        }
    }
    float _SpiderWebRatio = 0f;
    public float SpiderWebRatio
    {
        get
        {
            return _SpiderWebRatio;
        }
        set
        {
            if (_SpiderWebRatio != value)
            {
                _SpiderWebRatio = value;
                mChanged = true;
            }
        }
    }

    float _DistortionRatio = 0f;
    public float distortionRatio
    {
        get
        {
            return _DistortionRatio;
        }
        set
        {
            if (_DistortionRatio != value)
            {
                _DistortionRatio = value;
                mChanged = true;
            }
        }
    }
    public int distortionRatioDirChangeCount = 0;


    #region Fill Functions
    // Static variables to reduce garbage collection
    static protected Vector2[] mTempPos = new Vector2[4];
	static protected Vector2[] mTempUVs = new Vector2[4];

	/// <summary>
	/// Convenience function that returns the drawn UVs after flipping gets considered.
	/// X = left, Y = bottom, Z = right, W = top.
	/// </summary>

	protected Vector4 drawingUVs
	{
		get
		{
			switch (mFlip)
			{
				case Flip.Horizontally: return new Vector4(mOuterUV.xMax, mOuterUV.yMin, mOuterUV.xMin, mOuterUV.yMax);
				case Flip.Vertically: return new Vector4(mOuterUV.xMin, mOuterUV.yMax, mOuterUV.xMax, mOuterUV.yMin);
				case Flip.Both: return new Vector4(mOuterUV.xMax, mOuterUV.yMax, mOuterUV.xMin, mOuterUV.yMin);
				default: return new Vector4(mOuterUV.xMin, mOuterUV.yMin, mOuterUV.xMax, mOuterUV.yMax);
			}
		}
	}

	/// <summary>
	/// Final widget's color passed to the draw buffer.
	/// </summary>

	protected Color drawingColor
	{
		get
		{
			Color colF = color;
			colF.a = finalAlpha;
			if (premultipliedAlpha) colF = NGUITools.ApplyPMA(colF);
			return colF;
		}
	}

	/// <summary>
	/// Fill the draw buffers.
	/// </summary>

	protected void Fill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, Rect outer, Rect inner)
	{
		mOuterUV = outer;
		mInnerUV = inner;

		var v = drawingDimensions;
		var u = drawingUVs;
		var c = drawingColor;

		switch (type)
		{
			case Type.Simple:
			SimpleFill(verts, uvs, cols, ref v, ref u, ref c);
			break;

			case Type.Sliced:
			SlicedFill(verts, uvs, cols, ref v, ref u, ref c);
			break;

			case Type.Filled:
			FilledFill(verts, uvs, cols, ref v, ref u, ref c);
			break;

			case Type.Tiled:
			TiledFill(verts, uvs, cols, ref v, ref c);
			break;

			case Type.Advanced:
			AdvancedFill(verts, uvs, cols, ref v, ref u, ref c);
			break;

			case Type.Block:
                BlockFill(verts, uvs, cols);
                break;

            case Type.Rope:
                RopeFill(verts, uvs, cols);
                break;

            case Type.Environment:
                EnvironmentFill(verts, uvs, cols);
                break;
            case Type.Lava:
                LavaFill(verts, uvs, cols);
                break;

            case Type.Water:
                WaterFill(verts, uvs, cols);
                break;

            case Type.SpiderWeb1:
                SpiderWeb1Fill(verts, uvs, cols);
                break;

            case Type.SpiderWeb2:
                SpiderWeb2Fill(verts, uvs, cols);
                break;

            case Type.HitRB:
            case Type.HitLB:
            case Type.HitLT:
            case Type.HitRT:
                HitFill(verts, uvs, cols);
                break;
        }
	}

	/// <summary>
	/// Regular sprite fill function is quite simple.
	/// </summary>

	protected void SimpleFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, ref Vector4 v, ref Vector4 u, ref Color c)
	{
		verts.Add(new Vector3(v.x, v.y));
		verts.Add(new Vector3(v.x, v.w));
		verts.Add(new Vector3(v.z, v.w));
		verts.Add(new Vector3(v.z, v.y));

		uvs.Add(new Vector2(u.x, u.y));
		uvs.Add(new Vector2(u.x, u.w));
		uvs.Add(new Vector2(u.z, u.w));
		uvs.Add(new Vector2(u.z, u.y));

		if (!mApplyGradient)
		{
			cols.Add(c);
			cols.Add(c);
			cols.Add(c);
			cols.Add(c);
		}
		else
		{
			AddVertexColours(cols, ref c, 1, 1);
			AddVertexColours(cols, ref c, 1, 2);
			AddVertexColours(cols, ref c, 2, 2);
			AddVertexColours(cols, ref c, 2, 1);
		}
	}

	/// <summary>
	/// Sliced sprite fill function is more complicated as it generates 9 quads instead of 1.
	/// </summary>

	protected void SlicedFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, ref Vector4 v, ref Vector4 u, ref Color gc)
	{
		Vector4 br = border * pixelSize;
		
		if (br.x == 0f && br.y == 0f && br.z == 0f && br.w == 0f)
		{
			SimpleFill(verts, uvs, cols, ref v, ref u, ref gc);
			return;
		}

		mTempPos[0].x = v.x;
		mTempPos[0].y = v.y;
		mTempPos[3].x = v.z;
		mTempPos[3].y = v.w;

		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + br.z;
			mTempPos[2].x = mTempPos[3].x - br.x;

			mTempUVs[3].x = mOuterUV.xMin;
			mTempUVs[2].x = mInnerUV.xMin;
			mTempUVs[1].x = mInnerUV.xMax;
			mTempUVs[0].x = mOuterUV.xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + br.x;
			mTempPos[2].x = mTempPos[3].x - br.z;

			mTempUVs[0].x = mOuterUV.xMin;
			mTempUVs[1].x = mInnerUV.xMin;
			mTempUVs[2].x = mInnerUV.xMax;
			mTempUVs[3].x = mOuterUV.xMax;
		}

		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + br.w;
			mTempPos[2].y = mTempPos[3].y - br.y;

			mTempUVs[3].y = mOuterUV.yMin;
			mTempUVs[2].y = mInnerUV.yMin;
			mTempUVs[1].y = mInnerUV.yMax;
			mTempUVs[0].y = mOuterUV.yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + br.y;
			mTempPos[2].y = mTempPos[3].y - br.w;

			mTempUVs[0].y = mOuterUV.yMin;
			mTempUVs[1].y = mInnerUV.yMin;
			mTempUVs[2].y = mInnerUV.yMax;
			mTempUVs[3].y = mOuterUV.yMax;
		}

		for (int x = 0; x < 3; ++x)
		{
			int x2 = x + 1;

			for (int y = 0; y < 3; ++y)
			{
				if (centerType == AdvancedType.Invisible && x == 1 && y == 1) continue;

				int y2 = y + 1;

				verts.Add(new Vector3(mTempPos[x].x, mTempPos[y].y));
				verts.Add(new Vector3(mTempPos[x].x, mTempPos[y2].y));
				verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y2].y));
				verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y].y));

				uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y].y));
				uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y2].y));
				uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y2].y));
				uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y].y));

				if (!mApplyGradient)
				{
					cols.Add(gc);
					cols.Add(gc);
					cols.Add(gc);
					cols.Add(gc);
				}
				else
				{
					AddVertexColours(cols, ref gc, x, y);
					AddVertexColours(cols, ref gc, x, y2);
					AddVertexColours(cols, ref gc, x2, y2);
					AddVertexColours(cols, ref gc, x2, y);
				}
			}
		}
	}
	
	/// <summary>
	/// Adds a gradient-based vertex color to the sprite.
	/// </summary>

	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	void AddVertexColours (List<Color> cols, ref Color color, int x, int y)
	{
		Vector4 br = border * pixelSize;
		if (type == Type.Simple || (br.x == 0f && br.y == 0f && br.z == 0f && br.w == 0f))
		{
			if (y == 0 || y == 1)
			{
				cols.Add(color * mGradientBottom);
			}
			else if (y == 2 || y == 3)
			{
				cols.Add(color * mGradientTop);
			}
		}
		else
		{
			if (y == 0)
			{
				cols.Add(color*mGradientBottom);
			}
			if (y == 1)
			{
				var gradient = Color.Lerp(mGradientBottom, mGradientTop, br.y / mHeight);
				cols.Add(color*gradient);
			}
			if (y == 2)
			{
				var gradient = Color.Lerp(mGradientTop, mGradientBottom, br.w / mHeight);
				cols.Add(color*gradient);
			}
			if (y == 3)
			{
				cols.Add(color*mGradientTop);
			}
		}
	}

	/// <summary>
	/// Tiled sprite fill function.
	/// </summary>

	protected void TiledFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, ref Vector4 v, ref Color c)
	{
		var tex = mainTexture;
		if (tex == null) return;

		var size = new Vector2(mInnerUV.width * tex.width, mInnerUV.height * tex.height);
		size *= pixelSize;
		if (size.x < 2f || size.y < 2f) return;

		Vector4 u;
		Vector4 p;
		var padding = this.padding;

		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			u.x = mInnerUV.xMax;
			u.z = mInnerUV.xMin;
			
			p.x = padding.z * pixelSize;
			p.z = padding.x * pixelSize;
		}
		else
		{
			u.x = mInnerUV.xMin;
			u.z = mInnerUV.xMax;

			p.x = padding.x * pixelSize;
			p.z = padding.z * pixelSize;
		}

		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			u.y = mInnerUV.yMax;
			u.w = mInnerUV.yMin;

			p.y = padding.w * pixelSize;
			p.w = padding.y * pixelSize;
		}
		else
		{
			u.y = mInnerUV.yMin;
			u.w = mInnerUV.yMax;

			p.y = padding.y * pixelSize;
			p.w = padding.w * pixelSize;
		}

		float x0 = v.x;
		float y0 = v.y;
		float u0 = u.x;
		float v0 = u.y;

		while (y0 < v.w)
		{
			y0 += p.y;
			x0 = v.x;
			float y1 = y0 + size.y;
			float v1 = u.w;

			if (y1 > v.w)
			{
				v1 = Mathf.Lerp(u.y, u.w, (v.w - y0) / size.y);
				y1 = v.w;
			}

			while (x0 < v.z)
			{
				x0 += p.x;
				float x1 = x0 + size.x;
				float u1 = u.z;

				if (x1 > v.z)
				{
					u1 = Mathf.Lerp(u.x, u.z, (v.z - x0) / size.x);
					x1 = v.z;
				}

				verts.Add(new Vector3(x0, y0));
				verts.Add(new Vector3(x0, y1));
				verts.Add(new Vector3(x1, y1));
				verts.Add(new Vector3(x1, y0));

				uvs.Add(new Vector2(u0, v0));
				uvs.Add(new Vector2(u0, v1));
				uvs.Add(new Vector2(u1, v1));
				uvs.Add(new Vector2(u1, v0));

				cols.Add(c);
				cols.Add(c);
				cols.Add(c);
				cols.Add(c);

				x0 += size.x + p.z;
			}

			y0 += size.y + p.w;
		}
	}

    private const float PRECISION = 10000.0f;
    private float uvCeil(float uv)
    {
        uv *= PRECISION;
        uv = Mathf.Ceil(uv);
        return uv / PRECISION;
    }

    private float uvFloor(float uv)
    {
        uv *= PRECISION;
        uv = Mathf.Floor(uv);
        return uv / PRECISION;
    }

	/// <summary>
	/// Filled sprite fill function.
	/// </summary>

	protected void FilledFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, ref Vector4 v, ref Vector4 u, ref Color c)
	{
		if (mFillAmount < 0.001f) return;

		// Horizontal and vertical filled sprites are simple -- just end the sprite prematurely
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			if (mFillDirection == FillDirection.Horizontal)
			{
				float fill = (u.z - u.x) * mFillAmount;

				if (mInvert)
				{
					v.x = v.z - (v.z - v.x) * mFillAmount;
					u.x = u.z - fill;
				}
				else
				{
					v.z = v.x + (v.z - v.x) * mFillAmount;
					u.z = u.x + fill;
				}
			}
			else if (mFillDirection == FillDirection.Vertical)
			{
				float fill = (u.w - u.y) * mFillAmount;

				if (mInvert)
				{
					v.y = v.w - (v.w - v.y) * mFillAmount;
					u.y = u.w - fill;
				}
				else
				{
					v.w = v.y + (v.w - v.y) * mFillAmount;
					u.w = u.y + fill;
				}
			}
		}

		mTempPos[0] = new Vector2(v.x, v.y);
		mTempPos[1] = new Vector2(v.x, v.w);
		mTempPos[2] = new Vector2(v.z, v.w);
		mTempPos[3] = new Vector2(v.z, v.y);

		mTempUVs[0] = new Vector2(u.x, u.y);
		mTempUVs[1] = new Vector2(u.x, u.w);
		mTempUVs[2] = new Vector2(u.z, u.w);
		mTempUVs[3] = new Vector2(u.z, u.y);

		if (mFillAmount < 1f)
		{
			if (mFillDirection == FillDirection.Radial90)
			{
				if (RadialCut(mTempPos, mTempUVs, mFillAmount, mInvert, 0))
				{
					for (int i = 0; i < 4; ++i)
					{
						verts.Add(mTempPos[i]);
						uvs.Add(mTempUVs[i]);
						cols.Add(c);
					}
				}
				return;
			}

			if (mFillDirection == FillDirection.Radial180)
			{
				for (int side = 0; side < 2; ++side)
				{
					float fx0, fx1, fy0, fy1;

					fy0 = 0f;
					fy1 = 1f;

					if (side == 0) { fx0 = 0f; fx1 = 0.5f; }
					else { fx0 = 0.5f; fx1 = 1f; }

					mTempPos[0].x = Mathf.Lerp(v.x, v.z, fx0);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(v.x, v.z, fx1);
					mTempPos[3].x = mTempPos[2].x;

					mTempPos[0].y = Mathf.Lerp(v.y, v.w, fy0);
					mTempPos[1].y = Mathf.Lerp(v.y, v.w, fy1);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;

					mTempUVs[0].x = Mathf.Lerp(u.x, u.z, fx0);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(u.x, u.z, fx1);
					mTempUVs[3].x = mTempUVs[2].x;

					mTempUVs[0].y = Mathf.Lerp(u.y, u.w, fy0);
					mTempUVs[1].y = Mathf.Lerp(u.y, u.w, fy1);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;

					float val = !mInvert ? fillAmount * 2f - side : mFillAmount * 2f - (1 - side);

					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(val), !mInvert, NGUIMath.RepeatIndex(side + 3, 4)))
					{
						for (int i = 0; i < 4; ++i)
						{
							verts.Add(mTempPos[i]);
							uvs.Add(mTempUVs[i]);
							cols.Add(c);
						}
					}
				}
				return;
			}

			if (mFillDirection == FillDirection.Radial360)
			{
				for (int corner = 0; corner < 4; ++corner)
				{
					float fx0, fx1, fy0, fy1;

					if (corner < 2) { fx0 = 0f; fx1 = 0.5f; }
					else { fx0 = 0.5f; fx1 = 1f; }

					if (corner == 0 || corner == 3) { fy0 = 0f; fy1 = 0.5f; }
					else { fy0 = 0.5f; fy1 = 1f; }

					mTempPos[0].x = Mathf.Lerp(v.x, v.z, fx0);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(v.x, v.z, fx1);
					mTempPos[3].x = mTempPos[2].x;

					mTempPos[0].y = Mathf.Lerp(v.y, v.w, fy0);
					mTempPos[1].y = Mathf.Lerp(v.y, v.w, fy1);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;

					mTempUVs[0].x = Mathf.Lerp(u.x, u.z, fx0);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(u.x, u.z, fx1);
					mTempUVs[3].x = mTempUVs[2].x;

					mTempUVs[0].y = Mathf.Lerp(u.y, u.w, fy0);
					mTempUVs[1].y = Mathf.Lerp(u.y, u.w, fy1);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;

					float val = mInvert ?
						mFillAmount * 4f - NGUIMath.RepeatIndex(corner + 2, 4) :
						mFillAmount * 4f - (3 - NGUIMath.RepeatIndex(corner + 2, 4));

					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(val), mInvert, NGUIMath.RepeatIndex(corner + 2, 4)))
					{
						for (int i = 0; i < 4; ++i)
						{
							verts.Add(mTempPos[i]);
							uvs.Add(mTempUVs[i]);
							cols.Add(c);
						}
					}
				}
				return;
			}
		}

		// Fill the buffer with the quad for the sprite
		for (int i = 0; i < 4; ++i)
		{
			verts.Add(mTempPos[i]);
			uvs.Add(mTempUVs[i]);
			cols.Add(c);
		}
	}

	/// <summary>
	/// Advanced sprite fill function. Contributed by Nicki Hansen.
	/// </summary>

	protected void AdvancedFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols, ref Vector4 v, ref Vector4 u, ref Color c)
	{
		var tex = mainTexture;
		if (tex == null) return;

		var br = border * pixelSize;

		if (br.x == 0f && br.y == 0f && br.z == 0f && br.w == 0f)
		{
			SimpleFill(verts, uvs, cols, ref v, ref u, ref c);
			return;
		}

		var tileSize = new Vector2(mInnerUV.width * tex.width, mInnerUV.height * tex.height);
		tileSize *= pixelSize;

		if (tileSize.x < 1f) tileSize.x = 1f;
		if (tileSize.y < 1f) tileSize.y = 1f;

		mTempPos[0].x = v.x;
		mTempPos[0].y = v.y;
		mTempPos[3].x = v.z;
		mTempPos[3].y = v.w;

		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + br.z;
			mTempPos[2].x = mTempPos[3].x - br.x;

			mTempUVs[3].x = mOuterUV.xMin;
			mTempUVs[2].x = mInnerUV.xMin;
			mTempUVs[1].x = mInnerUV.xMax;
			mTempUVs[0].x = mOuterUV.xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + br.x;
			mTempPos[2].x = mTempPos[3].x - br.z;

			mTempUVs[0].x = mOuterUV.xMin;
			mTempUVs[1].x = mInnerUV.xMin;
			mTempUVs[2].x = mInnerUV.xMax;
			mTempUVs[3].x = mOuterUV.xMax;
		}

		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + br.w;
			mTempPos[2].y = mTempPos[3].y - br.y;

			mTempUVs[3].y = mOuterUV.yMin;
			mTempUVs[2].y = mInnerUV.yMin;
			mTempUVs[1].y = mInnerUV.yMax;
			mTempUVs[0].y = mOuterUV.yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + br.y;
			mTempPos[2].y = mTempPos[3].y - br.w;

			mTempUVs[0].y = mOuterUV.yMin;
			mTempUVs[1].y = mInnerUV.yMin;
			mTempUVs[2].y = mInnerUV.yMax;
			mTempUVs[3].y = mOuterUV.yMax;
		}

		for (int x = 0; x < 3; ++x)
		{
			int x2 = x + 1;

			for (int y = 0; y < 3; ++y)
			{
				if (centerType == AdvancedType.Invisible && x == 1 && y == 1) continue;
				int y2 = y + 1;

				if (x == 1 && y == 1) // Center
				{
					if (centerType == AdvancedType.Tiled)
					{
						float startPositionX = mTempPos[x].x;
						float endPositionX = mTempPos[x2].x;
						float startPositionY = mTempPos[y].y;
						float endPositionY = mTempPos[y2].y;
						float textureStartX = mTempUVs[x].x;
						float textureStartY = mTempUVs[y].y;
						float tileStartY = startPositionY;

						while (tileStartY < endPositionY)
						{
							float tileStartX = startPositionX;
							float textureEndY = mTempUVs[y2].y;
							float tileEndY = tileStartY + tileSize.y;

							if (tileEndY > endPositionY)
							{
								textureEndY = Mathf.Lerp(textureStartY, textureEndY, (endPositionY - tileStartY) / tileSize.y);
								tileEndY = endPositionY;
							}

							while (tileStartX < endPositionX)
							{
								float tileEndX = tileStartX + tileSize.x;
								float textureEndX = mTempUVs[x2].x;

								if (tileEndX > endPositionX)
								{
									textureEndX = Mathf.Lerp(textureStartX, textureEndX, (endPositionX - tileStartX) / tileSize.x);
									tileEndX = endPositionX;
								}

								Fill(verts, uvs, cols,
									tileStartX, tileEndX,
									tileStartY, tileEndY,
									textureStartX, textureEndX,
									textureStartY, textureEndY, c);

								tileStartX += tileSize.x;
							}
							tileStartY += tileSize.y;
						}
					}
					else if (centerType == AdvancedType.Sliced)
					{
						Fill(verts, uvs, cols,
							mTempPos[x].x, mTempPos[x2].x,
							mTempPos[y].y, mTempPos[y2].y,
							mTempUVs[x].x, mTempUVs[x2].x,
							mTempUVs[y].y, mTempUVs[y2].y, c);
					}
				}
				else if (x == 1) // Top or bottom
				{
					if ((y == 0 && bottomType == AdvancedType.Tiled) || (y == 2 && topType == AdvancedType.Tiled))
					{
						float startPositionX = mTempPos[x].x;
						float endPositionX = mTempPos[x2].x;
						float startPositionY = mTempPos[y].y;
						float endPositionY = mTempPos[y2].y;
						float textureStartX = mTempUVs[x].x;
						float textureStartY = mTempUVs[y].y;
						float textureEndY = mTempUVs[y2].y;
						float tileStartX = startPositionX;

						while (tileStartX < endPositionX)
						{
							float tileEndX = tileStartX + tileSize.x;
							float textureEndX = mTempUVs[x2].x;

							if (tileEndX > endPositionX)
							{
								textureEndX = Mathf.Lerp(textureStartX, textureEndX, (endPositionX - tileStartX) / tileSize.x);
								tileEndX = endPositionX;
							}

							Fill(verts, uvs, cols,
								tileStartX, tileEndX,
								startPositionY, endPositionY,
								textureStartX, textureEndX,
								textureStartY, textureEndY, c);

							tileStartX += tileSize.x;
						}
					}
					else if ((y == 0 && bottomType != AdvancedType.Invisible) || (y == 2 && topType != AdvancedType.Invisible))
					{
						Fill(verts, uvs, cols,
							mTempPos[x].x, mTempPos[x2].x,
							mTempPos[y].y, mTempPos[y2].y,
							mTempUVs[x].x, mTempUVs[x2].x,
							mTempUVs[y].y, mTempUVs[y2].y, c);
					}
				}
				else if (y == 1) // Left or right
				{
					if ((x == 0 && leftType == AdvancedType.Tiled) || (x == 2 && rightType == AdvancedType.Tiled))
					{
						float startPositionX = mTempPos[x].x;
						float endPositionX = mTempPos[x2].x;
						float startPositionY = mTempPos[y].y;
						float endPositionY = mTempPos[y2].y;
						float textureStartX = mTempUVs[x].x;
						float textureEndX = mTempUVs[x2].x;
						float textureStartY = mTempUVs[y].y;
						float tileStartY = startPositionY;

						while (tileStartY < endPositionY)
						{
							float textureEndY = mTempUVs[y2].y;
							float tileEndY = tileStartY + tileSize.y;

							if (tileEndY > endPositionY)
							{
								textureEndY = Mathf.Lerp(textureStartY, textureEndY, (endPositionY - tileStartY) / tileSize.y);
								tileEndY = endPositionY;
							}

							Fill(verts, uvs, cols,
								startPositionX, endPositionX,
								tileStartY, tileEndY,
								textureStartX, textureEndX,
								textureStartY, textureEndY, c);

							tileStartY += tileSize.y;
						}
					}
					else if ((x == 0 && leftType != AdvancedType.Invisible) || (x == 2 && rightType != AdvancedType.Invisible))
					{
						Fill(verts, uvs, cols,
							mTempPos[x].x, mTempPos[x2].x,
							mTempPos[y].y, mTempPos[y2].y,
							mTempUVs[x].x, mTempUVs[x2].x,
							mTempUVs[y].y, mTempUVs[y2].y, c);
					}
				}
				else // Corner
				{
					if (y == 0 && bottomType == AdvancedType.Invisible) continue;
					if (y == 2 && topType == AdvancedType.Invisible) continue;
					if (x == 0 && leftType == AdvancedType.Invisible) continue;
					if (x == 2 && rightType == AdvancedType.Invisible) continue;

					Fill(verts, uvs, cols,
						mTempPos[x].x, mTempPos[x2].x,
						mTempPos[y].y, mTempPos[y2].y,
						mTempUVs[x].x, mTempUVs[x2].x,
						mTempUVs[y].y, mTempUVs[y2].y, c);
				}
			}
		}
	}

	/// <summary>
	/// Adjust the specified quad, making it be radially filled instead.
	/// </summary>

	static bool RadialCut (Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner)
	{
		// Nothing to fill
		if (fill < 0.001f) return false;

		// Even corners invert the fill direction
		if ((corner & 1) == 1) invert = !invert;

		// Nothing to adjust
		if (!invert && fill > 0.999f) return true;

		// Convert 0-1 value into 0 to 90 degrees angle in radians
		float angle = Mathf.Clamp01(fill);
		if (invert) angle = 1f - angle;
		angle *= 90f * Mathf.Deg2Rad;

		// Calculate the effective X and Y factors
		float cos = Mathf.Cos(angle);
		float sin = Mathf.Sin(angle);

		RadialCut(xy, cos, sin, invert, corner);
		RadialCut(uv, cos, sin, invert, corner);
		return true;
	}

	/// <summary>
	/// Adjust the specified quad, making it be radially filled instead.
	/// </summary>

	static void RadialCut (Vector2[] xy, float cos, float sin, bool invert, int corner)
	{
		int i0 = corner;
		int i1 = NGUIMath.RepeatIndex(corner + 1, 4);
		int i2 = NGUIMath.RepeatIndex(corner + 2, 4);
		int i3 = NGUIMath.RepeatIndex(corner + 3, 4);

		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;

				if (invert)
				{
					xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
					xy[i2].x = xy[i1].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;

				if (!invert)
				{
					xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
					xy[i3].y = xy[i2].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}

			if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
			else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
		}
		else
		{
			if (cos > sin)
			{
				sin /= cos;
				cos = 1f;

				if (!invert)
				{
					xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
					xy[i2].y = xy[i1].y;
				}
			}
			else if (sin > cos)
			{
				cos /= sin;
				sin = 1f;

				if (invert)
				{
					xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
					xy[i3].x = xy[i2].x;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}

			if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
			else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
		}
	}

	/// <summary>
	/// Helper function that adds the specified values to the buffers.
	/// </summary>

	static void Fill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols,
		float v0x, float v1x, float v0y, float v1y, float u0x, float u1x, float u0y, float u1y, Color col)
	{
		verts.Add(new Vector3(v0x, v0y));
		verts.Add(new Vector3(v0x, v1y));
		verts.Add(new Vector3(v1x, v1y));
		verts.Add(new Vector3(v1x, v0y));

		uvs.Add(new Vector2(u0x, u0y));
		uvs.Add(new Vector2(u0x, u1y));
		uvs.Add(new Vector2(u1x, u1y));
		uvs.Add(new Vector2(u1x, u0y));

		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
	}

	protected void BlockFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
        Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);
        if (verticalRatio > 0f) //(1 <- 0 ) 0:促焊??<- 1 ??��?�肺绝绢??
        {
            uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
            uv1 = new Vector2(mOuterUV.xMax - (mOuterUV.xMax - mOuterUV.xMin) * verticalRatio, mOuterUV.yMax);
            float s = v.z - (v.z - v.x) * verticalRatio;
            verts.Add(new Vector3(v.x, v.y, 0f));
            verts.Add(new Vector3(v.x, v.w, 0f));
            verts.Add(new Vector3(s, v.w, 0f));
            verts.Add(new Vector3(s, v.y, 0f));
        }
        else if (verticalRatio < 0f) //(0 -> -1 ) 0:促焊??-> -1 ?�弗?�栏?�绝绢咙 
        {
            uv0 = new Vector2(mOuterUV.xMin - (mOuterUV.xMax - mOuterUV.xMin) * verticalRatio, mOuterUV.yMin);
            uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);
            float s = v.x - (v.z - v.x) * verticalRatio;
            verts.Add(new Vector3(s, v.y, 0f));
            verts.Add(new Vector3(s, v.w, 0f));
            verts.Add(new Vector3(v.z, v.w, 0f));
            verts.Add(new Vector3(v.z, v.y, 0f));
        }
        else if (blockRatio > 0f)
        {
            uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
            uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax - (mOuterUV.yMax - mOuterUV.yMin) * blockRatio);//(mOuterUV.yMax - mOuterUV.yMin) * ratio);
            float s = (v.w - v.y) * (1f - blockRatio);
            verts.Add(new Vector3(v.x, v.y, 0f));
            verts.Add(new Vector3(v.x, v.y + s, 0f));
            verts.Add(new Vector3(v.z, v.y + s, 0f));
            verts.Add(new Vector3(v.z, v.y, 0f));
        }
        else if (blockRatio < 0f)
        {
            uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin - (mOuterUV.yMin - mOuterUV.yMax) * (1f + blockRatio));
            uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);
            float ss = (v.w - v.y);
            verts.Add(new Vector3(v.x, v.y - ss * -blockRatio + ss, 0f));
            verts.Add(new Vector3(v.x, v.y + ss, 0f));
            verts.Add(new Vector3(v.z, v.y + ss, 0f));
            verts.Add(new Vector3(v.z, v.y - ss * -blockRatio + ss, 0f));
        }
        else
        {
            verts.Add(new Vector3(v.x, v.y, 0f));
            verts.Add(new Vector3(v.x, v.w, 0f));
            verts.Add(new Vector3(v.z, v.w, 0f));
            verts.Add(new Vector3(v.z, v.y, 0f));
        }
        uvs.Add(uv0);
        uvs.Add(new Vector2(uv0.x, uv1.y));
        uvs.Add(uv1);
        uvs.Add(new Vector2(uv1.x, uv0.y));
        Color colF = color;
        colF.a = finalAlpha;
        Color32 col = colF; //atlas.premultipliedAlpha ? NGUITools.ApplyPMA(colF) : colF;
        cols.Add(col);
        cols.Add(col);
        cols.Add(col);
        cols.Add(col);
    }
    protected void RopeFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, (v.w) + (v.w - v.y) * ropeRatio));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, (v.y) + (v.w - v.y) * ropeRatio));
        uvs.Add(new Vector2(u.x, u.y));
        uvs.Add(new Vector2(u.x, u.w));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.w));  //        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.w) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.y)); //        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y) + (u.w - u.y) * ropeRatio));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        verts.Add(new Vector3((v.x + v.z) * 0.5f, (v.y) + (v.w - v.y) * ropeRatio));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, (v.w) + (v.w - v.y) * ropeRatio));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, v.y));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.y));  // (u.y) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.w));  // (u.w) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector2(u.z, u.w));
        uvs.Add(new Vector2(u.z, u.y));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
    }
    protected void EnvironmentFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        float moveRatio = (v.z - v.x) * 0.05f * _EnvironmentRatio;
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x + moveRatio, v.w));
        verts.Add(new Vector3(v.z + moveRatio, v.w));
        verts.Add(new Vector3(v.z, v.y));
        uvs.Add(new Vector2(u.x, u.y));
        uvs.Add(new Vector2(u.x, u.w));
        uvs.Add(new Vector3(u.z, u.w));
        uvs.Add(new Vector3(u.z, u.y));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
    }
    public Color lavaColor1 = new Color(1f, 1f, 1f, 1);
    public Color lavaColor2 = new Color(0.0f, 0.0f, 0.0f, 1);
    protected void LavaFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        //1
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, (v.y +v.w)*0.5f));
        verts.Add(new Vector3((v.x+v.z)*0.5f + (v.z - v.x) * _LavaRatioH, (v.y + v.w) * 0.5f + (v.w -v.y)* _LavaRatioV));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, v.y));
        uvs.Add(new Vector3(u.x, u.y));
        uvs.Add(new Vector3(u.x, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.y));
        cols.Add(gc);// lavaColor2);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//lavaColor2);
        //2
        verts.Add(new Vector3(v.x, (v.y + v.w) * 0.5f));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, v.w));
        verts.Add(new Vector3((v.x + v.z) * 0.5f + (v.z - v.x) * _LavaRatioH, (v.y + v.w) * 0.5f+ (v.w - v.y) * _LavaRatioV));
        uvs.Add(new Vector3(u.x, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.x, u.w));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.w));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y + u.w) * 0.5f));
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//lavaColor1);
        cols.Add(gc);//lavaColor1);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        //3
        verts.Add(new Vector3((v.x + v.z) * 0.5f + (v.z - v.x) * _LavaRatioH, (v.y + v.w) * 0.5f + (v.w - v.y) * _LavaRatioV));
        verts.Add(new Vector3((v.x + v.z) * 0.5f, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.w));
        uvs.Add(new Vector3(u.z, u.w));
        uvs.Add(new Vector3(u.z, (u.y + u.w) * 0.5f));
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//lavaColor1);
        cols.Add(gc);//lavaColor1);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        //4
        verts.Add(new Vector3((v.x + v.z) * 0.5f, v.y));
        verts.Add(new Vector3((v.x + v.z) * 0.5f + (v.z - v.x) * _LavaRatioH, (v.y + v.w) * 0.5f + (v.w - v.y) * _LavaRatioV));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f));
        verts.Add(new Vector3(v.z, v.y));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, u.y));
        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.z, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.z, u.y));
        cols.Add(gc);//lavaColor2);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//(lavaColor1 + lavaColor2) * 0.5f);
        cols.Add(gc);//lavaColor2);
    }
    
   
    protected void WaterFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, (v.y + v.w) * 0.5f + (v.w - v.y) * _WaterRatio));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f + (v.w - v.y) * _WaterRatio));
        verts.Add(new Vector3(v.z, v.y));
        uvs.Add(new Vector2(u.x, u.y));
        uvs.Add(new Vector2(u.x, (u.y + u.w)*0.5f));
        uvs.Add(new Vector3(u.z, (u.y + u.w) * 0.5f));  //        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.w) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector3(u.z, u.y)); //        uvs.Add(new Vector3((u.x + u.z) * 0.5f, (u.y) + (u.w - u.y) * ropeRatio));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        verts.Add(new Vector3(v.x, (v.y + v.w) * 0.5f + (v.w - v.y) * _WaterRatio));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f + (v.w - v.y) * _WaterRatio));
        uvs.Add(new Vector3(u.x, (u.y + u.w) * 0.5f));  // (u.y) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector3(u.x, u.w));  // (u.w) + (u.w - u.y) * ropeRatio));
        uvs.Add(new Vector2(u.z, u.w));
        uvs.Add(new Vector2(u.z, (u.y + u.w) * 0.5f));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
    }

    protected void SpiderWeb1Fill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        float vx = v.x;
        float vy = v.y;
        float vz = v.z;
        float vw = v.w;
        float ux = u.x;
        float uy = u.y;
        float uz = u.z;
        float uw = u.w;

        #region Left Bottom
        float vLBx = vx;
        float vLBy = vy;
        float vLBz = (vx + vz) * 0.5f;
        float vLBw = (vy + vw) * 0.5f;
        float vLB1 = (vLBy + vLBw) * 0.5f;
        float vLB2 = (vLBx + vLBz) * 0.5f;
        float vLB3 = vLB1 - _SpiderWebRatio * (vLBw - vLBy);
        float vLB4 = vLB2 - _SpiderWebRatio * (vLBz - vLBx);

        float uLBx = ux;
        float uLBy = uy;
        float uLBz = (ux + uz) * 0.5f;
        float uLBw = (uy + uw) * 0.5f;
        float uLB1 = (uLBy + uLBw) * 0.5f;
        float uLB2 = (uLBx + uLBz) * 0.5f;

        verts.Add(new Vector3(vLBx, vLBy));
        verts.Add(new Vector3(vLBx, vLB3));
        verts.Add(new Vector3(vLB4, vLB3));
        verts.Add(new Vector3(vLB4, vLBy));
        uvs.Add(new Vector2(uLBx, uLBy));
        uvs.Add(new Vector2(uLBx, uLB1));
        uvs.Add(new Vector2(uLB2, uLB1));
        uvs.Add(new Vector2(uLB2, uLBy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLB4, vLBy));
        verts.Add(new Vector3(vLB4, vLB3));
        verts.Add(new Vector3(vLBz, vLB3));
        verts.Add(new Vector3(vLBz, vLBy));
        uvs.Add(new Vector2(uLB2, uLBy));
        uvs.Add(new Vector2(uLB2, uLB1));
        uvs.Add(new Vector2(uLBz, uLB1));
        uvs.Add(new Vector2(uLBz, uLBy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLBx, vLB3));
        verts.Add(new Vector3(vLBx, vLBw));
        verts.Add(new Vector3(vLB4, vLBw));
        verts.Add(new Vector3(vLB4, vLB3));
        uvs.Add(new Vector2(uLBx, uLB1));
        uvs.Add(new Vector2(uLBx, uLBw));
        uvs.Add(new Vector2(uLB2, uLBw));
        uvs.Add(new Vector2(uLB2, uLB1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLB4, vLB3));
        verts.Add(new Vector3(vLB4, vLBw));
        verts.Add(new Vector3(vLBz, vLBw));
        verts.Add(new Vector3(vLBz, vLB3));
        uvs.Add(new Vector2(uLB2, uLB1));
        uvs.Add(new Vector2(uLB2, uLBw));
        uvs.Add(new Vector2(uLBz, uLBw));
        uvs.Add(new Vector2(uLBz, uLB1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        #endregion

        #region Right Bottom
        float vRBx = (vx + vz) * 0.5f;
        float vRBy = vy;
        float vRBz = vz;
        float vRBw = (vy + vw) * 0.5f;
        float vRB1 = (vRBy + vRBw) * 0.5f;
        float vRB2 = (vRBx + vRBz) * 0.5f;
        float vRB3 = vRB1 - _SpiderWebRatio * (vRBw - vRBy);
        float vRB4 = vRB2 + _SpiderWebRatio * (vRBz - vRBx);

        float uRBx = (ux + uz) * 0.5f;
        float uRBy = uy;
        float uRBz = uz;
        float uRBw = (uy + uw) * 0.5f;
        float uRB1 = (uRBy + uRBw) * 0.5f;
        float uRB2 = (uRBx + uRBz) * 0.5f;

        verts.Add(new Vector3(vRBx, vRBy));
        verts.Add(new Vector3(vRBx, vRB3));
        verts.Add(new Vector3(vRB4, vRB3));
        verts.Add(new Vector3(vRB4, vRBy));
        uvs.Add(new Vector2(uRBx, uRBy));
        uvs.Add(new Vector2(uRBx, uRB1));
        uvs.Add(new Vector2(uRB2, uRB1));
        uvs.Add(new Vector2(uRB2, uRBy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRB4, vRBy));
        verts.Add(new Vector3(vRB4, vRB3));
        verts.Add(new Vector3(vRBz, vRB3));
        verts.Add(new Vector3(vRBz, vRBy));
        uvs.Add(new Vector2(uRB2, uRBy));
        uvs.Add(new Vector2(uRB2, uRB1));
        uvs.Add(new Vector2(uRBz, uRB1));
        uvs.Add(new Vector2(uRBz, uRBy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRBx, vRB3));
        verts.Add(new Vector3(vRBx, vRBw));
        verts.Add(new Vector3(vRB4, vRBw));
        verts.Add(new Vector3(vRB4, vRB3));
        uvs.Add(new Vector2(uRBx, uRB1));
        uvs.Add(new Vector2(uRBx, uRBw));
        uvs.Add(new Vector2(uRB2, uRBw));
        uvs.Add(new Vector2(uRB2, uRB1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRB4, vRB3));
        verts.Add(new Vector3(vRB4, vRBw));
        verts.Add(new Vector3(vRBz, vRBw));
        verts.Add(new Vector3(vRBz, vRB3));
        uvs.Add(new Vector2(uRB2, uRB1));
        uvs.Add(new Vector2(uRB2, uRBw));
        uvs.Add(new Vector2(uRBz, uRBw));
        uvs.Add(new Vector2(uRBz, uRB1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        #endregion

        #region Left Top
        float vLTx = vx;
        float vLTy = (vy + vw) * 0.5f;
        float vLTz = (vx + vz) * 0.5f;
        float vLTw = vw;
        float vLT1 = (vLTy + vLTw) * 0.5f;
        float vLT2 = (vLTx + vLTz) * 0.5f;
        float vLT3 = vLT1 + _SpiderWebRatio * (vLTw - vLTy);
        float vLT4 = vLT2 - _SpiderWebRatio * (vLTz - vLTx);

        float uLTx = ux;
        float uLTy = (uy + uw) * 0.5f;
        float uLTz = (ux + uz) * 0.5f;
        float uLTw = uw;
        float uLT1 = (uLTy + uLTw) * 0.5f;
        float uLT2 = (uLTx + uLTz) * 0.5f;

        verts.Add(new Vector3(vLTx, vLTy));
        verts.Add(new Vector3(vLTx, vLT3));
        verts.Add(new Vector3(vLT4, vLT3));
        verts.Add(new Vector3(vLT4, vLTy));
        uvs.Add(new Vector2(uLTx, uLTy));
        uvs.Add(new Vector2(uLTx, uLT1));
        uvs.Add(new Vector2(uLT2, uLT1));
        uvs.Add(new Vector2(uLT2, uLTy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLT4, vLTy));
        verts.Add(new Vector3(vLT4, vLT3));
        verts.Add(new Vector3(vLTz, vLT3));
        verts.Add(new Vector3(vLTz, vLTy));
        uvs.Add(new Vector2(uLT2, uLTy));
        uvs.Add(new Vector2(uLT2, uLT1));
        uvs.Add(new Vector2(uLTz, uLT1));
        uvs.Add(new Vector2(uLTz, uLTy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLTx, vLT3));
        verts.Add(new Vector3(vLTx, vLTw));
        verts.Add(new Vector3(vLT4, vLTw));
        verts.Add(new Vector3(vLT4, vLT3));
        uvs.Add(new Vector2(uLTx, uLT1));
        uvs.Add(new Vector2(uLTx, uLTw));
        uvs.Add(new Vector2(uLT2, uLTw));
        uvs.Add(new Vector2(uLT2, uLT1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vLT4, vLT3));
        verts.Add(new Vector3(vLT4, vLTw));
        verts.Add(new Vector3(vLTz, vLTw));
        verts.Add(new Vector3(vLTz, vLT3));
        uvs.Add(new Vector2(uLT2, uLT1));
        uvs.Add(new Vector2(uLT2, uLTw));
        uvs.Add(new Vector2(uLTz, uLTw));
        uvs.Add(new Vector2(uLTz, uLT1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        #endregion

        #region Right Top
        float vRTx = (vx + vz) * 0.5f;
        float vRTy = (vy + vw) * 0.5f;
        float vRTz = vz;
        float vRTw = vw;
        float vRT1 = (vRTy + vRTw) * 0.5f;
        float vRT2 = (vRTx + vRTz) * 0.5f;
        float vRT3 = vRT1 + _SpiderWebRatio * (vRTw - vRTy);
        float vRT4 = vRT2 + _SpiderWebRatio * (vRTz - vRTx);

        float uRTx = (ux + uz) * 0.5f;
        float uRTy = (uy + uw) * 0.5f;
        float uRTz = uz;
        float uRTw = uw;
        float uRT1 = (uRTy + uRTw) * 0.5f;
        float uRT2 = (uRTx + uRTz) * 0.5f;

        verts.Add(new Vector3(vRTx, vRTy));
        verts.Add(new Vector3(vRTx, vRT3));
        verts.Add(new Vector3(vRT4, vRT3));
        verts.Add(new Vector3(vRT4, vRTy));
        uvs.Add(new Vector2(uRTx, uRTy));
        uvs.Add(new Vector2(uRTx, uRT1));
        uvs.Add(new Vector2(uRT2, uRT1));
        uvs.Add(new Vector2(uRT2, uRTy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRT4, vRTy));
        verts.Add(new Vector3(vRT4, vRT3));
        verts.Add(new Vector3(vRTz, vRT3));
        verts.Add(new Vector3(vRTz, vRTy));
        uvs.Add(new Vector2(uRT2, uRTy));
        uvs.Add(new Vector2(uRT2, uRT1));
        uvs.Add(new Vector2(uRTz, uRT1));
        uvs.Add(new Vector2(uRTz, uRTy));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRTx, vRT3));
        verts.Add(new Vector3(vRTx, vRTw));
        verts.Add(new Vector3(vRT4, vRTw));
        verts.Add(new Vector3(vRT4, vRT3));
        uvs.Add(new Vector2(uRTx, uRT1));
        uvs.Add(new Vector2(uRTx, uRTw));
        uvs.Add(new Vector2(uRT2, uRTw));
        uvs.Add(new Vector2(uRT2, uRT1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);

        verts.Add(new Vector3(vRT4, vRT3));
        verts.Add(new Vector3(vRT4, vRTw));
        verts.Add(new Vector3(vRTz, vRTw));
        verts.Add(new Vector3(vRTz, vRT3));
        uvs.Add(new Vector2(uRT2, uRT1));
        uvs.Add(new Vector2(uRT2, uRTw));
        uvs.Add(new Vector2(uRTz, uRTw));
        uvs.Add(new Vector2(uRTz, uRT1));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        #endregion
    }

    protected void SpiderWeb2Fill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, (v.y + v.w) * 0.5f + (v.w - v.y) * _SpiderWebRatio));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f + (v.w - v.y) * _SpiderWebRatio));
        verts.Add(new Vector3(v.z, v.y));
        uvs.Add(new Vector2(u.x, u.y));
        uvs.Add(new Vector2(u.x, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.z, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.z, u.y));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        verts.Add(new Vector3(v.x, (v.y + v.w) * 0.5f + (v.w - v.y) * _SpiderWebRatio));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, (v.y + v.w) * 0.5f + (v.w - v.y) * _SpiderWebRatio));
        uvs.Add(new Vector3(u.x, (u.y + u.w) * 0.5f));
        uvs.Add(new Vector3(u.x, u.w));
        uvs.Add(new Vector2(u.z, u.w));
        uvs.Add(new Vector2(u.z, (u.y + u.w) * 0.5f));
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
        cols.Add(gc);
    }

    protected void HitFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        Vector4 v = drawingDimensions;
        Vector4 u = drawingUVs;
        Color gc = drawingColor;

        float vx = v.x;
        float vy = v.y;
        float vz = v.z;
        float vw = v.w;
        float ux = u.x;
        float uy = u.y;
        float uz = u.z;
        float uw = u.w;

        #region vertex define
        float vLBx = vx;
        float vLBy = vy;
        float vLBz = (vx + vz) * 0.5f;
        float vLBw = (vy + vw) * 0.5f;
        float uLBx = ux;
        float uLBy = uy;
        float uLBz = (ux + uz) * 0.5f;
        float uLBw = (uy + uw) * 0.5f;

        float vRBx = (vx + vz) * 0.5f;
        float vRBy = vy;
        float vRBz = vz;
        float vRBw = (vy + vw) * 0.5f;

        float uRBx = (ux + uz) * 0.5f;
        float uRBy = uy;
        float uRBz = uz;
        float uRBw = (uy + uw) * 0.5f;

        float vLTx = vx;
        float vLTy = (vy + vw) * 0.5f;
        float vLTz = (vx + vz) * 0.5f;
        float vLTw = vw;

        float uLTx = ux;
        float uLTy = (uy + uw) * 0.5f;
        float uLTz = (ux + uz) * 0.5f;
        float uLTw = uw;

        float vRTx = (vx + vz) * 0.5f;
        float vRTy = (vy + vw) * 0.5f;
        float vRTz = vz;
        float vRTw = vw;

        float uRTx = (ux + uz) * 0.5f;
        float uRTy = (uy + uw) * 0.5f;
        float uRTz = uz;
        float uRTw = uw;

        Vector3 destVertex = Vector3.zero;
        switch (type)
        {
            case Type.HitRB:
                destVertex = new Vector3(vRBz, vRBy);
                break;

            case Type.HitLB:
                destVertex = new Vector3(vLBx, vLBy);
                break;

            case Type.HitLT:
                destVertex = new Vector3(vLTx, vLTw);
                break;

            case Type.HitRT:
                destVertex = new Vector3(vRTz, vRTw);
                break;
        }
        Vector3 moveVertex = Vector3.Lerp(new Vector3(vLBz, vLBw), destVertex, _DistortionRatio * 0.8f);
        #endregion

        #region Left Bottom
        if (type == Type.HitLB)
        {
            verts.Add(new Vector3(vLBx, vLBy));
            verts.Add(new Vector3(vLBx, vLBw));
            verts.Add(moveVertex);
            verts.Add(moveVertex);
            uvs.Add(new Vector2(uLBx, uLBy));
            uvs.Add(new Vector2(uLBx, uLBw));
            uvs.Add(new Vector2(uLBz, uLBw));
            uvs.Add(new Vector2(uLBz, uLBw));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);

            verts.Add(new Vector3(vLBx, vLBy));
            verts.Add(moveVertex);
            verts.Add(new Vector3(vLBz, vLBy));
            verts.Add(new Vector3(vLBz, vLBy));
            uvs.Add(new Vector2(uLBx, uLBy));
            uvs.Add(new Vector2(uLBz, uLBw));
            uvs.Add(new Vector2(uLBz, uLBy));
            uvs.Add(new Vector2(uLBz, uLBy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        else
        {
            verts.Add(new Vector3(vLBx, vLBy));
            verts.Add(new Vector3(vLBx, vLBw));
            verts.Add(moveVertex); //
            verts.Add(new Vector3(vLBz, vLBy));
            uvs.Add(new Vector2(uLBx, uLBy));
            uvs.Add(new Vector2(uLBx, uLBw));
            uvs.Add(new Vector2(uLBz, uLBw));
            uvs.Add(new Vector2(uLBz, uLBy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        #endregion

        #region Right Bottom
        if (type == Type.HitRB)
        {
            verts.Add(new Vector3(vRBx, vRBy));
            verts.Add(moveVertex);
            verts.Add(new Vector3(vRBz, vRBy));
            verts.Add(new Vector3(vRBz, vRBy));
            uvs.Add(new Vector2(uRBx, uRBy));
            uvs.Add(new Vector2(uRBx, uRBw));
            uvs.Add(new Vector2(uRBz, uRBy));
            uvs.Add(new Vector2(uRBz, uRBy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);

            verts.Add(moveVertex);
            verts.Add(new Vector3(vRBz, vRBw));
            verts.Add(new Vector3(vRBz, vRBy));
            verts.Add(new Vector3(vRBz, vRBy));
            uvs.Add(new Vector2(uRBx, uRBw));
            uvs.Add(new Vector2(uRBz, uRBw));
            uvs.Add(new Vector2(uRBz, uRBy));
            uvs.Add(new Vector2(uRBz, uRBy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        else
        {
            verts.Add(new Vector3(vRBx, vRBy));
            verts.Add(moveVertex); //
            verts.Add(new Vector3(vRBz, vRBw));
            verts.Add(new Vector3(vRBz, vRBy));
            uvs.Add(new Vector2(uRBx, uRBy));
            uvs.Add(new Vector2(uRBx, uRBw));
            uvs.Add(new Vector2(uRBz, uRBw));
            uvs.Add(new Vector2(uRBz, uRBy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        #endregion

        #region Left Top
        if (type == Type.HitLT)
        {
            verts.Add(new Vector3(vLTx, vLTy));
            verts.Add(new Vector3(vLTx, vLTw));
            verts.Add(moveVertex); //
            verts.Add(moveVertex); //
            uvs.Add(new Vector2(uLTx, uLTy));
            uvs.Add(new Vector2(uLTx, uLTw));
            uvs.Add(new Vector2(uLTz, uLTy));
            uvs.Add(new Vector2(uLTz, uLTy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);

            verts.Add(new Vector3(vLTx, vLTw));
            verts.Add(new Vector3(vLTz, vLTw));
            verts.Add(moveVertex); //
            verts.Add(moveVertex); //
            uvs.Add(new Vector2(uLTx, uLTw));
            uvs.Add(new Vector2(uLTz, uLTw));
            uvs.Add(new Vector2(uLTz, uLTy));
            uvs.Add(new Vector2(uLTz, uLTy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        else
        {
            verts.Add(new Vector3(vLTx, vLTy));
            verts.Add(new Vector3(vLTx, vLTw));
            verts.Add(new Vector3(vLTz, vLTw));
            verts.Add(moveVertex); //
            uvs.Add(new Vector2(uLTx, uLTy));
            uvs.Add(new Vector2(uLTx, uLTw));
            uvs.Add(new Vector2(uLTz, uLTw));
            uvs.Add(new Vector2(uLTz, uLTy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        #endregion

        #region Right Top
        if (type == Type.HitRT)
        {
            verts.Add(moveVertex); //
            verts.Add(new Vector3(vRTx, vRTw));
            verts.Add(new Vector3(vRTz, vRTw));
            verts.Add(new Vector3(vRTz, vRTw));
            uvs.Add(new Vector2(uRTx, uRTy));
            uvs.Add(new Vector2(uRTx, uRTw));
            uvs.Add(new Vector2(uRTz, uRTw));
            uvs.Add(new Vector2(uRTz, uRTw));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);

            verts.Add(moveVertex); //
            verts.Add(new Vector3(vRTz, vRTw));
            verts.Add(new Vector3(vRTz, vRTy));
            verts.Add(new Vector3(vRTz, vRTy));
            uvs.Add(new Vector2(uRTx, uRTy));
            uvs.Add(new Vector2(uRTz, uRTw));
            uvs.Add(new Vector2(uRTz, uRTy));
            uvs.Add(new Vector2(uRTz, uRTy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        else
        {
            verts.Add(moveVertex); //
            verts.Add(new Vector3(vRTx, vRTw));
            verts.Add(new Vector3(vRTz, vRTw));
            verts.Add(new Vector3(vRTz, vRTy));
            uvs.Add(new Vector2(uRTx, uRTy));
            uvs.Add(new Vector2(uRTx, uRTw));
            uvs.Add(new Vector2(uRTz, uRTw));
            uvs.Add(new Vector2(uRTz, uRTy));
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
            cols.Add(gc);
        }
        #endregion
    }

    #endregion // Fill functions
}
