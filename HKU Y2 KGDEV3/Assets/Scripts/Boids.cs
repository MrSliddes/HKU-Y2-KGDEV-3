using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.kfish.org/boids/pseudocode.html, programmed by Tymon Versmoren
public class Boids : MonoBehaviour
{
    [Header("Values to play with")]
    [Tooltip("Boid attraction to center of mass")]
    [Range(-1, 1)]
    public float m1 = 1;
    [Tooltip("Boid avoidance of other boids")]
    [Range(-1, 1)]
    public float m2 = 1;
    [Tooltip("Boid matching velocity of other boids")]
    [Range(-1, 1)]
    public float m3 = 1;

    [Header("Vars to change")]
    [Tooltip("Speed of all boids")]
    public float speed = 1;
    [Tooltip("Limit the boids velocities")]
    public float limitSpeed = 4;
    [Tooltip("Stimulation of wind")]
    public Vector3 wind = Vector3.zero;
    [Tooltip("Draw the gizmos too")]
    public bool drawGizmos = false;
    [Tooltip("Y axis value where boids will perch")]
    public int groundLevel = -10;
    [Header("Bounding box")]
    public int Xmin = -20;
    public int Xmax = 20;
    public int Ymin = -20;
    public int Ymax = 20;
    public int Zmin = -20;
    public int Zmax = 20;

    [Header("Components")]
    [Tooltip("Target of boids to go towords")]
    public Transform target1;
    [Tooltip("The boid to spawn")]
    public GameObject boidPrefab;

    private List<Boid> boids = new List<Boid>();

    // Start is called before the first frame update
    void Start()
    {
        SetBoidsStartPositions();
    }

    // Update is called once per frame
    void Update()
    {
        MoveBoidsToNewPosition();
    }

    /// <summary>
    /// Start the boids at a random position on the screen
    /// </summary>
    private void SetBoidsStartPositions()
    {
        for(int i = 0; i < 100; i++)
        {
            boids.Add(new Boid());
            boids[i].position = new Vector3(Random.Range(Xmin, Xmax), Random.Range(Ymin, Ymax), Random.Range(Zmin, Zmax));
            boids[i].boidPrefab = Instantiate(boidPrefab, boids[i].position, Quaternion.identity);
        }
    }

    /// <summary>
    /// This updates all boids
    /// </summary>
    private void MoveBoidsToNewPosition()
    {
        Vector3 velocity, v1, v2, v3, vTTP, boundPos;

        foreach(Boid b in boids)
        {
            // Check isPerching
            if(b.isPerching)
            {
                if(b.perchTimer > 0)
                {
                    b.perchTimer -= Time.deltaTime;
                }
                else
                {
                    b.isPerching = false;
                    b.velocity = Vector3.up;
                    b.perchTimerCooldown = Random.Range(3, 6);
                }
                continue;
            }
            b.perchTimerCooldown -= Time.deltaTime;

            // Apply rules
            v1 = m1 * Rule1(b);
            v2 = m2 * Rule2(b);
            v3 = m3 * Rule3(b);
            vTTP = TendencyTowordsPlace(b);
            boundPos = BoundPosition(b);

            // Set velocity of boid
            velocity = v1 + v2 + v3 + vTTP + boundPos + wind;
            // Limit velocity
            LimitVelocity(b);

            // Add velocity to position
            b.velocity += velocity;
            b.position = Vector3.MoveTowards(b.position, b.position + b.velocity, Time.deltaTime * speed);
            b.boidPrefab.transform.position = b.position;
        }
    }

    /// <summary>
    /// Fly towords center of mass of neighbouring boids
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 Rule1(Boid b)
    {
        // Perceived center (center of all other boids not including itself)
        Vector3 pcj = Vector3.zero;
        int i = 0; // amount of boids not perching
        foreach(Boid item in boids)
        {
            if(item != b || !item.isPerching)
            {
                i++;
                pcj = pcj + item.position;
            }
        }
        pcj = pcj / (i - 1);
        //print(pcj);

        return (pcj - b.position) / 100; /// 100; add weight // move it 1% towords center
    }

    /// <summary>
    /// Keep away from other boids
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 Rule2(Boid b)
    {
        Vector3 c = Vector3.zero;
        foreach(Boid item in boids)
        {
            if(item != b)
            {
                // Check if it is too close
                if(Vector3.Distance(item.position, b.position) < 4) //Vector3.Distance(item.position, b.position
                {
                    c = c - (item.position - b.position);
                }
            }
        }
        return c;
    }

