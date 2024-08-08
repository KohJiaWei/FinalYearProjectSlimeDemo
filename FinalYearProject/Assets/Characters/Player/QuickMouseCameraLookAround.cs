using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class QuickMouseCameraLookAround : MonoBehaviour
{
    public float mouseSensitivity = 1;

    public float yaw = 0;
    public float pitch = 0;

    public bool turnThisOff = false;

    public bool lockCursor = false;

    Vector3 originalLocalEuler;
    private void Awake()
    {
        originalLocalEuler = transform.localEulerAngles;

        SetLockCursor(true);
    }

    // Update is called once per frame
    void Update()
    {
       
        if (turnThisOff)
        {
            transform.localEulerAngles = originalLocalEuler;

            SetLockCursor(false);
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.ctrlKey.wasPressedThisFrame)
        {
            SetLockCursor(!lockCursor);
        }

        if (!lockCursor) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        yaw += mouse.delta.x.value * mouseSensitivity;
        yaw = yaw >= 360 ? yaw - 360 : yaw <= -360 ? yaw + 360 : yaw;
        pitch = Mathf.Clamp(pitch - mouse.delta.y.value * mouseSensitivity, -90, 90);

        transform.localEulerAngles = new Vector3(pitch, yaw, 0);
    }

    public void SetLockCursor(bool on)
    {
        lockCursor = on;
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
