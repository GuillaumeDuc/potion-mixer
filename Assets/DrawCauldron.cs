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

public class DrawCauldron : MonoBehaviour
{
    public EdgeCollider2D cauldronCoreCollider;

    [Header("Shape Settings")]
    [Range(1, 180)]
    public int resolution = 1;
    [Range(1, 20)]
    public float radius = 1.0f;
    [Range(0f, 20)]
    public float height = .5f;

    [Header("Color Settings")]
    [Range(0f, 1)]
    public float alpha = 1;
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
    NativeArray<Vector2> topPoints;
    NativeArray<Vector2> bottomPoints;
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
            OnDisable();
			GenerateMesh();
            ApplyMaterial();
            generate = false;
		}
    }

	void GenerateMesh()
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

        bottomPoints = new NativeArray<Vector2>(resolution * 2, Allocator.Persistent);
        topPoints = new NativeArray<Vector2>(resolution * 2, Allocator.Persistent);

        CauldronJob cj = new()
        {
            Resolution = resolution,
            Radius = radius,
            Height = height,
            BottomPoints = bottomPoints,
            TopPoints = topPoints
        };
        cj.Setup(meshData);
        cj.ScheduleParallel(resolution, resolution, default).Complete();
        // Set collider trigger
        Vector2[] points = new Vector2[bottomPoints.Length + 2];
        points[0] = topPoints[0];
        for (int i = 1; i < points.Length - 1; i++)
        {
            points[i] = bottomPoints[i - 1];
        }
        points[^1] = topPoints[topPoints.Length - 3];
        GetComponent<PolygonCollider2D>().points = points.Select(p => p * .95f).ToArray();
        cauldronCoreCollider.points = points;

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
		GetComponent<MeshFilter>().mesh = mesh;
	}

    void OnDisable()
    {
        if (topPoints != null)
        {
            topPoints.Dispose();
        }
        if (bottomPoints != null)
        {
            bottomPoints.Dispose();
        }
    }

    public void ApplyMaterial()
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
