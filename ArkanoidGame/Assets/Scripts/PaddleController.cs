using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    private Rigidbody2D rb;

    public Dictionary<Vector2, Rigidbody2D> attachedBalls = new Dictionary<Vector2, Rigidbody2D>();

    [SerializeField] private Rigidbody2D ballPrefab;
    [SerializeField] private int startingBalls = 1;
    [SerializeField] private float ballLaunchSpeed = 10f;
    [SerializeField] private bool useRBVelocityBasedMovement = true;
    public float defaultPaddleLength = 3f;

    [SerializeField] private float currentPaddleLength = 3f;

    private bool canControl = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (canControl)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (attachedBalls.Count > 0) LaunchAttachedBalls();
            }

            if (!useRBVelocityBasedMovement)
            {
                //Move paddle to mouse position using tranform.position
                float posXMax = LevelManager.playArea.width - currentPaddleLength / 2;
                float posXMin = 0 + currentPaddleLength / 2;

                Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(Mathf.Clamp(
                    targetPos.x, posXMin, posXMax),
                    transform.position.y, transform.position.z);
            }
        }

        //Update attached balls
        foreach (KeyValuePair<Vector2, Rigidbody2D> entry in attachedBalls)
        {
            entry.Value.transform.position = transform.position + (Vector3)entry.Key;
        }

    }

    private void FixedUpdate()
    {
        if (useRBVelocityBasedMovement && canControl)
        {
            //Move paddle to mouse position using Rigidbody2D.velocity to better detect collisions
            Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 posDelta = new Vector2((targetPos.x - transform.position.x), 0);

            float posXMax = LevelManager.playArea.width - currentPaddleLength / 2;
            float posXMin = 0 + currentPaddleLength / 2;

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
        SetPaddleLength(currentPaddleLength);
    }



    public IEnumerator ResizePaddleOverTimeSequence(float targetLength, float duration, float smoothing = 1)
    {
        float timer = 0;
        float startingLength = currentPaddleLength;

        while (timer < duration)
        {
            SetPaddleLength(Mathf.Lerp(startingLength, targetLength, Mathf.Pow(timer / duration, smoothing)));

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        SetPaddleLength(targetLength);
    }

    public void SetControlsActive(bool active = true)
    {
        rb.velocity = Vector2.zero;
        canControl = active;
    }

    public void SetPaddleLength(float _length)
    {
        currentPaddleLength = _length;
        transform.localScale = new Vector2(_length, transform.localScale.y);
    }

    private void LaunchBall(Rigidbody2D ballInstance, Vector2 velocity)
    {
        ballInstance.simulated = true;
        ballInstance.velocity = velocity;
    }

    private void AttachBall(Vector2 attachmentPosition, Rigidbody2D ballInstance)
    {
        attachedBalls.Add(attachmentPosition, ballInstance);
        ballInstance.simulated = false;
        ballInstance.velocity = Vector2.zero;
    }

    public void LaunchAttachedBalls()
    {
        foreach (KeyValuePair<Vector2, Rigidbody2D> entry in attachedBalls)
        {
            Vector2 dir = entry.Key.normalized;

            LaunchBall(entry.Value, dir * ballLaunchSpeed);
        }

        attachedBalls.Clear();
    }

    public void SpawnStartingBalls()
    {
        for (int i = 0; i < startingBalls; i++)
        {
            SpawnNewBall();
        }
    }

    public Rigidbody2D SpawnNewBall()
    {
        Vector2 localSpawnPos = GetNextBallSpawnPosition();
        Rigidbody2D newBall = Instantiate(ballPrefab, (Vector2)transform.position + localSpawnPos, Quaternion.identity);
        LevelManager.entityList.Add(newBall.gameObject);
        AttachBall(localSpawnPos, newBall);
        GameManager.instance.OnBallGained();

        VFXManager.SpawnParticleOneshot(VFXManager.instance.ballSpawnExplosionVFX, newBall.transform.position);

        return newBall;
    }

    public Vector2 GetNextBallSpawnPosition()
    {
        //TODO better position selection, paddle height?
        return new Vector2(Random.Range(0, currentPaddleLength) - currentPaddleLength / 2, 1);
    }

}
