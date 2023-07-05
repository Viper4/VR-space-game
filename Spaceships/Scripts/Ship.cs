using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using TMPro;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Ship : MonoBehaviour
{
    public int ID;

    Player player;
    public bool isPilot;
    Transform flatCameraTransform;
    Dictionary<Transform, Transform> playerParentPair = new Dictionary<Transform, Transform>();
    public PhysicsHandler physicsHandler;
    [SerializeField] Transform[] turretPoints;
    [HideInInspector] public Turret[] turrets;
    [SerializeField] GameObject turretCrosshairPrefab;
    public RectTransform combatUI;

    [SerializeField] GameObject pilotUI;
    [SerializeField] Color baseUIColor;
    [SerializeField] Transform HUDPivot;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] Image[] torpedoIcons;
    [SerializeField] Slider ammoSlider;
    [SerializeField] Slider fuelSlider;
    [SerializeField] Slider healthSlider;
    [SerializeField] Transform velocityDirectionPivot;

    [SerializeField] Switch cockpitChair;
    [SerializeField] Color fadeColor;
    [SerializeField] float fadeTime = 0.1f;

    void Start()
    {
        player = Player.instance;
        flatCameraTransform = FlatCamera.instance.transform;

        turrets = new Turret[turretPoints.Length];
        for (int i = 0; i < turretPoints.Length; i++)
        {
            turrets[i] = turretPoints[i].GetChild(0).GetComponent<Turret>();
            Instantiate(turretCrosshairPrefab, combatUI);
        }
    }

    private void OnEnable()
    {
        cockpitChair.OnSwitchToggle += SetPilot;
    }
    private void OnDisable()
    {
        cockpitChair.OnSwitchToggle -= SetPilot;
    }

    void LateUpdate()
    {
        if (isPilot)
        {
            HUDPivot.SetPositionAndRotation(flatCameraTransform.position, flatCameraTransform.rotation);
        }

        speedText.text = "Speed: " + CustomMethods.SpeedToFormattedString(physicsHandler.velocity.magnitude, 2);
        if(physicsHandler.velocity != Vector3d.zero)
            velocityDirectionPivot.rotation = Quaternion.LookRotation(physicsHandler.velocity.ToVector3(), transform.up);
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

    public void SetPilot(int index, int state)
    {
        isPilot = state == 1;
        pilotUI.SetActive(isPilot);
        StartCoroutine(AlignPilot());
    }

    IEnumerator AlignPilot()
    {
        SteamVR_Fade.Start(fadeColor, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        /*Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
        player.trackingOriginTransform.position = cockpitChair.transform.position + playerFeetOffset;

        if (player.leftHand.currentAttachedObjectInfo.HasValue)
            player.leftHand.ResetAttachedTransform(player.leftHand.currentAttachedObjectInfo.Value);
        if (player.rightHand.currentAttachedObjectInfo.HasValue)
            player.rightHand.ResetAttachedTransform(player.rightHand.currentAttachedObjectInfo.Value);

        player.transform.rotation = cockpitChair.transform.rotation;*/
    }

    public void UpdateTorpedoUI(int torpedoIndex, bool active)
    {
        torpedoIcons[torpedoIndex].color = active ? baseUIColor : Color.black;
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.transform.name + ": " + collision.collider.name + " layer " + collision.collider.gameObject.layer + " " + collision.body.name);
    }
}
