using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public CharacterController controller;
    public LayerMask hitMask;
    public Transform gunMuzzle;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    Vector3 velocity;

    [Header("Look")]
    public float mouseSensitivity = 100f;
    float xRot = 0f;

    [Header("Weapons")]
    public enum WeaponType { AR, Sniper }
    public WeaponType currentWeapon = WeaponType.AR;
    public float arDamage = 20f;
    public float arFireRate = 0.12f;
    public float sniperDamage = 90f;
    public float sniperFireRate = 1.2f;
    float nextFireTime = 0f;

    [Header("Aim Assist")]
    public bool aimAssistEnabled = true;
    public float aimAssistRadius = 0.08f; // viewport radius
    public float aimAssistRange = 80f;
    public float aimAssistStrength = 12f;
    public bool autoShootOnAssist = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode switchWeaponKey = KeyCode.Q;

    [Header("State")]
    public bool isLocalPlayer = true; // for future netcode
    public string playerName = "Player";
    public int teamId = 0; // 0 = FFA, 1/2 = teams

    void Start()
    {
        if (isLocalPlayer)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleLook();
        HandleMovement();
        HandleWeaponSwitch();
        HandleShooting();
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = moveSpeed;

        if (Input.GetKey(sprintKey))
            speed *= sprintMultiplier;

        controller.Move(move.normalized * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(jumpKey) && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -89f, 89f);

        playerCam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(switchWeaponKey))
        {
            currentWeapon = currentWeapon == WeaponType.AR ? WeaponType.Sniper : WeaponType.AR;
        }
    }

    void HandleShooting()
    {
        bool isControllerOrMobile = IsControllerOrMobileInput();

        Ray aimRay = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Transform target = null;

        if (aimAssistEnabled && isControllerOrMobile)
            target = GetAimAssistTarget(ref aimRay);

        bool wantsToShoot = Input.GetButton("Fire1");
        if (autoShootOnAssist && isControllerOrMobile && target != null)
            wantsToShoot = true;

        if (!wantsToShoot || Time.time < nextFireTime)
            return;

        float dmg = currentWeapon == WeaponType.AR ? arDamage : sniperDamage;
        float fireRate = currentWeapon == WeaponType.AR ? arFireRate : sniperFireRate;
        nextFireTime = Time.time + fireRate;

        if (Physics.Raycast(aimRay, out RaycastHit hit, 1000f, hitMask))
        {
            Health hp = hit.collider.GetComponentInParent<Health>();
            if (hp != null)
                hp.TakeDamage(dmg, this);

            Debug.DrawLine(gunMuzzle.position, hit.point, Color.red, 0.2f);
        }
    }

    Transform GetAimAssistTarget(ref Ray aimRay)
    {
        Transform bestTarget = null;
        float bestScore = Mathf.Infinity;

        Collider[] hits = Physics.OverlapSphere(playerCam.transform.position, aimAssistRange);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player") && !col.CompareTag("Bot")) continue;
            if (col.transform == this.transform) continue;

            Vector3 screenPos = playerCam.WorldToViewportPoint(col.transform.position);
            if (screenPos.z < 0) continue;

            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 pos2D = new Vector2(screenPos.x, screenPos.y);
            float dist = Vector2.Distance(center, pos2D);

            if (dist < aimAssistRadius && dist < bestScore)
            {
                bestScore = dist;
                bestTarget = col.transform;
            }
        }

        if (bestTarget != null)
        {
            Vector3 dir = (bestTarget.position - playerCam.transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            playerCam.transform.rotation = Quaternion.Lerp(playerCam.transform.rotation, targetRot, aimAssistStrength * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, playerCam.transform.eulerAngles.y, 0);
            aimRay = new Ray(playerCam.transform.position, playerCam.transform.forward);
        }

        return bestTarget;
    }

    bool IsControllerOrMobileInput()
    {
        // Super basic detection; later we’ll wire proper input system
        if (Application.isMobilePlatform) return true;
        if (Mathf.Abs(Input.GetAxis("Joystick X")) > 0.1f || Mathf.Abs(Input.GetAxis("Joystick Y")) > 0.1f)
            return true;
        return false;
    }
}
