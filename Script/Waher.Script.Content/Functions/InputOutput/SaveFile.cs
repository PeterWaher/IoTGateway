using System;
using System.Collections.Generic;
using System.IO;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Content.Functions.InputOutput
{
	/// <summary>
	/// SaveFile(Object,FileName)
	/// </summary>
	public class SaveFile : FunctionTwoVariables
	{
		/// <summary>
		/// SaveFile(Object,FileName)
		/// </summary>
		/// <param name="Object">Object to encode and save.</param>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SaveFile(ScriptNode Object, ScriptNode FileName, int Start, int Length, Expression Expression)
			: base(Object, FileName, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "savefile"; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			string FileName = Argument2.AssociatedObjectValue.ToString();

			return this.DoSave(Argument1.AssociatedObjectValue, FileName, Variables);
		}

		private IElement DoSave(object Obj, string FileName, Variables Variables)
		{
			if (Obj is IMatrix M)
			{
				string[][] Records;
				string[] Record;
				int x, y;
				int Rows = M.Rows;
				int Cols = M.Columns;
				IElement Element;

				Records = new string[Rows][];

				for (y = 0; y < Rows; y++)
				{
					Record = new string[Cols];
					Records[y] = Record;

					for (x = 0; x < Cols; x++)
					{
						Element = M.GetElement(x, y);
						Record[x] = Element.ToString();
					}
				}

				Obj = Records;
			}
			else if (Obj is Graph G)
				Obj = G.CreateBitmap(Variables);

			byte[] Bin = null;

			if (InternetContent.TryGetContentType(Path.GetExtension(FileName), out string ContentType) &&
				InternetContent.Encodes(Obj, out Grade Grade, out IContentEncoder Encoder))
			{
				Bin = Encoder.Encode(Obj, System.Text.Encoding.UTF8, out ContentType, ContentType);
			}

			if (Bin == null)
				Bin = InternetContent.Encode(Obj, System.Text.Encoding.UTF8, out ContentType);

			File.WriteAllBytes(FileName, Bin);

			return new StringValue(ContentType);
		}
	}
}
