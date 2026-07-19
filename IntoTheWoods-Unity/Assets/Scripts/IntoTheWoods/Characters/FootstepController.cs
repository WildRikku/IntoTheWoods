using UnityEngine;

public class FootstepController : MonoBehaviour
{
    public AudioClip[] footstepSounds; // Array to hold footstep sound clips
    public float minTimeBetweenFootsteps = 0.3f; // Minimum time between footstep sounds
    public float maxTimeBetweenFootsteps = 0.6f; // Maximum time between footstep sounds
    public int startIndex = 0;

    private AudioSource _audioSource; // Reference to the Audio Source component
    private bool _isWalking = false; // Flag to track if the player is walking
    private float _timeSinceLastFootstep; // Time since the last footstep sound

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>(); // Get the Audio Source component
    }

    private void Update()
    {
        // Check if the player is walking
        if (_isWalking)
        {
            // Check if enough time has passed to play the next footstep sound
            if (Time.time - _timeSinceLastFootstep >= Random.Range(minTimeBetweenFootsteps, maxTimeBetweenFootsteps))
            {
                // Play next footstep sound from the array
                AudioClip footstepSound = footstepSounds[startIndex];
                _audioSource.PlayOneShot(footstepSound);
                startIndex++;
                if (startIndex >= footstepSounds.Length) {
                    startIndex = 0;
                }
                _timeSinceLastFootstep = Time.time; // Update the time since the last footstep sound
            }
        }
    }

    // Call this method when the player starts walking
    public void StartWalking()
    {
        _isWalking = true;
    }

    // Call this method when the player stops walking
    public void StopWalking()
    {
        _isWalking = false;
    }
}
