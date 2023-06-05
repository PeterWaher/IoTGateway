using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Profiling.Events;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Static class for extracting semantic graph information from semantic models.
	/// </summary>
	public static class SemanticGraphs
	{
		/// <summary>
		/// Creates an in-memory semantic cube from a semantic model.
		/// </summary>
		/// <param name="Model">Model</param>
		/// <returns>Cube</returns>
		public static Task<InMemorySemanticCube> CreateInMemoryCube(ISemanticModel Model)
		{
			return InMemorySemanticCube.Create(Model);
		}

		/// <summary>
		/// Clears any tags set on elements in the model.
		/// </summary>
		/// <param name="Model"></param>
		public static void ClearTags(ISemanticModel Model)
		{
			foreach (ISemanticTriple Triple in Model)
			{
				Triple.Subject.Tag = null;
				Triple.Predicate.Tag = null;
				Triple.Object.Tag = null;
			}
		}

		/// <summary>
		/// Gets connected graphs available in a semantic model.
		/// </summary>
		/// <param name="Model">Semantic model.</param>
		/// <returns>Array of connected graphs.</returns>
		public static async Task<SemanticGraph[]> GetConnectedGraphs(ISemanticModel Model)
		{
			InMemorySemanticCube Cube = await InMemorySemanticCube.Create(Model);
			return await GetConnectedGraphs(Model, Cube);
		}

		/// <summary>
		/// Gets connected graphs available in a semantic model.
		/// </summary>
		/// <param name="Model">Semantic model.</param>
		/// <param name="Cube">Cube of model.</param>
		/// <returns>Array of connected graphs.</returns>
		public static async Task<SemanticGraph[]> GetConnectedGraphs(ISemanticModel Model, ISemanticCube Cube)
		{
			Dictionary<ISemanticElement, int> Nodes = new Dictionary<ISemanticElement, int>();
			List<SemanticGraph> Graphs = new List<SemanticGraph>();
			LinkedList<ISemanticTriple> ToCheck = new LinkedList<ISemanticTriple>();
			SymmetricMatrix<bool> Connections = new SymmetricMatrix<bool>();
			int NrTraces = 0;
			int TraceNr;

			ClearTags(Model);

			foreach (ISemanticTriple Triple in Model)
			{
				if (Triple.Subject.Tag is null)
				{
					if (Nodes.TryGetValue(Triple.Subject, out TraceNr))
						Triple.Subject.Tag = TraceNr;
					else
					{
						TraceNr = NrTraces++;
						Triple.Subject.Tag = TraceNr;
						Nodes[Triple.Subject] = TraceNr;
						Connections[TraceNr, TraceNr] = true;
					}

					ToCheck.AddLast(Triple);
				}
				else if (Triple.Object.Tag is null)
				{
					TraceNr = (int)Triple.Subject.Tag;

					if (Triple.Object.IsLiteral)
					{
						Triple.Object.Tag = TraceNr;
						continue;
					}

					if (Nodes.TryGetValue(Triple.Object, out int i))
					{
						if (i == TraceNr)
							continue;

						Connections[i, TraceNr] = true;
						continue;
					}
					else
					{
						Triple.Object.Tag = TraceNr;
						Nodes[Triple.Object] = TraceNr;

						ISemanticPlane Plane = await Cube.GetTriplesByObject(Triple.Object);

						foreach (ISemanticTriple T2 in Plane)
							ToCheck.AddLast(T2);
					}
				}
				else
				{
					if (!Triple.Subject.Tag.Equals(Triple.Object.Tag))
					{
						TraceNr = (int)Triple.Subject.Tag;
						int TraceNr2 = (int)Triple.Object.Tag;

						Connections[TraceNr, TraceNr2] = true;
					}

					continue;
				}

				while (!(ToCheck.First is null))
				{
					ISemanticTriple T = ToCheck.First.Value;
					ToCheck.RemoveFirst();

					if (T.Subject.Tag is null)
					{
						T.Subject.Tag = TraceNr;

						if (Nodes.TryGetValue(T.Subject, out int i))
						{
							if (i != TraceNr)
								Connections[i, TraceNr] = true;
						}
						else
							Nodes[T.Subject] = TraceNr;
					}
					else
					{
						int i = (int)T.Subject.Tag;
						if (i != TraceNr)
							Connections[i, TraceNr] = true;
					}

					if (T.Object.Tag is null)
					{
						if (T.Object.IsLiteral)
							T.Object.Tag = TraceNr;
						else
						{
							if (Nodes.TryGetValue(T.Object, out int i))
							{
								if (i != TraceNr)
									Connections[i, TraceNr] = true;
							}
							else
							{
								T.Object.Tag = TraceNr;
								Nodes[T.Object] = TraceNr;

								ISemanticPlane Plane = await Cube.GetTriplesByObject(T.Object);

								foreach (ISemanticTriple T2 in Plane)
									ToCheck.AddLast(T2);
							}
						}
					}
					else
					{
						int i = (int)T.Object.Tag;
						if (i != TraceNr)
							Connections[i, TraceNr] = true;
					}
				}
			}

			Dictionary<int, SemanticGraph> GraphByTrace = new Dictionary<int, SemanticGraph>();

			while (NrTraces-- > 0)
			{
				if (!Connections[NrTraces, NrTraces])
					continue;

				SortedDictionary<int, bool> Connected = new SortedDictionary<int, bool>();
				GetTraces(Connections, NrTraces, Connected);

				SemanticGraph Graph = new SemanticGraph();
				Graphs.Add(Graph);

				foreach (int i in Connected.Keys)
					GraphByTrace[i] = Graph;
			}

			int LastTraceNr = -1;
			SemanticGraph LastGraph = null;

			foreach (ISemanticTriple Triple in Model)
			{
				TraceNr = (int)Triple.Subject.Tag;

				if (TraceNr != LastTraceNr)
				{
					LastTraceNr = TraceNr;
					LastGraph = GraphByTrace[TraceNr];
				}

				LastGraph.Add(Triple);
			}

			return Graphs.ToArray();
		}

		private static void GetTraces(SymmetricMatrix<bool> M, int TraceNr, SortedDictionary<int, bool> Elements)
		{
			int i;

			for (i = M.Size - 1; i >= 0; i--)
			{
				if (M[i, TraceNr])
				{
					M[i, TraceNr] = false;
					Elements[i] = true;
					GetTraces(M, i, Elements);
				}
			}
		}
	}
}
