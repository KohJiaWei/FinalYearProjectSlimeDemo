using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FollowPlayer : MonoBehaviour
{
    public Transform targetToFollow;
    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = targetToFollow.position;
        Vector3 currentRotation = transform.eulerAngles; // Get current rotation
        float targetYRotation = targetToFollow.eulerAngles.y; // Get target's Y rotation

        // Apply only the Y rotation while keeping X and Z the same
        transform.rotation = Quaternion.Euler(currentRotation.x, targetYRotation, currentRotation.z);

    }
}

