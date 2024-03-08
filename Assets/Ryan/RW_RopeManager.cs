using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RW_RopeManager : MonoBehaviour
{
    private int rows = 8;
    private int columns = 8;
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
   
    public bool isClothSimulation = true; // Toggle between cloth and rope simulation
    public bool simulating = false;
    
    
    void Start()
    {
        startPos = new Vector2(0, 0);
        endPos = new Vector2(rows, -columns);
        InitialiseSimulation();
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
    
    private void InitialiseSimulation()
    {
        ClearSimulation();

        if (isClothSimulation)
        {
            InitialiseCloth(startPos, endPos);
        }
        else
        {
            InitialiseRope(startPos, endPos);
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
                GameObject sphere = Instantiate(pointPrefabNoCol, sphereContainer.transform);
                var mat = sphere.GetComponent<Renderer>();
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
            GameObject sphere = Instantiate(pointPrefab, sphereContainer.transform);
            Renderer renderer = sphere.GetComponent<Renderer>();
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
        
        // destroy drag visual
        // if (dragVisualisation != null)
        // {
        //     Destroy(dragVisualisation);
        // }

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
        for (int p = 0; p < particles.Count; p++)
        {
            Particle point = particles[p];
            spheres[p].transform.position = new Vector2(point.pos.x, point.pos.y);
            //spheres[p].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
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

        // Set pinned state
        point.pinned = isPinned;
        if (isPinned) renderer.material = pinnedMaterial;

        // Add particle and sphere to lists
        spheres.Add(sphere);
        particles.Add(point);
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