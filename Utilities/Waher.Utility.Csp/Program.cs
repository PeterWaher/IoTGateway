using System;
using System.Security.Cryptography;
using Waher.Runtime.Console;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.Utility.Csp
{
	class Program
	{
		static void Main(string[] Arguments)
		{
			try
			{
				CspProviderFlags Flags = CspProviderFlags.UseMachineKeyStore;
				CspParameters Csp;
				RSAParameters Parameters;
				int i, c = Arguments.Length;
				string s;
				string Key = null;
				int KeySize = 4096;

				if (c == 0)
				{
					c = 1;
					Arguments = new string[] { "-?" };
				}

				for (i = 0; i < c; i++)
				{
					s = Arguments[i];
					switch (s.ToLower())
					{
						case "-?":
							ConsoleOut.WriteLine("Command-line arguments:");
							ConsoleOut.WriteLine();
							ConsoleOut.WriteLine("-?     Shows this help.");
							ConsoleOut.WriteLine("-key NAME     Provides a name of a key.");
							ConsoleOut.WriteLine("-size SIZE    Provides a key size of the currently named key.");
							ConsoleOut.WriteLine("-currentuser  Uses the current user key store.");
							ConsoleOut.WriteLine("-cu           Same as -currentuser.");
							ConsoleOut.WriteLine("-machine      Uses the current machine key store.");
							ConsoleOut.WriteLine("-m            Same as -machine.");
							ConsoleOut.WriteLine("-default      Uses the default key store.");
							ConsoleOut.WriteLine("-d            Same as -default.");
							ConsoleOut.WriteLine("-ephemeral    Used for creating an ephemeral key.");
							ConsoleOut.WriteLine("-e            Same as -ephemeral.");
							ConsoleOut.WriteLine("-create       Creates a key with the given name and size.");
							ConsoleOut.WriteLine("-delete       Deletes the currently named key.");
							ConsoleOut.WriteLine("-get          Gets the currently named key.");
							break;

						case "-currentuser":
						case "-cu":
							Flags = CspProviderFlags.UseUserProtectedKey;
							break;

						case "-machine":
						case "-m":
							Flags = CspProviderFlags.UseMachineKeyStore;
							break;

						case "-default":
						case "-d":
							Flags = CspProviderFlags.UseDefaultKeyContainer;
							break;

						case "-ephemeral":
						case "-e":
							Flags = CspProviderFlags.CreateEphemeralKey;
							break;

						case "-key":
							if (++i >= c)
								throw new Exception("Expected key name.");

							Key = Arguments[i];
							break;

						case "-size":
							if (++i >= c)
								throw new Exception("Expected key size.");

							if (!int.TryParse(Arguments[i], out int j) || j <= 0)
								throw new Exception("Invalid key size.");

							KeySize = j;
							break;

						case "-create":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags
							};

							using (RSACryptoServiceProvider RSA = new(KeySize, Csp))
							{
								ConsoleOut.WriteLine("Key created.");
							}
							break;

						case "-delete":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags | CspProviderFlags.UseExistingKey
							};

							using (RSACryptoServiceProvider RSA = new(KeySize, Csp))
							{
								RSA.PersistKeyInCsp = false;    // Deletes key.
								RSA.Clear();
							}

							ConsoleOut.WriteLine("Key deleted.");
							break;

						case "-get":
							Csp = new CspParameters()
							{
								KeyContainerName = Key,
								Flags = Flags | CspProviderFlags.UseExistingKey
							};

							using (RSACryptoServiceProvider RSA = new(KeySize, Csp))
							{
								Parameters = RSA.ExportParameters(true);

								ConsoleOut.WriteLine("Public:");
								ConsoleOut.WriteLine("n: " + Convert.ToBase64String(Parameters.Modulus));
								ConsoleOut.WriteLine("e: " + Convert.ToBase64String(Parameters.Exponent));

								ConsoleOut.WriteLine("Private:");
								ConsoleOut.WriteLine("p: " + Convert.ToBase64String(Parameters.P));
								ConsoleOut.WriteLine("q: " + Convert.ToBase64String(Parameters.Q));
								ConsoleOut.WriteLine("d: " + Convert.ToBase64String(Parameters.D));
								ConsoleOut.WriteLine("dP: " + Convert.ToBase64String(Parameters.DP));
								ConsoleOut.WriteLine("dQ: " + Convert.ToBase64String(Parameters.DQ));
								ConsoleOut.WriteLine("qinv: " + Convert.ToBase64String(Parameters.InverseQ));
							}
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
			}
		}
	}
}

#pragma warning restore CA1416 // Validate platform compatibility
