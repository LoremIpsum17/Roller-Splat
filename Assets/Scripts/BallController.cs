using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController singleton;

    public Rigidbody rb;
    public float speed = 15;

    private bool isTraveling;
    private Vector3 travelDirection;
    private Vector3 nextCollisionPosition;

    public ParticleSystem collisionParticle;
    public int minSwipeRecognition = 500;

    private Vector2 swipePosLastFrame;
    private Vector2 swipePosCurrentFrame;
    private Vector2 currentSwipe;

    private Color solveColor;

    private AudioSource ballAudio;
    public AudioClip levelSound;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        solveColor = Random.ColorHSV(0.5f, 1);
        GetComponent<MeshRenderer>().material.color = solveColor;
        ballAudio = GetComponent<AudioSource>();
        collisionParticle = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        // Set the balls speed when it travels
        if(isTraveling)
        {
            rb.velocity = speed * travelDirection;
        }

        // Creates a small ball underneath the ball that paints the ground when it touches it
        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), 0.05f);
        int i = 0;
        while (i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();
            if (ground && !ground.isColored)
            {
                ground.ChangeColor(solveColor);
            }
            i++;
        }

        // Ball stops moving and changes direction when it hits a wall
        if(nextCollisionPosition != Vector3.zero)
        {
            if(Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTraveling =false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
            }
        }

        if (isTraveling)
            return;

        // Swipe mechanism
        if(Input.GetMouseButton(0))
        {
            // Where is the mouse now?
            swipePosCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if(swipePosLastFrame != Vector2.zero)
            {
                // Calculate the swipe direction
                currentSwipe = swipePosCurrentFrame - swipePosLastFrame;

                // Minium amount of swipe recognition
                if (currentSwipe.sqrMagnitude < minSwipeRecognition)
                    return;

                // Normalize it to get the direction
                currentSwipe.Normalize();

                // Up/Down Swipe
                if (currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back);
                }

                // Left/Right Swipe
                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);
                }
            }

            swipePosLastFrame = swipePosCurrentFrame;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
        // End of swipe mechanism
        
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        // Check which object it will collide with
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTraveling = true;
    }
}
