using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Counters;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Contains a reference to a graph in the graph store.
	/// </summary>
	[CollectionName("GraphStore")]
	[TypeName(TypeNameSerialization.None)]
	[Index("GraphUri")]
	[Index("Created")]
	public class GraphReference
	{
		/// <summary>
		/// Number of files posted to a graph, before graph is converted to a database graph.
		/// </summary>
		public const int DatabaseMinFileCount = 10;

		/// <summary>
		/// Contains a reference to a graph in the graph store.
		/// </summary>
		public GraphReference()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Graph URI
		/// </summary>
		public CaseInsensitiveString GraphUri { get; set; }

		/// <summary>
		/// Graph Digest, used to create file names.
		/// </summary>
		public CaseInsensitiveString GraphDigest { get; set; }

		/// <summary>
		/// Folder where graph(s) are stored.
		/// </summary>
		public string Folder { get; set; }

		/// <summary>
		/// When graph was created in graph store.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When graph was updated in graph store.
		/// </summary>
		public DateTime Updated { get; set; }

		/// <summary>
		/// Number of files used to define graph.
		/// </summary>
		public int NrFiles { get; set; }

		/// <summary>
		/// Creators of graph
		/// </summary>
		public string[] Creators { get; set; }

		/// <summary>
		/// If triples are persisted in database.
		/// </summary>
		[DefaultValue(false)]
		public bool InDatabase { get; set; }

		/// <summary>
		/// Graph key in database.
		/// </summary>
		[DefaultValue(0L)]
		public long DatabaseKey { get; set; }

		/// <summary>
		/// Gets a Graph Source object corresponding to the graph referenced by the object.
		/// </summary>
		/// <returns>Graph Source object.</returns>
		public async Task<IGraphSource> GetGraphSource()
		{
			if (this.InDatabase)
				return new GraphStoreDbSource(this);

			GraphStoreFileSource FileSource = new GraphStoreFileSource(this);

			if (this.NrFiles <= DatabaseMinFileCount)
				return FileSource;

			ISemanticCube Model = await FileSource.LoadGraph(new Uri(this.GraphUri), true);

			this.DatabaseKey = await RuntimeCounters.IncrementCounter("GraphStore.LastGraphKey");
			this.InDatabase = true;

			await Database.Update(this);
			await this.AddTriplesToDatabase(new IEnumerable<ISemanticTriple>[] { Model });

			return new GraphStoreDbSource(this);
		}

		/// <summary>
		/// Method called when new semantic models have been added to the graph.
		/// </summary>
		/// <param name="Models">Semantic models.</param>
		public async Task ModelsAdded(LinkedList<ISemanticModel> Models)
		{
			if (!this.InDatabase && this.NrFiles > DatabaseMinFileCount)
			{
				GraphStoreFileSource FileSource = new GraphStoreFileSource(this);
				ISemanticCube Model = await FileSource.LoadGraph(new Uri(this.GraphUri), true);

				this.DatabaseKey = await RuntimeCounters.IncrementCounter("GraphStore.LastGraphKey");
				this.InDatabase = true;

				await Database.Update(this);

				Models.Clear();
				if (!(Model is null))
					Models.AddLast(Model);
			}

			await this.AddTriplesToDatabase(Models);
		}

		/// <summary>
		/// Adds semantic triples to the database.
		/// </summary>
		/// <param name="Models">Semantic models to save.</param>
		public async Task AddTriplesToDatabase(IEnumerable<IEnumerable<ISemanticTriple>> Models)
		{
			List<DatabaseTriple> ToSave = new List<DatabaseTriple>();
			int c = 0;

			foreach (IEnumerable<ISemanticTriple> Model in Models)
			{
				foreach (ISemanticTriple T in Model)
				{
					if (T.Subject is SemanticElement S &&
						T.Predicate is SemanticElement P &&
						T.Object is SemanticElement O)
					{
						ToSave.Add(new DatabaseTriple()
						{
							GraphKey = this.DatabaseKey,
							S = S,
							P = P,
							O = O
						});

						c++;

						if (c >= 1000)
						{
							await Database.Insert(ToSave.ToArray());
							ToSave.Clear();
							c = 0;
						}
					}
				}
			}

			if (c > 0)
				await Database.Insert(ToSave.ToArray());
		}
	}
}
