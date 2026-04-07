using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        if (transform.position.y < GameManager.Instance.FallYLevel)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}