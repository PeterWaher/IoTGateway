using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Test vectors available in:
    /// https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHAKE128_Msg0.pdf
    /// </summary>
    public partial class SHAKE128_Tests
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
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // Round #0
            // After Theta
            "1F 00 00 00 00 00 00 00 1F 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "1F 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 1F 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "1F 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 80 1F 00 00 00 00 00 00 80 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00",
            // After Rho  
            "1F 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 F8 01 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F8 01 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 F0 03 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 7E 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F0 03 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 7E 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 C0 0F 00 00 00 00 00",
            // After Pi
            "1F 00 00 00 00 00 00 00 00 00 00 00 00 F8 01 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 C0 0F 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 F0 03 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F0 03 00 00 00 00 00 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 00 00 00 F8 01 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 7E 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "7E 00 00 00 00 00 00 00",
            // After Chi  
            "1F 00 00 00 00 00 00 00 00 00 00 00 00 F8 01 00 " +
            "00 C0 0F 00 00 00 00 00 1F 00 00 00 00 00 00 00 " +
            "00 C0 0F 00 00 F8 01 00 00 00 00 00 00 00 00 00 " +
            "00 00 F0 03 00 F0 03 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F0 03 00 00 00 F0 03 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 3F 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 00 7E 00 F8 01 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 7E 00 00 00 00 00 00 " +
            "00 00 00 F8 01 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "7E 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "7E 00 00 00 00 00 00 00",
            // After Iota 
            "1E 00 00 00 00 00 00 00 00 00 00 00 00 F8 01 00 " +
            "00 C0 0F 00 00 00 00 00 1F 00 00 00 00 00 00 00 " +
            "00 C0 0F 00 00 F8 01 00 00 00 00 00 00 00 00 00 " +
            "00 00 F0 03 00 F0 03 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 F0 03 00 00 00 F0 03 00 00 00 00 " +
            "3F 00 00 00 00 00 00 00 00 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 3F 3F 00 00 00 00 00 00 " +
            "00 00 02 00 00 00 00 00 00 7E 00 F8 01 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 7E 00 00 00 00 00 00 " +
            "00 00 00 F8 01 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "7E 00 00 00 80 1F 00 00 00 00 00 00 00 00 00 00 " +
            "7E 00 00 00 00 00 00 00",
            // Round #1
            // After Theta
            "60 BE 1D 04 00 E8 05 00 DD 02 1B F8 81 D8 01 00 " +
            "40 81 FF F3 03 E8 05 00 9D 3E F6 07 80 EF 03 00 " +
            "62 03 0F 08 02 37 02 00 7E BE 1D 04 00 E8 05 00 " +
            "DD 02 EB FB 81 D0 03 00 40 41 F0 F3 03 E8 05 00 " +
            "82 3E F6 07 80 1F 00 00 62 C3 F0 0B 02 CF 03 00 " +
            "41 BE 1D 04 00 E8 05 00 DD 3D 1B F8 81 20 00 00 " +
            "40 41 F2 F3 03 E8 05 00 BD 01 F6 07 80 EF 03 00 " +
            "62 C3 02 08 02 CF 03 00 7E C0 1D FC 01 E8 05 00 " +
            "DD 02 1B F8 81 20 00 00 40 3F F0 F3 03 E8 05 00 " +
            "82 3E F6 FF 81 EF 03 00 62 C3 00 08 02 CF 03 00 " +
            "7E BE 1D 04 80 F7 05 00 DD 02 1B F8 81 20 00 00 " +
            "3E 41 F0 F3 83 F7 05 00 82 3E F6 07 80 EF 03 00 " +
            "1C C3 00 08 02 CF 03 00",
            // After Rho  
            "60 BE 1D 04 00 E8 05 00 BA 05 36 F0 03 B1 03 00 " +
            "50 E0 FF FC 00 7A 01 00 F8 3E 00 D0 E9 63 7F 00 " +
            "B8 11 00 10 1B 78 40 10 00 80 5E 00 E0 E7 DB 41 " +
            "BE 1F 08 3D 00 D0 2D B0 00 50 10 FC FC 00 7A 01 " +
            "1F FB 03 C0 0F 00 00 41 3C 00 20 36 0C BF 20 F0 " +
            "08 F2 ED 20 00 40 2F 00 00 74 F7 6C E0 07 82 00 " +
            "9F 1F 40 2F 00 00 0A 92 DF 07 00 7A 03 EC 0F 00 " +
            "04 81 E7 01 00 B1 61 01 F8 03 D0 0B 00 FC 80 3B " +
            "03 3F 10 04 00 A0 5B 60 02 00 A0 1F F8 F9 01 F4 " +
            "7D 00 40 D0 C7 FE 3F F0 00 62 C3 00 08 02 CF 03 " +
            "17 00 F8 F9 76 10 00 DE 74 0B 6C E0 07 82 00 00 " +
            "27 08 7E 7E F0 BE 00 C0 3E F6 07 80 EF 03 00 82 " +
            "00 00 C7 30 00 82 C0 F3",
            // After Pi
            "60 BE 1D 04 00 E8 05 00 BE 1F 08 3D 00 D0 2D B0 " +
            "9F 1F 40 2F 00 00 0A 92 7D 00 40 D0 C7 FE 3F F0 " +
            "00 00 C7 30 00 82 C0 F3 F8 3E 00 D0 E9 63 7F 00 " +
            "3C 00 20 36 0C BF 20 F0 08 F2 ED 20 00 40 2F 00 " +
            "03 3F 10 04 00 A0 5B 60 27 08 7E 7E F0 BE 00 C0 " +
            "BA 05 36 F0 03 B1 03 00 00 50 10 FC FC 00 7A 01 " +
            "DF 07 00 7A 03 EC 0F 00 00 62 C3 00 08 02 CF 03 " +
            "17 00 F8 F9 76 10 00 DE B8 11 00 10 1B 78 40 10 " +
            "00 80 5E 00 E0 E7 DB 41 00 74 F7 6C E0 07 82 00 " +
            "02 00 A0 1F F8 F9 01 F4 3E F6 07 80 EF 03 00 82 " +
            "50 E0 FF FC 00 7A 01 00 1F FB 03 C0 0F 00 00 41 " +
            "04 81 E7 01 00 B1 61 01 F8 03 D0 0B 00 FC 80 3B " +
            "74 0B 6C E0 07 82 00 00",
            // After Chi  
            "61 BE 5D 06 00 E8 07 02 DE 1F 08 ED C7 2E 18 D0 " +
            "9F 1F C7 0F 00 00 CA 91 1D BE 58 D4 C7 96 3A F0 " +
            "9E 01 C7 09 00 92 E8 43 F8 CC CD D0 E9 23 70 00 " +
            "3F 0D 30 32 0C 1F 70 90 2C F2 83 5A F0 5E 2F 80 " +
            "DB 09 10 84 09 E1 24 60 23 08 5E 58 F4 22 00 30 " +
            "65 02 36 F2 00 5D 06 00 00 30 D3 FC F4 02 BA 02 " +
            "C8 07 38 83 75 FC 0F DC A8 67 C5 00 09 A3 CC 03 " +
            "17 50 F8 F5 8A 10 78 DF B8 65 A1 7C 1B 78 40 10 " +
            "02 80 5E 13 F8 1F DA B5 3C 82 F0 EC E7 05 82 02 " +
            "82 01 A0 0F E8 81 41 E4 3E 76 59 80 0F 84 9B C3 " +
            "50 E0 1B FD 00 CB 60 00 E7 F9 13 CA 0F 4C 80 7B " +
            "00 89 CB E1 07 B3 61 01 F8 E3 43 17 00 84 81 3B " +
            "7B 10 6C E0 08 82 00 41",
            // After Iota 
            "E3 3E 5D 06 00 E8 07 02 DE 1F 08 ED C7 2E 18 D0 " +
            "9F 1F C7 0F 00 00 CA 91 1D BE 58 D4 C7 96 3A F0 " +
            "9E 01 C7 09 00 92 E8 43 F8 CC CD D0 E9 23 70 00 " +
            "3F 0D 30 32 0C 1F 70 90 2C F2 83 5A F0 5E 2F 80 " +
            "DB 09 10 84 09 E1 24 60 23 08 5E 58 F4 22 00 30 " +
            "65 02 36 F2 00 5D 06 00 00 30 D3 FC F4 02 BA 02 " +
            "C8 07 38 83 75 FC 0F DC A8 67 C5 00 09 A3 CC 03 " +
            "17 50 F8 F5 8A 10 78 DF B8 65 A1 7C 1B 78 40 10 " +
            "02 80 5E 13 F8 1F DA B5 3C 82 F0 EC E7 05 82 02 " +
            "82 01 A0 0F E8 81 41 E4 3E 76 59 80 0F 84 9B C3 " +
            "50 E0 1B FD 00 CB 60 00 E7 F9 13 CA 0F 4C 80 7B " +
            "00 89 CB E1 07 B3 61 01 F8 E3 43 17 00 84 81 3B " +
            "7B 10 6C E0 08 82 00 41",
            // Round #2
            // After Theta
            "05 B7 45 37 E8 8F 1C 35 C7 A8 9B FE FE 23 5B 5E " +
            "B3 20 BD 65 96 C2 67 85 84 20 B7 87 51 CE 24 62 " +
            "A6 D8 91 0B CA 08 58 2B 1E 45 D5 E1 01 44 6B 37 " +
            "26 BA A3 21 35 12 33 1E 00 CD F9 30 66 9C 82 94 " +
            "42 97 FF D7 9F B9 3A F2 1B D1 08 5A 3E B8 B0 58 " +
            "83 8B 2E C3 E8 3A 1D 37 19 87 40 EF CD 0F F9 8C " +
            "E4 38 42 E9 E3 3E A2 C8 31 F9 2A 53 9F FB D2 91 " +
            "2F 89 AE F7 40 8A C8 B7 5E EC B9 4D F3 1F 5B 27 " +
            "1B 37 CD 00 C1 12 99 3B 10 BD 8A 86 71 C7 2F 16 " +
            "1B 9F 4F 5C 7E D9 5F 76 06 AF 0F 82 C5 1E 2B AB " +
            "B6 69 03 CC E8 AC 7B 37 FE 4E 80 D9 36 41 C3 F5 " +
            "2C B6 B1 8B 91 71 CC 15 61 7D AC 44 96 DC 9F A9 " +
            "43 C9 3A E2 C2 18 B0 29",
            // After Rho  
            "05 B7 45 37 E8 8F 1C 35 8E 51 37 FD FD 47 B6 BC " +
            "2C 48 6F 99 A5 F0 59 E1 E5 4C 22 46 08 72 7B 18 " +
            "46 C0 5A 31 C5 8E 5C 50 1E 40 B4 76 E3 51 54 1D " +
            "1A 52 23 31 E3 61 A2 3B 25 40 73 3E 8C 19 A7 20 " +
            "CB FF EB CF 5C 1D 79 A1 0B 8B B5 11 8D A0 E5 83 " +
            "19 5C 74 19 46 D7 E9 B8 33 66 1C 02 BD 37 3F E4 " +
            "4A 1F F7 11 45 26 C7 11 F7 A5 23 63 F2 55 A6 3E " +
            "7B 20 45 E4 DB 97 44 D7 9B E6 3F B6 4E BC D8 73 " +
            "19 20 58 22 73 67 E3 A6 17 0B 88 5E 45 C3 B8 E3 " +
            "FB CB 6E E3 F3 89 CB 2F AB 06 AF 0F 82 C5 1E 2B " +
            "EE DD D8 A6 0D 30 A3 B3 FB 3B 01 66 DB 04 0D D7 " +
            "C5 36 76 31 32 8E B9 82 7D AC 44 96 DC 9F A9 61 " +
            "6C CA 50 B2 8E B8 30 06",
            // After Pi
            "05 B7 45 37 E8 8F 1C 35 1A 52 23 31 E3 61 A2 3B " +
            "4A 1F F7 11 45 26 C7 11 FB CB 6E E3 F3 89 CB 2F " +
            "6C CA 50 B2 8E B8 30 06 E5 4C 22 46 08 72 7B 18 " +
            "0B 8B B5 11 8D A0 E5 83 19 5C 74 19 46 D7 E9 B8 " +
            "19 20 58 22 73 67 E3 A6 C5 36 76 31 32 8E B9 82 " +
            "8E 51 37 FD FD 47 B6 BC 25 40 73 3E 8C 19 A7 20 " +
            "F7 A5 23 63 F2 55 A6 3E AB 06 AF 0F 82 C5 1E 2B " +
            "EE DD D8 A6 0D 30 A3 B3 46 C0 5A 31 C5 8E 5C 50 " +
            "1E 40 B4 76 E3 51 54 1D 33 66 1C 02 BD 37 3F E4 " +
            "17 0B 88 5E 45 C3 B8 E3 7D AC 44 96 DC 9F A9 61 " +
            "2C 48 6F 99 A5 F0 59 E1 CB FF EB CF 5C 1D 79 A1 " +
            "7B 20 45 E4 DB 97 44 D7 9B E6 3F B6 4E BC D8 73 " +
            "FB 3B 01 66 DB 04 0D D7",
            // After Chi  
            "45 BA 91 37 EC 89 59 35 AB 92 2B D3 51 E8 AA 15 " +
            "4E 1F E7 01 49 16 F7 11 FA FE 6B E6 93 8E C7 1E " +
            "76 8A 72 B2 8D D8 92 0C F5 18 62 4E 4A 25 73 20 " +
            "0B AB BD 33 BC 80 E7 85 DD 4A 52 08 46 5F F1 B8 " +
            "39 68 58 64 7B 17 A1 BE CF B5 E3 20 B7 0E 3D 01 " +
            "5C F4 37 BC 8F 03 B6 A2 2D 42 FF 32 8C 99 BF 21 " +
            "B3 7C 73 C3 FF 65 07 AE AB 06 88 56 72 82 0A 27 " +
            "CF DD 98 A4 0D 28 A2 B3 67 E6 52 31 D9 A8 77 B0 " +
            "1A 49 34 2A A3 91 D4 1E 5B C2 58 82 25 2B 3E E4 " +
            "15 4B 92 7F 44 C3 EC F3 65 AC E0 D0 FE CE A9 6C " +
            "1C 48 6B B9 26 72 5D B7 4B 39 D1 DD 58 35 E1 81 " +
            "1B 39 45 A4 4A 97 41 53 9F A6 51 2F 6A 4C 88 53 " +
            "38 8C 81 20 83 09 2D D7",
            // After Iota 
            "CF 3A 91 37 EC 89 59 B5 AB 92 2B D3 51 E8 AA 15 " +
            "4E 1F E7 01 49 16 F7 11 FA FE 6B E6 93 8E C7 1E " +
            "76 8A 72 B2 8D D8 92 0C F5 18 62 4E 4A 25 73 20 " +
            "0B AB BD 33 BC 80 E7 85 DD 4A 52 08 46 5F F1 B8 " +
            "39 68 58 64 7B 17 A1 BE CF B5 E3 20 B7 0E 3D 01 " +
            "5C F4 37 BC 8F 03 B6 A2 2D 42 FF 32 8C 99 BF 21 " +
            "B3 7C 73 C3 FF 65 07 AE AB 06 88 56 72 82 0A 27 " +
            "CF DD 98 A4 0D 28 A2 B3 67 E6 52 31 D9 A8 77 B0 " +
            "1A 49 34 2A A3 91 D4 1E 5B C2 58 82 25 2B 3E E4 " +
            "15 4B 92 7F 44 C3 EC F3 65 AC E0 D0 FE CE A9 6C " +
            "1C 48 6B B9 26 72 5D B7 4B 39 D1 DD 58 35 E1 81 " +
            "1B 39 45 A4 4A 97 41 53 9F A6 51 2F 6A 4C 88 53 " +
            "38 8C 81 20 83 09 2D D7",
            // Round #3
            // After Theta
            "5C EF E1 BA 92 1B 5E ED 77 4E 61 47 B8 BC E1 45 " +
            "56 EF 9B 2C BA 6A 21 71 CC A8 61 86 99 6C AB A5 " +
            "AE 07 F0 AD 95 A7 F6 4A 66 CD 12 C3 34 B7 74 78 " +
            "D7 77 F7 A7 55 D4 AC D5 C5 BA 2E 25 B5 23 27 D8 " +
            "0F 3E 52 04 71 F5 CD 05 17 38 61 3F AF 71 59 47 " +
            "CF 21 47 31 F1 91 B1 FA F1 9E B5 A6 65 CD F4 71 " +
            "AB 8C 0F EE 0C 19 D1 CE 9D 50 82 36 78 60 66 9C " +
            "17 50 1A BB 15 57 C6 F5 F4 33 22 BC A7 3A 70 E8 " +
            "C6 95 7E BE 4A C5 9F 4E 43 32 24 AF D6 57 E8 84 " +
            "23 1D 98 1F 4E 21 80 48 BD 21 62 CF E6 B1 CD 2A " +
            "8F 9D 1B 34 58 E0 5A EF 97 E5 9B 49 B1 61 AA D1 " +
            "03 C9 39 89 B9 EB 97 33 A9 F0 5B 4F 60 AE E4 E8 " +
            "E0 01 03 3F 9B 76 49 91",
            // After Rho  
            "5C EF E1 BA 92 1B 5E ED EE 9C C2 8E 70 79 C3 8B " +
            "D5 FB 26 8B AE 5A 48 9C C9 B6 5A CA 8C 1A 66 98 " +
            "3C B5 57 72 3D 80 6F AD 4C 73 4B 87 67 D6 2C 31 " +
            "7F 5A 45 CD 5A 7D 7D 77 76 B1 AE 4B 49 ED C8 09 " +
            "1F 29 82 B8 FA E6 82 07 97 75 74 81 13 F6 F3 1A " +
            "7F 0E 39 8A 89 8F 8C D5 C7 C5 7B D6 9A 96 35 D3 " +
            "70 67 C8 88 76 5E 65 7C C0 CC 38 3B A1 04 6D F0 " +
            "DD 8A 2B E3 FA 0B 28 8D 78 4F 75 E0 D0 E9 67 44 " +
            "CF 57 A9 F8 D3 C9 B8 D2 74 C2 21 19 92 57 EB 2B " +
            "04 10 69 A4 03 F3 C3 29 2A BD 21 62 CF E6 B1 CD " +
            "6B BD 3F 76 6E D0 60 81 5F 96 6F 26 C5 86 A9 46 " +
            "20 39 27 31 77 FD 72 66 F0 5B 4F 60 AE E4 E8 A9 " +
            "52 24 78 C0 C0 CF A6 5D",
            // After Pi
            "5C EF E1 BA 92 1B 5E ED 7F 5A 45 CD 5A 7D 7D 77 " +
            "70 67 C8 88 76 5E 65 7C 04 10 69 A4 03 F3 C3 29 " +
            "52 24 78 C0 C0 CF A6 5D C9 B6 5A CA 8C 1A 66 98 " +
            "97 75 74 81 13 F6 F3 1A 7F 0E 39 8A 89 8F 8C D5 " +
            "CF 57 A9 F8 D3 C9 B8 D2 20 39 27 31 77 FD 72 66 " +
            "EE 9C C2 8E 70 79 C3 8B 76 B1 AE 4B 49 ED C8 09 " +
            "C0 CC 38 3B A1 04 6D F0 2A BD 21 62 CF E6 B1 CD " +
            "6B BD 3F 76 6E D0 60 81 3C B5 57 72 3D 80 6F AD " +
            "4C 73 4B 87 67 D6 2C 31 C7 C5 7B D6 9A 96 35 D3 " +
            "74 C2 21 19 92 57 EB 2B F0 5B 4F 60 AE E4 E8 A9 " +
            "D5 FB 26 8B AE 5A 48 9C 1F 29 82 B8 FA E6 82 07 " +
            "DD 8A 2B E3 FA 0B 28 8D 78 4F 75 E0 D0 E9 67 44 " +
            "5F 96 6F 26 C5 86 A9 46",
            // After Chi  
            "5C CA 69 BA B6 19 5E E5 7B 4A 64 E9 5B DC FF 76 " +
            "22 43 D8 C8 B6 52 41 28 08 DB E8 9E 11 E3 9B 89 " +
            "71 34 7C 85 88 AB 87 4F A1 BC 53 C0 04 13 6A 5D " +
            "17 24 F4 F1 41 B6 C3 18 5F 26 3F 8B AD BB CE F1 " +
            "06 D1 F1 32 5B CB BC 4A 36 78 03 30 64 19 E3 64 " +
            "6E D0 D2 BE D0 79 E6 7B 5C 80 AF 0B 07 0F 58 04 " +
            "81 CC 26 2F 81 14 2D F0 AE BD E1 EA DF CF 32 C7 " +
            "7B 9C 13 37 67 54 68 81 BF 31 67 22 A5 80 7E 6F " +
            "7C 71 4B 8E 67 97 E6 19 47 DC 35 B6 B6 36 35 53 " +
            "78 66 31 0B 83 57 EC 2F B0 19 47 E5 EC B2 E8 B9 " +
            "15 79 0F C8 AE 53 60 14 3F 6C D6 B8 FA 06 C5 47 " +
            "DA 1A 21 E5 FF 0D A0 8F F8 26 75 69 FA B1 27 DC " +
            "55 96 EF 16 95 22 2B 45",
            // After Iota 
            "5C 4A 69 3A B6 19 5E 65 7B 4A 64 E9 5B DC FF 76 " +
            "22 43 D8 C8 B6 52 41 28 08 DB E8 9E 11 E3 9B 89 " +
            "71 34 7C 85 88 AB 87 4F A1 BC 53 C0 04 13 6A 5D " +
            "17 24 F4 F1 41 B6 C3 18 5F 26 3F 8B AD BB CE F1 " +
            "06 D1 F1 32 5B CB BC 4A 36 78 03 30 64 19 E3 64 " +
            "6E D0 D2 BE D0 79 E6 7B 5C 80 AF 0B 07 0F 58 04 " +
            "81 CC 26 2F 81 14 2D F0 AE BD E1 EA DF CF 32 C7 " +
            "7B 9C 13 37 67 54 68 81 BF 31 67 22 A5 80 7E 6F " +
            "7C 71 4B 8E 67 97 E6 19 47 DC 35 B6 B6 36 35 53 " +
            "78 66 31 0B 83 57 EC 2F B0 19 47 E5 EC B2 E8 B9 " +
            "15 79 0F C8 AE 53 60 14 3F 6C D6 B8 FA 06 C5 47 " +
            "DA 1A 21 E5 FF 0D A0 8F F8 26 75 69 FA B1 27 DC " +
            "55 96 EF 16 95 22 2B 45",
            // Round #4
            // After Theta
            "63 F3 E8 00 44 86 1E 5B 81 FA 4E 38 94 F1 5C A4 " +
            "10 5E 03 A4 EE A5 BA F3 DB 0B B5 42 26 C8 32 D1 " +
            "23 1F C0 FC B7 EA C0 C9 9E 05 D2 FA F6 8C 2A 63 " +
            "ED 94 DE 20 8E 9B 60 CA 6D 3B E4 E7 F5 4C 35 2A " +
            "D5 01 AC EE 6C E0 15 12 64 53 BF 49 5B 58 A4 E2 " +
            "51 69 53 84 22 E6 A6 45 A6 30 85 DA C8 22 FB D6 " +
            "B3 D1 FD 43 D9 E3 D6 2B 7D 6D BC 36 E8 E4 9B 9F " +
            "29 B7 AF 4E 58 15 2F 07 80 88 E6 18 57 1F 3E 51 " +
            "86 C1 61 5F A8 BA 45 CB 75 C1 EE DA EE C1 CE 88 " +
            "AB B6 6C D7 B4 7C 45 77 E2 32 FB 9C D3 F3 AF 3F " +
            "2A C0 8E F2 5C CC 20 2A C5 DC FC 69 35 2B 66 95 " +
            "E8 07 FA 89 A7 FA 5B 54 2B F6 28 B5 CD 9A 8E 84 " +
            "07 BD 53 6F AA 63 6C C3",
            // After Rho  
            "63 F3 E8 00 44 86 1E 5B 03 F5 9D 70 28 E3 B9 48 " +
            "84 D7 00 A9 7B A9 EE 3C 82 2C 13 BD BD 50 2B 64 " +
            "55 07 4E 1E F9 00 E6 BF 6F CF A8 32 E6 59 20 AD " +
            "0D E2 B8 09 A6 DC 4E E9 4A DB 0E F9 79 3D 53 8D " +
            "00 56 77 36 F0 0A 89 EA 45 2A 4E 36 F5 9B B4 85 " +
            "8A 4A 9B 22 14 31 37 2D 5B 9B C2 14 6A 23 8B EC " +
            "1F CA 1E B7 5E 99 8D EE C9 37 3F FB DA 78 6D D0 " +
            "27 AC 8A 97 83 94 DB 57 31 AE 3E 7C A2 00 11 CD " +
            "EC 0B 55 B7 68 D9 30 38 67 C4 BA 60 77 6D F7 60 " +
            "AF E8 6E D5 96 ED 9A 96 3F E2 32 FB 9C D3 F3 AF " +
            "83 A8 A8 00 3B CA 73 31 16 73 F3 A7 D5 AC 98 55 " +
            "FD 40 3F F1 54 7F 8B 0A F6 28 B5 CD 9A 8E 84 2B " +
            "DB F0 41 EF D4 9B EA 18",
            // After Pi
            "63 F3 E8 00 44 86 1E 5B 0D E2 B8 09 A6 DC 4E E9 " +
            "1F CA 1E B7 5E 99 8D EE AF E8 6E D5 96 ED 9A 96 " +
            "DB F0 41 EF D4 9B EA 18 82 2C 13 BD BD 50 2B 64 " +
            "45 2A 4E 36 F5 9B B4 85 8A 4A 9B 22 14 31 37 2D " +
            "EC 0B 55 B7 68 D9 30 38 FD 40 3F F1 54 7F 8B 0A " +
            "03 F5 9D 70 28 E3 B9 48 4A DB 0E F9 79 3D 53 8D " +
            "C9 37 3F FB DA 78 6D D0 3F E2 32 FB 9C D3 F3 AF " +
            "83 A8 A8 00 3B CA 73 31 55 07 4E 1E F9 00 E6 BF " +
            "6F CF A8 32 E6 59 20 AD 5B 9B C2 14 6A 23 8B EC " +
            "67 C4 BA 60 77 6D F7 60 F6 28 B5 CD 9A 8E 84 2B " +
            "84 D7 00 A9 7B A9 EE 3C 00 56 77 36 F0 0A 89 EA " +
            "27 AC 8A 97 83 94 DB 57 31 AE 3E 7C A2 00 11 CD " +
            "16 73 F3 A7 D5 AC 98 55",
            // After Chi  
            "71 FB EE B6 1C 87 9F 5D AD C2 D8 49 26 B8 5C F9 " +
            "4F DA 1F 9D 1E 8B ED E6 8F EB C6 D5 96 E9 8E D5 " +
            "D7 F0 51 E6 76 C3 AA B8 08 6C 82 BD BD 70 28 4C " +
            "21 2B 0A A3 9D 53 B4 95 9B 0A B1 62 00 17 BC 2F " +
            "EE 27 55 BB C1 D9 10 5C B8 42 73 F3 14 F4 1F 8B " +
            "82 D1 AC 72 AA A3 95 18 7C 1B 0E F9 7D BE C1 A2 " +
            "49 3F B7 FB F9 70 6D C0 3F B7 27 8B 9C F2 7B E7 " +
            "CB A2 AA 89 6A D6 31 B4 45 17 0C 1A F1 22 6D FF " +
            "4B 8B 90 52 F3 15 54 AD CB B3 C7 99 E2 A1 8B E7 " +
            "66 C3 F0 72 16 6D 95 F4 DC E0 15 ED 9C D7 84 2B " +
            "A3 7F 88 28 78 3D BC 29 10 54 43 5E D0 0A 89 62 " +
            "21 FD 4B 14 D6 38 53 47 B1 2A 3E 74 88 01 77 E5 " +
            "16 73 84 B1 55 AE 99 97",
            // After Iota 
            "FA 7B EE B6 1C 87 9F 5D AD C2 D8 49 26 B8 5C F9 " +
            "4F DA 1F 9D 1E 8B ED E6 8F EB C6 D5 96 E9 8E D5 " +
            "D7 F0 51 E6 76 C3 AA B8 08 6C 82 BD BD 70 28 4C " +
            "21 2B 0A A3 9D 53 B4 95 9B 0A B1 62 00 17 BC 2F " +
            "EE 27 55 BB C1 D9 10 5C B8 42 73 F3 14 F4 1F 8B " +
            "82 D1 AC 72 AA A3 95 18 7C 1B 0E F9 7D BE C1 A2 " +
            "49 3F B7 FB F9 70 6D C0 3F B7 27 8B 9C F2 7B E7 " +
            "CB A2 AA 89 6A D6 31 B4 45 17 0C 1A F1 22 6D FF " +
            "4B 8B 90 52 F3 15 54 AD CB B3 C7 99 E2 A1 8B E7 " +
            "66 C3 F0 72 16 6D 95 F4 DC E0 15 ED 9C D7 84 2B " +
            "A3 7F 88 28 78 3D BC 29 10 54 43 5E D0 0A 89 62 " +
            "21 FD 4B 14 D6 38 53 47 B1 2A 3E 74 88 01 77 E5 " +
            "16 73 84 B1 55 AE 99 97",
            // Round #5
            // After Theta
            "C2 A3 E9 48 17 8A EE 65 D4 2E B7 11 03 18 67 75 " +
            "F6 D2 E5 44 50 9D 16 19 24 4C 60 DC C6 AD 59 0B " +
            "73 3F A2 93 27 FA 4B 78 30 B4 85 43 B6 7D 59 74 " +
            "58 C7 65 FB B8 F3 8F 19 22 02 4B BB 4E 01 47 D0 " +
            "45 80 F3 B2 91 9D C7 82 1C 8D 80 86 45 CD FE 4B " +
            "BA 09 AB 8C A1 AE E4 20 05 F7 61 A1 58 1E FA 2E " +
            "F0 37 4D 22 B7 66 96 3F 94 10 81 82 CC B6 AC 39 " +
            "6F 6D 59 FC 3B EF D0 74 7D CF 0B E4 FA 2F 1C C7 " +
            "32 67 FF 0A D6 B5 6F 21 72 BB 3D 40 AC B7 70 18 " +
            "CD 64 56 7B 46 29 42 2A 78 2F E6 98 CD EE 65 EB " +
            "9B A7 8F D6 73 30 CD 11 69 B8 2C 06 F5 AA B2 EE " +
            "98 F5 B1 CD 98 2E A8 B8 1A 8D 98 7D D8 45 A0 3B " +
            "B2 BC 77 C4 04 97 78 57",
            // After Rho  
            "C2 A3 E9 48 17 8A EE 65 A8 5D 6E 23 06 30 CE EA " +
            "BD 74 39 11 54 A7 45 86 DC 9A B5 40 C2 04 C6 6D " +
            "D1 5F C2 9B FB 11 9D 3C 64 DB 97 45 07 43 5B 38 " +
            "B6 8F 3B FF 98 81 75 5C B4 88 C0 D2 AE 53 C0 11 " +
            "C0 79 D9 C8 CE 63 C1 22 EC BF C4 D1 08 68 58 D4 " +
            "D1 4D 58 65 0C 75 25 07 BB 14 DC 87 85 62 79 E8 " +
            "12 B9 35 B3 FC 81 BF 69 6D 59 73 28 21 02 05 99 " +
            "FE 9D 77 68 BA B7 B6 2C C8 F5 5F 38 8E FB 9E 17 " +
            "5F C1 BA F6 2D 44 E6 EC 38 0C B9 DD 1E 20 D6 5B " +
            "45 48 A5 99 CC 6A CF 28 EB 78 2F E6 98 CD EE 65 " +
            "34 47 6C 9E 3E 5A CF C1 A7 E1 B2 18 D4 AB CA BA " +
            "B3 3E B6 19 D3 05 15 17 8D 98 7D D8 45 A0 3B 1A " +
            "DE 95 2C EF 1D 31 C1 25",
            // After Pi
            "C2 A3 E9 48 17 8A EE 65 B6 8F 3B FF 98 81 75 5C " +
            "12 B9 35 B3 FC 81 BF 69 45 48 A5 99 CC 6A CF 28 " +
            "DE 95 2C EF 1D 31 C1 25 DC 9A B5 40 C2 04 C6 6D " +
            "EC BF C4 D1 08 68 58 D4 D1 4D 58 65 0C 75 25 07 " +
            "5F C1 BA F6 2D 44 E6 EC B3 3E B6 19 D3 05 15 17 " +
            "A8 5D 6E 23 06 30 CE EA B4 88 C0 D2 AE 53 C0 11 " +
            "6D 59 73 28 21 02 05 99 EB 78 2F E6 98 CD EE 65 " +
            "34 47 6C 9E 3E 5A CF C1 D1 5F C2 9B FB 11 9D 3C " +
            "64 DB 97 45 07 43 5B 38 BB 14 DC 87 85 62 79 E8 " +
            "38 0C B9 DD 1E 20 D6 5B 8D 98 7D D8 45 A0 3B 1A " +
            "BD 74 39 11 54 A7 45 86 C0 79 D9 C8 CE 63 C1 22 " +
            "FE 9D 77 68 BA B7 B6 2C C8 F5 5F 38 8E FB 9E 17 " +
            "A7 E1 B2 18 D4 AB CA BA",
            // After Chi  
            "C2 93 ED 48 73 8A 64 44 F3 CF BB F7 98 EB 35 5C " +
            "88 2C 3D D5 ED 90 BF 6C 45 6A 64 99 CE E0 E1 68 " +
            "EA 99 3E 58 95 30 D0 3D CD DA AD 64 C6 11 E3 6E " +
            "E2 3F 66 43 29 68 9A 3C 71 73 5C 6C DE 74 34 14 " +
            "13 41 BB B6 2D 44 24 84 93 1B F6 88 DB 6D 0D 87 " +
            "E1 0C 5D 0B 07 30 CB 62 36 A8 CC 14 36 9E 2A 75 " +
            "79 5E 33 30 07 10 04 19 63 60 2D C7 98 ED EE 4F " +
            "20 C7 EC 4E 96 19 CF D0 4A 5B 8A 19 7B 31 BD FC " +
            "64 D3 B6 1D 1D 43 DD 2B 3E 84 98 87 C4 E2 50 E8 " +
            "68 4B 3B DE A4 31 52 7F A9 18 68 9C 41 E2 79 1A " +
            "83 F0 1F 31 64 33 73 8A C0 19 D1 D8 CA 2B C9 31 " +
            "D9 9D D7 68 EA B7 F6 84 D0 E1 56 39 8E FF 9B 13 " +
            "E7 E8 72 D0 5E EB 4A 9A",
            // After Iota 
            "C3 93 ED C8 73 8A 64 44 F3 CF BB F7 98 EB 35 5C " +
            "88 2C 3D D5 ED 90 BF 6C 45 6A 64 99 CE E0 E1 68 " +
            "EA 99 3E 58 95 30 D0 3D CD DA AD 64 C6 11 E3 6E " +
            "E2 3F 66 43 29 68 9A 3C 71 73 5C 6C DE 74 34 14 " +
            "13 41 BB B6 2D 44 24 84 93 1B F6 88 DB 6D 0D 87 " +
            "E1 0C 5D 0B 07 30 CB 62 36 A8 CC 14 36 9E 2A 75 " +
            "79 5E 33 30 07 10 04 19 63 60 2D C7 98 ED EE 4F " +
            "20 C7 EC 4E 96 19 CF D0 4A 5B 8A 19 7B 31 BD FC " +
            "64 D3 B6 1D 1D 43 DD 2B 3E 84 98 87 C4 E2 50 E8 " +
            "68 4B 3B DE A4 31 52 7F A9 18 68 9C 41 E2 79 1A " +
            "83 F0 1F 31 64 33 73 8A C0 19 D1 D8 CA 2B C9 31 " +
            "D9 9D D7 68 EA B7 F6 84 D0 E1 56 39 8E FF 9B 13 " +
            "E7 E8 72 D0 5E EB 4A 9A",
            // Round #6
            // After Theta
            "D2 03 3E D0 14 2D 67 B1 1B 11 09 B4 01 00 E4 78 " +
            "10 7D 74 AF 1F EB EB FC 0D 18 04 5B 5B DA 8A B1 " +
            "2B A4 B0 48 9F E4 37 8F DC 4A 7E 7C A1 B6 E0 9B " +
            "0A E1 D4 00 B0 83 4B 18 E9 22 15 16 2C 0F 60 84 " +
            "5B 33 DB 74 B8 7E 4F 5D 52 26 78 98 D1 B9 EA 35 " +
            "F0 9C 8E 13 60 97 C8 97 DE 76 7E 57 AF 75 FB 51 " +
            "E1 0F 7A 4A F5 6B 50 89 2B 12 4D 05 0D D7 85 96 " +
            "E1 FA 62 5E 9C CD 28 62 5B CB 59 01 1C 96 BE 09 " +
            "8C 0D 04 5E 84 A8 0C 0F A6 D5 D1 FD 36 99 04 78 " +
            "20 39 5B 1C 31 0B 39 A6 68 25 E6 8C 4B 36 9E A8 " +
            "92 60 CC 29 03 94 70 7F 28 C7 63 9B 53 C0 18 15 " +
            "41 CC 9E 12 18 CC A2 14 98 93 36 FB 1B C5 F0 CA " +
            "26 D5 FC C0 54 3F AD 28",
            // After Rho  
            "D2 03 3E D0 14 2D 67 B1 36 22 12 68 03 00 C8 F1 " +
            "44 1F DD EB C7 FA 3A 3F A5 AD 18 DB 80 41 B0 B5 " +
            "24 BF 79 5C 21 85 45 FA 17 6A 0B BE C9 AD E4 C7 " +
            "0D 00 3B B8 84 A1 10 4E 61 BA 48 85 05 CB 03 18 " +
            "99 6D 3A 5C BF A7 AE AD AB 5E 23 65 82 87 19 9D " +
            "84 E7 74 9C 00 BB 44 BE 47 79 DB F9 5D BD D6 ED " +
            "53 AA 5F 83 4A 0C 7F D0 AE 0B 2D 57 24 9A 0A 1A " +
            "2F CE 66 14 B1 70 7D 31 02 38 2C 7D 13 B6 96 B3 " +
            "C0 8B 10 95 E1 81 B1 81 02 3C D3 EA E8 7E 9B 4C " +
            "21 C7 14 24 67 8B 23 66 A8 68 25 E6 8C 4B 36 9E " +
            "C2 FD 49 82 31 A7 0C 50 A0 1C 8F 6D 4E 01 63 54 " +
            "88 D9 53 02 83 59 94 22 93 36 FB 1B C5 F0 CA 98 " +
            "2B 8A 49 35 3F 30 D5 4F",
            // After Pi
            "D2 03 3E D0 14 2D 67 B1 0D 00 3B B8 84 A1 10 4E " +
            "53 AA 5F 83 4A 0C 7F D0 21 C7 14 24 67 8B 23 66 " +
            "2B 8A 49 35 3F 30 D5 4F A5 AD 18 DB 80 41 B0 B5 " +
            "AB 5E 23 65 82 87 19 9D 84 E7 74 9C 00 BB 44 BE " +
            "C0 8B 10 95 E1 81 B1 81 88 D9 53 02 83 59 94 22 " +
            "36 22 12 68 03 00 C8 F1 61 BA 48 85 05 CB 03 18 " +
            "AE 0B 2D 57 24 9A 0A 1A A8 68 25 E6 8C 4B 36 9E " +
            "C2 FD 49 82 31 A7 0C 50 24 BF 79 5C 21 85 45 FA " +
            "17 6A 0B BE C9 AD E4 C7 47 79 DB F9 5D BD D6 ED " +
            "02 3C D3 EA E8 7E 9B 4C 93 36 FB 1B C5 F0 CA 98 " +
            "44 1F DD EB C7 FA 3A 3F 99 6D 3A 5C BF A7 AE AD " +
            "2F CE 66 14 B1 70 7D 31 02 38 2C 7D 13 B6 96 B3 " +
            "A0 1C 8F 6D 4E 01 63 54",
            // After Chi  
            "80 A9 7A D3 5E 21 08 21 2D 45 3B 9C A1 22 10 68 " +
            "59 A2 16 92 52 3C AB D9 F1 C6 22 E4 67 86 01 D6 " +
            "26 8A 48 1D BF B0 C5 01 A1 0C 4C 43 80 79 F4 97 " +
            "EB 56 23 64 63 87 A8 9C 8C B7 37 9E 02 E3 40 9C " +
            "E5 AF 18 4C E1 81 91 14 82 8B 70 26 81 DF 9D 2A " +
            "B8 23 37 3A 23 10 C0 F3 61 DA 48 25 8D 8A 37 9C " +
            "EC 9E 65 57 15 3E 02 5A 9C 6A 37 8E 8E 4B F6 3F " +
            "83 65 01 07 35 6C 0F 58 64 AE A9 1D 35 95 57 D2 " +
            "17 6E 0B BC 69 EF ED C7 D6 7B F3 E8 58 3D 96 7D " +
            "26 B5 D3 AE C8 7B 9E 2E 80 76 F9 B9 0D D8 6A 9D " +
            "62 9D 99 EB C7 AA 6B 2F 99 5D 32 35 BD 21 2C 2F " +
            "8F CA E5 14 FD 71 1C 75 46 3B 7C FF 92 4C 8E 98 " +
            "39 7C AD 79 76 04 E7 D4",
            // After Iota 
            "01 29 7A 53 5E 21 08 A1 2D 45 3B 9C A1 22 10 68 " +
            "59 A2 16 92 52 3C AB D9 F1 C6 22 E4 67 86 01 D6 " +
            "26 8A 48 1D BF B0 C5 01 A1 0C 4C 43 80 79 F4 97 " +
            "EB 56 23 64 63 87 A8 9C 8C B7 37 9E 02 E3 40 9C " +
            "E5 AF 18 4C E1 81 91 14 82 8B 70 26 81 DF 9D 2A " +
            "B8 23 37 3A 23 10 C0 F3 61 DA 48 25 8D 8A 37 9C " +
            "EC 9E 65 57 15 3E 02 5A 9C 6A 37 8E 8E 4B F6 3F " +
            "83 65 01 07 35 6C 0F 58 64 AE A9 1D 35 95 57 D2 " +
            "17 6E 0B BC 69 EF ED C7 D6 7B F3 E8 58 3D 96 7D " +
            "26 B5 D3 AE C8 7B 9E 2E 80 76 F9 B9 0D D8 6A 9D " +
            "62 9D 99 EB C7 AA 6B 2F 99 5D 32 35 BD 21 2C 2F " +
            "8F CA E5 14 FD 71 1C 75 46 3B 7C FF 92 4C 8E 98 " +
            "39 7C AD 79 76 04 E7 D4",
            // Round #7
            // After Theta
            "CC B3 C4 07 18 3D 4F 9B F3 04 AE 0E 6F 0E D7 7E " +
            "A0 43 3A 29 6D 2B 09 CF AD 21 AA BB 66 95 D7 B4 " +
            "F2 6D 88 D2 F2 25 B3 3A 6C 96 F2 17 C6 65 B3 AD " +
            "35 17 B6 F6 AD AB 6F 8A 75 56 1B 25 3D F4 E2 8A " +
            "B9 48 90 13 E0 92 47 76 56 6C B0 E9 CC 4A EB 11 " +
            "75 B9 89 6E 65 0C 87 C9 BF 9B DD B7 43 A6 F0 8A " +
            "15 7F 49 EC 2A 29 A0 4C C0 8D BF D1 8F 58 20 5D " +
            "57 82 C1 C8 78 F9 79 63 A9 34 17 49 73 89 10 E8 " +
            "C9 2F 9E 2E A7 C3 2A D1 2F 9A DF 53 67 2A 34 6B " +
            "7A 52 5B F1 C9 68 48 4C 54 91 39 76 40 4D 1C A6 " +
            "AF 07 27 BF 81 B6 2C 15 47 1C A7 A7 73 0D EB 39 " +
            "76 2B C9 AF C2 66 BE 63 1A DC F4 A0 93 5F 58 FA " +
            "ED 9B 6D B6 3B 91 91 EF",
            // After Rho  
            "CC B3 C4 07 18 3D 4F 9B E6 09 5C 1D DE 1C AE FD " +
            "E8 90 4E 4A DB 4A C2 33 56 79 4D DB 1A A2 BA 6B " +
            "2F 99 D5 91 6F 43 94 96 61 5C 36 DB CA 66 29 7F " +
            "6B DF BA FA A6 58 73 61 62 9D D5 46 49 0F BD B8 " +
            "24 C8 09 70 C9 23 BB 5C B4 1E 61 C5 06 9B CE AC " +
            "AE CB 4D 74 2B 63 38 4C 2B FE 6E 76 DF 0E 99 C2 " +
            "62 57 49 01 65 AA F8 4B B1 40 BA 80 1B 7F A3 1F " +
            "64 BC FC BC B1 2B C1 60 92 E6 12 21 D0 53 69 2E " +
            "D3 E5 74 58 25 3A F9 C5 9A B5 17 CD EF A9 33 15 " +
            "0D 89 49 4F 6A 2B 3E 19 A6 54 91 39 76 40 4D 1C " +
            "B2 54 BC 1E 9C FC 06 DA 1C 71 9C 9E CE 35 AC E7 " +
            "6E 25 F9 55 D8 CC 77 CC DC F4 A0 93 5F 58 FA 1A " +
            "E4 7B FB 66 9B ED 4E 64",
            // After Pi
            "CC B3 C4 07 18 3D 4F 9B 6B DF BA FA A6 58 73 61 " +
            "62 57 49 01 65 AA F8 4B 0D 89 49 4F 6A 2B 3E 19 " +
            "E4 7B FB 66 9B ED 4E 64 56 79 4D DB 1A A2 BA 6B " +
            "B4 1E 61 C5 06 9B CE AC AE CB 4D 74 2B 63 38 4C " +
            "D3 E5 74 58 25 3A F9 C5 6E 25 F9 55 D8 CC 77 CC " +
            "E6 09 5C 1D DE 1C AE FD 62 9D D5 46 49 0F BD B8 " +
            "B1 40 BA 80 1B 7F A3 1F A6 54 91 39 76 40 4D 1C " +
            "B2 54 BC 1E 9C FC 06 DA 2F 99 D5 91 6F 43 94 96 " +
            "61 5C 36 DB CA 66 29 7F 2B FE 6E 76 DF 0E 99 C2 " +
            "9A B5 17 CD EF A9 33 15 DC F4 A0 93 5F 58 FA 1A " +
            "E8 90 4E 4A DB 4A C2 33 24 C8 09 70 C9 23 BB 5C " +
            "64 BC FC BC B1 2B C1 60 92 E6 12 21 D0 53 69 2E " +
            "1C 71 9C 9E CE 35 AC E7",
            // After Chi  
            "CC B3 85 06 59 9F C7 91 66 57 BA B4 AC 59 75 71 " +
            "82 25 FB 21 F4 6E B8 2F 05 09 4D 4E 6A 3B 3F 82 " +
            "C7 37 C1 9E 3D AD 7E 04 5C B8 41 EB 33 C2 8A 2B " +
            "E5 3A 51 CD 02 83 0F 2D 82 CB C4 71 F3 A7 3E 44 " +
            "C3 BD 70 D2 27 18 71 E6 CE 23 D9 51 DC D5 33 48 " +
            "77 49 76 9D CC 6C AC FA 64 89 D4 7F 2D 0F F1 B8 " +
            "A1 40 96 86 93 C3 A1 DD E2 5D D1 38 34 40 E5 39 " +
            "B2 C0 3D 5C 9D FF 17 DA 25 3B 9D B5 7A 4B 04 16 " +
            "F1 5D 27 52 EA C7 0B 6A 6F BE CE 64 CF 5E 51 C8 " +
            "B9 BC 42 CD CF AA 37 91 9C B0 82 D9 DF 7C D3 73 " +
            "A8 A4 BA C6 EB 42 82 13 B6 8A 0B 71 89 73 93 52 " +
            "68 AD 70 22 BF 0F 45 A1 72 66 50 61 C1 19 2B 3E " +
            "18 39 9D AE CE 14 95 AB",
            // After Iota 
            "C5 33 85 06 59 9F C7 11 66 57 BA B4 AC 59 75 71 " +
            "82 25 FB 21 F4 6E B8 2F 05 09 4D 4E 6A 3B 3F 82 " +
            "C7 37 C1 9E 3D AD 7E 04 5C B8 41 EB 33 C2 8A 2B " +
            "E5 3A 51 CD 02 83 0F 2D 82 CB C4 71 F3 A7 3E 44 " +
            "C3 BD 70 D2 27 18 71 E6 CE 23 D9 51 DC D5 33 48 " +
            "77 49 76 9D CC 6C AC FA 64 89 D4 7F 2D 0F F1 B8 " +
            "A1 40 96 86 93 C3 A1 DD E2 5D D1 38 34 40 E5 39 " +
            "B2 C0 3D 5C 9D FF 17 DA 25 3B 9D B5 7A 4B 04 16 " +
            "F1 5D 27 52 EA C7 0B 6A 6F BE CE 64 CF 5E 51 C8 " +
            "B9 BC 42 CD CF AA 37 91 9C B0 82 D9 DF 7C D3 73 " +
            "A8 A4 BA C6 EB 42 82 13 B6 8A 0B 71 89 73 93 52 " +
            "68 AD 70 22 BF 0F 45 A1 72 66 50 61 C1 19 2B 3E " +
            "18 39 9D AE CE 14 95 AB",
            // Round #8
            // After Theta
            "BB 09 99 A8 F4 B3 FD E7 48 71 00 97 52 D6 74 0A " +
            "FD 71 14 15 FA AF C4 16 DD 0E 2E 16 55 BE 35 C1 " +
            "EF BE 15 91 24 0D 07 7C 22 82 5D 45 9E EE B0 DD " +
            "CB 1C EB EE FC 0C 0E 56 FD 9F 2B 45 FD 66 42 7D " +
            "1B BA 13 8A 18 9D 7B A5 E6 AA 0D 5E C5 75 4A 30 " +
            "09 73 6A 33 61 40 96 0C 4A AF 6E 5C D3 80 F0 C3 " +
            "DE 14 79 B2 9D 02 DD E4 3A 5A B2 60 0B C5 EF 7A " +
            "9A 49 E9 53 84 5F 6E A2 5B 01 81 1B D7 67 3E E0 " +
            "DF 7B 9D 71 14 48 0A 11 10 EA 21 50 C1 9F 2D F1 " +
            "61 BB 21 95 F0 2F 3D D2 B4 39 56 D6 C6 DC AA 0B " +
            "D6 9E A6 68 46 6E B8 E5 98 AC B1 52 77 FC 92 29 " +
            "17 F9 9F 16 B1 CE 39 98 AA 61 33 39 FE 9C 21 7D " +
            "30 B0 49 A1 D7 B4 EC D3",
            // After Rho  
            "BB 09 99 A8 F4 B3 FD E7 90 E2 00 2E A5 AC E9 14 " +
            "7F 1C 45 85 FE 2B B1 45 E5 5B 13 DC ED E0 62 51 " +
            "69 38 E0 7B F7 AD 88 24 E4 E9 0E DB 2D 22 D8 55 " +
            "EE CE CF E0 60 B5 CC B1 5F FF E7 4A 51 BF 99 50 " +
            "DD 09 45 8C CE BD D2 0D A7 04 63 AE DA E0 55 5C " +
            "48 98 53 9B 09 03 B2 64 0F 2B BD BA 71 4D 03 C2 " +
            "93 ED 14 E8 26 F7 A6 C8 8A DF F5 74 B4 64 C1 16 " +
            "29 C2 2F 37 51 CD A4 F4 37 AE CF 7C C0 B7 02 02 " +
            "33 8E 02 49 21 E2 7B AF 96 78 08 F5 10 A8 E0 CF " +
            "A5 47 3A 6C 37 A4 12 FE 0B B4 39 56 D6 C6 DC AA " +
            "E1 96 5B 7B 9A A2 19 B9 60 B2 C6 4A DD F1 4B A6 " +
            "22 FF D3 22 D6 39 07 F3 61 33 39 FE 9C 21 7D AA " +
            "FB 34 0C 6C 52 E8 35 2D",
            // After Pi
            "BB 09 99 A8 F4 B3 FD E7 EE CE CF E0 60 B5 CC B1 " +
            "93 ED 14 E8 26 F7 A6 C8 A5 47 3A 6C 37 A4 12 FE " +
            "FB 34 0C 6C 52 E8 35 2D E5 5B 13 DC ED E0 62 51 " +
            "A7 04 63 AE DA E0 55 5C 48 98 53 9B 09 03 B2 64 " +
            "33 8E 02 49 21 E2 7B AF 22 FF D3 22 D6 39 07 F3 " +
            "90 E2 00 2E A5 AC E9 14 5F FF E7 4A 51 BF 99 50 " +
            "8A DF F5 74 B4 64 C1 16 0B B4 39 56 D6 C6 DC AA " +
            "E1 96 5B 7B 9A A2 19 B9 69 38 E0 7B F7 AD 88 24 " +
            "E4 E9 0E DB 2D 22 D8 55 0F 2B BD BA 71 4D 03 C2 " +
            "96 78 08 F5 10 A8 E0 CF 61 33 39 FE 9C 21 7D AA " +
            "7F 1C 45 85 FE 2B B1 45 DD 09 45 8C CE BD D2 0D " +
            "29 C2 2F 37 51 CD A4 F4 37 AE CF 7C C0 B7 02 02 " +
            "60 B2 C6 4A DD F1 4B A6",
            // After Chi  
            "AA 28 89 A0 F2 F1 DF AF CA CC E5 E4 71 B5 DC 87 " +
            "C9 DD 10 E8 66 BF 83 C9 A5 4E AB EC 93 B7 DA 3C " +
            "BF F2 4A 2C 52 EC 35 3D AD C3 03 CD EC E3 C0 71 " +
            "94 02 63 EE FA 00 1C D7 48 E9 82 B9 DF 1A B6 34 " +
            "F6 8E 02 95 08 22 1B AF 20 FB B3 00 C4 39 12 FF " +
            "10 E2 10 1A 01 EC A9 12 5E DF EF 48 13 3D 85 F8 " +
            "6A DD B7 5D BC 44 C0 07 1B D4 39 52 F3 CA 3C AE " +
            "AE 8B BC 3B CA B1 09 F9 62 3A 51 5B A7 E0 8B A6 " +
            "74 B9 0E 9E 2D 82 38 58 6E 28 8C B0 FD 4C 1E E2 " +
            "9E 70 C8 F4 73 24 60 CB E5 F2 37 7E 94 23 2D FB " +
            "5F DE 6F B6 EF 6B 95 B5 CB 25 85 C4 4E 8F D0 0F " +
            "69 D2 2F 35 4C 8D ED 50 28 A2 CE F9 E2 BD B2 43 " +
            "E0 B3 C6 42 DD 65 09 AE",
            // After Iota 
            "20 28 89 A0 F2 F1 DF AF CA CC E5 E4 71 B5 DC 87 " +
            "C9 DD 10 E8 66 BF 83 C9 A5 4E AB EC 93 B7 DA 3C " +
            "BF F2 4A 2C 52 EC 35 3D AD C3 03 CD EC E3 C0 71 " +
            "94 02 63 EE FA 00 1C D7 48 E9 82 B9 DF 1A B6 34 " +
            "F6 8E 02 95 08 22 1B AF 20 FB B3 00 C4 39 12 FF " +
            "10 E2 10 1A 01 EC A9 12 5E DF EF 48 13 3D 85 F8 " +
            "6A DD B7 5D BC 44 C0 07 1B D4 39 52 F3 CA 3C AE " +
            "AE 8B BC 3B CA B1 09 F9 62 3A 51 5B A7 E0 8B A6 " +
            "74 B9 0E 9E 2D 82 38 58 6E 28 8C B0 FD 4C 1E E2 " +
            "9E 70 C8 F4 73 24 60 CB E5 F2 37 7E 94 23 2D FB " +
            "5F DE 6F B6 EF 6B 95 B5 CB 25 85 C4 4E 8F D0 0F " +
            "69 D2 2F 35 4C 8D ED 50 28 A2 CE F9 E2 BD B2 43 " +
            "E0 B3 C6 42 DD 65 09 AE",
            // Round #9
            // After Theta
            "6B F0 F8 BA 11 D8 8E 3E B2 06 4D 6D 4F 81 78 C8 " +
            "8B DD DF BD 6F B7 71 5C 21 DB 44 32 0D D3 C8 A8 " +
            "00 EF 95 3F 04 C0 4A 37 E6 1B 72 D7 0F CA 91 E0 " +
            "EC C8 CB 67 C4 34 B8 98 0A E9 4D EC D6 12 44 A1 " +
            "72 1B ED 4B 96 46 09 3B 9F E6 6C 13 92 15 6D F5 " +
            "5B 3A 61 00 E2 C5 F8 83 26 15 47 C1 2D 09 21 B7 " +
            "28 DD 78 08 B5 4C 32 92 9F 41 D6 8C 6D AE 2E 3A " +
            "11 96 63 28 9C 9D 76 F3 29 E2 20 41 44 C9 DA 37 " +
            "0C 73 A6 17 13 B6 9C 17 2C 28 43 E5 F4 44 EC 77 " +
            "1A E5 27 2A ED 40 72 5F 5A EF E8 6D C2 0F 52 F1 " +
            "14 06 1E AC 0C 42 C4 24 B3 EF 2D 4D 70 BB 74 40 " +
            "2B D2 E0 60 45 85 1F C5 AC 37 21 27 7C D9 A0 D7 " +
            "5F AE 19 51 8B 49 76 A4",
            // After Rho  
            "6B F0 F8 BA 11 D8 8E 3E 65 0D 9A DA 9E 02 F1 90 " +
            "62 F7 77 EF DB 6D 1C D7 30 8D 8C 1A B2 4D 24 D3 " +
            "00 56 BA 01 78 AF FC 21 FD A0 1C 09 6E BE 21 77 " +
            "7C 46 4C 83 8B C9 8E BC A8 42 7A 13 BB B5 04 51 " +
            "8D F6 25 4B A3 84 1D B9 D1 56 FF 69 CE 36 21 59 " +
            "DC D2 09 03 10 2F C6 1F DC 9A 54 1C 05 B7 24 84 " +
            "43 A8 65 92 91 44 E9 C6 5C 5D 74 3E 83 AC 19 DB " +
            "14 CE 4E BB F9 08 CB 31 82 88 92 B5 6F 52 C4 41 " +
            "F4 62 C2 96 F3 82 61 CE F6 3B 16 94 A1 72 7A 22 " +
            "48 EE 4B A3 FC 44 A5 1D F1 5A EF E8 6D C2 0F 52 " +
            "11 93 50 18 78 B0 32 08 CD BE B7 34 C1 ED D2 01 " +
            "45 1A 1C AC A8 F0 A3 78 37 21 27 7C D9 A0 D7 AC " +
            "1D E9 97 6B 46 D4 62 92",
            // After Pi
            "6B F0 F8 BA 11 D8 8E 3E 7C 46 4C 83 8B C9 8E BC " +
            "43 A8 65 92 91 44 E9 C6 48 EE 4B A3 FC 44 A5 1D " +
            "1D E9 97 6B 46 D4 62 92 30 8D 8C 1A B2 4D 24 D3 " +
            "D1 56 FF 69 CE 36 21 59 DC D2 09 03 10 2F C6 1F " +
            "F4 62 C2 96 F3 82 61 CE 45 1A 1C AC A8 F0 A3 78 " +
            "65 0D 9A DA 9E 02 F1 90 A8 42 7A 13 BB B5 04 51 " +
            "5C 5D 74 3E 83 AC 19 DB F1 5A EF E8 6D C2 0F 52 " +
            "11 93 50 18 78 B0 32 08 00 56 BA 01 78 AF FC 21 " +
            "FD A0 1C 09 6E BE 21 77 DC 9A 54 1C 05 B7 24 84 " +
            "F6 3B 16 94 A1 72 7A 22 37 21 27 7C D9 A0 D7 AC " +
            "62 F7 77 EF DB 6D 1C D7 8D F6 25 4B A3 84 1D B9 " +
            "14 CE 4E BB F9 08 CB 31 82 88 92 B5 6F 52 C4 41 " +
            "CD BE B7 34 C1 ED D2 01",
            // After Chi  
            "68 58 D9 AA 01 DC EF 7C 74 00 46 A2 E7 C9 8A A5 " +
            "56 A9 F1 DA 93 D4 AB 44 2A FE 23 33 ED 4C 29 31 " +
            "09 EF 93 6A CC D5 62 12 3C 0D 8C 18 A2 44 E2 D5 " +
            "F1 76 3D FD 2D B6 00 99 DD CA 15 2B 18 5F 44 2F " +
            "C4 E7 42 84 E1 8F 65 4D 84 48 6F CD E4 C2 A2 70 " +
            "31 10 9E F6 9E 0A E8 1A 09 40 F1 D3 D7 F7 02 51 " +
            "5C DC 64 2E 93 9C 29 D3 95 56 65 2A EB C0 CE C2 " +
            "99 D1 30 19 59 05 36 49 00 4C FA 15 79 AE F8 A1 " +
            "DF 81 1E 89 CE FE 7B 55 DD 9A 75 74 5D 37 A1 08 " +
            "F6 6D 8E 95 81 7D 52 23 CA 81 23 74 DF B0 D6 FA " +
            "72 FF 3D 5F 83 65 DE D7 0F F6 B5 4F A5 D6 19 F9 " +
            "59 F8 6B BB 79 A5 D9 31 A0 C9 D2 7E 75 52 C8 97 " +
            "40 BE B7 34 E1 6D D3 29",
            // After Iota 
            "E0 58 D9 AA 01 DC EF 7C 74 00 46 A2 E7 C9 8A A5 " +
            "56 A9 F1 DA 93 D4 AB 44 2A FE 23 33 ED 4C 29 31 " +
            "09 EF 93 6A CC D5 62 12 3C 0D 8C 18 A2 44 E2 D5 " +
            "F1 76 3D FD 2D B6 00 99 DD CA 15 2B 18 5F 44 2F " +
            "C4 E7 42 84 E1 8F 65 4D 84 48 6F CD E4 C2 A2 70 " +
            "31 10 9E F6 9E 0A E8 1A 09 40 F1 D3 D7 F7 02 51 " +
            "5C DC 64 2E 93 9C 29 D3 95 56 65 2A EB C0 CE C2 " +
            "99 D1 30 19 59 05 36 49 00 4C FA 15 79 AE F8 A1 " +
            "DF 81 1E 89 CE FE 7B 55 DD 9A 75 74 5D 37 A1 08 " +
            "F6 6D 8E 95 81 7D 52 23 CA 81 23 74 DF B0 D6 FA " +
            "72 FF 3D 5F 83 65 DE D7 0F F6 B5 4F A5 D6 19 F9 " +
            "59 F8 6B BB 79 A5 D9 31 A0 C9 D2 7E 75 52 C8 97 " +
            "40 BE B7 34 E1 6D D3 29",
            // Round #10  
            // After Theta
            "C7 93 C3 C0 A2 53 C9 07 4C 4C 77 8D 58 9A 34 63 " +
            "50 3E 61 7C C3 2C 71 91 44 B0 0D DF 4E 57 70 41 " +
            "1B E9 D2 00 51 4A FC 93 1B C6 96 72 01 CB C4 AE " +
            "C9 3A 0C D2 92 E5 BE 5F DB 5D 85 8D 48 A7 9E FA " +
            "AA A9 6C 68 42 94 3C 3D 96 4E 2E A7 79 5D 3C F1 " +
            "16 DB 84 9C 3D 85 CE 61 31 0C C0 FC 68 A4 BC 97 " +
            "5A 4B F4 88 C3 64 F3 06 FB 18 4B C6 48 DB 97 B2 " +
            "8B D7 71 73 C4 9A A8 C8 27 87 E0 7F DA 21 DE DA " +
            "E7 CD 2F A6 71 AD C5 93 DB 0D E5 D2 0D CF 7B DD " +
            "98 23 A0 79 22 66 0B 53 D8 87 62 1E 42 2F 48 7B " +
            "55 34 27 35 20 EA F8 AC 37 BA 84 60 1A 85 A7 3F " +
            "5F 6F FB 1D 29 5D 03 E4 CE 87 FC 92 D6 49 91 E7 " +
            "52 B8 F6 5E 7C F2 4D A8",
            // After Rho  
            "C7 93 C3 C0 A2 53 C9 07 98 98 EE 1A B1 34 69 C6 " +
            "94 4F 18 DF 30 4B 5C 24 74 05 17 44 04 DB F0 ED " +
            "52 E2 9F DC 48 97 06 88 17 B0 4C EC BA 61 6C 29 " +
            "20 2D 59 EE FB 95 AC C3 FE 76 57 61 23 D2 A9 A7 " +
            "54 36 34 21 4A 9E 1E D5 C5 13 6F E9 E4 72 9A D7 " +
            "B3 D8 26 E4 EC 29 74 0E 5E C6 30 00 F3 A3 91 F2 " +
            "47 1C 26 9B 37 D0 5A A2 B6 2F 65 F7 31 96 8C 91 " +
            "39 62 4D 54 E4 C5 EB B8 FF B4 43 BC B5 4F 0E C1 " +
            "C5 34 AE B5 78 F2 BC F9 BD EE ED 86 72 E9 86 E7 " +
            "6C 61 0A 73 04 34 4F C4 7B D8 87 62 1E 42 2F 48 " +
            "E3 B3 56 D1 9C D4 80 A8 DC E8 12 82 69 14 9E FE " +
            "EB 6D BF 23 A5 6B 80 FC 87 FC 92 D6 49 91 E7 CE " +
            "13 AA 14 AE BD 17 9F 7C",
            // After Pi
            "C7 93 C3 C0 A2 53 C9 07 20 2D 59 EE FB 95 AC C3 " +
            "47 1C 26 9B 37 D0 5A A2 6C 61 0A 73 04 34 4F C4 " +
            "13 AA 14 AE BD 17 9F 7C 74 05 17 44 04 DB F0 ED " +
            "C5 13 6F E9 E4 72 9A D7 B3 D8 26 E4 EC 29 74 0E " +
            "C5 34 AE B5 78 F2 BC F9 EB 6D BF 23 A5 6B 80 FC " +
            "98 98 EE 1A B1 34 69 C6 FE 76 57 61 23 D2 A9 A7 " +
            "B6 2F 65 F7 31 96 8C 91 7B D8 87 62 1E 42 2F 48 " +
            "E3 B3 56 D1 9C D4 80 A8 52 E2 9F DC 48 97 06 88 " +
            "17 B0 4C EC BA 61 6C 29 5E C6 30 00 F3 A3 91 F2 " +
            "BD EE ED 86 72 E9 86 E7 87 FC 92 D6 49 91 E7 CE " +
            "94 4F 18 DF 30 4B 5C 24 54 36 34 21 4A 9E 1E D5 " +
            "39 62 4D 54 E4 C5 EB B8 FF B4 43 BC B5 4F 0E C1 " +
            "DC E8 12 82 69 14 9E FE",
            // After Chi  
            "80 83 E5 D1 A6 13 9B 27 08 4C 51 8E FB B1 A9 87 " +
            "54 96 32 17 8E D3 CA 9A A8 70 C9 33 06 74 0F C7 " +
            "33 86 0C 80 E4 93 BB BC 46 CD 17 40 0C D2 94 E5 " +
            "81 37 E7 F8 F4 A0 12 26 99 91 37 E6 69 20 74 0A " +
            "D1 34 AE F1 78 62 CC F8 6A 7F D7 8A 45 4B 8A EE " +
            "98 91 CE 8C A1 30 6D D6 B7 A6 D5 61 2D 92 8A EF " +
            "36 0C 35 66 B1 02 0C 31 63 D0 2F 68 3F 62 46 0E " +
            "85 D5 47 B0 9E 16 00 89 1A A4 AF DC 09 15 97 5A " +
            "B6 98 81 6A BA 29 6A 2C 5C D6 22 50 FA B3 F0 FA " +
            "ED EC E0 8E 72 EF 86 E7 82 EC D2 F6 FB F1 8F EF " +
            "BD 0F 51 8B 94 0A BD 0C 92 A2 36 89 5B 94 1A 94 " +
            "39 2A 5D 56 AC D5 7B 86 FF B3 4B E1 A5 04 4E C1 " +
            "9C D8 36 A2 23 80 9C 2F",
            // After Iota 
            "89 03 E5 51 A6 13 9B 27 08 4C 51 8E FB B1 A9 87 " +
            "54 96 32 17 8E D3 CA 9A A8 70 C9 33 06 74 0F C7 " +
            "33 86 0C 80 E4 93 BB BC 46 CD 17 40 0C D2 94 E5 " +
            "81 37 E7 F8 F4 A0 12 26 99 91 37 E6 69 20 74 0A " +
            "D1 34 AE F1 78 62 CC F8 6A 7F D7 8A 45 4B 8A EE " +
            "98 91 CE 8C A1 30 6D D6 B7 A6 D5 61 2D 92 8A EF " +
            "36 0C 35 66 B1 02 0C 31 63 D0 2F 68 3F 62 46 0E " +
            "85 D5 47 B0 9E 16 00 89 1A A4 AF DC 09 15 97 5A " +
            "B6 98 81 6A BA 29 6A 2C 5C D6 22 50 FA B3 F0 FA " +
            "ED EC E0 8E 72 EF 86 E7 82 EC D2 F6 FB F1 8F EF " +
            "BD 0F 51 8B 94 0A BD 0C 92 A2 36 89 5B 94 1A 94 " +
            "39 2A 5D 56 AC D5 7B 86 FF B3 4B E1 A5 04 4E C1 " +
            "9C D8 36 A2 23 80 9C 2F",
            // Round #11  
            // After Theta
            "7E D5 34 56 C6 D1 3B D0 C5 57 0C 66 6C 71 92 7F " +
            "5E E7 21 68 60 D2 10 42 B2 B6 76 7E C9 9C 73 2C " +
            "DB A4 6A D0 5F D1 67 2F B1 1B C6 47 6C 10 34 12 " +
            "4C 2C BA 10 63 60 29 DE 93 E0 24 99 87 21 AE D2 " +
            "CB F2 11 BC B7 8A B0 13 82 5D B1 DA FE 09 56 7D " +
            "6F 47 1F 8B C1 F2 CD 21 7A BD 88 89 BA 52 B1 17 " +
            "3C 7D 26 19 5F 03 D6 E9 79 16 90 25 F0 8A 3A E5 " +
            "6D F7 21 E0 25 54 DC 1A ED 72 7E DB 69 D7 37 AD " +
            "7B 83 DC 82 2D E9 51 D4 56 A7 31 2F 14 B2 2A 22 " +
            "F7 2A 5F C3 BD 07 FA 0C 6A CE B4 A6 40 B3 53 7C " +
            "4A D9 80 8C F4 C8 1D FB 5F B9 6B 61 CC 54 21 6C " +
            "33 5B 4E 29 42 D4 A1 5E E5 75 F4 AC 6A EC 32 2A " +
            "74 FA 50 F2 98 C2 40 BC",
            // After Rho  
            "7E D5 34 56 C6 D1 3B D0 8A AF 18 CC D8 E2 24 FF " +
            "D7 79 08 1A 98 34 84 90 CC 39 C7 22 6B 6B E7 97 " +
            "8A 3E 7B D9 26 55 83 FE C4 06 41 23 11 BB 61 7C " +
            "0B 31 06 96 E2 CD C4 A2 F4 24 38 49 E6 61 88 AB " +
            "F9 08 DE 5B 45 D8 89 65 60 D5 27 D8 15 AB ED 9F " +
            "79 3B FA 58 0C 96 6F 0E 5E E8 F5 22 26 EA 4A C5 " +
            "C9 F8 1A B0 4E E7 E9 33 15 75 CA F3 2C 20 4B E0 " +
            "F0 12 2A 6E 8D B6 FB 10 B6 D3 AE 6F 5A DB E5 FC " +
            "5B B0 25 3D 8A 7A 6F 90 15 11 AB D3 98 17 0A 59 " +
            "40 9F E1 5E E5 6B B8 F7 7C 6A CE B4 A6 40 B3 53 " +
            "77 EC 2B 65 03 32 D2 23 7D E5 AE 85 31 53 85 B0 " +
            "66 CB 29 45 88 3A D4 6B 75 F4 AC 6A EC 32 2A E5 " +
            "10 2F 9D 3E 94 3C A6 30",
            // After Pi
            "7E D5 34 56 C6 D1 3B D0 0B 31 06 96 E2 CD C4 A2 " +
            "C9 F8 1A B0 4E E7 E9 33 40 9F E1 5E E5 6B B8 F7 " +
            "10 2F 9D 3E 94 3C A6 30 CC 39 C7 22 6B 6B E7 97 " +
            "60 D5 27 D8 15 AB ED 9F 79 3B FA 58 0C 96 6F 0E " +
            "5B B0 25 3D 8A 7A 6F 90 66 CB 29 45 88 3A D4 6B " +
            "8A AF 18 CC D8 E2 24 FF F4 24 38 49 E6 61 88 AB " +
            "15 75 CA F3 2C 20 4B E0 7C 6A CE B4 A6 40 B3 53 " +
            "77 EC 2B 65 03 32 D2 23 8A 3E 7B D9 26 55 83 FE " +
            "C4 06 41 23 11 BB 61 7C 5E E8 F5 22 26 EA 4A C5 " +
            "15 11 AB D3 98 17 0A 59 75 F4 AC 6A EC 32 2A E5 " +
            "D7 79 08 1A 98 34 84 90 F9 08 DE 5B 45 D8 89 65 " +
            "F0 12 2A 6E 8D B6 FB 10 B6 D3 AE 6F 5A DB E5 FC " +
            "7D E5 AE 85 31 53 85 B0",
            // After Chi  
            "BE 1D 2C 76 CA F3 12 C1 0B 36 E7 D8 43 C5 D4 66 " +
            "D9 D8 06 90 5E F3 EF 33 2E 4F C1 1E A7 AA A1 37 " +
            "11 0F 9F BE B4 30 62 12 D5 13 1F 22 63 7F E5 97 " +
            "62 55 22 FD 97 C3 ED 0F 5D 70 F2 18 0C 96 FF 65 " +
            "D3 80 E3 1F E9 3B 4C 04 46 0F 09 9D 9C BA DC 63 " +
            "8B FE DA 7E D0 E2 67 BF 9C 2E 3C 4D 64 21 38 B8 " +
            "16 F1 EB B2 2D 12 0B C0 F4 69 DE 3C 7E 80 97 8F " +
            "03 EC 0B 64 25 33 5A 23 90 D6 CF D9 00 15 89 7F " +
            "C5 17 4B F2 89 AE 61 64 3E 0C F1 0A 42 CA 6A 61 " +
            "9F 1B F8 42 9A 52 8B 43 31 F4 AC 48 FD 98 4A E5 " +
            "D7 6B 28 3E 10 12 F6 80 FF C9 5A 5A 17 91 8D 89 " +
            "B9 36 2A EE AC B6 FB 10 34 CB AE 75 D2 FF E5 FC " +
            "55 E5 78 C4 74 9B 8C D5",
            // After Iota 
            "B4 1D 2C F6 CA F3 12 C1 0B 36 E7 D8 43 C5 D4 66 " +
            "D9 D8 06 90 5E F3 EF 33 2E 4F C1 1E A7 AA A1 37 " +
            "11 0F 9F BE B4 30 62 12 D5 13 1F 22 63 7F E5 97 " +
            "62 55 22 FD 97 C3 ED 0F 5D 70 F2 18 0C 96 FF 65 " +
            "D3 80 E3 1F E9 3B 4C 04 46 0F 09 9D 9C BA DC 63 " +
            "8B FE DA 7E D0 E2 67 BF 9C 2E 3C 4D 64 21 38 B8 " +
            "16 F1 EB B2 2D 12 0B C0 F4 69 DE 3C 7E 80 97 8F " +
            "03 EC 0B 64 25 33 5A 23 90 D6 CF D9 00 15 89 7F " +
            "C5 17 4B F2 89 AE 61 64 3E 0C F1 0A 42 CA 6A 61 " +
            "9F 1B F8 42 9A 52 8B 43 31 F4 AC 48 FD 98 4A E5 " +
            "D7 6B 28 3E 10 12 F6 80 FF C9 5A 5A 17 91 8D 89 " +
            "B9 36 2A EE AC B6 FB 10 34 CB AE 75 D2 FF E5 FC " +
            "55 E5 78 C4 74 9B 8C D5",
            // Round #12  
            // After Theta
            "1A C7 B4 BC 13 79 EA DA 8D BD 61 28 09 BB 2F BF " +
            "52 A6 BA 45 80 93 2B 09 5B D6 96 56 3F D4 6E 14 " +
            "E9 E2 29 2E 1E 5E A8 3C 7B C9 87 68 BA F5 1D 8C " +
            "E4 DE A4 0D DD BD 16 D6 D6 0E 4E CD D2 F6 3B 5F " +
            "A6 19 B4 57 71 45 83 27 BE E2 BF 0D 36 D4 16 4D " +
            "25 24 42 34 09 68 9F A4 1A A5 BA BD 2E 5F C3 61 " +
            "9D 8F 57 67 F3 72 CF FA 81 F0 89 74 E6 FE 58 AC " +
            "FB 01 BD F4 8F 5D 90 0D 3E 0C 57 93 D9 9F 71 64 " +
            "43 9C CD 02 C3 D0 9A BD B5 72 4D DF 9C AA AE 5B " +
            "EA 82 AF 0A 02 2C 44 60 C9 19 1A D8 57 F6 80 CB " +
            "79 B1 B0 74 C9 98 0E 9B 79 42 DC AA 5D EF 76 50 " +
            "32 48 96 3B 72 D6 3F 2A 41 52 F9 3D 4A 81 2A DF " +
            "AD 08 CE 54 DE F5 46 FB",
            // After Rho  
            "1A C7 B4 BC 13 79 EA DA 1B 7B C3 50 12 76 5F 7E " +
            "94 A9 6E 11 E0 E4 4A 82 43 ED 46 B1 65 6D 69 F5 " +
            "F0 42 E5 49 17 4F 71 F1 A6 5B DF C1 B8 97 7C 88 " +
            "DA D0 DD 6B 61 4D EE 4D 97 B5 83 53 B3 B4 FD CE " +
            "0C DA AB B8 A2 C1 13 D3 6D D1 E4 2B FE DB 60 43 " +
            "2D 21 11 A2 49 40 FB 24 87 69 94 EA F6 BA 7C 0D " +
            "3A 9B 97 7B D6 EF 7C BC FD B1 58 03 E1 13 E9 CC " +
            "FA C7 2E C8 86 FD 80 5E 26 B3 3F E3 C8 7C 18 AE " +
            "59 60 18 5A B3 77 88 B3 D7 AD 5A B9 A6 6F 4E 55 " +
            "85 08 4C 5D F0 55 41 80 CB C9 19 1A D8 57 F6 80 " +
            "3A 6C E6 C5 C2 D2 25 63 E5 09 71 AB 76 BD DB 41 " +
            "06 C9 72 47 CE FA 47 45 52 F9 3D 4A 81 2A DF 41 " +
            "D1 7E 2B 82 33 95 77 BD",
            // After Pi
            "1A C7 B4 BC 13 79 EA DA DA D0 DD 6B 61 4D EE 4D " +
            "3A 9B 97 7B D6 EF 7C BC 85 08 4C 5D F0 55 41 80 " +
            "D1 7E 2B 82 33 95 77 BD 43 ED 46 B1 65 6D 69 F5 " +
            "6D D1 E4 2B FE DB 60 43 2D 21 11 A2 49 40 FB 24 " +
            "59 60 18 5A B3 77 88 B3 06 C9 72 47 CE FA 47 45 " +
            "1B 7B C3 50 12 76 5F 7E 97 B5 83 53 B3 B4 FD CE " +
            "FD B1 58 03 E1 13 E9 CC CB C9 19 1A D8 57 F6 80 " +
            "3A 6C E6 C5 C2 D2 25 63 F0 42 E5 49 17 4F 71 F1 " +
            "A6 5B DF C1 B8 97 7C 88 87 69 94 EA F6 BA 7C 0D " +
            "D7 AD 5A B9 A6 6F 4E 55 52 F9 3D 4A 81 2A DF 41 " +
            "94 A9 6E 11 E0 E4 4A 82 0C DA AB B8 A2 C1 13 D3 " +
            "FA C7 2E C8 86 FD 80 5E 26 B3 3F E3 C8 7C 18 AE " +
            "E5 09 71 AB 76 BD DB 41",
            // After Chi  
            "3A CC B6 AC 85 DB FA 6A 5F D0 95 6F 41 5D EF 4D " +
            "6A ED B4 F9 D5 6F 4A 81 8F 89 D8 61 F0 3D C9 C2 " +
            "11 6E 62 C1 53 91 73 B8 43 CD 57 31 64 6D F2 D1 " +
            "3D 91 EC 73 4C EC 60 D0 2B A8 73 A7 05 C8 BC 60 " +
            "18 44 1C EA 92 72 A0 03 2A D9 D2 4D 54 68 47 47 " +
            "73 7B 9B 50 52 75 5F 7E 95 FD 82 4B AB F0 EB CE " +
            "CD 95 BE C6 E3 93 E8 AF CA DA 18 0A C8 73 AC 9C " +
            "BE E8 E6 C6 63 52 85 E3 F1 62 E5 63 51 67 71 F4 " +
            "F6 DF 95 D0 B8 D2 7E D8 87 39 B1 A8 F7 BA ED 0D " +
            "77 AF 9A B8 B0 2A 6E E5 54 E0 27 CA 29 BA D3 49 " +
            "66 AC 6A 51 E4 D8 CA 8E 08 EA BA 9B EA C1 0B 73 " +
            "3B CF 6E C0 B0 7C 43 1F 36 13 31 F3 48 3C 18 2C " +
            "ED 5B F0 03 74 BC CA 10",
            // After Iota 
            "B1 4C B6 2C 85 DB FA 6A 5F D0 95 6F 41 5D EF 4D " +
            "6A ED B4 F9 D5 6F 4A 81 8F 89 D8 61 F0 3D C9 C2 " +
            "11 6E 62 C1 53 91 73 B8 43 CD 57 31 64 6D F2 D1 " +
            "3D 91 EC 73 4C EC 60 D0 2B A8 73 A7 05 C8 BC 60 " +
            "18 44 1C EA 92 72 A0 03 2A D9 D2 4D 54 68 47 47 " +
            "73 7B 9B 50 52 75 5F 7E 95 FD 82 4B AB F0 EB CE " +
            "CD 95 BE C6 E3 93 E8 AF CA DA 18 0A C8 73 AC 9C " +
            "BE E8 E6 C6 63 52 85 E3 F1 62 E5 63 51 67 71 F4 " +
            "F6 DF 95 D0 B8 D2 7E D8 87 39 B1 A8 F7 BA ED 0D " +
            "77 AF 9A B8 B0 2A 6E E5 54 E0 27 CA 29 BA D3 49 " +
            "66 AC 6A 51 E4 D8 CA 8E 08 EA BA 9B EA C1 0B 73 " +
            "3B CF 6E C0 B0 7C 43 1F 36 13 31 F3 48 3C 18 2C " +
            "ED 5B F0 03 74 BC CA 10",
            // Round #13  
            // After Theta
            "9E BA 9E 96 54 D3 70 DF 29 A8 2C F1 AE C5 62 4B " +
            "5A 32 8F 71 84 69 3D 50 C7 67 7D 96 F7 95 28 15 " +
            "20 AD FF F4 0D 43 18 53 6C 3B 7F 8B B5 65 78 64 " +
            "4B E9 55 ED A3 74 ED D6 1B 77 48 2F 54 CE CB B1 " +
            "50 AA B9 1D 95 DA 41 D4 1B 1A 4F 78 0A BA 2C AC " +
            "5C 8D B3 EA 83 7D D5 CB E3 85 3B D5 44 68 66 C8 " +
            "FD 4A 85 4E B2 95 9F 7E 82 34 BD FD CF DB 4D 4B " +
            "8F 2B 7B F3 3D 80 EE 08 DE 94 CD D9 80 6F FB 41 " +
            "80 A7 2C 4E 57 4A F3 DE B7 E6 8A 20 A6 BC 9A DC " +
            "3F 41 3F 4F B7 82 8F 32 65 23 BA FF 77 68 B8 A2 " +
            "49 5A 42 EB 35 D0 40 3B 7E 92 03 05 05 59 86 75 " +
            "0B 10 55 48 E1 7A 34 CE 7E FD 94 04 4F 94 F9 FB " +
            "DC 98 6D 36 2A 6E A1 FB",
            // After Rho  
            "9E BA 9E 96 54 D3 70 DF 52 50 59 E2 5D 8B C5 96 " +
            "96 CC 63 1C 61 5A 0F 94 5F 89 52 71 7C D6 67 79 " +
            "18 C2 98 02 69 FD A7 6F 58 5B 86 47 C6 B6 F3 B7 " +
            "D5 3E 4A D7 6E BD 94 5E EC C6 1D D2 0B 95 F3 72 " +
            "D5 DC 8E 4A ED 20 6A 28 CB C2 BA A1 F1 84 A7 A0 " +
            "E6 6A 9C 55 1F EC AB 5E 21 8F 17 EE 54 13 A1 99 " +
            "74 92 AD FC F4 EB 57 2A B7 9B 96 04 69 7A FB 9F " +
            "F9 1E 40 77 84 C7 95 BD B3 01 DF F6 83 BC 29 9B " +
            "C5 E9 4A 69 DE 1B F0 94 4D EE 5B 73 45 10 53 5E " +
            "F0 51 E6 27 E8 E7 E9 56 A2 65 23 BA FF 77 68 B8 " +
            "03 ED 24 69 09 AD D7 40 F9 49 0E 14 14 64 19 D6 " +
            "01 A2 0A 29 5C 8F C6 79 FD 94 04 4F 94 F9 FB 7E " +
            "E8 3E 37 66 9B 8D 8A 5B",
            // After Pi
            "9E BA 9E 96 54 D3 70 DF D5 3E 4A D7 6E BD 94 5E " +
            "74 92 AD FC F4 EB 57 2A F0 51 E6 27 E8 E7 E9 56 " +
            "E8 3E 37 66 9B 8D 8A 5B 5F 89 52 71 7C D6 67 79 " +
            "CB C2 BA A1 F1 84 A7 A0 E6 6A 9C 55 1F EC AB 5E " +
            "C5 E9 4A 69 DE 1B F0 94 01 A2 0A 29 5C 8F C6 79 " +
            "52 50 59 E2 5D 8B C5 96 EC C6 1D D2 0B 95 F3 72 " +
            "B7 9B 96 04 69 7A FB 9F A2 65 23 BA FF 77 68 B8 " +
            "03 ED 24 69 09 AD D7 40 18 C2 98 02 69 FD A7 6F " +
            "58 5B 86 47 C6 B6 F3 B7 21 8F 17 EE 54 13 A1 99 " +
            "4D EE 5B 73 45 10 53 5E FD 94 04 4F 94 F9 FB 7E " +
            "96 CC 63 1C 61 5A 0F 94 D5 DC 8E 4A ED 20 6A 28 " +
            "F9 1E 40 77 84 C7 95 BD B3 01 DF F6 83 BC 29 9B " +
            "F9 49 0E 14 14 64 19 D6",
            // After Chi  
            "BE 3A 3B BE C4 91 33 FF 55 7F 08 D4 66 B9 3C 0A " +
            "7C BC BC BC E7 E3 55 23 E6 D1 6E B7 AC B5 99 D2 " +
            "A9 3A 77 27 B1 A1 0E 5B 7B A1 56 25 72 BE 6F 27 " +
            "CA 43 F8 89 31 97 F7 20 E6 68 9C 55 1F 68 AD 37 " +
            "9B E0 1A 39 FE 4B D1 94 81 E0 A2 A9 DD 8F 46 F9 " +
            "41 49 DB E6 3D E1 CD 1B EC A2 3C 68 9D 90 F3 52 " +
            "B6 13 92 45 69 F2 6C DF F2 75 7A 38 AB 75 68 2E " +
            "AF 6B 20 79 0B B9 E5 20 39 46 89 AA 79 FC A7 67 " +
            "14 3B CE 56 C7 B6 A1 F1 91 9F 13 E2 C4 FA 09 B9 " +
            "4D AC C3 73 2C 14 57 5F BD 8D 02 0A 12 FB AB EE " +
            "BE CE 23 29 61 9D 9A 01 D7 DD 11 CA EE 18 42 2A " +
            "B1 56 40 77 90 87 85 F9 B5 85 BE FE E2 A6 2F 9B " +
            "B8 59 82 56 98 44 79 FE",
            // After Iota 
            "35 3A 3B BE C4 91 33 7F 55 7F 08 D4 66 B9 3C 0A " +
            "7C BC BC BC E7 E3 55 23 E6 D1 6E B7 AC B5 99 D2 " +
            "A9 3A 77 27 B1 A1 0E 5B 7B A1 56 25 72 BE 6F 27 " +
            "CA 43 F8 89 31 97 F7 20 E6 68 9C 55 1F 68 AD 37 " +
            "9B E0 1A 39 FE 4B D1 94 81 E0 A2 A9 DD 8F 46 F9 " +
            "41 49 DB E6 3D E1 CD 1B EC A2 3C 68 9D 90 F3 52 " +
            "B6 13 92 45 69 F2 6C DF F2 75 7A 38 AB 75 68 2E " +
            "AF 6B 20 79 0B B9 E5 20 39 46 89 AA 79 FC A7 67 " +
            "14 3B CE 56 C7 B6 A1 F1 91 9F 13 E2 C4 FA 09 B9 " +
            "4D AC C3 73 2C 14 57 5F BD 8D 02 0A 12 FB AB EE " +
            "BE CE 23 29 61 9D 9A 01 D7 DD 11 CA EE 18 42 2A " +
            "B1 56 40 77 90 87 85 F9 B5 85 BE FE E2 A6 2F 9B " +
            "B8 59 82 56 98 44 79 FE",
            // Round #14  
            // After Theta
            "D6 AE 68 47 EE 98 FA AA C4 39 D6 59 7F 1F A0 39 " +
            "23 1E 49 63 6A 81 3E D8 EF 14 65 D8 B2 E0 7F 7D " +
            "CE E2 3C E0 A1 C7 0F BC 98 35 05 DC 58 B7 A6 F2 " +
            "5B 05 26 04 28 31 6B 13 B9 CA 69 8A 92 0A C6 CC " +
            "92 25 11 56 E0 1E 37 3B E6 38 E9 6E CD E9 47 1E " +
            "A2 DD 88 1F 17 E8 04 CE 7D E4 E2 E5 84 36 6F 61 " +
            "E9 B1 67 9A E4 90 07 24 FB B0 71 57 B5 20 8E 81 " +
            "C8 B3 6B BE 1B DF E4 C7 DA D2 DA 53 53 F5 6E B2 " +
            "85 7D 10 DB DE 10 3D C2 CE 3D E6 3D 49 98 62 42 " +
            "44 69 C8 1C 32 41 B1 F0 DA 55 49 CD 02 9D AA 09 " +
            "5D 5A 70 D0 4B 94 53 D4 46 9B CF 47 F7 BE DE 19 " +
            "EE F4 B5 A8 1D E5 EE 02 BC 40 B5 91 FC F3 C9 34 " +
            "DF 81 C9 91 88 22 78 19",
            // After Rho  
            "D6 AE 68 47 EE 98 FA AA 88 73 AC B3 FE 3E 40 73 " +
            "88 47 D2 98 5A A0 0F F6 0B FE D7 F7 4E 51 86 2D " +
            "3D 7E E0 75 16 E7 01 0F 8D 75 6B 2A 8F 59 53 C0 " +
            "42 80 12 B3 36 B1 55 60 73 AE 72 9A A2 A4 82 31 " +
            "92 08 2B 70 8F 9B 1D C9 7E E4 61 8E 93 EE D6 9C " +
            "16 ED 46 FC B8 40 27 70 85 F5 91 8B 97 13 DA BC " +
            "D3 24 87 3C 20 49 8F 3D 41 1C 03 F7 61 E3 AE 6A " +
            "DF 8D 6F F2 63 E4 D9 35 A7 A6 EA DD 64 B5 A5 B5 " +
            "62 DB 1B A2 47 B8 B0 0F 31 21 E7 1E F3 9E 24 4C " +
            "28 16 9E 28 0D 99 43 26 09 DA 55 49 CD 02 9D AA " +
            "4E 51 77 69 C1 41 2F 51 18 6D 3E 1F DD FB 7A 67 " +
            "9D BE 16 B5 A3 DC 5D C0 40 B5 91 FC F3 C9 34 BC " +
            "5E C6 77 60 72 24 A2 08",
            // After Pi
            "D6 AE 68 47 EE 98 FA AA 42 80 12 B3 36 B1 55 60 " +
            "D3 24 87 3C 20 49 8F 3D 28 16 9E 28 0D 99 43 26 " +
            "5E C6 77 60 72 24 A2 08 0B FE D7 F7 4E 51 86 2D " +
            "7E E4 61 8E 93 EE D6 9C 16 ED 46 FC B8 40 27 70 " +
            "62 DB 1B A2 47 B8 B0 0F 9D BE 16 B5 A3 DC 5D C0 " +
            "88 73 AC B3 FE 3E 40 73 73 AE 72 9A A2 A4 82 31 " +
            "41 1C 03 F7 61 E3 AE 6A 09 DA 55 49 CD 02 9D AA " +
            "4E 51 77 69 C1 41 2F 51 3D 7E E0 75 16 E7 01 0F " +
            "8D 75 6B 2A 8F 59 53 C0 85 F5 91 8B 97 13 DA BC " +
            "31 21 E7 1E F3 9E 24 4C 40 B5 91 FC F3 C9 34 BC " +
            "88 47 D2 98 5A A0 0F F6 92 08 2B 70 8F 9B 1D C9 " +
            "DF 8D 6F F2 63 E4 D9 35 A7 A6 EA DD 64 B5 A5 B5 " +
            "18 6D 3E 1F DD FB 7A 67",
            // After Chi  
            "47 8A ED 4B EE D0 70 B7 6A 92 0A B3 3B 21 15 62 " +
            "85 E4 E6 7C 52 6D 2F 35 A8 3E 96 2F 81 01 1B 84 " +
            "5E C6 65 D0 62 05 A7 48 0B F7 D1 87 66 51 A7 4D " +
            "1E F6 78 8C D4 56 46 93 8B C9 42 E9 18 04 6A B0 " +
            "60 9B DA E0 0B B9 32 22 E9 BE 36 BD 32 72 0D 50 " +
            "88 63 AD D6 BF 7D 6C 39 7B 6C 26 92 2E A4 93 B1 " +
            "07 1D 21 D7 61 A2 8C 3B 89 F8 DD DB F3 3C DD 88 " +
            "3D DD 25 61 C1 C1 AD 51 3D FE 70 F4 06 E5 89 33 " +
            "BD 75 0D 3E EF D5 77 80 C5 61 81 6B 97 52 CA 0C " +
            "0C 6B 87 1F F7 B8 25 4F C0 B4 9A F6 7A D1 66 7C " +
            "C5 C2 96 1A 3A C4 CF C2 B2 2A AB 7D 8B 8A 39 49 " +
            "C7 C4 7B F0 FA AE 83 77 27 A4 2A 5D 66 B5 A0 25 " +
            "0A 65 17 7F 58 E0 6A 6E",
            // After Iota 
            "CE 0A ED 4B EE D0 70 37 6A 92 0A B3 3B 21 15 62 " +
            "85 E4 E6 7C 52 6D 2F 35 A8 3E 96 2F 81 01 1B 84 " +
            "5E C6 65 D0 62 05 A7 48 0B F7 D1 87 66 51 A7 4D " +
            "1E F6 78 8C D4 56 46 93 8B C9 42 E9 18 04 6A B0 " +
            "60 9B DA E0 0B B9 32 22 E9 BE 36 BD 32 72 0D 50 " +
            "88 63 AD D6 BF 7D 6C 39 7B 6C 26 92 2E A4 93 B1 " +
            "07 1D 21 D7 61 A2 8C 3B 89 F8 DD DB F3 3C DD 88 " +
            "3D DD 25 61 C1 C1 AD 51 3D FE 70 F4 06 E5 89 33 " +
            "BD 75 0D 3E EF D5 77 80 C5 61 81 6B 97 52 CA 0C " +
            "0C 6B 87 1F F7 B8 25 4F C0 B4 9A F6 7A D1 66 7C " +
            "C5 C2 96 1A 3A C4 CF C2 B2 2A AB 7D 8B 8A 39 49 " +
            "C7 C4 7B F0 FA AE 83 77 27 A4 2A 5D 66 B5 A0 25 " +
            "0A 65 17 7F 58 E0 6A 6E",
            // Round #15  
            // After Theta
            "8F D0 F2 13 16 4E 66 7F C8 1A 82 F5 BD 92 E8 5B " +
            "51 97 6D 3E 27 F2 42 34 23 43 1F FD A0 39 8C F7 " +
            "5F 11 B6 6E 9D 36 2D 69 4A 2D CE DF 9E CF B1 05 " +
            "BC 7E F0 CA 52 E5 BB AA 5F BA C9 AB 6D 9B 07 B1 " +
            "EB E6 53 32 2A 81 A5 51 E8 69 E5 03 CD 41 87 71 " +
            "C9 B9 B2 8E 47 E3 7A 71 D9 E4 AE D4 A8 17 6E 88 " +
            "D3 6E AA 95 14 3D E1 3A 02 85 54 09 D2 04 4A FB " +
            "3C 0A F6 DF 3E F2 27 70 7C 24 6F AC FE 7B 9F 7B " +
            "1F FD 85 78 69 66 8A B9 11 12 0A 29 E2 CD A7 0D " +
            "87 16 0E CD D6 80 B2 3C C1 63 49 48 85 E2 EC 5D " +
            "84 18 89 42 C2 5A D9 8A 10 A2 23 3B 0D 39 C4 70 " +
            "13 B7 F0 B2 8F 31 EE 76 AC D9 A3 8F 47 8D 37 56 " +
            "0B B2 C4 C1 A7 D3 E0 4F",
            // After Rho  
            "8F D0 F2 13 16 4E 66 7F 90 35 04 EB 7B 25 D1 B7 " +
            "D4 65 9B CF 89 BC 10 4D 9A C3 78 3F 32 F4 D1 0F " +
            "B4 69 49 FB 8A B0 75 EB ED F9 1C 5B A0 D4 E2 FC " +
            "AF 2C 55 BE AB CA EB 07 EC 97 6E F2 6A DB E6 41 " +
            "F3 29 19 95 C0 D2 A8 75 74 18 87 9E 56 3E D0 1C " +
            "4B CE 95 75 3C 1A D7 8B 21 66 93 BB 52 A3 5E B8 " +
            "AD A4 E8 09 D7 99 76 53 09 94 F6 05 0A A9 12 A4 " +
            "6F 1F F9 13 38 1E 05 FB 58 FD F7 3E F7 F8 48 DE " +
            "10 2F CD 4C 31 F7 A3 BF D3 86 08 09 85 14 F1 E6 " +
            "50 96 E7 D0 C2 A1 D9 1A 5D C1 63 49 48 85 E2 EC " +
            "65 2B 12 62 24 0A 09 6B 41 88 8E EC 34 E4 10 C3 " +
            "E2 16 5E F6 31 C6 DD 6E D9 A3 8F 47 8D 37 56 AC " +
            "F8 D3 82 2C 71 F0 E9 34",
            // After Pi
            "8F D0 F2 13 16 4E 66 7F AF 2C 55 BE AB CA EB 07 " +
            "AD A4 E8 09 D7 99 76 53 50 96 E7 D0 C2 A1 D9 1A " +
            "F8 D3 82 2C 71 F0 E9 34 9A C3 78 3F 32 F4 D1 0F " +
            "74 18 87 9E 56 3E D0 1C 4B CE 95 75 3C 1A D7 8B " +
            "10 2F CD 4C 31 F7 A3 BF E2 16 5E F6 31 C6 DD 6E " +
            "90 35 04 EB 7B 25 D1 B7 EC 97 6E F2 6A DB E6 41 " +
            "09 94 F6 05 0A A9 12 A4 5D C1 63 49 48 85 E2 EC " +
            "65 2B 12 62 24 0A 09 6B B4 69 49 FB 8A B0 75 EB " +
            "ED F9 1C 5B A0 D4 E2 FC 21 66 93 BB 52 A3 5E B8 " +
            "D3 86 08 09 85 14 F1 E6 D9 A3 8F 47 8D 37 56 AC " +
            "D4 65 9B CF 89 BC 10 4D F3 29 19 95 C0 D2 A8 75 " +
            "6F 1F F9 13 38 1E 05 FB 58 FD F7 3E F7 F8 48 DE " +
            "41 88 8E EC 34 E4 10 C3",
            // After Chi  
            "8F 50 5A 12 42 5F 72 2F FF 3E 52 6E AB EA 62 0F " +
            "05 E5 E8 25 E6 C9 56 77 57 96 97 C3 C4 AF DF 51 " +
            "D8 FF 87 80 D8 70 60 34 91 05 68 5E 1A F4 D6 8C " +
            "64 39 CF 96 57 DB F0 28 A9 DE 87 C7 3C 1A 8B CB " +
            "08 EE ED 45 33 C7 A3 BE 86 0E D9 76 75 CC DD 7E " +
            "91 35 94 EE 7B 05 C1 13 B8 D6 6F BA 2A DF 06 09 " +
            "29 BE E6 27 2E A3 1B A7 CD D5 67 C0 13 A0 32 78 " +
            "09 A9 78 72 24 D0 2F 2B B4 6F CA 5B D8 93 69 EB " +
            "3F 79 14 5B 25 C0 43 BA 29 47 14 FD 5A 80 58 B0 " +
            "F7 CE 48 B1 87 94 D0 A5 90 33 9B 47 AD 73 D4 B8 " +
            "D8 73 7B CD B1 B0 15 C7 E3 C9 1F B9 07 32 E0 71 " +
            "6E 1F F1 D3 38 1A 15 FA CC 98 E6 3D 7E E0 48 D2 " +
            "62 80 8E FC 74 A6 B8 F3",
            // After Iota 
            "8C D0 5A 12 42 5F 72 AF FF 3E 52 6E AB EA 62 0F " +
            "05 E5 E8 25 E6 C9 56 77 57 96 97 C3 C4 AF DF 51 " +
            "D8 FF 87 80 D8 70 60 34 91 05 68 5E 1A F4 D6 8C " +
            "64 39 CF 96 57 DB F0 28 A9 DE 87 C7 3C 1A 8B CB " +
            "08 EE ED 45 33 C7 A3 BE 86 0E D9 76 75 CC DD 7E " +
            "91 35 94 EE 7B 05 C1 13 B8 D6 6F BA 2A DF 06 09 " +
            "29 BE E6 27 2E A3 1B A7 CD D5 67 C0 13 A0 32 78 " +
            "09 A9 78 72 24 D0 2F 2B B4 6F CA 5B D8 93 69 EB " +
            "3F 79 14 5B 25 C0 43 BA 29 47 14 FD 5A 80 58 B0 " +
            "F7 CE 48 B1 87 94 D0 A5 90 33 9B 47 AD 73 D4 B8 " +
            "D8 73 7B CD B1 B0 15 C7 E3 C9 1F B9 07 32 E0 71 " +
            "6E 1F F1 D3 38 1A 15 FA CC 98 E6 3D 7E E0 48 D2 " +
            "62 80 8E FC 74 A6 B8 F3",
            // Round #16  
            // After Theta
            "D6 F8 9B 6C FB DF E2 4F 9B 79 9C 8C CC B2 6C B0 " +
            "A9 73 76 10 29 AD CC 53 DF 9C 9C 56 F2 37 A9 55 " +
            "B1 FD 1B 22 51 D6 85 EC CB 2D A9 20 A3 74 46 6C " +
            "00 7E 01 74 30 83 FE 97 05 48 19 F2 F3 7E 11 EF " +
            "80 E4 E6 D0 05 5F D5 BA EF 0C 45 D4 FC 6A 38 A6 " +
            "CB 1D 55 90 C2 85 51 F3 DC 91 A1 58 4D 87 08 B6 " +
            "85 28 78 12 E1 C7 81 83 45 DF 6C 55 25 38 44 7C " +
            "60 AB E4 D0 AD 76 CA F3 EE 47 0B 25 61 13 F9 0B " +
            "5B 3E DA B9 42 98 4D 05 85 D1 8A C8 95 E4 C2 94 " +
            "7F C4 43 24 B1 0C A6 A1 F9 31 07 E5 24 D5 31 60 " +
            "82 5B BA B3 08 30 85 27 87 8E D1 5B 60 6A EE CE " +
            "C2 89 6F E6 F7 7E 8F DE 44 92 ED A8 48 78 3E D6 " +
            "0B 82 12 5E FD 00 5D 2B",
            // After Rho  
            "D6 F8 9B 6C FB DF E2 4F 37 F3 38 19 99 65 D9 60 " +
            "EA 9C 1D 44 4A 2B F3 54 7F 93 5A F5 CD C9 69 25 " +
            "B2 2E 64 8F ED DF 10 89 32 4A 67 C4 B6 DC 92 0A " +
            "40 07 33 E8 7F 09 E0 17 7B 01 52 86 FC BC 5F C4 " +
            "72 73 E8 82 AF 6A 5D 40 86 63 FA CE 50 44 CD AF " +
            "5F EE A8 82 14 2E 8C 9A D8 72 47 86 62 35 1D 22 " +
            "93 08 3F 0E 1C 2C 44 C1 70 88 F8 8A BE D9 AA 4A " +
            "E8 56 3B E5 79 B0 55 72 4A C2 26 F2 17 DC 8F 16 " +
            "3B 57 08 B3 A9 60 CB 47 61 CA C2 68 45 E4 4A 72 " +
            "C1 34 F4 8F 78 88 24 96 60 F9 31 07 E5 24 D5 31 " +
            "14 9E 08 6E E9 CE 22 C0 1F 3A 46 6F 81 A9 B9 3B " +
            "38 F1 CD FC DE EF D1 5B 92 ED A8 48 78 3E D6 44 " +
            "D7 CA 82 A0 84 57 3F 40",
            // After Pi
            "D6 F8 9B 6C FB DF E2 4F 40 07 33 E8 7F 09 E0 17 " +
            "93 08 3F 0E 1C 2C 44 C1 C1 34 F4 8F 78 88 24 96 " +
            "D7 CA 82 A0 84 57 3F 40 7F 93 5A F5 CD C9 69 25 " +
            "86 63 FA CE 50 44 CD AF 5F EE A8 82 14 2E 8C 9A " +
            "3B 57 08 B3 A9 60 CB 47 38 F1 CD FC DE EF D1 5B " +
            "37 F3 38 19 99 65 D9 60 7B 01 52 86 FC BC 5F C4 " +
            "70 88 F8 8A BE D9 AA 4A 60 F9 31 07 E5 24 D5 31 " +
            "14 9E 08 6E E9 CE 22 C0 B2 2E 64 8F ED DF 10 89 " +
            "32 4A 67 C4 B6 DC 92 0A D8 72 47 86 62 35 1D 22 " +
            "61 CA C2 68 45 E4 4A 72 92 ED A8 48 78 3E D6 44 " +
            "EA 9C 1D 44 4A 2B F3 54 72 73 E8 82 AF 6A 5D 40 " +
            "E8 56 3B E5 79 B0 55 72 4A C2 26 F2 17 DC 8F 16 " +
            "1F 3A 46 6F 81 A9 B9 3B",
            // After Chi  
            "45 F0 97 6A FB FB E6 8F 00 33 F3 69 1F 89 C0 01 " +
            "85 C2 3D 2E 98 7B 5F 81 C1 04 ED C3 03 00 E4 99 " +
            "D7 CD A2 20 80 57 3F 50 26 1F 5A F5 C9 E3 69 35 " +
            "A6 72 FA FF F9 04 8E EA 5F 4E 6D CE 42 A1 9C 82 " +
            "7C 55 1A B2 A8 60 E3 63 B8 91 6D F6 CE EB 55 D1 " +
            "37 7B 90 11 9B 24 79 6A 7B 70 53 83 BD 98 0A F5 " +
            "64 8E F0 E2 B6 13 88 8A 43 98 01 16 F5 05 0C 11 " +
            "5C 9E 4A E8 8D 56 24 44 7A 1E 64 8D AD FE 1D A9 " +
            "13 C2 E7 AC B3 1C D0 5A 4A 57 6F 86 5A 2F 89 26 " +
            "41 C8 86 EF C0 25 4A FB 92 AD AB 08 6A 3E 54 46 " +
            "62 98 0E 21 1A BB F3 66 70 F3 EC 90 A9 26 D7 44 " +
            "FD 6E 7B E8 F9 91 65 5B AA 46 3F F2 5D DE CD 52 " +
            "0F 59 A6 ED 24 E9 B5 3B",
            // After Iota 
            "47 70 97 6A FB FB E6 0F 00 33 F3 69 1F 89 C0 01 " +
            "85 C2 3D 2E 98 7B 5F 81 C1 04 ED C3 03 00 E4 99 " +
            "D7 CD A2 20 80 57 3F 50 26 1F 5A F5 C9 E3 69 35 " +
            "A6 72 FA FF F9 04 8E EA 5F 4E 6D CE 42 A1 9C 82 " +
            "7C 55 1A B2 A8 60 E3 63 B8 91 6D F6 CE EB 55 D1 " +
            "37 7B 90 11 9B 24 79 6A 7B 70 53 83 BD 98 0A F5 " +
            "64 8E F0 E2 B6 13 88 8A 43 98 01 16 F5 05 0C 11 " +
            "5C 9E 4A E8 8D 56 24 44 7A 1E 64 8D AD FE 1D A9 " +
            "13 C2 E7 AC B3 1C D0 5A 4A 57 6F 86 5A 2F 89 26 " +
            "41 C8 86 EF C0 25 4A FB 92 AD AB 08 6A 3E 54 46 " +
            "62 98 0E 21 1A BB F3 66 70 F3 EC 90 A9 26 D7 44 " +
            "FD 6E 7B E8 F9 91 65 5B AA 46 3F F2 5D DE CD 52 " +
            "0F 59 A6 ED 24 E9 B5 3B",
            // Round #17  
            // After Theta
            "95 47 BD E3 F4 98 CF B7 5D D7 AC 92 9F 1F 96 77 " +
            "11 4C F2 F3 5F 69 05 04 95 52 49 18 D7 0C 1D 1C " +
            "5F AE 82 1E 7F 3B 83 2C F4 28 70 7C C6 80 40 8D " +
            "FB 96 A5 04 79 92 D8 9C CB C0 A2 13 85 B3 C6 07 " +
            "28 03 BE 69 7C 6C 1A E6 30 F2 4D C8 31 87 E9 AD " +
            "E5 4C BA 98 94 47 50 D2 26 94 0C 78 3D 0E 5C 83 " +
            "F0 00 3F 3F 71 01 D2 0F 17 CE A5 CD 21 09 F5 94 " +
            "D4 FD 6A D6 72 3A 98 38 A8 29 4E 04 A2 9D 34 11 " +
            "4E 26 B8 57 33 8A 86 2C DE D9 A0 5B 9D 3D D3 A3 " +
            "15 9E 22 34 14 29 B3 7E 1A CE 8B 36 95 52 E8 3A " +
            "B0 AF 24 A8 15 D8 DA DE 2D 17 B3 6B 29 B0 81 32 " +
            "69 E0 B4 35 3E 83 3F DE FE 10 9B 29 89 D2 34 D7 " +
            "87 3A 86 D3 DB 85 09 47",
            // After Rho  
            "95 47 BD E3 F4 98 CF B7 BA AE 59 25 3F 3F 2C EF " +
            "04 93 FC FC 57 5A 01 41 CD D0 C1 51 29 95 84 71 " +
            "DB 19 64 F9 72 15 F4 F8 67 0C 08 D4 48 8F 02 C7 " +
            "4A 90 27 89 CD B9 6F 59 C1 32 B0 E8 44 E1 AC F1 " +
            "01 DF 34 3E 36 0D 73 94 98 DE 0A 23 DF 84 1C 73 " +
            "2E 67 D2 C5 A4 3C 82 92 0D 9A 50 32 E0 F5 38 70 " +
            "F9 89 0B 90 7E 80 07 F8 12 EA 29 2F 9C 4B 9B 43 " +
            "6B 39 1D 4C 1C EA 7E 35 08 44 3B 69 22 50 53 9C " +
            "F7 6A 46 D1 90 C5 C9 04 E9 51 EF 6C D0 AD CE 9E " +
            "65 D6 AF C2 53 84 86 22 3A 1A CE 8B 36 95 52 E8 " +
            "6B 7B C3 BE 92 A0 56 60 B4 5C CC AE A5 C0 06 CA " +
            "0D 9C B6 C6 67 F0 C7 3B 10 9B 29 89 D2 34 D7 FE " +
            "C2 D1 A1 8E E1 F4 76 61",
            // After Pi
            "95 47 BD E3 F4 98 CF B7 4A 90 27 89 CD B9 6F 59 " +
            "F9 89 0B 90 7E 80 07 F8 65 D6 AF C2 53 84 86 22 " +
            "C2 D1 A1 8E E1 F4 76 61 CD D0 C1 51 29 95 84 71 " +
            "98 DE 0A 23 DF 84 1C 73 2E 67 D2 C5 A4 3C 82 92 " +
            "F7 6A 46 D1 90 C5 C9 04 0D 9C B6 C6 67 F0 C7 3B " +
            "BA AE 59 25 3F 3F 2C EF C1 32 B0 E8 44 E1 AC F1 " +
            "12 EA 29 2F 9C 4B 9B 43 3A 1A CE 8B 36 95 52 E8 " +
            "6B 7B C3 BE 92 A0 56 60 DB 19 64 F9 72 15 F4 F8 " +
            "67 0C 08 D4 48 8F 02 C7 0D 9A 50 32 E0 F5 38 70 " +
            "E9 51 EF 6C D0 AD CE 9E 10 9B 29 89 D2 34 D7 FE " +
            "04 93 FC FC 57 5A 01 41 01 DF 34 3E 36 0D 73 94 " +
            "6B 39 1D 4C 1C EA 7E 35 08 44 3B 69 22 50 53 9C " +
            "B4 5C CC AE A5 C0 06 CA",
            // After Chi  
            "24 4E B5 F3 C6 98 CF 17 4E C6 83 CB CC BD EF 5B " +
            "7B 88 0B 9C DE F0 77 B9 70 D0 B3 A3 47 8C 0F B4 " +
            "88 41 A3 86 E8 D5 56 29 EB F1 11 95 09 AD 06 F1 " +
            "49 D6 0E 33 CF 45 55 77 26 F3 62 C3 C3 0C 84 A9 " +
            "37 2A 07 C0 98 C0 C9 44 1D 92 BC E4 B1 F0 DF 39 " +
            "A8 66 50 22 A7 35 3F ED E9 22 76 68 66 75 EC 59 " +
            "53 8B 28 1B 1C 6B 9F 43 AA 9E D6 8A 1B 8A 7A 67 " +
            "2A 6B 63 76 D2 60 D6 70 D3 8B 34 DB D2 65 CC C8 " +
            "87 4D A7 98 58 87 C4 49 1D 10 50 B3 E2 E5 29 10 " +
            "22 51 AB 1C F0 AC EE 9E 34 9F 21 8D DA BE D5 F9 " +
            "6E B3 F5 BC 5F B8 0D 60 01 9B 16 1F 14 1D 72 1C " +
            "DF 21 D9 CA 99 6A 7A 77 08 C7 0B 39 70 4A 52 9D " +
            "B5 10 CC AC 85 C5 74 5E",
            // After Iota 
            "A4 4E B5 F3 C6 98 CF 97 4E C6 83 CB CC BD EF 5B " +
            "7B 88 0B 9C DE F0 77 B9 70 D0 B3 A3 47 8C 0F B4 " +
            "88 41 A3 86 E8 D5 56 29 EB F1 11 95 09 AD 06 F1 " +
            "49 D6 0E 33 CF 45 55 77 26 F3 62 C3 C3 0C 84 A9 " +
            "37 2A 07 C0 98 C0 C9 44 1D 92 BC E4 B1 F0 DF 39 " +
            "A8 66 50 22 A7 35 3F ED E9 22 76 68 66 75 EC 59 " +
            "53 8B 28 1B 1C 6B 9F 43 AA 9E D6 8A 1B 8A 7A 67 " +
            "2A 6B 63 76 D2 60 D6 70 D3 8B 34 DB D2 65 CC C8 " +
            "87 4D A7 98 58 87 C4 49 1D 10 50 B3 E2 E5 29 10 " +
            "22 51 AB 1C F0 AC EE 9E 34 9F 21 8D DA BE D5 F9 " +
            "6E B3 F5 BC 5F B8 0D 60 01 9B 16 1F 14 1D 72 1C " +
            "DF 21 D9 CA 99 6A 7A 77 08 C7 0B 39 70 4A 52 9D " +
            "B5 10 CC AC 85 C5 74 5E",
            // Round #18  
            // After Theta
            "4A B1 B1 E8 40 88 F1 11 8C A4 27 93 DD 50 A6 10 " +
            "9C 89 C4 12 7E A7 97 B1 C1 7F 59 F5 95 E9 CC 0F " +
            "FB 71 0A 0C 66 4E 39 FB 05 0E 15 8E 8F BD 38 77 " +
            "8B B4 AA 6B DE A8 1C 3C C1 F2 AD 4D 63 5B 64 A1 " +
            "86 85 ED 96 4A A5 0A FF 6E A2 15 6E 3F 6B B0 EB " +
            "46 99 54 39 21 25 01 6B 2B 40 D2 30 77 98 A5 12 " +
            "B4 8A E7 95 BC 3C 7F 4B 1B 31 3C DC C9 EF B9 DC " +
            "59 5B CA FC 5C FB B9 A2 3D 74 30 C0 54 75 F2 4E " +
            "45 2F 03 C0 49 6A 8D 02 FA 11 9F 3D 42 B2 C9 18 " +
            "93 FE 41 4A 22 C9 2D 25 47 AF 88 07 54 25 BA 2B " +
            "80 4C F1 A7 D9 A8 33 E6 C3 F9 B2 47 05 F0 3B 57 " +
            "38 20 16 44 39 3D 9A 7F B9 68 E1 6F A2 2F 91 26 " +
            "C6 20 65 26 0B 5E 1B 8C",
            // After Rho  
            "4A B1 B1 E8 40 88 F1 11 18 49 4F 26 BB A1 4C 21 " +
            "67 22 B1 84 DF E9 65 2C 99 CE FC 10 FC 97 55 5F " +
            "73 CA D9 DF 8F 53 60 30 F8 D8 8B 73 57 E0 50 E1 " +
            "BA E6 8D CA C1 B3 48 AB 68 B0 7C 6B D3 D8 16 59 " +
            "C2 76 4B A5 52 85 7F C3 06 BB EE 26 5A E1 F6 B3 " +
            "33 CA A4 CA 09 29 09 58 4A AC 00 49 C3 DC 61 96 " +
            "AF E4 E5 F9 5B A2 55 3C DF 73 B9 37 62 78 B8 93 " +
            "7E AE FD 5C D1 AC 2D 65 80 A9 EA E4 9D 7A E8 60 " +
            "00 38 49 AD 51 A0 E8 65 64 0C FD 88 CF 1E 21 D9 " +
            "B9 A5 64 D2 3F 48 49 24 2B 47 AF 88 07 54 25 BA " +
            "CE 98 03 32 C5 9F 66 A3 0D E7 CB 1E 15 C0 EF 5C " +
            "07 C4 82 28 A7 47 F3 0F 68 E1 6F A2 2F 91 26 B9 " +
            "06 A3 31 48 99 C9 82 D7",
            // After Pi
            "4A B1 B1 E8 40 88 F1 11 BA E6 8D CA C1 B3 48 AB " +
            "AF E4 E5 F9 5B A2 55 3C B9 A5 64 D2 3F 48 49 24 " +
            "06 A3 31 48 99 C9 82 D7 99 CE FC 10 FC 97 55 5F " +
            "06 BB EE 26 5A E1 F6 B3 33 CA A4 CA 09 29 09 58 " +
            "00 38 49 AD 51 A0 E8 65 07 C4 82 28 A7 47 F3 0F " +
            "18 49 4F 26 BB A1 4C 21 68 B0 7C 6B D3 D8 16 59 " +
            "DF 73 B9 37 62 78 B8 93 2B 47 AF 88 07 54 25 BA " +
            "CE 98 03 32 C5 9F 66 A3 73 CA D9 DF 8F 53 60 30 " +
            "F8 D8 8B 73 57 E0 50 E1 4A AC 00 49 C3 DC 61 96 " +
            "64 0C FD 88 CF 1E 21 D9 68 E1 6F A2 2F 91 26 B9 " +
            "67 22 B1 84 DF E9 65 2C C2 76 4B A5 52 85 7F C3 " +
            "7E AE FD 5C D1 AC 2D 65 80 A9 EA E4 9D 7A E8 60 " +
            "0D E7 CB 1E 15 C0 EF 5C",
            // After Chi  
            "4F B1 D1 D9 5A 88 E4 05 AA E7 8D C8 E5 FB 40 AB " +
            "A9 E6 F4 F1 DB 23 D7 EF F1 B5 E4 72 7F 48 38 24 " +
            "B6 E5 3D 4A 18 FA 8A 7D A8 8E FC D8 FD 9F 5C 17 " +
            "06 8B A7 03 0A 61 16 96 34 0E 26 CA AF 6E 1A 52 " +
            "98 32 35 BD 09 30 EC 35 01 F5 80 0E A5 27 51 AF " +
            "8F 0A CE 32 9B 81 E4 A3 48 B4 7A E3 D6 DC 13 71 " +
            "1B EB B9 05 A2 F3 FA 92 3B 06 E3 8C 3D 74 2D BA " +
            "AE 28 33 7B 85 C7 74 FB 71 EE D9 D7 0F 4F 41 26 " +
            "DC D8 76 F3 5B E2 50 A8 42 4D 02 6B E3 5D 67 B6 " +
            "77 06 6D D5 4F 5C 61 D9 E0 F1 6D 82 7F 31 36 78 " +
            "5B AA 05 DC 5E C1 65 08 42 77 49 05 5E D7 BF C3 " +
            "73 E8 FC 46 D1 2C 2A 79 E2 A9 DA 64 57 53 E8 40 " +
            "8D B3 81 3F 15 C4 F5 9F",
            // After Iota 
            "45 31 D1 D9 5A 88 E4 05 AA E7 8D C8 E5 FB 40 AB " +
            "A9 E6 F4 F1 DB 23 D7 EF F1 B5 E4 72 7F 48 38 24 " +
            "B6 E5 3D 4A 18 FA 8A 7D A8 8E FC D8 FD 9F 5C 17 " +
            "06 8B A7 03 0A 61 16 96 34 0E 26 CA AF 6E 1A 52 " +
            "98 32 35 BD 09 30 EC 35 01 F5 80 0E A5 27 51 AF " +
            "8F 0A CE 32 9B 81 E4 A3 48 B4 7A E3 D6 DC 13 71 " +
            "1B EB B9 05 A2 F3 FA 92 3B 06 E3 8C 3D 74 2D BA " +
            "AE 28 33 7B 85 C7 74 FB 71 EE D9 D7 0F 4F 41 26 " +
            "DC D8 76 F3 5B E2 50 A8 42 4D 02 6B E3 5D 67 B6 " +
            "77 06 6D D5 4F 5C 61 D9 E0 F1 6D 82 7F 31 36 78 " +
            "5B AA 05 DC 5E C1 65 08 42 77 49 05 5E D7 BF C3 " +
            "73 E8 FC 46 D1 2C 2A 79 E2 A9 DA 64 57 53 E8 40 " +
            "8D B3 81 3F 15 C4 F5 9F",
            // Round #19  
            // After Theta
            "C5 A5 6D E7 71 81 DC 84 8D 5B 99 D7 40 7C CD F4 " +
            "5D CC 91 CA 40 56 9D AC AF E7 B5 65 3E 59 9B 58 " +
            "E0 29 C7 C8 91 C9 0A 71 28 1A 40 E6 D6 96 64 96 " +
            "21 37 B3 1C AF E6 9B C9 C0 24 43 F1 34 1B 50 11 " +
            "C6 60 64 AA 48 21 4F 49 57 39 7A 8C 2C 14 D1 A3 " +
            "0F 9E 72 0C B0 88 DC 22 6F 08 6E FC 73 5B 9E 2E " +
            "EF C1 DC 3E 39 86 B0 D1 65 54 B2 9B 7C 65 8E C6 " +
            "F8 E4 C9 F9 0C F4 F4 F7 F1 7A 65 E9 24 46 79 A7 " +
            "FB 64 62 EC FE 65 DD F7 B6 67 67 50 78 28 2D F5 " +
            "29 54 3C C2 0E 4D C2 A5 B6 3D 97 00 F6 02 B6 74 " +
            "DB 3E B9 E2 75 C8 5D 89 65 CB 5D 1A FB 50 32 9C " +
            "87 C2 99 7D 4A 59 60 3A BC FB 8B 73 16 42 4B 3C " +
            "DB 7F 7B BD 9C F7 75 93",
            // After Rho  
            "C5 A5 6D E7 71 81 DC 84 1B B7 32 AF 81 F8 9A E9 " +
            "17 73 A4 32 90 55 27 6B 93 B5 89 F5 7A 5E 5B E6 " +
            "4C 56 88 03 4F 39 46 8E 6E 6D 49 66 89 A2 01 64 " +
            "CB F1 6A BE 99 1C 72 33 04 30 C9 50 3C CD 06 54 " +
            "30 32 55 A4 90 A7 24 63 11 3D 7A 95 A3 C7 C8 42 " +
            "79 F0 94 63 80 45 E4 16 BA BC 21 B8 F1 CF 6D 79 " +
            "F6 C9 31 84 8D 7E 0F E6 CA 1C 8D CB A8 64 37 F9 " +
            "7C 06 7A FA 7B 7C F2 E4 D2 49 8C F2 4E E3 F5 CA " +
            "8C DD BF AC FB 7E 9F 4C 96 7A DB B3 33 28 3C 94 " +
            "49 B8 34 85 8A 47 D8 A1 74 B6 3D 97 00 F6 02 B6 " +
            "77 25 6E FB E4 8A D7 21 96 2D 77 69 EC 43 C9 70 " +
            "50 38 B3 4F 29 0B 4C E7 FB 8B 73 16 42 4B 3C BC " +
            "DD E4 F6 DF 5E 2F E7 7D",
            // After Pi
            "C5 A5 6D E7 71 81 DC 84 CB F1 6A BE 99 1C 72 33 " +
            "F6 C9 31 84 8D 7E 0F E6 49 B8 34 85 8A 47 D8 A1 " +
            "DD E4 F6 DF 5E 2F E7 7D 93 B5 89 F5 7A 5E 5B E6 " +
            "11 3D 7A 95 A3 C7 C8 42 79 F0 94 63 80 45 E4 16 " +
            "8C DD BF AC FB 7E 9F 4C 50 38 B3 4F 29 0B 4C E7 " +
            "1B B7 32 AF 81 F8 9A E9 04 30 C9 50 3C CD 06 54 " +
            "CA 1C 8D CB A8 64 37 F9 74 B6 3D 97 00 F6 02 B6 " +
            "77 25 6E FB E4 8A D7 21 4C 56 88 03 4F 39 46 8E " +
            "6E 6D 49 66 89 A2 01 64 BA BC 21 B8 F1 CF 6D 79 " +
            "96 7A DB B3 33 28 3C 94 FB 8B 73 16 42 4B 3C BC " +
            "17 73 A4 32 90 55 27 6B 30 32 55 A4 90 A7 24 63 " +
            "7C 06 7A FA 7B 7C F2 E4 D2 49 8C F2 4E E3 F5 CA " +
            "96 2D 77 69 EC 43 C9 70",
            // After Chi  
            "F1 AD 7C E7 75 E3 D1 40 C2 C1 6E BF 9B 1D A2 32 " +
            "62 8D F3 DE D9 56 28 BA 49 B9 3D A5 AB C7 C0 21 " +
            "D7 B4 F4 C7 D6 33 C5 4E FB 75 0D 97 7A 5E 7F F2 " +
            "95 30 51 19 D8 FD D3 0A 29 D0 94 20 80 44 A4 B5 " +
            "0F 58 B7 1C A9 2A 8C 4C 50 30 C1 4F A8 8A CC E7 " +
            "D1 BB 36 24 01 D8 AB 40 30 92 F9 44 3C 5F 06 52 " +
            "C9 1D CF A3 4C 6C E2 F8 7C 24 2D 93 01 86 0A 7E " +
            "73 25 A7 AB D8 8F D3 35 DC C6 A8 9B 3F 74 2A 97 " +
            "6A 2F 93 65 8B 82 11 E0 D3 3D 01 BC B1 8C 6D 51 " +
            "92 2E 53 B2 3E 18 7E 96 D9 A2 32 72 C2 C9 3D DC " +
            "5B 77 8E 68 FB 0D F5 EF B2 7B D1 A4 94 24 21 69 " +
            "78 22 09 F3 DB 7C FA D4 D3 1B 0C E0 5E F7 D3 C1 " +
            "B6 2D 26 ED EC E1 C9 70",
            // After Iota 
            "FB AD 7C 67 75 E3 D1 C0 C2 C1 6E BF 9B 1D A2 32 " +
            "62 8D F3 DE D9 56 28 BA 49 B9 3D A5 AB C7 C0 21 " +
            "D7 B4 F4 C7 D6 33 C5 4E FB 75 0D 97 7A 5E 7F F2 " +
            "95 30 51 19 D8 FD D3 0A 29 D0 94 20 80 44 A4 B5 " +
            "0F 58 B7 1C A9 2A 8C 4C 50 30 C1 4F A8 8A CC E7 " +
            "D1 BB 36 24 01 D8 AB 40 30 92 F9 44 3C 5F 06 52 " +
            "C9 1D CF A3 4C 6C E2 F8 7C 24 2D 93 01 86 0A 7E " +
            "73 25 A7 AB D8 8F D3 35 DC C6 A8 9B 3F 74 2A 97 " +
            "6A 2F 93 65 8B 82 11 E0 D3 3D 01 BC B1 8C 6D 51 " +
            "92 2E 53 B2 3E 18 7E 96 D9 A2 32 72 C2 C9 3D DC " +
            "5B 77 8E 68 FB 0D F5 EF B2 7B D1 A4 94 24 21 69 " +
            "78 22 09 F3 DB 7C FA D4 D3 1B 0C E0 5E F7 D3 C1 " +
            "B6 2D 26 ED EC E1 C9 70",
            // Round #20  
            // After Theta
            "1F EC F2 9C 3D CF 71 36 C6 AD 4F BD AF 1D 8B DD " +
            "2B 5A 86 0C 7F 47 B8 D0 56 BB 91 CE C5 74 65 33 " +
            "00 E0 CF F1 21 8E 9A 1F 1F 34 83 6C 32 72 DF 04 " +
            "91 5C 70 1B EC FD FA E5 60 07 E1 F2 26 55 34 DF " +
            "10 5A 1B 77 C7 99 29 5E 87 64 FA 79 5F 37 93 B6 " +
            "35 FA B8 DF 49 F4 0B B6 34 FE D8 46 08 5F 2F BD " +
            "80 CA BA 71 EA 7D 72 92 63 26 81 F8 6F 35 AF 6C " +
            "A4 71 9C 9D 2F 32 8C 64 38 87 26 60 77 58 8A 61 " +
            "6E 43 B2 67 BF 82 38 0F 9A EA 74 6E 17 9D FD 3B " +
            "8D 2C FF D9 50 AB DB 84 0E F6 09 44 35 74 62 8D " +
            "BF 36 00 93 B3 21 55 19 B6 17 F0 A6 A0 24 08 86 " +
            "31 F5 7C 21 7D 6D 6A BE CC 19 A0 8B 30 44 76 D3 " +
            "61 79 1D DB 1B 5C 96 21",
            // After Rho  
            "1F EC F2 9C 3D CF 71 36 8D 5B 9F 7A 5F 3B 16 BB " +
            "8A 96 21 C3 DF 11 2E F4 4C 57 36 63 B5 1B E9 5C " +
            "71 D4 FC 00 00 7F 8E 0F 26 23 F7 4D F0 41 33 C8 " +
            "B7 C1 DE AF 5F 1E C9 05 37 D8 41 B8 BC 49 15 CD " +
            "AD 8D BB E3 CC 14 2F 08 33 69 7B 48 A6 9F F7 75 " +
            "AD D1 C7 FD 4E A2 5F B0 F4 D2 F8 63 1B 21 7C BD " +
            "8D 53 EF 93 93 04 54 D6 6A 5E D9 C6 4C 02 F1 DF " +
            "CE 17 19 46 32 D2 38 CE C0 EE B0 14 C3 70 0E 4D " +
            "F6 EC 57 10 E7 C1 6D 48 FE 1D 4D 75 3A B7 8B CE " +
            "75 9B B0 91 E5 3F 1B 6A 8D 0E F6 09 44 35 74 62 " +
            "54 65 FC DA 00 4C CE 86 DA 5E C0 9B 82 92 20 18 " +
            "A6 9E 2F A4 AF 4D CD 37 19 A0 8B 30 44 76 D3 CC " +
            "65 48 58 5E C7 F6 06 97",
            // After Pi
            "1F EC F2 9C 3D CF 71 36 B7 C1 DE AF 5F 1E C9 05 " +
            "8D 53 EF 93 93 04 54 D6 75 9B B0 91 E5 3F 1B 6A " +
            "65 48 58 5E C7 F6 06 97 4C 57 36 63 B5 1B E9 5C " +
            "33 69 7B 48 A6 9F F7 75 AD D1 C7 FD 4E A2 5F B0 " +
            "F6 EC 57 10 E7 C1 6D 48 A6 9E 2F A4 AF 4D CD 37 " +
            "8D 5B 9F 7A 5F 3B 16 BB 37 D8 41 B8 BC 49 15 CD " +
            "6A 5E D9 C6 4C 02 F1 DF 8D 0E F6 09 44 35 74 62 " +
            "54 65 FC DA 00 4C CE 86 71 D4 FC 00 00 7F 8E 0F " +
            "26 23 F7 4D F0 41 33 C8 F4 D2 F8 63 1B 21 7C BD " +
            "FE 1D 4D 75 3A B7 8B CE 19 A0 8B 30 44 76 D3 CC " +
            "8A 96 21 C3 DF 11 2E F4 AD 8D BB E3 CC 14 2F 08 " +
            "CE 17 19 46 32 D2 38 CE C0 EE B0 14 C3 70 0E 4D " +
            "DA 5E C0 9B 82 92 20 18",
            // After Chi  
            "17 FE D3 8C BD CF 65 E4 C7 49 CE AF 3B 25 C2 2D " +
            "8D 13 A7 DD 91 C4 50 43 6F 3F 12 11 DD 36 6A 4A " +
            "C5 49 54 7D 85 E6 8E 96 C0 C7 B2 D6 FD 3B E1 DC " +
            "61 45 6B 48 07 DE D7 3D AD C3 EF 59 46 AE DF 87 " +
            "BE AD 47 53 F7 D3 4D 00 95 B6 66 AC AD C9 DB 16 " +
            "C5 5D 07 3C 1F 39 F6 A9 B2 D8 67 B1 BC 7C 11 ED " +
            "3A 3F D1 14 4C 4A 7B 5B 04 14 F5 29 1B 06 64 5B " +
            "66 E5 BC 5A A0 0C CF C2 A1 04 F4 22 0B 5F C2 3A " +
            "2C 2E F2 59 D0 D7 B0 8A F5 72 7A 63 5F 61 2C BD " +
            "9E 49 39 75 3A BE 87 CD 1F 83 88 7D B4 76 E2 0C " +
            "C8 84 21 C7 ED D3 3E 32 AD 65 1B F3 0D 34 29 09 " +
            "D4 07 59 CD 32 50 18 DE C0 6E 91 54 9E 71 00 A9 " +
            "FF 57 5A BB 82 96 21 10",
            // After Iota 
            "96 7E D3 0C BD CF 65 64 C7 49 CE AF 3B 25 C2 2D " +
            "8D 13 A7 DD 91 C4 50 43 6F 3F 12 11 DD 36 6A 4A " +
            "C5 49 54 7D 85 E6 8E 96 C0 C7 B2 D6 FD 3B E1 DC " +
            "61 45 6B 48 07 DE D7 3D AD C3 EF 59 46 AE DF 87 " +
            "BE AD 47 53 F7 D3 4D 00 95 B6 66 AC AD C9 DB 16 " +
            "C5 5D 07 3C 1F 39 F6 A9 B2 D8 67 B1 BC 7C 11 ED " +
            "3A 3F D1 14 4C 4A 7B 5B 04 14 F5 29 1B 06 64 5B " +
            "66 E5 BC 5A A0 0C CF C2 A1 04 F4 22 0B 5F C2 3A " +
            "2C 2E F2 59 D0 D7 B0 8A F5 72 7A 63 5F 61 2C BD " +
            "9E 49 39 75 3A BE 87 CD 1F 83 88 7D B4 76 E2 0C " +
            "C8 84 21 C7 ED D3 3E 32 AD 65 1B F3 0D 34 29 09 " +
            "D4 07 59 CD 32 50 18 DE C0 6E 91 54 9E 71 00 A9 " +
            "FF 57 5A BB 82 96 21 10",
            // Round #21  
            // After Theta
            "6A 8F D8 B9 B8 C4 06 C7 4A 19 08 D1 6E 47 CC CD " +
            "0E CF 9D B5 E6 F9 45 D6 F8 38 11 B5 57 A0 19 0A " +
            "BA 21 3A 30 62 49 56 D0 3C 36 B9 63 F8 30 82 7F " +
            "EC 15 AD 36 52 BC D9 DD 2E 1F D5 31 31 93 CA 12 " +
            "29 AA 44 F7 7D 45 3E 40 EA DE 08 E1 4A 66 03 50 " +
            "39 AC 0C 89 1A 32 95 0A 3F 88 A1 CF E9 1E 1F 0D " +
            "B9 E3 EB 7C 3B 77 6E CE 93 13 F6 8D 91 90 17 1B " +
            "19 8D D2 17 47 A3 17 84 5D F5 FF 97 0E 54 A1 99 " +
            "A1 7E 34 27 85 B5 BE 6A 76 AE 40 0B 28 5C 39 28 " +
            "09 4E 3A D1 B0 28 F4 8D 60 EB E6 30 53 D9 3A 4A " +
            "34 75 2A 72 E8 D8 5D 91 20 35 DD 8D 58 56 27 E9 " +
            "57 DB 63 A5 45 6D 0D 4B 57 69 92 F0 14 E7 73 E9 " +
            "80 3F 34 F6 65 39 F9 56",
            // After Rho  
            "6A 8F D8 B9 B8 C4 06 C7 95 32 10 A2 DD 8E 98 9B " +
            "C3 73 67 AD 79 7E 91 B5 05 9A A1 80 8F 13 51 7B " +
            "4B B2 82 D6 0D D1 81 11 86 0F 23 F8 C7 63 93 3B " +
            "6A 23 C5 9B DD CD 5E D1 84 CB 47 75 4C CC A4 B2 " +
            "55 A2 FB BE 22 1F A0 14 36 00 A5 EE 8D 10 AE 64 " +
            "C8 61 65 48 D4 90 A9 54 34 FC 20 86 3E A7 7B 7C " +
            "E7 DB B9 73 73 CE 1D 5F 21 2F 36 26 27 EC 1B 23 " +
            "8B A3 D1 0B C2 8C 46 E9 2F 1D A8 42 33 BB EA FF " +
            "E6 A4 B0 D6 57 2D D4 8F 1C 14 3B 57 A0 05 14 AE " +
            "85 BE 31 C1 49 27 1A 16 4A 60 EB E6 30 53 D9 3A " +
            "77 45 D2 D4 A9 C8 A1 63 83 D4 74 37 62 59 9D A4 " +
            "6A 7B AC B4 A8 AD 61 E9 69 92 F0 14 E7 73 E9 57 " +
            "BE 15 E0 0F 8D 7D 59 4E",
            // After Pi
            "6A 8F D8 B9 B8 C4 06 C7 6A 23 C5 9B DD CD 5E D1 " +
            "E7 DB B9 73 73 CE 1D 5F 85 BE 31 C1 49 27 1A 16 " +
            "BE 15 E0 0F 8D 7D 59 4E 05 9A A1 80 8F 13 51 7B " +
            "36 00 A5 EE 8D 10 AE 64 C8 61 65 48 D4 90 A9 54 " +
            "E6 A4 B0 D6 57 2D D4 8F 6A 7B AC B4 A8 AD 61 E9 " +
            "95 32 10 A2 DD 8E 98 9B 84 CB 47 75 4C CC A4 B2 " +
            "21 2F 36 26 27 EC 1B 23 4A 60 EB E6 30 53 D9 3A " +
            "77 45 D2 D4 A9 C8 A1 63 4B B2 82 D6 0D D1 81 11 " +
            "86 0F 23 F8 C7 63 93 3B 34 FC 20 86 3E A7 7B 7C " +
            "1C 14 3B 57 A0 05 14 AE 69 92 F0 14 E7 73 E9 57 " +
            "C3 73 67 AD 79 7E 91 B5 55 A2 FB BE 22 1F A0 14 " +
            "8B A3 D1 0B C2 8C 46 E9 2F 1D A8 42 33 BB EA FF " +
            "83 D4 74 37 62 59 9D A4",
            // After Chi  
            "EF 57 E0 D9 9A C6 07 C9 6A 07 C5 1B D5 EC 5C D1 " +
            "DD DA 79 7D F7 96 5C 17 C5 34 29 71 79 A7 1C 97 " +
            "BE 35 E5 0D C8 74 01 5E CD FB E1 80 DF 93 50 6B " +
            "10 84 35 78 8E 3D FA EF C0 3A 69 68 7C 10 88 34 " +
            "E3 24 B1 D6 50 3F C4 9D 58 7B A8 DA A8 AD CF ED " +
            "B4 16 20 A0 FE AE 83 9A CE 8B 8E B5 5C DF 64 AA " +
            "14 2A 26 36 AE 64 3B 62 CA 52 EB C4 64 55 C1 A2 " +
            "77 8C 95 81 A9 88 85 43 7B 42 82 D0 35 55 E9 55 " +
            "8E 0F 38 A9 47 63 97 B9 55 7E E0 86 79 D5 92 2D " +
            "1E 34 39 95 A8 85 14 AE ED 9F D1 3C 25 51 FB 7D " +
            "49 72 67 AC B9 FE D7 5C 71 BE D3 FE 13 2C 08 02 " +
            "0B 63 85 3E 82 CC 53 E9 6F 3E AB CA 2A 9D EA EE " +
            "97 54 EC 25 60 58 BD A4",
            // After Iota 
            "6F D7 E0 D9 9A C6 07 49 6A 07 C5 1B D5 EC 5C D1 " +
            "DD DA 79 7D F7 96 5C 17 C5 34 29 71 79 A7 1C 97 " +
            "BE 35 E5 0D C8 74 01 5E CD FB E1 80 DF 93 50 6B " +
            "10 84 35 78 8E 3D FA EF C0 3A 69 68 7C 10 88 34 " +
            "E3 24 B1 D6 50 3F C4 9D 58 7B A8 DA A8 AD CF ED " +
            "B4 16 20 A0 FE AE 83 9A CE 8B 8E B5 5C DF 64 AA " +
            "14 2A 26 36 AE 64 3B 62 CA 52 EB C4 64 55 C1 A2 " +
            "77 8C 95 81 A9 88 85 43 7B 42 82 D0 35 55 E9 55 " +
            "8E 0F 38 A9 47 63 97 B9 55 7E E0 86 79 D5 92 2D " +
            "1E 34 39 95 A8 85 14 AE ED 9F D1 3C 25 51 FB 7D " +
            "49 72 67 AC B9 FE D7 5C 71 BE D3 FE 13 2C 08 02 " +
            "0B 63 85 3E 82 CC 53 E9 6F 3E AB CA 2A 9D EA EE " +
            "97 54 EC 25 60 58 BD A4",
            // Round #22  
            // After Theta
            "12 AC 2E 95 B1 1C B0 3E E1 A3 A6 A8 5F 4B EB 6A " +
            "AD F2 2E 85 3A 7C CE E9 44 F0 B0 75 BF ED 28 40 " +
            "6A 69 8C 3A 68 01 32 D5 B0 80 2F CC F4 49 E7 1C " +
            "9B 20 56 CB 04 9A 4D 54 B0 12 3E 90 B1 FA 1A CA " +
            "62 E0 28 D2 96 75 F0 4A 8C 27 C1 ED 08 D8 FC 66 " +
            "C9 6D EE EC D5 74 34 ED 45 2F ED 06 D6 78 D3 11 " +
            "64 02 71 CE 63 8E A9 9C 4B 96 72 C0 A2 1F F5 75 " +
            "A3 D0 FC B6 09 FD B6 C8 06 39 4C 9C 1E 8F 5E 22 " +
            "05 AB 5B 1A CD C4 20 02 25 56 B7 7E B4 3F 00 D3 " +
            "9F F0 A0 91 6E CF 20 79 39 C3 B8 0B 85 24 C8 F6 " +
            "34 09 A9 E0 92 24 60 2B FA 1A B0 4D 99 8B BF B9 " +
            "7B 4B D2 C6 4F 26 C1 17 EE FA 32 CE EC D7 DE 39 " +
            "43 08 85 12 C0 2D 8E 2F",
            // After Rho  
            "12 AC 2E 95 B1 1C B0 3E C2 47 4D 51 BF 96 D6 D5 " +
            "AB BC 4B A1 0E 9F 73 7A DB 8E 02 44 04 0F 5B F7 " +
            "0B 90 A9 56 4B 63 D4 41 4C 9F 74 CE 01 0B F8 C2 " +
            "B5 4C A0 D9 44 B5 09 62 32 AC 84 0F 64 AC BE 86 " +
            "70 14 69 CB 3A 78 25 31 CD 6F C6 78 12 DC 8E 80 " +
            "4F 6E 73 67 AF A6 A3 69 47 14 BD B4 1B 58 E3 4D " +
            "73 1E 73 4C E5 24 13 88 3F EA EB 96 2C E5 80 45 " +
            "DB 84 7E 5B E4 51 68 7E 38 3D 1E BD 44 0C 72 98 " +
            "4B A3 99 18 44 A0 60 75 80 E9 12 AB 5B 3F DA 1F " +
            "19 24 EF 13 1E 34 D2 ED F6 39 C3 B8 0B 85 24 C8 " +
            "80 AD D0 24 A4 82 4B 92 EA 6B C0 36 65 2E FE E6 " +
            "6F 49 DA F8 C9 24 F8 62 FA 32 CE EC D7 DE 39 EE " +
            "E3 CB 10 42 A1 04 70 8B",
            // After Pi
            "12 AC 2E 95 B1 1C B0 3E B5 4C A0 D9 44 B5 09 62 " +
            "73 1E 73 4C E5 24 13 88 19 24 EF 13 1E 34 D2 ED " +
            "E3 CB 10 42 A1 04 70 8B DB 8E 02 44 04 0F 5B F7 " +
            "CD 6F C6 78 12 DC 8E 80 4F 6E 73 67 AF A6 A3 69 " +
            "4B A3 99 18 44 A0 60 75 6F 49 DA F8 C9 24 F8 62 " +
            "C2 47 4D 51 BF 96 D6 D5 32 AC 84 0F 64 AC BE 86 " +
            "3F EA EB 96 2C E5 80 45 F6 39 C3 B8 0B 85 24 C8 " +
            "80 AD D0 24 A4 82 4B 92 0B 90 A9 56 4B 63 D4 41 " +
            "4C 9F 74 CE 01 0B F8 C2 47 14 BD B4 1B 58 E3 4D " +
            "80 E9 12 AB 5B 3F DA 1F FA 32 CE EC D7 DE 39 EE " +
            "AB BC 4B A1 0E 9F 73 7A 70 14 69 CB 3A 78 25 31 " +
            "DB 84 7E 5B E4 51 68 7E 38 3D 1E BD 44 0C 72 98 " +
            "EA 6B C0 36 65 2E FE E6",
            // After Chi  
            "50 BE 7D 91 10 1C A2 B6 BD 6C 2C CA 5E A5 C9 07 " +
            "91 D5 63 0C 44 24 33 8A 09 00 C1 86 0E 2C 52 D9 " +
            "46 8B 90 0A E5 A5 79 CB D9 8E 33 43 A9 2D 7A 9E " +
            "CD EE 4E 60 52 DC CE 94 6B 26 31 87 26 A2 3B 6B " +
            "DB 25 99 1C 40 AB 63 E0 6B 28 1E C0 DB F4 7C 62 " +
            "CF 05 26 C1 B7 D7 D6 94 F2 BD 84 27 67 AC 9A 0E " +
            "3F 6E FB 92 88 E7 CB 57 B4 7B CE E9 10 91 B0 8D " +
            "B0 05 50 2A E4 AA 63 90 08 90 20 66 51 33 D7 4C " +
            "CC 76 76 C5 41 2C E0 D0 3D 06 71 F0 9F 98 C2 AD " +
            "81 69 33 B9 53 1E 1E 1E BE 3D 9A 64 D7 D6 11 6C " +
            "20 3C 5D B1 CA 9E 3B 34 50 2D 69 6F 3A 74 37 B1 " +
            "19 C6 BE 59 C5 73 E4 18 39 A9 15 3C 4E 9D 73 80 " +
            "BA 6B E0 7C 55 4E FA E7",
            // After Iota 
            "51 BE 7D 11 10 1C A2 B6 BD 6C 2C CA 5E A5 C9 07 " +
            "91 D5 63 0C 44 24 33 8A 09 00 C1 86 0E 2C 52 D9 " +
            "46 8B 90 0A E5 A5 79 CB D9 8E 33 43 A9 2D 7A 9E " +
            "CD EE 4E 60 52 DC CE 94 6B 26 31 87 26 A2 3B 6B " +
            "DB 25 99 1C 40 AB 63 E0 6B 28 1E C0 DB F4 7C 62 " +
            "CF 05 26 C1 B7 D7 D6 94 F2 BD 84 27 67 AC 9A 0E " +
            "3F 6E FB 92 88 E7 CB 57 B4 7B CE E9 10 91 B0 8D " +
            "B0 05 50 2A E4 AA 63 90 08 90 20 66 51 33 D7 4C " +
            "CC 76 76 C5 41 2C E0 D0 3D 06 71 F0 9F 98 C2 AD " +
            "81 69 33 B9 53 1E 1E 1E BE 3D 9A 64 D7 D6 11 6C " +
            "20 3C 5D B1 CA 9E 3B 34 50 2D 69 6F 3A 74 37 B1 " +
            "19 C6 BE 59 C5 73 E4 18 39 A9 15 3C 4E 9D 73 80 " +
            "BA 6B E0 7C 55 4E FA E7",
            // Round #23  
            // After Theta
            "F5 86 2B A6 68 65 BA FC 10 4E F5 EE AA FB E0 C4 " +
            "33 8C FB C6 D3 83 A0 23 DB BC EE C7 0F 60 AD BF " +
            "47 27 0B 74 8C A7 51 68 7D B6 65 F4 D1 54 62 D4 " +
            "60 CC 97 44 A6 82 E7 57 C9 7F A9 4D B1 05 A8 C2 " +
            "09 99 B6 5D 41 E7 9C 86 6A 84 85 BE B2 F6 54 C1 " +
            "6B 3D 70 76 CF AE CE DE 5F 9F 5D 03 93 F2 B3 CD " +
            "9D 37 63 58 1F 40 58 FE 66 C7 E1 A8 11 DD 4F EB " +
            "B1 A9 CB 54 8D A8 4B 33 AC A8 76 D1 29 4A CF 06 " +
            "61 54 AF E1 B5 72 C9 13 9F 5F E9 3A 08 3F 51 04 " +
            "53 D5 1C F8 52 52 E1 78 BF 91 01 1A BE D4 39 CF " +
            "84 04 0B 06 B2 E7 23 7E FD 0F B0 4B CE 2A 1E 72 " +
            "BB 9F 26 93 52 D4 77 B1 EB 15 3A 7D 4F D1 8C E6 " +
            "BB C7 7B 02 3C 4C D2 44",
            // After Rho  
            "F5 86 2B A6 68 65 BA FC 21 9C EA DD 55 F7 C1 89 " +
            "0C E3 BE F1 F4 20 E8 C8 00 D6 FA BB CD EB 7E FC " +
            "3C 8D 42 3B 3A 59 A0 63 1F 4D 25 46 DD 67 5B 46 " +
            "49 64 2A 78 7E 05 C6 7C 70 F2 5F 6A 53 6C 01 AA " +
            "4C DB AE A0 73 4E C3 84 4F 15 AC 46 58 E8 2B 6B " +
            "5E EB 81 B3 7B 76 75 F6 36 7F 7D 76 0D 4C CA CF " +
            "C3 FA 00 C2 F2 EF BC 19 BA 9F D6 CD 8E C3 51 23 " +
            "AA 46 D4 A5 99 D8 D4 65 A2 53 94 9E 0D 58 51 ED " +
            "35 BC 56 2E 79 22 8C EA 28 82 CF AF 74 1D 84 9F " +
            "2A 1C 6F AA 9A 03 5F 4A CF BF 91 01 1A BE D4 39 " +
            "8F F8 11 12 2C 18 C8 9E F5 3F C0 2E 39 AB 78 C8 " +
            "F7 D3 64 52 8A FA 2E 76 15 3A 7D 4F D1 8C E6 EB " +
            "34 D1 EE F1 9E 00 0F 93",
            // After Pi
            "F5 86 2B A6 68 65 BA FC 49 64 2A 78 7E 05 C6 7C " +
            "C3 FA 00 C2 F2 EF BC 19 2A 1C 6F AA 9A 03 5F 4A " +
            "34 D1 EE F1 9E 00 0F 93 00 D6 FA BB CD EB 7E FC " +
            "4F 15 AC 46 58 E8 2B 6B 5E EB 81 B3 7B 76 75 F6 " +
            "35 BC 56 2E 79 22 8C EA F7 D3 64 52 8A FA 2E 76 " +
            "21 9C EA DD 55 F7 C1 89 70 F2 5F 6A 53 6C 01 AA " +
            "BA 9F D6 CD 8E C3 51 23 CF BF 91 01 1A BE D4 39 " +
            "8F F8 11 12 2C 18 C8 9E 3C 8D 42 3B 3A 59 A0 63 " +
            "1F 4D 25 46 DD 67 5B 46 36 7F 7D 76 0D 4C CA CF " +
            "28 82 CF AF 74 1D 84 9F 15 3A 7D 4F D1 8C E6 EB " +
            "0C E3 BE F1 F4 20 E8 C8 4C DB AE A0 73 4E C3 84 " +
            "AA 46 D4 A5 99 D8 D4 65 A2 53 94 9E 0D 58 51 ED " +
            "F5 3F C0 2E 39 AB 78 C8",
            // After Chi  
            "77 1C 2B 24 E8 8F 82 FD 61 60 45 50 76 05 85 3E " +
            "D7 3B 80 93 F6 EF BC 88 EB 1A 6E AC FA 66 EF 26 " +
            "3C B1 EE A9 88 00 4B 93 10 3C FB 0A EE FD 2A 68 " +
            "6E 01 FA 4A 58 E8 A3 63 9C A8 A1 E3 F9 AE 57 E2 " +
            "35 B8 CC 87 3C 23 DC 62 B8 D2 60 16 9A FA 2F 75 " +
            "AB 91 6A 58 D9 74 91 88 35 D2 5E 6A 43 50 85 B2 " +
            "BA DF D6 DF AA C3 59 A5 EF BB 7B CC 4B 59 D5 38 " +
            "DF 9A 04 30 2E 10 C8 BC 1C BF 1A 0B 3A 51 20 EA " +
            "17 CD A7 CF AD 76 5F 56 23 47 4D 36 8C CC A8 AF " +
            "00 07 CD 9F 5E 4C 84 9F 16 7A 58 0B 14 AA BD EF " +
            "AE E7 EE F4 7C B0 FC A9 4C CA AE BA 77 4E C2 0C " +
            "FF 6A 94 85 A9 7B FC 65 AA 93 AA 4F C9 58 D1 ED " +
            "B5 27 C0 2E 3A E5 7B CC",
            // After Iota 
            "7F 9C 2B A4 E8 8F 82 7D 61 60 45 50 76 05 85 3E " +
            "D7 3B 80 93 F6 EF BC 88 EB 1A 6E AC FA 66 EF 26 " +
            "3C B1 EE A9 88 00 4B 93 10 3C FB 0A EE FD 2A 68 " +
            "6E 01 FA 4A 58 E8 A3 63 9C A8 A1 E3 F9 AE 57 E2 " +
            "35 B8 CC 87 3C 23 DC 62 B8 D2 60 16 9A FA 2F 75 " +
            "AB 91 6A 58 D9 74 91 88 35 D2 5E 6A 43 50 85 B2 " +
            "BA DF D6 DF AA C3 59 A5 EF BB 7B CC 4B 59 D5 38 " +
            "DF 9A 04 30 2E 10 C8 BC 1C BF 1A 0B 3A 51 20 EA " +
            "17 CD A7 CF AD 76 5F 56 23 47 4D 36 8C CC A8 AF " +
            "00 07 CD 9F 5E 4C 84 9F 16 7A 58 0B 14 AA BD EF " +
            "AE E7 EE F4 7C B0 FC A9 4C CA AE BA 77 4E C2 0C " +
            "FF 6A 94 85 A9 7B FC 65 AA 93 AA 4F C9 58 D1 ED " +
            "B5 27 C0 2E 3A E5 7B CC",
            // Xor'd state (in bytes)
            "7F 9C 2B A4 E8 8F 82 7D 61 60 45 50 76 05 85 3E " +
            "D7 3B 80 93 F6 EF BC 88 EB 1A 6E AC FA 66 EF 26 " +
            "3C B1 EE A9 88 00 4B 93 10 3C FB 0A EE FD 2A 68 " +
            "6E 01 FA 4A 58 E8 A3 63 9C A8 A1 E3 F9 AE 57 E2 " +
            "35 B8 CC 87 3C 23 DC 62 B8 D2 60 16 9A FA 2F 75 " +
            "AB 91 6A 58 D9 74 91 88 35 D2 5E 6A 43 50 85 B2 " +
            "BA DF D6 DF AA C3 59 A5 EF BB 7B CC 4B 59 D5 38 " +
            "DF 9A 04 30 2E 10 C8 BC 1C BF 1A 0B 3A 51 20 EA " +
            "17 CD A7 CF AD 76 5F 56 23 47 4D 36 8C CC A8 AF " +
            "00 07 CD 9F 5E 4C 84 9F 16 7A 58 0B 14 AA BD EF " +
            "AE E7 EE F4 7C B0 FC A9 4C CA AE BA 77 4E C2 0C " +
            "FF 6A 94 85 A9 7B FC 65 AA 93 AA 4F C9 58 D1 ED " +
            "B5 27 C0 2E 3A E5 7B CC",
            // Round #0
            // After Theta
            "44 50 E8 05 94 21 95 6E 4D CB 57 61 EF 89 AC EB " +
            "80 94 15 F9 75 7A E4 20 36 32 65 E4 5F 19 DC D1 " +
            "4A EE CC 8C A0 C7 33 20 2B F0 38 AB 92 53 3D 7B " +
            "42 AA E8 7B C1 64 8A B6 CB 07 34 89 7A 3B 0F 4A " +
            "E8 90 C7 CF 99 5C EF 95 CE 8D 42 33 B2 3D 57 C6 " +
            "90 5D A9 F9 A5 DA 86 9B 19 79 4C 5B DA DC AC 67 " +
            "ED 70 43 B5 29 56 01 0D 32 93 70 84 EE 26 E6 CF " +
            "A9 C5 26 15 06 D7 B0 0F 27 73 D9 AA 46 FF 37 F9 " +
            "3B 66 B5 FE 34 FA 76 83 74 E8 D8 5C 0F 59 F0 07 " +
            "DD 2F C6 D7 FB 33 B7 68 60 25 7A 2E 3C 6D C5 5C " +
            "95 2B 2D 55 00 1E EB BA 60 61 BC 8B EE C2 EB D9 " +
            "A8 C5 01 EF 2A EE A4 CD 77 BB A1 07 6C 27 E2 1A " +
            "C3 78 E2 0B 12 22 03 7F",
            // After Rho  
            "44 50 E8 05 94 21 95 6E 9B 96 AF C2 DE 13 59 D7 " +
            "20 65 45 7E 9D 1E 39 08 95 C1 1D 6D 23 53 46 FE " +
            "3D 9E 01 51 72 67 66 04 2A 39 D5 B3 B7 02 8F B3 " +
            "BE 17 4C A6 68 2B A4 8A D2 F2 01 4D A2 DE CE 83 " +
            "C8 E3 E7 4C AE F7 4A 74 73 65 EC DC 28 34 23 DB " +
            "84 EC 4A CD 2F D5 36 DC 9E 65 E4 31 6D 69 73 B3 " +
            "AA 4D B1 0A 68 68 87 1B 4D CC 9F 65 26 E1 08 DD " +
            "0A 83 6B D8 87 D4 62 93 55 8D FE 6F F2 4F E6 B2 " +
            "D6 9F 46 DF 6E 70 C7 AC F8 03 3A 74 6C AE 87 2C " +
            "E6 16 AD FB C5 F8 7A 7F 5C 60 25 7A 2E 3C 6D C5 " +
            "AC EB 56 AE B4 54 01 78 83 85 F1 2E BA 0B AF 67 " +
            "B5 38 E0 5D C5 9D B4 19 BB A1 07 6C 27 E2 1A 77 " +
            "C0 DF 30 9E F8 82 84 C8",
            // After Pi
            "44 50 E8 05 94 21 95 6E BE 17 4C A6 68 2B A4 8A " +
            "AA 4D B1 0A 68 68 87 1B E6 16 AD FB C5 F8 7A 7F " +
            "C0 DF 30 9E F8 82 84 C8 95 C1 1D 6D 23 53 46 FE " +
            "73 65 EC DC 28 34 23 DB 84 EC 4A CD 2F D5 36 DC " +
            "D6 9F 46 DF 6E 70 C7 AC B5 38 E0 5D C5 9D B4 19 " +
            "9B 96 AF C2 DE 13 59 D7 D2 F2 01 4D A2 DE CE 83 " +
            "4D CC 9F 65 26 E1 08 DD 5C 60 25 7A 2E 3C 6D C5 " +
            "AC EB 56 AE B4 54 01 78 3D 9E 01 51 72 67 66 04 " +
            "2A 39 D5 B3 B7 02 8F B3 9E 65 E4 31 6D 69 73 B3 " +
            "F8 03 3A 74 6C AE 87 2C BB A1 07 6C 27 E2 1A 77 " +
            "20 65 45 7E 9D 1E 39 08 C8 E3 E7 4C AE F7 4A 74 " +
            "0A 83 6B D8 87 D4 62 93 55 8D FE 6F F2 4F E6 B2 " +
            "83 85 F1 2E BA 0B AF 67",
            // After Chi  
            "44 18 59 0D 94 61 96 7F FA 05 40 57 ED BB DC EE " +
            "AA 84 A1 0E 50 6A 03 9B E2 16 65 FA C1 D9 6B 59 " +
            "7A D8 34 3C 90 88 A4 48 11 49 1F 6C 24 92 52 FA " +
            "21 76 E8 CE 68 14 E2 FB A5 CC EA CD AE 58 06 CD " +
            "D6 5E 5B FF 4C 32 85 4A D7 1C 00 CD CD B9 95 18 " +
            "96 9A 31 E2 DA 32 59 8B C2 D2 21 57 AA C2 AB 83 " +
            "ED 47 CD E1 B6 A1 08 E5 4F 74 8C 3A 64 3F 35 42 " +
            "EC 8B 56 A3 94 98 87 78 A9 DA 21 51 3A 0E 16 04 " +
            "4A 3B CF F7 B7 84 0B BF 9D C5 E1 39 6E 29 6B E0 " +
            "FC 1D 3A 65 3C AB E3 2C B9 80 D3 CE A2 E2 93 C4 " +
            "22 65 4D EE 9C 1E 19 8B 9D EF 73 6B DE FC CE 54 " +
            "88 83 6A D8 8F D4 6B D6 75 ED FA 3F F7 5B F6 BA " +
            "4B 07 53 2E 98 EA ED 13",
            // After Iota 
            "45 18 59 0D 94 61 96 7F FA 05 40 57 ED BB DC EE " +
            "AA 84 A1 0E 50 6A 03 9B E2 16 65 FA C1 D9 6B 59 " +
            "7A D8 34 3C 90 88 A4 48 11 49 1F 6C 24 92 52 FA " +
            "21 76 E8 CE 68 14 E2 FB A5 CC EA CD AE 58 06 CD " +
            "D6 5E 5B FF 4C 32 85 4A D7 1C 00 CD CD B9 95 18 " +
            "96 9A 31 E2 DA 32 59 8B C2 D2 21 57 AA C2 AB 83 " +
            "ED 47 CD E1 B6 A1 08 E5 4F 74 8C 3A 64 3F 35 42 " +
            "EC 8B 56 A3 94 98 87 78 A9 DA 21 51 3A 0E 16 04 " +
            "4A 3B CF F7 B7 84 0B BF 9D C5 E1 39 6E 29 6B E0 " +
            "FC 1D 3A 65 3C AB E3 2C B9 80 D3 CE A2 E2 93 C4 " +
            "22 65 4D EE 9C 1E 19 8B 9D EF 73 6B DE FC CE 54 " +
            "88 83 6A D8 8F D4 6B D6 75 ED FA 3F F7 5B F6 BA " +
            "4B 07 53 2E 98 EA ED 13",
            // Round #1
            // After Theta
            "6A 3B D1 1B EB EA FE 7A 5C E2 41 ED 72 B7 54 65 " +
            "81 68 71 96 52 37 CF 69 72 CE AD 5C 8F F4 F7 23 " +
            "1B FC 70 21 2A 0F 4F 8C 3E 6A 97 7A 5B 19 3A FF " +
            "87 91 E9 74 F7 18 6A 70 8E 20 3A 55 AC 05 CA 3F " +
            "46 86 93 59 02 1F 19 30 B6 38 44 D0 77 3E 7E DC " +
            "B9 B9 B9 F4 A5 B9 31 8E 64 35 20 ED 35 CE 23 08 " +
            "C6 AB 1D 79 B4 FC C4 17 DF AC 44 9C 2A 12 A9 38 " +
            "8D AF 12 BE 2E 1F 6C BC 86 F9 A9 47 45 85 7E 01 " +
            "EC DC CE 4D 28 88 83 34 B6 29 31 A1 6C 74 A7 12 " +
            "6C C5 F2 C3 72 86 7F 56 D8 A4 97 D3 18 65 78 00 " +
            "0D 46 C5 F8 E3 95 71 8E 3B 08 72 D1 41 F0 46 DF " +
            "A3 6F BA 40 8D 89 A7 24 E5 35 32 99 B9 76 6A C0 " +
            "2A 23 17 33 22 6D 06 D7",
            // After Rho  
            "6A 3B D1 1B EB EA FE 7A B8 C4 83 DA E5 6E A9 CA " +
            "20 5A 9C A5 D4 CD 73 5A 48 7F 3F 22 E7 DC CA F5 " +
            "79 78 62 DC E0 87 0B 51 B7 95 A1 F3 EF A3 76 A9 " +
            "4E 77 8F A1 06 77 18 99 8F 23 88 4E 15 6B 81 F2 " +
            "C3 C9 2C 81 8F 0C 18 23 E3 C7 6D 8B 43 04 7D E7 " +
            "CC CD CD A5 2F CD 8D 71 20 90 D5 80 B4 D7 38 8F " +
            "C8 A3 E5 27 BE 30 5E ED 24 52 71 BE 59 89 38 55 " +
            "5F 97 0F 36 DE C6 57 09 8F 8A 0A FD 02 0C F3 53 " +
            "B9 09 05 71 90 86 9D DB 53 09 DB 94 98 50 36 BA " +
            "F0 CF 8A AD 58 7E 58 CE 00 D8 A4 97 D3 18 65 78 " +
            "C6 39 36 18 15 E3 8F 57 EF 20 C8 45 07 C1 1B 7D " +
            "F4 4D 17 A8 31 F1 94 64 35 32 99 B9 76 6A C0 E5 " +
            "C1 B5 CA C8 C5 8C 48 9B",
            // After Pi
            "6A 3B D1 1B EB EA FE 7A 4E 77 8F A1 06 77 18 99 " +
            "C8 A3 E5 27 BE 30 5E ED F0 CF 8A AD 58 7E 58 CE " +
            "C1 B5 CA C8 C5 8C 48 9B 48 7F 3F 22 E7 DC CA F5 " +
            "E3 C7 6D 8B 43 04 7D E7 CC CD CD A5 2F CD 8D 71 " +
            "B9 09 05 71 90 86 9D DB F4 4D 17 A8 31 F1 94 64 " +
            "B8 C4 83 DA E5 6E A9 CA 8F 23 88 4E 15 6B 81 F2 " +
            "24 52 71 BE 59 89 38 55 00 D8 A4 97 D3 18 65 78 " +
            "C6 39 36 18 15 E3 8F 57 79 78 62 DC E0 87 0B 51 " +
            "B7 95 A1 F3 EF A3 76 A9 20 90 D5 80 B4 D7 38 8F " +
            "53 09 DB 94 98 50 36 BA 35 32 99 B9 76 6A C0 E5 " +
            "20 5A 9C A5 D4 CD 73 5A C3 C9 2C 81 8F 0C 18 23 " +
            "5F 97 0F 36 DE C6 57 09 8F 8A 0A FD 02 0C F3 53 " +
            "EF 20 C8 45 07 C1 1B 7D",
            // After Chi  
            "EA BB B1 1D 53 EA B8 1E 7E 3B 85 29 46 39 18 9B " +
            "C9 93 A5 67 3B B0 5E FC DA C5 9B BE 72 1C EE AE " +
            "C5 F1 C4 68 C1 99 48 1A 44 77 BF 06 CB 15 4A E5 " +
            "D2 C7 6D DB D3 06 6D 6D 88 89 DF 2D 0E BC 8D 55 " +
            "B1 3B 2D 73 56 8A D7 4A 57 CD 57 21 31 F1 A1 66 " +
            "98 94 F2 6A AD EE 91 CF 8F AB 0C 4F 97 7B C4 DA " +
            "E2 73 63 B6 5D 6A B2 52 38 1C 25 55 33 14 45 F0 " +
            "C1 1A 3E 1C 05 E2 8F 67 79 78 36 DC F0 D3 03 57 " +
            "E4 9C AB E7 E7 A3 70 99 04 A2 D5 A9 D2 FD F8 CA " +
            "1B 41 B9 D0 18 D5 3D AA B3 B7 18 9A 79 4A B4 4D " +
            "3C 4C 9F 93 84 0F 34 52 43 C1 2C 48 8F 04 B8 71 " +
            "3F B7 CF 36 DB 07 5F 25 8F D0 1E 5D D2 00 93 51 " +
            "2C A1 E8 45 0C C1 13 5C",
            // After Iota 
            "68 3B B1 1D 53 EA B8 1E 7E 3B 85 29 46 39 18 9B " +
            "C9 93 A5 67 3B B0 5E FC DA C5 9B BE 72 1C EE AE " +
            "C5 F1 C4 68 C1 99 48 1A 44 77 BF 06 CB 15 4A E5 " +
            "D2 C7 6D DB D3 06 6D 6D 88 89 DF 2D 0E BC 8D 55 " +
            "B1 3B 2D 73 56 8A D7 4A 57 CD 57 21 31 F1 A1 66 " +
            "98 94 F2 6A AD EE 91 CF 8F AB 0C 4F 97 7B C4 DA " +
            "E2 73 63 B6 5D 6A B2 52 38 1C 25 55 33 14 45 F0 " +
            "C1 1A 3E 1C 05 E2 8F 67 79 78 36 DC F0 D3 03 57 " +
            "E4 9C AB E7 E7 A3 70 99 04 A2 D5 A9 D2 FD F8 CA " +
            "1B 41 B9 D0 18 D5 3D AA B3 B7 18 9A 79 4A B4 4D " +
            "3C 4C 9F 93 84 0F 34 52 43 C1 2C 48 8F 04 B8 71 " +
            "3F B7 CF 36 DB 07 5F 25 8F D0 1E 5D D2 00 93 51 " +
            "2C A1 E8 45 0C C1 13 5C",
            // Round #2
            // After Theta
            "AD 1E 2A B3 07 2D 8A 9C BF 2E D6 D1 C5 CC C1 83 " +
            "C2 7E AE 5F EB FC 83 E7 DA D8 22 C9 12 83 AA AF " +
            "E0 5B 5B 01 9E 54 33 97 81 52 24 A8 9F D2 78 67 " +
            "13 D2 3E 23 50 F3 B4 75 83 64 D4 15 DE F0 50 4E " +
            "B1 26 94 04 36 15 93 4B 72 67 C8 48 6E 3C DA EB " +
            "5D B1 69 C4 F9 29 A3 4D 4E BE 5F B7 14 8E 1D C2 " +
            "E9 9E 68 8E 8D 26 6F 49 38 01 9C 22 53 8B 01 F1 " +
            "E4 B0 A1 75 5A 2F F4 EA BC 5D AD 72 A4 14 31 D5 " +
            "25 89 F8 1F 64 56 A9 81 0F 4F DE 91 02 B1 25 D1 " +
            "1B 5C 00 A7 78 4A 79 AB 96 1D 87 F3 26 87 CF C0 " +
            "F9 69 04 3D D0 C8 06 D0 82 D4 7F B0 0C F1 61 69 " +
            "34 5A C4 0E 0B 4B 82 3E 8F CD A7 2A B2 9F D7 50 " +
            "09 0B 77 2C 53 0C 68 D1",
            // After Rho  
            "AD 1E 2A B3 07 2D 8A 9C 7F 5D AC A3 8B 99 83 07 " +
            "B0 9F EB D7 3A FF E0 B9 31 A8 FA AA 8D 2D 92 2C " +
            "A4 9A B9 04 DF DA 0A F0 FA 29 8D 77 16 28 45 82 " +
            "33 02 35 4F 5B 37 21 ED D3 20 19 75 85 37 3C 94 " +
            "13 4A 02 9B 8A C9 A5 58 A3 BD 2E 77 86 8C E4 C6 " +
            "EA 8A 4D 23 CE 4F 19 6D 08 3B F9 7E DD 52 38 76 " +
            "73 6C 34 79 4B 4A F7 44 16 03 E2 71 02 38 45 A6 " +
            "3A AD 17 7A 75 72 D8 D0 E5 48 29 62 AA 79 BB 5A " +
            "FF 83 CC 2A 35 B0 24 11 92 E8 87 27 EF 48 81 D8 " +
            "29 6F 75 83 0B E0 14 4F C0 96 1D 87 F3 26 87 CF " +
            "1B 40 E7 A7 11 F4 40 23 09 52 FF C1 32 C4 87 A5 " +
            "46 8B D8 61 61 49 D0 87 CD A7 2A B2 9F D7 50 8F " +
            "5A 74 C2 C2 1D CB 14 03",
            // After Pi
            "AD 1E 2A B3 07 2D 8A 9C 33 02 35 4F 5B 37 21 ED " +
            "73 6C 34 79 4B 4A F7 44 29 6F 75 83 0B E0 14 4F " +
            "5A 74 C2 C2 1D CB 14 03 31 A8 FA AA 8D 2D 92 2C " +
            "A3 BD 2E 77 86 8C E4 C6 EA 8A 4D 23 CE 4F 19 6D " +
            "FF 83 CC 2A 35 B0 24 11 46 8B D8 61 61 49 D0 87 " +
            "7F 5D AC A3 8B 99 83 07 D3 20 19 75 85 37 3C 94 " +
            "16 03 E2 71 02 38 45 A6 C0 96 1D 87 F3 26 87 CF " +
            "1B 40 E7 A7 11 F4 40 23 A4 9A B9 04 DF DA 0A F0 " +
            "FA 29 8D 77 16 28 45 82 08 3B F9 7E DD 52 38 76 " +
            "92 E8 87 27 EF 48 81 D8 CD A7 2A B2 9F D7 50 8F " +
            "B0 9F EB D7 3A FF E0 B9 13 4A 02 9B 8A C9 A5 58 " +
            "3A AD 17 7A 75 72 D8 D0 E5 48 29 62 AA 79 BB 5A " +
            "09 52 FF C1 32 C4 87 A5",
            // After Chi  
            "ED 72 2A 83 07 65 5C 9C 3B 01 74 CD 5B 97 21 E6 " +
            "21 7C B6 39 5F 41 F7 44 8C 65 5D B2 09 C4 9E D3 " +
            "48 74 D7 8E 45 D9 35 62 79 AA BB AA C5 6E 8B 05 " +
            "B6 BC AE 7F B7 3C C0 D6 EA 82 5D 62 8E 06 C9 EB " +
            "CE A3 EE A0 B9 94 26 39 C4 9E DC 34 63 C9 B4 45 " +
            "7B 5E 4E A3 89 91 C2 25 13 B4 04 F3 74 31 BE DD " +
            "0D 43 00 51 02 E8 05 86 A4 8B 15 87 79 2F 04 CB " +
            "9B 60 F6 F3 15 D2 7C B3 A4 88 C9 0C 16 88 32 84 " +
            "68 E9 8B 76 34 20 C4 0A 45 3C D1 EE CD C5 68 71 " +
            "B2 F0 16 23 AF 40 8B A8 97 86 2E C1 9F F7 15 8D " +
            "98 3A FE B7 4F CD B8 39 D6 0A 2A 9B 00 C0 86 52 " +
            "32 BF C1 FB 65 F6 DC 75 55 C5 29 74 A2 42 DB 42 " +
            "0A 12 FF C9 B2 C4 82 E5",
            // After Iota 
            "67 F2 2A 83 07 65 5C 1C 3B 01 74 CD 5B 97 21 E6 " +
            "21 7C B6 39 5F 41 F7 44 8C 65 5D B2 09 C4 9E D3 " +
            "48 74 D7 8E 45 D9 35 62 79 AA BB AA C5 6E 8B 05 " +
            "B6 BC AE 7F B7 3C C0 D6 EA 82 5D 62 8E 06 C9 EB " +
            "CE A3 EE A0 B9 94 26 39 C4 9E DC 34 63 C9 B4 45 " +
            "7B 5E 4E A3 89 91 C2 25 13 B4 04 F3 74 31 BE DD " +
            "0D 43 00 51 02 E8 05 86 A4 8B 15 87 79 2F 04 CB " +
            "9B 60 F6 F3 15 D2 7C B3 A4 88 C9 0C 16 88 32 84 " +
            "68 E9 8B 76 34 20 C4 0A 45 3C D1 EE CD C5 68 71 " +
            "B2 F0 16 23 AF 40 8B A8 97 86 2E C1 9F F7 15 8D " +
            "98 3A FE B7 4F CD B8 39 D6 0A 2A 9B 00 C0 86 52 " +
            "32 BF C1 FB 65 F6 DC 75 55 C5 29 74 A2 42 DB 42 " +
            "0A 12 FF C9 B2 C4 82 E5",
            // Round #3
            // After Theta
            "AC 38 F9 9A 40 61 0C 8A 00 C8 6A C3 BF 70 A1 3C " +
            "02 66 FB 10 7A C0 32 66 28 66 FE 2F 4E BA C4 06 " +
            "FA 64 9F 2F A5 1A E6 AA B2 60 68 B3 82 6A DB 93 " +
            "8D 75 B0 71 53 DB 40 0C C9 98 10 4B AB 87 0C C9 " +
            "6A A0 4D 3D FE EA 7C EC 76 8E 94 95 83 0A 67 8D " +
            "B0 94 9D BA CE 95 92 B3 28 7D 1A FD 90 D6 3E 07 " +
            "2E 59 4D 78 27 69 C0 A4 00 88 B6 1A 3E 51 5E 1E " +
            "29 70 BE 52 F5 11 AF 7B 6F 42 1A 15 51 8C 62 12 " +
            "53 20 95 78 D0 C7 44 D0 66 26 9C C7 E8 44 AD 53 " +
            "16 F3 B5 BE E8 3E D1 7D 25 96 66 60 7F 34 C6 45 " +
            "53 F0 2D AE 08 C9 E8 AF ED C3 34 95 E4 27 06 88 " +
            "11 A5 8C D2 40 77 19 57 F1 C6 8A E9 E5 3C 81 97 " +
            "B8 02 B7 68 52 07 51 2D",
            // After Rho  
            "AC 38 F9 9A 40 61 0C 8A 00 90 D5 86 7F E1 42 79 " +
            "80 D9 3E 84 1E B0 8C 99 A4 4B 6C 80 62 E6 FF E2 " +
            "D5 30 57 D5 27 FB 7C 29 2B A8 B6 3D 29 0B 86 36 " +
            "1B 37 B5 0D C4 D0 58 07 72 32 26 C4 D2 EA 21 43 " +
            "D0 A6 1E 7F 75 3E 76 35 70 D6 68 E7 48 59 39 A8 " +
            "85 A5 EC D4 75 AE 94 9C 1C A0 F4 69 F4 43 5A FB " +
            "C2 3B 49 03 26 75 C9 6A A2 BC 3C 00 10 6D 35 7C " +
            "A9 FA 88 D7 BD 14 38 5F 2A A2 18 C5 24 DE 84 34 " +
            "12 0F FA 98 08 7A 0A A4 D6 29 33 13 CE 63 74 A2 " +
            "27 BA CF 62 BE D6 17 DD 45 25 96 66 60 7F 34 C6 " +
            "A3 BF 4E C1 B7 B8 22 24 B6 0F D3 54 92 9F 18 20 " +
            "A2 94 51 1A E8 2E E3 2A C6 8A E9 E5 3C 81 97 F1 " +
            "54 0B AE C0 2D 9A D4 41",
            // After Pi
            "AC 38 F9 9A 40 61 0C 8A 1B 37 B5 0D C4 D0 58 07 " +
            "C2 3B 49 03 26 75 C9 6A 27 BA CF 62 BE D6 17 DD " +
            "54 0B AE C0 2D 9A D4 41 A4 4B 6C 80 62 E6 FF E2 " +
            "70 D6 68 E7 48 59 39 A8 85 A5 EC D4 75 AE 94 9C " +
            "12 0F FA 98 08 7A 0A A4 A2 94 51 1A E8 2E E3 2A " +
            "00 90 D5 86 7F E1 42 79 72 32 26 C4 D2 EA 21 43 " +
            "A2 BC 3C 00 10 6D 35 7C 45 25 96 66 60 7F 34 C6 " +
            "A3 BF 4E C1 B7 B8 22 24 D5 30 57 D5 27 FB 7C 29 " +
            "2B A8 B6 3D 29 0B 86 36 1C A0 F4 69 F4 43 5A FB " +
            "D6 29 33 13 CE 63 74 A2 C6 8A E9 E5 3C 81 97 F1 " +
            "80 D9 3E 84 1E B0 8C 99 D0 A6 1E 7F 75 3E 76 35 " +
            "A9 FA 88 D7 BD 14 38 5F 2A A2 18 C5 24 DE 84 34 " +
            "B6 0F D3 54 92 9F 18 20",
            // After Chi  
            "6C 30 B1 98 62 44 8D E2 3E B7 33 6D 5C 52 4E 92 " +
            "92 3A 69 83 27 7D 09 6A 8F 8A 9E 78 FE B7 1F 57 " +
            "47 0C AA C5 A9 0A 84 44 21 6A E8 90 57 40 7B F6 " +
            "62 DC 7A EF 40 09 33 88 25 35 ED D6 95 AA 75 96 " +
            "16 44 D6 18 0A BA 16 64 F2 00 51 7D E0 37 E3 22 " +
            "80 1C CD 86 7F E4 56 45 37 33 A4 A2 B2 F8 21 C1 " +
            "00 26 74 81 87 ED 37 5C 45 25 07 60 28 3E 74 9F " +
            "D1 9D 6C 81 37 B2 03 26 C1 30 17 95 F3 BB 24 E0 " +
            "E9 A1 B5 2F 23 2B A2 36 1C 22 3C 8D C4 C3 D9 AA " +
            "C7 19 25 03 CD 19 1C AA EC 02 49 CD 34 81 15 E7 " +
            "A9 81 BE 04 96 B0 84 D3 D2 A6 0E 7F 75 F4 F2 15 " +
            "3D F7 4B C7 2F 15 20 5F 2A 72 34 45 28 FE 00 AD " +
            "E6 29 D3 2F F3 91 6A 04",
            // After Iota 
            "6C B0 B1 18 62 44 8D 62 3E B7 33 6D 5C 52 4E 92 " +
            "92 3A 69 83 27 7D 09 6A 8F 8A 9E 78 FE B7 1F 57 " +
            "47 0C AA C5 A9 0A 84 44 21 6A E8 90 57 40 7B F6 " +
            "62 DC 7A EF 40 09 33 88 25 35 ED D6 95 AA 75 96 " +
            "16 44 D6 18 0A BA 16 64 F2 00 51 7D E0 37 E3 22 " +
            "80 1C CD 86 7F E4 56 45 37 33 A4 A2 B2 F8 21 C1 " +
            "00 26 74 81 87 ED 37 5C 45 25 07 60 28 3E 74 9F " +
            "D1 9D 6C 81 37 B2 03 26 C1 30 17 95 F3 BB 24 E0 " +
            "E9 A1 B5 2F 23 2B A2 36 1C 22 3C 8D C4 C3 D9 AA " +
            "C7 19 25 03 CD 19 1C AA EC 02 49 CD 34 81 15 E7 " +
            "A9 81 BE 04 96 B0 84 D3 D2 A6 0E 7F 75 F4 F2 15 " +
            "3D F7 4B C7 2F 15 20 5F 2A 72 34 45 28 FE 00 AD " +
            "E6 29 D3 2F F3 91 6A 04",
            // Round #4
            // After Theta
            "A3 B4 10 23 2B 22 8E 31 B7 39 01 CF CE 60 2B DB " +
            "A1 65 82 7F AD A9 C6 C4 C4 02 02 50 53 64 9A 44 " +
            "3D 63 8E BD CF 08 E4 2B EE 6E 49 AB 1E 26 78 A5 " +
            "EB 52 48 4D D2 3B 56 C1 16 6A 06 2A 1F 7E BA 38 " +
            "5D CC 4A 30 A7 69 93 77 88 6F 75 05 86 35 83 4D " +
            "4F 18 6C BD 36 82 55 16 BE BD 96 00 20 CA 44 88 " +
            "33 79 9F 7D 0D 39 F8 F2 0E AD 9B 48 85 ED F1 8C " +
            "AB F2 48 F9 51 B0 63 49 0E 34 B6 AE BA DD 27 B3 " +
            "60 2F 87 8D B1 19 C7 7F 2F 7D D7 71 4E 17 16 04 " +
            "8C 91 B9 2B 60 CA 99 B9 96 6D 6D B5 52 83 75 88 " +
            "66 85 1F 3F DF D6 87 80 5B 28 3C DD E7 C6 97 5C " +
            "0E A8 A0 3B A5 C1 EF F1 61 FA A8 6D 85 2D 85 BE " +
            "9C 46 F7 57 95 93 0A 6B",
            // After Rho  
            "A3 B4 10 23 2B 22 8E 31 6F 73 02 9E 9D C1 56 B6 " +
            "68 99 E0 5F 6B AA 31 71 45 A6 49 44 2C 20 00 35 " +
            "46 20 5F E9 19 73 EC 7D EA 61 82 57 EA EE 96 B4 " +
            "D4 24 BD 63 15 BC 2E 85 8E 85 9A 81 CA 87 9F 2E " +
            "66 25 98 D3 B4 C9 BB 2E 33 D8 84 F8 56 57 60 58 " +
            "78 C2 60 EB B5 11 AC B2 21 FA F6 5A 02 80 28 13 " +
            "EC 6B C8 C1 97 9F C9 FB DB E3 19 1D 5A 37 91 0A " +
            "FC 28 D8 B1 A4 55 79 A4 5D 75 BB 4F 66 1D 68 6C " +
            "B0 31 36 E3 F8 0F EC E5 0B 82 97 BE EB 38 A7 0B " +
            "39 33 97 31 32 77 05 4C 88 96 6D 6D B5 52 83 75 " +
            "1F 02 9A 15 7E FC 7C 5B 6D A1 F0 74 9F 1B 5F 72 " +
            "01 15 74 A7 34 F8 3D DE FA A8 6D 85 2D 85 BE 61 " +
            "C2 1A A7 D1 FD 55 E5 A4",
            // After Pi
            "A3 B4 10 23 2B 22 8E 31 D4 24 BD 63 15 BC 2E 85 " +
            "EC 6B C8 C1 97 9F C9 FB 39 33 97 31 32 77 05 4C " +
            "C2 1A A7 D1 FD 55 E5 A4 45 A6 49 44 2C 20 00 35 " +
            "33 D8 84 F8 56 57 60 58 78 C2 60 EB B5 11 AC B2 " +
            "B0 31 36 E3 F8 0F EC E5 01 15 74 A7 34 F8 3D DE " +
            "6F 73 02 9E 9D C1 56 B6 8E 85 9A 81 CA 87 9F 2E " +
            "DB E3 19 1D 5A 37 91 0A 88 96 6D 6D B5 52 83 75 " +
            "1F 02 9A 15 7E FC 7C 5B 46 20 5F E9 19 73 EC 7D " +
            "EA 61 82 57 EA EE 96 B4 21 FA F6 5A 02 80 28 13 " +
            "0B 82 97 BE EB 38 A7 0B FA A8 6D 85 2D 85 BE 61 " +
            "68 99 E0 5F 6B AA 31 71 66 25 98 D3 B4 C9 BB 2E " +
            "FC 28 D8 B1 A4 55 79 A4 5D 75 BB 4F 66 1D 68 6C " +
            "6D A1 F0 74 9F 1B 5F 72",
            // After Chi  
            "8B FF 50 A3 A9 21 4F 4B C5 34 AA 53 35 DC 2A 81 " +
            "2E 63 E8 01 5A 9F 29 5B 18 97 87 13 30 55 0F 5D " +
            "96 1A 0A 91 E9 C9 C5 20 0D A4 29 47 8D 20 8C 97 " +
            "B3 E9 92 F8 1E 59 20 1D 79 C6 20 EF B1 E1 BD A8 " +
            "F4 93 3F A3 F0 0F EC C4 33 4D F0 1F 66 AF 5D 96 " +
            "3E 11 03 82 8D F1 56 B6 8E 91 FE E1 6F C7 9D 5B " +
            "CC E3 8B 0D 10 9B ED 00 E8 E7 6D E7 34 53 81 D1 " +
            "9F 86 02 14 3C FA F5 53 47 BA 2B E1 19 73 C4 7E " +
            "E0 61 83 F3 03 D6 11 BC D1 D2 9E 5B 06 05 30 73 " +
            "0F 82 85 D6 FB 4A E7 17 52 E9 ED 93 CF 09 AC E1 " +
            "F0 91 A0 7F 6B BE 71 F1 67 70 BB 9D F6 C1 BB 66 " +
            "DC A8 98 81 3D 57 6E B6 5D 6D BB 44 06 BD 48 6D " +
            "6B 85 E8 F4 0B 5A D5 7C",
            // After Iota 
            "00 7F 50 A3 A9 21 4F 4B C5 34 AA 53 35 DC 2A 81 " +
            "2E 63 E8 01 5A 9F 29 5B 18 97 87 13 30 55 0F 5D " +
            "96 1A 0A 91 E9 C9 C5 20 0D A4 29 47 8D 20 8C 97 " +
            "B3 E9 92 F8 1E 59 20 1D 79 C6 20 EF B1 E1 BD A8 " +
            "F4 93 3F A3 F0 0F EC C4 33 4D F0 1F 66 AF 5D 96 " +
            "3E 11 03 82 8D F1 56 B6 8E 91 FE E1 6F C7 9D 5B " +
            "CC E3 8B 0D 10 9B ED 00 E8 E7 6D E7 34 53 81 D1 " +
            "9F 86 02 14 3C FA F5 53 47 BA 2B E1 19 73 C4 7E " +
            "E0 61 83 F3 03 D6 11 BC D1 D2 9E 5B 06 05 30 73 " +
            "0F 82 85 D6 FB 4A E7 17 52 E9 ED 93 CF 09 AC E1 " +
            "F0 91 A0 7F 6B BE 71 F1 67 70 BB 9D F6 C1 BB 66 " +
            "DC A8 98 81 3D 57 6E B6 5D 6D BB 44 06 BD 48 6D " +
            "6B 85 E8 F4 0B 5A D5 7C",
            // Round #5
            // After Theta
            "FD 78 51 17 BC 45 21 09 6D AC D1 D9 6E 8E 45 08 " +
            "FD 26 C0 AE F8 36 8F 23 88 D1 39 D1 1F 7C 01 9B " +
            "C9 D5 02 A5 57 4C 48 D8 F0 A3 28 F3 98 44 E2 D5 " +
            "1B 71 E9 72 45 0B 4F 94 AA 83 08 40 13 48 1B D0 " +
            "64 D5 81 61 DF 26 E2 02 6C 82 F8 2B D8 2A D0 6E " +
            "C3 16 02 36 98 95 38 F4 26 09 85 6B 34 95 F2 D2 " +
            "1F A6 A3 A2 B2 32 4B 78 78 A1 D3 25 1B 7A 8F 17 " +
            "C0 49 0A 20 82 7F 78 AB BA BD 2A 55 0C 17 AA 3C " +
            "48 F9 F8 79 58 84 7E 35 02 97 B6 F4 A4 AC 96 0B " +
            "9F C4 3B 14 D4 63 E9 D1 0D 26 E5 A7 71 8C 21 19 " +
            "0D 96 A1 CB 7E DA 1F B3 CF E8 C0 17 AD 93 D4 EF " +
            "0F ED B0 2E 9F FE C8 CE CD 2B 05 86 29 94 46 AB " +
            "34 4A E0 C0 B5 DF 58 84",
            // After Rho  
            "FD 78 51 17 BC 45 21 09 DA 58 A3 B3 DD 1C 8B 10 " +
            "BF 09 B0 2B BE CD E3 48 C1 17 B0 89 18 9D 13 FD " +
            "62 42 C2 4E AE 16 28 BD 8F 49 24 5E 0D 3F 8A 32 " +
            "2E 57 B4 F0 44 B9 11 97 B4 EA 20 02 D0 04 D2 06 " +
            "EA C0 B0 6F 13 71 01 B2 02 ED C6 26 88 BF 82 AD " +
            "1F B6 10 B0 C1 AC C4 A1 4B 9B 24 14 AE D1 54 CA " +
            "15 95 95 59 C2 FB 30 1D F4 1E 2F F0 42 A7 4B 36 " +
            "10 C1 3F BC 55 E0 24 05 AA 18 2E 54 79 74 7B 55 " +
            "3F 0F 8B D0 AF 06 29 1F CB 05 81 4B 5B 7A 52 56 " +
            "2C 3D FA 93 78 87 82 7A 19 0D 26 E5 A7 71 8C 21 " +
            "7F CC 36 58 86 2E FB 69 3F A3 03 5F B4 4E 52 BF " +
            "A1 1D D6 E5 D3 1F D9 F9 2B 05 86 29 94 46 AB CD " +
            "16 21 8D 12 38 70 ED 37",
            // After Pi
            "FD 78 51 17 BC 45 21 09 2E 57 B4 F0 44 B9 11 97 " +
            "15 95 95 59 C2 FB 30 1D 2C 3D FA 93 78 87 82 7A " +
            "16 21 8D 12 38 70 ED 37 C1 17 B0 89 18 9D 13 FD " +
            "02 ED C6 26 88 BF 82 AD 1F B6 10 B0 C1 AC C4 A1 " +
            "3F 0F 8B D0 AF 06 29 1F A1 1D D6 E5 D3 1F D9 F9 " +
            "DA 58 A3 B3 DD 1C 8B 10 B4 EA 20 02 D0 04 D2 06 " +
            "F4 1E 2F F0 42 A7 4B 36 19 0D 26 E5 A7 71 8C 21 " +
            "7F CC 36 58 86 2E FB 69 62 42 C2 4E AE 16 28 BD " +
            "8F 49 24 5E 0D 3F 8A 32 4B 9B 24 14 AE D1 54 CA " +
            "CB 05 81 4B 5B 7A 52 56 2B 05 86 29 94 46 AB CD " +
            "BF 09 B0 2B BE CD E3 48 EA C0 B0 6F 13 71 01 B2 " +
            "10 C1 3F BC 55 E0 24 05 AA 18 2E 54 79 74 7B 55 " +
            "3F A3 03 5F B4 4E 52 BF",
            // After Chi  
            "EC F8 50 1E 3E 07 01 01 06 7F DE 72 7C BD 93 F5 " +
            "07 95 90 59 C2 8B 5D 18 C5 65 AA 96 FC 82 82 72 " +
            "14 26 29 F2 78 C8 FD A1 DC 05 A0 19 59 9D 57 FD " +
            "22 E4 4D 66 A6 BD AB B3 9F A6 44 95 91 B5 14 41 " +
            "7F 0D AB D8 A7 86 2B 1B A3 F5 90 C3 53 3D 59 F9 " +
            "9A 4C AC 43 DF BF 82 20 BD EB 20 07 75 54 56 07 " +
            "92 DE 3F E8 42 A9 38 7E 99 1D A7 46 FE 61 8C 31 " +
            "5B 6E 36 58 86 2E AB 6F 22 D0 C2 4E 0C D6 7C 75 " +
            "0F 4D A5 15 5C 15 88 26 6B 9B 22 34 2A D5 FD 43 " +
            "8B 47 C1 0D 71 6A 52 66 A6 0C A2 39 95 6F 29 CF " +
            "AF 08 BF BB FA 4D C7 4D 40 D8 B0 2F 3B 65 5A E2 " +
            "05 62 3E B7 D1 EA 24 AF 2A 10 9E 74 73 F5 DA 15 " +
            "7F 63 03 1B B5 7E 52 0D",
            // After Iota 
            "ED F8 50 9E 3E 07 01 01 06 7F DE 72 7C BD 93 F5 " +
            "07 95 90 59 C2 8B 5D 18 C5 65 AA 96 FC 82 82 72 " +
            "14 26 29 F2 78 C8 FD A1 DC 05 A0 19 59 9D 57 FD " +
            "22 E4 4D 66 A6 BD AB B3 9F A6 44 95 91 B5 14 41 " +
            "7F 0D AB D8 A7 86 2B 1B A3 F5 90 C3 53 3D 59 F9 " +
            "9A 4C AC 43 DF BF 82 20 BD EB 20 07 75 54 56 07 " +
            "92 DE 3F E8 42 A9 38 7E 99 1D A7 46 FE 61 8C 31 " +
            "5B 6E 36 58 86 2E AB 6F 22 D0 C2 4E 0C D6 7C 75 " +
            "0F 4D A5 15 5C 15 88 26 6B 9B 22 34 2A D5 FD 43 " +
            "8B 47 C1 0D 71 6A 52 66 A6 0C A2 39 95 6F 29 CF " +
            "AF 08 BF BB FA 4D C7 4D 40 D8 B0 2F 3B 65 5A E2 " +
            "05 62 3E B7 D1 EA 24 AF 2A 10 9E 74 73 F5 DA 15 " +
            "7F 63 03 1B B5 7E 52 0D",
            // Round #6
            // After Theta
            "75 E1 33 86 23 84 0D FF E9 3E 11 0C E7 52 AD 86 " +
            "D5 35 C4 93 44 5A BA CA CA D5 00 A7 0C BF C3 53 " +
            "DB D6 92 E1 43 4E 8F 42 44 1C C3 01 44 1E 5B 03 " +
            "CD A5 82 18 3D 52 95 C0 4D 06 10 5F 17 64 F3 93 " +
            "70 BD 01 E9 57 BB 6A 3A 6C 05 2B D0 68 BB 2B 1A " +
            "02 55 CF 5B C2 3C 8E DE 52 AA EF 79 EE BB 68 74 " +
            "40 7E 6B 22 C4 78 DF AC 96 AD 0D 77 0E 5C CD 10 " +
            "94 9E 8D 4B BD A8 D9 8C BA C9 A1 56 11 55 70 8B " +
            "E0 0C 6A 6B C7 FA B6 55 B9 3B 76 FE AC 04 1A 91 " +
            "84 F7 6B 3C 81 57 13 47 69 FC 19 2A AE E9 5B 2C " +
            "37 11 DC A3 E7 CE CB B3 AF 99 7F 51 A0 8A 64 91 " +
            "D7 C2 6A 7D 57 3B C3 7D 25 A0 34 45 83 C8 9B 34 " +
            "B0 93 B8 08 8E F8 20 EE",
            // After Rho  
            "75 E1 33 86 23 84 0D FF D3 7D 22 18 CE A5 5A 0D " +
            "75 0D F1 24 91 96 AE 72 F0 3B 3C A5 5C 0D 70 CA " +
            "72 7A 14 DA B6 96 0C 1F 40 E4 B1 35 40 C4 31 1C " +
            "88 D1 23 55 09 DC 5C 2A 64 93 01 C4 D7 05 D9 FC " +
            "DE 80 F4 AB 5D 35 1D B8 BB A2 C1 56 B0 02 8D B6 " +
            "16 A8 7A DE 12 E6 71 F4 D1 49 A9 BE E7 B9 EF A2 " +
            "13 21 C6 FB 66 05 F2 5B B8 9A 21 2C 5B 1B EE 1C " +
            "A5 5E D4 6C 46 4A CF C6 AD 22 AA E0 16 75 93 43 " +
            "6D ED 58 DF B6 0A 9C 41 8D C8 DC 1D 3B 7F 56 02 " +
            "6A E2 88 F0 7E 8D 27 F0 2C 69 FC 19 2A AE E9 5B " +
            "2F CF DE 44 70 8F 9E 3B BE 66 FE 45 81 2A 92 45 " +
            "5A 58 AD EF 6A 67 B8 EF A0 34 45 83 C8 9B 34 25 " +
            "88 3B EC 24 2E 82 23 3E",
            // After Pi
            "75 E1 33 86 23 84 0D FF 88 D1 23 55 09 DC 5C 2A " +
            "13 21 C6 FB 66 05 F2 5B 6A E2 88 F0 7E 8D 27 F0 " +
            "88 3B EC 24 2E 82 23 3E F0 3B 3C A5 5C 0D 70 CA " +
            "BB A2 C1 56 B0 02 8D B6 16 A8 7A DE 12 E6 71 F4 " +
            "6D ED 58 DF B6 0A 9C 41 5A 58 AD EF 6A 67 B8 EF " +
            "D3 7D 22 18 CE A5 5A 0D 64 93 01 C4 D7 05 D9 FC " +
            "B8 9A 21 2C 5B 1B EE 1C 2C 69 FC 19 2A AE E9 5B " +
            "2F CF DE 44 70 8F 9E 3B 72 7A 14 DA B6 96 0C 1F " +
            "40 E4 B1 35 40 C4 31 1C D1 49 A9 BE E7 B9 EF A2 " +
            "8D C8 DC 1D 3B 7F 56 02 A0 34 45 83 C8 9B 34 25 " +
            "75 0D F1 24 91 96 AE 72 DE 80 F4 AB 5D 35 1D B8 " +
            "A5 5E D4 6C 46 4A CF C6 AD 22 AA E0 16 75 93 43 " +
            "BE 66 FE 45 81 2A 92 45",
            // After Chi  
            "66 C1 F7 2C 45 85 AF AE E0 13 2B 55 11 54 59 8A " +
            "93 38 A2 FF 66 07 F2 55 1F 22 9B 72 7F 89 2B 31 " +
            "00 2B EC 75 26 DA 73 3E F4 33 06 2D 5E E9 00 8A " +
            "D2 E7 C1 57 14 0A 01 B7 04 B8 DF FE 5A 83 51 5A " +
            "CD CE 48 DF A2 02 DC 41 51 D8 6C BD CA 65 35 DB " +
            "4B 75 02 30 C6 BF 7C 0D 60 F2 DD D5 F7 A1 D8 BF " +
            "BB 1C 23 68 0B 1A F8 3C FC 59 DC 01 A4 8E A9 5F " +
            "0B 4D DF 80 61 8F 1F CB E3 73 1C 50 11 AF C2 BD " +
            "4C 64 E5 34 58 82 21 1C F1 7D A8 3C 27 39 CF 87 " +
            "DF 82 CC 45 0D 7B 5E 18 A0 B0 E4 A6 88 DB 05 25 " +
            "54 53 F1 60 93 DC 6C 34 D6 A0 DE 2B 4D 00 0D B9 " +
            "B7 1A 80 69 C7 40 CF C2 EC 2B AB C0 06 E1 BF 71 " +
            "34 E6 FA CE CD 0B 83 CD",
            // After Iota 
            "E7 41 F7 AC 45 85 AF 2E E0 13 2B 55 11 54 59 8A " +
            "93 38 A2 FF 66 07 F2 55 1F 22 9B 72 7F 89 2B 31 " +
            "00 2B EC 75 26 DA 73 3E F4 33 06 2D 5E E9 00 8A " +
            "D2 E7 C1 57 14 0A 01 B7 04 B8 DF FE 5A 83 51 5A " +
            "CD CE 48 DF A2 02 DC 41 51 D8 6C BD CA 65 35 DB " +
            "4B 75 02 30 C6 BF 7C 0D 60 F2 DD D5 F7 A1 D8 BF " +
            "BB 1C 23 68 0B 1A F8 3C FC 59 DC 01 A4 8E A9 5F " +
            "0B 4D DF 80 61 8F 1F CB E3 73 1C 50 11 AF C2 BD " +
            "4C 64 E5 34 58 82 21 1C F1 7D A8 3C 27 39 CF 87 " +
            "DF 82 CC 45 0D 7B 5E 18 A0 B0 E4 A6 88 DB 05 25 " +
            "54 53 F1 60 93 DC 6C 34 D6 A0 DE 2B 4D 00 0D B9 " +
            "B7 1A 80 69 C7 40 CF C2 EC 2B AB C0 06 E1 BF 71 " +
            "34 E6 FA CE CD 0B 83 CD",
            // Round #7
            // After Theta
            "B9 2C AF 1C 42 9E 28 A7 DB C2 D8 AC E0 3B 93 46 " +
            "61 C2 7E 65 65 44 21 FF E8 08 6E 0E 38 AF CF CA " +
            "C3 78 B8 5E EB 05 37 38 AA 5E 5E 9D 59 F2 87 03 " +
            "E9 36 32 AE E5 65 CB 7B F6 42 03 64 59 C0 82 F0 " +
            "3A E4 BD A3 E5 24 38 BA 92 8B 38 96 07 BA 71 DD " +
            "15 18 5A 80 C1 A4 FB 84 5B 23 2E 2C 06 CE 12 73 " +
            "49 E6 FF F2 08 59 2B 96 0B 73 29 7D E3 A8 4D A4 " +
            "C8 1E 8B AB AC 50 5B CD BD 1E 44 E0 16 B4 45 34 " +
            "77 B5 16 CD A9 ED EB D0 03 87 74 A6 24 7A 1C 2D " +
            "28 A8 39 39 4A 5D BA E3 63 E3 B0 8D 45 04 41 23 " +
            "0A 3E A9 D0 94 C7 EB BD ED 71 2D D2 BC 6F C7 75 " +
            "45 E0 5C F3 C4 03 1C 68 1B 01 5E BC 41 C7 5B 8A " +
            "F7 B5 AE E5 00 D4 C7 CB",
            // After Rho  
            "B9 2C AF 1C 42 9E 28 A7 B6 85 B1 59 C1 77 26 8D " +
            "98 B0 5F 59 19 51 C8 7F F3 FA AC 8C 8E E0 E6 80 " +
            "2F B8 C1 19 C6 C3 F5 5A 99 25 7F 38 A0 EA E5 D5 " +
            "E3 5A 5E B6 BC 97 6E 23 BC BD D0 00 59 16 B0 20 " +
            "F2 DE D1 72 12 1C 5D 1D 1B D7 2D B9 88 63 79 A0 " +
            "AC C0 D0 02 0C 26 DD 27 CC 6D 8D B8 B0 18 38 4B " +
            "97 47 C8 5A B1 4C 32 FF 51 9B 48 17 E6 52 FA C6 " +
            "55 56 A8 AD 66 64 8F C5 C0 2D 68 8B 68 7A 3D 88 " +
            "A2 39 B5 7D 1D FA AE D6 8E 96 81 43 3A 53 12 3D " +
            "4B 77 1C 05 35 27 47 A9 23 63 E3 B0 8D 45 04 41 " +
            "AF F7 2A F8 A4 42 53 1E B5 C7 B5 48 F3 BE 1D D7 " +
            "08 9C 6B 9E 78 80 03 AD 01 5E BC 41 C7 5B 8A 1B " +
            "F1 F2 7D AD 6B 39 00 F5",
            // After Pi
            "B9 2C AF 1C 42 9E 28 A7 E3 5A 5E B6 BC 97 6E 23 " +
            "97 47 C8 5A B1 4C 32 FF 4B 77 1C 05 35 27 47 A9 " +
            "F1 F2 7D AD 6B 39 00 F5 F3 FA AC 8C 8E E0 E6 80 " +
            "1B D7 2D B9 88 63 79 A0 AC C0 D0 02 0C 26 DD 27 " +
            "A2 39 B5 7D 1D FA AE D6 08 9C 6B 9E 78 80 03 AD " +
            "B6 85 B1 59 C1 77 26 8D BC BD D0 00 59 16 B0 20 " +
            "51 9B 48 17 E6 52 FA C6 23 63 E3 B0 8D 45 04 41 " +
            "AF F7 2A F8 A4 42 53 1E 2F B8 C1 19 C6 C3 F5 5A " +
            "99 25 7F 38 A0 EA E5 D5 CC 6D 8D B8 B0 18 38 4B " +
            "8E 96 81 43 3A 53 12 3D 01 5E BC 41 C7 5B 8A 1B " +
            "98 B0 5F 59 19 51 C8 7F F2 DE D1 72 12 1C 5D 1D " +
            "55 56 A8 AD 66 64 8F C5 C0 2D 68 8B 68 7A 3D 88 " +
            "B5 C7 B5 48 F3 BE 1D D7",
            // After Chi  
            "AD 29 2F 54 43 D6 38 7B AB 6A 4A B3 B8 B4 2B 23 " +
            "27 C7 A9 F2 FB 54 32 AB 43 7B 9E 15 35 A1 6F AB " +
            "B3 A0 2D 0F D7 38 46 F5 57 FA 7C 8E 8A E4 62 87 " +
            "19 EE 08 C4 99 BB 5B 70 A4 44 9A 80 6C 26 DC 0E " +
            "51 5B 31 7D 9B 9A 4A D6 00 99 6A AF 78 83 1A 8D " +
            "F7 87 B9 4E 67 37 6C 4B 9E DD 73 A0 50 13 B4 21 " +
            "DD 0F 40 5F C6 50 A9 D8 33 63 72 B1 CC 70 20 C0 " +
            "A7 CF 6A F8 BC 42 C3 3E 6B F0 41 99 D6 D3 ED 50 " +
            "9B B7 7F 7B AA A9 E7 E1 CD 25 B1 B8 75 10 B0 49 " +
            "A0 36 C0 5B 3A D3 67 7D 91 5B 82 61 E7 73 8A 9E " +
            "9D B0 77 D4 7D 31 4A BF 72 F7 91 70 1A 06 6D 15 " +
            "60 94 3D ED F5 E0 8F 92 C8 1D 22 9A 60 3B FD A0 " +
            "D7 89 35 6A F1 B2 08 D7",
            // After Iota 
            "A4 A9 2F 54 43 D6 38 FB AB 6A 4A B3 B8 B4 2B 23 " +
            "27 C7 A9 F2 FB 54 32 AB 43 7B 9E 15 35 A1 6F AB " +
            "B3 A0 2D 0F D7 38 46 F5 57 FA 7C 8E 8A E4 62 87 " +
            "19 EE 08 C4 99 BB 5B 70 A4 44 9A 80 6C 26 DC 0E " +
            "51 5B 31 7D 9B 9A 4A D6 00 99 6A AF 78 83 1A 8D " +
            "F7 87 B9 4E 67 37 6C 4B 9E DD 73 A0 50 13 B4 21 " +
            "DD 0F 40 5F C6 50 A9 D8 33 63 72 B1 CC 70 20 C0 " +
            "A7 CF 6A F8 BC 42 C3 3E 6B F0 41 99 D6 D3 ED 50 " +
            "9B B7 7F 7B AA A9 E7 E1 CD 25 B1 B8 75 10 B0 49 " +
            "A0 36 C0 5B 3A D3 67 7D 91 5B 82 61 E7 73 8A 9E " +
            "9D B0 77 D4 7D 31 4A BF 72 F7 91 70 1A 06 6D 15 " +
            "60 94 3D ED F5 E0 8F 92 C8 1D 22 9A 60 3B FD A0 " +
            "D7 89 35 6A F1 B2 08 D7",
            // Round #8
            // After Theta
            "7D BE 0B BE C5 89 B8 F8 BE 85 68 9B 1F F6 4B B7 " +
            "70 0E 08 1E 4A A1 43 EC 14 0E 55 CA EE 03 2D 13 " +
            "1F E1 AB A4 E4 55 FA 24 8E ED 58 64 0C BB E2 84 " +
            "0C 01 2A EC 3E F9 3B E4 F3 8D 3B 6C DD D3 AD 49 " +
            "06 2E FA A2 40 38 08 6E AC D8 EC 04 4B EE A6 5C " +
            "2E 90 9D A4 E1 68 EC 48 8B 32 51 88 F7 51 D4 B5 " +
            "8A C6 E1 B3 77 A5 D8 9F 64 16 B9 6E 17 D2 62 78 " +
            "0B 8E EC 53 8F 2F 7F EF B2 E7 65 73 50 8C 6D 53 " +
            "8E 58 5D 53 0D EB 87 75 9A EC 10 54 C4 E5 C1 0E " +
            "F7 43 0B 84 E1 71 25 C5 3D 1A 04 CA D4 1E 36 4F " +
            "44 A7 53 3E FB 6E CA BC 67 18 B3 58 BD 44 0D 81 " +
            "37 5D 9C 01 44 15 FE D5 9F 68 E9 45 BB 99 BF 18 " +
            "7B C8 B3 C1 C2 DF B4 06",
            // After Rho  
            "7D BE 0B BE C5 89 B8 F8 7D 0B D1 36 3F EC 97 6E " +
            "9C 03 82 87 52 E8 10 3B 3E D0 32 41 E1 50 A5 EC " +
            "AF D2 27 F9 08 5F 25 25 C6 B0 2B 4E E8 D8 8E 45 " +
            "C2 EE 93 BF 43 CE 10 A0 D2 7C E3 0E 5B F7 74 6B " +
            "17 7D 51 20 1C 04 37 03 6E CA C5 8A CD 4E B0 E4 " +
            "72 81 EC 24 0D 47 63 47 D7 2E CA 44 21 DE 47 51 " +
            "9F BD 2B C5 FE 54 34 0E A4 C5 F0 C8 2C 72 DD 2E " +
            "A9 C7 97 BF F7 05 47 F6 E6 A0 18 DB A6 64 CF CB " +
            "6B AA 61 FD B0 CE 11 AB 60 07 4D 76 08 2A E2 F2 " +
            "AE A4 F8 7E 68 81 30 3C 4F 3D 1A 04 CA D4 1E 36 " +
            "29 F3 12 9D 4E F9 EC BB 9E 61 CC 62 F5 12 35 04 " +
            "A6 8B 33 80 A8 C2 BF FA 68 E9 45 BB 99 BF 18 9F " +
            "AD C1 1E F2 6C B0 F0 37",
            // After Pi
            "7D BE 0B BE C5 89 B8 F8 C2 EE 93 BF 43 CE 10 A0 " +
            "9F BD 2B C5 FE 54 34 0E AE A4 F8 7E 68 81 30 3C " +
            "AD C1 1E F2 6C B0 F0 37 3E D0 32 41 E1 50 A5 EC " +
            "6E CA C5 8A CD 4E B0 E4 72 81 EC 24 0D 47 63 47 " +
            "6B AA 61 FD B0 CE 11 AB A6 8B 33 80 A8 C2 BF FA " +
            "7D 0B D1 36 3F EC 97 6E D2 7C E3 0E 5B F7 74 6B " +
            "A4 C5 F0 C8 2C 72 DD 2E 4F 3D 1A 04 CA D4 1E 36 " +
            "29 F3 12 9D 4E F9 EC BB AF D2 27 F9 08 5F 25 25 " +
            "C6 B0 2B 4E E8 D8 8E 45 D7 2E CA 44 21 DE 47 51 " +
            "60 07 4D 76 08 2A E2 F2 68 E9 45 BB 99 BF 18 9F " +
            "9C 03 82 87 52 E8 10 3B 17 7D 51 20 1C 04 37 03 " +
            "A9 C7 97 BF F7 05 47 F6 E6 A0 18 DB A6 64 CF CB " +
            "9E 61 CC 62 F5 12 35 04",
            // After Chi  
            "60 AF 23 FE 79 99 9C F6 E2 EE 43 85 43 4F 10 90 " +
            "9E FC 2D 45 FA 64 F4 0D FE 9A F9 72 E9 88 38 F4 " +
            "2F 81 8E F3 6E F6 F0 37 2E D1 1A 65 E1 51 E6 EF " +
            "67 E0 C4 53 7D C6 A0 4C F6 80 FE 24 05 47 CD 17 " +
            "73 FA 61 BC F1 DE 11 AF E6 81 F6 0A A4 CC AF FA " +
            "59 8A C1 F6 1B EC 1E 6A 99 44 E9 0A 99 73 76 7B " +
            "84 07 F0 51 28 5B 3D A7 1B 35 DB 26 FB D0 0D 72 " +
            "AB 87 30 95 0E EA 8C BA BE DC E7 F9 09 59 64 35 " +
            "E6 B1 2E 7C E0 F8 2E E7 DF C6 CA CD B0 4B 5F 5C " +
            "E7 15 6F 36 08 6A C7 D2 28 C9 4D BD 79 3F 92 DF " +
            "34 81 04 18 B1 E9 50 CF 51 5D 59 60 1C 64 BF 0A " +
            "B1 86 53 9F A6 17 77 F2 E6 A2 1A 5E A4 8C CF F0 " +
            "9D 1D 9D 42 F9 16 12 04",
            // After Iota 
            "EA AF 23 FE 79 99 9C F6 E2 EE 43 85 43 4F 10 90 " +
            "9E FC 2D 45 FA 64 F4 0D FE 9A F9 72 E9 88 38 F4 " +
            "2F 81 8E F3 6E F6 F0 37 2E D1 1A 65 E1 51 E6 EF " +
            "67 E0 C4 53 7D C6 A0 4C F6 80 FE 24 05 47 CD 17 " +
            "73 FA 61 BC F1 DE 11 AF E6 81 F6 0A A4 CC AF FA " +
            "59 8A C1 F6 1B EC 1E 6A 99 44 E9 0A 99 73 76 7B " +
            "84 07 F0 51 28 5B 3D A7 1B 35 DB 26 FB D0 0D 72 " +
            "AB 87 30 95 0E EA 8C BA BE DC E7 F9 09 59 64 35 " +
            "E6 B1 2E 7C E0 F8 2E E7 DF C6 CA CD B0 4B 5F 5C " +
            "E7 15 6F 36 08 6A C7 D2 28 C9 4D BD 79 3F 92 DF " +
            "34 81 04 18 B1 E9 50 CF 51 5D 59 60 1C 64 BF 0A " +
            "B1 86 53 9F A6 17 77 F2 E6 A2 1A 5E A4 8C CF F0 " +
            "9D 1D 9D 42 F9 16 12 04",
            // Round #9
            // After Theta
            "6B B1 88 ED 8A AC 61 CE F1 30 2C CC FA 92 18 3F " +
            "1B 9F 59 85 3E C2 FB 51 D3 06 73 37 A1 5E B3 BF " +
            "97 31 8F 6B 56 BE 7D 2E AF CF B1 76 12 64 1B D7 " +
            "74 3E AB 1A C4 1B A8 E3 73 E3 8A E4 C1 E1 C2 4B " +
            "5E 66 EB F9 B9 08 9A E4 5E 31 F7 92 9C 84 22 E3 " +
            "D8 94 6A E5 E8 D9 E3 52 8A 9A 86 43 20 AE 7E D4 " +
            "01 64 84 91 EC FD 32 FB 36 A9 51 63 B3 06 86 39 " +
            "13 37 31 0D 36 A2 01 A3 3F C2 4C EA FA 6C 99 0D " +
            "F5 6F 41 35 59 25 26 48 5A A5 BE 0D 74 ED 50 00 " +
            "CA 89 E5 73 40 BC 4C 99 90 79 4C 25 41 77 1F C6 " +
            "B5 9F AF 0B 42 DC AD F7 42 83 36 29 A5 B9 B7 A5 " +
            "34 E5 27 5F 62 B1 78 AE CB 3E 90 1B EC 5A 44 BB " +
            "25 AD 9C DA C1 5E 9F 1D",
            // After Rho  
            "6B B1 88 ED 8A AC 61 CE E2 61 58 98 F5 25 31 7E " +
            "C6 67 56 A1 8F F0 7E D4 EA 35 FB 3B 6D 30 77 13 " +
            "F2 ED 73 B9 8C 79 5C B3 27 41 B6 71 FD FA 1C 6B " +
            "AA 41 BC 81 3A 4E E7 B3 D2 DC B8 22 79 70 B8 F0 " +
            "B3 F5 FC 5C 04 4D 72 2F 28 32 EE 15 73 2F C9 49 " +
            "C2 A6 54 2B 47 CF 1E 97 51 2B 6A 1A 0E 81 B8 FA " +
            "8C 64 EF 97 D9 0F 20 23 0D 0C 73 6C 52 A3 C6 66 " +
            "06 1B D1 80 D1 89 9B 98 D4 F5 D9 32 1B 7E 84 99 " +
            "A8 26 AB C4 04 A9 FE 2D 28 00 AD 52 DF 06 BA 76 " +
            "97 29 53 39 B1 7C 0E 88 C6 90 79 4C 25 41 77 1F " +
            "B7 DE D7 7E BE 2E 08 71 0A 0D DA A4 94 E6 DE 96 " +
            "A6 FC E4 4B 2C 16 CF 95 3E 90 1B EC 5A 44 BB CB " +
            "67 47 49 2B A7 76 B0 D7",
            // After Pi
            "6B B1 88 ED 8A AC 61 CE AA 41 BC 81 3A 4E E7 B3 " +
            "8C 64 EF 97 D9 0F 20 23 97 29 53 39 B1 7C 0E 88 " +
            "67 47 49 2B A7 76 B0 D7 EA 35 FB 3B 6D 30 77 13 " +
            "28 32 EE 15 73 2F C9 49 C2 A6 54 2B 47 CF 1E 97 " +
            "A8 26 AB C4 04 A9 FE 2D A6 FC E4 4B 2C 16 CF 95 " +
            "E2 61 58 98 F5 25 31 7E D2 DC B8 22 79 70 B8 F0 " +
            "0D 0C 73 6C 52 A3 C6 66 C6 90 79 4C 25 41 77 1F " +
            "B7 DE D7 7E BE 2E 08 71 F2 ED 73 B9 8C 79 5C B3 " +
            "27 41 B6 71 FD FA 1C 6B 51 2B 6A 1A 0E 81 B8 FA " +
            "28 00 AD 52 DF 06 BA 76 3E 90 1B EC 5A 44 BB CB " +
            "C6 67 56 A1 8F F0 7E D4 B3 F5 FC 5C 04 4D 72 2F " +
            "06 1B D1 80 D1 89 9B 98 D4 F5 D9 32 1B 7E 84 99 " +
            "0A 0D DA A4 94 E6 DE 96",
            // After Chi  
            "6F 95 CB FB 4B AD 61 CE B9 48 AC A9 1A 3E E9 3B " +
            "EC 22 E7 95 DF 0D 90 74 9F 99 D3 FD B9 F4 4F 80 " +
            "E7 07 7D 2B 97 34 36 E6 28 B1 EB 11 69 F0 61 85 " +
            "00 32 45 D1 73 0F 29 61 C4 7E 10 20 6F D9 1F 07 " +
            "E0 27 B0 F4 45 89 CE 2F A6 FE E0 4F 3E 19 47 DD " +
            "EF 61 1B D4 F7 A6 77 78 10 4C B0 22 5C 30 89 E9 " +
            "3C 42 F5 5E C8 8D CE 06 86 B1 71 CC 64 40 46 11 " +
            "A7 42 77 5C B6 7E 80 F1 A2 C7 3B B3 8E 78 FC 23 " +
            "0F 41 33 31 2C FC 1E 6F 47 BB 78 B6 0E C1 B9 73 " +
            "E8 6D CD 43 5B 3F FE 46 3B 90 9F AC 2B C6 BB 83 " +
            "C2 6D 57 21 5E 70 F7 44 63 11 F4 6E 0E 3B 76 2E " +
            "0C 13 D3 04 55 09 C1 9E 10 97 DD 33 10 6E A4 D9 " +
            "3B 9D 72 F8 94 EB DE BD",
            // After Iota 
            "E7 95 CB FB 4B AD 61 CE B9 48 AC A9 1A 3E E9 3B " +
            "EC 22 E7 95 DF 0D 90 74 9F 99 D3 FD B9 F4 4F 80 " +
            "E7 07 7D 2B 97 34 36 E6 28 B1 EB 11 69 F0 61 85 " +
            "00 32 45 D1 73 0F 29 61 C4 7E 10 20 6F D9 1F 07 " +
            "E0 27 B0 F4 45 89 CE 2F A6 FE E0 4F 3E 19 47 DD " +
            "EF 61 1B D4 F7 A6 77 78 10 4C B0 22 5C 30 89 E9 " +
            "3C 42 F5 5E C8 8D CE 06 86 B1 71 CC 64 40 46 11 " +
            "A7 42 77 5C B6 7E 80 F1 A2 C7 3B B3 8E 78 FC 23 " +
            "0F 41 33 31 2C FC 1E 6F 47 BB 78 B6 0E C1 B9 73 " +
            "E8 6D CD 43 5B 3F FE 46 3B 90 9F AC 2B C6 BB 83 " +
            "C2 6D 57 21 5E 70 F7 44 63 11 F4 6E 0E 3B 76 2E " +
            "0C 13 D3 04 55 09 C1 9E 10 97 DD 33 10 6E A4 D9 " +
            "3B 9D 72 F8 94 EB DE BD",
            // Round #10  
            // After Theta
            "8A EE F0 9C C5 5F B6 DE 46 CB A8 B6 59 EF E6 5F " +
            "2B AE 7C FA 6F 12 8B C5 0D 42 75 7C DA 98 5E F1 " +
            "66 2C D0 C6 4F BE 52 6F 45 CA D0 76 E7 02 B6 95 " +
            "FF B1 41 CE 30 DE 26 05 03 F2 8B 4F DF C6 04 B6 " +
            "72 FC 16 75 26 E5 DF 5E 27 D5 4D A2 E6 93 23 54 " +
            "82 1A 20 B3 79 54 A0 68 EF CF B4 3D 1F E1 86 8D " +
            "FB CE 6E 31 78 92 D5 B7 14 6A D7 4D 07 2C 57 60 " +
            "26 69 DA B1 6E F4 E4 78 CF BC 00 D4 00 8A 2B 33 " +
            "F0 C2 37 2E 6F 2D 11 0B 80 37 E3 D9 BE DE A2 C2 " +
            "7A B6 6B C2 38 53 EF 37 BA BB 32 41 F3 4C DF 0A " +
            "AF 16 6C 46 D0 82 20 54 9C 92 F0 71 4D EA 79 4A " +
            "CB 9F 48 6B E5 16 DA 2F 82 4C 7B B2 73 02 B5 A8 " +
            "BA B6 DF 15 4C 61 BA 34",
            // After Rho  
            "8A EE F0 9C C5 5F B6 DE 8C 96 51 6D B3 DE CD BF " +
            "8A 2B 9F FE 9B C4 62 F1 8D E9 15 DF 20 54 C7 A7 " +
            "F2 95 7A 33 63 81 36 7E 77 2E 60 5B 59 A4 0C 6D " +
            "E4 0C E3 6D 52 F0 1F 1B ED 80 FC E2 D3 B7 31 81 " +
            "7E 8B 3A 93 F2 6F 2F 39 39 42 75 52 DD 24 6A 3E " +
            "13 D4 00 99 CD A3 02 45 36 BE 3F D3 F6 7C 84 1B " +
            "8B C1 93 AC BE DD 77 76 58 AE C0 28 D4 AE 9B 0E " +
            "58 37 7A 72 3C 93 34 ED A8 01 14 57 66 9E 79 01 " +
            "C6 E5 AD 25 62 01 5E F8 51 61 C0 9B F1 6C 5F 6F " +
            "EA FD 46 CF 76 4D 18 67 0A BA BB 32 41 F3 4C DF " +
            "82 50 BD 5A B0 19 41 0B 71 4A C2 C7 35 A9 E7 29 " +
            "F9 13 69 AD DC 42 FB 65 4C 7B B2 73 02 B5 A8 82 " +
            "2E 8D AE ED 77 05 53 98",
            // After Pi
            "8A EE F0 9C C5 5F B6 DE E4 0C E3 6D 52 F0 1F 1B " +
            "8B C1 93 AC BE DD 77 76 EA FD 46 CF 76 4D 18 67 " +
            "2E 8D AE ED 77 05 53 98 8D E9 15 DF 20 54 C7 A7 " +
            "39 42 75 52 DD 24 6A 3E 13 D4 00 99 CD A3 02 45 " +
            "C6 E5 AD 25 62 01 5E F8 F9 13 69 AD DC 42 FB 65 " +
            "8C 96 51 6D B3 DE CD BF ED 80 FC E2 D3 B7 31 81 " +
            "58 AE C0 28 D4 AE 9B 0E 0A BA BB 32 41 F3 4C DF " +
            "82 50 BD 5A B0 19 41 0B F2 95 7A 33 63 81 36 7E " +
            "77 2E 60 5B 59 A4 0C 6D 36 BE 3F D3 F6 7C 84 1B " +
            "51 61 C0 9B F1 6C 5F 6F 4C 7B B2 73 02 B5 A8 82 " +
            "8A 2B 9F FE 9B C4 62 F1 7E 8B 3A 93 F2 6F 2F 39 " +
            "58 37 7A 72 3C 93 34 ED A8 01 14 57 66 9E 79 01 " +
            "71 4A C2 C7 35 A9 E7 29",
            // After Chi  
            "81 2F E0 1C 69 52 D6 BA 84 30 A7 2E 12 F0 17 1A " +
            "8F C1 3B 8C BF DD 34 EE 6A 9F 16 DF F6 17 BC 21 " +
            "4A 8D AD 8C 65 A5 5A 99 8F 7D 15 56 20 D7 C7 E6 " +
            "FD 63 D8 76 FF 24 36 86 2A C6 40 11 51 E1 A3 40 " +
            "C2 0D B9 77 42 15 5A 7A C9 11 09 AD 01 62 D3 7D " +
            "9C B8 51 65 B7 D6 47 B1 EF 90 C7 F0 D2 E6 75 50 " +
            "D8 EE C4 60 64 A6 9A 0E 06 3C FB 17 42 35 C0 6B " +
            "E3 50 11 D8 F0 38 71 0B F2 05 65 B3 C5 D9 B6 6C " +
            "36 6F A0 53 58 A4 57 09 3A A4 0D B3 F4 ED 24 9B " +
            "E3 E5 88 9B 90 6C 49 13 49 51 B2 3B 1A 91 A0 83 " +
            "8A 1F DF 9E 97 54 72 35 DE 8B 3E 96 B0 63 66 39 " +
            "09 7D B8 F2 2D B2 B2 C5 22 20 09 6F EC DA 79 D1 " +
            "05 CA E2 C6 55 82 EA 21",
            // After Iota 
            "88 AF E0 9C 69 52 D6 BA 84 30 A7 2E 12 F0 17 1A " +
            "8F C1 3B 8C BF DD 34 EE 6A 9F 16 DF F6 17 BC 21 " +
            "4A 8D AD 8C 65 A5 5A 99 8F 7D 15 56 20 D7 C7 E6 " +
            "FD 63 D8 76 FF 24 36 86 2A C6 40 11 51 E1 A3 40 " +
            "C2 0D B9 77 42 15 5A 7A C9 11 09 AD 01 62 D3 7D " +
            "9C B8 51 65 B7 D6 47 B1 EF 90 C7 F0 D2 E6 75 50 " +
            "D8 EE C4 60 64 A6 9A 0E 06 3C FB 17 42 35 C0 6B " +
            "E3 50 11 D8 F0 38 71 0B F2 05 65 B3 C5 D9 B6 6C " +
            "36 6F A0 53 58 A4 57 09 3A A4 0D B3 F4 ED 24 9B " +
            "E3 E5 88 9B 90 6C 49 13 49 51 B2 3B 1A 91 A0 83 " +
            "8A 1F DF 9E 97 54 72 35 DE 8B 3E 96 B0 63 66 39 " +
            "09 7D B8 F2 2D B2 B2 C5 22 20 09 6F EC DA 79 D1 " +
            "05 CA E2 C6 55 82 EA 21",
            // Round #11  
            // After Theta
            "59 B6 49 42 1C 55 AF 0F FA 20 AD D4 19 A4 B2 53 " +
            "2E 30 B7 76 7C 2B 7C F6 7C 01 D6 6A 13 0B 42 44 " +
            "E2 07 44 C3 B6 99 69 02 5E 64 BC 88 55 D0 BE 53 " +
            "83 73 D2 8C F4 70 93 CF 8B 37 CC EB 92 17 EB 58 " +
            "D4 93 79 C2 A7 09 A4 1F 61 9B E0 E2 D2 5E E0 E6 " +
            "4D A1 F8 BB C2 D1 3E 04 91 80 CD 0A D9 B2 D0 19 " +
            "79 1F 48 9A A7 50 D2 16 10 A2 3B A2 A7 29 3E 0E " +
            "4B DA F8 97 23 04 42 90 23 1C CC 6D B0 DE CF D9 " +
            "48 7F AA A9 53 F0 F2 40 9B 55 81 49 37 1B 6C 83 " +
            "F5 7B 48 2E 75 70 B7 76 E1 DB 5B 74 C9 AD 93 18 " +
            "5B 06 76 40 E2 53 0B 80 A0 9B 34 6C BB 37 C3 70 " +
            "A8 8C 34 08 EE 44 FA DD 34 BE C9 DA 09 C6 87 B4 " +
            "AD 40 0B 89 86 BE D9 BA",
            // After Rho  
            "59 B6 49 42 1C 55 AF 0F F4 41 5A A9 33 48 65 A7 " +
            "0B CC AD 1D DF 0A 9F BD B1 20 44 C4 17 60 AD 36 " +
            "CD 4C 13 10 3F 20 1A B6 58 05 ED 3B E5 45 C6 8B " +
            "CD 48 0F 37 F9 3C 38 27 D6 E2 0D F3 BA E4 C5 3A " +
            "C9 3C E1 D3 04 D2 0F EA 05 6E 1E B6 09 2E 2E ED " +
            "68 0A C5 DF 15 8E F6 21 67 44 02 36 2B 64 CB 42 " +
            "D2 3C 85 92 B6 C8 FB 40 53 7C 1C 20 44 77 44 4F " +
            "CB 11 02 21 C8 25 6D FC DB 60 BD 9F B3 47 38 98 " +
            "35 75 0A 5E 1E 08 E9 4F B6 C1 CD AA C0 A4 9B 0D " +
            "EE D6 AE 7E 0F C9 A5 0E 18 E1 DB 5B 74 C9 AD 93 " +
            "2D 00 6E 19 D8 01 89 4F 81 6E D2 B0 ED DE 0C C3 " +
            "95 91 06 C1 9D 48 BF 1B BE C9 DA 09 C6 87 B4 34 " +
            "B6 6E 2B D0 42 A2 A1 6F",
            // After Pi
            "59 B6 49 42 1C 55 AF 0F CD 48 0F 37 F9 3C 38 27 " +
            "D2 3C 85 92 B6 C8 FB 40 EE D6 AE 7E 0F C9 A5 0E " +
            "B6 6E 2B D0 42 A2 A1 6F B1 20 44 C4 17 60 AD 36 " +
            "05 6E 1E B6 09 2E 2E ED 68 0A C5 DF 15 8E F6 21 " +
            "35 75 0A 5E 1E 08 E9 4F 95 91 06 C1 9D 48 BF 1B " +
            "F4 41 5A A9 33 48 65 A7 D6 E2 0D F3 BA E4 C5 3A " +
            "53 7C 1C 20 44 77 44 4F 18 E1 DB 5B 74 C9 AD 93 " +
            "2D 00 6E 19 D8 01 89 4F CD 4C 13 10 3F 20 1A B6 " +
            "58 05 ED 3B E5 45 C6 8B 67 44 02 36 2B 64 CB 42 " +
            "B6 C1 CD AA C0 A4 9B 0D BE C9 DA 09 C6 87 B4 34 " +
            "0B CC AD 1D DF 0A 9F BD C9 3C E1 D3 04 D2 0F EA " +
            "CB 11 02 21 C8 25 6D FC DB 60 BD 9F B3 47 38 98 " +
            "81 6E D2 B0 ED DE 0C C3",
            // After Chi  
            "4B 82 C9 C2 1A 95 6C 4F E1 8A 25 5B F0 3D 3C 29 " +
            "C2 14 84 12 F6 EA FB 21 A7 46 EE 7C 13 9C AB 0E " +
            "32 26 2D E5 A3 8A B1 4F D9 20 85 8D 03 E0 7D 36 " +
            "10 1B 14 B6 03 2E 27 A3 E8 8A C1 5E 94 CE E0 31 " +
            "15 55 4A 5A 1C 28 E9 6B 91 DF 1C F3 95 46 BD D2 " +
            "F5 5D 4A A9 77 5B 65 E2 DE 63 CE A8 8A 6C 6C AA " +
            "76 7C 38 20 CC 77 44 03 C8 A0 CB FB 57 81 C9 33 " +
            "2F A2 6B 4B 50 A5 09 57 EA 0C 11 14 35 00 13 F6 " +
            "C8 84 20 B3 25 C5 D6 86 6F 4C 10 37 2D 67 EF 72 " +
            "F7 C5 CC BA F9 84 91 8F AE C8 36 22 06 C2 70 3D " +
            "09 CD AF 3D 17 2F FF A9 D9 5C 5C 4D 37 90 1F EA " +
            "CB 1F 40 01 84 BD 69 BF D1 E0 90 92 A1 47 AB A4 " +
            "41 5E 92 72 ED 0E 0C 81",
            // After Iota 
            "41 82 C9 42 1A 95 6C 4F E1 8A 25 5B F0 3D 3C 29 " +
            "C2 14 84 12 F6 EA FB 21 A7 46 EE 7C 13 9C AB 0E " +
            "32 26 2D E5 A3 8A B1 4F D9 20 85 8D 03 E0 7D 36 " +
            "10 1B 14 B6 03 2E 27 A3 E8 8A C1 5E 94 CE E0 31 " +
            "15 55 4A 5A 1C 28 E9 6B 91 DF 1C F3 95 46 BD D2 " +
            "F5 5D 4A A9 77 5B 65 E2 DE 63 CE A8 8A 6C 6C AA " +
            "76 7C 38 20 CC 77 44 03 C8 A0 CB FB 57 81 C9 33 " +
            "2F A2 6B 4B 50 A5 09 57 EA 0C 11 14 35 00 13 F6 " +
            "C8 84 20 B3 25 C5 D6 86 6F 4C 10 37 2D 67 EF 72 " +
            "F7 C5 CC BA F9 84 91 8F AE C8 36 22 06 C2 70 3D " +
            "09 CD AF 3D 17 2F FF A9 D9 5C 5C 4D 37 90 1F EA " +
            "CB 1F 40 01 84 BD 69 BF D1 E0 90 92 A1 47 AB A4 " +
            "41 5E 92 72 ED 0E 0C 81",
            // Round #12  
            // After Theta
            "5E 1B 31 38 40 64 69 A0 9E D7 C6 A0 B2 2E 17 50 " +
            "44 12 60 43 9C 2C 26 96 99 6D 3E 3D 0E 5E 81 3C " +
            "73 CD 6E 8F 3B 7E 30 BB C6 B9 7D F7 59 11 78 D9 " +
            "6F 46 F7 4D 41 3D 0C DA 6E 8C 25 0F FE 08 3D 86 " +
            "2B 7E 9A 1B 01 EA C3 59 D0 34 5F 99 0D B2 3C 26 " +
            "EA C4 B2 D3 2D AA 60 0D A1 3E 2D 53 C8 7F 47 D3 " +
            "F0 7A DC 71 A6 B1 99 B4 F6 8B 1B BA 4A 43 E3 01 " +
            "6E 49 28 21 C8 51 88 A3 F5 95 E9 6E 6F F1 16 19 " +
            "B7 D9 C3 48 67 D6 FD FF E9 4A F4 66 47 A1 32 C5 " +
            "C9 EE 1C FB E4 46 BB BD EF 23 75 48 9E 36 F1 C9 " +
            "16 54 57 47 4D DE FA 46 A6 01 BF B6 75 83 34 93 " +
            "4D 19 A4 50 EE 7B B4 08 EF CB 40 D3 BC 85 81 96 " +
            "00 B5 D1 18 75 FA 8D 75",
            // After Rho  
            "5E 1B 31 38 40 64 69 A0 3C AF 8D 41 65 5D 2E A0 " +
            "91 04 D8 10 27 8B 89 25 E0 15 C8 93 D9 E6 D3 E3 " +
            "F1 83 D9 9D 6B 76 7B DC 9F 15 81 97 6D 9C DB 77 " +
            "DF 14 D4 C3 A0 FD 66 74 A1 1B 63 C9 83 3F 42 8F " +
            "3F CD 8D 00 F5 E1 AC 15 CB 63 02 4D F3 95 D9 20 " +
            "50 27 96 9D 6E 51 05 6B 4D 87 FA B4 4C 21 FF 1D " +
            "8E 33 8D CD A4 85 D7 E3 86 C6 03 EC 17 37 74 95 " +
            "10 E4 28 C4 51 B7 24 94 DD DE E2 2D 32 EA 2B D3 " +
            "18 E9 CC BA FF FF 36 7B 99 E2 74 25 7A B3 A3 50 " +
            "68 B7 37 D9 9D 63 9F DC C9 EF 23 75 48 9E 36 F1 " +
            "EB 1B 59 50 5D 1D 35 79 9A 06 FC DA D6 0D D2 4C " +
            "29 83 14 CA 7D 8F 16 A1 CB 40 D3 BC 85 81 96 EF " +
            "63 1D 40 6D 34 46 9D 7E",
            // After Pi
            "5E 1B 31 38 40 64 69 A0 DF 14 D4 C3 A0 FD 66 74 " +
            "8E 33 8D CD A4 85 D7 E3 68 B7 37 D9 9D 63 9F DC " +
            "63 1D 40 6D 34 46 9D 7E E0 15 C8 93 D9 E6 D3 E3 " +
            "CB 63 02 4D F3 95 D9 20 50 27 96 9D 6E 51 05 6B " +
            "18 E9 CC BA FF FF 36 7B 29 83 14 CA 7D 8F 16 A1 " +
            "3C AF 8D 41 65 5D 2E A0 A1 1B 63 C9 83 3F 42 8F " +
            "86 C6 03 EC 17 37 74 95 C9 EF 23 75 48 9E 36 F1 " +
            "EB 1B 59 50 5D 1D 35 79 F1 83 D9 9D 6B 76 7B DC " +
            "9F 15 81 97 6D 9C DB 77 4D 87 FA B4 4C 21 FF 1D " +
            "99 E2 74 25 7A B3 A3 50 CB 40 D3 BC 85 81 96 EF " +
            "91 04 D8 10 27 8B 89 25 3F CD 8D 00 F5 E1 AC 15 " +
            "10 E4 28 C4 51 B7 24 94 DD DE E2 2D 32 EA 2B D3 " +
            "9A 06 FC DA D6 0D D2 4C",
            // After Chi  
            "5E 38 38 34 44 64 F8 23 BF 90 E6 D3 B9 9F 6E 68 " +
            "8D 3B CD E9 84 81 D7 C1 74 B5 06 C9 DD 43 FF 5C " +
            "E2 19 84 AE 94 DF 9B 2A F0 11 5C 03 D5 A6 D7 A8 " +
            "C3 AB 4A 6F 62 3B EB 30 71 25 86 DD 6E 51 05 EB " +
            "D8 FD 04 AB 7F 9F F7 39 22 E1 16 86 5F 9E 1E A1 " +
            "3A 6B 8D 65 71 5D 1A B0 E8 32 43 D8 CB B7 40 EF " +
            "A4 D6 5B EC 02 36 75 9D DD 4B A7 74 68 DE 3C 71 " +
            "6A 0B 3B D8 DF 3F 75 76 B1 01 A3 BD 6B 57 5F D4 " +
            "0F 75 85 96 5F 0E DB 37 0F 87 79 2C C9 21 EB B2 " +
            "A9 61 7C 24 10 C5 CA 40 C5 54 D3 BE 81 09 16 CC " +
            "91 24 F8 D4 27 9D 89 A5 F2 D7 4F 29 D7 A9 A7 56 " +
            "12 E4 34 16 95 B2 F4 98 DC DE E2 2D 13 68 22 F2 " +
            "B4 CF F9 DA 06 6D F6 5C",
            // After Iota 
            "D5 B8 38 B4 44 64 F8 23 BF 90 E6 D3 B9 9F 6E 68 " +
            "8D 3B CD E9 84 81 D7 C1 74 B5 06 C9 DD 43 FF 5C " +
            "E2 19 84 AE 94 DF 9B 2A F0 11 5C 03 D5 A6 D7 A8 " +
            "C3 AB 4A 6F 62 3B EB 30 71 25 86 DD 6E 51 05 EB " +
            "D8 FD 04 AB 7F 9F F7 39 22 E1 16 86 5F 9E 1E A1 " +
            "3A 6B 8D 65 71 5D 1A B0 E8 32 43 D8 CB B7 40 EF " +
            "A4 D6 5B EC 02 36 75 9D DD 4B A7 74 68 DE 3C 71 " +
            "6A 0B 3B D8 DF 3F 75 76 B1 01 A3 BD 6B 57 5F D4 " +
            "0F 75 85 96 5F 0E DB 37 0F 87 79 2C C9 21 EB B2 " +
            "A9 61 7C 24 10 C5 CA 40 C5 54 D3 BE 81 09 16 CC " +
            "91 24 F8 D4 27 9D 89 A5 F2 D7 4F 29 D7 A9 A7 56 " +
            "12 E4 34 16 95 B2 F4 98 DC DE E2 2D 13 68 22 F2 " +
            "B4 CF F9 DA 06 6D F6 5C",
            // Round #13  
            // After Theta
            "DD 86 F0 96 E6 17 9B E3 0B 21 EF AC 7C 21 FD 19 " +
            "ED E8 9F 0C 8E 6A D7 5A 87 CF 5D 02 4E 03 67 1B " +
            "98 6B DA C6 04 DB 81 19 F8 2F 94 21 77 D5 B4 68 " +
            "77 1A 43 10 A7 85 78 41 11 F6 D4 38 64 BA 05 70 " +
            "2B 87 5F 60 EC DF 6F 7E 58 93 48 EE CF 9A 04 92 " +
            "32 55 45 47 D3 2E 79 70 5C 83 4A A7 0E 09 D3 9E " +
            "C4 05 09 09 08 DD 75 06 2E 31 FC BF FB 9E A4 36 " +
            "10 79 65 B0 4F 3B 6F 45 B9 3F 6B 9F C9 24 3C 14 " +
            "BB C4 8C E9 9A B0 48 46 6F 54 2B C9 C3 CA EB 29 " +
            "5A 1B 27 EF 83 85 52 07 BF 26 8D D6 11 0D 0C FF " +
            "99 1A 30 F6 85 EE EA 65 46 66 46 56 12 17 34 27 " +
            "72 37 66 F3 9F 59 F4 03 2F A4 B9 E6 80 28 BA B5 " +
            "CE BD A7 B2 96 69 EC 6F",
            // After Rho  
            "DD 86 F0 96 E6 17 9B E3 16 42 DE 59 F9 42 FA 33 " +
            "3B FA 27 83 A3 DA B5 56 34 70 B6 71 F8 DC 25 E0 " +
            "D8 0E CC C0 5C D3 36 26 72 57 4D 8B 86 FF 42 19 " +
            "04 71 5A 88 17 74 A7 31 5C 84 3D 35 0E 99 6E 01 " +
            "C3 2F 30 F6 EF 37 BF 95 49 20 89 35 89 E4 FE AC " +
            "93 A9 2A 3A 9A 76 C9 83 7B 72 0D 2A 9D 3A 24 4C " +
            "48 40 E8 AE 33 20 2E 48 3D 49 6D 5C 62 F8 7F F7 " +
            "D8 A7 9D B7 22 88 BC 32 3E 93 49 78 28 72 7F D6 " +
            "31 5D 13 16 C9 68 97 98 F5 94 37 AA 95 E4 61 E5 " +
            "50 EA 40 6B E3 E4 7D B0 FF BF 26 8D D6 11 0D 0C " +
            "AB 97 65 6A C0 D8 17 BA 18 99 19 59 49 5C D0 9C " +
            "EE C6 6C FE 33 8B 7E 40 A4 B9 E6 80 28 BA B5 2F " +
            "FB 9B 73 EF A9 AC 65 1A",
            // After Pi
            "DD 86 F0 96 E6 17 9B E3 04 71 5A 88 17 74 A7 31 " +
            "48 40 E8 AE 33 20 2E 48 50 EA 40 6B E3 E4 7D B0 " +
            "FB 9B 73 EF A9 AC 65 1A 34 70 B6 71 F8 DC 25 E0 " +
            "49 20 89 35 89 E4 FE AC 93 A9 2A 3A 9A 76 C9 83 " +
            "31 5D 13 16 C9 68 97 98 EE C6 6C FE 33 8B 7E 40 " +
            "16 42 DE 59 F9 42 FA 33 5C 84 3D 35 0E 99 6E 01 " +
            "3D 49 6D 5C 62 F8 7F F7 FF BF 26 8D D6 11 0D 0C " +
            "AB 97 65 6A C0 D8 17 BA D8 0E CC C0 5C D3 36 26 " +
            "72 57 4D 8B 86 FF 42 19 7B 72 0D 2A 9D 3A 24 4C " +
            "F5 94 37 AA 95 E4 61 E5 A4 B9 E6 80 28 BA B5 2F " +
            "3B FA 27 83 A3 DA B5 56 C3 2F 30 F6 EF 37 BF 95 " +
            "D8 A7 9D B7 22 88 BC 32 3E 93 49 78 28 72 7F D6 " +
            "18 99 19 59 49 5C D0 9C",
            // After Chi  
            "95 86 50 B0 C6 17 93 AB 14 DB 5A C9 D7 B0 F6 81 " +
            "E3 51 DB 2A 3B 28 2E 42 54 EE C0 7B A5 F7 E7 51 " +
            "FB EA 79 E7 B8 CC 41 0A A6 F9 94 7B EA CE 24 E3 " +
            "69 74 98 31 C8 EC E8 B4 5D 2B 46 D2 A8 F5 A1 C3 " +
            "21 6D 81 17 01 3C 96 38 A7 C6 65 FA 32 AB A4 4C " +
            "37 0B 9E 11 99 22 EB C5 9E 32 3F B4 9A 98 6E 09 " +
            "3D 49 2C 3E 62 30 6D 45 EB FF BC 9C EF 13 E5 0D " +
            "E3 13 44 4E C6 41 13 BA D1 2E CC E0 45 D3 12 62 " +
            "F6 D3 7F 0B 86 3B 03 B8 7B 5B CD 2A B5 20 B0 46 " +
            "AD 92 3F EA C1 A5 63 E5 86 E8 E7 8B AA 96 F5 36 " +
            "23 7A AA 82 A3 52 B5 74 E5 3F 70 BE E7 45 FC 51 " +
            "D8 AF 8D B6 63 84 3C 3A 1D F1 6F FA 8A F0 5A 94 " +
            "D8 9C 09 2D 05 79 DA 1D",
            // After Iota 
            "1E 86 50 B0 C6 17 93 2B 14 DB 5A C9 D7 B0 F6 81 " +
            "E3 51 DB 2A 3B 28 2E 42 54 EE C0 7B A5 F7 E7 51 " +
            "FB EA 79 E7 B8 CC 41 0A A6 F9 94 7B EA CE 24 E3 " +
            "69 74 98 31 C8 EC E8 B4 5D 2B 46 D2 A8 F5 A1 C3 " +
            "21 6D 81 17 01 3C 96 38 A7 C6 65 FA 32 AB A4 4C " +
            "37 0B 9E 11 99 22 EB C5 9E 32 3F B4 9A 98 6E 09 " +
            "3D 49 2C 3E 62 30 6D 45 EB FF BC 9C EF 13 E5 0D " +
            "E3 13 44 4E C6 41 13 BA D1 2E CC E0 45 D3 12 62 " +
            "F6 D3 7F 0B 86 3B 03 B8 7B 5B CD 2A B5 20 B0 46 " +
            "AD 92 3F EA C1 A5 63 E5 86 E8 E7 8B AA 96 F5 36 " +
            "23 7A AA 82 A3 52 B5 74 E5 3F 70 BE E7 45 FC 51 " +
            "D8 AF 8D B6 63 84 3C 3A 1D F1 6F FA 8A F0 5A 94 " +
            "D8 9C 09 2D 05 79 DA 1D",
            // Round #14  
            // After Theta
            "1E 2E 02 B6 EC AB 55 57 28 75 85 C4 CA 58 D1 EA " +
            "4F 1E 73 12 DE 88 FA BC B7 BE 5D CA 45 2D 3A 46 " +
            "2F B5 AC 77 1F B5 1A 28 A6 51 C6 7D C0 72 E2 9F " +
            "55 DA 47 3C D5 04 CF DF F1 64 EE EA 4D 55 75 3D " +
            "C2 3D 1C A6 E1 E6 4B 2F 73 99 B0 6A 95 D2 FF 6E " +
            "37 A3 CC 17 B3 9E 2D B9 A2 9C E0 B9 87 70 49 62 " +
            "91 06 84 06 87 90 B9 BB 08 AF 21 2D 0F C9 38 1A " +
            "37 4C 91 DE 61 38 48 98 D1 86 9E E6 6F 6F D4 1E " +
            "CA 7D A0 06 9B D3 24 D3 D7 14 65 12 50 80 64 B8 " +
            "4E C2 A2 5B 21 7F BE F2 52 B7 32 1B 0D EF AE 14 " +
            "23 D2 F8 84 89 EE 73 08 D9 91 AF B3 FA AD DB 3A " +
            "74 E0 25 8E 86 24 E8 C4 FE A1 F2 4B 6A 2A 87 83 " +
            "0C C3 DC BD A2 00 81 3F",
            // After Rho  
            "1E 2E 02 B6 EC AB 55 57 51 EA 0A 89 95 B1 A2 D5 " +
            "93 C7 9C 84 37 A2 3E EF D4 A2 63 74 EB DB A5 5C " +
            "A8 D5 40 79 A9 65 BD FB 07 2C 27 FE 69 1A 65 DC " +
            "C4 53 4D F0 FC 5D A5 7D 4F 3C 99 BB 7A 53 55 5D " +
            "1E 0E D3 70 F3 A5 17 E1 FD EF 36 97 09 AB 56 29 " +
            "BD 19 65 BE 98 F5 6C C9 89 89 72 82 E7 1E C2 25 " +
            "34 38 84 CC DD 8D 34 20 92 71 34 10 5E 43 5A 1E " +
            "EF 30 1C 24 CC 1B A6 48 CD DF DE A8 3D A2 0D 3D " +
            "D4 60 73 9A 64 5A B9 0F 32 DC 6B 8A 32 09 28 40 " +
            "CF 57 DE 49 58 74 2B E4 14 52 B7 32 1B 0D EF AE " +
            "CF 21 8C 48 E3 13 26 BA 64 47 BE CE EA B7 6E EB " +
            "0E BC C4 D1 90 04 9D 98 A1 F2 4B 6A 2A 87 83 FE " +
            "E0 0F C3 30 77 AF 28 40",
            // After Pi
            "1E 2E 02 B6 EC AB 55 57 C4 53 4D F0 FC 5D A5 7D " +
            "34 38 84 CC DD 8D 34 20 CF 57 DE 49 58 74 2B E4 " +
            "E0 0F C3 30 77 AF 28 40 D4 A2 63 74 EB DB A5 5C " +
            "FD EF 36 97 09 AB 56 29 BD 19 65 BE 98 F5 6C C9 " +
            "D4 60 73 9A 64 5A B9 0F 0E BC C4 D1 90 04 9D 98 " +
            "51 EA 0A 89 95 B1 A2 D5 4F 3C 99 BB 7A 53 55 5D " +
            "92 71 34 10 5E 43 5A 1E 14 52 B7 32 1B 0D EF AE " +
            "CF 21 8C 48 E3 13 26 BA A8 D5 40 79 A9 65 BD FB " +
            "07 2C 27 FE 69 1A 65 DC 89 89 72 82 E7 1E C2 25 " +
            "32 DC 6B 8A 32 09 28 40 A1 F2 4B 6A 2A 87 83 FE " +
            "93 C7 9C 84 37 A2 3E EF 1E 0E D3 70 F3 A5 17 E1 " +
            "EF 30 1C 24 CC 1B A6 48 CD DF DE A8 3D A2 0D 3D " +
            "64 47 BE CE EA B7 6E EB",
            // After Chi  
            "2E 06 82 BA ED 2B 45 57 0F 14 17 F1 FC 2D AE B9 " +
            "14 30 85 FC FA 06 34 20 D1 77 DE CF D0 74 7E F3 " +
            "20 5E 8E 70 67 FB 88 68 D4 B2 22 5C 7B 8F 8D 9C " +
            "BD 8F 24 97 6D A1 C7 2F B7 85 E1 FF 08 F1 68 59 " +
            "04 62 50 BE 0F 81 99 4B 27 F1 D0 52 90 24 CF B9 " +
            "C1 AB 2E 89 91 B1 A8 D7 4B 3E 1A 99 7B 5F F0 FD " +
            "59 50 3C 58 BE 51 5A 0E 04 98 B5 B3 0F AD 6F EB " +
            "C1 35 1D 7A 89 51 73 B2 20 54 10 79 2F 61 3F DA " +
            "35 78 2E F6 79 1B 4D 9C 08 AB 72 E2 EF 98 41 9B " +
            "3A D9 6B 9B B3 69 14 41 A6 DA 6C EC 6A 9D C3 FA " +
            "72 F7 90 80 3B B8 9E E7 1E C1 11 F8 C2 05 1E D4 " +
            "CF 30 3C 62 0E 0E C4 8A 5E 5F DE A8 28 A2 1D 39 " +
            "68 4F FD BE 2A B2 6F EB",
            // After Iota 
            "A7 86 82 BA ED 2B 45 D7 0F 14 17 F1 FC 2D AE B9 " +
            "14 30 85 FC FA 06 34 20 D1 77 DE CF D0 74 7E F3 " +
            "20 5E 8E 70 67 FB 88 68 D4 B2 22 5C 7B 8F 8D 9C " +
            "BD 8F 24 97 6D A1 C7 2F B7 85 E1 FF 08 F1 68 59 " +
            "04 62 50 BE 0F 81 99 4B 27 F1 D0 52 90 24 CF B9 " +
            "C1 AB 2E 89 91 B1 A8 D7 4B 3E 1A 99 7B 5F F0 FD " +
            "59 50 3C 58 BE 51 5A 0E 04 98 B5 B3 0F AD 6F EB " +
            "C1 35 1D 7A 89 51 73 B2 20 54 10 79 2F 61 3F DA " +
            "35 78 2E F6 79 1B 4D 9C 08 AB 72 E2 EF 98 41 9B " +
            "3A D9 6B 9B B3 69 14 41 A6 DA 6C EC 6A 9D C3 FA " +
            "72 F7 90 80 3B B8 9E E7 1E C1 11 F8 C2 05 1E D4 " +
            "CF 30 3C 62 0E 0E C4 8A 5E 5F DE A8 28 A2 1D 39 " +
            "68 4F FD BE 2A B2 6F EB",
            // Round #15  
            // After Theta
            "0B B0 7C 52 70 10 48 E2 95 D4 35 D1 B4 80 69 D5 " +
            "AC 3B 8F EE 3C ED FD 54 FC 17 6C 01 01 06 CC 70 " +
            "54 2C 1C AD 0B F0 8A 00 78 84 DC B4 E6 B4 80 A9 " +
            "27 4F 06 B7 25 0C 00 43 0F 8E EB ED CE 1A A1 2D " +
            "29 02 E2 70 DE F3 2B C8 53 83 42 8F FC 2F CD D1 " +
            "6D 9D D0 61 0C 8A A5 E2 D1 FE 38 B9 33 F2 37 91 " +
            "E1 5B 36 4A 78 BA 93 7A 29 F8 07 7D DE DF DD 68 " +
            "B5 47 8F A7 E5 5A 71 DA 8C 62 EE 91 B2 5A 32 EF " +
            "AF B8 0C D6 31 B6 8A F0 B0 A0 78 F0 29 73 88 EF " +
            "17 B9 D9 55 62 1B A6 C2 D2 A8 FE 31 06 96 C1 92 " +
            "DE C1 6E 68 A6 83 93 D2 84 01 33 D8 8A A8 D9 B8 " +
            "77 3B 36 70 C8 E5 0D FE 73 3F 6C 66 F9 D0 AF BA " +
            "1C 3D 6F 63 46 B9 6D 83",
            // After Rho  
            "0B B0 7C 52 70 10 48 E2 2B A9 6B A2 69 01 D3 AA " +
            "EB CE A3 3B 4F 7B 3F 15 60 C0 0C C7 7F C1 16 10 " +
            "80 57 04 A0 62 E1 68 5D 6B 4E 0B 98 8A 47 C8 4D " +
            "70 5B C2 00 30 74 F2 64 CB 83 E3 7A BB B3 46 68 " +
            "01 71 38 EF F9 15 E4 14 D2 1C 3D 35 28 F4 C8 FF " +
            "6F EB 84 0E 63 50 2C 15 44 46 FB E3 E4 CE C8 DF " +
            "51 C2 D3 9D D4 0B DF B2 BF BB D1 52 F0 0F FA BC " +
            "D3 72 AD 38 ED DA A3 C7 23 65 B5 64 DE 19 C5 DC " +
            "C1 3A C6 56 11 FE 15 97 C4 77 58 50 3C F8 94 39 " +
            "C3 54 F8 22 37 BB 4A 6C 92 D2 A8 FE 31 06 96 C1 " +
            "4E 4A 7B 07 BB A1 99 0E 12 06 CC 60 2B A2 66 E3 " +
            "6E C7 06 0E B9 BC C1 FF 3F 6C 66 F9 D0 AF BA 73 " +
            "DB 20 47 CF DB 98 51 6E",
            // After Pi
            "0B B0 7C 52 70 10 48 E2 70 5B C2 00 30 74 F2 64 " +
            "51 C2 D3 9D D4 0B DF B2 C3 54 F8 22 37 BB 4A 6C " +
            "DB 20 47 CF DB 98 51 6E 60 C0 0C C7 7F C1 16 10 " +
            "D2 1C 3D 35 28 F4 C8 FF 6F EB 84 0E 63 50 2C 15 " +
            "C1 3A C6 56 11 FE 15 97 6E C7 06 0E B9 BC C1 FF " +
            "2B A9 6B A2 69 01 D3 AA CB 83 E3 7A BB B3 46 68 " +
            "BF BB D1 52 F0 0F FA BC 92 D2 A8 FE 31 06 96 C1 " +
            "4E 4A 7B 07 BB A1 99 0E 80 57 04 A0 62 E1 68 5D " +
            "6B 4E 0B 98 8A 47 C8 4D 44 46 FB E3 E4 CE C8 DF " +
            "C4 77 58 50 3C F8 94 39 3F 6C 66 F9 D0 AF BA 73 " +
            "EB CE A3 3B 4F 7B 3F 15 01 71 38 EF F9 15 E4 14 " +
            "D3 72 AD 38 ED DA A3 C7 23 65 B5 64 DE 19 C5 DC " +
            "12 06 CC 60 2B A2 66 E3",
            // After Chi  
            "0A 30 6D CF B4 1B 45 70 F2 4F EA 22 13 C4 F2 28 " +
            "49 E2 D4 50 1C 0B CE B0 C3 C4 C0 32 17 BB 42 EC " +
            "AB 6B C5 CF DB FC E3 6A 4D 23 8C CD 3C C1 32 10 " +
            "52 0C 7F 65 38 5A D9 7D 41 2E 84 06 CB 50 EC 7D " +
            "C1 3A CE 97 57 BF 03 97 FC DB 37 3E B9 88 09 10 " +
            "1F 91 7B A2 29 0D 6B 3E CB C3 CB D6 BA B3 42 29 " +
            "F3 B3 82 53 7A AE F3 B2 B3 73 A8 5E 71 06 D4 61 " +
            "8E 48 FB 5F 29 13 9D 4E 84 57 F4 C3 06 69 68 CF " +
            "EB 7F 0B 88 92 77 DC 6D 7F 4E DD 4A 24 C9 E2 9D " +
            "44 64 58 50 1E B8 D4 35 54 64 6D E1 58 A9 3A 73 " +
            "39 CC 26 2B 4B B1 3C D6 21 74 28 AB EB 14 A0 0C " +
            "C3 70 E5 38 CC 78 81 E4 CA AD 96 7F 9A 40 DC C8 " +
            "12 37 D4 A4 9B A6 A6 E3",
            // After Iota 
            "09 B0 6D CF B4 1B 45 F0 F2 4F EA 22 13 C4 F2 28 " +
            "49 E2 D4 50 1C 0B CE B0 C3 C4 C0 32 17 BB 42 EC " +
            "AB 6B C5 CF DB FC E3 6A 4D 23 8C CD 3C C1 32 10 " +
            "52 0C 7F 65 38 5A D9 7D 41 2E 84 06 CB 50 EC 7D " +
            "C1 3A CE 97 57 BF 03 97 FC DB 37 3E B9 88 09 10 " +
            "1F 91 7B A2 29 0D 6B 3E CB C3 CB D6 BA B3 42 29 " +
            "F3 B3 82 53 7A AE F3 B2 B3 73 A8 5E 71 06 D4 61 " +
            "8E 48 FB 5F 29 13 9D 4E 84 57 F4 C3 06 69 68 CF " +
            "EB 7F 0B 88 92 77 DC 6D 7F 4E DD 4A 24 C9 E2 9D " +
            "44 64 58 50 1E B8 D4 35 54 64 6D E1 58 A9 3A 73 " +
            "39 CC 26 2B 4B B1 3C D6 21 74 28 AB EB 14 A0 0C " +
            "C3 70 E5 38 CC 78 81 E4 CA AD 96 7F 9A 40 DC C8 " +
            "12 37 D4 A4 9B A6 A6 E3",
            // Round #16  
            // After Theta
            "D4 0C 26 40 ED EE 84 6E 9A 54 76 85 75 43 DE E2 " +
            "97 E1 79 4A 9F B0 E0 62 BB D2 4B 92 43 2E 26 A3 " +
            "59 1C 3C 8B B6 19 EE 03 90 9F C7 42 65 34 F3 8E " +
            "3A 17 E3 C2 5E DD F5 B7 9F 2D 29 1C 48 EB C2 AF " +
            "B9 2C 45 37 03 2A 67 D8 0E AC CE 7A D4 6D 04 79 " +
            "C2 2D 30 2D 70 F8 AA A0 A3 D8 57 71 DC 34 6E E3 " +
            "2D B0 2F 49 F9 15 DD 60 CB 65 23 FE 25 93 B0 2E " +
            "7C 3F 02 1B 44 F6 90 27 59 EB BF 4C 5F 9C A9 51 " +
            "83 64 97 2F F4 F0 F0 A7 A1 4D 70 50 A7 72 CC 4F " +
            "3C 72 D3 F0 4A 2D B0 7A A6 13 94 A5 35 4C 37 1A " +
            "E4 70 6D A4 12 44 FD 48 49 6F B4 0C 8D 93 8C C6 " +
            "1D 73 48 22 4F C3 AF 36 B2 BB 1D DF CE D5 B8 87 " +
            "E0 40 2D E0 F6 43 AB 8A",
            // After Rho  
            "D4 0C 26 40 ED EE 84 6E 35 A9 EC 0A EB 86 BC C5 " +
            "65 78 9E D2 27 2C B8 D8 E4 62 32 BA 2B BD 24 39 " +
            "CD 70 1F C8 E2 E0 59 B4 54 46 33 EF 08 F9 79 2C " +
            "2E EC D5 5D 7F AB 73 31 EB 67 4B 0A 07 D2 BA F0 " +
            "96 A2 9B 01 95 33 EC 5C 46 90 E7 C0 EA AC 47 DD " +
            "15 6E 81 69 81 C3 57 05 8D 8F 62 5F C5 71 D3 B8 " +
            "49 CA AF E8 06 6B 81 7D 26 61 5D 96 CB 46 FC 4B " +
            "0D 22 7B C8 13 BE 1F 81 99 BE 38 53 A3 B2 D6 7F " +
            "F2 85 1E 1E FE 74 90 EC E6 A7 D0 26 38 A8 53 39 " +
            "05 56 8F 47 6E 1A 5E A9 1A A6 13 94 A5 35 4C 37 " +
            "F5 23 91 C3 B5 91 4A 10 27 BD D1 32 34 4E 32 1A " +
            "63 0E 49 E4 69 F8 D5 A6 BB 1D DF CE D5 B8 87 B2 " +
            "AA 22 38 50 0B B8 FD D0",
            // After Pi
            "D4 0C 26 40 ED EE 84 6E 2E EC D5 5D 7F AB 73 31 " +
            "49 CA AF E8 06 6B 81 7D 05 56 8F 47 6E 1A 5E A9 " +
            "AA 22 38 50 0B B8 FD D0 E4 62 32 BA 2B BD 24 39 " +
            "46 90 E7 C0 EA AC 47 DD 15 6E 81 69 81 C3 57 05 " +
            "F2 85 1E 1E FE 74 90 EC 63 0E 49 E4 69 F8 D5 A6 " +
            "35 A9 EC 0A EB 86 BC C5 EB 67 4B 0A 07 D2 BA F0 " +
            "26 61 5D 96 CB 46 FC 4B 1A A6 13 94 A5 35 4C 37 " +
            "F5 23 91 C3 B5 91 4A 10 CD 70 1F C8 E2 E0 59 B4 " +
            "54 46 33 EF 08 F9 79 2C 8D 8F 62 5F C5 71 D3 B8 " +
            "E6 A7 D0 26 38 A8 53 39 BB 1D DF CE D5 B8 87 B2 " +
            "65 78 9E D2 27 2C B8 D8 96 A2 9B 01 95 33 EC 5C " +
            "0D 22 7B C8 13 BE 1F 81 99 BE 38 53 A3 B2 D6 7F " +
            "27 BD D1 32 34 4E 32 1A",
            // After Chi  
            "95 0E 0C E0 ED AE 04 22 2A F8 D5 5A 17 BB 2D B1 " +
            "E3 EA 9F F8 07 CB 20 2D 51 5A 89 47 8A 5C 5E 87 " +
            "80 C2 E9 4D 19 B9 8E C1 F5 0C 32 93 2A FE 34 39 " +
            "A4 11 F9 D6 94 98 C7 35 14 64 C0 89 80 4B 12 07 " +
            "76 E5 2C 04 FC 71 B0 F5 61 9E 8C A4 A9 F8 96 62 " +
            "31 A9 F8 9E 23 82 F8 CE F3 E1 49 0A 23 E3 BA C4 " +
            "C3 60 DD D5 DB C6 FE 4B 1A 2E 7F 9C EF 33 F8 F2 " +
            "3F 65 92 C3 B1 C1 48 20 44 F9 5F D8 27 E0 DB 24 " +
            "36 66 A3 CF 30 71 79 2D 94 97 6D 97 00 61 57 3A " +
            "A2 C7 D0 26 1A E8 0B 3D AB 1B FF E9 DD A1 A7 BA " +
            "6C 78 FE 1A 25 A0 AB 59 06 3E 9B 12 35 33 2C 22 " +
            "2B 23 BA E8 07 F2 3F 81 D9 FE 36 93 A0 92 5E BF " +
            "B5 3F D0 33 A4 5D 76 1E",
            // After Iota 
            "97 8E 0C E0 ED AE 04 A2 2A F8 D5 5A 17 BB 2D B1 " +
            "E3 EA 9F F8 07 CB 20 2D 51 5A 89 47 8A 5C 5E 87 " +
            "80 C2 E9 4D 19 B9 8E C1 F5 0C 32 93 2A FE 34 39 " +
            "A4 11 F9 D6 94 98 C7 35 14 64 C0 89 80 4B 12 07 " +
            "76 E5 2C 04 FC 71 B0 F5 61 9E 8C A4 A9 F8 96 62 " +
            "31 A9 F8 9E 23 82 F8 CE F3 E1 49 0A 23 E3 BA C4 " +
            "C3 60 DD D5 DB C6 FE 4B 1A 2E 7F 9C EF 33 F8 F2 " +
            "3F 65 92 C3 B1 C1 48 20 44 F9 5F D8 27 E0 DB 24 " +
            "36 66 A3 CF 30 71 79 2D 94 97 6D 97 00 61 57 3A " +
            "A2 C7 D0 26 1A E8 0B 3D AB 1B FF E9 DD A1 A7 BA " +
            "6C 78 FE 1A 25 A0 AB 59 06 3E 9B 12 35 33 2C 22 " +
            "2B 23 BA E8 07 F2 3F 81 D9 FE 36 93 A0 92 5E BF " +
            "B5 3F D0 33 A4 5D 76 1E",
            // Round #17  
            // After Theta
            "CD 33 6E A6 DF D7 8E 1B 46 E7 18 C3 46 83 DC 2C " +
            "22 EA BB 77 E4 81 A3 66 5A 3B 6C 7D 20 71 F8 12 " +
            "30 3E 1A 79 F6 F8 BC 92 AF B1 50 D5 18 87 BE 80 " +
            "C8 0E 34 4F C5 A0 36 A8 D5 64 E4 06 63 01 91 4C " +
            "7D 84 C9 3E 56 5C 16 60 D1 62 7F 90 46 B9 A4 31 " +
            "6B 14 9A D8 11 FB 72 77 9F FE 84 93 72 DB 4B 59 " +
            "02 60 F9 5A 38 8C 7D 00 11 4F 9A A6 45 1E 5E 67 " +
            "8F 99 61 F7 5E 80 7A 73 1E 44 3D 9E 15 99 51 9D " +
            "5A 79 6E 56 61 49 88 B0 55 97 49 18 E3 2B D4 71 " +
            "A9 A6 35 1C B0 C5 AD A8 1B E7 0C DD 32 E0 95 E9 " +
            "36 C5 9C 5C 17 D9 21 E0 6A 21 56 8B 64 0B DD BF " +
            "EA 23 9E 67 E4 B8 BC CA D2 9F D3 A9 0A BF F8 2A " +
            "05 C3 23 07 4B 1C 44 4D",
            // After Rho  
            "CD 33 6E A6 DF D7 8E 1B 8C CE 31 86 8D 06 B9 59 " +
            "88 FA EE 1D 79 E0 A8 99 12 87 2F A1 B5 C3 D6 07 " +
            "C7 E7 95 84 F1 D1 C8 B3 8D 71 E8 0B F8 1A 0B 55 " +
            "F3 54 0C 6A 83 8A EC 40 53 35 19 B9 C1 58 40 24 " +
            "C2 64 1F 2B 2E 0B B0 3E 4B 1A 13 2D F6 07 69 94 " +
            "5B A3 D0 C4 8E D8 97 BB 65 7D FA 13 4E CA 6D 2F " +
            "D7 C2 61 EC 03 10 00 CB 3C BC CE 22 9E 34 4D 8B " +
            "7B 2F 40 BD B9 C7 CC B0 3C 2B 32 A3 3A 3D 88 7A " +
            "CD 2A 2C 09 11 56 2B CF EA B8 AA CB 24 8C F1 15 " +
            "B8 15 35 D5 B4 86 03 B6 E9 1B E7 0C DD 32 E0 95 " +
            "87 80 DB 14 73 72 5D 64 AA 85 58 2D 92 2D 74 FF " +
            "7D C4 F3 8C 1C 97 57 59 9F D3 A9 0A BF F8 2A D2 " +
            "51 53 C1 F0 C8 C1 12 07",
            // After Pi
            "CD 33 6E A6 DF D7 8E 1B F3 54 0C 6A 83 8A EC 40 " +
            "D7 C2 61 EC 03 10 00 CB B8 15 35 D5 B4 86 03 B6 " +
            "51 53 C1 F0 C8 C1 12 07 12 87 2F A1 B5 C3 D6 07 " +
            "4B 1A 13 2D F6 07 69 94 5B A3 D0 C4 8E D8 97 BB " +
            "CD 2A 2C 09 11 56 2B CF 7D C4 F3 8C 1C 97 57 59 " +
            "8C CE 31 86 8D 06 B9 59 53 35 19 B9 C1 58 40 24 " +
            "3C BC CE 22 9E 34 4D 8B E9 1B E7 0C DD 32 E0 95 " +
            "87 80 DB 14 73 72 5D 64 C7 E7 95 84 F1 D1 C8 B3 " +
            "8D 71 E8 0B F8 1A 0B 55 65 7D FA 13 4E CA 6D 2F " +
            "EA B8 AA CB 24 8C F1 15 9F D3 A9 0A BF F8 2A D2 " +
            "88 FA EE 1D 79 E0 A8 99 C2 64 1F 2B 2E 0B B0 3E " +
            "7B 2F 40 BD B9 C7 CC B0 3C 2B 32 A3 3A 3D 88 7A " +
            "AA 85 58 2D 92 2D 74 FF",
            // After Chi  
            "C9 B1 0F 22 DF C7 8E 90 DB 41 18 7B 37 0C EF 74 " +
            "96 80 A1 CC 4B 51 10 CA 34 35 1B D3 A3 90 8F AE " +
            "63 17 C1 B8 C8 C9 72 47 02 26 EF 61 BD 1B 40 2C " +
            "CF 12 3F 24 E7 01 41 D0 6B 67 03 40 82 59 C3 AB " +
            "CF 29 20 28 B0 16 AB C9 34 DC E3 80 5E 93 7E C9 " +
            "A0 46 F7 84 93 22 B4 D2 92 36 38 B5 80 5A E0 30 " +
            "3A 3C D6 32 BC 74 50 EB E1 55 C7 8E 51 36 40 8C " +
            "D4 B1 D3 2D 33 2A 1D 40 A7 EB 87 94 F7 11 AC 99 " +
            "07 F1 E8 C3 D8 1E 9B 45 70 3E FB 13 D5 BA 67 ED " +
            "AA 9C BE 4F 64 8D 31 34 97 C3 C1 01 B7 F2 29 96 " +
            "B1 F1 AE 89 E8 24 E4 19 C6 64 2D 29 2C 33 B0 74 " +
            "F9 AB 08 B1 39 C7 B8 35 3C 51 94 B3 53 FD 00 7A " +
            "E8 81 49 0F 94 26 64 D9",
            // After Iota 
            "49 B1 0F 22 DF C7 8E 10 DB 41 18 7B 37 0C EF 74 " +
            "96 80 A1 CC 4B 51 10 CA 34 35 1B D3 A3 90 8F AE " +
            "63 17 C1 B8 C8 C9 72 47 02 26 EF 61 BD 1B 40 2C " +
            "CF 12 3F 24 E7 01 41 D0 6B 67 03 40 82 59 C3 AB " +
            "CF 29 20 28 B0 16 AB C9 34 DC E3 80 5E 93 7E C9 " +
            "A0 46 F7 84 93 22 B4 D2 92 36 38 B5 80 5A E0 30 " +
            "3A 3C D6 32 BC 74 50 EB E1 55 C7 8E 51 36 40 8C " +
            "D4 B1 D3 2D 33 2A 1D 40 A7 EB 87 94 F7 11 AC 99 " +
            "07 F1 E8 C3 D8 1E 9B 45 70 3E FB 13 D5 BA 67 ED " +
            "AA 9C BE 4F 64 8D 31 34 97 C3 C1 01 B7 F2 29 96 " +
            "B1 F1 AE 89 E8 24 E4 19 C6 64 2D 29 2C 33 B0 74 " +
            "F9 AB 08 B1 39 C7 B8 35 3C 51 94 B3 53 FD 00 7A " +
            "E8 81 49 0F 94 26 64 D9",
            // Round #18  
            // After Theta
            "3A 69 C3 38 11 96 18 DB BA 16 28 98 EB C4 65 BE " +
            "C8 79 D6 DF 04 AB DE 25 83 0A 6E F9 36 D8 6A FE " +
            "15 04 6A 85 60 9E 42 3E 71 FE 23 7B 73 4A D6 E7 " +
            "AE 45 0F C7 3B C9 CB 1A 35 9E 74 53 CD A3 0D 44 " +
            "78 16 55 02 25 5E 4E 99 42 CF 48 BD F6 C4 4E B0 " +
            "D3 9E 3B 9E 5D 73 22 19 F3 61 08 56 5C 92 6A FA " +
            "64 C5 A1 21 F3 8E 9E 04 56 6A B2 A4 C4 7E A5 DC " +
            "A2 A2 78 10 9B 7D 2D 39 D4 33 4B 8E 39 40 3A 52 " +
            "66 A6 D8 20 04 D6 11 8F 2E C7 8C 00 9A 40 A9 02 " +
            "1D A3 CB 65 F1 C5 D4 64 E1 D0 6A 3C 1F A5 19 EF " +
            "C2 29 62 93 26 75 72 D2 A7 33 1D CA F0 FB 3A BE " +
            "A7 52 7F A2 76 3D 76 DA 8B 6E E1 99 C6 B5 E5 2A " +
            "9E 92 E2 32 3C 71 54 A0",
            // After Rho  
            "3A 69 C3 38 11 96 18 DB 75 2D 50 30 D7 89 CB 7C " +
            "72 9E F5 37 C1 AA 77 09 83 AD E6 3F A8 E0 96 6F " +
            "F3 14 F2 A9 20 50 2B 04 37 A7 64 7D 1E E7 3F B2 " +
            "70 BC 93 BC AC E1 5A F4 51 8D 27 DD 54 F3 68 03 " +
            "8B 2A 81 12 2F A7 4C 3C EC 04 2B F4 8C D4 6B 4F " +
            "98 F6 DC F1 EC 9A 13 C9 E9 CF 87 21 58 71 49 AA " +
            "0D 99 77 F4 24 20 2B 0E FD 4A B9 AD D4 64 49 89 " +
            "88 CD BE 96 1C 51 51 3C 1C 73 80 74 A4 A8 67 96 " +
            "1B 84 C0 3A E2 D1 CC 14 54 01 97 63 46 00 4D A0 " +
            "98 9A AC 63 74 B9 2C BE EF E1 D0 6A 3C 1F A5 19 " +
            "C9 49 0B A7 88 4D 9A D4 9E CE 74 28 C3 EF EB F8 " +
            "54 EA 4F D4 AE C7 4E FB 6E E1 99 C6 B5 E5 2A 8B " +
            "15 A8 A7 A4 B8 0C 4F 1C",
            // After Pi
            "3A 69 C3 38 11 96 18 DB 70 BC 93 BC AC E1 5A F4 " +
            "0D 99 77 F4 24 20 2B 0E 98 9A AC 63 74 B9 2C BE " +
            "15 A8 A7 A4 B8 0C 4F 1C 83 AD E6 3F A8 E0 96 6F " +
            "EC 04 2B F4 8C D4 6B 4F 98 F6 DC F1 EC 9A 13 C9 " +
            "1B 84 C0 3A E2 D1 CC 14 54 EA 4F D4 AE C7 4E FB " +
            "75 2D 50 30 D7 89 CB 7C 51 8D 27 DD 54 F3 68 03 " +
            "FD 4A B9 AD D4 64 49 89 EF E1 D0 6A 3C 1F A5 19 " +
            "C9 49 0B A7 88 4D 9A D4 F3 14 F2 A9 20 50 2B 04 " +
            "37 A7 64 7D 1E E7 3F B2 E9 CF 87 21 58 71 49 AA " +
            "54 01 97 63 46 00 4D A0 6E E1 99 C6 B5 E5 2A 8B " +
            "72 9E F5 37 C1 AA 77 09 8B 2A 81 12 2F A7 4C 3C " +
            "88 CD BE 96 1C 51 51 3C 1C 73 80 74 A4 A8 67 96 " +
            "9E CE 74 28 C3 EF EB F8",
            // After Chi  
            "37 68 A7 78 11 96 39 D1 E0 BE 1B BF FC 78 5E 44 " +
            "08 B9 74 70 AC 24 68 0E B2 DB EC 7B 75 2B 3C 7D " +
            "55 3C B7 20 14 6D 0D 38 93 5F 32 3E C8 EA 86 EF " +
            "EF 04 2B FE 8E 95 A7 5B DC 9C D3 35 E0 9C 11 22 " +
            "98 81 60 11 E2 F1 5C 10 38 EA 46 14 AA D3 27 FB " +
            "D9 6F C8 10 57 8D CA F4 53 2C 67 9F 7C E8 CC 13 " +
            "FD 42 B2 28 54 24 53 4D DB C5 80 7A 6B 9F E4 31 " +
            "C9 C9 2C 6A 88 3F BA D7 3B 5C 71 A9 60 40 6B 0C " +
            "23 A7 74 3F 18 E7 3B B2 C3 2F 8F A5 E9 94 6B A1 " +
            "C5 15 F5 4A 46 10 4C A4 6A 42 9D 92 AB 42 3E 39 " +
            "72 5B CB B3 D1 FA 66 09 9F 18 81 72 8F 0F 6A BE " +
            "0A 41 CA 9E 5F 16 D9 54 7C 63 01 63 A4 A8 73 97 " +
            "17 EE 74 28 ED EA E3 CC",
            // After Iota 
            "3D E8 A7 78 11 96 39 D1 E0 BE 1B BF FC 78 5E 44 " +
            "08 B9 74 70 AC 24 68 0E B2 DB EC 7B 75 2B 3C 7D " +
            "55 3C B7 20 14 6D 0D 38 93 5F 32 3E C8 EA 86 EF " +
            "EF 04 2B FE 8E 95 A7 5B DC 9C D3 35 E0 9C 11 22 " +
            "98 81 60 11 E2 F1 5C 10 38 EA 46 14 AA D3 27 FB " +
            "D9 6F C8 10 57 8D CA F4 53 2C 67 9F 7C E8 CC 13 " +
            "FD 42 B2 28 54 24 53 4D DB C5 80 7A 6B 9F E4 31 " +
            "C9 C9 2C 6A 88 3F BA D7 3B 5C 71 A9 60 40 6B 0C " +
            "23 A7 74 3F 18 E7 3B B2 C3 2F 8F A5 E9 94 6B A1 " +
            "C5 15 F5 4A 46 10 4C A4 6A 42 9D 92 AB 42 3E 39 " +
            "72 5B CB B3 D1 FA 66 09 9F 18 81 72 8F 0F 6A BE " +
            "0A 41 CA 9E 5F 16 D9 54 7C 63 01 63 A4 A8 73 97 " +
            "17 EE 74 28 ED EA E3 CC",
            // Round #19  
            // After Theta
            "24 08 D7 BB 52 64 BD 30 1F 72 5C 5F 9F 0E 16 A2 " +
            "78 42 27 90 09 33 7B D1 E1 B5 D5 E5 3A 67 3E 2B " +
            "60 6B 80 80 74 06 46 C9 8A BF 42 FD 8B 18 02 0E " +
            "10 C8 6C 1E ED E3 EF BD AC 67 80 D5 45 8B 02 FD " +
            "CB EF 59 8F AD BD 5E 46 0D BD 71 B4 CA B8 6C 0A " +
            "C0 8F B8 D3 14 7F 4E 15 AC E0 20 7F 1F 9E 84 F5 " +
            "8D B9 E1 C8 F1 33 40 92 88 AB B9 E4 24 D3 E6 67 " +
            "FC 9E 1B CA E8 54 F1 26 22 BC 01 6A 23 B2 EF ED " +
            "DC 6B 33 DF 7B 91 73 54 B3 D4 DC 45 4C 83 78 7E " +
            "96 7B CC D4 09 5C 4E F2 5F 15 AA 32 CB 29 75 C8 " +
            "6B BB BB 70 92 08 E2 E8 60 D4 C6 92 EC 79 22 58 " +
            "7A BA 99 7E FA 01 CA 8B 2F 0D 38 FD EB E4 71 C1 " +
            "22 B9 43 88 8D 81 A8 3D",
            // After Rho  
            "24 08 D7 BB 52 64 BD 30 3F E4 B8 BE 3E 1D 2C 44 " +
            "9E D0 09 64 C2 CC 5E 34 73 E6 B3 12 5E 5B 5D AE " +
            "33 30 4A 06 5B 03 04 A4 BF 88 21 E0 A0 F8 2B D4 " +
            "E6 D1 3E FE DE 0B 81 CC 3F EB 19 60 75 D1 A2 40 " +
            "F7 AC C7 D6 5E 2F A3 E5 CB A6 D0 D0 1B 47 AB 8C " +
            "00 7E C4 9D A6 F8 73 AA D6 B3 82 83 FC 7D 78 12 " +
            "47 8E 9F 01 92 6C CC 0D A6 CD CF 10 57 73 C9 49 " +
            "65 74 AA 78 13 7E CF 0D D4 46 64 DF DB 45 78 03 " +
            "E6 7B 2F 72 8E 8A 7B 6D 3C BF 59 6A EE 22 A6 41 " +
            "CB 49 DE 72 8F 99 3A 81 C8 5F 15 AA 32 CB 29 75 " +
            "88 A3 AF ED EE C2 49 22 81 51 1B 4B B2 E7 89 60 " +
            "4F 37 D3 4F 3F 40 79 51 0D 38 FD EB E4 71 C1 2F " +
            "6A 8F 48 EE 10 62 63 20",
            // After Pi
            "24 08 D7 BB 52 64 BD 30 E6 D1 3E FE DE 0B 81 CC " +
            "47 8E 9F 01 92 6C CC 0D CB 49 DE 72 8F 99 3A 81 " +
            "6A 8F 48 EE 10 62 63 20 73 E6 B3 12 5E 5B 5D AE " +
            "CB A6 D0 D0 1B 47 AB 8C 00 7E C4 9D A6 F8 73 AA " +
            "E6 7B 2F 72 8E 8A 7B 6D 4F 37 D3 4F 3F 40 79 51 " +
            "3F E4 B8 BE 3E 1D 2C 44 3F EB 19 60 75 D1 A2 40 " +
            "A6 CD CF 10 57 73 C9 49 C8 5F 15 AA 32 CB 29 75 " +
            "88 A3 AF ED EE C2 49 22 33 30 4A 06 5B 03 04 A4 " +
            "BF 88 21 E0 A0 F8 2B D4 D6 B3 82 83 FC 7D 78 12 " +
            "3C BF 59 6A EE 22 A6 41 0D 38 FD EB E4 71 C1 2F " +
            "9E D0 09 64 C2 CC 5E 34 F7 AC C7 D6 5E 2F A3 E5 " +
            "65 74 AA 78 13 7E CF 0D D4 46 64 DF DB 45 78 03 " +
            "81 51 1B 4B B2 E7 89 60",
            // After Chi  
            "25 06 56 BA 52 00 F1 31 6E 90 7E 8C D3 9A B3 4C " +
            "67 08 9F 8D 82 0E 8D 2D CF 49 49 63 CD 9D A6 91 " +
            "A8 5E 60 AA 9C 69 63 EC 73 BE B7 1F FA E3 0D 8C " +
            "2D A7 FB B2 13 45 A3 C9 09 7A 14 90 97 B8 73 BA " +
            "D6 BB 0F 62 CE 91 7F C3 C7 37 93 8F 3E 44 DB 51 " +
            "BF E0 7E AE 3C 3F 65 4D 77 F9 09 CA 55 59 82 74 " +
            "A6 6D 65 55 9B 73 89 4B FF 1B 05 B8 22 D6 0D 31 " +
            "88 A8 AE AD AF 02 CB 22 73 03 C8 05 07 06 54 A6 " +
            "97 84 78 88 A2 FA AD 95 D7 B3 26 02 FC 2C 39 3C " +
            "0E BF 5B 6E F5 20 A2 C1 81 B0 DC 0B 44 89 EA 7F " +
            "9E 80 21 4C C3 9C 12 3C 67 AE 83 51 96 2E 93 E7 " +
            "64 65 B1 78 33 DC 4E 6D CA C6 64 FB 9B 4D 2E 17 " +
            "E0 7D DD D9 AE C4 28 A1",
            // After Iota 
            "2F 06 56 3A 52 00 F1 B1 6E 90 7E 8C D3 9A B3 4C " +
            "67 08 9F 8D 82 0E 8D 2D CF 49 49 63 CD 9D A6 91 " +
            "A8 5E 60 AA 9C 69 63 EC 73 BE B7 1F FA E3 0D 8C " +
            "2D A7 FB B2 13 45 A3 C9 09 7A 14 90 97 B8 73 BA " +
            "D6 BB 0F 62 CE 91 7F C3 C7 37 93 8F 3E 44 DB 51 " +
            "BF E0 7E AE 3C 3F 65 4D 77 F9 09 CA 55 59 82 74 " +
            "A6 6D 65 55 9B 73 89 4B FF 1B 05 B8 22 D6 0D 31 " +
            "88 A8 AE AD AF 02 CB 22 73 03 C8 05 07 06 54 A6 " +
            "97 84 78 88 A2 FA AD 95 D7 B3 26 02 FC 2C 39 3C " +
            "0E BF 5B 6E F5 20 A2 C1 81 B0 DC 0B 44 89 EA 7F " +
            "9E 80 21 4C C3 9C 12 3C 67 AE 83 51 96 2E 93 E7 " +
            "64 65 B1 78 33 DC 4E 6D CA C6 64 FB 9B 4D 2E 17 " +
            "E0 7D DD D9 AE C4 28 A1",
            // Round #20  
            // After Theta
            "20 C3 E5 3A F7 C7 18 F7 97 D9 FB 2A 01 B6 6C BC " +
            "E6 CC 11 F8 BD 32 90 C4 B8 99 88 E5 42 6D C4 9F " +
            "97 78 F1 02 72 52 85 8C 7C 7B 04 1F 5F 24 E4 CA " +
            "D4 EE 7E 14 C1 69 7C 39 88 BE 9A E5 A8 84 6E 53 " +
            "A1 6B CE E4 41 61 1D CD F8 11 02 27 D0 7F 3D 31 " +
            "B0 25 CD AE 99 F8 8C 0B 8E B0 8C 6C 87 75 5D 84 " +
            "27 A9 EB 20 A4 4F 94 A2 88 CB C4 3E AD 26 6F 3F " +
            "B7 8E 3F 05 41 39 2D 42 7C C6 7B 05 A2 C1 BD E0 " +
            "6E CD FD 2E 70 D6 72 65 56 77 A8 77 C3 10 24 D5 " +
            "79 6F 9A E8 7A D0 C0 CF BE 96 4D A3 AA B2 0C 1F " +
            "91 45 92 4C 66 5B FB 7A 9E E7 06 F7 44 02 4C 17 " +
            "E5 A1 3F 0D 0C E0 53 84 BD 16 A5 7D 14 BD 4C 19 " +
            "DF 5B 4C 71 40 FF CE C1",
            // After Rho  
            "20 C3 E5 3A F7 C7 18 F7 2F B3 F7 55 02 6C D9 78 " +
            "39 73 04 7E AF 0C 24 B1 D4 46 FC 89 9B 89 58 2E " +
            "93 2A 64 BC C4 8B 17 90 F1 45 42 AE CC B7 47 F0 " +
            "47 11 9C C6 97 43 ED EE 14 A2 AF 66 39 2A A1 DB " +
            "35 67 F2 A0 B0 8E E6 D0 D7 13 83 1F 21 70 02 FD " +
            "80 2D 69 76 CD C4 67 5C 11 3A C2 32 B2 1D D6 75 " +
            "07 21 7D A2 14 3D 49 5D 4D DE 7E 10 97 89 7D 5A " +
            "82 A0 9C 16 A1 5B C7 9F 0A 44 83 7B C1 F9 8C F7 " +
            "DF 05 CE 5A AE CC AD B9 92 6A AB 3B D4 BB 61 08 " +
            "1A F8 39 EF 4D 13 5D 0F 1F BE 96 4D A3 AA B2 0C " +
            "ED EB 45 16 49 32 99 6D 78 9E 1B DC 13 09 30 5D " +
            "3C F4 A7 81 01 7C 8A B0 16 A5 7D 14 BD 4C 19 BD " +
            "73 F0 F7 16 53 1C D0 BF",
            // After Pi
            "20 C3 E5 3A F7 C7 18 F7 47 11 9C C6 97 43 ED EE " +
            "07 21 7D A2 14 3D 49 5D 1A F8 39 EF 4D 13 5D 0F " +
            "73 F0 F7 16 53 1C D0 BF D4 46 FC 89 9B 89 58 2E " +
            "D7 13 83 1F 21 70 02 FD 80 2D 69 76 CD C4 67 5C " +
            "DF 05 CE 5A AE CC AD B9 3C F4 A7 81 01 7C 8A B0 " +
            "2F B3 F7 55 02 6C D9 78 14 A2 AF 66 39 2A A1 DB " +
            "4D DE 7E 10 97 89 7D 5A 1F BE 96 4D A3 AA B2 0C " +
            "ED EB 45 16 49 32 99 6D 93 2A 64 BC C4 8B 17 90 " +
            "F1 45 42 AE CC B7 47 F0 11 3A C2 32 B2 1D D6 75 " +
            "92 6A AB 3B D4 BB 61 08 16 A5 7D 14 BD 4C 19 BD " +
            "39 73 04 7E AF 0C 24 B1 35 67 F2 A0 B0 8E E6 D0 " +
            "82 A0 9C 16 A1 5B C7 9F 0A 44 83 7B C1 F9 8C F7 " +
            "78 9E 1B DC 13 09 30 5D",
            // After Chi  
            "20 E3 84 1A F7 FB 18 E6 5F C9 9C 8B DE 41 F9 EC " +
            "66 21 BB B2 06 31 C9 ED 1A FB 39 C7 E9 D0 55 4F " +
            "34 E0 EF D2 53 1C 35 B7 D4 6A 94 E9 57 0D 3D 2E " +
            "88 13 05 17 03 78 8A 5C A0 DD 48 F7 CC F4 65 5C " +
            "1F 07 96 52 34 4D FD B7 3F E5 A4 97 21 0C 88 61 " +
            "66 EF A7 45 84 ED 85 78 06 82 2F 2B 19 08 23 DF " +
            "AD 9F 3F 02 DF 99 74 3B 1D AE 24 0C A1 E6 F2 1C " +
            "FD EB 4D 34 70 30 B9 EE 93 10 E4 AC F6 83 87 95 " +
            "73 05 6B A7 88 15 66 F8 15 BF 96 36 9B 59 CE C0 " +
            "13 60 AB 93 94 38 67 08 76 E0 7F 16 B5 78 59 DD " +
            "BB F3 08 68 AE 5D 25 BE 3D 23 F1 C9 F0 2E EE B0 " +
            "F2 3A 84 92 B3 5B F7 97 0B 25 87 59 6D FD 88 57 " +
            "7C 9A E9 5C 03 8B F2 1D",
            // After Iota 
            "A1 63 84 9A F7 FB 18 66 5F C9 9C 8B DE 41 F9 EC " +
            "66 21 BB B2 06 31 C9 ED 1A FB 39 C7 E9 D0 55 4F " +
            "34 E0 EF D2 53 1C 35 B7 D4 6A 94 E9 57 0D 3D 2E " +
            "88 13 05 17 03 78 8A 5C A0 DD 48 F7 CC F4 65 5C " +
            "1F 07 96 52 34 4D FD B7 3F E5 A4 97 21 0C 88 61 " +
            "66 EF A7 45 84 ED 85 78 06 82 2F 2B 19 08 23 DF " +
            "AD 9F 3F 02 DF 99 74 3B 1D AE 24 0C A1 E6 F2 1C " +
            "FD EB 4D 34 70 30 B9 EE 93 10 E4 AC F6 83 87 95 " +
            "73 05 6B A7 88 15 66 F8 15 BF 96 36 9B 59 CE C0 " +
            "13 60 AB 93 94 38 67 08 76 E0 7F 16 B5 78 59 DD " +
            "BB F3 08 68 AE 5D 25 BE 3D 23 F1 C9 F0 2E EE B0 " +
            "F2 3A 84 92 B3 5B F7 97 0B 25 87 59 6D FD 88 57 " +
            "7C 9A E9 5C 03 8B F2 1D",
            // Round #21  
            // After Theta
            "63 0A 4C 13 3A 3D 07 D1 7D 01 7A BE D9 38 39 4C " +
            "F8 71 D9 CC B0 46 7A BD 6F 34 C6 53 BC 29 EB 63 " +
            "42 FD FE 65 2F 28 85 3A 16 03 5C 60 9A CB 22 99 " +
            "AA DB E3 22 04 01 4A FC 3E 8D 2A 89 7A 83 D6 0C " +
            "6A C8 69 C6 61 B4 43 9B 49 F8 B5 20 5D 38 38 EC " +
            "A4 86 6F CC 49 2B 9A CF 24 4A C9 1E 1E 71 E3 7F " +
            "33 CF 5D 7C 69 EE C7 6B 68 61 DB 98 F4 1F 4C 30 " +
            "8B F6 5C 83 0C 04 09 63 51 79 2C 25 3B 45 98 22 " +
            "51 CD 8D 92 8F 6C A6 58 8B EF F4 48 2D 2E 7D 90 " +
            "66 AF 54 07 C1 C1 D9 24 00 FD 6E A1 C9 4C E9 50 " +
            "79 9A C0 E1 63 9B 3A 09 1F EB 17 FC F7 57 2E 10 " +
            "6C 6A E6 EC 05 2C 44 C7 7E EA 78 CD 38 04 36 7B " +
            "0A 87 F8 EB 7F BF 42 90",
            // After Rho  
            "63 0A 4C 13 3A 3D 07 D1 FA 02 F4 7C B3 71 72 98 " +
            "7E 5C 36 33 AC 91 5E 2F 9B B2 3E F6 46 63 3C C5 " +
            "41 29 D4 11 EA F7 2F 7B A6 B9 2C 92 69 31 C0 05 " +
            "2E 42 10 A0 C4 AF BA 3D 83 4F A3 4A A2 DE A0 35 " +
            "E4 34 E3 30 DA A1 4D 35 83 C3 9E 84 5F 0B D2 85 " +
            "26 35 7C 63 4E 5A D1 7C FF 91 28 25 7B 78 C4 8D " +
            "E2 4B 73 3F 5E 9B 79 EE 3F 98 60 D0 C2 B6 31 E9 " +
            "41 06 82 84 B1 45 7B AE 4A 76 8A 30 45 A2 F2 58 " +
            "51 F2 91 CD 14 2B AA B9 3E C8 C5 77 7A A4 16 97 " +
            "38 9B C4 EC 95 EA 20 38 50 00 FD 6E A1 C9 4C E9 " +
            "EA 24 E4 69 02 87 8F 6D 7C AC 5F F0 DF 5F B9 40 " +
            "4D CD 9C BD 80 85 E8 98 EA 78 CD 38 04 36 7B 7E " +
            "10 A4 C2 21 FE FA DF AF",
            // After Pi
            "63 0A 4C 13 3A 3D 07 D1 2E 42 10 A0 C4 AF BA 3D " +
            "E2 4B 73 3F 5E 9B 79 EE 38 9B C4 EC 95 EA 20 38 " +
            "10 A4 C2 21 FE FA DF AF 9B B2 3E F6 46 63 3C C5 " +
            "83 C3 9E 84 5F 0B D2 85 26 35 7C 63 4E 5A D1 7C " +
            "51 F2 91 CD 14 2B AA B9 4D CD 9C BD 80 85 E8 98 " +
            "FA 02 F4 7C B3 71 72 98 83 4F A3 4A A2 DE A0 35 " +
            "3F 98 60 D0 C2 B6 31 E9 50 00 FD 6E A1 C9 4C E9 " +
            "EA 24 E4 69 02 87 8F 6D 41 29 D4 11 EA F7 2F 7B " +
            "A6 B9 2C 92 69 31 C0 05 FF 91 28 25 7B 78 C4 8D " +
            "3E C8 C5 77 7A A4 16 97 EA 78 CD 38 04 36 7B 7E " +
            "7E 5C 36 33 AC 91 5E 2F E4 34 E3 30 DA A1 4D 35 " +
            "41 06 82 84 B1 45 7B AE 4A 76 8A 30 45 A2 F2 58 " +
            "7C AC 5F F0 DF 5F B9 40",
            // After Chi  
            "A3 03 2F 0C 20 2D 46 13 36 D2 94 60 45 CF BA 2D " +
            "E2 6F 71 3E 34 8B A6 69 5B 91 C8 FE 95 EF 20 68 " +
            "1C E4 D2 81 3A 78 67 83 BF 86 5E 95 46 33 3D BD " +
            "D2 01 1F 08 4F 2A F8 04 2A 38 70 53 CE DE 91 7C " +
            "C3 C0 B3 8F 52 49 BE FC 4D 8C 1C BD 99 8D 2A 98 " +
            "C6 92 B4 EC F3 51 63 50 C3 4F 3E 64 83 97 EC 35 " +
            "95 BC 60 D1 C0 B0 B2 ED 40 02 ED 7A 10 B9 3C 79 " +
            "EB 69 E7 6B 02 09 0F 48 18 29 D4 34 F8 BF 2B F3 " +
            "A6 F1 E9 C0 69 B5 D2 17 3F A1 20 2D 7F 6A AD E5 " +
            "3F C9 D5 76 90 65 12 96 4C E8 E5 BA 05 36 BB 7A " +
            "7F 5E 36 B7 8D D5 6C A5 EE 44 EB 00 9E 03 CD 65 " +
            "75 8E D7 44 2B 18 72 AE 48 26 AA 33 65 22 B4 77 " +
            "FC 8C 9E F0 8D 7F B8 50",
            // After Iota 
            "23 83 2F 0C 20 2D 46 93 36 D2 94 60 45 CF BA 2D " +
            "E2 6F 71 3E 34 8B A6 69 5B 91 C8 FE 95 EF 20 68 " +
            "1C E4 D2 81 3A 78 67 83 BF 86 5E 95 46 33 3D BD " +
            "D2 01 1F 08 4F 2A F8 04 2A 38 70 53 CE DE 91 7C " +
            "C3 C0 B3 8F 52 49 BE FC 4D 8C 1C BD 99 8D 2A 98 " +
            "C6 92 B4 EC F3 51 63 50 C3 4F 3E 64 83 97 EC 35 " +
            "95 BC 60 D1 C0 B0 B2 ED 40 02 ED 7A 10 B9 3C 79 " +
            "EB 69 E7 6B 02 09 0F 48 18 29 D4 34 F8 BF 2B F3 " +
            "A6 F1 E9 C0 69 B5 D2 17 3F A1 20 2D 7F 6A AD E5 " +
            "3F C9 D5 76 90 65 12 96 4C E8 E5 BA 05 36 BB 7A " +
            "7F 5E 36 B7 8D D5 6C A5 EE 44 EB 00 9E 03 CD 65 " +
            "75 8E D7 44 2B 18 72 AE 48 26 AA 33 65 22 B4 77 " +
            "FC 8C 9E F0 8D 7F B8 50",
            // Round #22  
            // After Theta
            "F7 B4 13 88 F4 10 64 37 24 BA 9E 3D 78 C4 50 63 " +
            "D3 3F 15 6F 0E FF 1F 1F 58 9F FA 11 A9 12 F9 29 " +
            "C9 98 74 23 D9 6B DD DF 6B B1 62 11 92 0E 1F 19 " +
            "C0 69 15 55 72 21 12 4A 1B 68 14 02 F4 AA 28 0A " +
            "C0 CE 81 60 6E B4 67 BD 98 F0 BA 1F 7A 9E 90 C4 " +
            "12 A5 88 68 27 6C 41 F4 D1 27 34 39 BE 9C 06 7B " +
            "A4 EC 04 80 FA C4 0B 9B 43 0C DF 95 2C 44 E5 38 " +
            "3E 15 41 C9 E1 1A B5 14 CC 1E E8 B0 2C 82 09 57 " +
            "B4 99 E3 9D 54 BE 38 59 0E F1 44 7C 45 1E 14 93 " +
            "3C C7 E7 99 AC 98 CB D7 99 94 43 18 E6 25 01 26 " +
            "AB 69 0A 33 59 E8 4E 01 FC 2C E1 5D A3 08 27 2B " +
            "44 DE B3 15 11 6C CB D8 4B 28 98 DC 59 DF 6D 36 " +
            "29 F0 38 52 6E 6C 02 0C",
            // After Rho  
            "F7 B4 13 88 F4 10 64 37 48 74 3D 7B F0 88 A1 C6 " +
            "F4 4F C5 9B C3 FF C7 C7 2A 91 9F 82 F5 A9 1F 91 " +
            "5E EB FE 4E C6 A4 1B C9 21 E9 F0 91 B1 16 2B 16 " +
            "51 25 17 22 A1 04 9C 56 C2 06 1A 85 00 BD 2A 8A " +
            "E7 40 30 37 DA B3 5E 60 09 49 8C 09 AF FB A1 E7 " +
            "97 28 45 44 3B 61 0B A2 EC 45 9F D0 E4 F8 72 1A " +
            "00 D4 27 5E D8 24 65 27 88 CA 71 86 18 BE 2B 59 " +
            "E4 70 8D 5A 0A 9F 8A A0 61 59 04 13 AE 98 3D D0 " +
            "BC 93 CA 17 27 8B 36 73 8A 49 87 78 22 BE 22 0F " +
            "73 F9 9A E7 F8 3C 93 15 26 99 94 43 18 E6 25 01 " +
            "3B 05 AC A6 29 CC 64 A1 F0 B3 84 77 8D 22 9C AC " +
            "C8 7B B6 22 82 6D 19 9B 28 98 DC 59 DF 6D 36 4B " +
            "00 43 0A 3C 8E 94 1B 9B",
            // After Pi
            "F7 B4 13 88 F4 10 64 37 51 25 17 22 A1 04 9C 56 " +
            "00 D4 27 5E D8 24 65 27 73 F9 9A E7 F8 3C 93 15 " +
            "00 43 0A 3C 8E 94 1B 9B 2A 91 9F 82 F5 A9 1F 91 " +
            "09 49 8C 09 AF FB A1 E7 97 28 45 44 3B 61 0B A2 " +
            "BC 93 CA 17 27 8B 36 73 C8 7B B6 22 82 6D 19 9B " +
            "48 74 3D 7B F0 88 A1 C6 C2 06 1A 85 00 BD 2A 8A " +
            "88 CA 71 86 18 BE 2B 59 26 99 94 43 18 E6 25 01 " +
            "3B 05 AC A6 29 CC 64 A1 5E EB FE 4E C6 A4 1B C9 " +
            "21 E9 F0 91 B1 16 2B 16 EC 45 9F D0 E4 F8 72 1A " +
            "8A 49 87 78 22 BE 22 0F 28 98 DC 59 DF 6D 36 4B " +
            "F4 4F C5 9B C3 FF C7 C7 E7 40 30 37 DA B3 5E 60 " +
            "E4 70 8D 5A 0A 9F 8A A0 61 59 04 13 AE 98 3D D0 " +
            "F0 B3 84 77 8D 22 9C AC",
            // After Chi  
            "F7 64 33 D4 AC 30 05 16 22 0C 8F 83 81 1C 0E 46 " +
            "00 D6 27 46 DE A4 6D AD 84 4D 8B 67 88 3C F7 31 " +
            "00 42 0E 1E 8F 90 83 DB BC B1 DE C6 E5 A9 15 91 " +
            "21 DA 06 1A AB 71 95 B6 D7 40 71 64 BB 05 02 2A " +
            "9E 13 C3 97 52 0B 30 73 C9 33 B6 2B 88 3F B9 FD " +
            "40 BC 5C 79 E8 8A A0 97 E4 17 9E C4 00 FD 2E 8A " +
            "91 CE 59 22 39 B6 6B F9 66 E9 85 1A C8 E6 A4 47 " +
            "B9 07 AE 22 29 F9 6E A9 92 EF F1 0E 82 4C 4B C1 " +
            "23 E1 F0 B9 B3 10 2B 13 CC D5 C7 D1 39 B9 66 5A " +
            "DC 2A A5 7E 22 3E 2B 8F 09 98 DC C8 EE 7F 16 5D " +
            "F4 7F 48 D3 C3 F3 47 47 E6 49 30 36 7E B3 6B 30 " +
            "74 D2 0D 3E 0B BD 0A 8C 65 15 45 9B EC 45 7E 93 " +
            "F3 B3 B4 53 95 22 84 8C",
            // After Iota 
            "F6 64 33 54 AC 30 05 16 22 0C 8F 83 81 1C 0E 46 " +
            "00 D6 27 46 DE A4 6D AD 84 4D 8B 67 88 3C F7 31 " +
            "00 42 0E 1E 8F 90 83 DB BC B1 DE C6 E5 A9 15 91 " +
            "21 DA 06 1A AB 71 95 B6 D7 40 71 64 BB 05 02 2A " +
            "9E 13 C3 97 52 0B 30 73 C9 33 B6 2B 88 3F B9 FD " +
            "40 BC 5C 79 E8 8A A0 97 E4 17 9E C4 00 FD 2E 8A " +
            "91 CE 59 22 39 B6 6B F9 66 E9 85 1A C8 E6 A4 47 " +
            "B9 07 AE 22 29 F9 6E A9 92 EF F1 0E 82 4C 4B C1 " +
            "23 E1 F0 B9 B3 10 2B 13 CC D5 C7 D1 39 B9 66 5A " +
            "DC 2A A5 7E 22 3E 2B 8F 09 98 DC C8 EE 7F 16 5D " +
            "F4 7F 48 D3 C3 F3 47 47 E6 49 30 36 7E B3 6B 30 " +
            "74 D2 0D 3E 0B BD 0A 8C 65 15 45 9B EC 45 7E 93 " +
            "F3 B3 B4 53 95 22 84 8C",
            // Round #23  
            // After Theta
            "38 EB E3 7D 36 5C 29 FB B3 4A 0D 6A BC 96 62 80 " +
            "A8 AE AB 8A 81 C2 F5 C6 6E A9 B2 90 4D 39 13 24 " +
            "1C 38 32 7D 93 63 CC EF 72 3E 0E EF 7F C5 39 7C " +
            "B0 9C 84 F3 96 FB F9 70 7F 38 FD A8 E4 63 9A 41 " +
            "74 F7 FA 60 97 0E D4 66 D5 49 8A 48 94 CC F6 C9 " +
            "8E 33 8C 50 72 E6 8C 7A 75 51 1C 2D 3D 77 42 4C " +
            "39 B6 D5 EE 66 D0 F3 92 8C 0D BC ED 0D E3 40 52 " +
            "A5 7D 92 41 35 0A 21 9D 5C 60 21 27 18 20 67 2C " +
            "B2 A7 72 50 8E 9A 47 D5 64 AD 4B 1D 66 DF FE 31 " +
            "36 CE 9C 89 E7 3B CF 9A 15 E2 E0 AB F2 8C 59 69 " +
            "3A F0 98 FA 59 9F 6B AA 77 0F B2 DF 43 39 07 F6 " +
            "DC AA 81 F2 54 DB 92 E7 8F F1 7C 6C 29 40 9A 86 " +
            "EF C9 88 30 89 D1 CB B8",
            // After Rho  
            "38 EB E3 7D 36 5C 29 FB 67 95 1A D4 78 2D C5 00 " +
            "AA EB AA 62 A0 70 BD 31 94 33 41 E2 96 2A 0B D9 " +
            "1C 63 7E E7 C0 91 E9 9B FE 57 9C C3 27 E7 E3 F0 " +
            "38 6F B9 9F 0F 07 CB 49 D0 1F 4E 3F 2A F9 98 66 " +
            "7B 7D B0 4B 07 6A 33 BA 6C 9F 5C 9D A4 88 44 C9 " +
            "73 9C 61 84 92 33 67 D4 31 D5 45 71 B4 F4 DC 09 " +
            "76 37 83 9E 97 CC B1 AD C6 81 A4 18 1B 78 DB 1B " +
            "A0 1A 85 90 CE D2 3E C9 4E 30 40 CE 58 B8 C0 42 " +
            "0E CA 51 F3 A8 5A F6 54 FF 18 B2 D6 A5 0E B3 6F " +
            "E7 59 D3 C6 99 33 F1 7C 69 15 E2 E0 AB F2 8C 59 " +
            "AE A9 EA C0 63 EA 67 7D DF 3D C8 7E 0F E5 1C D8 " +
            "5B 35 50 9E 6A 5B F2 9C F1 7C 6C 29 40 9A 86 8F " +
            "32 EE 7B 32 22 4C 62 F4",
            // After Pi
            "38 EB E3 7D 36 5C 29 FB 38 6F B9 9F 0F 07 CB 49 " +
            "76 37 83 9E 97 CC B1 AD E7 59 D3 C6 99 33 F1 7C " +
            "32 EE 7B 32 22 4C 62 F4 94 33 41 E2 96 2A 0B D9 " +
            "6C 9F 5C 9D A4 88 44 C9 73 9C 61 84 92 33 67 D4 " +
            "0E CA 51 F3 A8 5A F6 54 5B 35 50 9E 6A 5B F2 9C " +
            "67 95 1A D4 78 2D C5 00 D0 1F 4E 3F 2A F9 98 66 " +
            "C6 81 A4 18 1B 78 DB 1B 69 15 E2 E0 AB F2 8C 59 " +
            "AE A9 EA C0 63 EA 67 7D 1C 63 7E E7 C0 91 E9 9B " +
            "FE 57 9C C3 27 E7 E3 F0 31 D5 45 71 B4 F4 DC 09 " +
            "FF 18 B2 D6 A5 0E B3 6F F1 7C 6C 29 40 9A 86 8F " +
            "AA EB AA 62 A0 70 BD 31 7B 7D B0 4B 07 6A 33 BA " +
            "A0 1A 85 90 CE D2 3E C9 4E 30 40 CE 58 B8 C0 42 " +
            "DF 3D C8 7E 0F E5 1C D8",
            // After Chi  
            "7E FB E1 7D A6 94 19 5F B9 27 E9 DF 07 34 8B 19 " +
            "66 91 AB AE B5 80 B3 2D EF 58 53 8B 8D 23 F8 77 " +
            "32 EA 63 B0 2B 4F A0 F4 87 33 60 E2 84 19 28 CD " +
            "60 DD 4C EE 8C C0 D4 C9 22 A9 61 88 D0 32 67 5C " +
            "8A C8 50 93 3C 7A FF 15 33 B9 4C 83 4A DB B6 9C " +
            "61 15 BA D4 69 2D 86 19 F9 0B 0C DF 8A 7B 9C 26 " +
            "40 29 AC 18 5B 70 B8 3F 28 01 F2 F4 B3 F7 0C 59 " +
            "3E A3 AE EB 61 3A 7F 1B 1D E3 3F D7 50 81 F5 92 " +
            "30 5F 2E 45 26 ED C0 96 31 B1 09 58 F4 64 D8 89 " +
            "F3 1B A0 10 25 0F DA 7F 13 68 EC 29 67 FC 84 EF " +
            "2A E9 AF F2 68 E0 B1 70 35 5D F0 05 17 42 F3 B8 " +
            "31 17 0D A0 C9 97 22 51 6E F2 62 CE F8 A8 61 63 " +
            "8E 29 D8 77 08 EF 1E 52",
            // After Iota 
            "76 7B E1 FD A6 94 19 DF B9 27 E9 DF 07 34 8B 19 " +
            "66 91 AB AE B5 80 B3 2D EF 58 53 8B 8D 23 F8 77 " +
            "32 EA 63 B0 2B 4F A0 F4 87 33 60 E2 84 19 28 CD " +
            "60 DD 4C EE 8C C0 D4 C9 22 A9 61 88 D0 32 67 5C " +
            "8A C8 50 93 3C 7A FF 15 33 B9 4C 83 4A DB B6 9C " +
            "61 15 BA D4 69 2D 86 19 F9 0B 0C DF 8A 7B 9C 26 " +
            "40 29 AC 18 5B 70 B8 3F 28 01 F2 F4 B3 F7 0C 59 " +
            "3E A3 AE EB 61 3A 7F 1B 1D E3 3F D7 50 81 F5 92 " +
            "30 5F 2E 45 26 ED C0 96 31 B1 09 58 F4 64 D8 89 " +
            "F3 1B A0 10 25 0F DA 7F 13 68 EC 29 67 FC 84 EF " +
            "2A E9 AF F2 68 E0 B1 70 35 5D F0 05 17 42 F3 B8 " +
            "31 17 0D A0 C9 97 22 51 6E F2 62 CE F8 A8 61 63 " +
            "8E 29 D8 77 08 EF 1E 52",
            // Xor'd state (in bytes)
            "76 7B E1 FD A6 94 19 DF B9 27 E9 DF 07 34 8B 19 " +
            "66 91 AB AE B5 80 B3 2D EF 58 53 8B 8D 23 F8 77 " +
            "32 EA 63 B0 2B 4F A0 F4 87 33 60 E2 84 19 28 CD " +
            "60 DD 4C EE 8C C0 D4 C9 22 A9 61 88 D0 32 67 5C " +
            "8A C8 50 93 3C 7A FF 15 33 B9 4C 83 4A DB B6 9C " +
            "61 15 BA D4 69 2D 86 19 F9 0B 0C DF 8A 7B 9C 26 " +
            "40 29 AC 18 5B 70 B8 3F 28 01 F2 F4 B3 F7 0C 59 " +
            "3E A3 AE EB 61 3A 7F 1B 1D E3 3F D7 50 81 F5 92 " +
            "30 5F 2E 45 26 ED C0 96 31 B1 09 58 F4 64 D8 89 " +
            "F3 1B A0 10 25 0F DA 7F 13 68 EC 29 67 FC 84 EF " +
            "2A E9 AF F2 68 E0 B1 70 35 5D F0 05 17 42 F3 B8 " +
            "31 17 0D A0 C9 97 22 51 6E F2 62 CE F8 A8 61 63 " +
            "8E 29 D8 77 08 EF 1E 52",
            // Round #0
            // After Theta
            "9F 2C BB 27 A8 69 0A A0 17 1E 87 BD 73 97 54 DD " +
            "E3 93 BA 64 3B B3 23 BA AE 8C 5A 40 51 68 89 7C " +
            "AD 3D 06 5F 13 C4 F7 00 6E 64 3A 38 8A E4 3B B2 " +
            "CE E4 22 8C F8 63 0B 0D A7 AB 70 42 5E 01 F7 CB " +
            "CB 1C 59 58 E0 31 8E 1E AC 6E 29 6C 72 50 E1 68 " +
            "88 42 E0 0E 67 D0 95 66 57 32 62 BD FE D8 43 E2 " +
            "C5 2B BD D2 D5 43 28 A8 69 D5 FB 3F 6F BC 7D 52 " +
            "A1 74 CB 04 59 B1 28 EF F4 B4 65 0D 5E 7C E6 ED " +
            "9E 66 40 27 52 4E 1F 52 B4 B3 18 92 7A 57 48 1E " +
            "B2 CF A9 DB F9 44 AB 74 8C BF 89 C6 5F 77 D3 1B " +
            "C3 BE F5 28 66 1D A2 0F 9B 64 9E 67 63 E1 2C 7C " +
            "B4 15 1C 6A 47 A4 B2 C6 2F 26 6B 05 24 E3 10 68 " +
            "11 FE BD 98 30 64 49 A6",
            // After Rho  
            "9F 2C BB 27 A8 69 0A A0 2F 3C 0E 7B E7 2E A9 BA " +
            "F8 A4 2E D9 CE EC 88 EE 85 96 C8 E7 CA A8 05 14 " +
            "20 BE 07 68 ED 31 F8 9A A3 48 BE 23 EB 46 A6 83 " +
            "C2 88 3F B6 D0 E0 4C 2E F2 E9 2A 9C 90 57 C0 FD " +
            "8E 2C 2C F0 18 47 8F 65 15 8E C6 EA 96 C2 26 07 " +
            "43 14 02 77 38 83 AE 34 89 5F C9 88 F5 FA 63 0F " +
            "95 AE 1E 42 41 2D 5E E9 78 FB A4 D2 AA F7 7F DE " +
            "82 AC 58 94 F7 50 BA 65 1A BC F8 CC DB E9 69 CB " +
            "E8 44 CA E9 43 CA D3 0C 24 0F DA 59 0C 49 BD 2B " +
            "68 95 4E F6 39 75 3B 9F 1B 8C BF 89 C6 5F 77 D3 " +
            "88 3E 0C FB D6 A3 98 75 6D 92 79 9E 8D 85 B3 F0 " +
            "B6 82 43 ED 88 54 D6 98 26 6B 05 24 E3 10 68 2F " +
            "92 69 84 7F 2F 26 0C 59",
            // After Pi
            "9F 2C BB 27 A8 69 0A A0 C2 88 3F B6 D0 E0 4C 2E " +
            "95 AE 1E 42 41 2D 5E E9 68 95 4E F6 39 75 3B 9F " +
            "92 69 84 7F 2F 26 0C 59 85 96 C8 E7 CA A8 05 14 " +
            "15 8E C6 EA 96 C2 26 07 43 14 02 77 38 83 AE 34 " +
            "E8 44 CA E9 43 CA D3 0C B6 82 43 ED 88 54 D6 98 " +
            "2F 3C 0E 7B E7 2E A9 BA F2 E9 2A 9C 90 57 C0 FD " +
            "78 FB A4 D2 AA F7 7F DE 1B 8C BF 89 C6 5F 77 D3 " +
            "88 3E 0C FB D6 A3 98 75 20 BE 07 68 ED 31 F8 9A " +
            "A3 48 BE 23 EB 46 A6 83 89 5F C9 88 F5 FA 63 0F " +
            "24 0F DA 59 0C 49 BD 2B 26 6B 05 24 E3 10 68 2F " +
            "F8 A4 2E D9 CE EC 88 EE 8E 2C 2C F0 18 47 8F 65 " +
            "82 AC 58 94 F7 50 BA 65 1A BC F8 CC DB E9 69 CB " +
            "6D 92 79 9E 8D 85 B3 F0",
            // After Chi  
            "8A 0A BB 67 A9 64 18 61 AA 99 7F 02 E8 B0 6D 38 " +
            "07 C6 9E 4B 47 2F 5A A9 65 91 75 F6 B9 3C 39 3F " +
            "D2 E9 80 EF 7F A6 48 57 C7 86 C8 F2 E2 A9 8D 24 " +
            "BD CE 0E 62 D5 8A 77 0F 55 96 03 73 B0 97 AA A4 " +
            "E9 50 42 EB 01 62 D2 08 A6 8A 45 E5 9C 16 F4 9B " +
            "27 2E 8A 39 CD 8E 96 B8 F1 ED 31 95 D4 5F C0 FC " +
            "F8 C9 A4 A0 BA 57 F7 FA 3C 8C BD 89 E7 53 56 59 " +
            "58 FF 2C 7F C6 F2 D8 30 28 A9 46 E0 F9 89 B9 96 " +
            "87 48 AC 72 E3 47 3A A3 8B 3F CC AC 16 EA 23 0B " +
            "24 9B D8 11 00 68 2D BB A5 2B BD 27 E1 56 6E 2E " +
            "F8 24 7E DD 29 FC B8 EE 96 3C 8C B8 10 EE CE EF " +
            "E7 AE 59 86 F3 54 28 55 8A 98 FE 8D 99 81 61 C5 " +
            "6B 9A 79 BE 9D 86 B4 F1",
            // After Iota 
            "8B 0A BB 67 A9 64 18 61 AA 99 7F 02 E8 B0 6D 38 " +
            "07 C6 9E 4B 47 2F 5A A9 65 91 75 F6 B9 3C 39 3F " +
            "D2 E9 80 EF 7F A6 48 57 C7 86 C8 F2 E2 A9 8D 24 " +
            "BD CE 0E 62 D5 8A 77 0F 55 96 03 73 B0 97 AA A4 " +
            "E9 50 42 EB 01 62 D2 08 A6 8A 45 E5 9C 16 F4 9B " +
            "27 2E 8A 39 CD 8E 96 B8 F1 ED 31 95 D4 5F C0 FC " +
            "F8 C9 A4 A0 BA 57 F7 FA 3C 8C BD 89 E7 53 56 59 " +
            "58 FF 2C 7F C6 F2 D8 30 28 A9 46 E0 F9 89 B9 96 " +
            "87 48 AC 72 E3 47 3A A3 8B 3F CC AC 16 EA 23 0B " +
            "24 9B D8 11 00 68 2D BB A5 2B BD 27 E1 56 6E 2E " +
            "F8 24 7E DD 29 FC B8 EE 96 3C 8C B8 10 EE CE EF " +
            "E7 AE 59 86 F3 54 28 55 8A 98 FE 8D 99 81 61 C5 " +
            "6B 9A 79 BE 9D 86 B4 F1",
            // Round #1
            // After Theta
            "86 BA 57 F5 C4 6E FB 4C 9C A7 E6 F6 EF 25 77 EF " +
            "CC 94 A6 65 D1 2A 97 0F 67 C2 83 9C A2 49 48 D1 " +
            "BB F8 AE C4 14 2E BD 4D CA 36 24 60 8F A3 6E 09 " +
            "8B F0 97 96 D2 1F 6D D8 9E C4 3B 5D 26 92 67 02 " +
            "EB 03 B4 81 1A 17 A3 E6 CF 9B 6B CE F7 9E 01 81 " +
            "2A 9E 66 AB A0 84 75 95 C7 D3 A8 61 D3 CA DA 2B " +
            "33 9B 9C 8E 2C 52 3A 5C 3E DF 4B E3 FC 26 27 B7 " +
            "31 EE 02 54 AD 7A 2D 2A 25 19 AA 72 94 83 5A BB " +
            "B1 76 35 86 E4 D2 20 74 40 6D F4 82 80 EF EE AD " +
            "26 C8 2E 7B 1B 1D 5C 55 CC 3A 93 0C 8A DE 9B 34 " +
            "F5 94 92 4F 44 F6 5B C3 A0 02 15 4C 17 7B D4 38 " +
            "2C FC 61 A8 65 51 E5 F3 88 CB 08 E7 82 F4 10 2B " +
            "02 8B 57 95 F6 0E 41 EB",
            // After Rho  
            "86 BA 57 F5 C4 6E FB 4C 39 4F CD ED DF 4B EE DE " +
            "33 A5 69 59 B4 CA E5 03 9A 84 14 7D 26 3C C8 29 " +
            "70 E9 6D DA C5 77 25 A6 F6 38 EA 96 A0 6C 43 02 " +
            "69 29 FD D1 86 BD 08 7F 80 27 F1 4E 97 89 E4 99 " +
            "01 DA 40 8D 8B 51 F3 F5 19 10 F8 BC B9 E6 7C EF " +
            "54 F1 34 5B 05 25 AC AB AF 1C 4F A3 86 4D 2B 6B " +
            "74 64 91 D2 E1 9A D9 E4 4D 4E 6E 7D BE 97 C6 F9 " +
            "AA 56 BD 16 95 18 77 01 E5 28 07 B5 76 4B 32 54 " +
            "C6 90 5C 1A 84 2E D6 AE F7 56 A0 36 7A 41 C0 77 " +
            "83 AB CA 04 D9 65 6F A3 34 CC 3A 93 0C 8A DE 9B " +
            "6F 0D D7 53 4A 3E 11 D9 80 0A 54 30 5D EC 51 E3 " +
            "85 3F 0C B5 2C AA 7C 9E CB 08 E7 82 F4 10 2B 88 " +
            "D0 BA C0 E2 55 A5 BD 43",
            // After Pi
            "86 BA 57 F5 C4 6E FB 4C 69 29 FD D1 86 BD 08 7F " +
            "74 64 91 D2 E1 9A D9 E4 83 AB CA 04 D9 65 6F A3 " +
            "D0 BA C0 E2 55 A5 BD 43 9A 84 14 7D 26 3C C8 29 " +
            "19 10 F8 BC B9 E6 7C EF 54 F1 34 5B 05 25 AC AB " +
            "C6 90 5C 1A 84 2E D6 AE 85 3F 0C B5 2C AA 7C 9E " +
            "39 4F CD ED DF 4B EE DE 80 27 F1 4E 97 89 E4 99 " +
            "4D 4E 6E 7D BE 97 C6 F9 34 CC 3A 93 0C 8A DE 9B " +
            "6F 0D D7 53 4A 3E 11 D9 70 E9 6D DA C5 77 25 A6 " +
            "F6 38 EA 96 A0 6C 43 02 AF 1C 4F A3 86 4D 2B 6B " +
            "F7 56 A0 36 7A 41 C0 77 CB 08 E7 82 F4 10 2B 88 " +
            "33 A5 69 59 B4 CA E5 03 01 DA 40 8D 8B 51 F3 F5 " +
            "AA 56 BD 16 95 18 77 01 E5 28 07 B5 76 4B 32 54 " +
            "80 0A 54 30 5D EC 51 E3",
            // After Chi  
            "92 FE 57 F7 A5 6C 2A CC EA A2 B7 D5 9E D8 2E 7C " +
            "24 74 91 30 E5 1A 49 A4 85 AB DD 11 59 2F 2D AF " +
            "B9 BB 68 E2 57 34 BD 70 DE 65 10 3E 22 3D 48 29 " +
            "9B 10 B0 BC 39 EC 2E EB 55 DE 34 FE 2D A5 84 BB " +
            "DC 10 4C 52 86 3A 56 8F 84 2F E4 35 B5 68 48 58 " +
            "74 07 C3 DC F7 5D EC BE B0 A7 E1 CC 97 81 FC 9B " +
            "06 4F AB 3D FC A3 C7 B9 24 8E 32 3F 99 CB 30 9D " +
            "EF 2D E7 51 4A BE 11 D8 79 ED 68 FB C3 76 0D CF " +
            "A6 7A 4A 82 D8 6C 83 16 A7 14 08 23 02 5D 00 E3 " +
            "C7 B7 A8 6E 7B 26 C4 51 4D 18 65 86 D4 18 69 88 " +
            "99 A1 D4 4B A0 C2 E1 03 44 F2 42 2C E9 12 F3 A1 " +
            "AA 54 ED 16 9C BC 36 A2 D6 8D 2E FC D6 49 96 54 " +
            "80 50 54 B4 56 FD 43 17",
            // After Iota 
            "10 7E 57 F7 A5 6C 2A CC EA A2 B7 D5 9E D8 2E 7C " +
            "24 74 91 30 E5 1A 49 A4 85 AB DD 11 59 2F 2D AF " +
            "B9 BB 68 E2 57 34 BD 70 DE 65 10 3E 22 3D 48 29 " +
            "9B 10 B0 BC 39 EC 2E EB 55 DE 34 FE 2D A5 84 BB " +
            "DC 10 4C 52 86 3A 56 8F 84 2F E4 35 B5 68 48 58 " +
            "74 07 C3 DC F7 5D EC BE B0 A7 E1 CC 97 81 FC 9B " +
            "06 4F AB 3D FC A3 C7 B9 24 8E 32 3F 99 CB 30 9D " +
            "EF 2D E7 51 4A BE 11 D8 79 ED 68 FB C3 76 0D CF " +
            "A6 7A 4A 82 D8 6C 83 16 A7 14 08 23 02 5D 00 E3 " +
            "C7 B7 A8 6E 7B 26 C4 51 4D 18 65 86 D4 18 69 88 " +
            "99 A1 D4 4B A0 C2 E1 03 44 F2 42 2C E9 12 F3 A1 " +
            "AA 54 ED 16 9C BC 36 A2 D6 8D 2E FC D6 49 96 54 " +
            "80 50 54 B4 56 FD 43 17",
            // Round #2
            // After Theta
            "48 B5 D0 54 8D FD FD D4 45 B8 58 FD D8 9B 35 25 " +
            "DE F7 35 E7 33 B2 F6 6F C1 EC 83 BF A6 DC 8D 97 " +
            "60 14 3D 46 9B F5 61 E6 86 AE 97 9D 0A AC 9F 31 " +
            "34 0A 5F 94 7F AF 35 B2 AF 5D 90 29 FB 0D 3B 70 " +
            "98 57 12 FC 79 C9 F6 B7 5D 80 B1 91 79 A9 94 CE " +
            "2C CC 44 7F DF CC 3B A6 1F BD 0E E4 D1 C2 E7 C2 " +
            "FC CC 0F EA 2A 0B 78 72 60 C9 6C 91 66 38 90 A5 " +
            "36 82 B2 F5 86 7F CD 4E 21 26 EF 58 EB E7 DA D7 " +
            "09 60 A5 AA 9E 2F 98 4F 5D 97 AC F4 D4 F5 BF 28 " +
            "83 F0 F6 C0 84 D5 64 69 94 B7 30 22 18 D9 B5 1E " +
            "C1 6A 53 E8 88 53 36 1B EB E8 AD 04 AF 51 E8 F8 " +
            "50 D7 49 C1 4A 14 89 69 92 CA 70 52 29 BA 36 6C " +
            "59 FF 01 10 9A 3C 9F 81",
            // After Rho  
            "48 B5 D0 54 8D FD FD D4 8A 70 B1 FA B1 37 6B 4A " +
            "F7 7D CD F9 8C AC FD 9B CA DD 78 19 CC 3E F8 6B " +
            "AC 0F 33 07 A3 E8 31 DA A9 C0 FA 19 63 E8 7A D9 " +
            "45 F9 F7 5A 23 4B A3 F0 DC 6B 17 64 CA 7E C3 0E " +
            "2B 09 FE BC 64 FB 5B CC 4A E9 DC 05 18 1B 99 97 " +
            "65 61 26 FA FB 66 DE 31 0B 7F F4 3A 90 47 0B 9F " +
            "50 57 59 C0 93 E3 67 7E 70 20 4B C1 92 D9 22 CD " +
            "7A C3 BF 66 27 1B 41 D9 B1 D6 CF B5 AF 43 4C DE " +
            "54 D5 F3 05 F3 29 01 AC 5F 94 AE 4B 56 7A EA FA " +
            "9A 2C 6D 10 DE 1E 98 B0 1E 94 B7 30 22 18 D9 B5 " +
            "D9 6C 04 AB 4D A1 23 4E AF A3 B7 12 BC 46 A1 E3 " +
            "EA 3A 29 58 89 22 31 0D CA 70 52 29 BA 36 6C 92 " +
            "67 60 D6 7F 00 84 26 CF",
            // After Pi
            "48 B5 D0 54 8D FD FD D4 45 F9 F7 5A 23 4B A3 F0 " +
            "50 57 59 C0 93 E3 67 7E 9A 2C 6D 10 DE 1E 98 B0 " +
            "67 60 D6 7F 00 84 26 CF CA DD 78 19 CC 3E F8 6B " +
            "4A E9 DC 05 18 1B 99 97 65 61 26 FA FB 66 DE 31 " +
            "54 D5 F3 05 F3 29 01 AC EA 3A 29 58 89 22 31 0D " +
            "8A 70 B1 FA B1 37 6B 4A DC 6B 17 64 CA 7E C3 0E " +
            "70 20 4B C1 92 D9 22 CD 1E 94 B7 30 22 18 D9 B5 " +
            "D9 6C 04 AB 4D A1 23 4E AC 0F 33 07 A3 E8 31 DA " +
            "A9 C0 FA 19 63 E8 7A D9 0B 7F F4 3A 90 47 0B 9F " +
            "5F 94 AE 4B 56 7A EA FA CA 70 52 29 BA 36 6C 92 " +
            "F7 7D CD F9 8C AC FD 9B 2B 09 FE BC 64 FB 5B CC " +
            "7A C3 BF 66 27 1B 41 D9 B1 D6 CF B5 AF 43 4C DE " +
            "AF A3 B7 12 BC 46 A1 E3",
            // After Chi  
            "58 B3 D8 D4 1D 5D B9 DA CF D1 D3 4A 6F 57 3B 70 " +
            "35 17 CB AF 93 63 41 31 92 B9 6D 10 53 67 41 A0 " +
            "62 28 F1 75 22 86 24 EF EF DD 5A E3 2F 5A BE 4B " +
            "5A 7D 0D 00 18 12 98 1B CF 4B 2E A2 F3 64 EE 30 " +
            "54 10 A3 04 B7 35 C9 CE EA 1A AD 5C 99 23 30 99 " +
            "AA 70 F9 7B A1 B6 4B 8B D2 FF A3 54 EA 7E 1A 3E " +
            "B1 48 4B 4A DF 78 00 87 1C 84 06 60 92 0E 91 B5 " +
            "8D 67 02 AF 07 E9 A3 4A AE 30 37 25 33 EF 30 DC " +
            "FD 40 F0 58 25 D0 9A B9 8B 1F A4 1A 38 43 0F 9F " +
            "7B 9B 8F 4D 57 B2 FB B2 CB B0 9A 31 FA 36 26 93 " +
            "A7 BF CC BB 8F AC FD 8A AA 1D BE 2D EC BB 57 CA " +
            "74 E2 8F 64 37 1F E0 F8 E1 8A 87 5C AF EB 10 C6 " +
            "A7 A3 85 16 DC 15 A3 A7",
            // After Iota 
            "D2 33 D8 D4 1D 5D B9 5A CF D1 D3 4A 6F 57 3B 70 " +
            "35 17 CB AF 93 63 41 31 92 B9 6D 10 53 67 41 A0 " +
            "62 28 F1 75 22 86 24 EF EF DD 5A E3 2F 5A BE 4B " +
            "5A 7D 0D 00 18 12 98 1B CF 4B 2E A2 F3 64 EE 30 " +
            "54 10 A3 04 B7 35 C9 CE EA 1A AD 5C 99 23 30 99 " +
            "AA 70 F9 7B A1 B6 4B 8B D2 FF A3 54 EA 7E 1A 3E " +
            "B1 48 4B 4A DF 78 00 87 1C 84 06 60 92 0E 91 B5 " +
            "8D 67 02 AF 07 E9 A3 4A AE 30 37 25 33 EF 30 DC " +
            "FD 40 F0 58 25 D0 9A B9 8B 1F A4 1A 38 43 0F 9F " +
            "7B 9B 8F 4D 57 B2 FB B2 CB B0 9A 31 FA 36 26 93 " +
            "A7 BF CC BB 8F AC FD 8A AA 1D BE 2D EC BB 57 CA " +
            "74 E2 8F 64 37 1F E0 F8 E1 8A 87 5C AF EB 10 C6 " +
            "A7 A3 85 16 DC 15 A3 A7",
            // Round #3
            // After Theta
            "9B 69 FF A3 2F 92 63 1E 38 13 58 EB 20 E2 3A 7E " +
            "A4 61 78 0F DB 38 D1 48 F4 DC 6A 6B D6 9B 65 51 " +
            "1F 37 31 B5 F3 67 D5 D9 A6 87 7D 94 1D 95 64 0F " +
            "AD BF 86 A1 57 A7 99 15 5E 3D 9D 02 BB 3F 7E 49 " +
            "32 75 A4 7F 32 C9 ED 3F 97 05 6D 9C 48 C2 C1 AF " +
            "E3 2A DE 0C 93 79 91 CF 25 3D 28 F5 A5 CB 1B 30 " +
            "20 3E F8 EA 97 23 90 FE 7A E1 01 1B 17 F2 B5 44 " +
            "F0 78 C2 6F D6 08 52 7C E7 6A 10 52 01 20 EA 98 " +
            "0A 82 7B F9 6A 65 9B B7 1A 69 17 BA 70 18 9F E6 " +
            "1D FE 88 36 D2 4E DF 43 B6 AF 5A F1 2B D7 D7 A5 " +
            "EE E5 EB CC BD 63 27 CE 5D DF 35 8C A3 0E 56 C4 " +
            "E5 94 3C C4 7F 44 70 81 87 EF 80 27 2A 17 34 37 " +
            "DA BC 45 D6 0D F4 52 91",
            // After Rho  
            "9B 69 FF A3 2F 92 63 1E 70 26 B0 D6 41 C4 75 FC " +
            "69 18 DE C3 36 4E 34 12 BD 59 16 45 CF AD B6 66 " +
            "3F AB CE FE B8 89 A9 9D D9 51 49 F6 60 7A D8 47 " +
            "18 7A 75 9A 59 D1 FA 6B 92 57 4F A7 C0 EE 8F 5F " +
            "3A D2 3F 99 E4 F6 1F 99 1C FC 7A 59 D0 C6 89 24 " +
            "1E 57 F1 66 98 CC 8B 7C C0 94 F4 A0 D4 97 2E 6F " +
            "57 BF 1C 81 F4 07 F1 C1 E4 6B 89 F4 C2 03 36 2E " +
            "37 6B 04 29 3E 78 3C E1 A4 02 40 D4 31 CF D5 20 " +
            "2F 5F AD 6C F3 56 41 70 4F 73 8D B4 0B 5D 38 8C " +
            "E9 7B A8 C3 1F D1 46 DA A5 B6 AF 5A F1 2B D7 D7 " +
            "9D 38 BB 97 AF 33 F7 8E 77 7D D7 30 8E 3A 58 11 " +
            "9C 92 87 F8 8F 08 2E B0 EF 80 27 2A 17 34 37 87 " +
            "54 A4 36 6F 91 75 03 BD",
            // After Pi
            "9B 69 FF A3 2F 92 63 1E 18 7A 75 9A 59 D1 FA 6B " +
            "57 BF 1C 81 F4 07 F1 C1 E9 7B A8 C3 1F D1 46 DA " +
            "54 A4 36 6F 91 75 03 BD BD 59 16 45 CF AD B6 66 " +
            "1C FC 7A 59 D0 C6 89 24 1E 57 F1 66 98 CC 8B 7C " +
            "2F 5F AD 6C F3 56 41 70 9C 92 87 F8 8F 08 2E B0 " +
            "70 26 B0 D6 41 C4 75 FC 92 57 4F A7 C0 EE 8F 5F " +
            "E4 6B 89 F4 C2 03 36 2E A5 B6 AF 5A F1 2B D7 D7 " +
            "9D 38 BB 97 AF 33 F7 8E 3F AB CE FE B8 89 A9 9D " +
            "D9 51 49 F6 60 7A D8 47 C0 94 F4 A0 D4 97 2E 6F " +
            "4F 73 8D B4 0B 5D 38 8C EF 80 27 2A 17 34 37 87 " +
            "69 18 DE C3 36 4E 34 12 3A D2 3F 99 E4 F6 1F 99 " +
            "37 6B 04 29 3E 78 3C E1 A4 02 40 D4 31 CF D5 20 " +
            "77 7D D7 30 8E 3A 58 11",
            // After Chi  
            "DC EC F7 A2 8B 94 62 9E B0 3A D5 D8 52 01 FC 71 " +
            "43 3B 0A AD 74 23 F0 E4 62 32 61 43 31 53 26 D8 " +
            "54 B6 36 77 C1 34 9B DC BF 5A 97 63 C7 A5 B4 3E " +
            "3D F4 76 51 B3 D4 C9 24 8E D7 F3 F6 94 C4 A5 FC " +
            "0E 16 BD 69 B3 F3 D1 36 9C 36 EF E0 9F 4A 27 B0 " +
            "14 0E 30 86 43 C5 45 DC 93 C3 69 AD F1 C6 4E 8E " +
            "FC 63 99 71 CC 13 16 26 C5 B0 AF 1A B1 EF D7 A7 " +
            "1F 69 F4 B6 2F 19 7D 8D 3F 2F 7A FE 2C 0C 8F B5 " +
            "D6 32 40 E2 6B 32 C8 C7 60 14 D6 AA C0 B7 29 6C " +
            "5F 58 45 60 A3 D4 B0 94 2F D0 26 2A 57 46 67 C5 " +
            "6C 31 DE E3 2C 46 14 72 BA D2 7F 4D E5 71 DE 99 " +
            "64 16 93 09 B0 48 34 F0 AC 02 48 17 01 8B F1 22 " +
            "65 BF F6 28 4E 8A 53 98",
            // After Iota 
            "DC 6C F7 22 8B 94 62 1E B0 3A D5 D8 52 01 FC 71 " +
            "43 3B 0A AD 74 23 F0 E4 62 32 61 43 31 53 26 D8 " +
            "54 B6 36 77 C1 34 9B DC BF 5A 97 63 C7 A5 B4 3E " +
            "3D F4 76 51 B3 D4 C9 24 8E D7 F3 F6 94 C4 A5 FC " +
            "0E 16 BD 69 B3 F3 D1 36 9C 36 EF E0 9F 4A 27 B0 " +
            "14 0E 30 86 43 C5 45 DC 93 C3 69 AD F1 C6 4E 8E " +
            "FC 63 99 71 CC 13 16 26 C5 B0 AF 1A B1 EF D7 A7 " +
            "1F 69 F4 B6 2F 19 7D 8D 3F 2F 7A FE 2C 0C 8F B5 " +
            "D6 32 40 E2 6B 32 C8 C7 60 14 D6 AA C0 B7 29 6C " +
            "5F 58 45 60 A3 D4 B0 94 2F D0 26 2A 57 46 67 C5 " +
            "6C 31 DE E3 2C 46 14 72 BA D2 7F 4D E5 71 DE 99 " +
            "64 16 93 09 B0 48 34 F0 AC 02 48 17 01 8B F1 22 " +
            "65 BF F6 28 4E 8A 53 98",
            // Round #4
            // After Theta
            "A4 30 E1 16 DE 9E 4D A8 FF 06 6A 10 E4 A9 48 0E " +
            "84 4A 02 A8 C8 52 5F 9F 6C B2 BF 8D BD 0E 93 03 " +
            "46 34 A0 85 4F 58 EB 55 C7 06 81 57 92 AF 9B 88 " +
            "72 C8 C9 99 05 7C 7D 5B 49 A6 FB F3 28 B5 0A 87 " +
            "00 96 63 A7 3F AE 64 ED 8E B4 79 12 11 26 57 39 " +
            "6C 52 26 B2 16 CF 6A 6A DC FF D6 65 47 6E FA F1 " +
            "3B 12 91 74 70 62 B9 5D CB 30 71 D4 3D B2 62 7C " +
            "0D EB 62 44 A1 75 0D 04 47 73 6C CA 79 06 A0 03 " +
            "99 0E FF 2A DD 9A 7C B8 A7 65 DE AF 7C C6 86 17 " +
            "51 D8 9B AE 2F 89 05 4F 3D 52 B0 D8 D9 2A 17 4C " +
            "14 6D C8 D7 79 4C 3B C4 F5 EE C0 85 53 D9 6A E6 " +
            "A3 67 9B 0C 0C 39 9B 8B A2 82 96 D9 8D D6 44 F9 " +
            "77 3D 60 DA C0 E6 23 11",
            // After Rho  
            "A4 30 E1 16 DE 9E 4D A8 FE 0D D4 20 C8 53 91 1C " +
            "A1 92 00 2A B2 D4 D7 27 EB 30 39 C0 26 FB DB D8 " +
            "C2 5A AF 32 A2 01 2D 7C 25 F9 BA 89 78 6C 10 78 " +
            "9C 59 C0 D7 B7 25 87 9C 61 92 E9 FE 3C 4A AD C2 " +
            "CB B1 D3 1F 57 B2 76 00 72 95 E3 48 9B 27 11 61 " +
            "63 93 32 91 B5 78 56 53 C7 73 FF 5B 97 1D B9 E9 " +
            "A4 83 13 CB ED DA 91 88 64 C5 F8 96 61 E2 A8 7B " +
            "A2 D0 BA 06 82 86 75 31 94 F3 0C 40 07 8E E6 D8 " +
            "5F A5 5B 93 0F 37 D3 E1 C3 8B D3 32 EF 57 3E 63 " +
            "B1 E0 29 0A 7B D3 F5 25 4C 3D 52 B0 D8 D9 2A 17 " +
            "ED 10 53 B4 21 5F E7 31 D7 BB 03 17 4E 65 AB 99 " +
            "F4 6C 93 81 21 67 73 71 82 96 D9 8D D6 44 F9 A2 " +
            "48 C4 5D 0F 98 36 B0 F9",
            // After Pi
            "A4 30 E1 16 DE 9E 4D A8 9C 59 C0 D7 B7 25 87 9C " +
            "A4 83 13 CB ED DA 91 88 B1 E0 29 0A 7B D3 F5 25 " +
            "48 C4 5D 0F 98 36 B0 F9 EB 30 39 C0 26 FB DB D8 " +
            "72 95 E3 48 9B 27 11 61 63 93 32 91 B5 78 56 53 " +
            "5F A5 5B 93 0F 37 D3 E1 F4 6C 93 81 21 67 73 71 " +
            "FE 0D D4 20 C8 53 91 1C 61 92 E9 FE 3C 4A AD C2 " +
            "64 C5 F8 96 61 E2 A8 7B 4C 3D 52 B0 D8 D9 2A 17 " +
            "ED 10 53 B4 21 5F E7 31 C2 5A AF 32 A2 01 2D 7C " +
            "25 F9 BA 89 78 6C 10 78 C7 73 FF 5B 97 1D B9 E9 " +
            "C3 8B D3 32 EF 57 3E 63 82 96 D9 8D D6 44 F9 A2 " +
            "A1 92 00 2A B2 D4 D7 27 CB B1 D3 1F 57 B2 76 00 " +
            "A2 D0 BA 06 82 86 75 31 94 F3 0C 40 07 8E E6 D8 " +
            "D7 BB 03 17 4E 65 AB 99",
            // After Chi  
            "84 B2 F2 1E 96 44 5D A8 8D 39 E8 D7 A5 24 E3 B9 " +
            "EC 87 47 CE 6D FE 91 50 15 D0 89 1A 3D 5B B8 25 " +
            "50 8D 5D CE B9 17 32 ED EA 32 29 51 02 A3 9D CA " +
            "6E B1 AA 4A 91 20 90 C1 C3 DB B2 91 95 38 76 43 " +
            "54 B5 73 D3 09 AF 5B 69 E4 E9 51 89 B8 63 73 50 " +
            "FA 48 C4 20 89 F3 91 25 69 AA EB DE A4 53 AF C6 " +
            "C5 C5 F9 92 40 E4 6D 5B 5E 30 D6 B0 10 D9 3A 1B " +
            "EC 82 7A 6A 15 57 CB F3 00 58 EA 60 25 10 84 FD " +
            "25 71 BA A9 10 2E 16 7A C7 67 F7 D6 87 1D 78 69 " +
            "83 C3 F5 00 CF 56 3A 3F A7 37 C9 04 8E 28 E9 A2 " +
            "81 D2 28 2A 32 D0 D6 16 DF 92 D7 5F 52 BA F4 C8 " +
            "E1 D8 B9 11 CA E7 7C 30 B4 F3 0C 68 B7 1E B2 FE " +
            "9D 9A D0 02 0B 47 8B 99",
            // After Iota 
            "0F 32 F2 1E 96 44 5D A8 8D 39 E8 D7 A5 24 E3 B9 " +
            "EC 87 47 CE 6D FE 91 50 15 D0 89 1A 3D 5B B8 25 " +
            "50 8D 5D CE B9 17 32 ED EA 32 29 51 02 A3 9D CA " +
            "6E B1 AA 4A 91 20 90 C1 C3 DB B2 91 95 38 76 43 " +
            "54 B5 73 D3 09 AF 5B 69 E4 E9 51 89 B8 63 73 50 " +
            "FA 48 C4 20 89 F3 91 25 69 AA EB DE A4 53 AF C6 " +
            "C5 C5 F9 92 40 E4 6D 5B 5E 30 D6 B0 10 D9 3A 1B " +
            "EC 82 7A 6A 15 57 CB F3 00 58 EA 60 25 10 84 FD " +
            "25 71 BA A9 10 2E 16 7A C7 67 F7 D6 87 1D 78 69 " +
            "83 C3 F5 00 CF 56 3A 3F A7 37 C9 04 8E 28 E9 A2 " +
            "81 D2 28 2A 32 D0 D6 16 DF 92 D7 5F 52 BA F4 C8 " +
            "E1 D8 B9 11 CA E7 7C 30 B4 F3 0C 68 B7 1E B2 FE " +
            "9D 9A D0 02 0B 47 8B 99",
            // Round #5
            // After Theta
            "8D FB 14 5E A2 8F C8 C5 8B B6 B1 E6 45 41 FD 36 " +
            "CD 8C 29 58 07 F7 0D 70 1D 60 15 46 EA 1A E6 DF " +
            "45 6D 33 94 F1 DA 64 23 68 FB CF 11 36 68 08 A7 " +
            "68 3E F3 7B 71 45 8E 4E E2 D0 DC 07 FF 31 EA 63 " +
            "5C 05 EF 8F DE EE 05 93 F1 09 3F D3 F0 AE 25 9E " +
            "78 81 22 60 BD 38 04 48 6F 25 B2 EF 44 36 B1 49 " +
            "E4 CE 97 04 2A ED F1 7B 56 80 4A EC C7 98 64 E1 " +
            "F9 62 14 30 5D 9A 9D 3D 82 91 0C 20 11 DB 11 90 " +
            "23 FE E3 98 F0 4B 08 F5 E6 6C 99 40 ED 14 E4 49 " +
            "8B 73 69 5C 18 17 64 C5 B2 D7 A7 5E C6 E5 BF 6C " +
            "03 1B CE 6A 06 1B 43 7B D9 1D 8E 6E B2 DF EA 47 " +
            "C0 D3 D7 87 A0 EE E0 10 BC 43 90 34 60 5F EC 04 " +
            "88 7A BE 58 43 8A DD 57",
            // After Rho  
            "8D FB 14 5E A2 8F C8 C5 16 6D 63 CD 8B 82 FA 6D " +
            "33 63 0A D6 C1 7D 03 5C AE 61 FE DD 01 56 61 A4 " +
            "D7 26 1B 29 6A 9B A1 8C 61 83 86 70 8A B6 FF 1C " +
            "BF 17 57 E4 E8 84 E6 33 98 38 34 F7 C1 7F 8C FA " +
            "82 F7 47 6F F7 82 49 AE 5A E2 19 9F F0 33 0D EF " +
            "C2 0B 14 01 EB C5 21 40 26 BD 95 C8 BE 13 D9 C4 " +
            "24 50 69 8F DF 23 77 BE 31 C9 C2 AD 00 95 D8 8F " +
            "98 2E CD CE 9E 7C 31 0A 40 22 B6 23 20 05 23 19 " +
            "1C 13 7E 09 A1 7E C4 7F F2 24 73 B6 4C A0 76 0A " +
            "82 AC 78 71 2E 8D 0B E3 6C B2 D7 A7 5E C6 E5 BF " +
            "0C ED 0D 6C 38 AB 19 6C 65 77 38 BA C9 7E AB 1F " +
            "78 FA FA 10 D4 1D 1C 02 43 90 34 60 5F EC 04 BC " +
            "F7 15 A2 9E 2F D6 90 62",
            // After Pi
            "8D FB 14 5E A2 8F C8 C5 BF 17 57 E4 E8 84 E6 33 " +
            "24 50 69 8F DF 23 77 BE 82 AC 78 71 2E 8D 0B E3 " +
            "F7 15 A2 9E 2F D6 90 62 AE 61 FE DD 01 56 61 A4 " +
            "5A E2 19 9F F0 33 0D EF C2 0B 14 01 EB C5 21 40 " +
            "1C 13 7E 09 A1 7E C4 7F 78 FA FA 10 D4 1D 1C 02 " +
            "16 6D 63 CD 8B 82 FA 6D 98 38 34 F7 C1 7F 8C FA " +
            "31 C9 C2 AD 00 95 D8 8F 6C B2 D7 A7 5E C6 E5 BF " +
            "0C ED 0D 6C 38 AB 19 6C D7 26 1B 29 6A 9B A1 8C " +
            "61 83 86 70 8A B6 FF 1C 26 BD 95 C8 BE 13 D9 C4 " +
            "F2 24 73 B6 4C A0 76 0A 43 90 34 60 5F EC 04 BC " +
            "33 63 0A D6 C1 7D 03 5C 82 F7 47 6F F7 82 49 AE " +
            "98 2E CD CE 9E 7C 31 0A 40 22 B6 23 20 05 23 19 " +
            "65 77 38 BA C9 7E AB 1F",
            // After Chi  
            "8D BB 3C 55 B5 AC D9 49 3D BB 47 94 C8 08 EE 72 " +
            "51 41 EB 01 DE 71 E7 BE 8A 46 6C 31 AE 84 43 66 " +
            "C5 11 E1 3E 67 D6 B6 50 2E 68 FA DD 0A 92 41 A4 " +
            "46 F2 73 97 F0 09 C9 D0 A2 E3 94 11 BF C4 39 40 " +
            "9A 12 7A C4 A0 3C A5 DB 28 78 FB 12 24 3C 10 49 " +
            "37 AC A1 C5 8B 02 AA 68 D4 0A 21 F5 9F 3D A9 CA " +
            "31 84 CA E5 20 BC C0 CF 7E B2 B5 26 DD C6 07 BE " +
            "84 FD 19 5E 78 D6 1D FE D1 1A 0A A1 5E 9A A1 4C " +
            "B1 83 E4 46 CA 16 D9 16 27 2D 91 88 AD 5F D9 70 " +
            "66 02 78 BF 6C B3 D7 0A 63 11 B0 30 DF C8 5A AC " +
            "2B 6B 82 56 C9 01 33 5C C2 F7 75 4E D7 83 4B BF " +
            "BD 7B C5 56 57 06 B9 0C 52 22 B4 67 20 04 23 59 " +
            "E5 E3 7D 93 FF FC E3 BD",
            // After Iota 
            "8C BB 3C D5 B5 AC D9 49 3D BB 47 94 C8 08 EE 72 " +
            "51 41 EB 01 DE 71 E7 BE 8A 46 6C 31 AE 84 43 66 " +
            "C5 11 E1 3E 67 D6 B6 50 2E 68 FA DD 0A 92 41 A4 " +
            "46 F2 73 97 F0 09 C9 D0 A2 E3 94 11 BF C4 39 40 " +
            "9A 12 7A C4 A0 3C A5 DB 28 78 FB 12 24 3C 10 49 " +
            "37 AC A1 C5 8B 02 AA 68 D4 0A 21 F5 9F 3D A9 CA " +
            "31 84 CA E5 20 BC C0 CF 7E B2 B5 26 DD C6 07 BE " +
            "84 FD 19 5E 78 D6 1D FE D1 1A 0A A1 5E 9A A1 4C " +
            "B1 83 E4 46 CA 16 D9 16 27 2D 91 88 AD 5F D9 70 " +
            "66 02 78 BF 6C B3 D7 0A 63 11 B0 30 DF C8 5A AC " +
            "2B 6B 82 56 C9 01 33 5C C2 F7 75 4E D7 83 4B BF " +
            "BD 7B C5 56 57 06 B9 0C 52 22 B4 67 20 04 23 59 " +
            "E5 E3 7D 93 FF FC E3 BD",
            // Round #6
            // After Theta
            "DA B2 FA F9 DB F7 E2 3D E2 55 6A F9 1D 0E B2 7D " +
            "39 FA B0 E9 5A 4B D0 DF 0D FB 11 B9 22 C4 39 C7 " +
            "40 CB 50 40 BE 50 E2 2B 78 61 3C F1 64 C9 7A D0 " +
            "99 1C 5E FA 25 0F 95 DF CA 58 CF F9 3B FE 0E 21 " +
            "1D AF 07 4C 2C 7C DF 7A AD A2 4A 6C FD BA 44 32 " +
            "61 A5 67 E9 E5 59 91 1C 0B E4 0C 98 4A 3B F5 C5 " +
            "59 3F 91 0D A4 86 F7 AE F9 0F C8 AE 51 86 7D 1F " +
            "01 27 A8 20 A1 50 49 85 87 13 CC 8D 30 C1 9A 38 " +
            "6E 6D C9 2B 1F 10 85 19 4F 96 CA 60 29 65 EE 11 " +
            "E1 BF 05 37 E0 F3 AD AB E6 CB 01 4E 06 4E 0E D7 " +
            "7D 62 44 7A A7 5A 08 28 1D 19 58 23 02 85 17 B0 " +
            "D5 C0 9E BE D3 3C 8E 6D D5 9F C9 EF AC 44 59 F8 " +
            "60 39 CC ED 26 7A B7 C6",
            // After Rho  
            "DA B2 FA F9 DB F7 E2 3D C4 AB D4 F2 3B 1C 64 FB " +
            "8E 3E 6C BA D6 12 F4 77 42 9C 73 DC B0 1F 91 2B " +
            "85 12 5F 01 5A 86 02 F2 4F 96 AC 07 8D 17 C6 13 " +
            "A5 5F F2 50 F9 9D C9 E1 88 32 D6 73 FE 8E BF 43 " +
            "D7 03 26 16 BE 6F BD 8E 4B 24 D3 2A AA C4 D6 AF " +
            "08 2B 3D 4B 2F CF 8A E4 17 2F 90 33 60 2A ED D4 " +
            "6C 20 35 BC 77 CD FA 89 0C FB 3E F2 1F 90 5D A3 " +
            "90 50 A8 A4 C2 80 13 54 1B 61 82 35 71 0E 27 98 " +
            "79 E5 03 A2 30 C3 AD 2D F7 88 27 4B 65 B0 94 32 " +
            "BE 75 35 FC B7 E0 06 7C D7 E6 CB 01 4E 06 4E 0E " +
            "21 A0 F4 89 11 E9 9D 6A 76 64 60 8D 08 14 5E C0 " +
            "1A D8 D3 77 9A C7 B1 AD 9F C9 EF AC 44 59 F8 D5 " +
            "AD 31 58 0E 73 BB 89 DE",
            // After Pi
            "DA B2 FA F9 DB F7 E2 3D A5 5F F2 50 F9 9D C9 E1 " +
            "6C 20 35 BC 77 CD FA 89 BE 75 35 FC B7 E0 06 7C " +
            "AD 31 58 0E 73 BB 89 DE 42 9C 73 DC B0 1F 91 2B " +
            "4B 24 D3 2A AA C4 D6 AF 08 2B 3D 4B 2F CF 8A E4 " +
            "79 E5 03 A2 30 C3 AD 2D 1A D8 D3 77 9A C7 B1 AD " +
            "C4 AB D4 F2 3B 1C 64 FB 88 32 D6 73 FE 8E BF 43 " +
            "0C FB 3E F2 1F 90 5D A3 D7 E6 CB 01 4E 06 4E 0E " +
            "21 A0 F4 89 11 E9 9D 6A 85 12 5F 01 5A 86 02 F2 " +
            "4F 96 AC 07 8D 17 C6 13 17 2F 90 33 60 2A ED D4 " +
            "F7 88 27 4B 65 B0 94 32 9F C9 EF AC 44 59 F8 D5 " +
            "8E 3E 6C BA D6 12 F4 77 D7 03 26 16 BE 6F BD 8E " +
            "90 50 A8 A4 C2 80 13 54 1B 61 82 35 71 0E 27 98 " +
            "76 64 60 8D 08 14 5E C0",
            // After Chi  
            "92 92 FF 55 DD B7 D0 35 37 0A F2 10 79 BD CD 95 " +
            "6D 20 7D BE 37 D6 73 0B EC F7 97 0D 3F A4 64 5D " +
            "88 7C 58 0E 53 B3 80 1E 42 97 5F 9D B5 14 99 6B " +
            "3A E0 D1 8A BA C4 F3 A6 0A 33 ED 1E A5 CB 9A 64 " +
            "39 E1 23 2A 10 DB AD 2F 13 F8 53 55 90 07 F7 29 " +
            "C0 62 FC 72 3A 0C 24 5B 5B 36 17 72 BE 88 BD 4F " +
            "2C FB 0A 7A 0E 79 CC C3 13 ED CB 73 64 12 2E 9F " +
            "29 B0 F6 88 D5 6B 06 6A 95 3B 4F 31 3A AE 2B 36 " +
            "AF 16 8B 4F 88 87 D6 31 1F 6E 58 97 60 63 85 11 " +
            "F7 9A 37 4A 7F 36 96 10 D5 4D 4F AA C1 48 3C D4 " +
            "8E 6E E4 1A 96 92 F6 27 DC 22 24 07 8F 61 99 06 " +
            "F4 54 C8 2C CA 90 4B 14 93 7B 8E 07 A7 0C 87 AF " +
            "27 65 62 89 20 79 57 48",
            // After Iota 
            "13 12 FF D5 DD B7 D0 B5 37 0A F2 10 79 BD CD 95 " +
            "6D 20 7D BE 37 D6 73 0B EC F7 97 0D 3F A4 64 5D " +
            "88 7C 58 0E 53 B3 80 1E 42 97 5F 9D B5 14 99 6B " +
            "3A E0 D1 8A BA C4 F3 A6 0A 33 ED 1E A5 CB 9A 64 " +
            "39 E1 23 2A 10 DB AD 2F 13 F8 53 55 90 07 F7 29 " +
            "C0 62 FC 72 3A 0C 24 5B 5B 36 17 72 BE 88 BD 4F " +
            "2C FB 0A 7A 0E 79 CC C3 13 ED CB 73 64 12 2E 9F " +
            "29 B0 F6 88 D5 6B 06 6A 95 3B 4F 31 3A AE 2B 36 " +
            "AF 16 8B 4F 88 87 D6 31 1F 6E 58 97 60 63 85 11 " +
            "F7 9A 37 4A 7F 36 96 10 D5 4D 4F AA C1 48 3C D4 " +
            "8E 6E E4 1A 96 92 F6 27 DC 22 24 07 8F 61 99 06 " +
            "F4 54 C8 2C CA 90 4B 14 93 7B 8E 07 A7 0C 87 AF " +
            "27 65 62 89 20 79 57 48",
            // Round #7
            // After Theta
            "19 DE 18 64 DF 77 52 E3 FC 1D 10 C3 EB 00 AA 52 " +
            "0C FD 6A 2D 6B 6E 53 E5 CD 1D 3D 8D E6 EE BA 76 " +
            "3F 03 71 34 3C C3 17 65 48 5B B8 2C B7 D4 1B 3D " +
            "F1 F7 33 59 28 79 94 61 6B EE FA 8D F9 73 BA 8A " +
            "18 0B 89 AA C9 91 73 04 A4 87 7A 6F FF 77 60 52 " +
            "CA AE 1B C3 38 CC A6 0D 90 21 F5 A1 2C 35 DA 88 " +
            "4D 26 1D E9 52 C1 EC 2D 32 07 61 F3 BD 58 F0 B4 " +
            "9E CF DF B2 BA 1B 91 11 9F F7 A8 80 38 6E A9 60 " +
            "64 01 69 9C 1A 3A B1 F6 7E B3 4F 04 3C DB A5 FF " +
            "D6 70 9D CA A6 7C 48 3B 62 32 66 90 AE 38 AB AF " +
            "84 A2 03 AB 94 52 74 71 17 35 C6 D4 1D DC FE C1 " +
            "95 89 DF BF 96 28 6B FA B2 91 24 87 7E 46 59 84 " +
            "90 1A 4B B3 4F 09 C0 33",
            // After Rho  
            "19 DE 18 64 DF 77 52 E3 F8 3B 20 86 D7 01 54 A5 " +
            "43 BF 5A CB 9A DB 54 39 EE AE 6B D7 DC D1 D3 68 " +
            "19 BE 28 FB 19 88 A3 E1 72 4B BD D1 83 B4 85 CB " +
            "93 85 92 47 19 16 7F 3F E2 9A BB 7E 63 FE 9C AE " +
            "85 44 D5 E4 C8 39 02 8C 07 26 45 7A A8 F7 F6 7F " +
            "50 76 DD 18 C6 61 36 6D 23 42 86 D4 87 B2 D4 68 " +
            "48 97 0A 66 6F 69 32 E9 B1 E0 69 65 0E C2 E6 7B " +
            "59 DD 8D C8 08 CF E7 6F 01 71 DC 52 C1 3E EF 51 " +
            "8D 53 43 27 D6 9E 2C 20 D2 7F BF D9 27 02 9E ED " +
            "0F 69 C7 1A AE 53 D9 94 AF 62 32 66 90 AE 38 AB " +
            "D1 C5 11 8A 0E AC 52 4A 5F D4 18 53 77 70 FB 07 " +
            "32 F1 FB D7 12 65 4D BF 91 24 87 7E 46 59 84 B2 " +
            "F0 0C A4 C6 D2 EC 53 02",
            // After Pi
            "19 DE 18 64 DF 77 52 E3 93 85 92 47 19 16 7F 3F " +
            "48 97 0A 66 6F 69 32 E9 0F 69 C7 1A AE 53 D9 94 " +
            "F0 0C A4 C6 D2 EC 53 02 EE AE 6B D7 DC D1 D3 68 " +
            "07 26 45 7A A8 F7 F6 7F 50 76 DD 18 C6 61 36 6D " +
            "8D 53 43 27 D6 9E 2C 20 32 F1 FB D7 12 65 4D BF " +
            "F8 3B 20 86 D7 01 54 A5 E2 9A BB 7E 63 FE 9C AE " +
            "B1 E0 69 65 0E C2 E6 7B AF 62 32 66 90 AE 38 AB " +
            "D1 C5 11 8A 0E AC 52 4A 19 BE 28 FB 19 88 A3 E1 " +
            "72 4B BD D1 83 B4 85 CB 23 42 86 D4 87 B2 D4 68 " +
            "D2 7F BF D9 27 02 9E ED 91 24 87 7E 46 59 84 B2 " +
            "43 BF 5A CB 9A DB 54 39 85 44 D5 E4 C8 39 02 8C " +
            "59 DD 8D C8 08 CF E7 6F 01 71 DC 52 C1 3E EF 51 " +
            "5F D4 18 53 77 70 FB 07",
            // After Chi  
            "51 CC 10 44 B9 1E 52 23 94 ED 57 5F 99 04 B6 2B " +
            "B8 93 2A A2 3F C5 30 EB 06 BB DF 3A A3 40 D9 75 " +
            "72 0D 26 C5 D2 EC 7E 1E BE FE F3 D7 9A D1 D3 68 " +
            "8A 27 47 5D B8 69 FE 7F 62 D6 65 C8 C6 00 77 F2 " +
            "41 5D 43 27 1A 0E BE 60 33 F1 FF FF 32 43 69 A8 " +
            "E9 5B 60 87 DB 01 36 F4 EC 98 A9 7C F3 D2 84 2E " +
            "E1 65 68 ED 00 C2 A4 3B 87 58 12 62 41 AF 3C 0E " +
            "D3 45 8A F2 2E 52 DA 40 18 BE 2A FF 1D 8A F3 C1 " +
            "A2 76 84 D8 A3 B4 8F 4E 22 42 86 F2 C7 EB D4 7A " +
            "DA E5 97 58 3E 82 BD AC F3 65 12 7E C4 6D 80 B8 " +
            "1B 26 52 C3 9A 1D B1 5A 85 64 85 F6 09 09 0A 9C " +
            "07 59 8D C9 3E 8F F7 69 01 5A 9E DA 49 B5 EB 69 " +
            "DB 94 9D 77 37 50 F9 83",
            // After Iota 
            "58 4C 10 44 B9 1E 52 A3 94 ED 57 5F 99 04 B6 2B " +
            "B8 93 2A A2 3F C5 30 EB 06 BB DF 3A A3 40 D9 75 " +
            "72 0D 26 C5 D2 EC 7E 1E BE FE F3 D7 9A D1 D3 68 " +
            "8A 27 47 5D B8 69 FE 7F 62 D6 65 C8 C6 00 77 F2 " +
            "41 5D 43 27 1A 0E BE 60 33 F1 FF FF 32 43 69 A8 " +
            "E9 5B 60 87 DB 01 36 F4 EC 98 A9 7C F3 D2 84 2E " +
            "E1 65 68 ED 00 C2 A4 3B 87 58 12 62 41 AF 3C 0E " +
            "D3 45 8A F2 2E 52 DA 40 18 BE 2A FF 1D 8A F3 C1 " +
            "A2 76 84 D8 A3 B4 8F 4E 22 42 86 F2 C7 EB D4 7A " +
            "DA E5 97 58 3E 82 BD AC F3 65 12 7E C4 6D 80 B8 " +
            "1B 26 52 C3 9A 1D B1 5A 85 64 85 F6 09 09 0A 9C " +
            "07 59 8D C9 3E 8F F7 69 01 5A 9E DA 49 B5 EB 69 " +
            "DB 94 9D 77 37 50 F9 83",
            // Round #8
            // After Theta
            "49 85 BC 24 74 DA 74 3E A4 EA F4 0F E7 9B C3 EC " +
            "5A D1 9C 09 58 6A 62 FF 6D 11 4B 05 D8 A3 70 DF " +
            "70 EE 57 69 A3 88 99 89 AF 37 5F B7 57 15 F5 F5 " +
            "BA 20 E4 0D C6 F6 8B B8 80 94 D3 63 A1 AF 25 E6 " +
            "2A F7 D7 18 61 ED 17 CA 31 12 8E 53 43 27 8E 3F " +
            "F8 92 CC E7 16 C5 10 69 DC 9F 0A 2C 8D 4D F1 E9 " +
            "03 27 DE 46 67 6D F6 2F EC F2 86 5D 3A 4C 95 A4 " +
            "D1 A6 FB 5E 5F 36 3D D7 09 77 86 9F D0 4E D5 5C " +
            "92 71 27 88 DD 2B FA 89 C0 00 30 59 A0 44 86 6E " +
            "B1 4F 03 67 45 61 14 06 F1 86 63 D2 B5 09 67 2F " +
            "0A EF FE A3 57 D9 97 C7 B5 63 26 A6 77 96 7F 5B " +
            "E5 1B 3B 62 59 20 A5 7D 6A F0 0A E5 32 56 42 C3 " +
            "D9 77 EC DB 46 34 1E 14",
            // After Rho  
            "49 85 BC 24 74 DA 74 3E 49 D5 E9 1F CE 37 87 D9 " +
            "56 34 67 02 96 9A D8 BF 3D 0A F7 DD 16 B1 54 80 " +
            "45 CC 4C 84 73 BF 4A 1B 7B 55 51 5F FF 7A F3 75 " +
            "DE 60 6C BF 88 AB 0B 42 39 20 E5 F4 58 E8 6B 89 " +
            "FB 6B 8C B0 F6 0B 65 95 E2 F8 13 23 E1 38 35 74 " +
            "C3 97 64 3E B7 28 86 48 A7 73 7F 2A B0 34 36 C5 " +
            "36 3A 6B B3 7F 19 38 F1 98 2A 49 D9 E5 0D BB 74 " +
            "AF 2F 9B 9E EB 68 D3 7D 3F A1 9D AA B9 12 EE 0C " +
            "04 B1 7B 45 3F 51 32 EE 43 37 60 00 98 2C 50 22 " +
            "8C C2 20 F6 69 E0 AC 28 2F F1 86 63 D2 B5 09 67 " +
            "5F 1E 2B BC FB 8F 5E 65 D5 8E 99 98 DE 59 FE 6D " +
            "7C 63 47 2C 0B A4 B4 AF F0 0A E5 32 56 42 C3 6A " +
            "07 45 F6 1D FB B6 11 8D",
            // After Pi
            "49 85 BC 24 74 DA 74 3E DE 60 6C BF 88 AB 0B 42 " +
            "36 3A 6B B3 7F 19 38 F1 8C C2 20 F6 69 E0 AC 28 " +
            "07 45 F6 1D FB B6 11 8D 3D 0A F7 DD 16 B1 54 80 " +
            "E2 F8 13 23 E1 38 35 74 C3 97 64 3E B7 28 86 48 " +
            "04 B1 7B 45 3F 51 32 EE 7C 63 47 2C 0B A4 B4 AF " +
            "49 D5 E9 1F CE 37 87 D9 39 20 E5 F4 58 E8 6B 89 " +
            "98 2A 49 D9 E5 0D BB 74 2F F1 86 63 D2 B5 09 67 " +
            "5F 1E 2B BC FB 8F 5E 65 45 CC 4C 84 73 BF 4A 1B " +
            "7B 55 51 5F FF 7A F3 75 A7 73 7F 2A B0 34 36 C5 " +
            "43 37 60 00 98 2C 50 22 F0 0A E5 32 56 42 C3 6A " +
            "56 34 67 02 96 9A D8 BF FB 6B 8C B0 F6 0B 65 95 " +
            "AF 2F 9B 9E EB 68 D3 7D 3F A1 9D AA B9 12 EE 0C " +
            "D5 8E 99 98 DE 59 FE 6D",
            // After Chi  
            "69 9F BF 24 03 CA 44 8F 56 A0 6C FB 88 4B 8F 4A " +
            "35 3F BD BA ED 0F 29 74 C4 42 28 D6 6D A8 C8 1A " +
            "91 25 B6 86 73 97 1A CD 3C 0D 93 C1 00 B1 D6 88 " +
            "E6 D8 08 62 E9 69 05 D2 BB D5 60 16 B7 8C 02 49 " +
            "05 B9 CB 94 2B 40 72 EE BE 93 47 0E EA AC 95 DB " +
            "C9 DF E1 16 6B 32 17 AD 1E F1 63 D6 4A 58 6B 8A " +
            "C8 24 60 45 CC 07 ED 74 2F 30 46 60 D6 85 88 FF " +
            "6F 3E 2F 5C EB 47 36 65 C1 EE 62 A4 73 BB 4E 9B " +
            "3B 51 51 5F F7 72 B3 57 17 7B FA 18 F6 76 B5 8D " +
            "46 F3 68 84 B9 91 58 33 CA 1B F4 69 DA 02 72 0E " +
            "52 30 74 0C 9F FA 4A D7 EB EB 88 90 E6 19 49 95 " +
            "6F 21 9B 8E AD 21 C3 1C 3D 91 FB A8 B9 90 EE 9E " +
            "7C C5 11 28 BE 58 DB 6D",
            // After Iota 
            "E3 9F BF 24 03 CA 44 8F 56 A0 6C FB 88 4B 8F 4A " +
            "35 3F BD BA ED 0F 29 74 C4 42 28 D6 6D A8 C8 1A " +
            "91 25 B6 86 73 97 1A CD 3C 0D 93 C1 00 B1 D6 88 " +
            "E6 D8 08 62 E9 69 05 D2 BB D5 60 16 B7 8C 02 49 " +
            "05 B9 CB 94 2B 40 72 EE BE 93 47 0E EA AC 95 DB " +
            "C9 DF E1 16 6B 32 17 AD 1E F1 63 D6 4A 58 6B 8A " +
            "C8 24 60 45 CC 07 ED 74 2F 30 46 60 D6 85 88 FF " +
            "6F 3E 2F 5C EB 47 36 65 C1 EE 62 A4 73 BB 4E 9B " +
            "3B 51 51 5F F7 72 B3 57 17 7B FA 18 F6 76 B5 8D " +
            "46 F3 68 84 B9 91 58 33 CA 1B F4 69 DA 02 72 0E " +
            "52 30 74 0C 9F FA 4A D7 EB EB 88 90 E6 19 49 95 " +
            "6F 21 9B 8E AD 21 C3 1C 3D 91 FB A8 B9 90 EE 9E " +
            "7C C5 11 28 BE 58 DB 6D",
            // Round #9
            // After Theta
            "E8 AF 38 B0 60 CE 62 3F AE 1B 0E 5F 96 E4 6F 1D " +
            "60 5F 0E 26 F7 C7 3A E9 16 7B 82 83 8D 37 58 E2 " +
            "0F AB 37 3F EB EA 9C A6 37 3D 14 55 63 B5 F0 38 " +
            "1E 63 6A C6 F7 C6 E5 85 EE B5 D3 8A AD 44 11 D4 " +
            "D7 80 61 C1 CB DF E2 16 20 1D C6 B7 72 D1 13 B0 " +
            "C2 EF 66 82 08 36 31 1D E6 4A 01 72 54 F7 8B DD " +
            "9D 44 D3 D9 D6 CF FE E9 FD 09 EC 35 36 1A 18 07 " +
            "F1 B0 AE E5 73 3A B0 0E CA DE E5 30 10 BF 68 2B " +
            "C3 EA 33 FB E9 DD 53 00 42 1B 49 84 EC BE A6 10 " +
            "94 CA C2 D1 59 0E C8 CB 54 95 75 D0 42 7F F4 65 " +
            "59 00 F3 98 FC FE 6C 67 13 50 EA 34 F8 B6 A9 C2 " +
            "3A 41 28 12 B7 E9 D0 81 EF A8 51 FD 59 0F 7E 66 " +
            "E2 4B 90 91 26 25 5D 06",
            // After Rho  
            "E8 AF 38 B0 60 CE 62 3F 5C 37 1C BE 2C C9 DF 3A " +
            "D8 97 83 C9 FD B1 4E 3A 78 83 25 6E B1 27 38 D8 " +
            "57 E7 34 7D 58 BD F9 59 35 56 0B 8F 73 D3 43 51 " +
            "66 7C 6F 5C 5E E8 31 A6 B5 7B ED B4 62 2B 51 04 " +
            "C0 B0 E0 E5 6F 71 8B 6B 3D 01 0B D2 61 7C 2B 17 " +
            "10 7E 37 13 44 B0 89 E9 76 9B 2B 05 C8 51 DD 2F " +
            "CE B6 7E F6 4F EF 24 9A 34 30 0E FA 13 D8 6B 6C " +
            "F2 39 1D 58 87 78 58 D7 61 20 7E D1 56 94 BD CB " +
            "66 3F BD 7B 0A 60 58 7D 53 08 A1 8D 24 42 76 5F " +
            "01 79 99 52 59 38 3A CB 65 54 95 75 D0 42 7F F4 " +
            "B3 9D 65 01 CC 63 F2 FB 4F 40 A9 D3 E0 DB A6 0A " +
            "27 08 45 E2 36 1D 3A 50 A8 51 FD 59 0F 7E 66 EF " +
            "97 81 F8 12 64 A4 49 49",
            // After Pi
            "E8 AF 38 B0 60 CE 62 3F 66 7C 6F 5C 5E E8 31 A6 " +
            "CE B6 7E F6 4F EF 24 9A 01 79 99 52 59 38 3A CB " +
            "97 81 F8 12 64 A4 49 49 78 83 25 6E B1 27 38 D8 " +
            "3D 01 0B D2 61 7C 2B 17 10 7E 37 13 44 B0 89 E9 " +
            "66 3F BD 7B 0A 60 58 7D 27 08 45 E2 36 1D 3A 50 " +
            "5C 37 1C BE 2C C9 DF 3A B5 7B ED B4 62 2B 51 04 " +
            "34 30 0E FA 13 D8 6B 6C 65 54 95 75 D0 42 7F F4 " +
            "B3 9D 65 01 CC 63 F2 FB 57 E7 34 7D 58 BD F9 59 " +
            "35 56 0B 8F 73 D3 43 51 76 9B 2B 05 C8 51 DD 2F " +
            "53 08 A1 8D 24 42 76 5F A8 51 FD 59 0F 7E 66 EF " +
            "D8 97 83 C9 FD B1 4E 3A C0 B0 E0 E5 6F 71 8B 6B " +
            "F2 39 1D 58 87 78 58 D7 61 20 7E D1 56 94 BD CB " +
            "4F 40 A9 D3 E0 DB A6 0A",
            // After Chi  
            "60 2D 28 12 61 C9 66 27 67 35 EE 5C 4E F8 2B E7 " +
            "58 36 1E F6 6B 6B 65 9A 69 57 99 F2 59 72 18 FD " +
            "91 D1 BF 5E 7A 84 58 C9 78 FD 11 6F B5 A7 B8 30 " +
            "5B 00 83 BA 6B 3C 7B 03 11 7E 77 93 70 AD AB E9 " +
            "3E BC 9D 77 8B 42 58 F5 22 08 4F 72 76 45 39 57 " +
            "5C 37 1E F4 3D 19 F5 52 F4 3F 7C B1 A2 29 45 94 " +
            "A6 B9 6E FA 1F F9 EB 67 29 76 8D CB F0 CA 72 F4 " +
            "12 D5 84 01 8E 41 F2 FF 15 6E 14 7D D0 BD 65 77 " +
            "34 56 8B 07 57 D1 61 01 DE CA 77 55 C3 6D DD 8F " +
            "04 AE A1 A9 74 C3 EF 4F 88 41 F6 DB 2C 3C 64 EF " +
            "EA 9E 9E D1 7D B9 1E AE C1 B0 82 64 3F F5 2E 63 " +
            "FC 79 9C 5A 27 33 5A D7 F1 B7 7C D9 4B B4 F5 FB " +
            "4F 60 C9 F7 E2 9B 27 4B",
            // After Iota 
            "E8 2D 28 12 61 C9 66 27 67 35 EE 5C 4E F8 2B E7 " +
            "58 36 1E F6 6B 6B 65 9A 69 57 99 F2 59 72 18 FD " +
            "91 D1 BF 5E 7A 84 58 C9 78 FD 11 6F B5 A7 B8 30 " +
            "5B 00 83 BA 6B 3C 7B 03 11 7E 77 93 70 AD AB E9 " +
            "3E BC 9D 77 8B 42 58 F5 22 08 4F 72 76 45 39 57 " +
            "5C 37 1E F4 3D 19 F5 52 F4 3F 7C B1 A2 29 45 94 " +
            "A6 B9 6E FA 1F F9 EB 67 29 76 8D CB F0 CA 72 F4 " +
            "12 D5 84 01 8E 41 F2 FF 15 6E 14 7D D0 BD 65 77 " +
            "34 56 8B 07 57 D1 61 01 DE CA 77 55 C3 6D DD 8F " +
            "04 AE A1 A9 74 C3 EF 4F 88 41 F6 DB 2C 3C 64 EF " +
            "EA 9E 9E D1 7D B9 1E AE C1 B0 82 64 3F F5 2E 63 " +
            "FC 79 9C 5A 27 33 5A D7 F1 B7 7C D9 4B B4 F5 FB " +
            "4F 60 C9 F7 E2 9B 27 4B",
            // Round #10  
            // After Theta
            "F4 D8 52 7B F3 7D 03 C6 CE A7 9B 58 CB 48 3F E2 " +
            "73 D3 AF BE BE B8 6E 18 69 4F E3 60 21 5D 1A 3A " +
            "7D 7B B1 2B EF EF D0 B9 64 08 6B 06 27 13 DD D1 " +
            "F2 92 F6 BE EE 8C 6F 06 3A 9B C6 DB A5 7E A0 6B " +
            "3E A4 E7 E5 F3 6D 5A 32 CE A2 41 07 E3 2E B1 27 " +
            "40 C2 64 9D AF AD 90 B3 5D AD 09 B5 27 99 51 91 " +
            "8D 5C DF B2 CA 2A E0 E5 29 6E F7 59 88 E5 70 33 " +
            "FE 7F 8A 74 1B 2A 7A 8F 09 9B 6E 14 42 09 00 96 " +
            "9D C4 FE 03 D2 61 75 04 F5 2F C6 1D 16 BE D6 0D " +
            "04 B6 DB 3B 0C EC ED 88 64 EB F8 AE B9 57 EC 9F " +
            "F6 6B E4 B8 EF 0D 7B 4F 68 22 F7 60 BA 45 3A 66 " +
            "D7 9C 2D 12 F2 E0 51 55 F1 AF 06 4B 33 9B F7 3C " +
            "A3 CA C7 82 77 F0 AF 3B",
            // After Rho  
            "F4 D8 52 7B F3 7D 03 C6 9D 4F 37 B1 96 91 7E C4 " +
            "DC F4 AB AF 2F AE 1B C6 D2 A5 A1 93 F6 34 0E 16 " +
            "7F 87 CE ED DB 8B 5D 79 70 32 D1 1D 4D 86 B0 66 " +
            "EF EB CE F8 66 20 2F 69 9A CE A6 F1 76 A9 1F E8 " +
            "D2 F3 F2 F9 36 2D 19 1F 12 7B E2 2C 1A 74 30 EE " +
            "05 12 26 EB 7C 6D 85 9C 45 76 B5 26 D4 9E 64 46 " +
            "96 55 56 01 2F 6F E4 FA CB E1 66 52 DC EE B3 10 " +
            "BA 0D 15 BD 47 FF 3F 45 28 84 12 00 2C 13 36 DD " +
            "7F 40 3A AC 8E A0 93 D8 EB 86 FA 17 E3 0E 0B 5F " +
            "BD 1D 91 C0 76 7B 87 81 9F 64 EB F8 AE B9 57 EC " +
            "EC 3D D9 AF 91 E3 BE 37 A1 89 DC 83 E9 16 E9 98 " +
            "9A B3 45 42 1E 3C AA EA AF 06 4B 33 9B F7 3C F1 " +
            "EB CE A8 F2 B1 E0 1D FC",
            // After Pi
            "F4 D8 52 7B F3 7D 03 C6 EF EB CE F8 66 20 2F 69 " +
            "96 55 56 01 2F 6F E4 FA BD 1D 91 C0 76 7B 87 81 " +
            "EB CE A8 F2 B1 E0 1D FC D2 A5 A1 93 F6 34 0E 16 " +
            "12 7B E2 2C 1A 74 30 EE 05 12 26 EB 7C 6D 85 9C " +
            "7F 40 3A AC 8E A0 93 D8 9A B3 45 42 1E 3C AA EA " +
            "9D 4F 37 B1 96 91 7E C4 9A CE A6 F1 76 A9 1F E8 " +
            "CB E1 66 52 DC EE B3 10 9F 64 EB F8 AE B9 57 EC " +
            "EC 3D D9 AF 91 E3 BE 37 7F 87 CE ED DB 8B 5D 79 " +
            "70 32 D1 1D 4D 86 B0 66 45 76 B5 26 D4 9E 64 46 " +
            "EB 86 FA 17 E3 0E 0B 5F AF 06 4B 33 9B F7 3C F1 " +
            "DC F4 AB AF 2F AE 1B C6 D2 F3 F2 F9 36 2D 19 1F " +
            "BA 0D 15 BD 47 FF 3F 45 28 84 12 00 2C 13 36 DD " +
            "A1 89 DC 83 E9 16 E9 98",
            // After Chi  
            "E4 CC 42 7A FA 32 C3 54 C6 E3 4F 38 36 30 2C 68 " +
            "D4 97 7E 33 AE EF FC 86 A9 0D C3 C9 34 66 85 83 " +
            "E0 ED 24 72 B5 E0 31 D5 D7 A5 A5 50 92 3D 8B 06 " +
            "68 3B FA 28 98 F4 22 AE 85 A1 63 A9 6C 71 AD BE " +
            "3F 44 9A 3D 6E A0 97 CC 9A E9 07 6E 16 7C 9A 02 " +
            "DC 6E 77 B3 1E D7 DE D4 8E CA 2F 59 54 B8 5B 04 " +
            "AB F8 76 55 CD AC 1B 03 8E 26 CD E8 A8 A9 17 2C " +
            "EE BD 59 EF F1 CB BF 1F 7A C3 EA CF 4B 93 19 79 " +
            "DA B2 9B 0C 6E 86 BB 7F 41 76 B4 06 CC 6F 50 E6 " +
            "BB 07 7E DB A3 06 4A 57 AF 36 5A 23 9F F3 9C F7 " +
            "F4 F8 AE AB 6E 7C 3D 86 D2 73 F0 F9 1E 2D 19 87 " +
            "3B 04 D9 3E 86 FB F6 45 74 F0 31 2C 2A BB 24 9B " +
            "A3 8A 8C D3 F9 17 E9 81",
            // After Iota 
            "ED 4C 42 FA FA 32 C3 54 C6 E3 4F 38 36 30 2C 68 " +
            "D4 97 7E 33 AE EF FC 86 A9 0D C3 C9 34 66 85 83 " +
            "E0 ED 24 72 B5 E0 31 D5 D7 A5 A5 50 92 3D 8B 06 " +
            "68 3B FA 28 98 F4 22 AE 85 A1 63 A9 6C 71 AD BE " +
            "3F 44 9A 3D 6E A0 97 CC 9A E9 07 6E 16 7C 9A 02 " +
            "DC 6E 77 B3 1E D7 DE D4 8E CA 2F 59 54 B8 5B 04 " +
            "AB F8 76 55 CD AC 1B 03 8E 26 CD E8 A8 A9 17 2C " +
            "EE BD 59 EF F1 CB BF 1F 7A C3 EA CF 4B 93 19 79 " +
            "DA B2 9B 0C 6E 86 BB 7F 41 76 B4 06 CC 6F 50 E6 " +
            "BB 07 7E DB A3 06 4A 57 AF 36 5A 23 9F F3 9C F7 " +
            "F4 F8 AE AB 6E 7C 3D 86 D2 73 F0 F9 1E 2D 19 87 " +
            "3B 04 D9 3E 86 FB F6 45 74 F0 31 2C 2A BB 24 9B " +
            "A3 8A 8C D3 F9 17 E9 81",
            // Round #11  
            // After Theta
            "25 EF 0D 80 DB 2E 4D 9F AF 26 96 AB EE 4B 47 20 " +
            "53 75 38 58 D3 9C DC E2 18 BA 9D 39 19 A6 AA 67 " +
            "E7 0D 56 62 68 5C 3E 89 1F 06 EA 2A B3 21 05 CD " +
            "01 FE 23 BB 40 8F 49 E6 02 43 25 C2 11 02 8D DA " +
            "8E F3 C4 CD 43 60 B8 28 9D 09 75 7E CB C0 95 5E " +
            "14 CD 38 C9 3F CB 50 1F E7 0F F6 CA 8C C3 30 4C " +
            "2C 1A 30 3E B0 DF 3B 67 3F 91 93 18 85 69 38 C8 " +
            "E9 5D 2B FF 2C 77 B0 43 B2 60 A5 B5 6A 8F 97 B2 " +
            "B3 77 42 9F B6 FD D0 37 C6 94 F2 6D B1 1C 70 82 " +
            "0A B0 20 2B 8E C6 65 B3 A8 D6 28 33 42 4F 93 AB " +
            "3C 5B E1 D1 4F 60 B3 4D BB B6 29 6A C6 56 72 CF " +
            "BC E6 9F 55 FB 88 D6 21 C5 47 6F DC 07 7B 0B 7F " +
            "A4 6A FE C3 24 AB E6 DD",
            // After Rho  
            "25 EF 0D 80 DB 2E 4D 9F 5E 4D 2C 57 DD 97 8E 40 " +
            "54 1D 0E D6 34 27 B7 F8 61 AA 7A 86 A1 DB 99 93 " +
            "E3 F2 49 3C 6F B0 12 43 32 1B 52 D0 FC 61 A0 AE " +
            "B2 0B F4 98 64 1E E0 3F B6 C0 50 89 70 84 40 A3 " +
            "79 E2 E6 21 30 5C 14 C7 5C E9 D5 99 50 E7 B7 0C " +
            "A0 68 C6 49 FE 59 86 FA 30 9D 3F D8 2B 33 0E C3 " +
            "F1 81 FD DE 39 63 D1 80 D3 70 90 7F 22 27 31 0A " +
            "7F 96 3B D8 A1 F4 AE 95 6B D5 1E 2F 65 65 C1 4A " +
            "E8 D3 B6 1F FA 66 F6 4E 38 41 63 4A F9 B6 58 0E " +
            "B8 6C 56 01 16 64 C5 D1 AB A8 D6 28 33 42 4F 93 " +
            "CD 36 F1 6C 85 47 3F 81 EF DA A6 A8 19 5B C9 3D " +
            "D7 FC B3 6A 1F D1 3A 84 47 6F DC 07 7B 0B 7F C5 " +
            "79 37 A9 9A FF 30 C9 AA",
            // After Pi
            "25 EF 0D 80 DB 2E 4D 9F B2 0B F4 98 64 1E E0 3F " +
            "F1 81 FD DE 39 63 D1 80 B8 6C 56 01 16 64 C5 D1 " +
            "79 37 A9 9A FF 30 C9 AA 61 AA 7A 86 A1 DB 99 93 " +
            "5C E9 D5 99 50 E7 B7 0C A0 68 C6 49 FE 59 86 FA " +
            "E8 D3 B6 1F FA 66 F6 4E D7 FC B3 6A 1F D1 3A 84 " +
            "5E 4D 2C 57 DD 97 8E 40 B6 C0 50 89 70 84 40 A3 " +
            "D3 70 90 7F 22 27 31 0A AB A8 D6 28 33 42 4F 93 " +
            "CD 36 F1 6C 85 47 3F 81 E3 F2 49 3C 6F B0 12 43 " +
            "32 1B 52 D0 FC 61 A0 AE 30 9D 3F D8 2B 33 0E C3 " +
            "38 41 63 4A F9 B6 58 0E 47 6F DC 07 7B 0B 7F C5 " +
            "54 1D 0E D6 34 27 B7 F8 79 E2 E6 21 30 5C 14 C7 " +
            "7F 96 3B D8 A1 F4 AE 95 6B D5 1E 2F 65 65 C1 4A " +
            "EF DA A6 A8 19 5B C9 3D",
            // After Chi  
            "64 6F 04 C6 C2 4F 5C 1F BA 67 F6 99 62 1A E4 6E " +
            "B0 92 54 44 D0 73 D9 AA BC A4 52 01 16 6A C1 C4 " +
            "EB 37 59 82 DB 20 69 8A C1 AA 78 C6 0F C3 99 61 " +
            "14 7A E5 8F 50 C1 C7 08 B7 44 C7 29 FB C8 8E 7A " +
            "C8 D1 FE 9B 5A 6C 77 5D CB BD 36 73 4F F5 1C 88 " +
            "1F 7D AC 21 DF B4 BF 48 9E 48 16 89 61 C4 0E 32 " +
            "97 66 B1 3B A6 22 01 0A B9 E1 DA 3B 6B D2 CF D3 " +
            "6D B6 A1 E4 A5 47 7F 22 E3 76 64 34 6C A2 1C 02 " +
            "3A 5B 12 D2 2C E5 F0 A2 77 B3 A3 DD 29 3A 29 02 " +
            "98 D1 62 72 FD 06 58 0C 57 66 CE C7 EB 4A DF 69 " +
            "52 09 17 0E B5 87 1D E8 79 A3 E2 06 74 5D 55 8D " +
            "FB 9C 9B 58 B9 EE A6 A0 7B D0 16 79 41 41 F7 8A " +
            "C6 38 46 89 19 03 C9 3A",
            // After Iota 
            "6E 6F 04 46 C2 4F 5C 1F BA 67 F6 99 62 1A E4 6E " +
            "B0 92 54 44 D0 73 D9 AA BC A4 52 01 16 6A C1 C4 " +
            "EB 37 59 82 DB 20 69 8A C1 AA 78 C6 0F C3 99 61 " +
            "14 7A E5 8F 50 C1 C7 08 B7 44 C7 29 FB C8 8E 7A " +
            "C8 D1 FE 9B 5A 6C 77 5D CB BD 36 73 4F F5 1C 88 " +
            "1F 7D AC 21 DF B4 BF 48 9E 48 16 89 61 C4 0E 32 " +
            "97 66 B1 3B A6 22 01 0A B9 E1 DA 3B 6B D2 CF D3 " +
            "6D B6 A1 E4 A5 47 7F 22 E3 76 64 34 6C A2 1C 02 " +
            "3A 5B 12 D2 2C E5 F0 A2 77 B3 A3 DD 29 3A 29 02 " +
            "98 D1 62 72 FD 06 58 0C 57 66 CE C7 EB 4A DF 69 " +
            "52 09 17 0E B5 87 1D E8 79 A3 E2 06 74 5D 55 8D " +
            "FB 9C 9B 58 B9 EE A6 A0 7B D0 16 79 41 41 F7 8A " +
            "C6 38 46 89 19 03 C9 3A",
            // Round #12  
            // After Theta
            "54 57 A9 8A 17 DA 51 9B 83 9E 60 A4 92 9D 2D 43 " +
            "9E 15 A4 5B EC F3 FC 48 18 FE C4 64 8D 90 21 5A " +
            "C6 2C 1C 1F D7 88 49 FE FB 92 D5 0A DA 56 94 E5 " +
            "2D 83 73 B2 A0 46 0E 25 99 C3 37 36 C7 48 AB 98 " +
            "6C 8B 68 FE C1 96 97 C3 E6 A6 73 EE 43 5D 3C FC " +
            "25 45 01 ED 0A 21 B2 CC A7 B1 80 B4 91 43 C7 1F " +
            "B9 E1 41 24 9A A2 24 E8 1D BB 4C 5E F0 28 2F 4D " +
            "40 AD E4 79 A9 EF 5F 56 D9 4E C9 F8 B9 37 11 86 " +
            "03 A2 84 EF DC 62 39 8F 59 34 53 C2 15 BA 0C E0 " +
            "3C 8B F4 17 66 FC B8 92 7A 7D 8B 5A E7 E2 FF 1D " +
            "68 31 BA C2 60 12 10 6C 40 5A 74 3B 84 DA 9C A0 " +
            "D5 1B 6B 47 85 6E 83 42 DF 8A 80 1C DA BB 17 14 " +
            "EB 23 03 14 15 AB E9 4E",
            // After Rho  
            "54 57 A9 8A 17 DA 51 9B 06 3D C1 48 25 3B 5B 86 " +
            "67 05 E9 16 FB 3C 3F 92 08 19 A2 85 E1 4F 4C D6 " +
            "46 4C F2 37 66 E1 F8 B8 A0 6D 45 59 BE 2F 59 AD " +
            "27 0B 6A E4 50 D2 32 38 66 E6 F0 8D CD 31 D2 2A " +
            "45 34 FF 60 CB CB 61 B6 C5 C3 6F 6E 3A E7 3E D4 " +
            "2E 29 0A 68 57 08 91 65 7F 9C C6 02 D2 46 0E 1D " +
            "22 D1 14 25 41 CF 0D 0F 51 5E 9A 3A 76 99 BC E0 " +
            "BC D4 F7 2F 2B A0 56 F2 F1 73 6F 22 0C B3 9D 92 " +
            "F0 9D 5B 2C E7 71 40 94 06 F0 2C 9A 29 E1 0A 5D " +
            "1F 57 92 67 91 FE C2 8C 1D 7A 7D 8B 5A E7 E2 FF " +
            "40 B0 A1 C5 E8 0A 83 49 02 69 D1 ED 10 6A 73 82 " +
            "7A 63 ED A8 D0 6D 50 A8 8A 80 1C DA BB 17 14 DF " +
            "BA D3 FA C8 00 45 C5 6A",
            // After Pi
            "54 57 A9 8A 17 DA 51 9B 27 0B 6A E4 50 D2 32 38 " +
            "22 D1 14 25 41 CF 0D 0F 1F 57 92 67 91 FE C2 8C " +
            "BA D3 FA C8 00 45 C5 6A 08 19 A2 85 E1 4F 4C D6 " +
            "C5 C3 6F 6E 3A E7 3E D4 2E 29 0A 68 57 08 91 65 " +
            "F0 9D 5B 2C E7 71 40 94 7A 63 ED A8 D0 6D 50 A8 " +
            "06 3D C1 48 25 3B 5B 86 66 E6 F0 8D CD 31 D2 2A " +
            "51 5E 9A 3A 76 99 BC E0 1D 7A 7D 8B 5A E7 E2 FF " +
            "40 B0 A1 C5 E8 0A 83 49 46 4C F2 37 66 E1 F8 B8 " +
            "A0 6D 45 59 BE 2F 59 AD 7F 9C C6 02 D2 46 0E 1D " +
            "06 F0 2C 9A 29 E1 0A 5D 8A 80 1C DA BB 17 14 DF " +
            "67 05 E9 16 FB 3C 3F 92 45 34 FF 60 CB CB 61 B6 " +
            "BC D4 F7 2F 2B A0 56 F2 F1 73 6F 22 0C B3 9D 92 " +
            "02 69 D1 ED 10 6A 73 82",
            // After Chi  
            "54 87 BD 8B 16 D7 5C 9C 3A 0D E8 A6 C0 E2 F0 B8 " +
            "82 51 7C AD 41 CE 08 6D 5B 53 93 65 86 64 D2 1D " +
            "99 DB B8 AC 40 45 E7 4A 22 31 A2 85 A4 47 CD F7 " +
            "15 57 3E 6A 9A 96 7E 44 24 4B AE E8 47 04 81 4D " +
            "F0 85 59 29 C6 73 4C C2 BF A1 A0 C2 CA CD 62 A8 " +
            "17 25 CB 7A 17 B3 77 46 6A C6 95 0C C5 57 90 35 " +
            "11 DE 1A 7E D6 91 BD E0 1B 77 3D 83 5F D6 BA 79 " +
            "20 72 91 40 20 0A 03 61 19 DC 70 35 26 A1 FE A8 " +
            "A0 0D 6D C1 97 8E 59 ED F7 9C D6 42 40 50 1A 9F " +
            "42 BC CE BF 6D 01 E2 7D 2A A1 19 92 23 19 15 DA " +
            "DF C5 E9 19 DB 1C 29 D2 04 17 F7 60 CF D8 E8 B6 " +
            "BE DC 67 E2 3B E8 34 F2 94 77 47 30 E7 A7 91 82 " +
            "02 59 C7 8D 10 A9 33 A6",
            // After Iota 
            "DF 07 BD 0B 16 D7 5C 9C 3A 0D E8 A6 C0 E2 F0 B8 " +
            "82 51 7C AD 41 CE 08 6D 5B 53 93 65 86 64 D2 1D " +
            "99 DB B8 AC 40 45 E7 4A 22 31 A2 85 A4 47 CD F7 " +
            "15 57 3E 6A 9A 96 7E 44 24 4B AE E8 47 04 81 4D " +
            "F0 85 59 29 C6 73 4C C2 BF A1 A0 C2 CA CD 62 A8 " +
            "17 25 CB 7A 17 B3 77 46 6A C6 95 0C C5 57 90 35 " +
            "11 DE 1A 7E D6 91 BD E0 1B 77 3D 83 5F D6 BA 79 " +
            "20 72 91 40 20 0A 03 61 19 DC 70 35 26 A1 FE A8 " +
            "A0 0D 6D C1 97 8E 59 ED F7 9C D6 42 40 50 1A 9F " +
            "42 BC CE BF 6D 01 E2 7D 2A A1 19 92 23 19 15 DA " +
            "DF C5 E9 19 DB 1C 29 D2 04 17 F7 60 CF D8 E8 B6 " +
            "BE DC 67 E2 3B E8 34 F2 94 77 47 30 E7 A7 91 82 " +
            "02 59 C7 8D 10 A9 33 A6",
            // Round #13  
            // After Theta
            "32 FA 59 F9 01 0E A2 46 EB 0E 56 48 CF BB F4 B5 " +
            "AF 03 59 4C AC 74 09 4D F8 37 45 9C 1F E2 88 4F " +
            "A7 A5 5C 5C 64 1E D3 BD CF CC 46 77 B3 9E 33 2D " +
            "C4 54 80 84 95 CF 7A 49 09 19 8B 09 AA BE 80 6D " +
            "53 E1 8F D0 5F F5 16 90 81 DF 44 32 EE 96 56 5F " +
            "FA D8 2F 88 00 6A 89 9C BB C5 2B E2 CA 0E 94 38 " +
            "3C 8C 3F 9F 3B 2B BC C0 B8 13 EB 7A C6 50 E0 2B " +
            "1E 0C 75 B0 04 51 37 96 F4 21 94 C7 31 78 00 72 " +
            "71 0E D3 2F 98 D7 5D E0 DA CE F3 A3 AD EA 1B BF " +
            "E1 D8 18 46 F4 87 B8 2F 14 DF FD 62 07 42 21 2D " +
            "32 38 0D EB CC C5 D7 08 D5 14 49 8E C0 81 EC BB " +
            "93 8E 42 03 D6 52 35 D2 37 13 91 C9 7E 21 CB D0 " +
            "3C 27 23 7D 34 F2 07 51",
            // After Rho  
            "32 FA 59 F9 01 0E A2 46 D7 1D AC 90 9E 77 E9 6B " +
            "EB 40 16 13 2B 5D 42 D3 21 8E F8 84 7F 53 C4 F9 " +
            "F3 98 EE 3D 2D E5 E2 22 37 EB 39 D3 F2 CC 6C 74 " +
            "48 58 F9 AC 97 44 4C 05 5B 42 C6 62 82 AA 2F 60 " +
            "F0 47 E8 AF 7A 0B C8 A9 69 F5 15 F8 4D 24 E3 6E " +
            "D4 C7 7E 41 04 50 4B E4 E2 EC 16 AF 88 2B 3B 50 " +
            "F9 DC 59 E1 05 E6 61 FC A1 C0 57 70 27 D6 F5 8C " +
            "58 82 A8 1B 4B 0F 86 3A 8F 63 F0 00 E4 E8 43 28 " +
            "FA 05 F3 BA 0B 3C CE 61 8D 5F 6D E7 F9 D1 56 F5 " +
            "10 F7 25 1C 1B C3 88 FE 2D 14 DF FD 62 07 42 21 " +
            "5F 23 C8 E0 34 AC 33 17 56 53 24 39 02 07 B2 EF " +
            "D2 51 68 C0 5A AA 46 7A 13 91 C9 7E 21 CB D0 37 " +
            "41 14 CF C9 48 1F 8D FC",
            // After Pi
            "32 FA 59 F9 01 0E A2 46 48 58 F9 AC 97 44 4C 05 " +
            "F9 DC 59 E1 05 E6 61 FC 10 F7 25 1C 1B C3 88 FE " +
            "41 14 CF C9 48 1F 8D FC 21 8E F8 84 7F 53 C4 F9 " +
            "69 F5 15 F8 4D 24 E3 6E D4 C7 7E 41 04 50 4B E4 " +
            "FA 05 F3 BA 0B 3C CE 61 D2 51 68 C0 5A AA 46 7A " +
            "D7 1D AC 90 9E 77 E9 6B 5B 42 C6 62 82 AA 2F 60 " +
            "A1 C0 57 70 27 D6 F5 8C 2D 14 DF FD 62 07 42 21 " +
            "5F 23 C8 E0 34 AC 33 17 F3 98 EE 3D 2D E5 E2 22 " +
            "37 EB 39 D3 F2 CC 6C 74 E2 EC 16 AF 88 2B 3B 50 " +
            "8D 5F 6D E7 F9 D1 56 F5 13 91 C9 7E 21 CB D0 37 " +
            "EB 40 16 13 2B 5D 42 D3 F0 47 E8 AF 7A 0B C8 A9 " +
            "58 82 A8 1B 4B 0F 86 3A 8F 63 F0 00 E4 E8 43 28 " +
            "56 53 24 39 02 07 B2 EF",
            // After Chi  
            "83 7E 59 B8 01 AC 83 BE 48 7B DD B0 8D 45 C4 07 " +
            "B8 DC 93 20 45 FA 64 FC 22 1D 35 2C 1A C3 AA FC " +
            "09 14 6F CD DE 5F C1 FD B5 8C 92 85 7F 03 CC 79 " +
            "43 F5 94 42 46 08 67 6F D4 97 76 01 54 D2 4B FE " +
            "DB 8B 63 BE 2E 6D 4E E0 9A 20 6D B8 5A 8E 65 7C " +
            "77 9D BD 80 BB 23 39 E7 57 56 4E EF C2 AB 2D 41 " +
            "F3 E3 57 70 33 7E C4 9A AD 08 FB ED E8 54 8A 49 " +
            "57 61 8A 82 34 24 35 17 33 9C E8 11 25 C6 F1 22 " +
            "3A F8 50 93 83 1C 28 D1 F0 6C 96 B7 88 21 BB 52 " +
            "6D 57 4B E6 F5 F5 74 F5 17 F2 D8 BC F3 C3 DC 63 " +
            "E3 C0 16 03 2A 59 44 C1 77 26 B8 AF DE EB 89 A9 " +
            "08 92 AC 22 49 08 36 FD 26 63 E2 02 CD B0 03 38 " +
            "46 54 CC 95 52 05 3A C7",
            // After Iota 
            "08 7E 59 B8 01 AC 83 3E 48 7B DD B0 8D 45 C4 07 " +
            "B8 DC 93 20 45 FA 64 FC 22 1D 35 2C 1A C3 AA FC " +
            "09 14 6F CD DE 5F C1 FD B5 8C 92 85 7F 03 CC 79 " +
            "43 F5 94 42 46 08 67 6F D4 97 76 01 54 D2 4B FE " +
            "DB 8B 63 BE 2E 6D 4E E0 9A 20 6D B8 5A 8E 65 7C " +
            "77 9D BD 80 BB 23 39 E7 57 56 4E EF C2 AB 2D 41 " +
            "F3 E3 57 70 33 7E C4 9A AD 08 FB ED E8 54 8A 49 " +
            "57 61 8A 82 34 24 35 17 33 9C E8 11 25 C6 F1 22 " +
            "3A F8 50 93 83 1C 28 D1 F0 6C 96 B7 88 21 BB 52 " +
            "6D 57 4B E6 F5 F5 74 F5 17 F2 D8 BC F3 C3 DC 63 " +
            "E3 C0 16 03 2A 59 44 C1 77 26 B8 AF DE EB 89 A9 " +
            "08 92 AC 22 49 08 36 FD 26 63 E2 02 CD B0 03 38 " +
            "46 54 CC 95 52 05 3A C7",
            // Round #14  
            // After Theta
            "BF 81 1B 25 B8 BD AA AE 9C E4 45 96 80 A9 CB 2A " +
            "96 8E 75 37 D8 94 78 9D 6F AC 84 55 DA DA 22 AF " +
            "22 D8 7B 09 AF C7 5E E2 02 73 D0 18 C6 12 E5 E9 " +
            "97 6A 0C 64 4B E4 68 42 FA C5 90 16 C9 BC 57 9F " +
            "96 3A D2 C7 EE 74 C6 B3 B1 EC 79 7C 2B 16 FA 63 " +
            "C0 62 FF 1D 02 32 10 77 83 C9 D6 C9 CF 47 22 6C " +
            "DD B1 B1 67 AE 10 D8 FB E0 B9 4A 94 28 4D 02 1A " +
            "7C AD 9E 46 45 BC AA 08 84 63 AA 8C 9C D7 D8 B2 " +
            "EE 67 C8 B5 8E F0 27 FC DE 3E 70 A0 15 4F A7 33 " +
            "20 E6 FA 9F 35 EC FC A6 3C 3E CC 78 82 5B 43 7C " +
            "54 3F 54 9E 93 48 6D 51 A3 B9 20 89 D3 07 86 84 " +
            "26 C0 4A 35 D4 66 2A 9C 6B D2 53 7B 0D A9 8B 6B " +
            "6D 98 D8 51 23 9D A5 D8",
            // After Rho  
            "BF 81 1B 25 B8 BD AA AE 38 C9 8B 2C 01 53 97 55 " +
            "A5 63 DD 0D 36 25 5E A7 AD 2D F2 FA C6 4A 58 A5 " +
            "3D F6 12 17 C1 DE 4B 78 61 2C 51 9E 2E 30 07 8D " +
            "40 B6 44 8E 26 74 A9 C6 A7 7E 31 A4 45 32 EF D5 " +
            "1D E9 63 77 3A E3 59 4B A1 3F 16 CB 9E C7 B7 62 " +
            "03 16 FB EF 10 90 81 B8 B0 0D 26 5B 27 3F 1F 89 " +
            "3D 73 85 C0 DE EF 8E 8D 9A 04 34 C0 73 95 28 51 " +
            "A3 22 5E 55 04 BE 56 4F 19 39 AF B1 65 09 C7 54 " +
            "B9 D6 11 FE 84 DF FD 0C D3 19 6F 1F 38 D0 8A A7 " +
            "9D DF 14 C4 5C FF B3 86 7C 3C 3E CC 78 82 5B 43 " +
            "B5 45 51 FD 50 79 4E 22 8E E6 82 24 4E 1F 18 12 " +
            "04 58 A9 86 DA 4C 85 D3 D2 53 7B 0D A9 8B 6B 6B " +
            "29 76 1B 26 76 D4 48 67",
            // After Pi
            "BF 81 1B 25 B8 BD AA AE 40 B6 44 8E 26 74 A9 C6 " +
            "3D 73 85 C0 DE EF 8E 8D 9D DF 14 C4 5C FF B3 86 " +
            "29 76 1B 26 76 D4 48 67 AD 2D F2 FA C6 4A 58 A5 " +
            "A1 3F 16 CB 9E C7 B7 62 03 16 FB EF 10 90 81 B8 " +
            "B9 D6 11 FE 84 DF FD 0C 04 58 A9 86 DA 4C 85 D3 " +
            "38 C9 8B 2C 01 53 97 55 A7 7E 31 A4 45 32 EF D5 " +
            "9A 04 34 C0 73 95 28 51 7C 3C 3E CC 78 82 5B 43 " +
            "B5 45 51 FD 50 79 4E 22 3D F6 12 17 C1 DE 4B 78 " +
            "61 2C 51 9E 2E 30 07 8D B0 0D 26 5B 27 3F 1F 89 " +
            "D3 19 6F 1F 38 D0 8A A7 D2 53 7B 0D A9 8B 6B 6B " +
            "A5 63 DD 0D 36 25 5E A7 1D E9 63 77 3A E3 59 4B " +
            "A3 22 5E 55 04 BE 56 4F 19 39 AF B1 65 09 C7 54 " +
            "8E E6 82 24 4E 1F 18 12",
            // After Chi  
            "82 C0 9A 65 60 36 AC A7 C0 3A 54 8A 26 64 98 C4 " +
            "1D 53 8E E2 FC EF C6 EC 0B 5E 14 C5 D4 D6 11 0E " +
            "69 40 5F AC 70 94 49 27 AF 2D 1B DE C6 5A 58 3D " +
            "19 FF 16 DB 1A 88 CB 66 07 1E 53 EF 4A 90 81 6B " +
            "10 F3 43 86 80 DD A5 28 04 4A AD 87 C2 C9 22 91 " +
            "20 C9 8F 6C 33 D6 97 55 C3 46 3B A8 4D 30 BC D7 " +
            "1B 45 75 F1 73 EC 2C 71 74 B4 B4 CC 79 80 CA 16 " +
            "32 73 61 7D 14 59 26 A2 AD F7 34 56 C0 D1 53 78 " +
            "22 3C 18 9A 36 F0 87 AB B0 4F 36 5B A6 34 7E C1 " +
            "FE BD 6F 0D 78 84 8A B7 92 5B 3A 85 87 AB 6F EE " +
            "07 61 C1 0D 32 39 58 A3 05 F0 C2 D7 5B E2 D8 5B " +
            "25 E4 5E 51 0E A8 4E 4D 38 38 F2 B8 55 29 81 F1 " +
            "96 6E A0 56 46 DD 19 5A",
            // After Iota 
            "0B 40 9A 65 60 36 AC 27 C0 3A 54 8A 26 64 98 C4 " +
            "1D 53 8E E2 FC EF C6 EC 0B 5E 14 C5 D4 D6 11 0E " +
            "69 40 5F AC 70 94 49 27 AF 2D 1B DE C6 5A 58 3D " +
            "19 FF 16 DB 1A 88 CB 66 07 1E 53 EF 4A 90 81 6B " +
            "10 F3 43 86 80 DD A5 28 04 4A AD 87 C2 C9 22 91 " +
            "20 C9 8F 6C 33 D6 97 55 C3 46 3B A8 4D 30 BC D7 " +
            "1B 45 75 F1 73 EC 2C 71 74 B4 B4 CC 79 80 CA 16 " +
            "32 73 61 7D 14 59 26 A2 AD F7 34 56 C0 D1 53 78 " +
            "22 3C 18 9A 36 F0 87 AB B0 4F 36 5B A6 34 7E C1 " +
            "FE BD 6F 0D 78 84 8A B7 92 5B 3A 85 87 AB 6F EE " +
            "07 61 C1 0D 32 39 58 A3 05 F0 C2 D7 5B E2 D8 5B " +
            "25 E4 5E 51 0E A8 4E 4D 38 38 F2 B8 55 29 81 F1 " +
            "96 6E A0 56 46 DD 19 5A",
            // Round #15  
            // After Theta
            "2B 92 D5 89 3E D8 F6 8C C6 4F 2E EB 9A 28 46 A4 " +
            "72 25 D0 22 E0 6D 9C 85 28 65 C6 39 76 3D 3C 34 " +
            "9D B8 D7 8F BF 16 EC 79 8F FF 54 32 98 B4 02 96 " +
            "1F 8A 6C BA A6 C4 15 06 68 68 0D 2F 56 12 DB 02 " +
            "33 C8 91 7A 22 36 88 12 F0 B2 25 A4 0D 4B 87 CF " +
            "00 1B C0 80 6D 38 CD FE C5 33 41 C9 F1 7C 62 B7 " +
            "74 33 2B 31 6F 6E 76 18 57 8F 66 30 DB 6B E7 2C " +
            "C6 8B E9 5E DB DB 83 FC 8D 25 7B BA 9E 3F 09 D3 " +
            "24 49 62 FB 8A BC 59 CB DF 39 68 9B BA B6 24 A8 " +
            "DD 86 BD F1 DA 6F A7 8D 66 A3 B2 A6 48 29 CA B0 " +
            "27 B3 8E E1 6C D7 02 08 03 85 B8 B6 E7 AE 06 3B " +
            "4A 92 00 91 12 2A 14 24 1B 03 20 44 F7 C2 AC CB " +
            "62 96 28 75 89 5F BC 04",
            // After Rho  
            "2B 92 D5 89 3E D8 F6 8C 8D 9F 5C D6 35 51 8C 48 " +
            "5C 09 B4 08 78 1B 67 A1 D7 C3 43 83 52 66 9C 63 " +
            "B5 60 CF EB C4 BD 7E FC 83 49 2B 60 F9 F8 4F 25 " +
            "A6 6B 4A 5C 61 F0 A1 C8 00 1A 5A C3 8B 95 C4 B6 " +
            "E4 48 3D 11 1B 44 89 19 74 F8 0C 2F 5B 42 DA B0 " +
            "07 D8 00 06 6C C3 69 F6 DD 16 CF 04 25 C7 F3 89 " +
            "89 79 73 B3 C3 A0 9B 59 D7 CE 59 AE 1E CD 60 B6 " +
            "AF ED ED 41 7E E3 C5 74 74 3D 7F 12 A6 1B 4B F6 " +
            "6C 5F 91 37 6B 99 24 49 12 D4 EF 1C B4 4D 5D 5B " +
            "ED B4 B1 DB B0 37 5E FB B0 66 A3 B2 A6 48 29 CA " +
            "0B 20 9C CC 3A 86 B3 5D 0C 14 E2 DA 9E BB 1A EC " +
            "49 12 20 52 42 85 82 44 03 20 44 F7 C2 AC CB 1B " +
            "2F 81 98 25 4A 5D E2 17",
            // After Pi
            "2B 92 D5 89 3E D8 F6 8C A6 6B 4A 5C 61 F0 A1 C8 " +
            "89 79 73 B3 C3 A0 9B 59 ED B4 B1 DB B0 37 5E FB " +
            "2F 81 98 25 4A 5D E2 17 D7 C3 43 83 52 66 9C 63 " +
            "74 F8 0C 2F 5B 42 DA B0 07 D8 00 06 6C C3 69 F6 " +
            "6C 5F 91 37 6B 99 24 49 49 12 20 52 42 85 82 44 " +
            "8D 9F 5C D6 35 51 8C 48 00 1A 5A C3 8B 95 C4 B6 " +
            "D7 CE 59 AE 1E CD 60 B6 B0 66 A3 B2 A6 48 29 CA " +
            "0B 20 9C CC 3A 86 B3 5D B5 60 CF EB C4 BD 7E FC " +
            "83 49 2B 60 F9 F8 4F 25 DD 16 CF 04 25 C7 F3 89 " +
            "12 D4 EF 1C B4 4D 5D 5B 03 20 44 F7 C2 AC CB 1B " +
            "5C 09 B4 08 78 1B 67 A1 E4 48 3D 11 1B 44 89 19 " +
            "AF ED ED 41 7E E3 C5 74 74 3D 7F 12 A6 1B 4B F6 " +
            "0C 14 E2 DA 9E BB 1A EC",
            // After Chi  
            "22 82 E4 2A BC D8 EC 9D C2 EF CA 14 51 E7 E5 6A " +
            "8B 78 7B 97 89 E8 3B 5D ED A6 F4 53 84 B7 4A 73 " +
            "AB E8 92 71 0B 7D E3 57 D4 C3 43 83 76 E7 BD 25 " +
            "1C FF 9D 1E 58 5A DE B9 06 D8 20 46 6C C7 EB F2 " +
            "FA 9E D2 B6 7B FB 38 6A 69 2A 2C 7E 4B 85 C0 D4 " +
            "5A 5B 5D FA 21 19 AC 48 20 3A F8 D3 2B 95 CD FE " +
            "DC CE 45 E2 06 4B F2 A3 34 F9 E3 A0 A3 19 25 CA " +
            "0B 20 9E CD B0 02 F3 EB E9 76 0B EF C0 BA CE 74 " +
            "81 89 0B 78 69 F0 43 77 DC 36 CF E7 67 67 71 89 " +
            "A6 94 64 14 B0 5C 69 BF 01 29 64 F7 FB EC CA 1A " +
            "57 AC 74 48 1C B8 23 C5 B4 58 2F 03 9B 5C 83 9B " +
            "A7 ED 6D 89 66 43 D5 7C 24 34 6B 12 C6 1B 2E F7 " +
            "AC 54 EB CB 9D FF 92 F4",
            // After Iota 
            "21 02 E4 2A BC D8 EC 1D C2 EF CA 14 51 E7 E5 6A " +
            "8B 78 7B 97 89 E8 3B 5D ED A6 F4 53 84 B7 4A 73 " +
            "AB E8 92 71 0B 7D E3 57 D4 C3 43 83 76 E7 BD 25 " +
            "1C FF 9D 1E 58 5A DE B9 06 D8 20 46 6C C7 EB F2 " +
            "FA 9E D2 B6 7B FB 38 6A 69 2A 2C 7E 4B 85 C0 D4 " +
            "5A 5B 5D FA 21 19 AC 48 20 3A F8 D3 2B 95 CD FE " +
            "DC CE 45 E2 06 4B F2 A3 34 F9 E3 A0 A3 19 25 CA " +
            "0B 20 9E CD B0 02 F3 EB E9 76 0B EF C0 BA CE 74 " +
            "81 89 0B 78 69 F0 43 77 DC 36 CF E7 67 67 71 89 " +
            "A6 94 64 14 B0 5C 69 BF 01 29 64 F7 FB EC CA 1A " +
            "57 AC 74 48 1C B8 23 C5 B4 58 2F 03 9B 5C 83 9B " +
            "A7 ED 6D 89 66 43 D5 7C 24 34 6B 12 C6 1B 2E F7 " +
            "AC 54 EB CB 9D FF 92 F4",
            // Round #16  
            // After Theta
            "D2 6A 5C 91 8B 38 09 19 86 C5 36 5B A2 42 F9 58 " +
            "03 40 64 B2 0D 48 2D AA 0E 2D 17 F3 4B 24 DD 87 " +
            "29 09 52 DB 4E 27 D3 4E 27 AB FB 38 41 07 58 21 " +
            "58 D5 61 51 AB FF C2 8B 8E E0 3F 63 E8 67 FD 05 " +
            "19 15 31 16 B4 68 AF 9E EB CB EC D4 0E DF F0 CD " +
            "A9 33 E5 41 16 F9 49 4C 64 10 04 9C D8 30 D1 CC " +
            "54 F6 5A C7 82 EB E4 54 D7 72 00 00 6C 8A B2 3E " +
            "89 C1 5E 67 F5 58 C3 F2 1A 1E B3 54 F7 5A 2B 70 " +
            "C5 A3 F7 37 9A 55 5F 45 54 0E D0 C2 E3 C7 67 7E " +
            "45 1F 87 B4 7F CF FE 4B 83 C8 A4 5D BE B6 FA 03 " +
            "A4 C4 CC F3 2B 58 C6 C1 F0 72 D3 4C 68 F9 9F A9 " +
            "2F D5 72 AC E2 E3 C3 8B C7 BF 88 B2 09 88 B9 03 " +
            "2E B5 2B 61 D8 A5 A2 ED",
            // After Rho  
            "D2 6A 5C 91 8B 38 09 19 0C 8B 6D B6 44 85 F2 B1 " +
            "00 10 99 6C 03 52 8B EA 44 D2 7D E8 D0 72 31 BF " +
            "3A 99 76 4A 49 90 DA 76 13 74 80 15 72 B2 BA 8F " +
            "16 B5 FA 2F BC 88 55 1D 81 23 F8 CF 18 FA 59 7F " +
            "8A 18 0B 5A B4 57 CF 8C 0D DF BC BE CC 4E ED F0 " +
            "4A 9D 29 0F B2 C8 4F 62 33 93 41 10 70 62 C3 44 " +
            "3A 16 5C 27 A7 A2 B2 D7 14 65 7D AE E5 00 00 D8 " +
            "B3 7A AC 61 F9 C4 60 AF A9 EE B5 56 E0 34 3C 66 " +
            "FE 46 B3 EA AB A8 78 F4 33 3F 2A 07 68 E1 F1 E3 " +
            "D9 7F A9 E8 E3 90 F6 EF 03 83 C8 A4 5D BE B6 FA " +
            "19 07 93 12 33 CF AF 60 C2 CB 4D 33 A1 E5 7F A6 " +
            "A5 5A 8E 55 7C 7C 78 F1 BF 88 B2 09 88 B9 03 C7 " +
            "68 BB 4B ED 4A 18 76 A9",
            // After Pi
            "D2 6A 5C 91 8B 38 09 19 16 B5 FA 2F BC 88 55 1D " +
            "3A 16 5C 27 A7 A2 B2 D7 D9 7F A9 E8 E3 90 F6 EF " +
            "68 BB 4B ED 4A 18 76 A9 44 D2 7D E8 D0 72 31 BF " +
            "0D DF BC BE CC 4E ED F0 4A 9D 29 0F B2 C8 4F 62 " +
            "FE 46 B3 EA AB A8 78 F4 A5 5A 8E 55 7C 7C 78 F1 " +
            "0C 8B 6D B6 44 85 F2 B1 81 23 F8 CF 18 FA 59 7F " +
            "14 65 7D AE E5 00 00 D8 03 83 C8 A4 5D BE B6 FA " +
            "19 07 93 12 33 CF AF 60 3A 99 76 4A 49 90 DA 76 " +
            "13 74 80 15 72 B2 BA 8F 33 93 41 10 70 62 C3 44 " +
            "33 3F 2A 07 68 E1 F1 E3 BF 88 B2 09 88 B9 03 C7 " +
            "00 10 99 6C 03 52 8B EA 8A 18 0B 5A B4 57 CF 8C " +
            "B3 7A AC 61 F9 C4 60 AF A9 EE B5 56 E0 34 3C 66 " +
            "C2 CB 4D 33 A1 E5 7F A6",
            // After Chi  
            "FA 68 58 91 88 1A AB DB D7 DC 5B E7 FC 98 11 35 " +
            "1A 96 1E 22 AF AA B2 D7 4B 3F BD F8 62 B0 FF FF " +
            "6C 2E E9 C3 7E 98 22 AD 06 D2 7C E9 E2 F2 33 BD " +
            "B9 9D 2E 5E C5 6E DD 64 4B 85 25 1A E6 9C 4F 63 " +
            "BE C6 C2 42 2B AA 79 FA AC 57 0E 43 70 70 B4 B1 " +
            "18 CF 68 96 A1 85 F2 31 82 A1 78 CF 00 44 EF 5D " +
            "0C 61 6E BC C7 41 09 D8 07 0B A4 00 19 BE E6 6B " +
            "98 27 03 5B 2B B5 A6 2E 1A 1A 37 4A 49 D0 9B 36 " +
            "13 58 AA 12 7A 33 8A 2C BF 13 D1 18 F0 7A C1 40 " +
            "33 2E 6E 45 29 E1 29 D3 BE EC 32 1C BA 9B 23 4E " +
            "31 72 3D 4D 4A D2 AB C9 82 9C 1A 4C B4 67 D3 CC " +
            "F1 7B E4 40 F8 05 23 2F A9 FE 25 1A E2 26 BC 2E " +
            "48 C3 4F 21 15 E0 3B A2",
            // After Iota 
            "F8 E8 58 91 88 1A AB 5B D7 DC 5B E7 FC 98 11 35 " +
            "1A 96 1E 22 AF AA B2 D7 4B 3F BD F8 62 B0 FF FF " +
            "6C 2E E9 C3 7E 98 22 AD 06 D2 7C E9 E2 F2 33 BD " +
            "B9 9D 2E 5E C5 6E DD 64 4B 85 25 1A E6 9C 4F 63 " +
            "BE C6 C2 42 2B AA 79 FA AC 57 0E 43 70 70 B4 B1 " +
            "18 CF 68 96 A1 85 F2 31 82 A1 78 CF 00 44 EF 5D " +
            "0C 61 6E BC C7 41 09 D8 07 0B A4 00 19 BE E6 6B " +
            "98 27 03 5B 2B B5 A6 2E 1A 1A 37 4A 49 D0 9B 36 " +
            "13 58 AA 12 7A 33 8A 2C BF 13 D1 18 F0 7A C1 40 " +
            "33 2E 6E 45 29 E1 29 D3 BE EC 32 1C BA 9B 23 4E " +
            "31 72 3D 4D 4A D2 AB C9 82 9C 1A 4C B4 67 D3 CC " +
            "F1 7B E4 40 F8 05 23 2F A9 FE 25 1A E2 26 BC 2E " +
            "48 C3 4F 21 15 E0 3B A2",
            // Round #17  
            // After Theta
            "AD D1 BB 26 EC F1 76 5D 3C 75 DD B6 39 E6 67 1B " +
            "B6 F6 83 C1 6F 8B 22 1C 05 C6 EF E9 F1 F5 B9 40 " +
            "9E 37 F4 F4 74 24 63 6E 53 EB 9F 5E 86 19 EE BB " +
            "52 34 A8 0F 00 10 AB 4A E7 E5 B8 F9 26 BD DF A8 " +
            "F0 3F 90 53 B8 EF 3F 45 5E 4E 13 74 7A CC F5 72 " +
            "4D F6 8B 21 C5 6E 2F 37 69 08 FE 9E C5 3A 99 73 " +
            "A0 01 F3 5F 07 60 99 13 49 F2 F6 11 8A FB A0 D4 " +
            "6A 3E 1E 6C 21 09 E7 ED 4F 23 D4 FD 2D 3B 46 30 " +
            "F8 F1 2C 43 BF 4D FC 02 13 73 4C FB 30 5B 51 8B " +
            "7D D7 3C 54 BA A4 6F 6C 4C F5 2F 2B B0 27 62 8D " +
            "64 4B DE FA 2E 39 76 CF 69 35 9C 1D 71 19 A5 E2 " +
            "5D 1B 79 A3 38 24 B3 E4 E7 07 77 0B 71 63 FA 91 " +
            "BA DA 52 16 1F 5C 7A 61",
            // After Rho  
            "AD D1 BB 26 EC F1 76 5D 78 EA BA 6D 73 CC CF 36 " +
            "AD FD 60 F0 DB A2 08 87 5F 9F 0B 54 60 FC 9E 1E " +
            "23 19 73 F3 BC A1 A7 A7 65 98 E1 BE 3B B5 FE E9 " +
            "FA 00 00 B1 AA 24 45 83 EA 79 39 6E BE 49 EF 37 " +
            "1F C8 29 DC F7 9F 22 F8 5C 2F E7 E5 34 41 A7 C7 " +
            "69 B2 5F 0C 29 76 7B B9 CE A5 21 F8 7B 16 EB 64 " +
            "FF 3A 00 CB 9C 00 0D 98 F7 41 A9 93 E4 ED 23 14 " +
            "B6 90 84 F3 76 35 1F 0F FB 5B 76 8C 60 9E 46 A8 " +
            "65 E8 B7 89 5F 00 3F 9E A8 C5 89 39 A6 7D 98 AD " +
            "F4 8D AD EF 9A 87 4A 97 8D 4C F5 2F 2B B0 27 62 " +
            "D8 3D 93 2D 79 EB BB E4 A7 D5 70 76 C4 65 94 8A " +
            "6B 23 6F 14 87 64 96 BC 07 77 0B 71 63 FA 91 E7 " +
            "5E 98 AE B6 94 C5 07 97",
            // After Pi
            "AD D1 BB 26 EC F1 76 5D FA 00 00 B1 AA 24 45 83 " +
            "FF 3A 00 CB 9C 00 0D 98 F4 8D AD EF 9A 87 4A 97 " +
            "5E 98 AE B6 94 C5 07 97 5F 9F 0B 54 60 FC 9E 1E " +
            "5C 2F E7 E5 34 41 A7 C7 69 B2 5F 0C 29 76 7B B9 " +
            "65 E8 B7 89 5F 00 3F 9E 6B 23 6F 14 87 64 96 BC " +
            "78 EA BA 6D 73 CC CF 36 EA 79 39 6E BE 49 EF 37 " +
            "F7 41 A9 93 E4 ED 23 14 8D 4C F5 2F 2B B0 27 62 " +
            "D8 3D 93 2D 79 EB BB E4 23 19 73 F3 BC A1 A7 A7 " +
            "65 98 E1 BE 3B B5 FE E9 CE A5 21 F8 7B 16 EB 64 " +
            "A8 C5 89 39 A6 7D 98 AD 07 77 0B 71 63 FA 91 E7 " +
            "AD FD 60 F0 DB A2 08 87 1F C8 29 DC F7 9F 22 F8 " +
            "B6 90 84 F3 76 35 1F 0F FB 5B 76 8C 60 9E 46 A8 " +
            "A7 D5 70 76 C4 65 94 8A",
            // After Chi  
            "A8 EB BB 6C F8 F1 7E 45 FA 85 AD 95 A8 A3 07 84 " +
            "F5 2A 02 DB 98 40 08 98 55 CC BC EF F2 B7 3A DF " +
            "0C 98 AE 27 96 C1 06 15 7E 0F 13 5C 69 CA C6 26 " +
            "58 67 47 64 62 41 A3 C1 63 B1 17 18 A9 12 FB 99 " +
            "71 74 B7 C9 3F 98 37 9C 6B 03 8B B5 93 65 B7 7D " +
            "6D EA 3A FC 33 68 CF 36 E2 75 6D 42 B5 59 EB 55 " +
            "A7 70 AB 93 B4 A6 BB 90 AD 8E DD 6F 29 B4 63 70 " +
            "5A 2C 92 2F F5 EA 9B E5 A9 3C 73 B3 FC A3 A6 A3 " +
            "45 D8 69 BF BF DC EE 60 C9 97 23 B8 3A 94 EA 26 " +
            "88 CD F9 BB 3A 7C BE AD 43 F7 8B 7D 60 EE C9 AF " +
            "0D ED E4 D3 DB 82 15 80 56 83 5B D0 F7 15 62 58 " +
            "B2 14 84 81 F2 54 8F 0D F3 73 76 0C 7B 1C 4E AD " +
            "B5 D5 79 7A E0 78 B6 F2",
            // After Iota 
            "28 EB BB 6C F8 F1 7E C5 FA 85 AD 95 A8 A3 07 84 " +
            "F5 2A 02 DB 98 40 08 98 55 CC BC EF F2 B7 3A DF " +
            "0C 98 AE 27 96 C1 06 15 7E 0F 13 5C 69 CA C6 26 " +
            "58 67 47 64 62 41 A3 C1 63 B1 17 18 A9 12 FB 99 " +
            "71 74 B7 C9 3F 98 37 9C 6B 03 8B B5 93 65 B7 7D " +
            "6D EA 3A FC 33 68 CF 36 E2 75 6D 42 B5 59 EB 55 " +
            "A7 70 AB 93 B4 A6 BB 90 AD 8E DD 6F 29 B4 63 70 " +
            "5A 2C 92 2F F5 EA 9B E5 A9 3C 73 B3 FC A3 A6 A3 " +
            "45 D8 69 BF BF DC EE 60 C9 97 23 B8 3A 94 EA 26 " +
            "88 CD F9 BB 3A 7C BE AD 43 F7 8B 7D 60 EE C9 AF " +
            "0D ED E4 D3 DB 82 15 80 56 83 5B D0 F7 15 62 58 " +
            "B2 14 84 81 F2 54 8F 0D F3 73 76 0C 7B 1C 4E AD " +
            "B5 D5 79 7A E0 78 B6 F2",
            // Round #18  
            // After Theta
            "45 E6 95 6F E7 CD AD 44 F0 8A 9A EB B7 B9 99 06 " +
            "42 F7 04 FB E4 C5 F6 D7 88 8F 2E F2 5E 33 BC C5 " +
            "C1 AF FC 81 38 DF 10 CB 13 02 3D 5F 76 F6 15 A7 " +
            "52 68 70 1A 7D 5B 3D 43 D4 6C 11 38 D5 97 05 D6 " +
            "AC 37 25 D4 93 1C B1 86 A6 34 D9 13 3D 7B A1 A3 " +
            "00 E7 14 FF 2C 54 1C B7 E8 7A 5A 3C AA 43 75 D7 " +
            "10 AD AD B3 C8 23 45 DF 70 CD 4F 72 85 30 E5 6A " +
            "97 1B C0 89 5B F4 8D 3B C4 31 5D B0 E3 9F 75 22 " +
            "4F D7 5E C1 A0 C6 70 E2 7E 4A 25 98 46 11 14 69 " +
            "55 8E 6B A6 96 F8 38 B7 8E C0 D9 DB CE F0 DF 71 " +
            "60 E0 CA D0 C4 BE C6 01 5C 8C 6C AE E8 0F FC DA " +
            "05 C9 82 A1 8E D1 71 42 2E 30 E4 11 D7 98 C8 B7 " +
            "78 E2 2B DC 4E 66 A0 2C",
            // After Rho  
            "45 E6 95 6F E7 CD AD 44 E0 15 35 D7 6F 73 33 0D " +
            "D0 3D C1 3E 79 B1 FD B5 35 C3 5B 8C F8 E8 22 EF " +
            "F9 86 58 0E 7E E5 0F C4 65 67 5F 71 3A 21 D0 F3 " +
            "A7 D1 B7 D5 33 24 85 06 35 35 5B 04 4E F5 65 81 " +
            "9B 12 EA 49 8E 58 43 D6 17 3A 6A 4A 93 3D D1 B3 " +
            "05 38 A7 F8 67 A1 E2 B8 5D A3 EB 69 F1 A8 0E D5 " +
            "9D 45 1E 29 FA 86 68 6D 61 CA D5 E0 9A 9F E4 0A " +
            "C4 2D FA C6 9D CB 0D E0 60 C7 3F EB 44 88 63 BA " +
            "2B 18 D4 18 4E FC E9 DA 8A 34 3F A5 12 4C A3 08 " +
            "1F E7 B6 CA 71 CD D4 12 71 8E C0 D9 DB CE F0 DF " +
            "1A 07 80 81 2B 43 13 FB 73 31 B2 B9 A2 3F F0 6B " +
            "20 59 30 D4 31 3A 4E A8 30 E4 11 D7 98 C8 B7 2E " +
            "28 0B 9E F8 0A B7 93 19",
            // After Pi
            "45 E6 95 6F E7 CD AD 44 A7 D1 B7 D5 33 24 85 06 " +
            "9D 45 1E 29 FA 86 68 6D 1F E7 B6 CA 71 CD D4 12 " +
            "28 0B 9E F8 0A B7 93 19 35 C3 5B 8C F8 E8 22 EF " +
            "17 3A 6A 4A 93 3D D1 B3 05 38 A7 F8 67 A1 E2 B8 " +
            "2B 18 D4 18 4E FC E9 DA 20 59 30 D4 31 3A 4E A8 " +
            "E0 15 35 D7 6F 73 33 0D 35 35 5B 04 4E F5 65 81 " +
            "61 CA D5 E0 9A 9F E4 0A 71 8E C0 D9 DB CE F0 DF " +
            "1A 07 80 81 2B 43 13 FB F9 86 58 0E 7E E5 0F C4 " +
            "65 67 5F 71 3A 21 D0 F3 5D A3 EB 69 F1 A8 0E D5 " +
            "8A 34 3F A5 12 4C A3 08 30 E4 11 D7 98 C8 B7 2E " +
            "D0 3D C1 3E 79 B1 FD B5 9B 12 EA 49 8E 58 43 D6 " +
            "C4 2D FA C6 9D CB 0D E0 60 C7 3F EB 44 88 63 BA " +
            "73 31 B2 B9 A2 3F F0 6B",
            // After Chi  
            "5D E2 9D 47 2F 4F C5 2D A5 73 17 17 32 6D 11 14 " +
            "BD 4D 16 19 F0 B4 6B 64 5A 03 B7 CD 94 85 F8 56 " +
            "8A 1A BC 68 1A 97 93 1B 35 C3 DE 3C 9C 68 00 E7 " +
            "3D 3A 3A 4A 9B 61 D8 F1 05 79 87 3C 56 A3 E4 98 " +
            "3E 9A 9F 10 86 3C C9 9D 22 61 10 96 32 2F 9F B8 " +
            "A0 DF B1 37 FF 79 B3 07 25 31 5B 1D 0F B5 75 54 " +
            "6B CB D5 E0 BA 9E E7 2A 91 9E F5 8F 9F FE D0 DB " +
            "0F 27 CA 81 2B C7 57 7B E1 06 F8 06 BF 6D 01 C0 " +
            "E7 73 4B F5 38 65 71 FB 6D 63 EB 3B 79 28 1A F3 " +
            "43 36 77 AD 74 69 AB C8 34 85 16 A6 98 C8 67 1D " +
            "94 10 D1 B8 68 32 F1 95 BB D0 EF 60 CE 58 21 CC " +
            "D7 1D 7A D6 3F FC 9D A1 E0 CB 7E ED 1D 08 6E 2E " +
            "78 33 98 F8 24 77 F2 29",
            // After Iota 
            "57 62 9D 47 2F 4F C5 2D A5 73 17 17 32 6D 11 14 " +
            "BD 4D 16 19 F0 B4 6B 64 5A 03 B7 CD 94 85 F8 56 " +
            "8A 1A BC 68 1A 97 93 1B 35 C3 DE 3C 9C 68 00 E7 " +
            "3D 3A 3A 4A 9B 61 D8 F1 05 79 87 3C 56 A3 E4 98 " +
            "3E 9A 9F 10 86 3C C9 9D 22 61 10 96 32 2F 9F B8 " +
            "A0 DF B1 37 FF 79 B3 07 25 31 5B 1D 0F B5 75 54 " +
            "6B CB D5 E0 BA 9E E7 2A 91 9E F5 8F 9F FE D0 DB " +
            "0F 27 CA 81 2B C7 57 7B E1 06 F8 06 BF 6D 01 C0 " +
            "E7 73 4B F5 38 65 71 FB 6D 63 EB 3B 79 28 1A F3 " +
            "43 36 77 AD 74 69 AB C8 34 85 16 A6 98 C8 67 1D " +
            "94 10 D1 B8 68 32 F1 95 BB D0 EF 60 CE 58 21 CC " +
            "D7 1D 7A D6 3F FC 9D A1 E0 CB 7E ED 1D 08 6E 2E " +
            "78 33 98 F8 24 77 F2 29",
            // Round #19  
            // After Theta
            "7F 3F D0 CD 31 87 D2 CC C1 19 67 B4 1D D6 49 85 " +
            "F1 62 6D E9 68 7D CF 0E E4 57 B3 A6 B0 59 8A 0B " +
            "B3 31 DE 9F C9 B2 BB DC 1D 9E 93 B6 82 A0 17 06 " +
            "59 50 4A E9 B4 DA 80 60 49 56 FC CC CE 6A 40 F2 " +
            "80 CE 9B 7B A2 E0 BB C0 1B 4A 72 61 E1 0A B7 7F " +
            "88 82 FC BD E1 B1 A4 E6 41 5B 2B BE 20 0E 2D C5 " +
            "27 E4 AE 10 22 57 43 40 2F CA F1 E4 BB 22 A2 86 " +
            "36 0C A8 76 F8 E2 7F BC C9 5B B5 8C A1 A5 16 21 " +
            "83 19 3B 56 17 DE 29 6A 21 4C 90 CB E1 E1 BE 99 " +
            "FD 62 73 C6 50 B5 D9 95 0D AE 74 51 4B ED 4F DA " +
            "BC 4D 9C 32 76 FA E6 74 DF BA 9F C3 E1 E3 79 5D " +
            "9B 32 01 26 A7 35 39 CB 5E 9F 7A 86 39 D4 1C 73 " +
            "41 18 FA 0F F7 52 DA EE",
            // After Rho  
            "7F 3F D0 CD 31 87 D2 CC 83 33 CE 68 3B AC 93 0A " +
            "BC 58 5B 3A 5A DF B3 43 9B A5 B8 40 7E 35 6B 0A " +
            "96 DD E5 9E 8D F1 FE 4C 2B 08 7A 61 D0 E1 39 69 " +
            "94 4E AB 0D 08 96 05 A5 7C 92 15 3F B3 B3 1A 90 " +
            "E7 CD 3D 51 F0 5D 60 40 70 FB B7 A1 24 17 16 AE " +
            "47 14 E4 EF 0D 8F 25 35 14 07 6D AD F8 82 38 B4 " +
            "85 10 B9 1A 02 3A 21 77 45 44 0D 5F 94 E3 C9 77 " +
            "3B 7C F1 3F 5E 1B 06 54 19 43 4B 2D 42 92 B7 6A " +
            "C7 EA C2 3B 45 6D 30 63 DF CC 10 26 C8 E5 F0 70 " +
            "36 BB B2 5F 6C CE 18 AA DA 0D AE 74 51 4B ED 4F " +
            "9B D3 F1 36 71 CA D8 E9 7D EB 7E 0E 87 8F E7 75 " +
            "53 26 C0 E4 B4 26 67 79 9F 7A 86 39 D4 1C 73 5E " +
            "B6 7B 10 86 FE C3 BD 94",
            // After Pi
            "7F 3F D0 CD 31 87 D2 CC 94 4E AB 0D 08 96 05 A5 " +
            "85 10 B9 1A 02 3A 21 77 36 BB B2 5F 6C CE 18 AA " +
            "B6 7B 10 86 FE C3 BD 94 9B A5 B8 40 7E 35 6B 0A " +
            "70 FB B7 A1 24 17 16 AE 47 14 E4 EF 0D 8F 25 35 " +
            "C7 EA C2 3B 45 6D 30 63 53 26 C0 E4 B4 26 67 79 " +
            "83 33 CE 68 3B AC 93 0A 7C 92 15 3F B3 B3 1A 90 " +
            "45 44 0D 5F 94 E3 C9 77 DA 0D AE 74 51 4B ED 4F " +
            "9B D3 F1 36 71 CA D8 E9 96 DD E5 9E 8D F1 FE 4C " +
            "2B 08 7A 61 D0 E1 39 69 14 07 6D AD F8 82 38 B4 " +
            "DF CC 10 26 C8 E5 F0 70 9F 7A 86 39 D4 1C 73 5E " +
            "BC 58 5B 3A 5A DF B3 43 E7 CD 3D 51 F0 5D 60 40 " +
            "3B 7C F1 3F 5E 1B 06 54 19 43 4B 2D 42 92 B7 6A " +
            "7D EB 7E 0E 87 8F E7 75",
            // After Chi  
            "7E 2F C0 DF 33 AF F2 9E A6 E5 A9 48 64 52 1D 2D " +
            "05 50 B9 9A 90 3B 84 63 7F BF 72 16 6D CA 5A E2 " +
            "36 3B 3B 86 F6 D3 B8 B5 9C A1 F8 0E 77 BD 4A 1B " +
            "F0 11 B5 B1 64 77 06 EC 57 10 E4 2B BD 8D 62 2D " +
            "4F 6B FA 3B 0F 7C 38 61 33 7C C7 45 B4 24 73 DD " +
            "82 77 C6 28 3F EC 52 6D E6 9B B7 1F F2 BB 3E 98 " +
            "44 96 5C 5D B4 63 D9 D7 DA 2D A0 3C 5B 6F EE 4D " +
            "E7 53 E0 21 F1 D9 D0 79 82 DA E0 12 A5 F3 FE D8 " +
            "E0 C0 6A 63 D0 84 F9 29 14 35 EB B4 EC 9A 3B BA " +
            "DF 49 71 A0 C1 04 7C 70 B6 7A 9C 58 84 1C 72 7F " +
            "A4 68 9B 14 54 DD B5 57 E7 CE 37 51 F0 DD D1 6A " +
            "5F D4 C5 3D DB 16 46 41 99 53 4A 1D 1A C2 A7 68 " +
            "3E 6E 5A 4F 27 8F A7 75",
            // After Iota 
            "74 2F C0 5F 33 AF F2 1E A6 E5 A9 48 64 52 1D 2D " +
            "05 50 B9 9A 90 3B 84 63 7F BF 72 16 6D CA 5A E2 " +
            "36 3B 3B 86 F6 D3 B8 B5 9C A1 F8 0E 77 BD 4A 1B " +
            "F0 11 B5 B1 64 77 06 EC 57 10 E4 2B BD 8D 62 2D " +
            "4F 6B FA 3B 0F 7C 38 61 33 7C C7 45 B4 24 73 DD " +
            "82 77 C6 28 3F EC 52 6D E6 9B B7 1F F2 BB 3E 98 " +
            "44 96 5C 5D B4 63 D9 D7 DA 2D A0 3C 5B 6F EE 4D " +
            "E7 53 E0 21 F1 D9 D0 79 82 DA E0 12 A5 F3 FE D8 " +
            "E0 C0 6A 63 D0 84 F9 29 14 35 EB B4 EC 9A 3B BA " +
            "DF 49 71 A0 C1 04 7C 70 B6 7A 9C 58 84 1C 72 7F " +
            "A4 68 9B 14 54 DD B5 57 E7 CE 37 51 F0 DD D1 6A " +
            "5F D4 C5 3D DB 16 46 41 99 53 4A 1D 1A C2 A7 68 " +
            "3E 6E 5A 4F 27 8F A7 75",
            // Round #20  
            // After Theta
            "70 EC F6 03 86 9D 27 31 50 C0 72 FD B2 31 38 0E " +
            "EB F6 68 16 87 C3 27 D5 F6 88 E9 98 E2 E9 85 B7 " +
            "03 4E 22 D5 00 6D AC AC 98 62 CE 52 C2 8F 9F 34 " +
            "06 34 6E 04 B2 14 23 CF B9 B6 35 A7 AA 75 C1 9B " +
            "C6 5C 61 B5 80 5F E7 34 06 09 DE 16 42 9A 67 C4 " +
            "86 B4 F0 74 8A DE 87 42 10 BE 6C AA 24 D8 1B BB " +
            "AA 30 8D D1 A3 9B 7A 61 53 1A 3B B2 D4 4C 31 18 " +
            "D2 26 F9 72 07 67 C4 60 86 19 D6 4E 10 C1 2B F7 " +
            "16 E5 B1 D6 06 E7 DC 0A FA 93 3A 38 FB 62 98 0C " +
            "56 7E EA 2E 4E 27 A3 25 83 0F 85 0B 72 A2 66 66 " +
            "A0 AB AD 48 E1 EF 60 78 11 EB EC E4 26 BE F4 49 " +
            "B1 72 14 B1 CC EE E5 F7 10 64 D1 93 95 E1 78 3D " +
            "0B 1B 43 1C D1 31 B3 6C",
            // After Rho  
            "70 EC F6 03 86 9D 27 31 A0 80 E5 FA 65 63 70 1C " +
            "BA 3D 9A C5 E1 F0 49 F5 9E 5E 78 6B 8F 98 8E 29 " +
            "68 63 65 1D 70 12 A9 06 25 FC F8 49 83 29 E6 2C " +
            "46 20 4B 31 F2 6C 40 E3 66 AE 6D CD A9 6A 5D F0 " +
            "AE B0 5A C0 AF 73 1A 63 79 46 6C 90 E0 6D 21 A4 " +
            "32 A4 85 A7 53 F4 3E 14 EC 42 F8 B2 A9 92 60 6F " +
            "8C 1E DD D4 0B 53 85 69 99 62 30 A6 34 76 64 A9 " +
            "B9 83 33 62 30 69 93 7C 9D 20 82 57 EE 0D 33 AC " +
            "D6 DA E0 9C 5B C1 A2 3C 4C 06 FD 49 1D 9C 7D 31 " +
            "64 B4 C4 CA 4F DD C5 E9 66 83 0F 85 0B 72 A2 66 " +
            "83 E1 81 AE B6 22 85 BF 45 AC B3 93 9B F8 D2 27 " +
            "56 8E 22 96 D9 BD FC 3E 64 D1 93 95 E1 78 3D 10 " +
            "2C DB C2 C6 10 47 74 CC",
            // After Pi
            "70 EC F6 03 86 9D 27 31 46 20 4B 31 F2 6C 40 E3 " +
            "8C 1E DD D4 0B 53 85 69 64 B4 C4 CA 4F DD C5 E9 " +
            "2C DB C2 C6 10 47 74 CC 9E 5E 78 6B 8F 98 8E 29 " +
            "79 46 6C 90 E0 6D 21 A4 32 A4 85 A7 53 F4 3E 14 " +
            "D6 DA E0 9C 5B C1 A2 3C 56 8E 22 96 D9 BD FC 3E " +
            "A0 80 E5 FA 65 63 70 1C 66 AE 6D CD A9 6A 5D F0 " +
            "99 62 30 A6 34 76 64 A9 66 83 0F 85 0B 72 A2 66 " +
            "83 E1 81 AE B6 22 85 BF 68 63 65 1D 70 12 A9 06 " +
            "25 FC F8 49 83 29 E6 2C EC 42 F8 B2 A9 92 60 6F " +
            "4C 06 FD 49 1D 9C 7D 31 64 D1 93 95 E1 78 3D 10 " +
            "BA 3D 9A C5 E1 F0 49 F5 AE B0 5A C0 AF 73 1A 63 " +
            "B9 83 33 62 30 69 93 7C 9D 20 82 57 EE 0D 33 AC " +
            "45 AC B3 93 9B F8 D2 27",
            // After Chi  
            "F8 F2 62 C7 8F 8E A2 39 26 80 4B 3B B6 E0 00 63 " +
            "84 55 DF D0 1B 51 B5 6D 34 90 F0 CB C9 45 C6 D8 " +
            "2A DB CB F6 60 27 34 0E 9C FE F9 4C 9C 08 90 39 " +
            "BD 1C 0C 88 E8 6C A1 8C 32 A0 87 A5 D3 C8 62 16 " +
            "5E 8A B8 F5 5D C1 A0 3D 37 8E 26 06 B9 D8 DD BA " +
            "39 C0 F5 D8 71 77 50 15 00 2F 62 CC A2 6A DF B6 " +
            "18 02 B0 8C 80 76 61 30 46 83 6B D5 4A 33 D2 66 " +
            "C5 CF 89 AB 3E 2A 88 5F A0 61 65 AF 58 80 A9 45 " +
            "25 F8 FD 00 97 25 FB 3C CC 93 FA 26 49 F2 60 6F " +
            "44 24 99 41 0D 9E FD 37 61 4D 0B D5 62 51 7B 38 " +
            "AB 3E BB E7 F1 F8 C8 E9 AA 90 DA D5 61 77 3A E3 " +
            "F9 0F 02 E2 21 99 53 7F 27 31 8A 13 8E 0D 3A 7C " +
            "41 2C F3 93 95 FB C0 25",
            // After Iota 
            "79 72 62 47 8F 8E A2 B9 26 80 4B 3B B6 E0 00 63 " +
            "84 55 DF D0 1B 51 B5 6D 34 90 F0 CB C9 45 C6 D8 " +
            "2A DB CB F6 60 27 34 0E 9C FE F9 4C 9C 08 90 39 " +
            "BD 1C 0C 88 E8 6C A1 8C 32 A0 87 A5 D3 C8 62 16 " +
            "5E 8A B8 F5 5D C1 A0 3D 37 8E 26 06 B9 D8 DD BA " +
            "39 C0 F5 D8 71 77 50 15 00 2F 62 CC A2 6A DF B6 " +
            "18 02 B0 8C 80 76 61 30 46 83 6B D5 4A 33 D2 66 " +
            "C5 CF 89 AB 3E 2A 88 5F A0 61 65 AF 58 80 A9 45 " +
            "25 F8 FD 00 97 25 FB 3C CC 93 FA 26 49 F2 60 6F " +
            "44 24 99 41 0D 9E FD 37 61 4D 0B D5 62 51 7B 38 " +
            "AB 3E BB E7 F1 F8 C8 E9 AA 90 DA D5 61 77 3A E3 " +
            "F9 0F 02 E2 21 99 53 7F 27 31 8A 13 8E 0D 3A 7C " +
            "41 2C F3 93 95 FB C0 25",
            // Round #21  
            // After Theta
            "A8 3F FB 0E 8A 99 07 42 C7 44 DB DA 3D 61 08 ED " +
            "0F 96 BC 08 AA AD EC 7B 5E 0C D9 CD C9 3F F7 6E " +
            "CB 70 9B 78 AA 10 40 B4 4D B3 60 05 99 1F 35 C2 " +
            "5C D8 9C 69 63 ED A9 02 B9 63 E4 7D 62 34 3B 00 " +
            "34 16 91 F3 5D BB 91 8B D6 25 76 88 73 EF A9 00 " +
            "E8 8D 6C 91 74 60 F5 EE E1 EB F2 2D 29 EB D7 38 " +
            "93 C1 D3 54 31 8A 38 26 2C 1F 42 D3 4A 49 E3 D0 " +
            "24 64 D9 25 F4 1D FC E5 71 2C FC E6 5D 97 0C BE " +
            "C4 3C 6D E1 1C A4 F3 B2 47 50 99 FE F8 0E 39 79 " +
            "2E B8 B0 47 0D E4 CC 81 80 E6 5B 5B A8 66 0F 82 " +
            "7A 73 22 AE F4 EF 6D 12 4B 54 4A 34 EA F6 32 6D " +
            "72 CC 61 3A 90 65 0A 69 4D AD A3 15 8E 77 0B CA " +
            "A0 87 A3 1D 5F CC B4 9F",
            // After Rho  
            "A8 3F FB 0E 8A 99 07 42 8F 89 B6 B5 7B C2 10 DA " +
            "83 25 2F 82 6A 2B FB DE FC 73 EF E6 C5 90 DD 9C " +
            "85 00 A2 5D 86 DB C4 53 90 F9 51 23 DC 34 0B 56 " +
            "99 36 D6 9E 2A C0 85 CD 40 EE 18 79 9F 18 CD 0E " +
            "8B C8 F9 AE DD C8 45 1A 9E 0A 60 5D 62 87 38 F7 " +
            "47 6F 64 8B A4 03 AB 77 E3 84 AF CB B7 A4 AC 5F " +
            "A6 8A 51 C4 31 99 0C 9E 92 C6 A1 59 3E 84 A6 95 " +
            "12 FA 0E FE 72 12 B2 EC CD BB 2E 19 7C E3 58 F8 " +
            "2D 9C 83 74 5E 96 98 A7 9C BC 23 A8 4C 7F 7C 87 " +
            "9C 39 D0 05 17 F6 A8 81 82 80 E6 5B 5B A8 66 0F " +
            "B7 49 E8 CD 89 B8 D2 BF 2D 51 29 D1 A8 DB CB B4 " +
            "8E 39 4C 07 B2 4C 21 4D AD A3 15 8E 77 0B CA 4D " +
            "ED 27 E8 E1 68 C7 17 33",
            // After Pi
            "A8 3F FB 0E 8A 99 07 42 99 36 D6 9E 2A C0 85 CD " +
            "A6 8A 51 C4 31 99 0C 9E 9C 39 D0 05 17 F6 A8 81 " +
            "ED 27 E8 E1 68 C7 17 33 FC 73 EF E6 C5 90 DD 9C " +
            "9E 0A 60 5D 62 87 38 F7 47 6F 64 8B A4 03 AB 77 " +
            "2D 9C 83 74 5E 96 98 A7 8E 39 4C 07 B2 4C 21 4D " +
            "8F 89 B6 B5 7B C2 10 DA 40 EE 18 79 9F 18 CD 0E " +
            "92 C6 A1 59 3E 84 A6 95 82 80 E6 5B 5B A8 66 0F " +
            "B7 49 E8 CD 89 B8 D2 BF 85 00 A2 5D 86 DB C4 53 " +
            "90 F9 51 23 DC 34 0B 56 E3 84 AF CB B7 A4 AC 5F " +
            "9C BC 23 A8 4C 7F 7C 87 AD A3 15 8E 77 0B CA 4D " +
            "83 25 2F 82 6A 2B FB DE 8B C8 F9 AE DD C8 45 1A " +
            "12 FA 0E FE 72 12 B2 EC CD BB 2E 19 7C E3 58 F8 " +
            "2D 51 29 D1 A8 DB CB B4",
            // After Chi  
            "8E B7 FA 4E 9B 80 0F 50 81 07 56 9F 2C A6 25 CC " +
            "C7 8C 79 24 59 98 1B AC 9C 21 C3 0B 95 EE A8 C1 " +
            "FC 27 EC 71 48 87 97 BE BD 16 EB 64 41 90 5E 9C " +
            "B6 9A E3 29 38 13 28 77 C5 4E 28 88 04 4B 8A 3F " +
            "5D DE 20 94 1B 06 44 37 8C 31 4C 1E 90 4B 01 2E " +
            "1D 89 17 B5 5B 46 32 4B 40 EE 5E 7B DE 30 8D 04 " +
            "A7 8F A9 DD BE 94 36 25 8A 00 F0 6B 29 EA 66 4F " +
            "F7 2F E0 85 0D A0 1F BB E6 04 0C 95 A5 5B 60 5A " +
            "8C C1 51 03 94 6F 5B D6 C2 87 BB CD 84 A4 2E 17 " +
            "9C BC 81 F9 CC AF 78 95 BD 5A 44 AC 2F 2F C1 49 " +
            "93 17 29 D2 48 39 49 3A 46 C9 D9 AF D1 29 0D 0A " +
            "32 BA 0F 3E F2 0A 31 E8 4F 9F 28 1B 3E C3 68 B2 " +
            "25 99 F9 FD 3D 1B CF B4",
            // After Iota 
            "0E 37 FA 4E 9B 80 0F D0 81 07 56 9F 2C A6 25 CC " +
            "C7 8C 79 24 59 98 1B AC 9C 21 C3 0B 95 EE A8 C1 " +
            "FC 27 EC 71 48 87 97 BE BD 16 EB 64 41 90 5E 9C " +
            "B6 9A E3 29 38 13 28 77 C5 4E 28 88 04 4B 8A 3F " +
            "5D DE 20 94 1B 06 44 37 8C 31 4C 1E 90 4B 01 2E " +
            "1D 89 17 B5 5B 46 32 4B 40 EE 5E 7B DE 30 8D 04 " +
            "A7 8F A9 DD BE 94 36 25 8A 00 F0 6B 29 EA 66 4F " +
            "F7 2F E0 85 0D A0 1F BB E6 04 0C 95 A5 5B 60 5A " +
            "8C C1 51 03 94 6F 5B D6 C2 87 BB CD 84 A4 2E 17 " +
            "9C BC 81 F9 CC AF 78 95 BD 5A 44 AC 2F 2F C1 49 " +
            "93 17 29 D2 48 39 49 3A 46 C9 D9 AF D1 29 0D 0A " +
            "32 BA 0F 3E F2 0A 31 E8 4F 9F 28 1B 3E C3 68 B2 " +
            "25 99 F9 FD 3D 1B CF B4",
            // Round #22  
            // After Theta
            "6B 3A C1 37 42 5F 25 C1 F0 5C ED 43 6B 41 1E 38 " +
            "4B 4E 6F 68 7C 87 F9 F2 F6 A5 74 FE 8F B6 1E 25 " +
            "D2 8C 11 D7 C4 81 99 EE D8 1B D0 1D 98 4F 74 8D " +
            "C7 C1 58 F5 7F F4 13 83 49 8C 3E C4 21 54 68 61 " +
            "37 5A 97 61 01 5E F2 D3 A2 9A B1 B8 1C 4D 0F 7E " +
            "78 84 2C CC 82 99 18 5A 31 B5 E5 A7 99 D7 B6 F0 " +
            "2B 4D BF 91 9B 8B D4 7B E0 84 47 9E 33 B2 D0 AB " +
            "D9 84 1D 23 81 A6 11 EB 83 09 37 EC 7C 84 4A 4B " +
            "FD 9A EA DF D3 88 60 22 4E 45 AD 81 A1 BB CC 49 " +
            "F6 38 36 0C D6 F7 CE 71 93 F1 B9 0A A3 29 CF 19 " +
            "F6 1A 12 AB 91 E6 63 2B 37 92 62 73 96 CE 36 FE " +
            "BE 78 19 72 D7 15 D3 B6 25 1B 9F EE 24 9B DE 56 " +
            "0B 32 04 5B B1 1D C1 E4",
            // After Rho  
            "6B 3A C1 37 42 5F 25 C1 E0 B9 DA 87 D6 82 3C 70 " +
            "92 D3 1B 1A DF 61 BE FC 68 EB 51 62 5F 4A E7 FF " +
            "0E CC 74 97 66 8C B8 26 81 F9 44 D7 88 BD 01 DD " +
            "55 FF 47 3F 31 78 1C 8C 58 12 A3 0F 71 08 15 5A " +
            "AD CB B0 00 2F F9 E9 1B F4 E0 27 AA 19 8B CB D1 " +
            "C2 23 64 61 16 CC C4 D0 C2 C7 D4 96 9F 66 5E DB " +
            "8D DC 5C A4 DE 5B 69 FA 64 A1 57 C1 09 8F 3C 67 " +
            "91 40 D3 88 F5 6C C2 8E D8 F9 08 95 96 06 13 6E " +
            "FD 7B 1A 11 4C A4 5F 53 E6 24 A7 A2 D6 C0 D0 5D " +
            "DE 39 CE 1E C7 86 C1 FA 19 93 F1 B9 0A A3 29 CF " +
            "8F AD D8 6B 48 AC 46 9A DF 48 8A CD 59 3A DB F8 " +
            "17 2F 43 EE BA 62 DA D6 1B 9F EE 24 9B DE 56 25 " +
            "30 F9 82 0C C1 56 6C 47",
            // After Pi
            "6B 3A C1 37 42 5F 25 C1 55 FF 47 3F 31 78 1C 8C " +
            "8D DC 5C A4 DE 5B 69 FA DE 39 CE 1E C7 86 C1 FA " +
            "30 F9 82 0C C1 56 6C 47 68 EB 51 62 5F 4A E7 FF " +
            "F4 E0 27 AA 19 8B CB D1 C2 23 64 61 16 CC C4 D0 " +
            "FD 7B 1A 11 4C A4 5F 53 17 2F 43 EE BA 62 DA D6 " +
            "E0 B9 DA 87 D6 82 3C 70 58 12 A3 0F 71 08 15 5A " +
            "64 A1 57 C1 09 8F 3C 67 19 93 F1 B9 0A A3 29 CF " +
            "8F AD D8 6B 48 AC 46 9A 0E CC 74 97 66 8C B8 26 " +
            "81 F9 44 D7 88 BD 01 DD C2 C7 D4 96 9F 66 5E DB " +
            "E6 24 A7 A2 D6 C0 D0 5D 1B 9F EE 24 9B DE 56 25 " +
            "92 D3 1B 1A DF 61 BE FC AD CB B0 00 2F F9 E9 1B " +
            "91 40 D3 88 F5 6C C2 8E D8 F9 08 95 96 06 13 6E " +
            "DF 48 8A CD 59 3A DB F8",
            // After Chi  
            "E3 3A D9 B7 8C 5C 44 B3 07 DE C5 25 30 FC 9C 8C " +
            "AD 1C 5C A4 DE 0B 45 FF 95 3B 8F 2D C5 8F C0 7A " +
            "24 3C 84 04 F0 76 74 4B 6A E8 11 23 59 0E E3 FF " +
            "C9 B8 3D BA 51 AB D0 D2 C0 27 25 8F A4 8E 44 54 " +
            "95 BB 0A 11 09 AC 7A 7A 83 2F 65 66 BA E3 D2 D6 " +
            "C4 18 8E 47 DE 05 14 55 41 00 03 37 73 28 14 D2 " +
            "E2 8D 5F 83 49 83 7A 77 79 83 F3 3D 9C A1 11 AF " +
            "97 AF F9 63 69 A4 47 90 4C CA E4 97 71 CE E6 24 " +
            "A5 D9 67 F7 C8 3D 81 D9 DB 5C 9C 92 96 78 58 FB " +
            "E2 64 B7 31 B2 C0 78 5F 9A AE EE 64 13 EF 57 FC " +
            "82 D3 58 92 0F 65 BC 78 E5 72 B8 15 2D FB F8 7B " +
            "96 40 51 C0 BC 54 0A 1E D8 6A 19 87 10 47 37 6A " +
            "F2 40 2A CD 79 A2 9A FB",
            // After Iota 
            "E2 3A D9 37 8C 5C 44 B3 07 DE C5 25 30 FC 9C 8C " +
            "AD 1C 5C A4 DE 0B 45 FF 95 3B 8F 2D C5 8F C0 7A " +
            "24 3C 84 04 F0 76 74 4B 6A E8 11 23 59 0E E3 FF " +
            "C9 B8 3D BA 51 AB D0 D2 C0 27 25 8F A4 8E 44 54 " +
            "95 BB 0A 11 09 AC 7A 7A 83 2F 65 66 BA E3 D2 D6 " +
            "C4 18 8E 47 DE 05 14 55 41 00 03 37 73 28 14 D2 " +
            "E2 8D 5F 83 49 83 7A 77 79 83 F3 3D 9C A1 11 AF " +
            "97 AF F9 63 69 A4 47 90 4C CA E4 97 71 CE E6 24 " +
            "A5 D9 67 F7 C8 3D 81 D9 DB 5C 9C 92 96 78 58 FB " +
            "E2 64 B7 31 B2 C0 78 5F 9A AE EE 64 13 EF 57 FC " +
            "82 D3 58 92 0F 65 BC 78 E5 72 B8 15 2D FB F8 7B " +
            "96 40 51 C0 BC 54 0A 1E D8 6A 19 87 10 47 37 6A " +
            "F2 40 2A CD 79 A2 9A FB",
            // Round #23  
            // After Theta
            "24 F3 4C 0B 2B 53 2B E5 01 58 E8 86 76 54 27 BB " +
            "E5 CB C8 81 CC B9 AC E4 E7 35 DC 86 4F 5D B1 57 " +
            "63 96 A9 1E E8 8B 43 5A AC 21 84 1F FE 01 8C A9 " +
            "CF 3E 10 19 17 03 6B E5 88 F0 B1 AA B6 3C AD 4F " +
            "E7 B5 59 BA 83 7E 0B 57 C4 85 48 7C A2 1E E5 C7 " +
            "02 D1 1B 7B 79 0A 7B 03 47 86 2E 94 35 80 AF E5 " +
            "AA 5A CB A6 5B 31 93 6C 0B 8D A0 96 16 73 60 82 " +
            "D0 05 D4 79 71 59 70 81 8A 03 71 AB D6 C1 89 72 " +
            "A3 5F 4A 54 8E 95 3A EE 93 8B 08 B7 84 CA B1 E0 " +
            "90 6A E4 9A 38 12 09 72 DD 04 C3 7E 0B 12 60 ED " +
            "44 1A CD AE A8 6A D3 2E E3 F4 95 B6 6B 53 43 4C " +
            "DE 97 C5 E5 AE E6 E3 05 AA 64 4A 2C 9A 95 46 47 " +
            "B5 EA 07 D7 61 5F AD EA",
            // After Rho  
            "24 F3 4C 0B 2B 53 2B E5 03 B0 D0 0D ED A8 4E 76 " +
            "F9 32 72 20 73 2E 2B 79 D4 15 7B 75 5E C3 6D F8 " +
            "5F 1C D2 1A B3 4C F5 40 E1 1F C0 98 CA 1A 42 F8 " +
            "91 71 31 B0 56 FE EC 03 13 22 7C AC AA 2D 4F EB " +
            "DA 2C DD 41 BF 85 AB F3 51 7E 4C 5C 88 C4 27 EA " +
            "10 88 DE D8 CB 53 D8 1B 96 1F 19 BA 50 D6 00 BE " +
            "36 DD 8A 99 64 53 D5 5A E6 C0 04 17 1A 41 2D 2D " +
            "BC B8 2C B8 40 E8 02 EA 56 AD 83 13 E5 14 07 E2 " +
            "89 CA B1 52 C7 7D F4 4B 58 F0 C9 45 84 5B 42 E5 " +
            "22 41 0E 52 8D 5C 13 47 ED DD 04 C3 7E 0B 12 60 " +
            "4D BB 10 69 34 BB A2 AA 8D D3 57 DA AE 4D 0D 31 " +
            "FB B2 B8 DC D5 7C BC C0 64 4A 2C 9A 95 46 47 AA " +
            "AB 7A AD FA C1 75 D8 57",
            // After Pi
            "24 F3 4C 0B 2B 53 2B E5 91 71 31 B0 56 FE EC 03 " +
            "36 DD 8A 99 64 53 D5 5A 22 41 0E 52 8D 5C 13 47 " +
            "AB 7A AD FA C1 75 D8 57 D4 15 7B 75 5E C3 6D F8 " +
            "51 7E 4C 5C 88 C4 27 EA 10 88 DE D8 CB 53 D8 1B " +
            "89 CA B1 52 C7 7D F4 4B FB B2 B8 DC D5 7C BC C0 " +
            "03 B0 D0 0D ED A8 4E 76 13 22 7C AC AA 2D 4F EB " +
            "E6 C0 04 17 1A 41 2D 2D ED DD 04 C3 7E 0B 12 60 " +
            "4D BB 10 69 34 BB A2 AA 5F 1C D2 1A B3 4C F5 40 " +
            "E1 1F C0 98 CA 1A 42 F8 96 1F 19 BA 50 D6 00 BE " +
            "58 F0 C9 45 84 5B 42 E5 64 4A 2C 9A 95 46 47 AA " +
            "F9 32 72 20 73 2E 2B 79 DA 2C DD 41 BF 85 AB F3 " +
            "BC B8 2C B8 40 E8 02 EA 56 AD 83 13 E5 14 07 E2 " +
            "8D D3 57 DA AE 4D 0D 31",
            // After Chi  
            "02 7F C6 02 0B 52 3A BD 91 71 35 F2 DF F2 EE 06 " +
            "BF E7 2B 31 24 72 1D 4A 26 C0 4E 53 A7 5E 30 E7 " +
            "3A 7A 9C 4A 95 D9 1C 55 D4 95 E9 F5 1D D0 B5 E9 " +
            "D8 3C 6D 5E 8C E8 03 AA 62 B8 D6 54 DB 53 D0 9B " +
            "8D CF F2 73 CD FE B5 73 FA D8 BC D4 55 78 BE C2 " +
            "E7 70 D0 1E FD E8 6E 72 1A 3F 7C 6C CE 27 5D AB " +
            "E6 E2 14 3F 1A F1 8D A7 EF DD C4 C7 B7 0B 5E 34 " +
            "5D B9 3C C9 36 BE A3 23 49 1C CB 38 A3 88 F5 46 " +
            "A9 FF 00 DD 4E 13 00 B9 B2 15 3D 20 41 D2 05 B4 " +
            "43 E4 1B 45 A6 53 F2 A5 C4 49 2C 1A DD 54 45 12 " +
            "DD A2 52 98 33 46 2B 71 98 29 5E 42 1A 91 AE F3 " +
            "35 EA 78 70 4A A1 0A FB 26 8D A3 33 B4 36 25 AA " +
            "8F DF DA 9B 22 CC 8D B3",
            // After Iota 
            "0A FF C6 82 0B 52 3A 3D 91 71 35 F2 DF F2 EE 06 " +
            "BF E7 2B 31 24 72 1D 4A 26 C0 4E 53 A7 5E 30 E7 " +
            "3A 7A 9C 4A 95 D9 1C 55 D4 95 E9 F5 1D D0 B5 E9 " +
            "D8 3C 6D 5E 8C E8 03 AA 62 B8 D6 54 DB 53 D0 9B " +
            "8D CF F2 73 CD FE B5 73 FA D8 BC D4 55 78 BE C2 " +
            "E7 70 D0 1E FD E8 6E 72 1A 3F 7C 6C CE 27 5D AB " +
            "E6 E2 14 3F 1A F1 8D A7 EF DD C4 C7 B7 0B 5E 34 " +
            "5D B9 3C C9 36 BE A3 23 49 1C CB 38 A3 88 F5 46 " +
            "A9 FF 00 DD 4E 13 00 B9 B2 15 3D 20 41 D2 05 B4 " +
            "43 E4 1B 45 A6 53 F2 A5 C4 49 2C 1A DD 54 45 12 " +
            "DD A2 52 98 33 46 2B 71 98 29 5E 42 1A 91 AE F3 " +
            "35 EA 78 70 4A A1 0A FB 26 8D A3 33 B4 36 25 AA " +
            "8F DF DA 9B 22 CC 8D B3",
            // Xor'd state (in bytes)
            "0A FF C6 82 0B 52 3A 3D 91 71 35 F2 DF F2 EE 06 " +
            "BF E7 2B 31 24 72 1D 4A 26 C0 4E 53 A7 5E 30 E7 " +
            "3A 7A 9C 4A 95 D9 1C 55 D4 95 E9 F5 1D D0 B5 E9 " +
            "D8 3C 6D 5E 8C E8 03 AA 62 B8 D6 54 DB 53 D0 9B " +
            "8D CF F2 73 CD FE B5 73 FA D8 BC D4 55 78 BE C2 " +
            "E7 70 D0 1E FD E8 6E 72 1A 3F 7C 6C CE 27 5D AB " +
            "E6 E2 14 3F 1A F1 8D A7 EF DD C4 C7 B7 0B 5E 34 " +
            "5D B9 3C C9 36 BE A3 23 49 1C CB 38 A3 88 F5 46 " +
            "A9 FF 00 DD 4E 13 00 B9 B2 15 3D 20 41 D2 05 B4 " +
            "43 E4 1B 45 A6 53 F2 A5 C4 49 2C 1A DD 54 45 12 " +
            "DD A2 52 98 33 46 2B 71 98 29 5E 42 1A 91 AE F3 " +
            "35 EA 78 70 4A A1 0A FB 26 8D A3 33 B4 36 25 AA " +
            "8F DF DA 9B 22 CC 8D B3",
            // Round #0
            // After Theta
            "18 3A D9 EA 90 AA CE B2 44 50 0B 2E 78 11 4E E5 " +
            "9E 35 D0 4D 72 50 1A 59 36 99 37 F4 5A F3 EC F5 " +
            "40 88 91 49 AD 5F 6F D8 C6 50 F6 9D 86 28 41 66 " +
            "0D 1D 53 82 2B 0B A3 49 43 6A 2D 28 8D 71 D7 88 " +
            "9D 96 8B D4 30 53 69 61 80 2A B1 D7 6D FE CD 4F " +
            "F5 B5 CF 76 66 10 9A FD CF 1E 42 B0 69 C4 FD 48 " +
            "C7 30 EF 43 4C D3 8A B4 FF 84 BD 60 4A A6 82 26 " +
            "27 4B 31 CA 0E 38 D0 AE 5B D9 D4 50 38 70 01 C9 " +
            "7C DE 3E 01 E9 F0 A0 5A 93 C7 C6 5C 17 F0 02 A7 " +
            "53 BD 62 E2 5B FE 2E B7 BE BB 21 19 E5 D2 36 9F " +
            "CF 67 4D F0 A8 BE DF FE 4D 08 60 9E BD 72 0E 10 " +
            "14 38 83 0C 1C 83 0D E8 36 D4 DA 94 49 9B F9 B8 " +
            "F5 2D D7 98 1A 4A FE 3E",
            // After Rho  
            "18 3A D9 EA 90 AA CE B2 89 A0 16 5C F0 22 9C CA " +
            "67 0D 74 93 1C 94 46 96 35 CF 5E 6F 93 79 43 AF " +
            "FD 7A C3 06 42 8C 4C 6A 69 88 12 64 66 0C 65 DF " +
            "25 B8 B2 30 9A D4 D0 31 E2 90 5A 0B 4A 63 DC 35 " +
            "CB 45 6A 98 A9 B4 B0 4E DF FC 04 A8 12 7B DD E6 " +
            "AF AF 7D B6 33 83 D0 EC 23 3D 7B 08 C1 A6 11 F7 " +
            "1F 62 9A 56 A4 3D 86 79 4C 05 4D FE 09 7B C1 94 " +
            "65 07 1C 68 D7 93 A5 18 A1 70 E0 02 92 B7 B2 A9 " +
            "27 20 1D 1E 54 8B CF DB 81 D3 C9 63 63 AE 0B 78 " +
            "DF E5 76 AA 57 4C 7C CB 9F BE BB 21 19 E5 D2 36 " +
            "7E FB 3F 9F 35 C1 A3 FA 34 21 80 79 F6 CA 39 40 " +
            "02 67 90 81 63 B0 01 9D D4 DA 94 49 9B F9 B8 36 " +
            "BF 4F 7D CB 35 A6 86 92",
            // After Pi
            "18 3A D9 EA 90 AA CE B2 25 B8 B2 30 9A D4 D0 31 " +
            "1F 62 9A 56 A4 3D 86 79 DF E5 76 AA 57 4C 7C CB " +
            "BF 4F 7D CB 35 A6 86 92 35 CF 5E 6F 93 79 43 AF " +
            "DF FC 04 A8 12 7B DD E6 AF AF 7D B6 33 83 D0 EC " +
            "27 20 1D 1E 54 8B CF DB 02 67 90 81 63 B0 01 9D " +
            "89 A0 16 5C F0 22 9C CA E2 90 5A 0B 4A 63 DC 35 " +
            "4C 05 4D FE 09 7B C1 94 9F BE BB 21 19 E5 D2 36 " +
            "7E FB 3F 9F 35 C1 A3 FA FD 7A C3 06 42 8C 4C 6A " +
            "69 88 12 64 66 0C 65 DF 23 3D 7B 08 C1 A6 11 F7 " +
            "81 D3 C9 63 63 AE 0B 78 D4 DA 94 49 9B F9 B8 36 " +
            "67 0D 74 93 1C 94 46 96 CB 45 6A 98 A9 B4 B0 4E " +
            "65 07 1C 68 D7 93 A5 18 A1 70 E0 02 92 B7 B2 A9 " +
            "34 21 80 79 F6 CA 39 40",
            // After Chi  
            "02 78 D1 AC B4 83 C8 FA E5 3D D6 98 C9 94 A8 B3 " +
            "3F 68 93 17 84 9F 04 69 DF D5 F6 8A D7 44 34 EB " +
            "9A CF 5F DB 3F F2 96 93 15 CC 27 79 B2 F9 43 A7 " +
            "DF FC 04 A0 56 73 D2 F5 AF E8 FD 37 10 B3 D0 E8 " +
            "12 A8 53 70 C4 C2 8D F9 C8 57 90 01 63 B2 9D DD " +
            "85 A5 13 A8 F1 3A 9D 4A 71 2A E8 0A 5A E7 CE 17 " +
            "2C 44 49 60 2D 7B E0 5C 1E BE BB 61 D9 C7 CE 36 " +
            "1C EB 77 9C 3F 80 E3 CF FF 4F AA 0E C3 2E 5C 4A " +
            "E9 4A 92 07 44 04 6F D7 77 35 6F 00 59 F7 A1 F1 " +
            "A8 F3 8A 65 23 AA 4F 30 D4 5A 84 29 BF F9 99 A3 " +
            "43 0F 60 F3 4A 97 43 86 4B 35 8A 9A A9 90 A2 EF " +
            "71 06 1C 11 B3 DB AC 58 E2 7C 94 80 9A A3 F4 3F " +
            "BC 61 8A 71 57 EA 89 08",
            // After Iota 
            "03 78 D1 AC B4 83 C8 FA E5 3D D6 98 C9 94 A8 B3 " +
            "3F 68 93 17 84 9F 04 69 DF D5 F6 8A D7 44 34 EB " +
            "9A CF 5F DB 3F F2 96 93 15 CC 27 79 B2 F9 43 A7 " +
            "DF FC 04 A0 56 73 D2 F5 AF E8 FD 37 10 B3 D0 E8 " +
            "12 A8 53 70 C4 C2 8D F9 C8 57 90 01 63 B2 9D DD " +
            "85 A5 13 A8 F1 3A 9D 4A 71 2A E8 0A 5A E7 CE 17 " +
            "2C 44 49 60 2D 7B E0 5C 1E BE BB 61 D9 C7 CE 36 " +
            "1C EB 77 9C 3F 80 E3 CF FF 4F AA 0E C3 2E 5C 4A " +
            "E9 4A 92 07 44 04 6F D7 77 35 6F 00 59 F7 A1 F1 " +
            "A8 F3 8A 65 23 AA 4F 30 D4 5A 84 29 BF F9 99 A3 " +
            "43 0F 60 F3 4A 97 43 86 4B 35 8A 9A A9 90 A2 EF " +
            "71 06 1C 11 B3 DB AC 58 E2 7C 94 80 9A A3 F4 3F " +
            "BC 61 8A 71 57 EA 89 08",
            // Round #1
            // After Theta
            "F7 19 22 EC 6E 78 C3 02 BE 83 50 BA 11 9B D3 80 " +
            "E4 65 B1 44 4A 9B E5 57 29 B2 CE E6 92 98 FC CA " +
            "5C 21 01 A5 B1 48 49 0E E1 AD D4 39 68 02 48 5F " +
            "84 42 82 82 8E 7C A9 C6 74 E5 DF 64 DE B7 31 D6 " +
            "E4 CF 6B 1C 81 1E 45 D8 0E B9 CE 7F ED 08 42 40 " +
            "71 C4 E0 E8 2B C1 96 B2 2A 94 6E 28 82 E8 B5 24 " +
            "F7 49 6B 33 E3 7F 01 62 E8 D9 83 0D 9C 1B 06 17 " +
            "DA 05 29 E2 B1 3A 3C 52 0B 2E 59 4E 19 D5 57 B2 " +
            "B2 F4 14 25 9C 0B 14 E4 AC 38 4D 53 97 F3 40 CF " +
            "5E 94 B2 09 66 76 87 11 12 B4 DA 57 31 43 46 3E " +
            "B7 6E 93 B3 90 6C 48 7E 10 8B 0C B8 71 9F D9 DC " +
            "AA 0B 3E 42 7D DF 4D 66 14 1B AC EC DF 7F 3C 1E " +
            "7A 8F D4 0F D9 50 56 95",
            // After Rho  
            "F7 19 22 EC 6E 78 C3 02 7D 07 A1 74 23 36 A7 01 " +
            "79 59 2C 91 D2 66 F9 15 89 C9 AF 9C 22 EB 6C 2E " +
            "45 4A 72 E0 0A 09 28 8D 83 26 80 F4 15 DE 4A 9D " +
            "28 E8 C8 97 6A 4C 28 24 35 5D F9 37 99 F7 6D 8C " +
            "E7 35 8E 40 8F 22 6C F2 20 04 E4 90 EB FC D7 8E " +
            "8D 23 06 47 5F 09 B6 94 92 A8 50 BA A1 08 A2 D7 " +
            "9B 19 FF 0B 10 BB 4F 5A 37 0C 2E D0 B3 07 1B 38 " +
            "F1 58 1D 1E 29 ED 82 14 9C 32 AA AF 64 17 5C B2 " +
            "A2 84 73 81 82 5C 96 9E A0 67 56 9C A6 A9 CB 79 " +
            "EE 30 C2 8B 52 36 C1 CC 3E 12 B4 DA 57 31 43 46 " +
            "21 F9 DD BA 4D CE 42 B2 43 2C 32 E0 C6 7D 66 73 " +
            "75 C1 47 A8 EF BB C9 4C 1B AC EC DF 7F 3C 1E 14 " +
            "55 A5 DE 23 F5 43 36 94",
            // After Pi
            "F7 19 22 EC 6E 78 C3 02 28 E8 C8 97 6A 4C 28 24 " +
            "9B 19 FF 0B 10 BB 4F 5A EE 30 C2 8B 52 36 C1 CC " +
            "55 A5 DE 23 F5 43 36 94 89 C9 AF 9C 22 EB 6C 2E " +
            "20 04 E4 90 EB FC D7 8E 8D 23 06 47 5F 09 B6 94 " +
            "A2 84 73 81 82 5C 96 9E 75 C1 47 A8 EF BB C9 4C " +
            "7D 07 A1 74 23 36 A7 01 35 5D F9 37 99 F7 6D 8C " +
            "37 0C 2E D0 B3 07 1B 38 3E 12 B4 DA 57 31 43 46 " +
            "21 F9 DD BA 4D CE 42 B2 45 4A 72 E0 0A 09 28 8D " +
            "83 26 80 F4 15 DE 4A 9D 92 A8 50 BA A1 08 A2 D7 " +
            "A0 67 56 9C A6 A9 CB 79 1B AC EC DF 7F 3C 1E 14 " +
            "79 59 2C 91 D2 66 F9 15 E7 35 8E 40 8F 22 6C F2 " +
            "F1 58 1D 1E 29 ED 82 14 9C 32 AA AF 64 17 5C B2 " +
            "43 2C 32 E0 C6 7D 66 73",
            // After Chi  
            "64 08 15 E4 7E CB 84 58 4C C8 C8 17 28 48 A8 A0 " +
            "8A 9C E3 2B B5 FA 79 4A 4C 28 E2 47 58 0E 00 CE " +
            "5D 45 16 30 F5 47 1E B0 04 EA AD DB 36 EA 4C 3E " +
            "02 80 95 10 6B A8 D7 84 D8 62 02 6F 32 AA FF D4 " +
            "2A 8C DB 95 82 1C B2 BC 55 C5 07 A8 26 AF 5A CC " +
            "7F 07 A7 B4 01 36 B5 31 3D 4F 69 3D DD C7 2D CA " +
            "36 E5 67 F0 BB C9 1B 88 62 14 94 9E 75 01 E6 47 " +
            "21 A1 85 B9 D5 0F 0A 3E 55 C2 22 EA AA 09 88 CF " +
            "A3 61 86 F0 13 7F 03 B5 89 20 F8 F9 F8 1C B6 D3 " +
            "E4 25 44 BC A6 A8 EB F0 99 88 6C CB 6A EA 5C 04 " +
            "69 11 3D 8F F2 AB 7B 11 EB 17 2C E1 CB 30 30 50 " +
            "B2 54 0D 5E AB 85 A0 55 A4 63 A6 BE 74 15 C5 B6 " +
            "C5 08 B0 A0 CB 7D 62 91",
            // After Iota 
            "E6 88 15 E4 7E CB 84 58 4C C8 C8 17 28 48 A8 A0 " +
            "8A 9C E3 2B B5 FA 79 4A 4C 28 E2 47 58 0E 00 CE " +
            "5D 45 16 30 F5 47 1E B0 04 EA AD DB 36 EA 4C 3E " +
            "02 80 95 10 6B A8 D7 84 D8 62 02 6F 32 AA FF D4 " +
            "2A 8C DB 95 82 1C B2 BC 55 C5 07 A8 26 AF 5A CC " +
            "7F 07 A7 B4 01 36 B5 31 3D 4F 69 3D DD C7 2D CA " +
            "36 E5 67 F0 BB C9 1B 88 62 14 94 9E 75 01 E6 47 " +
            "21 A1 85 B9 D5 0F 0A 3E 55 C2 22 EA AA 09 88 CF " +
            "A3 61 86 F0 13 7F 03 B5 89 20 F8 F9 F8 1C B6 D3 " +
            "E4 25 44 BC A6 A8 EB F0 99 88 6C CB 6A EA 5C 04 " +
            "69 11 3D 8F F2 AB 7B 11 EB 17 2C E1 CB 30 30 50 " +
            "B2 54 0D 5E AB 85 A0 55 A4 63 A6 BE 74 15 C5 B6 " +
            "C5 08 B0 A0 CB 7D 62 91",
            // Round #2
            // After Theta
            "E5 CB 61 F9 55 6B 36 99 52 A0 2E DF E7 FD 30 08 " +
            "39 01 E2 9C 09 CE ED A7 F8 05 00 C0 79 EF 6B F0 " +
            "5A DE 58 A2 AB 83 79 D0 07 A9 D9 C6 1D 4A FE FF " +
            "1C E8 73 D8 A4 1D 4F 2C 6B FF 03 D8 8E 9E 6B 39 " +
            "9E A1 39 12 A3 FD D9 82 52 5E 49 3A 78 6B 3D AC " +
            "7C 44 D3 A9 2A 96 07 F0 23 27 8F F5 12 72 B5 62 " +
            "85 78 66 47 07 FD 8F 65 D6 39 76 19 54 E0 8D 79 " +
            "26 3A CB 2B 8B CB 6D 5E 56 81 56 F7 81 A9 3A 0E " +
            "BD 09 60 38 DC CA 9B 1D 3A BD F9 4E 44 28 22 3E " +
            "50 08 A6 3B 87 49 80 CE 9E 13 22 59 34 2E 3B 64 " +
            "6A 52 49 92 D9 0B C9 D0 F5 7F CA 29 04 85 A8 F8 " +
            "01 C9 0C E9 17 B1 34 B8 10 4E 44 39 55 F4 AE 88 " +
            "C2 93 FE 32 95 B9 05 F1",
            // After Rho  
            "E5 CB 61 F9 55 6B 36 99 A4 40 5D BE CF FB 61 10 " +
            "4E 80 38 67 82 73 FB 69 F7 BE 06 8F 5F 00 00 9C " +
            "1D CC 83 D6 F2 C6 12 5D DC A1 E4 FF 7F 90 9A 6D " +
            "87 4D DA F1 C4 C2 81 3E CE DA FF 00 B6 A3 E7 5A " +
            "D0 1C 89 D1 FE 6C 41 CF D6 C3 2A E5 95 A4 83 B7 " +
            "E7 23 9A 4E 55 B1 3C 80 8A 8D 9C 3C D6 4B C8 D5 " +
            "3B 3A E8 7F 2C 2B C4 33 C0 1B F3 AC 73 EC 32 A8 " +
            "95 C5 E5 36 2F 13 9D E5 EE 03 53 75 1C AC 02 AD " +
            "0C 87 5B 79 B3 A3 37 01 11 1F 9D DE 7C 27 22 14 " +
            "09 D0 19 0A C1 74 E7 30 64 9E 13 22 59 34 2E 3B " +
            "24 43 AB 49 25 49 66 2F D7 FF 29 A7 10 14 A2 E2 " +
            "20 99 21 FD 22 96 06 37 4E 44 39 55 F4 AE 88 10 " +
            "41 BC F0 A4 BF 4C 65 6E",
            // After Pi
            "E5 CB 61 F9 55 6B 36 99 87 4D DA F1 C4 C2 81 3E " +
            "3B 3A E8 7F 2C 2B C4 33 09 D0 19 0A C1 74 E7 30 " +
            "41 BC F0 A4 BF 4C 65 6E F7 BE 06 8F 5F 00 00 9C " +
            "D6 C3 2A E5 95 A4 83 B7 E7 23 9A 4E 55 B1 3C 80 " +
            "0C 87 5B 79 B3 A3 37 01 20 99 21 FD 22 96 06 37 " +
            "A4 40 5D BE CF FB 61 10 CE DA FF 00 B6 A3 E7 5A " +
            "C0 1B F3 AC 73 EC 32 A8 64 9E 13 22 59 34 2E 3B " +
            "24 43 AB 49 25 49 66 2F 1D CC 83 D6 F2 C6 12 5D " +
            "DC A1 E4 FF 7F 90 9A 6D 8A 8D 9C 3C D6 4B C8 D5 " +
            "11 1F 9D DE 7C 27 22 14 4E 44 39 55 F4 AE 88 10 " +
            "4E 80 38 67 82 73 FB 69 D0 1C 89 D1 FE 6C 41 CF " +
            "95 C5 E5 36 2F 13 9D E5 EE 03 53 75 1C AC 02 AD " +
            "D7 FF 29 A7 10 14 A2 E2",
            // After Chi  
            "DD F9 41 F7 7D 42 72 98 87 8D CB F1 05 96 A2 3E " +
            "7B 16 08 DB 12 23 C4 7D AD 93 18 53 81 57 F5 A1 " +
            "43 B8 6A A4 3F CC E4 48 D6 9E 96 85 1F 11 3C 9C " +
            "DE 47 6B D4 37 A6 80 B6 C7 3B BA CA 55 A5 3C B6 " +
            "DB A1 5D 7B EE A3 37 89 20 D8 09 9D A2 32 85 14 " +
            "A4 41 5D 12 8E B7 71 B0 EA 5E FF 02 BE B3 EB 49 " +
            "C0 5A 5B E5 57 A5 72 AC E4 9E 47 94 93 86 2F 2B " +
            "6E D9 09 49 15 49 E0 65 1F C0 9B D6 72 8D 52 CD " +
            "CD B3 E5 3D 57 B4 B8 6D C4 CD BC 3D 56 C3 40 D5 " +
            "00 97 1F 5C 7E 67 30 59 8E 65 5D 7C F9 BE 00 30 " +
            "4B 41 5C 41 83 60 67 49 BA 1E 9B 90 EE C0 43 C7 " +
            "84 39 CD B4 2F 03 3D A7 E6 03 43 35 9E CF 5B A4 " +
            "47 E3 A8 37 6C 18 A2 64",
            // After Iota 
            "57 79 41 F7 7D 42 72 18 87 8D CB F1 05 96 A2 3E " +
            "7B 16 08 DB 12 23 C4 7D AD 93 18 53 81 57 F5 A1 " +
            "43 B8 6A A4 3F CC E4 48 D6 9E 96 85 1F 11 3C 9C " +
            "DE 47 6B D4 37 A6 80 B6 C7 3B BA CA 55 A5 3C B6 " +
            "DB A1 5D 7B EE A3 37 89 20 D8 09 9D A2 32 85 14 " +
            "A4 41 5D 12 8E B7 71 B0 EA 5E FF 02 BE B3 EB 49 " +
            "C0 5A 5B E5 57 A5 72 AC E4 9E 47 94 93 86 2F 2B " +
            "6E D9 09 49 15 49 E0 65 1F C0 9B D6 72 8D 52 CD " +
            "CD B3 E5 3D 57 B4 B8 6D C4 CD BC 3D 56 C3 40 D5 " +
            "00 97 1F 5C 7E 67 30 59 8E 65 5D 7C F9 BE 00 30 " +
            "4B 41 5C 41 83 60 67 49 BA 1E 9B 90 EE C0 43 C7 " +
            "84 39 CD B4 2F 03 3D A7 E6 03 43 35 9E CF 5B A4 " +
            "47 E3 A8 37 6C 18 A2 64",
            // Round #3
            // After Theta
            "1B 35 9C D8 0B BD 34 A3 8E AC B7 FD CA 59 47 A5 " +
            "56 5F 95 FB 1E 60 FB EB 19 6F BE 59 D2 96 44 6E " +
            "D4 CE AE 9F 18 04 76 D6 9A D2 4B AA 69 EE 7A 27 " +
            "D7 66 17 D8 F8 69 65 2D EA 72 27 EA 59 E6 03 20 " +
            "6F 5D FB 71 BD 62 86 46 B7 AE CD A6 85 FA 17 8A " +
            "E8 0D 80 3D F8 48 37 0B E3 7F 83 0E 71 7C 0E D2 " +
            "ED 13 C6 C5 5B E6 4D 3A 50 62 E1 9E C0 47 9E E4 " +
            "F9 AF CD 72 32 81 72 FB 53 8C 46 F9 04 72 14 76 " +
            "C4 92 99 31 98 7B 5D F6 E9 84 21 1D 5A 80 7F 43 " +
            "B4 6B B9 56 2D A6 81 96 19 13 99 47 DE 76 92 AE " +
            "07 0D 81 6E F5 9F 21 F2 B3 3F E7 9C 21 0F A6 5C " +
            "A9 70 50 94 23 40 02 31 52 FF E5 3F CD 0E EA 6B " +
            "D0 95 6C 0C 4B D0 30 FA",
            // After Rho  
            "1B 35 9C D8 0B BD 34 A3 1D 59 6F FB 95 B3 8E 4A " +
            "D5 57 E5 BE 07 D8 FE BA 6D 49 E4 96 F1 E6 9B 25 " +
            "20 B0 B3 A6 76 76 FD C4 9A E6 AE 77 A2 29 BD A4 " +
            "81 8D 9F 56 D6 72 6D 76 88 BA DC 89 7A 96 F9 00 " +
            "AE FD B8 5E 31 43 A3 B7 7F A1 78 EB DA 6C 5A A8 " +
            "40 6F 00 EC C1 47 BA 59 48 8F FF 0D 3A C4 F1 39 " +
            "2E DE 32 6F D2 69 9F 30 8F 3C C9 A1 C4 C2 3D 81 " +
            "39 99 40 B9 FD FC D7 66 F2 09 E4 28 EC A6 18 8D " +
            "33 06 73 AF CB 9E 58 32 BF A1 74 C2 90 0E 2D C0 " +
            "34 D0 92 76 2D D7 AA C5 AE 19 13 99 47 DE 76 92 " +
            "86 C8 1F 34 04 BA D5 7F CD FE 9C 73 86 3C 98 72 " +
            "15 0E 8A 72 04 48 20 26 FF E5 3F CD 0E EA 6B 52 " +
            "8C 3E 74 25 1B C3 12 34",
            // After Pi
            "1B 35 9C D8 0B BD 34 A3 81 8D 9F 56 D6 72 6D 76 " +
            "2E DE 32 6F D2 69 9F 30 34 D0 92 76 2D D7 AA C5 " +
            "8C 3E 74 25 1B C3 12 34 6D 49 E4 96 F1 E6 9B 25 " +
            "7F A1 78 EB DA 6C 5A A8 40 6F 00 EC C1 47 BA 59 " +
            "33 06 73 AF CB 9E 58 32 15 0E 8A 72 04 48 20 26 " +
            "1D 59 6F FB 95 B3 8E 4A 88 BA DC 89 7A 96 F9 00 " +
            "8F 3C C9 A1 C4 C2 3D 81 AE 19 13 99 47 DE 76 92 " +
            "86 C8 1F 34 04 BA D5 7F 20 B0 B3 A6 76 76 FD C4 " +
            "9A E6 AE 77 A2 29 BD A4 48 8F FF 0D 3A C4 F1 39 " +
            "BF A1 74 C2 90 0E 2D C0 FF E5 3F CD 0E EA 6B 52 " +
            "D5 57 E5 BE 07 D8 FE BA AE FD B8 5E 31 43 A3 B7 " +
            "39 99 40 B9 FD FC D7 66 F2 09 E4 28 EC A6 18 8D " +
            "CD FE 9C 73 86 3C 98 72",
            // After Chi  
            "35 67 BC F1 0B B4 A6 A3 91 8D 1F 46 FB E4 4D B3 " +
            "A6 F0 56 6E C0 69 8F 00 27 D1 1A AE 2D EB 8E 46 " +
            "0C B6 77 23 CF 81 5B 60 6D 07 E4 92 F0 E5 3B 74 " +
            "4C A1 0B E8 D0 F4 1A 8A 44 67 88 BC C5 07 9A 5D " +
            "5B 47 17 2B 3A 38 C3 33 07 AE 92 1B 0E 40 60 AE " +
            "1A 5D 6E DB 11 F3 8A CB A8 BB CE 91 79 8A BB 12 " +
            "8F FC C5 85 C4 E2 BC EC B7 08 73 52 D6 DF 7C 92 " +
            "06 6A 8F 34 6E BE A4 7F 60 B9 E2 AE 6E B2 BD DD " +
            "2D C6 AE B5 22 23 B1 64 08 CB F4 00 34 24 B3 2B " +
            "BF B1 F4 E0 E0 1A B9 44 65 A3 33 9C 8E E3 6B 72 " +
            "C4 57 A5 1F CB 64 AA FA 6C FD 1C 5E 31 41 AB 3E " +
            "34 6F 58 EA FF E4 57 14 E2 08 85 A4 ED 66 7E 05 " +
            "E7 56 84 33 B6 3F 99 77",
            // After Iota 
            "35 E7 BC 71 0B B4 A6 23 91 8D 1F 46 FB E4 4D B3 " +
            "A6 F0 56 6E C0 69 8F 00 27 D1 1A AE 2D EB 8E 46 " +
            "0C B6 77 23 CF 81 5B 60 6D 07 E4 92 F0 E5 3B 74 " +
            "4C A1 0B E8 D0 F4 1A 8A 44 67 88 BC C5 07 9A 5D " +
            "5B 47 17 2B 3A 38 C3 33 07 AE 92 1B 0E 40 60 AE " +
            "1A 5D 6E DB 11 F3 8A CB A8 BB CE 91 79 8A BB 12 " +
            "8F FC C5 85 C4 E2 BC EC B7 08 73 52 D6 DF 7C 92 " +
            "06 6A 8F 34 6E BE A4 7F 60 B9 E2 AE 6E B2 BD DD " +
            "2D C6 AE B5 22 23 B1 64 08 CB F4 00 34 24 B3 2B " +
            "BF B1 F4 E0 E0 1A B9 44 65 A3 33 9C 8E E3 6B 72 " +
            "C4 57 A5 1F CB 64 AA FA 6C FD 1C 5E 31 41 AB 3E " +
            "34 6F 58 EA FF E4 57 14 E2 08 85 A4 ED 66 7E 05 " +
            "E7 56 84 33 B6 3F 99 77",
            // Round #4
            // After Theta
            "D2 38 B0 7A 1F E7 26 74 D4 40 01 B4 A1 08 D7 14 " +
            "BF 13 20 9C 18 70 95 3C 69 11 16 54 08 E0 18 A0 " +
            "57 36 9A A2 9C 19 AD B0 8A D8 E8 99 E4 B6 BB 23 " +
            "09 6C 15 1A 8A 18 80 2D 5D 84 FE 4E 1D 1E 80 61 " +
            "15 87 1B D1 1F 33 55 D5 5C 2E 7F 9A 5D D8 96 7E " +
            "FD 82 62 D0 05 A0 0A 9C ED 76 D0 63 23 66 21 B5 " +
            "96 1F B3 77 1C FB A6 D0 F9 C8 7F A8 F3 D4 EA 74 " +
            "5D EA 62 B5 3D 26 52 AF 87 66 EE A5 7A E1 3D 8A " +
            "68 0B B0 47 78 CF 2B C3 11 28 82 F2 EC 3D A9 17 " +
            "F1 71 F8 1A C5 11 2F A2 3E 23 DE 1D DD 7B 9D A2 " +
            "23 88 A9 14 DF 37 2A AD 29 30 02 AC 6B AD 31 99 " +
            "2D 8C 2E 18 27 FD 4D 28 AC C8 89 5E C8 6D E8 E3 " +
            "BC D6 69 B2 E5 A7 6F A7",
            // After Rho  
            "D2 38 B0 7A 1F E7 26 74 A8 81 02 68 43 11 AE 29 " +
            "EF 04 08 27 06 5C 25 CF 00 8E 01 9A 16 61 41 85 " +
            "CC 68 85 BD B2 D1 14 E5 49 6E BB 3B A2 88 8D 9E " +
            "A1 A1 88 01 D8 92 C0 56 58 17 A1 BF 53 87 07 60 " +
            "C3 8D E8 8F 99 AA EA 8A 6D E9 C7 E5 F2 A7 D9 85 " +
            "EC 17 14 83 2E 00 55 E0 D4 B6 DB 41 8F 8D 98 85 " +
            "BD E3 D8 37 85 B6 FC 98 A9 D5 E9 F2 91 FF 50 E7 " +
            "DA 1E 13 A9 D7 2E 75 B1 4B F5 C2 7B 14 0F CD DC " +
            "F6 08 EF 79 65 18 6D 01 D4 8B 08 14 41 79 F6 9E " +
            "E2 45 34 3E 0E 5F A3 38 A2 3E 23 DE 1D DD 7B 9D " +
            "A8 B4 8E 20 A6 52 7C DF A6 C0 08 B0 AE B5 C6 64 " +
            "85 D1 05 E3 A4 BF 09 A5 C8 89 5E C8 6D E8 E3 AC " +
            "DB 29 AF 75 9A 6C F9 E9",
            // After Pi
            "D2 38 B0 7A 1F E7 26 74 A1 A1 88 01 D8 92 C0 56 " +
            "BD E3 D8 37 85 B6 FC 98 E2 45 34 3E 0E 5F A3 38 " +
            "DB 29 AF 75 9A 6C F9 E9 00 8E 01 9A 16 61 41 85 " +
            "6D E9 C7 E5 F2 A7 D9 85 EC 17 14 83 2E 00 55 E0 " +
            "F6 08 EF 79 65 18 6D 01 85 D1 05 E3 A4 BF 09 A5 " +
            "A8 81 02 68 43 11 AE 29 58 17 A1 BF 53 87 07 60 " +
            "A9 D5 E9 F2 91 FF 50 E7 A2 3E 23 DE 1D DD 7B 9D " +
            "A8 B4 8E 20 A6 52 7C DF CC 68 85 BD B2 D1 14 E5 " +
            "49 6E BB 3B A2 88 8D 9E D4 B6 DB 41 8F 8D 98 85 " +
            "D4 8B 08 14 41 79 F6 9E C8 89 5E C8 6D E8 E3 AC " +
            "EF 04 08 27 06 5C 25 CF C3 8D E8 8F 99 AA EA 8A " +
            "DA 1E 13 A9 D7 2E 75 B1 4B F5 C2 7B 14 0F CD DC " +
            "A6 C0 08 B0 AE B5 C6 64",
            // After Chi  
            "CE 7A E0 4C 1A C3 1A FC E3 A5 AC 09 D2 DB C3 76 " +
            "A4 CB 53 76 15 96 A4 59 E2 55 24 34 0B DC A5 2C " +
            "FA A8 A7 74 5A 7C 39 EB 80 98 11 98 1A 61 45 E5 " +
            "7F E1 2C 9D B3 BF F1 84 ED C6 14 01 AE A7 55 44 " +
            "F6 06 EF 61 77 58 2D 01 E8 B0 C3 86 44 39 91 A5 " +
            "09 41 4A 28 C3 69 FE AE 5A 3D A3 B3 5F 87 2C 78 " +
            "A1 55 65 D2 33 FD 54 A5 A2 3F 23 96 5C DC F9 BD " +
            "F8 A2 2F B7 B6 D4 7D 9F 58 F8 C5 FD BF D4 04 E4 " +
            "49 67 BB 2F E2 F8 EB 84 DC B6 8D 89 A3 0D 99 A5 " +
            "D0 EB 89 21 D3 68 E2 DF C9 8F 64 CA 6D E0 6A B6 " +
            "F7 16 1B 07 40 58 30 FE C2 6C 28 DD 99 AB 62 C6 " +
            "7E 1E 1B 29 7D 9E 77 91 02 F1 C2 7C 14 47 EC 57 " +
            "A6 49 E8 38 37 17 0C 64",
            // After Iota 
            "45 FA E0 4C 1A C3 1A FC E3 A5 AC 09 D2 DB C3 76 " +
            "A4 CB 53 76 15 96 A4 59 E2 55 24 34 0B DC A5 2C " +
            "FA A8 A7 74 5A 7C 39 EB 80 98 11 98 1A 61 45 E5 " +
            "7F E1 2C 9D B3 BF F1 84 ED C6 14 01 AE A7 55 44 " +
            "F6 06 EF 61 77 58 2D 01 E8 B0 C3 86 44 39 91 A5 " +
            "09 41 4A 28 C3 69 FE AE 5A 3D A3 B3 5F 87 2C 78 " +
            "A1 55 65 D2 33 FD 54 A5 A2 3F 23 96 5C DC F9 BD " +
            "F8 A2 2F B7 B6 D4 7D 9F 58 F8 C5 FD BF D4 04 E4 " +
            "49 67 BB 2F E2 F8 EB 84 DC B6 8D 89 A3 0D 99 A5 " +
            "D0 EB 89 21 D3 68 E2 DF C9 8F 64 CA 6D E0 6A B6 " +
            "F7 16 1B 07 40 58 30 FE C2 6C 28 DD 99 AB 62 C6 " +
            "7E 1E 1B 29 7D 9E 77 91 02 F1 C2 7C 14 47 EC 57 " +
            "A6 49 E8 38 37 17 0C 64",
            // Round #5
            // After Theta
            "5B 62 47 50 63 C5 86 6E 15 88 A0 04 42 22 C0 C3 " +
            "21 55 A5 9E 9F C9 CD A1 A2 5C 1E 5E B8 4E 88 A7 " +
            "59 44 CF E6 C5 85 6C A8 9E 00 B6 84 63 67 D9 77 " +
            "89 CC 20 90 23 46 F2 31 68 58 E2 E9 24 F8 3C BC " +
            "B6 0F D5 0B C4 CA 00 8A 4B 5C AB 14 DB C0 C4 E6 " +
            "17 D9 ED 34 BA 6F 62 3C AC 10 AF BE CF 7E 2F CD " +
            "24 CB 93 3A B9 A2 3D 5D E2 36 19 FC EF 4E D4 36 " +
            "5B 4E 47 25 29 2D 28 DC 46 60 62 E1 C6 D2 98 76 " +
            "BF 4A B7 22 72 01 E8 31 59 28 7B 61 29 52 F0 5D " +
            "90 E2 B3 4B 60 FA CF 54 6A 63 0C 58 F2 19 3F F5 " +
            "E9 8E BC 1B 39 5E AC 6C 34 41 24 D0 09 52 61 73 " +
            "FB 80 ED C1 F7 C1 1E 69 42 F8 F8 16 A7 D5 C1 DC " +
            "05 A5 80 AA A8 EE 59 27",
            // After Rho  
            "5B 62 47 50 63 C5 86 6E 2B 10 41 09 84 44 80 87 " +
            "48 55 A9 E7 67 72 73 68 EB 84 78 2A CA E5 E1 85 " +
            "2E 64 43 CD 22 7A 36 2F 38 76 96 7D E7 09 60 4B " +
            "02 39 62 24 1F 93 C8 0C 2F 1A 96 78 3A 09 3E 0F " +
            "87 EA 05 62 65 00 45 DB 4C 6C BE C4 B5 4A B1 0D " +
            "B9 C8 6E A7 D1 7D 13 E3 34 B3 42 BC FA 3E FB BD " +
            "D4 C9 15 ED E9 22 59 9E 9D A8 6D C4 6D 32 F8 DF " +
            "92 94 16 14 EE 2D A7 A3 C2 8D A5 31 ED 8C C0 C4 " +
            "56 44 2E 00 3D E6 57 E9 F8 AE 2C 94 BD B0 14 29 " +
            "FF 99 0A 52 7C 76 09 4C F5 6A 63 0C 58 F2 19 3F " +
            "B1 B2 A5 3B F2 6E E4 78 D1 04 91 40 27 48 85 CD " +
            "1F B0 3D F8 3E D8 23 6D F8 F8 16 A7 D5 C1 DC 42 " +
            "D6 49 41 29 A0 2A AA 7B",
            // After Pi
            "5B 62 47 50 63 C5 86 6E 02 39 62 24 1F 93 C8 0C " +
            "D4 C9 15 ED E9 22 59 9E FF 99 0A 52 7C 76 09 4C " +
            "D6 49 41 29 A0 2A AA 7B EB 84 78 2A CA E5 E1 85 " +
            "4C 6C BE C4 B5 4A B1 0D B9 C8 6E A7 D1 7D 13 E3 " +
            "56 44 2E 00 3D E6 57 E9 1F B0 3D F8 3E D8 23 6D " +
            "2B 10 41 09 84 44 80 87 2F 1A 96 78 3A 09 3E 0F " +
            "9D A8 6D C4 6D 32 F8 DF F5 6A 63 0C 58 F2 19 3F " +
            "B1 B2 A5 3B F2 6E E4 78 2E 64 43 CD 22 7A 36 2F " +
            "38 76 96 7D E7 09 60 4B 34 B3 42 BC FA 3E FB BD " +
            "F8 AE 2C 94 BD B0 14 29 F8 F8 16 A7 D5 C1 DC 42 " +
            "48 55 A9 E7 67 72 73 68 87 EA 05 62 65 00 45 DB " +
            "92 94 16 14 EE 2D A7 A3 C2 8D A5 31 ED 8C C0 C4 " +
            "D1 04 91 40 27 48 85 CD",
            // After Chi  
            "8F A2 52 99 83 E5 97 FC 29 29 68 36 0B C7 C8 4C " +
            "D4 89 54 C4 69 2A FB AD F6 BB 0C 02 3F B3 0D 48 " +
            "D6 50 61 0D BC 38 E2 7B 5A 04 38 09 8A D0 E3 67 " +
            "0A 68 BE C4 99 C8 F5 05 B0 78 7F 5F D3 65 33 E7 " +
            "B6 40 6E 02 FD C3 97 69 1B D8 BB 3C 0B D2 33 65 " +
            "BB B0 28 8D C1 76 40 57 4F 58 94 70 2A C9 3F 2F " +
            "9D 38 E9 F7 CF 3E 1C 9F FF 6A 23 0C 5C F2 19 B8 " +
            "B5 B8 33 4B C8 67 DA 70 2A E5 03 4D 3A 4C AD 9B " +
            "F0 7A BA 7D E2 89 64 4B 34 E3 50 9F BA 7F 33 FF " +
            "FE AA 6D DC 9F 8A 36 04 E8 EA 82 97 10 C0 9C 02 " +
            "58 41 BB F3 ED 5F D1 48 C7 E3 A4 43 64 80 05 9F " +
            "83 94 06 54 EC 6D A2 AA CA DC 8D 96 AD BE B2 E4 " +
            "56 AE 95 40 27 48 81 5E",
            // After Iota 
            "8E A2 52 19 83 E5 97 FC 29 29 68 36 0B C7 C8 4C " +
            "D4 89 54 C4 69 2A FB AD F6 BB 0C 02 3F B3 0D 48 " +
            "D6 50 61 0D BC 38 E2 7B 5A 04 38 09 8A D0 E3 67 " +
            "0A 68 BE C4 99 C8 F5 05 B0 78 7F 5F D3 65 33 E7 " +
            "B6 40 6E 02 FD C3 97 69 1B D8 BB 3C 0B D2 33 65 " +
            "BB B0 28 8D C1 76 40 57 4F 58 94 70 2A C9 3F 2F " +
            "9D 38 E9 F7 CF 3E 1C 9F FF 6A 23 0C 5C F2 19 B8 " +
            "B5 B8 33 4B C8 67 DA 70 2A E5 03 4D 3A 4C AD 9B " +
            "F0 7A BA 7D E2 89 64 4B 34 E3 50 9F BA 7F 33 FF " +
            "FE AA 6D DC 9F 8A 36 04 E8 EA 82 97 10 C0 9C 02 " +
            "58 41 BB F3 ED 5F D1 48 C7 E3 A4 43 64 80 05 9F " +
            "83 94 06 54 EC 6D A2 AA CA DC 8D 96 AD BE B2 E4 " +
            "56 AE 95 40 27 48 81 5E",
            // Round #6
            // After Theta
            "FF D6 15 CC B6 7E 46 AA A9 E7 BB 5A 53 51 0A 53 " +
            "99 C6 4B F5 0F 88 97 ED 34 EC 64 FE 8D DA 64 AC " +
            "67 D3 35 0C 2E 2E 75 3C 2B 70 7F DC BF 4B 32 31 " +
            "8A A6 6D A8 C1 5E 37 1A FD 37 60 6E B5 C7 5F A7 " +
            "74 17 06 FE 4F AA FE 8D AA 5B EF 3D 99 C4 A4 22 " +
            "CA C4 6F 58 F4 ED 91 01 CF 96 47 1C 72 5F FD 30 " +
            "D0 77 F6 C6 A9 9C 70 DF 3D 3D 4B F0 EE 9B 70 5C " +
            "04 3B 67 4A 5A 71 4D 37 5B 91 44 98 0F D7 7C CD " +
            "70 B4 69 11 BA 1F A6 54 79 AC 4F AE DC DD 5F BF " +
            "3C FD 05 20 2D E3 5F E0 59 69 D6 96 82 D6 0B 45 " +
            "29 35 FC 26 D8 C4 00 1E 47 2D 77 2F 3C 16 C7 80 " +
            "CE DB 19 65 8A CF CE EA 08 8B E5 6A 1F D7 DB 00 " +
            "E7 2D C1 41 B5 5E 16 19",
            // After Rho  
            "FF D6 15 CC B6 7E 46 AA 52 CF 77 B5 A6 A2 14 A6 " +
            "A6 F1 52 FD 03 E2 65 7B A8 4D C6 4A C3 4E E6 DF " +
            "71 A9 E3 39 9B AE 61 70 FD BB 24 13 B3 02 F7 C7 " +
            "86 1A EC 75 A3 A1 68 DA 69 FF 0D 98 5B ED F1 D7 " +
            "0B 03 FF 27 55 FF 46 BA 4C 2A A2 BA F5 DE 93 49 " +
            "50 26 7E C3 A2 6F 8F 0C C3 3C 5B 1E 71 C8 7D F5 " +
            "37 4E E5 84 FB 86 BE B3 37 E1 B8 7A 7A 96 E0 DD " +
            "25 AD B8 A6 1B 82 9D 33 30 1F AE F9 9A B7 22 89 " +
            "2D 42 F7 C3 94 0A 8E 36 AF DF 3C D6 27 57 EE EE " +
            "FC 0B 9C A7 BF 00 A4 65 45 59 69 D6 96 82 D6 0B " +
            "03 78 A4 D4 F0 9B 60 13 1E B5 DC BD F0 58 1C 03 " +
            "79 3B A3 4C F1 D9 59 DD 8B E5 6A 1F D7 DB 00 08 " +
            "45 C6 79 4B 70 50 AD 97",
            // After Pi
            "FF D6 15 CC B6 7E 46 AA 86 1A EC 75 A3 A1 68 DA " +
            "37 4E E5 84 FB 86 BE B3 FC 0B 9C A7 BF 00 A4 65 " +
            "45 C6 79 4B 70 50 AD 97 A8 4D C6 4A C3 4E E6 DF " +
            "4C 2A A2 BA F5 DE 93 49 50 26 7E C3 A2 6F 8F 0C " +
            "2D 42 F7 C3 94 0A 8E 36 79 3B A3 4C F1 D9 59 DD " +
            "52 CF 77 B5 A6 A2 14 A6 69 FF 0D 98 5B ED F1 D7 " +
            "37 E1 B8 7A 7A 96 E0 DD 45 59 69 D6 96 82 D6 0B " +
            "03 78 A4 D4 F0 9B 60 13 71 A9 E3 39 9B AE 61 70 " +
            "FD BB 24 13 B3 02 F7 C7 C3 3C 5B 1E 71 C8 7D F5 " +
            "AF DF 3C D6 27 57 EE EE 8B E5 6A 1F D7 DB 00 08 " +
            "A6 F1 52 FD 03 E2 65 7B 0B 03 FF 27 55 FF 46 BA " +
            "25 AD B8 A6 1B 82 9D 33 30 1F AE F9 9A B7 22 89 " +
            "1E B5 DC BD F0 58 1C 03",
            // After Chi  
            "CE 92 14 4C EE 78 D0 8B 4E 1B F4 56 A7 A1 68 9E " +
            "36 8A 84 CC BB D6 B7 21 46 1B 98 23 39 2E E6 4D " +
            "45 CE 91 7A 71 D1 85 C7 B8 49 9A 0B C1 6F EA DB " +
            "61 6A 23 BA E1 DE 93 7B 00 1F 7E CF C3 BE DE C5 " +
            "AD 06 B3 C1 96 0C 28 34 3D 19 83 FC C5 49 48 DD " +
            "44 CF C7 D7 86 B0 14 AE 29 E7 4C 1C DF ED E7 D5 " +
            "35 C1 3C 7A 1A 8F C0 CD 15 DE 3A F7 90 A2 C2 AF " +
            "2A 48 AC DC A9 D6 81 42 73 AD B8 35 DB 66 69 40 " +
            "D1 78 00 D3 B5 15 75 CD C3 1C 19 17 A1 40 7D F5 " +
            "DF D7 BD F6 2F 73 8F 9E 07 F7 6E 1D F7 DB 96 8F " +
            "82 5D 52 7D 09 E2 FC 7A 1B 11 F9 7E D5 CA 64 32 " +
            "2B 0D E8 A2 7B CA 81 31 90 5F AC B9 99 15 43 F1 " +
            "17 B7 71 BF A4 45 1E 83",
            // After Iota 
            "4F 12 14 CC EE 78 D0 0B 4E 1B F4 56 A7 A1 68 9E " +
            "36 8A 84 CC BB D6 B7 21 46 1B 98 23 39 2E E6 4D " +
            "45 CE 91 7A 71 D1 85 C7 B8 49 9A 0B C1 6F EA DB " +
            "61 6A 23 BA E1 DE 93 7B 00 1F 7E CF C3 BE DE C5 " +
            "AD 06 B3 C1 96 0C 28 34 3D 19 83 FC C5 49 48 DD " +
            "44 CF C7 D7 86 B0 14 AE 29 E7 4C 1C DF ED E7 D5 " +
            "35 C1 3C 7A 1A 8F C0 CD 15 DE 3A F7 90 A2 C2 AF " +
            "2A 48 AC DC A9 D6 81 42 73 AD B8 35 DB 66 69 40 " +
            "D1 78 00 D3 B5 15 75 CD C3 1C 19 17 A1 40 7D F5 " +
            "DF D7 BD F6 2F 73 8F 9E 07 F7 6E 1D F7 DB 96 8F " +
            "82 5D 52 7D 09 E2 FC 7A 1B 11 F9 7E D5 CA 64 32 " +
            "2B 0D E8 A2 7B CA 81 31 90 5F AC B9 99 15 43 F1 " +
            "17 B7 71 BF A4 45 1E 83",
            // Round #7
            // After Theta
            "94 32 70 8E 52 33 0E C1 DB F4 39 96 AD 59 79 00 " +
            "99 E2 E6 25 50 56 3B 9D 29 E0 EC 1E 1C E3 3A 09 " +
            "70 4D D7 91 0E 71 33 F7 63 69 FE 49 7D 24 34 11 " +
            "F4 85 EE 7A EB 26 82 E5 AF 77 1C 26 28 3E 52 79 " +
            "C2 FD C7 FC B3 C1 F4 70 08 9A C5 17 BA E9 FE ED " +
            "9F EF A3 95 3A FB CA 64 BC 08 81 DC D5 15 F6 4B " +
            "9A A9 5E 93 F1 0F 4C 71 7A 25 4E CA B5 6F 1E EB " +
            "1F CB EA 37 D6 76 37 72 A8 8D DC 77 67 2D B7 8A " +
            "44 97 CD 13 BF ED 64 53 6C 74 7B FE 4A C0 F1 49 " +
            "B0 2C C9 CB 0A BE 53 DA 32 74 28 F6 88 7B 20 BF " +
            "59 7D 36 3F B5 A9 22 B0 8E FE 34 BE DF 32 75 AC " +
            "84 65 8A 4B 90 4A 0D 8D FF A4 D8 84 BC D8 9F B5 " +
            "22 34 37 54 DB E5 A8 B3",
            // After Rho  
            "94 32 70 8E 52 33 0E C1 B6 E9 73 2C 5B B3 F2 00 " +
            "A6 B8 79 09 94 D5 4E 67 31 AE 93 90 02 CE EE C1 " +
            "88 9B B9 87 6B BA 8E 74 D4 47 42 13 31 96 E6 9F " +
            "AE B7 6E 22 58 4E 5F E8 DE EB 1D 87 09 8A 8F 54 " +
            "FE 63 FE D9 60 7A 38 E1 EE DF 8E A0 59 7C A1 9B " +
            "FB 7C 1F AD D4 D9 57 26 2F F1 22 04 72 57 57 D8 " +
            "9A 8C 7F 60 8A D3 4C F5 DF 3C D6 F5 4A 9C 94 6B " +
            "1B 6B BB 1B B9 8F 65 F5 EF CE 5A 6E 15 51 1B B9 " +
            "79 E2 B7 9D 6C 8A E8 B2 F8 24 36 BA 3D 7F 25 E0 " +
            "77 4A 1B 96 25 79 59 C1 BF 32 74 28 F6 88 7B 20 " +
            "8A C0 66 F5 D9 FC D4 A6 3A FA D3 F8 7E CB D4 B1 " +
            "B0 4C 71 09 52 A9 A1 91 A4 D8 84 BC D8 9F B5 FF " +
            "EA AC 08 CD 0D D5 76 39",
            // After Pi
            "94 32 70 8E 52 33 0E C1 AE B7 6E 22 58 4E 5F E8 " +
            "9A 8C 7F 60 8A D3 4C F5 77 4A 1B 96 25 79 59 C1 " +
            "EA AC 08 CD 0D D5 76 39 31 AE 93 90 02 CE EE C1 " +
            "EE DF 8E A0 59 7C A1 9B FB 7C 1F AD D4 D9 57 26 " +
            "79 E2 B7 9D 6C 8A E8 B2 B0 4C 71 09 52 A9 A1 91 " +
            "B6 E9 73 2C 5B B3 F2 00 DE EB 1D 87 09 8A 8F 54 " +
            "DF 3C D6 F5 4A 9C 94 6B BF 32 74 28 F6 88 7B 20 " +
            "8A C0 66 F5 D9 FC D4 A6 88 9B B9 87 6B BA 8E 74 " +
            "D4 47 42 13 31 96 E6 9F 2F F1 22 04 72 57 57 D8 " +
            "F8 24 36 BA 3D 7F 25 E0 A4 D8 84 BC D8 9F B5 FF " +
            "A6 B8 79 09 94 D5 4E 67 FE 63 FE D9 60 7A 38 E1 " +
            "1B 6B BB 1B B9 8F 65 F5 EF CE 5A 6E 15 51 1B B9 " +
            "3A FA D3 F8 7E CB D4 B1",
            // After Chi  
            "84 3A 61 CE D0 A2 0E D4 CB F5 6E B4 7D 66 4E E8 " +
            "12 28 7F 29 82 57 6A CD 63 58 6B 94 77 5B 51 01 " +
            "C0 29 06 ED 05 99 27 11 20 8E 82 9D 86 4F B8 E5 " +
            "EE 5D 2E B0 71 7E 09 0B 7B 70 5F AD C6 F8 56 27 " +
            "78 40 35 0D 6C CC A6 F2 7E 1D 7D 29 0B 99 A0 8B " +
            "B7 FD B1 5C 19 A7 E2 2B FE E9 3D 8F BD 8A E4 54 " +
            "DF FC D4 20 43 E8 10 ED 8B 1B 65 20 F4 8B 59 20 " +
            "C2 C2 6A 76 D9 F4 D9 F2 A3 2B 99 83 29 FB 9F 34 " +
            "04 43 56 A9 3C BE C6 BF 2B 29 A2 00 B2 D7 C7 C7 " +
            "F0 27 0F B9 1E 5F 2F E0 F0 9C C6 AC C8 9B D5 74 " +
            "A7 B0 78 0B 0D 50 0B 73 1A E7 BE BD 64 2A 22 E9 " +
            "0B 5B 3A 8B D3 05 A1 F5 6B CE 72 6F 95 45 11 FF " +
            "62 B9 55 28 1E E1 E4 31",
            // After Iota 
            "8D BA 61 CE D0 A2 0E 54 CB F5 6E B4 7D 66 4E E8 " +
            "12 28 7F 29 82 57 6A CD 63 58 6B 94 77 5B 51 01 " +
            "C0 29 06 ED 05 99 27 11 20 8E 82 9D 86 4F B8 E5 " +
            "EE 5D 2E B0 71 7E 09 0B 7B 70 5F AD C6 F8 56 27 " +
            "78 40 35 0D 6C CC A6 F2 7E 1D 7D 29 0B 99 A0 8B " +
            "B7 FD B1 5C 19 A7 E2 2B FE E9 3D 8F BD 8A E4 54 " +
            "DF FC D4 20 43 E8 10 ED 8B 1B 65 20 F4 8B 59 20 " +
            "C2 C2 6A 76 D9 F4 D9 F2 A3 2B 99 83 29 FB 9F 34 " +
            "04 43 56 A9 3C BE C6 BF 2B 29 A2 00 B2 D7 C7 C7 " +
            "F0 27 0F B9 1E 5F 2F E0 F0 9C C6 AC C8 9B D5 74 " +
            "A7 B0 78 0B 0D 50 0B 73 1A E7 BE BD 64 2A 22 E9 " +
            "0B 5B 3A 8B D3 05 A1 F5 6B CE 72 6F 95 45 11 FF " +
            "62 B9 55 28 1E E1 E4 31",
            // Round #8
            // After Theta
            "E8 A2 C8 C7 02 21 EF BB F9 0A 04 6D DA AD 1B 5F " +
            "C0 19 67 68 A3 5D 0D B5 29 29 02 D6 13 D2 C4 6E " +
            "F6 67 26 8D B6 5D 36 66 45 96 2B 94 54 CC 59 0A " +
            "DC A2 44 69 D6 B5 5C BC A9 41 47 EC E7 F2 31 5F " +
            "32 31 5C 4F 08 45 33 9D 48 53 5D 49 B8 5D B1 FC " +
            "D2 E5 18 55 CB 24 03 C4 CC 16 57 56 1A 41 B1 E3 " +
            "0D CD CC 61 62 E2 77 95 C1 6A 0C 62 90 02 CC 4F " +
            "F4 8C 4A 16 6A 30 C8 85 C6 33 30 8A FB 78 7E DB " +
            "36 BC 3C 70 9B 75 93 08 F9 18 BA 41 93 DD A0 BF " +
            "BA 56 66 FB 7A D6 BA 8F C6 D2 E6 CC 7B 5F C4 03 " +
            "C2 A8 D1 02 DF D3 EA 9C 28 18 D4 64 C3 E1 77 5E " +
            "D9 6A 22 CA F2 0F C6 8D 21 BF 1B 2D F1 CC 84 90 " +
            "54 F7 75 48 AD 25 F5 46",
            // After Rho  
            "E8 A2 C8 C7 02 21 EF BB F2 15 08 DA B4 5B 37 BE " +
            "70 C6 19 DA 68 57 43 2D 21 4D EC 96 92 22 60 3D " +
            "ED B2 31 B3 3F 33 69 B4 49 C5 9C A5 50 64 B9 42 " +
            "94 66 5D CB C5 CB 2D 4A 57 6A D0 11 FB B9 7C CC " +
            "18 AE 27 84 A2 99 4E 99 15 CB 8F 34 D5 95 84 DB " +
            "96 2E C7 A8 5A 26 19 20 8E 33 5B 5C 59 69 04 C5 " +
            "0E 13 13 BF AB 6C 68 66 05 98 9F 82 D5 18 C4 20 " +
            "0B 35 18 E4 42 7A 46 25 14 F7 F1 FC B6 8D 67 60 " +
            "07 6E B3 6E 12 C1 86 97 D0 DF 7C 0C DD A0 C9 6E " +
            "5A F7 51 D7 CA 6C 5F CF 03 C6 D2 E6 CC 7B 5F C4 " +
            "AB 73 0A A3 46 0B 7C 4F A1 60 50 93 0D 87 DF 79 " +
            "5B 4D 44 59 FE C1 B8 31 BF 1B 2D F1 CC 84 90 21 " +
            "BD 11 D5 7D 1D 52 6B 49",
            // After Pi
            "E8 A2 C8 C7 02 21 EF BB 94 66 5D CB C5 CB 2D 4A " +
            "0E 13 13 BF AB 6C 68 66 5A F7 51 D7 CA 6C 5F CF " +
            "BD 11 D5 7D 1D 52 6B 49 21 4D EC 96 92 22 60 3D " +
            "15 CB 8F 34 D5 95 84 DB 96 2E C7 A8 5A 26 19 20 " +
            "07 6E B3 6E 12 C1 86 97 5B 4D 44 59 FE C1 B8 31 " +
            "F2 15 08 DA B4 5B 37 BE 57 6A D0 11 FB B9 7C CC " +
            "05 98 9F 82 D5 18 C4 20 03 C6 D2 E6 CC 7B 5F C4 " +
            "AB 73 0A A3 46 0B 7C 4F ED B2 31 B3 3F 33 69 B4 " +
            "49 C5 9C A5 50 64 B9 42 8E 33 5B 5C 59 69 04 C5 " +
            "D0 DF 7C 0C DD A0 C9 6E BF 1B 2D F1 CC 84 90 21 " +
            "70 C6 19 DA 68 57 43 2D 18 AE 27 84 A2 99 4E 99 " +
            "0B 35 18 E4 42 7A 46 25 14 F7 F1 FC B6 8D 67 60 " +
            "A1 60 50 93 0D 87 DF 79",
            // After Chi  
            "E2 B3 CA F3 28 05 AF 9F C4 82 1D 8B 85 CB 3A C3 " +
            "AB 13 97 97 BE 7E 48 66 1A 55 59 55 C8 4D DB 7D " +
            "A9 55 C0 75 D8 98 6B 09 A3 69 AC 1E 98 00 79 1D " +
            "14 8B BF 72 D5 54 02 4C CE 2F 83 B9 B6 26 21 00 " +
            "27 6E 1B E8 12 E3 C6 9B 4F CF 47 79 BB 54 3C F3 " +
            "F2 85 07 58 B0 5B B7 9E 55 2C 90 75 F3 DA 67 08 " +
            "AD A9 97 83 D7 18 E4 2B 53 C2 D2 BE 7C 2B 5C 74 " +
            "AE 19 DA A2 0D AB 34 0F 6B 80 72 EB 36 3A 6D 31 " +
            "19 09 B8 A5 D4 E4 70 68 A1 33 5A AD 59 6D 14 C4 " +
            "90 7F 6C 0E EE 93 A0 FA BF 5E A1 F5 8C C0 00 63 " +
            "73 D7 01 BA 28 35 43 09 0C 6C C6 9C 16 1C 6F D9 " +
            "AA 35 18 E7 4B 78 DE 3C 44 71 F8 B4 D6 DD 67 64 " +
            "A9 48 76 97 8F 0F D3 E9",
            // After Iota 
            "68 B3 CA F3 28 05 AF 9F C4 82 1D 8B 85 CB 3A C3 " +
            "AB 13 97 97 BE 7E 48 66 1A 55 59 55 C8 4D DB 7D " +
            "A9 55 C0 75 D8 98 6B 09 A3 69 AC 1E 98 00 79 1D " +
            "14 8B BF 72 D5 54 02 4C CE 2F 83 B9 B6 26 21 00 " +
            "27 6E 1B E8 12 E3 C6 9B 4F CF 47 79 BB 54 3C F3 " +
            "F2 85 07 58 B0 5B B7 9E 55 2C 90 75 F3 DA 67 08 " +
            "AD A9 97 83 D7 18 E4 2B 53 C2 D2 BE 7C 2B 5C 74 " +
            "AE 19 DA A2 0D AB 34 0F 6B 80 72 EB 36 3A 6D 31 " +
            "19 09 B8 A5 D4 E4 70 68 A1 33 5A AD 59 6D 14 C4 " +
            "90 7F 6C 0E EE 93 A0 FA BF 5E A1 F5 8C C0 00 63 " +
            "73 D7 01 BA 28 35 43 09 0C 6C C6 9C 16 1C 6F D9 " +
            "AA 35 18 E7 4B 78 DE 3C 44 71 F8 B4 D6 DD 67 64 " +
            "A9 48 76 97 8F 0F D3 E9",
            // Round #9
            // After Theta
            "16 A7 D8 55 86 D7 9E 8C 62 AD 8C A0 00 31 FB 8D " +
            "4F BC D2 50 E2 54 05 49 65 EC 8D 2B DE 48 FD 37 " +
            "51 B2 E0 04 7B F1 73 4D DD 7D BE B8 36 D2 48 0E " +
            "B2 A4 2E 59 50 AE C3 02 2A 80 C6 7E EA 0C 6C 2F " +
            "58 D7 CF 96 04 E6 E0 D1 B7 28 67 08 18 3D 24 B7 " +
            "8C 91 15 FE 1E 89 86 8D F3 03 01 5E 76 20 A6 46 " +
            "49 06 D2 44 8B 32 A9 04 2C 7B 06 C0 6A 2E 7A 3E " +
            "56 FE FA D3 AE C2 2C 4B 15 94 60 4D 98 E8 5C 22 " +
            "BF 26 29 8E 51 1E B1 26 45 9C 1F 6A 05 47 59 EB " +
            "EF C6 B8 70 F8 96 86 B0 47 B9 81 84 2F A9 18 27 " +
            "0D C3 13 1C 86 E7 72 1A AA 43 57 B7 93 E6 AE 97 " +
            "4E 9A 5D 20 17 52 93 13 3B C8 2C CA C0 D8 41 2E " +
            "51 AF 56 E6 2C 66 CB AD",
            // After Rho  
            "16 A7 D8 55 86 D7 9E 8C C5 5A 19 41 01 62 F6 1B " +
            "13 AF 34 94 38 55 41 D2 8D D4 7F 53 C6 DE B8 E2 " +
            "8B 9F 6B 8A 92 05 27 D8 6B 23 8D E4 D0 DD E7 8B " +
            "92 05 E5 3A 2C 20 4B EA 8B 0A A0 B1 9F 3A 03 DB " +
            "EB 67 4B 02 73 F0 68 AC 43 72 7B 8B 72 86 80 D1 " +
            "64 8C AC F0 F7 48 34 6C 1A CD 0F 04 78 D9 81 98 " +
            "26 5A 94 49 25 48 32 90 5C F4 7C 58 F6 0C 80 D5 " +
            "69 57 61 96 25 2B 7F FD 9A 30 D1 B9 44 2A 28 C1 " +
            "C5 31 CA 23 D6 E4 D7 24 AC F5 22 CE 0F B5 82 A3 " +
            "D2 10 F6 DD 18 17 0E DF 27 47 B9 81 84 2F A9 18 " +
            "CB 69 34 0C 4F 70 18 9E AA 0E 5D DD 4E 9A BB 5E " +
            "49 B3 0B E4 42 6A 72 C2 C8 2C CA C0 D8 41 2E 3B " +
            "72 6B D4 AB 95 39 8B D9",
            // After Pi
            "16 A7 D8 55 86 D7 9E 8C 92 05 E5 3A 2C 20 4B EA " +
            "26 5A 94 49 25 48 32 90 D2 10 F6 DD 18 17 0E DF " +
            "72 6B D4 AB 95 39 8B D9 8D D4 7F 53 C6 DE B8 E2 " +
            "43 72 7B 8B 72 86 80 D1 64 8C AC F0 F7 48 34 6C " +
            "C5 31 CA 23 D6 E4 D7 24 49 B3 0B E4 42 6A 72 C2 " +
            "C5 5A 19 41 01 62 F6 1B 8B 0A A0 B1 9F 3A 03 DB " +
            "5C F4 7C 58 F6 0C 80 D5 27 47 B9 81 84 2F A9 18 " +
            "CB 69 34 0C 4F 70 18 9E 8B 9F 6B 8A 92 05 27 D8 " +
            "6B 23 8D E4 D0 DD E7 8B 1A CD 0F 04 78 D9 81 98 " +
            "AC F5 22 CE 0F B5 82 A3 C8 2C CA C0 D8 41 2E 3B " +
            "13 AF 34 94 38 55 41 D2 EB 67 4B 02 73 F0 68 AC " +
            "69 57 61 96 25 2B 7F FD 9A 30 D1 B9 44 2A 28 C1 " +
            "AA 0E 5D DD 4E 9A BB 5E",
            // After Chi  
            "32 FD C8 14 87 9F AE 9C 42 05 87 AE 34 37 47 A5 " +
            "06 31 94 6B A0 60 B3 90 D6 94 FE 89 1A D1 1A DB " +
            "F2 6B F1 81 BD 19 CA BB A9 58 FB 23 43 96 8C CE " +
            "C2 43 39 88 72 22 43 D1 6C 0E AD 34 F7 42 14 AE " +
            "41 75 BE 30 52 70 5F 04 0B 91 0B 6C 72 6A 72 D3 " +
            "91 AE 45 09 61 66 76 1F A8 09 21 30 9F 19 2A D3 " +
            "94 DC 78 54 BD 5C 90 53 23 55 B0 C0 84 2D 4F 19 " +
            "C1 69 94 BC D1 68 19 5E 9B 53 69 8A BA 05 27 C8 " +
            "CF 13 AD 2E D7 F9 E5 A8 5A C5 C7 04 A8 99 AD 80 " +
            "AF 66 03 C4 0D B1 83 63 A8 0C 4E A4 98 99 EE 38 " +
            "13 BF 14 00 3C 5E 56 83 79 47 DB 2B 33 F0 68 AC " +
            "49 59 6D D2 2F BB EC E3 8B 91 F1 B9 74 6F 68 41 " +
            "42 4E 16 DF 0D 3A 93 72",
            // After Iota 
            "BA FD C8 14 87 9F AE 9C 42 05 87 AE 34 37 47 A5 " +
            "06 31 94 6B A0 60 B3 90 D6 94 FE 89 1A D1 1A DB " +
            "F2 6B F1 81 BD 19 CA BB A9 58 FB 23 43 96 8C CE " +
            "C2 43 39 88 72 22 43 D1 6C 0E AD 34 F7 42 14 AE " +
            "41 75 BE 30 52 70 5F 04 0B 91 0B 6C 72 6A 72 D3 " +
            "91 AE 45 09 61 66 76 1F A8 09 21 30 9F 19 2A D3 " +
            "94 DC 78 54 BD 5C 90 53 23 55 B0 C0 84 2D 4F 19 " +
            "C1 69 94 BC D1 68 19 5E 9B 53 69 8A BA 05 27 C8 " +
            "CF 13 AD 2E D7 F9 E5 A8 5A C5 C7 04 A8 99 AD 80 " +
            "AF 66 03 C4 0D B1 83 63 A8 0C 4E A4 98 99 EE 38 " +
            "13 BF 14 00 3C 5E 56 83 79 47 DB 2B 33 F0 68 AC " +
            "49 59 6D D2 2F BB EC E3 8B 91 F1 B9 74 6F 68 41 " +
            "42 4E 16 DF 0D 3A 93 72",
            // Round #10  
            // After Theta
            "55 1B 2C 19 76 2D 34 A7 92 1D 5A A1 CC BB 8E BF " +
            "B9 AD 79 70 F7 C0 D2 FA 9F 48 78 00 61 FC D5 2C " +
            "76 E6 E4 ED 4F 23 61 53 46 BE 1F 2E B2 24 16 F5 " +
            "12 5B E4 87 8A AE 8A CB D3 92 40 2F A0 E2 75 C4 " +
            "08 A9 38 B9 29 5D 90 F3 8F 1C 1E 00 80 50 D9 3B " +
            "7E 48 A1 04 90 D4 EC 24 78 11 FC 3F 67 95 E3 C9 " +
            "2B 40 95 4F EA FC F1 39 6A 89 36 49 FF 00 80 EE " +
            "45 E4 81 D0 23 52 B2 B6 74 B5 8D 87 4B B7 BD F3 " +
            "1F 0B 70 21 2F 75 2C B2 E5 59 2A 1F FF 39 CC EA " +
            "E6 BA 85 4D 76 9C 4C 94 2C 81 5B C8 6A A3 45 D0 " +
            "FC 59 F0 0D CD EC CC B8 A9 5F 06 24 CB 7C A1 B6 " +
            "F6 C5 80 C9 78 1B 8D 89 C2 4D 77 30 0F 42 A7 B6 " +
            "C6 C3 03 B3 FF 00 38 9A",
            // After Rho  
            "55 1B 2C 19 76 2D 34 A7 25 3B B4 42 99 77 1D 7F " +
            "6E 6B 1E DC 3D B0 B4 7E C6 5F CD F2 89 84 07 10 " +
            "1A 09 9B B2 33 27 6F 7F 22 4B 62 51 6F E4 FB E1 " +
            "7E A8 E8 AA B8 2C B1 45 F1 B4 24 D0 0B A8 78 1D " +
            "54 9C DC 94 2E C8 79 84 95 BD F3 C8 E1 01 00 08 " +
            "F1 43 0A 25 80 A4 66 27 27 E3 45 F0 FF 9C 55 8E " +
            "7C 52 E7 8F CF 59 01 AA 01 00 DD D5 12 6D 92 FE " +
            "E8 11 29 59 DB 22 F2 40 0F 97 6E 7B E7 E9 6A 1B " +
            "2E E4 A5 8E 45 F6 63 01 66 F5 F2 2C 95 8F FF 1C " +
            "93 89 D2 5C B7 B0 C9 8E D0 2C 81 5B C8 6A A3 45 " +
            "33 E3 F2 67 C1 37 34 B3 A6 7E 19 90 2C F3 85 DA " +
            "BE 18 30 19 6F A3 31 D1 4D 77 30 0F 42 A7 B6 C2 " +
            "8E A6 F1 F0 C0 EC 3F 00",
            // After Pi
            "55 1B 2C 19 76 2D 34 A7 7E A8 E8 AA B8 2C B1 45 " +
            "7C 52 E7 8F CF 59 01 AA 93 89 D2 5C B7 B0 C9 8E " +
            "8E A6 F1 F0 C0 EC 3F 00 C6 5F CD F2 89 84 07 10 " +
            "95 BD F3 C8 E1 01 00 08 F1 43 0A 25 80 A4 66 27 " +
            "2E E4 A5 8E 45 F6 63 01 BE 18 30 19 6F A3 31 D1 " +
            "25 3B B4 42 99 77 1D 7F F1 B4 24 D0 0B A8 78 1D " +
            "01 00 DD D5 12 6D 92 FE D0 2C 81 5B C8 6A A3 45 " +
            "33 E3 F2 67 C1 37 34 B3 1A 09 9B B2 33 27 6F 7F " +
            "22 4B 62 51 6F E4 FB E1 27 E3 45 F0 FF 9C 55 8E " +
            "66 F5 F2 2C 95 8F FF 1C 4D 77 30 0F 42 A7 B6 C2 " +
            "6E 6B 1E DC 3D B0 B4 7E 54 9C DC 94 2E C8 79 84 " +
            "E8 11 29 59 DB 22 F2 40 0F 97 6E 7B E7 E9 6A 1B " +
            "A6 7E 19 90 2C F3 85 DA",
            // After Chi  
            "55 49 2B 1C 31 7C 34 0D FD 21 F8 FA 88 8C 79 41 " +
            "70 74 C6 2F 8F 15 37 AA C2 90 DE 55 81 B1 C9 29 " +
            "A4 06 31 52 48 EC BE 40 A6 1D C5 D7 89 20 61 37 " +
            "9B 19 56 42 A4 53 01 08 61 5B 1A 34 AA A5 76 F7 " +
            "6E A3 68 6C C5 F2 65 01 AF B8 02 11 0F A2 31 D9 " +
            "25 3B 6D 47 89 32 9F 9D 21 98 24 DA C3 AA 59 1C " +
            "22 C3 AF F1 13 78 86 4C D4 34 85 5B D0 2A AA 09 " +
            "E3 67 F2 F7 C3 BF 54 B3 1F A9 9E 12 A3 3F 6B 71 " +
            "62 5F D0 5D 6F E7 51 F1 2E E1 45 F3 BD BC 55 4C " +
            "74 FD 79 9C A4 8F B6 21 6D 35 50 4E 0E 67 26 42 " +
            "C6 6A 3F 95 EC 92 36 3E 53 1A 9A B6 0A 01 71 9F " +
            "48 79 38 D9 D3 30 77 80 47 96 68 37 F6 E9 5A 3F " +
            "B6 EA D9 90 2E BB CC 5A",
            // After Iota 
            "5C C9 2B 9C 31 7C 34 0D FD 21 F8 FA 88 8C 79 41 " +
            "70 74 C6 2F 8F 15 37 AA C2 90 DE 55 81 B1 C9 29 " +
            "A4 06 31 52 48 EC BE 40 A6 1D C5 D7 89 20 61 37 " +
            "9B 19 56 42 A4 53 01 08 61 5B 1A 34 AA A5 76 F7 " +
            "6E A3 68 6C C5 F2 65 01 AF B8 02 11 0F A2 31 D9 " +
            "25 3B 6D 47 89 32 9F 9D 21 98 24 DA C3 AA 59 1C " +
            "22 C3 AF F1 13 78 86 4C D4 34 85 5B D0 2A AA 09 " +
            "E3 67 F2 F7 C3 BF 54 B3 1F A9 9E 12 A3 3F 6B 71 " +
            "62 5F D0 5D 6F E7 51 F1 2E E1 45 F3 BD BC 55 4C " +
            "74 FD 79 9C A4 8F B6 21 6D 35 50 4E 0E 67 26 42 " +
            "C6 6A 3F 95 EC 92 36 3E 53 1A 9A B6 0A 01 71 9F " +
            "48 79 38 D9 D3 30 77 80 47 96 68 37 F6 E9 5A 3F " +
            "B6 EA D9 90 2E BB CC 5A",
            // Round #11  
            // After Theta
            "83 05 E2 E5 80 76 06 49 50 E5 C6 F1 47 C7 24 12 " +
            "90 49 42 34 88 99 E2 EE F1 E8 40 41 91 AE 4E 90 " +
            "E2 32 57 8D 73 65 7B AE 79 D1 0C AE 38 2A 53 73 " +
            "36 DD 68 49 6B 18 5C 5B 81 66 9E 2F AD 29 A3 B3 " +
            "5D DB F6 78 D5 ED E2 B8 E9 8C 64 CE 34 2B F4 37 " +
            "FA F7 A4 3E 38 38 AD D9 8C 5C 1A D1 0C E1 04 4F " +
            "C2 FE 2B EA 14 F4 53 08 E7 4C 1B 4F C0 35 2D B0 " +
            "A5 53 94 28 F8 36 91 5D C0 65 57 6B 12 35 59 35 " +
            "CF 9B EE 56 A0 AC 0C A2 CE DC C1 E8 BA 30 80 08 " +
            "47 85 E7 88 B4 90 31 98 2B 01 36 91 35 EE E3 AC " +
            "19 A6 F6 EC 5D 98 04 7A FE DE A4 BD C5 4A 2C CC " +
            "A8 44 BC C2 D4 BC A2 C4 74 EE F6 23 E6 F6 DD 86 " +
            "F0 DE BF 4F 15 32 09 B4",
            // After Rho  
            "83 05 E2 E5 80 76 06 49 A0 CA 8D E3 8F 8E 49 24 " +
            "64 92 10 0D 62 A6 B8 3B E9 EA 04 19 8F 0E 14 14 " +
            "2B DB 73 15 97 B9 6A 9C 8A A3 32 35 97 17 CD E0 " +
            "96 B4 86 C1 B5 65 D3 8D 6C A0 99 E7 4B 6B CA E8 " +
            "6D 7B BC EA 76 71 DC AE 42 7F 93 CE 48 E6 4C B3 " +
            "D6 BF 27 F5 C1 C1 69 CD 3C 31 72 69 44 33 84 13 " +
            "51 A7 A0 9F 42 10 F6 5F 6B 5A 60 CF 99 36 9E 80 " +
            "14 7C 9B C8 AE D2 29 4A D6 24 6A B2 6A 80 CB AE " +
            "DD 0A 94 95 41 F4 79 D3 40 04 67 EE 60 74 5D 18 " +
            "32 06 F3 A8 F0 1C 91 16 AC 2B 01 36 91 35 EE E3 " +
            "12 E8 65 98 DA B3 77 61 FB 7B 93 F6 16 2B B1 30 " +
            "95 88 57 98 9A 57 94 18 EE F6 23 E6 F6 DD 86 74 " +
            "02 2D BC F7 EF 53 85 4C",
            // After Pi
            "83 05 E2 E5 80 76 06 49 96 B4 86 C1 B5 65 D3 8D " +
            "51 A7 A0 9F 42 10 F6 5F 32 06 F3 A8 F0 1C 91 16 " +
            "02 2D BC F7 EF 53 85 4C E9 EA 04 19 8F 0E 14 14 " +
            "42 7F 93 CE 48 E6 4C B3 D6 BF 27 F5 C1 C1 69 CD " +
            "DD 0A 94 95 41 F4 79 D3 95 88 57 98 9A 57 94 18 " +
            "A0 CA 8D E3 8F 8E 49 24 6C A0 99 E7 4B 6B CA E8 " +
            "6B 5A 60 CF 99 36 9E 80 AC 2B 01 36 91 35 EE E3 " +
            "12 E8 65 98 DA B3 77 61 2B DB 73 15 97 B9 6A 9C " +
            "8A A3 32 35 97 17 CD E0 3C 31 72 69 44 33 84 13 " +
            "40 04 67 EE 60 74 5D 18 EE F6 23 E6 F6 DD 86 74 " +
            "64 92 10 0D 62 A6 B8 3B 6D 7B BC EA 76 71 DC AE " +
            "14 7C 9B C8 AE D2 29 4A D6 24 6A B2 6A 80 CB AE " +
            "FB 7B 93 F6 16 2B B1 30",
            // After Chi  
            "C2 06 C2 FB C2 66 22 1B B4 B4 D5 E1 05 69 D2 8D " +
            "51 8E AC C8 4D 53 F2 17 B3 06 B1 A8 F0 38 93 17 " +
            "16 9D B8 F7 DA 52 54 C8 7D 6A 20 28 0E 0F 35 58 " +
            "4B 7F 03 CE 48 D2 5C A1 D6 3F 64 FD 5B C2 ED C5 " +
            "B5 68 94 94 44 FC 79 D7 97 9D C4 5E DA B7 DC BB " +
            "A3 90 ED EB 1F 9A 5D 24 E8 81 98 D7 4B 6A AA 8B " +
            "79 9A 04 47 D3 B4 8F 80 0C 29 89 55 94 39 E6 E7 " +
            "5E C8 75 9C 9A D2 F5 A9 1F CB 33 5D D7 99 6A 8F " +
            "CA A7 37 B3 B7 53 94 E8 92 C3 72 69 D2 BA 06 77 " +
            "41 0D 37 FF 61 54 35 90 6E D6 23 C6 F6 DB 03 14 " +
            "74 96 13 0D EA 24 99 7B AF 7B DC D8 36 71 1E 0A " +
            "3D 27 0A 8C BA F9 19 5A D2 A4 6A BB 0A 04 C3 A5 " +
            "F2 12 3F 14 02 7A F5 B4",
            // After Iota 
            "C8 06 C2 7B C2 66 22 1B B4 B4 D5 E1 05 69 D2 8D " +
            "51 8E AC C8 4D 53 F2 17 B3 06 B1 A8 F0 38 93 17 " +
            "16 9D B8 F7 DA 52 54 C8 7D 6A 20 28 0E 0F 35 58 " +
            "4B 7F 03 CE 48 D2 5C A1 D6 3F 64 FD 5B C2 ED C5 " +
            "B5 68 94 94 44 FC 79 D7 97 9D C4 5E DA B7 DC BB " +
            "A3 90 ED EB 1F 9A 5D 24 E8 81 98 D7 4B 6A AA 8B " +
            "79 9A 04 47 D3 B4 8F 80 0C 29 89 55 94 39 E6 E7 " +
            "5E C8 75 9C 9A D2 F5 A9 1F CB 33 5D D7 99 6A 8F " +
            "CA A7 37 B3 B7 53 94 E8 92 C3 72 69 D2 BA 06 77 " +
            "41 0D 37 FF 61 54 35 90 6E D6 23 C6 F6 DB 03 14 " +
            "74 96 13 0D EA 24 99 7B AF 7B DC D8 36 71 1E 0A " +
            "3D 27 0A 8C BA F9 19 5A D2 A4 6A BB 0A 04 C3 A5 " +
            "F2 12 3F 14 02 7A F5 B4",
            // Round #12  
            // After Theta
            "6F 26 9C BB A3 17 F4 EA 6B 8B 93 26 B0 EA 75 E1 " +
            "11 C5 EA 00 5C FA A9 77 64 D1 2F F1 80 72 0B 9D " +
            "74 31 16 0A 4C 62 DC FD DA 4A 7E E8 6F 7E E3 A9 " +
            "94 40 45 09 FD 51 FB CD 96 74 22 35 4A 6B B6 A5 " +
            "62 BF 0A CD 34 B6 E1 5D F5 31 6A A3 4C 87 54 8E " +
            "04 B0 B3 2B 7E EB 8B D5 37 BE DE 10 FE E9 0D E7 " +
            "39 D1 42 8F C2 1D D4 E0 DB FE 17 0C E4 73 7E 6D " +
            "3C 64 DB 61 0C E2 7D 9C B8 EB 6D 9D B6 E8 BC 7E " +
            "15 98 71 74 02 D0 33 84 D2 88 34 A1 C3 13 5D 17 " +
            "96 DA A9 A6 11 1E AD 1A 0C 7A 8D 3B 60 EB 8B 21 " +
            "D3 B6 4D CD 8B 55 4F 8A 70 44 9A 1F 83 F2 B9 66 " +
            "7D 6C 4C 44 AB 50 42 3A 05 73 F4 E2 7A 4E 5B 2F " +
            "90 BE 91 E9 94 4A 7D 81",
            // After Rho  
            "6F 26 9C BB A3 17 F4 EA D7 16 27 4D 60 D5 EB C2 " +
            "44 B1 3A 00 97 7E EA 5D 28 B7 D0 49 16 FD 12 0F " +
            "12 E3 EE A7 8B B1 50 60 FE E6 37 9E AA AD E4 87 " +
            "94 D0 1F B5 DF 4C 09 54 A9 25 9D 48 8D D2 9A 6D " +
            "5F 85 66 1A DB F0 2E B1 48 E5 58 1F A3 36 CA 74 " +
            "26 80 9D 5D F1 5B 5F AC 9C DF F8 7A 43 F8 A7 37 " +
            "7A 14 EE A0 06 CF 89 16 E7 FC DA B6 FD 2F 18 C8 " +
            "30 06 F1 3E 4E 1E B2 ED 3A 6D D1 79 FD 70 D7 DB " +
            "8E 4E 00 7A 86 B0 02 33 AE 0B 69 44 9A D0 E1 89 " +
            "A3 55 C3 52 3B D5 34 C2 21 0C 7A 8D 3B 60 EB 8B " +
            "3D 29 4E DB 36 35 2F 56 C1 11 69 7E 0C CA E7 9A " +
            "8F 8D 89 68 15 4A 48 A7 73 F4 E2 7A 4E 5B 2F 05 " +
            "5F 20 A4 6F 64 3A A5 52",
            // After Pi
            "6F 26 9C BB A3 17 F4 EA 94 D0 1F B5 DF 4C 09 54 " +
            "7A 14 EE A0 06 CF 89 16 A3 55 C3 52 3B D5 34 C2 " +
            "5F 20 A4 6F 64 3A A5 52 28 B7 D0 49 16 FD 12 0F " +
            "48 E5 58 1F A3 36 CA 74 26 80 9D 5D F1 5B 5F AC " +
            "8E 4E 00 7A 86 B0 02 33 8F 8D 89 68 15 4A 48 A7 " +
            "D7 16 27 4D 60 D5 EB C2 A9 25 9D 48 8D D2 9A 6D " +
            "E7 FC DA B6 FD 2F 18 C8 21 0C 7A 8D 3B 60 EB 8B " +
            "3D 29 4E DB 36 35 2F 56 12 E3 EE A7 8B B1 50 60 " +
            "FE E6 37 9E AA AD E4 87 9C DF F8 7A 43 F8 A7 37 " +
            "AE 0B 69 44 9A D0 E1 89 73 F4 E2 7A 4E 5B 2F 05 " +
            "44 B1 3A 00 97 7E EA 5D 5F 85 66 1A DB F0 2E B1 " +
            "30 06 F1 3E 4E 1E B2 ED 3A 6D D1 79 FD 70 D7 DB " +
            "C1 11 69 7E 0C CA E7 9A",
            // After Chi  
            "05 22 7C BB A3 94 74 E8 15 91 1E E7 E6 5C 3D 94 " +
            "26 34 CA 8D 42 E5 08 06 83 53 DB C2 B8 D0 64 6A " +
            "CF F0 A7 6B 38 72 AC 46 0E B7 55 09 46 B4 07 87 " +
            "C0 AB 58 3D A5 96 CA 67 27 01 14 5D E0 11 17 28 " +
            "AE 7C 50 7B 84 05 10 3B CF CD 81 7E B4 48 80 D7 " +
            "91 CE 65 FB 10 F8 EB 42 A9 25 BD 41 8F 92 79 6E " +
            "FB DD DE E4 F9 3A 1C 9C E3 1A 5B 89 7B A0 2B 0B " +
            "15 08 D6 DB BB 37 3F 7B 12 FA 26 C7 CA E1 53 50 " +
            "DC E6 36 9A 32 AD A4 0F CD 2B 7A 40 07 F3 A9 33 " +
            "AE 08 65 C1 1B 70 B1 E9 9F F0 F3 62 6E 57 8B 82 " +
            "64 B3 AB 24 93 70 7A 11 55 EC 66 5B 6A 90 6B A3 " +
            "F1 16 D9 38 4E 94 92 ED 3E CD C3 79 6E 44 DF 9E " +
            "DA 15 2D 64 44 4A E3 3A",
            // After Iota 
            "8E A2 7C 3B A3 94 74 E8 15 91 1E E7 E6 5C 3D 94 " +
            "26 34 CA 8D 42 E5 08 06 83 53 DB C2 B8 D0 64 6A " +
            "CF F0 A7 6B 38 72 AC 46 0E B7 55 09 46 B4 07 87 " +
            "C0 AB 58 3D A5 96 CA 67 27 01 14 5D E0 11 17 28 " +
            "AE 7C 50 7B 84 05 10 3B CF CD 81 7E B4 48 80 D7 " +
            "91 CE 65 FB 10 F8 EB 42 A9 25 BD 41 8F 92 79 6E " +
            "FB DD DE E4 F9 3A 1C 9C E3 1A 5B 89 7B A0 2B 0B " +
            "15 08 D6 DB BB 37 3F 7B 12 FA 26 C7 CA E1 53 50 " +
            "DC E6 36 9A 32 AD A4 0F CD 2B 7A 40 07 F3 A9 33 " +
            "AE 08 65 C1 1B 70 B1 E9 9F F0 F3 62 6E 57 8B 82 " +
            "64 B3 AB 24 93 70 7A 11 55 EC 66 5B 6A 90 6B A3 " +
            "F1 16 D9 38 4E 94 92 ED 3E CD C3 79 6E 44 DF 9E " +
            "DA 15 2D 64 44 4A E3 3A",
            // Round #13  
            // After Theta
            "34 59 04 46 96 4F 8D D8 FE A8 98 54 6E 47 FD 20 " +
            "6F C1 8C C7 B3 02 2B 6D E5 26 25 1E 91 59 AA A2 " +
            "5F 24 52 B6 52 A0 FF B2 B4 4C 2D 74 73 6F FE B7 " +
            "2B 92 DE 8E 2D 8D 0A D3 6E F4 52 17 11 F6 34 43 " +
            "C8 09 AE A7 AD 8C DE F3 5F 19 74 A3 DE 9A D3 23 " +
            "2B 35 1D 86 25 23 12 72 42 1C 3B F2 07 89 B9 DA " +
            "B2 28 98 AE 08 DD 3F F7 85 6F A5 55 52 29 E5 C3 " +
            "85 DC 23 06 D1 E5 6C 8F A8 01 5E BA FF 3A AA 60 " +
            "37 DF B0 29 BA B6 64 BB 84 DE 3C 0A F6 14 8A 58 " +
            "C8 7D 9B 1D 32 F9 7F 21 0F 24 06 BF 04 85 D8 76 " +
            "DE 48 D3 59 A6 AB 83 21 BE D5 E0 E8 E2 8B AB 17 " +
            "B8 E3 9F 72 BF 73 B1 86 58 B8 3D A5 47 CD 11 56 " +
            "4A C1 D8 B9 2E 98 B0 CE",
            // After Rho  
            "34 59 04 46 96 4F 8D D8 FC 51 31 A9 DC 8E FA 41 " +
            "5B 30 E3 F1 AC C0 4A DB 99 A5 2A 5A 6E 52 E2 11 " +
            "02 FD 97 FD 22 91 B2 95 37 F7 E6 7F 4B CB D4 42 " +
            "ED D8 D2 A8 30 BD 22 E9 90 1B BD D4 45 84 3D CD " +
            "04 D7 D3 56 46 EF 79 E4 39 3D F2 95 41 37 EA AD " +
            "5B A9 E9 30 2C 19 91 90 6A 0B 71 EC C8 1F 24 E6 " +
            "74 45 E8 FE B9 97 45 C1 52 CA 87 0B DF 4A AB A4 " +
            "83 E8 72 B6 C7 42 EE 11 74 FF 75 54 C1 50 03 BC " +
            "36 45 D7 96 6C F7 E6 1B 45 2C 42 6F 1E 05 7B 0A " +
            "FF 2F 04 B9 6F B3 43 26 76 0F 24 06 BF 04 85 D8 " +
            "0E 86 78 23 4D 67 99 AE F8 56 83 A3 8B 2F AE 5E " +
            "77 FC 53 EE 77 2E D6 10 B8 3D A5 47 CD 11 56 58 " +
            "AC B3 52 30 76 AE 0B 26",
            // After Pi
            "34 59 04 46 96 4F 8D D8 ED D8 D2 A8 30 BD 22 E9 " +
            "74 45 E8 FE B9 97 45 C1 FF 2F 04 B9 6F B3 43 26 " +
            "AC B3 52 30 76 AE 0B 26 99 A5 2A 5A 6E 52 E2 11 " +
            "39 3D F2 95 41 37 EA AD 5B A9 E9 30 2C 19 91 90 " +
            "36 45 D7 96 6C F7 E6 1B 77 FC 53 EE 77 2E D6 10 " +
            "FC 51 31 A9 DC 8E FA 41 90 1B BD D4 45 84 3D CD " +
            "52 CA 87 0B DF 4A AB A4 76 0F 24 06 BF 04 85 D8 " +
            "0E 86 78 23 4D 67 99 AE 02 FD 97 FD 22 91 B2 95 " +
            "37 F7 E6 7F 4B CB D4 42 6A 0B 71 EC C8 1F 24 E6 " +
            "45 2C 42 6F 1E 05 7B 0A B8 3D A5 47 CD 11 56 58 " +
            "5B 30 E3 F1 AC C0 4A DB 04 D7 D3 56 46 EF 79 E4 " +
            "83 E8 72 B6 C7 42 EE 11 74 FF 75 54 C1 50 03 BC " +
            "F8 56 83 A3 8B 2F AE 5E",
            // After Chi  
            "24 5C 2C 10 1F 4D C8 D8 66 F2 D6 A9 76 9D 20 CF " +
            "74 D5 BA FE A9 9B 4D C1 EF 67 00 FF EF F2 C7 FE " +
            "65 33 80 98 56 1E 29 07 DB 25 23 7A 42 5A F3 01 " +
            "1D 79 E4 13 01 D1 8C A6 1A 11 E9 58 3F 11 81 90 " +
            "BE 44 FF 86 64 A7 C6 1A 57 E4 83 6B 76 0B DE BC " +
            "BE 91 33 A2 46 C4 78 61 B4 1E 9D D0 65 80 39 95 " +
            "5A 4A DF 2A 9F 29 B3 82 86 5E 25 8E 2F 8C E7 99 " +
            "0E 8C F4 77 4C 67 9C 22 4A F5 86 7D A2 85 92 31 " +
            "32 D3 E4 7C 5D CB 8F 4A D2 1A D4 EC 09 0F 20 B6 " +
            "47 EC 50 D7 3C 85 DB 8F 8D 3F C5 45 84 5B 12 1A " +
            "D8 18 C3 51 2D C0 CC CA 70 C0 D6 16 46 FF 78 48 " +
            "0B E8 F0 15 CD 6D 42 53 77 DF 15 04 E5 90 43 3D " +
            "FC 91 93 A5 C9 00 9F 7A",
            // After Iota 
            "AF 5C 2C 10 1F 4D C8 58 66 F2 D6 A9 76 9D 20 CF " +
            "74 D5 BA FE A9 9B 4D C1 EF 67 00 FF EF F2 C7 FE " +
            "65 33 80 98 56 1E 29 07 DB 25 23 7A 42 5A F3 01 " +
            "1D 79 E4 13 01 D1 8C A6 1A 11 E9 58 3F 11 81 90 " +
            "BE 44 FF 86 64 A7 C6 1A 57 E4 83 6B 76 0B DE BC " +
            "BE 91 33 A2 46 C4 78 61 B4 1E 9D D0 65 80 39 95 " +
            "5A 4A DF 2A 9F 29 B3 82 86 5E 25 8E 2F 8C E7 99 " +
            "0E 8C F4 77 4C 67 9C 22 4A F5 86 7D A2 85 92 31 " +
            "32 D3 E4 7C 5D CB 8F 4A D2 1A D4 EC 09 0F 20 B6 " +
            "47 EC 50 D7 3C 85 DB 8F 8D 3F C5 45 84 5B 12 1A " +
            "D8 18 C3 51 2D C0 CC CA 70 C0 D6 16 46 FF 78 48 " +
            "0B E8 F0 15 CD 6D 42 53 77 DF 15 04 E5 90 43 3D " +
            "FC 91 93 A5 C9 00 9F 7A",
            // Round #14  
            // After Theta
            "F9 A4 B6 75 2C 94 EB 5D E4 0E FF A6 78 88 06 60 " +
            "36 CE 19 B7 5A FB D2 A1 99 F1 EB 43 60 61 16 3B " +
            "33 77 ED 74 02 FF 6C 4E 8D DD B9 1F 71 83 D0 04 " +
            "9F 85 CD 1C 0F C4 AA 09 58 0A 4A 11 CC 71 1E F0 " +
            "C8 D2 14 3A EB 34 17 DF 01 A0 EE 87 22 EA 9B F5 " +
            "E8 69 A9 C7 75 1D 5B 64 36 E2 B4 DF 6B 95 1F 3A " +
            "18 51 7C 63 6C 49 2C E2 F0 C8 CE 32 A0 1F 36 5C " +
            "58 C8 99 9B 18 86 D9 6B 1C 0D 1C 18 91 5C B1 34 " +
            "B0 2F CD 73 53 DE A9 E5 90 01 77 A5 FA 6F BF D6 " +
            "31 7A BB 6B B3 16 0A 4A DB 7B A8 A9 D0 BA 57 53 " +
            "8E E0 59 34 1E 19 EF CF F2 3C FF 19 48 EA 5E E7 " +
            "49 F3 53 5C 3E 0D DD 33 01 49 FE B8 6A 03 92 F8 " +
            "AA D5 FE 49 9D E1 DA 33",
            // After Rho  
            "F9 A4 B6 75 2C 94 EB 5D C8 1D FE 4D F1 10 0D C0 " +
            "8D 73 C6 AD D6 BE 74 A8 16 66 B1 93 19 BF 3E 04 " +
            "F8 67 73 9A B9 6B A7 13 11 37 08 4D D0 D8 9D FB " +
            "CC F1 40 AC 9A F0 59 D8 3C 96 82 52 04 73 9C 07 " +
            "69 0A 9D 75 9A 8B 6F 64 BE 59 1F 00 EA 7E 28 A2 " +
            "43 4F 4B 3D AE EB D8 22 E8 D8 88 D3 7E AF 55 7E " +
            "1B 63 4B 62 11 C7 88 E2 3F 6C B8 E0 91 9D 65 40 " +
            "4D 0C C3 EC 35 2C E4 CC 30 22 B9 62 69 38 1A 38 " +
            "79 6E CA 3B B5 1C F6 A5 5F 6B C8 80 BB 52 FD B7 " +
            "42 41 29 46 6F 77 6D D6 53 DB 7B A8 A9 D0 BA 57 " +
            "BC 3F 3B 82 67 D1 78 64 CB F3 FC 67 20 A9 7B 9D " +
            "69 7E 8A CB A7 A1 7B 26 49 FE B8 6A 03 92 F8 01 " +
            "F6 8C 6A B5 7F 52 67 B8",
            // After Pi
            "F9 A4 B6 75 2C 94 EB 5D CC F1 40 AC 9A F0 59 D8 " +
            "1B 63 4B 62 11 C7 88 E2 42 41 29 46 6F 77 6D D6 " +
            "F6 8C 6A B5 7F 52 67 B8 16 66 B1 93 19 BF 3E 04 " +
            "BE 59 1F 00 EA 7E 28 A2 43 4F 4B 3D AE EB D8 22 " +
            "79 6E CA 3B B5 1C F6 A5 69 7E 8A CB A7 A1 7B 26 " +
            "C8 1D FE 4D F1 10 0D C0 3C 96 82 52 04 73 9C 07 " +
            "3F 6C B8 E0 91 9D 65 40 53 DB 7B A8 A9 D0 BA 57 " +
            "BC 3F 3B 82 67 D1 78 64 F8 67 73 9A B9 6B A7 13 " +
            "11 37 08 4D D0 D8 9D FB E8 D8 88 D3 7E AF 55 7E " +
            "5F 6B C8 80 BB 52 FD B7 49 FE B8 6A 03 92 F8 01 " +
            "8D 73 C6 AD D6 BE 74 A8 69 0A 9D 75 9A 8B 6F 64 " +
            "4D 0C C3 EC 35 2C E4 CC 30 22 B9 62 69 38 1A 38 " +
            "CB F3 FC 67 20 A9 7B 9D",
            // After Chi  
            "EA A6 BD 37 2D 93 6B 7F 8C F1 60 A8 F4 C0 3C CC " +
            "AF EF 09 D3 01 C7 8A CA 4B 61 BD 06 6F F3 E5 93 " +
            "F2 DD 2A 3D ED 32 77 38 57 60 F1 AE 1D 3E EE 04 " +
            "86 79 9F 02 FB 6A 0E 27 43 5F 4B FD AC 4A D1 20 " +
            "6F 6E FB 2B AD 02 F2 A5 C1 67 84 CB 45 E1 7B 84 " +
            "CB 75 C6 ED 60 9C 6C 80 7C 05 C1 5A 2C 33 06 10 " +
            "93 48 B8 E2 D7 9C 25 60 13 DB BF E5 39 D0 BF D7 " +
            "88 BD 3B 90 63 B2 E8 63 10 AF F3 08 97 4C E7 17 " +
            "06 14 48 4D 51 88 35 7A E8 4C B8 B9 7E 2F 55 7E " +
            "EF 6A 8B 10 03 3B FA A5 48 EE B0 2F 43 02 E0 E9 " +
            "89 77 84 25 F3 9A F4 20 59 28 A5 77 D2 9B 75 54 " +
            "86 DD 87 E9 35 AD 85 49 34 22 BB EA BF 2E 1E 18 " +
            "AB FB E5 37 28 A8 70 D9",
            // After Iota 
            "63 26 BD 37 2D 93 6B FF 8C F1 60 A8 F4 C0 3C CC " +
            "AF EF 09 D3 01 C7 8A CA 4B 61 BD 06 6F F3 E5 93 " +
            "F2 DD 2A 3D ED 32 77 38 57 60 F1 AE 1D 3E EE 04 " +
            "86 79 9F 02 FB 6A 0E 27 43 5F 4B FD AC 4A D1 20 " +
            "6F 6E FB 2B AD 02 F2 A5 C1 67 84 CB 45 E1 7B 84 " +
            "CB 75 C6 ED 60 9C 6C 80 7C 05 C1 5A 2C 33 06 10 " +
            "93 48 B8 E2 D7 9C 25 60 13 DB BF E5 39 D0 BF D7 " +
            "88 BD 3B 90 63 B2 E8 63 10 AF F3 08 97 4C E7 17 " +
            "06 14 48 4D 51 88 35 7A E8 4C B8 B9 7E 2F 55 7E " +
            "EF 6A 8B 10 03 3B FA A5 48 EE B0 2F 43 02 E0 E9 " +
            "89 77 84 25 F3 9A F4 20 59 28 A5 77 D2 9B 75 54 " +
            "86 DD 87 E9 35 AD 85 49 34 22 BB EA BF 2E 1E 18 " +
            "AB FB E5 37 28 A8 70 D9",
            // Round #15  
            // After Theta
            "68 56 DA DC CC 4D F6 BA C9 C8 17 C8 A3 01 9B FB " +
            "5E 67 49 7C 2F 25 66 A7 EB 2C F8 67 1E F7 A2 F0 " +
            "D2 97 18 BC C2 C8 CE FD 5C 10 96 45 FC E0 73 41 " +
            "C3 40 E8 62 AC AB A9 10 B2 D7 0B 52 82 A8 3D 4D " +
            "CF 23 BE 4A DC 06 B5 C6 E1 2D B6 4A 6A 1B C2 41 " +
            "C0 05 A1 06 81 42 F1 C5 39 3C B6 3A 7B F2 A1 27 " +
            "62 C0 F8 4D F9 7E C9 0D B3 96 FA 84 48 D4 F8 B4 " +
            "A8 F7 09 11 4C 48 51 A6 1B DF 94 E3 76 92 7A 52 " +
            "43 2D 3F 2D 06 49 92 4D 19 C4 F8 16 50 CD B9 13 " +
            "4F 27 CE 71 72 3F BD C6 68 A4 82 AE 6C F8 59 2C " +
            "82 07 E3 CE 12 44 69 65 1C 11 D2 17 85 5A D2 63 " +
            "77 55 C7 46 1B 4F 69 24 94 6F FE 8B CE 2A 59 7B " +
            "8B B1 D7 B6 07 52 C9 1C",
            // After Rho  
            "68 56 DA DC CC 4D F6 BA 93 91 2F 90 47 03 36 F7 " +
            "D7 59 12 DF 4B 89 D9 A9 71 2F 0A BF CE 82 7F E6 " +
            "46 76 EE 97 BE C4 E0 15 C4 0F 3E 17 C4 05 61 59 " +
            "2E C6 BA 9A 0A 31 0C 84 93 EC F5 82 94 20 6A 4F " +
            "11 5F 25 6E 83 5A E3 E7 21 1C 14 DE 62 AB A4 B6 " +
            "06 2E 08 35 08 14 8A 2F 9E E4 F0 D8 EA EC C9 87 " +
            "6F CA F7 4B 6E 10 03 C6 A8 F1 69 67 2D F5 09 91 " +
            "08 26 A4 28 53 D4 FB 84 C7 ED 24 F5 A4 36 BE 29 " +
            "A7 C5 20 49 B2 69 A8 E5 DC 89 0C 62 7C 0B A8 E6 " +
            "A7 D7 F8 E9 C4 39 4E EE 2C 68 A4 82 AE 6C F8 59 " +
            "A5 95 09 1E 8C 3B 4B 10 71 44 48 5F 14 6A 49 8F " +
            "AE EA D8 68 E3 29 8D E4 6F FE 8B CE 2A 59 7B 94 " +
            "32 C7 62 EC B5 ED 81 54",
            // After Pi
            "68 56 DA DC CC 4D F6 BA 2E C6 BA 9A 0A 31 0C 84 " +
            "6F CA F7 4B 6E 10 03 C6 A7 D7 F8 E9 C4 39 4E EE " +
            "32 C7 62 EC B5 ED 81 54 71 2F 0A BF CE 82 7F E6 " +
            "21 1C 14 DE 62 AB A4 B6 06 2E 08 35 08 14 8A 2F " +
            "A7 C5 20 49 B2 69 A8 E5 AE EA D8 68 E3 29 8D E4 " +
            "93 91 2F 90 47 03 36 F7 93 EC F5 82 94 20 6A 4F " +
            "A8 F1 69 67 2D F5 09 91 2C 68 A4 82 AE 6C F8 59 " +
            "A5 95 09 1E 8C 3B 4B 10 46 76 EE 97 BE C4 E0 15 " +
            "C4 0F 3E 17 C4 05 61 59 9E E4 F0 D8 EA EC C9 87 " +
            "DC 89 0C 62 7C 0B A8 E6 6F FE 8B CE 2A 59 7B 94 " +
            "D7 59 12 DF 4B 89 D9 A9 11 5F 25 6E 83 5A E3 E7 " +
            "08 26 A4 28 53 D4 FB 84 C7 ED 24 F5 A4 36 BE 29 " +
            "71 44 48 5F 14 6A 49 8F",
            // After Chi  
            "29 5E 9F 9D A8 4D F5 F8 AE D3 B2 3A 8A 18 40 AC " +
            "7F CA F5 4F 5F D4 82 D6 EF C7 60 F9 8C 39 38 44 " +
            "34 47 42 EE B7 DD 89 50 77 0D 02 9E C6 96 75 EF " +
            "80 DD 34 96 D0 C2 84 76 0E 04 D0 15 49 14 8F 2F " +
            "F6 C0 22 DE BE EB DA E7 AE FA CC 28 C3 00 0D F4 " +
            "BB 80 27 F5 6E D6 37 67 97 E4 71 02 16 28 9A 07 " +
            "29 64 60 7B 2D E6 0A 91 3E 68 82 02 ED 6C CC BE " +
            "A5 F9 D9 1C 1C 1B 03 18 5C 96 2E 5F 94 2C 68 93 " +
            "84 06 32 35 D0 06 41 39 BD 92 73 54 E8 BC 9A 97 " +
            "DC 89 68 73 E8 8F 28 E7 EF F7 9B CE 6A 58 7A DC " +
            "DF 79 92 DF 1B 0D C1 A9 D6 96 25 BB 27 78 E7 CE " +
            "38 26 EC 22 43 9C BA 02 41 F4 36 75 EF B7 2E 09 " +
            "71 42 6D 7F 94 38 6B C9",
            // After Iota 
            "2A DE 9F 9D A8 4D F5 78 AE D3 B2 3A 8A 18 40 AC " +
            "7F CA F5 4F 5F D4 82 D6 EF C7 60 F9 8C 39 38 44 " +
            "34 47 42 EE B7 DD 89 50 77 0D 02 9E C6 96 75 EF " +
            "80 DD 34 96 D0 C2 84 76 0E 04 D0 15 49 14 8F 2F " +
            "F6 C0 22 DE BE EB DA E7 AE FA CC 28 C3 00 0D F4 " +
            "BB 80 27 F5 6E D6 37 67 97 E4 71 02 16 28 9A 07 " +
            "29 64 60 7B 2D E6 0A 91 3E 68 82 02 ED 6C CC BE " +
            "A5 F9 D9 1C 1C 1B 03 18 5C 96 2E 5F 94 2C 68 93 " +
            "84 06 32 35 D0 06 41 39 BD 92 73 54 E8 BC 9A 97 " +
            "DC 89 68 73 E8 8F 28 E7 EF F7 9B CE 6A 58 7A DC " +
            "DF 79 92 DF 1B 0D C1 A9 D6 96 25 BB 27 78 E7 CE " +
            "38 26 EC 22 43 9C BA 02 41 F4 36 75 EF B7 2E 09 " +
            "71 42 6D 7F 94 38 6B C9",
            // Round #16  
            // After Theta
            "5D DA FE B7 48 F2 92 84 70 52 00 E3 25 39 10 9C " +
            "E1 95 29 28 54 55 2B 1A 71 3A F9 79 30 72 32 EA " +
            "45 2D D1 21 71 02 9D 37 00 09 63 B4 26 29 12 13 " +
            "5E 5C 86 4F 7F E3 D4 46 90 5B 0C 72 42 95 26 E3 " +
            "68 3D BB 5E 02 A0 D0 49 DF 90 5F E7 05 DF 19 93 " +
            "CC 84 46 DF 8E 69 50 9B 49 65 C3 DB B9 09 CA 37 " +
            "B7 3B BC 1C 26 67 A3 5D A0 95 1B 82 51 27 C6 10 " +
            "D4 93 4A D3 DA C4 17 7F 2B 92 4F 75 74 93 0F 6F " +
            "5A 87 80 EC 7F 27 11 09 23 CD AF 33 E3 3D 33 5B " +
            "42 74 F1 F3 54 C4 22 49 9E 9D 08 01 AC 87 6E BB " +
            "A8 7D F3 F5 FB B2 A6 55 08 17 97 62 88 59 B7 FE " +
            "A6 79 30 45 48 1D 13 CE DF 09 AF F5 53 FC 24 A7 " +
            "00 28 FE B0 52 E7 7F AE",
            // After Rho  
            "5D DA FE B7 48 F2 92 84 E1 A4 00 C6 4B 72 20 38 " +
            "78 65 0A 0A 55 D5 8A 46 23 27 A3 1E A7 93 9F 07 " +
            "13 E8 BC 29 6A 89 0E 89 6B 92 22 31 01 90 30 46 " +
            "F8 F4 37 4E 6D E4 C5 65 38 E4 16 83 9C 50 A5 C9 " +
            "9E 5D 2F 01 50 E8 24 B4 9D 31 F9 0D F9 75 5E F0 " +
            "64 26 34 FA 76 4C 83 DA DF 24 95 0D 6F E7 26 28 " +
            "E5 30 39 1B ED BA DD E1 4E 8C 21 40 2B 37 04 A3 " +
            "69 6D E2 8B 3F EA 49 A5 EA E8 26 1F DE 56 24 9F " +
            "90 FD EF 24 22 41 EB 10 99 AD 91 E6 D7 99 F1 9E " +
            "58 24 49 88 2E 7E 9E 8A BB 9E 9D 08 01 AC 87 6E " +
            "9A 56 A1 F6 CD D7 EF CB 23 5C 5C 8A 21 66 DD FA " +
            "34 0F A6 08 A9 63 C2 D9 09 AF F5 53 FC 24 A7 DF " +
            "9F 2B 00 8A 3F AC D4 F9",
            // After Pi
            "5D DA FE B7 48 F2 92 84 F8 F4 37 4E 6D E4 C5 65 " +
            "E5 30 39 1B ED BA DD E1 58 24 49 88 2E 7E 9E 8A " +
            "9F 2B 00 8A 3F AC D4 F9 23 27 A3 1E A7 93 9F 07 " +
            "9D 31 F9 0D F9 75 5E F0 64 26 34 FA 76 4C 83 DA " +
            "90 FD EF 24 22 41 EB 10 34 0F A6 08 A9 63 C2 D9 " +
            "E1 A4 00 C6 4B 72 20 38 38 E4 16 83 9C 50 A5 C9 " +
            "4E 8C 21 40 2B 37 04 A3 BB 9E 9D 08 01 AC 87 6E " +
            "9A 56 A1 F6 CD D7 EF CB 13 E8 BC 29 6A 89 0E 89 " +
            "6B 92 22 31 01 90 30 46 DF 24 95 0D 6F E7 26 28 " +
            "99 AD 91 E6 D7 99 F1 9E 09 AF F5 53 FC 24 A7 DF " +
            "78 65 0A 0A 55 D5 8A 46 9E 5D 2F 01 50 E8 24 B4 " +
            "69 6D E2 8B 3F EA 49 A5 EA E8 26 1F DE 56 24 9F " +
            "23 5C 5C 8A 21 66 DD FA",
            // After Chi  
            "58 DA F6 A6 C8 E8 8A 04 E0 F0 77 CE 6F A0 C7 6F " +
            "62 3B 39 19 FC 3A 9D 90 18 F4 B7 BD 6E 2C 9C 8E " +
            "3F 0F 01 C2 1A A8 91 98 43 21 A7 EC A1 9B 1E 0D " +
            "0D E8 32 09 F9 74 36 F0 40 24 34 F2 FF 6E 83 13 " +
            "93 DD EE 32 24 D1 F6 16 A8 1F FE 09 F1 07 82 29 " +
            "A7 AC 21 86 68 55 20 1A 89 F6 8A 8B 9C D8 26 85 " +
            "4E CC 01 B6 E7 64 6C 22 DA 3E 9D 08 03 8C 87 5E " +
            "82 16 B7 F7 59 D7 6A 0A 87 CC 29 25 04 EE 08 A1 " +
            "6B 1B 22 D3 91 88 E1 D0 DF 26 F1 1C 47 C3 20 69 " +
            "8B ED 99 CE D5 10 F9 9E 61 BD F7 43 FD 34 97 99 " +
            "19 45 CA 80 7A D7 C3 47 1C DD 2B 15 90 FC 00 AE " +
            "68 79 BA 0B 1E CA 90 C5 B2 C9 24 1F 8A C7 26 9B " +
            "A5 44 79 8B 21 4E F9 4A",
            // After Iota 
            "5A 5A F6 A6 C8 E8 8A 84 E0 F0 77 CE 6F A0 C7 6F " +
            "62 3B 39 19 FC 3A 9D 90 18 F4 B7 BD 6E 2C 9C 8E " +
            "3F 0F 01 C2 1A A8 91 98 43 21 A7 EC A1 9B 1E 0D " +
            "0D E8 32 09 F9 74 36 F0 40 24 34 F2 FF 6E 83 13 " +
            "93 DD EE 32 24 D1 F6 16 A8 1F FE 09 F1 07 82 29 " +
            "A7 AC 21 86 68 55 20 1A 89 F6 8A 8B 9C D8 26 85 " +
            "4E CC 01 B6 E7 64 6C 22 DA 3E 9D 08 03 8C 87 5E " +
            "82 16 B7 F7 59 D7 6A 0A 87 CC 29 25 04 EE 08 A1 " +
            "6B 1B 22 D3 91 88 E1 D0 DF 26 F1 1C 47 C3 20 69 " +
            "8B ED 99 CE D5 10 F9 9E 61 BD F7 43 FD 34 97 99 " +
            "19 45 CA 80 7A D7 C3 47 1C DD 2B 15 90 FC 00 AE " +
            "68 79 BA 0B 1E CA 90 C5 B2 C9 24 1F 8A C7 26 9B " +
            "A5 44 79 8B 21 4E F9 4A",
            // Round #17  
            // After Theta
            "AD F5 BC 47 B1 1A F1 24 76 B7 6B 33 6A CC 3C 01 " +
            "A0 75 0D 3F DB 0E CE 72 61 87 7D 1E 0E 11 70 53 " +
            "17 80 5E 47 F2 30 5D B1 B4 8E ED 0D D8 69 65 AD " +
            "9B AF 2E F4 FC 18 CD 9E 82 6A 00 D4 D8 5A D0 F1 " +
            "EA AE 24 91 44 EC 1A CB 80 90 A1 8C 19 9F 4E 00 " +
            "50 03 6B 67 11 A7 5B BA 1F B1 96 76 99 B4 DD EB " +
            "8C 82 35 90 C0 50 3F C0 A3 4D 57 AB 63 B1 6B 83 " +
            "AA 99 E8 72 B1 4F A6 23 70 63 63 C4 7D 1C 73 01 " +
            "FD 5C 3E 2E 94 E4 1A BE 1D 68 C5 3A 60 F7 73 8B " +
            "F2 9E 53 6D B5 2D 15 43 49 32 A8 C6 15 AC 5B B0 " +
            "EE EA 80 61 03 25 B8 E7 8A 9A 37 E8 95 90 FB C0 " +
            "AA 37 8E 2D 39 FE C3 27 CB BA EE BC EA FA CA 46 " +
            "8D CB 26 0E C9 D6 35 63",
            // After Rho  
            "AD F5 BC 47 B1 1A F1 24 EC 6E D7 66 D4 98 79 02 " +
            "68 5D C3 CF B6 83 B3 1C 10 01 37 15 76 D8 E7 E1 " +
            "87 E9 8A BD 00 F4 3A 92 80 9D 56 D6 4A EB D8 DE " +
            "42 CF 8F D1 EC B9 F9 EA BC A0 1A 00 35 B6 16 74 " +
            "57 92 48 22 76 8D 65 75 E9 04 00 08 19 CA 98 F1 " +
            "85 1A 58 3B 8B 38 DD D2 AF 7F C4 5A DA 65 D2 76 " +
            "81 04 86 FA 01 66 14 AC 62 D7 06 47 9B AE 56 C7 " +
            "B9 D8 27 D3 11 D5 4C 74 88 FB 38 E6 02 E0 C6 C6 " +
            "C7 85 92 5C C3 B7 9F CB B9 C5 0E B4 62 1D B0 FB " +
            "A5 62 48 DE 73 AA AD B6 B0 49 32 A8 C6 15 AC 5B " +
            "E0 9E BB AB 03 86 0D 94 2B 6A DE A0 57 42 EE 03 " +
            "F5 C6 B1 25 C7 7F F8 44 BA EE BC EA FA CA 46 CB " +
            "CD 58 E3 B2 89 43 B2 75",
            // After Pi
            "AD F5 BC 47 B1 1A F1 24 42 CF 8F D1 EC B9 F9 EA " +
            "81 04 86 FA 01 66 14 AC A5 62 48 DE 73 AA AD B6 " +
            "CD 58 E3 B2 89 43 B2 75 10 01 37 15 76 D8 E7 E1 " +
            "E9 04 00 08 19 CA 98 F1 85 1A 58 3B 8B 38 DD D2 " +
            "C7 85 92 5C C3 B7 9F CB F5 C6 B1 25 C7 7F F8 44 " +
            "EC 6E D7 66 D4 98 79 02 BC A0 1A 00 35 B6 16 74 " +
            "62 D7 06 47 9B AE 56 C7 B0 49 32 A8 C6 15 AC 5B " +
            "E0 9E BB AB 03 86 0D 94 87 E9 8A BD 00 F4 3A 92 " +
            "80 9D 56 D6 4A EB D8 DE AF 7F C4 5A DA 65 D2 76 " +
            "B9 C5 0E B4 62 1D B0 FB BA EE BC EA FA CA 46 CB " +
            "68 5D C3 CF B6 83 B3 1C 57 92 48 22 76 8D 65 75 " +
            "B9 D8 27 D3 11 D5 4C 74 88 FB 38 E6 02 E0 C6 C6 " +
            "2B 6A DE A0 57 42 EE 03",
            // After Chi  
            "2C F5 BC 6D B0 5C F5 20 66 AD C7 D5 9E 31 50 F8 " +
            "C9 1C 25 DA 89 27 06 ED 85 C7 54 9B 43 B2 EC B6 " +
            "8F 52 E0 22 C5 E2 BA BF 14 1B 6F 26 F4 E8 A2 E3 " +
            "AB 81 82 4C 59 4D 9A F8 B5 58 79 1A 8F 70 BD D6 " +
            "C7 84 94 4C F3 37 98 6A 1C C2 B1 2D CE 7D E0 54 " +
            "AE 39 D3 21 5E 90 39 81 2C A8 2A A8 71 A7 BE 6C " +
            "22 41 8F 44 9A 2C 57 43 BC 29 76 EC 12 0D DC 59 " +
            "F0 1E B3 AB 22 A0 0B E0 A8 8B 0A B5 90 F0 38 B2 " +
            "90 1D 5C 72 6A F3 F8 57 AD 55 74 10 42 A7 94 76 " +
            "BC C4 0C A1 62 29 88 EB BA FA E8 A8 B0 C1 86 87 " +
            "C0 15 E4 1E B7 D3 BB 1C 57 B1 50 06 74 AD E7 F7 " +
            "9A D8 E1 D3 44 D7 64 75 C8 EE 39 A9 A2 61 D7 DA " +
            "3C E8 D6 80 17 4E AA 62",
            // After Iota 
            "AC F5 BC 6D B0 5C F5 A0 66 AD C7 D5 9E 31 50 F8 " +
            "C9 1C 25 DA 89 27 06 ED 85 C7 54 9B 43 B2 EC B6 " +
            "8F 52 E0 22 C5 E2 BA BF 14 1B 6F 26 F4 E8 A2 E3 " +
            "AB 81 82 4C 59 4D 9A F8 B5 58 79 1A 8F 70 BD D6 " +
            "C7 84 94 4C F3 37 98 6A 1C C2 B1 2D CE 7D E0 54 " +
            "AE 39 D3 21 5E 90 39 81 2C A8 2A A8 71 A7 BE 6C " +
            "22 41 8F 44 9A 2C 57 43 BC 29 76 EC 12 0D DC 59 " +
            "F0 1E B3 AB 22 A0 0B E0 A8 8B 0A B5 90 F0 38 B2 " +
            "90 1D 5C 72 6A F3 F8 57 AD 55 74 10 42 A7 94 76 " +
            "BC C4 0C A1 62 29 88 EB BA FA E8 A8 B0 C1 86 87 " +
            "C0 15 E4 1E B7 D3 BB 1C 57 B1 50 06 74 AD E7 F7 " +
            "9A D8 E1 D3 44 D7 64 75 C8 EE 39 A9 A2 61 D7 DA " +
            "3C E8 D6 80 17 4E AA 62",
            // Round #18  
            // After Theta
            "04 39 A6 6B 6E E7 5F D6 CA F4 A4 9A 97 21 85 62 " +
            "FA B5 40 F8 E5 22 82 48 27 76 AB C5 C4 D8 0B 11 " +
            "F9 80 BF 92 DC 2C 97 D2 BC D7 75 20 2A 53 08 95 " +
            "07 D8 E1 03 50 5D 4F 62 86 F1 1C 38 E3 75 39 73 " +
            "65 35 6B 12 74 5D 7F CD 6A 10 EE 9D D7 B3 CD 39 " +
            "06 F5 C9 27 80 2B 93 F7 80 F1 49 E7 78 B7 6B F6 " +
            "11 E8 EA 66 F6 29 D3 E6 1E 98 89 B2 95 67 3B FE " +
            "86 CC EC 1B 3B 6E 26 8D 00 47 10 B3 4E 4B 92 C4 " +
            "3C 44 3F 3D 63 E3 2D CD 9E FC 11 32 2E A2 10 D3 " +
            "1E 75 F3 FF E5 43 6F 4C CC 28 B7 18 A9 0F AB EA " +
            "68 D9 FE 18 69 68 11 6A FB E8 33 49 7D BD 32 6D " +
            "A9 71 84 F1 28 D2 E0 D0 6A 5F C6 F7 25 0B 30 7D " +
            "4A 3A 89 30 0E 80 87 0F",
            // After Rho  
            "04 39 A6 6B 6E E7 5F D6 94 E9 49 35 2F 43 0A C5 " +
            "7E 2D 10 7E B9 88 20 92 8C BD 10 71 62 B7 5A 4C " +
            "66 B9 94 CE 07 FC 95 E4 A2 32 85 50 C9 7B 5D 07 " +
            "3E 00 D5 F5 24 76 80 1D 9C 61 3C 07 CE 78 5D CE " +
            "9A 35 09 BA AE BF E6 B2 DB 9C A3 06 E1 DE 79 3D " +
            "37 A8 4F 3E 01 5C 99 BC D9 03 C6 27 9D E3 DD AE " +
            "37 B3 4F 99 36 8F 40 57 CF 76 FC 3D 30 13 65 2B " +
            "8D 1D 37 93 46 43 66 F6 66 9D 96 24 89 01 8E 20 " +
            "A7 67 6C BC A5 99 87 E8 88 69 4F FE 08 19 17 51 " +
            "E8 8D C9 A3 6E FE BF 7C EA CC 28 B7 18 A9 0F AB " +
            "45 A8 A1 65 FB 63 A4 A1 ED A3 CF 24 F5 F5 CA B4 " +
            "35 8E 30 1E 45 1A 1C 3A 5F C6 F7 25 0B 30 7D 6A " +
            "E1 83 92 4E 22 8C 03 E0",
            // After Pi
            "04 39 A6 6B 6E E7 5F D6 3E 00 D5 F5 24 76 80 1D " +
            "37 B3 4F 99 36 8F 40 57 E8 8D C9 A3 6E FE BF 7C " +
            "E1 83 92 4E 22 8C 03 E0 8C BD 10 71 62 B7 5A 4C " +
            "DB 9C A3 06 E1 DE 79 3D 37 A8 4F 3E 01 5C 99 BC " +
            "A7 67 6C BC A5 99 87 E8 35 8E 30 1E 45 1A 1C 3A " +
            "94 E9 49 35 2F 43 0A C5 9C 61 3C 07 CE 78 5D CE " +
            "CF 76 FC 3D 30 13 65 2B EA CC 28 B7 18 A9 0F AB " +
            "45 A8 A1 65 FB 63 A4 A1 66 B9 94 CE 07 FC 95 E4 " +
            "A2 32 85 50 C9 7B 5D 07 D9 03 C6 27 9D E3 DD AE " +
            "88 69 4F FE 08 19 17 51 5F C6 F7 25 0B 30 7D 6A " +
            "7E 2D 10 7E B9 88 20 92 9A 35 09 BA AE BF E6 B2 " +
            "8D 1D 37 93 46 43 66 F6 66 9D 96 24 89 01 8E 20 " +
            "ED A3 CF 24 F5 F5 CA B4",
            // After Chi  
            "05 8A AC 63 7C 6E 1F 94 F6 0C 55 D7 6C 06 3F 35 " +
            "36 B1 5D D5 36 8F 40 D7 EC B5 ED 82 22 9D E3 6A " +
            "DB 83 C3 DA 22 9C 83 E9 A8 9D 5C 49 62 B7 DA CC " +
            "5B DB 83 86 45 5F 7F 7D 27 20 5F 3C 41 5E 81 AE " +
            "2F 56 6C DD 87 3C C5 AC 66 8E 93 18 C4 52 3D 0B " +
            "D7 FF 89 0D 1F 40 2A E4 BC E9 3C 85 C6 D0 57 4E " +
            "CA 56 7D 7D D3 51 C5 2B 7A 8D 60 A7 1C A9 05 EF " +
            "4D A8 95 67 3B 5B F1 AB 3F B8 D6 E9 13 7C 15 4C " +
            "A2 5A 8C 88 C9 63 5F 56 8E 85 76 26 9E C3 B5 84 " +
            "A8 50 4F 34 0C D5 97 D5 DF C4 F6 35 C3 33 35 69 " +
            "7B 25 26 7F F9 C8 20 D6 F8 B5 89 9E 27 BF 6E B2 " +
            "04 3F 7E 93 32 B7 26 62 74 91 86 7E 81 09 AE 22 " +
            "6D B3 C6 A4 F3 C2 0C 94",
            // After Iota 
            "0F 0A AC 63 7C 6E 1F 94 F6 0C 55 D7 6C 06 3F 35 " +
            "36 B1 5D D5 36 8F 40 D7 EC B5 ED 82 22 9D E3 6A " +
            "DB 83 C3 DA 22 9C 83 E9 A8 9D 5C 49 62 B7 DA CC " +
            "5B DB 83 86 45 5F 7F 7D 27 20 5F 3C 41 5E 81 AE " +
            "2F 56 6C DD 87 3C C5 AC 66 8E 93 18 C4 52 3D 0B " +
            "D7 FF 89 0D 1F 40 2A E4 BC E9 3C 85 C6 D0 57 4E " +
            "CA 56 7D 7D D3 51 C5 2B 7A 8D 60 A7 1C A9 05 EF " +
            "4D A8 95 67 3B 5B F1 AB 3F B8 D6 E9 13 7C 15 4C " +
            "A2 5A 8C 88 C9 63 5F 56 8E 85 76 26 9E C3 B5 84 " +
            "A8 50 4F 34 0C D5 97 D5 DF C4 F6 35 C3 33 35 69 " +
            "7B 25 26 7F F9 C8 20 D6 F8 B5 89 9E 27 BF 6E B2 " +
            "04 3F 7E 93 32 B7 26 62 74 91 86 7E 81 09 AE 22 " +
            "6D B3 C6 A4 F3 C2 0C 94",
            // Round #19  
            // After Theta
            "DA 7A 86 D2 92 A0 25 E4 61 03 32 24 97 C3 CA 7A " +
            "B6 3E E3 73 5E 72 53 89 38 6C 71 CA F0 A0 98 B6 " +
            "D6 C6 F8 0B C1 13 2D 7A 7D ED 76 F8 8C 79 E0 BC " +
            "CC D4 E4 75 BE 9A 8A 32 A7 AF E1 9A 29 A3 92 F0 " +
            "FB 8F F0 95 55 01 BE 70 6B CB A8 C9 27 DD 93 98 " +
            "02 8F A3 BC F1 8E 10 94 2B E6 5B 76 3D 15 A2 01 " +
            "4A D9 C3 DB BB AC D6 75 AE 54 FC EF CE 94 7E 33 " +
            "40 ED AE B6 D8 D4 5F 38 EA C8 FC 58 FD B2 2F 3C " +
            "35 55 EB 7B 32 A6 AA 19 0E 0A C8 80 F6 3E A6 DA " +
            "7C 89 D3 7C DE E8 EC 09 D2 81 CD E4 20 BC 9B FA " +
            "AE 55 0C CE 17 06 1A A6 6F BA EE 6D DC 7A 9B FD " +
            "84 B0 C0 35 5A 4A 35 3C A0 48 1A 36 53 34 D5 FE " +
            "60 F6 FD 75 10 4D A2 07",
            // After Rho  
            "DA 7A 86 D2 92 A0 25 E4 C2 06 64 48 2E 87 95 F5 " +
            "AD CF F8 9C 97 DC 54 A2 0F 8A 69 8B C3 16 A7 0C " +
            "9E 68 D1 B3 36 C6 5F 08 CF 98 07 CE DB D7 6E 87 " +
            "5E E7 AB A9 28 C3 4C 4D FC E9 6B B8 66 CA A8 24 " +
            "47 F8 CA AA 00 5F B8 FD 3D 89 B9 B6 8C 9A 7C D2 " +
            "14 78 1C E5 8D 77 84 A0 06 AC 98 6F D9 F5 54 88 " +
            "DE DE 65 B5 AE 53 CA 1E 29 FD 66 5C A9 F8 DF 9D " +
            "5B 6C EA 2F 1C A0 76 57 B1 FA 65 5F 78 D4 91 F9 " +
            "7D 4F C6 54 35 A3 A6 6A 53 6D 07 05 64 40 7B 1F " +
            "9D 3D 81 2F 71 9A CF 1B FA D2 81 CD E4 20 BC 9B " +
            "68 98 BA 56 31 38 5F 18 BF E9 BA B7 71 EB 6D F6 " +
            "10 16 B8 46 4B A9 86 87 48 1A 36 53 34 D5 FE A0 " +
            "E8 01 98 7D 7F 1D 44 93",
            // After Pi
            "DA 7A 86 D2 92 A0 25 E4 5E E7 AB A9 28 C3 4C 4D " +
            "DE DE 65 B5 AE 53 CA 1E 9D 3D 81 2F 71 9A CF 1B " +
            "E8 01 98 7D 7F 1D 44 93 0F 8A 69 8B C3 16 A7 0C " +
            "3D 89 B9 B6 8C 9A 7C D2 14 78 1C E5 8D 77 84 A0 " +
            "7D 4F C6 54 35 A3 A6 6A 10 16 B8 46 4B A9 86 87 " +
            "C2 06 64 48 2E 87 95 F5 FC E9 6B B8 66 CA A8 24 " +
            "29 FD 66 5C A9 F8 DF 9D FA D2 81 CD E4 20 BC 9B " +
            "68 98 BA 56 31 38 5F 18 9E 68 D1 B3 36 C6 5F 08 " +
            "CF 98 07 CE DB D7 6E 87 06 AC 98 6F D9 F5 54 88 " +
            "53 6D 07 05 64 40 7B 1F 48 1A 36 53 34 D5 FE A0 " +
            "AD CF F8 9C 97 DC 54 A2 47 F8 CA AA 00 5F B8 FD " +
            "5B 6C EA 2F 1C A0 76 57 B1 FA 65 5F 78 D4 91 F9 " +
            "BF E9 BA B7 71 EB 6D F6",
            // After Chi  
            "5A 62 C2 C6 14 B0 A7 F6 5F C6 2B A3 79 4B 49 4C " +
            "BE DE 7D E5 A0 56 CA 9E 8F 47 87 AD F1 3A EE 7F " +
            "EC 84 B1 54 57 5E 0C 9A 0F FA 6D CA C2 73 27 2C " +
            "54 8E 7B A6 BC 1A 5E 98 14 68 24 E7 C7 7F 84 25 " +
            "72 C7 87 DD B5 B5 87 62 20 17 28 72 47 21 DE 55 " +
            "C3 12 60 0C A7 B7 C2 6C 2E EB EA 39 22 CA 88 26 " +
            "29 F5 5C 4E B8 E0 9C 9D 78 D4 C5 C5 EA A7 3C 7E " +
            "54 71 B1 E6 71 70 77 18 9E 4C 49 92 36 E6 4F 00 " +
            "9E D9 00 CE FF D7 45 90 0E BE A8 3D C9 60 D0 28 " +
            "C5 0D C6 A5 66 42 7A 17 09 8A 30 1F FD C4 DE 27 " +
            "B5 CB D8 99 8B 7C 12 A0 E7 6A CF FA 60 0B 39 55 " +
            "55 6D 70 8F 1D 8B 1A 51 B1 FC 25 57 FE C0 81 F9 " +
            "FD D9 B8 95 71 E8 C5 AB",
            // After Iota 
            "50 62 C2 46 14 B0 A7 76 5F C6 2B A3 79 4B 49 4C " +
            "BE DE 7D E5 A0 56 CA 9E 8F 47 87 AD F1 3A EE 7F " +
            "EC 84 B1 54 57 5E 0C 9A 0F FA 6D CA C2 73 27 2C " +
            "54 8E 7B A6 BC 1A 5E 98 14 68 24 E7 C7 7F 84 25 " +
            "72 C7 87 DD B5 B5 87 62 20 17 28 72 47 21 DE 55 " +
            "C3 12 60 0C A7 B7 C2 6C 2E EB EA 39 22 CA 88 26 " +
            "29 F5 5C 4E B8 E0 9C 9D 78 D4 C5 C5 EA A7 3C 7E " +
            "54 71 B1 E6 71 70 77 18 9E 4C 49 92 36 E6 4F 00 " +
            "9E D9 00 CE FF D7 45 90 0E BE A8 3D C9 60 D0 28 " +
            "C5 0D C6 A5 66 42 7A 17 09 8A 30 1F FD C4 DE 27 " +
            "B5 CB D8 99 8B 7C 12 A0 E7 6A CF FA 60 0B 39 55 " +
            "55 6D 70 8F 1D 8B 1A 51 B1 FC 25 57 FE C0 81 F9 " +
            "FD D9 B8 95 71 E8 C5 AB",
            // Round #20  
            // After Theta
            "84 F3 88 1C 09 1D DF 42 58 EA CE D5 A2 E1 66 64 " +
            "01 85 45 63 B4 45 74 B2 8F B5 1B C6 20 5F 8A 97 " +
            "72 3A 2B 05 F8 29 9D 3B DB 6B 27 90 DF DE 5F 18 " +
            "53 A2 9E D0 67 B0 71 B0 AB 33 1C 61 D3 6C 3A 09 " +
            "72 35 1B B6 64 D0 E3 8A BE A9 B2 23 E8 56 4F F4 " +
            "17 83 2A 56 BA 1A BA 58 29 C7 0F 4F F9 60 A7 0E " +
            "96 AE 64 C8 AC F3 22 B1 78 26 59 AE 3B C2 58 96 " +
            "CA CF 2B B7 DE 07 E6 B9 4A DD 03 C8 2B 4B 37 34 " +
            "99 F5 E5 B8 24 7D 6A B8 B1 E5 90 BB DD 73 6E 04 " +
            "C5 FF 5A CE B7 27 1E FF 97 34 AA 4E 52 B3 4F 86 " +
            "61 5A 92 C3 96 D1 6A 94 E0 46 2A 8C BB A1 16 7D " +
            "EA 36 48 09 09 98 A4 7D B1 0E B9 3C 2F A5 E5 11 " +
            "63 67 22 C4 DE 9F 54 0A",
            // After Rho  
            "84 F3 88 1C 09 1D DF 42 B0 D4 9D AB 45 C3 CD C8 " +
            "40 61 D1 18 6D 11 9D 6C F2 A5 78 F9 58 BB 61 0C " +
            "4F E9 DC 91 D3 59 29 C0 F9 ED FD 85 B1 BD 76 02 " +
            "09 7D 06 1B 07 3B 25 EA C2 EA 0C 47 D8 34 9B 4E " +
            "9A 0D 5B 32 E8 71 45 B9 F5 44 EF 9B 2A 3B 82 6E " +
            "BA 18 54 B1 D2 D5 D0 C5 3A A4 1C 3F 3C E5 83 9D " +
            "43 66 9D 17 89 B5 74 25 84 B1 2C F1 4C B2 5C 77 " +
            "5B EF 03 F3 5C E5 E7 95 90 57 96 6E 68 94 BA 07 " +
            "1C 97 A4 4F 0D 37 B3 BE 37 82 D8 72 C8 DD EE 39 " +
            "C4 E3 BF F8 5F CB F9 F6 86 97 34 AA 4E 52 B3 4F " +
            "AB 51 86 69 49 0E 5B 46 81 1B A9 30 EE 86 5A F4 " +
            "DD 06 29 21 01 93 B4 4F 0E B9 3C 2F A5 E5 11 B1 " +
            "95 C2 D8 99 08 B1 F7 27",
            // After Pi
            "84 F3 88 1C 09 1D DF 42 09 7D 06 1B 07 3B 25 EA " +
            "43 66 9D 17 89 B5 74 25 C4 E3 BF F8 5F CB F9 F6 " +
            "95 C2 D8 99 08 B1 F7 27 F2 A5 78 F9 58 BB 61 0C " +
            "F5 44 EF 9B 2A 3B 82 6E BA 18 54 B1 D2 D5 D0 C5 " +
            "1C 97 A4 4F 0D 37 B3 BE DD 06 29 21 01 93 B4 4F " +
            "B0 D4 9D AB 45 C3 CD C8 C2 EA 0C 47 D8 34 9B 4E " +
            "84 B1 2C F1 4C B2 5C 77 86 97 34 AA 4E 52 B3 4F " +
            "AB 51 86 69 49 0E 5B 46 4F E9 DC 91 D3 59 29 C0 " +
            "F9 ED FD 85 B1 BD 76 02 3A A4 1C 3F 3C E5 83 9D " +
            "37 82 D8 72 C8 DD EE 39 0E B9 3C 2F A5 E5 11 B1 " +
            "40 61 D1 18 6D 11 9D 6C 9A 0D 5B 32 E8 71 45 B9 " +
            "5B EF 03 F3 5C E5 E7 95 90 57 96 6E 68 94 BA 07 " +
            "81 1B A9 30 EE 86 5A F4",
            // After Chi  
            "C6 F1 11 18 81 99 8F 47 8D FC 24 F3 51 71 AC 38 " +
            "52 66 DD 16 89 85 72 24 C4 D2 BF FC 5E C7 F1 B6 " +
            "9C CE DE 9A 0E 93 D7 8F F8 BD 68 D9 88 7F 31 8D " +
            "F1 C3 4F D5 27 19 A1 54 7B 18 5D 91 D2 55 D4 84 " +
            "3E 36 F4 97 55 1F F2 BE D8 46 AE 23 23 93 36 2D " +
            "B4 C5 BD 1B 41 41 89 F9 C0 EC 1C 4D DA 74 38 46 " +
            "AD F1 AE B0 4D BE 14 77 96 13 2D 28 4A 93 37 C7 " +
            "E9 7B 86 2D D1 3A 49 40 4D E9 DC AB DF 19 A8 5D " +
            "FC EF 3D C5 71 A5 1A 22 32 9D 38 32 19 C5 92 1D " +
            "76 C2 18 E2 9A C5 C6 79 BE BD 1D 2B 85 41 47 B3 " +
            "01 83 D1 D9 79 95 3F 68 1A 1D CF 3E C8 61 5D BB " +
            "5A E7 2A E3 DA E7 A7 65 D0 37 C6 66 69 85 3F 0F " +
            "1B 17 A3 12 6E E6 1A 65",
            // After Iota 
            "47 71 11 98 81 99 8F C7 8D FC 24 F3 51 71 AC 38 " +
            "52 66 DD 16 89 85 72 24 C4 D2 BF FC 5E C7 F1 B6 " +
            "9C CE DE 9A 0E 93 D7 8F F8 BD 68 D9 88 7F 31 8D " +
            "F1 C3 4F D5 27 19 A1 54 7B 18 5D 91 D2 55 D4 84 " +
            "3E 36 F4 97 55 1F F2 BE D8 46 AE 23 23 93 36 2D " +
            "B4 C5 BD 1B 41 41 89 F9 C0 EC 1C 4D DA 74 38 46 " +
            "AD F1 AE B0 4D BE 14 77 96 13 2D 28 4A 93 37 C7 " +
            "E9 7B 86 2D D1 3A 49 40 4D E9 DC AB DF 19 A8 5D " +
            "FC EF 3D C5 71 A5 1A 22 32 9D 38 32 19 C5 92 1D " +
            "76 C2 18 E2 9A C5 C6 79 BE BD 1D 2B 85 41 47 B3 " +
            "01 83 D1 D9 79 95 3F 68 1A 1D CF 3E C8 61 5D BB " +
            "5A E7 2A E3 DA E7 A7 65 D0 37 C6 66 69 85 3F 0F " +
            "1B 17 A3 12 6E E6 1A 65",
            // Round #21  
            // After Theta
            "FA 6A 53 14 BD B4 9F 95 13 74 94 17 14 C3 02 E1 " +
            "9D 42 28 09 F9 4A 9A E4 38 95 13 40 A4 B1 9D 70 " +
            "D9 0A F4 0C 60 CF 5A 3B 45 A6 2A 55 B4 52 21 DF " +
            "6F 4B FF 31 62 AB 0F 8D B4 3C A8 8E A2 9A 3C 44 " +
            "C2 71 58 2B AF 69 9E 78 9D 82 84 B5 4D CF BB 99 " +
            "09 DE FF 97 7D 6C 99 AB 5E 64 AC A9 9F C6 96 9F " +
            "62 D5 5B AF 3D 71 FC B7 6A 54 81 94 B0 E5 5B 01 " +
            "AC BF AC BB BF 66 C4 F4 F0 F2 9E 27 E3 34 B8 0F " +
            "62 67 8D 21 34 17 B4 FB FD B9 CD 2D 69 0A 7A DD " +
            "8A 85 B4 5E 60 B3 AA BF FB 79 37 BD EB 1D CA 07 " +
            "BC 98 93 55 45 B8 2F 3A 84 95 7F DA 8D D3 F3 62 " +
            "95 C3 DF FC AA 28 4F A5 2C 70 6A DA 93 F3 53 C9 " +
            "5E D3 89 84 00 BA 97 D1",
            // After Rho  
            "FA 6A 53 14 BD B4 9F 95 27 E8 28 2F 28 86 05 C2 " +
            "A7 10 4A 42 BE 92 26 79 1A DB 09 87 53 39 01 44 " +
            "7B D6 DA C9 56 A0 67 00 45 2B 15 F2 5D 64 AA 52 " +
            "1F 23 B6 FA D0 F8 B6 F4 11 2D 0F AA A3 A8 26 0F " +
            "38 AC 95 D7 34 4F 3C E1 BC 9B D9 29 48 58 DB F4 " +
            "4D F0 FE BF EC 63 CB 5C 7E 7A 91 B1 A6 7E 1A 5B " +
            "7A ED 89 E3 BF 15 AB DE CB B7 02 D4 A8 02 29 61 " +
            "DD 5F 33 62 7A D6 5F D6 4F C6 69 70 1F E0 E5 3D " +
            "31 84 E6 82 76 5F EC AC BD EE FE DC E6 96 34 05 " +
            "56 F5 57 B1 90 D6 0B 6C 07 FB 79 37 BD EB 1D CA " +
            "BE E8 F0 62 4E 56 15 E1 11 56 FE 69 37 4E CF 8B " +
            "72 F8 9B 5F 15 E5 A9 B4 70 6A DA 93 F3 53 C9 2C " +
            "65 B4 D7 74 22 21 80 EE",
            // After Pi
            "FA 6A 53 14 BD B4 9F 95 1F 23 B6 FA D0 F8 B6 F4 " +
            "7A ED 89 E3 BF 15 AB DE 56 F5 57 B1 90 D6 0B 6C " +
            "65 B4 D7 74 22 21 80 EE 1A DB 09 87 53 39 01 44 " +
            "BC 9B D9 29 48 58 DB F4 4D F0 FE BF EC 63 CB 5C " +
            "31 84 E6 82 76 5F EC AC 72 F8 9B 5F 15 E5 A9 B4 " +
            "27 E8 28 2F 28 86 05 C2 11 2D 0F AA A3 A8 26 0F " +
            "CB B7 02 D4 A8 02 29 61 07 FB 79 37 BD EB 1D CA " +
            "BE E8 F0 62 4E 56 15 E1 7B D6 DA C9 56 A0 67 00 " +
            "45 2B 15 F2 5D 64 AA 52 7E 7A 91 B1 A6 7E 1A 5B " +
            "BD EE FE DC E6 96 34 05 70 6A DA 93 F3 53 C9 2C " +
            "A7 10 4A 42 BE 92 26 79 38 AC 95 D7 34 4F 3C E1 " +
            "DD 5F 33 62 7A D6 5F D6 4F C6 69 70 1F E0 E5 3D " +
            "11 56 FE 69 37 4E CF 8B",
            // After Chi  
            "9A A6 5A 15 92 B1 96 9F 1B 33 E0 EA D0 3A B6 D4 " +
            "5B ED 09 A7 9D 34 2B 5C CC BF 57 B1 0D 42 14 7D " +
            "60 B5 73 9E 62 69 A0 8E 5B BB 2F 11 F7 1A 01 4C " +
            "8C 9F D9 29 5A 44 FF 54 0F 88 E7 E2 ED C3 CA 4C " +
            "39 87 E6 02 34 47 EC EC D6 F8 4B 77 1D A5 73 04 " +
            "ED 7A 28 7B 20 84 0C A2 15 65 76 89 B6 41 32 85 " +
            "73 B7 82 94 EA 16 29 40 06 FB 71 3A 9D 6B 1D C8 " +
            "AE ED F7 E2 CD 7E 37 EC 41 86 5A C8 F4 BA 77 09 " +
            "C4 AF 7B BE 1D E4 8E 56 3E 7A 91 B2 B7 3F D3 73 " +
            "B6 7A FE 94 E2 36 12 05 74 43 DF A1 FA 17 41 7E " +
            "62 43 68 62 F4 02 65 6F 3A 2C DD C7 31 6F 9C C8 " +
            "CD 4F A5 6B 5A D8 55 54 E9 C6 69 72 97 70 C5 4D " +
            "09 FA 6B FC 37 03 D7 0B",
            // After Iota 
            "1A 26 5A 15 92 B1 96 1F 1B 33 E0 EA D0 3A B6 D4 " +
            "5B ED 09 A7 9D 34 2B 5C CC BF 57 B1 0D 42 14 7D " +
            "60 B5 73 9E 62 69 A0 8E 5B BB 2F 11 F7 1A 01 4C " +
            "8C 9F D9 29 5A 44 FF 54 0F 88 E7 E2 ED C3 CA 4C " +
            "39 87 E6 02 34 47 EC EC D6 F8 4B 77 1D A5 73 04 " +
            "ED 7A 28 7B 20 84 0C A2 15 65 76 89 B6 41 32 85 " +
            "73 B7 82 94 EA 16 29 40 06 FB 71 3A 9D 6B 1D C8 " +
            "AE ED F7 E2 CD 7E 37 EC 41 86 5A C8 F4 BA 77 09 " +
            "C4 AF 7B BE 1D E4 8E 56 3E 7A 91 B2 B7 3F D3 73 " +
            "B6 7A FE 94 E2 36 12 05 74 43 DF A1 FA 17 41 7E " +
            "62 43 68 62 F4 02 65 6F 3A 2C DD C7 31 6F 9C C8 " +
            "CD 4F A5 6B 5A D8 55 54 E9 C6 69 72 97 70 C5 4D " +
            "09 FA 6B FC 37 03 D7 0B",
            // Round #22  
            // After Theta
            "86 AB F3 24 CD 7F 37 3A 3C DE 3E 2F 7B A1 A3 AD " +
            "7F 58 4E 4A 2F D1 26 E5 D2 6A F9 15 84 08 BF 2C " +
            "D3 8F FA 5B 38 6F 81 B0 C7 36 86 20 A8 D4 A0 69 " +
            "AB 72 07 EC F1 DF EA 2D 2B 3D A0 0F 5F 26 C7 F5 " +
            "27 52 48 A6 BD 0D 47 BD 65 C2 C2 B2 47 A3 52 3A " +
            "71 F7 81 4A 7F 4A AD 87 32 88 A8 4C 1D DA 27 FC " +
            "57 02 C5 79 58 F3 24 F9 18 2E DF 9E 14 21 B6 99 " +
            "1D D7 7E 27 97 78 16 D2 DD 0B F3 F9 AB 74 D6 2C " +
            "E3 42 A5 7B B6 7F 9B 2F 1A CF D6 5F 05 DA DE CA " +
            "A8 AF 50 30 6B 7C B9 54 C7 79 56 64 A0 11 60 40 " +
            "FE CE C1 53 AB CC C4 4A 1D C1 03 02 9A F4 89 B1 " +
            "E9 FA E2 86 E8 3D 58 ED F7 13 C7 D6 1E 3A 6E 1C " +
            "BA C0 E2 39 6D 05 F6 35",
            // After Rho  
            "86 AB F3 24 CD 7F 37 3A 79 BC 7D 5E F6 42 47 5B " +
            "1F 96 93 D2 4B B4 49 F9 88 F0 CB 22 AD 96 5F 41 " +
            "79 0B 84 9D 7E D4 DF C2 82 4A 0D 9A 76 6C 63 08 " +
            "C0 1E FF AD DE B2 2A 77 FD 4A 0F E8 C3 97 C9 71 " +
            "29 24 D3 DE 86 A3 DE 13 2A A5 53 26 2C 2C 7B 34 " +
            "8C BB 0F 54 FA 53 6A 3D F0 CB 20 A2 32 75 68 9F " +
            "CE C3 9A 27 C9 BF 12 28 42 6C 33 31 5C BE 3D 29 " +
            "93 4B 3C 0B E9 8E 6B BF F3 57 E9 AC 59 BA 17 E6 " +
            "74 CF F6 6F F3 65 5C A8 6F 65 8D 67 EB AF 02 6D " +
            "2F 97 0A F5 15 0A 66 8D 40 C7 79 56 64 A0 11 60 " +
            "13 2B F9 3B 07 4F AD 32 76 04 0F 08 68 D2 27 C6 " +
            "5D 5F DC 10 BD 07 AB 3D 13 C7 D6 1E 3A 6E 1C F7 " +
            "7D 8D 2E B0 78 4E 5B 81",
            // After Pi
            "86 AB F3 24 CD 7F 37 3A C0 1E FF AD DE B2 2A 77 " +
            "CE C3 9A 27 C9 BF 12 28 2F 97 0A F5 15 0A 66 8D " +
            "7D 8D 2E B0 78 4E 5B 81 88 F0 CB 22 AD 96 5F 41 " +
            "2A A5 53 26 2C 2C 7B 34 8C BB 0F 54 FA 53 6A 3D " +
            "74 CF F6 6F F3 65 5C A8 5D 5F DC 10 BD 07 AB 3D " +
            "79 BC 7D 5E F6 42 47 5B FD 4A 0F E8 C3 97 C9 71 " +
            "42 6C 33 31 5C BE 3D 29 40 C7 79 56 64 A0 11 60 " +
            "13 2B F9 3B 07 4F AD 32 79 0B 84 9D 7E D4 DF C2 " +
            "82 4A 0D 9A 76 6C 63 08 F0 CB 20 A2 32 75 68 9F " +
            "6F 65 8D 67 EB AF 02 6D 13 C7 D6 1E 3A 6E 1C F7 " +
            "1F 96 93 D2 4B B4 49 F9 29 24 D3 DE 86 A3 DE 13 " +
            "93 4B 3C 0B E9 8E 6B BF F3 57 E9 AC 59 BA 17 E6 " +
            "76 04 0F 08 68 D2 27 C6",
            // After Chi  
            "88 6A F3 26 CC 72 27 32 E1 0A FF 7D CA B2 4E F2 " +
            "9E CB BE 27 A1 FB 0B 28 AD B5 DB F1 90 3B 42 B7 " +
            "3D 99 22 39 6A CE 53 C4 0C EA C7 72 7F C5 5F 48 " +
            "5A E1 A3 0D 2D 08 6F B4 85 AB 07 44 F6 51 C9 28 " +
            "F4 6F F5 4D F3 F5 08 E8 7F 5A CC 14 BD 2F 8B 09 " +
            "7B 98 4D 4F EA 6A 73 53 FD C9 47 AE E3 97 C9 31 " +
            "51 44 B3 18 5F F1 91 3B 28 53 7D 12 94 A0 53 29 " +
            "97 69 FB 9B 06 DA 25 12 09 8A A4 BD 7E C5 D7 55 " +
            "8D 6E 80 DF BF E6 61 68 E0 49 72 BA 22 35 74 0D " +
            "07 6D 8D E6 AF 3F C1 6D 91 87 DF 1C 3A 46 3C FF " +
            "8D DD BF D3 22 B8 68 55 49 30 12 7A 96 93 CA 53 " +
            "97 4B 3A 0B C9 CE 4B BF FA C5 79 7E 5A 9E 5F DF " +
            "56 24 4F 04 EC D1 B1 C4",
            // After Iota 
            "89 6A F3 A6 CC 72 27 32 E1 0A FF 7D CA B2 4E F2 " +
            "9E CB BE 27 A1 FB 0B 28 AD B5 DB F1 90 3B 42 B7 " +
            "3D 99 22 39 6A CE 53 C4 0C EA C7 72 7F C5 5F 48 " +
            "5A E1 A3 0D 2D 08 6F B4 85 AB 07 44 F6 51 C9 28 " +
            "F4 6F F5 4D F3 F5 08 E8 7F 5A CC 14 BD 2F 8B 09 " +
            "7B 98 4D 4F EA 6A 73 53 FD C9 47 AE E3 97 C9 31 " +
            "51 44 B3 18 5F F1 91 3B 28 53 7D 12 94 A0 53 29 " +
            "97 69 FB 9B 06 DA 25 12 09 8A A4 BD 7E C5 D7 55 " +
            "8D 6E 80 DF BF E6 61 68 E0 49 72 BA 22 35 74 0D " +
            "07 6D 8D E6 AF 3F C1 6D 91 87 DF 1C 3A 46 3C FF " +
            "8D DD BF D3 22 B8 68 55 49 30 12 7A 96 93 CA 53 " +
            "97 4B 3A 0B C9 CE 4B BF FA C5 79 7E 5A 9E 5F DF " +
            "56 24 4F 04 EC D1 B1 C4",
            // Round #23  
            // After Theta
            "9F 9A 64 FF 91 6E D1 4E E0 09 19 1C 08 53 23 C9 " +
            "05 F4 79 31 88 3D 47 ED B5 81 93 66 7C C3 CF F6 " +
            "45 26 41 E5 63 41 BD 53 1A 1A 50 2B 22 D9 A9 34 " +
            "5B E2 45 6C EF E9 02 8F 1E 94 C0 52 DF 97 85 ED " +
            "EC 5B BD DA 1F 0D 85 A9 07 E5 AF C8 B4 A0 65 9E " +
            "6D 68 DA 16 B7 76 85 2F FC CA A1 CF 21 76 A4 0A " +
            "CA 7B 74 0E 76 37 DD FE 30 67 35 85 78 58 DE 68 " +
            "EF D6 98 47 0F 55 CB 85 1F 7A 33 E4 23 D9 21 29 " +
            "8C 6D 66 BE 7D 07 0C 53 7B 76 B5 AC 0B F3 38 C8 " +
            "1F 59 C5 71 43 C7 4C 2C E9 38 BC C0 33 C9 D2 68 " +
            "9B 2D 28 8A 7F A4 9E 29 48 33 F4 1B 54 72 A7 68 " +
            "0C 74 FD 1D E0 08 07 7A E2 F1 31 E9 B6 66 D2 9E " +
            "2E 9B 2C D8 E5 5E 5F 53",
            // After Rho  
            "9F 9A 64 FF 91 6E D1 4E C1 13 32 38 10 A6 46 92 " +
            "01 7D 5E 0C 62 CF 51 7B 37 FC 6C 5F 1B 38 69 C6 " +
            "0B EA 9D 2A 32 09 2A 1F 22 92 9D 4A A3 A1 01 B5 " +
            "C4 F6 9E 2E F0 B8 25 5E BB 07 25 B0 D4 F7 65 61 " +
            "AD 5E ED 8F 86 C2 54 F6 5A E6 79 50 FE 8A 4C 0B " +
            "69 43 D3 B6 B8 B5 2B 7C 2A F0 2B 87 3E 87 D8 91 " +
            "73 B0 BB E9 F6 57 DE A3 B0 BC D1 60 CE 6A 0A F1 " +
            "A3 87 AA E5 C2 77 6B CC C8 47 B2 43 52 3E F4 66 " +
            "CC B7 EF 80 61 8A B1 CD 1C E4 3D BB 5A D6 85 79 " +
            "98 89 E5 23 AB 38 6E E8 68 E9 38 BC C0 33 C9 D2 " +
            "7A A6 6C B6 A0 28 FE 91 21 CD D0 6F 50 C9 9D A2 " +
            "81 AE BF 03 1C E1 40 8F F1 31 E9 B6 66 D2 9E E2 " +
            "D7 94 CB 26 0B 76 B9 D7",
            // After Pi
            "9F 9A 64 FF 91 6E D1 4E C4 F6 9E 2E F0 B8 25 5E " +
            "73 B0 BB E9 F6 57 DE A3 98 89 E5 23 AB 38 6E E8 " +
            "D7 94 CB 26 0B 76 B9 D7 37 FC 6C 5F 1B 38 69 C6 " +
            "5A E6 79 50 FE 8A 4C 0B 69 43 D3 B6 B8 B5 2B 7C " +
            "CC B7 EF 80 61 8A B1 CD 81 AE BF 03 1C E1 40 8F " +
            "C1 13 32 38 10 A6 46 92 BB 07 25 B0 D4 F7 65 61 " +
            "B0 BC D1 60 CE 6A 0A F1 68 E9 38 BC C0 33 C9 D2 " +
            "7A A6 6C B6 A0 28 FE 91 0B EA 9D 2A 32 09 2A 1F " +
            "22 92 9D 4A A3 A1 01 B5 2A F0 2B 87 3E 87 D8 91 " +
            "1C E4 3D BB 5A D6 85 79 F1 31 E9 B6 66 D2 9E E2 " +
            "01 7D 5E 0C 62 CF 51 7B AD 5E ED 8F 86 C2 54 F6 " +
            "A3 87 AA E5 C2 77 6B CC C8 47 B2 43 52 3E F4 66 " +
            "21 CD D0 6F 50 C9 9D A2",
            // After Chi  
            "AC 9A 45 3E 97 29 0B EF 4C FF DA 2C F9 90 05 16 " +
            "34 A4 B1 ED F6 11 4F B4 90 83 C1 FA 3B 30 2E E0 " +
            "97 F0 51 26 6B E6 9D C7 16 FD EE F9 1B 0D 4A B2 " +
            "DE 52 55 50 BF 80 DC 8A 68 4B C3 B5 A4 D4 6B 7E " +
            "FA E7 AF DC 62 92 98 8D C9 AC AE 03 F8 63 44 86 " +
            "C1 AB E2 78 1A AE 4C 02 F3 46 0D 2C D4 E6 A4 63 " +
            "A2 BA 95 62 EE 62 3C F0 E9 F8 2A B4 D0 B5 C9 D0 " +
            "40 A2 69 36 64 79 DF F0 03 8A BF AF 2E 0F F2 1F " +
            "36 96 89 72 E3 F1 04 DD CB E1 EB 83 1A 87 C2 13 " +
            "16 2E 29 B3 4A DF A5 64 D1 21 E9 F6 E7 72 9F 42 " +
            "03 FC 5C 6C 22 FA 7A 73 E5 1E FD 8D 96 CA C0 D4 " +
            "82 0F EA C9 C2 B6 62 4C C8 77 BC 43 70 38 B4 3F " +
            "8D CF 71 EC D4 C9 99 26",
            // After Iota 
            "A4 1A 45 BE 97 29 0B 6F 4C FF DA 2C F9 90 05 16 " +
            "34 A4 B1 ED F6 11 4F B4 90 83 C1 FA 3B 30 2E E0 " +
            "97 F0 51 26 6B E6 9D C7 16 FD EE F9 1B 0D 4A B2 " +
            "DE 52 55 50 BF 80 DC 8A 68 4B C3 B5 A4 D4 6B 7E " +
            "FA E7 AF DC 62 92 98 8D C9 AC AE 03 F8 63 44 86 " +
            "C1 AB E2 78 1A AE 4C 02 F3 46 0D 2C D4 E6 A4 63 " +
            "A2 BA 95 62 EE 62 3C F0 E9 F8 2A B4 D0 B5 C9 D0 " +
            "40 A2 69 36 64 79 DF F0 03 8A BF AF 2E 0F F2 1F " +
            "36 96 89 72 E3 F1 04 DD CB E1 EB 83 1A 87 C2 13 " +
            "16 2E 29 B3 4A DF A5 64 D1 21 E9 F6 E7 72 9F 42 " +
            "03 FC 5C 6C 22 FA 7A 73 E5 1E FD 8D 96 CA C0 D4 " +
            "82 0F EA C9 C2 B6 62 4C C8 77 BC 43 70 38 B4 3F " +
            "8D CF 71 EC D4 C9 99 26"
        };
    }
}
