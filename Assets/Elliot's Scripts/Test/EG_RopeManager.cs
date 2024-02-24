using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EG_RopeManager : MonoBehaviour
{
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

        private Camera cam;


        private void Awake()
        {
            nodes = new VerletNode[totalNodes];

            Vector2 pos = transform.position;

            for (int i = 0; i < totalNodes; i++)
            {
                nodes[i] = new VerletNode(pos);
                pos.y -= nodeDistance;
            }
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































    //Point[] points = null;
    //Constraint[] constraints = null;

    //public float gravity = 10;
    //public int iterations = 10;
    //int[] order;

    //public bool constraintStickMinLength = true;


    //public void FixedUpdate()
    //{
    //    Simulate();
    //}


    //public class Point
    //{
    //    public Verlet verlet = new Verlet();
    //    public bool locked;

    //}

    //public class Constraint
    //{
    //    public Point pointA, pointB;
    //    public float length;
    //    public bool dead;

    //    public Constraint(Point pointA, Point pointB)
    //    {
    //        this.pointA = pointA;
    //        this.pointB = pointB;
    //        length = Vector2.Distance(pointA.verlet.state.pos, pointB.verlet.state.pos);
            
    //    }

    //}

    //public void Simulate()
    //{
    //    //foreach(Point p in points)
    //    //{
    //    //    if (!p.locked)
    //    //    {
    //    //        Vector2 positionBeforeUpdate = p.position;
    //    //        p.position += p.position - p.prevPosition;
    //    //        p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
    //    //        p.prevPosition = positionBeforeUpdate;
    //    //    }
    //    //}

    //    for (int i = 0; i < iterations; i++)
    //    {
    //        for (int j = 0; j < constraints.Length; j++)
    //        {
    //            Debug.DrawLine(new Vector3(constraints[j].pointA.verlet.state.pos.x, constraints[j].pointA.verlet.state.pos.y, 0),
    //                new Vector3(constraints[j].pointB.verlet.state.pos.x, constraints[j].pointB.verlet.state.pos.y, 0));

    //            Constraint constraint = constraints[order[i]];
    //            if (constraint.dead)
    //            {
    //                continue;
    //            }

    //            Vector2 constraintCenter = (constraint.pointA.verlet.state.pos + constraint.pointB.verlet.state.pos) / 2;
    //            Vector2 constraintDirection = (constraint.pointA.verlet.state.pos - constraint.pointB.verlet.state.pos).normalized;
    //            float length = (constraint.pointA.verlet.state.pos - constraint.pointB.verlet.state.pos).magnitude;

    //            if (length > constraint.length || constraintStickMinLength)
    //            {
    //                if (!constraint.pointA.locked)
    //                {
    //                    constraint.pointA.verlet.state.pos = constraintCenter + constraintDirection * constraint.length / 2;
    //                }
    //                if (!constraint.pointB.locked)
    //                {
    //                    constraint.pointB.verlet.state.pos = constraintCenter + constraintDirection * constraint.length / 2;
    //                }
    //            }

    //        }
    //    }
    //}

}
