using System.Collections.Generic;
using UnityEngine;

public class RopeDraw : MonoBehaviour
{
    private int rows = 8;
    private int columns = 8;
    public float spacing = 0.5f;
    public Material material;
    public Material pinnedMaterial;
    public Camera mainCamera;
    private List<GameObject> spheres = new List<GameObject>();
    private List<Particle> particles = new List<Particle>();
    private List<Connector> connectors = new List<Connector>();

    public GameObject sphereContainer;
    public GameObject lineContainer;

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private GameObject dragVisualization; // Added for visualization

    public GameObject selectedSphere = null;
    private bool isDraggingConnection = false;

    public bool isClothSimulation = true; // Toggle between cloth and rope simulation
    public bool simulating = false;

   
    void Start()
    {
        startPos = new Vector2(0, 0);
        endPos = new Vector2(rows, -columns);
        InitialiseSimulation();
    }

    private void InitialiseSimulation()
    {
        ClearSimulation();

        if (isClothSimulation)
        {
            InitialiseCloth(startPos, endPos);
            mainCamera.orthographicSize = 15f;
        }
        else
        {
            //InitialiseRope(startPos, endPos);
            mainCamera.orthographicSize = 5f;
        }
    }

    private void InitialiseCloth(Vector2 start, Vector2 end)
    {
        // Implementation for cloth simulation (spawn a grid of connected points)

        Vector2 spawnParticlePos = start;

        // Calculate rows and columns based on the difference between start and end positions
        int newRows = Mathf.RoundToInt(Mathf.Abs((end.y - start.y)) / spacing);
        int newCols = Mathf.RoundToInt(Mathf.Abs((end.x - start.x)) / spacing);

        Vector2 direction = (end - start).normalized;

        rows = newRows;
        columns = newCols;

        for (int y = 0; y <= rows; y++)
        {
            for (int x = 0; x <= columns; x++)
            {
                // Create a sphere
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var mat = sphere.GetComponent<Renderer>();
                sphere.transform.parent = sphereContainer.transform;
                mat.material = material;
                sphere.transform.position = new Vector2(spawnParticlePos.x, spawnParticlePos.y);
                sphere.transform.localScale = new Vector2(0.2f, 0.2f);

                // Create particle
                Particle point = new Particle();

                point.pinnedPos = new Vector2(spawnParticlePos.x, spawnParticlePos.y);

                // Create a vertical connector 
                if (x != 0)
                {
                    LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
                    line.transform.parent = lineContainer.transform;
                    Connector connector = new Connector();
                    connector.p0 = sphere;
                    connector.p1 = spheres[spheres.Count - 1];

                    connector.point0 = point;
                    connector.point1 = particles[particles.Count - 1];
                    connector.point0.pos = sphere.transform.position;
                    connector.point0.oldPos = sphere.transform.position;

                    connectors.Add(connector);

                    connector.lineRender = line;
                    connector.lineRender.material = material;
                }

                // Create a horizontal connector
                if (y != 0)
                {
                    LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
                    line.transform.parent = lineContainer.transform;
                    Connector connector = new Connector();
                    connector.p0 = sphere;
                    connector.p1 = spheres[(y - 1) * (columns + 1) + x];

                    connector.point0 = point;
                    connector.point1 = particles[(y - 1) * (columns + 1) + x];
                    connector.point0.pos = sphere.transform.position;
                    connector.point0.oldPos = sphere.transform.position;

                    connectors.Add(connector);

                    connector.lineRender = line;
                    connector.lineRender.material = material;
                }

                // Pin the points in the top row of the grid
                if (y == 0)
                {
                    point.pinned = true;
                    mat.material = pinnedMaterial;
                }

                // Add particle and spehere to lists
                spheres.Add(sphere);
                particles.Add(point);

                if (direction.x >= 0)
                {
                    spawnParticlePos.x += spacing;
                }
                else
                {
                    spawnParticlePos.x -= spacing;
                }
            }

            spawnParticlePos.x = start.x;

            if (direction.y > 0)
            {
                spawnParticlePos.y += spacing;
            }
            else
            {
                spawnParticlePos.y -= spacing;
            }
        }
    }
    
