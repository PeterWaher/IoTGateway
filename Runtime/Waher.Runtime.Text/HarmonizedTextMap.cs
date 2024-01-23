using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Waher.Runtime.Text
{
	/// <summary>
	/// Maps strings of text to a harmonized set of strings using collections of
	/// regular expressions and parameters.
	/// </summary>
	public class HarmonizedTextMap
	{
		private readonly Dictionary<string, MappingExpression> mappings = new Dictionary<string, MappingExpression>();
		private MappingStep mappingsStep0 = null;
		private bool hasMaps = false;

		/// <summary>
		/// Maps strings of text to a harmonized set of strings using collections of
		/// regular expressions and parameters.
		/// </summary>
		public HarmonizedTextMap()
		{
		}

		/// <summary>
		/// Registers a mapping.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <param name="MapTo">Strings matching <paramref name="RegexPattern"/> will be mapped to this string.
		/// Named group values found using the regular expression can be used in the map, between curly braces { and }.</param>
		public void RegisterMapping(string RegexPattern, string MapTo)
		{
			this.RegisterMapping(RegexPattern, MapTo, null);
		}

		/// <summary>
		/// Registers a mapping.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <param name="MapTo">Strings matching <paramref name="RegexPattern"/> will be mapped to this string.
		/// Named group values found using the regular expression can be used in the map, between curly braces { and }.</param>
		/// <param name="Tag">Tags the expression with an object. This tag can be used when
		/// unregistering all mappings tagged with the given tag.</param>
		public void RegisterMapping(string RegexPattern, string MapTo, object Tag)
		{
			lock (this.mappings)
			{
				if (!this.mappings.TryGetValue(RegexPattern, out MappingExpression Exp))
				{
					Exp = new MappingExpression()
					{
						Pattern = RegexPattern,
						Expression = new Regex(RegexPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant),
						Tag = Tag
					};
					this.mappings[RegexPattern] = Exp;
				}

				Dictionary<string, bool> Names = new Dictionary<string, bool>();

				foreach (string Name in Exp.Expression.GetGroupNames())
					Names[Name] = true;

				List<KeyValuePair<int, string>> Parameters = new List<KeyValuePair<int, string>>();
				int i = MapTo.IndexOf('{');
				int j;

				while (i >= 0)
				{
					j = MapTo.IndexOf('}', i + 1);
					if (j < 0)
						break;

					string Name = MapTo.Substring(i + 1, j - i - 1);

					if (Names.ContainsKey(Name))
					{
						Parameters.Add(new KeyValuePair<int, string>(i, Name));
						MapTo = MapTo.Remove(i, j - i + 1);
						i = MapTo.IndexOf('{', i);
					}
					else
						i = MapTo.IndexOf('{', i + 1);
				}

				Parameters.Reverse();

				Exp.MapSeed = MapTo;
				Exp.Parameters = Parameters.ToArray();

				this.hasMaps = true;
				this.mappingsStep0 = null;
			}
		}

		/// <summary>
		/// Unregisters a mapping.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <returns>If a mapping matching the parameters was found, and consequently removed.</returns>
		public bool UnregisterMapping(string RegexPattern)
		{
			lock (this.mappings)
			{
				if (this.mappings.Remove(RegexPattern))
				{
					this.hasMaps = this.mappings.Count > 0;
					this.mappingsStep0 = null;
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Unregisters mappings tagged with a specific object.
		/// </summary>
		/// <param name="Tag">Remove all mappings tagged with this object.</param>
		/// <returns>Number of mappings removed.</returns>
		public int UnregisterMappings(object Tag)
		{
			if (Tag is null)
				return 0;

			LinkedList<string> ToRemove = null;
			int NrRemoved = 0;

			lock (this.mappings)
			{
				foreach (KeyValuePair<string, MappingExpression> P in this.mappings)
				{
					if (!(P.Value.Tag is null) && P.Value.Tag.Equals(Tag))
					{
						if (ToRemove is null)
							ToRemove = new LinkedList<string>();

						ToRemove.AddLast(P.Key);
					}
				}

				if (!(ToRemove is null))
				{
					foreach (string Key in ToRemove)
					{
						if (this.mappings.Remove(Key))
							NrRemoved++;
					}
				}
			}

			return NrRemoved;
		}

		/// <summary>
		/// Tries to map a string using registered mappings.
		/// </summary>
		/// <param name="InputString">String to map.</param>
		/// <param name="Harmonized">Harmonized result, if found.</param>
		/// <returns>If a mapping was found to map the string to a harmonized string.</returns>
		public bool TryMap(string InputString, out string Harmonized)
		{
			Harmonized = null;

			if (this.hasMaps)
			{
				MappingStep Step = this.mappingsStep0;
				MappingStep Next;
				char ch2;

				if (Step is null)
				{
					MappingExpression[] Expressions;

					lock (this.mappings)
					{
						Expressions = new MappingExpression[this.mappings.Count];
						this.mappings.Values.CopyTo(Expressions, 0);
					}

					Step = this.mappingsStep0 = MappingStep.CalcStep((char)0, Expressions);
				}

				foreach (char ch in InputString)
				{
					if (!(Step.Expressions is null))
					{
						foreach (Mapping Map in Step.Expressions)
						{
							Match M = Map.Expression.Match(InputString);
							if (M.Success && M.Index == 0 && M.Length == InputString.Length)
							{
								Harmonized = Map.MapSeed;
								foreach (KeyValuePair<int, string> P in Map.Parameters)
									Harmonized = Harmonized.Insert(P.Key, M.Groups[P.Value]?.Value ?? string.Empty);

								return true;
							}
						}
					}

					if (Step.Next is null)
						return false;

					Next = null;

					foreach (MappingStep Step2 in Step.Next)
					{
						ch2 = Step2.Character;
						if (ch2 == ch)
						{
							Next = Step2;
							break;
						}
						else if (ch2 > ch)
							return false;
					}

					if (Next is null)
						return false;

					Step = Next;
				}

				if (!(Step.Next is null))
					return false;

				if (!(Step.Expressions is null))
				{
					foreach (Mapping Map in Step.Expressions)
					{
						Match M = Map.Expression.Match(InputString);
						if (M.Success && M.Index == 0 && M.Length == InputString.Length)
						{
							Harmonized = Map.MapSeed;
							foreach (KeyValuePair<int, string> P in Map.Parameters)
								Harmonized = Harmonized.Insert(P.Key, M.Groups[P.Value]?.Value ?? string.Empty);

							return true;
						}
					}
				}
			}

			return false;
		}

		private class Mapping
		{
			public Regex Expression;
			public string MapSeed;
			public KeyValuePair<int, string>[] Parameters;
		}

		private class MappingExpression
		{
			public string Pattern;
			public Regex Expression;
			public string MapSeed;
			public KeyValuePair<int, string>[] Parameters;
			public object Tag;
		}

		private class MappingStep
		{
			public char Character;
			public MappingStep[] Next;
			public Mapping[] Expressions;

			internal static MappingStep CalcStep(char Character, MappingExpression[] Expressions)
			{
				SortedDictionary<char, List<MappingExpression>> Next = new SortedDictionary<char, List<MappingExpression>>();
				List<Mapping> Maps = null;
				List<MappingStep> Steps = null;

				foreach (MappingExpression Exp in Expressions)
				{
					if (string.IsNullOrEmpty(Exp.Pattern))
						AddMap(ref Maps, Exp);
					else
					{
						char ch = Exp.Pattern[0];

						if ("\\^$.|?*+()[{".IndexOf(ch) >= 0)
							AddMap(ref Maps, Exp);
						else
						{
							if (!Next.TryGetValue(ch, out List<MappingExpression> List))
							{
								List = new List<MappingExpression>();
								Next[ch] = List;
							}

							List.Add(new MappingExpression()
							{
								Pattern = Exp.Pattern.Substring(1),
								Expression = Exp.Expression,
								MapSeed = Exp.MapSeed,
								Parameters = Exp.Parameters,
								Tag = Exp.Tag
							});
						}
					}
				}

				foreach (KeyValuePair<char, List<MappingExpression>> P in Next)
				{
					if (Steps is null)
						Steps = new List<MappingStep>();

					Steps.Add(CalcStep(P.Key, P.Value.ToArray()));
				}

				return new MappingStep()
				{
					Character = Character,
					Next = Steps?.ToArray(),
					Expressions = Maps?.ToArray()
				};
			}

			private static void AddMap(ref List<Mapping> Maps, MappingExpression Exp)
			{
				if (Maps is null)
					Maps = new List<Mapping>();

				Maps.Add(new Mapping()
				{
					Expression = Exp.Expression,
					MapSeed = Exp.MapSeed,
					Parameters = Exp.Parameters
				});
			}

			public override string ToString()
			{
				return new string(this.Character, 1);
			}
		}
	}
}