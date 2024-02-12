using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RW_RopeManager : MonoBehaviour
{
    public int numOfNodes = 6;

    public Node[] nodes;
    public Constraint[] constraints;

    private void Start()
    {
        nodes = new Node[numOfNodes];

        for (int i = 0; i < numOfNodes; i++)
        {
            nodes[i] = new Node();
        }


    }

    public void Simulate()
    {
        foreach (Constraint constraint in constraints)
        {
            Vector2 deltaP = constraint.node2.verlet.state.pos - constraint.node1.verlet.state.pos;
            float deltaLength = deltaP.sqrMagnitude;
            if (deltaLength > 0)
            {
                float diff = (deltaLength - constraint.length) / deltaLength;

                constraint.node1.verlet.state.pos += deltaP * constraint.compensate1 * diff;
                constraint.node2.verlet.state.pos -= deltaP * constraint.compensate1 * diff;
            }
        }
    }
}



public class Node
{
    public Verlet verlet = new Verlet();

    public bool isFixed; 

}

public class Constraint
{
    public Node node1, node2;
    public float length;
    public float compensate1, compensate2;   
}

