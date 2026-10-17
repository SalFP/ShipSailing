using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ShipController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private Transform sailTransform; 
    [SerializeField] private UIDocument uiDocument;

    [Header("Sail Parameters (Scale)")]
    private const float MIN_SCALE_Y = 0.3f;
    private const float MAX_SCALE_Y = 2.8f;
    private const float TOP_EDGE_Y = 3.9f;

    [Header("Sail Parameters (Rotation)")]
    private const float MAX_SAIL_ROTATION = 45f;

    [Header("Reaction speed")]
    [SerializeField] private float sailChangeSpeed = 2.0f;
    [SerializeField] private float sailRotateSpeed = 45.0f;

    [Header("Wind Parameters")]
    [SerializeField] private Vector3 windDirection = new Vector3(0f, 0f, 1f);
    [SerializeField] private float windStrength = 15f;
    [SerializeField] private float waterDrag = 1.2f;
    [SerializeField] private float turnSpeed = 30f;

    [Header("New Input System Actions")]
    [SerializeField] private InputActionReference sailUpAction;
    [SerializeField] private InputActionReference sailDownAction;
    [SerializeField] private InputActionReference sailLeftAction;
    [SerializeField] private InputActionReference sailRightAction;
    [SerializeField] private InputActionReference rudderLeftAction;
    [SerializeField] private InputActionReference rudderRightAction;

    // Starting parameters
    private float sailOpenPercent = 100f;
    private float sailRotationAngle = 0f;
    private Vector3 currentVelocity = Vector3.zero;

    // UI Toolkit Elements
    private Label sailOpenLabel;
    private Label sailRotationLabel;
    private Label shipSpeedLabel;

    private void OnEnable()
    {
        if (sailUpAction != null) sailUpAction.action.Enable();
        if (sailDownAction != null) sailDownAction.action.Enable();
        if (sailLeftAction != null) sailLeftAction.action.Enable();
        if (sailRightAction != null) sailRightAction.action.Enable();
        if (rudderLeftAction != null) rudderLeftAction.action.Enable();
        if (rudderRightAction != null) rudderRightAction.action.Enable();

        
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            sailOpenLabel = root.Q<Label>("SailOpenLabel");
            sailRotationLabel = root.Q<Label>("SailRotationLabel");
            shipSpeedLabel = root.Q<Label>("ShipSpeedLabel");
        }
    }

    private void OnDisable()
    {
        if (sailUpAction != null) sailUpAction.action.Disable();
        if (sailDownAction != null) sailDownAction.action.Disable();
        if (sailLeftAction != null) sailLeftAction.action.Disable();
        if (sailRightAction != null) sailRightAction.action.Disable();
        if (rudderLeftAction != null) rudderLeftAction.action.Disable();
        if (rudderRightAction != null) rudderRightAction.action.Disable();
    }

    private void Update()
    {
        HandleSailInput();
        UpdateSailMesh();
        UpdateShipPhysics();
        UpdateUI();
    }

    private void HandleSailInput()
    {
        // Sail Up/Down
        float verticalInput = 0f;
        if (sailUpAction != null && sailUpAction.action.IsPressed()) verticalInput -= 1f;
        if (sailDownAction != null && sailDownAction.action.IsPressed()) verticalInput += 1f;

        sailOpenPercent += verticalInput * sailChangeSpeed * 50f * Time.deltaTime;
        sailOpenPercent = Mathf.Clamp(sailOpenPercent, 0f, 100f);

        // Sail Rotation
        float horizontalInput = 0f;
        if (sailLeftAction != null && sailLeftAction.action.IsPressed()) horizontalInput -= 1f;
        if (sailRightAction != null && sailRightAction.action.IsPressed()) horizontalInput += 1f;

        sailRotationAngle += horizontalInput * sailRotateSpeed * Time.deltaTime;
        sailRotationAngle = Mathf.Clamp(sailRotationAngle, -MAX_SAIL_ROTATION, MAX_SAIL_ROTATION);
    }

    private void UpdateSailMesh()
    {
        if (sailTransform == null) return;

        float currentScaleY = Mathf.Lerp(MIN_SCALE_Y, MAX_SCALE_Y, sailOpenPercent / 100f);
        float currentPosY = TOP_EDGE_Y - (currentScaleY / 2f);

        Vector3 currentScale = sailTransform.localScale;
        sailTransform.localScale = new Vector3(currentScale.x, currentScaleY, currentScale.z);

        Vector3 currentPos = sailTransform.localPosition;
        sailTransform.localPosition = new Vector3(currentPos.x, currentPosY, currentPos.z);

        sailTransform.localRotation = Quaternion.Euler(0f, sailRotationAngle, 0f);
    }

    private void UpdateShipPhysics()
    {
        Vector3 normalizedWind = windDirection.normalized;
        Vector3 sailForward = sailTransform.forward;
        Vector3 shipForward = transform.right;

        float windDotSail = Mathf.Abs(Vector3.Dot(normalizedWind, sailForward));
        float openFactor = sailOpenPercent / 100f;
        float driveForce = windDotSail * openFactor * windStrength;

        Vector3 acceleration = shipForward * driveForce;

        currentVelocity += acceleration * Time.deltaTime;
        currentVelocity -= currentVelocity * waterDrag * Time.deltaTime;

        transform.position += currentVelocity * Time.deltaTime;

        if (currentVelocity.magnitude > 0.05f)
        {
            float rudderInput = 0f;
            if (rudderLeftAction != null && rudderLeftAction.action.IsPressed()) rudderInput -= 1f;
            if (rudderRightAction != null && rudderRightAction.action.IsPressed()) rudderInput += 1f;

            transform.Rotate(Vector3.up, rudderInput * turnSpeed * Time.deltaTime);
        }
    }

    private void UpdateUI()
    {
        if (sailOpenLabel != null)
        {
            sailOpenLabel.text = $"Sail strength: {Mathf.RoundToInt(sailOpenPercent)}%";
        }

        if (sailRotationLabel != null)
        {
            sailRotationLabel.text = $"Sail Rotation: {Mathf.RoundToInt(sailRotationAngle)}°";
        }

        if (shipSpeedLabel != null)
        {
            float speed = currentVelocity.magnitude;
            shipSpeedLabel.text = $"Speed: {speed:F1} kn";
        }
    }
}