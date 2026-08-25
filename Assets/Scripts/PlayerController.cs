using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Combat")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;

    [Header("Animation")]
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isAttacking = false;
    private bool isDead = false;
    private bool facingRight = true;

    private Vector2 movement;

    private InputSystem_Actions controls;
    private bool sprintHeld = false;
    private bool attackPressed = false;

    void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Attack.performed += ctx => attackPressed = true;
        controls.Player.Sprint.performed += ctx => sprintHeld = true;
        controls.Player.Sprint.canceled += ctx => sprintHeld = false;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleAttack();
        UpdateAnimation();

        // Reset one-shot flags after use
        attackPressed = false;
    }

    void HandleMovement()
    {
        if (isAttacking) return;

        movement = controls.Player.Move.ReadValue<Vector2>();

        if (movement.x > 0 && !facingRight) Flip();
        if (movement.x < 0 && facingRight) Flip();
    }

    void HandleAttack()
    {
        if (attackPressed && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");
        }
    }

    public void OnAttackEnd()
    {
        Debug.Log("OnAttackEnd called!");
        isAttacking = false;
    }
        
    void UpdateAnimation()
    {
        bool isMoving = movement.magnitude > 0;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        float currentSpeed = sprintHeld ? runSpeed : moveSpeed;
        rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
    }

    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    public void TakeHit(int damage, Vector2 knockbackDir)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        rb.AddForce(knockbackDir.normalized * 5f, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
    }

    public bool IsDead() => isDead;
}