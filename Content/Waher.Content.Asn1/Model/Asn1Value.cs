using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for values.
	/// </summary>
	public abstract class Asn1Value : Asn1Node
	{
		/// <summary>
		/// Abstract base class for values.
		/// </summary>
		public Asn1Value()
		{
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public virtual string CSharpType
		{
			get
			{
				throw new NotImplementedException("Support for value type " +
					this.GetType().FullName + " not implemented.");
			}
		}
	}
}
