using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Interface for iterations of database contents.
	/// </summary>
	public interface IDatabaseIteration<T>
		where T : class
	{
		/// <summary>
		/// Is called when iteration of database is started.
		/// </summary>
		Task StartDatabase();

		/// <summary>
		/// Is called when iteration of database is finished.
		/// </summary>
		Task EndDatabase();

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		Task StartCollection(string CollectionName);

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		Task EndCollection();

		/// <summary>
		/// Is called when an object is processed.
		/// </summary>
		/// <param name="Object">Object being iterated.</param>
		/// <returns>Object ID of object, after optional mapping.</returns>
		Task ProcessObject(T Object);

		/// <summary>
		/// Is called when an incompatible (with <typeparamref name="T"/>) object 
		/// is processed.
		/// </summary>
		/// <param name="ObjectId">Object ID of object.</param>
		Task IncompatibleObject(object ObjectId);

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		Task ReportException(Exception Exception);
	}
}
