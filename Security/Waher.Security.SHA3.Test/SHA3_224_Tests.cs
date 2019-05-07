using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Test vectors available in:
    /// https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHA3-224_Msg0.pdf
    /// </summary>
    [TestClass]
    public class SHA3_224_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHA3_224 H = new SHA3_224();
            int i = 0;

            H.NewState += (sender, e) =>
            {
                string Expected = States[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable(new byte[0]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7", s);
        }

        private static readonly string[] States = new string[]
        {
            // Xor'd state (in bytes)
            "06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // Round #0, After Theta
            "06 00 00 00 00 00 00 00 07 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "0C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 0C 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 07 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "0C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 80 0C 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 07 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "0C 00 00 00 00 00 00 00",
            // After Rho
            "06 00 00 00 00 00 00 00 0E 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 08 00 00 00 00 " +
            "00 00 00 60 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 70 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 40 00 00 00 C0 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 1C 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 " +
            "00 00 00 00 00 06 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 E0 00 00 00 40 00 00 00 00 00 00 " +
            "00 00 10 00 00 00 00 00 00 0C 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 1C 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 00 " +
            "00 00 03 00 00 00 00 00",
            // After Pi
            "06 00 00 00 00 00 00 00 00 00 00 00 00 70 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 10 00 00 00 00 00 " +
            "00 00 03 00 00 00 00 00 00 00 00 08 00 00 00 00 " +
            "00 00 C0 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 E0 00 00 00 00 00 00 00 00 00 00 " +
            "0E 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 01 00 00 00 00 00 0C 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 60 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 1C 00 00 00 00 00 00 " +
            "00 40 00 00 00 00 00 00 00 00 00 00 00 00 80 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 40 00 " +
            "00 00 00 00 00 06 00 00 00 00 00 00 00 00 00 00 " +
            "1C 00 00 00 00 00 00 00",
            // After Chi
            "06 00 00 00 00 00 00 00 00 00 10 00 00 70 00 00 " +
            "00 00 03 00 00 00 00 00 06 00 10 00 00 00 00 00 " +
            "00 00 03 00 00 70 00 00 00 00 00 08 00 00 00 00 " +
            "00 00 C0 00 00 E0 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 08 00 E0 00 00 00 00 C0 00 00 00 00 00 " +
            "0E 00 00 01 00 00 00 00 00 0C 00 00 00 00 00 00 " +
            "00 00 00 01 00 00 00 00 0E 0C 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 1C 00 60 00 00 00 00 " +
            "00 40 00 00 00 00 00 00 00 1C 00 00 00 00 80 00 " +
            "00 40 00 60 00 00 00 00 00 00 00 00 00 00 80 00 " +
            "00 00 00 00 00 06 00 00 00 00 00 00 00 00 40 00 " +
            "1C 00 00 00 00 06 00 00 00 00 00 00 00 00 00 00 " +
            "1C 00 00 00 00 00 40 00",
            // After Iota
            "07 00 00 00 00 00 00 00 00 00 10 00 00 70 00 00 " +
            "00 00 03 00 00 00 00 00 06 00 10 00 00 00 00 00 " +
            "00 00 03 00 00 70 00 00 00 00 00 08 00 00 00 00 " +
            "00 00 C0 00 00 E0 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 08 00 E0 00 00 00 00 C0 00 00 00 00 00 " +
            "0E 00 00 01 00 00 00 00 00 0C 00 00 00 00 00 00 " +
            "00 00 00 01 00 00 00 00 0E 0C 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 1C 00 60 00 00 00 00 " +
            "00 40 00 00 00 00 00 00 00 1C 00 00 00 00 80 00 " +
            "00 40 00 60 00 00 00 00 00 00 00 00 00 00 80 00 " +
            "00 00 00 00 00 06 00 00 00 00 00 00 00 00 40 00 " +
            "1C 00 00 00 00 06 00 00 00 00 00 00 00 00 00 00 " +
            "1C 00 00 00 00 00 40 00",
            // Round #1, After Theta
            "1B 98 63 01 00 50 41 00 31 24 16 6B 00 7A 00 01 " +
            "10 D4 F3 D0 00 50 41 00 22 1C 95 00 00 E6 00 01 " +
            "1A 74 13 BA 00 9C 00 00 1C 98 63 09 00 50 41 00 " +
            "31 24 C6 6B 00 EA 00 01 10 D4 F0 D0 00 50 41 00 " +
            "24 1C 85 08 00 06 00 01 1A 74 D0 BA 00 EC 00 00 " +
            "12 98 63 00 00 50 41 00 31 28 06 6B 00 0A 00 01 " +
            "10 D4 F0 D1 00 50 41 00 2A 10 85 00 00 E6 00 01 " +
            "1A 74 10 BA 00 EC 00 00 1C 84 63 61 00 50 41 00 " +
            "31 64 06 6B 00 0A 00 01 10 C8 F0 D0 00 50 C1 00 " +
            "24 5C 85 60 00 E6 00 01 1A 74 10 BA 00 EC 80 00 " +
            "1C 98 63 01 00 56 41 00 31 24 06 6B 00 0A 40 01 " +
            "0C D4 F0 D0 00 56 41 00 24 1C 85 00 00 E6 00 01 " +
            "06 74 10 BA 00 EC 40 00",
            // After Rho
            "1B 98 63 01 00 50 41 00 62 48 2C D6 00 F4 00 02 " +
            "04 F5 3C 34 00 54 10 00 60 0E 10 20 C2 51 09 00 " +
            "E0 04 00 D0 A0 9B D0 05 00 00 15 04 C0 81 39 96 " +
            "BC 06 A0 0E 10 10 43 62 00 04 35 3C 34 00 54 10 " +
            "8E 42 04 00 03 80 00 12 0E 00 A0 41 07 AD 0B C0 " +
            "90 C0 1C 03 00 80 0A 02 04 C4 A0 18 AC 01 28 00 " +
            "8F 06 80 0A 02 80 A0 86 CC 01 02 54 20 0A 01 00 " +
            "5D 00 76 00 00 0D 3A 08 C2 00 A0 82 00 38 08 C7 " +
            "60 0D 40 01 20 20 86 CC 60 00 08 64 78 68 00 A8 " +
            "1C 20 80 84 AB 10 0C C0 00 1A 74 10 BA 00 EC 80 " +
            "05 01 70 60 8E 05 00 58 C4 90 18 AC 01 28 00 05 " +
            "81 1A 1E 1A C0 2A 08 80 1C 85 00 00 E6 00 01 24 " +
            "10 80 01 1D 84 2E 00 3B", 
            // After Pi
            "1B 98 63 01 00 50 41 00 BC 06 A0 0E 10 10 43 62 " +
            "8F 06 80 0A 02 80 A0 86 1C 20 80 84 AB 10 0C C0 " +
            "10 80 01 1D 84 2E 00 3B 60 0E 10 20 C2 51 09 00 " +
            "0E 00 A0 41 07 AD 0B C0 90 C0 1C 03 00 80 0A 02 " +
            "60 0D 40 01 20 20 86 CC 81 1A 1E 1A C0 2A 08 80 " +
            "62 48 2C D6 00 F4 00 02 00 04 35 3C 34 00 54 10 " +
            "CC 01 02 54 20 0A 01 00 00 1A 74 10 BA 00 EC 80 " +
            "05 01 70 60 8E 05 00 58 E0 04 00 D0 A0 9B D0 05 " +
            "00 00 15 04 C0 81 39 96 04 C4 A0 18 AC 01 28 00 " +
            "60 00 08 64 78 68 00 A8 1C 85 00 00 E6 00 01 24 " +
            "04 F5 3C 34 00 54 10 00 8E 42 04 00 03 80 00 12 " +
            "5D 00 76 00 00 0D 3A 08 C2 00 A0 82 00 38 08 C7 " +
            "C4 90 18 AC 01 28 00 05", 
            // After Chi
            "18 98 63 01 02 D0 E1 84 AC 26 A0 8A B9 00 4F 22 " +
            "8F 86 81 13 06 AE A0 BD 17 38 E2 84 AB 40 4D C0 " +
            "B4 86 81 13 94 2E 02 59 F0 CE 0C 22 C2 51 09 02 " +
            "6E 0D E0 41 27 8D 8F 0C 11 D2 02 19 C0 8A 02 02 " +
            "00 09 40 21 22 71 87 CC 8F 1A BE 5B C5 86 0A 40 " +
            "AE 49 2E 96 00 FE 01 02 00 1E 41 3C AE 00 B8 90 " +
            "C9 00 02 34 24 0F 01 58 62 52 78 86 BA F0 EC 82 " +
            "05 05 61 48 BA 05 54 48 E4 C0 A0 C8 8C 9B D0 05 " +
            "60 00 1D 60 90 E9 39 3E 18 41 A0 18 2A 01 29 04 " +
            "80 00 08 B4 78 F3 D0 A9 1C 85 15 04 A6 00 28 B6 " +
            "55 F5 4E 34 00 59 2A 08 0C 42 84 82 03 B0 00 D5 " +
            "59 90 6E 2C 01 0D 3A 08 C2 65 84 92 00 6C 18 C7 " +
            "4E 92 18 AC 02 A8 00 17", 
            // After Iota
            "9A 18 63 01 02 D0 E1 84 AC 26 A0 8A B9 00 4F 22 " +
            "8F 86 81 13 06 AE A0 BD 17 38 E2 84 AB 40 4D C0 " +
            "B4 86 81 13 94 2E 02 59 F0 CE 0C 22 C2 51 09 02 " +
            "6E 0D E0 41 27 8D 8F 0C 11 D2 02 19 C0 8A 02 02 " +
            "00 09 40 21 22 71 87 CC 8F 1A BE 5B C5 86 0A 40 " +
            "AE 49 2E 96 00 FE 01 02 00 1E 41 3C AE 00 B8 90 " +
            "C9 00 02 34 24 0F 01 58 62 52 78 86 BA F0 EC 82 " +
            "05 05 61 48 BA 05 54 48 E4 C0 A0 C8 8C 9B D0 05 " +
            "60 00 1D 60 90 E9 39 3E 18 41 A0 18 2A 01 29 04 " +
            "80 00 08 B4 78 F3 D0 A9 1C 85 15 04 A6 00 28 B6 " +
            "55 F5 4E 34 00 59 2A 08 0C 42 84 82 03 B0 00 D5 " +
            "59 90 6E 2C 01 0D 3A 08 C2 65 84 92 00 6C 18 C7 " +
            "4E 92 18 AC 02 A8 00 17",
            // Round #2, After Theta
            "AA 79 00 82 0B 7C 16 DE F4 86 90 D7 67 F2 3C 7C " +
            "4E FD B5 0C 33 C6 3D 29 D8 A1 0A DE FD 6D 15 CB " +
            "68 D4 88 85 47 0A CB AB C0 AF 6F A1 CB FD FE 58 " +
            "36 AD D0 1C F9 7F FC 52 D0 A9 36 06 F5 E2 9F 96 " +
            "CF 90 A8 7B 74 5C DF C7 53 48 B7 CD 16 A2 C3 B2 " +
            "9E 28 4D 15 09 52 F6 58 58 BE 71 61 70 F2 CB CE " +
            "08 7B 36 2B 11 67 9C CC AD CB 90 DC EC DD B4 89 " +
            "D9 57 68 DE 69 21 9D BA D4 A1 C3 4B 85 37 27 5F " +
            "38 A0 2D 3D 4E 1B 4A 60 D9 3A 94 07 1F 69 B4 90 " +
            "4F 99 E0 EE 2E DE 88 A2 C0 D7 1C 92 75 24 E1 44 " +
            "65 94 2D B7 09 F5 DD 52 54 E2 B4 DF DD 42 73 8B " +
            "98 EB 5A 33 34 65 A7 9C 0D FC 6C C8 56 41 40 CC " +
            "92 C0 11 3A D1 8C C9 E5", 
            // After Rho
            "AA 79 00 82 0B 7C 16 DE E8 0D 21 AF CF E4 79 F8 " +
            "53 7F 2D C3 8C 71 4F 8A DF 56 B1 8C 1D AA E0 DD " +
            "52 58 5E 45 A3 46 2C 3C BA DC EF 8F 05 FC FA 16 " +
            "CD 91 FF C7 2F 65 D3 0A 25 74 AA 8D 41 BD F8 A7 " +
            "48 D4 3D 3A AE EF E3 67 3A 2C 3B 85 74 DB 6C 21 " +
            "F2 44 69 AA 48 90 B2 C7 3B 63 F9 C6 85 C1 C9 2F " +
            "59 89 38 E3 64 46 D8 B3 BB 69 13 5B 97 21 B9 D9 " +
            "EF B4 90 4E DD EC 2B 34 97 0A 6F 4E BE A8 43 87 " +
            "A5 C7 69 43 09 0C 07 B4 5A C8 6C 1D CA 83 8F 34 " +
            "1B 51 F4 29 13 DC DD C5 44 C0 D7 1C 92 75 24 E1 " +
            "77 4B 95 51 B6 DC 26 D4 52 89 D3 7E 77 0B CD 2D " +
            "73 5D 6B 86 A6 EC 94 13 FC 6C C8 56 41 40 CC 0D " +
            "72 B9 24 70 84 4E 34 63", 
            // After Pi
            "AA 79 00 82 0B 7C 16 DE CD 91 FF C7 2F 65 D3 0A " +
            "59 89 38 E3 64 46 D8 B3 1B 51 F4 29 13 DC DD C5 " +
            "72 B9 24 70 84 4E 34 63 DF 56 B1 8C 1D AA E0 DD " +
            "3A 2C 3B 85 74 DB 6C 21 F2 44 69 AA 48 90 B2 C7 " +
            "A5 C7 69 43 09 0C 07 B4 73 5D 6B 86 A6 EC 94 13 " +
            "E8 0D 21 AF CF E4 79 F8 25 74 AA 8D 41 BD F8 A7 " +
            "BB 69 13 5B 97 21 B9 D9 44 C0 D7 1C 92 75 24 E1 " +
            "77 4B 95 51 B6 DC 26 D4 52 58 5E 45 A3 46 2C 3C " +
            "BA DC EF 8F 05 FC FA 16 3B 63 F9 C6 85 C1 C9 2F " +
            "5A C8 6C 1D CA 83 8F 34 FC 6C C8 56 41 40 CC 0D " +
            "53 7F 2D C3 8C 71 4F 8A 48 D4 3D 3A AE EF E3 67 " +
            "EF B4 90 4E DD EC 2B 34 97 0A 6F 4E BE A8 43 87 " +
            "52 89 D3 7E 77 0B CD 2D", 
            // After Chi
            "BA 71 00 A2 4B 7E 1E 6F CF C1 3B CF 3C FD D6 4E " +
            "39 21 38 B3 E0 44 F8 91 93 11 F4 AB 18 EC DF 59 " +
            "37 39 DB 35 A0 4F F5 63 1F 16 F1 A6 15 AA 72 1B " +
            "3F AF 3B C4 75 D7 69 11 A0 5C 6B 2E EE 70 22 C4 " +
            "29 C5 F9 4B 10 0E 67 78 53 75 61 87 C6 BD 98 33 " +
            "72 04 30 FD 59 E4 78 A0 61 F4 6E 89 41 E9 FC 87 " +
            "88 62 13 1A B3 A9 BB CD CC C4 F7 B2 DB 55 7D C9 " +
            "72 3B 1F 51 B6 C5 A6 D3 53 7B 4E 05 23 47 2D 15 " +
            "FA 54 EB 96 4F FE FC 06 9F 47 79 84 84 81 89 26 " +
            "58 D8 7A 1C 68 85 AF 04 54 E8 69 DC 45 F8 1E 0F " +
            "F4 5F AD 87 DD 71 47 9A 58 DE 52 3A 8C EF A3 E4 " +
            "AF 35 00 7E 9C EF A7 1C 96 7C 43 CF 36 D8 41 05 " +
            "5A 09 C3 46 55 85 6D 48", 
            // After Iota
            "30 F1 00 A2 4B 7E 1E EF CF C1 3B CF 3C FD D6 4E " +
            "39 21 38 B3 E0 44 F8 91 93 11 F4 AB 18 EC DF 59 " +
            "37 39 DB 35 A0 4F F5 63 1F 16 F1 A6 15 AA 72 1B " +
            "3F AF 3B C4 75 D7 69 11 A0 5C 6B 2E EE 70 22 C4 " +
            "29 C5 F9 4B 10 0E 67 78 53 75 61 87 C6 BD 98 33 " +
            "72 04 30 FD 59 E4 78 A0 61 F4 6E 89 41 E9 FC 87 " +
            "88 62 13 1A B3 A9 BB CD CC C4 F7 B2 DB 55 7D C9 " +
            "72 3B 1F 51 B6 C5 A6 D3 53 7B 4E 05 23 47 2D 15 " +
            "FA 54 EB 96 4F FE FC 06 9F 47 79 84 84 81 89 26 " +
            "58 D8 7A 1C 68 85 AF 04 54 E8 69 DC 45 F8 1E 0F " +
            "F4 5F AD 87 DD 71 47 9A 58 DE 52 3A 8C EF A3 E4 " +
            "AF 35 00 7E 9C EF A7 1C 96 7C 43 CF 36 D8 41 05 " +
            "5A 09 C3 46 55 85 6D 48",
            // Round #3, After Theta
            "4E 47 A1 86 1D 91 9F 5F 76 DC 6B 4E 8F 1C 37 D1 " +
            "7B 58 68 9E 30 43 B3 79 83 50 D2 24 3D 8A E0 72 " +
            "7A 02 5D 42 DF A8 22 3C 61 A0 50 82 43 45 F3 AB " +
            "86 B2 6B 45 C6 36 88 8E E2 25 3B 03 3E 77 69 2C " +
            "39 84 DF C4 35 68 58 53 1E 4E E7 F0 B9 5A 4F 6C " +
            "0C B2 91 D9 0F 0B F9 10 D8 E9 3E 08 F2 08 1D 18 " +
            "CA 1B 43 37 63 AE F0 25 DC 85 D1 3D FE 33 42 E2 " +
            "3F 00 99 26 C9 22 71 8C 2D CD EF 21 75 A8 AC A5 " +
            "43 49 BB 17 FC 1F 1D 99 DD 3E 29 A9 54 86 C2 CE " +
            "48 99 5C 93 4D E3 90 2F 19 D3 EF AB 3A 1F C9 50 " +
            "8A E9 0C A3 8B 9E C6 2A E1 C3 02 BB 3F 0E 42 7B " +
            "ED 4C 50 53 4C E8 EC F4 86 3D 65 40 13 BE 7E 2E " +
            "17 32 45 31 2A 62 BA 17", 
            // After Rho
            "4E 47 A1 86 1D 91 9F 5F ED B8 D7 9C 1E 39 6E A2 " +
            "1E 16 9A 27 CC D0 6C DE A3 08 2E 37 08 25 4D D2 " +
            "46 15 E1 D1 13 E8 12 FA 38 54 34 BF 1A 06 0A 25 " +
            "56 64 6C 83 E8 68 28 BB 8B 78 C9 CE 80 CF 5D 1A " +
            "C2 6F E2 1A 34 AC A9 1C F5 C4 E6 E1 74 0E 9F AB " +
            "60 90 8D CC 7E 58 C8 87 60 60 A7 FB 20 C8 23 74 " +
            "BA 19 73 85 2F 51 DE 18 67 84 C4 B9 0B A3 7B FC " +
            "93 64 91 38 C6 1F 80 4C 43 EA 50 59 4B 5B 9A DF " +
            "F7 82 FF A3 23 73 28 69 61 E7 6E 9F 94 54 2A 43 " +
            "1C F2 05 29 93 6B B2 69 50 19 D3 EF AB 3A 1F C9 " +
            "1A AB 28 A6 33 8C 2E 7A 85 0F 0B EC FE 38 08 ED " +
            "9D 09 6A 8A 09 9D 9D BE 3D 65 40 13 BE 7E 2E 86 " +
            "EE C5 85 4C 51 8C 8A 98", 
            // After Pi
            "4E 47 A1 86 1D 91 9F 5F 56 64 6C 83 E8 68 28 BB " +
            "BA 19 73 85 2F 51 DE 18 1C F2 05 29 93 6B B2 69 " +
            "EE C5 85 4C 51 8C 8A 98 A3 08 2E 37 08 25 4D D2 " +
            "F5 C4 E6 E1 74 0E 9F AB 60 90 8D CC 7E 58 C8 87 " +
            "F7 82 FF A3 23 73 28 69 9D 09 6A 8A 09 9D 9D BE " +
            "ED B8 D7 9C 1E 39 6E A2 8B 78 C9 CE 80 CF 5D 1A " +
            "67 84 C4 B9 0B A3 7B FC 50 19 D3 EF AB 3A 1F C9 " +
            "1A AB 28 A6 33 8C 2E 7A 46 15 E1 D1 13 E8 12 FA " +
            "38 54 34 BF 1A 06 0A 25 60 60 A7 FB 20 C8 23 74 " +
            "61 E7 6E 9F 94 54 2A 43 3D 65 40 13 BE 7E 2E 86 " +
            "1E 16 9A 27 CC D0 6C DE C2 6F E2 1A 34 AC A9 1C " +
            "93 64 91 38 C6 1F 80 4C 43 EA 50 59 4B 5B 9A DF " +
            "85 0F 0B EC FE 38 08 ED", 
            // After Chi
            "E6 5E B2 82 1A 80 49 5F 52 86 68 AB 78 42 08 DA " +
            "58 1C F3 C1 6F D5 D6 88 1C F0 25 AB 9F 7A A7 2E " +
            "FE E5 C9 4D B1 E4 AA 38 A3 18 27 3B 02 75 0D D6 " +
            "62 C6 94 C2 75 2D BF C3 68 99 8D C4 76 D4 5D 11 " +
            "D5 82 FB 96 23 53 68 29 C9 CD AA 4A 7D 97 0F 97 " +
            "89 3C D3 AD 15 19 4C 46 9B 61 DA 88 20 D7 59 1B " +
            "6D 26 EC B9 1B 27 5B CE B5 09 04 F7 A7 0B 5F 49 " +
            "18 EB 20 E4 B3 4A 3F 62 06 35 62 91 33 20 33 AA " +
            "39 D3 7C BB 8E 12 02 26 7C 60 A7 FB 0A E2 27 F0 " +
            "23 F7 CF 5F 95 D4 3A 3B 05 25 54 3D B6 78 26 83 " +
            "0F 16 8B 07 0E C3 6C 9E 82 E5 A2 5B 3D EC B3 8F " +
            "17 61 9A 9C 72 3F 80 6C 59 FA C0 5A 4B 9B FE CD " +
            "45 66 6B F4 CE 14 89 ED", 
            // After Iota
            "E6 DE B2 02 1A 80 49 DF 52 86 68 AB 78 42 08 DA " +
            "58 1C F3 C1 6F D5 D6 88 1C F0 25 AB 9F 7A A7 2E " +
            "FE E5 C9 4D B1 E4 AA 38 A3 18 27 3B 02 75 0D D6 " +
            "62 C6 94 C2 75 2D BF C3 68 99 8D C4 76 D4 5D 11 " +
            "D5 82 FB 96 23 53 68 29 C9 CD AA 4A 7D 97 0F 97 " +
            "89 3C D3 AD 15 19 4C 46 9B 61 DA 88 20 D7 59 1B " +
            "6D 26 EC B9 1B 27 5B CE B5 09 04 F7 A7 0B 5F 49 " +
            "18 EB 20 E4 B3 4A 3F 62 06 35 62 91 33 20 33 AA " +
            "39 D3 7C BB 8E 12 02 26 7C 60 A7 FB 0A E2 27 F0 " +
            "23 F7 CF 5F 95 D4 3A 3B 05 25 54 3D B6 78 26 83 " +
            "0F 16 8B 07 0E C3 6C 9E 82 E5 A2 5B 3D EC B3 8F " +
            "17 61 9A 9C 72 3F 80 6C 59 FA C0 5A 4B 9B FE CD " +
            "45 66 6B F4 CE 14 89 ED",
            // Round #4, After Theta
            "A8 70 3E 2B 21 58 C2 2A FA 1B 98 1E BD BB B0 37 " +
            "45 E7 A1 5F 7A 48 21 53 F5 52 73 24 EB 2B BA A3 " +
            "72 20 43 87 14 97 50 76 ED B6 AB 12 39 AD 86 23 " +
            "CA 5B 64 77 B0 D4 07 2E 75 62 DF 5A 63 49 AA CA " +
            "3C 20 AD 19 57 02 75 A4 45 08 20 80 D8 E4 F5 D9 " +
            "C7 92 5F 84 2E C1 C7 B3 33 FC 2A 3D E5 2E E1 F6 " +
            "70 DD BE 27 0E BA AC 15 5C AB 52 78 D3 5A 42 C4 " +
            "94 2E AA 2E 16 39 C5 2C 48 9B EE B8 08 F8 B8 5F " +
            "91 4E 8C 0E 4B EB BA CB 61 9B F5 65 1F 7F D0 2B " +
            "CA 55 99 D0 E1 85 27 B6 89 E0 DE F7 13 0B DC CD " +
            "41 B8 07 2E 35 1B E7 6B 2A 78 52 EE F8 15 0B 62 " +
            "0A 9A C8 02 67 A2 77 B7 B0 58 96 D5 3F CA E3 40 " +
            "C9 A3 E1 3E 6B 67 73 A3", 
            // After Rho
            "A8 70 3E 2B 21 58 C2 2A F4 37 30 3D 7A 77 61 6F " +
            "D1 79 E8 97 1E 52 C8 54 BE A2 3B 5A 2F 35 47 B2 " +
            "B8 84 B2 93 03 19 3A A4 91 D3 6A 38 D2 6E BB 2A " +
            "76 07 4B 7D E0 A2 BC 45 72 9D D8 B7 D6 58 92 AA " +
            "90 D6 8C 2B 81 3A 52 1E 5E 9F 5D 84 00 02 88 4D " +
            "3D 96 FC 22 74 09 3E 9E DB CF F0 AB F4 94 BB 84 " +
            "3D 71 D0 65 AD 80 EB F6 B5 84 88 B9 56 A5 F0 A6 " +
            "17 8B 9C 62 16 4A 17 55 71 11 F0 71 BF 90 36 DD " +
            "D1 61 69 5D 77 39 D2 89 E8 95 B0 CD FA B2 8F 3F " +
            "F0 C4 56 B9 2A 13 3A BC CD 89 E0 DE F7 13 0B DC " +
            "9C AF 05 E1 1E B8 D4 6C A9 E0 49 B9 E3 57 2C 88 " +
            "41 13 59 E0 4C F4 EE 56 58 96 D5 3F CA E3 40 B0 " +
            "DC 68 F2 68 B8 CF DA D9", 
            // After Pi
            "A8 70 3E 2B 21 58 C2 2A 76 07 4B 7D E0 A2 BC 45 " +
            "3D 71 D0 65 AD 80 EB F6 F0 C4 56 B9 2A 13 3A BC " +
            "DC 68 F2 68 B8 CF DA D9 BE A2 3B 5A 2F 35 47 B2 " +
            "5E 9F 5D 84 00 02 88 4D 3D 96 FC 22 74 09 3E 9E " +
            "D1 61 69 5D 77 39 D2 89 41 13 59 E0 4C F4 EE 56 " +
            "F4 37 30 3D 7A 77 61 6F 72 9D D8 B7 D6 58 92 AA " +
            "B5 84 88 B9 56 A5 F0 A6 CD 89 E0 DE F7 13 0B DC " +
            "9C AF 05 E1 1E B8 D4 6C B8 84 B2 93 03 19 3A A4 " +
            "91 D3 6A 38 D2 6E BB 2A DB CF F0 AB F4 94 BB 84 " +
            "E8 95 B0 CD FA B2 8F 3F 58 96 D5 3F CA E3 40 B0 " +
            "D1 79 E8 97 1E 52 C8 54 90 D6 8C 2B 81 3A 52 1E " +
            "17 8B 9C 62 16 4A 17 55 71 11 F0 71 BF 90 36 DD " +
            "A9 E0 49 B9 E3 57 2C 88", 
            // After Chi
            "A1 00 AE 2B 2C 58 81 98 B6 83 4D E5 E2 B1 AC 4D " +
            "31 59 70 25 3D 4C 2B B7 D0 D4 5A BA 2B 03 3A 9E " +
            "8A 6F B3 3C 78 6D E6 9C 9F A2 9B 78 5B 3C 71 20 " +
            "9E FE 5C D9 03 32 48 4C 3D 84 EC 82 7C CD 12 C8 " +
            "6F C1 4B 47 54 38 D3 29 01 0E 1D 64 4C F6 66 1B " +
            "71 37 30 35 7A D2 01 6B 3A 94 B8 F1 77 4A 99 F2 " +
            "A5 A2 8D 98 5E 0D 24 86 AD 99 D0 C2 97 54 2A DF " +
            "9E 27 CD 63 9A B0 46 EC F2 88 22 10 27 89 3A 20 " +
            "B1 C3 6A 7C D8 4C BF 11 CB CD B5 99 F4 D5 FB 04 " +
            "48 95 92 4D FB AA B5 3B 59 C5 9D 17 1A 85 C1 BA " +
            "D6 70 F8 D7 08 12 CD 15 F0 C6 EC 3A 28 AA 72 96 " +
            "9F 6B 95 EA 56 0D 1F 55 21 08 50 77 A3 90 F6 89 " +
            "A9 66 4D 91 62 7F 3E 82", 
            // After Iota
            "2A 80 AE 2B 2C 58 81 98 B6 83 4D E5 E2 B1 AC 4D " +
            "31 59 70 25 3D 4C 2B B7 D0 D4 5A BA 2B 03 3A 9E " +
            "8A 6F B3 3C 78 6D E6 9C 9F A2 9B 78 5B 3C 71 20 " +
            "9E FE 5C D9 03 32 48 4C 3D 84 EC 82 7C CD 12 C8 " +
            "6F C1 4B 47 54 38 D3 29 01 0E 1D 64 4C F6 66 1B " +
            "71 37 30 35 7A D2 01 6B 3A 94 B8 F1 77 4A 99 F2 " +
            "A5 A2 8D 98 5E 0D 24 86 AD 99 D0 C2 97 54 2A DF " +
            "9E 27 CD 63 9A B0 46 EC F2 88 22 10 27 89 3A 20 " +
            "B1 C3 6A 7C D8 4C BF 11 CB CD B5 99 F4 D5 FB 04 " +
            "48 95 92 4D FB AA B5 3B 59 C5 9D 17 1A 85 C1 BA " +
            "D6 70 F8 D7 08 12 CD 15 F0 C6 EC 3A 28 AA 72 96 " +
            "9F 6B 95 EA 56 0D 1F 55 21 08 50 77 A3 90 F6 89 " +
            "A9 66 4D 91 62 7F 3E 82",
            // Round #5, After Theta
            "69 BD 42 80 37 D7 D8 22 AD DD F1 DC BA 35 58 FA " +
            "95 97 59 A4 3B C8 9B 76 E7 C6 0C 8D 3B F4 B0 90 " +
            "30 A5 0F 7A 8D 62 6A 8A DC 9F 77 D3 40 B3 28 9A " +
            "85 A0 E0 E0 5B B6 BC FB 99 4A C5 03 7A 49 A2 09 " +
            "58 D3 1D 70 44 CF 59 27 BB C4 A1 22 B9 F9 EA 0D " +
            "32 0A DC 9E 61 5D 58 D1 21 CA 04 C8 2F CE 6D 45 " +
            "01 6C A4 19 58 89 94 47 9A 8B 86 F5 87 A3 A0 D1 " +
            "24 ED 71 25 6F BF CA FA B1 B5 CE BB 3C 06 63 9A " +
            "AA 9D D6 45 80 C8 4B A6 6F 03 9C 18 F2 51 4B C5 " +
            "7F 87 C4 7A EB 5D 3F 35 E3 0F 21 51 EF 8A 4D AC " +
            "95 4D 14 7C 13 9D 94 AF EB 98 50 03 70 2E 86 21 " +
            "3B A5 BC 6B 50 89 AF 94 16 1A 06 40 B3 67 7C 87 " +
            "13 AC F1 D7 97 70 B2 94", 
            // After Rho
            "69 BD 42 80 37 D7 D8 22 5B BB E3 B9 75 6B B0 F4 " +
            "E5 65 16 E9 0E F2 A6 5D 43 0F 0B 79 6E CC D0 B8 " +
            "14 53 53 84 29 7D D0 6B 0D 34 8B A2 C9 FD 79 37 " +
            "0E BE 65 CB BB 5F 08 0A 42 A6 52 F1 80 5E 92 68 " +
            "E9 0E 38 A2 E7 AC 13 AC AF DE B0 4B 1C 2A 92 9B " +
            "96 51 E0 F6 0C EB C2 8A 15 85 28 13 20 BF 38 B7 " +
            "CD C0 4A A4 3C 0A 60 23 47 41 A3 35 17 0D EB 0F " +
            "92 B7 5F 65 7D 92 F6 B8 77 79 0C C6 34 63 6B 9D " +
            "BA 08 10 79 C9 54 B5 D3 A5 E2 B7 01 4E 0C F9 A8 " +
            "EB A7 E6 EF 90 58 6F BD AC E3 0F 21 51 EF 8A 4D " +
            "52 BE 56 36 51 F0 4D 74 AC 63 42 0D C0 B9 18 86 " +
            "A7 94 77 0D 2A F1 95 72 1A 06 40 B3 67 7C 87 16 " +
            "2C E5 04 6B FC F5 25 9C", 
            // After Pi
            "69 BD 42 80 37 D7 D8 22 0E BE 65 CB BB 5F 08 0A " +
            "CD C0 4A A4 3C 0A 60 23 EB A7 E6 EF 90 58 6F BD " +
            "2C E5 04 6B FC F5 25 9C 43 0F 0B 79 6E CC D0 B8 " +
            "AF DE B0 4B 1C 2A 92 9B 96 51 E0 F6 0C EB C2 8A " +
            "BA 08 10 79 C9 54 B5 D3 A7 94 77 0D 2A F1 95 72 " +
            "5B BB E3 B9 75 6B B0 F4 42 A6 52 F1 80 5E 92 68 " +
            "47 41 A3 35 17 0D EB 0F AC E3 0F 21 51 EF 8A 4D " +
            "52 BE 56 36 51 F0 4D 74 14 53 53 84 29 7D D0 6B " +
            "0D 34 8B A2 C9 FD 79 37 15 85 28 13 20 BF 38 B7 " +
            "A5 E2 B7 01 4E 0C F9 A8 1A 06 40 B3 67 7C 87 16 " +
            "E5 65 16 E9 0E F2 A6 5D E9 0E 38 A2 E7 AC 13 AC " +
            "92 B7 5F 65 7D 92 F6 B8 77 79 0C C6 34 63 6B 9D " +
            "AC 63 42 0D C0 B9 18 86", 
            // After Chi
            "A8 FD 48 A4 33 D7 B8 03 2C 99 C1 80 3B 0F 07 96 " +
            "C9 80 4A A4 50 AF 60 23 AA BF A4 6F 93 5A B7 9F " +
            "2A E7 21 20 74 FD 25 94 53 0E 4B CD 6E 0D 90 B8 " +
            "87 D6 A0 42 DD 3E A7 CA 93 C5 87 F2 2E 4A C2 AA " +
            "FA 03 18 09 8D 58 F5 5B 0B 44 C7 0F 3A D3 97 71 " +
            "5E FA 42 BD 62 6A D9 F3 EA 04 5E F1 C0 BC 92 28 " +
            "15 5D F3 23 17 1D AE 3F A5 E2 AE A8 75 E4 3A CD " +
            "52 BA 46 76 D1 E4 4F 7C 04 D2 73 95 09 7F D0 EB " +
            "AD 56 1C A2 87 FD B8 3F 0F 81 68 A1 01 CF 3E A1 " +
            "A1 B3 A4 05 46 0D A9 C1 13 22 C8 91 A7 FC AE 02 " +
            "F7 D4 51 AC 16 E0 42 4D 8C 46 38 20 E7 CD 1A A9 " +
            "1A B5 1D 6C BD 0A E6 BA 36 7D 18 26 3A 21 CD C4 " +
            "A4 69 6A 0F 21 B5 09 26", 
            // After Iota
            "A9 FD 48 24 33 D7 B8 03 2C 99 C1 80 3B 0F 07 96 " +
            "C9 80 4A A4 50 AF 60 23 AA BF A4 6F 93 5A B7 9F " +
            "2A E7 21 20 74 FD 25 94 53 0E 4B CD 6E 0D 90 B8 " +
            "87 D6 A0 42 DD 3E A7 CA 93 C5 87 F2 2E 4A C2 AA " +
            "FA 03 18 09 8D 58 F5 5B 0B 44 C7 0F 3A D3 97 71 " +
            "5E FA 42 BD 62 6A D9 F3 EA 04 5E F1 C0 BC 92 28 " +
            "15 5D F3 23 17 1D AE 3F A5 E2 AE A8 75 E4 3A CD " +
            "52 BA 46 76 D1 E4 4F 7C 04 D2 73 95 09 7F D0 EB " +
            "AD 56 1C A2 87 FD B8 3F 0F 81 68 A1 01 CF 3E A1 " +
            "A1 B3 A4 05 46 0D A9 C1 13 22 C8 91 A7 FC AE 02 " +
            "F7 D4 51 AC 16 E0 42 4D 8C 46 38 20 E7 CD 1A A9 " +
            "1A B5 1D 6C BD 0A E6 BA 36 7D 18 26 3A 21 CD C4 " +
            "A4 69 6A 0F 21 B5 09 26",
            // Round #6, After Theta
            "AC 19 7C 81 A7 2E C3 7B CE CE 34 9D B0 5B CC 23 " +
            "6D FB 0C CE 39 86 C9 D9 79 36 EB 59 75 61 D6 48 " +
            "E7 69 49 17 23 69 FF 44 56 EA 7F 68 FA F4 EB C0 " +
            "65 81 55 5F 56 6A 6C 7F 37 BE C1 98 47 63 6B 50 " +
            "29 8A 57 3F 6B 63 94 8C C6 CA AF 38 6D 47 4D A1 " +
            "5B 1E 76 18 F6 93 A2 8B 08 53 AB EC 4B E8 59 9D " +
            "B1 26 B5 49 7E 34 07 C5 76 6B E1 9E 93 DF 5B 1A " +
            "9F 34 2E 41 86 70 95 AC 01 36 47 30 9D 86 AB 93 " +
            "4F 01 E9 BF 0C A9 73 8A AB FA 2E CB 68 E6 97 5B " +
            "72 3A EB 33 A0 36 C8 16 DE AC A0 A6 F0 68 74 D2 " +
            "F2 30 65 09 82 19 39 35 6E 11 CD 3D 6C 99 D1 1C " +
            "BE CE 5B 06 D4 23 4F 40 E5 F4 57 10 DC 1A AC 13 " +
            "69 E7 02 38 76 21 D3 F6", 
            // After Rho
            "AC 19 7C 81 A7 2E C3 7B 9C 9D 69 3A 61 B7 98 47 " +
            "DB 3E 83 73 8E 61 72 76 17 66 8D 94 67 B3 9E 55 " +
            "49 FB 27 3A 4F 4B BA 18 A6 4F BF 0E 6C A5 FE 87 " +
            "F5 65 A5 C6 F6 57 16 58 D4 8D 6F 30 E6 D1 D8 1A " +
            "C5 AB 9F B5 31 4A C6 14 D4 14 6A AC FC 8A D3 76 " +
            "DC F2 B0 C3 B0 9F 14 5D 75 22 4C AD B2 2F A1 67 " +
            "4D F2 A3 39 28 8E 35 A9 BF B7 34 EC D6 C2 3D 27 " +
            "20 43 B8 4A D6 4F 1A 97 60 3A 0D 57 27 03 6C 8E " +
            "FD 97 21 75 4E F1 29 20 CB AD 55 7D 97 65 34 F3 " +
            "06 D9 42 4E 67 7D 06 D4 D2 DE AC A0 A6 F0 68 74 " +
            "E4 D4 C8 C3 94 25 08 66 B8 45 34 F7 B0 65 46 73 " +
            "D7 79 CB 80 7A E4 09 C8 F4 57 10 DC 1A AC 13 E5 " +
            "B4 7D DA B9 00 8E 5D C8", 
            // After Pi
            "AC 19 7C 81 A7 2E C3 7B F5 65 A5 C6 F6 57 16 58 " +
            "4D F2 A3 39 28 8E 35 A9 06 D9 42 4E 67 7D 06 D4 " +
            "B4 7D DA B9 00 8E 5D C8 17 66 8D 94 67 B3 9E 55 " +
            "D4 14 6A AC FC 8A D3 76 DC F2 B0 C3 B0 9F 14 5D " +
            "FD 97 21 75 4E F1 29 20 D7 79 CB 80 7A E4 09 C8 " +
            "9C 9D 69 3A 61 B7 98 47 D4 8D 6F 30 E6 D1 D8 1A " +
            "BF B7 34 EC D6 C2 3D 27 D2 DE AC A0 A6 F0 68 74 " +
            "E4 D4 C8 C3 94 25 08 66 49 FB 27 3A 4F 4B BA 18 " +
            "A6 4F BF 0E 6C A5 FE 87 75 22 4C AD B2 2F A1 67 " +
            "CB AD 55 7D 97 65 34 F3 F4 57 10 DC 1A AC 13 E5 " +
            "DB 3E 83 73 8E 61 72 76 C5 AB 9F B5 31 4A C6 14 " +
            "20 43 B8 4A D6 4F 1A 97 60 3A 0D 57 27 03 6C 8E " +
            "B8 45 34 F7 B0 65 46 73", 
            // After Chi
            "A4 8B 7E B8 AF A6 E2 DA F7 6C E5 80 B1 26 14 0C " +
            "FD D6 3B 88 28 0C 6C A1 0E D9 66 4E C0 5D 84 E7 " +
            "E5 19 5B FF 50 DF 49 C8 1F 84 1D D7 67 A6 9A 5C " +
            "F5 11 6B 98 B2 EA FA 56 DE 9A 7A 43 80 9B 14 95 " +
            "FD 91 25 61 4B E2 BF 35 17 69 A9 A8 E2 EC 48 EA " +
            "B7 AF 79 F6 71 B5 BD 62 94 C5 E7 30 C6 E1 98 4A " +
            "9B B7 74 AF C6 C7 3D 25 CA D7 8D 98 C7 62 F8 75 " +
            "A4 D4 CE C3 12 65 48 7E 18 DB 67 9B DD 41 BB 78 " +
            "2C C2 AE 5E 69 E5 EA 17 41 70 4C 2D BA A7 A2 63 " +
            "C2 05 72 5F D2 26 9C EB 52 53 88 D8 3A 08 57 62 " +
            "FB 7E A3 39 48 64 6A F5 85 93 9A A0 10 4A A2 1C " +
            "B8 06 88 EA 46 2B 18 E6 23 00 8E 57 29 03 5C 8A " +
            "BC C4 28 73 81 6F C2 73", 
            // After Iota
            "25 0B 7E 38 AF A6 E2 5A F7 6C E5 80 B1 26 14 0C " +
            "FD D6 3B 88 28 0C 6C A1 0E D9 66 4E C0 5D 84 E7 " +
            "E5 19 5B FF 50 DF 49 C8 1F 84 1D D7 67 A6 9A 5C " +
            "F5 11 6B 98 B2 EA FA 56 DE 9A 7A 43 80 9B 14 95 " +
            "FD 91 25 61 4B E2 BF 35 17 69 A9 A8 E2 EC 48 EA " +
            "B7 AF 79 F6 71 B5 BD 62 94 C5 E7 30 C6 E1 98 4A " +
            "9B B7 74 AF C6 C7 3D 25 CA D7 8D 98 C7 62 F8 75 " +
            "A4 D4 CE C3 12 65 48 7E 18 DB 67 9B DD 41 BB 78 " +
            "2C C2 AE 5E 69 E5 EA 17 41 70 4C 2D BA A7 A2 63 " +
            "C2 05 72 5F D2 26 9C EB 52 53 88 D8 3A 08 57 62 " +
            "FB 7E A3 39 48 64 6A F5 85 93 9A A0 10 4A A2 1C " +
            "B8 06 88 EA 46 2B 18 E6 23 00 8E 57 29 03 5C 8A " +
            "BC C4 28 73 81 6F C2 73",
            // Round #7, After Theta
            "E3 EA 59 AB CD 92 43 21 1A F3 D8 7C B8 0F FF CC " +
            "73 0A 03 20 FB 7F 55 36 3F 33 AF 92 64 E3 C3 E8 " +
            "E0 89 D4 37 BE 07 63 DC D9 65 3A 44 05 92 3B 27 " +
            "18 8E 56 64 BB C3 11 96 50 46 42 EB 53 E8 2D 02 " +
            "CC 7B EC BD EF 5C F8 3A 12 F9 26 60 0C 34 62 FE " +
            "71 4E 5E 65 13 81 1C 19 79 5A DA CC CF C8 73 8A " +
            "15 6B 4C 07 15 B4 04 B2 FB 3D 44 44 63 DC BF 7A " +
            "A1 44 41 0B FC BD 62 6A DE 3A 40 08 BF 75 1A 03 " +
            "C1 5D 93 A2 60 CC 01 D7 CF AC 74 85 69 D4 9B F4 " +
            "F3 EF BB 83 76 98 DB E4 57 C3 07 10 D4 D0 7D 76 " +
            "3D 9F 84 AA 2A 50 CB 8E 68 0C A7 5C 19 63 49 DC " +
            "36 DA B0 42 95 58 21 71 12 EA 47 8B 8D BD 1B 85 " +
            "B9 54 A7 BB 6F B7 E8 67", 
            // After Rho
            "E3 EA 59 AB CD 92 43 21 35 E6 B1 F9 70 1F FE 99 " +
            "9C C2 00 C8 FE 5F 95 CD 36 3E 8C FE 33 F3 2A 49 " +
            "3D 18 E3 06 4F A4 BE F1 54 20 B9 73 92 5D A6 43 " +
            "45 B6 3B 1C 61 89 E1 68 00 94 91 D0 FA 14 7A 8B " +
            "3D F6 DE 77 2E 7C 1D E6 23 E6 2F 91 6F 02 C6 40 " +
            "88 73 F2 2A 9B 08 E4 C8 29 E6 69 69 33 3F 23 CF " +
            "3A A8 A0 25 90 AD 58 63 B8 7F F5 F6 7B 88 88 C6 " +
            "05 FE 5E 31 B5 50 A2 A0 10 7E EB 34 06 BC 75 80 " +
            "52 14 8C 39 E0 3A B8 6B 4D FA 67 56 BA C2 34 EA " +
            "73 9B 7C FE 7D 77 D0 0E 76 57 C3 07 10 D4 D0 7D " +
            "2D 3B F6 7C 12 AA AA 40 A3 31 9C 72 65 8C 25 71 " +
            "46 1B 56 A8 12 2B 24 CE EA 47 8B 8D BD 1B 85 12 " +
            "FA 59 2E D5 E9 EE DB 2D", 
            // After Pi
            "E3 EA 59 AB CD 92 43 21 45 B6 3B 1C 61 89 E1 68 " +
            "3A A8 A0 25 90 AD 58 63 73 9B 7C FE 7D 77 D0 0E " +
            "FA 59 2E D5 E9 EE DB 2D 36 3E 8C FE 33 F3 2A 49 " +
            "23 E6 2F 91 6F 02 C6 40 88 73 F2 2A 9B 08 E4 C8 " +
            "52 14 8C 39 E0 3A B8 6B 46 1B 56 A8 12 2B 24 CE " +
            "35 E6 B1 F9 70 1F FE 99 00 94 91 D0 FA 14 7A 8B " +
            "B8 7F F5 F6 7B 88 88 C6 76 57 C3 07 10 D4 D0 7D " +
            "2D 3B F6 7C 12 AA AA 40 3D 18 E3 06 4F A4 BE F1 " +
            "54 20 B9 73 92 5D A6 43 29 E6 69 69 33 3F 23 CF " +
            "4D FA 67 56 BA C2 34 EA EA 47 8B 8D BD 1B 85 12 " +
            "9C C2 00 C8 FE 5F 95 CD 3D F6 DE 77 2E 7C 1D E6 " +
            "05 FE 5E 31 B5 50 A2 A0 10 7E EB 34 06 BC 75 80 " +
            "A3 31 9C 72 65 8C 25 71", 
            // After Chi
            "D9 E2 D9 8A 5D B6 5B 22 04 A5 67 C6 0C DB 61 64 " +
            "B2 E8 A2 24 10 25 53 42 72 39 2D D4 79 67 D0 0E " +
            "FE 4D 0C C1 C9 E7 7B 65 BE 2F 5C D4 A3 FB 0A C1 " +
            "71 E2 23 80 0F 30 DE 63 8C 78 A0 AA 89 09 E0 4C " +
            "62 30 04 6F C1 EA B2 6A 47 DB 75 A9 5E 2B E0 CE " +
            "8D 8D D5 DF 71 97 7E DD 46 94 93 D1 FA 40 2A B2 " +
            "B1 57 C1 8E 79 A2 A2 C6 66 93 C2 86 70 C1 84 E4 " +
            "2D 2B F6 7C 98 AA AA 42 14 DE A3 0E 6E 86 BF 7D " +
            "10 38 BF 65 1A 9D B2 63 8B E3 E1 E0 36 26 A2 DF " +
            "58 E2 07 54 F8 66 0E 0B AA 67 93 FC 2D 42 85 10 " +
            "9C CA 00 C8 6F 5F 37 CD 2D F6 7F 73 2C D0 48 E6 " +
            "A6 FF 4A 73 D4 50 A2 D1 0C BC EB BC 9C EF E5 0C " +
            "82 05 42 45 65 AC 2D 53", 
            // After Iota
            "D0 62 D9 8A 5D B6 5B A2 04 A5 67 C6 0C DB 61 64 " +
            "B2 E8 A2 24 10 25 53 42 72 39 2D D4 79 67 D0 0E " +
            "FE 4D 0C C1 C9 E7 7B 65 BE 2F 5C D4 A3 FB 0A C1 " +
            "71 E2 23 80 0F 30 DE 63 8C 78 A0 AA 89 09 E0 4C " +
            "62 30 04 6F C1 EA B2 6A 47 DB 75 A9 5E 2B E0 CE " +
            "8D 8D D5 DF 71 97 7E DD 46 94 93 D1 FA 40 2A B2 " +
            "B1 57 C1 8E 79 A2 A2 C6 66 93 C2 86 70 C1 84 E4 " +
            "2D 2B F6 7C 98 AA AA 42 14 DE A3 0E 6E 86 BF 7D " +
            "10 38 BF 65 1A 9D B2 63 8B E3 E1 E0 36 26 A2 DF " +
            "58 E2 07 54 F8 66 0E 0B AA 67 93 FC 2D 42 85 10 " +
            "9C CA 00 C8 6F 5F 37 CD 2D F6 7F 73 2C D0 48 E6 " +
            "A6 FF 4A 73 D4 50 A2 D1 0C BC EB BC 9C EF E5 0C " +
            "82 05 42 45 65 AC 2D 53",
            // Round #8, After Theta
            "70 87 A9 25 85 F3 1D 68 2A C6 45 A7 87 28 E5 E6 " +
            "F9 7D BA 0F 86 48 27 7C A9 5D F8 1D F4 8F F2 9D " +
            "0A 21 EC 9B 79 25 38 FF 1E CA 2C 7B 7B BE 4C 0B " +
            "5F 81 01 E1 84 C3 5A E1 C7 ED B8 81 1F 64 94 72 " +
            "B9 54 D1 A6 4C 02 90 F9 B3 B7 95 F3 EE E9 A3 54 " +
            "2D 68 A5 70 A9 D2 38 17 68 F7 B1 B0 71 B3 AE 30 " +
            "FA C2 D9 A5 EF CF D6 F8 BD F7 17 4F FD 29 A6 77 " +
            "D9 47 16 26 28 68 E9 D8 B4 3B D3 A1 B6 C3 F9 B7 " +
            "3E 5B 9D 04 91 6E 36 E1 C0 76 F9 CB A0 4B D6 E1 " +
            "83 86 D2 9D 75 8E 2C 98 5E 0B 73 A6 9D 80 C6 8A " +
            "3C 2F 70 67 B7 1A 71 07 03 95 5D 12 A7 23 CC 64 " +
            "ED 6A 52 58 42 3D D6 EF D7 D8 3E 75 11 07 C7 9F " +
            "76 69 A2 1F D5 6E 6E C9", 
            // After Rho
            "70 87 A9 25 85 F3 1D 68 55 8C 8B 4E 0F 51 CA CD " +
            "7E 9F EE 83 21 D2 09 5F FF 28 DF 99 DA 85 DF 41 " +
            "2B C1 F9 57 08 61 DF CC B7 E7 CB B4 E0 A1 CC B2 " +
            "10 4E 38 AC 15 FE 15 18 DC 71 3B 6E E0 07 19 A5 " +
            "AA 68 53 26 01 C8 FC 5C 3E 4A 35 7B 5B 39 EF 9E " +
            "68 41 2B 85 4B 95 C6 B9 C2 A0 DD C7 C2 C6 CD BA " +
            "2E 7D 7F B6 C6 D7 17 CE 53 4C EF 7A EF 2F 9E FA " +
            "13 14 B4 74 EC EC 23 0B 43 6D 87 F3 6F 69 77 A6 " +
            "93 20 D2 CD 26 DC 67 AB EB 70 60 BB FC 65 D0 25 " +
            "91 05 73 D0 50 BA B3 CE 8A 5E 0B 73 A6 9D 80 C6 " +
            "C4 1D F0 BC C0 9D DD 6A 0D 54 76 49 9C 8E 30 93 " +
            "5D 4D 0A 4B A8 C7 FA BD D8 3E 75 11 07 C7 9F D7 " +
            "5B B2 5D 9A E8 47 B5 9B", 
            // After Pi
            "70 87 A9 25 85 F3 1D 68 10 4E 38 AC 15 FE 15 18 " +
            "2E 7D 7F B6 C6 D7 17 CE 91 05 73 D0 50 BA B3 CE " +
            "5B B2 5D 9A E8 47 B5 9B FF 28 DF 99 DA 85 DF 41 " +
            "3E 4A 35 7B 5B 39 EF 9E 68 41 2B 85 4B 95 C6 B9 " +
            "93 20 D2 CD 26 DC 67 AB 5D 4D 0A 4B A8 C7 FA BD " +
            "55 8C 8B 4E 0F 51 CA CD DC 71 3B 6E E0 07 19 A5 " +
            "53 4C EF 7A EF 2F 9E FA 8A 5E 0B 73 A6 9D 80 C6 " +
            "C4 1D F0 BC C0 9D DD 6A 2B C1 F9 57 08 61 DF CC " +
            "B7 E7 CB B4 E0 A1 CC B2 C2 A0 DD C7 C2 C6 CD BA " +
            "EB 70 60 BB FC 65 D0 25 D8 3E 75 11 07 C7 9F D7 " +
            "7E 9F EE 83 21 D2 09 5F AA 68 53 26 01 C8 FC 5C " +
            "13 14 B4 74 EC EC 23 0B 43 6D 87 F3 6F 69 77 A6 " +
            "0D 54 76 49 9C 8E 30 93", 
            // After Chi
            "5E B6 EE 37 47 F2 1F AE 81 4E 38 EC 05 D6 B5 18 " +
            "64 CF 73 BC 6E 92 13 DF B1 00 D3 F5 55 0A BB AE " +
            "5B FA 4D 12 F8 4B B5 8B BF 29 D5 1D DA 01 DF 60 " +
            "AD 6A E5 33 7F 71 CE 9C 24 0C 23 87 C3 96 5E AD " +
            "31 00 07 5D 74 DC 62 EB 5D 0F 2A 29 A9 FF DA 23 " +
            "56 80 4F 5E 00 79 4C 97 54 63 3B 6F E0 97 19 A1 " +
            "17 4D 1F F6 AF 2F C3 D2 9B DE 00 31 A9 DD 82 43 " +
            "4C 6C C0 9C 20 9B CC 4A 6B C1 ED 14 0A 27 DE C4 " +
            "9E B7 EB 8C DC 80 DC B7 D2 AE C8 C7 C1 44 C2 68 " +
            "C8 B1 E8 FD F4 45 90 2D 4C 18 77 B1 E7 47 9F E5 " +
            "6F 8B 4A D3 CD F6 0A 5C EA 01 50 A5 02 C9 A8 F8 " +
            "1F 04 C4 7C 7C 6A 23 1A 31 E6 0F 71 4E 39 7E EA " +
            "8D 34 67 6D 9C 86 C4 93", 
            // After Iota
            "D4 B6 EE 37 47 F2 1F AE 81 4E 38 EC 05 D6 B5 18 " +
            "64 CF 73 BC 6E 92 13 DF B1 00 D3 F5 55 0A BB AE " +
            "5B FA 4D 12 F8 4B B5 8B BF 29 D5 1D DA 01 DF 60 " +
            "AD 6A E5 33 7F 71 CE 9C 24 0C 23 87 C3 96 5E AD " +
            "31 00 07 5D 74 DC 62 EB 5D 0F 2A 29 A9 FF DA 23 " +
            "56 80 4F 5E 00 79 4C 97 54 63 3B 6F E0 97 19 A1 " +
            "17 4D 1F F6 AF 2F C3 D2 9B DE 00 31 A9 DD 82 43 " +
            "4C 6C C0 9C 20 9B CC 4A 6B C1 ED 14 0A 27 DE C4 " +
            "9E B7 EB 8C DC 80 DC B7 D2 AE C8 C7 C1 44 C2 68 " +
            "C8 B1 E8 FD F4 45 90 2D 4C 18 77 B1 E7 47 9F E5 " +
            "6F 8B 4A D3 CD F6 0A 5C EA 01 50 A5 02 C9 A8 F8 " +
            "1F 04 C4 7C 7C 6A 23 1A 31 E6 0F 71 4E 39 7E EA " +
            "8D 34 67 6D 9C 86 C4 93",
            // Round #9, After Theta
            "47 E1 E2 7E C4 EE CB EE 8D 52 6D B3 21 86 33 7D " +
            "AD 2D 49 0F 4E 05 6F 36 3C 4F FF 74 FE D3 25 55 " +
            "CA D9 D8 60 7F 8A B0 C8 2C 7E D9 54 59 1D 0B 20 " +
            "A1 76 B0 6C 5B 21 48 F9 ED EE 19 34 E3 01 22 44 " +
            "BC 4F 2B DC DF 05 FC 10 CC 2C BF 5B 2E 3E DF 60 " +
            "C5 D7 43 17 83 65 98 D7 58 7F 6E 30 C4 C7 9F C4 " +
            "DE AF 25 45 8F B8 BF 3B 16 91 2C B0 02 04 1C B8 " +
            "DD 4F 55 EE A7 5A C9 09 F8 96 E1 5D 89 3B 0A 84 " +
            "92 AB BE D3 F8 D0 5A D2 1B 4C F2 74 E1 D3 BE 81 " +
            "45 FE C4 7C 5F 9C 0E D6 DD 3B E2 C3 60 86 9A A6 " +
            "FC DC 46 9A 4E EA DE 1C E6 1D 05 FA 26 99 2E 9D " +
            "D6 E6 FE CF 5C FD 5F F3 BC A9 23 F0 E5 E0 E0 11 " +
            "1C 17 F2 1F 1B 47 C1 D0", 
            // After Rho
            "47 E1 E2 7E C4 EE CB EE 1A A5 DA 66 43 0C 67 FA " +
            "6B 4B D2 83 53 C1 9B 4D 3F 5D 52 C5 F3 F4 4F E7 " +
            "53 84 45 56 CE C6 06 FB 95 D5 B1 00 C2 E2 97 4D " +
            "CB B6 15 82 94 1F 6A 07 51 BB 7B 06 CD 78 80 08 " +
            "A7 15 EE EF 02 7E 08 DE F3 0D C6 CC F2 BB E5 E2 " +
            "2E BE 1E BA 18 2C C3 BC 12 63 FD B9 C1 10 1F 7F " +
            "29 7A C4 FD DD F1 7E 2D 08 38 70 2D 22 59 60 05 " +
            "F7 53 AD E4 84 EE A7 2A BB 12 77 14 08 F1 2D C3 " +
            "77 1A 1F 5A 4B 5A 72 D5 DF C0 0D 26 79 BA F0 69 " +
            "D3 C1 BA C8 9F 98 EF 8B A6 DD 3B E2 C3 60 86 9A " +
            "7B 73 F0 73 1B 69 3A A9 9A 77 14 E8 9B 64 BA 74 " +
            "DA DC FF 99 AB FF 6B DE A9 23 F0 E5 E0 E0 11 BC " +
            "30 34 C7 85 FC C7 C6 51", 
            // After Pi
            "47 E1 E2 7E C4 EE CB EE CB B6 15 82 94 1F 6A 07 " +
            "29 7A C4 FD DD F1 7E 2D D3 C1 BA C8 9F 98 EF 8B " +
            "30 34 C7 85 FC C7 C6 51 3F 5D 52 C5 F3 F4 4F E7 " +
            "F3 0D C6 CC F2 BB E5 E2 2E BE 1E BA 18 2C C3 BC " +
            "77 1A 1F 5A 4B 5A 72 D5 DA DC FF 99 AB FF 6B DE " +
            "1A A5 DA 66 43 0C 67 FA 51 BB 7B 06 CD 78 80 08 " +
            "08 38 70 2D 22 59 60 05 A6 DD 3B E2 C3 60 86 9A " +
            "7B 73 F0 73 1B 69 3A A9 53 84 45 56 CE C6 06 FB " +
            "95 D5 B1 00 C2 E2 97 4D 12 63 FD B9 C1 10 1F 7F " +
            "DF C0 0D 26 79 BA F0 69 A9 23 F0 E5 E0 E0 11 BC " +
            "6B 4B D2 83 53 C1 9B 4D A7 15 EE EF 02 7E 08 DE " +
            "F7 53 AD E4 84 EE A7 2A BB 12 77 14 08 F1 2D C3 " +
            "9A 77 14 E8 9B 64 BA 74", 
            // After Chi
            "67 A9 22 03 8D 0E DF C6 19 37 2F 82 96 17 EB 85 " +
            "09 4E 81 F8 BD B6 7E 7D 94 00 9A B2 9F B0 E6 25 " +
            "B8 22 D2 05 EC D6 E6 50 33 EF 4A F7 FB F0 4D FB " +
            "A2 0D C7 8C B1 E9 D5 A3 A6 7A FE 3B B8 89 CA B6 " +
            "52 1B 1F 1E 1B 5A 76 F4 1A DC 7B 91 AB F4 CB DE " +
            "12 A5 DA 4F 61 0D 07 FF F7 7E 70 C4 0C 58 06 92 " +
            "51 1A B0 3C 3A 50 58 24 A6 59 31 E6 83 64 C3 C8 " +
            "3A 69 D1 73 97 19 BA A9 51 A6 09 EF CF D6 0E C9 " +
            "58 55 B1 06 FA 48 77 4D 32 40 0D 78 41 50 1E EB " +
            "8D 44 08 34 77 BC F6 2A 2D 72 40 E5 E0 C0 80 B8 " +
            "3B 09 D3 83 D7 41 3C 6D AF 15 BC FF 0A 6F 00 1F " +
            "F7 36 AD 0C 17 EA 35 1E DA 1A B5 17 48 70 2C CA " +
            "1E 63 38 84 9B 5A BA E6", 
            // After Iota
            "EF A9 22 03 8D 0E DF C6 19 37 2F 82 96 17 EB 85 " +
            "09 4E 81 F8 BD B6 7E 7D 94 00 9A B2 9F B0 E6 25 " +
            "B8 22 D2 05 EC D6 E6 50 33 EF 4A F7 FB F0 4D FB " +
            "A2 0D C7 8C B1 E9 D5 A3 A6 7A FE 3B B8 89 CA B6 " +
            "52 1B 1F 1E 1B 5A 76 F4 1A DC 7B 91 AB F4 CB DE " +
            "12 A5 DA 4F 61 0D 07 FF F7 7E 70 C4 0C 58 06 92 " +
            "51 1A B0 3C 3A 50 58 24 A6 59 31 E6 83 64 C3 C8 " +
            "3A 69 D1 73 97 19 BA A9 51 A6 09 EF CF D6 0E C9 " +
            "58 55 B1 06 FA 48 77 4D 32 40 0D 78 41 50 1E EB " +
            "8D 44 08 34 77 BC F6 2A 2D 72 40 E5 E0 C0 80 B8 " +
            "3B 09 D3 83 D7 41 3C 6D AF 15 BC FF 0A 6F 00 1F " +
            "F7 36 AD 0C 17 EA 35 1E DA 1A B5 17 48 70 2C CA " +
            "1E 63 38 84 9B 5A BA E6",
            // Round #10, After Theta
            "33 26 08 E2 90 AC ED 73 CB CB 99 43 4A D9 C3 D6 " +
            "DD 72 06 19 16 B3 23 68 F9 55 F4 35 A1 26 7A CC " +
            "C7 A7 0B C2 CB 5C 21 64 EF 60 60 16 E6 52 7F 4E " +
            "70 F1 71 4D 6D 27 FD F0 72 46 79 DA 13 8C 97 A3 " +
            "3F 4E 71 99 25 CC EA 1D 65 59 A2 56 8C 7E 0C EA " +
            "CE 2A F0 AE 7C AF 35 4A 25 82 C6 05 D0 96 2E C1 " +
            "85 26 37 DD 91 55 05 31 CB 0C 5F 61 BD F2 5F 21 " +
            "45 EC 08 B4 B0 93 7D 9D 8D 29 23 0E D2 74 3C 7C " +
            "8A A9 07 C7 26 86 5F 1E E6 7C 8A 99 EA 55 43 FE " +
            "E0 11 66 B3 49 2A 6A C3 52 F7 99 22 C7 4A 47 8C " +
            "E7 86 F9 62 CA E3 0E D8 7D E9 0A 3E D6 A1 28 4C " +
            "23 0A 2A ED BC EF 68 0B B7 4F DB 90 76 E6 B0 23 " +
            "61 E6 E1 43 BC D0 7D D2", 
            // After Rho
            "33 26 08 E2 90 AC ED 73 97 97 33 87 94 B2 87 AD " +
            "B7 9C 41 86 C5 EC 08 5A 6A A2 C7 9C 5F 45 5F 13 " +
            "E6 0A 21 3B 3E 5D 10 5E 61 2E F5 E7 F4 0E 06 66 " +
            "D7 D4 76 D2 0F 0F 17 1F A8 9C 51 9E F6 04 E3 E5 " +
            "A7 B8 CC 12 66 F5 8E 1F C7 A0 5E 96 25 6A C5 E8 " +
            "72 56 81 77 E5 7B AD 51 04 97 08 1A 17 40 5B BA " +
            "E9 8E AC 2A 88 29 34 B9 E5 BF 42 96 19 BE C2 7A " +
            "5A D8 C9 BE CE 22 76 04 1C A4 E9 78 F8 1A 53 46 " +
            "E0 D8 C4 F0 CB 43 31 F5 21 7F 73 3E C5 4C F5 AA " +
            "45 6D 18 3C C2 6C 36 49 8C 52 F7 99 22 C7 4A 47 " +
            "3B 60 9F 1B E6 8B 29 8F F5 A5 2B F8 58 87 A2 30 " +
            "44 41 A5 9D F7 1D 6D 61 4F DB 90 76 E6 B0 23 B7 " +
            "9F 74 98 79 F8 10 2F 74", 
            // After Pi
            "33 26 08 E2 90 AC ED 73 D7 D4 76 D2 0F 0F 17 1F " +
            "E9 8E AC 2A 88 29 34 B9 45 6D 18 3C C2 6C 36 49 " +
            "9F 74 98 79 F8 10 2F 74 6A A2 C7 9C 5F 45 5F 13 " +
            "C7 A0 5E 96 25 6A C5 E8 72 56 81 77 E5 7B AD 51 " +
            "E0 D8 C4 F0 CB 43 31 F5 44 41 A5 9D F7 1D 6D 61 " +
            "97 97 33 87 94 B2 87 AD A8 9C 51 9E F6 04 E3 E5 " +
            "E5 BF 42 96 19 BE C2 7A 8C 52 F7 99 22 C7 4A 47 " +
            "3B 60 9F 1B E6 8B 29 8F E6 0A 21 3B 3E 5D 10 5E " +
            "61 2E F5 E7 F4 0E 06 66 04 97 08 1A 17 40 5B BA " +
            "21 7F 73 3E C5 4C F5 AA 4F DB 90 76 E6 B0 23 B7 " +
            "B7 9C 41 86 C5 EC 08 5A A7 B8 CC 12 66 F5 8E 1F " +
            "5A D8 C9 BE CE 22 76 04 1C A4 E9 78 F8 1A 53 46 " +
            "F5 A5 2B F8 58 87 A2 30", 
            // After Chi
            "1B 2C 80 CA 10 8C CD D3 D3 B5 66 C6 4D 4B 15 5F " +
            "73 9E 2C 6B B0 39 3D 8D 65 6F 18 BE C2 C0 F6 4A " +
            "5B A4 EE 69 F7 13 3D 78 5A F4 46 FD 9F 54 77 02 " +
            "47 28 1A 16 2F 6A D5 4C 76 57 A0 7A D1 67 E1 51 " +
            "CA 7A 86 F0 C3 03 23 E7 C1 41 BD 9F D7 37 ED 89 " +
            "D2 B4 31 87 9D 08 87 B7 A0 DC E4 97 D4 45 EB E0 " +
            "D6 9F 4A 94 DD B6 E3 F2 08 C5 D7 1D 32 F7 CC 67 " +
            "13 68 DF 03 84 8F 49 CF E2 9B 29 23 3D 1D 49 C6 " +
            "40 46 86 C3 34 02 A2 66 4A 17 88 5A 35 F0 59 AF " +
            "81 7F 52 37 DD 01 E5 E2 4E FF 44 B2 26 B2 25 97 " +
            "EF DC 40 2A 4D EE 78 5A A3 9C EC 52 56 ED 8F 5D " +
            "BB D9 CB 3E CE A7 D6 34 1E BC A9 7E 7D 72 5B 0C " +
            "F5 85 A7 E8 7A 96 24 35", 
            // After Iota
            "12 AC 80 4A 10 8C CD D3 D3 B5 66 C6 4D 4B 15 5F " +
            "73 9E 2C 6B B0 39 3D 8D 65 6F 18 BE C2 C0 F6 4A " +
            "5B A4 EE 69 F7 13 3D 78 5A F4 46 FD 9F 54 77 02 " +
            "47 28 1A 16 2F 6A D5 4C 76 57 A0 7A D1 67 E1 51 " +
            "CA 7A 86 F0 C3 03 23 E7 C1 41 BD 9F D7 37 ED 89 " +
            "D2 B4 31 87 9D 08 87 B7 A0 DC E4 97 D4 45 EB E0 " +
            "D6 9F 4A 94 DD B6 E3 F2 08 C5 D7 1D 32 F7 CC 67 " +
            "13 68 DF 03 84 8F 49 CF E2 9B 29 23 3D 1D 49 C6 " +
            "40 46 86 C3 34 02 A2 66 4A 17 88 5A 35 F0 59 AF " +
            "81 7F 52 37 DD 01 E5 E2 4E FF 44 B2 26 B2 25 97 " +
            "EF DC 40 2A 4D EE 78 5A A3 9C EC 52 56 ED 8F 5D " +
            "BB D9 CB 3E CE A7 D6 34 1E BC A9 7E 7D 72 5B 0C " +
            "F5 85 A7 E8 7A 96 24 35",
            // Round #11, After Theta
            "8F 6C 0A 48 41 14 58 DF 01 2E F3 3C A0 16 78 CE " +
            "D4 23 BA 88 42 3D 75 0C 22 19 42 01 74 60 77 C6 " +
            "4C E0 61 00 A0 12 82 A8 C7 34 CC FF CE CC E2 0E " +
            "95 B3 8F EC C2 37 B8 DD D1 EA 36 99 23 63 A9 D0 " +
            "8D 0C DC 4F 75 A3 A2 6B D6 05 32 F6 80 36 52 59 " +
            "4F 74 BB 85 CC 90 12 BB 72 47 71 6D 39 18 86 71 " +
            "71 22 DC 77 2F B2 AB 73 4F B3 8D A2 84 57 4D EB " +
            "04 2C 50 6A D3 8E F6 1F 7F 5B A3 21 6C 85 DC CA " +
            "92 DD 13 39 D9 5F CF F7 ED AA 1E B9 C7 F4 11 2E " +
            "C6 09 08 88 6B A1 64 6E 59 BB CB DB 71 B3 9A 47 " +
            "72 1C CA 28 1C 76 ED 56 71 07 79 A8 BB B0 E2 CC " +
            "1C 64 5D DD 3C A3 9E B5 59 CA F3 C1 CB D2 DA 80 " +
            "E2 C1 28 81 2D 97 9B E5", 
            // After Rho
            "8F 6C 0A 48 41 14 58 DF 03 5C E6 79 40 2D F0 9C " +
            "F5 88 2E A2 50 4F 1D 03 07 76 67 2C 92 21 14 40 " +
            "95 10 44 65 02 0F 03 00 EF CC 2C EE 70 4C C3 FC " +
            "C8 2E 7C 83 DB 5D 39 FB 74 B4 BA 4D E6 C8 58 2A " +
            "06 EE A7 BA 51 D1 B5 46 23 95 65 5D 20 63 0F 68 " +
            "7D A2 DB 2D 64 86 94 D8 C6 C9 1D C5 B5 E5 60 18 " +
            "BE 7B 91 5D 9D 8B 13 E1 AF 9A D6 9F 66 1B 45 09 " +
            "B5 69 47 FB 0F 02 16 28 43 D8 0A B9 95 FF B6 46 " +
            "22 27 FB EB F9 5E B2 7B 08 97 76 55 8F DC 63 FA " +
            "94 CC CD 38 01 01 71 2D 47 59 BB CB DB 71 B3 9A " +
            "B5 5B C9 71 28 A3 70 D8 C7 1D E4 A1 EE C2 8A 33 " +
            "83 AC AB 9B 67 D4 B3 96 CA F3 C1 CB D2 DA 80 59 " +
            "66 B9 78 30 4A 60 CB E5", 
            // After Pi
            "8F 6C 0A 48 41 14 58 DF C8 2E 7C 83 DB 5D 39 FB " +
            "BE 7B 91 5D 9D 8B 13 E1 94 CC CD 38 01 01 71 2D " +
            "66 B9 78 30 4A 60 CB E5 07 76 67 2C 92 21 14 40 " +
            "23 95 65 5D 20 63 0F 68 7D A2 DB 2D 64 86 94 D8 " +
            "22 27 FB EB F9 5E B2 7B 83 AC AB 9B 67 D4 B3 96 " +
            "03 5C E6 79 40 2D F0 9C 74 B4 BA 4D E6 C8 58 2A " +
            "AF 9A D6 9F 66 1B 45 09 47 59 BB CB DB 71 B3 9A " +
            "B5 5B C9 71 28 A3 70 D8 95 10 44 65 02 0F 03 00 " +
            "EF CC 2C EE 70 4C C3 FC C6 C9 1D C5 B5 E5 60 18 " +
            "08 97 76 55 8F DC 63 FA CA F3 C1 CB D2 DA 80 59 " +
            "F5 88 2E A2 50 4F 1D 03 06 EE A7 BA 51 D1 B5 46 " +
            "B5 69 47 FB 0F 02 16 28 43 D8 0A B9 95 FF B6 46 " +
            "C7 1D E4 A1 EE C2 8A 33", 
            // After Chi
            "B9 3D 8B 14 45 96 5A DF C8 AA 30 A3 DB 5D 59 F7 " +
            "DC 4A A1 5D D7 EB 99 21 1D 88 CF 70 00 15 61 37 " +
            "26 BB 0C B3 D0 29 EA C5 5B 54 FD 0C D6 A5 84 D0 " +
            "21 90 45 9F B9 3B 2D 4B FC 2A DB 3D 62 06 95 5C " +
            "26 75 BF CF 69 7F B6 3B A3 2D AB CA 47 96 B8 BE " +
            "88 56 A2 EB 40 3E F5 9D 34 F5 93 0D 7F A8 EA B8 " +
            "1F 98 96 AF 46 99 05 49 45 5D 9D C3 9B 7D 33 9E " +
            "C1 FB D1 75 8E 63 78 FA 95 11 55 64 87 AE 23 00 " +
            "E7 DA 4E FE 7A 54 C0 1E 04 A9 9C 4F E5 E7 E0 19 " +
            "1D 97 72 71 8F D9 60 FA A0 3F E9 41 A2 9A 40 A5 " +
            "44 89 6E E3 5E 4D 1F 2B 44 7E AF BA C1 2C 15 00 " +
            "31 6C A3 FB 65 02 1E 19 73 58 00 BB 85 F2 A3 46 " +
            "C5 7B 65 B9 EF 52 2A 77", 
            // After Iota
            "B3 3D 8B 94 45 96 5A DF C8 AA 30 A3 DB 5D 59 F7 " +
            "DC 4A A1 5D D7 EB 99 21 1D 88 CF 70 00 15 61 37 " +
            "26 BB 0C B3 D0 29 EA C5 5B 54 FD 0C D6 A5 84 D0 " +
            "21 90 45 9F B9 3B 2D 4B FC 2A DB 3D 62 06 95 5C " +
            "26 75 BF CF 69 7F B6 3B A3 2D AB CA 47 96 B8 BE " +
            "88 56 A2 EB 40 3E F5 9D 34 F5 93 0D 7F A8 EA B8 " +
            "1F 98 96 AF 46 99 05 49 45 5D 9D C3 9B 7D 33 9E " +
            "C1 FB D1 75 8E 63 78 FA 95 11 55 64 87 AE 23 00 " +
            "E7 DA 4E FE 7A 54 C0 1E 04 A9 9C 4F E5 E7 E0 19 " +
            "1D 97 72 71 8F D9 60 FA A0 3F E9 41 A2 9A 40 A5 " +
            "44 89 6E E3 5E 4D 1F 2B 44 7E AF BA C1 2C 15 00 " +
            "31 6C A3 FB 65 02 1E 19 73 58 00 BB 85 F2 A3 46 " +
            "C5 7B 65 B9 EF 52 2A 77",
            // Round #12, After Theta
            "6E C2 7F 8A 5D EF 8D B8 6D 77 79 A0 37 91 A1 27 " +
            "82 FF 98 45 80 24 9C 67 55 E7 E8 E2 DA AC 16 A5 " +
            "55 9B 4C EC 3D C9 E2 99 86 AB 09 12 CE DC 53 B7 " +
            "84 4D 0C 9C 55 F7 D5 9B A2 9F E2 25 35 C9 90 1A " +
            "6E 1A 98 5D B3 C6 C1 A9 D0 0D EB 95 AA 76 B0 E2 " +
            "55 A9 56 F5 58 47 22 FA 91 28 DA 0E 93 64 12 68 " +
            "41 2D AF B7 11 56 00 0F 0D 32 BA 51 41 C4 44 0C " +
            "B2 DB 91 2A 63 83 70 A6 48 EE A1 7A 9F D7 F4 67 " +
            "42 07 07 FD 96 98 38 CE 5A 1C A5 57 B2 28 E5 5F " +
            "55 F8 55 E3 55 60 17 68 D3 1F A9 1E 4F 7A 48 F9 " +
            "99 76 9A FD 46 34 C8 4C E1 A3 E6 B9 2D E0 ED D0 " +
            "6F D9 9A E3 32 CD 1B 5F 3B 37 27 29 5F 4B D4 D4 " +
            "B6 5B 25 E6 02 B2 22 2B", 
            // After Rho
            "6E C2 7F 8A 5D EF 8D B8 DA EE F2 40 6F 22 43 4F " +
            "E0 3F 66 11 20 09 E7 99 CD 6A 51 5A 75 8E 2E AE " +
            "49 16 CF AC DA 64 62 EF E1 CC 3D 75 6B B8 9A 20 " +
            "C0 59 75 5F BD 49 D8 C4 86 E8 A7 78 49 4D 32 A4 " +
            "0D CC AE 59 E3 E0 54 37 07 2B 0E DD B0 5E A9 6A " +
            "AF 4A B5 AA C7 3A 12 D1 A0 45 A2 68 3B 4C 92 49 " +
            "BD 8D B0 02 78 08 6A 79 88 89 18 1A 64 74 A3 82 " +
            "95 B1 41 38 53 D9 ED 48 F5 3E AF E9 CF 90 DC 43 " +
            "A0 DF 12 13 C7 59 E8 E0 F2 2F 2D 8E D2 2B 59 94 " +
            "EC 02 AD 0A BF 6A BC 0A F9 D3 1F A9 1E 4F 7A 48 " +
            "20 33 65 DA 69 F6 1B D1 87 8F 9A E7 B6 80 B7 43 " +
            "2D 5B 73 5C A6 79 E3 EB 37 27 29 5F 4B D4 D4 3B " +
            "C8 8A ED 56 89 B9 80 AC", 
            // After Pi
            "6E C2 7F 8A 5D EF 8D B8 C0 59 75 5F BD 49 D8 C4 " +
            "BD 8D B0 02 78 08 6A 79 EC 02 AD 0A BF 6A BC 0A " +
            "C8 8A ED 56 89 B9 80 AC CD 6A 51 5A 75 8E 2E AE " +
            "07 2B 0E DD B0 5E A9 6A AF 4A B5 AA C7 3A 12 D1 " +
            "A0 DF 12 13 C7 59 E8 E0 2D 5B 73 5C A6 79 E3 EB " +
            "DA EE F2 40 6F 22 43 4F 86 E8 A7 78 49 4D 32 A4 " +
            "88 89 18 1A 64 74 A3 82 F9 D3 1F A9 1E 4F 7A 48 " +
            "20 33 65 DA 69 F6 1B D1 49 16 CF AC DA 64 62 EF " +
            "E1 CC 3D 75 6B B8 9A 20 A0 45 A2 68 3B 4C 92 49 " +
            "F2 2F 2D 8E D2 2B 59 94 37 27 29 5F 4B D4 D4 3B " +
            "E0 3F 66 11 20 09 E7 99 0D CC AE 59 E3 E0 54 37 " +
            "95 B1 41 38 53 D9 ED 48 F5 3E AF E9 CF 90 DC 43 " +
            "87 8F 9A E7 B6 80 B7 43", 
            // After Chi
            "53 46 FF 8A 1D EF AF 81 80 5B 78 57 3A 2B 4C C6 " +
            "BD 05 F0 56 78 99 6A DD CA 42 BF 82 EB 2C B1 1A " +
            "48 93 ED 03 29 B9 D0 E8 65 2A E0 78 32 AE 3C 3F " +
            "07 BE 0C CC B0 1F 41 4A A2 4A D4 E6 E7 1A 11 DA " +
            "60 FF 12 11 96 DF E4 E4 2F 5A 7D D9 26 29 62 AB " +
            "D2 EF EA 42 4B 12 C2 4D F7 BA A0 D9 53 46 6A EC " +
            "88 A9 78 48 05 C4 A2 13 23 1F 8D A9 18 4F 3A 46 " +
            "24 33 60 E2 69 BB 2B 71 49 17 4D A4 CA 20 62 A6 " +
            "B3 E6 30 F3 AB 9B D3 B4 A5 45 A2 39 32 98 16 62 " +
            "BA 3F EB 2E 42 0B 7B 50 97 EF 19 0E 6A 4C 4C 3B " +
            "70 0E 27 31 30 10 4E D1 6D C2 00 98 6F E0 44 34 " +
            "97 30 51 3E 63 D9 CE 48 95 0E CB F9 CF 99 9C DB " +
            "8A 4F 12 AF 75 60 A7 65", 
            // After Iota
            "D8 C6 FF 0A 1D EF AF 81 80 5B 78 57 3A 2B 4C C6 " +
            "BD 05 F0 56 78 99 6A DD CA 42 BF 82 EB 2C B1 1A " +
            "48 93 ED 03 29 B9 D0 E8 65 2A E0 78 32 AE 3C 3F " +
            "07 BE 0C CC B0 1F 41 4A A2 4A D4 E6 E7 1A 11 DA " +
            "60 FF 12 11 96 DF E4 E4 2F 5A 7D D9 26 29 62 AB " +
            "D2 EF EA 42 4B 12 C2 4D F7 BA A0 D9 53 46 6A EC " +
            "88 A9 78 48 05 C4 A2 13 23 1F 8D A9 18 4F 3A 46 " +
            "24 33 60 E2 69 BB 2B 71 49 17 4D A4 CA 20 62 A6 " +
            "B3 E6 30 F3 AB 9B D3 B4 A5 45 A2 39 32 98 16 62 " +
            "BA 3F EB 2E 42 0B 7B 50 97 EF 19 0E 6A 4C 4C 3B " +
            "70 0E 27 31 30 10 4E D1 6D C2 00 98 6F E0 44 34 " +
            "97 30 51 3E 63 D9 CE 48 95 0E CB F9 CF 99 9C DB " +
            "8A 4F 12 AF 75 60 A7 65",
            // Round #13, After Theta
            "DB 6B CC C0 5E FA 3D 2C 9C 66 B8 0D 33 45 33 3E " +
            "5F 59 15 A5 B4 CD 8A 5A D3 65 E6 4E D3 24 54 FC " +
            "43 34 D3 A5 FC 50 A2 D3 66 87 D3 B2 71 BB AE 92 " +
            "1B 83 CC 96 B9 71 3E B2 40 16 31 15 2B 4E F1 5D " +
            "79 D8 4B DD AE D7 01 02 24 FD 43 7F F3 C0 10 90 " +
            "D1 42 D9 88 08 07 50 E0 EB 87 60 83 5A 28 15 14 " +
            "6A F5 9D BB C9 90 42 94 3A 38 D4 65 20 47 DF A0 " +
            "2F 94 5E 44 BC 52 59 4A 4A BA 7E 6E 89 35 F0 0B " +
            "AF DB F0 A9 A2 F5 AC 4C 47 19 47 CA FE CC F6 E5 " +
            "A3 18 B2 E2 7A 03 9E B6 9C 48 27 A8 BF A5 3E 00 " +
            "73 A3 14 FB 73 05 DC 7C 71 FF C0 C2 66 8E 3B CC " +
            "75 6C B4 CD AF 8D 2E CF 8C 29 92 35 F7 91 79 3D " +
            "81 E8 2C 09 A0 89 D5 5E", 
            // After Rho
            "DB 6B CC C0 5E FA 3D 2C 38 CD 70 1B 66 8A 66 7C " +
            "57 56 45 29 6D B3 A2 D6 4D 42 C5 3F 5D 66 EE 34 " +
            "87 12 9D 1E A2 99 2E E5 1B B7 EB 2A 69 76 38 2D " +
            "6C 99 1B E7 23 BB 31 C8 17 90 45 4C C5 8A 53 7C " +
            "EC A5 6E D7 EB 00 81 3C 0C 01 49 D2 3F F4 37 0F " +
            "8F 16 CA 46 44 38 80 02 50 AC 1F 82 0D 6A A1 54 " +
            "DC 4D 86 14 A2 54 AB EF 8E BE 41 75 70 A8 CB 40 " +
            "22 5E A9 2C A5 17 4A 2F DC 12 6B E0 17 94 74 FD " +
            "3E 55 B4 9E 95 E9 75 1B FB F2 A3 8C 23 65 7F 66 " +
            "C0 D3 76 14 43 56 5C 6F 00 9C 48 27 A8 BF A5 3E " +
            "70 F3 CD 8D 52 EC CF 15 C7 FD 03 0B 9B 39 EE 30 " +
            "8E 8D B6 F9 B5 D1 E5 B9 29 92 35 F7 91 79 3D 8C " +
            "B5 57 20 3A 4B 02 68 62", 
            // After Pi
            "DB 6B CC C0 5E FA 3D 2C 6C 99 1B E7 23 BB 31 C8 " +
            "DC 4D 86 14 A2 54 AB EF C0 D3 76 14 43 56 5C 6F " +
            "B5 57 20 3A 4B 02 68 62 4D 42 C5 3F 5D 66 EE 34 " +
            "0C 01 49 D2 3F F4 37 0F 8F 16 CA 46 44 38 80 02 " +
            "3E 55 B4 9E 95 E9 75 1B 8E 8D B6 F9 B5 D1 E5 B9 " +
            "38 CD 70 1B 66 8A 66 7C 17 90 45 4C C5 8A 53 7C " +
            "8E BE 41 75 70 A8 CB 40 00 9C 48 27 A8 BF A5 3E " +
            "70 F3 CD 8D 52 EC CF 15 87 12 9D 1E A2 99 2E E5 " +
            "1B B7 EB 2A 69 76 38 2D 50 AC 1F 82 0D 6A A1 54 " +
            "FB F2 A3 8C 23 65 7F 66 29 92 35 F7 91 79 3D 8C " +
            "57 56 45 29 6D B3 A2 D6 EC A5 6E D7 EB 00 81 3C " +
            "22 5E A9 2C A5 17 4A 2F DC 12 6B E0 17 94 74 FD " +
            "C7 FD 03 0B 9B 39 EE 30", 
            // After Chi
            "4B 2F 48 D0 DE BE B7 0B 6C 0B 6B E7 62 B9 65 C8 " +
            "E9 49 86 3E AA 54 8B EF 8A FB BA D4 57 AE 49 63 " +
            "91 C7 33 1D 6A 03 68 A2 CE 54 47 3B 1D 6E 6E 34 " +
            "3C 40 7D 4A AE 35 42 16 0F 9E C8 27 64 28 00 A2 " +
            "7F 17 F5 98 DD CF 7F 1F 8E 8C BE 39 97 41 F4 B2 " +
            "B0 E3 70 2A 56 AA EE 7C 17 90 4D 4E 4D 9D 77 42 " +
            "FE DD C4 FD 22 E8 81 41 08 90 78 35 8C BD 85 56 " +
            "77 E3 C8 C9 D3 EC DE 15 C7 1A 89 9E A6 91 AF B5 " +
            "B0 E5 4B 26 4B 73 66 0F 50 AC 0B F1 9D 72 A1 DC " +
            "7D F2 2B 84 01 E5 7D 07 31 37 57 D7 D8 1F 2D 84 " +
            "55 0C C4 01 69 A4 E8 D5 30 A5 2C 17 F9 80 B5 EC " +
            "21 B3 A9 27 2D 3E C0 2F CC 10 2F C0 73 16 74 3B " +
            "6F 5C 29 DD 19 39 EF 18", 
            // After Iota
            "C0 2F 48 D0 DE BE B7 8B 6C 0B 6B E7 62 B9 65 C8 " +
            "E9 49 86 3E AA 54 8B EF 8A FB BA D4 57 AE 49 63 " +
            "91 C7 33 1D 6A 03 68 A2 CE 54 47 3B 1D 6E 6E 34 " +
            "3C 40 7D 4A AE 35 42 16 0F 9E C8 27 64 28 00 A2 " +
            "7F 17 F5 98 DD CF 7F 1F 8E 8C BE 39 97 41 F4 B2 " +
            "B0 E3 70 2A 56 AA EE 7C 17 90 4D 4E 4D 9D 77 42 " +
            "FE DD C4 FD 22 E8 81 41 08 90 78 35 8C BD 85 56 " +
            "77 E3 C8 C9 D3 EC DE 15 C7 1A 89 9E A6 91 AF B5 " +
            "B0 E5 4B 26 4B 73 66 0F 50 AC 0B F1 9D 72 A1 DC " +
            "7D F2 2B 84 01 E5 7D 07 31 37 57 D7 D8 1F 2D 84 " +
            "55 0C C4 01 69 A4 E8 D5 30 A5 2C 17 F9 80 B5 EC " +
            "21 B3 A9 27 2D 3E C0 2F CC 10 2F C0 73 16 74 3B " +
            "6F 5C 29 DD 19 39 EF 18",
            // Round #14, After Theta
            "78 DB 0A 93 56 F2 30 ED 93 AF 09 DD 80 46 C2 95 " +
            "B6 EE DD 96 71 E8 7C BD 8E 68 E5 28 D4 67 23 AF " +
            "84 45 65 9C AA B2 32 F2 76 A0 05 78 95 22 E9 52 " +
            "C3 E4 1F 70 4C CA E5 4B 50 39 93 8F BF 94 F7 F0 " +
            "7B 84 AA 64 5E 06 15 D3 9B 0E E8 B8 57 F0 AE E2 " +
            "08 17 32 69 DE E6 69 1A E8 34 2F 74 AF 62 D0 1F " +
            "A1 7A 9F 55 F9 54 76 13 0C 03 27 C9 0F 74 EF 9A " +
            "62 61 9E 48 13 5D 84 45 7F EE CB DD 2E DD 28 D3 " +
            "4F 41 29 1C A9 8C C1 52 0F 0B 50 59 46 CE 56 8E " +
            "79 61 74 78 82 2C 17 CB 24 B5 01 56 18 AE 77 D4 " +
            "ED F8 86 42 E1 E8 6F B3 CF 01 4E 2D 1B 7F 12 B1 " +
            "7E 14 F2 8F F6 82 37 7D C8 83 70 3C F0 DF 1E F7 " +
            "7A DE 7F 5C D9 88 B5 48", 
            // After Rho
            "78 DB 0A 93 56 F2 30 ED 27 5F 13 BA 01 8D 84 2B " +
            "AD 7B B7 65 1C 3A 5F AF 7D 36 F2 EA 88 56 8E 42 " +
            "95 95 91 27 2C 2A E3 54 57 29 92 2E 65 07 5A 80 " +
            "01 C7 A4 5C BE 34 4C FE 3C 54 CE E4 E3 2F E5 3D " +
            "42 55 32 2F 83 8A E9 3D EF 2A BE E9 80 8E 7B 05 " +
            "40 B8 90 49 F3 36 4F D3 7F A0 D3 BC D0 BD 8A 41 " +
            "AC CA A7 B2 9B 08 D5 FB E8 DE 35 19 06 4E 92 1F " +
            "A4 89 2E C2 22 B1 30 4F BB 5D BA 51 A6 FF DC 97 " +
            "85 23 95 31 58 EA 29 28 2B C7 87 05 A8 2C 23 67 " +
            "E5 62 39 2F 8C 0E 4F 90 D4 24 B5 01 56 18 AE 77 " +
            "BF CD B6 E3 1B 0A 85 A3 3E 07 38 B5 6C FC 49 C4 " +
            "8F 42 FE D1 5E F0 A6 CF 83 70 3C F0 DF 1E F7 C8 " +
            "2D 92 9E F7 1F 57 36 62", 
            // After Pi
            "78 DB 0A 93 56 F2 30 ED 01 C7 A4 5C BE 34 4C FE " +
            "AC CA A7 B2 9B 08 D5 FB E5 62 39 2F 8C 0E 4F 90 " +
            "2D 92 9E F7 1F 57 36 62 7D 36 F2 EA 88 56 8E 42 " +
            "EF 2A BE E9 80 8E 7B 05 40 B8 90 49 F3 36 4F D3 " +
            "85 23 95 31 58 EA 29 28 8F 42 FE D1 5E F0 A6 CF " +
            "27 5F 13 BA 01 8D 84 2B 3C 54 CE E4 E3 2F E5 3D " +
            "E8 DE 35 19 06 4E 92 1F D4 24 B5 01 56 18 AE 77 " +
            "BF CD B6 E3 1B 0A 85 A3 95 95 91 27 2C 2A E3 54 " +
            "57 29 92 2E 65 07 5A 80 7F A0 D3 BC D0 BD 8A 41 " +
            "2B C7 87 05 A8 2C 23 67 83 70 3C F0 DF 1E F7 C8 " +
            "AD 7B B7 65 1C 3A 5F AF 42 55 32 2F 83 8A E9 3D " +
            "A4 89 2E C2 22 B1 30 4F BB 5D BA 51 A6 FF DC 97 " +
            "3E 07 38 B5 6C FC 49 C4", 
            // After Chi
            "D4 D3 09 31 57 FA A1 EC 40 E7 BC 51 BA 32 46 FE " +
            "A4 5A 21 62 88 59 E5 99 B5 2B 39 2F CC AE 4F 1D " +
            "2C 96 3A BB B7 53 7A 70 7D A6 F2 EA FB 66 8A 90 " +
            "6A 29 BB D9 88 46 5B 2D 4A F8 FA 89 F5 26 C9 14 " +
            "F5 17 95 1B D8 EC 21 28 0D 4A F2 D0 5E 78 D7 CA " +
            "E7 D5 22 A3 05 CD 96 29 28 74 4E E4 B3 3F C9 5D " +
            "C3 17 37 FB 0F 4C 93 9F D4 36 B4 19 56 9D AE 7F " +
            "A7 CD 7A A7 F9 28 E4 B7 BD 15 D0 B7 BC 92 63 15 " +
            "57 6E 96 2F 4D 07 7B A6 FF 90 EB 4C 87 AF 5E C9 " +
            "3F 42 06 02 88 0C 23 73 C1 58 3E F8 9E 1B EF 48 " +
            "09 F3 BB A5 3C 0B 4F ED 59 01 A2 3E 07 C4 25 AD " +
            "A0 8B 2E 66 6A B1 31 0F 3A 25 3D 11 B6 FD CA BC " +
            "7C 03 38 BF EF 7C E9 D4", 
            // After Iota
            "5D 53 09 31 57 FA A1 6C 40 E7 BC 51 BA 32 46 FE " +
            "A4 5A 21 62 88 59 E5 99 B5 2B 39 2F CC AE 4F 1D " +
            "2C 96 3A BB B7 53 7A 70 7D A6 F2 EA FB 66 8A 90 " +
            "6A 29 BB D9 88 46 5B 2D 4A F8 FA 89 F5 26 C9 14 " +
            "F5 17 95 1B D8 EC 21 28 0D 4A F2 D0 5E 78 D7 CA " +
            "E7 D5 22 A3 05 CD 96 29 28 74 4E E4 B3 3F C9 5D " +
            "C3 17 37 FB 0F 4C 93 9F D4 36 B4 19 56 9D AE 7F " +
            "A7 CD 7A A7 F9 28 E4 B7 BD 15 D0 B7 BC 92 63 15 " +
            "57 6E 96 2F 4D 07 7B A6 FF 90 EB 4C 87 AF 5E C9 " +
            "3F 42 06 02 88 0C 23 73 C1 58 3E F8 9E 1B EF 48 " +
            "09 F3 BB A5 3C 0B 4F ED 59 01 A2 3E 07 C4 25 AD " +
            "A0 8B 2E 66 6A B1 31 0F 3A 25 3D 11 B6 FD CA BC " +
            "7C 03 38 BF EF 7C E9 D4",
            // Round #15, After Theta
            "7F B3 46 40 A0 8F FB F6 D6 7D 5D 4F AD A1 77 7A " +
            "8B 54 1A 63 BB 8D 3D 16 B0 11 78 02 90 4B 01 EB " +
            "5B 77 7C 50 99 ED 70 AE 5F 46 BD 9B 0C 13 D0 0A " +
            "FC B3 5A C7 9F D5 6A A9 65 F6 C1 88 C6 F2 11 9B " +
            "F0 2D D4 36 84 09 6F DE 7A AB B4 3B 70 C6 DD 14 " +
            "C5 35 6D D2 F2 B8 CC B3 BE EE AF FA A4 AC F8 D9 " +
            "EC 19 0C FA 3C 98 4B 10 D1 0C F5 34 0A 78 E0 89 " +
            "D0 2C 3C 4C D7 96 EE 69 9F F5 9F C6 4B E7 39 8F " +
            "C1 F4 77 31 5A 94 4A 22 D0 9E D0 4D B4 7B 86 46 " +
            "3A 78 47 2F D4 E9 6D 85 B6 B9 78 13 B0 A5 E5 96 " +
            "2B 13 F4 D4 CB 7E 15 77 CF 9B 43 20 10 57 14 29 " +
            "8F 85 15 67 59 65 E9 80 3F 1F 7C 3C EA 18 84 4A " +
            "0B E2 7E 54 C1 C2 E3 0A", 
            // After Rho
            "7F B3 46 40 A0 8F FB F6 AC FB BA 9E 5A 43 EF F4 " +
            "22 95 C6 D8 6E 63 8F C5 B9 14 B0 0E 1B 81 27 00 " +
            "6C 87 73 DD BA E3 83 CA C9 30 01 AD F0 65 D4 BB " +
            "75 FC 59 AD 96 CA 3F AB 66 99 7D 30 A2 B1 7C C4 " +
            "16 6A 1B C2 84 37 6F F8 DC 4D A1 B7 4A BB 03 67 " +
            "2D AE 69 93 96 C7 65 9E 67 FB BA BF EA 93 B2 E2 " +
            "D0 E7 C1 5C 82 60 CF 60 F0 C0 13 A3 19 EA 69 14 " +
            "A6 6B 4B F7 34 68 16 1E 8D 97 CE 73 1E 3F EB 3F " +
            "2E 46 8B 52 49 24 98 FE 43 23 68 4F E8 26 DA 3D " +
            "BD AD 50 07 EF E8 85 3A 96 B6 B9 78 13 B0 A5 E5 " +
            "55 DC AD 4C D0 53 2F FB 3C 6F 0E 81 40 5C 51 A4 " +
            "B1 B0 E2 2C AB 2C 1D F0 1F 7C 3C EA 18 84 4A 3F " +
            "B8 C2 82 B8 1F 55 B0 F0", 
            // After Pi
            "7F B3 46 40 A0 8F FB F6 75 FC 59 AD 96 CA 3F AB " +
            "D0 E7 C1 5C 82 60 CF 60 BD AD 50 07 EF E8 85 3A " +
            "B8 C2 82 B8 1F 55 B0 F0 B9 14 B0 0E 1B 81 27 00 " +
            "DC 4D A1 B7 4A BB 03 67 2D AE 69 93 96 C7 65 9E " +
            "2E 46 8B 52 49 24 98 FE B1 B0 E2 2C AB 2C 1D F0 " +
            "AC FB BA 9E 5A 43 EF F4 66 99 7D 30 A2 B1 7C C4 " +
            "F0 C0 13 A3 19 EA 69 14 96 B6 B9 78 13 B0 A5 E5 " +
            "55 DC AD 4C D0 53 2F FB 6C 87 73 DD BA E3 83 CA " +
            "C9 30 01 AD F0 65 D4 BB 67 FB BA BF EA 93 B2 E2 " +
            "43 23 68 4F E8 26 DA 3D 1F 7C 3C EA 18 84 4A 3F " +
            "22 95 C6 D8 6E 63 8F C5 16 6A 1B C2 84 37 6F F8 " +
            "A6 6B 4B F7 34 68 16 1E 8D 97 CE 73 1E 3F EB 3F " +
            "3C 6F 0E 81 40 5C 51 A4", 
            // After Chi
            "FF B0 C6 10 A0 AF 3B B6 58 F4 49 AE FB 42 3F B1 " +
            "D0 A5 43 E4 92 75 FF A0 FA 9C 14 47 4F 62 CE 3C " +
            "B8 8E 9B 15 09 15 B4 F9 98 B6 F8 0E 8F C5 43 98 " +
            "DE 0D 23 F7 03 9B 9B 07 BC 1E 09 BF 34 CF 60 9E " +
            "26 42 9B 50 59 A5 BA FE F5 F9 E3 9D EB 16 1D 97 " +
            "3C BB B8 1D 43 09 EE E4 60 AF D5 68 A0 A1 F8 25 " +
            "B1 88 17 A7 D9 A9 63 0E 3E 95 AB EA 19 B0 65 E1 " +
            "17 DC E8 6C 70 E3 3F FB 4A 4C C9 CF B0 71 A1 8A " +
            "C9 30 41 ED F0 41 9C A6 7B A7 AE 1F FA 13 B2 E0 " +
            "23 A0 2B 5A 4A 45 5B FD 9E 4C 3C CA 58 80 1E 0E " +
            "82 94 86 ED 5E 2B 9F C3 1F FE 9F C2 8E 20 86 D9 " +
            "96 03 4B 77 74 28 06 9E 8F 07 0E 2B 30 1C 65 7E " +
            "28 05 17 83 C0 48 31 9C", 
            // After Iota
            "FC 30 C6 10 A0 AF 3B 36 58 F4 49 AE FB 42 3F B1 " +
            "D0 A5 43 E4 92 75 FF A0 FA 9C 14 47 4F 62 CE 3C " +
            "B8 8E 9B 15 09 15 B4 F9 98 B6 F8 0E 8F C5 43 98 " +
            "DE 0D 23 F7 03 9B 9B 07 BC 1E 09 BF 34 CF 60 9E " +
            "26 42 9B 50 59 A5 BA FE F5 F9 E3 9D EB 16 1D 97 " +
            "3C BB B8 1D 43 09 EE E4 60 AF D5 68 A0 A1 F8 25 " +
            "B1 88 17 A7 D9 A9 63 0E 3E 95 AB EA 19 B0 65 E1 " +
            "17 DC E8 6C 70 E3 3F FB 4A 4C C9 CF B0 71 A1 8A " +
            "C9 30 41 ED F0 41 9C A6 7B A7 AE 1F FA 13 B2 E0 " +
            "23 A0 2B 5A 4A 45 5B FD 9E 4C 3C CA 58 80 1E 0E " +
            "82 94 86 ED 5E 2B 9F C3 1F FE 9F C2 8E 20 86 D9 " +
            "96 03 4B 77 74 28 06 9E 8F 07 0E 2B 30 1C 65 7E " +
            "28 05 17 83 C0 48 31 9C",
            // Round #16, After Theta
            "71 E2 BE 81 E6 B5 0E E9 A8 3F F1 A6 9A 2A 07 2E " +
            "7D E5 21 E2 5F 30 E7 0C 12 CE DB 88 AB 1A F4 7D " +
            "D6 A9 09 DA 78 48 CB 5E 15 64 80 9F C9 DF 76 47 " +
            "2E C6 9B FF 62 F3 A3 98 11 5E 6B B9 F9 8A 78 32 " +
            "CE 10 54 9F BD DD 80 BF 9B DE 71 52 9A 4B 62 30 " +
            "B1 69 C0 8C 05 13 DB 3B 90 64 6D 60 C1 C9 C0 BA " +
            "1C C8 75 A1 14 EC 7B A2 D6 C7 64 25 FD C8 5F A0 " +
            "79 FB 7A A3 01 BE 40 5C C7 9E B1 5E F6 6B 94 55 " +
            "39 FB F9 E5 91 29 A4 39 D6 E7 CC 19 37 56 AA 4C " +
            "CB F2 E4 95 AE 3D 61 BC F0 6B AE 05 29 DD 61 A9 " +
            "0F 46 FE 7C 18 31 AA 1C EF 35 27 CA EF 48 BE 46 " +
            "3B 43 29 71 B9 6D 1E 32 67 55 C1 E4 D4 64 5F 3F " +
            "46 22 85 4C B1 15 4E 3B", 
            // After Rho
            "71 E2 BE 81 E6 B5 0E E9 50 7F E2 4D 35 55 0E 5C " +
            "5F 79 88 F8 17 CC 39 43 AA 41 DF 27 E1 BC 8D B8 " +
            "43 5A F6 B2 4E 4D D0 C6 99 FC 6D 77 54 41 06 F8 " +
            "F9 2F 36 3F 8A E9 62 BC 4C 84 D7 5A 6E BE 22 9E " +
            "08 AA CF DE 6E C0 5F 67 24 06 B3 E9 1D 27 A5 B9 " +
            "89 4D 03 66 2C 98 D8 DE EB 42 92 B5 81 05 27 03 " +
            "0B A5 60 DF 13 E5 40 AE 91 BF 40 AD 8F C9 4A FA " +
            "D1 00 5F 20 AE BC 7D BD BD EC D7 28 AB 8E 3D 63 " +
            "BF 3C 32 85 34 27 67 3F 55 26 EB 73 E6 8C 1B 2B " +
            "27 8C 77 59 9E BC D2 B5 A9 F0 6B AE 05 29 DD 61 " +
            "A8 72 3C 18 F9 F3 61 C4 BD D7 9C 28 BF 23 F9 1A " +
            "67 28 25 2E B7 CD 43 66 55 C1 E4 D4 64 5F 3F 67 " +
            "D3 8E 91 48 21 53 6C 85", 
            // After Pi
            "71 E2 BE 81 E6 B5 0E E9 F9 2F 36 3F 8A E9 62 BC " +
            "0B A5 60 DF 13 E5 40 AE 27 8C 77 59 9E BC D2 B5 " +
            "D3 8E 91 48 21 53 6C 85 AA 41 DF 27 E1 BC 8D B8 " +
            "24 06 B3 E9 1D 27 A5 B9 89 4D 03 66 2C 98 D8 DE " +
            "BF 3C 32 85 34 27 67 3F 67 28 25 2E B7 CD 43 66 " +
            "50 7F E2 4D 35 55 0E 5C 4C 84 D7 5A 6E BE 22 9E " +
            "91 BF 40 AD 8F C9 4A FA A9 F0 6B AE 05 29 DD 61 " +
            "A8 72 3C 18 F9 F3 61 C4 43 5A F6 B2 4E 4D D0 C6 " +
            "99 FC 6D 77 54 41 06 F8 EB 42 92 B5 81 05 27 03 " +
            "55 26 EB 73 E6 8C 1B 2B 55 C1 E4 D4 64 5F 3F 67 " +
            "5F 79 88 F8 17 CC 39 43 08 AA CF DE 6E C0 5F 67 " +
            "D1 00 5F 20 AE BC 7D BD BD EC D7 28 AB 8E 3D 63 " +
            "BD D7 9C 28 BF 23 F9 1A", 
            // After Chi
            "73 62 FE 41 F7 B1 0E EB DD 27 21 3F 06 F1 F0 AD " +
            "DB A7 E0 DF 32 A6 6C AE 07 EC 59 D8 58 18 D0 DD " +
            "5B 83 91 76 29 1B 0C 91 23 08 DF 21 C1 24 D5 FE " +
            "12 36 83 68 0D 00 82 98 C9 4D 06 4C AF 50 D8 9E " +
            "37 7D E8 84 74 17 EB A7 63 2E 05 E6 AB CE 63 67 " +
            "C1 44 E2 E8 B4 14 46 3C 64 C4 FC 58 6E 9E B7 9F " +
            "91 BD 54 BD 77 1B 6A 7E F9 FD A9 EB 01 2D D3 79 " +
            "A4 F2 29 0A B3 59 41 46 21 58 64 32 CF 49 F1 C5 " +
            "8D D8 04 35 32 C9 1E D0 EB 83 96 31 81 56 03 47 " +
            "57 3C F9 51 EC 8C DB AB CD 65 ED 91 74 5F 39 5F " +
            "8E 79 98 D8 97 F0 19 DB 24 46 4F D6 6F C2 5F 25 " +
            "D1 13 57 20 BA 9D BD A5 FF C4 D7 F8 AB 42 3D 22 " +
            "BD 55 DB 2E D7 23 BF 3E", 
            // After Iota
            "71 E2 FE 41 F7 B1 0E 6B DD 27 21 3F 06 F1 F0 AD " +
            "DB A7 E0 DF 32 A6 6C AE 07 EC 59 D8 58 18 D0 DD " +
            "5B 83 91 76 29 1B 0C 91 23 08 DF 21 C1 24 D5 FE " +
            "12 36 83 68 0D 00 82 98 C9 4D 06 4C AF 50 D8 9E " +
            "37 7D E8 84 74 17 EB A7 63 2E 05 E6 AB CE 63 67 " +
            "C1 44 E2 E8 B4 14 46 3C 64 C4 FC 58 6E 9E B7 9F " +
            "91 BD 54 BD 77 1B 6A 7E F9 FD A9 EB 01 2D D3 79 " +
            "A4 F2 29 0A B3 59 41 46 21 58 64 32 CF 49 F1 C5 " +
            "8D D8 04 35 32 C9 1E D0 EB 83 96 31 81 56 03 47 " +
            "57 3C F9 51 EC 8C DB AB CD 65 ED 91 74 5F 39 5F " +
            "8E 79 98 D8 97 F0 19 DB 24 46 4F D6 6F C2 5F 25 " +
            "D1 13 57 20 BA 9D BD A5 FF C4 D7 F8 AB 42 3D 22 " +
            "BD 55 DB 2E D7 23 BF 3E",
            // Round #17, After Theta
            "99 1B 5F BC 14 89 AE 05 92 27 F9 23 7E 84 45 42 " +
            "1A C4 98 0F DE 1A F5 E5 67 F4 3C AC AD DF E1 D2 " +
            "43 09 D8 AC F7 86 E8 75 CB F1 7E DC 22 1C 75 90 " +
            "5D 36 5B 74 75 75 37 77 08 2E 7E 9C 43 EC 41 D5 " +
            "57 65 8D F0 81 D0 DA A8 7B A4 4C 3C 75 53 87 83 " +
            "29 BD 43 15 57 2C E6 52 2B C4 24 44 16 EB 02 70 " +
            "50 DE 2C 6D 9B A7 F3 35 99 E5 CC 9F F4 EA E2 76 " +
            "BC 78 60 D0 6D C4 A5 A2 C9 A1 C5 CF 2C 71 51 AB " +
            "C2 D8 DC 29 4A BC AB 3F 2A E0 EE E1 6D EA 9A 0C " +
            "37 24 9C 25 19 4B EA A4 D5 EF A4 4B AA C2 DD BB " +
            "66 80 39 25 74 C8 B9 B5 6B 46 97 CA 17 B7 EA CA " +
            "10 70 2F F0 56 21 24 EE 9F DC B2 8C 5E 85 0C 2D " +
            "A5 DF 92 F4 09 BE 5B DA", 
            // After Rho
            "99 1B 5F BC 14 89 AE 05 24 4F F2 47 FC 08 8B 84 " +
            "06 31 E6 83 B7 46 7D B9 FA 1D 2E 7D 46 CF C3 DA " +
            "37 44 AF 1B 4A C0 66 BD 2D C2 51 07 B9 1C EF C7 " +
            "45 57 57 77 73 D7 65 B3 35 82 8B 1F E7 10 7B 50 " +
            "B2 46 F8 40 68 6D D4 AB 75 38 B8 47 CA C4 53 37 " +
            "4A E9 1D AA B8 62 31 97 C0 AD 10 93 10 59 AC 0B " +
            "69 DB 3C 9D AF 81 F2 66 D5 C5 ED 32 CB 99 3F E9 " +
            "E8 36 E2 52 51 5E 3C 30 9F 59 E2 A2 56 93 43 8B " +
            "3B 45 89 77 F5 47 18 9B 4D 06 15 70 F7 F0 36 75 " +
            "49 9D F4 86 84 B3 24 63 BB D5 EF A4 4B AA C2 DD " +
            "E7 D6 9A 01 E6 94 D0 21 AF 19 5D 2A 5F DC AA 2B " +
            "02 EE 05 DE 2A 84 C4 1D DC B2 8C 5E 85 0C 2D 9F " +
            "96 76 E9 B7 24 7D 82 EF", 
            // After Pi
            "99 1B 5F BC 14 89 AE 05 45 57 57 77 73 D7 65 B3 " +
            "69 DB 3C 9D AF 81 F2 66 49 9D F4 86 84 B3 24 63 " +
            "96 76 E9 B7 24 7D 82 EF FA 1D 2E 7D 46 CF C3 DA " +
            "75 38 B8 47 CA C4 53 37 4A E9 1D AA B8 62 31 97 " +
            "3B 45 89 77 F5 47 18 9B 02 EE 05 DE 2A 84 C4 1D " +
            "24 4F F2 47 FC 08 8B 84 35 82 8B 1F E7 10 7B 50 " +
            "D5 C5 ED 32 CB 99 3F E9 BB D5 EF A4 4B AA C2 DD " +
            "E7 D6 9A 01 E6 94 D0 21 37 44 AF 1B 4A C0 66 BD " +
            "2D C2 51 07 B9 1C EF C7 C0 AD 10 93 10 59 AC 0B " +
            "4D 06 15 70 F7 F0 36 75 DC B2 8C 5E 85 0C 2D 9F " +
            "06 31 E6 83 B7 46 7D B9 B2 46 F8 40 68 6D D4 AB " +
            "E8 36 E2 52 51 5E 3C 30 9F 59 E2 A2 56 93 43 8B " +
            "AF 19 5D 2A 5F DC AA 2B", 
            // After Chi
            "B1 93 77 34 98 89 3C 41 45 53 97 75 73 E5 61 B2 " +
            "FF B9 35 AC 8F CD 70 EA 40 94 E2 8E 94 33 08 63 " +
            "D2 32 E9 F4 47 2B C3 5D F0 DC 2B D5 76 ED E3 5A " +
            "44 3C 38 12 8F C1 5B 3F 4A 43 19 22 B2 E2 F5 93 " +
            "C3 54 A3 56 B1 0C 1B 59 07 CE 95 DC A2 84 D4 38 " +
            "E4 0A 96 67 F4 81 8F 2D 1F 92 89 9B E7 32 BB 44 " +
            "91 C7 FD 33 6F 8D 2F C9 BB DC 8F E2 53 A2 C9 59 " +
            "F6 56 93 19 E5 84 A0 71 F7 69 AF 8B 4A 81 66 B5 " +
            "20 C0 54 67 5E BC FD B3 50 1D 98 9D 10 55 A5 81 " +
            "6E 42 36 71 BD 30 74 55 D4 30 DC 5A 34 10 A4 DD " +
            "4E 01 E4 91 A6 54 55 A9 A5 0F F8 E0 6E EC 97 20 " +
            "C8 36 FF 5A 58 12 94 10 9F 79 40 23 F6 91 16 1B " +
            "1F 5F 45 6A 17 F5 2A 29", 
            // After Iota
            "31 93 77 34 98 89 3C C1 45 53 97 75 73 E5 61 B2 " +
            "FF B9 35 AC 8F CD 70 EA 40 94 E2 8E 94 33 08 63 " +
            "D2 32 E9 F4 47 2B C3 5D F0 DC 2B D5 76 ED E3 5A " +
            "44 3C 38 12 8F C1 5B 3F 4A 43 19 22 B2 E2 F5 93 " +
            "C3 54 A3 56 B1 0C 1B 59 07 CE 95 DC A2 84 D4 38 " +
            "E4 0A 96 67 F4 81 8F 2D 1F 92 89 9B E7 32 BB 44 " +
            "91 C7 FD 33 6F 8D 2F C9 BB DC 8F E2 53 A2 C9 59 " +
            "F6 56 93 19 E5 84 A0 71 F7 69 AF 8B 4A 81 66 B5 " +
            "20 C0 54 67 5E BC FD B3 50 1D 98 9D 10 55 A5 81 " +
            "6E 42 36 71 BD 30 74 55 D4 30 DC 5A 34 10 A4 DD " +
            "4E 01 E4 91 A6 54 55 A9 A5 0F F8 E0 6E EC 97 20 " +
            "C8 36 FF 5A 58 12 94 10 9F 79 40 23 F6 91 16 1B " +
            "1F 5F 45 6A 17 F5 2A 29",
            // Round #18, After Theta
            "EF 33 15 C2 ED CB D3 94 A1 53 7A 1C B1 1F 35 5B " +
            "F6 C4 CF 06 DE F3 EB EB 2D 09 B9 F6 C8 4A E0 82 " +
            "22 4E 53 A5 97 76 BD 24 2E 7C 49 23 03 AF 0C 0F " +
            "A0 3C D5 7B 4D 3B 0F D6 43 3E E3 88 E3 DC 6E 92 " +
            "AE C9 F8 2E ED 75 F3 B8 F7 B2 2F 8D 72 D9 AA 41 " +
            "3A AA F4 91 81 C3 60 78 FB 92 64 F2 25 C8 EF AD " +
            "98 BA 07 99 3E B3 B4 C8 D6 41 D4 9A 0F DB 21 B8 " +
            "06 2A 29 48 35 D9 DE 08 29 C9 CD 7D 3F C3 89 E0 " +
            "C4 C0 B9 0E 9C 46 A9 5A 59 60 62 37 41 6B 3E 80 " +
            "03 DF 6D 09 E1 49 9C B4 24 4C 66 0B E4 4D DA A4 " +
            "90 A1 86 67 D3 16 BA FC 41 0F 15 89 AC 16 C3 C9 " +
            "C1 4B 05 F0 09 2C 0F 11 F2 E4 1B 5B AA E8 FE FA " +
            "EF 23 FF 3B C7 A8 54 50", 
            // After Rho
            "EF 33 15 C2 ED CB D3 94 42 A7 F4 38 62 3F 6A B6 " +
            "3D F1 B3 81 F7 FC FA BA AC 04 2E D8 92 90 6B 8F " +
            "B4 EB 25 11 71 9A 2A BD 32 F0 CA F0 E0 C2 97 34 " +
            "BD D7 B4 F3 60 0D CA 53 E4 90 CF 38 E2 38 B7 9B " +
            "64 7C 97 F6 BA 79 5C D7 AD 1A 74 2F FB D2 28 97 " +
            "D3 51 A5 8F 0C 1C 06 C3 B7 EE 4B 92 C9 97 20 BF " +
            "C8 F4 99 A5 45 C6 D4 3D B6 43 70 AD 83 A8 35 1F " +
            "A4 9A 6C 6F 04 03 95 14 FB 7E 86 13 C1 53 92 9B " +
            "D7 81 D3 28 55 8B 18 38 1F C0 2C 30 B1 9B A0 35 " +
            "89 93 76 E0 BB 2D 21 3C A4 24 4C 66 0B E4 4D DA " +
            "E8 F2 43 86 1A 9E 4D 5B 07 3D 54 24 B2 5A 0C 27 " +
            "78 A9 00 3E 81 E5 21 22 E4 1B 5B AA E8 FE FA F2 " +
            "15 D4 FB C8 FF CE 31 2A", 
            // After Pi
            "EF 33 15 C2 ED CB D3 94 BD D7 B4 F3 60 0D CA 53 " +
            "C8 F4 99 A5 45 C6 D4 3D 89 93 76 E0 BB 2D 21 3C " +
            "15 D4 FB C8 FF CE 31 2A AC 04 2E D8 92 90 6B 8F " +
            "AD 1A 74 2F FB D2 28 97 D3 51 A5 8F 0C 1C 06 C3 " +
            "D7 81 D3 28 55 8B 18 38 78 A9 00 3E 81 E5 21 22 " +
            "42 A7 F4 38 62 3F 6A B6 E4 90 CF 38 E2 38 B7 9B " +
            "B6 43 70 AD 83 A8 35 1F A4 24 4C 66 0B E4 4D DA " +
            "E8 F2 43 86 1A 9E 4D 5B B4 EB 25 11 71 9A 2A BD " +
            "32 F0 CA F0 E0 C2 97 34 B7 EE 4B 92 C9 97 20 BF " +
            "1F C0 2C 30 B1 9B A0 35 E4 1B 5B AA E8 FE FA F2 " +
            "3D F1 B3 81 F7 FC FA BA 64 7C 97 F6 BA 79 5C D7 " +
            "A4 9A 6C 6F 04 03 95 14 FB 7E 86 13 C1 53 92 9B " +
            "07 3D 54 24 B2 5A 0C 27", 
            // After Chi
            "AF 13 1C C6 E8 09 C7 B8 BC D4 D2 B3 DA 24 EB 53 " +
            "DC B0 10 AD 01 04 C4 3F 63 B0 72 E2 BB 2C E3 A8 " +
            "05 10 5B F9 FF CA 39 69 FE 45 AF 58 96 9C 6D CF " +
            "A9 9A 26 0F AA 51 30 AF FB 79 A5 99 8C 78 27 C1 " +
            "53 85 FD E8 47 9B 52 B5 79 B3 50 19 E8 A7 21 32 " +
            "50 E4 C4 BD 63 BF 6A B2 E4 B4 C3 7A EA 7C FF 5B " +
            "FE 91 73 2D 93 B2 35 1E A6 21 F8 5E 6B C5 6F 7E " +
            "4C E2 48 86 9A 9E D8 52 31 E5 24 13 78 8F 0A 36 " +
            "3A F0 EE D0 D0 CA 17 34 57 F5 18 18 81 F3 7A 7D " +
            "0F 20 08 21 A0 9B A0 38 E6 0B 91 4A 68 BE 6F F2 " +
            "BD 73 DB 88 F3 FE 7B BA 3F 18 15 E6 7B 29 5E 5C " +
            "A0 9B 3C 4B 36 0B 99 30 C3 BE 25 92 84 F7 60 03 " +
            "47 31 50 52 BA 5B 08 62", 
            // After Iota
            "A5 93 1C C6 E8 09 C7 B8 BC D4 D2 B3 DA 24 EB 53 " +
            "DC B0 10 AD 01 04 C4 3F 63 B0 72 E2 BB 2C E3 A8 " +
            "05 10 5B F9 FF CA 39 69 FE 45 AF 58 96 9C 6D CF " +
            "A9 9A 26 0F AA 51 30 AF FB 79 A5 99 8C 78 27 C1 " +
            "53 85 FD E8 47 9B 52 B5 79 B3 50 19 E8 A7 21 32 " +
            "50 E4 C4 BD 63 BF 6A B2 E4 B4 C3 7A EA 7C FF 5B " +
            "FE 91 73 2D 93 B2 35 1E A6 21 F8 5E 6B C5 6F 7E " +
            "4C E2 48 86 9A 9E D8 52 31 E5 24 13 78 8F 0A 36 " +
            "3A F0 EE D0 D0 CA 17 34 57 F5 18 18 81 F3 7A 7D " +
            "0F 20 08 21 A0 9B A0 38 E6 0B 91 4A 68 BE 6F F2 " +
            "BD 73 DB 88 F3 FE 7B BA 3F 18 15 E6 7B 29 5E 5C " +
            "A0 9B 3C 4B 36 0B 99 30 C3 BE 25 92 84 F7 60 03 " +
            "47 31 50 52 BA 5B 08 62",
            // Round #19, After Theta
            "DD CD 06 59 D4 CB BB BF 66 1C 9E 9E 1E 12 30 40 " +
            "9C B6 69 93 57 D3 95 40 6E 71 94 55 AC 36 98 36 " +
            "51 D3 10 6F 61 63 45 A2 86 1B B5 C7 AA 5E 11 C8 " +
            "73 52 6A 22 6E 67 EB BC BB 7F DC A7 DA AF 76 BE " +
            "5E 44 1B 5F 50 81 29 2B 2D 70 1B 8F 76 0E 5D F9 " +
            "28 BA DE 22 5F 7D 16 B5 3E 7C 8F 57 2E 4A 24 48 " +
            "BE 97 0A 13 C5 65 64 61 AB E0 1E E9 7C DF 14 E0 " +
            "18 21 03 10 04 37 A4 99 49 BB 3E 8C 44 4D 76 31 " +
            "E0 38 A2 FD 14 FC CC 27 17 F3 61 26 D7 24 2B 02 " +
            "02 E1 EE 96 B7 81 DB A6 B2 C8 DA DC F6 17 13 39 " +
            "C5 2D C1 17 CF 3C 07 BD E5 D0 59 CB BF 1F 85 4F " +
            "E0 9D 45 75 60 DC C8 4F CE 7F C3 25 93 ED 1B 9D " +
            "13 F2 1B C4 24 F2 74 A9", 
            // After Rho
            "DD CD 06 59 D4 CB BB BF CC 38 3C 3D 3D 24 60 80 " +
            "A7 6D DA E4 D5 74 25 10 6A 83 69 E3 16 47 59 C5 " +
            "1B 2B 12 8D 9A 86 78 0B AC EA 15 81 6C B8 51 7B " +
            "26 E2 76 B6 CE 3B 27 A5 EF EE 1F F7 A9 F6 AB 9D " +
            "A2 8D 2F A8 C0 94 15 2F D0 95 DF 02 B7 F1 68 E7 " +
            "45 D1 F5 16 F9 EA B3 A8 20 F9 F0 3D 5E B9 28 91 " +
            "98 28 2E 23 0B F3 BD 54 BE 29 C0 57 C1 3D D2 F9 " +
            "08 82 1B D2 4C 8C 90 01 18 89 9A EC 62 92 76 7D " +
            "B4 9F 82 9F F9 04 1C 47 15 81 8B F9 30 93 6B 92 " +
            "70 DB 54 20 DC DD F2 36 39 B2 C8 DA DC F6 17 13 " +
            "1C F4 16 B7 04 5F 3C F3 95 43 67 2D FF 7E 14 3E " +
            "BC B3 A8 0E 8C 1B F9 09 7F C3 25 93 ED 1B 9D CE " +
            "5D EA 84 FC 06 31 89 3C", 
            // After Pi
            "DD CD 06 59 D4 CB BB BF 26 E2 76 B6 CE 3B 27 A5 " +
            "98 28 2E 23 0B F3 BD 54 70 DB 54 20 DC DD F2 36 " +
            "5D EA 84 FC 06 31 89 3C 6A 83 69 E3 16 47 59 C5 " +
            "D0 95 DF 02 B7 F1 68 E7 45 D1 F5 16 F9 EA B3 A8 " +
            "B4 9F 82 9F F9 04 1C 47 BC B3 A8 0E 8C 1B F9 09 " +
            "CC 38 3C 3D 3D 24 60 80 EF EE 1F F7 A9 F6 AB 9D " +
            "BE 29 C0 57 C1 3D D2 F9 39 B2 C8 DA DC F6 17 13 " +
            "1C F4 16 B7 04 5F 3C F3 1B 2B 12 8D 9A 86 78 0B " +
            "AC EA 15 81 6C B8 51 7B 20 F9 F0 3D 5E B9 28 91 " +
            "15 81 8B F9 30 93 6B 92 7F C3 25 93 ED 1B 9D CE " +
            "A7 6D DA E4 D5 74 25 10 A2 8D 2F A8 C0 94 15 2F " +
            "08 82 1B D2 4C 8C 90 01 18 89 9A EC 62 92 76 7D " +
            "95 43 67 2D FF 7E 14 3E", 
            // After Chi
            "45 C5 0E 58 D5 0B 23 EF 46 31 26 B6 1A 37 65 87 " +
            "95 08 AE FF 09 D3 B4 5C F0 DE 56 21 0C 17 C0 B5 " +
            "7F C8 F4 5A 0C 01 8D 3C 6F C3 49 F7 5E 4D CA CD " +
            "60 9B DD 8B B7 F5 64 A0 4D F1 DD 16 FD F1 52 A0 " +
            "F6 9F C3 7E EB 40 1C 83 2C A7 3E 0E 2D AB D9 2B " +
            "DC 39 FC 3D 7D 2D 30 E0 EE 7C 17 7F B5 34 AE 9F " +
            "BA 6D D6 72 C1 34 FA 19 F9 BA E0 D2 E5 D6 57 13 " +
            "3F 32 15 75 84 8D B7 EE 1B 3A F2 B1 88 87 50 8B " +
            "B9 EA 1E 41 4C BA 12 79 4A BB D4 3F 93 B1 BC DD " +
            "15 A9 99 F5 22 17 0B 93 DB 03 20 93 89 23 9C BE " +
            "AF 6F CA B6 D9 7C A5 10 B2 84 AF 84 E2 86 73 53 " +
            "8D C0 7E D3 D1 E0 90 03 3A A5 02 2C 62 92 57 7D " +
            "95 C3 42 25 FF FE 04 11", 
            // After Iota
            "4F C5 0E D8 D5 0B 23 6F 46 31 26 B6 1A 37 65 87 " +
            "95 08 AE FF 09 D3 B4 5C F0 DE 56 21 0C 17 C0 B5 " +
            "7F C8 F4 5A 0C 01 8D 3C 6F C3 49 F7 5E 4D CA CD " +
            "60 9B DD 8B B7 F5 64 A0 4D F1 DD 16 FD F1 52 A0 " +
            "F6 9F C3 7E EB 40 1C 83 2C A7 3E 0E 2D AB D9 2B " +
            "DC 39 FC 3D 7D 2D 30 E0 EE 7C 17 7F B5 34 AE 9F " +
            "BA 6D D6 72 C1 34 FA 19 F9 BA E0 D2 E5 D6 57 13 " +
            "3F 32 15 75 84 8D B7 EE 1B 3A F2 B1 88 87 50 8B " +
            "B9 EA 1E 41 4C BA 12 79 4A BB D4 3F 93 B1 BC DD " +
            "15 A9 99 F5 22 17 0B 93 DB 03 20 93 89 23 9C BE " +
            "AF 6F CA B6 D9 7C A5 10 B2 84 AF 84 E2 86 73 53 " +
            "8D C0 7E D3 D1 E0 90 03 3A A5 02 2C 62 92 57 7D " +
            "95 C3 42 25 FF FE 04 11",
            // Round #20, After Theta
            "EA 29 08 41 6B 64 C5 1C 44 84 BA 4D 53 29 29 28 " +
            "F7 5F 2E D1 3B 11 D4 59 11 0B 22 79 DC A5 07 22 " +
            "3E EB 1C 25 00 24 03 45 CA 2F 4F 6E E0 22 2C BE " +
            "62 2E 41 70 FE EB 28 0F 2F A6 5D 38 CF 33 32 A5 " +
            "17 4A B7 26 3B F2 DB 14 6D 84 D6 71 21 8E 57 52 " +
            "79 D5 FA A4 C3 42 D6 93 EC C9 8B 84 FC 2A E2 30 " +
            "D8 3A 56 5C F3 F6 9A 1C 18 6F 94 8A 35 64 90 84 " +
            "7E 11 FD 0A 88 A8 39 97 BE D6 F4 28 36 E8 B6 F8 " +
            "BB 5F 82 BA 05 A4 5E D6 28 EC 54 11 A1 73 DC D8 " +
            "F4 7C ED AD F2 A5 CC 04 9A 20 C8 EC 85 06 12 C7 " +
            "0A 83 CC 2F 67 13 43 63 B0 31 33 7F AB 98 3F FC " +
            "EF 97 FE FD E3 22 F0 06 DB 70 76 74 B2 20 90 EA " +
            "D4 E0 AA 5A F3 DB 8A 68", 
            // After Rho
            "EA 29 08 41 6B 64 C5 1C 88 08 75 9B A6 52 52 50 " +
            "FD 97 4B F4 4E 04 75 D6 5D 7A 20 12 B1 20 92 C7 " +
            "20 19 28 F2 59 E7 28 01 06 2E C2 E2 AB FC F2 E4 " +
            "04 E7 BF 8E F2 20 E6 12 E9 8B 69 17 CE F3 8C 4C " +
            "A5 5B 93 1D F9 6D 8A 0B 78 25 D5 46 68 1D 17 E2 " +
            "CC AB D6 27 1D 16 B2 9E C3 B0 27 2F 12 F2 AB 88 " +
            "E2 9A B7 D7 E4 C0 D6 B1 C8 20 09 31 DE 28 15 6B " +
            "05 44 D4 9C 4B BF 88 7E 51 6C D0 6D F1 7D AD E9 " +
            "50 B7 80 D4 CB 7A F7 4B 6E 6C 14 76 AA 88 D0 39 " +
            "94 99 80 9E AF BD 55 BE C7 9A 20 C8 EC 85 06 12 " +
            "0C 8D 29 0C 32 BF 9C 4D C3 C6 CC FC AD 62 FE F0 " +
            "FD D2 BF 7F 5C 04 DE E0 70 76 74 B2 20 90 EA DB " +
            "22 1A 35 B8 AA D6 FC B6", 
            // After Pi
            "EA 29 08 41 6B 64 C5 1C 04 E7 BF 8E F2 20 E6 12 " +
            "E2 9A B7 D7 E4 C0 D6 B1 94 99 80 9E AF BD 55 BE " +
            "22 1A 35 B8 AA D6 FC B6 5D 7A 20 12 B1 20 92 C7 " +
            "78 25 D5 46 68 1D 17 E2 CC AB D6 27 1D 16 B2 9E " +
            "50 B7 80 D4 CB 7A F7 4B FD D2 BF 7F 5C 04 DE E0 " +
            "88 08 75 9B A6 52 52 50 E9 8B 69 17 CE F3 8C 4C " +
            "C8 20 09 31 DE 28 15 6B C7 9A 20 C8 EC 85 06 12 " +
            "0C 8D 29 0C 32 BF 9C 4D 20 19 28 F2 59 E7 28 01 " +
            "06 2E C2 E2 AB FC F2 E4 C3 B0 27 2F 12 F2 AB 88 " +
            "6E 6C 14 76 AA 88 D0 39 70 76 74 B2 20 90 EA DB " +
            "FD 97 4B F4 4E 04 75 D6 A5 5B 93 1D F9 6D 8A 0B " +
            "05 44 D4 9C 4B BF 88 7E 51 6C D0 6D F1 7D AD E9 " +
            "C3 C6 CC FC AD 62 FE F0", 
            // After Chi
            "08 31 08 10 6F A4 D5 BD 10 E6 BF 86 F9 1D E7 1C " +
            "C0 98 82 F7 E4 82 7E B1 5C B8 88 DF EE 9D 54 B6 " +
            "26 DC 82 36 3A D6 DE B4 D9 F0 22 33 A4 22 32 DB " +
            "68 31 D5 96 AA 75 52 A3 61 EB E9 0C 09 12 BA 3E " +
            "50 9F 80 D4 6A 5A F7 4C DD D7 6A 3B 14 19 DB C0 " +
            "88 28 75 BB B6 5A 43 73 EE 11 49 DF EE 76 8E 5C " +
            "C0 25 00 35 CC 12 8D 26 47 9A 74 5B 68 C5 44 02 " +
            "6D 0E 21 08 7A 1E 10 41 E1 89 0D FF 49 E5 21 09 " +
            "2A 62 D2 B2 03 F4 A2 D5 D3 A2 47 AF 12 E2 81 4A " +
            "6E 65 1C 36 F3 EF D0 39 76 50 B6 B2 82 88 38 3F " +
            "FD 93 0F 74 4C 96 75 A2 F5 73 93 7C 49 2D AF 8A " +
            "87 C6 D8 0C 47 BD DA 6E 6D 7D D3 6D B3 79 AC EF " +
            "C3 8E 5C F5 1C 0B 74 F9", 
            // After Iota
            "89 B1 08 90 6F A4 D5 3D 10 E6 BF 86 F9 1D E7 1C " +
            "C0 98 82 F7 E4 82 7E B1 5C B8 88 DF EE 9D 54 B6 " +
            "26 DC 82 36 3A D6 DE B4 D9 F0 22 33 A4 22 32 DB " +
            "68 31 D5 96 AA 75 52 A3 61 EB E9 0C 09 12 BA 3E " +
            "50 9F 80 D4 6A 5A F7 4C DD D7 6A 3B 14 19 DB C0 " +
            "88 28 75 BB B6 5A 43 73 EE 11 49 DF EE 76 8E 5C " +
            "C0 25 00 35 CC 12 8D 26 47 9A 74 5B 68 C5 44 02 " +
            "6D 0E 21 08 7A 1E 10 41 E1 89 0D FF 49 E5 21 09 " +
            "2A 62 D2 B2 03 F4 A2 D5 D3 A2 47 AF 12 E2 81 4A " +
            "6E 65 1C 36 F3 EF D0 39 76 50 B6 B2 82 88 38 3F " +
            "FD 93 0F 74 4C 96 75 A2 F5 73 93 7C 49 2D AF 8A " +
            "87 C6 D8 0C 47 BD DA 6E 6D 7D D3 6D B3 79 AC EF " +
            "C3 8E 5C F5 1C 0B 74 F9",
            // Round #21, After Theta
            "39 C4 EE D0 4B 79 E1 B6 BF F1 0A CE 69 08 32 38 " +
            "19 05 87 E1 4B 6C 7F 50 2E 3C 3B 36 0E E5 F4 DD " +
            "E6 9E 8B 1B 67 1C A4 E7 69 85 C4 73 80 FF 06 50 " +
            "C7 26 60 DE 3A 60 87 87 B8 76 EC 1A A6 FC BB DF " +
            "22 1B 33 3D 8A 22 57 27 1D 95 63 16 49 D3 A1 93 " +
            "38 5D 93 FB 92 87 77 F8 41 06 FC 97 7E 63 5B 78 " +
            "19 B8 05 23 63 FC 8C C7 35 1E C7 B2 88 BD E4 69 " +
            "AD 4C 28 25 27 D4 6A 12 51 FC EB BF 6D 38 15 82 " +
            "85 75 67 FA 93 E1 77 F1 0A 3F 42 B9 BD 0C 80 AB " +
            "1C E1 AF DF 13 97 70 52 B6 12 BF 9F DF 42 42 6C " +
            "4D E6 E9 34 68 4B 41 29 5A 64 26 34 D9 38 7A AE " +
            "5E 5B DD 1A E8 53 DB 8F 1F F9 60 84 53 01 0C 84 " +
            "03 CC 55 D8 41 C1 0E AA", 
            // After Rho
            "39 C4 EE D0 4B 79 E1 B6 7E E3 15 9C D3 10 64 70 " +
            "46 C1 61 F8 12 DB 1F 54 50 4E DF ED C2 B3 63 E3 " +
            "E3 20 3D 37 F7 5C DC 38 07 F8 6F 00 95 56 48 3C " +
            "E6 AD 03 76 78 78 6C 02 37 AE 1D BB 86 29 FF EE " +
            "8D 99 1E 45 91 AB 13 91 1D 3A D9 51 39 66 91 34 " +
            "C7 E9 9A DC 97 3C BC C3 E1 05 19 F0 5F FA 8D 6D " +
            "18 19 E3 67 3C CE C0 2D 7B C9 D3 6A 3C 8E 65 11 " +
            "92 13 6A 35 89 56 26 94 7F DB 70 2A 04 A3 F8 D7 " +
            "4C 7F 32 FC 2E BE B0 EE C0 55 85 1F A1 DC 5E 06 " +
            "12 4E 8A 23 FC F5 7B E2 6C B6 12 BF 9F DF 42 42 " +
            "05 A5 34 99 A7 D3 A0 2D 6A 91 99 D0 64 E3 E8 B9 " +
            "6B AB 5B 03 7D 6A FB D1 F9 60 84 53 01 0C 84 1F " +
            "83 EA 00 73 15 76 50 B0", 
            // After Pi
            "39 C4 EE D0 4B 79 E1 B6 E6 AD 03 76 78 78 6C 02 " +
            "18 19 E3 67 3C CE C0 2D 12 4E 8A 23 FC F5 7B E2 " +
            "83 EA 00 73 15 76 50 B0 50 4E DF ED C2 B3 63 E3 " +
            "1D 3A D9 51 39 66 91 34 C7 E9 9A DC 97 3C BC C3 " +
            "4C 7F 32 FC 2E BE B0 EE 6B AB 5B 03 7D 6A FB D1 " +
            "7E E3 15 9C D3 10 64 70 37 AE 1D BB 86 29 FF EE " +
            "7B C9 D3 6A 3C 8E 65 11 6C B6 12 BF 9F DF 42 42 " +
            "05 A5 34 99 A7 D3 A0 2D E3 20 3D 37 F7 5C DC 38 " +
            "07 F8 6F 00 95 56 48 3C E1 05 19 F0 5F FA 8D 6D " +
            "C0 55 85 1F A1 DC 5E 06 F9 60 84 53 01 0C 84 1F " +
            "46 C1 61 F8 12 DB 1F 54 8D 99 1E 45 91 AB 13 91 " +
            "92 13 6A 35 89 56 26 94 7F DB 70 2A 04 A3 F8 D7 " +
            "6A 91 99 D0 64 E3 E8 B9", 
            // After Chi
            "21 D4 0E D1 4F FF 61 9B E4 EB 0B 76 B8 49 57 C0 " +
            "99 B9 E3 37 3D CC C0 3D 2A 4A 64 A3 B6 FC DA E4 " +
            "45 C3 01 55 25 76 5C B0 92 8F DD 61 44 AB 4F 20 " +
            "15 2C F9 71 11 E4 91 18 E4 69 D3 DF C6 7C F7 D2 " +
            "5C 3B B6 10 AC 2F B0 CC 66 9B 5B 13 44 2E 6B C5 " +
            "36 A2 D7 DC EB 96 64 61 33 98 1D 2E 05 78 FD AC " +
            "7A C8 F7 6A 1C 8E C5 3C 16 F4 13 BB CF DF 06 12 " +
            "04 A9 3C BA A3 FA 3B A3 03 25 2D C7 BD F4 59 79 " +
            "07 A8 EB 0F 35 52 1A 3E D8 25 19 B0 5F FA 0D 74 " +
            "C2 55 BC 3B 57 8C 06 26 FD B8 C6 53 01 0E 84 1B " +
            "54 C3 01 C8 1A 8F 3B 50 E0 51 0E 4F 95 0A CB D2 " +
            "92 13 E3 E5 E9 16 26 BC 7B 9B 10 02 16 BB EF 93 " +
            "E3 89 87 D5 E5 C3 E8 38", 
            // After Iota
            "A1 54 0E D1 4F FF 61 1B E4 EB 0B 76 B8 49 57 C0 " +
            "99 B9 E3 37 3D CC C0 3D 2A 4A 64 A3 B6 FC DA E4 " +
            "45 C3 01 55 25 76 5C B0 92 8F DD 61 44 AB 4F 20 " +
            "15 2C F9 71 11 E4 91 18 E4 69 D3 DF C6 7C F7 D2 " +
            "5C 3B B6 10 AC 2F B0 CC 66 9B 5B 13 44 2E 6B C5 " +
            "36 A2 D7 DC EB 96 64 61 33 98 1D 2E 05 78 FD AC " +
            "7A C8 F7 6A 1C 8E C5 3C 16 F4 13 BB CF DF 06 12 " +
            "04 A9 3C BA A3 FA 3B A3 03 25 2D C7 BD F4 59 79 " +
            "07 A8 EB 0F 35 52 1A 3E D8 25 19 B0 5F FA 0D 74 " +
            "C2 55 BC 3B 57 8C 06 26 FD B8 C6 53 01 0E 84 1B " +
            "54 C3 01 C8 1A 8F 3B 50 E0 51 0E 4F 95 0A CB D2 " +
            "92 13 E3 E5 E9 16 26 BC 7B 9B 10 02 16 BB EF 93 " +
            "E3 89 87 D5 E5 C3 E8 38",
            // Round #22, After Theta
            "D3 D8 3C 79 71 8A D4 DF 2C 28 59 BB 5C 54 CC 84 " +
            "0F 88 33 3C 19 36 20 BA 14 E4 16 80 AB F0 C3 15 " +
            "38 B6 3D A2 3F 3F 88 D9 E0 03 EF C9 7A DE FA E4 " +
            "DD EF AB BC F5 F9 0A 5C 72 58 03 D4 E2 86 17 55 " +
            "62 95 C4 33 B1 23 A9 3D 1B EE 67 E4 5E 67 BF AC " +
            "44 2E E5 74 D5 E3 D1 A5 FB 5B 4F E3 E1 65 66 E8 " +
            "EC F9 27 61 38 74 25 BB 28 5A 61 98 D2 D3 1F E3 " +
            "79 DC 00 4D B9 B3 EF CA 71 A9 1F 6F 83 81 EC BD " +
            "CF 6B B9 C2 D1 4F 81 7A 4E 14 C9 BB 7B 00 ED F3 " +
            "FC FB CE 18 4A 80 1F D7 80 CD FA A4 1B 47 50 72 " +
            "26 4F 33 60 24 FA 8E 94 28 92 5C 82 71 17 50 96 " +
            "04 22 33 EE CD EC C6 3B 45 35 62 21 0B B7 F6 62 " +
            "9E FC BB 22 FF 8A 3C 51", 
            // After Rho
            "D3 D8 3C 79 71 8A D4 DF 59 50 B2 76 B9 A8 98 09 " +
            "03 E2 0C 4F 86 0D 88 EE 0A 3F 5C 41 41 6E 01 B8 " +
            "F9 41 CC C6 B1 ED 11 FD AC E7 AD 4F 0E 3E F0 9E " +
            "CA 5B 9F AF C0 D5 FD BE 95 1C D6 00 B5 B8 E1 45 " +
            "4A E2 99 D8 91 D4 1E B1 F6 CB BA E1 7E 46 EE 75 " +
            "25 72 29 A7 AB 1E 8F 2E A1 EF 6F 3D 8D 87 97 99 " +
            "09 C3 A1 2B D9 65 CF 3F A7 3F C6 51 B4 C2 30 A5 " +
            "A6 DC D9 77 E5 3C 6E 80 DE 06 03 D9 7B E3 52 3F " +
            "57 38 FA 29 50 EF 79 2D F6 79 27 8A E4 DD 3D 80 " +
            "F0 E3 9A 7F DF 19 43 09 72 80 CD FA A4 1B 47 50 " +
            "3B 52 9A 3C CD 80 91 E8 A2 48 72 09 C6 5D 40 59 " +
            "40 64 C6 BD 99 DD 78 87 35 62 21 0B B7 F6 62 45 " +
            "4F 94 27 FF AE C8 BF 22", 
            // After Pi
            "D3 D8 3C 79 71 8A D4 DF CA 5B 9F AF C0 D5 FD BE " +
            "09 C3 A1 2B D9 65 CF 3F F0 E3 9A 7F DF 19 43 09 " +
            "4F 94 27 FF AE C8 BF 22 0A 3F 5C 41 41 6E 01 B8 " +
            "F6 CB BA E1 7E 46 EE 75 25 72 29 A7 AB 1E 8F 2E " +
            "57 38 FA 29 50 EF 79 2D 40 64 C6 BD 99 DD 78 87 " +
            "59 50 B2 76 B9 A8 98 09 95 1C D6 00 B5 B8 E1 45 " +
            "A7 3F C6 51 B4 C2 30 A5 72 80 CD FA A4 1B 47 50 " +
            "3B 52 9A 3C CD 80 91 E8 F9 41 CC C6 B1 ED 11 FD " +
            "AC E7 AD 4F 0E 3E F0 9E A1 EF 6F 3D 8D 87 97 99 " +
            "F6 79 27 8A E4 DD 3D 80 35 62 21 0B B7 F6 62 45 " +
            "03 E2 0C 4F 86 0D 88 EE 4A E2 99 D8 91 D4 1E B1 " +
            "A6 DC D9 77 E5 3C 6E 80 DE 06 03 D9 7B E3 52 3F " +
            "A2 48 72 09 C6 5D 40 59", 
            // After Chi
            "D2 58 1C 79 68 AA D6 DE 3A 7B 85 FB C6 CD FD BE " +
            "06 D7 84 AB F9 A5 73 1D 60 AB 82 7F 8E 1B 03 D4 " +
            "47 97 A4 79 2E 9D 96 02 0B 0F 5D 47 C0 76 00 B2 " +
            "A4 C3 68 E9 2E A7 9E 74 25 36 2D 33 22 0E 8F AC " +
            "5D 23 E2 69 10 CD 78 15 B4 A4 64 1D A7 DD 96 C2 " +
            "7B 73 B2 27 B9 EA 88 A9 C5 9C DF AA B5 A1 A6 15 " +
            "AE 6D D4 55 FD 42 A0 0D 32 80 ED B8 94 33 4F 51 " +
            "BF 5E DE 3C C9 90 F0 AC F8 49 8E F6 30 6C 16 FC " +
            "FA F7 AD CD 6E 66 D8 9E A0 ED 6F 3C 9E A5 D5 DC " +
            "3E 78 EB 4E E4 D4 2C 38 31 C4 00 02 B9 E4 82 47 " +
            "A7 FE 4C 68 E2 25 E8 EE 12 E0 9B 50 8B 17 0E 8E " +
            "86 94 A9 77 61 20 6E C0 DF A4 0F 9F 7B E3 DA 99 " +
            "EA 48 E3 99 D7 8D 56 48", 
            // After Iota
            "D3 58 1C F9 68 AA D6 DE 3A 7B 85 FB C6 CD FD BE " +
            "06 D7 84 AB F9 A5 73 1D 60 AB 82 7F 8E 1B 03 D4 " +
            "47 97 A4 79 2E 9D 96 02 0B 0F 5D 47 C0 76 00 B2 " +
            "A4 C3 68 E9 2E A7 9E 74 25 36 2D 33 22 0E 8F AC " +
            "5D 23 E2 69 10 CD 78 15 B4 A4 64 1D A7 DD 96 C2 " +
            "7B 73 B2 27 B9 EA 88 A9 C5 9C DF AA B5 A1 A6 15 " +
            "AE 6D D4 55 FD 42 A0 0D 32 80 ED B8 94 33 4F 51 " +
            "BF 5E DE 3C C9 90 F0 AC F8 49 8E F6 30 6C 16 FC " +
            "FA F7 AD CD 6E 66 D8 9E A0 ED 6F 3C 9E A5 D5 DC " +
            "3E 78 EB 4E E4 D4 2C 38 31 C4 00 02 B9 E4 82 47 " +
            "A7 FE 4C 68 E2 25 E8 EE 12 E0 9B 50 8B 17 0E 8E " +
            "86 94 A9 77 61 20 6E C0 DF A4 0F 9F 7B E3 DA 99 " +
            "EA 48 E3 99 D7 8D 56 48",
            // Round #23, After Theta
            "23 DE E9 70 36 66 D5 23 91 03 C3 F1 B6 6B 93 28 " +
            "69 4D 53 70 6B BA E5 B1 E5 9D C2 7E 0A 05 AD B2 " +
            "50 64 AE 08 3D B0 14 9C FB 89 A8 CE 9E BA 03 4F " +
            "0F BB 2E E3 5E 01 F0 E2 4A AC FA E8 B0 11 19 00 " +
            "D8 15 A2 68 94 D3 D6 73 A3 57 6E 6C B4 F0 14 5C " +
            "8B F5 47 AE E7 26 8B 54 6E E4 99 A0 C5 07 C8 83 " +
            "C1 F7 03 8E 6F 5D 36 A1 B7 B6 AD B9 10 2D E1 37 " +
            "A8 AD D4 4D DA BD 72 32 08 CF 7B 7F 6E A0 15 01 " +
            "51 8F EB C7 1E C0 B6 08 CF 77 B8 E7 0C BA 43 70 " +
            "BB 4E AB 4F 60 CA 82 5E 26 37 0A 73 AA C9 00 D9 " +
            "57 78 B9 E1 BC E9 EB 13 B9 98 DD 5A FB B1 60 18 " +
            "E9 0E 7E AC F3 3F F8 6C 5A 92 4F 9E FF FD 74 FF " +
            "FD BB E9 E8 C4 A0 D4 D6", 
            // After Rho
            "23 DE E9 70 36 66 D5 23 22 07 86 E3 6D D7 26 51 " +
            "5A D3 14 DC 9A 6E 79 6C 50 D0 2A 5B DE 29 EC A7 " +
            "81 A5 E0 84 22 73 45 E8 EC A9 3B F0 B4 9F 88 EA " +
            "32 EE 15 00 2F FE B0 EB 80 12 AB 3E 3A 6C 44 06 " +
            "0A 51 34 CA 69 EB 39 EC 4F C1 35 7A E5 C6 46 0B " +
            "5A AC 3F 72 3D 37 59 A4 0F BA 91 67 82 16 1F 20 " +
            "70 7C EB B2 09 0D BE 1F 5A C2 6F 6E 6D 5B 73 21 " +
            "26 ED 5E 39 19 D4 56 EA FE DC 40 2B 02 10 9E F7 " +
            "FD D8 03 D8 16 21 EA 71 21 B8 E7 3B DC 73 06 DD " +
            "59 D0 6B D7 69 F5 09 4C D9 26 37 0A 73 AA C9 00 " +
            "AF 4F 5C E1 E5 86 F3 A6 E4 62 76 6B ED C7 82 61 " +
            "DD C1 8F 75 FE 07 9F 2D 92 4F 9E FF FD 74 FF 5A " +
            "B5 75 FF 6E 3A 3A 31 28", 
            // After Pi
            "23 DE E9 70 36 66 D5 23 32 EE 15 00 2F FE B0 EB " +
            "70 7C EB B2 09 0D BE 1F 59 D0 6B D7 69 F5 09 4C " +
            "B5 75 FF 6E 3A 3A 31 28 50 D0 2A 5B DE 29 EC A7 " +
            "4F C1 35 7A E5 C6 46 0B 5A AC 3F 72 3D 37 59 A4 " +
            "FD D8 03 D8 16 21 EA 71 DD C1 8F 75 FE 07 9F 2D " +
            "22 07 86 E3 6D D7 26 51 80 12 AB 3E 3A 6C 44 06 " +
            "5A C2 6F 6E 6D 5B 73 21 D9 26 37 0A 73 AA C9 00 " +
            "AF 4F 5C E1 E5 86 F3 A6 81 A5 E0 84 22 73 45 E8 " +
            "EC A9 3B F0 B4 9F 88 EA 0F BA 91 67 82 16 1F 20 " +
            "21 B8 E7 3B DC 73 06 DD 92 4F 9E FF FD 74 FF 5A " +
            "5A D3 14 DC 9A 6E 79 6C 0A 51 34 CA 69 EB 39 EC " +
            "26 ED 5E 39 19 D4 56 EA FE DC 40 2B 02 10 9E F7 " +
            "E4 62 76 6B ED C7 82 61", 
            // After Chi
            "63 CE 03 C2 36 67 DB 37 3B 6E 15 45 4F 0E B1 AB " +
            "D4 59 7F 9A 1B 07 8E 3F 5B 5A 6B C7 6D B1 CD 4F " +
            "A5 55 EB 6E 33 A2 11 E0 40 FC 20 5B C6 18 F5 03 " +
            "EA 91 35 F2 E7 C6 E4 5A 5A AD B3 57 D5 31 4C A8 " +
            "FD C8 23 D2 16 09 8A F3 D2 C0 9A 55 DF C1 9D 25 " +
            "78 C7 C2 A3 28 C4 15 70 01 36 BB 3E 28 CC CC 06 " +
            "7C 8B 27 8F E9 5F 41 87 D9 26 B5 08 7B FB CD 51 " +
            "2F 5F 75 FD F7 AE B3 A0 82 B7 60 83 20 73 52 E8 " +
            "CC A9 5D E8 E8 FE 88 37 9D FD 89 A3 A3 12 E6 22 " +
            "20 18 87 3B DE 70 06 7D FE 47 85 8F 69 F8 77 58 " +
            "7E 7F 5E ED 8A 7A 3F 6E D2 41 34 C8 6B EB B1 F9 " +
            "26 CF 68 79 F4 13 56 EA E4 4D 40 BF 10 38 E7 FB " +
            "E4 62 56 69 8C 46 82 E1", 
            // After Iota
            "6B 4E 03 42 36 67 DB B7 3B 6E 15 45 4F 0E B1 AB " +
            "D4 59 7F 9A 1B 07 8E 3F 5B 5A 6B C7 6D B1 CD 4F " +
            "A5 55 EB 6E 33 A2 11 E0 40 FC 20 5B C6 18 F5 03 " +
            "EA 91 35 F2 E7 C6 E4 5A 5A AD B3 57 D5 31 4C A8 " +
            "FD C8 23 D2 16 09 8A F3 D2 C0 9A 55 DF C1 9D 25 " +
            "78 C7 C2 A3 28 C4 15 70 01 36 BB 3E 28 CC CC 06 " +
            "7C 8B 27 8F E9 5F 41 87 D9 26 B5 08 7B FB CD 51 " +
            "2F 5F 75 FD F7 AE B3 A0 82 B7 60 83 20 73 52 E8 " +
            "CC A9 5D E8 E8 FE 88 37 9D FD 89 A3 A3 12 E6 22 " +
            "20 18 87 3B DE 70 06 7D FE 47 85 8F 69 F8 77 58 " +
            "7E 7F 5E ED 8A 7A 3F 6E D2 41 34 C8 6B EB B1 F9 " +
            "26 CF 68 79 F4 13 56 EA E4 4D 40 BF 10 38 E7 FB " +
            "E4 62 56 69 8C 46 82 E1"
        };
    }
}
