using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class FootstepsHandler : MonoBehaviour
{
    [Header("Footsteps Collections")]
    [SerializeField] private List<AudioClip> footstepSounds = new List<AudioClip>(); // List of footstep sounds
    [SerializeField] private AudioClip landingClip; // Sound played when landing

    [Header("Footsteps Parameters")]
    [SerializeField] [Range(0f, 1f)] private float m_RunStepLengthen; // How much faster the character runs compared to walking
    [SerializeField] private float m_StepInterval; // Base time interval between steps
    private float m_StepCycle; 
    private float m_NextStep;
    [SerializeField] private float crouchStepSpeed = 1.75f;// crouch walk
    [SerializeField] private float walkStepSpeed = 1.85f; // all walk
    [SerializeField] private float runStepSpeed = 3f; // run forward only

    [Header("Required Components")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerLocomotionPreset playerLocomotionPreset;
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] FootstepsSwapper footstepsSwapper;

    #region Steps Cycles & Audio Methods

    private void ProgressStepCycle(float speed)
    {
        if (characterController.velocity.sqrMagnitude > 0 && (playerLocomotion.moveInput.x != 0 || playerLocomotion.moveInput.y != 0))
        {
            m_StepCycle += (characterController.velocity.magnitude + (speed * ((playerLocomotion.isWalking || playerLocomotion.isWalkingBackwards) ? 1f : m_RunStepLengthen))) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        footstepsSwapper.CheckLayers();

        if (playerLocomotion.canMove)
        {
            if (!characterController.isGrounded)
            {
                return;
            }

            footstepsSwapper.UpdateFootstepType(playerLocomotion.isRunning);

            int n = Random.Range(1, footstepSounds.Count);
            footstepAudioSource.clip = footstepSounds[n];
            footstepAudioSource.PlayOneShot(footstepAudioSource.clip);
            // Move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = footstepAudioSource.clip;
        }
    }

    private void PlayLandingAudio()
    {
        footstepsSwapper.CheckLayers();
        footstepAudioSource.PlayOneShot(landingClip);
    }

    public void SwapFootsteps(FootstepsCollection collection)
    {
        footstepSounds.Clear();
   
        if (playerLocomotion.isRunning)
        {
            for (int i = 0; i < collection.runFootstepSounds.Count; i++)
            {
                footstepSounds.Add(collection.runFootstepSounds[i]);
            }
        }
        else
        {
            for (int i = 0; i < collection.walkFootstepSounds.Count; i++)
            {
                footstepSounds.Add(collection.walkFootstepSounds[i]);
            }
        }

        landingClip = collection.landSound;
    }

    #endregion

    #region Unity Methods

    private void OnValidate()
    {
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();

        }

        if (playerLocomotion == null)
        {
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        }

        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
        }

        if (footstepsSwapper == null)
        {
            footstepsSwapper = GetComponent<FootstepsSwapper>();
        }
    }

    private void Start()
    {
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
    }

    private void Update()
    {   
        if (playerLocomotion.isCrouching)
        {   
            footstepAudioSource.pitch = 1.2f;
            footstepAudioSource.volume = 0.2f;
        }
        else
        {
            footstepAudioSource.pitch = 1.1f;
            footstepAudioSource.volume = 0.4f;
        }

        // Adjust step interval based on player state
        if (playerLocomotion.isRunning && !playerLocomotion.isWalkingBackwards)
        {
            m_StepInterval = runStepSpeed;
        }
        else if (playerLocomotion.isCrouching)
        {
            m_StepInterval = crouchStepSpeed;
        }
        else
        {
            m_StepInterval = walkStepSpeed;
        }

        // Landing sound
        if (characterController.isGrounded && playerLocomotion.airTime > 0.3f)
        {
            PlayLandingAudio();
        }
    }

    private void FixedUpdate()
    {
        ProgressStepCycle(speed: playerLocomotion.isRunning ? playerLocomotionPreset.runSpeed : playerLocomotionPreset.walkSpeed);
    }


    #endregion
}
