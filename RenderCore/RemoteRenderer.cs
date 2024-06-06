using Dalamud.Plugin;

namespace Pictomancy;
public class RemoteRenderer
{
    private List<NativePoint> currentPath;
    private Pipe<(List<NativePoint>, Style)> strokes;
    private Pipe<(NativeFan, Style)> fans;
    private Pipe<(NativeTriangle, Style)> triangles;
    private Pipe<(NativeLine, Style)> lines;
    private Pipe<(NativeQuad, Style)> quads;

    public RemoteRenderer(DalamudPluginInterface plugin)
    {
        currentPath = new();
        strokes = new(plugin, Registrar.StrokeElementTag, false);
        fans = new(plugin, Registrar.FanElementTag, false);
        triangles = new(plugin, Registrar.TriangleElementTag, false);
        lines = new(plugin, Registrar.LineElementTag, false);
        quads = new(plugin, Registrar.QuadElementTag, false);
    }

    void PathLineTo(NativePoint point)
    {
        currentPath.Add(point);
    }

    void PathStroke(bool closed, Style style)
    {
        if (closed)
        {
            currentPath.Add(currentPath.First());
        }
        Stroke(currentPath, style);
        currentPath = new();
    }

    void Stroke(List<NativePoint> points, Style style)
    {
        strokes.Writer.Write((points, style));
    }
    void DrawLine(NativeLine line, Style style)
    {
        lines.Writer.Write((line, style));
    }
    void DrawTriangle(NativeTriangle tri, Style style)
    {
        triangles.Writer.Write((tri, style));
    }
    void DrawQuad(NativeQuad quad, Style style)
    {
        quads.Writer.Write((quad, style));
    }
    void DrawFan(NativeFan fan, Style style)
    {
        fans.Writer.Write((fan, style));
    }
}
