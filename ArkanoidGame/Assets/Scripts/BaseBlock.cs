using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBlock : MonoBehaviour
{
    public int hitPoints = 1;

    protected SpriteRenderer sr;

    public Color HPColor1;
    public Color HPColor2;
    public Color HPColor3;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateDamageVisuals(hitPoints);
    }

    public virtual void OnHit()
    {
        if (hitPoints > 1)
        {
            hitPoints--;
            UpdateDamageVisuals(hitPoints);

        } 
        else OnDeath();
    }
    
    protected virtual void UpdateDamageVisuals(int hitPointsLeft)
    {
        if(hitPointsLeft==3)
        {
            sr.color = HPColor3;
        }
        else if(hitPoints==2)
        {
            sr.color = HPColor2;
        }
        else if(hitPoints==1)
        {
            sr.color = HPColor1;
        }
    }

    public virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            OnHit();
        }

    }

}
