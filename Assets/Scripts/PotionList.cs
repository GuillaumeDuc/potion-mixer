using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PotionList
{
    public static Potion stillWaterPotion = new()
    {
        potionName = "Still Water",
        color = Color.blue,
        glowingPower = 1,
        alpha = .1f,
        enableWave = false,
        ignoreWave = false,
        amplitude = 0,
        speed = 0,
        period = 0,
        enableSmoke = false,
        ignoreSmoke = false,
    };

    public static Potion lightPotion = new()
    {
        potionName = "Light Potion",
        alpha = .5f,
        glowingPower = 25
    };

    public static Potion moonWaterPotion = new()
    {
        potionName = "Moon Water",
        color = Color.blue,
        alpha = .2f,
        glowingPower = 25,
    };

    public static Potion ghostPotion = new()
    {
        potionName = "Ghost Potion",
        alpha = .1f,
        glowingPower = 1,
        color = Color.white,
    };

    public static Potion disapearingPotion = new()
    {
        potionName = "Disappearing Potion",
        color = Color.red,
        enableSmoke = true,
    };

    public static Potion lovePotion = new()
    {
        potionName = "Love Potion",
        color = Color.magenta,
        amplitude = 0,
        speed = 0,
        period = 0,
    };

    public static Potion waterPotion = new()
    {
        potionName = "Water",
        color = Color.blue,
        glowingPower = 1,
        alpha = .1f,
    };

    public static List<Potion> GetPotions()
    {
        return new List<Potion>
        {
            stillWaterPotion,
            lightPotion,
            ghostPotion,
            disapearingPotion,
            lovePotion,
            waterPotion,
            moonWaterPotion,
        };
    }

    public static Potion GetMatchingPotion(Potion potion)
    {
        return GetPotions().OrderBy(p => p.GetMatchingPercentage(potion)).First();
    }
}
