
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
        if (score > HighScore || gameManager.GetBossesThisGame() > 0) ReviewPrompt.MarkGoodGame();
        destroyShips(gameManager, killsAlliens);
        oneGameKills(gameManager, killsAlliens);

        lastScoreCoins = gameManager.AwardScoreCoins(score);
        kills = killsAlliens;
        baseText = txt.text;
        layoutWindow();
        showSummary();
        createDoubleCoinsOffer();
        QuestToast.Show(DailyQuests.ReportGame(killsAlliens, gameManager.GetBossesThisGame(),
            gameManager.GetCoinsThisGame(), score, gameManager.GetBestMultiplier()));

        PlayerPrefs.Save();

    }

    private long kills;
    private long lastScoreCoins;
    private string baseText;
    private GameObject doubleCoinsButton;

    // Tlacitko "x2 coins" nad radou tlacitek: reklama za odmenu zdvoji mince z teto hry.
    private void createDoubleCoinsOffer()
    {
        if (!gameManager.CanDoubleCoins) return;

        GameObject adObj = GameObject.Find("AdIns");
        Rewarded rewarded = adObj != null ? adObj.GetComponent<Rewarded>() : null;
        if (rewarded == null || !rewarded.readyAdd()) return;

        BackMenu backMenu = GetComponent<BackMenu>();
        if (backMenu == null) backMenu = FindAnyObjectByType<BackMenu>();
        if (backMenu == null || backMenu.continueGame == null) return;

        // Kopie tlacitka "Watch ad to continue" (ma textovy popisek) -> stejny vzhled jako zbytek menu.
        doubleCoinsButton = Instantiate(backMenu.continueGame.gameObject, backMenu.continueGame.transform.parent, false);
        doubleCoinsButton.SetActive(true);
        doubleCoinsButton.name = "DoubleCoins";
        var rt = doubleCoinsButton.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 160);
        rt.sizeDelta = new Vector2(620, 90);

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
        string coins = "Coins  <color=#FFD633>+" + gameManager.GetCoinsThisGame() + "</color>";
        if (lastScoreCoins > 0) coins += "  <size=36>(+" + lastScoreCoins + " for score)</size>";
        t.text = "<size=76>" + baseText + "</size>\n"
                + "<size=50>Kills  " + kills
                + "     Bosses  " + gameManager.GetBossesThisGame()
                + "     Best combo  x" + gameManager.GetBestMultiplier()
                + "\n" + coins + "</size>";
    }

    // Okno vysledku: vetsi panel, skore nahore, statistiky pod nim, tlacitka v rade dole.
    private void layoutWindow()
    {
        Text t = txt.GetComponent<Text>();
        bool newHigh = t.color == Color.red;
        var panel = t.transform.parent as RectTransform;
        if (panel == null) return;

        panel.sizeDelta = new Vector2(1100, 620);
        var panelImg = panel.GetComponent<Image>();
        if (panelImg != null)
        {
            panelImg.color = new Color(0.08f, 0.1f, 0.2f, 0.93f);
            var outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.2f);
            outline.effectDistance = new Vector2(4, -4);
        }

        // Text: horni cast panelu nad tlacitky.
        var trt = t.rectTransform;
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -30);
        trt.sizeDelta = new Vector2(-60, 330);
        t.supportRichText = true;
        t.resizeTextForBestFit = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.alignment = TextAnchor.MiddleCenter;
        t.lineSpacing = 1.1f;
        t.color = newHigh ? new Color(1f, 0.85f, 0.2f) : Color.white;
        if (newHigh) baseText = "NEW HIGH SCORE!\n" + score;
        var shadow = t.GetComponent<Shadow>();
        if (shadow == null) shadow = t.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(3, -3);

        // Tlacitka: [Watch ad to continue]  [restart]  [Menu]
        BackMenu menu = GetComponent<BackMenu>();
        if (menu == null) menu = FindAnyObjectByType<BackMenu>();
        if (menu == null) return;
        placeButton(menu.continueGame, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40, 40), new Vector2(380, 110));
        placeButton(menu.restart, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(130, 110));
        placeButton(menu.yourButton, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40, 40), new Vector2(300, 110));
    }

    private static void placeButton(Button b, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        if (b == null) return;
        var rt = b.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var label = b.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 20;
            label.resizeTextMaxSize = Mathf.Max(label.fontSize, 60);
            var lrt = label.rectTransform;
            lrt.offsetMin = new Vector2(10, 6);
            lrt.offsetMax = new Vector2(-10, -6);
        }
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
