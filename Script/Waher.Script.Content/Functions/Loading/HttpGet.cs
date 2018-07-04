using System;
using System.Collections.Generic;
using System.Net.Http;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Loading
{
	/// <summary>
	/// HttpGet(Url)
	/// </summary>
	public class HttpGet : FunctionOneScalarVariable
	{
		/// <summary>
		/// HttpGet(Url)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public HttpGet(ScriptNode FileName, int Start, int Length, Expression Expression)
			: base(FileName, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "httpget"; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			using (HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(10000)
			})
			{
				Uri Url = new Uri(Argument);
				HttpResponseMessage Response = HttpClient.GetAsync(Url).Result;
				Response.EnsureSuccessStatusCode();

				byte[] Bin = Response.Content.ReadAsByteArrayAsync().Result;
				string ContentType = Response.Content.Headers.ContentType.ToString();
				object Decoded = InternetContent.Decode(ContentType, Bin, Url);

				return new ObjectValue(Decoded);
			}
		}
	}
}
