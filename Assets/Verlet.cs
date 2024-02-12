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

    void Start()
    {
        state.pos = transform.position;
        state.prevPos = state.pos;
        state.force = Vector2.zero;
    }

    void Update()
    {
        // GetKeyDown is unreliable in FixedUpdate, so do this here.
        if (Input.GetKeyDown(KeyCode.W))
        {
            state.prevPos.y = state.pos.y - 10.0f*Time.fixedDeltaTime;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            state.addForce(new Vector2(-7.0f, 0));
        }
        if (Input.GetKey(KeyCode.D))
        {
            state.addForce(new Vector2(7.0f, 0));
        }

        // Add gravity
        state.addForce(new Vector2(0, -9.8f));

        // Perform Verlet Integration
        state.integrate();
        
        // If the object goes below the ground, stop it.
        if (state.pos.y < -3.5f)
        {
            state.pos.y = -3.5f;
        }

        // Update gameobject using the state data
        transform.position = state.pos;
    }
}
