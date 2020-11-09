using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.kfish.org/boids/pseudocode.html
public class Boids : MonoBehaviour
{
    public float speed = 1;
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
        Vector3 v1, v2, v3;

        foreach(GameObject b in boids)
        {
            v1 = Rule1(b);
            v2 = Rule2(b);
            v3 = Rule3(b);

            b.GetComponent<Rigidbody>().velocity = (b.GetComponent<Rigidbody>().velocity + v1 + v2 + v3) * Time.deltaTime * speed;
            print(b.GetComponent<Rigidbody>().velocity);
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
}
