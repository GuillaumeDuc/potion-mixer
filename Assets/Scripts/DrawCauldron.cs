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
using System;
using System.Linq;

using static Unity.Mathematics.math;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;

public class DrawCauldron : MonoBehaviour
{
    public EdgeCollider2D cauldronCoreCollider;
    public TMPro.TextMeshProUGUI potionNameTMP;

    [Header("Shape Settings")]
    [Range(1, 180)]
    public int resolution;
    [Range(1, 20)]
    public float radius;
    [Range(0f, 20)]
    public float height;


    [Header("Color Settings")]
    [Range(0f, 1)]
    public float alpha;
    [Range(0f, 1000)]
    public float glowingPower;
    public Color color;

    [Header("Wave Settings")]
    public bool wave;
    public float amplitude;
    public float speed;
    public float period;
    public Vector3 origin;

    [Header("Smoke Settings")]
    public bool smoke;
    [ColorUsage(true, true)]
    public Color smokeColor;
    public GameObject smokeGO;

    Mesh mesh;
    NativeArray<Vector2> topPoints;
    NativeArray<Vector2> bottomPoints;
    bool generate = false;

    private bool startTransition;
    private Potion potion = new(), currentPotion = new(), targetPotion = new();
    private float currentTransitionTime, transitionSpeed;

	void Awake()
	{
		mesh = new Mesh
		{
			name = "Cauldron"
		};
		GetComponent<MeshFilter>().mesh = mesh;
        ApplySmoke();
        SynchroPotionWithView();
        generate = true;
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
            SynchroPotionWithView();
            potionNameTMP.SetText(PotionList.GetMatchingPotion(potion).potionName);
            generate = false;
		}
        if (startTransition)
        {
            currentTransitionTime += Time.deltaTime / transitionSpeed;
            TransitionToTarget();
            SynchroViewWithPotion();
            potionNameTMP.SetText(PotionList.GetMatchingPotion(potion).potionName);
            if (currentTransitionTime >= 1)
            {
                startTransition = false;
                currentTransitionTime = 0;
            }
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
        material.SetFloat("_Alpha", Mathf.Max(potion.alpha, .01f));
        // Color
        material.SetColor("_Color", potion.color);
        material.SetFloat("_Power", Mathf.Max(potion.glowingPower, 1));
        // Wave
        material.SetFloat("_Wave", Convert.ToInt32(wave));
        material.SetFloat("_Height", height);
        material.SetVector("_Origin", potion.GetOrigin());
        material.SetFloat("_Period", potion.period);
        material.SetFloat("_Speed", potion.speed);
        material.SetFloat("_Amplitude", potion.amplitude);
    }

    public void ApplySmoke()
    {
        Material smokeMat = smokeGO.GetComponent<Renderer>().material;
        smokeMat.SetColor("_Color", smokeColor);
        smokeGO.SetActive(smoke);
    }

    public void AddIngredient(Ingredient ingredient)
    {
        Potion ingredientPotion = ingredient.potion;

        potion.enableWave = ingredientPotion.ignoreWave ? potion.enableWave : ingredientPotion.enableWave;
        potion.enableSmoke = ingredientPotion.ignoreSmoke ? potion.enableSmoke : ingredientPotion.enableSmoke;
        transitionSpeed = ingredient.disappearSpeed;

        potion.speed += ingredientPotion.speed;
        potion.period += ingredientPotion.period;

        currentPotion.SetPotion(potion);
        targetPotion.SetPotion(potion);
        targetPotion.AddPotion(ingredientPotion);

        startTransition = true;
    }

    public void TransitionToTarget()
    {
        potion.alpha = Mathf.Lerp(currentPotion.alpha, targetPotion.alpha, currentTransitionTime);
        potion.glowingPower = Mathf.Lerp(currentPotion.glowingPower, targetPotion.glowingPower, currentTransitionTime);
        potion.color = Color.Lerp(currentPotion.color, targetPotion.color, currentTransitionTime);
        potion.amplitude = Mathf.Lerp(currentPotion.amplitude, targetPotion.amplitude, currentTransitionTime);
        // speed = Mathf.Lerp(currentPotion.speed, targetPotion.speed, currentTransitionTime);
        // period = Mathf.Lerp(currentPotion.period, targetPotion.period, currentTransitionTime);
        potion.origin = Vector3.Lerp(currentPotion.GetOrigin(), targetPotion.GetOrigin(), currentTransitionTime);
        potion.smokeColor = Color.Lerp(currentPotion.smokeColor, targetPotion.smokeColor, currentTransitionTime);

        ApplyMaterial();
        ApplySmoke();
    }

    private void SynchroPotionWithView()
    {
        potion.SetPotion("", alpha, glowingPower, color, wave, false , amplitude, speed, period, origin, smoke, false, smokeColor);
    }
    private void SynchroViewWithPotion()
    {
        alpha = potion.alpha;
        glowingPower = potion.glowingPower;
        color = potion.color;
        amplitude = potion.amplitude;
        speed = potion.speed;
        period = potion.period;
        origin = potion.GetOrigin();
        smokeColor = potion.smokeColor;
        wave = potion.enableWave;
        smoke = potion.enableSmoke;
    }
}
