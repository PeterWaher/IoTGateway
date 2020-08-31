using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Runtime.Inventory;

namespace Waher.Utility.Sign
{
    class Program
    {
        private const string Namespace = "http://waher.se/Schema/Signatures.xsd";

        /// <summary>
        /// Signs files using an asymmetric cipher.
        /// 
        /// Command line switches:
        /// 
        /// -c NAME                Creates a new cipher object with random keys,
        ///                        given the name of the cipher.
        /// -l                     Lists supported cipher names.
        /// -keys                  Exports keys of selected cipher object.
        /// -pub KEY               Creates a new cipher object of the same type as
        ///                        the current one, using a public key. Such a
        ///                        cipher object can be used for validating
        ///                        signatures.
        /// -priv KEY              Creates a new cipher object of the same type as
        ///                        the current one, using a private key. Such a
        ///                        cipher can be used to create signatures.
        /// -o FILENAME            Directs output to the XML file FILENAME.
        /// -s FILENAME            Signs a file with the given filename.
        ///                        FILENAME can contain wildcards. This will
        ///                        generate one signature per file.
        /// -r                     If recursive search for files is to be used.
        /// -v FILENAME SIGNATURE  Validates a signature for the selected file.
        /// -?                     Help.
        /// </summary>
        static int Main(string[] args)
        {
            IE2eEndpoint Endpoint = null;
            string OutputFileName = null;
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string s;
            byte[] Bin, Bin2;
            XmlWriter Output = null;
            int i = 0;
            int c = args.Length;
            bool Help = false;
            bool Recursive = false;

            try
            {
                Types.Initialize(
                    typeof(Program).Assembly,
                    typeof(IE2eEndpoint).Assembly,
                    typeof(EndpointSecurity).Assembly,
                    typeof(Security.EllipticCurves.EllipticCurve).Assembly);

                while (i < c)
                {
                    s = args[i++].ToLower();

                    switch (s)
                    {
                        case "-c":
                            if (i >= c)
                                throw new Exception("Missing cipher name.");

                            s = args[i++];
                            if (!EndpointSecurity.TryCreateEndpoint(s, EndpointSecurity.IoTHarmonizationE2E, out Endpoint))
                                throw new Exception("Algorithm not recognized: " + s);

                            break;

                        case "-l":

                            foreach (Type T in Types.GetTypesImplementingInterface(typeof(IE2eEndpoint)))
                            {
                                TypeInfo TI = T.GetTypeInfo();
                                if (TI.IsAbstract)
                                    continue;

                                try
                                {
                                    using (IE2eEndpoint Endpoint2 = (IE2eEndpoint)Activator.CreateInstance(T))
                                    {
                                        if (Endpoint2.Namespace == EndpointSecurity.IoTHarmonizationE2E)
                                        {
                                            if (Output is null)
                                                Console.Out.WriteLine(Endpoint2.LocalName);
                                            else
                                                Output.WriteElementString("Cipher", Namespace, Endpoint2.LocalName);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            break;

                        case "-keys":
                            if (Endpoint is null)
                                throw new Exception("No cipher has been selected.");

                            if (Output is null)
                                Console.Out.WriteLine("Public Key: " + Endpoint.PublicKeyBase64);
                            else
                                Output.WriteElementString("PublicKey", Namespace, Endpoint.PublicKeyBase64);

                            if (Endpoint is EllipticCurveEndpoint EC)
                            {
                                s = EC.Curve.Export();
                                XmlDocument Doc = new XmlDocument()
                                {
                                    PreserveWhitespace = true
                                };
                                Doc.LoadXml(s);
                                s = Doc.DocumentElement.GetAttribute("d");
                            }
                            else if (Endpoint is RsaEndpoint Rsa)
                            {
                                Bin = Rsa.Export(true);
                                s = Convert.ToBase64String(Bin);
                            }
                            else
                                break;

                            if (Output is null)
                                Console.Out.WriteLine("Private Key: " + s);
                            else
                                Output.WriteElementString("PrivateKey", Namespace, s);

                            break;

                        case "-pub":
                            if (Endpoint is null)
                                throw new Exception("No cipher has been selected.");

                            if (i >= c)
                                throw new Exception("Missing public key.");

                            s = args[i++];

                            try
                            {
                                Bin = Convert.FromBase64String(s);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Invalid public key.");
                            }

                            Endpoint = Endpoint.CreatePublic(Bin);
                            break;

                        case "-priv":
                            if (Endpoint is null)
                                throw new Exception("No cipher has been selected.");

                            if (i >= c)
                                throw new Exception("Missing private key.");

                            s = args[i++];

                            try
                            {
                                Bin = Convert.FromBase64String(s);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Invalid private key.");
                            }

                            Endpoint = Endpoint.CreatePrivate(Bin);
                            break;

                        case "-o":
                            if (i >= c)
                                throw new Exception("Missing output file name.");

                            if (string.IsNullOrEmpty(OutputFileName))
                                OutputFileName = args[i++];
                            else
                                throw new Exception("Only one output file name allowed.");

                            XmlWriterSettings Settings = new XmlWriterSettings()
                            {
                                CloseOutput = true,
                                ConformanceLevel = ConformanceLevel.Document,
                                Encoding = Encoding.UTF8,
                                Indent = true,
                                IndentChars = "\t",
                                NewLineChars = "\r\n",
                                NewLineHandling = NewLineHandling.Entitize,
                                NewLineOnAttributes = false,
                                OmitXmlDeclaration = false,
                                WriteEndDocumentOnClose = true,
                                NamespaceHandling = NamespaceHandling.OmitDuplicates
                            };

                            Output = XmlWriter.Create(OutputFileName, Settings);
                            Output.WriteStartDocument();
                            Output.WriteStartElement("Signatures", Namespace);
                            break;

                        case "-s":
                            if (Endpoint is null)
                                throw new Exception("No cipher has been selected.");

                            if (i >= c)
                                throw new Exception("Missing input file name.");

                            s = args[i++];

                            string[] FileNames;

                            if (s.Contains("*") || s.Contains("?"))
                            {
                                s = Path.Combine(CurrentDirectory, s);

                                string FileName = Path.GetFileName(s);
                                string Folder = Path.GetDirectoryName(s);

                                FileNames = Directory.GetFiles(Folder, FileName, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                            }
                            else
                                FileNames = new string[] { s };

                            foreach (string FileName in FileNames)
                            {
                                using (FileStream f = File.OpenRead(FileName))
                                {
                                    Bin = Endpoint.Sign(f);
                                }
                                s = Convert.ToBase64String(Bin);

                                if (Output is null)
                                    Console.Out.WriteLine("Signature: " + s);
                                else
                                {
                                    Output.WriteStartElement("Signature");
                                    Output.WriteAttributeString("fileName", Path.GetRelativePath(CurrentDirectory, FileName));
                                    Output.WriteValue(s);
                                    Output.WriteEndElement();
                                }
                            }
                            break;

                        case "-v":
                            if (Endpoint is null)
                                throw new Exception("No cipher has been selected.");

                            if (i >= c)
                                throw new Exception("Missing input file name.");

                            s = args[i++];

                            Bin = File.ReadAllBytes(s);

                            if (i >= c)
                                throw new Exception("Missing signature.");

                            try
                            {
                                Bin2 = Convert.FromBase64String(args[i++]);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Invalid signature.");
                            }

                            bool Valid = Endpoint.Verify(Bin, Bin2);

                            if (Output is null)
                                Console.Out.WriteLine("Valid: " + Valid.ToString());
                            else
                            {
                                Output.WriteStartElement("Valid");
                                Output.WriteAttributeString("fileName", Path.GetRelativePath(CurrentDirectory, s));
                                Output.WriteValue(Valid ? "true" : "false");
                                Output.WriteEndElement();
                            }
                            break;

                        case "-?":
                            Help = true;
                            break;

                        case "-r":
                            Recursive = true;
                            break;

                        default:
                            throw new Exception("Unrecognized switch: " + s);
                    }
                }

                if (Help || c == 0)
                {
                    Console.Out.WriteLine("Signs files using an asymmetric cipher.");
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Command line switches:");
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("-c NAME                Creates a new cipher object with random keys,");
                    Console.Out.WriteLine("                       given the name of the cipher.");
                    Console.Out.WriteLine("-l                     Lists supported cipher names.");
                    Console.Out.WriteLine("-keys                  Exports keys of selected cipher object.");
                    Console.Out.WriteLine("-pub KEY               Creates a new cipher object of the same type as");
                    Console.Out.WriteLine("                       the current one, using a public key. Such a");
                    Console.Out.WriteLine("                       cipher object can be used for validating");
                    Console.Out.WriteLine("                       signatures.");
                    Console.Out.WriteLine("-priv KEY              Creates a new cipher object of the same type as");
                    Console.Out.WriteLine("                       the current one, using a private key. Such a");
                    Console.Out.WriteLine("                       cipher can be used to create signatures.");
                    Console.Out.WriteLine("-o FILENAME            Directs output to the XML file FILENAME.");
                    Console.Out.WriteLine("-s FILENAME            Signs a file with the given filename.");
                    Console.Out.WriteLine("                       FILENAME can contain wildcards. This will");
                    Console.Out.WriteLine("                       generate one signature per file.");
                    Console.Out.WriteLine("-r                     If recursive search for files is to be used.");
                    Console.Out.WriteLine("-v FILENAME SIGNATURE  Validates a signature for the selected file.");
                    Console.Out.WriteLine("-?                     Help.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                Output?.WriteEndElement();
                Output?.WriteEndDocument();
                Output?.Dispose();
            }
        }
    }
}
