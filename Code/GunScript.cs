using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GunScript : MonoBehaviour
{
    public Transform gunTip;              // Tip of the gun barrel
    public LayerMask targetLayer;         // Layers to hit with raycast
    public float maxDistance = 100f;      // Max shooting distance
    public GameObject crosshair;          // The 3D crosshair object (Quad or Sprite)
    public float crosshairDistance = 2f;  // Distance in front of gun where crosshair appears

    public GameObject projectilePrefab;   // Prefab for the projectile (e.g., bullet or beam)
    public float projectileSpeed = 50f;   // Speed of the projectile
    public ParticleSystem muzzleFlashPrefab; // Particle system for muzzle flash
    public AudioClip shootSoundClip;      // Audio clip for shooting (instead of relying on AudioSource)

    private AudioSource audioSource;      // AudioSource for the gun
    private bool isAiming = false;        // Track aiming state
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable; // For detecting grab
    private bool isGrabbed = false;       // Track grab state

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("⚠️ No AudioSource found on the Gun! Added one automatically.");
        }

        // Set up grab detection
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("❌ No XRGrabInteractable on Gun! Crosshair won’t show on grab.");
        }
        else
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        // Hide crosshair initially
        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
    }

    private void Update()
    {
        var devices = InputSystem.devices;
        bool aimingNow = false;

        // Original shooting logic
        foreach (var device in devices)
        {
            if (device is UnityEngine.InputSystem.XR.XRController controller) // Fully qualified
            {
                foreach (var control in controller.allControls)
                {
                    if (control.name == "triggerpressed" && control is ButtonControl button)
                    {
                        if (button.isPressed)
                        {
                            aimingNow = true;
                        }

                        if (button.wasPressedThisFrame && isGrabbed) // Only shoot if grabbed
                        {
                            Debug.Log("🎯 Trigger Pressed! Shooting...");
                            Shoot();
                        }
                    }
                }
            }
        }

        // Original aiming state logic (optional, can remove if not needed)
        if (aimingNow != isAiming)
        {
            isAiming = aimingNow;
        }

        // Move crosshair if grabbed
        if (isGrabbed && crosshair != null)
        {
            Vector3 crosshairPos = gunTip.position + gunTip.forward * crosshairDistance;
            crosshair.transform.position = crosshairPos;
            crosshair.transform.rotation = Quaternion.LookRotation(gunTip.forward); // Face the crosshair forward
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        if (crosshair != null)
        {
            crosshair.SetActive(true); // Show crosshair
        }
        Debug.Log("🔫 Gun Grabbed - Crosshair ON");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (crosshair != null)
        {
            crosshair.SetActive(false); // Hide crosshair
        }
        Debug.Log("🔫 Gun Released - Crosshair OFF");
    }

    private void Shoot()
    {
        // Play shooting sound only if grabbed
        if (isGrabbed && shootSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSoundClip);
        }
        else if (shootSoundClip == null)
        {
            Debug.LogWarning("⚠️ No shoot sound clip assigned!");
        }

        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            ParticleSystem muzzleFlash = Instantiate(muzzleFlashPrefab, gunTip.position, gunTip.rotation);
            Destroy(muzzleFlash.gameObject, 0.5f); // Clean up after 0.5 seconds
        }

        // Shoot projectile
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, gunTip.position, gunTip.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = gunTip.forward * projectileSpeed;
            }
            Destroy(projectile, maxDistance / projectileSpeed + 0.1f); // Destroy after traveling max distance
        }

        RaycastHit hit;
        if (Physics.Raycast(gunTip.position, gunTip.forward, out hit, maxDistance, targetLayer))
        {
            Debug.Log("🎯 Hit: " + hit.collider.gameObject.name);

            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage();
            }
        }
        else
        {
            Debug.Log("❌ Missed! Nothing hit.");
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}