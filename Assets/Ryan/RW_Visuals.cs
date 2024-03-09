using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RW_Visuals : MonoBehaviour
{
    private RW_Input input;
    private RW_RopeManager ropeManager;
    private void Start()
    {
        if (input == null)
        {
            input = GetComponent<RW_Input>();
        }
        if (ropeManager == null)
        {
            ropeManager = GetComponent<RW_RopeManager>();
        }
    }
    public void UpdateDragVisualisation()
    {
        // Destroy previous visualization if it exists
        if (input.dragVisualisation != null)
            Destroy(input.dragVisualisation);

        // Calculate width and height of dragged section
        float width = Mathf.Abs(input.dragEndPos.x - input.dragStartPos.x);
        float height = Mathf.Abs(input.dragEndPos.y - input.dragStartPos.y);

        // Calculate position of center of dragged section
        Vector2 center = (input.dragStartPos + input.dragEndPos) / 2f;

        // Create square visualization
        input.dragVisualisation = new GameObject("DragVisualization");
        input.dragVisualisation.transform.position = new Vector3(center.x, center.y, 0f);
        for (float x = -width / 2f; x <= width / 2f; x += ropeManager.spacing)
        {
            for (float y = -height / 2f; y <= height / 2f; y += ropeManager.spacing)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.parent = input.dragVisualisation.transform;
                point.transform.localPosition = new Vector3(x, y, 0f);
                point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                point.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
    
    public void UpdateRopeVisualisation()
    {
        // Destroy previous visualization if it exists
        if (input.dragVisualisation != null)
            Destroy(input.dragVisualisation);

        // Create a line renderer for the dragged connection
        input.dragVisualisation = new GameObject("DragVisualization");
        input.dragVisualisation.transform.parent = ropeManager.lineContainer.transform;

        LineRenderer lineRenderer = input.dragVisualisation.AddComponent<LineRenderer>();
        lineRenderer.material = ropeManager.material;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;

        // Set the positions of the line renderer
        Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint.z = 0f;
        lineRenderer.SetPosition(0, input.selectedSphere.transform.position + new Vector3(0, 0, 1));
        lineRenderer.SetPosition(1, endPoint + new Vector3(0, 0, 1));

        Debug.DrawLine(input.selectedSphere.transform.position + new Vector3(0, 0, 1), endPoint + new Vector3(0, 0, 1));
    }
}
