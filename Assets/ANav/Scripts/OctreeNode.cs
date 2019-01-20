using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class OctreeNode {
    public static int maxObjectLimit = 0; // # of contained items before split
    public static int maxDepth = 5;

    static private OctreeNode _octreeRoot;
    static public OctreeNode OctreeRoot
    {
        get
        {
            if (_octreeRoot == null)
            {
                _octreeRoot = new OctreeNode(null, Vector3.zero, 15f, 0, new List<OctreeItem>());
            }
            return _octreeRoot;
        }
    }

    GameObject octantGO;
    LineRenderer octantLineRenderer;

    public float halfDimensionLength; // length from center of Node to one of its faces.
    private Vector3 pos;
    private int depth;

    public List<OctreeItem> containedItems = new List<OctreeItem>();
    private LayerMask itemCollisionLayer = LayerMask.GetMask("OctreeCollision");

    public OctreeNode parent;
    private OctreeNode[] _children = new OctreeNode[8];
    public OctreeNode[] Children
    {
        get { return _children; }
    }

    public void EraseChildrenNodes()
    {
        _children = new OctreeNode[8];
    }

    [RuntimeInitializeOnLoadMethod]
    static bool Init()
    {
        return OctreeRoot == null;
    }

    public OctreeNode(OctreeNode parent, Vector3 pos, float halfDimensionLength, int depth, List<OctreeItem> potentialItems)
    {
        this.parent = parent;
        this.pos = pos;
        this.halfDimensionLength = halfDimensionLength;
        this.depth = depth;

        octantGO = new GameObject();
        octantGO.hideFlags = HideFlags.HideInHierarchy;
        octantLineRenderer = octantGO.AddComponent<LineRenderer>();

        FillCubeVisualizeCoords();

        foreach (OctreeItem item in potentialItems)
        {
            ProcessItem(item);
        }
    }

    public bool ProcessItem(OctreeItem item)
    {
        if (ContainsItemPos(item.transform)) // is the position in octant?
        {
            if (ReferenceEquals(Children[0], null)) // there are no other children
            {
                PushItem(item);
                return true;
            }
            else
            {
                foreach (OctreeNode childNode in Children)
                {
                    if (childNode.ProcessItem(item))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void PushItem(OctreeItem item)
    {
        if (!containedItems.Contains(item))
        {
            containedItems.Add(item);
            item.overlappingNodes.Add(this);
        }

        if (containedItems.Count > maxObjectLimit && depth <= maxDepth)
        {
            Split();
        }
    }

    private void Split()
    {
        foreach (OctreeItem items in containedItems)
        {
            items.overlappingNodes.Remove(this); // item forgets about THIS node because we are splitting and passing the item on.
        }

        Vector3 positionVector = new Vector3(halfDimensionLength / 2f, halfDimensionLength / 2f, halfDimensionLength / 2f);

        for (int i=0; i < 4; i++)
        {
            _children[i] = new OctreeNode(this, pos + positionVector, halfDimensionLength / 2f, depth+1, containedItems);
            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
        }

        positionVector = new Vector3(halfDimensionLength / 2f, -halfDimensionLength / 2f, halfDimensionLength / 2f);
        for (int i = 4; i < 8; i++)
        {
            _children[i] = new OctreeNode(this, pos + positionVector, halfDimensionLength / 2f, depth + 1, containedItems);
            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
        }

        containedItems.Clear();
    }

    public void AttemptReduceSubdivisions(OctreeItem item)
    {
        if (!ReferenceEquals(this, OctreeRoot) && !ChildrenInSiblingsOrMaxLimitInParentExceeded()) // If this is not a root node and the siblings of this do not have any child nodes nor is the max limit in parent exceeded...
        {
            foreach (OctreeNode node in parent.Children)
            {
                if (node == null) { continue; }
                node.KillNode(parent.Children.Where(i => !ReferenceEquals(i, this)).ToArray());
            }
            parent.EraseChildrenNodes();
        }
        else
        {
            containedItems.Remove(item);
            item.overlappingNodes.Remove(this);
        }
    }

    private void KillNode(OctreeNode[] siblingNodes)
    {
        foreach (OctreeItem octreeItem in containedItems) // for every item in to be deleted node...
        {
            octreeItem.overlappingNodes = octreeItem.overlappingNodes.Except(siblingNodes).ToList();
            octreeItem.overlappingNodes.Remove(this);
            octreeItem.overlappingNodes.Add(parent);
            parent.containedItems.Add(octreeItem);
        }

        foreach (OctreeNode node in siblingNodes)
        {
            GameObject.Destroy(node.octantGO);
        }
        GameObject.Destroy(octantGO);
    }

    private bool ChildrenInSiblingsOrMaxLimitInParentExceeded() // true if children nodes are present in siblings or if their total number of items is too much for the parent to accept
    {
        List<OctreeItem> legacyItems = new List<OctreeItem>();

        foreach (OctreeNode sibling in parent.Children) // iterate siblings to see if they have children
        {
            if (sibling == null) { continue; }
            if (!ReferenceEquals(sibling.Children[0], null)) // if it does have children (it is enough to check the first one) then return true and this node and its sibilings will not get deleted.
            {
                return true;
            }
            legacyItems.AddRange(sibling.containedItems.Where(i => !legacyItems.Contains(i)));
        }

        if (legacyItems.Count > maxObjectLimit + 1)
        {
            return true;
        }

        return false;
    }

    public bool ContainsItemPos(Transform itemTransform)
    {
        //if (itemTransform.position.x > pos.x + halfDimensionLength || itemTransform.position.x < pos.x - halfDimensionLength)
        //{
        //    return false;
        //}
        //if (itemTransform.position.y > pos.y + halfDimensionLength || itemTransform.position.y < pos.y - halfDimensionLength)
        //{
        //    return false;
        //}
        //if (itemTransform.position.z > pos.z + halfDimensionLength || itemTransform.position.z < pos.z - halfDimensionLength)
        //{
        //    return false;
        //}
        //return true;

        Vector3 halfVector = new Vector3(halfDimensionLength, halfDimensionLength, halfDimensionLength);
        Collider[] overlappedColliders = Physics.OverlapBox(pos, halfVector, Quaternion.identity, itemCollisionLayer);

        //DebugUtilities.DrawBoxCastBox(pos, halfVector, Quaternion.identity, Vector3.zero, halfDimensionLength, Color.red);

        if (overlappedColliders.Length > 0)
        {
            foreach (Collider col in overlappedColliders)
            {
                if (col.transform == itemTransform)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void FillCubeVisualizeCoords()
    {
        Vector3[] cubeCoords = new Vector3[8];

        Vector3 corner = new Vector3(halfDimensionLength, halfDimensionLength, halfDimensionLength);

        for (int x = 0; x < 4; x++)
        {
            cubeCoords[x] = pos + corner;
            corner = Quaternion.Euler(0f, 90f, 0f) * corner;
        }
        corner = new Vector3(halfDimensionLength, -halfDimensionLength, halfDimensionLength);

        for (int x = 4; x < 8; x++)
        {
            cubeCoords[x] = pos + corner;
            corner = Quaternion.Euler(0f, 90f, 0f) * corner;
        }

        octantLineRenderer.useWorldSpace = true;
        octantLineRenderer.positionCount = 16;
        octantLineRenderer.startWidth = 0.03f;
        octantLineRenderer.endWidth = 0.03f;

        octantLineRenderer.SetPosition(0, cubeCoords[0]);
        octantLineRenderer.SetPosition(1, cubeCoords[1]);
        octantLineRenderer.SetPosition(2, cubeCoords[2]);
        octantLineRenderer.SetPosition(3, cubeCoords[3]);
        octantLineRenderer.SetPosition(4, cubeCoords[0]);
        octantLineRenderer.SetPosition(5, cubeCoords[4]);
        octantLineRenderer.SetPosition(6, cubeCoords[5]);
        octantLineRenderer.SetPosition(7, cubeCoords[1]);

        octantLineRenderer.SetPosition(8, cubeCoords[5]);
        octantLineRenderer.SetPosition(9, cubeCoords[6]);
        octantLineRenderer.SetPosition(10, cubeCoords[2]);
        octantLineRenderer.SetPosition(11, cubeCoords[6]);
        octantLineRenderer.SetPosition(12, cubeCoords[7]);
        octantLineRenderer.SetPosition(13, cubeCoords[3]);
        octantLineRenderer.SetPosition(14, cubeCoords[7]);
        octantLineRenderer.SetPosition(15, cubeCoords[4]);
    }
}
