using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public class Radar : MonoBehaviour
{
    public bool active = true;
    [SerializeField] float[] radarRanges;
    [SerializeField] int rangeIndex = 0;
    [SerializeField] LayerMask ignoreLayers;
    [SerializeField] Transform ship;
    [SerializeField] Rigidbody shipRigidbody;
    [SerializeField] PhysicsHandler shipPhysicsHandler;

    [SerializeField] GameObject iconParent;
    [SerializeField] Transform hologram;

    [SerializeField] GameObject shipIcon;
    [SerializeField] Color friendlyShipColor;
    [SerializeField] Color friendlyShipEmission;
    [SerializeField] Color hostileShipColor;
    [SerializeField] Color hostileShipEmission;

    [SerializeField] GameObject pointIcon;
    [SerializeField] Color projectileColor;
    [SerializeField] Color projectileEmission;
    [SerializeField] Color celestialBodyColor;
    [SerializeField] Color celestialBodyEmission;

    [SerializeField] Transform displayPulse;
    [SerializeField] float pulseSpeed = 0.5f;

    bool displayOnHUD = false;
    [SerializeField] Transform HUDPivot;
    [SerializeField] GameObject HUDObjectParent;
    [SerializeField] float HUDDistanceMin = 1.5f;
    [SerializeField] float[] HUDDistanceMax;
    [SerializeField] GameObject HUDObject;

    [SerializeField] Switch toggleHUDSwitch;
    [SerializeField] Switch radarSwitch;
    Dictionary<int, RadarIcon> instanceIDIconPair = new Dictionary<int, RadarIcon>();
    Dictionary<int, HUDObject> instanceIDHUDPair = new Dictionary<int, HUDObject>();

    private void OnEnable()
    {
        toggleHUDSwitch.OnSwitchToggle += ToggleSwitch;
        radarSwitch.OnSwitchToggle += ToggleSwitch;
    }

    private void OnDisable()
    {
        toggleHUDSwitch.OnSwitchToggle -= ToggleSwitch;
        radarSwitch.OnSwitchToggle -= ToggleSwitch;
    }

    private void Update()
    {
        if(active && !displayOnHUD)
        {
            float pulseAddition = Time.deltaTime * pulseSpeed;
            displayPulse.localScale = CustomMethods.WrapClamp(new Vector3(displayPulse.localScale.x + pulseAddition, displayPulse.localScale.y + pulseAddition, displayPulse.localScale.z + pulseAddition), 0, 1);
        }
    }

    void FixedUpdate()
    {
        if(active)
        {
            Collider[] overlappingColliders = Physics.OverlapSphere(ship.position, radarRanges[rangeIndex], ~ignoreLayers);
            foreach (Collider collider in overlappingColliders)
            {
                if(collider.transform != ship)
                {
                    if(collider.transform.HasTag("RadarTarget"))
                    {
                        Vector3 direction;
                        double distance;
                        ScaledTransform scaledTransform;
                        if(collider.TryGetComponent(out scaledTransform))
                        {
                            distance = Vector3d.Distance(ship.position.ToVector3d(), scaledTransform.position);
                            direction = (scaledTransform.position.ToVector3() - ship.position).normalized;
                        }
                        else
                        {
                            distance = Vector3.Distance(ship.position, collider.transform.position);
                            direction = (collider.transform.position - ship.position).normalized;
                        }
                        if(distance < radarRanges[rangeIndex])
                        {
                            int instanceID = collider.transform.GetInstanceID();
                            if (displayOnHUD)
                            {
                                Bounds referenceBounds = collider.TryGetComponent<MeshRenderer>(out var colliderRenderer) ? colliderRenderer.bounds : new Bounds() { center = collider.transform.position, size = Vector3.zero };
                                foreach (Transform child in collider.transform)
                                {
                                    if(child.TryGetComponent<MeshRenderer>(out var childRenderer))
                                    {
                                        referenceBounds.Encapsulate(childRenderer.bounds);
                                    }
                                }
                                float HUDDistance = (float)(distance / radarRanges[rangeIndex] * (HUDDistanceMax[rangeIndex] - HUDDistanceMin) + HUDDistanceMin);
                                Vector3 hudPosition = HUDPivot.position + direction * HUDDistance;

                                double speed = 0;
                                double colliderVelocityInDirection = 0;
                                if (collider.TryGetComponent<PhysicsHandler>(out var physicsHandler) && physicsHandler.active)
                                {
                                    speed = physicsHandler.velocity.magnitude;
                                    colliderVelocityInDirection = Vector3d.Dot(physicsHandler.velocity, direction.ToVector3d());
                                }
                                else if (collider.TryGetComponent<Rigidbody>(out var colliderRigidbody) && !colliderRigidbody.isKinematic)
                                {
                                    speed = colliderRigidbody.velocity.magnitude;
                                    colliderVelocityInDirection = Vector3.Dot(colliderRigidbody.velocity, direction);
                                }

                                double shipVelocityInDirection = 0;
                                if (shipPhysicsHandler != null && shipPhysicsHandler.active)
                                {
                                    shipVelocityInDirection = Vector3d.Dot(shipPhysicsHandler.velocity, direction.ToVector3d());
                                }
                                else if (shipRigidbody != null && !shipRigidbody.isKinematic)
                                {
                                    shipVelocityInDirection = Vector3.Dot(shipRigidbody.velocity, direction);
                                }

                                string timeToArrive = colliderVelocityInDirection > shipVelocityInDirection ? "Never" : CustomMethods.SecondsToFormattedString(distance / (shipVelocityInDirection - colliderVelocityInDirection), 2);
                                string hudText = "<style=RadarHUD>" + collider.transform.name.ToUpper() + "</style>" +
                                    "\nDistance: " + CustomMethods.MetersToFormattedString(distance, 2) +
                                    "\nSpeed: " + CustomMethods.MetersToFormattedString(speed, 2, "/s") +
                                    "\nClosing Velocity: " + CustomMethods.MetersToFormattedString(colliderVelocityInDirection - shipVelocityInDirection, 2, "/s") +
                                    "\nETA: " + timeToArrive;

                                if (instanceIDHUDPair.TryGetValue(instanceID, out HUDObject hudObject))
                                {
                                    hudObject.UpdateObject(hudPosition, referenceBounds, hudText);
                                }
                                else
                                {
                                    HUDObject newHUDObject = Instantiate(HUDObject, HUDObjectParent.transform).GetComponent<HUDObject>();
                                    switch (collider.transform.tag)
                                    {
                                        case "Ship":
                                            newHUDObject.SetColor(friendlyShipColor);
                                            break;
                                        case "Projectile":
                                            newHUDObject.SetColor(projectileColor);
                                            break;
                                        case "CelestialBody":
                                            newHUDObject.SetColor(celestialBodyColor);
                                            break;
                                    }

                                    newHUDObject.Init(this, hudPosition, referenceBounds, instanceID, hudText);
                                    instanceIDHUDPair.Add(instanceID, newHUDObject);
                                }
                            }
                            else
                            {
                                Vector3 radarPosition = hologram.position + (direction * (float)(distance / radarRanges[rangeIndex] * 0.5));
                                if (instanceIDIconPair.TryGetValue(instanceID, out RadarIcon icon))
                                {
                                    icon.UpdateIcon(radarPosition, collider.transform.rotation, collider.transform.name + "\n" + CustomMethods.MetersToFormattedString(distance, 2));
                                }
                                else
                                {
                                    RadarIcon newIcon;
                                    Color iconColor = friendlyShipColor;
                                    Color iconEmission = friendlyShipEmission;
                                    switch (collider.transform.tag)
                                    {
                                        case "Ship":
                                            newIcon = Instantiate(shipIcon, iconParent.transform).GetComponent<RadarIcon>();
                                            iconColor = friendlyShipColor;
                                            iconEmission = friendlyShipEmission;
                                            break;
                                        case "Projectile":
                                            newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                            iconColor = projectileColor;
                                            iconEmission = projectileEmission;
                                            break;
                                        case "CelestialBody":
                                            newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                            float iconRadius = (float)scaledTransform.scale.x / radarRanges[rangeIndex];
                                            if (iconRadius < 0.01f)
                                                iconRadius = 0.01f;
                                            newIcon.model.localScale = new Vector3(iconRadius, iconRadius, iconRadius);
                                            iconColor = celestialBodyColor;
                                            iconEmission = celestialBodyEmission;
                                            break;
                                        default:
                                            newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                            break;
                                    }

                                    newIcon.Init(this, instanceID, radarPosition, collider.transform.rotation, iconColor, iconEmission, collider.transform.name + "\n" + CustomMethods.MetersToFormattedString(distance, 2));
                                    instanceIDIconPair.Add(instanceID, newIcon);
                                }
                            }
                        }
                    }
                }
                else if (!displayOnHUD)
                {
                    int originID = ship.GetInstanceID();
                    if (instanceIDIconPair.TryGetValue(originID, out RadarIcon icon))
                    {
                        icon.UpdateIcon(hologram.position, ship.rotation);
                    }
                    else
                    {
                        RadarIcon newIcon = Instantiate(shipIcon, iconParent.transform).GetComponent<RadarIcon>();
                        newIcon.Init(this, originID, hologram.position, ship.rotation, friendlyShipColor, friendlyShipEmission);
                        instanceIDIconPair.Add(originID, newIcon);
                    }
                }
            }
        }
    }

    void ToggleSwitch(int index, int state)
    {
        switch (index)
        {
            case 0:
                displayOnHUD = state == 1;
                if (active)
                {
                    iconParent.SetActive(!displayOnHUD);
                    HUDObjectParent.SetActive(displayOnHUD);
                }
                break;
            case 1:
                rangeIndex = state;
                switch (state)
                {
                    case 0:
                        iconParent.SetActive(false);
                        HUDObjectParent.SetActive(false);
                        active = false;
                        break;
                    case 1:
                        iconParent.SetActive(!displayOnHUD);
                        HUDObjectParent.SetActive(displayOnHUD);
                        active = true;
                        break;
                }
                break;
        }
    }

    public void RemoveIcon(int ID)
    {
        instanceIDIconPair.Remove(ID);
    }

    public void RemoveHUDObject(int ID)
    {
        instanceIDHUDPair.Remove(ID);
    }
}
