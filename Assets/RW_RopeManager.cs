using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RW_RopeManager : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();
    public List<Constraint> constraints = new List<Constraint>();

    public GameObject nodePrefab; // prefab for the node gameobject

    public int numNodes = 10; // Number of nodes in the rope
    public float spacing = 1.0f; // Spacing between nodes
    public bool fixEndPoints = true; // Whether to fix the end points of the rope

    public float minDistanceConstraint = 0.8f; // Minimum distance constraint
    public float maxDistanceConstraint = 1.2f; // Maximum distance constraint

    public int itterationCount = 10;

    private void Start()
    {
        SpawnRope();
    }

    void SpawnRope()
    {
        for (int i = 0; i < numNodes; i++)
        {
            float xPos = transform.position.x + i * spacing; // Calculate the x-position horizontally

            GameObject newNodeObj = Instantiate(nodePrefab, new Vector3(xPos, transform.position.y, 0), Quaternion.identity);
            newNodeObj.transform.parent = transform;

            Verlet newNodeVerlet = newNodeObj.GetComponent<Verlet>();

            int newNodeIndex = AddNode(newNodeVerlet.state.pos, 1.0f, fixEndPoints && (i == 0 || i == numNodes - 1));

            if (i > 0)
            {
                AddConstraint(nodes.Count - 2, newNodeIndex, spacing); // -2 bc -1 for indexing a list, another -1 bc we want the node before
            }
        }
    }

    public int AddNode(Vector2 position, float mass = 1.0f, bool fixedNode = false)
    {
        Node newNode = new Node();
        newNode.state.pos = position;
        newNode.state.prevPos = position;
        newNode.state.force = Vector2.zero;
        newNode.mass = mass;
        newNode.fixedNode = fixedNode;

        nodes.Add(newNode);
        return nodes.Count - 1; // Return the index of the newly added node
    }

    public void AddConstraint(int node1Index, int node2Index,
        float desiredDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        Constraint newConstraint = new Constraint();
        newConstraint.node1 = node1Index;
        newConstraint.node2 = node2Index;
        newConstraint.desiredDistance = desiredDistance;
        newConstraint.compensate1 = compensate1;
        newConstraint.compensate2 = compensate2;

        constraints.Add(newConstraint);
    }

    public static void ConstraintLengthMin(List<Node> nodes, List<Constraint> constraints, float minDistance)
    {
        foreach (Constraint constraint in constraints)
        {
            // Get positions of nodes involved in the constraint
            Vector2 pos1 = nodes[constraint.node1].state.pos;
            Vector2 pos2 = nodes[constraint.node2].state.pos;

            // Calculate distance between nodes
            float distance = Vector2.Distance(pos1, pos2);

            // Apply constraint based on minimum distance
            if (distance > 0 && distance < minDistance)
            {
                float diff = (minDistance - distance) / distance;
                nodes[constraint.node1].state.pos -= (pos1 - pos2) * constraint.compensate1 * diff;
                nodes[constraint.node2].state.pos += (pos1 - pos2) * constraint.compensate2 * diff;
            }
        }
    }

    public static void ConstraintLengthMax(List<Node> nodes, List<Constraint> constraints, float maxDistance)
    {
        foreach (Constraint constraint in constraints)
        {
            // Get positions of nodes involved in the constraint
            Vector2 pos1 = nodes[constraint.node1].state.pos;
            Vector2 pos2 = nodes[constraint.node2].state.pos;

            // Calculate distance between nodes
            float distance = Vector2.Distance(pos1, pos2);

            // Apply constraint based on maximum distance
            if (distance > maxDistance)
            {
                float diff = (distance - maxDistance) / distance;
                nodes[constraint.node1].state.pos += (pos1 - pos2) * constraint.compensate1 * diff;
                nodes[constraint.node2].state.pos -= (pos1 - pos2) * constraint.compensate2 * diff;
            }
        }
    }

    void FixedUpdate()
    {
        // Physics update for each node
        foreach (Node node in nodes)
        {
            // Skip fixed nodes
            if (node.fixedNode)
                continue;

            // Apply gravity force
            node.state.addForce(new Vector2(0, -9.8f));

            // Integrate node
            node.state.integrate();

            // If the node goes below the ground, stop it
            if (node.state.pos.y < -3.5f)
            {
                node.state.pos.y = -3.5f;
            }
        }
        for (int i = 0; i < itterationCount; i++)
        {
            // Satisfy all constraints
            ConstraintLengthMin(nodes, constraints, minDistanceConstraint);
            ConstraintLengthMax(nodes, constraints, maxDistanceConstraint);
        }

        //SatisfyConstraints(constraints);
    }

    void SatisfyConstraints(List<Constraint> constraints)
    {
        foreach (Constraint constraint in constraints)
        {
            // Get positions of nodes involved in the constraint
            Vector2 pos1 = nodes[constraint.node1].state.pos;
            Vector2 pos2 = nodes[constraint.node2].state.pos;

            // Calculate distance between nodes
            float distance = Vector2.Distance(pos1, pos2);

            // Apply constraint based on desired distance
            if (distance > constraint.desiredDistance)
            {
                float diff = (distance - constraint.desiredDistance) / distance;
                nodes[constraint.node1].state.pos -= (pos1 - pos2) * constraint.compensate1 * diff;
                nodes[constraint.node2].state.pos += (pos1 - pos2) * constraint.compensate2 * diff;
            }
        }
    }
}

public class Node
{
    public VerletState state = new VerletState();
    public float mass;
    public bool fixedNode;
}

public class Constraint
{
    public int node1;
    public int node2;
    public float compensate1;
    public float compensate2;
    public float desiredDistance;
}