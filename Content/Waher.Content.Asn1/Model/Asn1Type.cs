using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 types.
	/// </summary>
	public abstract class Asn1Type : Asn1Node
	{
		private readonly bool _implicit;

		/// <summary>
		/// Abstract base class for ASN.1 types.
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Type(bool Implicit)
		{
			this._implicit = Implicit;
		}

		/// <summary>
		/// Implicit type definition
		/// </summary>
		public bool Implicit => this._implicit;

		/// <summary>
		/// C# type reference.
		/// </summary>
		public virtual string CSharpTypeReference
		{
			get
			{
				throw new NotImplementedException("Support for exporting type references of type " +
					this.GetType().FullName + " not implemented.");
			}
		}

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public virtual bool CSharpTypeNullable
		{
			get
			{
				throw new NotImplementedException("Nullable information for type " +
					this.GetType().FullName + " not implemented.");
			}
		}
	}
}
