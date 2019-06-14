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
    /// Installs a module in an IoT Gateway, or generates a module package file.
    /// 
    /// Command line switches:
    /// 
    /// -m MANIFEST_FILE     Points to a manifest file describing the files in the module.
    /// -p PACKAGE_FILE      If provided together with a manifest file, files will be
    ///                      packed into a package file that can be easily distributed,
    ///                      instead of being installed on the local machine.
    ///                      If a manifest file is not specified, the package file will
    ///                      be used instead.
    /// -d APP_DATA_FOLDER   Points to the application data folder. Required if
    ///                      installing a module.
    /// -s SERVER_EXE        Points to the executable file of the IoT Gateway. Required
    ///                      if installing a module.
    /// -k KEY               Encryption key used for the package file. Secret used in
    ///                      encryption is based on the local package file name and the
    ///                      KEY parameter, if provided. You cannot rename a package file.
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
                    Console.Out.WriteLine("-m MANIFEST_FILE     Points to a manifest file describing the files in the module.");
                    Console.Out.WriteLine("-p PACKAGE_FILE      If provided together with a manifest file, files will be");
                    Console.Out.WriteLine("                     packed into a package file that can be easily distributed,");
                    Console.Out.WriteLine("                     instead of being installed on the local machine.");
                    Console.Out.WriteLine("                     If a manifest file is not specified, the package file will");
                    Console.Out.WriteLine("                     be used instead.");
                    Console.Out.WriteLine("-d APP_DATA_FOLDER   Points to the application data folder. Required if");
                    Console.Out.WriteLine("                     installing a module.");
                    Console.Out.WriteLine("-s SERVER_EXE        Points to the executable file of the IoT Gateway. Required");
                    Console.Out.WriteLine("                     if installing a module.");
                    Console.Out.WriteLine("-k KEY               Encryption key used for the package file. Secret used in");
                    Console.Out.WriteLine("                     encryption is based on the local package file name and the");
                    Console.Out.WriteLine("                     KEY parameter, if provided. You cannot rename a package file.");
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
                {
                    if (string.IsNullOrEmpty(ManifestFile))
                    {
                        if (UninstallService)
                            UninstallPackage(PackageFile, Key, ServerApplication, ProgramDataFolder, RemoveFiles);
                        else
                            InstallPackage(PackageFile, Key, ServerApplication, ProgramDataFolder);
                    }
                    else
                        GeneratePackage(ManifestFile, PackageFile, Key);
                }
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
                                CopyFileIfNewer(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, true);
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
            // Same code as for custom action UninstallManifest in Waher.IoTGateway.Installers

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

                    WriteBin(Encoding.ASCII.GetBytes("IoTGatewayPackage"), Compressed);
                }

                CopyFile(1, ManifestFile, Path.GetFileName(ManifestFile), Compressed);

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

            WriteVarLenUInt((uint)File.GetAttributes(SourceFileName), Output);
            WriteVarLenUInt((ulong)File.GetCreationTimeUtc(SourceFileName).Ticks, Output);
            WriteVarLenUInt((ulong)File.GetLastAccessTimeUtc(SourceFileName).Ticks, Output);
            WriteVarLenUInt((ulong)File.GetLastWriteTimeUtc(SourceFileName).Ticks, Output);

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

        private static byte[] ReadBin(Stream Input)
        {
            ulong Len = ReadVarLenUInt(Input);
            if (Len > int.MaxValue)
                throw new Exception("Invalid package.");

            int c = (int)Len;
            byte[] Result = new byte[c];
            Input.Read(Result, 0, c);

            return Result;
        }

        private static ulong ReadVarLenUInt(Stream Input)
        {
            ulong Len = 0;
            int Offset = 0;
            byte b;

            do
            {
                Len <<= 7;
                b = (byte)Input.ReadByte();

                Len |= ((ulong)(b & 127)) << Offset;
                Offset += 7;
            }
            while ((b & 0x80) != 0);

            return Len;
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

        private static void InstallPackage(string PackageFile, string Key, string ServerApplication, string ProgramDataFolder)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

            if (string.IsNullOrEmpty(PackageFile))
                throw new Exception("Missing package file.");

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

            Log.Informational("Loading package file.");

            string LocalName = Path.GetFileName(PackageFile);
            SHAKE256 H = new SHAKE256(384);
            byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
            byte[] AesKey = new byte[32];
            byte[] IV = new byte[16];
            Aes Aes = null;
            FileStream fs = null;
            ICryptoTransform AesTransform = null;
            CryptoStream Decrypted = null;
            GZipStream Decompressed = null;

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
                AesTransform = Aes.CreateDecryptor(AesKey, IV);
                Decrypted = new CryptoStream(fs, AesTransform, CryptoStreamMode.Read);
                Decompressed = new GZipStream(Decrypted, CompressionMode.Decompress);

                byte b = (byte)Decompressed.ReadByte();
                byte[] Bin;

                if (b > 0)
                {
                    Bin = new byte[b];
                    Decompressed.Read(Bin, 0, b);
                }

                Bin = ReadBin(Decompressed);
                if (Encoding.ASCII.GetString(Bin) != "IoTGatewayPackage")
                    throw new Exception("Invalid package file.");

                string SourceFolder = Path.GetDirectoryName(PackageFile);
                string AppFolder = Path.GetDirectoryName(ServerApplication);

                Log.Informational("Source folder: " + SourceFolder);
                Log.Informational("App folder: " + AppFolder);

                while ((b = (byte)Decrypted.ReadByte()) != 0)
                {
                    string RelativeName = Encoding.UTF8.GetString(ReadBin(Decrypted));
                    FileAttributes Attr = (FileAttributes)ReadVarLenUInt(Decrypted);
                    DateTime CreationTimeUtc = new DateTime((long)ReadVarLenUInt(Decrypted));
                    DateTime LastAccessTimeUtc = new DateTime((long)ReadVarLenUInt(Decrypted));
                    DateTime LastWriteTimeUtc = new DateTime((long)ReadVarLenUInt(Decrypted));
                    ulong Bytes = ReadVarLenUInt(Decrypted);
                    string FileName;

                    switch (b)
                    {
                        case 1: // Assembly file
                            FileName = Path.Combine(AppFolder, RelativeName);
                            CopyFile(Decrypted, FileName, false, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc);

                            Assembly A = Assembly.LoadFrom(FileName);
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
                                            { Path.GetFileName(FileName), new Dictionary<string,object>() }
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

                            break;

                        case 2: // Content file (copy if newer)
                        case 3: // Content file (always copy)
                            bool OnlyIfNewer = b == 2;
                            string TempFileName = Path.GetTempFileName();
                            CopyFile(Decrypted, TempFileName, false, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc);
                            try
                            {
                                using (FileStream TempFile = File.OpenRead(TempFileName))
                                {
                                    FileName = Path.Combine(ProgramDataFolder, RelativeName);
                                    CopyFile(TempFile, FileName, OnlyIfNewer, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc);

                                    FileName = Path.Combine(AppFolder, RelativeName);
                                    TempFile.Position = 0;
                                    CopyFile(TempFile, FileName, OnlyIfNewer, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc);
                                }
                            }
                            finally
                            {
                                File.Delete(TempFileName);
                            }
                            break;

                        default:
                            throw new Exception("Invalid package file.");
                    }
                }

                Log.Informational("Encoding JSON");
                s = JSON.Encode(Deps, true);

                Log.Informational("Writing " + DepsJsonFileName);
                File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
            }
            finally
            {
                Decompressed?.Dispose();
                Decrypted?.Dispose();
                AesTransform?.Dispose();
                Aes?.Dispose();
                fs?.Dispose();
            }
        }

        private static void CopyFile(Stream Input, string OutputFileName, bool OnlyIfNewer, ulong Bytes, FileAttributes Attr,
            DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc)
        {
            Log.Informational("Content file: " + OutputFileName);

            string Folder = Path.GetDirectoryName(OutputFileName);

            if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
            {
                Log.Informational("Creating folder " + Folder + ".");
                Directory.CreateDirectory(Folder);
            }

            int c = Math.Min(65536, Bytes > int.MaxValue ? int.MaxValue : (int)Bytes);
            byte[] Buffer = new byte[c];

            if (OnlyIfNewer && File.Exists(OutputFileName) && File.GetLastWriteTimeUtc(OutputFileName) >= LastWriteTimeUtc)
                return;

            using (FileStream f = File.Create(OutputFileName))
            {
                while (Bytes > 0)
                {
                    Input.Read(Buffer, 0, c);
                    f.Write(Buffer, 0, c);
                    Bytes -= (uint)c;
                }
            }

            File.SetAttributes(OutputFileName, Attr);
            File.SetCreationTimeUtc(OutputFileName, CreationTimeUtc);
            File.SetLastAccessTimeUtc(OutputFileName, LastAccessTimeUtc);
            File.SetLastWriteTimeUtc(OutputFileName, LastWriteTimeUtc);
        }

        private static void UninstallPackage(string PackageFile, string Key, string ServerApplication, string ProgramDataFolder, bool Remove)
        {
            // Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

            if (string.IsNullOrEmpty(PackageFile))
                throw new Exception("Missing package file.");

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

            Log.Informational("Loading package file.");

            string LocalName = Path.GetFileName(PackageFile);
            SHAKE256 H = new SHAKE256(384);
            byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
            byte[] AesKey = new byte[32];
            byte[] IV = new byte[16];
            Aes Aes = null;
            FileStream fs = null;
            ICryptoTransform AesTransform = null;
            CryptoStream Decrypted = null;
            GZipStream Decompressed = null;

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
                AesTransform = Aes.CreateDecryptor(AesKey, IV);
                Decrypted = new CryptoStream(fs, AesTransform, CryptoStreamMode.Read);
                Decompressed = new GZipStream(Decrypted, CompressionMode.Decompress);

                byte b = (byte)Decompressed.ReadByte();
                byte[] Bin;

                if (b > 0)
                {
                    Bin = new byte[b];
                    Decompressed.Read(Bin, 0, b);
                }

                Bin = ReadBin(Decompressed);
                if (Encoding.ASCII.GetString(Bin) != "IoTGatewayPackage")
                    throw new Exception("Invalid package file.");

                string AppFolder = Path.GetDirectoryName(ServerApplication);

                Log.Informational("App folder: " + AppFolder);


                while ((b = (byte)Decrypted.ReadByte()) != 0)
                {
                    string RelativeName = Encoding.UTF8.GetString(ReadBin(Decrypted));
                    ReadVarLenUInt(Decrypted);
                    ReadVarLenUInt(Decrypted);
                    ReadVarLenUInt(Decrypted);
                    ReadVarLenUInt(Decrypted);
                    ulong Bytes = ReadVarLenUInt(Decrypted);
                    string FileName;

                    int c = Math.Min(65536, Bytes > int.MaxValue ? int.MaxValue : (int)Bytes);
                    byte[] Buffer = new byte[c];

                    while (Bytes > 0)
                    {
                        Decrypted.Read(Buffer, 0, c);
                        Bytes -= (uint)c;
                    }

                    switch (b)
                    {
                        case 1: // Assembly file
                            FileName = Path.Combine(AppFolder, RelativeName);

                            Assembly A = Assembly.LoadFrom(FileName);
                            AssemblyName AN = A.GetName();
                            Key = AN.Name + "/" + AN.Version.ToString();

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
                                RemoveFile(FileName);
                                if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
                                    RemoveFile(PdbFileName);
                                }
                            }
                            break;

                        case 2: // Content file (copy if newer)
                        case 3: // Content file (always copy)
                            break;

                        default:
                            throw new Exception("Invalid package file.");
                    }
                }

            }
            finally
            {
                Decompressed?.Dispose();
                Decrypted?.Dispose();
                AesTransform?.Dispose();
                Aes?.Dispose();
                fs?.Dispose();
            }

            Log.Informational("Encoding JSON");
            s = JSON.Encode(Deps, true);

            Log.Informational("Writing " + DepsJsonFileName);
            File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
        }

    }
}
