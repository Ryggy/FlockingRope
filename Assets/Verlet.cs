using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VerletState
{
    public Vector2 pos; // Position
    public Vector2 prevPos; // Previous Position
    public Vector2 force; // Force accumulator

    public void integrate()
    {
        Vector2 currentPos = pos;
        pos = 2.0f * pos - prevPos + force * Time.fixedDeltaTime * Time.fixedDeltaTime;
        prevPos = currentPos;
        force = Vector2.zero;
    }

    public void addForce(Vector2 f)
    {
        force += f;
    }
}

public class Verlet : MonoBehaviour
{
    public VerletState state = new VerletState();
    public Vector2 pos;

    public void Start()
    {
        state.pos = transform.position;
        state.prevPos = state.pos;
        state.force = Vector2.zero;
        pos = state.pos;
    }

    void FixedUpdate()
    {
        // Physics update for Verlet integration
        //state.integrate();

        // Update gameobject position using the state data
        transform.position = state.pos;
    }
}
