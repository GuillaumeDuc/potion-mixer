float4 blur(UnityTexture2D MainTex, float4 UV, float Strength)
{
    float4 output = 0;
    float4 output2 = 0;
    float4 output3 = 0;
    float4 output4 = 0;
    float4 originalMain = tex2D(MainTex, UV);
    int sum = 0;

    for (int x = 0; x < Strength; x++) {
        output += tex2D(MainTex, UV + float2(-x, 0) * MainTex.texelSize.xy);
        output2 += tex2D(MainTex, UV + float2(x, 0) * MainTex.texelSize.xy);
        output3 += tex2D(MainTex, UV + float2(0, x) * MainTex.texelSize.xy);
        output4 += tex2D(MainTex, UV + float2(0, -x) * MainTex.texelSize.xy);
        sum++;
    }

    if (Strength <= 0)
    {
        return originalMain;
    }
    else
    {
        return (output + output2 + output3 + output4) / sum / 4;
    }
}

void Blur_float(UnityTexture2D mainTex, float4 UV, float Strength, out float4 O)
{
    O = blur(mainTex, UV, Strength);
}