using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Worm : MonoBehaviour
{
//STATS------------------------------------------------------------------------
//movement
    public float walkSpeed = 1f;
    public float maxRelativeVelocity = 6f;
//slope
    [SerializeField]
    private float maxSlopeAngle;
    [SerializeField]
    private float slopeCheckDistance;
//jump
    [SerializeField]
    private float groundCheckRadius;
    public float jumpForce = 5f;
//shooting
    public float bulletMaxInitialVelocity = 5f;
    public float shootingTime;
    public float[] shootForce;
    public GameObject currentGun;
    public GameObject shootingEffect;
    public GameObject healthCanvas;
    public GameObject weaponsCanvas;
    public GameObject[] selectIcon;
//explosion
    public float hitVelocity;
    public float distanceFactor = 1f;
    public float bounciness;
    public float hitTime = 1f;
//team
    [SerializeField]
    private GameObject backgroundHP;
    public int wormID;
    public int teamID;
    public bool IsTurn { get { return WormMenager.instance.IsMyTurn(wormID); }  }


//PROPERTIES-------------------------------------------------------------------
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private Transform slopeCheck;
    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private Transform bulletInitialTransform;

    [SerializeField]
    private Transform hpText;

    [SerializeField]
    private PhysicsMaterial2D noFriction;
    [SerializeField]
    private PhysicsMaterial2D fullFriction;
    [SerializeField]
    private PhysicsMaterial2D bouncy;
    

    [SerializeField]
    private GameObject[] bulletprefabs;


    private Rigidbody2D rb;
    private CapsuleCollider2D cc;
    private Animator animator;

    private Camera mainCamera;
    private WormMenager wormMenager;
    private AudioMenager audioMenager;

//UTILITIES--------------------------------------------------------------------
//movement
    [SerializeField]
    private Vector2 currentVel;
    private bool facingDirection = true; //true = right
    private bool isTouching;
    [SerializeField]
    private bool isHit;
    //slope
    private bool isOnSlope;
    private bool canWalkOnSlope;
    private float xInput;

    [SerializeField]
    private float slopeDownAngle;
    [SerializeField]
    private float slopeSideAngle;

    private float lastSlopeAngle;
    private Vector2 capsuleColliderSize;
    private Vector2 slopeNormalPerp;
//jump
    private bool isGrounded;
    private bool isJumping;
    private bool canJump;
    private Vector2 newVelocity;
    private Vector2 newForce;
//shooting
    private Vector2 shootDirection;
    private bool targetting;
    private bool shooting;

    private float shootingCounter;

    private Vector3 diff;

    private GameObject bulletprefab;

//explosion
    private float hitCounter;
//mouse
    private Vector3 mousePosScreen;
    private Vector3 mousePosWorld;
    [SerializeField]
    private Vector2 playerToMouse;
    private float angle;
//animation
    private float jumpAnimationCounter;
    private float jumpAnimationTime = 0.1f;
    private bool jumpAnimation;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        cc = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        capsuleColliderSize = cc.size;

        audioMenager = GameObject.FindObjectOfType<AudioMenager>();
        wormMenager = GameObject.FindObjectOfType<WormMenager>();

        mainCamera = Camera.main;

        bulletprefab = bulletprefabs[0];
        bulletMaxInitialVelocity = shootForce[0];

        transform.position = new Vector2(Random.Range(-25f, 7f), transform.position.y);

        ChangeColor();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void FixedUpdate()
    {
        CheckGround();
        SlopeCheck();

        if (isHit)
        {
            MakingBoncy();
            hitCounter += Time.deltaTime;
        }

        if (!targetting && !isHit)
        {
            Facing();
            ApplyMovement();
        }

        currentVel = rb.velocity;
    }

    private void Update()
    {
        if(IsTurn && !wormMenager.IswormWating())
        {
            if (!wormMenager.IsGameOver())
            {
                CheckInput();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0); //menu
            }
            animator.SetFloat("speed", Mathf.Abs(xInput));
            animator.SetFloat("fallingSpeed", rb.velocity.y);
        }
        else if (!IsTurn || wormMenager.IswormWating())
        {
            ZeroInput();
        }

        if (targetting)
        {
            UpdateMouse();
            UpdateTargetting();
            RotateGun();
        }

        if (shooting)
        {
            shootingCounter += Time.deltaTime;
            if (Input.GetMouseButtonUp(0))
            {
                Shoot();
            }
            if (shootingCounter > shootingTime)
            {
                Shoot();
            }
        }

        JumpAnimation();
    }
    void RotateGun()
    {
        diff = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition) - transform.position;
        diff.Normalize();
        
        float rot_Z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        currentGun.transform.rotation = Quaternion.Euler(0, 0, rot_Z + 180f);
    }

    private void CheckInput()
    {
        //movement
        xInput = Input.GetAxisRaw("Horizontal");

        if (xInput != 0 && isGrounded)
        {
            if(!audioMenager.IsSoundMovePlaying())
                audioMenager.PlaySoundMove();
        }
        else
        {
            audioMenager.StopSoundMove();
        }

        if (!targetting)
        {
            //jump
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
                animator.SetBool("isJumping", true);
                jumpAnimation = true;
                audioMenager.PlaySoundJump();

            }
        }

        //shooting
        if (Input.GetButtonDown("Fire3"))
        {
            if (!shooting)
            {
                targetting = !targetting;
                if (targetting)
                {
                    animator.SetBool("isTargetting", true);
                }
                else
                {
                    animator.SetBool("isTargetting", false);
                }
            }

            if (targetting)
            {
                currentGun.SetActive(true);
                healthCanvas.SetActive(false);
                weaponsCanvas.SetActive(true);
            }
            else
            {
                currentGun.SetActive(false);
                healthCanvas.SetActive(true);
                weaponsCanvas.SetActive(false);
            }
        }

        if (targetting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                shooting = true;
                shootingCounter = 0f;
                shootingEffect.SetActive(true);
            }

            //weapon swap
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                bulletprefab = bulletprefabs[0];
                bulletMaxInitialVelocity = shootForce[0];

                selectIcon[0].SetActive(true);
                selectIcon[1].SetActive(false);
                selectIcon[2].SetActive(false);
                selectIcon[3].SetActive(false);
                selectIcon[4].SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                bulletprefab = bulletprefabs[1];
                bulletMaxInitialVelocity = shootForce[1];

                selectIcon[0].SetActive(false);
                selectIcon[1].SetActive(true);
                selectIcon[2].SetActive(false);
                selectIcon[3].SetActive(false);
                selectIcon[4].SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                bulletprefab = bulletprefabs[2];
                bulletMaxInitialVelocity = shootForce[2];

                selectIcon[0].SetActive(false);
                selectIcon[1].SetActive(false);
                selectIcon[2].SetActive(true);
                selectIcon[3].SetActive(false);
                selectIcon[4].SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                bulletprefab = bulletprefabs[3];
                bulletMaxInitialVelocity = shootForce[3];

                selectIcon[0].SetActive(false);
                selectIcon[1].SetActive(false);
                selectIcon[2].SetActive(false);
                selectIcon[3].SetActive(true);
                selectIcon[4].SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                bulletprefab = bulletprefabs[4];
                bulletMaxInitialVelocity = shootForce[4];

                selectIcon[0].SetActive(false);
                selectIcon[1].SetActive(false);
                selectIcon[2].SetActive(false);
                selectIcon[3].SetActive(false);
                selectIcon[4].SetActive(true);
            }
        }
    }
    private void SlopeCheck()
    {
        Vector2 checkPos = slopeCheck.position;

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
        Debug.DrawRay(slopeHitFront.point, slopeHitFront.normal, UnityEngine.Color.red);
        Debug.DrawRay(slopeHitBack.point, slopeHitBack.normal, UnityEngine.Color.cyan);
        Debug.DrawRay(slopeHitFront.point, Vector2.up, UnityEngine.Color.green);
        Debug.DrawRay(slopeHitBack.point, Vector2.up, UnityEngine.Color.blue);
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;
        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (isOnSlope && canWalkOnSlope && xInput == 0.0f || targetting)
        {
            rb.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (rb.velocity.y <= 0.0f)
        {
            isJumping = false;
        }

        if (isGrounded && !isJumping && slopeDownAngle <= maxSlopeAngle)
        {
            canJump = true;
        }
    }

    private void ApplyMovement()
    {
        if (isGrounded && !isOnSlope && !isJumping) //if not on slope
        {
            newVelocity.Set(walkSpeed * xInput, 0.0f);
            rb.velocity = newVelocity;
        }
        else if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping) //If on slope
        {
            newVelocity.Set(walkSpeed * slopeNormalPerp.x * -xInput, walkSpeed * slopeNormalPerp.y * -xInput);
            rb.velocity = newVelocity;
        }
        else if (!isGrounded && !isTouching) //If in air
        {
            newVelocity.Set(walkSpeed * xInput, rb.velocity.y);
            rb.velocity = newVelocity;
        }
    }
    private void Jump()
    {
        if (canJump)
        {
            canJump = false;
            isJumping = true;
            newVelocity.Set(0.0f, 0.0f);
            rb.velocity = newVelocity;
            newForce.Set(0.0f, jumpForce);
            rb.AddForce(newForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateMouse()
    {
        mousePosScreen = Input.mousePosition;
        mousePosWorld = Camera.main.ScreenToWorldPoint(mousePosScreen);
        playerToMouse = new Vector2(mousePosWorld.x - transform.position.x,
                                    mousePosWorld.y - transform.position.y);
        playerToMouse.Normalize();
    }

    void UpdateTargetting()
    {
        angle = Mathf.Asin(playerToMouse.y) * Mathf.Rad2Deg;

        if (playerToMouse.x < 0f)
            angle = 180 - angle;

        if (playerToMouse.x >= 0f && !facingDirection)
        {
            Flip();
        }
        else if (playerToMouse.x < 0f && facingDirection)
        {
            Flip();
        }
    }

    void Shoot()
    {
        shootingEffect.SetActive(false);
        shootDirection = playerToMouse;
        GameObject bullet = Instantiate(bulletprefab);
        bullet.transform.position = bulletInitialTransform.position;
        bullet.GetComponent<Rigidbody2D>().velocity = shootDirection * bulletMaxInitialVelocity * (shootingCounter / shootingTime);
        shooting = false;
        targetting = false;
        currentGun.SetActive(false);
        wormMenager.StartNextWormCounter();
        animator.SetBool("isTargetting", false);
        weaponsCanvas.SetActive(false);
        healthCanvas.SetActive(true);
        audioMenager.PlaySoundShoot();
    }

    private void Facing()
    {
        if (xInput == 1 && !facingDirection)
        {
            Flip();
        }
        else if (xInput == -1 && facingDirection)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingDirection = !facingDirection;
        transform.Rotate(0.0f, 180.0f, 0.0f);
        hpText.transform.Rotate(0.0f, 180.0f, 0.0f);
        weaponsCanvas.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void Knockback(Transform explPosition, float explStrength)
    {
        isHit = true;

        Vector2 currentVelocity = rb.velocity;

        Vector2 direction = (rb.position - (Vector2)explPosition.position).normalized;
        float distance = Vector2.Distance(explPosition.position, transform.position);
        float proximityValue = distanceFactor / (distance + 0.01f);
        float clampedValue = Mathf.Clamp(proximityValue, 0f, 10f);

        explStrength = explStrength * clampedValue * 0.7f;

        newForce.Set( (direction.x / Mathf.Abs(direction.x)) * Mathf.Abs(direction.x + 2),
                      (direction.y / Mathf.Abs(direction.y)) * Mathf.Abs(direction.y + 3));

        Vector2 targetVelocity = newForce;
        Vector2 difference = targetVelocity - currentVelocity;
        Vector2 force = new Vector2(rb.mass * difference.x * explStrength, rb.mass * difference.y * explStrength);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void MakingBoncy()
    {
        if(hitCounter <= 1f || rb.velocity.x > hitVelocity || -rb.velocity.x > hitVelocity || rb.velocity.y > hitVelocity * 3 || -rb.velocity.y > hitVelocity * 3)
        {
            bouncy.bounciness = (Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.y)) * bounciness;
            rb.sharedMaterial = bouncy;
        }
        else
        {
            isHit = false;
            hitCounter = 0f;
        }
    }

    private void ChangeColor()
    {
        if(teamID == 0)
        {
            backgroundHP.GetComponent<Image>().color = new Color32(255, 0, 0, 166);
        }
        else if (teamID == 1)
        {
            backgroundHP.GetComponent<Image>().color = new Color32(0, 255, 21, 166);
        }
        else if (teamID == 2)
        {
            backgroundHP.GetComponent<Image>().color = new Color32(42, 0, 225, 166);
        }
        else if (teamID == 3)
        {
            backgroundHP.GetComponent<Image>().color = new Color32(255, 255, 0, 166);
        }
    }

    private void JumpAnimation()
    {
        if (jumpAnimation)
        {
            jumpAnimationCounter += Time.deltaTime;
        }
        if (jumpAnimationCounter >= jumpAnimationTime)
        {
            jumpAnimation = false;
            jumpAnimationCounter = 0;
        }
        if (isGrounded && !jumpAnimation)
        {
            animator.SetBool("isJumping", false);
        }
    }

    public void ChangeID(int id)
    {
        wormID = id;
    }

    public int GetID()
    {
        return wormID;
    }
    public int GetTeamID()
    {
        return teamID;
    }

    public void ChangeTeam(int id)
    {
        teamID = id;
    }

    public void ZeroInput()
    {
        xInput = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isTouching = true; 
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isTouching = false; 
    }
}
