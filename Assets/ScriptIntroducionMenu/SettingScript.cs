using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    public Image MusicNot;
    public Image SoundNot;
    public GameObject Direction;

    private void Start()
    {
        CreateVibrationToggle();
    }

    // Prepinac vibraci vznikne kopii prepinace "Change direction" (nad nim).
    private void CreateVibrationToggle()
    {
        if (Direction == null || Direction.GetComponent<Toggle>() == null) return;

        var holder = new GameObject("tmp");
        holder.SetActive(false);
        GameObject clone = Instantiate(Direction, holder.transform);
        var dirScript = clone.GetComponent<Direction>();
        if (dirScript != null) DestroyImmediate(dirScript);
        clone.name = "vibration";

        Toggle toggle = clone.GetComponent<Toggle>();
        for (int i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++)
            toggle.onValueChanged.SetPersistentListenerState(i, UnityEventCallState.Off);

        Text label = clone.GetComponentInChildren<Text>(true);
        if (label != null) label.text = "Vibration";

        clone.transform.SetParent(Direction.transform.parent, false);
        Destroy(holder);
        var src = Direction.GetComponent<RectTransform>();
        var rt = clone.GetComponent<RectTransform>();
        rt.anchoredPosition = src.anchoredPosition + new Vector2(0, src.rect.height + 15);

        toggle.isOn = PlayerPrefs.GetString("VibrationOff") != "1";
        toggle.onValueChanged.AddListener(on =>
        {
            PlayerPrefs.SetString("VibrationOff", on ? "0" : "1");
            PlayerPrefs.Save();
            if (on) Haptics.Vibrate(0f);
        });
    }
   
    public void turnOffMusic() {

        var active = PlayerPrefs.GetString("MusicActive");
        if (active == "1")
        {
            active = "0";
            MusicNot.enabled = false;
        }
        else
        {
            active = "1";
            MusicNot.enabled = true;
        }

        PlayerPrefs.SetString("MusicActive", active);
        PlayerPrefs.Save();
        GameObject.Find("AudioManager").GetComponent<AudioManager>().setVolume();
    }
    public void turnOffSound()
    {
        var active = PlayerPrefs.GetString("SoundActive");

        if (active == "1")
        {
            active = "0";
            SoundNot.enabled = false;
        }
        else
        {
            active = "1";
            SoundNot.enabled = true;
        }
        PlayerPrefs.SetString("SoundActive", active);
        PlayerPrefs.Save();
        GameObject.Find("AudioManager").GetComponent<AudioManager>().setVolume();
        
    
}

    public void changeDirection()
    {
        var inGameToggle = GameObject.Find("direction");
        var active = "0";
        if (inGameToggle.GetComponent<Toggle>().isOn)
        {
            active = "1";
        }
        PlayerPrefs.SetString("Direction",active);
        PlayerPrefs.Save();
    }
}
