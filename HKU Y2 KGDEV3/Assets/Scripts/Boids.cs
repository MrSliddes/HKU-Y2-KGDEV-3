using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.kfish.org/boids/pseudocode.html
// Yes i know, grabbing the rigidbody every time isnt efficient
public class Boids : MonoBehaviour
{
    public float speed = 1;
    public float limitSpeed = 4;
    public Vector3 wind = Vector3.zero;
    public Transform target1;
    public int Xmin, Xmax, Ymin, Ymax, Zmin, Zmax;

    public GameObject[] boids;

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

    }

    private void MoveBoidsToNewPosition()
    {
        Vector3 v1, v2, v3, v4, vTTP;

        foreach(GameObject b in boids)
        {
            v1 = Rule1(b);
            v2 = Rule2(b);
            v3 = Rule3(b);
            v4 = Rule4(b);
            vTTP = TendencyTowordsPlace(b);
            

            b.GetComponent<Rigidbody>().velocity = (b.GetComponent<Rigidbody>().velocity + v1 + v2 + v3 + wind + vTTP + BoundPosition(b)) * Time.deltaTime * speed;
            LimitVelocity(b);
            b.transform.position = (b.transform.position + b.GetComponent<Rigidbody>().velocity);
        }
    }

    /// <summary>
    /// Fly towords center of mass of neighbouring boids
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 Rule1(GameObject b)
    {
        // Center of mass
        //Vector3 c = Vector3.zero;
        //foreach(GameObject bb in boids)
        //{
        //    c += bb.transform.position;
        //}
        //c = c / boids.Length;
        //print(c);

        // Perceived center (center of all other boids not including itself)
        Vector3 pcj = Vector3.zero;
        foreach(GameObject bb in boids)
        {
            if(bb != b)
            {
                pcj = pcj + bb.transform.position;
            }
        }
        pcj = pcj / (boids.Length - 1);
        //print(pcj);

        return (pcj - b.transform.position) / 100; // move it 1% towords center
    }

    // Keep a small distance away from other objects (including other boids)
    private Vector3 Rule2(GameObject b)
    {
        Vector3 c = Vector3.zero;
        foreach(GameObject bb in boids)
        {
            if(bb != b)
            {
                // Check if it is too close
                if(Vector3.Distance(bb.transform.position, b.transform.position) < 3)
                {
                    c = c - (bb.transform.position - b.transform.position);
                }
            }
        }

        return c;
    }

    // Try to match velocity with other boids
    private Vector3 Rule3(GameObject b)
    {
        // Perceived velocity
        Vector3 pvj = Vector3.zero;
        foreach(GameObject bb in boids)
        {
            if(bb != b)
            {
                pvj = pvj + bb.GetComponent<Rigidbody>().velocity;
            }
        }
        pvj = pvj / (boids.Length - 1);

        return (pvj - b.GetComponent<Rigidbody>().velocity) / 8; // add a small portion (1/8) to the boids current velocity
    }

    private Vector3 Rule4(GameObject b)
    {
        return Vector3.zero;
    }

    private Vector3 TendencyTowordsPlace(GameObject b)
    {
        return (target1.position - b.transform.position) / 100;
    }

    private void LimitVelocity(GameObject b)
    {
        if(b.GetComponent<Rigidbody>().velocity.sqrMagnitude > limitSpeed)
        {
            b.GetComponent<Rigidbody>().velocity = (b.GetComponent<Rigidbody>().velocity / b.GetComponent<Rigidbody>().velocity.magnitude) * limitSpeed;
        }
    }

    // Kinda works, boids go outside the border a bit but dont go futer
    private Vector3 BoundPosition(GameObject b)
    {
        Vector3 v = Vector3.zero;
        float f = 10;

        if(b.transform.position.x < Xmin)
        {
            v.x = f;
        }
        else if(b.transform.position.x > Xmax)
        {
            v.x = -f;
        }

        if(b.transform.position.y < Ymin)
        {
            v.y = f;
        }
        else if(b.transform.position.y > Ymax)
        {
            v.y = -f;
        }

        if(b.transform.position.z < Zmin)
        {
            v.z = f;
        }
        else if(b.transform.position.z > Zmax)
        {
            v.z = -f;
        }
        print(v);
        return v;
    }

    // Not implemented, do in future, cause i am interested in this stuff
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
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(0, 0, 0), new Vector3(Xmin - 10, Ymin - 10, Zmin - 10));
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target1.position, 1);
    }
}
