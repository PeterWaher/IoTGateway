using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 types.
	/// </summary>
	public abstract class Asn1Type : Asn1Node
	{
		private bool _implicit;
		private Asn1Restriction restriction = null;
		private bool? optional = false;
		private bool? unique = false;
		private bool? present = false;
		private bool? absent = false;
		private Asn1Node _default = null;
		private Asn1NamedValue[] namedOptions = null;

		/// <summary>
		/// Abstract base class for ASN.1 types.
		/// </summary>
		public Asn1Type()
		{
		}

		/// <summary>
		/// Implicit type definition
		/// </summary>
		public bool Implicit
		{
			get => this._implicit;
			internal set => this._implicit = value;
		}

		/// <summary>
		/// Any restrictions placed on the type.
		/// </summary>
		public Asn1Restriction Restriction
		{
			get => this.restriction;
			internal set => this.restriction = value;
		}

		/// <summary>
		/// If the type is optional
		/// </summary>
		public bool? Optional
		{
			get => this.optional;
			internal set => this.optional = value;
		}

		/// <summary>
		/// If values must be unique.
		/// </summary>
		public bool? Unique
		{
			get => this.unique;
			internal set => this.unique = value;
		}

		/// <summary>
		/// If the type must be present
		/// </summary>
		public bool? Present
		{
			get => this.present;
			internal set => this.present = value;
		}

		/// <summary>
		/// If the type must be absent
		/// </summary>
		public bool? Absent
		{
			get => this.absent;
			internal set => this.absent = value;
		}

		/// <summary>
		/// Default value for type
		/// </summary>
		public Asn1Node Default
		{
			get => this._default;
			internal set => this._default = value;
		}

		/// <summary>
		/// Any named options.
		/// </summary>
		public Asn1NamedValue[] NamedOptions
		{
			get => this.namedOptions;
			internal set => this.namedOptions = value;
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public virtual bool ConstructedType => false;
	}
}
