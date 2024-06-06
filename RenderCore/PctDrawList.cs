using ImGuiNET;
using Pictomancy.Core;
using System.Numerics;

namespace Pictomancy;
public class PctDrawList : IDisposable
{
    private readonly List<NativePoint> path;
    private readonly ImDrawListPtr _drawList;
    private readonly Renderer _renderer;
    private readonly AutoClipZones _autoClipZones;

    public PctDrawList(ImDrawListPtr drawlist, Renderer renderer, AutoClipZones autoClipZones)
    {
        path = new();
        _drawList = drawlist;
        _renderer = renderer;
        _autoClipZones = autoClipZones;

        _renderer.BeginFrame();
    }

    public void Dispose()
    {
        _autoClipZones.Update();
        var target = _renderer.EndFrame();
        _drawList.AddImage(target.ImguiHandle, Vector2.Zero, new(target.Width, target.Height));
    }

    public void AddText(Vector3 position, uint color, string text)
    {
        if (!Services.GameGui.WorldToScreen(position, out var position2D))
        {
            return;
        }
        var textPosition = position2D - (ImGui.CalcTextSize(text) / 2f);
        _drawList.AddText(textPosition, color, text);
    }

    public void AddDot(Vector3 position, float radiusPixels, uint color, uint numSegments = 0)
    {
        if (!Services.GameGui.WorldToScreen(position, out var position2D))
        {
            return;
        }
        _drawList.AddCircleFilled(position2D, radiusPixels, color, (int)numSegments);
    }

    public void PathLineTo(NativePoint point)
    {
        path.Add(point);
    }

    public void PathArcTo(NativePoint point, float radius, float minAngle, float maxAngle, uint numSegments = 0)
    {
        float totalAngle = maxAngle - minAngle;
        if (numSegments == 0) numSegments = (uint)(totalAngle * 180);

        float angleStep = totalAngle / numSegments;

        for (int step = 0; step <= numSegments; step++)
        {
            float angle = MathF.PI / 2 + minAngle + step * angleStep;
            Vector3 offset = new(MathF.Cos(angle), 0, MathF.Sin(angle));
            path.Add(point + radius * offset);
        }
    }

    public void PathStroke(uint color, PicDrawFlags flags, float thickness = 2f)
    {
        _renderer.DrawStroke(path, new(0, color, thickness), (flags & PicDrawFlags.Closed) > 0);
        path.Clear();
    }

    public void AddTriangleFilled(NativePoint a, NativePoint b, NativePoint c, uint color)
    {
        _renderer.DrawTriangle((a, b, c), new(color, 0, 0));
    }

    public void AddLineFilled(NativePoint start, NativePoint stop, float halfWidth, uint color)
    {
        // _renderer.DrawLine((start, stop, halfWidth), new());
        Vector3 direction = stop - start;
        Vector3 perpendicular = halfWidth * Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
        AddQuadFilled(start - perpendicular, stop - perpendicular, stop + perpendicular, start + perpendicular, color);
    }

    public void AddQuadFilled(NativePoint a, NativePoint b, NativePoint c, NativePoint d, uint color)
    {
        _renderer.DrawTriangle((a, b, c), new(color, 0, 0));
        _renderer.DrawTriangle((c, b, d), new(color, 0, 0));
    }

    public void AddCircle(NativePoint origin, float radius, uint color, uint numSegments = 0, float thickness = 2)
    {
        PathArcTo(origin, radius, 0, 2 * MathF.PI);
        PathStroke(color, PicDrawFlags.Closed, thickness);
    }

    public void AddCircleFilled(NativePoint origin, float radius, uint color, uint numSegments = 0)
    {
        AddFanFilled(origin, 0, radius, 0, 2 * MathF.PI, color, numSegments);
    }

    public void AddArc(NativePoint origin, float radius, float minAngle, float maxAngle, uint color, uint numSegments = 0, float thickness = 2)
    {
        PathArcTo(origin, radius, minAngle, maxAngle);
        PathStroke(color, PicDrawFlags.None, thickness);
    }

    public void AddArcFilled(NativePoint origin, float radius, float minAngle, float maxAngle, uint color, uint numSegments = 0)
    {
        AddFanFilled(origin, 0, radius, minAngle, maxAngle, color, numSegments);
    }

    public void AddConeFilled(NativePoint origin, float radius, float rotation, float angle, uint color, uint numSegments = 0)
    {
        var halfAngle = angle / 2;
        AddFanFilled(origin, 0, radius, rotation - halfAngle, rotation + halfAngle, color, numSegments);
    }

    public void AddFanFilled(NativePoint origin, float innerRadius, float outerRadius, float minAngle, float maxAngle, uint color, uint numSegments = 0)
    {
        _renderer.DrawFan((origin, innerRadius, outerRadius, minAngle, maxAngle), new(color, 0, 0));
    }
}
