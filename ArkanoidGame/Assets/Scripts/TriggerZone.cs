using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public enum Mode {loss}
    public Mode mode = Mode.loss;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (GameManager.instance.allowEvents && mode == Mode.loss)
            {
                GameManager.instance.OnBallLost();
                Destroy(collision.gameObject, 1f);
            }
        }

        if(collision.CompareTag("PickUp"))
        {
            Destroy(collision.gameObject, 1f);
        }

        //TODO slow down? speed up? invisible ball?
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,transform.localScale);
    }

}
