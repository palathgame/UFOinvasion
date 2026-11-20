
using UnityEngine;

public class EnemyShipMove : EnemyShip
{
    public float speed;
    public float moveTime;
    public int score;

    protected EnemyMovePoints point;
    protected EnemyPoints enemyPoints;
    protected float innerMoveTime = 0;
   
    public GameObject speedBuffs;
    public GameObject dmgBuff;
    public GameObject healBuff;
    public GameObject shieldBuff;
    public GameObject coin;


    protected override void Start()
    {
        enemyPoints = FindAnyObjectByType<EnemyPoints>();
        point = enemyPoints.GetRandomMovePoint();

        if (point == null)
        {
            Destroy(gameObject);

        }
        else { 
            point.SetIsEnemyOnPoint(true);
        
        }
        base.Start();

    }

    private void Update()
    {
        moveShip();

    }

    private void FixedUpdate()
    {
        Move();
    }


    private void Move() {

        transform.position = Vector2.MoveTowards(transform.position,point.transform.position,speed * Time.fixedDeltaTime);
    
    }

    protected override void DestoyShip()
    {
        try
        {
            point.SetIsEnemyOnPoint(false);

            Score.scoreValue += ComboScore(score);

            BuffDrops.Roll(transform.position, ComboMultiplier, speedBuffs, dmgBuff, healBuff, shieldBuff, coin, true);

            FindAnyObjectByType<AudioManager>().Play("AlienExplosion");
            base.DestoyShip();

        }
        catch {
            Debug.Log("----- chyba ---------");
           // Debug.Log(point);


                }
    }



    protected virtual void moveShip()
    {

        if (innerMoveTime >= moveTime)
        {

            point.SetIsEnemyOnPoint(false);
            point = enemyPoints.GetRandomMovePoint();
            point.SetIsEnemyOnPoint(true);
            innerMoveTime = 0;

        }

        innerMoveTime += Time.deltaTime;



    }

}
