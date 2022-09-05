using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance { get { return _instance; } }

    public Transform m_cameraTransform;

    public float m_normalSpeed;
    public float m_fastSpeed;
    public float m_movementSpeed;
    public float m_movementTime;
    public float m_rotationAmount;
    public Vector3 m_zoomAmount;

    public Vector2 m_minCameraPosition;
    public Vector2 m_maxCameraPosition;

    private Vector3 m_newPosition;
    private Quaternion m_newRotation;
    private Vector3 m_newZoom;

    private Vector3 m_dragStartPosition;
    private Vector3 m_dragCurrentPosition;
    private Vector3 m_rotateStartPosition;
    private Vector3 m_rotateCurrentPosition;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        m_newPosition = transform.position;
        m_newRotation = transform.rotation;
        m_newZoom = m_cameraTransform.localPosition;
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void LateUpdate()
    {
        MoveCamera();
    }

    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y != 0)
        {
            m_newZoom += Input.mouseScrollDelta.y * m_zoomAmount;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry))
            {
                m_dragStartPosition = ray.GetPoint(entry);
            }
        }
        if(Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                m_dragCurrentPosition = ray.GetPoint(entry);

                m_newPosition = transform.position + m_dragStartPosition - m_dragCurrentPosition;
            }
        }

        if(Input.GetMouseButtonDown(2))
        {
            m_rotateStartPosition = Input.mousePosition;
        }
        if(Input.GetMouseButton(2))
        {
            m_rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = m_rotateStartPosition - m_rotateCurrentPosition;

            m_rotateStartPosition = m_rotateCurrentPosition;

            m_newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    void HandleKeyboardInput()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            m_movementSpeed = m_fastSpeed;
        }
        else
        {
            m_movementSpeed = m_normalSpeed;
        }

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            m_newPosition += (transform.forward * m_movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            m_newPosition -= (transform.forward * m_movementSpeed);
        }

        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            m_newPosition += (transform.right * m_movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            m_newPosition -= (transform.right * m_movementSpeed);
        }

        if(Input.GetKey(KeyCode.Q))
        {
            m_newRotation *= Quaternion.Euler(Vector3.up * m_rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            m_newRotation *= Quaternion.Euler(Vector3.up * -m_rotationAmount);
        }

        if(Input.GetKey(KeyCode.R))
        {
            m_newZoom += m_zoomAmount;
        }
        if (Input.GetKey(KeyCode.F))
        {
            m_newZoom -= m_zoomAmount;
        }
    }

    void MoveCamera()
    {
        if(m_newPosition.x > m_maxCameraPosition.x)
        {
            m_newPosition.x = m_maxCameraPosition.x;
        }
        if(m_newPosition.z > m_maxCameraPosition.y)
        {
            m_newPosition.z = m_maxCameraPosition.y;
        }
        if (m_newPosition.x < m_minCameraPosition.x)
        {
            m_newPosition.x = m_minCameraPosition.x;
        }
        if(m_newPosition.z < m_minCameraPosition.y)
        {
            m_newPosition.z = m_minCameraPosition.y;
        }

        m_cameraTransform.localPosition = Vector3.Lerp(m_cameraTransform.localPosition, m_newZoom, Time.deltaTime * m_movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, m_newRotation, Time.deltaTime * m_movementTime);
        transform.position = Vector3.Lerp(transform.position, m_newPosition, Time.deltaTime * m_movementTime);
    }
}
