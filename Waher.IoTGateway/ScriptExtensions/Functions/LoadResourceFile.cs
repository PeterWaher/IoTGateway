using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// LoadResourceFile(LocalResource)
	/// </summary>
	public class LoadResourceFile : FunctionMultiVariate
	{
		/// <summary>
		/// LoadResourceFile(LocalResource)
		/// </summary>
		/// <param name="LocalResource">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadResourceFile(ScriptNode LocalResource, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { LocalResource }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// LoadResourceFile(LocalResource, ContentType)
		/// </summary>
		/// <param name="LocalResource">File name.</param>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadResourceFile(ScriptNode LocalResource, ScriptNode ContentType, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { LocalResource, ContentType }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(LoadResourceFile);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "LocalResource", "ContentType" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

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
			string LocalResource = Arguments[0].AssociatedObjectValue?.ToString();
			string ContentType;

			if (!Gateway.HttpServer.TryGetFileName(LocalResource, true, out string FileName))
				throw new ScriptRuntimeException("Unable to convert local resource to file name.", this);

			if (Arguments.Length > 1)
				ContentType = Arguments[1].AssociatedObjectValue?.ToString();
			else
				ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));

			byte[] Bin;

			try
			{
				using (FileStream fs = File.OpenRead(FileName))
				{
					Bin = await fs.ReadAllAsync();
				}
			}
			catch (IOException)
			{
				string TempFileName = Path.GetTempFileName();
				File.Copy(FileName, TempFileName, true);

				try
				{
					using (FileStream fs = File.OpenRead(TempFileName))
					{
						Bin = await fs.ReadAllAsync();
					}
				}
				finally
				{
					File.Delete(TempFileName);
				}
			}

			object Decoded = await InternetContent.DecodeAsync(ContentType, Bin, new Uri(Gateway.GetUrl(LocalResource)));

			return Expression.Encapsulate(Decoded);
		}
	}
}
