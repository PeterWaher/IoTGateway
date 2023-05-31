using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Content.Semantic.Model;

namespace Waher.Content.Semantic.Test
{
	public abstract class SemanticTests
	{
		protected static void CompareTriples(ISemanticModel Result, ISemanticModel Expected)
		{
			Dictionary<string, string> NodeIdMap = new();
			List<ISemanticTriple> Triples = new();
			Triples.AddRange(Result);
			List<ISemanticTriple> NotFound = new();

			int i, c = Triples.Count;

			foreach (ISemanticTriple T in Expected)
			{
				bool Match = false;

				for (i = 0; i < c; i++)
				{
					ISemanticTriple T2 = Triples[i];

					if (Matches(Triples[i], T, NodeIdMap, false))
					{
						Assert.IsTrue(Matches(Triples[i], T, NodeIdMap, true), "Blank node inconsistency.");

						Match = true;
						Triples.RemoveAt(i);
						c--;
						break;
					}
				}

				if (!Match)
					NotFound.Add(T);
			}

			if (NotFound.Count > 0 || Triples.Count > 0)
			{
				StringBuilder sb = new();

				sb.AppendLine("Unexpected result.");

				if (NotFound.Count > 0)
				{
					sb.AppendLine();
					sb.AppendLine("Expected Triples not found in result: ");
					sb.AppendLine("=======================================");

					foreach (ISemanticTriple T in NotFound)
					{
						sb.Append(T.Subject.ToString());
						sb.Append('\t');
						sb.Append(T.Predicate.ToString());
						sb.Append('\t');
						sb.AppendLine(T.Object.ToString());
					}
				}

				if (Triples.Count > 0)
				{
					sb.AppendLine();
					sb.AppendLine("Generated Triples not expected: ");
					sb.AppendLine("==================================");

					foreach (ISemanticTriple T in Triples)
					{
						sb.Append(T.Subject.ToString());
						sb.Append('\t');
						sb.Append(T.Predicate.ToString());
						sb.Append('\t');
						sb.AppendLine(T.Object.ToString());
					}
				}

				Assert.Fail(sb.ToString());
			}
		}

		private static bool Matches(ISemanticTriple T1, ISemanticTriple T2, Dictionary<string, string> NodeIdMap, bool AddToMap)
		{
			return Matches(T1.Subject, T2.Subject, NodeIdMap, AddToMap) &&
				Matches(T1.Predicate, T2.Predicate, NodeIdMap, AddToMap) &&
				Matches(T1.Object, T2.Object, NodeIdMap, AddToMap);
		}

		private static bool Matches(ISemanticElement E1, ISemanticElement E2, Dictionary<string, string> NodeIdMap, bool AddToMap)
		{
			if (E1 is BlankNode B1 && E2 is BlankNode B2)
			{
				if (NodeIdMap.TryGetValue(B1.NodeId, out string? NodeId))
					return B2.NodeId == NodeId;

				if (AddToMap)
					NodeIdMap[B1.NodeId] = B2.NodeId;

				return true;
			}
			else
				return E1.ToString() == E2.ToString();
		}
	}
}