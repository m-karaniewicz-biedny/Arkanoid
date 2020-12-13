using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public ParticleSystem ballSpawnExplosionVFX;
    public ParticleSystem ballBounceVFX;
    public ParticleSystem blockDeathVFX;
    public ParticleSystem blockDamagedVFX;

    public static void SpawnParticleOneshot(ParticleSystem particle, Vector2 position)
    {
        ParticleSystem p = Instantiate(particle, position, Quaternion.identity);
        p.Play();
    }
}
