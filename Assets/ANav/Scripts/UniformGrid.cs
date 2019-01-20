using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniformGrid : MonoBehaviour {

    public static float cellSize = 2f;

    [SerializeField] private int maxGridSizeX = 4;
    [SerializeField] private int maxGridSizeY = 4;
    [SerializeField] private int maxGridSizeZ = 4;

    UniformGridNode[,,] gridNodes;

    public Stack<Vector3> path = new Stack<Vector3>();

    void Start () {
        InitGrid();
	}

    //private void Update()
    //{
    //    for (ushort x = 0; x < maxGridSizeX; x++)
    //    {
    //        for (ushort y = 0; y < maxGridSizeY; y++)
    //        {
    //            for (ushort z = 0; z < maxGridSizeZ; z++)
    //            {
    //                // gridNodes[x, y, z].DebugNodeOverlap();
    //            }
    //        }
    //    }
    //}

    private void InitGrid()
    {
        gridNodes = new UniformGridNode[maxGridSizeX, maxGridSizeY, maxGridSizeZ];

        for (ushort x =0; x < maxGridSizeX; x++)
        {
            for (ushort y = 0; y < maxGridSizeY; y++)
            {
                for (ushort z = 0; z < maxGridSizeZ; z++)
                {
                    gridNodes[x, y, z] = new UniformGridNode(x, y, z, Mathf.Infinity, cellSize / 4f, Mathf.Infinity, new GameObject());
                }
            }
        }
    }

    public IEnumerator FindPathRoutine(Vector3 startPos, Vector3 endPos)
    {
        path.Clear();

        ushort startX = (ushort)Mathf.Floor(startPos.x / cellSize);
        ushort startY = (ushort)Mathf.Floor(startPos.y / cellSize);
        ushort startZ = (ushort)Mathf.Floor(startPos.z / cellSize);

        ushort endX = (ushort)Mathf.Floor(endPos.x / cellSize);
        ushort endY = (ushort)Mathf.Floor(endPos.y / cellSize);
        ushort endZ = (ushort)Mathf.Floor(endPos.z / cellSize);

        List<UniformGridNode> openList = new List<UniformGridNode>();
        List<UniformGridNode> closedList = new List<UniformGridNode>();

        for (ushort x = 0; x < maxGridSizeX; x++)
        {
            for (ushort y = 0; y < maxGridSizeY; y++)
            {
                for (ushort z = 0; z < maxGridSizeZ; z++)
                {
                    if (gridNodes[x, y, z].isTraversable)
                    {
                        gridNodes[x, y, z].ChangeLRColor(Color.grey);
                        gridNodes[x, y, z].g = Mathf.Infinity;
                        gridNodes[x, y, z].parents.Clear();
                        gridNodes[x, y, z].CalculateDistanceToEnd(endX, endY, endZ);
                    }
                }
            }
        }

        gridNodes[startX, startY, startZ].ChangeLRColor(Color.yellow);
        gridNodes[endX, endY, endZ].ChangeLRColor(Color.cyan);

        gridNodes[startX, startY, startZ].g = 0f;

        openList.Add(gridNodes[startX, startY, startZ]);

        UniformGridNode endNode = new UniformGridNode();

        while (openList.Count > 0)
        {
            yield return new WaitForSeconds(0.01f);

            UniformGridNode currentNode = GetHighestPriorityNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            currentNode.ChangeLRColor(Color.red);
            if (currentNode.x == endX && currentNode.y == endY && currentNode.z == endZ)
            {
                endNode = currentNode;
                break;
            }

            //Debug.Log("Current is: " + currentNode.toString());
            //Debug.LogFormat("Current g ({0}) and e_dist ({1})", currentNode.g, currentNode.e_distance);
            foreach (UniformGridNode neighbour in GetNeighboursNonDiagonal(currentNode))
            {
                //Debug.Log("Neighbour: " + neighbour.toString());
                //Debug.Log("Neighbour g: " + neighbour.g);
                //Debug.Log("Neighbour e: " + neighbour.e_distance);
                if (NodeListContains(openList, neighbour))
                {
                    //Debug.Log("Open contains");
                    if (currentNode.g + neighbour.travelCost < neighbour.g)
                    {
                        //Debug.Log("Updated open");
                        neighbour.g = currentNode.g + neighbour.travelCost;
                        neighbour.parents.Clear();
                        neighbour.parents.Add(currentNode);
                    }
                }
                else if (NodeListContains(closedList, neighbour))
                {
                    //Debug.Log("Closed contains");
                    if (currentNode.g + neighbour.travelCost < neighbour.g)
                    {
                        //Debug.LogFormat("Closed g ({0}) and e_dist ({1})", neighbour.g, neighbour.e_distance);
                        // Debug.Log("Updated closed");
                        neighbour.g = currentNode.g + neighbour.travelCost;
                        neighbour.parents.Clear();
                        neighbour.parents.Add(currentNode);
                        closedList.Remove(neighbour);
                        openList.Add(neighbour);
                    }
                }
                else
                {
                    // Debug.Log("Unexplored");
                    neighbour.g = currentNode.g + neighbour.travelCost;
                    neighbour.parents.Clear();
                    neighbour.parents.Add(currentNode);
                    openList.Add(neighbour);
                }
            }
        }

        //Debug.Log("We got the path!");

        // Debug.Log("here:::: " + gridNodes[0,0,0].parents.Count);

        int breakCounter = 500;

        while (endNode.parents.Count > 0 && breakCounter > 0)
        {
            yield return new WaitForSeconds(0.01f);
            endNode.ChangeLRColor(Color.cyan);
            path.Push(new Vector3(endNode.x, endNode.y, endNode.z));

            // Debug.Log("(" + endNode.parents[0].x + "," + endNode.parents[0].y + "," + endNode.parents[0].z + ")");
            // Debug.Log("g val: " + endNode.g);
            // Debug.Log("e_dist: " + endNode.e_distance);

            endNode = endNode.parents[0];

            breakCounter--;
        }
    }

    private UniformGridNode GetHighestPriorityNode(List<UniformGridNode> openList)
    {
        UniformGridNode highestPriorityNode = openList[0];
        foreach (UniformGridNode node in openList)
        {
            if (node.g + node.e_distance < highestPriorityNode.g + highestPriorityNode.e_distance)
            {
                highestPriorityNode = node;
            }
        }
        return highestPriorityNode;
    }

    private List<UniformGridNode> GetNeighboursNonDiagonal(UniformGridNode node)
    {
        List<UniformGridNode> neighbours = new List<UniformGridNode>();

        // TOP
        if (node.y < gridNodes.GetLength(1) - 1)
        {
            neighbours.Add(gridNodes[node.x, node.y + 1, node.z]);
        }
        // BOTTOM
        if (node.y > 0)
        {
            neighbours.Add(gridNodes[node.x, node.y - 1, node.z]);
        }
        // LEFT
        if (node.x > 0)
        {
            neighbours.Add(gridNodes[node.x-1, node.y, node.z]);
        }
        // RIGHT
        if (node.x < gridNodes.GetLength(0) - 1)
        {
            neighbours.Add(gridNodes[node.x + 1, node.y, node.z]);
        }
        // FORWARD
        if (node.z < gridNodes.GetLength(2) - 1)
        {
            neighbours.Add(gridNodes[node.x, node.y, node.z + 1]);
        }
        // BACK
        if (node.z > 0)
        {
            neighbours.Add(gridNodes[node.x, node.y, node.z - 1]);
        }

        neighbours.RemoveAll(n => !n.isTraversable);

        return neighbours;
    }

    private bool NodeListContains(List<UniformGridNode> nodeList, UniformGridNode node)
    {
        bool contains = false;
        foreach (UniformGridNode n in nodeList)
        {
            if (n.x == node.x && n.y == node.y && n.z == node.z)
            {
                contains = true;
                break;
            }
        }
        return contains;
    }
}

