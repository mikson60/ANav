using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour {

    //public float scale = 1f;
    public float speed = 1f;

    private Queue<Vector3> path = new Queue<Vector3>();
    private Vector3 target;

    public BoidController Controller;
    private float speed_weight;

    private bool stackLock = false;

    private void Start()
    {
        speed = Controller.maxVelocity * 2;
        target = transform.position;
    }

    // Update is called once per frame 
    void Update () {
        if (!stackLock && path.Count > 0)
        {
            if (target == null) target = path.Dequeue();
            if (Vector3.Distance(transform.position, target) < 0.01f) target = path.Dequeue();
        }

        speed_weight = Mathf.Max(Vector3.Distance(Controller.flockCenter, transform.position), 1f);
        if (Vector3.Distance(Controller.flockCenter, transform.position) < Vector3.Distance(Controller.flockCenter, target))
            speed_weight = 1.0f / speed_weight;
        

        // The step size is equal to speed times frame time.
        float step = speed_weight * speed * Time.deltaTime;

        // Move our position a step closer to the target.
        transform.position = Vector3.MoveTowards(transform.position, target, step);


        
    }

    public void AddPath(Stack<Vector3> AStarPath)
    {
        stackLock = true;
        path.Clear();
        while (AStarPath.Count > 0) path.Enqueue(AStarPath.Pop());
        stackLock = false;
    }

}
