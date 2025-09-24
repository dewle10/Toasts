using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WormMenager : MonoBehaviour
{
    public static WormMenager instance;

    [SerializeField]
    private List<Worm> worms;
    [SerializeField]
    private Worm[] wormsArray;
    [SerializeField]
    private List<WormHealth> wormsHealth;
    [SerializeField]
    private WormHealth[] wormsHealthArray;
    [SerializeField]
    private bool[] deadWorms;

    [SerializeField]
    private GameObject wormPrefab;

    [SerializeField]
    private int currentWorm;

    public CinemachineVirtualCamera vCam;
    public TMP_Text[] teamHPTexts;
    public GameObject[] teamTexts;

    public int numberOfTeams;
    private int teamsID;

    private int[] teamsHP;


    private bool bulletInAir;
    private bool wormWating;
    
    public float nextWormTime = 10f;
    private float nextWormCounter;

    private int numberOfWormsTeam0;
    private int numberOfWormsTeam1;
    private int numberOfWormsTeam2;
    private int numberOfWormsTeam3;

    private int numberOfDeadWormsTeam0;
    private int numberOfDeadWormsTeam1;
    private int numberOfDeadWormsTeam2;
    private int numberOfDeadWormsTeam3;

    public GameObject WinText0;
    public GameObject WinText1;
    public GameObject WinText2;
    public GameObject WinText3; 
    public GameObject WinTextDraw;

    private int losingTeams;
    private bool gameOver;

    private bool loseTeam0;
    private bool loseTeam1;
    private bool loseTeam2;
    private bool loseTeam3;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        for (int i = 0; i < Menu.menuNumberOfWorms * Menu.menuNumberOfTeams; i++)
        {
            Instantiate(wormPrefab);
        }

        numberOfTeams = Menu.menuNumberOfTeams;

        wormsArray = GameObject.FindObjectsOfType<Worm>();
        foreach (Worm wormsArray in wormsArray)
        {
            worms.Add(wormsArray);
        }

        wormsHealthArray = GameObject.FindObjectsOfType<WormHealth>();
        foreach (WormHealth wormsHealthArray in wormsHealthArray)
        {
            wormsHealth.Add(wormsHealthArray);
        }

        deadWorms = new bool[wormsArray.Length];

        teamsHP = new int[numberOfTeams];

        for (int i = 0; i < worms.Count; i++)
        {
            worms[i].ChangeID(i);
        }

        for (int i = 0; i < worms.Count; i++)
        {
            teamsID++;
            if (teamsID == numberOfTeams)
                teamsID = 0;

            worms[i].ChangeTeam(teamsID);
        }
        for (int i = 0; i < teamHPTexts.Length; i++)
        {
            if (i >= numberOfTeams)
            {
                teamHPTexts[i].text = null;
                teamTexts[i].SetActive(false);
            }
        }

        for (int i = 0; i < worms.Count; i++)
        {
            if (wormsArray[i].GetTeamID() == 0)
            {
                numberOfWormsTeam0++;
            }
            else if (wormsArray[i].GetTeamID() == 1)
            {
                numberOfWormsTeam1++;
            }
            else if (wormsArray[i].GetTeamID() == 2)
            {
                numberOfWormsTeam2++;
            }
            else if (wormsArray[i].GetTeamID() == 3)
            {
                numberOfWormsTeam3++;
            }
        }
    }

    private void Start()
    {
        UpdateTeamHP();
        NextWorm();

    }

    private void Update()
    {
        if (bulletInAir)
        {
            nextWormCounter += Time.deltaTime;
            if (nextWormCounter >= nextWormTime)
            {
                EndNextWormCounter();
                NextWorm();
            }
        }
    }

    public void StartNextWormCounter()
    {
        bulletInAir = true;
        wormWating = true;
    }
    public void EndNextWormCounter()
    {
        nextWormCounter = 0f;
        bulletInAir = false;
    }

    public void UpdateTeamHP()
    {
        for (int i = 0; i < teamsHP.Length; i++)
        {
            teamsHP[i] = 0;
        }

        for (int i = 0; i < wormsHealth.Count; i++)
        {
            teamsHP[worms[i].GetTeamID()] += wormsHealth[i].GetWormHP();
        }
        UpdateTexts();
    }

    public void UpdateTexts()
    {
        for (int i = 0; i < numberOfTeams; i++)
        {
            teamHPTexts[i].text = teamsHP[i].ToString();
        }
    }

    public void RemoveWorm(int id)
    {
        deadWorms[id] = true;
    }


    public bool IsMyTurn(int i)
    {
        return i == currentWorm;
    }

    public void NextWorm()
    {
        StartCoroutine(_NextWorm());
    }

    public IEnumerator _NextWorm()
    {
        yield return new WaitForSeconds(0.1f);

        int nextWorm = currentWorm + 1;
        currentWorm -= 1;
        currentWorm = nextWorm;
        if (currentWorm >= worms.Count)
            currentWorm = 0;

        if (!deadWorms[currentWorm])
        {
            yield return new WaitForSeconds(2f);

            vCam.Follow = worms[currentWorm].transform;
            wormWating = false;
        }
        else
        {
            NextWorm();
        }

        ZeroDeadWorms();
        AddDeadWorms();
        CheckGameOver();
    }

    private void AddDeadWorms()
    {
        for (int i = 0; i < worms.Count; i++)
        {
            if (deadWorms[i] == true && wormsArray[i].GetTeamID() == 0)
            {
                numberOfDeadWormsTeam0++;
            }
            if (deadWorms[i] == true && wormsArray[i].GetTeamID() == 1)
            {
                numberOfDeadWormsTeam1++;
            }
            if (deadWorms[i] == true && wormsArray[i].GetTeamID() == 2)
            {
                numberOfDeadWormsTeam2++;
            }
            if (deadWorms[i] == true && wormsArray[i].GetTeamID() == 3)
            {
                numberOfDeadWormsTeam3++;
            }
        }
    }

    private void ZeroDeadWorms()
    {
        numberOfDeadWormsTeam0 = 0;
        numberOfDeadWormsTeam1 = 0;
        numberOfDeadWormsTeam2 = 0;
        numberOfDeadWormsTeam3 = 0;
    }
        private void CheckGameOver()
    {
        if (numberOfDeadWormsTeam0 == numberOfWormsTeam0 && !loseTeam0)
        {
            losingTeams++;
            loseTeam0 = true;
        }
        if (numberOfDeadWormsTeam1 == numberOfWormsTeam1 && !loseTeam1)
        {
            losingTeams++;
            loseTeam1 = true;
        }
        if (numberOfDeadWormsTeam2 == numberOfWormsTeam2 && !loseTeam2)
        {
            losingTeams++;
            loseTeam2 = true;
        }
        if (numberOfDeadWormsTeam3 == numberOfWormsTeam3 && !loseTeam3)
        {
            losingTeams++;
            loseTeam3 = true;
        }

        if (losingTeams == 4)
        {
            WinTextDraw.SetActive(true);
            gameOver = true;
        }
        else if (losingTeams == 3)
        {
            if (numberOfDeadWormsTeam0 != numberOfWormsTeam0)
            {
                WinText0.SetActive(true);
                gameOver = true;
            }
            else if (numberOfDeadWormsTeam1 != numberOfWormsTeam1)
            {
                WinText1.SetActive(true);
                gameOver = true;
            }
            else if (numberOfDeadWormsTeam2 != numberOfWormsTeam2)
            {
                WinText2.SetActive(true);
                gameOver = true;
            }
            else if (numberOfDeadWormsTeam3 != numberOfWormsTeam3)
            {
                WinText3.SetActive(true);
                gameOver = true;
            }
        }
    }
    public bool IswormWating() { return wormWating; }
    public bool IsGameOver() { return gameOver; }
}