class UniformGridNode
{
    public ushort x, y, z;

    public float e_distance;
    public float travelCost;
    public float g;

    public List<UniformGridNode> parents;

    private GameObject gameObject;

    public bool isTraversable;

    public UniformGridNode() { }

    public UniformGridNode(ushort x, ushort y, ushort z, float e_distance, float travelCost, float g, GameObject go)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.e_distance = e_distance;
        this.travelCost = travelCost;
        this.g = g;
        this.parents = new List<UniformGridNode>();
        this.gameObject = go;

        isTraversable = true;
        isTraversable = IsTraversable();

        go.hideFlags = HideFlags.HideInHierarchy;
        go.AddComponent<LineRenderer>();
        FillCubeVisualizeCoords(isTraversable);
    }

    public void SetG(float newG)
    {
        Debug.LogFormat("Setting g of {0} to {1}", toString(), newG);
        this.g = newG;
    }

    public void CalculateDistanceToEnd(ushort end_x, ushort end_y, ushort end_z)
    {
        // Euclidean
        //float dx = Mathf.Abs(end_x - x);
        //float dy = Mathf.Abs(end_y - y);
        //float dz = Mathf.Abs(end_z - z);


        //e_distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        // Manhattan
        e_distance = Mathf.Abs(end_x - x) + Mathf.Abs(end_y - y) + Mathf.Abs(end_z - z);
    }

    public void DebugNodeOverlap()
    {
        DebugUtilities.DrawBoxCastBox(WorldLocation(), new Vector3(UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f), Quaternion.identity, Vector3.zero, 0f, Color.red);
    }

    public void ChangeLRColor(Color newCol)
    {
        if (this.gameObject != null)
        {
            LineRenderer lr = this.gameObject.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.material.color = newCol;
            }
        }
    }

    private bool IsTraversable()
    {
        Collider[] overlapCollisions = Physics.OverlapBox(
            WorldLocation(),
            new Vector3(UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f),
            Quaternion.identity,
            LayerMask.GetMask("NavBlock"));

        return overlapCollisions.Length <= 0;
    }

    private void FillCubeVisualizeCoords(bool isTraversable)
    {
        if (!isTraversable) { return; }

        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = Resources.Load("LineMat") as Material;
        lineRenderer.startColor = isTraversable ? Color.grey : Color.red;
        lineRenderer.endColor = isTraversable ? Color.grey : Color.red;
        lineRenderer.material.color = isTraversable ? Color.grey : Color.red;

        Vector3[] cubeCoords = new Vector3[8];

        Vector3 corner = new Vector3(UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f);

        for (int x = 0; x < 4; x++)
        {
            cubeCoords[x] = WorldLocation() + corner;
            corner = Quaternion.Euler(0f, 90f, 0f) * corner;
        }
        corner = new Vector3(UniformGrid.cellSize / 2f, -UniformGrid.cellSize / 2f, UniformGrid.cellSize / 2f);

        for (int x = 4; x < 8; x++)
        {
            cubeCoords[x] = WorldLocation() + corner;
            corner = Quaternion.Euler(0f, 90f, 0f) * corner;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 16;
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;

        lineRenderer.SetPosition(0, cubeCoords[0]);
        lineRenderer.SetPosition(1, cubeCoords[1]);
        lineRenderer.SetPosition(2, cubeCoords[2]);
        lineRenderer.SetPosition(3, cubeCoords[3]);
        lineRenderer.SetPosition(4, cubeCoords[0]);
        lineRenderer.SetPosition(5, cubeCoords[4]);
        lineRenderer.SetPosition(6, cubeCoords[5]);
        lineRenderer.SetPosition(7, cubeCoords[1]);

        lineRenderer.SetPosition(8, cubeCoords[5]);
        lineRenderer.SetPosition(9, cubeCoords[6]);
        lineRenderer.SetPosition(10, cubeCoords[2]);
        lineRenderer.SetPosition(11, cubeCoords[6]);
        lineRenderer.SetPosition(12, cubeCoords[7]);
        lineRenderer.SetPosition(13, cubeCoords[3]);
        lineRenderer.SetPosition(14, cubeCoords[7]);
        lineRenderer.SetPosition(15, cubeCoords[4]);
    }

    private Vector3 WorldLocation()
    {
        return new Vector3((float)x * UniformGrid.cellSize, (float)y * UniformGrid.cellSize, (float)z * UniformGrid.cellSize);
    }

    public string toString()
    {
        return "(" + x + "," + y + "," + z + ")";
    }
}