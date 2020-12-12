using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    private Rigidbody2D rb;

    public Dictionary<Vector2, Rigidbody2D> attachedBalls = new Dictionary<Vector2, Rigidbody2D>();


    [SerializeField] private Rigidbody2D ballPrefab;
    [SerializeField] private float ballLaunchSpeed = 10f;
    [SerializeField] private float paddleLength = 3f;
    [SerializeField] private bool useRBVelocityBasedMovement = true;

    Vector2 mouseAxis;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (attachedBalls.Count > 0) LaunchAttachedBalls();
        }


        if (useRBVelocityBasedMovement)
        {
            mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        else
        {
            //Move paddle to mouse position using tranform.position
            float posXMax = LevelManager.playArea.width - paddleLength / 2;
            float posXMin = 0 + paddleLength / 2;

            Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(Mathf.Clamp(
                targetPos.x, posXMin, posXMax),
                transform.position.y, transform.position.z);
        }

        //Update attached balls
        foreach (KeyValuePair<Vector2, Rigidbody2D> entry in attachedBalls)
        {
            entry.Value.transform.position = transform.position + (Vector3)entry.Key;
        }

    }

    private void FixedUpdate()
    {
        if (useRBVelocityBasedMovement)
        {
            //Move paddle to mouse position using Rigidbody2D.velocity to better detect collisions
            Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 posDelta = new Vector2((targetPos.x - transform.position.x), 0);

            float posXMax = LevelManager.playArea.width - paddleLength / 2;
            float posXMin = 0 + paddleLength / 2;

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
        SetPaddleLength(paddleLength);
    }

    public void SetPaddleLength(float _length)
    {
        paddleLength = _length;
        transform.localScale = new Vector2(_length, transform.localScale.y);
    }

    private void LaunchBall(Rigidbody2D ballInstance, Vector2 velocity)
    {
        ballInstance.simulated = true;
        ballInstance.velocity = velocity;
        Debug.Log($"Launching with {velocity}");
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

    public Rigidbody2D SpawnNewBall()
    {
        Rigidbody2D newBall = Instantiate(ballPrefab, GetNextBallSpawnPosition(), Quaternion.identity);
        
        AttachBall(newBall.transform.position, newBall);

        return newBall;
    }

    public Vector2 GetNextBallSpawnPosition()
    {
        //TODO better position selection, paddle height?
        return new Vector2(Random.Range(0, paddleLength) - paddleLength / 2, 1);
    }

}
