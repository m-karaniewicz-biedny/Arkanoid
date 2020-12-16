using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private PaddleController vaus;
    [SerializeField] private SpriteRenderer iconSR;
    [SerializeField] private SpriteRenderer backgroundSR;

    private float baseSpeed;
    private float acceleration;

    private Rigidbody2D rb;

    private Vector2 velocity;

    private PowerUpEffect effect;

    internal static PowerUpEffect[] effectPool = new PowerUpEffect[0];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        vaus = GameManager.instance.vaus;
        if (effectPool.Length == 0) effectPool = Resources.LoadAll<PowerUpEffect>("PowerUpEffects");

        effect = effectPool[Random.Range(0, effectPool.Length)];

        baseSpeed = effect.baseSpeed;
        acceleration = effect.acceleration;
        backgroundSR.color = effect.backgroundColor;
        iconSR.color = effect.iconColor;
        if (effect.sprite != null) iconSR.sprite = effect.sprite;

        rb.velocity = new Vector2(0, -baseSpeed);
    }

    private void FixedUpdate()
    {
        rb.velocity += Vector2.down * acceleration * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            VFXManager.SpawnParticleOneshot(VFXManager.instance.powerUpCollectVFX, transform.position, effect.backgroundColor);
            vaus.ReceivePowerUp(effect);
            DestroyPowerUp();
        }
    }

    private void DestroyPowerUp()
    {
        Destroy(gameObject);
    }

}
