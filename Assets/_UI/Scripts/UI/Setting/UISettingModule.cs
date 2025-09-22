using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingModule : MonoBehaviour
{
    [SerializeField] private Button musicBtn;
    [SerializeField] private Button soundBtn;
    [SerializeField] private Button vibrateBtn;
    [SerializeField] private Animation musicAni;
    [SerializeField] private Animation soundAni;
    [SerializeField] private Animation vibrateAni;
    [SerializeField] AudioClip SFX_ButtonSwitch;

    private void Start()
    {
        musicBtn.onClick.AddListener(OnClickMusic);
        soundBtn.onClick.AddListener(OnClickSound);
        vibrateBtn.onClick.AddListener(OnClickVibrate);
        Refresh();
    }

    private void OnClickMusic()
    {
        bool isActiveMusic = AudioManager.MusicSetting == 1;
        AudioManager.Instance.EnableMusic(!isActiveMusic);
        AudioManager.Instance.PlayOneShot(SFX_ButtonSwitch, 1);
        musicAni.Play(!isActiveMusic ? "SettingChangeOn_New" : "SettingChangeOff_New");
    }

    private void OnClickSound()
    {
        bool isActiveSFX = AudioManager.SoundSetting == 1;
        AudioManager.Instance.EnableSound(!isActiveSFX);
        AudioManager.Instance.PlayOneShot(SFX_ButtonSwitch, 1);
        soundAni.Play(!isActiveSFX ? "SettingChangeOn_New" : "SettingChangeOff_New");
    }

    private void OnClickVibrate()
    {
        bool isActiveVibrate = PlayerPrefs.GetInt("isActiveVibrate", 1) == 1;
        PlayerPrefs.SetInt("isActiveVibrate", !isActiveVibrate ? 1 : 0);
        AudioManager.Instance.PlayOneShot(SFX_ButtonSwitch, 1);
        vibrateAni.Play(!isActiveVibrate ? "SettingChangeOn_New" : "SettingChangeOff_New");
    }

    private void Refresh()
    {
        musicAni.Play(AudioManager.MusicSetting == 1 ? "SettingOn_New" : "SettingOff_New");
        soundAni.Play(AudioManager.SoundSetting == 1 ? "SettingOn_New" : "SettingOff_New");
        vibrateAni.Play(PlayerPrefs.GetInt("KeyConfigVibrate", 1) == 1 ? "SettingOn_New" : "SettingOff_New");
    }
}
