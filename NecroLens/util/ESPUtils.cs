using Dalamud.Game.ClientState.Objects.Types;
using NecroLens.Model;
using Pictomancy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;

namespace NecroLens.util;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ESPUtils
{
    public const float DefaultCircleThickness = 2f;
    public const float DefaultFilledOpacity = 0.4f;
    public const int CircleSegments = 50;
    public const float CircleSegmentFullRotation = 2 * MathF.PI / CircleSegments;

    public static float Distance2D(this Vector3 v, Vector3 v2)
    {
        return new Vector2(v.X - v2.X, v.Z - v2.Z).Length();
    }

    public static bool IsIgnoredObject(GameObject gameObject)
    {
        if (DataIds.IgnoredDataIDs.Contains(gameObject.DataId)) return true;
        if (gameObject.IsDead || gameObject is BattleNpc { CurrentHp: <= 0 }) return true;

        return false;
    }

    public static void DrawName(PctDrawList drawList, ESPObject espObject)
    {
        var name = espObject.Name();

        if (espObject.Type == ESPObject.ESPType.GoldChest && espObject.ContainingPomander != null)
        {
            name += "\n" + DungeonService.PomanderNames[espObject.ContainingPomander.Value];
        }

        drawList.AddText(espObject.GameObject.Position, espObject.RenderColor(), name);
    }

    public static void DrawPlayerDot(PctDrawList drawList, Vector3 position)
    {
        drawList.AddDot(position, 3f, Config.PlayerDotColor, 100);
    }

    public static void DrawInteractionCircle(PctDrawList drawList, ESPObject espObject, float radius)
    {
        var color = Color.White.ToUint(1 - (espObject.Distance() / (radius + 5)));
        drawList.AddCircle(espObject.GameObject.Position, radius, color);
    }

    public static void DrawConeFromCenterPoint(
        PctDrawList drawList, ESPObject espObject, float angleRadian, float radius, uint outlineColor, float thickness = DefaultCircleThickness)
    {
        var position = espObject.GameObject.Position;
        var rotation = -espObject.GameObject.Rotation;
        var coneColor = outlineColor.SetAlpha(0.1f);

        drawList.AddConeFilled(position, radius, rotation, angleRadian, coneColor);
        drawList.PathLineTo(position);
        var halfAngle = angleRadian / 2;
        drawList.PathArcTo(position, radius, rotation - halfAngle, rotation + halfAngle);
        drawList.PathStroke(outlineColor, PctDrawFlags.Closed, thickness);
    }

    public static void DrawCircleFilled(
        PctDrawList drawList, ESPObject espObject, float radius, uint circleColor,
        float thickness = DefaultCircleThickness)
    {
        var filledColor = circleColor.SetAlpha(0.15f);
        drawList.AddCircleFilled(espObject.GameObject.Position, radius, filledColor);
    }

    public static void DrawCircle(
        PctDrawList drawList, ESPObject espObject, float radius, uint color,
        float opacity = 1f, float thickness = DefaultCircleThickness)
    {
        drawList.AddCircle(espObject.GameObject.Position, radius, color.SetAlpha(opacity));
    }

    public static void DrawFacingDirectionArrow(
        PctDrawList drawList, ESPObject espObject, uint color,
        float opacity = 1f, float thickness = DefaultCircleThickness)
    {
        var points = new List<(float, float)>
        {
            (4, -40), (6f, 0), (4, 40), (3.9f, 11), (2f, 22), (2f, -22), (3.9f, -11)
        };

        foreach (var (radian, steps) in points)
            drawList.PathLineTo(CreatePointAroundObject(espObject, radian, steps));

        drawList.PathStroke(color.SetAlpha(opacity), PctDrawFlags.Closed, thickness);
    }

    private static Vector3 CreatePointAroundObject(ESPObject espObject, float radian, float steps)
    {
        var pos = espObject.GameObject.Position;
        var rotation = espObject.GameObject.Rotation;
        var partialCircleSegmentRotation = 2 * MathF.PI / 1000;
        var stepRotation = rotation - (steps * partialCircleSegmentRotation);
        var xValue = radian * MathF.Sin(stepRotation);
        var yValue = radian * MathF.Cos(stepRotation);
        var stepPos = pos with { X = pos.X + xValue, Z = pos.Z + yValue };
        return stepPos;
    }
}
