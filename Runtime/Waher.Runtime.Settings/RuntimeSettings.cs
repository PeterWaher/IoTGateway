using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, string Value, StringSetting Setting)
		{
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, long Value, Int64Setting Setting)
		{
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, bool Value, BooleanSetting Setting)
		{
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, DateTime Value, DateTimeSetting Setting)
		{
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, TimeSpan Value, TimeSpanSetting Setting)
		{
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
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, double Value, DoubleSetting Setting)
		{
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

		#endregion

		#region Enum-valued settings

		/// <summary>
		/// Gets a Enum-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static Enum Get(string Key, Enum DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a Enum-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<Enum> GetAsync(string Key, Enum DefaultValue)
		{
			EnumSetting Setting = await GetAsync<EnumSetting>(Key);
			return Setting?.Value ?? DefaultValue;
		}

		/// <summary>
		/// Sets a Enum-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, Enum Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a Enum-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, Enum Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				EnumSetting Setting = await Database.FindFirstDeleteRest<EnumSetting>(new FilterFieldEqualTo("Key", Key));
				return await SetAsyncLocked(Key, Value, Setting);
			}
		}

		private static async Task<bool> SetAsyncLocked(string Key, Enum Value, EnumSetting Setting)
		{
			if (Setting is null)
			{
				Setting = new EnumSetting(Key, Value);
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
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static object Get(string Key, object DefaultValue)
		{
			return GetAsync(Key, DefaultValue).Result;
		}

		/// <summary>
		/// Gets a object-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="DefaultValue">Default value, if not found.</param>
		/// <returns>Setting value.</returns>
		public static async Task<object> GetAsync(string Key, object DefaultValue)
		{
			Setting Setting = await GetAsync<Setting>(Key);
			return Setting?.GetValueObject() ?? DefaultValue;
		}

		/// <summary>
		/// Sets a object-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static bool Set(string Key, object Value)
		{
			return SetAsync(Key, Value).Result;
		}

		/// <summary>
		/// Sets a object-valued setting.
		/// </summary>
		/// <param name="Key">Key name.</param>
		/// <param name="Value">New value.</param>
		/// <returns>If the setting was saved (true). If the setting existed, and had the same value, false is returned.</returns>
		public static async Task<bool> SetAsync(string Key, object Value)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("setting:" + Key))
			{
				Setting Setting = await Database.FindFirstDeleteRest<Setting>(new FilterFieldEqualTo("Key", Key));

				if (Value is null)
				{
					if (!(Setting is ObjectSetting))
					{
						await Database.Delete(Setting);
						Setting = null;
					}
				}
				else
				{
					if (Value is string s)
					{
						if (!(Setting is StringSetting StringSetting))
						{
							await Database.Delete(Setting);
							StringSetting = null;
						}

						return await SetAsyncLocked(Key, s, StringSetting);
					}
					else if (Value is long l)
					{
						if (!(Setting is Int64Setting Int64Setting))
						{
							await Database.Delete(Setting);
							Int64Setting = null;
						}

						return await SetAsyncLocked(Key, l, Int64Setting);
					}
					else if (Value is double d)
					{
						if (!(Setting is DoubleSetting DoubleSetting))
						{
							await Database.Delete(Setting);
							DoubleSetting = null;
						}

						return await SetAsyncLocked(Key, d, DoubleSetting);
					}
					else if (Value is bool b)
					{
						if (!(Setting is BooleanSetting BooleanSetting))
						{
							await Database.Delete(Setting);
							BooleanSetting = null;
						}

						return await SetAsyncLocked(Key, b, BooleanSetting);
					}
					else if (Value is DateTime TP)
					{
						if (!(Setting is DateTimeSetting DateTimeSetting))
						{
							await Database.Delete(Setting);
							DateTimeSetting = null;
						}

						return await SetAsyncLocked(Key, TP, DateTimeSetting);
					}
					else if (Value is TimeSpan TS)
					{
						if (!(Setting is TimeSpanSetting TimeSpanSetting))
						{
							await Database.Delete(Setting);
							TimeSpanSetting = null;
						}

						return await SetAsyncLocked(Key, TS, TimeSpanSetting);
					}
					else if (Value is Enum E)
					{
						if (!(Setting is EnumSetting EnumSetting))
						{
							await Database.Delete(Setting);
							EnumSetting = null;
						}

						return await SetAsyncLocked(Key, E, EnumSetting);
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

				if (Setting is ObjectSetting ObjectSetting)
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
					Setting = new ObjectSetting(Key, Value);
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

		#region Batch Get

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="Filter">Search filter.</param>
		/// <returns>Matching settings found.</returns>
		public static Dictionary<string, object> GetWhere(Filter Filter)
		{
			return GetWhereAsync(Filter).Result;
		}

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="Filter">Search filter.</param>
		/// <returns>Matching settings found.</returns>
		public static async Task<Dictionary<string, object>> GetWhereAsync(Filter Filter)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>();

			foreach (Setting Setting in await Database.Find<Setting>(Filter))
				Result[Setting.Key] = Setting.GetValueObject();

			return Result;
		}

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
		/// <returns>Matching settings found.</returns>
		public static Dictionary<string, object> GetWhereKeyLikeRegEx(string KeyPattern)
		{
			return GetWhere(new FilterFieldLikeRegEx("Key", KeyPattern));
		}

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="KeyPattern">Return settings whose keys match this regular expression.</param>
		/// <returns>Matching settings found.</returns>
		public static Task<Dictionary<string, object>> GetWhereKeyLikeRegExAsync(string KeyPattern)
		{
			return GetWhereAsync(new FilterFieldLikeRegEx("Key", KeyPattern));
		}

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="Key">Return settings whose keys match this wildcard expression.</param>
		/// <param name="Wildcard">What wildcard has been used.</param>
		/// <returns>Matching settings found.</returns>
		public static Dictionary<string, object> GetWhereKeyLike(string Key, string Wildcard)
		{
			return GetWhere(new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard)));
		}

		/// <summary>
		/// Gets available settings, matching a search filter.
		/// </summary>
		/// <param name="Key">Return settings whose keys match this wildcard expression.</param>
		/// <param name="Wildcard">What wildcard has been used.</param>
		/// <returns>Matching settings found.</returns>
		public static Task<Dictionary<string, object>> GetWhereKeyLikeAsync(string Key, string Wildcard)
		{
			return GetWhereAsync(new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard)));
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

			foreach (Setting Setting in await Database.FindDelete<Setting>(Filter))
				Result++;

			return Result;
		}

		/// <summary>
		/// Deletes available settings, matching a search filter.
		/// </summary>
		/// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
		/// <returns>Number of settings deleted.</returns>
		public static int DeleteWhereKeyLikeRegEx(string KeyPattern)
		{
			return DeleteWhere(new FilterFieldLikeRegEx("Key", KeyPattern));
		}

		/// <summary>
		/// Deletes available settings, matching a search filter.
		/// </summary>
		/// <param name="KeyPattern">Delete settings whose keys match this regular expression.</param>
		/// <returns>Number of settings deleted.</returns>
		public static Task<int> DeleteWhereKeyLikeRegExAsync(string KeyPattern)
		{
			return DeleteWhereAsync(new FilterFieldLikeRegEx("Key", KeyPattern));
		}

		/// <summary>
		/// Deletes available settings, matching a search filter.
		/// </summary>
		/// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
		/// <param name="Wildcard">What wildcard has been used.</param>
		/// <returns>Number of settings deleted.</returns>
		public static int DeleteWhereKeyLike(string Key, string Wildcard)
		{
			return DeleteWhere(new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard)));
		}

		/// <summary>
		/// Deletes available settings, matching a search filter.
		/// </summary>
		/// <param name="Key">Delete settings whose keys match this wildcard expression.</param>
		/// <param name="Wildcard">What wildcard has been used.</param>
		/// <returns>Number of settings deleted.</returns>
		public static Task<int> DeleteWhereKeyLikeAsync(string Key, string Wildcard)
		{
			return DeleteWhereAsync(new FilterFieldLikeRegEx("Key", Database.WildcardToRegex(Key, Wildcard)));
		}

		#endregion
	}
}
