using UnityEngine;

public class BoulderDoorScript : MonoBehaviour
{
    public GameObject[] targets;
    public Vector3 moveDirection = Vector3.up;
    public float liftDistance = 5f;
    public float liftSpeed = 2f;
    private bool doorOpened = false;
    private Vector3 startPosition;
    private float movedDistance = 0f;
    
    private AudioSource liftSound;
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        liftSound = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>(); // Get Rigidbody

        if (liftSound == null)
        {
            Debug.LogWarning("⚠️ No AudioSource found on the Boulder!");
        }

        if (rb == null)
        {
            Debug.LogError("❌ No Rigidbody found on the Boulder! Add one.");
        }
    }

    void Update()
    {
        if (!doorOpened && AllTargetsDestroyed())
        {
            StartLifting();
        }

        if (doorOpened && movedDistance < liftDistance)
        {
            LiftBoulder();
        }
    }

    private bool AllTargetsDestroyed()
    {
        foreach (GameObject target in targets)
        {
            if (target.activeInHierarchy)
                return false;
        }
        return true;
    }

    private void StartLifting()
    {
        Debug.Log("🪨 Boulder is lifting!");
        doorOpened = true;

        if (liftSound != null && liftSound.clip != null)
        {
            liftSound.PlayOneShot(liftSound.clip);
        }

        if (rb != null)
        {
            rb.isKinematic = true; // Enable kinematic mode so movement works
        }
    }

    private void LiftBoulder()
    {
        float moveStep = liftSpeed * Time.deltaTime;
        transform.position += moveDirection.normalized * moveStep;
        movedDistance += moveStep;

        if (movedDistance >= liftDistance)
        {
            Debug.Log("✅ Boulder fully lifted!");
            enabled = false; // Stop updating
        }
    }
}
