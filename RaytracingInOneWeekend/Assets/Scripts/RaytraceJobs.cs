using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace RaytracerInOneWeekend
{
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    struct AccumulateJob : IJobParallelFor
    {
        [ReadOnly] public int2 Size;
        [ReadOnly] public Camera Camera;
        [ReadOnly] public int SampleCount;
        [ReadOnly] public int TraceDepth;
        [ReadOnly] public Random Rng;
        [ReadOnly] public NativeArray<Primitive> Primitives;
        
        [ReadOnly] public NativeArray<float4> InputSamples;
        [WriteOnly] public NativeArray<float4> OutputSamples;

        bool Color(Ray r, int depth, out float3 color)
        {
            if (Primitives.Hit(r, 0.001f, float.PositiveInfinity, out HitRecord rec))
            {
                if (depth < TraceDepth && rec.Material.Scatter(r, rec, Rng, out float3 attenuation, out Ray scattered))
                {
                    if (Color(scattered, depth + 1, out float3 scatteredColor))
                    {
                        color = attenuation * scatteredColor;
                        return true;
                    }
                }
                color = default;
                return false;
            }

            float3 unitDirection = normalize(r.Direction);
            float t = 0.5f * (unitDirection.y + 1);
            color = lerp(1, float3(0.5f, 0.7f, 1), t);
            return true;
        }

        public void Execute(int index)
        {
            int2 coordinates = int2(
                index % Size.x, // column
                index / Size.x // row
            );

            float4 lastValue = InputSamples[index];

            float3 colorAcc = lastValue.xyz;
            int sampleCount = (int) lastValue.w;
            
            for (int s = 0; s < SampleCount; s++)
            {
                float2 normalizedCoordinates = (coordinates + Rng.NextFloat2()) / Size; // (u, v)
                Ray r = Camera.GetRay(normalizedCoordinates);
                if (Color(r, 0, out float3 sampleColor))
                {
                    colorAcc += sampleColor;
                    sampleCount++;
                }
            }

            OutputSamples[index] = float4(colorAcc, sampleCount);
        }
    }
    
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    struct CombineJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4> Input;
        [WriteOnly] public NativeArray<half4> Output;

        public void Execute(int index)
        {
            var realSampleCount = (int) Input[index].w;
            
            float3 finalColor;
            if (realSampleCount == 0)
                finalColor = float3(1, 0, 1);
            else
            {
                finalColor = Input[index].xyz / realSampleCount;
                finalColor = sqrt(finalColor);
            }

            Output[index] = half4(half3(finalColor), half(1));
        }
    }
}