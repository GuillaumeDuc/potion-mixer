using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Potion : IComparable
{
    public string potionName;

    [Header("Color Settings")]
    [Range(-1f, 1)]
    public float alpha;
    [Range(0, 1000)]
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
    public bool enableSmoke;
    public bool disableSmoke;
    [ColorUsage(true, true)]
    public Color smokeColor;

    public void SetPotion(Potion potion)
    {
        SetPotion
        (
            potion.potionName,
            potion.alpha,
            potion.glowingPower,
            potion.color,
            potion.enableWave,
            potion.disableWave,
            potion.amplitude,
            potion.speed,
            potion.period,
            potion.origin,
            potion.enableSmoke,
            potion.disableSmoke,
            potion.smokeColor
        );
    }

    public void SetPotion
    (
        string potionName,
        float alpha,
        float glowingPower,
        Color color,
        bool enableWave,
        bool disableWave,
        float amplitude,
        float speed,
        float period,
        Vector3 origin,
        bool enableSmoke,
        bool disableSmoke,
        Color smokeColor
    )
    {
        this.potionName = potionName;
        this.alpha = alpha;
        this.glowingPower = glowingPower;
        this.color = color;
        this.amplitude = amplitude;
        this.speed = speed;
        this.period = period;
        this.smokeColor = smokeColor;
        this.origin = origin;
        this.enableSmoke = enableWave;
        this.disableWave = disableWave;
        this.enableSmoke = enableSmoke;
        this.disableSmoke = disableSmoke;
    }

    public void AddPotion(Potion potion)
    {
        alpha = Mathf.Max(alpha + potion.alpha, 0);
        glowingPower += potion.glowingPower;
        color += potion.color / 2;
        amplitude += potion.amplitude;
        speed += potion.speed;
        period += potion.period;
        smokeColor = potion.enableSmoke ? smokeColor + potion.smokeColor / 2 : smokeColor;
        origin += potion.origin;
    }

    public float GetMatchingPercentage(Potion potion)
    {
        return 100;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        if (obj is Potion otherPotion)
            return (int)(GetMatchingPercentage(this) - otherPotion.GetMatchingPercentage(this));
        else
            throw new ArgumentException("Object is not a Potion");
    }
}
