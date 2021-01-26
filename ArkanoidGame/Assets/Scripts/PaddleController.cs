using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{

    [Header("Starting stats")]
    [SerializeField] private int startingBalls = 1;
    public float defaultBasePaddleLength = 3f;

    [Header("Settings")]
    [SerializeField] private Ball ballPrefab;
    [SerializeField] private bool useRBVelocityBasedMovement = true;

    //Persistent Stats
    internal int money = 0;
    internal int shields = 0;
    internal float permaBonusBasePaddleLength = 0;
    internal int ammoLeft = 0;

    //Temporary Stats
    private float paddleLengthBonusMultiplier = 1;
    private float globalBallSpeedBonusMultiplier = 1;

    private Rigidbody2D rb;

    public Dictionary<Ball, Vector2> attachedBalls = new Dictionary<Ball, Vector2>();

    private float currentActualPaddleLength;
    private float targetPaddleLength;

    private Coroutine resizePaddleCoroutine;

    private bool canControl = false;

    private static Transform ballParent;

    internal Vector2 lastFramePosition = Vector2.zero;

    #region MonoBehaviour

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastFramePosition = transform.position;
        ballParent = new GameObject().transform;
        ballParent.name = "BallParent";

        targetPaddleLength = defaultBasePaddleLength;
        SetPaddleLength(defaultBasePaddleLength);
    }

    private void Update()
    {
        lastFramePosition = transform.position;

        if (canControl && !GameManager.instance.isPaused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (attachedBalls.Count > 0) LaunchAttachedBalls();
            }

            if (!useRBVelocityBasedMovement)
            {
                //Move paddle to mouse position using tranform.position
                float posXMax = LevelManager.playArea.width - currentActualPaddleLength / 2;
                float posXMin = 0 + currentActualPaddleLength / 2;

                Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(Mathf.Clamp(
                    targetPos.x, posXMin, posXMax),
                    transform.position.y, transform.position.z);
            }
        }

        //Update attached balls
        if (attachedBalls.Count == 1)
        {
            foreach (KeyValuePair<Ball, Vector2> entry in attachedBalls)
            {
                if (entry.Key != null)
                {
                    Vector2 targetDir = (LevelManager.playArea.position - (Vector2)transform.position).normalized;

                    entry.Key.transform.position = (Vector2)transform.position + targetDir * 1;
                }
            }
        }
        else
        {
            foreach (KeyValuePair<Ball, Vector2> entry in attachedBalls)
            {
                if (entry.Key != null)
                {
                    entry.Key.transform.position = transform.position + (Vector3)entry.Value;
                }
            }
        }


    }

    private void FixedUpdate()
    {
        if (useRBVelocityBasedMovement && canControl)
        {
            //Move paddle to mouse position using Rigidbody2D.velocity to better detect collisions
            Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 posDelta = new Vector2((targetPos.x - transform.position.x), 0);

            float posXMax = LevelManager.playArea.width - currentActualPaddleLength / 2;
            float posXMin = 0 + currentActualPaddleLength / 2;

            if (transform.position.x + posDelta.x > posXMax)
            {
                posDelta.x = posXMax - transform.position.x;
            }
            else if (transform.position.x + posDelta.x < posXMin)
            {
                posDelta.x = posXMin - transform.position.x;
            }

            rb.velocity = posDelta / Time.deltaTime;
        }
    }

    private void OnValidate()
    {
        SetPaddleLength(currentActualPaddleLength);
    }

    #endregion

    #region Paddle Length

    //Updates paddle length during gameplay
    public void UpdatePaddleLengthToTargetLength()
    {
        float newLength = Mathf.Clamp((defaultBasePaddleLength + permaBonusBasePaddleLength) * paddleLengthBonusMultiplier, 0.2f, LevelManager.playArea.width);
        ResizePaddle(newLength, 0.5f, 2);
    }

    //Use this to resize paddle length over time outside of gameplay
    public void ResizePaddle(float newLength, float duration, float smoothing = 1)
    {
        targetPaddleLength = newLength;
        if (resizePaddleCoroutine != null) StopCoroutine(resizePaddleCoroutine);
        resizePaddleCoroutine = StartCoroutine(ResizePaddleOverTimeSequence(newLength, duration, smoothing));
    }

    //Set paddle length over time (do not use)
    private IEnumerator ResizePaddleOverTimeSequence(float newLength, float duration, float smoothing = 1)
    {

        float timer = 0;
        float startingLength = GetPaddleLength(false);

        //Debug.Log($"Starting resizing paddle. {startingLength} > {newLength}");

        while (timer < duration)
        {
            SetPaddleLength(Mathf.Lerp(startingLength, newLength, Mathf.Pow(timer / duration, smoothing)));

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        //Debug.Log($"Ending resizing paddle. {startingLength} > {newLength}");

        SetPaddleLength(newLength);
    }

    //Set paddle length directly (do not use)
    private void SetPaddleLength(float _length)
    {
        currentActualPaddleLength = _length;
        transform.localScale = new Vector2(_length, transform.localScale.y);
    }

    private float GetPaddleLength(bool target = true)
    {
        if (target) return targetPaddleLength;
        else return currentActualPaddleLength;
    }

    #endregion

    #region Ball Spawning And Handling
    public void LaunchAttachedBalls()
    {
        if (attachedBalls.Count == 1)
        {
            foreach (KeyValuePair<Ball, Vector2> entry in attachedBalls)
            {
                Vector2 dir = (entry.Key.transform.position - transform.position).normalized;

                LaunchBall(entry.Key, dir);
            }
        }
        else
        {
            foreach (KeyValuePair<Ball, Vector2> entry in attachedBalls)
            {
                Vector2 dir = entry.Value.normalized;

                LaunchBall(entry.Key, dir);
            }
        }

        attachedBalls.Clear();
    }
    public void SpawnStartingBalls()
    {
        SpawnNewBalls(startingBalls, true);
    }
    public void SpawnNewBalls(int ballCount, bool levelStart = false)
    {
        if (ballCount == 0) return;

        Debug.Log($"New balls: {ballCount}");

        int allBall = ballCount + attachedBalls.Count;

        Vector2[] positions = CalculateBallPositions(allBall);

        for (int i = 0; i < ballCount; i++)
        {
            Ball newBall = Instantiate(ballPrefab, transform.position + (Vector3)positions[i], Quaternion.identity, ballParent);
            LevelManager.entityList.Add(newBall.gameObject);
            GameManager.instance.OnBallGained();
            AttachBall(newBall, positions[i]);

            if (levelStart)
            {
                VFXManager.SpawnParticleOneshot(VFXManager.instance.ballSpawnExplosionVFX, newBall.transform.position);
                AudioManager.instance.Play("BallSpawn");
            }
            else
            {
                VFXManager.SpawnParticleOneshot(VFXManager.instance.ballSpawnExplosionSmallVFX, newBall.transform.position);
                AudioManager.instance.Play("BallSpawn");
            }
        }

        Dictionary<Ball, Vector2> dict = new Dictionary<Ball, Vector2>(attachedBalls);
        int it = ballCount - 1;
        foreach (KeyValuePair<Ball, Vector2> entry in dict)
        {
            attachedBalls[entry.Key] = positions[it];

            it++;
        }
    }
    private void LaunchBall(Ball ballInstance, Vector2 direction)
    {
        ballInstance.Launch(direction);
    }
    private void AttachBall(Ball ballInstance, Vector2 attachmentPosition)
    {
        attachedBalls.Add(ballInstance, attachmentPosition);
        ballInstance.Freeze(true);
    }
    private Vector2[] CalculateBallPositions(int ballCount)
    {
        Vector2[] pos = new Vector2[ballCount];

        float cone = 120;
        float interval = cone / ballCount;

        for (int i = 0; i < ballCount; i++)
        {
            pos[i] = PointInCircle(defaultBasePaddleLength / 2f,
                90 - (cone - interval) / 2 + i * interval, new Vector2(0, -0.5f));
        }

        return pos;
    }

    //private Vector2 GetNextBallSpawnPosition()
    //{
    //    //TODO better position selection, paddle height?
    //    if (randomBallStartingPosition)
    //    {
    //        return new Vector2(Random.Range(0, currentActualPaddleLength) - currentActualPaddleLength / 2, 1);
    //    }
    //    else
    //    {
    //        return new Vector2(0, 1);
    //    }
    //}

    #endregion

    #region Power Up Handling

    private void SetMoney(int newMoney)
    {
        money = newMoney;
    }

    private void SetShields(int newShields)
    {
        shields = newShields;
    }

    public void ReceivePowerUp(PowerUpEffect effect)
    {
        if(GameManager.instance.allowEvents)
        {
            //Debug.Log(effect.name);

            //One-time effects
            SpawnNewBalls(effect.balls);
            SetMoney(money + effect.money);
            SetShields(shields + effect.shield);

            permaBonusBasePaddleLength += effect.paddleLength;
            UpdatePaddleLengthToTargetLength();

            if(effect.mainDuration != 0) StartCoroutine(LaunchTemporaryPowerUpSequence(effect));
        }
    }

    private IEnumerator LaunchTemporaryPowerUpSequence(PowerUpEffect effect)
    {
        float duration = effect.mainDuration;
        float timer = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        paddleLengthBonusMultiplier += (effect.paddleLengthMultiplier - 1);

        Debug.Log($"Started temporary effect {effect.name}.");

        while (timer < duration)
        {


            timer += Time.deltaTime;
            yield return wait;
        }

        paddleLengthBonusMultiplier -= (effect.paddleLengthMultiplier - 1);
        UpdatePaddleLengthToTargetLength();

        Debug.Log($"Ended temporary effect {effect.name}.");

        yield return null;
    }

    #endregion

    public void StopActivity(bool reset = false)
    {
        StopAllCoroutines();
    }

    public void SetControlsActive(bool active = true)
    {
        rb.velocity = Vector2.zero;
        canControl = active;
    }

    public float GetPositionXDifference()
    {
        return (transform.position.x - lastFramePosition.x) / Time.deltaTime;
    }

    public static Vector2 PointInCircle(float radius, float angleInDegrees, Vector2 origin)
    {
        Vector2 pos;
        pos.x = (float)(radius * Mathf.Cos(angleInDegrees * Mathf.PI / 180)) + origin.x;
        pos.y = (float)(radius * Mathf.Sin(angleInDegrees * Mathf.PI / 180)) + origin.y;
        return pos;
    }
}
