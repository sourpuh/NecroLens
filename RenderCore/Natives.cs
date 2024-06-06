global using Registration = (string plugin, int majorVersion, int minorVersion);

global using Style = (uint type, uint strokeColor, float strokeThickness, bool filled, float fillIntensity);
global using NativePoint = System.Numerics.Vector3;
global using NativeLine = (System.Numerics.Vector3 a, System.Numerics.Vector3 b, float halfWidth);
global using NativeTriangle = (System.Numerics.Vector3 a, System.Numerics.Vector3 b, System.Numerics.Vector3 c);
global using NativeQuad = (System.Numerics.Vector3 a, System.Numerics.Vector3 b, System.Numerics.Vector3 c, System.Numerics.Vector3 d);
global using NativeFan = (System.Numerics.Vector3 center, float innerRadius, float outerRadius, float minAngle, float maxAngle);
