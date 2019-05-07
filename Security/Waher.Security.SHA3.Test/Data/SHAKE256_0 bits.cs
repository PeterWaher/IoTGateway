using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Test vectors available in:
    /// https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHAKE256_Msg0.pdf
    /// </summary>
    public partial class SHAKE256_Tests
    {
        private static readonly string[] States0Bits = new string[]
        {
            // Xor'd state (in bytes)
            "1F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // Round #0
            // After Theta
            "1E 00 00 00 00 00 00 00 1F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "3E 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 " +
            "1F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 3E 00 00 00 00 00 00 00 " +
            "01 00 00 00 00 00 00 00 1F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "3E 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 " +
            "1F 00 00 00 00 00 00 80 00 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 3E 00 00 00 00 00 00 00 " +
            "01 00 00 00 00 00 00 00 1F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "3E 00 00 00 00 00 00 00",
            // After Rho
            "1E 00 00 00 00 00 00 00 3E 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 20 00 00 00 00 00 00 00 00 " +
            "00 00 00 F0 01 00 00 00 00 00 00 00 10 00 00 00 " +
            "00 00 00 00 00 F0 01 00 20 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 E0 03 00 00 00 00 " +
            "08 00 00 00 00 00 00 00 00 7C 00 00 00 00 00 00 " +
            "00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 1F 00 00 00 00 00 00 00 02 00 00 " +
            "00 00 00 00 00 F0 03 00 00 40 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 7C 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 10 00 00 00 00 00 00 00 00 " +
            "00 80 0F 00 00 00 00 00",
            // After Pi
            "1E 00 00 00 00 00 00 00 00 00 00 00 00 F0 01 00 " +
            "00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 " +
            "00 80 0F 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 E0 03 00 00 00 00 08 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F0 03 00 00 00 00 00 00 00 00 10 " +
            "3E 00 00 00 00 00 00 00 20 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 00 00 00 F0 01 00 00 00 " +
            "00 00 00 00 10 00 00 00 00 7C 00 00 00 00 00 00 " +
            "00 40 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 20 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 1F 00 00 00 00 00 00 00 02 00 00 " +
            "7C 00 00 00 00 00 00 00",
            // After Chi
            "1E 00 00 00 00 04 00 00 00 00 00 00 00 F0 01 00 " +
            "00 80 0F 00 00 04 00 00 1E 00 00 00 00 00 00 00 " +
            "00 80 0F 00 00 F0 01 00 08 00 00 00 00 00 00 00 " +
            "00 00 E0 03 00 F0 03 00 08 00 00 00 00 00 00 10 " +
            "00 00 00 00 00 F0 03 00 00 00 E0 03 00 00 00 10 " +
            "3E 00 00 00 00 00 00 00 20 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 3E 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 00 7C 00 F0 01 00 00 00 " +
            "00 00 00 00 10 00 00 00 00 7C 00 00 00 00 00 00 " +
            "00 40 00 F0 01 00 00 00 00 00 00 00 10 00 00 00 " +
            "00 00 00 00 00 1F 00 20 00 00 00 00 00 00 00 00 " +
            "7C 00 00 00 00 1F 00 00 00 00 00 00 00 02 00 20 " +
            "7C 00 00 00 00 00 00 00",
            // After Iota 
            "1F 00 00 00 00 04 00 00 00 00 00 00 00 F0 01 00 " +
            "00 80 0F 00 00 04 00 00 1E 00 00 00 00 00 00 00 " +
            "00 80 0F 00 00 F0 01 00 08 00 00 00 00 00 00 00 " +
            "00 00 E0 03 00 F0 03 00 08 00 00 00 00 00 00 10 " +
            "00 00 00 00 00 F0 03 00 00 00 E0 03 00 00 00 10 " +
            "3E 00 00 00 00 00 00 00 20 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 3E 3E 00 00 00 00 00 00 " +
            "00 00 04 00 00 00 00 00 00 7C 00 F0 01 00 00 00 " +
            "00 00 00 00 10 00 00 00 00 7C 00 00 00 00 00 00 " +
            "00 40 00 F0 01 00 00 00 00 00 00 00 10 00 00 00 " +
            "00 00 00 00 00 1F 00 20 00 00 00 00 00 00 00 00 " +
            "7C 00 00 00 00 1F 00 00 00 00 00 00 00 02 00 20 " +
            "7C 00 00 00 00 00 00 00",
            // Round #1
            // After Theta
            "23 FC 2B 04 30 F4 05 10 C1 84 17 F0 01 DD 01 00 " +
            "60 42 EF E3 13 E0 05 40 92 FC DC 07 20 FB 03 30 " +
            "72 06 0F 10 02 34 02 60 34 FC 2B 04 30 F0 05 10 " +
            "C1 84 F7 F3 01 DD 03 00 68 C2 E0 E3 13 E4 05 50 " +
            "8C FC DC 07 20 0B 00 30 72 86 E0 13 02 C4 03 70 " +
            "02 FC 2B 04 30 F0 05 10 E1 BA 17 F0 01 2D 00 00 " +
            "60 C2 E4 E3 13 E4 05 40 B2 C2 DC 07 20 FB 03 30 " +
            "72 86 04 10 02 C4 03 60 3C 80 2B F4 31 F0 05 10 " +
            "C1 84 17 F0 11 2D 00 00 60 BE E0 E3 13 E4 05 40 " +
            "8C BC DC F7 21 FB 03 30 72 86 00 10 12 C4 03 60 " +
            "3C FC 2B 04 30 EF 05 30 C1 84 17 F0 01 2D 00 00 " +
            "1C C2 E0 E3 13 FB 05 40 8C FC DC 07 20 F9 03 10 " +
            "0E 86 00 10 02 C4 03 60",
            // After Rho
            "23 FC 2B 04 30 F4 05 10 82 09 2F E0 03 BA 03 00 " +
            "98 D0 FB F8 04 78 01 10 B2 3F 00 23 C9 CF 7D 00 " +
            "A0 11 00 93 33 78 80 10 00 03 5F 00 41 C3 BF 42 " +
            "3F 1F D0 3D 00 10 4C 78 14 9A 30 F8 F8 04 79 01 " +
            "7E EE 03 90 05 00 18 46 3C 00 27 67 08 3E 21 40 " +
            "10 E0 5F 21 80 81 2F 80 00 84 EB 5E C0 07 B4 00 " +
            "1F 9F 20 2F 00 02 13 26 F6 07 60 64 85 B9 0F 40 " +
            "08 01 E2 01 30 39 43 02 E8 63 E0 0B 20 78 00 57 " +
            "02 3E A2 05 00 20 98 F0 02 20 30 5F F0 F1 09 F2 " +
            "7F 00 86 91 97 FB 3E 64 60 72 86 00 10 12 C4 03 " +
            "17 C0 F0 F0 AF 10 C0 BC 04 13 5E C0 07 B4 00 00 " +
            "43 18 7C 7C 62 BF 00 88 FC DC 07 20 F9 03 10 8C " +
            "00 98 83 21 00 84 00 F1",
            // After Pi
            "23 FC 2B 04 30 F4 05 10 3F 1F D0 3D 00 10 4C 78 " +
            "1F 9F 20 2F 00 02 13 26 7F 00 86 91 97 FB 3E 64 " +
            "00 98 83 21 00 84 00 F1 B2 3F 00 23 C9 CF 7D 00 " +
            "3C 00 27 67 08 3E 21 40 10 E0 5F 21 80 81 2F 80 " +
            "02 3E A2 05 00 20 98 F0 43 18 7C 7C 62 BF 00 88 " +
            "82 09 2F E0 03 BA 03 00 14 9A 30 F8 F8 04 79 01 " +
            "F6 07 60 64 85 B9 0F 40 60 72 86 00 10 12 C4 03 " +
            "17 C0 F0 F0 AF 10 C0 BC A0 11 00 93 33 78 80 10 " +
            "00 03 5F 00 41 C3 BF 42 00 84 EB 5E C0 07 B4 00 " +
            "02 20 30 5F F0 F1 09 F2 FC DC 07 20 F9 03 10 8C " +
            "98 D0 FB F8 04 78 01 10 7E EE 03 90 05 00 18 46 " +
            "08 01 E2 01 30 39 43 02 E8 63 E0 0B 20 78 00 57 " +
            "04 13 5E C0 07 B4 00 00",
            // After Chi
            "23 7C 0B 06 30 F6 16 16 5F 1F 56 AD 97 E9 60 38 " +
            "1F 07 21 0F 00 06 13 B7 5C 64 AE 95 A7 8B 3B 64 " +
            "1C 9B 53 18 00 84 48 99 B2 DF 58 23 49 4E 73 80 " +
            "3E 1E 87 63 08 1E B1 30 51 E0 03 59 E2 1E 2F 88 " +
            "B2 19 A2 06 89 60 E5 F0 4F 18 5B 38 62 8F 00 C8 " +
            "60 0C 6F E4 06 03 05 40 14 EA B6 F8 E8 06 B9 02 " +
            "E1 87 10 94 2A B9 0F FC E0 7B 89 00 10 B8 C7 03 " +
            "03 52 E0 E8 57 14 B8 BD A0 95 A0 CD B3 7C 80 10 " +
            "02 23 4F 01 71 33 B6 B0 FC 58 EC 7E C9 05 A4 0C " +
            "02 21 30 CC F2 89 89 E2 FC DE 58 20 B9 80 2F CE " +
            "98 D1 1B F9 34 41 42 10 9E 8C 03 9A 05 40 18 13 " +
            "0C 11 FC C1 37 BD 43 02 70 A3 41 33 20 30 01 47 " +
            "62 3D 5E C0 06 B4 18 46",
            // After Iota 
            "A1 FC 0B 06 30 F6 16 16 5F 1F 56 AD 97 E9 60 38 " +
            "1F 07 21 0F 00 06 13 B7 5C 64 AE 95 A7 8B 3B 64 " +
            "1C 9B 53 18 00 84 48 99 B2 DF 58 23 49 4E 73 80 " +
            "3E 1E 87 63 08 1E B1 30 51 E0 03 59 E2 1E 2F 88 " +
            "B2 19 A2 06 89 60 E5 F0 4F 18 5B 38 62 8F 00 C8 " +
            "60 0C 6F E4 06 03 05 40 14 EA B6 F8 E8 06 B9 02 " +
            "E1 87 10 94 2A B9 0F FC E0 7B 89 00 10 B8 C7 03 " +
            "03 52 E0 E8 57 14 B8 BD A0 95 A0 CD B3 7C 80 10 " +
            "02 23 4F 01 71 33 B6 B0 FC 58 EC 7E C9 05 A4 0C " +
            "02 21 30 CC F2 89 89 E2 FC DE 58 20 B9 80 2F CE " +
            "98 D1 1B F9 34 41 42 10 9E 8C 03 9A 05 40 18 13 " +
            "0C 11 FC C1 37 BD 43 02 70 A3 41 33 20 30 01 47 " +
            "62 3D 5E C0 06 B4 18 46",
            // Round #2
            // After Theta
            "BC 47 B3 74 BD D9 5C 21 AB 26 95 A2 03 5D 6A 75 " +
            "0E 4B E3 7B DB 51 F6 7B 9F 28 50 B9 85 C5 61 60 " +
            "F7 C9 A9 9F 1D 63 9C 06 AF 64 E0 51 C4 61 39 B7 " +
            "CA 27 44 6C 9C AA BB 7D 40 AC C1 2D 39 49 CA 44 " +
            "71 55 5C 2A AB 2E BF F4 A4 4A A1 BF 7F 68 D4 57 " +
            "7D B7 D7 96 8B 2C 4F 77 E0 D3 75 F7 7C B2 B3 4F " +
            "F0 CB D2 E0 F1 EE EA 30 23 37 77 2C 32 F6 9D 07 " +
            "E8 00 1A 6F 4A F3 6C 22 BD 2E 18 BF 3E 53 CA 27 " +
            "F6 1A 8C 0E E5 87 BC FD ED 14 2E 0A 12 52 41 C0 " +
            "C1 6D CE E0 D0 C7 D3 E6 17 8C A2 A7 A4 67 FB 51 " +
            "85 6A A3 8B B9 6E 08 27 6A B5 C0 95 91 F4 12 5E " +
            "1D 5D 3E B5 EC EA A6 CE B3 EF BF 1F 02 7E 5B 43 " +
            "89 6F A4 47 1B 53 CC D9",
            // After Rho
            "BC 47 B3 74 BD D9 5C 21 56 4D 2A 45 07 BA D4 EA " +
            "C3 D2 F8 DE 76 94 FD 9E 58 1C 06 F6 89 02 95 5B " +
            "18 E3 34 B8 4F 4E FD EC 45 1C 96 73 FB 4A 06 1E " +
            "C4 C6 A9 BA DB A7 7C 42 11 10 6B 70 4B 4E 92 32 " +
            "2A 2E 95 55 97 5F FA B8 46 7D 45 AA 14 FA FB 87 " +
            "EB BB BD B6 5C 64 79 BA 3E 81 4F D7 DD F3 C9 CE " +
            "06 8F 77 57 87 81 5F 96 EC 3B 0F 46 6E EE 58 64 " +
            "37 A5 79 36 11 74 00 8D 7E 7D A6 94 4F 7A 5D 30 " +
            "D1 A1 FC 90 B7 DF 5E 83 20 E0 76 0A 17 05 09 A9 " +
            "78 DA 3C B8 CD 19 1C FA 51 17 8C A2 A7 A4 67 FB " +
            "21 9C 14 AA 8D 2E E6 BA A9 D5 02 57 46 D2 4B 78 " +
            "A3 CB A7 96 5D DD D4 B9 EF BF 1F 02 7E 5B 43 B3 " +
            "73 76 E2 1B E9 D1 C6 14",
            // After Pi
            "BC 47 B3 74 BD D9 5C 21 C4 C6 A9 BA DB A7 7C 42 " +
            "06 8F 77 57 87 81 5F 96 78 DA 3C B8 CD 19 1C FA " +
            "73 76 E2 1B E9 D1 C6 14 58 1C 06 F6 89 02 95 5B " +
            "46 7D 45 AA 14 FA FB 87 EB BB BD B6 5C 64 79 BA " +
            "D1 A1 FC 90 B7 DF 5E 83 A3 CB A7 96 5D DD D4 B9 " +
            "56 4D 2A 45 07 BA D4 EA 11 10 6B 70 4B 4E 92 32 " +
            "EC 3B 0F 46 6E EE 58 64 51 17 8C A2 A7 A4 67 FB " +
            "21 9C 14 AA 8D 2E E6 BA 18 E3 34 B8 4F 4E FD EC " +
            "45 1C 96 73 FB 4A 06 1E 3E 81 4F D7 DD F3 C9 CE " +
            "20 E0 76 0A 17 05 09 A9 EF BF 1F 02 7E 5B 43 B3 " +
            "C3 D2 F8 DE 76 94 FD 9E 2A 2E 95 55 97 5F FA B8 " +
            "37 A5 79 36 11 74 00 8D 7E 7D A6 94 4F 7A 5D 30 " +
            "A9 D5 02 57 46 D2 4B 78",
            // After Chi
            "BE 4E E5 31 B9 D9 5F B5 BC 96 A1 12 93 BF 7C 2A " +
            "05 AB B5 54 A7 41 9D 92 F4 DB 2D DC D9 11 04 DB " +
            "33 F6 EA 91 AB F7 E6 56 F1 9E BE E2 C1 06 95 63 " +
            "56 7D 05 AA B7 61 FD 86 C9 F1 BE B0 14 64 F9 82 " +
            "89 B5 FC F0 37 DD 5F C1 A5 AA E6 9E 49 25 BE 3D " +
            "BA 66 2E 43 23 1A 9C AE 00 14 EB D0 CA 4E B5 A9 " +
            "CC B3 1F 4E 66 E4 D8 64 07 56 A6 E7 A5 34 77 BB " +
            "20 8C 55 9A C5 6A E4 AA 22 62 7D 3C 4B FF 34 2C " +
            "45 7C A6 7B F9 4E 06 3F F1 9E 46 D7 B5 A9 8B DC " +
            "30 A0 56 B2 16 01 B5 E5 AA A3 9D 41 CE 5B 41 A1 " +
            "D6 53 90 FC 76 B4 FD 9B 62 76 13 D5 D9 55 A7 88 " +
            "B6 25 79 75 11 F4 02 C5 3C 7F 5E 1C 7F 7E E9 B6 " +
            "81 F9 07 56 C7 99 49 58",
            // After Iota 
            "34 CE E5 31 B9 D9 5F 35 BC 96 A1 12 93 BF 7C 2A " +
            "05 AB B5 54 A7 41 9D 92 F4 DB 2D DC D9 11 04 DB " +
            "33 F6 EA 91 AB F7 E6 56 F1 9E BE E2 C1 06 95 63 " +
            "56 7D 05 AA B7 61 FD 86 C9 F1 BE B0 14 64 F9 82 " +
            "89 B5 FC F0 37 DD 5F C1 A5 AA E6 9E 49 25 BE 3D " +
            "BA 66 2E 43 23 1A 9C AE 00 14 EB D0 CA 4E B5 A9 " +
            "CC B3 1F 4E 66 E4 D8 64 07 56 A6 E7 A5 34 77 BB " +
            "20 8C 55 9A C5 6A E4 AA 22 62 7D 3C 4B FF 34 2C " +
            "45 7C A6 7B F9 4E 06 3F F1 9E 46 D7 B5 A9 8B DC " +
            "30 A0 56 B2 16 01 B5 E5 AA A3 9D 41 CE 5B 41 A1 " +
            "D6 53 90 FC 76 B4 FD 9B 62 76 13 D5 D9 55 A7 88 " +
            "B6 25 79 75 11 F4 02 C5 3C 7F 5E 1C 7F 7E E9 B6 " +
            "81 F9 07 56 C7 99 49 58",
            // Round #3
            // After Theta
            "32 AF D3 3E 0A B4 C0 68 B9 35 6F 52 17 09 88 BF " +
            "25 90 B0 58 2D C4 E9 C4 89 9C 81 D1 F5 79 59 C7 " +
            "53 1E A5 55 45 6C A9 3B F7 FF 88 ED 72 6B 0A 3E " +
            "53 DE CB EA 33 D7 09 13 E9 CA BB BC 9E E1 8D D4 " +
            "F4 F2 50 FD 1B B5 02 DD C5 42 A9 5A A7 BE F1 50 " +
            "BC 07 18 4C 90 77 03 F3 05 B7 25 90 4E F8 41 3C " +
            "EC 88 1A 42 EC 61 AC 32 7A 11 0A EA 89 5C 2A A7 " +
            "40 64 1A 5E 2B F1 AB C7 24 03 4B 33 F8 92 AB 71 " +
            "40 DF 68 3B 7D F8 F2 AA D1 A5 43 DB 3F 2C FF 8A " +
            "4D E7 FA BF 3A 69 E8 F9 CA 4B D2 85 20 C0 0E CC " +
            "D0 32 A6 F3 C5 D9 62 C6 67 D5 DD 95 5D E3 53 1D " +
            "96 1E 7C 79 9B 71 76 93 41 38 F2 11 53 16 B4 AA " +
            "E1 11 48 92 29 02 06 35",
            // After Rho
            "32 AF D3 3E 0A B4 C0 68 73 6B DE A4 2E 12 10 7F " +
            "09 24 2C 56 0B 71 3A 71 9F 97 75 9C C8 19 18 5D " +
            "62 4B DD 99 F2 28 AD 2A 2E B7 A6 E0 73 FF 8F D8 " +
            "AC 3E 73 9D 30 31 E5 BD 75 BA F2 2E AF 67 78 23 " +
            "79 A8 FE 8D 5A 81 6E 7A 1B 0F 55 2C 94 AA 75 EA " +
            "E7 3D C0 60 82 BC 1B 98 F1 14 DC 96 40 3A E1 07 " +
            "10 62 0F 63 95 61 47 D4 B9 54 4E F5 22 14 D4 13 " +
            "AF 95 F8 D5 63 20 32 0D 66 F0 25 57 E3 48 06 96 " +
            "6D A7 0F 5F 5E 15 E8 1B 7F C5 E8 D2 A1 ED 1F 96 " +
            "0D 3D BF E9 5C FF 57 27 CC CA 4B D2 85 20 C0 0E " +
            "8B 19 43 CB 98 CE 17 67 9C 55 77 57 76 8D 4F 75 " +
            "D2 83 2F 6F 33 CE 6E D2 38 F2 11 53 16 B4 AA 41 " +
            "41 4D 78 04 92 64 8A 80",
            // After Pi
            "32 AF D3 3E 0A B4 C0 68 AC 3E 73 9D 30 31 E5 BD " +
            "10 62 0F 63 95 61 47 D4 0D 3D BF E9 5C FF 57 27 " +
            "41 4D 78 04 92 64 8A 80 9F 97 75 9C C8 19 18 5D " +
            "1B 0F 55 2C 94 AA 75 EA E7 3D C0 60 82 BC 1B 98 " +
            "6D A7 0F 5F 5E 15 E8 1B D2 83 2F 6F 33 CE 6E D2 " +
            "73 6B DE A4 2E 12 10 7F 75 BA F2 2E AF 67 78 23 " +
            "B9 54 4E F5 22 14 D4 13 CC CA 4B D2 85 20 C0 0E " +
            "8B 19 43 CB 98 CE 17 67 62 4B DD 99 F2 28 AD 2A " +
            "2E B7 A6 E0 73 FF 8F D8 F1 14 DC 96 40 3A E1 07 " +
            "7F C5 E8 D2 A1 ED 1F 96 38 F2 11 53 16 B4 AA 41 " +
            "09 24 2C 56 0B 71 3A 71 79 A8 FE 8D 5A 81 6E 7A " +
            "AF 95 F8 D5 63 20 32 0D 66 F0 25 57 E3 48 06 96 " +
            "9C 55 77 57 76 8D 4F 75",
            // After Chi
            "22 EF DF 5C 8F F4 C2 28 A1 23 C3 15 78 AF F5 9E " +
            "50 22 4F 67 17 61 CF 54 3F 9F 3C D3 54 6F 17 4F " +
            "CD 5D 58 85 A2 65 AF 15 7B A7 F5 DC CA 0D 12 4D " +
            "13 8D 5A 33 C8 AB 95 E9 75 3D E0 40 A3 76 1D 58 " +
            "60 B3 5F CF 96 04 F8 16 D2 8B 2F 4F 27 6C 0B 70 " +
            "FB 2F D2 75 2E 02 94 6F 31 30 F3 2C 2A 47 78 2F " +
            "BA 45 4E FC 3A DA C3 72 BC A8 D7 F6 A3 30 C0 16 " +
            "8F 89 63 C1 19 AB 7F 67 B3 4B 85 8F F2 28 CD 2D " +
            "20 76 86 A0 D2 3A 91 48 F1 26 CD 97 56 2A 41 46 " +
            "3D CC 24 5A 41 E5 1A BC 34 46 33 33 17 63 A8 91 " +
            "8F 31 2C 06 2A 51 2A 74 39 C8 FB 8F DA C9 6A E8 " +
            "37 90 AA D5 77 A5 7B 6C 67 D0 2D 57 EA 38 36 96 " +
            "EC DD A5 DE 26 0D 0B 7F",
            // After Iota 
            "22 6F DF DC 8F F4 C2 A8 A1 23 C3 15 78 AF F5 9E " +
            "50 22 4F 67 17 61 CF 54 3F 9F 3C D3 54 6F 17 4F " +
            "CD 5D 58 85 A2 65 AF 15 7B A7 F5 DC CA 0D 12 4D " +
            "13 8D 5A 33 C8 AB 95 E9 75 3D E0 40 A3 76 1D 58 " +
            "60 B3 5F CF 96 04 F8 16 D2 8B 2F 4F 27 6C 0B 70 " +
            "FB 2F D2 75 2E 02 94 6F 31 30 F3 2C 2A 47 78 2F " +
            "BA 45 4E FC 3A DA C3 72 BC A8 D7 F6 A3 30 C0 16 " +
            "8F 89 63 C1 19 AB 7F 67 B3 4B 85 8F F2 28 CD 2D " +
            "20 76 86 A0 D2 3A 91 48 F1 26 CD 97 56 2A 41 46 " +
            "3D CC 24 5A 41 E5 1A BC 34 46 33 33 17 63 A8 91 " +
            "8F 31 2C 06 2A 51 2A 74 39 C8 FB 8F DA C9 6A E8 " +
            "37 90 AA D5 77 A5 7B 6C 67 D0 2D 57 EA 38 36 96 " +
            "EC DD A5 DE 26 0D 0B 7F",
            // Round #4
            // After Theta
            "5F EA 73 70 06 59 7D B5 8D 66 9F DA 94 A8 00 E5 " +
            "B8 33 23 8D 10 DC 2B 66 F7 FB BF 87 A0 B4 CD C3 " +
            "49 FE 46 9A 0F E6 EB D7 06 22 59 70 43 A0 AD 50 " +
            "3F C8 06 FC 24 AC 60 92 9D 2C 8C AA A4 CB F9 6A " +
            "A8 D7 DC 9B 62 DF 22 9A 56 28 31 50 8A EF 4F B2 " +
            "86 AA 7E D9 A7 AF 2B 72 1D 75 AF E3 C6 40 8D 54 " +
            "52 54 22 16 3D 67 27 40 74 CC 54 A2 57 EB 1A 9A " +
            "0B 2A 7D DE B4 28 3B A5 CE CE 29 23 7B 85 72 30 " +
            "0C 33 DA 6F 3E 3D 64 33 19 37 A1 7D 51 97 A5 74 " +
            "F5 A8 A7 0E B5 3E C0 30 B0 E5 2D 2C BA E0 EC 53 " +
            "F2 B4 80 AA A3 FC 95 69 15 8D A7 40 36 CE 9F 93 " +
            "DF 81 C6 3F 70 18 9F 5E AF B4 AE 03 1E E3 EC 1A " +
            "68 7E BB C1 8B 8E 4F BD",
            // After Rho
            "5F EA 73 70 06 59 7D B5 1B CD 3E B5 29 51 01 CA " +
            "EE CC 48 23 04 F7 8A 19 4A DB 3C 7C BF FF 7B 08 " +
            "30 5F BF 4E F2 37 D2 7C 37 04 DA 0A 65 20 92 05 " +
            "C0 4F C2 0A 26 F9 83 6C 5A 27 0B A3 2A E9 72 BE " +
            "6B EE 4D B1 6F 11 4D D4 FE 24 6B 85 12 03 A5 F8 " +
            "33 54 F5 CB 3E 7D 5D 91 52 75 D4 BD 8E 1B 03 35 " +
            "B1 E8 39 3B 01 92 A2 12 D6 35 34 E9 98 A9 44 AF " +
            "6F 5A 94 9D D2 05 95 3E 46 F6 0A E5 60 9C 9D 53 " +
            "FB CD A7 87 6C 86 61 46 52 BA 8C 9B D0 BE A8 CB " +
            "07 18 A6 1E F5 D4 A1 D6 53 B0 E5 2D 2C BA E0 EC " +
            "57 A6 C9 D3 02 AA 8E F2 56 34 9E 02 D9 38 7F 4E " +
            "3B D0 F8 07 0E E3 D3 EB B4 AE 03 1E E3 EC 1A AF " +
            "53 2F 9A DF 6E F0 A2 E3",
            // After Pi
            "5F EA 73 70 06 59 7D B5 C0 4F C2 0A 26 F9 83 6C " +
            "B1 E8 39 3B 01 92 A2 12 07 18 A6 1E F5 D4 A1 D6 " +
            "53 2F 9A DF 6E F0 A2 E3 4A DB 3C 7C BF FF 7B 08 " +
            "FE 24 6B 85 12 03 A5 F8 33 54 F5 CB 3E 7D 5D 91 " +
            "FB CD A7 87 6C 86 61 46 3B D0 F8 07 0E E3 D3 EB " +
            "1B CD 3E B5 29 51 01 CA 5A 27 0B A3 2A E9 72 BE " +
            "D6 35 34 E9 98 A9 44 AF 53 B0 E5 2D 2C BA E0 EC " +
            "57 A6 C9 D3 02 AA 8E F2 30 5F BF 4E F2 37 D2 7C " +
            "37 04 DA 0A 65 20 92 05 52 75 D4 BD 8E 1B 03 35 " +
            "52 BA 8C 9B D0 BE A8 CB B4 AE 03 1E E3 EC 1A AF " +
            "EE CC 48 23 04 F7 8A 19 6B EE 4D B1 6F 11 4D D4 " +
            "6F 5A 94 9D D2 05 95 3E 46 F6 0A E5 60 9C 9D 53 " +
            "56 34 9E 02 D9 38 7F 4E",
            // After Chi
            "6E 4A 4A 41 07 5B 5D A7 C6 5F 44 0E D2 BD 82 A8 " +
            "E1 CF 21 FA 0B B2 A0 33 0B D8 C7 3E F5 DD FC C2 " +
            "D3 2A 1A D5 4E 50 20 AB 4B 8B A8 36 93 83 23 09 " +
            "36 AD 69 81 52 81 85 BE 33 44 AD CB 3C 1C CF 38 " +
            "BB C6 A3 FF DD 9A 49 46 8F F4 BB 86 0E E3 57 1B " +
            "9F DD 0A FD B9 51 05 CB 5B A7 CA A7 0E FB D2 FE " +
            "D2 33 3C 3B 9A A9 4A BD 5B F9 D3 09 05 EB E1 E4 " +
            "17 84 C8 D1 00 02 FC C6 70 2E BB FB 78 2C D3 4C " +
            "37 8E D2 08 35 84 3A CF F6 71 D7 B9 AD 5B 11 11 " +
            "52 EB 30 DB C0 AD 68 9B B3 AE 43 1E E6 EC 1A AE " +
            "EA DC D8 2F 94 F3 1A 33 6B 4A 47 D1 4F 89 45 95 " +
            "7F 5A 00 9F 4B 25 F7 32 EE 3E 4A C4 64 5B 1D 42 " +
            "57 16 9B 92 B2 38 3A 8A",
            // After Iota 
            "E5 CA 4A 41 07 5B 5D A7 C6 5F 44 0E D2 BD 82 A8 " +
            "E1 CF 21 FA 0B B2 A0 33 0B D8 C7 3E F5 DD FC C2 " +
            "D3 2A 1A D5 4E 50 20 AB 4B 8B A8 36 93 83 23 09 " +
            "36 AD 69 81 52 81 85 BE 33 44 AD CB 3C 1C CF 38 " +
            "BB C6 A3 FF DD 9A 49 46 8F F4 BB 86 0E E3 57 1B " +
            "9F DD 0A FD B9 51 05 CB 5B A7 CA A7 0E FB D2 FE " +
            "D2 33 3C 3B 9A A9 4A BD 5B F9 D3 09 05 EB E1 E4 " +
            "17 84 C8 D1 00 02 FC C6 70 2E BB FB 78 2C D3 4C " +
            "37 8E D2 08 35 84 3A CF F6 71 D7 B9 AD 5B 11 11 " +
            "52 EB 30 DB C0 AD 68 9B B3 AE 43 1E E6 EC 1A AE " +
            "EA DC D8 2F 94 F3 1A 33 6B 4A 47 D1 4F 89 45 95 " +
            "7F 5A 00 9F 4B 25 F7 32 EE 3E 4A C4 64 5B 1D 42 " +
            "57 16 9B 92 B2 38 3A 8A",
            // Round #5
            // After Theta
            "A5 0B 1E AD FA AB A3 90 7E 16 00 08 85 19 B6 99 " +
            "B9 3A C9 A4 EC CD 48 F3 DC 8E C3 0F 96 6E 69 F2 " +
            "D2 C5 C1 BF 45 A7 65 27 0B 4A FC DA 6E 73 DD 3E " +
            "8E E4 2D 87 05 25 B1 8F 6B B1 45 95 DB 63 27 F8 " +
            "6C 90 A7 CE BE 29 DC 76 8E 1B 60 EC 05 14 12 97 " +
            "DF 1C 5E 11 44 A1 FB FC E3 EE 8E A1 59 5F E6 CF " +
            "8A C6 D4 65 7D D6 A2 7D 8C AF D7 38 66 58 74 D4 " +
            "16 6B 13 BB 0B F5 B9 4A 30 EF EF 17 85 DC 2D 7B " +
            "8F C7 96 0E 62 20 0E FE AE 84 3F E7 4A 24 F9 D1 " +
            "85 BD 34 EA A3 1E FD AB B2 41 98 74 ED 1B 5F 22 " +
            "AA 1D 8C C3 69 03 E4 04 D3 03 03 D7 18 2D 71 A4 " +
            "27 AF E8 C1 AC 5A 1F F2 39 68 4E F5 07 E8 88 72 " +
            "56 F9 40 F8 B9 CF 7F 06",
            // After Rho
            "A5 0B 1E AD FA AB A3 90 FD 2C 00 10 0A 33 6C 33 " +
            "AE 4E 32 29 7B 33 D2 7C E9 96 26 CF ED 38 FC 60 " +
            "3A 2D 3B 91 2E 0E FE 2D ED 36 D7 ED B3 A0 C4 AF " +
            "72 58 50 12 FB E8 48 DE FE 5A 6C 51 E5 F6 D8 09 " +
            "C8 53 67 DF 14 6E 3B 36 21 71 E9 B8 01 C6 5E 40 " +
            "FF E6 F0 8A 20 0A DD E7 3F 8F BB 3B 86 66 7D 99 " +
            "2E EB B3 16 ED 53 34 A6 B0 E8 A8 19 5F AF 71 CC " +
            "DD 85 FA 5C 25 8B B5 89 2F 0A B9 5B F6 60 DE DF " +
            "D2 41 0C C4 C1 FF F1 D8 FC 68 57 C2 9F 73 25 92 " +
            "A3 7F B5 B0 97 46 7D D4 22 B2 41 98 74 ED 1B 5F " +
            "90 13 A8 76 30 0E A7 0D 4E 0F 0C 5C 63 B4 C4 91 " +
            "E4 15 3D 98 55 EB 43 FE 68 4E F5 07 E8 88 72 39 " +
            "9F 81 55 3E 10 7E EE F3",
            // After Pi
            "A5 0B 1E AD FA AB A3 90 72 58 50 12 FB E8 48 DE " +
            "2E EB B3 16 ED 53 34 A6 A3 7F B5 B0 97 46 7D D4 " +
            "9F 81 55 3E 10 7E EE F3 E9 96 26 CF ED 38 FC 60 " +
            "21 71 E9 B8 01 C6 5E 40 FF E6 F0 8A 20 0A DD E7 " +
            "D2 41 0C C4 C1 FF F1 D8 E4 15 3D 98 55 EB 43 FE " +
            "FD 2C 00 10 0A 33 6C 33 FE 5A 6C 51 E5 F6 D8 09 " +
            "B0 E8 A8 19 5F AF 71 CC 22 B2 41 98 74 ED 1B 5F " +
            "90 13 A8 76 30 0E A7 0D 3A 2D 3B 91 2E 0E FE 2D " +
            "ED 36 D7 ED B3 A0 C4 AF 3F 8F BB 3B 86 66 7D 99 " +
            "FC 68 57 C2 9F 73 25 92 68 4E F5 07 E8 88 72 39 " +
            "AE 4E 32 29 7B 33 D2 7C C8 53 67 DF 14 6E 3B 36 " +
            "DD 85 FA 5C 25 8B B5 89 2F 0A B9 5B F6 60 DE DF " +
            "4E 0F 0C 5C 63 B4 C4 91",
            // After Chi
            "A9 A8 BD A9 FE B8 97 B0 F3 4C 54 B2 E9 EC 01 8E " +
            "32 6B F3 18 ED 6B B6 85 83 75 BF 31 7D C7 7C D4 " +
            "CD D1 15 2C 11 3E A6 BD 37 10 36 CD CD 30 7D C7 " +
            "21 70 E5 FC C0 33 7E 58 DB F2 C1 92 34 0A DF C1 " +
            "DB C3 0E 83 69 EF 4D D8 E4 74 F4 A8 55 2D 41 FE " +
            "FD 8C 80 18 10 3A 4D F7 FC 48 2D D1 C5 B6 D2 1A " +
            "20 E9 00 7F 5F AD D5 CC 4F 9E 41 98 7E DC 53 6D " +
            "92 41 C4 37 D5 CA 37 05 28 A4 13 83 2A 48 C7 3D " +
            "2D 56 93 2D AA B1 C4 AD 3F 89 1B 3E E6 EE 2F B0 " +
            "EE 49 5D 52 99 75 A9 96 AD 5C 31 6B 79 28 72 BB " +
            "BB CA AA 29 5A B2 56 F5 EA 59 66 DC C6 0E 71 60 " +
            "9D 80 FE 58 24 1F B5 89 8F 4A 8B 7A EE 63 CC B3 " +
            "0E 1E 49 8A 67 F8 ED 93",
            // After Iota 
            "A8 A8 BD 29 FE B8 97 B0 F3 4C 54 B2 E9 EC 01 8E " +
            "32 6B F3 18 ED 6B B6 85 83 75 BF 31 7D C7 7C D4 " +
            "CD D1 15 2C 11 3E A6 BD 37 10 36 CD CD 30 7D C7 " +
            "21 70 E5 FC C0 33 7E 58 DB F2 C1 92 34 0A DF C1 " +
            "DB C3 0E 83 69 EF 4D D8 E4 74 F4 A8 55 2D 41 FE " +
            "FD 8C 80 18 10 3A 4D F7 FC 48 2D D1 C5 B6 D2 1A " +
            "20 E9 00 7F 5F AD D5 CC 4F 9E 41 98 7E DC 53 6D " +
            "92 41 C4 37 D5 CA 37 05 28 A4 13 83 2A 48 C7 3D " +
            "2D 56 93 2D AA B1 C4 AD 3F 89 1B 3E E6 EE 2F B0 " +
            "EE 49 5D 52 99 75 A9 96 AD 5C 31 6B 79 28 72 BB " +
            "BB CA AA 29 5A B2 56 F5 EA 59 66 DC C6 0E 71 60 " +
            "9D 80 FE 58 24 1F B5 89 8F 4A 8B 7A EE 63 CC B3 " +
            "0E 1E 49 8A 67 F8 ED 93",
            // Round #6
            // After Theta
            "62 F9 32 A7 71 1C E9 DC D5 E4 48 C3 33 DE 7B A4 " +
            "37 46 D6 72 57 79 A1 0C D8 40 D3 06 27 E9 C4 B9 " +
            "59 4F 57 83 AA 4C CD 69 FD 41 B9 43 42 94 03 AB " +
            "07 D8 F9 8D 1A 01 04 72 DE DF E4 F8 8E 18 C8 48 " +
            "80 F6 62 B4 33 C1 F5 B5 70 EA B6 07 EE 5F 2A 2A " +
            "37 DD 0F 96 9F 9E 33 9B DA E0 31 A0 1F 84 A8 30 " +
            "25 C4 25 15 E5 BF C2 45 14 AB 2D AF 24 F2 EB 00 " +
            "06 DF 86 98 6E B8 5C D1 E2 F5 9C 0D A5 EC B9 51 " +
            "0B FE 8F 5C 70 83 BE 87 3A A4 3E 54 5C FC 38 39 " +
            "B5 7C 31 65 C3 5B 11 FB 39 C2 73 C4 C2 5A 19 6F " +
            "71 9B 25 A7 D5 16 28 99 CC F1 7A AD 1C 3C 0B 4A " +
            "98 AD DB 32 9E 0D A2 00 D4 7F E7 4D B4 4D 74 DE " +
            "9A 80 0B 25 DC 8A 86 47",
            // After Rho
            "62 F9 32 A7 71 1C E9 DC AB C9 91 86 67 BC F7 48 " +
            "8D 91 B5 DC 55 5E 28 C3 92 4E 9C 8B 0D 34 6D 70 " +
            "65 6A 4E CB 7A BA 1A 54 24 44 39 B0 DA 1F 94 3B " +
            "DF A8 11 40 20 77 80 9D 92 F7 37 39 BE 23 06 32 " +
            "7B 31 DA 99 E0 FA 5A 40 A5 A2 02 A7 6E 7B E0 FE " +
            "BC E9 7E B0 FC F4 9C D9 C2 68 83 C7 80 7E 10 A2 " +
            "A9 28 FF 15 2E 2A 21 2E E4 D7 01 28 56 5B 5E 49 " +
            "4C 37 5C AE 68 83 6F 43 1B 4A D9 73 A3 C4 EB 39 " +
            "91 0B 6E D0 F7 70 C1 FF 9C 1C 1D 52 1F 2A 2E 7E " +
            "2B 62 BF 96 2F A6 6C 78 6F 39 C2 73 C4 C2 5A 19 " +
            "A0 64 C6 6D 96 9C 56 5B 31 C7 EB B5 72 F0 2C 28 " +
            "B3 75 5B C6 B3 41 14 00 7F E7 4D B4 4D 74 DE D4 " +
            "E1 91 26 E0 42 09 B7 A2",
            // After Pi
            "62 F9 32 A7 71 1C E9 DC DF A8 11 40 20 77 80 9D " +
            "A9 28 FF 15 2E 2A 21 2E 2B 62 BF 96 2F A6 6C 78 " +
            "E1 91 26 E0 42 09 B7 A2 92 4E 9C 8B 0D 34 6D 70 " +
            "A5 A2 02 A7 6E 7B E0 FE BC E9 7E B0 FC F4 9C D9 " +
            "91 0B 6E D0 F7 70 C1 FF B3 75 5B C6 B3 41 14 00 " +
            "AB C9 91 86 67 BC F7 48 92 F7 37 39 BE 23 06 32 " +
            "E4 D7 01 28 56 5B 5E 49 6F 39 C2 73 C4 C2 5A 19 " +
            "A0 64 C6 6D 96 9C 56 5B 65 6A 4E CB 7A BA 1A 54 " +
            "24 44 39 B0 DA 1F 94 3B C2 68 83 C7 80 7E 10 A2 " +
            "9C 1C 1D 52 1F 2A 2E 7E 7F E7 4D B4 4D 74 DE D4 " +
            "8D 91 B5 DC 55 5E 28 C3 7B 31 DA 99 E0 FA 5A 40 " +
            "4C 37 5C AE 68 83 6F 43 1B 4A D9 73 A3 C4 EB 39 " +
            "31 C7 EB B5 72 F0 2C 28",
            // After Chi
            "42 F9 DC B2 7F 14 C8 FE DD EA 11 C2 21 F3 CC CD " +
            "69 B9 FF 75 6E 23 B2 AC 29 0A AF 91 1E B2 24 24 " +
            "7C 91 27 A0 42 6A B7 A3 8A 07 E0 9B 9D B0 71 71 " +
            "A4 A0 02 E7 6D 7B A1 D8 9E 9D 6F B6 FC F5 88 D9 " +
            "91 01 EA D9 FB 44 A8 8F 96 D5 59 E2 D1 0A 94 8E " +
            "CF C9 91 86 27 E4 AF 01 99 DF F5 6A 3E A3 06 22 " +
            "64 93 05 24 44 47 5A 0B 64 B0 D3 F1 A5 E2 FB 19 " +
            "B0 52 E0 54 0E 9F 56 69 A7 42 CC 8C 7A DA 1A D4 " +
            "38 50 25 A0 C5 1F BA 67 A1 8B C3 63 C0 2A C0 22 " +
            "9C 14 1F 19 2D A0 2E 7E 7F E3 7C 84 CD 71 5A FF " +
            "89 97 B1 FA 5D 5F 0D C0 68 79 5B C8 63 BE DA 78 " +
            "6C B2 7E 2A 38 B3 6B 43 97 5A CD 3B A6 CA EB FA " +
            "43 E7 A1 B4 D2 50 7E 28",
            // After Iota 
            "C3 79 DC 32 7F 14 C8 7E DD EA 11 C2 21 F3 CC CD " +
            "69 B9 FF 75 6E 23 B2 AC 29 0A AF 91 1E B2 24 24 " +
            "7C 91 27 A0 42 6A B7 A3 8A 07 E0 9B 9D B0 71 71 " +
            "A4 A0 02 E7 6D 7B A1 D8 9E 9D 6F B6 FC F5 88 D9 " +
            "91 01 EA D9 FB 44 A8 8F 96 D5 59 E2 D1 0A 94 8E " +
            "CF C9 91 86 27 E4 AF 01 99 DF F5 6A 3E A3 06 22 " +
            "64 93 05 24 44 47 5A 0B 64 B0 D3 F1 A5 E2 FB 19 " +
            "B0 52 E0 54 0E 9F 56 69 A7 42 CC 8C 7A DA 1A D4 " +
            "38 50 25 A0 C5 1F BA 67 A1 8B C3 63 C0 2A C0 22 " +
            "9C 14 1F 19 2D A0 2E 7E 7F E3 7C 84 CD 71 5A FF " +
            "89 97 B1 FA 5D 5F 0D C0 68 79 5B C8 63 BE DA 78 " +
            "6C B2 7E 2A 38 B3 6B 43 97 5A CD 3B A6 CA EB FA " +
            "43 E7 A1 B4 D2 50 7E 28",
            // Round #7
            // After Theta
            "C5 12 AE 5B 55 DF 8E BD C9 94 90 C7 9E 26 5B E8 " +
            "77 EE EE 64 2D 54 DD E9 BA A0 01 73 34 07 4C 1D " +
            "FB A1 C3 88 4D 9F 06 A1 8C 6C 92 F2 B7 7B 37 B2 " +
            "B0 DE 83 E2 D2 AE 36 FD 80 CA 7E A7 BF 82 E7 9C " +
            "02 AB 44 3B D1 F1 C0 B6 11 E5 BD CA DE FF 25 8C " +
            "C9 A2 E3 EF 0D 2F E9 C2 8D A1 74 6F 81 76 91 07 " +
            "7A C4 14 35 07 30 35 4E F7 1A 7D 13 8F 57 93 20 " +
            "37 62 04 7C 01 6A E7 6B A1 29 BE E5 50 11 5C 17 " +
            "2C 2E A4 A5 7A CA 2D 42 BF DC D2 72 83 5D AF 67 " +
            "0F BE B1 FB 07 15 46 47 F8 D3 98 AC C2 84 EB FD " +
            "8F FC C3 93 77 94 4B 03 7C 07 DA CD DC 6B 4D 5D " +
            "72 E5 6F 3B 7B C4 04 06 04 F0 63 D9 8C 7F 83 C3 " +
            "C4 D7 45 9C DD A5 CF 2A",
            // After Rho
            "C5 12 AE 5B 55 DF 8E BD 93 29 21 8F 3D 4D B6 D0 " +
            "9D BB 3B 59 0B 55 77 FA 73 C0 D4 A1 0B 1A 30 47 " +
            "FA 34 08 DD 0F 1D 46 6C 7F BB 77 23 CB C8 26 29 " +
            "28 2E ED 6A D3 0F EB 3D 27 A0 B2 DF E9 AF E0 39 " +
            "55 A2 9D E8 78 60 5B 81 5F C2 18 51 DE AB EC FD " +
            "4E 16 1D 7F 6F 78 49 17 1E 34 86 D2 BD 05 DA 45 " +
            "A8 39 80 A9 71 D2 23 A6 AF 26 41 EE 35 FA 26 1E " +
            "BE 00 B5 F3 B5 1B 31 02 CB A1 22 B8 2E 42 53 7C " +
            "B4 54 4F B9 45 88 C5 85 D7 B3 5F 6E 69 B9 C1 AE " +
            "C2 E8 E8 C1 37 76 FF A0 FD F8 D3 98 AC C2 84 EB " +
            "2E 0D 3C F2 0F 4F DE 51 F1 1D 68 37 73 AF 35 75 " +
            "AE FC 6D 67 8F 98 C0 40 F0 63 D9 8C 7F 83 C3 04 " +
            "B3 0A F1 75 11 67 77 E9",
            // After Pi
            "C5 12 AE 5B 55 DF 8E BD 28 2E ED 6A D3 0F EB 3D " +
            "A8 39 80 A9 71 D2 23 A6 C2 E8 E8 C1 37 76 FF A0 " +
            "B3 0A F1 75 11 67 77 E9 73 C0 D4 A1 0B 1A 30 47 " +
            "5F C2 18 51 DE AB EC FD 4E 16 1D 7F 6F 78 49 17 " +
            "B4 54 4F B9 45 88 C5 85 AE FC 6D 67 8F 98 C0 40 " +
            "93 29 21 8F 3D 4D B6 D0 27 A0 B2 DF E9 AF E0 39 " +
            "AF 26 41 EE 35 FA 26 1E FD F8 D3 98 AC C2 84 EB " +
            "2E 0D 3C F2 0F 4F DE 51 FA 34 08 DD 0F 1D 46 6C " +
            "7F BB 77 23 CB C8 26 29 1E 34 86 D2 BD 05 DA 45 " +
            "D7 B3 5F 6E 69 B9 C1 AE F0 63 D9 8C 7F 83 C3 04 " +
            "9D BB 3B 59 0B 55 77 FA 55 A2 9D E8 78 60 5B 81 " +
            "BE 00 B5 F3 B5 1B 31 02 CB A1 22 B8 2E 42 53 7C " +
            "F1 1D 68 37 73 AF 35 75",
            // After Chi
            "45 03 AE DA 75 0F 8E 3F 6A EE 85 2A D5 2B 37 3D " +
            "99 3B 91 9D 71 D3 23 EF 86 F8 E6 CB 73 EE 77 B4 " +
            "9B 26 B0 55 93 67 16 E9 73 D4 D1 8F 2A 4A 31 45 " +
            "EF 82 5A D1 DE 2B 68 7D 44 BE 3D 39 E5 68 49 57 " +
            "E5 54 DF 39 45 8A F5 82 A2 FE 65 37 5B 39 0C F8 " +
            "1B 2F 60 AF 29 1D B0 D6 77 78 20 CF 61 AF 60 D8 " +
            "AD 23 6D 8C 36 F7 7C 0E 6C D8 D2 95 9C C2 A4 6B " +
            "0A 8D AE A2 CF ED 9E 78 FA 30 88 0D 3B 18 9E 28 " +
            "BE 38 2E 0F 8B 70 27 83 3E 74 06 52 AB 07 D8 45 " +
            "DD A7 5F 3F 69 A5 C5 C6 F5 E8 AE AE BF 43 E3 05 " +
            "37 BB 1B 4A 8E 4E 57 F8 14 03 9F E0 72 20 19 FD " +
            "8E 1C FD F4 E4 B6 15 03 C7 03 31 F0 26 12 11 F6 " +
            "B1 1D EC 97 03 8F 3D 74",
            // After Iota 
            "4C 83 AE DA 75 0F 8E BF 6A EE 85 2A D5 2B 37 3D " +
            "99 3B 91 9D 71 D3 23 EF 86 F8 E6 CB 73 EE 77 B4 " +
            "9B 26 B0 55 93 67 16 E9 73 D4 D1 8F 2A 4A 31 45 " +
            "EF 82 5A D1 DE 2B 68 7D 44 BE 3D 39 E5 68 49 57 " +
            "E5 54 DF 39 45 8A F5 82 A2 FE 65 37 5B 39 0C F8 " +
            "1B 2F 60 AF 29 1D B0 D6 77 78 20 CF 61 AF 60 D8 " +
            "AD 23 6D 8C 36 F7 7C 0E 6C D8 D2 95 9C C2 A4 6B " +
            "0A 8D AE A2 CF ED 9E 78 FA 30 88 0D 3B 18 9E 28 " +
            "BE 38 2E 0F 8B 70 27 83 3E 74 06 52 AB 07 D8 45 " +
            "DD A7 5F 3F 69 A5 C5 C6 F5 E8 AE AE BF 43 E3 05 " +
            "37 BB 1B 4A 8E 4E 57 F8 14 03 9F E0 72 20 19 FD " +
            "8E 1C FD F4 E4 B6 15 03 C7 03 31 F0 26 12 11 F6 " +
            "B1 1D EC 97 03 8F 3D 74",
            // Round #8
            // After Theta
            "8A 7D 0B 95 E9 8F D7 6B 02 80 7C 8B CD DE 46 20 " +
            "EB B4 D4 17 29 0F C6 D2 A8 76 AF B7 E9 EC 18 74 " +
            "5D 11 2C 86 F1 6B 68 7D B5 2A 74 C0 B6 CA 68 91 " +
            "87 EC A3 70 C6 DE 19 60 36 31 78 B3 BD B4 AC 6A " +
            "CB DA 96 45 DF 88 9A 42 64 C9 F9 E4 39 35 72 6C " +
            "DD D1 C5 E0 B5 9D E9 02 1F 16 D9 6E 79 5A 11 C5 " +
            "DF AC 28 06 6E 2B 99 33 42 56 9B E9 06 C0 CB AB " +
            "CC BA 32 71 AD E1 E0 EC 3C CE 2D 42 A7 98 C7 FC " +
            "D6 56 D7 AE 93 85 56 9E 4C FB 43 D8 F3 DB 3D 78 " +
            "F3 29 16 43 F3 A7 AA 06 33 DF 32 7D DD 4F 9D 91 " +
            "F1 45 BE 05 12 CE 0E 2C 7C 6D 66 41 6A D5 68 E0 " +
            "FC 93 B8 7E BC 6A F0 3E E9 8D 78 8C BC 10 7E 36 " +
            "77 2A 70 44 61 83 43 E0",
            // After Rho
            "8A 7D 0B 95 E9 8F D7 6B 04 00 F9 16 9B BD 8D 40 " +
            "3A 2D F5 45 CA 83 B1 F4 CE 8E 41 87 6A F7 7A 9B " +
            "5F 43 EB EB 8A 60 31 8C 6C AB 8C 16 59 AB 42 07 " +
            "0A 67 EC 9D 01 76 C8 3E 9A 4D 0C DE 6C 2F 2D AB " +
            "6D CB A2 6F 44 4D A1 65 23 C7 46 96 9C 4F 9E 53 " +
            "E8 8E 2E 06 AF ED 4C 17 14 7F 58 64 BB E5 69 45 " +
            "31 70 5B C9 9C F9 66 45 80 97 57 85 AC 36 D3 0D " +
            "B8 D6 70 70 76 66 5D 99 84 4E 31 8F F9 79 9C 5B " +
            "DA 75 B2 D0 CA D3 DA EA 1E 3C A6 FD 21 EC F9 ED " +
            "54 D5 60 3E C5 62 68 FE 91 33 DF 32 7D DD 4F 9D " +
            "3B B0 C4 17 F9 16 48 38 F3 B5 99 05 A9 55 A3 81 " +
            "7F 12 D7 8F 57 0D DE 87 8D 78 8C BC 10 7E 36 E9 " +
            "10 F8 9D 0A 1C 51 D8 E0",
            // After Pi
            "8A 7D 0B 95 E9 8F D7 6B 0A 67 EC 9D 01 76 C8 3E " +
            "31 70 5B C9 9C F9 66 45 54 D5 60 3E C5 62 68 FE " +
            "10 F8 9D 0A 1C 51 D8 E0 CE 8E 41 87 6A F7 7A 9B " +
            "23 C7 46 96 9C 4F 9E 53 E8 8E 2E 06 AF ED 4C 17 " +
            "DA 75 B2 D0 CA D3 DA EA 7F 12 D7 8F 57 0D DE 87 " +
            "04 00 F9 16 9B BD 8D 40 9A 4D 0C DE 6C 2F 2D AB " +
            "80 97 57 85 AC 36 D3 0D 91 33 DF 32 7D DD 4F 9D " +
            "3B B0 C4 17 F9 16 48 38 5F 43 EB EB 8A 60 31 8C " +
            "6C AB 8C 16 59 AB 42 07 14 7F 58 64 BB E5 69 45 " +
            "1E 3C A6 FD 21 EC F9 ED 8D 78 8C BC 10 7E 36 E9 " +
            "3A 2D F5 45 CA 83 B1 F4 6D CB A2 6F 44 4D A1 65 " +
            "B8 D6 70 70 76 66 5D 99 84 4E 31 8F F9 79 9C 5B " +
            "F3 B5 99 05 A9 55 A3 81",
            // After Chi
            "BB 6D 18 D5 75 06 F1 2A 4E E2 CC AB 40 74 C0 84 " +
            "31 58 C6 C9 84 E8 F6 45 DE D0 62 AB 24 EC 6F F5 " +
            "10 FA 79 02 1C 21 D0 F4 06 86 69 87 49 57 3A 9F " +
            "31 B6 D6 46 DC 5D 0C BB CD 8C 6B 09 BA E1 48 12 " +
            "5A F9 B2 D0 E2 21 FA F2 5E 53 D1 9F C3 05 5A C7 " +
            "04 92 AA 17 1B AD 5F 44 8B 6D 84 EC 3D E6 21 3B " +
            "AA 17 57 80 2C 34 D3 2D 95 33 E6 32 7F 74 CA DD " +
            "A1 FD C0 DF 9D 14 68 93 4F 17 BB 8B 28 24 18 CC " +
            "66 AB 2A 8F 59 A3 D2 AF 95 3F 50 64 AB F7 6F 45 " +
            "4C 3F C5 BE AB EC F8 E9 AD D0 88 A8 41 F5 74 EA " +
            "AA 39 A5 55 F8 A1 ED 6C 69 C3 A3 E0 CD 54 21 27 " +
            "CB 67 F8 70 76 62 7E 19 8C 46 55 CF BB FB 8C 2F " +
            "B6 77 9B 2F AD 19 A3 80",
            // After Iota 
            "31 6D 18 D5 75 06 F1 2A 4E E2 CC AB 40 74 C0 84 " +
            "31 58 C6 C9 84 E8 F6 45 DE D0 62 AB 24 EC 6F F5 " +
            "10 FA 79 02 1C 21 D0 F4 06 86 69 87 49 57 3A 9F " +
            "31 B6 D6 46 DC 5D 0C BB CD 8C 6B 09 BA E1 48 12 " +
            "5A F9 B2 D0 E2 21 FA F2 5E 53 D1 9F C3 05 5A C7 " +
            "04 92 AA 17 1B AD 5F 44 8B 6D 84 EC 3D E6 21 3B " +
            "AA 17 57 80 2C 34 D3 2D 95 33 E6 32 7F 74 CA DD " +
            "A1 FD C0 DF 9D 14 68 93 4F 17 BB 8B 28 24 18 CC " +
            "66 AB 2A 8F 59 A3 D2 AF 95 3F 50 64 AB F7 6F 45 " +
            "4C 3F C5 BE AB EC F8 E9 AD D0 88 A8 41 F5 74 EA " +
            "AA 39 A5 55 F8 A1 ED 6C 69 C3 A3 E0 CD 54 21 27 " +
            "CB 67 F8 70 76 62 7E 19 8C 46 55 CF BB FB 8C 2F " +
            "B6 77 9B 2F AD 19 A3 80",
            // Round #9
            // After Theta
            "32 3D 4D CC B1 AA F8 F8 88 83 AC 98 29 5C 58 99 " +
            "68 CE 9D D6 E3 8D BF F1 3F AC C7 75 B6 FD 78 47 " +
            "6D 36 55 0D 5A 7C 39 4A 05 D6 3C 9E 8D FB 33 4D " +
            "F7 D7 B6 75 B5 75 94 A6 94 1A 30 16 DD 84 01 A6 " +
            "BB 85 17 0E 70 30 ED 40 23 9F FD 90 85 58 B3 79 " +
            "07 C2 FF 0E DF 01 56 96 4D 0C E4 DF 54 CE B9 26 " +
            "F3 81 0C 9F 4B 51 9A 99 74 4F 43 EC ED 65 DD 6F " +
            "DC 31 EC D0 DB 49 81 2D 4C 47 EE 92 EC 88 11 1E " +
            "A0 CA 4A BC 30 8B 4A B2 CC A9 0B 7B CC 92 26 F1 " +
            "AD 43 60 60 39 FD EF 5B D0 1C A4 A7 07 A8 9D 54 " +
            "A9 69 F0 4C 3C 0D E4 BE AF A2 C3 D3 A4 7C B9 3A " +
            "92 F1 A3 6F 11 07 37 AD 6D 3A F0 11 29 EA 9B 9D " +
            "CB BB B7 20 EB 44 4A 3E",
            // After Rho
            "32 3D 4D CC B1 AA F8 F8 11 07 59 31 53 B8 B0 32 " +
            "9A 73 A7 F5 78 E3 6F 3C DB 8F 77 F4 C3 7A 5C 67 " +
            "E2 CB 51 6A B3 A9 6A D0 D9 B8 3F D3 54 60 CD E3 " +
            "5B 57 5B 47 69 7A 7F 6D 29 A5 06 8C 45 37 61 80 " +
            "C2 0B 07 38 98 76 A0 DD 35 9B 37 F2 D9 0F 59 88 " +
            "3C 10 FE 77 F8 0E B0 B2 9A 34 31 90 7F 53 39 E7 " +
            "F8 5C 8A D2 CC 9C 0F 64 CB BA DF E8 9E 86 D8 DB " +
            "E8 ED A4 C0 16 EE 18 76 25 D9 11 23 3C 98 8E DC " +
            "89 17 66 51 49 16 54 59 93 78 E6 D4 85 3D 66 49 " +
            "FF 7D AB 75 08 0C 2C A7 54 D0 1C A4 A7 07 A8 9D " +
            "90 FB A6 A6 C1 33 F1 34 BC 8A 0E 4F 93 F2 E5 EA " +
            "32 7E F4 2D E2 E0 A6 55 3A F0 11 29 EA 9B 9D 6D " +
            "92 CF F2 EE 2D C8 3A 91",
            // After Pi
            "32 3D 4D CC B1 AA F8 F8 5B 57 5B 47 69 7A 7F 6D " +
            "F8 5C 8A D2 CC 9C 0F 64 FF 7D AB 75 08 0C 2C A7 " +
            "92 CF F2 EE 2D C8 3A 91 DB 8F 77 F4 C3 7A 5C 67 " +
            "35 9B 37 F2 D9 0F 59 88 3C 10 FE 77 F8 0E B0 B2 " +
            "89 17 66 51 49 16 54 59 32 7E F4 2D E2 E0 A6 55 " +
            "11 07 59 31 53 B8 B0 32 29 A5 06 8C 45 37 61 80 " +
            "CB BA DF E8 9E 86 D8 DB 54 D0 1C A4 A7 07 A8 9D " +
            "90 FB A6 A6 C1 33 F1 34 E2 CB 51 6A B3 A9 6A D0 " +
            "D9 B8 3F D3 54 60 CD E3 9A 34 31 90 7F 53 39 E7 " +
            "93 78 E6 D4 85 3D 66 49 3A F0 11 29 EA 9B 9D 6D " +
            "9A 73 A7 F5 78 E3 6F 3C C2 0B 07 38 98 76 A0 DD " +
            "E8 ED A4 C0 16 EE 18 76 25 D9 11 23 3C 98 8E DC " +
            "BC 8A 0E 4F 93 F2 E5 EA",
            // After Chi
            "92 35 CD 5C 35 2E F8 F8 5C 76 7A 62 69 7A 5F EE " +
            "F8 DE DA 58 E9 5C 1D 74 DF 4D A6 75 98 2E EC CF " +
            "DB 8D E0 ED 65 98 3D 94 D3 8F BF F1 E3 7A FC 55 " +
            "B4 9C 37 F2 D8 1F 1D C1 0E 78 6E 5B 5A EE 12 B6 " +
            "40 96 65 81 48 0C 0C 7B 16 6E F4 2F FA E5 A7 DD " +
            "D3 1D 80 51 C9 38 28 69 3D E5 06 88 64 36 41 84 " +
            "4B 91 7D EA DE B6 89 FB 55 D4 45 B5 B5 8F A8 9F " +
            "B8 5B A0 2A C5 34 B0 B4 E0 CF 51 6A 98 BA 5A D4 " +
            "D8 F0 F9 97 D4 4C 8B EB B2 B4 20 B9 15 D1 A0 C3 " +
            "53 73 A6 96 94 1D 04 D9 23 C0 3F B8 AE DB 18 4E " +
            "B2 97 07 35 7E 6B 77 1E C7 1B 16 1B B0 66 26 55 " +
            "70 EF AA 8C 95 8C 79 54 27 A8 B0 93 54 99 84 C8 " +
            "FC 82 0E 47 13 E6 65 2B",
            // After Iota 
            "1A 35 CD 5C 35 2E F8 F8 5C 76 7A 62 69 7A 5F EE " +
            "F8 DE DA 58 E9 5C 1D 74 DF 4D A6 75 98 2E EC CF " +
            "DB 8D E0 ED 65 98 3D 94 D3 8F BF F1 E3 7A FC 55 " +
            "B4 9C 37 F2 D8 1F 1D C1 0E 78 6E 5B 5A EE 12 B6 " +
            "40 96 65 81 48 0C 0C 7B 16 6E F4 2F FA E5 A7 DD " +
            "D3 1D 80 51 C9 38 28 69 3D E5 06 88 64 36 41 84 " +
            "4B 91 7D EA DE B6 89 FB 55 D4 45 B5 B5 8F A8 9F " +
            "B8 5B A0 2A C5 34 B0 B4 E0 CF 51 6A 98 BA 5A D4 " +
            "D8 F0 F9 97 D4 4C 8B EB B2 B4 20 B9 15 D1 A0 C3 " +
            "53 73 A6 96 94 1D 04 D9 23 C0 3F B8 AE DB 18 4E " +
            "B2 97 07 35 7E 6B 77 1E C7 1B 16 1B B0 66 26 55 " +
            "70 EF AA 8C 95 8C 79 54 27 A8 B0 93 54 99 84 C8 " +
            "FC 82 0E 47 13 E6 65 2B",
            // Round #10
            // After Theta
            "24 06 01 62 B1 A9 F3 4B EB 51 58 79 4B 74 E0 BC " +
            "4E 93 5F 45 12 76 23 14 F5 D4 EE 86 BB 9E 1D 51 " +
            "F5 A7 39 EE 33 CA F6 B2 ED BC 73 CF 67 FD F7 E6 " +
            "03 BB 15 E9 FA 11 A2 93 B8 35 EB 46 A1 C4 2C D6 " +
            "6A 0F 2D 72 6B BC FD E5 38 44 2D 2C AC B7 6C FB " +
            "ED 2E 4C 6F 4D BF 23 DA 8A C2 24 93 46 38 FE D6 " +
            "FD DC F8 F7 25 9C B7 9B 7F 4D 0D 46 96 3F 59 01 " +
            "96 71 79 29 93 66 7B 92 DE FC 9D 54 1C 3D 51 67 " +
            "6F D7 DB 8C F6 42 34 B9 04 F9 A5 A4 EE FB 9E A3 " +
            "79 EA EE 65 B7 AD F5 47 0D EA E6 BB F8 89 D3 68 " +
            "8C A4 CB 0B FA EC 7C AD 70 3C 34 00 92 68 99 07 " +
            "C6 A2 2F 91 6E A6 47 34 0D 31 F8 60 77 29 75 56 " +
            "D2 A8 D7 44 45 B4 AE 0D",
            // After Rho
            "24 06 01 62 B1 A9 F3 4B D7 A3 B0 F2 96 E8 C0 79 " +
            "D3 E4 57 91 84 DD 08 85 EB D9 11 55 4F ED 6E B8 " +
            "51 B6 97 AD 3F CD 71 9F 7C D6 7F 6F DE CE 3B F7 " +
            "91 AE 1F 21 3A 39 B0 5B 35 6E CD BA 51 28 31 8B " +
            "87 16 B9 35 DE FE 72 B5 CB B6 8F 43 D4 C2 C2 7A " +
            "6E 77 61 7A 6B FA 1D D1 5B 2B 0A 93 4C 1A E1 F8 " +
            "BF 2F E1 BC DD EC E7 C6 7F B2 02 FE 9A 1A 8C 2C " +
            "94 49 B3 3D 49 CB B8 BC A9 38 7A A2 CE BC F9 3B " +
            "9B D1 5E 88 26 F7 ED 7A CF 51 82 FC 52 52 F7 7D " +
            "B5 FE 28 4F DD BD EC B6 68 0D EA E6 BB F8 89 D3 " +
            "F3 B5 32 92 2E 2F E8 B3 C0 F1 D0 00 48 A2 65 1E " +
            "58 F4 25 D2 CD F4 88 C6 31 F8 60 77 29 75 56 0D " +
            "6B 83 34 EA 35 51 11 AD",
            // After Pi
            "24 06 01 62 B1 A9 F3 4B 91 AE 1F 21 3A 39 B0 5B " +
            "BF 2F E1 BC DD EC E7 C6 B5 FE 28 4F DD BD EC B6 " +
            "6B 83 34 EA 35 51 11 AD EB D9 11 55 4F ED 6E B8 " +
            "CB B6 8F 43 D4 C2 C2 7A 6E 77 61 7A 6B FA 1D D1 " +
            "9B D1 5E 88 26 F7 ED 7A 58 F4 25 D2 CD F4 88 C6 " +
            "D7 A3 B0 F2 96 E8 C0 79 35 6E CD BA 51 28 31 8B " +
            "7F B2 02 FE 9A 1A 8C 2C 68 0D EA E6 BB F8 89 D3 " +
            "F3 B5 32 92 2E 2F E8 B3 51 B6 97 AD 3F CD 71 9F " +
            "7C D6 7F 6F DE CE 3B F7 5B 2B 0A 93 4C 1A E1 F8 " +
            "CF 51 82 FC 52 52 F7 7D 31 F8 60 77 29 75 56 0D " +
            "D3 E4 57 91 84 DD 08 85 87 16 B9 35 DE FE 72 B5 " +
            "94 49 B3 3D 49 CB B8 BC A9 38 7A A2 CE BC F9 3B " +
            "C0 F1 D0 00 48 A2 65 1E",
            // After Chi
            "0A 07 E1 FE 74 6D B4 CF 91 7E 17 62 3A 28 B8 6B " +
            "F5 2E F5 1C FD AC F6 CF B1 FA 29 4F 5D 15 0E F4 " +
            "FA 2B 2A EB 3F 41 11 BD CF 98 71 6D 64 D5 73 39 " +
            "5A 36 91 C3 D0 C7 22 50 2E 53 40 28 A2 FA 1D 55 " +
            "38 D8 4E 8D 24 FE 8B 42 58 D2 AB D0 5D F6 08 84 " +
            "9D 33 B2 B6 1C FA 4C 5D 35 63 25 BA 70 C8 30 58 " +
            "EC 02 12 EE 9E 1D EC 0C 6C 0F 6A 86 2B 38 89 9B " +
            "D3 F9 7F 9A 6F 2F D9 31 52 9F 97 3D 3F DD B1 97 " +
            "F8 86 FF 03 CC 8E 2D F2 6B 83 6A 90 65 3F E1 F8 " +
            "8F 57 15 74 44 DA D6 EF 1D B8 08 35 E9 77 5C 6D " +
            "C3 AD 55 99 85 DC 80 8D AE 26 F1 B7 58 CA 33 B6 " +
            "D4 88 33 3D 49 C9 BC B8 BA 3C 7D 33 4A E1 F1 BA " +
            "C4 E3 78 24 12 80 17 2E",
            // After Iota 
            "03 87 E1 7E 74 6D B4 CF 91 7E 17 62 3A 28 B8 6B " +
            "F5 2E F5 1C FD AC F6 CF B1 FA 29 4F 5D 15 0E F4 " +
            "FA 2B 2A EB 3F 41 11 BD CF 98 71 6D 64 D5 73 39 " +
            "5A 36 91 C3 D0 C7 22 50 2E 53 40 28 A2 FA 1D 55 " +
            "38 D8 4E 8D 24 FE 8B 42 58 D2 AB D0 5D F6 08 84 " +
            "9D 33 B2 B6 1C FA 4C 5D 35 63 25 BA 70 C8 30 58 " +
            "EC 02 12 EE 9E 1D EC 0C 6C 0F 6A 86 2B 38 89 9B " +
            "D3 F9 7F 9A 6F 2F D9 31 52 9F 97 3D 3F DD B1 97 " +
            "F8 86 FF 03 CC 8E 2D F2 6B 83 6A 90 65 3F E1 F8 " +
            "8F 57 15 74 44 DA D6 EF 1D B8 08 35 E9 77 5C 6D " +
            "C3 AD 55 99 85 DC 80 8D AE 26 F1 B7 58 CA 33 B6 " +
            "D4 88 33 3D 49 C9 BC B8 BA 3C 7D 33 4A E1 F1 BA " +
            "C4 E3 78 24 12 80 17 2E",
            // Round #11
            // After Theta
            "FB CB 34 91 9F C4 57 CB 40 89 0B 8C 56 10 B7 76 " +
            "FD 28 92 B5 4B 1F 15 18 69 39 CB 59 5D 77 42 B5 " +
            "AB 50 8F EB 0F 2E 4E A6 37 D4 A4 82 8F 7C 90 3D " +
            "8B C1 8D 2D BC FF 2D 4D 26 55 27 81 14 49 FE 82 " +
            "E0 1B AC 9B 24 9C C7 03 09 A9 0E D0 6D 99 57 9F " +
            "65 7F 67 59 F7 53 AF 59 E4 94 39 54 1C F0 3F 45 " +
            "E4 04 75 47 28 AE 0F DB B4 CC 88 90 2B 5A C5 DA " +
            "82 82 DA 9A 5F 40 86 2A AA D3 42 D2 D4 74 52 93 " +
            "29 71 E3 ED A0 B6 22 EF 63 85 0D 39 D3 8C 02 2F " +
            "57 94 F7 62 44 B8 9A AE 4C C3 AD 35 D9 18 03 76 " +
            "3B E1 80 76 6E 75 63 89 7F D1 ED 59 34 F2 3C AB " +
            "DC 8E 54 94 FF 7A 5F 6F 62 FF 9F 25 4A 83 BD FB " +
            "95 98 DD 24 22 EF 48 35",
            // After Rho
            "FB CB 34 91 9F C4 57 CB 80 12 17 18 AD 20 6E ED " +
            "3F 8A 64 ED D2 47 05 46 75 27 54 9B 96 B3 9C D5 " +
            "70 71 32 5D 85 7A 5C 7F F8 C8 07 D9 73 43 4D 2A " +
            "D8 C2 FB DF D2 B4 18 DC A0 49 D5 49 20 45 92 BF " +
            "0D D6 4D 12 CE E3 01 F0 79 F5 99 90 EA 00 DD 96 " +
            "2A FB 3B CB BA 9F 7A CD 14 91 53 E6 50 71 C0 FF " +
            "3B 42 71 7D D8 26 27 A8 B4 8A B5 69 99 11 21 57 " +
            "CD 2F 20 43 15 41 41 6D A4 A9 E9 A4 26 55 A7 85 " +
            "BC 1D D4 56 E4 3D 25 6E 81 97 B1 C2 86 9C 69 46 " +
            "57 D3 F5 8A F2 5E 8C 08 76 4C C3 AD 35 D9 18 03 " +
            "8D 25 EE 84 03 DA B9 D5 FE 45 B7 67 D1 C8 F3 AC " +
            "DB 91 8A F2 5F EF EB 8D FF 9F 25 4A 83 BD FB 62 " +
            "52 4D 25 66 37 89 C8 3B",
            // After Pi
            "FB CB 34 91 9F C4 57 CB D8 C2 FB DF D2 B4 18 DC " +
            "3B 42 71 7D D8 26 27 A8 57 D3 F5 8A F2 5E 8C 08 " +
            "52 4D 25 66 37 89 C8 3B 75 27 54 9B 96 B3 9C D5 " +
            "79 F5 99 90 EA 00 DD 96 2A FB 3B CB BA 9F 7A CD " +
            "BC 1D D4 56 E4 3D 25 6E DB 91 8A F2 5F EF EB 8D " +
            "80 12 17 18 AD 20 6E ED A0 49 D5 49 20 45 92 BF " +
            "B4 8A B5 69 99 11 21 57 76 4C C3 AD 35 D9 18 03 " +
            "8D 25 EE 84 03 DA B9 D5 70 71 32 5D 85 7A 5C 7F " +
            "F8 C8 07 D9 73 43 4D 2A 14 91 53 E6 50 71 C0 FF " +
            "81 97 B1 C2 86 9C 69 46 FF 9F 25 4A 83 BD FB 62 " +
            "3F 8A 64 ED D2 47 05 46 0D D6 4D 12 CE E3 01 F0 " +
            "CD 2F 20 43 15 41 41 6D A4 A9 E9 A4 26 55 A7 85 " +
            "FE 45 B7 67 D1 C8 F3 AC",
            // After Chi
            "D8 CB 34 B1 97 C6 70 EB 9C 53 7F 5D F0 EC 90 DC " +
            "3B 4E 71 19 DD A7 67 9B FE 51 E5 1B 7A 1A 9B C8 " +
            "52 4D EE 28 77 B9 C0 2F 77 2D 76 D0 86 2C BE 9C " +
            "ED F1 5D 84 AE 20 D8 B4 69 7B 31 6B A1 5D B0 4C " +
            "98 3B 80 5F 64 2D 31 3E D3 41 03 F2 37 EF AA 8F " +
            "94 90 37 38 34 30 4F AD E2 0D 97 CD 04 8D 8A BF " +
            "3D AB 99 69 9B 13 80 83 76 5E D2 B5 99 F9 5E 2B " +
            "AD 6C 2E C5 03 9F 29 C7 74 60 62 7B 85 4A DC AA " +
            "79 CE A7 D9 F5 CF 64 2A 6A 99 57 EE 51 50 52 DF " +
            "81 F7 A3 D7 82 DE 6D 5B 77 17 20 CA F1 BC FA 62 " +
            "FF A3 44 AC C3 47 45 4B 2D 56 84 B6 EC F7 A7 70 " +
            "97 6B 36 00 C4 C9 11 45 A5 23 A9 2C 24 52 A3 C7 " +
            "FE 11 BE 75 DD 68 F3 1C",
            // After Iota 
            "D2 CB 34 31 97 C6 70 EB 9C 53 7F 5D F0 EC 90 DC " +
            "3B 4E 71 19 DD A7 67 9B FE 51 E5 1B 7A 1A 9B C8 " +
            "52 4D EE 28 77 B9 C0 2F 77 2D 76 D0 86 2C BE 9C " +
            "ED F1 5D 84 AE 20 D8 B4 69 7B 31 6B A1 5D B0 4C " +
            "98 3B 80 5F 64 2D 31 3E D3 41 03 F2 37 EF AA 8F " +
            "94 90 37 38 34 30 4F AD E2 0D 97 CD 04 8D 8A BF " +
            "3D AB 99 69 9B 13 80 83 76 5E D2 B5 99 F9 5E 2B " +
            "AD 6C 2E C5 03 9F 29 C7 74 60 62 7B 85 4A DC AA " +
            "79 CE A7 D9 F5 CF 64 2A 6A 99 57 EE 51 50 52 DF " +
            "81 F7 A3 D7 82 DE 6D 5B 77 17 20 CA F1 BC FA 62 " +
            "FF A3 44 AC C3 47 45 4B 2D 56 84 B6 EC F7 A7 70 " +
            "97 6B 36 00 C4 C9 11 45 A5 23 A9 2C 24 52 A3 C7 " +
            "FE 11 BE 75 DD 68 F3 1C",
            // Round #12
            // After Theta
            "F8 C2 45 66 7E 29 38 E8 03 3F 5C B8 76 DB A0 7B " +
            "94 B9 9C 77 DC 5A 12 94 26 F0 E7 AE D7 50 1B 34 " +
            "12 C6 F4 3E 90 55 CB 18 5D 24 07 87 6F C3 F6 9F " +
            "72 9D 7E 61 28 17 E8 13 C6 8C DC 05 A0 A0 C5 43 " +
            "40 9A 82 EA C9 67 B1 C2 93 CA 19 E4 D0 03 A1 B8 " +
            "BE 99 46 6F DD DF 07 AE 7D 61 B4 28 82 BA BA 18 " +
            "92 5C 74 07 9A EE F5 8C AE FF D0 00 34 B3 DE D7 " +
            "ED E7 34 D3 E4 73 22 F0 5E 69 13 2C 6C A5 94 A9 " +
            "E6 A2 84 3C 73 F8 54 8D C5 6E BA 80 50 AD 27 D0 " +
            "59 56 A1 62 2F 94 ED A7 37 9C 3A DC 16 50 F1 55 " +
            "D5 AA 35 FB 2A A8 0D 48 B2 3A A7 53 6A C0 97 D7 " +
            "38 9C DB 6E C5 34 64 4A 7D 82 AB 99 89 18 23 3B " +
            "BE 9A A4 63 3A 84 F8 2B",
            // After Rho
            "F8 C2 45 66 7E 29 38 E8 06 7E B8 70 ED B6 41 F7 " +
            "65 2E E7 1D B7 96 04 25 0D B5 41 63 02 7F EE 7A " +
            "AC 5A C6 90 30 A6 F7 81 F8 36 6C FF D9 45 72 70 " +
            "17 86 72 81 3E 21 D7 E9 90 31 23 77 01 28 68 F1 " +
            "4D 41 F5 E4 B3 58 61 20 10 8A 3B A9 9C 41 0E 3D " +
            "F5 CD 34 7A EB FE 3E 70 62 F4 85 D1 A2 08 EA EA " +
            "3B D0 74 AF 67 94 E4 A2 66 BD AF 5D FF A1 01 68 " +
            "69 F2 39 11 F8 F6 73 9A 58 D8 4A 29 53 BD D2 26 " +
            "90 67 0E 9F AA D1 5C 94 13 E8 62 37 5D 40 A8 D6 " +
            "B2 FD 34 CB 2A 54 EC 85 55 37 9C 3A DC 16 50 F1 " +
            "36 20 55 AB D6 EC AB A0 CB EA 9C 4E A9 01 5F 5E " +
            "87 73 DB AD 98 86 4C 09 82 AB 99 89 18 23 3B 7D " +
            "FE 8A AF 26 E9 98 0E 21",
            // After Pi
            "F8 C2 45 66 7E 29 38 E8 17 86 72 81 3E 21 D7 E9 " +
            "3B D0 74 AF 67 94 E4 A2 B2 FD 34 CB 2A 54 EC 85 " +
            "FE 8A AF 26 E9 98 0E 21 0D B5 41 63 02 7F EE 7A " +
            "10 8A 3B A9 9C 41 0E 3D F5 CD 34 7A EB FE 3E 70 " +
            "90 67 0E 9F AA D1 5C 94 87 73 DB AD 98 86 4C 09 " +
            "06 7E B8 70 ED B6 41 F7 90 31 23 77 01 28 68 F1 " +
            "66 BD AF 5D FF A1 01 68 55 37 9C 3A DC 16 50 F1 " +
            "36 20 55 AB D6 EC AB A0 AC 5A C6 90 30 A6 F7 81 " +
            "F8 36 6C FF D9 45 72 70 62 F4 85 D1 A2 08 EA EA " +
            "13 E8 62 37 5D 40 A8 D6 82 AB 99 89 18 23 3B 7D " +
            "65 2E E7 1D B7 96 04 25 4D 41 F5 E4 B3 58 61 20 " +
            "69 F2 39 11 F8 F6 73 9A 58 D8 4A 29 53 BD D2 26 " +
            "CB EA 9C 4E A9 01 5F 5E",
            // After Chi
            "D0 92 41 48 3F BD 18 EA 97 AB 72 C1 36 61 DF EC " +
            "77 D2 FF 8B A6 1C E6 82 B2 BD 74 8B 3C 75 DC 4D " +
            "F9 8E 9D A7 E9 98 C9 20 E8 F0 45 31 61 C1 DE 3A " +
            "10 A8 31 2C 9C 40 4E B9 F2 DD E5 5A FB F8 3E 79 " +
            "98 E3 0E DD A8 A8 FE E6 97 79 E1 25 04 86 4C 0C " +
            "60 F2 34 78 13 37 40 FF 81 33 33 55 01 3E 38 60 " +
            "44 BD EE DC FD 49 AA 68 55 69 34 6A F5 04 10 A6 " +
            "A6 21 56 AC D6 E4 83 A0 AE 9A 47 90 12 AE 7F 0B " +
            "E9 3E 0E D9 84 05 72 64 E2 F7 1C 59 A2 2B F9 C3 " +
            "3F B8 24 27 7D C4 6C 56 D2 8F B1 E6 D1 62 3B 0D " +
            "45 9C EF 0C FF 30 16 BF 5D 49 B7 CC B0 51 E1 04 " +
            "EA D0 AD 57 50 F6 7E C2 7C DC 29 38 45 2B D2 07 " +
            "C3 AB 8C AE A9 49 3E 5E",
            // After Iota 
            "5B 12 41 C8 3F BD 18 EA 97 AB 72 C1 36 61 DF EC " +
            "77 D2 FF 8B A6 1C E6 82 B2 BD 74 8B 3C 75 DC 4D " +
            "F9 8E 9D A7 E9 98 C9 20 E8 F0 45 31 61 C1 DE 3A " +
            "10 A8 31 2C 9C 40 4E B9 F2 DD E5 5A FB F8 3E 79 " +
            "98 E3 0E DD A8 A8 FE E6 97 79 E1 25 04 86 4C 0C " +
            "60 F2 34 78 13 37 40 FF 81 33 33 55 01 3E 38 60 " +
            "44 BD EE DC FD 49 AA 68 55 69 34 6A F5 04 10 A6 " +
            "A6 21 56 AC D6 E4 83 A0 AE 9A 47 90 12 AE 7F 0B " +
            "E9 3E 0E D9 84 05 72 64 E2 F7 1C 59 A2 2B F9 C3 " +
            "3F B8 24 27 7D C4 6C 56 D2 8F B1 E6 D1 62 3B 0D " +
            "45 9C EF 0C FF 30 16 BF 5D 49 B7 CC B0 51 E1 04 " +
            "EA D0 AD 57 50 F6 7E C2 7C DC 29 38 45 2B D2 07 " +
            "C3 AB 8C AE A9 49 3E 5E",
            // Round #13
            // After Theta
            "E6 6F C4 F5 43 FB 6F 9F 3C 96 61 DA 32 54 DA 52 " +
            "BD 33 B0 60 8B 3B C4 6E C8 CD 1E 44 E8 A7 2E 61 " +
            "B4 F1 EE BF F0 05 9A 4B 55 8D C0 0C 1D 87 A9 4F " +
            "BB 95 22 37 98 75 4B 07 38 3C AA B1 D6 DF 1C 95 " +
            "E2 93 64 12 7C 7A 0C CA DA 06 92 3D 1D 1B 1F 67 " +
            "DD 8F B1 45 6F 71 37 8A 2A 0E 20 4E 05 0B 3D DE " +
            "8E 5C A1 37 D0 6E 88 84 2F 19 5E A5 21 D6 E2 8A " +
            "EB 5E 25 B4 CF 79 D0 CB 13 E7 C2 AD 6E E8 08 7E " +
            "42 03 1D C2 80 30 77 DA 28 16 53 B2 8F 0C DB 2F " +
            "45 C8 4E E8 A9 16 9E 7A 9F F0 C2 FE C8 FF 68 66 " +
            "F8 E1 6A 31 83 76 61 CA F6 74 A4 D7 B4 64 E4 BA " +
            "20 31 E2 BC 7D D1 5C 2E 06 AC 43 F7 91 F9 20 2B " +
            "8E D4 FF B6 B0 D4 6D 35",
            // After Rho
            "E6 6F C4 F5 43 FB 6F 9F 78 2C C3 B4 65 A8 B4 A5 " +
            "EF 0C 2C D8 E2 0E B1 5B 7E EA 12 86 DC EC 41 84 " +
            "2F D0 5C A2 8D 77 FF 85 D0 71 98 FA 54 D5 08 CC " +
            "72 83 59 B7 74 B0 5B 29 25 0E 8F 6A AC F5 37 47 " +
            "49 32 09 3E 3D 06 65 F1 F1 71 A6 6D 20 D9 D3 B1 " +
            "EC 7E 8C 2D 7A 8B BB 51 78 AB 38 80 38 15 2C F4 " +
            "BD 81 76 43 24 74 E4 0A AC C5 15 5F 32 BC 4A 43 " +
            "DA E7 3C E8 E5 75 AF 12 5B DD D0 11 FC 26 CE 85 " +
            "43 18 10 E6 4E 5B 68 A0 ED 17 14 8B 29 D9 47 86 " +
            "C2 53 AF 08 D9 09 3D D5 66 9F F0 C2 FE C8 FF 68 " +
            "85 29 E3 87 AB C5 0C DA DA D3 91 5E D3 92 91 EB " +
            "24 46 9C B7 2F 9A CB 05 AC 43 F7 91 F9 20 2B 06 " +
            "5B 8D 23 F5 BF 2D 2C 75",
            // After Pi
            "E6 6F C4 F5 43 FB 6F 9F 72 83 59 B7 74 B0 5B 29 " +
            "BD 81 76 43 24 74 E4 0A C2 53 AF 08 D9 09 3D D5 " +
            "5B 8D 23 F5 BF 2D 2C 75 7E EA 12 86 DC EC 41 84 " +
            "F1 71 A6 6D 20 D9 D3 B1 EC 7E 8C 2D 7A 8B BB 51 " +
            "43 18 10 E6 4E 5B 68 A0 24 46 9C B7 2F 9A CB 05 " +
            "78 2C C3 B4 65 A8 B4 A5 25 0E 8F 6A AC F5 37 47 " +
            "AC C5 15 5F 32 BC 4A 43 66 9F F0 C2 FE C8 FF 68 " +
            "85 29 E3 87 AB C5 0C DA 2F D0 5C A2 8D 77 FF 85 " +
            "D0 71 98 FA 54 D5 08 CC 78 AB 38 80 38 15 2C F4 " +
            "ED 17 14 8B 29 D9 47 86 AC 43 F7 91 F9 20 2B 06 " +
            "EF 0C 2C D8 E2 0E B1 5B 49 32 09 3E 3D 06 65 F1 " +
            "DA E7 3C E8 E5 75 AF 12 5B DD D0 11 FC 26 CE 85 " +
            "DA D3 91 5E D3 92 91 EB",
            // After Chi
            "6B 6F E2 B5 43 BF CB 9D 30 D1 D0 BF AD B9 42 FC " +
            "A4 0D 76 B6 02 50 E4 2A 66 31 6B 08 99 DB 7E 5F " +
            "4B 0D 3A F7 8B 2D 3C 55 72 E4 1A 86 86 EE 69 C4 " +
            "F2 71 B6 AF 24 89 93 11 C8 38 00 3C 5B 0B 38 54 " +
            "19 B0 12 E6 9E 3F 68 20 A5 57 38 DE 0F 8B 59 34 " +
            "F0 ED D3 A1 77 A0 FC A5 67 14 6F EA 60 B5 82 6F " +
            "2D E5 16 5A 33 B9 4A D1 1E 9B F0 F2 BA E0 4F 4D " +
            "80 2B EF CD 23 90 0F 98 07 5A 7C A2 A5 77 DB B5 " +
            "55 65 9C F1 55 1D 4B CE 78 EB DB 90 E8 35 04 F4 " +
            "EE 87 1C A9 2D 8E 93 07 7C 62 77 C9 A9 A0 2B 4E " +
            "7D C9 18 18 22 7F 3B 59 48 2A C9 2F 25 04 25 74 " +
            "5A E5 3D A6 E6 E5 BE 78 7E D1 FC 91 DC 2A EE 95 " +
            "DA E1 90 78 CE 92 D5 4B",
            // After Iota 
            "E0 6F E2 B5 43 BF CB 1D 30 D1 D0 BF AD B9 42 FC " +
            "A4 0D 76 B6 02 50 E4 2A 66 31 6B 08 99 DB 7E 5F " +
            "4B 0D 3A F7 8B 2D 3C 55 72 E4 1A 86 86 EE 69 C4 " +
            "F2 71 B6 AF 24 89 93 11 C8 38 00 3C 5B 0B 38 54 " +
            "19 B0 12 E6 9E 3F 68 20 A5 57 38 DE 0F 8B 59 34 " +
            "F0 ED D3 A1 77 A0 FC A5 67 14 6F EA 60 B5 82 6F " +
            "2D E5 16 5A 33 B9 4A D1 1E 9B F0 F2 BA E0 4F 4D " +
            "80 2B EF CD 23 90 0F 98 07 5A 7C A2 A5 77 DB B5 " +
            "55 65 9C F1 55 1D 4B CE 78 EB DB 90 E8 35 04 F4 " +
            "EE 87 1C A9 2D 8E 93 07 7C 62 77 C9 A9 A0 2B 4E " +
            "7D C9 18 18 22 7F 3B 59 48 2A C9 2F 25 04 25 74 " +
            "5A E5 3D A6 E6 E5 BE 78 7E D1 FC 91 DC 2A EE 95 " +
            "DA E1 90 78 CE 92 D5 4B",
            // Round #14
            // After Theta
            "58 6A 51 A8 B1 82 24 91 EE 98 92 5A 51 24 A4 2A " +
            "FF 6F F8 DA 03 8C 90 52 94 0A F8 44 7D E0 7A 85 " +
            "8B AB CC 83 AD 7F 65 D4 CA E1 A9 9B 74 D3 86 48 " +
            "2C 38 F4 4A D8 14 75 C7 93 5A 8E 50 5A D7 4C 2C " +
            "EB 8B 81 AA 7A 04 6C FA 65 F1 CE AA 29 D9 00 B5 " +
            "48 E8 60 BC 85 9D 13 29 B9 5D 2D 0F 9C 28 64 B9 " +
            "76 87 98 36 32 65 3E A9 EC A0 63 BE 5E DB 4B 97 " +
            "40 8D 19 B9 05 C2 56 19 BF 5F CF BF 57 4A 34 39 " +
            "8B 2C DE 14 A9 80 AD 18 23 89 55 FC E9 E9 70 8C " +
            "1C BC 8F E5 C9 B5 97 DD BC C4 81 BD 8F F2 72 CF " +
            "C5 CC AB 05 D0 42 D4 D5 96 63 8B CA D9 99 C3 A2 " +
            "01 87 B3 CA E7 39 CA 00 8C EA 6F DD 38 11 EA 4F " +
            "1A 47 66 0C E8 C0 8C CA",
            // After Rho
            "58 6A 51 A8 B1 82 24 91 DC 31 25 B5 A2 48 48 55 " +
            "FF 1B BE F6 00 23 A4 D4 07 AE 57 48 A9 80 4F D4 " +
            "FD 2B A3 5E 5C 65 1E 6C 49 37 6D 88 A4 1C 9E BA " +
            "AF 84 4D 51 77 CC 82 43 CB A4 96 23 94 D6 35 13 " +
            "C5 40 55 3D 02 36 FD F5 0D 50 5B 16 EF AC 9A 92 " +
            "41 42 07 E3 2D EC 9C 48 E5 E6 76 B5 3C 70 A2 90 " +
            "B4 91 29 F3 49 B5 3B C4 B6 97 2E D9 41 C7 7C BD " +
            "DC 02 61 AB 0C A0 C6 8C 7F AF 94 68 72 7E BF 9E " +
            "9B 22 15 B0 15 63 91 C5 38 C6 91 C4 2A FE F4 74 " +
            "F6 B2 9B 83 F7 B1 3C B9 CF BC C4 81 BD 8F F2 72 " +
            "51 57 17 33 AF 16 40 0B 5A 8E 2D 2A 67 67 0E 8B " +
            "E0 70 56 F9 3C 47 19 20 EA 6F DD 38 11 EA 4F 8C " +
            "A3 B2 C6 91 19 03 3A 30",
            // After Pi
            "58 6A 51 A8 B1 82 24 91 AF 84 4D 51 77 CC 82 43 " +
            "B4 91 29 F3 49 B5 3B C4 F6 B2 9B 83 F7 B1 3C B9 " +
            "A3 B2 C6 91 19 03 3A 30 07 AE 57 48 A9 80 4F D4 " +
            "0D 50 5B 16 EF AC 9A 92 41 42 07 E3 2D EC 9C 48 " +
            "9B 22 15 B0 15 63 91 C5 E0 70 56 F9 3C 47 19 20 " +
            "DC 31 25 B5 A2 48 48 55 CB A4 96 23 94 D6 35 13 " +
            "B6 97 2E D9 41 C7 7C BD CF BC C4 81 BD 8F F2 72 " +
            "51 57 17 33 AF 16 40 0B FD 2B A3 5E 5C 65 1E 6C " +
            "49 37 6D 88 A4 1C 9E BA E5 E6 76 B5 3C 70 A2 90 " +
            "38 C6 91 C4 2A FE F4 74 EA 6F DD 38 11 EA 4F 8C " +
            "FF 1B BE F6 00 23 A4 D4 C5 40 55 3D 02 36 FD F5 " +
            "DC 02 61 AB 0C A0 C6 8C 7F AF 94 68 72 7E BF 9E " +
            "5A 8E 2D 2A 67 67 0E 8B",
            // After Chi
            "48 7B 71 0A B9 B3 1D 15 ED A6 DF 51 C1 CC 86 7A " +
            "B5 91 6D E3 41 B7 39 C4 AE FA 8A AB 57 31 38 38 " +
            "04 36 CA C0 5F 4F B8 72 47 AC 53 A9 A9 C0 4B 9C " +
            "97 70 4B 06 FF AF 9B 17 21 12 45 AA 05 E8 94 68 " +
            "9C AC 14 B0 94 E3 D7 11 E8 20 5E EF 7A 6B 89 22 " +
            "E8 22 0D 6D E3 49 00 F9 82 8C 56 23 28 DE B7 51 " +
            "A6 D4 3D EB 43 D7 7C B4 43 9C E4 05 BD C7 FA 26 " +
            "52 D3 85 31 BB 80 75 09 59 EB B1 6B 44 05 3E 6C " +
            "51 37 EC C8 A6 92 CA DE 27 CF 3A 8D 2D 70 A9 18 " +
            "2D C6 B3 82 66 FB E4 14 EA 7B 91 B8 B1 F2 CF 1E " +
            "E7 19 9E 74 0C A3 A6 DC E6 ED C1 7D 70 68 C4 E7 " +
            "DC 02 48 A9 09 A1 C6 8D DA BE 06 BC 72 7E 1F CA " +
            "5A CE 6C 23 65 73 57 AA",
            // After Iota 
            "C1 FB 71 0A B9 B3 1D 95 ED A6 DF 51 C1 CC 86 7A " +
            "B5 91 6D E3 41 B7 39 C4 AE FA 8A AB 57 31 38 38 " +
            "04 36 CA C0 5F 4F B8 72 47 AC 53 A9 A9 C0 4B 9C " +
            "97 70 4B 06 FF AF 9B 17 21 12 45 AA 05 E8 94 68 " +
            "9C AC 14 B0 94 E3 D7 11 E8 20 5E EF 7A 6B 89 22 " +
            "E8 22 0D 6D E3 49 00 F9 82 8C 56 23 28 DE B7 51 " +
            "A6 D4 3D EB 43 D7 7C B4 43 9C E4 05 BD C7 FA 26 " +
            "52 D3 85 31 BB 80 75 09 59 EB B1 6B 44 05 3E 6C " +
            "51 37 EC C8 A6 92 CA DE 27 CF 3A 8D 2D 70 A9 18 " +
            "2D C6 B3 82 66 FB E4 14 EA 7B 91 B8 B1 F2 CF 1E " +
            "E7 19 9E 74 0C A3 A6 DC E6 ED C1 7D 70 68 C4 E7 " +
            "DC 02 48 A9 09 A1 C6 8D DA BE 06 BC 72 7E 1F CA " +
            "5A CE 6C 23 65 73 57 AA",
            // Round #15
            // After Theta
            "51 8B 42 0C 72 19 89 73 AE 14 10 8C 3D E2 34 21 " +
            "F7 74 1D 63 55 D0 40 62 7A 80 35 26 E1 22 3E 6E " +
            "22 8B 04 42 42 E6 CB 22 D7 DC 60 AF 62 6A DF 7A " +
            "D4 C2 84 DB 03 81 29 4C 63 F7 35 2A 11 8F ED CE " +
            "48 D6 AB 3D 22 F0 D1 47 CE 9D 90 6D 67 C2 FA 72 " +
            "78 52 3E 6B 28 E3 94 1F C1 3E 99 FE D4 F0 05 0A " +
            "E4 31 4D 6B 57 B0 05 12 97 E6 5B 88 0B D4 FC 70 " +
            "74 6E 4B B3 A6 29 06 59 C9 9B 82 6D 8F AF AA 8A " +
            "12 85 23 15 5A BC 78 85 65 2A 4A 0D 39 17 D0 BE " +
            "F9 BC 0C 0F D0 E8 E2 42 CC C6 5F 3A AC 5B BC 4E " +
            "77 69 AD 72 C7 09 32 3A A5 5F 0E A0 8C 46 76 BC " +
            "9E E7 38 29 1D C6 BF 2B 0E C4 B9 31 C4 6D 19 9C " +
            "7C 73 A2 A1 78 DA 24 FA",
            // After Rho
            "51 8B 42 0C 72 19 89 73 5C 29 20 18 7B C4 69 42 " +
            "3D 5D C7 58 15 34 90 D8 2E E2 E3 A6 07 58 63 12 " +
            "32 5F 16 11 59 24 10 12 2A A6 F6 AD 77 CD 0D F6 " +
            "B8 3D 10 98 C2 44 2D 4C F3 D8 7D 8D 4A C4 63 BB " +
            "EB D5 1E 11 F8 E8 23 24 AC 2F E7 DC 09 D9 76 26 " +
            "C0 93 F2 59 43 19 A7 FC 28 04 FB 64 FA 53 C3 17 " +
            "5A BB 82 2D 90 20 8F 69 A8 F9 E1 2E CD B7 10 17 " +
            "59 D3 14 83 2C 3A B7 A5 DB 1E 5F 55 15 93 37 05 " +
            "A4 42 8B 17 AF 50 A2 70 68 DF 32 15 A5 86 9C 0B " +
            "5D 5C 28 9F 97 E1 01 1A 4E CC C6 5F 3A AC 5B BC " +
            "C8 E8 DC A5 B5 CA 1D 27 96 7E 39 80 32 1A D9 F1 " +
            "F3 1C 27 A5 C3 F8 77 C5 C4 B9 31 C4 6D 19 9C 0E " +
            "89 3E DF 9C 68 28 9E 36",
            // After Pi
            "51 8B 42 0C 72 19 89 73 B8 3D 10 98 C2 44 2D 4C " +
            "5A BB 82 2D 90 20 8F 69 5D 5C 28 9F 97 E1 01 1A " +
            "89 3E DF 9C 68 28 9E 36 2E E2 E3 A6 07 58 63 12 " +
            "AC 2F E7 DC 09 D9 76 26 C0 93 F2 59 43 19 A7 FC " +
            "A4 42 8B 17 AF 50 A2 70 F3 1C 27 A5 C3 F8 77 C5 " +
            "5C 29 20 18 7B C4 69 42 F3 D8 7D 8D 4A C4 63 BB " +
            "A8 F9 E1 2E CD B7 10 17 4E CC C6 5F 3A AC 5B BC " +
            "C8 E8 DC A5 B5 CA 1D 27 32 5F 16 11 59 24 10 12 " +
            "2A A6 F6 AD 77 CD 0D F6 28 04 FB 64 FA 53 C3 17 " +
            "68 DF 32 15 A5 86 9C 0B C4 B9 31 C4 6D 19 9C 0E " +
            "3D 5D C7 58 15 34 90 D8 EB D5 1E 11 F8 E8 23 24 " +
            "59 D3 14 83 2C 3A B7 A5 DB 1E 5F 55 15 93 37 05 " +
            "96 7E 39 80 32 1A D9 F1",
            // After Chi
            "13 09 C0 29 62 39 0B 52 BD 79 38 0A C5 85 2D 5E " +
            "DA 99 55 2D F8 28 11 4D 0D DD 28 9F 85 F0 00 5B " +
            "21 0A CF 0C E8 6C BA 3A 6E 72 F3 A7 45 58 E2 CA " +
            "88 6F EE DA A5 99 76 26 93 8F D6 F9 03 B1 F2 79 " +
            "A8 A0 4B 15 AB 50 A2 62 73 11 23 FD CB 79 63 E1 " +
            "54 08 A0 3A FE F7 79 46 B5 DC 7B DC 78 CC 28 13 " +
            "28 D9 F9 8E 48 F5 14 14 5A CD E6 47 70 A8 3B FC " +
            "6B 38 81 20 B5 CA 1F 9E 32 5F 1F 51 D1 36 D2 13 " +
            "6A 7D F6 BC 72 49 11 FE AC 24 FA A4 B2 4A C3 13 " +
            "5A 99 34 04 B5 A2 9C 1B CC 19 D1 68 4B D0 91 EA " +
            "2D 5F C7 DA 11 26 04 59 69 D9 55 45 E9 69 23 24 " +
            "5D B3 34 03 0E 32 7F 55 F2 1F 99 0D 10 B7 37 0D " +
            "54 FE 21 81 DA D2 FA D5",
            // After Iota 
            "10 89 C0 29 62 39 0B D2 BD 79 38 0A C5 85 2D 5E " +
            "DA 99 55 2D F8 28 11 4D 0D DD 28 9F 85 F0 00 5B " +
            "21 0A CF 0C E8 6C BA 3A 6E 72 F3 A7 45 58 E2 CA " +
            "88 6F EE DA A5 99 76 26 93 8F D6 F9 03 B1 F2 79 " +
            "A8 A0 4B 15 AB 50 A2 62 73 11 23 FD CB 79 63 E1 " +
            "54 08 A0 3A FE F7 79 46 B5 DC 7B DC 78 CC 28 13 " +
            "28 D9 F9 8E 48 F5 14 14 5A CD E6 47 70 A8 3B FC " +
            "6B 38 81 20 B5 CA 1F 9E 32 5F 1F 51 D1 36 D2 13 " +
            "6A 7D F6 BC 72 49 11 FE AC 24 FA A4 B2 4A C3 13 " +
            "5A 99 34 04 B5 A2 9C 1B CC 19 D1 68 4B D0 91 EA " +
            "2D 5F C7 DA 11 26 04 59 69 D9 55 45 E9 69 23 24 " +
            "5D B3 34 03 0E 32 7F 55 F2 1F 99 0D 10 B7 37 0D " +
            "54 FE 21 81 DA D2 FA D5",
            // Round #16
            // After Theta
            "B6 90 41 FB 62 05 25 CA A8 3B 1B CE C3 2B FD 86 " +
            "F6 9B 0B 50 8C E3 34 5A DF 0C A7 13 84 5E 10 C8 " +
            "1C DA 70 B6 21 7D 05 C1 C8 6B 72 75 45 64 CC D2 " +
            "9D 2D CD 1E A3 37 A6 FE BF 8D 88 84 77 7A D7 6E " +
            "7A 71 C4 99 AA FE B2 F1 4E C1 9C 47 02 68 DC 1A " +
            "F2 11 21 E8 FE CB 57 5E A0 9E 58 18 7E 62 F8 CB " +
            "04 DB A7 F3 3C 3E 31 03 88 1C 69 CB 71 06 2B 6F " +
            "56 E8 3E 9A 7C DB A0 65 94 46 9E 83 D1 0A FC 0B " +
            "7F 3F D5 78 74 E7 C1 26 80 26 A4 D9 C6 81 E6 04 " +
            "88 48 BB 88 B4 0C 8C 88 F1 C9 6E D2 82 C1 2E 11 " +
            "8B 46 46 08 11 1A 2A 41 7C 9B 76 81 EF C7 F3 FC " +
            "71 B1 6A 7E 7A F9 5A 42 20 CE 16 81 11 19 27 9E " +
            "69 2E 9E 3B 13 C3 45 2E",
            // After Rho
            "B6 90 41 FB 62 05 25 CA 51 77 36 9C 87 57 FA 0D " +
            "FD E6 02 14 E3 38 8D 96 E8 05 81 FC CD 70 3A 41 " +
            "E9 2B 08 E6 D0 86 B3 0D 57 44 C6 2C 8D BC 26 57 " +
            "EC 31 7A 63 EA DF D9 D2 DB 6F 23 22 E1 9D DE B5 " +
            "38 E2 4C 55 7F D9 78 BD C6 AD E1 14 CC 79 24 80 " +
            "92 8F 08 41 F7 5F BE F2 2F 83 7A 62 61 F8 89 E1 " +
            "9D E7 F1 89 19 20 D8 3E 0C 56 DE 10 39 D2 96 E3 " +
            "4D BE 6D D0 32 2B 74 1F 07 A3 15 F8 17 28 8D 3C " +
            "1A 8F EE 3C D8 E4 EF A7 73 02 40 13 D2 6C E3 40 " +
            "81 11 11 11 69 17 91 96 11 F1 C9 6E D2 82 C1 2E " +
            "A8 04 2D 1A 19 21 44 68 F3 6D DA 05 BE 1F CF F3 " +
            "2E 56 CD 4F 2F 5F 4B 28 CE 16 81 11 19 27 9E 20 " +
            "91 4B 9A 8B E7 CE C4 70",
            // After Pi
            "B6 90 41 FB 62 05 25 CA EC 31 7A 63 EA DF D9 D2 " +
            "9D E7 F1 89 19 20 D8 3E 81 11 11 11 69 17 91 96 " +
            "91 4B 9A 8B E7 CE C4 70 E8 05 81 FC CD 70 3A 41 " +
            "C6 AD E1 14 CC 79 24 80 92 8F 08 41 F7 5F BE F2 " +
            "1A 8F EE 3C D8 E4 EF A7 2E 56 CD 4F 2F 5F 4B 28 " +
            "51 77 36 9C 87 57 FA 0D DB 6F 23 22 E1 9D DE B5 " +
            "0C 56 DE 10 39 D2 96 E3 11 F1 C9 6E D2 82 C1 2E " +
            "A8 04 2D 1A 19 21 44 68 E9 2B 08 E6 D0 86 B3 0D " +
            "57 44 C6 2C 8D BC 26 57 2F 83 7A 62 61 F8 89 E1 " +
            "73 02 40 13 D2 6C E3 40 CE 16 81 11 19 27 9E 20 " +
            "FD E6 02 14 E3 38 8D 96 38 E2 4C 55 7F D9 78 BD " +
            "4D BE 6D D0 32 2B 74 1F 07 A3 15 F8 17 28 8D 3C " +
            "F3 6D DA 05 BE 1F CF F3",
            // After Chi
            "A7 56 C0 73 73 25 25 E6 EC 21 7A 73 8A C8 D8 52 " +
            "8D AD 7B 03 9F E8 9C 5E A7 81 50 61 69 16 B0 1C " +
            "D9 6A A0 8B 6F 14 1C 60 F8 07 89 BD FE 76 A0 33 " +
            "CE AD 07 28 C4 D9 65 85 B6 DF 09 02 D0 44 BE FA " +
            "DA 8E EE 8C 18 C4 DF E6 28 FE AD 4F 2F 56 4F A8 " +
            "55 67 EA 8C 9F 15 FA 4F CA CE 22 4C 23 9D 9F B9 " +
            "A4 52 FA 00 30 F3 92 A3 40 82 DB EA 54 D4 7B 2B " +
            "22 0C 2C 38 79 A9 40 D8 C1 A8 30 A4 B0 C6 3A AD " +
            "07 44 C6 3D 1F B8 44 57 A3 97 FB 62 68 FB 95 C1 " +
            "52 2B 48 F5 12 EC C2 4D D8 52 47 19 14 1F 9A 72 " +
            "B8 FA 23 94 E3 1A 89 94 3A E3 5C 7D 7A D9 F1 9D " +
            "BD F2 A7 D5 9A 3C 36 DC 0B 21 15 E8 56 08 8D 38 " +
            "F3 6D 96 44 A2 DE BF DA",
            // After Iota 
            "A5 D6 C0 73 73 25 25 66 EC 21 7A 73 8A C8 D8 52 " +
            "8D AD 7B 03 9F E8 9C 5E A7 81 50 61 69 16 B0 1C " +
            "D9 6A A0 8B 6F 14 1C 60 F8 07 89 BD FE 76 A0 33 " +
            "CE AD 07 28 C4 D9 65 85 B6 DF 09 02 D0 44 BE FA " +
            "DA 8E EE 8C 18 C4 DF E6 28 FE AD 4F 2F 56 4F A8 " +
            "55 67 EA 8C 9F 15 FA 4F CA CE 22 4C 23 9D 9F B9 " +
            "A4 52 FA 00 30 F3 92 A3 40 82 DB EA 54 D4 7B 2B " +
            "22 0C 2C 38 79 A9 40 D8 C1 A8 30 A4 B0 C6 3A AD " +
            "07 44 C6 3D 1F B8 44 57 A3 97 FB 62 68 FB 95 C1 " +
            "52 2B 48 F5 12 EC C2 4D D8 52 47 19 14 1F 9A 72 " +
            "B8 FA 23 94 E3 1A 89 94 3A E3 5C 7D 7A D9 F1 9D " +
            "BD F2 A7 D5 9A 3C 36 DC 0B 21 15 E8 56 08 8D 38 " +
            "F3 6D 96 44 A2 DE BF DA",
            // Round #17
            // After Theta
            "F6 BA BB 7D EC D5 3C 97 9F 4E 62 6C D0 63 33 45 " +
            "91 46 CF 60 55 C1 BC B2 D7 8B 65 94 FB DB CF 76 " +
            "5F 25 F9 74 8C C2 DE 83 AB 6B F2 B3 61 86 B9 C2 " +
            "BD C2 1F 37 9E 72 8E 92 AA 34 BD 61 1A 6D 9E 16 " +
            "AA 84 DB 79 8A 09 A0 8C AE B1 F4 B0 CC 80 8D 4B " +
            "06 0B 91 82 00 E5 E3 BE B9 A1 3A 53 79 36 74 AE " +
            "B8 B9 4E 63 FA DA B2 4F 30 88 EE 1F C6 19 04 41 " +
            "A4 43 75 C7 9A 7F 82 3B 92 C4 4B AA 2F 36 23 5C " +
            "74 2B DE 22 45 13 AF 40 BF 7C 4F 01 A2 D2 B5 2D " +
            "22 21 7D 00 80 21 BD 27 5E 1D 1E E6 F7 C9 58 91 " +
            "EB 96 58 9A 7C EA 90 65 49 8C 44 62 20 72 1A 8A " +
            "A1 19 13 B6 50 15 16 30 7B 2B 20 1D C4 C5 F2 52 " +
            "75 22 CF BB 41 08 7D 39",
            // After Rho
            "F6 BA BB 7D EC D5 3C 97 3E 9D C4 D8 A0 C7 66 8A " +
            "A4 D1 33 58 55 30 AF 6C BF FD 6C 77 BD 58 46 B9 " +
            "14 F6 1E FC 2A C9 A7 63 1B 66 98 2B BC BA 26 3F " +
            "71 E3 29 E7 28 D9 2B FC 85 2A 4D 6F 98 46 9B A7 " +
            "C2 ED 3C C5 04 50 46 55 D8 B8 E4 1A 4B 0F CB 0C " +
            "35 58 88 14 04 28 1F F7 B9 E6 86 EA 4C E5 D9 D0 " +
            "1A D3 D7 96 7D C2 CD 75 33 08 82 60 10 DD 3F 8C " +
            "63 CD 3F C1 1D D2 A1 BA 54 5F 6C 46 B8 24 89 97 " +
            "5B A4 68 E2 15 88 6E C5 DA 96 5F BE A7 00 51 E9 " +
            "A4 F7 44 24 A4 0F 00 30 91 5E 1D 1E E6 F7 C9 58 " +
            "43 96 AD 5B 62 69 F2 A9 26 31 12 89 81 C8 69 28 " +
            "34 63 C2 16 AA C2 02 26 2B 20 1D C4 C5 F2 52 7B " +
            "5F 4E 9D C8 F3 6E 10 42",
            // After Pi
            "F6 BA BB 7D EC D5 3C 97 71 E3 29 E7 28 D9 2B FC " +
            "1A D3 D7 96 7D C2 CD 75 A4 F7 44 24 A4 0F 00 30 " +
            "5F 4E 9D C8 F3 6E 10 42 BF FD 6C 77 BD 58 46 B9 " +
            "D8 B8 E4 1A 4B 0F CB 0C 35 58 88 14 04 28 1F F7 " +
            "5B A4 68 E2 15 88 6E C5 34 63 C2 16 AA C2 02 26 " +
            "3E 9D C4 D8 A0 C7 66 8A 85 2A 4D 6F 98 46 9B A7 " +
            "33 08 82 60 10 DD 3F 8C 91 5E 1D 1E E6 F7 C9 58 " +
            "43 96 AD 5B 62 69 F2 A9 14 F6 1E FC 2A C9 A7 63 " +
            "1B 66 98 2B BC BA 26 3F B9 E6 86 EA 4C E5 D9 D0 " +
            "DA 96 5F BE A7 00 51 E9 2B 20 1D C4 C5 F2 52 7B " +
            "A4 D1 33 58 55 30 AF 6C C2 ED 3C C5 04 50 46 55 " +
            "63 CD 3F C1 1D D2 A1 BA 54 5F 6C 46 B8 24 89 97 " +
            "26 31 12 89 81 C8 69 28",
            // After Chi
            "FC AA 6D 6D B9 D7 F8 96 D5 C7 29 C7 A8 D4 2B FC " +
            "41 DB 4E 5E 2E A2 DD 37 04 47 66 11 A8 9E 2C A5 " +
            "5E 0F 9D 4A F3 66 13 2A 9A BD 64 73 B9 78 52 4A " +
            "92 1C 84 F8 5A 8F AB 0C 11 1B 0A 00 AE 6A 1F D5 " +
            "D0 38 44 83 00 90 2A 5C 74 63 42 1E E8 C5 8B 22 " +
            "0C 9D 46 D8 A0 5E 42 82 05 7C 50 71 7E 64 5B F7 " +
            "71 88 22 21 10 D5 0D 2D AD 57 5D 9E 66 71 CD 5A " +
            "C2 B4 A4 7C 7A 69 6B 8C B4 76 18 3C 6A 8C 7E A3 " +
            "59 76 C1 3F 1F BA 26 16 98 C6 86 AA 0C 17 DB C2 " +
            "CE 40 5D 86 8D 09 F4 E9 20 20 9D C7 51 C0 52 67 " +
            "85 D1 30 58 4C B2 0E C6 D6 FF 7C C3 A4 74 4E 50 " +
            "41 ED 2D 48 1C 1A C1 92 D4 9F 4D 16 EC 14 0F D3 " +
            "64 1D 1E 0C 81 88 29 39",
            // After Iota 
            "7C AA 6D 6D B9 D7 F8 16 D5 C7 29 C7 A8 D4 2B FC " +
            "41 DB 4E 5E 2E A2 DD 37 04 47 66 11 A8 9E 2C A5 " +
            "5E 0F 9D 4A F3 66 13 2A 9A BD 64 73 B9 78 52 4A " +
            "92 1C 84 F8 5A 8F AB 0C 11 1B 0A 00 AE 6A 1F D5 " +
            "D0 38 44 83 00 90 2A 5C 74 63 42 1E E8 C5 8B 22 " +
            "0C 9D 46 D8 A0 5E 42 82 05 7C 50 71 7E 64 5B F7 " +
            "71 88 22 21 10 D5 0D 2D AD 57 5D 9E 66 71 CD 5A " +
            "C2 B4 A4 7C 7A 69 6B 8C B4 76 18 3C 6A 8C 7E A3 " +
            "59 76 C1 3F 1F BA 26 16 98 C6 86 AA 0C 17 DB C2 " +
            "CE 40 5D 86 8D 09 F4 E9 20 20 9D C7 51 C0 52 67 " +
            "85 D1 30 58 4C B2 0E C6 D6 FF 7C C3 A4 74 4E 50 " +
            "41 ED 2D 48 1C 1A C1 92 D4 9F 4D 16 EC 14 0F D3 " +
            "64 1D 1E 0C 81 88 29 39",
            // Round #18
            // After Theta
            "4A 12 15 EA 67 B7 17 4F FF 2D D4 5E 2F 3A 19 78 " +
            "4B 1B D1 D4 46 96 0E 44 A5 EF 5A 4B 4B 8B E8 8F " +
            "8A A3 3C 92 51 9B 12 C4 AC 05 1C F4 67 18 BD 13 " +
            "B8 F6 79 61 DD 61 99 88 1B DB 95 8A C6 5E CC A6 " +
            "71 90 78 D9 E3 85 EE 76 A0 CF E3 C6 4A 38 8A CC " +
            "3A 25 3E 5F 7E 3E AD DB 2F 96 AD E8 F9 8A 69 73 " +
            "7B 48 BD AB 78 E1 DE 5E 0C FF 61 C4 85 64 09 70 " +
            "16 18 05 A4 D8 94 6A 62 82 CE 60 BB B4 EC 91 FA " +
            "73 9C 3C A6 98 54 14 92 92 06 19 20 64 23 08 B1 " +
            "6F E8 61 DC 6E 1C 30 C3 F4 8C 3C 1F F3 3D 53 89 " +
            "B3 69 48 DF 92 D2 E1 9F FC 15 81 5A 23 9A 7C D4 " +
            "4B 2D B2 C2 74 2E 12 E1 75 37 71 4C 0F 01 CB F9 " +
            "B0 B1 BF D4 23 75 28 D7",
            // After Rho
            "4A 12 15 EA 67 B7 17 4F FE 5B A8 BD 5E 74 32 F0 " +
            "D2 46 34 B5 91 A5 03 D1 B4 88 FE 58 FA AE B5 B4 " +
            "DA 94 20 56 1C E5 91 8C 7F 86 D1 3B C1 5A C0 41 " +
            "17 D6 1D 96 89 88 6B 9F E9 C6 76 A5 A2 B1 17 B3 " +
            "48 BC EC F1 42 77 BB 38 A3 C8 0C FA 3C 6E AC 84 " +
            "D6 29 F1 F9 F2 F3 69 DD CD BD 58 B6 A2 E7 2B A6 " +
            "5D C5 0B F7 F6 DA 43 EA C9 12 E0 18 FE C3 88 0B " +
            "52 6C 4A 35 31 0B 8C 02 76 69 D9 23 F5 05 9D C1 " +
            "C7 14 93 8A 42 72 8E 93 84 58 49 83 0C 10 B2 11 " +
            "03 66 F8 0D 3D 8C DB 8D 89 F4 8C 3C 1F F3 3D 53 " +
            "87 7F CE A6 21 7D 4B 4A F3 57 04 6A 8D 68 F2 51 " +
            "A9 45 56 98 CE 45 22 7C 37 71 4C 0F 01 CB F9 75 " +
            "CA 35 6C EC 2F F5 48 1D",
            // After Pi
            "4A 12 15 EA 67 B7 17 4F 17 D6 1D 96 89 88 6B 9F " +
            "5D C5 0B F7 F6 DA 43 EA 03 66 F8 0D 3D 8C DB 8D " +
            "CA 35 6C EC 2F F5 48 1D B4 88 FE 58 FA AE B5 B4 " +
            "A3 C8 0C FA 3C 6E AC 84 D6 29 F1 F9 F2 F3 69 DD " +
            "C7 14 93 8A 42 72 8E 93 A9 45 56 98 CE 45 22 7C " +
            "FE 5B A8 BD 5E 74 32 F0 E9 C6 76 A5 A2 B1 17 B3 " +
            "C9 12 E0 18 FE C3 88 0B 89 F4 8C 3C 1F F3 3D 53 " +
            "87 7F CE A6 21 7D 4B 4A DA 94 20 56 1C E5 91 8C " +
            "7F 86 D1 3B C1 5A C0 41 CD BD 58 B6 A2 E7 2B A6 " +
            "84 58 49 83 0C 10 B2 11 37 71 4C 0F 01 CB F9 75 " +
            "D2 46 34 B5 91 A5 03 D1 48 BC EC F1 42 77 BB 38 " +
            "52 6C 4A 35 31 0B 8C 02 76 69 D9 23 F5 05 9D C1 " +
            "F3 57 04 6A 8D 68 F2 51",
            // After Chi
            "02 13 17 8B 11 E5 17 2F 15 F4 ED 9E 80 8C F3 9A " +
            "95 D4 0F 17 F4 AB 43 FA 03 64 E9 0F 7D 8E CC CF " +
            "DF F1 64 F8 A7 FD 20 8D E0 A9 0F 59 38 3F F4 ED " +
            "A2 DC 0E F8 3C 6E 2A 86 FE 68 B5 E9 7E F6 49 B1 " +
            "D3 9C 3B CA 72 D8 1B 13 AA 05 56 3A CA 05 2A 7C " +
            "FE 4B 28 A5 02 36 BA F8 E9 22 7A 81 A3 81 22 E3 " +
            "CF 19 A2 9A DE CF CA 03 F1 F4 AC 25 41 F3 0D E3 " +
            "86 FB 98 A6 81 FC 4E 49 5A AD 28 D2 3E 40 BA 2A " +
            "7F C6 D0 3A CD 4A 50 50 FE 9C 5C BA A3 2C 62 C2 " +
            "4C DC 69 D3 10 34 B2 99 12 73 9D 26 C0 D1 B9 34 " +
            "C0 06 36 B1 A0 AD 07 D3 6C BD 7D F3 86 73 AA F9 " +
            "D3 7A 4E 7D 39 63 EE 12 76 69 E9 B6 E5 80 9C 41 " +
            "FB EF CC 2A CF 3A 4A 79",
            // After Iota 
            "08 93 17 8B 11 E5 17 2F 15 F4 ED 9E 80 8C F3 9A " +
            "95 D4 0F 17 F4 AB 43 FA 03 64 E9 0F 7D 8E CC CF " +
            "DF F1 64 F8 A7 FD 20 8D E0 A9 0F 59 38 3F F4 ED " +
            "A2 DC 0E F8 3C 6E 2A 86 FE 68 B5 E9 7E F6 49 B1 " +
            "D3 9C 3B CA 72 D8 1B 13 AA 05 56 3A CA 05 2A 7C " +
            "FE 4B 28 A5 02 36 BA F8 E9 22 7A 81 A3 81 22 E3 " +
            "CF 19 A2 9A DE CF CA 03 F1 F4 AC 25 41 F3 0D E3 " +
            "86 FB 98 A6 81 FC 4E 49 5A AD 28 D2 3E 40 BA 2A " +
            "7F C6 D0 3A CD 4A 50 50 FE 9C 5C BA A3 2C 62 C2 " +
            "4C DC 69 D3 10 34 B2 99 12 73 9D 26 C0 D1 B9 34 " +
            "C0 06 36 B1 A0 AD 07 D3 6C BD 7D F3 86 73 AA F9 " +
            "D3 7A 4E 7D 39 63 EE 12 76 69 E9 B6 E5 80 9C 41 " +
            "FB EF CC 2A CF 3A 4A 79",
            // Round #19
            // After Theta
            "88 E2 84 BF 5A BE A2 76 8A A9 D7 CC A8 36 8E 69 " +
            "EF D7 C6 32 D7 D2 AA 63 BF 01 14 7D 75 8C EF BC " +
            "DD FD C7 55 76 EF 1C ED 60 D8 9C 6D 73 64 41 B4 " +
            "3D 81 34 AA 14 D4 57 75 84 6B 7C CC 5D 8F A0 28 " +
            "6F F9 C6 B8 7A DA 38 60 A8 09 F5 97 1B 17 16 1C " +
            "7E 3A BB 91 49 6D 0F A1 76 7F 40 D3 8B 3B 5F 10 " +
            "B5 1A 6B BF FD B6 23 9A 4D 91 51 57 49 F1 2E 90 " +
            "84 F7 3B 0B 50 EE 72 29 DA DC BB E6 75 1B 0F 73 " +
            "E0 9B EA 68 E5 F0 2D A3 84 9F 95 9F 80 55 8B 5B " +
            "F0 B9 94 A1 18 36 91 EA 10 7F 3E 8B 11 C3 85 54 " +
            "40 77 A5 85 EB F6 B2 8A F3 E0 47 A1 AE C9 D7 0A " +
            "A9 79 87 58 1A 1A 07 8B CA 0C 14 C4 ED 82 BF 32 " +
            "F9 E3 6F 87 1E 28 76 19",
            // After Rho
            "88 E2 84 BF 5A BE A2 76 14 53 AF 99 51 6D 1C D3 " +
            "FB B5 B1 CC B5 B4 EA D8 C7 F8 CE FB 1B 40 D1 57 " +
            "7B E7 68 EF EE 3F AE B2 36 47 16 44 0B 86 CD D9 " +
            "A3 4A 41 7D 55 D7 13 48 0A E1 1A 1F 73 D7 23 28 " +
            "7C 63 5C 3D 6D 1C B0 B7 61 C1 81 9A 50 7F B9 71 " +
            "F5 D3 D9 8D 4C 6A 7B 08 41 D8 FD 01 4D 2F EE 7C " +
            "FB ED B7 1D D1 AC D5 58 E2 5D 20 9B 22 A3 AE 92 " +
            "05 28 77 B9 14 C2 FB 9D CD EB 36 1E E6 B4 B9 77 " +
            "1D AD 1C BE 65 14 7C 53 C5 2D C2 CF CA 4F C0 AA " +
            "26 52 1D 3E 97 32 14 C3 54 10 7F 3E 8B 11 C3 85 " +
            "CB 2A 02 DD 95 16 AE DB CC 83 1F 85 BA 26 5F 2B " +
            "35 EF 10 4B 43 E3 60 31 0C 14 C4 ED 82 BF 32 CA " +
            "5D 46 FE F8 DB A1 07 8A",
            // After Pi
            "88 E2 84 BF 5A BE A2 76 A3 4A 41 7D 55 D7 13 48 " +
            "FB ED B7 1D D1 AC D5 58 26 52 1D 3E 97 32 14 C3 " +
            "5D 46 FE F8 DB A1 07 8A C7 F8 CE FB 1B 40 D1 57 " +
            "61 C1 81 9A 50 7F B9 71 F5 D3 D9 8D 4C 6A 7B 08 " +
            "1D AD 1C BE 65 14 7C 53 35 EF 10 4B 43 E3 60 31 " +
            "14 53 AF 99 51 6D 1C D3 0A E1 1A 1F 73 D7 23 28 " +
            "E2 5D 20 9B 22 A3 AE 92 54 10 7F 3E 8B 11 C3 85 " +
            "CB 2A 02 DD 95 16 AE DB 7B E7 68 EF EE 3F AE B2 " +
            "36 47 16 44 0B 86 CD D9 41 D8 FD 01 4D 2F EE 7C " +
            "C5 2D C2 CF CA 4F C0 AA 0C 14 C4 ED 82 BF 32 CA " +
            "FB B5 B1 CC B5 B4 EA D8 7C 63 5C 3D 6D 1C B0 B7 " +
            "05 28 77 B9 14 C2 FB 9D CD EB 36 1E E6 B4 B9 77 " +
            "CC 83 1F 85 BA 26 5F 2B",
            // After Chi
            "D0 47 32 BF DA 96 66 66 A7 58 49 5F 53 C5 13 CB " +
            "A2 E9 55 DD 99 2D D6 50 A6 F2 1D 39 97 2C B4 B7 " +
            "7E 4E BF B8 DE E0 16 82 53 EA 96 FE 17 40 93 5F " +
            "69 ED 85 A8 71 6B BD 22 D5 91 D9 CC 4E 89 7B 28 " +
            "DF BD D2 0E 7D 14 ED 15 15 EE 11 4B 03 DC 48 11 " +
            "F4 4F 8F 19 51 4D 90 41 1E E1 45 3B FA C7 62 2D " +
            "69 77 20 5A 36 A5 82 C8 40 41 D2 3E CB 78 D3 85 " +
            "C1 8A 12 DB B7 84 8D F3 3A 7F 81 EE AA 16 8C 96 " +
            "B2 62 14 8A 89 C6 CD 5B 49 C8 F9 21 4D 9F DC 3C " +
            "B6 CE EA CD A6 4F 4C 9A 08 14 D2 ED 83 3F 73 83 " +
            "FA BD 92 4C A5 76 A1 D0 B4 A0 5C 3B 8F 28 B0 D5 " +
            "05 28 7E 38 0C C0 BD 95 FE DF 96 56 E3 24 19 A7 " +
            "C8 C1 53 B4 F2 2E 4F 0C",
            // After Iota 
            "DA 47 32 3F DA 96 66 E6 A7 58 49 5F 53 C5 13 CB " +
            "A2 E9 55 DD 99 2D D6 50 A6 F2 1D 39 97 2C B4 B7 " +
            "7E 4E BF B8 DE E0 16 82 53 EA 96 FE 17 40 93 5F " +
            "69 ED 85 A8 71 6B BD 22 D5 91 D9 CC 4E 89 7B 28 " +
            "DF BD D2 0E 7D 14 ED 15 15 EE 11 4B 03 DC 48 11 " +
            "F4 4F 8F 19 51 4D 90 41 1E E1 45 3B FA C7 62 2D " +
            "69 77 20 5A 36 A5 82 C8 40 41 D2 3E CB 78 D3 85 " +
            "C1 8A 12 DB B7 84 8D F3 3A 7F 81 EE AA 16 8C 96 " +
            "B2 62 14 8A 89 C6 CD 5B 49 C8 F9 21 4D 9F DC 3C " +
            "B6 CE EA CD A6 4F 4C 9A 08 14 D2 ED 83 3F 73 83 " +
            "FA BD 92 4C A5 76 A1 D0 B4 A0 5C 3B 8F 28 B0 D5 " +
            "05 28 7E 38 0C C0 BD 95 FE DF 96 56 E3 24 19 A7 " +
            "C8 C1 53 B4 F2 2E 4F 0C",
            // Round #20
            // After Theta
            "1C 95 8C B5 7D 30 EA 9C BE A6 26 81 80 83 C7 47 " +
            "96 41 56 84 8E FC D9 2F 21 E3 4D 89 01 20 25 71 " +
            "74 10 AE DE 9C 3C 58 E4 95 38 28 74 B0 E6 1F 25 " +
            "70 13 EA 76 A2 2D 69 AE E1 39 DA 95 59 58 74 57 " +
            "58 AC 82 BE EB 18 7C D3 1F B0 00 2D 41 00 06 77 " +
            "32 9D 31 93 F6 EB 1C 3B 07 1F 2A E5 29 81 B6 A1 " +
            "5D DF 23 03 21 74 8D B7 C7 50 82 8E 5D 74 42 43 " +
            "CB D4 03 BD F5 58 C3 95 FC AD 3F 64 0D B0 00 EC " +
            "AB 9C 7B 54 5A 80 19 D7 7D 60 FA 78 5A 4E D3 43 " +
            "31 DF BA 7D 30 43 DD 5C 02 4A C3 8B C1 E3 3D E5 " +
            "3C 6F 2C C6 02 D0 2D AA AD 5E 33 E5 5C 6E 64 59 " +
            "31 80 7D 61 1B 11 B2 EA 79 CE C6 E6 75 28 88 61 " +
            "C2 9F 42 D2 B0 F2 01 6A",
            // After Rho
            "1C 95 8C B5 7D 30 EA 9C 7C 4D 4D 02 01 07 8F 8F " +
            "65 90 15 A1 23 7F F6 8B 00 52 12 17 32 DE 94 18 " +
            "E4 C1 22 A7 83 70 F5 E6 07 6B FE 51 52 89 83 42 " +
            "6E 27 DA 92 E6 0A 37 A1 55 78 8E 76 65 16 16 DD " +
            "56 41 DF 75 0C BE 69 2C 60 70 F7 01 0B D0 12 04 " +
            "91 E9 8C 99 B4 5F E7 D8 86 1E 7C A8 94 A7 04 DA " +
            "19 08 A1 6B BC ED FA 1E E8 84 86 8E A1 04 1D BB " +
            "DE 7A AC E1 CA 65 EA 81 C8 1A 60 01 D8 F9 5B 7F " +
            "8F 4A 0B 30 E3 7A 95 73 E9 A1 3E 30 7D 3C 2D A7 " +
            "A8 9B 2B E6 5B B7 0F 66 E5 02 4A C3 8B C1 E3 3D " +
            "B7 A8 F2 BC B1 18 0B 40 B5 7A CD 94 73 B9 91 65 " +
            "06 B0 2F 6C 23 42 56 3D CE C6 E6 75 28 88 61 79 " +
            "80 9A F0 A7 90 34 AC 7C",
            // After Pi
            "1C 95 8C B5 7D 30 EA 9C 6E 27 DA 92 E6 0A 37 A1 " +
            "19 08 A1 6B BC ED FA 1E A8 9B 2B E6 5B B7 0F 66 " +
            "80 9A F0 A7 90 34 AC 7C 00 52 12 17 32 DE 94 18 " +
            "60 70 F7 01 0B D0 12 04 91 E9 8C 99 B4 5F E7 D8 " +
            "8F 4A 0B 30 E3 7A 95 73 06 B0 2F 6C 23 42 56 3D " +
            "7C 4D 4D 02 01 07 8F 8F 55 78 8E 76 65 16 16 DD " +
            "E8 84 86 8E A1 04 1D BB E5 02 4A C3 8B C1 E3 3D " +
            "B7 A8 F2 BC B1 18 0B 40 E4 C1 22 A7 83 70 F5 E6 " +
            "07 6B FE 51 52 89 83 42 86 1E 7C A8 94 A7 04 DA " +
            "E9 A1 3E 30 7D 3C 2D A7 CE C6 E6 75 28 88 61 79 " +
            "65 90 15 A1 23 7F F6 8B 56 41 DF 75 0C BE 69 2C " +
            "DE 7A AC E1 CA 65 EA 81 C8 1A 60 01 D8 F9 5B 7F " +
            "B5 7A CD 94 73 B9 91 65",
            // After Chi
            "0D 9D AD DC 65 D5 22 82 CE B4 D0 16 A5 18 32 C1 " +
            "19 08 71 6A 3C ED 5A 06 B4 9E 27 F6 36 B7 4D E6 " +
            "E2 B8 A2 A5 12 3E B9 5D 91 DB 1A 8F 86 D1 71 C0 " +
            "6E 72 F4 21 48 F0 02 27 91 59 A8 D5 B4 5F A5 D4 " +
            "8F 08 1B 23 F3 E6 15 73 66 90 CA 6C 2A 42 54 39 " +
            "D4 C9 4D 8A 81 07 86 AD 50 7A C6 37 6F D7 F4 D9 " +
            "FA 2C 36 B2 91 1C 15 FB AD 47 47 C1 8B C6 67 B2 " +
            "B6 98 70 C8 D5 08 1B 10 64 D5 22 0F 07 56 F1 7E " +
            "6E CA FC 41 3B 91 AA 67 80 58 BC ED 94 27 44 82 " +
            "C9 A0 3E B2 FE 4C B9 21 CD EC 3A 25 78 01 63 79 " +
            "ED AA 35 21 E1 3E 74 0A 56 41 9F 75 1C 26 78 52 " +
            "EB 1A 21 75 E9 65 6A 81 88 9A 70 20 D8 BF 3D F5 " +
            "A7 3B 07 C0 7F 39 98 41",
            // After Iota 
            "8C 1D AD 5C 65 D5 22 02 CE B4 D0 16 A5 18 32 C1 " +
            "19 08 71 6A 3C ED 5A 06 B4 9E 27 F6 36 B7 4D E6 " +
            "E2 B8 A2 A5 12 3E B9 5D 91 DB 1A 8F 86 D1 71 C0 " +
            "6E 72 F4 21 48 F0 02 27 91 59 A8 D5 B4 5F A5 D4 " +
            "8F 08 1B 23 F3 E6 15 73 66 90 CA 6C 2A 42 54 39 " +
            "D4 C9 4D 8A 81 07 86 AD 50 7A C6 37 6F D7 F4 D9 " +
            "FA 2C 36 B2 91 1C 15 FB AD 47 47 C1 8B C6 67 B2 " +
            "B6 98 70 C8 D5 08 1B 10 64 D5 22 0F 07 56 F1 7E " +
            "6E CA FC 41 3B 91 AA 67 80 58 BC ED 94 27 44 82 " +
            "C9 A0 3E B2 FE 4C B9 21 CD EC 3A 25 78 01 63 79 " +
            "ED AA 35 21 E1 3E 74 0A 56 41 9F 75 1C 26 78 52 " +
            "EB 1A 21 75 E9 65 6A 81 88 9A 70 20 D8 BF 3D F5 " +
            "A7 3B 07 C0 7F 39 98 41",
            // Round #21
            // After Theta
            "44 15 8A D1 C5 88 02 5A BC BA D9 4B E8 AB EB 8F " +
            "7E E8 9B 52 48 AD 3A EB 1D 6F 1F AB 87 C2 93 54 " +
            "B5 B3 4D CC 72 8D A2 98 59 D3 3D 02 26 8C 51 98 " +
            "1C 7C FD 7C 05 43 DB 69 F6 B9 42 ED C0 1F C5 39 " +
            "26 F9 23 7E 42 93 CB C1 31 9B 25 05 4A F1 4F FC " +
            "1C C1 6A 07 21 5A A6 F5 22 74 CF 6A 22 64 2D 97 " +
            "9D CC DC 8A E5 5C 75 16 04 B6 7F 9C 3A B3 B9 00 " +
            "E1 93 9F A1 B5 BB 00 D5 AC DD 05 82 A7 0B D1 26 " +
            "1C C4 F5 1C 76 22 73 29 E7 B8 56 D5 E0 67 24 6F " +
            "60 51 06 EF 4F 39 67 93 9A E7 D5 4C 18 B2 78 BC " +
            "25 A2 12 AC 41 63 54 52 24 4F 96 28 51 95 A1 1C " +
            "8C FA CB 4D 9D 25 0A 6C 21 6B 48 7D 69 CA E3 47 " +
            "F0 30 E8 A9 1F 8A 83 84",
            // After Rho
            "44 15 8A D1 C5 88 02 5A 79 75 B3 97 D0 57 D7 1F " +
            "1F FA A6 14 52 AB CE BA 28 3C 49 D5 F1 F6 B1 7A " +
            "6B 14 C5 AC 9D 6D 62 96 60 C2 18 85 99 35 DD 23 " +
            "CF 57 30 B4 9D C6 C1 D7 8E 7D AE 50 3B F0 47 71 " +
            "FC 11 3F A1 C9 E5 60 93 FF C4 1F B3 59 52 A0 14 " +
            "E7 08 56 3B 08 D1 32 AD 5C 8A D0 3D AB 89 90 B5 " +
            "56 2C E7 AA B3 E8 64 E6 66 73 01 08 6C FF 38 75 " +
            "D0 DA 5D 80 EA F0 C9 CF 04 4F 17 A2 4D 58 BB 0B " +
            "9E C3 4E 64 2E 85 83 B8 92 B7 73 5C AB 6A F0 33 " +
            "E7 6C 12 2C CA E0 FD 29 BC 9A E7 D5 4C 18 B2 78 " +
            "51 49 95 88 4A B0 06 8D 90 3C 59 A2 44 55 86 72 " +
            "51 7F B9 A9 B3 44 81 8D 6B 48 7D 69 CA E3 47 21 " +
            "20 21 3C 0C 7A EA 87 E2",
            // After Pi
            "44 15 8A D1 C5 88 02 5A CF 57 30 B4 9D C6 C1 D7 " +
            "56 2C E7 AA B3 E8 64 E6 E7 6C 12 2C CA E0 FD 29 " +
            "20 21 3C 0C 7A EA 87 E2 28 3C 49 D5 F1 F6 B1 7A " +
            "FF C4 1F B3 59 52 A0 14 E7 08 56 3B 08 D1 32 AD " +
            "9E C3 4E 64 2E 85 83 B8 51 7F B9 A9 B3 44 81 8D " +
            "79 75 B3 97 D0 57 D7 1F 8E 7D AE 50 3B F0 47 71 " +
            "66 73 01 08 6C FF 38 75 BC 9A E7 D5 4C 18 B2 78 " +
            "51 49 95 88 4A B0 06 8D 6B 14 C5 AC 9D 6D 62 96 " +
            "60 C2 18 85 99 35 DD 23 5C 8A D0 3D AB 89 90 B5 " +
            "92 B7 73 5C AB 6A F0 33 6B 48 7D 69 CA E3 47 21 " +
            "1F FA A6 14 52 AB CE BA FC 11 3F A1 C9 E5 60 93 " +
            "D0 DA 5D 80 EA F0 C9 CF 04 4F 17 A2 4D 58 BB 0B " +
            "90 3C 59 A2 44 55 86 72",
            // After Chi
            "54 3D 4D DB E7 A0 26 7A 6E 17 20 B0 D5 C6 58 DE " +
            "56 2D CB AA 83 E2 66 24 A3 78 90 FD 4F E0 FD 31 " +
            "AB 63 0C 28 62 AC 46 67 28 34 09 DD F1 77 A3 D3 " +
            "E7 07 17 F7 7F 56 21 04 A6 34 E7 B2 99 91 32 A8 " +
            "B6 C3 0E 30 6E 37 B3 CA 86 BF AF 8B BB 44 81 89 " +
            "19 77 B2 9F 94 58 EF 1B 16 F5 48 85 3B F0 C5 79 " +
            "27 32 11 00 6E 5F 3C F0 94 AE C5 C2 DC 5F 63 6A " +
            "D7 41 99 C8 61 10 06 ED 77 1C 05 94 BF E5 62 02 " +
            "E2 F7 3B C5 99 57 BD 21 35 C2 DC 1C EB 08 97 B5 " +
            "92 A3 F3 D8 BE 66 D0 A5 6B 8A 65 68 CA F3 DA 00 " +
            "1F 30 E6 14 70 BB 47 F6 F8 14 3D 83 CC ED 52 93 " +
            "40 EA 15 80 EA F5 CD BF 0B 8D B1 B6 5F F2 F3 83 " +
            "70 3D 40 03 CD 11 A6 73",
            // After Iota 
            "D4 BD 4D DB E7 A0 26 FA 6E 17 20 B0 D5 C6 58 DE " +
            "56 2D CB AA 83 E2 66 24 A3 78 90 FD 4F E0 FD 31 " +
            "AB 63 0C 28 62 AC 46 67 28 34 09 DD F1 77 A3 D3 " +
            "E7 07 17 F7 7F 56 21 04 A6 34 E7 B2 99 91 32 A8 " +
            "B6 C3 0E 30 6E 37 B3 CA 86 BF AF 8B BB 44 81 89 " +
            "19 77 B2 9F 94 58 EF 1B 16 F5 48 85 3B F0 C5 79 " +
            "27 32 11 00 6E 5F 3C F0 94 AE C5 C2 DC 5F 63 6A " +
            "D7 41 99 C8 61 10 06 ED 77 1C 05 94 BF E5 62 02 " +
            "E2 F7 3B C5 99 57 BD 21 35 C2 DC 1C EB 08 97 B5 " +
            "92 A3 F3 D8 BE 66 D0 A5 6B 8A 65 68 CA F3 DA 00 " +
            "1F 30 E6 14 70 BB 47 F6 F8 14 3D 83 CC ED 52 93 " +
            "40 EA 15 80 EA F5 CD BF 0B 8D B1 B6 5F F2 F3 83 " +
            "70 3D 40 03 CD 11 A6 73",
            // Round #22
            // After Theta
            "3F 9A A0 D3 D1 0F 3C A8 A7 C2 DD A0 73 B5 72 F4 " +
            "E2 5D 80 EC 7F 00 29 5B C3 2E 5A 79 44 04 B5 A6 " +
            "A8 FD 3E 7B E4 12 D7 5C C3 13 E4 D5 C7 D8 B9 81 " +
            "2E D2 EA E7 D9 25 0B 2E 12 44 AC F4 65 73 7D D7 " +
            "D6 95 C4 B4 65 D3 FB 5D 85 21 9D D8 3D FA 10 B2 " +
            "F2 50 5F 97 A2 F7 F5 49 DF 20 B5 95 9D 83 EF 53 " +
            "93 42 5A 46 92 BD 73 8F F4 F8 0F 46 D7 BB 2B FD " +
            "D4 DF AB 9B E7 AE 97 D6 9C 3B E8 9C 89 4A 78 50 " +
            "2B 22 C6 D5 3F 24 97 0B 81 B2 97 5A 17 EA D8 CA " +
            "F2 F5 39 5C B5 82 98 32 68 14 57 3B 4C 4D 4B 3B " +
            "F4 17 0B 1C 46 14 5D A4 31 C1 C0 93 6A 9E 78 B9 " +
            "F4 9A 5E C6 16 17 82 C0 6B DB 7B 32 54 16 BB 14 " +
            "73 A3 72 50 4B AF 37 48",
            // After Rho
            "3F 9A A0 D3 D1 0F 3C A8 4F 85 BB 41 E7 6A E5 E8 " +
            "78 17 20 FB 1F 40 CA 96 44 50 6B 3A EC A2 95 47 " +
            "97 B8 E6 42 ED F7 D9 23 7D 8C 9D 1B 38 3C 41 5E " +
            "7E 9E 5D B2 E0 E2 22 AD B5 04 11 2B 7D D9 5C DF " +
            "4A 62 DA B2 E9 FD 2E EB 0F 21 5B 18 D2 89 DD A3 " +
            "92 87 FA BA 14 BD AF 4F 4F 7D 83 D4 56 76 0E BE " +
            "32 92 EC 9D 7B 9C 14 D2 77 57 FA E9 F1 1F 8C AE " +
            "CD 73 D7 4B 6B EA EF D5 39 13 95 F0 A0 38 77 D0 " +
            "B8 FA 87 E4 72 61 45 C4 6C E5 40 D9 4B AD 0B 75 " +
            "10 53 46 BE 3E 87 AB 56 3B 68 14 57 3B 4C 4D 4B " +
            "74 91 D2 5F 2C 70 18 51 C6 04 03 4F AA 79 E2 E5 " +
            "5E D3 CB D8 E2 42 10 98 DB 7B 32 54 16 BB 14 6B " +
            "0D D2 DC A8 1C D4 D2 EB",
            // After Pi
            "3F 9A A0 D3 D1 0F 3C A8 7E 9E 5D B2 E0 E2 22 AD " +
            "32 92 EC 9D 7B 9C 14 D2 10 53 46 BE 3E 87 AB 56 " +
            "0D D2 DC A8 1C D4 D2 EB 44 50 6B 3A EC A2 95 47 " +
            "0F 21 5B 18 D2 89 DD A3 92 87 FA BA 14 BD AF 4F " +
            "B8 FA 87 E4 72 61 45 C4 5E D3 CB D8 E2 42 10 98 " +
            "4F 85 BB 41 E7 6A E5 E8 B5 04 11 2B 7D D9 5C DF " +
            "77 57 FA E9 F1 1F 8C AE 3B 68 14 57 3B 4C 4D 4B " +
            "74 91 D2 5F 2C 70 18 51 97 B8 E6 42 ED F7 D9 23 " +
            "7D 8C 9D 1B 38 3C 41 5E 4F 7D 83 D4 56 76 0E BE " +
            "6C E5 40 D9 4B AD 0B 75 DB 7B 32 54 16 BB 14 6B " +
            "78 17 20 FB 1F 40 CA 96 4A 62 DA B2 E9 FD 2E EB " +
            "CD 73 D7 4B 6B EA EF D5 39 13 95 F0 A0 38 77 D0 " +
            "C6 04 03 4F AA 79 E2 E5",
            // After Chi
            "3F 9A 00 DE CA 13 28 FA 7E DF 5F 90 E4 E1 89 A9 " +
            "3F 12 74 9D 7B CC 44 7B 22 5B 66 ED FF 8C 87 56 " +
            "4D D6 81 88 3C 34 D0 EE D4 D6 CB 98 E8 96 B7 0B " +
            "27 59 5E 5C B0 C9 9D 23 D4 86 B2 A2 94 BF BF 57 " +
            "B8 FA A7 C6 7E C1 C0 83 55 F2 DB D8 F0 4B 58 38 " +
            "0D D6 51 81 67 6C 65 C8 BD 2C 15 3D 77 99 1D 9E " +
            "33 C6 38 E1 F5 2F 9C BE 30 6C 3D 57 F8 46 A8 E3 " +
            "C4 91 D2 75 34 E1 00 46 95 C9 E4 86 AB B5 D7 83 " +
            "5D 0C DD 12 31 B5 40 1F DC 67 B1 D0 42 64 1A B4 " +
            "68 65 84 DB A2 E9 C2 75 B3 7F 2B 4D 06 B3 14 37 " +
            "FD 06 25 B2 1D 42 0B 82 7A 62 DA 02 69 ED 3E EB " +
            "0B 77 D5 44 61 AB 6F F0 01 00 B5 40 B5 38 7F C2 " +
            "C4 64 D9 4F 4A C4 C6 8C",
            // After Iota 
            "3E 9A 00 5E CA 13 28 FA 7E DF 5F 90 E4 E1 89 A9 " +
            "3F 12 74 9D 7B CC 44 7B 22 5B 66 ED FF 8C 87 56 " +
            "4D D6 81 88 3C 34 D0 EE D4 D6 CB 98 E8 96 B7 0B " +
            "27 59 5E 5C B0 C9 9D 23 D4 86 B2 A2 94 BF BF 57 " +
            "B8 FA A7 C6 7E C1 C0 83 55 F2 DB D8 F0 4B 58 38 " +
            "0D D6 51 81 67 6C 65 C8 BD 2C 15 3D 77 99 1D 9E " +
            "33 C6 38 E1 F5 2F 9C BE 30 6C 3D 57 F8 46 A8 E3 " +
            "C4 91 D2 75 34 E1 00 46 95 C9 E4 86 AB B5 D7 83 " +
            "5D 0C DD 12 31 B5 40 1F DC 67 B1 D0 42 64 1A B4 " +
            "68 65 84 DB A2 E9 C2 75 B3 7F 2B 4D 06 B3 14 37 " +
            "FD 06 25 B2 1D 42 0B 82 7A 62 DA 02 69 ED 3E EB " +
            "0B 77 D5 44 61 AB 6F F0 01 00 B5 40 B5 38 7F C2 " +
            "C4 64 D9 4F 4A C4 C6 8C",
            // Round #23
            // After Theta
            "12 BD 5D BB 89 28 9D 11 EE 0E 30 76 65 D9 8A 3D " +
            "7B 87 FC B3 DD 91 96 99 7B 44 09 E9 AE CC 20 D6 " +
            "90 D5 FA 89 B4 D3 CE 1F F8 F1 96 7D AB AD 02 E0 " +
            "B7 88 31 BA 31 F1 9E B7 90 13 3A 8C 32 E2 6D B5 " +
            "E1 E5 C8 C2 2F 81 67 03 88 F1 A0 D9 78 AC 46 C9 " +
            "21 F1 0C 64 24 57 D0 23 2D FD 7A DB F6 A1 1E 0A " +
            "77 53 B0 CF 53 72 4E 5C 69 73 52 53 A9 06 0F 63 " +
            "19 92 A9 74 BC 06 1E B7 B9 EE B9 63 E8 8E 62 68 " +
            "CD DD B2 F4 B0 8D 43 8B 98 F2 39 FE E4 39 C8 56 " +
            "31 7A EB DF F3 A9 65 F5 6E 7C 50 4C 8E 54 0A C6 " +
            "D1 21 78 57 5E 79 BE 69 EA B3 B5 E4 E8 D5 3D 7F " +
            "4F E2 5D 6A C7 F6 BD 12 58 1F DA 44 E4 78 D8 42 " +
            "19 67 A2 4E C2 23 D8 7D",
            // After Rho
            "12 BD 5D BB 89 28 9D 11 DC 1D 60 EC CA B2 15 7B " +
            "DE 21 FF 6C 77 A4 65 E6 CA 0C 62 BD 47 94 90 EE " +
            "9D 76 FE 80 AC D6 4F A4 B7 DA 2A 00 8E 1F 6F D9 " +
            "A3 1B 13 EF 79 7B 8B 18 2D E4 84 0E A3 8C 78 5B " +
            "72 64 E1 97 C0 B3 81 F0 6A 94 8C 18 0F 9A 8D C7 " +
            "09 89 67 20 23 B9 82 1E 28 B4 F4 EB 6D DB 87 7A " +
            "7D 9E 92 73 E2 BA 9B 82 0D 1E C6 D2 E6 A4 A6 52 " +
            "3A 5E 03 8F DB 0C C9 54 C7 D0 1D C5 D0 72 DD 73 " +
            "96 1E B6 71 68 B1 B9 5B 64 2B 4C F9 1C 7F F2 1C " +
            "B5 AC 3E 46 6F FD 7B 3E C6 6E 7C 50 4C 8E 54 0A " +
            "F9 A6 45 87 E0 5D 79 E5 A9 CF D6 92 A3 57 F7 FC " +
            "49 BC 4B ED D8 BE 57 E2 1F DA 44 E4 78 D8 42 58 " +
            "76 5F C6 99 A8 93 F0 08",
            // After Pi
            "12 BD 5D BB 89 28 9D 11 A3 1B 13 EF 79 7B 8B 18 " +
            "7D 9E 92 73 E2 BA 9B 82 B5 AC 3E 46 6F FD 7B 3E " +
            "76 5F C6 99 A8 93 F0 08 CA 0C 62 BD 47 94 90 EE " +
            "6A 94 8C 18 0F 9A 8D C7 09 89 67 20 23 B9 82 1E " +
            "96 1E B6 71 68 B1 B9 5B 49 BC 4B ED D8 BE 57 E2 " +
            "DC 1D 60 EC CA B2 15 7B 2D E4 84 0E A3 8C 78 5B " +
            "0D 1E C6 D2 E6 A4 A6 52 C6 6E 7C 50 4C 8E 54 0A " +
            "F9 A6 45 87 E0 5D 79 E5 9D 76 FE 80 AC D6 4F A4 " +
            "B7 DA 2A 00 8E 1F 6F D9 28 B4 F4 EB 6D DB 87 7A " +
            "64 2B 4C F9 1C 7F F2 1C 1F DA 44 E4 78 D8 42 58 " +
            "DE 21 FF 6C 77 A4 65 E6 72 64 E1 97 C0 B3 81 F0 " +
            "3A 5E 03 8F DB 0C C9 54 C7 D0 1D C5 D0 72 DD 73 " +
            "A9 CF D6 92 A3 57 F7 FC",
            // After Chi
            "4E 39 DD AB 0B A8 8D 93 23 3B 3F EB 74 3E EB 24 " +
            "3F CD 52 EA 62 B8 1B 82 B5 0C 27 64 6E D5 76 2F " +
            "D7 5D C4 DD D8 C0 F2 00 CB 05 01 9D 67 B5 92 F6 " +
            "FC 82 1C 49 47 9A B4 86 40 29 2E AC B3 B7 C4 BE " +
            "14 1E 96 61 6F B1 39 57 69 2C C7 ED D0 B4 5A E3 " +
            "DC 07 22 3C 8E 92 93 7B EF 84 BC 0E AB 86 28 53 " +
            "34 9E C7 55 46 F5 8F B7 C2 77 5C 38 46 2C 50 10 " +
            "D8 46 C1 85 C1 51 11 E5 95 52 2A 6B CD 16 CF 86 " +
            "F3 D1 22 10 9E 3B 1F DD 33 64 F4 EF 0D 5B 87 3A " +
            "E4 0F F6 F9 98 79 FF B8 3D 52 44 E4 7A D1 62 01 " +
            "D6 3B FD 64 6C A8 2D E2 B7 E4 FD D7 C0 C1 95 D3 " +
            "12 51 C1 9D F8 09 EB D8 91 F0 34 A9 84 D2 DD 71 " +
            "89 8B D6 01 23 44 77 EC",
            // After Iota 
            "46 B9 DD 2B 0B A8 8D 13 23 3B 3F EB 74 3E EB 24 " +
            "3F CD 52 EA 62 B8 1B 82 B5 0C 27 64 6E D5 76 2F " +
            "D7 5D C4 DD D8 C0 F2 00 CB 05 01 9D 67 B5 92 F6 " +
            "FC 82 1C 49 47 9A B4 86 40 29 2E AC B3 B7 C4 BE " +
            "14 1E 96 61 6F B1 39 57 69 2C C7 ED D0 B4 5A E3 " +
            "DC 07 22 3C 8E 92 93 7B EF 84 BC 0E AB 86 28 53 " +
            "34 9E C7 55 46 F5 8F B7 C2 77 5C 38 46 2C 50 10 " +
            "D8 46 C1 85 C1 51 11 E5 95 52 2A 6B CD 16 CF 86 " +
            "F3 D1 22 10 9E 3B 1F DD 33 64 F4 EF 0D 5B 87 3A " +
            "E4 0F F6 F9 98 79 FF B8 3D 52 44 E4 7A D1 62 01 " +
            "D6 3B FD 64 6C A8 2D E2 B7 E4 FD D7 C0 C1 95 D3 " +
            "12 51 C1 9D F8 09 EB D8 91 F0 34 A9 84 D2 DD 71 " +
            "89 8B D6 01 23 44 77 EC",
            // Xor'd state (in bytes)
            "46 B9 DD 2B 0B A8 8D 13 23 3B 3F EB 74 3E EB 24 " +
            "3F CD 52 EA 62 B8 1B 82 B5 0C 27 64 6E D5 76 2F " +
            "D7 5D C4 DD D8 C0 F2 00 CB 05 01 9D 67 B5 92 F6 " +
            "FC 82 1C 49 47 9A B4 86 40 29 2E AC B3 B7 C4 BE " +
            "14 1E 96 61 6F B1 39 57 69 2C C7 ED D0 B4 5A E3 " +
            "DC 07 22 3C 8E 92 93 7B EF 84 BC 0E AB 86 28 53 " +
            "34 9E C7 55 46 F5 8F B7 C2 77 5C 38 46 2C 50 10 " +
            "D8 46 C1 85 C1 51 11 E5 95 52 2A 6B CD 16 CF 86 " +
            "F3 D1 22 10 9E 3B 1F DD 33 64 F4 EF 0D 5B 87 3A " +
            "E4 0F F6 F9 98 79 FF B8 3D 52 44 E4 7A D1 62 01 " +
            "D6 3B FD 64 6C A8 2D E2 B7 E4 FD D7 C0 C1 95 D3 " +
            "12 51 C1 9D F8 09 EB D8 91 F0 34 A9 84 D2 DD 71 " +
            "89 8B D6 01 23 44 77 EC",
            // Round #0
            // After Theta
            "7D 47 0D AD 17 A9 DA 07 E5 77 0A AD F3 5F FC 0C " +
            "66 F1 4D 5B 12 A6 9D 3F 7A 9E 08 A5 2C 1C 13 91 " +
            "E4 63 B8 BA 04 41 13 55 F0 FB D1 1B 7B B4 C5 E2 " +
            "3A CE 29 0F C0 FB A3 AE 19 15 31 1D C3 A9 42 03 " +
            "DB 8C B9 A0 2D 78 5C E9 5A 12 BB 8A 0C 35 BB B6 " +
            "E7 F9 F2 BA 92 93 C4 6F 29 C8 89 48 2C E7 3F 7B " +
            "6D A2 D8 E4 36 EB 09 0A 0D E5 73 F9 04 E5 35 AE " +
            "EB 78 BD E2 1D D0 F0 B0 AE AC FA ED D1 17 98 92 " +
            "35 9D 17 56 19 5A 08 F5 6A 58 EB 5E 7D 45 01 87 " +
            "2B 9D D9 38 DA B0 9A 06 0E 6C 38 83 A6 50 83 54 " +
            "ED C5 2D E2 70 A9 7A F6 71 A8 C8 91 47 A0 82 FB " +
            "4B 6D DE 2C 88 17 6D 65 5E 62 1B 68 C6 1B B8 CF " +
            "BA B5 AA 66 FF C5 96 B9",
            // After Rho
            "7D 47 0D AD 17 A9 DA 07 CA EF 14 5A E7 BF F8 19 " +
            "59 7C D3 96 84 69 E7 8F C2 31 11 A9 E7 89 50 CA " +
            "08 9A A8 22 1F C3 D5 25 B1 47 5B 2C 0E BF 1F BD " +
            "F2 00 BC 3F EA AA E3 9C 40 46 45 4C C7 70 AA D0 " +
            "C6 5C D0 16 3C AE F4 6D B3 6B AB 25 B1 AB C8 50 " +
            "3B CF 97 D7 95 9C 24 7E EC A5 20 27 22 B1 9C FF " +
            "26 B7 59 4F 50 68 13 C5 CA 6B 5C 1B CA E7 F2 09 " +
            "F1 0E 68 78 D8 75 BC 5E DB A3 2F 30 25 5D 59 F5 " +
            "C2 2A 43 0B A1 BE A6 F3 80 43 35 AC 75 AF BE A2 " +
            "56 D3 60 A5 33 1B 47 1B 54 0E 6C 38 83 A6 50 83 " +
            "EA D9 B7 17 B7 88 C3 A5 C7 A1 22 47 1E 81 0A EE " +
            "A9 CD 9B 05 F1 A2 AD 6C 62 1B 68 C6 1B B8 CF 5E " +
            "65 AE 6E AD AA D9 7F B1",
            // After Pi
            "7D 47 0D AD 17 A9 DA 07 F2 00 BC 3F EA AA E3 9C " +
            "26 B7 59 4F 50 68 13 C5 56 D3 60 A5 33 1B 47 1B " +
            "65 AE 6E AD AA D9 7F B1 C2 31 11 A9 E7 89 50 CA " +
            "B3 6B AB 25 B1 AB C8 50 3B CF 97 D7 95 9C 24 7E " +
            "C2 2A 43 0B A1 BE A6 F3 A9 CD 9B 05 F1 A2 AD 6C " +
            "CA EF 14 5A E7 BF F8 19 40 46 45 4C C7 70 AA D0 " +
            "CA 6B 5C 1B CA E7 F2 09 54 0E 6C 38 83 A6 50 83 " +
            "EA D9 B7 17 B7 88 C3 A5 08 9A A8 22 1F C3 D5 25 " +
            "B1 47 5B 2C 0E BF 1F BD EC A5 20 27 22 B1 9C FF " +
            "80 43 35 AC 75 AF BE A2 62 1B 68 C6 1B B8 CF 5E " +
            "59 7C D3 96 84 69 E7 8F C6 5C D0 16 3C AE F4 6D " +
            "F1 0E 68 78 D8 75 BC 5E DB A3 2F 30 25 5D 59 F5 " +
            "C7 A1 22 47 1E 81 0A EE",
            // After Chi
            "79 F0 4C ED 07 E9 CA 46 A2 40 9C 9F C9 B9 A7 86 " +
            "07 9B 57 47 D8 A8 2B 65 4E 92 61 A5 26 3B C7 1D " +
            "E7 AE DE BF 42 DB 5E 29 CA B5 05 7B E3 9D 74 E4 " +
            "73 4B EB 2D 91 89 4A D1 12 0A 0F D3 C5 9C 2D 72 " +
            "80 1A 43 A3 A7 B7 F6 71 98 87 31 01 E1 80 25 7C " +
            "40 C6 0C 49 EF 38 A8 10 54 42 65 6C C6 70 AA 52 " +
            "60 BA CF 1C FE EF 71 2D 54 28 6C 70 C3 91 68 9B " +
            "EA D9 F6 13 B7 C8 C1 65 44 3A 88 21 3F C3 55 67 " +
            "B1 05 4E A4 5B B1 3D BD 8E BD 68 65 28 A1 DD A3 " +
            "88 C3 B5 8C 71 EC AE 83 D3 5E 3B CA 1B 84 C5 C6 " +
            "68 7E FB FE 44 38 EF 9D CC FD D7 16 19 A6 B5 CC " +
            "F5 0E 68 3F C2 F5 BE 54 C3 FF FE A0 A5 35 BC F4 " +
            "41 A1 22 47 26 07 1A 8E",
            // After Iota 
            "78 F0 4C ED 07 E9 CA 46 A2 40 9C 9F C9 B9 A7 86 " +
            "07 9B 57 47 D8 A8 2B 65 4E 92 61 A5 26 3B C7 1D " +
            "E7 AE DE BF 42 DB 5E 29 CA B5 05 7B E3 9D 74 E4 " +
            "73 4B EB 2D 91 89 4A D1 12 0A 0F D3 C5 9C 2D 72 " +
            "80 1A 43 A3 A7 B7 F6 71 98 87 31 01 E1 80 25 7C " +
            "40 C6 0C 49 EF 38 A8 10 54 42 65 6C C6 70 AA 52 " +
            "60 BA CF 1C FE EF 71 2D 54 28 6C 70 C3 91 68 9B " +
            "EA D9 F6 13 B7 C8 C1 65 44 3A 88 21 3F C3 55 67 " +
            "B1 05 4E A4 5B B1 3D BD 8E BD 68 65 28 A1 DD A3 " +
            "88 C3 B5 8C 71 EC AE 83 D3 5E 3B CA 1B 84 C5 C6 " +
            "68 7E FB FE 44 38 EF 9D CC FD D7 16 19 A6 B5 CC " +
            "F5 0E 68 3F C2 F5 BE 54 C3 FF FE A0 A5 35 BC F4 " +
            "41 A1 22 47 26 07 1A 8E",
            // Round #1
            // After Theta
            "8F 9C 5B 14 96 56 31 D7 61 B7 85 3A AA 10 22 54 " +
            "5C 13 D7 9F 28 76 73 11 4E 14 F6 37 7D 94 19 20 " +
            "8A BD B6 E5 34 71 4C 38 3D D9 12 82 72 22 8F 75 " +
            "B0 BC F2 88 F2 20 CF 03 49 82 8F 0B 35 42 75 06 " +
            "80 9C D4 31 FC 18 28 4C F5 94 59 5B 97 2A 37 6D " +
            "B7 AA 1B B0 7E 87 53 81 97 B5 7C C9 A5 D9 2F 80 " +
            "3B 32 4F C4 0E 31 29 59 54 AE FB E2 98 3E B6 A6 " +
            "87 CA 9E 49 C1 62 D3 74 B3 56 9F D8 AE 7C AE F6 " +
            "72 F2 57 01 38 18 B8 6F D5 35 E8 BD D8 7F 85 D7 " +
            "88 45 22 1E 2A 43 70 BE BE 4D 53 90 6D 2E D7 D7 " +
            "9F 12 EC 07 D5 87 14 0C 0F 0A CE B3 7A 0F 30 1E " +
            "AE 86 E8 E7 32 2B E6 20 C3 79 69 32 FE 9A 62 C9 " +
            "2C B2 4A 1D 50 AD 08 9F",
            // After Rho
            "8F 9C 5B 14 96 56 31 D7 C2 6E 0B 75 54 21 44 A8 " +
            "D7 C4 F5 27 8A DD 5C 04 47 99 01 E2 44 61 7F D3 " +
            "89 63 C2 51 EC B5 2D A7 28 27 F2 58 D7 93 2D 21 " +
            "8F 28 0F F2 3C 00 CB 2B 41 92 E0 E3 42 8D 50 9D " +
            "4E EA 18 7E 0C 14 26 40 72 D3 56 4F 99 B5 75 A9 " +
            "BC 55 DD 80 F5 3B 9C 0A 00 5E D6 F2 25 97 66 BF " +
            "22 76 88 49 C9 DA 91 79 7D 6C 4D A9 5C F7 C5 31 " +
            "A4 60 B1 69 BA 43 65 CF B1 5D F9 5C ED 67 AD 3E " +
            "2A 00 07 03 F7 4D 4E FE C2 EB EA 1A F4 5E EC BF " +
            "08 CE 17 B1 48 C4 43 65 D7 BE 4D 53 90 6D 2E D7 " +
            "52 30 7C 4A B0 1F 54 1F 3C 28 38 CF EA 3D C0 78 " +
            "D5 10 FD 5C 66 C5 1C C4 79 69 32 FE 9A 62 C9 C3 " +
            "C2 27 8B AC 52 07 54 2B",
            // After Pi
            "8F 9C 5B 14 96 56 31 D7 8F 28 0F F2 3C 00 CB 2B " +
            "22 76 88 49 C9 DA 91 79 08 CE 17 B1 48 C4 43 65 " +
            "C2 27 8B AC 52 07 54 2B 47 99 01 E2 44 61 7F D3 " +
            "72 D3 56 4F 99 B5 75 A9 BC 55 DD 80 F5 3B 9C 0A " +
            "2A 00 07 03 F7 4D 4E FE D5 10 FD 5C 66 C5 1C C4 " +
            "C2 6E 0B 75 54 21 44 A8 41 92 E0 E3 42 8D 50 9D " +
            "7D 6C 4D A9 5C F7 C5 31 D7 BE 4D 53 90 6D 2E D7 " +
            "52 30 7C 4A B0 1F 54 1F 89 63 C2 51 EC B5 2D A7 " +
            "28 27 F2 58 D7 93 2D 21 00 5E D6 F2 25 97 66 BF " +
            "C2 EB EA 1A F4 5E EC BF 79 69 32 FE 9A 62 C9 C3 " +
            "D7 C4 F5 27 8A DD 5C 04 4E EA 18 7E 0C 14 26 40 " +
            "A4 60 B1 69 BA 43 65 CF B1 5D F9 5C ED 67 AD 3E " +
            "3C 28 38 CF EA 3D C0 78",
            // After Chi
            "AF CA DB 1D 57 8C 21 87 87 A0 18 42 3C 04 89 2F " +
            "E0 57 00 45 DB D9 85 73 05 56 47 A1 CC 94 62 B1 " +
            "C2 07 8F 4E 7A 07 9E 03 CB 9D 88 62 20 6B F7 D1 " +
            "70 D3 54 4C 9B F1 37 5D 69 45 25 DC F5 BB 8C 0A " +
            "28 89 07 A1 F7 6D 2D ED E5 52 AB 51 FF 51 1C EC " +
            "FE 02 06 7D 48 53 C1 88 C3 00 E0 B1 C2 85 7A 5B " +
            "7D 6C 7D A1 7C E5 95 39 57 F0 4E 66 D4 4D 2E 77 " +
            "53 A0 9C C8 B2 93 44 0A 89 3B C6 F3 CC B1 6F 39 " +
            "EA 86 DA 50 07 DB A5 21 39 5E C6 16 2F B7 67 FF " +
            "42 E9 2A 1B 90 CB C8 9B 59 6D 02 F6 89 60 C9 C3 " +
            "77 C4 54 26 38 9E 1D 8B 5F F7 50 6A 49 30 AE 70 " +
            "A8 40 B1 EA B8 5B 25 8F 72 99 3C 7C ED A7 B1 3A " +
            "34 02 30 97 EE 3D E2 38",
            // After Iota 
            "2D 4A DB 1D 57 8C 21 87 87 A0 18 42 3C 04 89 2F " +
            "E0 57 00 45 DB D9 85 73 05 56 47 A1 CC 94 62 B1 " +
            "C2 07 8F 4E 7A 07 9E 03 CB 9D 88 62 20 6B F7 D1 " +
            "70 D3 54 4C 9B F1 37 5D 69 45 25 DC F5 BB 8C 0A " +
            "28 89 07 A1 F7 6D 2D ED E5 52 AB 51 FF 51 1C EC " +
            "FE 02 06 7D 48 53 C1 88 C3 00 E0 B1 C2 85 7A 5B " +
            "7D 6C 7D A1 7C E5 95 39 57 F0 4E 66 D4 4D 2E 77 " +
            "53 A0 9C C8 B2 93 44 0A 89 3B C6 F3 CC B1 6F 39 " +
            "EA 86 DA 50 07 DB A5 21 39 5E C6 16 2F B7 67 FF " +
            "42 E9 2A 1B 90 CB C8 9B 59 6D 02 F6 89 60 C9 C3 " +
            "77 C4 54 26 38 9E 1D 8B 5F F7 50 6A 49 30 AE 70 " +
            "A8 40 B1 EA B8 5B 25 8F 72 99 3C 7C ED A7 B1 3A " +
            "34 02 30 97 EE 3D E2 38",
            // Round #2
            // After Theta
            "36 D5 1D A1 50 22 53 68 AB 4A 81 1D 7C 48 50 22 " +
            "F4 EB 16 C2 D4 F3 7B 1F 52 02 7D 08 A8 CF 67 BC " +
            "44 0D 19 E0 7F E8 4D 51 D0 02 4E DE 27 C5 85 3E " +
            "5C 39 CD 13 DB BD EE 50 7D F9 33 5B FA 91 72 66 " +
            "7F DD 3D 08 93 36 28 E0 63 58 3D FF FA BE CF BE " +
            "E5 9D C0 C1 4F FD B3 67 EF EA 79 EE 82 C9 A3 56 " +
            "69 D0 6B 26 73 CF 6B 55 00 A4 74 CF B0 16 2B 7A " +
            "D5 AA 0A 66 B7 7C 97 58 92 A4 00 4F CB 1F 1D D6 " +
            "C6 6C 43 0F 47 97 7C 2C 2D E2 D0 91 20 9D 99 93 " +
            "15 BD 10 B2 F4 90 CD 96 DF 67 94 58 8C 8F 1A 91 " +
            "6C 5B 92 9A 3F 30 6F 64 73 1D C9 35 09 7C 77 7D " +
            "BC FC A7 6D B7 71 DB E3 25 CD 06 D5 89 FC B4 37 " +
            "B2 08 A6 39 EB D2 31 6A",
            // After Rho
            "36 D5 1D A1 50 22 53 68 56 95 02 3B F8 90 A0 44 " +
            "FD BA 85 30 F5 FC DE 07 FA 7C C6 2B 25 D0 87 80 " +
            "43 6F 8A 22 6A C8 00 FF 7D 52 5C E8 03 2D E0 E4 " +
            "3C B1 DD EB 0E C5 95 D3 59 5F FE CC 96 7E A4 9C " +
            "EE 1E 84 49 1B 14 F0 BF FB EC 3B 86 D5 F3 AF EF " +
            "2B EF 04 0E 7E EA 9F 3D 5A BD AB E7 B9 0B 26 8F " +
            "33 99 7B 5E AB 4A 83 5E 2D 56 F4 00 48 E9 9E 61 " +
            "B3 5B BE 4B AC 6A 55 05 9E 96 3F 3A AC 25 49 01 " +
            "E8 E1 E8 92 8F C5 98 6D CC C9 16 71 E8 48 90 CE " +
            "B2 D9 B2 A2 17 42 96 1E 91 DF 67 94 58 8C 8F 1A " +
            "BC 91 B1 6D 49 6A FE C0 CD 75 24 D7 24 F0 DD F5 " +
            "97 FF B4 ED 36 6E 7B 9C CD 06 D5 89 FC B4 37 25 " +
            "8C 9A 2C 82 69 CE BA 74",
            // After Pi
            "36 D5 1D A1 50 22 53 68 3C B1 DD EB 0E C5 95 D3 " +
            "33 99 7B 5E AB 4A 83 5E B2 D9 B2 A2 17 42 96 1E " +
            "8C 9A 2C 82 69 CE BA 74 FA 7C C6 2B 25 D0 87 80 " +
            "FB EC 3B 86 D5 F3 AF EF 2B EF 04 0E 7E EA 9F 3D " +
            "E8 E1 E8 92 8F C5 98 6D 97 FF B4 ED 36 6E 7B 9C " +
            "56 95 02 3B F8 90 A0 44 59 5F FE CC 96 7E A4 9C " +
            "2D 56 F4 00 48 E9 9E 61 91 DF 67 94 58 8C 8F 1A " +
            "BC 91 B1 6D 49 6A FE C0 43 6F 8A 22 6A C8 00 FF " +
            "7D 52 5C E8 03 2D E0 E4 5A BD AB E7 B9 0B 26 8F " +
            "CC C9 16 71 E8 48 90 CE CD 06 D5 89 FC B4 37 25 " +
            "FD BA 85 30 F5 FC DE 07 EE 1E 84 49 1B 14 F0 BF " +
            "B3 5B BE 4B AC 6A 55 05 9E 96 3F 3A AC 25 49 01 " +
            "CD 75 24 D7 24 F0 DD F5",
            // After Chi
            "35 DD 3F B5 F1 28 51 64 BC F1 5D 4B 1A C5 81 D3 " +
            "3F 9B 77 5E C3 C6 AB 3E 80 9C A3 83 07 62 D7 16 " +
            "84 BA EC C8 67 0B 3E E7 FA 7F C2 23 0F D8 97 90 " +
            "3B EC D3 16 54 F6 AF AF 3C F1 10 63 4E C0 FC AD " +
            "80 E1 AA 90 8E 55 1C 6D 96 7F 8D 69 E6 4D 53 F3 " +
            "72 95 02 3B B0 11 BA 25 C9 D6 FD 58 86 7A A5 86 " +
            "01 56 64 69 49 8B EE A1 D3 DB 65 86 E8 1C 8F 1E " +
            "B5 DB 4D A9 4F 04 FA 58 41 C2 29 25 D2 CA 06 F4 " +
            "F9 12 48 F8 43 6D 70 A4 5B BB 6A 6F AD BF 01 AE " +
            "CE A0 1C 53 EA 00 90 14 F1 16 81 41 FD 91 D7 25 " +
            "EC FB BF 32 51 96 DB 07 E2 9A 85 79 1B 11 F8 BF " +
            "F2 3A BE 8E AC BA C1 F1 AE 1C BE 1A 7D 29 4B 03 " +
            "CF 71 24 9E 2E F0 FD 4D",
            // After Iota 
            "BF 5D 3F B5 F1 28 51 E4 BC F1 5D 4B 1A C5 81 D3 " +
            "3F 9B 77 5E C3 C6 AB 3E 80 9C A3 83 07 62 D7 16 " +
            "84 BA EC C8 67 0B 3E E7 FA 7F C2 23 0F D8 97 90 " +
            "3B EC D3 16 54 F6 AF AF 3C F1 10 63 4E C0 FC AD " +
            "80 E1 AA 90 8E 55 1C 6D 96 7F 8D 69 E6 4D 53 F3 " +
            "72 95 02 3B B0 11 BA 25 C9 D6 FD 58 86 7A A5 86 " +
            "01 56 64 69 49 8B EE A1 D3 DB 65 86 E8 1C 8F 1E " +
            "B5 DB 4D A9 4F 04 FA 58 41 C2 29 25 D2 CA 06 F4 " +
            "F9 12 48 F8 43 6D 70 A4 5B BB 6A 6F AD BF 01 AE " +
            "CE A0 1C 53 EA 00 90 14 F1 16 81 41 FD 91 D7 25 " +
            "EC FB BF 32 51 96 DB 07 E2 9A 85 79 1B 11 F8 BF " +
            "F2 3A BE 8E AC BA C1 F1 AE 1C BE 1A 7D 29 4B 03 " +
            "CF 71 24 9E 2E F0 FD 4D",
            // Round #3
            // After Theta
            "8D A2 CA 6B CD 60 EA 02 70 04 9B 9A 5C 69 D3 AB " +
            "0C ED 55 63 BE F6 96 3A 19 D2 66 99 F9 AC D4 32 " +
            "02 BD F1 60 0A 72 E2 D0 C8 80 37 FD 33 90 2C 76 " +
            "F7 19 15 C7 12 5A FD D7 0F 87 32 5E 33 F0 C1 A9 " +
            "19 AF 6F 8A 70 9B 1F 49 10 78 90 C1 8B 34 8F C4 " +
            "40 6A F7 E5 8C 59 01 C3 05 23 3B 89 C0 D6 F7 FE " +
            "32 20 46 54 34 BB D3 A5 4A 95 A0 9C 16 D2 8C 3A " +
            "33 DC 50 01 22 7D 26 6F 73 3D DC FB EE 82 BD 12 " +
            "35 E7 8E 29 05 C1 22 DC 68 CD 48 52 D0 8F 3C AA " +
            "57 EE D9 49 14 CE 93 30 77 11 9C E9 90 E8 0B 12 " +
            "DE 04 4A EC 6D DE 60 E1 2E 6F 43 A8 5D BD AA C7 " +
            "C1 4C 9C B3 D1 8A FC F5 37 52 7B 00 83 E7 48 27 " +
            "49 76 39 36 43 89 21 7A",
            // After Rho
            "8D A2 CA 6B CD 60 EA 02 E1 08 36 35 B9 D2 A6 57 " +
            "43 7B D5 98 AF BD A5 0E CF 4A 2D 93 21 6D 96 99 " +
            "90 13 87 16 E8 8D 07 53 3F 03 C9 62 87 0C 78 D3 " +
            "71 2C A1 D5 7F 7D 9F 51 EA C3 A1 8C D7 0C 7C 70 " +
            "D7 37 45 B8 CD 8F A4 8C F3 48 0C 81 07 19 BC 48 " +
            "06 52 BB 2F 67 CC 0A 18 FB 17 8C EC 24 02 5B DF " +
            "A2 A2 D9 9D 2E 95 01 31 A4 19 75 94 2A 41 39 2D " +
            "00 91 3E 93 B7 19 6E A8 F7 DD 05 7B 25 E6 7A B8 " +
            "31 A5 20 58 84 BB E6 DC 1E 55 B4 66 24 29 E8 47 " +
            "79 12 E6 CA 3D 3B 89 C2 12 77 11 9C E9 90 E8 0B " +
            "83 85 7B 13 28 B1 B7 79 BB BC 0D A1 76 F5 AA 1E " +
            "98 89 73 36 5A 91 BF 3E 52 7B 00 83 E7 48 27 37 " +
            "88 5E 92 5D 8E CD 50 62",
            // After Pi
            "8D A2 CA 6B CD 60 EA 02 71 2C A1 D5 7F 7D 9F 51 " +
            "A2 A2 D9 9D 2E 95 01 31 79 12 E6 CA 3D 3B 89 C2 " +
            "88 5E 92 5D 8E CD 50 62 CF 4A 2D 93 21 6D 96 99 " +
            "F3 48 0C 81 07 19 BC 48 06 52 BB 2F 67 CC 0A 18 " +
            "31 A5 20 58 84 BB E6 DC 98 89 73 36 5A 91 BF 3E " +
            "E1 08 36 35 B9 D2 A6 57 EA C3 A1 8C D7 0C 7C 70 " +
            "A4 19 75 94 2A 41 39 2D 12 77 11 9C E9 90 E8 0B " +
            "83 85 7B 13 28 B1 B7 79 90 13 87 16 E8 8D 07 53 " +
            "3F 03 C9 62 87 0C 78 D3 FB 17 8C EC 24 02 5B DF " +
            "1E 55 B4 66 24 29 E8 47 52 7B 00 83 E7 48 27 37 " +
            "43 7B D5 98 AF BD A5 0E D7 37 45 B8 CD 8F A4 8C " +
            "00 91 3E 93 B7 19 6E A8 F7 DD 05 7B 25 E6 7A B8 " +
            "BB BC 0D A1 76 F5 AA 1E",
            // After Chi
            "0F 20 92 63 CD E0 EA 22 28 3C 87 97 6E 57 17 93 " +
            "22 EE C9 88 AC 51 51 11 7C B2 AE E8 7C 1B 23 C2 " +
            "F8 52 B3 C9 BC D0 45 33 CB 58 9E BD 41 A9 94 89 " +
            "C2 ED 0C D1 87 2A 58 8C 8E 5A E8 09 3D CC 13 3A " +
            "76 E7 2C D9 A5 D7 E6 5D A8 89 73 36 5C 81 97 7E " +
            "E5 10 62 25 91 93 A7 5A F8 A5 A1 84 16 9C BC 72 " +
            "25 99 1F 97 2A 60 2E 5D 72 7F 15 B8 78 D2 E8 0D " +
            "89 46 FA 9B 6E BD EF 59 50 07 83 9A C8 8F 04 5F " +
            "3B 43 F9 60 87 25 D8 D3 BB 3D 8C 6D E7 42 5C EF " +
            "9E 55 33 72 2C AC E8 07 7D 7B 48 E3 E0 48 5F B7 " +
            "43 FB EF 9B 9D AD EF 2E 20 7B 44 D0 CD 69 B4 9C " +
            "08 B1 36 13 E5 08 EE AE B7 9E D5 63 AC EE 7F B8 " +
            "2F B8 0D 81 36 F7 AA 9E",
            // After Iota 
            "0F A0 92 E3 CD E0 EA A2 28 3C 87 97 6E 57 17 93 " +
            "22 EE C9 88 AC 51 51 11 7C B2 AE E8 7C 1B 23 C2 " +
            "F8 52 B3 C9 BC D0 45 33 CB 58 9E BD 41 A9 94 89 " +
            "C2 ED 0C D1 87 2A 58 8C 8E 5A E8 09 3D CC 13 3A " +
            "76 E7 2C D9 A5 D7 E6 5D A8 89 73 36 5C 81 97 7E " +
            "E5 10 62 25 91 93 A7 5A F8 A5 A1 84 16 9C BC 72 " +
            "25 99 1F 97 2A 60 2E 5D 72 7F 15 B8 78 D2 E8 0D " +
            "89 46 FA 9B 6E BD EF 59 50 07 83 9A C8 8F 04 5F " +
            "3B 43 F9 60 87 25 D8 D3 BB 3D 8C 6D E7 42 5C EF " +
            "9E 55 33 72 2C AC E8 07 7D 7B 48 E3 E0 48 5F B7 " +
            "43 FB EF 9B 9D AD EF 2E 20 7B 44 D0 CD 69 B4 9C " +
            "08 B1 36 13 E5 08 EE AE B7 9E D5 63 AC EE 7F B8 " +
            "2F B8 0D 81 36 F7 AA 9E",
            // Round #4
            // After Theta
            "96 66 C3 00 FF E8 1D DA 6E 6A 8C 3C 54 C0 98 FC " +
            "89 60 BD CA 5A 44 BA 68 50 AE D4 8C 75 0A 6D 8E " +
            "CD 9B C6 A5 0D 7C 9A 1E 52 9E CF 5E 73 A1 63 F1 " +
            "84 BB 07 7A BD BD D7 E3 25 D4 9C 4B CB D9 F8 43 " +
            "5A FB 56 BD AC C6 A8 11 9D 40 06 5A ED 2D 48 53 " +
            "7C D6 33 C6 A3 9B 50 22 BE F3 AA 2F 2C 0B 33 1D " +
            "8E 17 6B D5 DC 75 C5 24 5E 63 6F DC 71 C3 A6 41 " +
            "BC 8F 8F F7 DF 11 30 74 C9 C1 D2 79 FA 87 F3 27 " +
            "7D 15 F2 CB BD B2 57 BC 10 B3 F8 2F 11 57 B7 96 " +
            "B2 49 49 16 25 BD A6 4B 48 B2 3D 8F 51 E4 80 9A " +
            "DA 3D BE 78 AF A5 18 56 66 2D 4F 7B F7 FE 3B F3 " +
            "A3 3F 42 51 13 1D 05 D7 9B 82 AF 07 A5 FF 31 F4 " +
            "1A 71 78 ED 87 5B 75 B3",
            // After Rho
            "96 66 C3 00 FF E8 1D DA DD D4 18 79 A8 80 31 F9 " +
            "22 58 AF B2 16 91 2E 5A A7 D0 E6 08 E5 4A CD 58 " +
            "E0 D3 F4 68 DE 34 2E 6D 35 17 3A 16 2F E5 F9 EC " +
            "A0 D7 DB 7B 3D 4E B8 7B 50 09 35 E7 D2 72 36 FE " +
            "7D AB 5E 56 63 D4 08 AD 82 34 D5 09 64 A0 D5 DE " +
            "E1 B3 9E 31 1E DD 84 12 74 F8 CE AB BE B0 2C CC " +
            "AB E6 AE 2B 26 71 BC 58 86 4D 83 BC C6 DE B8 E3 " +
            "FB EF 08 18 3A DE C7 C7 F3 F4 0F E7 4F 92 83 A5 " +
            "7E B9 57 F6 8A B7 AF 42 5B 4B 88 59 FC 97 88 AB " +
            "D7 74 49 36 29 C9 A2 A4 9A 48 B2 3D 8F 51 E4 80 " +
            "62 58 69 F7 F8 E2 BD 96 9B B5 3C ED DD FB EF CC " +
            "F4 47 28 6A A2 A3 E0 7A 82 AF 07 A5 FF 31 F4 9B " +
            "DD AC 46 1C 5E FB E1 56",
            // After Pi
            "96 66 C3 00 FF E8 1D DA A0 D7 DB 7B 3D 4E B8 7B " +
            "AB E6 AE 2B 26 71 BC 58 D7 74 49 36 29 C9 A2 A4 " +
            "DD AC 46 1C 5E FB E1 56 A7 D0 E6 08 E5 4A CD 58 " +
            "82 34 D5 09 64 A0 D5 DE E1 B3 9E 31 1E DD 84 12 " +
            "7E B9 57 F6 8A B7 AF 42 F4 47 28 6A A2 A3 E0 7A " +
            "DD D4 18 79 A8 80 31 F9 50 09 35 E7 D2 72 36 FE " +
            "86 4D 83 BC C6 DE B8 E3 9A 48 B2 3D 8F 51 E4 80 " +
            "62 58 69 F7 F8 E2 BD 96 E0 D3 F4 68 DE 34 2E 6D " +
            "35 17 3A 16 2F E5 F9 EC 74 F8 CE AB BE B0 2C CC " +
            "5B 4B 88 59 FC 97 88 AB 82 AF 07 A5 FF 31 F4 9B " +
            "22 58 AF B2 16 91 2E 5A 7D AB 5E 56 63 D4 08 AD " +
            "FB EF 08 18 3A DE C7 C7 F3 F4 0F E7 4F 92 83 A5 " +
            "9B B5 3C ED DD FB EF CC",
            // After Chi
            "9D 46 E7 00 FD D9 19 DA F4 C7 9A 6F 34 C6 BA DF " +
            "A3 6E A8 23 70 43 FD 0A D5 36 C8 36 88 C9 BE 2C " +
            "FD 3D 5E 67 5E FD 41 77 C6 53 EC 38 FF 17 CD 58 " +
            "9C 3C 94 CF E4 82 FE 9E 61 F5 B6 39 3E DD C4 2A " +
            "7D 29 91 F6 CF FF A2 42 F4 63 39 6B A2 03 F0 FC " +
            "5B 90 9A 61 AC 0C B9 F8 48 09 05 E6 DB 73 72 FE " +
            "E6 5D CA 7E B6 7C A1 F5 07 CC A2 35 8F 51 E4 E9 " +
            "62 51 4C 71 AA 90 BB 90 A0 3B 30 C1 4E 24 2A 6D " +
            "3E 14 3A 46 6F E2 79 CF F4 5C C9 0F BD 90 58 DC " +
            "3B 1B 78 11 FC 93 82 CF 97 AB 0D B3 DE F0 25 1B " +
            "A0 1C AF BA 0E 9B E9 18 7D BB 59 B1 26 D4 08 8D " +
            "F3 EE 38 10 AA B7 AB 8F D3 BC 8C F5 4D 92 83 B7 " +
            "C6 16 6C A9 BC BF EF 69",
            // After Iota 
            "16 C6 E7 00 FD D9 19 DA F4 C7 9A 6F 34 C6 BA DF " +
            "A3 6E A8 23 70 43 FD 0A D5 36 C8 36 88 C9 BE 2C " +
            "FD 3D 5E 67 5E FD 41 77 C6 53 EC 38 FF 17 CD 58 " +
            "9C 3C 94 CF E4 82 FE 9E 61 F5 B6 39 3E DD C4 2A " +
            "7D 29 91 F6 CF FF A2 42 F4 63 39 6B A2 03 F0 FC " +
            "5B 90 9A 61 AC 0C B9 F8 48 09 05 E6 DB 73 72 FE " +
            "E6 5D CA 7E B6 7C A1 F5 07 CC A2 35 8F 51 E4 E9 " +
            "62 51 4C 71 AA 90 BB 90 A0 3B 30 C1 4E 24 2A 6D " +
            "3E 14 3A 46 6F E2 79 CF F4 5C C9 0F BD 90 58 DC " +
            "3B 1B 78 11 FC 93 82 CF 97 AB 0D B3 DE F0 25 1B " +
            "A0 1C AF BA 0E 9B E9 18 7D BB 59 B1 26 D4 08 8D " +
            "F3 EE 38 10 AA B7 AB 8F D3 BC 8C F5 4D 92 83 B7 " +
            "C6 16 6C A9 BC BF EF 69",
            // Round #5
            // After Theta
            "EB CE 7D 05 4C FA 57 49 38 0D DE BB 04 30 C3 DC " +
            "4F DB DE B0 C0 8E 48 08 82 26 78 83 0F 4E 55 79 " +
            "AC 0C 4D 32 FB 60 E4 97 3B 5B 76 3D 4E 34 83 CB " +
            "50 F6 D0 1B D4 74 87 9D 8D 40 C0 AA 8E 10 71 28 " +
            "2A 39 21 43 48 78 49 17 A5 52 2A 3E 07 9E 55 1C " +
            "A6 98 00 64 1D 2F F7 6B 84 C3 41 32 EB 85 0B FD " +
            "0A E8 BC ED 06 B1 14 F7 50 DC 12 80 08 D6 0F BC " +
            "33 60 5F 24 0F 0D 1E 70 5D 33 AA C4 FF 07 64 FE " +
            "F2 DE 7E 92 5F 14 00 CC 18 E9 BF 9C 0D 5D ED DE " +
            "6C 0B C8 A4 7B 14 69 9A C6 9A 1E E6 7B 6D 80 FB " +
            "5D 14 35 BF BF B8 A7 8B B1 71 1D 65 16 22 71 8E " +
            "1F 5B 4E 83 1A 7A 1E 8D 84 AC 3C 40 CA 15 68 E2 " +
            "97 27 7F FC 19 22 4A 89",
            // After Rho
            "EB CE 7D 05 4C FA 57 49 71 1A BC 77 09 60 86 B9 " +
            "D3 B6 37 2C B0 23 12 C2 E0 54 95 27 68 82 37 F8 " +
            "07 23 BF 64 65 68 92 D9 E3 44 33 B8 BC B3 65 D7 " +
            "BD 41 4D 77 D8 09 65 0F 4A 23 10 B0 AA 23 44 1C " +
            "9C 90 21 24 BC A4 0B 95 59 C5 51 2A A5 E2 73 E0 " +
            "33 C5 04 20 EB 78 B9 5F F4 13 0E 07 C9 AC 17 2E " +
            "6D 37 88 A5 B8 57 40 E7 AC 1F 78 A1 B8 25 00 11 " +
            "92 87 06 0F B8 19 B0 2F 89 FF 0F C8 FC BB 66 54 " +
            "4F F2 8B 02 80 59 DE DB 76 6F 8C F4 5F CE 86 AE " +
            "22 4D 93 6D 01 99 74 8F FB C6 9A 1E E6 7B 6D 80 " +
            "9E 2E 76 51 D4 FC FE E2 C6 C6 75 94 59 88 C4 39 " +
            "63 CB 69 50 43 CF A3 F1 AC 3C 40 CA 15 68 E2 84 " +
            "52 E2 E5 C9 1F 7F 86 88",
            // After Pi
            "EB CE 7D 05 4C FA 57 49 BD 41 4D 77 D8 09 65 0F " +
            "6D 37 88 A5 B8 57 40 E7 22 4D 93 6D 01 99 74 8F " +
            "52 E2 E5 C9 1F 7F 86 88 E0 54 95 27 68 82 37 F8 " +
            "59 C5 51 2A A5 E2 73 E0 33 C5 04 20 EB 78 B9 5F " +
            "4F F2 8B 02 80 59 DE DB 63 CB 69 50 43 CF A3 F1 " +
            "71 1A BC 77 09 60 86 B9 4A 23 10 B0 AA 23 44 1C " +
            "AC 1F 78 A1 B8 25 00 11 FB C6 9A 1E E6 7B 6D 80 " +
            "9E 2E 76 51 D4 FC FE E2 07 23 BF 64 65 68 92 D9 " +
            "E3 44 33 B8 BC B3 65 D7 F4 13 0E 07 C9 AC 17 2E " +
            "76 6F 8C F4 5F CE 86 AE AC 3C 40 CA 15 68 E2 84 " +
            "D3 B6 37 2C B0 23 12 C2 9C 90 21 24 BC A4 0B 95 " +
            "92 87 06 0F B8 19 B0 2F 89 FF 0F C8 FC BB 66 54 " +
            "C6 C6 75 94 59 88 C4 39",
            // After Chi
            "AB F8 FD 85 6C AC 57 A9 BF 09 5E 3F D9 81 51 07 " +
            "3D 95 EC 25 A6 31 C2 E7 8B 41 8B 69 41 19 25 CE " +
            "46 E3 E5 BB 8F 7E A6 8E C2 54 91 27 22 9A BF E7 " +
            "15 F7 DA 28 A5 E3 35 60 13 CC 64 70 A8 FE 98 7F " +
            "CF E6 1F 25 A8 59 CA D3 7A 4A 29 58 C6 AF E3 F1 " +
            "D5 06 D4 76 19 64 86 B8 19 E3 92 AE EC 79 29 9C " +
            "A8 37 1C E0 A8 A1 92 73 9A D6 12 38 EF 7B 6D 99 " +
            "94 0F 76 D1 76 FF BE E6 13 30 B3 63 24 64 80 F1 " +
            "E1 28 B3 48 AA F1 E5 57 7C 03 4E 0D C9 8C 77 2E " +
            "75 6C 33 D0 3F CE 96 F7 4C 78 40 52 8D FB 87 82 " +
            "D1 B1 31 27 B0 3A A2 E8 95 E8 28 E4 F8 06 4D C5 " +
            "D4 87 76 1B B9 19 30 06 98 CF 0D E0 5C 98 74 96 " +
            "CA C6 75 94 55 0C CD 2C",
            // After Iota 
            "AA F8 FD 05 6C AC 57 A9 BF 09 5E 3F D9 81 51 07 " +
            "3D 95 EC 25 A6 31 C2 E7 8B 41 8B 69 41 19 25 CE " +
            "46 E3 E5 BB 8F 7E A6 8E C2 54 91 27 22 9A BF E7 " +
            "15 F7 DA 28 A5 E3 35 60 13 CC 64 70 A8 FE 98 7F " +
            "CF E6 1F 25 A8 59 CA D3 7A 4A 29 58 C6 AF E3 F1 " +
            "D5 06 D4 76 19 64 86 B8 19 E3 92 AE EC 79 29 9C " +
            "A8 37 1C E0 A8 A1 92 73 9A D6 12 38 EF 7B 6D 99 " +
            "94 0F 76 D1 76 FF BE E6 13 30 B3 63 24 64 80 F1 " +
            "E1 28 B3 48 AA F1 E5 57 7C 03 4E 0D C9 8C 77 2E " +
            "75 6C 33 D0 3F CE 96 F7 4C 78 40 52 8D FB 87 82 " +
            "D1 B1 31 27 B0 3A A2 E8 95 E8 28 E4 F8 06 4D C5 " +
            "D4 87 76 1B B9 19 30 06 98 CF 0D E0 5C 98 74 96 " +
            "CA C6 75 94 55 0C CD 2C",
            // Round #6
            // After Theta
            "0A 5B 69 DA 0F AC 2D 4D 9D F6 3D 68 B7 7A 02 6F " +
            "9D EC 10 B9 AE 07 E7 44 F9 9B 39 23 58 51 C9 62 " +
            "8A 67 29 DF 6C 0A 5E B5 62 F7 05 F8 41 9A C5 03 " +
            "37 08 B9 7F CB 18 66 08 B3 B5 98 EC A0 C8 BD DC " +
            "BD 3C AD 6F B1 11 26 7F B6 CE E5 3C 25 DB 1B CA " +
            "75 A5 40 A9 7A 64 FC 5C 3B 1C F1 F9 82 82 7A F4 " +
            "08 4E E0 7C A0 97 B7 D0 E8 0C A0 72 F6 33 81 35 " +
            "58 8B BA B5 95 8B 46 DD B3 93 27 BC 47 64 FA 15 " +
            "C3 D7 D0 1F C4 0A B6 3F DC 7A B2 91 C1 BA 52 8D " +
            "07 B6 81 9A 26 86 7A 5B 80 FC 8C 36 6E 8F 7F B9 " +
            "71 12 A5 F8 D3 3A D8 0C B7 17 4B B3 96 FD 1E AD " +
            "74 FE 8A 87 B1 2F 15 A5 EA 15 BF AA 45 D0 98 3A " +
            "06 42 B9 F0 B6 78 35 17",
            // After Rho
            "0A 5B 69 DA 0F AC 2D 4D 3A ED 7B D0 6E F5 04 DE " +
            "27 3B 44 AE EB C1 39 51 15 95 2C 96 BF 99 33 82 " +
            "53 F0 AA 55 3C 4B F9 66 1F A4 59 3C 20 76 5F 80 " +
            "FB B7 8C 61 86 70 83 90 F7 6C 2D 26 3B 28 72 2F " +
            "9E D6 B7 D8 08 93 BF 5E BD A1 6C EB 5C CE 53 B2 " +
            "AA 2B 05 4A D5 23 E3 E7 D1 EF 70 C4 E7 0B 0A EA " +
            "E7 03 BD BC 85 46 70 02 67 02 6B D0 19 40 E5 EC " +
            "DA CA 45 A3 6E AC 45 DD 78 8F C8 F4 2B 66 27 4F " +
            "FA 83 58 C1 F6 67 F8 1A A9 46 6E 3D D9 C8 60 5D " +
            "50 6F EB C0 36 50 D3 C4 B9 80 FC 8C 36 6E 8F 7F " +
            "60 33 C4 49 94 E2 4F EB DE 5E 2C CD 5A F6 7B B4 " +
            "CE 5F F1 30 F6 A5 A2 94 15 BF AA 45 D0 98 3A EA " +
            "CD 85 81 50 2E BC 2D 5E",
            // After Pi
            "0A 5B 69 DA 0F AC 2D 4D FB B7 8C 61 86 70 83 90 " +
            "E7 03 BD BC 85 46 70 02 50 6F EB C0 36 50 D3 C4 " +
            "CD 85 81 50 2E BC 2D 5E 15 95 2C 96 BF 99 33 82 " +
            "BD A1 6C EB 5C CE 53 B2 AA 2B 05 4A D5 23 E3 E7 " +
            "FA 83 58 C1 F6 67 F8 1A CE 5F F1 30 F6 A5 A2 94 " +
            "3A ED 7B D0 6E F5 04 DE F7 6C 2D 26 3B 28 72 2F " +
            "67 02 6B D0 19 40 E5 EC B9 80 FC 8C 36 6E 8F 7F " +
            "60 33 C4 49 94 E2 4F EB 53 F0 AA 55 3C 4B F9 66 " +
            "1F A4 59 3C 20 76 5F 80 D1 EF 70 C4 E7 0B 0A EA " +
            "A9 46 6E 3D D9 C8 60 5D 15 BF AA 45 D0 98 3A EA " +
            "27 3B 44 AE EB C1 39 51 9E D6 B7 D8 08 93 BF 5E " +
            "DA CA 45 A3 6E AC 45 DD 78 8F C8 F4 2B 66 27 4F " +
            "DE 5E 2C CD 5A F6 7B B4",
            // After Chi
            "0E 5B 58 46 0E AA 5D 4F EB DB CE 21 B4 60 00 54 " +
            "6A 83 BD AC 8D EA 5C 18 52 35 83 4A 37 50 D3 C5 " +
            "3C 21 05 71 AE EC AF CE 17 9F 2D 96 3E B8 93 C7 " +
            "ED 21 34 6A 7E 8A 4B AA AE 77 A4 7A D5 A3 E1 63 " +
            "EB 03 54 47 FF 7F E9 18 66 7F B1 59 B6 E3 E2 A4 " +
            "3A EF 39 00 6E B5 81 1E 6F EC B9 2A 1D 06 78 3C " +
            "27 31 6B 91 99 C0 A5 6C A3 4C C7 1C 5C 7B 8F 6B " +
            "A5 33 C0 6F 85 EA 3D CA 93 BB 8A 95 FB 42 F9 0C " +
            "37 A4 57 05 38 B6 3F 95 C5 56 F0 84 E7 1B 10 48 " +
            "EB 06 6E 2D F5 8B A1 59 19 BB FB 6D D0 AC 3C 6A " +
            "67 33 04 8D 8D ED 79 D0 BE D3 3F 8C 09 D1 9D 5C " +
            "5C 9A 61 AA 3E 3C 1D 6D 59 AE 88 D6 8A 67 27 0E " +
            "46 9A 9F 9D 5A E4 FD BA",
            // After Iota 
            "8F DB 58 C6 0E AA 5D CF EB DB CE 21 B4 60 00 54 " +
            "6A 83 BD AC 8D EA 5C 18 52 35 83 4A 37 50 D3 C5 " +
            "3C 21 05 71 AE EC AF CE 17 9F 2D 96 3E B8 93 C7 " +
            "ED 21 34 6A 7E 8A 4B AA AE 77 A4 7A D5 A3 E1 63 " +
            "EB 03 54 47 FF 7F E9 18 66 7F B1 59 B6 E3 E2 A4 " +
            "3A EF 39 00 6E B5 81 1E 6F EC B9 2A 1D 06 78 3C " +
            "27 31 6B 91 99 C0 A5 6C A3 4C C7 1C 5C 7B 8F 6B " +
            "A5 33 C0 6F 85 EA 3D CA 93 BB 8A 95 FB 42 F9 0C " +
            "37 A4 57 05 38 B6 3F 95 C5 56 F0 84 E7 1B 10 48 " +
            "EB 06 6E 2D F5 8B A1 59 19 BB FB 6D D0 AC 3C 6A " +
            "67 33 04 8D 8D ED 79 D0 BE D3 3F 8C 09 D1 9D 5C " +
            "5C 9A 61 AA 3E 3C 1D 6D 59 AE 88 D6 8A 67 27 0E " +
            "46 9A 9F 9D 5A E4 FD BA",
            // Round #7
            // After Theta
            "EF 54 1E A1 D4 10 CF A8 49 EA CA BA AC 34 E4 FA " +
            "DB 47 7B 91 BC 10 AA D1 68 A5 40 4D 00 A4 A5 16 " +
            "39 B5 77 0A 15 44 02 BA 77 10 6B F1 E4 02 01 A0 " +
            "4F 10 30 F1 66 DE AF 04 1F B3 62 47 E4 59 17 AA " +
            "D1 93 97 40 C8 8B 9F CB 63 EB C3 22 0D 4B 4F D0 " +
            "5A 60 7F 67 B4 0F 13 79 CD DD BD B1 05 52 9C 92 " +
            "96 F5 AD AC A8 3A 53 A5 99 DC 04 1B 6B 8F F9 B8 " +
            "A0 A7 B2 14 3E 42 90 BE F3 34 CC F2 21 F8 6B 6B " +
            "95 95 53 9E 20 E2 DB 3B 74 92 36 B9 D6 E1 E6 81 " +
            "D1 96 AD 2A C2 7F D7 8A 1C 2F 89 16 6B 04 91 1E " +
            "07 BC 42 EA 57 57 EB B7 1C E2 3B 17 11 85 79 F2 " +
            "ED 5E A7 97 0F C6 EB A4 63 3E 4B D1 BD 93 51 DD " +
            "43 0E ED E6 E1 4C 50 CE",
            // After Rho
            "EF 54 1E A1 D4 10 CF A8 93 D4 95 75 59 69 C8 F5 " +
            "F6 D1 5E 24 2F 84 6A F4 40 5A 6A 81 56 0A D4 04 " +
            "20 12 D0 CD A9 BD 53 A8 4F 2E 10 00 7A 07 B1 16 " +
            "13 6F E6 FD 4A F0 04 01 EA C7 AC D8 11 79 D6 85 " +
            "C9 4B 20 E4 C5 CF E5 E8 F4 04 3D B6 3E 2C D2 B0 " +
            "D3 02 FB 3B A3 7D 98 C8 4A 36 77 F7 C6 16 48 71 " +
            "65 45 D5 99 2A B5 AC 6F 1E F3 71 33 B9 09 36 D6 " +
            "0A 1F 21 48 5F D0 53 59 E5 43 F0 D7 D6 E6 69 98 " +
            "CA 13 44 7C 7B A7 B2 72 F3 40 3A 49 9B 5C EB 70 " +
            "EF 5A 31 DA B2 55 45 F8 1E 1C 2F 89 16 6B 04 91 " +
            "AD DF 1E F0 0A A9 5F 5D 73 88 EF 5C 44 14 E6 C9 " +
            "DD EB F4 F2 C1 78 9D B4 3E 4B D1 BD 93 51 DD 63 " +
            "94 F3 90 43 BB 79 38 13",
            // After Pi
            "EF 54 1E A1 D4 10 CF A8 13 6F E6 FD 4A F0 04 01 " +
            "65 45 D5 99 2A B5 AC 6F EF 5A 31 DA B2 55 45 F8 " +
            "94 F3 90 43 BB 79 38 13 40 5A 6A 81 56 0A D4 04 " +
            "F4 04 3D B6 3E 2C D2 B0 D3 02 FB 3B A3 7D 98 C8 " +
            "CA 13 44 7C 7B A7 B2 72 DD EB F4 F2 C1 78 9D B4 " +
            "93 D4 95 75 59 69 C8 F5 EA C7 AC D8 11 79 D6 85 " +
            "1E F3 71 33 B9 09 36 D6 1E 1C 2F 89 16 6B 04 91 " +
            "AD DF 1E F0 0A A9 5F 5D 20 12 D0 CD A9 BD 53 A8 " +
            "4F 2E 10 00 7A 07 B1 16 4A 36 77 F7 C6 16 48 71 " +
            "F3 40 3A 49 9B 5C EB 70 3E 4B D1 BD 93 51 DD 63 " +
            "F6 D1 5E 24 2F 84 6A F4 C9 4B 20 E4 C5 CF E5 E8 " +
            "0A 1F 21 48 5F D0 53 59 E5 43 F0 D7 D6 E6 69 98 " +
            "73 88 EF 5C 44 14 E6 C9",
            // After Chi
            "8B 54 0F A1 F4 15 67 C6 99 75 C6 BF DA B0 45 91 " +
            "75 E4 55 98 23 9D 94 6C 84 5E 3F 7A F6 55 82 50 " +
            "84 D8 70 1F B1 99 38 12 43 58 A8 88 D7 5B DC 4C " +
            "FC 15 39 F2 66 AE F0 82 C6 EA 4B B9 23 25 95 4C " +
            "CA 03 4E 7D 6D A5 F2 72 69 EF E1 C4 E9 5C 9F 04 " +
            "87 E4 C4 56 F1 69 E8 A7 EA CB A2 50 17 1B D6 84 " +
            "BF 30 61 43 B1 89 6D 9A 0C 1C AE 8C 47 2B 84 31 " +
            "C5 DC 36 78 0A B9 49 5D 20 02 B7 3A 2D AD 1B C9 " +
            "FE 6E 18 08 63 4F 12 16 46 3D B6 43 C6 17 5C 72 " +
            "F3 50 3A 09 B3 F0 E9 F8 71 67 D1 BD C1 53 7D 75 " +
            "F4 C5 5F 2C 35 94 78 E5 2C 0B F0 73 45 E9 CD 68 " +
            "18 97 2E 40 5F C0 D5 18 61 12 E0 F7 FD 66 61 AC " +
            "7A 82 CF 9C 84 5F 63 C1",
            // After Iota 
            "82 D4 0F A1 F4 15 67 46 99 75 C6 BF DA B0 45 91 " +
            "75 E4 55 98 23 9D 94 6C 84 5E 3F 7A F6 55 82 50 " +
            "84 D8 70 1F B1 99 38 12 43 58 A8 88 D7 5B DC 4C " +
            "FC 15 39 F2 66 AE F0 82 C6 EA 4B B9 23 25 95 4C " +
            "CA 03 4E 7D 6D A5 F2 72 69 EF E1 C4 E9 5C 9F 04 " +
            "87 E4 C4 56 F1 69 E8 A7 EA CB A2 50 17 1B D6 84 " +
            "BF 30 61 43 B1 89 6D 9A 0C 1C AE 8C 47 2B 84 31 " +
            "C5 DC 36 78 0A B9 49 5D 20 02 B7 3A 2D AD 1B C9 " +
            "FE 6E 18 08 63 4F 12 16 46 3D B6 43 C6 17 5C 72 " +
            "F3 50 3A 09 B3 F0 E9 F8 71 67 D1 BD C1 53 7D 75 " +
            "F4 C5 5F 2C 35 94 78 E5 2C 0B F0 73 45 E9 CD 68 " +
            "18 97 2E 40 5F C0 D5 18 61 12 E0 F7 FD 66 61 AC " +
            "7A 82 CF 9C 84 5F 63 C1",
            // Round #8
            // After Theta
            "1A 46 DD EE F9 22 EE 6A AE F2 82 15 40 62 BE B1 " +
            "88 2D EA 14 8A A5 D0 0B 91 D6 AA 1E F1 53 87 7F " +
            "71 84 62 B9 B7 E9 24 57 DB CA 7A C7 DA 6C 55 60 " +
            "CB 92 7D 58 FC 7C 0B A2 3B 23 F4 35 8A 1D D1 2B " +
            "DF 8B DB 19 6A A3 F7 5D 9C B3 F3 62 EF 2C 83 41 " +
            "1F 76 16 19 FC 5E 61 8B DD 4C E6 FA 8D C9 2D A4 " +
            "42 F9 DE CF 18 B1 29 FD 19 94 3B E8 40 2D 81 1E " +
            "30 80 24 DE 0C C9 55 18 B8 90 65 75 20 9A 92 E5 " +
            "C9 E9 5C A2 F9 9D E9 36 BB F4 09 CF 6F 2F 18 15 " +
            "E6 D8 AF 6D B4 F6 EC D7 84 3B C3 1B C7 23 61 30 " +
            "6C 57 8D 63 38 A3 F1 C9 1B 8C B4 D9 DF 3B 36 48 " +
            "E5 5E 91 CC F6 F8 91 7F 74 9A 75 93 FA 60 64 83 " +
            "8F DE DD 3A 82 2F 7F 84",
            // After Rho
            "1A 46 DD EE F9 22 EE 6A 5D E5 05 2B 80 C4 7C 63 " +
            "62 8B 3A 85 62 29 F4 02 3F 75 F8 17 69 AD EA 11 " +
            "4D 27 B9 8A 23 14 CB BD AC CD 56 05 B6 AD AC 77 " +
            "87 C5 CF B7 20 BA 2C D9 CA CE 08 7D 8D 62 47 F4 " +
            "C5 ED 0C B5 D1 FB AE EF 32 18 C4 39 3B 2F F6 CE " +
            "FC B0 B3 C8 E0 F7 0A 5B 90 76 33 99 EB 37 26 B7 " +
            "7E C6 88 4D E9 17 CA F7 5A 02 3D 32 28 77 D0 81 " +
            "6F 86 E4 2A 0C 18 40 12 EA 40 34 25 CB 71 21 CB " +
            "4B 34 BF 33 DD 26 39 9D 8C 8A 5D FA 84 E7 B7 17 " +
            "9E FD DA 1C FB B5 8D D6 30 84 3B C3 1B C7 23 61 " +
            "C6 27 B3 5D 35 8E E1 8C 6D 30 D2 66 7F EF D8 20 " +
            "DC 2B 92 D9 1E 3F F2 AF 9A 75 93 FA 60 64 83 74 " +
            "1F E1 A3 77 B7 8E E0 CB",
            // After Pi
            "1A 46 DD EE F9 22 EE 6A 87 C5 CF B7 20 BA 2C D9 " +
            "7E C6 88 4D E9 17 CA F7 9E FD DA 1C FB B5 8D D6 " +
            "1F E1 A3 77 B7 8E E0 CB 3F 75 F8 17 69 AD EA 11 " +
            "32 18 C4 39 3B 2F F6 CE FC B0 B3 C8 E0 F7 0A 5B " +
            "4B 34 BF 33 DD 26 39 9D DC 2B 92 D9 1E 3F F2 AF " +
            "5D E5 05 2B 80 C4 7C 63 CA CE 08 7D 8D 62 47 F4 " +
            "5A 02 3D 32 28 77 D0 81 30 84 3B C3 1B C7 23 61 " +
            "C6 27 B3 5D 35 8E E1 8C 4D 27 B9 8A 23 14 CB BD " +
            "AC CD 56 05 B6 AD AC 77 90 76 33 99 EB 37 26 B7 " +
            "8C 8A 5D FA 84 E7 B7 17 9A 75 93 FA 60 64 83 74 " +
            "62 8B 3A 85 62 29 F4 02 C5 ED 0C B5 D1 FB AE EF " +
            "6F 86 E4 2A 0C 18 40 12 EA 40 34 25 CB 71 21 CB " +
            "6D 30 D2 66 7F EF D8 20",
            // After Chi
            "62 44 DD A6 30 27 2C 4C 07 FC 9D A7 32 1A 29 D9 " +
            "7F C6 A9 2E ED 1D AA FE 9E FB 86 94 B3 95 83 F6 " +
            "9A 60 A1 66 B7 16 E0 5A F3 D5 CB D7 A9 7D E2 00 " +
            "31 1C C8 0A 26 2F C7 4A 68 BB B3 00 E2 EE C8 79 " +
            "68 60 D7 35 BC A6 31 8D DC 23 96 F1 0C 3D E6 61 " +
            "4D E5 30 29 A0 D1 EC 62 EA 4A 0A BC 9E E2 64 94 " +
            "9C 21 BD 2E 0C 7F 10 0D 29 44 3F E1 9B 87 3F 02 " +
            "44 2D BB 09 38 AC E2 18 5D 15 98 12 6A 06 C9 3D " +
            "A0 45 1A 67 B2 6D 3D 77 82 03 B1 99 8B 37 26 D7 " +
            "C9 88 75 FA 87 F7 FF 9E 3A BD D5 FF F4 CD A7 36 " +
            "48 89 DA 8F 6E 29 B4 12 45 AD 1C B0 12 9A 8F 26 " +
            "6A B6 26 68 38 96 98 32 E8 CB 1C A4 CB 71 05 C9 " +
            "E8 54 D6 56 EE 3D D2 CD",
            // After Iota 
            "E8 44 DD A6 30 27 2C 4C 07 FC 9D A7 32 1A 29 D9 " +
            "7F C6 A9 2E ED 1D AA FE 9E FB 86 94 B3 95 83 F6 " +
            "9A 60 A1 66 B7 16 E0 5A F3 D5 CB D7 A9 7D E2 00 " +
            "31 1C C8 0A 26 2F C7 4A 68 BB B3 00 E2 EE C8 79 " +
            "68 60 D7 35 BC A6 31 8D DC 23 96 F1 0C 3D E6 61 " +
            "4D E5 30 29 A0 D1 EC 62 EA 4A 0A BC 9E E2 64 94 " +
            "9C 21 BD 2E 0C 7F 10 0D 29 44 3F E1 9B 87 3F 02 " +
            "44 2D BB 09 38 AC E2 18 5D 15 98 12 6A 06 C9 3D " +
            "A0 45 1A 67 B2 6D 3D 77 82 03 B1 99 8B 37 26 D7 " +
            "C9 88 75 FA 87 F7 FF 9E 3A BD D5 FF F4 CD A7 36 " +
            "48 89 DA 8F 6E 29 B4 12 45 AD 1C B0 12 9A 8F 26 " +
            "6A B6 26 68 38 96 98 32 E8 CB 1C A4 CB 71 05 C9 " +
            "E8 54 D6 56 EE 3D D2 CD",
            // Round #9
            // After Theta
            "4A 47 E0 1D FC 10 CD 38 82 C6 98 80 6E E5 EE 07 " +
            "BA BD FF D4 77 58 7C F4 5C 1D A9 0A 31 57 6D 28 " +
            "E2 2C 6F F2 14 6C 28 76 51 D6 F6 6C 65 4A 03 74 " +
            "B4 26 CD 2D 7A D0 00 94 AD C0 E5 FA 78 AB 1E 73 " +
            "AA 86 F8 AB 3E 64 DF 53 A4 6F 58 65 AF 47 2E 4D " +
            "EF E6 0D 92 6C E6 0D 16 6F 70 0F 9B C2 1D A3 4A " +
            "59 5A EB D4 96 3A C6 07 EB A2 10 7F 19 45 D1 DC " +
            "3C 61 75 9D 9B D6 2A 34 FF 16 A5 A9 A6 31 28 49 " +
            "25 7F 1F 40 EE 92 FA A9 47 78 E7 63 11 72 F0 DD " +
            "0B 6E 5A 64 05 35 11 40 42 F1 1B 6B 57 B7 6F 1A " +
            "EA 8A E7 34 A2 1E 55 66 C0 97 19 97 4E 65 48 F8 " +
            "AF CD 70 92 A2 D3 4E 38 2A 2D 33 3A 49 B3 EB 17 " +
            "90 18 18 C2 4D 47 1A E1",
            // After Rho
            "4A 47 E0 1D FC 10 CD 38 04 8D 31 01 DD CA DD 0F " +
            "6E EF 3F F5 1D 16 1F BD 73 D5 86 C2 D5 91 AA 10 " +
            "60 43 B1 13 67 79 93 A7 56 A6 34 40 17 65 6D CF " +
            "DC A2 07 0D 40 49 6B D2 5C 2B 70 B9 3E DE AA C7 " +
            "43 FC 55 1F B2 EF 29 55 E4 D2 44 FA 86 55 F6 7A " +
            "78 37 6F 90 64 33 6F B0 2A BD C1 3D 6C 0A 77 8C " +
            "A7 B6 D4 31 3E C8 D2 5A 8A A2 B9 D7 45 21 FE 32 " +
            "CE 4D 6B 15 1A 9E B0 BA 53 4D 63 50 92 FE 2D 4A " +
            "03 C8 5D 52 3F B5 E4 EF F8 EE 23 BC F3 B1 08 39 " +
            "26 02 68 C1 4D 8B AC A0 1A 42 F1 1B 6B 57 B7 6F " +
            "54 99 A9 2B 9E D3 88 7A 03 5F 66 5C 3A 95 21 E1 " +
            "B5 19 4E 52 74 DA 09 E7 2D 33 3A 49 B3 EB 17 2A " +
            "46 38 24 06 86 70 D3 91",
            // After Pi
            "4A 47 E0 1D FC 10 CD 38 DC A2 07 0D 40 49 6B D2 " +
            "A7 B6 D4 31 3E C8 D2 5A 26 02 68 C1 4D 8B AC A0 " +
            "46 38 24 06 86 70 D3 91 73 D5 86 C2 D5 91 AA 10 " +
            "E4 D2 44 FA 86 55 F6 7A 78 37 6F 90 64 33 6F B0 " +
            "03 C8 5D 52 3F B5 E4 EF B5 19 4E 52 74 DA 09 E7 " +
            "04 8D 31 01 DD CA DD 0F 5C 2B 70 B9 3E DE AA C7 " +
            "8A A2 B9 D7 45 21 FE 32 1A 42 F1 1B 6B 57 B7 6F " +
            "54 99 A9 2B 9E D3 88 7A 60 43 B1 13 67 79 93 A7 " +
            "56 A6 34 40 17 65 6D CF 2A BD C1 3D 6C 0A 77 8C " +
            "F8 EE 23 BC F3 B1 08 39 2D 33 3A 49 B3 EB 17 2A " +
            "6E EF 3F F5 1D 16 1F BD 43 FC 55 1F B2 EF 29 55 " +
            "CE 4D 6B 15 1A 9E B0 BA 53 4D 63 50 92 FE 2D 4A " +
            "03 5F 66 5C 3A 95 21 E1",
            // After Chi
            "69 53 30 2D C2 90 5D 30 DC A2 2F CD 01 4A 47 72 " +
            "E7 8E D0 37 BC B8 81 4B 2E 45 A8 D8 35 8B A0 88 " +
            "D2 98 23 06 86 39 F1 53 6B F0 AD C2 B5 B3 A3 90 " +
            "E7 1A 54 B8 9D D1 76 35 CC 26 6D 90 24 79 66 B0 " +
            "41 0C DD D2 BE B4 46 FF 31 1B 0E 6A 76 9E 5D 8D " +
            "86 0D B8 47 9C EB 89 3F 4C 6B 30 B1 14 88 AB 8A " +
            "CE 3B B1 F7 D1 A1 F6 22 1A 46 E1 1B 2A 5F E2 6A " +
            "0C BB E9 93 BC C7 AA BA 48 5A 70 2E 0F 73 81 A7 " +
            "86 E4 16 C0 84 D4 65 FE 2F AC D9 7C 6C 40 60 8E " +
            "B8 AE A2 AE B7 A1 88 BC 3B 97 3E 09 A3 EF 7B 62 " +
            "E2 EE 15 F5 15 06 8F 17 52 FC 55 5F 32 8F 24 15 " +
            "CE 5F 6F 19 32 9F B0 1B 3F ED 7A F1 97 FC 33 56 " +
            "02 4F 26 56 98 7C 01 A1",
            // After Iota 
            "E1 53 30 2D C2 90 5D 30 DC A2 2F CD 01 4A 47 72 " +
            "E7 8E D0 37 BC B8 81 4B 2E 45 A8 D8 35 8B A0 88 " +
            "D2 98 23 06 86 39 F1 53 6B F0 AD C2 B5 B3 A3 90 " +
            "E7 1A 54 B8 9D D1 76 35 CC 26 6D 90 24 79 66 B0 " +
            "41 0C DD D2 BE B4 46 FF 31 1B 0E 6A 76 9E 5D 8D " +
            "86 0D B8 47 9C EB 89 3F 4C 6B 30 B1 14 88 AB 8A " +
            "CE 3B B1 F7 D1 A1 F6 22 1A 46 E1 1B 2A 5F E2 6A " +
            "0C BB E9 93 BC C7 AA BA 48 5A 70 2E 0F 73 81 A7 " +
            "86 E4 16 C0 84 D4 65 FE 2F AC D9 7C 6C 40 60 8E " +
            "B8 AE A2 AE B7 A1 88 BC 3B 97 3E 09 A3 EF 7B 62 " +
            "E2 EE 15 F5 15 06 8F 17 52 FC 55 5F 32 8F 24 15 " +
            "CE 5F 6F 19 32 9F B0 1B 3F ED 7A F1 97 FC 33 56 " +
            "02 4F 26 56 98 7C 01 A1",
            // Round #10
            // After Theta
            "71 24 FD 3B C9 F3 97 DA 72 78 1B D5 DE 89 BD C4 " +
            "A1 DC 40 F0 80 8B 24 82 87 E4 AB AC CD D2 98 8A " +
            "6C E1 EF AE E5 7F BD FA FB 87 60 D4 BE D0 69 7A " +
            "49 C0 60 A0 42 12 8C 83 8A 74 FD 57 18 4A C3 79 " +
            "E8 AD DE A6 46 ED 7E FD 8F 62 C2 C2 15 D8 11 24 " +
            "16 7A 75 51 97 88 43 D5 E2 B1 04 A9 CB 4B 51 3C " +
            "88 69 21 30 ED 92 53 EB B3 E7 E2 6F D2 06 DA 68 " +
            "B2 C2 25 3B DF 81 E6 13 D8 2D BD 38 04 10 4B 4D " +
            "28 3E 22 D8 5B 17 9F 48 69 FE 49 BB 50 73 C5 47 " +
            "11 0F A1 DA 4F F8 B0 BE 85 EE F2 A1 C0 A9 37 CB " +
            "72 99 D8 E3 1E 65 45 FD FC 26 61 47 ED 4C DE A3 " +
            "88 0D FF DE 0E AC 15 D2 96 4C 79 85 6F A5 0B 54 " +
            "BC 36 EA FE FB 3A 4D 08",
            // After Rho
            "71 24 FD 3B C9 F3 97 DA E5 F0 36 AA BD 13 7B 89 " +
            "28 37 10 3C E0 22 89 60 2C 8D A9 78 48 BE CA DA " +
            "FF EB D5 67 0B 7F 77 2D ED 0B 9D A6 B7 7F 08 46 " +
            "06 2A 24 C1 38 98 04 0C 9E 22 5D FF 15 86 D2 70 " +
            "56 6F 53 A3 76 BF 7E F4 1D 41 F2 28 26 2C 5C 81 " +
            "B6 D0 AB 8B BA 44 1C AA F1 88 C7 12 A4 2E 2F 45 " +
            "81 69 97 9C 5A 47 4C 0B 0D B4 D1 66 CF C5 DF A4 " +
            "9D EF 40 F3 09 59 E1 92 71 08 20 96 9A B0 5B 7A " +
            "04 7B EB E2 13 09 C5 47 E2 A3 34 FF A4 5D A8 B9 " +
            "1F D6 37 E2 21 54 FB 09 CB 85 EE F2 A1 C0 A9 37 " +
            "15 F5 CB 65 62 8F 7B 94 F2 9B 84 1D B5 33 79 8F " +
            "B1 E1 DF DB 81 B5 42 1A 4C 79 85 6F A5 0B 54 96 " +
            "13 02 AF 8D BA FF BE 4E",
            // After Pi
            "71 24 FD 3B C9 F3 97 DA 06 2A 24 C1 38 98 04 0C " +
            "81 69 97 9C 5A 47 4C 0B 1F D6 37 E2 21 54 FB 09 " +
            "13 02 AF 8D BA FF BE 4E 2C 8D A9 78 48 BE CA DA " +
            "1D 41 F2 28 26 2C 5C 81 B6 D0 AB 8B BA 44 1C AA " +
            "04 7B EB E2 13 09 C5 47 B1 E1 DF DB 81 B5 42 1A " +
            "E5 F0 36 AA BD 13 7B 89 9E 22 5D FF 15 86 D2 70 " +
            "0D B4 D1 66 CF C5 DF A4 CB 85 EE F2 A1 C0 A9 37 " +
            "15 F5 CB 65 62 8F 7B 94 FF EB D5 67 0B 7F 77 2D " +
            "ED 0B 9D A6 B7 7F 08 46 F1 88 C7 12 A4 2E 2F 45 " +
            "E2 A3 34 FF A4 5D A8 B9 4C 79 85 6F A5 0B 54 96 " +
            "28 37 10 3C E0 22 89 60 56 6F 53 A3 76 BF 7E F4 " +
            "9D EF 40 F3 09 59 E1 92 71 08 20 96 9A B0 5B 7A " +
            "F2 9B 84 1D B5 33 79 8F",
            // After Chi
            "F0 65 6E 27 8B B4 DF D9 18 BC 04 A3 19 88 B7 0C " +
            "81 69 1F 91 C0 EC 48 4D 7F F2 67 D0 60 54 FA 99 " +
            "15 08 AF 4D 8A F7 BE 4A 8E 1D A0 FB D0 FE CA F0 " +
            "1D 6A B2 48 27 25 9D C4 07 50 BF 92 3A F0 1E B2 " +
            "08 77 CB C2 5B 03 4D 87 A0 A1 8D DB A7 B5 56 1B " +
            "E4 64 B6 AA 77 52 76 0D 5C 23 73 6F 35 86 F2 63 " +
            "19 C4 D0 63 8D CA 8D 24 2B 85 DA 78 3C D0 A9 3E " +
            "0F F7 82 30 62 0B FB E4 EF 6B 97 77 0B 7F 50 2C " +
            "EF 28 AD 4B B7 2E 88 FE FD D0 46 12 A5 2C 7B 43 " +
            "51 21 64 FF AE 29 8B 90 4C 79 8D EF 11 0B 5C D4 " +
            "A1 B7 10 6C E9 62 08 62 36 6F 73 A7 E4 1F 64 9C " +
            "1F 7C C4 FA 2C 5A C1 17 79 2C 30 B6 DA B0 DB 1A " +
            "A4 D3 C7 9E A3 AE 0F 1B",
            // After Iota 
            "F9 E5 6E A7 8B B4 DF D9 18 BC 04 A3 19 88 B7 0C " +
            "81 69 1F 91 C0 EC 48 4D 7F F2 67 D0 60 54 FA 99 " +
            "15 08 AF 4D 8A F7 BE 4A 8E 1D A0 FB D0 FE CA F0 " +
            "1D 6A B2 48 27 25 9D C4 07 50 BF 92 3A F0 1E B2 " +
            "08 77 CB C2 5B 03 4D 87 A0 A1 8D DB A7 B5 56 1B " +
            "E4 64 B6 AA 77 52 76 0D 5C 23 73 6F 35 86 F2 63 " +
            "19 C4 D0 63 8D CA 8D 24 2B 85 DA 78 3C D0 A9 3E " +
            "0F F7 82 30 62 0B FB E4 EF 6B 97 77 0B 7F 50 2C " +
            "EF 28 AD 4B B7 2E 88 FE FD D0 46 12 A5 2C 7B 43 " +
            "51 21 64 FF AE 29 8B 90 4C 79 8D EF 11 0B 5C D4 " +
            "A1 B7 10 6C E9 62 08 62 36 6F 73 A7 E4 1F 64 9C " +
            "1F 7C C4 FA 2C 5A C1 17 79 2C 30 B6 DA B0 DB 1A " +
            "A4 D3 C7 9E A3 AE 0F 1B",
            // Round #11
            // After Theta
            "AA 74 B3 A0 C6 6C F7 31 3E 5E 1F 5F 2A CC 4F 78 " +
            "E8 C1 40 BF 7E CA E0 D0 A6 4B 40 F7 65 2D 1A E2 " +
            "DB 84 73 B5 64 E2 86 34 DD 8C 7D FC 9D 26 E2 18 " +
            "3B 88 A9 B4 14 61 65 B0 6E F8 E0 BC 84 D6 B6 2F " +
            "D1 CE EC E5 5E 7A AD FC 6E 2D 51 23 49 A0 6E 65 " +
            "B7 F5 6B AD 3A 8A 5E E5 7A C1 68 93 06 C2 0A 17 " +
            "70 6C 8F 4D 33 EC 25 B9 F2 3C FD 5F 39 A9 49 45 " +
            "C1 7B 5E C8 8C 1E C3 9A BC FA 4A 70 46 A7 78 C4 " +
            "C9 CA B6 B7 84 6A 70 8A 94 78 19 3C 1B 0A D3 DE " +
            "88 98 43 D8 AB 50 6B EB 82 F5 51 17 FF 1E 64 AA " +
            "F2 26 CD 6B A4 BA 20 8A 10 8D 68 5B D7 5B 9C E8 " +
            "76 D4 9B D4 92 7C 69 8A A0 95 17 91 DF C9 3B 61 " +
            "6A 5F 1B 66 4D BB 37 65",
            // After Rho
            "AA 74 B3 A0 C6 6C F7 31 7C BC 3E BE 54 98 9F F0 " +
            "7A 30 D0 AF 9F 32 38 34 D6 A2 21 6E BA 04 74 5F " +
            "13 37 A4 D9 26 9C AB 25 DF 69 22 8E D1 CD D8 C7 " +
            "4A 4B 11 56 06 BB 83 98 8B 1B 3E 38 2F A1 B5 ED " +
            "67 F6 72 2F BD 56 FE 68 EA 56 E6 D6 12 35 92 04 " +
            "BF AD 5F 6B D5 51 F4 2A 5C E8 05 A3 4D 1A 08 2B " +
            "6C 9A 61 2F C9 85 63 7B 52 93 8A E4 79 FA BF 72 " +
            "64 46 8F 61 CD E0 3D 2F E0 8C 4E F1 88 79 F5 95 " +
            "F6 96 50 0D 4E 31 59 D9 69 6F 4A BC 0C 9E 0D 85 " +
            "6A 6D 1D 11 73 08 7B 15 AA 82 F5 51 17 FF 1E 64 " +
            "82 28 CA 9B 34 AF 91 EA 43 34 A2 6D 5D 6F 71 A2 " +
            "8E 7A 93 5A 92 2F 4D D1 95 17 91 DF C9 3B 61 A0 " +
            "4D 99 DA D7 86 59 D3 EE",
            // After Pi
            "AA 74 B3 A0 C6 6C F7 31 4A 4B 11 56 06 BB 83 98 " +
            "6C 9A 61 2F C9 85 63 7B 6A 6D 1D 11 73 08 7B 15 " +
            "4D 99 DA D7 86 59 D3 EE D6 A2 21 6E BA 04 74 5F " +
            "EA 56 E6 D6 12 35 92 04 BF AD 5F 6B D5 51 F4 2A " +
            "F6 96 50 0D 4E 31 59 D9 8E 7A 93 5A 92 2F 4D D1 " +
            "7C BC 3E BE 54 98 9F F0 8B 1B 3E 38 2F A1 B5 ED " +
            "52 93 8A E4 79 FA BF 72 AA 82 F5 51 17 FF 1E 64 " +
            "82 28 CA 9B 34 AF 91 EA 13 37 A4 D9 26 9C AB 25 " +
            "DF 69 22 8E D1 CD D8 C7 5C E8 05 A3 4D 1A 08 2B " +
            "69 6F 4A BC 0C 9E 0D 85 95 17 91 DF C9 3B 61 A0 " +
            "7A 30 D0 AF 9F 32 38 34 67 F6 72 2F BD 56 FE 68 " +
            "64 46 8F 61 CD E0 3D 2F E0 8C 4E F1 88 79 F5 95 " +
            "43 34 A2 6D 5D 6F 71 A2",
            // After Chi
            "8E E4 D3 89 0F 68 97 52 48 2E 0D 46 34 B3 9B 9C " +
            "69 0A A3 E9 4D D4 E3 91 C8 09 3C 31 33 2C 5F 04 " +
            "0D 92 DA 81 86 CA D3 66 C3 0B 38 47 7F 44 10 75 " +
            "AA 44 E6 D2 18 15 9B D5 B7 C5 DC 39 45 5F F0 2A " +
            "A6 16 70 29 66 31 69 D7 A6 2E 55 CA 92 1E CF D1 " +
            "2C 3C BE 7A 04 C2 95 E2 23 1B 4B 29 29 A4 B5 E9 " +
            "52 BB 80 6E 59 FA 3E F8 D6 16 C1 75 57 EF 10 74 " +
            "01 2B CA 9B 1F 8E B1 E7 13 B7 A1 F8 2A 8E AB 0D " +
            "FE 6E 68 92 D1 49 DD 43 C8 F8 94 E0 8C 3B 68 0B " +
            "6B 4F 6E BC 2A 1A 87 80 59 5F 93 D9 18 7A 31 62 " +
            "7A 30 5D EF DF 92 39 33 E7 7E 32 BF BD 4F 3E F8 " +
            "67 76 2F 6D 98 E6 3D 0D D8 8C 1E 73 0A 69 FD 81 " +
            "46 F2 80 6D 7D 2B B7 EA",
            // After Iota 
            "84 E4 D3 09 0F 68 97 52 48 2E 0D 46 34 B3 9B 9C " +
            "69 0A A3 E9 4D D4 E3 91 C8 09 3C 31 33 2C 5F 04 " +
            "0D 92 DA 81 86 CA D3 66 C3 0B 38 47 7F 44 10 75 " +
            "AA 44 E6 D2 18 15 9B D5 B7 C5 DC 39 45 5F F0 2A " +
            "A6 16 70 29 66 31 69 D7 A6 2E 55 CA 92 1E CF D1 " +
            "2C 3C BE 7A 04 C2 95 E2 23 1B 4B 29 29 A4 B5 E9 " +
            "52 BB 80 6E 59 FA 3E F8 D6 16 C1 75 57 EF 10 74 " +
            "01 2B CA 9B 1F 8E B1 E7 13 B7 A1 F8 2A 8E AB 0D " +
            "FE 6E 68 92 D1 49 DD 43 C8 F8 94 E0 8C 3B 68 0B " +
            "6B 4F 6E BC 2A 1A 87 80 59 5F 93 D9 18 7A 31 62 " +
            "7A 30 5D EF DF 92 39 33 E7 7E 32 BF BD 4F 3E F8 " +
            "67 76 2F 6D 98 E6 3D 0D D8 8C 1E 73 0A 69 FD 81 " +
            "46 F2 80 6D 7D 2B B7 EA",
            // Round #12
            // After Theta
            "81 1D 71 4C B2 6B 10 BC 0C 8E 2D 03 3F 19 EA ED " +
            "A6 FF A2 3C 61 D2 0C C6 80 86 D4 CA AA 96 71 F1 " +
            "03 F0 75 64 A6 AE 8E 37 C6 F2 9A 02 C2 47 97 9B " +
            "EE E4 C6 97 13 BF EA A4 78 30 DD EC 69 59 1F 7D " +
            "EE 99 98 D2 FF 8B 47 22 A8 4C FA 2F B2 7A 92 80 " +
            "29 C5 1C 3F B9 C1 12 0C 67 BB 6B 6C 22 0E C4 98 " +
            "9D 4E 81 BB 75 FC D1 AF 9E 99 29 8E CE 55 3E 81 " +
            "0F 49 65 7E 3F EA EC B6 16 4E 03 BD 97 8D 2C E3 " +
            "BA CE 48 D7 DA E3 AC 32 07 0D 95 35 A0 3D 87 5C " +
            "23 C0 86 47 B3 A0 A9 75 57 3D 3C 3C 38 1E 6C 33 " +
            "7F C9 FF AA 62 91 BE DD A3 DE 12 FA B6 E5 4F 89 " +
            "A8 83 2E B8 B4 E0 D2 5A 90 03 F6 88 93 D3 D3 74 " +
            "48 90 2F 88 5D 4F EA BB",
            // After Rho
            "81 1D 71 4C B2 6B 10 BC 19 1C 5B 06 7E 32 D4 DB " +
            "E9 BF 28 4F 98 34 83 B1 6A 19 17 0F 68 48 AD AC " +
            "75 75 BC 19 80 AF 23 33 20 7C 74 B9 69 2C AF 29 " +
            "7C 39 F1 AB 4E EA 4E 6E 1F 1E 4C 37 7B 5A D6 47 " +
            "4C 4C E9 FF C5 23 11 F7 27 09 88 CA A4 FF 22 AB " +
            "48 29 E6 F8 C9 0D 96 60 63 9E ED AE B1 89 38 10 " +
            "DC AD E3 8F 7E ED 74 0A AB 7C 02 3D 33 53 1C 9D " +
            "BF 1F 75 76 DB 87 A4 32 7A 2F 1B 59 C6 2D 9C 06 " +
            "E9 5A 7B 9C 55 46 D7 19 43 AE 83 86 CA 1A D0 9E " +
            "34 B5 6E 04 D8 F0 68 16 33 57 3D 3C 3C 38 1E 6C " +
            "FA 76 FF 25 FF AB 8A 45 8E 7A 4B E8 DB 96 3F 25 " +
            "75 D0 05 97 16 5C 5A 0B 03 F6 88 93 D3 D3 74 90 " +
            "FA 2E 12 E4 0B 62 D7 93",
            // After Pi
            "81 1D 71 4C B2 6B 10 BC 7C 39 F1 AB 4E EA 4E 6E " +
            "DC AD E3 8F 7E ED 74 0A 34 B5 6E 04 D8 F0 68 16 " +
            "FA 2E 12 E4 0B 62 D7 93 6A 19 17 0F 68 48 AD AC " +
            "27 09 88 CA A4 FF 22 AB 48 29 E6 F8 C9 0D 96 60 " +
            "E9 5A 7B 9C 55 46 D7 19 75 D0 05 97 16 5C 5A 0B " +
            "19 1C 5B 06 7E 32 D4 DB 1F 1E 4C 37 7B 5A D6 47 " +
            "AB 7C 02 3D 33 53 1C 9D 33 57 3D 3C 3C 38 1E 6C " +
            "FA 76 FF 25 FF AB 8A 45 75 75 BC 19 80 AF 23 33 " +
            "20 7C 74 B9 69 2C AF 29 63 9E ED AE B1 89 38 10 " +
            "43 AE 83 86 CA 1A D0 9E 03 F6 88 93 D3 D3 74 90 " +
            "E9 BF 28 4F 98 34 83 B1 4C 4C E9 FF C5 23 11 F7 " +
            "BF 1F 75 76 DB 87 A4 32 7A 2F 1B 59 C6 2D 9C 06 " +
            "8E 7A 4B E8 DB 96 3F 25",
            // After Chi
            "01 99 73 48 82 6E 20 BC 5C 29 FD AB CE FA 46 7A " +
            "16 A7 F3 6F 7D EF E3 8B 35 A4 0F 0C 68 F9 68 3A " +
            "86 0E 92 47 47 E2 99 D1 22 39 71 3F 21 48 39 EC " +
            "86 5B 91 CE B0 BD 63 B2 5C A9 E2 FB CB 15 9E 62 " +
            "E3 53 69 94 3D 46 72 BD 70 D0 8D 57 92 EB 58 08 " +
            "B9 7C 59 0E 7E 33 DC 43 0F 1D 71 37 77 72 D4 27 " +
            "63 5C C0 3C F0 D0 9C 9C 32 5F 3D 3E 3C 28 4A F6 " +
            "FC 74 FB 14 FE E3 88 41 36 F7 35 1F 10 2E 33 23 " +
            "20 5C 76 B9 23 3E 6F A7 63 CE E5 BF A0 48 1C 10 " +
            "37 AF B7 8E CA 36 D3 BD 03 FE C8 33 BA D3 F8 98 " +
            "5A AC 3C 4F 82 B0 27 B1 0C 6C E3 F6 C1 0B 09 F3 " +
            "3B 4F 35 D6 C2 15 87 13 1B AA 3B 5E C6 0D 1C 96 " +
            "8A 3A 8A 58 9E 95 2F 63",
            // After Iota 
            "8A 19 73 C8 82 6E 20 BC 5C 29 FD AB CE FA 46 7A " +
            "16 A7 F3 6F 7D EF E3 8B 35 A4 0F 0C 68 F9 68 3A " +
            "86 0E 92 47 47 E2 99 D1 22 39 71 3F 21 48 39 EC " +
            "86 5B 91 CE B0 BD 63 B2 5C A9 E2 FB CB 15 9E 62 " +
            "E3 53 69 94 3D 46 72 BD 70 D0 8D 57 92 EB 58 08 " +
            "B9 7C 59 0E 7E 33 DC 43 0F 1D 71 37 77 72 D4 27 " +
            "63 5C C0 3C F0 D0 9C 9C 32 5F 3D 3E 3C 28 4A F6 " +
            "FC 74 FB 14 FE E3 88 41 36 F7 35 1F 10 2E 33 23 " +
            "20 5C 76 B9 23 3E 6F A7 63 CE E5 BF A0 48 1C 10 " +
            "37 AF B7 8E CA 36 D3 BD 03 FE C8 33 BA D3 F8 98 " +
            "5A AC 3C 4F 82 B0 27 B1 0C 6C E3 F6 C1 0B 09 F3 " +
            "3B 4F 35 D6 C2 15 87 13 1B AA 3B 5E C6 0D 1C 96 " +
            "8A 3A 8A 58 9E 95 2F 63",
            // Round #13
            // After Theta
            "FA C8 C5 9C 5B C3 90 A8 C3 88 AC 80 C8 9F 63 17 " +
            "7F A3 D4 9F 5C B7 4B 85 42 AA 42 12 52 D6 2F 8B " +
            "B5 AD E1 63 BD 58 A5 88 52 E8 C7 6B F8 E5 89 F8 " +
            "19 FA C0 E5 B6 D8 46 DF 35 AD C5 0B EA 4D 36 6C " +
            "94 5D 24 8A 07 69 35 0C 43 73 FE 73 68 51 64 51 " +
            "C9 AD EF 5A A7 9E 6C 57 90 BC 20 1C 71 17 F1 4A " +
            "0A 58 E7 CC D1 88 34 92 45 51 70 20 06 07 0D 47 " +
            "CF D7 88 30 04 59 B4 18 46 26 83 4B C9 83 83 37 " +
            "BF FD 27 92 25 5B 4A CA 0A CA C2 4F 81 10 B4 1E " +
            "40 A1 FA 90 F0 19 94 0C 30 5D BB 17 40 69 C4 C1 " +
            "2A 7D 8A 1B 5B 1D 97 A5 93 CD B2 DD C7 6E 2C 9E " +
            "52 4B 12 26 E3 4D 2F 1D 6C A4 76 40 FC 22 5B 27 " +
            "B9 99 F9 7C 64 2F 13 3A",
            // After Rho
            "FA C8 C5 9C 5B C3 90 A8 86 11 59 01 91 3F C7 2E " +
            "DF 28 F5 27 D7 ED 52 E1 65 FD B2 28 A4 2A 24 21 " +
            "C5 2A 45 AC 6D 0D 1F EB 86 5F 9E 88 2F 85 7E BC " +
            "5C 6E 8B 6D F4 9D A1 0F 5B 4D 6B F1 82 7A 93 0D " +
            "2E 12 C5 83 B4 1A 06 CA 45 16 35 34 E7 3F 87 16 " +
            "4A 6E 7D D7 3A F5 64 BB 2B 41 F2 82 70 C4 5D C4 " +
            "67 8E 46 A4 91 54 C0 3A 0E 1A 8E 8A A2 E0 40 0C " +
            "18 82 2C 5A 8C E7 6B 44 97 92 07 07 6F 8C 4C 06 " +
            "44 B2 64 4B 49 F9 B7 FF 5A 0F 05 65 E1 A7 40 08 " +
            "83 92 01 28 54 1F 12 3E C1 30 5D BB 17 40 69 C4 " +
            "5C 96 AA F4 29 6E 6C 75 4E 36 CB 76 1F BB B1 78 " +
            "6A 49 C2 64 BC E9 A5 43 A4 76 40 FC 22 5B 27 6C " +
            "84 4E 6E 66 3E 1F D9 CB",
            // After Pi
            "FA C8 C5 9C 5B C3 90 A8 5C 6E 8B 6D F4 9D A1 0F " +
            "67 8E 46 A4 91 54 C0 3A 83 92 01 28 54 1F 12 3E " +
            "84 4E 6E 66 3E 1F D9 CB 65 FD B2 28 A4 2A 24 21 " +
            "45 16 35 34 E7 3F 87 16 4A 6E 7D D7 3A F5 64 BB " +
            "44 B2 64 4B 49 F9 B7 FF 6A 49 C2 64 BC E9 A5 43 " +
            "86 11 59 01 91 3F C7 2E 5B 4D 6B F1 82 7A 93 0D " +
            "0E 1A 8E 8A A2 E0 40 0C C1 30 5D BB 17 40 69 C4 " +
            "5C 96 AA F4 29 6E 6C 75 C5 2A 45 AC 6D 0D 1F EB " +
            "86 5F 9E 88 2F 85 7E BC 2B 41 F2 82 70 C4 5D C4 " +
            "5A 0F 05 65 E1 A7 40 08 A4 76 40 FC 22 5B 27 6C " +
            "DF 28 F5 27 D7 ED 52 E1 2E 12 C5 83 B4 1A 06 CA " +
            "18 82 2C 5A 8C E7 6B 44 97 92 07 07 6F 8C 4C 06 " +
            "4E 36 CB 76 1F BB B1 78",
            // After Chi
            "D9 48 81 1C 5A 83 D0 98 DC 7E 8A 65 B0 96 B3 0B " +
            "63 C2 28 E2 BB 54 09 FB F9 12 80 B0 15 DF 12 1E " +
            "80 68 64 07 9A 03 F8 CC 6F 95 FA EB BC EA 44 88 " +
            "41 86 35 3C A6 37 14 52 60 27 FF F3 8E F5 64 BB " +
            "41 06 54 43 49 FB B7 DF 6A 4B C7 70 FF FC 26 55 " +
            "82 03 DD 0B B1 BF 87 2E 9A 6D 3A C0 97 7A BA CD " +
            "12 9C 2C CE 8A CE 44 3D 43 31 0C BA 87 51 EA CE " +
            "05 DA 88 04 2B 2E 7C 74 EC 2A 25 AE 3D 4D 1E AB " +
            "D6 51 9B ED AE A6 7E B4 8F 31 B2 1A 72 9C 7A A0 " +
            "1B 07 00 65 AC A3 58 8B A6 23 DA FC 20 DB 47 78 " +
            "CF A8 DD 7F DF 08 3B E5 A9 02 C6 86 D7 12 02 C8 " +
            "50 A6 E4 2A 9C D4 DA 3C 06 9A 33 06 AF C8 0E 87 " +
            "6E 24 CB F6 3F A9 B5 72",
            // After Iota 
            "52 48 81 1C 5A 83 D0 18 DC 7E 8A 65 B0 96 B3 0B " +
            "63 C2 28 E2 BB 54 09 FB F9 12 80 B0 15 DF 12 1E " +
            "80 68 64 07 9A 03 F8 CC 6F 95 FA EB BC EA 44 88 " +
            "41 86 35 3C A6 37 14 52 60 27 FF F3 8E F5 64 BB " +
            "41 06 54 43 49 FB B7 DF 6A 4B C7 70 FF FC 26 55 " +
            "82 03 DD 0B B1 BF 87 2E 9A 6D 3A C0 97 7A BA CD " +
            "12 9C 2C CE 8A CE 44 3D 43 31 0C BA 87 51 EA CE " +
            "05 DA 88 04 2B 2E 7C 74 EC 2A 25 AE 3D 4D 1E AB " +
            "D6 51 9B ED AE A6 7E B4 8F 31 B2 1A 72 9C 7A A0 " +
            "1B 07 00 65 AC A3 58 8B A6 23 DA FC 20 DB 47 78 " +
            "CF A8 DD 7F DF 08 3B E5 A9 02 C6 86 D7 12 02 C8 " +
            "50 A6 E4 2A 9C D4 DA 3C 06 9A 33 06 AF C8 0E 87 " +
            "6E 24 CB F6 3F A9 B5 72",
            // Round #14
            // After Theta
            "84 3A 0A 80 FA FF 42 2F DD FF 8F 97 A6 4B 97 38 " +
            "D7 75 27 45 F3 06 5A 15 78 00 58 AD E6 BE 3A 31 " +
            "5F 69 33 77 28 3A 8C 2F B9 E7 71 77 1C 96 D6 BF " +
            "40 07 30 CE B0 EA 30 61 D4 90 F0 54 C6 A7 37 55 " +
            "C0 14 8C 5E BA 9A 9F F0 B5 4A 90 00 4D C5 52 B6 " +
            "54 71 56 97 11 C3 15 19 9B EC 3F 32 81 A7 9E FE " +
            "A6 2B 23 69 C2 9C 17 D3 C2 23 D4 A7 74 30 C2 E1 " +
            "DA DB DF 74 99 17 08 97 3A 58 AE 32 9D 31 8C 9C " +
            "D7 D0 9E 1F B8 7B 5A 87 3B 86 BD BD 3A CE 29 4E " +
            "9A 15 D8 78 5F C2 70 A4 79 22 8D 8C 92 E2 33 9B " +
            "19 DA 56 E3 7F 74 A9 D2 A8 83 C3 74 C1 CF 26 FB " +
            "E4 11 EB 8D D4 86 89 D2 87 88 EB 1B 5C A9 26 A8 " +
            "B1 25 9C 86 8D 90 C1 91",
            // After Rho
            "84 3A 0A 80 FA FF 42 2F BA FF 1F 2F 4D 97 2E 71 " +
            "75 DD 49 D1 BC 81 56 C5 EE AB 13 83 07 80 D5 6A " +
            "D1 61 7C F9 4A 9B B9 43 C7 61 69 FD 9B 7B 1E 77 " +
            "E3 0C AB 0E 13 06 74 00 15 35 24 3C 95 F1 E9 4D " +
            "0A 46 2F 5D CD 4F 78 60 2C 65 5B AB 04 09 D0 54 " +
            "A0 8A B3 BA 8C 18 AE C8 FA 6F B2 FF C8 04 9E 7A " +
            "49 13 E6 BC 98 36 5D 19 60 84 C3 85 47 A8 4F E9 " +
            "BA CC 0B 84 4B ED ED 6F 65 3A 63 18 39 75 B0 5C " +
            "F3 03 77 4F EB F0 1A DA 14 A7 1D C3 DE 5E 1D E7 " +
            "18 8E 54 B3 02 1B EF 4B 9B 79 22 8D 8C 92 E2 33 " +
            "A5 4A 67 68 5B 8D FF D1 A3 0E 0E D3 05 3F 9B EC " +
            "3C 62 BD 91 DA 30 51 9A 88 EB 1B 5C A9 26 A8 87 " +
            "70 64 6C 09 A7 61 23 64",
            // After Pi
            "84 3A 0A 80 FA FF 42 2F E3 0C AB 0E 13 06 74 00 " +
            "49 13 E6 BC 98 36 5D 19 18 8E 54 B3 02 1B EF 4B " +
            "70 64 6C 09 A7 61 23 64 EE AB 13 83 07 80 D5 6A " +
            "2C 65 5B AB 04 09 D0 54 A0 8A B3 BA 8C 18 AE C8 " +
            "F3 03 77 4F EB F0 1A DA 3C 62 BD 91 DA 30 51 9A " +
            "BA FF 1F 2F 4D 97 2E 71 15 35 24 3C 95 F1 E9 4D " +
            "60 84 C3 85 47 A8 4F E9 9B 79 22 8D 8C 92 E2 33 " +
            "A5 4A 67 68 5B 8D FF D1 D1 61 7C F9 4A 9B B9 43 " +
            "C7 61 69 FD 9B 7B 1E 77 FA 6F B2 FF C8 04 9E 7A " +
            "14 A7 1D C3 DE 5E 1D E7 88 EB 1B 5C A9 26 A8 87 " +
            "75 DD 49 D1 BC 81 56 C5 0A 46 2F 5D CD 4F 78 60 " +
            "BA CC 0B 84 4B ED ED 6F 65 3A 63 18 39 75 B0 5C " +
            "A3 0E 0E D3 05 3F 9B EC",
            // After Chi
            "8C 29 4E 30 72 CF 4B 36 F3 80 BB 0D 11 0F D6 42 " +
            "29 73 CE B4 3D 56 5D 3D 9C 94 56 33 5A 85 AF 40 " +
            "13 60 CD 07 A6 61 17 64 6E 21 B3 93 8F 90 FB E2 " +
            "7F 64 1F EE 67 E9 C0 46 AC EA 3B 2A 9C 18 EF C8 " +
            "31 8A 75 4D EE 70 9E BA 3C 26 F5 B9 DA 39 51 8E " +
            "DA 7F DC AE 0F 9F 28 D1 8E 4C 04 34 1D E3 49 5F " +
            "44 86 86 E5 14 A5 52 29 81 CC 3A 8A 88 80 E2 13 " +
            "A0 4A 47 78 CB ED 3E DD E9 6F EE FB 0A 9F 39 4B " +
            "C3 E1 64 FD 8D 21 1F F2 72 27 B0 E3 E9 24 3E 7A " +
            "45 A7 79 62 9C C7 0C A7 8E EB 1A 58 38 46 AE B3 " +
            "C5 55 49 51 BE 21 D3 CA 4F 74 4F 45 FD 5F 68 70 " +
            "38 C8 07 47 4F E7 E6 CF 31 EB 22 18 81 F5 F4 5D " +
            "A9 0C 28 DF 44 71 B3 CC",
            // After Iota 
            "05 A9 4E 30 72 CF 4B B6 F3 80 BB 0D 11 0F D6 42 " +
            "29 73 CE B4 3D 56 5D 3D 9C 94 56 33 5A 85 AF 40 " +
            "13 60 CD 07 A6 61 17 64 6E 21 B3 93 8F 90 FB E2 " +
            "7F 64 1F EE 67 E9 C0 46 AC EA 3B 2A 9C 18 EF C8 " +
            "31 8A 75 4D EE 70 9E BA 3C 26 F5 B9 DA 39 51 8E " +
            "DA 7F DC AE 0F 9F 28 D1 8E 4C 04 34 1D E3 49 5F " +
            "44 86 86 E5 14 A5 52 29 81 CC 3A 8A 88 80 E2 13 " +
            "A0 4A 47 78 CB ED 3E DD E9 6F EE FB 0A 9F 39 4B " +
            "C3 E1 64 FD 8D 21 1F F2 72 27 B0 E3 E9 24 3E 7A " +
            "45 A7 79 62 9C C7 0C A7 8E EB 1A 58 38 46 AE B3 " +
            "C5 55 49 51 BE 21 D3 CA 4F 74 4F 45 FD 5F 68 70 " +
            "38 C8 07 47 4F E7 E6 CF 31 EB 22 18 81 F5 F4 5D " +
            "A9 0C 28 DF 44 71 B3 CC",
            // Round #15
            // After Theta
            "B0 39 15 AE 8F BB 7E 4C 78 AC B4 15 70 21 D4 94 " +
            "17 72 C0 C7 65 A3 23 C2 47 B3 09 6E DF A8 5C B9 " +
            "71 65 82 C6 0A DA D8 7F DB B1 E8 0D 72 E4 CE 18 " +
            "F4 48 10 F6 06 C7 C2 90 92 EB 35 59 C4 ED 91 37 " +
            "EA AD 2A 10 6B 5D 6D 43 5E 23 BA 78 76 82 9E 95 " +
            "6F EF 87 30 F2 EB 1D 2B 05 60 0B 2C 7C CD 4B 89 " +
            "7A 87 88 96 4C 50 2C D6 5A EB 65 D7 0D AD 11 EA " +
            "C2 4F 08 B9 67 56 F1 C6 5C FF B5 65 F7 EB 0C B1 " +
            "48 CD 6B E5 EC 0F 1D 24 4C 26 BE 90 B1 D1 40 85 " +
            "9E 80 26 3F 19 EA FF 5E EC EE 55 99 94 FD 61 A8 " +
            "70 C5 12 CF 43 55 E6 30 C4 58 40 5D 9C 71 6A A6 " +
            "06 C9 09 34 17 12 98 30 EA CC 7D 45 04 D8 07 A4 " +
            "CB 09 67 1E E8 CA 7C D7",
            // After Rho
            "B0 39 15 AE 8F BB 7E 4C F1 58 69 2B E0 42 A8 29 " +
            "85 1C F0 71 D9 E8 88 F0 8D CA 95 7B 34 9B E0 F6 " +
            "D0 C6 FE 8B 2B 13 34 56 20 47 EE 8C B1 1D 8B DE " +
            "61 6F 70 2C 0C 49 8F 04 8D E4 7A 4D 16 71 7B E4 " +
            "56 15 88 B5 AE B6 21 F5 E8 59 E9 35 A2 8B 67 27 " +
            "79 7B 3F 84 91 5F EF 58 25 16 80 2D B0 F0 35 2F " +
            "B4 64 82 62 B1 D6 3B 44 5A 23 D4 B5 D6 CB AE 1B " +
            "DC 33 AB 78 63 E1 27 84 CB EE D7 19 62 B9 FE 6B " +
            "AD 9C FD A1 83 04 A9 79 A0 42 26 13 5F C8 D8 68 " +
            "FD DF CB 13 D0 E4 27 43 A8 EC EE 55 99 94 FD 61 " +
            "99 C3 C0 15 4B 3C 0F 55 12 63 01 75 71 C6 A9 99 " +
            "20 39 81 E6 42 02 13 C6 CC 7D 45 04 D8 07 A4 EA " +
            "DF F5 72 C2 99 07 BA 32",
            // After Pi
            "B0 39 15 AE 8F BB 7E 4C 61 6F 70 2C 0C 49 8F 04 " +
            "B4 64 82 62 B1 D6 3B 44 FD DF CB 13 D0 E4 27 43 " +
            "DF F5 72 C2 99 07 BA 32 8D CA 95 7B 34 9B E0 F6 " +
            "E8 59 E9 35 A2 8B 67 27 79 7B 3F 84 91 5F EF 58 " +
            "AD 9C FD A1 83 04 A9 79 20 39 81 E6 42 02 13 C6 " +
            "F1 58 69 2B E0 42 A8 29 8D E4 7A 4D 16 71 7B E4 " +
            "5A 23 D4 B5 D6 CB AE 1B A8 EC EE 55 99 94 FD 61 " +
            "99 C3 C0 15 4B 3C 0F 55 D0 C6 FE 8B 2B 13 34 56 " +
            "20 47 EE 8C B1 1D 8B DE 25 16 80 2D B0 F0 35 2F " +
            "A0 42 26 13 5F C8 D8 68 CC 7D 45 04 D8 07 A4 EA " +
            "85 1C F0 71 D9 E8 88 F0 56 15 88 B5 AE B6 21 F5 " +
            "DC 33 AB 78 63 E1 27 84 CB EE D7 19 62 B9 FE 6B " +
            "12 63 01 75 71 C6 A9 99",
            // After Chi
            "24 39 97 EC 3E 2D 4E 0C 28 F4 39 3D 4C 69 8B 07 " +
            "B6 44 B2 A2 B8 D5 A3 74 DD D7 CE 3F D6 5C 63 0F " +
            "9E B3 12 C2 99 47 3B 32 9C E8 83 FB 25 CF 68 AE " +
            "6C DD 29 14 A0 8B 67 06 79 5A 3F C2 D1 5D FD DE " +
            "20 5E E9 B8 B7 9D 49 49 40 28 E9 E2 C0 02 14 C7 " +
            "A3 5B ED 9B 20 C8 2C 32 2D 28 50 0D 1F 65 2A 84 " +
            "4B 20 D4 B5 94 E3 AC 0F C8 F4 C7 7F 39 D6 5D 49 " +
            "95 67 D2 51 5D 0D 5C 91 D5 D6 FE AA 2B F3 00 77 " +
            "A0 07 C8 9E FE 15 43 9E 69 2B C1 29 30 F7 11 AD " +
            "B0 C0 9C 98 7C D8 C8 7C EC 7C 45 00 48 0B 2F 62 " +
            "0D 3E D3 39 98 A9 8E F0 55 D9 DC B4 AE AE F9 9E " +
            "CC 32 AB 1C 72 A7 26 14 4E F2 27 19 EA 91 FE 0B " +
            "40 62 09 F1 57 D0 88 9C",
            // After Iota 
            "27 B9 97 EC 3E 2D 4E 8C 28 F4 39 3D 4C 69 8B 07 " +
            "B6 44 B2 A2 B8 D5 A3 74 DD D7 CE 3F D6 5C 63 0F " +
            "9E B3 12 C2 99 47 3B 32 9C E8 83 FB 25 CF 68 AE " +
            "6C DD 29 14 A0 8B 67 06 79 5A 3F C2 D1 5D FD DE " +
            "20 5E E9 B8 B7 9D 49 49 40 28 E9 E2 C0 02 14 C7 " +
            "A3 5B ED 9B 20 C8 2C 32 2D 28 50 0D 1F 65 2A 84 " +
            "4B 20 D4 B5 94 E3 AC 0F C8 F4 C7 7F 39 D6 5D 49 " +
            "95 67 D2 51 5D 0D 5C 91 D5 D6 FE AA 2B F3 00 77 " +
            "A0 07 C8 9E FE 15 43 9E 69 2B C1 29 30 F7 11 AD " +
            "B0 C0 9C 98 7C D8 C8 7C EC 7C 45 00 48 0B 2F 62 " +
            "0D 3E D3 39 98 A9 8E F0 55 D9 DC B4 AE AE F9 9E " +
            "CC 32 AB 1C 72 A7 26 14 4E F2 27 19 EA 91 FE 0B " +
            "40 62 09 F1 57 D0 88 9C",
            // Round #16
            // After Theta
            "F9 E4 5B 70 63 C7 62 1C AA 58 8B E2 BB 6E 85 A9 " +
            "BC 04 50 5E 87 54 5D 01 33 35 36 DF 5E 41 0F 26 " +
            "D4 39 E0 84 47 F8 72 65 42 B5 4F 67 78 25 44 3E " +
            "EE 71 9B CB 57 8C 69 A8 73 1A DD 3E EE DC 03 AB " +
            "CE BC 11 58 3F 80 25 60 0A A2 1B A4 1E BD 5D 90 " +
            "7D 06 21 07 7D 22 00 A2 AF 84 E2 D2 E8 62 24 2A " +
            "41 60 36 49 AB 62 52 7A 26 16 3F 9F B1 CB 31 60 " +
            "DF ED 20 17 83 B2 15 C6 0B 8B 32 36 76 19 2C E7 " +
            "22 AB 7A 41 09 12 4D 30 63 6B 23 D5 0F 76 EF D8 " +
            "5E 22 64 78 F4 C5 A4 55 A6 F6 B7 46 96 B4 66 35 " +
            "D3 63 1F A5 C5 43 A2 60 D7 75 6E 6B 59 A9 F7 30 " +
            "C6 72 49 E0 4D 26 D8 61 A0 10 DF F9 62 8C 92 22 " +
            "0A E8 FB B7 89 6F C1 CB",
            // After Rho
            "F9 E4 5B 70 63 C7 62 1C 55 B1 16 C5 77 DD 0A 53 " +
            "2F 01 94 D7 21 55 57 00 15 F4 60 32 53 63 F3 ED " +
            "C2 97 2B A3 CE 01 27 3C 86 57 42 E4 23 54 FB 74 " +
            "B9 7C C5 98 86 EA 1E B7 EA 9C 46 B7 8F 3B F7 C0 " +
            "DE 08 AC 1F C0 12 30 67 DB 05 A9 20 BA 41 EA D1 " +
            "ED 33 08 39 E8 13 01 10 A8 BC 12 8A 4B A3 8B 91 " +
            "49 5A 15 93 D2 0B 02 B3 97 63 C0 4C 2C 7E 3E 63 " +
            "8B 41 D9 0A E3 EF 76 90 6C EC 32 58 CE 17 16 65 " +
            "2F 28 41 A2 09 46 64 55 77 EC B1 B5 91 EA 07 BB " +
            "98 B4 CA 4B 84 0C 8F BE 35 A6 F6 B7 46 96 B4 66 " +
            "89 82 4D 8F 7D 94 16 0F 5C D7 B9 AD 65 A5 DE C3 " +
            "58 2E 09 BC C9 04 3B CC 10 DF F9 62 8C 92 22 A0 " +
            "F0 B2 02 FA FE 6D E2 5B",
            // After Pi
            "F9 E4 5B 70 63 C7 62 1C B9 7C C5 98 86 EA 1E B7 " +
            "49 5A 15 93 D2 0B 02 B3 98 B4 CA 4B 84 0C 8F BE " +
            "F0 B2 02 FA FE 6D E2 5B 15 F4 60 32 53 63 F3 ED " +
            "DB 05 A9 20 BA 41 EA D1 ED 33 08 39 E8 13 01 10 " +
            "2F 28 41 A2 09 46 64 55 58 2E 09 BC C9 04 3B CC " +
            "55 B1 16 C5 77 DD 0A 53 EA 9C 46 B7 8F 3B F7 C0 " +
            "97 63 C0 4C 2C 7E 3E 63 35 A6 F6 B7 46 96 B4 66 " +
            "89 82 4D 8F 7D 94 16 0F C2 97 2B A3 CE 01 27 3C " +
            "86 57 42 E4 23 54 FB 74 A8 BC 12 8A 4B A3 8B 91 " +
            "77 EC B1 B5 91 EA 07 BB 10 DF F9 62 8C 92 22 A0 " +
            "2F 01 94 D7 21 55 57 00 DE 08 AC 1F C0 12 30 67 " +
            "8B 41 D9 0A E3 EF 76 90 6C EC 32 58 CE 17 16 65 " +
            "5C D7 B9 AD 65 A5 DE C3",
            // After Chi
            "B9 E6 4B 73 33 C6 62 1C 29 D8 0F D0 82 EE 93 BB " +
            "29 58 15 23 A8 6A 62 F2 91 F0 93 4B 85 8E 8F BA " +
            "F0 AA 86 72 7A 45 FE F8 31 C6 60 2B 13 71 F2 ED " +
            "D9 0D E8 A2 BB 05 8E 94 BD 35 00 25 28 13 1A 98 " +
            "2A F8 21 A0 1B 25 A4 74 92 2F 80 BC 61 04 33 DC " +
            "40 D2 96 8D 57 99 02 70 CA 18 70 04 CD BB 77 C4 " +
            "1F 63 C9 44 15 7E 3C 6A 61 97 E4 F7 44 DF BC 36 " +
            "23 8E 0D BD F5 B6 E3 8F EA 3F 3B A9 86 A2 27 BD " +
            "D1 17 E3 D1 B3 1C FF 5E A8 AF 5A C8 47 B3 AB 91 " +
            "B5 EC B3 34 D3 EB 02 A7 14 9F B9 26 AD C6 FA E0 " +
            "2E 40 C5 D7 02 B8 11 90 BA A4 8E 4F CC 02 30 02 " +
            "9B 52 50 AF C2 4F BE 12 4F EC 36 0A CE 47 17 65 " +
            "8C DF 91 A5 A5 A7 FE A4",
            // After Iota 
            "BB 66 4B 73 33 C6 62 9C 29 D8 0F D0 82 EE 93 BB " +
            "29 58 15 23 A8 6A 62 F2 91 F0 93 4B 85 8E 8F BA " +
            "F0 AA 86 72 7A 45 FE F8 31 C6 60 2B 13 71 F2 ED " +
            "D9 0D E8 A2 BB 05 8E 94 BD 35 00 25 28 13 1A 98 " +
            "2A F8 21 A0 1B 25 A4 74 92 2F 80 BC 61 04 33 DC " +
            "40 D2 96 8D 57 99 02 70 CA 18 70 04 CD BB 77 C4 " +
            "1F 63 C9 44 15 7E 3C 6A 61 97 E4 F7 44 DF BC 36 " +
            "23 8E 0D BD F5 B6 E3 8F EA 3F 3B A9 86 A2 27 BD " +
            "D1 17 E3 D1 B3 1C FF 5E A8 AF 5A C8 47 B3 AB 91 " +
            "B5 EC B3 34 D3 EB 02 A7 14 9F B9 26 AD C6 FA E0 " +
            "2E 40 C5 D7 02 B8 11 90 BA A4 8E 4F CC 02 30 02 " +
            "9B 52 50 AF C2 4F BE 12 4F EC 36 0A CE 47 17 65 " +
            "8C DF 91 A5 A5 A7 FE A4",
            // Round #17
            // After Theta
            "C1 D1 9C 52 C2 CD 02 1C 56 32 E1 30 51 2C 94 91 " +
            "38 18 48 8E AD 95 C2 30 9A 94 03 8E 58 58 8B E7 " +
            "CC 2F D3 06 5A F4 34 9B 4B 71 B7 0A E2 7A 92 6D " +
            "A6 E7 06 42 68 C7 89 BE AC 75 5D 88 2D EC BA 5A " +
            "21 9C B1 65 C6 F3 A0 29 AE AA D5 C8 41 B5 F9 BF " +
            "3A 65 41 AC A6 92 62 F0 B5 F2 9E E4 1E 79 70 EE " +
            "0E 23 94 E9 10 81 9C A8 6A F3 74 32 99 09 B8 6B " +
            "1F 0B 58 C9 D5 07 29 EC 90 88 EC 88 77 A9 47 3D " +
            "AE FD 0D 31 60 DE F8 74 B9 EF 07 65 42 4C 0B 53 " +
            "BE 88 23 F1 0E 3D 06 FA 28 1A EC 52 8D 77 30 83 " +
            "54 F7 12 F6 F3 B3 71 10 C5 4E 60 AF 1F C0 37 28 " +
            "8A 12 0D 02 C7 B0 1E D0 44 88 A6 CF 13 91 13 38 " +
            "B0 5A C4 D1 85 16 34 C7",
            // After Rho
            "C1 D1 9C 52 C2 CD 02 1C AD 64 C2 61 A2 58 28 23 " +
            "0E 06 92 63 6B A5 30 0C 85 B5 78 AE 49 39 E0 88 " +
            "A2 A7 D9 64 7E 99 36 D0 20 AE 27 D9 B6 14 77 AB " +
            "20 84 76 9C E8 6B 7A 6E 16 6B 5D 17 62 0B BB AE " +
            "CE D8 32 E3 79 D0 94 10 9B FF EB AA 5A 8D 1C 54 " +
            "D7 29 0B 62 35 95 14 83 B9 D7 CA 7B 92 7B E4 C1 " +
            "4C 87 08 E4 44 75 18 A1 13 70 D7 D4 E6 E9 64 32 " +
            "E4 EA 83 14 F6 8F 05 AC 11 EF 52 8F 7A 20 11 D9 " +
            "21 06 CC 1B 9F CE B5 BF 85 A9 DC F7 83 32 21 A6 " +
            "C7 40 DF 17 71 24 DE A1 83 28 1A EC 52 8D 77 30 " +
            "C6 41 50 DD 4B D8 CF CF 14 3B 81 BD 7E 00 DF A0 " +
            "51 A2 41 E0 18 D6 03 5A 88 A6 CF 13 91 13 38 44 " +
            "CD 31 AC 16 71 74 A1 05",
            // After Pi
            "C1 D1 9C 52 C2 CD 02 1C 20 84 76 9C E8 6B 7A 6E " +
            "4C 87 08 E4 44 75 18 A1 C7 40 DF 17 71 24 DE A1 " +
            "CD 31 AC 16 71 74 A1 05 85 B5 78 AE 49 39 E0 88 " +
            "9B FF EB AA 5A 8D 1C 54 D7 29 0B 62 35 95 14 83 " +
            "21 06 CC 1B 9F CE B5 BF 51 A2 41 E0 18 D6 03 5A " +
            "AD 64 C2 61 A2 58 28 23 16 6B 5D 17 62 0B BB AE " +
            "13 70 D7 D4 E6 E9 64 32 83 28 1A EC 52 8D 77 30 " +
            "C6 41 50 DD 4B D8 CF CF A2 A7 D9 64 7E 99 36 D0 " +
            "20 AE 27 D9 B6 14 77 AB B9 D7 CA 7B 92 7B E4 C1 " +
            "85 A9 DC F7 83 32 21 A6 88 A6 CF 13 91 13 38 44 " +
            "0E 06 92 63 6B A5 30 0C CE D8 32 E3 79 D0 94 10 " +
            "E4 EA 83 14 F6 8F 05 AC 11 EF 52 8F 7A 20 11 D9 " +
            "14 3B 81 BD 7E 00 DF A0",
            // After Chi
            "8D D2 94 32 C6 D9 02 9D A3 C4 A1 8F D9 6B BC 6E " +
            "44 B6 28 E4 44 25 39 A5 C7 80 CF 57 F3 AD DC B9 " +
            "ED 35 CE 9A 59 56 D9 67 C1 B5 78 EE 6C 29 E0 0B " +
            "BB F9 2F B3 D0 C7 BD 68 87 89 0A 82 35 85 16 C3 " +
            "A5 13 F4 15 DE E7 55 3F 4B E8 C2 E0 0A 52 1F 0E " +
            "AC 74 40 A1 26 B8 6C 33 96 63 55 3F 72 0F A8 AE " +
            "57 31 97 C5 EF B9 EC FD AA 0C 98 CC F2 8D 57 10 " +
            "D4 4A 4D CB 0B DB 5C 43 3B F6 11 46 7E F2 B6 90 " +
            "24 86 33 5D B7 14 76 8D B1 D1 C9 7B 82 7A FC 81 " +
            "A7 A8 CC 93 ED BA 27 36 88 AE E9 8A 11 17 79 6F " +
            "2E 24 13 77 ED AA 31 A0 DF DD 62 68 71 F0 84 41 " +
            "E0 FA 02 24 F2 8F CB 8C 1B EB 40 CD 7B 85 31 D5 " +
            "D4 E3 A1 3D 6E 50 5B B0",
            // After Iota 
            "0D D2 94 32 C6 D9 02 1D A3 C4 A1 8F D9 6B BC 6E " +
            "44 B6 28 E4 44 25 39 A5 C7 80 CF 57 F3 AD DC B9 " +
            "ED 35 CE 9A 59 56 D9 67 C1 B5 78 EE 6C 29 E0 0B " +
            "BB F9 2F B3 D0 C7 BD 68 87 89 0A 82 35 85 16 C3 " +
            "A5 13 F4 15 DE E7 55 3F 4B E8 C2 E0 0A 52 1F 0E " +
            "AC 74 40 A1 26 B8 6C 33 96 63 55 3F 72 0F A8 AE " +
            "57 31 97 C5 EF B9 EC FD AA 0C 98 CC F2 8D 57 10 " +
            "D4 4A 4D CB 0B DB 5C 43 3B F6 11 46 7E F2 B6 90 " +
            "24 86 33 5D B7 14 76 8D B1 D1 C9 7B 82 7A FC 81 " +
            "A7 A8 CC 93 ED BA 27 36 88 AE E9 8A 11 17 79 6F " +
            "2E 24 13 77 ED AA 31 A0 DF DD 62 68 71 F0 84 41 " +
            "E0 FA 02 24 F2 8F CB 8C 1B EB 40 CD 7B 85 31 D5 " +
            "D4 E3 A1 3D 6E 50 5B B0",
            // Round #18
            // After Theta
            "C9 02 89 59 9B CE 0C 20 5D 4E F3 3B 1B A2 5C 56 " +
            "D9 0B FD 72 6A 92 F3 2A 5F 11 A2 A7 53 71 59 C4 " +
            "73 6B BC D3 2E 8E 03 38 05 65 65 85 31 3E EE 36 " +
            "45 73 7D 07 12 0E 5D 50 1A 34 DF 14 1B 32 DC 4C " +
            "3D 82 99 E5 7E 3B D0 42 D5 B6 B0 A9 7D 8A C5 51 " +
            "68 A4 5D CA 7B AF 62 0E 68 E9 07 8B B0 C6 48 96 " +
            "CA 8C 42 53 C1 0E 26 72 32 9D F5 3C 52 51 D2 6D " +
            "4A 14 3F 82 7C 03 86 1C FF 26 0C 2D 23 E5 B8 AD " +
            "DA 0C 61 E9 75 DD 96 B5 2C 6C 1C ED AC CD 36 0E " +
            "3F 39 A1 63 4D 66 A2 4B 16 F0 9B C3 66 CF A3 30 " +
            "EA F4 0E 1C B0 BD 3F 9D 21 57 30 DC B3 39 64 79 " +
            "7D 47 D7 B2 DC 38 01 03 83 7A 2D 3D DB 59 B4 A8 " +
            "4A BD D3 74 19 88 81 EF",
            // After Rho
            "C9 02 89 59 9B CE 0C 20 BA 9C E6 77 36 44 B9 AC " +
            "F6 42 BF 9C 9A E4 BC 4A 15 97 45 FC 15 21 7A 3A " +
            "71 1C C0 99 5B E3 9D 76 18 E3 E3 6E 53 50 56 56 " +
            "77 20 E1 D0 05 55 34 D7 93 06 CD 37 C5 86 0C 37 " +
            "C1 CC 72 BF 1D 68 A1 1E 58 1C 55 6D 0B 9B DA A7 " +
            "40 23 ED 52 DE 7B 15 73 59 A2 A5 1F 2C C2 1A 23 " +
            "9A 0A 76 30 91 53 66 14 A2 A4 DB 64 3A EB 79 A4 " +
            "41 BE 01 43 0E 25 8A 1F 5A 46 CA 71 5B FF 4D 18 " +
            "2C BD AE DB B2 56 9B 21 1B 07 16 36 8E 76 D6 66 " +
            "4C 74 E9 27 27 74 AC C9 30 16 F0 9B C3 66 CF A3 " +
            "FE 74 AA D3 3B 70 C0 F6 85 5C C1 70 CF E6 90 E5 " +
            "EF E8 5A 96 1B 27 60 A0 7A 2D 3D DB 59 B4 A8 83 " +
            "E0 BB 52 EF 34 5D 06 62",
            // After Pi
            "C9 02 89 59 9B CE 0C 20 77 20 E1 D0 05 55 34 D7 " +
            "9A 0A 76 30 91 53 66 14 4C 74 E9 27 27 74 AC C9 " +
            "E0 BB 52 EF 34 5D 06 62 15 97 45 FC 15 21 7A 3A " +
            "58 1C 55 6D 0B 9B DA A7 40 23 ED 52 DE 7B 15 73 " +
            "2C BD AE DB B2 56 9B 21 EF E8 5A 96 1B 27 60 A0 " +
            "BA 9C E6 77 36 44 B9 AC 93 06 CD 37 C5 86 0C 37 " +
            "A2 A4 DB 64 3A EB 79 A4 30 16 F0 9B C3 66 CF A3 " +
            "FE 74 AA D3 3B 70 C0 F6 71 1C C0 99 5B E3 9D 76 " +
            "18 E3 E3 6E 53 50 56 56 59 A2 A5 1F 2C C2 1A 23 " +
            "1B 07 16 36 8E 76 D6 66 7A 2D 3D DB 59 B4 A8 83 " +
            "F6 42 BF 9C 9A E4 BC 4A C1 CC 72 BF 1D 68 A1 1E " +
            "41 BE 01 43 0E 25 8A 1F 5A 46 CA 71 5B FF 4D 18 " +
            "85 5C C1 70 CF E6 90 E5",
            // After Chi
            "41 08 9F 79 0B CC 4E 20 33 54 68 D7 23 71 BC 1E " +
            "3A 81 64 F8 81 5A 64 36 45 74 60 37 AC F6 A4 C9 " +
            "D6 9B 32 6F 30 4C 36 B5 15 B4 ED EE C1 41 7F 6A " +
            "74 80 57 E4 2B 9F 50 A7 83 63 BD 56 D7 5A 75 F3 " +
            "3C AA AB B3 B6 56 81 3B A7 E0 4A 97 11 BD E0 25 " +
            "9A 3C F4 37 0C 2D C8 2C 83 14 ED AC 04 82 8A 34 " +
            "6C C4 D1 24 02 FB 79 F0 30 9E B4 BF C7 62 F6 AB " +
            "FF 76 A3 D3 FA F2 C4 E5 30 1C C4 88 77 61 95 57 " +
            "1A E6 F1 4E D1 64 92 12 39 8A 8C D6 7D 42 32 A2 " +
            "1A 17 D6 36 8C 35 C3 12 72 CE 1E BD 59 A4 EA 83 " +
            "F6 70 BE DC 98 E1 B6 4B DB 8C B8 8F 4C B2 E4 1E " +
            "C4 A6 00 43 8A 25 1A FA 28 44 F4 FD 4B FF 61 12 " +
            "84 D0 81 53 CA EE 91 F1",
            // After Iota 
            "4B 88 9F 79 0B CC 4E 20 33 54 68 D7 23 71 BC 1E " +
            "3A 81 64 F8 81 5A 64 36 45 74 60 37 AC F6 A4 C9 " +
            "D6 9B 32 6F 30 4C 36 B5 15 B4 ED EE C1 41 7F 6A " +
            "74 80 57 E4 2B 9F 50 A7 83 63 BD 56 D7 5A 75 F3 " +
            "3C AA AB B3 B6 56 81 3B A7 E0 4A 97 11 BD E0 25 " +
            "9A 3C F4 37 0C 2D C8 2C 83 14 ED AC 04 82 8A 34 " +
            "6C C4 D1 24 02 FB 79 F0 30 9E B4 BF C7 62 F6 AB " +
            "FF 76 A3 D3 FA F2 C4 E5 30 1C C4 88 77 61 95 57 " +
            "1A E6 F1 4E D1 64 92 12 39 8A 8C D6 7D 42 32 A2 " +
            "1A 17 D6 36 8C 35 C3 12 72 CE 1E BD 59 A4 EA 83 " +
            "F6 70 BE DC 98 E1 B6 4B DB 8C B8 8F 4C B2 E4 1E " +
            "C4 A6 00 43 8A 25 1A FA 28 44 F4 FD 4B FF 61 12 " +
            "84 D0 81 53 CA EE 91 F1",
            // Round #19
            // After Theta
            "38 CF EC 01 61 F0 06 25 61 2C 9C 1C 4C 68 E7 BE " +
            "C9 0D 45 46 25 F0 96 05 9D 58 6C A2 9E F8 36 AA " +
            "A9 50 97 76 79 04 F3 19 66 F3 9E 96 AB 7D 37 6F " +
            "26 F8 A3 2F 44 86 0B 07 70 EF 9C E8 73 F0 87 C0 " +
            "E4 86 A7 26 84 58 13 58 D8 2B EF 8E 58 F5 25 89 " +
            "E9 7B 87 4F 66 11 80 29 D1 6C 19 67 6B 9B D1 94 " +
            "9F 48 F0 9A A6 51 8B C3 E8 B2 B8 2A F5 6C 64 C8 " +
            "80 BD 06 CA B3 BA 01 49 43 5B B7 F0 1D 5D DD 52 " +
            "48 9E 05 85 BE 7D C9 B2 CA 06 AD 68 D9 E8 C0 91 " +
            "C2 3B DA A3 BE 3B 51 71 0D 05 BB A4 10 EC 2F 2F " +
            "85 37 CD A4 F2 DD FE 4E 89 F4 4C 44 23 AB BF BE " +
            "37 2A 21 FD 2E 8F E8 C9 F0 68 F8 68 79 F1 F3 71 " +
            "FB 1B 24 4A 83 A6 54 5D",
            // After Rho
            "38 CF EC 01 61 F0 06 25 C3 58 38 39 98 D0 CE 7D " +
            "72 43 91 51 09 BC 65 41 89 6F A3 DA 89 C5 26 EA " +
            "23 98 CF 48 85 BA B4 CB B9 DA 77 F3 66 36 EF 69 " +
            "FA 42 64 B8 70 60 82 3F 30 DC 3B 27 FA 1C FC 21 " +
            "C3 53 13 42 AC 09 2C 72 5F 92 88 BD F2 EE 88 55 " +
            "49 DF 3B 7C 32 8B 00 4C 53 46 B3 65 9C AD 6D 46 " +
            "D7 34 8D 5A 1C FE 44 82 D9 C8 90 D1 65 71 55 EA " +
            "E5 59 DD 80 24 C0 5E 03 E1 3B BA BA A5 86 B6 6E " +
            "A0 D0 B7 2F 59 16 C9 B3 E0 48 65 83 56 B4 6C 74 " +
            "27 2A 4E 78 47 7B D4 77 2F 0D 05 BB A4 10 EC 2F " +
            "FB 3B 15 DE 34 93 CA 77 26 D2 33 11 8D AC FE FA " +
            "46 25 A4 DF E5 11 3D F9 68 F8 68 79 F1 F3 71 F0 " +
            "55 D7 FE 06 89 D2 A0 29",
            // After Pi
            "38 CF EC 01 61 F0 06 25 FA 42 64 B8 70 60 82 3F " +
            "D7 34 8D 5A 1C FE 44 82 27 2A 4E 78 47 7B D4 77 " +
            "55 D7 FE 06 89 D2 A0 29 89 6F A3 DA 89 C5 26 EA " +
            "5F 92 88 BD F2 EE 88 55 49 DF 3B 7C 32 8B 00 4C " +
            "A0 D0 B7 2F 59 16 C9 B3 46 25 A4 DF E5 11 3D F9 " +
            "C3 58 38 39 98 D0 CE 7D 30 DC 3B 27 FA 1C FC 21 " +
            "D9 C8 90 D1 65 71 55 EA 2F 0D 05 BB A4 10 EC 2F " +
            "FB 3B 15 DE 34 93 CA 77 23 98 CF 48 85 BA B4 CB " +
            "B9 DA 77 F3 66 36 EF 69 53 46 B3 65 9C AD 6D 46 " +
            "E0 48 65 83 56 B4 6C 74 68 F8 68 79 F1 F3 71 F0 " +
            "72 43 91 51 09 BC 65 41 C3 53 13 42 AC 09 2C 72 " +
            "E5 59 DD 80 24 C0 5E 03 E1 3B BA BA A5 86 B6 6E " +
            "26 D2 33 11 8D AC FE FA",
            // After Chi
            "3D FB 65 43 6D 6E 42 A5 DA 48 26 98 33 61 12 4A " +
            "87 E1 3D 5C 94 7E 64 8A 0F 22 4E 79 27 5B D2 73 " +
            "97 D7 FE BE 99 D2 20 33 89 22 90 9A 89 C4 26 E2 " +
            "FF 92 0C BE BB FA 41 E6 0F FA 3B AC 96 8A 34 04 " +
            "29 9A B4 2F 51 D2 CB B1 10 B5 AC FA 97 3B B5 EC " +
            "0A 58 B8 E9 9D B1 CF B7 16 D9 3E 0D 7A 1C 54 24 " +
            "09 FA 80 95 75 F2 57 BA 2F 4D 2D 9A 2C 50 E8 27 " +
            "CB BF 16 D8 56 9F FA 77 61 9C 4F 4C 1D 33 B4 CD " +
            "19 D2 33 71 24 26 EF 59 5B F6 BB 1D 3D EE 7C C6 " +
            "E3 48 E2 83 52 BC E8 7F F0 BA 58 CA 93 F7 3A D0 " +
            "56 4B 5D D1 09 7C 37 40 C3 71 31 78 2D 0F 8C 1E " +
            "E3 99 DC 81 2C E8 16 93 B1 3A 3A FA A5 96 B7 6F " +
            "A7 C2 31 13 29 AD F6 C8",
            // After Iota 
            "37 FB 65 C3 6D 6E 42 25 DA 48 26 98 33 61 12 4A " +
            "87 E1 3D 5C 94 7E 64 8A 0F 22 4E 79 27 5B D2 73 " +
            "97 D7 FE BE 99 D2 20 33 89 22 90 9A 89 C4 26 E2 " +
            "FF 92 0C BE BB FA 41 E6 0F FA 3B AC 96 8A 34 04 " +
            "29 9A B4 2F 51 D2 CB B1 10 B5 AC FA 97 3B B5 EC " +
            "0A 58 B8 E9 9D B1 CF B7 16 D9 3E 0D 7A 1C 54 24 " +
            "09 FA 80 95 75 F2 57 BA 2F 4D 2D 9A 2C 50 E8 27 " +
            "CB BF 16 D8 56 9F FA 77 61 9C 4F 4C 1D 33 B4 CD " +
            "19 D2 33 71 24 26 EF 59 5B F6 BB 1D 3D EE 7C C6 " +
            "E3 48 E2 83 52 BC E8 7F F0 BA 58 CA 93 F7 3A D0 " +
            "56 4B 5D D1 09 7C 37 40 C3 71 31 78 2D 0F 8C 1E " +
            "E3 99 DC 81 2C E8 16 93 B1 3A 3A FA A5 96 B7 6F " +
            "A7 C2 31 13 29 AD F6 C8",
            // Round #20
            // After Theta
            "FF 1F 65 C2 79 1F 28 0B 2B 02 BA 46 93 35 E0 75 " +
            "D9 4F 34 14 34 37 5D AE 01 E6 F4 0A 85 02 F9 73 " +
            "CB FD 4F 51 EE 89 DE 3C 41 C6 90 9B 9D B5 4C CC " +
            "0E D8 90 60 1B AE B3 D9 51 54 32 E4 36 C3 0D 20 " +
            "27 5E 0E 5C F3 8B E0 B1 4C 9F 1D 15 E0 60 4B E3 " +
            "C2 BC B8 E8 89 C0 A5 99 E7 93 A2 D3 DA 48 A6 1B " +
            "57 54 89 DD D5 BB 6E 9E 21 89 97 E9 8E 09 C3 27 " +
            "97 95 A7 37 21 C4 04 78 A9 78 4F 4D 09 42 DE E3 " +
            "E8 98 AF AF 84 72 1D 66 05 58 B2 55 9D A7 45 E2 " +
            "ED 8C 58 F0 F0 E5 C3 7F AC 90 E9 25 E4 AC C4 DF " +
            "9E AF 5D D0 1D 0D 5D 6E 32 3B AD A6 8D 5B 7E 21 " +
            "BD 37 D5 C9 8C A1 2F B7 BF FE 80 89 07 CF 9C 6F " +
            "FB E8 80 FC 5E F6 08 C7",
            // After Rho
            "FF 1F 65 C2 79 1F 28 0B 56 04 74 8D 26 6B C0 EB " +
            "F6 13 0D 05 CD 4D 97 6B 28 90 3F 17 60 4E AF 50 " +
            "4F F4 E6 59 EE 7F 8A 72 D9 59 CB C4 1C 64 0C B9 " +
            "09 B6 E1 3A 9B ED 80 0D 48 14 95 0C B9 CD 70 03 " +
            "2F 07 AE F9 45 F0 D8 13 B6 34 CE F4 D9 51 01 0E " +
            "14 E6 C5 45 4F 04 2E CD 6E 9C 4F 8A 4E 6B 23 99 " +
            "EC AE DE 75 F3 BC A2 4A 13 86 4F 42 12 2F D3 1D " +
            "9B 10 62 02 BC CB CA D3 9A 12 84 BC C7 53 F1 9E " +
            "F5 95 50 AE C3 0C 1D F3 22 F1 02 2C D9 AA CE D3 " +
            "7C F8 AF 9D 11 0B 1E BE DF AC 90 E9 25 E4 AC C4 " +
            "74 B9 79 BE 76 41 77 34 C8 EC B4 9A 36 6E F9 85 " +
            "F7 A6 3A 99 31 F4 E5 B6 FE 80 89 07 CF 9C 6F BF " +
            "C2 F1 3E 3A 20 BF 97 3D",
            // After Pi
            "FF 1F 65 C2 79 1F 28 0B 09 B6 E1 3A 9B ED 80 0D " +
            "EC AE DE 75 F3 BC A2 4A 7C F8 AF 9D 11 0B 1E BE " +
            "C2 F1 3E 3A 20 BF 97 3D 28 90 3F 17 60 4E AF 50 " +
            "B6 34 CE F4 D9 51 01 0E 14 E6 C5 45 4F 04 2E CD " +
            "F5 95 50 AE C3 0C 1D F3 F7 A6 3A 99 31 F4 E5 B6 " +
            "56 04 74 8D 26 6B C0 EB 48 14 95 0C B9 CD 70 03 " +
            "13 86 4F 42 12 2F D3 1D DF AC 90 E9 25 E4 AC C4 " +
            "74 B9 79 BE 76 41 77 34 4F F4 E6 59 EE 7F 8A 72 " +
            "D9 59 CB C4 1C 64 0C B9 6E 9C 4F 8A 4E 6B 23 99 " +
            "22 F1 02 2C D9 AA CE D3 FE 80 89 07 CF 9C 6F BF " +
            "F6 13 0D 05 CD 4D 97 6B 2F 07 AE F9 45 F0 D8 13 " +
            "9B 10 62 02 BC CB CA D3 9A 12 84 BC C7 53 F1 9E " +
            "C8 EC B4 9A 36 6E F9 85",
            // After Chi
            "1B 17 7B 87 19 0F 0A 49 19 E6 C0 B2 9B EE 9C B9 " +
            "6E AF CE 57 D3 08 23 4B 41 F6 EE 5D 48 0B 36 BC " +
            "C2 51 BE 02 A2 5F 17 39 28 52 3E 16 66 4A 81 91 " +
            "57 25 DE 5E 59 59 10 3C 16 C4 EF 54 7F F4 CE C9 " +
            "FD 85 55 A8 83 06 17 B3 61 82 FA 79 A8 E5 E5 B8 " +
            "45 86 3E CF 24 49 43 F7 84 3C 05 A5 9C 0D 5C C3 " +
            "33 97 26 54 40 2E 80 2D DD A8 94 E8 25 CE 2C 0F " +
            "7C A9 F8 BE EF C5 47 34 69 70 E2 53 AC 74 A9 72 " +
            "D9 38 CB E0 8D E4 C0 FB B2 9C C6 89 48 7F 02 B5 " +
            "23 85 64 74 F9 C9 4E 93 6E 89 80 83 DF 9C 6B 36 " +
            "66 03 4D 07 75 46 95 AB 2F 05 2A 45 06 E0 E9 1F " +
            "DB FC 52 00 8C E7 C2 D2 AC 01 8D B9 0E 52 F7 F4 " +
            "C1 E8 16 62 36 DE B1 95",
            // After Iota 
            "9A 97 7B 07 19 0F 0A C9 19 E6 C0 B2 9B EE 9C B9 " +
            "6E AF CE 57 D3 08 23 4B 41 F6 EE 5D 48 0B 36 BC " +
            "C2 51 BE 02 A2 5F 17 39 28 52 3E 16 66 4A 81 91 " +
            "57 25 DE 5E 59 59 10 3C 16 C4 EF 54 7F F4 CE C9 " +
            "FD 85 55 A8 83 06 17 B3 61 82 FA 79 A8 E5 E5 B8 " +
            "45 86 3E CF 24 49 43 F7 84 3C 05 A5 9C 0D 5C C3 " +
            "33 97 26 54 40 2E 80 2D DD A8 94 E8 25 CE 2C 0F " +
            "7C A9 F8 BE EF C5 47 34 69 70 E2 53 AC 74 A9 72 " +
            "D9 38 CB E0 8D E4 C0 FB B2 9C C6 89 48 7F 02 B5 " +
            "23 85 64 74 F9 C9 4E 93 6E 89 80 83 DF 9C 6B 36 " +
            "66 03 4D 07 75 46 95 AB 2F 05 2A 45 06 E0 E9 1F " +
            "DB FC 52 00 8C E7 C2 D2 AC 01 8D B9 0E 52 F7 F4 " +
            "C1 E8 16 62 36 DE B1 95",
            // Round #21
            // After Theta
            "93 08 A4 FA BE 4F 96 9A A4 EE 33 85 48 44 32 5E " +
            "8E D2 B8 1A 35 06 B2 26 83 5C 29 CB 78 3B 45 58 " +
            "DC 6F D0 C7 BE 7A 4B B3 21 CD E1 EB C1 0A 1D C2 " +
            "EA 2D 2D 69 8A F3 BE DB F6 B9 99 19 99 FA 5F A4 " +
            "3F 2F 92 3E B3 36 64 57 7F BC 94 BC B4 C0 B9 32 " +
            "4C 19 E1 32 83 09 DF A4 39 34 F6 92 4F A7 F2 24 " +
            "D3 EA 50 19 A6 20 11 40 1F 02 53 7E 15 FE 5F EB " +
            "62 97 96 7B F3 E0 1B BE 60 EF 3D AE 0B 34 35 21 " +
            "64 30 38 D7 5E 4E 6E 1C 52 E1 B0 C4 AE 71 93 D8 " +
            "E1 2F A3 E2 C9 F9 3D 77 70 B7 EE 46 C3 B9 37 BC " +
            "6F 9C 92 FA D2 06 09 F8 92 0D D9 72 D5 4A 47 F8 " +
            "3B 81 24 4D 6A E9 53 BF 6E AB 4A 2F 3E 62 84 10 " +
            "DF D6 78 A7 2A FB ED 1F",
            // After Rho
            "93 08 A4 FA BE 4F 96 9A 48 DD 67 0A 91 88 64 BC " +
            "A3 34 AE 46 8D 81 AC 89 B7 53 84 35 C8 95 B2 8C " +
            "D5 5B 9A E5 7E 83 3E F6 1E AC D0 21 1C D2 1C BE " +
            "92 A6 38 EF BB AD DE D2 A9 7D 6E 66 46 A6 FE 17 " +
            "17 49 9F 59 1B B2 AB 9F 9C 2B F3 C7 4B C9 4B 0B " +
            "65 CA 08 97 19 4C F8 26 93 E4 D0 D8 4B 3E 9D CA " +
            "CA 30 05 89 00 9A 56 87 FC BF D6 3F 04 A6 FC 2A " +
            "BD 79 F0 0D 5F B1 4B CB 5C 17 68 6A 42 C0 DE 7B " +
            "E7 DA CB C9 8D 83 0C 06 49 6C A9 70 58 62 D7 B8 " +
            "BF E7 2E FC 65 54 3C 39 BC 70 B7 EE 46 C3 B9 37 " +
            "24 E0 BF 71 4A EA 4B 1B 4B 36 64 CB 55 2B 1D E1 " +
            "27 90 A4 49 2D 7D EA 77 AB 4A 2F 3E 62 84 10 6E " +
            "FB C7 B7 35 DE A9 CA 7E",
            // After Pi
            "93 08 A4 FA BE 4F 96 9A 92 A6 38 EF BB AD DE D2 " +
            "CA 30 05 89 00 9A 56 87 BF E7 2E FC 65 54 3C 39 " +
            "FB C7 B7 35 DE A9 CA 7E B7 53 84 35 C8 95 B2 8C " +
            "9C 2B F3 C7 4B C9 4B 0B 65 CA 08 97 19 4C F8 26 " +
            "E7 DA CB C9 8D 83 0C 06 27 90 A4 49 2D 7D EA 77 " +
            "48 DD 67 0A 91 88 64 BC A9 7D 6E 66 46 A6 FE 17 " +
            "FC BF D6 3F 04 A6 FC 2A BC 70 B7 EE 46 C3 B9 37 " +
            "24 E0 BF 71 4A EA 4B 1B D5 5B 9A E5 7E 83 3E F6 " +
            "1E AC D0 21 1C D2 1C BE 93 E4 D0 D8 4B 3E 9D CA " +
            "49 6C A9 70 58 62 D7 B8 AB 4A 2F 3E 62 84 10 6E " +
            "A3 34 AE 46 8D 81 AC 89 17 49 9F 59 1B B2 AB 9F " +
            "BD 79 F0 0D 5F B1 4B CB 5C 17 68 6A 42 C0 DE 7B " +
            "4B 36 64 CB 55 2B 1D E1",
            // After Chi
            "DB 18 A1 FA BE 5D 96 9F A7 61 12 9B DE E9 F6 EA " +
            "8A 30 94 88 9A 33 94 C1 BF EF 2E 36 45 12 28 B9 " +
            "FB 61 AF 30 DF 09 82 3E D6 93 8C 25 D8 91 02 A8 " +
            "1E 3B 30 8F CF 4A 4F 0B 65 CA 2C 97 39 30 1A 57 " +
            "77 99 CB FD 4D 03 1C 8E 2F B8 D7 8B 2E 35 A3 74 " +
            "1C 5F F7 13 91 88 64 94 A9 3D 4F A6 04 E7 FF 02 " +
            "FC 3F DE 2E 0C 8E BE 22 F4 6D F7 E4 D7 C3 9D 93 " +
            "85 C0 B7 15 0C CC D1 18 54 1B 9A 3D 3D AF BF B6 " +
            "56 A4 F9 01 0C 92 5E 8E 31 E6 D6 D6 69 BA 9D 8C " +
            "1D 7D 39 B1 44 61 F9 28 A1 EE 6F 3E 62 D4 10 66 " +
            "0B 04 CE 42 C9 80 EC C9 57 4F 97 3B 1B F2 3F AF " +
            "BE 59 F4 8C 4A 9A 4A 4B FC 17 E2 6E CA 40 7E 73 " +
            "5F 7F 75 D2 47 19 1E F7",
            // After Iota 
            "5B 98 A1 FA BE 5D 96 1F A7 61 12 9B DE E9 F6 EA " +
            "8A 30 94 88 9A 33 94 C1 BF EF 2E 36 45 12 28 B9 " +
            "FB 61 AF 30 DF 09 82 3E D6 93 8C 25 D8 91 02 A8 " +
            "1E 3B 30 8F CF 4A 4F 0B 65 CA 2C 97 39 30 1A 57 " +
            "77 99 CB FD 4D 03 1C 8E 2F B8 D7 8B 2E 35 A3 74 " +
            "1C 5F F7 13 91 88 64 94 A9 3D 4F A6 04 E7 FF 02 " +
            "FC 3F DE 2E 0C 8E BE 22 F4 6D F7 E4 D7 C3 9D 93 " +
            "85 C0 B7 15 0C CC D1 18 54 1B 9A 3D 3D AF BF B6 " +
            "56 A4 F9 01 0C 92 5E 8E 31 E6 D6 D6 69 BA 9D 8C " +
            "1D 7D 39 B1 44 61 F9 28 A1 EE 6F 3E 62 D4 10 66 " +
            "0B 04 CE 42 C9 80 EC C9 57 4F 97 3B 1B F2 3F AF " +
            "BE 59 F4 8C 4A 9A 4A 4B FC 17 E2 6E CA 40 7E 73 " +
            "5F 7F 75 D2 47 19 1E F7",
            // Round #22
            // After Theta
            "D7 08 73 A8 63 28 26 58 51 DF 14 FE C5 D9 9A 51 " +
            "20 5F 05 E1 3B F1 EE FD 7C 84 C1 D8 79 C4 33 4D " +
            "BA 87 7A A7 89 2C EA 78 5A 03 5E 77 05 E4 B2 EF " +
            "E8 85 36 EA D4 7A 23 B0 CF A5 BD FE 98 F2 60 6B " +
            "B4 F2 24 13 71 D5 07 7A 6E 5E 02 1C 78 10 CB 32 " +
            "90 CF 25 41 4C FD D4 D3 5F 83 49 C3 1F D7 93 B9 " +
            "56 50 4F 47 AD 4C C4 1E 37 06 18 0A EB 15 86 67 " +
            "C4 26 62 82 5A E9 B9 5E D8 8B 48 6F E0 DA 0F F1 " +
            "A0 1A FF 64 17 A2 32 35 9B 89 47 BF C8 78 E7 B0 " +
            "DE 16 D6 5F 78 B7 E2 DC E0 08 BA A9 34 F1 78 20 " +
            "87 94 1C 10 14 F5 5C 8E A1 F1 91 5E 00 C2 53 14 " +
            "14 36 65 E5 EB 58 30 77 3F 7C 0D 80 F6 96 65 87 " +
            "1E 99 A0 45 11 3C 76 B1",
            // After Rho
            "D7 08 73 A8 63 28 26 58 A2 BE 29 FC 8B B3 35 A3 " +
            "C8 57 41 F8 4E BC 7B 3F 47 3C D3 C4 47 18 8C 9D " +
            "64 51 C7 D3 3D D4 3B 4D 57 40 2E FB AE 35 E0 75 " +
            "A3 4E AD 37 02 8B 5E 68 DA 73 69 AF 3F A6 3C D8 " +
            "79 92 89 B8 EA 03 3D 5A B1 2C E3 E6 25 C0 81 07 " +
            "86 7C 2E 09 62 EA A7 9E E6 7E 0D 26 0D 7F 5C 4F " +
            "3A 6A 65 22 F6 B0 82 7A 2B 0C CF 6E 0C 30 14 D6 " +
            "41 AD F4 5C 2F 62 13 31 DE C0 B5 1F E2 B1 17 91 " +
            "9F EC 42 54 A6 06 54 E3 73 D8 CD C4 A3 5F 64 BC " +
            "56 9C DB DB C2 FA 0B EF 20 E0 08 BA A9 34 F1 78 " +
            "73 39 1E 52 72 40 50 D4 84 C6 47 7A 01 08 4F 51 " +
            "C2 A6 AC 7C 1D 0B E6 8E 7C 0D 80 F6 96 65 87 3F " +
            "5D AC 47 26 68 51 04 8F",
            // After Pi
            "D7 08 73 A8 63 28 26 58 A3 4E AD 37 02 8B 5E 68 " +
            "3A 6A 65 22 F6 B0 82 7A 56 9C DB DB C2 FA 0B EF " +
            "5D AC 47 26 68 51 04 8F 47 3C D3 C4 47 18 8C 9D " +
            "B1 2C E3 E6 25 C0 81 07 86 7C 2E 09 62 EA A7 9E " +
            "9F EC 42 54 A6 06 54 E3 C2 A6 AC 7C 1D 0B E6 8E " +
            "A2 BE 29 FC 8B B3 35 A3 DA 73 69 AF 3F A6 3C D8 " +
            "2B 0C CF 6E 0C 30 14 D6 20 E0 08 BA A9 34 F1 78 " +
            "73 39 1E 52 72 40 50 D4 64 51 C7 D3 3D D4 3B 4D " +
            "57 40 2E FB AE 35 E0 75 E6 7E 0D 26 0D 7F 5C 4F " +
            "73 D8 CD C4 A3 5F 64 BC 7C 0D 80 F6 96 65 87 3F " +
            "C8 57 41 F8 4E BC 7B 3F 79 92 89 B8 EA 03 3D 5A " +
            "41 AD F4 5C 2F 62 13 31 DE C0 B5 1F E2 B1 17 91 " +
            "84 C6 47 7A 01 08 4F 51",
            // After Chi
            "CF 28 33 A8 97 18 A6 4A E7 DA 37 EE 02 C1 57 ED " +
            "33 4A 61 06 DE B1 86 7A D4 9C EB 53 C1 D2 29 BF " +
            "7D EA CB 31 68 D2 5C AF 41 6C DF CD 05 32 AA 05 " +
            "A8 AC A3 B2 A1 C4 D1 66 C6 7E 82 21 7B E3 05 92 " +
            "9A F4 11 D4 E4 16 5C F2 72 A6 8C 5E 3D CB E7 8C " +
            "83 B2 AF BC 8B A3 35 A5 DA 93 69 3F 9E A2 DD F0 " +
            "78 15 D9 2E 5E 70 14 52 A0 66 29 16 20 87 D4 5B " +
            "2B 78 5E 51 46 44 58 8C C4 6F C6 D7 3C 9E 27 47 " +
            "46 C0 EE 3B 0C 35 C0 C5 EA 7B 0D 14 19 5F DF 4C " +
            "73 88 8A C5 8A CF 5C FC 6F 0D A8 DE 14 44 47 0F " +
            "C8 7A 35 BC 4B DC 79 1E E7 D2 88 BB 2A 92 39 DA " +
            "41 AB B6 3C 2E 6A 5B 71 96 D1 B5 9F AC 05 27 BF " +
            "B5 46 CF 7A A1 0B 4B 11",
            // After Iota 
            "CE 28 33 28 97 18 A6 4A E7 DA 37 EE 02 C1 57 ED " +
            "33 4A 61 06 DE B1 86 7A D4 9C EB 53 C1 D2 29 BF " +
            "7D EA CB 31 68 D2 5C AF 41 6C DF CD 05 32 AA 05 " +
            "A8 AC A3 B2 A1 C4 D1 66 C6 7E 82 21 7B E3 05 92 " +
            "9A F4 11 D4 E4 16 5C F2 72 A6 8C 5E 3D CB E7 8C " +
            "83 B2 AF BC 8B A3 35 A5 DA 93 69 3F 9E A2 DD F0 " +
            "78 15 D9 2E 5E 70 14 52 A0 66 29 16 20 87 D4 5B " +
            "2B 78 5E 51 46 44 58 8C C4 6F C6 D7 3C 9E 27 47 " +
            "46 C0 EE 3B 0C 35 C0 C5 EA 7B 0D 14 19 5F DF 4C " +
            "73 88 8A C5 8A CF 5C FC 6F 0D A8 DE 14 44 47 0F " +
            "C8 7A 35 BC 4B DC 79 1E E7 D2 88 BB 2A 92 39 DA " +
            "41 AB B6 3C 2E 6A 5B 71 96 D1 B5 9F AC 05 27 BF " +
            "B5 46 CF 7A A1 0B 4B 11",
            // Round #23
            // After Theta
            "58 B9 7A 75 06 0A 0D 32 AA DB 84 9F F4 25 16 50 " +
            "11 13 22 72 82 A3 91 B5 0F 92 96 46 40 E0 E4 5B " +
            "77 7B 46 9F 97 CD 49 9C D7 FD 96 90 94 20 01 7D " +
            "E5 AD 10 C3 57 20 90 DB E4 27 C1 55 27 F1 12 5D " +
            "41 FA 6C C1 65 24 91 16 78 37 01 F0 C2 D4 F2 BF " +
            "15 23 E6 E1 1A B1 9E DD 97 92 DA 4E 68 46 9C 4D " +
            "5A 4C 9A 5A 02 62 03 9D 7B 68 54 03 A1 B5 19 BF " +
            "21 E9 D3 FF B9 5B 4D BF 52 FE 8F 8A AD 8C 8C 3F " +
            "0B C1 5D 4A FA D1 81 78 C8 22 4E 60 45 4D C8 83 " +
            "A8 86 F7 D0 0B FD 91 18 65 9C 25 70 EB 5B 52 3C " +
            "5E EB 7C E1 DA CE D2 66 AA D3 3B CA DC 76 78 67 " +
            "63 F2 F5 48 72 78 4C BE 4D DF C8 8A 2D 37 EA 5B " +
            "BF D7 42 D4 5E 14 5E 22",
            // After Rho
            "58 B9 7A 75 06 0A 0D 32 54 B7 09 3F E9 4B 2C A0 " +
            "C4 84 88 9C E0 68 64 6D 04 4E BE F5 20 69 69 04 " +
            "6C 4E E2 BC DB 33 FA BC 49 09 12 D0 77 DD 6F 09 " +
            "31 7C 05 02 B9 5D DE 0A 17 F9 49 70 D5 49 BC 44 " +
            "7D B6 E0 32 92 48 8B 20 2D FF 8B 77 13 00 2F 4C " +
            "AE 18 31 0F D7 88 F5 EC 36 5D 4A 6A 3B A1 19 71 " +
            "D4 12 10 1B E8 D4 62 D2 6B 33 7E F7 D0 A8 06 42 " +
            "FF DC AD A6 DF 90 F4 E9 15 5B 19 19 7F A4 FC 1F " +
            "4B 49 3F 3A 10 6F 21 B8 E4 41 64 11 27 B0 A2 26 " +
            "3F 12 03 D5 F0 1E 7A A1 3C 65 9C 25 70 EB 5B 52 " +
            "4B 9B 79 AD F3 85 6B 3B A9 4E EF 28 73 DB E1 9D " +
            "4C BE 1E 49 0E 8F C9 77 DF C8 8A 2D 37 EA 5B 4D " +
            "97 C8 EF B5 10 B5 17 85",
            // After Pi
            "58 B9 7A 75 06 0A 0D 32 31 7C 05 02 B9 5D DE 0A " +
            "D4 12 10 1B E8 D4 62 D2 3F 12 03 D5 F0 1E 7A A1 " +
            "97 C8 EF B5 10 B5 17 85 04 4E BE F5 20 69 69 04 " +
            "2D FF 8B 77 13 00 2F 4C AE 18 31 0F D7 88 F5 EC " +
            "4B 49 3F 3A 10 6F 21 B8 4C BE 1E 49 0E 8F C9 77 " +
            "54 B7 09 3F E9 4B 2C A0 17 F9 49 70 D5 49 BC 44 " +
            "6B 33 7E F7 D0 A8 06 42 3C 65 9C 25 70 EB 5B 52 " +
            "4B 9B 79 AD F3 85 6B 3B 6C 4E E2 BC DB 33 FA BC " +
            "49 09 12 D0 77 DD 6F 09 36 5D 4A 6A 3B A1 19 71 " +
            "E4 41 64 11 27 B0 A2 26 DF C8 8A 2D 37 EA 5B 4D " +
            "C4 84 88 9C E0 68 64 6D 7D B6 E0 32 92 48 8B 20 " +
            "FF DC AD A6 DF 90 F4 E9 15 5B 19 19 7F A4 FC 1F " +
            "A9 4E EF 28 73 DB E1 9D",
            // After Chi
            "9C BB 6A 6C 46 8A 2D E2 1A 7C 06 C6 A9 57 C6 2B " +
            "54 DA FC 3B E8 75 67 D6 77 23 13 95 F6 14 72 93 " +
            "B6 8C EA B7 A9 E0 C5 8D 86 4E 8E FD E4 E1 B9 A4 " +
            "6C BE 85 47 13 67 2F 5C AA AE 31 4E D9 08 3D AB " +
            "4B 09 9F 8E 30 0F 01 B8 65 0F 1F 4B 1D 8F CF 3F " +
            "3C B5 3F B8 E9 EB 2E A2 03 BD C9 70 F5 0A E5 54 " +
            "28 A9 1F 7F 53 AC 26 6B 28 41 9C 37 78 A1 5F D2 " +
            "48 D3 39 ED E7 85 FB 7F 5A 1A AA 96 D3 13 EA CC " +
            "89 09 36 C1 73 CD CD 0F 2D D5 C0 46 2B EB 40 38 " +
            "C4 47 04 81 EF A1 02 96 DE C9 9A 6D 13 26 5E 4C " +
            "46 CC 85 18 AD F8 10 A4 7D B5 F0 2B B2 6C 83 36 " +
            "57 D8 4B 86 DF CB F5 69 51 DB 19 8D FF 84 F8 7F " +
            "90 7C 8F 0A 61 DB 6A 9D",
            // After Iota 
            "94 3B 6A EC 46 8A 2D 62 1A 7C 06 C6 A9 57 C6 2B " +
            "54 DA FC 3B E8 75 67 D6 77 23 13 95 F6 14 72 93 " +
            "B6 8C EA B7 A9 E0 C5 8D 86 4E 8E FD E4 E1 B9 A4 " +
            "6C BE 85 47 13 67 2F 5C AA AE 31 4E D9 08 3D AB " +
            "4B 09 9F 8E 30 0F 01 B8 65 0F 1F 4B 1D 8F CF 3F " +
            "3C B5 3F B8 E9 EB 2E A2 03 BD C9 70 F5 0A E5 54 " +
            "28 A9 1F 7F 53 AC 26 6B 28 41 9C 37 78 A1 5F D2 " +
            "48 D3 39 ED E7 85 FB 7F 5A 1A AA 96 D3 13 EA CC " +
            "89 09 36 C1 73 CD CD 0F 2D D5 C0 46 2B EB 40 38 " +
            "C4 47 04 81 EF A1 02 96 DE C9 9A 6D 13 26 5E 4C " +
            "46 CC 85 18 AD F8 10 A4 7D B5 F0 2B B2 6C 83 36 " +
            "57 D8 4B 86 DF CB F5 69 51 DB 19 8D FF 84 F8 7F " +
            "90 7C 8F 0A 61 DB 6A 9D",
            // Xor'd state (in bytes)
            "94 3B 6A EC 46 8A 2D 62 1A 7C 06 C6 A9 57 C6 2B " +
            "54 DA FC 3B E8 75 67 D6 77 23 13 95 F6 14 72 93 " +
            "B6 8C EA B7 A9 E0 C5 8D 86 4E 8E FD E4 E1 B9 A4 " +
            "6C BE 85 47 13 67 2F 5C AA AE 31 4E D9 08 3D AB " +
            "4B 09 9F 8E 30 0F 01 B8 65 0F 1F 4B 1D 8F CF 3F " +
            "3C B5 3F B8 E9 EB 2E A2 03 BD C9 70 F5 0A E5 54 " +
            "28 A9 1F 7F 53 AC 26 6B 28 41 9C 37 78 A1 5F D2 " +
            "48 D3 39 ED E7 85 FB 7F 5A 1A AA 96 D3 13 EA CC " +
            "89 09 36 C1 73 CD CD 0F 2D D5 C0 46 2B EB 40 38 " +
            "C4 47 04 81 EF A1 02 96 DE C9 9A 6D 13 26 5E 4C " +
            "46 CC 85 18 AD F8 10 A4 7D B5 F0 2B B2 6C 83 36 " +
            "57 D8 4B 86 DF CB F5 69 51 DB 19 8D FF 84 F8 7F " +
            "90 7C 8F 0A 61 DB 6A 9D",
            // Round #0
            // After Theta
            "43 59 AA AD 7B AA 6D 4A 70 CB 41 75 B1 DF 15 A8 " +
            "D7 F6 6B 60 3A D1 88 ED 71 38 F9 B2 22 CB 31 ED " +
            "53 57 0F D8 6D A9 93 85 51 2C 4E BC D9 C1 F9 8C " +
            "06 09 C2 F4 0B EF FC DF 29 82 A6 15 0B AC D2 90 " +
            "4D 12 75 A9 E4 D0 42 C6 80 D4 FA 24 D9 C6 99 37 " +
            "EB D7 FF F9 D4 CB 6E 8A 69 0A 8E C3 ED 82 36 D7 " +
            "AB 85 88 24 81 08 C9 50 2E 5A 76 10 AC 7E 1C AC " +
            "AD 08 DC 82 23 CC AD 77 8D 78 6A D7 EE 33 AA E4 " +
            "E3 BE 71 72 6B 45 1E 8C AE F9 57 1D F9 4F AF 03 " +
            "C2 5C EE A6 3B 7E 41 E8 3B 12 7F 02 D7 6F 08 44 " +
            "91 AE 45 59 90 D8 50 8C 17 02 B7 98 AA E4 50 B5 " +
            "D4 F4 DC DD 0D 6F 1A 52 57 C0 F3 AA 2B 5B BB 01 " +
            "75 A7 6A 65 A5 92 3C 95",
            // After Rho
            "43 59 AA AD 7B AA 6D 4A E1 96 83 EA 62 BF 2B 50 " +
            "B5 FD 1A 98 4E 34 62 FB B2 1C D3 1E 87 93 2F 2B " +
            "4B 9D 2C 9C BA 7A C0 6E 9B 1D 9C CF 18 C5 E2 C4 " +
            "4C BF F0 CE FF 6D 90 20 64 8A A0 69 C5 02 AB 34 " +
            "89 BA 54 72 68 21 E3 26 9C 79 03 48 AD 4F 92 6D " +
            "5C BF FE CF A7 5E 76 53 5C A7 29 38 0E B7 0B DA " +
            "24 09 44 48 86 5A 2D 44 FD 38 58 5D B4 EC 20 58 " +
            "C1 11 E6 D6 BB 56 04 6E AE DD 67 54 C9 1B F1 D4 " +
            "4E 6E AD C8 83 71 DC 37 D7 01 D7 FC AB 8E FC A7 " +
            "2F 08 5D 98 CB DD 74 C7 44 3B 12 7F 02 D7 6F 08 " +
            "43 31 46 BA 16 65 41 62 5E 08 DC 62 AA 92 43 D5 " +
            "9A 9E BB BB E1 4D 43 8A C0 F3 AA 2B 5B BB 01 57 " +
            "4F 65 DD A9 5A 59 A9 24",
            // After Pi
            "43 59 AA AD 7B AA 6D 4A 4C BF F0 CE FF 6D 90 20 " +
            "24 09 44 48 86 5A 2D 44 2F 08 5D 98 CB DD 74 C7 " +
            "4F 65 DD A9 5A 59 A9 24 B2 1C D3 1E 87 93 2F 2B " +
            "9C 79 03 48 AD 4F 92 6D 5C BF FE CF A7 5E 76 53 " +
            "4E 6E AD C8 83 71 DC 37 9A 9E BB BB E1 4D 43 8A " +
            "E1 96 83 EA 62 BF 2B 50 64 8A A0 69 C5 02 AB 34 " +
            "FD 38 58 5D B4 EC 20 58 44 3B 12 7F 02 D7 6F 08 " +
            "43 31 46 BA 16 65 41 62 4B 9D 2C 9C BA 7A C0 6E " +
            "9B 1D 9C CF 18 C5 E2 C4 5C A7 29 38 0E B7 0B DA " +
            "D7 01 D7 FC AB 8E FC A7 C0 F3 AA 2B 5B BB 01 57 " +
            "B5 FD 1A 98 4E 34 62 FB 89 BA 54 72 68 21 E3 26 " +
            "C1 11 E6 D6 BB 56 04 6E AE DD 67 54 C9 1B F1 D4 " +
            "5E 08 DC 62 AA 92 43 D5",
            // After Chi
            "63 59 AE AD 7B B8 40 0E 47 BF E9 5E B6 E8 C0 A3 " +
            "64 6C C4 69 96 5A A4 64 2F 10 7F 9C EA 7F 30 8D " +
            "43 C3 8D EB DE 1C 39 04 F2 9A 2F 99 85 83 4B 39 " +
            "9E 39 02 48 AD 6E 1A 49 CC 2F EC FC C7 52 75 DB " +
            "6E 6E ED CC 85 E3 F0 16 96 FF BB FB C9 01 D3 CE " +
            "78 A6 DB FE 52 53 2B 18 64 89 A2 4B C7 11 E4 34 " +
            "FE 38 1C DD A0 CC 20 3A E4 BD 93 3F 62 4D 45 18 " +
            "47 39 66 BB 93 65 C1 46 0F 3F 0D AC BC 48 C9 74 " +
            "18 1D 4A 0B B9 CD 16 E1 5C 55 01 3B 5E 86 0A 8A " +
            "DC 0D D3 68 0B CE 3C 8F 50 F3 3A 68 5B 3E 23 D7 " +
            "F5 FC B8 1C DD 62 66 B3 A7 76 55 72 28 28 12 B6 " +
            "91 11 7E F4 99 D6 06 6F 0F 28 65 CC 8D 3F D1 FE " +
            "56 0A 98 00 8A 93 C2 D1",
            // After Iota 
            "62 59 AE AD 7B B8 40 0E 47 BF E9 5E B6 E8 C0 A3 " +
            "64 6C C4 69 96 5A A4 64 2F 10 7F 9C EA 7F 30 8D " +
            "43 C3 8D EB DE 1C 39 04 F2 9A 2F 99 85 83 4B 39 " +
            "9E 39 02 48 AD 6E 1A 49 CC 2F EC FC C7 52 75 DB " +
            "6E 6E ED CC 85 E3 F0 16 96 FF BB FB C9 01 D3 CE " +
            "78 A6 DB FE 52 53 2B 18 64 89 A2 4B C7 11 E4 34 " +
            "FE 38 1C DD A0 CC 20 3A E4 BD 93 3F 62 4D 45 18 " +
            "47 39 66 BB 93 65 C1 46 0F 3F 0D AC BC 48 C9 74 " +
            "18 1D 4A 0B B9 CD 16 E1 5C 55 01 3B 5E 86 0A 8A " +
            "DC 0D D3 68 0B CE 3C 8F 50 F3 3A 68 5B 3E 23 D7 " +
            "F5 FC B8 1C DD 62 66 B3 A7 76 55 72 28 28 12 B6 " +
            "91 11 7E F4 99 D6 06 6F 0F 28 65 CC 8D 3F D1 FE " +
            "56 0A 98 00 8A 93 C2 D1",
            // Round #1
            // After Theta
            "F3 6D F0 26 B4 89 FE 96 63 66 90 2A 16 82 B4 8A " +
            "8B C4 FD DA CC 69 4E 09 9D D6 D1 9C 77 41 58 F8 " +
            "10 69 E5 D5 CF B9 4F 27 63 AE 71 12 4A B2 F5 A1 " +
            "BA E0 7B 3C 0D 04 6E 60 23 87 D5 4F 9D 61 9F B6 " +
            "DC A8 43 CC 18 DD 98 63 C5 55 D3 C5 D8 A4 A5 ED " +
            "E9 92 85 75 9D 62 95 80 40 50 DB 3F 67 7B 90 1D " +
            "11 90 25 6E FA FF CA 57 56 7B 3D 3F FF 73 2D 6D " +
            "14 93 0E 85 82 C0 B7 65 9E 0B 53 27 73 79 77 EC " +
            "3C C4 33 7F 19 A7 62 C8 B3 FD 38 88 04 B5 E0 E7 " +
            "6E CB 7D 68 96 F0 54 FA 03 59 52 56 4A 9B 55 F4 " +
            "64 C8 E6 97 12 53 D8 2B 83 AF 2C 06 88 42 66 9F " +
            "7E B9 47 47 C3 E5 EC 02 BD EE CB CC 10 01 B9 8B " +
            "05 A0 F0 3E 9B 36 B4 F2",
            // After Rho
            "F3 6D F0 26 B4 89 FE 96 C7 CC 20 55 2C 04 69 15 " +
            "22 71 BF 36 73 9A 53 C2 17 84 85 DF 69 1D CD 79 " +
            "CE 7D 3A 81 48 2B AF 7E A1 24 5B 1F 3A E6 1A 27 " +
            "C7 D3 40 E0 06 A6 0B BE ED C8 61 F5 53 67 D8 A7 " +
            "D4 21 66 8C 6E CC 31 6E 5A DA 5E 5C 35 5D 8C 4D " +
            "4C 97 2C AC EB 14 AB 04 76 00 41 6D FF 9C ED 41 " +
            "71 D3 FF 57 BE 8A 80 2C E7 5A DA AC F6 7A 7E FE " +
            "42 41 E0 DB 32 8A 49 87 4E E6 F2 EE D8 3D 17 A6 " +
            "E6 2F E3 54 0C 99 87 78 F0 F3 D9 7E 1C 44 82 5A " +
            "9E 4A DF 6D B9 0F CD 12 F4 03 59 52 56 4A 9B 55 " +
            "61 AF 90 21 9B 5F 4A 4C 0E BE B2 18 20 0A 99 7D " +
            "2F F7 E8 68 B8 9C 5D C0 EE CB CC 10 01 B9 8B BD " +
            "AD 7C 01 28 BC CF A6 0D",
            // After Pi
            "F3 6D F0 26 B4 89 FE 96 C7 D3 40 E0 06 A6 0B BE " +
            "71 D3 FF 57 BE 8A 80 2C 9E 4A DF 6D B9 0F CD 12 " +
            "AD 7C 01 28 BC CF A6 0D 17 84 85 DF 69 1D CD 79 " +
            "5A DA 5E 5C 35 5D 8C 4D 4C 97 2C AC EB 14 AB 04 " +
            "E6 2F E3 54 0C 99 87 78 2F F7 E8 68 B8 9C 5D C0 " +
            "C7 CC 20 55 2C 04 69 15 ED C8 61 F5 53 67 D8 A7 " +
            "E7 5A DA AC F6 7A 7E FE F4 03 59 52 56 4A 9B 55 " +
            "61 AF 90 21 9B 5F 4A 4C CE 7D 3A 81 48 2B AF 7E " +
            "A1 24 5B 1F 3A E6 1A 27 76 00 41 6D FF 9C ED 41 " +
            "F0 F3 D9 7E 1C 44 82 5A EE CB CC 10 01 B9 8B BD " +
            "22 71 BF 36 73 9A 53 C2 D4 21 66 8C 6E CC 31 6E " +
            "42 41 E0 DB 32 8A 49 87 4E E6 F2 EE D8 3D 17 A6 " +
            "0E BE B2 18 20 0A 99 7D",
            // After Chi
            "C3 6D 4F 31 0C 81 7E 96 49 DB 40 C8 07 A3 46 AC " +
            "50 E7 FF 57 BA 4A A2 21 CC 4B 2F 6B B9 0F 95 80 " +
            "A9 EE 01 E8 BE E9 A7 25 13 81 A5 7F A3 1D EE 79 " +
            "F8 F2 9D 0C 31 D4 88 35 45 47 24 84 5B 10 F3 84 " +
            "F6 2F E6 C3 4D 98 07 41 67 AD B2 68 AC DC 5D C4 " +
            "C5 DE BA 5D 88 1C 4F 4D FD C9 60 A7 53 67 59 A6 " +
            "E6 F6 5A 8D 7F 6F 3E F6 72 43 79 06 72 4A BA 44 " +
            "49 AF D1 81 C8 3C DA EE 98 7D 3A E1 8D 33 4A 3E " +
            "21 D7 C3 0D 3A A6 18 3D 78 08 45 6D FE 25 E4 E4 " +
            "F0 C7 EB FF 54 46 A6 18 CF CB 8D 0E 33 7D 9B BC " +
            "20 31 3F 65 63 98 1B 43 D8 87 74 A8 A6 F9 27 4E " +
            "42 59 E0 CB 12 88 C1 DE 6E A7 FF C8 8B AD 55 24 " +
            "DA BE F2 90 2C 4E B9 51",
            // After Iota 
            "41 ED 4F 31 0C 81 7E 96 49 DB 40 C8 07 A3 46 AC " +
            "50 E7 FF 57 BA 4A A2 21 CC 4B 2F 6B B9 0F 95 80 " +
            "A9 EE 01 E8 BE E9 A7 25 13 81 A5 7F A3 1D EE 79 " +
            "F8 F2 9D 0C 31 D4 88 35 45 47 24 84 5B 10 F3 84 " +
            "F6 2F E6 C3 4D 98 07 41 67 AD B2 68 AC DC 5D C4 " +
            "C5 DE BA 5D 88 1C 4F 4D FD C9 60 A7 53 67 59 A6 " +
            "E6 F6 5A 8D 7F 6F 3E F6 72 43 79 06 72 4A BA 44 " +
            "49 AF D1 81 C8 3C DA EE 98 7D 3A E1 8D 33 4A 3E " +
            "21 D7 C3 0D 3A A6 18 3D 78 08 45 6D FE 25 E4 E4 " +
            "F0 C7 EB FF 54 46 A6 18 CF CB 8D 0E 33 7D 9B BC " +
            "20 31 3F 65 63 98 1B 43 D8 87 74 A8 A6 F9 27 4E " +
            "42 59 E0 CB 12 88 C1 DE 6E A7 FF C8 8B AD 55 24 " +
            "DA BE F2 90 2C 4E B9 51",
            // Round #2
            // After Theta
            "B9 15 47 22 3A 24 2C ED F4 2A 5D AF 2B B8 5D A1 " +
            "48 D8 BD A2 F0 69 BC 1E 20 7F 30 AD 40 E2 DB 2D " +
            "20 55 0E 5F 74 88 60 23 EB 79 AD 6C 95 B8 BC 02 " +
            "45 03 80 6B 1D CF 93 38 5D 78 66 71 11 33 ED BB " +
            "1A 1B F9 05 B4 75 49 EC EE 16 BD DF 66 BD 9A C2 " +
            "3D 26 B2 4E BE B9 1D 36 40 38 7D C0 7F 7C 42 AB " +
            "FE C9 18 78 35 4C 20 C9 9E 77 66 C0 8B A7 F4 E9 " +
            "C0 14 DE 36 02 5D 1D E8 60 85 32 F2 BB 96 18 45 " +
            "9C 26 DE 6A 16 BD 03 30 60 37 07 98 B4 06 FA DB " +
            "1C F3 F4 39 AD AB E8 B5 46 70 82 B9 F9 1C 5C BA " +
            "D8 C9 37 76 55 3D 49 38 65 76 69 CF 8A E2 3C 43 " +
            "5A 66 A2 3E 58 AB DF E1 82 93 E0 0E 72 40 1B 89 " +
            "53 05 FD 27 E6 2F 7E 57",
            // After Rho
            "B9 15 47 22 3A 24 2C ED E9 55 BA 5E 57 70 BB 42 " +
            "12 76 AF 28 7C 1A AF 07 24 BE DD 02 F2 07 D3 0A " +
            "43 04 1B 01 A9 72 F8 A2 56 89 CB 2B B0 9E D7 CA " +
            "B8 D6 F1 3C 89 53 34 00 6E 17 9E 59 5C C4 4C FB " +
            "8D FC 02 DA BA 24 76 8D AB 29 EC 6E D1 FB 6D D6 " +
            "E9 31 91 75 F2 CD ED B0 AD 02 E1 F4 01 FF F1 09 " +
            "C0 AB 61 02 49 F6 4F C6 4F E9 D3 3D EF CC 80 17 " +
            "1B 81 AE 0E 74 60 0A 6F E4 77 2D 31 8A C0 0A 65 " +
            "5B CD A2 77 00 86 D3 C4 FD 6D B0 9B 03 4C 5A 03 " +
            "15 BD 96 63 9E 3E A7 75 BA 46 70 82 B9 F9 1C 5C " +
            "24 E1 60 27 DF D8 55 F5 95 D9 A5 3D 2B 8A F3 0C " +
            "CB 4C D4 07 6B F5 3B 5C 93 E0 0E 72 40 1B 89 82 " +
            "DF D5 54 41 FF 89 F9 8B",
            // After Pi
            "B9 15 47 22 3A 24 2C ED B8 D6 F1 3C 89 53 34 00 " +
            "C0 AB 61 02 49 F6 4F C6 15 BD 96 63 9E 3E A7 75 " +
            "DF D5 54 41 FF 89 F9 8B 24 BE DD 02 F2 07 D3 0A " +
            "AB 29 EC 6E D1 FB 6D D6 E9 31 91 75 F2 CD ED B0 " +
            "5B CD A2 77 00 86 D3 C4 CB 4C D4 07 6B F5 3B 5C " +
            "E9 55 BA 5E 57 70 BB 42 6E 17 9E 59 5C C4 4C FB " +
            "4F E9 D3 3D EF CC 80 17 BA 46 70 82 B9 F9 1C 5C " +
            "24 E1 60 27 DF D8 55 F5 43 04 1B 01 A9 72 F8 A2 " +
            "56 89 CB 2B B0 9E D7 CA AD 02 E1 F4 01 FF F1 09 " +
            "FD 6D B0 9B 03 4C 5A 03 93 E0 0E 72 40 1B 89 82 " +
            "12 76 AF 28 7C 1A AF 07 8D FC 02 DA BA 24 76 8D " +
            "1B 81 AE 0E 74 60 0A 6F E4 77 2D 31 8A C0 0A 65 " +
            "95 D9 A5 3D 2B 8A F3 0C",
            // After Chi
            "F9 3C 47 20 7A 80 67 2B AD C2 67 5D 1F 5B 94 31 " +
            "0A EB 21 02 28 77 17 4C 35 BD 95 41 9E 1A A3 11 " +
            "DF 17 E4 5D 7E DA E9 8B 64 AE CC 13 D0 03 53 2A " +
            "B9 E5 CE 6C D1 F9 7F 92 69 31 C5 75 99 BC C5 A8 " +
            "7F 7F AB 77 90 84 13 C6 40 4D F4 6B 6A 0D 17 88 " +
            "E8 BD FB 7A F4 78 3B 46 DE 11 BE DB 4C F5 50 B3 " +
            "4B 48 D3 18 A9 CC C1 B6 73 52 EA DA B9 D9 B6 5E " +
            "22 E3 64 26 D7 5C 11 4C EA 06 3B D5 A8 13 D8 A3 " +
            "06 E4 DB 20 B2 9E DD C8 AF 82 EF 94 41 EC 70 89 " +
            "BD 69 A1 9A AA 2C 2A 23 87 69 CE 58 50 97 8E CA " +
            "00 77 03 2C 38 5A A7 65 69 8A 03 EB 30 A4 76 8D " +
            "0A 09 2E 02 55 6A FB 67 E6 51 27 31 DE D0 06 66 " +
            "18 51 A5 EF A9 AE A3 84",
            // After Iota 
            "73 BC 47 20 7A 80 67 AB AD C2 67 5D 1F 5B 94 31 " +
            "0A EB 21 02 28 77 17 4C 35 BD 95 41 9E 1A A3 11 " +
            "DF 17 E4 5D 7E DA E9 8B 64 AE CC 13 D0 03 53 2A " +
            "B9 E5 CE 6C D1 F9 7F 92 69 31 C5 75 99 BC C5 A8 " +
            "7F 7F AB 77 90 84 13 C6 40 4D F4 6B 6A 0D 17 88 " +
            "E8 BD FB 7A F4 78 3B 46 DE 11 BE DB 4C F5 50 B3 " +
            "4B 48 D3 18 A9 CC C1 B6 73 52 EA DA B9 D9 B6 5E " +
            "22 E3 64 26 D7 5C 11 4C EA 06 3B D5 A8 13 D8 A3 " +
            "06 E4 DB 20 B2 9E DD C8 AF 82 EF 94 41 EC 70 89 " +
            "BD 69 A1 9A AA 2C 2A 23 87 69 CE 58 50 97 8E CA " +
            "00 77 03 2C 38 5A A7 65 69 8A 03 EB 30 A4 76 8D " +
            "0A 09 2E 02 55 6A FB 67 E6 51 27 31 DE D0 06 66 " +
            "18 51 A5 EF A9 AE A3 84",
            // Round #3
            // After Theta
            "1B 8C C6 C4 40 E8 85 00 A3 2F C3 1E C8 EB D5 49 " +
            "6A E3 4B AD AE 6D 52 81 FC A6 5C F6 E7 FF BE AE " +
            "97 03 27 7A 20 04 22 45 0C 9E 4D F7 EA 6B B1 81 " +
            "B7 08 6A 2F 06 49 3E EA 09 39 AF DA 1F A6 80 65 " +
            "B6 64 62 C0 E9 61 0E 79 08 59 37 4C 34 D3 DC 46 " +
            "80 8D 7A 9E CE 10 D9 ED D0 FC 1A 98 9B 45 11 CB " +
            "2B 40 B9 B7 2F D6 84 7B BA 49 23 6D C0 3C AB E1 " +
            "6A F7 A7 01 89 82 DA 82 82 36 BA 31 92 7B 3A 08 " +
            "08 09 7F 63 65 2E 9C B0 CF 8A 85 3B C7 F6 35 44 " +
            "74 72 68 2D D3 C9 37 9C CF 7D 0D 7F 0E 49 45 04 " +
            "68 47 82 C8 02 32 45 CE 67 67 A7 A8 E7 14 37 F5 " +
            "6A 01 44 AD D3 70 BE AA 2F 4A EE 86 A7 35 1B D9 " +
            "50 45 66 C8 F7 70 68 4A",
            // After Rho
            "1B 8C C6 C4 40 E8 85 00 46 5F 86 3D 90 D7 AB 93 " +
            "DA F8 52 AB 6B 9B 54 A0 FE EF EB CA 6F CA 65 7F " +
            "21 10 29 BA 1C 38 D1 03 AF BE 16 1B C8 E0 D9 74 " +
            "F6 62 90 E4 A3 7E 8B A0 59 42 CE AB F6 87 29 60 " +
            "32 31 E0 F4 30 87 3C 5B CD 6D 84 90 75 C3 44 33 " +
            "07 6C D4 F3 74 86 C8 6E 2C 43 F3 6B 60 6E 16 45 " +
            "BD 7D B1 26 DC 5B 01 CA 79 56 C3 75 93 46 DA 80 " +
            "80 44 41 6D 41 B5 FB D3 63 24 F7 74 10 04 6D 74 " +
            "6F AC CC 85 13 16 21 E1 1A A2 67 C5 C2 9D 63 FB " +
            "F9 86 93 4E 0E AD 65 3A 04 CF 7D 0D 7F 0E 49 45 " +
            "14 39 A3 1D 09 22 0B C8 9F 9D 9D A2 9E 53 DC D4 " +
            "2D 80 A8 75 1A CE 57 55 4A EE 86 A7 35 1B D9 2F " +
            "9A 12 54 91 19 F2 3D 1C",
            // After Pi
            "1B 8C C6 C4 40 E8 85 00 F6 62 90 E4 A3 7E 8B A0 " +
            "BD 7D B1 26 DC 5B 01 CA F9 86 93 4E 0E AD 65 3A " +
            "9A 12 54 91 19 F2 3D 1C FE EF EB CA 6F CA 65 7F " +
            "CD 6D 84 90 75 C3 44 33 07 6C D4 F3 74 86 C8 6E " +
            "6F AC CC 85 13 16 21 E1 2D 80 A8 75 1A CE 57 55 " +
            "46 5F 86 3D 90 D7 AB 93 59 42 CE AB F6 87 29 60 " +
            "79 56 C3 75 93 46 DA 80 04 CF 7D 0D 7F 0E 49 45 " +
            "14 39 A3 1D 09 22 0B C8 21 10 29 BA 1C 38 D1 03 " +
            "AF BE 16 1B C8 E0 D9 74 2C 43 F3 6B 60 6E 16 45 " +
            "1A A2 67 C5 C2 9D 63 FB 4A EE 86 A7 35 1B D9 2F " +
            "DA F8 52 AB 6B 9B 54 A0 32 31 E0 F4 30 87 3C 5B " +
            "80 44 41 6D 41 B5 FB D3 63 24 F7 74 10 04 6D 74 " +
            "9F 9D 9D A2 9E 53 DC D4",
            // After Chi
            "12 91 E7 C6 1C E9 85 4A B6 E0 92 AC A1 DA EF 90 " +
            "BF 6D F5 B7 CD 09 19 CE F8 0A 11 0A 4E A5 E5 3A " +
            "7E 70 44 B1 BA E4 37 BC FC EF BB A9 6F CE ED 33 " +
            "A5 ED 8C 94 76 D3 65 B2 07 6C F4 83 7C 4E 9E 7A " +
            "BD C3 8F 0F 76 16 01 CB 2C 80 AC 65 0A CF 57 55 " +
            "66 4B 87 69 91 97 79 13 5D CB F2 A3 9A 8F 28 25 " +
            "69 66 41 65 93 66 D8 08 46 89 79 2D EF DB E9 56 " +
            "0D 39 EB 9F 6F 22 0B A8 21 51 C8 DA 3C 36 D7 02 " +
            "BD 1E 12 9F 4A 71 B8 CE 6C 0F 73 49 55 6C 8E 41 " +
            "3B B2 4E DD CA BD 63 FB C4 40 90 A6 F5 DB D1 5B " +
            "5A BC 53 A2 2A AB 97 20 51 11 56 E4 20 87 38 7F " +
            "1C DD 49 EF CF E6 6B 53 23 44 B5 7D 71 8C 6D 54 " +
            "BF 9C 3D F6 8E 57 F4 8F",
            // After Iota 
            "12 11 E7 46 1C E9 85 CA B6 E0 92 AC A1 DA EF 90 " +
            "BF 6D F5 B7 CD 09 19 CE F8 0A 11 0A 4E A5 E5 3A " +
            "7E 70 44 B1 BA E4 37 BC FC EF BB A9 6F CE ED 33 " +
            "A5 ED 8C 94 76 D3 65 B2 07 6C F4 83 7C 4E 9E 7A " +
            "BD C3 8F 0F 76 16 01 CB 2C 80 AC 65 0A CF 57 55 " +
            "66 4B 87 69 91 97 79 13 5D CB F2 A3 9A 8F 28 25 " +
            "69 66 41 65 93 66 D8 08 46 89 79 2D EF DB E9 56 " +
            "0D 39 EB 9F 6F 22 0B A8 21 51 C8 DA 3C 36 D7 02 " +
            "BD 1E 12 9F 4A 71 B8 CE 6C 0F 73 49 55 6C 8E 41 " +
            "3B B2 4E DD CA BD 63 FB C4 40 90 A6 F5 DB D1 5B " +
            "5A BC 53 A2 2A AB 97 20 51 11 56 E4 20 87 38 7F " +
            "1C DD 49 EF CF E6 6B 53 23 44 B5 7D 71 8C 6D 54 " +
            "BF 9C 3D F6 8E 57 F4 8F",
            // Round #4
            // After Theta
            "73 97 18 9C F7 8C 8F 33 06 D3 27 BC 24 A0 CB 05 " +
            "2B C8 64 47 33 CB 3D 68 10 95 37 CA BE 05 C2 BE " +
            "82 77 D8 C5 3F E6 96 24 9D 69 44 73 84 AB E7 CA " +
            "15 DE 39 84 F3 A9 41 27 93 C9 65 73 82 8C BA DC " +
            "55 5C A9 CF 86 B6 26 4F D0 87 30 11 8F CD F6 CD " +
            "07 CD 78 B3 7A F2 73 EA ED F8 47 B3 1F F5 0C B0 " +
            "FD C3 D0 95 6D A4 FC AE AE 16 5F ED 1F 7B CE D2 " +
            "F1 3E 77 EB EA 20 AA 30 40 D7 37 00 D7 53 DD FB " +
            "0D 2D A7 8F CF 0B 9C 5B F8 AA E2 B9 AB AE AA E7 " +
            "D3 2D 68 1D 3A 1D 44 7F 38 47 0C D2 70 D9 70 C3 " +
            "3B 3A AC 78 C1 CE 9D D9 E1 22 E3 F4 A5 FD 1C EA " +
            "88 78 D8 1F 31 24 4F F5 CB DB 93 BD 81 2C 4A D0 " +
            "43 9B A1 82 0B 55 55 17",
            // After Rho
            "73 97 18 9C F7 8C 8F 33 0C A6 4F 78 49 40 97 0B " +
            "0A 32 D9 D1 CC 72 0F DA 5B 20 EC 0B 51 79 A3 EC " +
            "31 B7 24 11 BC C3 2E FE 47 B8 7A AE DC 99 46 34 " +
            "43 38 9F 1A 74 52 E1 9D F7 64 72 D9 9C 20 A3 2E " +
            "AE D4 67 43 5B 93 A7 2A 6C DF 0C 7D 08 13 F1 D8 " +
            "3F 68 C6 9B D5 93 9F 53 C0 B6 E3 1F CD 7E D4 33 " +
            "AE 6C 23 E5 77 ED 1F 86 F6 9C A5 5D 2D BE DA 3F " +
            "75 75 10 55 98 78 9F BB 00 AE A7 BA F7 81 AE 6F " +
            "F4 F1 79 81 73 AB A1 E5 D5 73 7C 55 F1 DC 55 57 " +
            "83 E8 6F BA 05 AD 43 A7 C3 38 47 0C D2 70 D9 70 " +
            "77 66 EF E8 B0 E2 05 3B 87 8B 8C D3 97 F6 73 A8 " +
            "11 0F FB 23 86 E4 A9 1E DB 93 BD 81 2C 4A D0 CB " +
            "D5 C5 D0 66 A8 E0 42 55",
            // After Pi
            "73 97 18 9C F7 8C 8F 33 43 38 9F 1A 74 52 E1 9D " +
            "AE 6C 23 E5 77 ED 1F 86 83 E8 6F BA 05 AD 43 A7 " +
            "D5 C5 D0 66 A8 E0 42 55 5B 20 EC 0B 51 79 A3 EC " +
            "6C DF 0C 7D 08 13 F1 D8 3F 68 C6 9B D5 93 9F 53 " +
            "F4 F1 79 81 73 AB A1 E5 11 0F FB 23 86 E4 A9 1E " +
            "0C A6 4F 78 49 40 97 0B F7 64 72 D9 9C 20 A3 2E " +
            "F6 9C A5 5D 2D BE DA 3F C3 38 47 0C D2 70 D9 70 " +
            "77 66 EF E8 B0 E2 05 3B 31 B7 24 11 BC C3 2E FE " +
            "47 B8 7A AE DC 99 46 34 C0 B6 E3 1F CD 7E D4 33 " +
            "D5 73 7C 55 F1 DC 55 57 DB 93 BD 81 2C 4A D0 CB " +
            "0A 32 D9 D1 CC 72 0F DA AE D4 67 43 5B 93 A7 2A " +
            "75 75 10 55 98 78 9F BB 00 AE A7 BA F7 81 AE 6F " +
            "87 8B 8C D3 97 F6 73 A8",
            // After Chi
            "DF D3 38 79 F4 21 91 31 42 B8 D3 00 74 52 A1 BC " +
            "FA 69 B3 A1 DF AD 1F D6 A1 FA 67 22 52 A1 CE 85 " +
            "D5 ED 57 64 A8 B2 22 D9 48 00 2E 89 84 F9 AD EF " +
            "AC 4E 35 7D 2A 3B D1 7C 3E 66 44 B9 51 D7 97 49 " +
            "BE D1 7D 89 22 B2 A3 05 35 D0 FB 57 8E E6 F9 0E " +
            "0C 3E CA 7C 68 DE CF 1A F6 44 30 D9 4E 60 A2 6E " +
            "C2 DA 0D BD 0D 3C DE 34 CB B8 47 1C 9B 70 4B 70 " +
            "84 26 DF 69 24 C2 25 1F B1 B1 A5 00 BD A5 BE FD " +
            "52 F9 66 EE EC 19 47 70 CA 36 62 9F C1 7C 54 BB " +
            "F5 57 7C 45 61 5D 7B 63 9D 9B E7 2F 6C 52 90 CB " +
            "5B 13 C9 C5 4C 1A 17 4B AE 5E C0 E9 3C 12 87 6E " +
            "F2 74 18 14 98 0E CE 3B 08 9E F6 BA BF 81 A2 3D " +
            "23 4F AA D1 84 77 D3 88",
            // After Iota 
            "54 53 38 79 F4 21 91 31 42 B8 D3 00 74 52 A1 BC " +
            "FA 69 B3 A1 DF AD 1F D6 A1 FA 67 22 52 A1 CE 85 " +
            "D5 ED 57 64 A8 B2 22 D9 48 00 2E 89 84 F9 AD EF " +
            "AC 4E 35 7D 2A 3B D1 7C 3E 66 44 B9 51 D7 97 49 " +
            "BE D1 7D 89 22 B2 A3 05 35 D0 FB 57 8E E6 F9 0E " +
            "0C 3E CA 7C 68 DE CF 1A F6 44 30 D9 4E 60 A2 6E " +
            "C2 DA 0D BD 0D 3C DE 34 CB B8 47 1C 9B 70 4B 70 " +
            "84 26 DF 69 24 C2 25 1F B1 B1 A5 00 BD A5 BE FD " +
            "52 F9 66 EE EC 19 47 70 CA 36 62 9F C1 7C 54 BB " +
            "F5 57 7C 45 61 5D 7B 63 9D 9B E7 2F 6C 52 90 CB " +
            "5B 13 C9 C5 4C 1A 17 4B AE 5E C0 E9 3C 12 87 6E " +
            "F2 74 18 14 98 0E CE 3B 08 9E F6 BA BF 81 A2 3D " +
            "23 4F AA D1 84 77 D3 88",
            // Round #5
            // After Theta
            "47 B7 E6 9B 9F 97 08 DA C4 59 62 14 29 82 63 99 " +
            "4D C8 6D 93 75 D1 F2 3B 2A F2 9A 44 5D F2 79 B9 " +
            "08 28 E1 BF 4F 7E 68 93 5B E4 F0 6B EF 4F 34 04 " +
            "2A AF 84 69 77 EB 13 59 89 C7 9A 8B FB AB 7A A4 " +
            "35 D9 80 EF 2D E1 14 39 E8 15 4D 8C 69 2A B3 44 " +
            "1F DA 14 9E 03 68 56 F1 70 A5 81 CD 13 B0 60 4B " +
            "75 7B D3 8F A7 40 33 D9 40 B0 BA 7A 94 23 FC 4C " +
            "59 E3 69 B2 C3 0E 6F 55 A2 55 7B E2 D6 13 27 16 " +
            "D4 18 D7 FA B1 C9 85 55 7D 97 BC AD 6B 00 B9 56 " +
            "7E 5F 81 23 6E 0E CC 5F 40 5E 51 F4 8B 9E DA 81 " +
            "48 F7 17 27 27 AC 8E A0 28 BF 71 FD 61 C2 45 4B " +
            "45 D5 C6 26 32 72 23 D6 83 96 0B DC B0 D2 15 01 " +
            "FE 8A 1C 0A 63 BB 99 C2",
            // After Rho
            "47 B7 E6 9B 9F 97 08 DA 89 B3 C4 28 52 04 C7 32 " +
            "13 72 DB 64 5D B4 FC 4E 25 9F 97 AB 22 AF 49 D4 " +
            "F2 43 9B 44 40 09 FF 7D F6 FE 44 43 B0 45 0E BF " +
            "98 76 B7 3E 91 A5 F2 4A 69 E2 B1 E6 E2 FE AA 1E " +
            "6C C0 F7 96 70 8A 9C 9A 32 4B 84 5E D1 C4 98 A6 " +
            "FF D0 A6 F0 1C 40 B3 8A 2D C1 95 06 36 4F C0 82 " +
            "7E 3C 05 9A C9 AE DB 9B 47 F8 99 80 60 75 F5 28 " +
            "D9 61 87 B7 AA AC F1 34 C4 AD 27 4E 2C 44 AB F6 " +
            "5A 3F 36 B9 B0 8A 1A E3 5C AB BE 4B DE D6 35 80 " +
            "81 F9 CB EF 2B 70 C4 CD 81 40 5E 51 F4 8B 9E DA " +
            "3A 82 22 DD 5F 9C 9C B0 A1 FC C6 F5 87 09 17 2D " +
            "A8 DA D8 44 46 6E C4 BA 96 0B DC B0 D2 15 01 83 " +
            "A6 B0 BF 22 87 C2 D8 6E",
            // After Pi
            "47 B7 E6 9B 9F 97 08 DA 98 76 B7 3E 91 A5 F2 4A " +
            "7E 3C 05 9A C9 AE DB 9B 81 F9 CB EF 2B 70 C4 CD " +
            "A6 B0 BF 22 87 C2 D8 6E 25 9F 97 AB 22 AF 49 D4 " +
            "32 4B 84 5E D1 C4 98 A6 FF D0 A6 F0 1C 40 B3 8A " +
            "5A 3F 36 B9 B0 8A 1A E3 A8 DA D8 44 46 6E C4 BA " +
            "89 B3 C4 28 52 04 C7 32 69 E2 B1 E6 E2 FE AA 1E " +
            "47 F8 99 80 60 75 F5 28 81 40 5E 51 F4 8B 9E DA " +
            "3A 82 22 DD 5F 9C 9C B0 F2 43 9B 44 40 09 FF 7D " +
            "F6 FE 44 43 B0 45 0E BF 2D C1 95 06 36 4F C0 82 " +
            "5C AB BE 4B DE D6 35 80 96 0B DC B0 D2 15 01 83 " +
            "13 72 DB 64 5D B4 FC 4E 6C C0 F7 96 70 8A 9C 9A " +
            "D9 61 87 B7 AA AC F1 34 C4 AD 27 4E 2C 44 AB F6 " +
            "A1 FC C6 F5 87 09 17 2D",
            // After Chi
            "21 BF E6 1B D7 9D 01 4B 19 B7 7D 5B B3 F5 F6 0E " +
            "58 3C 31 9A 4D 2C C3 B9 C0 FE 8B 76 33 65 C4 5D " +
            "3E F0 AE 06 87 E2 2A 6E E8 0F B5 0B 2E AF 6A DC " +
            "32 64 94 57 71 4E 90 C7 5F 10 6E B4 5A 24 77 92 " +
            "5F 3A 31 12 90 0B 13 A7 BA 9A D8 10 97 2E 54 98 " +
            "8F AB CC 28 52 05 92 12 E9 E2 F7 B7 76 74 A0 CC " +
            "7D 7A B9 0C 6B 61 F5 08 00 71 9A 71 F4 8B DD D8 " +
            "5A C2 13 1B FF 66 B4 BC FB 42 0A 40 46 03 3F 7D " +
            "A6 D4 6E 0A 78 D5 3B BF AF C1 D5 B6 36 4E C0 81 " +
            "3C EB BD 0F DE DE CB FC 92 B7 98 B3 62 51 01 01 " +
            "82 53 DB 45 D7 90 9D 6A 68 4C D7 DE 74 CA 96 58 " +
            "F8 31 47 06 29 A5 E5 3D D6 AF 3E 4E 74 F0 43 B4 " +
            "CD 7C E2 67 A7 03 17 BD",
            // After Iota 
            "20 BF E6 9B D7 9D 01 4B 19 B7 7D 5B B3 F5 F6 0E " +
            "58 3C 31 9A 4D 2C C3 B9 C0 FE 8B 76 33 65 C4 5D " +
            "3E F0 AE 06 87 E2 2A 6E E8 0F B5 0B 2E AF 6A DC " +
            "32 64 94 57 71 4E 90 C7 5F 10 6E B4 5A 24 77 92 " +
            "5F 3A 31 12 90 0B 13 A7 BA 9A D8 10 97 2E 54 98 " +
            "8F AB CC 28 52 05 92 12 E9 E2 F7 B7 76 74 A0 CC " +
            "7D 7A B9 0C 6B 61 F5 08 00 71 9A 71 F4 8B DD D8 " +
            "5A C2 13 1B FF 66 B4 BC FB 42 0A 40 46 03 3F 7D " +
            "A6 D4 6E 0A 78 D5 3B BF AF C1 D5 B6 36 4E C0 81 " +
            "3C EB BD 0F DE DE CB FC 92 B7 98 B3 62 51 01 01 " +
            "82 53 DB 45 D7 90 9D 6A 68 4C D7 DE 74 CA 96 58 " +
            "F8 31 47 06 29 A5 E5 3D D6 AF 3E 4E 74 F0 43 B4 " +
            "CD 7C E2 67 A7 03 17 BD",
            // Round #6
            // After Theta
            "B8 8E B6 9D 8D C4 0A 79 7C F1 DA C2 4E 55 64 A2 " +
            "BE 77 D1 5C 0F 6B AD 8E EE 9F C1 56 05 17 19 2F " +
            "36 15 91 28 0F 61 1F 20 70 3E E5 0D 74 F6 61 EE " +
            "57 22 33 CE 8C EE 02 6B B9 5B 8E 72 18 63 19 A5 " +
            "71 5B 7B 32 A6 79 CE D5 B2 7F E7 3E 1F AD 61 D6 " +
            "17 9A 9C 2E 08 5C 99 20 8C A4 50 2E 8B D4 32 60 " +
            "9B 31 59 CA 29 26 9B 3F 2E 10 D0 51 C2 F9 00 AA " +
            "52 27 2C 35 77 E5 81 F2 63 73 5A 46 1C 5A 34 4F " +
            "C3 92 C9 93 85 75 A9 13 49 8A 35 70 74 09 AE B6 " +
            "12 8A F7 2F E8 AC 16 8E 9A 52 A7 9D EA D2 34 4F " +
            "1A 62 8B 43 8D C9 96 58 0D 0A 70 47 89 6A 04 F4 " +
            "1E 7A A7 C0 6B E2 8B 0A F8 CE 74 6E 42 82 9E C6 " +
            "C5 99 DD 49 2F 80 22 F3",
            // After Rho
            "B8 8E B6 9D 8D C4 0A 79 F9 E2 B5 85 9D AA C8 44 " +
            "EF 5D 34 D7 C3 5A AB A3 70 91 F1 E2 FE 19 6C 55 " +
            "08 FB 00 B1 A9 88 44 79 40 67 1F E6 0E E7 53 DE " +
            "E3 CC E8 2E B0 76 25 32 69 EE 96 A3 1C C6 58 46 " +
            "AD 3D 19 D3 3C E7 EA B8 1A 66 2D FB 77 EE F3 D1 " +
            "B9 D0 E4 74 41 E0 CA 04 80 31 92 42 B9 2C 52 CB " +
            "52 4E 31 D9 FC D9 8C C9 F3 01 54 5D 20 A0 A3 84 " +
            "9A BB F2 40 79 A9 13 96 8C 38 B4 68 9E C6 E6 B4 " +
            "79 B2 B0 2E 75 62 58 32 57 DB 24 C5 1A 38 BA 04 " +
            "D5 C2 51 42 F1 FE 05 9D 4F 9A 52 A7 9D EA D2 34 " +
            "5B 62 69 88 2D 0E 35 26 37 28 C0 1D 25 AA 11 D0 " +
            "43 EF 14 78 4D 7C 51 C1 CE 74 6E 42 82 9E C6 F8 " +
            "C8 7C 71 66 77 D2 0B A0",
            // After Pi
            "B8 8E B6 9D 8D C4 0A 79 E3 CC E8 2E B0 76 25 32 " +
            "52 4E 31 D9 FC D9 8C C9 D5 C2 51 42 F1 FE 05 9D " +
            "C8 7C 71 66 77 D2 0B A0 70 91 F1 E2 FE 19 6C 55 " +
            "1A 66 2D FB 77 EE F3 D1 B9 D0 E4 74 41 E0 CA 04 " +
            "79 B2 B0 2E 75 62 58 32 43 EF 14 78 4D 7C 51 C1 " +
            "F9 E2 B5 85 9D AA C8 44 69 EE 96 A3 1C C6 58 46 " +
            "F3 01 54 5D 20 A0 A3 84 4F 9A 52 A7 9D EA D2 34 " +
            "5B 62 69 88 2D 0E 35 26 08 FB 00 B1 A9 88 44 79 " +
            "40 67 1F E6 0E E7 53 DE 80 31 92 42 B9 2C 52 CB " +
            "57 DB 24 C5 1A 38 BA 04 CE 74 6E 42 82 9E C6 F8 " +
            "EF 5D 34 D7 C3 5A AB A3 AD 3D 19 D3 3C E7 EA B8 " +
            "9A BB F2 40 79 A9 13 96 8C 38 B4 68 9E C6 E6 B4 " +
            "37 28 C0 1D 25 AA 11 D0",
            // After Chi
            "A8 8C A7 4C C1 4D 82 B0 66 4C A8 2C B1 50 24 26 " +
            "5A 72 11 FD FA D9 86 E9 E5 40 D7 DB 79 FA 05 C4 " +
            "8B 3C 39 44 47 E0 2E A2 D1 01 31 E6 FE 19 64 51 " +
            "5A 44 3D F1 43 EC E3 E3 BB 9D E0 24 49 FC CB C5 " +
            "49 A2 51 AC C7 63 74 26 49 89 18 61 4C 9A C2 41 " +
            "6B E3 F5 D9 BD 8A 6B C4 65 74 94 01 81 8C 08 76 " +
            "E3 61 7D 55 00 A4 86 86 EF 1A C6 A2 0D 4A 1A 74 " +
            "5B 6E 6B AA 2D 4A 25 24 88 EB 80 B1 18 80 44 78 " +
            "17 AD 3B 63 0C F7 FB DA 08 15 D8 40 39 AA 16 33 " +
            "57 50 24 74 33 38 BA 05 8E 70 71 04 84 F9 D5 7E " +
            "FD DF D6 D7 82 52 BA A5 A9 3D 1D FB BA A1 0E 98 " +
            "A9 BB B2 55 58 81 02 D6 44 6D 80 AA 5C 96 4C 97 " +
            "37 08 C9 1D 19 0F 51 C8",
            // After Iota 
            "29 0C A7 CC C1 4D 82 30 66 4C A8 2C B1 50 24 26 " +
            "5A 72 11 FD FA D9 86 E9 E5 40 D7 DB 79 FA 05 C4 " +
            "8B 3C 39 44 47 E0 2E A2 D1 01 31 E6 FE 19 64 51 " +
            "5A 44 3D F1 43 EC E3 E3 BB 9D E0 24 49 FC CB C5 " +
            "49 A2 51 AC C7 63 74 26 49 89 18 61 4C 9A C2 41 " +
            "6B E3 F5 D9 BD 8A 6B C4 65 74 94 01 81 8C 08 76 " +
            "E3 61 7D 55 00 A4 86 86 EF 1A C6 A2 0D 4A 1A 74 " +
            "5B 6E 6B AA 2D 4A 25 24 88 EB 80 B1 18 80 44 78 " +
            "17 AD 3B 63 0C F7 FB DA 08 15 D8 40 39 AA 16 33 " +
            "57 50 24 74 33 38 BA 05 8E 70 71 04 84 F9 D5 7E " +
            "FD DF D6 D7 82 52 BA A5 A9 3D 1D FB BA A1 0E 98 " +
            "A9 BB B2 55 58 81 02 D6 44 6D 80 AA 5C 96 4C 97 " +
            "37 08 C9 1D 19 0F 51 C8",
            // Round #7
            // After Theta
            "C6 76 1A D2 F0 46 BB A3 C6 D7 51 8A 0C 09 E8 C1 " +
            "1D 14 FF AE 87 44 86 11 06 26 D4 6F DC DD 41 69 " +
            "17 4C B6 65 AA 85 55 56 3E 7B 8C F8 CF 12 5D C2 " +
            "FA DF C4 57 FE B5 2F 04 FC FB 0E 77 34 61 CB 3D " +
            "AA C4 52 18 62 44 30 8B D5 F9 97 40 A1 FF B9 B5 " +
            "84 99 48 C7 8C 81 52 57 C5 EF 6D A7 3C D5 C4 91 " +
            "A4 07 93 06 7D 39 86 7E 0C 7C C5 16 A8 6D 5E D9 " +
            "C7 1E E4 8B C0 2F 5E D0 67 91 3D AF 29 8B 7D EB " +
            "B7 36 C2 C5 B1 AE 37 3D 4F 73 36 13 44 37 16 CB " +
            "B4 36 27 C0 96 1F FE A8 12 00 FE 25 69 9C AE 8A " +
            "12 A5 6B C9 B3 59 83 36 09 A6 E4 5D 07 F8 C2 7F " +
            "EE DD 5C 06 25 1C 02 2E A7 0B 83 1E F9 B1 08 3A " +
            "AB 78 46 3C F4 6A 2A 3C",
            // After Rho
            "C6 76 1A D2 F0 46 BB A3 8D AF A3 14 19 12 D0 83 " +
            "07 C5 BF EB 21 91 61 44 DD 1D 94 66 60 42 FD C6 " +
            "2D AC B2 BA 60 B2 2D 53 FF 2C D1 25 EC B3 C7 88 " +
            "7C E5 5F FB 42 A0 FF 4D 0F FF BE C3 1D 4D D8 72 " +
            "62 29 0C 31 22 98 45 55 9F 5B 5B 9D 7F 09 14 FA " +
            "22 CC 44 3A 66 0C 94 BA 47 16 BF B7 9D F2 54 13 " +
            "34 E8 CB 31 F4 23 3D 98 DB BC B2 19 F8 8A 2D 50 " +
            "45 E0 17 2F E8 63 0F F2 5E 53 16 FB D6 CF 22 7B " +
            "B8 38 D6 F5 A6 E7 D6 46 8B E5 A7 39 9B 09 A2 1B " +
            "C3 1F 95 D6 E6 04 D8 F2 8A 12 00 FE 25 69 9C AE " +
            "0D DA 48 94 AE 25 CF 66 25 98 92 77 1D E0 0B FF " +
            "BD 9B CB A0 84 43 C0 C5 0B 83 1E F9 B1 08 3A A7 " +
            "0A CF 2A 9E 11 0F BD 9A",
            // After Pi
            "C6 76 1A D2 F0 46 BB A3 7C E5 5F FB 42 A0 FF 4D " +
            "34 E8 CB 31 F4 23 3D 98 C3 1F 95 D6 E6 04 D8 F2 " +
            "0A CF 2A 9E 11 0F BD 9A DD 1D 94 66 60 42 FD C6 " +
            "9F 5B 5B 9D 7F 09 14 FA 22 CC 44 3A 66 0C 94 BA " +
            "B8 38 D6 F5 A6 E7 D6 46 BD 9B CB A0 84 43 C0 C5 " +
            "8D AF A3 14 19 12 D0 83 0F FF BE C3 1D 4D D8 72 " +
            "DB BC B2 19 F8 8A 2D 50 8A 12 00 FE 25 69 9C AE " +
            "0D DA 48 94 AE 25 CF 66 2D AC B2 BA 60 B2 2D 53 " +
            "FF 2C D1 25 EC B3 C7 88 47 16 BF B7 9D F2 54 13 " +
            "8B E5 A7 39 9B 09 A2 1B 0B 83 1E F9 B1 08 3A A7 " +
            "07 C5 BF EB 21 91 61 44 62 29 0C 31 22 98 45 55 " +
            "45 E0 17 2F E8 63 0F F2 5E 53 16 FB D6 CF 22 7B " +
            "25 98 92 77 1D E0 0B FF",
            // After Chi
            "C6 7E 9A D2 44 45 BB 33 BF F2 4B 3D 40 A4 3F 2F " +
            "3C 28 E1 39 E5 28 18 90 07 2F 85 96 06 44 DA D3 " +
            "32 4E 6F B7 13 AF F9 D6 FD 99 90 44 60 46 7D C6 " +
            "07 6B C9 58 FF EA 56 BE 27 4F 4D 3A 66 0C 94 3B " +
            "F8 3C C2 B3 C6 E7 EB 44 BF D9 80 39 9B 4A C0 FD " +
            "5D AF A3 0C F9 90 F5 83 0F FD BE 25 18 2C 48 DC " +
            "DE 74 FA 19 72 8E 6E 10 0A 37 A3 FE 34 7B 8C 2F " +
            "0F 8A 54 57 AA 68 C7 16 2D BE 9C 28 71 F2 3D 40 " +
            "77 CD D1 2D EE BA 65 80 47 14 A7 77 BD F2 4C B7 " +
            "AF C9 07 3B DB BB A7 4B D9 83 5F FC 3D 09 F8 2F " +
            "02 05 AC E5 E9 F2 6B E6 78 3A 0C E1 34 14 65 5C " +
            "64 68 97 2B E1 43 06 76 5C 16 3B 73 F6 DE 42 7B " +
            "45 B0 92 67 1F E8 0F EE",
            // After Iota 
            "CF FE 9A D2 44 45 BB B3 BF F2 4B 3D 40 A4 3F 2F " +
            "3C 28 E1 39 E5 28 18 90 07 2F 85 96 06 44 DA D3 " +
            "32 4E 6F B7 13 AF F9 D6 FD 99 90 44 60 46 7D C6 " +
            "07 6B C9 58 FF EA 56 BE 27 4F 4D 3A 66 0C 94 3B " +
            "F8 3C C2 B3 C6 E7 EB 44 BF D9 80 39 9B 4A C0 FD " +
            "5D AF A3 0C F9 90 F5 83 0F FD BE 25 18 2C 48 DC " +
            "DE 74 FA 19 72 8E 6E 10 0A 37 A3 FE 34 7B 8C 2F " +
            "0F 8A 54 57 AA 68 C7 16 2D BE 9C 28 71 F2 3D 40 " +
            "77 CD D1 2D EE BA 65 80 47 14 A7 77 BD F2 4C B7 " +
            "AF C9 07 3B DB BB A7 4B D9 83 5F FC 3D 09 F8 2F " +
            "02 05 AC E5 E9 F2 6B E6 78 3A 0C E1 34 14 65 5C " +
            "64 68 97 2B E1 43 06 76 5C 16 3B 73 F6 DE 42 7B " +
            "45 B0 92 67 1F E8 0F EE",
            // Round #8
            // After Theta
            "A0 F7 2F 89 BF B1 F1 6D 33 5E 1E E6 5F 00 0A 8A " +
            "89 4D B1 92 2B 9F 88 11 DC 1C 0F 54 AB 87 60 51 " +
            "B4 53 85 8B 40 34 6A FE 92 90 25 1F 9B B2 37 18 " +
            "8B C7 9C 83 E0 4E 63 1B 92 2A 1D 91 A8 BB 04 BA " +
            "23 0F 48 71 6B 24 51 C6 39 C4 6A 05 C8 D1 53 D5 " +
            "32 A6 16 57 02 64 BF 5D 83 51 EB FE 07 88 7D 79 " +
            "6B 11 AA B2 BC 39 FE 91 D1 04 29 3C 99 B8 36 AD " +
            "89 97 BE 6B F9 F3 54 3E 42 B7 29 73 8A 06 77 9E " +
            "FB 61 84 F6 F1 1E 50 25 F2 71 F7 DC 73 45 DC 36 " +
            "74 FA 8D F9 76 78 1D C9 5F 9E B5 C0 6E 92 6B 07 " +
            "6D 0C 19 BE 12 06 21 38 F4 96 59 3A 2B B0 50 F9 " +
            "D1 0D C7 80 2F F4 96 F7 87 25 B1 B1 5B 1D F8 F9 " +
            "C3 AD 78 5B 4C 73 9C C6",
            // After Rho
            "A0 F7 2F 89 BF B1 F1 6D 67 BC 3C CC BF 00 14 14 " +
            "62 53 AC E4 CA 27 62 44 7A 08 16 C5 CD F1 40 B5 " +
            "A2 51 F3 A7 9D 2A 5C 04 B1 29 7B 83 21 09 59 F2 " +
            "39 08 EE 34 B6 B1 78 CC AE A4 4A 47 24 EA 2E 81 " +
            "07 A4 B8 35 92 28 E3 91 3D 55 9D 43 AC 56 80 1C " +
            "92 31 B5 B8 12 20 FB ED E5 0D 46 AD FB 1F 20 F6 " +
            "95 E5 CD F1 8F 5C 8B 50 71 6D 5A A3 09 52 78 32 " +
            "B5 FC 79 2A 9F C4 4B DF E6 14 0D EE 3C 85 6E 53 " +
            "D0 3E DE 03 AA 64 3F 8C 6E 1B F9 B8 7B EE B9 22 " +
            "AF 23 99 4E BF 31 DF 0E 07 5F 9E B5 C0 6E 92 6B " +
            "84 E0 B4 31 64 F8 4A 18 D3 5B 66 E9 AC C0 42 E5 " +
            "BA E1 18 F0 85 DE F2 3E 25 B1 B1 5B 1D F8 F9 87 " +
            "A7 F1 70 2B DE 16 D3 1C",
            // After Pi
            "A0 F7 2F 89 BF B1 F1 6D 39 08 EE 34 B6 B1 78 CC " +
            "95 E5 CD F1 8F 5C 8B 50 AF 23 99 4E BF 31 DF 0E " +
            "A7 F1 70 2B DE 16 D3 1C 7A 08 16 C5 CD F1 40 B5 " +
            "3D 55 9D 43 AC 56 80 1C 92 31 B5 B8 12 20 FB ED " +
            "D0 3E DE 03 AA 64 3F 8C BA E1 18 F0 85 DE F2 3E " +
            "67 BC 3C CC BF 00 14 14 AE A4 4A 47 24 EA 2E 81 " +
            "71 6D 5A A3 09 52 78 32 07 5F 9E B5 C0 6E 92 6B " +
            "84 E0 B4 31 64 F8 4A 18 A2 51 F3 A7 9D 2A 5C 04 " +
            "B1 29 7B 83 21 09 59 F2 E5 0D 46 AD FB 1F 20 F6 " +
            "6E 1B F9 B8 7B EE B9 22 25 B1 B1 5B 1D F8 F9 87 " +
            "62 53 AC E4 CA 27 62 44 07 A4 B8 35 92 28 E3 91 " +
            "B5 FC 79 2A 9F C4 4B DF E6 14 0D EE 3C 85 6E 53 " +
            "D3 5B 66 E9 AC C0 42 E5",
            // After Chi
            "24 12 2E 48 B6 FD 72 7D 13 0A FE 3A 86 90 2C C2 " +
            "95 35 AD D0 CF 5A 8B 40 AF 25 96 CE 9E 90 FF 6F " +
            "BE F9 B0 1F DE 16 DB 9C F8 28 36 7D DF D1 3B 54 " +
            "7D 5B D7 40 04 12 84 1C B8 F0 B5 48 17 BA 3B DF " +
            "90 36 D8 06 E2 45 3F 0D BF B4 91 F2 A5 D8 72 36 " +
            "36 F5 2C 6C B6 10 44 26 A8 B6 CE 53 E4 C6 AC C8 " +
            "F1 CD 7A A3 2D C2 30 22 64 43 96 79 5B 6E 86 6F " +
            "0C E0 F6 32 64 12 60 99 E6 55 F7 8B 47 3C 7C 00 " +
            "BB 3B C2 93 21 E9 C0 F2 E4 AD 46 EE FF 0F 60 73 " +
            "EC 5B BB 1C FB EC BD 22 34 99 B9 5B 3D F9 F8 75 " +
            "D2 0B ED EE C7 E3 6A 0A 45 A4 BC F1 B2 29 C7 91 " +
            "A4 B7 1B 2B 1F 84 4B 7B C6 14 85 EA 7E A2 4E 53 " +
            "D6 FF 76 F8 BC C8 C3 74",
            // After Iota 
            "AE 12 2E 48 B6 FD 72 7D 13 0A FE 3A 86 90 2C C2 " +
            "95 35 AD D0 CF 5A 8B 40 AF 25 96 CE 9E 90 FF 6F " +
            "BE F9 B0 1F DE 16 DB 9C F8 28 36 7D DF D1 3B 54 " +
            "7D 5B D7 40 04 12 84 1C B8 F0 B5 48 17 BA 3B DF " +
            "90 36 D8 06 E2 45 3F 0D BF B4 91 F2 A5 D8 72 36 " +
            "36 F5 2C 6C B6 10 44 26 A8 B6 CE 53 E4 C6 AC C8 " +
            "F1 CD 7A A3 2D C2 30 22 64 43 96 79 5B 6E 86 6F " +
            "0C E0 F6 32 64 12 60 99 E6 55 F7 8B 47 3C 7C 00 " +
            "BB 3B C2 93 21 E9 C0 F2 E4 AD 46 EE FF 0F 60 73 " +
            "EC 5B BB 1C FB EC BD 22 34 99 B9 5B 3D F9 F8 75 " +
            "D2 0B ED EE C7 E3 6A 0A 45 A4 BC F1 B2 29 C7 91 " +
            "A4 B7 1B 2B 1F 84 4B 7B C6 14 85 EA 7E A2 4E 53 " +
            "D6 FF 76 F8 BC C8 C3 74",
            // Round #9
            // After Theta
            "31 29 04 A3 C2 19 87 A5 7E BE AE FA F2 21 60 AC " +
            "4F 73 F8 14 7E 35 E3 CC ED A0 98 C8 B7 E2 B1 BF " +
            "67 C4 0B 20 C2 25 59 EA 67 13 1C 96 AB 35 CE 8C " +
            "10 EF 87 80 70 A3 C8 72 62 B6 E0 8C A6 D5 53 53 " +
            "D2 B3 D6 00 CB 37 71 DD 66 89 2A CD B9 EB F0 40 " +
            "A9 CE 06 87 C2 F4 B1 FE C5 02 9E 93 90 77 E0 A6 " +
            "2B 8B 2F 67 9C AD 58 AE 26 C6 98 7F 72 1C C8 BF " +
            "D5 DD 4D 0D 78 21 E2 EF 79 6E DD 60 33 D8 89 D8 " +
            "D6 8F 92 53 55 58 8C 9C 3E EB 13 2A 4E 60 08 FF " +
            "AE DE B5 1A D2 9E F3 F2 ED A4 02 64 21 CA 7A 03 " +
            "4D 30 C7 05 B3 07 9F D2 28 10 EC 31 C6 98 8B FF " +
            "7E F1 4E EF AE EB 23 F7 84 91 8B EC 57 D0 00 83 " +
            "0F C2 CD C7 A0 FB 41 02",
            // After Rho
            "31 29 04 A3 C2 19 87 A5 FD 7C 5D F5 E5 43 C0 58 " +
            "D3 1C 3E 85 5F CD 38 F3 2B 1E FB DB 0E 8A 89 7C " +
            "2E C9 52 3F 23 5E 00 11 B9 5A E3 CC 78 36 C1 61 " +
            "08 08 37 8A 2C 07 F1 7E 94 98 2D 38 A3 69 F5 D4 " +
            "59 6B 80 E5 9B B8 6E E9 0E 0F 64 96 A8 D2 9C BB " +
            "4F 75 36 38 14 A6 8F F5 9B 16 0B 78 4E 42 DE 81 " +
            "39 E3 6C C5 72 5D 59 7C 38 90 7F 4D 8C 31 FF E4 " +
            "06 BC 10 F1 F7 EA EE A6 C1 66 B0 13 B1 F3 DC BA " +
            "72 AA 0A 8B 91 D3 FA 51 84 7F 9F F5 09 15 27 30 " +
            "73 5E DE D5 BB 56 43 DA 03 ED A4 02 64 21 CA 7A " +
            "7C 4A 37 C1 1C 17 CC 1E A3 40 B0 C7 18 63 2E FE " +
            "2F DE E9 DD 75 7D E4 DE 91 8B EC 57 D0 00 83 84 " +
            "90 C0 83 70 F3 31 E8 7E",
            // After Pi
            "31 29 04 A3 C2 19 87 A5 08 08 37 8A 2C 07 F1 7E " +
            "39 E3 6C C5 72 5D 59 7C 73 5E DE D5 BB 56 43 DA " +
            "90 C0 83 70 F3 31 E8 7E 2B 1E FB DB 0E 8A 89 7C " +
            "0E 0F 64 96 A8 D2 9C BB 4F 75 36 38 14 A6 8F F5 " +
            "72 AA 0A 8B 91 D3 FA 51 2F DE E9 DD 75 7D E4 DE " +
            "FD 7C 5D F5 E5 43 C0 58 94 98 2D 38 A3 69 F5 D4 " +
            "38 90 7F 4D 8C 31 FF E4 03 ED A4 02 64 21 CA 7A " +
            "7C 4A 37 C1 1C 17 CC 1E 2E C9 52 3F 23 5E 00 11 " +
            "B9 5A E3 CC 78 36 C1 61 9B 16 0B 78 4E 42 DE 81 " +
            "84 7F 9F F5 09 15 27 30 91 8B EC 57 D0 00 83 84 " +
            "D3 1C 3E 85 5F CD 38 F3 59 6B 80 E5 9B B8 6E E9 " +
            "06 BC 10 F1 F7 EA EE A6 C1 66 B0 13 B1 F3 DC BA " +
            "A3 40 B0 C7 18 63 2E FE",
            // After Chi
            "00 CA 4C E6 90 41 8F A5 4A 14 A5 9A A5 05 F3 FC " +
            "B9 63 6D E5 32 7C F1 58 52 77 DA 56 BB 5E 44 5B " +
            "98 C0 B0 78 DF 37 98 24 6A 6E E9 F3 1A AE 8A 38 " +
            "3E 85 6C 15 29 83 EC BB 42 21 D7 6C 70 8A 8B 7B " +
            "72 AA 18 89 9B 51 F3 71 2B DF ED D9 D5 2D F0 5D " +
            "D5 7C 0F B0 E9 53 CA 78 97 F5 AD 3A C3 69 F5 CE " +
            "44 92 6C 8C 94 27 FB E0 82 D9 EC 36 85 61 CA 3A " +
            "7C CA 17 C9 1E 3F F9 9A 2C CD 5A 0F 25 1E 1E 91 " +
            "BD 33 77 49 79 23 E0 51 8A 96 6B 7A 9E 42 5E 05 " +
            "AA 3F 8D DD 2A 4B 27 21 00 99 4D 97 88 20 42 E4 " +
            "D5 88 2E 95 3B 8F B8 F5 98 29 20 E7 9B A9 7E F1 " +
            "24 BC 10 35 FF EA CC E2 91 7A BE 13 F6 7F CC BB " +
            "AB 23 30 A7 98 53 68 F6",
            // After Iota 
            "88 CA 4C E6 90 41 8F A5 4A 14 A5 9A A5 05 F3 FC " +
            "B9 63 6D E5 32 7C F1 58 52 77 DA 56 BB 5E 44 5B " +
            "98 C0 B0 78 DF 37 98 24 6A 6E E9 F3 1A AE 8A 38 " +
            "3E 85 6C 15 29 83 EC BB 42 21 D7 6C 70 8A 8B 7B " +
            "72 AA 18 89 9B 51 F3 71 2B DF ED D9 D5 2D F0 5D " +
            "D5 7C 0F B0 E9 53 CA 78 97 F5 AD 3A C3 69 F5 CE " +
            "44 92 6C 8C 94 27 FB E0 82 D9 EC 36 85 61 CA 3A " +
            "7C CA 17 C9 1E 3F F9 9A 2C CD 5A 0F 25 1E 1E 91 " +
            "BD 33 77 49 79 23 E0 51 8A 96 6B 7A 9E 42 5E 05 " +
            "AA 3F 8D DD 2A 4B 27 21 00 99 4D 97 88 20 42 E4 " +
            "D5 88 2E 95 3B 8F B8 F5 98 29 20 E7 9B A9 7E F1 " +
            "24 BC 10 35 FF EA CC E2 91 7A BE 13 F6 7F CC BB " +
            "AB 23 30 A7 98 53 68 F6",
            // Round #10
            // After Theta
            "60 58 1D 88 CE DC DC 06 A6 7D 20 30 B6 DB BC 35 " +
            "4C 9E 64 B0 6D AD A9 64 8A 53 19 AC 04 8B 21 9C " +
            "9C BA 10 20 5C 37 DC AC 82 FC B8 9D 44 33 D9 9B " +
            "D2 EC E9 BF 3A 5D A3 72 B7 DC DE 39 2F 5B D3 47 " +
            "AA 8E DB 73 24 84 96 B6 2F A5 4D 81 56 2D B4 D5 " +
            "3D EE 5E DE B7 CE 99 DB 7B 9C 28 90 D0 B7 BA 07 " +
            "B1 6F 65 D9 CB F6 A3 DC 5A FD 2F CC 3A B4 AF FD " +
            "78 B0 B7 91 9D 3F BD 12 C4 5F 0B 61 7B 83 4D 32 " +
            "51 5A F2 E3 6A FD AF 98 7F 6B 62 2F C1 93 06 39 " +
            "72 1B 4E 27 95 9E 42 E6 04 E3 ED CF 0B 20 06 6C " +
            "3D 1A 7F FB 65 12 EB 56 74 40 A5 4D 88 77 31 38 " +
            "D1 41 19 60 A0 3B 94 DE 49 5E 7D E9 49 AA A9 7C " +
            "AF 59 90 FF 1B 53 2C 7E",
            // After Rho
            "60 58 1D 88 CE DC DC 06 4C FB 40 60 6C B7 79 6B " +
            "93 27 19 6C 5B 6B 2A 19 B0 18 C2 A9 38 95 C1 4A " +
            "BA E1 66 E5 D4 85 00 E1 49 34 93 BD 29 C8 8F DB " +
            "FE AB D3 35 2A 27 CD 9E D1 2D B7 77 CE CB D6 F4 " +
            "C7 ED 39 12 42 4B 5B 55 42 5B FD 52 DA 14 68 D5 " +
            "EE 71 F7 F2 BE 75 CE DC 1E EC 71 A2 40 42 DF EA " +
            "CB 5E B6 1F E5 8E 7D 2B 68 5F FB B5 FA 5F 98 75 " +
            "C8 CE 9F 5E 09 3C D8 DB C2 F6 06 9B 64 88 BF 16 " +
            "7E 5C AD FF 15 33 4A 4B 83 9C BF 35 B1 97 E0 49 " +
            "53 C8 5C 6E C3 E9 A4 D2 6C 04 E3 ED CF 0B 20 06 " +
            "AC 5B F5 68 FC ED 97 49 D0 01 95 36 21 DE C5 E0 " +
            "3A 28 03 0C 74 87 D2 3B 5E 7D E9 49 AA A9 7C 49 " +
            "8B DF 6B 16 E4 FF C6 14",
            // After Pi
            "60 58 1D 88 CE DC DC 06 FE AB D3 35 2A 27 CD 9E " +
            "CB 5E B6 1F E5 8E 7D 2B 53 C8 5C 6E C3 E9 A4 D2 " +
            "8B DF 6B 16 E4 FF C6 14 B0 18 C2 A9 38 95 C1 4A " +
            "42 5B FD 52 DA 14 68 D5 EE 71 F7 F2 BE 75 CE DC " +
            "7E 5C AD FF 15 33 4A 4B 3A 28 03 0C 74 87 D2 3B " +
            "4C FB 40 60 6C B7 79 6B D1 2D B7 77 CE CB D6 F4 " +
            "68 5F FB B5 FA 5F 98 75 6C 04 E3 ED CF 0B 20 06 " +
            "AC 5B F5 68 FC ED 97 49 BA E1 66 E5 D4 85 00 E1 " +
            "49 34 93 BD 29 C8 8F DB 1E EC 71 A2 40 42 DF EA " +
            "83 9C BF 35 B1 97 E0 49 5E 7D E9 49 AA A9 7C 49 " +
            "93 27 19 6C 5B 6B 2A 19 C7 ED 39 12 42 4B 5B 55 " +
            "C8 CE 9F 5E 09 3C D8 DB C2 F6 06 9B 64 88 BF 16 " +
            "D0 01 95 36 21 DE C5 E0",
            // After Chi
            "61 0C 39 82 0B 54 EC 27 EE 2B 9B 55 28 46 4D 4E " +
            "43 49 95 0F C1 98 3F 2F 33 C8 48 E6 C9 E9 BC D0 " +
            "15 7C A9 23 C4 DC C7 8C 1C 38 C0 09 1C F4 47 42 " +
            "52 57 F5 5F DB 16 68 D6 EE 51 F5 F2 DE F1 5E EC " +
            "FE 4C 6D 5E 1D 23 4B 0B 78 6B 3E 5E B6 87 FA AE " +
            "64 A9 08 E0 5C A3 71 6A D5 2D B7 3F CB CB F6 F6 " +
            "E8 04 EF B5 CA BB 0F 3C 2C A4 E3 ED CF 19 48 24 " +
            "3D 5F 42 7F 7E A5 11 DD AC 29 06 E7 94 87 50 C1 " +
            "C8 24 1D A8 98 5D AF DA 42 8D 31 EA 4A 6A C3 EA " +
            "23 1C B9 91 E5 93 E0 E9 1F 69 78 51 83 E1 F3 53 " +
            "9B 25 9F 20 52 5F AA 93 C5 DD 39 93 26 CB 7C 51 " +
            "D8 CF 0E 7A 08 6A 98 3B C1 D0 0E D3 3E A9 95 0F " +
            "94 C9 B5 24 21 DE 94 A4",
            // After Iota 
            "68 8C 39 02 0B 54 EC 27 EE 2B 9B 55 28 46 4D 4E " +
            "43 49 95 0F C1 98 3F 2F 33 C8 48 E6 C9 E9 BC D0 " +
            "15 7C A9 23 C4 DC C7 8C 1C 38 C0 09 1C F4 47 42 " +
            "52 57 F5 5F DB 16 68 D6 EE 51 F5 F2 DE F1 5E EC " +
            "FE 4C 6D 5E 1D 23 4B 0B 78 6B 3E 5E B6 87 FA AE " +
            "64 A9 08 E0 5C A3 71 6A D5 2D B7 3F CB CB F6 F6 " +
            "E8 04 EF B5 CA BB 0F 3C 2C A4 E3 ED CF 19 48 24 " +
            "3D 5F 42 7F 7E A5 11 DD AC 29 06 E7 94 87 50 C1 " +
            "C8 24 1D A8 98 5D AF DA 42 8D 31 EA 4A 6A C3 EA " +
            "23 1C B9 91 E5 93 E0 E9 1F 69 78 51 83 E1 F3 53 " +
            "9B 25 9F 20 52 5F AA 93 C5 DD 39 93 26 CB 7C 51 " +
            "D8 CF 0E 7A 08 6A 98 3B C1 D0 0E D3 3E A9 95 0F " +
            "94 C9 B5 24 21 DE 94 A4",
            // Round #11
            // After Theta
            "7A 34 DA 68 A9 8E A7 E5 77 87 93 C8 8A 38 06 4F " +
            "21 39 8B 2F C7 46 AA F9 5A 47 C9 D0 02 B8 1E EE " +
            "58 B2 08 6C 1E 82 4C 2F 0E 80 23 63 BE 2E 0C 80 " +
            "CB FB FD C2 79 68 23 D7 8C 21 EB D2 D8 2F CB 3A " +
            "97 C3 EC 68 D6 72 E9 35 35 A5 9F 11 6C D9 71 0D " +
            "76 11 EB 8A FE 79 3A A8 4C 81 BF A2 69 B5 BD F7 " +
            "8A 74 F1 95 CC 65 9A EA 45 2B 62 DB 04 48 EA 1A " +
            "70 91 E3 30 A4 FB 9A 7E BE 91 E5 8D 36 5D 1B 03 " +
            "51 88 15 35 3A 23 E4 DB 20 FD 2F CA 4C B4 56 3C " +
            "4A 93 38 A7 2E C2 42 D7 52 A7 D9 1E 59 BF 78 F0 " +
            "89 9D 7C 4A F0 85 E1 51 5C 71 31 0E 84 B5 37 50 " +
            "BA BF 10 5A 0E B4 0D ED A8 5F 8F E5 F5 F8 37 31 " +
            "D9 07 14 6B FB 80 1F 07",
            // After Rho
            "7A 34 DA 68 A9 8E A7 E5 EE 0E 27 91 15 71 0C 9E " +
            "48 CE E2 CB B1 91 6A 7E 80 EB E1 AE 75 94 0C 2D " +
            "10 64 7A C1 92 45 60 F3 E6 EB C2 00 E8 00 38 32 " +
            "2F 9C 87 36 72 BD BC DF 0E 63 C8 BA 34 F6 CB B2 " +
            "61 76 34 6B B9 F4 9A CB 1D D7 50 53 FA 19 C1 96 " +
            "B5 8B 58 57 F4 CF D3 41 DE 33 05 FE 8A A6 D5 F6 " +
            "AF 64 2E D3 54 57 A4 8B 90 D4 35 8A 56 C4 B6 09 " +
            "18 D2 7D 4D 3F B8 C8 71 1B 6D BA 36 06 7C 23 CB " +
            "A2 46 67 84 7C 3B 0A B1 2B 1E 90 FE 17 65 26 5A " +
            "58 E8 5A 69 12 E7 D4 45 F0 52 A7 D9 1E 59 BF 78 " +
            "86 47 25 76 F2 29 C1 17 71 C5 C5 38 10 D6 DE 40 " +
            "F7 17 42 CB 81 B6 A1 5D 5F 8F E5 F5 F8 37 31 A8 " +
            "C7 41 F6 01 C5 DA 3E E0",
            // After Pi
            "7A 34 DA 68 A9 8E A7 E5 2F 9C 87 36 72 BD BC DF " +
            "AF 64 2E D3 54 57 A4 8B 58 E8 5A 69 12 E7 D4 45 " +
            "C7 41 F6 01 C5 DA 3E E0 80 EB E1 AE 75 94 0C 2D " +
            "1D D7 50 53 FA 19 C1 96 B5 8B 58 57 F4 CF D3 41 " +
            "A2 46 67 84 7C 3B 0A B1 F7 17 42 CB 81 B6 A1 5D " +
            "EE 0E 27 91 15 71 0C 9E 0E 63 C8 BA 34 F6 CB B2 " +
            "90 D4 35 8A 56 C4 B6 09 F0 52 A7 D9 1E 59 BF 78 " +
            "86 47 25 76 F2 29 C1 17 10 64 7A C1 92 45 60 F3 " +
            "E6 EB C2 00 E8 00 38 32 DE 33 05 FE 8A A6 D5 F6 " +
            "2B 1E 90 FE 17 65 26 5A 5F 8F E5 F5 F8 37 31 A8 " +
            "48 CE E2 CB B1 91 6A 7E 61 76 34 6B B9 F4 9A CB " +
            "18 D2 7D 4D 3F B8 C8 71 1B 6D BA 36 06 7C 23 CB " +
            "71 C5 C5 38 10 D6 DE 40",
            // After Chi
            "FA 54 F2 A9 AD CC A7 E5 7F 14 D7 1E 70 1D EC 9B " +
            "28 65 8A D3 91 4F 8E 2B 60 DC 52 01 3A E3 55 40 " +
            "C2 C9 F3 17 97 EB 26 FA 20 E3 E9 AA 71 52 1E 6C " +
            "1F 93 77 D3 F2 29 C9 26 E0 9A 58 1C 75 4B 72 0D " +
            "A2 AE C6 A0 08 3B 06 91 EA 03 52 9A 0B BF 60 CF " +
            "7E 9A 12 91 57 71 38 97 6E 61 4A EB 3C EF C2 C2 " +
            "96 D1 35 AC B6 E4 F6 0E 98 5A A5 58 1B 09 B3 F0 " +
            "86 26 ED 5C D2 AF 02 37 08 74 7F 3F 90 E3 A5 37 " +
            "C7 E7 52 00 FD 41 1A 3A 8A B2 60 FF 62 B4 C4 56 " +
            "2B 7E 8A FE 15 25 66 09 B9 04 65 F5 90 37 29 A8 " +
            "50 4E AB CF B7 99 2A 4E 62 5B B6 59 B9 B0 B9 41 " +
            "78 52 38 45 2F 3A 14 71 13 67 98 F5 A7 7D 03 F5 " +
            "50 F5 D1 18 18 B2 4E C1",
            // After Iota 
            "F0 54 F2 29 AD CC A7 E5 7F 14 D7 1E 70 1D EC 9B " +
            "28 65 8A D3 91 4F 8E 2B 60 DC 52 01 3A E3 55 40 " +
            "C2 C9 F3 17 97 EB 26 FA 20 E3 E9 AA 71 52 1E 6C " +
            "1F 93 77 D3 F2 29 C9 26 E0 9A 58 1C 75 4B 72 0D " +
            "A2 AE C6 A0 08 3B 06 91 EA 03 52 9A 0B BF 60 CF " +
            "7E 9A 12 91 57 71 38 97 6E 61 4A EB 3C EF C2 C2 " +
            "96 D1 35 AC B6 E4 F6 0E 98 5A A5 58 1B 09 B3 F0 " +
            "86 26 ED 5C D2 AF 02 37 08 74 7F 3F 90 E3 A5 37 " +
            "C7 E7 52 00 FD 41 1A 3A 8A B2 60 FF 62 B4 C4 56 " +
            "2B 7E 8A FE 15 25 66 09 B9 04 65 F5 90 37 29 A8 " +
            "50 4E AB CF B7 99 2A 4E 62 5B B6 59 B9 B0 B9 41 " +
            "78 52 38 45 2F 3A 14 71 13 67 98 F5 A7 7D 03 F5 " +
            "50 F5 D1 18 18 B2 4E C1",
            // Round #12
            // After Theta
            "E1 FC 16 EB 9F E7 0C 86 D1 9E 75 4F E3 54 56 E3 " +
            "46 5D C2 48 5C 76 C1 94 42 28 1D A1 A9 70 C9 99 " +
            "4C D7 6A 20 55 49 BE E9 31 4B 0D 68 43 79 B5 0F " +
            "B1 19 D5 82 61 60 73 5E 8E A2 10 87 B8 72 3D B2 " +
            "80 5A 89 00 9B A8 9A 48 64 1D CB AD C9 1D F8 DC " +
            "6F 32 F6 53 65 5A 93 F4 C0 EB E8 BA AF A6 78 BA " +
            "F8 E9 7D 37 7B DD B9 B1 BA AE EA F8 88 9A 2F 29 " +
            "08 38 74 6B 10 0D 9A 24 19 DC 9B FD A2 C8 0E 54 " +
            "69 6D F0 51 6E 08 A0 42 E4 8A 28 64 AF 8D 8B E9 " +
            "09 8A C5 5E 86 B6 FA D0 37 1A FC C2 52 95 B1 BB " +
            "41 E6 4F 0D 85 B2 81 2D CC D1 14 08 2A F9 03 39 " +
            "16 6A 70 DE E2 03 5B CE 31 93 D7 55 34 EE 9F 2C " +
            "DE EB 48 2F DA 10 D6 D2",
            // After Rho
            "E1 FC 16 EB 9F E7 0C 86 A3 3D EB 9E C6 A9 AC C6 " +
            "51 97 30 12 97 5D 30 A5 0A 97 9C 29 84 D2 11 9A " +
            "4A F2 4D 67 BA 56 03 A9 36 94 57 FB 10 B3 D4 80 " +
            "2D 18 06 36 E7 15 9B 51 AC A3 28 C4 21 AE 5C 8F " +
            "AD 44 80 4D 54 4D 24 40 81 CF 4D D6 B1 DC 9A DC " +
            "7F 93 B1 9F 2A D3 9A A4 E9 02 AF A3 EB BE 9A E2 " +
            "BB D9 EB CE 8D C5 4F EF 35 5F 52 74 5D D5 F1 11 " +
            "35 88 06 4D 12 04 1C BA FB 45 91 1D A8 32 B8 37 " +
            "3E CA 0D 01 54 28 AD 0D C5 74 72 45 14 B2 D7 C6 " +
            "56 1F 3A 41 B1 D8 CB D0 BB 37 1A FC C2 52 95 B1 " +
            "06 B6 04 99 3F 35 14 CA 30 47 53 20 A8 E4 0F E4 " +
            "42 0D CE 5B 7C 60 CB D9 93 D7 55 34 EE 9F 2C 31 " +
            "B5 B4 F7 3A D2 8B 36 84",
            // After Pi
            "E1 FC 16 EB 9F E7 0C 86 2D 18 06 36 E7 15 9B 51 " +
            "BB D9 EB CE 8D C5 4F EF 56 1F 3A 41 B1 D8 CB D0 " +
            "B5 B4 F7 3A D2 8B 36 84 0A 97 9C 29 84 D2 11 9A " +
            "81 CF 4D D6 B1 DC 9A DC 7F 93 B1 9F 2A D3 9A A4 " +
            "3E CA 0D 01 54 28 AD 0D 42 0D CE 5B 7C 60 CB D9 " +
            "A3 3D EB 9E C6 A9 AC C6 AC A3 28 C4 21 AE 5C 8F " +
            "35 5F 52 74 5D D5 F1 11 BB 37 1A FC C2 52 95 B1 " +
            "06 B6 04 99 3F 35 14 CA 4A F2 4D 67 BA 56 03 A9 " +
            "36 94 57 FB 10 B3 D4 80 E9 02 AF A3 EB BE 9A E2 " +
            "C5 74 72 45 14 B2 D7 C6 93 D7 55 34 EE 9F 2C 31 " +
            "51 97 30 12 97 5D 30 A5 AD 44 80 4D 54 4D 24 40 " +
            "35 88 06 4D 12 04 1C BA FB 45 91 1D A8 32 B8 37 " +
            "30 47 53 20 A8 E4 0F E4",
            // After Chi
            "73 3D FF 23 97 27 48 28 69 1E 16 37 D7 0D 1B 41 " +
            "1A 79 2E F4 CF C6 7B EB 16 57 3A 80 BC BC C3 D2 " +
            "B9 B4 F7 2E B2 9B A5 D5 74 87 2C 20 8E D1 11 BA " +
            "81 87 41 D6 E5 F4 BF D5 3F 96 73 C5 02 93 D8 74 " +
            "36 58 1D 21 D4 BA BD 0F C3 45 8F 8D 4D 6C 41 9D " +
            "B2 61 B9 AE 9A F8 0D D6 26 83 20 4C A3 AC 58 2F " +
            "31 DF 56 75 60 F0 F1 5B 1A 3E F1 FA 02 DA 3D B5 " +
            "0A 34 04 D9 1E 33 44 C3 83 F0 E5 67 51 5A 09 CB " +
            "32 E0 07 BF 04 B3 91 84 FB 81 AA 93 01 B3 B2 D3 " +
            "8D 54 7A 06 04 F2 D4 4E A7 D3 47 AC EE 3E F8 31 " +
            "41 1F 36 12 95 5D 28 1F 67 01 11 5D FC 7F 84 45 " +
            "35 8A 44 6D 12 C0 1B 7A BA D5 B1 0F BF 2B 88 36 " +
            "9C 07 D3 6D E8 E4 0B A4",
            // After Iota 
            "F8 BD FF A3 97 27 48 28 69 1E 16 37 D7 0D 1B 41 " +
            "1A 79 2E F4 CF C6 7B EB 16 57 3A 80 BC BC C3 D2 " +
            "B9 B4 F7 2E B2 9B A5 D5 74 87 2C 20 8E D1 11 BA " +
            "81 87 41 D6 E5 F4 BF D5 3F 96 73 C5 02 93 D8 74 " +
            "36 58 1D 21 D4 BA BD 0F C3 45 8F 8D 4D 6C 41 9D " +
            "B2 61 B9 AE 9A F8 0D D6 26 83 20 4C A3 AC 58 2F " +
            "31 DF 56 75 60 F0 F1 5B 1A 3E F1 FA 02 DA 3D B5 " +
            "0A 34 04 D9 1E 33 44 C3 83 F0 E5 67 51 5A 09 CB " +
            "32 E0 07 BF 04 B3 91 84 FB 81 AA 93 01 B3 B2 D3 " +
            "8D 54 7A 06 04 F2 D4 4E A7 D3 47 AC EE 3E F8 31 " +
            "41 1F 36 12 95 5D 28 1F 67 01 11 5D FC 7F 84 45 " +
            "35 8A 44 6D 12 C0 1B 7A BA D5 B1 0F BF 2B 88 36 " +
            "9C 07 D3 6D E8 E4 0B A4",
            // Round #13
            // After Theta
            "85 5B D4 86 A2 0B C8 C3 21 DD 65 1A ED A9 99 0A " +
            "9B E2 74 1F 04 54 AC B1 5A 4E 0F 4D CD 57 9E 83 " +
            "4D 6D 99 CD ED 8C 50 E5 09 61 07 05 BB FD 91 51 " +
            "C9 44 32 FB DF 50 3D 9E BE 0D 29 2E C9 01 0F 2E " +
            "7A 41 28 EC A5 51 E0 5E 37 9C E1 6E 12 7B B4 AD " +
            "CF 87 92 8B AF D4 8D 3D 6E 40 53 61 99 08 DA 64 " +
            "B0 44 0C 9E AB 62 26 01 56 27 C4 37 73 31 60 E4 " +
            "FE ED 6A 3A 41 24 B1 F3 FE 16 CE 42 64 76 89 20 " +
            "7A 23 74 92 3E 17 13 CF 7A 1A F0 78 CA 21 65 89 " +
            "C1 4D 4F CB 75 19 89 1F 53 0A 29 4F B1 29 0D 01 " +
            "3C F9 1D 37 A0 71 A8 F4 2F C2 62 70 C6 DB 06 0E " +
            "B4 11 1E 86 D9 52 CC 20 F6 CC 84 C2 CE C0 D5 67 " +
            "68 DE BD 8E B7 F3 FE 94",
            // After Rho
            "85 5B D4 86 A2 0B C8 C3 42 BA CB 34 DA 53 33 15 " +
            "A6 38 DD 07 01 15 6B EC 7C E5 39 A8 E5 F4 D0 D4 " +
            "67 84 2A 6F 6A CB 6C 6E B0 DB 1F 19 95 10 76 50 " +
            "B3 FF 0D D5 E3 99 4C 24 8B 6F 43 8A 4B 72 C0 83 " +
            "20 14 F6 D2 28 70 2F BD 47 DB 7A C3 19 EE 26 B1 " +
            "79 3E 94 5C 7C A5 6E EC 93 B9 01 4D 85 65 22 68 " +
            "F0 5C 15 33 09 80 25 62 62 C0 C8 AD 4E 88 6F E6 " +
            "9D 20 92 D8 79 FF 76 35 85 C8 EC 12 41 FC 2D 9C " +
            "4E D2 E7 62 E2 59 6F 84 B2 44 3D 0D 78 3C E5 90 " +
            "23 F1 23 B8 E9 69 B9 2E 01 53 0A 29 4F B1 29 0D " +
            "A1 D2 F3 E4 77 DC 80 C6 BC 08 8B C1 19 6F 1B 38 " +
            "36 C2 C3 30 5B 8A 19 84 CC 84 C2 CE C0 D5 67 F6 " +
            "3F 25 9A 77 AF E3 ED BC",
            // After Pi
            "85 5B D4 86 A2 0B C8 C3 B3 FF 0D D5 E3 99 4C 24 " +
            "F0 5C 15 33 09 80 25 62 23 F1 23 B8 E9 69 B9 2E " +
            "3F 25 9A 77 AF E3 ED BC 7C E5 39 A8 E5 F4 D0 D4 " +
            "47 DB 7A C3 19 EE 26 B1 79 3E 94 5C 7C A5 6E EC " +
            "4E D2 E7 62 E2 59 6F 84 36 C2 C3 30 5B 8A 19 84 " +
            "42 BA CB 34 DA 53 33 15 8B 6F 43 8A 4B 72 C0 83 " +
            "62 C0 C8 AD 4E 88 6F E6 01 53 0A 29 4F B1 29 0D " +
            "A1 D2 F3 E4 77 DC 80 C6 67 84 2A 6F 6A CB 6C 6E " +
            "B0 DB 1F 19 95 10 76 50 93 B9 01 4D 85 65 22 68 " +
            "B2 44 3D 0D 78 3C E5 90 CC 84 C2 CE C0 D5 67 F6 " +
            "A6 38 DD 07 01 15 6B EC 20 14 F6 D2 28 70 2F BD " +
            "9D 20 92 D8 79 FF 76 35 85 C8 EC 12 41 FC 2D 9C " +
            "BC 08 8B C1 19 6F 1B 38",
            // After Chi
            "C5 5B C4 A4 AA 0B E9 81 B0 5E 2F 5D 03 F0 D4 28 " +
            "EC 58 8D 74 0F 02 61 F2 A3 AB 67 38 E9 61 B9 6D " +
            "0D 81 93 26 EE 73 E9 98 44 C1 BD B4 81 F5 98 98 " +
            "41 1B 19 E1 9B B6 27 B1 49 3E 94 4C 65 27 7E EC " +
            "06 F7 DF EA 46 2D AF D4 35 D8 81 73 43 80 3F A5 " +
            "22 3A 43 11 DE DB 1C 71 8A 7C 41 8A 4A 43 C0 8A " +
            "C2 40 39 69 7E C4 EF 24 43 7B 02 39 C7 B2 1A 1C " +
            "28 97 F3 6E 76 FC 40 44 64 A4 2A 2B 6A AE 6C 46 " +
            "90 9F 23 19 ED 08 B3 C0 DF 39 C3 8F 05 A4 20 0E " +
            "91 44 15 2C 52 36 ED 98 5C DF D7 DE 55 C5 75 E6 " +
            "3B 18 DD 0F 50 9A 3B EC 20 DC 9A D0 28 70 26 35 " +
            "A5 20 91 19 61 FC 64 15 87 F8 B8 14 41 EC 4D 58 " +
            "BC 0C A9 11 31 0F 1F 29",
            // After Iota 
            "4E 5B C4 A4 AA 0B E9 01 B0 5E 2F 5D 03 F0 D4 28 " +
            "EC 58 8D 74 0F 02 61 F2 A3 AB 67 38 E9 61 B9 6D " +
            "0D 81 93 26 EE 73 E9 98 44 C1 BD B4 81 F5 98 98 " +
            "41 1B 19 E1 9B B6 27 B1 49 3E 94 4C 65 27 7E EC " +
            "06 F7 DF EA 46 2D AF D4 35 D8 81 73 43 80 3F A5 " +
            "22 3A 43 11 DE DB 1C 71 8A 7C 41 8A 4A 43 C0 8A " +
            "C2 40 39 69 7E C4 EF 24 43 7B 02 39 C7 B2 1A 1C " +
            "28 97 F3 6E 76 FC 40 44 64 A4 2A 2B 6A AE 6C 46 " +
            "90 9F 23 19 ED 08 B3 C0 DF 39 C3 8F 05 A4 20 0E " +
            "91 44 15 2C 52 36 ED 98 5C DF D7 DE 55 C5 75 E6 " +
            "3B 18 DD 0F 50 9A 3B EC 20 DC 9A D0 28 70 26 35 " +
            "A5 20 91 19 61 FC 64 15 87 F8 B8 14 41 EC 4D 58 " +
            "BC 0C A9 11 31 0F 1F 29",
            // Round #14
            // After Theta
            "29 B3 C7 AF 3A 34 59 7A FD 3C 06 F6 2D 93 87 29 " +
            "C7 15 6C 2D EF 37 9F DF 5F AF 2B 16 E6 53 F4 21 " +
            "13 22 1E BE 0B 74 31 79 23 29 BE BF 11 CA 28 E3 " +
            "0C 79 30 4A B5 D5 74 B0 62 73 75 15 85 12 80 C1 " +
            "FA F3 93 C4 49 1F E2 98 2B 7B 0C EB A6 87 E7 44 " +
            "45 D2 40 1A 4E E4 AC 0A C7 1E 68 21 64 20 93 8B " +
            "E9 0D D8 30 9E F1 11 09 BF 7F 4E 17 C8 80 57 50 " +
            "36 34 7E F6 93 FB 98 A5 03 4C 29 20 FA 91 DC 3D " +
            "DD FD 0A B2 C3 6B E0 C1 F4 74 22 D6 E5 91 DE 23 " +
            "6D 40 59 02 5D 04 A0 D4 42 7C 5A 46 B0 C2 AD 07 " +
            "5C F0 DE 04 C0 A5 8B 97 6D BE B3 7B 06 13 75 34 " +
            "8E 6D 70 40 81 C9 9A 38 7B FC F4 3A 4E DE 00 14 " +
            "A2 AF 24 89 D4 08 C7 C8",
            // After Rho
            "29 B3 C7 AF 3A 34 59 7A FA 79 0C EC 5B 26 0F 53 " +
            "71 05 5B CB FB CD E7 F7 3E 45 1F F2 F5 BA 62 61 " +
            "A0 8B C9 9B 10 F1 F0 5D 1B A1 8C 32 3E 92 E2 FB " +
            "A3 54 5B 4D 07 CB 90 07 B0 D8 5C 5D 45 A1 04 60 " +
            "F9 49 E2 A4 0F 71 4C FD 78 4E B4 B2 C7 B0 6E 7A " +
            "28 92 06 D2 70 22 67 55 2E 1E 7B A0 85 90 81 4C " +
            "86 F1 8C 8F 48 48 6F C0 01 AF A0 7E FF 9C 2E 90 " +
            "FB C9 7D CC 52 1B 1A 3F 40 F4 23 B9 7B 06 98 52 " +
            "41 76 78 0D 3C B8 BB 5F EF 11 7A 3A 11 EB F2 48 " +
            "00 94 BA 0D 28 4B A0 8B 07 42 7C 5A 46 B0 C2 AD " +
            "2E 5E 72 C1 7B 13 00 97 B4 F9 CE EE 19 4C D4 D1 " +
            "B1 0D 0E 28 30 59 13 C7 FC F4 3A 4E DE 00 14 7B " +
            "31 B2 E8 2B 49 22 35 C2",
            // After Pi
            "29 B3 C7 AF 3A 34 59 7A A3 54 5B 4D 07 CB 90 07 " +
            "86 F1 8C 8F 48 48 6F C0 00 94 BA 0D 28 4B A0 8B " +
            "31 B2 E8 2B 49 22 35 C2 3E 45 1F F2 F5 BA 62 61 " +
            "78 4E B4 B2 C7 B0 6E 7A 28 92 06 D2 70 22 67 55 " +
            "41 76 78 0D 3C B8 BB 5F B1 0D 0E 28 30 59 13 C7 " +
            "FA 79 0C EC 5B 26 0F 53 B0 D8 5C 5D 45 A1 04 60 " +
            "01 AF A0 7E FF 9C 2E 90 07 42 7C 5A 46 B0 C2 AD " +
            "2E 5E 72 C1 7B 13 00 97 A0 8B C9 9B 10 F1 F0 5D " +
            "1B A1 8C 32 3E 92 E2 FB 2E 1E 7B A0 85 90 81 4C " +
            "EF 11 7A 3A 11 EB F2 48 FC F4 3A 4E DE 00 14 7B " +
            "71 05 5B CB FB CD E7 F7 F9 49 E2 A4 0F 71 4C FD " +
            "FB C9 7D CC 52 1B 1A 3F 40 F4 23 B9 7B 06 98 52 " +
            "B4 F9 CE EE 19 4C D4 D1",
            // After Chi
            "2D 12 43 2D 72 34 36 BA A3 50 69 4D 27 C8 10 0C " +
            "B7 D3 CC AD 09 68 7A 80 08 95 BD 89 1A 5F E8 B3 " +
            "B3 F6 F0 6B 4C E9 B5 C7 3E D5 1D B2 C5 B8 63 64 " +
            "39 2A CC BF CB 28 F6 70 98 9B 00 F2 70 63 67 D5 " +
            "4F 36 69 DF F9 1A DB 7F F1 07 AE 28 32 59 1F DD " +
            "FB 5E AC CE E1 3A 25 C3 B6 98 00 5D 45 81 C4 4D " +
            "29 B3 A2 FF C6 9F 2E 82 D7 63 70 76 46 94 CD ED " +
            "2E DE 22 D0 7F 92 00 B7 84 95 BA 1B 91 F1 F1 59 " +
            "DA A0 8C 28 2E F9 90 FB 3E FA 7B E4 4B 90 85 7F " +
            "EF 1A BB AB 11 1A 12 4C E7 D4 3E 6E F0 02 16 D9 " +
            "73 85 46 83 AB C7 F5 F5 F9 7D E0 95 26 75 CC BD " +
            "4F C0 B1 8A 52 53 5E BE 01 F0 32 B8 99 87 BB 74 " +
            "3C B1 6E CA 1D 7C DC D9",
            // After Iota 
            "A4 92 43 2D 72 34 36 3A A3 50 69 4D 27 C8 10 0C " +
            "B7 D3 CC AD 09 68 7A 80 08 95 BD 89 1A 5F E8 B3 " +
            "B3 F6 F0 6B 4C E9 B5 C7 3E D5 1D B2 C5 B8 63 64 " +
            "39 2A CC BF CB 28 F6 70 98 9B 00 F2 70 63 67 D5 " +
            "4F 36 69 DF F9 1A DB 7F F1 07 AE 28 32 59 1F DD " +
            "FB 5E AC CE E1 3A 25 C3 B6 98 00 5D 45 81 C4 4D " +
            "29 B3 A2 FF C6 9F 2E 82 D7 63 70 76 46 94 CD ED " +
            "2E DE 22 D0 7F 92 00 B7 84 95 BA 1B 91 F1 F1 59 " +
            "DA A0 8C 28 2E F9 90 FB 3E FA 7B E4 4B 90 85 7F " +
            "EF 1A BB AB 11 1A 12 4C E7 D4 3E 6E F0 02 16 D9 " +
            "73 85 46 83 AB C7 F5 F5 F9 7D E0 95 26 75 CC BD " +
            "4F C0 B1 8A 52 53 5E BE 01 F0 32 B8 99 87 BB 74 " +
            "3C B1 6E CA 1D 7C DC D9",
            // Round #15
            // After Theta
            "0D A6 FD 3F DC B3 AB 79 DB DB 2E 19 06 E7 B4 10 " +
            "44 B8 5F D9 F2 1D AA C5 10 C1 41 29 64 B1 C0 FF " +
            "E1 CF C1 CA B8 A5 0B BC 97 E1 A3 A0 6B 3F FE 27 " +
            "41 A1 8B EB EA 07 52 6C 6B F0 93 86 8B 16 B7 90 " +
            "57 62 95 7F 87 F4 F3 33 A3 3E 9F 89 C6 15 A1 A6 " +
            "52 6A 12 DC 4F BD B8 80 CE 13 47 09 64 AE 60 51 " +
            "DA D8 31 8B 3D EA FE C7 CF 37 8C D6 38 7A E5 A1 " +
            "7C E7 13 71 8B DE BE CC 2D A1 04 09 3F 76 6C 1A " +
            "A2 2B CB 7C 0F D6 34 E7 CD 91 E8 90 B0 E5 55 3A " +
            "F7 4E 47 0B 6F F4 3A 00 B5 ED 0F CF 04 4E A8 A2 " +
            "DA B1 F8 91 05 40 68 B6 81 F6 A7 C1 07 5A 68 A1 " +
            "BC AB 22 FE A9 26 8E FB 19 A4 CE 18 E7 69 93 38 " +
            "6E 88 5F 6B E9 30 62 A2",
            // After Rho
            "0D A6 FD 3F DC B3 AB 79 B6 B7 5D 32 0C CE 69 21 " +
            "11 EE 57 B6 7C 87 6A 31 16 0B FC 0F 11 1C 94 42 " +
            "2D 5D E0 0D 7F 0E 56 C6 BA F6 E3 7F 72 19 3E 0A " +
            "B8 AE 7E 20 C5 16 14 BA E4 1A FC A4 E1 A2 C5 2D " +
            "B1 CA BF 43 FA F9 99 2B 11 6A 3A EA F3 99 68 5C " +
            "94 52 93 E0 7E EA C5 05 45 39 4F 1C 25 90 B9 82 " +
            "59 EC 51 F7 3F D6 C6 8E F4 CA 43 9F 6F 18 AD 71 " +
            "B8 45 6F 5F 66 BE F3 89 12 7E EC D8 34 5A 42 09 " +
            "99 EF C1 9A E6 5C 74 65 2A 9D E6 48 74 48 D8 F2 " +
            "5E 07 E0 DE E9 68 E1 8D A2 B5 ED 0F CF 04 4E A8 " +
            "A1 D9 6A C7 E2 47 16 00 06 DA 9F 06 1F 68 A1 85 " +
            "77 55 C4 3F D5 C4 71 9F A4 CE 18 E7 69 93 38 19 " +
            "98 A8 1B E2 D7 5A 3A 8C",
            // After Pi
            "0D A6 FD 3F DC B3 AB 79 B8 AE 7E 20 C5 16 14 BA " +
            "59 EC 51 F7 3F D6 C6 8E 5E 07 E0 DE E9 68 E1 8D " +
            "98 A8 1B E2 D7 5A 3A 8C 16 0B FC 0F 11 1C 94 42 " +
            "11 6A 3A EA F3 99 68 5C 94 52 93 E0 7E EA C5 05 " +
            "99 EF C1 9A E6 5C 74 65 77 55 C4 3F D5 C4 71 9F " +
            "B6 B7 5D 32 0C CE 69 21 E4 1A FC A4 E1 A2 C5 2D " +
            "F4 CA 43 9F 6F 18 AD 71 A2 B5 ED 0F CF 04 4E A8 " +
            "A1 D9 6A C7 E2 47 16 00 2D 5D E0 0D 7F 0E 56 C6 " +
            "BA F6 E3 7F 72 19 3E 0A 45 39 4F 1C 25 90 B9 82 " +
            "2A 9D E6 48 74 48 D8 F2 A4 CE 18 E7 69 93 38 19 " +
            "11 EE 57 B6 7C 87 6A 31 B1 CA BF 43 FA F9 99 2B " +
            "B8 45 6F 5F 66 BE F3 89 12 7E EC D8 34 5A 42 09 " +
            "06 DA 9F 06 1F 68 A1 85",
            // After Chi
            "4C E6 FC E8 E6 73 69 7D BE AD DE 28 05 3E 35 BB " +
            "D9 44 4A D7 29 C4 DC 8E 5B 01 04 C3 E1 C9 60 FC " +
            "28 A0 19 E2 D6 5E 2E 0E 92 1B 7D 0F 1D 7E 11 43 " +
            "18 C7 7A F0 73 8D 58 3C F2 42 97 C5 6F 6A C4 9F " +
            "99 E5 F9 9A E6 44 F0 25 76 35 C6 DF 37 45 19 83 " +
            "A6 77 5E 29 02 D6 41 71 E6 2F 50 A4 61 A6 87 A5 " +
            "F5 82 41 5F 4F 5B BD 71 B4 93 F8 3F C3 8C 27 89 " +
            "E1 D1 CA 43 03 67 92 0C 68 54 EC 0D 7A 8E D7 46 " +
            "90 72 43 3F 22 51 7E 7A C1 7B 57 BB 2C 03 99 8B " +
            "23 8C 06 40 62 44 9E 34 36 6C 1B 95 69 82 10 11 " +
            "19 EB 17 AA 78 81 08 B1 B3 F0 3F C3 EA B9 99 2B " +
            "BC C5 7C 59 6D 9E 52 0D 03 5A AC 68 54 DD 08 39 " +
            "A6 DA 37 47 9D 10 30 8F",
            // After Iota 
            "4F 66 FC E8 E6 73 69 FD BE AD DE 28 05 3E 35 BB " +
            "D9 44 4A D7 29 C4 DC 8E 5B 01 04 C3 E1 C9 60 FC " +
            "28 A0 19 E2 D6 5E 2E 0E 92 1B 7D 0F 1D 7E 11 43 " +
            "18 C7 7A F0 73 8D 58 3C F2 42 97 C5 6F 6A C4 9F " +
            "99 E5 F9 9A E6 44 F0 25 76 35 C6 DF 37 45 19 83 " +
            "A6 77 5E 29 02 D6 41 71 E6 2F 50 A4 61 A6 87 A5 " +
            "F5 82 41 5F 4F 5B BD 71 B4 93 F8 3F C3 8C 27 89 " +
            "E1 D1 CA 43 03 67 92 0C 68 54 EC 0D 7A 8E D7 46 " +
            "90 72 43 3F 22 51 7E 7A C1 7B 57 BB 2C 03 99 8B " +
            "23 8C 06 40 62 44 9E 34 36 6C 1B 95 69 82 10 11 " +
            "19 EB 17 AA 78 81 08 B1 B3 F0 3F C3 EA B9 99 2B " +
            "BC C5 7C 59 6D 9E 52 0D 03 5A AC 68 54 DD 08 39 " +
            "A6 DA 37 47 9D 10 30 8F",
            // Round #16
            // After Theta
            "A6 1A D4 45 4F 66 F7 04 F3 6D 94 1E 6F 3A 0F 4F " +
            "16 C1 9D CA 12 08 92 47 A6 DF C0 34 84 7D 05 25 " +
            "6A 6B FF 7E D2 6F C2 22 7B 67 55 A2 B4 6B 8F BA " +
            "55 07 30 C6 19 89 62 C8 3D C7 40 D8 54 A6 8A 56 " +
            "64 3B 3D 6D 83 F0 95 FC 34 FE 20 43 33 74 F5 AF " +
            "4F 0B 76 84 AB C3 DF 88 AB EF 1A 92 0B A2 BD 51 " +
            "3A 07 96 42 74 97 F3 B8 49 4D 3C C8 A6 38 42 50 " +
            "A3 1A 2C DF 07 56 7E 20 81 28 C4 A0 D3 9B 49 BF " +
            "DD B2 09 09 48 55 44 8E 0E FE 80 A6 17 CF D7 42 " +
            "DE 52 C2 B7 07 F0 FB ED 74 A7 FD 09 6D B3 FC 3D " +
            "F0 97 3F 07 D1 94 96 48 FE 30 75 F5 80 BD A3 DF " +
            "73 40 AB 44 56 52 1C C4 FE 84 68 9F 31 69 6D E0 " +
            "E4 11 D1 DB 99 21 DC A3",
            // After Rho
            "A6 1A D4 45 4F 66 F7 04 E6 DB 28 3D DE 74 1E 9E " +
            "45 70 A7 B2 04 82 E4 91 D8 57 50 62 FA 0D 4C 43 " +
            "7E 13 16 51 5B FB F7 93 4A BB F6 A8 BB 77 56 25 " +
            "63 9C 91 28 86 5C 75 00 55 CF 31 10 36 95 A9 A2 " +
            "9D 9E B6 41 F8 4A 7E B2 57 FF 4A E3 0F 32 34 43 " +
            "7C 5A B0 23 5C 1D FE 46 46 AD BE 6B 48 2E 88 F6 " +
            "14 A2 BB 9C C7 D5 39 B0 71 84 A0 92 9A 78 90 4D " +
            "EF 03 2B 3F 90 51 0D 96 41 A7 37 93 7E 03 51 88 " +
            "21 01 A9 8A C8 B1 5B 36 6B 21 07 7F 40 D3 8B E7 " +
            "7E BF DD 5B 4A F8 F6 00 3D 74 A7 FD 09 6D B3 FC " +
            "5A 22 C1 5F FE 1C 44 53 FB C3 D4 D5 03 F6 8E 7E " +
            "0E 68 95 C8 4A 8A 83 78 84 68 9F 31 69 6D E0 FE " +
            "F7 28 79 44 F4 76 66 08",
            // After Pi
            "A6 1A D4 45 4F 66 F7 04 63 9C 91 28 86 5C 75 00 " +
            "14 A2 BB 9C C7 D5 39 B0 7E BF DD 5B 4A F8 F6 00 " +
            "F7 28 79 44 F4 76 66 08 D8 57 50 62 FA 0D 4C 43 " +
            "57 FF 4A E3 0F 32 34 43 7C 5A B0 23 5C 1D FE 46 " +
            "21 01 A9 8A C8 B1 5B 36 0E 68 95 C8 4A 8A 83 78 " +
            "E6 DB 28 3D DE 74 1E 9E 55 CF 31 10 36 95 A9 A2 " +
            "71 84 A0 92 9A 78 90 4D 3D 74 A7 FD 09 6D B3 FC " +
            "5A 22 C1 5F FE 1C 44 53 7E 13 16 51 5B FB F7 93 " +
            "4A BB F6 A8 BB 77 56 25 46 AD BE 6B 48 2E 88 F6 " +
            "6B 21 07 7F 40 D3 8B E7 84 68 9F 31 69 6D E0 FE " +
            "45 70 A7 B2 04 82 E4 91 9D 9E B6 41 F8 4A 7E B2 " +
            "EF 03 2B 3F 90 51 0D 96 41 A7 37 93 7E 03 51 88 " +
            "FB C3 D4 D5 03 F6 8E 7E",
            // After Chi
            "B2 38 FE D1 0E E7 FF B4 09 81 D5 6B 8E 74 B3 00 " +
            "95 A2 9B 98 73 D3 39 B8 7E AD 59 5A 41 F8 67 04 " +
            "B6 AC 78 6C 74 6E 66 08 F0 57 E0 62 AA 00 86 47 " +
            "56 FE 43 6B 8F 92 35 73 72 32 A4 63 5E 17 7E 0E " +
            "F1 16 E9 A8 78 B4 17 35 09 C0 9F 49 4F B8 B3 78 " +
            "C6 DB A8 BF 56 1C 0E D3 59 BF 36 7D 37 90 8A 12 " +
            "33 86 E0 90 6C 68 D4 4E 99 AD 8F DD 09 0D A9 70 " +
            "4B 26 D0 5F DE 9D E5 73 7A 17 1E 12 1B F3 7F 41 " +
            "63 BB F7 BC BB A6 55 24 C2 E5 26 6B 61 02 E8 EE " +
            "11 32 07 3F 52 41 9C E6 84 C0 7F 99 C9 69 E0 DA " +
            "27 71 AE 8C 04 93 E5 95 9D 3A A2 C1 96 48 2E BA " +
            "55 43 EB 7B 91 A5 83 E0 45 97 14 B1 7A 03 31 09 " +
            "63 4D C4 94 FB BE 94 5C",
            // After Iota 
            "B0 B8 FE D1 0E E7 FF 34 09 81 D5 6B 8E 74 B3 00 " +
            "95 A2 9B 98 73 D3 39 B8 7E AD 59 5A 41 F8 67 04 " +
            "B6 AC 78 6C 74 6E 66 08 F0 57 E0 62 AA 00 86 47 " +
            "56 FE 43 6B 8F 92 35 73 72 32 A4 63 5E 17 7E 0E " +
            "F1 16 E9 A8 78 B4 17 35 09 C0 9F 49 4F B8 B3 78 " +
            "C6 DB A8 BF 56 1C 0E D3 59 BF 36 7D 37 90 8A 12 " +
            "33 86 E0 90 6C 68 D4 4E 99 AD 8F DD 09 0D A9 70 " +
            "4B 26 D0 5F DE 9D E5 73 7A 17 1E 12 1B F3 7F 41 " +
            "63 BB F7 BC BB A6 55 24 C2 E5 26 6B 61 02 E8 EE " +
            "11 32 07 3F 52 41 9C E6 84 C0 7F 99 C9 69 E0 DA " +
            "27 71 AE 8C 04 93 E5 95 9D 3A A2 C1 96 48 2E BA " +
            "55 43 EB 7B 91 A5 83 E0 45 97 14 B1 7A 03 31 09 " +
            "63 4D C4 94 FB BE 94 5C",
            // Round #17
            // After Theta
            "52 FC 98 A7 EF 4B 54 4F 55 B3 F6 0F 01 F8 AE 99 " +
            "E8 85 37 DA 59 4D A6 1B 1A 93 52 CE 5E CA 16 F8 " +
            "42 BA 58 E9 B7 5A C9 4F 12 13 86 14 4B AC 2D 3C " +
            "0A CC 60 0F 00 1E 28 EA 0F 15 08 21 74 89 E1 AD " +
            "95 28 E2 3C 67 86 66 C9 FD D6 BF CC 8C 8C 1C 3F " +
            "24 9F CE C9 B7 B0 A5 A8 05 8D 15 19 B8 1C 97 8B " +
            "4E A1 4C D2 46 F6 4B ED FD 93 84 49 16 3F D8 8C " +
            "BF 30 F0 DA 1D A9 4A 34 98 53 78 64 FA 5F D4 3A " +
            "3F 89 D4 D8 34 2A 48 BD BF C2 8A 29 4B 9C 77 4D " +
            "75 0C 0C AB 4D 73 ED 1A 70 D6 5F 1C 0A 5D 4F 9D " +
            "C5 35 C8 FA E5 3F 4E EE C1 08 81 A5 19 C4 33 23 " +
            "28 64 47 39 BB 3B 1C 43 21 A9 1F 25 65 31 40 F5 " +
            "97 5B E4 11 38 8A 3B 1B",
            // After Rho
            "52 FC 98 A7 EF 4B 54 4F AB 66 ED 1F 02 F0 5D 33 " +
            "7A E1 8D 76 56 93 E9 06 A5 6C 81 AF 31 29 E5 EC " +
            "D5 4A 7E 12 D2 C5 4A BF B1 C4 DA C2 23 31 61 48 " +
            "F6 00 E0 81 A2 AE C0 0C EB 43 05 42 08 5D 62 78 " +
            "14 71 9E 33 43 B3 E4 4A C8 F1 D3 6F FD CB CC C8 " +
            "25 F9 74 4E BE 85 2D 45 2E 16 34 56 64 E0 72 5C " +
            "92 36 B2 5F 6A 77 0A 65 7E B0 19 FB 27 09 93 2C " +
            "ED 8E 54 25 9A 5F 18 78 C8 F4 BF A8 75 30 A7 F0 " +
            "1A 9B 46 05 A9 F7 27 91 BB A6 5F 61 C5 94 25 CE " +
            "AE 5D A3 8E 81 61 B5 69 9D 70 D6 5F 1C 0A 5D 4F " +
            "38 B9 17 D7 20 EB 97 FF 04 23 04 96 66 10 CF 8C " +
            "85 EC 28 67 77 87 63 08 A9 1F 25 65 31 40 F5 21 " +
            "CE C6 E5 16 79 04 8E E2",
            // After Pi
            "52 FC 98 A7 EF 4B 54 4F F6 00 E0 81 A2 AE C0 0C " +
            "92 36 B2 5F 6A 77 0A 65 AE 5D A3 8E 81 61 B5 69 " +
            "CE C6 E5 16 79 04 8E E2 A5 6C 81 AF 31 29 E5 EC " +
            "C8 F1 D3 6F FD CB CC C8 25 F9 74 4E BE 85 2D 45 " +
            "1A 9B 46 05 A9 F7 27 91 85 EC 28 67 77 87 63 08 " +
            "AB 66 ED 1F 02 F0 5D 33 EB 43 05 42 08 5D 62 78 " +
            "7E B0 19 FB 27 09 93 2C 9D 70 D6 5F 1C 0A 5D 4F " +
            "38 B9 17 D7 20 EB 97 FF D5 4A 7E 12 D2 C5 4A BF " +
            "B1 C4 DA C2 23 31 61 48 2E 16 34 56 64 E0 72 5C " +
            "BB A6 5F 61 C5 94 25 CE A9 1F 25 65 31 40 F5 21 " +
            "7A E1 8D 76 56 93 E9 06 14 71 9E 33 43 B3 E4 4A " +
            "ED 8E 54 25 9A 5F 18 78 C8 F4 BF A8 75 30 A7 F0 " +
            "04 23 04 96 66 10 CF 8C",
            // After Chi
            "52 CA 8A F9 A7 1A 5E 2E DA 49 E1 01 23 AE 75 04 " +
            "D2 B4 F6 4F 12 73 00 E7 BE 65 BB 2F 07 2A E5 64 " +
            "6A C6 85 16 79 A0 0E E2 80 64 A5 AF 33 2D C4 E9 " +
            "D2 F3 D1 6E FC B9 CE 58 A0 9D 5C 2C E8 85 6D 4D " +
            "3A 9B C7 8D A9 DF A3 75 CD 7D 7A 27 BB 45 6B 08 " +
            "BF D6 F5 A6 25 F0 CC 37 6A 03 C3 46 10 5F 2E 3B " +
            "5E 39 18 7B 07 E8 11 9C 1E 36 3E 57 1E 1A 15 4F " +
            "78 B8 17 97 28 E6 B5 B7 DB 58 5A 06 96 05 58 AB " +
            "20 64 91 E3 A2 25 64 CA 2E 0F 14 52 54 A0 A2 7D " +
            "EF E6 05 73 07 11 2F 50 89 9B A5 A5 10 70 D4 61 " +
            "93 6F CD 72 CE DF F1 36 14 01 35 BB 26 93 43 CA " +
            "E9 8D 54 33 98 5F 50 74 B2 34 36 C8 65 B3 87 F2 " +
            "00 33 16 97 67 30 CB C4",
            // After Iota 
            "D2 CA 8A F9 A7 1A 5E AE DA 49 E1 01 23 AE 75 04 " +
            "D2 B4 F6 4F 12 73 00 E7 BE 65 BB 2F 07 2A E5 64 " +
            "6A C6 85 16 79 A0 0E E2 80 64 A5 AF 33 2D C4 E9 " +
            "D2 F3 D1 6E FC B9 CE 58 A0 9D 5C 2C E8 85 6D 4D " +
            "3A 9B C7 8D A9 DF A3 75 CD 7D 7A 27 BB 45 6B 08 " +
            "BF D6 F5 A6 25 F0 CC 37 6A 03 C3 46 10 5F 2E 3B " +
            "5E 39 18 7B 07 E8 11 9C 1E 36 3E 57 1E 1A 15 4F " +
            "78 B8 17 97 28 E6 B5 B7 DB 58 5A 06 96 05 58 AB " +
            "20 64 91 E3 A2 25 64 CA 2E 0F 14 52 54 A0 A2 7D " +
            "EF E6 05 73 07 11 2F 50 89 9B A5 A5 10 70 D4 61 " +
            "93 6F CD 72 CE DF F1 36 14 01 35 BB 26 93 43 CA " +
            "E9 8D 54 33 98 5F 50 74 B2 34 36 C8 65 B3 87 F2 " +
            "00 33 16 97 67 30 CB C4",
            // Round #18
            // After Theta
            "28 D9 7E 8F AC A5 F4 99 A9 23 49 76 A8 71 97 96 " +
            "0B 5D 43 A2 FD 16 44 79 F8 A1 FE 7E 0D 4C F5 AA " +
            "E6 43 6E 50 78 D6 0B C5 7A 77 51 D9 38 92 6E DE " +
            "A1 99 79 19 77 66 2C CA 79 74 E9 C1 07 E0 29 D3 " +
            "7C 5F 82 DC A3 B9 B3 BB 41 F8 91 61 BA 33 6E 2F " +
            "45 C5 01 D0 2E 4F 66 00 19 69 6B 31 9B 80 CC A9 " +
            "87 D0 AD 96 E8 8D 55 02 58 F2 7B 06 14 7C 05 81 " +
            "F4 3D FC D1 29 90 B0 90 21 4B AE 70 9D BA F2 9C " +
            "53 0E 39 94 29 FA 86 58 F7 E6 A1 BF BB C5 E6 E3 " +
            "A9 22 40 22 0D 77 3F 9E 05 1E 4E E3 11 06 D1 46 " +
            "69 7C 39 04 C5 60 5B 01 67 6B 9D CC AD 4C A1 58 " +
            "30 64 E1 DE 77 3A 14 EA F4 F0 73 99 6F D5 97 3C " +
            "8C B6 FD D1 66 46 CE E3",
            // After Rho
            "28 D9 7E 8F AC A5 F4 99 53 47 92 EC 50 E3 2E 2D " +
            "42 D7 90 68 BF 05 51 DE C0 54 AF 8A 1F EA EF D7 " +
            "B3 5E 28 36 1F 72 83 C2 8D 23 E9 E6 AD 77 17 95 " +
            "97 71 67 C6 A2 1C 9A 99 74 1E 5D 7A F0 01 78 CA " +
            "2F 41 EE D1 DC D9 5D BE E3 F6 12 84 1F 19 A6 3B " +
            "28 2A 0E 80 76 79 32 03 A7 66 A4 AD C5 6C 02 32 " +
            "B5 44 6F AC 12 38 84 6E F8 0A 02 B1 E4 F7 0C 28 " +
            "E8 14 48 58 48 FA 1E FE E1 3A 75 E5 39 43 96 5C " +
            "87 32 45 DF 10 6B CA 21 F3 F1 7B F3 D0 DF DD 62 " +
            "EE C7 33 55 04 48 A4 E1 46 05 1E 4E E3 11 06 D1 " +
            "6D 05 A4 F1 E5 10 14 83 9D AD 75 32 B7 32 85 62 " +
            "86 2C DC FB 4E 87 42 1D F0 73 99 6F D5 97 3C F4 " +
            "F3 38 A3 6D 7F B4 99 91",
            // After Pi
            "28 D9 7E 8F AC A5 F4 99 97 71 67 C6 A2 1C 9A 99 " +
            "B5 44 6F AC 12 38 84 6E EE C7 33 55 04 48 A4 E1 " +
            "F3 38 A3 6D 7F B4 99 91 C0 54 AF 8A 1F EA EF D7 " +
            "E3 F6 12 84 1F 19 A6 3B 28 2A 0E 80 76 79 32 03 " +
            "87 32 45 DF 10 6B CA 21 86 2C DC FB 4E 87 42 1D " +
            "53 47 92 EC 50 E3 2E 2D 74 1E 5D 7A F0 01 78 CA " +
            "F8 0A 02 B1 E4 F7 0C 28 46 05 1E 4E E3 11 06 D1 " +
            "6D 05 A4 F1 E5 10 14 83 B3 5E 28 36 1F 72 83 C2 " +
            "8D 23 E9 E6 AD 77 17 95 A7 66 A4 AD C5 6C 02 32 " +
            "F3 F1 7B F3 D0 DF DD 62 F0 73 99 6F D5 97 3C F4 " +
            "42 D7 90 68 BF 05 51 DE 2F 41 EE D1 DC D9 5D BE " +
            "E8 14 48 58 48 FA 1E FE E1 3A 75 E5 39 43 96 5C " +
            "9D AD 75 32 B7 32 85 62",
            // After Chi
            "08 DD 76 A7 BC 85 F0 FF DD F2 77 97 A6 5C BA 18 " +
            "A4 7C EF 84 69 8C 9D 7E E6 06 6F D7 84 49 C0 E9 " +
            "64 18 A2 2D 7D AC 93 91 C8 5C A3 8A 7F 8A FF D7 " +
            "64 E6 53 DB 1F 1B 6E 1B 28 26 96 A0 38 FD 32 1F " +
            "C7 62 66 DF 01 03 67 E3 A5 8E CC FF 4E 96 42 35 " +
            "DB 47 90 6D 54 15 2A 0D 72 1B 41 34 F3 01 7A 1B " +
            "D1 0A A2 00 E0 F7 1C 2A 54 47 0C 42 F3 F2 2C FD " +
            "49 1D E9 E3 45 10 44 41 91 1A 2C 3F 5F 7A 83 E0 " +
            "DD B2 B2 B4 BD E4 CA D5 A7 64 24 A1 C0 6C 22 A6 " +
            "F0 FD 5B E3 DA BF 5E 60 FC 52 58 AF 75 92 28 E1 " +
            "82 C3 90 60 BF 27 53 9E 2E 6B DB 74 ED D8 DD BE " +
            "F4 91 48 4A CE CA 1F DC A3 68 F5 AD 31 46 C6 C0 " +
            "B0 AD 1B A3 F7 EA 89 42",
            // After Iota 
            "02 5D 76 A7 BC 85 F0 FF DD F2 77 97 A6 5C BA 18 " +
            "A4 7C EF 84 69 8C 9D 7E E6 06 6F D7 84 49 C0 E9 " +
            "64 18 A2 2D 7D AC 93 91 C8 5C A3 8A 7F 8A FF D7 " +
            "64 E6 53 DB 1F 1B 6E 1B 28 26 96 A0 38 FD 32 1F " +
            "C7 62 66 DF 01 03 67 E3 A5 8E CC FF 4E 96 42 35 " +
            "DB 47 90 6D 54 15 2A 0D 72 1B 41 34 F3 01 7A 1B " +
            "D1 0A A2 00 E0 F7 1C 2A 54 47 0C 42 F3 F2 2C FD " +
            "49 1D E9 E3 45 10 44 41 91 1A 2C 3F 5F 7A 83 E0 " +
            "DD B2 B2 B4 BD E4 CA D5 A7 64 24 A1 C0 6C 22 A6 " +
            "F0 FD 5B E3 DA BF 5E 60 FC 52 58 AF 75 92 28 E1 " +
            "82 C3 90 60 BF 27 53 9E 2E 6B DB 74 ED D8 DD BE " +
            "F4 91 48 4A CE CA 1F DC A3 68 F5 AD 31 46 C6 C0 " +
            "B0 AD 1B A3 F7 EA 89 42",
            // Round #19
            // After Theta
            "B6 85 AB EA 7D 23 B6 5E C3 27 E1 17 AE 5A 53 20 " +
            "D0 C6 B4 35 49 75 02 A3 60 4A 50 63 D3 CC 26 54 " +
            "46 90 FA 16 0E 63 6A 71 7C 84 7E C7 BE 2C B9 76 " +
            "7A 33 C5 5B 17 1D 87 23 5C 9C CD 11 18 04 AD C2 " +
            "41 2E 59 6B 56 86 81 5E 87 06 94 C4 3D 59 BB D5 " +
            "6F 9F 4D 20 95 B3 6C AC 6C CE D7 B4 FB 07 93 23 " +
            "A5 B0 F9 B1 C0 0E 83 F7 D2 0B 33 F6 A4 77 CA 40 " +
            "6B 95 B1 D8 36 DF BD A1 25 C2 F1 72 9E DC C5 41 " +
            "C3 67 24 34 B5 E2 23 ED D3 DE 7F 10 E0 95 BD 7B " +
            "76 B1 64 57 8D 3A B8 DD DE DA 00 94 06 5D D1 01 " +
            "36 1B 4D 2D 7E 81 15 3F 30 BE 4D F4 E5 DE 34 86 " +
            "80 2B 13 FB EE 33 80 01 25 24 CA 19 66 C3 20 7D " +
            "92 25 43 98 84 25 70 A2",
            // After Rho
            "B6 85 AB EA 7D 23 B6 5E 86 4F C2 2F 5C B5 A6 40 " +
            "B4 31 6D 4D 52 9D C0 28 CD 6C 42 05 A6 04 35 36 " +
            "18 53 8B 33 82 D4 B7 70 EC CB 92 6B C7 47 E8 77 " +
            "BC 75 D1 71 38 A2 37 53 30 17 67 73 04 06 41 AB " +
            "97 AC 35 2B C3 40 AF 20 B5 5B 7D 68 40 49 DC 93 " +
            "7D FB 6C 02 A9 9C 65 63 8E B0 39 5F D3 EE 1F 4C " +
            "8F 05 76 18 BC 2F 85 CD EF 94 81 A4 17 66 EC 49 " +
            "6C 9B EF DE D0 B5 CA 58 E5 3C B9 8B 83 4A 84 E3 " +
            "84 A6 56 7C A4 7D F8 8C DE BD 69 EF 3F 08 F0 CA " +
            "07 B7 DB 2E 96 EC AA 51 01 DE DA 00 94 06 5D D1 " +
            "56 FC D8 6C 34 B5 F8 05 C2 F8 36 D1 97 7B D3 18 " +
            "70 65 62 DF 7D 06 30 00 24 CA 19 66 C3 20 7D 25 " +
            "9C A8 64 C9 10 26 61 09",
            // After Pi
            "B6 85 AB EA 7D 23 B6 5E BC 75 D1 71 38 A2 37 53 " +
            "8F 05 76 18 BC 2F 85 CD 07 B7 DB 2E 96 EC AA 51 " +
            "9C A8 64 C9 10 26 61 09 CD 6C 42 05 A6 04 35 36 " +
            "B5 5B 7D 68 40 49 DC 93 7D FB 6C 02 A9 9C 65 63 " +
            "84 A6 56 7C A4 7D F8 8C 70 65 62 DF 7D 06 30 00 " +
            "86 4F C2 2F 5C B5 A6 40 30 17 67 73 04 06 41 AB " +
            "EF 94 81 A4 17 66 EC 49 01 DE DA 00 94 06 5D D1 " +
            "56 FC D8 6C 34 B5 F8 05 18 53 8B 33 82 D4 B7 70 " +
            "EC CB 92 6B C7 47 E8 77 8E B0 39 5F D3 EE 1F 4C " +
            "DE BD 69 EF 3F 08 F0 CA 24 CA 19 66 C3 20 7D 25 " +
            "B4 31 6D 4D 52 9D C0 28 97 AC 35 2B C3 40 AF 20 " +
            "6C 9B EF DE D0 B5 CA 58 E5 3C B9 8B 83 4A 84 E3 " +
            "C2 F8 36 D1 97 7B D3 18",
            // After Chi
            "B5 85 8D E2 F9 2E 36 D2 BC C7 58 57 3A 62 1D 43 " +
            "17 0D 52 D9 BC 2D C4 C5 25 B2 50 0C FB ED 3C 07 " +
            "94 D8 34 D8 10 A6 60 08 85 CC 42 07 0F 90 14 56 " +
            "35 5F 6F 14 44 28 44 1F 0D BA 4C 81 F0 9E 65 63 " +
            "09 AE 56 7C 26 7D FD BA 40 76 5F B7 3D 4F F8 81 " +
            "49 CF 42 AB 4F D5 0A 00 30 5D 3D 73 84 06 50 3B " +
            "B9 B4 81 C8 37 D7 4C 4D 81 DD D8 03 DC 06 5B 91 " +
            "66 EC FD 3C 34 B7 B9 AE 1A 63 A2 27 92 7C A0 78 " +
            "BC C6 D2 CB EB 47 08 F5 AE F2 29 5F 13 CE 12 69 " +
            "C6 AC EB FE 3F DC 72 9A C0 42 09 2E 86 23 35 22 " +
            "DC 22 A7 99 42 28 80 70 16 88 25 2A C0 0A AB 83 " +
            "6E 5B E9 8E C4 84 99 40 D1 3D F0 87 C3 CE 84 C3 " +
            "C1 74 26 F3 16 3B FC 18",
            // After Iota 
            "BF 85 8D 62 F9 2E 36 52 BC C7 58 57 3A 62 1D 43 " +
            "17 0D 52 D9 BC 2D C4 C5 25 B2 50 0C FB ED 3C 07 " +
            "94 D8 34 D8 10 A6 60 08 85 CC 42 07 0F 90 14 56 " +
            "35 5F 6F 14 44 28 44 1F 0D BA 4C 81 F0 9E 65 63 " +
            "09 AE 56 7C 26 7D FD BA 40 76 5F B7 3D 4F F8 81 " +
            "49 CF 42 AB 4F D5 0A 00 30 5D 3D 73 84 06 50 3B " +
            "B9 B4 81 C8 37 D7 4C 4D 81 DD D8 03 DC 06 5B 91 " +
            "66 EC FD 3C 34 B7 B9 AE 1A 63 A2 27 92 7C A0 78 " +
            "BC C6 D2 CB EB 47 08 F5 AE F2 29 5F 13 CE 12 69 " +
            "C6 AC EB FE 3F DC 72 9A C0 42 09 2E 86 23 35 22 " +
            "DC 22 A7 99 42 28 80 70 16 88 25 2A C0 0A AB 83 " +
            "6E 5B E9 8E C4 84 99 40 D1 3D F0 87 C3 CE 84 C3 " +
            "C1 74 26 F3 16 3B FC 18",
            // Round #20
            // After Theta
            "2A E7 CF 4F D3 6B 8A 6C CE 54 6F A5 0B 00 D9 CB " +
            "70 27 25 1D 97 25 B7 3E 20 F1 7D 50 44 4E 8A FE " +
            "44 07 E0 33 3F 5C 1C 65 10 AE 00 2A 25 D5 A8 68 " +
            "47 CC 58 E6 75 4A 80 97 6A 90 3B 45 DB 96 16 98 " +
            "0C ED 7B 20 99 DE 4B 43 90 A9 8B 5C 12 B5 84 EC " +
            "DC AD 00 86 65 90 B6 3E 42 CE 0A 81 B5 64 94 B3 " +
            "DE 9E F6 0C 1C DF 3F B6 84 9E F5 5F 63 A5 ED 68 " +
            "B6 33 29 D7 1B 4D C5 C3 8F 01 E0 0A B8 39 1C 46 " +
            "CE 55 E5 39 DA 25 CC 7D C9 D8 5E 9B 38 C6 61 92 " +
            "C3 EF C6 A2 80 7F C4 63 10 9D DD C5 A9 D9 49 4F " +
            "49 40 E5 B4 68 6D 3C 4E 64 1B 12 D8 F1 68 6F 0B " +
            "09 71 9E 4A EF 8C EA BB D4 7E DD DB 7C 6D 32 3A " +
            "11 AB F2 18 39 C1 80 75",
            // After Rho
            "2A E7 CF 4F D3 6B 8A 6C 9D A9 DE 4A 17 00 B2 97 " +
            "DC 49 49 C7 65 C9 AD 0F E4 A4 E8 0F 12 DF 07 45 " +
            "E1 E2 28 23 3A 00 9F F9 52 52 8D 8A 06 E1 0A A0 " +
            "65 5E A7 04 78 79 C4 8C A6 1A E4 4E D1 B6 A5 05 " +
            "F6 3D 90 4C EF A5 21 86 4B C8 0E 99 BA C8 25 51 " +
            "E1 6E 05 30 2C 83 B4 F5 CE 0A 39 2B 04 D6 92 51 " +
            "67 E0 F8 FE B1 F5 F6 B4 4A DB D1 08 3D EB BF C6 " +
            "EB 8D A6 E2 61 DB 99 94 15 70 73 38 8C 1E 03 C0 " +
            "3C 47 BB 84 B9 CF B9 AA 30 C9 64 6C AF 4D 1C E3 " +
            "8F 78 6C F8 DD 58 14 F0 4F 10 9D DD C5 A9 D9 49 " +
            "F1 38 25 01 95 D3 A2 B5 90 6D 48 60 C7 A3 BD 2D " +
            "21 CE 53 E9 9D 51 7D 37 7E DD DB 7C 6D 32 3A D4 " +
            "60 5D C4 AA 3C 46 4E 30",
            // After Pi
            "2A E7 CF 4F D3 6B 8A 6C 65 5E A7 04 78 79 C4 8C " +
            "67 E0 F8 FE B1 F5 F6 B4 8F 78 6C F8 DD 58 14 F0 " +
            "60 5D C4 AA 3C 46 4E 30 E4 A4 E8 0F 12 DF 07 45 " +
            "4B C8 0E 99 BA C8 25 51 E1 6E 05 30 2C 83 B4 F5 " +
            "3C 47 BB 84 B9 CF B9 AA 21 CE 53 E9 9D 51 7D 37 " +
            "9D A9 DE 4A 17 00 B2 97 A6 1A E4 4E D1 B6 A5 05 " +
            "4A DB D1 08 3D EB BF C6 4F 10 9D DD C5 A9 D9 49 " +
            "F1 38 25 01 95 D3 A2 B5 E1 E2 28 23 3A 00 9F F9 " +
            "52 52 8D 8A 06 E1 0A A0 CE 0A 39 2B 04 D6 92 51 " +
            "30 C9 64 6C AF 4D 1C E3 7E DD DB 7C 6D 32 3A D4 " +
            "DC 49 49 C7 65 C9 AD 0F F6 3D 90 4C EF A5 21 86 " +
            "EB 8D A6 E2 61 DB 99 94 15 70 73 38 8C 1E 03 C0 " +
            "90 6D 48 60 C7 A3 BD 2D",
            // After Chi
            "28 47 97 B5 52 EF B8 5C ED 46 A3 04 34 71 C4 CC " +
            "07 E5 78 FC 91 F3 BC B4 85 DA 67 BD 1E 71 94 BC " +
            "25 45 E4 AA 14 56 0A B0 44 82 E9 2F 16 DC 97 E1 " +
            "57 C9 B4 1D 2B 84 2C 5B E0 E6 45 59 28 93 F0 E0 " +
            "F8 67 13 82 BB 41 BB EA 2A 86 55 79 35 51 5D 27 " +
            "D5 68 CF 4A 3B 49 A8 55 A3 1A E8 9B 11 B6 E5 0C " +
            "FA F3 F1 08 2D B9 9D 72 43 91 47 97 C7 A9 C9 4B " +
            "D3 2A 05 05 55 65 A7 B5 6D EA 18 02 3A 16 0F A8 " +
            "62 93 C9 CE AD E8 06 02 80 1E A2 3B 44 E4 B0 45 " +
            "B1 EB 44 6F BD 4D 99 CA 6C CD 5E F4 69 D3 3A D4 " +
            "D5 C9 6F 65 65 93 35 1F E2 4D C1 54 63 A1 23 C6 " +
            "6B 80 AE A2 22 7A 25 B9 59 70 72 BF AC 56 03 C2 " +
            "B2 59 D8 68 4D 87 BD AD",
            // After Iota 
            "A9 C7 97 35 52 EF B8 DC ED 46 A3 04 34 71 C4 CC " +
            "07 E5 78 FC 91 F3 BC B4 85 DA 67 BD 1E 71 94 BC " +
            "25 45 E4 AA 14 56 0A B0 44 82 E9 2F 16 DC 97 E1 " +
            "57 C9 B4 1D 2B 84 2C 5B E0 E6 45 59 28 93 F0 E0 " +
            "F8 67 13 82 BB 41 BB EA 2A 86 55 79 35 51 5D 27 " +
            "D5 68 CF 4A 3B 49 A8 55 A3 1A E8 9B 11 B6 E5 0C " +
            "FA F3 F1 08 2D B9 9D 72 43 91 47 97 C7 A9 C9 4B " +
            "D3 2A 05 05 55 65 A7 B5 6D EA 18 02 3A 16 0F A8 " +
            "62 93 C9 CE AD E8 06 02 80 1E A2 3B 44 E4 B0 45 " +
            "B1 EB 44 6F BD 4D 99 CA 6C CD 5E F4 69 D3 3A D4 " +
            "D5 C9 6F 65 65 93 35 1F E2 4D C1 54 63 A1 23 C6 " +
            "6B 80 AE A2 22 7A 25 B9 59 70 72 BF AC 56 03 C2 " +
            "B2 59 D8 68 4D 87 BD AD",
            // Round #21
            // After Theta
            "99 2D 4B 4E 82 CC 9F 39 80 95 E5 5A F0 01 F1 A7 " +
            "32 C1 84 14 B7 FD 6D C1 77 4E C3 1D 4C 5A 3E D0 " +
            "F2 EF 6D BD 27 2A 0D 1A 74 68 35 54 C6 FF B0 04 " +
            "3A 1A F2 43 EF F4 19 30 D5 C2 B9 B1 0E 9D 21 95 " +
            "0A F3 B7 22 E9 6A 11 86 FD 2C DC 6E 06 2D 5A 8D " +
            "E5 82 13 31 EB 6A 8F B0 CE C9 AE C5 D5 C6 D0 67 " +
            "CF D7 0D E0 0B B7 4C 07 B1 05 E3 37 95 82 63 27 " +
            "04 80 8C 12 66 19 A0 1F 5D 00 C4 79 EA 35 28 4D " +
            "0F 40 8F 90 69 98 33 69 B5 3A 5E D3 62 EA 61 30 " +
            "43 7F E0 CF EF 66 33 A6 BB 67 D7 E3 5A AF 3D 7E " +
            "E5 23 B3 1E B5 B0 12 FA 8F 9E 87 0A A7 D1 16 AD " +
            "5E A4 52 4A 04 74 F4 CC AB E4 D6 1F FE 7D A9 AE " +
            "65 F3 51 7F 7E FB BA 07",
            // After Rho
            "99 2D 4B 4E 82 CC 9F 39 01 2B CB B5 E0 03 E2 4F " +
            "4C 30 21 C5 6D 7F 5B B0 A4 E5 03 7D E7 34 DC C1 " +
            "51 69 D0 90 7F 6F EB 3D 65 FC 0F 4B 40 87 56 43 " +
            "3F F4 4E 9F 01 A3 A3 21 65 B5 70 6E AC 43 67 48 " +
            "F9 5B 91 74 B5 08 43 85 A2 D5 D8 CF C2 ED 66 D0 " +
            "2D 17 9C 88 59 57 7B 84 9F 39 27 BB 16 57 1B 43 " +
            "00 5F B8 65 3A 78 BE 6E 05 C7 4E 62 0B C6 6F 2A " +
            "09 B3 0C D0 0F 02 40 46 F3 D4 6B 50 9A BA 00 88 " +
            "11 32 0D 73 26 ED 01 E8 30 98 5A 1D AF 69 31 F5 " +
            "6C C6 74 E8 0F FC F9 DD 7E BB 67 D7 E3 5A AF 3D " +
            "4A E8 97 8F CC 7A D4 C2 3E 7A 1E 2A 9C 46 5B B4 " +
            "8B 54 4A 89 80 8E 9E D9 E4 D6 1F FE 7D A9 AE AB " +
            "EE 41 D9 7C D4 9F DF BE",
            // After Pi
            "99 2D 4B 4E 82 CC 9F 39 3F F4 4E 9F 01 A3 A3 21 " +
            "00 5F B8 65 3A 78 BE 6E 6C C6 74 E8 0F FC F9 DD " +
            "EE 41 D9 7C D4 9F DF BE A4 E5 03 7D E7 34 DC C1 " +
            "A2 D5 D8 CF C2 ED 66 D0 2D 17 9C 88 59 57 7B 84 " +
            "11 32 0D 73 26 ED 01 E8 8B 54 4A 89 80 8E 9E D9 " +
            "01 2B CB B5 E0 03 E2 4F 65 B5 70 6E AC 43 67 48 " +
            "05 C7 4E 62 0B C6 6F 2A 7E BB 67 D7 E3 5A AF 3D " +
            "4A E8 97 8F CC 7A D4 C2 51 69 D0 90 7F 6F EB 3D " +
            "65 FC 0F 4B 40 87 56 43 9F 39 27 BB 16 57 1B 43 " +
            "30 98 5A 1D AF 69 31 F5 E4 D6 1F FE 7D A9 AE AB " +
            "4C 30 21 C5 6D 7F 5B B0 F9 5B 91 74 B5 08 43 85 " +
            "09 B3 0C D0 0F 02 40 46 F3 D4 6B 50 9A BA 00 88 " +
            "3E 7A 1E 2A 9C 46 5B B4",
            // After Chi
            "99 26 FB 2E B8 94 83 77 53 74 0A 17 04 27 E2 B0 " +
            "82 5E 31 71 EA 7B B8 4C 7D EA 76 EA 0D BC F9 DC " +
            "C8 91 DD ED D5 BC FF BE A9 E7 07 7D FE 26 C5 C5 " +
            "B2 F5 D9 BC E4 45 66 B8 A7 53 DE 00 D9 55 E5 95 " +
            "35 93 0C 07 41 DD 41 E8 89 44 92 0B 80 47 BC C9 " +
            "01 69 C5 B5 E3 87 EA 6D 1F 8D 51 FB 4C 5B E7 5D " +
            "05 87 DE 6A 07 E6 3F E8 7F B8 2F E7 C3 5B 8D 30 " +
            "2E 7C A7 C5 C0 3A D1 C2 CB 68 F0 20 69 3F E2 3D " +
            "45 7C 57 4F E9 AF 76 F7 5B 7F 22 59 46 D7 95 49 " +
            "21 B1 9A 1D AD 2F 70 E1 C0 42 10 B5 7D 29 BA E9 " +
            "4C 90 2D 45 67 7D 5B F2 0B 1F F2 74 25 B0 43 0D " +
            "05 99 18 FA 0B 46 1B 72 B3 D4 4A 95 FB 83 00 88 " +
            "8F 31 8E 1A 0C 46 5B B1",
            // After Iota 
            "19 A6 FB 2E B8 94 83 F7 53 74 0A 17 04 27 E2 B0 " +
            "82 5E 31 71 EA 7B B8 4C 7D EA 76 EA 0D BC F9 DC " +
            "C8 91 DD ED D5 BC FF BE A9 E7 07 7D FE 26 C5 C5 " +
            "B2 F5 D9 BC E4 45 66 B8 A7 53 DE 00 D9 55 E5 95 " +
            "35 93 0C 07 41 DD 41 E8 89 44 92 0B 80 47 BC C9 " +
            "01 69 C5 B5 E3 87 EA 6D 1F 8D 51 FB 4C 5B E7 5D " +
            "05 87 DE 6A 07 E6 3F E8 7F B8 2F E7 C3 5B 8D 30 " +
            "2E 7C A7 C5 C0 3A D1 C2 CB 68 F0 20 69 3F E2 3D " +
            "45 7C 57 4F E9 AF 76 F7 5B 7F 22 59 46 D7 95 49 " +
            "21 B1 9A 1D AD 2F 70 E1 C0 42 10 B5 7D 29 BA E9 " +
            "4C 90 2D 45 67 7D 5B F2 0B 1F F2 74 25 B0 43 0D " +
            "05 99 18 FA 0B 46 1B 72 B3 D4 4A 95 FB 83 00 88 " +
            "8F 31 8E 1A 0C 46 5B B1",
            // Round #22
            // After Theta
            "58 A3 C3 74 9C 76 5C 44 99 7C F8 E4 5C E2 2F 35 " +
            "78 78 1D 1F 39 70 65 39 42 32 90 4A BD B8 F2 0C " +
            "00 95 91 68 5B C5 90 F3 E8 E2 3F 27 DA C4 1A 76 " +
            "78 FD 2B 4F BC 80 AB 3D 5D 75 F2 6E 0A 5E 38 E0 " +
            "0A 4B EA A7 F1 D9 4A 38 41 40 DE 8E 0E 3E D3 84 " +
            "40 6C FD EF C7 65 35 DE D5 85 A3 08 14 9E 2A D8 " +
            "FF A1 F2 04 D4 ED E2 9D 40 60 C9 47 73 5F 86 E0 " +
            "E6 78 EB 40 4E 43 BE 8F 8A 6D C8 7A 4D DD 3D 8E " +
            "8F 74 A5 BC B1 6A BB 72 A1 59 0E 37 95 DC 48 3C " +
            "1E 69 7C BD 1D 2B 7B 31 08 46 5C 30 F3 50 D5 A4 " +
            "0D 95 15 1F 43 9F 84 41 C1 17 00 87 7D 75 8E 88 " +
            "FF BF 34 94 D8 4D C6 07 8C 0C AC 35 4B 87 0B 58 " +
            "47 35 C2 9F 82 3F 34 FC",
            // After Rho
            "58 A3 C3 74 9C 76 5C 44 32 F9 F0 C9 B9 C4 5F 6A " +
            "1E 5E C7 47 0E 5C 59 0E 8B 2B CF 20 24 03 A9 D4 " +
            "2A 86 9C 07 A8 8C 44 DB A2 4D AC 61 87 2E FE 73 " +
            "F2 C4 0B B8 DA 83 D7 BF 78 57 9D BC 9B 82 17 0E " +
            "25 F5 D3 F8 6C 25 1C 85 33 4D 18 04 E4 ED E8 E0 " +
            "06 62 EB 7F 3F 2E AB F1 60 57 17 8E 22 50 78 AA " +
            "27 A0 6E 17 EF FC 0F 95 BE 0C C1 81 C0 92 8F E6 " +
            "20 A7 21 DF 47 73 BC 75 F5 9A BA 7B 1C 15 DB 90 " +
            "94 37 56 6D 57 EE 91 AE 24 9E D0 2C 87 9B 4A 6E " +
            "65 2F C6 23 8D AF B7 63 A4 08 46 5C 30 F3 50 D5 " +
            "12 06 35 54 56 7C 0C 7D 06 5F 00 1C F6 D5 39 22 " +
            "FF 97 86 12 BB C9 F8 E0 0C AC 35 4B 87 0B 58 8C " +
            "0D FF 51 8D F0 A7 E0 0F",
            // After Pi
            "58 A3 C3 74 9C 76 5C 44 F2 C4 0B B8 DA 83 D7 BF " +
            "27 A0 6E 17 EF FC 0F 95 65 2F C6 23 8D AF B7 63 " +
            "0D FF 51 8D F0 A7 E0 0F 8B 2B CF 20 24 03 A9 D4 " +
            "33 4D 18 04 E4 ED E8 E0 06 62 EB 7F 3F 2E AB F1 " +
            "94 37 56 6D 57 EE 91 AE FF 97 86 12 BB C9 F8 E0 " +
            "32 F9 F0 C9 B9 C4 5F 6A 78 57 9D BC 9B 82 17 0E " +
            "BE 0C C1 81 C0 92 8F E6 A4 08 46 5C 30 F3 50 D5 " +
            "12 06 35 54 56 7C 0C 7D 2A 86 9C 07 A8 8C 44 DB " +
            "A2 4D AC 61 87 2E FE 73 60 57 17 8E 22 50 78 AA " +
            "24 9E D0 2C 87 9B 4A 6E 0C AC 35 4B 87 0B 58 8C " +
            "1E 5E C7 47 0E 5C 59 0E 25 F5 D3 F8 6C 25 1C 85 " +
            "20 A7 21 DF 47 73 BC 75 F5 9A BA 7B 1C 15 DB 90 " +
            "06 5F 00 1C F6 D5 39 22",
            // After Chi
            "5D 83 A7 73 B9 0A 54 44 B2 CB 8B 98 DA 80 67 DD " +
            "2F 70 7F 9B 9F FC 4F 99 35 2F 44 53 81 FF AB 23 " +
            "AF BB 59 05 B2 26 63 B4 8F 09 2C 5B 3F 01 AA C5 " +
            "A3 58 0C 04 A4 2D F8 EE 6D E2 6B 6D 97 2F C3 B1 " +
            "94 1F 1F 4D 53 EC 90 BA CF D3 96 16 7B 25 B8 C0 " +
            "B4 F1 B0 C8 F9 D4 D7 8A 78 57 9B E0 AB E3 47 1F " +
            "AC 0A F0 81 86 9E 83 CE 84 F1 86 D5 99 73 03 D7 " +
            "5A 00 38 60 54 7E 0C 79 6A 94 8F 89 88 DC 44 53 " +
            "A6 C5 6C 41 02 A5 FC 37 68 77 32 CD 22 50 68 2A " +
            "06 9C 58 28 AF 1F 4E 3D 8C E5 15 2B 80 29 E2 AC " +
            "1E 5C E7 40 0D 0E F9 7E F0 ED 49 D8 74 21 5F 05 " +
            "22 E2 21 DB A5 B3 9C 57 ED 9A 7D 38 14 1D 9B 9C " +
            "27 FE 10 A4 96 F4 3D A3",
            // After Iota 
            "5C 83 A7 F3 B9 0A 54 44 B2 CB 8B 98 DA 80 67 DD " +
            "2F 70 7F 9B 9F FC 4F 99 35 2F 44 53 81 FF AB 23 " +
            "AF BB 59 05 B2 26 63 B4 8F 09 2C 5B 3F 01 AA C5 " +
            "A3 58 0C 04 A4 2D F8 EE 6D E2 6B 6D 97 2F C3 B1 " +
            "94 1F 1F 4D 53 EC 90 BA CF D3 96 16 7B 25 B8 C0 " +
            "B4 F1 B0 C8 F9 D4 D7 8A 78 57 9B E0 AB E3 47 1F " +
            "AC 0A F0 81 86 9E 83 CE 84 F1 86 D5 99 73 03 D7 " +
            "5A 00 38 60 54 7E 0C 79 6A 94 8F 89 88 DC 44 53 " +
            "A6 C5 6C 41 02 A5 FC 37 68 77 32 CD 22 50 68 2A " +
            "06 9C 58 28 AF 1F 4E 3D 8C E5 15 2B 80 29 E2 AC " +
            "1E 5C E7 40 0D 0E F9 7E F0 ED 49 D8 74 21 5F 05 " +
            "22 E2 21 DB A5 B3 9C 57 ED 9A 7D 38 14 1D 9B 9C " +
            "27 FE 10 A4 96 F4 3D A3",
            // Round #23
            // After Theta
            "B3 28 26 C5 75 3F AB 7A E8 63 36 F2 32 D1 04 CC " +
            "8D 13 B7 C9 DD F3 EE 58 B3 C5 57 CB 9F 10 41 BC " +
            "47 1A 06 8C B7 5F A6 16 60 A2 AD 6D F3 34 55 FB " +
            "F9 F0 B1 6E 4C 7C 9B FF CF 81 A3 3F D5 20 62 70 " +
            "12 F5 0C D5 4D 03 7A 25 27 72 C9 9F 7E 5C 7D 62 " +
            "5B 5A 31 FE 35 E1 28 B4 22 FF 26 8A 43 B2 24 0E " +
            "0E 69 38 D3 C4 91 22 0F 02 1B 95 4D 87 9C E9 48 " +
            "B2 A1 67 E9 51 07 C9 DB 85 3F 0E BF 44 E9 BB 6D " +
            "FC 6D D1 2B EA F4 9F 26 CA 14 FA 9F 60 5F C9 EB " +
            "80 76 4B B0 B1 F0 A4 A2 64 44 4A A2 85 50 27 0E " +
            "F1 F7 66 76 C1 3B 06 40 AA 45 F4 B2 9C 70 3C 14 " +
            "80 81 E9 89 E7 BC 3D 96 6B 70 6E A0 0A F2 71 03 " +
            "CF 5F 4F 2D 93 8D F8 01",
            // After Rho
            "B3 28 26 C5 75 3F AB 7A D1 C7 6C E4 65 A2 09 98 " +
            "E3 C4 6D 72 F7 BC 3B 56 09 11 C4 3B 5B 7C B5 FC " +
            "FD 32 B5 38 D2 30 60 BC 36 4F 53 B5 0F 26 DA DA " +
            "EB C6 C4 B7 F9 9F 0F 1F DC 73 E0 E8 4F 35 88 18 " +
            "7A 86 EA A6 01 BD 12 89 D5 27 76 22 97 FC E9 C7 " +
            "DD D2 8A F1 AF 09 47 A1 38 88 FC 9B 28 0E C9 92 " +
            "99 26 8E 14 79 70 48 C3 39 D3 91 04 36 2A 9B 0E " +
            "F4 A8 83 E4 6D D9 D0 B3 7E 89 D2 77 DB 0A 7F 1C " +
            "7A 45 9D FE D3 84 BF 2D E4 75 65 0A FD 4F B0 AF " +
            "9E 54 14 D0 6E 09 36 16 0E 64 44 4A A2 85 50 27 " +
            "18 00 C5 DF 9B D9 05 EF A8 16 D1 CB 72 C2 F1 50 " +
            "30 30 3D F1 9C B7 C7 12 70 6E A0 0A F2 71 03 6B " +
            "7E C0 F3 D7 53 CB 64 23",
            // After Pi
            "B3 28 26 C5 75 3F AB 7A EB C6 C4 B7 F9 9F 0F 1F " +
            "99 26 8E 14 79 70 48 C3 9E 54 14 D0 6E 09 36 16 " +
            "7E C0 F3 D7 53 CB 64 23 09 11 C4 3B 5B 7C B5 FC " +
            "D5 27 76 22 97 FC E9 C7 DD D2 8A F1 AF 09 47 A1 " +
            "7A 45 9D FE D3 84 BF 2D 30 30 3D F1 9C B7 C7 12 " +
            "D1 C7 6C E4 65 A2 09 98 DC 73 E0 E8 4F 35 88 18 " +
            "39 D3 91 04 36 2A 9B 0E 0E 64 44 4A A2 85 50 27 " +
            "18 00 C5 DF 9B D9 05 EF FD 32 B5 38 D2 30 60 BC " +
            "36 4F 53 B5 0F 26 DA DA 38 88 FC 9B 28 0E C9 92 " +
            "E4 75 65 0A FD 4F B0 AF 70 6E A0 0A F2 71 03 6B " +
            "E3 C4 6D 72 F7 BC 3B 56 7A 86 EA A6 01 BD 12 89 " +
            "F4 A8 83 E4 6D D9 D0 B3 7E 89 D2 77 DB 0A 7F 1C " +
            "A8 16 D1 CB 72 C2 F1 50",
            // After Chi
            "A3 08 2C C5 75 5F EB BA ED 96 D4 77 FF 96 39 0B " +
            "F9 A6 6D 13 68 B2 08 E2 1F 7C 10 D0 4A 3D BD 4E " +
            "36 06 33 E5 DB 4B 60 26 01 C1 4C EA 73 7D B3 DC " +
            "F7 22 63 2C C7 78 51 CB DD E2 AA F0 A3 3A 07 B3 " +
            "73 44 5D F4 90 CC 8F C1 E4 16 0F F1 18 37 8F 11 " +
            "F0 47 7D E0 55 A8 1A 9E DA 57 A4 A2 CF B0 C8 39 " +
            "29 D3 10 91 2F 72 9E C6 CF A3 6C 6A C6 A7 58 37 " +
            "14 30 45 D7 91 CC 85 EF F5 B2 19 32 F2 38 61 BC " +
            "F2 3A 52 B5 DA 67 EA F7 28 82 7C 9B 2A 3E CA D2 " +
            "69 65 70 3A FD 4F D0 3B 72 23 E2 8F FF 77 99 29 " +
            "67 EC 6C 32 9B FC FB 64 70 87 BA B5 93 BF 3D 85 " +
            "74 BE 82 6C 4D 19 50 F3 3D 49 FE 47 5E 36 75 1A " +
            "B0 14 53 4F 72 C3 F1 D9",
            // After Iota 
            "AB 88 2C 45 75 5F EB 3A ED 96 D4 77 FF 96 39 0B " +
            "F9 A6 6D 13 68 B2 08 E2 1F 7C 10 D0 4A 3D BD 4E " +
            "36 06 33 E5 DB 4B 60 26 01 C1 4C EA 73 7D B3 DC " +
            "F7 22 63 2C C7 78 51 CB DD E2 AA F0 A3 3A 07 B3 " +
            "73 44 5D F4 90 CC 8F C1 E4 16 0F F1 18 37 8F 11 " +
            "F0 47 7D E0 55 A8 1A 9E DA 57 A4 A2 CF B0 C8 39 " +
            "29 D3 10 91 2F 72 9E C6 CF A3 6C 6A C6 A7 58 37 " +
            "14 30 45 D7 91 CC 85 EF F5 B2 19 32 F2 38 61 BC " +
            "F2 3A 52 B5 DA 67 EA F7 28 82 7C 9B 2A 3E CA D2 " +
            "69 65 70 3A FD 4F D0 3B 72 23 E2 8F FF 77 99 29 " +
            "67 EC 6C 32 9B FC FB 64 70 87 BA B5 93 BF 3D 85 " +
            "74 BE 82 6C 4D 19 50 F3 3D 49 FE 47 5E 36 75 1A " +
            "B0 14 53 4F 72 C3 F1 D9",
            // Xor'd state (in bytes)
            "AB 88 2C 45 75 5F EB 3A ED 96 D4 77 FF 96 39 0B " +
            "F9 A6 6D 13 68 B2 08 E2 1F 7C 10 D0 4A 3D BD 4E " +
            "36 06 33 E5 DB 4B 60 26 01 C1 4C EA 73 7D B3 DC " +
            "F7 22 63 2C C7 78 51 CB DD E2 AA F0 A3 3A 07 B3 " +
            "73 44 5D F4 90 CC 8F C1 E4 16 0F F1 18 37 8F 11 " +
            "F0 47 7D E0 55 A8 1A 9E DA 57 A4 A2 CF B0 C8 39 " +
            "29 D3 10 91 2F 72 9E C6 CF A3 6C 6A C6 A7 58 37 " +
            "14 30 45 D7 91 CC 85 EF F5 B2 19 32 F2 38 61 BC " +
            "F2 3A 52 B5 DA 67 EA F7 28 82 7C 9B 2A 3E CA D2 " +
            "69 65 70 3A FD 4F D0 3B 72 23 E2 8F FF 77 99 29 " +
            "67 EC 6C 32 9B FC FB 64 70 87 BA B5 93 BF 3D 85 " +
            "74 BE 82 6C 4D 19 50 F3 3D 49 FE 47 5E 36 75 1A " +
            "B0 14 53 4F 72 C3 F1 D9",
            // Round #0
            // After Theta
            "2A 23 12 B5 D7 56 06 04 86 90 EF 32 C2 63 F6 C7 " +
            "54 97 C9 8D A8 6B E1 5A 46 F9 A9 52 77 E9 B2 A8 " +
            "50 10 4C 48 10 F8 1F FE 80 6A 72 1A D1 74 5E E2 " +
            "9C 24 58 69 FA 8D 9E 07 70 D3 0E 6E 63 E3 EE 0B " +
            "2A C1 E4 76 AD 18 80 27 82 00 70 5C D3 84 F0 C9 " +
            "71 EC 43 10 F7 A1 F7 A0 B1 51 9F E7 F2 45 07 F5 " +
            "84 E2 B4 0F EF AB 77 7E 96 26 D5 E8 FB 73 57 D1 " +
            "72 26 3A 7A 5A 7F FA 37 74 19 27 C2 50 31 8C 82 " +
            "99 3C 69 F0 E7 92 25 3B 85 B3 D8 05 EA E7 23 6A " +
            "30 E0 C9 B8 C0 9B DF DD 14 35 9D 22 34 C4 E6 F1 " +
            "E6 47 52 C2 39 F5 16 5A 1B 81 81 F0 AE 4A F2 49 " +
            "D9 8F 26 F2 8D C0 B9 4B 64 CC 47 C5 63 E2 7A FC " +
            "D6 02 2C E2 B9 70 8E 01",
            // After Rho
            "2A 23 12 B5 D7 56 06 04 0D 21 DF 65 84 C7 EC 8F " +
            "D5 65 72 23 EA 5A B8 16 97 2E 8B 6A 94 9F 2A 75 " +
            "C0 FF F0 87 82 60 42 82 11 4D E7 25 0E A8 26 A7 " +
            "95 A6 DF E8 79 C0 49 82 02 DC B4 83 DB D8 B8 FB " +
            "60 72 BB 56 0C C0 13 95 08 9F 2C 08 00 C7 35 4D " +
            "8D 63 1F 82 B8 0F BD 07 D4 C7 46 7D 9E CB 17 1D " +
            "7D 78 5F BD F3 23 14 A7 E7 AE A2 2D 4D AA D1 F7 " +
            "3D AD 3F FD 1B 39 13 1D 84 A1 62 18 05 E9 32 4E " +
            "0D FE 5C B2 64 27 93 27 11 B5 C2 59 EC 02 F5 F3 " +
            "F3 BB 1B 06 3C 19 17 78 F1 14 35 9D 22 34 C4 E6 " +
            "5B 68 99 1F 49 09 E7 D4 6D 04 06 C2 BB 2A C9 27 " +
            "FB D1 44 BE 11 38 77 29 CC 47 C5 63 E2 7A FC 64 " +
            "63 80 B5 00 8B 78 2E 9C",
            // After Pi
            "2A 23 12 B5 D7 56 06 04 95 A6 DF E8 79 C0 49 82 " +
            "7D 78 5F BD F3 23 14 A7 F3 BB 1B 06 3C 19 17 78 " +
            "63 80 B5 00 8B 78 2E 9C 97 2E 8B 6A 94 9F 2A 75 " +
            "08 9F 2C 08 00 C7 35 4D 8D 63 1F 82 B8 0F BD 07 " +
            "0D FE 5C B2 64 27 93 27 FB D1 44 BE 11 38 77 29 " +
            "0D 21 DF 65 84 C7 EC 8F 02 DC B4 83 DB D8 B8 FB " +
            "E7 AE A2 2D 4D AA D1 F7 F1 14 35 9D 22 34 C4 E6 " +
            "5B 68 99 1F 49 09 E7 D4 C0 FF F0 87 82 60 42 82 " +
            "11 4D E7 25 0E A8 26 A7 D4 C7 46 7D 9E CB 17 1D " +
            "11 B5 C2 59 EC 02 F5 F3 CC 47 C5 63 E2 7A FC 64 " +
            "D5 65 72 23 EA 5A B8 16 60 72 BB 56 0C C0 13 95 " +
            "3D AD 3F FD 1B 39 13 1D 84 A1 62 18 05 E9 32 4E " +
            "6D 04 06 C2 BB 2A C9 27",
            // After Chi
            "42 7B 12 A0 55 75 12 21 17 25 DF EA 75 D8 4A DA " +
            "7D 78 FB BD 70 43 3C 23 FB 98 19 B3 68 1F 17 78 " +
            "F6 04 78 48 A3 F8 67 1E 12 4E 98 E8 2C 97 A2 77 " +
            "08 03 6C 38 44 E7 37 6D 7F 62 1F 8E A9 17 D9 0F " +
            "09 D0 D7 F2 E0 A0 9B 73 F3 40 60 BE 11 78 62 21 " +
            "E8 03 DD 49 80 E5 AD 8B 12 CC A1 13 F9 CC BC FB " +
            "ED C6 2A 2F 04 A3 F2 E7 F5 15 73 FD A6 F2 CC ED " +
            "59 B4 B9 9D 12 11 F7 A4 04 7D F0 DF 12 23 53 9A " +
            "10 7D 67 25 6E A8 C6 45 18 85 43 5F 9C B3 1F 19 " +
            "11 0D F2 DD EC 02 F7 71 DD 47 C2 43 EE F2 D8 41 " +
            "C8 E8 76 8A F9 63 B8 1E E0 72 FB 56 08 00 33 D7 " +
            "54 A9 3B 3F A1 3B DA 3C 14 C0 12 39 45 B9 02 5E " +
            "4D 16 8F 96 BF AA CA A6",
            // After Iota 
            "43 7B 12 A0 55 75 12 21 17 25 DF EA 75 D8 4A DA " +
            "7D 78 FB BD 70 43 3C 23 FB 98 19 B3 68 1F 17 78 " +
            "F6 04 78 48 A3 F8 67 1E 12 4E 98 E8 2C 97 A2 77 " +
            "08 03 6C 38 44 E7 37 6D 7F 62 1F 8E A9 17 D9 0F " +
            "09 D0 D7 F2 E0 A0 9B 73 F3 40 60 BE 11 78 62 21 " +
            "E8 03 DD 49 80 E5 AD 8B 12 CC A1 13 F9 CC BC FB " +
            "ED C6 2A 2F 04 A3 F2 E7 F5 15 73 FD A6 F2 CC ED " +
            "59 B4 B9 9D 12 11 F7 A4 04 7D F0 DF 12 23 53 9A " +
            "10 7D 67 25 6E A8 C6 45 18 85 43 5F 9C B3 1F 19 " +
            "11 0D F2 DD EC 02 F7 71 DD 47 C2 43 EE F2 D8 41 " +
            "C8 E8 76 8A F9 63 B8 1E E0 72 FB 56 08 00 33 D7 " +
            "54 A9 3B 3F A1 3B DA 3C 14 C0 12 39 45 B9 02 5E " +
            "4D 16 8F 96 BF AA CA A6",
            // Round #1
            // After Theta
            "74 11 E3 7B F9 0B 9A E1 25 67 63 47 A7 60 18 5E " +
            "85 BD CE BF D0 F5 63 6E C0 2B 76 B2 6B F3 04 6F " +
            "1E D2 86 B9 00 80 3E 64 25 24 69 33 80 E9 2A B7 " +
            "3A 41 D0 95 96 5F 65 E9 87 A7 2A 8C 09 A1 86 42 " +
            "32 63 B8 F3 E3 4C 88 64 1B 96 9E 4F B2 00 3B 5B " +
            "DF 69 2C 92 2C 9B 25 4B 20 8E 1D BE 2B 74 EE 7F " +
            "15 03 1F 2D A4 15 AD AA CE A6 1C FC A5 1E DF FA " +
            "B1 62 47 6C B1 69 AE DE 33 17 01 04 BE 5D DB 5A " +
            "22 3F DB 88 BC 10 94 C1 E0 40 76 5D 3C 05 40 54 " +
            "2A BE 9D DC EF EE E4 66 35 91 3C B2 4D 8A 81 3B " +
            "FF 82 87 51 55 1D 30 DE D2 30 47 FB DA B8 61 53 " +
            "AC 6C 0E 3D 01 8D 85 71 2F 73 7D 38 46 55 11 49 " +
            "A5 C0 71 67 1C D2 93 DC",
            // After Rho
            "74 11 E3 7B F9 0B 9A E1 4A CE C6 8E 4E C1 30 BC " +
            "61 AF F3 2F 74 FD 98 5B 36 4F F0 06 BC 62 27 BB " +
            "00 F4 21 F3 90 36 CC 05 03 98 AE 72 5B 42 92 36 " +
            "5D 69 F9 55 96 AE 13 04 D0 E1 A9 0A 63 42 A8 A1 " +
            "31 DC F9 71 26 44 32 99 B0 B3 B5 61 E9 F9 24 0B " +
            "FA 4E 63 91 64 D9 2C 59 FF 81 38 76 F8 AE D0 B9 " +
            "68 21 AD 68 55 AD 18 F8 3D BE F5 9D 4D 39 F8 4B " +
            "B6 D8 34 57 EF 58 B1 23 08 7C BB B6 B5 66 2E 02 " +
            "1B 91 17 82 32 58 E4 67 20 2A 70 20 BB 2E 9E 02 " +
            "9D DC 4C C5 B7 93 FB DD 3B 35 91 3C B2 4D 8A 81 " +
            "C0 78 FF 0B 1E 46 55 75 49 C3 1C ED 6B E3 86 4D " +
            "95 CD A1 27 A0 B1 30 8E 73 7D 38 46 55 11 49 2F " +
            "24 77 29 70 DC 19 87 F4",
            // After Pi
            "74 11 E3 7B F9 0B 9A E1 5D 69 F9 55 96 AE 13 04 " +
            "68 21 AD 68 55 AD 18 F8 9D DC 4C C5 B7 93 FB DD " +
            "24 77 29 70 DC 19 87 F4 36 4F F0 06 BC 62 27 BB " +
            "B0 B3 B5 61 E9 F9 24 0B FA 4E 63 91 64 D9 2C 59 " +
            "1B 91 17 82 32 58 E4 67 95 CD A1 27 A0 B1 30 8E " +
            "4A CE C6 8E 4E C1 30 BC D0 E1 A9 0A 63 42 A8 A1 " +
            "3D BE F5 9D 4D 39 F8 4B 3B 35 91 3C B2 4D 8A 81 " +
            "C0 78 FF 0B 1E 46 55 75 00 F4 21 F3 90 36 CC 05 " +
            "03 98 AE 72 5B 42 92 36 FF 81 38 76 F8 AE D0 B9 " +
            "20 2A 70 20 BB 2E 9E 02 73 7D 38 46 55 11 49 2F " +
            "61 AF F3 2F 74 FD 98 5B 31 DC F9 71 26 44 32 99 " +
            "B6 D8 34 57 EF 58 B1 23 08 7C BB B6 B5 66 2E 02 " +
            "49 C3 1C ED 6B E3 86 4D",
            // After Chi
            "54 11 E7 53 B8 0A 92 19 C8 B5 B9 D0 34 BC F0 01 " +
            "48 02 8C 58 1D A5 1C D8 CD DC 8E CE 96 91 E3 DC " +
            "2D 1F 31 74 DA BD 86 F0 7C 03 B2 96 B8 62 2F EB " +
            "B1 22 A1 63 FB F9 E4 2D 7E 02 C3 B4 E4 78 3C D1 " +
            "39 93 47 82 2E 1A E3 56 15 7D A4 46 E1 28 30 8E " +
            "67 D0 92 1B 42 F8 60 F6 D2 E0 A9 2A D1 06 AA 21 " +
            "FD F6 9B 9E 41 3B AD 3F 31 B3 91 B8 F2 CC AA 09 " +
            "50 59 D6 0B 3F 44 DD 74 FC F5 31 F7 30 9A 8C 8C " +
            "03 B2 EE 72 58 42 9C 34 AC D4 30 30 BC BF 91 94 " +
            "20 AA 71 91 3B 08 1A 02 70 75 B6 46 1E 51 5B 1D " +
            "E7 AF F7 29 BD E5 19 79 39 F8 72 D1 36 62 3C 99 " +
            "F7 5B 30 1E A5 D9 31 6E 28 50 58 B4 A1 7A 36 10 " +
            "59 93 14 BD 69 E3 A4 CD",
            // After Iota 
            "D6 91 E7 53 B8 0A 92 19 C8 B5 B9 D0 34 BC F0 01 " +
            "48 02 8C 58 1D A5 1C D8 CD DC 8E CE 96 91 E3 DC " +
            "2D 1F 31 74 DA BD 86 F0 7C 03 B2 96 B8 62 2F EB " +
            "B1 22 A1 63 FB F9 E4 2D 7E 02 C3 B4 E4 78 3C D1 " +
            "39 93 47 82 2E 1A E3 56 15 7D A4 46 E1 28 30 8E " +
            "67 D0 92 1B 42 F8 60 F6 D2 E0 A9 2A D1 06 AA 21 " +
            "FD F6 9B 9E 41 3B AD 3F 31 B3 91 B8 F2 CC AA 09 " +
            "50 59 D6 0B 3F 44 DD 74 FC F5 31 F7 30 9A 8C 8C " +
            "03 B2 EE 72 58 42 9C 34 AC D4 30 30 BC BF 91 94 " +
            "20 AA 71 91 3B 08 1A 02 70 75 B6 46 1E 51 5B 1D " +
            "E7 AF F7 29 BD E5 19 79 39 F8 72 D1 36 62 3C 99 " +
            "F7 5B 30 1E A5 D9 31 6E 28 50 58 B4 A1 7A 36 10 " +
            "59 93 14 BD 69 E3 A4 CD",
            // Round #2
            // After Theta
            "B4 37 5C E5 2B AF 3A 83 3F 5E 10 69 B9 52 E3 68 " +
            "42 32 43 C0 CC AD 0E 5B DE 1F 99 17 D0 D7 E6 A5 " +
            "4D 28 42 A5 94 57 91 83 1E A5 09 20 2B C7 87 71 " +
            "46 C9 08 DA 76 17 F7 44 74 32 0C 2C 35 70 2E 52 " +
            "2A 50 50 5B 68 5C E6 2F 75 4A D7 97 AF C2 27 FD " +
            "05 76 29 AD D1 5D C8 6C 25 0B 00 93 5C E8 B9 48 " +
            "F7 C6 54 06 90 33 BF BC 22 70 86 61 B4 8A AF 70 " +
            "30 6E A5 DA 71 AE CA 07 9E 53 8A 41 A3 3F 24 16 " +
            "F4 59 47 CB D5 AC 8F 5D A6 E4 FF A8 6D B7 83 17 " +
            "33 69 66 48 7D 4E 1F 7B 10 42 C5 97 50 BB 4C 6E " +
            "85 09 4C 9F 2E 40 B1 E3 CE 13 DB 68 BB 8C 2F F0 " +
            "FD 6B FF 86 74 D1 23 ED 3B 93 4F 6D E7 3C 33 69 " +
            "39 A4 67 6C 27 09 B3 BE",
            // After Rho
            "B4 37 5C E5 2B AF 3A 83 7E BC 20 D2 72 A5 C6 D1 " +
            "90 CC 10 30 73 AB C3 96 7D 6D 5E EA FD 91 79 01 " +
            "BC 8A 1C 6C 42 11 2A A5 B2 72 7C 18 E7 51 9A 00 " +
            "A0 6D 77 71 4F 64 94 8C 14 9D 0C 03 4B 0D 9C 8B " +
            "28 A8 2D 34 2E F3 17 15 7C D2 5F A7 74 7D F9 2A " +
            "2B B0 4B 69 8D EE 42 66 22 95 2C 00 4C 72 A1 E7 " +
            "32 80 9C F9 E5 BD 37 A6 15 5F E1 44 E0 0C C3 68 " +
            "ED 38 57 E5 03 18 B7 52 83 46 7F 48 2C 3C A7 14 " +
            "68 B9 9A F5 B1 8B 3E EB C1 0B 53 F2 7F D4 B6 DB " +
            "E9 63 6F 26 CD 0C A9 CF 6E 10 42 C5 97 50 BB 4C " +
            "C5 8E 17 26 30 7D BA 00 3B 4F 6C A3 ED 32 BE C0 " +
            "7F ED DF 90 2E 7A A4 BD 93 4F 6D E7 3C 33 69 3B " +
            "AC 6F 0E E9 19 DB 49 C2",
            // After Pi
            "B4 37 5C E5 2B AF 3A 83 A0 6D 77 71 4F 64 94 8C " +
            "32 80 9C F9 E5 BD 37 A6 E9 63 6F 26 CD 0C A9 CF " +
            "AC 6F 0E E9 19 DB 49 C2 7D 6D 5E EA FD 91 79 01 " +
            "7C D2 5F A7 74 7D F9 2A 2B B0 4B 69 8D EE 42 66 " +
            "68 B9 9A F5 B1 8B 3E EB 7F ED DF 90 2E 7A A4 BD " +
            "7E BC 20 D2 72 A5 C6 D1 14 9D 0C 03 4B 0D 9C 8B " +
            "15 5F E1 44 E0 0C C3 68 6E 10 42 C5 97 50 BB 4C " +
            "C5 8E 17 26 30 7D BA 00 BC 8A 1C 6C 42 11 2A A5 " +
            "B2 72 7C 18 E7 51 9A 00 22 95 2C 00 4C 72 A1 E7 " +
            "C1 0B 53 F2 7F D4 B6 DB 93 4F 6D E7 3C 33 69 3B " +
            "90 CC 10 30 73 AB C3 96 28 A8 2D 34 2E F3 17 15 " +
            "ED 38 57 E5 03 18 B7 52 83 46 7F 48 2C 3C A7 14 " +
            "3B 4F 6C A3 ED 32 BE C0",
            // After Chi
            "A6 B7 D4 6D 8B 36 19 A1 69 0E 14 77 47 64 1C C5 " +
            "36 8C 9C 30 F5 6E 77 A6 F9 73 3F 22 EF 28 9B CE " +
            "AC 27 2D F9 5D 9B CD CE 7E 4D 5E A2 74 13 7B 45 " +
            "3C DB CF 33 44 7C C5 A3 3C F4 0E 69 83 9E C2 72 " +
            "68 B9 9A 9F 60 0A 67 EB 7F 7F DE 95 2E 16 24 97 " +
            "7F FE C1 96 D2 A5 85 B1 7E 9D 0E 82 5C 5D A4 8F " +
            "94 D1 F4 66 C0 21 C3 68 54 20 62 15 D5 D0 FF 9D " +
            "C5 8F 1B 27 39 75 A2 0A BC 0F 1C 6C 4A 33 0B 42 " +
            "73 78 2F EA D4 D5 8C 18 30 D1 00 05 4C 51 E8 C7 " +
            "ED 8B 43 FA 3D D4 B4 5F 91 3F 0D F7 99 73 F9 3B " +
            "55 DC 42 F1 72 A3 63 D4 2A EE 05 3C 02 D7 17 11 " +
            "D5 31 57 46 C2 1A AF 92 03 C6 6F 58 3E B5 E6 02 " +
            "13 6F 41 A7 E1 62 AA C1",
            // After Iota 
            "2C 37 D4 6D 8B 36 19 21 69 0E 14 77 47 64 1C C5 " +
            "36 8C 9C 30 F5 6E 77 A6 F9 73 3F 22 EF 28 9B CE " +
            "AC 27 2D F9 5D 9B CD CE 7E 4D 5E A2 74 13 7B 45 " +
            "3C DB CF 33 44 7C C5 A3 3C F4 0E 69 83 9E C2 72 " +
            "68 B9 9A 9F 60 0A 67 EB 7F 7F DE 95 2E 16 24 97 " +
            "7F FE C1 96 D2 A5 85 B1 7E 9D 0E 82 5C 5D A4 8F " +
            "94 D1 F4 66 C0 21 C3 68 54 20 62 15 D5 D0 FF 9D " +
            "C5 8F 1B 27 39 75 A2 0A BC 0F 1C 6C 4A 33 0B 42 " +
            "73 78 2F EA D4 D5 8C 18 30 D1 00 05 4C 51 E8 C7 " +
            "ED 8B 43 FA 3D D4 B4 5F 91 3F 0D F7 99 73 F9 3B " +
            "55 DC 42 F1 72 A3 63 D4 2A EE 05 3C 02 D7 17 11 " +
            "D5 31 57 46 C2 1A AF 92 03 C6 6F 58 3E B5 E6 02 " +
            "13 6F 41 A7 E1 62 AA C1",
            // Round #3
            // After Theta
            "5D 0C 8F 57 AB 50 CD 49 5A CB 63 4B 22 40 F0 54 " +
            "13 1C B4 35 CE 0F 32 8C AB 35 47 69 B3 60 9B 75 " +
            "0F 2F EC 7B 2F 28 82 AC 0F 76 05 98 54 75 AF 2D " +
            "0F 1E B8 0F 21 58 29 32 19 64 26 6C B8 FF 87 58 " +
            "3A FF E2 D4 3C 42 67 50 DC 77 1F 17 5C A5 6B F5 " +
            "0E C5 9A AC F2 C3 51 D9 4D 58 79 BE 39 79 48 1E " +
            "B1 41 DC 63 FB 40 86 42 06 66 1A 5E 89 98 FF 26 " +
            "66 87 DA A5 4B C6 ED 68 CD 34 47 56 6A 55 DF 2A " +
            "40 BD 58 D6 B1 F1 60 89 15 41 28 00 77 30 AD ED " +
            "BF CD 3B B1 61 9C B4 E4 32 37 CC 75 EB C0 B6 59 " +
            "24 E7 19 CB 52 C5 B7 BC 19 2B 72 00 67 F3 FB 80 " +
            "F0 A1 7F 43 F9 7B EA B8 51 80 17 13 62 FD E6 B9 " +
            "B0 67 80 25 93 D1 E5 A3",
            // After Rho
            "5D 0C 8F 57 AB 50 CD 49 B4 96 C7 96 44 80 E0 A9 " +
            "04 07 6D 8D F3 83 0C E3 0B B6 59 B7 5A 73 94 36 " +
            "41 11 64 7D 78 61 DF 7B 49 55 F7 DA F2 60 57 80 " +
            "FB 10 82 95 22 F3 E0 81 56 06 99 09 1B EE FF 21 " +
            "7F 71 6A 1E A1 33 28 9D BA 56 CF 7D F7 71 C1 55 " +
            "76 28 D6 64 95 1F 8E CA 79 34 61 E5 F9 E6 E4 21 " +
            "1E DB 07 32 14 8A 0D E2 31 FF 4D 0C CC 34 BC 12 " +
            "D2 25 E3 76 34 B3 43 ED AC D4 AA BE 55 9A 69 8E " +
            "CB 3A 36 1E 2C 11 A8 17 D6 F6 8A 20 14 80 3B 98 " +
            "93 96 FC B7 79 27 36 8C 59 32 37 CC 75 EB C0 B6 " +
            "DF F2 92 9C 67 2C 4B 15 66 AC C8 01 9C CD EF 03 " +
            "3E F4 6F 28 7F 4F 1D 17 80 17 13 62 FD E6 B9 51 " +
            "F9 28 EC 19 60 C9 64 74",
            // After Pi
            "5D 0C 8F 57 AB 50 CD 49 FB 10 82 95 22 F3 E0 81 " +
            "1E DB 07 32 14 8A 0D E2 93 96 FC B7 79 27 36 8C " +
            "F9 28 EC 19 60 C9 64 74 0B B6 59 B7 5A 73 94 36 " +
            "BA 56 CF 7D F7 71 C1 55 76 28 D6 64 95 1F 8E CA " +
            "CB 3A 36 1E 2C 11 A8 17 3E F4 6F 28 7F 4F 1D 17 " +
            "B4 96 C7 96 44 80 E0 A9 56 06 99 09 1B EE FF 21 " +
            "31 FF 4D 0C CC 34 BC 12 59 32 37 CC 75 EB C0 B6 " +
            "DF F2 92 9C 67 2C 4B 15 41 11 64 7D 78 61 DF 7B " +
            "49 55 F7 DA F2 60 57 80 79 34 61 E5 F9 E6 E4 21 " +
            "D6 F6 8A 20 14 80 3B 98 80 17 13 62 FD E6 B9 51 " +
            "04 07 6D 8D F3 83 0C E3 7F 71 6A 1E A1 33 28 9D " +
            "D2 25 E3 76 34 B3 43 ED AC D4 AA BE 55 9A 69 8E " +
            "66 AC C8 01 9C CD EF 03",
            // After Chi
            "59 C7 8A 75 BF 58 C0 2B 7A 14 7A 10 4B D6 D2 8D " +
            "76 F3 07 3A 14 42 4D 92 97 92 FF F1 F2 37 BF 85 " +
            "5B 38 EC 99 60 6A 44 F4 4F 9E 49 B7 5A 7D 9A BC " +
            "33 44 EF 67 DF 71 E1 40 42 EC 9F 44 C6 51 9B CA " +
            "CA 38 26 89 2C 21 28 37 8E B4 E9 60 DA 4F 5C 56 " +
            "95 6F 83 92 80 90 E0 BB 1E 06 AB C9 2A 25 BF 85 " +
            "B7 3F CD 1C CE 30 B7 13 79 36 72 CE 75 6B 60 1E " +
            "9D F2 8A 95 7C 42 54 15 71 31 64 58 71 E7 7F 5A " +
            "CF 97 7D DA F6 60 4C 18 79 35 70 A7 10 80 64 60 " +
            "97 F6 EE 3D 14 81 7D B2 88 53 80 E0 7F E6 B9 D1 " +
            "84 03 EC ED E7 03 4F 83 53 A1 62 96 E0 3B 00 9F " +
            "90 0D A3 77 BC F6 C5 EC AC D7 8F 32 36 98 69 6E " +
            "1D DC CA 13 9C FD CF 1F",
            // After Iota 
            "59 47 8A F5 BF 58 C0 AB 7A 14 7A 10 4B D6 D2 8D " +
            "76 F3 07 3A 14 42 4D 92 97 92 FF F1 F2 37 BF 85 " +
            "5B 38 EC 99 60 6A 44 F4 4F 9E 49 B7 5A 7D 9A BC " +
            "33 44 EF 67 DF 71 E1 40 42 EC 9F 44 C6 51 9B CA " +
            "CA 38 26 89 2C 21 28 37 8E B4 E9 60 DA 4F 5C 56 " +
            "95 6F 83 92 80 90 E0 BB 1E 06 AB C9 2A 25 BF 85 " +
            "B7 3F CD 1C CE 30 B7 13 79 36 72 CE 75 6B 60 1E " +
            "9D F2 8A 95 7C 42 54 15 71 31 64 58 71 E7 7F 5A " +
            "CF 97 7D DA F6 60 4C 18 79 35 70 A7 10 80 64 60 " +
            "97 F6 EE 3D 14 81 7D B2 88 53 80 E0 7F E6 B9 D1 " +
            "84 03 EC ED E7 03 4F 83 53 A1 62 96 E0 3B 00 9F " +
            "90 0D A3 77 BC F6 C5 EC AC D7 8F 32 36 98 69 6E " +
            "1D DC CA 13 9C FD CF 1F",
            // Round #4
            // After Theta
            "13 77 0D 8E CB 97 7B 4D D9 A0 BE 10 D9 2C D8 77 " +
            "83 E9 B3 BB AF 52 4B BC 47 69 F2 7C 09 9A 0B B0 " +
            "A8 8D B7 EB 0F AD B3 6F 05 AE CE CC 2E B2 21 5A " +
            "90 F0 2B 67 4D 8B EB BA B7 F6 2B C5 7D 41 9D E4 " +
            "1A C3 2B 04 D7 8C 9C 02 7D 01 B2 12 B5 88 AB CD " +
            "DF 5F 04 E9 F4 5F 5B 5D BD B2 6F C9 B8 DF B5 7F " +
            "42 25 79 9D 75 20 B1 3D A9 CD 7F 43 8E C6 D4 2B " +
            "6E 47 D1 E7 13 85 A3 8E 3B 01 E3 23 05 28 C4 BC " +
            "6C 23 B9 DA 64 9A 46 E2 8C 2F C4 26 AB 90 62 4E " +
            "47 0D E3 B0 EF 2C C9 87 7B E6 DB 92 10 21 4E 4A " +
            "CE 33 6B 96 93 CC F4 65 F0 15 A6 96 72 C1 0A 65 " +
            "65 17 17 F6 07 E6 C3 C2 7C 2C 82 BF CD 35 DD 5B " +
            "EE 69 91 61 F3 3A 38 84",
            // After Rho
            "13 77 0D 8E CB 97 7B 4D B2 41 7D 21 B2 59 B0 EF " +
            "60 FA EC EE AB D4 12 EF A0 B9 00 7B 94 26 CF 97 " +
            "68 9D 7D 43 6D BC 5D 7F EC 22 1B A2 55 E0 EA CC " +
            "72 D6 B4 B8 AE 0B 09 BF F9 AD FD 4A 71 5F 50 27 " +
            "E1 15 82 6B 46 4E 01 8D B8 DA DC 17 20 2B 51 8B " +
            "FA FE 22 48 A7 FF DA EA FE F5 CA BE 25 E3 7E D7 " +
            "EB AC 03 89 ED 11 2A C9 8D A9 57 52 9B FF 86 1C " +
            "F3 89 C2 51 47 B7 A3 E8 47 0A 50 88 79 77 02 C6 " +
            "57 9B 4C D3 48 9C 6D 24 31 27 C6 17 62 93 55 48 " +
            "25 F9 F0 A8 61 1C F6 9D 4A 7B E6 DB 92 10 21 4E " +
            "D3 97 39 CF AC 59 4E 32 C1 57 98 5A CA 05 2B 94 " +
            "EC E2 C2 FE C0 7C 58 B8 2C 82 BF CD 35 DD 5B 7C " +
            "0E A1 7B 5A 64 D8 BC 0E",
            // After Pi
            "13 77 0D 8E CB 97 7B 4D 72 D6 B4 B8 AE 0B 09 BF " +
            "EB AC 03 89 ED 11 2A C9 25 F9 F0 A8 61 1C F6 9D " +
            "0E A1 7B 5A 64 D8 BC 0E A0 B9 00 7B 94 26 CF 97 " +
            "B8 DA DC 17 20 2B 51 8B FA FE 22 48 A7 FF DA EA " +
            "57 9B 4C D3 48 9C 6D 24 EC E2 C2 FE C0 7C 58 B8 " +
            "B2 41 7D 21 B2 59 B0 EF F9 AD FD 4A 71 5F 50 27 " +
            "8D A9 57 52 9B FF 86 1C 4A 7B E6 DB 92 10 21 4E " +
            "D3 97 39 CF AC 59 4E 32 68 9D 7D 43 6D BC 5D 7F " +
            "EC 22 1B A2 55 E0 EA CC FE F5 CA BE 25 E3 7E D7 " +
            "31 27 C6 17 62 93 55 48 2C 82 BF CD 35 DD 5B 7C " +
            "60 FA EC EE AB D4 12 EF E1 15 82 6B 46 4E 01 8D " +
            "F3 89 C2 51 47 B7 A3 E8 47 0A 50 88 79 77 02 C6 " +
            "C1 57 98 5A CA 05 2B 94",
            // After Chi
            "9A 5F 0E 8F 8A 87 59 0D 76 87 44 98 AE 07 DD AB " +
            "E1 AC 08 DB E9 D1 22 CB 34 AF F4 2C EA 1B B5 DC " +
            "6E 21 CB 6A 40 D0 BC BC E2 9D 22 33 13 F2 45 F7 " +
            "BD DB 90 84 68 2B 74 8F 52 9E A0 64 27 9F CA 72 " +
            "57 82 4C D2 5C 9E EA 23 F4 A0 1E FA E0 75 48 B0 " +
            "B6 41 7F 31 38 F9 36 F7 BB FF 5D C3 71 5F 71 65 " +
            "1C 2D 4E 56 B7 B6 C8 2C 6A 3B A2 FB 80 10 91 83 " +
            "9A 3B B9 85 ED 5F 0E 32 7A 48 BD 5F 4D BF 49 6C " +
            "ED 20 1F A3 17 F0 EB C4 F2 75 F3 76 30 AF 74 E3 " +
            "71 3A 86 15 2A B3 51 4B A8 A0 BD 6D 25 9D F9 FC " +
            "72 72 AC FE AA 65 B0 8F E5 17 92 E3 7E 0E 01 8B " +
            "73 DC 4A 03 C5 B7 8A F8 67 A2 34 2C 58 A7 12 AD " +
            "40 52 9A 5B 8E 0F 2A 94",
            // After Iota 
            "11 DF 0E 8F 8A 87 59 0D 76 87 44 98 AE 07 DD AB " +
            "E1 AC 08 DB E9 D1 22 CB 34 AF F4 2C EA 1B B5 DC " +
            "6E 21 CB 6A 40 D0 BC BC E2 9D 22 33 13 F2 45 F7 " +
            "BD DB 90 84 68 2B 74 8F 52 9E A0 64 27 9F CA 72 " +
            "57 82 4C D2 5C 9E EA 23 F4 A0 1E FA E0 75 48 B0 " +
            "B6 41 7F 31 38 F9 36 F7 BB FF 5D C3 71 5F 71 65 " +
            "1C 2D 4E 56 B7 B6 C8 2C 6A 3B A2 FB 80 10 91 83 " +
            "9A 3B B9 85 ED 5F 0E 32 7A 48 BD 5F 4D BF 49 6C " +
            "ED 20 1F A3 17 F0 EB C4 F2 75 F3 76 30 AF 74 E3 " +
            "71 3A 86 15 2A B3 51 4B A8 A0 BD 6D 25 9D F9 FC " +
            "72 72 AC FE AA 65 B0 8F E5 17 92 E3 7E 0E 01 8B " +
            "73 DC 4A 03 C5 B7 8A F8 67 A2 34 2C 58 A7 12 AD " +
            "40 52 9A 5B 8E 0F 2A 94",
            // Round #5
            // After Theta
            "09 BF 4C 92 D1 F4 15 47 66 D2 B9 8C F1 90 B3 58 " +
            "A6 24 5D 3D BF 5E 0B F0 CA 88 3D F6 AA 2A 39 FE " +
            "EA DD E7 0E 88 FD 97 FB FA FD 60 2E 48 81 09 BD " +
            "AD 8E 6D 90 37 BC 1A 7C 15 16 F5 82 71 10 E3 49 " +
            "A9 A5 85 08 1C AF 66 01 70 5C 32 9E 28 58 63 F7 " +
            "AE 21 3D 2C 63 8A 7A BD AB AA A0 D7 2E C8 1F 96 " +
            "5B A5 1B B0 E1 39 E1 17 94 1C 6B 21 C0 21 1D A1 " +
            "1E C7 95 E1 25 72 25 75 62 28 FF 42 16 CC 05 26 " +
            "FD 75 E2 B7 48 67 85 37 B5 FD A6 90 66 20 5D D8 " +
            "8F 1D 4F CF 6A 82 DD 69 2C 5C 91 09 ED B0 D2 BB " +
            "6A 12 EE E3 F1 16 FC C5 F5 42 6F F7 21 99 6F 78 " +
            "34 54 1F E5 93 38 A3 C3 99 85 FD F6 18 96 9E 8F " +
            "C4 AE B6 3F 46 22 01 D3",
            // After Rho
            "09 BF 4C 92 D1 F4 15 47 CC A4 73 19 E3 21 67 B1 " +
            "29 49 57 CF AF D7 02 BC AA 92 E3 AF 8C D8 63 AF " +
            "EC BF DC 57 EF 3E 77 40 82 14 98 D0 AB DF 0F E6 " +
            "06 79 C3 AB C1 D7 EA D8 52 85 45 BD 60 1C C4 78 " +
            "D2 42 04 8E 57 B3 80 D4 35 76 0F C7 25 E3 89 82 " +
            "75 0D E9 61 19 53 D4 EB 58 AE AA 82 5E BB 20 7F " +
            "80 0D CF 09 BF D8 2A DD 43 3A 42 29 39 D6 42 80 " +
            "F0 12 B9 92 3A 8F E3 CA 85 2C 98 0B 4C C4 50 FE " +
            "FC 16 E9 AC F0 A6 BF 4E 2E EC DA 7E 53 48 33 90 " +
            "B0 3B ED B1 E3 E9 59 4D BB 2C 5C 91 09 ED B0 D2 " +
            "F0 17 AB 49 B8 8F C7 5B D5 0B BD DD 87 64 BE E1 " +
            "86 EA A3 7C 12 67 74 98 85 FD F6 18 96 9E 8F 99 " +
            "C0 34 B1 AB ED 8F 91 48",
            // After Pi
            "09 BF 4C 92 D1 F4 15 47 06 79 C3 AB C1 D7 EA D8 " +
            "80 0D CF 09 BF D8 2A DD B0 3B ED B1 E3 E9 59 4D " +
            "C0 34 B1 AB ED 8F 91 48 AA 92 E3 AF 8C D8 63 AF " +
            "35 76 0F C7 25 E3 89 82 75 0D E9 61 19 53 D4 EB " +
            "FC 16 E9 AC F0 A6 BF 4E 86 EA A3 7C 12 67 74 98 " +
            "CC A4 73 19 E3 21 67 B1 52 85 45 BD 60 1C C4 78 " +
            "43 3A 42 29 39 D6 42 80 BB 2C 5C 91 09 ED B0 D2 " +
            "F0 17 AB 49 B8 8F C7 5B EC BF DC 57 EF 3E 77 40 " +
            "82 14 98 D0 AB DF 0F E6 58 AE AA 82 5E BB 20 7F " +
            "2E EC DA 7E 53 48 33 90 85 FD F6 18 96 9E 8F 99 " +
            "29 49 57 CF AF D7 02 BC D2 42 04 8E 57 B3 80 D4 " +
            "F0 12 B9 92 3A 8F E3 CA 85 2C 98 0B 4C C4 50 FE " +
            "D5 0B BD DD 87 64 BE E1",
            // After Chi
            "89 BB 40 92 EF FC 15 42 36 4B E3 1B 81 F6 BB D8 " +
            "C0 09 DF 03 B3 DE AA DD B9 B0 A1 A1 F3 99 5D 4A " +
            "C6 74 32 82 ED 8C 7B D0 EA 9B 03 8F 94 C8 37 C6 " +
            "BD 64 0F 4B C5 47 A2 86 77 E5 EB 31 1B 12 94 7B " +
            "D4 06 A9 2F 7C 3E BC 69 93 8E AF 3C 33 44 FC 98 " +
            "CD 9E 71 19 FA E3 65 31 EA 81 59 2D 60 35 74 2A " +
            "03 29 E1 61 89 D4 05 89 B7 8C 0C 81 4A CD 90 72 " +
            "E2 16 AF ED B8 93 47 13 B4 15 FE 55 BB 1E 57 59 " +
            "A4 54 C8 AC AA 9F 1C 66 D9 BF 8E 82 DA 2D AC 76 " +
            "46 EE D2 39 3A 68 43 D0 87 FD F6 98 96 5F 87 3F " +
            "09 59 EE DF 87 DB 61 B6 D7 6E 04 87 13 F3 90 E0 " +
            "A0 11 9C 46 B9 AF 4D CB AD 6C DA 09 64 57 50 E2 " +
            "07 09 BD DD D7 44 3E A1",
            // After Iota 
            "88 BB 40 12 EF FC 15 42 36 4B E3 1B 81 F6 BB D8 " +
            "C0 09 DF 03 B3 DE AA DD B9 B0 A1 A1 F3 99 5D 4A " +
            "C6 74 32 82 ED 8C 7B D0 EA 9B 03 8F 94 C8 37 C6 " +
            "BD 64 0F 4B C5 47 A2 86 77 E5 EB 31 1B 12 94 7B " +
            "D4 06 A9 2F 7C 3E BC 69 93 8E AF 3C 33 44 FC 98 " +
            "CD 9E 71 19 FA E3 65 31 EA 81 59 2D 60 35 74 2A " +
            "03 29 E1 61 89 D4 05 89 B7 8C 0C 81 4A CD 90 72 " +
            "E2 16 AF ED B8 93 47 13 B4 15 FE 55 BB 1E 57 59 " +
            "A4 54 C8 AC AA 9F 1C 66 D9 BF 8E 82 DA 2D AC 76 " +
            "46 EE D2 39 3A 68 43 D0 87 FD F6 98 96 5F 87 3F " +
            "09 59 EE DF 87 DB 61 B6 D7 6E 04 87 13 F3 90 E0 " +
            "A0 11 9C 46 B9 AF 4D CB AD 6C DA 09 64 57 50 E2 " +
            "07 09 BD DD D7 44 3E A1",
            // Round #6
            // After Theta
            "9A 8B CA A8 F2 6D AF 62 BF 6E 4F 3A B9 D0 7F A7 " +
            "B0 ED BF 2B 18 9D 8F E9 1B EB 94 1A FF 83 75 52 " +
            "D3 28 7B A1 0C FC FB 07 F8 AB 89 35 89 59 8D E6 " +
            "34 41 A3 6A FD 61 66 F9 07 01 8B 19 B0 51 B1 4F " +
            "76 5D 9C 94 70 24 94 71 86 D2 E6 1F D2 34 7C 4F " +
            "DF AE FB A3 E7 72 DF 11 63 A4 F5 0C 58 13 B0 55 " +
            "73 CD 81 49 22 97 20 BD 15 D7 39 3A 46 D7 B8 6A " +
            "F7 4A E6 CE 59 E3 C7 C4 A6 25 74 EF A6 8F ED 79 " +
            "2D 71 64 8D 92 B9 D8 19 A9 5B EE AA 71 6E 89 42 " +
            "E4 B5 E7 82 36 72 6B C8 92 A1 BF BB 77 2F 07 E8 " +
            "1B 69 64 65 9A 4A DB 96 5E 4B A8 A6 2B D5 54 9F " +
            "D0 F5 FC 6E 12 EC 68 FF 0F 37 EF B2 68 4D 78 FA " +
            "12 55 F4 FE 36 34 BE 76",
            // After Rho
            "9A 8B CA A8 F2 6D AF 62 7F DD 9E 74 72 A1 FF 4E " +
            "6C FB EF 0A 46 E7 63 3A 3F 58 27 B5 B1 4E A9 F1 " +
            "E0 DF 3F 98 46 D9 0B 65 93 98 D5 68 8E BF 9A 58 " +
            "AA D6 1F 66 96 4F 13 34 D3 41 C0 62 06 6C 54 EC " +
            "2E 4E 4A 38 12 CA 38 BB C3 F7 64 28 6D FE 21 4D " +
            "F8 76 DD 1F 3D 97 FB 8E 56 8D 91 D6 33 60 4D C0 " +
            "4C 12 B9 04 E9 9D 6B 0E AE 71 D5 2A AE 73 74 8C " +
            "E7 AC F1 63 E2 7B 25 73 DE 4D 1F DB F3 4C 4B E8 " +
            "AC 51 32 17 3B A3 25 8E 44 A1 D4 2D 77 D5 38 B7 " +
            "6E 0D 99 BC F6 5C D0 46 E8 92 A1 BF BB 77 2F 07 " +
            "6D 5B 6E A4 91 95 69 2A 7A 2D A1 9A AE 54 53 7D " +
            "BA 9E DF 4D 82 1D ED 1F 37 EF B2 68 4D 78 FA 0F " +
            "AF 9D 44 15 BD BF 0D 8D",
            // After Pi
            "9A 8B CA A8 F2 6D AF 62 AA D6 1F 66 96 4F 13 34 " +
            "4C 12 B9 04 E9 9D 6B 0E 6E 0D 99 BC F6 5C D0 46 " +
            "AF 9D 44 15 BD BF 0D 8D 3F 58 27 B5 B1 4E A9 F1 " +
            "C3 F7 64 28 6D FE 21 4D F8 76 DD 1F 3D 97 FB 8E " +
            "AC 51 32 17 3B A3 25 8E BA 9E DF 4D 82 1D ED 1F " +
            "7F DD 9E 74 72 A1 FF 4E D3 41 C0 62 06 6C 54 EC " +
            "AE 71 D5 2A AE 73 74 8C E8 92 A1 BF BB 77 2F 07 " +
            "6D 5B 6E A4 91 95 69 2A E0 DF 3F 98 46 D9 0B 65 " +
            "93 98 D5 68 8E BF 9A 58 56 8D 91 D6 33 60 4D C0 " +
            "44 A1 D4 2D 77 D5 38 B7 37 EF B2 68 4D 78 FA 0F " +
            "6C FB EF 0A 46 E7 63 3A 2E 4E 4A 38 12 CA 38 BB " +
            "E7 AC F1 63 E2 7B 25 73 DE 4D 1F DB F3 4C 4B E8 " +
            "7A 2D A1 9A AE 54 53 7D",
            // After Chi
            "DE 8B 6A A8 9B FD C7 68 88 DB 1F DE 80 0F 83 74 " +
            "CD 82 FD 05 E0 3E 66 87 7E 0F 13 14 B4 1C 72 24 " +
            "8F C9 51 53 B9 BD 1D 99 07 58 BE A2 A1 4F 73 73 " +
            "C7 F6 46 28 6F DE 25 4D EA F8 10 57 BD 8B 33 9F " +
            "A9 11 12 A7 0A E1 25 6E 7A 39 9F 45 CE AD ED 13 " +
            "53 ED 8B 7C DA B2 DF 4E 93 C3 E0 F7 17 68 5F EF " +
            "AB 38 9B 2A AE F3 34 A4 FA 16 31 EF D9 57 B9 43 " +
            "ED 5B 2E A6 95 D9 69 8A A4 DA 3F 0E 77 99 4E E5 " +
            "93 B8 91 41 CA 2A AA 6F 65 C3 B3 96 3B 48 8F C8 " +
            "84 B1 D9 BD 75 54 39 D7 24 EF 72 08 C5 5E 6A 17 " +
            "AD 5B 5E 49 A6 D6 66 7A 36 0F 44 A0 03 CE 72 33 " +
            "C7 8C 51 63 EE 6B 35 66 DA 9F 51 DB B3 EF 6B EA " +
            "78 29 A1 AA BE 5C 4B FC",
            // After Iota 
            "5F 0B 6A 28 9B FD C7 E8 88 DB 1F DE 80 0F 83 74 " +
            "CD 82 FD 05 E0 3E 66 87 7E 0F 13 14 B4 1C 72 24 " +
            "8F C9 51 53 B9 BD 1D 99 07 58 BE A2 A1 4F 73 73 " +
            "C7 F6 46 28 6F DE 25 4D EA F8 10 57 BD 8B 33 9F " +
            "A9 11 12 A7 0A E1 25 6E 7A 39 9F 45 CE AD ED 13 " +
            "53 ED 8B 7C DA B2 DF 4E 93 C3 E0 F7 17 68 5F EF " +
            "AB 38 9B 2A AE F3 34 A4 FA 16 31 EF D9 57 B9 43 " +
            "ED 5B 2E A6 95 D9 69 8A A4 DA 3F 0E 77 99 4E E5 " +
            "93 B8 91 41 CA 2A AA 6F 65 C3 B3 96 3B 48 8F C8 " +
            "84 B1 D9 BD 75 54 39 D7 24 EF 72 08 C5 5E 6A 17 " +
            "AD 5B 5E 49 A6 D6 66 7A 36 0F 44 A0 03 CE 72 33 " +
            "C7 8C 51 63 EE 6B 35 66 DA 9F 51 DB B3 EF 6B EA " +
            "78 29 A1 AA BE 5C 4B FC",
            // Round #7
            // After Theta
            "E8 D4 81 FA 61 8C 3D 17 D6 FE 09 74 FC 8A 76 1B " +
            "52 97 E1 90 93 40 3F 64 D9 D8 E1 BD A0 EE D8 E1 " +
            "F8 91 95 0B 7B 32 27 39 B0 87 55 70 5B 3E 89 8C " +
            "99 D3 50 82 13 5B D0 22 75 ED 0C C2 CE F5 6A 7C " +
            "0E C6 E0 0E 1E 13 8F AB 0D 61 5B 1D 0C 22 D7 B3 " +
            "E4 32 60 AE 20 C3 25 B1 CD E6 F6 5D 6B ED AA 80 " +
            "34 2D 87 BF DD 8D 6D 47 5D C1 C3 46 CD A5 13 86 " +
            "9A 03 EA FE 57 56 53 2A 13 05 D4 DC 8D E8 B4 1A " +
            "CD 9D 87 EB B6 AF 5F 00 FA D6 AF 03 48 36 D6 2B " +
            "23 66 2B 14 61 A6 93 12 53 B7 B6 50 07 D1 50 B7 " +
            "1A 84 B5 9B 5C A7 9C 85 68 2A 52 0A 7F 4B 87 5C " +
            "58 99 4D F6 9D 15 6C 85 7D 48 A3 72 A7 1D C1 2F " +
            "0F 71 65 F2 7C D3 71 5C",
            // After Rho
            "E8 D4 81 FA 61 8C 3D 17 AC FD 13 E8 F8 15 ED 36 " +
            "D4 65 38 E4 24 D0 0F 99 EA 8E 1D 9E 8D 1D DE 0B " +
            "93 39 C9 C1 8F AC 5C D8 B7 E5 93 C8 08 7B 58 05 " +
            "25 38 B1 05 2D 92 39 0D 5F 5D 3B 83 B0 73 BD 1A " +
            "63 70 07 8F 89 C7 55 07 72 3D DB 10 B6 D5 C1 20 " +
            "25 97 01 73 05 19 2E 89 02 36 9B DB 77 AD B5 AB " +
            "FC ED 6E 6C 3B A2 69 39 4B 27 0C BB 82 87 8D 9A " +
            "FF 2B AB 29 15 CD 01 75 B9 1B D1 69 35 26 0A A8 " +
            "70 DD F6 F5 0B A0 B9 F3 EB 15 7D EB D7 01 24 1B " +
            "74 52 62 C4 6C 85 22 CC B7 53 B7 B6 50 07 D1 50 " +
            "72 16 6A 10 D6 6E 72 9D A1 A9 48 29 FC 2D 1D 72 " +
            "2B B3 C9 BE B3 82 AD 10 48 A3 72 A7 1D C1 2F 7D " +
            "1C D7 43 5C 99 3C DF 74",
            // After Pi
            "E8 D4 81 FA 61 8C 3D 17 25 38 B1 05 2D 92 39 0D " +
            "FC ED 6E 6C 3B A2 69 39 74 52 62 C4 6C 85 22 CC " +
            "1C D7 43 5C 99 3C DF 74 EA 8E 1D 9E 8D 1D DE 0B " +
            "72 3D DB 10 B6 D5 C1 20 25 97 01 73 05 19 2E 89 " +
            "70 DD F6 F5 0B A0 B9 F3 2B B3 C9 BE B3 82 AD 10 " +
            "AC FD 13 E8 F8 15 ED 36 5F 5D 3B 83 B0 73 BD 1A " +
            "4B 27 0C BB 82 87 8D 9A B7 53 B7 B6 50 07 D1 50 " +
            "72 16 6A 10 D6 6E 72 9D 93 39 C9 C1 8F AC 5C D8 " +
            "B7 E5 93 C8 08 7B 58 05 02 36 9B DB 77 AD B5 AB " +
            "EB 15 7D EB D7 01 24 1B 48 A3 72 A7 1D C1 2F 7D " +
            "D4 65 38 E4 24 D0 0F 99 63 70 07 8F 89 C7 55 07 " +
            "FF 2B AB 29 15 CD 01 75 B9 1B D1 69 35 26 0A A8 " +
            "A1 A9 48 29 FC 2D 1D 72",
            // After Chi
            "30 11 CF 92 73 AC 7D 27 25 2A B1 85 69 97 3B C9 " +
            "F4 68 6F 74 AA 9A B4 09 94 52 E2 66 0C 05 02 CF " +
            "19 FF 73 59 95 2E DF 7C EF 0C 1D FD 8C 15 F0 82 " +
            "22 75 2D 94 BC 75 50 52 2E B5 08 79 B5 1B 2A 89 " +
            "B0 D1 E2 F5 07 BD EB F8 3B 82 0B BE 81 42 AC 30 " +
            "AC DF 17 D0 FA 91 ED B6 EB 0D 88 87 E0 73 ED 5A " +
            "0B 23 44 BB 04 EF AF 17 3B BA A6 5E 78 16 5C 72 " +
            "21 16 42 13 D6 0C 62 95 93 2B C1 D2 F8 28 F9 72 " +
            "5E E4 F7 E8 88 7B 58 15 02 94 99 DF 7F 6D BE CF " +
            "78 0D F4 AB 55 2D 74 9B 6C 67 60 AF 1D 92 2F 78 " +
            "48 6E 90 C4 30 D8 0F E9 63 60 57 CF A9 E5 5F 8F " +
            "FF 8B A3 29 DD C4 14 27 ED 5F E1 AD 35 F6 08 21 " +
            "82 B9 4F 22 75 2A 4D 74",
            // After Iota 
            "39 91 CF 92 73 AC 7D A7 25 2A B1 85 69 97 3B C9 " +
            "F4 68 6F 74 AA 9A B4 09 94 52 E2 66 0C 05 02 CF " +
            "19 FF 73 59 95 2E DF 7C EF 0C 1D FD 8C 15 F0 82 " +
            "22 75 2D 94 BC 75 50 52 2E B5 08 79 B5 1B 2A 89 " +
            "B0 D1 E2 F5 07 BD EB F8 3B 82 0B BE 81 42 AC 30 " +
            "AC DF 17 D0 FA 91 ED B6 EB 0D 88 87 E0 73 ED 5A " +
            "0B 23 44 BB 04 EF AF 17 3B BA A6 5E 78 16 5C 72 " +
            "21 16 42 13 D6 0C 62 95 93 2B C1 D2 F8 28 F9 72 " +
            "5E E4 F7 E8 88 7B 58 15 02 94 99 DF 7F 6D BE CF " +
            "78 0D F4 AB 55 2D 74 9B 6C 67 60 AF 1D 92 2F 78 " +
            "48 6E 90 C4 30 D8 0F E9 63 60 57 CF A9 E5 5F 8F " +
            "FF 8B A3 29 DD C4 14 27 ED 5F E1 AD 35 F6 08 21 " +
            "82 B9 4F 22 75 2A 4D 74",
            // Round #8
            // After Theta
            "76 89 B3 88 F0 6A 0C C5 DC EF 16 AC D6 C0 9A 3E " +
            "30 69 BD 52 99 7F A7 AD 63 D8 D0 D4 E1 73 7E 1A " +
            "D1 9B E8 C1 1D EA 3B 92 A0 14 61 E7 0F D3 81 E0 " +
            "DB B0 8A BD 03 22 F1 A5 EA B4 DA 5F 86 FE 39 2D " +
            "47 5B D0 47 EA CB 97 2D F3 E6 90 26 09 86 48 DE " +
            "E3 C7 6B CA 79 57 9C D4 12 C8 2F AE 5F 24 4C AD " +
            "CF 22 96 9D 37 0A BC B3 CC 30 94 EC 95 60 20 A7 " +
            "E9 72 D9 8B 5E C8 86 7B DC 33 BD C8 7B EE 88 10 " +
            "A7 21 50 C1 37 2C F9 E2 C6 95 4B F9 4C 88 AD 6B " +
            "8F 87 C6 19 B8 5B 08 4E A4 03 FB 37 95 56 CB 96 " +
            "07 76 EC DE B3 1E 7E 8B 9A A5 F0 E6 16 B2 FE 78 " +
            "3B 8A 71 0F EE 21 07 83 1A D5 D3 1F D8 80 74 F4 " +
            "4A DD D4 BA FD EE A9 9A",
            // After Rho
            "76 89 B3 88 F0 6A 0C C5 B8 DF 2D 58 AD 81 35 7D " +
            "4C 5A AF 54 E6 DF 69 2B 3E E7 A7 31 86 0D 4D 1D " +
            "50 DF 91 8C DE 44 0F EE FE 30 1D 08 0E 4A 11 76 " +
            "D8 3B 20 12 5F BA 0D AB 8B 3A AD F6 97 A1 7F 4E " +
            "2D E8 23 F5 E5 CB 96 A3 88 E4 3D 6F 0E 69 92 60 " +
            "1E 3F 5E 53 CE BB E2 A4 B5 4A 20 BF B8 7E 91 30 " +
            "EC BC 51 E0 9D 7D 16 B1 C1 40 4E 99 61 28 D9 2B " +
            "45 2F 64 C3 BD 74 B9 EC 91 F7 DC 11 21 B8 67 7A " +
            "2A F8 86 25 5F FC 34 04 D6 35 E3 CA A5 7C 26 C4 " +
            "0B C1 E9 F1 D0 38 03 77 96 A4 03 FB 37 95 56 CB " +
            "F8 2D 1E D8 B1 7B CF 7A 69 96 C2 9B 5B C8 FA E3 " +
            "47 31 EE C1 3D E4 60 70 D5 D3 1F D8 80 74 F4 1A " +
            "AA A6 52 37 B5 6E BF 7B",
            // After Pi
            "76 89 B3 88 F0 6A 0C C5 D8 3B 20 12 5F BA 0D AB " +
            "EC BC 51 E0 9D 7D 16 B1 0B C1 E9 F1 D0 38 03 77 " +
            "AA A6 52 37 B5 6E BF 7B 3E E7 A7 31 86 0D 4D 1D " +
            "88 E4 3D 6F 0E 69 92 60 1E 3F 5E 53 CE BB E2 A4 " +
            "2A F8 86 25 5F FC 34 04 47 31 EE C1 3D E4 60 70 " +
            "B8 DF 2D 58 AD 81 35 7D 8B 3A AD F6 97 A1 7F 4E " +
            "C1 40 4E 99 61 28 D9 2B 96 A4 03 FB 37 95 56 CB " +
            "F8 2D 1E D8 B1 7B CF 7A 50 DF 91 8C DE 44 0F EE " +
            "FE 30 1D 08 0E 4A 11 76 B5 4A 20 BF B8 7E 91 30 " +
            "D6 35 E3 CA A5 7C 26 C4 D5 D3 1F D8 80 74 F4 1A " +
            "4C 5A AF 54 E6 DF 69 2B 2D E8 23 F5 E5 CB 96 A3 " +
            "45 2F 64 C3 BD 74 B9 EC 91 F7 DC 11 21 B8 67 7A " +
            "69 96 C2 9B 5B C8 FA E3",
            // After Chi
            "52 0D E2 68 70 2F 1E D5 DB 7A 88 03 1F BA 0C ED " +
            "4C 9A 43 E6 B8 3B AA B9 5F C8 48 79 90 38 03 F3 " +
            "22 94 52 25 BA FE BE 51 28 FC E5 21 46 9F 2D 99 " +
            "A8 24 BD 4B 1F 2D 86 60 5B 3E 36 93 EE BB A2 D4 " +
            "12 3E 87 15 DD F5 39 09 C7 31 F6 8F 35 84 F2 10 " +
            "F8 9F 6F 51 CD 89 B5 5C 9D 9E AC 94 81 34 79 8E " +
            "A9 49 52 99 E1 42 50 1B 96 76 22 FB 3B 15 66 CE " +
            "FB 0D 9E 7E A3 5B 85 78 51 95 B1 3B 6E 70 8F EE " +
            "BC 05 DE 48 0B 4A 37 B2 B4 88 3C AF B8 7E 41 2A " +
            "D6 39 63 CE FB 7C 2D 20 7B F3 13 D8 80 7E E4 0A " +
            "0C 5D EB 56 FE EB 40 67 BD 38 BB E5 E5 43 D0 B1 " +
            "2D 2F 66 49 E7 34 21 6D 95 BF F1 55 85 AF 66 72 " +
            "48 36 C2 3A 5A C8 6C 63",
            // After Iota 
            "D8 0D E2 68 70 2F 1E D5 DB 7A 88 03 1F BA 0C ED " +
            "4C 9A 43 E6 B8 3B AA B9 5F C8 48 79 90 38 03 F3 " +
            "22 94 52 25 BA FE BE 51 28 FC E5 21 46 9F 2D 99 " +
            "A8 24 BD 4B 1F 2D 86 60 5B 3E 36 93 EE BB A2 D4 " +
            "12 3E 87 15 DD F5 39 09 C7 31 F6 8F 35 84 F2 10 " +
            "F8 9F 6F 51 CD 89 B5 5C 9D 9E AC 94 81 34 79 8E " +
            "A9 49 52 99 E1 42 50 1B 96 76 22 FB 3B 15 66 CE " +
            "FB 0D 9E 7E A3 5B 85 78 51 95 B1 3B 6E 70 8F EE " +
            "BC 05 DE 48 0B 4A 37 B2 B4 88 3C AF B8 7E 41 2A " +
            "D6 39 63 CE FB 7C 2D 20 7B F3 13 D8 80 7E E4 0A " +
            "0C 5D EB 56 FE EB 40 67 BD 38 BB E5 E5 43 D0 B1 " +
            "2D 2F 66 49 E7 34 21 6D 95 BF F1 55 85 AF 66 72 " +
            "48 36 C2 3A 5A C8 6C 63",
            // Round #9
            // After Theta
            "2B 9B F0 BD 58 EC 76 85 C0 48 40 62 A4 09 34 16 " +
            "93 6A 41 8F C7 87 90 75 22 58 E3 1E 94 9F B8 62 " +
            "11 DE 48 C3 64 B1 3A 05 DB 6A F7 F4 6E 5C 45 C9 " +
            "B3 16 75 2A A4 9E BE 9B 84 CE 34 FA 91 07 98 18 " +
            "6F AE 2C 72 D9 52 82 98 F4 7B EC 69 EB CB 76 44 " +
            "0B 09 7D 84 E5 4A DD 0C 86 AC 64 F5 3A 87 41 75 " +
            "76 B9 50 F0 9E FE 6A D7 EB E6 89 9C 3F B2 DD 5F " +
            "C8 47 84 98 7D 14 01 2C A2 03 A3 EE 46 B3 E7 BE " +
            "A7 37 16 29 B0 F9 0F 49 6B 78 3E C6 C7 C2 7B E6 " +
            "AB A9 C8 A9 FF DB 96 B1 48 B9 09 3E 5E 31 60 5E " +
            "FF CB F9 83 D6 28 28 37 A6 0A 73 84 5E F0 E8 4A " +
            "F2 DF 64 20 98 88 1B A1 E8 2F 5A 32 81 08 DD E3 " +
            "7B 7C D8 DC 84 87 E8 37",
            // After Rho
            "2B 9B F0 BD 58 EC 76 85 80 91 80 C4 48 13 68 2C " +
            "A4 5A D0 E3 F1 21 64 DD F9 89 2B 26 82 35 EE 41 " +
            "8B D5 29 88 F0 46 1A 26 EF C6 55 94 BC AD 76 4F " +
            "A7 42 EA E9 BB 39 6B 51 06 A1 33 8D 7E E4 01 26 " +
            "57 16 B9 6C 29 41 CC 37 6C 47 44 BF C7 9E B6 BE " +
            "58 48 E8 23 2C 57 EA 66 D5 19 B2 92 D5 EB 1C 06 " +
            "82 F7 F4 57 BB B6 CB 85 64 BB BF D6 CD 13 39 7F " +
            "CC 3E 8A 00 16 E4 23 42 DD 8D 66 CF 7D 45 07 46 " +
            "22 05 36 FF 21 E9 F4 C6 3D F3 35 3C 1F E3 63 E1 " +
            "DB 32 76 35 15 39 F5 7F 5E 48 B9 09 3E 5E 31 60 " +
            "A0 DC FC 2F E7 0F 5A A3 99 2A CC 11 7A C1 A3 2B " +
            "FE 9B 0C 04 13 71 23 54 2F 5A 32 81 08 DD E3 E8 " +
            "FA CD 1E 1F 36 37 E1 21",
            // After Pi
            "2B 9B F0 BD 58 EC 76 85 A7 42 EA E9 BB 39 6B 51 " +
            "82 F7 F4 57 BB B6 CB 85 DB 32 76 35 15 39 F5 7F " +
            "FA CD 1E 1F 36 37 E1 21 F9 89 2B 26 82 35 EE 41 " +
            "6C 47 44 BF C7 9E B6 BE 58 48 E8 23 2C 57 EA 66 " +
            "22 05 36 FF 21 E9 F4 C6 FE 9B 0C 04 13 71 23 54 " +
            "80 91 80 C4 48 13 68 2C 06 A1 33 8D 7E E4 01 26 " +
            "64 BB BF D6 CD 13 39 7F 5E 48 B9 09 3E 5E 31 60 " +
            "A0 DC FC 2F E7 0F 5A A3 8B D5 29 88 F0 46 1A 26 " +
            "EF C6 55 94 BC AD 76 4F D5 19 B2 92 D5 EB 1C 06 " +
            "3D F3 35 3C 1F E3 63 E1 2F 5A 32 81 08 DD E3 E8 " +
            "A4 5A D0 E3 F1 21 64 DD 57 16 B9 6C 29 41 CC 37 " +
            "CC 3E 8A 00 16 E4 23 42 DD 8D 66 CF 7D 45 07 46 " +
            "99 2A CC 11 7A C1 A3 2B",
            // After Chi
            "2B 2E E4 AB 58 6A F6 01 FE 42 E8 C9 BF 30 5F 2B " +
            "A2 3A FC 5D 99 B0 CB 85 DA 20 96 95 5D F1 E3 FB " +
            "7E 8D 14 5F 95 26 E8 71 E9 81 83 26 AA 74 A6 01 " +
            "4E 42 52 63 C6 36 A2 3E 84 D2 E0 23 3E 47 E9 76 " +
            "23 05 15 DD A1 ED 38 C7 FA DD 48 9D 56 FB 33 EA " +
            "E0 8B 0C 96 C9 00 50 75 1C E1 33 84 4C A8 01 26 " +
            "C4 2F FB F0 0C 12 73 FC 5E 49 B9 C9 36 4E 11 6C " +
            "A6 FC CF 26 D1 EB 5B A1 9B CC 8B 8A B1 04 12 26 " +
            "C7 24 50 B8 B6 AD 15 AE D7 11 B0 13 D5 F7 9C 0E " +
            "BD 76 3C 34 EF E1 7B E7 4B 58 66 95 04 74 87 A1 " +
            "2C 72 D2 E3 E7 85 47 9D 46 97 DD A3 40 40 C8 33 " +
            "CC 1C 02 10 14 64 83 6B F9 DD 76 2D FC 65 43 92 " +
            "CA 2E E5 1D 72 81 2B 09",
            // After Iota 
            "A3 2E E4 AB 58 6A F6 01 FE 42 E8 C9 BF 30 5F 2B " +
            "A2 3A FC 5D 99 B0 CB 85 DA 20 96 95 5D F1 E3 FB " +
            "7E 8D 14 5F 95 26 E8 71 E9 81 83 26 AA 74 A6 01 " +
            "4E 42 52 63 C6 36 A2 3E 84 D2 E0 23 3E 47 E9 76 " +
            "23 05 15 DD A1 ED 38 C7 FA DD 48 9D 56 FB 33 EA " +
            "E0 8B 0C 96 C9 00 50 75 1C E1 33 84 4C A8 01 26 " +
            "C4 2F FB F0 0C 12 73 FC 5E 49 B9 C9 36 4E 11 6C " +
            "A6 FC CF 26 D1 EB 5B A1 9B CC 8B 8A B1 04 12 26 " +
            "C7 24 50 B8 B6 AD 15 AE D7 11 B0 13 D5 F7 9C 0E " +
            "BD 76 3C 34 EF E1 7B E7 4B 58 66 95 04 74 87 A1 " +
            "2C 72 D2 E3 E7 85 47 9D 46 97 DD A3 40 40 C8 33 " +
            "CC 1C 02 10 14 64 83 6B F9 DD 76 2D FC 65 43 92 " +
            "CA 2E E5 1D 72 81 2B 09",
            // Round #10
            // After Theta
            "5B 50 FC AD BA 2E 98 CF 11 4D 71 A1 07 43 96 31 " +
            "49 E7 19 58 E9 5E 0F 60 64 5F E2 C0 FF 01 F4 B5 " +
            "A6 7E 01 23 96 CE B1 C8 11 FF 9B 20 48 30 C8 CF " +
            "A1 4D CB 0B 7E 45 6B 24 6F 0F 05 26 4E A9 2D 93 " +
            "9D 7A 61 88 03 1D 2F 89 22 2E 5D E1 55 13 6A 53 " +
            "18 F5 14 90 2B 44 3E BB F3 EE AA EC F4 DB C8 3C " +
            "2F F2 1E F5 7C FC B7 19 E0 36 CD 9C 94 BE 06 22 " +
            "7E 0F DA 5A D2 03 02 18 63 B2 93 8C 53 40 7C E8 " +
            "28 2B C9 D0 0E DE DC B4 3C CC 55 16 A5 19 58 EB " +
            "03 09 48 61 4D 11 6C A9 93 AB 73 E9 07 9C DE 18 " +
            "D4 0C CA E5 05 C1 29 53 A9 98 44 CB F8 33 01 29 " +
            "27 C1 E7 15 64 8A 47 8E 47 A2 02 78 5E 95 54 DC " +
            "12 DD F0 61 71 69 72 B0",
            // After Rho
            "5B 50 FC AD BA 2E 98 CF 22 9A E2 42 0F 86 2C 63 " +
            "D2 79 06 56 BA D7 03 58 1F 40 5F 4B F6 25 0E FC " +
            "74 8E 45 36 F5 0B 18 B1 82 04 83 FC 1C F1 BF 09 " +
            "BC E0 57 B4 46 12 DA B4 E4 DB 43 81 89 53 6A CB " +
            "BD 30 C4 81 8E 97 C4 4E A1 36 25 E2 D2 15 5E 35 " +
            "C5 A8 A7 80 5C 21 F2 D9 F3 CC BB AB B2 D3 6F 23 " +
            "A8 E7 E3 BF CD 78 91 F7 7D 0D 44 C0 6D 9A 39 29 " +
            "2D E9 01 01 0C BF 07 6D 19 A7 80 F8 D0 C7 64 27 " +
            "19 DA C1 9B 9B 16 65 25 AC 75 1E E6 2A 8B D2 0C " +
            "82 2D 75 20 01 29 AC 29 18 93 AB 73 E9 07 9C DE " +
            "A7 4C 51 33 28 97 17 04 A4 62 12 2D E3 CF 04 A4 " +
            "24 F8 BC 82 4C F1 C8 F1 A2 02 78 5E 95 54 DC 47 " +
            "1C AC 44 37 7C 58 5C 9A",
            // After Pi
            "5B 50 FC AD BA 2E 98 CF BC E0 57 B4 46 12 DA B4 " +
            "A8 E7 E3 BF CD 78 91 F7 82 2D 75 20 01 29 AC 29 " +
            "1C AC 44 37 7C 58 5C 9A 1F 40 5F 4B F6 25 0E FC " +
            "A1 36 25 E2 D2 15 5E 35 C5 A8 A7 80 5C 21 F2 D9 " +
            "19 DA C1 9B 9B 16 65 25 24 F8 BC 82 4C F1 C8 F1 " +
            "22 9A E2 42 0F 86 2C 63 E4 DB 43 81 89 53 6A CB " +
            "7D 0D 44 C0 6D 9A 39 29 18 93 AB 73 E9 07 9C DE " +
            "A7 4C 51 33 28 97 17 04 74 8E 45 36 F5 0B 18 B1 " +
            "82 04 83 FC 1C F1 BF 09 F3 CC BB AB B2 D3 6F 23 " +
            "AC 75 1E E6 2A 8B D2 0C A2 02 78 5E 95 54 DC 47 " +
            "D2 79 06 56 BA D7 03 58 BD 30 C4 81 8E 97 C4 4E " +
            "2D E9 01 01 0C BF 07 6D 19 A7 80 F8 D0 C7 64 27 " +
            "A4 62 12 2D E3 CF 04 A4",
            // After Chi
            "5B 57 5C A6 33 46 99 8C BE E8 43 B4 46 13 F6 BC " +
            "B4 67 E3 A8 B1 28 C1 65 C1 7D CD A8 83 0F 2C 6C " +
            "B8 0C 47 27 38 48 1E AA 5B C8 DD 4B FA 05 AE 34 " +
            "B9 64 65 F9 51 03 5B 11 E1 88 9B 80 18 C0 7A 09 " +
            "02 DA 82 D2 29 12 63 29 84 CE 9C 22 4C E1 98 F0 " +
            "3B 9E E6 02 6B 0E 3D 43 E4 49 E8 B2 09 56 EE 1D " +
            "DA 41 14 C0 6D 0A 3A 29 18 01 09 33 EE 07 B4 BD " +
            "63 0D 50 B2 A8 C6 55 8C 05 46 7D 35 57 09 58 93 " +
            "8E 35 87 B8 14 F9 2F 05 F1 CE DB B3 27 87 63 60 " +
            "F8 F9 1B C6 4A 80 D2 BC 20 02 FA 96 9D A4 7B 4F " +
            "D2 B0 07 56 BA FF 00 79 AD 36 44 79 5E D7 A4 4C " +
            "89 A9 13 04 2F B7 07 ED 4B BE 84 AA C8 D7 67 7F " +
            "89 62 D2 AC E7 CF C0 A2",
            // After Iota 
            "52 D7 5C 26 33 46 99 8C BE E8 43 B4 46 13 F6 BC " +
            "B4 67 E3 A8 B1 28 C1 65 C1 7D CD A8 83 0F 2C 6C " +
            "B8 0C 47 27 38 48 1E AA 5B C8 DD 4B FA 05 AE 34 " +
            "B9 64 65 F9 51 03 5B 11 E1 88 9B 80 18 C0 7A 09 " +
            "02 DA 82 D2 29 12 63 29 84 CE 9C 22 4C E1 98 F0 " +
            "3B 9E E6 02 6B 0E 3D 43 E4 49 E8 B2 09 56 EE 1D " +
            "DA 41 14 C0 6D 0A 3A 29 18 01 09 33 EE 07 B4 BD " +
            "63 0D 50 B2 A8 C6 55 8C 05 46 7D 35 57 09 58 93 " +
            "8E 35 87 B8 14 F9 2F 05 F1 CE DB B3 27 87 63 60 " +
            "F8 F9 1B C6 4A 80 D2 BC 20 02 FA 96 9D A4 7B 4F " +
            "D2 B0 07 56 BA FF 00 79 AD 36 44 79 5E D7 A4 4C " +
            "89 A9 13 04 2F B7 07 ED 4B BE 84 AA C8 D7 67 7F " +
            "89 62 D2 AC E7 CF C0 A2",
            // Round #11
            // After Theta
            "25 F5 E4 D7 3D 92 61 44 B4 0C 17 07 91 0D 6F 3C " +
            "A4 63 5D DD 69 DB 95 EA DA EB 2E EC 02 D4 19 D2 " +
            "1A 02 A4 1A 60 73 F5 B3 2C EA 65 BA F4 D1 56 FC " +
            "B3 80 31 4A 86 1D C2 91 F1 8C 25 F5 C0 33 2E 86 " +
            "19 4C 61 96 A8 C9 56 97 26 C0 7F 1F 14 DA 73 E9 " +
            "4C BC 5E F3 65 DA C5 8B EE AD BC 01 DE 48 77 9D " +
            "CA 45 AA B5 B5 F9 6E A6 03 97 EA 77 6F DC 81 03 " +
            "C1 03 B3 8F F0 FD BE 95 72 64 C5 C4 59 DD A0 5B " +
            "84 D1 D3 0B C3 E7 B6 85 E1 CA 65 C6 FF 74 37 EF " +
            "E3 6F F8 82 CB 5B E7 02 82 0C 19 AB C5 9F 90 56 " +
            "A5 92 BF A7 B4 2B F8 B1 A7 D2 10 CA 89 C9 3D CC " +
            "99 AD AD 71 F7 44 53 62 50 28 67 EE 49 0C 52 C1 " +
            "2B 6C 31 91 BF F4 2B BB",
            // After Rho
            "25 F5 E4 D7 3D 92 61 44 68 19 2E 0E 22 1B DE 78 " +
            "E9 58 57 77 DA 76 A5 3A 40 9D 21 AD BD EE C2 2E " +
            "9B AB 9F D5 10 20 D5 00 4B 1F 6D C5 CF A2 5E A6 " +
            "A3 64 D8 21 1C 39 0B 18 61 3C 63 49 3D F0 8C 8B " +
            "A6 30 4B D4 64 AB CB 0C 3D 97 6E 02 FC F7 41 A1 " +
            "64 E2 F5 9A 2F D3 2E 5E 75 BA B7 F2 06 78 23 DD " +
            "AD AD CD 77 33 55 2E 52 B8 03 07 06 2E D5 EF DE " +
            "47 F8 7E DF CA E0 81 D9 89 B3 BA 41 B7 E4 C8 8A " +
            "7A 61 F8 DC B6 90 30 7A 9B F7 70 E5 32 E3 7F BA " +
            "EB 5C 60 FC 0D 5F 70 79 56 82 0C 19 AB C5 9F 90 " +
            "E0 C7 96 4A FE 9E D2 AE 9F 4A 43 28 27 26 F7 30 " +
            "B3 B5 35 EE 9E 68 4A 2C 28 67 EE 49 0C 52 C1 50 " +
            "CA EE 0A 5B 4C E4 2F FD",
            // After Pi
            "25 F5 E4 D7 3D 92 61 44 A3 64 D8 21 1C 39 0B 18 " +
            "AD AD CD 77 33 55 2E 52 EB 5C 60 FC 0D 5F 70 79 " +
            "CA EE 0A 5B 4C E4 2F FD 40 9D 21 AD BD EE C2 2E " +
            "3D 97 6E 02 FC F7 41 A1 64 E2 F5 9A 2F D3 2E 5E " +
            "7A 61 F8 DC B6 90 30 7A B3 B5 35 EE 9E 68 4A 2C " +
            "68 19 2E 0E 22 1B DE 78 61 3C 63 49 3D F0 8C 8B " +
            "B8 03 07 06 2E D5 EF DE 56 82 0C 19 AB C5 9F 90 " +
            "E0 C7 96 4A FE 9E D2 AE 9B AB 9F D5 10 20 D5 00 " +
            "4B 1F 6D C5 CF A2 5E A6 75 BA B7 F2 06 78 23 DD " +
            "9B F7 70 E5 32 E3 7F BA 28 67 EE 49 0C 52 C1 50 " +
            "E9 58 57 77 DA 76 A5 3A A6 30 4B D4 64 AB CB 0C " +
            "47 F8 7E DF CA E0 81 D9 89 B3 BA 41 B7 E4 C8 8A " +
            "9F 4A 43 28 27 26 F7 30",
            // After Chi
            "29 7C E1 81 1E D6 45 06 E1 34 F8 A9 10 33 5B 31 " +
            "AD 0F C7 74 73 F5 21 D6 CE 4D 84 78 3C 4D 30 79 " +
            "48 EE 12 7B 4C CD 25 E5 00 FD B0 35 BE EE EC 70 " +
            "27 96 66 46 6C F7 51 81 E5 76 F0 B8 27 BB 64 5A " +
            "3A 69 F8 DD 97 16 B0 78 8E B7 7B EC DE 79 4B AD " +
            "F0 1A 2A 08 20 1E BD 2C 27 BC 6B 50 BC F0 9C 8B " +
            "18 46 95 44 7A CF AF F0 5E 9A 24 1D AB C4 93 C0 " +
            "E1 E3 D7 0B E3 7E D2 2D AF 0B 0D E7 10 78 F4 59 " +
            "C1 5A 2D C0 FF 21 02 84 55 BA 39 FA 0A 68 A3 9D " +
            "08 7F 61 71 22 C3 6B BA 68 73 8E 49 C3 D0 CB F6 " +
            "A8 90 63 7C 50 36 A5 EB 2E 33 CB D4 51 AF 83 0E " +
            "51 B0 3F F7 CA E2 B6 E9 E9 A3 AE 16 6F B4 C8 80 " +
            "99 6A 4B A8 03 AF BD 34",
            // After Iota 
            "23 7C E1 01 1E D6 45 06 E1 34 F8 A9 10 33 5B 31 " +
            "AD 0F C7 74 73 F5 21 D6 CE 4D 84 78 3C 4D 30 79 " +
            "48 EE 12 7B 4C CD 25 E5 00 FD B0 35 BE EE EC 70 " +
            "27 96 66 46 6C F7 51 81 E5 76 F0 B8 27 BB 64 5A " +
            "3A 69 F8 DD 97 16 B0 78 8E B7 7B EC DE 79 4B AD " +
            "F0 1A 2A 08 20 1E BD 2C 27 BC 6B 50 BC F0 9C 8B " +
            "18 46 95 44 7A CF AF F0 5E 9A 24 1D AB C4 93 C0 " +
            "E1 E3 D7 0B E3 7E D2 2D AF 0B 0D E7 10 78 F4 59 " +
            "C1 5A 2D C0 FF 21 02 84 55 BA 39 FA 0A 68 A3 9D " +
            "08 7F 61 71 22 C3 6B BA 68 73 8E 49 C3 D0 CB F6 " +
            "A8 90 63 7C 50 36 A5 EB 2E 33 CB D4 51 AF 83 0E " +
            "51 B0 3F F7 CA E2 B6 E9 E9 A3 AE 16 6F B4 C8 80 " +
            "99 6A 4B A8 03 AF BD 34",
            // Round #12
            // After Theta
            "E8 31 BC 2A 72 17 A0 C3 9D 5E A5 05 0D 4C E0 C8 " +
            "34 BC FA 60 86 9F 57 90 37 3F D7 07 B0 2D 5A 3E " +
            "AA 8D AF EA 80 F4 1F CE CB B0 ED 1E D2 2F 09 B5 " +
            "5B FC 3B EA 71 88 EA 78 7C C5 CD AC D2 D1 12 1C " +
            "C3 1B AB A2 1B 76 DA 3F 6C D4 C6 7D 12 40 71 86 " +
            "3B 57 77 23 4C DF 58 E9 5B D6 36 FC A1 8F 27 72 " +
            "81 F5 A8 50 8F A5 D9 B6 A7 E8 77 62 27 A4 F9 87 " +
            "03 80 6A 9A 2F 47 E8 06 64 46 50 CC 7C B9 11 9C " +
            "BD 30 70 6C E2 5E B9 7D CC 09 04 EE FF 02 D5 DB " +
            "F1 0D 32 0E AE A3 01 FD 8A 10 33 D8 0F E9 F1 DD " +
            "63 DD 3E 57 3C F7 40 2E 52 59 96 78 4C D0 38 F7 " +
            "C8 03 02 E3 3F 88 C0 AF 10 D1 FD 69 E3 D4 A2 C7 " +
            "7B 09 F6 39 CF 96 87 1F",
            // After Rho
            "E8 31 BC 2A 72 17 A0 C3 3B BD 4A 0B 1A 98 C0 91 " +
            "0D AF 3E 98 E1 E7 15 24 DB A2 E5 73 F3 73 7D 00 " +
            "A4 FF 70 56 6D 7C 55 07 21 FD 92 50 BB 0C DB EE " +
            "A3 1E 87 A8 8E B7 C5 BF 07 5F 71 33 AB 74 B4 04 " +
            "8D 55 D1 0D 3B ED 9F E1 14 67 C8 46 6D DC 27 01 " +
            "DF B9 BA 1B 61 FA C6 4A C8 6D 59 DB F0 87 3E 9E " +
            "85 7A 2C CD B6 0D AC 47 48 F3 0F 4F D1 EF C4 4E " +
            "CD 97 23 74 83 01 40 35 98 F9 72 23 38 C9 8C A0 " +
            "8E 4D DC 2B B7 AF 17 06 EA 6D E6 04 02 F7 7F 81 " +
            "34 A0 3F BE 41 C6 C1 75 DD 8A 10 33 D8 0F E9 F1 " +
            "03 B9 8C 75 FB 5C F1 DC 4B 65 59 E2 31 41 E3 DC " +
            "79 40 60 FC 07 11 F8 15 D1 FD 69 E3 D4 A2 C7 10 " +
            "E1 C7 5E 82 7D CE B3 E5",
            // After Pi
            "E8 31 BC 2A 72 17 A0 C3 A3 1E 87 A8 8E B7 C5 BF " +
            "85 7A 2C CD B6 0D AC 47 34 A0 3F BE 41 C6 C1 75 " +
            "E1 C7 5E 82 7D CE B3 E5 DB A2 E5 73 F3 73 7D 00 " +
            "14 67 C8 46 6D DC 27 01 DF B9 BA 1B 61 FA C6 4A " +
            "8E 4D DC 2B B7 AF 17 06 79 40 60 FC 07 11 F8 15 " +
            "3B BD 4A 0B 1A 98 C0 91 07 5F 71 33 AB 74 B4 04 " +
            "48 F3 0F 4F D1 EF C4 4E DD 8A 10 33 D8 0F E9 F1 " +
            "03 B9 8C 75 FB 5C F1 DC A4 FF 70 56 6D 7C 55 07 " +
            "21 FD 92 50 BB 0C DB EE C8 6D 59 DB F0 87 3E 9E " +
            "EA 6D E6 04 02 F7 7F 81 D1 FD 69 E3 D4 A2 C7 10 " +
            "0D AF 3E 98 E1 E7 15 24 8D 55 D1 0D 3B ED 9F E1 " +
            "CD 97 23 74 83 01 40 35 98 F9 72 23 38 C9 8C A0 " +
            "4B 65 59 E2 31 41 E3 DC",
            // After Chi
            "EC 51 94 6F 42 1F 88 83 93 9E 94 9A CF 75 84 8F " +
            "44 3D 6C CD 8A 05 9E C7 3C 90 9F 96 43 D7 C1 77 " +
            "E2 C9 5D 02 F1 6E F6 D9 10 3A D7 6A F3 51 BD 4A " +
            "14 23 8C 66 FB D9 36 05 AE B9 9A CF 61 EA 2E 5B " +
            "0C EF 59 28 47 CD 12 06 7D 05 68 F8 0B 9D FA 14 " +
            "73 1D 44 47 4A 13 80 DB 92 57 61 03 A3 74 9D B5 " +
            "4A C2 83 0B F2 BF D4 42 E5 8E 52 39 D8 8F E9 F0 " +
            "07 FB BD 45 5A 38 C5 D8 6C FF 39 DD 2D FF 71 17 " +
            "03 FD 34 54 B9 7C 9A EF D9 FD 50 38 24 87 BE 8E " +
            "CE 6F F6 10 2B AB 6F 86 D0 FD EB E3 46 A2 4D F8 " +
            "4D 2D 1C E8 61 E7 55 30 9D 3D 81 0E 03 25 13 61 " +
            "8E 93 2A B4 82 01 23 69 9C 73 54 3B F8 6F 98 80 " +
            "CB 35 98 E7 2B 49 69 1D",
            // After Iota 
            "67 D1 94 EF 42 1F 88 83 93 9E 94 9A CF 75 84 8F " +
            "44 3D 6C CD 8A 05 9E C7 3C 90 9F 96 43 D7 C1 77 " +
            "E2 C9 5D 02 F1 6E F6 D9 10 3A D7 6A F3 51 BD 4A " +
            "14 23 8C 66 FB D9 36 05 AE B9 9A CF 61 EA 2E 5B " +
            "0C EF 59 28 47 CD 12 06 7D 05 68 F8 0B 9D FA 14 " +
            "73 1D 44 47 4A 13 80 DB 92 57 61 03 A3 74 9D B5 " +
            "4A C2 83 0B F2 BF D4 42 E5 8E 52 39 D8 8F E9 F0 " +
            "07 FB BD 45 5A 38 C5 D8 6C FF 39 DD 2D FF 71 17 " +
            "03 FD 34 54 B9 7C 9A EF D9 FD 50 38 24 87 BE 8E " +
            "CE 6F F6 10 2B AB 6F 86 D0 FD EB E3 46 A2 4D F8 " +
            "4D 2D 1C E8 61 E7 55 30 9D 3D 81 0E 03 25 13 61 " +
            "8E 93 2A B4 82 01 23 69 9C 73 54 3B F8 6F 98 80 " +
            "CB 35 98 E7 2B 49 69 1D",
            // Round #13
            // After Theta
            "F3 7B F7 1F D4 3D 28 10 58 EB A8 67 07 9D E6 C9 " +
            "C0 CC CD 30 B8 26 A2 79 CC 47 67 64 67 40 E2 AF " +
            "2F 6C 2F 40 91 B4 19 35 84 90 B4 9A 65 73 1D D9 " +
            "DF 56 B0 9B 33 31 54 43 2A 48 3B 32 53 C9 12 E5 " +
            "FC 38 A1 DA 63 5A 31 DE B0 A0 1A BA 6B 47 15 F8 " +
            "E7 B7 27 B7 DC 31 20 48 59 22 5D FE 6B 9C FF F3 " +
            "CE 33 22 F6 C0 9C E8 FC 15 59 AA CB FC 18 CA 28 " +
            "CA 5E CF 07 3A E2 2A 34 F8 55 5A 2D BB DD D1 84 " +
            "C8 88 08 A9 71 94 F8 A9 5D 0C F1 C5 16 A4 82 30 " +
            "3E B8 0E E2 0F 3C 4C 5E 1D 58 99 A1 26 78 A2 14 " +
            "D9 87 7F 18 F7 C5 F5 A3 56 48 BD F3 CB CD 71 27 " +
            "0A 62 8B 49 B0 22 1F D7 6C A4 AC C9 DC F8 BB 58 " +
            "06 90 EA A5 4B 93 86 F1",
            // After Rho
            "F3 7B F7 1F D4 3D 28 10 B1 D6 51 CF 0E 3A CD 93 " +
            "30 73 33 0C AE 89 68 1E 06 24 FE CA 7C 74 46 76 " +
            "A4 CD A8 79 61 7B 01 8A 59 36 D7 91 4D 08 49 AB " +
            "BB 39 13 43 35 F4 6D 05 B9 0A D2 8E CC 54 B2 44 " +
            "9C 50 ED 31 AD 18 6F 7E 54 81 0F 0B AA A1 BB 76 " +
            "3A BF 3D B9 E5 8E 01 41 CF 67 89 74 F9 AF 71 FE " +
            "B1 07 E6 44 E7 77 9E 11 31 94 51 2A B2 54 97 F9 " +
            "03 1D 71 15 1A 65 AF E7 5A 76 BB A3 09 F1 AB B4 " +
            "21 35 8E 12 3F 15 19 11 41 98 2E 86 F8 62 0B 52 " +
            "87 C9 CB 07 D7 41 FC 81 14 1D 58 99 A1 26 78 A2 " +
            "D7 8F 66 1F FE 61 DC 17 58 21 F5 CE 2F 37 C7 9D " +
            "41 6C 31 09 56 E4 E3 5A A4 AC C9 DC F8 BB 58 6C " +
            "61 BC 01 A4 7A E9 D2 A4",
            // After Pi
            "F3 7B F7 1F D4 3D 28 10 BB 39 13 43 35 F4 6D 05 " +
            "B1 07 E6 44 E7 77 9E 11 87 C9 CB 07 D7 41 FC 81 " +
            "61 BC 01 A4 7A E9 D2 A4 06 24 FE CA 7C 74 46 76 " +
            "54 81 0F 0B AA A1 BB 76 3A BF 3D B9 E5 8E 01 41 " +
            "21 35 8E 12 3F 15 19 11 41 6C 31 09 56 E4 E3 5A " +
            "B1 D6 51 CF 0E 3A CD 93 B9 0A D2 8E CC 54 B2 44 " +
            "31 94 51 2A B2 54 97 F9 14 1D 58 99 A1 26 78 A2 " +
            "D7 8F 66 1F FE 61 DC 17 A4 CD A8 79 61 7B 01 8A " +
            "59 36 D7 91 4D 08 49 AB CF 67 89 74 F9 AF 71 FE " +
            "41 98 2E 86 F8 62 0B 52 A4 AC C9 DC F8 BB 58 6C " +
            "30 73 33 0C AE 89 68 1E 9C 50 ED 31 AD 18 6F 7E " +
            "03 1D 71 15 1A 65 AF E7 5A 76 BB A3 09 F1 AB B4 " +
            "58 21 F5 CE 2F 37 C7 9D",
            // After Chi
            "F3 7D 13 1B 16 3E BA 00 BD F1 1A 40 25 F4 0D 85 " +
            "D1 33 E6 E4 CF DF 9C 35 15 8A 3D 1C 53 55 D4 91 " +
            "69 BC 01 E4 5B 29 97 A1 2C 1A CE 7A 39 7A 46 77 " +
            "55 81 8D 09 B0 B0 A3 66 7A F7 0C B0 A5 6E E3 0B " +
            "27 35 40 D0 17 05 1D 35 11 ED 30 08 D4 65 5A 5A " +
            "B1 42 50 EF 3C 3A C8 2A BD 03 DA 1F CD 76 DA 46 " +
            "F2 16 77 2C EC 15 13 EC 34 4D 49 59 A1 3C 79 22 " +
            "DF 87 E4 1F 3E 25 EE 53 22 8C A0 1D D1 DC 31 DE " +
            "59 AE F1 13 4D 48 43 AB 6B 43 48 2C F9 36 21 D2 " +
            "41 D9 0E A7 F9 22 0A D0 FD 9E 9E 5C F4 BB 10 4D " +
            "33 7E 23 08 BC EC E8 9F C4 32 67 93 AC 88 6F 6E " +
            "03 1C 35 59 3C 63 EB EE 7A 24 B9 A3 89 79 83 B6 " +
            "D4 21 39 FF 2E 27 C0 FD",
            // After Iota 
            "78 7D 13 1B 16 3E BA 80 BD F1 1A 40 25 F4 0D 85 " +
            "D1 33 E6 E4 CF DF 9C 35 15 8A 3D 1C 53 55 D4 91 " +
            "69 BC 01 E4 5B 29 97 A1 2C 1A CE 7A 39 7A 46 77 " +
            "55 81 8D 09 B0 B0 A3 66 7A F7 0C B0 A5 6E E3 0B " +
            "27 35 40 D0 17 05 1D 35 11 ED 30 08 D4 65 5A 5A " +
            "B1 42 50 EF 3C 3A C8 2A BD 03 DA 1F CD 76 DA 46 " +
            "F2 16 77 2C EC 15 13 EC 34 4D 49 59 A1 3C 79 22 " +
            "DF 87 E4 1F 3E 25 EE 53 22 8C A0 1D D1 DC 31 DE " +
            "59 AE F1 13 4D 48 43 AB 6B 43 48 2C F9 36 21 D2 " +
            "41 D9 0E A7 F9 22 0A D0 FD 9E 9E 5C F4 BB 10 4D " +
            "33 7E 23 08 BC EC E8 9F C4 32 67 93 AC 88 6F 6E " +
            "03 1C 35 59 3C 63 EB EE 7A 24 B9 A3 89 79 83 B6 " +
            "D4 21 39 FF 2E 27 C0 FD",
            // Round #14
            // After Theta
            "66 CB D6 E6 0E 2E F8 58 2A 3C D5 C0 DD 58 AD C4 " +
            "62 C2 3B 11 5D 42 B6 95 38 D4 39 B1 C6 4E 95 4E " +
            "BD 1C 9F 43 33 82 74 78 32 AC 0B 87 21 6A 04 AF " +
            "C2 4C 42 89 48 1C 03 27 C9 06 D1 45 37 F3 C9 AB " +
            "0A 6B 44 7D 82 1E 5C EA C5 4D AE AF BC CE B9 83 " +
            "AF F4 95 12 24 2A 8A F2 2A CE 15 9F 35 DA 7A 07 " +
            "41 E7 AA D9 7E 88 39 4C 19 13 4D F4 34 27 38 FD " +
            "0B 27 7A B8 56 8E 0D 8A 3C 3A 65 E0 C9 CC 73 06 " +
            "CE 63 3E 93 B5 E4 E3 EA D8 B2 95 D9 6B AB 0B 72 " +
            "6C 87 0A 0A 6C 39 4B 0F 29 3E 00 FB 9C 10 F3 94 " +
            "2D C8 E6 F5 A4 FC AA 47 53 FF A8 13 54 24 CF 2F " +
            "B0 ED E8 AC AE FE C1 4E 57 7A BD 0E 1C 62 C2 69 " +
            "00 81 A7 58 46 8C 23 24",
            // After Rho
            "66 CB D6 E6 0E 2E F8 58 55 78 AA 81 BB B1 5A 89 " +
            "98 F0 4E 44 97 90 6D A5 EC 54 E9 84 43 9D 13 6B " +
            "11 A4 C3 EB E5 F8 1C 9A 18 A2 46 F0 2A C3 BA 70 " +
            "94 88 C4 31 70 22 CC 24 6A B2 41 74 D1 CD 7C F2 " +
            "35 A2 3E 41 0F 2E 75 85 9C 3B 58 DC E4 FA CA EB " +
            "7F A5 AF 94 20 51 51 94 1D A8 38 57 7C D6 68 EB " +
            "CD F6 43 CC 61 0A 3A 57 4E 70 FA 33 26 9A E8 69 " +
            "5C 2B C7 06 C5 85 13 3D C0 93 99 E7 0C 78 74 CA " +
            "67 B2 96 7C 5C DD 79 CC 05 39 6C D9 CA EC B5 D5 " +
            "67 E9 81 ED 50 41 81 2D 94 29 3E 00 FB 9C 10 F3 " +
            "AB 1E B5 20 9B D7 93 F2 4C FD A3 4E 50 91 3C BF " +
            "B6 1D 9D D5 D5 3F D8 09 7A BD 0E 1C 62 C2 69 57 " +
            "08 09 40 E0 29 96 11 E3",
            // After Pi
            "66 CB D6 E6 0E 2E F8 58 94 88 C4 31 70 22 CC 24 " +
            "CD F6 43 CC 61 0A 3A 57 67 E9 81 ED 50 41 81 2D " +
            "08 09 40 E0 29 96 11 E3 EC 54 E9 84 43 9D 13 6B " +
            "9C 3B 58 DC E4 FA CA EB 7F A5 AF 94 20 51 51 94 " +
            "67 B2 96 7C 5C DD 79 CC B6 1D 9D D5 D5 3F D8 09 " +
            "55 78 AA 81 BB B1 5A 89 6A B2 41 74 D1 CD 7C F2 " +
            "4E 70 FA 33 26 9A E8 69 94 29 3E 00 FB 9C 10 F3 " +
            "AB 1E B5 20 9B D7 93 F2 11 A4 C3 EB E5 F8 1C 9A " +
            "18 A2 46 F0 2A C3 BA 70 1D A8 38 57 7C D6 68 EB " +
            "05 39 6C D9 CA EC B5 D5 7A BD 0E 1C 62 C2 69 57 " +
            "98 F0 4E 44 97 90 6D A5 35 A2 3E 41 0F 2E 75 85 " +
            "5C 2B C7 06 C5 85 13 3D C0 93 99 E7 0C 78 74 CA " +
            "4C FD A3 4E 50 91 3C BF",
            // After Chi
            "2F BD D5 2A 0F 26 CA 0B B6 81 44 10 60 63 4D 0C " +
            "C5 F6 03 CC 48 9C 2A 95 01 2B 17 EB 56 69 69 35 " +
            "98 09 40 F1 59 96 15 C7 8F D0 4E 84 43 9C 02 7F " +
            "9C 29 48 B4 B8 76 E2 A3 EF A8 A6 15 A1 73 D1 95 " +
            "2F F2 F6 7C 5E 5D 7A AE A6 36 8D 8D 71 5D 10 89 " +
            "51 38 10 82 9D A3 DA 80 FA BB 45 74 08 C9 6C 60 " +
            "65 66 7B 13 26 D9 6B 69 C0 49 34 81 DB BC 58 FA " +
            "81 9C F4 54 DB 9B B7 80 14 AC FB EC B1 EC 5C 11 " +
            "18 B3 02 78 A8 EB 2F 64 67 2C 3A 53 5C D4 20 E9 " +
            "04 39 AD 3A 4F D4 A1 5D 72 BF 0A 0C 68 C1 CB 37 " +
            "D0 F9 8F 42 57 11 6F 9D B5 32 26 A0 07 56 11 47 " +
            "50 47 E5 0E 95 04 1B 08 50 93 D5 E7 8B 78 35 CA " +
            "69 FF 93 4F 58 BF 2C BF",
            // After Iota 
            "A6 3D D5 2A 0F 26 CA 8B B6 81 44 10 60 63 4D 0C " +
            "C5 F6 03 CC 48 9C 2A 95 01 2B 17 EB 56 69 69 35 " +
            "98 09 40 F1 59 96 15 C7 8F D0 4E 84 43 9C 02 7F " +
            "9C 29 48 B4 B8 76 E2 A3 EF A8 A6 15 A1 73 D1 95 " +
            "2F F2 F6 7C 5E 5D 7A AE A6 36 8D 8D 71 5D 10 89 " +
            "51 38 10 82 9D A3 DA 80 FA BB 45 74 08 C9 6C 60 " +
            "65 66 7B 13 26 D9 6B 69 C0 49 34 81 DB BC 58 FA " +
            "81 9C F4 54 DB 9B B7 80 14 AC FB EC B1 EC 5C 11 " +
            "18 B3 02 78 A8 EB 2F 64 67 2C 3A 53 5C D4 20 E9 " +
            "04 39 AD 3A 4F D4 A1 5D 72 BF 0A 0C 68 C1 CB 37 " +
            "D0 F9 8F 42 57 11 6F 9D B5 32 26 A0 07 56 11 47 " +
            "50 47 E5 0E 95 04 1B 08 50 93 D5 E7 8B 78 35 CA " +
            "69 FF 93 4F 58 BF 2C BF",
            // Round #15
            // After Theta
            "F9 FA AE 51 32 CA 65 14 FB A7 B9 BC 5A 4B 3B E5 " +
            "CD 11 34 53 18 B5 69 94 31 BF 57 AB D6 D2 68 31 " +
            "5B 32 12 3F 21 7A 89 C1 D0 17 35 FF 7E 70 AD E0 " +
            "D1 0F B5 18 82 5E 94 4A E7 4F 91 8A F1 5A 92 94 " +
            "1F 66 B6 3C DE E6 7B AA 65 0D DF 43 09 B1 8C 8F " +
            "0E FF 6B F9 A0 4F 75 1F B7 9D B8 D8 32 E1 1A 89 " +
            "6D 81 4C 8C 76 F0 28 68 F0 DD 74 C1 5B 07 59 FE " +
            "42 A7 A6 9A A3 77 2B 86 4B 6B 80 97 8C 00 F3 8E " +
            "55 95 FF D4 92 C3 59 8D 6F CB 0D CC 0C FD 63 E8 " +
            "34 AD ED 7A CF 6F A0 59 B1 84 58 C2 10 2D 57 31 " +
            "8F 3E F4 39 6A FD C0 02 F8 14 DB 0C 3D 7E 67 AE " +
            "58 A0 D2 91 C5 2D 58 09 60 07 95 A7 0B C3 34 CE " +
            "AA C4 C1 81 20 53 B0 B9",
            // After Rho
            "F9 FA AE 51 32 CA 65 14 F7 4F 73 79 B5 96 76 CA " +
            "73 04 CD 14 46 6D 1A 65 2D 8D 16 13 F3 7B B5 6A " +
            "D1 4B 0C DE 92 91 F8 09 EF 07 D7 0A 0E 7D 51 F3 " +
            "8B 21 E8 45 A9 14 FD 50 E5 F9 53 A4 62 BC 96 24 " +
            "33 5B 1E 6F F3 3D D5 0F CB F8 58 D6 F0 3D 94 10 " +
            "70 F8 5F CB 07 7D AA FB 24 DE 76 E2 62 CB 84 6B " +
            "62 B4 83 47 41 6B 0B 64 0E B2 FC E1 BB E9 82 B7 " +
            "CD D1 BB 15 43 A1 53 53 2F 19 01 E6 1D 97 D6 00 " +
            "9F 5A 72 38 AB B1 AA F2 31 F4 B7 E5 06 66 86 FE " +
            "0D 34 8B A6 B5 5D EF F9 31 B1 84 58 C2 10 2D 57 " +
            "03 0B 3C FA D0 E7 A8 F5 E2 53 6C 33 F4 F8 9D B9 " +
            "0B 54 3A B2 B8 05 2B 01 07 95 A7 0B C3 34 CE 60 " +
            "6C AE 2A 71 70 20 C8 14",
            // After Pi
            "F9 FA AE 51 32 CA 65 14 8B 21 E8 45 A9 14 FD 50 " +
            "62 B4 83 47 41 6B 0B 64 0D 34 8B A6 B5 5D EF F9 " +
            "6C AE 2A 71 70 20 C8 14 2D 8D 16 13 F3 7B B5 6A " +
            "CB F8 58 D6 F0 3D 94 10 70 F8 5F CB 07 7D AA FB " +
            "9F 5A 72 38 AB B1 AA F2 0B 54 3A B2 B8 05 2B 01 " +
            "F7 4F 73 79 B5 96 76 CA E5 F9 53 A4 62 BC 96 24 " +
            "0E B2 FC E1 BB E9 82 B7 31 B1 84 58 C2 10 2D 57 " +
            "03 0B 3C FA D0 E7 A8 F5 D1 4B 0C DE 92 91 F8 09 " +
            "EF 07 D7 0A 0E 7D 51 F3 24 DE 76 E2 62 CB 84 6B " +
            "31 F4 B7 E5 06 66 86 FE 07 95 A7 0B C3 34 CE 60 " +
            "73 04 CD 14 46 6D 1A 65 33 5B 1E 6F F3 3D D5 0F " +
            "CD D1 BB 15 43 A1 53 53 2F 19 01 E6 1D 97 D6 00 " +
            "E2 53 6C 33 F4 F8 9D B9",
            // After Chi
            "99 6E AD 53 72 A1 67 30 86 21 E0 E5 1D 00 19 C9 " +
            "02 3E A3 16 01 4B 0B 60 9C 64 0F A6 B7 97 CA F9 " +
            "6E AF 6A 75 F9 34 50 54 1D 8D 11 1A F4 3B 9F 81 " +
            "44 FA 78 E6 58 BD 94 10 70 FC 57 49 17 79 AB FA " +
            "BB D3 76 39 E8 CB 3E 98 C9 24 72 76 B8 01 2B 11 " +
            "FD 4D DF 38 2C D7 76 59 D4 F8 53 BC 22 AC BB 64 " +
            "0C B8 C4 43 AB 0E 02 17 C5 F5 C7 59 E7 00 7B 5D " +
            "03 BB 3C 7E 92 CF 28 D1 D1 93 2C 3E F2 13 7C 01 " +
            "FE 27 56 0F 0A 59 53 67 22 DF 76 E8 A3 DB CC 6B " +
            "E1 BE BF 31 16 E7 B6 F7 29 91 74 0B CF 58 CF 92 " +
            "BF 84 6C 04 46 ED 18 35 11 53 1E 8D EF 2B 51 0F " +
            "0D 93 D7 04 A3 C9 5A EA 3E 1D 80 E2 1F 92 D4 44 " +
            "E2 08 7E 58 45 E8 58 B3",
            // After Iota 
            "9A EE AD 53 72 A1 67 B0 86 21 E0 E5 1D 00 19 C9 " +
            "02 3E A3 16 01 4B 0B 60 9C 64 0F A6 B7 97 CA F9 " +
            "6E AF 6A 75 F9 34 50 54 1D 8D 11 1A F4 3B 9F 81 " +
            "44 FA 78 E6 58 BD 94 10 70 FC 57 49 17 79 AB FA " +
            "BB D3 76 39 E8 CB 3E 98 C9 24 72 76 B8 01 2B 11 " +
            "FD 4D DF 38 2C D7 76 59 D4 F8 53 BC 22 AC BB 64 " +
            "0C B8 C4 43 AB 0E 02 17 C5 F5 C7 59 E7 00 7B 5D " +
            "03 BB 3C 7E 92 CF 28 D1 D1 93 2C 3E F2 13 7C 01 " +
            "FE 27 56 0F 0A 59 53 67 22 DF 76 E8 A3 DB CC 6B " +
            "E1 BE BF 31 16 E7 B6 F7 29 91 74 0B CF 58 CF 92 " +
            "BF 84 6C 04 46 ED 18 35 11 53 1E 8D EF 2B 51 0F " +
            "0D 93 D7 04 A3 C9 5A EA 3E 1D 80 E2 1F 92 D4 44 " +
            "E2 08 7E 58 45 E8 58 B3",
            // Round #16
            // After Theta
            "06 E8 85 06 2F 2C CB AF 30 74 E1 4F 78 EE 9B 8D " +
            "80 AB 23 00 E1 7B E5 AA 12 00 C3 0A B8 2D 76 9E " +
            "7B 3C AD F6 74 7B 68 62 81 8B 39 4F A9 B6 33 9E " +
            "F2 AF 79 4C 3D 53 16 54 F2 69 D7 5F F7 49 45 30 " +
            "35 B7 BA 95 E7 71 82 FF DC B7 B5 F5 35 4E 13 27 " +
            "61 4B F7 6D 71 5A DA 46 62 AD 52 16 47 42 39 20 " +
            "8E 2D 44 55 4B 3E EC DD 4B 91 0B F5 E8 BA C7 3A " +
            "16 28 FB FD 1F 80 10 E7 4D 95 04 6B AF 9E D0 1E " +
            "48 72 57 A5 6F B7 D1 23 A0 4A F6 FE 43 EB 22 A1 " +
            "6F DA 73 9D 19 5D 0A 90 3C 02 B3 88 42 17 F7 A4 " +
            "23 82 44 51 1B 60 B4 2A A7 06 1F 27 8A C5 D3 4B " +
            "8F 06 57 12 43 F9 B4 20 B0 79 4C 4E 10 28 68 23 " +
            "F7 9B B9 DB C8 A7 60 85",
            // After Rho
            "06 E8 85 06 2F 2C CB AF 61 E8 C2 9F F0 DC 37 1B " +
            "E0 EA 08 40 F8 5E B9 2A DB 62 E7 29 01 30 AC 80 " +
            "DB 43 13 DB E3 69 B5 A7 94 6A 3B E3 19 B8 98 F3 " +
            "C7 D4 33 65 41 25 FF 9A 8C 7C DA F5 D7 7D 52 11 " +
            "5B DD CA F3 38 C1 FF 9A 34 71 C2 7D 5B 5B 5F E3 " +
            "0A 5B BA 6F 8B D3 D2 36 80 88 B5 4A 59 1C 09 E5 " +
            "AA 5A F2 61 EF 76 6C 21 75 8F 75 96 22 17 EA D1 " +
            "FE 0F 40 88 73 0B 94 FD D6 5E 3D A1 3D 9A 2A 09 " +
            "AA F4 ED 36 7A 04 49 EE 91 50 50 25 7B FF A1 75 " +
            "4B 01 F2 4D 7B AE 33 A3 A4 3C 02 B3 88 42 17 F7 " +
            "D1 AA 8C 08 12 45 6D 80 9D 1A 7C 9C 28 16 4F 2F " +
            "D1 E0 4A 62 28 9F 16 E4 79 4C 4E 10 28 68 23 B0 " +
            "58 E1 FD 66 EE 36 F2 29",
            // After Pi
            "06 E8 85 06 2F 2C CB AF C7 D4 33 65 41 25 FF 9A " +
            "AA 5A F2 61 EF 76 6C 21 4B 01 F2 4D 7B AE 33 A3 " +
            "58 E1 FD 66 EE 36 F2 29 DB 62 E7 29 01 30 AC 80 " +
            "34 71 C2 7D 5B 5B 5F E3 0A 5B BA 6F 8B D3 D2 36 " +
            "AA F4 ED 36 7A 04 49 EE D1 E0 4A 62 28 9F 16 E4 " +
            "61 E8 C2 9F F0 DC 37 1B 8C 7C DA F5 D7 7D 52 11 " +
            "75 8F 75 96 22 17 EA D1 A4 3C 02 B3 88 42 17 F7 " +
            "D1 AA 8C 08 12 45 6D 80 DB 43 13 DB E3 69 B5 A7 " +
            "94 6A 3B E3 19 B8 98 F3 80 88 B5 4A 59 1C 09 E5 " +
            "91 50 50 25 7B FF A1 75 79 4C 4E 10 28 68 23 B0 " +
            "E0 EA 08 40 F8 5E B9 2A 5B DD CA F3 38 C1 FF 9A " +
            "FE 0F 40 88 73 0B 94 FD D6 5E 3D A1 3D 9A 2A 09 " +
            "9D 1A 7C 9C 28 16 4F 2F",
            // After Chi
            "2E E2 45 06 81 7E CB 8E 86 D5 33 69 51 AD EC 18 " +
            "BA BA FF 43 6B 66 AC 29 4D 09 F2 4D 7A A6 3A 25 " +
            "99 F5 CF 07 AE 37 C6 39 D1 68 DF 2B 81 B0 2C 94 " +
            "94 D5 87 6D 2B 5F 56 2B 5B 5B B8 2F 8B 48 C4 36 " +
            "A0 F6 48 3F 7B 24 E1 EE F5 F1 4A 36 72 D4 45 87 " +
            "10 6B E7 9D D0 DE 9F DB 0C 4C D8 D4 5F 3D 47 37 " +
            "24 0D F9 9E 30 12 82 D1 84 7C 40 24 68 DA 05 EC " +
            "5D BE 94 68 15 64 2D 80 DB C3 97 D3 A3 6D B4 A3 " +
            "85 3A 7B C6 3B 5B 38 E3 E8 84 BB 5A 59 1C 0B 65 " +
            "13 53 41 EE B8 FE 35 72 7D 64 66 30 30 F8 2B E0 " +
            "44 E8 08 48 BB 54 B9 4F 5B 8D F7 D2 34 51 D5 9A " +
            "F7 0F 00 94 73 0F D1 DB B6 BE 3D E1 ED D2 9A 09 " +
            "86 0F BE 2F 28 97 09 BF",
            // After Iota 
            "2C 62 45 06 81 7E CB 0E 86 D5 33 69 51 AD EC 18 " +
            "BA BA FF 43 6B 66 AC 29 4D 09 F2 4D 7A A6 3A 25 " +
            "99 F5 CF 07 AE 37 C6 39 D1 68 DF 2B 81 B0 2C 94 " +
            "94 D5 87 6D 2B 5F 56 2B 5B 5B B8 2F 8B 48 C4 36 " +
            "A0 F6 48 3F 7B 24 E1 EE F5 F1 4A 36 72 D4 45 87 " +
            "10 6B E7 9D D0 DE 9F DB 0C 4C D8 D4 5F 3D 47 37 " +
            "24 0D F9 9E 30 12 82 D1 84 7C 40 24 68 DA 05 EC " +
            "5D BE 94 68 15 64 2D 80 DB C3 97 D3 A3 6D B4 A3 " +
            "85 3A 7B C6 3B 5B 38 E3 E8 84 BB 5A 59 1C 0B 65 " +
            "13 53 41 EE B8 FE 35 72 7D 64 66 30 30 F8 2B E0 " +
            "44 E8 08 48 BB 54 B9 4F 5B 8D F7 D2 34 51 D5 9A " +
            "F7 0F 00 94 73 0F D1 DB B6 BE 3D E1 ED D2 9A 09 " +
            "86 0F BE 2F 28 97 09 BF",
            // Round #17
            // After Theta
            "66 44 4D C9 05 1C 66 95 40 50 DB 3A 6D DB F9 55 " +
            "E2 9C 13 34 39 4B 5E EC 03 CD 64 FC 22 58 13 96 " +
            "B0 0F 8D 09 02 10 5D 3F 9B 4E D7 E4 05 D2 81 0F " +
            "52 50 6F 3E 17 29 43 66 03 7D 54 58 D9 65 36 F3 " +
            "EE 32 DE 8E 23 DA C8 5D DC 0B 08 38 DE F3 DE 81 " +
            "5A 4D EF 52 54 BC 32 40 CA C9 30 87 63 4B 52 7A " +
            "7C 2B 15 E9 62 3F 70 14 CA B8 D6 95 30 24 2C 5F " +
            "74 44 D6 66 B9 43 B6 86 91 E5 9F 1C 27 0F 19 38 " +
            "43 BF 93 95 07 2D 2D AE B0 A2 57 2D 0B 31 F9 A0 " +
            "5D 97 D7 5F E0 00 1C C1 54 9E 24 3E 9C DF B0 E6 " +
            "0E CE 00 87 3F 36 14 D4 9D 08 1F 81 08 27 C0 D7 " +
            "AF 29 EC E3 21 22 23 1E F8 7A AB 50 B5 2C B3 BA " +
            "AF F5 FC 21 84 B0 92 B9",
            // After Rho
            "66 44 4D C9 05 1C 66 95 80 A0 B6 75 DA B6 F3 AB " +
            "38 E7 04 4D CE 92 17 BB 82 35 61 39 D0 4C C6 2F " +
            "80 E8 FA 81 7D 68 4C 10 5E 20 1D F8 B0 E9 74 4D " +
            "E6 73 91 32 64 26 05 F5 FC 40 1F 15 56 76 99 CD " +
            "19 6F C7 11 6D E4 2E 77 EF 1D C8 BD 80 80 E3 3D " +
            "D2 6A 7A 97 A2 E2 95 01 E9 29 27 C3 1C 8E 2D 49 " +
            "48 17 FB 81 A3 E0 5B A9 48 58 BE 94 71 AD 2B 61 " +
            "B3 DC 21 5B 43 3A 22 6B 39 4E 1E 32 70 22 CB 3F " +
            "B2 F2 A0 A5 C5 75 E8 77 7C 50 58 D1 AB 96 85 98 " +
            "80 23 B8 EB F2 FA 0B 1C E6 54 9E 24 3E 9C DF B0 " +
            "50 50 3B 38 03 1C FE D8 77 22 7C 04 22 9C 00 5F " +
            "35 85 7D 3C 44 64 C4 E3 7A AB 50 B5 2C B3 BA F8 " +
            "64 EE 6B 3D 7F 08 21 AC",
            // After Pi
            "66 44 4D C9 05 1C 66 95 E6 73 91 32 64 26 05 F5 " +
            "48 17 FB 81 A3 E0 5B A9 80 23 B8 EB F2 FA 0B 1C " +
            "64 EE 6B 3D 7F 08 21 AC 82 35 61 39 D0 4C C6 2F " +
            "EF 1D C8 BD 80 80 E3 3D D2 6A 7A 97 A2 E2 95 01 " +
            "B2 F2 A0 A5 C5 75 E8 77 35 85 7D 3C 44 64 C4 E3 " +
            "80 A0 B6 75 DA B6 F3 AB FC 40 1F 15 56 76 99 CD " +
            "48 58 BE 94 71 AD 2B 61 E6 54 9E 24 3E 9C DF B0 " +
            "50 50 3B 38 03 1C FE D8 80 E8 FA 81 7D 68 4C 10 " +
            "5E 20 1D F8 B0 E9 74 4D E9 29 27 C3 1C 8E 2D 49 " +
            "7C 50 58 D1 AB 96 85 98 7A AB 50 B5 2C B3 BA F8 " +
            "38 E7 04 4D CE 92 17 BB 19 6F C7 11 6D E4 2E 77 " +
            "B3 DC 21 5B 43 3A 22 6B 39 4E 1E 32 70 22 CB 3F " +
            "77 22 7C 04 22 9C 00 5F",
            // After Chi
            "6E 40 27 48 86 DC 3C 9D 66 53 91 58 34 3C 05 E1 " +
            "2C DB B8 95 AE E0 7B 09 82 23 BC 2B F2 EE 4D 0D " +
            "E4 DD FB 0F 1F 2A 20 CC 92 57 53 3B F2 2E D2 2F " +
            "CF 8D 48 9D C5 95 8B 4B D7 6F 27 8F A2 E2 91 81 " +
            "30 C2 A0 A4 55 7D EA 7B 58 8D F5 B8 44 E4 E5 F3 " +
            "80 B8 16 F5 FB 3F D1 8B 5A 44 1F 35 58 66 4D 5D " +
            "58 58 9F 8C 70 AD 0B 29 66 F4 1A 61 E6 3E DE 93 " +
            "2C 10 32 38 07 5C F6 9C 21 E1 D8 82 71 6E 45 10 " +
            "4A 70 45 E8 13 F9 F4 DD EB 82 27 E7 18 AF 17 29 " +
            "FC 10 F2 D1 FA DE C1 98 24 AB 55 CD AC 32 8A B5 " +
            "9A 77 24 07 CC 88 17 B3 11 6D D9 31 5D E4 E7 63 " +
            "F5 FC 41 5F 41 A6 22 2B 31 8B 1E 7B BC 20 DC 9F " +
            "76 2A BF 14 03 F8 28 1B",
            // After Iota 
            "EE 40 27 48 86 DC 3C 1D 66 53 91 58 34 3C 05 E1 " +
            "2C DB B8 95 AE E0 7B 09 82 23 BC 2B F2 EE 4D 0D " +
            "E4 DD FB 0F 1F 2A 20 CC 92 57 53 3B F2 2E D2 2F " +
            "CF 8D 48 9D C5 95 8B 4B D7 6F 27 8F A2 E2 91 81 " +
            "30 C2 A0 A4 55 7D EA 7B 58 8D F5 B8 44 E4 E5 F3 " +
            "80 B8 16 F5 FB 3F D1 8B 5A 44 1F 35 58 66 4D 5D " +
            "58 58 9F 8C 70 AD 0B 29 66 F4 1A 61 E6 3E DE 93 " +
            "2C 10 32 38 07 5C F6 9C 21 E1 D8 82 71 6E 45 10 " +
            "4A 70 45 E8 13 F9 F4 DD EB 82 27 E7 18 AF 17 29 " +
            "FC 10 F2 D1 FA DE C1 98 24 AB 55 CD AC 32 8A B5 " +
            "9A 77 24 07 CC 88 17 B3 11 6D D9 31 5D E4 E7 63 " +
            "F5 FC 41 5F 41 A6 22 2B 31 8B 1E 7B BC 20 DC 9F " +
            "76 2A BF 14 03 F8 28 1B",
            // Round #18
            // After Theta
            "7C 8E 44 4C BB 21 0C 83 5A 4F C2 07 4C 5B C1 BC " +
            "B7 40 37 35 47 94 63 84 BB 32 77 A8 31 F9 BB B5 " +
            "73 21 2D 4C 7C 2F 9E 1A 00 99 30 3F CF D3 E2 B1 " +
            "F3 91 1B C2 BD F2 4F 16 4C F4 A8 2F 4B 96 89 0C " +
            "09 D3 6B 27 96 6A 1C C3 CF 71 23 FB 27 E1 5B 25 " +
            "12 76 75 F1 C6 C2 E1 15 66 58 4C 6A 20 01 89 00 " +
            "C3 C3 10 2C 99 D9 13 A4 5F E5 D1 E2 25 29 28 2B " +
            "BB EC E4 7B 64 59 48 4A B3 2F BB 86 4C 93 75 8E " +
            "76 6C 16 B7 6B 9E 30 80 70 19 A8 47 F1 DB 0F A4 " +
            "C5 01 39 52 39 C9 37 20 B3 57 83 8E CF 37 34 63 " +
            "08 B9 47 03 F1 75 27 2D 2D 71 8A 6E 25 83 23 3E " +
            "6E 67 CE FF A8 D2 3A A6 08 9A D5 F8 7F 37 2A 27 " +
            "E1 D6 69 57 60 FD 96 CD",
            // After Rho
            "7C 8E 44 4C BB 21 0C 83 B5 9E 84 0F 98 B6 82 79 " +
            "2D D0 4D CD 11 E5 18 E1 93 BF 5B BB 2B 73 87 1A " +
            "7B F1 D4 98 0B 69 61 E2 F3 3C 2D 1E 0B 90 09 F3 " +
            "21 DC 2B FF 64 31 1F B9 03 13 3D EA CB 92 65 22 " +
            "E9 B5 13 4B 35 8E E1 84 BE 55 F2 1C 37 B2 7F 12 " +
            "90 B0 AB 8B 37 16 0E AF 02 98 61 31 A9 81 04 24 " +
            "60 C9 CC 9E 20 1D 1E 86 52 50 56 BE CA A3 C5 4B " +
            "3D B2 2C 24 A5 5D 76 F2 0D 99 26 EB 1C 67 5F 76 " +
            "E2 76 CD 13 06 D0 8E CD 07 52 B8 0C D4 A3 F8 ED " +
            "F9 06 A4 38 20 47 2A 27 63 B3 57 83 8E CF 37 34 " +
            "9D B4 20 E4 1E 0D C4 D7 B4 C4 29 BA 95 0C 8E F8 " +
            "ED CC F9 1F 55 5A C7 D4 9A D5 F8 7F 37 2A 27 08 " +
            "65 73 B8 75 DA 15 58 BF",
            // After Pi
            "7C 8E 44 4C BB 21 0C 83 21 DC 2B FF 64 31 1F B9 " +
            "60 C9 CC 9E 20 1D 1E 86 F9 06 A4 38 20 47 2A 27 " +
            "65 73 B8 75 DA 15 58 BF 93 BF 5B BB 2B 73 87 1A " +
            "BE 55 F2 1C 37 B2 7F 12 90 B0 AB 8B 37 16 0E AF " +
            "E2 76 CD 13 06 D0 8E CD ED CC F9 1F 55 5A C7 D4 " +
            "B5 9E 84 0F 98 B6 82 79 03 13 3D EA CB 92 65 22 " +
            "52 50 56 BE CA A3 C5 4B 63 B3 57 83 8E CF 37 34 " +
            "9D B4 20 E4 1E 0D C4 D7 7B F1 D4 98 0B 69 61 E2 " +
            "F3 3C 2D 1E 0B 90 09 F3 02 98 61 31 A9 81 04 24 " +
            "07 52 B8 0C D4 A3 F8 ED 9A D5 F8 7F 37 2A 27 08 " +
            "2D D0 4D CD 11 E5 18 E1 E9 B5 13 4B 35 8E E1 84 " +
            "3D B2 2C 24 A5 5D 76 F2 0D 99 26 EB 1C 67 5F 76 " +
            "B4 C4 29 BA 95 0C 8E F8",
            // After Chi
            "3C 8F 80 4C BB 2D 0C 85 B8 DA 0B DF 64 73 3F 98 " +
            "64 B8 D4 DB FA 0D 4E 1E E1 8A E0 30 01 67 2E 27 " +
            "64 23 93 C6 9E 05 4B 87 93 1F 52 38 2B 77 87 B7 " +
            "DC 13 B6 0C 37 72 FF 52 9D 38 9B 87 66 1C 4F BF " +
            "F0 45 CF B3 2C F1 8E C7 C1 8C 59 1B 41 DA BF D4 " +
            "E5 DE C6 1B 98 97 02 30 22 B0 3C EB CF DE 57 16 " +
            "CE 54 76 DA DA A3 05 88 43 B9 D3 88 0E 7D 35 1C " +
            "9F B5 19 04 5D 0D A1 D5 7B 71 94 B9 AB 68 65 E6 " +
            "F6 7E B5 12 5F B2 F1 3A 9A 1D 21 42 8A 89 03 24 " +
            "66 72 BC 8C DC E2 B8 0F 1A D9 D1 79 37 BA 2F 19 " +
            "39 D2 61 E9 91 B4 0E 93 E9 BC 11 80 2D AC E8 80 " +
            "8D F6 25 34 24 55 F6 7A 04 89 62 AE 1C 86 4F 77 " +
            "74 E1 3B B8 B1 06 6F FC",
            // After Iota 
            "36 0F 80 4C BB 2D 0C 85 B8 DA 0B DF 64 73 3F 98 " +
            "64 B8 D4 DB FA 0D 4E 1E E1 8A E0 30 01 67 2E 27 " +
            "64 23 93 C6 9E 05 4B 87 93 1F 52 38 2B 77 87 B7 " +
            "DC 13 B6 0C 37 72 FF 52 9D 38 9B 87 66 1C 4F BF " +
            "F0 45 CF B3 2C F1 8E C7 C1 8C 59 1B 41 DA BF D4 " +
            "E5 DE C6 1B 98 97 02 30 22 B0 3C EB CF DE 57 16 " +
            "CE 54 76 DA DA A3 05 88 43 B9 D3 88 0E 7D 35 1C " +
            "9F B5 19 04 5D 0D A1 D5 7B 71 94 B9 AB 68 65 E6 " +
            "F6 7E B5 12 5F B2 F1 3A 9A 1D 21 42 8A 89 03 24 " +
            "66 72 BC 8C DC E2 B8 0F 1A D9 D1 79 37 BA 2F 19 " +
            "39 D2 61 E9 91 B4 0E 93 E9 BC 11 80 2D AC E8 80 " +
            "8D F6 25 34 24 55 F6 7A 04 89 62 AE 1C 86 4F 77 " +
            "74 E1 3B B8 B1 06 6F FC",
            // Round #19
            // After Theta
            "D0 5B F2 00 62 C0 04 2B FA C9 90 00 87 BF 3F 00 " +
            "5C 19 B4 23 D2 D3 05 70 69 F1 AF F0 E1 D5 F5 96 " +
            "50 74 73 90 19 A8 ED EC 75 4B 20 74 F2 9A 8F 19 " +
            "9E 00 2D D3 D4 BE FF CA A5 99 FB 7F 4E C2 04 D1 " +
            "78 3E 80 73 CC 43 55 76 F5 DB B9 4D C6 77 19 BF " +
            "03 8A B4 57 41 7A 0A 9E 60 A3 A7 34 2C 12 57 8E " +
            "F6 F5 16 22 F2 7D 4E E6 CB C2 9C 48 EE CF EE AD " +
            "AB E2 F9 52 DA A0 07 BE 9D 25 E6 F5 72 85 6D 48 " +
            "B4 6D 2E CD BC 7E F1 A2 A2 BC 41 BA A2 57 48 4A " +
            "EE 09 F3 4C 3C 50 63 BE 2E 8E 31 2F B0 17 89 72 " +
            "DF 86 13 A5 48 59 06 3D AB AF 8A 5F CE 60 E8 18 " +
            "B5 57 45 CC 0C 8B BD 14 8C F2 2D 6E FC 34 94 C6 " +
            "40 B6 DB EE 36 AB C9 97",
            // After Rho
            "D0 5B F2 00 62 C0 04 2B F4 93 21 01 0E 7F 7F 00 " +
            "57 06 ED 88 F4 74 01 1C 5E 5D 6F 99 16 FF 0A 1F " +
            "40 6D 67 87 A2 9B 83 CC 27 AF F9 98 51 B7 04 42 " +
            "32 4D ED FB AF EC 09 D0 74 69 E6 FE 9F 93 30 41 " +
            "1F C0 39 E6 A1 2A 3B 3C 97 F1 5B BF 9D DB 64 7C " +
            "1C 50 A4 BD 0A D2 53 F0 39 82 8D 9E D2 B0 48 5C " +
            "10 91 EF 73 32 B7 AF B7 9F DD 5B 97 85 39 91 DC " +
            "29 6D D0 03 DF 55 F1 7C EB E5 0A DB 90 3A 4B CC " +
            "A5 99 D7 2F 5E 94 B6 CD 24 25 51 DE 20 5D D1 2B " +
            "6A CC D7 3D 61 9E 89 07 72 2E 8E 31 2F B0 17 89 " +
            "19 F4 7C 1B 4E 94 22 65 AC BE 2A 7E 39 83 A1 63 " +
            "F6 AA 88 99 61 B1 97 A2 F2 2D 6E FC 34 94 C6 8C " +
            "F2 25 90 ED B6 BB CD 6A",
            // After Pi
            "D0 5B F2 00 62 C0 04 2B 32 4D ED FB AF EC 09 D0 " +
            "10 91 EF 73 32 B7 AF B7 6A CC D7 3D 61 9E 89 07 " +
            "F2 25 90 ED B6 BB CD 6A 5E 5D 6F 99 16 FF 0A 1F " +
            "97 F1 5B BF 9D DB 64 7C 1C 50 A4 BD 0A D2 53 F0 " +
            "A5 99 D7 2F 5E 94 B6 CD F6 AA 88 99 61 B1 97 A2 " +
            "F4 93 21 01 0E 7F 7F 00 74 69 E6 FE 9F 93 30 41 " +
            "9F DD 5B 97 85 39 91 DC 72 2E 8E 31 2F B0 17 89 " +
            "19 F4 7C 1B 4E 94 22 65 40 6D 67 87 A2 9B 83 CC " +
            "27 AF F9 98 51 B7 04 42 39 82 8D 9E D2 B0 48 5C " +
            "24 25 51 DE 20 5D D1 2B F2 2D 6E FC 34 94 C6 8C " +
            "57 06 ED 88 F4 74 01 1C 1F C0 39 E6 A1 2A 3B 3C " +
            "29 6D D0 03 DF 55 F1 7C EB E5 0A DB 90 3A 4B CC " +
            "AC BE 2A 7E 39 83 A1 63",
            // After Chi
            "D0 CB F0 00 72 D3 A2 0C 58 01 FD F7 EE E4 09 D0 " +
            "80 B0 EF B3 A4 96 EB DF 6A 96 B5 3D 21 DE 89 06 " +
            "D0 21 9D 16 3B 97 C4 BA 56 5D CB 99 14 FF 19 9F " +
            "36 78 08 BD C9 DF C0 71 4E 72 AC 2D 2B F3 52 D2 " +
            "AD CC B0 2F 48 DA BE D0 77 0A 98 BF E8 B1 F3 C2 " +
            "7F 07 38 00 0E 57 FE 9C 14 4B 62 DE B5 13 36 40 " +
            "96 0D 2B 9D C5 3D B1 B8 96 2D 8F 31 2F DB 4A 89 " +
            "19 9C BA E5 DF 14 22 24 58 6D 63 81 20 9B CB D0 " +
            "23 8A A9 D8 71 FA 95 61 EB 8A A3 BE C6 30 4E D8 " +
            "24 65 50 DD A2 56 D0 6B D5 AF F6 E4 65 B0 C2 8E " +
            "77 2B 2D 89 AA 21 C1 5C DD 40 33 3E A1 00 31 BC " +
            "2D 77 F0 27 F6 D4 51 5F B8 E5 CF 5B 54 4E 4B D0 " +
            "A4 7E 3A 18 38 89 9B 43",
            // After Iota 
            "DA CB F0 80 72 D3 A2 8C 58 01 FD F7 EE E4 09 D0 " +
            "80 B0 EF B3 A4 96 EB DF 6A 96 B5 3D 21 DE 89 06 " +
            "D0 21 9D 16 3B 97 C4 BA 56 5D CB 99 14 FF 19 9F " +
            "36 78 08 BD C9 DF C0 71 4E 72 AC 2D 2B F3 52 D2 " +
            "AD CC B0 2F 48 DA BE D0 77 0A 98 BF E8 B1 F3 C2 " +
            "7F 07 38 00 0E 57 FE 9C 14 4B 62 DE B5 13 36 40 " +
            "96 0D 2B 9D C5 3D B1 B8 96 2D 8F 31 2F DB 4A 89 " +
            "19 9C BA E5 DF 14 22 24 58 6D 63 81 20 9B CB D0 " +
            "23 8A A9 D8 71 FA 95 61 EB 8A A3 BE C6 30 4E D8 " +
            "24 65 50 DD A2 56 D0 6B D5 AF F6 E4 65 B0 C2 8E " +
            "77 2B 2D 89 AA 21 C1 5C DD 40 33 3E A1 00 31 BC " +
            "2D 77 F0 27 F6 D4 51 5F B8 E5 CF 5B 54 4E 4B D0 " +
            "A4 7E 3A 18 38 89 9B 43",
            // Round #20
            // After Theta
            "1D 5C 98 D4 A7 7C 59 65 B8 B3 C6 D2 F9 5D 69 B7 " +
            "9F A7 C9 8B 87 CB 7D 2A 6B 69 68 C7 F8 74 06 16 " +
            "A5 79 13 91 4F D3 BD 58 91 CA A3 CD C1 50 E2 76 " +
            "D6 CA 33 98 DE 66 A0 16 51 65 8A 15 08 AE C4 27 " +
            "AC 33 6D D5 91 70 31 C0 02 52 16 38 9C F5 8A 20 " +
            "B8 90 50 54 DB F8 05 75 F4 F9 59 FB A2 AA 56 27 " +
            "89 1A 0D A5 E6 60 27 4D 97 D2 52 CB F6 71 C5 99 " +
            "6C C4 34 62 AB 50 5B C6 9F FA 0B D5 F5 34 30 39 " +
            "C3 38 92 FD 66 43 F5 06 F4 9D 85 86 E5 6D D8 2D " +
            "25 9A 8D 27 7B FC 5F 7B A0 F7 78 63 11 F4 BB 6C " +
            "B0 BC 45 DD 7F 8E 3A B5 3D F2 08 1B B6 B9 51 DB " +
            "32 60 D6 1F D5 89 C7 AA B9 1A 12 A1 8D E4 C4 C0 " +
            "D1 26 B4 9F 4C CD E2 A1",
            // After Rho
            "1D 5C 98 D4 A7 7C 59 65 71 67 8D A5 F3 BB D2 6E " +
            "E7 69 F2 E2 E1 72 9F CA 4F 67 60 B1 96 86 76 8C " +
            "9A EE C5 2A CD 9B 88 7C 1C 0C 25 6E 17 A9 3C DA " +
            "83 E9 6D 06 6A 61 AD 3C 49 54 99 62 05 82 2B F1 " +
            "99 B6 EA 48 B8 18 60 D6 AF 08 22 20 65 81 C3 59 " +
            "C3 85 84 A2 DA C6 2F A8 9D D0 E7 67 ED 8B AA 5A " +
            "28 35 07 3B 69 4A D4 68 E3 8A 33 2F A5 A5 96 ED " +
            "B1 55 A8 2D 63 36 62 1A AA EB 69 60 72 3E F5 17 " +
            "B2 DF 6C A8 DE 60 18 47 EC 16 FA CE 42 C3 F2 36 " +
            "FF 6B AF 44 B3 F1 64 8F 6C A0 F7 78 63 11 F4 BB " +
            "EA D4 C2 F2 16 75 FF 39 F7 C8 23 6C D8 E6 46 6D " +
            "06 CC FA A3 3A F1 58 55 1A 12 A1 8D E4 C4 C0 B9 " +
            "78 68 B4 09 ED 27 53 B3",
            // After Pi
            "1D 5C 98 D4 A7 7C 59 65 83 E9 6D 06 6A 61 AD 3C " +
            "28 35 07 3B 69 4A D4 68 FF 6B AF 44 B3 F1 64 8F " +
            "78 68 B4 09 ED 27 53 B3 4F 67 60 B1 96 86 76 8C " +
            "AF 08 22 20 65 81 C3 59 C3 85 84 A2 DA C6 2F A8 " +
            "B2 DF 6C A8 DE 60 18 47 06 CC FA A3 3A F1 58 55 " +
            "71 67 8D A5 F3 BB D2 6E 49 54 99 62 05 82 2B F1 " +
            "E3 8A 33 2F A5 A5 96 ED 6C A0 F7 78 63 11 F4 BB " +
            "EA D4 C2 F2 16 75 FF 39 9A EE C5 2A CD 9B 88 7C " +
            "1C 0C 25 6E 17 A9 3C DA 9D D0 E7 67 ED 8B AA 5A " +
            "EC 16 FA CE 42 C3 F2 36 1A 12 A1 8D E4 C4 C0 B9 " +
            "E7 69 F2 E2 E1 72 9F CA 99 B6 EA 48 B8 18 60 D6 " +
            "B1 55 A8 2D 63 36 62 1A AA EB 69 60 72 3E F5 17 " +
            "F7 C8 23 6C D8 E6 46 6D",
            // After Chi
            "35 48 9A ED A6 76 09 25 54 A3 C5 42 F8 D0 8D BB " +
            "28 35 17 32 25 4C C7 58 FA 7F A7 90 B1 A9 6C CB " +
            "FA C9 D1 0B A5 26 F7 AB 0F E2 E4 33 0C C0 5A 2C " +
            "9F 52 4A 28 61 A1 D3 1E C7 85 16 A1 FA 57 6F B8 " +
            "FB FC 6C B8 5A 66 3E CF A6 C4 F8 A3 5B F0 D9 04 " +
            "D3 ED AF A8 53 9E 46 62 45 74 5D 32 47 92 4B E3 " +
            "61 DE 33 AD B1 C1 9D ED 7D 83 FA 7D 82 9B F4 FD " +
            "E2 C4 D2 B0 12 75 D6 A8 1B 3E 07 2B 25 99 0A 7C " +
            "7C 0A 3D E6 15 E9 6C FE 8F D0 E6 66 49 8F AA D3 " +
            "6C FA BE EC 4B D8 FA 72 1E 12 81 C9 F6 E4 F4 3B " +
            "C7 28 F2 C7 A2 54 9D C2 93 1C AB 08 A8 10 F5 D3 " +
            "E4 55 AA 21 EB F6 60 72 AA CA B9 E2 53 2E 6C 95 " +
            "EF 5E 2B 64 C0 EE 26 79",
            // After Iota 
            "B4 C8 9A 6D A6 76 09 A5 54 A3 C5 42 F8 D0 8D BB " +
            "28 35 17 32 25 4C C7 58 FA 7F A7 90 B1 A9 6C CB " +
            "FA C9 D1 0B A5 26 F7 AB 0F E2 E4 33 0C C0 5A 2C " +
            "9F 52 4A 28 61 A1 D3 1E C7 85 16 A1 FA 57 6F B8 " +
            "FB FC 6C B8 5A 66 3E CF A6 C4 F8 A3 5B F0 D9 04 " +
            "D3 ED AF A8 53 9E 46 62 45 74 5D 32 47 92 4B E3 " +
            "61 DE 33 AD B1 C1 9D ED 7D 83 FA 7D 82 9B F4 FD " +
            "E2 C4 D2 B0 12 75 D6 A8 1B 3E 07 2B 25 99 0A 7C " +
            "7C 0A 3D E6 15 E9 6C FE 8F D0 E6 66 49 8F AA D3 " +
            "6C FA BE EC 4B D8 FA 72 1E 12 81 C9 F6 E4 F4 3B " +
            "C7 28 F2 C7 A2 54 9D C2 93 1C AB 08 A8 10 F5 D3 " +
            "E4 55 AA 21 EB F6 60 72 AA CA B9 E2 53 2E 6C 95 " +
            "EF 5E 2B 64 C0 EE 26 79",
            // Round #21
            // After Theta
            "39 6B 42 B4 BB EB 3B 37 2B A5 1C AA 1E 72 F0 B7 " +
            "3D C7 3F 32 A4 12 2A 0F 81 9E 7A 83 C8 59 C6 ED " +
            "28 5A AE 64 28 4E C2 1E 82 41 3C EA 11 5D 68 BE " +
            "E0 54 93 C0 87 03 AE 12 D2 77 3E A1 7B 09 82 EF " +
            "80 1D B1 AB 23 96 94 E9 74 57 87 CC D6 98 EC B1 " +
            "5E 4E 77 71 4E 03 74 F0 3A 72 84 DA A1 30 36 EF " +
            "74 2C 1B AD 30 9F 70 BA 06 62 27 6E FB 6B 5E DB " +
            "30 57 AD DF 9F 1D E3 1D 96 9D DF F2 38 04 38 EE " +
            "03 0C E4 0E F3 4B 11 F2 9A 22 CE 66 C8 D1 47 84 " +
            "17 1B 63 FF 32 28 50 54 CC 81 FE A6 7B 8C C1 8E " +
            "4A 8B 2A 1E BF C9 AF 50 EC 1A 72 E0 4E B2 88 DF " +
            "F1 A7 82 21 6A A8 8D 25 D1 2B 64 F1 2A DE C6 B3 " +
            "3D CD 54 0B 4D 86 13 CC",
            // After Rho
            "39 6B 42 B4 BB EB 3B 37 57 4A 39 54 3D E4 E0 6F " +
            "CF F1 8F 0C A9 84 CA 43 9C 65 DC 1E E8 A9 37 88 " +
            "71 12 F6 40 D1 72 25 43 1E D1 85 E6 2B 18 C4 A3 " +
            "09 7C 38 E0 2A 01 4E 35 BB F4 9D 4F E8 5E 82 E0 " +
            "8E D8 D5 11 4B CA 74 C0 C9 1E 4B 77 75 C8 6C 8D " +
            "F7 72 BA 8B 73 1A A0 83 BC EB C8 11 6A 87 C2 D8 " +
            "68 85 F9 84 D3 A5 63 D9 D7 BC B6 0D C4 4E DC F6 " +
            "EF CF 8E F1 0E 98 AB D6 E5 71 08 70 DC 2D 3B BF " +
            "DC 61 7E 29 42 7E 80 81 23 42 4D 11 67 33 E4 E8 " +
            "05 8A EA 62 63 EC 5F 06 8E CC 81 FE A6 7B 8C C1 " +
            "BF 42 29 2D AA 78 FC 26 B3 6B C8 81 3B C9 22 7E " +
            "FE 54 30 44 0D B5 B1 24 2B 64 F1 2A DE C6 B3 D1 " +
            "04 73 4F 33 D5 42 93 E1",
            // After Pi
            "39 6B 42 B4 BB EB 3B 37 09 7C 38 E0 2A 01 4E 35 " +
            "68 85 F9 84 D3 A5 63 D9 05 8A EA 62 63 EC 5F 06 " +
            "04 73 4F 33 D5 42 93 E1 9C 65 DC 1E E8 A9 37 88 " +
            "C9 1E 4B 77 75 C8 6C 8D F7 72 BA 8B 73 1A A0 83 " +
            "DC 61 7E 29 42 7E 80 81 FE 54 30 44 0D B5 B1 24 " +
            "57 4A 39 54 3D E4 E0 6F BB F4 9D 4F E8 5E 82 E0 " +
            "D7 BC B6 0D C4 4E DC F6 8E CC 81 FE A6 7B 8C C1 " +
            "BF 42 29 2D AA 78 FC 26 71 12 F6 40 D1 72 25 43 " +
            "1E D1 85 E6 2B 18 C4 A3 BC EB C8 11 6A 87 C2 D8 " +
            "23 42 4D 11 67 33 E4 E8 2B 64 F1 2A DE C6 B3 D1 " +
            "CF F1 8F 0C A9 84 CA 43 8E D8 D5 11 4B CA 74 C0 " +
            "EF CF 8E F1 0E 98 AB D6 E5 71 08 70 DC 2D 3B BF " +
            "B3 6B C8 81 3B C9 22 7E",
            // After Chi
            "59 EA 83 B0 6A 4F 1A FF 0C 76 3A 82 0A 49 52 33 " +
            "68 F4 FC 95 47 A7 E3 38 3C 82 EA E6 49 45 77 10 " +
            "04 67 77 73 D5 42 D7 E1 AA 05 6C 96 EA BB B7 8A " +
            "C1 1F 0F 57 75 AC 6C 8D D5 66 BA CF 7E 9B 91 A7 " +
            "DC 40 B2 33 A2 76 86 09 BF 4E 33 25 18 F5 F9 21 " +
            "13 42 1B 54 39 E4 BC 79 B3 B4 9C BD CA 6F 82 E1 " +
            "E6 BE 9E 0C CC 4E AC D0 CE C4 91 AE B3 FF 8C 88 " +
            "17 F6 AD 26 6A 62 FE A6 D1 38 BE 51 91 F5 27 1B " +
            "1D D1 80 E6 2E 28 E0 83 B4 CF 78 3B F2 43 D1 C9 " +
            "73 50 4B 51 66 03 E0 EA 25 A5 F0 8C F4 CE 73 71 " +
            "AE F6 85 EC AD 94 41 55 8E E8 D5 11 9B EF 64 E9 " +
            "FD C5 4E 70 2D 58 AB 96 A9 E1 0F 7C 5C 29 F3 BE " +
            "B3 63 98 90 79 83 16 FE",
            // After Iota 
            "D9 6A 83 B0 6A 4F 1A 7F 0C 76 3A 82 0A 49 52 33 " +
            "68 F4 FC 95 47 A7 E3 38 3C 82 EA E6 49 45 77 10 " +
            "04 67 77 73 D5 42 D7 E1 AA 05 6C 96 EA BB B7 8A " +
            "C1 1F 0F 57 75 AC 6C 8D D5 66 BA CF 7E 9B 91 A7 " +
            "DC 40 B2 33 A2 76 86 09 BF 4E 33 25 18 F5 F9 21 " +
            "13 42 1B 54 39 E4 BC 79 B3 B4 9C BD CA 6F 82 E1 " +
            "E6 BE 9E 0C CC 4E AC D0 CE C4 91 AE B3 FF 8C 88 " +
            "17 F6 AD 26 6A 62 FE A6 D1 38 BE 51 91 F5 27 1B " +
            "1D D1 80 E6 2E 28 E0 83 B4 CF 78 3B F2 43 D1 C9 " +
            "73 50 4B 51 66 03 E0 EA 25 A5 F0 8C F4 CE 73 71 " +
            "AE F6 85 EC AD 94 41 55 8E E8 D5 11 9B EF 64 E9 " +
            "FD C5 4E 70 2D 58 AB 96 A9 E1 0F 7C 5C 29 F3 BE " +
            "B3 63 98 90 79 83 16 FE",
            // Round #22
            // After Theta
            "39 BA FB E3 41 4D DF FC 37 D9 29 76 DB EA 6D D0 " +
            "6C 7F 1B A7 83 26 06 87 5B 96 06 22 37 1C B8 D3 " +
            "CF 16 65 BA BC 47 57 A0 4A D5 14 C5 C1 B9 72 09 " +
            "FA B0 1C A3 A4 0F 53 6E D1 ED 5D FD BA 1A 74 18 " +
            "BB 54 5E F7 DC 2F 49 CA 74 3F 21 EC 71 F0 79 60 " +
            "F3 92 63 07 12 E6 79 FA 88 1B 8F 49 1B CC BD 02 " +
            "E2 35 79 3E 08 CF 49 6F A9 D0 7D 6A CD A6 43 4B " +
            "DC 87 BF EF 03 67 7E E7 31 E8 C6 02 BA F7 E2 98 " +
            "26 7E 93 12 FF 8B DF 60 B0 44 9F 09 36 C2 34 76 " +
            "14 44 A7 95 18 5A 2F 29 EE D4 E2 45 9D CB F3 30 " +
            "4E 26 FD BF 86 96 84 D6 B5 47 C6 E5 4A 4C 5B 0A " +
            "F9 4E A9 42 E9 D9 4E 29 CE F5 E3 B8 22 70 3C 7D " +
            "78 12 8A 59 10 86 96 BF",
            // After Rho
            "39 BA FB E3 41 4D DF FC 6F B2 53 EC B6 D5 DB A0 " +
            "DB DF C6 E9 A0 89 C1 21 C3 81 3B BD 65 69 20 72 " +
            "3D BA 02 7D B6 28 D3 E5 1C 9C 2B 97 A0 54 4D 51 " +
            "31 4A FA 30 E5 A6 0F CB 46 74 7B 57 BF AE 06 1D " +
            "2A AF 7B EE 97 24 E5 5D 9F 07 46 F7 13 C2 1E 07 " +
            "9F 97 1C 3B 90 30 CF D3 0A 20 6E 3C 26 6D 30 F7 " +
            "F3 41 78 4E 7A 13 AF C9 4D 87 96 52 A1 FB D4 9A " +
            "F7 81 33 BF 73 EE C3 DF 05 74 EF C5 31 63 D0 8D " +
            "52 E2 7F F1 1B CC C4 6F 1A 3B 58 A2 CF 04 1B 61 " +
            "EB 25 85 82 E8 B4 12 43 30 EE D4 E2 45 9D CB F3 " +
            "12 5A 3B 99 F4 FF 1A 5A D4 1E 19 97 2B 31 6D 29 " +
            "DF 29 55 28 3D DB 29 25 F5 E3 B8 22 70 3C 7D CE " +
            "E5 2F 9E 84 62 16 84 A1",
            // After Pi
            "39 BA FB E3 41 4D DF FC 31 4A FA 30 E5 A6 0F CB " +
            "F3 41 78 4E 7A 13 AF C9 EB 25 85 82 E8 B4 12 43 " +
            "E5 2F 9E 84 62 16 84 A1 C3 81 3B BD 65 69 20 72 " +
            "9F 07 46 F7 13 C2 1E 07 9F 97 1C 3B 90 30 CF D3 " +
            "52 E2 7F F1 1B CC C4 6F DF 29 55 28 3D DB 29 25 " +
            "6F B2 53 EC B6 D5 DB A0 46 74 7B 57 BF AE 06 1D " +
            "4D 87 96 52 A1 FB D4 9A 30 EE D4 E2 45 9D CB F3 " +
            "12 5A 3B 99 F4 FF 1A 5A 3D BA 02 7D B6 28 D3 E5 " +
            "1C 9C 2B 97 A0 54 4D 51 0A 20 6E 3C 26 6D 30 F7 " +
            "1A 3B 58 A2 CF 04 1B 61 F5 E3 B8 22 70 3C 7D CE " +
            "DB DF C6 E9 A0 89 C1 21 2A AF 7B EE 97 24 E5 5D " +
            "F7 81 33 BF 73 EE C3 DF 05 74 EF C5 31 63 D0 8D " +
            "D4 1E 19 97 2B 31 6D 29",
            // After Chi
            "FB BB FB AD 5B 5C 7F FC 39 6E 7F B0 65 02 1F C9 " +
            "F7 4B 62 4A 78 11 2B 69 F3 B5 E4 E1 E9 FD 49 1F " +
            "E5 6F 9E 94 C6 B4 84 A2 C3 11 23 B5 E5 59 E1 A2 " +
            "DF 67 25 37 18 0E 1E 2B 12 9E 1C 33 B4 23 E6 D3 " +
            "52 62 55 64 5B EC C4 3D C3 2F 11 6A 2F 59 37 20 " +
            "66 31 D7 EC B6 84 0B 22 76 1C 3B F7 FB AA 0D 7C " +
            "4F 97 BD 4B 11 99 C4 92 5D 4E 94 86 47 9D 0A 53 " +
            "12 1E 13 8A FD D5 1E 47 3F 9A 46 55 B0 01 E3 43 " +
            "0C 87 3B 15 69 54 46 51 EF E0 CE 3C 16 55 54 79 " +
            "12 23 5A FF 49 04 99 40 F5 E7 91 A0 70 68 71 DE " +
            "0E DF C6 F8 C0 43 C3 A3 2A DB B7 AE 97 25 F5 5D " +
            "27 8B 23 AD 79 FE EE FF 0E B5 29 AD B1 EB 50 8D " +
            "F4 3E 20 91 3C 15 49 75",
            // After Iota 
            "FA BB FB 2D 5B 5C 7F FC 39 6E 7F B0 65 02 1F C9 " +
            "F7 4B 62 4A 78 11 2B 69 F3 B5 E4 E1 E9 FD 49 1F " +
            "E5 6F 9E 94 C6 B4 84 A2 C3 11 23 B5 E5 59 E1 A2 " +
            "DF 67 25 37 18 0E 1E 2B 12 9E 1C 33 B4 23 E6 D3 " +
            "52 62 55 64 5B EC C4 3D C3 2F 11 6A 2F 59 37 20 " +
            "66 31 D7 EC B6 84 0B 22 76 1C 3B F7 FB AA 0D 7C " +
            "4F 97 BD 4B 11 99 C4 92 5D 4E 94 86 47 9D 0A 53 " +
            "12 1E 13 8A FD D5 1E 47 3F 9A 46 55 B0 01 E3 43 " +
            "0C 87 3B 15 69 54 46 51 EF E0 CE 3C 16 55 54 79 " +
            "12 23 5A FF 49 04 99 40 F5 E7 91 A0 70 68 71 DE " +
            "0E DF C6 F8 C0 43 C3 A3 2A DB B7 AE 97 25 F5 5D " +
            "27 8B 23 AD 79 FE EE FF 0E B5 29 AD B1 EB 50 8D " +
            "F4 3E 20 91 3C 15 49 75",
            // Round #23
            // After Theta
            "A2 AF 0C FF F2 B7 95 B7 92 E2 AC 2F 78 C0 CC 08 " +
            "80 1D 23 23 1A 00 08 83 FB 92 91 C8 EB 77 D0 6C " +
            "D8 DC D7 76 3A 51 A1 27 9B 05 D4 67 4C B2 0B E9 " +
            "74 EB F6 A8 05 CC CD EA 65 C8 5D 5A D6 32 C5 39 " +
            "5A 45 20 4D 59 66 5D 4E FE 9C 58 88 D3 BC 12 A5 " +
            "3E 25 20 3E 1F 6F E1 69 DD 90 E8 68 E6 68 DE BD " +
            "38 C1 FC 22 73 88 E7 78 55 69 E1 AF 45 17 93 20 " +
            "2F AD 5A 68 01 30 3B C2 67 8E B1 87 19 EA 09 08 " +
            "A7 0B E8 8A 74 96 95 90 98 B6 8F 55 74 44 77 93 " +
            "1A 04 2F D6 4B 8E 00 33 C8 54 D8 42 8C 8D 54 5B " +
            "56 CB 31 2A 69 A8 29 E8 81 57 64 31 8A E7 26 9C " +
            "50 DD 62 C4 1B EF CD 15 06 92 5C 84 B3 61 C9 FE " +
            "C9 8D 69 73 C0 F0 6C F0",
            // After Rho
            "A2 AF 0C FF F2 B7 95 B7 24 C5 59 5F F0 80 99 11 " +
            "60 C7 C8 88 06 00 C2 20 7E 07 CD B6 2F 19 89 BC " +
            "89 0A 3D C1 E6 BE B6 D3 C6 24 BB 90 BE 59 40 7D " +
            "8F 5A C0 DC AC 4E B7 6E 4E 19 72 97 96 B5 4C 71 " +
            "22 90 A6 2C B3 2E 27 AD 2B 51 EA CF 89 85 38 CD " +
            "F3 29 01 F1 F9 78 0B 4F F7 76 43 A2 A3 99 A3 79 " +
            "17 99 43 3C C7 C3 09 E6 2E 26 41 AA D2 C2 5F 8B " +
            "B4 00 98 1D E1 97 56 2D 0F 33 D4 13 10 CE 1C 63 " +
            "5D 91 CE B2 12 F2 74 01 BB 49 4C DB C7 2A 3A A2 " +
            "11 60 46 83 E0 C5 7A C9 5B C8 54 D8 42 8C 8D 54 " +
            "A6 A0 5B 2D C7 A8 A4 A1 06 5E 91 C5 28 9E 9B 70 " +
            "AA 5B 8C 78 E3 BD B9 02 92 5C 84 B3 61 C9 FE 06 " +
            "1B 7C 72 63 DA 1C 30 3C",
            // After Pi
            "A2 AF 0C FF F2 B7 95 B7 8F 5A C0 DC AC 4E B7 6E " +
            "17 99 43 3C C7 C3 09 E6 11 60 46 83 E0 C5 7A C9 " +
            "1B 7C 72 63 DA 1C 30 3C 7E 07 CD B6 2F 19 89 BC " +
            "2B 51 EA CF 89 85 38 CD F3 29 01 F1 F9 78 0B 4F " +
            "5D 91 CE B2 12 F2 74 01 AA 5B 8C 78 E3 BD B9 02 " +
            "24 C5 59 5F F0 80 99 11 4E 19 72 97 96 B5 4C 71 " +
            "2E 26 41 AA D2 C2 5F 8B 5B C8 54 D8 42 8C 8D 54 " +
            "A6 A0 5B 2D C7 A8 A4 A1 89 0A 3D C1 E6 BE B6 D3 " +
            "C6 24 BB 90 BE 59 40 7D F7 76 43 A2 A3 99 A3 79 " +
            "BB 49 4C DB C7 2A 3A A2 92 5C 84 B3 61 C9 FE 06 " +
            "60 C7 C8 88 06 00 C2 20 22 90 A6 2C B3 2E 27 AD " +
            "B4 00 98 1D E1 97 56 2D 0F 33 D4 13 10 CE 1C 63 " +
            "06 5E 91 C5 28 9E 9B 70",
            // After Chi
            "B2 2E 0F DF B1 36 9D 37 8F 3A C4 5F 8C 4A C5 67 " +
            "1D 85 73 5C DD DB 09 D2 B1 E3 4A 1F C0 66 FF 4A " +
            "16 2C B2 63 D6 54 12 74 AE 2F CC 86 5F 61 8A BE " +
            "27 C1 24 CD 8B 07 4C CD 51 63 01 B9 18 75 82 4D " +
            "09 95 8F 34 1E F2 74 BD AB 0B AE 31 63 39 89 43 " +
            "04 E3 58 77 B0 C2 8A 9B 1F D1 66 C7 96 B9 CC 25 " +
            "8A 06 4A 8F 57 E2 7F 2A 5B 8D 54 8A 72 8C 94 44 " +
            "EC B8 79 AD C1 9D E0 C1 B8 58 7D E3 E7 3E 15 D3 " +
            "CE 2D B7 C9 FA 7B 58 FF F7 62 C3 82 83 58 67 7D " +
            "B2 4B 75 9B 41 1C 3A 73 D4 78 06 A3 79 88 BE 2A " +
            "F4 C7 D0 99 46 91 92 20 29 A3 E2 2E A3 66 2F EF " +
            "B4 4C 99 D9 C9 87 D5 3D 6F B2 9C 1B 16 CE 5C 63 " +
            "04 4E B7 E1 99 B0 BE FD",
            // After Iota 
            "BA AE 0F 5F B1 36 9D B7 8F 3A C4 5F 8C 4A C5 67 " +
            "1D 85 73 5C DD DB 09 D2 B1 E3 4A 1F C0 66 FF 4A " +
            "16 2C B2 63 D6 54 12 74 AE 2F CC 86 5F 61 8A BE " +
            "27 C1 24 CD 8B 07 4C CD 51 63 01 B9 18 75 82 4D " +
            "09 95 8F 34 1E F2 74 BD AB 0B AE 31 63 39 89 43 " +
            "04 E3 58 77 B0 C2 8A 9B 1F D1 66 C7 96 B9 CC 25 " +
            "8A 06 4A 8F 57 E2 7F 2A 5B 8D 54 8A 72 8C 94 44 " +
            "EC B8 79 AD C1 9D E0 C1 B8 58 7D E3 E7 3E 15 D3 " +
            "CE 2D B7 C9 FA 7B 58 FF F7 62 C3 82 83 58 67 7D " +
            "B2 4B 75 9B 41 1C 3A 73 D4 78 06 A3 79 88 BE 2A " +
            "F4 C7 D0 99 46 91 92 20 29 A3 E2 2E A3 66 2F EF " +
            "B4 4C 99 D9 C9 87 D5 3D 6F B2 9C 1B 16 CE 5C 63 " +
            "04 4E B7 E1 99 B0 BE FD"
        };
    }
}
