using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    public LayerMask collisionMask;

    const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D col;
    Rigidbody2D rb;
    Corners raycastOrigins;
    public CollisionInfo collisionInfo;

    [Header("Slope Settings")]
    public float maxClimbAngle = 80;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        CalculateRaySpacing();
    }
    public void Move(Vector3 deltaPos)
    {
        collisionInfo.Reset();
        UpdateRaycastOrigins();
        bool hasXInput = Mathf.Abs(deltaPos.x) > 0.0001f;
        if (hasXInput && deltaPos.y < 0) // if has xinput and moving down, check for descending slope
        {
            Debug.Log("Checking Descend Slope");
            DescendSlope(ref deltaPos);
        }
        if (hasXInput) // if not moving in x axis, no need to check hor collisions
        {
            Debug.Log("Checking X Collisions");
            HorizontalCollisions(ref deltaPos);
        }
        if (deltaPos.y != 0) // same. but gravity is always acting on y axis, so this is always true
        {
            Debug.Log("Checking Y Collisions");
            VerticalCollisions(ref deltaPos);
        }
        Debug.DrawRay(transform.position, deltaPos * (1 / Time.deltaTime) / 2, Color.red);
        rb.MovePosition(rb.position + (Vector2)deltaPos);
        //transform.Translate(deltaPos);
    }

    void HorizontalCollisions(ref Vector3 deltaPos)
    {
        float xDir = Mathf.Sign(deltaPos.x);
        float rayLength = Mathf.Abs(deltaPos.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * xDir, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * xDir * rayLength, Color.red);

            if (hit)
            {
                Debug.DrawRay(hit.point, hit.normal, Color.blue);
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // only bottom ray should check for slope angle
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    ClimbSlope(ref deltaPos, slopeAngle);
                }
                // only check if we are not climbing a slope or we hit a wall 
                if (!collisionInfo.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    //deltaPos.x = (hit.distance - skinWidth) * xDir;
                    //rayLength = hit.distance;

                    // if we are climbing slope, deltaX may be less than the distance to the collision point
                    // if we are not on slope, deltaX>=distance to collision point
                    // thus we will set deltaX to the minimum of both
                    deltaPos.x = Mathf.Min(Mathf.Abs(deltaPos.x), (hit.distance - skinWidth)) * xDir;
                    rayLength = Mathf.Min(Mathf.Abs(deltaPos.x) + skinWidth, hit.distance);

                    // edge case: if we are climbing a slope and hit a floating wall, we need to adjust deltaPos.y using slope angle and distance from collision point
                    if (collisionInfo.climbingSlope)
                    {
                        deltaPos.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaPos.x);
                    }

                    collisionInfo.left = xDir == -1;
                    collisionInfo.right = xDir == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector3 deltaPos)
    {
        float yDir = Mathf.Sign(deltaPos.y);
        float rayLength = Mathf.Abs(deltaPos.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (yDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + deltaPos.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * yDir, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * yDir * rayLength, Color.red);

            if (hit)
            {
                Debug.Log("Y HIT");
                deltaPos.y = (hit.distance - skinWidth) * yDir;
                rayLength = hit.distance;

                // edge case: if we are climbing a slope and hit a ceiling, we need to adjust deltaPos.x using slope angle and distance from collision point 
                if (collisionInfo.climbingSlope)
                {
                    deltaPos.x = Mathf.Abs(deltaPos.y) / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad);
                }
                collisionInfo.below = yDir == -1;
                collisionInfo.above = yDir == 1;
            }
        }
    }

    private void ClimbSlope(ref Vector3 deltaPos, float slopeAngle)
    {
        Debug.Log("Climb Slope");

        float moveDistance = Mathf.Abs(deltaPos.x);
        float climbY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        float moveX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaPos.x);
        if (deltaPos.y > climbY)
        {
            Debug.Log("Jumping");
        }
        else
        {
            deltaPos.y = climbY;
            deltaPos.x = moveX;
            collisionInfo.below = true;
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
        }
    }
    private void DescendSlope(ref Vector3 deltaPos)
    {
        float xDir = Mathf.Sign(deltaPos.x);
        // if moving left, check bottom right, vice versa
        Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > 0.001 && slopeAngle <= maxClimbAngle)
            {
                // if we are moving in the same direction as the slope
                if (Mathf.Sign(hit.normal.x) == xDir)
                {
                    // only apply descend slope if we are close enough to the slope (to avoid 'descend slope' when falling above a slope)
                    // if distance to slope < how far we are moving in y axis in this physics frame
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaPos.x))
                    {
                        float moveDistance = Mathf.Abs(deltaPos.x);
                        float descendY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        deltaPos.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaPos.x);
                        deltaPos.y -= descendY;
                        Debug.DrawRay(transform.position, descendY * Vector2.down * (1 / Time.fixedDeltaTime) / 2, Color.green);
                        collisionInfo.slopeAngle = slopeAngle;
                        collisionInfo.below = true;
                        collisionInfo.descendingSlope = true;
                    }
                }
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct Corners
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
    [Serializable]
    public struct CollisionInfo
    {
        public bool below, above, left, right;
        public float slopeAngle, slopeAngleOld;
        public bool climbingSlope, descendingSlope;
        public void Reset()
        {
            below = false;
            above = false;
            left = false;
            right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
