using UnityEngine;

public class Target : MonoBehaviour
{
    public int health = 3; // Default health (adjust as needed)
    public ParticleSystem hitEffectPrefab; // Particle system for damage effect
    public AudioClip hitSound; // Sound for when hit
    public AudioClip destroySound; // Sound for when destroyed
    private AudioSource audioSource; // To play sounds

    private void Start()
    {
        // Add an AudioSource if not already present
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage()
    {
        health--;
        Debug.Log(gameObject.name + " hit! Remaining health: " + health);

        // Play hit effect
        if (hitEffectPrefab != null)
        {
            ParticleSystem hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect.gameObject, 2f); // Clean up after 2 seconds
        }

        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Disable target if health reaches 0
        if (health <= 0)
        {
            DestroyTarget();
        }
    }

    private void DestroyTarget()
    {
        Debug.Log(gameObject.name + " Destroyed!");

        // Play destroy sound first
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);

            // Delay disabling the target to allow the sound to play
            float soundLength = destroySound.length; // Get the length of the sound clip
            Invoke("DisableTarget", soundLength); // Wait for the sound to finish before disabling
        }
        else
        {
            // If no destroy sound, disable immediately
            DisableTarget();
        }
    }

    private void DisableTarget()
    {
        gameObject.SetActive(false); // Make the target disappear
    }
}