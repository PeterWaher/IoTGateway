using System;
using System.Text;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Abstract base class for all meta-data tags.
	/// </summary>
	public abstract class MetaDataTag
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for all meta-data tags.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		public MetaDataTag(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Tag name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// String-representation of meta-data tag value.
		/// </summary>
		public abstract string StringValue
		{
			get;
		}

		/// <summary>
		/// Meta-data tag value.
		/// </summary>
		public abstract object Value
		{
			get;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.name);
			sb.Append('=');
			sb.Append(this.StringValue);

			return sb.ToString();
		}
	}
}
