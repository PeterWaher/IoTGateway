using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
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
		static void Main(string[] args)
		{
			Uri Directory = null;
			List<string> ContactURLs = null;
			List<string> DomainNames = null;
			DateTime? NotBefore = null;
			DateTime? NotAfter = null;
			string HttpRootFolder = null;
			string EMail = null;
			string Country = null;
			string Locality = null;
			string StateOrProvince = null;
			string Organization = null;
			string OrganizationalUnit = null;
			string FileName = null;
			string Password = string.Empty;
			string s;
			int? PollingIngerval = null;
			int? KeySize = null;
			int i = 0;
			int c = args.Length;
			bool Help = false;
			bool Verbose = false;
			bool TermsOfServiceAgreed = false;
			bool NewKey = false;

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

							if (Directory == null)
								Directory = new Uri(args[i++]);
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-le":
							if (Directory == null)
								Directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-let":
							if (Directory == null)
								Directory = new Uri("https://acme-staging-v02.api.letsencrypt.org/directory");
							else
								throw new Exception("Only one directory URI allowed.");
							break;

						case "-ce":
							if (i >= c)
								throw new Exception("Missing contact e-mail.");

							if (ContactURLs == null)
								ContactURLs = new List<string>();

							if (EMail == null)
								EMail = args[i];

							ContactURLs.Add("mailto:" + args[i++]);
							break;

						case "-cu":
							if (i >= c)
								throw new Exception("Missing contact URI.");

							if (ContactURLs == null)
								ContactURLs = new List<string>();

							ContactURLs.Add(args[i++]);
							break;

						case "-dns":
							if (i >= c)
								throw new Exception("Missing domain name.");

							if (DomainNames == null)
								DomainNames = new List<string>();

							DomainNames.Add(args[i++]);
							break;

						case "-na":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out DateTime TP))
								NotAfter = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-nb":
							if (i >= c)
								throw new Exception("Missing timestamp.");

							if (DateTime.TryParse(args[i++], out TP))
								NotBefore = TP;
							else
								throw new Exception("Invalid timestamp: " + args[i - 1]);
							break;

						case "-http":
							if (i >= c)
								throw new Exception("Missing HTTP root folder.");

							if (HttpRootFolder == null)
								HttpRootFolder = args[i++];
							else
								throw new Exception("Only one HTTP Root Folder allowed.");
							break;

						case "-pi":
							if (i >= c)
								throw new Exception("Missing polling interval.");

							if (!int.TryParse(args[i++], out int j) || j <= 0)
								throw new Exception("Invalid polling interval.");

							if (PollingIngerval.HasValue)
								throw new Exception("Only one polling interval allowed.");
							else
								PollingIngerval = j;
							break;

						case "-ks":
							if (i >= c)
								throw new Exception("Missing key size.");

							if (!int.TryParse(args[i++], out j) || j <= 0)
								throw new Exception("Invalid key size.");

							if (KeySize.HasValue)
								throw new Exception("Only one key size allowed.");
							else
								KeySize = j;
							break;

						case "-c":
							if (i >= c)
								throw new Exception("Missing country name.");

							if (Country == null)
								Country = args[i++];
							else
								throw new Exception("Only one country name allowed.");
							break;

						case "-l":
							if (i >= c)
								throw new Exception("Missing locality name.");

							if (Locality == null)
								Locality = args[i++];
							else
								throw new Exception("Only one locality name allowed.");
							break;

						case "-st":
							if (i >= c)
								throw new Exception("Missing state or province name.");

							if (StateOrProvince == null)
								StateOrProvince = args[i++];
							else
								throw new Exception("Only one state or province name allowed.");
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing organization name.");

							if (Organization == null)
								Organization = args[i++];
							else
								throw new Exception("Only one organization name allowed.");
							break;

						case "-ou":
							if (i >= c)
								throw new Exception("Missing organizational unit name.");

							if (OrganizationalUnit == null)
								OrganizationalUnit = args[i++];
							else
								throw new Exception("Only one organizational unit name allowed.");
							break;

						case "-f":
							if (i >= c)
								throw new Exception("Missing file name.");

							if (FileName == null)
								FileName = args[i++];
							else
								throw new Exception("Only one file name allowed.");
							break;

						case "-pwd":
							if (i >= c)
								throw new Exception("Missing password.");

							if (string.IsNullOrEmpty(Password))
								Password = args[i++];
							else
								throw new Exception("Only one password allowed.");
							break;

						case "-?":
							Help = true;
							break;

						case "-v":
							Verbose = true;
							break;

						case "-a":
							TermsOfServiceAgreed = true;
							break;

						case "-nk":
							NewKey = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
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

				if (Verbose)
					Log.Register(new ConsoleEventSink(false));

				if (Directory == null)
					Directory = new Uri("https://acme-v02.api.letsencrypt.org/directory");

				if (!PollingIngerval.HasValue)
					PollingIngerval = 5000;

				if (!KeySize.HasValue)
					KeySize = 4096;

				if (FileName == null)
					throw new Exception("File name not provided.");

				if (string.IsNullOrEmpty(Password))
					Log.Warning("No password provided to protect the private key.");

				if (string.IsNullOrEmpty(HttpRootFolder))
					Log.Warning("No HTTP root folder provided. Challenge responses must be manually configured.");

				Types.Initialize(
					typeof(InternetContent).Assembly,
					typeof(AcmeClient).Assembly);

				ManualResetEvent Done = new ManualResetEvent(false);

				Process(Verbose, Directory, ContactURLs?.ToArray(), TermsOfServiceAgreed, NewKey,
					DomainNames?.ToArray(), NotBefore, NotAfter, HttpRootFolder, PollingIngerval.Value,
					KeySize.Value, EMail, Country, Locality, StateOrProvince, Organization,
					OrganizationalUnit, FileName, Password, Done).Wait();

				Done.WaitOne();
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (Verbose)
					Log.Error(ex.Message);
				else
					Console.Out.WriteLine(ex.Message);
			}
		}

		private static async Task Process(bool Verbose, Uri Directory, string[] ContactURLs, bool TermsOfServiceAgreed,
			bool NewKey, string[] DomainNames, DateTime? NotBefore, DateTime? NotAfter, string HttpRootFolder, int PollingInterval,
			int KeySize, string EMail, string Country, string Locality, string StateOrProvince, string Organization,
			string OrganizationalUnit, string FileName, string Password, ManualResetEvent Done)
		{
			try
			{
				using (AcmeClient Client = new AcmeClient(Directory))
				{
					Log.Informational("Connecting to directory.",
						new KeyValuePair<string, object>("URL", Directory.ToString()));

					AcmeDirectory AcmeDirectory = await Client.GetDirectory();

					if (AcmeDirectory.ExternalAccountRequired)
						Log.Warning("An external account is required.");

					if (AcmeDirectory.TermsOfService != null)
					{
						Log.Informational("Terms of service available.",
							new KeyValuePair<string, object>("URL", AcmeDirectory.TermsOfService.ToString()));
					}

					if (AcmeDirectory.Website != null)
					{
						Log.Informational("Web site available.",
							new KeyValuePair<string, object>("URL", AcmeDirectory.Website.ToString()));
					}


					Log.Informational("Getting account.");

					AcmeAccount Account;

					try
					{
						Account = await Client.GetAccount();

						Log.Informational("Account found.",
							new KeyValuePair<string, object>("Created", Account.CreatedAt),
							new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
							new KeyValuePair<string, object>("Status", Account.Status),
							new KeyValuePair<string, object>("Contact", Account.Contact));

						if (ContactURLs != null && !AreEqual(Account.Contact, ContactURLs))
						{
							Log.Informational("Updating contact URIs in account.");

							Account = await Account.Update(ContactURLs);

							Log.Informational("Account updated.",
								new KeyValuePair<string, object>("Created", Account.CreatedAt),
								new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
								new KeyValuePair<string, object>("Status", Account.Status),
								new KeyValuePair<string, object>("Contact", Account.Contact));
						}
					}
					catch (AcmeAccountDoesNotExistException)
					{
						Log.Warning("Account not found. Creating account.",
							new KeyValuePair<string, object>("Contact", ContactURLs),
							new KeyValuePair<string, object>("TermsOfServiceAgreed", TermsOfServiceAgreed));

						Account = await Client.CreateAccount(ContactURLs, TermsOfServiceAgreed);

						Log.Informational("Account created.",
							new KeyValuePair<string, object>("Created", Account.CreatedAt),
							new KeyValuePair<string, object>("Initial IP", Account.InitialIp),
							new KeyValuePair<string, object>("Status", Account.Status),
							new KeyValuePair<string, object>("Contact", Account.Contact));
					}

					if (NewKey)
					{
						Log.Informational("Generating new key.");

						await Account.NewKey();

						Log.Informational("New key generated.");
					}


					if (DomainNames != null)
					{
						if (!string.IsNullOrEmpty(HttpRootFolder))
						{
							CheckExists(HttpRootFolder);
							HttpRootFolder = Path.Combine(HttpRootFolder, ".well-known");
							CheckExists(HttpRootFolder);
							HttpRootFolder = Path.Combine(HttpRootFolder, "acme-challenge");
							CheckExists(HttpRootFolder);
						}

						Log.Informational("Creating order.");

						AcmeOrder Order = await Account.OrderCertificate(DomainNames, NotBefore, NotAfter);

						Log.Informational("Order created.",
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
								Log.Informational("Processing authorization.",
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
										Log.Informational(Index.ToString() + ") HTTP challenge.",
											new KeyValuePair<string, object>("Resource", HttpChallenge.ResourceName),
											new KeyValuePair<string, object>("Response", HttpChallenge.KeyAuthorization),
											new KeyValuePair<string, object>("Content-Type", "application/octet-stream"));

										if (!string.IsNullOrEmpty(HttpRootFolder))
										{
											string ChallengeFileName = Path.Combine(HttpRootFolder, HttpChallenge.Token);
											File.WriteAllBytes(ChallengeFileName, Encoding.ASCII.GetBytes(HttpChallenge.KeyAuthorization));

											if (FileNames == null)
												FileNames = new List<string>();

											FileNames.Add(ChallengeFileName);

											Log.Informational("Acknowleding challenge.");

											Challenge = await HttpChallenge.AcknowledgeChallenge();

											Log.Informational("Challenge acknowledged.",
												new KeyValuePair<string, object>("Status", Challenge.Status));

											Manual = false;
										}
										else if (!Verbose)
										{
											Console.Out.WriteLine(Index.ToString() + ") HTTP challenge.");
											Console.Out.WriteLine("Resource: " + HttpChallenge.ResourceName);
											Console.Out.WriteLine("Response: " + HttpChallenge.KeyAuthorization);
											Console.Out.WriteLine("Content-Type: " + "application/octet-stream");
										}
									}
									else if (Challenge is AcmeDnsChallenge DnsChallenge)
									{
										Log.Informational(Index.ToString() + ") DNS challenge.",
											new KeyValuePair<string, object>("Domain", DnsChallenge.ValidationDomainNamePrefix + Authorization.Value),
											new KeyValuePair<string, object>("TXT Record", DnsChallenge.KeyAuthorization));

										if (!Verbose)
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

									Log.Informational("Acknowleding challenge.");

									Challenge = await Authorization.Challenges[Index - 1].AcknowledgeChallenge();

									Log.Informational("Challenge acknowledged.",
										new KeyValuePair<string, object>("Status", Challenge.Status));
								}

								AcmeAuthorization Authorization2 = Authorization;

								do
								{
									Log.Informational("Waiting to poll authorization status.",
										new KeyValuePair<string, object>("ms", PollingInterval));

									System.Threading.Thread.Sleep(PollingInterval);

									Log.Informational("Polling authorization.");

									Authorization2 = await Authorization2.Poll();

									Log.Informational("Authorization polled.",
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

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KeySize))
							{
								Log.Informational("Finalizing order.");

								SignatureAlgorithm SignAlg = new RsaSha256(RSA);

								Order = await Order.FinalizeOrder(new Security.ACME.CertificateRequest(SignAlg)
								{
									CommonName = DomainNames[0],
									SubjectAlternativeNames = DomainNames,
									EMailAddress = EMail,
									Country = Country,
									Locality = Locality,
									StateOrProvince = StateOrProvince,
									Organization = Organization,
									OrganizationalUnit = OrganizationalUnit
								});

								Log.Informational("Order finalized.",
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

								System.Security.Cryptography.X509Certificates.X509Certificate2[] Certificates =
									await Order.DownloadCertificate();

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

								CertificateFileName = FileName + ".key";

								Log.Informational("Saving private key.",
									new KeyValuePair<string, object>("FileName", CertificateFileName));

								File.WriteAllText(CertificateFileName, PemOutput.ToString(), Encoding.ASCII);

								foreach (X509Certificate2 Certificate in Certificates)
								{
									if (Index == 1)
										CertificateFileNameBase = FileName;
									else
										CertificateFileNameBase = FileName + Index.ToString();

									CertificateFileName = CertificateFileNameBase + ".cer";
									CertificateFileName2 = CertificateFileNameBase + ".pem";

									Bin = Certificate.Export(X509ContentType.Cert);

									Log.Informational("Saving certificate.",
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

											Log.Informational("Exporting to PFX.",
												new KeyValuePair<string, object>("FileName", CertificateFileName));

											Certificate.PrivateKey = RSA;
											Bin = Certificate.Export(X509ContentType.Pfx, Password);

											File.WriteAllBytes(CertificateFileName, Bin);
										}
										catch (Exception ex)
										{
											Log.Error("Unable to export certificate to PFX: " + ex.Message);
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
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				Console.Out.WriteLine(ex.StackTrace);
				Log.Error(ex);
			}
			finally
			{
				Done.Set();
			}
		}

		private static void CheckExists(string Folder)
		{
			if (!Directory.Exists(Folder))
			{
				Log.Informational("Creating folder.",
					new KeyValuePair<string, object>("Folder", Folder));

				Directory.CreateDirectory(Folder);

				Log.Informational("Folder created.",
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

		/* TODO:
		 * 
		 * pre-authorization 7.4.1
		 * Retry-After (rate limiting when polling)
		 * Revoke certificate 7.6
		 */
	}
}
