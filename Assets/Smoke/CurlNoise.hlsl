inline float unity_noise_randomValue(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

inline float unity_noise_interpolate(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}

inline float unity_valueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_noise_randomValue(c0);
    float r1 = unity_noise_randomValue(c1);
    float r2 = unity_noise_randomValue(c2);
    float r3 = unity_noise_randomValue(c3);

    float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
    float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}

inline float2 computeCurl(float x, float y)
{
    float eps = 0.0001;

  //Find rate of change in X direction
    float n1 = unity_valueNoise((x + eps, y));
    float n2 = unity_valueNoise((x - eps, y));

  //Average to find approximate derivative
    float a = (n1 - n2) / (2 * eps);

  //Find rate of change in Y direction
    n1 = unity_valueNoise((x, y + eps));
    n2 = unity_valueNoise((x, y - eps));

  //Average to find approximate derivative
    float b = (n1 - n2) / (2 * eps);

  //Curl
    return (b, -a);
}

uint2 _pcg3d16(uint3 p)
{
    uint3 v = p * 1664525u + 1013904223u;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    return v.xy;
}

// Get random gradient from hash value.
float3 _gradient3d(uint hash)
{
    float3 g = float3(hash.xxx & uint3(0x80000, 0x40000, 0x20000));
    return g * float3(1.0 / 0x40000, 1.0 / 0x20000, 1.0 / 0x10000) - 1.0;
}

float3 BitangentNoise3D(float3 p)
{
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    const float4 D = float4(0.0, 0.5, 1.0, 2.0);

	// First corner
    float3 i = floor(p + dot(p, C.yyy));
    float3 x0 = p - i + dot(i, C.xxx);

	// Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

	// x0 = x0 - 0.0 + 0.0 * C.xxx;
	// x1 = x0 - i1  + 1.0 * C.xxx;
	// x2 = x0 - i2  + 2.0 * C.xxx;
	// x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    float3 x3 = x0 - D.yyy; // -1.0+3.0*C.x = -0.5 = -D.y

    i = i + 32768.5;
    uint2 hash0 = _pcg3d16((uint3) i);
    uint2 hash1 = _pcg3d16((uint3) (i + i1));
    uint2 hash2 = _pcg3d16((uint3) (i + i2));
    uint2 hash3 = _pcg3d16((uint3) (i + 1));

    float3 p00 = _gradient3d(hash0.x);
    float3 p01 = _gradient3d(hash0.y);
    float3 p10 = _gradient3d(hash1.x);
    float3 p11 = _gradient3d(hash1.y);
    float3 p20 = _gradient3d(hash2.x);
    float3 p21 = _gradient3d(hash2.y);
    float3 p30 = _gradient3d(hash3.x);
    float3 p31 = _gradient3d(hash3.y);

	// Calculate noise gradients.
    float4 m = saturate(0.5 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)));
    float4 mt = m * m;
    float4 m4 = mt * mt;

    mt = mt * m;
    float4 pdotx = float4(dot(p00, x0), dot(p10, x1), dot(p20, x2), dot(p30, x3));
    float4 temp = mt * pdotx;
    float3 gradient0 = -8.0 * (temp.x * x0 + temp.y * x1 + temp.z * x2 + temp.w * x3);
    gradient0 += m4.x * p00 + m4.y * p10 + m4.z * p20 + m4.w * p30;

    pdotx = float4(dot(p01, x0), dot(p11, x1), dot(p21, x2), dot(p31, x3));
    temp = mt * pdotx;
    float3 gradient1 = -8.0 * (temp.x * x0 + temp.y * x1 + temp.z * x2 + temp.w * x3);
    gradient1 += m4.x * p01 + m4.y * p11 + m4.z * p21 + m4.w * p31;

	// The cross products of two gradients is divergence free.
    return cross(gradient0, gradient1) * 3918.76;
}


void CurlNoise_float (
	float3 UV,
	out float3 PositionOut
) {

    PositionOut = BitangentNoise3D(UV);
    // PositionOut = computeCurl(PositionIn.x, PositionIn.y);
}