
using UnityEngine;

public class CoinScript : MonoBehaviour
{

    public int speed;


    protected void OnEnable()
    {
        speed = Random.Range(0, 150) + 200;
        this.Push(Vector2.left * speed);
        Destroy(transform.root.gameObject, 6);
    }


    public void DestroyBuff()
    {


        Destroy(transform.root.gameObject);


    }



}
