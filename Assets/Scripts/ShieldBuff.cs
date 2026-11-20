using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBuff : BuffScript
{
    new void OnEnable()
    {
        speed = Random.Range(0, 150) + speed;

        positiv = true;
        this.Push(Vector2.left * speed);
        Destroy(transform.root.gameObject, 6);
        FloatingLabel.Attach(transform, "SHIELD", FloatingLabel.ShieldColor);
    }

}



