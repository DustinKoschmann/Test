using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {
	public GameObject arrowPrefab;
    public GameObject fakeArrowPrefab;
    public GameObject bowPrefab;
	public Slider arrowChargeSlider;
    public float arrowPower = 30f;
    public float arrowTimerMax = 1f;

    private Animator animator;
    public float MaxSpeed = 50;
    public float Acceleration = 400;
    public float JumpSpeed = 20;
    public float JumpDuration;

    public bool EnableDoubleJump = true;
    public bool WallHitDoubleJumpOverride = true;


    //Internal 
    private bool canDoubleJump = true;
    private bool jumpKeyDown = false;
    private bool canVariableJump = false;
    private float jmpDuration;

    Rigidbody rb;

    private GameObject bow;
    private GameObject fakeArrow;
    
	private bool arrowCharging = false;
	private float arrowTimer = 0f;
    float percentageArrowPower = 0f;


    void Awake () {		
		SpawnBow ();
		rb = GetComponent<Rigidbody>();
        fakeArrow = new GameObject();

        animator = transform.GetChild(0).GetComponent<Animator>();
    }

	void Update() {
        Movement();
    }

	void FixedUpdate() {
		rb.velocity += Vector3.up * Physics.gravity.y * 3f * Time.deltaTime;
		RotateBow ();
	}

    //Alles Movement bezogenes
    private void Movement() {
        bool leftWallHit = false;
        bool rightWallHit = false;
        bool onTheGround = false;

        bool movingLeft = false;
        bool movingRight = false;

        percentageArrowPower = arrowTimer / arrowTimerMax;
        IsArrowIsCharging();
        arrowChargeSlider.value = percentageArrowPower;

        // Walls und Ground erst bei niedriger Velocity checken
        if(rb.velocity.x <= 2f && rb.velocity.x >= -2f) {
            leftWallHit = IsOnWallLeft();
            rightWallHit = IsOnWallRight();
        }
        if(rb.velocity.y <= 2f && rb.velocity.y >= -2f) {
            onTheGround = IsOnGround();
        }    

        float horizontal = Input.GetAxis("Horizontal");

        if(horizontal <= -0.1f) {
            movingLeft = true;
        } else if (horizontal >= 0.1f) {
            movingRight = true;
        }

        float wallSlideForce = 7f;

        if(leftWallHit && movingLeft) {
            horizontal = 0f;
            rb.velocity = new Vector3(rb.velocity.x, -wallSlideForce, rb.velocity.y);
        }

        if(rightWallHit && movingRight) {
            horizontal = 0f;
            rb.velocity = new Vector3(rb.velocity.x, -wallSlideForce, rb.velocity.y);
        }

        if(movingLeft) {
            if(rb.velocity.x > -this.MaxSpeed) {
                rb.AddForce(new Vector3(-this.Acceleration, 0f, 0f), ForceMode.Acceleration);
            } else {
                rb.velocity = new Vector3(-this.MaxSpeed, rb.velocity.y, rb.velocity.z);
            }
        } else if(movingRight) {
            if(rb.velocity.x < this.MaxSpeed) {
                rb.AddForce(new Vector3(this.Acceleration, 0f, 0f), ForceMode.Acceleration);
            } else {
                rb.velocity = new Vector3(this.MaxSpeed, rb.velocity.y, rb.velocity.z);
            }
        }

        if(Input.GetMouseButtonDown(0)) {
            ChargeArrow();
        }
        if(Input.GetMouseButtonUp(0)) {
            ShootArrow();
            ResetArrowCharge();
        }

        float vertical = Input.GetAxis("Vertical");
        
        if(onTheGround) {
            canDoubleJump = true;
        }

        if(vertical > 0.1f) {
            if(!jumpKeyDown) { // 1st Frame
                jumpKeyDown = true;
                canVariableJump = true;

                if(onTheGround || (canDoubleJump && EnableDoubleJump) || WallHitDoubleJumpOverride) {
                    bool wallHit = false;
                    int wallHitDirection = 0;
                    
                    if(leftWallHit) {
                        wallHit = true;
                        wallHitDirection = 1;
                    } else if(rightWallHit) {
                        wallHit = true;
                        wallHitDirection = -1;
                    }

                    if(!wallHit) {
                        if(onTheGround || (canDoubleJump && EnableDoubleJump)) {
                            rb.velocity = new Vector3(rb.velocity.x, this.JumpSpeed, rb.velocity.z);
                        }
                    } else {
                        rb.velocity = new Vector3(this.JumpSpeed * wallHitDirection, this.JumpSpeed);
                    }

                    if(!onTheGround && !wallHit) {
                        canDoubleJump = false;
                    }

                    //if(canVariableJump) {
                    //    jmpDuration += Time.deltaTime;
                    //    if(jmpDuration / 1000 < JumpDuration) {
                    //        rb.AddForce(Vector3.up * this.JumpSpeed);
                    //    } else {
                    //        jmpDuration = 0;
                    //        canVariableJump = false;
                    //    }
                    //}
                }
            }
        } else {
            jumpKeyDown = false;
        }

        //Movement Aimationen
        float actualSpeed = Mathf.Abs(rb.velocity.x) / MaxSpeed;
        animator.SetFloat("MoveSpeed", actualSpeed);
        //Debug.Log(actualSpeed);
    }

    //Schauen ob der Arrow auflädt und platziere Arrow Attrappe. Setze Maximale Chargedauer.
    private void IsArrowIsCharging() {
        if(arrowCharging) {
            arrowTimer += Time.deltaTime;
            if(arrowTimer >= arrowTimerMax) {
                arrowTimer = arrowTimerMax;
            }

            Vector3 lookVector = GetArrowDirectionVector();
            Vector3 positionOffset = lookVector * percentageArrowPower * -1.2f;
            fakeArrow.transform.SetParent(transform);
            fakeArrow.transform.position = transform.position + positionOffset;
            fakeArrow.transform.rotation = Quaternion.LookRotation(lookVector, Vector3.up);
        }
    }

    //Setzte arrowCharging auf true, solange Linksklick gedrückt wird. Lösche und erstelle neue Arrow Attrappe.
    private void ChargeArrow() {
        if(!arrowCharging) {
            arrowCharging = true;
            Destroy(fakeArrow);
            fakeArrow = Instantiate(fakeArrowPrefab, transform.position, Quaternion.identity) as GameObject;
        }
    }

    //Lösche Arrow Attrappe. Erstelle neuen Arrow und füge Velocity in Mausrichtung hinzu.
    private void ShootArrow() {
        Destroy(fakeArrow);
        float arrowVelocity = percentageArrowPower * arrowPower;
        Vector3 directionVector = GetArrowDirectionVector();
        GameObject newArrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity) as GameObject;
        Rigidbody rbArrow = newArrow.GetComponent<Rigidbody>();
        rbArrow.velocity = directionVector * arrowVelocity;

        Vector3 movementDirectionVector = rb.velocity.normalized;
    }

    private void ResetArrowCharge() {
        arrowCharging = false;
        arrowTimer = 0.0f;
    }

    //Ermittelt einen Richtungsvektor von der Spielerposition bis zur Mausposition und bildet Normalvektor.
    private Vector3 GetArrowDirectionVector() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		RaycastHit hit; 
		Vector3 mousePos = new Vector3();
		if (Physics.Raycast (ray, out hit, 100)) {
			mousePos = new Vector3 (hit.point.x, hit.point.y, rb.transform.position.z);
			Debug.DrawLine (rb.transform.position, mousePos);
		}

		return (mousePos - rb.transform.position).normalized;
	}

    private bool IsOnGround() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float lenthToSearch = 0.2f;
        float colliderThreshold = -0.1f;
        RaycastHit hit;
        bool retVal = false;

        Vector3 linestart = new Vector3(this.transform.position.x, this.transform.position.y - mesh.bounds.extents.y - colliderThreshold, this.transform.position.z);
        Vector3 vectorToSearch = new Vector3(this.transform.position.x, linestart.y - lenthToSearch, this.transform.position.z);

        if(Physics.Linecast(linestart, vectorToSearch, out hit)) {
            retVal = true;
        }

        Debug.DrawLine(linestart, vectorToSearch, Color.red);
        return retVal;
    }

    private bool IsOnWallLeft() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float lengthToSearch = 0.2f;
        float colliderThreshold = -0.1f;
        RaycastHit hit;
        bool retVal = false;

        Vector3 linestart = new Vector3(this.transform.position.x - mesh.bounds.extents.x - colliderThreshold, this.transform.position.y, this.transform.position.z);

        Vector3 vectorToSearch = new Vector3(linestart.x - lengthToSearch, this.transform.position.y, this.transform.position.z);

        if(Physics.Linecast(linestart, vectorToSearch, out hit)) {
            if(hit.collider.GetComponent<NoSlideJump>()) {
                retVal = false;
            } else {
                retVal = true;
            }
        }

        Debug.DrawLine(linestart, vectorToSearch, Color.red);
        return retVal;
    }

    private bool IsOnWallRight() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float lengthToSearch = 0.2f;
        float colliderThreshold = -.1f;
        RaycastHit hit;
        bool retVal = false;

        Vector3 linestart = new Vector3(this.transform.position.x + mesh.bounds.extents.x + colliderThreshold, this.transform.position.y, this.transform.position.z);

        Vector3 vectorToSearch = new Vector3(linestart.x + lengthToSearch, this.transform.position.y, this.transform.position.z);

        if(Physics.Linecast(linestart, vectorToSearch, out hit)) {
            if(hit.collider.GetComponent<NoSlideJump>()) {
                retVal = false;
            } else {
                retVal = true;
            }
        }

        Debug.DrawLine(linestart, vectorToSearch, Color.red);
        return retVal;
    }

    private void SpawnBow() {
		bow = Instantiate (bowPrefab, transform.position, Quaternion.identity) as GameObject;
		bow.transform.rotation = Quaternion.AngleAxis (90, Vector3.right);
		bow.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		bow.transform.SetParent (transform);
	}

	private void RotateBow() {
		Quaternion lookRot = Quaternion.LookRotation (GetArrowDirectionVector (), Vector3.up);
		bow.transform.rotation = lookRot;
		bow.gameObject.layer = 8;
	}
}