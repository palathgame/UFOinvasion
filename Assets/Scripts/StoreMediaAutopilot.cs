#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;
using UnityEditor.Media;

// JEN PRO EDITOR (do buildu se nedostane): autopilot pro snimky a video na web / do obchodu.
// Spousti ho menu Tools > Store Media (StoreMediaCapture). Projde menu, spusti hru, lita s lodi
// (nesmrtelna), privola bosse, nahodi power-up a minci a pri tom dela snimky obrazovky.
public class StoreMediaAutopilot : MonoBehaviour
{
    public const string TriggerKey = "StoreMediaCapture";
    public const string DoneKey = "StoreMediaCaptureDone";
    public const string OutDirKey = "StoreMediaOutDir";

    private string outDir;
    private float gameStart = -1f;
    private PlaneShip player;
    private PlaneScript planeControl;

    // Video (UnityEditor.Media.MediaEncoder - soucast Unity, zadny balicek navic).
    private const int Fps = 30, VideoW = 1280, VideoH = 720;
    private const float VideoSeconds = 32f;
    private MediaEncoder encoder;
    private NativeArray<float> audioBuffer;
    private RenderTexture videoRt;
    private Texture2D videoFrame;
    private bool recording;
    private string pendingShot;
    private int frames;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (PlayerPrefs.GetString(TriggerKey) != "1") return;
        PlayerPrefs.DeleteKey(TriggerKey);
        var go = new GameObject("StoreMediaAutopilot");
        DontDestroyOnLoad(go);
        go.AddComponent<StoreMediaAutopilot>();
    }

    private IEnumerator Start()
    {
        outDir = PlayerPrefs.GetString(OutDirKey);
        if (string.IsNullOrEmpty(outDir)) outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "StoreMedia"));
        Directory.CreateDirectory(outDir);

        // --- Menu
        yield return new WaitForSecondsRealtime(1f);
        var startMenu = FindAnyObjectByType<StartPageManager>();
        if (startMenu != null) startMenu.nickName.GetComponent<InputField>().text = "Pilot";
        yield return new WaitForSecondsRealtime(1f);
        yield return Shot("1-menu");

        var help = FindAnyObjectByType<PowerUpHelp>();
        if (help != null)
        {
            help.GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("2-power-ups");
            var panel = GameObject.Find("PowerUpHelpPanel");
            if (panel != null) Destroy(panel);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        var menu = FindAnyObjectByType<StartPageManager>();
        if (menu == null)
        {
            Finish();
            yield break;
        }
        var nick = menu.nickName.GetComponent<InputField>();
        nick.text = "Pilot"; // puvodni prezdivku vrati StoreMediaCapture po nahravani
        menu.StartGame();

        // --- Hra
        while (SceneManager.GetActiveScene().name != "Game") yield return null;
        gameStart = Time.time;
        StartVideo();

        yield return WaitGame(9f);
        yield return Shot("3-action");

        // Privolat bosse (stejne jako drivejsi testovaci prepinac).
        var manager = FindAnyObjectByType<EnemyManager>();
        var chance = typeof(EnemyManager).GetField("chanceToBoos", BindingFlags.NonPublic | BindingFlags.Instance);
        if (manager != null && chance != null) chance.SetValue(manager, 200);

        // Varovani "BOSS INCOMING!"
        float waitUntil = Time.time + 6f;
        while (Time.time < waitUntil && !IsActive("BossWarning")) yield return null;
        yield return new WaitForSeconds(0.6f);
        yield return Shot("4-boss-warning");

        // Boss
        waitUntil = Time.time + 10f;
        while (Time.time < waitUntil && FindAnyObjectByType<EnemyShipBoss>() == null) yield return null;
        yield return new WaitForSeconds(3.5f);
        yield return Shot("5-boss");

        // Power-up (magnet) a mince pred lodi
        SpawnPickups();
        yield return new WaitForSeconds(0.9f);
        yield return Shot("6-power-up");

        yield return WaitGame(36f);
        yield return Shot("7-combat");

        yield return WaitGame(40f);
        Finish();
    }

    private void Update()
    {
        if (gameStart < 0f) return;

        if (player == null)
        {
            player = FindAnyObjectByType<PlaneShip>();
            planeControl = FindAnyObjectByType<PlaneScript>();
            if (planeControl != null) planeControl.enabled = false; // mys/prst neovlada lod
        }
        if (player == null) return;

        // Nesmrtelnost (HP bar zustava plny).
        player.hp = 1000000;

        // Autopilot: plynuly pohyb nahoru/dolu a mirne dopredu/dozadu.
        if (planeControl != null)
        {
            float t = Time.time - gameStart;
            var target = new Vector2(-5.5f + 1.3f * Mathf.Sin(t * 0.6f), 2.6f * Mathf.Sin(t * 1.05f));
            // Mirit na nejblizsiho nepritele pred lodi (at to vypada jako opravdova hra).
            EnemyShip best = null;
            float bestDx = float.MaxValue;
            foreach (var e in FindObjectsByType<EnemyShip>(FindObjectsSortMode.None))
            {
                if (e is PlaneShip) continue;
                float dx = e.transform.position.x - planeControl.transform.position.x;
                if (dx > 0.5f && dx < bestDx) { bestDx = dx; best = e; }
            }
            if (best != null) target.y = Mathf.Clamp(best.transform.position.y, -3.5f, 3.5f);
            planeControl.transform.position = Vector2.MoveTowards(planeControl.transform.position, target, 6f * Time.deltaTime);
        }
    }

    private void SpawnPickups()
    {
        if (player == null) return;
        var enemy = FindAnyObjectByType<EnemyShipMove>();
        Vector3 ahead = player.transform.position + new Vector3(3.5f, 0.6f, 0f);
        if (enemy != null && enemy.shieldBuff != null) MagnetBuff.Spawn(enemy.shieldBuff, ahead);
        if (enemy != null && enemy.coin != null)
        {
            var coin = Instantiate(enemy.coin);
            coin.transform.position = ahead + new Vector3(1.2f, -1.4f, 0f);
        }
    }

    private static bool IsActive(string name)
    {
        var go = GameObject.Find(name);
        return go != null && go.activeInHierarchy;
    }

    private IEnumerator WaitGame(float seconds)
    {
        while (Time.time - gameStart < seconds) yield return null;
    }

    private IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        string path = Path.Combine(outDir, name + ".png");
        if (recording)
        {
            // Behem nahravani by druhe zachyceni obrazovky selhalo -> snimek ulozi CaptureVideo z tehoz framu.
            pendingShot = path;
            while (pendingShot != null && recording) yield return null;
            if (pendingShot == null) yield break;
            pendingShot = null;
            yield return new WaitForEndOfFrame();
        }
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Destroy(tex);
        Debug.Log("[StoreMedia] screenshot " + name);
    }

    // ---------- video ----------

    private void StartVideo()
    {
        string path = Path.Combine(outDir, "trailer.mp4");
        if (File.Exists(path)) File.Delete(path);
        Time.captureFramerate = Fps; // plynulych 30 fps bez ohledu na rychlost Editoru
        videoRt = new RenderTexture(VideoW, VideoH, 0, RenderTextureFormat.ARGB32);
        videoFrame = new Texture2D(VideoW, VideoH, TextureFormat.RGBA32, false);
        encoder = new MediaEncoder(path,
            new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps), width = VideoW, height = VideoH,
                includeAlpha = false, bitRateMode = UnityEditor.VideoBitrateMode.Low
            },
            new AudioTrackAttributes
            {
                sampleRate = new MediaRational(AudioSettings.outputSampleRate), channelCount = 2, language = "en"
            });
        AudioRenderer.Start();
        recording = true;
        StartCoroutine(CaptureVideo());
        Debug.Log("[StoreMedia] recording video");
    }

    private IEnumerator CaptureVideo()
    {
        while (recording)
        {
            yield return new WaitForEndOfFrame();
            if (!recording) break;

            int samples = AudioRenderer.GetSampleCountForCaptureFrame() * 2;
            if (!audioBuffer.IsCreated || audioBuffer.Length != samples)
            {
                if (audioBuffer.IsCreated) audioBuffer.Dispose();
                audioBuffer = new NativeArray<float>(samples, Allocator.Persistent);
            }
            AudioRenderer.Render(audioBuffer);

            // Obrazovka -> 1280x720 (zmenseni pres RenderTexture).
            var screen = ScreenCapture.CaptureScreenshotAsTexture();
            if (pendingShot != null)
            {
                File.WriteAllBytes(pendingShot, screen.EncodeToPNG());
                Debug.Log("[StoreMedia] screenshot " + Path.GetFileNameWithoutExtension(pendingShot));
                pendingShot = null;
            }
            Graphics.Blit(screen, videoRt);
            Destroy(screen);
            var prev = RenderTexture.active;
            RenderTexture.active = videoRt;
            videoFrame.ReadPixels(new Rect(0, 0, VideoW, VideoH), 0, 0);
            videoFrame.Apply();
            RenderTexture.active = prev;

            encoder.AddFrame(videoFrame);
            encoder.AddSamples(audioBuffer);
            frames++;
            if (frames >= VideoSeconds * Fps) StopVideo();
        }
    }

    private void StopVideo()
    {
        if (!recording) return;
        recording = false;
        AudioRenderer.Stop();
        encoder.Dispose();
        encoder = null;
        if (audioBuffer.IsCreated) audioBuffer.Dispose();
        Time.captureFramerate = 0;
        Debug.Log("[StoreMedia] video saved, frames=" + frames);
    }

    private void OnDestroy()
    {
        StopVideo(); // Play Mode ukoncen driv -> video se aspon uzavre
    }

    private void Finish()
    {
        StopVideo();
        PlayerPrefs.SetString(DoneKey, "1");
        Debug.Log("[StoreMedia] done");
    }
}
#endif
