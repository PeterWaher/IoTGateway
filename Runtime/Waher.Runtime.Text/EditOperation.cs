using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Text
{
	/// <summary>
	/// Type of edit-operation
	/// </summary>
	public enum EditOperation
	{
		/// <summary>
		/// Symbol is kept.
		/// </summary>
		Keep,

		/// <summary>
		/// Symbol is inserted.
		/// </summary>
		Insert,

		/// <summary>
		/// Symbol is deleted.
		/// </summary>
		Delete
	}
}
