using UnityEngine;
using System.Collections;
 
public class BoidFlocking : MonoBehaviour
{
    private float center_power;
    private float evade_distance;
    private float direction_power;
    private float follow_power;

    private GameObject Controller;
    private bool inited = false;
    private float minVelocity;
    private float maxVelocity;
    private float randomness;
    private GameObject chasee;

    private BoidController boidController;


    void Start ()
    {
        StartCoroutine("BoidSteering");
    }
 
    IEnumerator BoidSteering ()
    {
        while (!inited) yield return new WaitForSeconds(0.3f); ;
        boidController = Controller.GetComponent<BoidController>();

        while (true)
        {
            if (inited)
            {
                GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + Calc () * Time.deltaTime;
 
                // enforce minimum and maximum speeds for the boids
                float speed = GetComponent<Rigidbody>().velocity.magnitude;
                if (speed > maxVelocity)
                {
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * maxVelocity;
                }
                else if (speed < minVelocity)
                {
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * minVelocity;
                }
            }
 
            yield return new WaitForSeconds (0.3f);
        }
    }
 
    private Vector3 Calc ()
    {
        /*Vector3 randomize = new Vector3 ((Random.value *2) -1, (Random.value * 2) -1, (Random.value * 2) -1);
 
        randomize.Normalize();
        BoidController boidController = Controller.GetComponent<BoidController>();
        Vector3 flockCenter = boidController.flockCenter;
        Vector3 flockVelocity = boidController.flockVelocity;
        Vector3 follow = chasee.transform.localPosition;
 
        flockCenter = flockCenter - transform.localPosition;
        flockVelocity = flockVelocity - GetComponent<Rigidbody>().velocity;
        follow = follow - transform.localPosition;*/

        Vector3 v1 = Rule_1();
        Vector3 v2 = Rule_2();
        Vector3 v3 = Rule_3();
        Vector3 v4 = Rule_4();
        Vector3 v5 = Rule_5();

        return v1 + v2 + v3 + v4 + v5;
        //return (flockCenter + flockVelocity + follow * 2 + randomize * randomness);
    }
 
    public void SetController (GameObject theController)
    {
        Controller = theController;
        BoidController boidController = Controller.GetComponent<BoidController>();
        minVelocity = boidController.minVelocity;
        maxVelocity = boidController.maxVelocity;
        randomness = boidController.randomness;
        chasee = boidController.chasee;

        center_power = boidController.center_power;
        evade_distance = boidController.evade_distance;
        direction_power = boidController.direction_power;
        follow_power = boidController.follow_power;

        inited = true;
    }

    private Vector3 Rule_1()
    {
        Vector3 flockCenter = boidController.flockCenter;
        return (flockCenter - transform.localPosition) / center_power;
    }

    private Vector3 Rule_2()
    {
        Vector3 v2 = new Vector3();
        foreach (GameObject boid in boidController.boids)
        {
            if (boid != this.gameObject)
            {
                if (Vector3.Distance(boid.transform.position, this.transform.position) < evade_distance)
                {
                    v2 -= boid.transform.position - this.transform.position;
                }
            }
        }

        return v2;
    }

    private Vector3 Rule_3()
    {
        Vector3 flockVelocity = boidController.flockVelocity;
        return (flockVelocity - GetComponent<Rigidbody>().velocity) / direction_power;
    }

    private Vector3 Rule_4()
    {
        Vector3 follow = chasee.transform.localPosition;
        return (follow - transform.localPosition) / follow_power;
    }

    private Vector3 Rule_5()
    {
        Vector3 v5 = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
        v5.Normalize();
        return v5 * randomness;
    }
}