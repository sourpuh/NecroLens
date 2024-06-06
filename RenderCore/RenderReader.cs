using Dalamud.Plugin;

namespace Pictomancy;
public class RenderReader
{
    private List<NativePoint> currentPath;
    private Pipe<(List<NativePoint>, Style)> strokes;
    private Pipe<(NativeFan, Style)> fans;
    private Pipe<(NativeTriangle, Style)> triangles;
    private Pipe<(NativeLine, Style)> lines;
    private Pipe<(NativeQuad, Style)> quads;

    public RenderReader(DalamudPluginInterface plugin)
    {
        currentPath = new();
        strokes = new(plugin, Registrar.StrokeElementTag, true);
        fans = new(plugin, Registrar.FanElementTag, true);
        triangles = new(plugin, Registrar.TriangleElementTag, true);
        lines = new(plugin, Registrar.LineElementTag, true);
        quads = new(plugin, Registrar.QuadElementTag, true);
    }

    IEnumerable<(List<NativePoint> points, Style style)> ReadStroke()
    {
        return strokes.Reader.Read();
    }
    IEnumerable<(NativeLine, Style)> ReadLine()
    {
        return lines.Reader.Read();
    }
    IEnumerable<(NativeTriangle, Style)> ReadTriangle()
    {
        return triangles.Reader.Read();
    }
    IEnumerable<(NativeQuad, Style)> ReadQuad()
    {
        return quads.Reader.Read();
    }
    IEnumerable<(NativeFan, Style)> ReadFans()
    {
        return fans.Reader.Read();
    }
}
