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
    Corners raycastOrigins;
    public CollisionInfo collisionInfo;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 deltaPos)
    {
        collisionInfo.Reset();
        UpdateRaycastOrigins();
        if (deltaPos.x != 0) // if not moving in x axis, no need to check hor collisions
        {
            HorizontalCollisions(ref deltaPos);
        }
        if (deltaPos.y != 0) // same, but since gravity is always applied, this will always !=0
        {
            VerticalCollisions(ref deltaPos);
        }
        Debug.DrawRay(transform.position, deltaPos * (1/Time.deltaTime)/2, Color.red);
        transform.Translate(deltaPos);
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
                deltaPos.x = (hit.distance - skinWidth) * xDir;
                rayLength = hit.distance;

                collisionInfo.left = xDir == -1;
                collisionInfo.right = xDir == 1;
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
                deltaPos.y = (hit.distance - skinWidth) * yDir;
                rayLength = hit.distance;

                collisionInfo.below = yDir == -1;
                collisionInfo.above = yDir == 1;
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
        public void Reset()
        {
            below = false;
            above = false;
            left = false;
            right = false;
        }
    }
}
