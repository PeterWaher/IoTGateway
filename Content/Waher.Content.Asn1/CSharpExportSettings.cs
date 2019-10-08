using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// C# export settings
	/// </summary>
	public class CSharpExportSettings
	{
		/// <summary>
		/// What encoders and decoders to include in the generation of C# code.
		/// </summary>
		public EncodingSchemes Codecs = EncodingSchemes.All;

		/// <summary>
		/// Namespace
		/// </summary>
		public string Namespace;

		/// <summary>
		/// What encoders and decoders to include in the generation of C# code.
		/// </summary>
		/// <param name="Namespace">What namespace to generate classes in.</param>
		public CSharpExportSettings(string Namespace)
		{
			this.Namespace = Namespace;
		}
	}
}
