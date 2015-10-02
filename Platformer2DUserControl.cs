using UnityEngine;

[RequireComponent(typeof(PlatformerCharacter2D))]
public class Platformer2DUserControl : MonoBehaviour 
{
	private PlatformerCharacter2D character;
    private bool jumpPressed = false;		// Jump key pressed
	private bool jumpTrigger = false;		// True on first frame jump key pressed

	void Awake()
	{
		character = GetComponent<PlatformerCharacter2D>();
	}

	void Update()
	{
		// Read the jump input in Update so button presses aren't missed.
		#if CROSS_PLATFORM_INPUT

		// Get jump trigger (First frame with jump key down)
		if (CrossPlatformInput.GetButtonDown("Jump")){
			jumpTrigger = true;
			Debug.Log ("jumpTrigger : "+ jumpTrigger);
		}

		// Get whether jump key is pressed or not
		jumpPressed = CrossPlatformInput.GetButton("Jump");
		#else

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
		character.Jump(jumpPressed, jumpTrigger);

		// Reset jumpTrigger once processed
		jumpTrigger = false;

	}
}
