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
	}
}
