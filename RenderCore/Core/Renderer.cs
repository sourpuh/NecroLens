using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Pictomancy.Core;

public unsafe class Renderer : IDisposable
{
    public const int MAX_FANS = 1024;
    public const int MAX_LINES = 1024;
    public const int MAX_STROKE_SEGMENTS = MAX_FANS * Stroke.MAXIMUM_ARC_SEGMENTS;
    public const int MAX_CLIP_ZONES = 256;

    public float MaxAlpha { get; set; }
    public AlphaBlendMode alphaBlendMode { get; set; }

    public RenderContext RenderContext { get; init; } = new();

    public RenderTarget? RenderTarget { get; private set; }
    public TriFill TriFill { get; init; }
    public RenderTarget? FSPRenderTarget { get; private set; }

    public FanFill FanFill { get; init; }
    public LineFill LineFill { get; init; }
    public Stroke Stroke { get; init; }
    public ClipZone ClipZone { get; init; }
    public FullScreenPass FSP { get; init; }

    public SharpDX.Matrix ViewProj { get; private set; }
    public SharpDX.Matrix Proj { get; private set; }
    public SharpDX.Matrix View { get; private set; }
    public SharpDX.Matrix CameraWorld { get; private set; }
    public SharpDX.Vector2 ViewportSize { get; private set; }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint GetEngineCoreSingletonDelegate();
    private nint _engineCoreSingleton;

    private TriFill.Data _triFillDynamicData;
    private TriFill.Data.Builder? _triFillDynamicBuilder;

    private FanFill.Data _fanFillDynamicData;
    private FanFill.Data.Builder? _fanFillDynamicBuilder;

    private LineFill.Data _lineFillDynamicData;
    private LineFill.Data.Builder? _lineFillDynamicBuilder;

    private Stroke.Data _strokeDynamicData;
    private Stroke.Data.Builder? _strokeDynamicBuilder;

    private ClipZone.Data _clipDynamicData;
    private ClipZone.Data.Builder? _clipDynamicBuilder;

