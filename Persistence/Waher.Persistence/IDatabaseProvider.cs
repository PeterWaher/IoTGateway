using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for database providers that can be plugged into the static <see cref="Database"/> class.
	/// </summary>
	public interface IDatabaseProvider
	{
		/// <summary>
		/// Inserts an object into the default collection of the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		void Insert(object Object);

		// TODO: Insert Many
	}
}
