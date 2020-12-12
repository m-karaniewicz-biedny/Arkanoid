using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private LevelData[] levels;

    private void Awake()
    {
        levels = Resources.LoadAll<LevelData>("Levels");
        if (levels.Length <= 0)
        {
            Debug.LogError("No levels loaded");
        }
    }

    public static void LoadScene(string name)
    {
        Debug.Log("Level load requested for: " + name);
        //Application.LoadLevel(name);
        SceneManager.LoadScene(name);
    }

    public void LoadLevel(int index)
    {
        if (index >= 0 && index < levels.Length)
        {

        }
    }

    public static void QuitRequest()
    {
        Application.Quit();
    }

}
