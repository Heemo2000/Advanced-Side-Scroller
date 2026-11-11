#ifndef VORONOI_EFFECT_INCLUDED
#define VORONOI_EFFECT_INCLUDED

// Shader Graph Custom Function (Type: File, Name: VoronoiEffect).
// Single pass, 3x3 cell search, no textures. Mobile friendly (use Single precision).

// Sine-free hash: stable on mobile GPUs
float2 VE_Hash22(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

void VoronoiEffect_float(
    float2 UV,
    float  Scale,        // cells per UV unit, e.g. 8
    float  Time,         // pass Time node, or 0 for static
    float  Speed,        // cell motion speed, e.g. 1
    float  Jitter,       // 0 = regular grid, 1 = fully random
    out float  Distance, // F1: distance to nearest cell point (0 at center)
    out float  Edge,     // F2 - F1: 0 on cell borders, grows inside the cell
    out float2 CellId,   // random 0..1 value per cell
    out float3 CellColor // random color per cell
)
{
    float2 p  = UV * Scale;
    float2 ip = floor(p);
    float2 fp = frac(p);

    float f1 = 8.0, f2 = 8.0;
    float2 bestCell = 0;

    [unroll] for (int y = -1; y <= 1; y++)
    [unroll] for (int x = -1; x <= 1; x++)
    {
        float2 g = float2(x, y);
        float2 h = VE_Hash22(ip + g);
        float2 o = lerp(0.5, 0.5 + 0.5 * sin(Time * Speed + 6.2831853 * h), Jitter);
        float2 r = g + o - fp;
        float  d = dot(r, r);

        if (d < f1) { f2 = f1; f1 = d; bestCell = ip + g; }
        else if (d < f2) { f2 = d; }
    }

    Distance = sqrt(f1);
    Edge     = sqrt(f2) - Distance;

    CellId    = VE_Hash22(bestCell + 17.0);
    CellColor = float3(CellId, VE_Hash22(bestCell + 41.0).x);
}

void VoronoiEffect_half(
    half2 UV, half Scale, half Time, half Speed, half Jitter,
    out half Distance, out half Edge, out half2 CellId, out half3 CellColor)
{
    float d, e; float2 id; float3 col;
    VoronoiEffect_float(UV, Scale, Time, Speed, Jitter, d, e, id, col);
    Distance = d; Edge = e; CellId = id; CellColor = col;
}

#endif
