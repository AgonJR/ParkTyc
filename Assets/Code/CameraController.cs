using UnityEngine;
using FMODUnity;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamRef;
    private Camera _activeCam;

    [Header("Data")]
    public float wasdSpeed = 30.0f;
    public float pannSpeed = 03.0f;
    [Space]
    public float zoomSpeed = 3.0f ;
    public float maxZoom   = 31.0f;      
    public float minZoom   = 11.0f;
    public bool invrseZoom = false;
    [Space]
    public float tiltSpeed =  3.0f;
    public float tiltAngle = 45.0f;

    [Header("Alternate Cameras")]
    public KeyCode CameraToggle = KeyCode.C;
    public Camera OverheadCamera;

    [Header("Debugging")]
    public GameObject debugUIPanel;
    public GameObject screenBlockr;
    public GameObject gameplayHUD;
    public GameObject controlsPop;

    private Vector2 panStartPos;
    private float panStartHight;

    private float minX = 0.0f;
    private float maxX = 0.0f;
    private float minZ = 0.0f;
    private float maxZ = 0.0f;

    private float currentY;

    void Awake()
    {
        // Assume Controller's On Main Camera
        _mainCamRef = GetComponent<Camera>();
        _activeCam = _mainCamRef;
    }

    void Update()
    {
        processPAN();
        processWASD();
        processTilt();
        processDebug();
        processScroll();
        processAltToggle();

        currentY = _activeCam.transform.position.y;
        RuntimeManager.StudioSystem.setParameterByName("CameraHeight", currentY);
    }

    private void processWASD()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(hInput) > 0.0f || Mathf.Abs(vInput) > 0.0f)
        {
            Vector3 moveDirection = new Vector3(hInput, 0.0f, vInput).normalized;

            Vector3 newPosition = _activeCam.transform.position + moveDirection * wasdSpeed * Time.deltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            _activeCam.transform.position = newPosition;
        }
    }

    private void processTilt()
    {
        float tiltDir = 0;
        tiltDir += Input.GetKey(KeyCode.E) ?  1 : 0;
        tiltDir += Input.GetKey(KeyCode.Q) ? -1 : 0;

        int rX = _activeCam == OverheadCamera ? 90 : 50;

        Quaternion targetRotation = Quaternion.Euler(rX, tiltAngle * tiltDir, 0);
        _activeCam.transform.localRotation = Quaternion.Slerp(_activeCam.transform.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    private void processScroll()
    {
        float scrollInput  = Input.GetAxis("Mouse ScrollWheel") * (invrseZoom ? 1.0f : -1.0f);

        if (Mathf.Abs(scrollInput) > 0.0f)
        {
            CalculateXZMinMax();

            float newYPosition = _activeCam.transform.position.y + scrollInput * zoomSpeed;

            newYPosition = Mathf.Clamp(newYPosition, minZoom, maxZoom);

            _activeCam.transform.position = new Vector3(_activeCam.transform.position.x, newYPosition, _activeCam.transform.position.z);

        }
    }

    private void processDebug()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleDebugPanel();
        }

        if ( controlsPop.activeInHierarchy )
        {
            if ( Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space) 
                || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) 
                || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)  
                || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ) 
                {
                    controlsPop.SetActive(false);
                }
        }
        else if (Input.GetKeyDown(KeyCode.Tab) )
        {
            controlsPop.SetActive(true);
        }
    }

    public void ToggleDebugPanel()
    {
        screenBlockr.SetActive(!screenBlockr.activeInHierarchy);
        debugUIPanel.SetActive(!debugUIPanel.activeInHierarchy);

        gameplayHUD.SetActive(!debugUIPanel.activeInHierarchy);
    }

    private void processPAN()
    {
        if (Input.GetMouseButtonDown(1))
        {
            panStartPos = Input.mousePosition;
            panStartHight = _activeCam.transform.position.y;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 currentMousePos = Input.mousePosition;

            Vector2 deltaMousePos = currentMousePos - panStartPos;

            float pSpeed = Mathf.Min(Vector2.Distance(currentMousePos, panStartPos), pannSpeed);

            Vector3 moveDirection = new Vector3(deltaMousePos.x, 0, deltaMousePos.y) * pSpeed * Time.deltaTime;
            moveDirection = _activeCam.transform.TransformDirection(moveDirection);

            Vector3 newPosition = _activeCam.transform.position - moveDirection;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = panStartHight;
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            _activeCam.transform.position = newPosition;
            panStartPos = currentMousePos;
        }
    }

    private void processAltToggle()
    {
        if (Input.GetKeyDown(CameraToggle))
        {
            // Only make sense for two cams
            _mainCamRef.enabled = !_mainCamRef.enabled;
            OverheadCamera.enabled = !OverheadCamera.enabled;

            _activeCam = _mainCamRef.enabled ? _mainCamRef : OverheadCamera;
            CalculateXZMinMax();
        }
    }

    public void CalculateXZMinMax()
    {
        int gridQ = GridManager.instance.GetGridSizeQ();
        int gridR = GridManager.instance.GetGridSizeR();

        minX = 5.0f;
        maxX = (gridQ - 1) * 7.535f;

        minZ = (gridR * -8.70f) - (_activeCam.transform.position.y / 10.0f) - 5.0f;
        maxZ = -20.0f;

        if ( _activeCam == OverheadCamera ) minZ /= 2.0f;
    }

    public void FrameGrid(Transform topLeftTile, Transform botRightTile)
    {
        if (topLeftTile != null && botRightTile != null)
        {
            Bounds bounds = new Bounds(topLeftTile.position, Vector3.zero);
            bounds.Encapsulate(botRightTile.position);

            Vector3 camPos = bounds.center - Vector3.forward * 21.0f;
            camPos = new Vector3(camPos.x, _activeCam.transform.position.y, camPos.z);

            _activeCam.transform.position = camPos;
        }
    }

    public void ToggleZoomInverse()
    {
        invrseZoom = !invrseZoom;
    }
}