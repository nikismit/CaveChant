using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkMovement : MonoBehaviour {

	public AudioPitch pitch;

	public int currentPitch;
	public float currentAmp;
	[Header("Movement Speeds")]
	public float maximumForwardSpeed;
	public float forwardAccelaration;
	public float forwardDeceleration;

	public float currentTurn;
	public float currentSpeed;
	private float startMaxSpeed;
	private float startAccel;
	private float startDecel;

	[Header("Options")]
	public float minimumPitch;
	public float maximumPitch;
	public bool highPitchIsTurnRight;
	public bool mouseIsSteering;
	public bool walkingIsTrue;
	public float SteeringSpeed;
	private float SteeringSpeedSave;

	public GameObject car;
	public float xAngle, yAngle;
	public float angleOfFlight;
	public float properRotation;
	public float prevValue;

	public float FallingRate;
	private float SavedFallingRate;


  Rigidbody m_Rigidbody;




	[Header("Animation")]
	public Animator m_Animator;
	private float TopSpeed;


	public Light torchLight;
	public Light fillLight;
	public GlowController glow;
	private float initialTorchLightIntensity;
	private float initialFillLightIntensity;
	[Range(0.0f, 1.0f)]
	public float maxGlowAlpha = 0.5f;
	[Range(0.0f, 0.1f)]
	public float lightDecayRate = 0.005f;
	[Range(0.0f, 0.1f)]
	public float lightUpRate = 0.01f;
	[Range(0.0f, 0.1f)]
	public float glowDecayRate = 0.002f;
	private bool hasStopped;
	public AudioAnalyse audioAnalyse;


    void Start()
    {

    m_Rigidbody = GetComponent<Rigidbody>();


		startMaxSpeed = maximumForwardSpeed;
		startAccel = forwardAccelaration;
		startDecel = forwardDeceleration;
		initialTorchLightIntensity = torchLight.intensity;
		initialFillLightIntensity = fillLight.intensity;
		torchLight.intensity = 0;
		fillLight.intensity = 0;
		glow.alpha.a = 0;
		hasStopped = true;

    }



		private void NoTurning (){
			xAngle = 0;
			car.transform.Rotate(xAngle, -yAngle, 0, Space.Self);
		}

	private void Steering(){
		Vector3 mousePos = Input.mousePosition;
		{
			var normalised = (mousePos.x / Screen.width) * 2 - 1;
			Debug.Log (normalised);
			yAngle = - normalised * SteeringSpeed;
		}
	}

	private void SteerWithSpeed(){
		if (currentSpeed <= 0f) {
			SteeringSpeed = 0f;
		}
		else if (currentSpeed > 0f) {
			SteeringSpeed = SteeringSpeedSave;
		}
	}

	void FixedUpdate(){
		 if (walkingIsTrue == true) {
		 	if (currentSpeed <= 0){
		 		if (fillLight.intensity >= 0f){
		 			fillLight.intensity -= lightDecayRate;
		 		}
		 		if (torchLight.intensity >= 0f){
		 			torchLight.intensity -= lightDecayRate;
		 		}
		 		if (glow.alpha.a >= 0f){
		 			glow.alpha.a -= glowDecayRate;
		 		}
		 	}
		}
		//------------------------------------------------------------------------------------------------------------------------------
		if(audioAnalyse.On == true){
			currentTurn = (((currentPitch-minimumPitch)/(maximumPitch-minimumPitch))*2)-1;
			if(highPitchIsTurnRight == false){
				currentTurn *= -1;
			}
			 else {
				maximumForwardSpeed = startMaxSpeed;
				forwardAccelaration = startAccel;
				forwardDeceleration = startDecel;
			}

			if(currentSpeed < maximumForwardSpeed){
				currentSpeed += forwardAccelaration *Time.fixedDeltaTime;
			} else {
				currentSpeed -= forwardDeceleration *Time.fixedDeltaTime;
			}
			this.transform.Translate(transform.forward * currentSpeed * Time.fixedDeltaTime, Space.World);
			if (walkingIsTrue == true){
				NoTurning();
				m_Animator.SetBool("Walking", true);
				if (torchLight.intensity <= initialTorchLightIntensity){
					torchLight.intensity += lightUpRate;
				if (glow.alpha.a <= maxGlowAlpha){
					glow.alpha.a += lightUpRate;
				}
				}
				if (fillLight.intensity <= initialFillLightIntensity){
					fillLight.intensity += lightUpRate;
				}
			}
			SavedFallingRate = FallingRate;

			prevValue = currentTurn;
			TopSpeed = currentSpeed;
		//-----------------------------------------------------------------------------------------------------------------------
		} else if(currentSpeed >= 0) {
			currentSpeed -= forwardDeceleration *Time.fixedDeltaTime;

			this.transform.Translate(transform.forward * currentSpeed * Time.fixedDeltaTime, Space.World);

			if (walkingIsTrue == true) {
				NoTurning();
				if (torchLight.intensity <= initialTorchLightIntensity + 0.1f){
					torchLight.intensity -= lightDecayRate;
					glow.alpha.a -= glowDecayRate;
				}
				if (fillLight.intensity <= initialFillLightIntensity + 0.1f){
					fillLight.intensity -= lightDecayRate;
				}

				if (TopSpeed <= maximumForwardSpeed){
					m_Animator.SetBool("Walking", false);
				}
				else if (TopSpeed > maximumForwardSpeed && currentSpeed < maximumForwardSpeed/2f){
					m_Animator.SetBool("Walking", false);
				}
			}
			prevValue = currentTurn;
		}
	}


    // void FixedUpdate()
    // {
		//
		// // if (walkingIsTrue == true) {
		// // 	if (currentSpeed <= 0){
		// // 		if (fillLight.intensity >= 0f){
		// // 			fillLight.intensity -= lightDecayRate;
		// // 		}
		// // 		if (torchLight.intensity >= 0f){
		// // 			torchLight.intensity -= lightDecayRate;
		// // 		}
		// // 		if (glow.alpha.a >= 0f){
		// // 			glow.alpha.a -= glowDecayRate;
		// // 		}
		// // 	}
		// // }
		// currentPitch = pitch._currentpublicpitch;
		// currentAmp = pitch._currentPublicAmplitude;
		//
		// if(mouseIsSteering == true){
		// 	Steering ();
		// 	SteerWithSpeed ();
		// }
		//
		// properRotation = car.transform.eulerAngles.x;
		//
		//
		// // if(currentPitch > minimumPitch){
		// if(currentAmp > -20f && currentPitch > minimumPitch){
		// 	currentTurn = (((currentPitch-minimumPitch)/(maximumPitch-minimumPitch))*2)-1;
		// 	On = true;
		// 	if(highPitchIsTurnRight == false){
		// 		currentTurn *= -1;
		// 	}
		// 	 else {
		// 		maximumForwardSpeed = startMaxSpeed;
		// 		forwardAccelaration = startAccel;
		// 		forwardDeceleration = startDecel;
		// 	}
		//
		// 	if(currentSpeed < maximumForwardSpeed){
		// 		currentSpeed += forwardAccelaration *Time.fixedDeltaTime;
		// 	} else {
		// 		currentSpeed -= forwardDeceleration *Time.fixedDeltaTime;
		// 	}
		// 	this.transform.Translate(transform.forward * currentSpeed * Time.fixedDeltaTime, Space.World);
		// 	if (walkingIsTrue == true){
		// 		NoTurning();
		// 		m_Animator.SetBool("Walking", true);
		// 		if (torchLight.intensity <= initialTorchLightIntensity){
		// 			torchLight.intensity += lightUpRate;
		// 		if (glow.alpha.a <= maxGlowAlpha){
		// 			glow.alpha.a += lightUpRate;
		// 		}
		// 		}
		// 		if (fillLight.intensity <= initialFillLightIntensity){
		// 			fillLight.intensity += lightUpRate;
		// 		}
		//
		//
		// 	}
		// 	SavedFallingRate = FallingRate;
		//
		// 	prevValue = currentTurn;
		// 	TopSpeed = currentSpeed;
		//
		// } else if(currentSpeed >= 0) {
		// 	currentSpeed -= forwardDeceleration *Time.fixedDeltaTime;
		// 	On = false;
		//
		// 	this.transform.Translate(transform.forward * currentSpeed * Time.fixedDeltaTime, Space.World);
		//
		// 	if (walkingIsTrue == true) {
		// 		NoTurning();
		// 		if (torchLight.intensity <= initialTorchLightIntensity + 0.1f){
		// 			torchLight.intensity -= lightDecayRate;
		// 			glow.alpha.a -= glowDecayRate;
		// 		}
		// 		if (fillLight.intensity <= initialFillLightIntensity + 0.1f){
		// 			fillLight.intensity -= lightDecayRate;
		// 		}
		//
		// 		if (TopSpeed <= maximumForwardSpeed){
		// 			m_Animator.SetBool("Walking", false);
		// 		}
		// 		else if (TopSpeed > maximumForwardSpeed && currentSpeed < maximumForwardSpeed/2f){
		// 			m_Animator.SetBool("Walking", false);
		// 		}
		// 	}
		// 	prevValue = currentTurn;
		// }
    // }
}
