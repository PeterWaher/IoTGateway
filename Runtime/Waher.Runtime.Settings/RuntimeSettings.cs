using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings.SettingObjects;
using Waher.Runtime.Threading;

namespace Waher.Runtime.Settings
{
	/// <summary>
	/// Static class managing persistent settings.
	/// </summary>
	public static class RuntimeSettings
	{
		#region String-valued settings

		/// <summary>
		/// Gets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static string Get(string Key, string DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<string> GetAsync(string Key, string DefaultValue)
		{
			StringSetting Setting = await GetAsync<StringSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		private static async Task<T> GetAsync<T>(string Key)
			where T : class
		{
			using (Semaphore Semaphore = await Semaphores.BeginRead("setting:" + Key))
			{
				T Setting = await Database.FindFirstDeleteRest<T>(new FilterFieldEqualTo("Key", Key));
				return Setting;
			}
		}

		/// <summary>
		/// Sets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, string Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, string Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				StringSetting Setting = await Database.FindFirstDeleteRest<StringSetting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new StringSetting(Key, Value);
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
		}

		#endregion

		#region Int64-valued settings

		/// <summary>
		/// Gets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static long Get(string Key, long DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<long> GetAsync(string Key, long DefaultValue)
		{
			Int64Setting Setting = await GetAsync<Int64Setting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, long Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, long Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				Int64Setting Setting = await Database.FindFirstDeleteRest<Int64Setting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new Int64Setting(Key, Value);
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
		}

		#endregion

		#region Boolean-valued settings

		/// <summary>
		/// Gets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static bool Get(string Key, bool DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<bool> GetAsync(string Key, bool DefaultValue)
		{
			BooleanSetting Setting = await GetAsync<BooleanSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, bool Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, bool Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				BooleanSetting Setting = await Database.FindFirstDeleteRest<BooleanSetting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new BooleanSetting(Key, Value);
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
		}

		#endregion

		#region DateTime-valued settings

		/// <summary>
		/// Gets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static DateTime Get(string Key, DateTime DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<DateTime> GetAsync(string Key, DateTime DefaultValue)
		{
			DateTimeSetting Setting = await GetAsync<DateTimeSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, DateTime Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, DateTime Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				DateTimeSetting Setting = await Database.FindFirstDeleteRest<DateTimeSetting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new DateTimeSetting(Key, Value);
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
		}

		#endregion

		#region TimeSpan-valued settings

		/// <summary>
		/// Gets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static TimeSpan Get(string Key, TimeSpan DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<TimeSpan> GetAsync(string Key, TimeSpan DefaultValue)
		{
			TimeSpanSetting Setting = await GetAsync<TimeSpanSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, TimeSpan Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, TimeSpan Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				TimeSpanSetting Setting = await Database.FindFirstDeleteRest<TimeSpanSetting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new TimeSpanSetting(Key, Value);
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
		}

		#endregion

		#region Double-valued settings

		/// <summary>
		/// Gets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static double Get(string Key, double DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<double> GetAsync(string Key, double DefaultValue)
		{
			DoubleSetting Setting = await GetAsync<DoubleSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, double Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, double Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				DoubleSetting Setting = await Database.FindFirstDeleteRest<DoubleSetting>(new FilterFieldEqualTo("Key", Key));
				if (Setting is null)
				{
					Setting = new DoubleSetting(Key, Value);
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
		}

		#endregion

		#region Delete Setting

		/// <summary>
		/// Deletes a runtime setting
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <returns>If a setting was found with the given name and deleted.</returns>
		public static async Task<bool> DeleteAsync(string Key)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				bool Found = false;

				foreach (Setting Setting in await Database.FindDelete<Setting>(new FilterFieldEqualTo("Key", Key)))
					Found = true;

				return Found;
			}
		}

		#endregion

	}
}
