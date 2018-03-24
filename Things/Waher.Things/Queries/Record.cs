using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Defines a record in a table.
	/// </summary>
	public class Record
	{
		private object[] elements;

		/// <summary>
		/// Defines a record in a table.
		/// </summary>
		public Record(params object[] Elements)
		{
			this.elements = Elements;
		}

		/// <summary>
		/// Record elements.
		/// </summary>
		public object[] Elements
		{
			get { return this.elements; }
		}
	}
}
