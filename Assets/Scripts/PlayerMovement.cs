using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb2D;

    //���aInput
    public InputActionAsset inputActionAsset;
    private InputActionMap playerAct;

    private Vector2 inputVector;
    private bool isGrounded;

    [Header("�I���c��")]
    public LayerMask GroundLayer;

    [System.Serializable] public class HorizontalMovement
    {
        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public float TurnAcceleration;
    }
    public HorizontalMovement horizontalMovement;

    [System.Serializable] public class Jump
    {
        public float JumpHeight;
        public float TakeoffVelocity;

        public float DownGravity;
        public bool ReleasedJump;

        public float TerminalVelocity;

        public float BufferTime;
        public float currentBufferTime;
        public float CoyoteTime;
        public float currentCoyoteTime;
    }
    public Jump jump;



    private void OnEnable()
    {
        playerAct.Enable();
    }
    private void OnDisable()
    {
        playerAct.Disable();
    }
    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        inputActionAsset = GetComponent<PlayerInput>().actions;
        playerAct = inputActionAsset.FindActionMap("Player");

        isGrounded = true;
    }
    
    void Update()
    {
        inputVector = playerAct.FindAction("Movement").ReadValue<Vector2>();
        if (inputVector.x > -0.0001f && inputVector.x < 0.0001f)//�ե��n��u�W�U����x�b�|�ܦ�1.525879E-05��bug
            inputVector.x = 0;

        #region ���D
        if (jump.currentCoyoteTime > 0 && !isGrounded)
            jump.currentCoyoteTime = Mathf.Clamp(jump.currentCoyoteTime - Time.deltaTime, 0, jump.CoyoteTime);
        if (jump.currentBufferTime > 0)
            jump.currentBufferTime = Mathf.Clamp(jump.currentBufferTime - Time.deltaTime, 0, jump.BufferTime);

        if (playerAct.FindAction("Jump").IsPressed()) 
        {
            jump.currentBufferTime = jump.BufferTime;
        }
        if (jump.currentBufferTime > 0 && (jump.currentCoyoteTime > 0 || isGrounded))
        {
            rb2D.velocity = new Vector3(rb2D.velocity.x, jump.TakeoffVelocity);
            //isGrounded = true;
            jump.currentBufferTime = 0;
            jump.currentCoyoteTime = 0;
        }

        if (playerAct.FindAction("Jump").WasPressedThisFrame())
            jump.ReleasedJump = false;
        if (playerAct.FindAction("Jump").WasReleasedThisFrame())
            jump.ReleasedJump = true;

        #endregion
    }

    private void FixedUpdate()
    {
        float horizontalVelocity = 0;
        if (inputVector.x == 0)
        {
            //����rb2D.velocity.x������Ȱ����ƭp���A���Wrb2D.velocity.x�쥻�����t�A�o�˰��t�ƭp�⤣�|�Xbug
            horizontalVelocity = Sign(rb2D.velocity.x) * Mathf.Clamp(Mathf.Abs(rb2D.velocity.x) - horizontalMovement.Deceleration * Time.deltaTime, 0, horizontalMovement.MaxSpeed);
        }
        else if (inputVector.x != 0 && rb2D.velocity.x != 0 && Sign(inputVector.x) != Sign(rb2D.velocity.x)) //��J��V ������ �}�⥿�b���ʪ���V
        {
            horizontalVelocity = Sign(rb2D.velocity.x) * Mathf.Clamp(Mathf.Abs(rb2D.velocity.x) - horizontalMovement.TurnAcceleration * Time.deltaTime, 0, horizontalMovement.MaxSpeed);
        }
        else //��J��V ���� �}�⥿�b���ʪ���V
        {
            horizontalVelocity = Mathf.Clamp(rb2D.velocity.x + Sign(inputVector.x) * horizontalMovement.Acceleration * Time.deltaTime, -horizontalMovement.MaxSpeed, horizontalMovement.MaxSpeed);
        }

        float verticalVelocity = rb2D.velocity.y + Physics2D.gravity.y * Time.deltaTime;
        if (rb2D.velocity.y <= jump.TerminalVelocity)
            verticalVelocity = jump.TerminalVelocity;
        else if (rb2D.velocity.y > 0 && jump.ReleasedJump)
            verticalVelocity += jump.DownGravity * Time.deltaTime;

        rb2D.velocity = new Vector3(horizontalVelocity, verticalVelocity);
    }

    static float Sign(float number)
    {
        return number < 0 ? -1 : (number > 0 ? 1 : 0);
    }

    #region child trigger

    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == GroundLayer.value && collision.contacts[0].normal == Vector2.up)
        {
            isGrounded = true;
            jump.currentCoyoteTime = jump.CoyoteTime;
            print("Enter");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == GroundLayer.value)
        {
            print("Exit");
            isGrounded = false;
        }
    }
}