    /// <summary>
    /// Match velocity with other boids
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 Rule3(Boid b)
    {
        // Perceived velocity
        Vector3 pvj = Vector3.zero;
        foreach(Boid item in boids)
        {
            if(item != b)
            {
                pvj = pvj + item.velocity;
            }
        }
        pvj = pvj / (float)(boids.Count - 1);

        return (pvj - b.velocity) / 8; // add a small portion (1/8) to the boids current velocity
    }

    /// <summary>
    /// Move towords the target position
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 TendencyTowordsPlace(Boid b)
    {
        return (target1.position - b.position) / 100;
    }

    /// <summary>
    /// Limit the velocity of the boid
    /// </summary>
    /// <param name="b"></param>
    private void LimitVelocity(Boid b)
    {
        if(b.velocity.sqrMagnitude > limitSpeed)
        {
            b.velocity = (b.velocity / b.velocity.magnitude) * limitSpeed;
        }
    }

    /// <summary>
    /// Bound the boids in a box
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 BoundPosition(Boid b)
    {
        Vector3 v = Vector3.zero;
        float f = 10;

        if(b.position.x < Xmin)
        {
            v.x = f;
        }
        else if(b.position.x > Xmax)
        {
            v.x = -f;
        }

        if(b.position.y < Ymin)
        {
            v.y = f;
        }
        else if(b.position.y > Ymax)
        {
            v.y = -f;
        }

        if(b.position.z < Zmin)
        {
            v.z = f;
        }
        else if(b.position.z > Zmax)
        {
            v.z = -f;
        }
        
        // Apply perch rule
        if(b.position.y < groundLevel && b.perchTimerCooldown <= 0)
        {
            b.position.y = groundLevel;
            b.isPerching = true;
            b.perchTimer = Random.Range(2, 4);
        }

        return v;
    }

    /// <summary>
    /// Make bird eat food on ground
    /// </summary>
    private void Perching()
    {
        /*
         * Perching

The desired behaviour here has the boids occasionally landing and staying on the ground for a brief period of time before returning to the flock. This is accomplished by simply holding the boid on the ground for a breif period (of random length) whenever it gets to ground level, and then letting it go.

When checking the bounds, we test if the boid is at or below ground level, and if so we make it perch. We introduce the Boolean b.perching for each boid b. In addition, we introduce a timer b.perch_timer which determines how long the boid will perch for. We make this a random time, assuming we are simulating the boid eating or resting.

Thus, within the bound_position procedure, we add the following lines:

                Integer GroundLevel

		...

                IF b.position.y < GroundLevel THEN
                        b.position.y = GroundLevel
                        b.perching = True
                END IF

It is held on the ground by simply not applying the boids rules to its behaviour (obviously, as we don't want it to move). Thus, before attempting to apply the rules we check if the boid is perching, and if so we decrement the timer b.perch_timer and skip the rest of the loop. If the boid has finished perching then we reset the b.perching flag to allow it to return to the flock.

        PROCEDURE move_all_boids_to_new_positions()

                Vector v1, v2, v3, ...
		Boid b

                FOR EACH BOID b

                        IF b.perching THEN
				IF b.perch_timer > 0 THEN
					b.perch_timer = b.perch_timer - 1
					NEXT
				ELSE
					b.perching = FALSE
				END IF
			END IF


                        v1 = rule1(b)
                        v2 = rule2(b)
                        v3 = rule3(b)
			...

                        b.velocity = b.velocity + v1 + v2 + v3 + ...
			...
                        b.position = b.position + b.velocity
                END

        END PROCEDURE

Note that nothing else needs to be done to simulate the perching behaviour. As soon as we re-apply the boids rules this boid will fly directly towards the flock and continue on as normal.

A detail I implement here is that the lower bound for the boids' motion is actually a little above ground level. That way the boids are actually discouraged from going too near the ground, and when they do go to the ground they land gently rather than ploughing into it as there is an upward push from the bounding rule. They also land less often which stops them becoming too lazy. 
         * 
         * 
         * 
         * 
         */
    }

    private void AntiFlock()
    {
        // This can easly be achieved by playing with the m1 m2 m3 values. Changing them slowly over time creates a more interesting behaivor
    }
    

    private void OnDrawGizmos()
    {
        if(!drawGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(new Vector3(0, 0, 0), new Vector3(Xmin - 10, Ymin - 10, Zmin - 10));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target1.position, 1);

        if(!Application.isPlaying) return;
        foreach(Boid item in boids)
        {
            if(item.isPerching) Gizmos.color = Color.yellow; else Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(item.position, 1);
        }
    }
}

public class Boid
{
    public GameObject boidPrefab;
    public Vector3 position;
    public Vector3 velocity = new Vector3(Random.value, Random.value, Random.value);
    public bool isPerching = false;
    public float perchTimer;
    public float perchTimerCooldown;
}