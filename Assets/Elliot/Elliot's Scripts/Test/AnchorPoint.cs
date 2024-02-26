using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    public GameObject anchorPointPrefab; // Prefab for the anchor points
    public LayerMask anchorPointLayer; // Layer for the anchor points

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast from the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, anchorPointLayer))
            {
                // Create an anchor point at the hit position
                GameObject anchorPoint = Instantiate(anchorPointPrefab, hit.point, Quaternion.identity);
                anchorPoint.transform.SetParent(transform); // Set as child of this object
            }
        }
    }
}
