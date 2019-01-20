using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour {

    [SerializeField] Mover mover;
    [SerializeField] UniformGrid uniformGrid;

    private void Start()
    {
        // GenerateInitialOctree();
    }

    private void GenerateInitialOctree()
    {
        foreach (OctreeItem octreeItem in FindObjectsOfType<OctreeItem>())
        {
            OctreeNode.OctreeRoot.ProcessItem(octreeItem);
        }
    }

    public void StartRushToMoverRoutine()
    {

    }

    public IEnumerator RushToMover()
    {
        if (mover == null) { Debug.LogError("Mover is not specified"); }

        // Start pos is swarm current pos.
        yield return uniformGrid.StartCoroutine(uniformGrid.FindPathRoutine(Vector3.zero, mover.transform.position));

        // Start moving the swarm here.
        Stack<Vector3> pathFromStartToEnd = uniformGrid.path;
    }

    public void StopAllCoroutines()
    {
        uniformGrid.StopAllCoroutines();
        base.StopAllCoroutines();
    }
}
