using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private float tilt = 0f;
    private float facing = 90f;

    [HideInInspector]
    public Transform cameraTarget;

    public float cameraHeight;
    public float sensitivity = 100;

    private void OnEnable()
    {
        sensitivity = PlayerPrefs.GetFloat("settings_sensitivity");
    }

    private void Update()
    {
        if (cameraTarget != null)
        {
            tilt -= MouseInput().y * sensitivity * Time.deltaTime;
            facing += MouseInput().x * sensitivity * Time.deltaTime;

            tilt = Mathf.Clamp(tilt, -90, 90);

            cameraTarget.eulerAngles = new Vector3(0, facing, 0);
            transform.eulerAngles = new Vector3(tilt, facing, 0);
        }
    }

    private void LateUpdate()
    {
        if (cameraTarget != null)
        {
            transform.position = cameraTarget.position + Vector3.up * cameraHeight;
        }
    }

    private Vector2 MouseInput()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
}
