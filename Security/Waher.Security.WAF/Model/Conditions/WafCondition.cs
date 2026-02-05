using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script;
using Waher.Security.WAF.Model.Comparisons;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Abstract base class for Web Application Firewall conditions.
	/// </summary>
	public abstract class WafCondition : WafComparison
	{
		private readonly EnumAttribute<ComparisonMode> mode;
		private readonly StringAttribute value;

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		public WafCondition()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafCondition(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.value = new StringAttribute(Xml, "value");
			this.mode = new EnumAttribute<ComparisonMode>(Xml, "mode");
		}

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <param name="Property">Property value being reviewed.</param>
		/// <returns>Result to return, if any.</returns>
		public async Task<WafResult?> Review(ProcessingState State, string Property)
		{
			if (await this.Applies(State, Property))
				return await this.ReviewChildren(State);
			else
				return null;
		}

		/// <summary>
		/// Checks if the condition applies.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <param name="Property">Property value being reviewed.</param>
		/// <returns>If the condition applies.</returns>
		public async Task<bool> Applies(ProcessingState State, string Property)
		{
			Variables Variables = State.Variables;
			ComparisonMode Mode = await this.mode.EvaluateAsync(Variables, ComparisonMode.ContainsRegex);
			string Value = await this.value.EvaluateAsync(Variables, string.Empty);

			switch (Mode)
			{
				case ComparisonMode.ContainsRegex:
					Regex Regex = GetRegex(Value, false);
					Match M = Regex.Match(Property);
					return M.Success;

				case ComparisonMode.StartsWithRegex:
					Regex = GetRegex(Value, false);
					M = Regex.Match(Property);
					return M.Success && M.Index == 0;

				case ComparisonMode.EndsWithRegex:
					Regex = GetRegex(Value, false);
					M = Regex.Match(Property);
					return M.Success && M.Index + M.Length == Property.Length;

				case ComparisonMode.ExactRegex:
					Regex = GetRegex(Value, false);
					M = Regex.Match(Property);
					return M.Success && M.Index == 0 && M.Length == Property.Length;

				case ComparisonMode.ContainsConstant:
					return Property.Contains(Value, StringComparison.CurrentCulture);

				case ComparisonMode.StartsWithConstant:
					return Property.StartsWith(Value, StringComparison.CurrentCulture);

				case ComparisonMode.EndsWithConstant:
					return Property.EndsWith(Value, StringComparison.CurrentCulture);

				case ComparisonMode.ExactConstant:
					return Property.Equals(Value, StringComparison.CurrentCulture);

				case ComparisonMode.ContainsRegexIgnoreCase:
					Regex = GetRegex(Value, true);
					M = Regex.Match(Property);
					return M.Success;

				case ComparisonMode.StartsWithRegexIgnoreCase:
					Regex = GetRegex(Value, true);
					M = Regex.Match(Property);
					return M.Success && M.Index == 0;

				case ComparisonMode.EndsWithRegexIgnoreCase:
					Regex = GetRegex(Value, true);
					M = Regex.Match(Property);
					return M.Success && M.Index + M.Length == Property.Length;

				case ComparisonMode.ExactRegexIgnoreCase:
					Regex = GetRegex(Value, true);
					M = Regex.Match(Property);
					return M.Success && M.Index == 0 && M.Length == Property.Length;

				case ComparisonMode.ContainsConstantIgnoreCase:
					return Property.Contains(Value, StringComparison.CurrentCultureIgnoreCase);

				case ComparisonMode.StartsWithConstantIgnoreCase:
					return Property.StartsWith(Value, StringComparison.CurrentCultureIgnoreCase);

				case ComparisonMode.EndsWithConstantIgnoreCase:
					return Property.EndsWith(Value, StringComparison.CurrentCultureIgnoreCase);

				case ComparisonMode.ExactConstantIgnoreCase:
					return Property.Equals(Value, StringComparison.CurrentCultureIgnoreCase);

				case ComparisonMode.Script:
					Expression Expression = GetExpression(Value);
					object Result = await Expression.EvaluateAsync(Variables);
					return Result is bool b && b;

				case ComparisonMode.List:
					Dictionary<string, bool> List = GetList(Value, false);
					return List.ContainsKey(Property);

				case ComparisonMode.ListIgnoreCase:
					List = GetList(Value, true);
					return List.ContainsKey(Property);

				case ComparisonMode.FileList:
					List = await this.GetFileList(Value, false);
					return List.ContainsKey(Property);

				case ComparisonMode.FileListIgnoreCase:
					List = await this.GetFileList(Value, true);
					return List.ContainsKey(Property);

				case ComparisonMode.DatabaseList:
					string Key = "DB|" + Value + "|" + Property;
					if (State.TryGetCachedObject(Key, out bool Included))
						return Included;

					WafListProperty DbEntry = await Database.FindFirstIgnoreRest<WafListProperty>(
						new FilterAnd(
							new FilterFieldEqualTo("List", Value),
							new FilterFieldEqualTo("Property", Property)));

					Included = !(DbEntry is null);
					State.AddToCache(Key, Included, fiveMinutes);
					return Included;

				case ComparisonMode.DatabaseListIgnoreCase:
					Key = "DBCI|" + Value + "|" + Property;
					if (State.TryGetCachedObject(Key, out Included))
						return Included;

					WafListPropertyIgnoreCase DbEntry2 = await Database.FindFirstIgnoreRest<WafListPropertyIgnoreCase>(
						new FilterAnd(
							new FilterFieldEqualTo("List", Value),
							new FilterFieldEqualTo("Property", new CaseInsensitiveString(Property))));

					Included = !(DbEntry2 is null);
					State.AddToCache(Key, Included, fiveMinutes);
					return Included;

				default:
					return false;
			}
		}

		private static Regex GetRegex(string Pattern, bool IgnoreCase)
		{
			Dictionary<string, Regex> Expressions = IgnoreCase ?
				regularExpressionsIgnoreCase : regularExpressions;

			lock (Expressions)
			{
				if (!Expressions.TryGetValue(Pattern, out Regex Result))
				{
					RegexOptions Options = RegexOptions.Compiled | RegexOptions.Singleline;
					if (IgnoreCase)
						Options |= RegexOptions.IgnoreCase;

					Result = new Regex(Pattern, Options);
					Expressions[Pattern] = Result;
				}

				return Result;
			}
		}

		private static Expression GetExpression(string s)
		{
			lock (expressions)
			{
				if (!expressions.TryGetValue(s, out Expression Result))
				{
					Result = new Expression(s);
					expressions[s] = Result;
				}

				return Result;
			}
		}

		private static Dictionary<string, bool> GetList(string Pattern, bool IgnoreCase)
		{
			Dictionary<string, Dictionary<string, bool>> Lists = IgnoreCase ?
				listsIgnoreCase : lists;

			lock (Lists)
			{
				if (!Lists.TryGetValue(Pattern, out Dictionary<string, bool> Result))
				{
					Result = new Dictionary<string, bool>(IgnoreCase ?
						StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture);

					foreach (string Part in Pattern.Split(',', StringSplitOptions.RemoveEmptyEntries))
						Result[Part.Trim()] = true;

					Lists[Pattern] = Result;
				}

				return Result;
			}
		}

		private async Task<Dictionary<string, bool>> GetFileList(string FileName, bool IgnoreCase)
		{
			Dictionary<string, FileListInfo> Lists = IgnoreCase ? 
				fileListsIgnoreCase : fileLists;

			FileListInfo Result;
			DateTime Check = DateTime.UtcNow;
			DateTime LastWriteTime = DateTime.MinValue;

			lock (Lists)
			{
				if (Lists.TryGetValue(FileName, out Result))
				{
					if (Check.Subtract(Result.LastTimeCheckUtc).TotalMinutes <= 1)
						return Result.Values;

					LastWriteTime = File.GetLastWriteTimeUtc(Result.FullPath);
					if (LastWriteTime == Result.LastWriteTimeUtc)
					{
						Result.LastTimeCheckUtc = Check;
						return Result.Values;
					}
				}
			}

			string FullFileName = Path.Combine(this.Document.AppDataFolder, FileName);
			string FileContent = await File.ReadAllTextAsync(FullFileName);

			if (LastWriteTime == DateTime.MinValue)
				LastWriteTime = File.GetLastWriteTimeUtc(FullFileName);

			Dictionary<string, bool> List = new Dictionary<string, bool>(IgnoreCase ?
					StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture);

			Result = new FileListInfo()
			{
				FullPath = FullFileName,
				LastWriteTimeUtc = LastWriteTime,
				LastTimeCheckUtc = Check,
				Values = List
			};

			foreach (string Row in FileContent.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
				List[Row.Trim()] = true;

			lock (Lists)
			{
				Lists[FileName] = Result;
			}

			return Result.Values;
		}

		private static readonly Dictionary<string, Regex> regularExpressions = new Dictionary<string, Regex>();
		private static readonly Dictionary<string, Regex> regularExpressionsIgnoreCase = new Dictionary<string, Regex>();
		private static readonly Dictionary<string, Expression> expressions = new Dictionary<string, Expression>();
		private static readonly Dictionary<string, Dictionary<string, bool>> lists = new Dictionary<string, Dictionary<string, bool>>();
		private static readonly Dictionary<string, Dictionary<string, bool>> listsIgnoreCase = new Dictionary<string, Dictionary<string, bool>>();
		private static readonly Dictionary<string, FileListInfo> fileLists = new Dictionary<string, FileListInfo>();
		private static readonly Dictionary<string, FileListInfo> fileListsIgnoreCase = new Dictionary<string, FileListInfo>();

		private class FileListInfo
		{
			public string FullPath;
			public Dictionary<string, bool> Values;
			public DateTime LastWriteTimeUtc;
			public DateTime LastTimeCheckUtc;
		}
	}
}
