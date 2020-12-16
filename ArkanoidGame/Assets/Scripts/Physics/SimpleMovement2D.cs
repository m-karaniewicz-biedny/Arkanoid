using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class SimpleMovement2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected bool autoUpdate = true;
    [SerializeField] protected bool updateInFixedUpdate = true;
    [SerializeField] protected bool checkCollisions = true;
    [SerializeField] protected bool useOneWayTags = false;
    //[SerializeField] protected bool reccurentMovingPlatform = false;

    [Header("Collision Data")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private GameObject optionalWallColliderObject;

    [SerializeField] private string oneWayTagUp = "OneWayUp";
    [SerializeField] private string oneWayTagDown = "OneWayDown";
    [SerializeField] private string oneWayTagLeft = "OneWayLeft";
    [SerializeField] private string oneWayTagRight = "OneWayRight";

    internal Vector2 velocity;

    private const float skinWidth = .015f;
    private const float dstBetweenRays = .25f;

    protected BoxCollider2D wallCollider;

    private int horizontalRayCount;
    private int verticalRayCount;
    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    protected RaycastOrigins raycastOrigins;
    protected CollisionInfo collisions;
    protected CollisionInfo collisionsLastFrame;

    private Dictionary<Collider2D, CollisionSide> collidingObjects = new Dictionary<Collider2D, CollisionSide>();
    private Dictionary<Collider2D, CollisionSide> collidingObjectsLastFrame = new Dictionary<Collider2D, CollisionSide>();

    public event Action<Collider2D, CollisionSide> OnCollideStay;
    public event Action<Collider2D, CollisionSide> OnCollideEnter;
    public event Action<Collider2D, CollisionSide> OnCollideExit;

    protected virtual void Awake()
    {
        if (optionalWallColliderObject != null) wallCollider = optionalWallColliderObject.GetComponentInChildren<BoxCollider2D>();
        if (wallCollider == null) wallCollider = GetComponentInChildren<BoxCollider2D>();
        if (wallCollider == null) Debug.LogError("No collider assigned");
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();

        collisions.lastXDirection = 1;

        collisions.CopyValuesTo(ref collisionsLastFrame);
    }

    protected virtual void FixedUpdate()
    {
        if (autoUpdate && updateInFixedUpdate)
        {
            collisions.CopyValuesTo(ref collisionsLastFrame);
            Move(velocity * Time.deltaTime);
        }
    }

    protected virtual void Update()
    {
        if (autoUpdate && !updateInFixedUpdate)
        {
            collisions.CopyValuesTo(ref collisionsLastFrame);
            Move(velocity * Time.deltaTime);
        }
    }

    public void Move(Vector2 moveAmount)
    {
        UpdateRaycastOrigins();

        collidingObjectsLastFrame = new Dictionary<Collider2D, CollisionSide>(collidingObjects);
        collidingObjects.Clear();

        collisions.Reset();

        if (checkCollisions)
        {
            if (moveAmount.x == 0)
            {
                CheckCollisionsInPlace(CollisionSide.left);
                CheckCollisionsInPlace(CollisionSide.right);
            }
            else
            {
                collisions.lastXDirection = (int)Mathf.Sign(moveAmount.x);
                AdjustMovementForHorizontalCollision(ref moveAmount);
            }

            if (moveAmount.y == 0)
            {
                CheckCollisionsInPlace(CollisionSide.above);
                CheckCollisionsInPlace(CollisionSide.below);
            }
            else
            {
                AdjustMovementForVerticalCollision(ref moveAmount);
            }
        }
        else
        {
            CheckCollisionsInPlace(CollisionSide.above);
            CheckCollisionsInPlace(CollisionSide.below);
            CheckCollisionsInPlace(CollisionSide.left);
            CheckCollisionsInPlace(CollisionSide.right);
        }

        //CorrectHorizontalPlacement(ref moveAmount);

        transform.Translate(moveAmount);

        CallCollisionEvents();

    }

    private void CallCollisionEvents()
    {
        if (OnCollideStay != null || OnCollideEnter != null)
        {
            foreach (KeyValuePair<Collider2D, CollisionSide> entry in collidingObjects)
            {
                if (!collidingObjectsLastFrame.ContainsKey(entry.Key))
                {
                    OnCollideEnter?.Invoke(entry.Key, entry.Value);
                }

                OnCollideStay?.Invoke(entry.Key, entry.Value);
            }
        }

        if (OnCollideExit != null)
        {
            foreach (KeyValuePair<Collider2D, CollisionSide> entry in collidingObjectsLastFrame)
            {
                if (!collidingObjects.ContainsKey(entry.Key))
                {
                    Debug.Log("Exit");
                    OnCollideExit.Invoke(entry.Key, entry.Value);
                }
            }
        }
    }

    void RegisterRaycast(RaycastHit2D hit, CollisionSide side)
    {
        if (!collidingObjects.ContainsKey(hit.collider))
            collidingObjects.Add(hit.collider, side);
    }

    void CheckCollisionsInPlace(CollisionSide side)
    {
        RaycastHit2D[] hits;

        if (side == CollisionSide.left)
        {
            hits = Helpers.RaycastSalvo(raycastOrigins.bottomLeft, raycastOrigins.topLeft,
                Vector2.left, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);
        }
        else if (side == CollisionSide.right)
        {
            hits = Helpers.RaycastSalvo(raycastOrigins.topRight, raycastOrigins.bottomRight,
                Vector2.right, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);
        }
        else if (side == CollisionSide.above)
        {
            hits = Helpers.RaycastSalvo(raycastOrigins.bottomLeft, raycastOrigins.bottomRight,
                Vector2.down, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);
        }
        else
        {
            hits = Helpers.RaycastSalvo(raycastOrigins.bottomLeft, raycastOrigins.bottomRight,
                Vector2.down, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);
        }

        collisions.SetSide(side, false);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && useOneWayTags)
            {
                Collider2D col = hits[i].collider;

                if (side == CollisionSide.left)
                {
                    if (col.CompareTag(oneWayTagUp)) continue;
                    else if (col.CompareTag(oneWayTagDown)) continue;
                    else if (col.CompareTag(oneWayTagRight)) continue;
                }
                else if (side == CollisionSide.right)
                {
                    if (col.CompareTag(oneWayTagUp)) continue;
                    else if (col.CompareTag(oneWayTagDown)) continue;
                    else if (col.CompareTag(oneWayTagLeft)) continue;
                }
                else if (side == CollisionSide.above)
                {
                    if (col.CompareTag(oneWayTagLeft)) continue;
                    else if (col.CompareTag(oneWayTagDown)) continue;
                    else if (col.CompareTag(oneWayTagRight)) continue;
                }
                else
                {
                    if (col.CompareTag(oneWayTagUp)) continue;
                    else if (col.CompareTag(oneWayTagLeft)) continue;
                    else if (col.CompareTag(oneWayTagRight)) continue;
                }

                collisions.SetSide(side, true);

                RegisterRaycast(hits[i], side);
            }
        }
    }



    void AdjustMovementForHorizontalCollision(ref Vector2 moveAmount)
    {
        float directionX = collisions.lastXDirection;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength);

            if (hit)
            {
                if (hit.distance == 0) continue;

                if (hit.collider.CompareTag(oneWayTagUp) && useOneWayTags) continue;

                moveAmount.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;

                RegisterRaycast(hit, (directionX > 0) ? CollisionSide.right : CollisionSide.left);
            }
        }
    }

    void AdjustMovementForVerticalCollision(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength);

            if (hit)
            {
                if (hit.collider.CompareTag(oneWayTagUp) && directionY > 0 && useOneWayTags) continue;

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;

                RegisterRaycast(hit, (directionY > 0) ? CollisionSide.above : CollisionSide.below);
            }
        }
    }

    void CorrectHorizontalPlacement(ref Vector2 moveAmount)
    {
        float rayLength = skinWidth + Mathf.Abs(raycastOrigins.bottomLeft.x - raycastOrigins.bottomRight.x) / 2;

        for (int i = 1; i < horizontalRayCount - 1; i++)
        {
            Vector2 rayOrigin = raycastOrigins.bottomCenter;
            rayOrigin += moveAmount;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * rayLength, Color.red);

            if (hit && !hit.collider.CompareTag(oneWayTagUp))
            {
                moveAmount.x = moveAmount.x - Mathf.Abs(hit.distance - rayLength);
                collisions.right = true;
                Debug.Log("Right Side Horizontal Correction MoveAmout: " + (-Mathf.Abs(hit.distance - rayLength)));
                break;
            }
        }

        for (int i = 1; i < horizontalRayCount - 1; i++)
        {
            Vector2 rayOrigin = raycastOrigins.bottomCenter;
            rayOrigin += moveAmount;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.left * rayLength, Color.blue);

            if (hit && !hit.collider.CompareTag(oneWayTagUp))
            {
                moveAmount.x = moveAmount.x + Mathf.Abs(hit.distance - rayLength);
                collisions.left = true;
                Debug.Log("Left Side Horizontal Correction MoveAmout: " + Mathf.Abs(hit.distance - rayLength));
                break;
            }

        }
    }

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = wallCollider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

        raycastOrigins.bottomCenter = new Vector2((raycastOrigins.bottomLeft.x + raycastOrigins.bottomRight.x) / 2, raycastOrigins.bottomRight.y);
        raycastOrigins.topCenter = new Vector2((raycastOrigins.topLeft.x + raycastOrigins.topRight.x) / 2, raycastOrigins.topRight.y);

        raycastOrigins.rightCenter = new Vector2(raycastOrigins.topRight.x, (raycastOrigins.topRight.y + raycastOrigins.bottomRight.y) / 2);
        raycastOrigins.leftCenter = new Vector2(raycastOrigins.topLeft.x, (raycastOrigins.topLeft.y + raycastOrigins.bottomLeft.y) / 2);
    }


    private void CalculateRaySpacing()
    {
        Bounds bounds = wallCollider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    private string GetOneWayTag(CollisionSide side)
    {
        if (side == CollisionSide.right) return oneWayTagRight;
        else if (side == CollisionSide.left) return oneWayTagLeft;
        else if (side == CollisionSide.below) return oneWayTagDown;
        else return oneWayTagUp;
    }

    public static Vector2 GetDirFromSide(CollisionSide side)
    {
        if (side == CollisionSide.right) return Vector2.right;
        else if (side == CollisionSide.left) return Vector2.left;
        else if (side == CollisionSide.below) return Vector2.down;
        else return Vector2.up;
    }

    public RaycastOrigins GetRaycastOrigins()
    {
        return raycastOrigins;
    }

    public void ResetAllCollisionInfo()
    {
        collisions.Reset();
        collisionsLastFrame.Reset();
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;

        public Vector2 bottomCenter, topCenter;
        public Vector2 leftCenter, rightCenter;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public int lastXDirection;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            lastXDirection = 1;
        }

        public bool IsEqual(CollisionInfo toCompare)
        {

            return (above == toCompare.above && below == toCompare.below && left == toCompare.left
                && right == toCompare.right && lastXDirection == toCompare.lastXDirection);
        }

        public void CopyValuesTo(ref CollisionInfo copy)
        {
            copy.above = above;
            copy.below = below;
            copy.left = left;
            copy.right = right;
            copy.lastXDirection = lastXDirection;
        }

        public bool AnyCollision()
        {
            return above || below || left || right;
        }

        public bool GetSide(CollisionSide side)
        {
            if (side == CollisionSide.left) return left;
            else if (side == CollisionSide.right) return right;
            else if (side == CollisionSide.above) return above;
            else return below;
        }

        public void SetSide(CollisionSide side, bool value)
        {
            if (side == CollisionSide.left) left = value;
            else if (side == CollisionSide.right) right = value;
            else if (side == CollisionSide.above) above = value;
            else below = value;
        }

    }

    public enum CollisionSide
    {
        above,
        below,
        left,
        right,
    }

}
/*
void OldVerticalCollisions(ref Vector2 moveAmount)
{

    float directionY = Mathf.Sign(moveAmount.y);
    float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

    if (Mathf.Abs(moveAmount.y) < skinWidth)
    {
        Debug.Log("smol ray");
        rayLength = 2 * skinWidth;
    }

    for (int i = 0; i < verticalRayCount; i++)
    {

        Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
        rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

        Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength);

        if (hit)
        {
            moveAmount.y = (hit.distance - skinWidth) * directionY;
            rayLength = hit.distance;

            collisions.below = directionY == -1;
            collisions.above = directionY == 1;

            collidingObjects.Add(hit.collider.gameObject, (directionY > 0) ? CollisionSide.above : CollisionSide.below);
        }
    }
}

void CheckForRightCollisionsInPlace()
{
    //Right
    RaycastHit2D[] hits = Helpers.RaycastSalvo(raycastOrigins.topRight, raycastOrigins.bottomRight,
    Vector2.right, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);

    //collisionsLastFrame.right = collisions.right;
    collisions.right = false;

    for (int i = 0; i < hits.Length; i++)
    {

        if (hits[i].collider != null)
        {
            if (hits[i].collider.CompareTag(oneWayTagUp)) continue;


            collisions.right = true;
        }

        if (collisions.right)
        {
            collidingObjects.Add(hits[i].collider.gameObject, CollisionSide.right);
            break;
        }
    }
}

void CheckForVerticalCollisionsInPlace()
{
    //Below
    RaycastHit2D[] hits = Helpers.RaycastSalvo(raycastOrigins.bottomLeft, raycastOrigins.bottomRight,
        Vector2.down, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);

    //collisionsLastFrame.below = collisions.below;
    collisions.below = false;

    for (int i = 0; i < hits.Length; i++)
    {
        if (hits[i].collider != null)
        {
            collisions.below = true;
        }

        if (collisions.below)
        {
            collidingObjects.Add(hits[i].collider.gameObject, CollisionSide.below);
            break;
        }
    }

    //Above
    hits = Helpers.RaycastSalvo(raycastOrigins.topLeft, raycastOrigins.topRight,
    Vector2.up, skinWidth * 2, verticalRayCount, collisionMask, skinWidth);

    //collisionsLastFrame.above = collisions.above;
    collisions.above = false;

    for (int i = 0; i < hits.Length; i++)
    {


        if (hits[i].collider != null)
        {
            if (hits[i].collider.CompareTag(oneWayTagUp)) continue;

            collisions.above = true;
        }

        if (collisions.above)
        {
            collidingObjects.Add(hits[i].collider.gameObject, CollisionSide.above);
            break;
        }
    }
}

























*/