using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    private static Transform particleParent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        particleParent = new GameObject().transform;
        particleParent.name = "ParticleParent";
    }

    public ParticleSystem ballSpawnExplosionVFX;
    public ParticleSystem ballSpawnExplosionSmallVFX;
    public ParticleSystem ballBounceVFX;
    public ParticleSystem blockDeathVFX;
    public ParticleSystem blockDamagedVFX;
    public ParticleSystem powerUpCollectVFX;
    public ParticleSystem ballModificationVFX;


    public static void SpawnParticleOneshot(ParticleSystem particle, Vector2 position)
    {
        ParticleSystem p = Instantiate(particle, position, Quaternion.identity, particleParent);
        var main = p.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        p.Play();
    }

    public static void SpawnParticleOneshot(ParticleSystem particle, Vector2 position, Color color)
    {
        ParticleSystem p = Instantiate(particle, position, Quaternion.identity, particleParent);
        var main = p.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.startColor = color;
        p.Play();
    }

    public static void DestroyAllParticles()
    {
        Destroy(particleParent.gameObject);
        particleParent = new GameObject().transform;
        particleParent.name = "ParticleParent";
    }
}
