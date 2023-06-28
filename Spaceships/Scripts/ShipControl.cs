using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpaceStuff;

[RequireComponent(typeof(Ship))]
public class ShipControl : MonoBehaviour
{
    Ship ship;
    [SerializeField] Camera pilotCamera;

    [SerializeField] Joystick joystick;
    [SerializeField] Throttle throttle;
    [SerializeField] Switch[] shipSwitches;
    [SerializeField] float rotationForce = 1000;
    [HideInInspector] bool launchMode = false;
    [SerializeField] float cruiseForce = 1000;
    [SerializeField] float launchForce = 10000;
    [SerializeField] float translationForce = 750;
    bool turretControl = false;
    bool autoStabilizeRot = false;
    bool autoStabilizePos = false;

    [SerializeField] Transform cockpit;

    [SerializeField] LayerMask targetLayers;
    [SerializeField] Image mainCrosshair;
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
    PIDController rotationPID;
    PIDController translationPID;

    [SerializeField] Transform targetLockUI;
    Transform selectedTarget;
    Transform lockedTarget;

    void Start()
    {
        ship = GetComponent<Ship>();

        rotationPID = new PIDController(P, I, D);
        translationPID = new PIDController(P, I, D);
    }

    private void OnEnable()
    {
        joystick.TriggerDown += StartTurretFire;
        joystick.TriggerUp += StopTurretFire;
        joystick.ToggleSwitch += ToggleTurretControl;
        joystick.ReleaseButton += FireTorpedo;
        joystick.LockButton += LockTarget;

        foreach (Switch _switch in shipSwitches)
        {
            _switch.OnSwitchToggle += OnSwitchToggle;
        }
    }

    private void OnDisable()
    {
        joystick.TriggerDown -= StartTurretFire;
        joystick.TriggerUp -= StopTurretFire;
        joystick.ToggleSwitch -= ToggleTurretControl;
        joystick.ReleaseButton -= FireTorpedo;
        joystick.LockButton -= LockTarget;

        foreach (Switch _switch in shipSwitches)
        {
            _switch.OnSwitchToggle -= OnSwitchToggle;
        }
    }

