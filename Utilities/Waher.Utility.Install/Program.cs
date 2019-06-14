using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Security.SHA3;

namespace Waher.Utility.Install
{
    /// <summary>
    /// Installs a module in an IoT Gateway.
    /// 
    /// Command line switches:
    /// 
    /// -m MANIFEST_FILE     Points to the manifest file describing the files in the module.
    /// -d APP_DATA_FOLDER   Points to the application data folder.
    /// -s SERVER_EXE        Points to the executable file of the IoT Gateway.
    /// -p PACKAGE_FILE      If provided, files will be packed into a package file that can
    ///                      be easily distributed, instead of being installed on the local
    ///                      machine.
    /// -k KEY               Encrypts the package file with the KEY, if provided.
    ///                      Secret used in encryption is based on the local package file
    ///                      name and the KEY parameter, if provided. You cannot rename
    ///                      a package file.
    /// -v                   Verbose mode.
    /// -i                   Install. This is the default. Switch not required.
    /// -u                   Uninstall. Add this switch if the module is being uninstalled.
    /// -r                   Remove files. Add this switch if you want files removed during
    ///                      uninstallation. Default is to not remove files.
    /// -?                   Help.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string ManifestFile = null;
                string ProgramDataFolder = null;
                string ServerApplication = null;
                string PackageFile = null;
                string Key = string.Empty;
                int i = 0;
                int c = args.Length;
                string s;
                bool Help = false;
                bool Verbose = false;
                bool UninstallService = false;
                bool RemoveFiles = false;

                while (i < c)
                {
                    s = args[i++].ToLower();

                    switch (s)
                    {
                        case "-m":
                            if (i >= c)
                                throw new Exception("Missing manifest file.");

                            if (string.IsNullOrEmpty(ManifestFile))
                                ManifestFile = args[i++];
                            else
                                throw new Exception("Only one manifest file allowed.");
                            break;

                        case "-d":
                            if (i >= c)
                                throw new Exception("Missing program data folder.");

                            if (string.IsNullOrEmpty(ProgramDataFolder))
                                ProgramDataFolder = args[i++];
                            else
                                throw new Exception("Only one program data folder allowed.");
                            break;

                        case "-s":
                            if (i >= c)
                                throw new Exception("Missing server application.");

                            if (string.IsNullOrEmpty(ServerApplication))
                                ServerApplication = args[i++];
                            else
                                throw new Exception("Only one server application allowed.");
                            break;

                        case "-p":
                            if (i >= c)
                                throw new Exception("Missing package file name.");

                            if (string.IsNullOrEmpty(PackageFile))
                                PackageFile = args[i++];
                            else
                                throw new Exception("Only one package file name allowed.");
                            break;

                        case "-k":
                            if (i >= c)
                                throw new Exception("Missing key.");

                            if (string.IsNullOrEmpty(Key))
                                Key = args[i++];
                            else
                                throw new Exception("Only one key allowed.");
                            break;

                        case "-i":
                            UninstallService = false;
                            break;

                        case "-u":
                            UninstallService = true;
                            break;

                        case "-r":
                            RemoveFiles = true;
                            break;

                        case "-v":
                            Verbose = true;
                            break;

                        case "-?":
                            Help = true;
                            break;

                        default:
                            throw new Exception("Unrecognized switch: " + s);
                    }
                }

                if (Help || c == 0)
                {
                    Console.Out.WriteLine("-m MANIFEST_FILE     Points to the manifest file describing the files in the module.");
                    Console.Out.WriteLine("-d APP_DATA_FOLDER   Points to the application data folder.");
                    Console.Out.WriteLine("-s SERVER_EXE        Points to the executable file of the IoT Gateway.");
                    Console.Out.WriteLine("-p PACKAGE_FILE      If provided, files will be packed into a package file that can");
                    Console.Out.WriteLine("                     be easily distributed, instead of being installed on the local");
                    Console.Out.WriteLine("                     machine.");
                    Console.Out.WriteLine("-k KEY               Encrypts the package file with the KEY, if provided.");
                    Console.Out.WriteLine("                     Secret used in encryption is based on the local package file");
                    Console.Out.WriteLine("                     name and the KEY parameter, if provided. You cannot rename");
                    Console.Out.WriteLine("                     a package file.");
                    Console.Out.WriteLine("-v                   Verbose mode.");
                    Console.Out.WriteLine("-i                   Install. This the default. Switch not required.");
                    Console.Out.WriteLine("-u                   Uninstall. Add this switch if the module is being uninstalled.");
                    Console.Out.WriteLine("-r                   Remove files. Add this switch if you want files removed during");
                    Console.Out.WriteLine("                     uninstallation. Default is to not remove files.");
                    Console.Out.WriteLine("-?                   Help.");
                    return 0;
                }

