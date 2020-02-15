using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
	public	NetworkDiscovery	discovery;
	public	GameObject		    startScreen;
	public	GameObject		    languageScreen;
    public  GameObject          gameOverScreen;
    public	GameObject[]	    questions_1;
    public  GameObject[]        questions_2;
    public  GameObject[]        questions_3;
    public	GameObject[]	    answers_1;
    public  GameObject[]        answers_2;
    public  GameObject[]        answers_3;
    public  GameObject[]        englishTexts;
    public  GameObject[]        arabicTexts;
    public  Transform[]         progress;
    public	GameObject		    winnerScreeen;
	public	GameObject		    loserScreeen;
	public	GameObject		    tieScreeen;
	public	Client			    clientMessenger;
    public  bool                partnerQuestionCompleted;

	private	GameObject		    currentQuestion;
	private	GameObject		    currentAnswer;
    private GameObject          resultPage;
	private	int				    currentQuestionPage = 0;
	private	int				    currentAnswerPage = 0;
	private	bool			    serverIPSet;
    private Sequence            repeatSequence;
    private Sequence            appearSequence;
    private GameObject          nextButton;
    private bool                isEnglish;

    public	string			    ipAddress;
    public GameObject[]         currentAnswers;
    public GameObject[]         currentQuestions;

    void Start ()
    {
#if !UNITY_EDITOR
		discovery.Initialize ();
		discovery.StartAsClient ();

		StartCoroutine("SetServerIP");
#else
        Connect();
#endif
	}

    void DoStarAnimation ()
    {
        progress[0].parent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -260, 0);
        progress[0].parent.localScale = Vector3.one * 1.3f;
        progress[0].parent.GetComponent<UnityEngine.UI.Image>().enabled = false;

        appearSequence = DOTween.Sequence();
        repeatSequence = DOTween.Sequence();

        foreach (Transform prog in progress)
        {
            prog.localScale = Vector3.zero;
        }
        foreach(Transform prog in progress)
        {
            appearSequence.Append(prog.DOScale(1f, 0.1f).SetEase(Ease.OutBounce));
        }
        foreach (Transform prog in progress)
        {
            repeatSequence.Append(prog.DOScale(1.3f, 0.1f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo));
        }
        appearSequence.Play().OnComplete(() =>
        {
            repeatSequence.SetLoops(-1);
            repeatSequence.Play();
        });
    }

    void StopStarAnimation()
    {
        appearSequence.Kill();
        repeatSequence.Kill();

        progress[0].parent.GetComponent<UnityEngine.UI.Image>().enabled = true;
        progress[0].parent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -607, 0);
        progress[0].parent.localScale = Vector3.one;
        foreach (Transform prog in progress)
        {
            prog.localScale = Vector3.one;
        }
    }

	public void ReScanIP ()
	{
		serverIPSet = false;
		StopCoroutine("SetServerIP");
		StartCoroutine("SetServerIP");
	}

	IEnumerator SetServerIP()
    {
        while (!serverIPSet)
        {
            foreach (string key in discovery.broadcastsReceived.Keys)
            {
                NetworkBroadcastResult networkBroadcastResult = discovery.broadcastsReceived[key];

                ipAddress = networkBroadcastResult.serverAddress.Substring(7, networkBroadcastResult.serverAddress.Length - 7);
				Connect();
				break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

	void Connect ()
	{
		serverIPSet = true;
		clientMessenger.ip = ipAddress;
		clientMessenger.CreateClient();
	}

	public void StartApp ()
	{
		languageScreen.SetActive(true);
	}

	public void SelectLanguage (bool isEnglish)
	{
        this.isEnglish = isEnglish;
        if (isEnglish)
        {
            foreach (GameObject text in englishTexts)
            {
                text.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject text in arabicTexts)
            {
                text.SetActive(true);
            }
        }
    }

	public void SetWaterQuestions ()
	{
        progress[0].parent.gameObject.SetActive(true);

		if(currentQuestion != null)
			currentQuestion.SetActive(false);
		else
			currentQuestions[currentQuestionPage].SetActive(true);

		currentQuestion = currentQuestions[currentQuestionPage];
        int toSwap = Random.Range(0, 2);
        if(toSwap == 1)
        {
            currentQuestion.GetComponent<Swapper>().toSwap = true;
        }
		currentQuestion.SetActive(true);
		currentQuestionPage++;
        clientMessenger.SendMessage("TurnOffGlass");
	}

    public void SetNextQuestion()
    {
        clientMessenger.SendMessage("SetNextQuestion");
    }

	public void SetAnswers (string answerState)
	{
		string[] states = answerState.Split('_');
        if(currentAnswer != null)
        {
            currentAnswer.transform.GetChild(0).gameObject.SetActive(false);
            currentAnswer.transform.GetChild(1).gameObject.SetActive(false);
        }
		if(states[0] == "Right")
		{
            int number = int.Parse(states[1]) - 1;

            currentAnswer = currentAnswers[int.Parse(states[1]) - 1];

            progress[number].GetChild(0).gameObject.SetActive(true);
            progress[number].GetChild(1).gameObject.SetActive(false);

            currentAnswer.transform.GetChild(0).gameObject.SetActive(true);
			currentAnswer.transform.GetChild(1).gameObject.SetActive(false);

            if(isEnglish)
            {
                nextButton = currentAnswer.transform.GetChild(0).GetChild(0).GetChild(2).gameObject;
            }
            else
            {
                nextButton = currentAnswer.transform.GetChild(0).GetChild(1).GetChild(2).gameObject;
            }
        }
		else
		{
            int number = int.Parse(states[1]) - 1;

            currentAnswer = currentAnswers[int.Parse(states[1]) - 1];

            progress[number].GetChild(0).gameObject.SetActive(false);
            progress[number].GetChild(1).gameObject.SetActive(true);

            currentAnswer.transform.GetChild(1).gameObject.SetActive(true);
			currentAnswer.transform.GetChild(0).gameObject.SetActive(false);

            if (isEnglish)
            {
                nextButton = currentAnswer.transform.GetChild(1).GetChild(0).GetChild(2).gameObject;
            }
            else
            {
                nextButton = currentAnswer.transform.GetChild(1).GetChild(1).GetChild(2).gameObject;
            }
        }

        clientMessenger.SendMessage("SetNextQuestion");
    }

	public void OnMessageRecieved (string Message)
	{
        if(Message.Contains("_"))
        {
            string[] questions = Message.Split('_');
            int questionSeries = int.Parse(questions[1]);
            if (questionSeries == 1)
            {
                currentQuestions = questions_1;
                currentAnswers = answers_1;
            }
            if (questionSeries == 2)
            {
                currentQuestions = questions_2;
                currentAnswers = answers_2;
            }
            if (questionSeries == 3)
            {
                currentQuestions = questions_3;
                currentAnswers = answers_3;
            }
            languageScreen.SetActive(false);
            return;
        }

		switch(Message)
		{
			case "StartQuiz":
				SetWaterQuestions();
			break;
			case "SetNextQuestion"://Actually Enable Button
                //SetWaterQuestions();
                nextButton.SetActive(true);
                break;
			case "Winner":
                winnerScreeen.SetActive(true);
                DoStarAnimation();
                StartCoroutine("GameOver");
			break;
			case "Loser":
                loserScreeen.SetActive(true);
                DoStarAnimation();
                StartCoroutine("GameOver");
                break;
			case "Tie":
                tieScreeen.SetActive(true);
                DoStarAnimation();
                StartCoroutine("GameOver");
                break;
            case "Restart":
                // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                RestartApp();
                break;
		}
	}

    IEnumerator GameOver ()
    {
        yield return new WaitForSeconds(5f);
        gameOverScreen.SetActive(true);
        yield return new WaitForSeconds(10f);
        GameOverHomeButton();
    }

    public void HomeButton ()
    {
        if (currentAnswer != null)
        {
            currentAnswer.transform.GetChild(0).gameObject.SetActive(false);
            currentAnswer.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            currentAnswers[0].transform.GetChild(0).gameObject.SetActive(false);
            currentAnswers[0].transform.GetChild(1).gameObject.SetActive(false);
        }
        currentQuestion.SetActive(false);
        currentQuestion = null;
        currentQuestionPage = 0;

        RestartApp();
        clientMessenger.SendMessage("Quit");
    }

    public void GameOverHomeButton ()
    {
        StopCoroutine("GameOver");
        clientMessenger.SendMessage("Restart");
    }

    public void RestartApp ()
    {
        if (currentAnswer != null)
        {
            currentAnswer.transform.GetChild(0).gameObject.SetActive(false);
            currentAnswer.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            currentAnswers[0].transform.GetChild(0).gameObject.SetActive(false);
            currentAnswers[0].transform.GetChild(1).gameObject.SetActive(false);
        }
        if(currentQuestion != null)
            currentQuestion.SetActive(false);
        currentQuestion = null;

        currentQuestionPage = 0;
        currentAnswerPage = 0;
        currentQuestionPage = 0;

        winnerScreeen.SetActive(false);
        loserScreeen.SetActive(false);
        tieScreeen.SetActive(false);
        gameOverScreen.SetActive(false);

        progress[0].parent.gameObject.SetActive(false);

        clientMessenger.SendMessage("TurnOffGlass");
        StopAllCoroutines();

        StopStarAnimation();

        foreach (GameObject text in englishTexts)
        {
            text.SetActive(false);
        }
        foreach (GameObject text in arabicTexts)
        {
            text.SetActive(false);
        }
        foreach (Transform indicator in progress)
        {
            indicator.GetChild(0).gameObject.SetActive(false);
            indicator.GetChild(1).gameObject.SetActive(true);
        }
        StartApp();
    }

    public void FinishButton ()
    {
        clientMessenger.SendMessage("Finish");
    }
}
