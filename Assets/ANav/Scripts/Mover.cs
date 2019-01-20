using UnityEngine;

public class Mover : MonoBehaviour {

    private Material lastCubeMaterial;
    private Transform lastCubeTransform;

	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	void Update () {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 15f, 0f , Input.GetAxis("Vertical") * Time.deltaTime * 15f, Space.Self);
        transform.Rotate(0f, Input.GetAxis("Mouse X") * Time.deltaTime * 40f, 0f, Space.World);
        transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * 60f, 0f, 0f, Space.Self);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject newAgent = (GameObject)Instantiate(Resources.Load("Cube"));
            newAgent.transform.position = transform.position + transform.forward * 5f;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 100f))
        {
            if (hit.collider.tag == "OctCube")
            {
                GameObject cube = hit.collider.gameObject;
                Rigidbody cubeRb = cube.GetComponent<Rigidbody>();

                if (lastCubeMaterial != null)
                {
                    lastCubeMaterial.color = Color.white;
                }
                lastCubeMaterial = cube.GetComponent<Renderer>().material;
                lastCubeMaterial.color = Color.cyan;

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    cubeRb.isKinematic = true;
                    lastCubeTransform = cube.transform;
                    cube.transform.SetParent(transform);
                }
                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    cubeRb.isKinematic = false;
                    if (lastCubeTransform != null)
                    {
                        lastCubeTransform.SetParent(null);
                    }
                }
                if (Input.GetKeyUp(KeyCode.E))
                {
                    GameObject.Destroy(cube);
                }
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    cubeRb.AddForce(transform.forward * 150f);
                }
            }
        }
        else
        {
            if (lastCubeMaterial != null)
            {
                lastCubeMaterial.color = Color.white;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}
}
