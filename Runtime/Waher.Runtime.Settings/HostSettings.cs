using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings.HostSettingObjects;
using Waher.Runtime.Threading;

namespace Waher.Runtime.Settings
{
    /// <summary>
    /// Static class managing persistent host settings. Host settings default to runtime settings if host-specific settings
    /// are not available.
    /// </summary>
    public static class HostSettings
    {
        #region String-valued settings

        /// <summary>
        /// Gets a string-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static string Get(string Host, string Key, string DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a string-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<string> GetAsync(string Host, string Key, string DefaultValue)
        {
            StringHostSetting Setting = await GetAsync<StringHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        private static async Task<T> GetAsync<T>(string Host, string Key)
            where T : class
        {
            using (Semaphore Semaphore = await Semaphores.BeginRead("hostsetting:" + Host + " " + Key))
            {
                T Setting = await Database.FindFirstDeleteRest<T>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));

                return Setting;
            }
        }

        /// <summary>
        /// Sets a string-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, string Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a string-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, string Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                StringHostSetting Setting = await Database.FindFirstDeleteRest<StringHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, string Value, StringHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new StringHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static long Get(string Host, string Key, long DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a long-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<long> GetAsync(string Host, string Key, long DefaultValue)
        {
            Int64HostSetting Setting = await GetAsync<Int64HostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a long-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, long Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a long-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, long Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                Int64HostSetting Setting = await Database.FindFirstDeleteRest<Int64HostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, long Value, Int64HostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new Int64HostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static bool Get(string Host, string Key, bool DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a bool-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<bool> GetAsync(string Host, string Key, bool DefaultValue)
        {
            BooleanHostSetting Setting = await GetAsync<BooleanHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a bool-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, bool Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a bool-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, bool Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                BooleanHostSetting Setting = await Database.FindFirstDeleteRest<BooleanHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, bool Value, BooleanHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new BooleanHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static DateTime Get(string Host, string Key, DateTime DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a DateTime-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<DateTime> GetAsync(string Host, string Key, DateTime DefaultValue)
        {
            DateTimeHostSetting Setting = await GetAsync<DateTimeHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a DateTime-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, DateTime Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a DateTime-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, DateTime Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                DateTimeHostSetting Setting = await Database.FindFirstDeleteRest<DateTimeHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, DateTime Value, DateTimeHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new DateTimeHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static TimeSpan Get(string Host, string Key, TimeSpan DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<TimeSpan> GetAsync(string Host, string Key, TimeSpan DefaultValue)
        {
            TimeSpanHostSetting Setting = await GetAsync<TimeSpanHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, TimeSpan Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a TimeSpan-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, TimeSpan Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                TimeSpanHostSetting Setting = await Database.FindFirstDeleteRest<TimeSpanHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, TimeSpan Value, TimeSpanHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new TimeSpanHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static double Get(string Host, string Key, double DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a double-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<double> GetAsync(string Host, string Key, double DefaultValue)
        {
            DoubleHostSetting Setting = await GetAsync<DoubleHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a double-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, double Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a double-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, double Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                DoubleHostSetting Setting = await Database.FindFirstDeleteRest<DoubleHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, double Value, DoubleHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new DoubleHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static Enum Get(string Host, string Key, Enum DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a Enum-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<Enum> GetAsync(string Host, string Key, Enum DefaultValue)
        {
            EnumHostSetting Setting = await GetAsync<EnumHostSetting>(Host, Key);
            return Setting?.Value ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a Enum-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, Enum Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a Enum-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, Enum Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                EnumHostSetting Setting = await Database.FindFirstDeleteRest<EnumHostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));
                return await SetAsyncLocked(Host, Key, Value, Setting);
            }
        }

        private static async Task<bool> SetAsyncLocked(string Host, string Key, Enum Value, EnumHostSetting Setting)
        {
            if (Setting is null)
            {
                Setting = new EnumHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static object Get(string Host, string Key, object DefaultValue)
        {
            return GetAsync(Host, Key, DefaultValue).Result;
        }

        /// <summary>
        /// Gets a object-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="DefaultValue">Default value, if not found.</param>
        /// <returns>Setting value.</returns>
        public static async Task<object> GetAsync(string Host, string Key, object DefaultValue)
        {
            HostSetting Setting = await GetAsync<HostSetting>(Host, Key);
            return Setting?.GetValueObject() ?? await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Sets a object-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static bool Set(string Host, string Key, object Value)
        {
            return SetAsync(Host, Key, Value).Result;
        }

        /// <summary>
        /// Sets a object-valued setting.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <param name="Value">New value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetAsync(string Host, string Key, object Value)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                HostSetting Setting = await Database.FindFirstDeleteRest<HostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key)));

                if (Value is null)
                {
                    if (!(Setting is ObjectHostSetting))
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
                        if (!(Setting is StringHostSetting StringSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            StringSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, s, StringSetting);
                    }
                    else if (Value is long l)
                    {
                        if (!(Setting is Int64HostSetting Int64Setting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            Int64Setting = null;
                        }

                        return await SetAsyncLocked(Host, Key, l, Int64Setting);
                    }
                    else if (Value is double d)
                    {
                        if (!(Setting is DoubleHostSetting DoubleSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            DoubleSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, d, DoubleSetting);
                    }
                    else if (Value is bool b)
                    {
                        if (!(Setting is BooleanHostSetting BooleanSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            BooleanSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, b, BooleanSetting);
                    }
                    else if (Value is DateTime TP)
                    {
                        if (!(Setting is DateTimeHostSetting DateTimeSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            DateTimeSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, TP, DateTimeSetting);
                    }
                    else if (Value is TimeSpan TS)
                    {
                        if (!(Setting is TimeSpanHostSetting TimeSpanSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            TimeSpanSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, TS, TimeSpanSetting);
                    }
                    else if (Value is Enum E)
                    {
                        if (!(Setting is EnumHostSetting EnumSetting))
                        {
                            if (!(Setting is null))
                                await Database.Delete(Setting);

                            EnumSetting = null;
                        }

                        return await SetAsyncLocked(Host, Key, E, EnumSetting);
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

                if (Setting is ObjectHostSetting ObjectSetting)
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
                    Setting = new ObjectHostSetting(Host, Key, Value);
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
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Key name.</param>
        /// <returns>If a setting was found with the given name and deleted.</returns>
        public static async Task<bool> DeleteAsync(string Host, string Key)
        {
            using (Semaphore Semaphore = await Semaphores.BeginWrite("hostsetting:" + Host + " " + Key))
            {
                foreach (HostSetting _ in await Database.FindDelete<HostSetting>(new FilterAnd(
                    new FilterFieldEqualTo("Host", Host), new FilterFieldEqualTo("Key", Key))))
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
        /// <param name="HostAsKey">If the host parameter should be the key in the resultig dictionary (true) or 
        /// the key parameter (false).</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhere(Filter Filter, bool HostAsKey)
        {
            return GetWhereAsync(Filter, HostAsKey).Result;
        }

        /// <summary>
        /// Gets available settings, matching a search filter.
        /// </summary>
        /// <param name="Filter">Search filter.</param>
        /// <param name="HostAsKey">If the host parameter should be the key in the resultig dictionary (true) or 
        /// the key parameter (false).</param>
        /// <returns>Matching settings found.</returns>
        public static async Task<Dictionary<string, object>> GetWhereAsync(Filter Filter, bool HostAsKey)
        {
            Dictionary<string, object> Result = new Dictionary<string, object>();

            foreach (HostSetting Setting in await Database.Find<HostSetting>(Filter))
                Result[HostAsKey ? Setting.Host : Setting.Key] = Setting.GetValueObject();

            return Result;
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhereKeyLikeRegEx(string Host, string KeyPattern)
        {
            return GetWhere(new FilterAnd(
                new FilterFieldEqualTo("Host", Host), 
                new FilterFieldLikeRegEx("Key", KeyPattern)), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
        /// <returns>Matching settings found.</returns>
        public static Task<Dictionary<string, object>> GetWhereKeyLikeRegExAsync(string Host, string KeyPattern)
        {
            return GetWhereAsync(new FilterAnd(
                new FilterFieldEqualTo("Host", Host), 
                new FilterFieldLikeRegEx("Key", KeyPattern)), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Return settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Matching settings found.</returns>
        public static Dictionary<string, object> GetWhereKeyLike(string Host, string Key, string Wildcard)
        {
            return GetWhere(new FilterAnd(
                new FilterFieldEqualTo("Host", Host),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))), false);
        }

        /// <summary>
        /// Gets available settings, matching a search filter, indexed by key.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Return settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Matching settings found.</returns>
        public static Task<Dictionary<string, object>> GetWhereKeyLikeAsync(string Host, string Key, string Wildcard)
        {
            return GetWhereAsync(new FilterAnd(
                new FilterFieldEqualTo("Host", Host),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))), false);
        }

        /// <summary>
        /// Gets available settings for a given key, indexed by host.
        /// </summary>
        /// <returns>Host settings found.</returns>
        public static Dictionary<string, object> GetHostValues(string Key)
        {
            return GetWhere(new FilterFieldEqualTo("Key", Key), true);
        }

        /// <summary>
        /// Gets available settings for a given key, indexed by host.
        /// </summary>
        /// <returns>Host settings found.</returns>
        public static Task<Dictionary<string, object>> GetHostValuesAsync(string Key)
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

            foreach (HostSetting Setting in await Database.FindDelete<HostSetting>(Filter))
                Result++;

            return Result;
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
        /// <returns>Number of settings deleted.</returns>
        public static int DeleteWhereKeyLikeRegEx(string Host, string KeyPattern)
        {
            return DeleteWhere(new FilterAnd(new FilterFieldEqualTo("Host", Host), new FilterFieldLikeRegEx("Key", KeyPattern)));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
        /// <returns>Number of settings deleted.</returns>
        public static Task<int> DeleteWhereKeyLikeRegExAsync(string Host, string KeyPattern)
        {
            return DeleteWhereAsync(new FilterAnd(new FilterFieldEqualTo("Host", Host), new FilterFieldLikeRegEx("Key", KeyPattern)));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Number of settings deleted.</returns>
        public static int DeleteWhereKeyLike(string Host, string Key, string Wildcard)
        {
            return DeleteWhere(new FilterAnd(new FilterFieldEqualTo("Host", Host),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))));
        }

        /// <summary>
        /// Deletes available settings, matching a search filter.
        /// </summary>
		/// <param name="Host">Name of host.</param>
        /// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
        /// <param name="Wildcard">What wildcard has been used.</param>
        /// <returns>Number of settings deleted.</returns>
        public static Task<int> DeleteWhereKeyLikeAsync(string Host, string Key, string Wildcard)
        {
            return DeleteWhereAsync(new FilterAnd(new FilterFieldEqualTo("Host", Host),
                new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard))));
        }

        #endregion
    }
}