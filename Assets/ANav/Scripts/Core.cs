using UnityEngine;

public class Core : MonoBehaviour {

    private void Start()
    {
        GenerateInitialOctree();
    }

    private void GenerateInitialOctree()
    {
        foreach (OctreeItem octreeItem in FindObjectsOfType<OctreeItem>())
        {
            OctreeNode.OctreeRoot.ProcessItem(octreeItem);
        }
    }
}
