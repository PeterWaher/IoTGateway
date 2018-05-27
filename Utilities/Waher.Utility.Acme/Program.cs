using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Security.ACME;

namespace Waher.Utility.Acme
{
	/// <summary>
	/// Helps you create certificates using the Automatic Certificate 
	/// Management Environment (ACME) v2 protocol.
	/// 
	/// Command line switches:
	/// 
	/// -dir URI              URI to the ACME directory resource to use.
	///                       If not provided, the default Let's Encrypt
	///                       ACME v2 directory will be used:
	///                       https://acme-v02.api.letsencrypt.org/directory
	/// -le                   Uses the Let's Encrypt ACME v2 directory:
	///                       https://acme-v02.api.letsencrypt.org/directory
	/// -let                  Uses the Let's Encrypt ACME v2 staging directory:
	///                       https://acme-staging-v02.api.letsencrypt.org/directory
	/// -ce EMAIL             Adds EMAIL to the list of contact e-mail addresses
	///                       when creating an account. Can be used multiple
	///                       times. The first e-mail address will also be
	///                       encoded into the certificate request.
	/// -cu URI               Adds URI to the list of contact URIs when creating
	///                       an account. Can be used multiple times.
	/// -a                    You agree to the terms of service agreement. This
	///                       might be required if you want to be able to create
	///                       an account.
	/// -nk                   Generates a new account key.
	/// -dns DOMAIN           Adds DOMAIN to the list of domain names when creating
	///                       an order for a new certificate. Can be used multiple
	///                       times. The first DOMAIN will be used as the common name
	///                       for the certificate request. The following domain names
	///                       will be used as altenative names.
	/// -nb TIMESTAMP         Generated certificate will not be valid before
	///                       TIMESTAMP.
	/// -na TIMESTAMP         Generated certificate will not be valid after
	///                       TIMESTAMP.
	/// -http ROOTFOLDER      Allows the application to respond to HTTP challenges
	///                       by storing temporary files under the corresponding ACME
	///                       challenge response folder /.well-known/acme-challenge
	/// -pi MS                Polling Interval, in milliseconds. Default value is
	///                       5000.
	/// -ks BITS              Certificate key size, in bits. Default is 4096.
	/// -c COUNTRY            Country name (C) in the certificate request.
	/// -l LOCALITY           Locality name (L) in the certificate request.
	/// -st STATEORPROVINCE   State or Province name (ST) in the certificate request.
	/// -o ORGANIZATION       Organization name (O) in the certificate request.
	/// -ou ORGUNIT           Organizational unit name (OU) in the certificate request.
	/// -f FILENAME           Output filename of the certificate, without file
	///                       extension.
	/// -pwd PASSWORD         Password to protect the private key in the generated
	///                       certificate.
	/// -v                    Verbose mode.
	/// -?                    Help.
	/// </summary>
	class Program
	{
		private static Uri directory = null;
		private static List<string> contactURLs = null;
		private static List<string> domainNames = null;
		private static DateTime? notBefore = null;
		private static DateTime? notAfter = null;
		private static string httpRootFolder = null;
		private static string eMail = null;
		private static string country = null;
		private static string locality = null;
		private static string stateOrProvince = null;
		private static string organization = null;
		private static string organizationalUnit = null;
		private static string fileName = null;
		private static string password = string.Empty;
		private static int? pollingInterval = null;
		private static int? keySize = null;
		private static bool help = false;
		private static bool verbose = false;
		private static bool termsOfServiceAgreed = false;
		private static bool newKey = false;

