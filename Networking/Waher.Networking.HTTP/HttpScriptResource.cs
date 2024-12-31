using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Publishes a web resource whose contents is produced by script.
	/// </summary>
	public class HttpScriptResource : HttpAsynchronousResource, IHttpGetMethod
	{
		private readonly Expression script;
		private readonly ScriptNode scriptNode;
		private readonly string referenceFileName;
		private readonly bool userSessions;
		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Publishes a web resource whose contents is produced by script.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Script">Script expression.</param>
		/// <param name="ReferenceFileName">Optional reference file name for script.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpScriptResource(string ResourceName, string Script, string ReferenceFileName, bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: this(ResourceName, new Expression(Script), ReferenceFileName, UserSessions, AuthenticationSchemes)
		{
		}

		/// <summary>
		/// Publishes a web resource whose contents is produced by script.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Script">Script expression.</param>
		/// <param name="ReferenceFileName">Optional reference file name for script.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpScriptResource(string ResourceName, Expression Script, string ReferenceFileName, bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.script = Script;
			this.scriptNode = null;
			this.referenceFileName = ReferenceFileName;
			this.userSessions = UserSessions;
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// Publishes a web resource whose contents is produced by script.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Script">Script expression.</param>
		/// <param name="ReferenceFileName">Optional reference file name for script.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public HttpScriptResource(string ResourceName, ScriptNode Script, string ReferenceFileName, bool UserSessions, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.script = null;
			this.scriptNode = Script;
			this.referenceFileName = ReferenceFileName;
			this.userSessions = UserSessions;
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.userSessions;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return this.authenticationSchemes;
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			Variables v = Request.Session ?? new Variables();
			v["Request"] = Request;
			v["Response"] = Response;

			object Result;

			if (!(this.script is null))
				Result = await this.script.EvaluateAsync(v);
			else if (!(this.scriptNode is null))
			{
				IElement E;

				try
				{
					if (this.scriptNode.IsAsynchronous)
						E = await this.scriptNode.EvaluateAsync(v);
					else
						E = this.scriptNode.Evaluate(v);
				}
				catch (ScriptReturnValueException ex)
				{
					E = ex.ReturnValue;
				}

				Result = E.AssociatedObjectValue;
			}
			else
				Result = null;

			if (Response.ResponseSent)
				return;

			HttpRequestHeader Header = Request.Header;
			if (Header.Accept is null)
			{
				await Response.Return(Result);
				return;
			}

			ContentResponse P = await InternetContent.EncodeAsync(Result, Encoding.UTF8);
			if (P.HasError)
			{
				await Response.SendResponse(P.Error);
				return;
			}

			byte[] Binary = P.Encoded;
			string ContentType = P.ContentType;

			bool Acceptable = Header.Accept.IsAcceptable(ContentType, out double Quality, out AcceptanceLevel TypeAcceptance, null);

			if (!Acceptable || TypeAcceptance == AcceptanceLevel.Wildcard)
			{
				IContentConverter Converter = null;
				string NewContentType = null;
				int i = ContentType.IndexOf(';');
				string ContentTypeNoParams = i < 0 ? ContentType : ContentType[..i].TrimEnd();

				foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
				{
					NewContentType = AcceptRecord.Item;
					if (NewContentType.EndsWith("/*"))
					{
						NewContentType = null;
						continue;
					}

					if (InternetContent.CanConvert(ContentTypeNoParams, NewContentType, out Converter))
					{
						Acceptable = true;
						break;
					}
				}

				if (Converter is null)
				{
					IContentConverter[] Converters = InternetContent.GetConverters(ContentTypeNoParams);

					if (!(Converters is null))
					{
						string BestContentType = null;
						double BestQuality = 0;
						IContentConverter Best = null;
						bool Found;

						foreach (IContentConverter Converter2 in Converters)
						{
							Found = false;

							foreach (string FromContentType in Converter2.FromContentTypes)
							{
								if (ContentTypeNoParams == FromContentType)
								{
									Found = true;
									break;
								}
							}

							if (!Found)
								continue;

							foreach (string ToContentType in Converter2.ToContentTypes)
							{
								if (Header.Accept.IsAcceptable(ToContentType, out double Quality2) && Quality > BestQuality)
								{
									BestContentType = ToContentType;
									BestQuality = Quality;
									Best = Converter2;
								}
								else if (BestQuality == 0 && ToContentType == "*")
								{
									BestContentType = ContentType;
									BestQuality = double.Epsilon;
									Best = Converter2;
								}
							}
						}

						if (!(Best is null) && (!Acceptable || BestQuality >= Quality))
						{
							Acceptable = true;
							Converter = Best;
							NewContentType = BestContentType;
						}
					}
				}

				if (Acceptable && !(Converter is null))
				{
					MemoryStream f2 = null;
					MemoryStream f = new MemoryStream(Binary);

					try
					{
						f2 = new MemoryStream();

						List<string> Alternatives = null;
						string[] Range = Converter.ToContentTypes;
						bool All = Range.Length == 1 && Range[0] == "*";

						foreach (AcceptRecord AcceptRecord in Header.Accept.Records)
						{
							if (AcceptRecord.Item.EndsWith("/*") || AcceptRecord.Item == NewContentType)
								continue;

							if (All || Array.IndexOf(Range, AcceptRecord.Item) >= 0)
							{
								if (Alternatives is null)
									Alternatives = new List<string>();

								Alternatives.Add(AcceptRecord.Item);
							}
						}

						ConversionState State = new ConversionState(ContentType, f, this.referenceFileName, this.ResourceName,
							Request.Header.GetURL(false, false), NewContentType, f2, Request.Session, Alternatives?.ToArray());

						if (await Converter.ConvertAsync(State))
							Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");

						if (State.HasError)
						{
							await Response.SendResponse(State.Error);
							return;
						}

						NewContentType = State.ToContentType;
						ContentType = NewContentType;
						Acceptable = true;
						Binary = f2.ToArray();
					}
					finally
					{
						f?.Dispose();
						f2?.Dispose();
					}
				}
			}

			if (!Acceptable)
			{
				await Response.SendResponse(new NotAcceptableException());
				return;
			}

			await Response.Return(ContentType, true, Binary);
		}
	}
}
