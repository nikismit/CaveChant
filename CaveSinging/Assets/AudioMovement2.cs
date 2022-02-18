using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMovement2 : MonoBehaviour {

	public AudioPitch pitch;
	
	int currentPitch;
	float currentAmp;
	[Header("Movement Speeds")]
	public float maximumForwardSpeed;
	public float forwardAccelaration;
	public float forwardDeceleration;

	public float turningSpeed;
	public float turningDeceleration = 1;
	private float turningSpeedSave;

	public float currentTurn;
	public float currentSpeed;
	private float startMaxSpeed;
	private float startAccel;
	private float startDecel;
	public bool hasStopped;
	
	[Header("Options")]
	public float minimumPitch;
	public float maximumPitch;
	public bool highPitchIsTurnRight;
	public bool amplitudeControlsSpeed;
	public bool soundTriggersParticles;
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
	

    void Start()
    {
        
        	m_Rigidbody = GetComponent<Rigidbody>();
	

		startMaxSpeed = maximumForwardSpeed;
		startAccel = forwardAccelaration;
		startDecel = forwardDeceleration;

		turningSpeedSave = turningSpeed;
		SteeringSpeedSave = SteeringSpeed;
		SavedFallingRate = 0f;
		torchLight.intensity = 0;
		fillLight.intensity = 0;
		
    }


	private void Turning(){
		xAngle = turningSpeed * currentTurn * Time.fixedDeltaTime;
		car.transform.Rotate(-xAngle, 0, yAngle, Space.Self);
	}
	private void NoTurning (){
		xAngle = 0;
		car.transform.Rotate(xAngle, -yAngle, 0, Space.Self);
	}

	private void Buffer(){
		if ( properRotation >= 270 && properRotation <= 360 - angleOfFlight){
			turningSpeed = 0;
			hasStopped = true;			
		}
		else if (properRotation >= angleOfFlight && properRotation <= 90){
			turningSpeed = 0;
			hasStopped = true;
		}
		if (prevValue < currentTurn && hasStopped == true){
			turningSpeed = turningSpeedSave;
			hasStopped = false;
		}
		if (prevValue > currentTurn && hasStopped == true){
			turningSpeed = turningSpeedSave;
			hasStopped = false;
		}
	}

	private void Falling(){
		car.transform.Translate(-transform.up * SavedFallingRate * Time.fixedDeltaTime, Space.World);
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


    void FixedUpdate()
    {
		if (walkingIsTrue == true) {
			if (currentSpeed <= 0){
				if (fillLight.intensity >= 0f){
					fillLight.intensity -= 0.005f;
				}
				if (torchLight.intensity >= 0f){
					torchLight.intensity -= 0.005f;
				}
				glow.alpha.a -= 0.002f;
			}
		}
		currentPitch = pitch._currentpublicpitch;
		currentAmp = pitch._currentPublicAmplitude;

		if(mouseIsSteering == true){
			Steering ();
			SteerWithSpeed ();
		}

		properRotation = car.transform.eulerAngles.x;
		
			
		if(currentPitch > minimumPitch){ 
			currentTurn = (((currentPitch-minimumPitch)/(maximumPitch-minimumPitch))*2)-1; 
			
			if(highPitchIsTurnRight == false){
				currentTurn *= -1;
			}
			if(amplitudeControlsSpeed == true){
				maximumForwardSpeed = currentAmp+10;
				forwardAccelaration = (currentAmp+10)/2;
				forwardDeceleration = (currentAmp+10)/2;
			} else {
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
				if (torchLight.intensity <= 0.8){
					torchLight.intensity += 0.01f;
					if (glow.alpha.a < 0.5){
						glow.alpha.a += 0.01f;
					}
				}
				if (fillLight.intensity <= 0.3f){
					fillLight.intensity += 0.01f;
				}

				
			}
			else {
				Buffer();
				Turning();
			}
			SavedFallingRate = FallingRate;
			
			prevValue = currentTurn;
			TopSpeed = currentSpeed;

		} else if(currentSpeed >= 0) {
			currentSpeed -= forwardDeceleration *Time.fixedDeltaTime;
			if(currentTurn > 0){
				currentTurn -= Time.fixedDeltaTime *turningDeceleration;
			}else if(currentTurn < 0){
				currentTurn += Time.fixedDeltaTime *turningDeceleration;
			}
			
			this.transform.Translate(transform.forward * currentSpeed * Time.fixedDeltaTime, Space.World);

			if (walkingIsTrue == true) {
				NoTurning();
				if (torchLight.intensity <= 1.1f){
					torchLight.intensity -= 0.005f;
					glow.alpha.a -= 0.002f;
				}
				if (fillLight.intensity <= 0.4f){
					fillLight.intensity -= 0.005f;
				}
				
				if (TopSpeed <= 0.8f){
					m_Animator.SetBool("Walking", false);
				}
				else if (TopSpeed > 0.8f && currentSpeed < 0.4f){  
					m_Animator.SetBool("Walking", false);
				}
			}
			else {
				Falling ();
				Buffer();
				Turning();
			}
			prevValue = currentTurn;
		}
    }
}
