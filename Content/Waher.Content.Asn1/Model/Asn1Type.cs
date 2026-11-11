using Waher.Content.Asn1.Model.Values;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 types.
	/// </summary>
	public abstract class Asn1Type : Asn1Node
	{
		private Asn1Tags? tags;
		private Asn1Restriction restriction = null;
		private bool? optional;
		private bool? unique;
		private bool? present;
		private bool? absent;
		private Asn1Node _default = null;
		private Asn1NamedValue[] namedOptions = null;

		/// <summary>
		/// Abstract base class for ASN.1 types.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Type(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// How tags are encoded
		/// </summary>
		public Asn1Tags? Tags
		{
			get => this.tags;
			internal set => this.tags = value;
		}

		/// <summary>
		/// Any restrictions placed on the type.
		/// </summary>
		public Asn1Restriction Restriction
		{
			get => this.restriction;
			internal set
			{
				this.restriction?.SetParentNode(null);
				this.restriction = value;
				this.restriction?.SetParentNode(this);
			}
		}

		/// <summary>
		/// If the node is optional
		/// </summary>
		public override bool? Optional => this.optional;

		/// <summary>
		/// Sets the <see cref="Optional"/> flag of the type.
		/// </summary>
		/// <param name="Value">Value</param>
		internal void SetOptional(bool? Value)
		{ 
			this.optional = Value;
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
			internal set
			{
				this._default?.SetParentNode(null);
				this._default = value;
				this._default?.SetParentNode(this);
			}
		}

		/// <summary>
		/// Any named options.
		/// </summary>
		public Asn1NamedValue[] NamedOptions
		{
			get => this.namedOptions;
			internal set
			{
				if (!(this.namedOptions is null))
				{
					foreach (Asn1NamedValue Value in this.namedOptions)
						Value.SetParentNode(null);
				}

				this.namedOptions = value;

				if (!(this.namedOptions is null))
				{
					foreach (Asn1NamedValue Value in this.namedOptions)
						Value.SetParentNode(this);
				}
			}
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public virtual bool ConstructedType => false;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public virtual Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return Document.ParseValue();
		}
	}
}
