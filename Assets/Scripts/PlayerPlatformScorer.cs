using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformScorer : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private string platformTag = "Platform";
    [SerializeField] private float minLandingNormalY = 0.5f;

    private readonly HashSet<Collider> countedPlatforms = new HashSet<Collider>();

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(platformTag))
            return;

        // Make sure player is landing on top, not touching side or bottom
        bool landedOnTop = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.normal.y >= minLandingNormalY)
            {
                landedOnTop = true;
                break;
            }
        }

        if (!landedOnTop)
            return;

        if (countedPlatforms.Contains(collision.collider))
            return;

        countedPlatforms.Add(collision.collider);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPlatformScore();
    }
}