#ifndef SIMPLE_NOISE_INCLUDED
#define SIMPLE_NOISE_INCLUDED

// Shader Graph Custom Function (Type: File, Name: SimpleNoise)
// Inputs: UV, Scale   Output: Out (0..1)
// 2D value noise: 4 hashes per pixel, no sin, no textures, no branches.

float SN_Hash(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

void SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float2 p  = UV * Scale;
    float2 i  = floor(p);
    float2 f  = frac(p);
    float2 u  = f * f * (3.0 - 2.0 * f);   // cubic smoothing (cheaper than quintic)

    float a = SN_Hash(i);
    float b = SN_Hash(i + float2(1.0, 0.0));
    float c = SN_Hash(i + float2(0.0, 1.0));
    float d = SN_Hash(i + float2(1.0, 1.0));

    Out = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

void SimpleNoise_half(half2 UV, half Scale, out half Out)
{
    float o;
    SimpleNoise_float(UV, Scale, o);
    Out = o;
}

#endif
