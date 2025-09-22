using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

    public class MyUlti
    {
        public static int levelLog = 0;//0-debug; 1-warring; 2-error

        public static int screenWith = -1;
        public static int screenHight = -1;

        private static int originWidth = -1;
        private static int originHeight = -1;

        public static void SetScreenResolution(int resulation)
        {
            if (originWidth <= 0)
            {
                originWidth = Screen.width;
                originHeight = Screen.height;
            }
            Screen.SetResolution(originWidth * resulation / 100, originHeight * resulation / 100, true);
        }

        public static Vector2Int ScreenOriginSize()
        {
            if (originWidth <= 0)
            {
                originWidth = Screen.width;
                originHeight = Screen.height;
            }
            return new Vector2Int(originWidth, originHeight);
        }

        public static long CurrentTimeMilis()
        {
            var re = ToTimestamp(DateTime.UtcNow);
            return re;
        }

        public static long ToTimestamp(DateTime datetime)
        {
            var re = (datetime.Ticks - 621355968000000000) / 10000;
            return re;
        }

        public static DateTime TimeStamp2DateTime(long secondsUTC)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (secondsUTC > 1000000000000)
            {
                dateTime = dateTime.AddMilliseconds(secondsUTC);
            }
            else
            {
                dateTime = dateTime.AddSeconds(secondsUTC);
            }
            return dateTime;
        }

        public static TimeSpan MillisecondToTimeSpan(long millisecond)
        {
            return new TimeSpan(millisecond * 10000);
        }

        public static bool IsHighPerformDevice()
        {
#if UNITY_EDITOR
            return true;
#elif (UNITY_IOS || UNITY_IPHONE)
            int something = (int)UnityEngine.iOS.Device.generation;
            if (something >= 50)
            {
                return true;
            }
            else
            {
                return false;
            }
#else
            int mem = SystemInfo.systemMemorySize;
            if (mem >= 4000)
            {
                return true;
            }
            else
            {
                return false;
            }
#endif      
        }

        public static string MoneyToString(long money)
        {
            //Debug.Log($"aaa b={money}");
            var re = money.ToString();

            if (re.Length > 3)
            {
                var n = (re.Length - 1) / 3;
                var len = re.Length - 3;
                for (var i = 0; i < n; i++)
                {
                    re = re.Insert(len - 3 * i, ".");
                }
            }

            return re;
        }

        public static string ConverMoneyK(int money)
        {
            int m;
            if (money >= 1000)
                m = money / 1000;
            else
                m = money;
            var re = m.ToString();

            if (re.Length > 3)
            {
                var n = (re.Length - 1) / 3;
                var len = re.Length - 3;
                for (var i = 0; i < n; i++)
                {
                    re = re.Insert(len, ".");
                    len -= 3;
                }
            }

            return re;
        }

        public static string ConvertMoneyToString(long money)
        {
            if (money >= 1000000000)
            {
                return (money / 1000000000).ToString() + "B";
            }
            if (money >= 1000000)
            {
                return (money / 1000000).ToString() + "M";
            }
            if (money >= 10000)
            {
                return (money / 1000).ToString() + "K";
            }
            return money.ToString();
        }

        public static string GetMoney(int va)
        {
            string re = "";

            int tmp = va;
            while (tmp > 0)
            {
                int n = tmp / 1000;
                tmp = tmp % 1000;
                if (n > 0)
                {
                    if (re.Length == 0)
                    {
                        re = "" + n;
                    }
                    else
                    {
                        re += ("," + n.ToString("000"));
                    }
                }
                else
                {
                    if (re.Length == 0)
                    {
                        re = "" + tmp;
                    }
                    else
                    {
                        re += ("," + tmp.ToString("000"));
                    }
                    break;
                }
            }

            return re;
        }

