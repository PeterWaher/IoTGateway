using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Test vectors available in:
    /// https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHA3-224_1600.pdf
    /// </summary>
    public partial class SHA3_224_Tests
    {
        private static readonly string[] States1600Bits = new string[]
        {
            // Xor'd state (in bytes)
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // Round #0
            // After Theta
            "00 00 00 00 00 00 00 00 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "47 47 47 47 47 47 47 47 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 00 00 00 00 00 00 00 00 " +
            "47 47 47 47 47 47 47 47 47 47 47 47 47 47 47 47 " +
            "A3 A3 A3 A3 A3 A3 A3 A3",
            // After Rho
            "00 00 00 00 00 00 00 00 47 47 47 47 47 47 47 47 " +
            "39 39 39 39 39 39 39 39 4E 4E 4E 4E 4E 4E 4E 4E " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "3A 3A 3A 3A 3A 3A 3A 3A 39 39 39 39 39 39 39 39 " +
            "72 72 72 72 72 72 72 72 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 8E 8E 8E 8E 8E 8E 8E 8E " +
            "27 27 27 27 27 27 27 27 C9 C9 C9 C9 C9 C9 C9 C9 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "74 74 74 74 74 74 74 74 72 72 72 72 72 72 72 72 " +
            "E8 E8 E8 E8 E8 E8 E8 E8 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "8E 8E 8E 8E 8E 8E 8E 8E 00 00 00 00 00 00 00 00 " +
            "E8 E8 E8 E8 E8 E8 E8 E8 47 47 47 47 47 47 47 47 " +
            "E8 E8 E8 E8 E8 E8 E8 E8",
            // After Pi 
            "00 00 00 00 00 00 00 00 3A 3A 3A 3A 3A 3A 3A 3A " +
            "27 27 27 27 27 27 27 27 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "E8 E8 E8 E8 E8 E8 E8 E8 4E 4E 4E 4E 4E 4E 4E 4E " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "74 74 74 74 74 74 74 74 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "47 47 47 47 47 47 47 47 39 39 39 39 39 39 39 39 " +
            "C9 C9 C9 C9 C9 C9 C9 C9 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "8E 8E 8E 8E 8E 8E 8E 8E 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 8E 8E 8E 8E 8E 8E 8E 8E " +
            "72 72 72 72 72 72 72 72 47 47 47 47 47 47 47 47 " +
            "39 39 39 39 39 39 39 39 72 72 72 72 72 72 72 72 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // After Chi
            "05 05 05 05 05 05 05 05 F2 F2 F2 F2 F2 F2 F2 F2 " +
            "27 27 27 27 27 27 27 27 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "D2 D2 D2 D2 D2 D2 D2 D2 4E 4E 4E 4E 4E 4E 4E 4E " +
            "74 74 74 74 74 74 74 74 88 88 88 88 88 88 88 88 " +
            "72 72 72 72 72 72 72 72 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "87 87 87 87 87 87 87 87 1B 1B 1B 1B 1B 1B 1B 1B " +
            "C5 C5 C5 C5 C5 C5 C5 C5 E2 E2 E2 E2 E2 E2 E2 E2 " +
            "B6 B6 B6 B6 B6 B6 B6 B6 8E 8E 8E 8E 8E 8E 8E 8E " +
            "70 70 70 70 70 70 70 70 8B 8B 8B 8B 8B 8B 8B 8B " +
            "72 72 72 72 72 72 72 72 47 47 47 47 47 47 47 47 " +
            "39 39 39 39 39 39 39 39 72 72 72 72 72 72 72 72 " +
            "00 00 00 00 00 00 00 00 39 39 39 39 39 39 39 39 " +
            "42 42 42 42 42 42 42 42",
            // After Iota  
            "04 05 05 05 05 05 05 05 F2 F2 F2 F2 F2 F2 F2 F2 " +
            "27 27 27 27 27 27 27 27 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "D2 D2 D2 D2 D2 D2 D2 D2 4E 4E 4E 4E 4E 4E 4E 4E " +
            "74 74 74 74 74 74 74 74 88 88 88 88 88 88 88 88 " +
            "72 72 72 72 72 72 72 72 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "87 87 87 87 87 87 87 87 1B 1B 1B 1B 1B 1B 1B 1B " +
            "C5 C5 C5 C5 C5 C5 C5 C5 E2 E2 E2 E2 E2 E2 E2 E2 " +
            "B6 B6 B6 B6 B6 B6 B6 B6 8E 8E 8E 8E 8E 8E 8E 8E " +
            "70 70 70 70 70 70 70 70 8B 8B 8B 8B 8B 8B 8B 8B " +
            "72 72 72 72 72 72 72 72 47 47 47 47 47 47 47 47 " +
            "39 39 39 39 39 39 39 39 72 72 72 72 72 72 72 72 " +
            "00 00 00 00 00 00 00 00 39 39 39 39 39 39 39 39 " +
            "42 42 42 42 42 42 42 42",
            // Round #1 
            // After Theta
            "B2 B3 B3 B3 B3 B3 B3 B3 4B 4A 4A 4A 4A 4A 4A 4A " +
            "DE DE DE DE DE DE DE DE 1A 1A 1A 1A 1A 1A 1A 1A " +
            "15 17 17 17 17 17 17 17 F8 F8 F8 F8 F8 F8 F8 F8 " +
            "CD CC CC CC CC CC CC CC 71 71 71 71 71 71 71 71 " +
            "80 80 80 80 80 80 80 80 2F 2D 2D 2D 2D 2D 2D 2D " +
            "31 31 31 31 31 31 31 31 A2 A3 A3 A3 A3 A3 A3 A3 " +
            "3C 3C 3C 3C 3C 3C 3C 3C 10 10 10 10 10 10 10 10 " +
            "71 73 73 73 73 73 73 73 38 38 38 38 38 38 38 38 " +
            "C9 C8 C8 C8 C8 C8 C8 C8 72 72 72 72 72 72 72 72 " +
            "80 80 80 80 80 80 80 80 80 82 82 82 82 82 82 82 " +
            "8F 8F 8F 8F 8F 8F 8F 8F CB CA CA CA CA CA CA CA " +
            "F9 F9 F9 F9 F9 F9 F9 F9 CB CB CB CB CB CB CB CB " +
            "85 87 87 87 87 87 87 87",
            // After Rho
            "B2 B3 B3 B3 B3 B3 B3 B3 96 94 94 94 94 94 94 94 " +
            "B7 B7 B7 B7 B7 B7 B7 B7 A1 A1 A1 A1 A1 A1 A1 A1 " +
            "B8 B8 B8 A8 B8 B8 B8 B8 8F 8F 8F 8F 8F 8F 8F 8F " +
            "CC CC CC CC CC DC CC CC 5C 5C 5C 5C 5C 5C 5C 5C " +
            "40 40 40 40 40 40 40 40 D2 D2 F2 D2 D2 D2 D2 D2 " +
            "89 89 89 89 89 89 89 89 8E 8A 8E 8E 8E 8E 8E 8E " +
            "E1 E1 E1 E1 E1 E1 E1 E1 20 20 20 20 20 20 20 20 " +
            "B9 B9 B9 B9 B9 B8 B9 B9 70 70 70 70 70 70 70 70 " +
            "19 19 19 19 19 39 19 19 39 39 39 39 39 39 39 39 " +
            "10 10 10 10 10 10 10 10 82 80 82 82 82 82 82 82 " +
            "3E 3E 3E 3E 3E 3E 3E 3E 2F 2B 2B 2B 2B 2B 2B 2B " +
            "3F 3F 3F 3F 3F 3F 3F 3F CB CB CB CB CB CB CB CB " +
            "E1 61 E1 E1 E1 E1 E1 E1",
            // After Pi 
            "B2 B3 B3 B3 B3 B3 B3 B3 CC CC CC CC CC DC CC CC " +
            "E1 E1 E1 E1 E1 E1 E1 E1 10 10 10 10 10 10 10 10 " +
            "E1 61 E1 E1 E1 E1 E1 E1 A1 A1 A1 A1 A1 A1 A1 A1 " +
            "D2 D2 F2 D2 D2 D2 D2 D2 89 89 89 89 89 89 89 89 " +
            "19 19 19 19 19 39 19 19 3F 3F 3F 3F 3F 3F 3F 3F " +
            "96 94 94 94 94 94 94 94 5C 5C 5C 5C 5C 5C 5C 5C " +
            "20 20 20 20 20 20 20 20 82 80 82 82 82 82 82 82 " +
            "3E 3E 3E 3E 3E 3E 3E 3E B8 B8 B8 A8 B8 B8 B8 B8 " +
            "8F 8F 8F 8F 8F 8F 8F 8F 8E 8A 8E 8E 8E 8E 8E 8E " +
            "39 39 39 39 39 39 39 39 CB CB CB CB CB CB CB CB " +
            "B7 B7 B7 B7 B7 B7 B7 B7 40 40 40 40 40 40 40 40 " +
            "B9 B9 B9 B9 B9 B8 B9 B9 70 70 70 70 70 70 70 70 " +
            "2F 2B 2B 2B 2B 2B 2B 2B",
            // After Chi
            "93 92 92 92 92 92 92 92 DC DC DC DC DC CC DC DC " +
            "00 80 00 00 00 00 00 00 02 82 02 02 02 02 02 02 " +
            "AD 2D AD AD AD AD AD AD A8 A8 A8 A8 A8 A8 A8 A8 " +
            "C2 C2 E2 C2 C2 E2 C2 C2 AF AF AF AF AF 8F AF AF " +
            "99 99 99 99 99 B9 99 99 6D 6D 6D 6D 6D 6D 6D 6D " +
            "B6 B4 B4 B4 B4 B4 B4 B4 DE DC DE DE DE DE DE DE " +
            "1C 1E 1C 1C 1C 1C 1C 1C 02 00 02 02 02 02 02 02 " +
            "76 76 76 76 76 76 76 76 B8 B8 B8 A8 B8 B8 B8 B8 " +
            "BE BE BE BE BE BE BE BE 4C 48 4C 4C 4C 4C 4C 4C " +
            "09 09 09 19 09 09 09 09 CC CC CC CC CC CC CC CC " +
            "0E 0E 0E 0E 0E 0F 0E 0E 00 00 00 00 00 00 00 00 " +
            "B6 B2 B2 B2 B2 B3 B2 B2 E0 E4 E4 E4 E4 E4 E4 E4 " +
            "6F 6B 6B 6B 6B 6B 6B 6B",
            // After Iota  
            "11 12 92 92 92 92 92 92 DC DC DC DC DC CC DC DC " +
            "00 80 00 00 00 00 00 00 02 82 02 02 02 02 02 02 " +
            "AD 2D AD AD AD AD AD AD A8 A8 A8 A8 A8 A8 A8 A8 " +
            "C2 C2 E2 C2 C2 E2 C2 C2 AF AF AF AF AF 8F AF AF " +
            "99 99 99 99 99 B9 99 99 6D 6D 6D 6D 6D 6D 6D 6D " +
            "B6 B4 B4 B4 B4 B4 B4 B4 DE DC DE DE DE DE DE DE " +
            "1C 1E 1C 1C 1C 1C 1C 1C 02 00 02 02 02 02 02 02 " +
            "76 76 76 76 76 76 76 76 B8 B8 B8 A8 B8 B8 B8 B8 " +
            "BE BE BE BE BE BE BE BE 4C 48 4C 4C 4C 4C 4C 4C " +
            "09 09 09 19 09 09 09 09 CC CC CC CC CC CC CC CC " +
            "0E 0E 0E 0E 0E 0F 0E 0E 00 00 00 00 00 00 00 00 " +
            "B6 B2 B2 B2 B2 B3 B2 B2 E0 E4 E4 E4 E4 E4 E4 E4 " +
            "6F 6B 6B 6B 6B 6B 6B 6B",
            // Round #2 
            // After Theta
            "F8 7B 3F 7F 7F 1F 7F 7F F7 F2 7F 6E 7E 2D 7E 7E " +
            "9E 10 B7 B6 96 E6 96 96 61 6B 6C 6D 6D 4C 6D 6D " +
            "AF AA A8 99 A9 8B A9 A9 41 C1 05 45 45 25 45 45 " +
            "E9 EC 41 70 60 03 60 60 31 3F 18 19 39 69 39 39 " +
            "FA 70 F7 F6 F6 F7 F6 F6 6F EA 68 59 69 4B 69 69 " +
            "5F DD 19 59 59 39 59 59 F5 F2 7D 6C 7C 3F 7C 7C " +
            "82 8E AB AA 8A FA 8A 8A 61 E9 6C 6D 6D 4C 6D 6D " +
            "74 F1 73 42 72 50 72 72 51 D1 15 45 55 35 55 55 " +
            "95 90 1D 0C 1C 5F 1C 1C D2 D8 FB FA DA AA DA DA " +
            "6A E0 67 76 66 47 66 66 CE 4B C9 F8 C8 EA C8 C8 " +
            "E7 67 A3 E3 E3 82 E3 E3 2B 2E A3 B2 A2 E1 A2 A2 " +
            "28 22 05 04 24 55 24 24 83 0D 8A 8B 8B AA 8B 8B " +
            "6D EC 6E 5F 6F 4D 6F 6F",
            // After Rho
            "F8 7B 3F 7F 7F 1F 7F 7F EE E5 FF DC FC 5A FC FC " +
            "27 C4 AD AD A5 B9 A5 A5 C6 D4 D6 16 B6 C6 D6 D6 " +
            "5D 4C 4D 7D 55 45 CD 4C 54 54 52 54 14 14 5C 50 " +
            "04 07 36 00 06 96 CE 1E 4E CC 0F 46 46 4E 5A 4E " +
            "B8 7B 7B FB 7B 7B 7B 7D 94 96 F6 A6 8E 96 95 B6 " +
            "FA EA CE C8 CA CA C9 CA F1 D5 CB F7 B1 F1 FD F0 " +
            "55 55 D4 57 54 14 74 5C 98 DA DA C2 D2 D9 DA DA " +
            "21 39 28 39 39 BA F8 39 8A AA 6A AA AA A2 A2 2B " +
            "83 81 E3 8B 83 A3 12 B2 6D 6D 69 EC 7D 7D 6D 55 " +
            "C8 CC 4C 0D FC CC CE EC C8 CE 4B C9 F8 C8 EA C8 " +
            "8E 8F 9F 9F 8D 8E 8F 0B AE B8 8C CA 8A 86 8B 8A " +
            "45 A4 80 80 A4 8A 84 04 0D 8A 8B 8B AA 8B 8B 83 " +
            "DB 5B 1B BB DB D7 5B D3",
            // After Pi 
            "F8 7B 3F 7F 7F 1F 7F 7F 04 07 36 00 06 96 CE 1E " +
            "55 55 D4 57 54 14 74 5C C8 CC 4C 0D FC CC CE EC " +
            "DB 5B 1B BB DB D7 5B D3 C6 D4 D6 16 B6 C6 D6 D6 " +
            "94 96 F6 A6 8E 96 95 B6 FA EA CE C8 CA CA C9 CA " +
            "83 81 E3 8B 83 A3 12 B2 45 A4 80 80 A4 8A 84 04 " +
            "EE E5 FF DC FC 5A FC FC 4E CC 0F 46 46 4E 5A 4E " +
            "98 DA DA C2 D2 D9 DA DA C8 CE 4B C9 F8 C8 EA C8 " +
            "8E 8F 9F 9F 8D 8E 8F 0B 5D 4C 4D 7D 55 45 CD 4C " +
            "54 54 52 54 14 14 5C 50 F1 D5 CB F7 B1 F1 FD F0 " +
            "6D 6D 69 EC 7D 7D 6D 55 0D 8A 8B 8B AA 8B 8B 83 " +
            "27 C4 AD AD A5 B9 A5 A5 B8 7B 7B FB 7B 7B 7B 7D " +
            "21 39 28 39 39 BA F8 39 8A AA 6A AA AA A2 A2 2B " +
            "AE B8 8C CA 8A 86 8B 8A",
            // After Chi
            "A9 2B FF 28 2F 1F 4F 3F 8C 8F 3E 08 AE 5E 44 BE " +
            "46 46 C7 E5 57 07 65 4F E8 EC 68 49 D8 C4 EA C0 " +
            "DF 5F 1B BB DB 57 DB D3 AC BC DE 5E F6 8E 9E 9E " +
            "95 97 D7 A5 8F B7 87 86 BE CE CE C8 EE C2 4D CE " +
            "01 D1 B5 9D 91 E7 40 60 55 A6 A0 20 AC 9A 85 24 " +
            "7E F7 2F 5C 6C CB 7C 6C 0E C8 0E 4F 6E 4E 7A 4E " +
            "9E DB 4E D4 D7 DF DF D9 A8 AE 2B 89 88 98 9A 3C " +
            "8E 87 9F 9D 8F 8A 8D 09 FC CD C4 DE F4 A4 6C EC " +
            "58 7C 72 5C 58 18 5C 55 F1 57 49 F4 33 73 7F 72 " +
            "3D 29 2D 98 28 39 29 19 0D 9A 99 8B AA 9B 9B 93 " +
            "26 C4 AD AD A5 39 25 A5 32 F9 39 79 F9 7B 79 7F " +
            "05 29 AC 79 39 BE F1 B9 8B EE 4B 8F 8F 9B 86 0E " +
            "36 83 DE 98 D0 C4 D1 D2",
            // After Iota  
            "23 AB FF 28 2F 1F 4F BF 8C 8F 3E 08 AE 5E 44 BE " +
            "46 46 C7 E5 57 07 65 4F E8 EC 68 49 D8 C4 EA C0 " +
            "DF 5F 1B BB DB 57 DB D3 AC BC DE 5E F6 8E 9E 9E " +
            "95 97 D7 A5 8F B7 87 86 BE CE CE C8 EE C2 4D CE " +
            "01 D1 B5 9D 91 E7 40 60 55 A6 A0 20 AC 9A 85 24 " +
            "7E F7 2F 5C 6C CB 7C 6C 0E C8 0E 4F 6E 4E 7A 4E " +
            "9E DB 4E D4 D7 DF DF D9 A8 AE 2B 89 88 98 9A 3C " +
            "8E 87 9F 9D 8F 8A 8D 09 FC CD C4 DE F4 A4 6C EC " +
            "58 7C 72 5C 58 18 5C 55 F1 57 49 F4 33 73 7F 72 " +
            "3D 29 2D 98 28 39 29 19 0D 9A 99 8B AA 9B 9B 93 " +
            "26 C4 AD AD A5 39 25 A5 32 F9 39 79 F9 7B 79 7F " +
            "05 29 AC 79 39 BE F1 B9 8B EE 4B 8F 8F 9B 86 0E " +
            "36 83 DE 98 D0 C4 D1 D2",
            // Round #3 
            // After Theta
            "E6 66 C4 B2 70 8E EF B9 82 3D 1D B8 82 37 53 9C " +
            "D4 BA 4B B7 75 F1 C7 04 05 0F 0C 17 B8 22 A1 2C " +
            "7E D9 44 43 75 C1 8D 51 69 71 E5 C4 A9 1F 3E 98 " +
            "9B 25 F4 15 A3 DE 90 A4 2C 32 42 9A CC 34 EF 85 " +
            "EC 32 D1 C3 F1 01 0B 8C F4 20 FF D8 02 0C D3 A6 " +
            "BB 3A 14 C6 33 5A DC 6A 00 7A 2D FF 42 27 6D 6C " +
            "0C 27 C2 86 F5 29 7D 92 45 4D 4F D7 E8 7E D1 D0 " +
            "2F 01 C0 65 21 1C DB 8B 39 00 FF 44 AB 35 CC EA " +
            "56 CE 51 EC 74 71 4B 77 63 AB C5 A6 11 85 DD 39 " +
            "D0 CA 49 C6 48 DF 62 F5 AC 1C C6 73 04 0D CD 11 " +
            "E3 09 96 37 FA A8 85 A3 3C 4B 1A C9 D5 12 6E 5D " +
            "97 D5 20 2B 1B 48 53 F2 66 0D 2F D1 EF 7D CD E2 " +
            "97 05 81 60 7E 52 87 50",
            // After Rho
            "E6 66 C4 B2 70 8E EF B9 05 7B 3A 70 05 6F A6 38 " +
            "B5 EE D2 6D 5D FC 31 01 2B 12 CA 52 F0 C0 70 81 " +
            "0B 6E 8C F2 CB 26 1A AA 9C FA E1 83 99 16 57 4E " +
            "5F 31 EA 0D 49 BA 59 42 21 8B 8C 90 26 33 CD 7B " +
            "99 E8 E1 F8 80 05 46 76 30 6D 4A 0F F2 8F 2D C0 " +
            "DB D5 A1 30 9E D1 E2 56 B1 01 E8 B5 FC 0B 9D B4 " +
            "36 AC 4F E9 93 64 38 11 FD A2 A1 8B 9A 9E AE D1 " +
            "B2 10 8E ED C5 97 00 E0 89 56 6B 98 D5 73 00 FE " +
            "8A 9D 2E 6E E9 CE CA 39 EE 9C B1 D5 62 D3 88 C2 " +
            "5B AC 1E 5A 39 C9 18 E9 11 AC 1C C6 73 04 0D CD " +
            "16 8E 8E 27 58 DE E8 A3 F1 2C 69 24 57 4B B8 75 " +
            "B2 1A 64 65 03 69 4A FE 0D 2F D1 EF 7D CD E2 66 " +
            "21 D4 65 41 20 98 9F D4",
            // After Pi 
            "E6 66 C4 B2 70 8E EF B9 5F 31 EA 0D 49 BA 59 42 " +
            "36 AC 4F E9 93 64 38 11 5B AC 1E 5A 39 C9 18 E9 " +
            "21 D4 65 41 20 98 9F D4 2B 12 CA 52 F0 C0 70 81 " +
            "30 6D 4A 0F F2 8F 2D C0 DB D5 A1 30 9E D1 E2 56 " +
            "8A 9D 2E 6E E9 CE CA 39 B2 1A 64 65 03 69 4A FE " +
            "05 7B 3A 70 05 6F A6 38 21 8B 8C 90 26 33 CD 7B " +
            "FD A2 A1 8B 9A 9E AE D1 11 AC 1C C6 73 04 0D CD " +
            "16 8E 8E 27 58 DE E8 A3 0B 6E 8C F2 CB 26 1A AA " +
            "9C FA E1 83 99 16 57 4E B1 01 E8 B5 FC 0B 9D B4 " +
            "EE 9C B1 D5 62 D3 88 C2 0D 2F D1 EF 7D CD E2 66 " +
            "B5 EE D2 6D 5D FC 31 01 99 E8 E1 F8 80 05 46 76 " +
            "B2 10 8E ED C5 97 00 E0 89 56 6B 98 D5 73 00 FE " +
            "F1 2C 69 24 57 4B B8 75",
            // After Chi
            "C6 EA C1 52 E2 CA CF A8 16 31 FA 1F 61 33 59 AA " +
            "16 FC 2E E8 93 74 BF 05 9D 8E 9E E8 69 CF 78 C0 " +
            "38 C5 4F 4C 29 A8 8F 96 E0 82 6B 62 FC 90 B2 97 " +
            "30 65 44 41 93 81 25 E9 EB D7 E1 31 9C F0 E2 90 " +
            "83 9D A4 7C 19 4E FA 38 A2 77 64 68 01 66 47 BE " +
            "D9 5B 1B 7B 9D E3 84 B8 21 87 90 D4 47 33 CC 77 " +
            "FB A0 23 AA 92 44 4E F3 10 DD 2C 96 76 25 0B D5 " +
            "36 0E 0A A7 7A CE A1 E0 2A 6F 84 C6 AF 2F 92 1A " +
            "D2 66 F0 C3 9B C6 57 0C B0 22 A8 9F E1 07 FF 90 " +
            "EC DC BD C5 E0 F1 90 4A 99 BF B0 EE 6D DD A7 22 " +
            "97 FE DC 68 18 6E 31 81 90 AE 80 E8 90 65 46 68 " +
            "C2 38 8E C9 C7 9F B8 E1 8D 94 F9 D1 DD C7 01 FE " +
            "F9 2C 48 B4 D7 4A FE 03",
            // After Iota  
            "C6 6A C1 D2 E2 CA CF 28 16 31 FA 1F 61 33 59 AA " +
            "16 FC 2E E8 93 74 BF 05 9D 8E 9E E8 69 CF 78 C0 " +
            "38 C5 4F 4C 29 A8 8F 96 E0 82 6B 62 FC 90 B2 97 " +
            "30 65 44 41 93 81 25 E9 EB D7 E1 31 9C F0 E2 90 " +
            "83 9D A4 7C 19 4E FA 38 A2 77 64 68 01 66 47 BE " +
            "D9 5B 1B 7B 9D E3 84 B8 21 87 90 D4 47 33 CC 77 " +
            "FB A0 23 AA 92 44 4E F3 10 DD 2C 96 76 25 0B D5 " +
            "36 0E 0A A7 7A CE A1 E0 2A 6F 84 C6 AF 2F 92 1A " +
            "D2 66 F0 C3 9B C6 57 0C B0 22 A8 9F E1 07 FF 90 " +
            "EC DC BD C5 E0 F1 90 4A 99 BF B0 EE 6D DD A7 22 " +
            "97 FE DC 68 18 6E 31 81 90 AE 80 E8 90 65 46 68 " +
            "C2 38 8E C9 C7 9F B8 E1 8D 94 F9 D1 DD C7 01 FE " +
            "F9 2C 48 B4 D7 4A FE 03",
            // Round #4 
            // After Theta
            "80 73 A4 49 77 18 BD 60 BC 31 86 31 23 7A AB 18 " +
            "8C EB D5 65 5B 72 2F 67 70 40 E6 7E 03 B8 4D 05 " +
            "D2 07 CF 91 7A CA 22 37 A6 9B 0E F9 69 42 C0 DF " +
            "9A 65 38 6F D1 C8 D7 5B 71 C0 1A BC 54 F6 72 F2 " +
            "6E 53 DC EA 73 39 CF FD 48 B5 E4 B5 52 04 EA 1F " +
            "9F 42 7E E0 08 31 F6 F0 8B 87 EC FA 05 7A 3E C5 " +
            "61 B7 D8 27 5A 42 DE 91 FD 13 54 00 1C 52 3E 10 " +
            "DC CC 8A 7A 29 AC 0C 41 6C 76 E1 5D 3A FD E0 52 " +
            "78 66 8C ED D9 8F A5 BE 2A 35 53 12 29 01 6F F2 " +
            "01 12 C5 53 8A 86 A5 8F 73 7D 30 33 3E BF 0A 83 " +
            "D1 E7 B9 F3 8D BC 43 C9 3A AE FC C6 D2 2C B4 DA " +
            "58 2F 75 44 0F 99 28 83 60 5A 81 47 B7 B0 34 3B " +
            "13 EE C8 69 84 28 53 A2",
            // After Rho
            "80 73 A4 49 77 18 BD 60 78 63 0C 63 46 F4 56 31 " +
            "E3 7A 75 D9 96 DC CB 19 80 DB 54 00 07 64 EE 37 " +
            "53 16 B9 91 3E 78 8E D4 9F 26 04 FC 6D BA E9 90 " +
            "F3 16 8D 7C BD A5 59 86 7C 1C B0 06 2F 95 BD 9C " +
            "29 6E F5 B9 9C E7 7E B7 A0 FE 81 54 4B 5E 2B 45 " +
            "FF 14 F2 03 47 88 B1 87 14 2F 1E B2 EB 17 E8 F9 " +
            "3E D1 12 F2 8E 0C BB C5 A4 7C 20 FA 27 A8 00 38 " +
            "BD 14 56 86 20 6E 66 45 BB 74 FA C1 A5 D8 EC C2 " +
            "B1 3D FB B1 D4 17 CF 8C 37 79 95 9A 29 89 94 80 " +
            "B0 F4 31 40 A2 78 4A D1 83 73 7D 30 33 3E BF 0A " +
            "0E 25 47 9F E7 CE 37 F2 EB B8 F2 1B 4B B3 D0 6A " +
            "EB A5 8E E8 21 13 65 10 5A 81 47 B7 B0 34 3B 60 " +
            "94 E8 84 3B 72 1A 21 CA",
            // After Pi 
            "80 73 A4 49 77 18 BD 60 F3 16 8D 7C BD A5 59 86 " +
            "3E D1 12 F2 8E 0C BB C5 B0 F4 31 40 A2 78 4A D1 " +
            "94 E8 84 3B 72 1A 21 CA 80 DB 54 00 07 64 EE 37 " +
            "A0 FE 81 54 4B 5E 2B 45 FF 14 F2 03 47 88 B1 87 " +
            "B1 3D FB B1 D4 17 CF 8C EB A5 8E E8 21 13 65 10 " +
            "78 63 0C 63 46 F4 56 31 7C 1C B0 06 2F 95 BD 9C " +
            "A4 7C 20 FA 27 A8 00 38 83 73 7D 30 33 3E BF 0A " +
            "0E 25 47 9F E7 CE 37 F2 53 16 B9 91 3E 78 8E D4 " +
            "9F 26 04 FC 6D BA E9 90 14 2F 1E B2 EB 17 E8 F9 " +
            "37 79 95 9A 29 89 94 80 5A 81 47 B7 B0 34 3B 60 " +
            "E3 7A 75 D9 96 DC CB 19 29 6E F5 B9 9C E7 7E B7 " +
            "BD 14 56 86 20 6E 66 45 BB 74 FA C1 A5 D8 EC C2 " +
            "EB B8 F2 1B 4B B3 D0 6A",
            // After Chi
            "8C B2 B6 CB 75 10 1F 21 73 32 AC 7C 9D D5 19 96 " +
            "3A D9 96 C9 DE 0E 9A CF B0 E7 11 00 A7 78 D6 F1 " +
            "E7 EC 8D 0F FA BF 61 4C DF DB 26 03 03 E4 7E B5 " +
            "A0 D7 88 E4 DB 49 65 4D B5 94 F6 4B 66 88 91 97 " +
            "B1 67 AB B1 D2 73 45 AB CB 81 0F BC 69 09 64 50 " +
            "F8 03 0C 9B 46 DC 56 11 7F 1F ED 06 3F 83 02 9E " +
            "A8 78 22 75 E3 68 00 C8 F3 31 75 50 33 0E FF 0B " +
            "0A 39 F7 9B CE CF 9E 7E 53 1F A3 93 BC 7D 8E BD " +
            "BC 76 85 F4 6D 32 FD 90 5C AF 5C 97 7B 23 C3 99 " +
            "36 6F 2D 9A 27 C1 10 14 D6 A1 43 DB F1 B6 5A 60 " +
            "77 6A 77 DF B6 D4 CB 59 2B 0E 5D F8 19 77 F6 35 " +
            "FD 9C 56 9C 6A 4D 76 6D BB 36 FF 01 31 94 E7 D3 " +
            "E3 BC 72 3B 43 90 E4 CC",
            // After Iota  
            "07 32 B6 CB 75 10 1F 21 73 32 AC 7C 9D D5 19 96 " +
            "3A D9 96 C9 DE 0E 9A CF B0 E7 11 00 A7 78 D6 F1 " +
            "E7 EC 8D 0F FA BF 61 4C DF DB 26 03 03 E4 7E B5 " +
            "A0 D7 88 E4 DB 49 65 4D B5 94 F6 4B 66 88 91 97 " +
            "B1 67 AB B1 D2 73 45 AB CB 81 0F BC 69 09 64 50 " +
            "F8 03 0C 9B 46 DC 56 11 7F 1F ED 06 3F 83 02 9E " +
            "A8 78 22 75 E3 68 00 C8 F3 31 75 50 33 0E FF 0B " +
            "0A 39 F7 9B CE CF 9E 7E 53 1F A3 93 BC 7D 8E BD " +
            "BC 76 85 F4 6D 32 FD 90 5C AF 5C 97 7B 23 C3 99 " +
            "36 6F 2D 9A 27 C1 10 14 D6 A1 43 DB F1 B6 5A 60 " +
            "77 6A 77 DF B6 D4 CB 59 2B 0E 5D F8 19 77 F6 35 " +
            "FD 9C 56 9C 6A 4D 76 6D BB 36 FF 01 31 94 E7 D3 " +
            "E3 BC 72 3B 43 90 E4 CC",
            // Round #5 
            // After Theta
            "63 7F D1 27 81 FB D0 2F 7B A0 74 9B 32 54 16 3E " +
            "FE 8B BC AF 73 F4 D9 02 11 73 D1 6C 32 47 22 09 " +
            "90 3A 01 4B DE ED 1F 18 BB 96 41 EF F7 0F B1 BB " +
            "A8 45 50 03 74 C8 6A E5 71 C6 DC 2D CB 72 D2 5A " +
            "10 F3 6B DD 47 4C B1 53 BC 57 83 F8 4D 5B 1A 04 " +
            "9C 4E 6B 77 B2 37 99 1F 77 8D 35 E1 90 02 0D 36 " +
            "6C 2A 08 13 4E 92 43 05 52 A5 B5 3C A6 31 0B F3 " +
            "7D EF 7B DF EA 9D E0 2A 37 52 C4 7F 48 96 41 B3 " +
            "B4 E4 5D 13 C2 B3 F2 38 98 FD 76 F1 D6 D9 80 54 " +
            "97 FB ED F6 B2 FE E4 EC A1 77 CF 9F D5 E4 24 34 " +
            "13 27 10 33 42 3F 04 57 23 9C 85 1F B6 F6 F9 9D " +
            "39 CE 7C FA C7 B7 35 A0 1A A2 3F 6D A4 AB 13 2B " +
            "94 6A FE 7F 67 C2 9A 98",
            // After Rho
            "63 7F D1 27 81 FB D0 2F F6 40 E9 36 65 A8 2C 7C " +
            "FF 22 EF EB 1C 7D B6 80 73 24 92 10 31 17 CD 26 " +
            "6E FF C0 80 D4 09 58 F2 7E FF 10 BB BB 6B 19 F4 " +
            "35 40 87 AC 56 8E 5A 04 56 9C 31 77 CB B2 9C B4 " +
            "F9 B5 EE 23 A6 D8 29 88 A5 41 C0 7B 35 88 DF B4 " +
            "E0 74 5A BB 93 BD C9 FC D8 DC 35 D6 84 43 0A 34 " +
            "98 70 92 1C 2A 60 53 41 63 16 E6 A5 4A 6B 79 4C " +
            "6F F5 4E 70 95 BE F7 BD FF 90 2C 83 66 6F A4 88 " +
            "6B 42 78 56 1E 87 96 BC 40 2A CC 7E BB 78 EB 6C " +
            "9F 9C FD 72 BF DD 5E D6 34 A1 77 CF 9F D5 E4 24 " +
            "10 5C 4D 9C 40 CC 08 FD 8E 70 16 7E D8 DA E7 77 " +
            "C7 99 4F FF F8 B6 06 34 A2 3F 6D A4 AB 13 2B 1A " +
            "26 26 A5 9A FF DF 99 B0",
            // After Pi 
            "63 7F D1 27 81 FB D0 2F 35 40 87 AC 56 8E 5A 04 " +
            "98 70 92 1C 2A 60 53 41 9F 9C FD 72 BF DD 5E D6 " +
            "26 26 A5 9A FF DF 99 B0 73 24 92 10 31 17 CD 26 " +
            "A5 41 C0 7B 35 88 DF B4 E0 74 5A BB 93 BD C9 FC " +
            "6B 42 78 56 1E 87 96 BC C7 99 4F FF F8 B6 06 34 " +
            "F6 40 E9 36 65 A8 2C 7C 56 9C 31 77 CB B2 9C B4 " +
            "63 16 E6 A5 4A 6B 79 4C 34 A1 77 CF 9F D5 E4 24 " +
            "10 5C 4D 9C 40 CC 08 FD 6E FF C0 80 D4 09 58 F2 " +
            "7E FF 10 BB BB 6B 19 F4 D8 DC 35 D6 84 43 0A 34 " +
            "40 2A CC 7E BB 78 EB 6C A2 3F 6D A4 AB 13 2B 1A " +
            "FF 22 EF EB 1C 7D B6 80 F9 B5 EE 23 A6 D8 29 88 " +
            "6F F5 4E 70 95 BE F7 BD FF 90 2C 83 66 6F A4 88 " +
            "8E 70 16 7E D8 DA E7 77",
            // After Chi
            "EB 4F C1 37 A9 9B D1 6E 32 CC EA CE C3 13 56 92 " +
            "B8 52 92 94 6A 62 D2 61 DE C5 AD 57 BF FD 1E D9 " +
            "32 26 A3 12 A9 DB 93 B0 33 10 88 90 B3 22 CD 6E " +
            "AE 43 E0 3F 39 8A C9 B4 64 ED 5D 12 73 8D C9 FC " +
            "5B 66 E8 56 1F 86 5F BE 43 D8 0F 94 FC 3E 14 A4 " +
            "D7 42 2F B6 65 E1 4D 34 42 3D 20 3D 5E 26 18 94 " +
            "63 4A EE B5 0A 63 71 95 D2 A1 D7 ED BA F5 C0 24 " +
            "10 C0 5D DD CA DE 98 7D EE FF E5 C4 D0 09 5A F2 " +
            "7E DD D8 93 80 53 F8 BC 7A C9 14 56 84 40 0A 26 " +
            "0C EA 4C 7E EF 70 BB 8C B2 3F 7D 9F 80 71 2A 1E " +
            "F9 62 EF BB 0D 5B 60 B5 69 B5 CE A0 C4 99 29 88 " +
            "6F 95 5C 0C 0D 2E B4 CA 8E 92 C5 02 62 4A B4 08 " +
            "8E E5 16 7E 7A 5A EE 7F",
            // After Iota  
            "EA 4F C1 B7 A9 9B D1 6E 32 CC EA CE C3 13 56 92 " +
            "B8 52 92 94 6A 62 D2 61 DE C5 AD 57 BF FD 1E D9 " +
            "32 26 A3 12 A9 DB 93 B0 33 10 88 90 B3 22 CD 6E " +
            "AE 43 E0 3F 39 8A C9 B4 64 ED 5D 12 73 8D C9 FC " +
            "5B 66 E8 56 1F 86 5F BE 43 D8 0F 94 FC 3E 14 A4 " +
            "D7 42 2F B6 65 E1 4D 34 42 3D 20 3D 5E 26 18 94 " +
            "63 4A EE B5 0A 63 71 95 D2 A1 D7 ED BA F5 C0 24 " +
            "10 C0 5D DD CA DE 98 7D EE FF E5 C4 D0 09 5A F2 " +
            "7E DD D8 93 80 53 F8 BC 7A C9 14 56 84 40 0A 26 " +
            "0C EA 4C 7E EF 70 BB 8C B2 3F 7D 9F 80 71 2A 1E " +
            "F9 62 EF BB 0D 5B 60 B5 69 B5 CE A0 C4 99 29 88 " +
            "6F 95 5C 0C 0D 2E B4 CA 8E 92 C5 02 62 4A B4 08 " +
            "8E E5 16 7E 7A 5A EE 7F",
            // Round #6 
            // After Theta
            "24 1E 22 F3 0D 60 A6 6A 7E 1F 55 F2 55 DC 94 28 " +
            "DA 7D 98 4B A5 7E 99 68 CE A4 F1 4B EE 3F 7C 2C " +
            "D5 5C 61 5E 7B 7A CB 91 FD 41 6B D4 17 D9 BA 6A " +
            "E2 90 5F 03 AF 45 0B 0E 06 C2 57 CD BC 91 82 F5 " +
            "4B 07 B4 4A 4E 44 3D 4B A4 A2 CD D8 2E 9F 4C 85 " +
            "19 13 CC F2 C1 1A 3A 30 0E EE 9F 01 C8 E9 DA 2E " +
            "01 65 E4 6A C5 7F 3A 9C C2 C0 8B F1 EB 37 A2 D1 " +
            "F7 BA 9F 91 18 7F C0 5C 20 AE 06 80 74 F2 2D F6 " +
            "32 0E 67 AF 16 9C 3A 06 18 E6 1E 89 4B 5C 41 2F " +
            "1C 8B 10 62 BE B2 D9 79 55 45 BF D3 52 D0 72 3F " +
            "37 33 0C FF A9 A0 17 B1 25 66 71 9C 52 56 EB 32 " +
            "0D BA 56 D3 C2 32 FF C3 9E F3 99 1E 33 88 D6 FD " +
            "69 9F D4 32 A8 FB B6 5E",
            // After Rho
            "24 1E 22 F3 0D 60 A6 6A FC 3E AA E4 AB B8 29 51 " +
            "76 1F E6 52 A9 5F 26 9A FE C3 C7 E2 4C 1A BF E4 " +
            "D3 5B 8E AC E6 0A F3 DA 7D 91 AD AB D6 1F B4 46 " +
            "35 F0 5A B4 E0 20 0E F9 BD 81 F0 55 33 6F A4 60 " +
            "03 5A 25 27 A2 9E A5 A5 C9 54 48 2A DA 8C ED F2 " +
            "C9 98 60 96 0F D6 D0 81 BB 38 B8 7F 06 20 A7 6B " +
            "57 2B FE D3 E1 0C 28 23 6F 44 A3 85 81 17 E3 D7 " +
            "48 8C 3F 60 AE 7B DD CF 00 E9 E4 5B EC 41 5C 0D " +
            "EC D5 82 53 C7 40 C6 E1 A0 17 0C 73 8F C4 25 AE " +
            "36 3B 8F 63 11 42 CC 57 3F 55 45 BF D3 52 D0 72 " +
            "5E C4 DE CC 30 FC A7 82 94 98 C5 71 4A 59 AD CB " +
            "41 D7 6A 5A 58 E6 7F B8 F3 99 1E 33 88 D6 FD 9E " +
            "AD 57 DA 27 B5 0C EA BE",
            // After Pi 
            "24 1E 22 F3 0D 60 A6 6A 35 F0 5A B4 E0 20 0E F9 " +
            "57 2B FE D3 E1 0C 28 23 36 3B 8F 63 11 42 CC 57 " +
            "AD 57 DA 27 B5 0C EA BE FE C3 C7 E2 4C 1A BF E4 " +
            "C9 54 48 2A DA 8C ED F2 C9 98 60 96 0F D6 D0 81 " +
            "EC D5 82 53 C7 40 C6 E1 41 D7 6A 5A 58 E6 7F B8 " +
            "FC 3E AA E4 AB B8 29 51 BD 81 F0 55 33 6F A4 60 " +
            "6F 44 A3 85 81 17 E3 D7 3F 55 45 BF D3 52 D0 72 " +
            "5E C4 DE CC 30 FC A7 82 D3 5B 8E AC E6 0A F3 DA " +
            "7D 91 AD AB D6 1F B4 46 BB 38 B8 7F 06 20 A7 6B " +
            "A0 17 0C 73 8F C4 25 AE F3 99 1E 33 88 D6 FD 9E " +
            "76 1F E6 52 A9 5F 26 9A 03 5A 25 27 A2 9E A5 A5 " +
            "48 8C 3F 60 AE 7B DD CF 00 E9 E4 5B EC 41 5C 0D " +
            "94 98 C5 71 4A 59 AD CB",
            // After Chi
            "66 15 86 B0 0C 6C 86 68 15 E0 5B 94 F0 62 CA AD " +
            "DE 6F AE D7 45 00 0A 8B 36 33 AF B3 19 22 C8 17 " +
            "BC B7 82 23 55 0C E2 2F FE 4B E7 76 49 48 AF E5 " +
            "ED 11 CA 6B 1A 8C EB 92 C8 9A 08 9E 17 70 E9 99 " +
            "52 D5 07 F3 C3 58 46 A5 40 C3 62 52 CA 62 3F AA " +
            "BE 7A A9 64 2B A8 6A C6 AD 90 B4 6F 61 2F B4 40 " +
            "2F C4 39 C5 A1 BB C4 57 9F 6F 65 9F 58 52 D8 23 " +
            "5F 45 8E DD 20 BB 23 A2 51 73 9E F8 E6 2A F0 F3 " +
            "7D 96 A9 AB 5F DB B4 C2 E8 B0 AA 7F 06 32 7F 7B " +
            "A0 55 8C FF E9 CC 27 EE DF 19 3F 30 98 C3 F9 9A " +
            "3E 9B FC 12 A5 3E 7E D0 03 3B E5 3C E2 9E A5 A5 " +
            "DC 9C 3E 40 AC 63 7C 0D 62 EE C6 59 4D 47 5E 1D " +
            "95 D8 C4 54 48 D9 2C EE",
            // After Iota  
            "E7 95 86 30 0C 6C 86 E8 15 E0 5B 94 F0 62 CA AD " +
            "DE 6F AE D7 45 00 0A 8B 36 33 AF B3 19 22 C8 17 " +
            "BC B7 82 23 55 0C E2 2F FE 4B E7 76 49 48 AF E5 " +
            "ED 11 CA 6B 1A 8C EB 92 C8 9A 08 9E 17 70 E9 99 " +
            "52 D5 07 F3 C3 58 46 A5 40 C3 62 52 CA 62 3F AA " +
            "BE 7A A9 64 2B A8 6A C6 AD 90 B4 6F 61 2F B4 40 " +
            "2F C4 39 C5 A1 BB C4 57 9F 6F 65 9F 58 52 D8 23 " +
            "5F 45 8E DD 20 BB 23 A2 51 73 9E F8 E6 2A F0 F3 " +
            "7D 96 A9 AB 5F DB B4 C2 E8 B0 AA 7F 06 32 7F 7B " +
            "A0 55 8C FF E9 CC 27 EE DF 19 3F 30 98 C3 F9 9A " +
            "3E 9B FC 12 A5 3E 7E D0 03 3B E5 3C E2 9E A5 A5 " +
            "DC 9C 3E 40 AC 63 7C 0D 62 EE C6 59 4D 47 5E 1D " +
            "95 D8 C4 54 48 D9 2C EE",
            // Round #7 
            // After Theta
            "58 FD C0 F6 0F AB A4 8A C7 96 E7 3A 6E CE 4E 23 " +
            "87 C7 C9 23 3F C2 D1 57 E9 CF 8F 91 9F 26 BB 82 " +
            "14 1C 51 CB 28 9F 56 9C 41 23 A1 B0 4A 8F 8D 87 " +
            "3F 67 76 C5 84 20 6F 1C 91 32 6F 6A 6D B2 32 45 " +
            "8D 29 27 D1 45 5C 35 30 E8 68 B1 BA B7 F1 8B 19 " +
            "01 12 EF A2 28 6F 48 A4 7F E6 08 C1 FF 83 30 CE " +
            "76 6C 5E 31 DB 79 1F 8B 40 93 45 BD DE 56 AB B6 " +
            "F7 EE 5D 35 5D 28 97 11 EE 1B D8 3E E5 ED D2 91 " +
            "AF E0 15 05 C1 77 30 4C B1 18 CD 8B 7C F0 A4 A7 " +
            "7F A9 AC DD 6F C8 54 7B 77 B2 EC D8 E5 50 4D 29 " +
            "81 F3 BA D4 A6 F9 5C B2 D1 4D 59 92 7C 32 21 2B " +
            "85 34 59 B4 D6 A1 A7 D1 BD 12 E6 7B CB 43 2D 88 " +
            "3D 73 17 BC 35 4A 98 5D",
            // After Rho
            "58 FD C0 F6 0F AB A4 8A 8E 2D CF 75 DC 9C 9D 46 " +
            "E1 71 F2 C8 8F 70 F4 D5 69 B2 2B 98 FE FC 18 F9 " +
            "F9 B4 E2 A4 E0 88 5A 46 AB F4 D8 78 18 34 12 0A " +
            "57 4C 08 F2 C6 F1 73 66 51 A4 CC 9B 5A 9B AC 4C " +
            "94 93 E8 22 AE 1A 98 C6 BF 98 81 8E 16 AB 7B 1B " +
            "0D 90 78 17 45 79 43 22 38 FF 99 23 04 FF 0F C2 " +
            "8A D9 CE FB 58 B4 63 F3 AD 56 6D 81 26 8B 7A BD " +
            "9A 2E 94 CB 88 7B F7 AE 7D CA DB A5 23 DD 37 B0 " +
            "A2 20 F8 0E 86 E9 15 BC D2 D3 58 8C E6 45 3E 78 " +
            "99 6A EF 2F 95 B5 FB 0D 29 77 B2 EC D8 E5 50 4D " +
            "73 C9 06 CE EB 52 9B E6 44 37 65 49 F2 C9 84 AC " +
            "90 26 8B D6 3A F4 34 BA 12 E6 7B CB 43 2D 88 BD " +
            "66 57 CF DC 05 6F 8D 12",
            // After Pi 
            "58 FD C0 F6 0F AB A4 8A 57 4C 08 F2 C6 F1 73 66 " +
            "8A D9 CE FB 58 B4 63 F3 99 6A EF 2F 95 B5 FB 0D " +
            "66 57 CF DC 05 6F 8D 12 69 B2 2B 98 FE FC 18 F9 " +
            "BF 98 81 8E 16 AB 7B 1B 0D 90 78 17 45 79 43 22 " +
            "A2 20 F8 0E 86 E9 15 BC 90 26 8B D6 3A F4 34 BA " +
            "8E 2D CF 75 DC 9C 9D 46 51 A4 CC 9B 5A 9B AC 4C " +
            "AD 56 6D 81 26 8B 7A BD 29 77 B2 EC D8 E5 50 4D " +
            "73 C9 06 CE EB 52 9B E6 F9 B4 E2 A4 E0 88 5A 46 " +
            "AB F4 D8 78 18 34 12 0A 38 FF 99 23 04 FF 0F C2 " +
            "D2 D3 58 8C E6 45 3E 78 12 E6 7B CB 43 2D 88 BD " +
            "E1 71 F2 C8 8F 70 F4 D5 94 93 E8 22 AE 1A 98 C6 " +
            "9A 2E 94 CB 88 7B F7 AE 7D CA DB A5 23 DD 37 B0 " +
            "44 37 65 49 F2 C9 84 AC",
            // After Chi
            "D0 6C 06 FF 17 AF A4 1B 46 6E 29 F6 43 F0 EB 6A " +
            "EC CC CE 2B 58 FE 67 E1 81 C2 EF 0D 9F 35 DB 85 " +
            "61 57 C7 DC C5 3F DE 76 69 B2 53 89 BF AC 18 D9 " +
            "1D B8 01 86 94 2B 6F 87 1D 96 7B C7 7D 6D 63 20 " +
            "CB B0 D8 06 42 E1 1D FD 06 2E 0B D0 3A F7 57 B8 " +
            "22 7F EE 75 F8 9C CF F7 51 85 5E F7 82 FF AC 0C " +
            "FF DE 69 83 05 99 F1 1F A5 53 7B DD CC 69 54 4D " +
            "22 49 06 44 E9 51 BB EE E9 BF E3 A7 E4 43 57 86 " +
            "69 F4 98 F4 FA 34 22 32 38 DB BA 60 05 D7 8F 47 " +
            "3B C3 D8 A8 46 C5 6C 3A 10 A6 63 93 5B 19 88 B5 " +
            "EB 5D E6 01 8F 11 93 FD F1 53 A3 06 8D 9E 98 D6 " +
            "9A 1B B0 83 58 7B 77 A2 DC 8A 49 25 2E ED 47 E1 " +
            "50 B5 6D 6B D2 C3 8C AE",
            // After Iota  
            "D9 EC 06 FF 17 AF A4 9B 46 6E 29 F6 43 F0 EB 6A " +
            "EC CC CE 2B 58 FE 67 E1 81 C2 EF 0D 9F 35 DB 85 " +
            "61 57 C7 DC C5 3F DE 76 69 B2 53 89 BF AC 18 D9 " +
            "1D B8 01 86 94 2B 6F 87 1D 96 7B C7 7D 6D 63 20 " +
            "CB B0 D8 06 42 E1 1D FD 06 2E 0B D0 3A F7 57 B8 " +
            "22 7F EE 75 F8 9C CF F7 51 85 5E F7 82 FF AC 0C " +
            "FF DE 69 83 05 99 F1 1F A5 53 7B DD CC 69 54 4D " +
            "22 49 06 44 E9 51 BB EE E9 BF E3 A7 E4 43 57 86 " +
            "69 F4 98 F4 FA 34 22 32 38 DB BA 60 05 D7 8F 47 " +
            "3B C3 D8 A8 46 C5 6C 3A 10 A6 63 93 5B 19 88 B5 " +
            "EB 5D E6 01 8F 11 93 FD F1 53 A3 06 8D 9E 98 D6 " +
            "9A 1B B0 83 58 7B 77 A2 DC 8A 49 25 2E ED 47 E1 " +
            "50 B5 6D 6B D2 C3 8C AE",
            // Round #8 
            // After Theta
            "F8 26 59 A5 CC F0 B7 AB 8E 24 3B 4A 83 71 47 D2 " +
            "6F E8 39 E9 88 5A 86 39 27 C0 B1 E0 DD 14 BA C8 " +
            "48 B8 67 CC CB 30 08 05 48 78 0C D3 64 F3 0B E9 " +
            "D5 F2 13 3A 54 AA C3 3F 9E B2 8C 05 AD C9 82 F8 " +
            "6D B2 86 EB 00 C0 7C B0 2F C1 AB C0 34 F8 81 CB " +
            "03 B5 B1 2F 23 C3 DC C7 99 CF 4C 4B 42 7E 00 B4 " +
            "7C FA 9E 41 D5 3D 10 C7 03 51 25 30 8E 48 35 00 " +
            "0B A6 A6 54 E7 5E 6D 9D C8 75 BC FD 3F 1C 44 B6 " +
            "A1 BE 8A 48 3A B5 8E 8A BB FF 4D A2 D5 73 6E 9F " +
            "9D C1 86 45 04 E4 0D 77 39 49 C3 83 55 16 5E C6 " +
            "CA 97 B9 5B 54 4E 80 CD 39 19 B1 BA 4D 1F 34 6E " +
            "19 3F 47 41 88 DF 96 7A 7A 88 17 C8 6C CC 26 AC " +
            "79 5A CD 7B DC CC 5A DD",
            // After Rho
            "F8 26 59 A5 CC F0 B7 AB 1D 49 76 94 06 E3 8E A4 " +
            "1B 7A 4E 3A A2 96 61 CE 4D A1 8B 7C 02 1C 0B DE " +
            "86 41 28 40 C2 3D 63 5E 4D 36 BF 90 8E 84 C7 30 " +
            "A1 43 A5 3A FC 53 2D 3F BE A7 2C 63 41 6B B2 20 " +
            "59 C3 75 00 60 3E D8 36 1F B8 FC 12 BC 0A 4C 83 " +
            "1E A8 8D 7D 19 19 E6 3E D0 66 3E 33 2D 09 F9 01 " +
            "0C AA EE 81 38 E6 D3 F7 91 6A 00 06 A2 4A 60 1C " +
            "AA 73 AF B6 CE 05 53 53 FB 7F 38 88 6C 91 EB 78 " +
            "11 49 A7 D6 51 31 D4 57 B7 CF DD FF 26 D1 EA 39 " +
            "BC E1 AE 33 D8 B0 88 80 C6 39 49 C3 83 55 16 5E " +
            "01 36 2B 5F E6 6E 51 39 E5 64 C4 EA 36 7D D0 B8 " +
            "E3 E7 28 08 F1 DB 52 2F 88 17 C8 6C CC 26 AC 7A " +
            "56 77 9E 56 F3 1E 37 B3",
            // After Pi 
            "F8 26 59 A5 CC F0 B7 AB A1 43 A5 3A FC 53 2D 3F " +
            "0C AA EE 81 38 E6 D3 F7 BC E1 AE 33 D8 B0 88 80 " +
            "56 77 9E 56 F3 1E 37 B3 4D A1 8B 7C 02 1C 0B DE " +
            "1F B8 FC 12 BC 0A 4C 83 1E A8 8D 7D 19 19 E6 3E " +
            "11 49 A7 D6 51 31 D4 57 E3 E7 28 08 F1 DB 52 2F " +
            "1D 49 76 94 06 E3 8E A4 BE A7 2C 63 41 6B B2 20 " +
            "91 6A 00 06 A2 4A 60 1C C6 39 49 C3 83 55 16 5E " +
            "01 36 2B 5F E6 6E 51 39 86 41 28 40 C2 3D 63 5E " +
            "4D 36 BF 90 8E 84 C7 30 D0 66 3E 33 2D 09 F9 01 " +
            "B7 CF DD FF 26 D1 EA 39 88 17 C8 6C CC 26 AC 7A " +
            "1B 7A 4E 3A A2 96 61 CE 59 C3 75 00 60 3E D8 36 " +
            "AA 73 AF B6 CE 05 53 53 FB 7F 38 88 6C 91 EB 78 " +
            "E5 64 C4 EA 36 7D D0 B8",
            // After Chi
            "F4 8E 13 24 CC 54 65 6B 11 02 A5 08 3C 43 25 3F " +
            "4E BC FE C5 1B E8 E4 C4 14 E1 EF 92 D4 50 08 88 " +
            "57 36 3A 4C C3 1D 3F A7 4D A1 8A 11 03 0D A9 E2 " +
            "1E F9 DE 90 FC 2A 5C C2 FC 0E 85 75 B9 D3 E4 16 " +
            "1D 49 24 A2 53 35 DD 87 F1 FF 5C 0A 4D D9 16 2E " +
            "1C 01 76 90 A4 E3 CE B8 F8 B6 65 A2 40 7E A4 62 " +
            "90 6C 22 1A C6 60 21 3D DA 70 1D 43 83 D4 98 DA " +
            "A3 90 23 3C A7 66 61 39 16 01 28 63 E3 34 5B 5F " +
            "6A BF 7E 5C 8C 54 C5 08 D8 76 3E 33 E5 2F FD 43 " +
            "B1 8F FD FF 24 C8 A9 3D C1 21 5F FC C0 A6 28 5A " +
            "B9 4A C4 8C 2C 97 62 8F 08 CF 65 08 40 AE 70 1E " +
            "AE 73 6B D4 DC 69 43 D3 E1 65 32 98 EC 13 CA 3E " +
            "A5 E5 F5 EA 76 55 48 88",
            // After Iota  
            "7E 8E 13 24 CC 54 65 6B 11 02 A5 08 3C 43 25 3F " +
            "4E BC FE C5 1B E8 E4 C4 14 E1 EF 92 D4 50 08 88 " +
            "57 36 3A 4C C3 1D 3F A7 4D A1 8A 11 03 0D A9 E2 " +
            "1E F9 DE 90 FC 2A 5C C2 FC 0E 85 75 B9 D3 E4 16 " +
            "1D 49 24 A2 53 35 DD 87 F1 FF 5C 0A 4D D9 16 2E " +
            "1C 01 76 90 A4 E3 CE B8 F8 B6 65 A2 40 7E A4 62 " +
            "90 6C 22 1A C6 60 21 3D DA 70 1D 43 83 D4 98 DA " +
            "A3 90 23 3C A7 66 61 39 16 01 28 63 E3 34 5B 5F " +
            "6A BF 7E 5C 8C 54 C5 08 D8 76 3E 33 E5 2F FD 43 " +
            "B1 8F FD FF 24 C8 A9 3D C1 21 5F FC C0 A6 28 5A " +
            "B9 4A C4 8C 2C 97 62 8F 08 CF 65 08 40 AE 70 1E " +
            "AE 73 6B D4 DC 69 43 D3 E1 65 32 98 EC 13 CA 3E " +
            "A5 E5 F5 EA 76 55 48 88",
            // Round #9 
            // After Theta
            "34 68 F6 94 CB DF 9C 1B 39 D1 BF D8 22 60 20 21 " +
            "DC E4 C9 83 CF D0 D0 E1 82 00 3C 06 B7 EE C7 33 " +
            "D5 CF 25 CC 47 44 67 B3 07 47 6F A1 04 86 50 92 " +
            "36 2A C4 40 E2 09 59 DC 6E 56 B2 33 6D EB D0 33 " +
            "8B A8 F7 36 30 8B 12 3C 73 06 43 8A C9 80 4E 3A " +
            "56 E7 93 20 A3 68 37 C8 D0 65 7F 72 5E 5D A1 7C " +
            "02 34 15 5C 12 58 15 18 4C 91 CE D7 E0 6A 57 61 " +
            "21 69 3C BC 23 3F 39 2D 5C E7 CD D3 E4 BF A2 2F " +
            "42 6C 64 8C 92 77 C0 16 4A 2E 09 75 31 17 C9 66 " +
            "27 6E 2E 6B 47 76 66 86 43 D8 40 7C 44 FF 70 4E " +
            "F3 AC 21 3C 2B 1C 9B FF 20 1C 7F D8 5E 8D 75 00 " +
            "3C 2B 5C 92 08 51 77 F6 77 84 E1 0C 8F AD 05 85 " +
            "27 1C EA 6A F2 0C 10 9C",
            // After Rho
            "34 68 F6 94 CB DF 9C 1B 72 A2 7F B1 45 C0 40 42 " +
            "37 79 F2 E0 33 34 74 38 EB 7E 3C 23 08 C0 63 70 " +
            "22 3A 9B AD 7E 2E 61 3E 4A 60 08 25 79 70 F4 16 " +
            "0C 24 9E 90 C5 6D A3 42 8C 9B 95 EC 4C DB 3A F4 " +
            "D4 7B 1B 98 45 09 9E 45 E8 A4 33 67 30 A4 98 0C " +
            "B6 3A 9F 04 19 45 BB 41 F2 41 97 FD C9 79 75 85 " +
            "E0 92 C0 AA C0 10 A0 A9 D5 AE C2 98 22 9D AF C1 " +
            "DE 91 9F 9C 96 90 34 1E A7 C9 7F 45 5F B8 CE 9B " +
            "8C 51 F2 0E D8 42 88 8D 64 33 25 97 84 BA 98 8B " +
            "CE CC F0 C4 CD 65 ED C8 4E 43 D8 40 7C 44 FF 70 " +
            "6C FE CF B3 86 F0 AC 70 80 70 FC 61 7B 35 D6 01 " +
            "67 85 4B 12 21 EA CE 9E 84 E1 0C 8F AD 05 85 77 " +
            "04 E7 09 87 BA 9A 3C 03",
            // After Pi 
            "34 68 F6 94 CB DF 9C 1B 0C 24 9E 90 C5 6D A3 42 " +
            "E0 92 C0 AA C0 10 A0 A9 CE CC F0 C4 CD 65 ED C8 " +
            "04 E7 09 87 BA 9A 3C 03 EB 7E 3C 23 08 C0 63 70 " +
            "E8 A4 33 67 30 A4 98 0C B6 3A 9F 04 19 45 BB 41 " +
            "8C 51 F2 0E D8 42 88 8D 67 85 4B 12 21 EA CE 9E " +
            "72 A2 7F B1 45 C0 40 42 8C 9B 95 EC 4C DB 3A F4 " +
            "D5 AE C2 98 22 9D AF C1 4E 43 D8 40 7C 44 FF 70 " +
            "6C FE CF B3 86 F0 AC 70 22 3A 9B AD 7E 2E 61 3E " +
            "4A 60 08 25 79 70 F4 16 F2 41 97 FD C9 79 75 85 " +
            "64 33 25 97 84 BA 98 8B 84 E1 0C 8F AD 05 85 77 " +
            "37 79 F2 E0 33 34 74 38 D4 7B 1B 98 45 09 9E 45 " +
            "DE 91 9F 9C 96 90 34 1E A7 C9 7F 45 5F B8 CE 9B " +
            "80 70 FC 61 7B 35 D6 01",
            // After Chi
            "D4 FA B6 BE CB CF 9C B2 02 68 AE D4 C8 08 EE 02 " +
            "E0 B1 C9 A9 F2 8A B0 AA FE C4 06 D4 8C 20 6D D0 " +
            "0C E3 01 87 BE BA 1F 43 FD 64 B0 23 01 81 40 31 " +
            "E0 E5 53 6D F0 A6 98 80 D5 BE 96 14 38 ED FD 53 " +
            "04 2B C6 2F D0 42 A9 ED 67 05 48 56 11 CE 56 92 " +
            "23 86 3D A1 67 C4 C5 43 86 DA 8D AC 10 9B 6A C4 " +
            "F5 12 C5 2B A0 2D AF C1 5C 43 E8 40 3D 44 BF 72 " +
            "E0 E7 4F FF 8E EB 96 C4 92 3B 0C 75 FE 27 60 BF " +
            "4E 52 28 27 7D F2 7C 1C 72 81 9F F5 E0 7C 70 F1 " +
            "46 29 B6 B7 D6 90 F8 83 CC A1 0C 8F AC 55 11 77 " +
            "3D F9 76 E4 A1 A4 54 22 F5 33 7B D9 0C 21 54 C4 " +
            "DE A1 1F BC B6 95 24 1E 90 C0 7D C5 5F B8 EE A3 " +
            "40 72 F5 79 3F 3C 5C 44",
            // After Iota  
            "5C FA B6 BE CB CF 9C B2 02 68 AE D4 C8 08 EE 02 " +
            "E0 B1 C9 A9 F2 8A B0 AA FE C4 06 D4 8C 20 6D D0 " +
            "0C E3 01 87 BE BA 1F 43 FD 64 B0 23 01 81 40 31 " +
            "E0 E5 53 6D F0 A6 98 80 D5 BE 96 14 38 ED FD 53 " +
            "04 2B C6 2F D0 42 A9 ED 67 05 48 56 11 CE 56 92 " +
            "23 86 3D A1 67 C4 C5 43 86 DA 8D AC 10 9B 6A C4 " +
            "F5 12 C5 2B A0 2D AF C1 5C 43 E8 40 3D 44 BF 72 " +
            "E0 E7 4F FF 8E EB 96 C4 92 3B 0C 75 FE 27 60 BF " +
            "4E 52 28 27 7D F2 7C 1C 72 81 9F F5 E0 7C 70 F1 " +
            "46 29 B6 B7 D6 90 F8 83 CC A1 0C 8F AC 55 11 77 " +
            "3D F9 76 E4 A1 A4 54 22 F5 33 7B D9 0C 21 54 C4 " +
            "DE A1 1F BC B6 95 24 1E 90 C0 7D C5 5F B8 EE A3 " +
            "40 72 F5 79 3F 3C 5C 44",
            // Round #10
            // After Theta
            "E4 45 0F B0 CA F5 67 A8 F6 C8 DB C7 43 47 AE F0 " +
            "DF 0D 2C D1 7A 71 5E EA 9C 5D E3 BA D5 6E FE 4A " +
            "26 12 61 14 B3 A7 28 96 45 DB 09 2D 00 BB BB 2B " +
            "14 45 26 7E 7B E9 D8 72 EA 02 73 6C B0 16 13 13 " +
            "66 B2 23 41 89 0C 3A 77 4D F4 28 C5 1C D3 61 47 " +
            "9B 39 84 AF 66 FE 3E 59 72 7A F8 BF 9B D4 2A 36 " +
            "CA AE 20 53 28 D6 41 81 3E DA 0D 2E 64 0A 2C E8 " +
            "CA 16 2F 6C 83 F6 A1 11 2A 84 B5 7B FF 1D 9B A5 " +
            "BA F2 5D 34 F6 BD 3C EE 4D 3D 7A 8D 68 87 9E B1 " +
            "24 B0 53 D9 8F DE 6B 19 E6 50 6C 1C A1 48 26 A2 " +
            "85 46 CF EA A0 9E AF 38 01 93 0E CA 87 6E 14 36 " +
            "E1 1D FA C4 3E 6E CA 5E F2 59 98 AB 06 F6 7D 39 " +
            "6A 83 95 EA 32 21 6B 91",
            // After Rho
            "E4 45 0F B0 CA F5 67 A8 ED 91 B7 8F 87 8E 5C E1 " +
            "77 03 4B B4 5E 9C 97 FA ED E6 AF C4 D9 35 AE 5B " +
            "3D 45 B1 34 91 08 A3 98 02 B0 BB BB 52 B4 9D D0 " +
            "E2 B7 97 8E 2D 47 51 64 84 BA C0 1C 1B AC C5 C4 " +
            "D9 91 A0 44 06 9D 3B 33 1D 76 D4 44 8F 52 CC 31 " +
            "DA CC 21 7C 35 F3 F7 C9 D8 C8 E9 E1 FF 6E 52 AB " +
            "99 42 B1 0E 0A 54 76 05 14 58 D0 7D B4 1B 5C C8 " +
            "B6 41 FB D0 08 65 8B 17 F7 FE 3B 36 4B 55 08 6B " +
            "8B C6 BE 97 C7 5D 57 BE CF D8 A6 1E BD 46 B4 43 " +
            "7B 2D 83 04 76 2A FB D1 A2 E6 50 6C 1C A1 48 26 " +
            "BE E2 14 1A 3D AB 83 7A 04 4C 3A 28 1F BA 51 D8 " +
            "BC 43 9F D8 C7 4D D9 2B 59 98 AB 06 F6 7D 39 F2 " +
            "5A A4 DA 60 A5 BA 4C C8",
            // After Pi 
            "E4 45 0F B0 CA F5 67 A8 E2 B7 97 8E 2D 47 51 64 " +
            "99 42 B1 0E 0A 54 76 05 7B 2D 83 04 76 2A FB D1 " +
            "5A A4 DA 60 A5 BA 4C C8 ED E6 AF C4 D9 35 AE 5B " +
            "1D 76 D4 44 8F 52 CC 31 DA CC 21 7C 35 F3 F7 C9 " +
            "8B C6 BE 97 C7 5D 57 BE BC 43 9F D8 C7 4D D9 2B " +
            "ED 91 B7 8F 87 8E 5C E1 84 BA C0 1C 1B AC C5 C4 " +
            "14 58 D0 7D B4 1B 5C C8 A2 E6 50 6C 1C A1 48 26 " +
            "BE E2 14 1A 3D AB 83 7A 3D 45 B1 34 91 08 A3 98 " +
            "02 B0 BB BB 52 B4 9D D0 D8 C8 E9 E1 FF 6E 52 AB " +
            "CF D8 A6 1E BD 46 B4 43 59 98 AB 06 F6 7D 39 F2 " +
            "77 03 4B B4 5E 9C 97 FA D9 91 A0 44 06 9D 3B 33 " +
            "B6 41 FB D0 08 65 8B 17 F7 FE 3B 36 4B 55 08 6B " +
            "04 4C 3A 28 1F BA 51 D8",
            // After Chi
            "FD 05 2F B0 C8 E5 41 A9 80 9A 95 8E 59 6D D8 B4 " +
            "99 C2 E9 6E 8B C4 72 0D DF 6C 86 94 3C 6F D8 F1 " +
            "58 16 4A 6E 80 B8 5C 8C 2F 6E 8E FC E9 94 9D 93 " +
            "1C 74 4A C7 4D 5E CC 07 EE CD 20 34 35 F3 7F C8 " +
            "CA 62 9E 93 DF 6D 71 EE AC 53 CF D8 C1 0F 99 0B " +
            "FD D1 A7 EE 23 9D 44 E9 26 1C C0 1C 13 0C C5 E2 " +
            "08 58 D4 6F 95 11 DF 90 E3 F7 F3 E9 9E A5 14 A7 " +
            "BE C8 54 0A 25 8B 02 7E E5 0D F1 74 3C 42 E1 B3 " +
            "05 A0 BD A5 52 B4 39 90 C8 C8 E0 E1 BD 57 5B 1B " +
            "EB 9D B6 2E BC 46 36 4B 5B 28 A1 8D B4 C9 25 B2 " +
            "51 43 10 24 56 FC 17 FE 98 2F A0 62 45 8D 3B 5B " +
            "B6 41 FB D8 1C CF DA 87 84 FD 7A A2 0B 51 8E 49 " +
            "8C DC 9A 68 1F BB 79 D9",
            // After Iota  
            "F4 85 2F 30 C8 E5 41 A9 80 9A 95 8E 59 6D D8 B4 " +
            "99 C2 E9 6E 8B C4 72 0D DF 6C 86 94 3C 6F D8 F1 " +
            "58 16 4A 6E 80 B8 5C 8C 2F 6E 8E FC E9 94 9D 93 " +
            "1C 74 4A C7 4D 5E CC 07 EE CD 20 34 35 F3 7F C8 " +
            "CA 62 9E 93 DF 6D 71 EE AC 53 CF D8 C1 0F 99 0B " +
            "FD D1 A7 EE 23 9D 44 E9 26 1C C0 1C 13 0C C5 E2 " +
            "08 58 D4 6F 95 11 DF 90 E3 F7 F3 E9 9E A5 14 A7 " +
            "BE C8 54 0A 25 8B 02 7E E5 0D F1 74 3C 42 E1 B3 " +
            "05 A0 BD A5 52 B4 39 90 C8 C8 E0 E1 BD 57 5B 1B " +
            "EB 9D B6 2E BC 46 36 4B 5B 28 A1 8D B4 C9 25 B2 " +
            "51 43 10 24 56 FC 17 FE 98 2F A0 62 45 8D 3B 5B " +
            "B6 41 FB D8 1C CF DA 87 84 FD 7A A2 0B 51 8E 49 " +
            "8C DC 9A 68 1F BB 79 D9",
            // Round #11
            // After Theta
            "26 06 C1 4D 26 A7 7C 0E 11 52 7F E4 25 42 11 B8 " +
            "8D 8C A4 38 0F A3 AA E3 E5 41 54 2B 28 4C BD 1D " +
            "E4 66 A3 E9 9A AC 85 0A FD ED 60 81 07 D6 A0 34 " +
            "8D BC A0 AD 31 71 05 0B FA 83 6D 62 B1 94 A7 26 " +
            "F0 4F 4C 2C CB 4E 14 02 10 23 26 5F DB 1B 40 8D " +
            "2F 52 49 93 CD DF 79 4E B7 D4 2A 76 6F 23 0C EE " +
            "1C 16 99 39 11 76 07 7E D9 DA 21 56 8A 86 71 4B " +
            "02 B8 BD 8D 3F 9F DB F8 37 8E 1F 09 D2 00 DC 14 " +
            "94 68 57 CF 2E 9B F0 9C DC 86 AD B7 39 30 83 F5 " +
            "D1 B0 64 91 A8 65 53 A7 E7 58 48 0A AE DD FC 34 " +
            "83 C0 FE 59 B8 BE 2A 59 09 E7 4A 08 39 A2 F2 57 " +
            "A2 0F B6 8E 98 A8 02 69 BE D0 A8 1D 1F 72 EB A5 " +
            "30 AC 73 EF 05 AF A0 5F",
            // After Rho
            "26 06 C1 4D 26 A7 7C 0E 23 A4 FE C8 4B 84 22 70 " +
            "23 23 29 CE C3 A8 EA 78 C2 D4 DB 51 1E 44 B5 82 " +
            "64 2D 54 20 37 1B 4D D7 78 60 0D 4A D3 DF 0E 16 " +
            "DA 1A 13 57 B0 D0 C8 0B 89 FE 60 9B 58 2C E5 A9 " +
            "27 26 96 65 27 0A 01 F8 01 D4 08 31 62 F2 B5 BD " +
            "7A 91 4A 9A 6C FE CE 73 B8 DF 52 AB D8 BD 8D 30 " +
            "CC 89 B0 3B F0 E3 B0 C8 0D E3 96 B2 B5 43 AC 14 " +
            "C6 9F CF 6D 7C 01 DC DE 12 A4 01 B8 29 6E 1C 3F " +
            "EA D9 65 13 9E 93 12 ED C1 7A 6E C3 D6 DB 1C 98 " +
            "6C EA 34 1A 96 2C 12 B5 34 E7 58 48 0A AE DD FC " +
            "AA 64 0D 02 FB 67 E1 FA 25 9C 2B 21 E4 88 CA 5F " +
            "F4 C1 D6 11 13 55 20 4D D0 A8 1D 1F 72 EB A5 BE " +
            "E8 17 0C EB DC 7B C1 2B",
            // After Pi 
            "26 06 C1 4D 26 A7 7C 0E DA 1A 13 57 B0 D0 C8 0B " +
            "CC 89 B0 3B F0 E3 B0 C8 6C EA 34 1A 96 2C 12 B5 " +
            "E8 17 0C EB DC 7B C1 2B C2 D4 DB 51 1E 44 B5 82 " +
            "01 D4 08 31 62 F2 B5 BD 7A 91 4A 9A 6C FE CE 73 " +
            "EA D9 65 13 9E 93 12 ED F4 C1 D6 11 13 55 20 4D " +
            "23 A4 FE C8 4B 84 22 70 89 FE 60 9B 58 2C E5 A9 " +
            "0D E3 96 B2 B5 43 AC 14 34 E7 58 48 0A AE DD FC " +
            "AA 64 0D 02 FB 67 E1 FA 64 2D 54 20 37 1B 4D D7 " +
            "78 60 0D 4A D3 DF 0E 16 B8 DF 52 AB D8 BD 8D 30 " +
            "C1 7A 6E C3 D6 DB 1C 98 D0 A8 1D 1F 72 EB A5 BE " +
            "23 23 29 CE C3 A8 EA 78 27 26 96 65 27 0A 01 F8 " +
            "C6 9F CF 6D 7C 01 DC DE 12 A4 01 B8 29 6E 1C 3F " +
            "25 9C 2B 21 E4 88 CA 5F",
            // After Chi
            "22 87 61 65 66 84 4C CE FA 78 17 57 B6 DC CA 3E " +
            "4C 9C B8 DA B8 B0 71 C2 6A EA F5 1E B4 A8 2E B1 " +
            "30 0F 1E F9 4C 2B 41 2A B8 D5 99 DB 12 48 FF C0 " +
            "81 9C 2D 30 F0 F3 A5 31 6E 91 D8 9A 6D BA EE 73 " +
            "E8 CD 6C 53 92 93 87 6F F5 C1 D6 31 73 E7 20 70 " +
            "27 A5 68 E8 EE C7 2A 64 B9 FA 28 D3 52 80 B4 41 " +
            "87 E3 93 B0 44 02 8C 16 35 67 AA 80 0A 2E DF FC " +
            "22 3E 0D 11 EB 4F 24 73 E4 B2 06 81 3F 3B CC F7 " +
            "39 40 21 0A D5 9D 1E 9E A8 5F 43 B7 F8 9D 2C 16 " +
            "E5 7F 2E E3 D3 CB 54 D9 C8 E8 14 55 B2 2F A7 BE " +
            "E3 BA 60 C6 9B A9 36 7E 37 06 96 F5 26 64 01 D9 " +
            "E3 87 E5 6C B8 81 1E 9E 10 87 01 76 2A 4E 3C 1F " +
            "21 98 BD 00 C0 8A CB DF",
            // After Iota  
            "28 87 61 E5 66 84 4C CE FA 78 17 57 B6 DC CA 3E " +
            "4C 9C B8 DA B8 B0 71 C2 6A EA F5 1E B4 A8 2E B1 " +
            "30 0F 1E F9 4C 2B 41 2A B8 D5 99 DB 12 48 FF C0 " +
            "81 9C 2D 30 F0 F3 A5 31 6E 91 D8 9A 6D BA EE 73 " +
            "E8 CD 6C 53 92 93 87 6F F5 C1 D6 31 73 E7 20 70 " +
            "27 A5 68 E8 EE C7 2A 64 B9 FA 28 D3 52 80 B4 41 " +
            "87 E3 93 B0 44 02 8C 16 35 67 AA 80 0A 2E DF FC " +
            "22 3E 0D 11 EB 4F 24 73 E4 B2 06 81 3F 3B CC F7 " +
            "39 40 21 0A D5 9D 1E 9E A8 5F 43 B7 F8 9D 2C 16 " +
            "E5 7F 2E E3 D3 CB 54 D9 C8 E8 14 55 B2 2F A7 BE " +
            "E3 BA 60 C6 9B A9 36 7E 37 06 96 F5 26 64 01 D9 " +
            "E3 87 E5 6C B8 81 1E 9E 10 87 01 76 2A 4E 3C 1F " +
            "21 98 BD 00 C0 8A CB DF",
            // Round #12
            // After Theta
            "BE B6 47 FE 0E 0F ED 95 96 EA 4B 90 2A 6C EB 83 " +
            "05 B4 24 21 F5 C7 88 03 98 DC 79 2D 28 F1 5D 0E " +
            "13 48 EF 82 E4 89 98 08 2E E4 BF C0 7A C3 5E 9B " +
            "ED 0E 71 F7 6C 43 84 8C 27 B9 44 61 20 CD 17 B2 " +
            "1A FB E0 60 0E CA F4 D0 D6 86 27 4A DB 45 F9 52 " +
            "B1 94 4E F3 86 4C 8B 3F D5 68 74 14 CE 30 95 FC " +
            "CE CB 0F 4B 09 75 75 D7 C7 51 26 B3 96 77 AC 43 " +
            "01 79 FC 6A 43 ED FD 51 72 83 20 9A 57 B0 6D AC " +
            "55 D2 7D CD 49 2D 3F 23 E1 77 DF 4C B5 EA D5 D7 " +
            "17 49 A2 D0 4F 92 27 66 EB AF E5 2E 1A 8D 7E 9C " +
            "75 8B 46 DD F3 22 97 25 5B 94 CA 32 BA D4 20 64 " +
            "AA AF 79 97 F5 F6 E7 5F E2 B1 8D 45 B6 17 4F A0 " +
            "02 DF 4C 7B 68 28 12 FD",
            // After Rho
            "BE B6 47 FE 0E 0F ED 95 2D D5 97 20 55 D8 D6 07 " +
            "01 2D 49 48 FD 31 E2 40 12 DF E5 80 C9 9D D7 82 " +
            "4F C4 44 98 40 7A 17 24 AC 37 EC B5 E9 42 FE 0B " +
            "77 CF 36 44 C8 D8 EE 10 EC 49 2E 51 18 48 F3 85 " +
            "7D 70 30 07 65 7A 68 8D 94 2F 65 6D 78 A2 B4 5D " +
            "89 A5 74 9A 37 64 5A FC F2 57 A3 D1 51 38 C3 54 " +
            "58 4A A8 AB BB 76 5E 7E EF 58 87 8E A3 4C 66 2D " +
            "B5 A1 F6 FE A8 80 3C 7E 34 AF 60 DB 58 E5 06 41 " +
            "AF 39 A9 E5 67 A4 4A BA EA EB F0 BB 6F A6 5A F5 " +
            "F2 C4 EC 22 49 14 FA 49 9C EB AF E5 2E 1A 8D 7E " +
            "5C 96 D4 2D 1A 75 CF 8B 6D 51 2A CB E8 52 83 90 " +
            "F5 35 EF B2 DE FE FC 4B B1 8D 45 B6 17 4F A0 E2 " +
            "44 BF C0 37 D3 1E 1A 8A",
            // After Pi 
            "BE B6 47 FE 0E 0F ED 95 77 CF 36 44 C8 D8 EE 10 " +
            "58 4A A8 AB BB 76 5E 7E F2 C4 EC 22 49 14 FA 49 " +
            "44 BF C0 37 D3 1E 1A 8A 12 DF E5 80 C9 9D D7 82 " +
            "94 2F 65 6D 78 A2 B4 5D 89 A5 74 9A 37 64 5A FC " +
            "AF 39 A9 E5 67 A4 4A BA F5 35 EF B2 DE FE FC 4B " +
            "2D D5 97 20 55 D8 D6 07 EC 49 2E 51 18 48 F3 85 " +
            "EF 58 87 8E A3 4C 66 2D 9C EB AF E5 2E 1A 8D 7E " +
            "5C 96 D4 2D 1A 75 CF 8B 4F C4 44 98 40 7A 17 24 " +
            "AC 37 EC B5 E9 42 FE 0B F2 57 A3 D1 51 38 C3 54 " +
            "EA EB F0 BB 6F A6 5A F5 B1 8D 45 B6 17 4F A0 E2 " +
            "01 2D 49 48 FD 31 E2 40 7D 70 30 07 65 7A 68 8D " +
            "B5 A1 F6 FE A8 80 3C 7E 34 AF 60 DB 58 E5 06 41 " +
            "6D 51 2A CB E8 52 83 90",
            // After Chi
            "B6 B6 CF 55 3D 29 FD FB D5 4B 72 44 88 D8 4E 11 " +
            "5C 71 A8 BE 29 7C 5E FC 48 C4 EB EA 45 15 1F 5C " +
            "05 F6 F0 37 13 CE 18 8A 1B 5F F5 12 CE D9 9D 22 " +
            "B2 37 EC 08 38 22 B4 5F D9 A1 32 88 AF 3E EE BD " +
            "AD F3 A9 E5 66 A5 49 3A 71 15 EF DF EE DC DC 16 " +
            "2E C5 16 AE F6 DC D2 2F FC EA 06 30 14 5A 7A D7 " +
            "AF 4C D7 86 B3 29 24 AC BD AA AC E5 6B 92 9D 7A " +
            "9C 9E FC 7C 12 75 EE 0B 1D 84 47 D8 50 42 16 70 " +
            "A4 9F BC 9F C7 C4 E6 AA E3 53 A6 D5 41 71 63 56 " +
            "A4 AB F0 B3 2F 96 4D F1 11 BE ED 93 BE 4F 48 E9 " +
            "81 AC 8F B0 75 B1 F6 32 7D 7E 30 06 35 1F 6A 8C " +
            "FC F1 FC FE 08 92 BD EE 34 83 21 DB 4D C4 66 01 " +
            "11 01 1A CC E8 18 8B 1D",
            // After Iota  
            "3D 36 CF D5 3D 29 FD FB D5 4B 72 44 88 D8 4E 11 " +
            "5C 71 A8 BE 29 7C 5E FC 48 C4 EB EA 45 15 1F 5C " +
            "05 F6 F0 37 13 CE 18 8A 1B 5F F5 12 CE D9 9D 22 " +
            "B2 37 EC 08 38 22 B4 5F D9 A1 32 88 AF 3E EE BD " +
            "AD F3 A9 E5 66 A5 49 3A 71 15 EF DF EE DC DC 16 " +
            "2E C5 16 AE F6 DC D2 2F FC EA 06 30 14 5A 7A D7 " +
            "AF 4C D7 86 B3 29 24 AC BD AA AC E5 6B 92 9D 7A " +
            "9C 9E FC 7C 12 75 EE 0B 1D 84 47 D8 50 42 16 70 " +
            "A4 9F BC 9F C7 C4 E6 AA E3 53 A6 D5 41 71 63 56 " +
            "A4 AB F0 B3 2F 96 4D F1 11 BE ED 93 BE 4F 48 E9 " +
            "81 AC 8F B0 75 B1 F6 32 7D 7E 30 06 35 1F 6A 8C " +
            "FC F1 FC FE 08 92 BD EE 34 83 21 DB 4D C4 66 01 " +
            "11 01 1A CC E8 18 8B 1D",
            // Round #13
            // After Theta
            "50 1A F3 D4 29 EF 0C E6 2B B3 B8 73 51 17 89 0F " +
            "8F 6D C3 5F 2A E7 92 9A AD 7F D5 E7 4A FC 87 CE " +
            "E4 4A 06 B6 79 00 5D 0E 76 73 C9 13 DA 1F 6C 3F " +
            "4C CF 26 3F E1 ED 73 41 0A BD 59 69 AC A5 22 DB " +
            "48 48 97 E8 69 4C D1 A8 90 A9 19 5E 84 12 99 92 " +
            "43 E9 2A AF E2 1A 23 32 02 12 CC 07 CD 95 BD C9 " +
            "7C 50 BC 67 B0 B2 E8 CA 58 11 92 E8 64 7B 05 E8 " +
            "7D 22 0A FD 78 BB AB 8F 70 A8 7B D9 44 84 E7 6D " +
            "5A 67 76 A8 1E 0B 21 B4 30 4F CD 34 42 EA AF 30 " +
            "41 10 CE BE 20 7F D5 63 F0 02 1B 12 D4 81 0D 6D " +
            "EC 80 B3 B1 61 77 07 2F 83 86 FA 31 EC D0 AD 92 " +
            "2F ED 97 1F 0B 09 71 88 D1 38 1F D6 42 2D FE 93 " +
            "F0 BD EC 4D 82 D6 CE 99",
            // After Rho
            "50 1A F3 D4 29 EF 0C E6 56 66 71 E7 A2 2E 12 1F " +
            "63 DB F0 97 CA B9 A4 E6 C4 7F E8 DC FA 57 7D AE " +
            "03 E8 72 20 57 32 B0 CD A1 FD C1 F6 63 37 97 3C " +
            "F2 13 DE 3E 17 C4 F4 6C B6 42 6F 56 1A 6B A9 C8 " +
            "A4 4B F4 34 A6 68 54 24 91 29 09 99 9A E1 45 28 " +
            "19 4A 57 79 15 D7 18 91 26 0B 48 30 1F 34 57 F6 " +
            "3D 83 95 45 57 E6 83 E2 F6 0A D0 B1 22 24 D1 C9 " +
            "7E BC DD D5 C7 3E 11 85 B2 89 08 CF DB E0 50 F7 " +
            "0E D5 63 21 84 56 EB CC 57 18 98 A7 66 1A 21 F5 " +
            "AF 7A 2C 08 C2 D9 17 E4 6D F0 02 1B 12 D4 81 0D " +
            "1D BC B0 03 CE C6 86 DD 0E 1A EA C7 B0 43 B7 4A " +
            "A5 FD F2 63 21 21 0E F1 38 1F D6 42 2D FE 93 D1 " +
            "73 26 7C 2F 7B 93 A0 B5",
            // After Pi 
            "50 1A F3 D4 29 EF 0C E6 F2 13 DE 3E 17 C4 F4 6C " +
            "3D 83 95 45 57 E6 83 E2 AF 7A 2C 08 C2 D9 17 E4 " +
            "73 26 7C 2F 7B 93 A0 B5 C4 7F E8 DC FA 57 7D AE " +
            "91 29 09 99 9A E1 45 28 19 4A 57 79 15 D7 18 91 " +
            "0E D5 63 21 84 56 EB CC A5 FD F2 63 21 21 0E F1 " +
            "56 66 71 E7 A2 2E 12 1F B6 42 6F 56 1A 6B A9 C8 " +
            "F6 0A D0 B1 22 24 D1 C9 6D F0 02 1B 12 D4 81 0D " +
            "1D BC B0 03 CE C6 86 DD 03 E8 72 20 57 32 B0 CD " +
            "A1 FD C1 F6 63 37 97 3C 26 0B 48 30 1F 34 57 F6 " +
            "57 18 98 A7 66 1A 21 F5 38 1F D6 42 2D FE 93 D1 " +
            "63 DB F0 97 CA B9 A4 E6 A4 4B F4 34 A6 68 54 24 " +
            "7E BC DD D5 C7 3E 11 85 B2 89 08 CF DB E0 50 F7 " +
            "0E 1A EA C7 B0 43 B7 4A",
            // After Chi
            "5D 9A F2 95 69 CD 0F 64 70 6B F6 36 97 DD E0 68 " +
            "6D 87 C5 62 6E E4 23 F3 AF 62 AF D8 C2 B5 1B A6 " +
            "D1 27 70 05 6D 93 50 BD CC 3D BE BC FF 41 65 3F " +
            "97 BC 29 99 1A E1 A6 64 B8 62 C7 3B 34 F6 1C A0 " +
            "4E D7 6B BD 5E 00 9A C2 B4 FD F3 62 21 81 0E F1 " +
            "16 6E E1 46 82 2A 42 1E BF B2 6D 5C 0A BB A9 CC " +
            "E6 06 60 B1 EE 26 D7 19 2F B2 43 FF 32 FC 91 0F " +
            "BD BC BE 13 D6 87 2F 1D 05 EA 7A 20 4B 32 F0 0F " +
            "F0 ED 51 71 03 3D B7 3D 0E 0C 0E 70 16 D0 C5 F6 " +
            "54 F8 B8 87 34 1A 01 F9 98 0A 57 94 0D FB 94 E1 " +
            "39 6F F9 56 8B AF A5 67 24 4A F4 3E BE A8 14 56 " +
            "72 AE 3F D5 E7 3D B6 8D D3 48 18 DF 91 58 50 53 " +
            "8A 1A EE E7 94 03 E7 4A",
            // After Iota  
            "D6 9A F2 95 69 CD 0F E4 70 6B F6 36 97 DD E0 68 " +
            "6D 87 C5 62 6E E4 23 F3 AF 62 AF D8 C2 B5 1B A6 " +
            "D1 27 70 05 6D 93 50 BD CC 3D BE BC FF 41 65 3F " +
            "97 BC 29 99 1A E1 A6 64 B8 62 C7 3B 34 F6 1C A0 " +
            "4E D7 6B BD 5E 00 9A C2 B4 FD F3 62 21 81 0E F1 " +
            "16 6E E1 46 82 2A 42 1E BF B2 6D 5C 0A BB A9 CC " +
            "E6 06 60 B1 EE 26 D7 19 2F B2 43 FF 32 FC 91 0F " +
            "BD BC BE 13 D6 87 2F 1D 05 EA 7A 20 4B 32 F0 0F " +
            "F0 ED 51 71 03 3D B7 3D 0E 0C 0E 70 16 D0 C5 F6 " +
            "54 F8 B8 87 34 1A 01 F9 98 0A 57 94 0D FB 94 E1 " +
            "39 6F F9 56 8B AF A5 67 24 4A F4 3E BE A8 14 56 " +
            "72 AE 3F D5 E7 3D B6 8D D3 48 18 DF 91 58 50 53 " +
            "8A 1A EE E7 94 03 E7 4A",
            // Round #14
            // After Theta
            "05 69 59 EA 1F 84 95 48 DE A5 7E B5 C9 54 AA A6 " +
            "72 2B 9D 5A 43 E0 ED DA 75 CE F4 9A 81 B6 84 63 " +
            "F9 08 0B F5 CE EF EB 26 1F CE 15 C3 89 08 FF 93 " +
            "39 72 A1 1A 44 68 EC AA A7 CE 9F 03 19 F2 D2 89 " +
            "94 7B 30 FF 1D 03 05 07 9C D2 88 92 82 FD B5 6A " +
            "C5 9D 4A 39 F4 63 D8 B2 11 7C E5 DF 54 32 E3 02 " +
            "F9 AA 38 89 C3 22 19 30 F5 1E 18 BD 71 FF 0E CA " +
            "95 93 C5 E3 75 FB 94 86 D6 19 D1 5F 3D 7B 6A A3 " +
            "5E 23 D9 F2 5D B4 FD F3 11 A0 56 48 3B D4 0B DF " +
            "8E 54 E3 C5 77 19 9E 3C B0 25 2C 64 AE 87 2F 7A " +
            "EA 9C 52 29 FD E6 3F CB 8A 84 7C BD E0 21 5E 98 " +
            "6D 02 67 ED CA 39 78 A4 09 E4 43 9D D2 5B CF 96 " +
            "A2 35 95 17 37 7F 5C D1",
            // After Rho
            "05 69 59 EA 1F 84 95 48 BD 4B FD 6A 93 A9 54 4D " +
            "DC 4A A7 D6 10 78 BB B6 68 4B 38 56 E7 4C AF 19 " +
            "7E 5F 37 C9 47 58 A8 77 9C 88 F0 3F F9 E1 5C 31 " +
            "AA 41 84 C6 AE 9A 23 17 E2 A9 F3 E7 40 86 BC 74 " +
            "3D 98 FF 8E 81 82 03 CA 5F AB C6 29 8D 28 29 D8 " +
            "2D EE 54 CA A1 1F C3 96 0B 44 F0 95 7F 53 C9 8C " +
            "49 1C 16 C9 80 C9 57 C5 FE 1D 94 EB 3D 30 7A E3 " +
            "F1 BA 7D 4A C3 CA C9 E2 BF 7A F6 D4 46 AD 33 A2 " +
            "5B BE 8B B6 7F DE 6B 24 85 EF 08 50 2B A4 1D EA " +
            "C3 93 C7 91 6A BC F8 2E 7A B0 25 2C 64 AE 87 2F " +
            "FF 2C AB 73 4A A5 F4 9B 2A 12 F2 F5 82 87 78 61 " +
            "4D E0 AC 5D 39 07 8F B4 E4 43 9D D2 5B CF 96 09 " +
            "57 B4 68 4D E5 C5 CD 1F",
            // After Pi 
            "05 69 59 EA 1F 84 95 48 AA 41 84 C6 AE 9A 23 17 " +
            "49 1C 16 C9 80 C9 57 C5 C3 93 C7 91 6A BC F8 2E " +
            "57 B4 68 4D E5 C5 CD 1F 68 4B 38 56 E7 4C AF 19 " +
            "5F AB C6 29 8D 28 29 D8 2D EE 54 CA A1 1F C3 96 " +
            "5B BE 8B B6 7F DE 6B 24 4D E0 AC 5D 39 07 8F B4 " +
            "BD 4B FD 6A 93 A9 54 4D E2 A9 F3 E7 40 86 BC 74 " +
            "FE 1D 94 EB 3D 30 7A E3 7A B0 25 2C 64 AE 87 2F " +
            "FF 2C AB 73 4A A5 F4 9B 7E 5F 37 C9 47 58 A8 77 " +
            "9C 88 F0 3F F9 E1 5C 31 0B 44 F0 95 7F 53 C9 8C " +
            "85 EF 08 50 2B A4 1D EA E4 43 9D D2 5B CF 96 09 " +
            "DC 4A A7 D6 10 78 BB B6 3D 98 FF 8E 81 82 03 CA " +
            "F1 BA 7D 4A C3 CA C9 E2 BF 7A F6 D4 46 AD 33 A2 " +
            "2A 12 F2 F5 82 87 78 61",
            // After Chi
            "44 75 4B E3 1F C5 C1 88 28 C2 45 D6 C4 AE 8B 3D " +
            "5D 38 3E 85 05 88 52 D4 C3 DA D6 33 70 BC E8 6E " +
            "FD B4 EC 49 45 DF EF 08 48 0F 28 94 C7 5B 6D 1F " +
            "0D BB 4D 1D D3 E8 01 F8 29 AE 70 83 A1 1E 47 06 " +
            "7B B5 9B B4 B9 96 4B 2D 5A 40 6A 74 31 27 8F 74 " +
            "A1 5F F9 62 AE 99 16 CE E2 09 D2 E3 00 08 39 78 " +
            "7B 11 1E B8 37 31 0A 73 7A F3 71 24 F5 A6 87 6B " +
            "BD 8C A9 F6 0A A3 5C AB 7D 1B 37 49 41 4A 29 FB " +
            "18 23 F8 7F F9 45 48 53 6B 44 65 17 2F 18 4B 8D " +
            "9F F3 2A 59 2F B4 35 9C 64 C3 5D E4 E3 6E C2 09 " +
            "1C 68 A7 96 52 30 73 96 33 D8 7D 1A 85 A7 31 CA " +
            "F1 BA 7D 6B 43 C8 81 A3 6B 32 F3 D6 56 D5 B0 34 " +
            "0B 82 AA FD 03 05 78 29",
            // After Iota  
            "CD F5 4B E3 1F C5 C1 08 28 C2 45 D6 C4 AE 8B 3D " +
            "5D 38 3E 85 05 88 52 D4 C3 DA D6 33 70 BC E8 6E " +
            "FD B4 EC 49 45 DF EF 08 48 0F 28 94 C7 5B 6D 1F " +
            "0D BB 4D 1D D3 E8 01 F8 29 AE 70 83 A1 1E 47 06 " +
            "7B B5 9B B4 B9 96 4B 2D 5A 40 6A 74 31 27 8F 74 " +
            "A1 5F F9 62 AE 99 16 CE E2 09 D2 E3 00 08 39 78 " +
            "7B 11 1E B8 37 31 0A 73 7A F3 71 24 F5 A6 87 6B " +
            "BD 8C A9 F6 0A A3 5C AB 7D 1B 37 49 41 4A 29 FB " +
            "18 23 F8 7F F9 45 48 53 6B 44 65 17 2F 18 4B 8D " +
            "9F F3 2A 59 2F B4 35 9C 64 C3 5D E4 E3 6E C2 09 " +
            "1C 68 A7 96 52 30 73 96 33 D8 7D 1A 85 A7 31 CA " +
            "F1 BA 7D 6B 43 C8 81 A3 6B 32 F3 D6 56 D5 B0 34 " +
            "0B 82 AA FD 03 05 78 29",
            // Round #15
            // After Theta
            "60 DB 2C AB 57 AD D2 B6 46 E7 DF 98 5E 3C C1 96 " +
            "DC 09 AB 91 E4 FE DB F1 BD D1 2E 54 B2 AA 31 0E " +
            "40 45 1C F1 CB C8 8E E1 E5 21 4F DC 8F 33 7E A1 " +
            "63 9E D7 53 49 7A 4B 53 A8 9F E5 97 40 68 CE 23 " +
            "05 BE 63 D3 7B 80 92 4D E7 B1 9A CC BF 30 EE 9D " +
            "0C 71 9E 2A E6 F1 05 70 8C 2C 48 AD 9A 9A 73 D3 " +
            "FA 20 8B AC D6 47 83 56 04 F8 89 43 37 B0 5E 0B " +
            "00 7D 59 4E 84 B4 3D 42 D0 35 50 01 09 22 3A 45 " +
            "76 06 62 31 63 D7 02 F8 EA 75 F0 03 CE 6E C2 A8 " +
            "E1 F8 D2 3E ED A2 EC FC D9 32 AD 5C 6D 79 A3 E0 " +
            "B1 46 C0 DE 1A 58 60 28 5D FD E7 54 1F 35 7B 61 " +
            "70 8B E8 7F A2 BE 08 86 15 39 0B B1 94 C3 69 54 " +
            "B6 73 5A 45 8D 12 19 C0",
            // After Rho
            "60 DB 2C AB 57 AD D2 B6 8D CE BF 31 BD 78 82 2D " +
            "77 C2 6A 24 B9 FF 76 3C AB 1A E3 D0 1B ED 42 25 " +
            "46 76 0C 07 2A E2 88 5F FD 38 E3 17 5A 1E F2 C4 " +
            "3D 95 A4 B7 34 35 E6 79 08 EA 67 F9 25 10 9A F3 " +
            "DF B1 E9 3D 40 C9 A6 02 E3 DE 79 1E AB C9 FC 0B " +
            "63 88 F3 54 31 8F 2F 80 4D 33 B2 20 B5 6A 6A CE " +
            "64 B5 3E 1A B4 D2 07 59 60 BD 16 08 F0 13 87 6E " +
            "27 42 DA 1E 21 80 BE 2C 02 12 44 74 8A A0 6B A0 " +
            "2C 66 EC 5A 00 DF CE 40 61 54 F5 3A F8 01 67 37 " +
            "94 9D 3F 1C 5F DA A7 5D E0 D9 32 AD 5C 6D 79 A3 " +
            "81 A1 C4 1A 01 7B 6B 60 75 F5 9F 53 7D D4 EC 85 " +
            "6E 11 FD 4F D4 17 C1 10 39 0B B1 94 C3 69 54 15 " +
            "06 B0 ED 9C 56 51 A3 44",
            // After Pi 
            "60 DB 2C AB 57 AD D2 B6 3D 95 A4 B7 34 35 E6 79 " +
            "64 B5 3E 1A B4 D2 07 59 94 9D 3F 1C 5F DA A7 5D " +
            "06 B0 ED 9C 56 51 A3 44 AB 1A E3 D0 1B ED 42 25 " +
            "E3 DE 79 1E AB C9 FC 0B 63 88 F3 54 31 8F 2F 80 " +
            "2C 66 EC 5A 00 DF CE 40 6E 11 FD 4F D4 17 C1 10 " +
            "8D CE BF 31 BD 78 82 2D 08 EA 67 F9 25 10 9A F3 " +
            "60 BD 16 08 F0 13 87 6E E0 D9 32 AD 5C 6D 79 A3 " +
            "81 A1 C4 1A 01 7B 6B 60 46 76 0C 07 2A E2 88 5F " +
            "FD 38 E3 17 5A 1E F2 C4 4D 33 B2 20 B5 6A 6A CE " +
            "61 54 F5 3A F8 01 67 37 39 0B B1 94 C3 69 54 15 " +
            "77 C2 6A 24 B9 FF 76 3C DF B1 E9 3D 40 C9 A6 02 " +
            "27 42 DA 1E 21 80 BE 2C 02 12 44 74 8A A0 6B A0 " +
            "75 F5 9F 53 7D D4 EC 85",
            // After Chi
            "20 FB 36 A3 D7 6F D3 B6 AD 9D A5 B3 7F 3D 46 7D " +
            "66 95 FE 9A B4 D3 07 59 F4 D6 3F 3F 5E 76 F7 EF " +
            "1B B4 6D 88 76 41 87 0D AB 1A 61 90 0B EB 41 A5 " +
            "EF B8 75 14 AB 99 3C 4B 21 99 E2 51 E5 8F 2E 90 " +
            "AD 6C EE CA 0B 37 CC 65 2E D5 E5 41 74 17 7D 1A " +
            "ED DB AF 31 6D 7B 87 21 88 AA 47 5C 29 7C E2 72 " +
            "61 9D D2 1A F1 01 85 2E EC 97 09 8C E0 6D F9 AE " +
            "81 81 84 D2 01 7B 73 B2 46 75 1C 27 8F 82 80 55 " +
            "DD 7C A6 0D 12 1F F7 F5 55 38 B2 A4 B6 02 7A CE " +
            "27 20 F9 39 D0 83 EF 7D 80 03 52 84 93 75 26 95 " +
            "57 80 78 26 98 FF 6E 10 DF A1 ED 5D CA E9 E7 82 " +
            "52 A7 41 1D 54 D4 3A 29 00 10 24 50 0A 8B 79 98 " +
            "FD C4 1E 4A 3D D4 6C 87",
            // After Iota  
            "23 7B 36 A3 D7 6F D3 36 AD 9D A5 B3 7F 3D 46 7D " +
            "66 95 FE 9A B4 D3 07 59 F4 D6 3F 3F 5E 76 F7 EF " +
            "1B B4 6D 88 76 41 87 0D AB 1A 61 90 0B EB 41 A5 " +
            "EF B8 75 14 AB 99 3C 4B 21 99 E2 51 E5 8F 2E 90 " +
            "AD 6C EE CA 0B 37 CC 65 2E D5 E5 41 74 17 7D 1A " +
            "ED DB AF 31 6D 7B 87 21 88 AA 47 5C 29 7C E2 72 " +
            "61 9D D2 1A F1 01 85 2E EC 97 09 8C E0 6D F9 AE " +
            "81 81 84 D2 01 7B 73 B2 46 75 1C 27 8F 82 80 55 " +
            "DD 7C A6 0D 12 1F F7 F5 55 38 B2 A4 B6 02 7A CE " +
            "27 20 F9 39 D0 83 EF 7D 80 03 52 84 93 75 26 95 " +
            "57 80 78 26 98 FF 6E 10 DF A1 ED 5D CA E9 E7 82 " +
            "52 A7 41 1D 54 D4 3A 29 00 10 24 50 0A 8B 79 98 " +
            "FD C4 1E 4A 3D D4 6C 87",
            // Round #16
            // After Theta
            "7A F9 CE 21 31 BF 00 E6 9B CE 43 60 5D A9 64 8B " +
            "8B FC 28 11 4F B5 27 E8 46 97 82 FD 47 E4 9C 80 " +
            "60 37 50 9F 55 60 24 23 F2 98 99 12 ED 3B 92 75 " +
            "D9 EB 93 C7 89 0D 1E BD CC F0 34 DA 1E E9 0E 21 " +
            "1F 2D 53 08 12 A5 A7 0A 55 56 D8 56 57 36 DE 34 " +
            "B4 59 57 B3 8B AB 54 F1 BE F9 A1 8F 0B E8 C0 84 " +
            "8C F4 04 91 0A 67 A5 9F 5E D6 B4 4E F9 FF 92 C1 " +
            "FA 02 B9 C5 22 5A D0 9C 1F F7 E4 A5 69 52 53 85 " +
            "EB 2F 40 DE 30 8B D5 03 B8 51 64 2F 4D 64 5A 7F " +
            "95 61 44 FB C9 11 84 12 FB 80 6F 93 B0 54 85 BB " +
            "0E 02 80 A4 7E 2F BD C0 E9 F2 0B 8E E8 7D C5 74 " +
            "BF CE 97 96 AF B2 1A 98 B2 51 99 92 13 19 12 F7 " +
            "86 47 23 5D 1E F5 CF A9",
            // After Rho
            "7A F9 CE 21 31 BF 00 E6 37 9D 87 C0 BA 52 C9 16 " +
            "22 3F 4A C4 53 ED 09 FA 44 CE 09 68 74 29 D8 7F " +
            "02 23 19 01 BB 81 FA AC D1 BE 23 59 27 8F 99 29 " +
            "79 9C D8 E0 D1 9B BD 3E 08 33 3C 8D B6 47 BA 43 " +
            "96 29 04 89 D2 53 85 8F E3 4D 53 65 85 6D 75 65 " +
            "A7 CD BA 9A 5D 5C A5 8A 13 FA E6 87 3E 2E A0 03 " +
            "88 54 38 2B FD 64 A4 27 FF 25 83 BD AC 69 9D F2 " +
            "62 11 2D 68 4E 7D 81 DC 4B D3 A4 A6 0A 3F EE C9 " +
            "C8 1B 66 B1 7A 60 FD 05 AD 3F DC 28 B2 97 26 32 " +
            "82 50 A2 32 8C 68 3F 39 BB FB 80 6F 93 B0 54 85 " +
            "F4 02 3B 08 00 92 FA BD A5 CB 2F 38 A2 F7 15 D3 " +
            "D7 F9 D2 F2 55 56 03 F3 51 99 92 13 19 12 F7 B2 " +
            "73 AA E1 D1 48 97 47 FD",
            // After Pi 
            "7A F9 CE 21 31 BF 00 E6 79 9C D8 E0 D1 9B BD 3E " +
            "88 54 38 2B FD 64 A4 27 82 50 A2 32 8C 68 3F 39 " +
            "73 AA E1 D1 48 97 47 FD 44 CE 09 68 74 29 D8 7F " +
            "E3 4D 53 65 85 6D 75 65 A7 CD BA 9A 5D 5C A5 8A " +
            "C8 1B 66 B1 7A 60 FD 05 D7 F9 D2 F2 55 56 03 F3 " +
            "37 9D 87 C0 BA 52 C9 16 08 33 3C 8D B6 47 BA 43 " +
            "FF 25 83 BD AC 69 9D F2 BB FB 80 6F 93 B0 54 85 " +
            "F4 02 3B 08 00 92 FA BD 02 23 19 01 BB 81 FA AC " +
            "D1 BE 23 59 27 8F 99 29 13 FA E6 87 3E 2E A0 03 " +
            "AD 3F DC 28 B2 97 26 32 51 99 92 13 19 12 F7 B2 " +
            "22 3F 4A C4 53 ED 09 FA 96 29 04 89 D2 53 85 8F " +
            "62 11 2D 68 4E 7D 81 DC 4B D3 A4 A6 0A 3F EE C9 " +
            "A5 CB 2F 38 A2 F7 15 D3",
            // After Chi
            "FA B9 EE 2A 1D DB 00 E7 7B 9C 5A F0 D1 93 A6 26 " +
            "F9 FE 79 EA BD F3 E4 E3 8A 01 AC 12 BD 40 3F 3B " +
            "72 AE F1 11 88 97 FA E5 40 4E A1 F2 2C 39 58 F5 " +
            "AB 5F 17 44 A7 4D 2D 60 B0 2D 2A D8 58 4A A7 78 " +
            "C8 1D 6F B9 5A 49 25 09 74 F8 80 F7 D4 12 26 F3 " +
            "C0 99 04 F0 B2 7A CC A6 08 E9 3C CF A5 D7 FA 46 " +
            "BB 25 B8 BD AC 6B 37 CA B8 66 04 AF 29 F0 55 87 " +
            "FC 20 03 05 04 97 C8 FC 00 63 DD 87 A3 A1 DA AE " +
            "7D BB 3B 71 A7 1E 9F 19 43 7A E4 94 37 2E 71 83 " +
            "AF 1D D5 28 10 16 2E 3E 80 05 B0 4B 1D 1C F6 B3 " +
            "42 2F 63 A4 5F C1 09 AA 9F EB 84 0F D2 51 EB 8E " +
            "C6 19 26 70 EE BD 90 CE 49 E7 E4 62 5B 37 E6 E1 " +
            "31 CB 2B 31 22 E5 91 D6",
            // After Iota  
            "F8 39 EE 2A 1D DB 00 67 7B 9C 5A F0 D1 93 A6 26 " +
            "F9 FE 79 EA BD F3 E4 E3 8A 01 AC 12 BD 40 3F 3B " +
            "72 AE F1 11 88 97 FA E5 40 4E A1 F2 2C 39 58 F5 " +
            "AB 5F 17 44 A7 4D 2D 60 B0 2D 2A D8 58 4A A7 78 " +
            "C8 1D 6F B9 5A 49 25 09 74 F8 80 F7 D4 12 26 F3 " +
            "C0 99 04 F0 B2 7A CC A6 08 E9 3C CF A5 D7 FA 46 " +
            "BB 25 B8 BD AC 6B 37 CA B8 66 04 AF 29 F0 55 87 " +
            "FC 20 03 05 04 97 C8 FC 00 63 DD 87 A3 A1 DA AE " +
            "7D BB 3B 71 A7 1E 9F 19 43 7A E4 94 37 2E 71 83 " +
            "AF 1D D5 28 10 16 2E 3E 80 05 B0 4B 1D 1C F6 B3 " +
            "42 2F 63 A4 5F C1 09 AA 9F EB 84 0F D2 51 EB 8E " +
            "C6 19 26 70 EE BD 90 CE 49 E7 E4 62 5B 37 E6 E1 " +
            "31 CB 2B 31 22 E5 91 D6",
            // Round #17
            // After Theta 
            "C6 75 9B B8 36 BD 79 C6 AF 14 FC 2D 8E E8 CB 2F " +
            "FB 84 5A 72 11 04 EE A1 6A E4 56 4A E2 D7 4D 39 " +
            "1A 6A EC 48 F3 BF F2 EF 7E 02 D4 60 07 5F 21 54 " +
            "7F D7 B1 99 F8 36 40 69 B2 57 09 40 F4 BD AD 3A " +
            "28 F8 95 E1 05 DE 57 0B 1C 3C 9D AE AF 3A 2E F9 " +
            "FE D5 71 62 99 1C B5 07 DC 61 9A 12 FA AC 97 4F " +
            "B9 5F 9B 25 00 9C 3D 88 58 83 FE F7 76 67 27 85 " +
            "94 E4 1E 5C 7F BF C0 F6 3E 2F A8 15 88 C7 A3 0F " +
            "A9 33 9D AC F8 65 F2 10 41 00 C7 0C 9B D9 7B C1 " +
            "4F F8 2F 70 4F 81 5C 3C E8 C1 AD 12 66 34 FE B9 " +
            "7C 63 16 36 74 A7 70 0B 4B 63 22 D2 8D 2A 86 87 " +
            "C4 63 05 E8 42 4A 9A 8C A9 02 1E 3A 04 A0 94 E3 " +
            "59 0F 36 68 59 CD 99 DC",
            // After Rho
            "C6 75 9B B8 36 BD 79 C6 5E 29 F8 5B 1C D1 97 5F " +
            "3E A1 96 5C 04 81 7B E8 7E DD 94 A3 46 6E A5 24 " +
            "FF 95 7F D7 50 63 47 9A 76 F0 15 42 E5 27 40 0D " +
            "9B 89 6F 03 94 F6 77 1D 8E EC 55 02 10 7D 6F AB " +
            "FC CA F0 02 EF AB 05 14 E3 92 CF C1 D3 E9 FA AA " +
            "F0 AF 8E 13 CB E4 A8 3D 3E 71 87 69 4A E8 B3 5E " +
            "2C 01 E0 EC 41 CC FD DA CE 4E 0A B1 06 FD EF ED " +
            "AE BF 5F 60 7B 4A 72 0F 2B 10 8F 47 1F 7C 5E 50 " +
            "93 15 BF 4C 1E 22 75 A6 BD E0 20 80 63 86 CD EC " +
            "90 8B E7 09 FF 05 EE 29 B9 E8 C1 AD 12 66 34 FE " +
            "C2 2D F0 8D 59 D8 D0 9D 2E 8D 89 48 37 AA 18 1E " +
            "78 AC 00 5D 48 49 93 91 02 1E 3A 04 A0 94 E3 A9 " +
            "26 77 D6 83 0D 5A 56 73",
            // After Pi 
            "C6 75 9B B8 36 BD 79 C6 9B 89 6F 03 94 F6 77 1D " +
            "2C 01 E0 EC 41 CC FD DA 90 8B E7 09 FF 05 EE 29 " +
            "26 77 D6 83 0D 5A 56 73 7E DD 94 A3 46 6E A5 24 " +
            "E3 92 CF C1 D3 E9 FA AA F0 AF 8E 13 CB E4 A8 3D " +
            "93 15 BF 4C 1E 22 75 A6 78 AC 00 5D 48 49 93 91 " +
            "5E 29 F8 5B 1C D1 97 5F 8E EC 55 02 10 7D 6F AB " +
            "CE 4E 0A B1 06 FD EF ED B9 E8 C1 AD 12 66 34 FE " +
            "C2 2D F0 8D 59 D8 D0 9D FF 95 7F D7 50 63 47 9A " +
            "76 F0 15 42 E5 27 40 0D 3E 71 87 69 4A E8 B3 5E " +
            "BD E0 20 80 63 86 CD EC 02 1E 3A 04 A0 94 E3 A9 " +
            "3E A1 96 5C 04 81 7B E8 FC CA F0 02 EF AB 05 14 " +
            "AE BF 5F 60 7B 4A 72 0F 2B 10 8F 47 1F 7C 5E 50 " +
            "2E 8D 89 48 37 AA 18 1E",
            // After Chi
            "E2 75 1B 54 77 B5 F1 04 0B 03 68 02 2A F7 75 3C " +
            "0A 75 F0 6E 41 96 ED 88 50 8B EE 31 CD A0 C7 AD " +
            "3F FF B2 80 8D 18 50 6A 6E F0 94 B1 4E 6A A5 31 " +
            "E0 82 FE 8D C7 EB AF 28 98 07 8E 02 8B AD 2A 2C " +
            "95 44 2B EE 18 04 51 82 F9 AE 4B 1D D9 C8 C9 1B " +
            "1E 2B F2 EA 1A 51 17 1B BF 4C 94 0E 00 7F 7F B9 " +
            "8C 4B 3A B1 4F 65 2F EC A5 E8 C9 FF 16 67 33 BC " +
            "42 E9 F5 8D 59 F4 B8 3D F7 94 FD FE 5A AB F4 C8 " +
            "F7 70 35 C2 C4 21 0C AD 3C 6F 9D 6D CA F8 91 5F " +
            "40 61 65 53 33 E5 C9 FE 02 7E 3A 04 05 90 E3 AC " +
            "3C 94 99 3C 14 C1 09 E3 FD CA 70 05 EB 9F 09 44 " +
            "AA 32 5F 68 5B C8 72 01 3B 30 99 53 1F 7D 3D B0 " +
            "EE C7 E9 4A DC 80 1C 0A",
            // After Iota  
            "62 75 1B 54 77 B5 F1 84 0B 03 68 02 2A F7 75 3C " +
            "0A 75 F0 6E 41 96 ED 88 50 8B EE 31 CD A0 C7 AD " +
            "3F FF B2 80 8D 18 50 6A 6E F0 94 B1 4E 6A A5 31 " +
            "E0 82 FE 8D C7 EB AF 28 98 07 8E 02 8B AD 2A 2C " +
            "95 44 2B EE 18 04 51 82 F9 AE 4B 1D D9 C8 C9 1B " +
            "1E 2B F2 EA 1A 51 17 1B BF 4C 94 0E 00 7F 7F B9 " +
            "8C 4B 3A B1 4F 65 2F EC A5 E8 C9 FF 16 67 33 BC " +
            "42 E9 F5 8D 59 F4 B8 3D F7 94 FD FE 5A AB F4 C8 " +
            "F7 70 35 C2 C4 21 0C AD 3C 6F 9D 6D CA F8 91 5F " +
            "40 61 65 53 33 E5 C9 FE 02 7E 3A 04 05 90 E3 AC " +
            "3C 94 99 3C 14 C1 09 E3 FD CA 70 05 EB 9F 09 44 " +
            "AA 32 5F 68 5B C8 72 01 3B 30 99 53 1F 7D 3D B0 " +
            "EE C7 E9 4A DC 80 1C 0A",
            // Round #18
            // After Theta
            "B6 9A 4A 86 27 3A 6E E7 C2 64 7D 7E 6E CF DD 95 " +
            "63 EE 57 69 5D FC EF 76 09 ED D6 54 71 A7 70 6E " +
            "97 D4 71 3A B9 8B 7C BC BA 1F C5 63 1E E5 3A 52 " +
            "29 E5 EB F1 83 D3 07 81 F1 9C 29 05 97 C7 28 D2 " +
            "CC 22 13 8B A4 03 E6 41 51 85 88 A7 ED 5B E5 CD " +
            "CA C4 A3 38 4A DE 88 78 76 2B 81 72 44 47 D7 10 " +
            "E5 D0 9D B6 53 0F 2D 12 FC 8E F1 9A AA 60 84 7F " +
            "EA C2 36 37 6D 67 94 EB 23 7B AC 2C 0A 24 6B AB " +
            "3E 17 20 BE 80 19 A4 04 55 F4 3A 6A D6 92 93 A1 " +
            "19 07 5D 36 8F E2 7E 3D AA 55 F9 BE 31 03 CF 7A " +
            "E8 7B C8 EE 44 4E 96 80 34 AD 65 79 AF A7 A1 ED " +
            "C3 A9 F8 6F 47 A2 70 FF 62 56 A1 36 A3 7A 8A 73 " +
            "46 EC 2A F0 E8 13 30 DC",
            // After Rho
            "B6 9A 4A 86 27 3A 6E E7 85 C9 FA FC DC 9E BB 2B " +
            "98 FB 55 5A 17 FF BB DD 77 0A E7 96 D0 6E 4D 15 " +
            "5D E4 E3 BD A4 8E D3 C9 E6 51 AE 23 A5 FB 51 3C " +
            "1E 3F 38 7D 10 98 52 BE 74 3C 67 4A C1 E5 31 8A " +
            "91 89 45 D2 01 F3 20 66 55 DE 1C 55 88 78 DA BE " +
            "53 26 1E C5 51 F2 46 C4 43 D8 AD 04 CA 11 1D 5D " +
            "B4 9D 7A 68 91 28 87 EE C1 08 FF F8 1D E3 35 55 " +
            "9B B6 33 CA 75 75 61 9B 59 14 48 D6 56 47 F6 58 " +
            "C4 17 30 83 94 C0 E7 02 C9 D0 2A 7A 1D 35 6B C9 " +
            "DC AF 27 E3 A0 CB E6 51 7A AA 55 F9 BE 31 03 CF " +
            "59 02 A2 EF 21 BB 13 39 D3 B4 96 E5 BD 9E 86 B6 " +
            "38 15 FF ED 48 14 EE 7F 56 A1 36 A3 7A 8A 73 62 " +
            "0C B7 11 BB 0A 3C FA 04",
            // After Pi 
            "B6 9A 4A 86 27 3A 6E E7 1E 3F 38 7D 10 98 52 BE " +
            "B4 9D 7A 68 91 28 87 EE DC AF 27 E3 A0 CB E6 51 " +
            "0C B7 11 BB 0A 3C FA 04 77 0A E7 96 D0 6E 4D 15 " +
            "55 DE 1C 55 88 78 DA BE 53 26 1E C5 51 F2 46 C4 " +
            "C4 17 30 83 94 C0 E7 02 38 15 FF ED 48 14 EE 7F " +
            "85 C9 FA FC DC 9E BB 2B 74 3C 67 4A C1 E5 31 8A " +
            "C1 08 FF F8 1D E3 35 55 7A AA 55 F9 BE 31 03 CF " +
            "59 02 A2 EF 21 BB 13 39 5D E4 E3 BD A4 8E D3 C9 " +
            "E6 51 AE 23 A5 FB 51 3C 43 D8 AD 04 CA 11 1D 5D " +
            "C9 D0 2A 7A 1D 35 6B C9 56 A1 36 A3 7A 8A 73 62 " +
            "98 FB 55 5A 17 FF BB DD 91 89 45 D2 01 F3 20 66 " +
            "9B B6 33 CA 75 75 61 9B 59 14 48 D6 56 47 F6 58 " +
            "D3 B4 96 E5 BD 9E 86 B6",
            // After Chi
            "16 1A 08 86 A6 1A EB A7 56 1D 3D FE 30 5B 32 AF " +
            "B4 8D 6A 70 9B 1C 9F EA 6E A7 6D E7 85 C9 E2 B2 " +
            "04 92 21 C2 1A BC EA 1C 75 2A E5 16 81 EC 49 55 " +
            "D1 CF 3C 57 0C 78 7B BC 6B 26 D1 A9 19 E6 4E B9 " +
            "83 1D 30 91 04 AA E6 02 38 C1 E7 AC 40 04 7C D5 " +
            "04 C9 62 4C C0 9C BF 7E 4E 9E 67 4B 63 F5 33 00 " +
            "C0 08 5D FE 1C 69 25 65 FE 63 0D E9 62 35 AB CD " +
            "29 36 A7 ED 20 DA 13 B9 5C 6C E2 B9 EE 8E DF 88 " +
            "6E 51 AC 59 B0 DF 33 BC 55 F9 B9 85 A8 9B 0D 7F " +
            "C0 94 EB 66 99 31 EB 40 F4 B0 3A A1 7B FB 73 56 " +
            "92 CD 67 52 63 FB FA 44 D1 89 0D C6 03 F1 B6 26 " +
            "19 16 A5 EB DC ED 61 3D 51 5F 09 CC 54 26 CF 11 " +
            "D2 B4 96 65 BD 9E 86 94",
            // After Iota  
            "1C 9A 08 86 A6 1A EB A7 56 1D 3D FE 30 5B 32 AF " +
            "B4 8D 6A 70 9B 1C 9F EA 6E A7 6D E7 85 C9 E2 B2 " +
            "04 92 21 C2 1A BC EA 1C 75 2A E5 16 81 EC 49 55 " +
            "D1 CF 3C 57 0C 78 7B BC 6B 26 D1 A9 19 E6 4E B9 " +
            "83 1D 30 91 04 AA E6 02 38 C1 E7 AC 40 04 7C D5 " +
            "04 C9 62 4C C0 9C BF 7E 4E 9E 67 4B 63 F5 33 00 " +
            "C0 08 5D FE 1C 69 25 65 FE 63 0D E9 62 35 AB CD " +
            "29 36 A7 ED 20 DA 13 B9 5C 6C E2 B9 EE 8E DF 88 " +
            "6E 51 AC 59 B0 DF 33 BC 55 F9 B9 85 A8 9B 0D 7F " +
            "C0 94 EB 66 99 31 EB 40 F4 B0 3A A1 7B FB 73 56 " +
            "92 CD 67 52 63 FB FA 44 D1 89 0D C6 03 F1 B6 26 " +
            "19 16 A5 EB DC ED 61 3D 51 5F 09 CC 54 26 CF 11 " +
            "D2 B4 96 65 BD 9E 86 94",
            // Round #19
            // After Theta
            "C2 D3 4A 3A C2 EC 64 06 53 5D C3 5A 8E 8F 3B 06 " +
            "C6 3C C9 66 2B 66 76 3A 5A 29 0D 21 17 23 9A A2 " +
            "C0 31 86 99 E0 C3 11 B0 AB 63 A7 AA E5 1A C6 F4 " +
            "D4 8F C2 F3 B2 AC 72 15 19 97 72 BF A9 9C A7 69 " +
            "B7 93 50 57 96 40 9E 12 FC 62 40 F7 BA 7B 87 79 " +
            "DA 80 20 F0 A4 6A 30 DF 4B DE 99 EF DD 21 3A A9 " +
            "B2 B9 FE E8 AC 13 CC B5 CA ED 6D 2F F0 DF D3 DD " +
            "ED 95 00 B6 DA A5 E8 15 82 25 A0 05 8A 78 50 29 " +
            "6B 11 52 FD 0E 0B 3A 15 27 48 1A 93 18 E1 E4 AF " +
            "F4 1A 8B A0 0B DB 93 50 30 13 9D FA 81 84 88 FA " +
            "4C 84 25 EE 07 0D 75 E5 D4 C9 F3 62 BD 25 BF 8F " +
            "6B A7 06 FD 6C 97 88 ED 65 D1 69 0A C6 CC B7 01 " +
            "16 17 31 3E 47 E1 7D 38",
            // After Rho
            "C2 D3 4A 3A C2 EC 64 06 A6 BA 86 B5 1C 1F 77 0C " +
            "31 4F B2 D9 8A 99 9D 8E 31 A2 29 AA 95 D2 10 72 " +
            "1F 8E 80 05 8E 31 CC 04 5A AE 61 4C BF 3A 76 AA " +
            "3C 2F CB 2A 57 41 FD 28 5A C6 A5 DC 6F 2A E7 69 " +
            "49 A8 2B 4B 20 4F 89 DB 77 98 C7 2F 06 74 AF BB " +
            "D6 06 04 81 27 55 83 F9 A4 2E 79 67 BE 77 87 E8 " +
            "47 67 9D 60 AE 95 CD F5 BF A7 BB 95 DB DB 5E E0 " +
            "5B ED 52 F4 8A F6 4A 00 0B 14 F1 A0 52 04 4B 40 " +
            "AA DF 61 41 A7 62 2D 42 F2 D7 13 24 8D 49 8C 70 " +
            "7B 12 8A 5E 63 11 74 61 FA 30 13 9D FA 81 84 88 " +
            "D4 95 33 11 96 B8 1F 34 52 27 CF 8B F5 96 FC 3E " +
            "ED D4 A0 9F ED 12 B1 7D D1 69 0A C6 CC B7 01 65 " +
            "1F 8E C5 45 8C CF 51 78",
            // After Pi 
            "C2 D3 4A 3A C2 EC 64 06 3C 2F CB 2A 57 41 FD 28 " +
            "47 67 9D 60 AE 95 CD F5 7B 12 8A 5E 63 11 74 61 " +
            "1F 8E C5 45 8C CF 51 78 31 A2 29 AA 95 D2 10 72 " +
            "77 98 C7 2F 06 74 AF BB D6 06 04 81 27 55 83 F9 " +
            "AA DF 61 41 A7 62 2D 42 ED D4 A0 9F ED 12 B1 7D " +
            "A6 BA 86 B5 1C 1F 77 0C 5A C6 A5 DC 6F 2A E7 69 " +
            "BF A7 BB 95 DB DB 5E E0 FA 30 13 9D FA 81 84 88 " +
            "D4 95 33 11 96 B8 1F 34 1F 8E 80 05 8E 31 CC 04 " +
            "5A AE 61 4C BF 3A 76 AA A4 2E 79 67 BE 77 87 E8 " +
            "F2 D7 13 24 8D 49 8C 70 D1 69 0A C6 CC B7 01 65 " +
            "31 4F B2 D9 8A 99 9D 8E 49 A8 2B 4B 20 4F 89 DB " +
            "5B ED 52 F4 8A F6 4A 00 0B 14 F1 A0 52 04 4B 40 " +
            "52 27 CF 8B F5 96 FC 3E",
            // After Chi
            "81 93 5E 7A 6A 78 64 D3 04 3F C9 34 16 41 CD 28 " +
            "43 EB D8 61 22 5B CC ED BB 43 80 64 21 31 50 67 " +
            "23 A2 44 45 99 CE C8 50 B1 A4 29 2A B4 D3 10 32 " +
            "5F 41 A6 6F 86 56 83 B9 93 06 84 1F 6F 45 13 C4 " +
            "BA FD 68 61 B7 A2 2D 40 AB CC 66 9A EF 36 1E F4 " +
            "03 9B 9C B4 8C CE 6F 8C 1A D6 A5 D4 4F 2A 67 61 " +
            "BB 22 9B 95 DF E3 45 D4 D8 1A 97 39 F2 86 E4 80 " +
            "8C D1 12 59 F5 98 9F 55 BB 8E 98 26 8E 74 4D 44 " +
            "08 7F 63 4C BE 32 7E BA A5 06 71 A5 FE C1 86 ED " +
            "FC 51 93 25 8F 49 40 70 91 49 6B 8E FD BD 33 CF " +
            "23 0A E2 6D 00 29 DF 8E 49 B8 8A 4B 70 4F 88 9B " +
            "0B CE 5C FF 2F 64 FE 3E 2A 5C C1 F0 58 0D 4A C0 " +
            "1A 87 C6 89 D5 D0 FC 6F",
            // After Iota  
            "8B 93 5E FA 6A 78 64 53 04 3F C9 34 16 41 CD 28 " +
            "43 EB D8 61 22 5B CC ED BB 43 80 64 21 31 50 67 " +
            "23 A2 44 45 99 CE C8 50 B1 A4 29 2A B4 D3 10 32 " +
            "5F 41 A6 6F 86 56 83 B9 93 06 84 1F 6F 45 13 C4 " +
            "BA FD 68 61 B7 A2 2D 40 AB CC 66 9A EF 36 1E F4 " +
            "03 9B 9C B4 8C CE 6F 8C 1A D6 A5 D4 4F 2A 67 61 " +
            "BB 22 9B 95 DF E3 45 D4 D8 1A 97 39 F2 86 E4 80 " +
            "8C D1 12 59 F5 98 9F 55 BB 8E 98 26 8E 74 4D 44 " +
            "08 7F 63 4C BE 32 7E BA A5 06 71 A5 FE C1 86 ED " +
            "FC 51 93 25 8F 49 40 70 91 49 6B 8E FD BD 33 CF " +
            "23 0A E2 6D 00 29 DF 8E 49 B8 8A 4B 70 4F 88 9B " +
            "0B CE 5C FF 2F 64 FE 3E 2A 5C C1 F0 58 0D 4A C0 " +
            "1A 87 C6 89 D5 D0 FC 6F",
            // Round #20
            // After Theta
            "05 3C 85 6B E2 F5 5C A1 2F 18 8C 78 4D C9 80 52 " +
            "5D D6 A0 3B 54 B8 35 13 60 A7 50 D6 35 72 BE EA " +
            "6E 5A 4B F3 92 EE 49 08 3F 0B F2 BB 3C 5E 28 C0 " +
            "74 66 E3 23 DD DE CE C3 8D 3B FC 45 19 A6 EA 3A " +
            "61 19 B8 D3 A3 E1 C3 CD E6 34 69 2C E4 16 9F AC " +
            "8D 34 47 25 04 43 57 7E 31 F1 E0 98 14 A2 2A 1B " +
            "A5 1F E3 CF A9 00 BC 2A 03 FE 47 8B E6 C5 0A 0D " +
            "C1 29 1D EF FE B8 1E 0D 35 21 43 B7 06 F9 75 B6 " +
            "23 58 26 00 E5 BA 33 C0 BB 3B 09 FF 88 22 7F 13 " +
            "27 B5 43 97 9B 0A AE FD DC B1 64 38 F6 9D B2 97 " +
            "AD A5 39 FC 88 A4 E7 7C 62 9F CF 07 2B C7 C5 E1 " +
            "15 F3 24 A5 59 87 07 C0 F1 B8 11 42 4C 4E A4 4D " +
            "57 7F C9 3F DE F0 7D 37",
            // After Rho
            "05 3C 85 6B E2 F5 5C A1 5E 30 18 F1 9A 92 01 A5 " +
            "97 35 E8 0E 15 6E CD 44 23 E7 AB 0E 76 0A 65 5D " +
            "74 4F 42 70 D3 5A 9A 97 CB E3 85 02 FC B3 20 BF " +
            "3E D2 ED ED 3C 4C 67 36 4E E3 0E 7F 51 86 A9 BA " +
            "0C DC E9 D1 F0 E1 E6 B0 F1 C9 6A 4E 93 C6 42 6E " +
            "6B A4 39 2A 21 18 BA F2 6C C4 C4 83 63 52 88 AA " +
            "7F 4E 05 E0 55 29 FD 18 8B 15 1A 06 FC 8F 16 CD " +
            "77 7F 5C 8F 86 E0 94 8E 6E 0D F2 EB 6C 6B 42 86 " +
            "04 A0 5C 77 06 78 04 CB BF 89 DD 9D 84 7F 44 91 " +
            "C1 B5 FF A4 76 E8 72 53 97 DC B1 64 38 F6 9D B2 " +
            "9E F3 B5 96 E6 F0 23 92 8B 7D 3E 1F AC 1C 17 87 " +
            "62 9E A4 34 EB F0 00 B8 B8 11 42 4C 4E A4 4D F1 " +
            "DF CD D5 5F F2 8F 37 7C",
            // After Pi 
            "05 3C 85 6B E2 F5 5C A1 3E D2 ED ED 3C 4C 67 36 " +
            "7F 4E 05 E0 55 29 FD 18 C1 B5 FF A4 76 E8 72 53 " +
            "DF CD D5 5F F2 8F 37 7C 23 E7 AB 0E 76 0A 65 5D " +
            "F1 C9 6A 4E 93 C6 42 6E 6B A4 39 2A 21 18 BA F2 " +
            "04 A0 5C 77 06 78 04 CB 62 9E A4 34 EB F0 00 B8 " +
            "5E 30 18 F1 9A 92 01 A5 4E E3 0E 7F 51 86 A9 BA " +
            "8B 15 1A 06 FC 8F 16 CD 97 DC B1 64 38 F6 9D B2 " +
            "9E F3 B5 96 E6 F0 23 92 74 4F 42 70 D3 5A 9A 97 " +
            "CB E3 85 02 FC B3 20 BF 6C C4 C4 83 63 52 88 AA " +
            "BF 89 DD 9D 84 7F 44 91 B8 11 42 4C 4E A4 4D F1 " +
            "97 35 E8 0E 15 6E CD 44 0C DC E9 D1 F0 E1 E6 B0 " +
            "77 7F 5C 8F 86 E0 94 8E 6E 0D F2 EB 6C 6B 42 86 " +
            "8B 7D 3E 1F AC 1C 17 87",
            // After Chi
            "44 30 85 6B A3 D4 C4 A9 BE 63 17 E9 1E 8C 65 75 " +
            "61 06 05 BB D5 2E F8 34 C1 85 FF 84 76 98 3A D2 " +
            "E5 0F BD DB EE 87 14 6A 29 C3 BA 2E 56 12 DD CD " +
            "F5 C9 2E 1B 95 A6 46 67 09 BA 99 2A C8 98 BA C2 " +
            "05 C1 57 7D 12 72 61 8E B2 96 E4 74 6A 34 02 9A " +
            "DF 24 08 F1 36 9B 17 E0 5A 2B AF 1F 51 F6 20 88 " +
            "83 36 1E 94 3A 8F 34 CD D7 DC B9 05 20 F4 9D 97 " +
            "9E 30 B3 98 A7 F4 8B 88 50 4B 02 F1 D0 1A 12 97 " +
            "58 EA 9C 1E 78 9E 64 AE 6C D4 C6 C3 29 D2 81 CA " +
            "FB C7 DD AD 15 25 D6 97 33 B1 C7 4E 62 05 6D D9 " +
            "E4 16 FC 00 13 6E DD 4A 04 DC 4B B1 98 EA A4 B0 " +
            "F6 0F 50 9B 06 F4 81 8F 7A 0D 32 EB 7D 09 8A C6 " +
            "83 B5 3F CE 4C 9D 35 37",
            // After Iota  
            "C5 B0 85 EB A3 D4 C4 29 BE 63 17 E9 1E 8C 65 75 " +
            "61 06 05 BB D5 2E F8 34 C1 85 FF 84 76 98 3A D2 " +
            "E5 0F BD DB EE 87 14 6A 29 C3 BA 2E 56 12 DD CD " +
            "F5 C9 2E 1B 95 A6 46 67 09 BA 99 2A C8 98 BA C2 " +
            "05 C1 57 7D 12 72 61 8E B2 96 E4 74 6A 34 02 9A " +
            "DF 24 08 F1 36 9B 17 E0 5A 2B AF 1F 51 F6 20 88 " +
            "83 36 1E 94 3A 8F 34 CD D7 DC B9 05 20 F4 9D 97 " +
            "9E 30 B3 98 A7 F4 8B 88 50 4B 02 F1 D0 1A 12 97 " +
            "58 EA 9C 1E 78 9E 64 AE 6C D4 C6 C3 29 D2 81 CA " +
            "FB C7 DD AD 15 25 D6 97 33 B1 C7 4E 62 05 6D D9 " +
            "E4 16 FC 00 13 6E DD 4A 04 DC 4B B1 98 EA A4 B0 " +
            "F6 0F 50 9B 06 F4 81 8F 7A 0D 32 EB 7D 09 8A C6 " +
            "83 B5 3F CE 4C 9D 35 37",
            // Round #21
            // After Theta
            "27 73 14 D8 DA 5B 86 B6 DB CB F6 96 0E 9B 48 50 " +
            "09 14 B8 8C B6 E2 0F 85 43 8E CE B7 65 39 C7 81 " +
            "78 48 D1 EA C3 E7 0C 43 CB 00 2B 1D 2F 9D 9F 52 " +
            "90 61 CF 64 85 B1 6B 42 61 A8 24 1D AB 54 4D 73 " +
            "87 CA 66 4E 01 D3 9C DD 2F D1 88 45 47 54 1A B3 " +
            "3D E7 99 C2 4F 14 55 7F 3F 83 4E 60 41 E1 0D AD " +
            "EB 24 A3 A3 59 43 C3 7C 55 D7 88 36 33 55 60 C4 " +
            "03 77 DF A9 8A 94 93 A1 B2 88 93 C2 A9 95 50 08 " +
            "3D 42 7D 61 68 89 49 8B 04 C6 7B F4 4A 1E 76 7B " +
            "79 CC EC 9E 06 84 2B C4 AE F6 AB 7F 4F 65 75 F0 " +
            "06 D5 6D 33 6A E1 9F D5 61 74 AA CE 88 FD 89 95 " +
            "9E 1D ED AC 65 38 76 3E F8 06 03 D8 6E A8 77 95 " +
            "1E F2 53 FF 61 FD 2D 1E",
            // After Rho
            "27 73 14 D8 DA 5B 86 B6 B6 97 ED 2D 1D 36 91 A0 " +
            "02 05 2E A3 AD F8 43 61 96 73 1C 38 E4 E8 7C 5B " +
            "3E 67 18 C2 43 8A 56 1F F1 D2 F9 29 B5 0C B0 D2 " +
            "4C 56 18 BB 26 04 19 F6 5C 18 2A 49 C7 2A 55 D3 " +
            "65 33 A7 80 69 CE EE 43 A5 31 FB 12 8D 58 74 44 " +
            "EB 39 CF 14 7E A2 A8 FA B4 FE 0C 3A 81 05 85 37 " +
            "1D CD 1A 1A E6 5B 27 19 AA C0 88 AB AE 11 6D 66 " +
            "54 45 CA C9 D0 81 BB EF 85 53 2B A1 10 64 11 27 " +
            "2F 0C 2D 31 69 B1 47 A8 BB 3D 02 E3 3D 7A 25 0F " +
            "70 85 38 8F 99 DD D3 80 F0 AE F6 AB 7F 4F 65 75 " +
            "7F 56 1B 54 B7 CD A8 85 86 D1 A9 3A 23 F6 27 56 " +
            "B3 A3 9D B5 0C C7 CE C7 06 03 D8 6E A8 77 95 F8 " +
            "8B 87 87 FC D4 7F 58 7F",
            // After Pi 
            "27 73 14 D8 DA 5B 86 B6 4C 56 18 BB 26 04 19 F6 " +
            "1D CD 1A 1A E6 5B 27 19 70 85 38 8F 99 DD D3 80 " +
            "8B 87 87 FC D4 7F 58 7F 96 73 1C 38 E4 E8 7C 5B " +
            "A5 31 FB 12 8D 58 74 44 EB 39 CF 14 7E A2 A8 FA " +
            "2F 0C 2D 31 69 B1 47 A8 B3 A3 9D B5 0C C7 CE C7 " +
            "B6 97 ED 2D 1D 36 91 A0 5C 18 2A 49 C7 2A 55 D3 " +
            "AA C0 88 AB AE 11 6D 66 F0 AE F6 AB 7F 4F 65 75 " +
            "7F 56 1B 54 B7 CD A8 85 3E 67 18 C2 43 8A 56 1F " +
            "F1 D2 F9 29 B5 0C B0 D2 B4 FE 0C 3A 81 05 85 37 " +
            "BB 3D 02 E3 3D 7A 25 0F 06 03 D8 6E A8 77 95 F8 " +
            "02 05 2E A3 AD F8 43 61 65 33 A7 80 69 CE EE 43 " +
            "54 45 CA C9 D0 81 BB EF 85 53 2B A1 10 64 11 27 " +
            "86 D1 A9 3A 23 F6 27 56",
            // After Chi
            "36 FA 16 D8 1A 00 A0 BF 2C 56 38 3E 3F 80 C9 76 " +
            "96 CF 9D 6A A2 79 2F 66 54 F5 28 8F 93 DD 55 00 " +
            "C3 83 8F DF F0 7B 41 3F DC 7B 18 3C 96 4A F4 E1 " +
            "A1 35 DB 33 8C 49 33 44 7B 9A 5F 90 7A E4 20 BD " +
            "2B 5C 2D 39 89 99 77 B0 92 A3 7E B7 05 D7 CE C3 " +
            "14 57 6D 8F 35 27 B9 84 0C 36 5C 49 96 64 55 C2 " +
            "A5 90 81 FF 2E 91 E5 E6 70 2F 12 82 77 7D 74 55 " +
            "37 5E 19 14 75 C5 EC D6 3A 4B 1C D0 43 8B 53 3A " +
            "FA D3 FB E8 89 76 90 DA B0 FC D4 36 01 00 15 C7 " +
            "83 59 02 63 7E F2 67 08 C7 93 39 47 1C 73 35 38 " +
            "12 41 66 EA 3D F9 52 CD E4 21 86 A0 69 AA EE 43 " +
            "56 C5 4A D3 F3 13 9D BF 85 57 2D 20 9C 6C 51 06 " +
            "E3 E3 28 3A 63 F0 8B 54",
            // After Iota  
            "B6 7A 16 D8 1A 00 A0 3F 2C 56 38 3E 3F 80 C9 76 " +
            "96 CF 9D 6A A2 79 2F 66 54 F5 28 8F 93 DD 55 00 " +
            "C3 83 8F DF F0 7B 41 3F DC 7B 18 3C 96 4A F4 E1 " +
            "A1 35 DB 33 8C 49 33 44 7B 9A 5F 90 7A E4 20 BD " +
            "2B 5C 2D 39 89 99 77 B0 92 A3 7E B7 05 D7 CE C3 " +
            "14 57 6D 8F 35 27 B9 84 0C 36 5C 49 96 64 55 C2 " +
            "A5 90 81 FF 2E 91 E5 E6 70 2F 12 82 77 7D 74 55 " +
            "37 5E 19 14 75 C5 EC D6 3A 4B 1C D0 43 8B 53 3A " +
            "FA D3 FB E8 89 76 90 DA B0 FC D4 36 01 00 15 C7 " +
            "83 59 02 63 7E F2 67 08 C7 93 39 47 1C 73 35 38 " +
            "12 41 66 EA 3D F9 52 CD E4 21 86 A0 69 AA EE 43 " +
            "56 C5 4A D3 F3 13 9D BF 85 57 2D 20 9C 6C 51 06 " +
            "E3 E3 28 3A 63 F0 8B 54",
            // Round #22
            // After Theta
            "CA 3B 6A C0 6F 09 DF AA 26 F3 9A AE F1 A1 E1 51 " +
            "1A 78 2E 88 79 47 3F D9 7E 15 07 6C 69 17 8C C8 " +
            "67 B3 85 0A F1 E3 F9 8F A0 3A 64 24 E3 43 8B 74 " +
            "AB 90 79 A3 42 68 1B 63 F7 2D EC 72 A1 DA 30 02 " +
            "01 BC 02 DA 73 53 AE 78 36 93 74 62 04 4F 76 73 " +
            "68 16 11 97 40 2E C6 11 06 93 FE D9 58 45 7D E5 " +
            "29 27 32 1D F5 AF F5 59 5A CF 3D 61 8D B7 AD 9D " +
            "93 6E 13 C1 74 5D 54 66 46 0A 60 C8 36 82 2C AF " +
            "F0 76 59 78 47 57 B8 FD 3C 4B 67 D4 DA 3E 05 78 " +
            "A9 B9 2D 80 84 38 BE C0 63 A3 33 92 1D EB 8D 88 " +
            "6E 00 1A F2 48 F0 2D 58 EE 84 24 30 A7 8B C6 64 " +
            "DA 72 F9 31 28 2D 8D 00 AF B7 02 C3 66 A6 88 CE " +
            "47 D3 22 EF 62 68 33 E4",
            // After Rho
            "CA 3B 6A C0 6F 09 DF AA 4C E6 35 5D E3 43 C3 A3 " +
            "06 9E 0B 62 DE D1 4F B6 76 C1 88 EC 57 71 C0 96 " +
            "1F CF 7F 3C 9B 2D 54 88 32 3E B4 48 07 AA 43 46 " +
            "37 2A 84 B6 31 B6 0A 99 C0 7D 0B BB 5C A8 36 8C " +
            "5E 01 ED B9 29 57 BC 00 64 37 67 33 49 27 46 F0 " +
            "40 B3 88 B8 04 72 31 8E 95 1B 4C FA 67 63 15 F5 " +
            "E9 A8 7F AD CF 4A 39 91 6F 5B 3B B5 9E 7B C2 1A " +
            "60 BA 2E 2A B3 49 B7 89 90 6D 04 59 5E 8D 14 C0 " +
            "0B EF E8 0A B7 1F DE 2E 02 3C 9E A5 33 6A 6D 9F " +
            "C7 17 38 35 B7 05 90 10 88 63 A3 33 92 1D EB 8D " +
            "B7 60 B9 01 68 C8 23 C1 B9 13 92 C0 9C 2E 1A 93 " +
            "5B 2E 3F 06 A5 A5 11 40 B7 02 C3 66 A6 88 CE AF " +
            "0C F9 D1 B4 C8 BB 18 DA",
            // After Pi 
            "CA 3B 6A C0 6F 09 DF AA 37 2A 84 B6 31 B6 0A 99 " +
            "E9 A8 7F AD CF 4A 39 91 C7 17 38 35 B7 05 90 10 " +
            "0C F9 D1 B4 C8 BB 18 DA 76 C1 88 EC 57 71 C0 96 " +
            "64 37 67 33 49 27 46 F0 40 B3 88 B8 04 72 31 8E " +
            "0B EF E8 0A B7 1F DE 2E 5B 2E 3F 06 A5 A5 11 40 " +
            "4C E6 35 5D E3 43 C3 A3 C0 7D 0B BB 5C A8 36 8C " +
            "6F 5B 3B B5 9E 7B C2 1A 88 63 A3 33 92 1D EB 8D " +
            "B7 60 B9 01 68 C8 23 C1 1F CF 7F 3C 9B 2D 54 88 " +
            "32 3E B4 48 07 AA 43 46 95 1B 4C FA 67 63 15 F5 " +
            "02 3C 9E A5 33 6A 6D 9F B7 02 C3 66 A6 88 CE AF " +
            "06 9E 0B 62 DE D1 4F B6 5E 01 ED B9 29 57 BC 00 " +
            "60 BA 2E 2A B3 49 B7 89 90 6D 04 59 5E 8D 14 C0 " +
            "B9 13 92 C0 9C 2E 1A 93",
            // After Chi
            "02 BB 11 C9 A1 41 EE AA 31 3D 84 A6 01 B3 8A 99 " +
            "E1 40 BE 2D 87 F0 31 5B 05 15 12 75 90 05 57 30 " +
            "39 F9 55 82 D8 0D 18 CB 76 41 00 64 53 21 F1 98 " +
            "6F 7B 07 31 FA 2A 88 D0 10 B3 9F BC 04 D2 30 CE " +
            "2F 2E 68 E2 E5 4F 1E B8 5B 18 58 15 AD A3 17 20 " +
            "63 E4 05 59 61 10 03 B1 40 5D 8B B9 5C AC 1F 09 " +
            "58 5B 23 B5 F6 BB C2 5A C0 E5 A7 6F 11 1E 2B AF " +
            "37 79 B3 A3 74 60 17 CD 9A CE 37 8E FB 6C 40 39 " +
            "30 1A 26 4D 17 A2 2B 4C 20 19 0D B8 E3 E3 97 D5 " +
            "0A F1 A2 BD 2A 4F 7D 9F 97 32 43 26 A2 0A CD E9 " +
            "26 24 09 60 4C D9 4C 3F CE 44 ED E8 65 D3 BC 40 " +
            "49 A8 BC AA 33 6B BD 9A 96 E1 0D 7B 1C 5C 51 E4 " +
            "E1 12 76 59 BD 28 AA 93",
            // After Iota  
            "03 BB 11 49 A1 41 EE AA 31 3D 84 A6 01 B3 8A 99 " +
            "E1 40 BE 2D 87 F0 31 5B 05 15 12 75 90 05 57 30 " +
            "39 F9 55 82 D8 0D 18 CB 76 41 00 64 53 21 F1 98 " +
            "6F 7B 07 31 FA 2A 88 D0 10 B3 9F BC 04 D2 30 CE " +
            "2F 2E 68 E2 E5 4F 1E B8 5B 18 58 15 AD A3 17 20 " +
            "63 E4 05 59 61 10 03 B1 40 5D 8B B9 5C AC 1F 09 " +
            "58 5B 23 B5 F6 BB C2 5A C0 E5 A7 6F 11 1E 2B AF " +
            "37 79 B3 A3 74 60 17 CD 9A CE 37 8E FB 6C 40 39 " +
            "30 1A 26 4D 17 A2 2B 4C 20 19 0D B8 E3 E3 97 D5 " +
            "0A F1 A2 BD 2A 4F 7D 9F 97 32 43 26 A2 0A CD E9 " +
            "26 24 09 60 4C D9 4C 3F CE 44 ED E8 65 D3 BC 40 " +
            "49 A8 BC AA 33 6B BD 9A 96 E1 0D 7B 1C 5C 51 E4 " +
            "E1 12 76 59 BD 28 AA 93",
            // Round #23
            // After Theta
            "E0 88 1C 15 14 24 85 6F 1A FA C8 51 6F 55 48 1D " +
            "ED 99 98 DA F6 3A 27 AF 83 7C B6 D4 09 CC 41 08 " +
            "1A DE 72 88 C3 C0 77 9D 95 72 0D 38 E6 44 9A 5D " +
            "44 BC 4B C6 94 CC 4A 54 1C 6A B9 4B 75 18 26 3A " +
            "A9 47 CC 43 7C 86 08 80 78 3F 7F 1F B6 6E 78 76 " +
            "80 D7 08 05 D4 75 68 74 6B 9A C7 4E 32 4A DD 8D " +
            "54 82 05 42 87 71 D4 AE 46 8C 03 CE 88 D7 3D 97 " +
            "14 5E 94 A9 6F AD 78 9B 79 FD 3A D2 4E 09 2B FC " +
            "1B DD 6A BA 79 44 E9 C8 2C C0 2B 4F 92 29 81 21 " +
            "8C 98 06 1C B3 86 6B A7 B4 15 64 2C B9 C7 A2 BF " +
            "C5 17 04 3C F9 BC 27 FA E5 83 A1 1F 0B 35 7E C4 " +
            "45 71 9A 5D 42 A1 AB 6E 10 88 A9 DA 85 95 47 DC " +
            "C2 35 51 53 A6 E5 C5 C5",
            // After Rho
            "E0 88 1C 15 14 24 85 6F 34 F4 91 A3 DE AA 90 3A " +
            "7B 26 A6 B6 BD CE C9 6B C0 1C 84 30 C8 67 4B 9D " +
            "06 BE EB D4 F0 96 43 1C 63 4E A4 D9 55 29 D7 80 " +
            "64 4C C9 AC 44 45 C4 BB 0E 87 5A EE 52 1D 86 89 " +
            "23 E6 21 3E 43 04 C0 D4 86 67 87 F7 F3 F7 61 EB " +
            "03 BC 46 28 A0 AE 43 A3 37 AE 69 1E 3B C9 28 75 " +
            "10 3A 8C A3 76 A5 12 2C AF 7B 2E 8D 18 07 9C 11 " +
            "D4 B7 56 BC 4D 0A 2F CA A4 9D 12 56 F8 F3 FA 75 " +
            "4D 37 8F 28 1D 79 A3 5B C0 10 16 E0 95 27 C9 94 " +
            "70 ED 94 11 D3 80 63 D6 BF B4 15 64 2C B9 C7 A2 " +
            "9E E8 17 5F 10 F0 E4 F3 97 0F 86 7E 2C D4 F8 11 " +
            "28 4E B3 4B 28 74 D5 AD 88 A9 DA 85 95 47 DC 10 " +
            "71 B1 70 4D D4 94 69 79",
            // After Pi 
            "E0 88 1C 15 14 24 85 6F 64 4C C9 AC 44 45 C4 BB " +
            "10 3A 8C A3 76 A5 12 2C 70 ED 94 11 D3 80 63 D6 " +
            "71 B1 70 4D D4 94 69 79 C0 1C 84 30 C8 67 4B 9D " +
            "86 67 87 F7 F3 F7 61 EB 03 BC 46 28 A0 AE 43 A3 " +
            "4D 37 8F 28 1D 79 A3 5B 28 4E B3 4B 28 74 D5 AD " +
            "34 F4 91 A3 DE AA 90 3A 0E 87 5A EE 52 1D 86 89 " +
            "AF 7B 2E 8D 18 07 9C 11 BF B4 15 64 2C B9 C7 A2 " +
            "9E E8 17 5F 10 F0 E4 F3 06 BE EB D4 F0 96 43 1C " +
            "63 4E A4 D9 55 29 D7 80 37 AE 69 1E 3B C9 28 75 " +
            "C0 10 16 E0 95 27 C9 94 88 A9 DA 85 95 47 DC 10 " +
            "7B 26 A6 B6 BD CE C9 6B 23 E6 21 3E 43 04 C0 D4 " +
            "D4 B7 56 BC 4D 0A 2F CA A4 9D 12 56 F8 F3 FA 75 " +
            "97 0F 86 7E 2C D4 F8 11",
            // After Chi
            "F0 BA 18 16 26 84 97 6B 04 89 D9 BC C5 45 A5 69 " +
            "11 2A EC EF 72 B1 1A 05 F0 E5 98 01 D3 A0 E7 D0 " +
            "75 F5 B1 E5 94 D5 29 E9 C1 84 C4 38 C8 6F 49 9D " +
            "CA 64 0E F7 EE A6 C1 B3 23 F4 76 6B 80 AA 17 07 " +
            "8D 27 8B 18 DD 7A A9 4B 2E 2D B0 8C 1B E4 F5 CF " +
            "95 8C B5 A2 D6 A8 88 2A 1E 03 4B 8E 76 A5 C5 2B " +
            "AF 33 2C 96 08 47 BC 40 9F A0 95 C4 E2 B3 D7 AA " +
            "94 EB 5D 13 10 E5 E2 72 12 1E A2 D2 DA 56 6B 69 " +
            "A3 5E B2 39 D1 0F 16 00 3F 07 A1 1B 3B 89 3C 75 " +
            "C6 06 37 B0 F5 B7 CA 98 E9 E9 DE 8C 90 6E 48 90 " +
            "AF 37 F0 36 B1 C4 E6 61 03 EE 21 7C F3 F5 10 E1 " +
            "C7 B5 D2 94 49 0E 2F CA CC BD 32 D6 69 F9 FB 1F " +
            "97 CF 87 76 6E D4 F8 85",
            // After Iota  
            "F8 3A 18 96 26 84 97 EB 04 89 D9 BC C5 45 A5 69 " +
            "11 2A EC EF 72 B1 1A 05 F0 E5 98 01 D3 A0 E7 D0 " +
            "75 F5 B1 E5 94 D5 29 E9 C1 84 C4 38 C8 6F 49 9D " +
            "CA 64 0E F7 EE A6 C1 B3 23 F4 76 6B 80 AA 17 07 " +
            "8D 27 8B 18 DD 7A A9 4B 2E 2D B0 8C 1B E4 F5 CF " +
            "95 8C B5 A2 D6 A8 88 2A 1E 03 4B 8E 76 A5 C5 2B " +
            "AF 33 2C 96 08 47 BC 40 9F A0 95 C4 E2 B3 D7 AA " +
            "94 EB 5D 13 10 E5 E2 72 12 1E A2 D2 DA 56 6B 69 " +
            "A3 5E B2 39 D1 0F 16 00 3F 07 A1 1B 3B 89 3C 75 " +
            "C6 06 37 B0 F5 B7 CA 98 E9 E9 DE 8C 90 6E 48 90 " +
            "AF 37 F0 36 B1 C4 E6 61 03 EE 21 7C F3 F5 10 E1 " +
            "C7 B5 D2 94 49 0E 2F CA CC BD 32 D6 69 F9 FB 1F " +
            "97 CF 87 76 6E D4 F8 85",
            //"Xor'd state (in bytes) ",
            "5B 99 BB 35 85 27 34 48 A7 2A 7A 1F 66 E6 06 CA " +
            "B2 89 4F 4C D1 12 B9 A6 53 46 3B A2 70 03 44 73 " +
            "D6 56 12 46 37 76 8A 4A 62 27 67 9B 6B CC EA 3E " +
            "69 C7 AD 54 4D 05 62 10 25 F4 76 6B 80 AA 17 07 " +
            "8D 27 8B 18 DD 7A A9 4B 2E 2D B0 8C 1B E4 F5 CF " +
            "95 8C B5 A2 D6 A8 88 2A 1E 03 4B 8E 76 A5 C5 2B " +
            "AF 33 2C 96 08 47 BC 40 9F A0 95 C4 E2 B3 D7 AA " +
            "94 EB 5D 13 10 E5 E2 72 12 1E A2 D2 DA 56 6B 69 " +
            "A3 5E B2 39 D1 0F 16 00 3F 07 A1 1B 3B 89 3C F5 " +
            "C6 06 37 B0 F5 B7 CA 98 E9 E9 DE 8C 90 6E 48 90 " +
            "AF 37 F0 36 B1 C4 E6 61 03 EE 21 7C F3 F5 10 E1 " +
            "C7 B5 D2 94 49 0E 2F CA CC BD 32 D6 69 F9 FB 1F " +
            "97 CF 87 76 6E D4 F8 85",
            // Round #0
            // After Theta
            "A9 93 03 16 B8 92 56 8B 37 C8 8C 8B 63 C7 DF 22 " +
            "54 23 00 FC 08 A7 09 9C B6 D6 10 DB DF E0 1E 69 " +
            "BF 1A 44 8E 43 50 36 F6 90 2D DF B8 56 79 88 FD " +
            "F9 25 5B C0 48 24 BB F8 C3 5E 39 DB 59 1F A7 3D " +
            "68 B7 A0 61 72 99 F3 51 47 61 E6 44 6F C2 49 73 " +
            "67 86 0D 81 EB 1D EA E9 8E E1 BD 1A 73 84 1C C3 " +
            "49 99 63 26 D1 F2 0C 7A 7A 30 BE BD 4D 50 8D B0 " +
            "FD A7 0B DB 64 C3 5E CE E0 14 1A F1 E7 E3 09 AA " +
            "33 BC 44 AD D4 2E CF E8 D9 AD EE AB E2 3C 8C CF " +
            "23 96 1C C9 5A 54 90 82 80 A5 88 44 E4 48 F4 2C " +
            "5D 3D 48 15 8C 71 84 A2 93 0C D7 E8 F6 D4 C9 09 " +
            "21 1F 9D 24 90 BB 9F F0 29 2D 19 AF C6 1A A1 05 " +
            "FE 83 D1 BE 1A F2 44 39",
            // After Rho
            "A9 93 03 16 B8 92 56 8B 6E 90 19 17 C7 8E BF 45 " +
            "D5 08 00 3F C2 69 02 27 0D EE 91 66 6B 0D B1 FD " +
            "82 B2 B1 FF D5 20 72 1C 6B 95 87 D8 0F D9 F2 8D " +
            "05 8C 44 B2 8B 9F 5F B2 CF B0 57 CE 76 D6 C7 69 " +
            "5B D0 30 B9 CC F9 28 B4 9C 34 77 14 66 4E F4 26 " +
            "3F 33 6C 08 5C EF 50 4F 0C 3B 86 F7 6A CC 11 72 " +
            "33 89 96 67 D0 4B CA 1C A0 1A 61 F5 60 7C 7B 9B " +
            "6D B2 61 2F E7 FE D3 85 E2 CF C7 13 54 C1 29 34 " +
            "A8 95 DA E5 19 7D 86 97 C6 E7 EC 56 F7 55 71 1E " +
            "0A 52 70 C4 92 23 59 8B 2C 80 A5 88 44 E4 48 F4 " +
            "11 8A 76 F5 20 55 30 C6 4C 32 5C A3 DB 53 27 27 " +
            "E4 A3 93 04 72 F7 13 3E 2D 19 AF C6 1A A1 05 29 " +
            "51 8E FF 60 B4 AF 86 3C",
            // After Pi 
            "A9 93 03 16 B8 92 56 8B 05 8C 44 B2 8B 9F 5F B2 " +
            "33 89 96 67 D0 4B CA 1C 0A 52 70 C4 92 23 59 8B " +
            "51 8E FF 60 B4 AF 86 3C 0D EE 91 66 6B 0D B1 FD " +
            "9C 34 77 14 66 4E F4 26 3F 33 6C 08 5C EF 50 4F " +
            "A8 95 DA E5 19 7D 86 97 E4 A3 93 04 72 F7 13 3E " +
            "6E 90 19 17 C7 8E BF 45 CF B0 57 CE 76 D6 C7 69 " +
            "A0 1A 61 F5 60 7C 7B 9B 2C 80 A5 88 44 E4 48 F4 " +
            "11 8A 76 F5 20 55 30 C6 82 B2 B1 FF D5 20 72 1C " +
            "6B 95 87 D8 0F D9 F2 8D 0C 3B 86 F7 6A CC 11 72 " +
            "C6 E7 EC 56 F7 55 71 1E 2D 19 AF C6 1A A1 05 29 " +
            "D5 08 00 3F C2 69 02 27 5B D0 30 B9 CC F9 28 B4 " +
            "6D B2 61 2F E7 FE D3 85 E2 CF C7 13 54 C1 29 34 " +
            "4C 32 5C A3 DB 53 27 27",
            // After Chi
            "9B 92 91 53 E8 D2 D6 87 0D DE 24 32 89 BF 4E 31 " +
            "62 05 19 47 F4 C7 4C 28 A2 43 70 D2 9A 33 09 08 " +
            "55 82 BB C0 B7 A2 8F 0C 2E ED 99 6E 73 AC B1 B4 " +
            "1C B0 E5 F1 67 5E 72 B6 7B 11 6D 08 3E 6D 41 67 " +
            "A1 D9 DA 87 10 75 26 56 74 B3 F5 14 76 B5 57 3C " +
            "4E 9A 39 26 C7 A6 87 D7 C3 30 D3 C6 72 56 C7 0D " +
            "B1 10 33 80 40 6D 4B 99 42 90 AC 8A 83 6E C7 F5 " +
            "90 AA 30 3D 10 05 70 EE 86 98 B1 D8 B5 24 73 6E " +
            "A9 51 EF D8 9A C8 92 81 25 23 85 77 62 6C 15 53 " +
            "44 45 FC 6F 32 55 03 0A 44 1C A9 C6 10 78 85 A8 " +
            "F1 2A 41 39 E1 6F D1 26 D9 9D B6 A9 DC F8 00 84 " +
            "61 82 79 8F 6C EC D5 86 73 C7 C7 0F 54 E9 29 34 " +
            "46 E2 6C 23 D7 C3 0F B7",
            // After Iota  
            "9A 92 91 53 E8 D2 D6 87 0D DE 24 32 89 BF 4E 31 " +
            "62 05 19 47 F4 C7 4C 28 A2 43 70 D2 9A 33 09 08 " +
            "55 82 BB C0 B7 A2 8F 0C 2E ED 99 6E 73 AC B1 B4 " +
            "1C B0 E5 F1 67 5E 72 B6 7B 11 6D 08 3E 6D 41 67 " +
            "A1 D9 DA 87 10 75 26 56 74 B3 F5 14 76 B5 57 3C " +
            "4E 9A 39 26 C7 A6 87 D7 C3 30 D3 C6 72 56 C7 0D " +
            "B1 10 33 80 40 6D 4B 99 42 90 AC 8A 83 6E C7 F5 " +
            "90 AA 30 3D 10 05 70 EE 86 98 B1 D8 B5 24 73 6E " +
            "A9 51 EF D8 9A C8 92 81 25 23 85 77 62 6C 15 53 " +
            "44 45 FC 6F 32 55 03 0A 44 1C A9 C6 10 78 85 A8 " +
            "F1 2A 41 39 E1 6F D1 26 D9 9D B6 A9 DC F8 00 84 " +
            "61 82 79 8F 6C EC D5 86 73 C7 C7 0F 54 E9 29 34 " +
            "46 E2 6C 23 D7 C3 0F B7",
            // Round #1 
            // After Theta
            "6C D2 BD B7 4A 74 27 58 58 C2 92 A7 89 A3 00 9A " +
            "2D 87 29 4D F1 68 A0 8C 29 2D BD FC 32 26 CA 89 " +
            "38 A5 04 8A C9 10 C8 C1 D8 AD B5 8A D1 0A 40 6B " +
            "49 AC 53 64 67 42 3C 1D 34 93 5D 02 3B C2 AD C3 " +
            "2A B7 17 A9 B8 60 E5 D7 19 94 4A 5E 08 07 10 F1 " +
            "B8 DA 15 C2 65 00 76 08 96 2C 65 53 72 4A 89 A6 " +
            "FE 92 03 8A 45 C2 A7 3D C9 FE 61 A4 2B 7B 04 74 " +
            "FD 8D 8F 77 6E B7 37 23 70 D8 9D 3C 17 82 82 B1 " +
            "FC 4D 59 4D 9A D4 DC 2A 6A A1 B5 7D 67 C3 F9 F7 " +
            "CF 2B 31 41 9A 40 C0 8B 29 3B 16 8C 6E CA C2 65 " +
            "07 6A 6D DD 43 C9 20 F9 8C 81 00 3C DC E4 4E 2F " +
            "2E 00 49 85 69 43 39 22 F8 A9 0A 21 FC FC EA B5 " +
            "2B C5 D3 69 A9 71 48 7A",
            // After Rho
            "6C D2 BD B7 4A 74 27 58 B1 84 25 4F 13 47 01 34 " +
            "CB 61 4A 53 3C 1A 28 63 63 A2 9C 98 D2 D2 CB 2F " +
            "86 40 0E C6 29 25 50 4C 18 AD 00 B4 86 DD 5A AB " +
            "45 76 26 C4 D3 91 C4 3A 30 CD 64 97 C0 8E 70 EB " +
            "DB 8B 54 5C B0 F2 6B 95 00 11 9F 41 A9 E4 85 70 " +
            "C0 D5 AE 10 2E 03 B0 43 9A 5A B2 94 4D C9 29 25 " +
            "50 2C 12 3E ED F1 97 1C F6 08 E8 92 FD C3 48 57 " +
            "3B B7 DB 9B 91 FE C6 C7 79 2E 04 05 63 E1 B0 3B " +
            "AB 49 93 9A 5B 85 BF 29 FC 7B B5 D0 DA BE B3 E1 " +
            "08 78 F1 79 25 26 48 13 65 29 3B 16 8C 6E CA C2 " +
            "83 E4 1F A8 B5 75 0F 25 30 06 02 F0 70 93 3B BD " +
            "05 20 A9 30 6D 28 47 C4 A9 0A 21 FC FC EA B5 F8 " +
            "92 DE 4A F1 74 5A 6A 1C",
            // After Pi 
            "6C D2 BD B7 4A 74 27 58 45 76 26 C4 D3 91 C4 3A " +
            "50 2C 12 3E ED F1 97 1C 08 78 F1 79 25 26 48 13 " +
            "92 DE 4A F1 74 5A 6A 1C 63 A2 9C 98 D2 D2 CB 2F " +
            "00 11 9F 41 A9 E4 85 70 C0 D5 AE 10 2E 03 B0 43 " +
            "AB 49 93 9A 5B 85 BF 29 05 20 A9 30 6D 28 47 C4 " +
            "B1 84 25 4F 13 47 01 34 30 CD 64 97 C0 8E 70 EB " +
            "F6 08 E8 92 FD C3 48 57 65 29 3B 16 8C 6E CA C2 " +
            "83 E4 1F A8 B5 75 0F 25 86 40 0E C6 29 25 50 4C " +
            "18 AD 00 B4 86 DD 5A AB 9A 5A B2 94 4D C9 29 25 " +
            "FC 7B B5 D0 DA BE B3 E1 A9 0A 21 FC FC EA B5 F8 " +
            "CB 61 4A 53 3C 1A 28 63 DB 8B 54 5C B0 F2 6B 95 " +
            "3B B7 DB 9B 91 FE C6 C7 79 2E 04 05 63 E1 B0 3B " +
            "30 06 02 F0 70 93 3B BD",
            // After Chi
            "7C DA AD 8D 66 14 34 5C 4D 26 C7 85 D3 97 8C 39 " +
            "C2 AA 18 BE BD A9 B5 10 64 78 44 7F 2F 02 4D 53 " +
            "93 FA 48 B1 E5 DB AA 3E A3 66 BC 88 D4 D1 FB 2C " +
            "2B 19 8E CB F8 60 8A 58 C4 F5 86 30 0A 2B F0 87 " +
            "C9 CB 87 12 C9 57 37 02 05 31 AA 71 44 0C 43 94 " +
            "77 84 AD 4F 2E 06 09 20 31 EC 77 93 C0 A2 F2 6B " +
            "74 CC EC 3A CC D2 4D 72 55 29 1B 51 8E 6C CA D2 " +
            "83 AD 5F 38 75 FD 7F EE 04 12 BC C6 60 25 71 48 " +
            "7C 8C 05 F4 14 EB C8 6B 9B 5A B2 B8 69 89 2D 3D " +
            "FA 3B BB D2 DB BB F3 E5 B1 A7 21 CC 7A 32 BF 5B " +
            "EB 55 C1 D0 3D 16 AC 21 9B 83 50 58 D2 F3 5B AD " +
            "3B B7 D9 6B 81 EC CD 43 B2 4F 4C 06 6F E9 B0 79 " +
            "20 8C 16 FC F0 73 78 29",
            // After Iota  
            "FE 5A AD 8D 66 14 34 5C 4D 26 C7 85 D3 97 8C 39 " +
            "C2 AA 18 BE BD A9 B5 10 64 78 44 7F 2F 02 4D 53 " +
            "93 FA 48 B1 E5 DB AA 3E A3 66 BC 88 D4 D1 FB 2C " +
            "2B 19 8E CB F8 60 8A 58 C4 F5 86 30 0A 2B F0 87 " +
            "C9 CB 87 12 C9 57 37 02 05 31 AA 71 44 0C 43 94 " +
            "77 84 AD 4F 2E 06 09 20 31 EC 77 93 C0 A2 F2 6B " +
            "74 CC EC 3A CC D2 4D 72 55 29 1B 51 8E 6C CA D2 " +
            "83 AD 5F 38 75 FD 7F EE 04 12 BC C6 60 25 71 48 " +
            "7C 8C 05 F4 14 EB C8 6B 9B 5A B2 B8 69 89 2D 3D " +
            "FA 3B BB D2 DB BB F3 E5 B1 A7 21 CC 7A 32 BF 5B " +
            "EB 55 C1 D0 3D 16 AC 21 9B 83 50 58 D2 F3 5B AD " +
            "3B B7 D9 6B 81 EC CD 43 B2 4F 4C 06 6F E9 B0 79 " +
            "20 8C 16 FC F0 73 78 29",
            // Round #2 
            // After Theta
            "1B AE F0 A7 62 E5 AB F2 2D 24 34 17 34 0C 47 37 " +
            "12 AB 2C 1F 29 33 34 E3 BE 9D 49 89 01 E1 07 A4 " +
            "A9 EB E4 E0 BB 51 6E 53 46 92 E1 A2 D0 20 64 82 " +
            "4B 1B 7D 59 1F FB 41 56 14 F4 B2 91 9E B1 71 74 " +
            "13 2E 8A E4 E7 B4 7D F5 3F 20 06 20 1A 86 87 F9 " +
            "92 70 F0 65 2A F7 96 8E 51 EE 84 01 27 39 39 65 " +
            "A4 CD D8 9B 58 48 CC 81 8F CC 16 A7 A0 8F 80 25 " +
            "B9 BC F3 69 2B 77 BB 83 E1 E6 E1 EC 64 D4 EE E6 " +
            "1C 8E F6 66 F3 70 03 65 4B 5B 86 19 FD 13 AC CE " +
            "20 DE B6 24 F5 58 B9 12 8B B6 8D 9D 24 B8 7B 36 " +
            "0E A1 9C FA 39 E7 33 8F FB 81 A3 CA 35 68 90 A3 " +
            "EB B6 ED CA 15 76 4C B0 68 AA 41 F0 41 0A FA 8E " +
            "1A 9D BA AD AE F9 BC 44",
            // After Rho
            "1B AE F0 A7 62 E5 AB F2 5A 48 68 2E 68 18 8E 6E " +
            "C4 2A CB 47 CA 0C CD B8 10 7E 40 EA DB 99 94 18 " +
            "8D 72 9B 4A 5D 27 07 DF 0A 0D 42 26 68 24 19 2E " +
            "97 F5 B1 1F 64 B5 B4 D1 1D 05 BD 6C A4 67 6C 1C " +
            "17 45 F2 73 DA BE FA 09 78 98 FF 03 62 00 A2 61 " +
            "94 84 83 2F 53 B9 B7 74 94 45 B9 13 06 9C E4 E4 " +
            "DE C4 42 62 0E 24 6D C6 1F 01 4B 1E 99 2D 4E 41 " +
            "B4 95 BB DD C1 5C DE F9 D9 C9 A8 DD CD C3 CD C3 " +
            "DE 6C 1E 6E A0 8C C3 D1 56 E7 A5 2D C3 8C FE 09 " +
            "2B 57 02 C4 DB 96 A4 1E 36 8B B6 8D 9D 24 B8 7B " +
            "CF 3C 3A 84 72 EA E7 9C EE 07 8E 2A D7 A0 41 8E " +
            "DD B6 5D B9 C2 8E 09 76 AA 41 F0 41 0A FA 8E 68 " +
            "2F 91 46 A7 6E AB 6B 3E",
            // After Pi 
            "1B AE F0 A7 62 E5 AB F2 97 F5 B1 1F 64 B5 B4 D1 " +
            "DE C4 42 62 0E 24 6D C6 2B 57 02 C4 DB 96 A4 1E " +
            "2F 91 46 A7 6E AB 6B 3E 10 7E 40 EA DB 99 94 18 " +
            "78 98 FF 03 62 00 A2 61 94 84 83 2F 53 B9 B7 74 " +
            "DE 6C 1E 6E A0 8C C3 D1 DD B6 5D B9 C2 8E 09 76 " +
            "5A 48 68 2E 68 18 8E 6E 1D 05 BD 6C A4 67 6C 1C " +
            "1F 01 4B 1E 99 2D 4E 41 36 8B B6 8D 9D 24 B8 7B " +
            "CF 3C 3A 84 72 EA E7 9C 8D 72 9B 4A 5D 27 07 DF " +
            "0A 0D 42 26 68 24 19 2E 94 45 B9 13 06 9C E4 E4 " +
            "56 E7 A5 2D C3 8C FE 09 AA 41 F0 41 0A FA 8E 68 " +
            "C4 2A CB 47 CA 0C CD B8 17 45 F2 73 DA BE FA 09 " +
            "B4 95 BB DD C1 5C DE F9 D9 C9 A8 DD CD C3 CD C3 " +
            "EE 07 8E 2A D7 A0 41 8E",
            // After Chi
            "53 AE B2 C7 68 E5 E2 F4 B6 E6 B1 9B B5 27 34 C9 " +
            "DA 44 06 41 2A 0D 26 E6 3B 79 B2 C4 DB D2 24 DE " +
            "AB C0 47 BF 6A BB 7F 3F 94 7A 40 C6 CA 20 81 0C " +
            "32 F0 E3 43 C2 04 E2 E0 95 16 C2 BE 11 BB BF 52 " +
            "DE 24 1E 2C B9 9D 57 D9 B5 36 E2 B8 E2 8E 2B 17 " +
            "58 48 2A 3C 71 10 8C 2F 3D 8F 09 ED A0 67 DC 26 " +
            "D6 35 43 1E FB E7 09 C5 26 CB F6 A7 95 34 B0 19 " +
            "CA 39 AF C4 F6 8D 87 8C 19 32 22 5B 5B BF E3 1F " +
            "48 AF 46 0A A9 24 03 27 3C 45 E9 53 0E EE E4 84 " +
            "53 D5 AE 27 96 89 FF 9E A8 4C B0 65 2A FA 96 48 " +
            "64 BA C2 CB CB 4C C9 48 5E 0D F2 73 D6 3D FB 0B " +
            "92 93 BD FF D3 7C DE F5 D9 E1 E9 98 C5 CF 41 F3 " +
            "FD 42 BE 1A C7 12 73 8F",
            // After Iota  
            "D9 2E B2 C7 68 E5 E2 74 B6 E6 B1 9B B5 27 34 C9 " +
            "DA 44 06 41 2A 0D 26 E6 3B 79 B2 C4 DB D2 24 DE " +
            "AB C0 47 BF 6A BB 7F 3F 94 7A 40 C6 CA 20 81 0C " +
            "32 F0 E3 43 C2 04 E2 E0 95 16 C2 BE 11 BB BF 52 " +
            "DE 24 1E 2C B9 9D 57 D9 B5 36 E2 B8 E2 8E 2B 17 " +
            "58 48 2A 3C 71 10 8C 2F 3D 8F 09 ED A0 67 DC 26 " +
            "D6 35 43 1E FB E7 09 C5 26 CB F6 A7 95 34 B0 19 " +
            "CA 39 AF C4 F6 8D 87 8C 19 32 22 5B 5B BF E3 1F " +
            "48 AF 46 0A A9 24 03 27 3C 45 E9 53 0E EE E4 84 " +
            "53 D5 AE 27 96 89 FF 9E A8 4C B0 65 2A FA 96 48 " +
            "64 BA C2 CB CB 4C C9 48 5E 0D F2 73 D6 3D FB 0B " +
            "92 93 BD FF D3 7C DE F5 D9 E1 E9 98 C5 CF 41 F3 " +
            "FD 42 BE 1A C7 12 73 8F",
            // Round #3 
            // After Theta
            "06 98 68 E2 AB 0E 30 50 B0 10 2E AD CC 87 A4 C8 " +
            "E7 3B D2 ED CB 2B 2E 23 0E 4B 68 F1 E1 B0 E2 18 " +
            "32 4A 2B 15 49 CA 88 4D 4B CC 9A E3 09 CB 53 28 " +
            "34 06 7C 75 BB A4 72 E1 A8 69 16 12 F0 9D B7 97 " +
            "EB 16 C4 19 83 FF 91 1F 2C BC 8E 12 C1 FF DC 65 " +
            "87 FE F0 19 B2 FB 5E 0B 3B 79 96 DB D9 C7 4C 27 " +
            "EB 4A 97 B2 1A C1 01 00 13 F9 2C 92 AF 56 76 DF " +
            "53 B3 C3 6E D5 FC 70 FE C6 84 F8 7E 98 54 31 3B " +
            "4E 59 D9 3C D0 84 93 26 01 3A 3D FF EF C8 EC 41 " +
            "66 E7 74 12 AC EB 39 58 31 C6 DC CF 09 8B 61 3A " +
            "BB 0C 18 EE 08 A7 1B 6C 58 FB 6D 45 AF 9D 6B 0A " +
            "AF EC 69 53 32 5A D6 30 EC D3 33 AD FF AD 87 35 " +
            "64 C8 D2 B0 E4 63 84 FD",
            // After Rho
            "06 98 68 E2 AB 0E 30 50 61 21 5C 5A 99 0F 49 91 " +
            "F9 8E 74 FB F2 8A CB C8 0E 2B 8E E1 B0 84 16 1F " +
            "52 46 6C 92 51 5A A9 48 9E B0 3C 85 B2 C4 AC 39 " +
            "57 B7 4B 2A 17 4E 63 C0 25 6A 9A 85 04 7C E7 ED " +
            "0B E2 8C C1 FF C8 8F 75 CF 5D C6 C2 EB 28 11 FC " +
            "38 F4 87 CF 90 DD F7 5A 9D EC E4 59 6E 67 1F 33 " +
            "94 D5 08 0E 00 58 57 BA AD EC BE 27 F2 59 24 5F " +
            "B7 6A 7E 38 FF A9 D9 61 FD 30 A9 62 76 8C 09 F1 " +
            "9B 07 9A 70 D2 C4 29 2B F6 A0 00 9D 9E FF 77 64 " +
            "3D 07 CB EC 9C 4E 82 75 3A 31 C6 DC CF 09 8B 61 " +
            "6E B0 ED 32 60 B8 23 9C 60 ED B7 15 BD 76 AE 29 " +
            "95 3D 6D 4A 46 CB 1A E6 D3 33 AD FF AD 87 35 EC " +
            "61 3F 19 B2 34 2C F9 18",
            // After Pi 
            "06 98 68 E2 AB 0E 30 50 57 B7 4B 2A 17 4E 63 C0 " +
            "94 D5 08 0E 00 58 57 BA 3D 07 CB EC 9C 4E 82 75 " +
            "61 3F 19 B2 34 2C F9 18 0E 2B 8E E1 B0 84 16 1F " +
            "CF 5D C6 C2 EB 28 11 FC 38 F4 87 CF 90 DD F7 5A " +
            "9B 07 9A 70 D2 C4 29 2B 95 3D 6D 4A 46 CB 1A E6 " +
            "61 21 5C 5A 99 0F 49 91 25 6A 9A 85 04 7C E7 ED " +
            "AD EC BE 27 F2 59 24 5F 3A 31 C6 DC CF 09 8B 61 " +
            "6E B0 ED 32 60 B8 23 9C 52 46 6C 92 51 5A A9 48 " +
            "9E B0 3C 85 B2 C4 AC 39 9D EC E4 59 6E 67 1F 33 " +
            "F6 A0 00 9D 9E FF 77 64 D3 33 AD FF AD 87 35 EC " +
            "F9 8E 74 FB F2 8A CB C8 0B E2 8C C1 FF C8 8F 75 " +
            "B7 6A 7E 38 FF A9 D9 61 FD 30 A9 62 76 8C 09 F1 " +
            "60 ED B7 15 BD 76 AE 29",
            // After Chi
            "86 D8 68 E6 AB 1E 24 6A 7E B5 88 CA 8B 48 E3 85 " +
            "D4 ED 18 1C 20 78 2E B2 3B 87 AB AC 17 4C 82 35 " +
            "30 18 1A BA 20 6C BA 98 3E 8B 8F EC A0 51 F0 1D " +
            "4C 5E DE F2 A9 28 19 DD 3C CC E2 C5 94 D6 E5 9E " +
            "91 05 18 D1 62 C0 2D 32 54 69 2D 48 0D E3 1B 06 " +
            "E9 A5 78 78 6B 0E 49 83 37 7B DA 5D 09 7C 6C CD " +
            "E9 6C 97 05 D2 E9 04 C3 3B 30 D6 94 56 0E C3 60 " +
            "6A FA 6F B7 64 C8 85 F0 53 0A AC CA 1D 79 BA 4A " +
            "FC B0 3C 01 22 5C CC 7D 9C FF 49 3B 4F 67 1F BB " +
            "F6 E4 40 9D CE A7 FF 64 5F 83 BD FA 0F 03 31 DD " +
            "4D 86 06 C3 F2 AB 9B C8 43 F2 0D 83 FF CC 8F E5 " +
            "B7 A7 68 2D 76 DB 7F 69 64 32 E9 88 34 04 48 31 " +
            "62 8D 3F 15 B0 36 AA 1C",
            // After Iota  
            "86 58 68 66 AB 1E 24 EA 7E B5 88 CA 8B 48 E3 85 " +
            "D4 ED 18 1C 20 78 2E B2 3B 87 AB AC 17 4C 82 35 " +
            "30 18 1A BA 20 6C BA 98 3E 8B 8F EC A0 51 F0 1D " +
            "4C 5E DE F2 A9 28 19 DD 3C CC E2 C5 94 D6 E5 9E " +
            "91 05 18 D1 62 C0 2D 32 54 69 2D 48 0D E3 1B 06 " +
            "E9 A5 78 78 6B 0E 49 83 37 7B DA 5D 09 7C 6C CD " +
            "E9 6C 97 05 D2 E9 04 C3 3B 30 D6 94 56 0E C3 60 " +
            "6A FA 6F B7 64 C8 85 F0 53 0A AC CA 1D 79 BA 4A " +
            "FC B0 3C 01 22 5C CC 7D 9C FF 49 3B 4F 67 1F BB " +
            "F6 E4 40 9D CE A7 FF 64 5F 83 BD FA 0F 03 31 DD " +
            "4D 86 06 C3 F2 AB 9B C8 43 F2 0D 83 FF CC 8F E5 " +
            "B7 A7 68 2D 76 DB 7F 69 64 32 E9 88 34 04 48 31 " +
            "62 8D 3F 15 B0 36 AA 1C",
            // Round #4 
            // After Theta
            "C1 78 C9 03 B0 75 30 5E 65 65 25 A5 BB 2D 00 08 " +
            "68 F7 3D 02 65 B7 4D DA 76 98 52 33 A5 52 53 57 " +
            "AC 88 BD B0 E6 6A 18 47 79 AB 2E 89 BB 3A E4 A9 " +
            "57 8E 73 9D 99 4D FA 50 80 D6 C7 DB D1 19 86 F6 " +
            "DC 1A E1 4E D0 DE FC 50 C8 F9 8A 42 CB E5 B9 D9 " +
            "AE 85 D9 1D 70 65 5D 37 2C AB 77 32 39 19 8F 40 " +
            "55 76 B2 1B 97 26 67 AB 76 2F 2F 0B E4 10 12 02 " +
            "F6 6A C8 BD A2 CE 27 2F 14 2A 0D AF 06 12 AE FE " +
            "E7 60 91 6E 12 39 2F F0 20 E5 6C 25 0A A8 7C D3 " +
            "BB FB B9 02 7C B9 2E 06 C3 13 1A F0 C9 05 93 02 " +
            "0A A6 A7 A6 E9 C0 8F 7C 58 22 A0 EC CF A9 6C 68 " +
            "0B BD 4D 33 33 14 1C 01 29 2D 10 17 86 1A 99 53 " +
            "FE 1D 98 1F 76 30 08 C3",
            // After Rho
            "C1 78 C9 03 B0 75 30 5E CA CA 4A 4A 77 5B 00 10 " +
            "DA 7D 8F 40 D9 6D 93 36 2A 35 75 65 87 29 35 53 " +
            "57 C3 38 62 45 EC 85 35 B8 AB 43 9E 9A B7 EA 92 " +
            "D7 99 D9 A4 0F 75 E5 38 3D A0 F5 F1 76 74 86 A1 " +
            "8D 70 27 68 6F 7E 28 6E 9E 9B 8D 9C AF 28 B4 5C " +
            "71 2D CC EE 80 2B EB BA 02 B1 AC DE C9 E4 64 3C " +
            "DD B8 34 39 5B AD B2 93 21 24 04 EC 5E 5E 16 C8 " +
            "5E 51 E7 93 17 7B 35 E4 5E 0D 24 5C FD 29 54 1A " +
            "D2 4D 22 E7 05 FE 1C 2C BE 69 90 72 B6 12 05 54 " +
            "D7 C5 60 77 3F 57 80 2F 02 C3 13 1A F0 C9 05 93 " +
            "3F F2 29 98 9E 9A A6 03 61 89 80 B2 3F A7 B2 A1 " +
            "A1 B7 69 66 86 82 23 60 2D 10 17 86 1A 99 53 29 " +
            "C2 B0 7F 07 E6 87 1D 0C",
            // After Pi 
            "C1 78 C9 03 B0 75 30 5E D7 99 D9 A4 0F 75 E5 38 " +
            "DD B8 34 39 5B AD B2 93 D7 C5 60 77 3F 57 80 2F " +
            "C2 B0 7F 07 E6 87 1D 0C 2A 35 75 65 87 29 35 53 " +
            "9E 9B 8D 9C AF 28 B4 5C 71 2D CC EE 80 2B EB BA " +
            "D2 4D 22 E7 05 FE 1C 2C A1 B7 69 66 86 82 23 60 " +
            "CA CA 4A 4A 77 5B 00 10 3D A0 F5 F1 76 74 86 A1 " +
            "21 24 04 EC 5E 5E 16 C8 02 C3 13 1A F0 C9 05 93 " +
            "3F F2 29 98 9E 9A A6 03 57 C3 38 62 45 EC 85 35 " +
            "B8 AB 43 9E 9A B7 EA 92 02 B1 AC DE C9 E4 64 3C " +
            "BE 69 90 72 B6 12 05 54 2D 10 17 86 1A 99 53 29 " +
            "DA 7D 8F 40 D9 6D 93 36 8D 70 27 68 6F 7E 28 6E " +
            "5E 51 E7 93 17 7B 35 E4 5E 0D 24 5C FD 29 54 1A " +
            "61 89 80 B2 3F A7 B2 A1",
            // After Chi
            "C9 58 ED 1A E0 FD 22 DD D5 DC 99 E2 2B 27 E5 14 " +
            "DD 88 2B 39 9B 2D AF 93 D6 8D E0 77 2F 27 A0 7D " +
            "D4 31 6F A3 E9 87 D8 2C 4B 11 35 07 87 2A 7E F1 " +
            "1C DB AF 9D AA FC A0 58 50 9F 85 EE 02 2B C8 FA " +
            "D8 4D 36 E6 04 D7 08 3F 35 3D E1 FE AE 82 A3 6C " +
            "CA CE 4A 46 7F 51 10 58 3F 63 E6 E3 D6 F5 87 B2 " +
            "1C 14 2C 6C 50 4C B4 C8 C2 CB 51 58 91 88 05 83 " +
            "0A D2 9C 29 9E BE 20 A2 55 D3 94 22 04 AC 81 19 " +
            "04 E3 53 BE AC A5 EB D2 03 A1 AB 5A C1 6D 36 15 " +
            "EC AA B8 12 F3 76 81 40 85 38 54 1A 80 8A 39 AB " +
            "88 7C 4F D3 C9 6C 86 B6 8D 7C 27 24 87 7E 68 74 " +
            "7F D1 67 31 15 FD 97 45 C4 79 2B 1C 3D 61 55 0C " +
            "64 89 A0 9A 19 B5 9A E9",
            // After Iota  
            "42 D8 ED 1A E0 FD 22 DD D5 DC 99 E2 2B 27 E5 14 " +
            "DD 88 2B 39 9B 2D AF 93 D6 8D E0 77 2F 27 A0 7D " +
            "D4 31 6F A3 E9 87 D8 2C 4B 11 35 07 87 2A 7E F1 " +
            "1C DB AF 9D AA FC A0 58 50 9F 85 EE 02 2B C8 FA " +
            "D8 4D 36 E6 04 D7 08 3F 35 3D E1 FE AE 82 A3 6C " +
            "CA CE 4A 46 7F 51 10 58 3F 63 E6 E3 D6 F5 87 B2 " +
            "1C 14 2C 6C 50 4C B4 C8 C2 CB 51 58 91 88 05 83 " +
            "0A D2 9C 29 9E BE 20 A2 55 D3 94 22 04 AC 81 19 " +
            "04 E3 53 BE AC A5 EB D2 03 A1 AB 5A C1 6D 36 15 " +
            "EC AA B8 12 F3 76 81 40 85 38 54 1A 80 8A 39 AB " +
            "88 7C 4F D3 C9 6C 86 B6 8D 7C 27 24 87 7E 68 74 " +
            "7F D1 67 31 15 FD 97 45 C4 79 2B 1C 3D 61 55 0C " +
            "64 89 A0 9A 19 B5 9A E9",
            // Round #5 
            // After Theta
            "B6 41 42 E3 58 93 59 CD 10 93 4C E8 C5 D5 4B 2D " +
            "6B C2 A6 B1 0E 06 1C D1 2E 20 62 4E B3 F5 23 CD " +
            "0D B9 E8 30 36 65 37 17 BF 88 9A FE 3F 44 05 E1 " +
            "D9 94 7A 97 44 0E 0E 61 E6 D5 08 66 97 00 7B B8 " +
            "20 E0 B4 DF 98 05 8B 8F EC B5 66 6D 71 60 4C 57 " +
            "3E 57 E5 BF C7 3F 6B 48 FA 2C 33 E9 38 07 29 8B " +
            "AA 5E A1 E4 C5 67 07 8A 3A 66 D3 61 0D 5A 86 33 " +
            "D3 5A 1B BA 41 5C CF 99 A1 4A 3B DB BC C2 FA 09 " +
            "C1 AC 86 B4 42 57 45 EB B5 EB 26 D2 54 46 85 57 " +
            "14 07 3A 2B 6F A4 02 F0 5C B0 D3 89 5F 68 D6 90 " +
            "7C E5 E0 2A 71 02 FD A6 48 33 F2 2E 69 8C C6 4D " +
            "C9 9B EA B9 80 D6 24 07 3C D4 A9 25 A1 B3 D6 BC " +
            "BD 01 27 09 C6 57 75 D2",
            // After Rho
            "B6 41 42 E3 58 93 59 CD 20 26 99 D0 8B AB 97 5A " +
            "9A B0 69 AC 83 01 47 F4 5B 3F D2 EC 02 22 E6 34 " +
            "29 BB B9 68 C8 45 87 B1 FF 43 54 10 FE 8B A8 E9 " +
            "77 49 E4 E0 10 96 4D A9 AE 79 35 82 D9 25 C0 1E " +
            "70 DA 6F CC 82 C5 47 10 C6 74 C5 5E 6B D6 16 07 " +
            "F2 B9 2A FF 3D FE 59 43 2C EA B3 CC A4 E3 1C A4 " +
            "25 2F 3E 3B 50 54 F5 0A B4 0C 67 74 CC A6 C3 1A " +
            "DD 20 AE E7 CC 69 AD 0D B6 79 85 F5 13 42 95 76 " +
            "90 56 E8 AA 68 3D 98 D5 C2 AB DA 75 13 69 2A A3 " +
            "54 00 9E E2 40 67 E5 8D 90 5C B0 D3 89 5F 68 D6 " +
            "F4 9B F2 95 83 AB C4 09 21 CD C8 BB A4 31 1A 37 " +
            "79 53 3D 17 D0 9A E4 20 D4 A9 25 A1 B3 D6 BC 3C " +
            "9D 74 6F C0 49 82 F1 55",
            // After Pi 
            "B6 41 42 E3 58 93 59 CD 77 49 E4 E0 10 96 4D A9 " +
            "25 2F 3E 3B 50 54 F5 0A 54 00 9E E2 40 67 E5 8D " +
            "9D 74 6F C0 49 82 F1 55 5B 3F D2 EC 02 22 E6 34 " +
            "C6 74 C5 5E 6B D6 16 07 F2 B9 2A FF 3D FE 59 43 " +
            "90 56 E8 AA 68 3D 98 D5 79 53 3D 17 D0 9A E4 20 " +
            "20 26 99 D0 8B AB 97 5A AE 79 35 82 D9 25 C0 1E " +
            "B4 0C 67 74 CC A6 C3 1A 90 5C B0 D3 89 5F 68 D6 " +
            "F4 9B F2 95 83 AB C4 09 29 BB B9 68 C8 45 87 B1 " +
            "FF 43 54 10 FE 8B A8 E9 2C EA B3 CC A4 E3 1C A4 " +
            "C2 AB DA 75 13 69 2A A3 D4 A9 25 A1 B3 D6 BC 3C " +
            "9A B0 69 AC 83 01 47 F4 70 DA 6F CC 82 C5 47 10 " +
            "DD 20 AE E7 CC 69 AD 0D B6 79 85 F5 13 42 95 76 " +
            "21 CD C8 BB A4 31 1A 37",
            // After Chi
            "B6 67 58 F8 18 D3 E9 CF 27 49 64 20 10 B5 4D 2C " +
            "AC 5B 5F 3B 59 D4 E5 5A 76 01 9E C1 50 76 ED 05 " +
            "DC 7C CB C0 49 86 F5 75 6B B6 F8 4D 16 0A AF 74 " +
            "C6 32 05 5E 2B D7 96 93 9B B8 3F EA AD 7C 3D 63 " +
            "92 7A 2A 42 6A 1D 9A C1 FD 13 38 05 B9 4E F4 23 " +
            "30 22 DB A4 8F 29 94 5A AE 29 A5 01 D8 7C E8 DA " +
            "D0 8F 25 70 CE 06 47 13 90 78 B9 93 81 5F 7B 84 " +
            "7A C2 D6 97 D3 AF 84 0D 29 13 1A A4 C8 25 93 B5 " +
            "3D 42 1C 21 ED 83 8A EA 38 EA 96 4C 04 75 88 B8 " +
            "EB B9 42 3D 5B 68 29 22 02 E9 61 B1 85 5C 94 74 " +
            "17 90 E9 8F CF 29 EF F9 52 83 6E DC 91 C7 57 62 " +
            "DC A4 E6 ED 68 58 A7 0C 2C 49 A4 F1 10 42 D0 B6 " +
            "41 87 CE FB A4 F5 1A 37",
            // After Iota  
            "B7 67 58 78 18 D3 E9 CF 27 49 64 20 10 B5 4D 2C " +
            "AC 5B 5F 3B 59 D4 E5 5A 76 01 9E C1 50 76 ED 05 " +
            "DC 7C CB C0 49 86 F5 75 6B B6 F8 4D 16 0A AF 74 " +
            "C6 32 05 5E 2B D7 96 93 9B B8 3F EA AD 7C 3D 63 " +
            "92 7A 2A 42 6A 1D 9A C1 FD 13 38 05 B9 4E F4 23 " +
            "30 22 DB A4 8F 29 94 5A AE 29 A5 01 D8 7C E8 DA " +
            "D0 8F 25 70 CE 06 47 13 90 78 B9 93 81 5F 7B 84 " +
            "7A C2 D6 97 D3 AF 84 0D 29 13 1A A4 C8 25 93 B5 " +
            "3D 42 1C 21 ED 83 8A EA 38 EA 96 4C 04 75 88 B8 " +
            "EB B9 42 3D 5B 68 29 22 02 E9 61 B1 85 5C 94 74 " +
            "17 90 E9 8F CF 29 EF F9 52 83 6E DC 91 C7 57 62 " +
            "DC A4 E6 ED 68 58 A7 0C 2C 49 A4 F1 10 42 D0 B6 " +
            "41 87 CE FB A4 F5 1A 37",
            // Round #6 
            // After Theta
            "EE 82 BF 65 25 A8 3E 0C F2 7D 86 9A 3A 4F 82 BC " +
            "EB 2F 3E 00 27 B3 E1 1E 45 A5 BE F0 02 69 4A AB " +
            "CA 6E 30 69 B4 61 5D FA 32 53 1F 50 2B 71 78 B7 " +
            "13 06 E7 E4 01 2D 59 03 DC CC 5E D1 D3 1B 39 27 " +
            "A1 DE 0A 73 38 02 3D 6F EB 01 C3 AC 44 A9 5C AC " +
            "69 C7 3C B9 B2 52 43 99 7B 1D 47 BB F2 86 27 4A " +
            "97 FB 44 4B B0 61 43 57 A3 DC 99 A2 D3 40 DC 2A " +
            "6C D0 2D 3E 2E 48 2C 82 70 F6 FD B9 F5 5E 44 76 " +
            "E8 76 FE 9B C7 79 45 7A 7F 9E F7 77 7A 12 8C FC " +
            "D8 1D 62 0C 09 77 8E 8C 14 FB 9A 18 78 BB 3C FB " +
            "4E 75 0E 92 F2 52 38 3A 87 B7 8C 66 BB 3D 98 F2 " +
            "9B D0 87 D6 16 3F A3 48 1F ED 84 C0 42 5D 77 18 " +
            "57 95 35 52 59 12 B2 B8",
            // After Rho
            "EE 82 BF 65 25 A8 3E 0C E5 FB 0C 35 75 9E 04 79 " +
            "FA 8B 0F C0 C9 6C B8 C7 90 A6 B4 5A 54 EA 0B 2F " +
            "0D EB D2 57 76 83 49 A3 B5 12 87 77 2B 33 F5 01 " +
            "4E 1E D0 92 35 30 61 70 09 37 B3 57 F4 F4 46 CE " +
            "6F 85 39 1C 81 9E B7 50 CA C5 BA 1E 30 CC 4A 94 " +
            "4C 3B E6 C9 95 95 1A CA 28 ED 75 1C ED CA 1B 9E " +
            "5A 82 0D 1B BA BA DC 27 81 B8 55 46 B9 33 45 A7 " +
            "1F 17 24 16 41 36 E8 16 73 EB BD 88 EC E0 EC FB " +
            "7F F3 38 AF 48 0F DD CE 46 FE 3F CF FB 3B 3D 09 " +
            "CE 91 11 BB 43 8C 21 E1 FB 14 FB 9A 18 78 BB 3C " +
            "E1 E8 38 D5 39 48 CA 4B 1F DE 32 9A ED F6 60 CA " +
            "13 FA D0 DA E2 67 14 69 ED 84 C0 42 5D 77 18 1F " +
            "2C EE 55 65 8D 54 96 84",
            // After Pi 
            "EE 82 BF 65 25 A8 3E 0C 4E 1E D0 92 35 30 61 70 " +
            "5A 82 0D 1B BA BA DC 27 CE 91 11 BB 43 8C 21 E1 " +
            "2C EE 55 65 8D 54 96 84 90 A6 B4 5A 54 EA 0B 2F " +
            "CA C5 BA 1E 30 CC 4A 94 4C 3B E6 C9 95 95 1A CA " +
            "7F F3 38 AF 48 0F DD CE 13 FA D0 DA E2 67 14 69 " +
            "E5 FB 0C 35 75 9E 04 79 09 37 B3 57 F4 F4 46 CE " +
            "81 B8 55 46 B9 33 45 A7 FB 14 FB 9A 18 78 BB 3C " +
            "E1 E8 38 D5 39 48 CA 4B 0D EB D2 57 76 83 49 A3 " +
            "B5 12 87 77 2B 33 F5 01 28 ED 75 1C ED CA 1B 9E " +
            "46 FE 3F CF FB 3B 3D 09 ED 84 C0 42 5D 77 18 1F " +
            "FA 8B 0F C0 C9 6C B8 C7 6F 85 39 1C 81 9E B7 50 " +
            "1F 17 24 16 41 36 E8 16 73 EB BD 88 EC E0 EC FB " +
            "1F DE 32 9A ED F6 60 CA",
            // After Chi
            "FE 02 B2 6C AF 22 A2 0B CA 0F C0 32 74 34 40 B0 " +
            "7A EC 49 5F 36 EA 4A 23 0C 91 BB BB 63 24 09 E9 " +
            "2C F2 15 F7 9D 44 D7 F4 94 9C F0 9B D1 FB 1B 65 " +
            "F9 05 A2 38 78 C6 8F 90 4C 33 26 99 37 F5 1A EB " +
            "FF F7 1C AF 5C 87 D6 C8 59 BB DA DE C2 63 54 F9 " +
            "65 73 48 35 7C 9D 05 58 73 33 19 CF F4 BC FC D6 " +
            "81 50 55 03 98 33 05 E4 FF 07 FF BA 5C EE BF 0C " +
            "E9 EC 8B 97 B9 28 88 CD 05 06 A2 5F B2 4B 43 3D " +
            "F3 00 8D B4 39 02 D1 00 81 ED B5 1C E9 8E 1B 88 " +
            "46 95 2D DA D9 BB 7C A9 5D 94 C5 62 54 47 AC 1F " +
            "EA 99 0B C2 89 4C F0 C1 0F 6D A0 94 2D 5E B3 B9 " +
            "13 03 26 04 40 20 E8 16 93 EA B0 C8 EC E8 74 FE " +
            "1A DA 02 86 ED 64 67 DA",
            // After Iota  
            "7F 82 B2 EC AF 22 A2 8B CA 0F C0 32 74 34 40 B0 " +
            "7A EC 49 5F 36 EA 4A 23 0C 91 BB BB 63 24 09 E9 " +
            "2C F2 15 F7 9D 44 D7 F4 94 9C F0 9B D1 FB 1B 65 " +
            "F9 05 A2 38 78 C6 8F 90 4C 33 26 99 37 F5 1A EB " +
            "FF F7 1C AF 5C 87 D6 C8 59 BB DA DE C2 63 54 F9 " +
            "65 73 48 35 7C 9D 05 58 73 33 19 CF F4 BC FC D6 " +
            "81 50 55 03 98 33 05 E4 FF 07 FF BA 5C EE BF 0C " +
            "E9 EC 8B 97 B9 28 88 CD 05 06 A2 5F B2 4B 43 3D " +
            "F3 00 8D B4 39 02 D1 00 81 ED B5 1C E9 8E 1B 88 " +
            "46 95 2D DA D9 BB 7C A9 5D 94 C5 62 54 47 AC 1F " +
            "EA 99 0B C2 89 4C F0 C1 0F 6D A0 94 2D 5E B3 B9 " +
            "13 03 26 04 40 20 E8 16 93 EA B0 C8 EC E8 74 FE " +
            "1A DA 02 86 ED 64 67 DA",
            // Round #7 
            // After Theta
            "DC C0 9D 7C 29 2B C0 10 E0 3F 31 56 2C 73 02 9F " +
            "74 85 95 C3 77 C4 CB 98 9F 27 15 D3 ED FE 2F 50 " +
            "37 08 97 F4 B8 DC A1 1A 37 DE DF 0B 57 F2 79 FE " +
            "D3 35 53 5C 20 81 CD BF 42 5A FA 05 76 DB 9B 50 " +
            "6C 41 B2 C7 D2 5D F0 71 42 41 58 DD E7 FB 22 17 " +
            "C6 31 67 A5 FA 94 67 C3 59 03 E8 AB AC FB BE F9 " +
            "8F 39 89 9F D9 1D 84 5F 6C B1 51 D2 D2 34 99 B5 " +
            "F2 16 09 94 9C B0 FE 23 A6 44 8D CF 34 42 21 A6 " +
            "D9 30 7C D0 61 45 93 2F 8F 84 69 80 A8 A0 9A 33 " +
            "D5 23 83 B2 57 61 5A 10 46 6E 47 61 71 DF DA F1 " +
            "49 DB 24 52 0F 45 92 5A 25 5D 51 F0 75 19 F1 96 " +
            "1D 6A FA 98 01 0E 69 AD 00 5C 1E A0 62 32 52 47 " +
            "01 20 80 85 C8 FC 11 34",
            // After Rho
            "DC C0 9D 7C 29 2B C0 10 C1 7F 62 AC 58 E6 04 3E " +
            "5D 61 E5 F0 1D F1 32 26 EE FF 02 F5 79 52 31 DD " +
            "E5 0E D5 B8 41 B8 A4 C7 70 25 9F E7 7F E3 FD BD " +
            "C5 05 12 D8 FC 3B 5D 33 94 90 96 7E 81 DD F6 26 " +
            "20 D9 63 E9 2E F8 38 B6 2F 72 21 14 84 D5 7D BE " +
            "36 8E 39 2B D5 A7 3C 1B E6 67 0D A0 AF B2 EE FB " +
            "FC CC EE 20 FC 7A CC 49 69 32 6B D9 62 A3 A4 A5 " +
            "4A 4E 58 FF 11 79 8B 04 9F 69 84 42 4C 4D 89 1A " +
            "0F 3A AC 68 F2 25 1B 86 CD 99 47 C2 34 40 54 50 " +
            "4C 0B A2 7A 64 50 F6 2A F1 46 6E 47 61 71 DF DA " +
            "49 6A 25 6D 93 48 3D 14 96 74 45 C1 D7 65 C4 5B " +
            "43 4D 1F 33 C0 21 AD B5 5C 1E A0 62 32 52 47 00 " +
            "04 4D 00 08 60 21 32 7F",
            // After Pi 
            "DC C0 9D 7C 29 2B C0 10 C5 05 12 D8 FC 3B 5D 33 " +
            "FC CC EE 20 FC 7A CC 49 4C 0B A2 7A 64 50 F6 2A " +
            "04 4D 00 08 60 21 32 7F EE FF 02 F5 79 52 31 DD " +
            "2F 72 21 14 84 D5 7D BE 36 8E 39 2B D5 A7 3C 1B " +
            "0F 3A AC 68 F2 25 1B 86 43 4D 1F 33 C0 21 AD B5 " +
            "C1 7F 62 AC 58 E6 04 3E 94 90 96 7E 81 DD F6 26 " +
            "69 32 6B D9 62 A3 A4 A5 F1 46 6E 47 61 71 DF DA " +
            "49 6A 25 6D 93 48 3D 14 E5 0E D5 B8 41 B8 A4 C7 " +
            "70 25 9F E7 7F E3 FD BD E6 67 0D A0 AF B2 EE FB " +
            "CD 99 47 C2 34 40 54 50 5C 1E A0 62 32 52 47 00 " +
            "5D 61 E5 F0 1D F1 32 26 20 D9 63 E9 2E F8 38 B6 " +
            "4A 4E 58 FF 11 79 8B 04 9F 69 84 42 4C 4D 89 1A " +
            "96 74 45 C1 D7 65 C4 5B",
            // After Chi
            "E4 08 71 5C 29 6B 40 58 C5 06 12 82 FC 3B 6F 11 " +
            "FC 88 EE 20 FC 5B CC 1C 94 8B 3F 0E 6D 5A 36 2A " +
            "05 48 02 88 B4 31 2F 5C FE 73 1A DE 28 70 31 DC " +
            "26 42 A5 54 A6 D5 7E 3A 76 CB 2A 38 D5 A7 98 2A " +
            "A3 88 AC AC CB 77 0B CE 42 4D 3E 33 44 A4 E1 97 " +
            "A8 5D 0B 2D 3A C4 04 BF 04 D4 92 78 80 8D AD 7C " +
            "61 1A 6A F1 F0 AB 84 A1 71 53 2C C7 29 D7 DF F0 " +
            "5D EA B1 3F 12 51 CF 14 63 4C D5 B8 C1 A8 A6 85 " +
            "79 BD DD A5 6F A3 ED BD F6 61 AD 80 AD A0 ED FB " +
            "6C 99 12 5A 75 E8 F4 97 4C 3F AA 25 0C 11 1E 38 " +
            "17 67 FD E6 0C F0 B1 26 B5 F8 E7 E9 62 FC 38 AC " +
            "4A 5A 19 7E 82 59 CF 45 D6 68 24 72 44 DD BB 3E " +
            "B6 EC 47 C8 F5 6D CC CB",
            // After Iota  
            "ED 88 71 5C 29 6B 40 D8 C5 06 12 82 FC 3B 6F 11 " +
            "FC 88 EE 20 FC 5B CC 1C 94 8B 3F 0E 6D 5A 36 2A " +
            "05 48 02 88 B4 31 2F 5C FE 73 1A DE 28 70 31 DC " +
            "26 42 A5 54 A6 D5 7E 3A 76 CB 2A 38 D5 A7 98 2A " +
            "A3 88 AC AC CB 77 0B CE 42 4D 3E 33 44 A4 E1 97 " +
            "A8 5D 0B 2D 3A C4 04 BF 04 D4 92 78 80 8D AD 7C " +
            "61 1A 6A F1 F0 AB 84 A1 71 53 2C C7 29 D7 DF F0 " +
            "5D EA B1 3F 12 51 CF 14 63 4C D5 B8 C1 A8 A6 85 " +
            "79 BD DD A5 6F A3 ED BD F6 61 AD 80 AD A0 ED FB " +
            "6C 99 12 5A 75 E8 F4 97 4C 3F AA 25 0C 11 1E 38 " +
            "17 67 FD E6 0C F0 B1 26 B5 F8 E7 E9 62 FC 38 AC " +
            "4A 5A 19 7E 82 59 CF 45 D6 68 24 72 44 DD BB 3E " +
            "B6 EC 47 C8 F5 6D CC CB",
            // Round #8 
            // After Theta
            "5B 1E 2E F1 9D AA 41 78 A4 4F 6E 5D E6 E1 E8 5A " +
            "2E 1E E2 59 57 F8 FE 21 03 90 E5 CB AD 84 63 5A " +
            "67 F2 1A 27 E7 F1 47 D1 48 E5 45 73 9C B1 30 7C " +
            "47 0B D9 8B BC 0F F9 71 A4 5D 26 41 7E 04 AA 17 " +
            "34 93 76 69 0B A9 5E BE 20 F7 26 9C 17 64 89 1A " +
            "1E CB 54 80 8E 05 05 1F 65 9D EE A7 9A 57 2A 37 " +
            "B3 8C 66 88 5B 08 B6 9C E6 48 F6 02 E9 09 8A 80 " +
            "3F 50 A9 90 41 91 A7 99 D5 DA 8A 15 75 69 A7 25 " +
            "18 F4 A1 7A 75 79 6A F6 24 F7 A1 F9 06 03 DF C6 " +
            "FB 82 C8 9F B5 36 A1 E7 2E 85 B2 8A 5F D1 76 B5 " +
            "A1 F1 A2 4B B8 31 B0 86 D4 B1 9B 36 78 26 BF E7 " +
            "98 CC 15 07 29 FA FD 78 41 73 FE B7 84 03 EE 4E " +
            "D4 56 5F 67 A6 AD A4 46",
            // After Rho
            "5B 1E 2E F1 9D AA 41 78 48 9F DC BA CC C3 D1 B5 " +
            "8B 87 78 D6 15 BE 7F 88 4A 38 A6 35 00 59 BE DC " +
            "8F 3F 8A 3E 93 D7 38 39 C7 19 0B C3 87 54 5E 34 " +
            "BD C8 FB 90 1F 77 B4 90 05 69 97 49 90 1F 81 EA " +
            "49 BB B4 85 54 2F 5F 9A 96 A8 01 72 6F C2 79 41 " +
            "F0 58 A6 02 74 2C 28 F8 DC 94 75 BA 9F 6A 5E A9 " +
            "43 DC 42 B0 E5 9C 65 34 13 14 01 CD 91 EC 05 D2 " +
            "C8 A0 C8 D3 CC 1F A8 54 2B EA D2 4E 4B AA B5 15 " +
            "54 AF 2E 4F CD 1E 83 3E 6F 63 92 FB D0 7C 83 81 " +
            "26 F4 7C 5F 10 F9 B3 D6 B5 2E 85 B2 8A 5F D1 76 " +
            "C0 1A 86 C6 8B 2E E1 C6 53 C7 6E DA E0 99 FC 9E " +
            "93 B9 E2 20 45 BF 1F 0F 73 FE B7 84 03 EE 4E 41 " +
            "A9 11 B5 D5 D7 99 69 2B",
            // After Pi 
            "5B 1E 2E F1 9D AA 41 78 BD C8 FB 90 1F 77 B4 90 " +
            "43 DC 42 B0 E5 9C 65 34 26 F4 7C 5F 10 F9 B3 D6 " +
            "A9 11 B5 D5 D7 99 69 2B 4A 38 A6 35 00 59 BE DC " +
            "96 A8 01 72 6F C2 79 41 F0 58 A6 02 74 2C 28 F8 " +
            "54 AF 2E 4F CD 1E 83 3E 93 B9 E2 20 45 BF 1F 0F " +
            "48 9F DC BA CC C3 D1 B5 05 69 97 49 90 1F 81 EA " +
            "13 14 01 CD 91 EC 05 D2 B5 2E 85 B2 8A 5F D1 76 " +
            "C0 1A 86 C6 8B 2E E1 C6 8F 3F 8A 3E 93 D7 38 39 " +
            "C7 19 0B C3 87 54 5E 34 DC 94 75 BA 9F 6A 5E A9 " +
            "6F 63 92 FB D0 7C 83 81 73 FE B7 84 03 EE 4E 41 " +
            "8B 87 78 D6 15 BE 7F 88 49 BB B4 85 54 2F 5F 9A " +
            "C8 A0 C8 D3 CC 1F A8 54 2B EA D2 4E 4B AA B5 15 " +
            "53 C7 6E DA E0 99 FC 9E",
            // After Chi
            "19 0A 2E D1 7D 22 00 5C 99 E8 C7 DF 0F 16 26 52 " +
            "CA DD C3 30 22 9C 2D 1D 74 FA 76 7F 18 DB B3 86 " +
            "0D D1 64 D5 D5 CC DD AB 2A 68 00 35 10 75 BE 64 " +
            "92 0F 09 3F E6 D0 FA 47 73 48 66 22 74 8D 34 F9 " +
            "1C AF 2A 5A CD 5E 23 EE 07 39 E3 62 2A 3D 5E 0E " +
            "5A 8B DC 3E CD 23 D5 A5 A1 43 13 7B 9A 0C 51 CE " +
            "53 04 03 89 90 CC 25 52 BD AB DD 8A CE 9E C1 47 " +
            "C5 7A 85 87 9B 32 E1 8C 97 BB FE 06 8B FD 38 B0 " +
            "E4 7A 89 82 C7 40 DF 34 CC 08 50 BE 9C E8 12 E9 " +
            "E3 62 9A C1 40 6D B3 B9 33 FE B6 45 07 EE 08 45 " +
            "0B 87 30 84 9D AE DF CC 6A F1 A6 89 57 8F 4A 9B " +
            "98 A5 E4 43 6C 0E E0 DE A3 EA C2 4A 5E 8C B6 15 " +
            "13 FF EA DB A0 98 FC 8C",
            // After Iota  
            "93 0A 2E D1 7D 22 00 5C 99 E8 C7 DF 0F 16 26 52 " +
            "CA DD C3 30 22 9C 2D 1D 74 FA 76 7F 18 DB B3 86 " +
            "0D D1 64 D5 D5 CC DD AB 2A 68 00 35 10 75 BE 64 " +
            "92 0F 09 3F E6 D0 FA 47 73 48 66 22 74 8D 34 F9 " +
            "1C AF 2A 5A CD 5E 23 EE 07 39 E3 62 2A 3D 5E 0E " +
            "5A 8B DC 3E CD 23 D5 A5 A1 43 13 7B 9A 0C 51 CE " +
            "53 04 03 89 90 CC 25 52 BD AB DD 8A CE 9E C1 47 " +
            "C5 7A 85 87 9B 32 E1 8C 97 BB FE 06 8B FD 38 B0 " +
            "E4 7A 89 82 C7 40 DF 34 CC 08 50 BE 9C E8 12 E9 " +
            "E3 62 9A C1 40 6D B3 B9 33 FE B6 45 07 EE 08 45 " +
            "0B 87 30 84 9D AE DF CC 6A F1 A6 89 57 8F 4A 9B " +
            "98 A5 E4 43 6C 0E E0 DE A3 EA C2 4A 5E 8C B6 15 " +
            "13 FF EA DB A0 98 FC 8C",
            // Round #9 
            // After Theta
            "34 C7 94 5E 79 9C A6 54 9B 44 DF 4B D5 47 36 B0 " +
            "C5 1F 83 E9 CB 6D 9C 6F 15 E1 D9 45 A9 8B 50 C6 " +
            "67 0D C4 41 BC 79 91 EB 8D A5 BA BA 14 CB 18 6C " +
            "90 A3 11 AB 3C 81 EA A5 7C 8A 26 FB 9D 7C 85 8B " +
            "7D B4 85 60 7C 0E C0 AE 6D E5 43 F6 43 88 12 4E " +
            "FD 46 66 B1 C9 9D 73 AD A3 EF 0B EF 40 5D 41 2C " +
            "5C C6 43 50 79 3D 94 20 DC B0 72 B0 7F CE 22 07 " +
            "AF A6 25 13 F2 87 AD CC 30 76 44 89 8F 43 9E B8 " +
            "E6 D6 91 16 1D 11 CF D6 C3 CA 10 67 75 19 A3 9B " +
            "82 79 35 FB F1 3D 50 F9 59 22 16 D1 6E 5B 44 05 " +
            "AC 4A 8A 0B 99 10 79 C4 68 5D BE 1D 8D DE 5A 79 " +
            "97 67 A4 9A 85 FF 51 AC C2 F1 6D 70 EF DC 55 55 " +
            "79 23 4A 4F C9 2D B0 CC",
            // After Rho
            "34 C7 94 5E 79 9C A6 54 37 89 BE 97 AA 8F 6C 60 " +
            "F1 C7 60 FA 72 1B E7 5B BA 08 65 5C 11 9E 5D 94 " +
            "CD 8B 5C 3F 6B 20 0E E2 4B B1 8C C1 D6 58 AA AB " +
            "B1 CA 13 A8 5E 0A 39 1A 22 9F A2 C9 7E 27 5F E1 " +
            "DA 42 30 3E 07 60 D7 3E 28 E1 D4 56 3E 64 3F 84 " +
            "ED 37 32 8B 4D EE 9C 6B B1 8C BE 2F BC 03 75 05 " +
            "82 CA EB A1 04 E1 32 1E 9C 45 0E B8 61 E5 60 FF " +
            "09 F9 C3 56 E6 57 D3 92 12 1F 87 3C 71 61 EC 88 " +
            "D2 A2 23 E2 D9 DA DC 3A D1 CD 61 65 88 B3 BA 8C " +
            "07 2A 5F 30 AF 66 3F BE 05 59 22 16 D1 6E 5B 44 " +
            "E4 11 B3 2A 29 2E 64 42 A1 75 F9 76 34 7A 6B E5 " +
            "F2 8C 54 B3 F0 3F 8A F5 F1 6D 70 EF DC 55 55 C2 " +
            "2C 73 DE 88 D2 53 72 0B",
            // After Pi 
            "34 C7 94 5E 79 9C A6 54 B1 CA 13 A8 5E 0A 39 1A " +
            "82 CA EB A1 04 E1 32 1E 07 2A 5F 30 AF 66 3F BE " +
            "2C 73 DE 88 D2 53 72 0B BA 08 65 5C 11 9E 5D 94 " +
            "28 E1 D4 56 3E 64 3F 84 ED 37 32 8B 4D EE 9C 6B " +
            "D2 A2 23 E2 D9 DA DC 3A F2 8C 54 B3 F0 3F 8A F5 " +
            "37 89 BE 97 AA 8F 6C 60 22 9F A2 C9 7E 27 5F E1 " +
            "9C 45 0E B8 61 E5 60 FF 05 59 22 16 D1 6E 5B 44 " +
            "E4 11 B3 2A 29 2E 64 42 CD 8B 5C 3F 6B 20 0E E2 " +
            "4B B1 8C C1 D6 58 AA AB B1 8C BE 2F BC 03 75 05 " +
            "D1 CD 61 65 88 B3 BA 8C F1 6D 70 EF DC 55 55 C2 " +
            "F1 C7 60 FA 72 1B E7 5B DA 42 30 3E 07 60 D7 3E " +
            "09 F9 C3 56 E6 57 D3 92 12 1F 87 3C 71 61 EC 88 " +
            "A1 75 F9 76 34 7A 6B E5",
            // After Chi
            "36 C7 7C 5F 79 7D A4 50 B4 EA 07 B8 F5 0C 34 BA " +
            "AA 9B 6B 29 54 F0 72 1F 17 AE 5F 66 86 EA BB EA " +
            "AD 7B DD 28 D4 51 6B 01 7F 1E 47 D5 50 14 DD FF " +
            "3A 61 D5 36 AE 74 7F 94 CD 3B 66 9A 6D CB 9E AE " +
            "DA A2 02 AE D8 5A 89 3A F2 6D C4 B1 DE 5F A8 F5 " +
            "AB C9 B2 A7 AB 4F 4C 7E 23 87 82 CF EE 2D 44 E1 " +
            "7C 45 9F 90 49 E5 44 FD 16 D1 2E 83 53 EF 53 64 " +
            "E4 07 B3 62 7D 0E 77 C3 7D 87 6E 11 43 23 5B E6 " +
            "0B F0 CD 81 D6 E8 20 23 91 AC AE A5 E8 47 30 47 " +
            "DD 4F 6D 75 AB 93 B0 AC F3 5D F0 2F 48 0D F5 CB " +
            "F0 7E A3 BA 92 0C E7 DB C8 44 34 16 16 40 FB 36 " +
            "A8 99 BB 14 E2 4D D0 F7 42 9D 87 B4 33 60 68 92 " +
            "AB 75 E9 72 31 1A 7B C1",
            // After Iota  
            "BE C7 7C 5F 79 7D A4 50 B4 EA 07 B8 F5 0C 34 BA " +
            "AA 9B 6B 29 54 F0 72 1F 17 AE 5F 66 86 EA BB EA " +
            "AD 7B DD 28 D4 51 6B 01 7F 1E 47 D5 50 14 DD FF " +
            "3A 61 D5 36 AE 74 7F 94 CD 3B 66 9A 6D CB 9E AE " +
            "DA A2 02 AE D8 5A 89 3A F2 6D C4 B1 DE 5F A8 F5 " +
            "AB C9 B2 A7 AB 4F 4C 7E 23 87 82 CF EE 2D 44 E1 " +
            "7C 45 9F 90 49 E5 44 FD 16 D1 2E 83 53 EF 53 64 " +
            "E4 07 B3 62 7D 0E 77 C3 7D 87 6E 11 43 23 5B E6 " +
            "0B F0 CD 81 D6 E8 20 23 91 AC AE A5 E8 47 30 47 " +
            "DD 4F 6D 75 AB 93 B0 AC F3 5D F0 2F 48 0D F5 CB " +
            "F0 7E A3 BA 92 0C E7 DB C8 44 34 16 16 40 FB 36 " +
            "A8 99 BB 14 E2 4D D0 F7 42 9D 87 B4 33 60 68 92 " +
            "AB 75 E9 72 31 1A 7B C1",
            // Round #10
            // After Theta
            "80 8E 9C 54 9C 90 37 D8 16 A3 4C 1B 53 AD 2C AE " +
            "4D 3D F0 EA 0A 54 D5 D0 F3 0D BE B9 E1 10 87 6C " +
            "26 A7 CD AE E6 EF C0 52 41 57 A7 DE B5 F9 4E 77 " +
            "98 28 9E 95 08 D5 67 80 2A 9D FD 59 33 6F 39 61 " +
            "3E 01 E3 71 BF A0 B5 BC 79 B1 D4 37 EC E1 03 A6 " +
            "95 80 52 AC 4E A2 DF F6 81 CE C9 6C 48 8C 5C F5 " +
            "9B E3 04 53 17 41 E3 32 F2 72 CF 5C 34 15 6F E2 " +
            "6F DB A3 E4 4F B0 DC 90 43 CE 8E 1A A6 CE C8 6E " +
            "A9 B9 86 22 70 49 38 37 76 0A 35 66 B6 E3 97 88 " +
            "39 EC 8C AA CC 69 8C 2A 78 81 E0 A9 7A B3 5E 98 " +
            "CE 37 43 B1 77 E1 74 53 6A 0D 7F B5 B0 E1 E3 22 " +
            "4F 3F 20 D7 BC E9 77 38 A6 3E 66 6B 54 9A 54 14 " +
            "20 A9 F9 F4 03 A4 D0 92",
            // After Rho
            "80 8E 9C 54 9C 90 37 D8 2D 46 99 36 A6 5A 59 5C " +
            "53 0F BC BA 02 55 35 74 0E 71 C8 36 DF E0 9B 1B " +
            "7F 07 96 32 39 6D 76 35 5D 9B EF 74 17 74 75 EA " +
            "59 89 50 7D 06 88 89 E2 98 4A 67 7F D6 CC 5B 4E " +
            "80 F1 B8 5F D0 5A 5E 9F 3E 60 9A 17 4B 7D C3 1E " +
            "AF 04 94 62 75 12 FD B6 D5 07 3A 27 B3 21 31 72 " +
            "98 BA 08 1A 97 D9 1C 27 2A DE C4 E5 E5 9E B9 68 " +
            "F2 27 58 6E C8 B7 ED 51 35 4C 9D 91 DD 86 9C 1D " +
            "50 04 2E 09 E7 26 35 D7 4B 44 3B 85 1A 33 DB F1 " +
            "8D 51 25 87 9D 51 95 39 98 78 81 E0 A9 7A B3 5E " +
            "D3 4D 39 DF 0C C5 DE 85 A8 35 FC D5 C2 86 8F 8B " +
            "E9 07 E4 9A 37 FD 0E E7 3E 66 6B 54 9A 54 14 A6 " +
            "B4 24 48 6A 3E FD 00 29",
            // After Pi 
            "80 8E 9C 54 9C 90 37 D8 59 89 50 7D 06 88 89 E2 " +
            "98 BA 08 1A 97 D9 1C 27 8D 51 25 87 9D 51 95 39 " +
            "B4 24 48 6A 3E FD 00 29 0E 71 C8 36 DF E0 9B 1B " +
            "3E 60 9A 17 4B 7D C3 1E AF 04 94 62 75 12 FD B6 " +
            "50 04 2E 09 E7 26 35 D7 E9 07 E4 9A 37 FD 0E E7 " +
            "2D 46 99 36 A6 5A 59 5C 98 4A 67 7F D6 CC 5B 4E " +
            "2A DE C4 E5 E5 9E B9 68 98 78 81 E0 A9 7A B3 5E " +
            "D3 4D 39 DF 0C C5 DE 85 7F 07 96 32 39 6D 76 35 " +
            "5D 9B EF 74 17 74 75 EA D5 07 3A 27 B3 21 31 72 " +
            "4B 44 3B 85 1A 33 DB F1 3E 66 6B 54 9A 54 14 A6 " +
            "53 0F BC BA 02 55 35 74 80 F1 B8 5F D0 5A 5E 9F " +
            "F2 27 58 6E C8 B7 ED 51 35 4C 9D 91 DD 86 9C 1D " +
            "A8 35 FC D5 C2 86 8F 8B",
            // After Chi
            "00 BC 94 56 0D C1 23 DD 5C C8 75 F8 0E 88 08 FA " +
            "A8 9E 40 72 B5 75 1C 27 8D DB B1 93 1D 51 A2 E9 " +
            "ED 25 08 43 3C F5 88 0B 8F 75 CC 56 EB E2 A7 BB " +
            "6E 60 B0 1E C9 59 C3 5F 06 07 54 F0 65 CB F7 96 " +
            "56 74 26 2D 2F 26 A4 CF D9 07 F6 9B 37 E0 4E E3 " +
            "0F D2 19 B6 87 48 F9 7C 08 6A 66 7F DE AC 59 58 " +
            "69 DB FC FA E1 1B F5 E9 B4 7A 01 C0 0B 60 B2 06 " +
            "43 45 5F 96 5C 41 DC 87 FF 03 86 31 99 6C 76 25 " +
            "57 DB EE F4 1F 66 BF 6B E1 25 7A 77 33 65 35 74 " +
            "0A 45 AF A7 3B 1A B9 E0 3E FE 02 10 9C 44 15 6C " +
            "21 09 FC 9A 0A F0 94 34 85 B9 3D CE C5 5A 4E 93 " +
            "7A 16 38 2A CA B7 EE D3 66 46 9D BB DD D7 AC 69 " +
            "28 C5 FC 90 12 8C C5 00",
            // After Iota  
            "09 3C 94 D6 0D C1 23 DD 5C C8 75 F8 0E 88 08 FA " +
            "A8 9E 40 72 B5 75 1C 27 8D DB B1 93 1D 51 A2 E9 " +
            "ED 25 08 43 3C F5 88 0B 8F 75 CC 56 EB E2 A7 BB " +
            "6E 60 B0 1E C9 59 C3 5F 06 07 54 F0 65 CB F7 96 " +
            "56 74 26 2D 2F 26 A4 CF D9 07 F6 9B 37 E0 4E E3 " +
            "0F D2 19 B6 87 48 F9 7C 08 6A 66 7F DE AC 59 58 " +
            "69 DB FC FA E1 1B F5 E9 B4 7A 01 C0 0B 60 B2 06 " +
            "43 45 5F 96 5C 41 DC 87 FF 03 86 31 99 6C 76 25 " +
            "57 DB EE F4 1F 66 BF 6B E1 25 7A 77 33 65 35 74 " +
            "0A 45 AF A7 3B 1A B9 E0 3E FE 02 10 9C 44 15 6C " +
            "21 09 FC 9A 0A F0 94 34 85 B9 3D CE C5 5A 4E 93 " +
            "7A 16 38 2A CA B7 EE D3 66 46 9D BB DD D7 AC 69 " +
            "28 C5 FC 90 12 8C C5 00",
            // Round #11
            // After Theta
            "B8 21 2A 5E 53 DE 2F D4 B2 BB 1A 2E 6C 90 1D 0E " +
            "47 92 79 14 C8 81 3C 71 13 12 A5 2A 66 1F F2 11 " +
            "40 D1 DB 1B 06 C0 16 B5 3E 68 72 DE B5 FD AB B2 " +
            "80 13 DF C8 AB 41 D6 AB E9 0B 6D 96 18 3F D7 C0 " +
            "C8 BD 32 94 54 68 F4 37 74 F3 25 C3 0D D5 D0 5D " +
            "BE CF A7 3E D9 57 F5 75 E6 19 09 A9 BC B4 4C AC " +
            "86 D7 C5 9C 9C EF D5 BF 2A B3 15 79 70 2E E2 FE " +
            "EE B1 8C CE 66 74 42 39 4E 1E 38 B9 C7 73 7A 2C " +
            "B9 A8 81 22 7D 7E AA 9F 0E 29 43 11 4E 91 15 22 " +
            "94 8C BB 1E 40 54 E9 18 93 0A D1 48 A6 71 8B D2 " +
            "90 14 42 12 54 EF 98 3D 6B CA 52 18 A7 42 5B 67 " +
            "95 1A 01 4C B7 43 CE 85 F8 8F 89 02 A6 99 FC 91 " +
            "85 31 2F C8 28 B9 5B BE",
            // After Rho
            "B8 21 2A 5E 53 DE 2F D4 64 77 35 5C D8 20 3B 1C " +
            "91 64 1E 05 72 20 4F DC F6 21 1F 31 21 51 AA 62 " +
            "00 B6 A8 05 8A DE DE 30 5D DB BF 2A EB 83 26 E7 " +
            "8D BC 1A 64 BD 0A 38 F1 70 FA 42 9B 25 C6 CF 35 " +
            "5E 19 4A 2A 34 FA 1B E4 0D DD 45 37 5F 32 DC 50 " +
            "F3 7D 3E F5 C9 BE AA AF B1 9A 67 24 A4 F2 D2 32 " +
            "E6 E4 7C AF FE 35 BC 2E 5C C4 FD 55 66 2B F2 E0 " +
            "67 33 3A A1 1C F7 58 46 72 8F E7 F4 58 9C 3C 70 " +
            "50 A4 CF 4F F5 33 17 35 0A 11 87 94 A1 08 A7 C8 " +
            "2A 1D 83 92 71 D7 03 88 D2 93 0A D1 48 A6 71 8B " +
            "63 F6 40 52 08 49 50 BD AD 29 4B 61 9C 0A 6D 9D " +
            "52 23 80 E9 76 C8 B9 B0 8F 89 02 A6 99 FC 91 F8 " +
            "96 6F 61 CC 0B 32 4A EE",
            // After Pi 
            "B8 21 2A 5E 53 DE 2F D4 8D BC 1A 64 BD 0A 38 F1 " +
            "E6 E4 7C AF FE 35 BC 2E 2A 1D 83 92 71 D7 03 88 " +
            "96 6F 61 CC 0B 32 4A EE F6 21 1F 31 21 51 AA 62 " +
            "0D DD 45 37 5F 32 DC 50 F3 7D 3E F5 C9 BE AA AF " +
            "50 A4 CF 4F F5 33 17 35 52 23 80 E9 76 C8 B9 B0 " +
            "64 77 35 5C D8 20 3B 1C 70 FA 42 9B 25 C6 CF 35 " +
            "5C C4 FD 55 66 2B F2 E0 D2 93 0A D1 48 A6 71 8B " +
            "63 F6 40 52 08 49 50 BD 00 B6 A8 05 8A DE DE 30 " +
            "5D DB BF 2A EB 83 26 E7 B1 9A 67 24 A4 F2 D2 32 " +
            "0A 11 87 94 A1 08 A7 C8 8F 89 02 A6 99 FC 91 F8 " +
            "91 64 1E 05 72 20 4F DC 5E 19 4A 2A 34 FA 1B E4 " +
            "67 33 3A A1 1C F7 58 46 72 8F E7 F4 58 9C 3C 70 " +
            "AD 29 4B 61 9C 0A 6D 9D",
            // After Chi
            "DA 61 4E D5 11 EB AB DA 85 A5 99 74 BC C8 3B 71 " +
            "72 86 1C E3 F4 15 F4 48 02 1D 89 80 21 1B 26 98 " +
            "93 F3 71 EC A7 32 5A CF 04 01 25 F1 A1 DD 88 CD " +
            "0D 5D 84 3D 6B 33 C9 40 F1 7E 3E 55 CB 76 02 2F " +
            "F4 A4 D0 5F F4 22 15 77 5B FF C0 EF 28 EA ED A0 " +
            "68 73 88 18 9A 09 0B DC F2 E9 40 1B 2D 42 CE 3E " +
            "7D A0 BD 57 66 62 F2 D4 D6 92 3F DD 98 86 5A 8B " +
            "73 7E 02 D1 2D 8F 94 9C A0 B6 E8 01 8E AE 0E 20 " +
            "57 DA 3F BA EA 8B 03 2F 34 12 67 06 BC 06 C2 02 " +
            "0A 27 2F 95 A3 0A E9 C8 D2 C0 15 8C F8 FD B1 3F " +
            "B0 46 2E 84 7A 25 0F DE 4E 95 8F 7E 74 F2 3F D4 " +
            "EA 13 32 A0 98 F5 19 CB 62 CB F3 F0 3A BC 3E 30 " +
            "E3 30 0B 4B 98 D0 7D BD",
            // After Iota  
            "D0 61 4E 55 11 EB AB DA 85 A5 99 74 BC C8 3B 71 " +
            "72 86 1C E3 F4 15 F4 48 02 1D 89 80 21 1B 26 98 " +
            "93 F3 71 EC A7 32 5A CF 04 01 25 F1 A1 DD 88 CD " +
            "0D 5D 84 3D 6B 33 C9 40 F1 7E 3E 55 CB 76 02 2F " +
            "F4 A4 D0 5F F4 22 15 77 5B FF C0 EF 28 EA ED A0 " +
            "68 73 88 18 9A 09 0B DC F2 E9 40 1B 2D 42 CE 3E " +
            "7D A0 BD 57 66 62 F2 D4 D6 92 3F DD 98 86 5A 8B " +
            "73 7E 02 D1 2D 8F 94 9C A0 B6 E8 01 8E AE 0E 20 " +
            "57 DA 3F BA EA 8B 03 2F 34 12 67 06 BC 06 C2 02 " +
            "0A 27 2F 95 A3 0A E9 C8 D2 C0 15 8C F8 FD B1 3F " +
            "B0 46 2E 84 7A 25 0F DE 4E 95 8F 7E 74 F2 3F D4 " +
            "EA 13 32 A0 98 F5 19 CB 62 CB F3 F0 3A BC 3E 30 " +
            "E3 30 0B 4B 98 D0 7D BD",
            // Round #12
            // After Theta
            "9D 5F 39 6D 1A 11 45 43 69 F4 28 C2 98 98 AD B1 " +
            "80 56 84 BA 38 C6 88 85 36 41 18 EC D8 1C 27 01 " +
            "83 F3 80 F9 CF 52 B7 39 49 3F 52 C9 AA 27 66 54 " +
            "E1 0C 35 8B 4F 63 5F 80 03 AE A6 0C 07 A5 7E E2 " +
            "C0 F8 41 33 0D 25 14 EE 4B FF 31 FA 40 8A 00 56 " +
            "25 4D FF 20 91 F3 E5 45 1E B8 F1 AD 09 12 58 FE " +
            "8F 70 25 0E AA B1 8E 19 E2 CE AE B1 61 81 5B 12 " +
            "63 7E F3 C4 45 EF 79 6A ED 88 9F 39 85 54 E0 B9 " +
            "BB 8B 8E 0C CE DB 95 EF C6 C2 FF 5F 70 D5 BE CF " +
            "3E 7B BE F9 5A 0D E8 51 C2 C0 E4 99 90 9D 5C C9 " +
            "FD 78 59 BC 71 DF E1 47 A2 C4 3E C8 50 A2 A9 14 " +
            "18 C3 AA F9 54 26 65 06 56 97 62 9C C3 BB 3F A9 " +
            "F3 30 FA 5E F0 B0 90 4B",
            // After Rho
            "9D 5F 39 6D 1A 11 45 43 D3 E8 51 84 31 31 5B 63 " +
            "A0 15 A1 2E 8E 31 62 21 CD 71 12 60 13 84 C1 8E " +
            "96 BA CD 19 9C 07 CC 7F AC 7A 62 46 95 F4 23 95 " +
            "B3 F8 34 F6 05 18 CE 50 F8 80 AB 29 C3 41 A9 9F " +
            "FC A0 99 86 12 0A 77 60 08 60 B5 F4 1F A3 0F A4 " +
            "2A 69 FA 07 89 9C 2F 2F F9 7B E0 C6 B7 26 48 60 " +
            "71 50 8D 75 CC 78 84 2B 02 B7 24 C4 9D 5D 63 C3 " +
            "E2 A2 F7 3C B5 31 BF 79 73 0A A9 C0 73 DB 11 3F " +
            "91 C1 79 BB F2 7D 77 D1 DF 67 63 E1 FF 2F B8 6A " +
            "01 3D CA 67 CF 37 5F AB C9 C2 C0 E4 99 90 9D 5C " +
            "87 1F F5 E3 65 F1 C6 7D 88 12 FB 20 43 89 A6 52 " +
            "63 58 35 9F CA A4 CC 00 97 62 9C C3 BB 3F A9 56 " +
            "E4 D2 3C 8C BE 17 3C 2C",
            // After Pi 
            "9D 5F 39 6D 1A 11 45 43 B3 F8 34 F6 05 18 CE 50 " +
            "71 50 8D 75 CC 78 84 2B 01 3D CA 67 CF 37 5F AB " +
            "E4 D2 3C 8C BE 17 3C 2C CD 71 12 60 13 84 C1 8E " +
            "08 60 B5 F4 1F A3 0F A4 2A 69 FA 07 89 9C 2F 2F " +
            "91 C1 79 BB F2 7D 77 D1 63 58 35 9F CA A4 CC 00 " +
            "D3 E8 51 84 31 31 5B 63 F8 80 AB 29 C3 41 A9 9F " +
            "02 B7 24 C4 9D 5D 63 C3 C9 C2 C0 E4 99 90 9D 5C " +
            "87 1F F5 E3 65 F1 C6 7D 96 BA CD 19 9C 07 CC 7F " +
            "AC 7A 62 46 95 F4 23 95 F9 7B E0 C6 B7 26 48 60 " +
            "DF 67 63 E1 FF 2F B8 6A 97 62 9C C3 BB 3F A9 56 " +
            "A0 15 A1 2E 8E 31 62 21 FC A0 99 86 12 0A 77 60 " +
            "E2 A2 F7 3C B5 31 BF 79 73 0A A9 C0 73 DB 11 3F " +
            "88 12 FB 20 43 89 A6 52",
            // After Chi
            "DD 5F B0 6C D2 71 45 68 B3 D5 76 F4 06 1F 95 D0 " +
            "95 92 B9 FD FC 78 A4 2F 18 30 CB 06 CF 37 1E E8 " +
            "C6 72 38 1E BB 1F B6 3C EF 78 58 63 93 98 E1 85 " +
            "99 E0 B4 4C 6D C2 5F 74 48 71 FE 03 81 1C A7 2F " +
            "1D E0 7B DB E3 7D 76 5F 63 58 90 0B C6 87 C2 20 " +
            "D1 DF 55 40 2D 2D 19 23 31 C0 6B 09 C3 C1 35 83 " +
            "04 AA 11 C7 F9 3C 21 E2 99 22 C0 E0 89 90 84 5E " +
            "AF 1F 5F CA A7 B1 66 E1 C7 BB 4D 99 BE 05 84 1F " +
            "AA 7E 61 67 DD FD 93 9F F9 7B 7C C4 B7 36 49 74 " +
            "DF FF 22 F9 FB 2F FC 43 BF 22 BE 85 BA CF 8A D6 " +
            "A2 17 C7 16 2B 00 EA 38 ED A8 91 46 50 C0 77 66 " +
            "6A B2 A5 1C B5 31 19 39 53 0F A9 CE FF EB 51 1E " +
            "D4 B2 E3 A0 53 83 B3 12",
            // After Iota  
            "56 DF B0 EC D2 71 45 68 B3 D5 76 F4 06 1F 95 D0 " +
            "95 92 B9 FD FC 78 A4 2F 18 30 CB 06 CF 37 1E E8 " +
            "C6 72 38 1E BB 1F B6 3C EF 78 58 63 93 98 E1 85 " +
            "99 E0 B4 4C 6D C2 5F 74 48 71 FE 03 81 1C A7 2F " +
            "1D E0 7B DB E3 7D 76 5F 63 58 90 0B C6 87 C2 20 " +
            "D1 DF 55 40 2D 2D 19 23 31 C0 6B 09 C3 C1 35 83 " +
            "04 AA 11 C7 F9 3C 21 E2 99 22 C0 E0 89 90 84 5E " +
            "AF 1F 5F CA A7 B1 66 E1 C7 BB 4D 99 BE 05 84 1F " +
            "AA 7E 61 67 DD FD 93 9F F9 7B 7C C4 B7 36 49 74 " +
            "DF FF 22 F9 FB 2F FC 43 BF 22 BE 85 BA CF 8A D6 " +
            "A2 17 C7 16 2B 00 EA 38 ED A8 91 46 50 C0 77 66 " +
            "6A B2 A5 1C B5 31 19 39 53 0F A9 CE FF EB 51 1E " +
            "D4 B2 E3 A0 53 83 B3 12",
            // Round #13
            // After Theta
            "8E 3C A8 36 AA 56 58 ED 2B 01 5E 77 F2 61 A2 67 " +
            "E8 B5 16 78 9B 64 3D 99 90 FA 11 12 2E A2 3A 35 " +
            "CD D8 AC 94 E8 82 50 5B 37 9B 40 B9 EB BF FC 00 " +
            "01 34 9C CF 99 BC 68 C3 35 56 51 86 E6 00 3E 99 " +
            "95 2A A1 CF 02 E8 52 82 68 F2 04 81 95 1A 24 47 " +
            "09 3C 4D 9A 55 0A 04 A6 A9 14 43 8A 37 BF 02 34 " +
            "79 8D BE 42 9E 20 B8 54 11 E8 1A F4 68 05 A0 83 " +
            "A4 B5 CB 40 F4 2C 80 86 1F 58 55 43 C6 22 99 9A " +
            "32 AA 49 E4 29 83 A4 28 84 5C D3 41 D0 2A D0 C2 " +
            "57 35 F8 ED 1A BA D8 9E B4 88 2A 0F E9 52 6C B1 " +
            "7A F4 DF CC 53 27 F7 BD 75 7C B9 C5 A4 BE 40 D1 " +
            "17 95 0A 99 D2 2D 80 8F DB C5 73 DA 1E 7E 75 C3 " +
            "DF 18 77 2A 00 1E 55 75",
            // After Rho
            "8E 3C A8 36 AA 56 58 ED 56 02 BC EE E4 C3 44 CF " +
            "7A AD 05 DE 26 59 4F 26 22 AA 53 03 A9 1F 21 E1 " +
            "17 84 DA 6A C6 66 A5 44 BB FE CB 0F 70 B3 09 94 " +
            "F9 9C C9 8B 36 1C 40 C3 66 8D 55 94 A1 39 80 4F " +
            "95 D0 67 01 74 29 C1 4A 41 72 84 26 4F 10 58 A9 " +
            "4D E0 69 D2 AC 52 20 30 D0 A4 52 0C 29 DE FC 0A " +
            "15 F2 04 C1 A5 CA 6B F4 0A 40 07 23 D0 35 E8 D1 " +
            "20 7A 16 40 43 D2 DA 65 86 8C 45 32 35 3F B0 AA " +
            "89 3C 65 90 14 45 46 35 68 61 42 AE E9 20 68 15 " +
            "17 DB F3 AA 06 BF 5D 43 B1 B4 88 2A 0F E9 52 6C " +
            "DC F7 EA D1 7F 33 4F 9D D7 F1 E5 16 93 FA 02 45 " +
            "A2 52 21 53 BA 05 F0 F1 C5 73 DA 1E 7E 75 C3 DB " +
            "55 DD 37 C6 9D 0A 80 47",
            // After Pi 
            "8E 3C A8 36 AA 56 58 ED F9 9C C9 8B 36 1C 40 C3 " +
            "15 F2 04 C1 A5 CA 6B F4 17 DB F3 AA 06 BF 5D 43 " +
            "55 DD 37 C6 9D 0A 80 47 22 AA 53 03 A9 1F 21 E1 " +
            "41 72 84 26 4F 10 58 A9 4D E0 69 D2 AC 52 20 30 " +
            "89 3C 65 90 14 45 46 35 A2 52 21 53 BA 05 F0 F1 " +
            "56 02 BC EE E4 C3 44 CF 66 8D 55 94 A1 39 80 4F " +
            "0A 40 07 23 D0 35 E8 D1 B1 B4 88 2A 0F E9 52 6C " +
            "DC F7 EA D1 7F 33 4F 9D 17 84 DA 6A C6 66 A5 44 " +
            "BB FE CB 0F 70 B3 09 94 D0 A4 52 0C 29 DE FC 0A " +
            "68 61 42 AE E9 20 68 15 C5 73 DA 1E 7E 75 C3 DB " +
            "7A AD 05 DE 26 59 4F 26 95 D0 67 01 74 29 C1 4A " +
            "20 7A 16 40 43 D2 DA 65 86 8C 45 32 35 3F B0 AA " +
            "D7 F1 E5 16 93 FA 02 45",
            // After Chi
            "8A 5E AC 76 2B 94 73 D9 FB 95 3A A1 34 29 54 C0 " +
            "55 F6 00 85 3C CA EB F0 9D FB 7B 9A 24 EB 05 EB " +
            "24 5D 76 4F 89 02 80 45 2E 2A 3A D3 09 5D 01 F1 " +
            "C1 6E 80 26 5F 15 1E AC 6F A2 69 91 06 52 90 F0 " +
            "89 94 37 90 15 5F 47 35 E3 02 A5 77 FC 05 A8 F9 " +
            "5E 42 BE CD B4 C7 2C 5F D7 39 DD 9C AE F1 92 63 " +
            "46 03 65 F2 A0 27 E5 40 B3 B4 9C 04 8F 29 52 2E " +
            "FC 7A AB C1 7E 0B CF 9D 57 84 CA 6A CF 2A 51 4E " +
            "93 BF CB AD B0 93 09 81 55 B6 CA 1C 3F 8B 7F C0 " +
            "7A E5 42 CE 69 22 4C 11 6D 09 DB 1B 4E E4 CB 4B " +
            "5A 87 15 9E 25 8B 55 03 13 54 26 33 40 04 E1 C0 " +
            "71 0B B6 44 C1 12 D8 20 AE 80 45 FA 11 3E FD 88 " +
            "52 A1 87 17 C3 DA 82 0D",
            // After Iota  
            "01 5E AC 76 2B 94 73 59 FB 95 3A A1 34 29 54 C0 " +
            "55 F6 00 85 3C CA EB F0 9D FB 7B 9A 24 EB 05 EB " +
            "24 5D 76 4F 89 02 80 45 2E 2A 3A D3 09 5D 01 F1 " +
            "C1 6E 80 26 5F 15 1E AC 6F A2 69 91 06 52 90 F0 " +
            "89 94 37 90 15 5F 47 35 E3 02 A5 77 FC 05 A8 F9 " +
            "5E 42 BE CD B4 C7 2C 5F D7 39 DD 9C AE F1 92 63 " +
            "46 03 65 F2 A0 27 E5 40 B3 B4 9C 04 8F 29 52 2E " +
            "FC 7A AB C1 7E 0B CF 9D 57 84 CA 6A CF 2A 51 4E " +
            "93 BF CB AD B0 93 09 81 55 B6 CA 1C 3F 8B 7F C0 " +
            "7A E5 42 CE 69 22 4C 11 6D 09 DB 1B 4E E4 CB 4B " +
            "5A 87 15 9E 25 8B 55 03 13 54 26 33 40 04 E1 C0 " +
            "71 0B B6 44 C1 12 D8 20 AE 80 45 FA 11 3E FD 88 " +
            "52 A1 87 17 C3 DA 82 0D",
            // Round #14
            // After Theta
            "DF 81 9C 88 C6 12 BD A2 36 74 2C 41 81 CA 7C 3A " +
            "DE A3 25 75 85 93 98 6D CD 0B 42 CE 4D A8 60 84 " +
            "AE 89 4F 4C B6 DD 94 58 F0 F5 0A 2D E4 DB CF 0A " +
            "0C 8F 96 C6 EA F6 36 56 E4 F7 4C 61 BF 0B E3 6D " +
            "D9 64 0E C4 7C 1C 22 5A 69 D6 9C 74 C3 DA BC E4 " +
            "80 9D 8E 33 59 41 E2 A4 1A D8 CB 7C 1B 12 BA 99 " +
            "CD 56 40 02 19 7E 96 DD E3 44 A5 50 E6 6A 37 41 " +
            "76 AE 92 C2 41 D4 DB 80 89 5B FA 94 22 AC 9F B5 " +
            "5E 5E DD 4D 05 70 21 7B DE E3 EF EC 86 D2 0C 5D " +
            "2A 15 7B 9A 00 61 29 7E E7 DD E2 18 71 3B DF 56 " +
            "84 58 25 60 C8 0D 9B F8 DE B5 30 D3 F5 E7 C9 3A " +
            "FA 5E 93 B4 78 4B AB BD FE 70 7C AE 78 7D 98 E7 " +
            "D8 75 BE 14 FC 05 96 10",
            // After Rho
            "DF 81 9C 88 C6 12 BD A2 6C E8 58 82 02 95 F9 74 " +
            "F7 68 49 5D E1 24 66 9B 84 0A 46 D8 BC 20 E4 DC " +
            "ED A6 C4 72 4D 7C 62 B2 42 BE FD AC 00 5F AF D0 " +
            "69 AC 6E 6F 63 C5 F0 68 1B F9 3D 53 D8 EF C2 78 " +
            "32 07 62 3E 0E 11 AD 6C CD 4B 9E 66 CD 49 37 AC " +
            "05 EC 74 9C C9 0A 12 27 66 6A 60 2F F3 6D 48 E8 " +
            "12 C8 F0 B3 EC 6E B6 02 D5 6E 82 C6 89 4A A1 CC " +
            "E1 20 EA 6D 40 3B 57 49 29 45 58 3F 6B 13 B7 F4 " +
            "BB A9 00 2E 64 CF CB AB 86 2E EF F1 77 76 43 69 " +
            "2C C5 4F A5 62 4F 13 20 56 E7 DD E2 18 71 3B DF " +
            "6C E2 13 62 95 80 21 37 78 D7 C2 4C D7 9F 27 EB " +
            "DF 6B 92 16 6F 69 B5 57 70 7C AE 78 7D 98 E7 FE " +
            "25 04 76 9D 2F 05 7F 81",
            // After Pi 
            "DF 81 9C 88 C6 12 BD A2 69 AC 6E 6F 63 C5 F0 68 " +
            "12 C8 F0 B3 EC 6E B6 02 2C C5 4F A5 62 4F 13 20 " +
            "25 04 76 9D 2F 05 7F 81 84 0A 46 D8 BC 20 E4 DC " +
            "CD 4B 9E 66 CD 49 37 AC 05 EC 74 9C C9 0A 12 27 " +
            "BB A9 00 2E 64 CF CB AB DF 6B 92 16 6F 69 B5 57 " +
            "6C E8 58 82 02 95 F9 74 1B F9 3D 53 D8 EF C2 78 " +
            "D5 6E 82 C6 89 4A A1 CC 56 E7 DD E2 18 71 3B DF " +
            "6C E2 13 62 95 80 21 37 ED A6 C4 72 4D 7C 62 B2 " +
            "42 BE FD AC 00 5F AF D0 66 6A 60 2F F3 6D 48 E8 " +
            "86 2E EF F1 77 76 43 69 70 7C AE 78 7D 98 E7 FE " +
            "F7 68 49 5D E1 24 66 9B 32 07 62 3E 0E 11 AD 6C " +
            "E1 20 EA 6D 40 3B 57 49 29 45 58 3F 6B 13 B7 F4 " +
            "78 D7 C2 4C D7 9F 27 EB",
            // After Chi
            "CD C1 0C 18 4A 38 BB A0 45 A9 61 6B 61 C4 F1 48 " +
            "13 C8 C0 AB E1 6E DA 83 F6 44 C7 A5 A2 5D 93 02 " +
            "05 28 14 FA 0E C0 3F C9 84 AE 26 40 BC 22 E4 DF " +
            "77 4A 9E 44 E9 8C FE 24 41 AE E6 8C C2 2A 26 73 " +
            "BB A9 44 E6 F4 CF 8B 23 96 2A 0A 30 2E 20 A6 77 " +
            "A8 EE DA 06 03 95 D8 F0 19 78 60 73 C8 DE D8 6B " +
            "FD 6E 80 C6 0C CA A1 EC 56 EF 95 62 1A 64 E3 9F " +
            "7F F3 36 33 4D EA 23 3F C9 E6 C4 71 BE 5C 22 9A " +
            "C2 BA 72 7C 04 4D AC D1 16 3A 60 27 FB E5 EC 7E " +
            "0B AC AF F3 77 12 43 69 72 64 97 F4 7D 9B 6A BE " +
            "36 48 C1 1C A1 0E 34 9A 3A 42 72 2C 25 11 0D D8 " +
            "B1 B2 68 2D D4 B7 57 42 AE 6D 51 2E 4B 33 F7 E4 " +
            "78 D0 E0 6E D9 8E AE 8F",
            // After Iota  
            "44 41 0C 18 4A 38 BB 20 45 A9 61 6B 61 C4 F1 48 " +
            "13 C8 C0 AB E1 6E DA 83 F6 44 C7 A5 A2 5D 93 02 " +
            "05 28 14 FA 0E C0 3F C9 84 AE 26 40 BC 22 E4 DF " +
            "77 4A 9E 44 E9 8C FE 24 41 AE E6 8C C2 2A 26 73 " +
            "BB A9 44 E6 F4 CF 8B 23 96 2A 0A 30 2E 20 A6 77 " +
            "A8 EE DA 06 03 95 D8 F0 19 78 60 73 C8 DE D8 6B " +
            "FD 6E 80 C6 0C CA A1 EC 56 EF 95 62 1A 64 E3 9F " +
            "7F F3 36 33 4D EA 23 3F C9 E6 C4 71 BE 5C 22 9A " +
            "C2 BA 72 7C 04 4D AC D1 16 3A 60 27 FB E5 EC 7E " +
            "0B AC AF F3 77 12 43 69 72 64 97 F4 7D 9B 6A BE " +
            "36 48 C1 1C A1 0E 34 9A 3A 42 72 2C 25 11 0D D8 " +
            "B1 B2 68 2D D4 B7 57 42 AE 6D 51 2E 4B 33 F7 E4 " +
            "78 D0 E0 6E D9 8E AE 8F",
            // Round #15
            // After Theta
            "04 C3 6D 62 41 B3 28 8C C2 06 C9 8F 8A A1 AD 06 " +
            "BC 2C 8E 5E 61 0A 33 EB 33 4F D7 88 30 BE 89 42 " +
            "95 B4 17 61 AA AC 53 E5 C4 2C 47 3A B7 A9 77 73 " +
            "F0 E5 36 A0 02 E9 A2 6A EE 4A A8 79 42 4E CF 1B " +
            "7E A2 54 CB 66 2C 91 63 06 B6 09 AB 8A 4C CA 5B " +
            "E8 6C BB 7C 08 1E 4B 5C 9E D7 C8 97 23 BB 84 25 " +
            "52 8A CE 33 8C AE 48 84 93 E4 85 4F 88 87 F9 DF " +
            "EF 6F 35 A8 E9 86 4F 13 89 64 A5 0B B5 D7 B1 36 " +
            "45 15 DA 98 EF 28 F0 9F B9 DE 2E D2 7B 81 05 16 " +
            "CE A7 BF DE E5 F1 59 29 E2 F8 94 6F D9 F7 06 92 " +
            "76 CA A0 66 AA 85 A7 36 BD ED DA C8 CE 74 51 96 " +
            "1E 56 26 D8 54 D3 BE 2A 6B 66 41 03 D9 D0 ED A4 " +
            "E8 4C E3 F5 7D E2 C2 A3",
            // After Rho
            "04 C3 6D 62 41 B3 28 8C 84 0D 92 1F 15 43 5B 0D " +
            "2F 8B A3 57 98 C2 CC 3A E3 9B 28 34 F3 74 8D 08 " +
            "65 9D 2A AF A4 BD 08 53 73 9B 7A 37 47 CC 72 A4 " +
            "03 2A 90 2E AA 06 5F 6E 86 BB 12 6A 9E 90 D3 F3 " +
            "51 AA 65 33 96 C8 31 3F A4 BC 65 60 9B B0 AA C8 " +
            "42 67 DB E5 43 F0 58 E2 96 78 5E 23 5F 8E EC 12 " +
            "9E 61 74 45 22 94 52 74 0F F3 BF 27 C9 0B 9F 10 " +
            "D4 74 C3 A7 89 F7 B7 1A 17 6A AF 63 6D 12 C9 4A " +
            "1B F3 1D 05 FE B3 A8 42 02 8B 5C 6F 17 E9 BD C0 " +
            "3E 2B C5 F9 F4 D7 BB 3C 92 E2 F8 94 6F D9 F7 06 " +
            "9E DA D8 29 83 9A A9 16 F6 B6 6B 23 3B D3 45 59 " +
            "C3 CA 04 9B 6A DA 57 C5 66 41 03 D9 D0 ED A4 6B " +
            "F0 28 3A D3 78 7D 9F B8",
            // After Pi 
            "04 C3 6D 62 41 B3 28 8C 03 2A 90 2E AA 06 5F 6E " +
            "9E 61 74 45 22 94 52 74 3E 2B C5 F9 F4 D7 BB 3C " +
            "F0 28 3A D3 78 7D 9F B8 E3 9B 28 34 F3 74 8D 08 " +
            "A4 BC 65 60 9B B0 AA C8 42 67 DB E5 43 F0 58 E2 " +
            "1B F3 1D 05 FE B3 A8 42 C3 CA 04 9B 6A DA 57 C5 " +
            "84 0D 92 1F 15 43 5B 0D 86 BB 12 6A 9E 90 D3 F3 " +
            "0F F3 BF 27 C9 0B 9F 10 92 E2 F8 94 6F D9 F7 06 " +
            "9E DA D8 29 83 9A A9 16 65 9D 2A AF A4 BD 08 53 " +
            "73 9B 7A 37 47 CC 72 A4 96 78 5E 23 5F 8E EC 12 " +
            "02 8B 5C 6F 17 E9 BD C0 66 41 03 D9 D0 ED A4 6B " +
            "2F 8B A3 57 98 C2 CC 3A 51 AA 65 33 96 C8 31 3F " +
            "D4 74 C3 A7 89 F7 B7 1A 17 6A AF 63 6D 12 C9 4A " +
            "F6 B6 6B 23 3B D3 45 59",
            // After Chi
            "98 82 09 23 41 23 28 9C 23 20 11 96 7E 45 F6 66 " +
            "5E 61 4E 47 2A BC 56 F4 3A E8 80 D9 F5 55 9B 38 " +
            "F3 00 AA DF D2 79 C8 DA A1 D8 B2 B1 B3 34 DD 2A " +
            "BD 2C 61 60 27 B3 0A C8 82 6F DB 7F 43 B8 0F 67 " +
            "3B E2 35 21 6F 97 20 4A C7 EE 41 DB 62 5A 75 05 " +
            "8D 4D 3F 1A 54 48 57 0D 16 BB 52 FA B8 40 B3 F5 " +
            "03 EB BF 0E 49 09 97 00 92 E7 FA 82 7B 98 A5 0F " +
            "9C 68 D8 49 09 0A 29 E4 E1 FD 2E AF BC BF 84 41 " +
            "73 18 7A 7B 47 AD 63 64 F2 38 5D B3 9F 8A EC 39 " +
            "03 17 74 49 33 F9 B5 D0 74 43 53 C9 93 AD D6 CF " +
            "AB DF 21 D3 91 F5 4A 3A 52 A0 49 73 F2 C8 79 7F " +
            "34 E0 83 A7 9B 36 B3 0B 1E 63 2F 37 ED 12 41 68 " +
            "A6 96 2F 03 3D DB 74 5C",
            // After Iota  
            "9B 02 09 23 41 23 28 1C 23 20 11 96 7E 45 F6 66 " +
            "5E 61 4E 47 2A BC 56 F4 3A E8 80 D9 F5 55 9B 38 " +
            "F3 00 AA DF D2 79 C8 DA A1 D8 B2 B1 B3 34 DD 2A " +
            "BD 2C 61 60 27 B3 0A C8 82 6F DB 7F 43 B8 0F 67 " +
            "3B E2 35 21 6F 97 20 4A C7 EE 41 DB 62 5A 75 05 " +
            "8D 4D 3F 1A 54 48 57 0D 16 BB 52 FA B8 40 B3 F5 " +
            "03 EB BF 0E 49 09 97 00 92 E7 FA 82 7B 98 A5 0F " +
            "9C 68 D8 49 09 0A 29 E4 E1 FD 2E AF BC BF 84 41 " +
            "73 18 7A 7B 47 AD 63 64 F2 38 5D B3 9F 8A EC 39 " +
            "03 17 74 49 33 F9 B5 D0 74 43 53 C9 93 AD D6 CF " +
            "AB DF 21 D3 91 F5 4A 3A 52 A0 49 73 F2 C8 79 7F " +
            "34 E0 83 A7 9B 36 B3 0B 1E 63 2F 37 ED 12 41 68 " +
            "A6 96 2F 03 3D DB 74 5C",
            // Round #16
            // After Theta
            "B3 4E 64 AC FE DA B5 34 ED EF 72 27 BD 32 B9 65 " +
            "EA 5D 76 4B 00 0D D6 3F D6 73 EA F5 FE 5A 66 C9 " +
            "87 F2 A9 32 FA E3 FA 9F 89 94 DF 3E 0C CD 40 02 " +
            "73 E3 02 D1 E4 C4 45 CB 36 53 E3 73 69 09 8F AC " +
            "D7 79 5F 0D 64 98 DD BB B3 1C 42 36 4A C0 47 40 " +
            "A5 01 52 95 EB B1 CA 25 D8 74 31 4B 7B 37 FC F6 " +
            "B7 D7 87 02 63 B8 17 CB 7E 7C 90 AE 70 97 58 FE " +
            "E8 9A DB A4 21 90 1B A1 C9 B1 43 20 03 46 19 69 " +
            "BD D7 19 CA 84 DA 2C 67 46 04 65 BF B5 3B 6C F2 " +
            "EF 8C 1E 65 38 F6 48 21 00 B1 50 24 BB 37 E4 8A " +
            "83 93 4C 5C 2E 0C D7 12 9C 6F 2A C2 31 BF 36 7C " +
            "80 DC BB AB B1 87 33 C0 F2 F8 45 1B E6 1D BC 99 " +
            "D2 64 2C EE 15 41 46 19",
            // After Rho
            "B3 4E 64 AC FE DA B5 34 DA DF E5 4E 7A 65 72 CB " +
            "7A 97 DD 12 40 83 F5 8F AF 65 96 6C 3D A7 5E EF " +
            "1F D7 FF 3C 94 4F 95 D1 C3 D0 0C 24 90 48 F9 ED " +
            "10 4D 4E 5C B4 3C 37 2E AB CD D4 F8 5C 5A C2 23 " +
            "BC AF 06 32 CC EE DD EB 7C 04 34 CB 21 64 A3 04 " +
            "29 0D 90 AA 5C 8F 55 2E DB 63 D3 C5 2C ED DD F0 " +
            "14 18 C3 BD 58 BE BD 3E 2E B1 FC FD F8 20 5D E1 " +
            "D2 10 C8 8D 50 74 CD 6D 40 06 8C 32 D2 92 63 87 " +
            "43 99 50 9B E5 AC F7 3A 36 79 23 82 B2 DF DA 1D " +
            "1E 29 E4 9D D1 A3 0C C7 8A 00 B1 50 24 BB 37 E4 " +
            "5C 4B 0C 4E 32 71 B9 30 71 BE A9 08 C7 FC DA F0 " +
            "90 7B 77 35 F6 70 06 18 F8 45 1B E6 1D BC 99 F2 " +
            "51 86 34 19 8B 7B 45 90",
            // After Pi 
            "B3 4E 64 AC FE DA B5 34 10 4D 4E 5C B4 3C 37 2E " +
            "14 18 C3 BD 58 BE BD 3E 1E 29 E4 9D D1 A3 0C C7 " +
            "51 86 34 19 8B 7B 45 90 AF 65 96 6C 3D A7 5E EF " +
            "7C 04 34 CB 21 64 A3 04 29 0D 90 AA 5C 8F 55 2E " +
            "43 99 50 9B E5 AC F7 3A 90 7B 77 35 F6 70 06 18 " +
            "DA DF E5 4E 7A 65 72 CB AB CD D4 F8 5C 5A C2 23 " +
            "2E B1 FC FD F8 20 5D E1 8A 00 B1 50 24 BB 37 E4 " +
            "5C 4B 0C 4E 32 71 B9 30 1F D7 FF 3C 94 4F 95 D1 " +
            "C3 D0 0C 24 90 48 F9 ED DB 63 D3 C5 2C ED DD F0 " +
            "36 79 23 82 B2 DF DA 1D F8 45 1B E6 1D BC 99 F2 " +
            "7A 97 DD 12 40 83 F5 8F BC AF 06 32 CC EE DD EB " +
            "D2 10 C8 8D 50 74 CD 6D 40 06 8C 32 D2 92 63 87 " +
            "71 BE A9 08 C7 FC DA F0",
            // After Chi
            "B7 5E E5 0D B6 58 3D 24 1A 6C 6A 5C 35 3D 37 EF " +
            "55 9E D3 BD 52 E6 FC 2E BC 61 A4 39 A5 23 BC E3 " +
            "51 87 3E 49 8B 5F 47 9A AE 6C 16 4C 61 2C 0A C5 " +
            "3E 94 74 DA 80 44 01 14 B9 6F B7 8E 4E DF 55 2E " +
            "6C 9D D0 D3 EC 2B AF DD C0 7B 57 B6 F6 30 A7 18 " +
            "DE EF CD 4B DA 45 6F 0B 2B CD D5 F8 58 C1 E0 27 " +
            "7A FA F0 F3 EA 60 D5 F1 08 94 50 50 6C BF 75 2F " +
            "7D 4B 1C FE 36 6B 39 10 07 F4 2C FD B8 EA 91 C1 " +
            "E7 C8 2C 26 02 5A FB E0 13 67 CB A1 21 CD DC 12 " +
            "31 EB C7 9A 32 9C DE 1C 38 45 1B E6 1D BC F1 DE " +
            "38 87 15 9F 50 93 F5 8B BC A9 02 00 4E 6C FF 69 " +
            "E3 A8 E9 85 55 18 55 1D 4A 07 D8 20 D2 91 46 88 " +
            "F5 96 AB 28 4B 90 D2 90",
            // After Iota  
            "B5 DE E5 0D B6 58 3D A4 1A 6C 6A 5C 35 3D 37 EF " +
            "55 9E D3 BD 52 E6 FC 2E BC 61 A4 39 A5 23 BC E3 " +
            "51 87 3E 49 8B 5F 47 9A AE 6C 16 4C 61 2C 0A C5 " +
            "3E 94 74 DA 80 44 01 14 B9 6F B7 8E 4E DF 55 2E " +
            "6C 9D D0 D3 EC 2B AF DD C0 7B 57 B6 F6 30 A7 18 " +
            "DE EF CD 4B DA 45 6F 0B 2B CD D5 F8 58 C1 E0 27 " +
            "7A FA F0 F3 EA 60 D5 F1 08 94 50 50 6C BF 75 2F " +
            "7D 4B 1C FE 36 6B 39 10 07 F4 2C FD B8 EA 91 C1 " +
            "E7 C8 2C 26 02 5A FB E0 13 67 CB A1 21 CD DC 12 " +
            "31 EB C7 9A 32 9C DE 1C 38 45 1B E6 1D BC F1 DE " +
            "38 87 15 9F 50 93 F5 8B BC A9 02 00 4E 6C FF 69 " +
            "E3 A8 E9 85 55 18 55 1D 4A 07 D8 20 D2 91 46 88 " +
            "F5 96 AB 28 4B 90 D2 90",
            // Round #17
            // After Theta
            "3C 12 EA 73 E9 6D 62 D3 2D CA 00 FD D5 6C E0 32 " +
            "46 C3 41 E5 79 1D D3 70 99 6D 98 42 1C FF BD A4 " +
            "06 5E 0B 99 84 74 C1 5F 27 A0 19 32 3E 19 55 B2 " +
            "09 32 1E 7B 60 15 D6 C9 AA 32 25 D6 65 24 7A 70 " +
            "49 91 EC A8 55 F7 AE 9A 97 A2 62 66 F9 1B 21 DD " +
            "57 23 C2 35 85 70 30 7C 1C 6B BF 59 B8 90 37 FA " +
            "69 A7 62 AB C1 9B FA AF 2D 98 6C 2B D5 63 74 68 " +
            "2A 92 29 2E 39 40 BF D5 8E 38 23 83 E7 DF CE B6 " +
            "D0 6E 46 87 E2 0B 2C 3D 00 3A 59 F9 0A 36 F3 4C " +
            "14 E7 FB E1 8B 40 DF 5B 6F 9C 2E 36 12 97 77 1B " +
            "B1 4B 1A E1 0F A6 AA FC 8B 0F 68 A1 AE 3D 28 B4 " +
            "F0 F5 7B DD 7E E3 7A 43 6F 0B E4 5B 6B 4D 47 CF " +
            "A2 4F 9E F8 44 BB 54 55",
            // After Rho
            "3C 12 EA 73 E9 6D 62 D3 5A 94 01 FA AB D9 C0 65 " +
            "D1 70 50 79 5E C7 34 9C F1 DF 4B 9A D9 86 29 C4 " +
            "A4 0B FE 32 F0 5A C8 24 E3 93 51 25 7B 02 9A 21 " +
            "B1 07 56 61 9D 9C 20 E3 9C AA 4C 89 75 19 89 1E " +
            "48 76 D4 AA 7B 57 CD A4 11 D2 7D 29 2A 66 96 BF " +
            "BB 1A 11 AE 29 84 83 E1 E8 73 AC FD 66 E1 42 DE " +
            "5B 0D DE D4 7F 4D 3B 15 C7 E8 D0 5A 30 D9 56 AA " +
            "97 1C A0 DF 6A 15 C9 14 06 CF BF 9D 6D 1D 71 46 " +
            "E8 50 7C 81 A5 07 DA CD 79 26 00 9D AC 7C 05 9B " +
            "E8 7B 8B E2 7C 3F 7C 11 1B 6F 9C 2E 36 12 97 77 " +
            "AA F2 C7 2E 69 84 3F 98 2E 3E A0 85 BA F6 A0 D0 " +
            "BE 7E AF DB 6F 5C 6F 08 0B E4 5B 6B 4D 47 CF 6F " +
            "55 95 E8 93 27 3E D1 2E",
            // After Pi 
            "3C 12 EA 73 E9 6D 62 D3 B1 07 56 61 9D 9C 20 E3 " +
            "5B 0D DE D4 7F 4D 3B 15 E8 7B 8B E2 7C 3F 7C 11 " +
            "55 95 E8 93 27 3E D1 2E F1 DF 4B 9A D9 86 29 C4 " +
            "11 D2 7D 29 2A 66 96 BF BB 1A 11 AE 29 84 83 E1 " +
            "E8 50 7C 81 A5 07 DA CD BE 7E AF DB 6F 5C 6F 08 " +
            "5A 94 01 FA AB D9 C0 65 9C AA 4C 89 75 19 89 1E " +
            "C7 E8 D0 5A 30 D9 56 AA 1B 6F 9C 2E 36 12 97 77 " +
            "AA F2 C7 2E 69 84 3F 98 A4 0B FE 32 F0 5A C8 24 " +
            "E3 93 51 25 7B 02 9A 21 E8 73 AC FD 66 E1 42 DE " +
            "79 26 00 9D AC 7C 05 9B 0B E4 5B 6B 4D 47 CF 6F " +
            "D1 70 50 79 5E C7 34 9C 48 76 D4 AA 7B 57 CD A4 " +
            "97 1C A0 DF 6A 15 C9 14 06 CF BF 9D 6D 1D 71 46 " +
            "2E 3E A0 85 BA F6 A0 D0",
            // After Chi
            "76 1A 62 E7 8B 2C 79 C7 11 75 57 43 9D AE 64 E3 " +
            "4E 89 BE C5 7C 4D BA 3B C0 79 89 82 B4 7E 5E C0 " +
            "D4 90 FC 93 33 AE D1 0E 5B D7 4B 1C D8 06 28 84 " +
            "51 92 11 28 AE 65 CE B3 AD 34 92 F4 63 DC A6 E1 " +
            "A9 D1 3C 81 35 85 DA 09 BE 7E 9B FA 4D 3C F9 33 " +
            "19 D4 91 A8 AB 19 96 C5 84 AD 40 AD 73 1B 08 4B " +
            "67 78 93 5A 79 5D 7E 22 4B 6B 9C FE B4 4B 57 12 " +
            "2E D8 8B 2F 3D 84 36 82 AC 6B 52 EA F4 BB 88 FA " +
            "F2 97 51 25 F3 1E 9F 20 EA B3 F7 9F 27 E2 88 BA " +
            "DD 2D A4 8D 1C 64 05 9B 48 74 5A 6E 46 47 DD 6E " +
            "46 78 70 2C 5E C7 34 8C 48 B5 CB AA 7E 5F FD E6 " +
            "BF 2C A0 DF F8 F7 49 84 D7 8F EF E5 29 1C 65 4A " +
            "26 38 24 07 9B E6 69 F0",
            // After Iota  
            "F6 1A 62 E7 8B 2C 79 47 11 75 57 43 9D AE 64 E3 " +
            "4E 89 BE C5 7C 4D BA 3B C0 79 89 82 B4 7E 5E C0 " +
            "D4 90 FC 93 33 AE D1 0E 5B D7 4B 1C D8 06 28 84 " +
            "51 92 11 28 AE 65 CE B3 AD 34 92 F4 63 DC A6 E1 " +
            "A9 D1 3C 81 35 85 DA 09 BE 7E 9B FA 4D 3C F9 33 " +
            "19 D4 91 A8 AB 19 96 C5 84 AD 40 AD 73 1B 08 4B " +
            "67 78 93 5A 79 5D 7E 22 4B 6B 9C FE B4 4B 57 12 " +
            "2E D8 8B 2F 3D 84 36 82 AC 6B 52 EA F4 BB 88 FA " +
            "F2 97 51 25 F3 1E 9F 20 EA B3 F7 9F 27 E2 88 BA " +
            "DD 2D A4 8D 1C 64 05 9B 48 74 5A 6E 46 47 DD 6E " +
            "46 78 70 2C 5E C7 34 8C 48 B5 CB AA 7E 5F FD E6 " +
            "BF 2C A0 DF F8 F7 49 84 D7 8F EF E5 29 1C 65 4A " +
            "26 38 24 07 9B E6 69 F0",
            // Round #18
            // After Theta
            "21 B0 C8 5B 8F B8 52 DD EC CA 1D 81 BD 52 58 1E " +
            "60 23 E6 A6 B0 4C 1D F3 45 D7 45 F6 31 C8 A8 45 " +
            "40 E5 AA 2D 96 F8 94 E4 8C 7D E1 A0 DC 92 03 1E " +
            "AC 2D 5B EA 8E 99 F2 4E 83 9E CA 97 AF DD 01 29 " +
            "2C 7F F0 F5 B0 33 2C 8C 2A 0B CD 44 E8 6A BC D9 " +
            "CE 7E 3B 14 AF 8D BD 5F 79 12 0A 6F 53 E7 34 B6 " +
            "49 D2 CB 39 B5 5C D9 EA CE C5 50 8A 31 FD A1 97 " +
            "BA AD DD 91 98 D2 73 68 7B C1 F8 56 F0 2F A3 60 " +
            "0F 28 1B E7 D3 E2 A3 DD C4 19 AF FC EB E3 2F 72 " +
            "58 83 68 F9 99 D2 F3 1E DC 01 0C D0 E3 11 98 84 " +
            "91 D2 DA 90 5A 53 1F 16 B5 0A 81 68 5E A3 C1 1B " +
            "91 86 F8 BC 34 F6 EE 4C 52 21 23 91 AC AA 93 CF " +
            "B2 4D 72 B9 3E B0 2C 1A",
            // After Rho
            "21 B0 C8 5B 8F B8 52 DD D8 95 3B 02 7B A5 B0 3C " +
            "D8 88 B9 29 2C 53 C7 3C 83 8C 5A 54 74 5D 64 1F " +
            "C4 A7 24 07 2A 57 6D B1 CA 2D 39 E0 C1 D8 17 0E " +
            "A5 EE 98 29 EF C4 DA B2 CA A0 A7 F2 E5 6B 77 40 " +
            "3F F8 7A D8 19 16 46 96 C6 9B AD B2 D0 4C 84 AE " +
            "72 F6 DB A1 78 6D EC FD D8 E6 49 28 BC 4D 9D D3 " +
            "CE A9 E5 CA 56 4F 92 5E FA 43 2F 9D 8B A1 14 63 " +
            "48 4C E9 39 34 DD D6 EE AD E0 5F 46 C1 F6 82 F1 " +
            "E3 7C 5A 7C B4 FB 01 65 17 39 E2 8C 57 FE F5 F1 " +
            "7A DE 03 6B 10 2D 3F 53 84 DC 01 0C D0 E3 11 98 " +
            "7D 58 44 4A 6B 43 6A 4D D4 2A 04 A2 79 8D 06 6F " +
            "D2 10 9F 97 C6 DE 9D 29 21 23 91 AC AA 93 CF 52 " +
            "8B 86 6C 93 5C AE 0F 2C",
            // After Pi 
            "21 B0 C8 5B 8F B8 52 DD A5 EE 98 29 EF C4 DA B2 " +
            "CE A9 E5 CA 56 4F 92 5E 7A DE 03 6B 10 2D 3F 53 " +
            "8B 86 6C 93 5C AE 0F 2C 83 8C 5A 54 74 5D 64 1F " +
            "C6 9B AD B2 D0 4C 84 AE 72 F6 DB A1 78 6D EC FD " +
            "E3 7C 5A 7C B4 FB 01 65 D2 10 9F 97 C6 DE 9D 29 " +
            "D8 95 3B 02 7B A5 B0 3C CA A0 A7 F2 E5 6B 77 40 " +
            "FA 43 2F 9D 8B A1 14 63 84 DC 01 0C D0 E3 11 98 " +
            "7D 58 44 4A 6B 43 6A 4D C4 A7 24 07 2A 57 6D B1 " +
            "CA 2D 39 E0 C1 D8 17 0E D8 E6 49 28 BC 4D 9D D3 " +
            "17 39 E2 8C 57 FE F5 F1 21 23 91 AC AA 93 CF 52 " +
            "D8 88 B9 29 2C 53 C7 3C 3F F8 7A D8 19 16 46 96 " +
            "48 4C E9 39 34 DD D6 EE AD E0 5F 46 C1 F6 82 F1 " +
            "D4 2A 04 A2 79 8D 06 6F",
            // After Chi
            "6B B1 AD 99 9F B3 52 91 95 B8 9A 08 EF E4 F7 B3 " +
            "4F A9 89 5A 1A CD 92 72 5A EE 83 23 93 3D 6F 82 " +
            "0F C8 7C B3 3C EA 87 0E B3 E8 08 55 5C 7C 0C 4E " +
            "47 93 AD EE 54 DE 85 AE 62 F6 5E 22 3A 69 70 F5 " +
            "E2 F0 1A 3C 84 FA 61 73 96 03 3A 35 46 DE 1D 89 " +
            "E8 D6 33 0F 71 25 B0 1F CE 3C A7 F2 B5 29 76 D8 " +
            "83 43 6B DF A0 A1 7E 26 04 59 3A 0C C0 47 81 A8 " +
            "7F 78 C0 BA EF 09 2D 0D D4 65 64 0F 16 52 E5 60 " +
            "CD 34 9B 64 82 6A 77 2E F8 E4 58 08 14 4C 97 D1 " +
            "D3 BD C6 8F 57 BA D5 50 2B 2B 88 4C 6B 1B DD 5C " +
            "98 8C 38 08 08 9A 57 54 9A 58 6C 9E D8 34 46 87 " +
            "18 46 E9 99 0C D4 D2 E0 A5 60 E6 4F C5 A4 43 E1 " +
            "F3 5A 46 72 68 89 06 ED",
            // After Iota  
            "61 31 AD 99 9F B3 52 91 95 B8 9A 08 EF E4 F7 B3 " +
            "4F A9 89 5A 1A CD 92 72 5A EE 83 23 93 3D 6F 82 " +
            "0F C8 7C B3 3C EA 87 0E B3 E8 08 55 5C 7C 0C 4E " +
            "47 93 AD EE 54 DE 85 AE 62 F6 5E 22 3A 69 70 F5 " +
            "E2 F0 1A 3C 84 FA 61 73 96 03 3A 35 46 DE 1D 89 " +
            "E8 D6 33 0F 71 25 B0 1F CE 3C A7 F2 B5 29 76 D8 " +
            "83 43 6B DF A0 A1 7E 26 04 59 3A 0C C0 47 81 A8 " +
            "7F 78 C0 BA EF 09 2D 0D D4 65 64 0F 16 52 E5 60 " +
            "CD 34 9B 64 82 6A 77 2E F8 E4 58 08 14 4C 97 D1 " +
            "D3 BD C6 8F 57 BA D5 50 2B 2B 88 4C 6B 1B DD 5C " +
            "98 8C 38 08 08 9A 57 54 9A 58 6C 9E D8 34 46 87 " +
            "18 46 E9 99 0C D4 D2 E0 A5 60 E6 4F C5 A4 43 E1 " +
            "F3 5A 46 72 68 89 06 ED",
            // Round #19
            // After Theta
            "C9 05 2B 47 A0 86 54 72 7E 22 4B A0 73 FD 18 66 " +
            "91 E7 E9 13 C5 BC 94 CE 68 D4 1F 11 27 FF 6F 64 " +
            "28 9E 6A E9 20 31 26 0E 1B DC 8E 8B 63 49 0A AD " +
            "AC 09 7C 46 C8 C7 6A 7B BC B8 3E 6B E5 18 76 49 " +
            "D0 CA 86 0E 30 38 61 95 B1 55 2C 6F 5A 05 BC 89 " +
            "40 E2 B5 D1 4E 10 B6 FC 25 A6 76 5A 29 30 99 0D " +
            "5D 0D 0B 96 7F D0 78 9A 36 63 A6 3E 74 85 81 4E " +
            "58 2E D6 E0 F3 D2 8C 0D 7C 51 E2 D1 29 67 E3 83 " +
            "26 AE 4A CC 1E 73 98 FB 26 AA 38 41 CB 3D 91 6D " +
            "E1 87 5A BD E3 78 D5 B6 0C 7D 9E 16 77 C0 7C 5C " +
            "30 B8 BE D6 37 AF 51 B7 71 C2 BD 36 44 2D A9 52 " +
            "C6 08 89 D0 D3 A5 D4 5C 97 5A 7A 7D 71 66 43 07 " +
            "D4 0C 50 28 74 52 A7 ED",
            // After Rho
            "C9 05 2B 47 A0 86 54 72 FC 44 96 40 E7 FA 31 CC " +
            "E4 79 FA 44 31 2F A5 73 F2 FF 46 86 46 FD 11 71 " +
            "89 31 71 40 F1 54 4B 07 38 96 A4 D0 BA C1 ED B8 " +
            "67 84 7C AC B6 C7 9A C0 12 2F AE CF 5A 39 86 5D " +
            "65 43 07 18 9C B0 4A 68 C0 9B 18 5B C5 F2 A6 55 " +
            "07 12 AF 8D 76 82 B0 E5 36 94 98 DA 69 A5 C0 64 " +
            "B0 FC 83 C6 D3 EC 6A 58 0A 03 9D 6C C6 4C 7D E8 " +
            "F0 79 69 C6 06 2C 17 6B A3 53 CE C6 07 F9 A2 C4 " +
            "89 D9 63 0E 73 DF C4 55 C8 36 13 55 9C A0 E5 9E " +
            "AF DA 36 FC 50 AB 77 1C 5C 0C 7D 9E 16 77 C0 7C " +
            "46 DD C2 E0 FA 5A DF BC C5 09 F7 DA 10 B5 A4 4A " +
            "18 21 11 7A BA 94 9A CB 5A 7A 7D 71 66 43 07 97 " +
            "69 3B 35 03 14 0A 9D D4",
            // After Pi 
            "C9 05 2B 47 A0 86 54 72 67 84 7C AC B6 C7 9A C0 " +
            "B0 FC 83 C6 D3 EC 6A 58 AF DA 36 FC 50 AB 77 1C " +
            "69 3B 35 03 14 0A 9D D4 F2 FF 46 86 46 FD 11 71 " +
            "C0 9B 18 5B C5 F2 A6 55 07 12 AF 8D 76 82 B0 E5 " +
            "89 D9 63 0E 73 DF C4 55 18 21 11 7A BA 94 9A CB " +
            "FC 44 96 40 E7 FA 31 CC 12 2F AE CF 5A 39 86 5D " +
            "0A 03 9D 6C C6 4C 7D E8 5C 0C 7D 9E 16 77 C0 7C " +
            "46 DD C2 E0 FA 5A DF BC 89 31 71 40 F1 54 4B 07 " +
            "38 96 A4 D0 BA C1 ED B8 36 94 98 DA 69 A5 C0 64 " +
            "C8 36 13 55 9C A0 E5 9E 5A 7A 7D 71 66 43 07 97 " +
            "E4 79 FA 44 31 2F A5 73 65 43 07 18 9C B0 4A 68 " +
            "F0 79 69 C6 06 2C 17 6B A3 53 CE C6 07 F9 A2 C4 " +
            "C5 09 F7 DA 10 B5 A4 4A",
            // After Chi
            "59 7D A8 05 E1 AE 34 6A 68 86 48 94 B6 C4 8F C4 " +
            "F0 DD 82 C5 D7 EC E2 98 2F DE 3C B8 F0 2F 37 3E " +
            "4F BB 61 AB 02 4B 17 54 F5 FF E1 02 74 FD 01 D1 " +
            "48 52 58 59 C4 AF E2 45 17 32 BF FD FE 82 AA 6F " +
            "6B 07 25 8A 37 B6 C5 65 18 21 09 23 3B 96 3C CF " +
            "F4 44 87 60 63 BE 48 6C 46 23 CE 5D 4A 0A 06 49 " +
            "08 D2 1F 0C 2E 44 62 68 E4 0C 69 9E 13 D7 E0 3C " +
            "44 F6 EA 6F E2 5B 59 AD 8F 31 69 4A B0 70 4B 43 " +
            "F0 B4 A7 D5 2E C1 C8 22 24 DC F4 FA 0B E6 C2 65 " +
            "49 37 13 55 0D B4 AD 9E 6A FC F9 E1 6C C2 A3 2F " +
            "74 41 92 82 33 23 B0 70 66 41 81 18 9D 61 EA EC " +
            "B4 71 58 DE 16 28 13 61 83 23 C6 C2 26 F3 A3 F5 " +
            "C4 0B F2 C2 9C 25 EE 42",
            // After Iota  
            "53 7D A8 85 E1 AE 34 EA 68 86 48 94 B6 C4 8F C4 " +
            "F0 DD 82 C5 D7 EC E2 98 2F DE 3C B8 F0 2F 37 3E " +
            "4F BB 61 AB 02 4B 17 54 F5 FF E1 02 74 FD 01 D1 " +
            "48 52 58 59 C4 AF E2 45 17 32 BF FD FE 82 AA 6F " +
            "6B 07 25 8A 37 B6 C5 65 18 21 09 23 3B 96 3C CF " +
            "F4 44 87 60 63 BE 48 6C 46 23 CE 5D 4A 0A 06 49 " +
            "08 D2 1F 0C 2E 44 62 68 E4 0C 69 9E 13 D7 E0 3C " +
            "44 F6 EA 6F E2 5B 59 AD 8F 31 69 4A B0 70 4B 43 " +
            "F0 B4 A7 D5 2E C1 C8 22 24 DC F4 FA 0B E6 C2 65 " +
            "49 37 13 55 0D B4 AD 9E 6A FC F9 E1 6C C2 A3 2F " +
            "74 41 92 82 33 23 B0 70 66 41 81 18 9D 61 EA EC " +
            "B4 71 58 DE 16 28 13 61 83 23 C6 C2 26 F3 A3 F5 " +
            "C4 0B F2 C2 9C 25 EE 42",
            // Round #20
            // After Theta
            "0E E3 D1 FA DC 4C 98 BD 3E 10 60 9A F7 B2 FE 97 " +
            "D4 5D 31 EF A2 3E 93 86 2A 79 A1 21 BD 09 B2 13 " +
            "77 17 AF CE 17 3E 06 91 A8 61 98 7D 49 1F AD 86 " +
            "1E C4 70 57 85 D9 93 16 33 B2 0C D7 8B 50 DB 71 " +
            "6E A0 B8 13 7A 90 40 48 20 8D C7 46 2E E3 2D 0A " +
            "A9 DA FE 1F 5E 5C E4 3B 10 B5 E6 53 0B 7C 77 1A " +
            "2C 52 AC 26 5B 96 13 76 E1 AB F4 07 5E F1 65 11 " +
            "7C 5A 24 0A F7 2E 48 68 D2 AF 10 35 8D 92 E7 14 " +
            "A6 22 8F DB 6F B7 B9 71 00 5C 47 D0 7E 34 B3 7B " +
            "4C 90 8E CC 40 92 28 B3 52 50 37 84 79 B7 B2 EA " +
            "29 DF EB FD 0E C1 1C 27 30 D7 A9 16 DC 17 9B BF " +
            "90 F1 EB F4 63 FA 62 7F 86 84 5B 5B 6B D5 26 D8 " +
            "FC A7 3C A7 89 50 FF 87",
            // After Rho
            "0E E3 D1 FA DC 4C 98 BD 7D 20 C0 34 EF 65 FD 2F " +
            "75 57 CC BB A8 CF A4 21 9B 20 3B A1 92 17 1A D2 " +
            "F0 31 88 BC BB 78 75 BE 97 F4 D1 6A 88 1A 86 D9 " +
            "77 55 98 3D 69 E1 41 0C DC 8C 2C C3 F5 22 D4 76 " +
            "50 DC 09 3D 48 20 24 37 DE A2 00 D2 78 6C E4 32 " +
            "49 D5 F6 FF F0 E2 22 DF 69 40 D4 9A 4F 2D F0 DD " +
            "35 D9 B2 9C B0 63 91 62 E2 CB 22 C2 57 E9 0F BC " +
            "85 7B 17 24 34 3E 2D 12 6A 1A 25 CF 29 A4 5F 21 " +
            "71 FB ED 36 37 CE 54 E4 D9 3D 00 AE 23 68 3F 9A " +
            "12 65 96 09 D2 91 19 48 EA 52 50 37 84 79 B7 B2 " +
            "73 9C A4 7C AF F7 3B 04 C2 5C A7 5A 70 5F 6C FE " +
            "32 7E 9D 7E 4C 5F EC 0F 84 5B 5B 6B D5 26 D8 86 " +
            "FF 21 FF 29 CF 69 22 D4",
            // After Pi 
            "0E E3 D1 FA DC 4C 98 BD 77 55 98 3D 69 E1 41 0C " +
            "35 D9 B2 9C B0 63 91 62 12 65 96 09 D2 91 19 48 " +
            "FF 21 FF 29 CF 69 22 D4 9B 20 3B A1 92 17 1A D2 " +
            "DE A2 00 D2 78 6C E4 32 49 D5 F6 FF F0 E2 22 DF " +
            "71 FB ED 36 37 CE 54 E4 32 7E 9D 7E 4C 5F EC 0F " +
            "7D 20 C0 34 EF 65 FD 2F DC 8C 2C C3 F5 22 D4 76 " +
            "E2 CB 22 C2 57 E9 0F BC EA 52 50 37 84 79 B7 B2 " +
            "73 9C A4 7C AF F7 3B 04 F0 31 88 BC BB 78 75 BE " +
            "97 F4 D1 6A 88 1A 86 D9 69 40 D4 9A 4F 2D F0 DD " +
            "D9 3D 00 AE 23 68 3F 9A 84 5B 5B 6B D5 26 D8 86 " +
            "75 57 CC BB A8 CF A4 21 50 DC 09 3D 48 20 24 37 " +
            "85 7B 17 24 34 3E 2D 12 6A 1A 25 CF 29 A4 5F 21 " +
            "C2 5C A7 5A 70 5F 6C FE",
            // After Chi
            "0E 6B F3 7A 4C 4E 08 DF 75 71 9C 3C 2B 71 49 04 " +
            "D8 D9 DB BC BD 0B B3 F6 12 A7 96 DB C2 95 81 61 " +
            "8E 35 F7 2C EE C8 63 D4 9A 75 CD 8C 12 95 18 1F " +
            "EE 88 09 D2 7F 60 B0 12 4B D1 E6 B7 B8 F3 8A D4 " +
            "F8 FB CF B7 A5 CE 46 34 76 FC 9D 2C 24 37 08 2F " +
            "5F 63 C2 34 ED AC F6 A7 D4 9C 7C F6 75 32 64 74 " +
            "F3 47 86 8A 7C 6F 07 B8 E6 72 10 37 C4 79 73 99 " +
            "F3 10 88 BF BF F5 3B 54 98 31 8C 2C FC 5D 05 BA " +
            "07 C9 D1 4E A8 5A 89 DB 6D 02 8F DB 9B 2B 30 D9 " +
            "A9 1D 80 3A 09 30 1A A2 83 9F 0A 29 D5 24 5A C7 " +
            "F0 74 DA BB 9C D1 AD 21 3A DC 29 F6 41 A0 76 16 " +
            "05 3F 95 34 64 65 0D CC 5F 19 6D 6E A1 24 DF 20 " +
            "C2 D4 A6 5E 30 7F 6C E8",
            // After Iota  
            "8F EB F3 FA 4C 4E 08 5F 75 71 9C 3C 2B 71 49 04 " +
            "D8 D9 DB BC BD 0B B3 F6 12 A7 96 DB C2 95 81 61 " +
            "8E 35 F7 2C EE C8 63 D4 9A 75 CD 8C 12 95 18 1F " +
            "EE 88 09 D2 7F 60 B0 12 4B D1 E6 B7 B8 F3 8A D4 " +
            "F8 FB CF B7 A5 CE 46 34 76 FC 9D 2C 24 37 08 2F " +
            "5F 63 C2 34 ED AC F6 A7 D4 9C 7C F6 75 32 64 74 " +
            "F3 47 86 8A 7C 6F 07 B8 E6 72 10 37 C4 79 73 99 " +
            "F3 10 88 BF BF F5 3B 54 98 31 8C 2C FC 5D 05 BA " +
            "07 C9 D1 4E A8 5A 89 DB 6D 02 8F DB 9B 2B 30 D9 " +
            "A9 1D 80 3A 09 30 1A A2 83 9F 0A 29 D5 24 5A C7 " +
            "F0 74 DA BB 9C D1 AD 21 3A DC 29 F6 41 A0 76 16 " +
            "05 3F 95 34 64 65 0D CC 5F 19 6D 6E A1 24 DF 20 " +
            "C2 D4 A6 5E 30 7F 6C E8",
            // Round #21
            // After Theta
            "20 99 9F 72 4D AC AB 81 46 2D 74 34 F4 39 00 66 " +
            "5E FC 82 03 63 BE 33 C5 8F F1 AA 25 65 EF 4E EE " +
            "30 6F 06 88 42 09 8F 62 35 07 A1 04 13 77 BB C1 " +
            "DD D4 E1 DA A0 28 F9 70 CD F4 BF 08 66 46 0A E7 " +
            "65 AD F3 49 02 B4 89 BB C8 A6 6C 88 88 F6 E4 99 " +
            "F0 11 AE BC EC 4E 55 79 E7 C0 94 FE AA 7A 2D 16 " +
            "75 62 DF 35 A2 DA 87 8B 7B 24 2C C9 63 03 BC 16 " +
            "4D 4A 79 1B 13 34 D7 E2 37 43 E0 A4 FD BF A6 64 " +
            "34 95 39 46 77 12 C0 B9 EB 27 D6 64 45 9E B0 EA " +
            "34 4B BC C4 AE 4A D5 2D 3D C5 FB 8D 79 E5 B6 71 " +
            "5F 06 B6 33 9D 33 0E FF 09 80 C1 FE 9E E8 3F 74 " +
            "83 1A CC 8B BA D0 8D FF C2 4F 51 90 06 5E 10 AF " +
            "7C 8E 57 FA 9C BE 80 5E",
            // After Rho
            "20 99 9F 72 4D AC AB 81 8C 5A E8 68 E8 73 00 CC " +
            "17 BF E0 C0 98 EF 4C B1 F6 EE E4 FE 18 AF 5A 52 " +
            "4A 78 14 83 79 33 40 14 30 71 B7 1B 5C 73 10 4A " +
            "AE 0D 8A 92 0F D7 4D 1D 79 33 FD 2F 82 99 91 C2 " +
            "D6 F9 24 01 DA C4 DD B2 4F 9E 89 6C CA 86 88 68 " +
            "83 8F 70 E5 65 77 AA CA 58 9C 03 53 FA AB EA B5 " +
            "AE 11 D5 3E 5C AC 13 FB 06 78 2D F6 48 58 92 C7 " +
            "8D 09 9A 6B F1 26 A5 BC 49 FB 7F 4D C9 6E 86 C0 " +
            "C7 E8 4E 02 38 97 A6 32 58 F5 F5 13 6B B2 22 4F " +
            "A9 BA 85 66 89 97 D8 55 71 3D C5 FB 8D 79 E5 B6 " +
            "38 FC 7F 19 D8 CE 74 CE 25 00 06 FB 7B A2 FF D0 " +
            "50 83 79 51 17 BA F1 7F 4F 51 90 06 5E 10 AF C2 " +
            "A0 17 9F E3 95 3E A7 2F",
            // After Pi 
            "20 99 9F 72 4D AC AB 81 AE 0D 8A 92 0F D7 4D 1D " +
            "AE 11 D5 3E 5C AC 13 FB A9 BA 85 66 89 97 D8 55 " +
            "A0 17 9F E3 95 3E A7 2F F6 EE E4 FE 18 AF 5A 52 " +
            "4F 9E 89 6C CA 86 88 68 83 8F 70 E5 65 77 AA CA " +
            "C7 E8 4E 02 38 97 A6 32 50 83 79 51 17 BA F1 7F " +
            "8C 5A E8 68 E8 73 00 CC 79 33 FD 2F 82 99 91 C2 " +
            "06 78 2D F6 48 58 92 C7 71 3D C5 FB 8D 79 E5 B6 " +
            "38 FC 7F 19 D8 CE 74 CE 4A 78 14 83 79 33 40 14 " +
            "30 71 B7 1B 5C 73 10 4A 58 9C 03 53 FA AB EA B5 " +
            "58 F5 F5 13 6B B2 22 4F 4F 51 90 06 5E 10 AF C2 " +
            "17 BF E0 C0 98 EF 4C B1 D6 F9 24 01 DA C4 DD B2 " +
            "8D 09 9A 6B F1 26 A5 BC 49 FB 7F 4D C9 6E 86 C0 " +
            "25 00 06 FB 7B A2 FF D0",
            // After Chi
            "20 89 CA 5E 1D 84 B9 63 AF A7 8A D2 8E C4 85 19 " +
            "AE 14 CF BF 48 84 34 D1 A9 32 85 76 C1 17 D0 D5 " +
            "2E 13 9F 63 97 6D E3 33 76 EF 94 7F 3D DE 78 D0 " +
            "0B FE 87 6E D2 06 8C 58 93 8C 41 B4 62 5F FB 87 " +
            "61 84 CA AC 30 92 AC 32 59 93 70 51 D5 BA 71 57 " +
            "8A 12 E8 B8 A0 33 02 C9 08 36 3D 26 07 B8 F4 F2 " +
            "0E B8 17 F6 18 DE 82 8F F5 3F 45 9B AD 48 E5 B6 " +
            "49 DD 6A 1E DA 46 E5 CC 02 F4 14 C3 DB BB AA A1 " +
            "30 10 43 1B 5D 63 10 00 5F 9C 03 57 EE AB 67 35 " +
            "58 DD F1 92 4A 91 62 5B 7F 50 33 1E 5A 50 BF 88 " +
            "1E BF 7A AA B9 CD 6C BD 96 0B 41 05 D2 8C DF F2 " +
            "A9 09 9A D9 C3 A6 DC AC 5B 44 9F 4D 49 23 86 E1 " +
            "E5 40 02 FA 39 A2 6E D2",
            // After Iota  
            "A0 09 CA 5E 1D 84 B9 E3 AF A7 8A D2 8E C4 85 19 " +
            "AE 14 CF BF 48 84 34 D1 A9 32 85 76 C1 17 D0 D5 " +
            "2E 13 9F 63 97 6D E3 33 76 EF 94 7F 3D DE 78 D0 " +
            "0B FE 87 6E D2 06 8C 58 93 8C 41 B4 62 5F FB 87 " +
            "61 84 CA AC 30 92 AC 32 59 93 70 51 D5 BA 71 57 " +
            "8A 12 E8 B8 A0 33 02 C9 08 36 3D 26 07 B8 F4 F2 " +
            "0E B8 17 F6 18 DE 82 8F F5 3F 45 9B AD 48 E5 B6 " +
            "49 DD 6A 1E DA 46 E5 CC 02 F4 14 C3 DB BB AA A1 " +
            "30 10 43 1B 5D 63 10 00 5F 9C 03 57 EE AB 67 35 " +
            "58 DD F1 92 4A 91 62 5B 7F 50 33 1E 5A 50 BF 88 " +
            "1E BF 7A AA B9 CD 6C BD 96 0B 41 05 D2 8C DF F2 " +
            "A9 09 9A D9 C3 A6 DC AC 5B 44 9F 4D 49 23 86 E1 " +
            "E5 40 02 FA 39 A2 6E D2",
            // Round #22
            // After Theta
            "10 AC 1A 9E 4F CC 7A 93 65 73 53 C4 52 CB 6C 7E " +
            "D9 40 35 07 23 EF FC 46 25 1C ED 94 29 D8 6A 70 " +
            "91 7D 4A 1C 0D 2D 94 14 C6 4A 44 BF 6F 96 BB A0 " +
            "C1 2A 5E 78 0E 09 65 3F E4 D8 BB 0C 09 34 33 10 " +
            "ED AA A2 4E D8 5D 16 97 E6 FD A5 2E 4F FA 06 70 " +
            "3A B7 38 78 F2 7B C1 B9 C2 E2 E4 30 DB B7 1D 95 " +
            "79 EC ED 4E 73 B5 4A 18 79 11 2D 79 45 87 5F 13 " +
            "F6 B3 BF 61 40 06 92 EB B2 51 C4 03 89 F3 69 D1 " +
            "FA C4 9A 0D 81 6C F9 67 28 C8 F9 EF 85 C0 AF A2 " +
            "D4 F3 99 70 A2 5E D8 FE C0 3E E6 61 C0 10 C8 AF " +
            "AE 1A AA 6A EB 85 AF CD 5C DF 98 13 0E 83 36 95 " +
            "DE 5D 60 61 A8 CD 14 3B D7 6A F7 AF A1 EC 3C 44 " +
            "5A 2E D7 85 A3 E2 19 F5",
            // After Rho
            "10 AC 1A 9E 4F CC 7A 93 CA E6 A6 88 A5 96 D9 FC " +
            "36 50 CD C1 C8 3B BF 51 82 AD 06 57 C2 D1 4E 99 " +
            "68 A1 A4 88 EC 53 E2 68 FB 66 B9 0B 6A AC 44 F4 " +
            "85 E7 90 50 F6 13 AC E2 04 39 F6 2E 43 02 CD 0C " +
            "55 51 27 EC 2E 8B CB 76 6F 00 67 DE 5F EA F2 A4 " +
            "D5 B9 C5 C1 93 DF 0B CE 54 0A 8B 93 C3 6C DF 76 " +
            "77 9A AB 55 C2 C8 63 6F 0E BF 26 F2 22 5A F2 8A " +
            "30 20 03 C9 75 FB D9 DF 07 12 E7 D3 A2 65 A3 88 " +
            "B3 21 90 2D FF 4C 9F 58 57 51 14 E4 FC F7 42 E0 " +
            "0B DB 9F 7A 3E 13 4E D4 AF C0 3E E6 61 C0 10 C8 " +
            "BE 36 BB 6A A8 AA AD 17 72 7D 63 4E 38 0C DA 54 " +
            "BB 0B 2C 0C B5 99 62 C7 6A F7 AF A1 EC 3C 44 D7 " +
            "46 BD 96 CB 75 E1 A8 78",
            // After Pi 
            "10 AC 1A 9E 4F CC 7A 93 85 E7 90 50 F6 13 AC E2 " +
            "77 9A AB 55 C2 C8 63 6F 0B DB 9F 7A 3E 13 4E D4 " +
            "46 BD 96 CB 75 E1 A8 78 82 AD 06 57 C2 D1 4E 99 " +
            "6F 00 67 DE 5F EA F2 A4 D5 B9 C5 C1 93 DF 0B CE " +
            "B3 21 90 2D FF 4C 9F 58 BB 0B 2C 0C B5 99 62 C7 " +
            "CA E6 A6 88 A5 96 D9 FC 04 39 F6 2E 43 02 CD 0C " +
            "0E BF 26 F2 22 5A F2 8A AF C0 3E E6 61 C0 10 C8 " +
            "BE 36 BB 6A A8 AA AD 17 68 A1 A4 88 EC 53 E2 68 " +
            "FB 66 B9 0B 6A AC 44 F4 54 0A 8B 93 C3 6C DF 76 " +
            "57 51 14 E4 FC F7 42 E0 6A F7 AF A1 EC 3C 44 D7 " +
            "36 50 CD C1 C8 3B BF 51 55 51 27 EC 2E 8B CB 76 " +
            "30 20 03 C9 75 FB D9 DF 07 12 E7 D3 A2 65 A3 88 " +
            "72 7D 63 4E 38 0C DA 54",
            // After Chi
            "62 B4 31 9B 4F 04 39 9E 8D A6 84 7A CA 00 A0 72 " +
            "33 BE AB D4 83 28 C3 47 1B DB 97 6E 34 1F 1C 57 " +
            "C3 FE 16 8B C5 F2 2C 18 12 14 86 56 42 C4 47 D3 " +
            "4D 00 77 F2 33 EA 66 B4 DD B3 E9 C1 93 4E 6B 49 " +
            "B3 85 92 7E BD 0C 93 40 D6 0B 4D 84 A8 B3 D2 E3 " +
            "C0 60 A6 58 85 CE EB 7E A5 79 EE 2A 02 82 CD 4C " +
            "1E 89 A7 FA AA 70 5F 9D EF 00 3A 66 64 D4 40 20 " +
            "BA 2F EB 4C EA AA A9 17 6C A9 A6 18 6D 13 79 6A " +
            "F8 37 AD 6F 56 3F 44 74 7C AC 20 92 C3 64 DB 61 " +
            "57 51 14 EC FC B4 E0 C8 F9 B1 B6 A2 EE 90 40 43 " +
            "16 70 CD C0 99 4B AF D8 52 43 C3 FE AC 8F E9 76 " +
            "40 4D 03 C5 6D F3 81 8B 03 12 6B 52 62 56 86 89 " +
            "33 7C 41 62 1E 8C 9A 72",
            // After Iota  
            "63 B4 31 1B 4F 04 39 9E 8D A6 84 7A CA 00 A0 72 " +
            "33 BE AB D4 83 28 C3 47 1B DB 97 6E 34 1F 1C 57 " +
            "C3 FE 16 8B C5 F2 2C 18 12 14 86 56 42 C4 47 D3 " +
            "4D 00 77 F2 33 EA 66 B4 DD B3 E9 C1 93 4E 6B 49 " +
            "B3 85 92 7E BD 0C 93 40 D6 0B 4D 84 A8 B3 D2 E3 " +
            "C0 60 A6 58 85 CE EB 7E A5 79 EE 2A 02 82 CD 4C " +
            "1E 89 A7 FA AA 70 5F 9D EF 00 3A 66 64 D4 40 20 " +
            "BA 2F EB 4C EA AA A9 17 6C A9 A6 18 6D 13 79 6A " +
            "F8 37 AD 6F 56 3F 44 74 7C AC 20 92 C3 64 DB 61 " +
            "57 51 14 EC FC B4 E0 C8 F9 B1 B6 A2 EE 90 40 43 " +
            "16 70 CD C0 99 4B AF D8 52 43 C3 FE AC 8F E9 76 " +
            "40 4D 03 C5 6D F3 81 8B 03 12 6B 52 62 56 86 89 " +
            "33 7C 41 62 1E 8C 9A 72",
            // Round #23
            // After Theta
            "99 F4 91 FE 3A 43 F9 52 DE 74 72 C6 9F 54 B8 00 " +
            "DA 2F 58 77 65 BA 37 22 1C 90 DF D0 CF 70 AA 95 " +
            "47 D0 A2 D9 4F 7B 03 6C E8 54 26 B3 37 83 87 1F " +
            "1E D2 81 4E 66 BE 7E C6 34 22 1A 62 75 DC 9F 2C " +
            "B4 CE DA C0 46 63 25 82 52 25 F9 D6 22 3A FD 97 " +
            "3A 20 06 BD F0 89 2B B2 F6 AB 18 96 57 D6 D5 3E " +
            "F7 18 54 59 4C E2 AB F8 E8 4B 72 D8 9F BB F6 E2 " +
            "3E 01 5F 1E 60 23 86 63 96 E9 06 FD 18 54 B9 A6 " +
            "AB E5 5B D3 03 6B 5C 06 95 3D D3 31 25 F6 2F 04 " +
            "50 1A 5C 52 07 DB 56 0A 7D 9F 02 F0 64 19 6F 37 " +
            "EC 30 6D 25 EC 0C 6F 14 01 91 35 42 F9 DB F1 04 " +
            "A9 DC F0 66 8B 61 75 EE 04 59 23 EC 99 39 30 4B " +
            "B7 52 F5 30 94 05 B5 06",
            // After Rho
            "99 F4 91 FE 3A 43 F9 52 BC E9 E4 8C 3F A9 70 01 " +
            "F6 0B D6 5D 99 EE 8D 88 0C A7 5A C9 01 F9 0D FD " +
            "DA 1B 60 3B 82 16 CD 7E 7B 33 78 F8 81 4E 65 32 " +
            "E8 64 E6 EB 67 EC 21 1D 0B 8D 88 86 58 1D F7 27 " +
            "67 6D 60 A3 B1 12 41 5A D3 7F 29 55 92 6F 2D A2 " +
            "D5 01 31 E8 85 4F 5C 91 FB D8 AF 62 58 5E 59 57 " +
            "CA 62 12 5F C5 BF C7 A0 77 ED C5 D1 97 E4 B0 3F " +
            "0F B0 11 C3 31 9F 80 2F FA 31 A8 72 4D 2D D3 0D " +
            "6B 7A 60 8D CB 60 B5 7C 17 82 CA 9E E9 98 12 FB " +
            "DB 4A 01 4A 83 4B EA 60 37 7D 9F 02 F0 64 19 6F " +
            "BC 51 B0 C3 B4 95 B0 33 04 44 D6 08 E5 6F C7 13 " +
            "95 1B DE 6C 31 AC CE 3D 59 23 EC 99 39 30 4B 04 " +
            "AD C1 AD 54 3D 0C 65 41",
            // After Pi 
            "99 F4 91 FE 3A 43 F9 52 E8 64 E6 EB 67 EC 21 1D " +
            "CA 62 12 5F C5 BF C7 A0 DB 4A 01 4A 83 4B EA 60 " +
            "AD C1 AD 54 3D 0C 65 41 0C A7 5A C9 01 F9 0D FD " +
            "D3 7F 29 55 92 6F 2D A2 D5 01 31 E8 85 4F 5C 91 " +
            "6B 7A 60 8D CB 60 B5 7C 95 1B DE 6C 31 AC CE 3D " +
            "BC E9 E4 8C 3F A9 70 01 0B 8D 88 86 58 1D F7 27 " +
            "77 ED C5 D1 97 E4 B0 3F 37 7D 9F 02 F0 64 19 6F " +
            "BC 51 B0 C3 B4 95 B0 33 DA 1B 60 3B 82 16 CD 7E " +
            "7B 33 78 F8 81 4E 65 32 FB D8 AF 62 58 5E 59 57 " +
            "17 82 CA 9E E9 98 12 FB 59 23 EC 99 39 30 4B 04 " +
            "F6 0B D6 5D 99 EE 8D 88 67 6D 60 A3 B1 12 41 5A " +
            "0F B0 11 C3 31 9F 80 2F FA 31 A8 72 4D 2D D3 0D " +
            "04 44 D6 08 E5 6F C7 13",
            // After Chi
            "9B F6 81 EA BA 50 3F F2 F9 6C E7 EB 65 AC 09 5D " +
            "EE E3 BE 4B F9 BB C2 A1 CB 7E 11 E0 81 08 72 72 " +
            "CD C1 CB 55 78 A0 65 4C 08 A7 4A 61 04 F9 5D EC " +
            "F9 05 69 50 D8 4F 8C CE 41 00 AF 88 B5 C3 16 90 " +
            "63 DE 60 0C CB 31 B4 BC 46 43 FF 78 A3 AA EE 3F " +
            "C8 89 A1 DD B8 49 70 19 0B 9D 92 84 38 1D FE 67 " +
            "FF ED E5 10 93 75 10 2F 37 D5 DB 0E FB 4C 59 6F " +
            "BF 55 B8 C1 F4 81 37 15 5A D3 E7 39 DA 06 D5 3B " +
            "7F 31 38 64 20 CE 67 9A B3 F9 8B 63 48 7E 10 53 " +
            "95 9A CA BC 6B 9E 96 81 78 03 F4 59 38 78 6B 04 " +
            "FE 9B C7 1D 99 63 0D AD 97 6C C8 93 FD 32 12 5A " +
            "0B F4 47 CB 91 DD 84 3D 08 3A A8 27 55 AD DB 85 " +
            "05 20 F6 AA C5 7F 87 41",
            // After Iota  
            "93 76 81 6A BA 50 3F 72 F9 6C E7 EB 65 AC 09 5D " +
            "EE E3 BE 4B F9 BB C2 A1 CB 7E 11 E0 81 08 72 72 " +
            "CD C1 CB 55 78 A0 65 4C 08 A7 4A 61 04 F9 5D EC " +
            "F9 05 69 50 D8 4F 8C CE 41 00 AF 88 B5 C3 16 90 " +
            "63 DE 60 0C CB 31 B4 BC 46 43 FF 78 A3 AA EE 3F " +
            "C8 89 A1 DD B8 49 70 19 0B 9D 92 84 38 1D FE 67 " +
            "FF ED E5 10 93 75 10 2F 37 D5 DB 0E FB 4C 59 6F " +
            "BF 55 B8 C1 F4 81 37 15 5A D3 E7 39 DA 06 D5 3B " +
            "7F 31 38 64 20 CE 67 9A B3 F9 8B 63 48 7E 10 53 " +
            "95 9A CA BC 6B 9E 96 81 78 03 F4 59 38 78 6B 04 " +
            "FE 9B C7 1D 99 63 0D AD 97 6C C8 93 FD 32 12 5A " +
            "0B F4 47 CB 91 DD 84 3D 08 3A A8 27 55 AD DB 85 " +
            "05 20 F6 AA C5 7F 87 41"
        };
    }
}
