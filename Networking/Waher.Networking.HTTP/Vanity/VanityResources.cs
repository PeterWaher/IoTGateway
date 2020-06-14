using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Waher.Networking.HTTP.Vanity
{
	/// <summary>
	/// Transforms vanity resource names to actual resource names.
	/// </summary>
	public class VanityResources
	{
		private readonly Dictionary<string, VanityExpression> vanityResources = new Dictionary<string, VanityExpression>();
		private VanityStep vanityStep0 = null;
		private bool hasVanity = false;

		/// <summary>
		/// Transforms vanity resource names to actual resource names.
		/// </summary>
		public VanityResources()
		{
		}

		/// <summary>
		/// Registers a vanity resource.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <param name="MapTo">Resources matching <paramref name="RegexPattern"/> will be mapped to resources of this type.
		/// Named group values found using the regular expression can be used in the map, between curly braces { and }.</param>
		public void RegisterVanityResource(string RegexPattern, string MapTo)
		{
			lock (this.vanityResources)
			{
				if (!this.vanityResources.TryGetValue(RegexPattern, out VanityExpression Exp))
				{
					Exp = new VanityExpression()
					{
						Pattern = RegexPattern,
						Expression = new Regex(RegexPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)
					};
					this.vanityResources[RegexPattern] = Exp;
				}

				List<KeyValuePair<int, string>> Parameters = new List<KeyValuePair<int, string>>();
				int i = MapTo.IndexOf('{');
				int j;

				while (i >= 0)
				{
					j = MapTo.IndexOf('}', i + 1);
					if (j < 0)
						break;

					Parameters.Add(new KeyValuePair<int, string>(i, MapTo.Substring(i + 1, j - i - 1)));
					MapTo = MapTo.Remove(i, j - i + 1);
					i = MapTo.IndexOf('{', i);
				}

				Parameters.Reverse();

				Exp.MapSeed = MapTo;
				Exp.Parameters = Parameters.ToArray();

				this.hasVanity = true;
				this.vanityStep0 = null;
			}
		}

		/// <summary>
		/// Unregisters a vanity resource.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <returns>If a vanity resource matching the parameters was found, and consequently removed.</returns>
		public bool UnregisterVanityResource(string RegexPattern)
		{
			lock (this.vanityResources)
			{
				if (this.vanityResources.Remove(RegexPattern))
				{
					this.hasVanity = this.vanityResources.Count > 0;
					this.vanityStep0 = null;
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Checks if a resource name is a vanity resource name. If so, it is expanded to the true resource name.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>If resource was a vanity resource, and has been updated to reflect the true resource name.</returns>
		public bool CheckVanityResource(ref string ResourceName)
		{
			if (this.hasVanity)
			{
				VanityStep Step = this.vanityStep0;
				VanityStep Next;
				char ch2;

				if (Step is null)
				{
					VanityExpression[] Expressions;

					lock (this.vanityResources)
					{
						Expressions = new VanityExpression[this.vanityResources.Count];
						this.vanityResources.Values.CopyTo(Expressions, 0);
					}

					Step = vanityStep0 = VanityStep.CalcStep((char)0, Expressions);
				}

				foreach (char ch in ResourceName)
				{
					if (!(Step.Expressions is null))
					{
						foreach (VanityMap Map in Step.Expressions)
						{
							Match M = Map.Expression.Match(ResourceName);
							if (M.Success && M.Index == 0 && M.Length == ResourceName.Length)
							{
								ResourceName = Map.MapSeed;
								foreach (KeyValuePair<int, string> P in Map.Parameters)
									ResourceName = ResourceName.Insert(P.Key, M.Groups[P.Value]?.Value ?? string.Empty);

								return true;
							}
						}
					}

					if (Step.Next is null)
						return false;

					Next = null;

					foreach (VanityStep Step2 in Step.Next)
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
			}

			return false;
		}
	}
}
