using SkiaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;

namespace Waher.Layout.Layout2D.Functions
{
	/// <summary>
	/// Presents a Layout as a bitmapped graph.
	/// </summary>
	public class Layout : FunctionOneVariable
	{
		/// <summary>
		/// Presents a Layout as a bitmapped graph.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Layout(ScriptNode Xml, int Start, int Length, Expression Expression)
			: base(Xml, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Layout);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "XML" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => base.IsAsynchronous;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return this.EvaluateAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			object Obj = Argument.AssociatedObjectValue;

			if (Obj is string s)
				return await FromLayout(s, Variables);
			else if (Obj is XmlDocument XmlDoc)
				return await FromLayout(XmlDoc, Variables);
			else if (Obj is XmlElement E)
				return await FromLayout(E, Variables);
			else if (Obj is Layout2DDocument Doc)
				return await FromLayout(Doc, Variables);
			else
				throw new ScriptRuntimeException("Expected an XML argument.", this);
		}


		/// <summary>
		/// Generates a <see cref="GraphBitmap"/> from a Layout.
		/// </summary>
		/// <param name="Xml">XML representation of layout document.</param>
		/// <param name="Variables">Variable collection.</param>
		/// <returns>Graph bitmap.</returns>
		public static async Task<GraphBitmap> FromLayout(string Xml, Variables Variables)
		{
			Layout2DDocument Doc = await Layout2DDocument.FromXml(Xml, true, Variables);
			return await FromLayout(Doc, Variables);
		}

		/// <summary>
		/// Generates a <see cref="GraphBitmap"/> from a Layout.
		/// </summary>
		/// <param name="Xml">XML representation of layout document.</param>
		/// <param name="Variables">Variable collection.</param>
		/// <returns>Graph bitmap.</returns>
		public static async Task<GraphBitmap> FromLayout(XmlElement Xml, Variables Variables)
		{
			Layout2DDocument Doc = await Layout2DDocument.FromXml(Xml, Variables);
			return await FromLayout(Doc, Variables);
		}

		/// <summary>
		/// Generates a <see cref="GraphBitmap"/> from a Layout.
		/// </summary>
		/// <param name="Xml">XML representation of layout document.</param>
		/// <param name="Variables">Variable collection.</param>
		/// <returns>Graph bitmap.</returns>
		public static async Task<GraphBitmap> FromLayout(XmlDocument Xml, Variables Variables)
		{
			Layout2DDocument Doc = await Layout2DDocument.FromXml(Xml, Variables);
			return await FromLayout(Doc, Variables);
		}

		/// <summary>
		/// Generates a <see cref="GraphBitmap"/> from a Layout.
		/// </summary>
		/// <param name="Doc">Parsed layout document.</param>
		/// <param name="Variables">Variable collection.</param>
		/// <returns>Graph bitmap.</returns>
		public static async Task<GraphBitmap> FromLayout(Layout2DDocument Doc, Variables Variables)
		{
			RenderSettings Settings = await Doc.GetRenderSettings(Variables);
			KeyValuePair<SKImage, Map[]> P = await Doc.Render(Settings);

			PixelInformation Pixels = PixelInformation.FromImage(P.Key);
			GraphBitmap Result = new GraphBitmap(Variables, Pixels);

			return Result;
		}

	}
}
