
using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Enemy enemy = FindFirstObjectByType<Enemy>();
            if (enemy == null)
            {
                // switch off collider
                GetComponent<Collider>().enabled = false;
                gameManager.LoadRandomLevel();
            }
        }
    }
}