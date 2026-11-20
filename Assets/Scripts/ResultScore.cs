
using UnityEngine;
using UnityEngine.UI;

public class ResultScore : MonoBehaviour
{
    public Text txt;
    private long score;
    private long HighScore;
    private InGameManager gameManager;
    public GameObject UpdateScore;

    void Start()
    {
        gameManager = FindAnyObjectByType<InGameManager>();
        long killsAlliens;
        killsAlliens = gameManager.GetComponent<InGameManager>().getKillsAliens();

        highScore();
        destroyShips(gameManager, killsAlliens);
        oneGameKills(gameManager, killsAlliens);

        lastScoreCoins = gameManager.AwardScoreCoins(score);
        kills = killsAlliens;
        baseText = txt.text;
        showSummary();
        createDoubleCoinsOffer();

        PlayerPrefs.Save();

    }

    private long kills;
    private long lastScoreCoins;
    private string baseText;
    private GameObject doubleCoinsButton;

    // Tlacitko "x2 coins" nad tlacitkem Restart: reklama za odmenu zdvoji mince z teto hry.
    private void createDoubleCoinsOffer()
    {
        if (!gameManager.CanDoubleCoins) return;

        GameObject adObj = GameObject.Find("AdIns");
        Rewarded rewarded = adObj != null ? adObj.GetComponent<Rewarded>() : null;
        if (rewarded == null || !rewarded.readyAdd()) return;

        BackMenu backMenu = GetComponent<BackMenu>();
        if (backMenu == null) backMenu = FindAnyObjectByType<BackMenu>();
        if (backMenu == null || backMenu.restart == null) return;

        // Kopie tlacitka Restart -> stejny vzhled jako zbytek menu.
        doubleCoinsButton = Instantiate(backMenu.restart.gameObject, backMenu.restart.transform.parent, false);
        doubleCoinsButton.name = "DoubleCoins";
        var rt = doubleCoinsButton.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 130);
        rt.sizeDelta = new Vector2(460, 90);

        Text label = doubleCoinsButton.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = "Watch ad: x2 coins (+" + gameManager.GetCoinsThisGame() + ")";
            label.resizeTextForBestFit = true;
            label.resizeTextMaxSize = label.fontSize;
        }

        Button btn = doubleCoinsButton.GetComponent<Button>();
        btn.onClick = new Button.ButtonClickedEvent();
        btn.onClick.AddListener(() =>
        {
            rewarded.ShowRewardedForCoins(() =>
            {
                gameManager.DoubleCoins();
                PlayerPrefs.Save();
                showSummary();
                if (doubleCoinsButton != null) Destroy(doubleCoinsButton);
                // Reklama je spotrebovana -> "pokracovat za reklamu" uz nejde.
                if (backMenu != null && backMenu.continueGame != null) backMenu.continueGame.gameObject.SetActive(false);
            });
        });
    }

    private void OnDestroy()
    {
        if (doubleCoinsButton != null) Destroy(doubleCoinsButton);
    }



    private void highScore() {


        score = FindAnyObjectByType<Score>().getScore();

        string userNick = PlayerPrefs.GetString("Nick");
        FindAnyObjectByType<HighscoresUpload>().AddNewHighscore(userNick, score);


        string stringScore = PlayerPrefs.GetString("HighScore");

        if (stringScore == "")
        {

            HighScore = 0L;
        }
        else
        {

            HighScore = long.Parse(stringScore);

        }


        if (HighScore < score)
        {

            txt.GetComponent<UnityEngine.UI.Text>().text = "Your score is " + score + ", congratulation its new high score.";
            txt.GetComponent<UnityEngine.UI.Text>().color = Color.red;
            PlayerPrefs.SetString("HighScore", score.ToString());

        }
        else
        {

            txt.GetComponent<UnityEngine.UI.Text>().text = "Your score is " + score;
        }

    }


    private void showSummary()
    {
        Text t = txt.GetComponent<Text>();
        t.verticalOverflow = VerticalWrapMode.Overflow;
        string coins = "Coins: +" + gameManager.GetCoinsThisGame();
        if (lastScoreCoins > 0) coins += "  (+" + lastScoreCoins + " for score)";
        t.text = baseText
                + "\nKills: " + kills
                + "   Bosses: " + gameManager.GetBossesThisGame()
                + "   Best combo: x" + gameManager.GetBestMultiplier()
                + "\n" + coins;
    }

    private void destroyShips(InGameManager gameManager,long killsAlliens)
    {
        
        string killsAlliensBefore = PlayerPrefs.GetString("KillAliens");

        if (killsAlliensBefore == "") { killsAlliensBefore = "0"; }

        killsAlliens = killsAlliens + long.Parse(killsAlliensBefore);

        PlayerPrefs.SetString("KillAliens", killsAlliens.ToString());
    }



    private void oneGameKills(InGameManager gameManager,long killsAlliens) {


        string killsAlliensBefore = PlayerPrefs.GetString("KillAliensOneGame");
        if (killsAlliensBefore == "") { killsAlliensBefore = "0"; }


        if (long.Parse(killsAlliensBefore) < killsAlliens)
        {

            PlayerPrefs.SetString("KillAliensOneGame", killsAlliens.ToString());
        }


    }



}
