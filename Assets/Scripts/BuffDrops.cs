using UnityEngine;

// Spolecna logika padani power-upu a minci ze zabitych nepratel.
public static class BuffDrops
{
    // Sance na minci 1:N podle nasobice komba (x1, x2, x3).
    private const int CoinOddsX1 = 45;
    private const int CoinOddsX2 = 40;
    private const int CoinOddsX3 = 30;

    public static void Roll(Vector3 pos, int multiplier,
        GameObject speed, GameObject dmg, GameObject heal, GameObject shield, GameObject coin,
        bool extraHeal)
    {
        int r = Random.Range(0, 45);
        if (r == 1) Spawn(speed, pos);
        else if (r == 16) Spawn(dmg, pos);
        else if (r == 31 || (extraHeal && r == 37)) Spawn(heal, pos);
        else if (r == 40) Spawn(shield, pos);
        else if (r == 22 && shield != null) MagnetBuff.Spawn(shield, pos);

        if (coin != null)
        {
            int odds = multiplier >= 3 ? CoinOddsX3 : multiplier == 2 ? CoinOddsX2 : CoinOddsX1;
            if (Random.Range(0, odds) == 0) Spawn(coin, pos);
        }
    }

    private static void Spawn(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return;
        GameObject buff = Object.Instantiate(prefab);
        buff.transform.position = pos;
    }
}
