using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic cubes.
	/// </summary>
	public interface ISemanticCube : ISemanticModel
	{
		/// <summary>
		/// Gets available triples in the cube, having a given subject.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, ordered by predicate (X) and object (Y), or null if none.</returns>
		Task<ISemanticPlane> GetTriplesBySubject(ISemanticElement Subject);

		/// <summary>
		/// Gets available triples in the cube, having a given predicate.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, ordered by subject (X) and object (Y), or null if none.</returns>
		Task<ISemanticPlane> GetTriplesByPredicate(ISemanticElement Predicate);

		/// <summary>
		/// Gets available triples in the cube, having a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, ordered by subject (X) and predicate (Y), or null if none.</returns>
		Task<ISemanticPlane> GetTriplesByObject(ISemanticElement Object);

		/// <summary>
		/// Gets available triples in the cube, having a given subject and predicate.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesBySubjectAndPredicate(ISemanticElement Subject, ISemanticElement Predicate);

		/// <summary>
		/// Gets available triples in the cube, having a given subject and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesBySubjectAndObject(ISemanticElement Subject, ISemanticElement Object);

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and subject.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByPredicateAndSubject(ISemanticElement Predicate, ISemanticElement Subject);

		/// <summary>
		/// Gets available triples in the cube, having a given predicate and object.
		/// </summary>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByPredicateAndObject(ISemanticElement Predicate, ISemanticElement Object);

		/// <summary>
		/// Gets available triples in the cube, having a given object and subject.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Subject">Subject</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByObjectAndSubject(ISemanticElement Object, ISemanticElement Subject);

		/// <summary>
		/// Gets available triples in the cube, having a given object and predicate.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Predicate">Predicate</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<ISemanticLine> GetTriplesByObjectAndPredicate(ISemanticElement Object, ISemanticElement Predicate);

		/// <summary>
		/// Gets available triples in the cube, having a given subject, predicate and object.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<IEnumerable<ISemanticTriple>> GetTriplesBySubjectAndPredicateAndObject(ISemanticElement Subject, ISemanticElement Predicate, ISemanticElement Object);

		/// <summary>
		/// Gets an enumerator of all subjects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetSubjectEnumerator();

		/// <summary>
		/// Gets an enumerator of all predicates.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetPredicateEnumerator();

		/// <summary>
		/// Gets an enumerator of all objects.
		/// </summary>
		/// <returns>Enumerator of semantic elements.</returns>
		Task<IEnumerator<ISemanticElement>> GetObjectEnumerator();

		/// <summary>
		/// Returns a new cube, with a subject restriction.
		/// </summary>
		/// <param name="Subject">Subject restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		Task<ISemanticCube> RestrictSubject(ISemanticElement Subject);

		/// <summary>
		/// Returns a new cube, with a predicate restriction.
		/// </summary>
		/// <param name="Predicate">Predicate restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		Task<ISemanticCube> RestrictPredicate(ISemanticElement Predicate);

		/// <summary>
		/// Returns a new cube, with a object restriction.
		/// </summary>
		/// <param name="Object">Object restriction.</param>
		/// <returns>Restricted cube, or null if empty.</returns>
		Task<ISemanticCube> RestrictObject(ISemanticElement Object);

		/// <summary>
		/// Gets available triples in the cube, having a given value, along a given axis.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="AxisIndex">Axis Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement Value, int AxisIndex);

		/// <summary>
		/// Gets available triples in the cube, having two given values, along two given axes.
		/// </summary>
		/// <param name="Value1">Value 1</param>
		/// <param name="Axis1Index">Axis 1 Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <param name="Value2">Value 2</param>
		/// <param name="Axis2Index">Axis 2 Index: 0=Subject, 1=Predicate, 2=Object.</param>
		/// <returns>Available triples, or null if none.</returns>
		Task<IEnumerable<ISemanticTriple>> GetTriples(ISemanticElement Value1, int Axis1Index,
			ISemanticElement Value2, int Axis2Index);

	}
}
