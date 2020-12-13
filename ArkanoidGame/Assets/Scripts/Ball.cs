using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool active = true;

    internal int piercing = 0;
    internal float baseSpeed = 5;//not implemented first change physics


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            VFXManager.SpawnParticleOneshot(VFXManager.instance.ballBounceVFX, collision.contacts[0].point);
        }

        if(collision.collider.CompareTag("Player"))
        {
            VFXManager.SpawnParticleOneshot(VFXManager.instance.ballBounceVFX, collision.contacts[0].point);
        }
    }

}