    void LateUpdate()
    {
        Vector3 joystickDirection = Vector3.zero;
        if (ship.pilot != null)
        {
            joystickDirection = joystick.direction;
            if (translationMode != 1)
            {
                if (joystickDirection.x != 0 && joystickDirection.z != 0)
                {
                    switch (translationMode)
                    {
                        case 0: // Vertical
                            ship.physicsHandler.AddRelativeForce(new Vector3d(-joystickDirection.z, -joystickDirection.x, 0) * translationForce, ForceMode.Acceleration);
                            break;
                        case 2: // Horizontal
                            ship.physicsHandler.AddRelativeForce(new Vector3d(-joystickDirection.z, 0, joystickDirection.x) * translationForce, ForceMode.Acceleration);
                            break;
                    }
                }
            }
            else
            {
                if (!turretControl)
                    ship.physicsHandler.AddRelativeTorque(joystickDirection.ToVector3d() * rotationForce, ForceMode.Acceleration);
            }

            if (turretControl)
            {
                mainCrosshair.transform.localPosition = new Vector2(-joystick.direction.z * ship.combatUI.rect.width * 0.5f, -joystick.direction.x * ship.combatUI.rect.height * 0.5f);
                Vector3 crosshairDirection = mainCrosshair.transform.position - ship.combatUI.position;

                if (Physics.Raycast(mainCrosshair.transform.position, crosshairDirection, out RaycastHit hit, Mathf.Infinity, targetLayers))
                {
                    if (hit.transform.HasTag("RadarTarget"))
                    {
                        selectedTarget = hit.transform;
                    }
                    Debug.DrawLine(mainCrosshair.transform.position, hit.point, Color.green, 0.1f);
                    if (!triggerHeld)
                        mainCrosshair.color = hoverColor;
                    for (int i = 0; i < ship.turrets.Length; i++)
                    {
                        Turret turret = ship.turrets[i];
                        turret.targetDirection = hit.point - turret.firePoint.position;
                        Transform turretCrosshair = ship.combatUI.GetChild(i + 1);
                        Vector3 turretHitPoint;
                        if (Physics.Raycast(turret.firePoint.position, turret.firePoint.forward, out RaycastHit turretHit, Mathf.Infinity, ~turret.ignoreLayers))
                        {
                            if (turretHit.transform != transform)
                                turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                            else
                                turretCrosshair.GetComponent<Image>().color = turretBlockedColor;
                            turretHitPoint = turretHit.point;
                        }
                        else
                        {
                            turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                            turretHitPoint = turret.firePoint.position + turret.firePoint.forward * 500;
                        }

                        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ship.combatUI, pilotCamera.WorldToScreenPoint(turretHitPoint), pilotCamera, out Vector3 turretCrosshairPos))
                        {
                            turretCrosshair.position = turretCrosshairPos;
                        }
                    }
                }
                else
                {
                    selectedTarget = null;
                    if (!triggerHeld)
                        mainCrosshair.color = normalColor;
                    for (int i = 0; i < ship.turrets.Length; i++)
                    {
                        Turret turret = ship.turrets[i];
                        turret.targetDirection = crosshairDirection;
                        Transform turretCrosshair = ship.combatUI.GetChild(i + 1);
                        Vector3 turretAimPosition = turret.firePoint.position + turret.firePoint.forward * 500;

                        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ship.combatUI, pilotCamera.WorldToScreenPoint(turretAimPosition), pilotCamera, out Vector3 turretCrosshairPos))
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

            if(lockedTarget != null)
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ship.combatUI, pilotCamera.WorldToScreenPoint(lockedTarget.position), pilotCamera, out Vector3 crosshairPosition))
                {
                    targetLockUI.position = crosshairPosition;
                }
                targetLockUI.rotation = Quaternion.LookRotation(pilotCamera.transform.position - targetLockUI.position, FlatCamera.instance.transform.up);
            }
        }
        
        Vector3d force = launchMode ? launchForce * throttle.value * Vector3d.forward : cruiseForce * throttle.value * Vector3d.forward;
        ship.physicsHandler.AddRelativeForce(force, ForceMode.Acceleration);

        if (autoStabilizeRot && (turretControl || joystickDirection == Vector3.zero))
        {
            float torqueCorrectionX = Mathf.Clamp(-rotationPID.GetOutput(ship.physicsHandler.attachedRigidbody.angularVelocity.x, Time.deltaTime), -rotationForce, rotationForce);
            float torqueCorrectionY = Mathf.Clamp(-rotationPID.GetOutput(ship.physicsHandler.attachedRigidbody.angularVelocity.y, Time.deltaTime), -rotationForce, rotationForce);
            float torqueCorrectionZ = Mathf.Clamp(-rotationPID.GetOutput(ship.physicsHandler.attachedRigidbody.angularVelocity.z, Time.deltaTime), -rotationForce, rotationForce);
            ship.physicsHandler.AddTorque(new Vector3d(torqueCorrectionX, torqueCorrectionY, torqueCorrectionZ) * rotationForce, ForceMode.Acceleration);
        }
        if (autoStabilizePos && throttle.value == 0 && joystickDirection == Vector3.zero)
        {
            float forceCorrectionX = Mathf.Clamp(-translationPID.GetOutput((float)ship.physicsHandler.velocity.x, Time.deltaTime), -translationForce, translationForce);
            float forceCorrectionY = Mathf.Clamp(-translationPID.GetOutput((float)ship.physicsHandler.velocity.y, Time.deltaTime), -translationForce, translationForce);
            float forceCorrectionZ = Mathf.Clamp(-translationPID.GetOutput((float)ship.physicsHandler.velocity.z, Time.deltaTime), -translationForce, translationForce);
            ship.physicsHandler.AddForce(new Vector3d(forceCorrectionX, forceCorrectionY, forceCorrectionZ) * translationForce, ForceMode.Acceleration);
        }
    }

    private void StartTurretFire()
    {
        if (turretControl)
        {
            triggerHeld = true;
            mainCrosshair.color = triggerColor;
            for (int i = 0; i < ship.turrets.Length; i++)
            {
                Turret turret = ship.turrets[i];
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
            for (int i = 0; i < ship.turrets.Length; i++)
            {
                ship.turrets[i].fire = false;
            }
        }
    }

    private void ToggleTurretControl()
    {
        turretControl = !turretControl;
        ship.combatUI.gameObject.SetActive(turretControl);
        for (int i = 0; i < ship.turrets.Length; i++)
        {
            ship.turrets[i].manual = turretControl;
        }
    }

    private void FireTorpedo()
    {
        if (torpedoBayDoorOpen)
        {
            Debug.Log("Fire Torpedo");
            for(int i = 0; i < torpedoPoints.Length; i++)
            {
                if (torpedoPoints[i].hasTorpedo)
                {
                    torpedoPoints[i].LaunchTorpedo(lockedTarget, i);
                    ship.UpdateTorpedoUI(i, false);
                    break;
                }
            }
        }
    }

    private void LockTarget()
    {
        if (turretControl)
        {
            lockedTarget = selectedTarget;
            targetLockUI.gameObject.SetActive(lockedTarget != null);
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
