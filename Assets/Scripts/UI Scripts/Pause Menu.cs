using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public MainMenuManager menu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //menu.Options();
        }
    }
}
