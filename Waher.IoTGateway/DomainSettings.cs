using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Settings;

namespace Waher.IoTGateway
{
    /// <summary>
    /// Domain settings.
    /// </summary>
    public static class DomainSettings
    {
        #region Getting settings

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<string> GetSettingAsync(IHostReference HostRef, string Key, string DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<string> GetSettingAsync(string Host, string Key, string DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<long> GetSettingAsync(IHostReference HostRef, string Key, long DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<long> GetSettingAsync(string Host, string Key, long DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<double> GetSettingAsync(IHostReference HostRef, string Key, double DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<double> GetSettingAsync(string Host, string Key, double DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<DateTime> GetSettingAsync(IHostReference HostRef, string Key, DateTime DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<DateTime> GetSettingAsync(string Host, string Key, DateTime DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<TimeSpan> GetSettingAsync(IHostReference HostRef, string Key, TimeSpan DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<TimeSpan> GetSettingAsync(string Host, string Key, TimeSpan DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<Enum> GetSettingAsync(IHostReference HostRef, string Key, Enum DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<Enum> GetSettingAsync(string Host, string Key, Enum DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<bool> GetSettingAsync(IHostReference HostRef, string Key, bool DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<bool> GetSettingAsync(string Host, string Key, bool DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static Task<object> GetSettingAsync(IHostReference HostRef, string Key, object DefaultValue)
        {
            return GetSettingAsync(HostRef.Host, Key, DefaultValue);
        }

        /// <summary>
        /// Gets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="DefaultValue">Default value.</param>
        /// <returns>Setting value.</returns>
        public static async Task<object> GetSettingAsync(string Host, string Key, object DefaultValue)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.GetAsync(Host, Key, DefaultValue);
            else
                return await RuntimeSettings.GetAsync(Key, DefaultValue);
        }

        internal static string IsAlternativeDomain(string Host)
        {
            if (string.IsNullOrEmpty(Host))
                return null;

            int i = Host.LastIndexOf(':');
            if (i > 0)
				Host = Host[..i];

            if (Gateway.IsDomain(Host, true) && !Gateway.IsDomain(Host, false))
                return Host;
            else
                return null;
        }

        #endregion

        #region Setting settings

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, string Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, string Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, long Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, long Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, double Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, double Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, DateTime Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, DateTime Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, TimeSpan Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, TimeSpan Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, Enum Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, Enum Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, bool Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, bool Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static Task<bool> SetSettingAsync(IHostReference HostRef, string Key, object Value)
        {
            return SetSettingAsync(HostRef.Host, Key, Value);
        }

        /// <summary>
        /// Sets a setting that may vary depending on domain.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="Key">Settings key</param>
        /// <param name="Value">Value.</param>
        /// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
        public static async Task<bool> SetSettingAsync(string Host, string Key, object Value)
        {
            Host = IsAlternativeDomain(Host);

            if (!string.IsNullOrEmpty(Host))
                return await HostSettings.SetAsync(Host, Key, Value);
            else
                return await RuntimeSettings.SetAsync(Key, Value);
        }

        #endregion

        #region Getting file-based settings

        /// <summary>
        /// Gets the contents of a configurable domain-sensitive text file.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Setting value.</returns>
        public static Task<string> GetTextFileSettingAsync(IHostReference HostRef, string FileName)
        {
            return GetTextFileSettingAsync(HostRef.Host, FileName);
        }

        /// <summary>
        /// Gets the contents of a configurable domain-sensitive text file.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Setting value.</returns>
        public static async Task<string> GetTextFileSettingAsync(string Host, string FileName)
        {
            Host = IsAlternativeDomain(Host);

            string Content;

            if (!string.IsNullOrEmpty(Host))
                Content = await HostSettings.GetAsync(Host, FileName, string.Empty);
            else
                Content = await RuntimeSettings.GetAsync(FileName, string.Empty);

            if (!string.IsNullOrEmpty(Content))
                return Content;

            return await Resources.ReadAllTextAsync(FileName);
        }

        /// <summary>
        /// Gets the contents of a configurable domain-sensitive binary file.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Setting value.</returns>
        public static Task<byte[]> GetBinaryFileSettingAsync(IHostReference HostRef, string FileName)
        {
            return GetBinaryFileSettingAsync(HostRef.Host, FileName);
        }

        /// <summary>
        /// Gets the contents of a configurable domain-sensitive binary file.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Setting value.</returns>
        public static async Task<byte[]> GetBinaryFileSettingAsync(string Host, string FileName)
        {
            Host = IsAlternativeDomain(Host);

            string Content;

            if (!string.IsNullOrEmpty(Host))
                Content = await HostSettings.GetAsync(Host, FileName, string.Empty);
            else
                Content = await RuntimeSettings.GetAsync(FileName, string.Empty);

            if (!string.IsNullOrEmpty(Content))
            {
                try
                {
                    return Convert.FromBase64String(Content);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }

            return await Resources.ReadAllBytesAsync(FileName);
        }

        /// <summary>
        /// Gets the timestamp of contents of a configurable domain-sensitive file.
        /// </summary>
        /// <param name="HostRef">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Timestamp of Setting value.</returns>
        public static Task<DateTime> GetFileSettingTimestampAsync(IHostReference HostRef, string FileName)
        {
            return GetFileSettingTimestampAsync(HostRef.Host, FileName);
        }

        /// <summary>
        /// Gets the timestamp of contents of a configurable domain-sensitive file.
        /// </summary>
        /// <param name="Host">Host reference</param>
        /// <param name="FileName">Full path of file.</param>
        /// <returns>Timestamp of Setting value.</returns>
        public static async Task<DateTime> GetFileSettingTimestampAsync(string Host, string FileName)
        {
            Host = IsAlternativeDomain(Host);

            DateTime Timestamp;

            if (!string.IsNullOrEmpty(Host))
                Timestamp = await HostSettings.GetAsync(Host, FileName, DateTime.MinValue);
            else
                Timestamp = await RuntimeSettings.GetAsync(FileName, DateTime.MinValue);

            if (Timestamp == DateTime.MinValue)
                return File.GetLastWriteTime(FileName);
            else
                return Timestamp;
        }

        #endregion
    }
}
