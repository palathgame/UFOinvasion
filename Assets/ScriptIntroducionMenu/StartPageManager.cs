
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPageManager : MonoBehaviour
{
    // public GameObject ads;
    public GameObject setting;
    public GameObject achiev;
    public GameObject accessories;
    public GameObject leaderboard;
    public GameObject nickName;
    public GameObject difficulty;
    

    //then drag and drop the Username_field
    private long games;

    private void Start()
    {
        string userNick = PlayerPrefs.GetString("Nick");
        nickName.GetComponent<InputField>().text = userNick;
        AdsConsent.Gather(); // souhlas s reklamami (formular jen kde je povinny)
        DailyReward.CheckAndShow();
        // Zadost o recenzi jen kdyz zrovna nevyskocila denni odmena.
        if (FindAnyObjectByType<DailyReward>() == null) ReviewPrompt.TryShow();
        PowerUpHelp.Create(nickName.transform);
        DailyQuestsMenu.Create(nickName.transform);
    }

    // Na mobilu se appka casto jen obnovi z pozadi (scena se znovu nenacte),
    // takze denni odmenu je potreba zkontrolovat i pri navratu do popredi.
    private void OnApplicationPause(bool paused)
    {
        if (!paused) DailyReward.CheckAndShow();
    }
    public void StartGame() {

        

        string userNick = nickName.GetComponent<InputField>().text;

        int diffGame = difficulty.GetComponent<SetDific>().getDif();


        if (userNick != "")
        {
            PlayerPrefs.SetString("Nick", userNick);
        }
        else {
            nickName.GetComponent<InputField>().Select();
            return;
        
        }
        

        Score.scoreValue = 0;
        SceneManager.LoadScene("Game");

        string stringGames = PlayerPrefs.GetString("Games");

        if (stringGames == "")
        {

            games = 1L;
        }
        else
        {

            games = long.Parse(stringGames)+1;

        }
        PlayerPrefs.SetString("Games", games.ToString());
        PlayerPrefs.Save();

      //  Destroy(ads);

    }

    public void ExitGame()
    {
        
        Application.Quit();

    }

    public void viewAchiv()
    {
        Instantiate(achiev);

    }

    public void viewAccessories()
    {
        Instantiate(accessories);

    }

    public void viewLeaderboard()
    {
        Instantiate(leaderboard);

    }

    public void viewSetting()
    {
        Instantiate(setting);

    }
}
