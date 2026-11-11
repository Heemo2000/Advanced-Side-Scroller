#ifndef FLAME_VORONOI_INCLUDED
#define FLAME_VORONOI_INCLUDED

// Flamethrower stream for 2D games. Use from a Shader Graph Custom Function node
// (Type: File, Name: FlameVoronoi). Per-pixel cellular noise, no textures, no compute.
// Cost: 9 cells per octave. Detail = 0 skips the 2nd octave (uniform branch, cheap).

// Sine-free hash (stable on mobile GPUs, unlike frac(sin(x)) at low precision)
float2 FV_Hash22(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

// Returns (F1, F2): distances to nearest and second-nearest animated cell point
float2 FV_Voronoi(float2 p, float anim)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    float f1 = 8.0, f2 = 8.0;

    [unroll] for (int y = -1; y <= 1; y++)
    [unroll] for (int x = -1; x <= 1; x++)
    {
        float2 g = float2(x, y);
        float2 h = FV_Hash22(ip + g);
        float2 o = 0.5 + 0.5 * sin(anim + 6.2831853 * h);   // flickering cell points
        float2 r = g + o - fp;
        float d = dot(r, r);
        if (d < f1) { f2 = f1; f1 = d; }
        else if (d < f2) { f2 = d; }
    }
    return sqrt(float2(f1, f2));
}

void FlameVoronoi_float(
    float2 UV,            // 0..1 across the quad / sprite
    float  Time,          // Time node -> Time
    float2 Direction,     // flame travel direction, e.g. (1,0) for shooting right
    float  Aspect,        // quad width / height (keeps cells round)
    float  Scale,         // cell density, ~3-6
    float  Speed,         // scroll speed, ~1.5-4
    float  Turbulence,    // wobble, ~0.1-0.5
    float  Detail,        // 0..1 second octave
    float  WidthStart,    // half-width at nozzle, ~0.15
    float  WidthEnd,      // half-width at tip, ~0.9
    float  Cutoff,        // erosion threshold, ~0.25
    float  Softness,      // edge softness, ~0.2
    float  Intensity,     // brightness, ~1.5
    float  Seed,          // per-flame random offset (particle custom data / vertex color)
    float3 ColorOuter,    // dark red
    float3 ColorMid,      // orange
    float3 ColorCore,     // yellow-white (HDR for bloom)
    out float3 Color,
    out float  Alpha,
    out float  Heat)
{
    // Flow-aligned coordinates: t = 0 at nozzle .. 1 at tip, s = -1..1 across the stream
    float2 dir  = Direction * rsqrt(max(dot(Direction, Direction), 1e-6));
    float2 perp = float2(-dir.y, dir.x);
    float2 c    = UV - 0.5;
    float  t    = saturate(dot(c, dir) + 0.5);
    float  s    = dot(c, perp) * 2.0;

    // Cone shape and fades
    float hw         = max(lerp(WidthStart, WidthEnd, t), 1e-3);
    float sideMask   = 1.0 - smoothstep(hw * 0.4, hw, abs(s));
    float lengthFade = (1.0 - smoothstep(0.55, 1.0, t)) * smoothstep(0.0, 0.06, t);
    float shape      = sideMask * lengthFade;

    // Noise space: stretched along the flow, scrolling outward
    float2 n = float2(t * Aspect * 0.6, s) * Scale;
    n += Seed * float2(13.7, 7.3);
    n.x -= Time * Speed;
    n.y += sin(n.x * 0.9 + Time * 2.3 + Seed * 7.0) * Turbulence * (0.3 + t);

    float anim = Time * 2.5;
    float v = saturate(1.0 - FV_Voronoi(n, anim).x * 1.2);

    if (Detail > 0.001)
    {
        float2 n2 = n * 2.1 + float2(-Time * Speed * 0.8, 3.1);
        float v2 = saturate(1.0 - FV_Voronoi(n2, anim * 1.3).x * 1.2);
        v = lerp(v, saturate(v * 0.7 + v2 * 0.45), Detail);
    }

    // Hot core near the nozzle, then erosion to get torn flame edges
    float coreBoost = (1.0 - t) * (1.0 - t) * sideMask * 0.5;
    float heat = v * Intensity * shape + coreBoost;

    Alpha = smoothstep(Cutoff, Cutoff + max(Softness, 1e-3), heat);
    Heat  = saturate(heat);

    float3 col = lerp(ColorOuter, ColorMid,  smoothstep(0.15, 0.55, Heat));
    Color      = lerp(col,        ColorCore, smoothstep(0.55, 1.00, Heat));
}

// Half-precision wrapper (forwards to float; hash/fract need float to be safe on mobile)
void FlameVoronoi_half(
    half2 UV, half Time, half2 Direction, half Aspect, half Scale, half Speed,
    half Turbulence, half Detail, half WidthStart, half WidthEnd, half Cutoff,
    half Softness, half Intensity, half Seed,
    half3 ColorOuter, half3 ColorMid, half3 ColorCore,
    out half3 Color, out half Alpha, out half Heat)
{
    float3 c; float a, h;
    FlameVoronoi_float(UV, Time, Direction, Aspect, Scale, Speed, Turbulence, Detail,
                       WidthStart, WidthEnd, Cutoff, Softness, Intensity, Seed,
                       ColorOuter, ColorMid, ColorCore, c, a, h);
    Color = c; Alpha = a; Heat = h;
}

#endif
