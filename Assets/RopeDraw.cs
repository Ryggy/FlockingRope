using System.Collections.Generic;
using UnityEngine;

public class RopeDraw : MonoBehaviour
{
    private int rows = 32;
    private int columns = 32;
    private float spacing = 0.5f;
    public Material material;

    public List<GameObject> spheres;
    public List<Particle> particles;
    public List<Connector> connectors;

    public GameObject sphereContainer;
    public GameObject lineContainer;

    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;

    public bool simulating = false;

    // Initalize
    void Start()
    {
        InitializeRope();
    }

    private void InitializeRope()
    {
        Vector2 spawnParticlePos = new Vector2(0, 0);

        spheres = new List<GameObject>();
        particles = new List<Particle>();
        connectors = new List<Connector>();

        for (int y = 0; y <= rows; y++)
        {
            for (int x = 0; x <= columns; x++)
            {
                // Create a sphere
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var mat = sphere.GetComponent<Renderer>();
                sphere.transform.parent = sphereContainer.transform;
                mat.material = material;
                sphere.transform.position = new Vector2(spawnParticlePos.y, spawnParticlePos.x);
                sphere.transform.localScale = new Vector2(0.2f, 0.2f);

                // Create particle
                Particle point = new Particle();

                point.pinnedPos = new Vector2(spawnParticlePos.y, spawnParticlePos.x);

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
                    connector.p1 = spheres[(y - 1) * (rows + 1) + x];

                    connector.point0 = point;
                    connector.point1 = particles[(y - 1) * (rows + 1) + x];
                    connector.point0.pos = sphere.transform.position;
                    connector.point0.oldPos = sphere.transform.position;

                    connectors.Add(connector);

                    connector.lineRender = line;
                    connector.lineRender.material = material;

                }

                // Pin the points in the top row of the grid
                if (x == 0)
                {
                    point.pinned = true;
                }

                spawnParticlePos.x -= spacing;

                // Add particle and spehere to lists
                spheres.Add(sphere);
                particles.Add(point);


            }

            spawnParticlePos.x = 0;
            spawnParticlePos.y -= spacing;
        }
    }


    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnGridPoints();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Handle mouse input
        Vector3 mousePos = Input.mousePosition;
        Vector3 mousePos_new = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos_new.z = 0f;
        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                float dist = Vector3.Distance(mousePos_new, connectors[i].point0.pos);
                if (dist <= 1.05f)
                {
                    Debug.Log("removed connector");
                    connectors[i].enabled = false;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            simulating = !simulating;
        }
    }

    private void SpawnGridPoints()
    {
        ClearRope();

        int newRows = Mathf.RoundToInt(Mathf.Abs((dragEndPos - dragStartPos).y) / spacing);
        int newCols = Mathf.RoundToInt(Mathf.Abs((dragEndPos - dragStartPos).x) / spacing);

        rows = newRows;
        columns = newCols;

        InitializeRope();
    }

    private void ClearRope()
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
        var startDistance = 0.5f;
        for (int i = 0; i < connectors.Count; i++)
        {
            if (connectors[i].enabled == false)
            {
                // Do nothing
            }

            else
            {
                float dist = (connectors[i].point0.pos - connectors[i].point1.pos).magnitude;
                float error = Mathf.Abs(dist - startDistance);

                if (dist > startDistance)
                {
                    connectors[i].changeDir = (connectors[i].point0.pos - connectors[i].point1.pos).normalized;
                }
                else if (dist < startDistance)
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