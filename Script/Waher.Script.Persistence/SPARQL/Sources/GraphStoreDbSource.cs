using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Inventory;
using Waher.Script.Model;
using Waher.Things;

namespace Waher.Script.Persistence.SPARQL.Sources
{
	/// <summary>
	/// Graph source in the local graph store, based on triples persisted in the object database.
	/// </summary>
	public class GraphStoreDbSource : IGraphSource, ISemanticCube
	{
		private readonly GraphReference reference;
		private ISemanticCube cube = null;

		/// <summary>
		/// Graph source in the local graph store, based on triples persisted in the object database.
		/// </summary>
		public GraphStoreDbSource(GraphReference Reference)
		{
			this.reference = Reference;
		}

		/// <summary>
		/// How well a source with a given URI can be loaded by the class.
		/// </summary>
		/// <param name="_">Source URI</param>
		/// <returns>How well the class supports loading the graph.</returns>
		public Grade Supports(Uri _)
		{
			return Grade.NotAtAll;  // Explicitly selected by processor.
		}

		/// <summary>
		/// Loads the graph
		/// </summary>
		/// <param name="Source">Source URI</param>
		/// <param name="Node">Node performing the loading.</param>
		/// <param name="NullIfNotFound">If null should be returned, if graph is not found.</param>
		/// <param name="Caller">Information about entity making the request.</param>
		/// <returns>Graph, if found, null if not found, and null can be returned.</returns>
		public Task<ISemanticCube> LoadGraph(Uri Source, ScriptNode Node, bool NullIfNotFound,
			RequestOrigin Caller)
		{
			// TODO: Check access privileges

			return Task.FromResult<ISemanticCube>(this);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, ordered by predicate (X) and object (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesBySubject(ISemanticElement Subject)
		{
			if (this.cube is null)
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Subject);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("S", Subject)));

				Result.Add(Triples, 1, 2);

