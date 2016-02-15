using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all nodes in a parsed script tree.
	/// </summary>
	public abstract class ScriptNode
	{
		private int start;
		private int length;

		/// <summary>
		/// Base class for all nodes in a parsed script tree.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ScriptNode(int Start, int Length)
		{
			this.start = Start;
			this.length = Length;
		}

		/// <summary>
		/// Start position in script expression.
		/// </summary>
		public int Start
		{
			get { return this.start; }
		}

		/// <summary>
		/// Length of expression covered by node.
		/// </summary>
		public int Length
		{
			get { return this.length; }
		}

	}
}
