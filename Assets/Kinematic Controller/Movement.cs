using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPhysics;

public class Movement : MonoBehaviour
{
    [Header("Jump Parameters")]
    public float jumpHeight = 4f;
    public float timeToApex = .5f;
    [Header("Movement Parameters")]
    public float moveSpeed = 5f;
    [Header("Smoothing")]
    public float smoothTimeGrounded = .05f;
    public float smoothTimeAirborne = .15f;

    // will be set based on jump height & time to apex
    float jumpVel;
    float jumpGravity;
    float jumpGravityModifier;

    float gravity = -10;

    Vector2 targetVel;
    float xInput;
    float xVelSmooth;

    bool jumped;

    PlayerPhysics phyObj;

    void Start()
    {
        phyObj = GetComponent<PlayerPhysics>();
        CalculateJumpPhysics();
    }

    private void CalculateJumpPhysics()
    {
        jumpGravity = -2 * (jumpHeight / Mathf.Pow(timeToApex, 2));
        jumpGravityModifier = jumpGravity / gravity;
        jumpVel = -(jumpGravity * timeToApex);
    }
    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (phyObj.collisionInfo.below || phyObj.collisionInfo.above) targetVel.y = 0;
        if (Input.GetKeyDown(KeyCode.Space) && phyObj.collisionInfo.below)
        {
            jumped = true;
        }
    }

    void FixedUpdate()
    {
        if (jumped)
        {
            targetVel.y = jumpVel;
            jumped = false;
        }
        float rawTargetXVel = xInput * moveSpeed;
        targetVel.x = Mathf.SmoothDamp(targetVel.x, rawTargetXVel, ref xVelSmooth, phyObj.collisionInfo.below ? smoothTimeGrounded : smoothTimeAirborne);
        targetVel.y += jumpGravity * Time.fixedDeltaTime;
        phyObj.Move(targetVel * Time.fixedDeltaTime);
    }
}
