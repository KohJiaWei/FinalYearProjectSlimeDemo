using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraToggle : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    private bool isThirdPerson = true;

    void Start()
    {
        SetCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isThirdPerson = !isThirdPerson;
            SetCamera();
        }
    }

    void SetCamera()
    {
        if (isThirdPerson)
        {
            thirdPersonCamera.enabled = true;
            firstPersonCamera.enabled = false;
        }
        else
        {
            thirdPersonCamera.enabled = false;
            firstPersonCamera.enabled = true;
        }
    }
}