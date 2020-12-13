using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private LevelManager lm;

    private int levelProgress = 0;

    internal bool allowEvents = true;

    private int activeBalls = 0;

    private void Awake()
    {
        Random.InitState(Random.Range(-100000,100000));

        if (instance == null)
        {
            instance = this;
        } 
        else Destroy(gameObject);

        lm = GetComponent<LevelManager>();

    }

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if(allowEvents)
        {
            if (LevelManager.eliminationRequiredList.Count == 0)
            {
                GameOver(true);
            }
        }
    }

    public void OnBallGained()
    {
        activeBalls++;
    }

    public void OnBallLost()
    {
        activeBalls--;
        if (activeBalls <= 0) GameOver(false);
    }

    public void GameStart()
    {
        activeBalls = 0;
        allowEvents = true;
        lm.LoadLevel(levelProgress);
    }

    public void GameOver(bool win)
    {
        allowEvents = false;
        if (win) StartCoroutine(GameWonSequence());
        else StartCoroutine(GameLostSequence());
    }

    private IEnumerator GameLostSequence()
    {
        yield return new WaitForSeconds(3f);

        GameStart();

        yield return null;
    }
    private IEnumerator GameWonSequence()
    {
        Time.timeScale = 0.5f;

        yield return new WaitForSecondsRealtime(3f);

        Time.timeScale = 1f;

        levelProgress++;

        GameStart();

        yield return null;
    }


}
