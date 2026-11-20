
using UnityEngine;

public class HealBuf1 : BuffScript
{

    protected new void OnEnable()
    {
        speed = Random.Range(0, 150) + speed;

     

      positiv = true;
      gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);


        this.Push(Vector2.left * speed);
        // Mazat cely prefab (koren ma vlastni collider), ne jen tenhle objekt.
        Destroy(transform.root.gameObject, 6);
        FloatingLabel.Attach(transform, "HP +100", FloatingLabel.Good);
    }

    public new void DestroyBuff()
    {
        Destroy(transform.root.gameObject);
    }

    public int getStatHeal()
    {

            return 100;

    }

}
