using System;
using System.Collections;
using Waher.Persistence;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Interface for result-set enumerators.
	/// </summary>
	public interface IResultSetEnumerator : IEnumerator, IAsyncEnumerator
	{
	}
}
