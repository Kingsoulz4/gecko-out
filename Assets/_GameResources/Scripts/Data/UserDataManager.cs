
using Newtonsoft.Json;
using System;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public const int LevelMax = 200;
    [System.Serializable]
    public class UserData
    {
        public int level;
        public int gold;
        public int heart;
        public bool removeAds;
        public int timeIngameBooster;
        public int timePreBooster;
        public int hammerBooster;
        public int cissorBooster;
        public int suffleBooster;
        public int handMoveBooster;
        public long lastTimeLogin;
        public string userName;
        public long firstTimeJoinGame;
        public int avtarID = 1;
        public int isFirstShowChangeName;
    }

    private const string USER_DATA_KEY = Constant.PlayerPrefs.USER_DATA;
    public static Action<int, int, float> OnUpdateGold;


    #region Common

    public static void AddGold(int value, string where, bool isLog = false, string reason = "", float timeDelay = 0)
    {
        OnUpdateGold?.Invoke(Gold, value + Gold, timeDelay);
        Gold = value;
    }

    public static Action<int, int, bool> OnAddHeart;
    public static Action AvatarChange;
    public static Action UserNameChange;

    public static bool IsNewDay = false;

    public static void AddHeart(int amount, string where, bool hasAnimation, int typeHeart = 0, bool isLog = false, string reason = "")
    {
        if (typeHeart == 0)
        {
            int current = Heart;
            int newValue = current + amount;
            if (newValue < 0)
            {
                newValue = 0;
            }
            Heart = newValue;
            OnAddHeart?.Invoke(current, newValue, hasAnimation);
        }
        else
        {
            var now = GameTime.Instance.GetUtcTime();
            if (HeartManager.InfinityEndTime < now)
            {
                HeartManager.InfinityEndTime = now + amount;
            }
            else
            {
                HeartManager.InfinityEndTime += amount;
            }
        }
    }
    #endregion

    #region Get Set
    public static int Level
    {
        get { return LoadUserData().level; }
        set
        {
            UserData data = LoadUserData();
            data.level = value;
            SaveUserData(data);
        }
    }
    public static int avtarID
    {
        get
        {
            return LoadUserData().avtarID;
        }
        set
        {
            UserData data = LoadUserData();
            data.avtarID = value;
            SaveUserData(data);
            AvatarChange?.Invoke();
        }
    }

    public static string userName
    {
        get
        {
            string nameCurrent = LoadUserData().userName;
            if (string.IsNullOrEmpty(nameCurrent))
            {
                UserData data = LoadUserData();
                data.userName = getNewUserName();
                nameCurrent = data.userName;
                SaveUserData(data);
            }
            return nameCurrent;
        }
        set
        {
            UserData data = LoadUserData();
            data.userName = value;
            SaveUserData(data);
            UserNameChange?.Invoke();
        }
    }
    private static string getNewUserName()
    {
        return $"User#{UnityEngine.Random.Range(0, 10000000):0000}";
    }

    public static long firstTimeJoinGame
    {
        get { return LoadUserData().firstTimeJoinGame; }
        set
        {
            UserData data = LoadUserData();
            data.firstTimeJoinGame = value;
            SaveUserData(data);
        }
    }

    public static long LastTimeLogin
    {
        get { return LoadUserData().lastTimeLogin; }
        set
        {
            UserData data = LoadUserData();
            data.lastTimeLogin = value;
            SaveUserData(data);
        }
    }

    public static int Gold
    {
        get
        {
            UserData data = LoadUserData();
            return data.gold;
        }
        private set
        {
            UserData data = LoadUserData();
            data.gold = value;
            SaveUserData(data);
        }
    }

    public static int Heart
    {
        get
        {
            UserData data = LoadUserData();
            return data.heart;
        }
        private set
        {
            UserData data = LoadUserData();
            data.heart = value;
            SaveUserData(data);
        }
    }

    public static bool RemoveAds
    {
        get { return LoadUserData().removeAds; }
        set
        {
            UserData data = LoadUserData();
            data.removeAds = value;
            SaveUserData(data);
        }
    }


    public static bool IsFirstShowChangeName
    {
        get
        {
            return LoadUserData().isFirstShowChangeName == 0;
        }
        set
        {
            UserData data = LoadUserData();
            data.isFirstShowChangeName = value ? 0 : 1;
            SaveUserData(data);
        }
    }

    public static int TimeIngameBooster
    {
        get { return LoadUserData().timeIngameBooster; }
        set
        {
            UserData data = LoadUserData();
            data.timeIngameBooster = value;
            SaveUserData(data);
        }
    }

    public static int TimePreBooster
    {
        get { return LoadUserData().timePreBooster; }
        set
        {
            UserData data = LoadUserData();
            data.timePreBooster = value;
            SaveUserData(data);
        }
    }

    public static int HammerBooster
    {
        get { return LoadUserData().hammerBooster; }
        set
        {
            UserData data = LoadUserData();
            data.hammerBooster = value;
            SaveUserData(data);
        }
    }

    public static int SuffleBooster
    {
        get { return LoadUserData().suffleBooster; }
        set
        {
            UserData data = LoadUserData();
            data.suffleBooster = value;
            SaveUserData(data);
        }
    }

    public static int HandMoveBooster
    {
        get { return LoadUserData().handMoveBooster; }
        set
        {
            UserData data = LoadUserData();
            data.handMoveBooster = value;
            SaveUserData(data);
        }
    }

    public static int CissorBooster
    {
        get { return LoadUserData().cissorBooster; }
        set
        {
            UserData data = LoadUserData();
            data.cissorBooster = value;
            SaveUserData(data);
        }
    }

    public static UserData LoadUserData()
    {
        if (PlayerPrefs.HasKey(USER_DATA_KEY))
        {
            string jsonData = PlayerPrefs.GetString(USER_DATA_KEY);
            return JsonConvert.DeserializeObject<UserData>(jsonData);
        }
        return GetDefaultUserData();
    }

    private static void SaveUserData(UserData data)
    {
        string jsonData = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString(USER_DATA_KEY, jsonData);
        PlayerPrefs.Save();
    }
    #endregion


    private static UserData GetDefaultUserData()
    {
        return new UserData
        {
            level = 1,
            gold = 0,
            heart = 3,
            removeAds = false,
            timeIngameBooster = 0,
            hammerBooster = 0,
            cissorBooster = 0
        };
    }
}