		static void Main(string[] args)
		{
			ConsoleColor FgColorBak = Console.ForegroundColor;
			ConsoleColor BgColorBak = Console.BackgroundColor;
			int i = 0;
			int c = args.Length;
			string s;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-dir":
							if (i >= c)
								throw new Exception("Missing directory URI.");

							if (directory == null)
								directory = new Uri(args[i++]);
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-le":
							if (directory == null)
								directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-let":
							if (directory == null)
								directory = new Uri("https://acme-staging-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-ce":
							if (i >= c)
								throw new Exception("Missing contact e-mail.");

							if (contactURLs == null)
								contactURLs = new List<string>();

							if (eMail == null)
								eMail = args[i];

							contactURLs.Add("mailto:" + args[i++]);
							break;

						case "-cu":
							if (i >= c)
								throw new Exception("Missing contact URI.");

							if (contactURLs == null)
								contactURLs = new List<string>();

							contactURLs.Add(args[i++]);
							break;

						case "-dns":
							if (i >= c)
								throw new Exception("Missing domain name.");

							if (domainNames == null)
								domainNames = new List<string>();

							domainNames.Add(args[i++]);
							break;

						case "-na":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out DateTime TP))
								notAfter = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-nb":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out TP))
								notBefore = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-http":
							if (i >= c)
								throw new Exception("Missing HTTP root folder.");

							if (httpRootFolder == null)
								httpRootFolder = args[i++];
							else
								throw new Exception("Only one HTTP Root Folder allowed.");
							break;

						case "-pi":
							if (i >= c)
								throw new Exception("Missing polling interval.");

							if (!int.TryParse(args[i++], out int j) || j <= 0)
								throw new Exception("Invalid polling interval.");

							if (pollingInterval.HasValue)
								throw new Exception("Only one polling interval allowed.");
							else
								pollingInterval = j;
							break;

						case "-ks":
							if (i >= c)
								throw new Exception("Missing key size.");

							if (!int.TryParse(args[i++], out j) || j <= 0)
								throw new Exception("Invalid key size.");

							if (keySize.HasValue)
								throw new Exception("Only one key size allowed.");
							else
								keySize = j;
							break;

						case "-c":
							if (i >= c)
								throw new Exception("Missing country name.");

							if (country == null)
								country = args[i++];
							else
								throw new Exception("Only one country name allowed.");
							break;

						case "-l":
							if (i >= c)
								throw new Exception("Missing locality name.");

							if (locality == null)
								locality = args[i++];
							else
								throw new Exception("Only one locality name allowed.");
							break;

						case "-st":
							if (i >= c)
								throw new Exception("Missing state or province name.");

							if (stateOrProvince == null)
								stateOrProvince = args[i++];
							else
								throw new Exception("Only one state or province name allowed.");
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing organization name.");

							if (organization == null)
								organization = args[i++];
							else
								throw new Exception("Only one organization name allowed.");
							break;

						case "-ou":
							if (i >= c)
								throw new Exception("Missing organizational unit name.");

							if (organizationalUnit == null)
								organizationalUnit = args[i++];
							else
								throw new Exception("Only one organizational unit name allowed.");
							break;

						case "-f":
							if (i >= c)
								throw new Exception("Missing file name.");

							if (fileName == null)
								fileName = args[i++];
							else
								throw new Exception("Only one file name allowed.");
							break;

						case "-pwd":
							if (i >= c)
								throw new Exception("Missing password.");

							if (string.IsNullOrEmpty(password))
								password = args[i++];
							else
								throw new Exception("Only one password allowed.");
							break;

						case "-?":
							help = true;
							break;

						case "-v":
							verbose = true;
							break;

						case "-a":
							termsOfServiceAgreed = true;
							break;

