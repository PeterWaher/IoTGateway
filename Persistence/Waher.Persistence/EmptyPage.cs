using System;
using System.Collections.Generic;

namespace Waher.Persistence
{
	/// <summary>
	/// Represents an empty page of items.
	/// </summary>
	/// <typeparam name="T">Type of items on the page.</typeparam>
	public class EmptyPage<T> : IPage<T>
		where T : class
	{
		/// <summary>
		/// Items available in the page. The enumeration may be empty.
		/// </summary>
		public IEnumerable<T> Items => Array.Empty<T>();

		/// <summary>
		/// If there may be more pages following this page.
		/// </summary>
		public bool More => false;
	}
}
