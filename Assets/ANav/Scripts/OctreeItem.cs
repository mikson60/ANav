using System.Collections.Generic;
using UnityEngine;

public class OctreeItem : MonoBehaviour {

    public List<OctreeNode> overlappingNodes = new List<OctreeNode>();

    private Vector3 prevPos;

	private void Start () {
        prevPos = transform.position;
	}

    private void FixedUpdate()
    {
        if (transform.position != prevPos)
        {
            RefreshOwners();
            prevPos = transform.position;
        }
    }

    private void RefreshOwners()
    {
        OctreeNode.OctreeRoot.ProcessItem(this);

        List<OctreeNode> survivedNodes = new List<OctreeNode>();
        List<OctreeNode> obsoleteNodes = new List<OctreeNode>();

        foreach (OctreeNode node in overlappingNodes)
        {
            if (!node.ContainsItemPos(transform))
            {
                obsoleteNodes.Add(node);
            }
            else
            {
                survivedNodes.Add(node);
            }
        }

        overlappingNodes = survivedNodes;

        foreach (OctreeNode node in obsoleteNodes)
        {
            node.AttemptReduceSubdivisions(this);
        }
    }
}
