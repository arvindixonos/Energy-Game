using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public enum PlayerState
    {
        CONNECTED,
        READY,
        ANSWEWRING,
        ANSWERED,
        COMPLETE,
        FINISH
    }

    public  GameObject[]        uiScreens;
    public  Transform           loading;
    public  NetworkDiscovery    discovery;
    public  Server              server;
    public  Image               countdownImage;
    public  Color               toColor;
    public  VideoPlayer         player1VideoPlane;
    public  VideoPlayer         player2VideoPlane;
    public  GameObject          player1Glass;
    public  GameObject          player2Glass;
    public  VideoClip[]         positiveClips;
    public  VideoClip[]         negativeClips;
    public  Sprite[]            countdownSprites;
    public  Material[]          player1Materials;
    public  Material[]          player2Materials;

    private GameObject          prevScreen;
    public int                  player1ID = -1;
    public int                  player2ID = -1;
    public int                  player1Points;
    public int                  player2Points;
    public PlayerState          player1State;
    public PlayerState          player2State; 

    void Start()
    {
        discovery.Initialize();
        discovery.StartAsServer();

        StartCoroutine("Initialise");
    }

    IEnumerator FadeInMat (int questionNo, bool isPlayer1)
    {
        yield return new WaitForSeconds(3f);
        if(isPlayer1)
        {
            player1Materials[questionNo].DOColor(toColor, 1f).OnUpdate(() =>
            {
                player1Materials[questionNo].SetColor("_EmissionColor", player1Materials[questionNo].GetColor("_Color"));
            });
        }
        else
        {
            player2Materials[questionNo].DOColor(toColor, 1f).OnUpdate(() =>
            {
                player2Materials[questionNo].SetColor("_EmissionColor", player2Materials[questionNo].GetColor("_Color"));
            });
        }
    }

    void ResetMat ()
    {
        foreach(Material mat in player1Materials)
        {
            mat.DOKill();
            mat.SetColor("_Color", Color.black);
            mat.SetColor("_EmissionColor", Color.black);
        }
        foreach (Material mat in player2Materials)
        {
            mat.DOKill();
            mat.SetColor("_Color", Color.black);
            mat.SetColor("_EmissionColor", Color.black);
        }
    }

    IEnumerator Initialise ()
    {
        loading.DOKill();
        loading.DOBlendableRotateBy(new Vector3(0f, 0f, -10f) * 10, 2, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental);
        yield return null;
    }

    public void StartCountdown ()
    {
        ResetMat();
        Sequence countdownSequence = DOTween.Sequence();
        countdownSequence.Append(countdownImage.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce));
        countdownSequence.Append(countdownImage.transform.DOScale(0f, 1f).OnComplete(() =>
        {
            countdownImage.sprite = countdownSprites[1];
        }));
        countdownSequence.Append(countdownImage.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce));
        countdownSequence.Append(countdownImage.transform.DOScale(0f, 1f).OnComplete(() =>
        {
            countdownImage.sprite = countdownSprites[2];
        }));
        countdownSequence.Append(countdownImage.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce));
        countdownSequence.Append(countdownImage.transform.DOScale(0f, 1f).OnComplete(() =>
        {
            countdownImage.sprite = countdownSprites[3];
            countdownImage.SetNativeSize();
        }));
        countdownSequence.Append(countdownImage.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce));
        countdownSequence.Append(countdownImage.transform.DOScale(0f, 1f).OnComplete(() =>
        {
            countdownImage.sprite = countdownSprites[0];
            countdownImage.SetNativeSize();
            player1State = PlayerState.ANSWEWRING;
            player2State = PlayerState.ANSWEWRING;
            server.SendMessageToAll("StartQuiz");
        }));
        countdownSequence.Play();
    }

    void ResetTimer ()
    {
        player1State = PlayerState.READY;
        player2State = PlayerState.READY;

        server.SendMessageToAll("Restart");

        RestartApp();
    }
    int randomNumber = 0;
    public void OnMessageReceived (string Message, int playerID)
    {
        CancelInvoke("ResetTimer");
        Invoke("ResetTimer", 90f);
        switch(Message)
        {
            case "LanguageSelected":
            if(playerID == player1ID)
            {
                player1State = PlayerState.READY;
                if (player1State == PlayerState.READY && player2State == PlayerState.READY)
                {
                    uiScreens[1].SetActive(false);
                    uiScreens[2].SetActive(false);
                    uiScreens[3].SetActive(true);

                    StartCountdown();
                }
                else
                {
                    uiScreens[1].SetActive(false);
                    uiScreens[2].SetActive(true);

                    player2Points = 0;
                    player1Points = 0;
                }
            }
            else if(playerID == player2ID)
            {
                player2State = PlayerState.READY;
                if (player1State == PlayerState.READY)
                {
                    uiScreens[1].SetActive(false);
                    uiScreens[2].SetActive(false);
                    uiScreens[3].SetActive(true);

                    if (player1State == PlayerState.READY && player2State == PlayerState.READY)
                        StartCountdown();
                    }
                else
                {
                    uiScreens[1].SetActive(false);
                    uiScreens[2].SetActive(true);

                    player2Points = 0;
                    player1Points = 0;
                }
            }

            if(randomNumber == 0)    
                randomNumber = Random.Range(1, 4);
            if (playerID == player1ID && player1State == PlayerState.READY)
            {
                server.SendMessage(player1ID, "use_" + randomNumber);
            }
            if (playerID == player2ID && player2State == PlayerState.READY)
            {
                server.SendMessage(player2ID, "use_" + randomNumber);
            }
            break;
            case "Restart":
                if(!uiScreens[2].activeInHierarchy && !uiScreens[3].activeInHierarchy)
                {
                    uiScreens[1].SetActive(true);
                }
                server.SendMessage(playerID, "Restart");
                RestartApp();
            break;
            case "SetNextQuestion":
                if (playerID == player1ID)
                {
                    player1State = PlayerState.ANSWERED;
                }
                if (playerID == player2ID)
                {
                    player2State = PlayerState.ANSWERED;
                }
                if (player1State == PlayerState.ANSWERED && player2State == PlayerState.ANSWERED)
                {
                    player1State = PlayerState.ANSWEWRING;
                    player2State = PlayerState.ANSWEWRING;
                    server.SendMessageToAll("SetNextQuestion");
                }
                break;
            case "Finish":
                if(playerID == player1ID)
                {
                    player1State = PlayerState.FINISH;
                }
                else
                {
                    player2State = PlayerState.FINISH;
                }
                if(player1State == PlayerState.FINISH && player2State == PlayerState.FINISH)
                {
                    CalculateScores();
                }
            break;
            case "TurnOffGlass":
                if(player1ID == playerID)
                {
                    TurnOffGlassP1();
                }
                else
                {
                    TurnOffGlassP2();
                }
            break;
            case "Quit":
                if(playerID == player1ID)
                {
                    server.SendMessage(player2ID, "Winner");
                    player1State = PlayerState.COMPLETE;

                    TurnOffGlassP1();
                }
                else
                {
                    server.SendMessage(player1ID, "Winner");
                    player1State = PlayerState.COMPLETE;

                    TurnOffGlassP2();
                }
            break;
            default:                //QuestionNumber_true
            if (!Message.Contains("_"))
                return;

            string[] answer = Message.Split('_');
            int qNo = int.Parse(answer[0]) - 1;

            if(answer[1] == "true")
            {
                if(playerID == player1ID)
                {
                    player1VideoPlane.clip = positiveClips[qNo];
                    player1VideoPlane.Play();

                    player1Points++;

                    player1Glass.SetActive(true);
                    StartCoroutine(FadeInMat(qNo, true));
                    if(qNo == 5)
                    {
                        player1State = PlayerState.COMPLETE;
                    }
                }
                else
                {
                    player2VideoPlane.clip = positiveClips[qNo];
                    player2VideoPlane.Play();

                    player2Points++;

                    player2Glass.SetActive(true);
                    StartCoroutine(FadeInMat(qNo, false));
                    if (qNo == 5)
                    {
                        player2State = PlayerState.COMPLETE;
                    }
                }
            }
            else
            {
                if (playerID == player1ID)
                {
                    player1VideoPlane.clip = negativeClips[qNo];
                    player1VideoPlane.Play();

                    player1Glass.SetActive(true);
                    if (qNo == 5)
                    {
                        player1State = PlayerState.COMPLETE;
                    }
                }
                else
                {
                    player2VideoPlane.clip = negativeClips[qNo];
                    player2VideoPlane.Play();

                    player2Glass.SetActive(true);
                    if (qNo == 5)
                    {
                        player2State = PlayerState.COMPLETE;
                    }
                }
            }
            break;
        }
    }

    public void RestartApp ()
    {
        if (player1State == PlayerState.READY && player2State == PlayerState.READY)
        {
            ResetMat();

            for (int i = 1; i < 4; i++)
            {
                uiScreens[i].SetActive(false);
            }
            uiScreens[1].SetActive(true);
            player1State = PlayerState.CONNECTED;
            player2State = PlayerState.CONNECTED;
            randomNumber = 0;
        }
    }

    void CalculateScores ()
    {
        if(player1Points > player2Points)
        {
            server.SendMessage(player1ID, "Winner");
            server.SendMessage(player2ID, "Loser");
            player1State = PlayerState.COMPLETE;
            player2State = PlayerState.COMPLETE;
        }
        if(player2Points > player1Points)
        {
            server.SendMessage(player2ID, "Winner");
            server.SendMessage(player1ID, "Loser");
            player1State = PlayerState.COMPLETE;
            player2State = PlayerState.COMPLETE;
        }
        if(player1Points == player2Points)
        {
            server.SendMessage(player2ID, "Tie");
            server.SendMessage(player1ID, "Tie");
            player1State = PlayerState.COMPLETE;
            player2State = PlayerState.COMPLETE;
        }
        TurnOffGlassP1();
        TurnOffGlassP2();
    }

    void OnApplicationQuit ()
    {
        ResetMat();
    }

    void TurnOffGlassP1()
    {
        player1Glass.SetActive(false);
        player1VideoPlane.Stop();
    }

    void TurnOffGlassP2()
    {
        player2Glass.SetActive(false);
        player2VideoPlane.Stop();
    }

    public void OnPlayerConnected (int playerID)
    {
        uiScreens[0].SetActive(false);
        prevScreen = uiScreens[1];
        uiScreens[1].SetActive(true);

        if (player1ID != -1)
        {
            player2ID = playerID;
            player2State = PlayerState.CONNECTED;
        }
        else
        {
            player1ID = playerID;
            player1State = PlayerState.CONNECTED;
        }
    }
}
