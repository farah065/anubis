using UnityEngine;

enum BossPhase
{
	Start,
	PhaseOne,
    Transition,
	PhaseTwo,
	Defeated
}

public class BossManager : MonoBehaviour
{
	float healthThreshold = 0.5f;
	BossPhase bossPhase = BossPhase.Start;
	public float currentHealth = 100f;
	public float maxHealth = 100f;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

	// Update is called once per frame
	void Update()
	{
		switch (bossPhase)
		{
			case BossPhase.PhaseOne:
				if (currentHealth <= maxHealth * healthThreshold)
				{
					bossPhase = BossPhase.Transition;
				}
				break;
			case BossPhase.Transition:
				//HandleTransition();
				break;
			case BossPhase.PhaseTwo:
				// Boss behavior for Phase Two
				if (currentHealth <= 0)
				{
					bossPhase = BossPhase.Defeated;
				}
				break;
			case BossPhase.Defeated:
				//HandleDefeat();
				break;
		}
	}
}
