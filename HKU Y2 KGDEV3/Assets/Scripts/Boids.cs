using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.kfish.org/boids/pseudocode.html, programmed by Tymon Versmoren
public class Boids : MonoBehaviour
{
    [Header("Vars to change")]
    [Tooltip("Speed of all boids")]
    public float speed = 1;
    [Tooltip("Limit the boids velocities")]
    public float limitSpeed = 4;
    [Tooltip("Stimulation of wind")]
    public Vector3 wind = Vector3.zero;

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
        }
    }

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
            v1 = Rule1(b);
            v2 = Rule2(b);
            v3 = Rule3(b);
            vTTP = TendencyTowordsPlace(b);
            boundPos = BoundPosition(b);

            // Set velocity of boid
            velocity = v1 + v2 + v3 + vTTP + boundPos + wind;
            // Limit velocity
            LimitVelocity(b);

            // Add velocity to position
            b.velocity += velocity;
            b.position = Vector3.MoveTowards(b.position, b.position + b.velocity, Time.deltaTime * speed);
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
        /*
         * Anti-flocking behaviour

During the course of a simulation, one may want to break up the flock for various reasons. For example the introduction of a predator may cause the flock to scatter in all directions.

Scattering the flock

Here we simply want the flock to disperse; they are not necessarily moving away from any particular object, we just want to break the cohesion (for example, the flock is startled by a loud noise). Thus we actually want to negate part of the influence of the boids rules.

Of the three rules, it turns out we only want to negate the first one (moving towards the centre of mass of neighbours) -- ie. we want to make the boids move away from the centre of mass. As for the other rules: negating the second rule (avoiding nearby objects) will simply cause the boids to actively run into each other, and negating the third rule (matching velocity with nearby boids) will introduce a semi-chaotic oscillation.

It is a good idea to use non-constant multipliers for each of the rules, allowing you to vary the influence of each rule over the course of the simulation. If you put these multipliers in the move_all_boids_to_new_positions procedure, ending up with something like:

        PROCEDURE move_all_boids_to_new_positions()

                Vector v1, v2, v3, ...
		Integer m1, m2, m3, ...
		Boid b

                FOR EACH BOID b

			...

                        v1 = m1 * rule1(b)
                        v2 = m2 * rule2(b)
                        v3 = m3 * rule3(b)
			...

                        b.velocity = b.velocity + v1 + v2 + v3 + ...
			...
                        b.position = b.position + b.velocity
                END

        END PROCEDURE

then, during the course of the simulation, simply make m1 negative to scatter the flock. Setting m1 to a positive value again will cause the flock to spontaneously re-form.
Tendency away from a particular place

If, on the other hand, we want the flock to continue the flocking behaviour but to move away from a particular place or object (such as a predator), then we need to move each boid individually away from that point. The calculation required is identical to that of moving towards a particular place, implemented above as tend_to_place; all that is required is a negative multiplier:

			Vector v
			Integer m
			Boid b

			...

			v = -m * tend_to_place(b)

So we see that each of the extra routines are very simple to implement, as are the initial rules. We achieve complex, life-like behaviour by combining all of them together. By varying the influence of each rule over time we can change the behaviour of the flock to respond to events in the environment such as sounds, currents and predators.

Auxiliary functions

You will find it handy to set up a set of Vector manipulation routines first to do addition, subtraction and scalar multiplication and division. For example, all the additions and subtractions in the above pseudocode are vector operations, so for example the line:

			pcJ = pcJ + b.position

will end up looking something like:

			pcJ = Vector_Add(pcJ, b.position)

where Vector_Add is a procedure defined thus:

	PROCEDURE Vector_Add(Vector v1, Vector v2)

		Vector v

		v.x = v1.x + v2.x
		v.y = v1.y + v2.y
		v.z = v1.z + v2.z

		RETURN v

	END PROCEDURE

and the line:

			pcJ = pcJ / N-1

will be something like:

			pcJ = Vector_Div(pcJ, N-1)

where Vector_Div is a scalar division:

	PROCEDURE Vector_Div(Vector v1, Integer A)

		Vector v

		v.x = v1.x / A
		v.y = v1.y / A
		v.z = v1.z / A

		RETURN v

	END PROCEDURE

Of course if you're doing this in two dimensions you won't need the z-axis terms, and if you're doing this in more than three dimensions you'll need to add more terms :) 
         */
    }

    private void OnDrawGizmos()
    {
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
    public Vector3 position;
    public Vector3 velocity = new Vector3(Random.value, Random.value, Random.value);
    public bool isPerching = false;
    public float perchTimer;
    public float perchTimerCooldown;
}