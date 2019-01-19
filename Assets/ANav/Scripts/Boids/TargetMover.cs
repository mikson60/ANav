using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour {

    //public float scale = 1f;
    public float speed = 1f;

    public Queue<Vector3> path = new Queue<Vector3>();
    private Vector3 target;

    public List<Vector3> pathway;

    // Update is called once per frame
    void Update () {
        //transform.Translate(0, 0, Time.deltaTime * speed * scale); // move forward
        //transform.Rotate(0, Time.deltaTime * speed, 0); // turn a little
        if (path.Count == 0) if (fillStack()) return;
        if (target == null) target = path.Dequeue();
        if (Vector3.Distance(transform.position, target) < 0.01f) target = path.Dequeue();


            // The step size is equal to speed times frame time.
        float step = speed * Time.deltaTime;

        // Move our position a step closer to the target.
        transform.position = Vector3.MoveTowards(transform.position, target, step);
        
    }

    private bool fillStack()
    {
        if (pathway.Count == 0) return false;
        foreach (Vector3 point in pathway) path.Enqueue(point);
        return true;
    }
}
