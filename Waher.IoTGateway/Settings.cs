using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.IoTGateway.SettingObjects;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Static class managing persistent settings.
	/// </summary>
	public static class Settings
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
			Task<string> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<string> GetAsync(string Key, string DefaultValue)
		{
			foreach (StringSetting Setting in await Database.Find<StringSetting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, string Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a string-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, string Value)
		{
			foreach (StringSetting Setting in await Database.Find<StringSetting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			StringSetting NewSetting = new StringSetting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
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
			Task<long> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<long> GetAsync(string Key, long DefaultValue)
		{
			foreach (Int64Setting Setting in await Database.Find<Int64Setting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, long Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a long-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, long Value)
		{
			foreach (Int64Setting Setting in await Database.Find<Int64Setting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			Int64Setting NewSetting = new Int64Setting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
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
			Task<bool> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<bool> GetAsync(string Key, bool DefaultValue)
		{
			foreach (BooleanSetting Setting in await Database.Find<BooleanSetting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, bool Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a bool-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, bool Value)
		{
			foreach (BooleanSetting Setting in await Database.Find<BooleanSetting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			BooleanSetting NewSetting = new BooleanSetting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
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
			Task<DateTime> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<DateTime> GetAsync(string Key, DateTime DefaultValue)
		{
			foreach (DateTimeSetting Setting in await Database.Find<DateTimeSetting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, DateTime Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a DateTime-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, DateTime Value)
		{
			foreach (DateTimeSetting Setting in await Database.Find<DateTimeSetting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			DateTimeSetting NewSetting = new DateTimeSetting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
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
			Task<TimeSpan> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<TimeSpan> GetAsync(string Key, TimeSpan DefaultValue)
		{
			foreach (TimeSpanSetting Setting in await Database.Find<TimeSpanSetting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, TimeSpan Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a TimeSpan-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, TimeSpan Value)
		{
			foreach (TimeSpanSetting Setting in await Database.Find<TimeSpanSetting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			TimeSpanSetting NewSetting = new TimeSpanSetting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
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
			Task<double> Result = GetAsync(Key, DefaultValue);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Gets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<double> GetAsync(string Key, double DefaultValue)
		{
			foreach (DoubleSetting Setting in await Database.Find<DoubleSetting>(new FilterFieldEqualTo("Key", Key)))
				return Setting.Value;

			return DefaultValue;
		}

		/// <summary>
		/// Sets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, double Value)
		{
			Task<bool> Result = SetAsync(Key, Value);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Sets a double-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, double Value)
		{
			foreach (DoubleSetting Setting in await Database.Find<DoubleSetting>(new FilterFieldEqualTo("Key", Key)))
			{
				if (Setting.Value != Value)
				{
					Setting.Value = Value;
					await Database.Update(Setting);

					Log.Informational("Setting updated.", Key, new KeyValuePair<string, object>("Value", Value));
					return true;
				}
				else
					return false;
			}

			DoubleSetting NewSetting = new DoubleSetting(Key, Value);
			await Database.Insert(NewSetting);

			Log.Informational("Setting created.", Key, new KeyValuePair<string, object>("Value", Value));

			return true;
		}

		#endregion
	}
}
