using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EG_RopeManager : MonoBehaviour
{
    Point[] points = null;
    Constraint[] constraints = null;

    public float gravity = 10;
    public int iterations = 10;
    int[] order;

    public bool constraintStickMinLength = true;


    public void FixedUpdate()
    {
        Simulate();
    }


    public class Point
    {
        public Verlet verlet = new Verlet();
        public bool locked;

    }

    public class Constraint
    {
        public Point pointA, pointB;
        public float length;
        public bool dead;

        public Constraint(Point pointA, Point pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            length = Vector2.Distance(pointA.verlet.state.pos, pointB.verlet.state.pos);
            
        }

    }

    public void Simulate()
    {
        //foreach(Point p in points)
        //{
        //    if (!p.locked)
        //    {
        //        Vector2 positionBeforeUpdate = p.position;
        //        p.position += p.position - p.prevPosition;
        //        p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
        //        p.prevPosition = positionBeforeUpdate;
        //    }
        //}

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < constraints.Length; j++)
            {
                Debug.DrawLine(new Vector3(constraints[j].pointA.verlet.state.pos.x, constraints[j].pointA.verlet.state.pos.y, 0),
                    new Vector3(constraints[j].pointB.verlet.state.pos.x, constraints[j].pointB.verlet.state.pos.y, 0));

                Constraint constraint = constraints[order[i]];
                if (constraint.dead)
                {
                    continue;
                }

                Vector2 constraintCenter = (constraint.pointA.verlet.state.pos + constraint.pointB.verlet.state.pos) / 2;
                Vector2 constraintDirection = (constraint.pointA.verlet.state.pos - constraint.pointB.verlet.state.pos).normalized;
                float length = (constraint.pointA.verlet.state.pos - constraint.pointB.verlet.state.pos).magnitude;

                if (length > constraint.length || constraintStickMinLength)
                {
                    if (!constraint.pointA.locked)
                    {
                        constraint.pointA.verlet.state.pos = constraintCenter + constraintDirection * constraint.length / 2;
                    }
                    if (!constraint.pointB.locked)
                    {
                        constraint.pointB.verlet.state.pos = constraintCenter + constraintDirection * constraint.length / 2;
                    }
                }

            }
        }
    }

}
