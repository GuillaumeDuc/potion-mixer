using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Potion
{
    [Header("Color Settings")]
    [Range(-1f, 1)]
    public float alpha;
    [Range(-100f, 100)]
    public float glowingPower;
    public Color color;

    [Header("Wave Settings")]
    public bool enableWave;
    public bool disableWave;
    public float amplitude;
    public float speed;
    public float period;
    public Vector3 origin;

    [Header("Smoke Settings")]
    public bool enableSmoke = false;
    public bool disableSmoke = false;
    [ColorUsage(true, true)]
    public Color smokeColor;
}
