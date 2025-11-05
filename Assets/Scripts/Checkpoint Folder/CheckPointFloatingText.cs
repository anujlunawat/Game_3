using UnityEngine;
using TMPro;
using System.Collections;

public class CheckpointFloatingText : MonoBehaviour
{
    [Header("Floating Settings")]
    public float rotationSpeed = 50f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;

    [Header("Vanish Effect")]
    public float vanishUpwardSpeed = 3f;
    public float vanishDuration = 0.4f;

    private Vector3 startLocalPos;
    private bool isVanishing = false;

    private Transform player;

    void Start()
    {
        // Save start position (local to checkpoint)
        startLocalPos = transform.localPosition;

        // Try to find the player to face text toward
        var manager = FindAnyObjectByType<CheckpointManager>();
        if (manager != null && manager.car != null)
            player = manager.car.transform;
    }

    void Update()
    {
        if (isVanishing) return;

        // Floating up and down
        float newY = startLocalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);

        // Rotation
        //transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        ////Always face the car/ player
        //if (player != null)
        //{
        //    transform.LookAt(player);
        //    transform.Rotate(transform.rotation.x, 180, transform.rotation.z); // flip text to face forward
        //}
    }

    public void TriggerVanish()
    {
        if (!isVanishing)
            StartCoroutine(VanishEffect());
    }

    IEnumerator VanishEffect()
    {
        isVanishing = true;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < vanishDuration)
        {
            elapsed += Time.deltaTime;

            // Move upward slightly
            transform.localPosition += Vector3.up * vanishUpwardSpeed * Time.deltaTime;

            // Gradually shrink
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / vanishDuration);

            var text = GetComponent<TextMeshPro>();
            if (text != null)
                text.color = Color.Lerp(text.color, Color.yellow, 0.5f);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
