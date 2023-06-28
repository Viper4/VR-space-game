using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using TMPro;
using UnityEngine.UI;

public class Ship : MonoBehaviour
{
    public Transform pilot;
    Dictionary<Transform, Transform> playerParentPair = new Dictionary<Transform, Transform>();
    public PhysicsHandler physicsHandler;
    [SerializeField] Transform[] turretPoints;
    [HideInInspector] public Turret[] turrets;
    [SerializeField] GameObject turretCrosshairPrefab;
    public RectTransform combatUI;

    [SerializeField] Color baseUIColor;
    [SerializeField] Transform HUDPivot;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] Image[] torpedoIcons;
    [SerializeField] Slider ammoSlider;
    [SerializeField] Slider fuelSlider;
    [SerializeField] Slider healthSlider;

    void Start()
    {
        turrets = new Turret[turretPoints.Length];
        for (int i = 0; i < turretPoints.Length; i++)
        {
            turrets[i] = turretPoints[i].GetChild(0).GetComponent<Turret>();
            Instantiate(turretCrosshairPrefab, combatUI);
        }
    }

    void LateUpdate()
    {
        if (pilot != null)
        {
            HUDPivot.SetPositionAndRotation(FlatCamera.instance.transform.position, FlatCamera.instance.transform.rotation);
        }

        speedText.text = "Speed: " + physicsHandler.velocity.magnitude;
    }

    public void EnterShip(Transform player)
    {
        if (!playerParentPair.ContainsKey(player))
        {
            playerParentPair.Add(player, player.parent);
            player.SetParent(transform);
        }
    }

    public void ExitShip(Transform player)
    {
        if (playerParentPair.TryGetValue(player, out Transform playerParent))
        {
            player.SetParent(playerParent);
            playerParentPair.Remove(player);
        }
    }

    public void SetPilot(Transform player)
    {
        pilot = player;
        if (pilot != null)
            HUDPivot.gameObject.SetActive(true);
        else
            HUDPivot.gameObject.SetActive(false);
    }

    public void UpdateTorpedoUI(int torpedoIndex, bool active)
    {
        torpedoIcons[torpedoIndex].color = active ? baseUIColor : Color.black;
    }
}