                if (Verbose)
                    Log.Register(new Events.Console.ConsoleEventSink());

                if (!string.IsNullOrEmpty(PackageFile))
                    GeneratePackage(ManifestFile, PackageFile, Key);
                else if (UninstallService)
                    Uninstall(ManifestFile, ServerApplication, ProgramDataFolder, RemoveFiles);
                else
                    Install(ManifestFile, ServerApplication, ProgramDataFolder);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Critical(ex);

                Console.Out.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                Log.Terminate();
            }
        }

        private static void Install(string ManifestFile, string ServerApplication, string ProgramDataFolder)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

            if (string.IsNullOrEmpty(ManifestFile))
                throw new Exception("Missing manifest file.");

            if (string.IsNullOrEmpty(ServerApplication))
                throw new Exception("Missing server application.");

            if (string.IsNullOrEmpty(ProgramDataFolder))
            {
                ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
                Log.Informational("Using default program data folder: " + ProgramDataFolder);
            }

            if (!File.Exists(ServerApplication))
                throw new Exception("Server application not found: " + ServerApplication);

            Log.Informational("Getting assembly name of server.");
            AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);
            Log.Informational("Server assembly name: " + ServerName.ToString());

            string DepsJsonFileName;

            int i = ServerApplication.LastIndexOf('.');
            if (i < 0)
                DepsJsonFileName = ServerApplication;
            else
                DepsJsonFileName = ServerApplication.Substring(0, i);

            DepsJsonFileName += ".deps.json";

            Log.Informational("deps.json file name: " + DepsJsonFileName);

            if (!File.Exists(DepsJsonFileName))
                throw new Exception("Invalid server executable. No corresponding deps.json file found.");

            Log.Informational("Opening " + DepsJsonFileName);

            string s = File.ReadAllText(DepsJsonFileName);

            Log.Informational("Parsing " + DepsJsonFileName);

            Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
            if (Deps is null)
                throw new Exception("Invalid deps.json file. Unable to install.");

            Log.Informational("Loading manifest file.");

            XmlDocument Manifest = new XmlDocument();
            Manifest.Load(ManifestFile);

            Log.Informational("Validating manifest file.");

            XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
            XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

            XmlElement Module = Manifest["Module"];
            string SourceFolder = Path.GetDirectoryName(ManifestFile);
            string AppFolder = Path.GetDirectoryName(ServerApplication);
            string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

            CopyFileIfNewer(ManifestFile, DestManifestFileName, null, false);

            Log.Informational("Source folder: " + SourceFolder);
            Log.Informational("App folder: " + AppFolder);

            foreach (XmlNode N in Module.ChildNodes)
            {
                if (N is XmlElement E && E.LocalName == "Assembly")
                {
                    (string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

                    if (CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), null, true))
                    {
                        if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                            if (File.Exists(PdbFileName))
                                CopyFileIfNewer(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, false);
                        }
                    }

                    Assembly A = Assembly.LoadFrom(SourceFileName);
                    AssemblyName AN = A.GetName();

                    if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
                    {
                        foreach (KeyValuePair<string, object> P in Targets)
                        {
                            if (P.Value is Dictionary<string, object> Target)
                            {
                                foreach (KeyValuePair<string, object> P2 in Target)
                                {
                                    if (P2.Key.StartsWith(ServerName.Name + "/") &&
                                        P2.Value is Dictionary<string, object> App &&
                                        App.TryGetValue("dependencies", out object Obj2) &&
                                        Obj2 is Dictionary<string, object> Dependencies)
                                    {
                                        Dependencies[AN.Name] = AN.Version.ToString();
                                        break;
                                    }
                                }

                                Dictionary<string, object> Dependencies2 = new Dictionary<string, object>();

                                foreach (AssemblyName Dependency in A.GetReferencedAssemblies())
                                    Dependencies2[Dependency.Name] = Dependency.Version.ToString();

                                Dictionary<string, object> Runtime = new Dictionary<string, object>()
                                    {
                                        { Path.GetFileName(SourceFileName), new Dictionary<string,object>() }
                                    };

                                Target[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
                                    {
                                        { "dependencies", Dependencies2 },
                                        { "runtime", Runtime }
                                    };
                            }
                        }
                    }

                    if (Deps != null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
                    {
                        foreach (KeyValuePair<string, object> P in Libraries)
                        {
                            if (P.Key.StartsWith(AN.Name + "/"))
                            {
                                Libraries.Remove(P.Key);
                                break;
                            }
                        }

                        Libraries[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
                            {
                                { "type", "project" },
                                { "serviceable", false },
                                { "sha512", string.Empty }
                            };
                    }

                }
            }

            CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module);

            Log.Informational("Encoding JSON");
            s = JSON.Encode(Deps, true);

            Log.Informational("Writing " + DepsJsonFileName);
            File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
        }

        private static bool CopyFileIfNewer(string From, string To, string To2, bool OnlyIfNewer)
        {
            if (!File.Exists(From))
                throw new Exception("File not found: " + From);

            bool Copy1 = From != To;

            if (Copy1 && OnlyIfNewer && File.Exists(To))
            {
                DateTime ToTP = File.GetLastWriteTimeUtc(To);
                DateTime FromTP = File.GetLastWriteTimeUtc(From);

                if (ToTP >= FromTP)
                {
                    Log.Warning("Skipping file. Destination folder contains newer version: " + From,
                        new KeyValuePair<string, object>("FromTP", FromTP),
                        new KeyValuePair<string, object>("ToTP", ToTP),
                        new KeyValuePair<string, object>("From", From),
                        new KeyValuePair<string, object>("To", To));
                    Copy1 = false;
                }
            }

            if (Copy1)
            {
                Log.Informational("Copying " + From + " to " + To);
                File.Copy(From, To, true);
            }

            if (!string.IsNullOrEmpty(To2))
            {
                bool Copy2 = From != To2;

                if (Copy2 && OnlyIfNewer && File.Exists(To2))
                {
                    DateTime ToTP = File.GetLastWriteTimeUtc(To2);
                    DateTime FromTP = File.GetLastWriteTimeUtc(From);

                    if (ToTP >= FromTP)
                    {
                        Log.Warning("Skipping file. Destination folder contains newer version: " + From,
                            new KeyValuePair<string, object>("FromTP", FromTP),
                            new KeyValuePair<string, object>("ToTP", ToTP),
                            new KeyValuePair<string, object>("From", From),
                            new KeyValuePair<string, object>("To", To2));
                        Copy2 = false;
                    }
                }

                if (Copy2)
                {
                    Log.Informational("Copying " + From + " to " + To2);
                    File.Copy(From, To2, true);
                }
            }

            return true;
        }

        private enum CopyOptions
        {
            IfNewer = 2,
            Always = 3
        }

        private static void CopyContent(string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent)
        {
            foreach (XmlNode N in Parent.ChildNodes)
            {
                if (N is XmlElement E)
                {
                    switch (E.LocalName)
                    {
                        case "Content":
                            (string FileName, string SourceFileName) = GetFileName(E, SourceFolder);
                            CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

                            Log.Informational("Content file: " + FileName);

                            if (!string.IsNullOrEmpty(DataFolder) && !Directory.Exists(DataFolder))
                            {
                                Log.Informational("Creating folder " + DataFolder + ".");
                                Directory.CreateDirectory(DataFolder);
                            }

                            if (!string.IsNullOrEmpty(AppFolder) && !Directory.Exists(AppFolder))
                            {
                                Log.Informational("Creating folder " + AppFolder + ".");
                                Directory.CreateDirectory(AppFolder);
                            }

                            CopyFileIfNewer(SourceFileName,
                                Path.Combine(DataFolder, FileName),
                                Path.Combine(AppFolder, FileName),
                                CopyOptions == CopyOptions.IfNewer);
                            break;

                        case "Folder":
                            string Name = XML.Attribute(E, "name");

                            string SourceFolder2 = Path.Combine(SourceFolder, Name);
                            string AppFolder2 = Path.Combine(AppFolder, Name);
                            string DataFolder2 = Path.Combine(DataFolder, Name);

                            Log.Informational("Folder: " + Name,
                                new KeyValuePair<string, object>("Source", SourceFolder2),
                                new KeyValuePair<string, object>("App", AppFolder2),
                                new KeyValuePair<string, object>("Data", DataFolder2));

                            CopyContent(SourceFolder2, AppFolder2, DataFolder2, E);
                            break;
                    }
                }
            }
        }

        private static void Uninstall(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool Remove)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

            if (string.IsNullOrEmpty(ManifestFile))
                throw new Exception("Missing manifest file.");

            if (string.IsNullOrEmpty(ServerApplication))
                throw new Exception("Missing server application.");

            if (string.IsNullOrEmpty(ProgramDataFolder))
            {
                ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
                Log.Informational("Using default program data folder: " + ProgramDataFolder);
            }

            if (!File.Exists(ServerApplication))
                throw new Exception("Server application not found: " + ServerApplication);

            Log.Informational("Getting assembly name of server.");
            AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);
            Log.Informational("Server assembly name: " + ServerName.ToString());

            string DepsJsonFileName;

            int i = ServerApplication.LastIndexOf('.');
            if (i < 0)
                DepsJsonFileName = ServerApplication;
            else
                DepsJsonFileName = ServerApplication.Substring(0, i);

            DepsJsonFileName += ".deps.json";

            Log.Informational("deps.json file name: " + DepsJsonFileName);

            if (!File.Exists(DepsJsonFileName))
                throw new Exception("Invalid server executable. No corresponding deps.json file found.");

            Log.Informational("Opening " + DepsJsonFileName);

            string s = File.ReadAllText(DepsJsonFileName);

            Log.Informational("Parsing " + DepsJsonFileName);

            Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
            if (Deps is null)
                throw new Exception("Invalid deps.json file. Unable to install.");

            Log.Informational("Loading manifest file.");

            XmlDocument Manifest = new XmlDocument();
            Manifest.Load(ManifestFile);

            Log.Informational("Validating manifest file.");

            XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
            XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

            XmlElement Module = Manifest["Module"];
            string AppFolder = Path.GetDirectoryName(ServerApplication);

            Log.Informational("App folder: " + AppFolder);

            foreach (XmlNode N in Module.ChildNodes)
            {
                if (N is XmlElement E && E.LocalName == "Assembly")
                {
                    (string FileName, string AppFileName) = GetFileName(E, AppFolder);

                    Assembly A = Assembly.LoadFrom(AppFileName);
                    AssemblyName AN = A.GetName();
                    string Key = AN.Name + "/" + AN.Version.ToString();

                    if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
                    {
                        Targets.Remove(Key);

                        foreach (KeyValuePair<string, object> P in Targets)
                        {
                            if (P.Value is Dictionary<string, object> Target)
                            {
                                foreach (KeyValuePair<string, object> P2 in Target)
                                {
                                    if (P2.Key.StartsWith(ServerName.Name + "/") &&
                                        P2.Value is Dictionary<string, object> App &&
                                        App.TryGetValue("dependencies", out object Obj2) &&
                                        Obj2 is Dictionary<string, object> Dependencies)
                                    {
                                        Dependencies.Remove(AN.Name);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (Deps != null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
                    {
                        foreach (KeyValuePair<string, object> P in Libraries)
                        {
                            if (P.Key.StartsWith(AN.Name + "/"))
                            {
                                Libraries.Remove(P.Key);
                                break;
                            }
                        }
                    }

                    if (Remove)
                    {
                        RemoveFile(AppFileName);
                        if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                            RemoveFile(PdbFileName);
                        }
                    }
                }
            }

            Log.Informational("Encoding JSON");
            s = JSON.Encode(Deps, true);

            Log.Informational("Writing " + DepsJsonFileName);
            File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);

            if (Path.GetDirectoryName(ManifestFile) == AppFolder)
                RemoveFile(ManifestFile);
        }

        private static bool RemoveFile(string FileName)
        {
            if (!File.Exists(FileName))
                return false;

            Log.Informational("Deleting " + FileName);
            File.Delete(FileName);

            return true;
        }

        private static void GeneratePackage(string ManifestFile, string PackageFile, string Key)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

            if (string.IsNullOrEmpty(ManifestFile))
                throw new Exception("Missing manifest file.");

            if (string.IsNullOrEmpty(PackageFile))
                throw new Exception("Missing package file.");

            Log.Informational("Loading manifest file.");

            XmlDocument Manifest = new XmlDocument();
            Manifest.Load(ManifestFile);

            Log.Informational("Validating manifest file.");

            XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
            XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

            XmlElement Module = Manifest["Module"];
            string SourceFolder = Path.GetDirectoryName(ManifestFile);

            Log.Informational("Source folder: " + SourceFolder);

            string LocalName = Path.GetFileName(PackageFile);
            string PackageFolder = Path.GetDirectoryName(PackageFile);
            if (string.IsNullOrEmpty(PackageFolder))
                PackageFolder = Directory.GetCurrentDirectory();

            SHAKE256 H = new SHAKE256(384);
            byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
            byte[] AesKey = new byte[32];
            byte[] IV = new byte[16];
            Aes Aes = null;
            FileStream fs = null;
            ICryptoTransform AesTransform = null;
            CryptoStream Encrypted = null;
            GZipStream Compressed = null;

            Array.Copy(Digest, 0, AesKey, 0, 32);
            Array.Copy(Digest, 32, IV, 0, 16);

            try
            {
                Aes = Aes.Create();
                Aes.BlockSize = 128;
                Aes.KeySize = 256;
                Aes.Mode = CipherMode.CBC;
                Aes.Padding = PaddingMode.ISO10126;

                fs = File.Create(PackageFile);
                AesTransform = Aes.CreateEncryptor(AesKey, IV);
                Encrypted = new CryptoStream(fs, AesTransform, CryptoStreamMode.Write);
                Compressed = new GZipStream(Encrypted, CompressionLevel.Optimal, false);

                using (RandomNumberGenerator rnd = RandomNumberGenerator.Create())
                {
                    byte[] Bin = new byte[1];

                    rnd.GetBytes(Bin);
                    Compressed.Write(Bin, 0, 1);

                    Bin = new byte[Bin[0]];

                    rnd.GetBytes(Bin);
                    Compressed.Write(Bin, 0, Bin.Length);
                }

                foreach (XmlNode N in Module.ChildNodes)
                {
                    if (N is XmlElement E && E.LocalName == "Assembly")
                    {
                        (string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

                        CopyFile(1, SourceFileName, Path.GetRelativePath(PackageFolder, SourceFileName), Compressed);

                        if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                            if (File.Exists(PdbFileName))
                                CopyFile(1, PdbFileName, Path.GetRelativePath(PackageFolder, PdbFileName), Compressed);
                        }
                    }
                }

                CopyContent(SourceFolder, Compressed, PackageFolder, Module);

                Compressed.WriteByte(0);
            }
            finally
            {
                Compressed?.Flush();
                Encrypted?.Flush();
                fs?.Flush();

                Compressed?.Dispose();
                Encrypted?.Dispose();
                AesTransform?.Dispose();
                Aes?.Dispose();
                fs?.Dispose();
            }
        }

        private static void CopyFile(byte Type, string SourceFileName, string RelativeName, Stream Output)
        {
            Output.WriteByte(Type);

            WriteBin(Encoding.UTF8.GetBytes(RelativeName), Output);

            using (FileStream f = File.OpenRead(SourceFileName))
            {
                WriteVarLenUInt((ulong)f.Length, Output);
                f.CopyTo(Output);
            }
        }

        private static void WriteBin(byte[] Bin, Stream Output)
        {
            WriteVarLenUInt((uint)Bin.Length, Output);
            Output.Write(Bin, 0, Bin.Length);
        }

        private static void WriteVarLenUInt(ulong Len, Stream Output)
        {
            byte b;

            do
            {
                b = (byte)(Len & 127);
                Len >>= 7;
                if (Len > 0)
                    b |= 0x80;

                Output.WriteByte(b);
            }
            while (Len > 0);
        }

        private static void CopyContent(string SourceFolder, Stream Output, string PackageFolder, XmlElement Parent)
        {
            foreach (XmlNode N in Parent.ChildNodes)
            {
                if (N is XmlElement E)
                {
                    switch (E.LocalName)
                    {
                        case "Content":
                            (string FileName, string SourceFileName) = GetFileName(E, SourceFolder);
                            CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

                            Log.Informational("Content file: " + FileName);

                            CopyFile((byte)CopyOptions, SourceFileName, Path.GetRelativePath(PackageFolder, SourceFileName), Output);
                            break;

                        case "Folder":
                            string Name = XML.Attribute(E, "name");

                            string SourceFolder2 = Path.Combine(SourceFolder, Name);

                            Log.Informational("Folder: " + Name,
                                new KeyValuePair<string, object>("Source", SourceFolder2));

                            CopyContent(SourceFolder2, Output, PackageFolder, E);
                            break;
                    }
                }
            }
        }

        private static (string, string) GetFileName(XmlElement E, string ReferenceFolder)
        {
            string FileName = XML.Attribute(E, "fileName");
            string AbsFileName = Path.Combine(ReferenceFolder, FileName);
            if (File.Exists(AbsFileName))
                return (FileName, AbsFileName);

            string AltFolder = XML.Attribute(E, "altFolder");
            if (string.IsNullOrEmpty(AltFolder))
                throw new Exception("File not found: " + AbsFileName);

            AbsFileName = Path.Combine(AltFolder, FileName);
            if (File.Exists(AbsFileName))
                return (FileName, AbsFileName);

            throw new Exception("File not found: " + AbsFileName);
        }

    }
}
