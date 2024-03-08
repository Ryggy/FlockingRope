using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RW_Visuals : MonoBehaviour
{
    private void UpdateDragVisualisation()
    {
        // Destroy previous visualization if it exists
        if (dragVisualisation != null)
            Destroy(dragVisualisation);

        // Calculate width and height of dragged section
        float width = Mathf.Abs(dragEndPos.x - dragStartPos.x);
        float height = Mathf.Abs(dragEndPos.y - dragStartPos.y);

        // Calculate position of center of dragged section
        Vector2 center = (dragStartPos + dragEndPos) / 2f;

        // Create square visualization
        dragVisualisation = new GameObject("DragVisualization");
        dragVisualisation.transform.position = new Vector3(center.x, center.y, 0f);
        for (float x = -width / 2f; x <= width / 2f; x += spacing)
        {
            for (float y = -height / 2f; y <= height / 2f; y += spacing)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.parent = dragVisualisation.transform;
                point.transform.localPosition = new Vector3(x, y, 0f);
                point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                point.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
    
    private void UpdateRopeVisualisation()
    {
        // Destroy previous visualization if it exists
        if (dragVisualisation != null)
            Destroy(dragVisualisation);

        // Create a line renderer for the dragged connection
        dragVisualisation = new GameObject("DragVisualization");
        dragVisualisation.transform.parent = lineContainer.transform;

        LineRenderer lineRenderer = dragVisualisation.AddComponent<LineRenderer>();
        lineRenderer.material = material;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;

        // Set the positions of the line renderer
        Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint.z = 0f;
        lineRenderer.SetPosition(0, selectedSphere.transform.position + new Vector3(0, 0, 1));
        lineRenderer.SetPosition(1, endPoint + new Vector3(0, 0, 1));

        Debug.DrawLine(selectedSphere.transform.position + new Vector3(0, 0, 1), endPoint + new Vector3(0, 0, 1));
    }
}
