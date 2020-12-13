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

    public bool isUIActive = true;

    public bool intro = true;

    internal PaddleController vaus;

    private void Awake()
    {
        Random.InitState(Random.Range(-100000,100000));

        if (instance == null)
        {
            instance = this;
        } 
        else Destroy(gameObject);

        lm = GetComponent<LevelManager>();

        vaus = FindObjectOfType<PaddleController>();
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


        StartCoroutine(GameStartSequence());

    }

    public void GameOver(bool win)
    {
        allowEvents = false;
        if (win) StartCoroutine(GameWonSequence());
        else StartCoroutine(GameLostSequence());
    }

    private IEnumerator GameStartSequence()
    {
        vaus.SetControlsActive(false);
        float _duration;

        if(intro)
        {
            vaus.transform.position = new Vector2(LevelManager.playArea.width / 2f, 1f);
            vaus.SetPaddleLength(LevelManager.playArea.width);
            yield return new WaitForSeconds(2f);
        }

        if (intro)
        {
            UIManager.instance.TitleFadeout(3f);

        }
        else
        {
            _duration = 1f;

            StartCoroutine(Helpers.MoveObjectOverTimeSequence(vaus.transform, new Vector2(LevelManager.playArea.width / 2f, 1f), _duration, 3));
            StartCoroutine(vaus.ResizePaddleOverTimeSequence(LevelManager.playArea.width, _duration, 3f));

            yield return new WaitForSeconds(_duration);

            yield return new WaitForSeconds(0.5f);
        }

        _duration = 1f;
        StartCoroutine(vaus.ResizePaddleOverTimeSequence(vaus.defaultPaddleLength, _duration, 4));
        yield return new WaitForSeconds(_duration);

        vaus.SpawnStartingBalls();

        yield return new WaitForSeconds(0.5f);


        vaus.SetControlsActive(true);
        intro = false;

        yield return null;
    }

    private IEnumerator GameLostSequence()
    {
        yield return new WaitForSeconds(1f);

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