    private void InitialiseRope(Vector2 start, Vector2 end)
    {
        // Implementation for rope simulation (spawn a line of connected points)

        // Calculate the number of points based on the distance between start and end
        int pointCount = Mathf.RoundToInt(Vector2.Distance(start, end) / spacing);

        Vector2 direction = (end - start).normalized;
        Vector2 spawnPos = start;

        GameObject previousSphere = null; // storing previous points for optimisation
        Particle previousParticle = null; // we wont need to check previous index bc we already have a reference

        for (int i = 0; i <= pointCount; i++)
        {
            // Create a sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer renderer = sphere.GetComponent<Renderer>();
            sphere.transform.parent = sphereContainer.transform;
            renderer.material = material;
            sphere.transform.position = new Vector2(spawnPos.x, spawnPos.y);
            sphere.transform.localScale = new Vector2(0.2f, 0.2f);

            // Create particle
            Particle point = new Particle();
            point.pinnedPos = new Vector2(spawnPos.x, spawnPos.y);

            // Pin the points in the first and last index
            if (i == 0 || i == pointCount)
            {
                point.pinned = true;
                renderer.material = pinnedMaterial;
            }

            // Add particle and sphere to lists
            spheres.Add(sphere);
            particles.Add(point);

            if (i != 0)
            {
                // Create connector between current and previous nodes
                LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
                line.transform.parent = lineContainer.transform;
                line.material = material;

                Connector connector = new Connector();
                connector.p0 = sphere;
                connector.p1 = previousSphere;

                connector.point0 = point;
                connector.point1 = previousParticle;
                connector.point0.pos = sphere.transform.position;
                connector.point0.oldPos = sphere.transform.position;

                connectors.Add(connector);

                connector.lineRender = line;
            }

            // Update previous sphere and particle
            previousSphere = sphere;
            previousParticle = point;

            // Move spawn position to the next point
            spawnPos += direction * spacing;
        }
    }

    private void CreateRopePoint(Vector2 position, bool isPinned)
    {
        // Create a sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Renderer renderer = sphere.GetComponent<Renderer>();
        sphere.transform.parent = sphereContainer.transform;
        renderer.material = material;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sphere.tag = "RopePoint";

        // Create particle
        Particle point = new Particle();
        point.pinnedPos = position;

        // Set pinned state
        point.pinned = isPinned;
        if (isPinned) renderer.material = pinnedMaterial;

        // Add particle and sphere to lists
        spheres.Add(sphere);
        particles.Add(point);
    }