//        public static int GetAndroidBuildVersion()
//        {
//#if UNITY_ANDROID && !UNITY_EDITOR
//            if (verSdk <= 0)
//            {
//                using (var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
//                {
//                    int verSdk = buildVersion.GetStatic<int>("SDK_INT");
//                    return verSdk;
//                }
//            }
//            else
//            {
//                return verSdk;
//            }
//#else
//            return 1000;
//#endif
//        }

        public static string FormatNumber(int number)
        {
            var re = number.ToString();
            if (re.Length > 3)
            {
                var n = (re.Length - 1) / 3;
                var len = re.Length - 3;
                for (var i = 0; i < n; i++)
                {
                    re = re.Insert(len, ".");
                    len -= 3;
                }
            }

            return re;
        }

    

        public static string Int2TimeString(int seconds)
        {
            string re = "";
            int h = seconds / 3600;
            int m = seconds % 3600;
            int s = m % 60;
            m = m / 60;
            re = string.Format("{0}:{1:d2}:{2:d2}", h, m, s);

            return re;
        }

        public static int SubDay(long from, long to)
        {
            DateTime dbase = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            DateTime dFrom, dto;
            if (from > 1000000000000)
            {
                dFrom = dbase.AddMilliseconds(from);
            }
            else
            {
                dFrom = dbase.AddSeconds(from);
            }
            if (to  > 1000000000000)
            {
                dto = dbase.AddMilliseconds(to);
            }
            else
            {
                dto = dbase.AddSeconds(to);
            }
            int re = dFrom.DayOfYear - dto.DayOfYear;
            int dy = dFrom.Year - dto.Year;
            re = dy * 365 + re;
            return re;
        }

        public static int SubDay(DateTime from, DateTime to)
        {
            int re = (int)((from.ToBinary() - to.ToBinary()) / (24 * 3600));
            return re;
        }
		
#if UNITY_EDITOR
        public static void Encrypt(string bkey, out byte[] makey, out int[] paskey, out byte[] pasva)
        {
            if (bkey != null && bkey.Length > 5)
            {
                makey = new byte[bkey.Length];
                int nl = UnityEngine.Random.Range(bkey.Length / 2 - 5, bkey.Length / 2 + 5);
                if (nl <= 0)
                {
                    nl = bkey.Length / 2;
                }
                paskey = new int[nl];
                pasva = new byte[nl];
                int nd = bkey.Length / nl;
                if (nd <= 0)
                {
                    nd = 2;
                }
                for (int i = 0; i < nl; i++)
                {
                    int na = (i + 4) / 2;
                    paskey[i] = nd * i + UnityEngine.Random.Range(-na, na);
                    pasva[i] = (byte)(UnityEngine.Random.Range(0, 20));
                }
                int idxp = 0;
                //String sslog = "";
                for (int i = 0; i < bkey.Length; i++)
                {
                    byte ch = (byte)bkey[i];
                    if (idxp < paskey.Length && i >= paskey[idxp])
                    {
                        idxp++;
                    }
                    if (idxp < paskey.Length)
                    {
                        ch -= pasva[idxp];
                        //sslog += $"{i},{idxp};";
                    }
                    makey[i] = ch;
                }
                //Debug.Log($"aaa={sslog}");
            }
            else
            {
                makey = new byte[5];
                paskey = new int[5];
                pasva = new byte[5];
            }
        }
