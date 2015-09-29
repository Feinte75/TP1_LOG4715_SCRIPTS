using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;
    private bool jump;
	private float facteursaut = 0f;
	private float timer = 1f;
	private float hauteurmax;

	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

	void Update()
	{
		if (character.isGrounded())
		{
			hauteurmax = (transform.position.y+0.8f + (character.getJumpTime())*(character.getJumpForce()/400));
		}
		if (character.getDrawLine())
		{
			Debug.DrawLine(new Vector2(-20, hauteurmax), new Vector2(30, hauteurmax), Color.red);
		}
		//Debug.Log("I am not crazy");
		/*if (Input.GetButtonUp("Jump")) {
			jump = true;
		}

		if (Input.GetButtonDown ("Jump")) {
			jump = false;
		}*/
		// Read the jump input in Update so button presses aren't missed.
		#if CROSS_PLATFORM_INPUT
		if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
		if (CrossPlatformInput.GetButtonUp("Jump")){
			jump = false;
			Debug.Log("I am not crazy");
		}
		#else
		Debug.Log("I am not crazy");
		if (Input.GetButtonDown("Jump")) jump = true;
		if (Input.GetButtonUp("Jump")) jump = false;
		#endif
	}
	/*
    void Update()
    {
        // Read the jump input in Update so button presses aren't missed.
#if CROSS_PLATFORM_INPUT
        if (CrossPlatformInput.GetButtonDown("Jump")) jump = true;
#else
		Debug.Log("I am not crazy");
		if (Input.GetButtonDown("Jump")) 
		{
			facteursaut = timer;
			jump = true;

			while(Input.GetButton("Jump") && timer < 5f)
			{
				timer += Time.deltaTime;
			}

		}
#endif

    }
*/
	void FixedUpdate()
	{
		// Read the inputs.
		bool crouch = Input.GetKey(KeyCode.LeftControl);
		#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis("Horizontal");
		#else
		float h = Input.GetAxis("Horizontal");
		#endif

		// Pass all parameters to the character control script.

		character.Move( h, crouch , jump);

		if (jump) {
			//if walljump
			character.wallJump();
			if(!character.jump)
				StartCoroutine (character.JumpRoutine ());
		}
		character.jump = jump;
		character.Jetpack ();

	}
}
