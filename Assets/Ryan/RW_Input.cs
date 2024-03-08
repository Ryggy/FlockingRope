using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RW_Input : MonoBehaviour
{
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private GameObject dragVisualisation; // Added for visualization

    public GameObject selectedSphere = null;
    private bool isDraggingConnection = false;

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Handle mouse input
        Vector3 mousePos = Input.mousePosition;
        Vector3 mousePos_new = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos_new.z = 0f;

        if (isClothSimulation)
        {
            // Cloth simulation input handling
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                ClearSimulation();
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Update drag visualization
                UpdateDragVisualisation();
            }

            if (Input.GetMouseButtonUp(0))
            {
                ClearSimulation();
                InitialiseCloth(dragStartPos, dragEndPos);
                isDragging = false;
            }
        }
        else
        {
            // Rope simulation input handling
            if (Input.GetMouseButtonDown(0))
            {
                GameObject nearestPoint = GetNearestRopePoint(mousePos_new, 0.5f);
                if (nearestPoint != null)
                {
                    // Left-clicked on a rope point, select it
                    selectedSphere = nearestPoint;
                }
                else
                {
                    // Left-clicked on empty space, create a new rope point
                    // Shift + left-click creates a pinned point
                    CreateRopePoint(mousePos_new, Input.GetKey(KeyCode.LeftShift));
                }
            }

            if (Input.GetMouseButton(0) && selectedSphere != null)
            {
                isDraggingConnection = true;
                // To Do: Visualize the dragged connection 
                UpdateRopeVisualisation();
            }

            if (Input.GetMouseButtonUp(0) && isDraggingConnection)
            {
                GameObject nearestSphere = GetNearestRopePoint(mousePos_new, 0.5f);
                if (nearestSphere != null && nearestSphere != selectedSphere)
                {
                    // Left-clicked on another rope point, create connection
                    CreateConnection(selectedSphere, nearestSphere);
                }

                isDraggingConnection = false;
                selectedSphere = null;

                Destroy(dragVisualisation);
            }
        }

        // Handle disabling connectors
        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                float dist = Vector3.Distance(mousePos_new, connectors[i].point0.pos);
                if (dist <= 1.05f)
                {
                    //Debug.Log("removed connector");
                    connectors[i].enabled = false;
                }
            }
        }

        // pause and unpause simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            simulating = !simulating;
        }
    }
    
    private GameObject GetNearestRopePoint(Vector2 position, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("RopePoint"))
            {
                return collider.gameObject;
            }
        }
        return null;
    }
    

}
