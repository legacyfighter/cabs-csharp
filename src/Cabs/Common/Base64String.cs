using System.Text;

namespace LegacyFighter.Cabs.Common;

/// <summary>
/// Note to Legacy Fighter participants:
///
/// The "canonical" C# way of checking whether a string is base64
/// is by using the Convert.TryFromBase64String API, but it proved
/// incompatible with the algorithm used in the Java original and caused
/// test failures, so I instead ported the Java version over.
/// </summary>
public static class Base64String
{
  private static readonly int[] DecodeTable =
  {
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, 62, -1, 63, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
    -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
    24, 25, -1, -1, -1, -1, 63, -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46,
    47, 48, 49, 50, 51
  };

  public static bool IsBase64(this string str) 
    => IsBase64(Encoding.UTF8.GetBytes(str));

  private static bool IsBase64(byte octet) 
    => octet == 61 || octet < DecodeTable.Length && DecodeTable[octet] != -1;

  private static bool IsBase64(byte[] arrayOctet) 
    => arrayOctet.All(octet => IsBase64(octet) || IsWhiteSpace(octet));

  private static bool IsWhiteSpace(byte byteToCheck) =>
    byteToCheck switch
    {
      9 => true,
      10 => true,
      13 => true,
      32 => true,
      _ => false
    };
}