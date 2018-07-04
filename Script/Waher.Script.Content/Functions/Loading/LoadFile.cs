using System;
using System.IO;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Loading
{
	/// <summary>
	/// LoadFile(FileName)
	/// </summary>
	public class LoadFile : FunctionOneScalarVariable
    {
		/// <summary>
		/// LoadFile(FileName)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadFile(ScriptNode FileName, int Start, int Length, Expression Expression)
            : base(FileName, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "loadfile"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
			byte[] Bin = File.ReadAllBytes(Argument);
			string ContentType = InternetContent.GetContentType(Path.GetExtension(Argument));
			object Decoded = InternetContent.Decode(ContentType, Bin, new Uri(Argument));

			return new ObjectValue(Decoded);
        }
    }
}
