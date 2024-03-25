using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeTest_Eg : MonoBehaviour
{
//    public float gravity = 9.8f;
//    public int solveIterations = 5;
//    public bool constrainStickMinLength = true;
//    protected List<Point> points;
//    protected List<Stick> sticks;
//    int[] order;

//    // Number of nodes in the chain
//    public int numNodes = 5;
//    public float segmentLength = 1f;
//    public Vector2 startPos = new Vector2(0, 0);
//    private LineRenderer lineRenderer;

//    private bool isDragging = false;
//    private Point selectedNode;

//    protected virtual void Start()
//    {

//        if (points == null)
//        {
//            points = new List<Point>();
//        }
//        if (sticks == null)
//        {
//            sticks = new List<Stick>();
//        }

//        CreateRope();

//        CreateOrderArray();

//        // Initialize LineRenderer
//        lineRenderer = gameObject.AddComponent<LineRenderer>();
//        lineRenderer.positionCount = points.Count;
//        lineRenderer.startWidth = 0.1f; // Adjust width as needed
//        lineRenderer.endWidth = 0.1f;   // Adjust width as needed
//        lineRenderer.material.color = Color.white; // Set LineRenderer color to white
//    }

//    private void CreateRope()
//    {
//        for (int i = 0; i < numNodes; i++)
//        {
//            Point newNode = new Point { position = new Vector2(0, -i * segmentLength) + startPos }; // Adjust position as needed
//            newNode.prevPosition = newNode.position; // Initialize prevPosition to match position
//            points.Add(newNode);

//            // Connect the new node to the previous node with a stick
//            if (i > 0)
//            {
//                Stick ropeStick = new Stick(points[i - 1], newNode);
//                sticks.Add(ropeStick);
//            }
//        }

//        // Fix the first node
//        if (points.Count > 0)
//        {
//            points[0].locked = true;
//        }
//    }

//    private void Update()
//    {
//        HandleInput();
//        Simulate();
//        UpdateLineRenderer();
//    }

//    private void HandleInput()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

//            // Check if the mouse click is on a node
//            foreach (Point point in points)
//            {
//                float distance = Vector2.Distance(mousePos, point.position);
//                if (distance < 0.5f) // Adjust the radius as needed
//                {
//                    isDragging = true;
//                    selectedNode = point;
//                    break;
//                }
//            }
//        }

//        if (isDragging && Input.GetMouseButton(0))
//        {
//            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//            selectedNode.position = mousePos;
//        }

//        if (Input.GetMouseButtonUp(0))
//        {
//            isDragging = false;
//        }
//    }

//    private void Simulate()
//    {
//        // update physics of each point
//        foreach (Point p in points)
//        {
//            if (!p.locked)
//            {
//                Vector2 positionBeforeUpdate = p.position;
//                p.position += p.position - p.prevPosition;
//                p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
//                p.prevPosition = positionBeforeUpdate;

//            }
//        }

//        for (int i = 0; i < solveIterations; i++)
//        {
//            for (int s = 0; s < sticks.Count; s++)
//            {
//                Stick stick = sticks[order[s]];
//                if (stick.dead)
//                {
//                    continue;
//                }

//                Vector2 stickCentre = (stick.pointA.position + stick.pointB.position) / 2;
//                Vector2 stickDir = (stick.pointA.position - stick.pointB.position).normalized;
//                float length = (stick.pointA.position - stick.pointB.position).magnitude;

//                if (length > stick.length || constrainStickMinLength)
//                {
//                    if (!stick.pointA.locked)
//                    {
//                        stick.pointA.position = stickCentre + stickDir * stick.length / 2;
//                    }
//                    if (!stick.pointB.locked)
//                    {
//                        stick.pointB.position = stickCentre - stickDir * stick.length / 2;
//                    }
//                }

//            }
//        }

//        // Draw lines between nodes
//        for (int s = 0; s < sticks.Count; s++)
//        {
//            Stick stick = sticks[s];
//            if (!stick.dead)
//            {
//                Debug.DrawLine(stick.pointA.position, stick.pointB.position, Color.green);
//            }
//        }
//    }

//    public static T[] ShuffleArray<T>(T[] array, System.Random prng)
//    {

//        int elementsRemainingToShuffle = array.Length;
//        int randomIndex = 0;

//        while (elementsRemainingToShuffle > 1)
//        {

//            // Choose a random element from array
//            randomIndex = prng.Next(0, elementsRemainingToShuffle);
//            T chosenElement = array[randomIndex];

//            // Swap the randomly chosen element with the last unshuffled element in the array
//            elementsRemainingToShuffle--;
//            array[randomIndex] = array[elementsRemainingToShuffle];
//            array[elementsRemainingToShuffle] = chosenElement;
//        }

//        return array;
//    }

//    protected void CreateOrderArray()
//    {
//        order = new int[sticks.Count];
//        for (int i = 0; i < order.Length; i++)
//        {
//            order[i] = i;
//        }
//        ShuffleArray(order, new System.Random());
//    }

//    private void UpdateLineRenderer()
//    {
//        // Update LineRenderer positions
//        for (int i = 0; i < points.Count; i++)
//        {
//            lineRenderer.SetPosition(i, points[i].position);
//        }
//    }
//}

//public class Point
//{
//    public Vector2 position, prevPosition;
//    public bool locked;
//    public float mass = 1f;
//}

//public class Stick
//{
//    public Point pointA, pointB;
//    public float length;
//    public bool dead;

//    public Stick(Point pointA, Point pointB)
//    {
//        this.pointA = pointA;
//        this.pointB = pointB;
//        length = Vector2.Distance(pointA.position, pointB.position);
//    }
}
