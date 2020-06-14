using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.Vanity
{
	internal class VanityStep
	{
		public char Character;
		public VanityStep[] Next;
		public VanityMap[] Expressions;

		internal static VanityStep CalcStep(char Character, VanityExpression[] Expressions)
		{
			SortedDictionary<char, List<VanityExpression>> Next = new SortedDictionary<char, List<VanityExpression>>();
			List<VanityMap> Maps = null;
			List<VanityStep> Steps = null;

			foreach (VanityExpression Exp in Expressions)
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
						if (!Next.TryGetValue(ch, out List<VanityExpression> List))
						{
							List = new List<VanityExpression>();
							Next[ch] = List;
						}

						List.Add(new VanityExpression()
						{
							Pattern = Exp.Pattern.Substring(1),
							Expression = Exp.Expression,
							MapSeed = Exp.MapSeed,
							Parameters = Exp.Parameters
						});
					}
				}
			}

			foreach (KeyValuePair<char, List<VanityExpression>> P in Next)
			{
				if (Steps is null)
					Steps = new List<VanityStep>();

				Steps.Add(CalcStep(P.Key, P.Value.ToArray()));
			}

			return new VanityStep()
			{
				Character = Character,
				Next = Steps?.ToArray(),
				Expressions = Maps?.ToArray()
			};
		}

		private static void AddMap(ref List<VanityMap> Maps, VanityExpression Exp)
		{
			if (Maps is null)
				Maps = new List<VanityMap>();

			Maps.Add(new VanityMap()
			{
				Expression = Exp.Expression,
				MapSeed = Exp.MapSeed,
				Parameters = Exp.Parameters
			});
		}

	}
}
