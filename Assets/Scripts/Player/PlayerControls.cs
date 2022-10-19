using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float moveSpeed;
    public List<Globals.MODIFIERS> moveSpeedModifiers = new List<Globals.MODIFIERS>();
    public float baseMoveSpeed = 8.0f;
    public float jumpHeight = 3.0f;
    public float mouseSensitivity = 100f;
    public float gravity = -9.81f;
    public float gravityMultiplier = 5f;
    public CharacterController controller;
    public static PlayerControls instance;
    public bool processCamera = true;

    Transform playerHead, mainCam, playerLeg;
    float rotationX;
    float mouseSensMultiplier = 5;
    Vector3 moveDir, velocity, knockBack;
    public bool isGrounded = false;
    bool isSprinting = false;

    float x, z;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //To make sure the mouse don't anyhow move
        playerLeg = transform.Find("Body").Find("Leg");
        playerHead = transform.Find("Head");
        mainCam = playerHead.Find("Main Camera");
        instance = this;
    }

    private void FixedUpdate()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0) // if player is on the ground and is currently falling
        {
            velocity.y = -2f; // reset velocity
        }
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity * gravityMultiplier); // increase y velocity to jump
    }


    public void AddKnockback(Vector3 direction, float force)
    {
        knockBack = (direction * force + Vector3.up) * 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!GlobalSettings.instance.isInGameSettingsOpen && !GameplayLoop.instance.GameInProgress)
            {
                PlayerStats.instance.ToggleMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PlayerStats.instance.Menu.activeInHierarchy)
            {
                PlayerStats.instance.ToggleMenu();
            }
            {
                GlobalSettings.instance.ToggleIngameSettings();
            }
        }

        if (knockBack.magnitude > 0.2) controller.Move(knockBack * Time.deltaTime);
        // consumes the impact energy each cycle:
        knockBack = Vector3.Lerp(knockBack, Vector3.zero, 5 * Time.deltaTime);

        if (!PlayerStats.instance.HasEffect(StatusEffect.EffectType.STUNNED))
        {
            if (processCamera)
            {
                ProcessCamera();
            }
            ProcessMovement();
        }
        UpdateGravity();
    }

    void ProcessMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // only allow player to jump when they are grounded
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.E) && GameplayLoop.instance.GameInProgress)
        {
            PlayerStats.instance.UseSkill();
        }
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        
        x = Input.GetAxisRaw("Horizontal"); // get left right movement
        z = Input.GetAxisRaw("Vertical"); // get front back movement

        //Checks if the player is moving,
        bool hasHorizontalInput = !Mathf.Approximately(x, 0f);
        bool hasVerticalInput = !Mathf.Approximately(z, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        moveDir = (transform.right * x + transform.forward * z).normalized; // movement vector normalized (so that diagonal speed won't be faster)

        float result = 1;
        foreach (Globals.MODIFIERS mod in moveSpeedModifiers)
        {
            result *= mod.value;
        }
        moveSpeed = baseMoveSpeed * result;
        if (controller.enabled)
        {
            controller.Move((isSprinting ? moveSpeed * 1.5f : moveSpeed) * Time.deltaTime * moveDir); // move player
        }
    }

    public IEnumerator Skill3()
    {
        jumpHeight = 15;
        Jump();
        yield return new WaitUntil(() => velocity.y < 0);
        gravityMultiplier = 0.1f;
        yield return new WaitUntil(() => isGrounded == true);
        gravityMultiplier = 5;
        jumpHeight = 3;
        yield return null;
    }

    void ProcessCamera()
    {
        //Get Mouse Sens from settings
        mouseSensitivity = GlobalSettings.instance.mouseSensitivity;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * mouseSensMultiplier * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * mouseSensMultiplier * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // locking camera vertical angle to -90 and 90 deg

        mainCam.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void UpdateGravity()
    {
        velocity.y += gravity * Time.deltaTime * gravityMultiplier; // update player velocity
        controller.Move(velocity * Time.deltaTime); // apply fall to player
    }
}
