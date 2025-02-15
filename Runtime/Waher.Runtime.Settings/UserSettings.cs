using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings.UserSettingObjects;
using Waher.Runtime.Threading;

namespace Waher.Runtime.Settings
{
    /// <summary>
    /// Static class managing persistent user settings. User settings default to runtime settings if user-specific settings
    /// are not available.
    /// </summary>
    public static class UserSettings
    {
        #region String-valued settings

        /// <summary>
        /// Gets a string-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static string Get(string User, string Key, string DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a string-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<string> GetAsync(string User, string Key, string DefaultValue)
        {
            StringUserSetting Setting = await GetAsync<StringUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        private static async Task<T> GetAsync<T>(string User, string Key)
            where T : class
        {
            using (Semaphore Semaphore = await Semaphores.BeginRead("usersetting:" + User + " " + Key))
            {
                T Setting = await Database.FindFirstDeleteRest<T>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));

                return Setting;
            }
        }

        /// <summary>
        /// Sets a string-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, string Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a string-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, string Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                StringUserSetting Setting = await Database.FindFirstDeleteRest<StringUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, string Value, StringUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new StringUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region Int64-valued settings

        /// <summary>
        /// Gets a long-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static long Get(string User, string Key, long DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a long-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<long> GetAsync(string User, string Key, long DefaultValue)
        {
            Int64UserSetting Setting = await GetAsync<Int64UserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a long-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, long Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a long-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, long Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                Int64UserSetting Setting = await Database.FindFirstDeleteRest<Int64UserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, long Value, Int64UserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new Int64UserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region Boolean-valued settings

        /// <summary>
        /// Gets a bool-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static bool Get(string User, string Key, bool DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a bool-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<bool> GetAsync(string User, string Key, bool DefaultValue)
        {
            BooleanUserSetting Setting = await GetAsync<BooleanUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a bool-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, bool Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a bool-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, bool Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                BooleanUserSetting Setting = await Database.FindFirstDeleteRest<BooleanUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, bool Value, BooleanUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new BooleanUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region DateTime-valued settings

        /// <summary>
        /// Gets a DateTime-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static DateTime Get(string User, string Key, DateTime DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a DateTime-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<DateTime> GetAsync(string User, string Key, DateTime DefaultValue)
        {
            DateTimeUserSetting Setting = await GetAsync<DateTimeUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a DateTime-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, DateTime Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a DateTime-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, DateTime Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                DateTimeUserSetting Setting = await Database.FindFirstDeleteRest<DateTimeUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, DateTime Value, DateTimeUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new DateTimeUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region TimeSpan-valued settings

        /// <summary>
        /// Gets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static TimeSpan Get(string User, string Key, TimeSpan DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<TimeSpan> GetAsync(string User, string Key, TimeSpan DefaultValue)
        {
            TimeSpanUserSetting Setting = await GetAsync<TimeSpanUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, TimeSpan Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, TimeSpan Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                TimeSpanUserSetting Setting = await Database.FindFirstDeleteRest<TimeSpanUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, TimeSpan Value, TimeSpanUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new TimeSpanUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region Double-valued settings

        /// <summary>
        /// Gets a double-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static double Get(string User, string Key, double DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a double-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<double> GetAsync(string User, string Key, double DefaultValue)
        {
            DoubleUserSetting Setting = await GetAsync<DoubleUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a double-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, double Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a double-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, double Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                DoubleUserSetting Setting = await Database.FindFirstDeleteRest<DoubleUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, double Value, DoubleUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new DoubleUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region Enum-valued settings

        /// <summary>
        /// Gets a Enum-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static Enum Get(string User, string Key, Enum DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a Enum-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<Enum> GetAsync(string User, string Key, Enum DefaultValue)
        {
            EnumUserSetting Setting = await GetAsync<EnumUserSetting>(User, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a Enum-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, Enum Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a Enum-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, Enum Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                EnumUserSetting Setting = await Database.FindFirstDeleteRest<EnumUserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(User, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string User, string Key, Enum Value, EnumUserSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new EnumUserSetting(User, Key, Value);
                await Database.Insert(Setting);

                return true;
            }
            else
            {
                if (Setting.Value != Value)
                {
                    Setting.Value = Value;
                    await Database.Update(Setting);

                    return true;
                }
                else
                    return false;
            }
        }

        #endregion

        #region Object-valued settings

        /// <summary>
        /// Gets a object-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static object Get(string User, string Key, object DefaultValue)
        {
            return GetAsync(User, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a object-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<object> GetAsync(string User, string Key, object DefaultValue)
        {
            UserSetting Setting = await GetAsync<UserSetting>(User, Key);
            return Setting?.GetValueObject() ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a object-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string User, string Key, object Value)
        {
            return SetAsync(User, Key, Value).Result;
        }

        /// <summary>
        /// Sets a object-valued setting.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string User, string Key, object Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                UserSetting Setting = await Database.FindFirstDeleteRest<UserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key)));

                if (Value is null)
                {
                    if (!(Setting is ObjectUserSetting))
                    {
                        if (!(Setting is null))
                            await Database.Delete(Setting);

                        Setting = null;
                    }
                }
                else
                {
                    if (Value is string s)
                    {
                        if (!(Setting is StringUserSetting StringSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            StringSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, s, StringSetting);
                    }
                    else if (Value is long l)
                    {
                        if (!(Setting is Int64UserSetting Int64Setting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            Int64Setting = null;
                        }

                        return await SetAsyncLocked(User, Key, l, Int64Setting);
                    }
                    else if (Value is double d)
                    {
                        if (!(Setting is DoubleUserSetting DoubleSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            DoubleSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, d, DoubleSetting);
                    }
                    else if (Value is bool b)
                    {
                        if (!(Setting is BooleanUserSetting BooleanSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            BooleanSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, b, BooleanSetting);
                    }
                    else if (Value is DateTime TP)
                    {
                        if (!(Setting is DateTimeUserSetting DateTimeSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            DateTimeSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, TP, DateTimeSetting);
                    }
                    else if (Value is TimeSpan TS)
                    {
                        if (!(Setting is TimeSpanUserSetting TimeSpanSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            TimeSpanSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, TS, TimeSpanSetting);
                    }
                    else if (Value is Enum E)
                    {
                        if (!(Setting is EnumUserSetting EnumSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            EnumSetting = null;
                        }

                        return await SetAsyncLocked(User, Key, E, EnumSetting);
                    }
                    else
                    {
                        Type T = Value.GetType();
                        TypeInfo TI = T.GetTypeInfo();

                        if (!(TI.GetCustomAttribute(typeof(CollectionNameAttribute)) is null))
                            throw new InvalidOperationException("Object setting values cannot be stored separate collections. (CollectionName attribute found.)");

                        TypeNameAttribute TypeNameAttribute = TI.GetCustomAttribute<TypeNameAttribute>();
                        if (TypeNameAttribute is null || TypeNameAttribute.TypeNameSerialization != TypeNameSerialization.FullName)
                            throw new InvalidOperationException("Full Type names must be serialized when persisting object setting values. (TypeName attribute.). Exceptions for the types: Boolean, Int64, String, DateTime, TimeSpan, Double.");
                    }
                }

                if (Setting is ObjectUserSetting ObjectSetting)
                {
                    if (((ObjectSetting.Value is null) ^ (Value is null)) || !ObjectSetting.Value.Equals(Value))
                    {
                        ObjectSetting.Value = Value;
                        await Database.Update(ObjectSetting);

                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    Setting = new ObjectUserSetting(User, Key, Value);
                    await Database.Insert(Setting);

                    return true;
                }
            }
        }

        #endregion

        #region Delete Setting

        /// <summary>
        /// Deletes a runtime setting
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Key name.</param>
        /// <returns>If a setting was found with the given name and deleted.</returns>
        public static async Task<bool> DeleteAsync(string User, string Key)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("usersetting:" + User + " " + Key))
            {
                foreach (UserSetting _ in await Database.FindDelete<UserSetting>(new FilterAnd(
                    new FilterFieldEqualTo("User", User), new FilterFieldEqualTo("Key", Key))))
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Batch Get

        /// <summary>
        /// Gets available settings, matching a search filter.
        /// </summary>
        /// <param name="Filter">Search filter.</param>
        /// <param name="UserAsKey">If the user parameter should be the key in the resultig dictionary (true) or 
        /// the key parameter (false).</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhere(Filter Filter, bool UserAsKey)
        {
            return GetWhereAsync(Filter, UserAsKey).Result;
        }

        /// <summary>
        /// Gets available settings, matching a search filter.
        /// </summary>
        /// <param name="Filter">Search filter.</param>
        /// <param name="UserAsKey">If the user parameter should be the key in the resultig dictionary (true) or 
        /// the key parameter (false).</param>
        /// <returns>Matching settings found.</returns>
        public static async Task<Dictionary<string, object>> GetWhereAsync(Filter Filter, bool UserAsKey)
        {
            Dictionary<string, object> Result = new Dictionary<string, object>();

            foreach (UserSetting Setting in await Database.Find<UserSetting>(Filter))
                Result[UserAsKey ? Setting.User : Setting.Key] = Setting.GetValueObject();

            return Result;
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhereKeyLikeRegEx(string User, string KeyPattern)
        {
            return GetWhere(new FilterAnd(
                new FilterFieldEqualTo("User", User), 
                new FilterFieldLikeRegEx("Key", KeyPattern)), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
        /// <returns>Matching settings found.</returns>
        public static Task<Dictionary<string, object>> GetWhereKeyLikeRegExAsync(string User, string KeyPattern)
        {
            return GetWhereAsync(new FilterAnd(
                new FilterFieldEqualTo("User", User), 
                new FilterFieldLikeRegEx("Key", KeyPattern)), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Return settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhereKeyLike(string User, string Key, string Wildcard)
        {
            return GetWhere(new FilterAnd(
                new FilterFieldEqualTo("User", User),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Return settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Matching settings found.</returns>
        public static Task<Dictionary<string, object>> GetWhereKeyLikeAsync(string User, string Key, string Wildcard)
        {
            return GetWhereAsync(new FilterAnd(
                new FilterFieldEqualTo("User", User),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))), false);
        }

        /// <summary>
        /// Gets available settings for a given key, indexed by user.
        /// </summary>
        /// <returns>User settings found.</returns>
        public static Dictionary<string, object> GetUserValues(string Key)
        {
            return GetWhere(new FilterFieldEqualTo("Key", Key), true);
        }

        /// <summary>
        /// Gets available settings for a given key, indexed by user.
        /// </summary>
        /// <returns>User settings found.</returns>
        public static Task<Dictionary<string, object>> GetUserValuesAsync(string Key)
        {
            return GetWhereAsync(new FilterFieldEqualTo("Key", Key), true);
        }

        #endregion

        #region Batch Delete

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
        /// <param name="Filter">Search filter.</param>
        /// <returns>Number of settings deleted.</returns>
        public static int DeleteWhere(Filter Filter)
        {
            return DeleteWhereAsync(Filter).Result;
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
        /// <param name="Filter">Search filter.</param>
        /// <returns>Number of settings deleted.</returns>
        public static async Task<int> DeleteWhereAsync(Filter Filter)
        {
            int Result = 0;

            foreach (UserSetting Setting in await Database.FindDelete<UserSetting>(Filter))
                Result++;

            return Result;
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
        /// <returns>Number of settings deleted.</returns>
        public static int DeleteWhereKeyLikeRegEx(string User, string KeyPattern)
        {
            return DeleteWhere(new FilterAnd(new FilterFieldEqualTo("User", User), new FilterFieldLikeRegEx("Key", KeyPattern)));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
        /// <returns>Number of settings deleted.</returns>
        public static Task<int> DeleteWhereKeyLikeRegExAsync(string User, string KeyPattern)
        {
            return DeleteWhereAsync(new FilterAnd(new FilterFieldEqualTo("User", User), new FilterFieldLikeRegEx("Key", KeyPattern)));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Number of settings deleted.</returns>
        public static int DeleteWhereKeyLike(string User, string Key, string Wildcard)
        {
            return DeleteWhere(new FilterAnd(new FilterFieldEqualTo("User", User),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="User">Name of user.</param>
        /// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Number of settings deleted.</returns>
        public static Task<int> DeleteWhereKeyLikeAsync(string User, string Key, string Wildcard)
        {
            return DeleteWhereAsync(new FilterAnd(new FilterFieldEqualTo("User", User),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))));
        }

        #endregion
    }
}