using UnityEngine;

public class KamikazeShipMove : EnemyShip
{
    public float speed;
    public int score;


    public GameObject speedBuffs;
    public GameObject dmgBuff;
    public GameObject healBuff;
    public GameObject shieldBuff;
    public int destroyTime;


    protected override void Start()
     {

        base.Start();
        this.Push(Vector2.right * -1 * speed);

        Destroy(gameObject, destroyTime);
    }



    protected override void DestoyShip()
    {
       

        Score.scoreValue += ComboScore(score);

        BuffDrops.Roll(transform.position, ComboMultiplier, speedBuffs, dmgBuff, healBuff, shieldBuff, null, false);
        FindFirstObjectByType<AudioManager>().Play("AlienExplosion");
        base.DestoyShip();
    }



}
