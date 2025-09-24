using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static int menuNumberOfTeams;
    public static int menuNumberOfWorms;

    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject[] selectedIcons;

    private bool teamSelected = false;

    private void Start()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Play()
    {
        SceneManager.LoadScene("Menu2");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void SetTeams(int teams)
    {
        menuNumberOfTeams = teams;
        teamSelected = true;
        UnselectTeams();
        selectedIcons[teams - 2].SetActive(true); // 0= 2 teams, 1= 3 teams, 2= 4 teams
        UpdateStartButton();
    }

    public void SetWorms(int worms)
    {
        menuNumberOfWorms = worms;
        UnselectWorms();
        selectedIcons[worms + 2].SetActive(true); // 3= 1 worm, 4= 2 worms, 5= 3 worms, 6= 4 worms
        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        if (teamSelected && menuNumberOfWorms > 0)
            startButton.SetActive(true);
    }

    private void UnselectTeams()
    {
        for (int i = 0; i < 3; i++)
            selectedIcons[i].SetActive(false);
    }

    private void UnselectWorms()
    {
        for (int i = 3; i < selectedIcons.Length; i++)
            selectedIcons[i].SetActive(false);
    }
}
