using UnityEngine;

public class PlaneShip : EnemyShip
{

    public GameObject backMenu;
    private GameObject ad;
    public GameObject shieldBuff;

    protected override void Start()
    {
        hpBar = GameObject.Find("Main Camera/HpCanvas/HpBar").GetComponent<HeatlhBar>();
        SkinBonus.Apply(this, GetComponent<Attack>(), SkinBonus.CurrentSkin());
        base.Start();
        ad = GameObject.Find("AdIns");
        
    }


    protected override void DestoyShip() { 
          
        base.DestoyShip();
        Destroy(transform.parent.gameObject);
        FindAnyObjectByType<AudioManager>().Play("PlayerExplosion"); 
        CameraShake.Shake(0.35f, 0.4f);
        Haptics.Vibrate(0f);
        if(ad != null) { ad.GetComponent<AdIns>().ShowInterstitialCapped(); }
        
        Instantiate(backMenu);       

    }

    // Power-up prefaby maji tag a collider na korenu, ale skript a Rigidbody2D
    // az na potomkovi "Buff" -> komponentu hledame v celem prefabu.
    private static T FindBuff<T>(GameObject go) where T : Component
    {
        T c = go.GetComponentInChildren<T>();
        if (c == null) c = go.GetComponentInParent<T>();
        return c;
    }

    // Smaze cely prefab power-upu (koren i potomka), aby nezustal visici collider.
    private static void RemoveBuff(GameObject go)
    {
        Destroy(go.transform.root.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject buff = collision.gameObject;

        MagnetBuff magnet = FindBuff<MagnetBuff>(buff);
        if (magnet != null)
        {
            MagnetEffect.Activate(gameObject, MagnetBuff.Duration);
            RemoveBuff(buff);
            return;
        }

        if (collision.CompareTag("SpeedBuff"))
        {
            BuffScript b = FindBuff<BuffScript>(buff);
            Attack attack = GetComponent<Attack>();
            if (b != null && attack != null) attack.buffSpeed(b.getStat());
            RemoveBuff(buff);
        }
        else if (collision.CompareTag("DmgBuff"))
        {
            DMGBuff b = FindBuff<DMGBuff>(buff);
            Attack attack = GetComponent<Attack>();
            if (b != null && attack != null) attack.buffDmg(b.getStatDMG());
            RemoveBuff(buff);
        }
        else if (collision.CompareTag("HealBuff"))
        {
            HealBuf b = FindBuff<HealBuf>(buff);
            if (b != null) Heal(b.getStatHeal());
            RemoveBuff(buff);
        }
        else if (collision.CompareTag("HealBuff1"))
        {
            HealBuf1 b = FindBuff<HealBuf1>(buff);
            if (b != null) Heal(b.getStatHeal());
            RemoveBuff(buff);
        }
        else if (collision.CompareTag("ShieldBuff"))
        {
            shieldBuff.SetActive(true);
            shieldBuff.GetComponent<Shield>().SetActiveTime();
            RemoveBuff(buff);
        }
        else if (collision.CompareTag("Coin"))
        {
            RemoveBuff(buff);

            string coins = PlayerPrefs.GetString("Coins");
            if (coins == "") { coins = "0"; }
            coins = (1 + long.Parse(coins)).ToString();
            PlayerPrefs.SetString("Coins", coins);
            if (gameManager != null) gameManager.AddCoin();
        }
    }

    private void Heal(int heal) {

        hp += heal;
        if (hp > MaxHp) hp = MaxHp; 
        hpBar.setHealth(hp);
        checkHP(hp);

    }


    public new void TakeDamage(int Damage)
    {
        if (!shieldBuff.activeSelf) { 
            base.TakeDamage(Damage);
            CameraShake.Shake(0.12f, 0.15f);
            Haptics.Vibrate();
        } 

    }

}
