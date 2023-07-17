using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic cube.
	/// </summary>
	public class SemanticDataSet : ISemanticCube
	{
		private readonly LinkedList<ISemanticCube> sources = new LinkedList<ISemanticCube>();
		private ISemanticCube first = null;
		private int count = 0;

		/// <summary>
		/// In-memory semantic cube.
		/// </summary>
		/// <param name="Sources">Sources</param>
		public SemanticDataSet(params ISemanticCube[] Sources)
			: this((IEnumerable<ISemanticCube>)Sources)
		{
		}

		/// <summary>
		/// In-memory semantic cube.
		/// </summary>
		/// <param name="Sources">Sources</param>
		public SemanticDataSet(IEnumerable<ISemanticCube> Sources)
		{
			foreach (ISemanticCube Source in Sources)
			{
				if (!(Source is null))
				{
					if (this.first is null)
						this.first = Source;

					this.sources.AddLast(Source);
					this.count++;
				}
			}
		}

		/// <summary>
		/// Adds a source to the data set.
		/// </summary>
		/// <param name="Source">Source</param>
		public void Add(ISemanticCube Source)
		{
			if (!(Source is null))
			{
				if (this.first is null)
					this.first = Source;

				this.sources.AddLast(Source);
				this.count++;
			}
		}

		/// <summary>
		/// Gets an enumerator of available data sources.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.CreateJoinedEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available data sources.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.CreateJoinedEnumerator();
		}

		private IEnumerator<ISemanticTriple> CreateJoinedEnumerator()
		{
			LinkedList<IEnumerator<ISemanticTriple>> Enumerators = new LinkedList<IEnumerator<ISemanticTriple>>();

			foreach (ISemanticCube Source in this.sources)
				Enumerators.AddLast(Source.GetEnumerator());

			return new JoinedTripleEnumerator(Enumerators, true);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, ordered by predicate (X) and object (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesBySubject(ISemanticElement Subject)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesBySubject(Subject);
			else
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Subject);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesBySubject(Subject), 1, 2);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, ordered by subject (X) and object (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesByPredicate(ISemanticElement Predicate)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByPredicate(Predicate);
			else
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Predicate);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByPredicate(Predicate), 0, 2);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, ordered by subject (X) and predicate (Y), or null if none.</returns>
		public async Task<ISemanticPlane> GetTriplesByObject(ISemanticElement Object)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByObject(Object);
			else
			{
				InMemorySemanticPlane Result = new InMemorySemanticPlane(Object);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByObject(Object), 0, 1);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and predicate.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesBySubjectAndPredicate(ISemanticElement Subject, ISemanticElement Predicate)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesBySubjectAndPredicate(Subject, Predicate);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Subject);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesBySubjectAndPredicate(Subject, Predicate), 2);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesBySubjectAndObject(ISemanticElement Subject, ISemanticElement Object)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesBySubjectAndObject(Subject, Object);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Subject);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesBySubjectAndObject(Subject, Object), 1);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and subject.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByPredicateAndSubject(ISemanticElement Predicate, ISemanticElement Subject)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByPredicateAndSubject(Predicate, Subject);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Predicate);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByPredicateAndSubject(Predicate, Subject), 2);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and object.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByPredicateAndObject(ISemanticElement Predicate, ISemanticElement Object)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByPredicateAndObject(Predicate, Object);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Predicate);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByPredicateAndObject(Predicate, Object), 0);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and subject.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByObjectAndSubject(ISemanticElement Object, ISemanticElement Subject)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByObjectAndSubject(Object, Subject);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Object);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByObjectAndSubject(Object, Subject), 1);

				return Result;
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and predicate.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public async Task<ISemanticLine> GetTriplesByObjectAndPredicate(ISemanticElement Object, ISemanticElement Predicate)
		{
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesByObjectAndPredicate(Object, Predicate);
			else
			{
				InMemorySemanticLine Result = new InMemorySemanticLine(Object);

				foreach (ISemanticCube Source in this.sources)
					Result.Add(await Source.GetTriplesByObjectAndPredicate(Object, Predicate), 0);

				return Result;
			}
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
			if (this.first is null)
				return null;

			if (this.count <= 1)
				return await this.first.GetTriplesBySubjectAndPredicateAndObject(Subject, Predicate, Object);
			else
			{
				LinkedList<ISemanticTriple> Result = new LinkedList<ISemanticTriple>();

				foreach (ISemanticCube Source in this.sources)
				{
					IEnumerable<ISemanticTriple> Part = await this.first.GetTriplesBySubjectAndPredicateAndObject(Subject, Predicate, Object);
					if (Part is null)
						continue;

					foreach (ISemanticTriple Triple in Part)
						Result.AddLast(Triple);
				}

				return Result;
			}
		}

		/// <summary>
		/// Gets an enumerator of all subjects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetSubjectEnumerator()
		{
			LinkedList<IEnumerator<ISemanticElement>> Enumerators = new LinkedList<IEnumerator<ISemanticElement>>();

			foreach (ISemanticCube Source in this.sources)
				Enumerators.AddLast(await Source.GetSubjectEnumerator());

			return new JoinedElementEnumerator(Enumerators, true);
		}

		/// <summary>
		/// Gets an enumerator of all predicates.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetPredicateEnumerator()
		{
			LinkedList<IEnumerator<ISemanticElement>> Enumerators = new LinkedList<IEnumerator<ISemanticElement>>();

			foreach (ISemanticCube Source in this.sources)
				Enumerators.AddLast(await Source.GetPredicateEnumerator());

			return new JoinedElementEnumerator(Enumerators, true);
		}

		/// <summary>
		/// Gets an enumerator of all objects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public async Task<IEnumerator<ISemanticElement>> GetObjectEnumerator()
		{
			LinkedList<IEnumerator<ISemanticElement>> Enumerators = new LinkedList<IEnumerator<ISemanticElement>>();

			foreach (ISemanticCube Source in this.sources)
				Enumerators.AddLast(await Source.GetObjectEnumerator());

			return new JoinedElementEnumerator(Enumerators, true);
		}

		/// <summary>
		/// Returns a new cube, with a subject restriction.
		/// </summary>
		/// <param name="Subject">Subject restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		public async Task<ISemanticCube> RestrictSubject(ISemanticElement Subject)
		{
			ISemanticPlane Plane = await this.GetTriplesBySubject(Subject);
			return await InMemorySemanticCube.Create(Plane);
		}

		/// <summary>
		/// Returns a new cube, with a predicate restriction.
		/// </summary>
		/// <param name="Predicate">Predicate restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		public async Task<ISemanticCube> RestrictPredicate(ISemanticElement Predicate)
		{
			ISemanticPlane Plane = await this.GetTriplesByPredicate(Predicate);
			return await InMemorySemanticCube.Create(Plane);
		}

		/// <summary>
		/// Returns a new cube, with a object restriction.
		/// </summary>
		/// <param name="Object">Object restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		public async Task<ISemanticCube> RestrictObject(ISemanticElement Object)
		{
			ISemanticPlane Plane = await this.GetTriplesByObject(Object);
			return await InMemorySemanticCube.Create(Plane);
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
	}
}
