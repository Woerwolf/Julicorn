﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagment : MonoBehaviour
{
    public GameObject arrowPrefab;
    private GameObject arrow;

    public Text highscoreTxt;
    public Text coinsTxt;
    public Button startGame, shop;
    public int endingDuration = 50;
    public bool endingGame, ended, goldiEvent, goldiAniStarted, goldiAniFinished;
    private int frames;
    private float playerGravityScale;
    private float ShopDespawnTimer;
    private bool ShopDespawn;

    // Animators
    Animator ani;
    Animator aniUI;

    // Start is called before the first frame update
    void Start()
    {
        ended = false;
        endingGame = false;
        frames = 0;
        shop.onClick.AddListener(StartShop);
        startGame.onClick.AddListener(StartGame);
        playerGravityScale = GameObject.Find("Player").GetComponent<Rigidbody2D>().gravityScale;

        // get animators
        aniUI = GameObject.Find("Canvas").GetComponent<Animator>();
        ani = GameObject.Find("Main Camera").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if canvas is up in the shop, if so spawn arrow if not spawned already
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("CamUp") && !arrow)
        {
            arrow = Instantiate(arrowPrefab, GameObject.Find("MenuCanvas").transform);
            Button arrowbtn = arrow.GetComponent<Button>();
            GameManagment gm = GameObject.Find("GameManager").GetComponent<GameManagment>();
            arrowbtn.onClick.AddListener(gm.StopShop);
            print(arrowbtn.onClick.ToString());
        }
        // if arrowDestroy animation ended, destroy arrow
        if(arrow && arrow.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ArrowEnd")) {
            Destroy(arrow);
        }
        if (endingGame)
        {
            StopGame();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            StartGame();
        }
    }

    //--------------------  GAME  -----------------------

    public void StartGame()
    {
        // Despawn old shop
        GameObject.Find("ShopSpawner").GetComponent<ShopSpawner>().Despawn();
        GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().frames = 0;
        // Delete all tubes
        List<GameObject> toDelete = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().coins;
        for (int i = toDelete.Count; i > 0; i--) {
            Destroy(toDelete[i - 1]);
        }
        ClearWood();
        // Set check variables to false and reset player and score
        GameObject.Find("Cloud").GetComponent<ObstacleMover>().speed.x = 0;
        GameObject.Find("Cloud").GetComponent<ObstacleMover>().SetPos(new Vector3(-6.78f, -2.32f));
        GameObject.Find("Score").GetComponent<ScoreUpdater>().Reset();
        endingGame = false;
        ended = false;
        frames = 0;
        GameObject.Find("Player").GetComponent<Player_Move>().Setactive();
        GameObject.Find("Player").GetComponent<Player_Move>().gameStoped = false;
        GameObject.Find("Player").transform.position = new Vector3(-6.7f, -1, 0);
        // Animation to the ground
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            ani.Play("SwipeCamDown");
        }
        //SceneChanger sc = GameObject.Find("Fader").GetComponent<SceneChanger>();
        //sc.FadeToScene(1);
    }


    public void StopGame()
    {
        // Do animation after waiting endingDuration
        if (!ended && frames >= endingDuration && !goldiEvent)
        {
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("CamDown"))
            {
                ani.Play("SwipeCamUp");
            }
            ended = true;
        }

        // Goldi animation

        if(!ended && frames >= endingDuration && goldiEvent && !goldiAniStarted && !goldiAniFinished) {
            GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().StartGoldiAnimation();
            goldiAniStarted = true;
        }

        if (!ended && frames >= endingDuration && goldiEvent && goldiAniStarted && !goldiAniFinished) {
            if (GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().StepGoldiAnimation()) {
                GameObject player = GameObject.Find("Player");
                player.GetComponent<Player_Move>().playerInitialized = false;
                player.GetComponent<Player_Move>().gameStoped = false;
                Player_Move.firstGoldiGame = true;
                player.GetComponent<Player_Move>().jumpable = true;
                Player_Move.goldiEvent = false;
                endingGame = false;
                goldiAniStarted = false;
                goldiAniFinished = true;
                goldiEvent = false;
                frames = 0;
                return;
            }
        }

        // end Goldi animation

        // Stop the Game in the first frame after collision
        if (frames == 0)
        {
            List<GameObject> giveBorder = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().tubes;
            for(int i = giveBorder.Count; i>0; i--) {
                if (giveBorder[i - 1] != null)
                {
                    giveBorder[i - 1].GetComponent<ObstacleMover>().SetYBorder(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y);
                }
            }
            GameObject.Find("Score").GetComponent<ScoreUpdater>().counting = false;
            GameObject.Find("Player").GetComponent<Rigidbody2D>().gravityScale = 0;
            GameObject.Find("Player").GetComponent<Player_Move>().playerInitialized = false;
            GameObject.Find("Player").GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            GameObject.Find("Player").GetComponent<Player_Move>().gameStoped = true;
            //GameObject.Find("Background").GetComponent<BG_Move>().speed = 0;
            GameObject.Find("Foreground").GetComponent<BG_Move>().speed = 0;
            GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().active = false;
            int temp = -1;
            ScoreUpdater scr = GameObject.Find("Score").GetComponent<ScoreUpdater>();
            temp = scr.GetScore();
            if (DataManagement.highscore < temp)
            {
                DataManagement.highscore = temp;
            }
            WriteStats();
        }

        // count frames
        frames++;
    }

    public void WriteStats()
    {
        // Write new highscore and new coin balance to home screen
        highscoreTxt.text = DataManagement.highscore.ToString();
        coinsTxt.text = DataManagement.coins.ToString();
        Save_Load.SaveData();
    }

    public void ClearWood() {
        List<GameObject> toFall = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().tubes;
        float acc = 1;
        float grav = 1;
        for (int i = toFall.Count; i > 0; i--)
        {
            if (i % 2 == 0)
            {
                acc = Random.Range(60, 500);
                grav = Random.Range(200, 500);
            }
            if (toFall[i - 1] != null)
            {
                toFall[i - 1].GetComponent<ObstacleMover>().TreeFall(acc, grav);
            }
        }
    }


    //--------------------  SHOP  -----------------------

    public void StartShop() {
        // Despawn old shop
        GameObject.Find("ShopSpawner").GetComponent<ShopSpawner>().Despawn();
        //// Delete all coins; saw trees down (same part as in "StartGame")
        //List<GameObject> toDelete = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>().coins;
        //for (int i = toDelete.Count; i > 0; i--)
        //{
        //    Destroy(toDelete[i - 1]);
        //}
        //ClearWood();

        // Spawn all the shop items
        GameObject.Find("ShopSpawner").GetComponent<ShopSpawner>().Spawn();
        // Animation to the ground
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            ani.Play("SwipeCamUpShop");
        }
    }

    public void StopShop() {
        print("StopShop");
        // Animate arrow to Destroy it after it disappeared
        arrow.GetComponent<Animator>().Play("ArrowDestroy");
        // Animation back up
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("CamUp"))
        {
            ani.Play("SwipeCamDownShop");
        }
    }
}
