using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : SimpleMovement2D
{
    internal bool isActive = true;
    internal bool frozen = false;

    //Stats
    internal const float baseSpeed = 12;
    internal int piercing = 0;

    private const float bounceCooldownDuration = 0.15f;
    private bool bounceOnCooldown = false;

    private const float maxSpin = 120;
    private float spinSpeed = 0;

    private Vector2 unscaledVelocity;
    private float velocityScale = 1;

    private Vector2 storedVelocity;
    private float storedSpinSpeed;

    private PaddleController vaus;
    [SerializeField] private Transform spinTrailParent;

    private float priorityBounceGravity = 2f;
    private float maxTimeWithoutPriorityBounce = 2f;
    private float lastPriorityBounceTime;


    protected override void Awake()
    {
        base.Awake();
        OnCollideEnter += CollisionEnter;
    }

    protected override void Start()
    {
        base.Start();

        vaus = GameManager.instance.vaus;
    }

    protected override void Update()
    {
        base.Update();

        if (!frozen)
        {
            if (Time.time - lastPriorityBounceTime > maxTimeWithoutPriorityBounce)
            {
                SetVelocity(new Vector2(GetVelocity().x, GetVelocity().y - priorityBounceGravity * Time.deltaTime));

                //unscaledVelocity.y -= priorityBounceGravity * Time.deltaTime;
                //Debug.Log("GRAV");
            }

            if (spinSpeed != 0)
            {
                SetVelocity(Quaternion.AngleAxis(spinSpeed * Time.deltaTime, Vector3.forward) * GetVelocity());
            }

            float rot = 1000f;

            spinTrailParent.Rotate(Vector3.forward, Mathf.Lerp(0, rot, Mathf.Abs(spinSpeed) / maxSpin) * Time.deltaTime * Mathf.Sign(spinSpeed));
            spinTrailParent.localScale = new Vector3(Mathf.Lerp(0, 1f, Mathf.Abs(spinSpeed / maxSpin)), 1, 1);

            velocity = unscaledVelocity * velocityScale;
        }
    }

    public void Freeze(bool freeze)
    {
        frozen = freeze;
        if (freeze)
        {
            storedVelocity = velocity;
            storedSpinSpeed = spinSpeed;
            spinSpeed = 0;
            velocity = Vector2.zero;
        }
        else
        {
            spinSpeed = storedSpinSpeed;
            velocity = storedVelocity;
        }
    }

    public void Launch(Vector2 direction)
    {
        Freeze(false);
        SetVelocity(direction.normalized * baseSpeed);
    }

    public void SetVelocityScale(float newVelocityScale)
    {
        velocityScale = newVelocityScale;
    }

    private void SetVelocity(Vector2 newVel)
    {
        unscaledVelocity = newVel;
    }

    private Vector2 GetVelocity(bool unscaled = true)
    {
        if (unscaled) return unscaledVelocity;
        else return velocity;
    }

    private void CollisionEnter(Collider2D collider, CollisionSide side)
    {
        if (collider.CompareTag("Wall"))
        {
            BounceOffSide(side);
            //VFXManager.SpawnParticleOneshot(VFXManager.instance.ballBounceVFX,(Vector2)transform.position + GetDirFromSide(side) * wallCollider.bounds.extents.x);
        }
        else if (collider.CompareTag("Player"))
        {
            lastPriorityBounceTime = Time.time;

            BounceOffSide(CollisionSide.below);

            if (side == CollisionSide.below)
            {
                float vausSpd = vaus.GetPositionXDifference();

                float spinValue = Mathf.Lerp(0, maxSpin, Mathf.Abs(vausSpd) / LevelManager.playArea.width) * Mathf.Sign(vausSpd);

                SetSpin(spinValue);
            }

            VFXManager.SpawnParticleOneshot(VFXManager.instance.ballBounceVFX,
                (Vector2)transform.position + GetDirFromSide(side) * wallCollider.bounds.extents.x);
        }
        else
        {
            BounceOffSide(side);
            BaseBlock block = collider.GetComponent<BaseBlock>();
            if (block != null)
            {
                lastPriorityBounceTime = Time.time;

                VFXManager.SpawnParticleOneshot(VFXManager.instance.blockDamagedVFX,
                    (Vector2)transform.position + GetDirFromSide(side) * wallCollider.bounds.extents.x);
                block.OnHit();
            }
            else
            {
                Debug.LogError("Bounced off unidentified object!");
                VFXManager.SpawnParticleOneshot(VFXManager.instance.ballBounceVFX, (Vector2)transform.position + GetDirFromSide(side) * wallCollider.bounds.extents.x);
            }
        }
    }

    private void BounceOffSide(CollisionSide side)
    {
        if (!bounceOnCooldown)
        {
            OnBounce();

            if (side == CollisionSide.left)
            {
                SetVelocity(new Vector2(Mathf.Abs(GetVelocity().x), GetVelocity().y));
                //velocity.x = Mathf.Abs(GetVelocity().x);
            }
            else if (side == CollisionSide.right)
            {
                SetVelocity(new Vector2(-Mathf.Abs(GetVelocity().x), GetVelocity().y));
                //velocity.x = -Mathf.Abs(velocity.x);
            }
            else if (side == CollisionSide.above)
            {
                SetVelocity(new Vector2(GetVelocity().x, -Mathf.Abs(GetVelocity().y)));
                //velocity.y = -Mathf.Abs(velocity.y);
            }
            else
            {
                SetVelocity(new Vector2(GetVelocity().x, Mathf.Abs(GetVelocity().y)));
                //velocity.y = Mathf.Abs(velocity.y);
            }
        }
    }

    private void OnBounce()
    {
        AudioManager.instance.Play("BallBounce");
        ResetSpin();
        //StartCoroutine(BounceCooldown());
    }

    private IEnumerator BounceCooldown()
    {
        bounceOnCooldown = true;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        //WaitForSeconds wait = new WaitForSeconds(bounceCooldownDuration);

        yield return wait;

        bounceOnCooldown = false;

        yield return null;
    }

    public void SetSpin(float _spinSpeed)
    {
        spinSpeed = _spinSpeed;
        //Debug.Log($"spin speed: {spinSpeed}");
    }

    public void ResetSpin()
    {
        spinSpeed = 0;
    }

}
