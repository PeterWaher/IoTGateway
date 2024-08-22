using Waher.Script.Abstraction.Elements;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Defines a record in a table.
	/// </summary>
	public class Record
	{
		private readonly object[] elements;

		/// <summary>
		/// Defines a record in a table.
		/// </summary>
		public Record(params object[] Elements)
		{
			if (Elements is IElement[] Vector)
			{
				int i, c = Vector.Length;

				this.elements = new object[c];
				for (i = 0; i < c; i++)
					this.elements[i] = Vector[i].AssociatedObjectValue;
			}
			else
				this.elements = Elements;
		}

		/// <summary>
		/// Record elements.
		/// </summary>
		public object[] Elements => this.elements;
	}
}
