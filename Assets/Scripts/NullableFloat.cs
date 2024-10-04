using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NullableFloat
{
    public float value;
    public bool isNull;

    public NullableFloat()
    {
        isNull = false;
    }

    public NullableFloat(float newValue)
    {
        value = newValue;
        isNull = false;
    }

    public float GetValue() {

        return isNull ? 0 : value;
    }

    public float? GetNullableValue()
    {
        return isNull ? null : value;
    }

    public void SetValue(float newValue) {
        value = newValue;
    }

    public bool IsNull()
    {
        return isNull;
    }

    public static NullableFloat operator +(NullableFloat a, NullableFloat b) => new(a.GetValue() + b.GetValue());
    public static implicit operator float(NullableFloat f) => f.value;
    public static implicit operator NullableFloat(float f) => new(f);
}
