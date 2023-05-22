using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipControl : MonoBehaviour
{
    [SerializeField] ShipJoystick joystick;
    [SerializeField] ShipThrottle throttle;
    Rigidbody shipRigidbody;
    [SerializeField] float rotationForce = 1000;
    [HideInInspector] bool launchMode = false;
    [SerializeField] float cruiseForce = 1000;
    [SerializeField] float launchForce = 10000;
    [SerializeField] float translationForce = 750;
    [SerializeField] Transform[] turretPoints;
    Turret[] turrets;
    bool turretControl = false;
    bool autoStabilize = false;

    [SerializeField] Transform pilot;
    Camera pilotCam;
    [SerializeField] GameObject hudUI;
    [SerializeField] RectTransform hudArea;
    [SerializeField] Transform hudPivot;
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

    bool translationMode = false;

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
            Instantiate(turretCrosshairPrefab, hudArea);
        }

        xRotationPID = new PIDController(P, I, D);
        yRotationPID = new PIDController(P, I, D);
        zRotationPID = new PIDController(P, I, D);
        //shipRigidbody.AddTorque(new Vector3(0.05f, 0.05f, 0.05f), ForceMode.VelocityChange);
        pilotCam = pilot.GetComponent<Camera>();
    }

    private void OnEnable()
    {
        joystick.JoystickTriggerDown += StartTurretFire;
        joystick.JoystickTriggerUp += StopTurretFire;
        joystick.JoystickRelease += FireTorpedo;
        joystick.JoystickToggle += ToggleTurretControl;
        joystick.JoystickPad += ToggleTranslationPad;

        throttle.SwitchToggle += OnSwitchToggle;
    }

    private void OnDisable()
    {
        joystick.JoystickTriggerDown -= StartTurretFire;
        joystick.JoystickTriggerUp -= StopTurretFire;
        joystick.JoystickRelease -= FireTorpedo;
        joystick.JoystickToggle -= ToggleTurretControl;
        joystick.JoystickPad -= ToggleTranslationPad;

        throttle.SwitchToggle -= OnSwitchToggle;
    }

    void Update()
    {
        Vector3 rotateDirection = joystick.direction;
        if (translationMode)
        {
            if (joystick.padPosition != Vector2.zero)
            {
                rotateDirection = Vector3.zero;
                shipRigidbody.AddRelativeForce(joystick.padPosition * translationForce, ForceMode.Acceleration);
            }
        }

        if (!turretControl)
        {
            shipRigidbody.AddRelativeTorque(rotateDirection * rotationForce, ForceMode.Acceleration);
        }
        else
        {
            hudPivot.SetPositionAndRotation(pilot.position, pilot.rotation);
            hudPivot.localEulerAngles = new Vector3(hudPivot.localEulerAngles.x, hudPivot.localEulerAngles.y, 0);
            crosshair.transform.localPosition = new Vector2(-joystick.direction.z * hudArea.rect.width * 0.5f, -joystick.direction.x * hudArea.rect.height * 0.5f);
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
                    Transform turretCrosshair = hudArea.GetChild(i + 1);
                    Vector3 turretHitPoint = Physics.Raycast(turret.firePoint.position, turret.firePoint.forward, out RaycastHit turretHit, Mathf.Infinity) ? turretHit.point : turret.firePoint.position + turret.firePoint.forward * 500;
                    
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(hudArea, pilotCam.WorldToScreenPoint(turretHitPoint), pilotCam, out Vector3 turretCrosshairPos))
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
                    Transform turretCrosshair = hudArea.GetChild(i + 1);
                    Vector3 turretAimPosition = turret.firePoint.position + turret.firePoint.forward * 500;

                    if(RectTransformUtility.ScreenPointToWorldPointInRectangle(hudArea, pilotCam.WorldToScreenPoint(turretAimPosition), pilotCam, out Vector3 turretCrosshairPos))
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

        if (autoStabilize)
        {
            if(turretControl || rotateDirection == Vector3.zero)
            {
                float torqueCorrectionX = -xRotationPID.GetOutput(shipRigidbody.angularVelocity.x, Time.deltaTime);
                float torqueCorrectionY = -yRotationPID.GetOutput(shipRigidbody.angularVelocity.y, Time.deltaTime);
                float torqueCorrectionZ = -zRotationPID.GetOutput(shipRigidbody.angularVelocity.z, Time.deltaTime);
                shipRigidbody.AddTorque(new Vector3(torqueCorrectionX, torqueCorrectionY, torqueCorrectionZ) * rotationForce, ForceMode.Acceleration);
            }
            if(joystick.padPosition == Vector2.zero)
            {
                Vector3 localVelocity = shipRigidbody.transform.InverseTransformDirection(shipRigidbody.velocity);
                float forceCorrectionX = -xRotationPID.GetOutput(localVelocity.x, Time.deltaTime);
                float forceCorrectionY = -xRotationPID.GetOutput(localVelocity.y, Time.deltaTime);
                shipRigidbody.AddRelativeForce(new Vector3(forceCorrectionX, forceCorrectionY) * translationForce, ForceMode.Acceleration);
            }
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
        hudUI.SetActive(turretControl);
        for (int i = 0; i < turrets.Length; i++)
        {
            turrets[i].manual = turretControl;
        }
    }

    private void ToggleTranslationPad()
    {
        translationMode = !translationMode;
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
            case 2: // Auto Stabilize
                autoStabilize = state == 1;
                break;
        }
    }
}
