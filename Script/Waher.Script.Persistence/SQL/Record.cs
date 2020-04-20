using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Represents one record.
	/// </summary>
	public class Record
	{
		private readonly IElement[] elements;
		private readonly int count;
		private int? hash = null;

		/// <summary>
		/// Represents one record.
		/// </summary>
		/// <param name="Elements">Elements in the record.</param>
		public Record(IElement[] Elements)
		{
			this.elements = Elements;
			this.count = this.elements.Length;
		}

		/// <summary>
		/// Elements in the record.
		/// </summary>
		public IElement[] Elements => this.elements;

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is Record Rec) || this.count != Rec.count)
				return false;

			int i;

			for (i = 0; i < this.count; i++)
			{
				if (!this.elements[i].Equals(Rec.elements[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			if (!this.hash.HasValue)
			{
				int Result = 0;
				int i;

				for (i = 0; i < this.count; i++)
					Result ^= Result << 5 ^ this.elements[i].GetHashCode();

				this.hash = Result;
			}

			return this.hash.Value;
		}

	}
}
