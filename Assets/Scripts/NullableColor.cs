using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NullableColor
{
    [ColorUsage(true, true)]
    public Color value;
    public bool isNull;

    public NullableColor()
    {
        isNull = false;
    }

    public NullableColor(Color newValue)
    {
        value = newValue;
        isNull = false;
    }

    public void SetValue(Color newValue)
    {
        value = newValue;
    }

    public bool IsNull()
    {
        return isNull;
    }

    public static NullableColor operator +(NullableColor a, NullableColor b) => new(a.value + b.value);
    public static NullableColor operator -(NullableColor a, NullableColor b) => new(a.value - b.value);
    public static NullableColor operator /(NullableColor a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException();
        }
        return new NullableColor(a.value / b);
    }
    public static implicit operator Color(NullableColor c) => c.value;
    public static implicit operator NullableColor(Color c) => new(c);
}
