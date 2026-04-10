using UnityEngine;

/// <summary>
/// 무기 시스템이 플레이어 카메라 상하 반동을 전달할 때 사용하는 인터페이스.
/// </summary>
public interface ILookRecoilReceiver
{
    void AddPitchRecoil(float amount);
}

/// <summary>
/// STEP 1: 초보자용 3D FPS 플레이어 컨트롤러
/// 구현 기능:
/// 1) WASD 이동
/// 2) 마우스 시점 회전
/// 3) 점프
/// 4) 달리기(Left Shift)
/// 5) 바닥 판정
/// 6) 카메라 흔들림 최소화
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour, ILookRecoilReceiver
{
    [Header("=== 이동 설정 ===")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("=== 마우스 시점 설정 ===")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("=== 바닥 판정 설정 ===")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundMask;

    [Header("=== 연결할 오브젝트 ===")]
    [SerializeField] private Camera playerCamera;

    [Header("=== 반동 설정(총 시스템 연동) ===")]
    [Tooltip("총 반동이 0으로 돌아오는 속도")]
    [SerializeField] private float recoilRecoverSpeed = 16f;
    [Tooltip("반동 즉시 킥 배율")]
    [SerializeField] private float recoilKickMultiplier = 1f;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation;

    // 반동용 스프링 값
    private float recoilPitchCurrent;
    private float recoilPitchTarget;
    private float recoilPitchVelocity;

    private bool isGrounded;
    private Vector3 cameraInitialLocalPosition;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera != null)
        {
            cameraInitialLocalPosition = playerCamera.transform.localPosition;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckGround();
        HandleMouseLook();
        HandleMovement();
    }

    /// <summary>
    /// WeaponBase에서 호출: 위로 드는 반동 값을 추가.
    /// </summary>
    public void AddPitchRecoil(float amount)
    {
        recoilPitchTarget += amount * recoilKickMultiplier;
    }

    private void CheckGround()
    {
        Vector3 checkPosition = groundCheck != null
            ? groundCheck.position
            : transform.position + Vector3.down * (controller.height * 0.5f - controller.radius + 0.05f);

        isGrounded = Physics.CheckSphere(checkPosition, groundDistance, groundMask);

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;

        // 반동 복귀를 2단계(타겟 복귀 + 스무딩)로 처리해 더 자연스럽게 보이도록 함
        recoilPitchTarget = Mathf.Lerp(recoilPitchTarget, 0f, recoilRecoverSpeed * Time.deltaTime);
        recoilPitchCurrent = Mathf.SmoothDamp(
            recoilPitchCurrent,
            recoilPitchTarget,
            ref recoilPitchVelocity,
            0.04f,
            Mathf.Infinity,
            Time.deltaTime);

        float finalPitch = Mathf.Clamp(xRotation - recoilPitchCurrent, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
            playerCamera.transform.localPosition = cameraInitialLocalPosition;
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDirection = transform.right * inputX + transform.forward * inputZ;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 checkPosition;
        if (groundCheck != null)
        {
            checkPosition = groundCheck.position;
        }
        else
        {
            CharacterController cc = GetComponent<CharacterController>();
            checkPosition = cc != null
                ? transform.position + Vector3.down * (cc.height * 0.5f - cc.radius + 0.05f)
                : transform.position + Vector3.down * 0.9f;
        }

        Gizmos.DrawWireSphere(checkPosition, groundDistance);
    }
}
