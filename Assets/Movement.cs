using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPhysics;

public class Movement : MonoBehaviour
{
    [Header("Jump Parameters")]
    public float jumpHeight = 4f;
    public float timeToApex = .5f;

    // will be set based on jump height & time to apex
    float jumpVel;
    float jumpGravity;
    float jumpGravityModifier;

    float moveSpeed = 6;
    float gravity = -10;

    Vector2 targetVel;

    float xVelSmooth;
    [Header("Smoothing")]
    public float smoothTimeGrounded = .05f;
    public float smoothTimeAirborne = .15f;

    PlayerPhysics phyObj;

    void Start()
    {
        phyObj = GetComponent<PlayerPhysics>();

        jumpGravity = -2 * (jumpHeight / Mathf.Pow(timeToApex, 2));
        jumpGravityModifier = jumpGravity / gravity;
        jumpVel = -(jumpGravity * timeToApex);
    }

    void Update()
    {
        if (phyObj.collisionInfo.below || phyObj.collisionInfo.above) targetVel.y = 0;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        if (Input.GetKeyDown(KeyCode.Space) && phyObj.collisionInfo.below)
        {
            targetVel.y = jumpVel;
        }
        float rawTargetXVel = input.x * moveSpeed;
        targetVel.x = Mathf.SmoothDamp(targetVel.x, rawTargetXVel, ref xVelSmooth, phyObj.collisionInfo.below ? smoothTimeGrounded : smoothTimeAirborne);
        targetVel.y += jumpGravity * Time.deltaTime;
        phyObj.Move(targetVel * Time.deltaTime);
    }
}
