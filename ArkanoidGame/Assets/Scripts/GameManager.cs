using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Camera cam;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } 
        else Destroy(gameObject);

        cam = Camera.main;
    }


    public float GetPlayAreaWidth()
    {
        return cam.orthographicSize * 2 * cam.aspect;
    }

    public void GameOver()
    {

    }
}
