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
    [SerializeField] Transform radarOrigin;


    [SerializeField] GameObject hologram;
    [SerializeField] GameObject shipIcon;
    [SerializeField] Color friendlyShipColor;
    [SerializeField] Color hostileShipColor;

    [SerializeField] GameObject pointIcon;
    [SerializeField] Color projectileColor;
    [SerializeField] Color celestialBodyColor;

    [SerializeField] Transform displayPulse;
    [SerializeField] float pulseSpeed = 0.5f;

    bool displayOnHUD = false;
    [SerializeField] Transform HUDPivot;
    [SerializeField] GameObject radarHUD;

    [SerializeField] Switch toggleHUDSwitch;
    [SerializeField] Switch radarSwitch;
    Dictionary<int, RadarIcon> instanceIDIconPair = new Dictionary<int, RadarIcon>();

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
            Collider[] overlappingColliders = Physics.OverlapSphere(radarOrigin.position, radarRanges[rangeIndex], ~ignoreLayers);
            foreach (Collider collider in overlappingColliders)
            {
                if(collider.transform != radarOrigin)
                {
                    if (Physics.Raycast(radarOrigin.position, collider.transform.position - radarOrigin.position, out RaycastHit hit, radarRanges[rangeIndex], ~ignoreLayers))
                    {
                        if (collider.transform.HasTag("Target"))
                        {
                            float distance = Vector3.Distance(radarOrigin.position, collider.transform.position);
                            Vector3 radarPosition = displayOnHUD ? HUDPivot.position + ((collider.transform.position - radarOrigin.position).normalized * (distance / radarRanges[rangeIndex] * 0.5f)) : hologram.transform.position + ((collider.transform.position - radarOrigin.position).normalized * (distance / radarRanges[rangeIndex] * 0.5f));
                            if (instanceIDIconPair.TryGetValue(collider.transform.GetInstanceID(), out RadarIcon icon))
                            {
                                icon.UpdateIcon(radarPosition, collider.transform.rotation, collider.transform.name + "\n" + CustomMethods.DistanceToFormattedString(distance, 2));
                            }
                            else
                            {
                                RadarIcon newIcon;
                                switch (collider.transform.tag)
                                {
                                    case "Ship":
                                        newIcon = Instantiate(shipIcon, transform).GetComponent<RadarIcon>();

                                        break;
                                    case "Projectile":
                                        newIcon = Instantiate(pointIcon, transform).GetComponent<RadarIcon>();
                                        newIcon.GetComponent<MeshRenderer>().sharedMaterial.color = projectileColor;
                                        break;
                                    case "CelestialBody":
                                        newIcon = Instantiate(pointIcon, transform).GetComponent<RadarIcon>();
                                        newIcon.GetComponent<MeshRenderer>().sharedMaterial.color = celestialBodyColor;
                                        float iconRadius = collider.GetComponent<CelestialBodyGenerator>().shapeSettings.radius / radarRanges[rangeIndex];
                                        newIcon.transform.localScale = new Vector3(iconRadius, iconRadius, iconRadius);
                                        break;
                                    default:
                                        newIcon = Instantiate(pointIcon, transform).GetComponent<RadarIcon>();
                                        break;
                                }

                                int colliderID = collider.transform.GetInstanceID();
                                newIcon.CreateIcon(this, colliderID, radarPosition, collider.transform.rotation, collider.transform.name + "\n" + CustomMethods.DistanceToFormattedString(distance, 2));
                                instanceIDIconPair.Add(colliderID, newIcon);
                            }
                        }
                    }
                }
                else if (!displayOnHUD)
                {
                    if (instanceIDIconPair.TryGetValue(radarOrigin.GetInstanceID(), out RadarIcon icon))
                    {
                        icon.UpdateIcon(hologram.transform.position, radarOrigin.rotation);
                    }
                    else
                    {
                        RadarIcon newIcon = Instantiate(shipIcon, transform).GetComponent<RadarIcon>();
                        int colliderID = collider.transform.GetInstanceID();
                        newIcon.CreateIcon(this, colliderID, hologram.transform.position, radarOrigin.rotation);
                        instanceIDIconPair.Add(colliderID, newIcon);
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
                    hologram.SetActive(!displayOnHUD);
                    radarHUD.SetActive(displayOnHUD);
                }
                break;
            case 1:
                rangeIndex = state;
                switch (state)
                {
                    case 0:
                        hologram.SetActive(false);
                        radarHUD.SetActive(false);
                        active = false;
                        break;
                    case 1:
                        hologram.SetActive(!displayOnHUD);
                        radarHUD.SetActive(displayOnHUD);
                        active = true;
                        break;
                }
                break;
        }
    }

    public void RemoveIcon(int id)
    {
        instanceIDIconPair.Remove(id);
    }
}
