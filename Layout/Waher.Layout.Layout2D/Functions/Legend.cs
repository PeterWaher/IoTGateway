using SkiaSharp;
using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;

namespace Waher.Layout.Layout2D.Functions
{
	/// <summary>
	/// Creates a legend that can be displayed in association with a graph containing multiple series.
	/// </summary>
	public class Legend : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a legend that can be displayed in association with a graph containing multiple series.
		/// </summary>
		/// <param name="Labels">Labels</param>
		/// <param name="Colors">Colors</param>
		/// <param name="NrColumns">Number of columns</param>
		/// <param name="FgColor">Foreground color.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Legend(ScriptNode Labels, ScriptNode Colors, ScriptNode FgColor, ScriptNode NrColumns, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Labels, Colors, FgColor, NrColumns },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Legend);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Labels", "Colors", "FgColor", "NrColumns" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0] is IVector Labels))
				throw new ScriptRuntimeException("Expected a vector of labels.", this);

			if (!(Arguments[1] is IVector Colors))
				throw new ScriptRuntimeException("Expected a vector of colors.", this);

			SKColor FgColor = Graph.ToColor(Arguments[2].AssociatedObjectValue);
			double NrColumnsD = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
			int NrColumns = (int)NrColumnsD;
			if (NrColumns < 0 || NrColumns != NrColumnsD)
				throw new ScriptRuntimeException("Expected a positive integer of columns.", this);

			int i, c = Labels.Dimension;

			if (Colors.Dimension != c)
				throw new ScriptRuntimeException("Vector dimensions do not match.", this);

			string[] Labels2 = new string[c];
			SKColor[] Colors2 = new SKColor[c];

			for (i = 0; i < c; i++)
			{
				Labels2[i] = Labels.GetElement(i).AssociatedObjectValue?.ToString() ?? string.Empty;
				Colors2[i] = Graph.ToColor(Colors.GetElement(i).AssociatedObjectValue);
			}

			return await CreateAsync(Labels2, Colors2, FgColor, NrColumns, Variables);
		}

		/// <summary>
		/// Creates a graph bitmap containing a legend.
		/// </summary>
		/// <param name="Labels">Labels</param>
		/// <param name="Colors">Colors</param>
		/// <param name="FgColor">Foreground color.</param>
		/// <param name="NrColumns">Number of columns.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Graph bitmap.</returns>
		/// <exception cref="ArgumentNullException">If any of the arrays are null.</exception>
		/// <exception cref="ArgumentException">If the array lengths do not match.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the number of columns is zero or negative.</exception>
		public static Task<GraphBitmap> CreateAsync(string[] Labels, SKColor[] Colors, SKColor FgColor, int NrColumns, Variables Variables)
		{
			string LayoutXml = CreateLayout(Labels, Colors, FgColor, NrColumns, Variables);
			return Layout.FromLayout(LayoutXml, Variables);
		}

		/// <summary>
		/// Creates a graph bitmap containing a legend.
		/// </summary>
		/// <param name="Labels">Labels</param>
		/// <param name="Colors">Colors</param>
		/// <param name="FgColor">Foreground color.</param>
		/// <param name="NrColumns">Number of columns.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Graph bitmap.</returns>
		/// <exception cref="ArgumentNullException">If any of the arrays are null.</exception>
		/// <exception cref="ArgumentException">If the array lengths do not match.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the number of columns is zero or negative.</exception>
		public static string CreateLayout(string[] Labels, SKColor[] Colors, SKColor FgColor, int NrColumns, Variables Variables)
		{
			StringBuilder sb = new StringBuilder();

			if (Labels is null)
				throw new ArgumentNullException(nameof(Labels));

			if (Colors is null)
				throw new ArgumentNullException(nameof(Labels));

			int i, c = Labels.Length;

			if (c != Colors.Length)
				throw new ArgumentException("Array sizes do not match.");

			if (NrColumns <= 0)
				throw new ArgumentOutOfRangeException(nameof(NrColumns));

			sb.AppendLine("<Layout2D xmlns=\"http://waher.se/Schema/Layout2D.xsd\"");
			sb.AppendLine("background=\"WhiteBackground\" pen=\"FgPen\"");
			sb.Append("font=\"FgFont\" textColor=\"");
			Append(sb, FgColor);
			sb.AppendLine("\">");
			sb.Append("<SolidPen id=\"FgPen\" color=\"");
			Append(sb, FgColor);
			sb.AppendLine("\" width=\"1px\"/>");
			sb.Append("<Font id=\"FgFont\" color=\"");
			Append(sb, FgColor);
			sb.AppendLine("\" size=\"8pt\"/>");

			for (i = 0; i < c; i++)
			{
				SKColor Color = Colors[i];

				sb.Append("<SolidBackground id=\"Graph");
				sb.Append(i.ToString());
				sb.Append("\" color=\"");
				Append(sb, Color);
				sb.AppendLine("\"/>");
			}

			sb.Append("<Grid columns=\"");
			sb.Append(NrColumns.ToString());
			sb.AppendLine("\">");

			for (i = 0; i < c; i++)
			{
				string Label = Labels[i];

				sb.AppendLine("<Cell>");
				sb.AppendLine("<Margins left=\"1mm\" top=\"1mm\" bottom=\"1mm\" right=\"1mm\">");
				sb.Append("<RoundedRectangle radiusX=\"5mm\" radiusY=\"5mm\" fill=\"Graph");
				sb.Append(i.ToString());
				sb.AppendLine("\">");
				sb.AppendLine("<Margins left=\"0.5em\" right=\"0.5em\" top=\"0.25em\" bottom=\"0.25em\">");
				sb.Append("<Label text=\"");
				sb.Append(XML.Encode(Label));
				sb.AppendLine("\" x=\"50%\" y=\"50%\" halign=\"Center\" valign=\"Center\"/>");
				sb.AppendLine("</Margins>");
				sb.AppendLine("</RoundedRectangle>");
				sb.AppendLine("</Margins>");
				sb.AppendLine("</Cell>");
			}

			sb.AppendLine("</Grid>");
			sb.AppendLine("</Layout2D>");

			return sb.ToString();
		}

		private static void Append(StringBuilder sb, SKColor Color)
		{
			sb.Append('#');
			sb.Append(Color.Red.ToString("X2"));
			sb.Append(Color.Green.ToString("X2"));
			sb.Append(Color.Blue.ToString("X2"));

			if (Color.Alpha != 255)
				sb.Append(Color.Alpha.ToString("X2"));
		}
	}
}
