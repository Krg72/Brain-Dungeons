using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    public static SettingManager instance;

    [SerializeField] public UnityEngine.UI.Toggle musicOnTgl;
    [SerializeField] public UnityEngine.UI.Toggle soundOnTgl;

    //[SerializeField] public UnityEngine.UI.Toggle fpsTgl;
    [SerializeField] public UnityEngine.UI.Toggle vibrateTgl;
    
    [SerializeField] public UnityEngine.UI.Toggle FloatTextTgl;
    //[SerializeField] public UnityEngine.UI.Toggle FPSTextTgl;

    public bool SoundOn = true;
    public bool MusicOn = true;

    //public bool fps120On = true;
    public bool isvibrateOn = true;

    public bool isFloatTextOn = true;
    //public bool isFPSTextOn = true;

    /*public TMP_Dropdown qualityDropdown;

    public VolumeProfile lowQualityProfile;
    public VolumeProfile mediumQualityProfile;
    public VolumeProfile highQualityProfile;

    public Volume globalVolume;*/

    [SerializeField] TMP_Text[] VersionText;

    [SerializeField] GameObject SettingPanel;

    //public GameObject FpsTxtObject;

    private void Awake()
    {
        if(instance == null) instance = this;
    }

    async void Start()
    {
        SoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        MusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        // = PlayerPrefs.GetInt("120FPS", 0) == 1;
        isvibrateOn = PlayerPrefs.GetInt("IsVibrateOn", 1) == 1;
        isFloatTextOn = PlayerPrefs.GetInt("IsFloatTextOn", 1) == 1;
        //isFPSTextOn = PlayerPrefs.GetInt("isFPSTextOn", 0) == 1;

        int savedQualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());

        soundOnTgl.isOn = SoundOn;
        musicOnTgl.isOn = MusicOn;

        AudioManager.instance.ApplyAudioSettings(SoundOn, MusicOn);

        //fpsTgl.isOn = fps120On;
        vibrateTgl.isOn = isvibrateOn;

        //Application.targetFrameRate = fps120On ? 120 : 60;

        //FpsTxtObject.SetActive(isFPSTextOn);

        FloatTextTgl.isOn = isFloatTextOn;
        //FPSTextTgl.isOn = isFPSTextOn;

        //QualitySettings.SetQualityLevel(savedQualityLevel);
        //qualityDropdown.value = savedQualityLevel;
        //qualityDropdown.RefreshShownValue();
        //ApplyVolumeProfile(savedQualityLevel);

        soundOnTgl.onValueChanged.AddListener(SetSound);
        musicOnTgl.onValueChanged.AddListener(SetMusic);
        vibrateTgl.onValueChanged.AddListener(ChangeVibrateSetting);

        FloatTextTgl.onValueChanged.AddListener(ChangeFloatTextSetting);

        /*fpsTgl.onValueChanged.AddListener(ChangeFPSSetting);
        
        FPSTextTgl.onValueChanged.AddListener(ChangeFPSTextSetting);*/

        //qualityDropdown.onValueChanged.AddListener(OnQualitySettingChange);

        foreach (var vText in VersionText)
        {
            vText.text = Application.version.ToString();
        }

    }

    void Update()
    {
    }

    private void ApplyVolumeProfile(int qualityLevel)
    {
        /*if (globalVolume == null) return;

        // Switch the Volume Profile based on the quality level
        switch (qualityLevel)
        {
            case 0: // Low quality
                globalVolume.profile = highQualityProfile;
                break;
            case 1: // Medium quality
                globalVolume.profile = mediumQualityProfile;
                break;
            case 2: // High quality
                globalVolume.profile = lowQualityProfile;
                break;
            default:
                globalVolume.profile = mediumQualityProfile; // Fallback to medium
                break;
        }*/
    }

    public void OnQualitySettingChange(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
        ApplyVolumeProfile(index);

        AudioManager.instance.OnButtonClickSound();
    }

    public void SetSound(bool value)
    {
        SoundOn = value;
        PlayerPrefs.SetInt("SoundOn", value ? 1 : 0);
        AudioManager.instance.ApplyAudioSettings(SoundOn, MusicOn);

        AudioManager.instance.OnButtonClickSound();
    }

    public void SetMusic(bool value)
    {
        MusicOn = value;
        PlayerPrefs.SetInt("MusicOn", value ? 1 : 0);
        AudioManager.instance.ApplyAudioSettings(SoundOn, MusicOn);

        AudioManager.instance.OnButtonClickSound();
    }

    public void ChangeFPSSetting(bool value)
    {
        /*fps120On = value;
        PlayerPrefs.SetInt("120FPS", value ? 1 : 0);
        Application.targetFrameRate = fps120On ? 120 : 60;

        AudioManager.instance.OnButtonClickSound();*/
    }

    public void ChangeVibrateSetting(bool value)
    {
        isvibrateOn = value;
        PlayerPrefs.SetInt("IsVibrateOn", value ? 1 : 0);

        AudioManager.instance.OnButtonClickSound();
    }

    public void ChangeFloatTextSetting(bool value)
    {
        isFloatTextOn = value;
        PlayerPrefs.SetInt("IsFloatTextOn", value ? 1 : 0);

        AudioManager.instance.OnButtonClickSound();
    }

    public void ChangeFPSTextSetting(bool value)
    {
        /*isFPSTextOn = value;
        PlayerPrefs.SetInt("isFPSTextOn", value ? 1 : 0);
        FpsTxtObject.SetActive(isFPSTextOn);

        AudioManager.instance.OnButtonClickSound();*/
    }

    public void Mute(UnityEngine.Events.UnityEventBase ev)
    {
        int count = ev.GetPersistentEventCount();

        for (int i = 0; i < count; i++)
        {
            ev.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        }
    }

    public void Unmute(UnityEngine.Events.UnityEventBase ev)
    {
        int count = ev.GetPersistentEventCount();

        for (int i = 0; i < count; i++)
        {
            ev.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
        }
    }

    public void DiscordIniteBtn()
    {
        Application.OpenURL("https://discord.gg/3frN5bj9GY");
    }

    public void ShowGDPR()
    {
        //YsoCorp.GameUtils.YCManager.instance.settingManager.Show();
    }

    public void OpenSettingPanel()
    {
        AudioManager.instance.PlayOneShot(AudioClipNames.ButtonClick);
        SettingPanel.SetActive(true);
    }
    public void CloseSettingPanel()
    {
        AudioManager.instance.PlayOneShot(AudioClipNames.ButtonClick);
        SettingPanel.SetActive(false);
    }

}

public enum PanelsInsideSettings
{
    Credits,
    Lagunage
}
