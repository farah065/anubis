using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public MeshRenderer renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHit()
    {
        Debug.Log("YOUUUUUUUUUUUUUUUUUUUUUUUCHHHHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        StartCoroutine(FlashRed());
    }

    private System.Collections.IEnumerator FlashRed()
    {
        renderer.material.color = Color.red;
        // Wait for a short duration
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = Color.white;
    }
}
