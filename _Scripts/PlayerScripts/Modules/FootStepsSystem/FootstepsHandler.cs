using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class FootstepsHandler : MonoBehaviour
{
    [SerializeField] private List<AudioClip> footstepSounds = new List<AudioClip>(); // List of footstep sounds
    [SerializeField] private AudioClip landingClip; // Sound played when landing

    [SerializeField] [Range(0f, 1f)] private float m_RunStepLengthen; // How much faster the character runs compared to walking
    [SerializeField] private float m_StepInterval; // Base time interval between steps

    private float m_StepCycle; // Time interval between steps
    private float m_NextStep;

    public float crouchStepSpeed = 1.75f;// crouch walk
    public float walkStepSpeed = 1.85f; // all walk
    public float runStepSpeed = 3f; // run forward only


    [Header("Components")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerLocomotionPreset playerLocomotionPreset;
    [SerializeField] AudioSource audioSource;
    [SerializeField] FootstepsSwapper footstepsSwapper;

    private void Start()
    {
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
    }
    private void Update()
    {   
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
            audioSource.clip = footstepSounds[n];
            audioSource.PlayOneShot(audioSource.clip);
            // Move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = audioSource.clip;
        }
    }

    private void PlayLandingAudio()
    {
        footstepsSwapper.CheckLayers();
        audioSource.PlayOneShot(landingClip);
    }

    public void SwapFootsteps(FootstepsCollection collection)
    {
        footstepSounds.Clear();
        //for (int i = 0; i < collection.footstepSounds.Count; i++)
        {
            //footstepSounds.Add(collection.footstepSounds[i]);
        }

        // Choose correct variation based on movement
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
            characterController = FindAnyObjectByType<CharacterController>();

        }

        if (playerLocomotion == null)
        {
            playerLocomotion = FindAnyObjectByType<PlayerLocomotion>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (footstepsSwapper == null)
        {
            footstepsSwapper = GetComponent<FootstepsSwapper>();
        }
    }

    #endregion
}
