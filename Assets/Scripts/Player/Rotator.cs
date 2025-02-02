using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] GameObject playerRef;
    [SerializeField] Transform verticalRotator;
    private float xRotation;
    private float yRotation;
    float sensitivity = 1f;


    void FixedUpdate()
    {
        transform.position = playerRef.transform.position;
    }


    void Update()
    {
        float xRot = Input.GetAxisRaw("Mouse Y");
        float yRot = Input.GetAxisRaw("Mouse X");

        xRotation -= xRot;
        yRotation += yRot;

        verticalRotator.localRotation = Quaternion.Euler(xRotation * sensitivity, 0, 0);
        transform.localRotation = Quaternion.Euler(0, yRotation * sensitivity, 0);
    }
}