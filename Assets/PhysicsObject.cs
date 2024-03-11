using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float gravityModifier = 1f;
    protected Vector2 velocity;
    protected const float minMoveDistance = 0.001f;

    protected ContactFilter2D contactFilter;
    protected List<RaycastHit2D> hits = new();
    protected const float shellRadius = 0.01f; // slight padding between objects (when colliding)
    public const float minGroundNormalY = 0.707f; // value of y for a unit vector at 45 deg from (0,1)
    protected Vector2 groundNormal = new(0, 1);
    protected Vector2 groundParallel;
    protected bool grounded = false;

    protected Vector2 targetVelocity;

    protected Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        contactFilter.useTriggers = false; // dont collide with triggers
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); // only collide with layers that can collide with layer of gameobject (in physics2d settings)
        contactFilter.useLayerMask = true;
    }
    // we will separately check collisions
    // y will check for ground, if slope detected, treat as flat ground
    // x will check for wall/ slope
    private void FixedUpdate()
    {
        velocity += gravityModifier * Time.deltaTime * Physics2D.gravity; // dv = g * dt
        velocity.x = targetVelocity.x;
        grounded = false;
        // initial distance we want to move (without accounting for collisions)
        Vector2 deltaPos = velocity * Time.deltaTime; // dx = v * dt

        groundParallel = new(groundNormal.y, -groundNormal.x); // gets a vector perpendicular to ground normal
        Debug.DrawRay(transform.position, groundParallel * 2, Color.red);

        Vector2 move = deltaPos.y * Vector2.up;
        Vector2 y = Movement(move, true);

        move = groundParallel * deltaPos.x; //move along slope/flat ground at same speed
        Vector2 x = Movement(move, false);

        rb.MovePosition(rb.position + (x + y));
        Debug.DrawRay(transform.position, groundNormal, Color.green);
        Debug.DrawRay(transform.position, (x + y) * 10, Color.black);
    }
    private Vector2 Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude; // distance we want to move
        if (distance > minMoveDistance)
        {
            // one problem right now is that it returns 1 raycasthit per collider, very buggy when transitioning from flat to slope
            int count = rb.Cast(move, contactFilter, hits, distance + shellRadius);
            Debug.Log($"Colliding with {count} objects");
            foreach (RaycastHit2D hit in hits)
            {
                Vector2 normal = hit.normal;
                Debug.DrawRay(hit.point, normal, Color.blue);
                if (IsGround(normal))
                {
                    grounded = true;
                    groundNormal = normal;
                    if (yMovement)
                    {
                        normal.x = 0; // if we are considered grounded, set normal to (0, 1)
                    }
                }
                if (IsWall(normal))
                {
                    if (velocity.x * normal.x > 0) velocity.x = 0;
                }
                else
                {
                    float projection = Vector2.Dot(velocity, normal);
                    if (projection < 0)
                    {
                        velocity -= projection * normal; // cancel out component of velocity parallel to normal
                    }
                }
                float modifiedDistance = hit.distance - shellRadius; // distance between object and incoming colliding obj
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }
        return (move.normalized * distance);
    }
    // if slope <= 45 deg considered ground
    private bool IsGround(Vector2 normal)
    {
        return normal.y >= minGroundNormalY;
    }
    // if 45 deg < slope < 90 deg considered wall
    private bool IsWall(Vector2 normal)
    {
        return normal.y >= 0 && normal.y < minGroundNormalY;
    }
}
