using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PotionList
{
    public static Potion lightPotion = new()
    {
        potionName = "Light Potion",
        alpha = .5f,
        glowingPower = 25,
        color = new Color(),
        enableWave = false,
        disableWave = false,
        amplitude = 0,
        speed = 0,
        period = 0,
        origin = new Vector3(),
        enableSmoke = false,
        disableSmoke = false,
        smokeColor = new Color()
    };

    public static List<Potion> GetPotions()
    {
        return new List<Potion>
        {
            lightPotion,
        };
    }

    public static Potion GetMatchingPotion(Potion potion)
    {
        return GetPotions().Max();
    }
}
