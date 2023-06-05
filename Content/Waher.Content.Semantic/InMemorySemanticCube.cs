using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic cube.
	/// </summary>
	public class InMemorySemanticCube : ISemanticCube
	{
		private readonly ISemanticModel model;
		private readonly SortedDictionary<ISemanticElement, InMemorySemanticPlane> subjects = new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
		private readonly SortedDictionary<ISemanticElement, InMemorySemanticPlane> predicates = new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
		private readonly SortedDictionary<ISemanticElement, InMemorySemanticPlane> objects = new SortedDictionary<ISemanticElement, InMemorySemanticPlane>();
		private InMemorySemanticPlane lastSubject = null;
		private InMemorySemanticPlane lastPredicate = null;
		private InMemorySemanticPlane lastObject = null;

		/// <summary>
		/// In-memory semantic cube.
		/// </summary>
		private InMemorySemanticCube(ISemanticModel Model)
		{
			this.model = Model;
		}

		/// <summary>
		/// Creates an in-memory semantic cube from a semantic model.
		/// </summary>
		/// <param name="Model">Semantic model.</param>
		/// <returns>Semantic cube, created from model.</returns>
		public static async Task<InMemorySemanticCube> Create(ISemanticModel Model)
		{
			InMemorySemanticCube Result = new InMemorySemanticCube(Model);
			IEnumerator<ISemanticTriple> e = Model.GetEnumerator();

			if (e is IAsyncEnumerator eAsync)
			{
				while (await eAsync.MoveNextAsync())
					Result.Add(e.Current);
			}
			else
			{
				while (e.MoveNext())
					Result.Add(e.Current);
			}

			return Result;
		}

		private void Add(ISemanticTriple Triple)
		{
			if ((this.lastSubject is null || !this.lastSubject.Reference.Equals(Triple.Subject)) &&
				!this.subjects.TryGetValue(Triple.Subject, out this.lastSubject))
			{
				this.lastSubject = new InMemorySemanticPlane(Triple.Subject);
				this.subjects[Triple.Subject] = this.lastSubject;
			}

			this.lastSubject.Add(Triple.Predicate, Triple.Object, Triple);

			if ((this.lastPredicate is null || !this.lastPredicate.Reference.Equals(Triple.Predicate)) &&
				!this.predicates.TryGetValue(Triple.Predicate, out this.lastPredicate))
			{
				this.lastPredicate = new InMemorySemanticPlane(Triple.Predicate);
				this.predicates[Triple.Predicate] = this.lastPredicate;
			}

			this.lastPredicate.Add(Triple.Subject, Triple.Object, Triple);

			if ((this.lastObject is null || !this.lastObject.Reference.Equals(Triple.Object)) &&
				!this.objects.TryGetValue(Triple.Object, out this.lastObject))
			{
				this.lastObject = new InMemorySemanticPlane(Triple.Object);
				this.objects[Triple.Object] = this.lastObject;
			}

			this.lastObject.Add(Triple.Subject, Triple.Predicate, Triple);
		}

		/// <summary>
		/// Gets an enumerator of available semantic triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.model.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available semantic triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.model.GetEnumerator();
		}

		/// <summary>
		/// Gets available triples in the cube, having a given subject.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, ordered by predicate (X) and object (Y), or null if none.</returns>
		public Task<ISemanticPlane> GetTriplesBySubject(ISemanticElement Subject)
		{
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
			return Task.FromResult<IEnumerator<ISemanticElement>>(this.subjects.Keys.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator of all predicates.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetPredicateEnumerator()
		{
			return Task.FromResult<IEnumerator<ISemanticElement>>(this.predicates.Keys.GetEnumerator());
		}

		/// <summary>
		/// Gets an enumerator of all objects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		public Task<IEnumerator<ISemanticElement>> GetObjectEnumerator()
		{
			return Task.FromResult<IEnumerator<ISemanticElement>>(this.objects.Keys.GetEnumerator());
		}
	}
}
