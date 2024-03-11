using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicController : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Collider2D col;
    public LayerMask groundLayer;
    public float xInput;
    public float moveSpeed = 5f;
    public float maxSlopeAngle = 45;

    public float slopeCheckDistance = 0.5f;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;

    private bool onSlope;
    private bool isGrounded;

    Vector2 groundParallel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
    }
    private void FixedUpdate()
    {
        float targetXVel = xInput * moveSpeed;

        Vector2 bottomCenter = new(col.bounds.center.x, col.bounds.min.y);
        CheckGround();
        SlopeCheckHorizontal(bottomCenter);
        SlopeCheckVertical(bottomCenter);
        SetVelocity(targetXVel);
    }
    private void SetVelocity(float targetXVel)
    {
        if (isGrounded && !onSlope) //if not on slope
        {
            rb.velocity = new(targetXVel, 0.0f);
        }
        else if (isGrounded && onSlope) //If on slope
        {
            rb.velocity = new(moveSpeed * groundParallel.x * -xInput, moveSpeed * groundParallel.y * -xInput);
        }
        else if (!isGrounded) //If in air
        {
            rb.velocity = new(targetXVel, rb.velocity.y);
        }
    }
    private void CheckGround()
    {
        isGrounded = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
    }

    private void SlopeCheckHorizontal(Vector2 bottomCenter)
    {
        RaycastHit2D hitRight = Physics2D.Raycast(bottomCenter, transform.right, slopeCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(bottomCenter, -transform.right, slopeCheckDistance, groundLayer);

        if (hitRight)
        {
            onSlope = true;
            slopeSideAngle = Vector2.Angle(hitRight.normal, Vector2.up);
        }
        else if (hitLeft)
        {
            onSlope = true;
            slopeSideAngle = Vector2.Angle(hitLeft.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0;
            onSlope = false;
        }
    }
    private void SlopeCheckVertical(Vector2 bottomCenter)
    {
        RaycastHit2D hit = Physics2D.Raycast(bottomCenter, Vector2.down, slopeCheckDistance, groundLayer);
        if (hit)
        {
            groundParallel = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            Debug.DrawRay(hit.point, hit.normal, Color.red);
            Debug.DrawRay(transform.position, groundParallel, Color.red);

            if (slopeDownAngle != lastSlopeAngle)
            {
                onSlope = true;
            }
            lastSlopeAngle = slopeDownAngle;
        }
        if (onSlope && xInput == 0)
        {
            rb.sharedMaterial.friction = 1;
        }
        else
        {
            rb.sharedMaterial.friction = 0;
        }
    }
}
