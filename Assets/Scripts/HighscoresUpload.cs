using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HighscoresUpload : MonoBehaviour
{
	const string privateCode = "aPh5CNZz2UKJgmyzEL4XeArWar8J_8oUCwN4bAk_B5QQ";
	//const string publicCode = "62f262538f40bf84eca29727";
	const string webURL = "http://dreamlo.com/lb/";



	/*void Awake()
	{
		DownloadHighscores();
	}*/


    public void AddNewHighscore(string username, long score)
	{
		// Nulove nebo nesmyslne skore neposilame.
		if (score <= 0 || score > MaxScore) return;

		StartCoroutine(UploadNewHighscore(SanitizeNick(username), score));
	}

	private const int MaxNickLength = 16;
	private const long MaxScore = 100000000;

	// Povoli jen pismena, cisla, mezeru, '-' a '_' (dreamlo pouziva '|' a '/' jako oddelovace).
	private static string SanitizeNick(string nick)
	{
		var sb = new System.Text.StringBuilder();
		foreach (char c in (nick ?? "").Trim())
		{
			if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_') sb.Append(c);
			if (sb.Length >= MaxNickLength) break;
		}
		string result = sb.ToString().Trim();
		return result.Length > 0 ? result : "Player";
	}


    IEnumerator UploadNewHighscore(string username, long score)
	{
		using (UnityWebRequest www = UnityWebRequest.Get(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score))
		{
			yield return www.SendWebRequest();

			if (www.result == UnityWebRequest.Result.Success)
				Debug.Log("Upload Successful");
			else
			{
				Debug.Log("Error uploading: " + www.error);
			}
		}
	}

	/*public void DownloadHighscores()
	{
		StartCoroutine("DownloadHighscoresFromDatabase");
	}*/

	/*IEnumerator DownloadHighscoresFromDatabase()
	{
		WWW www = new WWW(webURL + publicCode + "/pipe/");
		yield return www;

		if (string.IsNullOrEmpty(www.error))
			FormatHighscores(www.text);
		else
		{
			Debug.Log("Error Downloading: " + www.error);
		}
	}*/

	/*void FormatHighscores(string textStream)
	{
		string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);


		for (int i = 0; i < entries.Length; i++)
		{
			string[] entryInfo = entries[i].Split(new char[] { '|' });
			string username = entryInfo[0];
			long score = long.Parse(entryInfo[1]);
			
			
		}
	}*/

}

