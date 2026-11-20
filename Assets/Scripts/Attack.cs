
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject shootPref;
    public Transform firePoint;
    public float attacRate;
    public string attackName;

    private float timeBuffSpeed = 0;
    private float speedBuff = 0; 
    private float timeBuffDmg = 0; 
    protected float innerAttackRate = 0;
    private int dmgBuff = 0;

    // Trvaly bonus poskozeni (napr. ze skinu), na rozdil od dmgBuff nevyprsi.
    [HideInInspector] public int baseDmgBonus = 0;


    // Update is called once per frame
    void Update()
    {
        if (innerAttackRate > (attacRate + speedBuff)) { 
        
          GameObject shoot =  Instantiate(shootPref,firePoint.position,Quaternion.identity);

            innerAttackRate = 0;

            if (shoot.GetComponent<Shoot>()) {

                shoot.GetComponent<Shoot>().addDmg(dmgBuff + baseDmgBonus);
            }


            FindFirstObjectByType<AudioManager>().Play(attackName);


        }

        innerAttackRate += Time.deltaTime;

        
       
        if (timeBuffSpeed > 10)
        {
            speedBuff = 0;
            timeBuffSpeed = 0;

        }
        else {
            timeBuffSpeed += Time.deltaTime;

        }

        if (timeBuffDmg > 10)
        {
            dmgBuff = 0;
            timeBuffDmg = 0;

        }
        else {
            timeBuffDmg += Time.deltaTime;

        }

    }


    // Pro HUD: zaporna hodnota = rychlejsi strelba.
    public float SpeedBuff { get { return speedBuff; } }
    public float SpeedBuffTimeLeft { get { return speedBuff != 0 ? Mathf.Max(0, 10 - timeBuffSpeed) : 0; } }
    public int DmgBuff { get { return dmgBuff; } }
    public float DmgBuffTimeLeft { get { return dmgBuff != 0 ? Mathf.Max(0, 10 - timeBuffDmg) : 0; } }

    public void buffSpeed(float add) {
       
        speedBuff += add;
        if (speedBuff < -0.4) speedBuff = -0.4f;
        if (speedBuff > 0.4) speedBuff = 0.4f;
        timeBuffSpeed = 0;


    }

    public void buffDmg(int add)
    {

        dmgBuff += add;
        timeBuffDmg = 0;


    }



}
