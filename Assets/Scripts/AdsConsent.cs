using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

// Souhlas s reklamami (Google UMP) + jedina inicializace AdMob.
// Reklamy se smi nacitat az po souhlasu: misto MobileAds.Initialize volej AdsConsent.WhenAdsReady(...).
// Formular se zobrazi jen tam, kde je vyzadovany (napr. EU/UK); jinde se pokracuje hned.
public class AdsConsent : MonoBehaviour
{
    private static AdsConsent instance;
    private static bool gathering;
    private static bool consentDone;
    private static bool initStarted;
    private static bool adsReady;
    private static readonly List<Action> waiting = new List<Action>();

    // Callbacky UMP/AdMob mohou prijit z jineho vlakna -> zpracuji se v Update na hlavnim vlakne.
    private static readonly Queue<Action> mainThread = new Queue<Action>();

    // Ma se v aplikaci nabizet tlacitko "Privacy settings"? (vyzaduje Google v EU)
    public static bool PrivacyOptionsRequired
    {
        get
        {
            return consentDone &&
                   ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        }
    }

    // Spusti zjisteni souhlasu (bezpecne volat opakovane).
    public static void Gather()
    {
        EnsureInstance();
        if (consentDone || gathering) return;
        gathering = true;

        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        var request = new ConsentRequestParameters { TagForUnderAgeOfConsent = false };

        ConsentInformation.Update(request, (FormError updateError) => RunOnMain(() =>
        {
            if (updateError != null)
            {
                Debug.LogWarning("UMP update failed: " + updateError.Message);
                FinishConsent();
                return;
            }
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) => RunOnMain(() =>
            {
                if (formError != null) Debug.LogWarning("UMP form failed: " + formError.Message);
                FinishConsent();
            }));
        }));
    }

    // Zavola action, az bude souhlas vyreseny a AdMob inicializovany.
    // Pokud uzivatel reklamy nepovolil (CanRequestAds == false), action se nezavola.
    public static void WhenAdsReady(Action action)
    {
        if (action == null) return;
        if (adsReady)
        {
            action();
            return;
        }
        waiting.Add(action);
        Gather();
        if (consentDone) TryInitAds();
    }

    // Formular "Privacy settings" (zmena souhlasu).
    public static void ShowPrivacyOptions()
    {
        ConsentForm.ShowPrivacyOptionsForm((FormError error) => RunOnMain(() =>
        {
            if (error != null) Debug.LogWarning("UMP privacy options failed: " + error.Message);
            TryInitAds(); // souhlas mohl byt prave udelen
        }));
    }

    private static void FinishConsent()
    {
        gathering = false;
        consentDone = true;
        TryInitAds();
    }

    private static void TryInitAds()
    {
        if (initStarted || !ConsentInformation.CanRequestAds()) return;
        initStarted = true;
        MobileAds.Initialize((InitializationStatus status) => RunOnMain(() =>
        {
            adsReady = true;
            var list = waiting.ToArray();
            waiting.Clear();
            foreach (var a in list)
            {
                try { a(); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }));
    }

    private static void RunOnMain(Action a)
    {
        lock (mainThread) mainThread.Enqueue(a);
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;
        var go = new GameObject("AdsConsent");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AdsConsent>();
    }

    private void Update()
    {
        while (true)
        {
            Action a;
            lock (mainThread)
            {
                if (mainThread.Count == 0) break;
                a = mainThread.Dequeue();
            }
            a();
        }
    }
}
