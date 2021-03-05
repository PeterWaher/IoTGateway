using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using Waher.Content.QR;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// QrEncode(Text,Level[,Width[,Height]])
	/// </summary>
	public class QrEncode : FunctionMultiVariate
	{
		private static readonly QrEncoder encoder = new QrEncoder();

		/// <summary>
		/// QrEncode(Text, Level)
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public QrEncode(ScriptNode Text, ScriptNode Level, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Text, Level }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// QrEncode(Text, Level, Width)
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public QrEncode(ScriptNode Text, ScriptNode Level, ScriptNode Width, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Text, Level, Width }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// QrEncode(Text, Level, Width, Height)
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Height">Height of image.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public QrEncode(ScriptNode Text, ScriptNode Level, ScriptNode Width, ScriptNode Height, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Text, Level, Width, Height }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "qrencode"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Text", "Level", "Width" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string Text = Arguments[0].AssociatedObjectValue?.ToString();
			object Obj = Arguments[1].AssociatedObjectValue;

			if (!(Obj is CorrectionLevel Level) && !Enum.TryParse<CorrectionLevel>(Obj?.ToString(), out Level))
				throw new ScriptRuntimeException("Invalid Error Correction Level.", this);

			if (Arguments.Length == 2)
				return new StringValue(encoder.GenerateMatrix(Level, Text).ToHalfBlockText());

			int Width = (int)(Expression.ToDouble(Arguments[2].AssociatedObjectValue) + 0.5);
			int Height;

			if (Arguments.Length == 3)
				Height = Width;
			else
				Height = (int)(Expression.ToDouble(Arguments[3].AssociatedObjectValue) + 0.5);

			byte[] Rgba = encoder.GenerateMatrix(Level, Text).ToRGBA(Width, Height);

			return new GraphBitmap(PixelInformation.FromRaw(SKColorType.Rgba8888, Rgba, Width, Height, Width << 2));
		}

	}
}
