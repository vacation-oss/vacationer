using UnityEngine;

/// <summary>
/// 1단계용 기본 3D FPS 플레이어 컨트롤러.
/// - WASD 이동
/// - 마우스 시점 회전
/// - 점프
/// - 달리기
/// - 바닥 판정
/// - 카메라 흔들림 최소화(카메라 위치 고정)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("기본 이동 속도")]
    public float walkSpeed = 4f;

    [Tooltip("달리기 이동 속도")]
    public float runSpeed = 7f;

    [Tooltip("점프 높이(값이 높을수록 더 높이 점프)")]
    public float jumpHeight = 1.5f;

    [Tooltip("중력 값(음수 권장)")]
    public float gravity = -20f;

    [Header("Mouse Look")]
    [Tooltip("마우스 감도")]
    public float mouseSensitivity = 2f;

    [Tooltip("위/아래 시점 회전 제한 각도")]
    public float maxLookAngle = 80f;

    [Header("Ground Check")]
    [Tooltip("땅 체크용 위치(플레이어 발 근처 빈 오브젝트)")]
    public Transform groundCheck;

    [Tooltip("땅 체크 반경")]
    public float groundDistance = 0.25f;

    [Tooltip("땅으로 사용할 레이어")]
    public LayerMask groundMask;

    [Header("References")]
    [Tooltip("플레이어 자식 카메라")]
    public Camera playerCamera;

    private CharacterController controller;

    // y축(위/아래) 이동 속도 저장용
    private float verticalVelocity;

    // 카메라의 상하 회전 누적값
    private float xRotation;

    // 바닥 여부
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // 마우스 커서를 화면 중앙에 고정(게임 시작 시 FPS 느낌)
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
    /// 바닥 판정 처리.
    /// </summary>
    private void CheckGround()
    {
        if (groundCheck == null)
        {
            // 초보자 실수 방지: groundCheck 미할당 시 CharacterController 기준으로 대체
            Vector3 fallbackPos = transform.position + Vector3.down * (controller.height * 0.5f - controller.radius + 0.05f);
            isGrounded = Physics.CheckSphere(fallbackPos, groundDistance, groundMask);
        }
        else
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // 바닥에 닿아 있고 아래로 떨어지는 중이면 아주 작은 음수로 고정
        // (착지 직후 튀는 느낌/카메라 미세 떨림 방지)
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    /// <summary>
    /// 마우스 시점 회전 처리.
    /// </summary>
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 상하 회전(카메라): 누적 후 제한
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // 카메라 흔들림 최소화를 위해 로컬 위치를 고정 유지
            // (애니메이션/코드에서 실수로 움직이는 상황 방지)
            Vector3 camPos = playerCamera.transform.localPosition;
            playerCamera.transform.localPosition = new Vector3(camPos.x, camPos.y, camPos.z);
        }

        // 좌우 회전(플레이어 몸체)
        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// 이동, 달리기, 점프, 중력 처리.
    /// </summary>
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A,D
        float moveZ = Input.GetAxis("Vertical");   // W,S

        // 달리기(기본: Left Shift)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 이동 방향: 플레이어 기준 앞/오른쪽
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // 점프(기본: Space)
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            // 물리식 기반 점프 초기 속도 계산
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 중력 누적
        verticalVelocity += gravity * Time.deltaTime;

        // 최종 이동 벡터(수평 + 수직)
        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        // CharacterController로 이동 처리
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // 씬 뷰에서 바닥 체크 구역 확인용
        Gizmos.color = Color.yellow;

        Vector3 pos;
        if (groundCheck != null)
        {
            pos = groundCheck.position;
        }
        else
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
                pos = transform.position + Vector3.down * (cc.height * 0.5f - cc.radius + 0.05f);
            else
                pos = transform.position + Vector3.down * 0.9f;
        }

        Gizmos.DrawWireSphere(pos, groundDistance);
    }
}