    public Renderer()
    {
        MaxAlpha = 255;
        alphaBlendMode = AlphaBlendMode.Add;
        TriFill = new(RenderContext);
        _triFillDynamicData = new(RenderContext, 1024, true);
        FanFill = new(RenderContext);
        _fanFillDynamicData = new(RenderContext, MAX_FANS, true);
        LineFill = new(RenderContext);
        _lineFillDynamicData = new(RenderContext, MAX_LINES, true);
        Stroke = new(RenderContext);
        _strokeDynamicData = new(RenderContext, MAX_STROKE_SEGMENTS, true);
        ClipZone = new(RenderContext);
        _clipDynamicData = new(RenderContext, MAX_CLIP_ZONES, true);
        FSP = new(RenderContext);
        // https://github.com/goatcorp/Dalamud/blob/d52118b3ad366a61216129c80c0fa250c885abac/Dalamud/Game/Gui/GameGuiAddressResolver.cs#L69
        _engineCoreSingleton = Marshal.GetDelegateForFunctionPointer<GetEngineCoreSingletonDelegate>(Services.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4C 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??"))();
    }

    public void Dispose()
    {
        RenderTarget?.Dispose();
        _triFillDynamicBuilder?.Dispose();
        _triFillDynamicData?.Dispose();
        FSPRenderTarget?.Dispose();
        _fanFillDynamicBuilder?.Dispose();
        _fanFillDynamicData?.Dispose();
        _lineFillDynamicBuilder?.Dispose();
        _lineFillDynamicData?.Dispose();
        _strokeDynamicBuilder?.Dispose();
        _strokeDynamicData?.Dispose();
        _clipDynamicBuilder?.Dispose();
        _clipDynamicData?.Dispose();
        FanFill.Dispose();
        LineFill.Dispose();
        Stroke.Dispose();
        ClipZone.Dispose();
        FSP.Dispose();
        RenderContext.Dispose();
    }

    public void BeginFrame()
    {
        ViewProj = ReadMatrix(_engineCoreSingleton + 0x1B4);
        Proj = ReadMatrix(_engineCoreSingleton + 0x174);
        View = ViewProj * SharpDX.Matrix.Invert(Proj);
        CameraWorld = SharpDX.Matrix.Invert(View);
        ViewportSize = ReadVec2(_engineCoreSingleton + 0x1F4);

        TriFill.UpdateConstants(RenderContext, new() { ViewProj = ViewProj });
        FanFill.UpdateConstants(RenderContext, new() { ViewProj = ViewProj });
        LineFill.UpdateConstants(RenderContext, new() { ViewProj = ViewProj });
        Stroke.UpdateConstants(RenderContext, new() { ViewProj = ViewProj, RenderTargetSize = new(ViewportSize.X, ViewportSize.Y) });
        ClipZone.UpdateConstants(RenderContext, new() { RenderTargetSize = new(ViewportSize.X, ViewportSize.Y) });
        FSP.UpdateConstants(RenderContext, new() { MaxAlpha = MaxAlpha / 255f });

        if (RenderTarget == null || RenderTarget.Size != ViewportSize)
        {
            RenderTarget?.Dispose();
            RenderTarget = new(RenderContext, (int)ViewportSize.X, (int)ViewportSize.Y, alphaBlendMode);
        }
        if (FSPRenderTarget == null || FSPRenderTarget.Size != ViewportSize)
        {
            FSPRenderTarget?.Dispose();
            FSPRenderTarget = new(RenderContext, (int)ViewportSize.X, (int)ViewportSize.Y, AlphaBlendMode.None);
        }
        RenderTarget.Bind(RenderContext);
    }

    public RenderTarget EndFrame()
    {
        // Draw all shapes and and perform clipping for the main RenderTarget.
        if (_triFillDynamicBuilder != null)
        {
            _triFillDynamicBuilder.Dispose();
            _triFillDynamicBuilder = null;
            TriFill.Draw(RenderContext, _triFillDynamicData);
        }
        if (_fanFillDynamicBuilder != null)
        {
            _fanFillDynamicBuilder.Dispose();
            _fanFillDynamicBuilder = null;
            FanFill.Draw(RenderContext, _fanFillDynamicData);
        }
        if (_lineFillDynamicBuilder != null)
        {
            _lineFillDynamicBuilder.Dispose();
            _lineFillDynamicBuilder = null;
            LineFill.Draw(RenderContext, _lineFillDynamicData);
        }
        if (_strokeDynamicBuilder != null)
        {
            _strokeDynamicBuilder.Dispose();
            _strokeDynamicBuilder = null;
            Stroke.Draw(RenderContext, _strokeDynamicData);
        }
        RenderTarget.Clip(RenderContext);
        if (_clipDynamicBuilder != null)
        {
            _clipDynamicBuilder.Dispose();
            _clipDynamicBuilder = null;
            ClipZone.Draw(RenderContext, _clipDynamicData);
        }
        // Plumb the main RenderTarget to the full screen pass for alpha correction.
        FSPRenderTarget.Bind(RenderContext, RenderTarget);
        FSP.Draw(RenderContext);

        RenderContext.Execute();
        return FSPRenderTarget;
    }
    public void DrawTriangle(NativeTriangle tri, DisplayStyle style)
    {
        GetTriFills().Add(tri.a, style.strokeColor.ToVector4());
        GetTriFills().Add(tri.b, style.strokeColor.ToVector4());
        GetTriFills().Add(tri.c, style.strokeColor.ToVector4());
    }
    private TriFill.Data.Builder GetTriFills() => _triFillDynamicBuilder ??= _triFillDynamicData.Map(RenderContext);

    public void DrawFan(NativeFan fan, DisplayStyle style)
    {
        if (style.filled)
        {
            GetFanFills().Add(
                fan.center,
                fan.innerRadius,
                fan.outerRadius,
                fan.minAngle,
                fan.maxAngle,
                style.fillColor.ToVector4(),
                style.fillColor.ToVector4());
        }
    }
    private FanFill.Data.Builder GetFanFills() => _fanFillDynamicBuilder ??= _fanFillDynamicData.Map(RenderContext);

    private void DrawStrokeLine(Vector3 a, Vector3 b, DisplayStyle style)
    {
        DrawStroke([a, b], style.strokeThickness, style.strokeColor.ToVector4(), false);
    }

    public void DrawLine(NativeLine line, DisplayStyle style)
    {
        if (line.halfWidth == 0)
        {
            DrawStrokeLine(line.a, line.b, style);
        }
        else
        {
            if (style.filled)
            {
                GetLines().Add(
                line.a,
                line.b,
                line.halfWidth,
                style.fillColor.ToVector4(),
                style.fillColor.ToVector4());
            }
        }
    }
    private LineFill.Data.Builder GetLines() => _lineFillDynamicBuilder ??= _lineFillDynamicData.Map(RenderContext);

    public void DrawStroke(IEnumerable<Vector3> world, DisplayStyle style, bool closed = false)
    {
        GetStroke().Add(world.ToArray(), style.strokeThickness, style.strokeColor.ToVector4(), closed);
    }
    public void DrawStroke(Vector3[] world, float thickness, Vector4 color, bool closed = false)
    {
        GetStroke().Add(world, thickness, color, closed);
    }
    private Stroke.Data.Builder GetStroke() => _strokeDynamicBuilder ??= _strokeDynamicData.Map(RenderContext);

    public void AddClipZone(Rectangle rect)
    {
        Vector2 upperleft = new(rect.X, rect.Y);
        Vector2 size = new(rect.Width, rect.Height);
        GetClipZones().Add(upperleft, size);
    }
    private ClipZone.Data.Builder GetClipZones() => _clipDynamicBuilder ??= _clipDynamicData.Map(RenderContext);

    private unsafe SharpDX.Matrix ReadMatrix(IntPtr address)
    {
        var p = (float*)address;
        SharpDX.Matrix mtx = new();
        for (var i = 0; i < 16; i++)
            mtx[i] = *p++;
        return mtx;
    }

    private unsafe SharpDX.Vector2 ReadVec2(IntPtr address)
    {
        var p = (float*)address;
        return new(p[0], p[1]);
    }
}
