using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public bool active = true;
    [SerializeField] float[] radarRanges;
    [SerializeField] int rangeIndex = 0;
    [SerializeField] LayerMask ignoreLayers;
    [SerializeField] Transform radarOrigin;

    [SerializeField] GameObject display;
    [SerializeField] GameObject shipIcon;
    [SerializeField] Color friendlyShipColor;
    [SerializeField] Color hostileShipColor;

    [SerializeField] GameObject pointIcon;
    [SerializeField] Color projectileColor;
    [SerializeField] Color celestialBodyColor;

    [SerializeField] Transform displayPulse;
    [SerializeField] float pulseSpeed = 0.5f;

    [SerializeField] Switch radarSwitch;
    Dictionary<int, RadarIcon> instanceIDIconPair = new Dictionary<int, RadarIcon>();

    private void OnEnable()
    {
        radarSwitch.SwitchToggle += ToggleSwitch;
    }

    private void OnDisable()
    {
        radarSwitch.SwitchToggle -= ToggleSwitch;
    }

    void Start()
    {
        
    }

    private void Update()
    {
        if(active)
        {
            float pulseAddition = Time.deltaTime * pulseSpeed;
            displayPulse.localScale = CustomExtensions.WrapClamp(new Vector3(displayPulse.localScale.x + pulseAddition, displayPulse.localScale.y + pulseAddition, displayPulse.localScale.z + pulseAddition), 0, 1);
        }
        else
        {

        }
    }

    void FixedUpdate()
    {
        if(active)
        {
            Collider[] overlappingColliders = Physics.OverlapSphere(radarOrigin.position, radarRanges[rangeIndex], ~ignoreLayers);
            foreach(Collider collider in overlappingColliders)
            {
                if(collider.transform == radarOrigin)
                {
                    if (instanceIDIconPair.TryGetValue(radarOrigin.GetInstanceID(), out RadarIcon icon))
                    {
                        icon.UpdateIcon(display.transform.position, radarOrigin.rotation);
                    }
                    else
                    {
                        RadarIcon newIcon = Instantiate(shipIcon, transform).transform.GetComponent<RadarIcon>();
                        int colliderID = collider.transform.GetInstanceID();
                        newIcon.CreateIcon(this, display.transform, colliderID, display.transform.position, radarOrigin.rotation);
                        instanceIDIconPair.Add(colliderID, newIcon);
                    }
                }
                else
                {
                    if (Physics.Raycast(radarOrigin.position, collider.transform.position - radarOrigin.position, out RaycastHit hit, radarRanges[rangeIndex], ~ignoreLayers))
                    {
                        if (collider.transform.HasTag("Target"))
                        {
                            Vector3 radarPosition = display.transform.position + ((collider.transform.position - radarOrigin.position).normalized * (Vector3.Distance(radarOrigin.position, collider.transform.position) / radarRanges[rangeIndex] * 0.5f));
                            if (instanceIDIconPair.TryGetValue(collider.transform.GetInstanceID(), out RadarIcon icon))
                            {
                                icon.UpdateIcon(radarPosition, collider.transform.rotation);
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
                                newIcon.CreateIcon(this, display.transform, colliderID, radarPosition, collider.transform.rotation);
                                instanceIDIconPair.Add(colliderID, newIcon);
                            }
                        }
                    }
                }
            }
        }
    }

    void ToggleSwitch(int index, int state)
    {
        rangeIndex = state;
        switch (state)
        {
            case 0:
                display.SetActive(false);
                active = false;
                break;
            case 1:
                display.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                display.SetActive(true);
                active = true;
                break;
            case 2:
                display.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case 3:
                display.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                break;
            case 4:
                display.transform.localScale = new Vector3(1, 1, 1);
                break;
        }
    }

    public void RemoveIcon(int id)
    {
        instanceIDIconPair.Remove(id);
    }
}
