using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Runtime.Settings
{
	/// <summary>
	/// Static class managing persistent environment settings. Environment settings 
	/// default to runtime settings if a corresponding environment variable is not
	/// available.
	/// </summary>
	public static class EnvironmentSettings
	{
		#region String-valued settings

		/// <summary>
		/// Gets a string-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static string Get(string EnvironmentVariable, string Key, string DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a string-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<string> GetAsync(string EnvironmentVariable, string Key, string DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null)
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else
				return Task.FromResult(s);
		}

		#endregion

		#region Int64-valued settings

		/// <summary>
		/// Gets a long-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static long Get(string EnvironmentVariable, string Key, long DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a long-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<long> GetAsync(string EnvironmentVariable, string Key, long DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null || !long.TryParse(s, out long Result))
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else
				return Task.FromResult(Result);
		}

		#endregion

		#region Boolean-valued settings

		/// <summary>
		/// Gets a bool-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static bool Get(string EnvironmentVariable, string Key, bool DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a bool-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<bool> GetAsync(string EnvironmentVariable, string Key, bool DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null || !CommonTypes.TryParse(s, out bool Result))
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else
				return Task.FromResult(Result);
		}

		#endregion

		#region DateTime-valued settings

		/// <summary>
		/// Gets a DateTime-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static DateTime Get(string EnvironmentVariable, string Key, DateTime DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a DateTime-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<DateTime> GetAsync(string EnvironmentVariable, string Key, DateTime DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null)
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else if (CommonTypes.TryParseRfc822(s, out DateTimeOffset Result))
				return Task.FromResult(Result.LocalDateTime);
			else if (XML.TryParse(s, out DateTime Result2))
				return Task.FromResult(Result2);
			else if (DateTime.TryParse(s, out Result2))
				return Task.FromResult(Result2);
			else
				return RuntimeSettings.GetAsync(Key, DefaultValue);
		}

		#endregion

		#region TimeSpan-valued settings

		/// <summary>
		/// Gets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static TimeSpan Get(string EnvironmentVariable, string Key, TimeSpan DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<TimeSpan> GetAsync(string EnvironmentVariable, string Key, TimeSpan DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null || !TimeSpan.TryParse(s, out TimeSpan Result))
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else
				return Task.FromResult(Result);
		}

		#endregion

		#region Double-valued settings

		/// <summary>
		/// Gets a double-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static double Get(string EnvironmentVariable, string Key, double DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a double-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<double> GetAsync(string EnvironmentVariable, string Key, double DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null || !CommonTypes.TryParse(s, out double Result))
				return RuntimeSettings.GetAsync(Key, DefaultValue);
			else
				return Task.FromResult(Result);
		}

		#endregion

		#region Enum-valued settings

		/// <summary>
		/// Gets a Enum-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Enum Get(string EnvironmentVariable, string Key, Enum DefaultValue)
		{
			return GetAsync(EnvironmentVariable, Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a Enum-valued setting.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Task<Enum> GetAsync(string EnvironmentVariable, string Key, Enum DefaultValue)
		{
			string s = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (s is null)
				return RuntimeSettings.GetAsync(Key, DefaultValue);

			Type T = DefaultValue.GetType();
			string[] Names = Enum.GetNames(T);
			if (Array.IndexOf(Names, s) < 0)
				return RuntimeSettings.GetAsync(Key, DefaultValue);

			return Task.FromResult((Enum)Enum.Parse(T, s));
		}

		#endregion
	}
}