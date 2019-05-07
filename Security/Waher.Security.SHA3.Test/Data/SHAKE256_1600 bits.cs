using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Test vectors available in:
    /// https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHAKE256_Msg1600.pdf
    /// </summary>
    public partial class SHAKE256_Tests
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
            "A3 A3 A3 A3 A3 A3 A3 A3 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00",
            // Round #0
            // After Theta
            "00 00 00 00 00 00 00 00 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 47 47 47 47 47 47 47 47 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "47 47 47 47 47 47 47 47 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 47 47 47 47 47 47 47 47 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 47 47 47 47 47 47 47 47 " +
            "E4 E4 E4 E4 E4 E4 E4 E4 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 47 47 47 47 47 47 47 47 " +
            "47 47 47 47 47 47 47 47 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "A3 A3 A3 A3 A3 A3 A3 A3",
            // After Rho
            "00 00 00 00 00 00 00 00 C9 C9 C9 C9 C9 C9 C9 C9 " +
            "39 39 39 39 39 39 39 39 74 74 74 74 74 74 74 74 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "4E 4E 4E 4E 4E 4E 4E 4E 39 39 39 39 39 39 39 39 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 93 93 93 93 93 93 93 93 " +
            "27 27 27 27 27 27 27 27 8E 8E 8E 8E 8E 8E 8E 8E " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "9C 9C 9C 9C 9C 9C 9C 9C A3 A3 A3 A3 A3 A3 A3 A3 " +
            "9C 9C 9C 9C 9C 9C 9C 9C A3 A3 A3 A3 A3 A3 A3 A3 " +
            "8E 8E 8E 8E 8E 8E 8E 8E 1D 1D 1D 1D 1D 1D 1D 1D " +
            "E8 E8 E8 E8 E8 E8 E8 E8 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "E8 E8 E8 E8 E8 E8 E8 E8",
            // After Pi
            "00 00 00 00 00 00 00 00 4E 4E 4E 4E 4E 4E 4E 4E " +
            "27 27 27 27 27 27 27 27 9C 9C 9C 9C 9C 9C 9C 9C " +
            "E8 E8 E8 E8 E8 E8 E8 E8 74 74 74 74 74 74 74 74 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "9C 9C 9C 9C 9C 9C 9C 9C E8 E8 E8 E8 E8 E8 E8 E8 " +
            "C9 C9 C9 C9 C9 C9 C9 C9 39 39 39 39 39 39 39 39 " +
            "8E 8E 8E 8E 8E 8E 8E 8E A3 A3 A3 A3 A3 A3 A3 A3 " +
            "8E 8E 8E 8E 8E 8E 8E 8E 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 93 93 93 93 93 93 93 93 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "39 39 39 39 39 39 39 39 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "1D 1D 1D 1D 1D 1D 1D 1D",
            // After Chi
            "21 21 21 21 21 21 21 21 D6 D6 D6 D6 D6 D6 D6 D6 " +
            "47 47 47 47 47 47 47 47 9C 9C 9C 9C 9C 9C 9C 9C " +
            "A6 A6 A6 A6 A6 A6 A6 A6 74 74 74 74 74 74 74 74 " +
            "9C 9C 9C 9C 9C 9C 9C 9C 60 60 60 60 60 60 60 60 " +
            "88 88 88 88 88 88 88 88 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "4F 4F 4F 4F 4F 4F 4F 4F 18 18 18 18 18 18 18 18 " +
            "82 82 82 82 82 82 82 82 E2 E2 E2 E2 E2 E2 E2 E2 " +
            "BE BE BE BE BE BE BE BE 93 93 93 93 93 93 93 93 " +
            "20 20 20 20 20 20 20 20 D7 D7 D7 D7 D7 D7 D7 D7 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "39 39 39 39 39 39 39 39 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "1D 1D 1D 1D 1D 1D 1D 1D 20 20 20 20 20 20 20 20 " +
            "9F 9F 9F 9F 9F 9F 9F 9F",
            // After Iota 
            "20 21 21 21 21 21 21 21 D6 D6 D6 D6 D6 D6 D6 D6 " +
            "47 47 47 47 47 47 47 47 9C 9C 9C 9C 9C 9C 9C 9C " +
            "A6 A6 A6 A6 A6 A6 A6 A6 74 74 74 74 74 74 74 74 " +
            "9C 9C 9C 9C 9C 9C 9C 9C 60 60 60 60 60 60 60 60 " +
            "88 88 88 88 88 88 88 88 E8 E8 E8 E8 E8 E8 E8 E8 " +
            "4F 4F 4F 4F 4F 4F 4F 4F 18 18 18 18 18 18 18 18 " +
            "82 82 82 82 82 82 82 82 E2 E2 E2 E2 E2 E2 E2 E2 " +
            "BE BE BE BE BE BE BE BE 93 93 93 93 93 93 93 93 " +
            "20 20 20 20 20 20 20 20 D7 D7 D7 D7 D7 D7 D7 D7 " +
            "A3 A3 A3 A3 A3 A3 A3 A3 E4 E4 E4 E4 E4 E4 E4 E4 " +
            "39 39 39 39 39 39 39 39 A3 A3 A3 A3 A3 A3 A3 A3 " +
            "1D 1D 1D 1D 1D 1D 1D 1D 20 20 20 20 20 20 20 20 " +
            "9F 9F 9F 9F 9F 9F 9F 9F",
            // Round #1
            // After Theta
            "08 09 09 09 09 09 09 09 B9 B8 B8 B8 B8 B8 B8 B8 " +
            "7C 7C 7C 7C 7C 7C 7C 7C E4 E4 E4 E4 E4 E4 E4 E4 " +
            "B0 B2 B2 B2 B2 B2 B2 B2 5C 5C 5C 5C 5C 5C 5C 5C " +
            "F3 F2 F2 F2 F2 F2 F2 F2 5B 5B 5B 5B 5B 5B 5B 5B " +
            "F0 F0 F0 F0 F0 F0 F0 F0 FE FC FC FC FC FC FC FC " +
            "67 67 67 67 67 67 67 67 77 76 76 76 76 76 76 76 " +
            "B9 B9 B9 B9 B9 B9 B9 B9 9A 9A 9A 9A 9A 9A 9A 9A " +
            "A8 AA AA AA AA AA AA AA BB BB BB BB BB BB BB BB " +
            "4F 4E 4E 4E 4E 4E 4E 4E EC EC EC EC EC EC EC EC " +
            "DB DB DB DB DB DB DB DB F2 F0 F0 F0 F0 F0 F0 F0 " +
            "11 11 11 11 11 11 11 11 CC CD CD CD CD CD CD CD " +
            "26 26 26 26 26 26 26 26 58 58 58 58 58 58 58 58 " +
            "89 8B 8B 8B 8B 8B 8B 8B",
            // After Rho
            "08 09 09 09 09 09 09 09 73 71 71 71 71 71 71 71 " +
            "1F 1F 1F 1F 1F 1F 1F 1F 4E 4E 4E 4E 4E 4E 4E 4E " +
            "95 95 95 85 95 95 95 95 C5 C5 C5 C5 C5 C5 C5 C5 " +
            "2F 2F 2F 2F 2F 3F 2F 2F D6 D6 D6 D6 D6 D6 D6 D6 " +
            "78 78 78 78 78 78 78 78 CF CF EF CF CF CF CF CF " +
            "3B 3B 3B 3B 3B 3B 3B 3B D9 DD D9 D9 D9 D9 D9 D9 " +
            "CD CD CD CD CD CD CD CD 35 35 35 35 35 35 35 35 " +
            "55 55 55 55 55 54 55 55 77 77 77 77 77 77 77 77 " +
            "C9 C9 C9 C9 C9 E9 C9 C9 76 76 76 76 76 76 76 76 " +
            "7B 7B 7B 7B 7B 7B 7B 7B F0 F2 F0 F0 F0 F0 F0 F0 " +
            "44 44 44 44 44 44 44 44 33 37 37 37 37 37 37 37 " +
            "C4 C4 C4 C4 C4 C4 C4 C4 58 58 58 58 58 58 58 58 " +
            "E2 62 E2 E2 E2 E2 E2 E2",
            // After Pi
            "08 09 09 09 09 09 09 09 2F 2F 2F 2F 2F 3F 2F 2F " +
            "CD CD CD CD CD CD CD CD 7B 7B 7B 7B 7B 7B 7B 7B " +
            "E2 62 E2 E2 E2 E2 E2 E2 4E 4E 4E 4E 4E 4E 4E 4E " +
            "CF CF EF CF CF CF CF CF 3B 3B 3B 3B 3B 3B 3B 3B " +
            "C9 C9 C9 C9 C9 E9 C9 C9 C4 C4 C4 C4 C4 C4 C4 C4 " +
            "73 71 71 71 71 71 71 71 D6 D6 D6 D6 D6 D6 D6 D6 " +
            "35 35 35 35 35 35 35 35 F0 F2 F0 F0 F0 F0 F0 F0 " +
            "44 44 44 44 44 44 44 44 95 95 95 85 95 95 95 95 " +
            "C5 C5 C5 C5 C5 C5 C5 C5 D9 DD D9 D9 D9 D9 D9 D9 " +
            "76 76 76 76 76 76 76 76 58 58 58 58 58 58 58 58 " +
            "1F 1F 1F 1F 1F 1F 1F 1F 78 78 78 78 78 78 78 78 " +
            "55 55 55 55 55 54 55 55 77 77 77 77 77 77 77 77 " +
            "33 37 37 37 37 37 37 37",
            // After Chi
            "C8 C9 C9 C9 C9 C9 C9 C9 1D 1D 1D 1D 1D 0D 1D 1D " +
            "4D CD 4D 4D 4D 4D 4D 4D 73 72 72 72 72 72 72 72 " +
            "C5 44 C4 C4 C4 D4 C4 C4 7E 7E 5E 7E 7E 7E 7E 7E " +
            "0F 0F 2F 0F 0F 0F 0F 0F 3F 3F 3F 3F 3F 3F 3F 3F " +
            "C3 C3 C3 C3 C3 E3 C3 C3 45 45 65 45 45 45 45 45 " +
            "52 50 50 50 50 50 50 50 16 14 16 16 16 16 16 16 " +
            "31 31 31 31 31 31 31 31 C3 C3 C1 C1 C1 C1 C1 C1 " +
            "C0 C2 C2 C2 C2 C2 C2 C2 8D 8D 8D 9D 8D 8D 8D 8D " +
            "E3 E7 E3 E3 E3 E3 E3 E3 D1 D5 D1 D1 D1 D1 D1 D1 " +
            "F3 F3 F3 F3 F3 F3 F3 F3 18 18 18 18 18 18 18 18 " +
            "1A 1A 1A 1A 1A 1B 1A 1A 5A 5A 5A 5A 5A 5B 5A 5A " +
            "55 55 55 55 55 54 55 55 7B 7F 7F 7F 7F 7F 7F 7F " +
            "53 57 57 57 57 57 57 57",
            // After Iota 
            "4A 49 C9 C9 C9 C9 C9 C9 1D 1D 1D 1D 1D 0D 1D 1D " +
            "4D CD 4D 4D 4D 4D 4D 4D 73 72 72 72 72 72 72 72 " +
            "C5 44 C4 C4 C4 D4 C4 C4 7E 7E 5E 7E 7E 7E 7E 7E " +
            "0F 0F 2F 0F 0F 0F 0F 0F 3F 3F 3F 3F 3F 3F 3F 3F " +
            "C3 C3 C3 C3 C3 E3 C3 C3 45 45 65 45 45 45 45 45 " +
            "52 50 50 50 50 50 50 50 16 14 16 16 16 16 16 16 " +
            "31 31 31 31 31 31 31 31 C3 C3 C1 C1 C1 C1 C1 C1 " +
            "C0 C2 C2 C2 C2 C2 C2 C2 8D 8D 8D 9D 8D 8D 8D 8D " +
            "E3 E7 E3 E3 E3 E3 E3 E3 D1 D5 D1 D1 D1 D1 D1 D1 " +
            "F3 F3 F3 F3 F3 F3 F3 F3 18 18 18 18 18 18 18 18 " +
            "1A 1A 1A 1A 1A 1B 1A 1A 5A 5A 5A 5A 5A 5B 5A 5A " +
            "55 55 55 55 55 54 55 55 7B 7F 7F 7F 7F 7F 7F 7F " +
            "53 57 57 57 57 57 57 57",
            // Round #2
            // After Theta
            "3A B2 DE BE BE 8C BE BE 63 6A C3 F2 E2 F1 E2 E2 " +
            "07 8B 29 09 09 58 09 09 A2 29 EC AD AD 8C AD AD " +
            "DC 5B 99 F8 D8 EA D8 D8 0E 85 49 09 09 3B 09 09 " +
            "71 78 F1 E0 F0 F3 F0 F0 75 79 5B 7B 7B 2A 7B 7B " +
            "12 98 5D 1C 1C 1D 1C 1C 5C 5A 38 79 59 7B 59 59 " +
            "22 AB 47 27 27 15 27 27 68 63 C8 F9 E9 EA E9 E9 " +
            "7B 77 55 75 75 24 75 75 12 98 5F 1E 1E 3F 1E 1E " +
            "D9 DD 9F FE DE FC DE DE FD 76 9A EA FA C8 FA FA " +
            "9D 90 3D 0C 1C 1F 1C 1C 9B 93 B5 95 95 C4 95 95 " +
            "22 A8 6D 2C 2C 0D 2C 2C 01 07 45 24 04 26 04 04 " +
            "6A E1 0D 6D 6D 5E 6D 6D 24 2D 84 B5 A5 A7 A5 A5 " +
            "1F 13 31 11 11 41 11 11 AA 24 E1 A0 A0 81 A0 A0 " +
            "4A 48 0A 6B 4B 69 4B 4B",
            // After Rho
            "3A B2 DE BE BE 8C BE BE C7 D4 86 E5 C5 E3 C5 C5 " +
            "C1 62 4A 42 02 56 42 C2 CA D8 DA 2A 9A C2 DE DA " +
            "56 C7 C6 E6 DE CA C4 C7 90 B0 93 90 E0 50 98 94 " +
            "0F 0E 3F 0F 0F 1F 87 17 5E 5D DE D6 DE 9E CA DE " +
            "CC 2E 0E 8E 0E 0E 0E 09 97 95 C5 A5 85 93 97 B5 " +
            "11 59 3D 3A 39 A9 38 39 A7 A3 8D 21 E7 A7 AB A7 " +
            "AA AB 23 A9 AB DB BB AB 7E 3C 3C 24 30 BF 3C 3C " +
            "7F 6F 7E 6F EF EC EE 4F D5 F5 91 F5 F5 FB ED 34 " +
            "87 81 E3 83 83 A3 13 B2 CA CA CD C9 DA CA 4A E2 " +
            "81 85 45 04 B5 8D 85 A5 04 01 07 45 24 04 26 04 " +
            "B5 B5 A9 85 37 B4 B5 79 92 B4 10 D6 96 9E 96 96 " +
            "63 22 26 22 22 28 22 E2 24 E1 A0 A0 81 A0 A0 AA " +
            "D2 92 12 92 C2 DA 52 DA",
            // After Pi
            "3A B2 DE BE BE 8C BE BE 0F 0E 3F 0F 0F 1F 87 17 " +
            "AA AB 23 A9 AB DB BB AB 81 85 45 04 B5 8D 85 A5 " +
            "D2 92 12 92 C2 DA 52 DA CA D8 DA 2A 9A C2 DE DA " +
            "97 95 C5 A5 85 93 97 B5 11 59 3D 3A 39 A9 38 39 " +
            "87 81 E3 83 83 A3 13 B2 63 22 26 22 22 28 22 E2 " +
            "C7 D4 86 E5 C5 E3 C5 C5 5E 5D DE D6 DE 9E CA DE " +
            "7E 3C 3C 24 30 BF 3C 3C 04 01 07 45 24 04 26 04 " +
            "B5 B5 A9 85 37 B4 B5 79 56 C7 C6 E6 DE CA C4 C7 " +
            "90 B0 93 90 E0 50 98 94 A7 A3 8D 21 E7 A7 AB A7 " +
            "CA CA CD C9 DA CA 4A E2 24 E1 A0 A0 81 A0 A0 AA " +
            "C1 62 4A 42 02 56 42 C2 CC 2E 0E 8E 0E 0E 0E 09 " +
            "7F 6F 7E 6F EF EC EE 4F D5 F5 91 F5 F5 FB ED 34 " +
            "92 B4 10 D6 96 9E 96 96",
            // After Chi
            "9A 13 DE 1E 1E 4C 86 16 0E 0A 7B 0B 1B 1B 83 13 " +
            "F8 B9 31 3B E9 89 E9 F1 A9 A5 89 28 89 89 29 81 " +
            "D7 9E 33 93 C3 C9 53 DB CA 90 E2 30 A2 EA F6 D2 " +
            "11 15 07 24 07 91 94 37 71 7B 39 1A 19 A1 18 79 " +
            "0F 59 3B 8B 1B 61 CF AA 76 27 23 A7 27 39 23 C7 " +
            "E7 F4 A6 C5 E5 C2 F1 E5 5E 5C DD 97 DA 9E C8 DE " +
            "CF 88 94 A4 23 0F AD 45 46 41 01 25 E4 47 66 80 " +
            "AD BC F1 97 2D A8 BF 63 71 C4 CA C7 D9 6D E7 E4 " +
            "D8 F8 D3 58 F8 18 D8 D4 83 82 AD 01 E6 87 0B AF " +
            "98 CC 8B 8F 84 80 0E A7 A4 D1 B1 B0 A1 B0 B8 BA " +
            "F2 23 3A 23 E3 B6 A2 84 4C BE 8F 1E 1E 1D 0F 39 " +
            "7D 6F 7E 6D ED E8 FC CD 94 B7 DB F5 F5 BB AD 74 " +
            "9E B8 14 5A 9A 96 9A 9F",
            // After Iota 
            "10 93 DE 1E 1E 4C 86 96 0E 0A 7B 0B 1B 1B 83 13 " +
            "F8 B9 31 3B E9 89 E9 F1 A9 A5 89 28 89 89 29 81 " +
            "D7 9E 33 93 C3 C9 53 DB CA 90 E2 30 A2 EA F6 D2 " +
            "11 15 07 24 07 91 94 37 71 7B 39 1A 19 A1 18 79 " +
            "0F 59 3B 8B 1B 61 CF AA 76 27 23 A7 27 39 23 C7 " +
            "E7 F4 A6 C5 E5 C2 F1 E5 5E 5C DD 97 DA 9E C8 DE " +
            "CF 88 94 A4 23 0F AD 45 46 41 01 25 E4 47 66 80 " +
            "AD BC F1 97 2D A8 BF 63 71 C4 CA C7 D9 6D E7 E4 " +
            "D8 F8 D3 58 F8 18 D8 D4 83 82 AD 01 E6 87 0B AF " +
            "98 CC 8B 8F 84 80 0E A7 A4 D1 B1 B0 A1 B0 B8 BA " +
            "F2 23 3A 23 E3 B6 A2 84 4C BE 8F 1E 1E 1D 0F 39 " +
            "7D 6F 7E 6D ED E8 FC CD 94 B7 DB F5 F5 BB AD 74 " +
            "9E B8 14 5A 9A 96 9A 9F",
            // Round #3
            // After Theta
            "8C F4 60 AA AD 10 7B E2 C1 55 8E D6 C9 35 11 8D " +
            "F5 31 0B 3C C6 B0 A6 16 7D DA 4E 53 B5 3C 58 9B " +
            "46 79 04 71 02 23 F9 20 56 F7 5C 84 11 B6 0B A6 " +
            "DE 4A F2 F9 D5 BF 06 A9 7C F3 03 1D 36 98 57 9E " +
            "DB 26 FC F0 27 D4 BE B0 E7 C0 14 45 E6 D3 89 3C " +
            "7B 93 18 71 56 9E 0C 91 91 03 28 4A 08 B0 5A 40 " +
            "C2 00 AE A3 0C 36 E2 A2 92 3E C6 5E D8 F2 17 9A " +
            "3C 5B C6 75 EC 42 15 98 ED A3 74 73 6A 31 1A 90 " +
            "17 A7 26 85 2A 36 4A 4A 8E 0A 97 06 C9 BE 44 48 " +
            "4C B3 4C F4 B8 35 7F BD 35 36 86 52 60 5A 12 41 " +
            "6E 44 84 97 50 EA 5F F0 83 E1 7A C3 CC 33 9D A7 " +
            "70 E7 44 6A C2 D1 B3 2A 40 C8 1C 8E C9 0E DC 6E " +
            "0F 5F 23 B8 5B 7C 30 64",
            // After Rho
            "8C F4 60 AA AD 10 7B E2 83 AB 1C AD 93 6B 22 1A " +
            "7D CC 02 8F 31 AC A9 45 CB 83 B5 D9 A7 ED 34 55 " +
            "18 C9 07 31 CA 23 88 13 18 61 BB 60 6A 75 CF 45 " +
            "9F 5F FD 6B 90 EA AD 24 27 DF FC 40 87 0D E6 95 " +
            "13 7E F8 13 6A 5F D8 6D 9D C8 73 0E 4C 51 64 3E " +
            "DC 9B C4 88 B3 F2 64 88 01 45 0E A0 28 21 C0 6A " +
            "1D 65 B0 11 17 15 06 70 E5 2F 34 25 7D 8C BD B0 " +
            "3A 76 A1 0A 4C 9E 2D E3 E6 D4 62 34 20 DB 47 E9 " +
            "A4 50 C5 46 49 E9 E2 D4 22 24 47 85 4B 83 64 5F " +
            "E6 AF 97 69 96 89 1E B7 41 35 36 86 52 60 5A 12 " +
            "7F C1 BB 11 11 5E 42 A9 0E 86 EB 0D 33 CF 74 9E " +
            "EE 9C 48 4D 38 7A 56 05 C8 1C 8E C9 0E DC 6E 40 " +
            "0C D9 C3 D7 08 EE 16 1F",
            // After Pi
            "8C F4 60 AA AD 10 7B E2 9F 5F FD 6B 90 EA AD 24 " +
            "1D 65 B0 11 17 15 06 70 E6 AF 97 69 96 89 1E B7 " +
            "0C D9 C3 D7 08 EE 16 1F CB 83 B5 D9 A7 ED 34 55 " +
            "9D C8 73 0E 4C 51 64 3E DC 9B C4 88 B3 F2 64 88 " +
            "A4 50 C5 46 49 E9 E2 D4 EE 9C 48 4D 38 7A 56 05 " +
            "83 AB 1C AD 93 6B 22 1A 27 DF FC 40 87 0D E6 95 " +
            "E5 2F 34 25 7D 8C BD B0 41 35 36 86 52 60 5A 12 " +
            "7F C1 BB 11 11 5E 42 A9 18 C9 07 31 CA 23 88 13 " +
            "18 61 BB 60 6A 75 CF 45 01 45 0E A0 28 21 C0 6A " +
            "22 24 47 85 4B 83 64 5F C8 1C 8E C9 0E DC 6E 40 " +
            "7D CC 02 8F 31 AC A9 45 13 7E F8 13 6A 5F D8 6D " +
            "3A 76 A1 0A 4C 9E 2D E3 E6 D4 62 34 20 DB 47 E9 " +
            "0E 86 EB 0D 33 CF 74 9E",
            // After Chi
            "8C D4 60 BA AA 05 79 B2 7D D5 FA 03 10 62 B5 A3 " +
            "15 35 F0 87 1F 73 06 78 66 8B B7 41 33 99 77 57 " +
            "1F D2 5E 96 18 04 92 1B 8B 90 31 59 14 4F 34 D5 " +
            "BD 88 72 48 04 58 E6 6A 96 17 CC 81 83 E0 70 89 " +
            "A5 53 70 D6 CE 6C C2 84 FA D4 0A 4B 70 6A 16 2F " +
            "43 8B 1C 88 EB EB 3B 3A 27 CF FE C2 85 6D A4 97 " +
            "DB EF BD 34 7C 92 BD 19 C1 1F 32 2A D0 41 7A 00 " +
            "5B 95 5B 51 15 5A 86 2C 19 CD 03 B1 CA 23 88 39 " +
            "3A 41 FA 65 29 F7 EB 50 C9 5D 86 E8 2C 7D CA 6A " +
            "32 E5 46 B5 8B A0 E4 4C C8 3C 36 89 2E 88 29 04 " +
            "55 CC 03 87 35 2C 8C C7 D7 FE BA 27 4A 1E 9A 65 " +
            "32 74 28 03 5F 9A 1D F5 97 9C 62 B6 20 FB CE A8 " +
            "0C B4 13 1D 79 9C 24 B6",
            // After Iota 
            "8C 54 60 3A AA 05 79 32 7D D5 FA 03 10 62 B5 A3 " +
            "15 35 F0 87 1F 73 06 78 66 8B B7 41 33 99 77 57 " +
            "1F D2 5E 96 18 04 92 1B 8B 90 31 59 14 4F 34 D5 " +
            "BD 88 72 48 04 58 E6 6A 96 17 CC 81 83 E0 70 89 " +
            "A5 53 70 D6 CE 6C C2 84 FA D4 0A 4B 70 6A 16 2F " +
            "43 8B 1C 88 EB EB 3B 3A 27 CF FE C2 85 6D A4 97 " +
            "DB EF BD 34 7C 92 BD 19 C1 1F 32 2A D0 41 7A 00 " +
            "5B 95 5B 51 15 5A 86 2C 19 CD 03 B1 CA 23 88 39 " +
            "3A 41 FA 65 29 F7 EB 50 C9 5D 86 E8 2C 7D CA 6A " +
            "32 E5 46 B5 8B A0 E4 4C C8 3C 36 89 2E 88 29 04 " +
            "55 CC 03 87 35 2C 8C C7 D7 FE BA 27 4A 1E 9A 65 " +
            "32 74 28 03 5F 9A 1D F5 97 9C 62 B6 20 FB CE A8 " +
            "0C B4 13 1D 79 9C 24 B6",
            // Round #4
            // After Theta
            "E2 15 26 B4 65 58 7B 4F 33 52 E8 6C 9D 01 FE 6E " +
            "51 65 65 31 E0 12 4B 7C 30 59 CC A8 F4 3F 75 74 " +
            "A8 F0 15 92 CB B6 92 6A E5 D1 77 D7 DB 12 36 A8 " +
            "F3 0F 60 27 89 3B AD A7 D2 47 59 37 7C 81 3D 8D " +
            "F3 81 0B 3F 09 CA C0 A7 4D F6 41 4F A3 D8 16 5E " +
            "2D CA 5A 06 24 B6 39 47 69 48 EC AD 08 0E EF 5A " +
            "9F BF 28 82 83 F3 F0 1D 97 CD 49 C3 17 E7 78 23 " +
            "EC B7 10 55 C6 E8 86 5D 77 8C 45 3F 05 7E 8A 44 " +
            "74 C6 E8 0A A4 94 A0 9D 8D 0D 13 5E D3 1C 87 6E " +
            "64 37 3D 5C 4C 06 E6 6F 7F 1E 7D 8D FD 3A 29 75 " +
            "3B 8D 45 09 FA 71 8E BA 99 79 A8 48 C7 7D D1 A8 " +
            "76 24 BD B5 A0 FB 50 F1 C1 4E 19 5F E7 5D CC 8B " +
            "BB 96 58 19 AA 2E 24 C7",
            // After Rho
            "E2 15 26 B4 65 58 7B 4F 66 A4 D0 D9 3A 03 FC DD " +
            "54 59 59 0C B8 C4 12 5F FF 53 47 07 93 C5 8C 4A " +
            "B6 95 54 43 85 AF 90 5C BD 2D 61 83 5A 1E 7D 77 " +
            "76 92 B8 D3 7A 3A FF 00 A3 F4 51 D6 0D 5F 60 4F " +
            "C0 85 9F 04 65 E0 D3 F9 6D E1 D5 64 1F F4 34 8A " +
            "6A 51 D6 32 20 B1 CD 39 6B A5 21 B1 B7 22 38 BC " +
            "11 1C 9C 87 EF F8 FC 45 CE F1 46 2E 9B 93 86 2F " +
            "2A 63 74 C3 2E F6 5B 88 7E 0A FC 14 89 EE 18 8B " +
            "5D 81 94 12 B4 93 CE 18 43 B7 C6 86 09 AF 69 8E " +
            "C0 FC 8D EC A6 87 8B C9 75 7F 1E 7D 8D FD 3A 29 " +
            "39 EA EE 34 16 25 E8 C7 66 E6 A1 22 1D F7 45 A3 " +
            "8E A4 B7 16 74 1F 2A DE 4E 19 5F E7 5D CC 8B C1 " +
            "C9 F1 AE 25 56 86 AA 0B",
            // After Pi
            "E2 15 26 B4 65 58 7B 4F 76 92 B8 D3 7A 3A FF 00 " +
            "11 1C 9C 87 EF F8 FC 45 C0 FC 8D EC A6 87 8B C9 " +
            "C9 F1 AE 25 56 86 AA 0B FF 53 47 07 93 C5 8C 4A " +
            "6D E1 D5 64 1F F4 34 8A 6A 51 D6 32 20 B1 CD 39 " +
            "5D 81 94 12 B4 93 CE 18 8E A4 B7 16 74 1F 2A DE " +
            "66 A4 D0 D9 3A 03 FC DD A3 F4 51 D6 0D 5F 60 4F " +
            "CE F1 46 2E 9B 93 86 2F 75 7F 1E 7D 8D FD 3A 29 " +
            "39 EA EE 34 16 25 E8 C7 B6 95 54 43 85 AF 90 5C " +
            "BD 2D 61 83 5A 1E 7D 77 6B A5 21 B1 B7 22 38 BC " +
            "43 B7 C6 86 09 AF 69 8E 4E 19 5F E7 5D CC 8B C1 " +
            "54 59 59 0C B8 C4 12 5F C0 85 9F 04 65 E0 D3 F9 " +
            "2A 63 74 C3 2E F6 5B 88 7E 0A FC 14 89 EE 18 8B " +
            "66 E6 A1 22 1D F7 45 A3",
            // After Chi
            "E3 19 22 B0 E0 98 7B 0A B6 72 B9 BB 7A 3D FC 88 " +
            "18 1D BE 86 BF F8 DC 47 E2 F8 8D 7C 87 DF DA 8D " +
            "DD 73 36 66 4C A4 2E 0B FD 43 45 15 B3 C4 45 7B " +
            "78 61 D5 64 8B F6 36 8A E8 75 F5 36 60 BD ED FF " +
            "2C D2 D4 13 37 53 4A 18 8E 04 27 76 78 2F 1A 5E " +
            "2A A5 D6 F1 A8 83 7A FD 92 FA 49 87 09 33 58 4F " +
            "C6 71 A6 2E 89 93 46 E9 33 7B 0E B4 A5 FF 2E 31 " +
            "B8 BA EF 32 13 79 E8 C5 F4 15 54 73 20 8F 90 D4 " +
            "BD 3F A7 85 52 93 3C 75 67 AD 38 D0 E3 62 BA FD " +
            "F3 33 C6 86 89 8C 79 92 47 31 7E 67 07 DC E6 E2 " +
            "7E 3B 39 CF B2 D2 1A 5F 94 8D 17 10 E4 E8 D3 FA " +
            "2A 87 75 E1 3A E7 1E A8 6E 13 A4 18 29 EE 0A D7 " +
            "E6 62 27 22 58 D7 84 03",
            // After Iota 
            "68 99 22 B0 E0 98 7B 0A B6 72 B9 BB 7A 3D FC 88 " +
            "18 1D BE 86 BF F8 DC 47 E2 F8 8D 7C 87 DF DA 8D " +
            "DD 73 36 66 4C A4 2E 0B FD 43 45 15 B3 C4 45 7B " +
            "78 61 D5 64 8B F6 36 8A E8 75 F5 36 60 BD ED FF " +
            "2C D2 D4 13 37 53 4A 18 8E 04 27 76 78 2F 1A 5E " +
            "2A A5 D6 F1 A8 83 7A FD 92 FA 49 87 09 33 58 4F " +
            "C6 71 A6 2E 89 93 46 E9 33 7B 0E B4 A5 FF 2E 31 " +
            "B8 BA EF 32 13 79 E8 C5 F4 15 54 73 20 8F 90 D4 " +
            "BD 3F A7 85 52 93 3C 75 67 AD 38 D0 E3 62 BA FD " +
            "F3 33 C6 86 89 8C 79 92 47 31 7E 67 07 DC E6 E2 " +
            "7E 3B 39 CF B2 D2 1A 5F 94 8D 17 10 E4 E8 D3 FA " +
            "2A 87 75 E1 3A E7 1E A8 6E 13 A4 18 29 EE 0A D7 " +
            "E6 62 27 22 58 D7 84 03",
            // Round #5
            // After Theta
            "C9 B1 AF 4C 05 67 3E FF 75 45 25 0C 0C 18 94 86 " +
            "AC A4 41 C1 9B 58 3B 46 0D F7 62 1C F8 7E 74 6A " +
            "D7 A0 BB F2 2A B1 7E E5 5C 6B C8 E9 56 3B 00 8E " +
            "BB 56 49 D3 FD D3 5E 84 5C CC 0A 71 44 1D 0A FE " +
            "C3 DD 3B 73 48 F2 E4 FF 84 D7 AA E2 1E 3A 4A B0 " +
            "8B 8D 5B 0D 4D 7C 3F 08 51 CD D5 30 7F 16 30 41 " +
            "72 C8 59 69 AD 33 A1 E8 DC 74 E1 D4 DA 5E 80 D6 " +
            "B2 69 62 A6 75 6C B8 2B 55 3D D9 8F C5 70 D5 21 " +
            "7E 08 3B 32 24 B6 54 7B D3 14 C7 97 C7 C2 5D FC " +
            "1C 3C 29 E6 F6 2D D7 75 4D E2 F3 F3 61 C9 B6 0C " +
            "DF 13 B4 33 57 2D 5F AA 57 BA 8B A7 92 CD BB F4 " +
            "9E 3E 8A A6 1E 47 F9 A9 81 1C 4B 78 56 4F A4 30 " +
            "EC B1 AA B6 3E C2 D4 ED",
            // After Rho
            "C9 B1 AF 4C 05 67 3E FF EB 8A 4A 18 18 30 28 0D " +
            "2B 69 50 F0 26 D6 8E 11 EF 47 A7 D6 70 2F C6 81 " +
            "89 F5 2B BF 06 DD 95 57 6E B5 03 E0 C8 B5 86 9C " +
            "34 DD 3F ED 45 B8 6B 95 3F 17 B3 42 1C 51 87 82 " +
            "EE 9D 39 24 79 F2 FF E1 A3 04 4B 78 AD 2A EE A1 " +
            "58 6C DC 6A 68 E2 FB 41 04 45 35 57 C3 FC 59 C0 " +
            "4A 6B 9D 09 45 97 43 CE BD 00 AD B9 E9 C2 A9 B5 " +
            "D3 3A 36 DC 15 D9 34 31 1F 8B E1 AA 43 AA 7A B2 " +
            "47 86 C4 96 6A CF 0F 61 2E FE 69 8A E3 CB 63 E1 " +
            "E5 BA 8E 83 27 C5 DC BE 0C 4D E2 F3 F3 61 C9 B6 " +
            "7C A9 7E 4F D0 CE 5C B5 5F E9 2E 9E 4A 36 EF D2 " +
            "D3 47 D1 D4 E3 28 3F D5 1C 4B 78 56 4F A4 30 81 " +
            "75 3B 7B AC AA AD 8F 30",
            // After Pi
            "C9 B1 AF 4C 05 67 3E FF 34 DD 3F ED 45 B8 6B 95 " +
            "4A 6B 9D 09 45 97 43 CE E5 BA 8E 83 27 C5 DC BE " +
            "75 3B 7B AC AA AD 8F 30 EF 47 A7 D6 70 2F C6 81 " +
            "A3 04 4B 78 AD 2A EE A1 58 6C DC 6A 68 E2 FB 41 " +
            "47 86 C4 96 6A CF 0F 61 D3 47 D1 D4 E3 28 3F D5 " +
            "EB 8A 4A 18 18 30 28 0D 3F 17 B3 42 1C 51 87 82 " +
            "BD 00 AD B9 E9 C2 A9 B5 0C 4D E2 F3 F3 61 C9 B6 " +
            "7C A9 7E 4F D0 CE 5C B5 89 F5 2B BF 06 DD 95 57 " +
            "6E B5 03 E0 C8 B5 86 9C 04 45 35 57 C3 FC 59 C0 " +
            "2E FE 69 8A E3 CB 63 E1 1C 4B 78 56 4F A4 30 81 " +
            "2B 69 50 F0 26 D6 8E 11 EE 9D 39 24 79 F2 FF E1 " +
            "D3 3A 36 DC 15 D9 34 31 1F 8B E1 AA 43 AA 7A B2 " +
            "5F E9 2E 9E 4A 36 EF D2",
            // After Chi
            "83 93 2F 4C 05 60 3E B5 91 4D 3D 6F 67 F8 F7 A5 " +
            "5A 6A EC 25 CD BF 40 CE 6D 3A 0A C3 22 87 EC 71 " +
            "41 77 6B 0D EA 35 CE 30 B7 2F 33 D4 30 EF D7 C1 " +
            "A4 86 4B EC AF 27 EA 81 C8 2D CD 2A E9 C2 CB D5 " +
            "6B 86 E2 94 7A C8 CF 61 D3 47 99 FC 6E 28 17 F5 " +
            "6B 8A 46 A1 F9 B2 00 38 3F 5A F1 00 0E 70 C7 80 " +
            "CD A0 B1 B5 E9 4C BD B4 8F 4F E2 E3 FB 51 E9 BE " +
            "68 BC CF 0D D4 8F DB 37 89 B5 1F A8 05 95 CC 17 " +
            "44 0F 4B 68 E8 B6 A4 BD 14 44 25 03 CF D8 49 C0 " +
            "AF 4A 6A 23 E3 92 E6 B7 7A 4B 78 16 87 84 32 09 " +
            "3A 4B 56 28 22 DF 8E 01 E2 1C F8 06 3B D0 B5 63 " +
            "93 5A 38 C8 1D CD B1 71 3F 8B B1 CA 67 6A 7A B3 " +
            "9B 7D 07 9A 13 16 9E 32",
            // After Iota 
            "82 93 2F CC 05 60 3E B5 91 4D 3D 6F 67 F8 F7 A5 " +
            "5A 6A EC 25 CD BF 40 CE 6D 3A 0A C3 22 87 EC 71 " +
            "41 77 6B 0D EA 35 CE 30 B7 2F 33 D4 30 EF D7 C1 " +
            "A4 86 4B EC AF 27 EA 81 C8 2D CD 2A E9 C2 CB D5 " +
            "6B 86 E2 94 7A C8 CF 61 D3 47 99 FC 6E 28 17 F5 " +
            "6B 8A 46 A1 F9 B2 00 38 3F 5A F1 00 0E 70 C7 80 " +
            "CD A0 B1 B5 E9 4C BD B4 8F 4F E2 E3 FB 51 E9 BE " +
            "68 BC CF 0D D4 8F DB 37 89 B5 1F A8 05 95 CC 17 " +
            "44 0F 4B 68 E8 B6 A4 BD 14 44 25 03 CF D8 49 C0 " +
            "AF 4A 6A 23 E3 92 E6 B7 7A 4B 78 16 87 84 32 09 " +
            "3A 4B 56 28 22 DF 8E 01 E2 1C F8 06 3B D0 B5 63 " +
            "93 5A 38 C8 1D CD B1 71 3F 8B B1 CA 67 6A 7A B3 " +
            "9B 7D 07 9A 13 16 9E 32",
            // Round #6
            // After Theta
            "C1 2C 04 66 EA F2 07 89 CC 76 35 B5 B2 C7 C0 C2 " +
            "C5 8C 7A 73 96 BA 26 E0 82 B7 02 52 B5 A2 7E FC " +
            "82 D4 9D 22 1B 3C CE 2F F4 90 18 7E DF 7D EE FD " +
            "F9 BD 43 36 7A 18 DD E6 57 CB 5B 7C B2 C7 AD FB " +
            "84 0B EA 05 ED ED 5D EC 10 E4 6F D3 9F 21 17 EA " +
            "28 35 6D 0B 16 20 39 04 62 61 F9 DA DB 4F F0 E7 " +
            "52 46 27 E3 B2 49 DB 9A 60 C2 EA 72 6C 74 7B 33 " +
            "AB 1F 39 22 25 86 DB 28 CA 0A 34 02 EA 07 F5 2B " +
            "19 34 43 B2 3D 89 93 DA 8B A2 B3 55 94 DD 2F EE " +
            "40 C7 62 B2 74 B7 74 3A B9 E8 8E 39 76 8D 32 16 " +
            "79 F4 7D 82 CD 4D B7 3D BF 27 F0 DC EE EF 82 04 " +
            "0C BC AE 9E 46 C8 D7 5F D0 06 B9 5B F0 4F E8 3E " +
            "58 DE F1 B5 E2 1F 9E 2D",
            // After Rho
            "C1 2C 04 66 EA F2 07 89 99 ED 6A 6A 65 8F 81 85 " +
            "31 A3 DE 9C A5 AE 09 78 2B EA C7 2F 78 2B 20 55 " +
            "E0 71 7E 11 A4 EE 14 D9 F7 DD E7 DE 4F 0F 89 E1 " +
            "64 A3 87 D1 6D 9E DF 3B FE D5 F2 16 9F EC 71 EB " +
            "05 F5 82 F6 F6 2E 76 C2 72 A1 0E 41 FE 36 FD 19 " +
            "40 A9 69 5B B0 00 C9 21 9F 8B 85 E5 6B 6F 3F C1 " +
            "19 97 4D DA D6 94 32 3A E8 F6 66 C0 84 D5 E5 D8 " +
            "91 12 C3 6D 94 D5 8F 1C 04 D4 0F EA 57 94 15 68 " +
            "48 B6 27 71 52 3B 83 66 17 F7 45 D1 D9 2A CA EE " +
            "96 4E 07 E8 58 4C 96 EE 16 B9 E8 8E 39 76 8D 32 " +
            "DD F6 E4 D1 F7 09 36 37 FC 9E C0 73 BB BF 0B 12 " +
            "81 D7 D5 D3 08 F9 FA 8B 06 B9 5B F0 4F E8 3E D0 " +
            "67 0B 96 77 7C AD F8 87",
            // After Pi
            "C1 2C 04 66 EA F2 07 89 64 A3 87 D1 6D 9E DF 3B " +
            "19 97 4D DA D6 94 32 3A 96 4E 07 E8 58 4C 96 EE " +
            "67 0B 96 77 7C AD F8 87 2B EA C7 2F 78 2B 20 55 " +
            "72 A1 0E 41 FE 36 FD 19 40 A9 69 5B B0 00 C9 21 " +
            "48 B6 27 71 52 3B 83 66 81 D7 D5 D3 08 F9 FA 8B " +
            "99 ED 6A 6A 65 8F 81 85 FE D5 F2 16 9F EC 71 EB " +
            "E8 F6 66 C0 84 D5 E5 D8 16 B9 E8 8E 39 76 8D 32 " +
            "DD F6 E4 D1 F7 09 36 37 E0 71 7E 11 A4 EE 14 D9 " +
            "F7 DD E7 DE 4F 0F 89 E1 9F 8B 85 E5 6B 6F 3F C1 " +
            "17 F7 45 D1 D9 2A CA EE 06 B9 5B F0 4F E8 3E D0 " +
            "31 A3 DE 9C A5 AE 09 78 05 F5 82 F6 F6 2E 76 C2 " +
            "91 12 C3 6D 94 D5 8F 1C 04 D4 0F EA 57 94 15 68 " +
            "FC 9E C0 73 BB BF 0B 12",
            // After Chi
            "D8 38 4C 6C 78 F2 27 89 E2 EB 85 F1 65 D6 5B FF " +
            "78 96 DD CD F2 35 5A 3B 16 6A 07 E8 DA 1E 91 E6 " +
            "43 88 15 E6 79 A1 20 B5 2B E2 A6 35 78 2B 20 75 " +
            "7A B7 08 61 BC 0D FF 5F C1 E8 B9 D9 B8 C0 B1 A8 " +
            "62 9E 25 5D 22 39 83 32 D1 D6 DD 93 8E ED 27 83 " +
            "99 CF 6E AA 65 9E 05 95 E8 DC 7A 18 A6 CE 79 C9 " +
            "21 B0 62 91 42 DC D7 DD 16 B0 E2 A4 39 F0 0C B2 " +
            "BB E6 74 C5 6D 69 46 5D E8 73 7E 30 84 8E 22 D9 " +
            "F7 A9 A7 CE DF 0F 49 CF 9F 83 9F C5 6D AF 0B D1 " +
            "F7 B7 61 D0 79 2C CA E7 11 35 DA 3E 04 E9 B7 F0 " +
            "A1 A1 9F 95 A5 7F 80 64 01 31 8E 74 B5 2E 66 A2 " +
            "69 18 03 7C 3C FE 85 0E 05 F5 11 66 53 94 15 00 " +
            "F8 CA C0 11 E9 BF 7D 90",
            // After Iota 
            "59 B8 4C EC 78 F2 27 09 E2 EB 85 F1 65 D6 5B FF " +
            "78 96 DD CD F2 35 5A 3B 16 6A 07 E8 DA 1E 91 E6 " +
            "43 88 15 E6 79 A1 20 B5 2B E2 A6 35 78 2B 20 75 " +
            "7A B7 08 61 BC 0D FF 5F C1 E8 B9 D9 B8 C0 B1 A8 " +
            "62 9E 25 5D 22 39 83 32 D1 D6 DD 93 8E ED 27 83 " +
            "99 CF 6E AA 65 9E 05 95 E8 DC 7A 18 A6 CE 79 C9 " +
            "21 B0 62 91 42 DC D7 DD 16 B0 E2 A4 39 F0 0C B2 " +
            "BB E6 74 C5 6D 69 46 5D E8 73 7E 30 84 8E 22 D9 " +
            "F7 A9 A7 CE DF 0F 49 CF 9F 83 9F C5 6D AF 0B D1 " +
            "F7 B7 61 D0 79 2C CA E7 11 35 DA 3E 04 E9 B7 F0 " +
            "A1 A1 9F 95 A5 7F 80 64 01 31 8E 74 B5 2E 66 A2 " +
            "69 18 03 7C 3C FE 85 0E 05 F5 11 66 53 94 15 00 " +
            "F8 CA C0 11 E9 BF 7D 90",
            // Round #7
            // After Theta
            "95 CE 56 16 25 E9 48 0B 9D 06 D4 5E 93 90 9F 88 " +
            "DF 83 63 B0 30 DE 2A 3C F8 B0 D1 EB 6C 80 35 60 " +
            "97 01 6F ED 1B A2 A0 9D E7 94 BC CF 25 30 4F 77 " +
            "05 5A 59 CE 4A 4B 3B 28 66 FD 07 A4 7A 2B C1 AF " +
            "8C 44 F3 5E 94 A7 27 B4 05 5F A7 98 EC EE A7 AB " +
            "55 B9 74 50 38 85 6A 97 97 31 2B B7 50 88 BD BE " +
            "86 A5 DC EC 80 37 A7 DA F8 6A 34 A7 8F 6E A8 34 " +
            "6F 6F 0E CE 0F 6A C6 75 24 05 64 CA D9 95 4D DB " +
            "88 44 F6 61 29 49 8D B8 38 96 21 B8 AF 44 7B D6 " +
            "19 6D B7 D3 CF B2 6E 61 C5 BC A0 35 66 EA 37 D8 " +
            "6D D7 85 6F F8 64 EF 66 7E DC DF DB 43 68 A2 D5 " +
            "CE 0D BD 01 FE 15 F5 09 EB 2F C7 65 E5 0A B1 86 " +
            "2C 43 BA 1A 8B BC FD B8",
            // After Rho
            "95 CE 56 16 25 E9 48 0B 3B 0D A8 BD 26 21 3F 11 " +
            "F7 E0 18 2C 8C B7 0A CF 06 58 03 86 0F 1B BD CE " +
            "10 05 ED BC 0C 78 6B DF 5C 02 F3 74 77 4E C9 FB " +
            "E5 AC B4 B4 83 52 A0 95 AB 59 FF 01 A9 DE 4A F0 " +
            "A2 79 2F CA D3 13 5A 46 7E BA 5A F0 75 8A C9 EE " +
            "AC CA A5 83 C2 29 54 BB FA 5E C6 AC DC 42 21 F6 " +
            "66 07 BC 39 D5 36 2C E5 DD 50 69 F0 D5 68 4E 1F " +
            "E7 07 35 E3 BA B7 37 07 94 B3 2B 9B B6 49 0A C8 " +
            "3E 2C 25 A9 11 17 91 C8 3D 6B 1C CB 10 DC 57 A2 " +
            "D6 2D 2C A3 ED 76 FA 59 D8 C5 BC A0 35 66 EA 37 " +
            "BD 9B B5 5D 17 BE E1 93 FB 71 7F 6F 0F A1 89 56 " +
            "B9 A1 37 C0 BF A2 3E C1 2F C7 65 E5 0A B1 86 EB " +
            "3F 2E CB 90 AE C6 22 6F",
            // After Pi
            "95 CE 56 16 25 E9 48 0B E5 AC B4 B4 83 52 A0 95 " +
            "66 07 BC 39 D5 36 2C E5 D6 2D 2C A3 ED 76 FA 59 " +
            "3F 2E CB 90 AE C6 22 6F 06 58 03 86 0F 1B BD CE " +
            "7E BA 5A F0 75 8A C9 EE AC CA A5 83 C2 29 54 BB " +
            "3E 2C 25 A9 11 17 91 C8 B9 A1 37 C0 BF A2 3E C1 " +
            "3B 0D A8 BD 26 21 3F 11 AB 59 FF 01 A9 DE 4A F0 " +
            "DD 50 69 F0 D5 68 4E 1F D8 C5 BC A0 35 66 EA 37 " +
            "BD 9B B5 5D 17 BE E1 93 10 05 ED BC 0C 78 6B DF " +
            "5C 02 F3 74 77 4E C9 FB FA 5E C6 AC DC 42 21 F6 " +
            "3D 6B 1C CB 10 DC 57 A2 2F C7 65 E5 0A B1 86 EB " +
            "F7 E0 18 2C 8C B7 0A CF A2 79 2F CA D3 13 5A 46 " +
            "E7 07 35 E3 BA B7 37 07 94 B3 2B 9B B6 49 0A C8 " +
            "FB 71 7F 6F 0F A1 89 56",
            // After Chi
            "97 CD 5E 1F 71 CD 44 6B 75 84 B4 36 AB 12 72 8D " +
            "4F 05 7F 29 D7 B6 2C C3 56 ED 38 A5 EC 5F B2 59 " +
            "5F 0E 6B 30 2C D4 82 FB 86 18 A6 85 8D 3A A9 DF " +
            "6C 9E 5A D8 64 9C 48 AE 2D 4B B7 C3 6C 89 7A BA " +
            "38 74 25 AF 11 0E 10 C6 C1 03 6F B0 CF 22 7E E1 " +
            "6F 0D A8 4D 72 01 3B 1E AB DC 6B 01 89 D8 EA D0 " +
            "F8 4A 68 AD D7 F0 4F 9F DA C1 B4 00 15 67 F4 37 " +
            "3D CB E2 5D 9E 60 A1 73 B2 59 E9 34 84 78 4B DB " +
            "59 23 EB 37 77 D2 9F FB F8 DA A7 88 D6 63 A1 BF " +
            "2D 6B 94 D3 14 94 3E B6 63 C5 77 A5 79 B7 06 CB " +
            "B2 E6 08 0D A4 13 2F CE B2 C9 25 D2 D7 5B 52 8E " +
            "8C 47 61 87 B3 17 B6 11 90 33 2B 9B 36 5F 08 41 " +
            "FB 68 58 AD 5C A1 D9 56",
            // After Iota 
            "9E 4D 5E 1F 71 CD 44 EB 75 84 B4 36 AB 12 72 8D " +
            "4F 05 7F 29 D7 B6 2C C3 56 ED 38 A5 EC 5F B2 59 " +
            "5F 0E 6B 30 2C D4 82 FB 86 18 A6 85 8D 3A A9 DF " +
            "6C 9E 5A D8 64 9C 48 AE 2D 4B B7 C3 6C 89 7A BA " +
            "38 74 25 AF 11 0E 10 C6 C1 03 6F B0 CF 22 7E E1 " +
            "6F 0D A8 4D 72 01 3B 1E AB DC 6B 01 89 D8 EA D0 " +
            "F8 4A 68 AD D7 F0 4F 9F DA C1 B4 00 15 67 F4 37 " +
            "3D CB E2 5D 9E 60 A1 73 B2 59 E9 34 84 78 4B DB " +
            "59 23 EB 37 77 D2 9F FB F8 DA A7 88 D6 63 A1 BF " +
            "2D 6B 94 D3 14 94 3E B6 63 C5 77 A5 79 B7 06 CB " +
            "B2 E6 08 0D A4 13 2F CE B2 C9 25 D2 D7 5B 52 8E " +
            "8C 47 61 87 B3 17 B6 11 90 33 2B 9B 36 5F 08 41 " +
            "FB 68 58 AD 5C A1 D9 56",
            // Round #8
            // After Theta
            "16 7E 01 DE E5 F2 FD 13 DE 50 C8 48 17 F9 DD 22 " +
            "04 29 18 A7 A5 92 F0 FB CF A2 CC 46 54 E4 B9 F8 " +
            "B8 C0 1E AF BB 12 87 DB 0E 2B F9 44 19 05 10 27 " +
            "C7 4A 26 A6 D8 77 E7 01 66 67 D0 4D 1E AD A6 82 " +
            "A1 3B D1 4C A9 B5 1B 67 26 CD 1A 2F 58 E4 7B C1 " +
            "E7 3E F7 8C E6 3E 82 E6 00 08 17 7F 35 33 45 7F " +
            "B3 66 0F 23 A5 D4 93 A7 43 8E 40 E3 AD DC FF 96 " +
            "DA 05 97 C2 09 A6 A4 53 3A 6A B6 F5 10 47 F2 23 " +
            "F2 F7 97 49 CB 39 30 54 B3 F6 C0 06 A4 47 7D 87 " +
            "B4 24 60 30 AC 2F 35 17 84 0B 02 3A EE 71 03 EB " +
            "3A D5 57 CC 30 2C 96 36 19 1D 59 AC 6B B0 FD 21 " +
            "C7 6B 06 09 C1 33 6A 29 09 7C DF 78 8E E4 03 E0 " +
            "1C A6 2D 32 CB 67 DC 76",
            // After Rho
            "16 7E 01 DE E5 F2 FD 13 BC A1 90 91 2E F2 BB 45 " +
            "41 0A C6 69 A9 24 FC 3E 45 9E 8B FF 2C CA 6C 44 " +
            "95 38 DC C6 05 F6 78 DD 94 51 00 71 E2 B0 92 4F " +
            "62 8A 7D 77 1E 70 AC 64 A0 D9 19 74 93 47 AB A9 " +
            "9D 68 A6 D4 DA 8D B3 D0 BE 17 6C D2 AC F1 82 45 " +
            "3F F7 B9 67 34 F7 11 34 FD 01 20 5C FC D5 CC 14 " +
            "18 29 A5 9E 3C 9D 35 7B B9 FF 2D 87 1C 81 C6 5B " +
            "E1 04 53 D2 29 ED 82 4B EB 21 8E E4 47 74 D4 6C " +
            "32 69 39 07 86 4A FE FE BE C3 59 7B 60 03 D2 A3 " +
            "A5 E6 82 96 04 0C 86 F5 EB 84 0B 02 3A EE 71 03 " +
            "58 DA E8 54 5F 31 C3 B0 64 74 64 B1 AE C1 F6 87 " +
            "78 CD 20 21 78 46 2D E5 7C DF 78 8E E4 03 E0 09 " +
            "B7 1D 87 69 8B CC F2 19",
            // After Pi
            "16 7E 01 DE E5 F2 FD 13 62 8A 7D 77 1E 70 AC 64 " +
            "18 29 A5 9E 3C 9D 35 7B A5 E6 82 96 04 0C 86 F5 " +
            "B7 1D 87 69 8B CC F2 19 45 9E 8B FF 2C CA 6C 44 " +
            "BE 17 6C D2 AC F1 82 45 3F F7 B9 67 34 F7 11 34 " +
            "32 69 39 07 86 4A FE FE 78 CD 20 21 78 46 2D E5 " +
            "BC A1 90 91 2E F2 BB 45 A0 D9 19 74 93 47 AB A9 " +
            "B9 FF 2D 87 1C 81 C6 5B EB 84 0B 02 3A EE 71 03 " +
            "58 DA E8 54 5F 31 C3 B0 95 38 DC C6 05 F6 78 DD " +
            "94 51 00 71 E2 B0 92 4F FD 01 20 5C FC D5 CC 14 " +
            "BE C3 59 7B 60 03 D2 A3 7C DF 78 8E E4 03 E0 09 " +
            "41 0A C6 69 A9 24 FC 3E 9D 68 A6 D4 DA 8D B3 D0 " +
            "E1 04 53 D2 29 ED 82 4B EB 21 8E E4 47 74 D4 6C " +
            "64 74 64 B1 AE C1 F6 87",
            // After Chi
            "0E 5F 81 56 C5 7F EC 08 C7 4C 7F 77 1E 70 2E E0 " +
            "0A 30 A0 F7 B7 5D 45 73 A5 84 82 00 60 3E 8B F7 " +
            "D7 9D FB 48 91 CC F2 7D 44 7E 1A DA 3C CC 7D 74 " +
            "BE 1F 6C D2 2E F9 6C 8F 77 73 B9 47 4C F3 10 35 " +
            "37 7B B2 D9 82 C2 BE FE C2 CC 44 21 F8 77 AF E4 " +
            "A5 87 B4 12 22 72 FF 17 E2 D9 1B 74 B1 29 9A A9 " +
            "A9 A5 CD D3 59 90 44 EB 4F A5 1B 83 1A 2C 49 46 " +
            "58 82 E1 30 CE 34 C3 18 FC 38 FC CA 19 B3 34 CD " +
            "96 93 59 52 E2 B2 80 EC BD 1D 00 D8 78 D5 EC 1C " +
            "3F E3 DD 3B 61 F7 CA 77 7C 9E 78 BF 06 03 62 0B " +
            "21 0E 97 6B 88 44 FC 35 97 49 2A F0 9C 9D E7 F4 " +
            "E5 50 33 C3 81 6C A0 C8 EA 2B 0C AC 46 50 DC 54 " +
            "F8 14 44 25 FC 48 F5 47",
            // After Iota 
            "84 5F 81 56 C5 7F EC 08 C7 4C 7F 77 1E 70 2E E0 " +
            "0A 30 A0 F7 B7 5D 45 73 A5 84 82 00 60 3E 8B F7 " +
            "D7 9D FB 48 91 CC F2 7D 44 7E 1A DA 3C CC 7D 74 " +
            "BE 1F 6C D2 2E F9 6C 8F 77 73 B9 47 4C F3 10 35 " +
            "37 7B B2 D9 82 C2 BE FE C2 CC 44 21 F8 77 AF E4 " +
            "A5 87 B4 12 22 72 FF 17 E2 D9 1B 74 B1 29 9A A9 " +
            "A9 A5 CD D3 59 90 44 EB 4F A5 1B 83 1A 2C 49 46 " +
            "58 82 E1 30 CE 34 C3 18 FC 38 FC CA 19 B3 34 CD " +
            "96 93 59 52 E2 B2 80 EC BD 1D 00 D8 78 D5 EC 1C " +
            "3F E3 DD 3B 61 F7 CA 77 7C 9E 78 BF 06 03 62 0B " +
            "21 0E 97 6B 88 44 FC 35 97 49 2A F0 9C 9D E7 F4 " +
            "E5 50 33 C3 81 6C A0 C8 EA 2B 0C AC 46 50 DC 54 " +
            "F8 14 44 25 FC 48 F5 47",
            // Round #9
            // After Theta
            "78 A7 15 73 66 A4 9A 78 67 8B F4 B9 E2 48 33 81 " +
            "80 44 2E 1F F7 3D 2E 75 BA 9C A1 FE 80 31 C5 14 " +
            "AE 2E 88 FB DA D7 D4 36 B8 86 8E FF 9F 17 0B 04 " +
            "1E D8 E7 1C D2 C1 71 EE FD 07 37 AF 0C 93 7B 33 " +
            "28 63 91 27 62 CD F0 1D BB 7F 37 92 B3 6C 89 AF " +
            "59 7F 20 37 81 A9 89 67 42 1E 90 BA 4D 11 87 C8 " +
            "23 D1 43 3B 19 F0 2F ED 50 BD 38 7D FA 23 07 A5 " +
            "21 31 92 83 85 2F E5 53 00 C0 68 EF BA 68 42 BD " +
            "36 54 D2 9C 1E 8A 9D 8D 37 69 8E 30 38 B5 87 1A " +
            "20 FB FE C5 81 F8 84 94 05 2D 0B 0C 4D 18 44 40 " +
            "DD F6 03 4E 2B 9F 8A 45 37 8E A1 3E 60 A5 FA 95 " +
            "6F 24 BD 2B C1 0C CB CE F5 33 2F 52 A6 5F 92 B7 " +
            "81 A7 37 96 B7 53 D3 0C",
            // After Rho
            "78 A7 15 73 66 A4 9A 78 CF 16 E9 73 C5 91 66 02 " +
            "20 91 CB C7 7D 8F 4B 1D 18 53 4C A1 CB 19 EA 0F " +
            "BE A6 B6 71 75 41 DC D7 FF 79 B1 40 80 6B E8 F8 " +
            "CE 21 1D 1C E7 EE 81 7D 4C FF C1 CD 2B C3 E4 DE " +
            "B1 C8 13 B1 66 F8 0E 94 96 F8 BA FB 77 23 39 CB " +
            "CB FA 03 B9 09 4C 4D 3C 22 0B 79 40 EA 36 45 1C " +
            "DA C9 80 7F 69 1F 89 1E 47 0E 4A A1 7A 71 FA F4 " +
            "C1 C2 97 F2 A9 90 18 C9 DE 75 D1 84 7A 01 80 D1 " +
            "9A D3 43 B1 B3 D1 86 4A 43 8D 9B 34 47 18 9C DA " +
            "9F 90 12 64 DF BF 38 10 40 05 2D 0B 0C 4D 18 44 " +
            "2A 16 75 DB 0F 38 AD 7C DE 38 86 FA 80 95 EA 57 " +
            "8D A4 77 25 98 61 D9 F9 33 2F 52 A6 5F 92 B7 F5 " +
            "34 43 E0 E9 8D E5 ED D4",
            // After Pi
            "78 A7 15 73 66 A4 9A 78 CE 21 1D 1C E7 EE 81 7D " +
            "DA C9 80 7F 69 1F 89 1E 9F 90 12 64 DF BF 38 10 " +
            "34 43 E0 E9 8D E5 ED D4 18 53 4C A1 CB 19 EA 0F " +
            "96 F8 BA FB 77 23 39 CB CB FA 03 B9 09 4C 4D 3C " +
            "9A D3 43 B1 B3 D1 86 4A 8D A4 77 25 98 61 D9 F9 " +
            "CF 16 E9 73 C5 91 66 02 4C FF C1 CD 2B C3 E4 DE " +
            "47 0E 4A A1 7A 71 FA F4 40 05 2D 0B 0C 4D 18 44 " +
            "2A 16 75 DB 0F 38 AD 7C BE A6 B6 71 75 41 DC D7 " +
            "FF 79 B1 40 80 6B E8 F8 22 0B 79 40 EA 36 45 1C " +
            "43 8D 9B 34 47 18 9C DA 33 2F 52 A6 5F 92 B7 F5 " +
            "20 91 CB C7 7D 8F 4B 1D B1 C8 13 B1 66 F8 0E 94 " +
            "C1 C2 97 F2 A9 90 18 C9 DE 75 D1 84 7A 01 80 D1 " +
            "DE 38 86 FA 80 95 EA 57",
            // After Chi
            "68 6F 95 10 6E B5 92 7A CB 31 0F 1C 71 4E B1 7D " +
            "FA 8A 60 F6 69 5F 4C DA D7 34 07 76 BD BF 2A 38 " +
            "B2 43 E8 E5 0C AF EC D1 51 51 4D A1 C3 55 AE 3B " +
            "86 F9 FA FB C5 B2 BB 89 CE DE 37 BD 01 6C 14 8D " +
            "8A 80 4B 31 F0 C9 A4 4C 0B 0C C5 7F AC 43 C8 39 " +
            "CC 16 E3 53 95 A1 7C 22 4C FE E4 C7 2F CF E4 DE " +
            "6D 1C 1A 71 79 41 5F CC 85 05 A5 2B CC CC 5A 46 " +
            "2A FF 75 57 25 7A 2D A0 BE A4 FE 71 1F 55 D9 D3 " +
            "BE FD 33 74 85 63 70 3A 12 29 39 C2 F2 B4 66 39 " +
            "CF 0D 3F 65 67 59 D4 D8 72 76 53 A6 DF B8 97 DD " +
            "60 93 4F 85 F4 8F 5B 54 AF FD 53 B5 34 F9 8E 84 " +
            "C1 CA 91 88 29 04 72 CF FE F4 98 81 07 0B 81 D9 " +
            "4F 70 96 CA 82 E5 EE D7",
            // After Iota 
            "E0 6F 95 10 6E B5 92 7A CB 31 0F 1C 71 4E B1 7D " +
            "FA 8A 60 F6 69 5F 4C DA D7 34 07 76 BD BF 2A 38 " +
            "B2 43 E8 E5 0C AF EC D1 51 51 4D A1 C3 55 AE 3B " +
            "86 F9 FA FB C5 B2 BB 89 CE DE 37 BD 01 6C 14 8D " +
            "8A 80 4B 31 F0 C9 A4 4C 0B 0C C5 7F AC 43 C8 39 " +
            "CC 16 E3 53 95 A1 7C 22 4C FE E4 C7 2F CF E4 DE " +
            "6D 1C 1A 71 79 41 5F CC 85 05 A5 2B CC CC 5A 46 " +
            "2A FF 75 57 25 7A 2D A0 BE A4 FE 71 1F 55 D9 D3 " +
            "BE FD 33 74 85 63 70 3A 12 29 39 C2 F2 B4 66 39 " +
            "CF 0D 3F 65 67 59 D4 D8 72 76 53 A6 DF B8 97 DD " +
            "60 93 4F 85 F4 8F 5B 54 AF FD 53 B5 34 F9 8E 84 " +
            "C1 CA 91 88 29 04 72 CF FE F4 98 81 07 0B 81 D9 " +
            "4F 70 96 CA 82 E5 EE D7",
            // Round #10
            // After Theta
            "6F B5 EA 73 E3 2C C3 10 7C 79 4E EB 36 50 54 43 " +
            "38 2D 8D 07 80 27 5F 29 01 F2 D9 45 C6 EA D8 D1 " +
            "1C 34 B2 40 4B 70 E8 2B DE 8B 32 C2 4E CC FF 51 " +
            "31 B1 BB 0C 82 AC 5E B7 0C 79 DA 4C E8 14 07 7E " +
            "5C 46 95 02 8B 9C 56 A5 A5 7B 9F DA EB 9C CC C3 " +
            "43 CC 9C 30 18 38 2D 48 FB B6 A5 30 68 D1 01 E0 " +
            "AF BB F7 80 90 39 4C 3F 53 C3 7B 18 B7 99 A8 AF " +
            "84 88 2F F2 62 A5 29 5A 31 7E 81 12 92 CC 88 B9 " +
            "09 B5 72 83 C2 7D 95 04 D0 8E D4 33 1B CC 75 CA " +
            "19 CB E1 56 1C 0C 26 31 DC 01 09 03 98 67 93 27 " +
            "EF 49 30 E6 79 16 0A 3E 18 B5 12 42 73 E7 6B BA " +
            "03 6D 7C 79 C0 7C 61 3C 28 32 46 B2 7C 5E 73 30 " +
            "E1 07 CC 6F C5 3A EA 2D",
            // After Rho
            "6F B5 EA 73 E3 2C C3 10 F8 F2 9C D6 6D A0 A8 86 " +
            "4E 4B E3 01 E0 C9 57 0A AC 8E 1D 1D 20 9F 5D 64 " +
            "82 43 5F E1 A0 91 05 5A EC C4 FC 1F E5 BD 28 23 " +
            "CB 20 C8 EA 75 1B 13 BB 1F 43 9E 36 13 3A C5 81 " +
            "A3 4A 81 45 4E AB 52 2E C9 3C 5C BA F7 A9 BD CE " +
            "1A 62 E6 84 C1 C0 69 41 80 EF DB 96 C2 A0 45 07 " +
            "07 84 CC 61 FA 79 DD BD 33 51 5F A7 86 F7 30 6E " +
            "79 B1 D2 14 2D 42 C4 17 25 24 99 11 73 63 FC 02 " +
            "6E 50 B8 AF 92 20 A1 56 3A 65 68 47 EA 99 0D E6 " +
            "C1 24 26 63 39 DC 8A 83 27 DC 01 09 03 98 67 93 " +
            "28 F8 BC 27 C1 98 E7 59 62 D4 4A 08 CD 9D AF E9 " +
            "A0 8D 2F 0F 98 2F 8C 67 32 46 B2 7C 5E 73 30 28 " +
            "7A 4B F8 01 F3 5B B1 8E",
            // After Pi
            "6F B5 EA 73 E3 2C C3 10 CB 20 C8 EA 75 1B 13 BB " +
            "07 84 CC 61 FA 79 DD BD C1 24 26 63 39 DC 8A 83 " +
            "7A 4B F8 01 F3 5B B1 8E AC 8E 1D 1D 20 9F 5D 64 " +
            "C9 3C 5C BA F7 A9 BD CE 1A 62 E6 84 C1 C0 69 41 " +
            "6E 50 B8 AF 92 20 A1 56 A0 8D 2F 0F 98 2F 8C 67 " +
            "F8 F2 9C D6 6D A0 A8 86 1F 43 9E 36 13 3A C5 81 " +
            "33 51 5F A7 86 F7 30 6E 27 DC 01 09 03 98 67 93 " +
            "28 F8 BC 27 C1 98 E7 59 82 43 5F E1 A0 91 05 5A " +
            "EC C4 FC 1F E5 BD 28 23 80 EF DB 96 C2 A0 45 07 " +
            "3A 65 68 47 EA 99 0D E6 32 46 B2 7C 5E 73 30 28 " +
            "4E 4B E3 01 E0 C9 57 0A A3 4A 81 45 4E AB 52 2E " +
            "79 B1 D2 14 2D 42 C4 17 25 24 99 11 73 63 FC 02 " +
            "62 D4 4A 08 CD 9D AF E9",
            // After Chi
            "6B 31 EE 72 69 4C 0F 14 0B 00 EA E8 74 9F 11 B9 " +
            "3D CF 14 61 38 7A EC B1 C4 90 24 11 39 F8 C8 93 " +
            "FA 4B F8 89 E7 48 A1 25 BE CC BF 19 20 DF 1D 65 " +
            "AD 2C 44 91 E5 89 3D D8 9A EF E1 84 C9 CF 65 60 " +
            "62 52 A8 BF B2 B0 F0 56 E1 BD 6F AD 4F 0F 2C ED " +
            "D8 E2 DD 57 E9 65 98 E8 1B CF 9E 3E 12 32 82 10 " +
            "3B 71 E3 81 46 F7 B0 26 F7 DE 01 D9 2F B8 6F 15 " +
            "2F F9 BE 07 D3 82 A2 58 82 68 5C 61 A2 91 40 5E " +
            "D6 C4 DC 5E CD A4 20 C3 80 ED 49 AE D6 C2 75 0F " +
            "BA 64 25 C6 4A 19 08 B4 5E C2 12 62 1B 5F 18 09 " +
            "16 FA B1 11 C1 89 D3 1B A7 4E 88 44 1C 8A 6A 2E " +
            "3B 61 90 1C A1 DE C7 FE 29 2F 38 10 53 23 AC 00 " +
            "C3 D4 4A 4C C3 BF AF CD",
            // After Iota 
            "62 B1 EE F2 69 4C 0F 14 0B 00 EA E8 74 9F 11 B9 " +
            "3D CF 14 61 38 7A EC B1 C4 90 24 11 39 F8 C8 93 " +
            "FA 4B F8 89 E7 48 A1 25 BE CC BF 19 20 DF 1D 65 " +
            "AD 2C 44 91 E5 89 3D D8 9A EF E1 84 C9 CF 65 60 " +
            "62 52 A8 BF B2 B0 F0 56 E1 BD 6F AD 4F 0F 2C ED " +
            "D8 E2 DD 57 E9 65 98 E8 1B CF 9E 3E 12 32 82 10 " +
            "3B 71 E3 81 46 F7 B0 26 F7 DE 01 D9 2F B8 6F 15 " +
            "2F F9 BE 07 D3 82 A2 58 82 68 5C 61 A2 91 40 5E " +
            "D6 C4 DC 5E CD A4 20 C3 80 ED 49 AE D6 C2 75 0F " +
            "BA 64 25 C6 4A 19 08 B4 5E C2 12 62 1B 5F 18 09 " +
            "16 FA B1 11 C1 89 D3 1B A7 4E 88 44 1C 8A 6A 2E " +
            "3B 61 90 1C A1 DE C7 FE 29 2F 38 10 53 23 AC 00 " +
            "C3 D4 4A 4C C3 BF AF CD",
            // Round #11
            // After Theta
            "52 7B 57 45 6E 7D 5F 79 D5 B7 14 89 36 CC 1E 68 " +
            "75 09 50 7F 11 E5 EF E4 B1 7E 09 DD BF ED 73 3C " +
            "19 07 AA B0 DD 5F 61 F9 8E 06 06 AE 27 EE 4D 08 " +
            "73 9B BA F0 A7 DA 32 09 D2 29 A5 9A E0 50 66 35 " +
            "17 BC 85 73 34 A5 4B F9 02 F1 3D 94 75 18 EC 31 " +
            "E8 28 64 E0 EE 54 C8 85 C5 78 60 5F 50 61 8D C1 " +
            "73 B7 A7 9F 6F 68 B3 73 82 30 2C 15 A9 AD D4 BA " +
            "CC B5 EC 3E E9 95 62 84 B2 A2 E5 D6 A5 A0 10 33 " +
            "08 73 22 3F 8F F7 2F 12 C8 2B 0D B0 FF 5D 76 5A " +
            "CF 8A 08 0A CC 0C B3 1B BD 8E 40 5B 21 48 D8 D5 " +
            "26 30 08 A6 C6 B8 83 76 79 F9 76 25 5E D9 65 FF " +
            "73 A7 D4 02 88 41 C4 AB 5C C1 15 DC D5 36 17 AF " +
            "20 98 18 75 F9 A8 6F 11",
            // After Rho
            "52 7B 57 45 6E 7D 5F 79 AA 6F 29 12 6D 98 3D D0 " +
            "5D 02 D4 5F 44 F9 3B 79 DB 3E C7 13 EB 97 D0 FD " +
            "FE 0A CB CF 38 50 85 ED 7A E2 DE 84 E0 68 60 E0 " +
            "0B 7F AA 2D 93 30 B7 A9 8D 74 4A A9 26 38 94 59 " +
            "DE C2 39 9A D2 A5 FC 0B C1 1E 23 10 DF 43 59 87 " +
            "44 47 21 03 77 A7 42 2E 06 17 E3 81 7D 41 85 35 " +
            "FD 7C 43 9B 9D 9B BB 3D 5B A9 75 05 61 58 2A 52 " +
            "9F F4 4A 31 42 E6 5A 76 AD 4B 41 21 66 64 45 CB " +
            "E4 E7 F1 FE 45 02 61 4E 3B 2D E4 95 06 D8 FF 2E " +
            "61 76 E3 59 11 41 81 99 D5 BD 8E 40 5B 21 48 D8 " +
            "0E DA 99 C0 20 98 1A E3 E7 E5 DB 95 78 65 97 FD " +
            "EE 94 5A 00 31 88 78 75 C1 15 DC D5 36 17 AF 5C " +
            "5B 04 08 26 46 5D 3E EA",
            // After Pi
            "52 7B 57 45 6E 7D 5F 79 0B 7F AA 2D 93 30 B7 A9 " +
            "FD 7C 43 9B 9D 9B BB 3D 61 76 E3 59 11 41 81 99 " +
            "5B 04 08 26 46 5D 3E EA DB 3E C7 13 EB 97 D0 FD " +
            "C1 1E 23 10 DF 43 59 87 44 47 21 03 77 A7 42 2E " +
            "E4 E7 F1 FE 45 02 61 4E EE 94 5A 00 31 88 78 75 " +
            "AA 6F 29 12 6D 98 3D D0 8D 74 4A A9 26 38 94 59 " +
            "5B A9 75 05 61 58 2A 52 D5 BD 8E 40 5B 21 48 D8 " +
            "0E DA 99 C0 20 98 1A E3 FE 0A CB CF 38 50 85 ED " +
            "7A E2 DE 84 E0 68 60 E0 06 17 E3 81 7D 41 85 35 " +
            "3B 2D E4 95 06 D8 FF 2E C1 15 DC D5 36 17 AF 5C " +
            "5D 02 D4 5F 44 F9 3B 79 DE C2 39 9A D2 A5 FC 0B " +
            "9F F4 4A 31 42 E6 5A 76 AD 4B 41 21 66 64 45 CB " +
            "E7 E5 DB 95 78 65 97 FD",
            // After Chi
            "A6 7B 16 D7 62 F6 57 6D 0B 7D 0A 6D 93 70 B7 29 " +
            "E7 7C 4B BD DB 87 85 5F 61 0D B4 18 39 61 C0 88 " +
            "52 00 A0 0E D7 5D 9E 6A DF 7F C7 10 CB 33 D2 D5 " +
            "61 BE F3 EC DF 43 78 C7 4E 57 2B 03 47 2F 5A 1F " +
            "F5 CD 74 ED 8F 15 E1 C6 EE 94 7A 00 25 C8 71 77 " +
            "F8 E6 1C 16 2C D8 17 D2 09 60 C0 E9 3C 19 D4 D1 " +
            "51 EB 64 85 41 C0 38 71 75 98 AE 52 16 21 6D C8 " +
            "0B CA DB 69 22 B8 9A EA FA 1F EA CE 25 51 00 F8 " +
            "43 CA DA 90 E2 F0 1A EA C6 07 FB C1 4D 46 85 65 " +
            "05 27 E7 9F 0E 98 FF 8F C1 F5 C8 D5 F6 3F CF 5C " +
            "5C 36 96 7E 44 BB 39 0D FE C9 38 9A F6 A5 F9 82 " +
            "DD 50 D0 A5 5A E7 C8 42 B5 49 45 6B 62 FC 6D CB " +
            "65 25 F2 15 EA 61 53 FF",
            // After Iota 
            "AC 7B 16 57 62 F6 57 6D 0B 7D 0A 6D 93 70 B7 29 " +
            "E7 7C 4B BD DB 87 85 5F 61 0D B4 18 39 61 C0 88 " +
            "52 00 A0 0E D7 5D 9E 6A DF 7F C7 10 CB 33 D2 D5 " +
            "61 BE F3 EC DF 43 78 C7 4E 57 2B 03 47 2F 5A 1F " +
            "F5 CD 74 ED 8F 15 E1 C6 EE 94 7A 00 25 C8 71 77 " +
            "F8 E6 1C 16 2C D8 17 D2 09 60 C0 E9 3C 19 D4 D1 " +
            "51 EB 64 85 41 C0 38 71 75 98 AE 52 16 21 6D C8 " +
            "0B CA DB 69 22 B8 9A EA FA 1F EA CE 25 51 00 F8 " +
            "43 CA DA 90 E2 F0 1A EA C6 07 FB C1 4D 46 85 65 " +
            "05 27 E7 9F 0E 98 FF 8F C1 F5 C8 D5 F6 3F CF 5C " +
            "5C 36 96 7E 44 BB 39 0D FE C9 38 9A F6 A5 F9 82 " +
            "DD 50 D0 A5 5A E7 C8 42 B5 49 45 6B 62 FC 6D CB " +
            "65 25 F2 15 EA 61 53 FF",
            // Round #12
            // After Theta
            "03 B4 9A 35 66 7B 4E 96 E0 99 E4 32 E3 14 49 9B " +
            "9A B0 08 78 27 9B C1 8D A4 86 EC 09 6A 4F B8 37 " +
            "58 A0 0F 9E D2 83 17 97 70 B0 4B 72 CF BE CB 2E " +
            "8A 5A 1D B3 AF 27 86 75 33 9B 68 C6 BB 33 1E CD " +
            "30 46 2C FC DC 3B 99 79 E4 34 D5 90 20 16 F8 8A " +
            "57 29 90 74 28 55 0E 29 E2 84 2E B6 4C 7D 2A 63 " +
            "2C 27 27 40 BD DC 7C A3 B0 13 F6 43 45 0F 15 77 " +
            "01 6A 74 F9 27 66 13 17 55 D0 66 AC 21 DC 19 03 " +
            "A8 2E 34 CF 92 94 E4 58 BB CB B8 04 B1 5A C1 B7 " +
            "C0 AC BF 8E 5D B6 87 30 CB 55 67 45 F3 E1 46 A1 " +
            "F3 F9 1A 1C 40 36 20 F6 15 2D D6 C5 86 C1 07 30 " +
            "A0 9C 93 60 A6 FB 8C 90 70 C2 1D 7A 31 D2 15 74 " +
            "6F 85 5D 85 EF BF DA 02",
            // After Rho
            "03 B4 9A 35 66 7B 4E 96 C1 33 C9 65 C6 29 92 36 " +
            "26 2C 02 DE C9 66 70 A3 F6 84 7B 43 6A C8 9E A0 " +
            "1E BC B8 C4 02 7D F0 94 F7 EC BB EC 02 07 BB 24 " +
            "31 FB 7A 62 58 A7 A8 D5 F3 CC 26 9A F1 EE 8C 47 " +
            "23 16 7E EE 9D CC 3C 18 81 AF 48 4E 53 0D 09 62 " +
            "B9 4A 81 A4 43 A9 72 48 8C 89 13 BA D8 32 F5 A9 " +
            "01 EA E5 E6 1B 65 39 39 1E 2A EE 60 27 EC 87 8A " +
            "FC 13 B3 89 8B 00 35 BA 58 43 B8 33 06 AA A0 CD " +
            "E6 59 92 92 1C 0B D5 85 E0 DB DD 65 5C 82 58 AD " +
            "F6 10 06 98 F5 D7 B1 CB A1 CB 55 67 45 F3 E1 46 " +
            "80 D8 CF E7 6B 70 00 D9 54 B4 58 17 1B 06 1F C0 " +
            "94 73 12 CC 74 9F 11 12 C2 1D 7A 31 D2 15 74 70 " +
            "B6 C0 5B 61 57 E1 FB AF",
            // After Pi
            "03 B4 9A 35 66 7B 4E 96 31 FB 7A 62 58 A7 A8 D5 " +
            "01 EA E5 E6 1B 65 39 39 F6 10 06 98 F5 D7 B1 CB " +
            "B6 C0 5B 61 57 E1 FB AF F6 84 7B 43 6A C8 9E A0 " +
            "81 AF 48 4E 53 0D 09 62 B9 4A 81 A4 43 A9 72 48 " +
            "E6 59 92 92 1C 0B D5 85 94 73 12 CC 74 9F 11 12 " +
            "C1 33 C9 65 C6 29 92 36 F3 CC 26 9A F1 EE 8C 47 " +
            "1E 2A EE 60 27 EC 87 8A A1 CB 55 67 45 F3 E1 46 " +
            "80 D8 CF E7 6B 70 00 D9 1E BC B8 C4 02 7D F0 94 " +
            "F7 EC BB EC 02 07 BB 24 8C 89 13 BA D8 32 F5 A9 " +
            "E0 DB DD 65 5C 82 58 AD C2 1D 7A 31 D2 15 74 70 " +
            "26 2C 02 DE C9 66 70 A3 23 16 7E EE 9D CC 3C 18 " +
            "FC 13 B3 89 8B 00 35 BA 58 43 B8 33 06 AA A0 CD " +
            "54 B4 58 17 1B 06 1F C0",
            // After Chi
            "03 B4 1F B1 65 3B 5F BE C7 EB 78 7A BC 35 28 17 " +
            "01 2A BC 87 19 45 73 1D F7 24 86 8C D5 CD B5 DB " +
            "86 8B 3B 23 4F 65 5B EE CE C4 FA E3 6A 68 EC A8 " +
            "C7 BE 5A 5C 4F 0F 8C E7 A9 68 81 E8 23 3D 72 5A " +
            "84 DD FB 91 16 4B 5B 25 95 58 12 C0 65 9A 10 50 " +
            "CD 11 01 05 C0 29 91 BE 52 0D 37 9D B1 FD EC 03 " +
            "1E 3A 64 E0 0D EC 87 13 E0 E8 55 67 C1 FA 73 60 " +
            "B2 14 E9 7D 5A B6 0C 98 16 BD B8 D6 DA 4D B4 1D " +
            "97 BE 77 A9 06 87 B3 20 8E 8D 31 AA 5A 27 D1 F9 " +
            "FC 7B 5D A1 5C EA D8 29 23 5D 79 19 D2 17 7F 50 " +
            "FA 2D 83 DF CB 66 71 01 23 56 76 DC 99 66 BC 5D " +
            "F8 A7 F3 8D 92 04 2A BA 7A 4B BA FB C6 CA C0 EE " +
            "55 A6 24 37 0F 8E 13 D8",
            // After Iota 
            "88 34 1F 31 65 3B 5F BE C7 EB 78 7A BC 35 28 17 " +
            "01 2A BC 87 19 45 73 1D F7 24 86 8C D5 CD B5 DB " +
            "86 8B 3B 23 4F 65 5B EE CE C4 FA E3 6A 68 EC A8 " +
            "C7 BE 5A 5C 4F 0F 8C E7 A9 68 81 E8 23 3D 72 5A " +
            "84 DD FB 91 16 4B 5B 25 95 58 12 C0 65 9A 10 50 " +
            "CD 11 01 05 C0 29 91 BE 52 0D 37 9D B1 FD EC 03 " +
            "1E 3A 64 E0 0D EC 87 13 E0 E8 55 67 C1 FA 73 60 " +
            "B2 14 E9 7D 5A B6 0C 98 16 BD B8 D6 DA 4D B4 1D " +
            "97 BE 77 A9 06 87 B3 20 8E 8D 31 AA 5A 27 D1 F9 " +
            "FC 7B 5D A1 5C EA D8 29 23 5D 79 19 D2 17 7F 50 " +
            "FA 2D 83 DF CB 66 71 01 23 56 76 DC 99 66 BC 5D " +
            "F8 A7 F3 8D 92 04 2A BA 7A 4B BA FB C6 CA C0 EE " +
            "55 A6 24 37 0F 8E 13 D8",
            // Round #13
            // After Theta
            "92 69 AB 1D 73 A6 FA 0C 20 3F 91 F5 9D 0B 34 8D " +
            "CD D8 36 08 F4 DA 3E 20 98 0F 27 45 71 DB 9F 90 " +
            "5C 48 4A BE 6A 9A 10 DE D4 99 4E CF 7C F5 49 1A " +
            "20 6A B3 D3 6E 31 90 7D 65 9A 0B 67 CE A2 3F 67 " +
            "EB F6 5A 58 B2 5D 71 6E 4F 9B 63 5D 40 65 5B 60 " +
            "D7 4C B5 29 D6 B4 34 0C B5 D9 DE 12 90 C3 F0 99 " +
            "D2 C8 EE 6F E0 73 CA 2E 8F C3 F4 AE 65 EC 59 2B " +
            "68 D7 98 E0 7F 49 47 A8 0C E0 0C FA CC D0 11 AF " +
            "70 6A 9E 26 27 B9 AF BA 42 7F BB 25 B7 B8 9C C4 " +
            "93 50 FC 68 F8 FC F2 62 F9 9E 08 84 F7 E8 34 60 " +
            "E0 70 37 F3 DD FB D4 B3 C4 82 9F 53 B8 58 A0 C7 " +
            "34 55 79 02 7F 9B 67 87 15 60 1B 32 62 DC EA A5 " +
            "8F 65 55 AA 2A 71 58 E8",
            // After Rho
            "92 69 AB 1D 73 A6 FA 0C 41 7E 22 EB 3B 17 68 1A " +
            "33 B6 0D 02 BD B6 0F 48 B7 FD 09 89 F9 70 52 14 " +
            "D3 84 F0 E6 42 52 F2 55 CC 57 9F A4 41 9D E9 F4 " +
            "3B ED 16 03 D9 07 A2 36 59 99 E6 C2 99 B3 E8 CF " +
            "7B 2D 2C D9 AE 38 B7 75 B6 05 F6 B4 39 D6 05 54 " +
            "B8 66 AA 4D B1 A6 A5 61 67 D6 66 7B 4B 40 0E C3 " +
            "7F 03 9F 53 76 91 46 76 D8 B3 56 1E 87 E9 5D CB " +
            "F0 BF A4 23 54 B4 6B 4C F4 99 A1 23 5E 19 C0 19 " +
            "D3 E4 24 F7 55 17 4E CD 4E 62 A1 BF DD 92 5B 5C " +
            "5F 5E 6C 12 8A 1F 0D 9F 60 F9 9E 08 84 F7 E8 34 " +
            "53 CF 82 C3 DD CC 77 EF 13 0B 7E 4E E1 62 81 1E " +
            "A6 2A 4F E0 6F F3 EC 90 60 1B 32 62 DC EA A5 15 " +
            "16 FA 63 59 95 AA 4A 1C",
            // After Pi
            "92 69 AB 1D 73 A6 FA 0C 3B ED 16 03 D9 07 A2 36 " +
            "7F 03 9F 53 76 91 46 76 5F 5E 6C 12 8A 1F 0D 9F " +
            "16 FA 63 59 95 AA 4A 1C B7 FD 09 89 F9 70 52 14 " +
            "B6 05 F6 B4 39 D6 05 54 B8 66 AA 4D B1 A6 A5 61 " +
            "D3 E4 24 F7 55 17 4E CD A6 2A 4F E0 6F F3 EC 90 " +
            "41 7E 22 EB 3B 17 68 1A 59 99 E6 C2 99 B3 E8 CF " +
            "D8 B3 56 1E 87 E9 5D CB 60 F9 9E 08 84 F7 E8 34 " +
            "53 CF 82 C3 DD CC 77 EF D3 84 F0 E6 42 52 F2 55 " +
            "CC 57 9F A4 41 9D E9 F4 67 D6 66 7B 4B 40 0E C3 " +
            "4E 62 A1 BF DD 92 5B 5C 60 1B 32 62 DC EA A5 15 " +
            "33 B6 0D 02 BD B6 0F 48 7B 2D 2C D9 AE 38 B7 75 " +
            "F0 BF A4 23 54 B4 6B 4C F4 99 A1 23 5E 19 C0 19 " +
            "13 0B 7E 4E E1 62 81 1E",
            // After Chi
            "D6 6B 22 4D 55 36 BE 4C 3B B1 76 03 51 09 AB BF " +
            "7F A3 9C 1A 63 31 04 76 DF 5F E4 16 E8 1B BD 9F " +
            "3F 7E 77 5B 1D AB 4A 2E BF 9F 01 C0 79 50 F2 35 " +
            "F5 85 F2 06 7D C7 4F D8 9C 6C E1 4D 9B 46 05 71 " +
            "C2 31 24 FE C5 17 5C C9 A6 2A B9 D4 6F 75 E9 D0 " +
            "C1 5C 32 F7 3D 5F 7D 1A 79 D1 6E C2 99 A5 48 FB " +
            "CB B5 56 DD DE E1 4A 00 60 C9 BE 20 A6 E4 E0 24 " +
            "4B 4E 46 C3 5D 6C F7 2A F0 04 90 BD 48 12 F4 56 " +
            "C4 77 1E 20 D5 0F B8 E8 47 CF 74 3B 4B 28 AA C2 " +
            "DD E6 61 3B DF 82 09 1C 6C 48 3D 62 DD 67 AC B5 " +
            "B3 24 8D 20 ED 32 47 40 7F 2D 2D D9 A4 31 37 64 " +
            "F3 BD FA 6F F5 D6 6A 4A D4 2D A0 23 42 8D CE 59 " +
            "5B 02 5E 97 E3 6A 31 2B",
            // After Iota 
            "5D 6B 22 4D 55 36 BE CC 3B B1 76 03 51 09 AB BF " +
            "7F A3 9C 1A 63 31 04 76 DF 5F E4 16 E8 1B BD 9F " +
            "3F 7E 77 5B 1D AB 4A 2E BF 9F 01 C0 79 50 F2 35 " +
            "F5 85 F2 06 7D C7 4F D8 9C 6C E1 4D 9B 46 05 71 " +
            "C2 31 24 FE C5 17 5C C9 A6 2A B9 D4 6F 75 E9 D0 " +
            "C1 5C 32 F7 3D 5F 7D 1A 79 D1 6E C2 99 A5 48 FB " +
            "CB B5 56 DD DE E1 4A 00 60 C9 BE 20 A6 E4 E0 24 " +
            "4B 4E 46 C3 5D 6C F7 2A F0 04 90 BD 48 12 F4 56 " +
            "C4 77 1E 20 D5 0F B8 E8 47 CF 74 3B 4B 28 AA C2 " +
            "DD E6 61 3B DF 82 09 1C 6C 48 3D 62 DD 67 AC B5 " +
            "B3 24 8D 20 ED 32 47 40 7F 2D 2D D9 A4 31 37 64 " +
            "F3 BD FA 6F F5 D6 6A 4A D4 2D A0 23 42 8D CE 59 " +
            "5B 02 5E 97 E3 6A 31 2B",
            // Round #14
            // After Theta
            "A0 45 7A 89 CC 22 31 A6 62 28 30 59 D4 C1 3F 55 " +
            "9B C4 3B 85 8A AA AA 09 89 F6 97 BB 53 0D A5 85 " +
            "8A 02 D1 45 62 7F 88 F2 42 B1 59 04 E0 44 7D 5F " +
            "AC 1C B4 5C F8 0F DB 32 78 0B 46 D2 72 DD AB 0E " +
            "94 98 57 53 7E 01 44 D3 13 56 1F CA 10 A1 2B 0C " +
            "3C 72 6A 33 A4 4B F2 70 20 48 28 98 1C 6D DC 11 " +
            "2F D2 F1 42 37 7A E4 7F 36 60 CD 8D 1D F2 F8 3E " +
            "FE 32 E0 DD 22 B8 35 F6 0D 2A C8 79 D1 06 7B 3C " +
            "9D EE 58 7A 50 C7 2C 02 A3 A8 D3 A4 A2 B3 04 BD " +
            "8B 4F 12 96 64 94 11 06 D9 34 9B 7C A2 B3 6E 69 " +
            "4E 0A D5 E4 74 26 C8 2A 26 B4 6B 83 21 F9 A3 8E " +
            "17 DA 5D F0 1C 4D C4 35 82 84 D3 8E F9 9B D6 43 " +
            "EE 7E F8 89 9C BE F3 F7",
            // After Rho
            "A0 45 7A 89 CC 22 31 A6 C4 50 60 B2 A8 83 7F AA " +
            "26 F1 4E A1 A2 AA 6A C2 D5 50 5A 98 68 7F B9 3B " +
            "FB 43 94 57 14 88 2E 12 00 4E D4 F7 25 14 9B 45 " +
            "CB 85 FF B0 2D C3 CA 41 03 DE 82 91 B4 5C F7 AA " +
            "CC AB 29 BF 00 A2 69 4A BA C2 30 61 F5 A1 0C 11 " +
            "E3 91 53 9B 21 5D 92 87 47 80 20 A1 60 72 B4 71 " +
            "17 BA D1 23 FF 7B 91 8E E4 F1 7D 6C C0 9A 1B 3B " +
            "6E 11 DC 1A 7B 7F 19 F0 F3 A2 0D F6 78 1A 54 90 " +
            "4B 0F EA 98 45 A0 D3 1D 82 DE 51 D4 69 52 D1 59 " +
            "32 C2 60 F1 49 C2 92 8C 69 D9 34 9B 7C A2 B3 6E " +
            "20 AB 38 29 54 93 D3 99 9A D0 AE 0D 86 E4 8F 3A " +
            "42 BB 0B 9E A3 89 B8 E6 84 D3 8E F9 9B D6 43 82 " +
            "FC BD BB 1F 7E 22 A7 EF",
            // After Pi
            "A0 45 7A 89 CC 22 31 A6 CB 85 FF B0 2D C3 CA 41 " +
            "17 BA D1 23 FF 7B 91 8E 32 C2 60 F1 49 C2 92 8C " +
            "FC BD BB 1F 7E 22 A7 EF D5 50 5A 98 68 7F B9 3B " +
            "BA C2 30 61 F5 A1 0C 11 E3 91 53 9B 21 5D 92 87 " +
            "4B 0F EA 98 45 A0 D3 1D 42 BB 0B 9E A3 89 B8 E6 " +
            "C4 50 60 B2 A8 83 7F AA 03 DE 82 91 B4 5C F7 AA " +
            "E4 F1 7D 6C C0 9A 1B 3B 69 D9 34 9B 7C A2 B3 6E " +
            "20 AB 38 29 54 93 D3 99 FB 43 94 57 14 88 2E 12 " +
            "00 4E D4 F7 25 14 9B 45 47 80 20 A1 60 72 B4 71 " +
            "82 DE 51 D4 69 52 D1 59 84 D3 8E F9 9B D6 43 82 " +
            "26 F1 4E A1 A2 AA 6A C2 CC AB 29 BF 00 A2 69 4A " +
            "6E 11 DC 1A 7B 7F 19 F0 F3 A2 0D F6 78 1A 54 90 " +
            "9A D0 AE 0D 86 E4 8F 3A",
            // After Chi
            "B4 7F 7A 8A 1E 1A 20 28 EB C5 DF 60 2D 43 C8 41 " +
            "DB 87 4A 2D C9 5B B4 ED 32 82 20 71 C9 C2 82 8C " +
            "B7 3D 3E 2F 5F E3 6D AE 94 41 19 02 68 23 2B BD " +
            "B2 CC 98 61 B1 01 4D 09 E3 21 52 9D 83 54 BA 65 " +
            "DE 4F BA 98 0D D6 D2 04 68 39 2B FF 36 09 BC E6 " +
            "20 71 1D DE E8 01 77 BB 0A D6 82 02 88 7C 57 EE " +
            "E4 D3 75 4C C0 8B 5B AA AD 89 74 09 D4 A2 9F 4C " +
            "23 25 BA 28 40 CF 53 99 BC C3 B4 57 54 EA 0A 22 " +
            "80 10 85 A3 2C 14 DA 4D 43 81 AE 88 F2 F6 B6 F3 " +
            "F9 DE 41 D2 6D 5A FD 49 84 DF CE 59 BA C2 D2 C7 " +
            "04 E1 9A A1 D9 F7 7A 72 5D 09 28 5B 00 A2 2D 4A " +
            "66 41 7E 13 FD 9B 92 DA D7 83 4D 56 58 10 34 50 " +
            "52 DA 8F 13 86 E4 8E 32",
            // After Iota 
            "3D FF 7A 8A 1E 1A 20 A8 EB C5 DF 60 2D 43 C8 41 " +
            "DB 87 4A 2D C9 5B B4 ED 32 82 20 71 C9 C2 82 8C " +
            "B7 3D 3E 2F 5F E3 6D AE 94 41 19 02 68 23 2B BD " +
            "B2 CC 98 61 B1 01 4D 09 E3 21 52 9D 83 54 BA 65 " +
            "DE 4F BA 98 0D D6 D2 04 68 39 2B FF 36 09 BC E6 " +
            "20 71 1D DE E8 01 77 BB 0A D6 82 02 88 7C 57 EE " +
            "E4 D3 75 4C C0 8B 5B AA AD 89 74 09 D4 A2 9F 4C " +
            "23 25 BA 28 40 CF 53 99 BC C3 B4 57 54 EA 0A 22 " +
            "80 10 85 A3 2C 14 DA 4D 43 81 AE 88 F2 F6 B6 F3 " +
            "F9 DE 41 D2 6D 5A FD 49 84 DF CE 59 BA C2 D2 C7 " +
            "04 E1 9A A1 D9 F7 7A 72 5D 09 28 5B 00 A2 2D 4A " +
            "66 41 7E 13 FD 9B 92 DA D7 83 4D 56 58 10 34 50 " +
            "52 DA 8F 13 86 E4 8E 32",
            // Round #15
            // After Theta
            "0A 56 45 CE 7A 09 B5 CE 28 43 F4 0F 34 B5 27 A9 " +
            "8A 73 E6 1F BB 2B 9C F6 9F 7F 41 73 67 2D 4F CE " +
            "BB FE 7D 0B 5D 55 73 8F A3 E8 26 46 0C 30 BE DB " +
            "71 4A B3 0E A8 F7 A2 E1 B2 D5 FE AF F1 24 92 7E " +
            "73 B2 DB 9A A3 39 1F 46 64 FA 68 DB 34 BF A2 C7 " +
            "17 D8 22 9A 8C 12 E2 DD C9 50 A9 6D 91 8A B8 06 " +
            "B5 27 D9 7E B2 FB 73 B1 00 74 15 0B 7A 4D 52 0E " +
            "2F E6 F9 0C 42 79 4D B8 8B 6A 8B 13 30 F9 9F 44 " +
            "43 96 AE CC 35 E2 35 A5 12 75 02 BA 80 86 9E E8 " +
            "54 23 20 D0 C3 B5 30 0B 88 1C 8D 7D B8 74 CC E6 " +
            "33 48 A5 E5 BD E4 EF 14 9E 8F 03 34 19 54 C2 A2 " +
            "37 B5 D2 21 8F EB BA C1 7A 7E 2C 54 F6 FF F9 12 " +
            "5E 19 CC 37 84 52 90 13",
            // After Rho
            "0A 56 45 CE 7A 09 B5 CE 51 86 E8 1F 68 6A 4F 52 " +
            "E2 9C F9 C7 EE 0A A7 BD D6 F2 E4 FC F9 17 34 77 " +
            "AA 9A 7B DC F5 EF 5B E8 C4 00 E3 BB 3D 8A 6E 62 " +
            "EB 80 7A 2F 1A 1E A7 34 9F 6C B5 FF 6B 3C 89 A4 " +
            "D9 6D CD D1 9C 0F A3 39 2B 7A 4C A6 8F B6 4D F3 " +
            "BE C0 16 D1 64 94 10 EF 1A 24 43 A5 B6 45 2A E2 " +
            "F6 93 DD 9F 8B AD 3D C9 9A A4 1C 00 E8 2A 16 F4 " +
            "06 A1 BC 26 DC 17 F3 7C 27 60 F2 3F 89 16 D5 16 " +
            "95 B9 46 BC A6 74 C8 D2 4F 74 89 3A 01 5D 40 43 " +
            "16 66 81 6A 04 04 7A B8 E6 88 1C 8D 7D B8 74 CC " +
            "BF 53 CC 20 95 96 F7 92 7A 3E 0E D0 64 50 09 8B " +
            "A6 56 3A E4 71 5D 37 F8 7E 2C 54 F6 FF F9 12 7A " +
            "E4 84 57 06 F3 0D A1 14",
            // After Pi
            "0A 56 45 CE 7A 09 B5 CE EB 80 7A 2F 1A 1E A7 34 " +
            "F6 93 DD 9F 8B AD 3D C9 16 66 81 6A 04 04 7A B8 " +
            "E4 84 57 06 F3 0D A1 14 D6 F2 E4 FC F9 17 34 77 " +
            "2B 7A 4C A6 8F B6 4D F3 BE C0 16 D1 64 94 10 EF " +
            "95 B9 46 BC A6 74 C8 D2 A6 56 3A E4 71 5D 37 F8 " +
            "51 86 E8 1F 68 6A 4F 52 9F 6C B5 FF 6B 3C 89 A4 " +
            "9A A4 1C 00 E8 2A 16 F4 E6 88 1C 8D 7D B8 74 CC " +
            "BF 53 CC 20 95 96 F7 92 AA 9A 7B DC F5 EF 5B E8 " +
            "C4 00 E3 BB 3D 8A 6E 62 1A 24 43 A5 B6 45 2A E2 " +
            "4F 74 89 3A 01 5D 40 43 7E 2C 54 F6 FF F9 12 7A " +
            "E2 9C F9 C7 EE 0A A7 BD D9 6D CD D1 9C 0F A3 39 " +
            "06 A1 BC 26 DC 17 F3 7C 27 60 F2 3F 89 16 D5 16 " +
            "7A 3E 0E D0 64 50 09 8B",
            // After Chi
            "1E 45 C0 5E FB A8 AD 07 EB E4 7A 4F 1E 1E E5 04 " +
            "16 13 8B 9B 78 A4 BC CD 1C 34 81 A2 0C 04 6E 72 " +
            "05 04 6D 27 F3 1B A3 24 42 72 F6 AD 99 17 24 7B " +
            "2A 43 0C 8A 0D D6 85 E3 9C 86 2E 91 35 9D 27 C7 " +
            "C5 19 82 A4 2E 76 C8 D5 8F 5E 32 E6 77 FD 7E 78 " +
            "51 06 E0 1F E8 68 59 02 FB 64 B5 72 7E AC E9 AC " +
            "83 F7 DC 20 68 2C 95 E6 A6 0C 3C 92 15 D0 7C 8C " +
            "31 3B D9 C0 96 82 77 36 B0 BE 7B D8 77 AA 5B 68 " +
            "81 50 6B A1 3C 92 2E 63 2A 2C 17 61 48 E5 38 DA " +
            "CF E6 A2 32 01 5B 09 C3 3A 2C D4 D5 F7 F9 36 78 " +
            "E4 1C C9 E1 AE 1A F7 F9 F8 2D 8F C8 9D 0F A7 3B " +
            "5E BF B0 E6 B8 57 FB F5 A7 E0 03 38 03 1C 73 22 " +
            "63 5F 0A C0 74 55 09 8B",
            // After Iota 
            "1D C5 C0 5E FB A8 AD 87 EB E4 7A 4F 1E 1E E5 04 " +
            "16 13 8B 9B 78 A4 BC CD 1C 34 81 A2 0C 04 6E 72 " +
            "05 04 6D 27 F3 1B A3 24 42 72 F6 AD 99 17 24 7B " +
            "2A 43 0C 8A 0D D6 85 E3 9C 86 2E 91 35 9D 27 C7 " +
            "C5 19 82 A4 2E 76 C8 D5 8F 5E 32 E6 77 FD 7E 78 " +
            "51 06 E0 1F E8 68 59 02 FB 64 B5 72 7E AC E9 AC " +
            "83 F7 DC 20 68 2C 95 E6 A6 0C 3C 92 15 D0 7C 8C " +
            "31 3B D9 C0 96 82 77 36 B0 BE 7B D8 77 AA 5B 68 " +
            "81 50 6B A1 3C 92 2E 63 2A 2C 17 61 48 E5 38 DA " +
            "CF E6 A2 32 01 5B 09 C3 3A 2C D4 D5 F7 F9 36 78 " +
            "E4 1C C9 E1 AE 1A F7 F9 F8 2D 8F C8 9D 0F A7 3B " +
            "5E BF B0 E6 B8 57 FB F5 A7 E0 03 38 03 1C 73 22 " +
            "63 5F 0A C0 74 55 09 8B",
            // Round #16
            // After Theta
            "79 AB D7 F6 F3 93 39 38 4A 15 A3 C1 E6 36 02 EC " +
            "7A E3 90 78 DF 97 FD 4B A4 E0 EF 27 FB 32 88 82 " +
            "A6 05 3B 13 61 30 FB 30 26 1C E1 05 91 2C B0 C4 " +
            "8B B2 D5 04 F5 FE 62 0B F0 76 35 72 92 AE 66 41 " +
            "7D CD EC 21 D9 40 2E 25 2C 5F 64 D2 E5 D6 26 6C " +
            "35 68 F7 B7 E0 53 CD BD 5A 95 6C FC 86 84 0E 44 " +
            "EF 07 C7 C3 CF 1F D4 60 1E D8 52 17 E2 E6 9A 7C " +
            "92 3A 8F F4 04 A9 2F 22 D4 D0 6C 70 7F 91 CF D7 " +
            "20 A1 B2 2F C4 BA C9 8B 46 DC 0C 82 EF D6 79 5C " +
            "77 32 CC B7 F6 6D EF 33 99 2D 82 E1 65 D2 6E 6C " +
            "80 72 DE 49 A6 21 63 46 59 DC 56 46 65 27 40 D3 " +
            "32 4F AB 05 1F 64 BA 73 1F 34 6D BD F4 2A 95 D2 " +
            "C0 5E 5C F4 E6 7E 51 9F",
            // After Rho
            "79 AB D7 F6 F3 93 39 38 95 2A 46 83 CD 6D 04 D8 " +
            "DE 38 24 DE F7 65 FF 92 2F 83 28 48 0A FE 7E B2 " +
            "83 D9 87 31 2D D8 99 08 10 C9 02 4B 6C C2 11 5E " +
            "4D 50 EF 2F B6 B0 28 5B 10 BC 5D 8D 9C A4 AB 59 " +
            "66 F6 90 6C 20 97 92 BE 6D C2 C6 F2 45 26 5D 6E " +
            "AD 41 BB BF 05 9F 6A EE 10 69 55 B2 F1 1B 12 3A " +
            "1E 7E FE A0 06 7B 3F 38 CD 35 F9 3C B0 A5 2E C4 " +
            "7A 82 D4 17 11 49 9D 47 E0 FE 22 9F AF A9 A1 D9 " +
            "F6 85 58 37 79 11 24 54 3C 2E 23 6E 06 C1 77 EB " +
            "ED 7D E6 4E 86 F9 D6 BE 6C 99 2D 82 E1 65 D2 6E " +
            "8C 19 01 CA 79 27 99 86 67 71 5B 19 95 9D 00 4D " +
            "E6 69 B5 E0 83 4C 77 4E 34 6D BD F4 2A 95 D2 1F " +
            "D4 27 B0 17 17 BD B9 5F",
            // After Pi
            "79 AB D7 F6 F3 93 39 38 4D 50 EF 2F B6 B0 28 5B " +
            "1E 7E FE A0 06 7B 3F 38 ED 7D E6 4E 86 F9 D6 BE " +
            "D4 27 B0 17 17 BD B9 5F 2F 83 28 48 0A FE 7E B2 " +
            "6D C2 C6 F2 45 26 5D 6E AD 41 BB BF 05 9F 6A EE " +
            "F6 85 58 37 79 11 24 54 E6 69 B5 E0 83 4C 77 4E " +
            "95 2A 46 83 CD 6D 04 D8 10 BC 5D 8D 9C A4 AB 59 " +
            "CD 35 F9 3C B0 A5 2E C4 6C 99 2D 82 E1 65 D2 6E " +
            "8C 19 01 CA 79 27 99 86 83 D9 87 31 2D D8 99 08 " +
            "10 C9 02 4B 6C C2 11 5E 10 69 55 B2 F1 1B 12 3A " +
            "3C 2E 23 6E 06 C1 77 EB 34 6D BD F4 2A 95 D2 1F " +
            "DE 38 24 DE F7 65 FF 92 66 F6 90 6C 20 97 92 BE " +
            "7A 82 D4 17 11 49 9D 47 E0 FE 22 9F AF A9 A1 D9 " +
            "67 71 5B 19 95 9D 00 4D",
            // After Chi
            "6B 85 C7 76 F3 D8 2E 18 AC 51 EF 61 36 30 E8 DD " +
            "0E 7C EE B1 17 7F 16 79 C4 F5 A1 AE 66 FB D6 9E " +
            "D0 77 98 1E 13 9D B9 1C AF 82 11 45 0A 67 5C 32 " +
            "3F 46 86 F2 3D 26 59 7E AD 29 1E 7F 87 D3 39 E4 " +
            "FF 07 50 3F 71 A3 2C E4 A6 29 73 52 C6 4C 76 02 " +
            "58 2B E6 B3 ED 6C 00 5C 30 34 59 0F DD E4 7B 73 " +
            "4D 35 F9 74 A8 A7 27 44 7D BB 6B 83 65 2D D6 36 " +
            "8C 8D 18 C6 69 A7 32 87 83 F9 D2 81 BC C1 9B 28 " +
            "3C CF 20 07 6A 02 74 9F 10 28 C9 22 D9 0F 92 2E " +
            "BF BE 21 6F 03 89 7E EB 24 6D BD BE 6A 97 D2 49 " +
            "C6 38 60 CD E6 2D F2 D3 E6 8A B2 E4 8E 37 B2 26 " +
            "7D 83 8D 17 01 5D 9D 43 78 F6 06 59 CD C9 5E 4B " +
            "47 B7 CB 39 95 0F 00 61",
            // After Iota 
            "69 05 C7 76 F3 D8 2E 98 AC 51 EF 61 36 30 E8 DD " +
            "0E 7C EE B1 17 7F 16 79 C4 F5 A1 AE 66 FB D6 9E " +
            "D0 77 98 1E 13 9D B9 1C AF 82 11 45 0A 67 5C 32 " +
            "3F 46 86 F2 3D 26 59 7E AD 29 1E 7F 87 D3 39 E4 " +
            "FF 07 50 3F 71 A3 2C E4 A6 29 73 52 C6 4C 76 02 " +
            "58 2B E6 B3 ED 6C 00 5C 30 34 59 0F DD E4 7B 73 " +
            "4D 35 F9 74 A8 A7 27 44 7D BB 6B 83 65 2D D6 36 " +
            "8C 8D 18 C6 69 A7 32 87 83 F9 D2 81 BC C1 9B 28 " +
            "3C CF 20 07 6A 02 74 9F 10 28 C9 22 D9 0F 92 2E " +
            "BF BE 21 6F 03 89 7E EB 24 6D BD BE 6A 97 D2 49 " +
            "C6 38 60 CD E6 2D F2 D3 E6 8A B2 E4 8E 37 B2 26 " +
            "7D 83 8D 17 01 5D 9D 43 78 F6 06 59 CD C9 5E 4B " +
            "47 B7 CB 39 95 0F 00 61",
            // Round #17
            // After Theta
            "02 C0 06 84 D4 B8 18 FB 70 AB F6 B3 B9 BC FD B8 " +
            "74 19 36 87 5D D3 02 C8 74 2D E6 3A 00 7E 8E 48 " +
            "E7 AD 21 A3 32 D6 83 EA C4 47 D0 B7 2D 07 6A 51 " +
            "E3 BC 9F 20 B2 AA 4C 1B D7 4C C6 49 CD 7F 2D 55 " +
            "4F DF 17 AB 17 26 74 32 91 F3 CA EF E7 07 4C F4 " +
            "33 EE 27 41 CA 0C 36 3F EC CE 40 DD 52 68 6E 16 " +
            "37 50 21 42 E2 0B 33 F5 CD 63 2C 17 03 A8 8E E0 " +
            "BB 57 A1 7B 48 EC 08 71 E8 3C 13 73 9B A1 AD 4B " +
            "E0 35 39 D5 E5 8E 61 FA 6A 4D 11 14 93 A3 86 9F " +
            "0F 66 66 FB 65 0C 26 3D 13 B7 04 03 4B DC E8 BF " +
            "AD FD A1 3F C1 4D C4 B0 3A 70 AB 36 01 BB A7 43 " +
            "07 E6 55 21 4B F1 89 F2 C8 2E 41 CD AB 4C 06 9D " +
            "70 6D 72 84 B4 44 3A 97",
            // After Rho
            "02 C0 06 84 D4 B8 18 FB E1 56 ED 67 73 79 FB 71 " +
            "5D 86 CD 61 D7 B4 00 32 E0 E7 88 44 D7 62 AE 03 " +
            "B1 1E 54 3F 6F 0D 19 95 DB 72 A0 16 45 7C 04 7D " +
            "09 22 AB CA B4 31 CE FB D5 35 93 71 52 F3 5F 4B " +
            "EF 8B D5 0B 13 3A 99 A7 C0 44 1F 39 AF FC 7E 7E " +
            "99 71 3F 09 52 66 B0 F9 59 B0 3B 03 75 4B A1 B9 " +
            "11 12 5F 98 A9 BF 81 0A 50 1D C1 9B C7 58 2E 06 " +
            "3D 24 76 84 B8 DD AB D0 E6 36 43 5B 97 D0 79 26 " +
            "A7 BA DC 31 4C 1F BC 26 C3 4F B5 A6 08 8A C9 51 " +
            "C1 A4 E7 C1 CC 6C BF 8C BF 13 B7 04 03 4B DC E8 " +
            "11 C3 B6 F6 87 FE 04 37 E9 C0 AD DA 04 EC 9E 0E " +
            "C0 BC 2A 64 29 3E 51 FE 2E 41 CD AB 4C 06 9D C8 " +
            "CE 25 5C 9B 1C 21 2D 91",
            // After Pi
            "02 C0 06 84 D4 B8 18 FB 09 22 AB CA B4 31 CE FB " +
            "11 12 5F 98 A9 BF 81 0A C1 A4 E7 C1 CC 6C BF 8C " +
            "CE 25 5C 9B 1C 21 2D 91 E0 E7 88 44 D7 62 AE 03 " +
            "C0 44 1F 39 AF FC 7E 7E 99 71 3F 09 52 66 B0 F9 " +
            "A7 BA DC 31 4C 1F BC 26 C0 BC 2A 64 29 3E 51 FE " +
            "E1 56 ED 67 73 79 FB 71 D5 35 93 71 52 F3 5F 4B " +
            "50 1D C1 9B C7 58 2E 06 BF 13 B7 04 03 4B DC E8 " +
            "11 C3 B6 F6 87 FE 04 37 B1 1E 54 3F 6F 0D 19 95 " +
            "DB 72 A0 16 45 7C 04 7D 59 B0 3B 03 75 4B A1 B9 " +
            "C3 4F B5 A6 08 8A C9 51 2E 41 CD AB 4C 06 9D C8 " +
            "5D 86 CD 61 D7 B4 00 32 EF 8B D5 0B 13 3A 99 A7 " +
            "3D 24 76 84 B8 DD AB D0 E6 36 43 5B 97 D0 79 26 " +
            "E9 C0 AD DA 04 EC 9E 0E",
            // After Chi
            "12 D0 52 94 DD 36 19 FB C9 86 0B 8B F0 71 F0 7F " +
            "1F 13 47 82 B9 BE 81 1B C1 64 E5 C5 0C F4 AF E6 " +
            "C7 07 F5 D1 3C 20 EB 91 F9 D6 A8 44 87 60 2E 82 " +
            "E6 CE DF 09 A3 E5 72 78 D9 75 1D 4D 73 46 F1 21 " +
            "87 F9 5C 31 9A 5F 12 27 C0 BC 3D 5D 01 A2 01 82 " +
            "E1 5E AD ED F6 71 DB 75 7A 37 A5 75 52 F0 8F A3 " +
            "50 DD C1 69 43 EC 2E 11 5F 07 FE 05 73 4A 27 A8 " +
            "05 E2 A4 E6 87 7C 00 3D B1 9E 4F 3E 5F 0E B8 15 " +
            "59 3D 24 B2 4D FC 4C 3D 75 B0 73 0A 31 4F B5 31 " +
            "52 51 A5 B2 2B 83 C9 44 64 21 6D AB 4C 76 99 A0 " +
            "4D A2 EF E5 7F 71 22 62 2D 99 D4 50 14 3A C9 81 " +
            "34 E4 DA 04 B8 F1 2D D8 F2 30 03 7A 44 C0 79 16 " +
            "4B C9 BD D0 04 E6 07 8B",
            // After Iota 
            "92 D0 52 94 DD 36 19 7B C9 86 0B 8B F0 71 F0 7F " +
            "1F 13 47 82 B9 BE 81 1B C1 64 E5 C5 0C F4 AF E6 " +
            "C7 07 F5 D1 3C 20 EB 91 F9 D6 A8 44 87 60 2E 82 " +
            "E6 CE DF 09 A3 E5 72 78 D9 75 1D 4D 73 46 F1 21 " +
            "87 F9 5C 31 9A 5F 12 27 C0 BC 3D 5D 01 A2 01 82 " +
            "E1 5E AD ED F6 71 DB 75 7A 37 A5 75 52 F0 8F A3 " +
            "50 DD C1 69 43 EC 2E 11 5F 07 FE 05 73 4A 27 A8 " +
            "05 E2 A4 E6 87 7C 00 3D B1 9E 4F 3E 5F 0E B8 15 " +
            "59 3D 24 B2 4D FC 4C 3D 75 B0 73 0A 31 4F B5 31 " +
            "52 51 A5 B2 2B 83 C9 44 64 21 6D AB 4C 76 99 A0 " +
            "4D A2 EF E5 7F 71 22 62 2D 99 D4 50 14 3A C9 81 " +
            "34 E4 DA 04 B8 F1 2D D8 F2 30 03 7A 44 C0 79 16 " +
            "4B C9 BD D0 04 E6 07 8B",
            // Round #18
            // After Theta
            "FD D7 ED AE 9F 1C 7C 4F 10 3D 99 3D 7D 7D 0B 01 " +
            "4C 3F 05 E4 F5 59 5C 75 4C E9 AE 4E E8 83 81 2E " +
            "93 34 FA 25 AF 33 2D 5C 96 D1 17 7E C5 4A 4B B6 " +
            "3F 75 4D BF 2E E9 89 06 8A 59 5F 2B 3F A1 2C 4F " +
            "0A 74 17 BA 7E 28 3C EF 94 8F 32 A9 92 B1 C7 4F " +
            "8E 59 12 D7 B4 5B BE 41 A3 8C 37 C3 DF FC 74 DD " +
            "03 F1 83 0F 0F 0B F3 7F D2 8A B5 8E 97 3D 09 60 " +
            "51 D1 AB 12 14 6F C6 F0 DE 99 F0 04 1D 24 DD 21 " +
            "80 86 B6 04 C0 F0 B7 43 26 9C 31 6C 7D A8 68 5F " +
            "DF DC EE 39 CF F4 E7 8C 30 12 62 5F DF 65 5F 6D " +
            "22 A5 50 DF 3D 5B 47 56 F4 22 46 E6 99 36 32 FF " +
            "67 C8 98 62 F4 16 F0 B6 7F BD 48 F1 A0 B7 57 DE " +
            "1F FA B2 24 97 F5 C1 46",
            // After Rho
            "FD D7 ED AE 9F 1C 7C 4F 20 7A 32 7B FA FA 16 02 " +
            "D3 4F 01 79 7D 16 57 1D 3E 18 E8 C2 94 EE EA 84 " +
            "9D 69 E1 9A A4 D1 2F 79 57 AC B4 64 6B 19 7D E1 " +
            "F4 EB 92 9E 68 F0 53 D7 93 62 D6 D7 CA 4F 28 CB " +
            "BA 0B 5D 3F 14 9E 77 05 7B FC 44 F9 28 93 2A 19 " +
            "72 CC 92 B8 A6 DD F2 0D 75 8F 32 DE 0C 7F F3 D3 " +
            "7C 78 58 98 FF 1B 88 1F 7B 12 C0 A4 15 6B 1D 2F " +
            "09 8A 37 63 F8 A8 E8 55 09 3A 48 BA 43 BC 33 E1 " +
            "96 00 18 FE 76 08 D0 D0 B4 2F 13 CE 18 B6 3E 54 " +
            "FE 9C F1 9B DB 3D E7 99 6D 30 12 62 5F DF 65 5F " +
            "1D 59 89 94 42 7D F7 6C D3 8B 18 99 67 DA C8 FC " +
            "0C 19 53 8C DE 02 DE F6 BD 48 F1 A0 B7 57 DE 7F " +
            "B0 D1 87 BE 2C C9 65 7D",
            // After Pi
            "FD D7 ED AE 9F 1C 7C 4F F4 EB 92 9E 68 F0 53 D7 " +
            "7C 78 58 98 FF 1B 88 1F FE 9C F1 9B DB 3D E7 99 " +
            "B0 D1 87 BE 2C C9 65 7D 3E 18 E8 C2 94 EE EA 84 " +
            "7B FC 44 F9 28 93 2A 19 72 CC 92 B8 A6 DD F2 0D " +
            "96 00 18 FE 76 08 D0 D0 0C 19 53 8C DE 02 DE F6 " +
            "20 7A 32 7B FA FA 16 02 93 62 D6 D7 CA 4F 28 CB " +
            "7B 12 C0 A4 15 6B 1D 2F 6D 30 12 62 5F DF 65 5F " +
            "1D 59 89 94 42 7D F7 6C 9D 69 E1 9A A4 D1 2F 79 " +
            "57 AC B4 64 6B 19 7D E1 75 8F 32 DE 0C 7F F3 D3 " +
            "B4 2F 13 CE 18 B6 3E 54 BD 48 F1 A0 B7 57 DE 7F " +
            "D3 4F 01 79 7D 16 57 1D BA 0B 5D 3F 14 9E 77 05 " +
            "09 8A 37 63 F8 A8 E8 55 09 3A 48 BA 43 BC 33 E1 " +
            "D3 8B 18 99 67 DA C8 FC",
            // After Chi
            "F5 C7 A5 AE 08 17 F4 47 76 6F 33 9D 68 D4 34 57 " +
            "7C 39 5E BC DB DB 88 7B B3 9A 99 9B 48 29 FF 9B " +
            "B0 F9 95 AE 4C 29 66 ED 3E 18 7A C2 12 A2 3A 80 " +
            "FF FC 4C BF 78 93 2A C9 7A D5 D1 B8 2E DF FC 2B " +
            "A4 00 B0 BC 76 E4 F0 D0 4D FD 57 B5 F6 13 DE EF " +
            "48 6A 32 5B EF DA 03 26 97 42 C4 95 80 DB 48 9B " +
            "6B 5B 49 30 15 4B 8F 0F 4D 12 20 09 E7 5D 65 5D " +
            "8E 59 4D 10 42 78 DF A5 BD 6A E3 00 A0 B7 AD 6B " +
            "D7 8C B5 64 7B 99 71 E5 7C CF D2 FE AB 3E 33 F8 " +
            "B4 0E 13 D4 18 36 1F 54 FF CC E5 C4 FC 5F 8E FF " +
            "D2 CF 23 39 95 36 DF 4D BA 3B 15 A7 17 8A 64 A5 " +
            "DB 0B 27 62 DC EA 20 49 09 7E 49 DA 5B B8 24 E0 " +
            "FB 8B 44 9F 67 52 E8 FC",
            // After Iota 
            "FF 47 A5 AE 08 17 F4 47 76 6F 33 9D 68 D4 34 57 " +
            "7C 39 5E BC DB DB 88 7B B3 9A 99 9B 48 29 FF 9B " +
            "B0 F9 95 AE 4C 29 66 ED 3E 18 7A C2 12 A2 3A 80 " +
            "FF FC 4C BF 78 93 2A C9 7A D5 D1 B8 2E DF FC 2B " +
            "A4 00 B0 BC 76 E4 F0 D0 4D FD 57 B5 F6 13 DE EF " +
            "48 6A 32 5B EF DA 03 26 97 42 C4 95 80 DB 48 9B " +
            "6B 5B 49 30 15 4B 8F 0F 4D 12 20 09 E7 5D 65 5D " +
            "8E 59 4D 10 42 78 DF A5 BD 6A E3 00 A0 B7 AD 6B " +
            "D7 8C B5 64 7B 99 71 E5 7C CF D2 FE AB 3E 33 F8 " +
            "B4 0E 13 D4 18 36 1F 54 FF CC E5 C4 FC 5F 8E FF " +
            "D2 CF 23 39 95 36 DF 4D BA 3B 15 A7 17 8A 64 A5 " +
            "DB 0B 27 62 DC EA 20 49 09 7E 49 DA 5B B8 24 E0 " +
            "FB 8B 44 9F 67 52 E8 FC",
            // Round #19
            // After Theta
            "6E 91 BD 16 93 47 72 69 05 18 78 C3 87 0D 5A 4D " +
            "C0 AE E2 88 13 69 69 7A 96 DD F6 93 19 2C 15 3D " +
            "9A 20 9D 92 56 EA 48 C0 AF CE 62 7A 89 F2 BC AE " +
            "8C 8B 07 E1 97 4A 44 D3 C6 42 6D 8C E6 6D 1D 2A " +
            "81 47 DF B4 27 E1 1A 76 67 24 5F 89 EC D0 F0 C2 " +
            "D9 BC 2A E3 74 8A 85 08 E4 35 8F CB 6F 02 26 81 " +
            "D7 CC F5 04 DD F9 6E 0E 68 55 4F 01 B6 58 8F FB " +
            "A4 80 45 2C 58 BB F1 88 2C BC FB B8 3B E7 2B 45 " +
            "A4 FB FE 3A 94 40 1F FF C0 58 6E CA 63 8C D2 F9 " +
            "91 49 7C DC 49 33 F5 F2 D5 15 ED F8 E6 9C A0 D2 " +
            "43 19 3B 81 0E 66 59 63 C9 4C 5E F9 F8 53 0A BF " +
            "67 9C 9B 56 14 58 C1 48 2C 39 26 D2 0A BD CE 46 " +
            "D1 52 4C A3 7D 91 C6 D1",
            // After Rho
            "6E 91 BD 16 93 47 72 69 0A 30 F0 86 0F 1B B4 9A " +
            "B0 AB 38 E2 44 5A 9A 1E C1 52 D1 63 D9 6D 3F 99 " +
            "52 47 02 D6 04 E9 94 B4 97 28 CF EB FA EA 2C A6 " +
            "10 7E A9 44 34 CD B8 78 8A B1 50 1B A3 79 5B 87 " +
            "A3 6F DA 93 70 0D BB C0 0D 2F 7C 46 F2 95 C8 0E " +
            "C8 E6 55 19 A7 53 2C 44 04 92 D7 3C 2E BF 09 98 " +
            "27 E8 CE 77 73 B8 66 AE B1 1E F7 D1 AA 9E 02 6C " +
            "16 AC DD 78 44 52 C0 22 71 77 CE 57 8A 58 78 F7 " +
            "5F 87 12 E8 E3 9F 74 DF E9 7C 60 2C 37 E5 31 46 " +
            "A6 5E 3E 32 89 8F 3B 69 D2 D5 15 ED F8 E6 9C A0 " +
            "65 8D 0D 65 EC 04 3A 98 26 33 79 E5 E3 4F 29 FC " +
            "8C 73 D3 8A 02 2B 18 E9 39 26 D2 0A BD CE 46 2C " +
            "71 74 B4 14 D3 68 5F A4",
            // After Pi
            "6E 91 BD 16 93 47 72 69 10 7E A9 44 34 CD B8 78 " +
            "27 E8 CE 77 73 B8 66 AE A6 5E 3E 32 89 8F 3B 69 " +
            "71 74 B4 14 D3 68 5F A4 C1 52 D1 63 D9 6D 3F 99 " +
            "0D 2F 7C 46 F2 95 C8 0E C8 E6 55 19 A7 53 2C 44 " +
            "5F 87 12 E8 E3 9F 74 DF 8C 73 D3 8A 02 2B 18 E9 " +
            "0A 30 F0 86 0F 1B B4 9A 8A B1 50 1B A3 79 5B 87 " +
            "B1 1E F7 D1 AA 9E 02 6C D2 D5 15 ED F8 E6 9C A0 " +
            "65 8D 0D 65 EC 04 3A 98 52 47 02 D6 04 E9 94 B4 " +
            "97 28 CF EB FA EA 2C A6 04 92 D7 3C 2E BF 09 98 " +
            "E9 7C 60 2C 37 E5 31 46 39 26 D2 0A BD CE 46 2C " +
            "B0 AB 38 E2 44 5A 9A 1E A3 6F DA 93 70 0D BB C0 " +
            "16 AC DD 78 44 52 C0 22 71 77 CE 57 8A 58 78 F7 " +
            "26 33 79 E5 E3 4F 29 FC",
            // After Chi
            "49 11 FB 25 D0 77 34 EF 90 68 99 44 BC CA A1 39 " +
            "76 C8 4E 73 21 D8 22 2A A8 DF 37 30 89 88 1B 20 " +
            "61 1A B4 54 F7 E0 D7 B4 01 92 D0 7A DC 2F 1B D9 " +
            "1A 2E 7E A6 B2 19 98 95 48 96 94 1B A7 73 24 64 " +
            "1E 87 12 89 3A DB 53 CF 80 5E FF 8E 20 BB D8 EF " +
            "3B 3E 57 46 07 9D B4 F2 C8 70 50 37 F3 19 C7 07 " +
            "94 16 FF D1 AE 9E 20 74 D8 E5 E5 6F FB FD 18 A2 " +
            "E5 0C 0D 7C 4C 64 71 9D 52 D5 12 C2 00 FC 95 AC " +
            "7E 44 EF EB EB AA 1C E0 14 90 45 3E A6 B5 4F B0 " +
            "AB 3D 60 F8 37 C4 A1 D6 BC 0E 1F 23 47 CC 6E 2E " +
            "A4 2B 3D 8A 40 08 DA 3C C2 3C D8 94 FA 05 83 15 " +
            "10 AC EC D8 25 55 C1 2A E1 FF CE 55 8E 48 EA F5 " +
            "25 77 BB F4 D3 4A 08 3C",
            // After Iota 
            "43 11 FB A5 D0 77 34 6F 90 68 99 44 BC CA A1 39 " +
            "76 C8 4E 73 21 D8 22 2A A8 DF 37 30 89 88 1B 20 " +
            "61 1A B4 54 F7 E0 D7 B4 01 92 D0 7A DC 2F 1B D9 " +
            "1A 2E 7E A6 B2 19 98 95 48 96 94 1B A7 73 24 64 " +
            "1E 87 12 89 3A DB 53 CF 80 5E FF 8E 20 BB D8 EF " +
            "3B 3E 57 46 07 9D B4 F2 C8 70 50 37 F3 19 C7 07 " +
            "94 16 FF D1 AE 9E 20 74 D8 E5 E5 6F FB FD 18 A2 " +
            "E5 0C 0D 7C 4C 64 71 9D 52 D5 12 C2 00 FC 95 AC " +
            "7E 44 EF EB EB AA 1C E0 14 90 45 3E A6 B5 4F B0 " +
            "AB 3D 60 F8 37 C4 A1 D6 BC 0E 1F 23 47 CC 6E 2E " +
            "A4 2B 3D 8A 40 08 DA 3C C2 3C D8 94 FA 05 83 15 " +
            "10 AC EC D8 25 55 C1 2A E1 FF CE 55 8E 48 EA F5 " +
            "25 77 BB F4 D3 4A 08 3C",
            // Round #20
            // After Theta
            "22 BD 19 81 06 05 EE 07 42 C2 D2 2A A1 50 24 AC " +
            "C0 78 12 2F 2F F8 75 A8 3D C8 7F 8C 3C 2F 82 28 " +
            "5A E2 7C 8D 91 A0 64 73 60 3E 32 5E 0A 5D C1 B1 " +
            "C8 84 35 C8 AF 83 1D 00 FE 26 C8 47 A9 53 73 E6 " +
            "8B 90 5A 35 8F 7C CA C7 BB A6 37 57 46 FB 6B 28 " +
            "5A 92 B5 62 D1 EF 6E 9A 1A DA 1B 59 EE 83 42 92 " +
            "22 A6 A3 8D A0 BE 77 F6 4D F2 AD D3 4E 5A 81 AA " +
            "DE F4 C5 A5 2A 24 C2 5A 33 79 F0 E6 D6 8E 4F C4 " +
            "AC EE A4 85 F6 30 99 75 A2 20 19 62 A8 95 18 32 " +
            "3E 2A 28 44 82 63 38 DE 87 F6 D7 FA 21 8C DD E9 " +
            "C5 87 DF AE 96 7A 00 54 10 96 93 FA E7 9F 06 80 " +
            "A6 1C B0 84 2B 75 96 A8 74 E8 86 E9 3B EF 73 FD " +
            "1E 8F 73 2D B5 0A BB FB",
            // After Rho
            "22 BD 19 81 06 05 EE 07 85 84 A5 55 42 A1 48 58 " +
            "30 9E C4 CB 0B 7E 1D 2A F3 22 88 D2 83 FC C7 C8 " +
            "04 25 9B D3 12 E7 6B 8C A5 D0 15 1C 0B E6 23 E3 " +
            "83 FC 3A D8 01 80 4C 58 B9 BF 09 F2 51 EA D4 9C " +
            "48 AD 9A 47 3E E5 E3 45 BF 86 B2 6B 7A 73 65 B4 " +
            "D4 92 AC 15 8B 7E 77 D3 49 6A 68 6F 64 B9 0F 0A " +
            "6D 04 F5 BD B3 17 31 1D B4 02 55 9B E4 5B A7 9D " +
            "52 15 12 61 2D 6F FA E2 CD AD 1D 9F 88 67 F2 E0 " +
            "B4 D0 1E 26 B3 8E D5 9D 0C 19 51 90 0C 31 D4 4A " +
            "0C C7 DB 47 05 85 48 70 E9 87 F6 D7 FA 21 8C DD " +
            "01 50 15 1F 7E BB 5A EA 42 58 4E EA 9F 7F 1A 00 " +
            "94 03 96 70 A5 CE 12 D5 E8 86 E9 3B EF 73 FD 74 " +
            "EE BE C7 E3 5C 4B AD C2",
            // After Pi
            "22 BD 19 81 06 05 EE 07 83 FC 3A D8 01 80 4C 58 " +
            "6D 04 F5 BD B3 17 31 1D 0C C7 DB 47 05 85 48 70 " +
            "EE BE C7 E3 5C 4B AD C2 F3 22 88 D2 83 FC C7 C8 " +
            "BF 86 B2 6B 7A 73 65 B4 D4 92 AC 15 8B 7E 77 D3 " +
            "B4 D0 1E 26 B3 8E D5 9D 94 03 96 70 A5 CE 12 D5 " +
            "85 84 A5 55 42 A1 48 58 B9 BF 09 F2 51 EA D4 9C " +
            "B4 02 55 9B E4 5B A7 9D E9 87 F6 D7 FA 21 8C DD " +
            "01 50 15 1F 7E BB 5A EA 04 25 9B D3 12 E7 6B 8C " +
            "A5 D0 15 1C 0B E6 23 E3 49 6A 68 6F 64 B9 0F 0A " +
            "0C 19 51 90 0C 31 D4 4A E8 86 E9 3B EF 73 FD 74 " +
            "30 9E C4 CB 0B 7E 1D 2A 48 AD 9A 47 3E E5 E3 45 " +
            "52 15 12 61 2D 6F FA E2 CD AD 1D 9F 88 67 F2 E0 " +
            "42 58 4E EA 9F 7F 1A 00",
            // After Chi
            "4E BD DC A4 B4 12 DF 02 83 3F 30 9A 05 00 04 38 " +
            "8F 3C F1 1D EB 5D 94 9F 0C C6 C3 47 07 81 0A 75 " +
            "6F FE E5 BB 5D CB AD 9A B3 32 84 C6 02 F0 D5 8B " +
            "9F C6 A0 49 4A F3 E5 B8 D4 91 2C 45 8F 3E 75 93 " +
            "D7 F0 16 A4 B1 BE 10 95 98 87 A4 59 DD CD 32 E1 " +
            "81 84 F1 5C E6 B0 6B 59 F0 3A AB B6 4B CA DC DC " +
            "B4 52 54 93 E0 C1 F5 BF 6D 03 56 97 FA 21 8C CD " +
            "39 6B 1D BD 6F F1 CE 6E 4C 0F F3 B0 76 FE 67 84 " +
            "A1 C1 04 8C 03 E6 F3 A3 A9 EC C0 44 87 FB 26 3E " +
            "08 38 43 50 1C B5 D6 C2 49 56 ED 37 E6 73 FD 17 " +
            "22 8E C4 EB 0A 74 05 88 C5 05 97 D9 BE E5 E3 45 " +
            "50 45 50 01 3A 77 F2 E2 FD 2B 9D 9E 88 67 F7 CA " +
            "0A 79 54 EE AB FE F8 45",
            // After Iota 
            "CF 3D DC 24 B4 12 DF 82 83 3F 30 9A 05 00 04 38 " +
            "8F 3C F1 1D EB 5D 94 9F 0C C6 C3 47 07 81 0A 75 " +
            "6F FE E5 BB 5D CB AD 9A B3 32 84 C6 02 F0 D5 8B " +
            "9F C6 A0 49 4A F3 E5 B8 D4 91 2C 45 8F 3E 75 93 " +
            "D7 F0 16 A4 B1 BE 10 95 98 87 A4 59 DD CD 32 E1 " +
            "81 84 F1 5C E6 B0 6B 59 F0 3A AB B6 4B CA DC DC " +
            "B4 52 54 93 E0 C1 F5 BF 6D 03 56 97 FA 21 8C CD " +
            "39 6B 1D BD 6F F1 CE 6E 4C 0F F3 B0 76 FE 67 84 " +
            "A1 C1 04 8C 03 E6 F3 A3 A9 EC C0 44 87 FB 26 3E " +
            "08 38 43 50 1C B5 D6 C2 49 56 ED 37 E6 73 FD 17 " +
            "22 8E C4 EB 0A 74 05 88 C5 05 97 D9 BE E5 E3 45 " +
            "50 45 50 01 3A 77 F2 E2 FD 2B 9D 9E 88 67 F7 CA " +
            "0A 79 54 EE AB FE F8 45",
            // Round #21
            // After Theta
            "53 0F 69 C3 64 1D D1 B1 3C 99 9C 63 5A 84 87 BB " +
            "81 77 E3 59 E3 FE D6 6E 00 EB 10 C4 7B 5A 62 94 " +
            "0A CD 84 CA DC B7 1D 07 2F 00 31 21 D2 FF DB B8 " +
            "20 60 0C B0 15 77 66 3B DA DA 3E 01 87 9D 37 62 " +
            "DB DD C5 27 CD 65 78 74 FD B4 C5 28 5C B1 82 7C " +
            "1D B6 44 BB 36 BF 65 6A 4F 9C 07 4F 14 4E 5F 5F " +
            "BA 19 46 D7 E8 62 B7 4E 61 2E 85 14 86 FA E4 2C " +
            "5C 58 7C CC EE 8D 7E F3 D0 3D 46 57 A6 F1 69 B7 " +
            "1E 67 A8 75 5C 62 70 20 A7 A7 D2 00 8F 58 64 CF " +
            "04 15 90 D3 60 6E BE 23 2C 65 8C 46 67 0F 4D 8A " +
            "BE BC 71 0C DA 7B 0B BB 7A A3 3B 20 E1 61 60 C6 " +
            "5E 0E 42 45 32 D4 B0 13 F1 06 4E 1D F4 BC 9F 2B " +
            "6F 4A 35 9F 2A 82 48 D8",
            // After Rho
            "53 0F 69 C3 64 1D D1 B1 79 32 39 C7 B4 08 0F 77 " +
            "E0 DD 78 D6 B8 BF B5 5B A7 25 46 09 B0 0E 41 BC " +
            "BE ED 38 50 68 26 54 E6 22 FD BF 8D FB 02 10 13 " +
            "00 5B 71 67 B6 03 02 C6 98 B6 B6 4F C0 61 E7 8D " +
            "EE E2 93 E6 32 3C BA ED 2B C8 D7 4F 5B 8C C2 15 " +
            "EB B0 25 DA B5 F9 2D 53 7D 3D 71 1E 3C 51 38 7D " +
            "BA 46 17 BB 75 D2 CD 30 F5 C9 59 C2 5C 0A 29 0C " +
            "66 F7 46 BF 79 2E 2C 3E AE 4C E3 D3 6E A1 7B 8C " +
            "B5 8E 4B 0C 0E C4 E3 0C B2 E7 D3 53 69 80 47 2C " +
            "CD 77 84 A0 02 72 1A CC 8A 2C 65 8C 46 67 0F 4D " +
            "2D EC FA F2 C6 31 68 EF EB 8D EE 80 84 87 81 19 " +
            "CB 41 A8 48 86 1A 76 C2 06 4E 1D F4 BC 9F 2B F1 " +
            "12 F6 9B 52 CD A7 8A 20",
            // After Pi
            "53 0F 69 C3 64 1D D1 B1 00 5B 71 67 B6 03 02 C6 " +
            "BA 46 17 BB 75 D2 CD 30 CD 77 84 A0 02 72 1A CC " +
            "12 F6 9B 52 CD A7 8A 20 A7 25 46 09 B0 0E 41 BC " +
            "2B C8 D7 4F 5B 8C C2 15 EB B0 25 DA B5 F9 2D 53 " +
            "B5 8E 4B 0C 0E C4 E3 0C CB 41 A8 48 86 1A 76 C2 " +
            "79 32 39 C7 B4 08 0F 77 98 B6 B6 4F C0 61 E7 8D " +
            "F5 C9 59 C2 5C 0A 29 0C 8A 2C 65 8C 46 67 0F 4D " +
            "2D EC FA F2 C6 31 68 EF BE ED 38 50 68 26 54 E6 " +
            "22 FD BF 8D FB 02 10 13 7D 3D 71 1E 3C 51 38 7D " +
            "B2 E7 D3 53 69 80 47 2C 06 4E 1D F4 BC 9F 2B F1 " +
            "E0 DD 78 D6 B8 BF B5 5B EE E2 93 E6 32 3C BA ED " +
            "66 F7 46 BF 79 2E 2C 3E AE 4C E3 D3 6E A1 7B 8C " +
            "EB 8D EE 80 84 87 81 19",
            // After Chi
            "E9 0B 6F 5B 25 CD 1C 81 45 6A F1 67 B4 23 10 0A " +
            "A8 C6 0C E9 B8 57 4D 10 8C 7E E4 21 22 6A 4B 5D " +
            "12 A6 8B 76 5F A5 88 66 67 15 66 99 14 7F 6C FE " +
            "3F C6 9D 4B 51 88 00 19 A1 F1 85 9A 35 E3 39 91 " +
            "91 AA 0D 0D 3E C0 E2 30 C3 89 39 0E CD 9A F4 C3 " +
            "1C 7B 70 47 A8 02 07 77 92 92 92 43 C2 04 E1 CC " +
            "D0 09 C3 B0 DC 1A 49 AE DA 3E 64 89 76 6F 08 5D " +
            "AD 68 7C FA 86 50 88 67 E3 ED 78 42 6C 77 7C 8A " +
            "A0 3F 3D CC BA 82 57 13 79 35 7D BA A8 4E 10 AC " +
            "0A 46 F3 53 29 A0 13 2A 06 5E 9A 79 2F 9F 2B E0 " +
            "E0 C8 3C CF F1 BD B1 49 66 EA 32 A6 34 BD E9 6D " +
            "27 76 4A BF F9 28 AC 2F AE 1C F3 85 56 99 4F CE " +
            "E5 AF 6D A0 86 87 8B BD",
            // After Iota 
            "69 8B 6F 5B 25 CD 1C 01 45 6A F1 67 B4 23 10 0A " +
            "A8 C6 0C E9 B8 57 4D 10 8C 7E E4 21 22 6A 4B 5D " +
            "12 A6 8B 76 5F A5 88 66 67 15 66 99 14 7F 6C FE " +
            "3F C6 9D 4B 51 88 00 19 A1 F1 85 9A 35 E3 39 91 " +
            "91 AA 0D 0D 3E C0 E2 30 C3 89 39 0E CD 9A F4 C3 " +
            "1C 7B 70 47 A8 02 07 77 92 92 92 43 C2 04 E1 CC " +
            "D0 09 C3 B0 DC 1A 49 AE DA 3E 64 89 76 6F 08 5D " +
            "AD 68 7C FA 86 50 88 67 E3 ED 78 42 6C 77 7C 8A " +
            "A0 3F 3D CC BA 82 57 13 79 35 7D BA A8 4E 10 AC " +
            "0A 46 F3 53 29 A0 13 2A 06 5E 9A 79 2F 9F 2B E0 " +
            "E0 C8 3C CF F1 BD B1 49 66 EA 32 A6 34 BD E9 6D " +
            "27 76 4A BF F9 28 AC 2F AE 1C F3 85 56 99 4F CE " +
            "E5 AF 6D A0 86 87 8B BD",
            // Round #22
            // After Theta
            "AB EB B5 0B CA 9B D7 DC 5B 51 36 E3 B1 C9 A9 18 " +
            "41 4D E6 0B 3B 3F F9 18 34 6E EA 51 58 4D 62 CF " +
            "53 96 7D 15 42 AD 01 25 A5 75 BC C9 FB 29 A7 23 " +
            "21 FD 5A CF 54 62 B9 0B 48 7A 6F 78 B6 8B 8D 99 " +
            "29 BA 03 7D 44 E7 CB A2 82 B9 CF 6D D0 92 7D 80 " +
            "DE 1B AA 17 47 54 CC AA 8C A9 55 C7 C7 EE 58 DE " +
            "39 82 29 52 5F 72 FD A6 62 2E 6A F9 0C 48 21 CF " +
            "EC 58 8A 99 9B 58 01 24 21 8D A2 12 83 21 B7 57 " +
            "BE 04 FA 48 BF 68 EE 01 90 BE 97 58 2B 26 A4 A4 " +
            "B2 56 FD 23 53 87 3A B8 47 6E 6C 1A 32 97 A2 A3 " +
            "22 A8 E6 9F 1E EB 7A 94 78 D1 F5 22 31 57 50 7F " +
            "CE FD A0 5D 7A 40 18 27 16 0C FD F5 2C BE 66 5C " +
            "A4 9F 9B C3 9B 8F 02 FE",
            // After Rho
            "AB EB B5 0B CA 9B D7 DC B6 A2 6C C6 63 93 53 31 " +
            "50 93 F9 C2 CE 4F 3E 46 D5 24 F6 4C E3 A6 1E 85 " +
            "6A 0D 28 99 B2 EC AB 10 BC 9F 72 3A 52 5A C7 9B " +
            "F5 4C 25 96 BB 10 D2 AF 26 92 DE 1B 9E ED 62 63 " +
            "DD 81 3E A2 F3 65 D1 14 D9 07 28 98 FB DC 06 2D " +
            "F5 DE 50 BD 38 A2 62 56 79 33 A6 56 1D 1F BB 63 " +
            "91 FA 92 EB 37 CD 11 4C 90 42 9E C5 5C D4 F2 19 " +
            "CC 4D AC 00 12 76 2C C5 25 06 43 6E AF 42 1A 45 " +
            "1F E9 17 CD 3D C0 97 40 52 52 48 DF 4B AC 15 13 " +
            "50 07 57 D6 AA 7F 64 EA A3 47 6E 6C 1A 32 97 A2 " +
            "EB 51 8A A0 9A 7F 7A AC E1 45 D7 8B C4 5C 41 FD " +
            "B9 1F B4 4B 0F 08 E3 C4 0C FD F5 2C BE 66 5C 16 " +
            "80 3F E9 E7 E6 F0 E6 A3",
            // After Pi
            "AB EB B5 0B CA 9B D7 DC F5 4C 25 96 BB 10 D2 AF " +
            "91 FA 92 EB 37 CD 11 4C 50 07 57 D6 AA 7F 64 EA " +
            "80 3F E9 E7 E6 F0 E6 A3 D5 24 F6 4C E3 A6 1E 85 " +
            "D9 07 28 98 FB DC 06 2D F5 DE 50 BD 38 A2 62 56 " +
            "1F E9 17 CD 3D C0 97 40 B9 1F B4 4B 0F 08 E3 C4 " +
            "B6 A2 6C C6 63 93 53 31 26 92 DE 1B 9E ED 62 63 " +
            "90 42 9E C5 5C D4 F2 19 A3 47 6E 6C 1A 32 97 A2 " +
            "EB 51 8A A0 9A 7F 7A AC 6A 0D 28 99 B2 EC AB 10 " +
            "BC 9F 72 3A 52 5A C7 9B 79 33 A6 56 1D 1F BB 63 " +
            "52 52 48 DF 4B AC 15 13 0C FD F5 2C BE 66 5C 16 " +
            "50 93 F9 C2 CE 4F 3E 46 DD 81 3E A2 F3 65 D1 14 " +
            "CC 4D AC 00 12 76 2C C5 25 06 43 6E AF 42 1A 45 " +
            "E1 45 D7 8B C4 5C 41 FD",
            // After Chi
            "AB 59 27 62 CE 56 D6 9C B5 49 60 82 33 22 B6 0D " +
            "11 C2 3A CA 73 4D 93 4D 7B C7 43 DE A2 74 75 B6 " +
            "D4 3B E9 73 D7 F0 E6 80 F1 FC A6 69 E3 84 7E D7 " +
            "D3 26 2F D8 FE 9C 93 2D 55 C8 F0 BF 3A AA 02 D2 " +
            "5B C9 55 C9 DD 66 8B 41 B1 1C BC DB 17 50 E3 EC " +
            "26 E2 6C 02 23 83 C3 29 05 97 BE 33 9C CF 67 C1 " +
            "D8 52 1E 45 DC 99 9A 15 B7 E5 0A 2A 7B B2 96 B3 " +
            "EB 41 18 B9 06 13 5A EE 2B 2D AC DD BF E9 93 70 " +
            "BE DF 3A B3 10 FA C3 8B 75 9E 13 76 A9 5D F3 67 " +
            "30 52 40 4E 4B 24 B6 13 98 6F A7 0E FE 74 18 9D " +
            "50 DF 79 C2 CE 5D 12 87 FC 83 7D CC 5E 65 C3 14 " +
            "0C 0C 38 81 52 6A 6D 7D 35 94 6B 2E A5 41 24 47 " +
            "6C 45 D1 AB F5 7C 80 ED",
            // After Iota 
            "AA 59 27 E2 CE 56 D6 9C B5 49 60 82 33 22 B6 0D " +
            "11 C2 3A CA 73 4D 93 4D 7B C7 43 DE A2 74 75 B6 " +
            "D4 3B E9 73 D7 F0 E6 80 F1 FC A6 69 E3 84 7E D7 " +
            "D3 26 2F D8 FE 9C 93 2D 55 C8 F0 BF 3A AA 02 D2 " +
            "5B C9 55 C9 DD 66 8B 41 B1 1C BC DB 17 50 E3 EC " +
            "26 E2 6C 02 23 83 C3 29 05 97 BE 33 9C CF 67 C1 " +
            "D8 52 1E 45 DC 99 9A 15 B7 E5 0A 2A 7B B2 96 B3 " +
            "EB 41 18 B9 06 13 5A EE 2B 2D AC DD BF E9 93 70 " +
            "BE DF 3A B3 10 FA C3 8B 75 9E 13 76 A9 5D F3 67 " +
            "30 52 40 4E 4B 24 B6 13 98 6F A7 0E FE 74 18 9D " +
            "50 DF 79 C2 CE 5D 12 87 FC 83 7D CC 5E 65 C3 14 " +
            "0C 0C 38 81 52 6A 6D 7D 35 94 6B 2E A5 41 24 47 " +
            "6C 45 D1 AB F5 7C 80 ED",
            // Round #23
            // After Theta
            "92 5D 71 7B 3D 31 94 92 78 69 A7 9B 91 55 76 B9 " +
            "14 3D E2 66 B8 28 24 12 6B 95 CA 71 57 4A 6F C3 " +
            "4B 7C AF 02 C2 FF C9 BB C9 F8 F0 F0 10 E3 3C D9 " +
            "1E 06 E8 C1 5C EB 53 99 50 37 28 13 F1 CF B5 8D " +
            "4B 9B DC 66 28 58 91 34 2E 5B FA AA 02 5F CC D7 " +
            "1E E6 3A 9B D0 E4 81 27 C8 B7 79 2A 3E B8 A7 75 " +
            "DD AD C6 E9 17 FC 2D 4A A7 B7 83 85 8E 8C 8C C6 " +
            "74 06 5E C8 13 1C 75 D5 13 29 FA 44 4C 8E D1 7E " +
            "73 FF FD AA B2 8D 03 3F 70 61 CB DA 62 38 44 38 " +
            "20 00 C9 E1 BE 1A AC 66 07 28 E1 7F EB 7B 37 A6 " +
            "68 DB 2F 5B 3D 3A 50 89 31 A3 BA D5 FC 12 03 A0 " +
            "09 F3 E0 2D 99 0F DA 22 25 C6 E2 81 50 7F 3E 32 " +
            "F3 02 97 DA E0 73 AF D6",
            // After Rho
            "92 5D 71 7B 3D 31 94 92 F1 D2 4E 37 23 AB EC 72 " +
            "45 8F B8 19 2E 0A 89 04 A5 F4 36 BC 56 A9 1C 77 " +
            "FE 4F DE 5D E2 7B 15 10 0F 31 CE 93 9D 8C 0F 0F " +
            "1E CC B5 3E 95 E9 61 80 23 D4 0D CA 44 FC 73 6D " +
            "4D 6E 33 14 AC 48 9A A5 C5 7C ED B2 A5 AF 2A F0 " +
            "F1 30 D7 D9 84 26 0F 3C D6 21 DF E6 A9 F8 E0 9E " +
            "4E BF E0 6F 51 EA 6E 35 19 19 8D 4F 6F 07 0B 1D " +
            "E4 09 8E BA 6A 3A 03 2F 89 98 1C A3 FD 26 52 F4 " +
            "5F 55 B6 71 E0 67 EE BF 22 1C B8 B0 65 6D 31 1C " +
            "83 D5 0C 04 20 39 DC 57 A6 07 28 E1 7F EB 7B 37 " +
            "40 25 A2 6D BF 6C F5 E8 C6 8C EA 56 F3 4B 0C 80 " +
            "61 1E BC 25 F3 41 5B 24 C6 E2 81 50 7F 3E 32 25 " +
            "AB F5 BC C0 A5 36 F8 DC",
            // After Pi
            "92 5D 71 7B 3D 31 94 92 1E CC B5 3E 95 E9 61 80 " +
            "4E BF E0 6F 51 EA 6E 35 83 D5 0C 04 20 39 DC 57 " +
            "AB F5 BC C0 A5 36 F8 DC A5 F4 36 BC 56 A9 1C 77 " +
            "C5 7C ED B2 A5 AF 2A F0 F1 30 D7 D9 84 26 0F 3C " +
            "5F 55 B6 71 E0 67 EE BF 61 1E BC 25 F3 41 5B 24 " +
            "F1 D2 4E 37 23 AB EC 72 23 D4 0D CA 44 FC 73 6D " +
            "19 19 8D 4F 6F 07 0B 1D A6 07 28 E1 7F EB 7B 37 " +
            "40 25 A2 6D BF 6C F5 E8 FE 4F DE 5D E2 7B 15 10 " +
            "0F 31 CE 93 9D 8C 0F 0F D6 21 DF E6 A9 F8 E0 9E " +
            "22 1C B8 B0 65 6D 31 1C C6 E2 81 50 7F 3E 32 25 " +
            "45 8F B8 19 2E 0A 89 04 4D 6E 33 14 AC 48 9A A5 " +
            "E4 09 8E BA 6A 3A 03 2F 89 98 1C A3 FD 26 52 F4 " +
            "C6 8C EA 56 F3 4B 0C 80",
            // After Chi
            "D2 6E 31 3A 7D 33 9A A7 9F 8C B9 3E B5 F8 F1 C2 " +
            "66 9F 50 AF D4 EC 4E BD 93 DD 4D 3F 38 38 D8 55 " +
            "A7 75 38 C4 25 FE 99 DC 95 F4 24 F5 56 A9 19 7B " +
            "CB 39 CD 92 C5 EE CA 73 D1 3A DF DD 97 26 1E 3C " +
            "DB B5 B4 E9 E4 CF EA EC 21 16 75 27 52 47 79 A4 " +
            "E9 DB CE 32 08 A8 E4 62 85 D2 2D 6A 54 14 03 4F " +
            "59 39 0F 43 EF 03 8F D5 17 D5 64 F3 7F 68 73 25 " +
            "42 21 A3 A5 FB 38 E6 E5 2E 4F CF 39 C2 0B F5 80 " +
            "2F 2D EE 83 D9 89 1E 0F 12 C3 DE A6 B3 EA E2 BF " +
            "1A 11 E6 BD E5 2C 34 0C C7 D2 81 D2 62 BA 38 2A " +
            "E5 8E 34 B3 6C 38 88 0E 44 FE 23 15 39 4C CA 75 " +
            "A2 0D 6C EE 68 73 0F 2F 88 9B 0C AA F1 26 D3 F0 " +
            "CE EC E9 52 73 0B 1E 21",
            // After Iota 
            "DA EE 31 BA 7D 33 9A 27 9F 8C B9 3E B5 F8 F1 C2 " +
            "66 9F 50 AF D4 EC 4E BD 93 DD 4D 3F 38 38 D8 55 " +
            "A7 75 38 C4 25 FE 99 DC 95 F4 24 F5 56 A9 19 7B " +
            "CB 39 CD 92 C5 EE CA 73 D1 3A DF DD 97 26 1E 3C " +
            "DB B5 B4 E9 E4 CF EA EC 21 16 75 27 52 47 79 A4 " +
            "E9 DB CE 32 08 A8 E4 62 85 D2 2D 6A 54 14 03 4F " +
            "59 39 0F 43 EF 03 8F D5 17 D5 64 F3 7F 68 73 25 " +
            "42 21 A3 A5 FB 38 E6 E5 2E 4F CF 39 C2 0B F5 80 " +
            "2F 2D EE 83 D9 89 1E 0F 12 C3 DE A6 B3 EA E2 BF " +
            "1A 11 E6 BD E5 2C 34 0C C7 D2 81 D2 62 BA 38 2A " +
            "E5 8E 34 B3 6C 38 88 0E 44 FE 23 15 39 4C CA 75 " +
            "A2 0D 6C EE 68 73 0F 2F 88 9B 0C AA F1 26 D3 F0 " +
            "CE EC E9 52 73 0B 1E 21",
            // Xor'd state (in bytes)                        " +
            "79 4D 92 19 DE 90 39 84 3C 2F 1A 9D 16 5B 52 61 " +
            "C5 3C F3 0C 77 4F ED 1E 30 7E EE 9C 9B 9B 7B F6 " +
            "04 D6 9B 67 86 5D 3A 7F 36 57 87 56 F5 0A BA D8 " +
            "68 9A 6E 31 66 4D 69 D0 72 99 7C 7E 34 85 BD 9F " +
            "C4 B5 B4 E9 E4 CF EA EC 21 16 75 27 52 47 79 A4 " +
            "E9 DB CE 32 08 A8 E4 62 85 D2 2D 6A 54 14 03 4F " +
            "59 39 0F 43 EF 03 8F D5 17 D5 64 F3 7F 68 73 25 " +
            "42 21 A3 A5 FB 38 E6 E5 2E 4F CF 39 C2 0B F5 80 " +
            "2F 2D EE 83 D9 89 1E 8F 12 C3 DE A6 B3 EA E2 BF " +
            "1A 11 E6 BD E5 2C 34 0C C7 D2 81 D2 62 BA 38 2A " +
            "E5 8E 34 B3 6C 38 88 0E 44 FE 23 15 39 4C CA 75 " +
            "A2 0D 6C EE 68 73 0F 2F 88 9B 0C AA F1 26 D3 F0 " +
            "CE EC E9 52 73 0B 1E 21",
            // Round #0
            // After Theta
            "63 FB 9E DD 68 8C 63 B8 EC 8B 5E 98 75 FA 2C 59 " +
            "9C A0 CE 7F 9A E4 0B 9C B2 92 97 2F 90 ED 4E 59 " +
            "AE 42 0F 18 89 68 0B DC 2C E1 8B 92 43 16 E0 E4 " +
            "B8 3E 2A 34 05 EC 17 E8 2B 05 41 0D D9 2E 5B 1D " +
            "46 59 CD 5A EF B9 DF 43 8B 82 E1 58 5D 72 48 07 " +
            "F3 6D C2 F6 BE B4 BE 5E 55 76 69 6F 37 B5 7D 77 " +
            "00 A5 32 30 02 A8 69 57 95 39 1D 40 74 1E 46 8A " +
            "E8 B5 37 DA F4 0D D7 46 34 F9 C3 FD 74 17 AF BC " +
            "FF 89 AA 86 BA 28 60 B7 4B 5F E3 D5 5E 41 04 3D " +
            "98 FD 9F 0E EE 5A 01 A3 6D 46 15 AD 6D 8F 09 89 " +
            "FF 38 38 77 DA 24 D2 32 94 5A 67 10 5A ED B4 4D " +
            "FB 91 51 9D 85 D8 E9 AD 0A 77 75 19 FA 50 E6 5F " +
            "64 78 7D 2D 7C 3E 2F 82",
            // After Rho
            "63 FB 9E DD 68 8C 63 B8 D8 17 BD 30 EB F4 59 B2 " +
            "27 A8 F3 9F 26 F9 02 27 D9 EE 94 25 2B 79 F9 02 " +
            "44 5B E0 76 15 7A C0 48 39 64 01 4E CE 12 BE 28 " +
            "42 53 C0 7E 81 8E EB A3 C7 4A 41 50 43 B6 CB 56 " +
            "AC 66 AD F7 DC EF 21 A3 87 74 B0 28 18 8E D5 25 " +
            "9A 6F 13 B6 F7 A5 F5 F5 DD 55 D9 A5 BD DD D4 F6 " +
            "81 11 40 4D BB 02 28 95 3C 8C 14 2B 73 3A 80 E8 " +
            "6D FA 86 6B 23 F4 DA 1B FB E9 2E 5E 79 69 F2 87 " +
            "D5 50 17 05 EC F6 3F 51 82 9E A5 AF F1 6A AF 20 " +
            "2B 60 14 B3 FF D3 C1 5D 89 6D 46 15 AD 6D 8F 09 " +
            "48 CB FC E3 E0 DC 69 93 51 6A 9D 41 68 B5 D3 36 " +
            "3F 32 AA B3 10 3B BD 75 77 75 19 FA 50 E6 5F 0A " +
            "8B 20 19 5E 5F 0B 9F CF",
            // After Pi
            "63 FB 9E DD 68 8C 63 B8 42 53 C0 7E 81 8E EB A3 " +
            "81 11 40 4D BB 02 28 95 2B 60 14 B3 FF D3 C1 5D " +
            "8B 20 19 5E 5F 0B 9F CF D9 EE 94 25 2B 79 F9 02 " +
            "87 74 B0 28 18 8E D5 25 9A 6F 13 B6 F7 A5 F5 F5 " +
            "D5 50 17 05 EC F6 3F 51 3F 32 AA B3 10 3B BD 75 " +
            "D8 17 BD 30 EB F4 59 B2 C7 4A 41 50 43 B6 CB 56 " +
            "3C 8C 14 2B 73 3A 80 E8 89 6D 46 15 AD 6D 8F 09 " +
            "48 CB FC E3 E0 DC 69 93 44 5B E0 76 15 7A C0 48 " +
            "39 64 01 4E CE 12 BE 28 DD 55 D9 A5 BD DD D4 F6 " +
            "82 9E A5 AF F1 6A AF 20 77 75 19 FA 50 E6 5F 0A " +
            "27 A8 F3 9F 26 F9 02 27 AC 66 AD F7 DC EF 21 A3 " +
            "6D FA 86 6B 23 F4 DA 1B FB E9 2E 5E 79 69 F2 87 " +
            "51 6A 9D 41 68 B5 D3 36",
            // After Chi
            "E2 FB 9E DC 52 8C 63 AC 68 33 D4 CC C5 5F 2A EB " +
            "01 11 49 01 BB 0A 36 17 4B BB 92 32 DF 57 A1 6D " +
            "8B 20 59 7C DE 09 17 CC C1 E5 97 B3 CC 58 D9 D2 " +
            "C2 64 B4 29 10 DC DF 25 B0 4D BB 04 E7 AC 75 D1 " +
            "15 9C 03 01 C7 B6 7F 53 39 22 8A BB 00 BD B9 50 " +
            "E0 93 A9 1B DB FC 59 1A 46 2B 03 44 CF F3 C4 57 " +
            "7C 0E AC C9 33 AA E0 7A 19 79 47 05 A6 4D 9F 29 " +
            "4F 83 BC A3 E0 DE EB D7 80 4A 38 D7 24 B7 80 9E " +
            "3B EE 25 44 8E 30 95 28 A8 34 C1 F5 BD 59 84 FC " +
            "82 94 45 AB F4 72 2F 60 4E 51 18 F2 9A E6 61 2A " +
            "66 30 F1 97 05 E9 D8 3F 3E 67 85 E3 84 E6 01 27 " +
            "6D F8 17 6A 23 60 DB 2B DD 69 4C C0 7F 21 F2 86 " +
            "D9 2C 91 21 B0 B3 F2 B6",
            // After Iota 
            "E3 FB 9E DC 52 8C 63 AC 68 33 D4 CC C5 5F 2A EB " +
            "01 11 49 01 BB 0A 36 17 4B BB 92 32 DF 57 A1 6D " +
            "8B 20 59 7C DE 09 17 CC C1 E5 97 B3 CC 58 D9 D2 " +
            "C2 64 B4 29 10 DC DF 25 B0 4D BB 04 E7 AC 75 D1 " +
            "15 9C 03 01 C7 B6 7F 53 39 22 8A BB 00 BD B9 50 " +
            "E0 93 A9 1B DB FC 59 1A 46 2B 03 44 CF F3 C4 57 " +
            "7C 0E AC C9 33 AA E0 7A 19 79 47 05 A6 4D 9F 29 " +
            "4F 83 BC A3 E0 DE EB D7 80 4A 38 D7 24 B7 80 9E " +
            "3B EE 25 44 8E 30 95 28 A8 34 C1 F5 BD 59 84 FC " +
            "82 94 45 AB F4 72 2F 60 4E 51 18 F2 9A E6 61 2A " +
            "66 30 F1 97 05 E9 D8 3F 3E 67 85 E3 84 E6 01 27 " +
            "6D F8 17 6A 23 60 DB 2B DD 69 4C C0 7F 21 F2 86 " +
            "D9 2C 91 21 B0 B3 F2 B6",
            // Round #1
            // After Theta
            "5A EC FF 66 66 FF FE 56 5C F8 AC 5F 43 42 69 F9 " +
            "D9 A2 35 BC C1 52 AA 62 96 DD D7 0E 07 1C F1 A9 " +
            "DA 6D 55 49 23 1A FD B6 78 F2 F6 09 F8 2B 44 28 " +
            "F6 AF CC BA 96 C1 9C 37 68 FE C7 B9 9D F4 E9 A4 " +
            "C8 FA 46 3D 1F FD 2F 97 68 6F 86 8E FD AE 53 2A " +
            "59 84 C8 A1 EF 8F C4 E0 72 E0 7B D7 49 EE 87 45 " +
            "A4 BD D0 74 49 F2 7C 0F C4 1F 02 39 7E 06 CF ED " +
            "1E CE B0 96 1D CD 01 AD 39 5D 59 6D 10 C4 1D 64 " +
            "0F 25 5D D7 08 2D D6 3A 70 87 BD 48 C7 01 18 89 " +
            "5F F2 00 97 2C 39 7F A4 1F 1C 14 C7 67 F5 8B 50 " +
            "DF 27 90 2D 31 9A 45 C5 0A AC FD 70 02 FB 42 35 " +
            "B5 4B 6B D7 59 38 47 5E 00 0F 09 FC A7 6A A2 42 " +
            "88 61 9D 14 4D A0 18 CC",
            // After Rho
            "5A EC FF 66 66 FF FE 56 B9 F0 59 BF 86 84 D2 F2 " +
            "B6 68 0D 6F B0 94 AA 58 C0 11 9F 6A D9 7D ED 70 " +
            "D1 E8 B7 D5 6E AB 4A 1A 80 BF 42 84 82 27 6F 9F " +
            "AC 6B 19 CC 79 63 FF CA 29 9A FF 71 6E 27 7D 3A " +
            "7D A3 9E 8F FE 97 4B 64 3A A5 82 F6 66 E8 D8 EF " +
            "CF 22 44 0E 7D 7F 24 06 16 C9 81 EF 5D 27 B9 1F " +
            "A6 4B 92 E7 7B 20 ED 85 0C 9E DB 89 3F 04 72 FC " +
            "CB 8E E6 80 56 0F 67 58 DA 20 88 3B C8 72 BA B2 " +
            "EB 1A A1 C5 5A E7 A1 A4 8C 44 B8 C3 5E A4 E3 00 " +
            "E7 8F F4 4B 1E E0 92 25 50 1F 1C 14 C7 67 F5 8B " +
            "16 15 7F 9F 40 B6 C4 68 28 B0 F6 C3 09 EC 0B D5 " +
            "76 69 ED 3A 0B E7 C8 AB 0F 09 FC A7 6A A2 42 00 " +
            "06 33 62 58 27 45 13 28",
            // After Pi
            "5A EC FF 66 66 FF FE 56 AC 6B 19 CC 79 63 FF CA " +
            "A6 4B 92 E7 7B 20 ED 85 E7 8F F4 4B 1E E0 92 25 " +
            "06 33 62 58 27 45 13 28 C0 11 9F 6A D9 7D ED 70 " +
            "3A A5 82 F6 66 E8 D8 EF CF 22 44 0E 7D 7F 24 06 " +
            "EB 1A A1 C5 5A E7 A1 A4 76 69 ED 3A 0B E7 C8 AB " +
            "B9 F0 59 BF 86 84 D2 F2 29 9A FF 71 6E 27 7D 3A " +
            "0C 9E DB 89 3F 04 72 FC 50 1F 1C 14 C7 67 F5 8B " +
            "16 15 7F 9F 40 B6 C4 68 D1 E8 B7 D5 6E AB 4A 1A " +
            "80 BF 42 84 82 27 6F 9F 16 C9 81 EF 5D 27 B9 1F " +
            "8C 44 B8 C3 5E A4 E3 00 0F 09 FC A7 6A A2 42 00 " +
            "B6 68 0D 6F B0 94 AA 58 7D A3 9E 8F FE 97 4B 64 " +
            "CB 8E E6 80 56 0F 67 58 DA 20 88 3B C8 72 BA B2 " +
            "28 B0 F6 C3 09 EC 0B D5",
            // After Chi
            "58 EC 7D 45 64 FF FE 53 ED EF 7D C4 7D A3 ED EA " +
            "A6 7B 90 F7 5A 25 EC 8D BF 43 69 6D 5E 5A 7E 73 " +
            "A2 30 62 D0 3E 45 12 A0 05 13 DB 62 C0 6A C9 70 " +
            "1A BD 23 37 64 68 59 4F DB 43 08 34 7C 7F 6C 0D " +
            "6B 0A B3 85 8A FF 84 F4 4C CD ED AE 2D 67 D8 24 " +
            "BD F4 59 37 97 84 D0 36 79 9B FB 65 AE 44 F8 39 " +
            "0A 9E B8 02 3F 94 72 9C F9 FF 1C 34 41 67 E7 19 " +
            "16 1F D9 DF 28 95 E9 60 C7 A8 36 BE 33 AB DA 1A " +
            "08 BB 7A 84 80 A7 2D 9F 15 C0 C5 CB 7D 25 B9 1F " +
            "5C A4 BB 93 5A AD EB 1A 0F 1E BC A7 EA A6 67 85 " +
            "34 64 6D 6F B0 9C 8E 40 6D 83 96 B4 76 E7 D3 C6 " +
            "EB 1E 90 40 57 83 66 1D 4C 68 81 17 78 62 1A BA " +
            "61 33 64 43 47 EF 4A F1",
            // After Iota 
            "DA 6C 7D 45 64 FF FE 53 ED EF 7D C4 7D A3 ED EA " +
            "A6 7B 90 F7 5A 25 EC 8D BF 43 69 6D 5E 5A 7E 73 " +
            "A2 30 62 D0 3E 45 12 A0 05 13 DB 62 C0 6A C9 70 " +
            "1A BD 23 37 64 68 59 4F DB 43 08 34 7C 7F 6C 0D " +
            "6B 0A B3 85 8A FF 84 F4 4C CD ED AE 2D 67 D8 24 " +
            "BD F4 59 37 97 84 D0 36 79 9B FB 65 AE 44 F8 39 " +
            "0A 9E B8 02 3F 94 72 9C F9 FF 1C 34 41 67 E7 19 " +
            "16 1F D9 DF 28 95 E9 60 C7 A8 36 BE 33 AB DA 1A " +
            "08 BB 7A 84 80 A7 2D 9F 15 C0 C5 CB 7D 25 B9 1F " +
            "5C A4 BB 93 5A AD EB 1A 0F 1E BC A7 EA A6 67 85 " +
            "34 64 6D 6F B0 9C 8E 40 6D 83 96 B4 76 E7 D3 C6 " +
            "EB 1E 90 40 57 83 66 1D 4C 68 81 17 78 62 1A BA " +
            "61 33 64 43 47 EF 4A F1",
            // Round #2
            // After Theta
            "9B 40 60 4C 71 9F 95 48 6E 59 33 91 AB 55 04 99 " +
            "37 7E 21 E0 75 F1 86 35 1B A4 01 AC 41 CF 4E 4D " +
            "BD C5 D6 0B E8 05 98 01 44 3F C6 6B D5 0A A2 6B " +
            "99 0B 6D 62 B2 9E B0 3C 4A 46 B9 23 53 AB 06 B5 " +
            "CF ED DB 44 95 6A B4 CA 53 38 59 75 FB 27 52 85 " +
            "FC D8 44 3E 82 E4 BB 2D FA 2D B5 30 78 B2 11 4A " +
            "9B 9B 09 15 10 40 18 24 5D 18 74 F5 5E F2 D7 27 " +
            "09 EA 6D 04 FE D5 63 C1 86 84 2B B7 26 CB B1 01 " +
            "8B 0D 34 D1 56 51 C4 EC 84 C5 74 DC 52 F1 D3 A7 " +
            "F8 43 D3 52 45 38 DB 24 10 EB 08 7C 3C E6 ED 24 " +
            "75 48 70 66 A5 FC E5 5B EE 35 D8 E1 A0 11 3A B5 " +
            "7A 1B 21 57 78 57 0C A5 E8 8F E9 D6 67 F7 2A 84 " +
            "7E C6 D0 98 91 AF C0 50",
            // After Rho
            "9B 40 60 4C 71 9F 95 48 DD B2 66 22 57 AB 08 32 " +
            "8D 5F 08 78 5D BC 61 CD F4 EC D4 B4 41 1A C0 1A " +
            "2F C0 0C E8 2D B6 5E 40 56 AD 20 BA 46 F4 63 BC " +
            "26 26 EB 09 CB 93 B9 D0 AD 92 51 EE C8 D4 AA 41 " +
            "F6 6D A2 4A 35 5A E5 E7 22 55 38 85 93 55 B7 7F " +
            "E1 C7 26 F2 11 24 DF 6D 28 E9 B7 D4 C2 E0 C9 46 " +
            "A8 80 00 C2 20 D9 DC 4C E4 AF 4F BA 30 E8 EA BD " +
            "02 FF EA B1 E0 04 F5 36 6E 4D 96 63 03 0C 09 57 " +
            "26 DA 2A 8A 98 7D B1 81 E9 53 C2 62 3A 6E A9 F8 " +
            "67 9B 04 7F 68 5A AA 08 24 10 EB 08 7C 3C E6 ED " +
            "97 6F D5 21 C1 99 95 F2 BA D7 60 87 83 46 E8 D4 " +
            "6F 23 E4 0A EF 8A A1 54 8F E9 D6 67 F7 2A 84 E8 " +
            "30 94 9F 31 34 66 E4 2B",
            // After Pi
            "9B 40 60 4C 71 9F 95 48 26 26 EB 09 CB 93 B9 D0 " +
            "A8 80 00 C2 20 D9 DC 4C 67 9B 04 7F 68 5A AA 08 " +
            "30 94 9F 31 34 66 E4 2B F4 EC D4 B4 41 1A C0 1A " +
            "22 55 38 85 93 55 B7 7F E1 C7 26 F2 11 24 DF 6D " +
            "26 DA 2A 8A 98 7D B1 81 6F 23 E4 0A EF 8A A1 54 " +
            "DD B2 66 22 57 AB 08 32 AD 92 51 EE C8 D4 AA 41 " +
            "E4 AF 4F BA 30 E8 EA BD 24 10 EB 08 7C 3C E6 ED " +
            "97 6F D5 21 C1 99 95 F2 2F C0 0C E8 2D B6 5E 40 " +
            "56 AD 20 BA 46 F4 63 BC 28 E9 B7 D4 C2 E0 C9 46 " +
            "E9 53 C2 62 3A 6E A9 F8 8F E9 D6 67 F7 2A 84 E8 " +
            "8D 5F 08 78 5D BC 61 CD F6 6D A2 4A 35 5A E5 E7 " +
            "02 FF EA B1 E0 04 F5 36 6E 4D 96 63 03 0C 09 57 " +
            "BA D7 60 87 83 46 E8 D4",
            // After Chi
            "13 C0 60 8E 51 D7 D1 44 61 3D EF 34 83 91 9B D0 " +
            "B8 84 9B C2 34 FD 98 6F EC DB 64 33 29 C3 BB 48 " +
            "14 B2 14 30 BE 66 CC BB 35 6E D2 C6 41 3A 88 1A " +
            "24 4D 30 8D 1B 0C 97 FF A8 E6 E2 F2 76 A6 DF 39 " +
            "B6 16 3A 3E 98 6D F1 8B 6D 32 CC 0B 7D CF 96 31 " +
            "9D 9F 68 32 67 83 48 8E AD 82 F1 EE 84 C0 AE 01 " +
            "77 C0 5B 9B B1 69 FB AF 6C 80 C9 0A 6A 1E EE ED " +
            "B7 6F C4 ED 49 CD 37 B3 07 80 9B AC AD B6 D6 02 " +
            "97 BF 60 98 7E FA 43 04 2E 41 A3 D1 07 E0 CD 46 " +
            "C9 53 CA EA 32 FA F3 F8 DF C4 F6 75 B5 6A A5 54 " +
            "8D CD 40 C9 9D B8 71 DD 9A 6D B6 08 36 52 ED A6 " +
            "92 6D 8A 35 60 46 15 B6 6B 45 9E 1B 5F B4 08 5E " +
            "C8 F7 C2 85 A3 04 6C F6",
            // After Iota 
            "99 40 60 8E 51 D7 D1 C4 61 3D EF 34 83 91 9B D0 " +
            "B8 84 9B C2 34 FD 98 6F EC DB 64 33 29 C3 BB 48 " +
            "14 B2 14 30 BE 66 CC BB 35 6E D2 C6 41 3A 88 1A " +
            "24 4D 30 8D 1B 0C 97 FF A8 E6 E2 F2 76 A6 DF 39 " +
            "B6 16 3A 3E 98 6D F1 8B 6D 32 CC 0B 7D CF 96 31 " +
            "9D 9F 68 32 67 83 48 8E AD 82 F1 EE 84 C0 AE 01 " +
            "77 C0 5B 9B B1 69 FB AF 6C 80 C9 0A 6A 1E EE ED " +
            "B7 6F C4 ED 49 CD 37 B3 07 80 9B AC AD B6 D6 02 " +
            "97 BF 60 98 7E FA 43 04 2E 41 A3 D1 07 E0 CD 46 " +
            "C9 53 CA EA 32 FA F3 F8 DF C4 F6 75 B5 6A A5 54 " +
            "8D CD 40 C9 9D B8 71 DD 9A 6D B6 08 36 52 ED A6 " +
            "92 6D 8A 35 60 46 15 B6 6B 45 9E 1B 5F B4 08 5E " +
            "C8 F7 C2 85 A3 04 6C F6",
            // Round #3
            // After Theta
            "8B DD B8 27 64 37 6C 47 6C DC F9 B5 EC D8 E4 4D " +
            "74 13 E5 E8 0D F5 2B F3 84 EC 3E 30 85 42 97 76 " +
            "F7 10 D4 F8 86 58 FF 2C 27 F3 0A 6F 74 DA 35 99 " +
            "29 AC 26 0C 74 45 E8 62 64 71 9C D8 4F AE 6C A5 " +
            "DE 21 60 3D 34 EC DD B5 8E 90 0C C3 45 F1 A5 A6 " +
            "8F 02 B0 9B 52 63 F5 0D A0 63 E7 6F EB 89 D1 9C " +
            "BB 57 25 B1 88 61 48 33 04 B7 93 09 C6 9F C2 D3 " +
            "54 CD 04 25 71 F3 04 24 15 1D 43 05 98 56 6B 81 " +
            "9A 5E 76 19 11 B3 3C 99 E2 D6 DD FB 3E E8 7E DA " +
            "A1 64 90 E9 9E 7B DF C6 3C 66 36 BD 8D 54 96 C3 " +
            "9F 50 98 60 A8 58 CC 5E 97 8C A0 89 59 1B 92 3B " +
            "5E FA F4 1F 59 4E A6 2A 03 72 C4 18 F3 35 24 60 " +
            "2B 55 02 4D 9B 3A 5F 61",
            // After Rho
            "8B DD B8 27 64 37 6C 47 D8 B8 F3 6B D9 B1 C9 9B " +
            "DD 44 39 7A 43 FD CA 3C 28 74 69 47 C8 EE 03 53 " +
            "C4 FA 67 B9 87 A0 C6 37 46 A7 5D 93 79 32 AF F0 " +
            "C2 40 57 84 2E 96 C2 6A 29 59 1C 27 F6 93 2B 5B " +
            "10 B0 1E 1A F6 EE 5A EF 5F 6A EA 08 C9 30 5C 14 " +
            "78 14 80 DD 94 1A AB 6F 73 82 8E 9D BF AD 27 46 " +
            "89 45 0C 43 9A D9 BD 2A 3F 85 A7 09 6E 27 13 8C " +
            "92 B8 79 02 12 AA 66 82 0A 30 AD D6 02 2B 3A 86 " +
            "2E 23 62 96 27 53 D3 CB 3F 6D 71 EB EE 7D 1F 74 " +
            "EF DB 38 94 0C 32 DD 73 C3 3C 66 36 BD 8D 54 96 " +
            "31 7B 7D 42 61 82 A1 62 5C 32 82 26 66 6D 48 EE " +
            "4B 9F FE 23 CB C9 54 C5 72 C4 18 F3 35 24 60 03 " +
            "57 D8 4A 95 40 D3 A6 CE",
            // After Pi
            "8B DD B8 27 64 37 6C 47 C2 40 57 84 2E 96 C2 6A " +
            "89 45 0C 43 9A D9 BD 2A EF DB 38 94 0C 32 DD 73 " +
            "57 D8 4A 95 40 D3 A6 CE 28 74 69 47 C8 EE 03 53 " +
            "5F 6A EA 08 C9 30 5C 14 78 14 80 DD 94 1A AB 6F " +
            "2E 23 62 96 27 53 D3 CB 4B 9F FE 23 CB C9 54 C5 " +
            "D8 B8 F3 6B D9 B1 C9 9B 29 59 1C 27 F6 93 2B 5B " +
            "3F 85 A7 09 6E 27 13 8C C3 3C 66 36 BD 8D 54 96 " +
            "31 7B 7D 42 61 82 A1 62 C4 FA 67 B9 87 A0 C6 37 " +
            "46 A7 5D 93 79 32 AF F0 73 82 8E 9D BF AD 27 46 " +
            "3F 6D 71 EB EE 7D 1F 74 72 C4 18 F3 35 24 60 03 " +
            "DD 44 39 7A 43 FD CA 3C 10 B0 1E 1A F6 EE 5A EF " +
            "92 B8 79 02 12 AA 66 82 0A 30 AD D6 02 2B 3A 86 " +
            "5C 32 82 26 66 6D 48 EE",
            // After Chi
            "82 D8 B0 64 F4 7E 51 47 A4 DA 67 10 2A B4 82 3B " +
            "99 45 4E 42 DA 18 9F A6 67 DE 88 B6 28 16 95 72 " +
            "17 D8 0D 15 4A 53 24 E6 08 60 69 92 DC E4 A0 38 " +
            "59 49 88 0A EA 71 0C 94 39 88 1C FC 5C 92 AF 6B " +
            "0E 43 63 D2 27 75 D0 D9 1C 95 7C 2B CA D9 08 C1 " +
            "CE 3C 50 63 D1 95 D9 1F E9 61 5C 11 67 1B 6F 49 " +
            "0F C6 BE 49 2E 25 B2 EC 0B BC E4 1F 25 BC 1C 0F " +
            "10 3A 71 46 47 80 83 22 F5 FA E5 B5 01 2D C6 31 " +
            "4A CA 2C F1 39 62 B7 C0 33 02 86 8D AE AD 47 45 " +
            "BB 57 16 E3 6C FD 99 40 70 C1 00 F1 4D 36 49 C3 " +
            "5F 4C 58 7A 43 FD EE 3C 18 B0 9A CE F6 EF 42 EB " +
            "C6 BA 7B 22 76 EE 26 EA 8B 74 94 8E 03 BB B8 96 " +
            "5C 82 84 26 D2 6F 58 2D",
            // After Iota 
            "82 58 B0 E4 F4 7E 51 C7 A4 DA 67 10 2A B4 82 3B " +
            "99 45 4E 42 DA 18 9F A6 67 DE 88 B6 28 16 95 72 " +
            "17 D8 0D 15 4A 53 24 E6 08 60 69 92 DC E4 A0 38 " +
            "59 49 88 0A EA 71 0C 94 39 88 1C FC 5C 92 AF 6B " +
            "0E 43 63 D2 27 75 D0 D9 1C 95 7C 2B CA D9 08 C1 " +
            "CE 3C 50 63 D1 95 D9 1F E9 61 5C 11 67 1B 6F 49 " +
            "0F C6 BE 49 2E 25 B2 EC 0B BC E4 1F 25 BC 1C 0F " +
            "10 3A 71 46 47 80 83 22 F5 FA E5 B5 01 2D C6 31 " +
            "4A CA 2C F1 39 62 B7 C0 33 02 86 8D AE AD 47 45 " +
            "BB 57 16 E3 6C FD 99 40 70 C1 00 F1 4D 36 49 C3 " +
            "5F 4C 58 7A 43 FD EE 3C 18 B0 9A CE F6 EF 42 EB " +
            "C6 BA 7B 22 76 EE 26 EA 8B 74 94 8E 03 BB B8 96 " +
            "5C 82 84 26 D2 6F 58 2D",
            // Round #4
            // After Theta
            "38 7C 3F 23 7C 8B C7 B6 FF 0E 70 7A 71 B3 45 CB " +
            "7B C9 51 5B 38 79 7A 8F 52 05 91 B1 E9 5C 0A 2B " +
            "98 BF E9 B7 78 75 5D 4E B2 44 E6 55 54 11 36 49 " +
            "02 9D 9F 60 B1 76 CB 64 DB 04 03 E5 BE F3 4A 42 " +
            "3B 98 7A D5 E6 3F 4F 80 93 F2 98 89 F8 FF 71 69 " +
            "74 18 DF A4 59 60 4F 6E B2 B5 4B 7B 3C 1C A8 B9 " +
            "ED 4A A1 50 CC 44 57 C5 3E 67 FD 18 E4 F6 83 56 " +
            "9F 5D 95 E4 75 A6 FA 8A 4F DE 6A 72 89 D8 50 40 " +
            "11 1E 3B 9B 62 65 70 30 D1 8E 99 94 4C CC A2 6C " +
            "8E 8C 0F E4 AD B7 06 19 FF A6 E4 53 7F 10 30 6B " +
            "E5 68 D7 BD CB 08 78 4D 43 64 8D A4 AD E8 85 1B " +
            "24 36 64 3B 94 8F C3 C3 BE AF 8D 89 C2 F1 27 CF " +
            "D3 E5 60 84 E0 49 21 85",
            // After Rho
            "38 7C 3F 23 7C 8B C7 B6 FF 1D E0 F4 E2 66 8B 96 " +
            "5E 72 D4 16 4E 9E DE E3 CE A5 B0 22 55 10 19 9B " +
            "AB EB 72 C2 FC 4D BF C5 45 15 61 93 24 4B 64 5E " +
            "09 16 6B B7 4C 26 D0 F9 D0 36 C1 40 B9 EF BC 92 " +
            "4C BD 6A F3 9F 27 C0 1D 1F 97 36 29 8F 99 88 FF " +
            "A3 C3 F8 26 CD 02 7B 72 E6 CA D6 2E ED F1 70 A0 " +
            "85 62 26 BA 2A 6E 57 0A ED 07 AD 7C CE FA 31 C8 " +
            "F2 3A 53 7D C5 CF AE 4A E4 12 B1 A1 80 9E BC D5 " +
            "67 53 AC 0C 0E 26 C2 63 51 B6 68 C7 4C 4A 26 66 " +
            "D6 20 C3 91 F1 81 BC F5 6B FF A6 E4 53 7F 10 30 " +
            "E0 35 95 A3 5D F7 2E 23 0C 91 35 92 B6 A2 17 6E " +
            "C4 86 6C 87 F2 71 78 98 AF 8D 89 C2 F1 27 CF BE " +
            "48 E1 74 39 18 21 78 52",
            // After Pi
            "38 7C 3F 23 7C 8B C7 B6 09 16 6B B7 4C 26 D0 F9 " +
            "85 62 26 BA 2A 6E 57 0A D6 20 C3 91 F1 81 BC F5 " +
            "48 E1 74 39 18 21 78 52 CE A5 B0 22 55 10 19 9B " +
            "1F 97 36 29 8F 99 88 FF A3 C3 F8 26 CD 02 7B 72 " +
            "67 53 AC 0C 0E 26 C2 63 C4 86 6C 87 F2 71 78 98 " +
            "FF 1D E0 F4 E2 66 8B 96 D0 36 C1 40 B9 EF BC 92 " +
            "ED 07 AD 7C CE FA 31 C8 6B FF A6 E4 53 7F 10 30 " +
            "E0 35 95 A3 5D F7 2E 23 AB EB 72 C2 FC 4D BF C5 " +
            "45 15 61 93 24 4B 64 5E E6 CA D6 2E ED F1 70 A0 " +
            "51 B6 68 C7 4C 4A 26 66 AF 8D 89 C2 F1 27 CF BE " +
            "5E 72 D4 16 4E 9E DE E3 4C BD 6A F3 9F 27 C0 1D " +
            "F2 3A 53 7D C5 CF AE 4A E4 12 B1 A1 80 9E BC D5 " +
            "0C 91 35 92 B6 A2 17 6E",
            // After Chi
            "BC 1C 3B 2B 5E C3 C0 B4 5B 16 AA B6 9D A7 78 0C " +
            "8D A3 12 92 22 4E 17 08 E6 3C C8 93 95 0B 3B 51 " +
            "49 E3 34 AD 18 05 68 1B 6E E5 78 24 15 12 6A 9B " +
            "5B 87 32 21 8D BD 08 FE 23 47 B8 A5 3D 53 43 EA " +
            "6D 72 3C 2C 0B 26 C3 60 D5 94 6A 8E 78 F8 F8 FC " +
            "D2 1C CC C8 A4 76 8A DE D2 CE C3 C0 A8 EA BC A2 " +
            "6D 07 BC 7F C2 7A 1F CB 74 F7 C6 B0 F1 7F 91 A4 " +
            "E0 17 94 A3 44 7E 1A 23 09 21 E4 EE 35 FD AF 65 " +
            "54 21 49 52 24 41 62 18 48 C3 57 2E 5C D4 B9 38 " +
            "51 D4 1A C7 40 02 16 27 EB 99 88 D3 F1 25 8F A4 " +
            "EC 70 C5 1A 0E 56 F0 A1 48 BD CA 73 9F 37 D0 88 " +
            "FA BB 57 6F F3 EF AD 60 B6 70 71 A5 C8 82 74 54 " +
            "0C 1C 1F 73 27 83 17 72",
            // After Iota 
            "37 9C 3B 2B 5E C3 C0 B4 5B 16 AA B6 9D A7 78 0C " +
            "8D A3 12 92 22 4E 17 08 E6 3C C8 93 95 0B 3B 51 " +
            "49 E3 34 AD 18 05 68 1B 6E E5 78 24 15 12 6A 9B " +
            "5B 87 32 21 8D BD 08 FE 23 47 B8 A5 3D 53 43 EA " +
            "6D 72 3C 2C 0B 26 C3 60 D5 94 6A 8E 78 F8 F8 FC " +
            "D2 1C CC C8 A4 76 8A DE D2 CE C3 C0 A8 EA BC A2 " +
            "6D 07 BC 7F C2 7A 1F CB 74 F7 C6 B0 F1 7F 91 A4 " +
            "E0 17 94 A3 44 7E 1A 23 09 21 E4 EE 35 FD AF 65 " +
            "54 21 49 52 24 41 62 18 48 C3 57 2E 5C D4 B9 38 " +
            "51 D4 1A C7 40 02 16 27 EB 99 88 D3 F1 25 8F A4 " +
            "EC 70 C5 1A 0E 56 F0 A1 48 BD CA 73 9F 37 D0 88 " +
            "FA BB 57 6F F3 EF AD 60 B6 70 71 A5 C8 82 74 54 " +
            "0C 1C 1F 73 27 83 17 72",
            // Round #5
            // After Theta
            "31 FE D7 E6 AA EA 2F 26 D7 14 29 97 AD 13 B9 DB " +
            "72 5A 78 3E EF 6D 7E 04 A1 6C 65 DA 03 1C 40 04 " +
            "8D 96 31 A7 57 CE 9D 97 68 87 94 E9 E1 3B 85 09 " +
            "D7 85 B1 00 BD 09 C9 29 DC BE D2 09 F0 70 2A E6 " +
            "2A 22 91 65 9D 31 B8 35 11 E1 6F 84 37 33 0D 70 " +
            "D4 7E 20 05 50 5F 65 4C 5E CC 40 E1 98 5E 7D 75 " +
            "92 FE D6 D3 0F 59 76 C7 33 A7 6B F9 67 68 EA F1 " +
            "24 62 91 A9 0B B5 EF AF 0F 43 08 23 C1 D4 40 F7 " +
            "D8 23 CA 73 14 F5 A3 CF B7 3A 3D 82 91 F7 D0 34 " +
            "16 84 B7 8E D6 15 6D 72 2F EC 8D D9 BE EE 7A 28 " +
            "EA 12 29 D7 FA 7F 1F 33 C4 BF 49 52 AF 83 11 5F " +
            "05 42 3D C3 3E CC C4 6C F1 20 DC EC 5E 95 0F 01 " +
            "C8 69 1A 79 68 48 E2 FE",
            // After Rho
            "31 FE D7 E6 AA EA 2F 26 AF 29 52 2E 5B 27 72 B7 " +
            "9C 16 9E CF 7B 9B 1F 81 C0 01 44 10 CA 56 A6 3D " +
            "72 EE BC 6C B4 8C 39 BD 1E BE 53 98 80 76 48 99 " +
            "0B D0 9B 90 9C 72 5D 18 39 B7 AF 74 02 3C 9C 8A " +
            "91 C8 B2 CE 18 DC 1A 15 D3 00 17 11 FE 46 78 33 " +
            "A2 F6 03 29 80 FA 2A 63 D5 79 31 03 85 63 7A F5 " +
            "9E 7E C8 B2 3B 96 F4 B7 D0 D4 E3 67 4E D7 F2 CF " +
            "D4 85 DA F7 57 12 B1 C8 46 82 A9 81 EE 1F 86 10 " +
            "79 8E A2 7E F4 19 7B 44 68 9A 5B 9D 1E C1 C8 7B " +
            "A2 4D CE 82 F0 D6 D1 BA 28 2F EC 8D D9 BE EE 7A " +
            "7D CC A8 4B A4 5C EB FF 11 FF 26 49 BD 0E 46 7C " +
            "40 A8 67 D8 87 99 98 AD 20 DC EC 5E 95 0F 01 F1 " +
            "B8 3F 72 9A 46 1E 1A 92",
            // After Pi
            "31 FE D7 E6 AA EA 2F 26 0B D0 9B 90 9C 72 5D 18 " +
            "9E 7E C8 B2 3B 96 F4 B7 A2 4D CE 82 F0 D6 D1 BA " +
            "B8 3F 72 9A 46 1E 1A 92 C0 01 44 10 CA 56 A6 3D " +
            "D3 00 17 11 FE 46 78 33 A2 F6 03 29 80 FA 2A 63 " +
            "79 8E A2 7E F4 19 7B 44 40 A8 67 D8 87 99 98 AD " +
            "AF 29 52 2E 5B 27 72 B7 39 B7 AF 74 02 3C 9C 8A " +
            "D0 D4 E3 67 4E D7 F2 CF 28 2F EC 8D D9 BE EE 7A " +
            "7D CC A8 4B A4 5C EB FF 72 EE BC 6C B4 8C 39 BD " +
            "1E BE 53 98 80 76 48 99 D5 79 31 03 85 63 7A F5 " +
            "68 9A 5B 9D 1E C1 C8 7B 20 DC EC 5E 95 0F 01 F1 " +
            "9C 16 9E CF 7B 9B 1F 81 91 C8 B2 CE 18 DC 1A 15 " +
            "D4 85 DA F7 57 12 B1 C8 46 82 A9 81 EE 1F 86 10 " +
            "11 FF 26 49 BD 0E 46 7C",
            // After Chi
            "A5 D0 97 C4 89 6E 8F 81 2B D1 9D 90 5C 32 5C 10 " +
            "86 4C F8 AA 3D 9E FE B7 A3 8D 4B E6 58 36 F4 9E " +
            "B2 3F 7A 8A 52 0E 4A 8A E0 F7 44 38 CA EE A4 7D " +
            "8A 08 B7 47 8A 47 29 37 A2 D6 46 A9 83 7A AA CA " +
            "F9 8F A2 7E BC 5F 5D 54 53 A8 74 D9 B3 99 C0 AF " +
            "6F 69 12 2D 17 E4 10 F2 11 9C A3 FC 93 14 90 BA " +
            "85 14 E3 25 6A 97 F3 4A AA 0E BE A9 82 9D FE 7A " +
            "6D 5A 05 1B A4 44 67 F7 B3 AF 9C 6F B1 8D 0B D9 " +
            "36 3C 19 04 9A F6 C8 93 D5 3D 95 41 04 6D 7B 75 " +
            "3A B8 4B BD 3E 41 F0 77 2C CC AF CE 95 7D 41 F1 " +
            "D8 13 D6 FE 3C 99 BE 49 93 CA 93 CE B0 D1 1C 05 " +
            "C5 F8 DC BF 46 12 F1 A4 CA 82 31 07 AC 8E 9F 91 " +
            "10 37 06 49 BD 4A 46 68",
            // After Iota 
            "A4 D0 97 44 89 6E 8F 81 2B D1 9D 90 5C 32 5C 10 " +
            "86 4C F8 AA 3D 9E FE B7 A3 8D 4B E6 58 36 F4 9E " +
            "B2 3F 7A 8A 52 0E 4A 8A E0 F7 44 38 CA EE A4 7D " +
            "8A 08 B7 47 8A 47 29 37 A2 D6 46 A9 83 7A AA CA " +
            "F9 8F A2 7E BC 5F 5D 54 53 A8 74 D9 B3 99 C0 AF " +
            "6F 69 12 2D 17 E4 10 F2 11 9C A3 FC 93 14 90 BA " +
            "85 14 E3 25 6A 97 F3 4A AA 0E BE A9 82 9D FE 7A " +
            "6D 5A 05 1B A4 44 67 F7 B3 AF 9C 6F B1 8D 0B D9 " +
            "36 3C 19 04 9A F6 C8 93 D5 3D 95 41 04 6D 7B 75 " +
            "3A B8 4B BD 3E 41 F0 77 2C CC AF CE 95 7D 41 F1 " +
            "D8 13 D6 FE 3C 99 BE 49 93 CA 93 CE B0 D1 1C 05 " +
            "C5 F8 DC BF 46 12 F1 A4 CA 82 31 07 AC 8E 9F 91 " +
            "10 37 06 49 BD 4A 46 68",
            // Round #6
            // After Theta
            "3E 80 32 49 3B 06 07 DC 08 B4 3E E0 A8 5B 88 42 " +
            "93 93 A1 5D BB AF BF 10 72 AB 1B A1 15 F2 0C EF " +
            "33 ED 40 80 15 D4 6E E1 7A A7 E1 35 78 86 2C 20 " +
            "A9 6D 14 37 7E 2E FD 65 B7 09 1F 5E 05 4B EB 6D " +
            "28 A9 F2 39 F1 9B A5 25 D2 7A 4E D3 F4 43 E4 C4 " +
            "F5 39 B7 20 A5 8C 98 AF 32 F9 00 8C 67 7D 44 E8 " +
            "90 CB BA D2 EC A6 B2 ED 7B 28 EE EE CF 59 06 0B " +
            "EC 88 3F 11 E3 9E 43 9C 29 FF 39 62 03 E5 83 84 " +
            "15 59 BA 74 6E 9F 1C C1 C0 E2 CC B6 82 5C 3A D2 " +
            "EB 9E 1B FA 73 85 08 06 AD 1E 95 C4 D2 A7 65 9A " +
            "42 43 73 F3 8E F1 36 14 B0 AF 30 BE 44 B8 C8 57 " +
            "D0 27 85 48 C0 23 B0 03 1B A4 61 40 E1 4A 67 E0 " +
            "91 E5 3C 43 FA 90 62 03",
            // After Rho
            "3E 80 32 49 3B 06 07 DC 10 68 7D C0 51 B7 10 85 " +
            "E4 64 68 D7 EE EB 2F C4 21 CF F0 2E B7 BA 11 5A " +
            "A0 76 0B 9F 69 07 02 AC 83 67 C8 02 A2 77 1A 5E " +
            "71 E3 E7 D2 5F 96 DA 46 DB 6D C2 87 57 C1 D2 7A " +
            "54 F9 9C F8 CD D2 12 94 44 4E 2C AD E7 34 4D 3F " +
            "AD CF B9 05 29 65 C4 7C A1 CB E4 03 30 9E F5 11 " +
            "95 66 37 95 6D 87 5C D6 B3 0C 16 F6 50 DC DD 9F " +
            "88 71 CF 21 4E 76 C4 9F C4 06 CA 07 09 53 FE 73 " +
            "97 CE ED 93 23 B8 22 4B 1D 69 60 71 66 5B 41 2E " +
            "10 C1 60 DD 73 43 7F AE 9A AD 1E 95 C4 D2 A7 65 " +
            "DB 50 08 0D CD CD 3B C6 C1 BE C2 F8 12 E1 22 5F " +
            "FA A4 10 09 78 04 76 00 A4 61 40 E1 4A 67 E0 1B " +
            "D8 40 64 39 CF 90 3E A4",
            // After Pi
            "3E 80 32 49 3B 06 07 DC 71 E3 E7 D2 5F 96 DA 46 " +
            "95 66 37 95 6D 87 5C D6 10 C1 60 DD 73 43 7F AE " +
            "D8 40 64 39 CF 90 3E A4 21 CF F0 2E B7 BA 11 5A " +
            "44 4E 2C AD E7 34 4D 3F AD CF B9 05 29 65 C4 7C " +
            "97 CE ED 93 23 B8 22 4B FA A4 10 09 78 04 76 00 " +
            "10 68 7D C0 51 B7 10 85 DB 6D C2 87 57 C1 D2 7A " +
            "B3 0C 16 F6 50 DC DD 9F 9A AD 1E 95 C4 D2 A7 65 " +
            "DB 50 08 0D CD CD 3B C6 A0 76 0B 9F 69 07 02 AC " +
            "83 67 C8 02 A2 77 1A 5E A1 CB E4 03 30 9E F5 11 " +
            "1D 69 60 71 66 5B 41 2E A4 61 40 E1 4A 67 E0 1B " +
            "E4 64 68 D7 EE EB 2F C4 54 F9 9C F8 CD D2 12 94 " +
            "88 71 CF 21 4E 76 C4 9F C4 06 CA 07 09 53 FE 73 " +
            "C1 BE C2 F8 12 E1 22 5F",
            // After Chi
            "BA 84 22 4C 1B 07 03 4C 71 62 A7 9A 4D D6 F9 6E " +
            "5D 66 33 B5 E1 17 5C D6 36 41 72 9D 43 45 7E F6 " +
            "99 23 A1 AB 8B 00 E6 A6 88 4E 61 2E BF FB 91 1A " +
            "56 4E 68 3F E5 AC 6F 3C C5 EF A9 0D 71 61 90 7C " +
            "96 85 0D B5 A4 02 23 11 BE A4 1C 88 38 00 3A 25 " +
            "30 68 69 B0 51 AB 1D 00 D3 CC CA 86 D3 C3 F0 1A " +
            "F2 5C 16 FE 59 D1 C5 1D 9A 85 6B 55 D4 E0 A7 64 " +
            "10 55 8A 0A CB 8D F9 BC 80 FE 2F 9E 79 8F E7 AD " +
            "9F 47 C8 72 E4 36 1A 70 01 CB E4 83 38 BA 55 00 " +
            "1D 7F 6B 6F 47 5B 43 8A A7 60 80 E1 C8 17 F8 49 " +
            "6C 64 2B D6 EC CF EB CF 10 FF 9C FE CC D3 28 F4 " +
            "89 C9 CF D9 5C D6 C4 93 E0 46 E2 00 E5 59 F3 F3 " +
            "D1 27 56 D0 13 F1 32 4F",
            // After Iota 
            "3B 04 22 CC 1B 07 03 CC 71 62 A7 9A 4D D6 F9 6E " +
            "5D 66 33 B5 E1 17 5C D6 36 41 72 9D 43 45 7E F6 " +
            "99 23 A1 AB 8B 00 E6 A6 88 4E 61 2E BF FB 91 1A " +
            "56 4E 68 3F E5 AC 6F 3C C5 EF A9 0D 71 61 90 7C " +
            "96 85 0D B5 A4 02 23 11 BE A4 1C 88 38 00 3A 25 " +
            "30 68 69 B0 51 AB 1D 00 D3 CC CA 86 D3 C3 F0 1A " +
            "F2 5C 16 FE 59 D1 C5 1D 9A 85 6B 55 D4 E0 A7 64 " +
            "10 55 8A 0A CB 8D F9 BC 80 FE 2F 9E 79 8F E7 AD " +
            "9F 47 C8 72 E4 36 1A 70 01 CB E4 83 38 BA 55 00 " +
            "1D 7F 6B 6F 47 5B 43 8A A7 60 80 E1 C8 17 F8 49 " +
            "6C 64 2B D6 EC CF EB CF 10 FF 9C FE CC D3 28 F4 " +
            "89 C9 CF D9 5C D6 C4 93 E0 46 E2 00 E5 59 F3 F3 " +
            "D1 27 56 D0 13 F1 32 4F",
            // Round #7
            // After Theta
            "8D 21 61 8A 1F D4 44 6D DA 75 C6 B9 77 56 4B 93 " +
            "A9 CF 58 3F 90 00 9D EE 56 BC 16 B0 A8 59 38 A1 " +
            "81 2B 61 8D DA 8B AA 35 3E 6B 22 68 BB 28 D6 BB " +
            "FD 59 09 1C DF 2C DD C1 31 46 C2 87 00 76 51 44 " +
            "F6 78 69 98 4F 1E 65 46 A6 AC DC AE 69 8B 76 B6 " +
            "86 4D 2A F6 55 78 5A A1 78 DB AB A5 E9 43 42 E7 " +
            "06 F5 7D 74 28 C6 04 25 FA 78 0F 78 3F FC E1 33 " +
            "08 5D 4A 2C 9A 06 B5 2F 36 DB 6C D8 7D 5C A0 0C " +
            "34 50 A9 51 DE B6 A8 8D F5 62 8F 09 49 AD 94 38 " +
            "7D 82 0F 42 AC 47 05 DD BF 68 40 C7 99 9C B4 DA " +
            "DA 41 68 90 E8 1C AC 6E BB E8 FD DD F6 53 9A 09 " +
            "7D 60 A4 53 2D C1 05 AB 80 BB 86 2D 0E 45 B5 A4 " +
            "C9 2F 96 F6 42 7A 7E DC",
            // After Rho
            "8D 21 61 8A 1F D4 44 6D B5 EB 8C 73 EF AC 96 26 " +
            "EA 33 D6 0F 24 40 A7 7B 9A 85 13 6A C5 6B 01 8B " +
            "5E 54 AD 09 5C 09 6B D4 B6 8B 62 BD EB B3 26 82 " +
            "C0 F1 CD D2 1D DC 9F 95 51 8C 91 F0 21 80 5D 14 " +
            "BC 34 CC 27 8F 32 23 7B 68 67 6B CA CA ED 9A B6 " +
            "35 6C 52 B1 AF C2 D3 0A 9D E3 6D AF 96 A6 0F 09 " +
            "A3 43 31 26 28 31 A8 EF F8 C3 67 F4 F1 1E F0 7E " +
            "16 4D 83 DA 17 84 2E 25 B0 FB B8 40 19 6C B6 D9 " +
            "35 CA DB 16 B5 91 06 2A 4A 9C 7A B1 C7 84 A4 56 " +
            "A8 A0 BB 4F F0 41 88 F5 DA BF 68 40 C7 99 9C B4 " +
            "B0 BA 69 07 A1 41 A2 73 EC A2 F7 77 DB 4F 69 26 " +
            "0F 8C 74 AA 25 B8 60 B5 BB 86 2D 0E 45 B5 A4 80 " +
            "1F 77 F2 8B A5 BD 90 9E",
            // After Pi
            "8D 21 61 8A 1F D4 44 6D C0 F1 CD D2 1D DC 9F 95 " +
            "A3 43 31 26 28 31 A8 EF A8 A0 BB 4F F0 41 88 F5 " +
            "1F 77 F2 8B A5 BD 90 9E 9A 85 13 6A C5 6B 01 8B " +
            "68 67 6B CA CA ED 9A B6 35 6C 52 B1 AF C2 D3 0A " +
            "35 CA DB 16 B5 91 06 2A 0F 8C 74 AA 25 B8 60 B5 " +
            "B5 EB 8C 73 EF AC 96 26 51 8C 91 F0 21 80 5D 14 " +
            "F8 C3 67 F4 F1 1E F0 7E DA BF 68 40 C7 99 9C B4 " +
            "B0 BA 69 07 A1 41 A2 73 5E 54 AD 09 5C 09 6B D4 " +
            "B6 8B 62 BD EB B3 26 82 9D E3 6D AF 96 A6 0F 09 " +
            "4A 9C 7A B1 C7 84 A4 56 BB 86 2D 0E 45 B5 A4 80 " +
            "EA 33 D6 0F 24 40 A7 7B BC 34 CC 27 8F 32 23 7B " +
            "16 4D 83 DA 17 84 2E 25 B0 FB B8 40 19 6C B6 D9 " +
            "EC A2 F7 77 DB 4F 69 26",
            // After Chi
            "AE 23 51 AE 3F F5 64 07 C8 51 47 9B CD 9C 9F 85 " +
            "B4 14 71 A6 2D 8D B8 E5 28 A0 BA 4F EA 01 CC 94 " +
            "5F A7 7E DB A5 B5 0B 0E 8F 8D 03 5B E0 69 40 83 " +
            "68 E5 E2 CC DA FC 9E 96 3F 68 76 19 AF EA B3 9F " +
            "A5 CB D8 56 75 D2 07 20 6F EE 1C 2A 2F 3C FA 81 " +
            "1D A8 EA 77 3F B2 36 4C 53 B0 99 F0 27 01 51 94 " +
            "D8 C3 66 F3 D1 5E D2 3D DF FE EC 30 89 35 88 B0 " +
            "F0 BE 78 87 A1 41 EB 63 57 34 A0 0B 48 0D 62 DD " +
            "F4 97 70 AD AA B3 86 D4 2C E1 68 A1 96 97 0F 89 " +
            "0E CC FA B0 DF 8C EF 02 1B 0D 6F BA E6 07 A0 82 " +
            "E8 7A D5 D7 34 C4 AB 7F 1C 86 F4 27 87 5A B3 A3 " +
            "5A 4D C4 ED D5 87 67 03 B2 EA B8 48 3D 6C 30 80 " +
            "F8 A6 FF 57 50 7D 69 26",
            // After Iota 
            "A7 A3 51 AE 3F F5 64 87 C8 51 47 9B CD 9C 9F 85 " +
            "B4 14 71 A6 2D 8D B8 E5 28 A0 BA 4F EA 01 CC 94 " +
            "5F A7 7E DB A5 B5 0B 0E 8F 8D 03 5B E0 69 40 83 " +
            "68 E5 E2 CC DA FC 9E 96 3F 68 76 19 AF EA B3 9F " +
            "A5 CB D8 56 75 D2 07 20 6F EE 1C 2A 2F 3C FA 81 " +
            "1D A8 EA 77 3F B2 36 4C 53 B0 99 F0 27 01 51 94 " +
            "D8 C3 66 F3 D1 5E D2 3D DF FE EC 30 89 35 88 B0 " +
            "F0 BE 78 87 A1 41 EB 63 57 34 A0 0B 48 0D 62 DD " +
            "F4 97 70 AD AA B3 86 D4 2C E1 68 A1 96 97 0F 89 " +
            "0E CC FA B0 DF 8C EF 02 1B 0D 6F BA E6 07 A0 82 " +
            "E8 7A D5 D7 34 C4 AB 7F 1C 86 F4 27 87 5A B3 A3 " +
            "5A 4D C4 ED D5 87 67 03 B2 EA B8 48 3D 6C 30 80 " +
            "F8 A6 FF 57 50 7D 69 26",
            // Round #8
            // After Theta
            "B3 D5 AB 6E 98 57 7C 2F 09 BF 10 C4 71 29 26 F4 " +
            "72 66 50 28 D9 08 E5 18 4B 0B 63 78 C1 4D DA C8 " +
            "A4 85 29 B7 69 7C 20 5D 9B FB F9 9B 47 CB 58 2B " +
            "A9 0B B5 93 66 49 27 E7 F9 1A 57 97 5B 6F EE 62 " +
            "C6 60 01 61 5E 9E 11 7C 94 CC 4B 46 E3 F5 D1 D2 " +
            "09 DE 10 B7 98 10 2E E4 92 5E CE AF 9B B4 E8 E5 " +
            "1E B1 47 7D 25 DB 8F C0 BC 55 35 07 A2 79 9E EC " +
            "0B 9C 2F EB 6D 88 C0 30 43 42 5A CB EF AF 7A 75 " +
            "35 79 27 F2 16 06 3F A5 EA 93 49 2F 62 12 52 74 " +
            "6D 67 23 87 F4 C0 F9 5E E0 2F 38 D6 2A CE 8B D1 " +
            "FC 0C 2F 17 93 66 B3 D7 DD 68 A3 78 3B EF 0A D2 " +
            "9C 3F E5 63 21 02 3A FE D1 41 61 7F 16 20 26 DC " +
            "03 84 A8 3B 9C B4 42 75",
            // After Rho
            "B3 D5 AB 6E 98 57 7C 2F 13 7E 21 88 E3 52 4C E8 " +
            "9C 19 14 4A 36 42 39 86 DC A4 8D BC B4 30 86 17 " +
            "E3 03 E9 22 2D 4C B9 4D 79 B4 8C B5 B2 B9 9F BF " +
            "3B 69 96 74 72 9E BA 50 58 BE C6 D5 E5 D6 9B BB " +
            "B0 80 30 2F CF 08 3E 63 1F 2D 4D C9 BC 64 34 5E " +
            "4F F0 86 B8 C5 84 70 21 97 4B 7A 39 BF 6E D2 A2 " +
            "EA 2B D9 7E 04 F6 88 3D F3 3C D9 79 AB 6A 0E 44 " +
            "F5 36 44 60 98 05 CE 97 96 DF 5F F5 EA 86 84 B4 " +
            "44 DE C2 E0 A7 B4 26 EF 29 3A F5 C9 A4 17 31 09 " +
            "38 DF AB ED 6C E4 90 1E D1 E0 2F 38 D6 2A CE 8B " +
            "CD 5E F3 33 BC 5C 4C 9A 77 A3 8D E2 ED BC 2B 48 " +
            "F3 A7 7C 2C 44 40 C7 9F 41 61 7F 16 20 26 DC D1 " +
            "50 DD 00 21 EA 0E 27 AD",
            // After Pi
            "B3 D5 AB 6E 98 57 7C 2F 3B 69 96 74 72 9E BA 50 " +
            "EA 2B D9 7E 04 F6 88 3D 38 DF AB ED 6C E4 90 1E " +
            "50 DD 00 21 EA 0E 27 AD DC A4 8D BC B4 30 86 17 " +
            "1F 2D 4D C9 BC 64 34 5E 4F F0 86 B8 C5 84 70 21 " +
            "44 DE C2 E0 A7 B4 26 EF F3 A7 7C 2C 44 40 C7 9F " +
            "13 7E 21 88 E3 52 4C E8 58 BE C6 D5 E5 D6 9B BB " +
            "F3 3C D9 79 AB 6A 0E 44 D1 E0 2F 38 D6 2A CE 8B " +
            "CD 5E F3 33 BC 5C 4C 9A E3 03 E9 22 2D 4C B9 4D " +
            "79 B4 8C B5 B2 B9 9F BF 97 4B 7A 39 BF 6E D2 A2 " +
            "29 3A F5 C9 A4 17 31 09 41 61 7F 16 20 26 DC D1 " +
            "9C 19 14 4A 36 42 39 86 B0 80 30 2F CF 08 3E 63 " +
            "F5 36 44 60 98 05 CE 97 96 DF 5F F5 EA 86 84 B4 " +
            "77 A3 8D E2 ED BC 2B 48",
            // After Chi
            "73 D7 E2 64 9C 37 7C 02 2B BD B4 F5 1A 9E AA 52 " +
            "AA 2B D9 7E 86 FC AF 9C 9B DF 00 A3 7C B5 C8 1C " +
            "58 F5 14 31 88 86 A5 FD 9C 74 0F 8C F5 B0 C6 36 " +
            "1F 23 0D 89 9E 54 32 90 FC D1 BA B4 85 C4 B1 31 " +
            "48 DE 43 70 17 84 26 EF F0 AE 3C 6D 4C 04 F7 D7 " +
            "B0 7E 38 A0 E9 7A 48 AC 58 7E E0 D5 B1 D6 5B 30 " +
            "FF 22 09 7A 83 3E 0E 54 C3 C0 2F B0 95 28 CE EB " +
            "85 DE 35 66 B8 D8 DF 89 65 48 9B 2A 20 0A F9 4D " +
            "51 84 09 75 B2 A8 BE B6 D7 0A 70 2F BF 4E 1E 72 " +
            "8B 38 75 E9 A9 5F 10 05 59 D5 7B 83 B2 97 DA 63 " +
            "D9 2F 50 0A 26 47 F9 12 B2 49 2B BA AD 8A 3E 43 " +
            "94 16 C4 62 9D 3D E5 DF 1E C7 4F FD F8 C4 94 32 " +
            "57 23 AD C7 24 B4 2D 29",
            // After Iota 
            "F9 D7 E2 64 9C 37 7C 02 2B BD B4 F5 1A 9E AA 52 " +
            "AA 2B D9 7E 86 FC AF 9C 9B DF 00 A3 7C B5 C8 1C " +
            "58 F5 14 31 88 86 A5 FD 9C 74 0F 8C F5 B0 C6 36 " +
            "1F 23 0D 89 9E 54 32 90 FC D1 BA B4 85 C4 B1 31 " +
            "48 DE 43 70 17 84 26 EF F0 AE 3C 6D 4C 04 F7 D7 " +
            "B0 7E 38 A0 E9 7A 48 AC 58 7E E0 D5 B1 D6 5B 30 " +
            "FF 22 09 7A 83 3E 0E 54 C3 C0 2F B0 95 28 CE EB " +
            "85 DE 35 66 B8 D8 DF 89 65 48 9B 2A 20 0A F9 4D " +
            "51 84 09 75 B2 A8 BE B6 D7 0A 70 2F BF 4E 1E 72 " +
            "8B 38 75 E9 A9 5F 10 05 59 D5 7B 83 B2 97 DA 63 " +
            "D9 2F 50 0A 26 47 F9 12 B2 49 2B BA AD 8A 3E 43 " +
            "94 16 C4 62 9D 3D E5 DF 1E C7 4F FD F8 C4 94 32 " +
            "57 23 AD C7 24 B4 2D 29",
            // Round #9
            // After Theta
            "C4 FF DF D6 22 32 80 E5 96 8E 17 66 D9 C5 8E 3C " +
            "2F 7B 0E F6 F2 C7 A5 C4 36 FD 48 A3 0A 33 D7 9A " +
            "0E BF 7F 96 2B 65 E4 5D A1 5C 32 3E 4B B5 3A D1 " +
            "A2 10 AE 1A 5D 0F 16 FE 79 81 6D 3C F1 FF BB 69 " +
            "E5 FC 0B 70 61 02 39 69 A6 E4 57 CA EF E7 B6 77 " +
            "8D 56 05 12 57 7F B4 4B E5 4D 43 46 72 8D 7F 5E " +
            "7A 72 DE F2 F7 05 04 0C 6E E2 67 B0 E3 AE D1 6D " +
            "D3 94 5E C1 1B 3B 9E 29 58 60 A6 98 9E 0F 05 AA " +
            "EC B7 AA E6 71 F3 9A D8 52 5A A7 A7 CB 75 14 2A " +
            "26 1A 3D E9 DF D9 0F 83 0F 9F 10 24 11 74 9B C3 " +
            "E4 07 6D B8 98 42 05 F5 0F 7A 88 29 6E D1 1A 2D " +
            "11 46 13 EA E9 06 EF 87 B3 E5 07 FD 8E 42 8B B4 " +
            "01 69 C6 60 87 57 6C 89",
            // After Rho
            "C4 FF DF D6 22 32 80 E5 2C 1D 2F CC B2 8B 1D 79 " +
            "CB 9E 83 BD FC 71 29 F1 30 73 AD 69 D3 8F 34 AA " +
            "29 23 EF 72 F8 FD B3 5C B3 54 AB 13 1D CA 25 E3 " +
            "AA D1 F5 60 E1 2F 0A E1 5A 5E 60 1B 4F FC FF 6E " +
            "FE 05 B8 30 81 9C B4 72 6E 7B 67 4A 7E A5 FC 7E " +
            "6A B4 2A 90 B8 FA A3 5D 79 95 37 0D 19 C9 35 FE " +
            "96 BF 2F 20 60 D0 93 F3 5D A3 DB DC C4 CF 60 C7 " +
            "E0 8D 1D CF 94 69 4A AF 31 3D 1F 0A 54 B1 C0 4C " +
            "D5 3C 6E 5E 13 9B FD 56 0A 15 29 AD D3 D3 E5 3A " +
            "FB 61 D0 44 A3 27 FD 3B C3 0F 9F 10 24 11 74 9B " +
            "15 D4 93 1F B4 E1 62 0A 3C E8 21 A6 B8 45 6B B4 " +
            "C2 68 42 3D DD E0 FD 30 E5 07 FD 8E 42 8B B4 B3 " +
            "5B 62 40 9A 31 D8 E1 15",
            // After Pi
            "C4 FF DF D6 22 32 80 E5 AA D1 F5 60 E1 2F 0A E1 " +
            "96 BF 2F 20 60 D0 93 F3 FB 61 D0 44 A3 27 FD 3B " +
            "5B 62 40 9A 31 D8 E1 15 30 73 AD 69 D3 8F 34 AA " +
            "6E 7B 67 4A 7E A5 FC 7E 6A B4 2A 90 B8 FA A3 5D " +
            "D5 3C 6E 5E 13 9B FD 56 C2 68 42 3D DD E0 FD 30 " +
            "2C 1D 2F CC B2 8B 1D 79 5A 5E 60 1B 4F FC FF 6E " +
            "5D A3 DB DC C4 CF 60 C7 C3 0F 9F 10 24 11 74 9B " +
            "15 D4 93 1F B4 E1 62 0A 29 23 EF 72 F8 FD B3 5C " +
            "B3 54 AB 13 1D CA 25 E3 79 95 37 0D 19 C9 35 FE " +
            "0A 15 29 AD D3 D3 E5 3A E5 07 FD 8E 42 8B B4 B3 " +
            "CB 9E 83 BD FC 71 29 F1 FE 05 B8 30 81 9C B4 72 " +
            "E0 8D 1D CF 94 69 4A AF 31 3D 1F 0A 54 B1 C0 4C " +
            "3C E8 21 A6 B8 45 6B B4",
            // After Chi
            "D0 D1 D5 D6 22 E2 11 F7 C3 91 25 24 62 08 66 E9 " +
            "96 BD 2F BA 70 08 93 F7 7F FC 4F 00 A1 05 FD DB " +
            "71 62 60 BA F0 D5 EB 15 30 F7 A5 F9 53 D5 37 AB " +
            "FB 73 23 04 7D A4 A0 7C 68 F4 2A B1 74 9A A3 7D " +
            "E5 2F C3 1E 11 94 FD DC 8C 60 00 3F F1 C0 35 64 " +
            "29 BC B4 08 32 88 1D F8 D8 52 64 1B 6F EC EB 76 " +
            "49 73 DB D3 54 2F 62 C7 EB 06 B3 D0 26 1B 69 EA " +
            "47 96 D3 0C F9 95 80 0C 61 A2 FB 7E F8 FC A3 40 " +
            "B1 54 A3 B3 DF D8 E5 E3 9C 97 E3 0F 19 C1 25 7F " +
            "02 35 2B DD 6B A7 E6 76 77 53 FD 8F 47 89 B0 10 " +
            "CB 16 86 72 E8 10 63 7C EF 35 BA 30 C1 0C 34 32 " +
            "EC 4D 3D 6B 3C 2D 61 1F F2 2B 9D 13 10 81 C0 0D " +
            "08 E9 19 A6 B9 C9 FF B6",
            // After Iota 
            "58 D1 D5 D6 22 E2 11 F7 C3 91 25 24 62 08 66 E9 " +
            "96 BD 2F BA 70 08 93 F7 7F FC 4F 00 A1 05 FD DB " +
            "71 62 60 BA F0 D5 EB 15 30 F7 A5 F9 53 D5 37 AB " +
            "FB 73 23 04 7D A4 A0 7C 68 F4 2A B1 74 9A A3 7D " +
            "E5 2F C3 1E 11 94 FD DC 8C 60 00 3F F1 C0 35 64 " +
            "29 BC B4 08 32 88 1D F8 D8 52 64 1B 6F EC EB 76 " +
            "49 73 DB D3 54 2F 62 C7 EB 06 B3 D0 26 1B 69 EA " +
            "47 96 D3 0C F9 95 80 0C 61 A2 FB 7E F8 FC A3 40 " +
            "B1 54 A3 B3 DF D8 E5 E3 9C 97 E3 0F 19 C1 25 7F " +
            "02 35 2B DD 6B A7 E6 76 77 53 FD 8F 47 89 B0 10 " +
            "CB 16 86 72 E8 10 63 7C EF 35 BA 30 C1 0C 34 32 " +
            "EC 4D 3D 6B 3C 2D 61 1F F2 2B 9D 13 10 81 C0 0D " +
            "08 E9 19 A6 B9 C9 FF B6",
            // Round #10
            // After Theta
            "E1 5C 75 06 F9 0A F9 49 A6 7E 9D 77 DA F9 B1 2B " +
            "2B FB 47 03 C4 C5 F0 E9 33 41 E1 FC D9 D4 C8 40 " +
            "27 F4 9B ED BB DF 52 B2 89 7A 05 29 88 3D DF 15 " +
            "9E 9C 9B 57 C5 55 77 BE D5 B2 42 08 C0 57 C0 63 " +
            "A9 92 6D E2 69 45 C8 47 DA F6 FB 68 BA CA 8C C3 " +
            "90 31 14 D8 E9 60 F5 46 BD BD DC 48 D7 1D 3C B4 " +
            "F4 35 B3 6A E0 E2 01 D9 A7 BB 1D 2C 5E CA 5C 71 " +
            "11 00 28 5B B2 9F 39 AB D8 2F 5B AE 23 14 4B FE " +
            "D4 BB 1B E0 67 29 32 21 21 D1 8B B6 AD 0C 46 61 " +
            "4E 88 85 21 13 76 D3 ED 21 C5 06 D8 0C 83 09 B7 " +
            "72 9B 26 A2 33 F8 8B C2 8A DA 02 63 79 FD E3 F0 " +
            "51 0B 55 D2 88 E0 02 01 BE 96 33 EF 68 50 F5 96 " +
            "5E 7F E2 F1 F2 C3 46 11",
            // After Rho
            "E1 5C 75 06 F9 0A F9 49 4C FD 3A EF B4 F3 63 57 " +
            "CA FE D1 00 71 31 7C FA 4D 8D 0C 34 13 14 CE 9F " +
            "FD 96 92 3D A1 DF 6C DF 82 D8 F3 5D 91 A8 57 90 " +
            "79 55 5C 75 E7 EB C9 B9 58 B5 AC 10 02 F0 15 F0 " +
            "C9 36 F1 B4 22 E4 A3 54 CC 38 AC 6D BF 8F A6 AB " +
            "82 8C A1 C0 4E 07 AB 37 D0 F6 F6 72 23 5D 77 F0 " +
            "55 03 17 0F C8 A6 AF 99 94 B9 E2 4E 77 3B 58 BC " +
            "2D D9 CF 9C D5 08 00 94 5C 47 28 96 FC B1 5F B6 " +
            "03 FC 2C 45 26 84 7A 77 A3 B0 90 E8 45 DB 56 06 " +
            "6E BA DD 09 B1 30 64 C2 B7 21 C5 06 D8 0C 83 09 " +
            "2F 0A CB 6D 9A 88 CE E0 2B 6A 0B 8C E5 F5 8F C3 " +
            "6A A1 4A 1A 11 5C 20 20 96 33 EF 68 50 F5 96 BE " +
            "51 84 D7 9F 78 BC FC B0",
            // After Pi
            "E1 5C 75 06 F9 0A F9 49 79 55 5C 75 E7 EB C9 B9 " +
            "55 03 17 0F C8 A6 AF 99 6E BA DD 09 B1 30 64 C2 " +
            "51 84 D7 9F 78 BC FC B0 4D 8D 0C 34 13 14 CE 9F " +
            "CC 38 AC 6D BF 8F A6 AB 82 8C A1 C0 4E 07 AB 37 " +
            "03 FC 2C 45 26 84 7A 77 6A A1 4A 1A 11 5C 20 20 " +
            "4C FD 3A EF B4 F3 63 57 58 B5 AC 10 02 F0 15 F0 " +
            "94 B9 E2 4E 77 3B 58 BC B7 21 C5 06 D8 0C 83 09 " +
            "2F 0A CB 6D 9A 88 CE E0 FD 96 92 3D A1 DF 6C DF " +
            "82 D8 F3 5D 91 A8 57 90 D0 F6 F6 72 23 5D 77 F0 " +
            "A3 B0 90 E8 45 DB 56 06 96 33 EF 68 50 F5 96 BE " +
            "CA FE D1 00 71 31 7C FA C9 36 F1 B4 22 E4 A3 54 " +
            "2D D9 CF 9C D5 08 00 94 5C 47 28 96 FC B1 5F B6 " +
            "2B 6A 0B 8C E5 F5 8F C3",
            // After Chi
            "E5 5E 76 0C F1 0E DF 49 53 ED 94 75 D6 FB 89 FB " +
            "44 07 15 99 80 2A 37 A9 CE E2 FD 09 30 32 65 8B " +
            "49 85 DF EE 7E 5D FC 00 4F 09 0D B4 53 14 C7 8B " +
            "CD 48 A0 68 9F 0F F6 EB EA 8D E3 DA 5F 5F AB 37 " +
            "06 F0 28 61 24 84 B4 E8 EA 91 EA 53 BD D7 00 00 " +
            "C8 F5 78 A1 C1 F8 2B 5B 7B B5 A9 10 8A F4 96 F1 " +
            "9C B3 E8 27 75 BB 14 5C F7 D4 F5 84 FC 7F A2 1E " +
            "3F 0A 4F 7D 98 88 DA 40 AD B0 96 1F 83 8A 4C BF " +
            "A1 D8 F3 D5 D5 2A 57 96 C4 F5 99 72 33 79 F7 48 " +
            "CA 34 80 FD E4 D1 3E 47 94 7B 8E 28 40 D5 85 BE " +
            "EE 37 DF 08 A4 39 7C 7A 99 30 D1 B6 0A 55 FC 76 " +
            "0E F1 CC 94 D4 4C 80 D5 9C D3 F8 96 EC B1 2F 8E " +
            "2A 6A 2B 38 E7 31 0C C7",
            // After Iota 
            "EC DE 76 8C F1 0E DF 49 53 ED 94 75 D6 FB 89 FB " +
            "44 07 15 99 80 2A 37 A9 CE E2 FD 09 30 32 65 8B " +
            "49 85 DF EE 7E 5D FC 00 4F 09 0D B4 53 14 C7 8B " +
            "CD 48 A0 68 9F 0F F6 EB EA 8D E3 DA 5F 5F AB 37 " +
            "06 F0 28 61 24 84 B4 E8 EA 91 EA 53 BD D7 00 00 " +
            "C8 F5 78 A1 C1 F8 2B 5B 7B B5 A9 10 8A F4 96 F1 " +
            "9C B3 E8 27 75 BB 14 5C F7 D4 F5 84 FC 7F A2 1E " +
            "3F 0A 4F 7D 98 88 DA 40 AD B0 96 1F 83 8A 4C BF " +
            "A1 D8 F3 D5 D5 2A 57 96 C4 F5 99 72 33 79 F7 48 " +
            "CA 34 80 FD E4 D1 3E 47 94 7B 8E 28 40 D5 85 BE " +
            "EE 37 DF 08 A4 39 7C 7A 99 30 D1 B6 0A 55 FC 76 " +
            "0E F1 CC 94 D4 4C 80 D5 9C D3 F8 96 EC B1 2F 8E " +
            "2A 6A 2B 38 E7 31 0C C7",
            // Round #11
            // After Theta
            "74 20 D6 81 35 16 F4 72 8B 33 48 FF 09 5C 75 18 " +
            "4A BD 1A F9 5D 06 B0 C0 72 C1 08 2A 84 04 C5 A7 " +
            "70 EE 12 75 17 56 98 0C D7 F7 AD B9 97 0C EC B0 " +
            "15 96 7C E2 40 A8 0A 08 E4 37 EC BA 82 73 2C 5E " +
            "BA D3 DD 42 90 B2 14 C4 D3 FA 27 C8 D4 DC 64 0C " +
            "50 0B D8 AC 05 E0 00 60 A3 6B 75 9A 55 53 6A 12 " +
            "92 09 E7 47 A8 97 93 35 4B F7 00 A7 48 49 02 32 " +
            "06 61 82 E6 F1 83 BE 4C 35 4E 36 12 47 92 67 84 " +
            "79 06 2F 5F 0A 8D AB 75 CA 4F 96 12 EE 55 70 21 " +
            "76 17 75 DE 50 E7 9E 6B AD 10 43 B3 29 DE E1 B2 " +
            "76 C9 7F 05 60 21 57 41 41 EE 0D 3C D5 F2 00 95 " +
            "00 4B C3 F4 09 60 07 BC 20 F0 0D B5 58 87 8F A2 " +
            "13 01 E6 A3 8E 3A 68 CB",
            // After Rho
            "74 20 D6 81 35 16 F4 72 16 67 90 FE 13 B8 EA 30 " +
            "52 AF 46 7E 97 01 2C B0 48 50 7C 2A 17 8C A0 42 " +
            "B0 C2 64 80 73 97 A8 BB 7B C9 C0 0E 7B 7D DF 9A " +
            "27 0E 84 AA 80 50 61 C9 17 F9 0D BB AE E0 1C 8B " +
            "E9 6E 21 48 59 0A 62 DD 4D C6 30 AD 7F 82 4C CD " +
            "83 5A C0 66 2D 00 07 00 49 8C AE D5 69 56 4D A9 " +
            "3F 42 BD 9C AC 91 4C 38 92 04 64 96 EE 01 4E 91 " +
            "F3 F8 41 5F 26 83 30 41 24 8E 24 CF 08 6B 9C 6C " +
            "E5 4B A1 71 B5 2E CF E0 B8 10 E5 27 4B 09 F7 2A " +
            "DC 73 CD EE A2 CE 1B EA B2 AD 10 43 B3 29 DE E1 " +
            "5C 05 D9 25 FF 15 80 85 06 B9 37 F0 54 CB 03 54 " +
            "60 69 98 3E 01 EC 80 17 F0 0D B5 58 87 8F A2 20 " +
            "DA F2 44 80 F9 A8 A3 0E",
            // After Pi
            "74 20 D6 81 35 16 F4 72 27 0E 84 AA 80 50 61 C9 " +
            "3F 42 BD 9C AC 91 4C 38 DC 73 CD EE A2 CE 1B EA " +
            "DA F2 44 80 F9 A8 A3 0E 48 50 7C 2A 17 8C A0 42 " +
            "4D C6 30 AD 7F 82 4C CD 83 5A C0 66 2D 00 07 00 " +
            "E5 4B A1 71 B5 2E CF E0 60 69 98 3E 01 EC 80 17 " +
            "16 67 90 FE 13 B8 EA 30 17 F9 0D BB AE E0 1C 8B " +
            "92 04 64 96 EE 01 4E 91 B2 AD 10 43 B3 29 DE E1 " +
            "5C 05 D9 25 FF 15 80 85 B0 C2 64 80 73 97 A8 BB " +
            "7B C9 C0 0E 7B 7D DF 9A 49 8C AE D5 69 56 4D A9 " +
            "B8 10 E5 27 4B 09 F7 2A F0 0D B5 58 87 8F A2 20 " +
            "52 AF 46 7E 97 01 2C B0 E9 6E 21 48 59 0A 62 DD " +
            "F3 F8 41 5F 26 83 30 41 24 8E 24 CF 08 6B 9C 6C " +
            "06 B9 37 F0 54 CB 03 54",
            // After Chi
            "6C 60 EF 95 19 97 F8 42 E7 3F C4 C8 82 1E 72 0B " +
            "3D C2 BD 9C F5 B1 EC 3C F8 73 5F EF A6 D8 4F 9A " +
            "D9 FC 44 AA 79 E8 A2 87 CA 48 BC 68 17 8C A3 42 " +
            "29 C7 11 BC EF AC 84 2D 83 7A D8 68 2D C0 07 17 " +
            "ED 5B C5 71 A3 2E EF A0 65 EF 98 BB 69 EE CC 9A " +
            "96 63 F0 FA 53 B9 A8 20 37 50 1D FA BF C8 8C EB " +
            "DE 04 AD B2 A2 15 4E 95 B0 CF 10 99 B3 81 B4 D1 " +
            "5D 9D D4 24 53 55 94 0E B0 C6 4A 51 73 95 A8 9A " +
            "CB D9 81 2C 79 74 6D 98 09 81 BE 8D ED D0 4D A9 " +
            "B8 D2 A5 A7 3B 19 FF B1 BB 04 35 56 8F E7 F5 20 " +
            "40 3F 06 69 B1 80 3C B0 ED 68 05 C8 51 62 EE F1 " +
            "F1 C9 52 6F 72 03 33 51 74 88 64 C1 8B 6B B0 CC " +
            "AF F9 16 F0 1C C1 41 19",
            // After Iota 
            "66 60 EF 15 19 97 F8 42 E7 3F C4 C8 82 1E 72 0B " +
            "3D C2 BD 9C F5 B1 EC 3C F8 73 5F EF A6 D8 4F 9A " +
            "D9 FC 44 AA 79 E8 A2 87 CA 48 BC 68 17 8C A3 42 " +
            "29 C7 11 BC EF AC 84 2D 83 7A D8 68 2D C0 07 17 " +
            "ED 5B C5 71 A3 2E EF A0 65 EF 98 BB 69 EE CC 9A " +
            "96 63 F0 FA 53 B9 A8 20 37 50 1D FA BF C8 8C EB " +
            "DE 04 AD B2 A2 15 4E 95 B0 CF 10 99 B3 81 B4 D1 " +
            "5D 9D D4 24 53 55 94 0E B0 C6 4A 51 73 95 A8 9A " +
            "CB D9 81 2C 79 74 6D 98 09 81 BE 8D ED D0 4D A9 " +
            "B8 D2 A5 A7 3B 19 FF B1 BB 04 35 56 8F E7 F5 20 " +
            "40 3F 06 69 B1 80 3C B0 ED 68 05 C8 51 62 EE F1 " +
            "F1 C9 52 6F 72 03 33 51 74 88 64 C1 8B 6B B0 CC " +
            "AF F9 16 F0 1C C1 41 19",
            // Round #12
            // After Theta
            "2C 20 5C 52 3D 3B 44 21 1D 64 62 3F D6 C6 A2 8C " +
            "31 A1 66 34 03 D7 A3 B4 8A 60 2D 6D E2 84 08 88 " +
            "24 24 D0 B4 40 82 36 05 80 08 0F 2F 33 20 1F 21 " +
            "D3 9C B7 4B BB 74 54 AA 8F 19 03 C0 DB A6 48 9F " +
            "9F 48 B7 F3 E7 72 A8 B2 98 37 0C A5 50 84 58 18 " +
            "DC 23 43 BD 77 15 14 43 CD 0B BB 0D EB 10 5C 6C " +
            "D2 67 76 1A 54 73 01 1D C2 DC 62 1B F7 DD F3 C3 " +
            "A0 45 40 3A 6A 3F 00 8C FA 86 F9 16 57 39 14 F9 " +
            "31 82 27 DB 2D AC BD 1F 05 E2 65 25 1B B6 02 21 " +
            "CA C1 D7 25 7F 45 B8 A3 46 DC A1 48 B6 8D 61 A2 " +
            "0A 7F B5 2E 95 2C 80 D3 17 33 A3 3F 05 BA 3E 76 " +
            "FD AA 89 C7 84 65 7C D9 06 9B 16 43 CF 37 F7 DE " +
            "52 21 82 EE 25 AB D5 9B",
            // After Rho
            "2C 20 5C 52 3D 3B 44 21 3B C8 C4 7E AC 8D 45 19 " +
            "4C A8 19 CD C0 F5 28 6D 4E 88 80 A8 08 D6 D2 26 " +
            "12 B4 29 20 21 81 A6 05 32 03 F2 11 02 88 F0 F0 " +
            "BB B4 4B 47 A5 3A CD 79 E7 63 C6 00 F0 B6 29 D2 " +
            "A4 DB F9 73 39 54 D9 4F 88 85 81 79 C3 50 0A 45 " +
            "E2 1E 19 EA BD AB A0 18 B1 35 2F EC 36 AC 43 70 " +
            "D3 A0 9A 0B E8 90 3E B3 BB E7 87 85 B9 C5 36 EE " +
            "1D B5 1F 00 46 D0 22 20 2D AE 72 28 F2 F5 0D F3 " +
            "64 BB 85 B5 F7 23 46 F0 81 90 02 F1 B2 92 0D 5B " +
            "08 77 54 39 F8 BA E4 AF A2 46 DC A1 48 B6 8D 61 " +
            "00 4E 2B FC D5 BA 54 B2 5D CC 8C FE 14 E8 FA D8 " +
            "5F 35 F1 98 B0 8C 2F BB 9B 16 43 CF 37 F7 DE 06 " +
            "F5 A6 54 88 A0 7B C9 6A",
            // After Pi
            "2C 20 5C 52 3D 3B 44 21 BB B4 4B 47 A5 3A CD 79 " +
            "D3 A0 9A 0B E8 90 3E B3 08 77 54 39 F8 BA E4 AF " +
            "F5 A6 54 88 A0 7B C9 6A 4E 88 80 A8 08 D6 D2 26 " +
            "88 85 81 79 C3 50 0A 45 E2 1E 19 EA BD AB A0 18 " +
            "64 BB 85 B5 F7 23 46 F0 5F 35 F1 98 B0 8C 2F BB " +
            "3B C8 C4 7E AC 8D 45 19 E7 63 C6 00 F0 B6 29 D2 " +
            "BB E7 87 85 B9 C5 36 EE A2 46 DC A1 48 B6 8D 61 " +
            "00 4E 2B FC D5 BA 54 B2 12 B4 29 20 21 81 A6 05 " +
            "32 03 F2 11 02 88 F0 F0 B1 35 2F EC 36 AC 43 70 " +
            "81 90 02 F1 B2 92 0D 5B 9B 16 43 CF 37 F7 DE 06 " +
            "4C A8 19 CD C0 F5 28 6D A4 DB F9 73 39 54 D9 4F " +
            "1D B5 1F 00 46 D0 22 20 2D AE 72 28 F2 F5 0D F3 " +
            "5D CC 8C FE 14 E8 FA D8",
            // After Chi
            "6C 20 CC 5A 75 BB 76 A3 B3 E3 0F 77 B5 10 0D 75 " +
            "26 20 9A 8B E8 D1 37 F3 00 77 5C 6B E5 BA E0 AE " +
            "66 32 57 8D 20 7B 40 32 2C 92 98 2A 34 7D 72 3E " +
            "8C 24 05 6C 81 50 4C A5 F9 1A 69 E2 BD 27 89 13 " +
            "64 33 85 95 FF 71 96 F4 DF 30 F0 C9 73 8C 27 FA " +
            "23 4C C5 FB A5 CC 53 35 E7 63 9E 20 B0 84 A0 D3 " +
            "BB EF A4 D9 2C CD 66 7C 99 C6 18 A3 60 B3 8C 68 " +
            "C4 6D 29 FC 85 88 7C 70 93 80 24 CC 15 A5 A5 05 " +
            "32 83 F2 00 82 9A FC FB AB 33 6E E2 33 C9 91 74 " +
            "81 30 2A D1 B2 92 2D 5A BB 15 91 DE 35 FF 8E F6 " +
            "55 8C 1F CD 86 75 0A 4D 84 D1 99 5B 89 71 D4 9C " +
            "4D F5 93 D6 42 D8 D0 28 2D 8E 63 29 32 E0 0D D6 " +
            "FD 9F 6C CC 2D E8 2B DA",
            // After Iota 
            "E7 A0 CC DA 75 BB 76 A3 B3 E3 0F 77 B5 10 0D 75 " +
            "26 20 9A 8B E8 D1 37 F3 00 77 5C 6B E5 BA E0 AE " +
            "66 32 57 8D 20 7B 40 32 2C 92 98 2A 34 7D 72 3E " +
            "8C 24 05 6C 81 50 4C A5 F9 1A 69 E2 BD 27 89 13 " +
            "64 33 85 95 FF 71 96 F4 DF 30 F0 C9 73 8C 27 FA " +
            "23 4C C5 FB A5 CC 53 35 E7 63 9E 20 B0 84 A0 D3 " +
            "BB EF A4 D9 2C CD 66 7C 99 C6 18 A3 60 B3 8C 68 " +
            "C4 6D 29 FC 85 88 7C 70 93 80 24 CC 15 A5 A5 05 " +
            "32 83 F2 00 82 9A FC FB AB 33 6E E2 33 C9 91 74 " +
            "81 30 2A D1 B2 92 2D 5A BB 15 91 DE 35 FF 8E F6 " +
            "55 8C 1F CD 86 75 0A 4D 84 D1 99 5B 89 71 D4 9C " +
            "4D F5 93 D6 42 D8 D0 28 2D 8E 63 29 32 E0 0D D6 " +
            "FD 9F 6C CC 2D E8 2B DA",
            // Round #13
            // After Theta
            "00 A9 40 B1 A5 8C 5A FE 98 B6 F1 74 D3 9E C7 14 " +
            "EB AE 75 A0 92 EB 4A EA F5 AE 11 BB 70 41 05 47 " +
            "6A EA 8B 3D 34 C5 6B 4D CB 9B 14 41 E4 4A 5E 63 " +
            "A7 71 FB 6F E7 DE 86 C4 34 94 86 C9 C7 1D F4 0A " +
            "91 EA C8 45 6A 8A 73 1D D3 E8 2C 79 67 32 0C 85 " +
            "C4 45 49 90 75 FB 7F 68 CC 36 60 23 D6 0A 6A B2 " +
            "76 61 4B F2 56 F7 1B 65 6C 1F 55 73 F5 48 69 81 " +
            "C8 B5 F5 4C 91 36 57 0F 74 89 A8 A7 C5 92 89 58 " +
            "19 D6 0C 03 E4 14 36 9A 66 BD 81 C9 49 F3 EC 6D " +
            "74 E9 67 01 27 69 C8 B3 B7 CD 4D 6E 21 41 A5 89 " +
            "B2 85 93 A6 56 42 26 10 AF 84 67 58 EF FF 1E FD " +
            "80 7B 7C FD 38 E2 AD 31 D8 57 2E F9 A7 1B E8 3F " +
            "F1 47 B0 7C 39 56 00 A5",
            // After Rho
            "00 A9 40 B1 A5 8C 5A FE 30 6D E3 E9 A6 3D 8F 29 " +
            "BA 6B 1D A8 E4 BA 92 FA 17 54 70 54 EF 1A B1 0B " +
            "29 5E 6B 52 53 5F EC A1 44 AE E4 35 B6 BC 49 11 " +
            "FF 76 EE 6D 48 7C 1A B7 02 0D A5 61 F2 71 07 BD " +
            "75 E4 22 35 C5 B9 8E 48 C3 50 38 8D CE 92 77 26 " +
            "23 2E 4A 82 AC DB FF 43 C9 32 DB 80 8D 58 2B A8 " +
            "92 B7 BA DF 28 B3 0B 5B 91 D2 02 D9 3E AA E6 EA " +
            "A6 48 9B AB 07 E4 DA 7A 4F 8B 25 13 B1 E8 12 51 " +
            "61 80 9C C2 46 33 C3 9A F6 36 B3 DE C0 E4 A4 79 " +
            "0D 79 96 2E FD 2C E0 24 89 B7 CD 4D 6E 21 41 A5 " +
            "99 40 C8 16 4E 9A 5A 09 BF 12 9E 61 BD FF 7B F4 " +
            "70 8F AF 1F 47 BC 35 06 57 2E F9 A7 1B E8 3F D8 " +
            "40 69 FC 11 2C 5F 8E 15",
            // After Pi
            "00 A9 40 B1 A5 8C 5A FE FF 76 EE 6D 48 7C 1A B7 " +
            "92 B7 BA DF 28 B3 0B 5B 0D 79 96 2E FD 2C E0 24 " +
            "40 69 FC 11 2C 5F 8E 15 17 54 70 54 EF 1A B1 0B " +
            "C3 50 38 8D CE 92 77 26 23 2E 4A 82 AC DB FF 43 " +
            "61 80 9C C2 46 33 C3 9A 70 8F AF 1F 47 BC 35 06 " +
            "30 6D E3 E9 A6 3D 8F 29 02 0D A5 61 F2 71 07 BD " +
            "91 D2 02 D9 3E AA E6 EA 89 B7 CD 4D 6E 21 41 A5 " +
            "99 40 C8 16 4E 9A 5A 09 29 5E 6B 52 53 5F EC A1 " +
            "44 AE E4 35 B6 BC 49 11 C9 32 DB 80 8D 58 2B A8 " +
            "F6 36 B3 DE C0 E4 A4 79 57 2E F9 A7 1B E8 3F D8 " +
            "BA 6B 1D A8 E4 BA 92 FA 75 E4 22 35 C5 B9 8E 48 " +
            "A6 48 9B AB 07 E4 DA 7A 4F 8B 25 13 B1 E8 12 51 " +
            "BF 12 9E 61 BD FF 7B F4",
            // After Chi
            "00 28 50 23 85 0F 5B B6 F2 3E EA 4D 9D 70 FA 93 " +
            "D2 B7 D2 CE 28 E0 05 4A 0D F9 96 8E 7C AC B0 CE " +
            "BF 3F 52 5D 64 2F 8E 14 37 7A 32 56 CF 53 39 4A " +
            "83 D0 AC CD 8C B2 77 BE 33 21 69 9F AD 57 CB 47 " +
            "66 D0 CC 82 EE 31 43 93 B0 8F A7 96 47 3C 73 22 " +
            "A1 BF E1 71 AA B7 6F 6B 0A 28 68 65 B2 70 06 B8 " +
            "81 92 02 CB 3E 30 FC E2 A9 9A EE A4 CE 04 C4 85 " +
            "9B 40 CC 16 1E DA 5A 9D A0 4E 70 D2 5A 1F CE 09 " +
            "72 AA C4 6B F6 18 CD 40 C8 3A 93 A1 96 50 30 28 " +
            "DE 66 B1 8E 80 F3 64 58 13 8E 7D 82 BF 48 3E C8 " +
            "38 63 84 22 E6 FE C2 C8 3C 67 06 25 75 B1 8E 49 " +
            "16 58 01 CB 0B F3 B3 DE 4F E2 24 9B F1 E8 92 5B " +
            "FA 96 BC 74 BC FE 77 F4",
            // After Iota 
            "8B 28 50 23 85 0F 5B 36 F2 3E EA 4D 9D 70 FA 93 " +
            "D2 B7 D2 CE 28 E0 05 4A 0D F9 96 8E 7C AC B0 CE " +
            "BF 3F 52 5D 64 2F 8E 14 37 7A 32 56 CF 53 39 4A " +
            "83 D0 AC CD 8C B2 77 BE 33 21 69 9F AD 57 CB 47 " +
            "66 D0 CC 82 EE 31 43 93 B0 8F A7 96 47 3C 73 22 " +
            "A1 BF E1 71 AA B7 6F 6B 0A 28 68 65 B2 70 06 B8 " +
            "81 92 02 CB 3E 30 FC E2 A9 9A EE A4 CE 04 C4 85 " +
            "9B 40 CC 16 1E DA 5A 9D A0 4E 70 D2 5A 1F CE 09 " +
            "72 AA C4 6B F6 18 CD 40 C8 3A 93 A1 96 50 30 28 " +
            "DE 66 B1 8E 80 F3 64 58 13 8E 7D 82 BF 48 3E C8 " +
            "38 63 84 22 E6 FE C2 C8 3C 67 06 25 75 B1 8E 49 " +
            "16 58 01 CB 0B F3 B3 DE 4F E2 24 9B F1 E8 92 5B " +
            "FA 96 BC 74 BC FE 77 F4",
            // Round #14
            // After Theta
            "9D D6 70 5F FA 46 25 98 0B 33 CB 59 8C 32 99 76 " +
            "40 D2 7C 1F 53 FF 4E 61 48 4F 4C 29 26 76 DD F8 " +
            "E7 89 9C 08 F0 B9 4D 63 21 84 12 2A B0 1A 47 E4 " +
            "7A DD 8D D9 9D F0 14 5B A1 44 C7 4E D6 48 80 6C " +
            "23 66 16 25 B4 EB 2E A5 E8 39 69 C3 D3 AA B0 55 " +
            "B7 41 C1 0D D5 FE 11 C5 F3 25 49 71 A3 32 65 5D " +
            "13 F7 AC 1A 45 2F B7 C9 EC 2C 34 03 94 DE A9 B3 " +
            "C3 F6 02 43 8A 4C 99 EA B6 B0 50 AE 25 56 B0 A7 " +
            "8B A7 E5 7F E7 5A AE A5 5A 5F 3D 70 ED 4F 7B 03 " +
            "9B D0 6B 29 DA 29 09 6E 4B 38 B3 D7 2B DE FD BF " +
            "2E 9D A4 5E 99 B7 BC 66 C5 6A 27 31 64 F3 ED AC " +
            "84 3D AF 1A 70 EC F8 F5 0A 54 FE 3C AB 32 FF 6D " +
            "A2 20 72 21 28 68 B4 83",
            // After Rho
            "9D D6 70 5F FA 46 25 98 16 66 96 B3 18 65 32 ED " +
            "90 34 DF C7 D4 BF 53 18 62 D7 8D 8F F4 C4 94 62 " +
            "CF 6D 1A 3B 4F E4 44 80 02 AB 71 44 1E 42 28 A1 " +
            "98 DD 09 4F B1 A5 D7 DD 5B 28 D1 B1 93 35 12 20 " +
            "33 8B 12 DA 75 97 D2 11 0A 5B 85 9E 93 36 3C AD " +
            "BE 0D 0A 6E A8 F6 8F 28 75 CD 97 24 C5 8D CA 94 " +
            "D5 28 7A B9 4D 9E B8 67 BD 53 67 D9 59 68 06 28 " +
            "21 45 A6 4C F5 61 7B 81 5C 4B AC 60 4F 6D 61 A1 " +
            "FC EF 5C CB B5 74 F1 B4 BD 01 AD AF 1E B8 F6 A7 " +
            "25 C1 6D 13 7A 2D 45 3B BF 4B 38 B3 D7 2B DE FD " +
            "F2 9A B9 74 92 7A 65 DE 16 AB 9D C4 90 CD B7 B3 " +
            "B0 E7 55 03 8E 1D BF 9E 54 FE 3C AB 32 FF 6D 0A " +
            "ED A0 28 88 5C 08 0A 1A",
            // After Pi
            "9D D6 70 5F FA 46 25 98 98 DD 09 4F B1 A5 D7 DD " +
            "D5 28 7A B9 4D 9E B8 67 25 C1 6D 13 7A 2D 45 3B " +
            "ED A0 28 88 5C 08 0A 1A 62 D7 8D 8F F4 C4 94 62 " +
            "0A 5B 85 9E 93 36 3C AD BE 0D 0A 6E A8 F6 8F 28 " +
            "FC EF 5C CB B5 74 F1 B4 B0 E7 55 03 8E 1D BF 9E " +
            "16 66 96 B3 18 65 32 ED 5B 28 D1 B1 93 35 12 20 " +
            "BD 53 67 D9 59 68 06 28 BF 4B 38 B3 D7 2B DE FD " +
            "F2 9A B9 74 92 7A 65 DE CF 6D 1A 3B 4F E4 44 80 " +
            "02 AB 71 44 1E 42 28 A1 75 CD 97 24 C5 8D CA 94 " +
            "BD 01 AD AF 1E B8 F6 A7 54 FE 3C AB 32 FF 6D 0A " +
            "90 34 DF C7 D4 BF 53 18 33 8B 12 DA 75 97 D2 11 " +
            "21 45 A6 4C F5 61 7B 81 5C 4B AC 60 4F 6D 61 A1 " +
            "16 AB 9D C4 90 CD B7 B3",
            // After Chi
            "D8 F6 02 EF B6 5C 0D BA B8 1C 0C 4D 83 84 92 C5 " +
            "1D 08 7A 31 49 9E B2 67 35 97 3D 44 D8 6B 60 BB " +
            "ED A9 21 88 5D A9 D8 5F D6 D3 87 EF DC 04 17 62 " +
            "4A B9 D1 1F 86 36 4C 39 BE 0D 0B 6E A2 FF 81 22 " +
            "BE FF D4 47 C5 B4 F1 D4 B8 EF 55 13 8D 2F 97 13 " +
            "B2 35 B0 FB 50 2D 36 E5 59 20 C9 93 15 36 CA F5 " +
            "FD C3 E6 9D 59 38 27 2A BB 2F 3E 30 DF 2E CC DC " +
            "BB 92 F8 74 11 6A 65 DE BA 29 9C 1B 8E 69 86 94 " +
            "8A AB 59 CF 04 72 1C 82 35 33 87 24 E5 CA C3 9C " +
            "36 00 AF BF 53 B8 F6 27 54 7C 5D EF 22 FD 45 2B " +
            "90 70 7B C3 54 DF 7A 98 6F 81 1A FA 7F 9B D2 31 " +
            "23 E5 B7 C8 65 E1 ED 93 DC 5F EE 63 0B 5F 21 A9 " +
            "35 20 9D DC B1 CD 37 B2",
            // After Iota 
            "51 76 02 EF B6 5C 0D 3A B8 1C 0C 4D 83 84 92 C5 " +
            "1D 08 7A 31 49 9E B2 67 35 97 3D 44 D8 6B 60 BB " +
            "ED A9 21 88 5D A9 D8 5F D6 D3 87 EF DC 04 17 62 " +
            "4A B9 D1 1F 86 36 4C 39 BE 0D 0B 6E A2 FF 81 22 " +
            "BE FF D4 47 C5 B4 F1 D4 B8 EF 55 13 8D 2F 97 13 " +
            "B2 35 B0 FB 50 2D 36 E5 59 20 C9 93 15 36 CA F5 " +
            "FD C3 E6 9D 59 38 27 2A BB 2F 3E 30 DF 2E CC DC " +
            "BB 92 F8 74 11 6A 65 DE BA 29 9C 1B 8E 69 86 94 " +
            "8A AB 59 CF 04 72 1C 82 35 33 87 24 E5 CA C3 9C " +
            "36 00 AF BF 53 B8 F6 27 54 7C 5D EF 22 FD 45 2B " +
            "90 70 7B C3 54 DF 7A 98 6F 81 1A FA 7F 9B D2 31 " +
            "23 E5 B7 C8 65 E1 ED 93 DC 5F EE 63 0B 5F 21 A9 " +
            "35 20 9D DC B1 CD 37 B2",
            // Round #15
            // After Theta
            "43 A0 E1 DB 33 5A E1 44 37 F5 90 33 07 A3 36 B4 " +
            "E7 96 01 1A 17 DE 7C A6 63 96 03 D2 4F A1 EB CD " +
            "08 23 12 20 07 38 F3 01 C4 05 64 DB 59 02 FB 1C " +
            "C5 50 4D 61 02 11 E8 48 44 93 70 45 FC BF 4F E3 " +
            "E8 FE EA D1 52 7E 7A A2 5D 65 66 BB D7 BE BC 4D " +
            "A0 E3 53 CF D5 2B DA 9B D6 C9 55 ED 91 11 6E 84 " +
            "07 5D 9D B6 07 78 E9 EB ED 2E 00 A6 48 E4 47 AA " +
            "5E 18 CB DC 4B FB 4E 80 A8 FF 7F 2F 0B 6F 6A EA " +
            "05 42 C5 B1 80 55 B8 F3 CF AD FC 0F BB 8A 0D 5D " +
            "60 01 91 29 C4 72 7D 51 B1 F6 6E 47 78 6C 6E 75 " +
            "82 A6 98 F7 D1 D9 96 E6 E0 68 86 84 FB BC 76 40 " +
            "D9 7B CC E3 3B A1 23 52 8A 5E D0 F5 9C 95 AA DF " +
            "D0 AA AE 74 EB 5C 1C EC",
            // After Rho
            "43 A0 E1 DB 33 5A E1 44 6F EA 21 67 0E 46 6D 68 " +
            "B9 65 80 C6 85 37 9F E9 14 BA DE 3C 66 39 20 FD " +
            "C0 99 0F 40 18 91 00 39 9D 25 B0 CF 41 5C 40 B6 " +
            "14 26 10 81 8E 54 0C D5 38 D1 24 5C 11 FF EF D3 " +
            "7F F5 68 29 3F 3D 51 74 CB DB D4 55 66 B6 7B ED " +
            "04 1D 9F 7A AE 5E D1 DE 11 5A 27 57 B5 47 46 B8 " +
            "B4 3D C0 4B 5F 3F E8 EA C8 8F 54 DB 5D 00 4C 91 " +
            "EE A5 7D 27 40 2F 8C 65 5E 16 DE D4 D4 51 FF FF " +
            "38 16 B0 0A 77 BE 40 A8 86 AE E7 56 FE 87 5D C5 " +
            "AE 2F 0A 2C 20 32 85 58 75 B1 F6 6E 47 78 6C 6E " +
            "5B 9A 0B 9A 62 DE 47 67 81 A3 19 12 EE F3 DA 01 " +
            "7B 8F 79 7C 27 74 44 2A 5E D0 F5 9C 95 AA DF 8A " +
            "07 3B B4 AA 2B DD 3A 17",
            // After Pi
            "43 A0 E1 DB 33 5A E1 44 14 26 10 81 8E 54 0C D5 " +
            "B4 3D C0 4B 5F 3F E8 EA AE 2F 0A 2C 20 32 85 58 " +
            "07 3B B4 AA 2B DD 3A 17 14 BA DE 3C 66 39 20 FD " +
            "CB DB D4 55 66 B6 7B ED 04 1D 9F 7A AE 5E D1 DE " +
            "38 16 B0 0A 77 BE 40 A8 7B 8F 79 7C 27 74 44 2A " +
            "6F EA 21 67 0E 46 6D 68 38 D1 24 5C 11 FF EF D3 " +
            "C8 8F 54 DB 5D 00 4C 91 75 B1 F6 6E 47 78 6C 6E " +
            "5B 9A 0B 9A 62 DE 47 67 C0 99 0F 40 18 91 00 39 " +
            "9D 25 B0 CF 41 5C 40 B6 11 5A 27 57 B5 47 46 B8 " +
            "86 AE E7 56 FE 87 5D C5 5E D0 F5 9C 95 AA DF 8A " +
            "B9 65 80 C6 85 37 9F E9 7F F5 68 29 3F 3D 51 74 " +
            "EE A5 7D 27 40 2F 8C 65 5E 16 DE D4 D4 51 FF FF " +
            "81 A3 19 12 EE F3 DA 01",
            // After Chi
            "E3 B9 21 91 62 71 01 6E 1E 24 1A A5 AE 54 09 C5 " +
            "B5 2D 74 C9 54 F2 D2 ED EE AF 4B 7D 30 30 44 18 " +
            "13 3D A4 AA A7 D9 36 86 10 BE D5 16 EE 71 A0 EF " +
            "F3 D9 F4 55 37 16 7B CD 47 94 D6 0E AE 1E D5 DC " +
            "3C 26 36 0A 37 B7 60 7D B0 CE 79 3D 27 F2 1F 2A " +
            "AF E4 71 E4 42 46 6D 68 0D E1 86 78 13 87 CF BD " +
            "C2 85 5D 4B 7D 86 4F 90 51 D1 D6 0B 4B 78 44 66 " +
            "4B 8B 0F 82 73 67 C5 F4 C0 C3 08 50 AC 92 06 31 " +
            "1B 81 70 CF 0B DC 59 F3 49 0A 37 DF B4 6F C4 B2 " +
            "06 A7 ED 16 F6 96 5D F4 43 F4 45 13 D4 E6 9F 0C " +
            "39 65 95 C0 C5 35 13 E8 6F E7 EA F9 AB 6D 22 EE " +
            "6F 04 7C 25 6A 8D 8C 65 66 52 5E 10 D5 55 FA 17 " +
            "C7 33 71 3B D4 FB 9A 15",
            // After Iota 
            "E0 39 21 91 62 71 01 EE 1E 24 1A A5 AE 54 09 C5 " +
            "B5 2D 74 C9 54 F2 D2 ED EE AF 4B 7D 30 30 44 18 " +
            "13 3D A4 AA A7 D9 36 86 10 BE D5 16 EE 71 A0 EF " +
            "F3 D9 F4 55 37 16 7B CD 47 94 D6 0E AE 1E D5 DC " +
            "3C 26 36 0A 37 B7 60 7D B0 CE 79 3D 27 F2 1F 2A " +
            "AF E4 71 E4 42 46 6D 68 0D E1 86 78 13 87 CF BD " +
            "C2 85 5D 4B 7D 86 4F 90 51 D1 D6 0B 4B 78 44 66 " +
            "4B 8B 0F 82 73 67 C5 F4 C0 C3 08 50 AC 92 06 31 " +
            "1B 81 70 CF 0B DC 59 F3 49 0A 37 DF B4 6F C4 B2 " +
            "06 A7 ED 16 F6 96 5D F4 43 F4 45 13 D4 E6 9F 0C " +
            "39 65 95 C0 C5 35 13 E8 6F E7 EA F9 AB 6D 22 EE " +
            "6F 04 7C 25 6A 8D 8C 65 66 52 5E 10 D5 55 FA 17 " +
            "C7 33 71 3B D4 FB 9A 15",
            // Round #16
            // After Theta
            "A5 73 23 D1 C4 C8 64 FE 94 85 6A BB BB A5 D1 99 " +
            "E6 0C B7 83 A0 FE 9A 84 20 E3 32 70 8F 1B 96 ED " +
            "BD 1B 8D 36 87 26 42 07 55 F4 D7 56 48 C8 C5 FF " +
            "79 78 84 4B 22 E7 A3 91 14 B5 15 44 5A 12 9D B5 " +
            "F2 6A 4F 07 88 9C B2 88 1E E8 50 A1 07 0D 6B AB " +
            "EA AE 73 A4 E4 FF 08 78 87 40 F6 66 06 76 17 E1 " +
            "91 A4 9E 01 89 8A 07 F9 9F 9D AF 06 F4 53 96 93 " +
            "E5 AD 26 1E 53 98 B1 75 85 89 0A 10 0A 2B 63 21 " +
            "91 20 00 D1 1E 2D 81 AF 1A 2B F4 95 40 63 8C DB " +
            "C8 EB 94 1B 49 BD 8F 01 ED D2 6C 8F F4 19 EB 8D " +
            "7C 2F 97 80 63 8C 76 F8 E5 46 9A E7 BE 9C FA B2 " +
            "3C 25 BF 6F 9E 81 C4 0C A8 1E 27 1D 6A 7E 28 E2 " +
            "69 15 58 A7 F4 04 EE 94",
            // After Rho
            "A5 73 23 D1 C4 C8 64 FE 29 0B D5 76 77 4B A3 33 " +
            "39 C3 ED 20 A8 BF 26 A1 B8 61 D9 0E 32 2E 03 F7 " +
            "34 11 3A E8 DD 68 B4 39 85 84 5C FC 5F 45 7F 6D " +
            "B8 24 72 3E 1A 99 87 47 2D 45 6D 05 91 96 44 67 " +
            "B5 A7 03 44 4E 59 44 79 B0 B6 EA 81 0E 15 7A D0 " +
            "53 77 9D 23 25 FF 47 C0 84 1F 02 D9 9B 19 D8 5D " +
            "0C 48 54 3C C8 8F 24 F5 A7 2C 27 3F 3B 5F 0D E8 " +
            "8F 29 CC D8 BA F2 56 13 20 14 56 C6 42 0A 13 15 " +
            "20 DA A3 25 F0 35 12 04 C6 6D 8D 15 FA 4A A0 31 " +
            "F7 31 00 79 9D 72 23 A9 8D ED D2 6C 8F F4 19 EB " +
            "DA E1 F3 BD 5C 02 8E 31 96 1B 69 9E FB 72 EA CB " +
            "A7 E4 F7 CD 33 90 98 81 1E 27 1D 6A 7E 28 E2 A8 " +
            "3B 65 5A 05 D6 29 3D 81",
            // After Pi
            "A5 73 23 D1 C4 C8 64 FE B8 24 72 3E 1A 99 87 47 " +
            "0C 48 54 3C C8 8F 24 F5 F7 31 00 79 9D 72 23 A9 " +
            "3B 65 5A 05 D6 29 3D 81 B8 61 D9 0E 32 2E 03 F7 " +
            "B0 B6 EA 81 0E 15 7A D0 53 77 9D 23 25 FF 47 C0 " +
            "20 DA A3 25 F0 35 12 04 A7 E4 F7 CD 33 90 98 81 " +
            "29 0B D5 76 77 4B A3 33 2D 45 6D 05 91 96 44 67 " +
            "A7 2C 27 3F 3B 5F 0D E8 8D ED D2 6C 8F F4 19 EB " +
            "DA E1 F3 BD 5C 02 8E 31 34 11 3A E8 DD 68 B4 39 " +
            "85 84 5C FC 5F 45 7F 6D 84 1F 02 D9 9B 19 D8 5D " +
            "C6 6D 8D 15 FA 4A A0 31 1E 27 1D 6A 7E 28 E2 A8 " +
            "39 C3 ED 20 A8 BF 26 A1 B5 A7 03 44 4E 59 44 79 " +
            "8F 29 CC D8 BA F2 56 13 20 14 56 C6 42 0A 13 15 " +
            "96 1B 69 9E FB 72 EA CB",
            // After Chi
            "A1 3B 27 D1 04 CE 44 4E 4B 15 72 7F 0F E9 84 4F " +
            "04 0C 0E 38 8A 86 38 F5 73 23 21 A9 9D B2 63 D7 " +
            "23 61 0A 2B CC 38 BE 80 FB 20 CC 2C 13 C4 06 F7 " +
            "90 3E C8 85 DE 15 6A D4 D4 53 C9 EB 26 7F CF 41 " +
            "38 DB AB 27 F0 1B 11 72 A7 72 D5 4C 3F 81 E0 81 " +
            "AB 23 D7 4C 5D 02 AA BB 25 84 BD 45 15 36 54 64 " +
            "F5 2C 06 AE 6B 5D 8B F8 AC E7 D6 2E AC BD 38 E9 " +
            "DE A5 DB BC DC 96 CA 75 34 0A 38 E9 5D 70 34 29 " +
            "C7 E4 D1 F8 3F 07 5F 4D 9C 1D 12 B3 9F 39 9A D5 " +
            "E6 7D AF 95 7B 0A B4 20 9F A3 59 7E 7C 2D A9 EC " +
            "33 CB 21 B8 18 1D 34 A3 95 B3 11 42 0E 51 45 7D " +
            "19 22 E5 C0 03 82 BE D9 09 D4 D2 E6 42 87 17 35 " +
            "12 3F 6B DA BD 32 AA 93",
            // After Iota 
            "A3 BB 27 D1 04 CE 44 CE 4B 15 72 7F 0F E9 84 4F " +
            "04 0C 0E 38 8A 86 38 F5 73 23 21 A9 9D B2 63 D7 " +
            "23 61 0A 2B CC 38 BE 80 FB 20 CC 2C 13 C4 06 F7 " +
            "90 3E C8 85 DE 15 6A D4 D4 53 C9 EB 26 7F CF 41 " +
            "38 DB AB 27 F0 1B 11 72 A7 72 D5 4C 3F 81 E0 81 " +
            "AB 23 D7 4C 5D 02 AA BB 25 84 BD 45 15 36 54 64 " +
            "F5 2C 06 AE 6B 5D 8B F8 AC E7 D6 2E AC BD 38 E9 " +
            "DE A5 DB BC DC 96 CA 75 34 0A 38 E9 5D 70 34 29 " +
            "C7 E4 D1 F8 3F 07 5F 4D 9C 1D 12 B3 9F 39 9A D5 " +
            "E6 7D AF 95 7B 0A B4 20 9F A3 59 7E 7C 2D A9 EC " +
            "33 CB 21 B8 18 1D 34 A3 95 B3 11 42 0E 51 45 7D " +
            "19 22 E5 C0 03 82 BE D9 09 D4 D2 E6 42 87 17 35 " +
            "12 3F 6B DA BD 32 AA 93",
            // Round #17
            // After Theta
            "2D 60 9E A5 00 C7 92 5A FF F5 3B 83 B6 B2 DC C7 " +
            "B8 98 8A 9B 8E 29 4B 89 7D 3A 7B 59 1A CC 15 80 " +
            "C3 24 61 38 2B 6B 87 C8 75 FB 75 58 17 CD D0 63 " +
            "24 DE 81 79 67 4E 32 5C 68 C7 4D 48 22 D0 BC 3D " +
            "36 C2 F1 D7 77 65 67 25 47 37 BE 5F D8 D2 D9 C9 " +
            "25 F8 6E 38 59 0B 7C 2F 91 64 F4 B9 AC 6D 0C EC " +
            "49 B8 82 0D 6F F2 F8 84 A2 FE 8C DE 2B C3 4E BE " +
            "3E E0 B0 AF 3B C5 F3 3D BA D1 81 9D 59 79 E2 BD " +
            "73 04 98 04 86 5C 07 C5 20 89 96 10 9B 96 E9 A9 " +
            "E8 64 F5 65 FC 74 C2 77 7F E6 32 6D 9B 7E 90 A4 " +
            "BD 10 98 CC 1C 14 E2 37 21 53 58 BE B7 0A 1D F5 " +
            "A5 B6 61 63 07 2D CD A5 07 CD 88 16 C5 F9 61 62 " +
            "F2 7A 00 C9 5A 61 93 DB",
            // After Rho
            "2D 60 9E A5 00 C7 92 5A FF EB 77 06 6D 65 B9 8F " +
            "2E A6 E2 A6 63 CA 52 22 C1 5C 01 D8 A7 B3 97 A5 " +
            "59 3B 44 1E 26 09 C3 59 75 D1 0C 3D 56 B7 5F 87 " +
            "98 77 E6 24 C3 45 E2 1D 0F DA 71 13 92 08 34 6F " +
            "E1 F8 EB BB B2 B3 12 1B 9D 9D 7C 74 E3 FB 85 2D " +
            "29 C1 77 C3 C9 5A E0 7B B0 47 92 D1 E7 B2 B6 31 " +
            "6C 78 93 C7 27 4C C2 15 86 9D 7C 45 FD 19 BD 57 " +
            "D7 9D E2 F9 1E 1F 70 D8 3B B3 F2 C4 7B 75 A3 03 " +
            "93 C0 90 EB A0 78 8E 00 F4 54 90 44 4B 88 4D CB " +
            "4E F8 0E 9D AC BE 8C 9F A4 7F E6 32 6D 9B 7E 90 " +
            "88 DF F4 42 60 32 73 50 87 4C 61 F9 DE 2A 74 D4 " +
            "D4 36 6C EC A0 A5 B9 B4 CD 88 16 C5 F9 61 62 07 " +
            "E4 B6 BC 1E 40 B2 56 D8",
            // After Pi
            "2D 60 9E A5 00 C7 92 5A 98 77 E6 24 C3 45 E2 1D " +
            "6C 78 93 C7 27 4C C2 15 4E F8 0E 9D AC BE 8C 9F " +
            "E4 B6 BC 1E 40 B2 56 D8 C1 5C 01 D8 A7 B3 97 A5 " +
            "9D 9D 7C 74 E3 FB 85 2D 29 C1 77 C3 C9 5A E0 7B " +
            "93 C0 90 EB A0 78 8E 00 D4 36 6C EC A0 A5 B9 B4 " +
            "FF EB 77 06 6D 65 B9 8F 0F DA 71 13 92 08 34 6F " +
            "86 9D 7C 45 FD 19 BD 57 A4 7F E6 32 6D 9B 7E 90 " +
            "88 DF F4 42 60 32 73 50 59 3B 44 1E 26 09 C3 59 " +
            "75 D1 0C 3D 56 B7 5F 87 B0 47 92 D1 E7 B2 B6 31 " +
            "F4 54 90 44 4B 88 4D CB CD 88 16 C5 F9 61 62 07 " +
            "2E A6 E2 A6 63 CA 52 22 E1 F8 EB BB B2 B3 12 1B " +
            "D7 9D E2 F9 1E 1F 70 D8 3B B3 F2 C4 7B 75 A3 03 " +
            "87 4C 61 F9 DE 2A 74 D4",
            // After Chi
            "49 68 8F 66 24 CF 92 5A 9A F7 EA 3C 4B F7 EE 97 " +
            "CC 7E 23 C5 67 4C 90 55 47 B8 0C 3C AC FB 0C 9D " +
            "74 A1 DC 1E 83 B2 36 DD E1 1C 02 5B AF B3 F7 F7 " +
            "0F 9D FC 5C C3 DB 8B 2D 6D F7 1B C7 C9 DF D1 CF " +
            "92 88 91 FB A7 6A 88 01 C8 B7 10 C8 E0 ED B9 BC " +
            "7F EE 7B 42 00 74 30 9F 2F B8 F3 21 92 8A 76 EF " +
            "8E 1D 6C 05 FD 39 BC 17 D3 5F E5 36 60 DE F6 1F " +
            "88 CF F4 53 F2 3A 77 30 D9 3D D6 DE 87 09 63 69 " +
            "31 C1 0C 39 5E BF 16 4D B9 CF 94 50 57 D3 94 35 " +
            "E4 67 D0 5E 4D 80 CC 93 E9 48 1E E4 A9 D7 7E 81 " +
            "38 A3 E2 E6 6F C6 32 E2 C9 DA FB BF D3 D3 91 18 " +
            "53 D1 E3 C0 9A 15 24 0C 13 11 70 C2 5A B5 A1 21 " +
            "46 14 68 E0 4E 1B 74 CD",
            // After Iota 
            "C9 68 8F 66 24 CF 92 DA 9A F7 EA 3C 4B F7 EE 97 " +
            "CC 7E 23 C5 67 4C 90 55 47 B8 0C 3C AC FB 0C 9D " +
            "74 A1 DC 1E 83 B2 36 DD E1 1C 02 5B AF B3 F7 F7 " +
            "0F 9D FC 5C C3 DB 8B 2D 6D F7 1B C7 C9 DF D1 CF " +
            "92 88 91 FB A7 6A 88 01 C8 B7 10 C8 E0 ED B9 BC " +
            "7F EE 7B 42 00 74 30 9F 2F B8 F3 21 92 8A 76 EF " +
            "8E 1D 6C 05 FD 39 BC 17 D3 5F E5 36 60 DE F6 1F " +
            "88 CF F4 53 F2 3A 77 30 D9 3D D6 DE 87 09 63 69 " +
            "31 C1 0C 39 5E BF 16 4D B9 CF 94 50 57 D3 94 35 " +
            "E4 67 D0 5E 4D 80 CC 93 E9 48 1E E4 A9 D7 7E 81 " +
            "38 A3 E2 E6 6F C6 32 E2 C9 DA FB BF D3 D3 91 18 " +
            "53 D1 E3 C0 9A 15 24 0C 13 11 70 C2 5A B5 A1 21 " +
            "46 14 68 E0 4E 1B 74 CD",
            // Round #18
            // After Theta
            "D6 7F E4 69 7D F3 49 C6 A7 E6 6F 55 15 E9 70 C6 " +
            "6C 84 81 D9 08 72 3A 37 B4 39 B2 A9 DF C5 A4 12 " +
            "E9 B1 80 FC 39 46 20 9E FE 0B 69 54 F6 8F 2C EB " +
            "32 8C 79 35 9D C5 15 7C CD 0D B9 DB A6 E1 7B AD " +
            "61 09 2F 6E D4 54 20 8E 55 A7 4C 2A 5A 19 AF FF " +
            "60 F9 10 4D 59 48 EB 83 12 A9 76 48 CC 94 E8 BE " +
            "2E E7 CE 19 92 07 16 75 20 DE 5B A3 13 E0 5E 90 " +
            "15 DF A8 B1 48 CE 61 73 C6 2A BD D1 DE 35 B8 75 " +
            "0C D0 89 50 00 A1 88 1C 19 35 36 4C 38 ED 3E 57 " +
            "17 E6 6E CB 3E BE 64 1C 74 58 42 06 13 23 68 C2 " +
            "27 B4 89 E9 36 FA E9 FE F4 CB 7E D6 8D CD 0F 49 " +
            "F3 2B 41 DC F5 2B 8E 6E E0 90 CE 57 29 8B 09 AE " +
            "DB 04 34 02 F4 EF 62 8E",
            // After Rho
            "D6 7F E4 69 7D F3 49 C6 4F CD DF AA 2A D2 E1 8C " +
            "1B 61 60 36 82 9C CE 0D 5D 4C 2A 41 9B 23 9B FA " +
            "31 02 F1 4C 8F 05 E4 CF 65 FF C8 B2 EE BF 90 46 " +
            "57 D3 59 5C C1 27 C3 98 6B 73 43 EE B6 69 F8 5E " +
            "84 17 37 6A 2A 10 C7 B0 F1 FA 5F 75 CA A4 A2 95 " +
            "04 CB 87 68 CA 42 5A 1F FB 4A A4 DA 21 31 53 A2 " +
            "CE 90 3C B0 A8 73 39 77 C0 BD 20 41 BC B7 46 27 " +
            "58 24 E7 B0 B9 8A 6F D4 A3 BD 6B 70 EB 8C 55 7A " +
            "11 0A 20 14 91 83 01 3A 9F AB 8C 1A 1B 26 9C 76 " +
            "97 8C E3 C2 DC 6D D9 C7 C2 74 58 42 06 13 23 68 " +
            "A7 FB 9F D0 26 A6 DB E8 D1 2F FB 59 37 36 3F 24 " +
            "7E 25 88 BB 7E C5 D1 6D 90 CE 57 29 8B 09 AE E0 " +
            "98 E3 36 01 8D 00 FD BB",
            // After Pi
            "D6 7F E4 69 7D F3 49 C6 57 D3 59 5C C1 27 C3 98 " +
            "CE 90 3C B0 A8 73 39 77 97 8C E3 C2 DC 6D D9 C7 " +
            "98 E3 36 01 8D 00 FD BB 5D 4C 2A 41 9B 23 9B FA " +
            "F1 FA 5F 75 CA A4 A2 95 04 CB 87 68 CA 42 5A 1F " +
            "11 0A 20 14 91 83 01 3A 7E 25 88 BB 7E C5 D1 6D " +
            "4F CD DF AA 2A D2 E1 8C 6B 73 43 EE B6 69 F8 5E " +
            "C0 BD 20 41 BC B7 46 27 C2 74 58 42 06 13 23 68 " +
            "A7 FB 9F D0 26 A6 DB E8 31 02 F1 4C 8F 05 E4 CF " +
            "65 FF C8 B2 EE BF 90 46 FB 4A A4 DA 21 31 53 A2 " +
            "9F AB 8C 1A 1B 26 9C 76 90 CE 57 29 8B 09 AE E0 " +
            "1B 61 60 36 82 9C CE 0D 84 17 37 6A 2A 10 C7 B0 " +
            "58 24 E7 B0 B9 8A 6F D4 A3 BD 6B 70 EB 8C 55 7A " +
            "D1 2F FB 59 37 36 3F 24",
            // After Chi
            "5E 7F C0 C9 55 A3 71 A1 46 DF 9A 1E 95 2B 03 18 " +
            "C6 F3 28 B1 A9 73 1D 4F D1 90 23 AA AC 9E D9 83 " +
            "99 63 2F 15 0D 04 7F A3 59 4D AA 49 9B 61 C3 F0 " +
            "E0 FA 7F 61 DB 25 A3 B5 6A EE 0F C3 A4 06 8A 5A " +
            "10 42 02 54 10 A1 0B A8 DE 97 DD 8F 3E 41 F1 68 " +
            "CF 41 FF AB 22 44 E7 AD 69 33 1B EC B4 69 D9 16 " +
            "E5 36 A7 D1 9C 13 9E A7 8A 70 18 68 0E 43 03 6C " +
            "87 C9 9F 94 B2 8F C3 BA AB 02 D5 04 8E 05 A7 6F " +
            "61 5E C0 B2 F4 B9 1C 12 FB 0E F7 FB A1 38 71 22 " +
            "BE AB 2C 5E 1F 22 DC 79 D4 33 5F 9B EB B3 BE E0 " +
            "43 41 A0 A6 13 16 E6 49 27 8E 3F 2A 68 14 D7 9A " +
            "08 26 77 B9 AD B8 45 D0 A9 FD 6B 56 6B 04 95 73 " +
            "55 39 EC 11 1F 36 3E 94",
            // After Iota 
            "54 FF C0 C9 55 A3 71 A1 46 DF 9A 1E 95 2B 03 18 " +
            "C6 F3 28 B1 A9 73 1D 4F D1 90 23 AA AC 9E D9 83 " +
            "99 63 2F 15 0D 04 7F A3 59 4D AA 49 9B 61 C3 F0 " +
            "E0 FA 7F 61 DB 25 A3 B5 6A EE 0F C3 A4 06 8A 5A " +
            "10 42 02 54 10 A1 0B A8 DE 97 DD 8F 3E 41 F1 68 " +
            "CF 41 FF AB 22 44 E7 AD 69 33 1B EC B4 69 D9 16 " +
            "E5 36 A7 D1 9C 13 9E A7 8A 70 18 68 0E 43 03 6C " +
            "87 C9 9F 94 B2 8F C3 BA AB 02 D5 04 8E 05 A7 6F " +
            "61 5E C0 B2 F4 B9 1C 12 FB 0E F7 FB A1 38 71 22 " +
            "BE AB 2C 5E 1F 22 DC 79 D4 33 5F 9B EB B3 BE E0 " +
            "43 41 A0 A6 13 16 E6 49 27 8E 3F 2A 68 14 D7 9A " +
            "08 26 77 B9 AD B8 45 D0 A9 FD 6B 56 6B 04 95 73 " +
            "55 39 EC 11 1F 36 3E 94",
            // Round #19
            // After Theta
            "07 45 1D 5B EC 78 D9 C3 18 68 7A 55 DF 73 6C 42 " +
            "F7 DD D4 86 42 0C 9F E7 E9 FD 9F 42 DA E6 7E C8 " +
            "90 F7 90 98 28 74 CE 5A 0A F7 77 DB 22 BA 6B 92 " +
            "BE 4D 9F 2A 91 7D CC EF 5B C0 F3 F4 4F 79 08 F2 " +
            "28 2F BE BC 66 D9 AC E3 D7 03 62 02 1B 31 40 91 " +
            "9C FB 22 39 9B 9F 4F CF 37 84 FB A7 FE 31 B6 4C " +
            "D4 18 5B E6 77 6C 1C 0F B2 1D A4 80 78 3B A4 27 " +
            "8E 5D 20 19 97 FF 72 43 F8 B8 08 96 37 DE 0F 0D " +
            "3F E9 20 F9 BE E1 73 48 CA 20 0B CC 4A 47 F3 8A " +
            "86 C6 90 B6 69 5A 7B 32 DD A7 E0 16 CE C3 0F 19 " +
            "10 FB 7D 34 AA CD 4E 2B 79 39 DF 61 22 4C B8 C0 " +
            "39 08 8B 8E 46 C7 C7 78 91 90 D7 BE 1D 7C 32 38 " +
            "5C AD 53 9C 3A 46 8F 6D",
            // After Rho
            "07 45 1D 5B EC 78 D9 C3 30 D0 F4 AA BE E7 D8 84 " +
            "7D 37 B5 A1 10 C3 E7 F9 6D EE 87 9C DE FF 29 A4 " +
            "A1 73 D6 82 BC 87 C4 44 2D A2 BB 26 A9 70 7F B7 " +
            "A9 12 D9 C7 FC EE DB F4 FC 16 F0 3C FD 53 1E 82 " +
            "17 5F 5E B3 6C D6 71 94 03 14 79 3D 20 26 B0 11 " +
            "E6 DC 17 C9 D9 FC 7C 7A 32 DD 10 EE 9F FA C7 D8 " +
            "32 BF 63 E3 78 A0 C6 D8 76 48 4F 64 3B 48 01 F1 " +
            "8C CB 7F B9 21 C7 2E 90 2C 6F BC 1F 1A F0 71 11 " +
            "24 DF 37 7C 0E E9 27 1D 79 45 65 90 05 66 A5 A3 " +
            "6B 4F C6 D0 18 D2 36 4D 19 DD A7 E0 16 CE C3 0F " +
            "3B AD 40 EC F7 D1 A8 36 E7 E5 7C 87 89 30 E1 02 " +
            "07 61 D1 D1 E8 F8 18 2F 90 D7 BE 1D 7C 32 38 91 " +
            "63 1B 57 EB 14 A7 8E D1",
            // After Pi
            "07 45 1D 5B EC 78 D9 C3 A9 12 D9 C7 FC EE DB F4 " +
            "32 BF 63 E3 78 A0 C6 D8 6B 4F C6 D0 18 D2 36 4D " +
            "63 1B 57 EB 14 A7 8E D1 6D EE 87 9C DE FF 29 A4 " +
            "03 14 79 3D 20 26 B0 11 E6 DC 17 C9 D9 FC 7C 7A " +
            "24 DF 37 7C 0E E9 27 1D 07 61 D1 D1 E8 F8 18 2F " +
            "30 D0 F4 AA BE E7 D8 84 FC 16 F0 3C FD 53 1E 82 " +
            "76 48 4F 64 3B 48 01 F1 19 DD A7 E0 16 CE C3 0F " +
            "3B AD 40 EC F7 D1 A8 36 A1 73 D6 82 BC 87 C4 44 " +
            "2D A2 BB 26 A9 70 7F B7 32 DD 10 EE 9F FA C7 D8 " +
            "79 45 65 90 05 66 A5 A3 90 D7 BE 1D 7C 32 38 91 " +
            "7D 37 B5 A1 10 C3 E7 F9 17 5F 5E B3 6C D6 71 94 " +
            "8C CB 7F B9 21 C7 2E 90 2C 6F BC 1F 1A F0 71 11 " +
            "E7 E5 7C 87 89 30 E1 02",
            // After Chi
            "15 E8 3F 7B EC 78 DD CB E0 52 5D D7 FC BC EB F1 " +
            "32 AF 72 C8 7C 85 4E 48 6F 0B CE C0 F0 8A 67 4F " +
            "CB 09 97 6F 04 21 8C E5 89 26 81 5C 07 27 65 CE " +
            "03 17 59 09 26 27 B3 14 E5 FC D7 48 39 EC 64 58 " +
            "4C 51 31 70 18 EE 06 9D 05 71 A9 F0 C8 F8 88 3E " +
            "32 98 FB EA BC EF D9 F5 F5 83 50 BC F9 D5 DC 8C " +
            "54 68 0F 68 DA 59 29 C1 19 8D 13 E2 1E E8 93 8F " +
            "F7 AB 40 F8 B6 C1 AE 34 B3 2E D6 4A AA 0D 44 0C " +
            "64 A2 DE 36 A9 74 5F 94 B2 4F 8A E3 E7 EA DF C8 " +
            "58 65 25 12 85 E3 61 E7 9C 57 97 39 7D 42 03 22 " +
            "F5 B7 94 A9 11 C2 E9 F9 37 7B DE B5 76 E6 20 95 " +
            "4F 4B 3F 39 A0 C7 AE 92 34 7D 3D 3F 0A 33 77 E8 " +
            "E5 AD 36 95 E5 24 F1 06",
            // After Iota 
            "1F E8 3F FB EC 78 DD 4B E0 52 5D D7 FC BC EB F1 " +
            "32 AF 72 C8 7C 85 4E 48 6F 0B CE C0 F0 8A 67 4F " +
            "CB 09 97 6F 04 21 8C E5 89 26 81 5C 07 27 65 CE " +
            "03 17 59 09 26 27 B3 14 E5 FC D7 48 39 EC 64 58 " +
            "4C 51 31 70 18 EE 06 9D 05 71 A9 F0 C8 F8 88 3E " +
            "32 98 FB EA BC EF D9 F5 F5 83 50 BC F9 D5 DC 8C " +
            "54 68 0F 68 DA 59 29 C1 19 8D 13 E2 1E E8 93 8F " +
            "F7 AB 40 F8 B6 C1 AE 34 B3 2E D6 4A AA 0D 44 0C " +
            "64 A2 DE 36 A9 74 5F 94 B2 4F 8A E3 E7 EA DF C8 " +
            "58 65 25 12 85 E3 61 E7 9C 57 97 39 7D 42 03 22 " +
            "F5 B7 94 A9 11 C2 E9 F9 37 7B DE B5 76 E6 20 95 " +
            "4F 4B 3F 39 A0 C7 AE 92 34 7D 3D 3F 0A 33 77 E8 " +
            "E5 AD 36 95 E5 24 F1 06",
            // Round #20
            // After Theta
            "D5 FF 48 F2 F7 BF 72 51 FF E3 64 1D A0 F8 C3 62 " +
            "DB 2E CF D6 72 E1 7D 85 90 66 6F 65 ED 6A A5 52 " +
            "58 59 6C 4C A4 82 F0 BC 43 31 F6 55 1C E0 CA D4 " +
            "1C A6 60 C3 7A 63 9B 87 0C 7D 6A 56 37 88 57 95 " +
            "B3 3C 90 D5 05 0E C4 80 96 21 52 D3 68 5B F4 67 " +
            "F8 8F 8C E3 A7 28 76 EF EA 32 69 76 A5 91 F4 1F " +
            "BD E9 B2 76 D4 3D 1A 0C E6 E0 B2 47 03 08 51 92 " +
            "64 FB BB DB 16 62 D2 6D 79 39 A1 43 B1 CA EB 16 " +
            "7B 13 E7 FC F5 30 77 07 5B CE 37 FD E9 8E EC 05 " +
            "A7 08 84 B7 98 03 A3 FA 0F 07 6C 1A DD E1 7F 7B " +
            "3F A0 E3 A0 0A 05 46 E3 28 CA E7 7F 2A A2 08 06 " +
            "A6 CA 82 27 AE A3 9D 5F CB 10 9C 9A 17 D3 B5 F5 " +
            "76 FD CD B6 45 87 8D 5F",
            // After Rho
            "D5 FF 48 F2 F7 BF 72 51 FE C7 C9 3A 40 F1 87 C5 " +
            "B6 CB B3 B5 5C 78 5F E1 AE 56 2A 05 69 F6 56 D6 " +
            "15 84 E7 C5 CA 62 63 22 C5 01 AE 4C 3D 14 63 5F " +
            "36 AC 37 B6 79 C8 61 0A 25 43 9F 9A D5 0D E2 55 " +
            "1E C8 EA 02 07 62 C0 59 45 7F 66 19 22 35 8D B6 " +
            "C7 7F 64 1C 3F 45 B1 7B 7F A8 CB A4 D9 95 46 D2 " +
            "B5 A3 EE D1 60 E8 4D 97 10 A2 24 CD C1 65 8F 06 " +
            "6D 0B 31 E9 36 B2 FD DD 87 62 95 D7 2D F2 72 42 " +
            "9C BF 1E E6 EE 60 6F E2 F6 82 2D E7 9B FE 74 47 " +
            "60 54 FF 14 81 F0 16 73 7B 0F 07 6C 1A DD E1 7F " +
            "18 8D FF 80 8E 83 2A 14 A0 28 9F FF A9 88 22 18 " +
            "54 59 F0 C4 75 B4 F3 CB 10 9C 9A 17 D3 B5 F5 CB " +
            "E3 97 5D 7F B3 6D D1 61",
            // After Pi
            "D5 FF 48 F2 F7 BF 72 51 36 AC 37 B6 79 C8 61 0A " +
            "B5 A3 EE D1 60 E8 4D 97 60 54 FF 14 81 F0 16 73 " +
            "E3 97 5D 7F B3 6D D1 61 AE 56 2A 05 69 F6 56 D6 " +
            "45 7F 66 19 22 35 8D B6 C7 7F 64 1C 3F 45 B1 7B " +
            "9C BF 1E E6 EE 60 6F E2 54 59 F0 C4 75 B4 F3 CB " +
            "FE C7 C9 3A 40 F1 87 C5 25 43 9F 9A D5 0D E2 55 " +
            "10 A2 24 CD C1 65 8F 06 7B 0F 07 6C 1A DD E1 7F " +
            "18 8D FF 80 8E 83 2A 14 15 84 E7 C5 CA 62 63 22 " +
            "C5 01 AE 4C 3D 14 63 5F 7F A8 CB A4 D9 95 46 D2 " +
            "F6 82 2D E7 9B FE 74 47 10 9C 9A 17 D3 B5 F5 CB " +
            "B6 CB B3 B5 5C 78 5F E1 1E C8 EA 02 07 62 C0 59 " +
            "6D 0B 31 E9 36 B2 FD DD 87 62 95 D7 2D F2 72 42 " +
            "A0 28 9F FF A9 88 22 18",
            // After Chi
            "54 FC 80 B3 F7 9F 7E C4 76 F8 26 B2 F8 D8 73 6A " +
            "36 20 EE BA 52 E5 8C 97 74 3C FF 94 C5 62 34 63 " +
            "C1 97 6A 7B BB 2D D0 6B 2C 56 2A 01 74 B6 66 9F " +
            "5D FF 7C FB E2 15 C3 36 87 3F 84 1C 2E D1 21 72 " +
            "36 B9 14 E7 E6 22 6B F6 15 70 B4 DC 77 B5 7A EB " +
            "EE 67 E9 7F 40 91 8A C7 4E 4E 9C BA CF 95 82 2C " +
            "10 22 DC 4D 45 67 85 06 9D 4D 07 56 5A AD 64 BE " +
            "19 8D E9 00 1B 8F 4A 04 2F 2C A6 65 0A E3 67 A2 " +
            "45 03 8A 0F 3F 7E 53 5A 7F B4 59 B4 99 94 C7 5A " +
            "F3 82 48 27 93 BC 76 67 D0 9D 92 1F E6 A1 F5 96 " +
            "D7 C8 A2 5C 6C E8 62 65 9C A8 6E 14 0E 22 C2 5B " +
            "4D 03 3B C1 B6 BA FD C5 91 A1 B5 D7 79 82 2F A3 " +
            "A8 28 D7 FD AA 8A A2 00",
            // After Iota 
            "D5 7C 80 33 F7 9F 7E 44 76 F8 26 B2 F8 D8 73 6A " +
            "36 20 EE BA 52 E5 8C 97 74 3C FF 94 C5 62 34 63 " +
            "C1 97 6A 7B BB 2D D0 6B 2C 56 2A 01 74 B6 66 9F " +
            "5D FF 7C FB E2 15 C3 36 87 3F 84 1C 2E D1 21 72 " +
            "36 B9 14 E7 E6 22 6B F6 15 70 B4 DC 77 B5 7A EB " +
            "EE 67 E9 7F 40 91 8A C7 4E 4E 9C BA CF 95 82 2C " +
            "10 22 DC 4D 45 67 85 06 9D 4D 07 56 5A AD 64 BE " +
            "19 8D E9 00 1B 8F 4A 04 2F 2C A6 65 0A E3 67 A2 " +
            "45 03 8A 0F 3F 7E 53 5A 7F B4 59 B4 99 94 C7 5A " +
            "F3 82 48 27 93 BC 76 67 D0 9D 92 1F E6 A1 F5 96 " +
            "D7 C8 A2 5C 6C E8 62 65 9C A8 6E 14 0E 22 C2 5B " +
            "4D 03 3B C1 B6 BA FD C5 91 A1 B5 D7 79 82 2F A3 " +
            "A8 28 D7 FD AA 8A A2 00",
            // Round #21
            // After Theta
            "18 66 B7 A6 A5 AA 8F B5 BF 44 C8 FB 70 91 C0 49 " +
            "F1 15 EF F8 91 46 EA 38 8D 09 CE 80 E5 66 48 3A " +
            "A3 2F F4 46 62 99 9D 33 E1 4C 1D 94 26 83 97 6E " +
            "94 43 92 B2 6A 5C 70 15 40 0A 85 5E ED 72 47 DD " +
            "CF 8C 25 F3 C6 26 17 AF 77 C8 2A E1 AE 01 37 B3 " +
            "23 7D DE EA 12 A4 7B 36 87 F2 72 F3 47 DC 31 0F " +
            "D7 17 DD 0F 86 C4 E3 A9 64 78 36 42 7A A9 18 E7 " +
            "7B 35 77 3D C2 3B 07 5C E2 36 91 F0 58 D6 96 53 " +
            "8C BF 64 46 B7 37 E0 79 B8 81 58 F6 5A 37 A1 F5 " +
            "0A B7 79 33 B3 B8 0A 3E B2 25 0C 22 3F 15 B8 CE " +
            "1A D2 95 C9 3E DD 93 94 55 14 80 5D 86 6B 71 78 " +
            "8A 36 3A 83 75 19 9B 6A 68 94 84 C3 59 86 53 FA " +
            "CA 90 49 C0 73 3E EF 58",
            // After Rho
            "18 66 B7 A6 A5 AA 8F B5 7E 89 90 F7 E1 22 81 93 " +
            "7C C5 3B 7E A4 91 3A 4E 6E 86 A4 D3 98 E0 0C 58 " +
            "CB EC 9C 19 7D A1 37 12 69 32 78 E9 16 CE D4 41 " +
            "29 AB C6 05 57 41 39 24 37 90 42 A1 57 BB DC 51 " +
            "C6 92 79 63 93 8B D7 67 70 33 7B 87 AC 12 EE 1A " +
            "19 E9 F3 56 97 20 DD B3 3C 1C CA CB CD 1F 71 C7 " +
            "7E 30 24 1E 4F BD BE E8 52 31 CE C9 F0 6C 84 F4 " +
            "1E E1 9D 03 AE BD 9A BB E1 B1 AC 2D A7 C4 6D 22 " +
            "CC E8 F6 06 3C 8F F1 97 D0 7A DC 40 2C 7B AD 9B " +
            "57 C1 47 E1 36 6F 66 16 CE B2 25 0C 22 3F 15 B8 " +
            "4F 52 6A 48 57 26 FB 74 55 51 00 76 19 AE C5 E1 " +
            "D1 46 67 B0 2E 63 53 4D 94 84 C3 59 86 53 FA 68 " +
            "3B 96 32 64 12 F0 9C CF",
            // After Pi
            "18 66 B7 A6 A5 AA 8F B5 29 AB C6 05 57 41 39 24 " +
            "7E 30 24 1E 4F BD BE E8 57 C1 47 E1 36 6F 66 16 " +
            "3B 96 32 64 12 F0 9C CF 6E 86 A4 D3 98 E0 0C 58 " +
            "70 33 7B 87 AC 12 EE 1A 19 E9 F3 56 97 20 DD B3 " +
            "CC E8 F6 06 3C 8F F1 97 D1 46 67 B0 2E 63 53 4D " +
            "7E 89 90 F7 E1 22 81 93 37 90 42 A1 57 BB DC 51 " +
            "52 31 CE C9 F0 6C 84 F4 CE B2 25 0C 22 3F 15 B8 " +
            "4F 52 6A 48 57 26 FB 74 CB EC 9C 19 7D A1 37 12 " +
            "69 32 78 E9 16 CE D4 41 3C 1C CA CB CD 1F 71 C7 " +
            "D0 7A DC 40 2C 7B AD 9B 94 84 C3 59 86 53 FA 68 " +
            "7C C5 3B 7E A4 91 3A 4E C6 92 79 63 93 8B D7 67 " +
            "1E E1 9D 03 AE BD 9A BB E1 B1 AC 2D A7 C4 6D 22 " +
            "55 51 00 76 19 AE C5 E1",
            // After Chi
            "4E 76 97 BC AD 16 09 7D 28 6A 85 E4 67 03 79 32 " +
            "56 26 14 1A 4F 2D 26 21 57 A1 C2 63 93 65 65 26 " +
            "1A 1F 72 65 40 B1 AC CF 67 4E 24 83 8B C0 1D F9 " +
            "B4 33 7F 87 84 9D CE 1E 08 EF F2 E6 95 40 DF FB " +
            "E2 68 76 45 AC 0F FD 87 C1 77 3C B4 0A 71 B1 4F " +
            "3E A8 1C BF 41 66 81 37 BB 12 63 A5 55 A8 CD 59 " +
            "53 71 84 89 A5 6C 6E B0 FE 3B B5 BB 82 3F 15 3B " +
            "4E 42 28 48 41 BF A7 34 DF E0 1E 1B B4 B0 16 94 " +
            "A9 50 6C E9 36 AE 58 59 38 98 C9 D2 4F 1F 23 A7 " +
            "9B 12 C0 40 55 DB A8 89 B4 96 A3 B9 84 1D 3A 29 " +
            "64 A4 BF 7E 88 A5 32 D6 27 82 59 4F 92 CB B2 67 " +
            "0A A1 9D 51 B6 97 1A 7A C9 35 97 25 03 D5 57 2C " +
            "D7 43 40 77 0A A4 00 C0",
            // After Iota 
            "CE F6 97 BC AD 16 09 FD 28 6A 85 E4 67 03 79 32 " +
            "56 26 14 1A 4F 2D 26 21 57 A1 C2 63 93 65 65 26 " +
            "1A 1F 72 65 40 B1 AC CF 67 4E 24 83 8B C0 1D F9 " +
            "B4 33 7F 87 84 9D CE 1E 08 EF F2 E6 95 40 DF FB " +
            "E2 68 76 45 AC 0F FD 87 C1 77 3C B4 0A 71 B1 4F " +
            "3E A8 1C BF 41 66 81 37 BB 12 63 A5 55 A8 CD 59 " +
            "53 71 84 89 A5 6C 6E B0 FE 3B B5 BB 82 3F 15 3B " +
            "4E 42 28 48 41 BF A7 34 DF E0 1E 1B B4 B0 16 94 " +
            "A9 50 6C E9 36 AE 58 59 38 98 C9 D2 4F 1F 23 A7 " +
            "9B 12 C0 40 55 DB A8 89 B4 96 A3 B9 84 1D 3A 29 " +
            "64 A4 BF 7E 88 A5 32 D6 27 82 59 4F 92 CB B2 67 " +
            "0A A1 9D 51 B6 97 1A 7A C9 35 97 25 03 D5 57 2C " +
            "D7 43 40 77 0A A4 00 C0",
            // Round #22
            // After Theta
            "6A 3A 4B 2A 0C 76 A9 37 7B 3C E6 ED 31 B5 95 2C " +
            "CD 15 15 8A 8A C9 52 14 84 DF FF 3A 1F 61 CA 2A " +
            "5B 62 38 57 1C A0 BD 13 C3 82 F8 15 2A A0 BD 33 " +
            "E7 65 1C 8E D2 2B 22 00 93 DC F3 76 50 A4 AB CE " +
            "31 16 4B 1C 20 0B 52 8B 80 0A 76 86 56 60 A0 93 " +
            "9A 64 C0 29 E0 06 21 FD E8 44 00 AC 03 1E 21 47 " +
            "C8 42 85 19 60 88 1A 85 2D 45 88 E2 0E 3B BA 37 " +
            "0F 3F 62 7A 1D AE B6 E8 7B 2C C2 8D 15 D0 B6 5E " +
            "FA 06 0F E0 60 18 B4 47 A3 AB C8 42 8A FB 57 92 " +
            "48 6C FD 19 D9 DF 07 85 F5 EB E9 8B D8 0C 2B F5 " +
            "C0 68 63 E8 29 C5 92 1C 74 D4 3A 46 C4 7D 5E 79 " +
            "91 92 9C C1 73 73 6E 4F 1A 4B AA 7C 8F D1 F8 20 " +
            "96 3E 0A 45 56 B5 11 1C",
            // After Rho
            "6A 3A 4B 2A 0C 76 A9 37 F6 78 CC DB 63 6A 2B 59 " +
            "73 45 85 A2 62 B2 14 45 11 A6 AC 42 F8 FD AF F3 " +
            "00 ED 9D D8 12 C3 B9 E2 A1 02 DA 3B 33 2C 88 5F " +
            "E1 28 BD 22 02 70 5E C6 F3 24 F7 BC 1D 14 E9 AA " +
            "8B 25 0E 90 05 A9 C5 18 06 3A 09 A8 60 67 68 05 " +
            "D7 24 03 4E 01 37 08 E9 1C A1 13 01 B0 0E 78 84 " +
            "CC 00 43 D4 28 44 16 2A 76 74 6F 5A 8A 10 C5 1D " +
            "BD 0E 57 5B F4 87 1F 31 1B 2B A0 6D BD F6 58 84 " +
            "01 1C 0C 83 F6 48 DF E0 2B C9 D1 55 64 21 C5 FD " +
            "FB A0 10 89 AD 3F 23 FB F5 F5 EB E9 8B D8 0C 2B " +
            "4B 72 00 A3 8D A1 A7 14 D1 51 EB 18 11 F7 79 E5 " +
            "52 92 33 78 6E CE ED 29 4B AA 7C 8F D1 F8 20 1A " +
            "04 87 A5 8F 42 91 55 6D",
            // After Pi
            "6A 3A 4B 2A 0C 76 A9 37 E1 28 BD 22 02 70 5E C6 " +
            "CC 00 43 D4 28 44 16 2A FB A0 10 89 AD 3F 23 FB " +
            "04 87 A5 8F 42 91 55 6D 11 A6 AC 42 F8 FD AF F3 " +
            "06 3A 09 A8 60 67 68 05 D7 24 03 4E 01 37 08 E9 " +
            "01 1C 0C 83 F6 48 DF E0 52 92 33 78 6E CE ED 29 " +
            "F6 78 CC DB 63 6A 2B 59 F3 24 F7 BC 1D 14 E9 AA " +
            "76 74 6F 5A 8A 10 C5 1D F5 F5 EB E9 8B D8 0C 2B " +
            "4B 72 00 A3 8D A1 A7 14 00 ED 9D D8 12 C3 B9 E2 " +
            "A1 02 DA 3B 33 2C 88 5F 1C A1 13 01 B0 0E 78 84 " +
            "2B C9 D1 55 64 21 C5 FD 4B AA 7C 8F D1 F8 20 1A " +
            "73 45 85 A2 62 B2 14 45 8B 25 0E 90 05 A9 C5 18 " +
            "BD 0E 57 5B F4 87 1F 31 1B 2B A0 6D BD F6 58 84 " +
            "D1 51 EB 18 11 F7 79 E5",
            // After Chi
            "66 3A 09 FE 24 72 A9 1F D2 88 AD 2B 87 4B 7F 17 " +
            "C8 07 E6 D2 6A C4 42 2E 91 98 5A A9 A1 59 8B E9 " +
            "85 87 11 8F 40 91 03 AD C0 A2 AE 04 F9 ED AF 1B " +
            "06 22 05 29 96 2F BF 05 85 A6 30 36 09 B1 28 E0 " +
            "00 38 80 81 66 79 DD 32 54 8A 32 D0 6E CC AD 2D " +
            "F2 28 C4 99 E1 6A 2F 4C 72 A5 77 1D 1C DC E1 88 " +
            "7C 76 6F 58 8E 31 66 09 41 FD 27 B1 E9 92 04 62 " +
            "4A 76 33 87 91 B5 67 B6 1C 4C 9C D8 92 C1 C9 62 " +
            "82 4A 1A 6F 77 0D 0D 26 5C 83 3F 8B 21 D6 58 86 " +
            "2B 8C 50 05 66 22 5C 1D EA A8 3E AC F0 D4 20 07 " +
            "47 4F D4 E9 92 B4 0E 64 89 04 AE B4 0C D9 85 9C " +
            "7D 5E 1C 4B F4 86 3E 50 39 2F A4 CF DF F6 5C 84 " +
            "59 71 E1 08 14 FE B8 FD",
            // After Iota 
            "67 3A 09 7E 24 72 A9 1F D2 88 AD 2B 87 4B 7F 17 " +
            "C8 07 E6 D2 6A C4 42 2E 91 98 5A A9 A1 59 8B E9 " +
            "85 87 11 8F 40 91 03 AD C0 A2 AE 04 F9 ED AF 1B " +
            "06 22 05 29 96 2F BF 05 85 A6 30 36 09 B1 28 E0 " +
            "00 38 80 81 66 79 DD 32 54 8A 32 D0 6E CC AD 2D " +
            "F2 28 C4 99 E1 6A 2F 4C 72 A5 77 1D 1C DC E1 88 " +
            "7C 76 6F 58 8E 31 66 09 41 FD 27 B1 E9 92 04 62 " +
            "4A 76 33 87 91 B5 67 B6 1C 4C 9C D8 92 C1 C9 62 " +
            "82 4A 1A 6F 77 0D 0D 26 5C 83 3F 8B 21 D6 58 86 " +
            "2B 8C 50 05 66 22 5C 1D EA A8 3E AC F0 D4 20 07 " +
            "47 4F D4 E9 92 B4 0E 64 89 04 AE B4 0C D9 85 9C " +
            "7D 5E 1C 4B F4 86 3E 50 39 2F A4 CF DF F6 5C 84 " +
            "59 71 E1 08 14 FE B8 FD",
            // Round #23
            // After Theta
            "15 1B 10 8A 92 68 AA 92 FC 2F B2 00 CB E3 45 7B " +
            "E1 BB 9E B0 32 65 4F 4E D0 D6 5F 2C 2F C9 42 60 " +
            "5B 1F 4F 78 AE F7 8C 10 B2 83 B7 F0 4F F7 AC 96 " +
            "28 85 1A 02 DA 87 85 69 AC 1A 48 54 51 10 25 80 " +
            "41 76 85 04 E8 E9 14 BB 8A 12 6C 27 80 AA 22 90 " +
            "80 09 DD 6D 57 70 2C C1 5C 02 68 36 50 74 DB E4 " +
            "55 CA 17 3A D6 90 6B 69 00 B3 22 34 67 02 CD EB " +
            "94 EE 6D 70 7F D3 E8 0B 6E 6D 85 2C 24 DB CA EF " +
            "AC ED 05 44 3B A5 37 4A 75 3F 47 E9 79 77 55 E6 " +
            "6A C2 55 80 E8 B2 95 94 34 30 60 5B 1E B2 AF BA " +
            "35 6E CD 1D 24 AE 0D E9 A7 A3 B1 9F 40 71 BF F0 " +
            "54 E2 64 29 AC 27 33 30 78 61 A1 4A 51 66 95 0D " +
            "87 E9 BF FF FA 98 37 40",
            // After Rho
            "15 1B 10 8A 92 68 AA 92 F8 5F 64 01 96 C7 8B F6 " +
            "F8 AE 27 AC 4C D9 93 53 92 2C 04 06 6D FD C5 F2 " +
            "BD 67 84 D8 FA 78 C2 73 FF 74 CF 6A 29 3B 78 0B " +
            "21 A0 7D 58 98 86 52 A8 20 AB 06 12 55 14 44 09 " +
            "BB 42 02 F4 74 8A DD 20 2A 02 A9 28 C1 76 02 A8 " +
            "06 4C E8 6E BB 82 63 09 93 73 09 A0 D9 40 D1 6D " +
            "D0 B1 86 5C 4B AB 52 BE 04 9A D7 01 66 45 68 CE " +
            "B8 BF 69 F4 05 4A F7 36 59 48 B6 95 DF DD DA 0A " +
            "80 68 A7 F4 46 89 B5 BD 2A F3 BA 9F A3 F4 BC BB " +
            "B6 92 52 4D B8 0A 10 5D BA 34 30 60 5B 1E B2 AF " +
            "36 A4 D7 B8 35 77 90 B8 9F 8E C6 7E 02 C5 FD C2 " +
            "4A 9C 2C 85 F5 64 06 86 61 A1 4A 51 66 95 0D 78 " +
            "0D D0 61 FA EF BF 3E E6",
            // After Pi
            "15 1B 10 8A 92 68 AA 92 21 A0 7D 58 98 86 52 A8 " +
            "D0 B1 86 5C 4B AB 52 BE B6 92 52 4D B8 0A 10 5D " +
            "0D D0 61 FA EF BF 3E E6 92 2C 04 06 6D FD C5 F2 " +
            "2A 02 A9 28 C1 76 02 A8 06 4C E8 6E BB 82 63 09 " +
            "80 68 A7 F4 46 89 B5 BD 4A 9C 2C 85 F5 64 06 86 " +
            "F8 5F 64 01 96 C7 8B F6 20 AB 06 12 55 14 44 09 " +
            "04 9A D7 01 66 45 68 CE BA 34 30 60 5B 1E B2 AF " +
            "36 A4 D7 B8 35 77 90 B8 BD 67 84 D8 FA 78 C2 73 " +
            "FF 74 CF 6A 29 3B 78 0B 93 73 09 A0 D9 40 D1 6D " +
            "2A F3 BA 9F A3 F4 BC BB 61 A1 4A 51 66 95 0D 78 " +
            "F8 AE 27 AC 4C D9 93 53 BB 42 02 F4 74 8A DD 20 " +
            "B8 BF 69 F4 05 4A F7 36 59 48 B6 95 DF DD DA 0A " +
            "9F 8E C6 7E 02 C5 FD C2",
            // After Chi
            "C5 0A 92 8E D1 41 AA 84 07 A2 2D 59 28 86 52 E9 " +
            "D9 F1 A7 EE 0C 1E 7C 1C A6 99 42 4D A8 4A 90 4D " +
            "2D 70 0C AA E7 39 6E CE 96 60 44 40 57 7D A4 F3 " +
            "AA 22 AE B8 85 7F 96 1C 4C D8 E0 6F 0A E6 61 0B " +
            "10 48 A7 F6 4E 10 74 CD 62 9E 85 AD 75 66 04 8E " +
            "FC 4F B5 00 B4 86 A3 30 9A 8F 26 72 4C 0E D6 28 " +
            "00 1A 10 99 42 24 68 DE 72 6F 10 61 D9 9E B9 E9 " +
            "36 04 D5 AA 74 67 D4 B1 BD 64 84 58 2A 38 43 17 " +
            "D7 F4 7D 75 0B 8F 54 99 D2 73 49 E0 9D 41 D0 2D " +
            "B6 B5 3E 17 3B 9C 7E B8 23 B1 01 73 67 96 35 70 " +
            "F8 13 4E AC 4D 99 B1 45 FA 02 94 F5 AE 1F D5 28 " +
            "3E 39 29 9E 05 4A D2 F6 39 68 97 15 93 C5 D8 1B " +
            "9C CE C6 2E 32 C7 B1 E2",
            // After Iota 
            "CD 8A 92 0E D1 41 AA 04 07 A2 2D 59 28 86 52 E9 " +
            "D9 F1 A7 EE 0C 1E 7C 1C A6 99 42 4D A8 4A 90 4D " +
            "2D 70 0C AA E7 39 6E CE 96 60 44 40 57 7D A4 F3 " +
            "AA 22 AE B8 85 7F 96 1C 4C D8 E0 6F 0A E6 61 0B " +
            "10 48 A7 F6 4E 10 74 CD 62 9E 85 AD 75 66 04 8E " +
            "FC 4F B5 00 B4 86 A3 30 9A 8F 26 72 4C 0E D6 28 " +
            "00 1A 10 99 42 24 68 DE 72 6F 10 61 D9 9E B9 E9 " +
            "36 04 D5 AA 74 67 D4 B1 BD 64 84 58 2A 38 43 17 " +
            "D7 F4 7D 75 0B 8F 54 99 D2 73 49 E0 9D 41 D0 2D " +
            "B6 B5 3E 17 3B 9C 7E B8 23 B1 01 73 67 96 35 70 " +
            "F8 13 4E AC 4D 99 B1 45 FA 02 94 F5 AE 1F D5 28 " +
            "3E 39 29 9E 05 4A D2 F6 39 68 97 15 93 C5 D8 1B " +
            "9C CE C6 2E 32 C7 B1 E2",
            // Xor'd state (in bytes)                        " +
            "CD 8A 92 0E D1 41 AA 04 07 A2 2D 59 28 86 52 E9 " +
            "D9 F1 A7 EE 0C 1E 7C 1C A6 99 42 4D A8 4A 90 4D " +
            "2D 70 0C AA E7 39 6E CE 96 60 44 40 57 7D A4 F3 " +
            "AA 22 AE B8 85 7F 96 1C 4C D8 E0 6F 0A E6 61 0B " +
            "10 48 A7 F6 4E 10 74 CD 62 9E 85 AD 75 66 04 8E " +
            "FC 4F B5 00 B4 86 A3 30 9A 8F 26 72 4C 0E D6 28 " +
            "00 1A 10 99 42 24 68 DE 72 6F 10 61 D9 9E B9 E9 " +
            "36 04 D5 AA 74 67 D4 B1 BD 64 84 58 2A 38 43 17 " +
            "D7 F4 7D 75 0B 8F 54 99 D2 73 49 E0 9D 41 D0 2D " +
            "B6 B5 3E 17 3B 9C 7E B8 23 B1 01 73 67 96 35 70 " +
            "F8 13 4E AC 4D 99 B1 45 FA 02 94 F5 AE 1F D5 28 " +
            "3E 39 29 9E 05 4A D2 F6 39 68 97 15 93 C5 D8 1B " +
            "9C CE C6 2E 32 C7 B1 E2",
            // Round #0
            // After Theta
            "3F ED 90 D8 EA E6 B6 BE 17 82 EA 2F C5 32 E2 58 " +
            "54 CE 53 4D 67 42 18 E5 53 CB 42 CA 13 4E 93 99 " +
            "A3 B6 03 07 DB 92 2B 2E 64 07 46 96 6C DA B8 49 " +
            "BA 02 69 CE 68 CB 26 AD C1 E7 14 CC 61 BA 05 F2 " +
            "E5 1A A7 71 F5 14 77 19 EC 58 8A 00 49 CD 41 6E " +
            "0E 28 B7 D6 8F 21 BF 8A 8A AF E1 04 A1 BA 66 99 " +
            "8D 25 E4 3A 29 78 0C 27 87 3D 10 E6 62 9A BA 3D " +
            "B8 C2 DA 07 48 CC 91 51 4F 03 86 8E 11 9F 5F AD " +
            "C7 D4 BA 03 E6 3B E4 28 5F 4C BD 43 F6 1D B4 D4 " +
            "43 E7 3E 90 80 98 7D 6C AD 77 0E DE 5B 3D 70 90 " +
            "0A 74 4C 7A 76 3E AD FF EA 22 53 83 43 AB 65 99 " +
            "B3 06 DD 3D 6E 16 B6 0F CC 3A 97 92 28 C1 DB CF " +
            "12 08 C9 83 0E 6C F4 02",
            // After Rho
            "3F ED 90 D8 EA E6 B6 BE 2E 04 D5 5F 8A 65 C4 B1 " +
            "95 F3 54 D3 99 10 46 39 E1 34 99 39 B5 2C A4 3C " +
            "96 5C 71 19 B5 1D 38 D8 C9 A6 8D 9B 44 76 60 64 " +
            "E6 8C B6 6C D2 AA 2B 90 7C F0 39 05 73 98 6E 81 " +
            "8D D3 B8 7A 8A BB 8C 72 1C E4 C6 8E A5 08 90 D4 " +
            "74 40 B9 B5 7E 0C F9 55 65 2A BE 86 13 84 EA 9A " +
            "D7 49 C1 63 38 69 2C 21 34 75 7B 0E 7B 20 CC C5 " +
            "03 24 E6 C8 28 5C 61 ED 1D 23 3E BF 5A 9F 06 0C " +
            "77 C0 7C 87 1C E5 98 5A 5A EA 2F A6 DE 21 FB 0E " +
            "B3 8F 6D E8 DC 07 12 10 90 AD 77 0E DE 5B 3D 70 " +
            "B4 FE 2B D0 31 E9 D9 F9 AA 8B 4C 0D 0E AD 96 65 " +
            "D6 A0 BB C7 CD C2 F6 61 3A 97 92 28 C1 DB CF CC " +
            "BD 80 04 42 F2 A0 03 1B",
            // After Pi
            "3F ED 90 D8 EA E6 B6 BE E6 8C B6 6C D2 AA 2B 90 " +
            "D7 49 C1 63 38 69 2C 21 B3 8F 6D E8 DC 07 12 10 " +
            "BD 80 04 42 F2 A0 03 1B E1 34 99 39 B5 2C A4 3C " +
            "1C E4 C6 8E A5 08 90 D4 74 40 B9 B5 7E 0C F9 55 " +
            "77 C0 7C 87 1C E5 98 5A D6 A0 BB C7 CD C2 F6 61 " +
            "2E 04 D5 5F 8A 65 C4 B1 7C F0 39 05 73 98 6E 81 " +
            "34 75 7B 0E 7B 20 CC C5 90 AD 77 0E DE 5B 3D 70 " +
            "B4 FE 2B D0 31 E9 D9 F9 96 5C 71 19 B5 1D 38 D8 " +
            "C9 A6 8D 9B 44 76 60 64 65 2A BE 86 13 84 EA 9A " +
            "5A EA 2F A6 DE 21 FB 0E 3A 97 92 28 C1 DB CF CC " +
            "95 F3 54 D3 99 10 46 39 8D D3 B8 7A 8A BB 8C 72 " +
            "03 24 E6 C8 28 5C 61 ED 1D 23 3E BF 5A 9F 06 0C " +
            "AA 8B 4C 0D 0E AD 96 65",
            // After Chi
            "2E AC D1 DB C2 A7 B2 9F C6 0A 9A E4 16 AC 39 80 " +
            "DB 49 C1 61 1A C9 2D 2A B1 E2 FD 70 D4 41 A6 B4 " +
            "7D 80 22 66 E2 A8 0A 1B 81 34 A0 08 EF 28 CD 3D " +
            "1F 64 82 8C A5 E9 90 DE F4 60 3A F5 BF 0E 9F 74 " +
            "56 D4 7C BF 2C C9 98 46 CA 60 FD 41 CD C2 E6 A1 " +
            "2E 01 97 55 82 45 44 F5 FC 78 3D 05 F7 C3 5F B1 " +
            "10 27 73 DE 5A 80 0C 4C 9A AD A3 01 54 5F 39 70 " +
            "E4 0E 03 D0 40 71 F3 F9 B2 54 43 1D A6 9D B2 42 " +
            "D3 66 8C BB 88 57 71 60 45 3F 2E 8E 12 5E EE 5A " +
            "DE A2 4E B7 EA 25 CB 1E 73 35 1E AA 81 B9 8F E8 " +
            "97 D7 12 53 B9 54 27 B4 91 D0 A0 4D D8 38 8A 72 " +
            "A1 AC A6 C8 2C 7C F1 8C 08 53 2E 6D CB 8F 46 14 " +
            "A2 8B E4 25 0C 06 1E 27",
            // After Iota 
            "2F AC D1 DB C2 A7 B2 9F C6 0A 9A E4 16 AC 39 80 " +
            "DB 49 C1 61 1A C9 2D 2A B1 E2 FD 70 D4 41 A6 B4 " +
            "7D 80 22 66 E2 A8 0A 1B 81 34 A0 08 EF 28 CD 3D " +
            "1F 64 82 8C A5 E9 90 DE F4 60 3A F5 BF 0E 9F 74 " +
            "56 D4 7C BF 2C C9 98 46 CA 60 FD 41 CD C2 E6 A1 " +
            "2E 01 97 55 82 45 44 F5 FC 78 3D 05 F7 C3 5F B1 " +
            "10 27 73 DE 5A 80 0C 4C 9A AD A3 01 54 5F 39 70 " +
            "E4 0E 03 D0 40 71 F3 F9 B2 54 43 1D A6 9D B2 42 " +
            "D3 66 8C BB 88 57 71 60 45 3F 2E 8E 12 5E EE 5A " +
            "DE A2 4E B7 EA 25 CB 1E 73 35 1E AA 81 B9 8F E8 " +
            "97 D7 12 53 B9 54 27 B4 91 D0 A0 4D D8 38 8A 72 " +
            "A1 AC A6 C8 2C 7C F1 8C 08 53 2E 6D CB 8F 46 14 " +
            "A2 8B E4 25 0C 06 1E 27",
            // Round #1
            // After Theta
            "62 BC E4 95 09 D1 27 E9 D4 2B 2C 34 24 64 D5 A8 " +
            "EB 3C 4C D2 14 DB 34 C6 6F DE B1 8C D1 6D 1A 69 " +
            "9D DF 0E E3 0E D2 DC D0 CC 24 95 46 24 5E 58 4B " +
            "0D 45 34 5C 97 21 7C F6 C4 15 B7 46 B1 1C 86 98 " +
            "88 E8 30 43 29 E5 24 9B 2A 3F D1 C4 21 B8 30 6A " +
            "63 11 A2 1B 49 33 D1 83 EE 59 8B D5 C5 0B B3 99 " +
            "20 52 FE 6D 54 92 15 A0 44 91 EF FD 51 73 85 AD " +
            "04 51 2F 55 AC 0B 25 32 FF 44 76 53 6D EB 27 34 " +
            "C1 47 3A 6B BA 9F 9D 48 75 4A A3 3D 1C 4C F7 B6 " +
            "00 9E 02 4B EF 09 77 C3 93 6A 32 2F 6D C3 59 23 " +
            "DA C7 27 1D 72 22 B2 C2 83 F1 16 9D EA F0 66 5A " +
            "91 D9 2B 7B 22 6E E8 60 D6 6F 62 91 CE A3 FA C9 " +
            "42 D4 C8 A0 E0 7C C8 EC",
            // After Rho
            "62 BC E4 95 09 D1 27 E9 A9 57 58 68 48 C8 AA 51 " +
            "3A 0F 93 34 C5 36 8D F1 DD A6 91 F6 E6 1D CB 18 " +
            "90 E6 86 EE FC 76 18 77 44 E2 85 B5 C4 4C 52 69 " +
            "C3 75 19 C2 67 DF 50 44 26 71 C5 AD 51 2C 87 21 " +
            "74 98 A1 94 72 92 4D 44 0B A3 A6 F2 13 4D 1C 82 " +
            "1C 8B 10 DD 48 9A 89 1E 66 BA 67 2D 56 17 2F CC " +
            "6F A3 92 AC 00 05 91 F2 E6 0A 5B 89 22 DF FB A3 " +
            "2A D6 85 12 19 82 A8 97 A6 DA D6 4F 68 FE 89 EC " +
            "67 4D F7 B3 13 29 F8 48 7B DB 3A A5 D1 1E 0E A6 " +
            "E1 6E 18 C0 53 60 E9 3D 23 93 6A 32 2F 6D C3 59 " +
            "C8 0A 6B 1F 9F 74 C8 89 0D C6 5B 74 AA C3 9B 69 " +
            "32 7B 65 4F C4 0D 1D 2C 6F 62 91 CE A3 FA C9 D6 " +
            "32 BB 10 35 32 28 38 1F",
            // After Pi
            "62 BC E4 95 09 D1 27 E9 C3 75 19 C2 67 DF 50 44 " +
            "6F A3 92 AC 00 05 91 F2 E1 6E 18 C0 53 60 E9 3D " +
            "32 BB 10 35 32 28 38 1F DD A6 91 F6 E6 1D CB 18 " +
            "0B A3 A6 F2 13 4D 1C 82 1C 8B 10 DD 48 9A 89 1E " +
            "67 4D F7 B3 13 29 F8 48 32 7B 65 4F C4 0D 1D 2C " +
            "A9 57 58 68 48 C8 AA 51 26 71 C5 AD 51 2C 87 21 " +
            "E6 0A 5B 89 22 DF FB A3 23 93 6A 32 2F 6D C3 59 " +
            "C8 0A 6B 1F 9F 74 C8 89 90 E6 86 EE FC 76 18 77 " +
            "44 E2 85 B5 C4 4C 52 69 66 BA 67 2D 56 17 2F CC " +
            "7B DB 3A A5 D1 1E 0E A6 6F 62 91 CE A3 FA C9 D6 " +
            "3A 0F 93 34 C5 36 8D F1 74 98 A1 94 72 92 4D 44 " +
            "2A D6 85 12 19 82 A8 97 A6 DA D6 4F 68 FE 89 EC " +
            "0D C6 5B 74 AA C3 9B 69",
            // After Chi
            "4E 3E 66 B9 09 D1 A6 5B 43 39 11 82 34 BF 38 49 " +
            "7D 32 92 99 20 0D 81 F0 A1 6A FC 40 5A B1 EE DD " +
            "B3 FA 09 77 54 26 68 1B C9 AE 81 FB AE 8F 4A 04 " +
            "68 E7 41 D0 00 6C 6C C2 0C B9 10 91 8C 9E 8C 3A " +
            "AA C9 67 03 31 39 3A 58 30 7A 43 4F D5 4D 09 AE " +
            "69 5D 42 68 6A 1B D2 D3 27 E0 E5 9F 5C 0C 87 79 " +
            "2E 02 5A 84 B2 CF F3 23 02 C6 7A 52 6F E5 E1 09 " +
            "CE 2A EE 9A 8E 50 CD A9 B2 FE E4 E6 EE 65 35 F3 " +
            "5D A3 9D 35 45 44 52 4B 62 9A E6 67 74 F7 EE 9C " +
            "EB 5F 3C 85 8D 1A 1E 87 2B 62 90 DF A3 F2 8B DE " +
            "30 49 97 36 CC 36 2D 62 F0 90 F3 D9 12 EE 4C 2C " +
            "23 D2 8C 22 9B 83 BA 96 94 D3 56 4F 2D CA 8D 7C " +
            "49 56 7B F4 98 43 DB 6D",
            // After Iota 
            "CC BE 66 B9 09 D1 A6 5B 43 39 11 82 34 BF 38 49 " +
            "7D 32 92 99 20 0D 81 F0 A1 6A FC 40 5A B1 EE DD " +
            "B3 FA 09 77 54 26 68 1B C9 AE 81 FB AE 8F 4A 04 " +
            "68 E7 41 D0 00 6C 6C C2 0C B9 10 91 8C 9E 8C 3A " +
            "AA C9 67 03 31 39 3A 58 30 7A 43 4F D5 4D 09 AE " +
            "69 5D 42 68 6A 1B D2 D3 27 E0 E5 9F 5C 0C 87 79 " +
            "2E 02 5A 84 B2 CF F3 23 02 C6 7A 52 6F E5 E1 09 " +
            "CE 2A EE 9A 8E 50 CD A9 B2 FE E4 E6 EE 65 35 F3 " +
            "5D A3 9D 35 45 44 52 4B 62 9A E6 67 74 F7 EE 9C " +
            "EB 5F 3C 85 8D 1A 1E 87 2B 62 90 DF A3 F2 8B DE " +
            "30 49 97 36 CC 36 2D 62 F0 90 F3 D9 12 EE 4C 2C " +
            "23 D2 8C 22 9B 83 BA 96 94 D3 56 4F 2D CA 8D 7C " +
            "49 56 7B F4 98 43 DB 6D",
            // Round #2
            // After Theta
            "A0 3B 9F 73 43 B1 C0 DF 90 41 A2 EB 38 F8 4A 93 " +
            "30 ED 5E 0F 56 03 01 8A E0 97 D1 9B C2 8D BD 61 " +
            "19 E6 2F 59 2F B6 82 56 A5 2B 78 31 E4 EF 2C 80 " +
            "BB 9F F2 B9 0C 2B 1E 18 41 66 DC 07 FA 90 0C 40 " +
            "EB 34 4A D8 A9 05 69 E4 9A 66 65 61 AE DD E3 E3 " +
            "05 D8 BB A2 20 7B B4 57 F4 98 56 F6 50 4B F5 A3 " +
            "63 DD 96 12 C4 C1 73 59 43 3B 57 89 F7 D9 B2 B5 " +
            "64 36 C8 B4 F5 C0 27 E4 DE 7B 1D 2C A4 05 53 77 " +
            "8E DB 2E 5C 49 03 20 91 2F 45 2A F1 02 F9 6E E6 " +
            "AA A2 11 5E 15 26 4D 3B 81 7E B6 F1 D8 62 61 93 " +
            "5C CC 6E FC 86 56 4B E6 23 E8 40 B0 1E A9 3E F6 " +
            "6E 0D 40 B4 ED 8D 3A EC D5 2E 7B 94 B5 F6 DE C0 " +
            "E3 4A 5D DA E3 D3 31 20",
            // After Rho
            "A0 3B 9F 73 43 B1 C0 DF 21 83 44 D7 71 F0 95 26 " +
            "4C BB D7 83 D5 40 80 22 DC D8 1B 06 7E 19 BD 29 " +
            "B1 15 B4 CA 30 7F C9 7A 43 FE CE 02 58 BA 82 17 " +
            "9F CB B0 E2 81 B1 FB 29 50 90 19 F7 81 3E 24 03 " +
            "1A 25 EC D4 82 34 F2 75 3D 3E AE 69 56 16 E6 DA " +
            "2A C0 DE 15 05 D9 A3 BD 8F D2 63 5A D9 43 2D D5 " +
            "94 20 0E 9E CB 1A EB B6 B3 65 6B 87 76 AE 12 EF " +
            "DA 7A E0 13 72 32 1B 64 58 48 0B A6 EE BC F7 3A " +
            "85 2B 69 00 24 D2 71 DB 37 F3 97 22 95 78 81 7C " +
            "A4 69 47 55 34 C2 AB C2 93 81 7E B6 F1 D8 62 61 " +
            "2D 99 73 31 BB F1 1B 5A 8F A0 03 C1 7A A4 FA D8 " +
            "AD 01 88 B6 BD 51 87 DD 2E 7B 94 B5 F6 DE C0 D5 " +
            "0C C8 B8 52 97 F6 F8 74",
            // After Pi
            "A0 3B 9F 73 43 B1 C0 DF 9F CB B0 E2 81 B1 FB 29 " +
            "94 20 0E 9E CB 1A EB B6 A4 69 47 55 34 C2 AB C2 " +
            "0C C8 B8 52 97 F6 F8 74 DC D8 1B 06 7E 19 BD 29 " +
            "3D 3E AE 69 56 16 E6 DA 2A C0 DE 15 05 D9 A3 BD " +
            "85 2B 69 00 24 D2 71 DB AD 01 88 B6 BD 51 87 DD " +
            "21 83 44 D7 71 F0 95 26 50 90 19 F7 81 3E 24 03 " +
            "B3 65 6B 87 76 AE 12 EF 93 81 7E B6 F1 D8 62 61 " +
            "2D 99 73 31 BB F1 1B 5A B1 15 B4 CA 30 7F C9 7A " +
            "43 FE CE 02 58 BA 82 17 8F D2 63 5A D9 43 2D D5 " +
            "37 F3 97 22 95 78 81 7C 2E 7B 94 B5 F6 DE C0 D5 " +
            "4C BB D7 83 D5 40 80 22 1A 25 EC D4 82 34 F2 75 " +
            "DA 7A E0 13 72 32 1B 64 58 48 0B A6 EE BC F7 3A " +
            "8F A0 03 C1 7A A4 FA D8",
            // After Chi
            "A0 1B 91 6F 09 BB C0 49 BF 82 F1 A3 B5 71 FB 69 " +
            "9C A0 B6 9C 48 2E BB 82 04 5A 40 74 74 C3 AB 49 " +
            "13 08 98 D2 17 F6 C3 54 DE 18 4B 12 7F D0 BC 0C " +
            "B8 15 8F 69 76 14 B6 98 02 C0 5E A3 9C D8 25 B9 " +
            "D5 F3 7A 00 66 DA 49 FB 8C 27 2C DF BD 57 C5 0F " +
            "82 E6 26 D7 07 70 87 CA 50 10 0D C7 00 6E 44 03 " +
            "9F 7D 6A 86 7C 8F 0B F5 93 83 7A 70 B1 D8 E6 45 " +
            "7D 89 6A 11 3B FF 3B 5B 3D 15 95 92 B1 3E E4 BA " +
            "73 DF 5A 22 5C 82 02 3F 87 DA 63 CF BB C5 6D 54 " +
            "A6 F7 B7 68 95 59 88 56 6C 91 DE B5 BE 5E C2 D0 " +
            "8C E1 D7 80 A5 42 89 22 1A 25 E7 70 0E B8 16 6F " +
            "5D DA E0 52 62 32 13 A4 18 53 DF A4 6B FC F7 18 " +
            "9D A4 2B 95 78 90 88 8D",
            // After Iota 
            "2A 9B 91 6F 09 BB C0 C9 BF 82 F1 A3 B5 71 FB 69 " +
            "9C A0 B6 9C 48 2E BB 82 04 5A 40 74 74 C3 AB 49 " +
            "13 08 98 D2 17 F6 C3 54 DE 18 4B 12 7F D0 BC 0C " +
            "B8 15 8F 69 76 14 B6 98 02 C0 5E A3 9C D8 25 B9 " +
            "D5 F3 7A 00 66 DA 49 FB 8C 27 2C DF BD 57 C5 0F " +
            "82 E6 26 D7 07 70 87 CA 50 10 0D C7 00 6E 44 03 " +
            "9F 7D 6A 86 7C 8F 0B F5 93 83 7A 70 B1 D8 E6 45 " +
            "7D 89 6A 11 3B FF 3B 5B 3D 15 95 92 B1 3E E4 BA " +
            "73 DF 5A 22 5C 82 02 3F 87 DA 63 CF BB C5 6D 54 " +
            "A6 F7 B7 68 95 59 88 56 6C 91 DE B5 BE 5E C2 D0 " +
            "8C E1 D7 80 A5 42 89 22 1A 25 E7 70 0E B8 16 6F " +
            "5D DA E0 52 62 32 13 A4 18 53 DF A4 6B FC F7 18 " +
            "9D A4 2B 95 78 90 88 8D",
            // Round #3
            // After Theta
            "44 F2 26 EC 7C 48 8D D0 CE 28 4D 53 32 0A BA 83 " +
            "5B C0 29 53 62 D7 50 52 F9 61 16 28 AB 6D AF CD " +
            "60 A5 CD 6B 81 5C 94 C2 B0 71 FC 91 0A 23 F1 15 " +
            "C9 BF 33 99 F1 6F F7 72 C5 A0 C1 6C B6 21 CE 69 " +
            "28 C8 2C 5C B9 74 4D 7F FF 8A 79 66 2B FD 92 99 " +
            "EC 8F 91 54 72 83 CA D3 21 BA B1 37 87 15 05 E9 " +
            "58 1D F5 49 56 76 E0 25 6E B8 2C 2C 6E 76 E2 C1 " +
            "0E 24 3F A8 AD 55 6C CD 53 7C 22 11 C4 CD A9 A3 " +
            "02 75 E6 D2 DB F9 43 D5 40 BA FC 00 91 3C 86 84 " +
            "5B CC E1 34 4A F7 8C D2 1F 3C 8B 0C 28 F4 95 46 " +
            "E2 88 60 03 D0 B1 C4 3B 6B 8F 5B 80 89 C3 57 85 " +
            "9A BA 7F 9D 48 CB F8 74 E5 68 89 F8 B4 52 F3 9C " +
            "EE 09 7E 2C EE 3A DF 1B",
            // After Rho
            "44 F2 26 EC 7C 48 8D D0 9D 51 9A A6 64 14 74 07 " +
            "16 70 CA 94 D8 35 94 D4 DA F6 DA 9C 1F 66 81 B2 " +
            "E4 A2 14 06 2B 6D 5E 0B A9 30 12 5F 01 1B C7 1F " +
            "93 19 FF 76 2F 97 FC 3B 5A 31 68 30 9B 6D 88 73 " +
            "64 16 AE 5C BA A6 3F 14 2F 99 F9 AF 98 67 B6 D2 " +
            "66 7F 8C A4 92 1B 54 9E A4 87 E8 C6 DE 1C 56 14 " +
            "4F B2 B2 03 2F C1 EA A8 EC C4 83 DD 70 59 58 DC " +
            "D4 D6 2A B6 66 07 92 1F 22 88 9B 53 47 A7 F8 44 " +
            "5C 7A 3B 7F A8 5A A0 CE 43 42 20 5D 7E 80 48 1E " +
            "9E 51 7A 8B 39 9C 46 E9 46 1F 3C 8B 0C 28 F4 95 " +
            "12 EF 88 23 82 0D 40 C7 AE 3D 6E 01 26 0E 5F 15 " +
            "53 F7 AF 13 69 19 9F 4E 68 89 F8 B4 52 F3 9C E5 " +
            "F7 86 7B 82 1F 8B BB CE",
            // After Pi
            "44 F2 26 EC 7C 48 8D D0 93 19 FF 76 2F 97 FC 3B " +
            "4F B2 B2 03 2F C1 EA A8 9E 51 7A 8B 39 9C 46 E9 " +
            "F7 86 7B 82 1F 8B BB CE DA F6 DA 9C 1F 66 81 B2 " +
            "2F 99 F9 AF 98 67 B6 D2 66 7F 8C A4 92 1B 54 9E " +
            "5C 7A 3B 7F A8 5A A0 CE 53 F7 AF 13 69 19 9F 4E " +
            "9D 51 9A A6 64 14 74 07 5A 31 68 30 9B 6D 88 73 " +
            "EC C4 83 DD 70 59 58 DC 46 1F 3C 8B 0C 28 F4 95 " +
            "12 EF 88 23 82 0D 40 C7 E4 A2 14 06 2B 6D 5E 0B " +
            "A9 30 12 5F 01 1B C7 1F A4 87 E8 C6 DE 1C 56 14 " +
            "43 42 20 5D 7E 80 48 1E 68 89 F8 B4 52 F3 9C E5 " +
            "16 70 CA 94 D8 35 94 D4 64 16 AE 5C BA A6 3F 14 " +
            "D4 D6 2A B6 66 07 92 1F 22 88 9B 53 47 A7 F8 44 " +
            "AE 3D 6E 01 26 0E 5F 15",
            // After Chi
            "08 50 26 ED 7C 08 8F 50 03 58 B7 FE 3F 8B F8 7A " +
            "2E 34 B3 03 29 C2 53 AE 9E 21 7E E7 59 DC 42 F9 " +
            "64 8F A2 90 1C 1C CB E5 9A 90 DE 9C 1D 7E C1 BE " +
            "37 99 CA F4 B0 27 16 92 65 FA 08 A4 D3 1A 4B 9E " +
            "D4 7A 6B F3 BE 3C A0 7E 76 FE 8E 30 E9 18 A9 0E " +
            "39 95 19 6B 04 04 24 8B 58 2A 54 32 97 4D 2C 72 " +
            "FC 24 03 FD F2 5C 58 9E CB 0F 2E 0F 68 38 C0 95 " +
            "50 CF E8 33 19 64 C8 B7 E0 25 FC 86 F5 69 4E 0B " +
            "EA 70 12 46 21 9B CF 15 8C 0E 30 66 DE 6F C2 F5 " +
            "C7 60 24 5F 57 8C 0A 14 61 99 FA ED 52 E1 1D F1 " +
            "86 B0 CA 36 9C 34 14 DF 46 1E 3F 1D BB 06 57 54 " +
            "58 E3 4E B6 46 0F 95 0E 32 C8 1B C7 9F 96 78 84 " +
            "CE 3B 4A 49 04 8C 74 15",
            // After Iota 
            "08 D0 26 6D 7C 08 8F D0 03 58 B7 FE 3F 8B F8 7A " +
            "2E 34 B3 03 29 C2 53 AE 9E 21 7E E7 59 DC 42 F9 " +
            "64 8F A2 90 1C 1C CB E5 9A 90 DE 9C 1D 7E C1 BE " +
            "37 99 CA F4 B0 27 16 92 65 FA 08 A4 D3 1A 4B 9E " +
            "D4 7A 6B F3 BE 3C A0 7E 76 FE 8E 30 E9 18 A9 0E " +
            "39 95 19 6B 04 04 24 8B 58 2A 54 32 97 4D 2C 72 " +
            "FC 24 03 FD F2 5C 58 9E CB 0F 2E 0F 68 38 C0 95 " +
            "50 CF E8 33 19 64 C8 B7 E0 25 FC 86 F5 69 4E 0B " +
            "EA 70 12 46 21 9B CF 15 8C 0E 30 66 DE 6F C2 F5 " +
            "C7 60 24 5F 57 8C 0A 14 61 99 FA ED 52 E1 1D F1 " +
            "86 B0 CA 36 9C 34 14 DF 46 1E 3F 1D BB 06 57 54 " +
            "58 E3 4E B6 46 0F 95 0E 32 C8 1B C7 9F 96 78 84 " +
            "CE 3B 4A 49 04 8C 74 15",
            // Round #4
            // After Theta
            "64 C7 5B 9C C2 FC F8 DE 08 16 EC C1 12 6D E7 E1 " +
            "07 49 BE 66 24 3A A8 71 26 1F 50 03 BD 23 D3 DD " +
            "8A F2 08 46 43 80 FB 05 F6 87 A3 6D A3 8A B6 B0 " +
            "3C D7 91 CB 9D C1 09 09 4C 87 05 C1 DE E2 B0 41 " +
            "6C 44 45 17 5A C3 31 5A 98 83 24 E6 B6 84 99 EE " +
            "55 82 64 9A BA F0 53 85 53 64 0F 0D BA AB 33 E9 " +
            "D5 59 0E 98 FF A4 A3 41 73 31 00 EB 8C C7 51 B1 " +
            "BE B2 42 E5 46 F8 F8 57 8C 32 81 77 4B 9D 39 05 " +
            "E1 3E 49 79 0C 7D D0 8E A5 73 3D 03 D3 97 39 2A " +
            "7F 5E 0A BB B3 73 9B 30 8F E4 50 3B 0D 7D 2D 11 " +
            "EA A7 B7 C7 22 C0 63 D1 4D 50 64 22 96 E0 48 CF " +
            "71 9E 43 D3 4B F7 6E D1 8A F6 35 23 7B 69 E9 A0 " +
            "20 46 E0 9F 5B 10 44 F5",
            // After Rho
            "64 C7 5B 9C C2 FC F8 DE 11 2C D8 83 25 DA CE C3 " +
            "41 92 AF 19 89 0E 6A DC 3B 32 DD 6D F2 01 35 D0 " +
            "02 DC 2F 50 94 47 30 1A 36 AA 68 0B 6B 7F 38 DA " +
            "B9 DC 19 9C 90 C0 73 1D 10 D3 61 41 B0 B7 38 6C " +
            "A2 A2 0B AD E1 18 2D 36 98 E9 8E 39 48 62 6E 4B " +
            "AC 12 24 D3 D4 85 9F 2A A4 4F 91 3D 34 E8 AE CE " +
            "C0 FC 27 1D 0D AA CE 72 8F A3 62 E7 62 00 D6 19 " +
            "72 23 7C FC 2B 5F 59 A1 EF 96 3A 73 0A 18 65 02 " +
            "29 8F A1 0F DA 31 DC 27 1C 95 D2 B9 9E 81 E9 CB " +
            "6E 13 E6 CF 4B 61 77 76 11 8F E4 50 3B 0D 7D 2D " +
            "8F 45 AB 9F DE 1E 8B 00 37 41 91 89 58 82 23 3D " +
            "CE 73 68 7A E9 DE 2D 3A F6 35 23 7B 69 E9 A0 8A " +
            "51 3D 88 11 F8 E7 16 04",
            // After Pi
            "64 C7 5B 9C C2 FC F8 DE B9 DC 19 9C 90 C0 73 1D " +
            "C0 FC 27 1D 0D AA CE 72 6E 13 E6 CF 4B 61 77 76 " +
            "51 3D 88 11 F8 E7 16 04 3B 32 DD 6D F2 01 35 D0 " +
            "98 E9 8E 39 48 62 6E 4B AC 12 24 D3 D4 85 9F 2A " +
            "29 8F A1 0F DA 31 DC 27 CE 73 68 7A E9 DE 2D 3A " +
            "11 2C D8 83 25 DA CE C3 10 D3 61 41 B0 B7 38 6C " +
            "8F A3 62 E7 62 00 D6 19 11 8F E4 50 3B 0D 7D 2D " +
            "8F 45 AB 9F DE 1E 8B 00 02 DC 2F 50 94 47 30 1A " +
            "36 AA 68 0B 6B 7F 38 DA A4 4F 91 3D 34 E8 AE CE " +
            "1C 95 D2 B9 9E 81 E9 CB F6 35 23 7B 69 E9 A0 8A " +
            "41 92 AF 19 89 0E 6A DC A2 A2 0B AD E1 18 2D 36 " +
            "72 23 7C FC 2B 5F 59 A1 EF 96 3A 73 0A 18 65 02 " +
            "37 41 91 89 58 82 23 3D",
            // After Chi
            "24 E7 7D 9D CF D6 74 BC 97 DF D9 5E D2 81 42 19 " +
            "D1 D0 2F 0D BD 2C CE 72 4A D1 B5 43 49 79 9F AC " +
            "C8 25 88 11 E8 E7 15 05 1F 20 FD AF 66 84 A4 F0 " +
            "99 64 0F 35 42 52 2E 4E 6A 62 6C A3 F5 4B BE 32 " +
            "18 8F 34 0A C8 30 CC E7 4E BA 6A 6A E1 BC 67 31 " +
            "9E 0C DA 25 67 DA 08 D2 00 DF E5 51 A9 BA 11 48 " +
            "01 E3 69 68 A6 12 54 19 01 A7 B4 50 1A CD 39 EE " +
            "8F 96 8A DF 4E 3B BB 2C 82 99 BE 64 80 C7 B6 1E " +
            "2E 3A 2A 8B E1 7E 79 DB 46 6F B0 7F 55 80 AE CE " +
            "1C 5D DE B9 0A 87 F9 DB C2 17 63 70 02 D1 A8 4A " +
            "11 93 DB 49 83 49 3A 5D 2F 36 09 AE E1 18 09 34 " +
            "62 62 FD 74 7B DD 5B 9C AF 04 14 63 8B 14 2D C2 " +
            "95 61 91 2D 38 92 26 1F",
            // After Iota 
            "AF 67 7D 9D CF D6 74 BC 97 DF D9 5E D2 81 42 19 " +
            "D1 D0 2F 0D BD 2C CE 72 4A D1 B5 43 49 79 9F AC " +
            "C8 25 88 11 E8 E7 15 05 1F 20 FD AF 66 84 A4 F0 " +
            "99 64 0F 35 42 52 2E 4E 6A 62 6C A3 F5 4B BE 32 " +
            "18 8F 34 0A C8 30 CC E7 4E BA 6A 6A E1 BC 67 31 " +
            "9E 0C DA 25 67 DA 08 D2 00 DF E5 51 A9 BA 11 48 " +
            "01 E3 69 68 A6 12 54 19 01 A7 B4 50 1A CD 39 EE " +
            "8F 96 8A DF 4E 3B BB 2C 82 99 BE 64 80 C7 B6 1E " +
            "2E 3A 2A 8B E1 7E 79 DB 46 6F B0 7F 55 80 AE CE " +
            "1C 5D DE B9 0A 87 F9 DB C2 17 63 70 02 D1 A8 4A " +
            "11 93 DB 49 83 49 3A 5D 2F 36 09 AE E1 18 09 34 " +
            "62 62 FD 74 7B DD 5B 9C AF 04 14 63 8B 14 2D C2 " +
            "95 61 91 2D 38 92 26 1F",
            // Round #5
            // After Theta
            "EE C8 C7 5A C0 EB 29 11 16 27 28 FE 9E D6 B4 D3 " +
            "1F F9 C0 95 B1 0D BF FB 68 73 E6 7D 72 17 C0 3D " +
            "53 06 09 A6 68 FD 03 03 5E 8F 47 68 69 B9 F9 5D " +
            "18 9C FE 95 0E 05 D8 84 A4 4B 83 3B F9 6A CF BB " +
            "3A 2D 67 34 F3 5E 93 76 D5 99 EB DD 61 A6 71 37 " +
            "DF A3 60 E2 68 E7 55 7F 81 27 14 F1 E5 ED E7 82 " +
            "CF CA 86 F0 AA 33 25 90 23 05 E7 6E 21 A3 66 7F " +
            "14 B5 0B 68 CE 21 AD 2A C3 36 04 A3 8F FA EB B3 " +
            "AF C2 DB 2B AD 29 8F 11 88 46 5F E7 59 A1 DF 47 " +
            "3E FF 8D 87 31 E9 A6 4A 59 34 E2 C7 82 CB BE 4C " +
            "50 3C 61 8E 8C 74 67 F0 AE CE F8 0E AD 4F FF FE " +
            "AC 4B 12 EC 77 FC 2A 15 8D A6 47 5D B0 7A 72 53 " +
            "0E 42 10 9A B8 88 30 19",
            // After Rho
            "EE C8 C7 5A C0 EB 29 11 2D 4E 50 FC 3D AD 69 A7 " +
            "47 3E 70 65 6C C3 EF FE 77 01 DC 83 36 67 DE 27 " +
            "EB 1F 18 98 32 48 30 45 96 96 9B DF E5 F5 78 84 " +
            "5F E9 50 80 4D 88 C1 E9 2E E9 D2 E0 4E BE DA F3 " +
            "96 33 9A 79 AF 49 3B 9D 1A 77 53 9D B9 DE 1D 66 " +
            "FB 1E 05 13 47 3B AF FA 0B 06 9E 50 C4 97 B7 9F " +
            "84 57 9D 29 81 7C 56 36 46 CD FE 46 0A CE DD 42 " +
            "34 E7 90 56 15 8A DA 05 46 1F F5 D7 67 87 6D 08 " +
            "7B A5 35 E5 31 E2 55 78 EF 23 44 A3 AF F3 AC D0 " +
            "DD 54 C9 E7 BF F1 30 26 4C 59 34 E2 C7 82 CB BE " +
            "9D C1 43 F1 84 39 32 D2 BB 3A E3 3B B4 3E FD FB " +
            "75 49 82 FD 8E 5F A5 82 A6 47 5D B0 7A 72 53 8D " +
            "4C 86 83 10 84 26 2E 22",
            // After Pi
            "EE C8 C7 5A C0 EB 29 11 5F E9 50 80 4D 88 C1 E9 " +
            "84 57 9D 29 81 7C 56 36 DD 54 C9 E7 BF F1 30 26 " +
            "4C 86 83 10 84 26 2E 22 77 01 DC 83 36 67 DE 27 " +
            "1A 77 53 9D B9 DE 1D 66 FB 1E 05 13 47 3B AF FA " +
            "7B A5 35 E5 31 E2 55 78 75 49 82 FD 8E 5F A5 82 " +
            "2D 4E 50 FC 3D AD 69 A7 2E E9 D2 E0 4E BE DA F3 " +
            "46 CD FE 46 0A CE DD 42 4C 59 34 E2 C7 82 CB BE " +
            "9D C1 43 F1 84 39 32 D2 EB 1F 18 98 32 48 30 45 " +
            "96 96 9B DF E5 F5 78 84 0B 06 9E 50 C4 97 B7 9F " +
            "EF 23 44 A3 AF F3 AC D0 A6 47 5D B0 7A 72 53 8D " +
            "47 3E 70 65 6C C3 EF FE 96 33 9A 79 AF 49 3B 9D " +
            "34 E7 90 56 15 8A DA 05 46 1F F5 D7 67 87 6D 08 " +
            "BB 3A E3 3B B4 3E FD FB",
            // After Chi
            "6E DE 4A 73 40 9F 3F 07 06 E9 10 46 73 09 E1 E9 " +
            "84 D5 9F 39 81 7A 58 36 7F 1C 8D AD FF 38 31 37 " +
            "5D A7 93 90 89 26 EE CA 96 09 D8 81 70 46 7C BF " +
            "1A D6 63 79 89 1E 4D 66 FF 56 87 0B C9 26 0F 78 " +
            "79 A5 69 E7 01 C2 0F 5D 7D 3F 81 E1 07 C7 A4 C2 " +
            "6D 4A 7C FA 3D ED 6C A7 26 F9 D2 40 8B BE D8 4F " +
            "D7 4D BD 57 0A F7 ED 02 6C 57 24 EE FE 06 82 9B " +
            "9F 60 C1 F1 C6 2B A0 82 E2 1F 1C 98 32 4A B7 5E " +
            "72 B7 DB 7C CE 95 70 C4 0B 42 87 40 94 97 E4 92 " +
            "A6 3B 44 AB AF FB 8C 90 B2 C7 DE F7 BF C7 1B 0D " +
            "67 FA 70 63 7C 41 2F FE D4 2B FF F8 CD 4C 1E 95 " +
            "8D C7 92 7E 85 B2 4A F6 02 1B E5 93 2F 46 6F 0C " +
            "2B 3B 69 23 37 36 ED FA",
            // After Iota 
            "6F DE 4A F3 40 9F 3F 07 06 E9 10 46 73 09 E1 E9 " +
            "84 D5 9F 39 81 7A 58 36 7F 1C 8D AD FF 38 31 37 " +
            "5D A7 93 90 89 26 EE CA 96 09 D8 81 70 46 7C BF " +
            "1A D6 63 79 89 1E 4D 66 FF 56 87 0B C9 26 0F 78 " +
            "79 A5 69 E7 01 C2 0F 5D 7D 3F 81 E1 07 C7 A4 C2 " +
            "6D 4A 7C FA 3D ED 6C A7 26 F9 D2 40 8B BE D8 4F " +
            "D7 4D BD 57 0A F7 ED 02 6C 57 24 EE FE 06 82 9B " +
            "9F 60 C1 F1 C6 2B A0 82 E2 1F 1C 98 32 4A B7 5E " +
            "72 B7 DB 7C CE 95 70 C4 0B 42 87 40 94 97 E4 92 " +
            "A6 3B 44 AB AF FB 8C 90 B2 C7 DE F7 BF C7 1B 0D " +
            "67 FA 70 63 7C 41 2F FE D4 2B FF F8 CD 4C 1E 95 " +
            "8D C7 92 7E 85 B2 4A F6 02 1B E5 93 2F 46 6F 0C " +
            "2B 3B 69 23 37 36 ED FA",
            // Round #6
            // After Theta
            "70 6F 24 50 65 44 17 58 43 07 F2 82 96 2A 7F 06 " +
            "84 12 D9 FA F2 89 FC 7D 19 5F F5 5E 2C C1 1D E5 " +
            "B0 99 F6 EB 8F 19 DF D8 89 B8 B6 22 55 9D 54 E0 " +
            "5F 38 81 BD 6C 3D D3 89 FF 91 C1 C8 BA D5 AB 33 " +
            "1F E6 11 14 D2 3B 23 8F 90 01 E4 9A 01 F8 95 D0 " +
            "72 FB 12 59 18 36 44 F8 63 17 30 84 6E 9D 46 A0 " +
            "D7 8A FB 94 79 04 49 49 0A 14 5C 1D 2D FF AE 49 " +
            "72 5E A4 8A C0 14 91 90 FD AE 72 3B 17 91 9F 01 " +
            "37 59 39 B8 2B B6 EE 2B 0B 85 C1 83 E7 64 40 D9 " +
            "C0 78 3C 58 7C 02 A0 42 5F F9 BB 8C B9 F8 2A 1F " +
            "78 4B 1E C0 59 9A 07 A1 91 C5 1D 3C 28 6F 80 7A " +
            "8D 00 D4 BD F6 41 EE BD 64 58 9D 60 FC BF 43 DE " +
            "C6 05 0C 58 31 09 DC E8",
            // After Rho
            "70 6F 24 50 65 44 17 58 86 0E E4 05 2D 55 FE 0C " +
            "A1 44 B6 BE 7C 22 7F 1F 12 DC 51 9E F1 55 EF C5 " +
            "CC F8 C6 86 CD B4 5F 7F 52 D5 49 05 9E 88 6B 2B " +
            "D8 CB D6 33 9D F8 85 13 CC 7F 64 30 B2 6E F5 EA " +
            "F3 08 0A E9 9D 91 C7 0F 5F 09 0D 19 40 AE 19 80 " +
            "97 DB 97 C8 C2 B0 21 C2 81 8E 5D C0 10 BA 75 1A " +
            "A7 CC 23 48 4A BA 56 DC FE 5D 93 14 28 B8 3A 5A " +
            "45 60 8A 48 48 39 2F 52 76 2E 22 3F 03 FA 5D E5 " +
            "07 77 C5 D6 7D E5 26 2B A0 EC 85 C2 E0 C1 73 32 " +
            "00 54 08 18 8F 07 8B 4F 1F 5F F9 BB 8C B9 F8 2A " +
            "1E 84 E2 2D 79 00 67 69 45 16 77 F0 A0 BC 01 EA " +
            "11 80 BA D7 3E C8 BD B7 58 9D 60 FC BF 43 DE 64 " +
            "37 BA 71 01 03 56 4C 02",
            // After Pi
            "70 6F 24 50 65 44 17 58 D8 CB D6 33 9D F8 85 13 " +
            "A7 CC 23 48 4A BA 56 DC 00 54 08 18 8F 07 8B 4F " +
            "37 BA 71 01 03 56 4C 02 12 DC 51 9E F1 55 EF C5 " +
            "5F 09 0D 19 40 AE 19 80 97 DB 97 C8 C2 B0 21 C2 " +
            "07 77 C5 D6 7D E5 26 2B 11 80 BA D7 3E C8 BD B7 " +
            "86 0E E4 05 2D 55 FE 0C CC 7F 64 30 B2 6E F5 EA " +
            "FE 5D 93 14 28 B8 3A 5A 1F 5F F9 BB 8C B9 F8 2A " +
            "1E 84 E2 2D 79 00 67 69 CC F8 C6 86 CD B4 5F 7F " +
            "52 D5 49 05 9E 88 6B 2B 81 8E 5D C0 10 BA 75 1A " +
            "A0 EC 85 C2 E0 C1 73 32 58 9D 60 FC BF 43 DE 64 " +
            "A1 44 B6 BE 7C 22 7F 1F F3 08 0A E9 9D 91 C7 0F " +
            "45 60 8A 48 48 39 2F 52 76 2E 22 3F 03 FA 5D E5 " +
            "45 16 77 F0 A0 BC 01 EA",
            // After Chi
            "57 6B 05 18 27 46 45 94 D8 DB DE 23 18 FD 0C 10 " +
            "90 66 52 49 4A EA 12 DC 40 11 0C 48 EB 07 98 17 " +
            "BF 3A A3 22 9B EE CC 01 92 0E C3 5E 73 45 CF 87 " +
            "5F 2D 4D 0F 7D EB 1F A9 87 5B AD C9 C0 B8 B8 56 " +
            "05 2B 84 DE BC F0 64 6B 5C 81 B6 D6 3E 62 AD B7 " +
            "B4 0E 77 01 25 C5 F4 1C CD 7D 0C 9B 36 6F 35 CA " +
            "FE DD 91 10 59 B8 3D 1B 9F 55 FD BB 88 EC 60 2E " +
            "56 F5 E2 1D EB 2A 66 8B 4D F2 D2 46 CD 86 4B 6F " +
            "72 B5 C9 07 7E C9 69 0B D9 9F 3D FC 0F B8 F9 5E " +
            "24 8C 03 C0 A0 75 72 29 4A 98 69 FD AD 4B FE 64 " +
            "A5 24 36 BE 3C 0A 57 4F C1 06 2A DE 9E 53 97 AA " +
            "44 70 DF 88 E8 3D 2F 58 D6 6E A2 31 5F F8 23 F0 " +
            "17 1E 7F B1 21 2D 81 EA",
            // After Iota 
            "D6 EB 05 98 27 46 45 14 D8 DB DE 23 18 FD 0C 10 " +
            "90 66 52 49 4A EA 12 DC 40 11 0C 48 EB 07 98 17 " +
            "BF 3A A3 22 9B EE CC 01 92 0E C3 5E 73 45 CF 87 " +
            "5F 2D 4D 0F 7D EB 1F A9 87 5B AD C9 C0 B8 B8 56 " +
            "05 2B 84 DE BC F0 64 6B 5C 81 B6 D6 3E 62 AD B7 " +
            "B4 0E 77 01 25 C5 F4 1C CD 7D 0C 9B 36 6F 35 CA " +
            "FE DD 91 10 59 B8 3D 1B 9F 55 FD BB 88 EC 60 2E " +
            "56 F5 E2 1D EB 2A 66 8B 4D F2 D2 46 CD 86 4B 6F " +
            "72 B5 C9 07 7E C9 69 0B D9 9F 3D FC 0F B8 F9 5E " +
            "24 8C 03 C0 A0 75 72 29 4A 98 69 FD AD 4B FE 64 " +
            "A5 24 36 BE 3C 0A 57 4F C1 06 2A DE 9E 53 97 AA " +
            "44 70 DF 88 E8 3D 2F 58 D6 6E A2 31 5F F8 23 F0 " +
            "17 1E 7F B1 21 2D 81 EA",
            // Round #7
            // After Theta
            "CD 52 1C E1 83 41 8C 02 29 F8 93 D5 F1 69 EC 91 " +
            "38 44 87 9E B8 25 51 19 E5 8F 43 E7 5A E9 28 E6 " +
            "A6 CD DD 80 BB ED C5 D4 89 B7 DA 27 D7 42 06 91 " +
            "AE 0E 00 F9 94 7F FF 28 2F 79 78 1E 32 77 FB 93 " +
            "A0 B5 CB 71 0D 1E D4 9A 45 76 C8 74 1E 61 A4 62 " +
            "AF B7 6E 78 81 C2 3D 0A 3C 5E 41 6D DF FB D5 4B " +
            "56 FF 44 C7 AB 77 7E DE 3A CB B2 14 39 02 D0 DF " +
            "4F 02 9C BF CB 29 6F 5E 56 4B CB 3F 69 81 82 79 " +
            "83 96 84 F1 97 5D 89 8A 71 BD E8 2B FD 77 BA 9B " +
            "81 12 4C 6F 11 9B C2 D8 53 6F 17 5F 8D 48 F7 B1 " +
            "BE 9D 2F C7 98 0D 9E 59 30 25 67 28 77 C7 77 2B " +
            "EC 52 0A 5F 1A F2 6C 9D 73 F0 ED 9E EE 16 93 01 " +
            "0E E9 01 13 01 2E 88 3F",
            // After Rho
            "CD 52 1C E1 83 41 8C 02 53 F0 27 AB E3 D3 D8 23 " +
            "0E D1 A1 27 6E 49 54 06 95 8E 62 5E FE 38 74 AE " +
            "6D 2F A6 36 6D EE 06 DC 72 2D 64 10 99 78 AB 7D " +
            "90 4F F9 F7 8F E2 EA 00 E4 4B 1E 9E 87 CC DD FE " +
            "DA E5 B8 06 0F 6A 4D D0 46 2A 56 64 87 4C E7 11 " +
            "78 BD 75 C3 0B 14 EE 51 2F F1 78 05 B5 7D EF 57 " +
            "3A 5E BD F3 F3 B6 FA 27 04 A0 BF 75 96 65 29 72 " +
            "DF E5 94 37 AF 27 01 CE 7F D2 02 05 F3 AC 96 96 " +
            "30 FE B2 2B 51 71 D0 92 DD CD B8 5E F4 95 FE 3B " +
            "53 18 3B 50 82 E9 2D 62 B1 53 6F 17 5F 8D 48 F7 " +
            "78 66 F9 76 BE 1C 63 36 C0 94 9C A1 DC 1D DF AD " +
            "5D 4A E1 4B 43 9E AD 93 F0 ED 9E EE 16 93 01 73 " +
            "E2 8F 43 7A C0 44 80 0B",
            // After Pi
            "CD 52 1C E1 83 41 8C 02 90 4F F9 F7 8F E2 EA 00 " +
            "3A 5E BD F3 F3 B6 FA 27 53 18 3B 50 82 E9 2D 62 " +
            "E2 8F 43 7A C0 44 80 0B 95 8E 62 5E FE 38 74 AE " +
            "46 2A 56 64 87 4C E7 11 78 BD 75 C3 0B 14 EE 51 " +
            "30 FE B2 2B 51 71 D0 92 5D 4A E1 4B 43 9E AD 93 " +
            "53 F0 27 AB E3 D3 D8 23 E4 4B 1E 9E 87 CC DD FE " +
            "04 A0 BF 75 96 65 29 72 B1 53 6F 17 5F 8D 48 F7 " +
            "78 66 F9 76 BE 1C 63 36 6D 2F A6 36 6D EE 06 DC " +
            "72 2D 64 10 99 78 AB 7D 2F F1 78 05 B5 7D EF 57 " +
            "DD CD B8 5E F4 95 FE 3B F0 ED 9E EE 16 93 01 73 " +
            "0E D1 A1 27 6E 49 54 06 DA E5 B8 06 0F 6A 4D D0 " +
            "DF E5 94 37 AF 27 01 CE 7F D2 02 05 F3 AC 96 96 " +
            "C0 94 9C A1 DC 1D DF AD",
            // After Chi
            "E7 42 18 E1 F3 55 9C 25 D1 4F FB F7 8F AB EF 40 " +
            "9A D9 FD D9 B3 B2 7A 2E 5E 48 27 D1 81 E8 21 62 " +
            "F2 82 A2 6C CC E6 E2 0B AD 1B 43 DD F6 28 7C EE " +
            "46 68 D4 4C D7 2D F7 93 35 BD 34 83 09 9A C3 50 " +
            "B0 7A B0 3F ED 51 80 BE 1F 6A F5 6B 42 DA 2E 82 " +
            "53 50 86 CA F3 F2 F8 23 55 18 5E 9C CE 44 9D 7B " +
            "4C 84 2F 15 36 75 0A 72 B2 C3 69 9E 1E 4E D0 F6 " +
            "DC 6D E1 62 BA 10 66 EA 60 FF BE 33 49 EB 42 DE " +
            "A2 21 E4 4A D9 F8 BB 55 0F D1 7E A5 B7 7F EE 17 " +
            "D0 CF 98 4E 9D F9 F8 B7 E2 ED DE EE 86 83 A8 52 " +
            "0B D1 A5 16 CE 4C 54 08 FA F7 BA 06 5F E2 DB C0 " +
            "5F E1 08 97 A3 36 48 E7 71 93 23 03 D1 EC 96 94 " +
            "10 B0 84 A1 DD 3F D6 7D",
            // After Iota 
            "EE C2 18 E1 F3 55 9C A5 D1 4F FB F7 8F AB EF 40 " +
            "9A D9 FD D9 B3 B2 7A 2E 5E 48 27 D1 81 E8 21 62 " +
            "F2 82 A2 6C CC E6 E2 0B AD 1B 43 DD F6 28 7C EE " +
            "46 68 D4 4C D7 2D F7 93 35 BD 34 83 09 9A C3 50 " +
            "B0 7A B0 3F ED 51 80 BE 1F 6A F5 6B 42 DA 2E 82 " +
            "53 50 86 CA F3 F2 F8 23 55 18 5E 9C CE 44 9D 7B " +
            "4C 84 2F 15 36 75 0A 72 B2 C3 69 9E 1E 4E D0 F6 " +
            "DC 6D E1 62 BA 10 66 EA 60 FF BE 33 49 EB 42 DE " +
            "A2 21 E4 4A D9 F8 BB 55 0F D1 7E A5 B7 7F EE 17 " +
            "D0 CF 98 4E 9D F9 F8 B7 E2 ED DE EE 86 83 A8 52 " +
            "0B D1 A5 16 CE 4C 54 08 FA F7 BA 06 5F E2 DB C0 " +
            "5F E1 08 97 A3 36 48 E7 71 93 23 03 D1 EC 96 94 " +
            "10 B0 84 A1 DD 3F D6 7D",
            // Round #8
            // After Theta
            "19 C9 AB 1D BC 75 83 92 CD 49 1C DF CE AA CB 06 " +
            "FA 6B 59 C8 DF AE A0 01 6B 29 6E F9 C7 DC 9D 07 " +
            "F8 61 6A F6 11 54 E1 7E 5A 10 F0 21 B9 08 63 D9 " +
            "5A 6E 33 64 96 2C D3 D5 55 0F 90 92 65 86 19 7F " +
            "85 1B F9 17 AB 65 3C DB 15 89 3D F1 9F 68 2D F7 " +
            "A4 5B 35 36 BC D2 E7 14 49 1E B9 B4 8F 45 B9 3D " +
            "2C 36 8B 04 5A 69 D0 5D 87 A2 20 B6 58 7A 6C 93 " +
            "D6 8E 29 F8 67 A2 65 9F 97 F4 0D CF 06 CB 5D E9 " +
            "BE 27 03 62 98 F9 9F 13 6F 63 DA B4 DB 63 34 38 " +
            "E5 AE D1 66 DB CD 44 D2 E8 0E 16 74 5B 31 AB 27 " +
            "FC DA 16 EA 81 6C 4B 3F E6 F1 5D 2E 1E E3 FF 86 " +
            "3F 53 AC 86 CF 2A 92 C8 44 F2 6A 2B 97 D8 2A F1 " +
            "1A 53 4C 3B 00 8D D5 08",
            // After Rho
            "19 C9 AB 1D BC 75 83 92 9A 93 38 BE 9D 55 97 0D " +
            "FE 5A 16 F2 B7 2B 68 80 CC DD 79 B0 96 E2 96 7F " +
            "A0 0A F7 C3 0F 53 B3 8F 92 8B 30 96 AD 05 01 1F " +
            "43 66 C9 32 5D AD E5 36 5F D5 03 A4 64 99 61 C6 " +
            "8D FC 8B D5 32 9E ED C2 D6 72 5F 91 D8 13 FF 89 " +
            "20 DD AA B1 E1 95 3E A7 F6 24 79 E4 D2 3E 16 E5 " +
            "24 D0 4A 83 EE 62 B1 59 F4 D8 26 0F 45 41 6C B1 " +
            "FC 33 D1 B2 4F 6B C7 14 9E 0D 96 BB D2 2F E9 1B " +
            "40 0C 33 FF 73 C2 F7 64 1A 9C B7 31 6D DA ED 31 " +
            "99 48 BA DC 35 DA 6C BB 27 E8 0E 16 74 5B 31 AB " +
            "2D FD F0 6B 5B A8 07 B2 9A C7 77 B9 78 8C FF 1B " +
            "67 8A D5 F0 59 45 12 F9 F2 6A 2B 97 D8 2A F1 44 " +
            "35 82 C6 14 D3 0E 40 63",
            // After Pi
            "19 C9 AB 1D BC 75 83 92 43 66 C9 32 5D AD E5 36 " +
            "24 D0 4A 83 EE 62 B1 59 99 48 BA DC 35 DA 6C BB " +
            "35 82 C6 14 D3 0E 40 63 CC DD 79 B0 96 E2 96 7F " +
            "D6 72 5F 91 D8 13 FF 89 20 DD AA B1 E1 95 3E A7 " +
            "40 0C 33 FF 73 C2 F7 64 67 8A D5 F0 59 45 12 F9 " +
            "9A 93 38 BE 9D 55 97 0D 5F D5 03 A4 64 99 61 C6 " +
            "F4 D8 26 0F 45 41 6C B1 27 E8 0E 16 74 5B 31 AB " +
            "2D FD F0 6B 5B A8 07 B2 A0 0A F7 C3 0F 53 B3 8F " +
            "92 8B 30 96 AD 05 01 1F F6 24 79 E4 D2 3E 16 E5 " +
            "1A 9C B7 31 6D DA ED 31 F2 6A 2B 97 D8 2A F1 44 " +
            "FE 5A 16 F2 B7 2B 68 80 8D FC 8B D5 32 9E ED C2 " +
            "FC 33 D1 B2 4F 6B C7 14 9E 0D 96 BB D2 2F E9 1B " +
            "9A C7 77 B9 78 8C FF 1B",
            // After Chi
            "3D 59 A9 9C 1E 37 93 DB DA 6E 79 6E 4C 35 A9 94 " +
            "00 52 0E 83 2C 66 B1 19 91 01 93 D5 19 AB EF 2B " +
            "77 A4 86 36 92 86 24 47 EC 50 D9 90 B7 66 96 59 " +
            "96 72 4E DF CA 51 3E C9 07 5F 6E B1 E9 90 3E 3E " +
            "C8 59 1B FF F5 60 73 62 75 A8 D3 F1 11 54 7B 79 " +
            "3A 9B 1C B5 9C 15 9B 3C 5C F5 0B B4 54 83 70 CC " +
            "FC CD D6 66 4E E1 6A A1 B5 EA 06 82 F0 0E A1 A6 " +
            "68 B9 F3 6B 3B 20 67 70 C4 2E BE A3 5D 69 A5 6F " +
            "9A 13 B6 87 80 C5 E8 0F 16 46 71 62 42 1E 06 A1 " +
            "1A 9C 63 71 6A 8B EF BA E0 EB 2B 83 78 2E F1 54 " +
            "8E 59 46 D0 FA 4A 6A 94 8F F0 8D DC A2 9A C5 C9 " +
            "FC F1 B0 B2 67 EB D1 14 FA 15 96 F9 55 0C E9 9B " +
            "9B 63 FE BC 78 18 7A 59",
            // After Iota 
            "B7 59 A9 9C 1E 37 93 DB DA 6E 79 6E 4C 35 A9 94 " +
            "00 52 0E 83 2C 66 B1 19 91 01 93 D5 19 AB EF 2B " +
            "77 A4 86 36 92 86 24 47 EC 50 D9 90 B7 66 96 59 " +
            "96 72 4E DF CA 51 3E C9 07 5F 6E B1 E9 90 3E 3E " +
            "C8 59 1B FF F5 60 73 62 75 A8 D3 F1 11 54 7B 79 " +
            "3A 9B 1C B5 9C 15 9B 3C 5C F5 0B B4 54 83 70 CC " +
            "FC CD D6 66 4E E1 6A A1 B5 EA 06 82 F0 0E A1 A6 " +
            "68 B9 F3 6B 3B 20 67 70 C4 2E BE A3 5D 69 A5 6F " +
            "9A 13 B6 87 80 C5 E8 0F 16 46 71 62 42 1E 06 A1 " +
            "1A 9C 63 71 6A 8B EF BA E0 EB 2B 83 78 2E F1 54 " +
            "8E 59 46 D0 FA 4A 6A 94 8F F0 8D DC A2 9A C5 C9 " +
            "FC F1 B0 B2 67 EB D1 14 FA 15 96 F9 55 0C E9 9B " +
            "9B 63 FE BC 78 18 7A 59",
            // Round #9
            // After Theta
            "AC 70 D4 B3 46 82 B5 37 D3 65 03 AC 83 97 9D B7 " +
            "1C 2E FF 9D 9A 5A 0D D2 A2 0C 02 77 C6 C0 BA 9F " +
            "2D 55 D4 83 94 0B BD 03 F7 79 A4 BF EF D3 B0 B5 " +
            "9F 79 34 1D 05 F3 0A EA 1B 23 9F AF 5F AC 82 F5 " +
            "FB 54 8A 5D 2A 0B 26 D6 2F 59 81 44 17 D9 E2 3D " +
            "21 B2 61 9A C4 A0 BD D0 55 FE 71 76 9B 21 44 EF " +
            "E0 B1 27 78 F8 DD D6 6A 86 E7 97 20 2F 65 F4 12 " +
            "32 48 A1 DE 3D AD FE 34 DF 07 C3 8C 05 DC 83 83 " +
            "93 18 CC 45 4F 67 DC 2C 0A 3A 80 7C F4 22 BA 6A " +
            "29 91 F2 D3 B5 E0 BA 0E BA 1A 79 36 7E A3 68 10 " +
            "95 70 3B FF A2 FF 4C 78 86 FB F7 1E 6D 38 F1 EA " +
            "E0 8D 41 AC D1 D7 6D DF C9 18 07 5B 8A 67 BC 2F " +
            "C1 92 AC 09 7E 95 E3 1D",
            // After Rho
            "AC 70 D4 B3 46 82 B5 37 A7 CB 06 58 07 2F 3B 6F " +
            "87 CB 7F A7 A6 56 83 34 0C AC FB 29 CA 20 70 67 " +
            "5C E8 1D 68 A9 A2 1E A4 FB 3E 0D 5B 7B 9F 47 FA " +
            "D3 51 30 AF A0 FE 99 47 FD C6 C8 E7 EB 17 AB 60 " +
            "2A C5 2E 95 05 13 EB 7D 2D DE F3 92 15 48 74 91 " +
            "0E 91 0D D3 24 06 ED 85 BD 57 F9 C7 D9 6D 86 10 " +
            "C1 C3 EF B6 56 03 8F 3D CA E8 25 0C CF 2F 41 5E " +
            "EF 9E 56 7F 1A 19 A4 50 19 0B B8 07 07 BF 0F 86 " +
            "B9 E8 E9 8C 9B 65 12 83 5D 35 05 1D 40 3E 7A 11 " +
            "5C D7 21 25 52 7E BA 16 10 BA 1A 79 36 7E A3 68 " +
            "33 E1 55 C2 ED FC 8B FE 1B EE DF 7B B4 E1 C4 AB " +
            "BC 31 88 35 FA BA ED 1B 18 07 5B 8A 67 BC 2F C9 " +
            "78 47 B0 24 6B 82 5F E5",
            // After Pi
            "AC 70 D4 B3 46 82 B5 37 D3 51 30 AF A0 FE 99 47 " +
            "C1 C3 EF B6 56 03 8F 3D 5C D7 21 25 52 7E BA 16 " +
            "78 47 B0 24 6B 82 5F E5 0C AC FB 29 CA 20 70 67 " +
            "2D DE F3 92 15 48 74 91 0E 91 0D D3 24 06 ED 85 " +
            "B9 E8 E9 8C 9B 65 12 83 BC 31 88 35 FA BA ED 1B " +
            "A7 CB 06 58 07 2F 3B 6F FD C6 C8 E7 EB 17 AB 60 " +
            "CA E8 25 0C CF 2F 41 5E 10 BA 1A 79 36 7E A3 68 " +
            "33 E1 55 C2 ED FC 8B FE 5C E8 1D 68 A9 A2 1E A4 " +
            "FB 3E 0D 5B 7B 9F 47 FA BD 57 F9 C7 D9 6D 86 10 " +
            "5D 35 05 1D 40 3E 7A 11 18 07 5B 8A 67 BC 2F C9 " +
            "87 CB 7F A7 A6 56 83 34 2A C5 2E 95 05 13 EB 7D " +
            "EF 9E 56 7F 1A 19 A4 50 19 0B B8 07 07 BF 0F 86 " +
            "1B EE DF 7B B4 E1 C4 AB",
            // After Chi
            "AC F2 1B A3 10 83 B3 0F CF 45 30 AE A0 82 A9 45 " +
            "E1 C3 7F B6 7F 83 CA DC D8 E7 65 B6 56 7E 1A 04 " +
            "2B 46 90 28 CB FE 57 A5 0E AD F7 68 EA 26 F9 63 " +
            "9C B6 13 9E 8E 29 66 93 0A 80 0D E2 44 9C 00 9D " +
            "B9 64 9A 84 9B 65 02 E7 9D 63 88 A7 EF F2 E9 8B " +
            "A5 E3 23 50 03 07 7B 71 ED D4 D2 96 DB 47 09 40 " +
            "E9 A9 60 8E 06 AF 49 C8 94 B0 18 61 34 7D 93 69 " +
            "6B E5 9D 65 05 EC 0B FE 58 A9 ED EC 29 C2 9E A4 " +
            "BB 1E 09 43 7B 8D 3F FB BD 55 A3 45 FE ED 83 D8 " +
            "19 DD 01 7D C8 3C 6A 35 BB 11 5B 99 35 A1 6E 93 " +
            "42 D1 2F CD BC 5E 87 34 3A C4 86 95 00 B5 E0 FB " +
            "ED 7A 11 07 AA 59 64 79 9D 0A 98 83 05 A9 0C 92 " +
            "33 EA DF 6B B5 E0 AC E2",
            // After Iota 
            "24 F2 1B A3 10 83 B3 0F CF 45 30 AE A0 82 A9 45 " +
            "E1 C3 7F B6 7F 83 CA DC D8 E7 65 B6 56 7E 1A 04 " +
            "2B 46 90 28 CB FE 57 A5 0E AD F7 68 EA 26 F9 63 " +
            "9C B6 13 9E 8E 29 66 93 0A 80 0D E2 44 9C 00 9D " +
            "B9 64 9A 84 9B 65 02 E7 9D 63 88 A7 EF F2 E9 8B " +
            "A5 E3 23 50 03 07 7B 71 ED D4 D2 96 DB 47 09 40 " +
            "E9 A9 60 8E 06 AF 49 C8 94 B0 18 61 34 7D 93 69 " +
            "6B E5 9D 65 05 EC 0B FE 58 A9 ED EC 29 C2 9E A4 " +
            "BB 1E 09 43 7B 8D 3F FB BD 55 A3 45 FE ED 83 D8 " +
            "19 DD 01 7D C8 3C 6A 35 BB 11 5B 99 35 A1 6E 93 " +
            "42 D1 2F CD BC 5E 87 34 3A C4 86 95 00 B5 E0 FB " +
            "ED 7A 11 07 AA 59 64 79 9D 0A 98 83 05 A9 0C 92 " +
            "33 EA DF 6B B5 E0 AC E2",
            // Round #10
            // After Theta
            "0E 33 E7 5B AD 8B F7 82 FE 0B 7C 25 1F B4 49 98 " +
            "3C F6 FC 9C 98 B1 08 11 21 54 C7 1E 7D 39 91 6E " +
            "71 2B F5 F1 26 71 EA 92 24 6C 0B 90 57 2E BD EE " +
            "AD F8 5F 15 31 1F 86 4E D7 B5 8E C8 A3 AE C2 50 " +
            "40 D7 38 2C B0 22 89 8D C7 0E ED 7E 02 7D 54 BC " +
            "8F 22 DF A8 BE 0F 3F FC DC 9A 9E 1D 64 71 E9 9D " +
            "34 9C E3 A4 E1 9D 8B 05 6D 03 BA C9 1F 3A 18 03 " +
            "31 88 F8 BC E8 63 B6 C9 72 68 11 14 94 CA DA 29 " +
            "8A 50 45 C8 C4 BB DF 26 60 60 20 6F 19 DF 41 15 " +
            "E0 6E A3 D5 E3 7B E1 5F E1 7C 3E 40 D8 2E D3 A4 " +
            "68 10 D3 35 01 56 C3 B9 0B 8A CA 1E BF 83 00 26 " +
            "30 4F 92 2D 4D 6B A6 B4 64 B9 3A 2B 2E EE 87 F8 " +
            "69 87 BA B2 58 6F 11 D5",
            // After Rho
            "0E 33 E7 5B AD 8B F7 82 FD 17 F8 4A 3E 68 93 30 " +
            "8F 3D 3F 27 66 2C 42 04 97 13 E9 16 42 75 EC D1 " +
            "89 53 97 8C 5B A9 8F 37 79 E5 D2 EB 4E C2 B6 00 " +
            "55 11 F3 61 E8 D4 8A FF D4 75 AD 23 F2 A8 AB 30 " +
            "6B 1C 16 58 91 C4 46 A0 47 C5 7B EC D0 EE 27 D0 " +
            "7F 14 F9 46 F5 7D F8 E1 77 72 6B 7A 76 90 C5 A5 " +
            "27 0D EF 5C 2C A0 E1 1C 74 30 06 DA 06 74 93 3F " +
            "5E F4 31 DB E4 18 44 7C 28 28 95 B5 53 E4 D0 22 " +
            "08 99 78 F7 DB 44 11 AA A0 0A 30 30 90 B7 8C EF " +
            "2F FC 0B DC 6D B4 7A 7C A4 E1 7C 3E 40 D8 2E D3 " +
            "0D E7 A2 41 4C D7 04 58 2C 28 2A 7B FC 0E 02 98 " +
            "E6 49 B2 A5 69 CD 94 16 B9 3A 2B 2E EE 87 F8 64 " +
            "44 75 DA A1 AE 2C D6 5B",
            // After Pi
            "0E 33 E7 5B AD 8B F7 82 55 11 F3 61 E8 D4 8A FF " +
            "27 0D EF 5C 2C A0 E1 1C 2F FC 0B DC 6D B4 7A 7C " +
            "44 75 DA A1 AE 2C D6 5B 97 13 E9 16 42 75 EC D1 " +
            "47 C5 7B EC D0 EE 27 D0 7F 14 F9 46 F5 7D F8 E1 " +
            "08 99 78 F7 DB 44 11 AA E6 49 B2 A5 69 CD 94 16 " +
            "FD 17 F8 4A 3E 68 93 30 D4 75 AD 23 F2 A8 AB 30 " +
            "74 30 06 DA 06 74 93 3F A4 E1 7C 3E 40 D8 2E D3 " +
            "0D E7 A2 41 4C D7 04 58 89 53 97 8C 5B A9 8F 37 " +
            "79 E5 D2 EB 4E C2 B6 00 77 72 6B 7A 76 90 C5 A5 " +
            "A0 0A 30 30 90 B7 8C EF B9 3A 2B 2E EE 87 F8 64 " +
            "8F 3D 3F 27 66 2C 42 04 6B 1C 16 58 91 C4 46 A0 " +
            "5E F4 31 DB E4 18 44 7C 28 28 95 B5 53 E4 D0 22 " +
            "2C 28 2A 7B FC 0E 02 98",
            // After Chi
            "2C 3F EB 47 A9 AB 96 82 5D E1 F3 E1 A9 C0 90 9F " +
            "67 0C 3F 7D AE A8 65 1F 25 FE 2E 86 6C 37 5B FC " +
            "15 75 CA 81 EE 78 DE 26 AF 03 69 14 67 64 34 F0 " +
            "47 4C 7B 5D DA EE 26 DA 99 54 7B 46 D5 F4 7C F5 " +
            "19 8B 31 E5 D9 74 79 6B A6 8D A0 4D F9 47 97 16 " +
            "DD 17 FA 92 3A 3C 83 3F 54 B4 D5 07 B2 20 87 F0 " +
            "7D 36 84 9B 0A 73 93 37 54 F1 24 34 72 F0 BD F3 " +
            "0D 87 A7 60 8C 57 2C 58 8F 41 BE 9C 6B B9 CE 92 " +
            "F9 ED C2 EB CE E5 BE 4A 6E 42 60 74 18 90 B5 A5 " +
            "A0 4B A4 B0 81 9F 8B FC C9 9E 6B 4D EA C5 C8 64 " +
            "9B DD 1E A4 02 34 42 58 4B 14 92 7C 82 20 D6 A2 " +
            "5A F4 1B 91 48 12 46 E4 AB 3D 80 B1 51 C4 90 26 " +
            "4C 28 2A 23 6D CE 06 38",
            // After Iota 
            "25 BF EB C7 A9 AB 96 82 5D E1 F3 E1 A9 C0 90 9F " +
            "67 0C 3F 7D AE A8 65 1F 25 FE 2E 86 6C 37 5B FC " +
            "15 75 CA 81 EE 78 DE 26 AF 03 69 14 67 64 34 F0 " +
            "47 4C 7B 5D DA EE 26 DA 99 54 7B 46 D5 F4 7C F5 " +
            "19 8B 31 E5 D9 74 79 6B A6 8D A0 4D F9 47 97 16 " +
            "DD 17 FA 92 3A 3C 83 3F 54 B4 D5 07 B2 20 87 F0 " +
            "7D 36 84 9B 0A 73 93 37 54 F1 24 34 72 F0 BD F3 " +
            "0D 87 A7 60 8C 57 2C 58 8F 41 BE 9C 6B B9 CE 92 " +
            "F9 ED C2 EB CE E5 BE 4A 6E 42 60 74 18 90 B5 A5 " +
            "A0 4B A4 B0 81 9F 8B FC C9 9E 6B 4D EA C5 C8 64 " +
            "9B DD 1E A4 02 34 42 58 4B 14 92 7C 82 20 D6 A2 " +
            "5A F4 1B 91 48 12 46 E4 AB 3D 80 B1 51 C4 90 26 " +
            "4C 28 2A 23 6D CE 06 38",
            // Round #11
            // After Theta
            "E6 B7 7C 5D AF 5F 8E 0C 71 67 5C 13 76 E4 CE 20 " +
            "5C 08 0D FD 0D B3 35 3F E4 B4 8C 46 74 5C 74 09 " +
            "F1 E9 65 24 C3 6D 00 97 6C 0B FE 8E 61 90 2C 7E " +
            "6B CA D4 AF 05 CA 78 65 A2 50 49 C6 76 EF 2C D5 " +
            "D8 C1 93 25 C1 1F 56 9E 42 11 0F E8 D4 52 49 A7 " +
            "1E 1F 6D 08 3C C8 9B B1 78 32 7A F5 6D 04 D9 4F " +
            "46 32 B6 1B A9 68 C3 17 95 BB 86 F4 6A 9B 92 06 " +
            "E9 1B 08 C5 A1 42 F2 E9 4C 49 29 06 6D 4D D6 1C " +
            "D5 6B 6D 19 11 C1 E0 F5 55 46 52 F4 BB 8B E5 85 " +
            "61 01 06 70 99 F4 A4 09 2D 02 C4 E8 C7 D0 16 D5 " +
            "58 D5 89 3E 04 C0 5A D6 67 92 3D 8E 5D 04 88 1D " +
            "61 F0 29 11 EB 09 16 C4 6A 77 22 71 49 AF BF D3 " +
            "A8 B4 85 86 40 DB D8 89",
            // After Rho
            "E6 B7 7C 5D AF 5F 8E 0C E2 CE B8 26 EC C8 9D 41 " +
            "17 42 43 7F C3 6C CD 0F C7 45 97 40 4E CB 68 44 " +
            "6E 03 B8 8C 4F 2F 23 19 18 06 C9 E2 C7 B6 E0 EF " +
            "FD 5A A0 8C 57 B6 A6 4C B5 28 54 92 B1 DD 3B 4B " +
            "E0 C9 92 E0 0F 2B 4F EC 95 74 2A 14 F1 80 4E 2D " +
            "F5 F8 68 43 E0 41 DE 8C 3F E1 C9 E8 D5 B7 11 64 " +
            "DD 48 45 1B BE 30 92 B1 36 25 0D 2A 77 0D E9 D5 " +
            "E2 50 21 F9 F4 F4 0D 84 0C DA 9A AC 39 98 92 52 " +
            "2D 23 22 18 BC BE 7A AD F2 C2 2A 23 29 FA DD C5 " +
            "9E 34 21 2C C0 00 2E 93 D5 2D 02 C4 E8 C7 D0 16 " +
            "6B 59 63 55 27 FA 10 00 9C 49 F6 38 76 11 20 76 " +
            "0C 3E 25 62 3D C1 82 38 77 22 71 49 AF BF D3 6A " +
            "76 22 2A 6D A1 21 D0 36",
            // After Pi
            "E6 B7 7C 5D AF 5F 8E 0C FD 5A A0 8C 57 B6 A6 4C " +
            "DD 48 45 1B BE 30 92 B1 9E 34 21 2C C0 00 2E 93 " +
            "76 22 2A 6D A1 21 D0 36 C7 45 97 40 4E CB 68 44 " +
            "95 74 2A 14 F1 80 4E 2D F5 F8 68 43 E0 41 DE 8C " +
            "2D 23 22 18 BC BE 7A AD 0C 3E 25 62 3D C1 82 38 " +
            "E2 CE B8 26 EC C8 9D 41 B5 28 54 92 B1 DD 3B 4B " +
            "36 25 0D 2A 77 0D E9 D5 D5 2D 02 C4 E8 C7 D0 16 " +
            "6B 59 63 55 27 FA 10 00 6E 03 B8 8C 4F 2F 23 19 " +
            "18 06 C9 E2 C7 B6 E0 EF 3F E1 C9 E8 D5 B7 11 64 " +
            "F2 C2 2A 23 29 FA DD C5 77 22 71 49 AF BF D3 6A " +
            "17 42 43 7F C3 6C CD 0F E0 C9 92 E0 0F 2B 4F EC " +
            "E2 50 21 F9 F4 F4 0D 84 0C DA 9A AC 39 98 92 52 " +
            "9C 49 F6 38 76 11 20 76",
            // After Chi
            "E6 B7 39 4E 07 5F 9E BD FF 6E 80 A8 17 B6 8A 4E " +
            "BD 4A 4F 5A 9F 11 42 95 1E A1 75 3C CE 5E 20 9B " +
            "6F 6A AA ED F1 81 F0 76 A7 CD D7 03 4E 8A F8 C4 " +
            "9D 77 28 0C ED 3E 6E 0C F5 E4 6D 21 E1 00 5E 9C " +
            "EE 62 B0 18 FE B4 12 E9 1C 0E 0D 76 8C C1 84 11 " +
            "E0 CB B1 0E AA C8 5D D5 74 20 56 56 39 1F 2B 49 " +
            "1C 75 6C 3B 70 35 E9 D5 55 AB 9A E6 20 C7 5D 57 " +
            "7E 79 27 C5 36 EF 32 0A 49 E2 B8 84 5F 2E 32 19 " +
            "D8 04 EB E1 EF FE 2C 6E 3A C1 98 A0 53 B2 13 4E " +
            "FA C3 A2 A7 69 FA FD D4 67 26 30 2B 2F 2F 13 8C " +
            "15 52 62 66 33 B8 CD 0F EC 43 08 E4 06 23 DD BE " +
            "72 51 45 E9 B2 F5 2D A0 0F D8 9B EB B8 F4 5F 5B " +
            "7C C0 66 B8 7A 12 22 96",
            // After Iota 
            "EC B7 39 CE 07 5F 9E BD FF 6E 80 A8 17 B6 8A 4E " +
            "BD 4A 4F 5A 9F 11 42 95 1E A1 75 3C CE 5E 20 9B " +
            "6F 6A AA ED F1 81 F0 76 A7 CD D7 03 4E 8A F8 C4 " +
            "9D 77 28 0C ED 3E 6E 0C F5 E4 6D 21 E1 00 5E 9C " +
            "EE 62 B0 18 FE B4 12 E9 1C 0E 0D 76 8C C1 84 11 " +
            "E0 CB B1 0E AA C8 5D D5 74 20 56 56 39 1F 2B 49 " +
            "1C 75 6C 3B 70 35 E9 D5 55 AB 9A E6 20 C7 5D 57 " +
            "7E 79 27 C5 36 EF 32 0A 49 E2 B8 84 5F 2E 32 19 " +
            "D8 04 EB E1 EF FE 2C 6E 3A C1 98 A0 53 B2 13 4E " +
            "FA C3 A2 A7 69 FA FD D4 67 26 30 2B 2F 2F 13 8C " +
            "15 52 62 66 33 B8 CD 0F EC 43 08 E4 06 23 DD BE " +
            "72 51 45 E9 B2 F5 2D A0 0F D8 9B EB B8 F4 5F 5B " +
            "7C C0 66 B8 7A 12 22 96",
            // Round #12
            // After Theta
            "BF B0 D5 ED 4C 59 95 7C 30 F9 23 9A 46 FA D8 91 " +
            "3E D2 9E B1 36 1C E6 1B 2E 1C 4B AE 1C 19 04 47 " +
            "D0 1A C6 20 2E B5 B4 A9 F4 CA 3B 20 05 8C F3 05 " +
            "52 E0 8B 3E BC 72 3C D3 76 7C BC CA 48 0D FA 12 " +
            "DE DF 8E 8A 2C F3 36 35 A3 7E 61 BB 53 F5 C0 CE " +
            "B3 CC 5D 2D E1 CE 56 14 BB B7 F5 64 68 53 79 96 " +
            "9F ED BD D0 D9 38 4D 5B 65 16 A4 74 F2 80 79 8B " +
            "C1 09 4B 08 E9 DB 76 D5 1A E5 54 A7 14 28 39 D8 " +
            "17 93 48 D3 BE B2 7E B1 B9 59 49 4B FA BF B7 C0 " +
            "CA 7E 9C 35 BB BD D9 08 D8 56 5C E6 F0 1B 57 53 " +
            "46 55 8E 45 78 BE C6 CE 23 D4 AB D6 57 6F 8F 61 " +
            "F1 C9 94 02 1B F8 89 2E 3F 65 A5 79 6A B3 7B 87 " +
            "C3 B0 0A 75 A5 26 66 49",
            // After Rho
            "BF B0 D5 ED 4C 59 95 7C 61 F2 47 34 8D F4 B1 23 " +
            "8F B4 67 AC 0D 87 F9 86 91 41 70 E4 C2 B1 E4 CA " +
            "A9 A5 4D 85 D6 30 06 71 52 C0 38 5F 40 AF BC 03 " +
            "E8 C3 2B C7 33 2D 05 BE 84 1D 1F AF 32 52 83 BE " +
            "6F 47 45 96 79 9B 1A EF 0F EC 3C EA 17 B6 3B 55 " +
            "98 65 EE 6A 09 77 B6 A2 59 EE DE D6 93 A1 4D E5 " +
            "85 CE C6 69 DA FA 6C EF 01 F3 16 CB 2C 48 E9 E4 " +
            "84 F4 6D BB EA E0 84 25 4E 29 50 72 B0 35 CA A9 " +
            "69 DA 57 D6 2F F6 62 12 5B E0 DC AC A4 25 FD DF " +
            "37 1B 41 D9 8F B3 66 B7 53 D8 56 5C E6 F0 1B 57 " +
            "1A 3B 1B 55 39 16 E1 F9 8D 50 AF 5A 5F BD 3D 86 " +
            "3E 99 52 60 03 3F D1 25 65 A5 79 6A B3 7B 87 3F " +
            "59 D2 30 AC 42 5D A9 89",
            // After Pi
            "BF B0 D5 ED 4C 59 95 7C E8 C3 2B C7 33 2D 05 BE " +
            "85 CE C6 69 DA FA 6C EF 37 1B 41 D9 8F B3 66 B7 " +
            "59 D2 30 AC 42 5D A9 89 91 41 70 E4 C2 B1 E4 CA " +
            "0F EC 3C EA 17 B6 3B 55 98 65 EE 6A 09 77 B6 A2 " +
            "69 DA 57 D6 2F F6 62 12 3E 99 52 60 03 3F D1 25 " +
            "61 F2 47 34 8D F4 B1 23 84 1D 1F AF 32 52 83 BE " +
            "01 F3 16 CB 2C 48 E9 E4 53 D8 56 5C E6 F0 1B 57 " +
            "1A 3B 1B 55 39 16 E1 F9 A9 A5 4D 85 D6 30 06 71 " +
            "52 C0 38 5F 40 AF BC 03 59 EE DE D6 93 A1 4D E5 " +
            "5B E0 DC AC A4 25 FD DF 65 A5 79 6A B3 7B 87 3F " +
            "8F B4 67 AC 0D 87 F9 86 6F 47 45 96 79 9B 1A EF " +
            "84 F4 6D BB EA E0 84 25 4E 29 50 72 B0 35 CA A9 " +
            "8D 50 AF 5A 5F BD 3D 86",
            // After Chi
            "BA BC 11 C5 84 8B FD 3D DA D2 2A 57 36 2C 07 AE " +
            "CD 0E F6 4D 9A B6 E5 E7 91 3B 84 98 83 B3 72 C3 " +
            "19 91 1A AE 71 79 A9 0B 01 40 B2 E4 CA F0 60 68 " +
            "6E 76 2D 7E 31 36 7B 45 8E 64 EE 4A 09 7E 27 87 " +
            "E8 9A 77 52 EF 76 46 D8 30 35 5E 6A 16 39 CA 30 " +
            "60 10 47 74 81 FC D9 63 D6 15 5F BB F0 E2 91 AD " +
            "09 D0 1F CA 35 4E 09 4C 32 18 12 7C 62 10 0B 55 " +
            "9E 36 03 DE 0B 14 E3 65 A0 8B 8B 05 45 30 47 95 " +
            "50 C0 38 77 64 AB 0C 19 7D EB FF 94 80 FB 4F C5 " +
            "D3 E0 D8 29 E0 25 FD 9F 37 E5 49 30 B3 F4 3F 3D " +
            "0F 04 4F 85 8F E7 7D 86 25 4E 55 D6 69 8E 50 67 " +
            "05 A4 C2 B3 A5 68 B1 23 4C 8D 10 D6 B0 37 0A A9 " +
            "ED 13 AF 48 2F A5 3F EF",
            // After Iota 
            "31 3C 11 45 84 8B FD 3D DA D2 2A 57 36 2C 07 AE " +
            "CD 0E F6 4D 9A B6 E5 E7 91 3B 84 98 83 B3 72 C3 " +
            "19 91 1A AE 71 79 A9 0B 01 40 B2 E4 CA F0 60 68 " +
            "6E 76 2D 7E 31 36 7B 45 8E 64 EE 4A 09 7E 27 87 " +
            "E8 9A 77 52 EF 76 46 D8 30 35 5E 6A 16 39 CA 30 " +
            "60 10 47 74 81 FC D9 63 D6 15 5F BB F0 E2 91 AD " +
            "09 D0 1F CA 35 4E 09 4C 32 18 12 7C 62 10 0B 55 " +
            "9E 36 03 DE 0B 14 E3 65 A0 8B 8B 05 45 30 47 95 " +
            "50 C0 38 77 64 AB 0C 19 7D EB FF 94 80 FB 4F C5 " +
            "D3 E0 D8 29 E0 25 FD 9F 37 E5 49 30 B3 F4 3F 3D " +
            "0F 04 4F 85 8F E7 7D 86 25 4E 55 D6 69 8E 50 67 " +
            "05 A4 C2 B3 A5 68 B1 23 4C 8D 10 D6 B0 37 0A A9 " +
            "ED 13 AF 48 2F A5 3F EF",
            // Round #13
            // After Theta
            "72 26 DA 41 80 35 1E C0 40 DB 7F D6 34 57 13 1F " +
            "72 98 90 EC DC E5 C5 2E 78 06 FC B7 E0 AD 47 10 " +
            "33 82 72 4D 25 1E 9D 39 42 5A 79 E0 CE 4E 83 95 " +
            "F4 7F 78 FF 33 4D 6F F4 31 F2 88 EB 4F 2D 07 4E " +
            "01 A7 0F 7D 8C 68 73 0B 1A 26 36 89 42 5E FE 02 " +
            "23 0A 8C 70 85 42 3A 9E 4C 1C 0A 3A F2 99 85 1C " +
            "B6 46 79 6B 73 1D 29 85 DB 25 6A 53 01 0E 3E 86 " +
            "B4 25 6B 3D 5F 73 D7 57 E3 91 40 01 41 8E A4 68 " +
            "CA C9 6D F6 66 D0 18 A8 C2 7D 99 35 C6 A8 6F 0C " +
            "3A DD A0 06 83 3B C8 4C 1D F6 21 D3 E7 93 0B 0F " +
            "4C 1E 84 81 8B 59 9E 7B BF 47 00 57 6B F5 44 D6 " +
            "BA 32 A4 12 E3 3B 91 EA A5 B0 68 F9 D3 29 3F 7A " +
            "C7 00 C7 AB 7B C2 0B DD",
            // After Rho
            "72 26 DA 41 80 35 1E C0 80 B6 FF AC 69 AE 26 3E " +
            "1C 26 24 3B 77 79 B1 8B DE 7A 04 81 67 C0 7F 0B " +
            "F1 E8 CC 99 11 94 6B 2A EE EC 34 58 29 A4 95 07 " +
            "F7 3F D3 F4 46 4F FF 87 53 8C 3C E2 FA 53 CB 81 " +
            "D3 87 3E 46 B4 B9 85 80 E5 2F A0 61 62 93 28 E4 " +
            "1C 51 60 84 2B 14 D2 F1 72 30 71 28 E8 C8 67 16 " +
            "5B 9B EB 48 29 B4 35 CA 1C 7C 0C B7 4B D4 A6 02 " +
            "9E AF B9 EB 2B DA 92 B5 02 82 1C 49 D1 C6 23 81 " +
            "CD DE 0C 1A 03 55 39 B9 37 06 E1 BE CC 1A 63 D4 " +
            "07 99 49 A7 1B D4 60 70 0F 1D F6 21 D3 E7 93 0B " +
            "79 EE 31 79 10 06 2E 66 FF 1E 01 5C AD D5 13 59 " +
            "57 86 54 62 7C 27 52 5D B0 68 F9 D3 29 3F 7A A5 " +
            "42 F7 31 C0 F1 EA 9E F0",
            // After Pi
            "72 26 DA 41 80 35 1E C0 F7 3F D3 F4 46 4F FF 87 " +
            "5B 9B EB 48 29 B4 35 CA 07 99 49 A7 1B D4 60 70 " +
            "42 F7 31 C0 F1 EA 9E F0 DE 7A 04 81 67 C0 7F 0B " +
            "E5 2F A0 61 62 93 28 E4 1C 51 60 84 2B 14 D2 F1 " +
            "CD DE 0C 1A 03 55 39 B9 57 86 54 62 7C 27 52 5D " +
            "80 B6 FF AC 69 AE 26 3E 53 8C 3C E2 FA 53 CB 81 " +
            "1C 7C 0C B7 4B D4 A6 02 0F 1D F6 21 D3 E7 93 0B " +
            "79 EE 31 79 10 06 2E 66 F1 E8 CC 99 11 94 6B 2A " +
            "EE EC 34 58 29 A4 95 07 72 30 71 28 E8 C8 67 16 " +
            "37 06 E1 BE CC 1A 63 D4 B0 68 F9 D3 29 3F 7A A5 " +
            "1C 26 24 3B 77 79 B1 8B D3 87 3E 46 B4 B9 85 80 " +
            "9E AF B9 EB 2B DA 92 B5 02 82 1C 49 D1 C6 23 81 " +
            "FF 1E 01 5C AD D5 13 59",
            // After Chi
            "7A A6 F2 49 A9 85 1E 88 F3 3F D3 53 54 0F BF B7 " +
            "1B FD DB 08 C9 9E AB 4A 37 99 83 A6 1B C1 60 70 " +
            "C7 EE 30 74 B7 A0 7F F7 C6 2A 44 05 6E C4 AD 1A " +
            "24 A1 AC 7B 62 D2 01 EC 0E 51 30 E4 57 36 90 B5 " +
            "45 A6 0C 9B 00 95 14 BB 76 83 F4 02 7C 34 52 B9 " +
            "8C C6 FF B9 68 2A 02 3C 50 8D CE E2 6A 70 DA 88 " +
            "6C 9E 0D EF 4B D4 8A 66 8F 0D 38 A5 BA 4F 93 13 " +
            "2A E6 31 3B 82 57 E7 E7 E1 F8 8D B9 D1 DC 09 3A " +
            "EB EA B4 CE 2D B6 95 C7 F2 58 69 69 C9 ED 7F 37 " +
            "76 86 E5 B6 DC 9A 62 DE BE 6C C9 93 01 1F EE A0 " +
            "10 0E A5 92 7C 3B A3 BE D3 87 3A 46 64 BD A4 80 " +
            "63 B3 B8 FF 07 CB 82 ED 02 A2 38 6A 83 EE 83 03 " +
            "3C 9F 1B 18 2D 55 17 59",
            // After Iota 
            "F1 A6 F2 49 A9 85 1E 08 F3 3F D3 53 54 0F BF B7 " +
            "1B FD DB 08 C9 9E AB 4A 37 99 83 A6 1B C1 60 70 " +
            "C7 EE 30 74 B7 A0 7F F7 C6 2A 44 05 6E C4 AD 1A " +
            "24 A1 AC 7B 62 D2 01 EC 0E 51 30 E4 57 36 90 B5 " +
            "45 A6 0C 9B 00 95 14 BB 76 83 F4 02 7C 34 52 B9 " +
            "8C C6 FF B9 68 2A 02 3C 50 8D CE E2 6A 70 DA 88 " +
            "6C 9E 0D EF 4B D4 8A 66 8F 0D 38 A5 BA 4F 93 13 " +
            "2A E6 31 3B 82 57 E7 E7 E1 F8 8D B9 D1 DC 09 3A " +
            "EB EA B4 CE 2D B6 95 C7 F2 58 69 69 C9 ED 7F 37 " +
            "76 86 E5 B6 DC 9A 62 DE BE 6C C9 93 01 1F EE A0 " +
            "10 0E A5 92 7C 3B A3 BE D3 87 3A 46 64 BD A4 80 " +
            "63 B3 B8 FF 07 CB 82 ED 02 A2 38 6A 83 EE 83 03 " +
            "3C 9F 1B 18 2D 55 17 59",
            // Round #14
            // After Theta
            "97 23 AB 0B E6 40 86 70 69 30 DD A7 61 37 3C 9B " +
            "B6 AE 30 C2 20 E7 F2 D4 ED B0 FA BF CB 89 4B 93 " +
            "DB 80 99 8C 4C D7 4E A6 A0 AF 1D 47 21 01 35 62 " +
            "BE AE A2 8F 57 EA 82 C0 A3 02 DB 2E BE 4F C9 2B " +
            "9F 8F 75 82 D0 DD 3F 58 6A ED 5D FA 87 43 63 E8 " +
            "EA 43 A6 FB 27 EF 9A 44 CA 82 C0 16 5F 48 59 A4 " +
            "C1 CD E6 25 A2 AD D3 F8 55 24 41 BC 6A 07 B8 F0 " +
            "36 88 98 C3 79 20 D6 B6 87 7D D4 FB 9E 19 91 42 " +
            "71 E5 BA 3A 18 8E 16 EB 5F 0B 82 A3 20 94 26 A9 " +
            "AC AF 9C AF 0C D2 49 3D A2 02 60 6B FA 68 DF F1 " +
            "76 8B FC D0 33 FE 3B C6 49 88 34 B2 51 85 27 AC " +
            "CE E0 53 35 EE B2 DB 73 D8 8B 41 73 53 A6 A8 E0 " +
            "20 F1 B2 E0 D6 22 26 08",
            // After Rho
            "97 23 AB 0B E6 40 86 70 D3 60 BA 4F C3 6E 78 36 " +
            "AD 2B 8C 30 C8 B9 3C B5 9C B8 34 D9 0E AB FF BB " +
            "BA 76 32 DD 06 CC 64 64 14 12 50 23 06 FA DA 71 " +
            "FA 78 A5 2E 08 EC EB 2A CA A8 C0 B6 8B EF 53 F2 " +
            "C7 3A 41 E8 EE 1F AC CF 34 86 AE D6 DE A5 7F 38 " +
            "52 1F 32 DD 3F 79 D7 24 91 2A 0B 02 5B 7C 21 65 " +
            "2F 11 6D 9D C6 0F 6E 36 0E 70 E1 AB 48 82 78 D5 " +
            "E1 3C 10 6B 5B 1B 44 CC F7 3D 33 22 85 0E FB A8 " +
            "57 07 C3 D1 62 3D AE 5C 93 D4 AF 05 C1 51 10 4A " +
            "3A A9 87 F5 95 F3 95 41 F1 A2 02 60 6B FA 68 DF " +
            "EF 18 DB 2D F2 43 CF F8 26 21 D2 C8 46 15 9E B0 " +
            "19 7C AA C6 5D 76 7B CE 8B 41 73 53 A6 A8 E0 D8 " +
            "09 02 48 BC 2C B8 B5 88",
            // After Pi
            "97 23 AB 0B E6 40 86 70 FA 78 A5 2E 08 EC EB 2A " +
            "2F 11 6D 9D C6 0F 6E 36 3A A9 87 F5 95 F3 95 41 " +
            "09 02 48 BC 2C B8 B5 88 9C B8 34 D9 0E AB FF BB " +
            "34 86 AE D6 DE A5 7F 38 52 1F 32 DD 3F 79 D7 24 " +
            "57 07 C3 D1 62 3D AE 5C 19 7C AA C6 5D 76 7B CE " +
            "D3 60 BA 4F C3 6E 78 36 CA A8 C0 B6 8B EF 53 F2 " +
            "0E 70 E1 AB 48 82 78 D5 F1 A2 02 60 6B FA 68 DF " +
            "EF 18 DB 2D F2 43 CF F8 BA 76 32 DD 06 CC 64 64 " +
            "14 12 50 23 06 FA DA 71 91 2A 0B 02 5B 7C 21 65 " +
            "93 D4 AF 05 C1 51 10 4A 8B 41 73 53 A6 A8 E0 D8 " +
            "AD 2B 8C 30 C8 B9 3C B5 C7 3A 41 E8 EE 1F AC CF " +
            "E1 3C 10 6B 5B 1B 44 CC F7 3D 33 22 85 0E FB A8 " +
            "26 21 D2 C8 46 15 9E B0",
            // After Chi
            "92 22 E3 9A 20 43 82 64 EA D0 27 4E 19 1C 7A 6B " +
            "2E 13 25 95 EE 07 4E BE AC 88 24 F6 57 B3 97 31 " +
            "61 5A 4C 98 24 14 DC 82 DE A1 24 D0 2F F3 7F BF " +
            "31 86 6F D6 9E A1 57 60 5A 67 1A DB 22 3B 86 A6 " +
            "D3 87 D7 C8 60 B4 2A 6D 39 7A 20 C0 8D 72 7B CE " +
            "D7 30 9B 46 83 6E 50 33 3B 2A C2 F6 A8 97 53 F8 " +
            "00 68 38 A6 D8 83 FF F5 E1 C2 22 22 6A D6 58 D9 " +
            "E7 90 9B 9D FA C2 CC 38 3B 5E 39 DD 5F C8 45 60 " +
            "16 C6 F4 26 86 FB CA 7B 99 2B 5B 50 7D D4 C1 F5 " +
            "A3 E2 AF 89 C1 15 14 6E 8F 41 33 71 A6 9A 7A C9 " +
            "8D 2F 9C 33 D9 B9 7C B5 D1 3B 62 E8 6A 1B 17 EF " +
            "E1 3C D0 A3 19 0A 40 DC 7E 37 3F 12 0D A6 DB AD " +
            "64 31 93 00 60 13 1E FA",
            // After Iota 
            "1B A2 E3 9A 20 43 82 E4 EA D0 27 4E 19 1C 7A 6B " +
            "2E 13 25 95 EE 07 4E BE AC 88 24 F6 57 B3 97 31 " +
            "61 5A 4C 98 24 14 DC 82 DE A1 24 D0 2F F3 7F BF " +
            "31 86 6F D6 9E A1 57 60 5A 67 1A DB 22 3B 86 A6 " +
            "D3 87 D7 C8 60 B4 2A 6D 39 7A 20 C0 8D 72 7B CE " +
            "D7 30 9B 46 83 6E 50 33 3B 2A C2 F6 A8 97 53 F8 " +
            "00 68 38 A6 D8 83 FF F5 E1 C2 22 22 6A D6 58 D9 " +
            "E7 90 9B 9D FA C2 CC 38 3B 5E 39 DD 5F C8 45 60 " +
            "16 C6 F4 26 86 FB CA 7B 99 2B 5B 50 7D D4 C1 F5 " +
            "A3 E2 AF 89 C1 15 14 6E 8F 41 33 71 A6 9A 7A C9 " +
            "8D 2F 9C 33 D9 B9 7C B5 D1 3B 62 E8 6A 1B 17 EF " +
            "E1 3C D0 A3 19 0A 40 DC 7E 37 3F 12 0D A6 DB AD " +
            "64 31 93 00 60 13 1E FA",
            // Round #15
            // After Theta
            "01 60 8D 6E 32 FB CA 6C 57 84 C6 9B F3 71 82 5F " +
            "8F A2 BB 3B 0E 08 B9 55 08 03 07 85 0C 89 3F 7B " +
            "6B C7 FF DA A0 28 DF BF C4 63 4A 24 3D 4B 37 37 " +
            "8C D2 8E 03 74 CC AF 54 FB D6 84 75 C2 34 71 4D " +
            "77 0C F4 BB 3B 8E 82 27 33 E7 93 82 09 4E 78 F3 " +
            "CD F2 F5 B2 91 D6 18 BB 86 7E 23 23 42 FA AB CC " +
            "A1 D9 A6 08 38 8C 08 1E 45 49 01 51 31 EC F0 93 " +
            "ED 0D 28 DF 7E FE CF 05 21 9C 57 29 4D 70 0D E8 " +
            "AB 92 15 F3 6C 96 32 4F 38 9A C5 FE 9D DB 36 1E " +
            "07 69 8C FA 9A 2F BC 24 85 DC 80 33 22 A6 79 F4 " +
            "97 ED F2 C7 CB 01 34 3D 6C 6F 83 3D 80 76 EF DB " +
            "40 8D 4E 0D F9 05 B7 37 DA BC 1C 61 56 9C 73 E7 " +
            "6E AC 20 42 E4 2F 1D C7",
            // After Rho
            "01 60 8D 6E 32 FB CA 6C AE 08 8D 37 E7 E3 04 BF " +
            "A3 E8 EE 8E 03 42 6E D5 90 F8 B3 87 30 70 50 C8 " +
            "45 F9 FE 5D 3B FE D7 06 D2 B3 74 73 43 3C A6 44 " +
            "38 40 C7 FC 4A C5 28 ED D3 BE 35 61 9D 30 4D 5C " +
            "06 FA DD 1D 47 C1 93 3B 84 37 3F 73 3E 29 98 E0 " +
            "6D 96 AF 97 8D B4 C6 D8 32 1B FA 8D 8C 08 E9 AF " +
            "45 C0 61 44 F0 08 CD 36 D8 E1 27 8B 92 02 A2 62 " +
            "6F 3F FF E7 82 F6 06 94 52 9A E0 1A D0 43 38 AF " +
            "62 9E CD 52 E6 69 55 B2 1B 0F 1C CD 62 FF CE 6D " +
            "85 97 E4 20 8D 51 5F F3 F4 85 DC 80 33 22 A6 79 " +
            "D0 F4 5C B6 CB 1F 2F 07 B3 BD 0D F6 00 DA BD 6F " +
            "A8 D1 A9 21 BF E0 F6 06 BC 1C 61 56 9C 73 E7 DA " +
            "C7 B1 1B 2B 88 10 F9 4B",
            // After Pi
            "01 60 8D 6E 32 FB CA 6C 38 40 C7 FC 4A C5 28 ED " +
            "45 C0 61 44 F0 08 CD 36 85 97 E4 20 8D 51 5F F3 " +
            "C7 B1 1B 2B 88 10 F9 4B 90 F8 B3 87 30 70 50 C8 " +
            "84 37 3F 73 3E 29 98 E0 6D 96 AF 97 8D B4 C6 D8 " +
            "62 9E CD 52 E6 69 55 B2 A8 D1 A9 21 BF E0 F6 06 " +
            "AE 08 8D 37 E7 E3 04 BF D3 BE 35 61 9D 30 4D 5C " +
            "D8 E1 27 8B 92 02 A2 62 F4 85 DC 80 33 22 A6 79 " +
            "D0 F4 5C B6 CB 1F 2F 07 45 F9 FE 5D 3B FE D7 06 " +
            "D2 B3 74 73 43 3C A6 44 32 1B FA 8D 8C 08 E9 AF " +
            "1B 0F 1C CD 62 FF CE 6D BC 1C 61 56 9C 73 E7 DA " +
            "A3 E8 EE 8E 03 42 6E D5 06 FA DD 1D 47 C1 93 3B " +
            "6F 3F FF E7 82 F6 06 94 52 9A E0 1A D0 43 38 AF " +
            "B3 BD 0D F6 00 DA BD 6F",
            // After Chi
            "44 E0 AD 6E 82 F3 0F 7E B8 57 43 DC 47 94 3A 2C " +
            "07 E0 7A 4F F0 08 6D 3E 85 D7 60 64 BF BA 5D D7 " +
            "FF B1 59 BB C0 14 D9 CA F9 78 33 03 B1 E4 16 D0 " +
            "86 3F 7F 33 5C 60 89 C2 E5 D7 8F B6 94 34 64 DC " +
            "72 B6 DF D4 E6 79 55 7A AC D6 A5 51 B1 E9 7E 26 " +
            "A6 49 8F BD E5 E1 A6 9D F7 BA ED 61 BC 10 49 45 " +
            "D8 91 27 BD 5A 1F AB 64 DA 8D 5D 81 17 C2 A6 C1 " +
            "81 42 6C F6 D3 0F 66 47 65 F1 74 D1 B7 FE 9E AD " +
            "DB B7 70 33 21 CB A0 04 96 0B 9B 9F 10 08 C8 3D " +
            "5A EE 82 C4 41 73 DE 69 2E 1E 61 74 DC 73 C7 9A " +
            "CA ED CC 6C 83 74 6A 51 16 7A DD 05 17 C0 AB 10 " +
            "CE 1A F2 03 82 6E 83 D4 52 DA 02 12 D3 43 7A 3F " +
            "B7 AF 1C E7 44 5B 2C 45",
            // After Iota 
            "47 60 AD 6E 82 F3 0F FE B8 57 43 DC 47 94 3A 2C " +
            "07 E0 7A 4F F0 08 6D 3E 85 D7 60 64 BF BA 5D D7 " +
            "FF B1 59 BB C0 14 D9 CA F9 78 33 03 B1 E4 16 D0 " +
            "86 3F 7F 33 5C 60 89 C2 E5 D7 8F B6 94 34 64 DC " +
            "72 B6 DF D4 E6 79 55 7A AC D6 A5 51 B1 E9 7E 26 " +
            "A6 49 8F BD E5 E1 A6 9D F7 BA ED 61 BC 10 49 45 " +
            "D8 91 27 BD 5A 1F AB 64 DA 8D 5D 81 17 C2 A6 C1 " +
            "81 42 6C F6 D3 0F 66 47 65 F1 74 D1 B7 FE 9E AD " +
            "DB B7 70 33 21 CB A0 04 96 0B 9B 9F 10 08 C8 3D " +
            "5A EE 82 C4 41 73 DE 69 2E 1E 61 74 DC 73 C7 9A " +
            "CA ED CC 6C 83 74 6A 51 16 7A DD 05 17 C0 AB 10 " +
            "CE 1A F2 03 82 6E 83 D4 52 DA 02 12 D3 43 7A 3F " +
            "B7 AF 1C E7 44 5B 2C 45",
            // Round #16
            // After Theta
            "05 CA B8 91 9B F6 C6 F5 CB 74 9D 00 FC 63 A3 BC " +
            "49 4F C3 39 D8 84 88 F5 71 48 00 A3 66 4B E1 50 " +
            "B4 F2 69 87 D8 DC 45 6E BB D2 26 FC A8 E1 DF DB " +
            "F5 1C A1 EF E7 97 10 52 AB 78 36 C0 BC B8 81 17 " +
            "86 29 BF 13 3F 88 E9 FD E7 95 95 6D A9 21 E2 82 " +
            "E4 E3 9A 42 FC E4 6F 96 84 99 33 BD 07 E7 D0 D5 " +
            "96 3E 9E CB 72 93 4E AF 2E 12 3D 46 CE 33 1A 46 " +
            "CA 01 5C CA CB C7 FA E3 27 5B 61 2E AE FB 57 A6 " +
            "A8 94 AE EF 9A 3C 39 94 D8 A4 22 E9 38 84 2D F6 " +
            "AE 71 E2 03 98 82 62 EE 65 5D 51 48 C4 BB 5B 3E " +
            "88 47 D9 93 9A 71 A3 5A 65 59 03 D9 AC 37 32 80 " +
            "80 B5 4B 75 AA E2 66 1F A6 45 62 D5 0A B2 C6 B8 " +
            "FC EC 2C DB 5C 93 B0 E1",
            // After Rho
            "05 CA B8 91 9B F6 C6 F5 97 E9 3A 01 F8 C7 46 79 " +
            "D2 D3 70 0E 36 21 62 7D B6 14 0E 15 87 04 30 6A " +
            "E6 2E 72 A3 95 4F 3B C4 8F 1A FE BD BD 2B 6D C2 " +
            "FA 7E 7E 09 21 55 CF 11 C5 2A 9E 0D 30 2F 6E E0 " +
            "94 DF 89 1F C4 F4 7E C3 22 2E 78 5E 59 D9 96 1A " +
            "24 1F D7 14 E2 27 7F B3 57 13 66 CE F4 1E 9C 43 " +
            "5C 96 9B 74 7A B5 F4 F1 67 34 8C 5C 24 7A 8C 9C " +
            "E5 E5 63 FD 71 E5 00 2E 5C 5C F7 AF 4C 4F B6 C2 " +
            "F5 5D 93 27 87 12 95 D2 16 7B 6C 52 91 74 1C C2 " +
            "50 CC DD 35 4E 7C 00 53 3E 65 5D 51 48 C4 BB 5B " +
            "8D 6A 21 1E 65 4F 6A C6 96 65 0D 64 B3 DE C8 00 " +
            "B0 76 A9 4E 55 DC EC 03 45 62 D5 0A B2 C6 B8 A6 " +
            "6C 38 3F 3B CB 36 D7 24",
            // After Pi
            "05 CA B8 91 9B F6 C6 F5 FA 7E 7E 09 21 55 CF 11 " +
            "5C 96 9B 74 7A B5 F4 F1 50 CC DD 35 4E 7C 00 53 " +
            "6C 38 3F 3B CB 36 D7 24 B6 14 0E 15 87 04 30 6A " +
            "22 2E 78 5E 59 D9 96 1A 24 1F D7 14 E2 27 7F B3 " +
            "F5 5D 93 27 87 12 95 D2 B0 76 A9 4E 55 DC EC 03 " +
            "97 E9 3A 01 F8 C7 46 79 C5 2A 9E 0D 30 2F 6E E0 " +
            "67 34 8C 5C 24 7A 8C 9C 3E 65 5D 51 48 C4 BB 5B " +
            "8D 6A 21 1E 65 4F 6A C6 E6 2E 72 A3 95 4F 3B C4 " +
            "8F 1A FE BD BD 2B 6D C2 57 13 66 CE F4 1E 9C 43 " +
            "16 7B 6C 52 91 74 1C C2 45 62 D5 0A B2 C6 B8 A6 " +
            "D2 D3 70 0E 36 21 62 7D 94 DF 89 1F C4 F4 7E C3 " +
            "E5 E5 63 FD 71 E5 00 2E 5C 5C F7 AF 4C 4F B6 C2 " +
            "96 65 0D 64 B3 DE C8 00",
            // After Chi
            "01 4A 39 E5 C1 56 F6 15 FA 36 3A 08 25 1D CF 13 " +
            "70 A6 B9 7E FB B7 23 D5 51 0E 5D B5 5E BC 00 82 " +
            "96 0C 79 33 EB 37 DE 24 B2 05 89 15 25 22 59 CB " +
            "F3 6E 78 7D 5C C9 16 5A 24 3D FF 5C B2 EB 17 B2 " +
            "F3 5D 95 36 05 12 85 BA B0 5C D9 04 0D 05 6A 13 " +
            "B5 FD 3A 51 FC 97 C6 65 DD 6B CF 0C 78 AB 5D A3 " +
            "E6 3E AC 52 01 71 CC 18 2C E4 47 50 D0 44 BF 62 " +
            "CD 68 A5 12 65 67 42 46 B6 2F 72 E1 D5 5B AB C5 " +
            "8F 72 F6 AD BC 4B 6D 42 16 13 F7 C6 D6 9C 3C 67 " +
            "B4 77 4E F3 94 7D 1F 82 4C 72 59 16 9A E6 FC A4 " +
            "B3 F3 12 EE 07 20 62 51 8C C7 1D 1D C8 FE C8 03 " +
            "67 C4 6B BD C2 75 48 2E 1C CE 87 A5 48 6E 94 BF " +
            "92 69 84 75 73 0A D4 82",
            // After Iota 
            "03 CA 39 E5 C1 56 F6 95 FA 36 3A 08 25 1D CF 13 " +
            "70 A6 B9 7E FB B7 23 D5 51 0E 5D B5 5E BC 00 82 " +
            "96 0C 79 33 EB 37 DE 24 B2 05 89 15 25 22 59 CB " +
            "F3 6E 78 7D 5C C9 16 5A 24 3D FF 5C B2 EB 17 B2 " +
            "F3 5D 95 36 05 12 85 BA B0 5C D9 04 0D 05 6A 13 " +
            "B5 FD 3A 51 FC 97 C6 65 DD 6B CF 0C 78 AB 5D A3 " +
            "E6 3E AC 52 01 71 CC 18 2C E4 47 50 D0 44 BF 62 " +
            "CD 68 A5 12 65 67 42 46 B6 2F 72 E1 D5 5B AB C5 " +
            "8F 72 F6 AD BC 4B 6D 42 16 13 F7 C6 D6 9C 3C 67 " +
            "B4 77 4E F3 94 7D 1F 82 4C 72 59 16 9A E6 FC A4 " +
            "B3 F3 12 EE 07 20 62 51 8C C7 1D 1D C8 FE C8 03 " +
            "67 C4 6B BD C2 75 48 2E 1C CE 87 A5 48 6E 94 BF " +
            "92 69 84 75 73 0A D4 82",
            // Round #17
            // After Theta
            "99 E4 2C 31 40 7B 6B 94 7D 3D 3C B0 57 0D 76 D1 " +
            "EB 3C 53 BD 21 8F 61 B1 F8 3A 9B 33 D6 0A 31 1B " +
            "B3 DE EA EB 29 FF 2E 1C 28 2B 9C C1 A4 0F C4 CA " +
            "74 65 7E C5 2E D9 AF 98 BF A7 15 9F 68 D3 55 D6 " +
            "5A 69 53 B0 8D A4 B4 23 95 8E 4A DC CF CD 9A 2B " +
            "2F D3 2F 85 7D BA 5B 64 5A 60 C9 B4 0A BB E4 61 " +
            "7D A4 46 91 DB 49 8E 7C 85 D0 81 D6 58 F2 8E FB " +
            "E8 BA 36 CA A7 AF B2 7E 2C 01 67 35 54 76 36 C4 " +
            "08 79 F0 15 CE 5B D4 80 8D 89 1D 05 0C A4 7E 03 " +
            "1D 43 88 75 1C CB 2E 1B 69 A0 CA CE 58 2E 0C 9C " +
            "29 DD 07 3A 86 0D FF 50 0B CC 1B A5 BA EE 71 C1 " +
            "FC 5E 81 7E 18 4D 0A 4A B5 FA 41 23 C0 D8 A5 26 " +
            "B7 BB 17 AD B1 C2 24 BA",
            // After Rho
            "99 E4 2C 31 40 7B 6B 94 FB 7A 78 60 AF 1A EC A2 " +
            "3A CF 54 6F C8 63 58 EC AD 10 B3 81 AF B3 39 63 " +
            "F9 77 E1 98 F5 56 5F 4F 4C FA 40 AC 8C B2 C2 19 " +
            "57 EC 92 FD 8A 49 57 E6 F5 EF 69 C5 27 DA 74 95 " +
            "B4 29 D8 46 52 DA 11 AD AC B9 52 E9 A8 C4 FD DC " +
            "7B 99 7E 29 EC D3 DD 22 87 69 81 25 D3 2A EC 92 " +
            "8A DC 4E 72 E4 EB 23 35 E4 1D F7 0B A1 03 AD B1 " +
            "E5 D3 57 59 3F 74 5D 1B 6A A8 EC 6C 88 59 02 CE " +
            "BE C2 79 8B 1A 10 21 0F BF 81 C6 C4 8E 02 06 52 " +
            "D9 65 A3 63 08 B1 8E 63 9C 69 A0 CA CE 58 2E 0C " +
            "FC 43 A5 74 1F E8 18 36 2F 30 6F 94 EA BA C7 05 " +
            "DF 2B D0 0F A3 49 41 89 FA 41 23 C0 D8 A5 26 B5 " +
            "89 EE ED EE 45 6B AC 30",
            // After Pi
            "99 E4 2C 31 40 7B 6B 94 57 EC 92 FD 8A 49 57 E6 " +
            "8A DC 4E 72 E4 EB 23 35 D9 65 A3 63 08 B1 8E 63 " +
            "89 EE ED EE 45 6B AC 30 AD 10 B3 81 AF B3 39 63 " +
            "AC B9 52 E9 A8 C4 FD DC 7B 99 7E 29 EC D3 DD 22 " +
            "BE C2 79 8B 1A 10 21 0F DF 2B D0 0F A3 49 41 89 " +
            "FB 7A 78 60 AF 1A EC A2 F5 EF 69 C5 27 DA 74 95 " +
            "E4 1D F7 0B A1 03 AD B1 9C 69 A0 CA CE 58 2E 0C " +
            "FC 43 A5 74 1F E8 18 36 F9 77 E1 98 F5 56 5F 4F " +
            "4C FA 40 AC 8C B2 C2 19 87 69 81 25 D3 2A EC 92 " +
            "BF 81 C6 C4 8E 02 06 52 FA 41 23 C0 D8 A5 26 B5 " +
            "3A CF 54 6F C8 63 58 EC B4 29 D8 46 52 DA 11 AD " +
            "E5 D3 57 59 3F 74 5D 1B 6A A8 EC 6C 88 59 02 CE " +
            "2F 30 6F 94 EA BA C7 05",
            // After Chi
            "11 F4 60 33 24 D9 4B 85 06 CD 33 FC 82 59 DB A4 " +
            "8A 56 02 FE A1 A1 03 25 C9 65 A3 72 08 A1 CD E7 " +
            "CF E6 7F 22 CF 6B B8 52 FE 10 9F 81 EB A0 39 41 " +
            "28 FB 53 6B BA C4 DD D1 3A B0 FE 2D 4D 9A 9D A2 " +
            "9E D2 5A 0B 16 A2 19 6D DF 82 90 67 A3 0D 85 15 " +
            "FB 6A EE 6A 2F 1B 65 82 ED 8F 69 05 69 82 76 99 " +
            "84 1F F2 3F B0 A3 BD 83 9F 51 F8 CA 6E 4A CA 8C " +
            "F8 C6 A4 F1 1F 28 08 23 7A 76 60 99 A6 5E 73 CD " +
            "74 7A 06 6C 80 B2 C0 59 C7 29 A0 25 83 8F CC 37 " +
            "BE B7 06 DC AB 50 5F 18 FE C9 23 E4 D0 05 A6 A5 " +
            "7B 1D 53 76 E5 47 14 FE BE 01 70 62 D2 D3 13 69 " +
            "E0 C3 54 C9 5D D6 98 1A 7A 67 FC 07 88 18 1A 26 " +
            "AB 10 E7 94 F8 22 C6 04",
            // After Iota 
            "91 F4 60 33 24 D9 4B 05 06 CD 33 FC 82 59 DB A4 " +
            "8A 56 02 FE A1 A1 03 25 C9 65 A3 72 08 A1 CD E7 " +
            "CF E6 7F 22 CF 6B B8 52 FE 10 9F 81 EB A0 39 41 " +
            "28 FB 53 6B BA C4 DD D1 3A B0 FE 2D 4D 9A 9D A2 " +
            "9E D2 5A 0B 16 A2 19 6D DF 82 90 67 A3 0D 85 15 " +
            "FB 6A EE 6A 2F 1B 65 82 ED 8F 69 05 69 82 76 99 " +
            "84 1F F2 3F B0 A3 BD 83 9F 51 F8 CA 6E 4A CA 8C " +
            "F8 C6 A4 F1 1F 28 08 23 7A 76 60 99 A6 5E 73 CD " +
            "74 7A 06 6C 80 B2 C0 59 C7 29 A0 25 83 8F CC 37 " +
            "BE B7 06 DC AB 50 5F 18 FE C9 23 E4 D0 05 A6 A5 " +
            "7B 1D 53 76 E5 47 14 FE BE 01 70 62 D2 D3 13 69 " +
            "E0 C3 54 C9 5D D6 98 1A 7A 67 FC 07 88 18 1A 26 " +
            "AB 10 E7 94 F8 22 C6 04",
            // Round #18
            // After Theta
            "3F 0B 10 CF 78 4C 58 79 B5 0E E5 CA 25 A1 44 03 " +
            "9B F8 8B B3 04 DD 16 89 A1 81 47 FB 3D B2 10 44 " +
            "E8 1B C1 24 DA 9D 03 80 50 EF EF 7D B7 35 2A 3D " +
            "9B 38 85 5D 1D 3C 42 76 2B 1E 77 60 E8 E6 88 0E " +
            "F6 36 BE 82 23 B1 C4 CE F8 7F 2E 61 B6 FB 3E C7 " +
            "55 95 9E 96 73 8E 76 FE 5E 4C BF 33 CE 7A E9 3E " +
            "95 B1 7B 72 15 DF A8 2F F7 B5 1C 43 5B 59 17 2F " +
            "DF 3B 1A F7 0A DE B3 F1 D4 89 10 65 FA CB 60 B1 " +
            "C7 B9 D0 5A 27 4A 5F FE D6 87 29 68 26 F3 D9 9B " +
            "D6 53 E2 55 9E 43 82 BB D9 34 9D E2 C5 F3 1D 77 " +
            "D5 E2 23 8A B9 D2 07 82 0D C2 A6 54 75 2B 8C CE " +
            "F1 6D DD 84 F8 AA 8D B6 12 83 18 8E BD 0B C7 85 " +
            "8C ED 59 92 ED D4 7D D6",
            // After Rho
            "3F 0B 10 CF 78 4C 58 79 6A 1D CA 95 4B 42 89 06 " +
            "26 FE E2 2C 41 B7 45 E2 23 0B 41 14 1A 78 B4 DF " +
            "EE 1C 00 44 DF 08 26 D1 77 5B A3 D2 03 F5 FE DE " +
            "D8 D5 C1 23 64 B7 89 53 C3 8A C7 1D 18 BA 39 A2 " +
            "1B 5F C1 91 58 62 67 7B EF 73 8C FF E7 12 66 BB " +
            "AF AA F4 B4 9C 73 B4 F3 FB 78 31 FD CE 38 EB A5 " +
            "93 AB F8 46 7D A9 8C DD B2 2E 5E EE 6B 39 86 B6 " +
            "7B 05 EF D9 F8 EF 1D 8D CA F4 97 C1 62 A9 13 21 " +
            "5A EB 44 E9 CB FF 38 17 EC 4D EB C3 14 34 93 F9 " +
            "48 70 D7 7A 4A BC CA 73 77 D9 34 9D E2 C5 F3 1D " +
            "1F 08 56 8B 8F 28 E6 4A 37 08 9B 52 D5 AD 30 3A " +
            "BE AD 9B 10 5F B5 D1 36 83 18 8E BD 0B C7 85 12 " +
            "9F 35 63 7B 96 64 3B 75",
            // After Pi
            "3F 0B 10 CF 78 4C 58 79 D8 D5 C1 23 64 B7 89 53 " +
            "93 AB F8 46 7D A9 8C DD 48 70 D7 7A 4A BC CA 73 " +
            "9F 35 63 7B 96 64 3B 75 23 0B 41 14 1A 78 B4 DF " +
            "EF 73 8C FF E7 12 66 BB AF AA F4 B4 9C 73 B4 F3 " +
            "5A EB 44 E9 CB FF 38 17 BE AD 9B 10 5F B5 D1 36 " +
            "6A 1D CA 95 4B 42 89 06 C3 8A C7 1D 18 BA 39 A2 " +
            "B2 2E 5E EE 6B 39 86 B6 77 D9 34 9D E2 C5 F3 1D " +
            "1F 08 56 8B 8F 28 E6 4A EE 1C 00 44 DF 08 26 D1 " +
            "77 5B A3 D2 03 F5 FE DE FB 78 31 FD CE 38 EB A5 " +
            "EC 4D EB C3 14 34 93 F9 83 18 8E BD 0B C7 85 12 " +
            "26 FE E2 2C 41 B7 45 E2 1B 5F C1 91 58 62 67 7B " +
            "7B 05 EF D9 F8 EF 1D 8D CA F4 97 C1 62 A9 13 21 " +
            "37 08 9B 52 D5 AD 30 3A",
            // After Chi
            "3C 21 28 8B 61 44 5C F5 90 85 C6 1B 66 A3 CB 71 " +
            "04 AE D8 47 E9 E9 BD D9 68 7A C7 FE 22 B4 8A 7B " +
            "5F E1 A2 5B 92 D7 BA 77 23 83 31 14 02 19 24 9F " +
            "BF 32 8C B6 A4 9E 6E BF 0B AE 6F A4 88 73 75 D3 " +
            "5B E9 04 ED CB B7 1C DE 72 DD 17 FB BA B7 93 16 " +
            "5A 39 D2 77 28 43 0F 12 86 5B E7 0C 98 7E 48 AB " +
            "BA 2E 1C EC 66 11 82 F4 17 CC BC 89 A2 87 FA 19 " +
            "9E 8A 53 83 9F 90 D6 EA 66 3C 10 69 13 00 27 F0 " +
            "73 5E 69 D0 13 F1 EE 86 F8 68 35 C1 C5 FB EF A7 " +
            "80 49 EB 83 C0 3C B1 38 92 5B 2D 2F 0B 32 5D 1C " +
            "46 FE CC 64 E1 3A 5D 66 9B AF D1 91 5A 62 65 5B " +
            "4E 0D E7 CB 6D EB 3D 97 CA 02 F7 ED 62 BB 56 E1 " +
            "2E 09 9A C3 CD ED 12 23",
            // After Iota 
            "36 A1 28 8B 61 44 5C F5 90 85 C6 1B 66 A3 CB 71 " +
            "04 AE D8 47 E9 E9 BD D9 68 7A C7 FE 22 B4 8A 7B " +
            "5F E1 A2 5B 92 D7 BA 77 23 83 31 14 02 19 24 9F " +
            "BF 32 8C B6 A4 9E 6E BF 0B AE 6F A4 88 73 75 D3 " +
            "5B E9 04 ED CB B7 1C DE 72 DD 17 FB BA B7 93 16 " +
            "5A 39 D2 77 28 43 0F 12 86 5B E7 0C 98 7E 48 AB " +
            "BA 2E 1C EC 66 11 82 F4 17 CC BC 89 A2 87 FA 19 " +
            "9E 8A 53 83 9F 90 D6 EA 66 3C 10 69 13 00 27 F0 " +
            "73 5E 69 D0 13 F1 EE 86 F8 68 35 C1 C5 FB EF A7 " +
            "80 49 EB 83 C0 3C B1 38 92 5B 2D 2F 0B 32 5D 1C " +
            "46 FE CC 64 E1 3A 5D 66 9B AF D1 91 5A 62 65 5B " +
            "4E 0D E7 CB 6D EB 3D 97 CA 02 F7 ED 62 BB 56 E1 " +
            "2E 09 9A C3 CD ED 12 23",
            // Round #19
            // After Theta
            "BA 7F 53 84 37 CB 21 31 F8 CA 23 F4 81 B0 F7 02 " +
            "99 9B 0B 4F 29 3E CD AA 74 F9 1D 65 6E 71 72 DC " +
            "EE 47 EE 65 08 9D 2B CE AF 5D 4A 1B 54 96 59 5B " +
            "D7 7D 69 59 43 8D 52 CC 96 9B BC AC 48 A4 05 A0 " +
            "47 6A DE 76 87 72 E4 79 C3 7B 5B C5 20 FD 02 AF " +
            "D6 E7 A9 78 7E CC 72 D6 EE 14 02 E3 7F 6D 74 D8 " +
            "27 1B CF E4 A6 C6 F2 87 0B 4F 66 12 EE 42 02 BE " +
            "2F 2C 1F BD 05 DA 47 53 EA E2 6B 66 45 8F 5A 34 " +
            "1B 11 8C 3F F4 E2 D2 F5 65 5D E6 C9 05 2C 9F D4 " +
            "9C CA 31 18 8C F9 49 9F 23 FD 61 11 91 78 CC A5 " +
            "CA 20 B7 6B B7 B5 20 A2 F3 E0 34 7E BD 71 59 28 " +
            "D3 38 34 C3 AD 3C 4D E4 D6 81 2D 76 2E 7E AE 46 " +
            "9F AF D6 FD 57 A7 83 9A",
            // After Rho
            "BA 7F 53 84 37 CB 21 31 F0 95 47 E8 03 61 EF 05 " +
            "E6 E6 C2 53 8A 4F B3 6A 16 27 C7 4D 97 DF 51 E6 " +
            "E8 5C 71 76 3F 72 2F 43 41 65 99 B5 F5 DA A5 B4 " +
            "96 35 D4 28 C5 7C DD 97 A8 E5 26 2F 2B 12 69 01 " +
            "35 6F BB 43 39 F2 BC 23 2F F0 3A BC B7 55 0C D2 " +
            "B6 3E 4F C5 F3 63 96 B3 61 BB 53 08 8C FF B5 D1 " +
            "26 37 35 96 3F 3C D9 78 85 04 7C 17 9E CC 24 DC " +
            "DE 02 ED A3 A9 17 96 8F CC 8A 1E B5 68 D4 C5 D7 " +
            "F1 87 5E 5C BA 7E 23 82 4F EA B2 2E F3 E4 02 96 " +
            "3F E9 93 53 39 06 83 31 A5 23 FD 61 11 91 78 CC " +
            "82 88 2A 83 DC AE DD D6 CC 83 D3 F8 F5 C6 65 A1 " +
            "1A 87 66 B8 95 A7 89 7C 81 2D 76 2E 7E AE 46 D6 " +
            "A0 E6 E7 AB 75 FF D5 E9",
            // After Pi
            "BA 7F 53 84 37 CB 21 31 96 35 D4 28 C5 7C DD 97 " +
            "26 37 35 96 3F 3C D9 78 3F E9 93 53 39 06 83 31 " +
            "A0 E6 E7 AB 75 FF D5 E9 16 27 C7 4D 97 DF 51 E6 " +
            "2F F0 3A BC B7 55 0C D2 B6 3E 4F C5 F3 63 96 B3 " +
            "F1 87 5E 5C BA 7E 23 82 1A 87 66 B8 95 A7 89 7C " +
            "F0 95 47 E8 03 61 EF 05 A8 E5 26 2F 2B 12 69 01 " +
            "85 04 7C 17 9E CC 24 DC A5 23 FD 61 11 91 78 CC " +
            "82 88 2A 83 DC AE DD D6 E8 5C 71 76 3F 72 2F 43 " +
            "41 65 99 B5 F5 DA A5 B4 61 BB 53 08 8C FF B5 D1 " +
            "4F EA B2 2E F3 E4 02 96 81 2D 76 2E 7E AE 46 D6 " +
            "E6 E6 C2 53 8A 4F B3 6A 35 6F BB 43 39 F2 BC 23 " +
            "DE 02 ED A3 A9 17 96 8F CC 8A 1E B5 68 D4 C5 D7 " +
            "CC 83 D3 F8 F5 C6 65 A1",
            // After Chi
            "9A 7D 72 12 0D CB 21 59 8F FD 56 69 C5 7E DF 96 " +
            "A6 31 51 3E 7B C5 8D B0 25 F0 83 57 3B 06 A3 21 " +
            "A4 E6 63 83 B5 CB 09 6F 86 29 82 0C D7 FD C3 C7 " +
            "6E 71 2A A4 BF 49 2D D2 BC 3E 6F 65 F6 E2 1E CF " +
            "F5 A7 DF 19 B8 26 73 00 33 57 5E 08 B5 A7 85 6C " +
            "F5 95 1F F8 97 AD EB D9 88 C6 A7 4F 2A 03 31 01 " +
            "87 8C 7E 95 52 E2 A1 CE D5 36 B8 09 12 D0 5A CD " +
            "8A E8 0A 84 F4 BC DD D6 C8 C6 33 7E 37 57 3F 02 " +
            "4F 25 39 93 86 DA A7 B2 E1 BE 17 08 80 F5 F1 91 " +
            "27 BA B3 7E F2 B4 2B 97 80 0C FE AF BE 26 C6 62 " +
            "2C E6 86 F3 0A 4A B1 E6 35 E7 A9 57 79 32 FD 73 " +
            "DE 03 2C EB 3C 15 B6 AF EE EE 1E B6 62 DD 57 9D " +
            "DD 8A EA F8 C4 76 69 A0",
            // After Iota 
            "90 7D 72 92 0D CB 21 D9 8F FD 56 69 C5 7E DF 96 " +
            "A6 31 51 3E 7B C5 8D B0 25 F0 83 57 3B 06 A3 21 " +
            "A4 E6 63 83 B5 CB 09 6F 86 29 82 0C D7 FD C3 C7 " +
            "6E 71 2A A4 BF 49 2D D2 BC 3E 6F 65 F6 E2 1E CF " +
            "F5 A7 DF 19 B8 26 73 00 33 57 5E 08 B5 A7 85 6C " +
            "F5 95 1F F8 97 AD EB D9 88 C6 A7 4F 2A 03 31 01 " +
            "87 8C 7E 95 52 E2 A1 CE D5 36 B8 09 12 D0 5A CD " +
            "8A E8 0A 84 F4 BC DD D6 C8 C6 33 7E 37 57 3F 02 " +
            "4F 25 39 93 86 DA A7 B2 E1 BE 17 08 80 F5 F1 91 " +
            "27 BA B3 7E F2 B4 2B 97 80 0C FE AF BE 26 C6 62 " +
            "2C E6 86 F3 0A 4A B1 E6 35 E7 A9 57 79 32 FD 73 " +
            "DE 03 2C EB 3C 15 B6 AF EE EE 1E B6 62 DD 57 9D " +
            "DD 8A EA F8 C4 76 69 A0",
            // Round #20
            // After Theta
            "F7 B2 C6 46 DD F2 EC C7 CD 61 FA D8 73 B2 B2 AB " +
            "2C D2 88 66 D7 2B F9 F9 07 70 BF CA 44 22 2B 81 " +
            "66 11 9F DA 55 5E F0 CE E1 E6 36 D8 07 C4 0E D9 " +
            "2C ED 86 15 09 85 40 EF 36 DD B6 3D 5A 0C 6A 86 " +
            "D7 27 E3 84 C7 02 FB A0 F1 A0 A2 51 55 32 7C CD " +
            "92 5A AB 2C 47 94 26 C7 CA 5A 0B FE 9C CF 5C 3C " +
            "0D 6F A7 CD FE 0C D5 87 F7 B6 84 94 6D F4 D2 6D " +
            "48 1F F6 DD 14 29 24 77 AF 09 87 AA E7 6E F2 1C " +
            "0D B9 95 22 30 16 CA 8F 6B 5D CE 50 2C 1B 85 D8 " +
            "05 3A 8F E3 8D 90 A3 37 42 FB 02 F6 5E B3 3F C3 " +
            "4B 29 32 27 DA 73 7C F8 77 7B 05 E6 CF FE 90 4E " +
            "54 E0 F5 B3 90 FB C2 E6 CC 6E 22 2B 1D F9 DF 3D " +
            "1F 7D 16 A1 24 E3 90 01",
            // After Rho
            "F7 B2 C6 46 DD F2 EC C7 9B C3 F4 B1 E7 64 65 57 " +
            "8B 34 A2 D9 F5 4A 7E 3E 24 B2 12 78 00 F7 AB 4C " +
            "F2 82 77 36 8B F8 D4 AE 7D 40 EC 90 1D 6E 6E 83 " +
            "58 91 50 08 F4 CE D2 6E A1 4D B7 6D 8F 16 83 9A " +
            "93 71 C2 63 81 7D D0 EB C3 D7 1C 0F 2A 1A 55 25 " +
            "96 D4 5A 65 39 A2 34 39 F1 28 6B 2D F8 73 3E 73 " +
            "6D F6 67 A8 3E 6C 78 3B E8 A5 DB EE 6D 09 29 DB " +
            "6E 8A 14 92 3B A4 0F FB 55 CF DD E4 39 5E 13 0E " +
            "52 04 C6 42 F9 B1 21 B7 42 EC B5 2E 67 28 96 8D " +
            "72 F4 A6 40 E7 71 BC 11 C3 42 FB 02 F6 5E B3 3F " +
            "F1 E1 2F A5 C8 9C 68 CF DD ED 15 98 3F FB 43 3A " +
            "0A BC 7E 16 72 5F D8 9C 6E 22 2B 1D F9 DF 3D CC " +
            "64 C0 47 9F 45 28 C9 38",
            // After Pi
            "F7 B2 C6 46 DD F2 EC C7 58 91 50 08 F4 CE D2 6E " +
            "6D F6 67 A8 3E 6C 78 3B 72 F4 A6 40 E7 71 BC 11 " +
            "64 C0 47 9F 45 28 C9 38 24 B2 12 78 00 F7 AB 4C " +
            "C3 D7 1C 0F 2A 1A 55 25 96 D4 5A 65 39 A2 34 39 " +
            "52 04 C6 42 F9 B1 21 B7 0A BC 7E 16 72 5F D8 9C " +
            "9B C3 F4 B1 E7 64 65 57 A1 4D B7 6D 8F 16 83 9A " +
            "E8 A5 DB EE 6D 09 29 DB C3 42 FB 02 F6 5E B3 3F " +
            "F1 E1 2F A5 C8 9C 68 CF F2 82 77 36 8B F8 D4 AE " +
            "7D 40 EC 90 1D 6E 6E 83 F1 28 6B 2D F8 73 3E 73 " +
            "42 EC B5 2E 67 28 96 8D 6E 22 2B 1D F9 DF 3D CC " +
            "8B 34 A2 D9 F5 4A 7E 3E 93 71 C2 63 81 7D D0 EB " +
            "6E 8A 14 92 3B A4 0F FB 55 CF DD E4 39 5E 13 0E " +
            "DD ED 15 98 3F FB 43 3A",
            // After Chi
            "D2 D4 E1 E6 D7 D2 C4 D6 4A 91 D0 48 35 DF 56 6E " +
            "69 F6 26 37 3E 64 39 13 E1 C6 26 00 7F A3 98 D6 " +
            "6C C1 57 97 65 24 DB 10 30 B2 50 18 11 57 8B 54 " +
            "83 D7 98 0D EA 0B 54 A3 9E 6C 62 71 3B EC EC 31 " +
            "76 06 C6 2A F9 11 02 F7 C9 F9 72 11 58 57 8C BD " +
            "D3 63 BC 33 87 6D 4D 16 A2 0F 97 6D 1D 40 11 BE " +
            "D8 04 DF 4B 65 89 61 1B C9 40 2B 12 D1 3E B6 2F " +
            "D1 ED 2C E9 C0 8E EA 47 72 AA 74 1B 6B E9 C4 DE " +
            "7F 84 78 92 1A 66 EE 0F DD 2A 61 3C 60 A4 17 33 " +
            "D2 6C E1 0C 65 08 56 AF 63 62 A3 9D ED D9 17 CD " +
            "E7 BE B6 49 CF CA 71 2E 82 34 0B 07 81 27 C0 EF " +
            "E6 AA 14 8A 3D 05 4F CB 57 DF 7F A5 F9 5E 2F 0A " +
            "CD AC 55 BA 3F CE C3 FB",
            // After Iota 
            "53 54 E1 66 D7 D2 C4 56 4A 91 D0 48 35 DF 56 6E " +
            "69 F6 26 37 3E 64 39 13 E1 C6 26 00 7F A3 98 D6 " +
            "6C C1 57 97 65 24 DB 10 30 B2 50 18 11 57 8B 54 " +
            "83 D7 98 0D EA 0B 54 A3 9E 6C 62 71 3B EC EC 31 " +
            "76 06 C6 2A F9 11 02 F7 C9 F9 72 11 58 57 8C BD " +
            "D3 63 BC 33 87 6D 4D 16 A2 0F 97 6D 1D 40 11 BE " +
            "D8 04 DF 4B 65 89 61 1B C9 40 2B 12 D1 3E B6 2F " +
            "D1 ED 2C E9 C0 8E EA 47 72 AA 74 1B 6B E9 C4 DE " +
            "7F 84 78 92 1A 66 EE 0F DD 2A 61 3C 60 A4 17 33 " +
            "D2 6C E1 0C 65 08 56 AF 63 62 A3 9D ED D9 17 CD " +
            "E7 BE B6 49 CF CA 71 2E 82 34 0B 07 81 27 C0 EF " +
            "E6 AA 14 8A 3D 05 4F CB 57 DF 7F A5 F9 5E 2F 0A " +
            "CD AC 55 BA 3F CE C3 FB",
            // Round #21
            // After Theta
            "A4 BC 47 55 4B 92 D6 AC 46 3C C3 20 AB 54 38 09 " +
            "48 68 20 A8 F0 04 AF D6 40 EF 36 2A 1C D7 A7 AF " +
            "FC D0 9D 39 64 69 E1 72 C7 5A F6 2B 8D 17 99 AE " +
            "8F 7A 8B 65 74 80 3A C4 BF F2 64 EE F5 8C 7A F4 " +
            "D7 2F D6 00 9A 65 3D 8E 59 E8 B8 BF 59 1A B6 DF " +
            "24 8B 1A 00 1B 2D 5F EC AE A2 84 05 83 CB 7F D9 " +
            "F9 9A D9 D4 AB E9 F7 DE 68 69 3B 38 B2 4A 89 56 " +
            "41 FC E6 47 C1 C3 D0 25 85 42 D2 28 F7 A9 D6 24 " +
            "73 29 6B FA 84 ED 80 68 FC B4 67 A3 AE C4 81 F6 " +
            "73 45 F1 26 06 7C 69 D6 F3 73 69 33 EC 94 2D AF " +
            "10 56 10 7A 53 8A 63 D4 8E 99 18 6F 1F AC AE 88 " +
            "C7 34 12 15 F3 65 D9 0E F6 F6 6F 8F 9A 2A 10 73 " +
            "5D BD 9F 14 3E 83 F9 99",
            // After Rho
            "A4 BC 47 55 4B 92 D6 AC 8C 78 86 41 56 A9 70 12 " +
            "12 1A 08 2A 3C C1 AB 35 71 7D FA 0A F4 6E A3 C2 " +
            "4B 0B 97 E3 87 EE CC 21 D2 78 91 E9 7A AC 65 BF " +
            "58 46 07 A8 43 FC A8 B7 FD AF 3C 99 7B 3D A3 1E " +
            "17 6B 00 CD B2 1E C7 EB 61 FB 9D 85 8E FB 9B A5 " +
            "27 59 D4 00 D8 68 F9 62 65 BB 8A 12 16 0C 2E FF " +
            "A6 5E 4D BF F7 CE D7 CC 95 12 AD D0 D2 76 70 64 " +
            "A3 E0 61 E8 92 20 7E F3 51 EE 53 AD 49 0A 85 A4 " +
            "4D 9F B0 1D 10 6D 2E 65 40 7B 7E DA B3 51 57 E2 " +
            "2F CD 7A AE 28 DE C4 80 AF F3 73 69 33 EC 94 2D " +
            "8E 51 43 58 41 E8 4D 29 3A 66 62 BC 7D B0 BA 22 " +
            "98 46 A2 62 BE 2C DB E1 F6 6F 8F 9A 2A 10 73 F6 " +
            "7E 66 57 EF 27 85 CF 60",
            // After Pi
            "A4 BC 47 55 4B 92 D6 AC 58 46 07 A8 43 FC A8 B7 " +
            "A6 5E 4D BF F7 CE D7 CC 2F CD 7A AE 28 DE C4 80 " +
            "7E 66 57 EF 27 85 CF 60 71 7D FA 0A F4 6E A3 C2 " +
            "61 FB 9D 85 8E FB 9B A5 27 59 D4 00 D8 68 F9 62 " +
            "4D 9F B0 1D 10 6D 2E 65 98 46 A2 62 BE 2C DB E1 " +
            "8C 78 86 41 56 A9 70 12 FD AF 3C 99 7B 3D A3 1E " +
            "95 12 AD D0 D2 76 70 64 AF F3 73 69 33 EC 94 2D " +
            "8E 51 43 58 41 E8 4D 29 4B 0B 97 E3 87 EE CC 21 " +
            "D2 78 91 E9 7A AC 65 BF 65 BB 8A 12 16 0C 2E FF " +
            "40 7B 7E DA B3 51 57 E2 F6 6F 8F 9A 2A 10 73 F6 " +
            "12 1A 08 2A 3C C1 AB 35 17 6B 00 CD B2 1E C7 EB " +
            "A3 E0 61 E8 92 20 7E F3 51 EE 53 AD 49 0A 85 A4 " +
            "3A 66 62 BC 7D B0 BA 22",
            // After Chi
            "02 A4 0F 42 FF 90 81 E4 51 C7 35 A8 4B EC A8 B7 " +
            "F6 7C 48 FE F0 CF DC AC AF 55 7A BE 60 CC D4 0C " +
            "26 24 57 47 27 E9 E7 73 77 7D BA 0A A4 6E C3 80 " +
            "29 7D BD 98 8E FE 9D A0 B7 19 D6 62 76 68 28 E2 " +
            "2C A6 E8 15 50 2F 0E 67 98 C4 A7 E7 B4 BD C3 C4 " +
            "8C 68 07 01 D6 EB 20 72 D7 4E 6E B0 5A B5 27 17 " +
            "95 12 AD C0 92 76 39 64 AF DB F7 68 25 ED A4 3F " +
            "FF D6 7B C0 68 FC CE 25 6E 88 9D F1 83 EE C6 61 " +
            "D2 38 E5 21 DB FD 34 BF D3 BF 0B 12 1E 0C 0E EB " +
            "49 7B 6E BB 36 BF DB E3 66 1F 8F 92 52 10 52 68 " +
            "B2 9A 69 0A 3C E1 93 25 47 65 12 C8 FB 14 46 EF " +
            "89 E0 41 F8 A6 90 44 F1 51 F6 5B AF 49 4B 84 B1 " +
            "3F 07 62 79 FF AE FE E8",
            // After Iota 
            "82 24 0F 42 FF 90 81 64 51 C7 35 A8 4B EC A8 B7 " +
            "F6 7C 48 FE F0 CF DC AC AF 55 7A BE 60 CC D4 0C " +
            "26 24 57 47 27 E9 E7 73 77 7D BA 0A A4 6E C3 80 " +
            "29 7D BD 98 8E FE 9D A0 B7 19 D6 62 76 68 28 E2 " +
            "2C A6 E8 15 50 2F 0E 67 98 C4 A7 E7 B4 BD C3 C4 " +
            "8C 68 07 01 D6 EB 20 72 D7 4E 6E B0 5A B5 27 17 " +
            "95 12 AD C0 92 76 39 64 AF DB F7 68 25 ED A4 3F " +
            "FF D6 7B C0 68 FC CE 25 6E 88 9D F1 83 EE C6 61 " +
            "D2 38 E5 21 DB FD 34 BF D3 BF 0B 12 1E 0C 0E EB " +
            "49 7B 6E BB 36 BF DB E3 66 1F 8F 92 52 10 52 68 " +
            "B2 9A 69 0A 3C E1 93 25 47 65 12 C8 FB 14 46 EF " +
            "89 E0 41 F8 A6 90 44 F1 51 F6 5B AF 49 4B 84 B1 " +
            "3F 07 62 79 FF AE FE E8",
            // Round #22
            // After Theta
            "EE 58 4A 1B D7 1B 07 D6 E8 B5 81 76 20 6D 91 04 " +
            "A4 9F F8 39 9A 75 FF F0 11 21 CF 1E 61 AD DF 18 " +
            "59 C6 8B F4 28 27 A8 D1 1B 01 FF 53 8C E5 45 32 " +
            "90 0F 09 46 E5 7F A4 13 E5 FA 66 A5 1C D2 0B BE " +
            "92 D2 5D B5 51 4E 05 73 E7 26 7B 54 BB 73 8C 66 " +
            "E0 14 42 58 FE 60 A6 C0 6E 3C DA 6E 31 34 1E A4 " +
            "C7 F1 1D 07 F8 CC 1A 38 11 AF 42 C8 24 8C AF 2B " +
            "80 34 A7 73 67 32 81 87 02 F4 D8 A8 AB 65 40 D3 " +
            "6B 4A 51 FF B0 7C 0D 0C 81 5C BB D5 74 B6 2D B7 " +
            "F7 0F DB 1B 37 DE D0 F7 19 FD 53 21 5D DE 1D CA " +
            "DE E6 2C 53 14 6A 15 97 FE 17 A6 16 90 95 7F 5C " +
            "DB 03 F1 3F CC 2A 67 AD EF 82 EE 0F 48 2A 8F A5 " +
            "40 E5 BE CA F0 60 B1 4A",
            // After Rho
            "EE 58 4A 1B D7 1B 07 D6 D0 6B 03 ED 40 DA 22 09 " +
            "E9 27 7E 8E 66 DD 3F 3C D6 FA 8D 11 11 F2 EC 11 " +
            "39 41 8D CE 32 5E A4 47 C5 58 5E 24 B3 11 F0 3F " +
            "60 54 FE 47 3A 01 F9 90 6F B9 BE 59 29 87 F4 82 " +
            "E9 AE DA 28 A7 82 39 49 C7 68 76 6E B2 47 B5 3B " +
            "06 A7 10 C2 F2 07 33 05 90 BA F1 68 BB C5 D0 78 " +
            "38 C0 67 D6 C0 39 8E EF 18 5F 57 22 5E 85 90 49 " +
            "B9 33 99 C0 43 40 9A D3 51 57 CB 80 A6 05 E8 B1 " +
            "EA 1F 96 AF 81 61 4D 29 96 DB 40 AE DD 6A 3A DB " +
            "1B FA FE FE 61 7B E3 C6 CA 19 FD 53 21 5D DE 1D " +
            "55 5C 7A 9B B3 4C 51 A8 F9 5F 98 5A 40 56 FE 71 " +
            "7B 20 FE 87 59 E5 AC 75 82 EE 0F 48 2A 8F A5 EF " +
            "AC 12 50 B9 AF 32 3C 58",
            // After Pi
            "EE 58 4A 1B D7 1B 07 D6 60 54 FE 47 3A 01 F9 90 " +
            "38 C0 67 D6 C0 39 8E EF 1B FA FE FE 61 7B E3 C6 " +
            "AC 12 50 B9 AF 32 3C 58 D6 FA 8D 11 11 F2 EC 11 " +
            "C7 68 76 6E B2 47 B5 3B 06 A7 10 C2 F2 07 33 05 " +
            "EA 1F 96 AF 81 61 4D 29 7B 20 FE 87 59 E5 AC 75 " +
            "D0 6B 03 ED 40 DA 22 09 6F B9 BE 59 29 87 F4 82 " +
            "18 5F 57 22 5E 85 90 49 CA 19 FD 53 21 5D DE 1D " +
            "55 5C 7A 9B B3 4C 51 A8 39 41 8D CE 32 5E A4 47 " +
            "C5 58 5E 24 B3 11 F0 3F 90 BA F1 68 BB C5 D0 78 " +
            "96 DB 40 AE DD 6A 3A DB 82 EE 0F 48 2A 8F A5 EF " +
            "E9 27 7E 8E 66 DD 3F 3C E9 AE DA 28 A7 82 39 49 " +
            "B9 33 99 C0 43 40 9A D3 51 57 CB 80 A6 05 E8 B1 " +
            "F9 5F 98 5A 40 56 FE 71",
            // After Chi
            "F6 D8 4B 8B 17 23 01 B9 63 6E 66 6F 1B 43 98 90 " +
            "9C C0 67 D7 4E 39 92 F7 59 B2 F4 FC 31 72 E0 40 " +
            "AC 16 E4 FD 87 32 C4 58 D6 7D 8D 91 51 F2 EE 15 " +
            "2F 70 F0 43 B3 27 F9 13 17 87 78 C2 AA 83 93 51 " +
            "6E C5 97 BF 81 73 0D 29 7A 20 8C E9 FB E0 BD 5F " +
            "C0 2D 42 CF 16 DA 22 40 AD B9 16 08 08 DF BA 96 " +
            "0D 1B 55 AA CC 85 91 E9 4A 3A FC 37 61 CF FC 1C " +
            "7A CC C6 8B 9A 49 85 2A 29 E3 2C 86 3A 9A A4 07 " +
            "C3 19 5E A2 F7 3B DA BC 90 9E FE 28 99 40 55 5C " +
            "AF DA C0 28 CD 3A 3A DB 46 F6 5D 68 AB 8E F5 D7 " +
            "F9 36 7F 4E 26 9D BD AE A9 EA 98 28 03 87 59 69 " +
            "11 3B 89 9A 03 12 8C 93 51 77 AD 04 80 8C E9 BD " +
            "F9 D7 18 7A C1 54 FE 30",
            // After Iota 
            "F7 D8 4B 0B 17 23 01 B9 63 6E 66 6F 1B 43 98 90 " +
            "9C C0 67 D7 4E 39 92 F7 59 B2 F4 FC 31 72 E0 40 " +
            "AC 16 E4 FD 87 32 C4 58 D6 7D 8D 91 51 F2 EE 15 " +
            "2F 70 F0 43 B3 27 F9 13 17 87 78 C2 AA 83 93 51 " +
            "6E C5 97 BF 81 73 0D 29 7A 20 8C E9 FB E0 BD 5F " +
            "C0 2D 42 CF 16 DA 22 40 AD B9 16 08 08 DF BA 96 " +
            "0D 1B 55 AA CC 85 91 E9 4A 3A FC 37 61 CF FC 1C " +
            "7A CC C6 8B 9A 49 85 2A 29 E3 2C 86 3A 9A A4 07 " +
            "C3 19 5E A2 F7 3B DA BC 90 9E FE 28 99 40 55 5C " +
            "AF DA C0 28 CD 3A 3A DB 46 F6 5D 68 AB 8E F5 D7 " +
            "F9 36 7F 4E 26 9D BD AE A9 EA 98 28 03 87 59 69 " +
            "11 3B 89 9A 03 12 8C 93 51 77 AD 04 80 8C E9 BD " +
            "F9 D7 18 7A C1 54 FE 30",
            // Round #23
            // After Theta
            "F3 AA 2C DA 32 6C 46 F3 5D C1 CA E8 33 94 DE D5 " +
            "11 55 C4 C8 22 CF 4E 10 79 FD 1E EA 9A 9C 47 55 " +
            "4D 4C B8 9E 82 52 AE C0 D2 0F EA 40 74 BD A9 5F " +
            "11 DF 5C C4 9B F0 BF 56 9A 12 DB DD C6 75 4F B6 " +
            "4E 8A 7D A9 2A 9D AA 3C 9B 7A D0 8A FE 80 D7 C7 " +
            "C4 5F 25 1E 33 95 65 0A 93 16 BA 8F 20 08 FC D3 " +
            "80 8E F6 B5 A0 73 4D 0E 6A 75 16 21 CA 21 5B 09 " +
            "9B 96 9A E8 9F 29 EF B2 2D 91 4B 57 1F D5 E3 4D " +
            "FD B6 F2 25 DF EC 9C F9 1D 0B 5D 37 F5 B6 89 BB " +
            "8F 95 2A 3E 66 D4 9D CE A7 AC 01 0B AE EE 9F 4F " +
            "FD 44 18 9F 03 D2 FA E4 97 45 34 AF 2B 50 1F 2C " +
            "9C AE 2A 85 6F E4 50 74 71 38 47 12 2B 62 4E A8 " +
            "18 8D 44 19 C4 34 94 A8",
            // After Rho
            "F3 AA 2C DA 32 6C 46 F3 BB 82 95 D1 67 28 BD AB " +
            "44 15 31 B2 C8 B3 13 44 C9 79 54 95 D7 EF A1 AE " +
            "94 72 05 6E 62 C2 F5 14 44 D7 9B FA 25 FD A0 0E " +
            "45 BC 09 FF 6B 15 F1 CD AD A6 C4 76 B7 71 DD 93 " +
            "C5 BE 54 95 4E 55 1E 27 78 7D BC A9 07 AD E8 0F " +
            "20 FE 2A F1 98 A9 2C 53 4F 4F 5A E8 3E 82 20 F0 " +
            "AF 05 9D 6B 72 00 74 B4 43 B6 12 D4 EA 2C 42 94 " +
            "F4 CF 94 77 D9 4D 4B 4D AE 3E AA C7 9B 5A 22 97 " +
            "BE E4 9B 9D 33 BF DF 56 C4 DD 8E 85 AE 9B 7A DB " +
            "BA D3 F9 B1 52 C5 C7 8C 4F A7 AC 01 0B AE EE 9F " +
            "EB 93 F7 13 61 7C 0E 48 5C 16 D1 BC AE 40 7D B0 " +
            "D3 55 A5 F0 8D 1C 8A 8E 38 47 12 2B 62 4E A8 71 " +
            "25 2A 46 23 51 06 31 0D",
            // After Pi
            "F3 AA 2C DA 32 6C 46 F3 45 BC 09 FF 6B 15 F1 CD " +
            "AF 05 9D 6B 72 00 74 B4 BA D3 F9 B1 52 C5 C7 8C " +
            "25 2A 46 23 51 06 31 0D C9 79 54 95 D7 EF A1 AE " +
            "78 7D BC A9 07 AD E8 0F 20 FE 2A F1 98 A9 2C 53 " +
            "BE E4 9B 9D 33 BF DF 56 D3 55 A5 F0 8D 1C 8A 8E " +
            "BB 82 95 D1 67 28 BD AB AD A6 C4 76 B7 71 DD 93 " +
            "43 B6 12 D4 EA 2C 42 94 4F A7 AC 01 0B AE EE 9F " +
            "EB 93 F7 13 61 7C 0E 48 94 72 05 6E 62 C2 F5 14 " +
            "44 D7 9B FA 25 FD A0 0E 4F 4F 5A E8 3E 82 20 F0 " +
            "C4 DD 8E 85 AE 9B 7A DB 38 47 12 2B 62 4E A8 71 " +
            "44 15 31 B2 C8 B3 13 44 C5 BE 54 95 4E 55 1E 27 " +
            "F4 CF 94 77 D9 4D 4B 4D AE 3E AA C7 9B 5A 22 97 " +
            "5C 16 D1 BC AE 40 7D B0",
            // After Chi
            "59 AB B8 DA 22 6C 42 C3 55 6E 69 6F 6B D0 72 C5 " +
            "AA 2D 9B 69 73 02 44 B5 68 53 D1 69 70 AD 81 7E " +
            "21 3E 47 06 18 17 80 01 C9 FB 56 C5 4F EF A5 FE " +
            "E6 7D 2D A5 24 BB 3B 0B 61 EF 0E 91 14 A9 2C DB " +
            "B6 CC CB 98 61 5C FE 76 E3 51 0D D8 8D 1C C2 8F " +
            "F9 92 87 51 2F 24 BF AF A1 A7 68 77 B6 F3 71 98 " +
            "E3 A6 41 C6 8A 7C 42 D4 5F A7 AC C1 0D AE 5F 3C " +
            "EF B7 B7 35 F1 2D 4E 58 9F 7A 45 6E 78 C0 F5 E4 " +
            "C4 47 1F FF A5 E4 FA 05 77 4D 4A C2 7E C6 A0 D0 " +
            "40 ED 8B C1 AE 1B 2F DF 78 C2 88 BB 67 73 A8 7B " +
            "74 54 B1 D0 59 BB 52 0C CF 8E 7E 15 4C 47 3E B5 " +
            "A4 CF C5 4F FD 4D 16 6D AE 3F 8A C5 DB E9 20 D3 " +
            "DD BC 95 B9 A8 04 71 93",
            // After Iota 
            "51 2B B8 5A 22 6C 42 43 55 6E 69 6F 6B D0 72 C5 " +
            "AA 2D 9B 69 73 02 44 B5 68 53 D1 69 70 AD 81 7E " +
            "21 3E 47 06 18 17 80 01 C9 FB 56 C5 4F EF A5 FE " +
            "E6 7D 2D A5 24 BB 3B 0B 61 EF 0E 91 14 A9 2C DB " +
            "B6 CC CB 98 61 5C FE 76 E3 51 0D D8 8D 1C C2 8F " +
            "F9 92 87 51 2F 24 BF AF A1 A7 68 77 B6 F3 71 98 " +
            "E3 A6 41 C6 8A 7C 42 D4 5F A7 AC C1 0D AE 5F 3C " +
            "EF B7 B7 35 F1 2D 4E 58 9F 7A 45 6E 78 C0 F5 E4 " +
            "C4 47 1F FF A5 E4 FA 05 77 4D 4A C2 7E C6 A0 D0 " +
            "40 ED 8B C1 AE 1B 2F DF 78 C2 88 BB 67 73 A8 7B " +
            "74 54 B1 D0 59 BB 52 0C CF 8E 7E 15 4C 47 3E B5 " +
            "A4 CF C5 4F FD 4D 16 6D AE 3F 8A C5 DB E9 20 D3 " +
            "DD BC 95 B9 A8 04 71 93",
            // Xor'd state (in bytes)                        " +
            "51 2B B8 5A 22 6C 42 43 55 6E 69 6F 6B D0 72 C5 " +
            "AA 2D 9B 69 73 02 44 B5 68 53 D1 69 70 AD 81 7E " +
            "21 3E 47 06 18 17 80 01 C9 FB 56 C5 4F EF A5 FE " +
            "E6 7D 2D A5 24 BB 3B 0B 61 EF 0E 91 14 A9 2C DB " +
            "B6 CC CB 98 61 5C FE 76 E3 51 0D D8 8D 1C C2 8F " +
            "F9 92 87 51 2F 24 BF AF A1 A7 68 77 B6 F3 71 98 " +
            "E3 A6 41 C6 8A 7C 42 D4 5F A7 AC C1 0D AE 5F 3C " +
            "EF B7 B7 35 F1 2D 4E 58 9F 7A 45 6E 78 C0 F5 E4 " +
            "C4 47 1F FF A5 E4 FA 05 77 4D 4A C2 7E C6 A0 D0 " +
            "40 ED 8B C1 AE 1B 2F DF 78 C2 88 BB 67 73 A8 7B " +
            "74 54 B1 D0 59 BB 52 0C CF 8E 7E 15 4C 47 3E B5 " +
            "A4 CF C5 4F FD 4D 16 6D AE 3F 8A C5 DB E9 20 D3 " +
            "DD BC 95 B9 A8 04 71 93",
            // Round #0
            // After Theta
            "EA 77 C2 1D A9 4B 6F B0 29 CF 43 79 D5 B4 B5 30 " +
            "6D 84 B9 57 B1 63 E7 23 83 F8 4B 09 49 52 B7 04 " +
            "5B 0D CA D3 B7 02 50 CC 72 A7 2C 82 C4 C8 88 0D " +
            "9A DC 07 B3 9A DF FC FE A6 46 2C AF D6 C8 8F 4D " +
            "5D 67 51 F8 58 A3 C8 0C 99 62 80 0D 22 09 12 42 " +
            "42 CE FD 16 A4 03 92 5C DD 06 42 61 08 97 B6 6D " +
            "24 0F 63 F8 48 1D E1 42 B4 0C 36 A1 34 51 69 46 " +
            "95 84 3A E0 5E 38 9E 95 24 26 3F 29 F3 E7 D8 17 " +
            "B8 E6 35 E9 1B 80 3D F0 B0 E4 68 FC BC A7 03 46 " +
            "AB 46 11 A1 97 E4 19 A5 02 F1 05 6E C8 66 78 B6 " +
            "CF 08 CB 97 D2 9C 7F FF B3 2F 54 03 F2 23 F9 40 " +
            "63 66 E7 71 3F 2C B5 FB 45 94 10 A5 E2 16 16 A9 " +
            "A7 8F 18 6C 07 11 A1 5E",
            // After Rho
            "EA 77 C2 1D A9 4B 6F B0 52 9E 87 F2 AA 69 6B 61 " +
            "1B 61 EE 55 EC D8 F9 48 24 75 4B 30 88 BF 94 90 " +
            "15 80 62 DE 6A 50 9E BE 48 8C 8C D8 20 77 CA 22 " +
            "30 AB F9 CD EF AF C9 7D 93 A9 11 CB AB 35 F2 63 " +
            "B3 28 7C AC 51 64 86 AE 20 21 94 29 06 D8 20 92 " +
            "12 72 EE B7 20 1D 90 E4 B6 75 1B 08 85 21 5C DA " +
            "C3 47 EA 08 17 22 79 18 A2 D2 8C 68 19 6C 42 69 " +
            "70 2F 1C CF CA 4A 42 1D 52 E6 CF B1 2F 48 4C 7E " +
            "26 7D 03 B0 07 1E D7 BC 01 23 58 72 34 7E DE D3 " +
            "3C A3 74 D5 28 22 F4 92 B6 02 F1 05 6E C8 66 78 " +
            "FE FD 3F 23 2C 5F 4A 73 CD BE 50 0D C8 8F E4 03 " +
            "CC EC 3C EE 87 A5 76 7F 94 10 A5 E2 16 16 A9 45 " +
            "A8 D7 E9 23 06 DB 41 44",
            // After Pi
            "EA 77 C2 1D A9 4B 6F B0 30 AB F9 CD EF AF C9 7D " +
            "C3 47 EA 08 17 22 79 18 3C A3 74 D5 28 22 F4 92 " +
            "A8 D7 E9 23 06 DB 41 44 24 75 4B 30 88 BF 94 90 " +
            "20 21 94 29 06 D8 20 92 12 72 EE B7 20 1D 90 E4 " +
            "26 7D 03 B0 07 1E D7 BC CC EC 3C EE 87 A5 76 7F " +
            "52 9E 87 F2 AA 69 6B 61 93 A9 11 CB AB 35 F2 63 " +
            "A2 D2 8C 68 19 6C 42 69 B6 02 F1 05 6E C8 66 78 " +
            "FE FD 3F 23 2C 5F 4A 73 15 80 62 DE 6A 50 9E BE " +
            "48 8C 8C D8 20 77 CA 22 B6 75 1B 08 85 21 5C DA " +
            "01 23 58 72 34 7E DE D3 94 10 A5 E2 16 16 A9 45 " +
            "1B 61 EE 55 EC D8 F9 48 B3 28 7C AC 51 64 86 AE " +
            "70 2F 1C CF CA 4A 42 1D 52 E6 CF B1 2F 48 4C 7E " +
            "CD BE 50 0D C8 8F E4 03",
            // After Chi
            "29 33 C0 1D B9 4B 5F B0 0C 0B ED 18 C7 AF 4D FF " +
            "43 13 63 2A 11 FB 78 5C 7E 83 76 C9 81 22 DA 22 " +
            "B8 5F D0 E3 40 7F C1 09 36 27 21 A6 A8 BA 04 F4 " +
            "04 2C 95 29 01 DA 67 8A DA F2 D2 F9 A0 BC B0 A7 " +
            "06 6C 40 A0 0F 04 57 3C CC EC A8 E7 81 E5 56 7D " +
            "72 CC 0B D2 BA 21 6B 69 87 A9 60 CE CD B5 D6 73 " +
            "EA 2F 82 4A 19 7B 4A 6A B6 00 71 D5 EC E8 47 78 " +
            "7F DC 2F 2A 2D 4B DA 71 A3 F1 71 DE EF 50 8A 66 " +
            "49 8E CC AA 10 29 48 23 22 65 BE 88 87 21 7D DE " +
            "00 A3 1A 6E 5C 3E C8 69 DC 1C 29 E2 16 31 E9 45 " +
            "5B 66 EE 16 66 D2 B9 59 B1 E8 BF 9C 74 64 8A CC " +
            "FD 37 0C C3 0A CD E2 1C 40 A7 61 E1 0B 18 55 36 " +
            "6D B6 40 A5 D9 AB E2 A5",
            // After Iota 
            "28 33 C0 1D B9 4B 5F B0 0C 0B ED 18 C7 AF 4D FF " +
            "43 13 63 2A 11 FB 78 5C 7E 83 76 C9 81 22 DA 22 " +
            "B8 5F D0 E3 40 7F C1 09 36 27 21 A6 A8 BA 04 F4 " +
            "04 2C 95 29 01 DA 67 8A DA F2 D2 F9 A0 BC B0 A7 " +
            "06 6C 40 A0 0F 04 57 3C CC EC A8 E7 81 E5 56 7D " +
            "72 CC 0B D2 BA 21 6B 69 87 A9 60 CE CD B5 D6 73 " +
            "EA 2F 82 4A 19 7B 4A 6A B6 00 71 D5 EC E8 47 78 " +
            "7F DC 2F 2A 2D 4B DA 71 A3 F1 71 DE EF 50 8A 66 " +
            "49 8E CC AA 10 29 48 23 22 65 BE 88 87 21 7D DE " +
            "00 A3 1A 6E 5C 3E C8 69 DC 1C 29 E2 16 31 E9 45 " +
            "5B 66 EE 16 66 D2 B9 59 B1 E8 BF 9C 74 64 8A CC " +
            "FD 37 0C C3 0A CD E2 1C 40 A7 61 E1 0B 18 55 36 " +
            "6D B6 40 A5 D9 AB E2 A5",
            // Round #1
            // After Theta
            "7D 26 29 E6 45 1A 64 87 C0 7D 9B 1C AE 5D 75 4B " +
            "28 2C 71 85 14 A6 E9 C7 A7 94 8A C9 E2 64 4B BB " +
            "1E 2B 06 92 30 33 90 14 63 32 C8 5D 54 EB 3F C3 " +
            "C8 5A E3 2D 68 28 5F 3E B1 CD C0 56 A5 E1 21 3C " +
            "DF 7B BC A0 6C 42 C6 A5 6A 98 7E 96 F1 A9 07 60 " +
            "27 D9 E2 29 46 70 50 5E 4B DF 16 CA A4 47 EE C7 " +
            "81 10 90 E5 1C 26 DB F1 6F 17 8D D5 8F AE D6 E1 " +
            "D9 A8 F9 5B 5D 07 8B 6C F6 E4 98 25 13 01 B1 51 " +
            "85 F8 BA AE 79 DB 70 97 49 5A AC 27 82 7C EC 45 " +
            "D9 B4 E6 6E 3F 78 59 F0 7A 68 FF 93 66 7D B8 58 " +
            "0E 73 07 ED 9A 83 82 6E 7D 9E C9 98 1D 96 B2 78 " +
            "96 08 1E 6C 0F 90 73 87 99 B0 9D E1 68 5E C4 AF " +
            "CB C2 96 D4 A9 E7 B3 B8",
            // After Rho
            "7D 26 29 E6 45 1A 64 87 80 FB 36 39 5C BB EA 96 " +
            "0A 4B 5C 21 85 69 FA 31 4E B6 B4 7B 4A A9 98 2C " +
            "99 81 A4 F0 58 31 90 84 45 B5 FE 33 3C 26 83 DC " +
            "DE 82 86 F2 E5 83 AC 35 4F 6C 33 B0 55 69 78 08 " +
            "3D 5E 50 36 21 E3 D2 EF 7A 00 A6 86 E9 67 19 9F " +
            "3A C9 16 4F 31 82 83 F2 1F 2F 7D 5B 28 93 1E B9 " +
            "2C E7 30 D9 8E 0F 84 80 5D AD C3 DF 2E 1A AB 1F " +
            "AD AE 83 45 B6 6C D4 FC 4B 26 02 62 A3 EC C9 31 " +
            "D7 35 6F 1B EE B2 10 5F F6 A2 24 2D D6 13 41 3E " +
            "2F 0B 3E 9B D6 DC ED 07 58 7A 68 FF 93 66 7D B8 " +
            "0A BA 39 CC 1D B4 6B 0E F5 79 26 63 76 58 CA E2 " +
            "12 C1 83 ED 01 72 EE D0 B0 9D E1 68 5E C4 AF 99 " +
            "2C EE B2 B0 25 75 EA F9",
            // After Pi
            "7D 26 29 E6 45 1A 64 87 DE 82 86 F2 E5 83 AC 35 " +
            "2C E7 30 D9 8E 0F 84 80 2F 0B 3E 9B D6 DC ED 07 " +
            "2C EE B2 B0 25 75 EA F9 4E B6 B4 7B 4A A9 98 2C " +
            "7A 00 A6 86 E9 67 19 9F 3A C9 16 4F 31 82 83 F2 " +
            "D7 35 6F 1B EE B2 10 5F 12 C1 83 ED 01 72 EE D0 " +
            "80 FB 36 39 5C BB EA 96 4F 6C 33 B0 55 69 78 08 " +
            "5D AD C3 DF 2E 1A AB 1F 58 7A 68 FF 93 66 7D B8 " +
            "0A BA 39 CC 1D B4 6B 0E 99 81 A4 F0 58 31 90 84 " +
            "45 B5 FE 33 3C 26 83 DC 1F 2F 7D 5B 28 93 1E B9 " +
            "F6 A2 24 2D D6 13 41 3E B0 9D E1 68 5E C4 AF 99 " +
            "0A 4B 5C 21 85 69 FA 31 3D 5E 50 36 21 E3 D2 EF " +
            "AD AE 83 45 B6 6C D4 FC 4B 26 02 62 A3 EC C9 31 " +
            "F5 79 26 63 76 58 CA E2",
            // After Chi
            "5D 43 19 EF 4F 16 64 07 DD 8A 88 F0 B5 53 C5 32 " +
            "2C 03 B0 F9 AF 2E 86 78 7E 0B 37 DD 96 D6 E9 01 " +
            "AE 6E 34 A0 85 F4 62 C9 4E 7F A4 32 5A 29 1A 4C " +
            "BF 34 CF 96 27 57 09 92 3A 09 96 AB 30 C2 6D 72 " +
            "9B 03 5B 09 A4 3B 00 73 22 C1 81 69 A0 34 EF 43 " +
            "90 7A F6 76 76 A9 69 81 4F 3E 1B 90 C4 0D 2C A8 " +
            "5F 2D D2 DF 22 8A A9 19 D8 3B 6E CE D3 6D FD 28 " +
            "45 BE 38 4C 1C F4 7B 06 83 8B A5 B8 58 A0 8C A5 " +
            "A5 35 FE 17 EA 26 C2 DA 1F 32 BC 1B 20 57 B0 38 " +
            "FF A2 20 BD D6 22 51 3A F4 A9 BB 6B 7A C2 AC C1 " +
            "8A EB DF 60 13 65 FE 21 7F 5E 50 14 20 63 DB EE " +
            "19 F7 A7 44 E2 7C D6 3E 41 24 5A 62 22 CD F9 20 " +
            "C0 6D 26 75 56 DA CA 2C",
            // After Iota 
            "DF C3 19 EF 4F 16 64 07 DD 8A 88 F0 B5 53 C5 32 " +
            "2C 03 B0 F9 AF 2E 86 78 7E 0B 37 DD 96 D6 E9 01 " +
            "AE 6E 34 A0 85 F4 62 C9 4E 7F A4 32 5A 29 1A 4C " +
            "BF 34 CF 96 27 57 09 92 3A 09 96 AB 30 C2 6D 72 " +
            "9B 03 5B 09 A4 3B 00 73 22 C1 81 69 A0 34 EF 43 " +
            "90 7A F6 76 76 A9 69 81 4F 3E 1B 90 C4 0D 2C A8 " +
            "5F 2D D2 DF 22 8A A9 19 D8 3B 6E CE D3 6D FD 28 " +
            "45 BE 38 4C 1C F4 7B 06 83 8B A5 B8 58 A0 8C A5 " +
            "A5 35 FE 17 EA 26 C2 DA 1F 32 BC 1B 20 57 B0 38 " +
            "FF A2 20 BD D6 22 51 3A F4 A9 BB 6B 7A C2 AC C1 " +
            "8A EB DF 60 13 65 FE 21 7F 5E 50 14 20 63 DB EE " +
            "19 F7 A7 44 E2 7C D6 3E 41 24 5A 62 22 CD F9 20 " +
            "C0 6D 26 75 56 DA CA 2C",
            // Round #2
            // After Theta
            "CC C1 EC 9F 63 A3 06 1F 4B E8 66 26 62 9A E8 56 " +
            "DD 83 B3 86 18 BC 07 C5 CB 42 F9 39 C2 C3 ED D7 " +
            "3D 97 2F 83 C0 3D 14 15 5D 7D 51 42 76 9C 78 54 " +
            "29 56 21 40 F0 9E 24 F6 CB 89 95 D4 87 50 EC CF " +
            "2E 4A 95 ED F0 2E 04 A5 B1 38 9A 4A E5 FD 99 9F " +
            "83 78 03 06 5A 1C 0B 99 D9 5C F5 46 13 C4 01 CC " +
            "AE AD D1 A0 95 18 28 A4 6D 72 A0 2A 87 78 F9 FE " +
            "D6 47 23 6F 59 3D 0D DA 90 89 50 C8 74 15 EE BD " +
            "33 57 10 C1 3D EF EF BE EE B2 BF 64 97 C5 31 85 " +
            "4A EB EE 59 82 37 55 EC 67 50 A0 48 3F 0B DA 1D " +
            "99 E9 2A 10 3F D0 9C 39 E9 3C BE C2 F7 AA F6 8A " +
            "E8 77 A4 3B 55 EE 57 83 F4 6D 94 86 76 D8 FD F6 " +
            "53 94 3D 56 13 13 BC F0",
            // After Rho
            "CC C1 EC 9F 63 A3 06 1F 96 D0 CD 4C C4 34 D1 AD " +
            "F7 E0 AC 21 06 EF 41 71 3C DC 7E BD 2C 94 9F 23 " +
            "EE A1 A8 E8 B9 7C 19 04 64 C7 89 47 D5 D5 17 25 " +
            "02 04 EF 49 62 9F 62 15 F3 72 62 25 F5 21 14 FB " +
            "A5 CA 76 78 17 82 52 17 9F F9 19 8B A3 A9 54 DE " +
            "1C C4 1B 30 D0 E2 58 C8 30 67 73 D5 1B 4D 10 07 " +
            "06 AD C4 40 21 75 6D 8D F1 F2 FD DB E4 40 55 0E " +
            "B7 AC 9E 06 6D EB A3 91 90 E9 2A DC 7B 21 13 A1 " +
            "22 B8 E7 FD DD 77 E6 0A 98 42 77 D9 5F B2 CB E2 " +
            "A6 8A 5D 69 DD 3D 4B F0 1D 67 50 A0 48 3F 0B DA " +
            "73 E6 64 A6 AB 40 FC 40 A6 F3 F8 0A DF AB DA 2B " +
            "FD 8E 74 A7 CA FD 6A 10 6D 94 86 76 D8 FD F6 F4 " +
            "2F FC 14 65 8F D5 C4 04",
            // After Pi
            "CC C1 EC 9F 63 A3 06 1F 02 04 EF 49 62 9F 62 15 " +
            "06 AD C4 40 21 75 6D 8D A6 8A 5D 69 DD 3D 4B F0 " +
            "2F FC 14 65 8F D5 C4 04 3C DC 7E BD 2C 94 9F 23 " +
            "9F F9 19 8B A3 A9 54 DE 1C C4 1B 30 D0 E2 58 C8 " +
            "22 B8 E7 FD DD 77 E6 0A FD 8E 74 A7 CA FD 6A 10 " +
            "96 D0 CD 4C C4 34 D1 AD F3 72 62 25 F5 21 14 FB " +
            "F1 F2 FD DB E4 40 55 0E 1D 67 50 A0 48 3F 0B DA " +
            "73 E6 64 A6 AB 40 FC 40 EE A1 A8 E8 B9 7C 19 04 " +
            "64 C7 89 47 D5 D5 17 25 30 67 73 D5 1B 4D 10 07 " +
            "98 42 77 D9 5F B2 CB E2 6D 94 86 76 D8 FD F6 F4 " +
            "F7 E0 AC 21 06 EF 41 71 A5 CA 76 78 17 82 52 17 " +
            "B7 AC 9E 06 6D EB A3 91 90 E9 2A DC 7B 21 13 A1 " +
            "A6 F3 F8 0A DF AB DA 2B",
            // After Chi
            "C8 68 EC 9F 62 C3 0B 97 A2 06 F6 60 BE 97 60 65 " +
            "0F D9 C4 44 23 B5 E9 89 66 8B B5 F3 BD 1F 49 EB " +
            "2D F8 17 25 8F C9 A4 04 3C D8 7C 8D 7C D6 97 23 " +
            "BD C1 FD 46 AE BC F2 DC C1 C2 0B 32 D2 6A 50 D8 " +
            "22 E8 ED E5 F9 77 73 29 7E AF 75 A5 49 D4 2A CC " +
            "96 50 50 96 C4 74 90 A9 FF 77 62 05 FD 1E 1E 2B " +
            "93 72 D9 DD 47 00 A1 0E 99 77 D9 E8 0C 0B 0A 77 " +
            "12 C4 46 87 9A 41 F8 12 FE 81 DA 78 B3 74 19 06 " +
            "EC C7 8D 4F 91 67 DC C5 55 F3 F3 F3 9B 00 24 13 " +
            "1A 63 5F 51 7E B2 C2 E2 6D D2 87 71 9C 7C F0 D5 " +
            "E5 C4 24 27 6E 86 E0 F1 A5 8B 56 A0 05 82 42 37 " +
            "91 BE 4E 04 E9 61 6B 9B C1 E9 2E FD 7B 65 12 F1 " +
            "A6 F9 AA 52 CE AB C8 2D",
            // After Iota 
            "42 E8 EC 9F 62 C3 0B 17 A2 06 F6 60 BE 97 60 65 " +
            "0F D9 C4 44 23 B5 E9 89 66 8B B5 F3 BD 1F 49 EB " +
            "2D F8 17 25 8F C9 A4 04 3C D8 7C 8D 7C D6 97 23 " +
            "BD C1 FD 46 AE BC F2 DC C1 C2 0B 32 D2 6A 50 D8 " +
            "22 E8 ED E5 F9 77 73 29 7E AF 75 A5 49 D4 2A CC " +
            "96 50 50 96 C4 74 90 A9 FF 77 62 05 FD 1E 1E 2B " +
            "93 72 D9 DD 47 00 A1 0E 99 77 D9 E8 0C 0B 0A 77 " +
            "12 C4 46 87 9A 41 F8 12 FE 81 DA 78 B3 74 19 06 " +
            "EC C7 8D 4F 91 67 DC C5 55 F3 F3 F3 9B 00 24 13 " +
            "1A 63 5F 51 7E B2 C2 E2 6D D2 87 71 9C 7C F0 D5 " +
            "E5 C4 24 27 6E 86 E0 F1 A5 8B 56 A0 05 82 42 37 " +
            "91 BE 4E 04 E9 61 6B 9B C1 E9 2E FD 7B 65 12 F1 " +
            "A6 F9 AA 52 CE AB C8 2D",
            // Round #3
            // After Theta
            "9A A9 80 22 9F E8 60 F5 62 6A 9E 02 31 79 3A A1 " +
            "AB 19 97 2D C0 0D 3A A4 EB DE 0D E7 65 B7 83 78 " +
            "CD 2D 9B C1 CD 5B AF 77 E4 99 10 30 81 FD FC C1 " +
            "7D AD 95 24 21 52 A8 18 65 02 58 5B 31 D2 83 F5 " +
            "AF BD 55 F1 21 DF B9 BA 9E 7A F9 41 0B 46 21 BF " +
            "4E 11 3C 2B 39 5F FB 4B 3F 1B 0A 67 72 F0 44 EF " +
            "37 B2 8A B4 A4 B8 72 23 14 22 61 FC D4 A3 C0 E4 " +
            "F2 11 CA 63 D8 D3 F3 61 26 C0 B6 C5 4E 5F 72 E4 " +
            "2C AB E5 2D 1E 89 86 01 F1 33 A0 9A 78 B8 F7 3E " +
            "97 36 E7 45 A6 1A 08 71 8D 07 0B 95 DE EE FB A6 " +
            "3D 85 48 9A 93 AD 8B 13 65 E7 3E C2 8A 6C 18 F3 " +
            "35 7E 1D 6D 0A D9 B8 B6 4C BC 96 E9 A3 CD D8 62 " +
            "46 2C 26 B6 8C 39 C3 5E",
            // After Rho
            "9A A9 80 22 9F E8 60 F5 C5 D4 3C 05 62 F2 74 42 " +
            "6A C6 65 0B 70 83 0E E9 76 3B 88 B7 EE DD 70 5E " +
            "DE 7A BD 6B 6E D9 0C 6E 13 D8 CF 1F 4C 9E 09 01 " +
            "49 12 22 85 8A D1 D7 5A 7D 99 00 D6 56 8C F4 60 " +
            "DE AA F8 90 EF 5C DD D7 14 F2 EB A9 97 1F B4 60 " +
            "72 8A E0 59 C9 F9 DA 5F BD FF 6C 28 9C C9 C1 13 " +
            "A4 25 C5 95 1B B9 91 55 47 81 C9 29 44 C2 F8 A9 " +
            "31 EC E9 F9 30 F9 08 E5 8B 9D BE E4 C8 4D 80 6D " +
            "BC C5 23 D1 30 80 65 B5 7B 9F F8 19 50 4D 3C DC " +
            "03 21 EE D2 E6 BC C8 54 A6 8D 07 0B 95 DE EE FB " +
            "2E 4E F4 14 22 69 4E B6 97 9D FB 08 2B B2 61 CC " +
            "C6 AF A3 4D 21 1B D7 B6 BC 96 E9 A3 CD D8 62 4C " +
            "B0 97 11 8B 89 2D 63 CE",
            // After Pi
            "9A A9 80 22 9F E8 60 F5 49 12 22 85 8A D1 D7 5A " +
            "A4 25 C5 95 1B B9 91 55 03 21 EE D2 E6 BC C8 54 " +
            "B0 97 11 8B 89 2D 63 CE 76 3B 88 B7 EE DD 70 5E " +
            "14 F2 EB A9 97 1F B4 60 72 8A E0 59 C9 F9 DA 5F " +
            "BC C5 23 D1 30 80 65 B5 C6 AF A3 4D 21 1B D7 B6 " +
            "C5 D4 3C 05 62 F2 74 42 7D 99 00 D6 56 8C F4 60 " +
            "47 81 C9 29 44 C2 F8 A9 A6 8D 07 0B 95 DE EE FB " +
            "2E 4E F4 14 22 69 4E B6 DE 7A BD 6B 6E D9 0C 6E " +
            "13 D8 CF 1F 4C 9E 09 01 BD FF 6C 28 9C C9 C1 13 " +
            "7B 9F F8 19 50 4D 3C DC BC 96 E9 A3 CD D8 62 4C " +
            "6A C6 65 0B 70 83 0E E9 DE AA F8 90 EF 5C DD D7 " +
            "31 EC E9 F9 30 F9 08 E5 8B 9D BE E4 C8 4D 80 6D " +
            "97 9D FB 08 2B B2 61 CC",
            // After Chi
            "3E 8C 45 32 8E C0 60 F0 4A 12 08 C7 6E D5 9F 5A " +
            "14 B3 D4 9C 12 B8 B2 DF 09 09 6E F2 F0 7C C8 65 " +
            "F1 85 33 0E 89 3C F4 C4 14 33 88 E7 A6 3D 3A 41 " +
            "98 B7 E8 29 A7 1F 91 C0 30 A0 60 55 C8 E2 48 5D " +
            "8C D5 2B 63 FE 44 45 FD C6 6F C0 45 30 19 53 96 " +
            "C7 D4 F5 2C 62 B0 7C CB DD 95 06 D4 C7 90 F2 32 " +
            "4F C3 39 3D 66 E3 F8 AD 67 1D 0F 0A D5 4C DE BB " +
            "16 47 F4 C6 36 65 CE 96 72 5D 9D 4B FE 98 CC 7C " +
            "51 D8 5F 0E 0C 9A 35 CD 39 FF 6D 8A 11 59 83 13 " +
            "39 F7 EC 51 72 4C 30 FE BD 16 AB B7 CD DE 63 4D " +
            "4B 82 64 62 60 22 0E C9 54 BB EE 94 27 58 5D DF " +
            "25 EC A8 F1 13 4B 69 65 E3 DF BA E7 98 4C 8E 4C " +
            "03 B5 63 98 A4 EE B0 DA",
            // After Iota 
            "3E 0C 45 B2 8E C0 60 70 4A 12 08 C7 6E D5 9F 5A " +
            "14 B3 D4 9C 12 B8 B2 DF 09 09 6E F2 F0 7C C8 65 " +
            "F1 85 33 0E 89 3C F4 C4 14 33 88 E7 A6 3D 3A 41 " +
            "98 B7 E8 29 A7 1F 91 C0 30 A0 60 55 C8 E2 48 5D " +
            "8C D5 2B 63 FE 44 45 FD C6 6F C0 45 30 19 53 96 " +
            "C7 D4 F5 2C 62 B0 7C CB DD 95 06 D4 C7 90 F2 32 " +
            "4F C3 39 3D 66 E3 F8 AD 67 1D 0F 0A D5 4C DE BB " +
            "16 47 F4 C6 36 65 CE 96 72 5D 9D 4B FE 98 CC 7C " +
            "51 D8 5F 0E 0C 9A 35 CD 39 FF 6D 8A 11 59 83 13 " +
            "39 F7 EC 51 72 4C 30 FE BD 16 AB B7 CD DE 63 4D " +
            "4B 82 64 62 60 22 0E C9 54 BB EE 94 27 58 5D DF " +
            "25 EC A8 F1 13 4B 69 65 E3 DF BA E7 98 4C 8E 4C " +
            "03 B5 63 98 A4 EE B0 DA",
            // Round #4
            // After Theta
            "B4 A4 24 50 23 80 F3 56 70 A0 58 89 C7 75 AA A6 " +
            "6F 32 BA 66 55 C8 FC 46 40 D7 B8 38 83 36 54 9B " +
            "61 05 AD 82 10 A7 D0 CA 9E 9B E9 05 0B 7D A9 67 " +
            "A2 05 B8 67 0E BF A4 3C 4B 21 0E AF 8F 92 06 C4 " +
            "C5 0B FD A9 8D 0E D9 03 56 EF 5E C9 A9 82 77 98 " +
            "4D 7C 94 CE CF F0 EF ED E7 27 56 9A 6E 30 C7 CE " +
            "34 42 57 C7 21 93 B6 34 2E C3 D9 C0 A6 06 42 45 " +
            "86 C7 6A 4A AF FE EA 98 F8 F5 FC A9 53 D8 5F 5A " +
            "6B 6A 0F 40 A5 3A 00 31 42 7E 03 70 56 29 CD 8A " +
            "70 29 3A 9B 01 06 AC 00 2D 96 35 3B 54 45 47 43 " +
            "C1 2A 05 80 CD 62 9D EF 6E 09 BE DA 8E F8 68 23 " +
            "5E 6D C6 0B 54 3B 27 FC AA 01 6C 2D EB 06 12 B2 " +
            "93 35 FD 14 3D 75 94 D4",
            // After Rho
            "B4 A4 24 50 23 80 F3 56 E1 40 B1 12 8F EB 54 4D " +
            "9B 8C AE 59 15 32 BF D1 68 43 B5 09 74 8D 8B 33 " +
            "38 85 56 0E 2B 68 15 84 B0 D0 97 7A E6 B9 99 5E " +
            "7B E6 F0 4B CA 23 5A 80 F1 52 88 C3 EB A3 A4 01 " +
            "85 FE D4 46 87 EC 81 E2 78 87 69 F5 EE 95 9C 2A " +
            "6F E2 A3 74 7E 86 7F 6F 3B 9F 9F 58 69 BA C1 1C " +
            "3A 0E 99 B4 A5 A1 11 BA 0D 84 8A 5C 86 B3 81 4D " +
            "A5 57 7F 75 4C C3 63 35 53 A7 B0 BF B4 F0 EB F9 " +
            "01 A8 54 07 20 66 4D ED 66 45 21 BF 01 38 AB 94 " +
            "80 15 00 2E 45 67 33 C0 43 2D 96 35 3B 54 45 47 " +
            "75 BE 07 AB 14 00 36 8B B8 25 F8 6A 3B E2 A3 8D " +
            "AB CD 78 81 6A E7 84 DF 01 6C 2D EB 06 12 B2 AA " +
            "25 F5 64 4D 3F 45 4F 1D",
            // After Pi
            "B4 A4 24 50 23 80 F3 56 7B E6 F0 4B CA 23 5A 80 " +
            "3A 0E 99 B4 A5 A1 11 BA 80 15 00 2E 45 67 33 C0 " +
            "25 F5 64 4D 3F 45 4F 1D 68 43 B5 09 74 8D 8B 33 " +
            "78 87 69 F5 EE 95 9C 2A 6F E2 A3 74 7E 86 7F 6F " +
            "01 A8 54 07 20 66 4D ED AB CD 78 81 6A E7 84 DF " +
            "E1 40 B1 12 8F EB 54 4D F1 52 88 C3 EB A3 A4 01 " +
            "0D 84 8A 5C 86 B3 81 4D 43 2D 96 35 3B 54 45 47 " +
            "75 BE 07 AB 14 00 36 8B 38 85 56 0E 2B 68 15 84 " +
            "B0 D0 97 7A E6 B9 99 5E 3B 9F 9F 58 69 BA C1 1C " +
            "66 45 21 BF 01 38 AB 94 01 6C 2D EB 06 12 B2 AA " +
            "9B 8C AE 59 15 32 BF D1 85 FE D4 46 87 EC 81 E2 " +
            "A5 57 7F 75 4C C3 63 35 53 A7 B0 BF B4 F0 EB F9 " +
            "B8 25 F8 6A 3B E2 A3 8D",
            // After Chi
            "B4 AC 2D E4 06 00 F2 6C FB F7 F0 41 8A 65 78 C0 " +
            "1F EE FD F5 9F A1 5D A7 10 15 00 3E 45 E7 83 82 " +
            "6E B7 B4 46 F7 66 47 9D 6F 23 37 09 64 8F E8 76 " +
            "78 8F 3D F6 EE F5 9C AA C5 A7 8B F4 34 07 FF 7D " +
            "41 AA D1 0F 34 6E 46 CD BB 49 30 75 E0 F7 90 D7 " +
            "ED C4 B3 0E 8B FB 55 01 B3 7B 9C E2 D2 E7 E0 03 " +
            "39 16 8B D6 82 B3 B3 C5 C3 6D 26 25 B0 BF 05 03 " +
            "65 AC 0F 6A 74 00 96 8B 33 8A 5E 0E 22 6A 55 84 " +
            "F4 90 B7 DD E6 B9 B3 DE 3A B7 93 18 6F B8 D1 36 " +
            "5E C4 73 BB 28 50 AE 90 81 3C AC 9B C2 83 3A F0 " +
            "BB 8D 85 68 5D 31 DD C4 D7 5E 54 CC 37 DC 09 2A " +
            "0D 57 37 35 47 C1 63 31 50 2F B6 AE B0 E0 F7 A9 " +
            "BC 57 A8 6C B9 2E A3 AF",
            // After Iota 
            "3F 2C 2D E4 06 00 F2 6C FB F7 F0 41 8A 65 78 C0 " +
            "1F EE FD F5 9F A1 5D A7 10 15 00 3E 45 E7 83 82 " +
            "6E B7 B4 46 F7 66 47 9D 6F 23 37 09 64 8F E8 76 " +
            "78 8F 3D F6 EE F5 9C AA C5 A7 8B F4 34 07 FF 7D " +
            "41 AA D1 0F 34 6E 46 CD BB 49 30 75 E0 F7 90 D7 " +
            "ED C4 B3 0E 8B FB 55 01 B3 7B 9C E2 D2 E7 E0 03 " +
            "39 16 8B D6 82 B3 B3 C5 C3 6D 26 25 B0 BF 05 03 " +
            "65 AC 0F 6A 74 00 96 8B 33 8A 5E 0E 22 6A 55 84 " +
            "F4 90 B7 DD E6 B9 B3 DE 3A B7 93 18 6F B8 D1 36 " +
            "5E C4 73 BB 28 50 AE 90 81 3C AC 9B C2 83 3A F0 " +
            "BB 8D 85 68 5D 31 DD C4 D7 5E 54 CC 37 DC 09 2A " +
            "0D 57 37 35 47 C1 63 31 50 2F B6 AE B0 E0 F7 A9 " +
            "BC 57 A8 6C B9 2E A3 AF",
            // Round #5
            // After Theta
            "95 8F C7 C3 D0 18 56 C9 66 44 31 30 1F 92 F9 AA " +
            "34 50 2B B3 4A BF D0 D1 DF D9 47 99 75 F3 90 A7 " +
            "98 16 63 4D 83 BF 50 5F C5 80 DD 2E B2 97 4C D3 " +
            "E5 3C FC 87 7B 02 1D C0 EE 19 5D B2 E1 19 72 0B " +
            "8E 66 96 A8 04 7A 55 E8 4D E8 E7 7E 94 2E 87 15 " +
            "47 67 59 29 5D E3 F1 A4 2E C8 5D 93 47 10 61 69 " +
            "12 A8 5D 90 57 AD 3E B3 0C A1 61 82 80 AB 16 26 " +
            "93 0D D8 61 00 D9 81 49 99 29 B4 29 F4 72 F1 21 " +
            "69 23 76 AC 73 4E 32 B4 11 09 45 5E BA A6 5C 40 " +
            "91 08 34 1C 18 44 BD B5 77 9D 7B 90 B6 5A 2D 32 " +
            "11 2E 6F 4F 8B 29 79 61 4A ED 95 BD A2 2B 88 40 " +
            "26 E9 E1 73 92 DF EE 47 9F E3 F1 09 80 F4 E4 8C " +
            "4A F6 7F 67 CD F7 B4 6D",
            // After Rho
            "95 8F C7 C3 D0 18 56 C9 CD 88 62 60 3E 24 F3 55 " +
            "0D D4 CA AC D2 2F 74 34 37 0F 79 FA 9D 7D 94 59 " +
            "FC 85 FA C2 B4 18 6B 1A 22 7B C9 34 5D 0C D8 ED " +
            "7F B8 27 D0 01 5C CE C3 82 7B 46 97 6C 78 86 DC " +
            "33 4B 54 02 BD 2A 74 47 72 58 D1 84 7E EE 47 E9 " +
            "3D 3A CB 4A E9 1A 8F 27 A5 B9 20 77 4D 1E 41 84 " +
            "82 BC 6A F5 99 95 40 ED 57 2D 4C 18 42 C3 04 01 " +
            "30 80 EC C0 A4 C9 06 EC 53 E8 E5 E2 43 32 53 68 " +
            "8E 75 CE 49 86 36 6D C4 2E A0 88 84 22 2F 5D 53 " +
            "A8 B7 36 12 81 86 03 83 32 77 9D 7B 90 B6 5A 2D " +
            "E4 85 45 B8 BC 3D 2D A6 29 B5 57 F6 8A AE 20 02 " +
            "24 3D 7C 4E F2 DB FD C8 E3 F1 09 80 F4 E4 8C 9F " +
            "6D 9B 92 FD DF 59 F3 3D",
            // After Pi
            "95 8F C7 C3 D0 18 56 C9 7F B8 27 D0 01 5C CE C3 " +
            "82 BC 6A F5 99 95 40 ED A8 B7 36 12 81 86 03 83 " +
            "6D 9B 92 FD DF 59 F3 3D 37 0F 79 FA 9D 7D 94 59 " +
            "72 58 D1 84 7E EE 47 E9 3D 3A CB 4A E9 1A 8F 27 " +
            "8E 75 CE 49 86 36 6D C4 24 3D 7C 4E F2 DB FD C8 " +
            "CD 88 62 60 3E 24 F3 55 82 7B 46 97 6C 78 86 DC " +
            "57 2D 4C 18 42 C3 04 01 32 77 9D 7B 90 B6 5A 2D " +
            "E4 85 45 B8 BC 3D 2D A6 FC 85 FA C2 B4 18 6B 1A " +
            "22 7B C9 34 5D 0C D8 ED A5 B9 20 77 4D 1E 41 84 " +
            "2E A0 88 84 22 2F 5D 53 E3 F1 09 80 F4 E4 8C 9F " +
            "0D D4 CA AC D2 2F 74 34 33 4B 54 02 BD 2A 74 47 " +
            "30 80 EC C0 A4 C9 06 EC 53 E8 E5 E2 43 32 53 68 " +
            "29 B5 57 F6 8A AE 20 02",
            // After Chi
            "15 8B 8F E6 48 99 56 E5 57 BB 33 D2 01 5E CD C1 " +
            "C7 B4 EA 18 C7 CC B0 D1 38 B3 73 10 81 86 07 43 " +
            "07 AB B2 ED DE 1D 7B 3F 3A 2D 73 B0 1C 6D 1C 5F " +
            "F0 1D D5 85 78 CA 27 29 1D 32 FB 4C 99 D3 1F 2F " +
            "9D 77 CF F9 8B 12 6D D5 64 6D FC 4A 90 59 BE 68 " +
            "98 8C 6A 68 3C A7 F3 54 A2 29 D7 F4 FC 4C DC F0 " +
            "93 AD 0C 98 6E CA 21 83 3B 7F BF 3B 92 B6 88 7C " +
            "E6 F6 41 2F FC 65 29 2E 79 05 DA 81 B4 0A 6A 1A " +
            "28 7B 41 B4 7F 2D C4 BE 64 E8 21 77 99 DE C1 08 " +
            "32 A4 7A C6 22 37 3E 53 E1 8B 08 B4 BD E0 1C 7A " +
            "0D 54 62 6C D2 EE 76 9C 70 23 55 20 FE 18 25 47 " +
            "18 95 FE D4 2C 45 26 EE 57 A8 6D EA 13 33 07 5C " +
            "1B BE 43 F4 A7 AE 20 41",
            // After Iota 
            "14 8B 8F 66 48 99 56 E5 57 BB 33 D2 01 5E CD C1 " +
            "C7 B4 EA 18 C7 CC B0 D1 38 B3 73 10 81 86 07 43 " +
            "07 AB B2 ED DE 1D 7B 3F 3A 2D 73 B0 1C 6D 1C 5F " +
            "F0 1D D5 85 78 CA 27 29 1D 32 FB 4C 99 D3 1F 2F " +
            "9D 77 CF F9 8B 12 6D D5 64 6D FC 4A 90 59 BE 68 " +
            "98 8C 6A 68 3C A7 F3 54 A2 29 D7 F4 FC 4C DC F0 " +
            "93 AD 0C 98 6E CA 21 83 3B 7F BF 3B 92 B6 88 7C " +
            "E6 F6 41 2F FC 65 29 2E 79 05 DA 81 B4 0A 6A 1A " +
            "28 7B 41 B4 7F 2D C4 BE 64 E8 21 77 99 DE C1 08 " +
            "32 A4 7A C6 22 37 3E 53 E1 8B 08 B4 BD E0 1C 7A " +
            "0D 54 62 6C D2 EE 76 9C 70 23 55 20 FE 18 25 47 " +
            "18 95 FE D4 2C 45 26 EE 57 A8 6D EA 13 33 07 5C " +
            "1B BE 43 F4 A7 AE 20 41",
            // Round #6
            // After Theta
            "D0 20 80 C0 E8 2C 29 64 FE 6C 99 5E 05 74 BA 9F " +
            "6D 0C E6 D3 90 6C D1 FB F3 EF 39 EF 55 17 CE 5D " +
            "78 EB FA B5 6B 55 EB 0B FE 86 7C 16 BC D8 63 DE " +
            "59 CA 7F 09 7C E0 50 77 B7 8A F7 87 CE 73 7E 05 " +
            "56 2B 85 06 5F 83 A4 CB 1B 2D B4 12 25 11 2E 5C " +
            "5C 27 65 CE 9C 12 8C D5 0B FE 7D 78 F8 66 AB AE " +
            "39 15 00 53 39 6A 40 A9 F0 23 F5 C4 46 27 41 62 " +
            "99 B6 09 77 49 2D B9 1A BD AE D5 27 14 BF 15 9B " +
            "81 AC EB 38 7B 07 B3 E0 CE 50 2D BC CE 7E A0 22 " +
            "F9 F8 30 39 F6 A6 F7 4D 9E CB 40 EC 08 A8 8C 4E " +
            "C9 FF 6D CA 72 5B 09 1D D9 F4 FF AC FA 32 52 19 " +
            "B2 2D F2 1F 7B E5 47 C4 9C F4 27 15 C7 A2 CE 42 " +
            "64 FE 0B AC 12 E6 B0 75",
            // After Rho
            "D0 20 80 C0 E8 2C 29 64 FD D9 32 BD 0A E8 74 3F " +
            "1B 83 F9 34 24 5B F4 7E 75 E1 DC 35 FF 9E F3 5E " +
            "AB 5A 5F C0 5B D7 AF 5D C1 8B 3D E6 ED 6F C8 67 " +
            "97 C0 07 0E 75 97 A5 FC C1 AD E2 FD A1 F3 9C 5F " +
            "95 42 83 AF 41 D2 65 AB E1 C2 B5 D1 42 2B 51 12 " +
            "E6 3A 29 73 E6 94 60 AC BA 2E F8 F7 E1 E1 9B AD " +
            "98 CA 51 03 4A CD A9 00 4E 82 C4 E0 47 EA 89 8D " +
            "BB A4 96 5C 8D 4C DB 84 4F 28 7E 2B 36 7B 5D AB " +
            "1D 67 EF 60 16 3C 90 75 50 11 67 A8 16 5E 67 3F " +
            "F4 BE 29 1F 1F 26 C7 DE 4E 9E CB 40 EC 08 A8 8C " +
            "25 74 24 FF B7 29 CB 6D 64 D3 FF B3 EA CB 48 65 " +
            "B6 45 FE 63 AF FC 88 58 F4 27 15 C7 A2 CE 42 9C " +
            "6C 1D 99 FF 02 AB 84 39",
            // After Pi
            "D0 20 80 C0 E8 2C 29 64 97 C0 07 0E 75 97 A5 FC " +
            "98 CA 51 03 4A CD A9 00 F4 BE 29 1F 1F 26 C7 DE " +
            "6C 1D 99 FF 02 AB 84 39 75 E1 DC 35 FF 9E F3 5E " +
            "E1 C2 B5 D1 42 2B 51 12 E6 3A 29 73 E6 94 60 AC " +
            "1D 67 EF 60 16 3C 90 75 B6 45 FE 63 AF FC 88 58 " +
            "FD D9 32 BD 0A E8 74 3F C1 AD E2 FD A1 F3 9C 5F " +
            "4E 82 C4 E0 47 EA 89 8D 4E 9E CB 40 EC 08 A8 8C " +
            "25 74 24 FF B7 29 CB 6D AB 5A 5F C0 5B D7 AF 5D " +
            "C1 8B 3D E6 ED 6F C8 67 BA 2E F8 F7 E1 E1 9B AD " +
            "50 11 67 A8 16 5E 67 3F F4 27 15 C7 A2 CE 42 9C " +
            "1B 83 F9 34 24 5B F4 7E 95 42 83 AF 41 D2 65 AB " +
            "BB A4 96 5C 8D 4C DB 84 4F 28 7E 2B 36 7B 5D AB " +
            "64 D3 FF B3 EA CB 48 65",
            // After Chi
            "D8 2A D0 C1 E2 64 21 64 F3 F4 2F 12 60 B5 E3 22 " +
            "90 CB C1 E3 4A 44 A9 21 64 9E 29 1F F7 22 EE 9A " +
            "6B DD 9E F1 17 38 00 A1 73 D9 D4 17 5B 0A D3 F2 " +
            "F8 87 73 D1 52 03 C1 43 44 3A 39 70 4F 54 68 A4 " +
            "5C C7 EF 74 46 3E E3 73 36 47 DF A3 AF DD 88 58 " +
            "F3 DB 36 BD 4C E0 75 BF C1 B1 E9 FD 09 F3 BC 5F " +
            "6F E2 E0 5F 54 CB CA EC 96 17 D9 40 E4 C8 9C 9E " +
            "25 50 E4 BF 16 3A 43 2D 91 7E 9F D1 5B 57 BC D5 " +
            "81 9A 3A EE FB 71 AC 75 1E 08 E8 B0 41 61 9B 2D " +
            "5B 49 2D A8 4F 4F CA 7E B4 A6 35 E1 06 E6 02 BE " +
            "31 27 ED 64 A8 57 6E 7A D1 4A EB 8C 73 E1 61 80 " +
            "9B 77 17 CC 45 CC DB C0 54 28 7E 2F 32 6B E9 B1 " +
            "E0 93 FD 38 AB 4B 49 E4",
            // After Iota 
            "59 AA D0 41 E2 64 21 E4 F3 F4 2F 12 60 B5 E3 22 " +
            "90 CB C1 E3 4A 44 A9 21 64 9E 29 1F F7 22 EE 9A " +
            "6B DD 9E F1 17 38 00 A1 73 D9 D4 17 5B 0A D3 F2 " +
            "F8 87 73 D1 52 03 C1 43 44 3A 39 70 4F 54 68 A4 " +
            "5C C7 EF 74 46 3E E3 73 36 47 DF A3 AF DD 88 58 " +
            "F3 DB 36 BD 4C E0 75 BF C1 B1 E9 FD 09 F3 BC 5F " +
            "6F E2 E0 5F 54 CB CA EC 96 17 D9 40 E4 C8 9C 9E " +
            "25 50 E4 BF 16 3A 43 2D 91 7E 9F D1 5B 57 BC D5 " +
            "81 9A 3A EE FB 71 AC 75 1E 08 E8 B0 41 61 9B 2D " +
            "5B 49 2D A8 4F 4F CA 7E B4 A6 35 E1 06 E6 02 BE " +
            "31 27 ED 64 A8 57 6E 7A D1 4A EB 8C 73 E1 61 80 " +
            "9B 77 17 CC 45 CC DB C0 54 28 7E 2F 32 6B E9 B1 " +
            "E0 93 FD 38 AB 4B 49 E4",
            // Round #7
            // After Theta
            "40 70 75 CD 87 BD 06 FC F7 DD A1 2D CD D7 20 2C " +
            "49 86 3D E7 A8 71 9F 9B 03 0C 15 C7 A4 B0 A5 03 " +
            "38 10 53 E1 33 D4 19 15 6A 03 71 9B 3E D3 F4 EA " +
            "FC AE FD EE FF 61 02 4D 9D 77 C5 74 AD 61 5E 1E " +
            "3B 55 D3 AC 15 AC A8 EA 65 8A 12 B3 8B 31 91 EC " +
            "EA 01 93 31 29 39 52 A7 C5 98 67 C2 A4 91 7F 51 " +
            "B6 AF 1C 5B B6 FE FC 56 F1 85 E5 98 B7 5A D7 07 " +
            "76 9D 29 AF 32 D6 5A 99 88 A4 3A 5D 3E 8E 9B CD " +
            "85 B3 B4 D1 56 13 6F 7B C7 45 14 B4 A3 54 AD 97 " +
            "3C DB 11 70 1C DD 81 E7 E7 6B F8 F1 22 0A 1B 0A " +
            "28 FD 48 E8 CD 8E 49 62 D5 63 65 B3 DE 83 A2 8E " +
            "42 3A EB C8 A7 F9 ED 7A 33 BA 42 F7 61 F9 A2 28 " +
            "B3 5E 30 28 8F A7 50 50",
            // After Rho
            "40 70 75 CD 87 BD 06 FC EE BB 43 5B 9A AF 41 58 " +
            "92 61 CF 39 6A DC E7 66 0A 5B 3A 30 C0 50 71 4C " +
            "A1 CE A8 C0 81 98 0A 9F E9 33 4D AF AE 36 10 B7 " +
            "EF FE 1F 26 D0 C4 EF DA 47 E7 5D 31 5D 6B 98 97 " +
            "AA 69 D6 0A 56 54 F5 9D 13 C9 5E A6 28 31 BB 18 " +
            "55 0F 98 8C 49 C9 91 3A 45 15 63 9E 09 93 46 FE " +
            "D8 B2 F5 E7 B7 B2 7D E5 B5 AE 0F E2 0B CB 31 6F " +
            "57 19 6B AD 4C BB CE 94 BA 7C 1C 37 9B 11 49 75 " +
            "36 DA 6A E2 6D AF 70 96 D6 CB E3 22 0A DA 51 AA " +
            "3B F0 9C 67 3B 02 8E A3 0A E7 6B F8 F1 22 0A 1B " +
            "26 89 A1 F4 23 A1 37 3B 56 8F 95 CD 7A 0F 8A 3A " +
            "48 67 1D F9 34 BF 5D 4F BA 42 F7 61 F9 A2 28 33 " +
            "14 D4 AC 17 0C CA E3 29",
            // After Pi
            "40 70 75 CD 87 BD 06 FC EF FE 1F 26 D0 C4 EF DA " +
            "D8 B2 F5 E7 B7 B2 7D E5 3B F0 9C 67 3B 02 8E A3 " +
            "14 D4 AC 17 0C CA E3 29 0A 5B 3A 30 C0 50 71 4C " +
            "13 C9 5E A6 28 31 BB 18 55 0F 98 8C 49 C9 91 3A " +
            "36 DA 6A E2 6D AF 70 96 48 67 1D F9 34 BF 5D 4F " +
            "EE BB 43 5B 9A AF 41 58 47 E7 5D 31 5D 6B 98 97 " +
            "B5 AE 0F E2 0B CB 31 6F 0A E7 6B F8 F1 22 0A 1B " +
            "26 89 A1 F4 23 A1 37 3B A1 CE A8 C0 81 98 0A 9F " +
            "E9 33 4D AF AE 36 10 B7 45 15 63 9E 09 93 46 FE " +
            "D6 CB E3 22 0A DA 51 AA BA 42 F7 61 F9 A2 28 33 " +
            "92 61 CF 39 6A DC E7 66 AA 69 D6 0A 56 54 F5 9D " +
            "57 19 6B AD 4C BB CE 94 BA 7C 1C 37 9B 11 49 75 " +
            "56 8F 95 CD 7A 0F 8A 3A",
            // After Chi
            "50 70 95 0C A0 8F 16 D9 CC BE 17 26 D8 C4 6D D8 " +
            "DC B6 D5 F7 B3 7A 1C ED 7B D0 CD AF B8 37 8A 77 " +
            "BB 5A A6 35 5C 8A 0A 2B 4E 5D BA 38 81 98 71 6E " +
            "31 19 3C C4 0C 17 DB 9C 1D 2A 8D 95 59 D9 9C 73 " +
            "34 C2 48 E2 AD EF 50 96 59 E7 59 7F 1C 9E D7 5F " +
            "5E B3 41 99 98 2F 60 30 4D A6 3D 29 AD 4B 92 87 " +
            "91 A6 8F E6 09 4A 04 4F C2 D5 29 F3 69 2C 4A 5B " +
            "27 CD BD D4 66 E1 AF BC A5 CA 8A D0 80 19 4C D7 " +
            "7B F9 CD 8F AC 7E 01 B7 6D 15 77 DF F8 B3 6E EF " +
            "D7 47 EB A2 0A C2 53 26 F2 73 B2 4E D7 84 38 13 " +
            "C7 71 E6 9C 62 77 ED 66 02 0D C2 18 C5 54 F4 FC " +
            "13 9A EA 65 2C B5 4C 9E 3A 1C 56 07 9B C1 2C 31 " +
            "7E 87 85 CF 6E 0F 9A A3",
            // After Iota 
            "59 F0 95 0C A0 8F 16 59 CC BE 17 26 D8 C4 6D D8 " +
            "DC B6 D5 F7 B3 7A 1C ED 7B D0 CD AF B8 37 8A 77 " +
            "BB 5A A6 35 5C 8A 0A 2B 4E 5D BA 38 81 98 71 6E " +
            "31 19 3C C4 0C 17 DB 9C 1D 2A 8D 95 59 D9 9C 73 " +
            "34 C2 48 E2 AD EF 50 96 59 E7 59 7F 1C 9E D7 5F " +
            "5E B3 41 99 98 2F 60 30 4D A6 3D 29 AD 4B 92 87 " +
            "91 A6 8F E6 09 4A 04 4F C2 D5 29 F3 69 2C 4A 5B " +
            "27 CD BD D4 66 E1 AF BC A5 CA 8A D0 80 19 4C D7 " +
            "7B F9 CD 8F AC 7E 01 B7 6D 15 77 DF F8 B3 6E EF " +
            "D7 47 EB A2 0A C2 53 26 F2 73 B2 4E D7 84 38 13 " +
            "C7 71 E6 9C 62 77 ED 66 02 0D C2 18 C5 54 F4 FC " +
            "13 9A EA 65 2C B5 4C 9E 3A 1C 56 07 9B C1 2C 31 " +
            "7E 87 85 CF 6E 0F 9A A3",
            // Round #8
            // After Theta
            "83 9F D3 AB 1F 95 65 30 BA 71 80 BB ED 4C 86 2F " +
            "D4 7B EF 9D 79 27 12 3E C7 6D 6C AF B1 25 8C 26 " +
            "8C 8C B2 EC 06 D1 A9 EB 94 32 FC 9F 3E 82 02 07 " +
            "47 D6 AB 59 39 9F 30 6B 15 E7 B7 FF 93 84 92 A0 " +
            "88 7F E9 E2 A4 FD 56 C7 6E 31 4D A6 46 C5 74 9F " +
            "84 DC 07 3E 27 35 13 59 3B 69 AA B4 98 C3 79 70 " +
            "99 6B B5 8C C3 17 0A 9C 7E 68 88 F3 60 3E 4C 0A " +
            "10 1B A9 0D 3C BA 0C 7C 7F A5 CC 77 3F 03 3F BE " +
            "0D 36 5A 12 99 F6 EA 40 65 D8 4D B5 32 EE 60 3C " +
            "6B FA 4A A2 03 D0 55 77 C5 A5 A6 97 8D DF 9B D3 " +
            "1D 1E A0 3B DD 6D 9E 0F 74 C2 55 85 F0 DC 1F 0B " +
            "1B 57 D0 0F E6 E8 42 4D 86 A1 F7 07 92 D3 2A 60 " +
            "49 51 91 16 34 54 39 63",
            // After Rho
            "83 9F D3 AB 1F 95 65 30 74 E3 00 77 DB 99 0C 5F " +
            "F5 DE 7B 67 DE 89 84 0F 5B C2 68 72 DC C6 F6 1A " +
            "88 4E 5D 67 64 94 65 37 E9 23 28 70 40 29 C3 FF " +
            "9A 95 F3 09 B3 76 64 BD 68 C5 F9 ED FF 24 A1 24 " +
            "BF 74 71 D2 7E AB 63 C4 4C F7 E9 16 D3 64 6A 54 " +
            "22 E4 3E F0 39 A9 99 C8 C1 ED A4 A9 D2 62 0E E7 " +
            "65 1C BE 50 E0 CC 5C AB 7C 98 14 FC D0 10 E7 C1 " +
            "06 1E 5D 06 3E 88 8D D4 EF 7E 06 7E 7C FF 4A 99 " +
            "4B 22 D3 5E 1D A8 C1 46 30 9E 32 EC A6 5A 19 77 " +
            "BA EA 6E 4D 5F 49 74 00 D3 C5 A5 A6 97 8D DF 9B " +
            "79 3E 74 78 80 EE 74 B7 D0 09 57 15 C2 73 7F 2C " +
            "E3 0A FA C1 1C 5D A8 69 A1 F7 07 92 D3 2A 60 86 " +
            "CE 58 52 54 A4 05 0D 55",
            // After Pi
            "83 9F D3 AB 1F 95 65 30 9A 95 F3 09 B3 76 64 BD " +
            "65 1C BE 50 E0 CC 5C AB BA EA 6E 4D 5F 49 74 00 " +
            "CE 58 52 54 A4 05 0D 55 5B C2 68 72 DC C6 F6 1A " +
            "4C F7 E9 16 D3 64 6A 54 22 E4 3E F0 39 A9 99 C8 " +
            "4B 22 D3 5E 1D A8 C1 46 E3 0A FA C1 1C 5D A8 69 " +
            "74 E3 00 77 DB 99 0C 5F 68 C5 F9 ED FF 24 A1 24 " +
            "7C 98 14 FC D0 10 E7 C1 D3 C5 A5 A6 97 8D DF 9B " +
            "79 3E 74 78 80 EE 74 B7 88 4E 5D 67 64 94 65 37 " +
            "E9 23 28 70 40 29 C3 FF C1 ED A4 A9 D2 62 0E E7 " +
            "30 9E 32 EC A6 5A 19 77 A1 F7 07 92 D3 2A 60 86 " +
            "F5 DE 7B 67 DE 89 84 0F BF 74 71 D2 7E AB 63 C4 " +
            "06 1E 5D 06 3E 88 8D D4 EF 7E 06 7E 7C FF 4A 99 " +
            "D0 09 57 15 C2 73 7F 2C",
            // After Chi
            "E6 97 DF FB 5F 1D 7D 32 00 77 B3 04 AC 77 44 BD " +
            "21 0C AE 40 40 C8 55 FE BB 6D EF E6 44 D9 14 20 " +
            "D6 58 72 54 04 67 0D D8 79 C2 7E 92 F4 4F 67 92 " +
            "05 F5 28 18 D7 64 2A 52 82 EC 16 71 39 FC B1 E1 " +
            "53 E2 D3 6C DD 2A 97 54 E7 3F 7B C5 1F 7D A0 2D " +
            "60 FB 04 67 DB 89 4A 9E EB 80 58 EF F8 A9 B9 3E " +
            "54 A2 44 A4 D0 72 C7 E5 D7 04 A5 A1 CC 9C D7 D3 " +
            "71 3A 8D F0 A4 CA D5 97 88 82 D9 EE F6 D6 69 37 " +
            "D9 31 3A 34 64 31 D2 EF 40 8C A1 BB 83 42 6E 67 " +
            "38 96 6A 89 82 CE 1C 46 C0 D6 27 82 D3 03 E2 4E " +
            "F5 D4 77 63 DE 89 08 1F 56 14 73 AA 3E DC 21 CD " +
            "16 1F 0C 07 BC 88 B8 F0 CA A8 2E 1C 60 77 CA 9A " +
            "DA 29 57 85 E2 51 1C EC",
            // After Iota 
            "6C 97 DF FB 5F 1D 7D 32 00 77 B3 04 AC 77 44 BD " +
            "21 0C AE 40 40 C8 55 FE BB 6D EF E6 44 D9 14 20 " +
            "D6 58 72 54 04 67 0D D8 79 C2 7E 92 F4 4F 67 92 " +
            "05 F5 28 18 D7 64 2A 52 82 EC 16 71 39 FC B1 E1 " +
            "53 E2 D3 6C DD 2A 97 54 E7 3F 7B C5 1F 7D A0 2D " +
            "60 FB 04 67 DB 89 4A 9E EB 80 58 EF F8 A9 B9 3E " +
            "54 A2 44 A4 D0 72 C7 E5 D7 04 A5 A1 CC 9C D7 D3 " +
            "71 3A 8D F0 A4 CA D5 97 88 82 D9 EE F6 D6 69 37 " +
            "D9 31 3A 34 64 31 D2 EF 40 8C A1 BB 83 42 6E 67 " +
            "38 96 6A 89 82 CE 1C 46 C0 D6 27 82 D3 03 E2 4E " +
            "F5 D4 77 63 DE 89 08 1F 56 14 73 AA 3E DC 21 CD " +
            "16 1F 0C 07 BC 88 B8 F0 CA A8 2E 1C 60 77 CA 9A " +
            "DA 29 57 85 E2 51 1C EC",
            // Round #9
            // After Theta
            "F5 7B 3F 46 63 30 B3 14 4A 2C 1B D5 D8 EA 9E 70 " +
            "DA 40 9F 50 F6 32 74 FA AF F8 57 02 CE 50 EC CC " +
            "0B 1D B8 EC 02 B9 EC 8F E0 2E 9E 2F C8 62 A9 B4 " +
            "4F AE 80 C9 A3 F9 F0 9F 79 A0 27 61 8F 06 90 E5 " +
            "47 77 6B 88 57 A3 6F B8 3A 7A B1 7D 19 A3 41 7A " +
            "F9 17 E4 DA E7 A4 84 B8 A1 DB F0 3E 8C 34 63 F3 " +
            "AF EE 75 B4 66 88 E6 E1 C3 91 1D 45 46 15 2F 3F " +
            "AC 7F 47 48 A2 14 34 C0 11 6E 39 53 CA FB A7 11 " +
            "93 6A 92 E5 10 AC 08 22 BB C0 90 AB 35 B8 4F 63 " +
            "2C 03 D2 6D 08 47 E4 AA 1D 93 ED 3A D5 DD 03 19 " +
            "6C 38 97 DE E2 A4 C6 39 1C 4F DB 7B 4A 41 FB 00 " +
            "ED 53 3D 17 0A 72 99 F4 DE 3D 96 F8 EA FE 32 76 " +
            "07 6C 9D 3D E4 8F FD BB",
            // After Rho
            "F5 7B 3F 46 63 30 B3 14 94 58 36 AA B1 D5 3D E1 " +
            "36 D0 27 94 BD 0C 9D BE 0C C5 CE FC 8A 7F 25 E0 " +
            "C8 65 7F 5C E8 C0 65 17 82 2C 96 4A 0B EE E2 F9 " +
            "98 3C 9A 0F FF F9 E4 0A 79 1E E8 49 D8 A3 01 64 " +
            "BB 35 C4 AB D1 37 DC A3 1A A4 A7 A3 17 DB 97 31 " +
            "CD BF 20 D7 3E 27 25 C4 CD 87 6E C3 FB 30 D2 8C " +
            "A3 35 43 34 0F 7F 75 AF 2A 5E 7E 86 23 3B 8A 8C " +
            "24 51 0A 1A 60 D6 BF 23 A6 94 F7 4F 23 22 DC 72 " +
            "B2 1C 82 15 41 64 52 4D A7 B1 5D 60 C8 D5 1A DC " +
            "88 5C 95 65 40 BA 0D E1 19 1D 93 ED 3A D5 DD 03 " +
            "1A E7 B0 E1 5C 7A 8B 93 70 3C 6D EF 29 05 ED 03 " +
            "7D AA E7 42 41 2E 93 BE 3D 96 F8 EA FE 32 76 DE " +
            "FF EE 01 5B 67 0F F9 63",
            // After Pi
            "F5 7B 3F 46 63 30 B3 14 98 3C 9A 0F FF F9 E4 0A " +
            "A3 35 43 34 0F 7F 75 AF 88 5C 95 65 40 BA 0D E1 " +
            "FF EE 01 5B 67 0F F9 63 0C C5 CE FC 8A 7F 25 E0 " +
            "1A A4 A7 A3 17 DB 97 31 CD BF 20 D7 3E 27 25 C4 " +
            "B2 1C 82 15 41 64 52 4D 7D AA E7 42 41 2E 93 BE " +
            "94 58 36 AA B1 D5 3D E1 79 1E E8 49 D8 A3 01 64 " +
            "2A 5E 7E 86 23 3B 8A 8C 19 1D 93 ED 3A D5 DD 03 " +
            "1A E7 B0 E1 5C 7A 8B 93 C8 65 7F 5C E8 C0 65 17 " +
            "82 2C 96 4A 0B EE E2 F9 CD 87 6E C3 FB 30 D2 8C " +
            "A7 B1 5D 60 C8 D5 1A DC 3D 96 F8 EA FE 32 76 DE " +
            "36 D0 27 94 BD 0C 9D BE BB 35 C4 AB D1 37 DC A3 " +
            "24 51 0A 1A 60 D6 BF 23 A6 94 F7 4F 23 22 DC 72 " +
            "70 3C 6D EF 29 05 ED 03",
            // After Chi
            "D6 7A 7E 76 63 36 A2 B1 90 74 0E 4E BF 79 EC 4A " +
            "D4 97 43 2E 28 7A 85 AD 88 4D AB 61 40 8A 0F F5 " +
            "F7 EA 81 52 FB C6 BD 69 C9 DE CE A8 A2 5B 05 24 " +
            "28 A4 25 A3 56 9B C5 38 80 1D 45 95 3E 2D A4 76 " +
            "B2 59 8A A9 CB 35 76 0D 6F 8A C6 41 54 AE 01 AF " +
            "96 18 20 2C 92 CD B7 69 68 1F 69 20 C0 67 54 67 " +
            "28 BC 5E 86 67 11 88 1C 9D 05 95 E7 9B 50 E9 63 " +
            "73 E1 78 A0 14 58 8B 97 85 E6 17 DD 18 D0 75 13 " +
            "A0 1C 87 6A 0B 2B EA A9 D5 81 CE 49 CD 12 B6 8E " +
            "67 D0 5A 74 C8 15 1B DD 3F 9E 78 E8 FD 1C F4 36 " +
            "32 90 2D 84 9D CC BE BE 39 B1 31 EE D2 17 9C F3 " +
            "74 79 02 BA 68 D3 9E 22 A0 54 F5 5F B7 2A CC CE " +
            "F9 19 AD C4 69 36 AD 02",
            // After Iota 
            "5E 7A 7E 76 63 36 A2 B1 90 74 0E 4E BF 79 EC 4A " +
            "D4 97 43 2E 28 7A 85 AD 88 4D AB 61 40 8A 0F F5 " +
            "F7 EA 81 52 FB C6 BD 69 C9 DE CE A8 A2 5B 05 24 " +
            "28 A4 25 A3 56 9B C5 38 80 1D 45 95 3E 2D A4 76 " +
            "B2 59 8A A9 CB 35 76 0D 6F 8A C6 41 54 AE 01 AF " +
            "96 18 20 2C 92 CD B7 69 68 1F 69 20 C0 67 54 67 " +
            "28 BC 5E 86 67 11 88 1C 9D 05 95 E7 9B 50 E9 63 " +
            "73 E1 78 A0 14 58 8B 97 85 E6 17 DD 18 D0 75 13 " +
            "A0 1C 87 6A 0B 2B EA A9 D5 81 CE 49 CD 12 B6 8E " +
            "67 D0 5A 74 C8 15 1B DD 3F 9E 78 E8 FD 1C F4 36 " +
            "32 90 2D 84 9D CC BE BE 39 B1 31 EE D2 17 9C F3 " +
            "74 79 02 BA 68 D3 9E 22 A0 54 F5 5F B7 2A CC CE " +
            "F9 19 AD C4 69 36 AD 02",
            // Round #10
            // After Theta
            "E1 B8 7C 7A AC 5F DB 4A 9C 23 8D 78 C0 CA 34 CC " +
            "5C DF 80 6F 06 63 01 F2 0F 8F EB 90 CB 39 52 54 " +
            "FB EA CF 01 39 6F 4D 42 76 1C CC A4 6D 32 7C DF " +
            "24 F3 A6 95 29 28 1D BE 08 55 86 D4 10 34 20 29 " +
            "35 9B CA 58 40 86 2B AC 63 8A 88 12 96 07 F1 84 " +
            "29 DA 22 20 5D A4 CE 92 64 48 EA 16 BF D4 8C E1 " +
            "A0 F4 9D C7 49 08 0C 43 1A C7 D5 16 10 E3 B4 C2 " +
            "7F E1 36 F3 D6 F1 7B BC 3A 24 15 D1 D7 B9 0C E8 " +
            "AC 4B 04 5C 74 98 32 2F 5D C9 0D 08 E3 0B 32 D1 " +
            "E0 12 1A 85 43 A6 46 7C 33 9E 36 BB 3F B5 04 1D " +
            "8D 52 2F 88 52 A5 C7 45 35 E6 B2 D8 AD A4 44 75 " +
            "FC 31 C1 FB 46 CA 1A 7D 27 96 B5 AE 3C 99 91 6F " +
            "F5 19 E3 97 AB 9F 5D 29",
            // After Rho
            "E1 B8 7C 7A AC 5F DB 4A 39 47 1A F1 80 95 69 98 " +
            "D7 37 E0 9B C1 58 80 3C 9C 23 45 F5 F0 B8 0E B9 " +
            "79 6B 12 DA 57 7F 0E C8 DA 26 C3 F7 6D C7 C1 4C " +
            "5A 99 82 D2 E1 4B 32 6F 0A 42 95 21 35 04 0D 48 " +
            "4D 65 2C 20 C3 15 D6 9A 10 4F 38 A6 88 28 61 79 " +
            "4C D1 16 01 E9 22 75 96 86 93 21 A9 5B FC 52 33 " +
            "3C 4E 42 60 18 02 A5 EF C6 69 85 35 8E AB 2D 20 " +
            "79 EB F8 3D DE BF 70 9B A2 AF 73 19 D0 75 48 2A " +
            "80 8B 0E 53 E6 85 75 89 99 E8 AE E4 06 84 F1 05 " +
            "D4 88 0F 5C 42 A3 70 C8 1D 33 9E 36 BB 3F B5 04 " +
            "1E 17 35 4A BD 20 4A 95 D5 98 CB 62 B7 92 12 D5 " +
            "3F 26 78 DF 48 59 A3 8F 96 B5 AE 3C 99 91 6F 27 " +
            "57 4A 7D C6 F8 E5 EA 67",
            // After Pi
            "E1 B8 7C 7A AC 5F DB 4A 5A 99 82 D2 E1 4B 32 6F " +
            "3C 4E 42 60 18 02 A5 EF D4 88 0F 5C 42 A3 70 C8 " +
            "57 4A 7D C6 F8 E5 EA 67 9C 23 45 F5 F0 B8 0E B9 " +
            "10 4F 38 A6 88 28 61 79 4C D1 16 01 E9 22 75 96 " +
            "80 8B 0E 53 E6 85 75 89 3F 26 78 DF 48 59 A3 8F " +
            "39 47 1A F1 80 95 69 98 0A 42 95 21 35 04 0D 48 " +
            "C6 69 85 35 8E AB 2D 20 1D 33 9E 36 BB 3F B5 04 " +
            "1E 17 35 4A BD 20 4A 95 79 6B 12 DA 57 7F 0E C8 " +
            "DA 26 C3 F7 6D C7 C1 4C 86 93 21 A9 5B FC 52 33 " +
            "99 E8 AE E4 06 84 F1 05 96 B5 AE 3C 99 91 6F 27 " +
            "D7 37 E0 9B C1 58 80 3C 4D 65 2C 20 C3 15 D6 9A " +
            "79 EB F8 3D DE BF 70 9B A2 AF 73 19 D0 75 48 2A " +
            "D5 98 CB 62 B7 92 12 D5",
            // After Chi
            "C5 FE 3C 5A B4 5F 5E CA 9A 19 8F CE A3 EA 62 6F " +
            "3F 0C 32 E2 A0 46 2F C8 74 38 0F 64 46 B9 61 C0 " +
            "4D 4B FF 46 B9 E5 CA 42 D0 B3 43 F4 91 BA 1A 3F " +
            "90 45 30 F4 8E AD 61 70 73 F5 66 8D E1 7A F7 90 " +
            "00 8A 0B 73 56 25 79 B9 3F 6A 40 DD 40 59 C2 CF " +
            "FD 6E 1A E5 0A 3E 49 B8 13 50 8F 23 04 10 9D 4C " +
            "C4 6D A4 7D 8A AB 67 B1 3C 73 94 87 BB AA 94 0C " +
            "1C 17 B0 4A 88 20 4E D5 7D FA 32 D2 45 47 1C FB " +
            "C3 4E 4D B3 69 C7 60 48 80 86 21 B1 C2 ED 5C 11 " +
            "F0 A2 BE 26 40 EA F1 CD 14 B1 6F 19 B1 11 AE 23 " +
            "E7 BD 30 86 DD F2 A0 3D CF 61 2F 20 C3 55 DE BA " +
            "2C FB 70 5F F9 3D 62 4E A0 88 53 80 90 3D C8 02 " +
            "DD D8 C7 42 B5 97 44 57",
            // After Iota 
            "CC 7E 3C DA B4 5F 5E CA 9A 19 8F CE A3 EA 62 6F " +
            "3F 0C 32 E2 A0 46 2F C8 74 38 0F 64 46 B9 61 C0 " +
            "4D 4B FF 46 B9 E5 CA 42 D0 B3 43 F4 91 BA 1A 3F " +
            "90 45 30 F4 8E AD 61 70 73 F5 66 8D E1 7A F7 90 " +
            "00 8A 0B 73 56 25 79 B9 3F 6A 40 DD 40 59 C2 CF " +
            "FD 6E 1A E5 0A 3E 49 B8 13 50 8F 23 04 10 9D 4C " +
            "C4 6D A4 7D 8A AB 67 B1 3C 73 94 87 BB AA 94 0C " +
            "1C 17 B0 4A 88 20 4E D5 7D FA 32 D2 45 47 1C FB " +
            "C3 4E 4D B3 69 C7 60 48 80 86 21 B1 C2 ED 5C 11 " +
            "F0 A2 BE 26 40 EA F1 CD 14 B1 6F 19 B1 11 AE 23 " +
            "E7 BD 30 86 DD F2 A0 3D CF 61 2F 20 C3 55 DE BA " +
            "2C FB 70 5F F9 3D 62 4E A0 88 53 80 90 3D C8 02 " +
            "DD D8 C7 42 B5 97 44 57",
            // Round #11
            // After Theta
            "40 67 3F 44 C6 CE B3 A4 A8 2F AB A8 F5 0B D1 89 " +
            "1B F9 9B 04 D5 41 64 1C 1E 6E E0 8D 5D CA B8 2F " +
            "A2 68 4D 4E AD D9 1D EF 5C AA 40 6A E3 2B F7 51 " +
            "A2 73 14 92 D8 4C D2 96 57 00 CF 6B 94 7D BC 44 " +
            "6A DC E4 9A 4D 56 A0 56 D0 49 F2 D5 54 65 15 62 " +
            "71 77 19 7B 78 AF A4 D6 21 66 AB 45 52 F1 2E AA " +
            "E0 98 0D 9B FF AC 2C 65 56 25 7B 6E A0 D9 4D E3 " +
            "F3 34 02 42 9C 1C 99 78 F1 E3 31 4C 37 D6 F1 95 " +
            "F1 78 69 D5 3F 26 D3 AE A4 73 88 57 B7 EA 17 C5 " +
            "9A F4 51 CF 5B 99 28 22 FB 92 DD 11 A5 2D 79 8E " +
            "6B A4 33 18 AF 63 4D 53 FD 57 0B 46 95 B4 6D 5C " +
            "08 0E D9 B9 8C 3A 29 9A CA DE BC 69 8B 4E 11 ED " +
            "32 FB 75 4A A1 AB 93 FA",
            // After Rho
            "40 67 3F 44 C6 CE B3 A4 51 5F 56 51 EB 17 A2 13 " +
            "46 FE 26 41 75 10 19 C7 A5 8C FB E2 E1 06 DE D8 " +
            "CD EE 78 17 45 6B 72 6A 36 BE 72 1F C5 A5 0A A4 " +
            "21 89 CD 24 6D 29 3A 47 D1 15 C0 F3 1A 65 1F 2F " +
            "6E 72 CD 26 2B 50 2B 35 56 21 06 9D 24 5F 4D 55 " +
            "8E BB CB D8 C3 7B 25 B5 A8 86 98 AD 16 49 C5 BB " +
            "D8 FC 67 65 29 03 C7 6C B3 9B C6 AD 4A F6 DC 40 " +
            "21 4E 8E 4C BC 79 1A 01 98 6E AC E3 2B E3 C7 63 " +
            "AD FA C7 64 DA 35 1E 2F 8B 62 D2 39 C4 AB 5B F5 " +
            "13 45 44 93 3E EA 79 2B 8E FB 92 DD 11 A5 2D 79 " +
            "35 4D AD 91 CE 60 BC 8E F5 5F 2D 18 55 D2 B6 71 " +
            "C1 21 3B 97 51 27 45 13 DE BC 69 8B 4E 11 ED CA " +
            "A4 BE CC 7E 9D 52 E8 EA",
            // After Pi
            "40 67 3F 44 C6 CE B3 A4 21 89 CD 24 6D 29 3A 47 " +
            "D8 FC 67 65 29 03 C7 6C 13 45 44 93 3E EA 79 2B " +
            "A4 BE CC 7E 9D 52 E8 EA A5 8C FB E2 E1 06 DE D8 " +
            "56 21 06 9D 24 5F 4D 55 8E BB CB D8 C3 7B 25 B5 " +
            "AD FA C7 64 DA 35 1E 2F C1 21 3B 97 51 27 45 13 " +
            "51 5F 56 51 EB 17 A2 13 D1 15 C0 F3 1A 65 1F 2F " +
            "B3 9B C6 AD 4A F6 DC 40 8E FB 92 DD 11 A5 2D 79 " +
            "35 4D AD 91 CE 60 BC 8E CD EE 78 17 45 6B 72 6A " +
            "36 BE 72 1F C5 A5 0A A4 A8 86 98 AD 16 49 C5 BB " +
            "8B 62 D2 39 C4 AB 5B F5 DE BC 69 8B 4E 11 ED CA " +
            "46 FE 26 41 75 10 19 C7 6E 72 CD 26 2B 50 2B 35 " +
            "21 4E 8E 4C BC 79 1A 01 98 6E AC E3 2B E3 C7 63 " +
            "F5 5F 2D 18 55 D2 B6 71",
            // After Chi
            "98 13 1D 05 C6 CC 76 8C 22 88 CD B6 7B C1 02 44 " +
            "7C 46 EF 09 A8 13 47 AC 53 04 77 93 7C 66 6A 2F " +
            "85 36 0C 5E B4 73 E0 A9 2D 16 32 A2 22 26 FE 78 " +
            "77 61 02 B9 3C 5B 57 5F CE BA F3 4B C2 79 64 A5 " +
            "89 76 07 04 7A 35 84 E7 93 00 3F 8A 55 7E 44 16 " +
            "73 D5 50 5D AB 85 62 53 DD 75 D0 A3 0B 64 3E 16 " +
            "82 9F EB AD 84 B6 4C C6 CE E9 C0 9D 30 B2 2F 68 " +
            "B5 4D 2D 33 DE 00 A1 A2 45 EE F0 B7 57 23 B7 71 " +
            "35 DE 30 0F 05 07 10 E0 FC 1A B1 2F 1C 59 61 B1 " +
            "8A 20 C2 2D C5 C1 49 D5 EC AC 6B 83 CE 95 E5 4E " +
            "47 F2 24 09 E1 39 09 C7 F6 52 ED 85 28 D2 EE 57 " +
            "44 5F 8F 54 E8 69 2A 11 9A CE AE A2 0B E3 CE E5 " +
            "DD 5F E4 3E 5F 92 94 41",
            // After Iota 
            "92 13 1D 85 C6 CC 76 8C 22 88 CD B6 7B C1 02 44 " +
            "7C 46 EF 09 A8 13 47 AC 53 04 77 93 7C 66 6A 2F " +
            "85 36 0C 5E B4 73 E0 A9 2D 16 32 A2 22 26 FE 78 " +
            "77 61 02 B9 3C 5B 57 5F CE BA F3 4B C2 79 64 A5 " +
            "89 76 07 04 7A 35 84 E7 93 00 3F 8A 55 7E 44 16 " +
            "73 D5 50 5D AB 85 62 53 DD 75 D0 A3 0B 64 3E 16 " +
            "82 9F EB AD 84 B6 4C C6 CE E9 C0 9D 30 B2 2F 68 " +
            "B5 4D 2D 33 DE 00 A1 A2 45 EE F0 B7 57 23 B7 71 " +
            "35 DE 30 0F 05 07 10 E0 FC 1A B1 2F 1C 59 61 B1 " +
            "8A 20 C2 2D C5 C1 49 D5 EC AC 6B 83 CE 95 E5 4E " +
            "47 F2 24 09 E1 39 09 C7 F6 52 ED 85 28 D2 EE 57 " +
            "44 5F 8F 54 E8 69 2A 11 9A CE AE A2 0B E3 CE E5 " +
            "DD 5F E4 3E 5F 92 94 41",
            // Round #12
            // After Theta
            "97 BB 08 92 AA 90 28 EB FC 09 F4 5B B7 6C 1F 8B " +
            "3E BC 95 24 38 BF 5F 36 FF 33 9D B2 3A 9F A6 64 " +
            "1D DA 87 52 BF 5B 0E 1B 28 BE 27 B5 4E 7A A0 1F " +
            "A9 E0 3B 54 F0 F6 4A 90 8C 40 89 66 52 D5 7C 3F " +
            "25 41 ED 25 3C CC 48 AC 0B EC B4 86 5E 56 AA A4 " +
            "76 7D 45 4A C7 D9 3C 34 03 F4 E9 4E C7 C9 23 D9 " +
            "C0 65 91 80 14 1A 54 5C 62 DE 2A BC 76 4B E3 23 " +
            "2D A1 A6 3F D5 28 4F 10 40 46 E5 A0 3B 7F E9 16 " +
            "EB 5F 09 E2 C9 AA 0D 2F BE E0 CB 02 8C F5 79 2B " +
            "26 17 28 0C 83 38 85 9E 74 40 E0 8F C5 BD 0B FC " +
            "42 5A 31 1E 8D 65 57 A0 28 D3 D4 68 E4 7F F3 98 " +
            "06 A5 F5 79 78 C5 32 8B 36 F9 44 83 4D 1A 02 AE " +
            "45 B3 6F 32 54 BA 7A F3",
            // After Rho
            "97 BB 08 92 AA 90 28 EB F9 13 E8 B7 6E D9 3E 16 " +
            "0F 6F 25 09 CE EF 97 8D F3 69 4A F6 3F D3 29 AB " +
            "DD 72 D8 E8 D0 3E 94 FA EB A4 07 FA 81 E2 7B 52 " +
            "43 05 6F AF 04 99 0A BE 0F 23 50 A2 99 54 35 DF " +
            "A0 F6 12 1E 66 24 D6 92 A5 4A BA C0 4E 6B E8 65 " +
            "B1 EB 2B 52 3A CE E6 A1 64 0F D0 A7 3B 1D 27 8F " +
            "04 A4 D0 A0 E2 02 2E 8B 96 C6 47 C4 BC 55 78 ED " +
            "9F 6A 94 27 88 96 50 D3 41 77 FE D2 2D 80 8C CA " +
            "41 3C 59 B5 E1 65 FD 2B BC 15 5F F0 65 01 C6 FA " +
            "A7 D0 D3 E4 02 85 61 10 FC 74 40 E0 8F C5 BD 0B " +
            "5D 81 0A 69 C5 78 34 96 A2 4C 53 A3 91 FF CD 63 " +
            "A0 B4 3E 0F AF 58 66 D1 F9 44 83 4D 1A 02 AE 36 " +
            "DE 7C D1 EC 9B 0C 95 AE",
            // After Pi
            "97 BB 08 92 AA 90 28 EB 43 05 6F AF 04 99 0A BE " +
            "04 A4 D0 A0 E2 02 2E 8B A7 D0 D3 E4 02 85 61 10 " +
            "DE 7C D1 EC 9B 0C 95 AE F3 69 4A F6 3F D3 29 AB " +
            "A5 4A BA C0 4E 6B E8 65 B1 EB 2B 52 3A CE E6 A1 " +
            "41 3C 59 B5 E1 65 FD 2B A0 B4 3E 0F AF 58 66 D1 " +
            "F9 13 E8 B7 6E D9 3E 16 0F 23 50 A2 99 54 35 DF " +
            "96 C6 47 C4 BC 55 78 ED FC 74 40 E0 8F C5 BD 0B " +
            "5D 81 0A 69 C5 78 34 96 DD 72 D8 E8 D0 3E 94 FA " +
            "EB A4 07 FA 81 E2 7B 52 64 0F D0 A7 3B 1D 27 8F " +
            "BC 15 5F F0 65 01 C6 FA F9 44 83 4D 1A 02 AE 36 " +
            "0F 6F 25 09 CE EF 97 8D A0 F6 12 1E 66 24 D6 92 " +
            "9F 6A 94 27 88 96 50 D3 41 77 FE D2 2D 80 8C CA " +
            "A2 4C 53 A3 91 FF CD 63",
            // After Chi
            "93 1B 98 92 48 92 0C EA E0 55 6C EB 04 1C 4B AE " +
            "5C 88 D0 A8 7B 0A BA 25 A6 53 DB F6 22 15 49 51 " +
            "9E 78 B6 C1 9F 05 97 BA E3 C8 4B E4 0F 57 2F 2B " +
            "E5 5E EA 65 8F 4A F1 6F 11 6B 0D 58 34 D6 E4 71 " +
            "12 75 19 45 F1 E6 F4 01 A4 B6 8E 0F EF 70 A6 95 " +
            "69 D7 EF F3 4A D8 76 36 67 13 50 82 9A D4 B0 DD " +
            "97 47 4D CD FC 6D 78 79 5C 66 A0 76 A5 44 B7 0B " +
            "5B A1 1A 69 54 7C 35 5F D9 79 08 ED EA 23 90 77 " +
            "73 B4 08 AA C5 E2 BB 22 25 4F 50 AA 21 1F 0F 8B " +
            "B8 27 07 50 A5 3D D6 32 DB C0 84 5F 1B C2 C5 36 " +
            "10 67 A1 28 46 7D 97 CC E0 E3 78 CE 43 24 5A 9A " +
            "3D 62 95 06 18 E9 11 F2 4C 54 DA DA 63 80 9E 46 " +
            "02 DC 41 B5 B1 FF 8D 71",
            // After Iota 
            "18 9B 98 12 48 92 0C EA E0 55 6C EB 04 1C 4B AE " +
            "5C 88 D0 A8 7B 0A BA 25 A6 53 DB F6 22 15 49 51 " +
            "9E 78 B6 C1 9F 05 97 BA E3 C8 4B E4 0F 57 2F 2B " +
            "E5 5E EA 65 8F 4A F1 6F 11 6B 0D 58 34 D6 E4 71 " +
            "12 75 19 45 F1 E6 F4 01 A4 B6 8E 0F EF 70 A6 95 " +
            "69 D7 EF F3 4A D8 76 36 67 13 50 82 9A D4 B0 DD " +
            "97 47 4D CD FC 6D 78 79 5C 66 A0 76 A5 44 B7 0B " +
            "5B A1 1A 69 54 7C 35 5F D9 79 08 ED EA 23 90 77 " +
            "73 B4 08 AA C5 E2 BB 22 25 4F 50 AA 21 1F 0F 8B " +
            "B8 27 07 50 A5 3D D6 32 DB C0 84 5F 1B C2 C5 36 " +
            "10 67 A1 28 46 7D 97 CC E0 E3 78 CE 43 24 5A 9A " +
            "3D 62 95 06 18 E9 11 F2 4C 54 DA DA 63 80 9E 46 " +
            "02 DC 41 B5 B1 FF 8D 71",
            // Round #13
            // After Theta
            "43 77 33 8E E8 2F 96 94 3F DC 52 09 B0 D0 69 4A " +
            "95 A1 08 5F 8C 5B D5 DF 14 3D 40 FC B4 3B E9 6B " +
            "34 7F 22 0F 6C 88 71 0D B8 24 E0 78 AF EA B5 55 " +
            "3A D7 D4 87 3B 86 D3 8B D8 42 D5 AF C3 87 8B 8B " +
            "A0 1B 82 4F 67 C8 54 3B 0E B1 1A C1 1C FD 40 22 " +
            "32 3B 44 6F EA 65 EC 48 B8 9A 6E 60 2E 18 92 39 " +
            "5E 6E 95 3A 0B 3C 17 83 EE 08 3B 7C 33 6A 17 31 " +
            "F1 A6 8E A7 A7 F1 D3 E8 82 95 A3 71 4A 9E 0A 09 " +
            "AC 3D 36 48 71 2E 99 C6 EC 66 88 5D D6 4E 60 71 " +
            "0A 49 9C 5A 33 13 76 08 71 C7 10 91 E8 4F 23 81 " +
            "4B 8B 0A B4 E6 C0 0D B2 3F 6A 46 2C F7 E8 78 7E " +
            "F4 4B 4D F1 EF B8 7E 08 FE 3A 41 D0 F5 AE 3E 7C " +
            "A8 DB D5 7B 42 72 6B C6",
            // After Rho
            "43 77 33 8E E8 2F 96 94 7E B8 A5 12 60 A1 D3 94 " +
            "65 28 C2 17 E3 56 F5 77 BB 93 BE 46 D1 03 C4 4F " +
            "43 8C 6B A0 F9 13 79 60 F7 AA 5E 5B 85 4B 02 8E " +
            "7D B8 63 38 BD A8 73 4D 22 B6 50 F5 EB F0 E1 E2 " +
            "0D C1 A7 33 64 AA 1D D0 0F 24 E2 10 AB 11 CC D1 " +
            "92 D9 21 7A 53 2F 63 47 E6 E0 6A BA 81 B9 60 48 " +
            "D4 59 E0 B9 18 F4 72 AB D4 2E 62 DC 11 76 F8 66 " +
            "D3 D3 F8 69 F4 78 53 C7 E3 94 3C 15 12 04 2B 47 " +
            "06 29 CE 25 D3 98 B5 C7 B0 38 76 33 C4 2E 6B 27 " +
            "C2 0E 41 21 89 53 6B 66 81 71 C7 10 91 E8 4F 23 " +
            "37 C8 2E 2D 2A D0 9A 03 FD A8 19 B1 DC A3 E3 F9 " +
            "7E A9 29 FE 1D D7 0F 81 3A 41 D0 F5 AE 3E 7C FE " +
            "9A 31 EA 76 F5 9E 90 DC",
            // After Pi
            "43 77 33 8E E8 2F 96 94 7D B8 63 38 BD A8 73 4D " +
            "D4 59 E0 B9 18 F4 72 AB C2 0E 41 21 89 53 6B 66 " +
            "9A 31 EA 76 F5 9E 90 DC BB 93 BE 46 D1 03 C4 4F " +
            "0F 24 E2 10 AB 11 CC D1 92 D9 21 7A 53 2F 63 47 " +
            "06 29 CE 25 D3 98 B5 C7 7E A9 29 FE 1D D7 0F 81 " +
            "7E B8 A5 12 60 A1 D3 94 22 B6 50 F5 EB F0 E1 E2 " +
            "D4 2E 62 DC 11 76 F8 66 81 71 C7 10 91 E8 4F 23 " +
            "37 C8 2E 2D 2A D0 9A 03 43 8C 6B A0 F9 13 79 60 " +
            "F7 AA 5E 5B 85 4B 02 8E E6 E0 6A BA 81 B9 60 48 " +
            "B0 38 76 33 C4 2E 6B 27 3A 41 D0 F5 AE 3E 7C FE " +
            "65 28 C2 17 E3 56 F5 77 0D C1 A7 33 64 AA 1D D0 " +
            "D3 D3 F8 69 F4 78 53 C7 E3 94 3C 15 12 04 2B 47 " +
            "FD A8 19 B1 DC A3 E3 F9",
            // After Chi
            "C3 36 B3 0F E8 7B 96 36 7F BE 62 38 3C AB 7A 09 " +
            "CC 68 4A EF 6C 78 E2 33 83 48 50 A9 81 72 6D 66 " +
            "A6 B9 AA 46 E0 1E F1 95 2B 4A BF 2C 81 2D E7 49 " +
            "0B 04 2C 15 2B 81 58 51 EA 59 00 A0 5F 68 69 47 " +
            "87 3B 58 25 13 98 75 89 7A 8D 69 EE 37 C7 07 11 " +
            "AA B0 87 1A 70 A7 CB 90 23 E7 D5 F5 6B 78 E6 E3 " +
            "E2 A6 4A F1 3B 66 68 66 C9 41 46 02 D1 C9 0E B7 " +
            "37 CE 7E C8 A1 80 BA 61 43 CC 4B 00 F9 A3 19 20 " +
            "E7 B2 4A 5A C1 4D 09 A9 EC A1 EA 7E AB A9 74 90 " +
            "F1 B4 5D 33 95 2F 6A 27 8E 63 C4 AE AA 76 7E 70 " +
            "B7 3A 9A 5F 73 06 B7 70 2D C5 A3 27 66 AE 35 D0 " +
            "CF FB F9 C9 38 DB 93 7F E3 94 FE 13 31 50 3F 41 " +
            "F5 69 3C 91 D8 0B EB 79",
            // After Iota 
            "48 36 B3 0F E8 7B 96 B6 7F BE 62 38 3C AB 7A 09 " +
            "CC 68 4A EF 6C 78 E2 33 83 48 50 A9 81 72 6D 66 " +
            "A6 B9 AA 46 E0 1E F1 95 2B 4A BF 2C 81 2D E7 49 " +
            "0B 04 2C 15 2B 81 58 51 EA 59 00 A0 5F 68 69 47 " +
            "87 3B 58 25 13 98 75 89 7A 8D 69 EE 37 C7 07 11 " +
            "AA B0 87 1A 70 A7 CB 90 23 E7 D5 F5 6B 78 E6 E3 " +
            "E2 A6 4A F1 3B 66 68 66 C9 41 46 02 D1 C9 0E B7 " +
            "37 CE 7E C8 A1 80 BA 61 43 CC 4B 00 F9 A3 19 20 " +
            "E7 B2 4A 5A C1 4D 09 A9 EC A1 EA 7E AB A9 74 90 " +
            "F1 B4 5D 33 95 2F 6A 27 8E 63 C4 AE AA 76 7E 70 " +
            "B7 3A 9A 5F 73 06 B7 70 2D C5 A3 27 66 AE 35 D0 " +
            "CF FB F9 C9 38 DB 93 7F E3 94 FE 13 31 50 3F 41 " +
            "F5 69 3C 91 D8 0B EB 79",
            // Round #14
            // After Theta
            "E3 93 12 1A 5B 3C BE DF 8D 1F 1F 4C 99 F6 66 CC " +
            "EF 67 E2 17 78 70 9C 8D 45 64 C8 1E 12 3E DB 42 " +
            "03 DF F3 24 21 EB 9A D5 80 EF 1E 39 32 6A CF 20 " +
            "F9 A5 51 61 8E DC 44 94 C9 56 A8 58 4B 60 17 F9 " +
            "41 17 C0 92 80 D4 C3 AD DF EB 30 8C F6 32 6C 51 " +
            "01 15 26 0F C3 E0 E3 F9 D1 46 A8 81 CE 25 FA 26 " +
            "C1 A9 E2 09 2F 6E 16 D8 0F 6D DE B5 42 85 B8 93 " +
            "92 A8 27 AA 60 75 D1 21 E8 69 EA 15 4A E4 31 49 " +
            "15 13 37 2E 64 10 15 6C CF AE 42 86 BF A1 0A 2E " +
            "37 98 C5 84 06 63 DC 03 2B 05 9D CC 6B 83 15 30 " +
            "1C 9F 3B 4A C0 41 9F 19 DF 64 DE 53 C3 F3 29 15 " +
            "EC F4 51 31 2C D3 ED C1 25 B8 66 A4 A2 1C 89 65 " +
            "50 0F 65 F3 19 FE 80 39",
            // After Rho
            "E3 93 12 1A 5B 3C BE DF 1B 3F 3E 98 32 ED CD 98 " +
            "FB 99 F8 05 1E 1C 67 E3 E1 B3 2D 54 44 86 EC 21 " +
            "59 D7 AC 1E F8 9E 27 09 23 A3 F6 0C 02 F8 EE 91 " +
            "15 E6 C8 4D 44 99 5F 1A 7E B2 15 2A D6 12 D8 45 " +
            "0B 60 49 40 EA E1 D6 A0 C3 16 F5 BD 0E C3 68 2F " +
            "0F A8 30 79 18 06 1F CF 9B 44 1B A1 06 3A 97 E8 " +
            "4F 78 71 B3 C0 0E 4E 15 0A 71 27 1F DA BC 6B 85 " +
            "55 B0 BA E8 10 49 D4 13 2B 94 C8 63 92 D0 D3 D4 " +
            "C6 85 0C A2 82 AD 62 E2 05 97 67 57 21 C3 DF 50 " +
            "8C 7B E0 06 B3 98 D0 60 30 2B 05 9D CC 6B 83 15 " +
            "7D 66 70 7C EE 28 01 07 7C 93 79 4F 0D CF A7 54 " +
            "9D 3E 2A 86 65 BA 3D 98 B8 66 A4 A2 1C 89 65 25 " +
            "60 0E D4 43 D9 7C 86 3F",
            // After Pi
            "E3 93 12 1A 5B 3C BE DF 15 E6 C8 4D 44 99 5F 1A " +
            "4F 78 71 B3 C0 0E 4E 15 8C 7B E0 06 B3 98 D0 60 " +
            "60 0E D4 43 D9 7C 86 3F E1 B3 2D 54 44 86 EC 21 " +
            "C3 16 F5 BD 0E C3 68 2F 0F A8 30 79 18 06 1F CF " +
            "C6 85 0C A2 82 AD 62 E2 9D 3E 2A 86 65 BA 3D 98 " +
            "1B 3F 3E 98 32 ED CD 98 7E B2 15 2A D6 12 D8 45 " +
            "0A 71 27 1F DA BC 6B 85 30 2B 05 9D CC 6B 83 15 " +
            "7D 66 70 7C EE 28 01 07 59 D7 AC 1E F8 9E 27 09 " +
            "23 A3 F6 0C 02 F8 EE 91 9B 44 1B A1 06 3A 97 E8 " +
            "05 97 67 57 21 C3 DF 50 B8 66 A4 A2 1C 89 65 25 " +
            "FB 99 F8 05 1E 1C 67 E3 0B 60 49 40 EA E1 D6 A0 " +
            "55 B0 BA E8 10 49 D4 13 2B 94 C8 63 92 D0 D3 D4 " +
            "7C 93 79 4F 0D CF A7 54",
            // After Chi
            "A9 8B 23 A8 DB 3A BE DA 95 E5 48 49 77 09 CF 7A " +
            "2F 7C 65 F2 88 6A 48 0A 0F EA E2 1E B1 98 E8 A0 " +
            "74 6A 1C 06 DD FD C7 3F ED 1B 2D 14 54 82 FB E1 " +
            "03 13 F9 3F 8C 6A 08 0F 16 92 12 7D 7D 14 02 D7 " +
            "A6 04 09 F2 82 A9 A2 C3 9F 3A FA 2F 6F FB 3D 96 " +
            "1B 7E 1C 8D 3A 41 EE 18 4E B8 15 AA D2 51 58 55 " +
            "47 35 57 7F F8 BC 6B 87 32 32 0B 1D DC AE 4F 8D " +
            "19 E6 71 5E 2A 3A 11 42 C1 93 A5 BF FC 9C 36 61 " +
            "27 30 92 5A 23 39 A6 81 23 24 9B 01 1A 32 B7 CD " +
            "44 06 6F 4B C1 D5 DD 58 9A 46 F6 A2 1E E9 AD B5 " +
            "AF 09 4A AD 0E 14 67 F0 21 64 09 43 68 71 D5 64 " +
            "01 B3 8B E4 1D 46 F0 13 A8 9C 48 63 80 C0 93 77 " +
            "7C F3 78 0F ED 2E 37 54",
            // After Iota 
            "20 0B 23 A8 DB 3A BE 5A 95 E5 48 49 77 09 CF 7A " +
            "2F 7C 65 F2 88 6A 48 0A 0F EA E2 1E B1 98 E8 A0 " +
            "74 6A 1C 06 DD FD C7 3F ED 1B 2D 14 54 82 FB E1 " +
            "03 13 F9 3F 8C 6A 08 0F 16 92 12 7D 7D 14 02 D7 " +
            "A6 04 09 F2 82 A9 A2 C3 9F 3A FA 2F 6F FB 3D 96 " +
            "1B 7E 1C 8D 3A 41 EE 18 4E B8 15 AA D2 51 58 55 " +
            "47 35 57 7F F8 BC 6B 87 32 32 0B 1D DC AE 4F 8D " +
            "19 E6 71 5E 2A 3A 11 42 C1 93 A5 BF FC 9C 36 61 " +
            "27 30 92 5A 23 39 A6 81 23 24 9B 01 1A 32 B7 CD " +
            "44 06 6F 4B C1 D5 DD 58 9A 46 F6 A2 1E E9 AD B5 " +
            "AF 09 4A AD 0E 14 67 F0 21 64 09 43 68 71 D5 64 " +
            "01 B3 8B E4 1D 46 F0 13 A8 9C 48 63 80 C0 93 77 " +
            "7C F3 78 0F ED 2E 37 54",
            // Round #15
            // After Theta
            "89 3D 44 F8 75 35 17 DB 94 89 D5 40 24 14 F8 40 " +
            "1E EA D4 84 B7 05 33 4D 7B A0 E0 BF 6C D8 6D 30 " +
            "73 C5 20 98 FD 95 78 9B 44 2D 4A 44 FA 8D 52 60 " +
            "02 7F 64 36 DF 77 3F 35 27 04 A3 0B 42 7B 79 90 " +
            "D2 4E 0B 53 5F E9 27 53 98 95 C6 B1 4F 93 82 32 " +
            "B2 48 7B DD 94 4E 47 99 4F D4 88 A3 81 4C 6F 6F " +
            "76 A3 E6 09 C7 D3 10 C0 46 78 09 BC 01 EE CA 1D " +
            "1E 49 4D C0 0A 52 AE E6 68 A5 C2 EF 52 93 9F E0 " +
            "26 5C 0F 53 70 24 91 BB 12 B2 2A 77 25 5D CC 8A " +
            "30 4C 6D EA 1C 95 58 C8 9D E9 CA 3C 3E 81 12 11 " +
            "06 3F 2D FD A0 1B CE 71 20 08 94 4A 3B 6C E2 5E " +
            "30 25 3A 92 22 29 8B 54 DC D6 4A C2 5D 80 16 E7 " +
            "7B 5C 44 91 CD 46 88 F0",
            // After Rho
            "89 3D 44 F8 75 35 17 DB 28 13 AB 81 48 28 F0 81 " +
            "87 3A 35 E1 6D C1 4C 93 86 DD 06 B3 07 0A FE CB " +
            "AF C4 DB 9C 2B 06 C1 EC A4 DF 28 05 46 D4 A2 44 " +
            "66 F3 7D F7 53 23 F0 47 E4 09 C1 E8 82 D0 5E 1E " +
            "A7 85 A9 AF F4 93 29 69 29 28 83 59 69 1C FB 34 " +
            "94 45 DA EB A6 74 3A CA BD 3D 51 23 8E 06 32 BD " +
            "4F 38 9E 86 00 B6 1B 35 DC 95 3B 8C F0 12 78 03 " +
            "60 05 29 57 73 8F A4 26 DF A5 26 3F C1 D1 4A 85 " +
            "61 0A 8E 24 72 D7 84 EB 66 45 09 59 95 BB 92 2E " +
            "12 0B 19 86 A9 4D 9D A3 11 9D E9 CA 3C 3E 81 12 " +
            "38 C7 19 FC B4 F4 83 6E 81 20 50 2A ED B0 89 7B " +
            "A6 44 47 52 24 65 91 0A D6 4A C2 5D 80 16 E7 DC " +
            "22 FC 1E 17 51 64 B3 11",
            // After Pi
            "89 3D 44 F8 75 35 17 DB 66 F3 7D F7 53 23 F0 47 " +
            "4F 38 9E 86 00 B6 1B 35 12 0B 19 86 A9 4D 9D A3 " +
            "22 FC 1E 17 51 64 B3 11 86 DD 06 B3 07 0A FE CB " +
            "29 28 83 59 69 1C FB 34 94 45 DA EB A6 74 3A CA " +
            "61 0A 8E 24 72 D7 84 EB A6 44 47 52 24 65 91 0A " +
            "28 13 AB 81 48 28 F0 81 E4 09 C1 E8 82 D0 5E 1E " +
            "DC 95 3B 8C F0 12 78 03 11 9D E9 CA 3C 3E 81 12 " +
            "38 C7 19 FC B4 F4 83 6E AF C4 DB 9C 2B 06 C1 EC " +
            "A4 DF 28 05 46 D4 A2 44 BD 3D 51 23 8E 06 32 BD " +
            "66 45 09 59 95 BB 92 2E D6 4A C2 5D 80 16 E7 DC " +
            "87 3A 35 E1 6D C1 4C 93 A7 85 A9 AF F4 93 29 69 " +
            "60 05 29 57 73 8F A4 26 DF A5 26 3F C1 D1 4A 85 " +
            "81 20 50 2A ED B0 89 7B",
            // After Chi
            "80 35 C6 F8 75 A1 1C EB 76 F0 7C F7 FA 6A 74 C5 " +
            "6F CC 98 97 50 96 39 25 9B 0A 59 6E 8D 5C 99 69 " +
            "44 3E 27 10 53 66 53 15 12 98 5E 11 81 6A FE 01 " +
            "48 22 87 5D 39 9F 7F 15 12 01 9B B9 A2 54 2B CA " +
            "61 93 8E 85 71 DD EA 2A 8F 64 C6 1A 4C 71 90 3E " +
            "30 87 91 85 38 2A D0 80 E5 01 01 AA 8E FC DF 0E " +
            "F4 D7 2B B8 70 D2 7A 6F 11 8D 4B CB 74 36 F1 93 " +
            "FC CF 59 94 36 24 8D 70 B6 E4 8A BE A3 04 D1 55 " +
            "E6 9F 20 5D 57 6D 22 46 2D 37 93 27 8E 02 57 6D " +
            "4F C1 10 D9 BE BB 92 0E D6 51 E2 5C C4 C6 C5 DC " +
            "C7 3A 35 B1 6E CD C8 95 38 25 AF 87 74 C3 63 E8 " +
            "60 05 79 57 5F AF 25 5C D9 BF 03 FE C1 90 0E 05 " +
            "A1 A5 D8 24 7D A2 A8 13",
            // After Iota 
            "83 B5 C6 F8 75 A1 1C 6B 76 F0 7C F7 FA 6A 74 C5 " +
            "6F CC 98 97 50 96 39 25 9B 0A 59 6E 8D 5C 99 69 " +
            "44 3E 27 10 53 66 53 15 12 98 5E 11 81 6A FE 01 " +
            "48 22 87 5D 39 9F 7F 15 12 01 9B B9 A2 54 2B CA " +
            "61 93 8E 85 71 DD EA 2A 8F 64 C6 1A 4C 71 90 3E " +
            "30 87 91 85 38 2A D0 80 E5 01 01 AA 8E FC DF 0E " +
            "F4 D7 2B B8 70 D2 7A 6F 11 8D 4B CB 74 36 F1 93 " +
            "FC CF 59 94 36 24 8D 70 B6 E4 8A BE A3 04 D1 55 " +
            "E6 9F 20 5D 57 6D 22 46 2D 37 93 27 8E 02 57 6D " +
            "4F C1 10 D9 BE BB 92 0E D6 51 E2 5C C4 C6 C5 DC " +
            "C7 3A 35 B1 6E CD C8 95 38 25 AF 87 74 C3 63 E8 " +
            "60 05 79 57 5F AF 25 5C D9 BF 03 FE C1 90 0E 05 " +
            "A1 A5 D8 24 7D A2 A8 13",
            // Round #16
            // After Theta
            "C9 06 AE AA 38 B8 14 1E 2F D5 4E 59 5C 38 6A 8D " +
            "91 71 F3 42 D0 08 91 E3 DE E0 9F 45 FF 4E C5 F0 " +
            "99 BD C4 D0 A6 AA 1B 9A 58 2B 36 43 CC 73 F6 74 " +
            "11 07 B5 F3 9F CD 61 5D EC BC F0 6C 22 CA 83 0C " +
            "24 79 48 AE 03 CF B6 B3 52 E7 25 DA B9 BD D8 B1 " +
            "7A 34 F9 D7 75 33 D8 F5 BC 24 33 04 28 AE C1 46 " +
            "0A 6A 40 6D F0 4C D2 A9 54 67 8D E0 06 24 AD 0A " +
            "21 4C BA 54 C3 E8 C5 FF FC 57 E2 EC EE 1D D9 20 " +
            "BF BA 12 F3 F1 3F 3C 0E D3 8A F8 F2 0E 9C FF AB " +
            "0A 2B D6 F2 CC A9 CE 97 0B D2 01 9C 31 0A 8D 53 " +
            "8D 89 5D E3 23 D4 C0 E0 61 00 9D 29 D2 91 7D A0 " +
            "9E B8 12 82 DF 31 8D 9A 9C 55 C5 D5 B3 82 52 9C " +
            "7C 26 3B E4 88 6E E0 9C",
            // After Rho
            "C9 06 AE AA 38 B8 14 1E 5F AA 9D B2 B8 70 D4 1A " +
            "64 DC BC 10 34 42 E4 78 EF 54 0C EF 0D FE 59 F4 " +
            "55 DD D0 CC EC 25 86 36 C4 3C 67 4F 87 B5 62 33 " +
            "3B FF D9 1C D6 15 71 50 03 3B 2F 3C 9B 88 F2 20 " +
            "3C 24 D7 81 67 DB 59 92 8B 1D 2B 75 5E A2 9D DB " +
            "D7 A3 C9 BF AE 9B C1 AE 1B F1 92 CC 10 A0 B8 06 " +
            "6A 83 67 92 4E 55 50 03 48 5A 15 A8 CE 1A C1 0D " +
            "AA 61 F4 E2 FF 10 26 5D D9 DD 3B B2 41 F8 AF C4 " +
            "62 3E FE 87 C7 E1 57 57 FF D5 69 45 7C 79 07 CE " +
            "D5 F9 52 61 C5 5A 9E 39 53 0B D2 01 9C 31 0A 8D " +
            "03 83 37 26 76 8D 8F 50 86 01 74 A6 48 47 F6 81 " +
            "13 57 42 F0 3B A6 51 D3 55 C5 D5 B3 82 52 9C 9C " +
            "38 27 9F C9 0E 39 A2 1B",
            // After Pi
            "C9 06 AE AA 38 B8 14 1E 3B FF D9 1C D6 15 71 50 " +
            "6A 83 67 92 4E 55 50 03 D5 F9 52 61 C5 5A 9E 39 " +
            "38 27 9F C9 0E 39 A2 1B EF 54 0C EF 0D FE 59 F4 " +
            "8B 1D 2B 75 5E A2 9D DB D7 A3 C9 BF AE 9B C1 AE " +
            "62 3E FE 87 C7 E1 57 57 13 57 42 F0 3B A6 51 D3 " +
            "5F AA 9D B2 B8 70 D4 1A 03 3B 2F 3C 9B 88 F2 20 " +
            "48 5A 15 A8 CE 1A C1 0D 53 0B D2 01 9C 31 0A 8D " +
            "03 83 37 26 76 8D 8F 50 55 DD D0 CC EC 25 86 36 " +
            "C4 3C 67 4F 87 B5 62 33 1B F1 92 CC 10 A0 B8 06 " +
            "FF D5 69 45 7C 79 07 CE 55 C5 D5 B3 82 52 9C 9C " +
            "64 DC BC 10 34 42 E4 78 3C 24 D7 81 67 DB 59 92 " +
            "AA 61 F4 E2 FF 10 26 5D D9 DD 3B B2 41 F8 AF C4 " +
            "86 01 74 A6 48 47 F6 81",
            // After Chi
            "89 06 88 28 30 F8 14 1D AE 87 C9 7D 57 1F FF 68 " +
            "42 85 EA 1A 44 74 70 01 14 F9 72 43 F5 DA 8A 3D " +
            "0A DE CE DD C8 3C C3 5B BB F6 CC 65 AD E7 19 D0 " +
            "AB 01 1D 75 1F C2 8B 8A C6 E2 C9 CF 96 9D C1 2E " +
            "8E 3E F2 88 C3 B9 5F 73 13 5E 61 E0 69 A6 D5 D8 " +
            "17 EA 8D 32 FC 62 D5 17 10 3A ED 3D 8B A9 F8 A0 " +
            "48 DA 30 8E AC 96 44 5D 0F 23 5A 91 14 41 5A 87 " +
            "03 92 15 2A 75 05 AD 70 4E 1C 40 4C FC 25 1E 32 " +
            "20 38 0E 4E EB EC 65 FB 1B F1 06 7E 92 A2 20 16 " +
            "FF CD 69 09 10 5C 05 EC D5 E5 F2 B0 81 C2 FC 9D " +
            "E6 9D 9C 72 AC 42 C2 35 6D B8 DC 91 67 33 D0 12 " +
            "AC 61 B0 E6 F7 17 76 5C B9 01 B3 A2 75 F8 AF BC " +
            "9E 21 37 27 0B DE EF 03",
            // After Iota 
            "8B 86 88 28 30 F8 14 9D AE 87 C9 7D 57 1F FF 68 " +
            "42 85 EA 1A 44 74 70 01 14 F9 72 43 F5 DA 8A 3D " +
            "0A DE CE DD C8 3C C3 5B BB F6 CC 65 AD E7 19 D0 " +
            "AB 01 1D 75 1F C2 8B 8A C6 E2 C9 CF 96 9D C1 2E " +
            "8E 3E F2 88 C3 B9 5F 73 13 5E 61 E0 69 A6 D5 D8 " +
            "17 EA 8D 32 FC 62 D5 17 10 3A ED 3D 8B A9 F8 A0 " +
            "48 DA 30 8E AC 96 44 5D 0F 23 5A 91 14 41 5A 87 " +
            "03 92 15 2A 75 05 AD 70 4E 1C 40 4C FC 25 1E 32 " +
            "20 38 0E 4E EB EC 65 FB 1B F1 06 7E 92 A2 20 16 " +
            "FF CD 69 09 10 5C 05 EC D5 E5 F2 B0 81 C2 FC 9D " +
            "E6 9D 9C 72 AC 42 C2 35 6D B8 DC 91 67 33 D0 12 " +
            "AC 61 B0 E6 F7 17 76 5C B9 01 B3 A2 75 F8 AF BC " +
            "9E 21 37 27 0B DE EF 03",
            // Round #17
            // After Theta
            "6B 28 21 7D F1 2D CF A6 D7 C6 96 BB 51 91 BC 44 " +
            "BD E8 01 12 84 D3 02 98 CD 78 28 80 53 16 78 DE " +
            "C7 C1 E4 AE ED 8E EE 78 5B 58 65 30 6C 32 C2 EB " +
            "D2 40 42 B3 19 4C C8 A6 39 8F 22 C7 56 3A B3 B7 " +
            "57 BF A8 4B 65 75 AD 90 DE 41 4B 93 4C 14 F8 FB " +
            "F7 44 24 67 3D B7 0E 2C 69 7B B2 FB 8D 27 BB 8C " +
            "B7 B7 DB 86 6C 31 36 C4 D6 A2 00 52 B2 8D A8 64 " +
            "CE 8D 3F 59 50 B7 80 53 AE B2 E9 19 3D F0 C5 09 " +
            "59 79 51 88 ED 62 26 D7 E4 9C ED 76 52 05 52 8F " +
            "26 4C 33 CA B6 90 F7 0F 18 FA D8 C3 A4 70 D1 BE " +
            "06 33 35 27 6D 97 19 0E 14 F9 83 57 61 BD 93 3E " +
            "53 0C 5B EE 37 B0 04 C5 60 80 E9 61 D3 34 5D 5F " +
            "53 3E 1D 54 2E 6C C2 20",
            // After Rho
            "6B 28 21 7D F1 2D CF A6 AE 8D 2D 77 A3 22 79 89 " +
            "2F 7A 80 04 E1 B4 00 66 65 81 E7 DD 8C 87 02 38 " +
            "77 74 C7 3B 0E 26 77 6D C3 26 23 BC BE 85 55 06 " +
            "34 9B C1 84 6C 2A 0D 24 6D CE A3 C8 B1 95 CE EC " +
            "5F D4 A5 B2 BA 56 C8 AB 81 BF EF 1D B4 34 C9 44 " +
            "B9 27 22 39 EB B9 75 60 32 A6 ED C9 EE 37 9E EC " +
            "36 64 8B B1 21 BE BD DD 1B 51 C9 AC 45 01 A4 64 " +
            "2C A8 5B C0 29 E7 C6 9F 33 7A E0 8B 13 5C 65 D3 " +
            "0A B1 5D CC E4 3A 2B 2F A9 47 72 CE 76 3B A9 02 " +
            "F2 FE C1 84 69 46 D9 16 BE 18 FA D8 C3 A4 70 D1 " +
            "66 38 18 CC D4 9C B4 5D 50 E4 0F 5E 85 F5 4E FA " +
            "8A 61 CB FD 06 96 A0 78 80 E9 61 D3 34 5D 5F 60 " +
            "30 C8 94 4F 07 95 0B 9B",
            // After Pi
            "6B 28 21 7D F1 2D CF A6 34 9B C1 84 6C 2A 0D 24 " +
            "36 64 8B B1 21 BE BD DD F2 FE C1 84 69 46 D9 16 " +
            "30 C8 94 4F 07 95 0B 9B 65 81 E7 DD 8C 87 02 38 " +
            "81 BF EF 1D B4 34 C9 44 B9 27 22 39 EB B9 75 60 " +
            "0A B1 5D CC E4 3A 2B 2F 8A 61 CB FD 06 96 A0 78 " +
            "AE 8D 2D 77 A3 22 79 89 6D CE A3 C8 B1 95 CE EC " +
            "1B 51 C9 AC 45 01 A4 64 BE 18 FA D8 C3 A4 70 D1 " +
            "66 38 18 CC D4 9C B4 5D 77 74 C7 3B 0E 26 77 6D " +
            "C3 26 23 BC BE 85 55 06 32 A6 ED C9 EE 37 9E EC " +
            "A9 47 72 CE 76 3B A9 02 80 E9 61 D3 34 5D 5F 60 " +
            "2F 7A 80 04 E1 B4 00 66 5F D4 A5 B2 BA 56 C8 AB " +
            "2C A8 5B C0 29 E7 C6 9F 33 7A E0 8B 13 5C 65 D3 " +
            "50 E4 0F 5E 85 F5 4E FA",
            // After Chi
            "69 4C 2B 4C F0 B9 7F 7F F4 01 81 80 24 6A 4D 26 " +
            "36 64 9F FA 27 2F BF 54 B9 DE E0 B4 99 6E 1D 32 " +
            "24 5B 54 CF 0B 97 0B 9B 5D 81 E7 FD C7 0E 36 18 " +
            "83 2F B2 D9 B0 36 C3 4B 39 67 A0 08 E9 3D F5 30 " +
            "6F 31 79 CC 6C 3B 29 2F 0A 5F C3 FD 36 A6 69 3C " +
            "BC 9C 65 53 E7 22 59 89 C9 C6 91 98 33 31 9E 7D " +
            "5B 71 C9 A8 51 19 20 68 36 9D DF EB E0 86 39 51 " +
            "27 7A 9A 44 C4 09 32 39 47 F4 0B 7A 4E 14 FD 85 " +
            "4A 67 31 BA AE 8D 74 04 32 0E EC D8 EE 73 C8 8C " +
            "DE 53 F4 E6 7C 19 89 0F 00 EB 41 57 84 DC 5F 62 " +
            "0F 52 DA 44 E0 15 06 72 4C 86 05 B9 A8 4E E9 EB " +
            "6C 2C 54 94 AD 46 CC B7 1C 60 60 8B 73 5C 65 D7 " +
            "00 60 2A EC 9F B7 86 73",
            // After Iota 
            "E9 4C 2B 4C F0 B9 7F FF F4 01 81 80 24 6A 4D 26 " +
            "36 64 9F FA 27 2F BF 54 B9 DE E0 B4 99 6E 1D 32 " +
            "24 5B 54 CF 0B 97 0B 9B 5D 81 E7 FD C7 0E 36 18 " +
            "83 2F B2 D9 B0 36 C3 4B 39 67 A0 08 E9 3D F5 30 " +
            "6F 31 79 CC 6C 3B 29 2F 0A 5F C3 FD 36 A6 69 3C " +
            "BC 9C 65 53 E7 22 59 89 C9 C6 91 98 33 31 9E 7D " +
            "5B 71 C9 A8 51 19 20 68 36 9D DF EB E0 86 39 51 " +
            "27 7A 9A 44 C4 09 32 39 47 F4 0B 7A 4E 14 FD 85 " +
            "4A 67 31 BA AE 8D 74 04 32 0E EC D8 EE 73 C8 8C " +
            "DE 53 F4 E6 7C 19 89 0F 00 EB 41 57 84 DC 5F 62 " +
            "0F 52 DA 44 E0 15 06 72 4C 86 05 B9 A8 4E E9 EB " +
            "6C 2C 54 94 AD 46 CC B7 1C 60 60 8B 73 5C 65 D7 " +
            "00 60 2A EC 9F B7 86 73",
            // Round #18
            // After Theta
            "91 AA 61 04 51 B7 ED 8F A0 56 65 70 E2 83 7A D1 " +
            "CB EF AD C5 B3 AD F1 82 A0 64 63 38 80 F7 61 1A " +
            "87 F4 77 89 EC 29 3D 3C 25 67 AD B5 66 00 A4 68 " +
            "D7 78 56 29 76 DF F4 BC C4 EC 92 37 7D BF BB E6 " +
            "76 8B FA 40 75 A2 55 07 A9 F0 E0 BB D1 18 5F 9B " +
            "C4 7A 2F 1B 46 2C CB F9 9D 91 75 68 F5 D8 A9 8A " +
            "A6 FA FB 97 C5 9B 6E BE 2F 27 5C 67 F9 1F 45 79 " +
            "84 D5 B9 02 23 B7 04 9E 3F 12 41 32 EF 1A 6F F5 " +
            "1E 30 D5 4A 68 64 43 F3 CF 85 DE E7 7A F1 86 5A " +
            "C7 E9 77 6A 65 80 F5 27 A3 44 62 11 63 62 69 C5 " +
            "77 B4 90 0C 41 1B 94 02 18 D1 E1 49 6E A7 DE 1C " +
            "91 A7 66 AB 39 C4 82 61 05 DA E3 07 6A C5 19 FF " +
            "A3 CF 09 AA 78 09 B0 D4",
            // After Rho
            "91 AA 61 04 51 B7 ED 8F 41 AD CA E0 C4 07 F5 A2 " +
            "F2 7B 6B F1 6C 6B BC E0 78 1F A6 01 4A 36 86 03 " +
            "4F E9 E1 39 A4 BF 4B 64 6B 06 40 8A 56 72 D6 5A " +
            "95 62 F7 4D CF 7B 8D 67 39 31 BB E4 4D DF EF AE " +
            "45 7D A0 3A D1 AA 03 BB F1 B5 99 0A 0F BE 1B 8D " +
            "27 D6 7B D9 30 62 59 CE 2A 76 46 D6 A1 D5 63 A7 " +
            "BF 2C DE 74 F3 35 D5 DF 3F 8A F2 5E 4E B8 CE F2 " +
            "81 91 5B 02 4F C2 EA 5C 64 DE 35 DE EA 7F 24 82 " +
            "5A 09 8D 6C 68 DE 03 A6 43 AD E7 42 EF 73 BD 78 " +
            "B0 FE E4 38 FD 4E AD 0C C5 A3 44 62 11 63 62 69 " +
            "50 0A DC D1 42 32 04 6D 60 44 87 27 B9 9D 7A 73 " +
            "F2 D4 6C 35 87 58 30 2C DA E3 07 6A C5 19 FF 05 " +
            "2C F5 E8 73 82 2A 5E 02",
            // After Pi
            "91 AA 61 04 51 B7 ED 8F 95 62 F7 4D CF 7B 8D 67 " +
            "BF 2C DE 74 F3 35 D5 DF B0 FE E4 38 FD 4E AD 0C " +
            "2C F5 E8 73 82 2A 5E 02 78 1F A6 01 4A 36 86 03 " +
            "F1 B5 99 0A 0F BE 1B 8D 27 D6 7B D9 30 62 59 CE " +
            "5A 09 8D 6C 68 DE 03 A6 F2 D4 6C 35 87 58 30 2C " +
            "41 AD CA E0 C4 07 F5 A2 39 31 BB E4 4D DF EF AE " +
            "3F 8A F2 5E 4E B8 CE F2 C5 A3 44 62 11 63 62 69 " +
            "50 0A DC D1 42 32 04 6D 4F E9 E1 39 A4 BF 4B 64 " +
            "6B 06 40 8A 56 72 D6 5A 2A 76 46 D6 A1 D5 63 A7 " +
            "43 AD E7 42 EF 73 BD 78 DA E3 07 6A C5 19 FF 05 " +
            "F2 7B 6B F1 6C 6B BC E0 45 7D A0 3A D1 AA 03 BB " +
            "81 91 5B 02 4F C2 EA 5C 64 DE 35 DE EA 7F 24 82 " +
            "60 44 87 27 B9 9D 7A 73",
            // After Chi
            "BB A6 69 34 61 B3 BD 17 95 B0 D7 45 C3 31 A5 67 " +
            "B3 2D D6 37 F1 15 87 DD 21 F4 E5 3C AC DB 0C 81 " +
            "28 B5 7E 3A 0C 62 5E 62 7E 5D C4 D0 7A 76 C6 41 " +
            "A9 BC 1D 2E 47 22 19 AD 87 02 1B C8 B7 62 69 C6 " +
            "52 02 0F 6C 20 F8 85 A5 73 74 75 3F 82 D0 29 A0 " +
            "47 27 8A FA C6 27 F5 F2 F9 10 BF C4 5C 9C CF A7 " +
            "2F 82 6A CF 0C A8 CA F6 C4 06 46 42 95 66 93 EB " +
            "68 1A ED D5 4B EA 0E 61 4F 99 E7 6D 05 3A 6A C1 " +
            "2A 8F E1 8A 18 50 4A 02 B2 34 46 FE A1 DD 21 A2 " +
            "46 A5 07 53 CF D5 BD 18 FA E5 07 E8 97 59 6B 1F " +
            "72 FB 30 F1 62 2B 54 A4 21 33 84 E6 71 97 07 39 " +
            "81 91 D9 23 5E 42 B0 2D F6 E5 5D 0E AE 1D A0 02 " +
            "65 40 07 2D 28 1D 79 68",
            // After Iota 
            "B1 26 69 34 61 B3 BD 17 95 B0 D7 45 C3 31 A5 67 " +
            "B3 2D D6 37 F1 15 87 DD 21 F4 E5 3C AC DB 0C 81 " +
            "28 B5 7E 3A 0C 62 5E 62 7E 5D C4 D0 7A 76 C6 41 " +
            "A9 BC 1D 2E 47 22 19 AD 87 02 1B C8 B7 62 69 C6 " +
            "52 02 0F 6C 20 F8 85 A5 73 74 75 3F 82 D0 29 A0 " +
            "47 27 8A FA C6 27 F5 F2 F9 10 BF C4 5C 9C CF A7 " +
            "2F 82 6A CF 0C A8 CA F6 C4 06 46 42 95 66 93 EB " +
            "68 1A ED D5 4B EA 0E 61 4F 99 E7 6D 05 3A 6A C1 " +
            "2A 8F E1 8A 18 50 4A 02 B2 34 46 FE A1 DD 21 A2 " +
            "46 A5 07 53 CF D5 BD 18 FA E5 07 E8 97 59 6B 1F " +
            "72 FB 30 F1 62 2B 54 A4 21 33 84 E6 71 97 07 39 " +
            "81 91 D9 23 5E 42 B0 2D F6 E5 5D 0E AE 1D A0 02 " +
            "65 40 07 2D 28 1D 79 68",
            // Round #19
            // After Theta
            "81 19 AE A7 78 3E AA 6F 70 9E 57 1D 12 43 7F 63 " +
            "72 ED 2B 6B B0 47 B6 21 50 01 11 FA ED A3 6F 4B " +
            "44 78 68 70 01 08 38 34 4E 62 03 43 63 FB D1 39 " +
            "4C 92 9D 76 96 50 C3 A9 46 C2 E6 94 F6 30 58 3A " +
            "23 F7 FB AA 61 80 E6 6F 1F B9 63 75 8F BA 4F F6 " +
            "77 18 4D 69 DF AA E2 8A 1C 3E 3F 9C 8D EE 15 A3 " +
            "EE 42 97 93 4D FA FB 0A B5 F3 B2 84 D4 1E F0 21 " +
            "04 D7 FB 9F 46 80 68 37 7F A6 20 FE 1C B7 7D B9 " +
            "CF A1 61 D2 C9 22 90 06 73 F4 BB A2 E0 8F 10 5E " +
            "37 50 F3 95 8E AD DE D2 96 28 11 A2 9A 33 0D 49 " +
            "42 C4 F7 62 7B A6 43 DC C4 1D 04 BE A0 E5 DD 3D " +
            "40 51 24 7F 1F 10 81 D1 87 10 A9 C8 EF 65 C3 C8 " +
            "09 8D 11 67 25 77 1F 3E",
            // After Rho
            "81 19 AE A7 78 3E AA 6F E0 3C AF 3A 24 86 FE C6 " +
            "5C FB CA 1A EC 91 6D 88 3E FA B6 04 15 10 A1 DF " +
            "40 C0 A1 21 C2 43 83 0B 34 B6 1F 9D E3 24 36 30 " +
            "69 67 09 35 9C CA 24 D9 8E 91 B0 39 A5 3D 0C 96 " +
            "FB 7D D5 30 40 F3 B7 91 FB 64 FF 91 3B 56 F7 A8 " +
            "BC C3 68 4A FB 56 15 57 8C 72 F8 FC 70 36 BA 57 " +
            "9C 6C D2 DF 57 70 17 BA 3D E0 43 6A E7 65 09 A9 " +
            "4F 23 40 B4 1B 82 EB FD FC 39 6E FB 72 FF 4C 41 " +
            "4C 3A 59 04 D2 E0 39 34 08 AF 39 FA 5D 51 F0 47 " +
            "D5 5B FA 06 6A BE D2 B1 49 96 28 11 A2 9A 33 0D " +
            "0E 71 0B 11 DF 8B ED 99 10 77 10 F8 82 96 77 F7 " +
            "28 8A E4 EF 03 22 30 1A 10 A9 C8 EF 65 C3 C8 87 " +
            "87 4F 42 63 C4 59 C9 DD",
            // After Pi
            "81 19 AE A7 78 3E AA 6F 69 67 09 35 9C CA 24 D9 " +
            "9C 6C D2 DF 57 70 17 BA D5 5B FA 06 6A BE D2 B1 " +
            "87 4F 42 63 C4 59 C9 DD 3E FA B6 04 15 10 A1 DF " +
            "FB 64 FF 91 3B 56 F7 A8 BC C3 68 4A FB 56 15 57 " +
            "4C 3A 59 04 D2 E0 39 34 28 8A E4 EF 03 22 30 1A " +
            "E0 3C AF 3A 24 86 FE C6 8E 91 B0 39 A5 3D 0C 96 " +
            "3D E0 43 6A E7 65 09 A9 49 96 28 11 A2 9A 33 0D " +
            "0E 71 0B 11 DF 8B ED 99 40 C0 A1 21 C2 43 83 0B " +
            "34 B6 1F 9D E3 24 36 30 8C 72 F8 FC 70 36 BA 57 " +
            "08 AF 39 FA 5D 51 F0 47 10 A9 C8 EF 65 C3 C8 87 " +
            "5C FB CA 1A EC 91 6D 88 FB 7D D5 30 40 F3 B7 91 " +
            "4F 23 40 B4 1B 82 EB FD FC 39 6E FB 72 FF 4C 41 " +
            "10 77 10 F8 82 96 77 F7",
            // After Chi
            "15 11 7C 6D 3B 0E B9 4D 28 74 21 35 B4 44 E4 D8 " +
            "9E 68 D2 BE D3 31 1E F6 D5 4B 56 82 52 98 F0 93 " +
            "EF 29 43 73 40 99 CD 4D 3A 79 B6 4E D5 10 A1 88 " +
            "BB 5C EE 95 3B F6 DF 88 9C 43 CC A1 FA 54 15 5D " +
            "5A 4A 4B 04 C6 F0 B8 F1 E9 8E AD 7E 29 64 66 3A " +
            "D1 5C EC 78 66 C6 FF EF CE 87 98 28 A5 A7 3E 92 " +
            "3B 81 40 6A BA 64 C5 39 A9 9A 8C 3B 82 9E 21 4B " +
            "00 F0 1B 10 5E B2 ED 89 C8 80 41 41 D2 51 0B 4C " +
            "34 3B 1E 9F EE 65 76 30 9C 72 38 F9 50 B4 B2 D7 " +
            "48 EF 18 FA DF 51 F3 4F 24 9F D6 73 44 E7 FC B7 " +
            "58 F9 CA 9E F7 91 25 E4 4B 65 FB 7B 20 8E B3 91 " +
            "4F 65 50 B4 9B 82 D8 4B B0 B1 A4 F9 1E FE 44 49 " +
            "B3 73 05 D8 82 F4 E5 E6",
            // After Iota 
            "1F 11 7C ED 3B 0E B9 CD 28 74 21 35 B4 44 E4 D8 " +
            "9E 68 D2 BE D3 31 1E F6 D5 4B 56 82 52 98 F0 93 " +
            "EF 29 43 73 40 99 CD 4D 3A 79 B6 4E D5 10 A1 88 " +
            "BB 5C EE 95 3B F6 DF 88 9C 43 CC A1 FA 54 15 5D " +
            "5A 4A 4B 04 C6 F0 B8 F1 E9 8E AD 7E 29 64 66 3A " +
            "D1 5C EC 78 66 C6 FF EF CE 87 98 28 A5 A7 3E 92 " +
            "3B 81 40 6A BA 64 C5 39 A9 9A 8C 3B 82 9E 21 4B " +
            "00 F0 1B 10 5E B2 ED 89 C8 80 41 41 D2 51 0B 4C " +
            "34 3B 1E 9F EE 65 76 30 9C 72 38 F9 50 B4 B2 D7 " +
            "48 EF 18 FA DF 51 F3 4F 24 9F D6 73 44 E7 FC B7 " +
            "58 F9 CA 9E F7 91 25 E4 4B 65 FB 7B 20 8E B3 91 " +
            "4F 65 50 B4 9B 82 D8 4B B0 B1 A4 F9 1E FE 44 49 " +
            "B3 73 05 D8 82 F4 E5 E6",
            // Round #20
            // After Theta
            "CA 48 3F 82 02 AF 67 A5 98 42 E1 41 A9 32 65 C7 " +
            "00 12 3B AE 98 7C 62 CA 1C 81 2D D6 E9 16 EA C3 " +
            "F9 76 34 C4 CD F1 81 67 EF 20 F5 21 EC B1 7F E0 " +
            "0B 6A 2E E1 26 80 5E 97 02 39 25 B1 B1 19 69 61 " +
            "93 80 30 50 7D 7E A2 A1 FF D1 DA C9 A4 0C 2A 10 " +
            "04 05 AF 17 5F 67 21 87 7E B1 58 5C B8 D1 BF 8D " +
            "A5 FB A9 7A F1 29 B9 05 60 50 F7 6F 39 10 3B 1B " +
            "16 AF 6C A7 D3 DA A1 A3 1D D9 02 2E EB F0 D5 24 " +
            "84 0D DE EB F3 13 F7 2F 02 08 D1 E9 1B F9 CE EB " +
            "81 25 63 AE 64 DF E9 1F 32 C0 A1 C4 C9 8F B0 9D " +
            "8D A0 89 F1 CE 30 FB 8C FB 53 3B 0F 3D F8 32 8E " +
            "D1 1F B9 A4 D0 CF A4 77 79 7B DF AD A5 70 5E 19 " +
            "A5 2C 72 6F 0F 9C A9 CC",
            // After Rho
            "CA 48 3F 82 02 AF 67 A5 31 85 C2 83 52 65 CA 8E " +
            "80 C4 8E 2B 26 9F 98 32 6E A1 3E CC 11 D8 62 9D " +
            "8E 0F 3C CB B7 A3 21 6E C2 1E FB 07 FE 0E 52 1F " +
            "12 6E 02 E8 75 B9 A0 E6 98 40 4E 49 6C 6C 46 5A " +
            "40 18 A8 3E 3F D1 D0 49 A0 02 F1 1F AD 9D 4C CA " +
            "24 28 78 BD F8 3A 0B 39 36 FA C5 62 71 E1 46 FF " +
            "D5 8B 4F C9 2D 28 DD 4F 20 76 36 C0 A0 EE DF 72 " +
            "D3 69 ED D0 51 8B 57 B6 5C D6 E1 AB 49 3A B2 05 " +
            "7B 7D 7E E2 FE 85 B0 C1 E7 75 01 84 E8 F4 8D 7C " +
            "3B FD 23 B0 64 CC 95 EC 9D 32 C0 A1 C4 C9 8F B0 " +
            "EC 33 36 82 26 C6 3B C3 EE 4F ED 3C F4 E0 CB 38 " +
            "FA 23 97 14 FA 99 F4 2E 7B DF AD A5 70 5E 19 79 " +
            "2A 73 29 8B DC DB 03 67",
            // After Pi
            "CA 48 3F 82 02 AF 67 A5 12 6E 02 E8 75 B9 A0 E6 " +
            "D5 8B 4F C9 2D 28 DD 4F 3B FD 23 B0 64 CC 95 EC " +
            "2A 73 29 8B DC DB 03 67 6E A1 3E CC 11 D8 62 9D " +
            "A0 02 F1 1F AD 9D 4C CA 24 28 78 BD F8 3A 0B 39 " +
            "7B 7D 7E E2 FE 85 B0 C1 FA 23 97 14 FA 99 F4 2E " +
            "31 85 C2 83 52 65 CA 8E 98 40 4E 49 6C 6C 46 5A " +
            "20 76 36 C0 A0 EE DF 72 9D 32 C0 A1 C4 C9 8F B0 " +
            "EC 33 36 82 26 C6 3B C3 8E 0F 3C CB B7 A3 21 6E " +
            "C2 1E FB 07 FE 0E 52 1F 36 FA C5 62 71 E1 46 FF " +
            "E7 75 01 84 E8 F4 8D 7C 7B DF AD A5 70 5E 19 79 " +
            "80 C4 8E 2B 26 9F 98 32 40 18 A8 3E 3F D1 D0 49 " +
            "D3 69 ED D0 51 8B 57 B6 5C D6 E1 AB 49 3A B2 05 " +
            "EE 4F ED 3C F4 E0 CB 38",
            // After Chi
            "0F C9 72 83 0A AF 3A AC 38 1A 22 D8 35 7D A0 46 " +
            "D5 89 47 C2 B5 3B DF 4C FB F5 35 B0 66 E8 F1 6C " +
            "3A 55 29 E3 A9 CB 83 25 6A 89 36 6C 41 FA 61 AC " +
            "FB 57 F7 5D AB 18 FC 0A A4 2A F9 A9 F8 22 4F 17 " +
            "7F FD 56 2A FF C5 B2 50 7A 21 56 07 56 9C F8 6C " +
            "11 B3 F2 03 D2 E7 53 AE 05 40 8E 68 28 6D 46 DA " +
            "40 77 00 C2 82 E8 EF 31 8C B6 00 A0 94 E8 4F BC " +
            "64 73 3A CA 0A CE 3F 93 BA EF 38 AB B6 42 25 8E " +
            "03 1B FB 83 76 1A DB 1F 2E 70 69 43 61 EB 56 FE " +
            "63 75 11 CE 6F 55 AD 7A 3B CF 6E A1 38 52 4B 68 " +
            "13 A5 CB EB 66 95 9F 84 4C 8E A8 15 37 E1 70 48 " +
            "71 60 E1 C4 E5 4B 1E 8E 5C 56 E3 A8 4B 25 A2 07 " +
            "AE 57 CD 28 ED A0 8B 71",
            // After Iota 
            "8E 49 72 03 0A AF 3A 2C 38 1A 22 D8 35 7D A0 46 " +
            "D5 89 47 C2 B5 3B DF 4C FB F5 35 B0 66 E8 F1 6C " +
            "3A 55 29 E3 A9 CB 83 25 6A 89 36 6C 41 FA 61 AC " +
            "FB 57 F7 5D AB 18 FC 0A A4 2A F9 A9 F8 22 4F 17 " +
            "7F FD 56 2A FF C5 B2 50 7A 21 56 07 56 9C F8 6C " +
            "11 B3 F2 03 D2 E7 53 AE 05 40 8E 68 28 6D 46 DA " +
            "40 77 00 C2 82 E8 EF 31 8C B6 00 A0 94 E8 4F BC " +
            "64 73 3A CA 0A CE 3F 93 BA EF 38 AB B6 42 25 8E " +
            "03 1B FB 83 76 1A DB 1F 2E 70 69 43 61 EB 56 FE " +
            "63 75 11 CE 6F 55 AD 7A 3B CF 6E A1 38 52 4B 68 " +
            "13 A5 CB EB 66 95 9F 84 4C 8E A8 15 37 E1 70 48 " +
            "71 60 E1 C4 E5 4B 1E 8E 5C 56 E3 A8 4B 25 A2 07 " +
            "AE 57 CD 28 ED A0 8B 71",
            // Round #21
            // After Theta
            "2C E7 85 52 C4 23 DD 6C B8 AB 0A A8 EA BA 7C 56 " +
            "33 2B 6C 00 10 A2 69 77 F6 0E CE D1 6C 6F CE F1 " +
            "B5 BA 32 E7 12 B4 E4 91 C8 27 C1 3D 8F 76 86 EC " +
            "7B E6 DF 2D 74 DF 20 1A 42 88 D2 6B 5D BB F9 2C " +
            "72 06 AD 4B F5 42 8D CD F5 CE 4D 03 ED E3 9F D8 " +
            "B3 1D 05 52 1C 6B B4 EE 85 F1 A6 18 F7 AA 9A CA " +
            "A6 D5 2B 00 27 71 59 0A 81 4D FB C1 9E 6F 70 21 " +
            "EB 9C 21 CE B1 B1 58 27 18 41 CF FA 78 CE C2 CE " +
            "83 AA D3 F3 A9 DD 07 0F C8 D2 42 81 C4 72 E0 C5 " +
            "6E 8E EA AF 65 D2 92 E7 B4 20 75 A5 83 2D 2C DC " +
            "B1 0B 3C BA A8 19 78 C4 CC 3F 80 65 E8 26 AC 58 " +
            "97 C2 CA 06 40 D2 A8 B5 51 AD 18 C9 41 A2 9D 9A " +
            "21 B8 D6 2C 56 DF EC C5",
            // After Rho
            "2C E7 85 52 C4 23 DD 6C 70 57 15 50 D5 75 F9 AC " +
            "CC 0A 1B 00 84 68 DA DD F6 E6 1C 6F EF E0 1C CD " +
            "A0 25 8F AC D5 95 39 97 F3 68 67 C8 8E 7C 12 DC " +
            "DD 42 F7 0D A2 B1 67 FE 8B 10 A2 F4 5A D7 6E 3E " +
            "83 D6 A5 7A A1 C6 66 39 FE 89 5D EF DC 34 D0 3E " +
            "9F ED 28 90 E2 58 A3 75 2A 17 C6 9B 62 DC AB 6A " +
            "01 38 89 CB 52 30 AD 5E DF E0 42 02 9B F6 83 3D " +
            "E7 D8 58 AC 93 75 CE 10 F5 F1 9C 85 9D 31 82 9E " +
            "7A 3E B5 FB E0 61 50 75 F0 62 64 69 A1 40 62 39 " +
            "5A F2 DC CD 51 FD B5 4C DC B4 20 75 A5 83 2D 2C " +
            "E0 11 C7 2E F0 E8 A2 66 31 FF 00 96 A1 9B B0 62 " +
            "52 58 D9 00 48 1A B5 F6 AD 18 C9 41 A2 9D 9A 51 " +
            "7B 71 08 AE 35 8B D5 37",
            // After Pi
            "2C E7 85 52 C4 23 DD 6C DD 42 F7 0D A2 B1 67 FE " +
            "01 38 89 CB 52 30 AD 5E 5A F2 DC CD 51 FD B5 4C " +
            "7B 71 08 AE 35 8B D5 37 F6 E6 1C 6F EF E0 1C CD " +
            "FE 89 5D EF DC 34 D0 3E 9F ED 28 90 E2 58 A3 75 " +
            "7A 3E B5 FB E0 61 50 75 52 58 D9 00 48 1A B5 F6 " +
            "70 57 15 50 D5 75 F9 AC 8B 10 A2 F4 5A D7 6E 3E " +
            "DF E0 42 02 9B F6 83 3D DC B4 20 75 A5 83 2D 2C " +
            "E0 11 C7 2E F0 E8 A2 66 A0 25 8F AC D5 95 39 97 " +
            "F3 68 67 C8 8E 7C 12 DC 2A 17 C6 9B 62 DC AB 6A " +
            "F0 62 64 69 A1 40 62 39 AD 18 C9 41 A2 9D 9A 51 " +
            "CC 0A 1B 00 84 68 DA DD 83 D6 A5 7A A1 C6 66 39 " +
            "E7 D8 58 AC 93 75 CE 10 F5 F1 9C 85 9D 31 82 9E " +
            "31 FF 00 96 A1 9B B0 62",
            // After Chi
            "2C DF 8D 90 94 23 55 6C 87 80 A3 09 A3 7C 77 FE " +
            "20 39 89 E9 76 32 ED 6D 5E 74 59 9D 91 DD BD 04 " +
            "AA 71 7A A3 17 1B F7 A5 F7 82 3C 7F CD A8 3F 8C " +
            "9E 9B C8 84 DC 15 80 3E 9F AD 60 90 EA 42 06 F7 " +
            "DE 98 B1 94 47 81 58 7C 5A 51 98 80 58 0E 75 C4 " +
            "24 B7 55 52 54 55 78 AD 8B 04 82 81 7E D6 42 3E " +
            "FF E1 85 08 CB 9E 01 7F CC F2 30 25 A0 96 74 A4 " +
            "6B 11 65 8A FA 6A A4 74 A8 32 0F BF B5 15 90 B5 " +
            "23 08 47 A8 0F 7C 52 CD 27 0F 4F 9B 60 41 33 2A " +
            "F0 47 62 C5 F4 40 43 BF FE 50 A9 01 A8 F5 98 19 " +
            "A8 02 43 84 96 59 52 DD 93 F7 21 7B AD C6 66 B7 " +
            "E7 D6 58 BE B3 FF FE 70 39 F1 87 85 99 51 C8 03 " +
            "32 2B A4 EC 80 1D 94 42",
            // After Iota 
            "AC 5F 8D 90 94 23 55 EC 87 80 A3 09 A3 7C 77 FE " +
            "20 39 89 E9 76 32 ED 6D 5E 74 59 9D 91 DD BD 04 " +
            "AA 71 7A A3 17 1B F7 A5 F7 82 3C 7F CD A8 3F 8C " +
            "9E 9B C8 84 DC 15 80 3E 9F AD 60 90 EA 42 06 F7 " +
            "DE 98 B1 94 47 81 58 7C 5A 51 98 80 58 0E 75 C4 " +
            "24 B7 55 52 54 55 78 AD 8B 04 82 81 7E D6 42 3E " +
            "FF E1 85 08 CB 9E 01 7F CC F2 30 25 A0 96 74 A4 " +
            "6B 11 65 8A FA 6A A4 74 A8 32 0F BF B5 15 90 B5 " +
            "23 08 47 A8 0F 7C 52 CD 27 0F 4F 9B 60 41 33 2A " +
            "F0 47 62 C5 F4 40 43 BF FE 50 A9 01 A8 F5 98 19 " +
            "A8 02 43 84 96 59 52 DD 93 F7 21 7B AD C6 66 B7 " +
            "E7 D6 58 BE B3 FF FE 70 39 F1 87 85 99 51 C8 03 " +
            "32 2B A4 EC 80 1D 94 42",
            // Round #22
            // After Theta
            "BE D5 18 6B 4E BF 7D AB F9 83 FC 27 85 4F E9 25 " +
            "08 88 7D EE E3 81 59 29 70 4C 36 40 2F A2 CF 27 " +
            "D0 6D 17 C2 51 E4 4C 8E E5 08 A9 84 17 34 17 CB " +
            "E0 98 97 AA FA 26 1E E5 B7 1C 94 97 7F F1 B2 B3 " +
            "F0 A0 DE 49 F9 FE 2A 5F 20 4D F5 E1 1E F1 CE EF " +
            "36 3D C0 A9 8E C9 50 EA F5 07 DD AF 58 E5 DC E5 " +
            "D7 50 71 0F 5E 2D B5 3B E2 CA 5F F8 1E E9 06 87 " +
            "11 0D 08 EB BC 95 1F 5F BA B8 9A 44 6F 89 B8 F2 " +
            "5D 0B 18 86 29 4F CC 16 0F BE BB 9C F5 F2 87 6E " +
            "DE 7F 0D 18 4A 3F 31 9C 84 4C C4 60 EE 0A 23 32 " +
            "BA 88 D6 7F 4C C5 7A 9A ED F4 7E 55 8B F5 F8 6C " +
            "CF 67 AC B9 26 4C 4A 34 17 C9 E8 58 27 2E BA 20 " +
            "48 37 C9 8D C6 E2 2F 69",
            // After Rho
            "BE D5 18 6B 4E BF 7D AB F2 07 F9 4F 0A 9F D2 4B " +
            "02 62 9F FB 78 60 56 0A 22 FA 7C 02 C7 64 03 F4 " +
            "22 67 72 84 6E BB 10 8E 78 41 73 B1 5C 8E 90 4A " +
            "A9 AA 6F E2 51 0E 8E 79 EC 2D 07 E5 E5 5F BC EC " +
            "50 EF A4 7C 7F 95 2F 78 EF FC 0E D2 54 1F EE 11 " +
            "B7 E9 01 4E 75 4C 86 52 97 D7 1F 74 BF 62 95 73 " +
            "7B F0 6A A9 DD B9 86 8A D2 0D 0E C5 95 BF F0 3D " +
            "75 DE CA 8F AF 88 06 84 89 DE 12 71 E5 75 71 35 " +
            "C3 30 E5 89 D9 A2 6B 01 43 B7 07 DF 5D CE 7A F9 " +
            "27 86 D3 FB AF 01 43 E9 32 84 4C C4 60 EE 0A 23 " +
            "EB 69 EA 22 5A FF 31 15 B5 D3 FB 55 2D D6 E3 B3 " +
            "F9 8C 35 D7 84 49 89 E6 C9 E8 58 27 2E BA 20 17 " +
            "4B 1A D2 4D 72 A3 B1 F8",
            // After Pi
            "BE D5 18 6B 4E BF 7D AB A9 AA 6F E2 51 0E 8E 79 " +
            "7B F0 6A A9 DD B9 86 8A 27 86 D3 FB AF 01 43 E9 " +
            "4B 1A D2 4D 72 A3 B1 F8 22 FA 7C 02 C7 64 03 F4 " +
            "EF FC 0E D2 54 1F EE 11 B7 E9 01 4E 75 4C 86 52 " +
            "C3 30 E5 89 D9 A2 6B 01 F9 8C 35 D7 84 49 89 E6 " +
            "F2 07 F9 4F 0A 9F D2 4B EC 2D 07 E5 E5 5F BC EC " +
            "D2 0D 0E C5 95 BF F0 3D 32 84 4C C4 60 EE 0A 23 " +
            "EB 69 EA 22 5A FF 31 15 22 67 72 84 6E BB 10 8E " +
            "78 41 73 B1 5C 8E 90 4A 97 D7 1F 74 BF 62 95 73 " +
            "43 B7 07 DF 5D CE 7A F9 C9 E8 58 27 2E BA 20 17 " +
            "02 62 9F FB 78 60 56 0A 50 EF A4 7C 7F 95 2F 78 " +
            "75 DE CA 8F AF 88 06 84 89 DE 12 71 E5 75 71 35 " +
            "B5 D3 FB 55 2D D6 E3 B3",
            // After Chi
            "EC 85 18 62 C2 0E 7D 29 AD AC FE B0 73 0E CF 18 " +
            "33 E8 6A AD 8D 1B 36 9A 93 43 DB D9 A3 1D 0F EA " +
            "4A 30 B5 CD 63 A3 33 A8 32 FB 7D 0E E6 24 03 B6 " +
            "AF EC EA 53 DC BD 87 10 8F 65 11 18 71 05 06 B4 " +
            "C1 42 AD 89 9A 86 69 11 34 88 37 07 94 52 65 E7 " +
            "E0 07 F1 4F 1A 3F 92 5A CC AD 47 E5 85 1F B6 EE " +
            "1B 64 AC E7 8F AE C1 29 22 82 5D 89 60 EE C8 69 " +
            "E7 41 EC 82 BF BF 1D B1 A5 F1 7E C0 CD DB 15 BF " +
            "38 61 73 3A 1C 02 FA C2 1F 9F 47 54 9D 52 95 75 " +
            "61 B0 25 5F 1D CF 6A 71 91 E8 59 16 3E BE A0 57 " +
            "27 72 D5 78 F8 68 56 8E D8 EF B4 0C 3F E0 5E 49 " +
            "41 DF 23 8B A7 0A 84 06 8B FE 16 DB B5 55 65 3D " +
            "E5 5E DB 51 2A 43 CA C3",
            // After Iota 
            "ED 85 18 E2 C2 0E 7D 29 AD AC FE B0 73 0E CF 18 " +
            "33 E8 6A AD 8D 1B 36 9A 93 43 DB D9 A3 1D 0F EA " +
            "4A 30 B5 CD 63 A3 33 A8 32 FB 7D 0E E6 24 03 B6 " +
            "AF EC EA 53 DC BD 87 10 8F 65 11 18 71 05 06 B4 " +
            "C1 42 AD 89 9A 86 69 11 34 88 37 07 94 52 65 E7 " +
            "E0 07 F1 4F 1A 3F 92 5A CC AD 47 E5 85 1F B6 EE " +
            "1B 64 AC E7 8F AE C1 29 22 82 5D 89 60 EE C8 69 " +
            "E7 41 EC 82 BF BF 1D B1 A5 F1 7E C0 CD DB 15 BF " +
            "38 61 73 3A 1C 02 FA C2 1F 9F 47 54 9D 52 95 75 " +
            "61 B0 25 5F 1D CF 6A 71 91 E8 59 16 3E BE A0 57 " +
            "27 72 D5 78 F8 68 56 8E D8 EF B4 0C 3F E0 5E 49 " +
            "41 DF 23 8B A7 0A 84 06 8B FE 16 DB B5 55 65 3D " +
            "E5 5E DB 51 2A 43 CA C3",
            // Round #23
            // After Theta
            "5C 0C DC 8C 8C 21 E8 99 E2 05 A6 B0 EB 78 A1 05 " +
            "28 10 CF 27 66 8A 2F 4A B0 75 B0 4B 52 93 AC 4A " +
            "AB 08 D2 A6 84 00 CD 9F 83 72 B9 60 A8 0B 96 06 " +
            "E0 45 B2 53 44 CB E9 0D 94 9D B4 92 9A 94 1F 64 " +
            "E2 74 C6 1B 6B 08 CA B1 D5 B0 50 6C 73 F1 9B D0 " +
            "51 8E 35 21 54 10 07 EA 83 04 1F E5 1D 69 D8 F3 " +
            "00 9C 09 6D 64 3F D8 F9 01 B4 36 1B 91 60 6B C9 " +
            "06 79 8B E9 58 1C E3 86 14 78 BA AE 83 F4 80 0F " +
            "77 C8 2B 3A 84 74 94 DF 04 67 E2 DE 76 C3 8C A5 " +
            "42 86 4E CD EC 41 C9 D1 70 D0 3E 7D D9 1D 5E 60 " +
            "96 FB 11 16 B6 47 C3 3E 97 46 EC 0C A7 96 30 54 " +
            "5A 27 86 01 4C 9B 9D D6 A8 C8 7D 49 44 DB C6 9D " +
            "04 66 BC 3A CD E0 34 F4",
            // After Rho
            "5C 0C DC 8C 8C 21 E8 99 C4 0B 4C 61 D7 F1 42 0B " +
            "0A C4 F3 89 99 E2 8B 12 35 C9 AA 04 5B 07 BB 24 " +
            "04 68 FE 5C 45 90 36 25 86 BA 60 69 30 28 97 0B " +
            "3B 45 B4 9C DE 00 5E 24 19 65 27 AD A4 26 E5 07 " +
            "3A E3 8D 35 04 E5 58 71 BF 09 5D 0D 0B C5 36 17 " +
            "8F 72 AC 09 A1 82 38 50 CF 0F 12 7C 94 77 A4 61 " +
            "68 23 FB C1 CE 07 E0 4C C1 D6 92 03 68 6D 36 22 " +
            "74 2C 8E 71 43 83 BC C5 5D 07 E9 01 1F 28 F0 74 " +
            "45 87 90 8E F2 FB 0E 79 C6 52 82 33 71 6F BB 61 " +
            "28 39 5A C8 D0 A9 99 3D 60 70 D0 3E 7D D9 1D 5E " +
            "0D FB 58 EE 47 58 D8 1E 5D 1A B1 33 9C 5A C2 50 " +
            "EB C4 30 80 69 B3 D3 5A C8 7D 49 44 DB C6 9D A8 " +
            "0D 3D 81 19 AF 4E 33 38",
            // After Pi
            "5C 0C DC 8C 8C 21 E8 99 3B 45 B4 9C DE 00 5E 24 " +
            "68 23 FB C1 CE 07 E0 4C 28 39 5A C8 D0 A9 99 3D " +
            "0D 3D 81 19 AF 4E 33 38 35 C9 AA 04 5B 07 BB 24 " +
            "BF 09 5D 0D 0B C5 36 17 8F 72 AC 09 A1 82 38 50 " +
            "45 87 90 8E F2 FB 0E 79 EB C4 30 80 69 B3 D3 5A " +
            "C4 0B 4C 61 D7 F1 42 0B 19 65 27 AD A4 26 E5 07 " +
            "C1 D6 92 03 68 6D 36 22 60 70 D0 3E 7D D9 1D 5E " +
            "0D FB 58 EE 47 58 D8 1E 04 68 FE 5C 45 90 36 25 " +
            "86 BA 60 69 30 28 97 0B CF 0F 12 7C 94 77 A4 61 " +
            "C6 52 82 33 71 6F BB 61 C8 7D 49 44 DB C6 9D A8 " +
            "0A C4 F3 89 99 E2 8B 12 3A E3 8D 35 04 E5 58 71 " +
            "74 2C 8E 71 43 83 BC C5 5D 07 E9 01 1F 28 F0 74 " +
            "5D 1A B1 33 9C 5A C2 50",
            // After Chi
            "1C 2E 97 CD 8C 26 48 D1 3B 5D B4 94 CE A8 47 15 " +
            "6D 27 7A D0 E1 41 C2 4C 78 39 06 4C D0 88 51 BC " +
            "2E 7C A1 09 FD 4E 25 1C 35 BB 0A 04 FB 05 B3 64 " +
            "FF 8C 4D 8B 59 BC 30 3E 25 32 8C 09 A8 82 E9 52 " +
            "51 8E 1A 8A E0 FF 26 5D 61 C4 65 89 69 73 D7 49 " +
            "04 99 DC 63 9F B8 50 2B 39 45 67 91 B1 B6 EC 5B " +
            "CC 5D 9A C3 6A 6D F6 22 A0 70 D4 3F ED 78 1F 5F " +
            "14 9F 7B 62 67 5E 7D 1A 4D 6D EC 48 C1 C7 16 45 " +
            "86 EA E0 6A 51 20 8C 0B C7 22 5B 38 1E F7 A0 E9 " +
            "C2 52 34 2B 75 7F 99 64 4A EF 49 65 EB EE 1C A2 " +
            "4E C8 F1 C9 DA E0 2F 96 33 E0 EC 35 18 CD 18 41 " +
            "74 34 9E 43 C3 D1 BE C5 5F C3 AB 89 1E 88 F9 76 " +
            "6D 39 BD 07 98 5F 92 31",
            // After Iota 
            "14 AE 97 4D 8C 26 48 51 3B 5D B4 94 CE A8 47 15 " +
            "6D 27 7A D0 E1 41 C2 4C 78 39 06 4C D0 88 51 BC " +
            "2E 7C A1 09 FD 4E 25 1C 35 BB 0A 04 FB 05 B3 64 " +
            "FF 8C 4D 8B 59 BC 30 3E 25 32 8C 09 A8 82 E9 52 " +
            "51 8E 1A 8A E0 FF 26 5D 61 C4 65 89 69 73 D7 49 " +
            "04 99 DC 63 9F B8 50 2B 39 45 67 91 B1 B6 EC 5B " +
            "CC 5D 9A C3 6A 6D F6 22 A0 70 D4 3F ED 78 1F 5F " +
            "14 9F 7B 62 67 5E 7D 1A 4D 6D EC 48 C1 C7 16 45 " +
            "86 EA E0 6A 51 20 8C 0B C7 22 5B 38 1E F7 A0 E9 " +
            "C2 52 34 2B 75 7F 99 64 4A EF 49 65 EB EE 1C A2 " +
            "4E C8 F1 C9 DA E0 2F 96 33 E0 EC 35 18 CD 18 41 " +
            "74 34 9E 43 C3 D1 BE C5 5F C3 AB 89 1E 88 F9 76 " +
            "6D 39 BD 07 98 5F 92 31",
            // Xor'd state (in bytes)                        " +
            "14 AE 97 4D 8C 26 48 51 3B 5D B4 94 CE A8 47 15 " +
            "6D 27 7A D0 E1 41 C2 4C 78 39 06 4C D0 88 51 BC " +
            "2E 7C A1 09 FD 4E 25 1C 35 BB 0A 04 FB 05 B3 64 " +
            "FF 8C 4D 8B 59 BC 30 3E 25 32 8C 09 A8 82 E9 52 " +
            "51 8E 1A 8A E0 FF 26 5D 61 C4 65 89 69 73 D7 49 " +
            "04 99 DC 63 9F B8 50 2B 39 45 67 91 B1 B6 EC 5B " +
            "CC 5D 9A C3 6A 6D F6 22 A0 70 D4 3F ED 78 1F 5F " +
            "14 9F 7B 62 67 5E 7D 1A 4D 6D EC 48 C1 C7 16 45 " +
            "86 EA E0 6A 51 20 8C 0B C7 22 5B 38 1E F7 A0 E9 " +
            "C2 52 34 2B 75 7F 99 64 4A EF 49 65 EB EE 1C A2 " +
            "4E C8 F1 C9 DA E0 2F 96 33 E0 EC 35 18 CD 18 41 " +
            "74 34 9E 43 C3 D1 BE C5 5F C3 AB 89 1E 88 F9 76 " +
            "6D 39 BD 07 98 5F 92 31",
            // Round #0
            // After Theta
            "F8 63 F9 6E D3 6A 57 F9 73 C8 BA FC C1 05 52 F9 " +
            "0C 15 46 B7 E2 FF DC 2E B6 85 38 2D 2F A5 91 14 " +
            "77 78 4E 04 AC CF 08 2B D9 76 64 27 A4 49 AC CC " +
            "B7 19 43 E3 56 11 25 D2 44 00 B0 6E AB 3C F7 30 " +
            "9F 32 24 EB 1F D2 E6 F5 38 C0 8A 84 38 F2 FA 7E " +
            "E8 54 B2 40 C0 F4 4F 83 71 D0 69 F9 BE 1B F9 B7 " +
            "AD 6F A6 A4 69 D3 E8 40 6E CC EA 5E 12 55 DF F7 " +
            "4D 9B 94 6F 36 DF 50 2D A1 A0 82 6B 9E 8B 09 ED " +
            "CE 7F EE 02 5E 8D 99 E7 A6 10 67 5F 1D 49 BE 8B " +
            "0C EE 0A 4A 8A 52 59 CC 13 EB A6 68 BA 6F 31 95 " +
            "A2 05 9F EA 85 AC 30 3E 7B 75 E2 5D 17 60 0D AD " +
            "15 06 A2 24 C0 6F A0 A7 91 7F 95 E8 E1 A5 39 DE " +
            "34 3D 52 0A C9 DE BF 06",
            // After Rho
            "F8 63 F9 6E D3 6A 57 F9 E7 90 75 F9 83 0B A4 F2 " +
            "43 85 D1 AD F8 3F B7 0B 52 1A 49 61 5B 88 D3 F2 " +
            "7D 46 58 B9 C3 73 22 60 42 9A C4 CA 9C 6D 47 76 " +
            "34 6E 15 51 22 7D 9B 31 0C 11 00 AC DB 2A CF 3D " +
            "19 92 F5 0F 69 F3 FA 4F AF EF 87 03 AC 48 88 23 " +
            "44 A7 92 05 02 A6 7F 1A DF C6 41 A7 E5 FB 6E E4 " +
            "25 4D 9B 46 07 6A 7D 33 AA BE EF DD 98 D5 BD 24 " +
            "37 9B 6F A8 96 A6 4D CA D7 3C 17 13 DA 43 41 05 " +
            "5D C0 AB 31 F3 DC F9 CF DF 45 53 88 B3 AF 8E 24 " +
            "2A 8B 99 C1 5D 41 49 51 95 13 EB A6 68 BA 6F 31 " +
            "C2 F8 88 16 7C AA 17 B2 EE D5 89 77 5D 80 35 B4 " +
            "C2 40 94 04 F8 0D F4 B4 7F 95 E8 E1 A5 39 DE 91 " +
            "AF 01 4D 8F 94 42 B2 F7",
            // After Pi
            "F8 63 F9 6E D3 6A 57 F9 34 6E 15 51 22 7D 9B 31 " +
            "25 4D 9B 46 07 6A 7D 33 2A 8B 99 C1 5D 41 49 51 " +
            "AF 01 4D 8F 94 42 B2 F7 52 1A 49 61 5B 88 D3 F2 " +
            "AF EF 87 03 AC 48 88 23 44 A7 92 05 02 A6 7F 1A " +
            "5D C0 AB 31 F3 DC F9 CF C2 40 94 04 F8 0D F4 B4 " +
            "E7 90 75 F9 83 0B A4 F2 0C 11 00 AC DB 2A CF 3D " +
            "AA BE EF DD 98 D5 BD 24 95 13 EB A6 68 BA 6F 31 " +
            "C2 F8 88 16 7C AA 17 B2 7D 46 58 B9 C3 73 22 60 " +
            "42 9A C4 CA 9C 6D 47 76 DF C6 41 A7 E5 FB 6E E4 " +
            "DF 45 53 88 B3 AF 8E 24 7F 95 E8 E1 A5 39 DE 91 " +
            "43 85 D1 AD F8 3F B7 0B 19 92 F5 0F 69 F3 FA 4F " +
            "37 9B 6F A8 96 A6 4D CA D7 3C 17 13 DA 43 41 05 " +
            "EE D5 89 77 5D 80 35 B4",
            // After Chi
            "F9 62 73 68 D6 68 33 FB 3E EC 15 D0 7A 7C 9B 71 " +
            "A0 4D DF 48 87 68 CF 95 7A E9 29 A1 1E 69 0C 59 " +
            "AB 0D 49 9E B4 57 3A F7 12 1A 59 65 59 2E A4 EA " +
            "B6 AF AE 33 5D 10 08 E6 C6 A7 86 01 0A A7 7B 2A " +
            "4D DA E2 50 F0 5C FA 8D 6F A5 12 06 5C 4D FC B5 " +
            "45 3E 9A A8 83 DE 94 F2 19 10 00 8E BB 00 8D 2C " +
            "E8 56 EF CD 8C D5 AD A6 B0 13 9E 4F EB BB CF 71 " +
            "CA F9 88 12 24 8A 5C BF E0 02 59 9C A2 E1 0A E0 " +
            "42 9B D6 C2 8E 69 C7 76 FF 56 E9 C6 E1 EB 3E 75 " +
            "DF 07 43 90 F1 ED AE 44 7D 0D 6C A3 B9 35 9B 87 " +
            "65 8C DB 0D 6E 3B B2 8B D9 B6 E5 1C 21 B2 FA 4A " +
            "1F 5A E7 CC 93 26 79 7A D6 3C 47 9B 7A 7C C3 0E " +
            "F6 C7 AD 75 5C 40 7D F0",
            // After Iota 
            "F8 62 73 68 D6 68 33 FB 3E EC 15 D0 7A 7C 9B 71 " +
            "A0 4D DF 48 87 68 CF 95 7A E9 29 A1 1E 69 0C 59 " +
            "AB 0D 49 9E B4 57 3A F7 12 1A 59 65 59 2E A4 EA " +
            "B6 AF AE 33 5D 10 08 E6 C6 A7 86 01 0A A7 7B 2A " +
            "4D DA E2 50 F0 5C FA 8D 6F A5 12 06 5C 4D FC B5 " +
            "45 3E 9A A8 83 DE 94 F2 19 10 00 8E BB 00 8D 2C " +
            "E8 56 EF CD 8C D5 AD A6 B0 13 9E 4F EB BB CF 71 " +
            "CA F9 88 12 24 8A 5C BF E0 02 59 9C A2 E1 0A E0 " +
            "42 9B D6 C2 8E 69 C7 76 FF 56 E9 C6 E1 EB 3E 75 " +
            "DF 07 43 90 F1 ED AE 44 7D 0D 6C A3 B9 35 9B 87 " +
            "65 8C DB 0D 6E 3B B2 8B D9 B6 E5 1C 21 B2 FA 4A " +
            "1F 5A E7 CC 93 26 79 7A D6 3C 47 9B 7A 7C C3 0E " +
            "F6 C7 AD 75 5C 40 7D F0",
            // Round #1
            // After Theta
            "68 05 71 53 98 E3 08 7F C8 44 56 F9 5D 90 9D D5 " +
            "B7 04 F5 91 A9 E0 44 CC 1F 6E B4 97 3F 74 AB 5B " +
            "70 86 7D 43 BA CD 18 09 82 7D 5B 5E 17 A5 9F 6E " +
            "40 07 ED 1A 7A FC 0E 42 D1 EE AC D8 24 2F F0 73 " +
            "28 5D 7F 66 D1 41 5D 8F B4 2E 26 DB 52 D7 DE 4B " +
            "D5 59 98 93 CD 55 AF 76 EF B8 43 A7 9C EC 8B 88 " +
            "FF 1F C5 14 A2 5D 26 FF D5 94 03 79 CA A6 68 73 " +
            "11 72 BC CF 2A 10 7E 41 70 65 5B A7 EC 6A 31 64 " +
            "B4 33 95 EB A9 85 C1 D2 E8 1F C3 1F CF 63 B5 2C " +
            "BA 80 DE A6 D0 F0 09 46 A6 86 58 7E B7 AF B9 79 " +
            "F5 EB D9 36 20 B0 89 0F 2F 1E A6 35 06 5E FC EE " +
            "08 13 CD 15 BD AE F2 23 B3 BB DA AD 5B 61 64 0C " +
            "2D 4C 99 A8 52 DA 5F 0E",
            // After Rho
            "68 05 71 53 98 E3 08 7F 91 89 AC F2 BB 20 3B AB " +
            "2D 41 7D 64 2A 38 11 F3 43 B7 BA F5 E1 46 7B F9 " +
            "6D C6 48 80 33 EC 1B D2 75 51 FA E9 26 D8 B7 E5 " +
            "AE A1 C7 EF 20 04 74 D0 5C B4 3B 2B 36 C9 0B FC " +
            "AE 3F B3 E8 A0 AE 47 94 ED BD 44 EB 62 B2 2D 75 " +
            "AB CE C2 9C 6C AE 7A B5 22 BE E3 0E 9D 72 B2 2F " +
            "A6 10 ED 32 F9 FF FF 28 4D D1 E6 AA 29 07 F2 94 " +
            "67 15 08 BF A0 08 39 DE 4E D9 D5 62 C8 E0 CA B6 " +
            "72 3D B5 30 58 9A 76 A6 5A 16 F4 8F E1 8F E7 B1 " +
            "3E C1 48 17 D0 DB 14 1A 79 A6 86 58 7E B7 AF B9 " +
            "26 3E D4 AF 67 DB 80 C0 BF 78 98 D6 18 78 F1 BB " +
            "61 A2 B9 A2 D7 55 7E 04 BB DA AD 5B 61 64 0C B3 " +
            "97 43 0B 53 26 AA 94 F6",
            // After Pi
            "68 05 71 53 98 E3 08 7F AE A1 C7 EF 20 04 74 D0 " +
            "A6 10 ED 32 F9 FF FF 28 3E C1 48 17 D0 DB 14 1A " +
            "97 43 0B 53 26 AA 94 F6 43 B7 BA F5 E1 46 7B F9 " +
            "ED BD 44 EB 62 B2 2D 75 AB CE C2 9C 6C AE 7A B5 " +
            "72 3D B5 30 58 9A 76 A6 61 A2 B9 A2 D7 55 7E 04 " +
            "91 89 AC F2 BB 20 3B AB 5C B4 3B 2B 36 C9 0B FC " +
            "4D D1 E6 AA 29 07 F2 94 79 A6 86 58 7E B7 AF B9 " +
            "26 3E D4 AF 67 DB 80 C0 6D C6 48 80 33 EC 1B D2 " +
            "75 51 FA E9 26 D8 B7 E5 22 BE E3 0E 9D 72 B2 2F " +
            "5A 16 F4 8F E1 8F E7 B1 BB DA AD 5B 61 64 0C B3 " +
            "2D 41 7D 64 2A 38 11 F3 AE 3F B3 E8 A0 AE 47 94 " +
            "67 15 08 BF A0 08 39 DE 4E D9 D5 62 C8 E0 CA B6 " +
            "BF 78 98 D6 18 78 F1 BB",
            // After Chi
            "68 15 59 43 41 18 83 57 B6 60 C7 EA 20 04 74 C2 " +
            "27 12 EE 72 DF DF 7F CC 56 C5 38 17 48 9A 1C 13 " +
            "11 E3 8D FF 06 AE E0 76 41 F5 38 E1 ED 4A 29 79 " +
            "BD 8C 71 CB 72 A2 29 77 AA 4C CA 1E EB EB 72 B5 " +
            "70 28 B7 65 78 98 77 5F CD AA FD A8 D5 E5 7A 00 " +
            "90 C8 68 72 B2 26 CB AB 6C 92 3B 7B 60 79 06 D5 " +
            "4B C9 B6 0D 28 4F F2 D4 E8 27 AE 08 E6 97 94 92 " +
            "6A 0A C7 A6 63 12 80 94 6F 68 49 86 AA CE 1B D8 " +
            "2D 51 EE 68 46 55 F2 75 83 76 EA 5E 9D 12 BA 2D " +
            "1E 12 B4 0F F3 07 F4 F1 AB CB 1F 32 65 74 A8 96 " +
            "6C 41 75 73 2A 38 29 B9 A6 F7 66 A8 E8 4E 85 B4 " +
            "D6 35 00 2B B0 10 08 D7 4E D8 B0 42 EA E0 CA F6 " +
            "3D 46 1A 5E 98 FE B7 BF",
            // After Iota 
            "EA 95 59 43 41 18 83 57 B6 60 C7 EA 20 04 74 C2 " +
            "27 12 EE 72 DF DF 7F CC 56 C5 38 17 48 9A 1C 13 " +
            "11 E3 8D FF 06 AE E0 76 41 F5 38 E1 ED 4A 29 79 " +
            "BD 8C 71 CB 72 A2 29 77 AA 4C CA 1E EB EB 72 B5 " +
            "70 28 B7 65 78 98 77 5F CD AA FD A8 D5 E5 7A 00 " +
            "90 C8 68 72 B2 26 CB AB 6C 92 3B 7B 60 79 06 D5 " +
            "4B C9 B6 0D 28 4F F2 D4 E8 27 AE 08 E6 97 94 92 " +
            "6A 0A C7 A6 63 12 80 94 6F 68 49 86 AA CE 1B D8 " +
            "2D 51 EE 68 46 55 F2 75 83 76 EA 5E 9D 12 BA 2D " +
            "1E 12 B4 0F F3 07 F4 F1 AB CB 1F 32 65 74 A8 96 " +
            "6C 41 75 73 2A 38 29 B9 A6 F7 66 A8 E8 4E 85 B4 " +
            "D6 35 00 2B B0 10 08 D7 4E D8 B0 42 EA E0 CA F6 " +
            "3D 46 1A 5E 98 FE B7 BF",
            // Round #2
            // After Theta
            "13 EA E0 EA 35 42 DF DE A8 48 03 E7 DC 74 BD 88 " +
            "F6 CB A1 86 DD FE D1 DE 84 8D 25 38 E2 45 5A D2 " +
            "FE E1 C3 82 F5 D9 86 67 B8 8A 81 48 99 10 75 F0 " +
            "A3 A4 B5 C6 8E D2 E0 3D 7B 95 85 EA E9 CA DC A7 " +
            "A2 60 AA 4A D2 47 31 9E 22 A8 B3 D5 26 92 1C 11 " +
            "69 B7 D1 DB C6 7C 97 22 72 BA FF 76 9C 09 CF 9F " +
            "9A 10 F9 F9 2A 6E 5C C6 3A 6F B3 27 4C 48 D2 53 " +
            "85 08 89 DB 90 65 E6 85 96 17 F0 2F DE 94 47 51 " +
            "33 79 2A 65 BA 25 3B 3F 52 AF A5 AA 9F 33 14 3F " +
            "CC 5A A9 20 59 D8 B2 30 44 C9 51 4F 96 03 CE 87 " +
            "95 3E CC DA 5E 62 75 30 B8 DF A2 A5 14 3E 4C FE " +
            "07 EC 4F DF B2 31 A6 C5 9C 90 AD 6D 40 3F 8C 37 " +
            "D2 44 54 23 6B 89 D1 AE",
            // After Rho
            "13 EA E0 EA 35 42 DF DE 51 91 06 CE B9 E9 7A 11 " +
            "FD 72 A8 61 B7 7F B4 B7 5E A4 25 4D D8 58 82 23 " +
            "CF 36 3C F3 0F 1F 16 AC 94 09 51 07 8F AB 18 88 " +
            "6B EC 28 0D DE 33 4A 5A E9 5E 65 A1 7A BA 32 F7 " +
            "30 55 25 E9 A3 18 4F 51 C9 11 21 82 3A 5B 6D 22 " +
            "49 BB 8D DE 36 E6 BB 14 7F CA E9 FE DB 71 26 3C " +
            "CF 57 71 E3 32 D6 84 C8 90 A4 A7 74 DE 66 4F 98 " +
            "6D C8 32 F3 C2 42 84 C4 5F BC 29 8F A2 2C 2F E0 " +
            "A5 4C B7 64 E7 67 26 4F 8A 1F A9 D7 52 D5 CF 19 " +
            "5B 16 86 59 2B 15 24 0B 87 44 C9 51 4F 96 03 CE " +
            "D5 C1 54 FA 30 6B 7B 89 E3 7E 8B 96 52 F8 30 F9 " +
            "80 FD E9 5B 36 C6 B4 F8 90 AD 6D 40 3F 8C 37 9C " +
            "B4 AB 34 11 D5 C8 5A 62",
            // After Pi
            "13 EA E0 EA 35 42 DF DE 6B EC 28 0D DE 33 4A 5A " +
            "CF 57 71 E3 32 D6 84 C8 5B 16 86 59 2B 15 24 0B " +
            "B4 AB 34 11 D5 C8 5A 62 5E A4 25 4D D8 58 82 23 " +
            "C9 11 21 82 3A 5B 6D 22 49 BB 8D DE 36 E6 BB 14 " +
            "A5 4C B7 64 E7 67 26 4F 80 FD E9 5B 36 C6 B4 F8 " +
            "51 91 06 CE B9 E9 7A 11 E9 5E 65 A1 7A BA 32 F7 " +
            "90 A4 A7 74 DE 66 4F 98 87 44 C9 51 4F 96 03 CE " +
            "D5 C1 54 FA 30 6B 7B 89 CF 36 3C F3 0F 1F 16 AC " +
            "94 09 51 07 8F AB 18 88 7F CA E9 FE DB 71 26 3C " +
            "8A 1F A9 D7 52 D5 CF 19 90 AD 6D 40 3F 8C 37 9C " +
            "FD 72 A8 61 B7 7F B4 B7 30 55 25 E9 A3 18 4F 51 " +
            "6D C8 32 F3 C2 42 84 C4 5F BC 29 8F A2 2C 2F E0 " +
            "E3 7E 8B 96 52 F8 30 F9",
            // After Chi
            "97 F9 B1 08 15 86 5B 5E 7B EC AE 15 D7 32 6A 59 " +
            "6B FE 41 E3 E6 1E DE A8 58 56 46 B3 0B 17 A1 97 " +
            "DC AF 3C 14 1F F9 5A 62 5E 0E A9 11 DC FC 10 37 " +
            "6D 55 13 A2 FB 5A 69 69 49 0A C5 C5 26 66 2B A4 " +
            "FB 4C B3 60 2F 7F 24 4C 01 EC E9 D9 14 C5 D9 F8 " +
            "41 31 84 9A 3D AD 37 19 EE 1E 2D A0 7B 2A 32 B1 " +
            "C0 25 B3 DE EE 0F 37 99 87 54 CB 55 C6 16 03 DE " +
            "7D 8F 35 DB 72 79 7B 6F A4 F4 94 0B 5F 4F 30 98 " +
            "14 1C 51 06 8F 2F D1 89 6F 6A AD FE F6 79 16 B8 " +
            "C5 0D B9 64 52 C6 CF 39 80 A4 2C 44 BF 2C 3F 9C " +
            "B0 FA BA 73 F7 3D 34 33 22 61 2C E5 83 34 64 71 " +
            "CD 8A B0 E3 92 92 94 DD 43 BC 09 EE 07 2B AB E6 " +
            "E3 7B 8E 1E 52 F8 7B B9",
            // After Iota 
            "1D 79 B1 08 15 86 5B DE 7B EC AE 15 D7 32 6A 59 " +
            "6B FE 41 E3 E6 1E DE A8 58 56 46 B3 0B 17 A1 97 " +
            "DC AF 3C 14 1F F9 5A 62 5E 0E A9 11 DC FC 10 37 " +
            "6D 55 13 A2 FB 5A 69 69 49 0A C5 C5 26 66 2B A4 " +
            "FB 4C B3 60 2F 7F 24 4C 01 EC E9 D9 14 C5 D9 F8 " +
            "41 31 84 9A 3D AD 37 19 EE 1E 2D A0 7B 2A 32 B1 " +
            "C0 25 B3 DE EE 0F 37 99 87 54 CB 55 C6 16 03 DE " +
            "7D 8F 35 DB 72 79 7B 6F A4 F4 94 0B 5F 4F 30 98 " +
            "14 1C 51 06 8F 2F D1 89 6F 6A AD FE F6 79 16 B8 " +
            "C5 0D B9 64 52 C6 CF 39 80 A4 2C 44 BF 2C 3F 9C " +
            "B0 FA BA 73 F7 3D 34 33 22 61 2C E5 83 34 64 71 " +
            "CD 8A B0 E3 92 92 94 DD 43 BC 09 EE 07 2B AB E6 " +
            "E3 7B 8E 1E 52 F8 7B B9",
            // Round #3
            // After Theta
            "42 DF 28 AD 36 A5 EF FD EC C6 48 24 1E AF 93 E2 " +
            "E0 DB B1 0E D3 60 9F 64 9F 40 E8 CE 69 A8 98 C6 " +
            "52 C0 D6 EF 11 20 49 0E 01 A8 30 B4 FF DF A4 14 " +
            "FA 7F F5 93 32 C7 90 D2 C2 2F 35 28 13 18 6A 68 " +
            "3C 5A 1D 1D 4D C0 1D 1D 8F 83 03 22 1A 1C CA 94 " +
            "1E 97 1D 3F 1E 8E 83 3A 79 34 CB 91 B2 B7 CB 0A " +
            "4B 00 43 33 DB 71 76 55 40 42 65 28 A4 A9 3A 8F " +
            "F3 E0 DF 20 7C A0 68 03 FB 52 0D AE 7C 6C 84 BB " +
            "83 36 B7 37 46 B2 28 32 E4 4F 5D 13 C3 07 57 74 " +
            "02 1B 17 19 30 79 F6 68 0E CB C6 BF B1 F5 2C F0 " +
            "EF 5C 23 D6 D4 1E 80 10 B5 4B CA D4 4A A9 9D CA " +
            "46 AF 40 0E A7 EC D5 11 84 AA A7 93 65 94 92 B7 " +
            "6D 14 64 E5 5C 21 68 D5",
            // After Rho
            "42 DF 28 AD 36 A5 EF FD D9 8D 91 48 3C 5E 27 C5 " +
            "F8 76 AC C3 34 D8 27 19 86 8A 69 FC 09 84 EE 9C " +
            "00 49 72 90 02 B6 7E 8F FB FF 4D 4A 11 80 0A 43 " +
            "3F 29 73 0C 29 AD FF 57 9A F0 4B 0D CA 04 86 1A " +
            "AD 8E 8E 26 E0 8E 0E 1E A1 4C F9 38 38 20 A2 C1 " +
            "F1 B8 EC F8 F1 70 1C D4 2B E4 D1 2C 47 CA DE 2E " +
            "9A D9 8E B3 AB 5A 02 18 53 75 1E 81 84 CA 50 48 " +
            "10 3E 50 B4 81 79 F0 6F 5C F9 D8 08 77 F7 A5 1A " +
            "F6 C6 48 16 45 66 D0 E6 2B 3A F2 A7 AE 89 E1 83 " +
            "CF 1E 4D 60 E3 22 03 26 F0 0E CB C6 BF B1 F5 2C " +
            "00 42 BC 73 8D 58 53 7B D7 2E 29 53 2B A5 76 2A " +
            "E8 15 C8 E1 94 BD 3A C2 AA A7 93 65 94 92 B7 84 " +
            "5A 75 1B 05 59 39 57 08",
            // After Pi
            "42 DF 28 AD 36 A5 EF FD 3F 29 73 0C 29 AD FF 57 " +
            "9A D9 8E B3 AB 5A 02 18 CF 1E 4D 60 E3 22 03 26 " +
            "5A 75 1B 05 59 39 57 08 86 8A 69 FC 09 84 EE 9C " +
            "A1 4C F9 38 38 20 A2 C1 F1 B8 EC F8 F1 70 1C D4 " +
            "F6 C6 48 16 45 66 D0 E6 E8 15 C8 E1 94 BD 3A C2 " +
            "D9 8D 91 48 3C 5E 27 C5 9A F0 4B 0D CA 04 86 1A " +
            "53 75 1E 81 84 CA 50 48 F0 0E CB C6 BF B1 F5 2C " +
            "00 42 BC 73 8D 58 53 7B 00 49 72 90 02 B6 7E 8F " +
            "FB FF 4D 4A 11 80 0A 43 2B E4 D1 2C 47 CA DE 2E " +
            "2B 3A F2 A7 AE 89 E1 83 AA A7 93 65 94 92 B7 84 " +
            "F8 76 AC C3 34 D8 27 19 AD 8E 8E 26 E0 8E 0E 1E " +
            "10 3E 50 B4 81 79 F0 6F 5C F9 D8 08 77 F7 A5 1A " +
            "D7 2E 29 53 2B A5 76 2A",
            // After Chi
            "C2 0F A4 1E B4 F7 EF F5 7A 2F 32 4C 69 8D FE 71 " +
            "8A B8 9C B6 B3 43 56 10 CF 94 6D C8 C5 A6 AB D3 " +
            "67 55 48 05 50 31 47 0A D6 3A 6D 3C C8 D4 F2 88 " +
            "A7 0A F9 3E 3C 26 62 E3 F9 A9 6C 19 61 E9 36 D4 " +
            "F0 4C 69 0A 4C 66 14 FA C9 51 58 E1 A4 9D 3A 83 " +
            "98 88 85 C8 38 94 77 85 3A FA 8A 4B F1 35 23 3E " +
            "53 35 2A B0 84 82 52 1B 29 83 CA CE 8F B7 D1 A8 " +
            "02 32 F6 76 4F 58 D3 61 00 49 E2 B4 44 FC AA A3 " +
            "FB E5 6F C9 B9 81 2B C2 AB 61 D0 6C 57 D8 C8 2A " +
            "2B 72 92 37 AC AD A9 88 51 11 9E 2F 85 92 B7 C4 " +
            "E8 46 FC 53 35 A9 D7 78 E1 4F 06 2E 96 08 0B 0E " +
            "93 38 71 E7 89 79 A2 4F 74 A9 5C 88 63 AF A4 0B " +
            "D2 A6 2B 77 EB A3 7E 2C",
            // After Iota 
            "C2 8F A4 9E B4 F7 EF 75 7A 2F 32 4C 69 8D FE 71 " +
            "8A B8 9C B6 B3 43 56 10 CF 94 6D C8 C5 A6 AB D3 " +
            "67 55 48 05 50 31 47 0A D6 3A 6D 3C C8 D4 F2 88 " +
            "A7 0A F9 3E 3C 26 62 E3 F9 A9 6C 19 61 E9 36 D4 " +
            "F0 4C 69 0A 4C 66 14 FA C9 51 58 E1 A4 9D 3A 83 " +
            "98 88 85 C8 38 94 77 85 3A FA 8A 4B F1 35 23 3E " +
            "53 35 2A B0 84 82 52 1B 29 83 CA CE 8F B7 D1 A8 " +
            "02 32 F6 76 4F 58 D3 61 00 49 E2 B4 44 FC AA A3 " +
            "FB E5 6F C9 B9 81 2B C2 AB 61 D0 6C 57 D8 C8 2A " +
            "2B 72 92 37 AC AD A9 88 51 11 9E 2F 85 92 B7 C4 " +
            "E8 46 FC 53 35 A9 D7 78 E1 4F 06 2E 96 08 0B 0E " +
            "93 38 71 E7 89 79 A2 4F 74 A9 5C 88 63 AF A4 0B " +
            "D2 A6 2B 77 EB A3 7E 2C",
            // Round #4
            // After Theta
            "17 E5 A7 E8 76 1D B6 B4 2F E7 96 E9 4D 7C 58 A6 " +
            "E5 CD B5 0E AB BF 0F 74 89 EB B1 C8 E6 A4 3C 69 " +
            "E7 B1 EC AC F2 80 0B 4E 03 50 6E 4A 0A 3E AB 49 " +
            "F2 C2 5D 9B 18 D7 C4 34 96 DC 45 A1 79 15 6F B0 " +
            "B6 33 B5 0A 6F 64 83 40 49 B5 FC 48 06 2C 76 C7 " +
            "4D E2 86 BE FA 7E 2E 44 6F 32 2E EE D5 C4 85 E9 " +
            "3C 40 03 08 9C 7E 0B 7F 6F FC 16 CE AC B5 46 12 " +
            "82 D6 52 DF ED E9 9F 25 D5 23 E1 C2 86 16 F3 62 " +
            "AE 2D CB 6C 9D 70 8D 15 C4 14 F9 D4 4F 24 91 4E " +
            "6D 0D 4E 37 8F AF 3E 32 D1 F5 3A 86 27 23 FB 80 " +
            "3D 2C FF 25 F7 43 8E B9 B4 87 A2 8B B2 F9 AD D9 " +
            "FC 4D 58 5F 91 85 FB 2B 32 D6 80 88 40 AD 33 B1 " +
            "52 42 8F DE 49 12 32 68",
            // After Rho
            "17 E5 A7 E8 76 1D B6 B4 5F CE 2D D3 9B F8 B0 4C " +
            "79 73 AD C3 EA EF 03 5D 4E CA 93 96 B8 1E 8B 6C " +
            "07 5C 70 3A 8F 65 67 95 A4 E0 B3 9A 34 00 E5 A6 " +
            "B5 89 71 4D 4C 23 2F DC AC 25 77 51 68 5E C5 1B " +
            "99 5A 85 37 B2 41 20 DB 62 77 9C 54 CB 8F 64 C0 " +
            "6A 12 37 F4 D5 F7 73 21 A6 BF C9 B8 B8 57 13 17 " +
            "40 E0 F4 5B F8 E3 01 1A 6B 8D 24 DE F8 2D 9C 59 " +
            "EF F6 F4 CF 12 41 6B A9 85 0D 2D E6 C5 AA 47 C2 " +
            "99 AD 13 AE B1 C2 B5 65 48 27 62 8A 7C EA 27 92 " +
            "D5 47 A6 AD C1 E9 E6 F1 80 D1 F5 3A 86 27 23 FB " +
            "39 E6 F6 B0 FC 97 DC 0F D3 1E 8A 2E CA E6 B7 66 " +
            "BF 09 EB 2B B2 70 7F 85 D6 80 88 40 AD 33 B1 32 " +
            "0C 9A 94 D0 A3 77 92 84",
            // After Pi
            "17 E5 A7 E8 76 1D B6 B4 B5 89 71 4D 4C 23 2F DC " +
            "40 E0 F4 5B F8 E3 01 1A D5 47 A6 AD C1 E9 E6 F1 " +
            "0C 9A 94 D0 A3 77 92 84 4E CA 93 96 B8 1E 8B 6C " +
            "62 77 9C 54 CB 8F 64 C0 6A 12 37 F4 D5 F7 73 21 " +
            "99 AD 13 AE B1 C2 B5 65 BF 09 EB 2B B2 70 7F 85 " +
            "5F CE 2D D3 9B F8 B0 4C AC 25 77 51 68 5E C5 1B " +
            "6B 8D 24 DE F8 2D 9C 59 80 D1 F5 3A 86 27 23 FB " +
            "39 E6 F6 B0 FC 97 DC 0F 07 5C 70 3A 8F 65 67 95 " +
            "A4 E0 B3 9A 34 00 E5 A6 A6 BF C9 B8 B8 57 13 17 " +
            "48 27 62 8A 7C EA 27 92 D6 80 88 40 AD 33 B1 32 " +
            "79 73 AD C3 EA EF 03 5D 99 5A 85 37 B2 41 20 DB " +
            "EF F6 F4 CF 12 41 6B A9 85 0D 2D E6 C5 AA 47 C2 " +
            "D3 1E 8A 2E CA E6 B7 66",
            // After Chi
            "57 85 23 FA C6 DD B6 B6 20 8E 73 E9 4D 2B C9 3D " +
            "48 78 E4 0B DA F5 11 1E C6 22 85 85 95 E1 C2 C1 " +
            "AC 92 C4 D5 AB 55 9B CC 46 CA B0 36 AC 6E 98 4D " +
            "F3 DA 9C 5E EB 8F E0 84 4C 12 DF F5 D7 C7 39 A1 " +
            "D9 6F 03 3A B9 CC 35 0D 9F 3C E7 6B F1 F1 1B 05 " +
            "1C 46 2D 5D 0B D9 A8 0C 2C 75 A6 71 6E 5C E6 B9 " +
            "52 AB 26 5E 80 BD 40 5D C6 D9 FC 79 85 4F 03 BB " +
            "99 C7 A4 B0 9C 91 99 1C 05 43 38 1A 07 32 75 84 " +
            "EC E0 91 98 70 A8 C1 26 30 3F 41 F8 39 46 83 37 " +
            "49 7B 12 B0 7E AE 61 17 76 20 0B C0 9D 33 31 10 " +
            "1F D7 DD 0B EA EF 48 7D 99 53 8C 17 77 EB 24 99 " +
            "BD E4 76 C7 18 05 DB 8D AD 6C 08 27 E5 A3 47 DB " +
            "53 16 8A 1A DA E6 97 E4",
            // After Iota 
            "DC 05 23 FA C6 DD B6 B6 20 8E 73 E9 4D 2B C9 3D " +
            "48 78 E4 0B DA F5 11 1E C6 22 85 85 95 E1 C2 C1 " +
            "AC 92 C4 D5 AB 55 9B CC 46 CA B0 36 AC 6E 98 4D " +
            "F3 DA 9C 5E EB 8F E0 84 4C 12 DF F5 D7 C7 39 A1 " +
            "D9 6F 03 3A B9 CC 35 0D 9F 3C E7 6B F1 F1 1B 05 " +
            "1C 46 2D 5D 0B D9 A8 0C 2C 75 A6 71 6E 5C E6 B9 " +
            "52 AB 26 5E 80 BD 40 5D C6 D9 FC 79 85 4F 03 BB " +
            "99 C7 A4 B0 9C 91 99 1C 05 43 38 1A 07 32 75 84 " +
            "EC E0 91 98 70 A8 C1 26 30 3F 41 F8 39 46 83 37 " +
            "49 7B 12 B0 7E AE 61 17 76 20 0B C0 9D 33 31 10 " +
            "1F D7 DD 0B EA EF 48 7D 99 53 8C 17 77 EB 24 99 " +
            "BD E4 76 C7 18 05 DB 8D AD 6C 08 27 E5 A3 47 DB " +
            "53 16 8A 1A DA E6 97 E4",
            // Round #5
            // After Theta
            "46 7F 8C BC D9 4A 5C E9 0A A6 7C 57 98 05 13 83 " +
            "B9 EC 71 E0 71 90 9F D6 03 87 A3 B2 3A EC 8D DA " +
            "A9 2A 12 84 80 55 3E 6A DC B0 1F 70 B3 F9 72 12 " +
            "D9 F2 93 E0 3E A1 3A 3A BD 86 4A 1E 7C A2 B7 69 " +
            "1C CA 25 0D 16 C1 7A 16 9A 84 31 3A DA F1 BE A3 " +
            "86 3C 82 1B 14 4E 42 53 06 5D A9 CF BB 72 3C 07 " +
            "A3 3F B3 B5 2B D8 CE 95 03 7C DA 4E 2A 42 4C A0 " +
            "9C 7F 72 E1 B7 91 3C BA 9F 39 97 5C 18 A5 9F DB " +
            "C6 C8 9E 26 A5 86 1B 98 C1 AB D4 13 92 23 0D FF " +
            "8C DE 34 87 D1 A3 2E 0C 73 98 DD 91 B6 33 94 B6 " +
            "85 AD 72 4D F5 78 A2 22 B3 7B 83 A9 A2 C5 FE 27 " +
            "4C 70 E3 2C B3 60 55 45 68 C9 2E 10 4A AE 08 C0 " +
            "56 AE 5C 4B F1 E6 32 42",
            // After Rho
            "46 7F 8C BC D9 4A 5C E9 15 4C F9 AE 30 0B 26 06 " +
            "2E 7B 1C 78 1C E4 A7 75 C3 DE A8 3D 70 38 2A AB " +
            "AC F2 51 4B 55 91 20 04 37 9B 2F 27 C1 0D FB 01 " +
            "09 EE 13 AA A3 93 2D 3F 5A AF A1 92 07 9F E8 6D " +
            "E5 92 06 8B 60 3D 0B 0E EF 3B AA 49 18 A3 A3 1D " +
            "32 E4 11 DC A0 70 12 9A 1C 18 74 A5 3E EF CA F1 " +
            "AD 5D C1 76 AE 1C FD 99 84 98 40 07 F8 B4 9D 54 " +
            "F0 DB 48 1E 5D CE 3F B9 B9 30 4A 3F B7 3F 73 2E " +
            "D3 A4 D4 70 03 D3 18 D9 86 FF E0 55 EA 09 C9 91 " +
            "D4 85 81 D1 9B E6 30 7A B6 73 98 DD 91 B6 33 94 " +
            "89 8A 14 B6 CA 35 D5 E3 CC EE 0D A6 8A 16 FB 9F " +
            "09 6E 9C 65 16 AC AA 88 C9 2E 10 4A AE 08 C0 68 " +
            "8C 90 95 2B D7 52 BC B9",
            // After Pi
            "46 7F 8C BC D9 4A 5C E9 09 EE 13 AA A3 93 2D 3F " +
            "AD 5D C1 76 AE 1C FD 99 D4 85 81 D1 9B E6 30 7A " +
            "8C 90 95 2B D7 52 BC B9 C3 DE A8 3D 70 38 2A AB " +
            "EF 3B AA 49 18 A3 A3 1D 32 E4 11 DC A0 70 12 9A " +
            "D3 A4 D4 70 03 D3 18 D9 09 6E 9C 65 16 AC AA 88 " +
            "15 4C F9 AE 30 0B 26 06 5A AF A1 92 07 9F E8 6D " +
            "84 98 40 07 F8 B4 9D 54 B6 73 98 DD 91 B6 33 94 " +
            "89 8A 14 B6 CA 35 D5 E3 AC F2 51 4B 55 91 20 04 " +
            "37 9B 2F 27 C1 0D FB 01 1C 18 74 A5 3E EF CA F1 " +
            "86 FF E0 55 EA 09 C9 91 C9 2E 10 4A AE 08 C0 68 " +
            "2E 7B 1C 78 1C E4 A7 75 E5 92 06 8B 60 3D 0B 0E " +
            "F0 DB 48 1E 5D CE 3F B9 B9 30 4A 3F B7 3F 73 2E " +
            "CC EE 0D A6 8A 16 FB 9F",
            // After Chi
            "E2 6E 4C E8 D5 46 8C 69 59 6E 13 2B B2 71 2D 5D " +
            "A5 4D D5 5C EA 0C 71 18 96 EA 89 45 93 EE 70 3A " +
            "85 10 86 29 F5 C3 9D AF D3 1A B9 A9 D0 68 3A 29 " +
            "2E 3B 6E 69 1B 20 AB 5C 3A AE 19 D9 B4 5C B0 9A " +
            "11 34 F4 68 63 C3 18 FA 25 4F 9E 25 1E 2F 2B 9C " +
            "91 5C B9 AB C8 2B 33 16 68 CC 39 4A 06 9D CA ED " +
            "8D 10 44 25 B2 B5 59 37 A2 37 71 D5 A1 BC 11 90 " +
            "C3 29 14 A6 CD A1 1D 8A A4 F2 01 CB 6B 73 20 F4 " +
            "B5 7C AF 77 01 0D FA 01 55 18 64 AF 3A EF CA 99 " +
            "A2 2F A1 54 BB 98 E9 95 DA 27 3E 6E 2E 04 1B 69 " +
            "3E 32 54 6C 01 26 93 C4 EC B2 04 AA C2 0C 4B 08 " +
            "B4 15 4D 9E 55 CE B7 28 9B 21 5A 67 A3 DF 77 4E " +
            "0D 6E 0F 25 EA 0F F3 95",
            // After Iota 
            "E3 6E 4C 68 D5 46 8C 69 59 6E 13 2B B2 71 2D 5D " +
            "A5 4D D5 5C EA 0C 71 18 96 EA 89 45 93 EE 70 3A " +
            "85 10 86 29 F5 C3 9D AF D3 1A B9 A9 D0 68 3A 29 " +
            "2E 3B 6E 69 1B 20 AB 5C 3A AE 19 D9 B4 5C B0 9A " +
            "11 34 F4 68 63 C3 18 FA 25 4F 9E 25 1E 2F 2B 9C " +
            "91 5C B9 AB C8 2B 33 16 68 CC 39 4A 06 9D CA ED " +
            "8D 10 44 25 B2 B5 59 37 A2 37 71 D5 A1 BC 11 90 " +
            "C3 29 14 A6 CD A1 1D 8A A4 F2 01 CB 6B 73 20 F4 " +
            "B5 7C AF 77 01 0D FA 01 55 18 64 AF 3A EF CA 99 " +
            "A2 2F A1 54 BB 98 E9 95 DA 27 3E 6E 2E 04 1B 69 " +
            "3E 32 54 6C 01 26 93 C4 EC B2 04 AA C2 0C 4B 08 " +
            "B4 15 4D 9E 55 CE B7 28 9B 21 5A 67 A3 DF 77 4E " +
            "0D 6E 0F 25 EA 0F F3 95",
            // Round #6
            // After Theta
            "DA FF AF 22 EE 9A 34 E7 84 7B 49 C5 12 A8 D0 32 " +
            "DA D4 D5 1E 15 6D 43 EA 0D 6B 52 16 D5 A7 13 B4 " +
            "EF 27 42 78 F3 B4 16 E8 EA 8B 5A E3 EB B4 82 A7 " +
            "F3 2E 34 87 BB F9 56 33 45 37 19 9B 4B 3D 82 68 " +
            "8A B5 2F 3B 25 8A 7B 74 4F 78 5A 74 18 58 A0 DB " +
            "A8 CD 5A E1 F3 F7 8B 98 B5 D9 63 A4 A6 44 37 82 " +
            "F2 89 44 67 4D D4 6B C5 39 B6 AA 86 E7 F5 72 1E " +
            "A9 1E D0 F7 CB D6 96 CD 9D 63 E2 81 50 AF 98 7A " +
            "68 69 F5 99 A1 D4 07 6E 2A 81 64 ED C5 8E F8 6B " +
            "39 AE 7A 07 FD D1 8A 1B B0 10 FA 3F 28 73 90 2E " +
            "07 A3 B7 26 3A FA 2B 4A 31 A7 5E 44 62 D5 B6 67 " +
            "CB 8C 4D DC AA AF 85 DA 00 A0 81 34 E5 96 14 C0 " +
            "67 59 CB 74 EC 78 78 D2",
            // After Rho
            "DA FF AF 22 EE 9A 34 E7 08 F7 92 8A 25 50 A1 65 " +
            "36 75 B5 47 45 DB 90 BA 7D 3A 41 DB B0 26 65 51 " +
            "A7 B5 40 7F 3F 11 C2 9B BE 4E 2B 78 AA BE A8 35 " +
            "73 B8 9B 6F 35 33 EF 42 5A D1 4D C6 E6 52 8F 20 " +
            "DA 97 9D 12 C5 3D 3A C5 05 BA FD 84 A7 45 87 81 " +
            "44 6D D6 0A 9F BF 5F C4 08 D6 66 8F 91 9A 12 DD " +
            "3A 6B A2 5E 2B 96 4F 24 EB E5 3C 72 6C 55 0D CF " +
            "FB 65 6B CB E6 54 0F E8 03 A1 5E 31 F5 3A C7 C4 " +
            "3E 33 94 FA C0 0D 2D AD FC 35 95 40 B2 F6 62 47 " +
            "5A 71 23 C7 55 EF A0 3F 2E B0 10 FA 3F 28 73 90 " +
            "AF 28 1D 8C DE 9A E8 E8 C5 9C 7A 11 89 55 DB 9E " +
            "99 B1 89 5B F5 B5 50 7B A0 81 34 E5 96 14 C0 00 " +
            "9E F4 59 D6 32 1D 3B 1E",
            // After Pi
            "DA FF AF 22 EE 9A 34 E7 73 B8 9B 6F 35 33 EF 42 " +
            "3A 6B A2 5E 2B 96 4F 24 5A 71 23 C7 55 EF A0 3F " +
            "9E F4 59 D6 32 1D 3B 1E 7D 3A 41 DB B0 26 65 51 " +
            "05 BA FD 84 A7 45 87 81 44 6D D6 0A 9F BF 5F C4 " +
            "3E 33 94 FA C0 0D 2D AD 99 B1 89 5B F5 B5 50 7B " +
            "08 F7 92 8A 25 50 A1 65 5A D1 4D C6 E6 52 8F 20 " +
            "EB E5 3C 72 6C 55 0D CF 2E B0 10 FA 3F 28 73 90 " +
            "AF 28 1D 8C DE 9A E8 E8 A7 B5 40 7F 3F 11 C2 9B " +
            "BE 4E 2B 78 AA BE A8 35 08 D6 66 8F 91 9A 12 DD " +
            "FC 35 95 40 B2 F6 62 47 A0 81 34 E5 96 14 C0 00 " +
            "36 75 B5 47 45 DB 90 BA DA 97 9D 12 C5 3D 3A C5 " +
            "FB 65 6B CB E6 54 0F E8 03 A1 5E 31 F5 3A C7 C4 " +
            "C5 9C 7A 11 89 55 DB 9E",
            // After Chi
            "D2 BC 8F 32 E4 1E 34 C3 33 A8 9A EE 61 5A 4F 59 " +
            "BE EF FA 4E 09 86 54 24 1A 7A 85 E7 99 6D A4 DE " +
            "BF F4 49 9B 23 3C F0 1E 3D 7F 43 D1 A8 9C 3D 15 " +
            "3F A8 FD 74 E7 45 A7 A8 C5 ED DF 0B AA 0F 0F 96 " +
            "5A 39 D4 7A C0 0F 08 AD 99 31 35 5F F2 F4 D2 FB " +
            "A9 D3 A2 BA 2D 55 A1 AA 5E C1 4D 4E F5 7A FD 30 " +
            "6A ED 31 76 AC C7 85 A7 2E 67 92 F8 1E 68 72 95 " +
            "FD 28 50 C8 1C 98 E6 E8 A7 25 04 F8 2E 11 D0 53 " +
            "4A 6F BA 38 88 DA C8 37 08 56 46 2A 95 9A 92 DD " +
            "FB 01 D5 5A 9B F7 60 DC B8 CB 1F E5 16 BA E8 24 " +
            "17 15 D7 8E 67 9B 95 92 DA 17 89 22 D4 17 FA C1 " +
            "3F 79 4B CB EE 11 17 F2 31 C0 DB 77 B1 B0 C7 E4 " +
            "0D 1E 72 01 09 71 F1 DB",
            // After Iota 
            "53 3C 8F B2 E4 1E 34 43 33 A8 9A EE 61 5A 4F 59 " +
            "BE EF FA 4E 09 86 54 24 1A 7A 85 E7 99 6D A4 DE " +
            "BF F4 49 9B 23 3C F0 1E 3D 7F 43 D1 A8 9C 3D 15 " +
            "3F A8 FD 74 E7 45 A7 A8 C5 ED DF 0B AA 0F 0F 96 " +
            "5A 39 D4 7A C0 0F 08 AD 99 31 35 5F F2 F4 D2 FB " +
            "A9 D3 A2 BA 2D 55 A1 AA 5E C1 4D 4E F5 7A FD 30 " +
            "6A ED 31 76 AC C7 85 A7 2E 67 92 F8 1E 68 72 95 " +
            "FD 28 50 C8 1C 98 E6 E8 A7 25 04 F8 2E 11 D0 53 " +
            "4A 6F BA 38 88 DA C8 37 08 56 46 2A 95 9A 92 DD " +
            "FB 01 D5 5A 9B F7 60 DC B8 CB 1F E5 16 BA E8 24 " +
            "17 15 D7 8E 67 9B 95 92 DA 17 89 22 D4 17 FA C1 " +
            "3F 79 4B CB EE 11 17 F2 31 C0 DB 77 B1 B0 C7 E4 " +
            "0D 1E 72 01 09 71 F1 DB",
            // Round #7
            // After Theta
            "B9 77 FD C6 69 D5 A6 DF 08 88 14 E5 A0 8D 15 10 " +
            "35 9D 78 11 FC B4 81 AF E1 CA 1E E5 48 9F 44 01 " +
            "F5 51 FF 8C 1F CB 53 BB D7 34 31 A5 25 57 AF 89 " +
            "04 88 73 7F 26 92 FD E1 4E 9F 5D 54 5F 3D DA 1D " +
            "A1 89 4F 78 11 FD E8 72 D3 94 83 48 CE 03 71 5E " +
            "43 98 D0 CE A0 9E 33 36 65 E1 C3 45 34 AD A7 79 " +
            "E1 9F B3 29 59 F5 50 2C D5 D7 09 FA CF 9A 92 4A " +
            "B7 8D E6 DF 20 6F 45 4D 4D 6E 76 8C A3 DA 42 CF " +
            "71 4F 34 33 49 0D 92 7E 83 24 C4 75 60 A8 47 56 " +
            "00 B1 4E 58 4A 05 80 03 F2 6E A9 F2 2A 4D 4B 81 " +
            "FD 5E A5 FA EA 50 07 0E E1 37 07 29 15 C0 A0 88 " +
            "B4 0B C9 94 1B 23 C2 79 CA 70 40 75 60 42 27 3B " +
            "47 BB C4 16 35 86 52 7E",
            // After Rho
            "B9 77 FD C6 69 D5 A6 DF 10 10 29 CA 41 1B 2B 20 " +
            "4D 27 5E 04 3F 6D E0 6B F4 49 14 10 AE EC 51 8E " +
            "58 9E DA AD 8F FA 67 FC 5A 72 F5 9A 78 4D 13 53 " +
            "F7 67 22 D9 1F 4E 80 38 87 D3 67 17 D5 57 8F 76 " +
            "C4 27 BC 88 7E 74 B9 D0 10 E7 35 4D 39 88 E4 3C " +
            "19 C2 84 76 06 F5 9C B1 E6 95 85 0F 17 D1 B4 9E " +
            "4D C9 AA 87 62 09 FF 9C 35 25 95 AA AF 13 F4 9F " +
            "6F 90 B7 A2 A6 DB 46 F3 18 47 B5 85 9E 9B DC EC " +
            "66 26 A9 41 D2 2F EE 89 23 AB 41 12 E2 3A 30 D4 " +
            "00 70 00 20 D6 09 4B A9 81 F2 6E A9 F2 2A 4D 4B " +
            "1D 38 F4 7B 95 EA AB 43 86 DF 1C A4 54 00 83 22 " +
            "76 21 99 72 63 44 38 8F 70 40 75 60 42 27 3B CA " +
            "94 DF D1 2E B1 45 8D A1",
            // After Pi
            "B9 77 FD C6 69 D5 A6 DF F7 67 22 D9 1F 4E 80 38 " +
            "4D C9 AA 87 62 09 FF 9C 00 70 00 20 D6 09 4B A9 " +
            "94 DF D1 2E B1 45 8D A1 F4 49 14 10 AE EC 51 8E " +
            "10 E7 35 4D 39 88 E4 3C 19 C2 84 76 06 F5 9C B1 " +
            "66 26 A9 41 D2 2F EE 89 76 21 99 72 63 44 38 8F " +
            "10 10 29 CA 41 1B 2B 20 87 D3 67 17 D5 57 8F 76 " +
            "35 25 95 AA AF 13 F4 9F 81 F2 6E A9 F2 2A 4D 4B " +
            "1D 38 F4 7B 95 EA AB 43 58 9E DA AD 8F FA 67 FC " +
            "5A 72 F5 9A 78 4D 13 53 E6 95 85 0F 17 D1 B4 9E " +
            "23 AB 41 12 E2 3A 30 D4 70 40 75 60 42 27 3B CA " +
            "4D 27 5E 04 3F 6D E0 6B C4 27 BC 88 7E 74 B9 D0 " +
            "6F 90 B7 A2 A6 DB 46 F3 18 47 B5 85 9E 9B DC EC " +
            "86 DF 1C A4 54 00 83 22",
            // After Chi
            "B1 FF 75 C0 09 D4 D9 5B F7 57 22 F9 8B 4E 80 19 " +
            "D9 46 7B 89 43 4D 7B 9C 29 50 2C E0 9E 99 69 F7 " +
            "D2 DF D3 37 A7 4F 8D 81 FD 49 94 22 A8 99 49 0F " +
            "76 C3 1C 4C E9 82 86 34 09 C3 94 44 27 B5 8C B7 " +
            "E6 6E AD 41 5E 87 AF 89 76 87 B8 3F 72 44 9C BF " +
            "20 34 B9 62 6B 1B 5B A9 07 01 0D 16 85 7F 86 36 " +
            "29 2D 05 F8 AA D3 56 9F 81 F2 67 29 B2 3B 4D 6B " +
            "9A FB B2 6E 01 AE 2F 15 FC 1B DA A8 88 6A C3 70 " +
            "5B 58 B5 8A 98 67 13 13 B6 D5 B1 6F 17 D4 BF 94 " +
            "2B 35 CB 9F 6F E2 74 E0 72 20 50 72 32 22 2B C9 " +
            "66 B7 5D 26 BF E6 A6 48 D4 60 BC 8D 66 74 21 DC " +
            "E9 08 BF 82 E6 DB 45 F1 51 67 F7 85 B5 F6 BC A5 " +
            "06 DF BC 2C 14 10 9A B2",
            // After Iota 
            "B8 7F 75 C0 09 D4 D9 DB F7 57 22 F9 8B 4E 80 19 " +
            "D9 46 7B 89 43 4D 7B 9C 29 50 2C E0 9E 99 69 F7 " +
            "D2 DF D3 37 A7 4F 8D 81 FD 49 94 22 A8 99 49 0F " +
            "76 C3 1C 4C E9 82 86 34 09 C3 94 44 27 B5 8C B7 " +
            "E6 6E AD 41 5E 87 AF 89 76 87 B8 3F 72 44 9C BF " +
            "20 34 B9 62 6B 1B 5B A9 07 01 0D 16 85 7F 86 36 " +
            "29 2D 05 F8 AA D3 56 9F 81 F2 67 29 B2 3B 4D 6B " +
            "9A FB B2 6E 01 AE 2F 15 FC 1B DA A8 88 6A C3 70 " +
            "5B 58 B5 8A 98 67 13 13 B6 D5 B1 6F 17 D4 BF 94 " +
            "2B 35 CB 9F 6F E2 74 E0 72 20 50 72 32 22 2B C9 " +
            "66 B7 5D 26 BF E6 A6 48 D4 60 BC 8D 66 74 21 DC " +
            "E9 08 BF 82 E6 DB 45 F1 51 67 F7 85 B5 F6 BC A5 " +
            "06 DF BC 2C 14 10 9A B2",
            // Round #8
            // After Theta
            "E1 79 35 B0 C8 03 33 22 45 12 35 46 09 DC 98 FE " +
            "B8 D7 F4 08 0B 8E 4F E8 1B 9D A2 48 45 92 2D 87 " +
            "18 1C B6 B8 F5 CB 93 5A A4 4F D4 52 69 4E A3 F6 " +
            "C4 86 0B F3 6B 10 9E D3 68 52 1B C5 6F 76 B8 C3 " +
            "D4 A3 23 E9 85 8C EB F9 BC 44 DD B0 20 C0 82 64 " +
            "79 32 F9 12 AA CC B1 50 B5 44 1A A9 07 ED 9E D1 " +
            "48 BC 8A 79 E2 10 62 EB B3 3F E9 81 69 30 09 1B " +
            "50 38 D7 E1 53 2A 31 CE A5 1D 9A D8 49 BD 29 89 " +
            "E9 1D A2 35 1A F5 0B F4 D7 44 3E EE 5F 17 8B E0 " +
            "19 F8 45 37 B4 E9 30 90 B8 E3 35 FD 60 A6 35 12 " +
            "3F B1 1D 56 7E 31 4C B1 66 25 AB 32 E4 E6 39 3B " +
            "88 99 30 03 AE 18 71 85 63 AA 79 2D 6E FD F8 D5 " +
            "CC 1C D9 A3 46 94 84 69",
            // After Rho
            "E1 79 35 B0 C8 03 33 22 8B 24 6A 8C 12 B8 31 FD " +
            "EE 35 3D C2 82 E3 13 3A 24 D9 72 B8 D1 29 8A 54 " +
            "5F 9E D4 C2 E0 B0 C5 AD 95 E6 34 6A 4F FA 44 2D " +
            "30 BF 06 E1 39 4D 6C B8 30 9A D4 46 F1 9B 1D EE " +
            "D1 91 F4 42 C6 F5 7C EA 2C 48 C6 4B D4 0D 0B 02 " +
            "CA 93 C9 97 50 65 8E 85 46 D7 12 69 A4 1E B4 7B " +
            "CC 13 87 10 5B 47 E2 55 60 12 36 66 7F D2 03 D3 " +
            "F0 29 95 18 67 28 9C EB B1 93 7A 53 12 4B 3B 34 " +
            "B4 46 A3 7E 81 3E BD 43 45 F0 6B 22 1F F7 AF 8B " +
            "1D 06 32 03 BF E8 86 36 12 B8 E3 35 FD 60 A6 35 " +
            "30 C5 FE C4 76 58 F9 C5 98 95 AC CA 90 9B E7 EC " +
            "31 13 66 C0 15 23 AE 10 AA 79 2D 6E FD F8 D5 63 " +
            "61 1A 33 47 F6 A8 11 25",
            // After Pi
            "E1 79 35 B0 C8 03 33 22 30 BF 06 E1 39 4D 6C B8 " +
            "CC 13 87 10 5B 47 E2 55 1D 06 32 03 BF E8 86 36 " +
            "61 1A 33 47 F6 A8 11 25 24 D9 72 B8 D1 29 8A 54 " +
            "2C 48 C6 4B D4 0D 0B 02 CA 93 C9 97 50 65 8E 85 " +
            "B4 46 A3 7E 81 3E BD 43 31 13 66 C0 15 23 AE 10 " +
            "8B 24 6A 8C 12 B8 31 FD 30 9A D4 46 F1 9B 1D EE " +
            "60 12 36 66 7F D2 03 D3 12 B8 E3 35 FD 60 A6 35 " +
            "30 C5 FE C4 76 58 F9 C5 5F 9E D4 C2 E0 B0 C5 AD " +
            "95 E6 34 6A 4F FA 44 2D 46 D7 12 69 A4 1E B4 7B " +
            "45 F0 6B 22 1F F7 AF 8B AA 79 2D 6E FD F8 D5 63 " +
            "EE 35 3D C2 82 E3 13 3A D1 91 F4 42 C6 F5 7C EA " +
            "F0 29 95 18 67 28 9C EB B1 93 7A 53 12 4B 3B 34 " +
            "98 95 AC CA 90 9B E7 EC",
            // After Chi
            "2D 79 B4 A0 8A 01 B1 67 21 BB 36 E2 9D E5 68 9A " +
            "AC 0B 86 54 1B 47 F3 54 9D 67 36 B3 B7 EB A4 34 " +
            "71 9C 31 06 C7 E4 5D BD E6 4A 7B 2C D1 49 0E D1 " +
            "18 0C E4 23 55 17 3A 40 CB 82 8D 17 44 64 8C 95 " +
            "B0 8E B3 46 41 36 BD 07 39 13 E2 83 11 27 AF 12 " +
            "CB 24 48 AC 1C F8 33 EC 22 32 15 57 71 BB B9 CA " +
            "40 57 2A A6 7D CA 5A 13 99 98 E3 3D FD C0 A6 0D " +
            "00 5F 6A 86 97 5B F5 C7 1D 8F D6 C3 40 B4 75 FF " +
            "94 C6 5D 68 54 1B 4F AD EC DE 16 25 44 16 E4 1B " +
            "10 76 BB A2 1F F7 AF 07 2A 19 0D 46 F2 B2 D5 63 " +
            "CE 1D 3C DA A3 EB 93 3B D0 03 9E 01 D6 B6 5F FE " +
            "F8 2D 11 90 E7 B8 58 23 D7 B3 6B 53 10 2B 2B 26 " +
            "89 15 6C CA D4 8F 8B 2C",
            // After Iota 
            "A7 79 B4 A0 8A 01 B1 67 21 BB 36 E2 9D E5 68 9A " +
            "AC 0B 86 54 1B 47 F3 54 9D 67 36 B3 B7 EB A4 34 " +
            "71 9C 31 06 C7 E4 5D BD E6 4A 7B 2C D1 49 0E D1 " +
            "18 0C E4 23 55 17 3A 40 CB 82 8D 17 44 64 8C 95 " +
            "B0 8E B3 46 41 36 BD 07 39 13 E2 83 11 27 AF 12 " +
            "CB 24 48 AC 1C F8 33 EC 22 32 15 57 71 BB B9 CA " +
            "40 57 2A A6 7D CA 5A 13 99 98 E3 3D FD C0 A6 0D " +
            "00 5F 6A 86 97 5B F5 C7 1D 8F D6 C3 40 B4 75 FF " +
            "94 C6 5D 68 54 1B 4F AD EC DE 16 25 44 16 E4 1B " +
            "10 76 BB A2 1F F7 AF 07 2A 19 0D 46 F2 B2 D5 63 " +
            "CE 1D 3C DA A3 EB 93 3B D0 03 9E 01 D6 B6 5F FE " +
            "F8 2D 11 90 E7 B8 58 23 D7 B3 6B 53 10 2B 2B 26 " +
            "89 15 6C CA D4 8F 8B 2C",
            // Round #9
            // After Theta
            "F2 25 64 D1 9A 6C 1F C7 1F 64 17 7B 3B 85 30 D1 " +
            "15 23 EF D8 28 21 7F 29 78 F3 A1 FC F9 E6 8E 90 " +
            "B1 22 5C 4D 8B FA B3 9E B3 16 AB 5D C1 24 A0 71 " +
            "26 D3 C5 BA F3 77 62 0B 72 AA E4 9B 77 02 00 E8 " +
            "55 1A 24 09 0F 3B 97 A3 F9 AD 8F C8 5D 39 41 31 " +
            "9E 78 98 DD 0C 95 9D 4C 1C ED 34 CE D7 DB E1 81 " +
            "F9 7F 43 2A 4E AC D6 6E 7C 0C 74 72 B3 CD 8C A9 " +
            "C0 E1 07 CD DB 45 1B E4 48 D3 06 B2 50 D9 DB 5F " +
            "AA 19 7C F1 F2 7B 17 E6 55 F6 7F A9 77 70 68 66 " +
            "F5 E2 2C ED 51 FA 85 A3 EA A7 60 0D BE AC 3B 40 " +
            "9B 41 EC AB B3 86 3D 9B EE DC BF 98 70 D6 07 B5 " +
            "41 05 78 1C D4 DE D4 5E 32 27 FC 1C 5E 26 01 82 " +
            "49 AB 01 81 98 91 65 0F",
            // After Rho
            "F2 25 64 D1 9A 6C 1F C7 3F C8 2E F6 76 0A 61 A2 " +
            "C5 C8 3B 36 4A C8 5F 4A 6F EE 08 89 37 1F CA 9F " +
            "D4 9F F5 8C 15 E1 6A 5A 15 4C 02 1A 37 6B B1 DA " +
            "AC 3B 7F 27 B6 60 32 5D BA 9C 2A F9 E6 9D 00 00 " +
            "0D 92 84 87 9D CB D1 2A 13 14 93 DF FA 88 DC 95 " +
            "F2 C4 C3 EC 66 A8 EC 64 07 72 B4 D3 38 5F 6F 87 " +
            "52 71 62 B5 76 CB FF 1B 9B 19 53 F9 18 E8 E4 66 " +
            "E6 ED A2 0D 72 E0 F0 83 64 A1 B2 B7 BF 90 A6 0D " +
            "2F 5E 7E EF C2 5C 35 83 34 B3 2A FB BF D4 3B 38 " +
            "BF 70 B4 5E 9C A5 3D 4A 40 EA A7 60 0D BE AC 3B " +
            "F6 6C 6E 06 B1 AF CE 1A BA 73 FF 62 C2 59 1F D4 " +
            "A8 00 8F 83 DA 9B DA 2B 27 FC 1C 5E 26 01 82 32 " +
            "D9 43 D2 6A 40 20 66 64",
            // After Pi
            "F2 25 64 D1 9A 6C 1F C7 AC 3B 7F 27 B6 60 32 5D " +
            "52 71 62 B5 76 CB FF 1B BF 70 B4 5E 9C A5 3D 4A " +
            "D9 43 D2 6A 40 20 66 64 6F EE 08 89 37 1F CA 9F " +
            "13 14 93 DF FA 88 DC 95 F2 C4 C3 EC 66 A8 EC 64 " +
            "2F 5E 7E EF C2 5C 35 83 A8 00 8F 83 DA 9B DA 2B " +
            "3F C8 2E F6 76 0A 61 A2 BA 9C 2A F9 E6 9D 00 00 " +
            "9B 19 53 F9 18 E8 E4 66 40 EA A7 60 0D BE AC 3B " +
            "F6 6C 6E 06 B1 AF CE 1A D4 9F F5 8C 15 E1 6A 5A " +
            "15 4C 02 1A 37 6B B1 DA 07 72 B4 D3 38 5F 6F 87 " +
            "34 B3 2A FB BF D4 3B 38 27 FC 1C 5E 26 01 82 32 " +
            "C5 C8 3B 36 4A C8 5F 4A 0D 92 84 87 9D CB D1 2A " +
            "E6 ED A2 0D 72 E0 F0 83 64 A1 B2 B7 BF 90 A6 0D " +
            "BA 73 FF 62 C2 59 1F D4",
            // After Chi
            "A0 65 64 41 DA E7 D2 C5 01 3B EB 6D 3E 44 32 1D " +
            "12 72 20 95 36 CB BD 3F 9D 54 90 CF 06 E9 24 C9 " +
            "D5 59 C9 4C 64 20 46 7C 8F 2E 48 A9 33 3F EA FF " +
            "1E 0E AF DC 7A DC CD 16 72 C4 42 EC 7E 2B 26 4C " +
            "68 B0 7E E7 E7 58 35 17 B8 10 1C D5 12 1B CE 2B " +
            "3E C9 7F F6 6E 6A 85 C4 FA 7E 8E F9 E3 8B 08 19 " +
            "2D 1D 1B FF A8 E9 A6 66 49 6A A7 90 4B BE 8D 9B " +
            "76 78 6E 0F 31 3A CE 1A D6 AD 41 4D 1D F5 24 5F " +
            "25 CD 08 32 B0 EB A1 E2 04 3E A0 D7 38 5E EF 85 " +
            "E4 B0 CB 7B AE 34 53 70 26 BC 1E 4C 04 0B 13 B2 " +
            "27 A5 19 3E 28 E8 7F CB 0D 92 94 35 10 DB D7 26 " +
            "7C BF EF 4D 32 A9 E9 53 21 29 B2 A3 B7 10 E6 07 " +
            "B2 61 7B E3 57 5A 9F F4",
            // After Iota 
            "28 65 64 41 DA E7 D2 C5 01 3B EB 6D 3E 44 32 1D " +
            "12 72 20 95 36 CB BD 3F 9D 54 90 CF 06 E9 24 C9 " +
            "D5 59 C9 4C 64 20 46 7C 8F 2E 48 A9 33 3F EA FF " +
            "1E 0E AF DC 7A DC CD 16 72 C4 42 EC 7E 2B 26 4C " +
            "68 B0 7E E7 E7 58 35 17 B8 10 1C D5 12 1B CE 2B " +
            "3E C9 7F F6 6E 6A 85 C4 FA 7E 8E F9 E3 8B 08 19 " +
            "2D 1D 1B FF A8 E9 A6 66 49 6A A7 90 4B BE 8D 9B " +
            "76 78 6E 0F 31 3A CE 1A D6 AD 41 4D 1D F5 24 5F " +
            "25 CD 08 32 B0 EB A1 E2 04 3E A0 D7 38 5E EF 85 " +
            "E4 B0 CB 7B AE 34 53 70 26 BC 1E 4C 04 0B 13 B2 " +
            "27 A5 19 3E 28 E8 7F CB 0D 92 94 35 10 DB D7 26 " +
            "7C BF EF 4D 32 A9 E9 53 21 29 B2 A3 B7 10 E6 07 " +
            "B2 61 7B E3 57 5A 9F F4",
            // Round #10
            // After Theta
            "3C A0 16 E6 C0 F1 1A 63 02 E5 8C 38 58 16 A3 F1 " +
            "2D 48 16 1A 57 BF 6E 8D B6 A7 1B A0 C4 B7 8B 1D " +
            "7C 5A EE F6 B3 54 A2 9B 9B EB 3A 0E 29 29 22 59 " +
            "1D D0 C8 89 1C 8E 5C FA 4D FE 74 63 1F 5F F5 FE " +
            "43 43 F5 88 25 06 9A C3 11 13 3B 6F C5 6F 2A CC " +
            "2A 0C 0D 51 74 7C 4D 62 F9 A0 E9 AC 85 D9 99 F5 " +
            "12 27 2D 70 C9 9D 75 D4 62 99 2C FF 89 E0 22 4F " +
            "DF 7B 49 B5 E6 4E 2A FD C2 68 33 EA 07 E3 EC F9 " +
            "26 13 6F 67 D6 B9 30 0E 3B 04 96 58 59 2A 3C 37 " +
            "CF 43 40 14 6C 6A FC A4 8F BF 39 F6 D3 7F F7 55 " +
            "33 60 6B 99 32 FE B7 6D 0E 4C F3 60 76 89 46 CA " +
            "43 85 D9 C2 53 DD 3A E1 0A DA 39 CC 75 4E 49 D3 " +
            "1B 62 5C 59 80 2E 7B 13",
            // After Rho
            "3C A0 16 E6 C0 F1 1A 63 05 CA 19 71 B0 2C 46 E3 " +
            "0B 92 85 C6 D5 AF 5B 63 7C BB D8 61 7B BA 01 4A " +
            "A5 12 DD E4 D3 72 B7 9F 90 92 22 92 B5 B9 AE E3 " +
            "9C C8 E1 C8 A5 DF 01 8D 7F 93 3F DD D8 C7 57 BD " +
            "A1 7A C4 12 03 CD E1 A1 A6 C2 1C 31 B1 F3 56 FC " +
            "53 61 68 88 A2 E3 6B 12 D6 E7 83 A6 B3 16 66 67 " +
            "81 4B EE AC A3 96 38 69 C1 45 9E C4 32 59 FE 13 " +
            "5A 73 27 95 FE EF BD A4 D4 0F C6 D9 F3 85 D1 66 " +
            "ED CC 3A 17 C6 C1 64 E2 9E 9B 1D 02 4B AC 2C 15 " +
            "8D 9F F4 79 08 88 82 4D 55 8F BF 39 F6 D3 7F F7 " +
            "DF B6 CD 80 AD 65 CA F8 3B 30 CD 83 D9 25 1A 29 " +
            "A8 30 5B 78 AA 5B 27 7C DA 39 CC 75 4E 49 D3 0A " +
            "DE C4 86 18 57 16 A0 CB",
            // After Pi
            "3C A0 16 E6 C0 F1 1A 63 9C C8 E1 C8 A5 DF 01 8D " +
            "81 4B EE AC A3 96 38 69 8D 9F F4 79 08 88 82 4D " +
            "DE C4 86 18 57 16 A0 CB 7C BB D8 61 7B BA 01 4A " +
            "A6 C2 1C 31 B1 F3 56 FC 53 61 68 88 A2 E3 6B 12 " +
            "ED CC 3A 17 C6 C1 64 E2 A8 30 5B 78 AA 5B 27 7C " +
            "05 CA 19 71 B0 2C 46 E3 7F 93 3F DD D8 C7 57 BD " +
            "C1 45 9E C4 32 59 FE 13 55 8F BF 39 F6 D3 7F F7 " +
            "DF B6 CD 80 AD 65 CA F8 A5 12 DD E4 D3 72 B7 9F " +
            "90 92 22 92 B5 B9 AE E3 D6 E7 83 A6 B3 16 66 67 " +
            "9E 9B 1D 02 4B AC 2C 15 DA 39 CC 75 4E 49 D3 0A " +
            "0B 92 85 C6 D5 AF 5B 63 A1 7A C4 12 03 CD E1 A1 " +
            "5A 73 27 95 FE EF BD A4 D4 0F C6 D9 F3 85 D1 66 " +
            "3B 30 CD 83 D9 25 1A 29",
            // After Chi
            "3D A3 18 C2 C2 F1 22 03 90 5C F1 99 AD D7 83 89 " +
            "D3 0B EC AC F4 80 18 EB AD BF E4 9F 88 69 98 6D " +
            "5E 8C 67 10 72 18 A1 47 2D 9A B8 E9 79 BA 28 48 " +
            "0A 4E 0E 26 F5 F3 52 1C 53 51 29 E0 8A F9 68 0E " +
            "B9 47 BA 16 97 61 64 E0 2A 70 5F 68 2A 1A 71 C8 " +
            "85 8E 99 71 92 34 EE E1 6B 19 1E E4 1C 45 56 59 " +
            "4B 75 DE 44 3B 7D 7E 1B 55 C7 AF 48 E6 DB 7B F4 " +
            "A5 A7 EB 0C E5 A6 DB E4 E3 77 5C C0 D1 74 F7 9B " +
            "98 8A 3E 92 FD 11 A6 F3 96 C7 43 D3 B7 57 B5 6D " +
            "BB 99 0C 82 DA 9E 08 80 CA B9 EE 67 6A C0 DB 6A " +
            "51 93 A6 43 29 8D 47 67 25 76 04 5A 02 CD A1 E3 " +
            "71 43 2E 97 F6 CF B7 AD D4 8D C6 9D F7 0F 90 24 " +
            "9B 58 8D 93 DB 65 BA A9",
            // After Iota 
            "34 23 18 42 C2 F1 22 03 90 5C F1 99 AD D7 83 89 " +
            "D3 0B EC AC F4 80 18 EB AD BF E4 9F 88 69 98 6D " +
            "5E 8C 67 10 72 18 A1 47 2D 9A B8 E9 79 BA 28 48 " +
            "0A 4E 0E 26 F5 F3 52 1C 53 51 29 E0 8A F9 68 0E " +
            "B9 47 BA 16 97 61 64 E0 2A 70 5F 68 2A 1A 71 C8 " +
            "85 8E 99 71 92 34 EE E1 6B 19 1E E4 1C 45 56 59 " +
            "4B 75 DE 44 3B 7D 7E 1B 55 C7 AF 48 E6 DB 7B F4 " +
            "A5 A7 EB 0C E5 A6 DB E4 E3 77 5C C0 D1 74 F7 9B " +
            "98 8A 3E 92 FD 11 A6 F3 96 C7 43 D3 B7 57 B5 6D " +
            "BB 99 0C 82 DA 9E 08 80 CA B9 EE 67 6A C0 DB 6A " +
            "51 93 A6 43 29 8D 47 67 25 76 04 5A 02 CD A1 E3 " +
            "71 43 2E 97 F6 CF B7 AD D4 8D C6 9D F7 0F 90 24 " +
            "9B 58 8D 93 DB 65 BA A9",
            // Round #11
            // After Theta
            "2D 77 1F E5 B9 8B 49 12 E6 D9 DF 58 74 69 CE A3 " +
            "C2 AA 41 83 E6 B8 A6 8D 80 61 F3 D2 95 F7 40 03 " +
            "2C 01 DB 7D 04 57 17 36 34 CE BF 4E 02 C0 43 59 " +
            "7C CB 20 E7 2C 4D 1F 36 42 F0 84 CF 98 C1 D6 68 " +
            "94 99 AD 5B 8A FF BC 8E 58 FD E3 05 5C 55 C7 B9 " +
            "9C DA 9E D6 E9 4E 85 F0 1D 9C 30 25 C5 FB 1B 73 " +
            "5A D4 73 6B 29 45 C0 7D 78 19 B8 05 FB 45 A3 9A " +
            "D7 2A 57 61 93 E9 6D 95 FA 23 5B 67 AA 0E 9C 8A " +
            "EE 0F 10 53 24 AF EB D9 87 66 EE FC A5 6F 0B 0B " +
            "96 47 1B CF C7 00 D0 EE B8 34 52 0A 1C 8F 6D 1B " +
            "48 C7 A1 E4 52 F7 2C 76 53 F3 2A 9B DB 73 EC C9 " +
            "60 E2 83 B8 E4 F7 09 CB F9 53 D1 D0 EA 91 48 4A " +
            "E9 D5 31 FE AD 2A 0C D8",
            // After Rho
            "2D 77 1F E5 B9 8B 49 12 CD B3 BF B1 E8 D2 9C 47 " +
            "B0 6A D0 A0 39 AE 69 A3 79 0F 34 00 18 36 2F 5D " +
            "B8 BA B0 61 09 D8 EE 23 24 00 3C 94 45 E3 FC EB " +
            "72 CE D2 F4 61 C3 B7 0C 9A 10 3C E1 33 66 B0 35 " +
            "CC D6 2D C5 7F 5E 47 CA 75 9C 8B D5 3F 5E C0 55 " +
            "E7 D4 F6 B4 4E 77 2A 84 CC 75 70 C2 94 14 EF 6F " +
            "5B 4B 29 02 EE D3 A2 9E 8B 46 35 F1 32 70 0B F6 " +
            "B0 C9 F4 B6 CA 6B 95 AB CE 54 1D 38 15 F5 47 B6 " +
            "62 8A E4 75 3D DB FD 01 85 85 43 33 77 FE D2 B7 " +
            "00 DA DD F2 68 E3 F9 18 1B B8 34 52 0A 1C 8F 6D " +
            "B3 D8 21 1D 87 92 4B DD 4F CD AB 6C 6E CF B1 27 " +
            "4C 7C 10 97 FC 3E 61 19 53 D1 D0 EA 91 48 4A F9 " +
            "03 76 7A 75 8C 7F AB 0A",
            // After Pi
            "2D 77 1F E5 B9 8B 49 12 72 CE D2 F4 61 C3 B7 0C " +
            "5B 4B 29 02 EE D3 A2 9E 00 DA DD F2 68 E3 F9 18 " +
            "03 76 7A 75 8C 7F AB 0A 79 0F 34 00 18 36 2F 5D " +
            "75 9C 8B D5 3F 5E C0 55 E7 D4 F6 B4 4E 77 2A 84 " +
            "62 8A E4 75 3D DB FD 01 4C 7C 10 97 FC 3E 61 19 " +
            "CD B3 BF B1 E8 D2 9C 47 9A 10 3C E1 33 66 B0 35 " +
            "8B 46 35 F1 32 70 0B F6 1B B8 34 52 0A 1C 8F 6D " +
            "B3 D8 21 1D 87 92 4B DD B8 BA B0 61 09 D8 EE 23 " +
            "24 00 3C 94 45 E3 FC EB CC 75 70 C2 94 14 EF 6F " +
            "85 85 43 33 77 FE D2 B7 53 D1 D0 EA 91 48 4A F9 " +
            "B0 6A D0 A0 39 AE 69 A3 CC D6 2D C5 7F 5E 47 CA " +
            "B0 C9 F4 B6 CA 6B 95 AB CE 54 1D 38 15 F5 47 B6 " +
            "4F CD AB 6C 6E CF B1 27",
            // After Chi
            "24 76 36 E7 37 9B 49 80 72 5E 06 04 61 E3 EE 0C " +
            "58 6F 0B 07 6A CF A0 9C 2C DB D8 72 59 63 B9 08 " +
            "51 FE BA 65 CC 3F 1D 06 FB 4F 40 20 58 17 05 DD " +
            "75 96 8B 94 0E D6 15 54 EB A0 E6 36 8E 53 2A 9C " +
            "53 89 C0 75 3D DB F3 45 48 EC 9B 42 DB 76 A1 19 " +
            "CC F5 BE A1 E8 C2 97 85 8A A8 3C E3 3B 6A 34 3C " +
            "2B 06 34 FC B7 F2 4B 66 57 9B AA F2 62 5C 1B 6F " +
            "A1 D8 21 5D 94 B6 6B ED 70 CF F0 23 99 CC ED 27 " +
            "25 80 3F A5 26 09 EC 7B 9E 25 E0 0A 14 14 E7 27 " +
            "2D AF 63 32 7F 6E 76 B5 57 D1 DC 7E D5 6B 5A 31 " +
            "80 63 00 92 B9 8F F9 82 82 C2 24 CD 6A CA 05 DE " +
            "B1 40 56 F2 A0 61 25 AA 7E 76 4D B8 04 D5 0F 36 " +
            "03 59 86 29 28 9F B7 6F",
            // After Iota 
            "2E 76 36 67 37 9B 49 80 72 5E 06 04 61 E3 EE 0C " +
            "58 6F 0B 07 6A CF A0 9C 2C DB D8 72 59 63 B9 08 " +
            "51 FE BA 65 CC 3F 1D 06 FB 4F 40 20 58 17 05 DD " +
            "75 96 8B 94 0E D6 15 54 EB A0 E6 36 8E 53 2A 9C " +
            "53 89 C0 75 3D DB F3 45 48 EC 9B 42 DB 76 A1 19 " +
            "CC F5 BE A1 E8 C2 97 85 8A A8 3C E3 3B 6A 34 3C " +
            "2B 06 34 FC B7 F2 4B 66 57 9B AA F2 62 5C 1B 6F " +
            "A1 D8 21 5D 94 B6 6B ED 70 CF F0 23 99 CC ED 27 " +
            "25 80 3F A5 26 09 EC 7B 9E 25 E0 0A 14 14 E7 27 " +
            "2D AF 63 32 7F 6E 76 B5 57 D1 DC 7E D5 6B 5A 31 " +
            "80 63 00 92 B9 8F F9 82 82 C2 24 CD 6A CA 05 DE " +
            "B1 40 56 F2 A0 61 25 AA 7E 76 4D B8 04 D5 0F 36 " +
            "03 59 86 29 28 9F B7 6F",
            // Round #12
            // After Theta
            "97 70 38 7D 79 A8 3E AE F4 67 E1 39 08 D9 27 A7 " +
            "85 6D 99 E3 88 ED D6 1F 42 F2 03 1D 42 6E CE BB " +
            "F8 2F 56 B4 FF 7B AB 5C 42 49 4E 3A 16 24 72 F3 " +
            "F3 AF 6C A9 67 EC DC FF 36 A2 74 D2 6C 71 5C 1F " +
            "3D A0 1B 1A 26 D6 84 F6 E1 3D 77 93 E8 32 17 43 " +
            "75 F3 B0 BB A6 F1 E0 AB 0C 91 DB DE 52 50 FD 97 " +
            "F6 04 A6 18 55 D0 3D E5 39 B2 71 9D 79 51 6C DC " +
            "08 09 CD 8C A7 F2 DD B7 C9 C9 FE 39 D7 FF 9A 09 " +
            "A3 B9 D8 98 4F 33 25 D0 43 27 72 EE F6 36 91 A4 " +
            "43 86 B8 5D 64 63 01 06 FE 00 30 AF E6 2F EC 6B " +
            "39 65 0E 88 F7 BC 8E AC 04 FB C3 F0 03 F0 CC 75 " +
            "6C 42 C4 16 42 43 53 29 10 5F 96 D7 1F D8 78 85 " +
            "AA 88 6A F8 1B DB 01 35",
            // After Rho
            "97 70 38 7D 79 A8 3E AE E9 CF C2 73 10 B2 4F 4E " +
            "61 5B E6 38 62 BB F5 47 E4 E6 BC 2B 24 3F D0 21 " +
            "DF 5B E5 C2 7F B1 A2 FD 63 41 22 37 2F 94 E4 A4 " +
            "96 7A C6 CE FD 3F FF CA 87 8D 28 9D 34 5B 1C D7 " +
            "D0 0D 0D 13 6B 42 FB 1E 73 31 14 DE 73 37 89 2E " +
            "AD 9B 87 DD 35 8D 07 5F 5F 32 44 6E 7B 4B 41 F5 " +
            "C5 A8 82 EE 29 B7 27 30 A2 D8 B8 73 64 E3 3A F3 " +
            "C6 53 F9 EE 5B 84 84 66 73 AE FF 35 13 92 93 FD " +
            "1B F3 69 A6 04 7A 34 17 48 D2 A1 13 39 77 7B 9B " +
            "2C C0 60 C8 10 B7 8B 6C 6B FE 00 30 AF E6 2F EC " +
            "3A B2 E6 94 39 20 DE F3 11 EC 0F C3 0F C0 33 D7 " +
            "4D 88 D8 42 68 68 2A 85 5F 96 D7 1F D8 78 85 10 " +
            "40 8D 2A A2 1A FE C6 76",
            // After Pi
            "97 70 38 7D 79 A8 3E AE 96 7A C6 CE FD 3F FF CA " +
            "C5 A8 82 EE 29 B7 27 30 2C C0 60 C8 10 B7 8B 6C " +
            "40 8D 2A A2 1A FE C6 76 E4 E6 BC 2B 24 3F D0 21 " +
            "73 31 14 DE 73 37 89 2E AD 9B 87 DD 35 8D 07 5F " +
            "1B F3 69 A6 04 7A 34 17 4D 88 D8 42 68 68 2A 85 " +
            "E9 CF C2 73 10 B2 4F 4E 87 8D 28 9D 34 5B 1C D7 " +
            "A2 D8 B8 73 64 E3 3A F3 6B FE 00 30 AF E6 2F EC " +
            "3A B2 E6 94 39 20 DE F3 DF 5B E5 C2 7F B1 A2 FD " +
            "63 41 22 37 2F 94 E4 A4 5F 32 44 6E 7B 4B 41 F5 " +
            "48 D2 A1 13 39 77 7B 9B 5F 96 D7 1F D8 78 85 10 " +
            "61 5B E6 38 62 BB F5 47 D0 0D 0D 13 6B 42 FB 1E " +
            "C6 53 F9 EE 5B 84 84 66 73 AE FF 35 13 92 93 FD " +
            "11 EC 0F C3 0F C0 33 D7",
            // After Chi
            "D6 F0 38 5D 79 28 3E 9E BE 3A A6 CE ED 3F 77 86 " +
            "85 A5 88 CC 23 FF 63 22 BB B0 70 95 71 B7 B3 E4 " +
            "40 87 EC 20 9E E9 07 36 68 6C 3F 2A 20 B7 D6 70 " +
            "61 51 7C FC 73 45 B9 2E E9 93 17 9D 5D 8D 0D DF " +
            "BB 95 4D 8F 00 6D E4 37 5E 99 D8 96 3B 68 23 8B " +
            "C9 9F 52 11 50 12 6D 6E CE AB 28 9D BF 5F 19 DB " +
            "B2 D8 5E F7 74 E3 EA E0 AA B3 00 53 AF 74 2E E0 " +
            "3C B2 CE 18 1D 69 CE 62 C3 69 A1 8A 2F FA A3 AC " +
            "63 81 83 26 2F A0 DE AE 48 36 12 62 BB 43 C5 F5 " +
            "C8 9B 81 D3 1E F6 59 76 7F 96 D5 2A D8 7C C1 10 " +
            "67 09 16 D4 72 3F F1 27 E1 A1 0B 02 6B 50 E8 87 " +
            "C6 13 F9 2C 57 C4 A4 64 13 BD 1F 0D 73 A9 57 FD " +
            "81 E8 06 C0 06 80 39 CF",
            // After Iota 
            "5D 70 38 DD 79 28 3E 9E BE 3A A6 CE ED 3F 77 86 " +
            "85 A5 88 CC 23 FF 63 22 BB B0 70 95 71 B7 B3 E4 " +
            "40 87 EC 20 9E E9 07 36 68 6C 3F 2A 20 B7 D6 70 " +
            "61 51 7C FC 73 45 B9 2E E9 93 17 9D 5D 8D 0D DF " +
            "BB 95 4D 8F 00 6D E4 37 5E 99 D8 96 3B 68 23 8B " +
            "C9 9F 52 11 50 12 6D 6E CE AB 28 9D BF 5F 19 DB " +
            "B2 D8 5E F7 74 E3 EA E0 AA B3 00 53 AF 74 2E E0 " +
            "3C B2 CE 18 1D 69 CE 62 C3 69 A1 8A 2F FA A3 AC " +
            "63 81 83 26 2F A0 DE AE 48 36 12 62 BB 43 C5 F5 " +
            "C8 9B 81 D3 1E F6 59 76 7F 96 D5 2A D8 7C C1 10 " +
            "67 09 16 D4 72 3F F1 27 E1 A1 0B 02 6B 50 E8 87 " +
            "C6 13 F9 2C 57 C4 A4 64 13 BD 1F 0D 73 A9 57 FD " +
            "81 E8 06 C0 06 80 39 CF",
            // Round #13
            // After Theta
            "A7 63 E4 8F D4 96 EF 2B 47 4F 11 A6 74 5A 6A 94 " +
            "F5 25 B5 68 21 C9 6D 08 53 DE 09 F5 5B 89 72 68 " +
            "81 F1 8A C6 84 88 DE 99 92 7F E3 78 8D 09 07 C5 " +
            "98 24 CB 94 EA 20 A4 3C 99 13 2A 39 5F BB 03 F5 " +
            "53 FB 34 EF 2A 53 25 BB 9F EF BE 70 21 09 FA 24 " +
            "33 8C 8E 43 FD AC BC DB 37 DE 9F F5 26 3A 04 C9 " +
            "C2 58 63 53 76 D5 E4 CA 42 DD 79 33 85 4A EF 6C " +
            "FD C4 A8 FE 07 08 17 CD 39 7A 7D D8 82 44 72 19 " +
            "9A F4 34 4E B6 C5 C3 BC 38 B6 2F C6 B9 75 CB DF " +
            "20 F5 F8 B3 34 C8 98 FA BE E0 B3 CC C2 1D 18 BF " +
            "9D 1A CA 86 DF 81 20 92 18 D4 BC 6A F2 35 F5 95 " +
            "B6 93 C4 88 55 F2 AA 4E FB D3 66 6D 59 97 96 71 " +
            "40 9E 60 26 1C E1 E0 60",
            // After Rho
            "A7 63 E4 8F D4 96 EF 2B 8F 9E 22 4C E9 B4 D4 28 " +
            "7D 49 2D 5A 48 72 1B 42 95 28 87 36 E5 9D 50 BF " +
            "44 F4 CE 0C 8C 57 34 26 D7 98 70 50 2C F9 37 8E " +
            "4C A9 0E 42 CA 83 49 B2 7D E6 84 4A CE D7 EE 40 " +
            "7D 9A 77 95 A9 92 DD A9 A0 4F F2 F9 EE 0B 17 92 " +
            "9E 61 74 1C EA 67 E5 DD 24 DF 78 7F D6 9B E8 10 " +
            "9B B2 AB 26 57 16 C6 1A 95 DE D9 84 BA F3 66 0A " +
            "FF 03 84 8B E6 7E 62 54 B0 05 89 E4 32 72 F4 FA " +
            "C6 C9 B6 78 98 57 93 9E E5 6F 1C DB 17 E3 DC BA " +
            "19 53 1F A4 1E 7F 96 06 BF BE E0 B3 CC C2 1D 18 " +
            "82 48 76 6A 28 1B 7E 07 62 50 F3 AA C9 D7 D4 57 " +
            "76 92 18 B1 4A 5E D5 C9 D3 66 6D 59 97 96 71 FB " +
            "38 18 90 27 98 09 47 38",
            // After Pi
            "A7 63 E4 8F D4 96 EF 2B 4C A9 0E 42 CA 83 49 B2 " +
            "9B B2 AB 26 57 16 C6 1A 19 53 1F A4 1E 7F 96 06 " +
            "38 18 90 27 98 09 47 38 95 28 87 36 E5 9D 50 BF " +
            "A0 4F F2 F9 EE 0B 17 92 9E 61 74 1C EA 67 E5 DD " +
            "C6 C9 B6 78 98 57 93 9E 76 92 18 B1 4A 5E D5 C9 " +
            "8F 9E 22 4C E9 B4 D4 28 7D E6 84 4A CE D7 EE 40 " +
            "95 DE D9 84 BA F3 66 0A BF BE E0 B3 CC C2 1D 18 " +
            "82 48 76 6A 28 1B 7E 07 44 F4 CE 0C 8C 57 34 26 " +
            "D7 98 70 50 2C F9 37 8E 24 DF 78 7F D6 9B E8 10 " +
            "E5 6F 1C DB 17 E3 DC BA D3 66 6D 59 97 96 71 FB " +
            "7D 49 2D 5A 48 72 1B 42 7D 9A 77 95 A9 92 DD A9 " +
            "FF 03 84 8B E6 7E 62 54 B0 05 89 E4 32 72 F4 FA " +
            "62 50 F3 AA C9 D7 D4 57",
            // After Chi
            "34 71 45 AB C1 82 69 23 4C E8 1A C2 C2 EA 59 B6 " +
            "BB BA 2B 25 D7 16 87 22 9E 30 7B 2C 5A E9 3E 05 " +
            "70 90 9A 67 92 08 47 A8 8B 08 83 32 E5 F9 B0 F2 " +
            "E0 C7 70 99 FE 1B 05 90 AE 73 7C 9D A8 6F A1 9C " +
            "47 E1 31 7E 3D D6 93 A8 56 D5 68 78 40 5C D2 C9 " +
            "0F 86 7B C8 D9 94 D4 22 57 C6 A4 79 8A D7 F7 50 " +
            "95 9E CF CC 9A EA 04 0D B2 28 E0 B7 0D 66 9D 30 " +
            "F2 28 F2 68 2E 58 54 47 64 B3 C6 23 5E 55 FC 36 " +
            "16 B8 74 D0 2D 99 23 24 36 DF 19 7F 56 8F C9 51 " +
            "E1 FF 9E DF 1F A2 D8 BE 40 6E 5D 09 B7 3E 72 73 " +
            "FF 48 AD 50 0E 1E 39 16 7D 9E 7E F1 B9 92 49 03 " +
            "BD 53 F6 81 2F FB 62 51 AD 0C 85 B4 32 52 FF FA " +
            "62 C2 A1 2F 68 57 10 FE",
            // After Iota 
            "BF 71 45 AB C1 82 69 A3 4C E8 1A C2 C2 EA 59 B6 " +
            "BB BA 2B 25 D7 16 87 22 9E 30 7B 2C 5A E9 3E 05 " +
            "70 90 9A 67 92 08 47 A8 8B 08 83 32 E5 F9 B0 F2 " +
            "E0 C7 70 99 FE 1B 05 90 AE 73 7C 9D A8 6F A1 9C " +
            "47 E1 31 7E 3D D6 93 A8 56 D5 68 78 40 5C D2 C9 " +
            "0F 86 7B C8 D9 94 D4 22 57 C6 A4 79 8A D7 F7 50 " +
            "95 9E CF CC 9A EA 04 0D B2 28 E0 B7 0D 66 9D 30 " +
            "F2 28 F2 68 2E 58 54 47 64 B3 C6 23 5E 55 FC 36 " +
            "16 B8 74 D0 2D 99 23 24 36 DF 19 7F 56 8F C9 51 " +
            "E1 FF 9E DF 1F A2 D8 BE 40 6E 5D 09 B7 3E 72 73 " +
            "FF 48 AD 50 0E 1E 39 16 7D 9E 7E F1 B9 92 49 03 " +
            "BD 53 F6 81 2F FB 62 51 AD 0C 85 B4 32 52 FF FA " +
            "62 C2 A1 2F 68 57 10 FE",
            // Round #14
            // After Theta
            "69 2F 30 FD A6 BD 48 AB FB 5A 23 F4 56 81 82 82 " +
            "64 61 8D 3B 7A 69 69 C1 78 68 F5 05 80 C4 F1 E1 " +
            "17 93 87 AC 8F E8 C1 D6 5D 56 F6 64 82 C6 91 FA " +
            "57 75 49 AF 6A 70 DE A4 71 A8 DA 83 05 10 4F 7F " +
            "A1 B9 BF 57 E7 FB 5C 4C 31 D6 75 B3 5D BC 54 B7 " +
            "D9 D8 0E 9E BE AB F5 2A E0 74 9D 4F 1E BC 2C 64 " +
            "4A 45 69 D2 37 95 EA EE 54 70 6E 9E D7 4B 52 D4 " +
            "95 2B EF A3 33 B8 D2 39 B2 ED B3 75 39 6A DD 3E " +
            "A1 0A 4D E6 B9 F2 F8 10 E9 04 BF 61 FB F0 27 B2 " +
            "07 A7 10 F6 C5 8F 17 5A 27 6D 40 C2 AA DE F4 0D " +
            "29 16 D8 06 69 21 18 1E CA 2C 47 C7 2D F9 92 37 " +
            "62 88 50 9F 82 84 8C B2 4B 54 0B 9D E8 7F 30 1E " +
            "05 C1 BC E4 75 B7 96 80",
            // After Rho
            "69 2F 30 FD A6 BD 48 AB F7 B5 46 E8 AD 02 05 05 " +
            "59 58 E3 8E 5E 5A 5A 30 48 1C 1F 8E 87 56 5F 00 " +
            "44 0F B6 BE 98 3C 64 7D 26 68 1C A9 DF 65 65 4F " +
            "F4 AA 06 E7 4D 7A 55 97 5F 1C AA F6 60 01 C4 D3 " +
            "DC DF AB F3 7D 2E A6 D0 4B 75 1B 63 5D 37 DB C5 " +
            "C9 C6 76 F0 F4 5D AD 57 90 81 D3 75 3E 79 F0 B2 " +
            "93 BE A9 54 77 57 2A 4A 97 A4 A8 A9 E0 DC 3C AF " +
            "D1 19 5C E9 9C CA 95 F7 EB 72 D4 BA 7D 64 DB 67 " +
            "C9 3C 57 1E 1F 22 54 A1 13 D9 74 82 DF B0 7D F8 " +
            "F1 42 EB E0 14 C2 BE F8 0D 27 6D 40 C2 AA DE F4 " +
            "60 78 A4 58 60 1B A4 85 28 B3 1C 1D B7 E4 4B DE " +
            "0C 11 EA 53 90 90 51 56 54 0B 9D E8 7F 30 1E 4B " +
            "25 60 41 30 2F 79 DD AD",
            // After Pi
            "69 2F 30 FD A6 BD 48 AB F4 AA 06 E7 4D 7A 55 97 " +
            "93 BE A9 54 77 57 2A 4A F1 42 EB E0 14 C2 BE F8 " +
            "25 60 41 30 2F 79 DD AD 48 1C 1F 8E 87 56 5F 00 " +
            "4B 75 1B 63 5D 37 DB C5 C9 C6 76 F0 F4 5D AD 57 " +
            "C9 3C 57 1E 1F 22 54 A1 0C 11 EA 53 90 90 51 56 " +
            "F7 B5 46 E8 AD 02 05 05 5F 1C AA F6 60 01 C4 D3 " +
            "97 A4 A8 A9 E0 DC 3C AF 0D 27 6D 40 C2 AA DE F4 " +
            "60 78 A4 58 60 1B A4 85 44 0F B6 BE 98 3C 64 7D " +
            "26 68 1C A9 DF 65 65 4F 90 81 D3 75 3E 79 F0 B2 " +
            "13 D9 74 82 DF B0 7D F8 54 0B 9D E8 7F 30 1E 4B " +
            "59 58 E3 8E 5E 5A 5A 30 DC DF AB F3 7D 2E A6 D0 " +
            "D1 19 5C E9 9C CA 95 F7 EB 72 D4 BA 7D 64 DB 67 " +
            "28 B3 1C 1D B7 E4 4B DE",
            // After Chi
            "6A 3B 99 ED 94 B8 62 E3 94 EA 44 47 4D FA C1 27 " +
            "97 9E A9 44 5C 6E 6B 4F B9 4D DB 2D 94 46 BE FA " +
            "B1 E0 47 32 66 3B C8 B9 C8 9E 7B 1E 27 1E 7B 12 " +
            "4B 4D 1A 6D 56 15 8B 65 CD C7 DE B1 74 CD AC 01 " +
            "89 30 42 92 18 64 5A A1 0F 70 EA 32 C8 B1 D1 93 " +
            "77 15 46 E1 2D DE 3D 29 57 1F EF B6 62 23 06 83 " +
            "F7 FC 28 B1 C0 CD 1C AE 9A A2 2F E0 4F AA DF F4 " +
            "68 70 0C 4E 20 1A 64 57 D4 8E 75 EA B8 24 F4 CD " +
            "25 30 38 2B 1E E5 68 07 D4 83 5A 1D 1E 79 F2 B1 " +
            "13 DD 56 94 5F BC 1D CC 76 6B 95 E9 38 71 1F 49 " +
            "58 58 B7 86 DE 9A 4B 17 F6 BD 2B E1 1C 0A EC D0 " +
            "D1 98 54 EC 1E 4A 95 6F BA 3A 37 38 35 7E CB 47 " +
            "AC 34 14 6C 96 C0 EF 1E",
            // After Iota 
            "E3 BB 99 ED 94 B8 62 63 94 EA 44 47 4D FA C1 27 " +
            "97 9E A9 44 5C 6E 6B 4F B9 4D DB 2D 94 46 BE FA " +
            "B1 E0 47 32 66 3B C8 B9 C8 9E 7B 1E 27 1E 7B 12 " +
            "4B 4D 1A 6D 56 15 8B 65 CD C7 DE B1 74 CD AC 01 " +
            "89 30 42 92 18 64 5A A1 0F 70 EA 32 C8 B1 D1 93 " +
            "77 15 46 E1 2D DE 3D 29 57 1F EF B6 62 23 06 83 " +
            "F7 FC 28 B1 C0 CD 1C AE 9A A2 2F E0 4F AA DF F4 " +
            "68 70 0C 4E 20 1A 64 57 D4 8E 75 EA B8 24 F4 CD " +
            "25 30 38 2B 1E E5 68 07 D4 83 5A 1D 1E 79 F2 B1 " +
            "13 DD 56 94 5F BC 1D CC 76 6B 95 E9 38 71 1F 49 " +
            "58 58 B7 86 DE 9A 4B 17 F6 BD 2B E1 1C 0A EC D0 " +
            "D1 98 54 EC 1E 4A 95 6F BA 3A 37 38 35 7E CB 47 " +
            "AC 34 14 6C 96 C0 EF 1E",
            // Round #15
            // After Theta
            "59 6E FD 8B 42 DF 7F 64 14 71 81 53 64 87 22 D8 " +
            "CA DB A5 F5 74 D8 79 10 09 8D CB 0E 3D 59 18 91 " +
            "13 15 5D 3D 3F FC 12 98 72 4B 1F 78 F1 79 66 15 " +
            "CB D6 DF 79 7F 68 68 9A 90 82 D2 00 5C 7B BE 5E " +
            "39 F0 52 B1 B1 7B FC CA AD 85 F0 3D 91 76 0B B2 " +
            "CD C0 22 87 FB B9 20 2E D7 84 2A A2 4B 5E E5 7C " +
            "AA B9 24 00 E8 7B 0E F1 2A 62 3F C3 E6 B5 79 9F " +
            "CA 85 16 41 79 DD BE 76 6E 5B 11 8C 6E 43 E9 CA " +
            "A5 AB FD 3F 37 98 8B F8 89 C6 56 AC 36 CF E0 EE " +
            "A3 1D 46 B7 F6 A3 BB A7 D4 9E 8F E6 61 B6 C5 68 " +
            "E2 8D D3 E0 08 FD 56 10 76 26 EE F5 35 77 0F 2F " +
            "8C DD 58 5D 36 FC 87 30 0A FA 27 1B 9C 61 6D 2C " +
            "0E C1 0E 63 CF 07 35 3F",
            // After Rho
            "59 6E FD 8B 42 DF 7F 64 29 E2 02 A7 C8 0E 45 B0 " +
            "F2 76 69 3D 1D 76 1E 84 93 85 11 99 D0 B8 EC D0 " +
            "E1 97 C0 9C A8 E8 EA F9 17 9F 67 56 21 B7 F4 81 " +
            "9D F7 87 86 A6 B9 6C FD 17 A4 A0 34 00 D7 9E AF " +
            "78 A9 D8 D8 3D 7E E5 1C B7 20 DB 5A 08 DF 13 69 " +
            "69 06 16 39 DC CF 05 71 F3 5D 13 AA 88 2E 79 95 " +
            "01 40 DF 73 88 57 CD 25 6B F3 3E 55 C4 7E 86 CD " +
            "A0 BC 6E 5F 3B E5 42 8B 18 DD 86 D2 95 DD B6 22 " +
            "FF E7 06 73 11 BF 74 B5 70 F7 44 63 2B 56 9B 67 " +
            "74 F7 74 B4 C3 E8 D6 7E 68 D4 9E 8F E6 61 B6 C5 " +
            "5B 41 88 37 4E 83 23 F4 D8 99 B8 D7 D7 DC 3D BC " +
            "B1 1B AB CB 86 FF 10 86 FA 27 1B 9C 61 6D 2C 0A " +
            "CD 8F 43 B0 C3 D8 F3 41",
            // After Pi
            "59 6E FD 8B 42 DF 7F 64 9D F7 87 86 A6 B9 6C FD " +
            "01 40 DF 73 88 57 CD 25 74 F7 74 B4 C3 E8 D6 7E " +
            "CD 8F 43 B0 C3 D8 F3 41 93 85 11 99 D0 B8 EC D0 " +
            "B7 20 DB 5A 08 DF 13 69 69 06 16 39 DC CF 05 71 " +
            "FF E7 06 73 11 BF 74 B5 B1 1B AB CB 86 FF 10 86 " +
            "29 E2 02 A7 C8 0E 45 B0 17 A4 A0 34 00 D7 9E AF " +
            "6B F3 3E 55 C4 7E 86 CD 68 D4 9E 8F E6 61 B6 C5 " +
            "5B 41 88 37 4E 83 23 F4 E1 97 C0 9C A8 E8 EA F9 " +
            "17 9F 67 56 21 B7 F4 81 F3 5D 13 AA 88 2E 79 95 " +
            "70 F7 44 63 2B 56 9B 67 FA 27 1B 9C 61 6D 2C 0A " +
            "F2 76 69 3D 1D 76 1E 84 78 A9 D8 D8 3D 7E E5 1C " +
            "A0 BC 6E 5F 3B E5 42 8B 18 DD 86 D2 95 DD B6 22 " +
            "D8 99 B8 D7 D7 DC 3D BC",
            // After Chi
            "59 6E A5 FA 4A 99 FE 64 E9 40 A7 02 E5 11 7E A7 " +
            "88 48 DC 73 88 47 EC 24 64 97 C8 BF C3 EF DA 5A " +
            "49 1E 41 B4 67 F8 F3 D8 DB 83 15 B8 04 B8 E8 C0 " +
            "21 C1 DB 18 09 EF 63 ED 69 1E BF B1 5A 8F 05 73 " +
            "FD 63 16 63 41 BF 98 E5 95 3B 61 89 8E B8 03 AF " +
            "41 B1 1C E6 0C 26 45 F0 17 A0 20 BE 22 D6 AE AF " +
            "78 F2 3E 65 CC FC 87 FD 48 76 9C 0F 66 6D F2 C5 " +
            "4D 45 28 27 4E 52 B9 FB 01 D7 D0 34 20 E0 E3 ED " +
            "17 3D 23 17 02 E7 76 E3 79 5D 08 36 C8 07 5D 9D " +
            "71 67 84 63 A3 D6 59 96 EC 2F 3C DE 60 7A 38 0A " +
            "72 62 4F 3A 1F F7 1C 07 60 E8 58 58 B9 66 51 3C " +
            "60 BC 56 5A 79 E5 4B 17 3A BB C7 FA 9D FF B4 22 " +
            "D0 10 28 17 F7 D4 DC A4",
            // After Iota 
            "5A EE A5 FA 4A 99 FE E4 E9 40 A7 02 E5 11 7E A7 " +
            "88 48 DC 73 88 47 EC 24 64 97 C8 BF C3 EF DA 5A " +
            "49 1E 41 B4 67 F8 F3 D8 DB 83 15 B8 04 B8 E8 C0 " +
            "21 C1 DB 18 09 EF 63 ED 69 1E BF B1 5A 8F 05 73 " +
            "FD 63 16 63 41 BF 98 E5 95 3B 61 89 8E B8 03 AF " +
            "41 B1 1C E6 0C 26 45 F0 17 A0 20 BE 22 D6 AE AF " +
            "78 F2 3E 65 CC FC 87 FD 48 76 9C 0F 66 6D F2 C5 " +
            "4D 45 28 27 4E 52 B9 FB 01 D7 D0 34 20 E0 E3 ED " +
            "17 3D 23 17 02 E7 76 E3 79 5D 08 36 C8 07 5D 9D " +
            "71 67 84 63 A3 D6 59 96 EC 2F 3C DE 60 7A 38 0A " +
            "72 62 4F 3A 1F F7 1C 07 60 E8 58 58 B9 66 51 3C " +
            "60 BC 56 5A 79 E5 4B 17 3A BB C7 FA 9D FF B4 22 " +
            "D0 10 28 17 F7 D4 DC A4",
            // Round #16
            // After Theta
            "A7 58 F6 FF 91 77 7A B3 5A A2 92 3E C7 AC 23 D9 " +
            "15 01 F9 0C 49 C7 C2 82 BE 6D F3 D2 0D 41 F9 3F " +
            "B5 93 26 AA 46 CC F6 6B 26 35 46 BD DF 56 6C 97 " +
            "92 23 EE 24 2B 52 3E 93 F4 57 9A CE 9B 0F 2B D5 " +
            "27 99 2D 0E 8F 11 BB 80 69 B6 06 97 AF 8C 06 1C " +
            "BC 07 4F E3 D7 C8 C1 A7 A4 42 15 82 00 6B F3 D1 " +
            "E5 BB 1B 1A 0D 7C A9 5B 92 8C A7 62 A8 C3 D1 A0 " +
            "B1 C8 4F 39 6F 66 BC 48 FC 61 83 31 FB 0E 67 BA " +
            "A4 DF 16 2B 20 5A 2B 9D E4 14 2D 49 09 87 73 3B " +
            "AB 9D BF 0E 6D 78 7A F3 10 A2 5B C0 41 4E 3D B9 " +
            "8F D4 1C 3F C4 19 98 50 D3 0A 6D 64 9B DB 0C 42 " +
            "FD F5 73 25 B8 65 65 B1 E0 41 FC 97 53 51 97 47 " +
            "2C 9D 4F 09 D6 E0 D9 17",
            // After Rho
            "A7 58 F6 FF 91 77 7A B3 B5 44 25 7D 8E 59 47 B2 " +
            "45 40 3E 43 D2 B1 B0 60 10 94 FF E3 DB 36 2F DD " +
            "62 B6 5F AB 9D 34 51 35 FB 6D C5 76 69 52 63 D4 " +
            "4E B2 22 E5 33 29 39 E2 35 FD 95 A6 F3 E6 C3 4A " +
            "CC 16 87 C7 88 5D C0 93 68 C0 91 66 6B 70 F9 CA " +
            "E5 3D 78 1A BF 46 0E 3E 47 93 0A 55 08 02 AC CD " +
            "D0 68 E0 4B DD 2A DF DD 87 A3 41 25 19 4F C5 50 " +
            "9C 37 33 5E A4 58 E4 A7 63 F6 1D CE 74 F9 C3 06 " +
            "62 05 44 6B A5 93 F4 DB B9 1D 72 8A 96 A4 84 C3 " +
            "4F 6F 7E B5 F3 D7 A1 0D B9 10 A2 5B C0 41 4E 3D " +
            "60 42 3D 52 73 FC 10 67 4D 2B B4 91 6D 6E 33 08 " +
            "BF 7E AE 04 B7 AC 2C B6 41 FC 97 53 51 97 47 E0 " +
            "F6 05 4B E7 53 82 35 78",
            // After Pi
            "A7 58 F6 FF 91 77 7A B3 4E B2 22 E5 33 29 39 E2 " +
            "D0 68 E0 4B DD 2A DF DD 4F 6F 7E B5 F3 D7 A1 0D " +
            "F6 05 4B E7 53 82 35 78 10 94 FF E3 DB 36 2F DD " +
            "68 C0 91 66 6B 70 F9 CA E5 3D 78 1A BF 46 0E 3E " +
            "62 05 44 6B A5 93 F4 DB BF 7E AE 04 B7 AC 2C B6 " +
            "B5 44 25 7D 8E 59 47 B2 35 FD 95 A6 F3 E6 C3 4A " +
            "87 A3 41 25 19 4F C5 50 B9 10 A2 5B C0 41 4E 3D " +
            "60 42 3D 52 73 FC 10 67 62 B6 5F AB 9D 34 51 35 " +
            "FB 6D C5 76 69 52 63 D4 47 93 0A 55 08 02 AC CD " +
            "B9 1D 72 8A 96 A4 84 C3 41 FC 97 53 51 97 47 E0 " +
            "45 40 3E 43 D2 B1 B0 60 CC 16 87 C7 88 5D C0 93 " +
            "9C 37 33 5E A4 58 E4 A7 63 F6 1D CE 74 F9 C3 06 " +
            "4D 2B B4 91 6D 6E 33 08",
            // After Chi
            "37 10 36 F5 5D 75 BC AE 41 B5 3C 51 11 FC 19 E2 " +
            "60 68 E1 09 DD 2A CB AD 4E 37 CA AD 73 A2 EB 8E " +
            "BE A7 4B E7 71 8A 34 38 95 A9 97 FB 4F 30 29 E9 " +
            "6A C0 95 07 6B E1 09 0B 78 47 D2 1E AD 6A 06 1A " +
            "62 85 15 88 ED 81 F7 92 D7 3E AE 00 97 EC FC B4 " +
            "37 46 65 7C 86 50 43 A2 0D ED 37 FC 33 E6 C9 67 " +
            "C7 E1 5C 25 2A F3 D5 12 2C 14 A2 76 4C 40 09 AD " +
            "60 FB AD D0 02 5A 90 2F 66 24 55 AA 9D 34 DD 3C " +
            "43 61 B5 FC FF F6 63 D6 07 73 8F 04 49 11 EF ED " +
            "9B 1F 3A 22 1A 84 94 D6 D8 B5 17 07 31 D5 65 20 " +
            "55 61 0E 5B F6 B1 94 44 AF D6 8B 47 D8 FC C3 93 " +
            "90 3E 93 4F AD 5E D4 AF 63 B6 17 8C E6 68 43 66 " +
            "C5 3D 35 15 65 22 73 9B",
            // After Iota 
            "35 90 36 F5 5D 75 BC 2E 41 B5 3C 51 11 FC 19 E2 " +
            "60 68 E1 09 DD 2A CB AD 4E 37 CA AD 73 A2 EB 8E " +
            "BE A7 4B E7 71 8A 34 38 95 A9 97 FB 4F 30 29 E9 " +
            "6A C0 95 07 6B E1 09 0B 78 47 D2 1E AD 6A 06 1A " +
            "62 85 15 88 ED 81 F7 92 D7 3E AE 00 97 EC FC B4 " +
            "37 46 65 7C 86 50 43 A2 0D ED 37 FC 33 E6 C9 67 " +
            "C7 E1 5C 25 2A F3 D5 12 2C 14 A2 76 4C 40 09 AD " +
            "60 FB AD D0 02 5A 90 2F 66 24 55 AA 9D 34 DD 3C " +
            "43 61 B5 FC FF F6 63 D6 07 73 8F 04 49 11 EF ED " +
            "9B 1F 3A 22 1A 84 94 D6 D8 B5 17 07 31 D5 65 20 " +
            "55 61 0E 5B F6 B1 94 44 AF D6 8B 47 D8 FC C3 93 " +
            "90 3E 93 4F AD 5E D4 AF 63 B6 17 8C E6 68 43 66 " +
            "C5 3D 35 15 65 22 73 9B",
            // Round #17
            // After Theta
            "B4 25 1C F3 31 5C 01 A0 74 89 44 20 92 95 C1 31 " +
            "5A 58 E1 E2 EE C5 37 65 2E 60 6C 9E AD C9 55 59 " +
            "0E DD 25 1D A0 24 C9 02 14 1C BD FD 23 19 94 67 " +
            "5F FC ED 76 E8 88 D1 D8 42 77 D2 F5 9E 85 FA D2 " +
            "02 D2 B3 BB 33 EA 49 45 67 44 C0 FA 46 42 01 8E " +
            "B6 F3 4F 7A EA 79 FE 2C 38 D1 4F 8D B0 8F 11 B4 " +
            "FD D1 5C CE 19 1C 29 DA 4C 43 04 45 92 2B B7 7A " +
            "D0 81 C3 2A D3 F4 6D 15 E7 91 7F AC F1 1D 60 B2 " +
            "76 5D CD 8D 7C 9F BB 05 3D 43 8F EF 7A FE 13 25 " +
            "FB 48 9C 11 C4 EF 2A 01 68 CF 79 FD E0 7B 98 1A " +
            "D4 D4 24 5D 9A 98 29 CA 9A EA F3 36 5B 95 1B 40 " +
            "AA 0E 93 A4 9E B1 28 67 03 E1 B1 BF 38 03 FD B1 " +
            "75 47 5B EF B4 8C 8E A1",
            // After Rho
            "B4 25 1C F3 31 5C 01 A0 E8 12 89 40 24 2B 83 63 " +
            "16 56 B8 B8 7B F1 4D 99 9A 5C 95 E5 02 C6 E6 D9 " +
            "25 49 16 70 E8 2E E9 00 3F 92 41 79 46 C1 D1 DB " +
            "6E 87 8E 18 8D FD C5 DF B4 D0 9D 74 BD 67 A1 BE " +
            "E9 D9 DD 19 F5 A4 22 01 14 E0 78 46 04 AC 6F 24 " +
            "B1 9D 7F D2 53 CF F3 67 D0 E2 44 3F 35 C2 3E 46 " +
            "72 CE E0 48 D1 EE 8F E6 57 6E F5 98 86 08 8A 24 " +
            "95 69 FA B6 0A E8 C0 61 58 E3 3B C0 64 CF 23 FF " +
            "B9 91 EF 73 B7 C0 AE AB 89 92 9E A1 C7 77 3D FF " +
            "5D 25 60 1F 89 33 82 F8 1A 68 CF 79 FD E0 7B 98 " +
            "A6 28 53 53 93 74 69 62 69 AA CF DB 6C 55 6E 00 " +
            "D5 61 92 D4 33 16 E5 4C E1 B1 BF 38 03 FD B1 03 " +
            "63 68 DD D1 D6 3B 2D A3",
            // After Pi
            "B4 25 1C F3 31 5C 01 A0 6E 87 8E 18 8D FD C5 DF " +
            "72 CE E0 48 D1 EE 8F E6 5D 25 60 1F 89 33 82 F8 " +
            "63 68 DD D1 D6 3B 2D A3 9A 5C 95 E5 02 C6 E6 D9 " +
            "14 E0 78 46 04 AC 6F 24 B1 9D 7F D2 53 CF F3 67 " +
            "B9 91 EF 73 B7 C0 AE AB D5 61 92 D4 33 16 E5 4C " +
            "E8 12 89 40 24 2B 83 63 B4 D0 9D 74 BD 67 A1 BE " +
            "57 6E F5 98 86 08 8A 24 1A 68 CF 79 FD E0 7B 98 " +
            "A6 28 53 53 93 74 69 62 25 49 16 70 E8 2E E9 00 " +
            "3F 92 41 79 46 C1 D1 DB D0 E2 44 3F 35 C2 3E 46 " +
            "89 92 9E A1 C7 77 3D FF E1 B1 BF 38 03 FD B1 03 " +
            "16 56 B8 B8 7B F1 4D 99 E9 D9 DD 19 F5 A4 22 01 " +
            "95 69 FA B6 0A E8 C0 61 58 E3 3B C0 64 CF 23 FF " +
            "69 AA CF DB 6C 55 6E 00",
            // After Chi
            "A4 6D 7C B3 61 5E 0B 80 63 A6 8E 0F 85 EC C5 C7 " +
            "50 86 7D 88 87 E6 A2 E5 C9 20 60 3D A8 77 82 F8 " +
            "29 EA 5F D9 5A 9A E9 FC 3B 41 92 75 51 85 76 9A " +
            "1C E0 F8 67 A0 AC 63 AC F5 FD 6F 56 53 D9 B2 23 " +
            "B3 8D EA 52 B7 00 AC 3A D1 C1 FA D6 37 3E EC 68 " +
            "AB 3C E9 C8 26 23 89 63 BC D0 97 15 C4 87 D0 26 " +
            "F3 6E E5 9A 84 1C 8A 46 52 7A 47 79 D9 EB F9 99 " +
            "B2 E8 47 67 0A 30 49 FE E5 29 12 76 D9 2C C7 04 " +
            "36 82 DB F9 84 F4 D0 62 B0 C3 65 27 35 4A BE 46 " +
            "8D DA 9E E1 2F 75 75 FF FB 23 FE 31 05 3C A1 D8 " +
            "02 76 9A 1E 71 B9 8D F9 A1 5B DC 59 91 A3 01 9F " +
            "B4 61 3E AD 02 F8 8C 61 4E B7 0B E0 77 6F 22 66 " +
            "80 23 8A DA E8 51 4C 00",
            // After Iota 
            "24 6D 7C B3 61 5E 0B 00 63 A6 8E 0F 85 EC C5 C7 " +
            "50 86 7D 88 87 E6 A2 E5 C9 20 60 3D A8 77 82 F8 " +
            "29 EA 5F D9 5A 9A E9 FC 3B 41 92 75 51 85 76 9A " +
            "1C E0 F8 67 A0 AC 63 AC F5 FD 6F 56 53 D9 B2 23 " +
            "B3 8D EA 52 B7 00 AC 3A D1 C1 FA D6 37 3E EC 68 " +
            "AB 3C E9 C8 26 23 89 63 BC D0 97 15 C4 87 D0 26 " +
            "F3 6E E5 9A 84 1C 8A 46 52 7A 47 79 D9 EB F9 99 " +
            "B2 E8 47 67 0A 30 49 FE E5 29 12 76 D9 2C C7 04 " +
            "36 82 DB F9 84 F4 D0 62 B0 C3 65 27 35 4A BE 46 " +
            "8D DA 9E E1 2F 75 75 FF FB 23 FE 31 05 3C A1 D8 " +
            "02 76 9A 1E 71 B9 8D F9 A1 5B DC 59 91 A3 01 9F " +
            "B4 61 3E AD 02 F8 8C 61 4E B7 0B E0 77 6F 22 66 " +
            "80 23 8A DA E8 51 4C 00",
            // Round #18
            // After Theta
            "BC 30 26 8B 02 86 E5 D3 95 87 58 F4 F4 A3 2A 8C " +
            "D3 BC 2A 7B 4F 7B 04 D0 F8 11 E1 F4 DA 15 69 3A " +
            "64 CE 19 03 B8 C7 15 37 A3 1C C8 4D 32 5D 98 49 " +
            "EA C1 2E 9C D1 E3 8C E7 76 C7 38 A5 9B 44 14 16 " +
            "82 BC 6B 9B C5 62 47 F8 9C E5 BC 0C D5 63 10 A3 " +
            "33 61 B3 F0 45 FB 67 B0 4A F1 41 EE B5 C8 3F 6D " +
            "70 54 B2 69 4C 81 2C 73 63 4B C6 B0 AB 89 12 5B " +
            "FF CC 01 BD E8 6D B5 35 7D 74 48 4E BA F4 29 D7 " +
            "C0 A3 0D 02 F5 BB 3F 29 33 F9 32 D4 FD D7 18 73 " +
            "BC EB 1F 28 5D 17 9E 3D B6 07 B8 EB E7 61 5D 13 " +
            "9A 2B C0 26 12 61 63 2A 57 7A 0A A2 E0 EC EE D4 " +
            "37 5B 69 5E CA 65 2A 54 7F 86 8A 29 05 0D C9 A4 " +
            "CD 07 CC 00 0A 0C B0 CB",
            // After Rho
            "BC 30 26 8B 02 86 E5 D3 2B 0F B1 E8 E9 47 55 18 " +
            "34 AF CA DE D3 1E 01 F4 5D 91 A6 83 1F 11 4E AF " +
            "3D AE B8 21 73 CE 18 C0 24 D3 85 99 34 CA 81 DC " +
            "C2 19 3D CE 78 AE 1E EC 85 DD 31 4E E9 26 11 85 " +
            "DE B5 CD 62 B1 23 7C 41 06 31 CA 59 CE CB 50 3D " +
            "9D 09 9B 85 2F DA 3F 83 B4 29 C5 07 B9 D7 22 FF " +
            "4D 63 0A 64 99 83 A3 92 13 25 B6 C6 96 8C 61 57 " +
            "5E F4 B6 DA 9A 7F E6 80 9C 74 E9 53 AE FB E8 90 " +
            "41 A0 7E F7 27 05 78 B4 8C B9 99 7C 19 EA FE 6B " +
            "C2 B3 87 77 FD 03 A5 EB 13 B6 07 B8 EB E7 61 5D " +
            "8D A9 68 AE 00 9B 48 84 5F E9 29 88 82 B3 BB 53 " +
            "66 2B CD 4B B9 4C 85 EA 86 8A 29 05 0D C9 A4 7F " +
            "EC 72 F3 01 33 80 02 03",
            // After Pi
            "BC 30 26 8B 02 86 E5 D3 C2 19 3D CE 78 AE 1E EC " +
            "4D 63 0A 64 99 83 A3 92 C2 B3 87 77 FD 03 A5 EB " +
            "EC 72 F3 01 33 80 02 03 5D 91 A6 83 1F 11 4E AF " +
            "06 31 CA 59 CE CB 50 3D 9D 09 9B 85 2F DA 3F 83 " +
            "41 A0 7E F7 27 05 78 B4 66 2B CD 4B B9 4C 85 EA " +
            "2B 0F B1 E8 E9 47 55 18 85 DD 31 4E E9 26 11 85 " +
            "13 25 B6 C6 96 8C 61 57 13 B6 07 B8 EB E7 61 5D " +
            "8D A9 68 AE 00 9B 48 84 3D AE B8 21 73 CE 18 C0 " +
            "24 D3 85 99 34 CA 81 DC B4 29 C5 07 B9 D7 22 FF " +
            "8C B9 99 7C 19 EA FE 6B 86 8A 29 05 0D C9 A4 7F " +
            "34 AF CA DE D3 1E 01 F4 DE B5 CD 62 B1 23 7C 41 " +
            "5E F4 B6 DA 9A 7F E6 80 9C 74 E9 53 AE FB E8 90 " +
            "5F E9 29 88 82 B3 BB 53",
            // After Chi
            "B1 52 24 AB 83 87 44 C1 40 89 B8 DD 1C AE 1A 85 " +
            "61 23 7A 64 9B 03 A1 92 D2 B3 83 FD FD 05 40 3B " +
            "AE 7B EA 45 4B A8 18 2F C4 99 B7 07 3E 01 61 2D " +
            "46 91 AE 2B CE CE 10 09 BB 02 1A 8D B7 92 BA C9 " +
            "58 30 5C 77 21 14 32 B1 64 0B 85 13 79 86 95 FA " +
            "39 2F 37 68 FF CF 35 4A 85 4F 30 76 80 45 11 8D " +
            "9F 2C DE C0 96 94 69 D7 31 B0 96 F8 02 A3 74 45 " +
            "09 79 68 A8 00 BB 48 01 AD 86 F8 27 FA DB 3A E3 " +
            "2C 43 9D E1 34 E2 5D DC B6 2B E5 06 BD D6 22 EB " +
            "B5 9D 09 5C 6B EC E6 EB 86 DB 2C 9D 09 C9 25 63 " +
            "34 EF F8 46 D9 42 83 74 5E B5 84 63 95 A3 74 51 " +
            "1D 7D B6 52 9A 7F F5 C3 BC 72 2B 05 FF F7 E8 34 " +
            "95 F9 2C A8 A2 92 C7 52",
            // After Iota 
            "BB D2 24 AB 83 87 44 C1 40 89 B8 DD 1C AE 1A 85 " +
            "61 23 7A 64 9B 03 A1 92 D2 B3 83 FD FD 05 40 3B " +
            "AE 7B EA 45 4B A8 18 2F C4 99 B7 07 3E 01 61 2D " +
            "46 91 AE 2B CE CE 10 09 BB 02 1A 8D B7 92 BA C9 " +
            "58 30 5C 77 21 14 32 B1 64 0B 85 13 79 86 95 FA " +
            "39 2F 37 68 FF CF 35 4A 85 4F 30 76 80 45 11 8D " +
            "9F 2C DE C0 96 94 69 D7 31 B0 96 F8 02 A3 74 45 " +
            "09 79 68 A8 00 BB 48 01 AD 86 F8 27 FA DB 3A E3 " +
            "2C 43 9D E1 34 E2 5D DC B6 2B E5 06 BD D6 22 EB " +
            "B5 9D 09 5C 6B EC E6 EB 86 DB 2C 9D 09 C9 25 63 " +
            "34 EF F8 46 D9 42 83 74 5E B5 84 63 95 A3 74 51 " +
            "1D 7D B6 52 9A 7F F5 C3 BC 72 2B 05 FF F7 E8 34 " +
            "95 F9 2C A8 A2 92 C7 52",
            // Round #19
            // After Theta
            "88 BA 5C 64 FC 80 07 3C 42 33 C6 83 47 27 F8 FD " +
            "F4 3B 92 30 FC 35 82 3E 9D BF 60 16 53 34 AA 55 " +
            "A2 BC C9 25 C2 A1 43 5C F7 F1 CF C8 41 06 22 D0 " +
            "44 2B D0 75 95 47 F2 71 2E 1A F2 D9 D0 A4 99 65 " +
            "17 3C BF 9C 8F 25 D8 DF 68 CC A6 73 F0 8F CE 89 " +
            "0A 47 4F A7 80 C8 76 B7 87 F5 4E 28 DB CC F3 F5 " +
            "0A 34 36 94 F1 A2 4A 7B 7E BC 75 13 AC 92 9E 2B " +
            "05 BE 4B C8 89 B2 13 72 9E EE 80 E8 85 DC 79 1E " +
            "2E F9 E3 BF 6F 6B BF A4 23 33 0D 52 DA E0 01 47 " +
            "FA 91 EA B7 C5 DD 0C 85 8A 1C 0F FD 80 C0 7E 10 " +
            "07 87 80 89 A6 45 C0 89 5C 0F FA 3D CE 2A 96 29 " +
            "88 65 5E 06 FD 49 D6 6F F3 7E C8 EE 51 C6 02 5A " +
            "99 3E 0F C8 2B 9B 9C 21",
            // After Rho
            "88 BA 5C 64 FC 80 07 3C 85 66 8C 07 8F 4E F0 FB " +
            "FD 8E 24 0C 7F 8D A0 0F 45 A3 5A D5 F9 0B 66 31 " +
            "0E 1D E2 12 E5 4D 2E 11 1C 64 20 02 7D 1F FF 8C " +
            "5D 57 79 24 1F 47 B4 02 99 8B 86 7C 36 34 69 66 " +
            "9E 5F CE C7 12 EC EF 0B E8 9C 88 C6 6C 3A 07 FF " +
            "55 38 7A 3A 05 44 B6 BB D7 1F D6 3B A1 6C 33 CF " +
            "A1 8C 17 55 DA 53 A0 B1 25 3D 57 FC 78 EB 26 58 " +
            "E4 44 D9 09 B9 02 DF 25 D1 0B B9 F3 3C 3C DD 01 " +
            "FC F7 6D ED 97 D4 25 7F 80 A3 91 99 06 29 6D F0 " +
            "9B A1 50 3F 52 FD B6 B8 10 8A 1C 0F FD 80 C0 7E " +
            "01 27 1E 1C 02 26 9A 16 70 3D E8 F7 38 AB 58 A6 " +
            "B1 CC CB A0 3F C9 FA 0D 7E C8 EE 51 C6 02 5A F3 " +
            "67 48 A6 CF 03 F2 CA 26",
            // After Pi
            "88 BA 5C 64 FC 80 07 3C 5D 57 79 24 1F 47 B4 02 " +
            "A1 8C 17 55 DA 53 A0 B1 9B A1 50 3F 52 FD B6 B8 " +
            "67 48 A6 CF 03 F2 CA 26 45 A3 5A D5 F9 0B 66 31 " +
            "E8 9C 88 C6 6C 3A 07 FF 55 38 7A 3A 05 44 B6 BB " +
            "FC F7 6D ED 97 D4 25 7F B1 CC CB A0 3F C9 FA 0D " +
            "85 66 8C 07 8F 4E F0 FB 99 8B 86 7C 36 34 69 66 " +
            "25 3D 57 FC 78 EB 26 58 10 8A 1C 0F FD 80 C0 7E " +
            "01 27 1E 1C 02 26 9A 16 0E 1D E2 12 E5 4D 2E 11 " +
            "1C 64 20 02 7D 1F FF 8C D7 1F D6 3B A1 6C 33 CF " +
            "80 A3 91 99 06 29 6D F0 7E C8 EE 51 C6 02 5A F3 " +
            "FD 8E 24 0C 7F 8D A0 0F 9E 5F CE C7 12 EC EF 0B " +
            "E4 44 D9 09 B9 02 DF 25 D1 0B B9 F3 3C 3C DD 01 " +
            "70 3D E8 F7 38 AB 58 A6",
            // After Chi
            "28 32 5A 35 3C 90 07 8D 47 76 39 0E 1F EB A2 0A " +
            "C5 C4 B1 95 DB 51 E8 B7 13 13 08 1F AE FD B3 A0 " +
            "32 0D 87 CF 00 B5 7A 24 50 83 28 ED F8 4F D6 31 " +
            "40 5B 8D 03 FE AA 06 BB 54 30 F8 3A 2D 4D 6C BB " +
            "B8 D4 7D B8 57 D6 21 4F 19 D0 4B A2 3B F9 FB C3 " +
            "A1 52 DD 87 C7 85 F6 E3 89 09 8E 7F B3 34 A9 40 " +
            "24 18 55 EC 7A CD 3C 58 94 CA 9C 0C 70 C8 A0 97 " +
            "19 AE 1C 64 32 16 93 12 CD 06 34 2B 65 2D 2E 52 " +
            "1C C4 21 82 7B 1E B3 BC A9 57 B8 7B 61 6E 21 CC " +
            "80 B6 91 9B 27 64 49 F0 6E A8 EE 51 DE 10 8B 7F " +
            "9D 8E 35 04 D6 8F B0 2B 8F 54 EE 35 16 D0 EF 0B " +
            "C4 70 99 0D B9 81 DF 83 5C 89 BD FB 7B 38 7D 08 " +
            "72 6C 22 34 38 CB 17 A6",
            // After Iota 
            "22 32 5A B5 3C 90 07 0D 47 76 39 0E 1F EB A2 0A " +
            "C5 C4 B1 95 DB 51 E8 B7 13 13 08 1F AE FD B3 A0 " +
            "32 0D 87 CF 00 B5 7A 24 50 83 28 ED F8 4F D6 31 " +
            "40 5B 8D 03 FE AA 06 BB 54 30 F8 3A 2D 4D 6C BB " +
            "B8 D4 7D B8 57 D6 21 4F 19 D0 4B A2 3B F9 FB C3 " +
            "A1 52 DD 87 C7 85 F6 E3 89 09 8E 7F B3 34 A9 40 " +
            "24 18 55 EC 7A CD 3C 58 94 CA 9C 0C 70 C8 A0 97 " +
            "19 AE 1C 64 32 16 93 12 CD 06 34 2B 65 2D 2E 52 " +
            "1C C4 21 82 7B 1E B3 BC A9 57 B8 7B 61 6E 21 CC " +
            "80 B6 91 9B 27 64 49 F0 6E A8 EE 51 DE 10 8B 7F " +
            "9D 8E 35 04 D6 8F B0 2B 8F 54 EE 35 16 D0 EF 0B " +
            "C4 70 99 0D B9 81 DF 83 5C 89 BD FB 7B 38 7D 08 " +
            "72 6C 22 34 38 CB 17 A6",
            // Round #20
            // After Theta
            "36 ED AD 52 AC 67 2A AD 74 8A EC 94 07 6F 97 9A " +
            "1F 15 CE C7 4F 95 B4 F1 97 B6 0C F2 24 C0 E8 E2 " +
            "D6 E8 1E E5 B4 FB 0F E9 44 5C DF 0A 68 B8 FB 91 " +
            "73 A7 58 99 E6 2E 33 2B 8E E1 87 68 B9 89 30 FD " +
            "3C 71 79 55 DD EB 7A 0D FD 35 D2 88 8F B7 8E 0E " +
            "B5 8D 2A 60 57 72 DB 43 BA F5 5B E5 AB B0 9C D0 " +
            "FE C9 2A BE EE 09 60 1E 10 6F 98 E1 FA F5 FB D5 " +
            "FD 4B 85 4E 86 58 E6 DF D9 D9 C3 CC F5 DA 03 F2 " +
            "2F 38 F4 18 63 9A 86 2C 73 86 C7 29 F5 AA 7D 8A " +
            "04 13 95 76 AD 59 12 B2 8A 4D 77 7B 6A 5E FE B2 " +
            "89 51 C2 E3 46 78 9D 8B BC A8 3B AF 0E 54 DA 9B " +
            "1E A1 E6 5F 2D 45 83 C5 D8 2C B9 16 F1 05 26 4A " +
            "96 89 BB 1E 8C 85 62 6B",
            // After Rho
            "36 ED AD 52 AC 67 2A AD E9 14 D9 29 0F DE 2E 35 " +
            "47 85 F3 F1 53 25 6D FC 02 8C 2E 7E 69 CB 20 4F " +
            "DD 7F 48 B7 46 F7 28 A7 80 86 BB 1F 49 C4 F5 AD " +
            "95 69 EE 32 B3 32 77 8A BF 63 F8 21 5A 6E 22 4C " +
            "B8 BC AA EE 75 BD 06 9E EB E8 D0 5F 23 8D F8 78 " +
            "AA 6D 54 01 BB 92 DB 1E 42 EB D6 6F 95 AF C2 72 " +
            "F1 75 4F 00 F3 F0 4F 56 EB F7 AB 21 DE 30 C3 F5 " +
            "27 43 2C F3 EF FE A5 42 99 EB B5 07 E4 B3 B3 87 " +
            "1E 63 4C D3 90 E5 05 87 3E C5 39 C3 E3 94 7A D5 " +
            "4B 42 96 60 A2 D2 AE 35 B2 8A 4D 77 7B 6A 5E FE " +
            "75 2E 26 46 09 8F 1B E1 F2 A2 EE BC 3A 50 69 6F " +
            "23 D4 FC AB A5 68 B0 D8 2C B9 16 F1 05 26 4A D8 " +
            "D8 9A 65 E2 AE 07 63 A1",
            // After Pi
            "36 ED AD 52 AC 67 2A AD 95 69 EE 32 B3 32 77 8A " +
            "F1 75 4F 00 F3 F0 4F 56 4B 42 96 60 A2 D2 AE 35 " +
            "D8 9A 65 E2 AE 07 63 A1 02 8C 2E 7E 69 CB 20 4F " +
            "EB E8 D0 5F 23 8D F8 78 AA 6D 54 01 BB 92 DB 1E " +
            "1E 63 4C D3 90 E5 05 87 23 D4 FC AB A5 68 B0 D8 " +
            "E9 14 D9 29 0F DE 2E 35 BF 63 F8 21 5A 6E 22 4C " +
            "EB F7 AB 21 DE 30 C3 F5 B2 8A 4D 77 7B 6A 5E FE " +
            "75 2E 26 46 09 8F 1B E1 DD 7F 48 B7 46 F7 28 A7 " +
            "80 86 BB 1F 49 C4 F5 AD 42 EB D6 6F 95 AF C2 72 " +
            "3E C5 39 C3 E3 94 7A D5 2C B9 16 F1 05 26 4A D8 " +
            "47 85 F3 F1 53 25 6D FC B8 BC AA EE 75 BD 06 9E " +
            "27 43 2C F3 EF FE A5 42 99 EB B5 07 E4 B3 B3 87 " +
            "F2 A2 EE BC 3A 50 69 6F",
            // After Chi
            "56 F9 AC 52 EC A7 22 F9 9F 6B 7E 52 B3 30 D7 AB " +
            "61 ED 2E 82 FF F5 0E D6 6D 27 1E 70 A2 B2 A6 39 " +
            "59 9A 27 C2 BD 17 36 A3 02 89 2A 7E F1 D9 23 49 " +
            "FF EA D8 8D 23 E8 FC F9 8B F9 E4 29 9E 9A 6B 46 " +
            "1E 6B 4E 87 D8 66 05 80 CA B4 2C AA A7 6C 68 E8 " +
            "A9 80 DA 29 8B CE EF 84 AF 6B BC 77 7B 24 3E 46 " +
            "AE D3 89 21 DE B5 C2 F4 3A 9A 94 5E 7D 3A 7A EA " +
            "63 4D 06 46 59 AF 1B A9 9F 16 0C D7 D2 DC 2A F5 " +
            "BC 82 92 9F 2B D4 CD 28 42 D3 D0 5F 91 8D C2 7A " +
            "EF 83 71 C5 A1 45 5A F2 2C 39 A5 F9 0C 26 9F D0 " +
            "40 C6 F7 E0 D9 67 CC BC 20 14 3B EA 75 BC 14 1B " +
            "45 43 66 4B F5 BE ED 2A 9C EE A4 46 A5 96 B7 17 " +
            "4A 9A E6 B2 1E C8 6B 6D",
            // After Iota 
            "D7 79 AC D2 EC A7 22 79 9F 6B 7E 52 B3 30 D7 AB " +
            "61 ED 2E 82 FF F5 0E D6 6D 27 1E 70 A2 B2 A6 39 " +
            "59 9A 27 C2 BD 17 36 A3 02 89 2A 7E F1 D9 23 49 " +
            "FF EA D8 8D 23 E8 FC F9 8B F9 E4 29 9E 9A 6B 46 " +
            "1E 6B 4E 87 D8 66 05 80 CA B4 2C AA A7 6C 68 E8 " +
            "A9 80 DA 29 8B CE EF 84 AF 6B BC 77 7B 24 3E 46 " +
            "AE D3 89 21 DE B5 C2 F4 3A 9A 94 5E 7D 3A 7A EA " +
            "63 4D 06 46 59 AF 1B A9 9F 16 0C D7 D2 DC 2A F5 " +
            "BC 82 92 9F 2B D4 CD 28 42 D3 D0 5F 91 8D C2 7A " +
            "EF 83 71 C5 A1 45 5A F2 2C 39 A5 F9 0C 26 9F D0 " +
            "40 C6 F7 E0 D9 67 CC BC 20 14 3B EA 75 BC 14 1B " +
            "45 43 66 4B F5 BE ED 2A 9C EE A4 46 A5 96 B7 17 " +
            "4A 9A E6 B2 1E C8 6B 6D",
            // Round #21
            // After Theta
            "E7 41 84 0C D6 B4 0A 69 BA 65 33 DD 99 E8 CE 3F " +
            "47 E7 BE 0B 4C 1B AA 9D 02 F1 76 24 DB 2F 4C B2 " +
            "24 60 79 8D 85 3D 12 EF 32 B1 02 A0 CB CA 0B 59 " +
            "DA E4 95 02 09 30 E5 6D AD F3 74 A0 2D 74 CF 0D " +
            "71 BD 26 D3 A1 FB EF 0B B7 4E 72 E5 9F 46 4C A4 " +
            "99 B8 F2 F7 B1 DD C7 94 8A 65 F1 F8 51 FC 27 D2 " +
            "88 D9 19 A8 6D 5B 66 BF 55 4C FC 0A 04 A7 90 61 " +
            "1E B7 58 09 61 85 3F E5 AF 2E 24 09 E8 CF 02 E5 " +
            "99 8C DF 10 01 0C D4 BC 64 D9 40 D6 22 63 66 31 " +
            "80 55 19 91 D8 D8 B0 79 51 C3 FB B6 34 0C BB 9C " +
            "70 FE DF 3E E3 74 E4 AC 05 1A 76 65 5F 64 0D 8F " +
            "63 49 F6 C2 46 50 49 61 F3 38 CC 12 DC 0B 5D 9C " +
            "37 60 B8 FD 26 E2 4F 21",
            // After Rho
            "E7 41 84 0C D6 B4 0A 69 74 CB 66 BA 33 D1 9D 7F " +
            "D1 B9 EF 02 D3 86 6A E7 FD C2 24 2B 10 6F 47 B2 " +
            "EC 91 78 27 01 CB 6B 2C BA AC BC 90 25 13 2B 00 " +
            "29 90 00 53 DE A6 4D 5E 43 EB 3C 1D 68 0B DD 73 " +
            "5E 93 E9 D0 FD F7 85 B8 C4 44 7A EB 24 57 FE 69 " +
            "CC C4 95 BF 8F ED 3E A6 48 2B 96 C5 E3 47 F1 9F " +
            "40 6D DB 32 FB 45 CC CE 4E 21 C3 AA 98 F8 15 08 " +
            "84 B0 C2 9F 72 8F 5B AC 12 D0 9F 05 CA 5F 5D 48 " +
            "1B 22 80 81 9A 37 93 F1 B3 18 B2 6C 20 6B 91 31 " +
            "1B 36 0F B0 2A 23 12 1B 9C 51 C3 FB B6 34 0C BB " +
            "91 B3 C2 F9 7F FB 8C D3 16 68 D8 95 7D 91 35 3C " +
            "2C C9 5E D8 08 2A 29 6C 38 CC 12 DC 0B 5D 9C F3 " +
            "53 C8 0D 18 6E BF 89 F8",
            // After Pi
            "E7 41 84 0C D6 B4 0A 69 29 90 00 53 DE A6 4D 5E " +
            "40 6D DB 32 FB 45 CC CE 1B 36 0F B0 2A 23 12 1B " +
            "53 C8 0D 18 6E BF 89 F8 FD C2 24 2B 10 6F 47 B2 " +
            "C4 44 7A EB 24 57 FE 69 CC C4 95 BF 8F ED 3E A6 " +
            "1B 22 80 81 9A 37 93 F1 2C C9 5E D8 08 2A 29 6C " +
            "74 CB 66 BA 33 D1 9D 7F 43 EB 3C 1D 68 0B DD 73 " +
            "4E 21 C3 AA 98 F8 15 08 9C 51 C3 FB B6 34 0C BB " +
            "91 B3 C2 F9 7F FB 8C D3 EC 91 78 27 01 CB 6B 2C " +
            "BA AC BC 90 25 13 2B 00 48 2B 96 C5 E3 47 F1 9F " +
            "B3 18 B2 6C 20 6B 91 31 38 CC 12 DC 0B 5D 9C F3 " +
            "D1 B9 EF 02 D3 86 6A E7 5E 93 E9 D0 FD F7 85 B8 " +
            "84 B0 C2 9F 72 8F 5B AC 12 D0 9F 05 CA 5F 5D 48 " +
            "16 68 D8 95 7D 91 35 3C",
            // After Chi
            "A7 2C 5F 2C F7 F5 8A E9 32 82 04 D3 DE 84 5F 4F " +
            "00 A5 DB 3A BF D9 45 2E BF 37 8F B4 BA 23 10 1A " +
            "5B 58 0D 4B 66 BD CC EE F5 42 A1 3F 9B C7 47 34 " +
            "D7 66 7A EB 34 45 7F 38 E8 0D CB E7 8F E5 16 AA " +
            "CA 20 A0 A2 8A 72 D5 63 2C CD 04 18 2C 3A 91 25 " +
            "78 CB A5 18 A3 21 9D 77 D3 BB 3C 4C 4E 0F D5 C0 " +
            "4F 83 C3 AA D1 33 95 48 F8 19 E7 F9 B6 34 1D 97 " +
            "92 93 DA FC 37 F1 CC D3 AC 92 7A 62 C3 8F BB B3 " +
            "09 BC 9C B8 25 3B 2B 20 40 EF 96 55 E8 53 FD 5D " +
            "77 09 DA 4F 20 E9 F2 3D 2A E0 96 4C 2F 4D 9C F3 " +
            "51 99 ED 0D D1 8E 30 E3 4C D3 F4 D0 75 A7 81 F8 " +
            "80 98 82 0F 47 0F 7B 98 D3 41 B8 07 48 59 17 8B " +
            "18 6A D8 45 51 E0 B0 24",
            // After Iota 
            "27 AC 5F 2C F7 F5 8A 69 32 82 04 D3 DE 84 5F 4F " +
            "00 A5 DB 3A BF D9 45 2E BF 37 8F B4 BA 23 10 1A " +
            "5B 58 0D 4B 66 BD CC EE F5 42 A1 3F 9B C7 47 34 " +
            "D7 66 7A EB 34 45 7F 38 E8 0D CB E7 8F E5 16 AA " +
            "CA 20 A0 A2 8A 72 D5 63 2C CD 04 18 2C 3A 91 25 " +
            "78 CB A5 18 A3 21 9D 77 D3 BB 3C 4C 4E 0F D5 C0 " +
            "4F 83 C3 AA D1 33 95 48 F8 19 E7 F9 B6 34 1D 97 " +
            "92 93 DA FC 37 F1 CC D3 AC 92 7A 62 C3 8F BB B3 " +
            "09 BC 9C B8 25 3B 2B 20 40 EF 96 55 E8 53 FD 5D " +
            "77 09 DA 4F 20 E9 F2 3D 2A E0 96 4C 2F 4D 9C F3 " +
            "51 99 ED 0D D1 8E 30 E3 4C D3 F4 D0 75 A7 81 F8 " +
            "80 98 82 0F 47 0F 7B 98 D3 41 B8 07 48 59 17 8B " +
            "18 6A D8 45 51 E0 B0 24",
            // Round #22
            // After Theta
            "16 40 96 B2 1C 8B 89 78 AB 14 46 EC 9F 30 04 27 " +
            "21 19 A5 69 96 20 61 F1 77 72 73 D4 F3 C6 2B 8C " +
            "DC 42 3F 25 32 4D 47 43 C4 AE 68 A1 70 B9 44 25 " +
            "4E F0 38 D4 75 F1 24 50 C9 B1 B5 B4 A6 1C 32 75 " +
            "02 65 5C C2 C3 97 EE F5 AB D7 36 76 78 CA 1A 88 " +
            "49 27 6C 86 48 5F 9E 66 4A 2D 7E 73 0F BB 8E A8 " +
            "6E 3F BD F9 F8 CA B1 97 30 5C 1B 99 FF D1 26 01 " +
            "15 89 E8 92 63 01 47 7E 9D 7E B3 FC 28 F1 B8 A2 " +
            "90 2A DE 87 64 8F 70 48 61 53 E8 06 C1 AA D9 82 " +
            "BF 4C 26 2F 69 0C C9 AB AD FA A4 22 7B BD 17 5E " +
            "60 75 24 93 3A F0 33 F2 D5 45 B6 EF 34 13 DA 90 " +
            "A1 24 FC 5C 6E F6 5F 47 1B 04 44 67 01 BC 2C 1D " +
            "9F 70 EA 2B 05 10 3B 89",
            // After Rho
            "16 40 96 B2 1C 8B 89 78 56 29 8C D8 3F 61 08 4E " +
            "48 46 69 9A 25 48 58 7C 6F BC C2 78 27 37 47 3D " +
            "69 3A 1A E2 16 FA 29 91 0A 97 4B 54 42 EC 8A 16 " +
            "43 5D 17 4F 02 E5 04 8F 5D 72 6C 2D AD 29 87 4C " +
            "32 2E E1 E1 4B F7 7A 81 AC 81 B8 7A 6D 63 87 A7 " +
            "4B 3A 61 33 44 FA F2 34 A2 2A B5 F8 CD 3D EC 3A " +
            "CD C7 57 8E BD 74 FB E9 A3 4D 02 60 B8 36 32 FF " +
            "C9 B1 80 23 BF 8A 44 74 F9 51 E2 71 45 3B FD 66 " +
            "FB 90 EC 11 0E 09 52 C5 6C C1 B0 29 74 83 60 D5 " +
            "21 79 F5 97 C9 E4 25 8D 5E AD FA A4 22 7B BD 17 " +
            "CF C8 83 D5 91 4C EA C0 56 17 D9 BE D3 4C 68 43 " +
            "94 84 9F CB CD FE EB 28 04 44 67 01 BC 2C 1D 1B " +
            "4E E2 27 9C FA 4A 01 C4",
            // After Pi
            "16 40 96 B2 1C 8B 89 78 43 5D 17 4F 02 E5 04 8F " +
            "CD C7 57 8E BD 74 FB E9 21 79 F5 97 C9 E4 25 8D " +
            "4E E2 27 9C FA 4A 01 C4 6F BC C2 78 27 37 47 3D " +
            "AC 81 B8 7A 6D 63 87 A7 4B 3A 61 33 44 FA F2 34 " +
            "FB 90 EC 11 0E 09 52 C5 94 84 9F CB CD FE EB 28 " +
            "56 29 8C D8 3F 61 08 4E 5D 72 6C 2D AD 29 87 4C " +
            "A3 4D 02 60 B8 36 32 FF 5E AD FA A4 22 7B BD 17 " +
            "CF C8 83 D5 91 4C EA C0 69 3A 1A E2 16 FA 29 91 " +
            "0A 97 4B 54 42 EC 8A 16 A2 2A B5 F8 CD 3D EC 3A " +
            "6C C1 B0 29 74 83 60 D5 04 44 67 01 BC 2C 1D 1B " +
            "48 46 69 9A 25 48 58 7C 32 2E E1 E1 4B F7 7A 81 " +
            "C9 B1 80 23 BF 8A 44 74 F9 51 E2 71 45 3B FD 66 " +
            "56 17 D9 BE D3 4C 68 43",
            // After Chi
            "9A C2 D6 32 A1 9B 72 18 63 65 B7 5E 42 65 00 8B " +
            "83 45 55 86 8F 7E FB A9 31 79 65 B5 CD 65 AD B5 " +
            "0F FF 26 D1 F8 2E 05 43 2C 86 83 79 27 AF 37 2D " +
            "1C 01 34 7A 67 62 87 66 4F 3E 72 F9 85 0C 5B 1C " +
            "90 A8 AC 21 2C 08 56 D0 14 85 A7 C9 85 BE 6B AA " +
            "F4 24 8E 98 2F 77 38 FD 01 D2 94 A9 AF 60 0A 4C " +
            "22 0D 03 31 29 32 70 3F 4E 8C F6 AC 0C 5A BD 19 " +
            "C6 9A E3 F0 11 44 6D C0 C9 12 AE 4A 9B EB 4D B9 " +
            "46 56 4B 55 72 6E 8A D3 A2 2E F2 F8 45 11 F1 30 " +
            "05 FB A8 CB 76 51 40 55 06 C1 26 15 FC 28 9F 1D " +
            "81 D7 69 98 91 40 5C 08 02 6E 83 B1 0B C6 C3 83 " +
            "CF B7 99 AD 2D CE 44 75 F1 11 C2 71 61 3B ED 5A " +
            "64 3F 59 DF 99 FB 4A C2",
            // After Iota 
            "9B C2 D6 B2 A1 9B 72 18 63 65 B7 5E 42 65 00 8B " +
            "83 45 55 86 8F 7E FB A9 31 79 65 B5 CD 65 AD B5 " +
            "0F FF 26 D1 F8 2E 05 43 2C 86 83 79 27 AF 37 2D " +
            "1C 01 34 7A 67 62 87 66 4F 3E 72 F9 85 0C 5B 1C " +
            "90 A8 AC 21 2C 08 56 D0 14 85 A7 C9 85 BE 6B AA " +
            "F4 24 8E 98 2F 77 38 FD 01 D2 94 A9 AF 60 0A 4C " +
            "22 0D 03 31 29 32 70 3F 4E 8C F6 AC 0C 5A BD 19 " +
            "C6 9A E3 F0 11 44 6D C0 C9 12 AE 4A 9B EB 4D B9 " +
            "46 56 4B 55 72 6E 8A D3 A2 2E F2 F8 45 11 F1 30 " +
            "05 FB A8 CB 76 51 40 55 06 C1 26 15 FC 28 9F 1D " +
            "81 D7 69 98 91 40 5C 08 02 6E 83 B1 0B C6 C3 83 " +
            "CF B7 99 AD 2D CE 44 75 F1 11 C2 71 61 3B ED 5A " +
            "64 3F 59 DF 99 FB 4A C2",
            // Round #23
            // After Theta
            "51 C0 74 43 4E 03 2D 0D 6F 1F 34 E9 77 B3 A7 6C " +
            "8F A5 21 EB 89 0A E9 BF CD AB 10 EA 94 F4 64 97 " +
            "02 02 4A 51 45 A2 37 C2 E6 84 21 88 C8 37 68 38 " +
            "10 7B B7 CD 52 B4 20 81 43 DE 06 94 83 78 49 0A " +
            "6C 7A D9 7E 75 99 9F F2 19 78 CB 49 38 32 59 2B " +
            "3E 26 2C 69 C0 EF 67 E8 0D A8 17 1E 9A B6 AD AB " +
            "2E ED 77 5C 2F 46 62 29 B2 5E 83 F3 55 CB 74 3B " +
            "CB 67 8F 70 AC C8 5F 41 03 10 0C BB 74 73 12 AC " +
            "4A 2C C8 E2 47 B8 2D 34 AE CE 86 95 43 65 E3 26 " +
            "F9 29 DD 94 2F C0 89 77 0B 3C 4A 95 41 A4 AD 9C " +
            "4B D5 CB 69 7E D8 03 1D 0E 14 00 06 3E 10 64 64 " +
            "C3 57 ED C0 2B BA 56 63 0D C3 B7 2E 38 AA 24 78 " +
            "69 C2 35 5F 24 77 78 43",
            // After Rho
            "51 C0 74 43 4E 03 2D 0D DE 3E 68 D2 EF 66 4F D9 " +
            "63 69 C8 7A A2 42 FA EF 49 4F 76 D9 BC 0A A1 4E " +
            "12 BD 11 16 10 50 8A 2A 88 7C 83 86 63 4E 18 82 " +
            "DB 2C 45 0B 12 08 B1 77 C2 90 B7 01 E5 20 5E 92 " +
            "BD 6C BF BA CC 4F 79 36 93 B5 92 81 B7 9C 84 23 " +
            "F7 31 61 49 03 7E 3F 43 AE 36 A0 5E 78 68 DA B6 " +
            "E3 7A 31 12 4B 71 69 BF 96 E9 76 64 BD 06 E7 AB " +
            "38 56 E4 AF A0 E5 B3 47 76 E9 E6 24 58 07 20 18 " +
            "59 FC 08 B7 85 46 89 05 71 13 57 67 C3 CA A1 B2 " +
            "38 F1 2E 3F A5 9B F2 05 9C 0B 3C 4A 95 41 A4 AD " +
            "0F 74 2C 55 2F A7 F9 61 39 50 00 18 F8 40 90 91 " +
            "F8 AA 1D 78 45 D7 6A 6C C3 B7 2E 38 AA 24 78 0D " +
            "DE 50 9A 70 CD 17 C9 1D",
            // After Pi
            "51 C0 74 43 4E 03 2D 0D DB 2C 45 0B 12 08 B1 77 " +
            "E3 7A 31 12 4B 71 69 BF 38 F1 2E 3F A5 9B F2 05 " +
            "DE 50 9A 70 CD 17 C9 1D 49 4F 76 D9 BC 0A A1 4E " +
            "93 B5 92 81 B7 9C 84 23 F7 31 61 49 03 7E 3F 43 " +
            "59 FC 08 B7 85 46 89 05 F8 AA 1D 78 45 D7 6A 6C " +
            "DE 3E 68 D2 EF 66 4F D9 C2 90 B7 01 E5 20 5E 92 " +
            "96 E9 76 64 BD 06 E7 AB 9C 0B 3C 4A 95 41 A4 AD " +
            "0F 74 2C 55 2F A7 F9 61 12 BD 11 16 10 50 8A 2A " +
            "88 7C 83 86 63 4E 18 82 AE 36 A0 5E 78 68 DA B6 " +
            "71 13 57 67 C3 CA A1 B2 C3 B7 2E 38 AA 24 78 0D " +
            "63 69 C8 7A A2 42 FA EF BD 6C BF BA CC 4F 79 36 " +
            "38 56 E4 AF A0 E5 B3 47 76 E9 E6 24 58 07 20 18 " +
            "39 50 00 18 F8 40 90 91",
            // After Chi
            "71 92 44 53 07 72 65 85 C3 AD 4B 26 B6 82 23 77 " +
            "25 7A A1 52 03 75 60 A7 39 71 4A 3C A7 9B D6 05 " +
            "54 7C 9B 78 DD 1F 59 6F 2D 4F 17 91 BC 68 9A 0E " +
            "9B 79 9A 37 33 9C 04 27 57 33 74 01 43 EF 5D 2B " +
            "58 B9 6A 36 3D 4E 08 07 6A 1A 9D 78 46 43 6E 4D " +
            "CA 57 28 B6 F7 60 EE F0 CA 92 BF 0B E5 61 5E 96 " +
            "95 9D 76 71 97 A0 BE EB 4C 01 7C C8 55 01 A2 35 " +
            "0F F4 BB 54 2F A7 E9 63 34 BF 31 4E 08 70 48 1E " +
            "D9 7D D4 A7 E0 CC 39 82 2C 92 88 46 50 4C 82 BB " +
            "61 1B 46 61 D3 9A 23 90 4B F7 AC B8 C9 2A 68 8D " +
            "63 7B 88 7F 82 E2 78 AE FB C5 BD BA 94 4D 79 2E " +
            "31 46 E4 B7 00 A5 23 C6 34 C0 2E 46 5A 05 4A 76 " +
            "A5 54 37 98 B4 4D 91 81",
            // After Iota 
            "79 12 44 D3 07 72 65 05 C3 AD 4B 26 B6 82 23 77 " +
            "25 7A A1 52 03 75 60 A7 39 71 4A 3C A7 9B D6 05 " +
            "54 7C 9B 78 DD 1F 59 6F 2D 4F 17 91 BC 68 9A 0E " +
            "9B 79 9A 37 33 9C 04 27 57 33 74 01 43 EF 5D 2B " +
            "58 B9 6A 36 3D 4E 08 07 6A 1A 9D 78 46 43 6E 4D " +
            "CA 57 28 B6 F7 60 EE F0 CA 92 BF 0B E5 61 5E 96 " +
            "95 9D 76 71 97 A0 BE EB 4C 01 7C C8 55 01 A2 35 " +
            "0F F4 BB 54 2F A7 E9 63 34 BF 31 4E 08 70 48 1E " +
            "D9 7D D4 A7 E0 CC 39 82 2C 92 88 46 50 4C 82 BB " +
            "61 1B 46 61 D3 9A 23 90 4B F7 AC B8 C9 2A 68 8D " +
            "63 7B 88 7F 82 E2 78 AE FB C5 BD BA 94 4D 79 2E " +
            "31 46 E4 B7 00 A5 23 C6 34 C0 2E 46 5A 05 4A 76 " +
            "A5 54 37 98 B4 4D 91 81"
        };
    }
}
