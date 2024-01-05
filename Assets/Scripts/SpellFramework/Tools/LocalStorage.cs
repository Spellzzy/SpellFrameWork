using System;
using System.Text;
using LitJson;
using UnityEngine;

namespace SpellFramework.Tools
{
    public class LocalStorage
    {
        // 是否添加额外标识
        public static bool UseSalt { get; set; } = true;
        private static bool _lastUseSalt;
        private const string SaltKey = "uid";
        public static string SaltValue { get; set; } = "";

        private static StringBuilder _hashKeyBuilder = new StringBuilder();

        public static void SetSalt(string value)
        {
            SaltValue = value;
        }

        private static void PushSalt(bool useSalt)
        {
            _lastUseSalt = UseSalt;
            UseSalt = useSalt;
        }

        private static void PopSalt()
        {
            UseSalt = _lastUseSalt;
        }


        public static string Hash(string key)
        {
            _hashKeyBuilder.Length = 0;
            _hashKeyBuilder.Append(Application.platform).Append(key);
            if (UseSalt)
            {
                var salt = string.IsNullOrEmpty(SaltValue) ? SaltKey : SaltValue;
                _hashKeyBuilder.Append(salt);
            }

            return Animator.StringToHash(_hashKeyBuilder.ToString()).ToString();
        }

        public static bool ExistKey(string key)
        {
            var hash = Hash(key);
            return PlayerPrefs.HasKey(key);
        }

        #region UserInterface

        public static string GetString(string key, string defaultStr = "")
        {
            var hash = Hash(key);
            var result = PlayerPrefs.HasKey(hash) ? PlayerPrefs.GetString(hash) : defaultStr;
            return result;
        }

        public static void SetString(string key, string value)
        {
            var hash = Hash(key);
            PlayerPrefs.SetString(hash, value);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var hash = Hash(key);
            var result = PlayerPrefs.HasKey(hash) ? PlayerPrefs.GetInt(hash) : defaultValue;
            return result;
        }

        public static void SetInt(string key, int value)
        {
            var hash = Hash(key);
            PlayerPrefs.SetInt(hash, value);
        }

        public static bool GetBool(string key, bool defaultBool = false)
        {
            return GetInt(key, Convert.ToInt32(defaultBool)) > 0;
        }

        public static void SetBool(string key, bool value)
        {
            SetInt(key, Convert.ToInt32(value));
        }

        public static void SetJson(string key, System.Object value)
        {
            var json = JsonMapper.ToJson(value);
            SetSysString(key, json);
        }

        public static T GetJson<T>(string key)
        {
            T result = default(T);
            var json = GetSysString(key);
            if (!string.IsNullOrEmpty(json))
            {
                result = JsonMapper.ToObject<T>(json);
            }
            return result;
        }

        #endregion

        #region System Interface

        public static string GetSysString(string key, string defaultValue = "")
        {
            PushSalt(false);
            var result = GetString(key, defaultValue);
            PopSalt();

            return result;
        }

        public static void SetSysString(string key, string value)
        {
            PushSalt(false);
            SetString(key, value);
            PopSalt();
        }

        public static int GetSysInt(string key, int defaultValue = 1)
        {
            PushSalt(false);
            var result = GetInt(key, defaultValue);
            PopSalt();
            return result;
        }

        public static void SetSysInt(string key, int value)
        {

            PushSalt(false);
            SetInt(key, value);
            PopSalt();
        }

        public static void SetSysBool(string key, bool value)
        {
            PushSalt(false);
            SetBool(key, value);
            PopSalt();
        }

        public static bool GetSysBool(string key, bool defaultValue = false)
        {
            PushSalt(false);
            var result = GetBool(key, defaultValue);
            PopSalt();
            return result;
        }
        #endregion

        public static void Clear()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}