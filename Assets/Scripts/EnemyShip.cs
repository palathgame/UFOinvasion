using UnityEngine;

public class EnemyShip : MonoBehaviour
{

    public int hp;
    public HeatlhBar hpBar;
    protected InGameManager gameManager;

    // Pocatecni HP (pro ukazatel zdravi bosse v HUD).
    public int MaxHp { get; private set; }

    protected virtual void Start()
    {
        MaxHp = hp;
        hpBar.setMaxHealth(hp);
        gameManager = FindAnyObjectByType<InGameManager>();

    }

    protected void checkHP(int hp) {

        if (hp <= 0) {

            DestoyShip();

        }


    }
    public void TakeDamage(int Damage)
    {


        hp -= Damage;
        HitFlash.Flash(gameObject);
        hpBar.setHealth(hp);
        checkHP(hp);


    }

    // Prida skore za zabiti vcetne nasobice za kombo.
    protected int ComboScore(int baseScore)
    {
        int m = gameManager != null ? gameManager.RegisterComboKill() : 1;
        int total = baseScore * m;
        if (total > 0)
        {
            Color c = m >= 3 ? new Color(1f, 0.55f, 0.15f) : m == 2 ? new Color(1f, 0.85f, 0.2f) : Color.white;
            FloatingScore.Spawn(transform.position, "+" + total, c, m > 1 ? 1.3f : 1f);
        }
        return total;
    }

    protected int ComboMultiplier
    {
        get { return gameManager != null ? gameManager.CurrentMultiplier : 1; }
    }

   protected virtual void DestoyShip()
    {

        Destroy(gameObject);

        // Smrt hrace se nepocita jako zabity nepritel.
        if (this is PlaneShip) return;

        try
        {
            gameManager.GetComponent<InGameManager>().plusKills();
        }
        catch {
            Debug.Log("chyba souctu");
        }



    }

}
