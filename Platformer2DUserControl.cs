using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;
    private bool jumpPressed = false;
	private bool jumpTrigger = false;
	private float facteursaut = 0f;
	private float timer = 1f;


	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

	void Update()
	{
		//Debug.Log("I am not crazy");
		/*if (Input.GetButtonUp("Jump")) {
			jump = true;
		}

		if (Input.GetButtonDown ("Jump")) {
			jump = false;
		}*/
		// Read the jump input in Update so button presses aren't missed.
		#if CROSS_PLATFORM_INPUT


		if (CrossPlatformInput.GetButtonDown("Jump")){
			jumpTrigger = true;
			Debug.Log ("jumpTrigger : "+ jumpTrigger);
		}

		jumpPressed = CrossPlatformInput.GetButton("Jump");
		//Debug.Log ("jumpPressed = " + jumpPressed);
		#else

		if (Input.GetButtonDown("Jump")) jump = true;
		if (Input.GetButtonUp("Jump")) jump = false;
		#endif
	}

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

		character.Move( h, crouch);
		character.jump(jumpPressed, jumpTrigger);
		jumpTrigger = false;

	}
}
