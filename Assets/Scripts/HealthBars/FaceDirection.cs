using UnityEngine;

public class CanvasFaceForward : MonoBehaviour
{
    public Transform target; // The target the canvas should face (e.g., the camera)

    void Update()
    {
        if (target != null)
        {
            // Make the canvas face the target direction
            transform.LookAt(target);
        }
    }
}

 /*
    void LookingForWater()
    {
        float checkRadius = 3.0f; // Radius for the overlap check
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Water"));
        
        if (colliders.Length > 0) // GO TO WATER
        {
            Collider closestCollider = null;
            foreach (Collider collider in colliders)
            {
                collider.transform
            }
        }
        else
        {
            Exploring();
        }
    }
    */