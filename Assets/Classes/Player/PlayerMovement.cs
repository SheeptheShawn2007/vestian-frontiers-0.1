using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float sensitivity;
    public float slowSpeed;
    public float normalSpeed;
    public float sprintSpeed;
    float currentSpeed;

    public float zoomSensitivity = 10f;
    public float minZoom = 20f;
    public float maxZoom = 60f;
    private float currentZoom = 60f;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
            currentZoom = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Movement();
            Rotation();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Movement();
        }

        Zoom();
    }

    public void Rotation()
    {
        Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        transform.Rotate(mouseInput * sensitivity);
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    public void Movement()
    {
        float vertical = 0f;
        if (Input.GetKey(KeyCode.Space)) vertical += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) vertical -= 1f;
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), vertical, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
        transform.Translate(input * currentSpeed * Time.deltaTime);
    }

    public void Zoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scrollInput * zoomSensitivity;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (playerCamera != null)
            playerCamera.fieldOfView = currentZoom;
    }
}