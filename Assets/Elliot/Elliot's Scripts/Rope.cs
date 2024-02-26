using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletNode
{
    public Vector2 position;
    public Vector2 oldPosition;

    public VerletNode(Vector2 startPos)
    {
        this.position = startPos;
        this.oldPosition = startPos;
    }
}

public class Rope : MonoBehaviour
{
    public int iterations = 80;
    public int totalNodes = 40;

    public float nodeDistance = 0.1f;

    public Vector2 gravity;

    private VerletNode[] nodes;

    public Camera cam;


    private void Awake()
    {
        nodes = new VerletNode[totalNodes];

        Vector2 pos = transform.position;

        for (int i = 0; i < totalNodes; i++)
        {
            nodes[i] = new VerletNode(pos);
            pos.y -= nodeDistance;
        }
        OnDrawGizmos();
    }


    private void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < iterations; i++)
        {
            ApplyConstraints();
        }
    }

    public void Simulate()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            VerletNode node = nodes[i];

            Vector2 tempNode = node.position;
            node.position += (node.position = node.oldPosition) + gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
            node.oldPosition = tempNode;
        }
    }

    public void ApplyConstraints()
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            VerletNode node1 = nodes[i];
            VerletNode node2 = nodes[i + 1];

            if (i == 0 && Input.GetMouseButtonDown(0))
            {
                Debug.Log("HELLO!!");
                node1.position = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            float diffX = node1.position.x - node2.position.x;
            float diffY = node1.position.y - node2.position.y;
            float distance = Vector2.Distance(node1.position, node2.position);
            float difference = 0;

            if (distance > 0)
            {
                difference = (nodeDistance - distance) / distance;
            }

            Vector2 translate = new Vector2(diffX, diffY) * (0.5f * difference);

            node1.position = translate;
            node2.position = translate;

        }
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        for (int i = 0; i < nodes.Length - 1; i++)
        {
            if (i % 2 == 0)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
        }

    }


}
