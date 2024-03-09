using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RopeDraw : MonoBehaviour
{
    private int rows = 8;
    private int columns = 8;
    [SerializeField] private int numNodes = 0;
    [SerializeField] private float spacing = 0.5f;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject pointPrefabNoCol;
    [SerializeField] private Material material;
    [SerializeField] private Material pinnedMaterial;
    [SerializeField] private Camera mainCamera;
    private List<GameObject> spheres = new List<GameObject>();
    private List<Particle> particles = new List<Particle>();
    private List<Connector> connectors = new List<Connector>();
   
    [SerializeField] private GameObject sphereContainer;
    [SerializeField] private GameObject lineContainer;

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private GameObject dragVisualisation; // Added for visualization

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
        }
        else
        {
            //InitialiseRope(startPos, endPos);
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
        numNodes = rows * columns;
        for (int y = 0; y <= rows; y++)
        {
            for (int x = 0; x <= columns; x++)
            {
                // Create a sphere
                GameObject sphere = Instantiate(pointPrefabNoCol, sphereContainer.transform);
                var mat = sphere.GetComponent<Renderer>();
                mat.material = material;
                sphere.transform.position = new Vector2(spawnParticlePos.x, spawnParticlePos.y);
                sphere.transform.localScale = new Vector2(0.2f, 0.2f);

                // Create particle
                Particle point = new Particle();
                point.visual = sphere;
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
            GameObject sphere = Instantiate(pointPrefab, sphereContainer.transform);
            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material = material;
            sphere.transform.position = new Vector2(spawnPos.x, spawnPos.y);
            sphere.transform.localScale = new Vector2(0.2f, 0.2f);

            // Create particle
            Particle point = new Particle();
            point.visual = sphere;
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
        GameObject sphere = Instantiate(pointPrefab,sphereContainer.transform);
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material = material;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sphere.tag = "RopePoint";

        // Create particle
        Particle point = new Particle();
        point.pinnedPos = position;
        point.visual = sphere;
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
        GameObject lineGo = new GameObject("Line");
        lineGo.transform.parent = lineContainer.transform;

        LineRenderer line = lineGo.AddComponent<LineRenderer>();
        line.material = material;
        line.startWidth = 0.04f;
        line.endWidth = 0.04f;

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

        line.SetPosition(0, sphere1.transform.position + new Vector3(0, 0, 1));
        line.SetPosition(1, sphere2.transform.position + new Vector3(0, 0, 1));

        connector.lineRender = line;
        Debug.DrawLine(startPos, endPos);

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
                UpdateDragVisualisation();
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
                UpdateRopeVisualisation();
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

                Destroy(dragVisualisation);
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

    private void UpdateDragVisualisation()
    {
        // Destroy previous visualization if it exists
        if (dragVisualisation != null)
            Destroy(dragVisualisation);

        // Calculate width and height of dragged section
        float width = Mathf.Abs(dragEndPos.x - dragStartPos.x);
        float height = Mathf.Abs(dragEndPos.y - dragStartPos.y);

        // Calculate position of center of dragged section
        Vector2 center = (dragStartPos + dragEndPos) / 2f;

        // Create square visualization
        dragVisualisation = new GameObject("DragVisualization");
        dragVisualisation.transform.position = new Vector3(center.x, center.y, 0f);
        for (float x = -width / 2f; x <= width / 2f; x += spacing)
        {
            for (float y = -height / 2f; y <= height / 2f; y += spacing)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.parent = dragVisualisation.transform;
                point.transform.localPosition = new Vector3(x, y, 0f);
                point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                point.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    private void UpdateRopeVisualisation()
    {
        // Destroy previous visualization if it exists
        if (dragVisualisation != null)
            Destroy(dragVisualisation);

        // Create a line renderer for the dragged connection
        dragVisualisation = new GameObject("DragVisualization");
        dragVisualisation.transform.parent = lineContainer.transform;

        LineRenderer lineRenderer = dragVisualisation.AddComponent<LineRenderer>();
        lineRenderer.material = material;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;

        // Set the positions of the line renderer
        Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint.z = 0f;
        lineRenderer.SetPosition(0, selectedSphere.transform.position + new Vector3(0, 0, 1));
        lineRenderer.SetPosition(1, endPoint + new Vector3(0, 0, 1));

        Debug.DrawLine(selectedSphere.transform.position + new Vector3(0, 0, 1), endPoint + new Vector3(0, 0, 1));
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

        if (dragVisualisation != null)
        {
            Destroy(dragVisualisation);
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
        float numOfIterations = 3f;
        for (int j = 0; j < numOfIterations; j++)
        {
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
    }

    private void SetSpheresAndLines()
    {

        if (particles.Count <= 0)
        {
            return;
        }
        
        for (int p = 0; p < particles.Count; p++)
        {
            Particle point = particles[p];
            point.visual.transform.position = new Vector2(point.pos.x, point.pos.y);
        }

        if (connectors.Count <= 0)
        {
            return;
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
    private void Update()
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
        public GameObject visual;
        public bool pinned = false;
        public Vector2 pinnedPos;
        public Vector2 pos;
        public Vector2 oldPos;
        public Vector2 vel;
        public float gravity = -0.24f;
        public float friction = 0.99f;
    }
}