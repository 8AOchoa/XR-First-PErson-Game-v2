using UnityEngine;
using System.Collections;

public class GhostAI : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float speed = 2f; // Speed when following the player
    public float hoverHeight = 1.5f; // Floating effect
    public float detectionRange = 10f; // Distance before ghost starts following
    public float stopRange = 2f; // How close the ghost gets before stopping
    public float wanderSpeed = 1f; // Speed when wandering
    public float wanderRadius = 5f; // Radius around start position for wandering
    public float followDuration = 7f; // Time the ghost follows before wandering
    public AudioSource creepySound; // Spooky sound effect

    private Vector3 startPosition;
    private bool isChasing = false;
    private bool isWandering = true;
    private bool hasPlayedSound = false; // Tracks if the sound has played recently

    void Start()
    {
        startPosition = transform.position;
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("⚠️ No player found! Make sure the player has a 'Player' tag.");
            }
        }

        if (creepySound != null)
        {
            creepySound.loop = false; // Make sure it does NOT loop
            creepySound.Stop();
        }

        StartCoroutine(WanderRandomly()); // Start with wandering
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange && !isChasing)
        {
            StartCoroutine(FollowForLimitedTime());

            if (!hasPlayedSound) // Play sound only if it hasn't played recently
            {
                PlayCreepySound();
                hasPlayedSound = true; // Mark sound as played
                StartCoroutine(ResetSoundCooldown()); // Allow it to play again later
            }
        }
    }

    private IEnumerator FollowForLimitedTime()
    {
        isChasing = true;
        isWandering = false;
        Debug.Log("👻 Ghost is following you...");

        float elapsedTime = 0;
        while (elapsedTime < followDuration)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stopRange)
            {
                MoveTowards(player.position, speed);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("👻 Ghost got bored and is wandering...");
        isChasing = false;
        isWandering = true;
        StartCoroutine(WanderRandomly());
    }

    private IEnumerator WanderRandomly()
    {
        while (isWandering)
        {
            Vector3 wanderTarget = startPosition + new Vector3(
                Random.Range(-wanderRadius, wanderRadius), 
                0, 
                Random.Range(-wanderRadius, wanderRadius)
            );

            float wanderTime = Random.Range(4f, 8f);
            float elapsedTime = 0;

            while (elapsedTime < wanderTime)
            {
                MoveTowards(wanderTarget, wanderSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(1f, 3f)); // Pause before picking a new spot
        }
    }

    private void MoveTowards(Vector3 target, float moveSpeed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Keep ghost floating at the same height
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void PlayCreepySound()
    {
        if (creepySound != null && !creepySound.isPlaying)
        {
            creepySound.Play(); // Play sound ONCE when the player gets near
        }
    }

    private IEnumerator ResetSoundCooldown()
    {
        yield return new WaitForSeconds(5f); // Wait 5 seconds before allowing the sound again
        hasPlayedSound = false; // Now the sound can play again when the player gets near
    }
}
