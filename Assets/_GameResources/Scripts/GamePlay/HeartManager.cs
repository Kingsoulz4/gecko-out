using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : SingletonMono<HeartManager>
{
    public static Action<bool> OnReceiveHeartDone;
    public static int CF_EnableHeart
    {
        get => PlayerPrefs.GetInt("cf_enable_heart", 0);
        set => PlayerPrefs.SetInt("cf_enable_heart", value);
    }
    public static bool IsActiveHeart => CF_EnableHeart > 0;
    public static int MAX_HEART
    {
        get => PlayerPrefs.GetInt("max_heart_in_game", 5);
        set => PlayerPrefs.SetInt("max_heart_in_game", value);
    }
    public static int CF_RecoverTimeHeart
    {
        get => PlayerPrefs.GetInt("cf_recover_time_heart", 1800);
        set => PlayerPrefs.SetInt("cf_recover_time_heart", value);
    }
    private static string lastTimeData
    {
        get { return PlayerPrefs.GetString("last_heart_time_long", "0"); }
        set { PlayerPrefs.SetString("last_heart_time_long", value); }
    }
    public static long LastTimeAddHeart
    {
        get => lastTimeAddHeart;
        set
        {
            lastTimeAddHeart = value;
            lastTimeData = value.ToString();
        }
    }
    private static long lastTimeAddHeart;
    public static string InfinityEndTimeData
    {
        get { return PlayerPrefs.GetString("infinity_end_time", "0"); }
        set { PlayerPrefs.SetString("infinity_end_time", value); }
    }
    public static long InfinityEndTime
    {
        get => infinityEndTime;
        set
        {
            infinityEndTime = value;
            InfinityEndTimeData = value.ToString();
        }
    }

    private static long infinityEndTime;

    private void Start()
    {
        UserDataManager.OnAddHeart += OnAddHeart;
        CheckOffline();
    }
    protected bool hasRegisterEvent;
    protected void Awake()
    {
        lastTimeAddHeart = long.Parse(lastTimeData);
        infinityEndTime = long.Parse(InfinityEndTimeData);
        if (!hasRegisterEvent)
        {
            hasRegisterEvent = true;
            RegisterListener();
        }
    }
    private void OnDestroy()
    {
        if (hasRegisterEvent)
        {
            RemoveListener();
        }
    }
    public void RegisterListener()
    {
        OnReceiveHeartDone += OnAddHeartDone;
    }
    void OnAddHeartDone(bool v)
    {
        if (v)
        {
            UserDataManager.AddHeart(1, "receive_heart", false, isLog: true, reason: "reward");
        }
    }
    public void RemoveListener()
    {
        OnReceiveHeartDone -= OnAddHeartDone;
    }
    private void Update()
    {
        if (!IsActiveHeart) return;
        long now = GameTime.Instance.GetUtcTime();
        if (UserDataManager.Heart >= MAX_HEART) return;
        long saveTime = lastTimeAddHeart;
        long recoverTime = AddTime(saveTime, CF_RecoverTimeHeart, 0);
        long r = recoverTime - now;
        if (r <= 0)
        {
            UserDataManager.AddHeart(1, "recover", false, isLog: true, reason: "reward");
        }
    }
    private void OnAddHeart(int old, int newValue, bool hasAnimation)
    {
        if (newValue < MAX_HEART && AddTime(lastTimeAddHeart, CF_RecoverTimeHeart, 0) <= GameTime.Instance.GetUtcTime())
        {
            LastTimeAddHeart = GameTime.Instance.GetUtcTime();
        }
        if (newValue == MAX_HEART)
        {
            LastTimeAddHeart = 0;
        }
    }
    public string GetTimeRemaningText()
    {
        long now = GameTime.Instance.GetUtcTime();
        if (infinityEndTime > now)
        {
            var timeLerp = infinityEndTime - now;
            var timeLeft = new TimeSpan(timeLerp * 10000);
            if (timeLeft.Hours > 0)
            {
                return $"{timeLeft.Hours}h:{timeLeft.Minutes}m";
            }
            else
            {
                if (timeLeft.Minutes > 0)
                {
                    return $"{timeLeft.Minutes}m:{timeLeft.Seconds}s";
                }
                else
                {
                    return $"{timeLeft.Seconds}s";
                }
            }
        }
        if (UserDataManager.Heart < MAX_HEART)
        {
            long lastTime = lastTimeAddHeart;
            long recoverTime = AddTime(lastTime, CF_RecoverTimeHeart, 0);
            long timeDifference = recoverTime - now;
            long hours = timeDifference / 3600000;
            long minutes = (timeDifference % 3600000) / 60000;
            long seconds = (timeDifference % 60000) / 1000;
            string minuteSt = minutes < 10 ? $"0{minutes}" : minutes.ToString();
            string secondSt = seconds < 10 ? $"0{seconds}" : seconds.ToString();
            if (hours > 0)
            {
                string hourSt = hours < 10 ? $"0{hours}" : hours.ToString();
                return $"{hourSt}:{minuteSt}:{secondSt}";
            }
            else
            {
                return $"{minuteSt}:{secondSt}";
            }
        }
        return "full";
    }
    public static long AddTime(long timeCurrent, int secon, int mini, int hour = 0, int day = 0)
    {
        long milliseconds = secon * 1000L;
        long minutesInMillis = mini * 60 * 1000L;
        long hoursInMillis = hour * 3600 * 1000L;
        long daysInMillis = day * 86400 * 1000L;
        long returnTime = timeCurrent + milliseconds + minutesInMillis + hoursInMillis + daysInMillis;
        return returnTime;
    }
    private void CheckOffline()
    {
        if (lastTimeData.Length == 0) return;
        long now = GameTime.Instance.GetUtcTime();
        long timeSave = lastTimeAddHeart;
        long r = now - timeSave;
        long timeRecover = CF_RecoverTimeHeart * 1000L;
        int lives = (int)(r / timeRecover);
        int maxLives = MAX_HEART - UserDataManager.Heart;
        if (lives > maxLives)
        {
            lives = maxLives;
        }
        if (lives > 0)
        {
            UserDataManager.AddHeart(lives, "recover", false, isLog: true, reason: "reward");
        }
    }

    public static bool CheckHeart()
    {
        if (IsActiveHeart)
        {
            if (UserDataManager.Heart <= 0 && InfinityEndTime < GameTime.Instance.GetUtcTime())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public static bool IsInfinityEndTime
    {
        get => InfinityEndTime >= GameTime.Instance.GetUtcTime();
    }

    public static bool UseHeart(int number)
    {
        if (IsActiveHeart)
        {
            if (IsInfinityEndTime)
            {
                return false;
            }
            UserDataManager.AddHeart(-number, "start_level", true);
            return true;
        }
        else
        {
            return false;
        }
    }
}
