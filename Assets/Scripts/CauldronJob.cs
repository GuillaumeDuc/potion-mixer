using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using static Unity.Mathematics.math;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;
using System;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct CauldronJob : IJobFor
{
    public struct Vertex
    {
        public float3 position, normal;
        public float4 tangent;
        public float2 texCoord0;
    }

    public struct Stream0
    {
        public float3 position, normal;
        public float4 tangent;
        public float2 texCoord0;
    }

    struct TriangleUInt16
    {
        public ushort a, b, c;
        public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<Stream0> stream0;

    [NativeDisableContainerSafetyRestriction]
    NativeArray<TriangleUInt16> triangles;

    [NativeDisableParallelForRestriction]
    public NativeArray<Vector2> BottomPoints;

    [NativeDisableParallelForRestriction]
    public NativeArray<Vector2> TopPoints;

    public int Resolution { get; set; }
    public float Radius { get; set; }
    public float Height { get; set; }
    public int VertexCount => 4 * Resolution;
    public int IndexCount => 6 * Resolution;
    public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0, 1f));

    float DeltaAngle => 180.0f / Resolution;

    public void Execute(int z)
    {
        int vi = 4 * z, ti = 2 * z;

        float val = Mathf.PI / 180f;
        float start = 180 + (180.0f / Resolution) * z;

        float3 firstPoint = float3(Radius * Mathf.Cos(start * val), Radius * Mathf.Sin(start * val) + 1, 0);
        float3 secondPoint = float3(Radius * Mathf.Cos((start + DeltaAngle) * val), Radius * Mathf.Sin((start + DeltaAngle) * val) + 1, 0);

        var vertex = new Vertex();
        vertex.normal.y = -1f;
        vertex.tangent.xw = float2(1f, -1f);
        vertex.position = float3(firstPoint.x, Mathf.Max(Height, firstPoint.y), 0);
        SetVertex(vi + 0, vertex);
        TopPoints[ti] = float2(vertex.position.x, vertex.position.y);

        vertex.texCoord0 = float2(1f, 0f);
        vertex.position = firstPoint;
        SetVertex(vi + 1, vertex);
        BottomPoints[ti] = float2(vertex.position.x, vertex.position.y);

        vertex.position = float3(secondPoint.x, Mathf.Max(Height, secondPoint.y), 0);
        vertex.texCoord0 = float2(0f, 1f);
        SetVertex(vi + 2, vertex);
        TopPoints[ti + 1] = float2(vertex.position.x, vertex.position.y);

        vertex.position = secondPoint;
        vertex.texCoord0 = 1f;
        SetVertex(vi + 3, vertex);
        BottomPoints[ti + 1] = float2(vertex.position.x, vertex.position.y);

        SetTriangle(ti + 0, vi + int3(0, 2, 1));
        SetTriangle(ti + 1, vi + int3(1, 2, 3));
    }

    public void Setup(Mesh.MeshData meshData)
    {
        var descriptor = new NativeArray<VertexAttributeDescriptor>(
            4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
        descriptor[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3
        );
        descriptor[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, dimension: 4
        );
        descriptor[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, dimension: 2
        );
        meshData.SetVertexBufferParams(VertexCount, descriptor);
        descriptor.Dispose();

        meshData.SetIndexBufferParams(IndexCount, IndexFormat.UInt16);

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(
            0, new SubMeshDescriptor(0, IndexCount)
            {
                bounds = Bounds,
                vertexCount = VertexCount
            },
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontValidateIndices
        );

        stream0 = meshData.GetVertexData<Stream0>();
        triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
    }

    public void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0()
    {
        position = vertex.position,
        normal = vertex.normal,
        tangent = vertex.tangent,
        texCoord0 = vertex.texCoord0
    };
    public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
}
