using UnityEngine;
using System.Collections;

public class PlatformerCharacter2D : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;				// The fastest the player can travel in the x axis.

	[Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%

	[Range(0, 1)]
	[SerializeField] float airControl = .5f;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	[SerializeField] float maxJumpDuration = 0.5f;				// Control jump time
	[SerializeField] float jumpForce = 1000f;			// Amount of force added when the player jumps.	
	[SerializeField] int maxStackedJump = 2;
	[SerializeField] bool drawLine = true;				// Debugging information on jump height

	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	[SerializeField] bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	Animator anim;										// Reference to the player's animator component.

	int stackedJump = 0;

	// Wall Jump
	Transform wallCheck;
	float wallRadius = .1f;
	bool wallBool = false;
	float walljumpMultiplier;
	
	bool jumpPressed;
	IEnumerator jumpRoutine = null;

	// Jetpack
	[SerializeField] float jetpackDuration = 10f;
	bool jetpack = false;

    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		wallCheck = transform.Find ("WallCheck");		// Ajout pour le nouveau transform
		anim = GetComponent<Animator>();
	}

	float hauteurMax = 0;

	void Update() {
		if (drawLine)
		{
			if (grounded)
				hauteurMax = (transform.position.y+0.8f + (maxJumpDuration)*(jumpForce/400));

			Debug.DrawLine(new Vector2(-20, hauteurMax), new Vector2(30, hauteurMax), Color.red);
		}
	}
	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Wall collision check
		wallBool = Physics2D.OverlapCircle(wallCheck.position, wallRadius, whatIsGround);

		// Set the vertical animation
		anim.SetFloat("vSpeed", rigidbody2D.velocity.y);
	}

	/**
	 * 	Jump Logic
	 * 	Method called at each update
	 * 	Takes care of running the good jump or jetpack
	 * 	according to different parameters
	 */ 
	public void jump(bool jumpPressed, bool jumpTrigger)
	{
		this.jumpPressed = jumpPressed;	// Needed for internal jump management

		// Resets variables when on the ground
		if (grounded) {
			stackedJump = 0;
			jetpack = false;	
		}

		// Wall jump
		if (wallBool && !grounded && jumpTrigger) {
			WallJump ();
		} 
		// TimedJump
		else if ((stackedJump < maxStackedJump) && jumpTrigger) {
			/* Takes care of running only one jump coroutine at a time
			 * If one is already ongoing, stop it and start another to 
			 * start another jump
			 */ 
			if (jumpRoutine != null)
				StopCoroutine (jumpRoutine);

			jumpRoutine = TimedJump ();
			StartCoroutine (jumpRoutine);
		} 
		// Jetpack
		else if ((stackedJump >= maxStackedJump)) {
			// Boolean used to start only one jetpack coroutine
			if (!jetpack)
			{
				jetpack = true;
				StartCoroutine (Jetpack ());
			}
		}
	}

	/**
	 *	WallJump 
	 */ 
	public void WallJump()
	{
		// Determine opposite direction
		if(facingRight)
			walljumpMultiplier = -1;
		else
			walljumpMultiplier = 1;

		// Apply a new force in opposite direction
		rigidbody2D.velocity = Vector2.zero;
		rigidbody2D.AddForce(new Vector2((walljumpMultiplier*jumpForce)/3, jumpForce/2));

		// Flip the character to align with new direction
		Flip ();
	}

	/**
	 *	TimedJump
	 *	During the first frame the character is given an impulse
	 *	Over the subsequent frames the character will progressively
	 *	gather force until either the key is freed or the jump timer
	 *	runs out
	 */
	public IEnumerator TimedJump() 
	{
		float timer = 0f;
	
		//rigidbody2D.velocity = Vector2.zero;
		rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0f);
		anim.SetBool ("Ground", false);
		rigidbody2D.AddForce (new Vector2 (0f, jumpForce / 2));
		Debug.Log ("Launching coroutine");

		while (jumpPressed && (timer < maxJumpDuration)) {

			//Add a constant force every frame of the jump
			rigidbody2D.AddForce (new Vector2 (0, Time.deltaTime * jumpForce * maxJumpDuration / 2));
			
			timer += Time.deltaTime;
			
			yield return 0;
		}
		Debug.Log ("Stop Jump" + "  timer = " + timer);
		stackedJump ++;
	}

	/**
	 *  Jetpack
	 *  When every stacked jump is exhausted the character can
	 *  continue gaining velocity by maintaining the jump key down
	 * 	A constant force is then applied until either the character 
	 * 	touches the ground or the jetpack timer runs out
	 */ 
	public IEnumerator Jetpack()
	{
		float timer = 0f;
		jetpack = true;
		Debug.Log ("Enter Jetpack");

		rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0f);

		while ((timer < jetpackDuration) && !grounded) {

			//Add a constant force every frame of the jump

			if(jumpPressed)
			{
				rigidbody2D.AddForce (new Vector2 (0, Time.deltaTime * jumpForce * 2.2f));
				timer += Time.deltaTime;
			}

			yield return 0;
		}
		Debug.Log ("Stop Jetpack" + "  timer = " + timer);
		Debug.Log ("Leave Jetpack");
	}

	/**
	 * 	VelocityCutJump (Not USED)
	 * 	The character is given full impulse on the first frame
	 * 	Then the method waits until the key is released or the 
	 * 	velocity goes negative at which time the velocity is set 
	 * 	to 0 and the character fall back to the ground
	 */ 
	public IEnumerator VelocityCutJump()
	{
		//Add force on the first frame of the jump
		anim.SetBool ("Ground", false);
		rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0f);
		rigidbody2D.AddForce(new Vector2 (0f, 15f), ForceMode2D.Impulse);
		
		//Wait while the character's y-velocity is positive (the character is going up
		while(jumpPressed && rigidbody2D.velocity.y > 0)	
		{	
			yield return null;
		}
		
		//If the jumpButton is released but the character's y-velocity is still positive...
		if(rigidbody2D.velocity.y > 0)
		{
			//...set the character's y-velocity to 0;
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
		}
		stackedJump ++;
	}

	public void Move(float move, bool crouch)
	{

		// If crouching, check to see if the character can stand up
		if(!crouch && anim.GetBool("Crouch"))
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if( Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true;
		}

		// Set whether or not the character is crouching in the animator
		anim.SetBool("Crouch", crouch);

		//only control the player if grounded or airControl is turned on
		if(grounded || airControl != 0f)
		{
			// Reduce the speed if crouching by the crouchSpeed multiplier
			move = (crouch ? move * crouchSpeed : move);

			if(!grounded)
			{
				move = move * airControl;
				rigidbody2D.AddForce(new Vector2 (move * maxSpeed, 0f));
			}
			else {
				// Move the character
				rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);	
			}

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(move));

			// If the input is moving the player right and the player is facing left...
			if(move > 0 && !facingRight)
				// ... flip the player.
				Flip();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if(move < 0 && facingRight)
				// ... flip the player.
				Flip();
		}
	}
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	/**
	 * 	Used in CameraFollow
	 */ 
	public bool inJetpack()
	{
		return jetpack;
	}

	/**
	 *	Used in CameraFollow
	 */ 
	public bool isGrounded() 
	{
		return grounded;
	}
	
	public bool getDrawLine()
	{
		return drawLine;
	}
}
