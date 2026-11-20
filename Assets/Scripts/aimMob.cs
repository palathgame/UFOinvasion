
using UnityEngine;


public class aimMob : EnemyShip
{

    public int score;
    public GameObject speedBuffs;
    public GameObject dmgBuff;
    public GameObject healBuff;
    public GameObject shieldBuff;
    public GameObject coin;


    private void FixedUpdate()
    {
        Move();
    }


    protected override void DestoyShip()
    {
        try
        {

            Score.scoreValue += ComboScore(score);

            BuffDrops.Roll(transform.position, ComboMultiplier, speedBuffs, dmgBuff, healBuff, shieldBuff, coin, true);
            FindFirstObjectByType<AudioManager>().Play("AlienExplosion");

        }
        catch
        {

        }
        base.DestoyShip();

    }

    private void Move() {

        var target = GameObject.FindGameObjectsWithTag("Player");

        if (target.Length > 1)
        {
            transform.position = Vector2.MoveTowards(transform.position, target[1].transform.position, 2 * Time.deltaTime);
        }

    }
}

