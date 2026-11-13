using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for values.
	/// </summary>
	public abstract class Asn1Value : Asn1Node
	{
		private bool powerOfTwo = false;

		/// <summary>
		/// Abstract base class for values.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Value(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// C# encoded value.
		/// </summary>
		public async Task<string> GetCSharpValue()
		{
			StringBuilder sb = new StringBuilder();
			await this.ExportCSharp(sb, emptyState, 0, CSharpExportPass.Explicit);
			return sb.ToString();
		}

		private static readonly CSharpExportSettings emptySettings = new CSharpExportSettings(string.Empty);
		private static readonly CSharpExportState emptyState = new CSharpExportState(emptySettings);

		/// <summary>
		/// If the value represents 2^original value.
		/// </summary>
		public bool PowerOfTwo
		{
			get => this.powerOfTwo;
			protected set => this.powerOfTwo = value;
		}

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public virtual void TwosPower()
		{
			throw new NotImplementedException("Invalid power of two.");
		}

	}
}
