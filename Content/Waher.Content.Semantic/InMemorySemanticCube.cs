using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic cube.
	/// </summary>
	public class InMemorySemanticCube : InMemorySemanticModel, ISemanticCube
	{
		private SortedDictionary<ISemanticElement, InMemorySemanticPlane> subjects = null;
		private SortedDictionary<ISemanticElement, InMemorySemanticPlane> predicates = null;
		private SortedDictionary<ISemanticElement, InMemorySemanticPlane> objects = null;

		/// <summary>
		/// In-memory semantic cube.
		/// </summary>
		public InMemorySemanticCube()
		{
		}

		/// <summary>
		/// Creates an in-memory semantic cube from a semantic model.
		/// </summary>
		/// <param name="Model">Semantic model.</param>
		/// <returns>Semantic cube, created from model.</returns>
		public static async Task<InMemorySemanticCube> Create(ISemanticModel Model)
		{
			if (Model is InMemorySemanticCube Result)
				return Result;

			Result = new InMemorySemanticCube();
			await Result.Add(Model);

			return Result;
		}

		/// <summary>
		/// Adds a triple to the cube.
		/// </summary>
		/// <param name="Triple">Semantic triple.</param>
		public override void Add(ISemanticTriple Triple)
		{
			base.Add(Triple);

			this.subjects = null;
			this.predicates = null;
			this.objects = null;
		}

		/// <summary>
		/// Adds a model to the cube.
		/// </summary>
		/// <param name="Model">Semantic model.</param>
		public virtual async Task Add(ISemanticModel Model)
		{
			IEnumerator<ISemanticTriple> e = Model.GetEnumerator();

			if (e is IAsyncEnumerator eAsync)
			{
				while (await eAsync.MoveNextAsync())
					this.Add(e.Current);
			}
			else
			{
				while (e.MoveNext())
					this.Add(e.Current);
			}
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, ordered by predicate (X) and object (Y), or null if none.</returns>
		public Task<ISemanticPlane> GetTriplesBySubject(ISemanticElement Subject)
		{
			this.CheckSubjectsOrdered();

			if (!this.subjects.TryGetValue(Subject, out InMemorySemanticPlane Plane))
				Plane = null;

			return Task.FromResult<ISemanticPlane>(Plane);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, ordered by subject (X) and object (Y), or null if none.</returns>
		public Task<ISemanticPlane> GetTriplesByPredicate(ISemanticElement Predicate)
		{
			this.CheckPredicatesOrdered();

			if (!this.predicates.TryGetValue(Predicate, out InMemorySemanticPlane Plane))
				Plane = null;

			return Task.FromResult<ISemanticPlane>(Plane);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, ordered by subject (X) and predicate (Y), or null if none.</returns>
		public Task<ISemanticPlane> GetTriplesByObject(ISemanticElement Object)
		{
			this.CheckObjectsOrdered();

			if (!this.objects.TryGetValue(Object, out InMemorySemanticPlane Plane))
				Plane = null;

			return Task.FromResult<ISemanticPlane>(Plane);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and predicate.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesBySubjectAndPredicate(ISemanticElement Subject, ISemanticElement Predicate)
		{
			this.CheckSubjectsOrdered();

			if (!this.subjects.TryGetValue(Subject, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByX(Predicate);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesBySubjectAndObject(ISemanticElement Subject, ISemanticElement Object)
		{
			this.CheckSubjectsOrdered();

			if (!this.subjects.TryGetValue(Subject, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByY(Object);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and subject.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByPredicateAndSubject(ISemanticElement Predicate, ISemanticElement Subject)
		{
			this.CheckPredicatesOrdered();

			if (!this.predicates.TryGetValue(Predicate, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByX(Subject);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and object.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByPredicateAndObject(ISemanticElement Predicate, ISemanticElement Object)
		{
			this.CheckPredicatesOrdered();

			if (!this.predicates.TryGetValue(Predicate, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByY(Object);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and subject.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByObjectAndSubject(ISemanticElement Object, ISemanticElement Subject)
		{
			this.CheckObjectsOrdered();

			if (!this.objects.TryGetValue(Object, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByX(Subject);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given object and predicate.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<ISemanticLine> GetTriplesByObjectAndPredicate(ISemanticElement Object, ISemanticElement Predicate)
		{
			this.CheckObjectsOrdered();

			if (!this.objects.TryGetValue(Object, out InMemorySemanticPlane Plane))
				return Task.FromResult<ISemanticLine>(null);

			return Plane.GetTriplesByY(Predicate);
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject, predicate and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		public Task<IEnumerable<ISemanticTriple>> GetTriplesBySubjectAndPredicateAndObject(ISemanticElement Subject, ISemanticElement Predicate, ISemanticElement Object)
		{
			this.CheckSubjectsOrdered();

			if (!this.subjects.TryGetValue(Subject, out InMemorySemanticPlane Plane))
				return Task.FromResult<IEnumerable<ISemanticTriple>>(null);

			return Plane.GetTriplesByXAndY(Predicate, Object);
		}

		/// <summary>
		/// Gets an enumerator of all subjects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetSubjectEnumerator()
		{
			this.CheckSubjectsOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.subjects.Keys.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator of all predicates.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetPredicateEnumerator()
		{
			this.CheckPredicatesOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.predicates.Keys.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator of all objects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetObjectEnumerator()
		{
			this.CheckObjectsOrdered();

			return Task.FromResult<IEnumerator<ISemanticElement>>(this.objects.Keys.GetEnumerator());
		}

		private void CheckSubjectsOrdered()
		{
			if (this.subjects is null)
			{
				SortedDictionary<ISemanticElement, InMemorySemanticPlane> Ordered =
					new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
				ISemanticElement LastPoint = null;
				InMemorySemanticPlane Last = null;

				foreach (ISemanticTriple T in this.triples)
				{
					if ((LastPoint is null || !LastPoint.Equals(T.Subject)) &&
						!Ordered.TryGetValue(T.Subject, out Last))
					{
						Last = new InMemorySemanticPlane(T.Subject);
						Ordered[T.Subject] = Last;
					}

					Last.Add(T.Predicate, T.Object, T);
				}

				this.subjects = Ordered;
			}
		}

		private void CheckPredicatesOrdered()
		{
			if (this.predicates is null)
			{
				SortedDictionary<ISemanticElement, InMemorySemanticPlane> Ordered =
					new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
				ISemanticElement LastPoint = null;
				InMemorySemanticPlane Last = null;

				foreach (ISemanticTriple T in this.triples)
				{
					if ((LastPoint is null || !LastPoint.Equals(T.Predicate)) &&
						!Ordered.TryGetValue(T.Predicate, out Last))
					{
						Last = new InMemorySemanticPlane(T.Predicate);
						Ordered[T.Predicate] = Last;
					}

					Last.Add(T.Subject, T.Object, T);
				}

				this.predicates = Ordered;
			}
		}

		private void CheckObjectsOrdered()
		{
			if (this.objects is null)
			{
				SortedDictionary<ISemanticElement, InMemorySemanticPlane> Ordered =
					new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
				ISemanticElement LastPoint = null;
				InMemorySemanticPlane Last = null;

				foreach (ISemanticTriple T in this.triples)
				{
					if ((LastPoint is null || !LastPoint.Equals(T.Object)) &&
						!Ordered.TryGetValue(T.Object, out Last))
					{
						Last = new InMemorySemanticPlane(T.Object);
						Ordered[T.Object] = Last;
					}

					Last.Add(T.Subject, T.Predicate, T);
				}

				this.objects = Ordered;
			}
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
