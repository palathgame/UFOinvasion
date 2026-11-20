using System.Collections.Generic;
using UnityEngine;

// Maly bonus za kazdy skin (index = cislo prefabu Resources/Plane/PlaneN).
// Dražší skiny (4, 6 = 250, 7 = 500 minci) maji vetsi bonus.
public static class SkinBonus
{
    private struct Bonus
    {
        public int hp;       // + max HP
        public float fire;   // o kolik procent rychlejsi strelba (0.1 = 10 %)
        public int dmg;      // + poskozeni kazde strely
    }

    private static readonly Bonus[] Table =
    {
        new Bonus(),                                  // 0 (zakladni)
        new Bonus { hp = 10 },                        // 1
        new Bonus { fire = 0.05f },                   // 2
        new Bonus { dmg = 1 },                        // 3
        new Bonus { hp = 20, fire = 0.05f },          // 4
        new Bonus { fire = 0.10f },                   // 5
        new Bonus { hp = 10, dmg = 2 },               // 6
        new Bonus { hp = 20, fire = 0.10f, dmg = 2 }, // 7
    };

    private static Bonus Get(int skin)
    {
        return skin >= 0 && skin < Table.Length ? Table[skin] : new Bonus();
    }

    public static int CurrentSkin()
    {
        int skin;
        int.TryParse(PlayerPrefs.GetString("AktualSkin"), out skin);
        return skin;
    }

    public static void Apply(PlaneShip ship, Attack attack, int skin)
    {
        Bonus b = Get(skin);
        ship.hp += b.hp;
        if (attack != null)
        {
            attack.attacRate *= 1f - b.fire;
            attack.baseDmgBonus += b.dmg;
        }
    }

    public static string Describe(int skin)
    {
        Bonus b = Get(skin);
        var parts = new List<string>();
        if (b.hp > 0) parts.Add("+" + b.hp + " HP");
        if (b.fire > 0) parts.Add("+" + Mathf.RoundToInt(b.fire * 100) + "% fire rate");
        if (b.dmg > 0) parts.Add("+" + b.dmg + " DMG");
        return string.Join(", ", parts);
    }
}
