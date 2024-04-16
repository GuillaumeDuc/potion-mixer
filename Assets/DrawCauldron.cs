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

public class DrawCauldron : MonoBehaviour
{
    [Header("Shape Settings")]
    [Range(0f, 1)]
    public float alpha = 1;
    [Range(1, 180)]
    public int resolution = 1;
    [Range(1, 20)]
    public float radius = 1.0f;
    [Range(0f, 20)]
    public float height = .5f;

    [Header("Color Settings")]
    [Range(1f, 100)]
    public float glowingPower = 1.0f;
    public Color color;

    [Header("Wave Settings")]
    public bool wave;
    public float amplitude = .1f;
    public float speed = .5f;
    public float period = .5f;
    public Vector3 origin;

    Mesh mesh;
	bool generate = false;

	void Awake()
	{
		mesh = new Mesh
		{
			name = "Cauldron"
		};
		GetComponent<MeshFilter>().mesh = mesh;
    }
	void OnValidate() 
	{
		generate = true;
	} 

    void Update()
    {
		if (generate)
		{
			GenerateMesh();
            ApplyMaterial();
            generate = false;
		}
    }

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct MeshJob : IJobFor
	{
        struct Vertex
        {
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        struct Stream0
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
        NativeArray<Stream0> stream0;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;

        public int Resolution { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        float DeltaAngle => 180.0f / Resolution;
        public int VertexCount => 4 * Resolution * Resolution + 3 * Resolution;
        public int IndexCount => 6 * Resolution * Resolution + 3 * Resolution;
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0, 1f));

        public void Execute(int z)
        {
            int vi = 4 * Resolution * z + 3 * z, ti = 2 * Resolution * z + z;
            DrawRectangle(z, ref vi, ref ti);
            DrawHalfCircle(z, ref vi, ref ti);
        }

        void DrawRectangle(int z, ref int vi, ref int ti)
        {
            for (int x = 0; x < Resolution; x++, vi += 4, ti += 2)
            {
                var xCoordinates = (float2(x, x + 1) / Resolution - 0.5f) * 2 * Radius;
                var yCoordinates = float2(z, z + 1) / Resolution * Height;

                var vertex = new Vertex();
                vertex.normal.y = -1f;
                vertex.tangent.xw = float2(1f, -1f);

                vertex.position.x = xCoordinates.x;
                vertex.position.y = yCoordinates.x;
                SetVertex(vi + 0, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = float2(1f, 0f);
                SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.position.y = yCoordinates.y;
                vertex.texCoord0 = float2(0f, 1f);
                SetVertex(vi + 2, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = 1f;
                SetVertex(vi + 3, vertex);

                SetTriangle(ti + 0, vi + int3(0, 2, 1));
                SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }

        void DrawHalfCircle(int z, ref int vi, ref int ti)
        {
            float val = Mathf.PI / 180f;
            float x = 180 + (180.0f / Resolution) * z;

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);
            SetVertex(vi, vertex);

            vertex.position = float3(Radius * Mathf.Cos(x * val), Radius * Mathf.Sin(x * val), 0);
            vertex.texCoord0 = float2((vertex.position.x + Radius) / 2 * Radius, (vertex.position.y + Radius) / 2 * Radius);
            SetVertex(vi + 1, vertex);

            float xCoordinates = Radius * Mathf.Cos((x + DeltaAngle) * val);
            float yCoordinates = Radius * Mathf.Sin((x + DeltaAngle) * val);
            vertex.texCoord0 = float2((xCoordinates + Radius) / 2 * Radius, (yCoordinates + Radius) / 2 * Radius);
            vertex.position = float3(xCoordinates, yCoordinates, 0);
            SetVertex(vi + 2, vertex);

            SetTriangle(ti + 0, vi + int3(0, 2, 1));
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

        void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0()
        {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0
        };
        void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
    }

	void GenerateMesh()
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];
        MeshJob mj = new()
        {
            Resolution = resolution,
            Radius = radius,
            Height = height,
        };
        mj.Setup(meshData);
        mj.ScheduleParallel(resolution, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
		GetComponent<MeshFilter>().mesh = mesh;
	}

    void ApplyMaterial()
    {
        Material material = GetComponent<MeshRenderer>().material;
        // Shape
        material.SetFloat("_Alpha", alpha);
        // Color
        material.SetColor("_Color", color);
        material.SetFloat("_Power", glowingPower);
        // Wave
        material.SetFloat("_Wave", Convert.ToInt32(wave));
        material.SetFloat("_Height", height);
        material.SetVector("_Origin", origin);
        material.SetFloat("_Period", period);
        material.SetFloat("_Speed", speed);
        material.SetFloat("_Amplitude", amplitude);
    }
}
