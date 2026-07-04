using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Script.Functions.Runtime;
using Waher.Script.Objects.Sets;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	public enum LoginMethod
	{
		CodeForm,
		CodePost,
		CodeFormWithPkceDefault,
		CodeFormWithPkcePlain,
		CodeFormWithPkceS256,
		CodePostWithPkcePlain,
		CodePostWithPkceS256,
		ImplicitGet,
		ImplicitPost,
		Password,
		ClientCredentials,
		ClientCredentialsBasicAuth
	}

	[TestClass]
	public class OAuthTests : IUserSource
	{
		private const string BaseUrl = "http://localhost:8081";
		private const string CallbackResource = "/Callback";
		private const string ProtectedResource = "/Hello";
		private const string Realm = "Test";
		private const string TestUserName = "User";
		private const string TestPassword = "Password";

		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;
		private JwtFactory jwtFactory = null;

		/// <summary>
		/// Test context
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "OAuth";

			SnifferFileName = "Sniffers" + Path.DirectorySeparatorChar + SnifferFileName + ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			this.jwtFactory = JwtFactory.CreateHmacSha256(BaseUrl);
			this.server = new HttpServer(8081, this.xmlSniffer);

			OAuthTokenResource TokenResource;
			OAuthAuthorizeResource AuthorizeResource;

			this.server.Register(new ProtectedResourceMetaData());
			this.server.Register(TokenResource = new OAuthTokenResource(this, this.jwtFactory));
			this.server.Register(AuthorizeResource = new OAuthAuthorizeResource(TokenResource, this.jwtFactory));
			this.server.Register(new AuthorizationServerMetaData(AuthorizeResource));

			this.server.Register(CallbackResource, Callback);
			this.server.Register(new Hello(this.jwtFactory, this));

			AuthorizeResource.ImplicitAuthenticationRequest += async (_, e) =>
			{
				if (e.Request.Header.TryGetHeaderField("X-Context", out HttpField Context))
					e.User = await this.TryGetUser(Context.Value) as IUserWithClaims;
			};
		}

		private static async Task Callback(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetQueryParameter("state", out string State))
				State = string.Empty;

			if (Request.Header.TryGetQueryParameter("error", out string Error) &&
				!string.IsNullOrEmpty(Error))
			{
				Dictionary<string, object> Result = new()
				{
					{ "error", Error },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("error_description", out string ErrorDescription))
					Result["error_description"] = ErrorDescription;

				if (Request.Header.TryGetQueryParameter("error_uri", out string ErrorUri))
					Result["error_uri"] = ErrorUri;

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				await Response.Return(Result);
				return;
			}

			if (Request.Header.TryGetQueryParameter("code", out string Code) &&
				!string.IsNullOrEmpty(Code))
			{
				Dictionary<string, object> Result = new()
				{
					{ "code", Code },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				await Response.Return(Result);
				return;
			}

			if (Request.Header.TryGetQueryParameter("access_token", out string AccessToken) &&
				!string.IsNullOrEmpty(AccessToken))
			{
				Dictionary<string, object> Result = new()
				{
					{ "access_token", AccessToken },
					{ "state", State }
				};

				if (Request.Header.TryGetQueryParameter("token_type", out string TokenType))
					Result["token_type"] = TokenType;

				if (Request.Header.TryGetQueryParameter("expires_in", out string ExpiresIn))
					Result["expires_in"] = ExpiresIn;

				if (Request.Header.TryGetQueryParameter("refresh_token", out string RefreshToken))
					Result["refresh_token"] = RefreshToken;

				if (Request.Header.TryGetQueryParameter("iss", out string Issuer))
					Result["iss"] = Issuer;

				await Response.Return(Result);
				return;
			}

			await Response.SendResponse(new BadRequestException("Missing code or access_token."));
			return;
		}

		[OAuthResourceName("Hello Test Web Service")]
		private class Hello(JwtFactory JwtFactory, IUserSource Users)
			: HttpProtectedResource(ProtectedResource), IHttpGetMethod
		{
			private readonly HttpAuthenticationScheme[] authenticationSchemes =
				[
					new JwtAuthentication(Realm, Users, JwtFactory,
						new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource))
				];

			public bool AllowsGET => true;
			public override bool Synchronous => true;
			public override bool HandlesSubPaths => false;
			public override bool UserSessions => false;

			public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
			{
				return this.authenticationSchemes;
			}

			public async Task GET(HttpRequest Request, HttpResponse Response)
			{
				if (Request.User is null)
				{
					await Response.SendResponse(new ForbiddenException());
					return;
				}

				StringBuilder sb = new();

				sb.Append("Hello ");
				sb.Append(Request.User.UserName);
				sb.AppendLine(".");

				await Response.Return(sb.ToString());
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
			}

			if (this.jwtFactory is not null)
			{
				this.jwtFactory.Dispose();
				this.jwtFactory = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}


		[TestMethod]
		public async Task Test_01_Metadata_Discovery()
		{
			ContentResponse MetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource));
			MetaDataResponse.AssertOk();

			object MetaData = MetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl + ProtectedResource, Required<string>(MetaData, "resource"));
			Assert.Contains(BaseUrl, Required<object[]>(MetaData, "authorization_servers"));
			Assert.Contains("header", Required<object[]>(MetaData, "bearer_methods_supported"));

			ContentResponse ServerMetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + AuthorizationServerMetaData.WellKnowResourcePath));
			ServerMetaDataResponse.AssertOk();

			object ServerMetaData = ServerMetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl, Required<string>(ServerMetaData, "issuer"));
			Assert.AreEqual(BaseUrl + OAuthAuthorizeResource.DefaultResourcePath, Required<string>(ServerMetaData, "authorization_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthTokenResource.DefaultResourcePath, Required<string>(ServerMetaData, "token_endpoint"));
			Assert.Contains("code", Required<object[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("token", Required<object[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("authorization_code", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("password", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("client_credentials", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("plain", Required<object[]>(ServerMetaData, "code_challenge_methods_supported"));
			Assert.Contains("S256", Required<object[]>(ServerMetaData, "code_challenge_methods_supported"));
			Assert.Contains("client_secret_basic",
				Required<object[]>(ServerMetaData, "token_endpoint_auth_methods_supported"));
			Assert.Contains("client_secret_post",
				Required<object[]>(ServerMetaData, "token_endpoint_auth_methods_supported"));
			Assert.IsTrue(Required<bool>(ServerMetaData, "authorization_response_iss_parameter_supported"));

		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_02_Login(LoginMethod Method)
		{
			string AccessToken = await Login(Method);
			await AssertHello(AccessToken);
		}

		private static async Task AssertHello(string AccessToken)
		{
			ContentResponse HelloResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer " + AccessToken));

			HelloResponse.AssertOk();
			Assert.AreEqual("Hello " + TestUserName + "." + Environment.NewLine, HelloResponse.Decoded);
		}

		[TestMethod]
		public async Task Test_03_NoBearerToken()
		{
			ContentResponse Response = await InternetContent.GetAsync(new Uri(BaseUrl + ProtectedResource));
			WebException Error = AssertWebException(Response, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		private static void AssertBearerChallenge(WebException Error)
		{
			bool FoundChallenge = false;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Error.Headers)
			{
				if (Header.Key == "WWW-Authenticate")
				{
					foreach (string Value in Header.Value)
					{
						if (Value.StartsWith("Bearer "))
						{
							FoundChallenge = true;
							string Expected = "resource_metadata=\"" + BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource + "\"";
							Assert.Contains(Expected, Value);
						}
					}
				}
			}

			Assert.IsTrue(FoundChallenge);
		}

		private static void AssertBearerChallenge(WebException Error, string ErrorCode)
		{
			bool FoundChallenge = false;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Error.Headers)
			{
				if (Header.Key == "WWW-Authenticate")
				{
					foreach (string Value in Header.Value)
					{
						if (Value.StartsWith("Bearer ") &&
							Value.Contains("error=\"" + ErrorCode + "\""))
						{
							FoundChallenge = true;
							break;
						}
					}
				}
			}

			Assert.IsTrue(FoundChallenge, "Expected Bearer challenge with error=\"" + ErrorCode + "\".");
		}

		[TestMethod]
		public async Task Test_04_InvalidBearerToken()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer this-is-not-a-jwt"));

			WebException Error = AssertWebException(Response, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		[TestMethod]
		public async Task Test_05_InvalidAuthorizationCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", "invalid-code" },
					{ "code_verifier", "not-used" }
				});

			WebException Error = AssertWebException(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Message);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_06_MissingPkceVerifier(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri }
				});

			WebException Error = AssertWebException(Response, BadRequestException.Code);
			Assert.Contains("Missing code_verifier", Error.Message);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_07_InvalidPkceVerifier(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "code_verifier", CreatePkceCodeVerifier() },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri }
				});

			WebException Error = AssertWebException(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Message);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_08_ReusedAuthorizationCode(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse FirstResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, TestUserName));
			FirstResponse.AssertOk();

			ContentResponse SecondResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, TestUserName));

			Assert.IsTrue(SecondResponse.HasError);
			WebException Error = AssertWebException(SecondResponse, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Message);
		}

		private static Dictionary<string, string> CreateAuthorizationCodeTokenRequest(
			AuthorizationResult AuthorizationCode, string ClientId)
		{
			Dictionary<string, string> Request = new()
			{
				{ "grant_type", "authorization_code" },
				{ "code", AuthorizationCode.Code },
				{ "client_id", ClientId },
				{ "redirect_uri", AuthorizationCode.RedirectUri }
			};

			if (!string.IsNullOrEmpty(AuthorizationCode.CodeVerifier))
				Request["code_verifier"] = AuthorizationCode.CodeVerifier;

			return Request;
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_09_InvalidUserName(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, "Invalid User", TestPassword));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_10_InvalidPassword(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, TestUserName, "Invalid Password"));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_11_MissingUserName(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, null, TestPassword));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodePost)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		[DataRow(LoginMethod.Password)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_12_MissingPassword(LoginMethod Method)
		{
			await Assert.ThrowsAsync<LoginError>(async () => await Login(
				Method, TestUserName, null));
		}

		[TestMethod]
		public async Task Test_13_MissingResponseType()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			});
		}

		[TestMethod]
		public async Task Test_14_UnsupportedResponseType()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=unsupported" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "unsupported" }
			});
		}

		[TestMethod]
		public async Task Test_15_AuthorizationEndpointIgnoresUnknownParameter()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			Dictionary<string, string> FormPostback = new()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			};

			ContentResponse AuthorizeResponse = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				"&unknown_parameter=ignored"));

			AuthorizeResponse.AssertOk();

			HtmlDocument Form = AuthorizeResponse.Decoded as HtmlDocument;
			Assert.IsNotNull(Form);

			Dictionary<string, object> Response = await CompleteAuthorizationForm(
				Form, FormPostback);

			Assert.AreEqual(State, Required<string>(Response, "state"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "code")));
		}

		[TestMethod]
		public async Task Test_16_MissingGrantType()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "code", "not-used" }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_17_UnsupportedGrantType()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "unsupported_grant_type" }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_18_MissingAuthorizationCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "client_id", TestUserName },
					{ "code_verifier", "not-used" }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_19_TokenEndpointIgnoresUnknownParameter(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier },
					{ "unknown_parameter", "ignored" }
				});

			string AccessToken = AssertAccessTokenResponse(TokenResponse);
			await AssertHello(AccessToken);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_20_AuthorizationCodeBoundToClientId(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", "Invalid User" },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertWebException(Response, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_21_UnsupportedPkceCodeChallengeMethod()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string CodeVerifier = CreatePkceCodeVerifier();

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				"&code_challenge=" + Uri.EscapeDataString(CodeVerifier) +
				"&code_challenge_method=unsupported"));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "code" },
				{ "code_challenge", CodeVerifier },
				{ "code_challenge_method", "unsupported" }
			});
		}

		[TestMethod]
		public async Task Test_22_PasswordGrantRejectsClientCredentialParameters()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "password" },
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_23_ClientCredentialsGrantRejectsPasswordParameters()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "client_credentials" },
					{ "username", TestUserName },
					{ "password", TestPassword }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_24_TokenEndpointGetIsRejected()
		{
			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthTokenResource.DefaultResourcePath +
				"?grant_type=password" +
				"&username=" + Uri.EscapeDataString(TestUserName) +
				"&password=" + Uri.EscapeDataString(TestPassword)));

			Assert.IsTrue(Response.HasError);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_25_AuthorizationCodeRequiresRedirectUri(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_26_AuthorizationCodeBoundToRedirectUri(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", BaseUrl + "/WrongCallback" },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertWebException(Response, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_27_EmptyResponseTypeIsTreatedAsMissing()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			});
		}

		[TestMethod]
		public async Task Test_28_EmptyGrantTypeIsTreatedAsMissing()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", string.Empty },
					{ "code", "not-used" }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_29_AuthorizationCodeRequiresClientId(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", AuthorizationCode.CodeVerifier }
				});

			AssertWebException(Response, BadRequestException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		public async Task Test_30_ImplicitGrantDoesNotIssueRefreshToken(LoginMethod Method)
		{
			AuthorizationResult Result = await Authorize(Method);

			Assert.IsTrue(Result.HasToken);
			Assert.IsTrue(string.IsNullOrEmpty(Result.RefreshToken));
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeForm)]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_31_TokenEndpointSuccessfulResponseIsNotCacheable(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);
			Dictionary<string, string> Request = CreateAuthorizationCodeTokenRequest(
				AuthorizationCode, TestUserName);

			using System.Net.Http.HttpClient Client = new();
			using System.Net.Http.FormUrlEncodedContent Content = new(Request);
			using System.Net.Http.HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			object Parsed = JSON.Parse(ResponseText);
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "access_token")));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
		}

		private static void AssertNoStoreHeaders(System.Net.Http.HttpResponseMessage Response)
		{
			Assert.IsNotNull(Response.Headers.CacheControl, "Missing Cache-Control header.");
			Assert.IsTrue(Response.Headers.CacheControl.NoStore, "Expected Cache-Control: no-store.");

			bool FoundPragmaNoCache = false;
			foreach (System.Net.Http.Headers.NameValueHeaderValue Header in Response.Headers.Pragma)
			{
				if (string.Equals(Header.Name, "no-cache", StringComparison.OrdinalIgnoreCase))
				{
					FoundPragmaNoCache = true;
					break;
				}
			}

			Assert.IsTrue(FoundPragmaNoCache, "Expected Pragma: no-cache.");
		}

		[TestMethod]
		public async Task Test_32_DuplicateAuthorizationParameterIsRejected()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&response_type=token" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));

			await AssertAuthorizationError(Response, new Dictionary<string, string>()
			{
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword },
				{ "redirect_uri", RedirectUri },
				{ "state", State },
				{ "response_type", "code" }
			});
		}

		private static async Task<Dictionary<string, object>> AssertAuthorizationError(ContentResponse Response,
			Dictionary<string, string> FormPostback)
		{
			if (Response.HasError)
			{
				AssertWebException(Response, BadRequestException.Code);
				return null;
			}

			Dictionary<string, object> Values;

			if (Response.Decoded is HtmlDocument HtmlDocument)
				Values = await CompleteAuthorizationForm(HtmlDocument, FormPostback);
			else
			{
				Values = Response.Decoded as Dictionary<string, object>;
				Assert.IsNotNull(Values);
			}

			Assert.IsTrue(Values.ContainsKey("error"), "Expected OAuth authorization error response.");

			if (FormPostback is not null &&
				FormPostback.TryGetValue("state", out string ExpectedState))
			{
				Assert.IsTrue(Values.TryGetValue("state", out object ReturnedState),
					"Expected state in OAuth authorization error response.");

				Assert.AreEqual(ExpectedState, ReturnedState);
			}

			return Values;
		}

		[TestMethod]
		public async Task Test_33_DuplicateTokenParameterIsRejected()
		{
			using System.Net.Http.HttpClient Client = new();
			using System.Net.Http.StringContent Content = new(
				"grant_type=password" +
				"&grant_type=client_credentials" +
				"&username=" + Uri.EscapeDataString(TestUserName) +
				"&password=" + Uri.EscapeDataString(TestPassword),
				Encoding.UTF8, "application/x-www-form-urlencoded");

			using System.Net.Http.HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		[TestMethod]
		public async Task Test_34_TokenEndpointRejectsMultipleClientAuthenticationMethods()
		{
			using System.Net.Http.HttpClient Client = new();
			Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(TestUserName + ":" + TestPassword)));

			using System.Net.Http.FormUrlEncodedContent Content = new(new Dictionary<string, string>()
			{
				{ "grant_type", "client_credentials" },
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword }
			});

			using System.Net.Http.HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceS256, "S256")]
		[DataRow(LoginMethod.CodePostWithPkceS256, "S256")]
		public async Task Test_35_S256PkceDowngradeProtection(LoginMethod Method, string MethodName)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);
			string CodeChallenge = CreateCodeChallenge(AuthorizationCode.CodeVerifier, MethodName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", CodeChallenge }
				});

			WebException Error = AssertWebException(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Message);
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceDefault)]
		[DataRow(LoginMethod.CodeFormWithPkcePlain)]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkcePlain)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_36_CodeVerifierWithoutChallengeIsRejected(LoginMethod Method)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", AuthorizationCode.Code },
					{ "client_id", TestUserName },
					{ "redirect_uri", AuthorizationCode.RedirectUri },
					{ "code_verifier", CreatePkceCodeVerifier() }
				});

			AssertWebException(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_37_InvalidBearerTokenChallengeIncludesInvalidTokenError()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer this-is-not-a-jwt"));

			WebException Error = AssertWebException(Response, UnauthorizedException.Code);
			AssertBearerChallenge(Error, "invalid_token");
		}

		[TestMethod]
		public async Task Test_38_BearerTokenInQueryStringIsRejected()
		{
			string AccessToken = await Login(LoginMethod.CodeFormWithPkceS256);

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + ProtectedResource + "?access_token=" + Uri.EscapeDataString(AccessToken)));

			WebException Error = AssertWebException(Response, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		[TestMethod]
		public async Task Test_39_AuthorizationEndpointHasClickjackingProtection()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			using System.Net.Http.HttpClient Client = new();
			using System.Net.Http.HttpResponseMessage Response = await Client.GetAsync(
				BaseUrl + OAuthAuthorizeResource.DefaultResourcePath +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestUserName) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri),
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);

			bool FoundProtection = false;
			if (Response.Headers.TryGetValues("X-Frame-Options", out IEnumerable<string> XFrameOptions))
			{
				foreach (string Value in XFrameOptions)
				{
					if (!string.IsNullOrEmpty(Value))
						FoundProtection = true;
				}
			}

			if (Response.Headers.TryGetValues("Content-Security-Policy", out IEnumerable<string> CspValues))
			{
				foreach (string Value in CspValues)
				{
					if (Value.Contains("frame-ancestors", StringComparison.OrdinalIgnoreCase))
						FoundProtection = true;
				}
			}

			Assert.IsTrue(FoundProtection,
				"Expected X-Frame-Options or Content-Security-Policy frame-ancestors on the authorization page.");
		}

		[TestMethod]
		public async Task Test_40_AccessTokenIsSignedJwtWithExpiration()
		{
			string AccessToken = await Login(LoginMethod.CodeFormWithPkceS256);

			IDictionary<string, object> Header = DecodeJwtPart(AccessToken, 0);
			Assert.IsTrue(Header.TryGetValue("alg", out object Algorithm),
				"Expected JWT header to contain alg.");
			Assert.IsFalse(string.Equals("none", Algorithm as string, StringComparison.OrdinalIgnoreCase),
				"Expected signed JWT access token.");

			IDictionary<string, object> Payload = DecodeJwtPart(AccessToken, 1);
			Assert.IsTrue(Payload.ContainsKey("sub"), "Expected JWT access token to contain sub.");
			AssertPositiveUnixTime(Required<object>(Payload, "exp"), "exp");
		}

		private static WebException AssertWebException(
			ContentResponse Response, params int[] ExpectedStatusCodes)
		{
			Assert.IsTrue(Response.HasError);
			WebException Result = Response.Error as WebException;
			Assert.IsNotNull(Result);

			foreach (int ExpectedStatusCode in ExpectedStatusCodes)
			{
				if ((int)Result.StatusCode == ExpectedStatusCode)
					return Result;
			}

			Assert.Fail("Unexpected status code: " + (int)Result.StatusCode);
			return Result;
		}

		private static void AssertPositiveUnixTime(object Value, string ClaimName)
		{
			long Timestamp = ToInt64(Value, ClaimName);
			long Now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Assert.IsGreaterThan(Now, Timestamp, "Expected " + ClaimName + " to be a future UNIX timestamp.");
		}

		private static long ToInt64(object Value, string Name)
		{
			Assert.IsNotNull(Value, "Missing " + Name + ".");

			if (Value is string s)
			{
				Assert.IsTrue(long.TryParse(s, out long Parsed),
					"Expected numeric " + Name + ".");
				return Parsed;
			}

			string Error;

			try
			{
				return Convert.ToInt64(Value);
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			Assert.Fail("Expected numeric " + Name + ": " + Error);
			return 0;
		}

		private static IDictionary<string, object> DecodeJwtPart(string Token, int Part)
		{
			Assert.IsFalse(string.IsNullOrEmpty(Token), "Missing JWT.");
			string[] Parts = Token.Split('.');
			Assert.HasCount(3, Parts, "Expected JWT to contain three parts.");

			string Encoded = Parts[Part].Replace('-', '+').Replace('_', '/');
			switch (Encoded.Length % 4)
			{
				case 2:
					Encoded += "==";
					break;

				case 3:
					Encoded += "=";
					break;
			}

			byte[] Decoded = Convert.FromBase64String(Encoded);
			object Parsed = JSON.Parse(Encoding.UTF8.GetString(Decoded));
			IDictionary<string, object> Result = Parsed as IDictionary<string, object>;
			Assert.IsNotNull(Result);
			return Result;
		}

		private static Task<string> Login(LoginMethod Method)
		{
			return Login(Method, TestUserName, TestPassword);
		}

		private static async Task<string> Login(LoginMethod Method,
			string UserName, string Password)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method, UserName, Password);
			
			Assert.AreEqual(BaseUrl, AuthorizationCode.Issuer);

			if (AuthorizationCode.HasToken)
				return AuthorizationCode.Token;

			Assert.IsTrue(AuthorizationCode.HasCode);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, UserName));

			return AssertAccessTokenResponse(TokenResponse);
		}

		private static string AssertAccessTokenResponse(ContentResponse TokenResponse)
		{
			if (TokenResponse.HasError)
				throw new LoginError(TokenResponse.Error.Message);

			object Parsed = JSON.Parse(Encoding.UTF8.GetString(TokenResponse.Encoded));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));

			string AccessToken = Required<string>(Parsed, "access_token");
			Assert.IsFalse(string.IsNullOrEmpty(AccessToken));

			int ExpiresIn = Required<int>(Parsed, "expires_in");
			Assert.IsGreaterThan(0, ExpiresIn, "Expected positive expires_in.");

			return AccessToken;
		}

		private class AuthorizationResult
		{
			public bool HasCode => !string.IsNullOrEmpty(this.Code);
			public bool HasToken => !string.IsNullOrEmpty(this.Token);
			public string Code;
			public string CodeVerifier;
			public string RedirectUri;
			public string Token;
			public string RefreshToken;
			public string Issuer;
			public int ExpiresIn;
		}

		private static Task<AuthorizationResult> Authorize(LoginMethod Method)
		{
			return Authorize(Method, TestUserName, TestPassword);
		}

		private static async Task<AuthorizationResult> Authorize(LoginMethod Method,
			string UserName, string Password)
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string AuthorizeUri = BaseUrl + OAuthAuthorizeResource.DefaultResourcePath;
			string CodeVerifier = null;
			string CodeChallenge;
			bool ExpectCode = false;
			bool ExpectToken = false;
			bool ExpectState = true;
			ContentResponse AuthorizeResponse;
			ContentResponse LoginResponse;
			Dictionary<string, string> FormPostback = new()
			{
				{ "redirect_uri", RedirectUri },
				{ "state", State }
			};

			if (!string.IsNullOrEmpty(UserName))
				FormPostback["client_id"] = UserName;

			if (!string.IsNullOrEmpty(Password))
				FormPostback["client_secret"] = Password;

			switch (Method)
			{
				case LoginMethod.CodeForm:
					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)));
					ExpectCode = true;
					break;

				case LoginMethod.CodePost:
					Dictionary<string, string> Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkceDefault:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge)));

					FormPostback["code_challenge"] = CodeChallenge;
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkcePlain:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge) +
						"&code_challenge_method=" + Uri.EscapeDataString("plain")));

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "plain";
					ExpectCode = true;
					break;

				case LoginMethod.CodeFormWithPkceS256:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "S256");

					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=code" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
						"&code_challenge=" + Uri.EscapeDataString(CodeChallenge) +
						"&code_challenge_method=" + Uri.EscapeDataString("S256")));

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "S256";
					ExpectCode = true;
					break;

				case LoginMethod.CodePostWithPkcePlain:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "plain");
					Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri },
						{ "code_challenge", CodeChallenge },
						{ "code_challenge_method", "plain" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "plain";
					ExpectCode = true;
					break;

				case LoginMethod.CodePostWithPkceS256:
					CodeVerifier = CreatePkceCodeVerifier();
					CodeChallenge = CreateCodeChallenge(CodeVerifier, "S256");

					Request = new()
					{
						{ "response_type", "code" },
						{ "state", State },
						{ "redirect_uri", RedirectUri },
						{ "code_challenge", CodeChallenge },
						{ "code_challenge_method", "S256" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request);

					FormPostback["code_challenge"] = CodeChallenge;
					FormPostback["code_challenge_method"] = "S256";
					ExpectCode = true;
					break;

				case LoginMethod.ImplicitGet:
					AuthorizeResponse = await InternetContent.GetAsync(new Uri(AuthorizeUri +
						"?response_type=token" +
						(string.IsNullOrEmpty(UserName) ? "" : "&client_id=" + Uri.EscapeDataString(UserName)) +
						"&state=" + Uri.EscapeDataString(State) +
						"&redirect_uri=" + Uri.EscapeDataString(RedirectUri)),
						new KeyValuePair<string, string>("X-Context", UserName));

					if (AuthorizeResponse.HasError)
						throw new LoginError(AuthorizeResponse.Error.Message);

					FormPostback = null;
					ExpectToken = true;
					break;

				case LoginMethod.ImplicitPost:
					Request = new()
					{
						{ "response_type", "token" },
						{ "state", State },
						{ "redirect_uri", RedirectUri }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(AuthorizeUri), Request,
						new KeyValuePair<string, string>("X-Context", UserName));

					if (AuthorizeResponse.HasError)
						throw new LoginError(AuthorizeResponse.Error.Message);

					FormPostback = null;
					ExpectToken = true;
					break;

				case LoginMethod.Password:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "password" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["username"] = UserName;

					if (!string.IsNullOrEmpty(Password))
						Request["password"] = Password;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
						Request);

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				case LoginMethod.ClientCredentials:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "client_credentials" }
					};

					if (!string.IsNullOrEmpty(UserName))
						Request["client_id"] = UserName;

					if (!string.IsNullOrEmpty(Password))
						Request["client_secret"] = Password;

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
						Request);

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				case LoginMethod.ClientCredentialsBasicAuth:
					Request = new Dictionary<string, string>()
					{
						{ "grant_type", "client_credentials" }
					};

					AuthorizeResponse = await InternetContent.PostAsync(
						new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath), Request,
						new KeyValuePair<string, string>("Authorization", "Basic " + 
							Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password))));

					FormPostback = null;
					ExpectToken = true;
					ExpectState = false;
					break;

				default:
					throw new Exception("Unknown code method: " + Method);
			}

			if (AuthorizeResponse.HasError)
				throw new LoginError(AuthorizeResponse.Error.Message);

			Dictionary<string, object> Response;

			if (AuthorizeResponse.Decoded is HtmlDocument HtmlDocument)
				Response = await CompleteAuthorizationForm(HtmlDocument, FormPostback);
			else
			{
				Assert.IsNull(FormPostback);
				LoginResponse = AuthorizeResponse;
				Response = LoginResponse.Decoded as Dictionary<string, object>;
				Assert.IsNotNull(Response);
			}

			if (ExpectState)
			{
				Assert.IsTrue(Response.TryGetValue("state", out object ReturnedState), "Missing state in response.");
				Assert.IsTrue(ReturnedState is string, "Returned state not a string.");
				Assert.AreEqual(State, ReturnedState);
			}

			AuthorizationResult Result = new()
			{
				CodeVerifier = CodeVerifier,
				RedirectUri = RedirectUri
			};

			if (Response.TryGetValue("iss", out object Issuer))
				Result.Issuer = Issuer as string;

			if (ExpectCode)
			{
				Assert.IsTrue(Response.TryGetValue("code", out object Code), "Response did not contain code.");
				Result.Code = Code as string;
				Assert.IsFalse(string.IsNullOrEmpty(Result.Code));
			}

			if (ExpectToken)
			{
				Assert.IsTrue(Response.TryGetValue("access_token", out object Token), "Response did not contain token.");
				Result.Token = Token as string;
				Assert.IsFalse(string.IsNullOrEmpty(Result.Token));

				Assert.IsTrue(Response.TryGetValue("token_type", out object TokenType), "Response did not contain token_type.");
				Assert.AreEqual("Bearer", TokenType as string);

				if (Response.TryGetValue("refresh_token", out object RefreshToken))
					Result.RefreshToken = RefreshToken as string;

				if (Response.TryGetValue("expires_in", out object ExpiresIn))
				{
					if (ExpiresIn is int i ||
						ExpiresIn is string s && int.TryParse(s, out i))
					{
						Assert.IsGreaterThan(0, i);
						Result.ExpiresIn = i;
					}
					else
						Assert.Fail("Invalid expires_in");
				}
			}

			return Result;
		}

		private static async Task<Dictionary<string, object>> CompleteAuthorizationForm(
			HtmlDocument HtmlDocument, Dictionary<string, string> FormPostback)
		{
			Assert.IsNotNull(FormPostback);

			foreach (Form Form in HtmlDocument.Form)
			{
				foreach (HtmlNode N in Form.Children)
				{
					if (N is not Input Input)
						continue;

					if (Input["type"] != "hidden")
						continue;

					if (!FormPostback.ContainsKey(Input["name"]))
						FormPostback[Input["name"]] = Input["value"];
				}
			}

			ContentResponse LoginResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthAuthorizeResource.DefaultResourcePath), FormPostback);

			if (LoginResponse.HasError)
				throw new LoginError(LoginResponse.Error.Message);

			if (LoginResponse.Decoded is HtmlDocument HtmlDocumentResponse &&
				TryGetErrorMessage(HtmlDocumentResponse, out string ErrorMessage))
			{
				throw new LoginError(ErrorMessage);
			}

			Dictionary<string, object> Response = LoginResponse.Decoded as Dictionary<string, object>;
			Assert.IsNotNull(Response);

			return Response;
		}

		private static bool TryGetErrorMessage(HtmlDocument HtmlDocument, out string ErrorMessage)
		{
			int i = HtmlDocument.HtmlText.IndexOf("id='errorMessage'>");
			if (i > 0)
			{
				i += 18;
				int j = HtmlDocument.HtmlText.IndexOf("</", i);
				if (j > i)
				{
					ErrorMessage = XML.DecodeString(HtmlDocument.HtmlText[i..j].Trim());
					return true;
				}
			}

			ErrorMessage = null;
			return false;
		}

		private class LoginError(string Message) : Exception(Message)
		{
		}

		private static string CreatePkceCodeVerifier()
		{
			byte[] RandomBytes = new byte[32];
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			Rnd.GetBytes(RandomBytes);
			return Base64Url.Encode(RandomBytes);
		}

		private static string CreateCodeChallenge(string CodeVerifier, string Method)
		{
			return Method switch
			{
				"plain" => CodeVerifier,
				"S256" => Base64Url.Encode(Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier))),
				_ => throw new Exception("Unknown code challenge method: " + Method),
			};
		}

		private static T Required<T>(object Dictionary, string Key)
		{
			object Result;

			if (Dictionary is IDictionary<string, object> Typed &&
				Typed.TryGetValue(Key, out object GenericValue))
			{
				Result = GenericValue;
			}
			else if (Dictionary is IDictionary Untyped && Untyped.Contains(Key))
				Result = Untyped[Key];
			else
				throw new Exception("Expected JSON object to contain key: " + Key);

			if (Result is null)
				throw new Exception("Property value is null: " + Key);

			if (Result is not T TypedResult)
			{
				throw new Exception("Property value not of expected type: " + Key +
					" (" + Result.GetType().FullName + ")");
			}

			return TypedResult;
		}
	}
}
