
using UnityEngine;

public class BuffScript : MonoBehaviour
{

    protected bool positiv;

    public int speed;


    protected void OnEnable()
    {
        speed = Random.Range(0, 150) + speed;

        if (Random.Range(0, 2) == 1)
        {

            positiv = true;
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

        }
        else
        {

            positiv = false;
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }




        this.Push(Vector2.left * speed);
        Destroy(transform.root.gameObject, 6);
        if (GetComponent<CoinScript>() == null) // prefab mince obsahuje i DMGBuff
            FloatingLabel.Attach(transform, Label(), positiv ? FloatingLabel.Good : FloatingLabel.Bad);
    }

    // Text nad power-upem, aby hrac vedel, co sbira.
    protected virtual string Label()
    {
        return positiv ? "FIRE RATE+" : "FIRE RATE-";
    }


    public float getStat()
    {

        if (positiv)
        {

            return -0.1f;
        }

        return 0.1f;

    }


    public void DestroyBuff()
    {


        Destroy(transform.root.gameObject);


    }



}
