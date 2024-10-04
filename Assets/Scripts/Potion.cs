using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Potion
{
    public string potionName;

    [Header("Color Settings")]
    public NullableFloat alpha = new();
    public NullableFloat glowingPower = new();
    public NullableColor color = new();

    [Header("Wave Settings")]
    public bool enableWave;
    public bool ignoreWave = true;
    public NullableFloat amplitude = new();
    public NullableFloat speed = new();
    public NullableFloat period = new();
    public Vector3 origin;
    bool isOriginNull = true;

    [Header("Smoke Settings")]
    public bool enableSmoke;
    public bool ignoreSmoke = true;
    public NullableColor smokeColor = new();

    public void SetPotion(Potion otherPotion)
    {
        SetPotion
        (
            otherPotion.potionName,
            otherPotion.alpha,
            otherPotion.glowingPower,
            otherPotion.color,
            otherPotion.enableWave,
            otherPotion.ignoreWave,
            otherPotion.amplitude,
            otherPotion.speed,
            otherPotion.period,
            otherPotion.origin,
            otherPotion.enableSmoke,
            otherPotion.ignoreSmoke,
            otherPotion.smokeColor
        );
    }

    public void SetPotion
    (
        string potionName,
        NullableFloat alpha,
        NullableFloat glowingPower,
        NullableColor color,
        bool enableWave,
        bool ignoreWave,
        NullableFloat amplitude,
        NullableFloat speed,
        NullableFloat period,
        Vector3 origin,
        bool enableSmoke,
        bool ignoreSmoke,
        NullableColor smokeColor
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
        this.ignoreWave = ignoreWave;
        this.enableSmoke = enableSmoke;
        this.ignoreSmoke = ignoreSmoke;
    }

    public void AddPotion(Potion otherPotion)
    {
        SetAlpha(otherPotion);
        SetGlowingPower(otherPotion);
        SetAmplitude(otherPotion);
        SetSpeed(otherPotion);
        SetPeriod(otherPotion);
        SetOrigin(otherPotion);
        color = (color + otherPotion.color) / 2;
        smokeColor = otherPotion.enableSmoke ? (smokeColor + otherPotion.smokeColor) / 2 : smokeColor;
    }

    public float GetMatchingPercentage(Potion potion)
    {
        // Instant rejection
        if ((!ignoreWave && (enableWave != potion.enableWave)) || (!ignoreSmoke && (enableSmoke != potion.enableSmoke)))
        {
            Debug.Log(potionName + " rejected / Potion has smoke " + enableSmoke + " potion has wave " + enableWave);
            return 10000;
        }

        float alphaMatch = !alpha.IsNull() ? Mathf.Abs(alpha - potion.alpha) : 0;
        float glowingMatch = !glowingPower.IsNull() ? Mathf.Abs(glowingPower - potion.glowingPower) : 0;
        Color diff = !color.IsNull() ? color - potion.color : new Color();
        float colorMatch = Mathf.Abs(diff.g) + Mathf.Abs(diff.b) + Mathf.Abs(diff.r);

        float amplitudeMatch = !amplitude.IsNull() ? Mathf.Abs(amplitude - potion.glowingPower) : 0;
        float speedMatch = !speed.IsNull() ? Mathf.Abs(speed - potion.speed) : 0;
        float periodMatch = !period.IsNull() ? Mathf.Abs(period - potion.period) : 0;
        Vector3 dist = !isOriginNull ? GetOrigin() - potion.GetOrigin() : new Vector3();
        float originMatch = dist.x + dist.y + dist.z;

        Color smokeDiff = !smokeColor.IsNull() && enableSmoke ? smokeColor - potion.smokeColor : new Color();
        float smokeMatch = Mathf.Abs(smokeDiff.g) + Mathf.Abs(smokeDiff.b) + Mathf.Abs(smokeDiff.r);


        float total = alphaMatch + glowingMatch + colorMatch + amplitudeMatch + speedMatch + periodMatch + originMatch + smokeMatch;

        Debug.Log(potionName + " matching : " + total);

        return total;
    }

    void SetAlpha(Potion otherPotion)
    {
        // if (alpha.IsNull()) { return; }
        alpha.SetValue(Mathf.Clamp(alpha + otherPotion.alpha, 0, 1));
    }

    void SetGlowingPower(Potion otherPotion)
    {
        // if (glowingPower.IsNull()) { return; }
        glowingPower.SetValue(Mathf.Max(glowingPower + otherPotion.glowingPower, 1));
    }

    void SetAmplitude(Potion otherPotion)
    {
        // if (amplitude.IsNull()) { return; }
        amplitude += otherPotion.amplitude;
    }

    void SetSpeed(Potion otherPotion)
    {
        // if (speed.IsNull()) { return; }
        speed += otherPotion.speed;
    }

    void SetPeriod(Potion otherPotion)
    {
        // if (period.IsNull()) { return; }
        period += otherPotion.period;
    }

    void SetOrigin(Potion otherPotion)
    {
        // if (origin == null) { return; }
        origin += otherPotion.origin;
    }

    public Vector3 GetOrigin()
    {
        return isOriginNull ? new Vector3() : origin;
    }
}