						case "-nk":
							newKey = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (help || c == 0)
				{
					Console.Out.WriteLine("Helps you create certificates using the Automatic Certificate");
					Console.Out.WriteLine("Management Environment (ACME) v2 protocol.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-dir URI              URI to the ACME directory resource to use.");
					Console.Out.WriteLine("                      If not provided, the default Let's Encrypt");
					Console.Out.WriteLine("                      ACME v2 directory will be used:");
					Console.Out.WriteLine("                      https://acme-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-le                   Uses the Let's Encrypt ACME v2 directory:");
					Console.Out.WriteLine("                      https://acme-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-let                  Uses the Let's Encrypt ACME v2 staging directory:");
					Console.Out.WriteLine("                      https://acme-staging-v02.api.letsencrypt.org/directory");
					Console.Out.WriteLine("-ce EMAIL             Adds EMAIL to the list of contact e-mail addresses");
					Console.Out.WriteLine("                      when creating an account. Can be used multiple");
					Console.Out.WriteLine("                      times. The first e-mail address will also be");
					Console.Out.WriteLine("                      encoded into the certificate request.");
					Console.Out.WriteLine("-cu URI               Adds URI to the list of contact URIs when creating");
					Console.Out.WriteLine("                      an account. Can be used multiple times.");
					Console.Out.WriteLine("-a                    You agree to the terms of service agreement. This");
					Console.Out.WriteLine("                      might be required if you want to be able to create");
					Console.Out.WriteLine("                      an account.");
					Console.Out.WriteLine("-nk                   Generates a new account key.");
					Console.Out.WriteLine("-dns DOMAIN           Adds DOMAIN to the list of domain names when creating");
					Console.Out.WriteLine("                      an order for a new certificate. Can be used multiple");
					Console.Out.WriteLine("                      times. The first DOMAIN will be used as the common name");
					Console.Out.WriteLine("                      for the certificate request. The following domain names");
					Console.Out.WriteLine("                      will be used as altenative names.");
					Console.Out.WriteLine("-nb TIMESTAMP         Generated certificate will not be valid before");
					Console.Out.WriteLine("                      TIMESTAMP.");
					Console.Out.WriteLine("-na TIMESTAMP         Generated certificate will not be valid after");
					Console.Out.WriteLine("                      TIMESTAMP.");
					Console.Out.WriteLine("-http ROOTFOLDER      Allows the application to respond to HTTP challenges");
					Console.Out.WriteLine("                      by storing temporary files under the corresponding ACME");
					Console.Out.WriteLine("                      challenge response folder /.well-known/acme-challenge");
					Console.Out.WriteLine("-pi MS                Polling Interval, in milliseconds. Default value is");
					Console.Out.WriteLine("                      5000.");
					Console.Out.WriteLine("-ks BITS              Certificate key size, in bits. Default is 4096.");
					Console.Out.WriteLine("-c COUNTRY            Country name (C) in the certificate request.");
					Console.Out.WriteLine("-l LOCALITY           Locality name (L) in the certificate request.");
					Console.Out.WriteLine("-st STATEORPROVINCE   State or Province name (ST) in the certificate request.");
					Console.Out.WriteLine("-o ORGANIZATION       Organization name (O) in the certificate request.");
					Console.Out.WriteLine("-ou ORGUNIT           Organizational unit name (OU) in the certificate request.");
					Console.Out.WriteLine("-f FILENAME           Output filename of the certificate, without file");
					Console.Out.WriteLine("                      extension.");
					Console.Out.WriteLine("-pwd PASSWORD         Password to protect the private key in the generated");
					Console.Out.WriteLine("                      certificate.");
					Console.Out.WriteLine("-v                    Verbose mode.");
					Console.Out.WriteLine("-?                    Help.");
					return;
				}

				if (directory == null)
					directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");

				if (!pollingInterval.HasValue)
					pollingInterval = 5000;

				if (!keySize.HasValue)
					keySize = 4096;

				if (verbose)
					Console.BackgroundColor = ConsoleColor.Black;

				if (fileName == null)
					throw new Exception("File name not provided.");

				if (string.IsNullOrEmpty(password))
					LogWarning("No password provided to protect the private key.");

				if (string.IsNullOrEmpty(httpRootFolder))
					LogWarning("No HTTP root folder provided. Challenge responses must be manually configured.");

				Types.Initialize(
					typeof(InternetContent).Assembly,
					typeof(AcmeClient).Assembly);

