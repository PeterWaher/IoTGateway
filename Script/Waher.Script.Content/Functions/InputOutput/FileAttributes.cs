using System.IO;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.InputOutput
{
	/// <summary>
	/// FileAttributes(FileName)
	/// </summary>
	public class FileAttributes : FunctionOneScalarStringVariable
	{
		/// <summary>
		/// FileAttributes(FileName)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FileAttributes(ScriptNode FileName, int Start, int Length, Expression Expression)
			: base(FileName, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(FileAttributes);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "FileName" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			string FileName = Path.Combine(Directory.GetCurrentDirectory(), Argument);

			return new ObjectValue(File.GetAttributes(FileName));
		}
	}
}