				return Result;
			}
			else
				return await this.cube.GetTriplesBySubject(Subject);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, ordered by subject (X) and object (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesByPredicate(ISemanticElement Predicate)
		{
			if (this.cube is null)
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Predicate);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("P", Predicate)));

				Result.Add(Triples, 0, 2);

				return Result;
			}
			else
				return await this.cube.GetTriplesByPredicate(Predicate);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, ordered by subject (X) and predicate (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesByObject(ISemanticElement Object)
		{
			if (this.cube is null)
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Object);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("O", Object)));

				Result.Add(Triples, 0, 1);

				return Result;
			}
			else
				return await this.cube.GetTriplesByObject(Object);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and predicate.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesBySubjectAndPredicate(ISemanticElement Subject, ISemanticElement Predicate)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Subject, Predicate);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("S", Subject),
					new FilterFieldEqualTo("P", Predicate)));

				Result.Add(Triples, 2);

				return Result;
			}
			else
				return await this.cube.GetTriplesBySubjectAndPredicate(Subject, Predicate);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesBySubjectAndObject(ISemanticElement Subject, ISemanticElement Object)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Subject, Object);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("S", Subject),
					new FilterFieldEqualTo("O", Object)));

				Result.Add(Triples, 1);

				return Result;
			}
			else
				return await this.cube.GetTriplesBySubjectAndObject(Subject, Object);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and subject.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByPredicateAndSubject(ISemanticElement Predicate, ISemanticElement Subject)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Predicate, Subject);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("P", Predicate),
					new FilterFieldEqualTo("S", Subject)));

				Result.Add(Triples, 2);

				return Result;
			}
			else
				return await this.cube.GetTriplesByPredicateAndSubject(Predicate, Subject);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and object.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByPredicateAndObject(ISemanticElement Predicate, ISemanticElement Object)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Predicate, Object);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("P", Predicate),
					new FilterFieldEqualTo("O", Object)));

				Result.Add(Triples, 0);

				return Result;
			}
			else
				return await this.cube.GetTriplesByPredicateAndObject(Predicate, Object);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and subject.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByObjectAndSubject(ISemanticElement Object, ISemanticElement Subject)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Object, Subject);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("O", Object),
					new FilterFieldEqualTo("S", Subject)));

				Result.Add(Triples, 1);

				return Result;
			}
			else
				return await this.cube.GetTriplesByObjectAndSubject(Object, Subject);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and predicate.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByObjectAndPredicate(ISemanticElement Object, ISemanticElement Predicate)
		{
			if (this.cube is null)
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Object, Predicate);
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
					new FilterFieldEqualTo("O", Object),
					new FilterFieldEqualTo("P", Predicate)));

				Result.Add(Triples, 2);

				return Result;
			}
			else
				return await this.cube.GetTriplesByObjectAndPredicate(Object, Predicate);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject, predicate and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<IEnumerable<ISemanticTriple>> GetTriplesBySubjectAndPredicateAndObject(ISemanticElement Subject, ISemanticElement Predicate, ISemanticElement Object)
		{
			if (this.cube is null)
			{
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(new FilterAnd(
				new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey),
				new FilterFieldEqualTo("S", Subject),
				new FilterFieldEqualTo("P", Predicate),
				new FilterFieldEqualTo("O", Object)));

				return Triples;
			}
			else
				return await this.cube.GetTriplesBySubjectAndPredicateAndObject(Subject, Predicate, Object);
		}

		private async Task<ISemanticCube> GetCube()
		{
			if (this.cube is null)
			{
				IEnumerable<DatabaseTriple> Triples = await Database.Find<DatabaseTriple>(
					new FilterFieldEqualTo("GraphKey", this.reference.DatabaseKey));
				InMemorySemanticCube Cube = new InMemorySemanticCube();

				foreach (DatabaseTriple T in Triples)
					Cube.Add(T);

				this.cube = Cube;
			}

			return this.cube;
		}

		/// <summary>
		/// Gets an enumerator of all subjects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetSubjectEnumerator()
		{
			ISemanticCube Cube = await this.GetCube();
			return await Cube.GetSubjectEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of all predicates.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetPredicateEnumerator()
		{
			ISemanticCube Cube = await this.GetCube();
			return await Cube.GetPredicateEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of all objects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetObjectEnumerator()
		{
			ISemanticCube Cube = await this.GetCube();
			return await Cube.GetObjectEnumerator();
		}

		/// <summary>
		/// Gets available triples in the cube, having a given value, along a given axis.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="AxisIndex">Axis Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement Value, int AxisIndex)
		{
			switch (AxisIndex)
			{
				case 0: return await this.GetTriplesBySubject(Value);
				case 1: return await this.GetTriplesByPredicate(Value);
				case 2: return await this.GetTriplesByObject(Value);
				default: return null;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having two given values, along two given axes.
		/// </summary>
		/// <param name="Value1">Value 1</param>
		/// <param name="Axis1Index">Axis 1 Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <param name="Value2">Value 2</param>
		/// <param name="Axis2Index">Axis 2 Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement Value1, int Axis1Index,
			ISemanticElement Value2, int Axis2Index)
		{
			if (Axis1Index == Axis2Index)
			{
				if (Value1.Equals(Value2))
					return await this.GetTriples(Value1, Axis1Index);
			}
			else
			{
				switch (Axis1Index)
				{
					case 0:
						switch (Axis2Index)
						{
							case 1: return await this.GetTriplesBySubjectAndPredicate(Value1, Value2);
							case 2: return await this.GetTriplesBySubjectAndObject(Value1, Value2);
						}
						break;

					case 1:
						switch (Axis2Index)
						{
							case 0: return await this.GetTriplesByPredicateAndSubject(Value1, Value2);
							case 2: return await this.GetTriplesByPredicateAndObject(Value1, Value2);
						}
						break;

					case 2:
						switch (Axis2Index)
						{
							case 0: return await this.GetTriplesByObjectAndSubject(Value1, Value2);
							case 1: return await this.GetTriplesByObjectAndPredicate(Value1, Value2);
						}
						break;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.GetCube().Result.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetCube().Result.GetEnumerator();
		}

	}
}
