using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public enum Mode {loss}
    public Mode mode = Mode.loss;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(mode == Mode.loss && GameManager.instance.allowEvents)
        {
            GameManager.instance.GameOver(false);
        }
        //TODO slow down? speed up? invisible ball?
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,transform.localScale);
    }

}