    private GameObject GetNearestRopePoint(Vector2 position, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("RopePoint"))
            {
                return collider.gameObject;
            }
        }
        return null;
    }

    private void CreateConnection(GameObject sphere1, GameObject sphere2)
    {
        // Create a connector between the selected spheres
        LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
        line.transform.parent = lineContainer.transform;
        line.material = material;

        Connector connector = new Connector();
        connector.p0 = sphere1;
        connector.p1 = sphere2;

        Particle particle1 = particles[spheres.IndexOf(sphere1)];
        Particle particle2 = particles[spheres.IndexOf(sphere2)];
        connector.point0 = particle1;
        connector.point1 = particle2;
        connector.point0.pos = sphere1.transform.position;
        connector.point0.oldPos = sphere1.transform.position;

        connectors.Add(connector);

        connector.lineRender = line;
    }

    private void HandleInput()
    {
        // Handle mouse input
        Vector3 mousePos = Input.mousePosition;
        Vector3 mousePos_new = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos_new.z = 0f;

        if (isClothSimulation)
        {
            // Cloth simulation input handling
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                ClearSimulation();
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Update drag visualization
                UpdateDragVisualization();
            }

            if (Input.GetMouseButtonUp(0))
            {
                ClearSimulation();
                InitialiseCloth(dragStartPos, dragEndPos);
                isDragging = false;
            }
        }
        else
        {
            // Rope simulation input handling
            if (Input.GetMouseButtonDown(0))
            {
                GameObject nearestPoint = GetNearestRopePoint(mousePos_new, 0.5f);
                if (nearestPoint != null)
                {
                    // Left-clicked on a rope point, select it
                    selectedSphere = nearestPoint;
                }
                else
                {
                    // Left-clicked on empty space, create a new rope point
                    // Shift + left-click creates a pinned point
                    CreateRopePoint(mousePos_new, Input.GetKey(KeyCode.LeftShift));
                }
            }

            if (Input.GetMouseButton(0) && selectedSphere != null)
            {
                isDraggingConnection = true;
                // To Do: Visualize the dragged connection 
            }

            if (Input.GetMouseButtonUp(0) && isDraggingConnection)
            {
                GameObject nearestSphere = GetNearestRopePoint(mousePos_new, 0.5f);
                if (nearestSphere != null && nearestSphere != selectedSphere)
                {
                    // Left-clicked on another rope point, create connection
                    CreateConnection(selectedSphere, nearestSphere);
                }

                isDraggingConnection = false;
                selectedSphere = null;
            }
        }

        // Handle disabling connectors
        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                float dist = Vector3.Distance(mousePos_new, connectors[i].point0.pos);
                if (dist <= 1.05f)
                {
                    //Debug.Log("removed connector");
                    connectors[i].enabled = false;
                }
            }
        }

        // pause and unpause simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            simulating = !simulating;
        }
    }

    private void UpdateDragVisualization()
    {
        // Destroy previous visualization if it exists
        if (dragVisualization != null)
            Destroy(dragVisualization);

        // Calculate width and height of dragged section
        float width = Mathf.Abs(dragEndPos.x - dragStartPos.x);
        float height = Mathf.Abs(dragEndPos.y - dragStartPos.y);

        // Calculate position of center of dragged section
        Vector2 center = (dragStartPos + dragEndPos) / 2f;

        // Create square visualization
        dragVisualization = new GameObject("DragVisualization");
        dragVisualization.transform.position = new Vector3(center.x, center.y, 0f);
        for (float x = -width / 2f; x <= width / 2f; x += spacing)
        {
            for (float y = -height / 2f; y <= height / 2f; y += spacing)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.parent = dragVisualization.transform;
                point.transform.localPosition = new Vector3(x, y, 0f);
                point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                point.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    private void ClearSimulation()
    {
        foreach (GameObject sphere in spheres)
        {
            Destroy(sphere);
        }

        int childs = lineContainer.transform.childCount;

        for (int i = 0; i < childs; i++)
        {
            Destroy(lineContainer.transform.GetChild(i).gameObject);
        }

        if (dragVisualization != null)
        {
            Destroy(dragVisualization);
        }

        spheres.Clear();
        particles.Clear();
        connectors.Clear();
    }

    private void UpdateParticlePositions()
    {
        for (int p = 0; p < particles.Count; p++)
        {
            Particle point = particles[p];
            if (point.pinned == true)
            {
                point.pos = point.pinnedPos;
                point.oldPos = point.pinnedPos;
            }
            else
            {
                point.vel = (point.pos - point.oldPos) * point.friction;
                point.oldPos = point.pos;

                point.pos += point.vel;
                point.pos.y += point.gravity * Time.fixedDeltaTime;
            }
        }
    }

    private void ConstraintPoints()
    {
        // Constraint the points together
        float constraintLength = 0.5f;

        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i].enabled == false)
            {
                // Do nothing
            }

            else
            {
                float dist = (connectors[i].point0.pos - connectors[i].point1.pos).magnitude;
                float error = Mathf.Abs(dist - constraintLength);

                if (dist > constraintLength)
                {
                    connectors[i].changeDir = (connectors[i].point0.pos - connectors[i].point1.pos).normalized;
                }
                else if (dist < constraintLength)
                {
                    connectors[i].changeDir = (connectors[i].point1.pos - connectors[i].point0.pos).normalized;
                }

                Vector2 changeAmount = connectors[i].changeDir * error;
                connectors[i].point0.pos -= changeAmount * 0.5f;
                connectors[i].point1.pos += changeAmount * 0.5f;
            }
        }
    }

    private void SetSpheresAndLines()
    {
        for (int p = 0; p < particles.Count; p++)
        {
            Particle point = particles[p];
            spheres[p].transform.position = new Vector2(point.pos.x, point.pos.y);
            spheres[p].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i].enabled == false)
            {
                Destroy(connectors[i].lineRender);
            }
            else
            {
                var points = new Vector3[2];
                points[0] = connectors[i].p0.transform.position + new Vector3(0, 0, 1);
                points[1] = connectors[i].p1.transform.position + new Vector3(0, 0, 1);

                connectors[i].lineRender.startWidth = 0.04f;
                connectors[i].lineRender.endWidth = 0.04f;
                connectors[i].lineRender.SetPositions(points);
            }
        }
    }


    void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (simulating)
        {
            UpdateParticlePositions();
            ConstraintPoints();
            SetSpheresAndLines();
        }
    }

    public class Connector
    {
        public bool enabled = true;
        public LineRenderer lineRender;
        public GameObject p0;
        public GameObject p1;
        public Particle point0;
        public Particle point1;
        public Vector2 changeDir;
    }

    public class Particle
    {
        public bool pinned = false;
        public Vector2 pinnedPos;
        public Vector2 pos;
        public Vector2 oldPos;
        public Vector2 vel;
        public float gravity = -0.24f;
        public float friction = 0.99f;
        public float damping = 1f;
    }
}