				Process().Wait();
			}
			catch (Exception ex)
			{
				while (ex != null && ex.InnerException != null)
				{
					if (ex is System.Reflection.TargetInvocationException || ex is TypeInitializationException)
						ex = ex.InnerException;
					else if (ex is AggregateException AggregateException && AggregateException.InnerExceptions.Count == 1)
						ex = AggregateException.InnerExceptions[0];
					else
						break;
				}

				if (verbose)
					LogError(ex.Message);
				else
					Console.Out.WriteLine(ex.Message);
			}
			finally
			{
				Console.ForegroundColor = FgColorBak;
				Console.BackgroundColor = BgColorBak;
			}
		}

		private static async Task Process()
		{
			using (AcmeClient Client = new AcmeClient(directory))
			{
				LogInformational("Connecting to directory.",
					new KeyValuePair<string, object>("URL", directory.ToString()));

				AcmeDirectory AcmeDirectory = await Client.GetDirectory();

				if (AcmeDirectory.ExternalAccountRequired)
					LogWarning("An external account is required.");

				if (AcmeDirectory.TermsOfService != null)
				{
					LogInformational("Terms of service available.",
						new KeyValuePair<string, object>("URL", AcmeDirectory.TermsOfService.ToString()));
				}

				if (AcmeDirectory.Website != null)
				{
					LogInformational("Web site available.",
						new KeyValuePair<string, object>("URL", AcmeDirectory.Website.ToString()));
				}


				LogInformational("Getting account.");

				string[] ContactURLs = contactURLs?.ToArray();
				string[] DomainNames = domainNames?.ToArray();

				AcmeAccount Account;

				try
				{
					Account = await Client.GetAccount();

					LogInformational("Account found.",
						new KeyValuePair<string, object>("Created", Account.CreatedAt),
						new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
						new KeyValuePair<string, object>("Status", Account.Status),
						new KeyValuePair<string, object>("Contact", Account.Contact));

					if (contactURLs != null && !AreEqual(Account.Contact, ContactURLs))
					{
						LogInformational("Updating contact URIs in account.");

						Account = await Account.Update(ContactURLs);

						LogInformational("Account updated.",
							new KeyValuePair<string, object>("Created", Account.CreatedAt),
							new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
							new KeyValuePair<string, object>("Status", Account.Status),
							new KeyValuePair<string, object>("Contact", Account.Contact));
					}
				}
				catch (AcmeAccountDoesNotExistException)
				{
					LogWarning("Account not found. Creating account.",
						new KeyValuePair<string, object>("Contact", contactURLs),
						new KeyValuePair<string, object>("TermsOfServiceAgreed", termsOfServiceAgreed));

					Account = await Client.CreateAccount(ContactURLs, termsOfServiceAgreed);

					LogInformational("Account created.",
						new KeyValuePair<string, object>("Created", Account.CreatedAt),
						new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
						new KeyValuePair<string, object>("Status", Account.Status),
						new KeyValuePair<string, object>("Contact", Account.Contact));
				}

				if (newKey)
				{
					LogInformational("Generating new key.");

					await Account.NewKey();

					LogInformational("New key generated.");
				}


				if (domainNames != null)
				{
					if (!string.IsNullOrEmpty(httpRootFolder))
					{
						CheckExists(httpRootFolder);
						httpRootFolder = Path.Combine(httpRootFolder, ".well-known");
						CheckExists(httpRootFolder);
						httpRootFolder = Path.Combine(httpRootFolder, "acme-challenge");
						CheckExists(httpRootFolder);
					}

					LogInformational("Creating order.");

					AcmeOrder Order = await Account.OrderCertificate(DomainNames, notBefore, notAfter);

					LogInformational("Order created.",
						new KeyValuePair<string, object>("Status", Order.Status),
						new KeyValuePair<string, object>("Expires", Order.Expires),
						new KeyValuePair<string, object>("NotBefore", Order.NotBefore),
						new KeyValuePair<string, object>("NotAfter", Order.NotAfter),
						new KeyValuePair<string, object>("Identifiers", Order.Identifiers));

					List<string> FileNames = null;

					try
					{
						foreach (AcmeAuthorization Authorization in await Order.GetAuthorizations())
						{
							LogInformational("Processing authorization.",
								new KeyValuePair<string, object>("Type", Authorization.Type),
								new KeyValuePair<string, object>("Value", Authorization.Value),
								new KeyValuePair<string, object>("Status", Authorization.Status),
								new KeyValuePair<string, object>("Expires", Authorization.Expires),
								new KeyValuePair<string, object>("Wildcard", Authorization.Wildcard));

							AcmeChallenge Challenge;
							bool Manual = true;
							int Index = 1;
							int NrChallenges = Authorization.Challenges.Length;
							string s;

							for (Index = 1; Index <= NrChallenges; Index++)
							{
								Challenge = Authorization.Challenges[Index - 1];

								if (Challenge is AcmeHttpChallenge HttpChallenge)
								{
									LogInformational(Index.ToString() + ") HTTP challenge.",
										new KeyValuePair<string, object>("Resource", HttpChallenge.ResourceName),
										new KeyValuePair<string, object>("Response", HttpChallenge.KeyAuthorization),
										new KeyValuePair<string, object>("Content-Type", "application/octet-stream"));

									if (!string.IsNullOrEmpty(httpRootFolder))
									{
										string ChallengeFileName = Path.Combine(httpRootFolder, HttpChallenge.Token);
										File.WriteAllBytes(ChallengeFileName, Encoding.ASCII.GetBytes(HttpChallenge.KeyAuthorization));

										if (FileNames == null)
											FileNames = new List<string>();

										FileNames.Add(ChallengeFileName);

										LogInformational("Acknowleding challenge.");

										Challenge = await HttpChallenge.AcknowledgeChallenge();

										LogInformational("Challenge acknowledged.",
											new KeyValuePair<string, object>("Status", Challenge.Status));

										Manual = false;
									}
									else if (!verbose)
									{
										Console.Out.WriteLine(Index.ToString() + ") HTTP challenge.");
										Console.Out.WriteLine("Resource: " + HttpChallenge.ResourceName);
										Console.Out.WriteLine("Response: " + HttpChallenge.KeyAuthorization);
										Console.Out.WriteLine("Content-Type: " + "application/octet-stream");
									}
								}
								else if (Challenge is AcmeDnsChallenge DnsChallenge)
								{
									LogInformational(Index.ToString() + ") DNS challenge.",
										new KeyValuePair<string, object>("Domain", DnsChallenge.ValidationDomainNamePrefix + Authorization.Value),
										new KeyValuePair<string, object>("TXT Record", DnsChallenge.KeyAuthorization));

									if (!verbose)
									{
										Console.Out.WriteLine(Index.ToString() + ") DNS challenge.");
										Console.Out.WriteLine("Domain: " + DnsChallenge.ValidationDomainNamePrefix + Authorization.Value);
										Console.Out.WriteLine("TXT Record: " + DnsChallenge.KeyAuthorization);
									}
								}
							}

							if (Manual)
							{
								Console.Out.WriteLine();
								Console.Out.WriteLine("No automated method found to respond to any of the authorization challenges. " +
									"You can respond to a challenge manually. After configuring the corresponding " +
									"resource, enter the number of the corresponding challenge and press ENTER to acknowledge it.");

								do
								{
									Console.Out.Write("Challenge to acknowledge: ");
									s = Console.In.ReadLine();
								}
								while (!int.TryParse(s, out Index) || Index <= 0 || Index > NrChallenges);

								LogInformational("Acknowleding challenge.");

								Challenge = await Authorization.Challenges[Index - 1].AcknowledgeChallenge();

								LogInformational("Challenge acknowledged.",
									new KeyValuePair<string, object>("Status", Challenge.Status));
							}

							AcmeAuthorization Authorization2 = Authorization;

							do
							{
								LogInformational("Waiting to poll authorization status.",
									new KeyValuePair<string, object>("ms", pollingInterval));

								System.Threading.Thread.Sleep(pollingInterval.Value);

								LogInformational("Polling authorization.");

								Authorization2 = await Authorization2.Poll();

								LogInformational("Authorization polled.",
									new KeyValuePair<string, object>("Type", Authorization2.Type),
									new KeyValuePair<string, object>("Value", Authorization2.Value),
									new KeyValuePair<string, object>("Status", Authorization2.Status),
									new KeyValuePair<string, object>("Expires", Authorization2.Expires),
									new KeyValuePair<string, object>("Wildcard", Authorization2.Wildcard));
							}
							while (Authorization2.Status == AcmeAuthorizationStatus.pending);

							if (Authorization2.Status != AcmeAuthorizationStatus.valid)
							{
								switch (Authorization2.Status)
								{
									case AcmeAuthorizationStatus.deactivated:
										throw new Exception("Authorization deactivated.");

									case AcmeAuthorizationStatus.expired:
										throw new Exception("Authorization expired.");

									case AcmeAuthorizationStatus.invalid:
										throw new Exception("Authorization invalid.");

									case AcmeAuthorizationStatus.revoked:
										throw new Exception("Authorization revoked.");

									default:
										throw new Exception("Authorization not validated.");
								}
							}
						}

						using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(keySize.Value))
						{
							LogInformational("Finalizing order.");

							SignatureAlgorithm SignAlg = new RsaSha256(RSA);

							Order = await Order.FinalizeOrder(new Security.ACME.CertificateRequest(SignAlg)
							{
								CommonName = domainNames[0],
								SubjectAlternativeNames = DomainNames,
								EMailAddress = eMail,
								Country = country,
								Locality = locality,
								StateOrProvince = stateOrProvince,
								Organization = organization,
								OrganizationalUnit = organizationalUnit
							});

							LogInformational("Order finalized.",
								new KeyValuePair<string, object>("Status", Order.Status),
								new KeyValuePair<string, object>("Expires", Order.Expires),
								new KeyValuePair<string, object>("NotBefore", Order.NotBefore),
								new KeyValuePair<string, object>("NotAfter", Order.NotAfter),
								new KeyValuePair<string, object>("Identifiers", Order.Identifiers));

							if (Order.Status != AcmeOrderStatus.valid)
							{
								switch (Order.Status)
								{
									case AcmeOrderStatus.invalid:
										throw new Exception("Order invalid.");

									default:
										throw new Exception("Unable to validate oder.");
								}
							}

							if (Order.Certificate == null)
								throw new Exception("No certificate URI provided.");

							X509Certificate2[] Certificates = await Order.DownloadCertificate();

							string CertificateFileNameBase;
							string CertificateFileName;
							string CertificateFileName2;
							int Index = 1;
							byte[] Bin;

							DerEncoder KeyOutput = new DerEncoder();
							SignAlg.ExportPrivateKey(KeyOutput);

							StringBuilder PemOutput = new StringBuilder();

							PemOutput.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
							PemOutput.AppendLine(Convert.ToBase64String(KeyOutput.ToArray(), Base64FormattingOptions.InsertLineBreaks));
							PemOutput.AppendLine("-----END RSA PRIVATE KEY-----");

							CertificateFileName = fileName + ".key";

							LogInformational("Saving private key.",
								new KeyValuePair<string, object>("FileName", CertificateFileName));

							File.WriteAllText(CertificateFileName, PemOutput.ToString(), Encoding.ASCII);

							foreach (X509Certificate2 Certificate in Certificates)
							{
								if (Index == 1)
									CertificateFileNameBase = fileName;
								else
									CertificateFileNameBase = fileName + Index.ToString();

								CertificateFileName = CertificateFileNameBase + ".cer";
								CertificateFileName2 = CertificateFileNameBase + ".pem";

								Bin = Certificate.Export(X509ContentType.Cert);

								LogInformational("Saving certificate.",
									new KeyValuePair<string, object>("FileName", CertificateFileName),
									new KeyValuePair<string, object>("FileName2", CertificateFileName2),
									new KeyValuePair<string, object>("FriendlyName", Certificate.FriendlyName),
									new KeyValuePair<string, object>("HasPrivateKey", Certificate.HasPrivateKey),
									new KeyValuePair<string, object>("Issuer", Certificate.Issuer),
									new KeyValuePair<string, object>("NotAfter", Certificate.NotAfter),
									new KeyValuePair<string, object>("NotBefore", Certificate.NotBefore),
									new KeyValuePair<string, object>("SerialNumber", Certificate.SerialNumber),
									new KeyValuePair<string, object>("Subject", Certificate.Subject),
									new KeyValuePair<string, object>("Thumbprint", Certificate.Thumbprint));

								File.WriteAllBytes(CertificateFileName, Bin);

								PemOutput.Clear();
								PemOutput.AppendLine("-----BEGIN CERTIFICATE-----");
								PemOutput.AppendLine(Convert.ToBase64String(Bin, Base64FormattingOptions.InsertLineBreaks));
								PemOutput.AppendLine("-----END CERTIFICATE-----");

								File.WriteAllText(CertificateFileName2, PemOutput.ToString(), Encoding.ASCII);

								if (Index == 1)
								{
									try
									{
										CertificateFileName = CertificateFileNameBase + ".pfx";

										LogInformational("Exporting to PFX.",
											new KeyValuePair<string, object>("FileName", CertificateFileName));

										Certificate.PrivateKey = RSA;
										Bin = Certificate.Export(X509ContentType.Pfx, password);

										File.WriteAllBytes(CertificateFileName, Bin);
									}
									catch (Exception ex)
									{
										LogError("Unable to export certificate to PFX: " + ex.Message);
									}
								}

								Index++;
							}
						}
					}
					finally
					{
						if (FileNames != null)
						{
							foreach (string FileName2 in FileNames)
								File.Delete(FileName2);
						}
					}
				}
			}
		}

		private static void CheckExists(string Folder)
		{
			if (!Directory.Exists(Folder))
			{
				LogInformational("Creating folder.",
					new KeyValuePair<string, object>("Folder", Folder));

				Directory.CreateDirectory(Folder);

				LogInformational("Folder created.",
					new KeyValuePair<string, object>("Folder", Folder));
			}
		}

		private static bool AreEqual(string[] A1, string[] A2)
		{
			int i, c;

			if (A1 == null ^ A2 == null)
				return false;

			if (A1 == null)
				return true;

			c = A1.Length;
			if (A2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (A1[i] != A2[i])
					return false;
			}

			return true;
		}

		private static void LogInformational(string Message, params KeyValuePair<string, object>[] Tags)
		{
			if (verbose)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Log(Message, Tags);
			}
		}

		private static void LogWarning(string Message, params KeyValuePair<string, object>[] Tags)
		{
			if (verbose)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Log(Message, Tags);
			}
		}

		private static void LogError(string Message, params KeyValuePair<string, object>[] Tags)
		{
			if (verbose)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Log(Message, Tags);
			}
		}

		private static void Log(string Message, params KeyValuePair<string, object>[] Tags)
		{
			Console.Out.WriteLine(Message);

			foreach (KeyValuePair<string, object> Tag in Tags)
			{
				Console.Out.Write('\t');
				Console.Out.Write(Tag.Key);
				Console.Out.Write(": ");

				if (Tag.Value is Array A)
				{
					bool First = true;

					foreach (object Item in A)
					{
						if (First)
							First = false;
						else
							Console.Out.Write(", ");

						Console.Out.Write(Item?.ToString());
					}
				}
				else
					Console.Out.Write(Tag.Value?.ToString());

				Console.Out.WriteLine();
			}
		}

		/* TODO:
		 * 
		 * pre-authorization 7.4.1
		 * Retry-After (rate limiting when polling)
		 * Revoke certificate 7.6
		 */
	}
}
