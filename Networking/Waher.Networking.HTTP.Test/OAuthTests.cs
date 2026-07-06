using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Json;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
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
	public class OAuthTests : IDynamicUserSource
	{
		private const string BaseUrl = "http://localhost:8081";
		private const string CallbackResource = "/Callback";
		private const string ProtectedResource = "/Hello";
		private const string Realm = "Test";
		private const string TestUserName = "User";
		private const string TestPassword = "Password";

		private Dictionary<string, User> users;
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
			OAuthRegistrationResource RegistrationResource;
			OAuthDeviceAuthorizationResource DeviceAuthorizationResource;
			OAuthAuthorizeResource AuthorizeResource;

			this.server.Register(new ProtectedResourceMetaData());
			this.server.Register(TokenResource = new OAuthTokenResource(this, this.jwtFactory));
			this.server.Register(RegistrationResource = new OAuthRegistrationResource(this, this.jwtFactory));
			this.server.Register(DeviceAuthorizationResource = new OAuthDeviceAuthorizationResource(this.jwtFactory));
			this.server.Register(AuthorizeResource = new OAuthAuthorizeResource(TokenResource,
				RegistrationResource, DeviceAuthorizationResource, this.jwtFactory));
			this.server.Register(new AuthorizationServerMetaData(AuthorizeResource));

			this.server.Register(CallbackResource, Callback);
			this.server.Register(new Hello(this.jwtFactory, this));

			this.users = new Dictionary<string, User>()
			{
				{ TestUserName, new User(TestUserName, TestPassword) }
			};

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
			if (this.users.TryGetValue(UserName, out User User))
				return Task.FromResult<IUser>(User);
			else
				return Task.FromResult<IUser>(null);
		}

		public Task<IRegistration> RegisterUser(IRegistrationRequest RegistrationRequest)
		{
			string UserName;
			string Password;

			do
			{
				UserName = Guid.NewGuid().ToString();
			}
			while (this.users.ContainsKey(UserName));

			Password = Guid.NewGuid().ToString();

			User User = new(UserName, Password);
			this.users[User.UserName] = User;

			return Task.FromResult<IRegistration>(
				new Registration(UserName, Password, RegistrationRequest));
		}

		private class Registration(string UserName, string Password,
			IRegistrationRequest Request) : IRegistration
		{
			public string ClientId { get; } = UserName;
			public string ClientSecret { get; } = Password;
			public DateTime? ClientSecretExpiresAt => null;
			public IRegistrationRequest Request { get; } = Request;
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
			Assert.AreEqual(BaseUrl + OAuthRegistrationResource.DefaultResourcePath, Required<string>(ServerMetaData, "registration_endpoint"));
			Assert.AreEqual(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath, Required<string>(ServerMetaData, "device_authorization_endpoint"));
			Assert.Contains("code", Required<object[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("token", Required<object[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("authorization_code", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("password", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("client_credentials", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("refresh_token", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains(OAuthDeviceAuthorizationResource.GrantType, Required<object[]>(ServerMetaData, "grant_types_supported"));
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
			TokenResult Token = await Login(Method);
			await AssertHello(Token.AccessToken);
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
			OAuthError Error = AssertOAuthError(Response, false, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		private static void AssertBearerChallenge(OAuthError Error)
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

		private static void AssertBearerChallenge(OAuthError Error, string ErrorCode)
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

			OAuthError Error = AssertOAuthError(Response, false, UnauthorizedException.Code);
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

			OAuthError Error = AssertOAuthError(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Description);
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

			OAuthError Error = AssertOAuthError(Response, BadRequestException.Code);
			Assert.Contains("Missing code_verifier", Error.Description);
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

			OAuthError Error = AssertOAuthError(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Description);
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
			OAuthError Error = AssertOAuthError(SecondResponse, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Description);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			TokenResult Token = AssertAccessTokenResponse(TokenResponse);
			await AssertHello(Token.AccessToken);
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

			AssertOAuthError(Response, ForbiddenException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, ForbiddenException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			AssertOAuthError(Response, BadRequestException.Code);
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

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "access_token")));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
		}

		private static void AssertNoStoreHeaders(HttpResponseMessage Response)
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
				AssertOAuthError(Response, BadRequestException.Code);
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
			using HttpClient Client = new();
			using StringContent Content = new(
				"grant_type=password" +
				"&grant_type=client_credentials" +
				"&username=" + Uri.EscapeDataString(TestUserName) +
				"&password=" + Uri.EscapeDataString(TestPassword),
				Encoding.UTF8, "application/x-www-form-urlencoded");

			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		[TestMethod]
		public async Task Test_34_TokenEndpointRejectsMultipleClientAuthenticationMethods()
		{
			using HttpClient Client = new();
			Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(TestUserName + ":" + TestPassword)));

			using FormUrlEncodedContent Content = new(new Dictionary<string, string>()
			{
				{ "grant_type", "client_credentials" },
				{ "client_id", TestUserName },
				{ "client_secret", TestPassword }
			});

			using HttpResponseMessage Response = await Client.PostAsync(
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

			OAuthError Error = AssertOAuthError(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Description);
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

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_37_InvalidBearerTokenChallengeIncludesInvalidTokenError()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer this-is-not-a-jwt"));

			OAuthError Error = AssertOAuthError(Response, false, UnauthorizedException.Code);
			AssertBearerChallenge(Error, "invalid_token");
		}

		[TestMethod]
		public async Task Test_38_BearerTokenInQueryStringIsRejected()
		{
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256);

			ContentResponse Response = await InternetContent.GetAsync(new Uri(
				BaseUrl + ProtectedResource + "?access_token=" + Uri.EscapeDataString(Token.AccessToken)));

			OAuthError Error = AssertOAuthError(Response, false, UnauthorizedException.Code);
			AssertBearerChallenge(Error);
		}

		[TestMethod]
		public async Task Test_39_AuthorizationEndpointHasClickjackingProtection()
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;

			using HttpClient Client = new();
			using HttpResponseMessage Response = await Client.GetAsync(
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
			TokenResult Token = await Login(LoginMethod.CodeFormWithPkceS256);

			IDictionary<string, object> Header = DecodeJwtPart(Token.AccessToken, 0);
			Assert.IsTrue(Header.TryGetValue("alg", out object Algorithm),
				"Expected JWT header to contain alg.");
			Assert.IsFalse(string.Equals("none", Algorithm as string, StringComparison.OrdinalIgnoreCase),
				"Expected signed JWT access token.");

			IDictionary<string, object> Payload = DecodeJwtPart(Token.AccessToken, 1);
			Assert.IsTrue(Payload.ContainsKey("sub"), "Expected JWT access token to contain sub.");
			AssertPositiveUnixTime(Required<object>(Payload, "exp"), "exp");
		}

		[TestMethod]
		[DataRow(LoginMethod.CodeFormWithPkceS256)]
		[DataRow(LoginMethod.CodePostWithPkceS256)]
		public async Task Test_41_AuthorizationCodeIssuesRefreshToken(LoginMethod Method)
		{
			TokenResult Token = await Login(Method);

			Assert.IsFalse(string.IsNullOrEmpty(Token.RefreshToken),
				"Expected authorization code grant to issue a refresh token.");
		}

		[TestMethod]
		public async Task Test_42_RefreshTokenGrantReturnsAccessToken()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			TokenResult RefreshedToken = await RefreshAccessToken(InitialToken.RefreshToken, TestUserName);
			Assert.IsFalse(string.IsNullOrEmpty(RefreshedToken.AccessToken));
			await AssertHello(RefreshedToken.AccessToken);
		}

		private static async Task<TokenResult> RefreshAccessToken(string RefreshToken, string ClientId)
		{
			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(RefreshToken, ClientId));

			return AssertAccessTokenResponse(TokenResponse);
		}

		private static Dictionary<string, string> CreateRefreshTokenRequest(string RefreshToken,
			string ClientId)
		{
			Dictionary<string, string> Request = new()
			{
				{ "grant_type", "refresh_token" }
			};

			if (!string.IsNullOrEmpty(RefreshToken))
				Request["refresh_token"] = RefreshToken;

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			return Request;
		}

		[TestMethod]
		public async Task Test_43_RefreshTokenGrantRequiresRefreshToken()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "refresh_token" },
					{ "client_id", TestUserName }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_44_InvalidRefreshTokenIsRejected()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest("invalid-refresh-token", TestUserName));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_45_RefreshTokenIsBoundToClientId()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(InitialToken.RefreshToken, "Invalid User"));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_46_RefreshTokenRotationInvalidatesPreviousToken()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256);
			Assert.IsFalse(string.IsNullOrEmpty(InitialToken.RefreshToken));

			TokenResult RefreshedToken = await RefreshAccessToken(InitialToken.RefreshToken, TestUserName);
			Assert.IsFalse(string.IsNullOrEmpty(RefreshedToken.RefreshToken),
				"Expected refresh token rotation to issue a replacement refresh token.");
			Assert.AreNotEqual(InitialToken.RefreshToken, RefreshedToken.RefreshToken,
				"Expected refresh token rotation to replace the previous refresh token.");

			ContentResponse ReuseResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateRefreshTokenRequest(InitialToken.RefreshToken, TestUserName));

			AssertOAuthError(ReuseResponse, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		[DataRow(LoginMethod.ImplicitGet)]
		[DataRow(LoginMethod.ImplicitPost)]
		[DataRow(LoginMethod.ClientCredentials)]
		[DataRow(LoginMethod.ClientCredentialsBasicAuth)]
		public async Task Test_47_GrantsThatShouldNotIssueRefreshTokens(LoginMethod Method)
		{
			AuthorizationResult Result = await Authorize(Method);

			Assert.IsTrue(Result.HasToken);
			Assert.IsTrue(string.IsNullOrEmpty(Result.RefreshToken));
		}

		[TestMethod]
		public async Task Test_48_DynamicClientRegistrationCreatesPublicClient()
		{
			Dictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Public Client" },
					{ "grant_types", new string[] { "authorization_code", "refresh_token" } },
					{ "response_types", new string[] { "code" } },
					{ "token_endpoint_auth_method", "none" }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
			Assert.Contains(BaseUrl + CallbackResource, Required<object[]>(Response, "redirect_uris"));
			Assert.Contains("authorization_code", Required<object[]>(Response, "grant_types"));
			Assert.Contains("refresh_token", Required<object[]>(Response, "grant_types"));
			Assert.Contains("code", Required<object[]>(Response, "response_types"));
			Assert.AreEqual("none", Required<string>(Response, "token_endpoint_auth_method"));
			Assert.IsFalse(Response.ContainsKey("client_secret"),
				"A public client using token_endpoint_auth_method=none must not receive a client_secret.");
		}

		private static async Task<Dictionary<string, object>> DoPost(string Uri,
			object Body, System.Net.HttpStatusCode ExpectedStatusCode)
		{
			ContentResponse Encoded = await InternetContent.EncodeAsync(Body,
				Encoding.UTF8, JsonCodec.DefaultContentType);

			Encoded.AssertOk();

			using HttpClient Client = new();
			using ByteArrayContent Content = new(Encoded.Encoded);
			using HttpRequestMessage Request = new()
			{
				Method = HttpMethod.Post,
				Content = Content,
				RequestUri = new Uri(Uri)
			};

			Request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Encoded.ContentType);

			using HttpResponseMessage Response = await Client.SendAsync(Request);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.AreEqual(ExpectedStatusCode, Response.StatusCode, ResponseText);

			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			return Parsed;
		}

		[TestMethod]
		public async Task Test_49_DynamicClientRegistrationCreatesConfidentialClient()
		{
			IDictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Confidential Client" },
					{ "grant_types", new string[] { "authorization_code", "refresh_token" } },
					{ "response_types", new string[] { "code" } },
					{ "token_endpoint_auth_method", "client_secret_basic" }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_secret")));

			int ExpiresAt = Required<int>(Response, "client_secret_expires_at");

			if (ExpiresAt != 0)
			{
				int Now = (int)DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds;
				Assert.IsGreaterThan(Now, ExpiresAt);
			}

			Assert.AreEqual("client_secret_basic", Required<string>(Response, "token_endpoint_auth_method"));
		}

		[TestMethod]
		public async Task Test_50_DynamicClientRegistrationRejectsInvalidRedirectUri()
		{
			Dictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource + "#fragment" } },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.BadRequest);

			Assert.IsTrue(Error.ContainsKey("error"));
		}

		[TestMethod]
		public async Task Test_51_DynamicClientRegistrationRejectsInconsistentGrantAndResponseTypes()
		{
			IDictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "grant_types", new string[] { "implicit" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("invalid_client_metadata", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_52_DynamicClientRegistrationRequiresJsonRequest()
		{
			await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, string>()
				{
					{ "redirect_uris", BaseUrl + CallbackResource },
				},
				System.Net.HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public async Task Test_53_DynamicClientRegistrationAcceptsUnknownMetadata()
		{
			IDictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Client With Unknown Metadata" },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } },
					{ "unknown_metadata", "ignored" }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
			Assert.Contains(BaseUrl + CallbackResource, Required<object[]>(Response, "redirect_uris"));
		}

		[TestMethod]
		public async Task Test_54_DeviceAuthorizationResponseContainsRequiredValues()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			Assert.IsFalse(string.IsNullOrEmpty(Device.DeviceCode));
			Assert.IsFalse(string.IsNullOrEmpty(Device.UserCode));
			Assert.IsFalse(string.IsNullOrEmpty(Device.VerificationUri));
			Assert.IsGreaterThan(0, Device.ExpiresIn);
			Assert.IsGreaterThan(0, Device.Interval);

			if (!string.IsNullOrEmpty(Device.VerificationUriComplete))
			{
				Assert.IsTrue(Device.VerificationUriComplete.Contains(
					Uri.EscapeDataString(Device.UserCode), StringComparison.OrdinalIgnoreCase) ||
					Device.VerificationUriComplete.Contains(Device.UserCode, StringComparison.OrdinalIgnoreCase),
					"Expected verification_uri_complete to contain the user code or its escaped representation.");
			}
		}

		private static async Task<DeviceAuthorizationResult> StartDeviceAuthorization(string ClientId)
		{
			Dictionary<string, string> Request = [];

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath,
				Content, CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			DeviceAuthorizationResult Result = new()
			{
				DeviceCode = Required<string>(Parsed, "device_code"),
				UserCode = Required<string>(Parsed, "user_code"),
				VerificationUri = Required<string>(Parsed, "verification_uri"),
				ExpiresIn = Required<int>(Parsed, "expires_in"),
				Interval = 5
			};

			if (Parsed.TryGetValue("verification_uri_complete", out object Obj))
			{
				Assert.IsTrue(Obj is string);
				Result.VerificationUriComplete = (string)Obj;
			}

			if (Parsed.TryGetValue("interval", out Obj))
			{
				Assert.IsTrue(Obj is int);
				Result.Interval = (int)Obj;
			}

			return Result;
		}

		[TestMethod]
		public async Task Test_55_DeviceTokenPollingBeforeAuthorizationReturnsAuthorizationPending()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			Dictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath,
				CreateDeviceTokenRequest(Device.DeviceCode, TestUserName),
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("authorization_pending", Required<string>(Error, "error"));
		}

		private static Dictionary<string, object> CreateDeviceTokenRequest(string DeviceCode,
			string ClientId)
		{
			Dictionary<string, object> Request = new()
			{
				{ "grant_type", OAuthDeviceAuthorizationResource.GrantType }
			};

			if (!string.IsNullOrEmpty(DeviceCode))
				Request["device_code"] = DeviceCode;

			if (!string.IsNullOrEmpty(ClientId))
				Request["client_id"] = ClientId;

			return Request;
		}

		[TestMethod]
		public async Task Test_56_DeviceTokenGrantRequiresDeviceCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", OAuthDeviceAuthorizationResource.GrantType },
					{ "client_id", TestUserName }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_57_DeviceTokenGrantRejectsInvalidDeviceCode()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest("invalid-device-code", TestUserName));

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_58_DeviceTokenGrantRequiresClientIdForPublicClient()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", OAuthDeviceAuthorizationResource.GrantType },
					{ "device_code", Device.DeviceCode }
				});

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_59_DeviceAuthorizationRejectsMissingClientId()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>());

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_60_DeviceFlowCompletesAfterUserVerification()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);
			await CompleteDeviceVerification(Device, TestUserName, TestPassword);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateDeviceTokenRequest(Device.DeviceCode, TestUserName));

			TokenResult TokenResult = AssertAccessTokenResponse(TokenResponse);
			await AssertHello(TokenResult.AccessToken);
		}

		private static async Task CompleteDeviceVerification(DeviceAuthorizationResult Device,
			string UserName, string Password)
		{
			ContentResponse Response = await VerifyDeviceUserCode(Device, UserName, Password);

			if (Response.HasError)
				throw new LoginError(Response.Error.Message);

			if (Response.Decoded is HtmlDocument HtmlDocument &&
				TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
			{
				throw new LoginError(ErrorMessage);
			}
		}

		private static async Task<ContentResponse> VerifyDeviceUserCode(DeviceAuthorizationResult Device,
			string UserName, string Password)
		{
			string VerificationUri = string.IsNullOrEmpty(Device.VerificationUriComplete) ?
				Device.VerificationUri : Device.VerificationUriComplete;

			Dictionary<string, string> FormPostback = new()
			{
				{ "user_code", Device.UserCode }
			};

			if (!string.IsNullOrEmpty(UserName))
				FormPostback["client_id"] = UserName;

			if (!string.IsNullOrEmpty(Password))
				FormPostback["client_secret"] = Password;

			ContentResponse VerificationResponse = await InternetContent.GetAsync(new Uri(VerificationUri));

			if (VerificationResponse.HasError)
				return VerificationResponse;

			if (VerificationResponse.Decoded is HtmlDocument HtmlDocument)
			{
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
			}

			return await InternetContent.PostAsync(new Uri(Device.VerificationUri), FormPostback);
		}

		[TestMethod]
		public async Task Test_61_DeviceTokenPollingTooFastReturnsSlowDown()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath,
				CreateDeviceTokenRequest(Device.DeviceCode, TestUserName),
				System.Net.HttpStatusCode.BadRequest);

			IDictionary<string, object> Error = await DoPost(
				BaseUrl + OAuthTokenResource.DefaultResourcePath,
				CreateDeviceTokenRequest(Device.DeviceCode, TestUserName),
				System.Net.HttpStatusCode.BadRequest);

			Assert.AreEqual("slow_down", Required<string>(Error, "error"));
		}

		[TestMethod]
		public async Task Test_62_DeviceUserCodeIsSingleUse()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);
			await CompleteDeviceVerification(Device, TestUserName, TestPassword);

			ContentResponse SecondVerificationResponse = await VerifyDeviceUserCode(
				Device, TestUserName, TestPassword);

			if (SecondVerificationResponse.HasError)
				return;

			if (SecondVerificationResponse.Decoded is HtmlDocument HtmlDocument &&
				TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
			{
				Assert.IsFalse(string.IsNullOrEmpty(ErrorMessage));
				return;
			}

			Assert.Fail("Expected second use of the device user code to be rejected or to return an error form.");
		}

		[TestMethod]
		public async Task Test_63_RefreshTokenGrantSuccessfulResponseIsNotCacheable()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256);
			Dictionary<string, string> Request = CreateRefreshTokenRequest(
				InitialToken.RefreshToken, TestUserName);

			using HttpClient Client = new();
			using FormUrlEncodedContent Content = new(Request);
			using HttpResponseMessage Response = await Client.PostAsync(
				BaseUrl + OAuthTokenResource.DefaultResourcePath, Content,
				CancellationToken.None);

			string ResponseText = await Response.Content.ReadAsStringAsync(CancellationToken.None);
			Assert.IsTrue(Response.IsSuccessStatusCode, ResponseText);
			AssertNoStoreHeaders(Response);

			Dictionary<string, object> Parsed = JSON.Parse(ResponseText) as Dictionary<string, object>;
			Assert.IsNotNull(Parsed);

			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "access_token")));
		}

		[TestMethod]
		public async Task Test_64_DynamicClientRegistrationSuccessfulResponseIsNotCacheable()
		{
			Dictionary<string, object> Response = await DoPost(
				BaseUrl + OAuthRegistrationResource.DefaultResourcePath,
				new Dictionary<string, object>()
				{
					{ "redirect_uris", new string[] { BaseUrl + CallbackResource } },
					{ "client_name", "Unit Test Cache Client" },
					{ "grant_types", new string[] { "authorization_code" } },
					{ "response_types", new string[] { "code" } }
				},
				System.Net.HttpStatusCode.Created);

			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Response, "client_id")));
		}

		[TestMethod]
		public async Task Test_65_DeviceVerificationRequiresUserCode()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(Device.VerificationUri),
				new Dictionary<string, string>()
				{
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertVerificationError(Response);
		}

		[TestMethod]
		public async Task Test_66_DeviceVerificationRejectsInvalidUserCode()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(Device.VerificationUri),
				new Dictionary<string, string>()
				{
					{ "user_code", "invalid-user-code" },
					{ "client_id", TestUserName },
					{ "client_secret", TestPassword }
				});

			AssertVerificationError(Response);
		}

		[TestMethod]
		public async Task Test_67_DeviceVerificationRejectsInvalidLogin()
		{
			DeviceAuthorizationResult Device = await StartDeviceAuthorization(TestUserName);

			ContentResponse Response = await VerifyDeviceUserCode(
				Device, TestUserName, "Invalid Password");

			AssertVerificationError(Response);
		}

		[TestMethod]
		public async Task Test_68_RefreshTokenGrantRejectsScopeEscalation()
		{
			TokenResult InitialToken = await Login(LoginMethod.CodeFormWithPkceS256);
			Dictionary<string, string> Request = CreateRefreshTokenRequest(
				InitialToken.RefreshToken, TestUserName);
			Request["scope"] = "scope-not-originally-granted";

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				Request);

			AssertOAuthError(Response, BadRequestException.Code, ForbiddenException.Code);
		}

		[TestMethod]
		public async Task Test_69_DeviceAuthorizationIgnoresUnknownParameter()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "client_id", TestUserName },
					{ "unknown_parameter", "ignored" }
				});

			Response.AssertOk();
			object Parsed = Response.Decoded;
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "device_code")));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parsed, "user_code")));
		}

		[TestMethod]
		public async Task Test_70_DeviceAuthorizationEmptyClientIdIsTreatedAsMissing()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "client_id", string.Empty }
				});

			AssertOAuthError(Response, BadRequestException.Code);
		}

		[TestMethod]
		public async Task Test_71_DuplicateDeviceAuthorizationParameterIsRejected()
		{
			using HttpClient Client = new();
			using StringContent Content = new(
				"client_id=" + Uri.EscapeDataString(TestUserName) +
				"&client_id=" + Uri.EscapeDataString("OtherUser"),
				Encoding.UTF8, "application/x-www-form-urlencoded");

			using HttpResponseMessage Response = await Client.PostAsync(
				new Uri(BaseUrl + OAuthDeviceAuthorizationResource.DefaultResourcePath),
				Content, CancellationToken.None);

			Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, Response.StatusCode);
		}

		private static OAuthError AssertOAuthError(ContentResponse Response,
			params int[] ExpectedStatusCodes)
		{
			return AssertOAuthError(Response, true, ExpectedStatusCodes);
		}

		private static OAuthError AssertOAuthError(ContentResponse Response,
			bool CheckBody, params int[] ExpectedStatusCodes)
		{
			Assert.IsTrue(Response.HasError);
			WebException Result = Response.Error as WebException;
			Assert.IsNotNull(Result);

			bool IsExpectedCode = false;

			foreach (int ExpectedStatusCode in ExpectedStatusCodes)
			{
				if ((int)Result.StatusCode == ExpectedStatusCode)
				{
					IsExpectedCode = true;
					break;
				}
			}

			Assert.IsTrue(IsExpectedCode, "Unexpected status code: " + (int)Result.StatusCode);

			OAuthError Error = new()
			{
				Headers = Result.Headers
			};

			if (Result.Content is Dictionary<string, object> ErrorObj)
			{
				if (!ErrorObj.TryGetValue("error", out object Obj))
					throw new Exception("Error code missing in response.");

				if (Obj is not string ErrorCode)
					throw new Exception("Error code is not a string.");
				else
					Error.Code = ErrorCode;

				if (!ErrorObj.TryGetValue("error_description", out Obj))
					throw new Exception("Error description missing in response.");

				if (Obj is not string ErrorDescription)
					throw new Exception("Error description is not a string.");
				else
					Error.Description = ErrorDescription;
			}
			else if (CheckBody)
			{
				if (Result.Content is not null)
					throw new Exception("Invalid error response.");
				else
					throw new Exception("Missing error object body.");
			}

			return Error;
		}

		private class OAuthError
		{
			public string Code;
			public string Description;
			public HttpHeaders Headers;
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

		private static Task<TokenResult> Login(LoginMethod Method)
		{
			return Login(Method, TestUserName, TestPassword);
		}

		private static async Task<TokenResult> Login(LoginMethod Method,
			string UserName, string Password)
		{
			AuthorizationResult AuthorizationCode = await Authorize(Method, UserName, Password);

			Assert.AreEqual(BaseUrl, AuthorizationCode.Issuer);

			if (AuthorizationCode.HasToken)
			{
				return new TokenResult()
				{
					AccessToken = AuthorizationCode.Token,
					RefreshToken = AuthorizationCode.RefreshToken,
					ExpiresIn = AuthorizationCode.ExpiresIn
				};
			}

			Assert.IsTrue(AuthorizationCode.HasCode);

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				CreateAuthorizationCodeTokenRequest(AuthorizationCode, UserName));

			return AssertAccessTokenResponse(TokenResponse);
		}

		private static TokenResult AssertAccessTokenResponse(ContentResponse TokenResponse)
		{
			if (TokenResponse.HasError)
				throw new LoginError(TokenResponse.Error.Message);

			object Parsed = JSON.Parse(Encoding.UTF8.GetString(TokenResponse.Encoded));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));

			string AccessToken = Required<string>(Parsed, "access_token");
			Assert.IsFalse(string.IsNullOrEmpty(AccessToken));

			string RefreshToken = Required<string>(Parsed, "refresh_token");
			Assert.IsFalse(string.IsNullOrEmpty(RefreshToken));

			int ExpiresIn = Required<int>(Parsed, "expires_in");
			Assert.IsGreaterThan(0, ExpiresIn, "Expected positive expires_in.");

			return new TokenResult()
			{
				AccessToken = AccessToken,
				RefreshToken = RefreshToken,
				ExpiresIn = ExpiresIn
			};
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

		private class TokenResult
		{
			public string AccessToken;
			public string RefreshToken;
			public int ExpiresIn;
		}

		private class DeviceAuthorizationResult
		{
			public string DeviceCode;
			public string UserCode;
			public string VerificationUri;
			public string VerificationUriComplete;
			public int ExpiresIn;
			public int Interval;
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

		private static void AssertVerificationError(ContentResponse Response)
		{
			if (Response.HasError)
				return;

			if (Response.Decoded is HtmlDocument HtmlDocument &&
				TryGetErrorMessage(HtmlDocument, out string ErrorMessage))
			{
				Assert.IsFalse(string.IsNullOrEmpty(ErrorMessage));
				return;
			}

			if (Response.Decoded is IDictionary<string, object> Values &&
				Values.ContainsKey("error"))
			{
				return;
			}

			Assert.Fail("Expected device verification to return an error.");
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
