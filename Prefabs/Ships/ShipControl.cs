using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipControl : MonoBehaviour
{
    [SerializeField] ShipJoystick joystick;
    [SerializeField] ShipThrottle throttle;
    [SerializeField] Switch[] shipSwitches;
    Rigidbody shipRigidbody;
    [SerializeField] float rotationForce = 1000;
    [HideInInspector] bool launchMode = false;
    [SerializeField] float cruiseForce = 1000;
    [SerializeField] float launchForce = 10000;
    [SerializeField] float translationForce = 750;
    [SerializeField] Transform[] turretPoints;
    Turret[] turrets;
    bool turretControl = false;
    bool autoStabilizeRot = false;
    bool autoStabilizePos = false;

    [SerializeField] Transform pilot;
    Camera pilotCam;

    [SerializeField] Transform hudPivot;

    [SerializeField] RectTransform turretControlUI;
    [SerializeField] GameObject turretCrosshairPrefab;
    [SerializeField] Image crosshair;
    [SerializeField] Color normalColor;
    [SerializeField] Color hoverColor;
    [SerializeField] Color triggerColor;
    bool triggerHeld = false;
    [SerializeField] Color turretNormalColor;
    [SerializeField] Color turretBlockedColor;

    [SerializeField] TorpedoPoint[] torpedoPoints;
    bool torpedoBayDoorOpen = false;
    [SerializeField] Animation bayDoorAnimation;

    int translationMode = 1;

    [SerializeField]
    [Range(-1000, 1000)]
    float P, I, D;
    PIDController xRotationPID;
    PIDController yRotationPID;
    PIDController zRotationPID;

    Transform target;

    void Start()
    {
        shipRigidbody = GetComponent<Rigidbody>();
        turrets = new Turret[turretPoints.Length];
        for (int i = 0; i < turretPoints.Length; i++)
        {
            turrets[i] = turretPoints[i].GetChild(0).GetComponent<Turret>();
            Instantiate(turretCrosshairPrefab, turretControlUI);
        }

        xRotationPID = new PIDController(P, I, D);
        yRotationPID = new PIDController(P, I, D);
        zRotationPID = new PIDController(P, I, D);
        pilotCam = pilot.GetComponent<Camera>();
    }

    private void OnEnable()
    {
        joystick.JoystickTriggerDown += StartTurretFire;
        joystick.JoystickTriggerUp += StopTurretFire;
        joystick.JoystickRelease += FireTorpedo;
        joystick.JoystickToggle += ToggleTurretControl;

        foreach (Switch _switch in shipSwitches)
        {
            _switch.OnSwitchToggle += OnSwitchToggle;
        }
    }

    private void OnDisable()
    {
        joystick.JoystickTriggerDown -= StartTurretFire;
        joystick.JoystickTriggerUp -= StopTurretFire;
        joystick.JoystickRelease -= FireTorpedo;
        joystick.JoystickToggle -= ToggleTurretControl;

        foreach (Switch _switch in shipSwitches)
        {
            _switch.OnSwitchToggle -= OnSwitchToggle;
        }
    }

    void Update()
    {
        Vector3 joystickDirection = joystick.direction;
        if(translationMode != 1)
        {
            if (joystickDirection.x != 0 && joystickDirection.z != 0)
            {
                switch (translationMode)
                {
                    case 0: // Vertical
                        shipRigidbody.AddRelativeForce(new Vector3(-joystickDirection.z, -joystickDirection.x, 0) * translationForce, ForceMode.Acceleration);
                        break;
                    case 2: // Horizontal
                        shipRigidbody.AddRelativeForce(new Vector3(-joystickDirection.z, 0, joystickDirection.x) * translationForce, ForceMode.Acceleration);
                        break;
                }
            }
        }
        else
        {
            if(!turretControl)
                shipRigidbody.AddRelativeTorque(joystickDirection * rotationForce, ForceMode.Acceleration);
        }

        hudPivot.SetPositionAndRotation(pilot.position, pilot.rotation);
        hudPivot.localEulerAngles = new Vector3(hudPivot.localEulerAngles.x, hudPivot.localEulerAngles.y, 0);
        if (turretControl)
        {
            crosshair.transform.localPosition = new Vector2(-joystick.direction.z * turretControlUI.rect.width * 0.5f, -joystick.direction.x * turretControlUI.rect.height * 0.5f);
            Vector3 crosshairDirection = crosshair.transform.position - pilot.position;

            if (Physics.Raycast(crosshair.transform.position, crosshairDirection, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform.HasTag("Target"))
                {
                    target = hit.transform;
                }
                Debug.DrawLine(crosshair.transform.position, hit.point, Color.green, 0.1f);
                if(!triggerHeld)
                    crosshair.color = hoverColor;
                for (int i = 0; i < turrets.Length; i++)
                {
                    Turret turret = turrets[i];
                    turret.targetDirection = hit.point - turret.firePoint.position;
                    Transform turretCrosshair = turretControlUI.GetChild(i + 1);
                    Vector3 turretHitPoint = Physics.Raycast(turret.firePoint.position, turret.firePoint.forward, out RaycastHit turretHit, Mathf.Infinity) ? turretHit.point : turret.firePoint.position + turret.firePoint.forward * 500;
                    
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(turretControlUI, pilotCam.WorldToScreenPoint(turretHitPoint), pilotCam, out Vector3 turretCrosshairPos))
                    {
                        turretCrosshair.position = turretCrosshairPos;
                    }
                    if (!Physics.Linecast(turret.firePoint.position, turret.firePoint.position + turret.firePoint.forward * 5, ~turret.ignoreLayers))
                    {
                        turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                    }
                    else
                    {
                        turretCrosshair.GetComponent<Image>().color = turretBlockedColor;
                    }
                }
            }
            else
            {
                target = null;
                if(!triggerHeld)
                    crosshair.color = normalColor;
                for (int i = 0; i < turrets.Length; i++)
                {
                    Turret turret = turrets[i];
                    turret.targetDirection = crosshairDirection;
                    Transform turretCrosshair = turretControlUI.GetChild(i + 1);
                    Vector3 turretAimPosition = turret.firePoint.position + turret.firePoint.forward * 500;

                    if(RectTransformUtility.ScreenPointToWorldPointInRectangle(turretControlUI, pilotCam.WorldToScreenPoint(turretAimPosition), pilotCam, out Vector3 turretCrosshairPos))
                    {
                        turretCrosshair.position = turretCrosshairPos;
                    }
                    if (!Physics.Linecast(turret.firePoint.position, turret.firePoint.position + turret.firePoint.forward * 5, ~turret.ignoreLayers))
                    {
                        turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                    }
                    else
                    {
                        turretCrosshair.GetComponent<Image>().color = turretBlockedColor;
                    }
                }
            }
        }
        Vector3 force = launchMode ? launchForce * throttle.value * Vector3.forward : cruiseForce * throttle.value * Vector3.forward;
        shipRigidbody.AddRelativeForce(force, ForceMode.Acceleration);

        if (autoStabilizeRot && (turretControl || joystickDirection == Vector3.zero))
        {
            float torqueCorrectionX = -xRotationPID.GetOutput(shipRigidbody.angularVelocity.x, Time.deltaTime);
            float torqueCorrectionY = -yRotationPID.GetOutput(shipRigidbody.angularVelocity.y, Time.deltaTime);
            float torqueCorrectionZ = -zRotationPID.GetOutput(shipRigidbody.angularVelocity.z, Time.deltaTime);
            shipRigidbody.AddTorque(new Vector3(torqueCorrectionX, torqueCorrectionY, torqueCorrectionZ) * rotationForce, ForceMode.Acceleration);
        }
        if (autoStabilizePos && throttle.value == 0 && joystickDirection == Vector3.zero)
        {
            Vector3 localVelocity = shipRigidbody.transform.InverseTransformDirection(shipRigidbody.velocity);
            float forceCorrectionX = -xRotationPID.GetOutput(localVelocity.x, Time.deltaTime);
            float forceCorrectionY = -yRotationPID.GetOutput(localVelocity.y, Time.deltaTime);
            float forceCorrectionZ = -zRotationPID.GetOutput(localVelocity.z, Time.deltaTime);
            shipRigidbody.AddRelativeForce(new Vector3(forceCorrectionX, forceCorrectionY, forceCorrectionZ) * translationForce, ForceMode.Acceleration);
        }
    }

    private void StartTurretFire()
    {
        if (turretControl)
        {
            triggerHeld = true;
            crosshair.color = triggerColor;
            for (int i = 0; i < turrets.Length; i++)
            {
                Turret turret = turrets[i];
                if (!Physics.Linecast(turret.firePoint.position, turret.firePoint.position + turret.firePoint.forward * 5, ~turret.ignoreLayers))
                {
                    turret.fire = true;
                }
            }
        }
    }

    private void StopTurretFire()
    {
        if (turretControl)
        {
            triggerHeld = false;
            for (int i = 0; i < turrets.Length; i++)
            {
                turrets[i].fire = false;
            }
        }
    }

    private void FireTorpedo()
    {
        if (torpedoBayDoorOpen)
        {
            Debug.Log("Fire Torpedo");
            foreach(TorpedoPoint torpedoPoint in torpedoPoints)
            {
                if (torpedoPoint.hasTorpedo)
                {
                    torpedoPoint.LaunchTorpedo(target);
                    break;
                }
            }
        }
    }

    private void ToggleTurretControl()
    {
        turretControl = !turretControl;
        turretControlUI.gameObject.SetActive(turretControl);
        for (int i = 0; i < turrets.Length; i++)
        {
            turrets[i].manual = turretControl;
        }
    }

    private void OnSwitchToggle(int index, int state)
    {
        switch (index)
        {
            case 0: // Launch
                launchMode = state == 1;
                break;
            case 1: // Torpedo Bay
                if (torpedoBayDoorOpen)
                {
                    torpedoBayDoorOpen = false;
                    bayDoorAnimation.Play("CloseTorpedoBay");
                }
                else
                {
                    torpedoBayDoorOpen = true;
                    bayDoorAnimation.Play("OpenTorpedoBay");
                }
                break;
            case 2: // Stabilize Rot
                autoStabilizeRot = state == 1;
                break;
            case 3: // Stabilize Pos
                autoStabilizePos = state == 1;
                break;
            case 4: // Constant Speed

                break;
            case 5: // Translation Control
                translationMode = state;
                break;
        }
    }
}