#endif

        public static string Decrypt(byte[] makey, int[] paskey, byte[] pasva)
        {
            if (makey != null && makey.Length > 5 && paskey != null && paskey.Length > 0 && pasva != null && pasva.Length == paskey.Length)
            {
                string pkey = "";
                var sb = new System.Text.StringBuilder();
                int idxp = 0;
                //String sslog = "";
                for (int i = 0; i < makey.Length; i++)
                {
                    char ch = (char)makey[i];
                    if (idxp < paskey.Length)
                    {
                        if (i >= paskey[idxp])
                        {
                            idxp++;
                        }
                    }
                    if (idxp < paskey.Length)
                    {
                        ch = (char)(makey[i] + pasva[idxp]);
                        //sslog += $"{i},{idxp};";
                    }
                    sb.Append(ch);
                }
                pkey = sb.ToString();
                //Debug.Log($"aaa={sslog}");
                //Debug.Log($"aaabbb={pkey}");
                return pkey;
            }
            return "";
        }

        public static bool IsiPad()
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
            {
                return true;
            }
            else
            {
                float w = Screen.width;
                float h = Screen.height;
                if (h < w)
                {
                    float th = h;
                    h = w;
                    w = th;
                }
                if (w > 0)
                {
                    float per = h / w;
                    if (per < 1.65f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
#else
            float w = Screen.width;
            float h = Screen.height;
            if (h < w)
            {
                float th = h;
                h = w;
                w = th;
            }
            if (w > 0)
            {
                float per = h / w;
                if (per < 1.65f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
#endif
        }

        public static float ScreenDpi()
        {
            float re = 1.5f;
#if UNITY_IOS || UNITY_IPHONE
            if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
            {
                Debug.Log("mysdk: iPad dpi=" + Screen.dpi);
                re = Screen.dpi;
            }
            else
            {
                Debug.Log("mysdk: iPhone dpi=" + Screen.dpi);
                re = Screen.dpi;
            }
#else
            Debug.Log("mysdk: Android dpi=" + Screen.dpi);
            re = Screen.dpi;
#endif
            if (re <= 0)
            {
                re = 1.65f * 160;
            }
            else if (re <= 1)
            {
                re = 1.5f * Screen.dpi;
            }
            Debug.Log("mysdk: screen dpi=" + re);
            return re;
        }
      
        public static float GetHeightBanner()
        {
            float dpi = ScreenDpi();
            if (IsiPad())
            {
                return 100 * dpi / 160.0f;
            }
            else
            {
                return 60 * dpi / 160.0f;
            }
        }

        public static bool isLog()
        {
#if ENABLE_MYLOG
            return true;
#endif
            return false;
        }

    public static void LogList<T>(List<T> list, string label = "")
    {
        string content = string.Join(", ", list);
        Debug.Log($"{label} [{list.Count}]: {content}");
    }

    public static void RemoveAllChilds(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
            else
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);

#else
            GameObject.Destroy(parent.GetChild(i).gameObject);
#endif
        }
    }

    public static void SetExistingGameViewSize(int width, int height)
    {
#if UNITY_EDITOR
        var asm = typeof(EditorWindow).Assembly;

        // Access GameViewSizes singleton
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var singletonType = asm.GetType("UnityEditor.ScriptableSingleton`1").MakeGenericType(sizesType);
        var instanceProp = singletonType.GetProperty("instance");
        var gameViewSizesInstance = instanceProp.GetValue(null, null);

        // Get current group (aspect ratios for Standalone, iOS, Android, etc.)
        var currentGroupProp = sizesType.GetProperty("currentGroup");
        var group = currentGroupProp.GetValue(gameViewSizesInstance, null);

        // Methods
        var getTotalCount = group.GetType().GetMethod("GetTotalCount");
        var getGameViewSize = group.GetType().GetMethod("GetGameViewSize");

        int totalCount = (int)getTotalCount.Invoke(group, null);

        int foundIndex = -1;

        for (int i = 0; i < totalCount; i++)
        {
            var sizeObj = getGameViewSize.Invoke(group, new object[] { i });
            var widthProp = sizeObj.GetType().GetProperty("width");
            var heightProp = sizeObj.GetType().GetProperty("height");

            int w = (int)widthProp.GetValue(sizeObj);
            int h = (int)heightProp.GetValue(sizeObj);

            if (w == width && h == height)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex == -1)
        {
            Debug.LogWarning($"? No existing GameView size found for {width}x{height}");
            return;
        }

        // Switch GameView to that index
        var gvType = asm.GetType("UnityEditor.GameView");
        var gameViewWindow = EditorWindow.GetWindow(gvType);
        var sizeSelectionCallback = gvType.GetMethod("SizeSelectionCallback",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        sizeSelectionCallback.Invoke(gameViewWindow, new object[] { foundIndex, null });

        Debug.Log($"? Switched GameView to existing size {width}x{height} (index {foundIndex})");
#endif
    }

}