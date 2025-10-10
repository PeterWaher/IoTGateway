using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Waher.Security.CallStack
{
	/// <summary>
	/// Represents an explicitly prohibited source.
	/// </summary>
	public class Prohibited
	{
		private readonly object source;

		/// <summary>
		/// Represents an explicitly prohibited source.
		/// </summary>
		/// <param name="Source">Assembly Source.</param>
		public Prohibited(Assembly Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Represents an explicitly prohibited source.
		/// </summary>
		/// <param name="Source">Type Source.</param>
		public Prohibited(Type Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Represents an explicitly prohibited source.
		/// </summary>
		/// <param name="Source">Regular expression of call stack source.</param>
		public Prohibited(Regex Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Represents an explicitly prohibited source.
		/// </summary>
		/// <param name="Source">Text representation of call stack source.</param>
		public Prohibited(string Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Prohibited source.
		/// </summary>
		public object Source => this.source;
	}
}
