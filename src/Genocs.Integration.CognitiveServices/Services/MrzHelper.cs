using Genocs.Integration.CognitiveServices.Models;
using System.Globalization;

namespace Genocs.Integration.CognitiveServices.Services;


/// <summary>
/// 
/// </summary>
public static class MrzHelper
{
    private const string VALID_CHARACTERS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DATE_FORMAT = "yyMMdd";

    private const int TD1_LENGTH = 30;
    private const int TD3_LENGTH = 44;

    private static readonly int[] CharacterWeight = new[] { 7, 3, 1 };
    private static string? mrz;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="string"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetCheckDigit(string @string)
    {
        int digit = 0;

        @string = @string.Replace('<', '0');

        for (var i = 0; i < @string.Length; i++)
        {
            var @char = @string[i];

            var charIndex = VALID_CHARACTERS.IndexOf(@char);
            if (charIndex < 0)
            {
                throw new ArgumentException($"{nameof(MrzHelper)}:{nameof(GetCheckDigit)} {nameof(@string)} contains invalid characters (i.e. {@char})");
            }

            digit += charIndex * CharacterWeight[i % 3];
        }

        return Convert.ToString(digit % 10);
    }

    public static MRZCheckResult CheckMRZ(string mrz, string documentNumber, DateTime? dateOfBirth, DateTime? expirationDate, string lastName, string firstName)
    {
        if (string.IsNullOrWhiteSpace(mrz))
        {
            return MRZCheckResult.Unknown;
        }

        if (!dateOfBirth.HasValue)
        {
            return MRZCheckResult.Unknown;
        }

        if (!expirationDate.HasValue)
        {
            return MRZCheckResult.Unknown;
        }

        char firstChar = mrz[0];

        // Split multiline
        string[] lines = mrz.Split(
            new[] { "\r\n", "\r", "\n", "\\n", " " },
            StringSplitOptions.None
        );

        string composedString = string.Join("", lines);

        switch (composedString.Length)
        {
            case 90:
                return IsValidTD1MRZ(composedString, documentNumber, dateOfBirth.Value, expirationDate.Value, lastName, firstName);
            case 88:
                switch (firstChar)
                {
                    case 'P':
                        return IsValidTD3MRZ(composedString, documentNumber, dateOfBirth.Value, expirationDate.Value, lastName, firstName);
                    case 'V':
                        return IsValidMRVAMRZ(composedString, documentNumber, dateOfBirth.Value, expirationDate.Value, lastName, firstName);
                }
                break;
            case 72:
                switch (firstChar)
                {
                    case 'A':
                    case 'C':
                    case 'I':
                        return IsValidTD2MRZ(composedString, documentNumber, dateOfBirth.Value, expirationDate.Value, lastName, firstName);
                    case 'V':
                        return IsValidMRVBMRZ(composedString, documentNumber, dateOfBirth.Value, expirationDate.Value, lastName, firstName);
                }
                break;
        }

        return MRZCheckResult.Unknown;
    }


    private static MRZCheckResult IsValidTD1MRZ(string mrz, string documentNumber, DateTime dateOfBirth, DateTime expirationDate, string lastName, string firstName)
    {
        string[] lines = GetLines(mrz, TD1_LENGTH).ToArray();
        if (lines.Length != 3)
        {
            return MRZCheckResult.Lenght;
        }

        string documentNumberHash = GetCheckDigit(documentNumber);

        string mrzDocumentNumberHash = lines[0].Substring(14, 1);

        if (documentNumberHash != mrzDocumentNumberHash)
        {
            return MRZCheckResult.DocumentNumber;
        }

        string dateOfBirthHash = GetCheckDigit(dateOfBirth.ToString(DATE_FORMAT));

        string mrzDateOfBirthHash = lines[1].Substring(6, 1);

        if (dateOfBirthHash != mrzDateOfBirthHash)
        {
            return MRZCheckResult.DateOfBirth;
        }

        string expirationDateHash = GetCheckDigit(expirationDate.ToString(DATE_FORMAT));

        string mrzExpirationDateHash = lines[1].Substring(14, 1);

        if (expirationDateHash != mrzExpirationDateHash)
        {
            return MRZCheckResult.ExpirationDate;
        }

        string overallCheckDigitString = documentNumber + documentNumberHash + lines[0].Substring(15) + dateOfBirth.ToString(DATE_FORMAT) + dateOfBirthHash + expirationDate.ToString(DATE_FORMAT) + expirationDateHash + lines[1].Substring(18, 11);

        string overallCheckDigit = GetCheckDigit(overallCheckDigitString);

        string mrzOverallCheckDigit = lines[1].Substring(29);

        if (overallCheckDigit != mrzOverallCheckDigit)
        {
            return MRZCheckResult.CRC;
        }

        var documentNames = GetDocumentNames(lines[2]);
        if (!string.Equals(documentNames.lastName, lastName, StringComparison.OrdinalIgnoreCase) || !string.Equals(documentNames.firstName, firstName, StringComparison.OrdinalIgnoreCase))
        {
            return MRZCheckResult.Names;
        }

        return MRZCheckResult.OK;
    }

    private static MRZCheckResult IsValidTD2MRZ(string mrz, string documentNumber, DateTime dateOfBirth, DateTime expirationDate, string lastName, string firstName)
    {
        var lines = GetLines(mrz, 36).ToArray();
        if (lines.Length != 2)
        {
            return MRZCheckResult.Lenght;
        }

        string documentNumberHash = GetCheckDigit(documentNumber);

        string mrzDocumentNumberHash = lines[1].Substring(9, 1);

        if (documentNumberHash != mrzDocumentNumberHash)
        {
            return MRZCheckResult.DocumentNumber;
        }

        string dateOfBirthHash = GetCheckDigit(dateOfBirth.ToString(DATE_FORMAT));

        string mrzDateOfBirthHash = lines[1].Substring(19, 1);

        if (dateOfBirthHash != mrzDateOfBirthHash)
        {
            return MRZCheckResult.DateOfBirth;
        }

        string expirationDateHash = GetCheckDigit(expirationDate.ToString(DATE_FORMAT));

        string mrzExpirationDateHash = lines[1].Substring(27, 1);

        if (expirationDateHash != mrzExpirationDateHash)
        {
            return MRZCheckResult.ExpirationDate;
        }

        string overallCheckDigitString = lines[1].Substring(0, 10) + lines[1].Substring(13, 7) + lines[1].Substring(21, 14);

        string overallCheckDigit = GetCheckDigit(overallCheckDigitString);

        string mrzOverallCheckDigit = lines[1].Substring(35);

        if (overallCheckDigit != mrzOverallCheckDigit)
        {
            return MRZCheckResult.CRC;
        }

        var documentNames = GetDocumentNames(lines[0].Substring(5));
        if (!string.Equals(documentNames.lastName, lastName, StringComparison.OrdinalIgnoreCase) || !string.Equals(documentNames.firstName, firstName, StringComparison.OrdinalIgnoreCase))
        {
            return MRZCheckResult.Names;
        }

        return MRZCheckResult.OK;
    }

    private static MRZCheckResult IsValidTD3MRZ(string mrz, string documentNumber, DateTime dateOfBirth, DateTime expirationDate, string lastName, string firstName)
    {
        string[] lines = GetLines(mrz, 44).ToArray();
        if (lines.Length != 2)
        {
            return MRZCheckResult.Lenght;
        }

        string documentNumberHash = GetCheckDigit(documentNumber);

        string mrzDocumentNumberHash = lines[1].Substring(9, 1);

        if (documentNumberHash != mrzDocumentNumberHash)
        {
            return MRZCheckResult.DocumentNumber;
        }

        string dateOfBirthHash = GetCheckDigit(dateOfBirth.ToString(DATE_FORMAT));

        string mrzDateOfBirthHash = lines[1].Substring(19, 1);

        if (dateOfBirthHash != mrzDateOfBirthHash)
        {
            return MRZCheckResult.DateOfBirth;
        }

        string expirationDateHash = GetCheckDigit(expirationDate.ToString(DATE_FORMAT));

        string mrzExpirationDateHash = lines[1].Substring(27, 1);

        if (expirationDateHash != mrzExpirationDateHash)
        {
            return MRZCheckResult.ExpirationDate;
        }

        string overallCheckDigitString = lines[1].Substring(0, 10) + lines[1].Substring(13, 7) + lines[1].Substring(21, 22);

        string overallCheckDigit = GetCheckDigit(overallCheckDigitString);

        string mrzOverallCheckDigit = lines[1].Substring(43);

        if (overallCheckDigit != mrzOverallCheckDigit)
        {
            return MRZCheckResult.CRC;
        }

        var documentNames = GetDocumentNames(lines[0].Substring(5));
        if (!string.Equals(documentNames.lastName, lastName, StringComparison.OrdinalIgnoreCase) || !string.Equals(documentNames.firstName, firstName, StringComparison.OrdinalIgnoreCase))
        {
            return MRZCheckResult.Names;
        }

        return MRZCheckResult.OK;
    }

    private static MRZCheckResult IsValidMRVAMRZ(string mrz, string documentNumber, DateTime dateOfBirth, DateTime expirationDate, string lastName, string firstName)
    {
        var lines = GetLines(mrz, 44).ToArray();
        if (lines.Length != 2)
        {
            return MRZCheckResult.Lenght;
        }

        string documentNumberHash = GetCheckDigit(documentNumber);

        string mrzDocumentNumberHash = lines[1].Substring(9, 1);

        if (documentNumberHash != mrzDocumentNumberHash)
        {
            return MRZCheckResult.DocumentNumber;
        }

        string dateOfBirthHash = GetCheckDigit(dateOfBirth.ToString(DATE_FORMAT));

        string mrzDateOfBirthHash = lines[1].Substring(19, 1);

        if (dateOfBirthHash != mrzDateOfBirthHash)
        {
            return MRZCheckResult.DateOfBirth;
        }

        string expirationDateHash = GetCheckDigit(expirationDate.ToString(DATE_FORMAT));

        string mrzExpirationDateHash = lines[1].Substring(27, 1);

        if (expirationDateHash != mrzExpirationDateHash)
        {
            return MRZCheckResult.ExpirationDate;
        }

        var documentNames = GetDocumentNames(lines[0].Substring(5));
        if (!string.Equals(documentNames.lastName, lastName, StringComparison.OrdinalIgnoreCase) || !string.Equals(documentNames.firstName, firstName, StringComparison.OrdinalIgnoreCase))
        {
            return MRZCheckResult.Names;
        }

        return MRZCheckResult.OK;
    }

    private static MRZCheckResult IsValidMRVBMRZ(string mrz, string documentNumber, DateTime dateOfBirth, DateTime expirationDate, string lastName, string firstName)
    {
        var lines = GetLines(mrz, 36).ToArray();
        if (lines.Length != 2)
        {
            return MRZCheckResult.Lenght;
        }

        string documentNumberHash = GetCheckDigit(documentNumber);

        string mrzDocumentNumberHash = lines[1].Substring(9, 1);

        if (documentNumberHash != mrzDocumentNumberHash)
        {
            return MRZCheckResult.DocumentNumber;
        }

        string dateOfBirthHash = GetCheckDigit(dateOfBirth.ToString(DATE_FORMAT));

        string mrzDateOfBirthHash = lines[1].Substring(19, 1);

        if (dateOfBirthHash != mrzDateOfBirthHash)
        {
            return MRZCheckResult.DateOfBirth;
        }

        string expirationDateHash = GetCheckDigit(expirationDate.ToString(DATE_FORMAT));

        string mrzExpirationDateHash = lines[1].Substring(27, 1);

        if (expirationDateHash != mrzExpirationDateHash)
        {
            return MRZCheckResult.ExpirationDate;
        }

        var documentNames = GetDocumentNames(lines[0].Substring(5));
        if (!string.Equals(documentNames.lastName, lastName, StringComparison.OrdinalIgnoreCase) || !string.Equals(documentNames.firstName, firstName, StringComparison.OrdinalIgnoreCase))
        {
            return MRZCheckResult.Names;
        }

        return MRZCheckResult.OK;
    }

    public static IEnumerable<string> GetLines(string @string, int chunkSize)
    {
        if (string.IsNullOrEmpty(@string))
        {
            throw new ArgumentException($"{nameof(MrzHelper)}:{nameof(GetLines)} {nameof(@string)} cannot be null or empty.");
        }

        if (chunkSize <= 0 || chunkSize > @string.Length)
        {
            throw new ArgumentOutOfRangeException($"{nameof(MrzHelper)}:{nameof(GetLines)} {nameof(chunkSize)} cannot be less or equal to zero or greater than {nameof(@string)} length.");
        }

        for (var i = 0; i < @string.Length; i += chunkSize)
        {
            yield return @string.Substring(i, chunkSize);
        }
    }

    /// <summary>
    /// Extract both 'primary' and 'secondary' name. Primary name is the surname, Seconary name is a list of names 
    /// </summary>
    /// <param name="mrz"></param>
    /// <returns></returns>

    public static (string lastName, string firstName) GetDocumentNames(string @string)
    {
        if (string.IsNullOrWhiteSpace(@string))
        {
            return (string.Empty, string.Empty);
        }

        string trimNames = @string.TrimEnd('<');
        bool containsPrimaryAndSecondary = trimNames.Contains("<<");

        string primaryName = string.Empty;
        string secondaryName = string.Empty;

        if (containsPrimaryAndSecondary)
        {
            int splitChar = trimNames.IndexOf("<<");
            primaryName = trimNames.Substring(0, splitChar).Replace('<', ' ').Trim();
            secondaryName = trimNames.Substring(splitChar + 2).Replace('<', ' ').Trim();
        }
        else
        {
            primaryName = trimNames.Replace('<', ' ').Trim();
        }

        return (primaryName, secondaryName);
    }

    public static MRZType GetMRZType(string mrz)
    {
        mrz = RecomposeString(mrz);

        if (string.IsNullOrWhiteSpace(mrz))
        {
            return MRZType.UNKNOWN;
        }

        // Check TD3
        // Two lines of 44 chars each
        if (mrz.Length == TD3_LENGTH * 2)
        {
            return MRZType.TD3;
        }

        // Check TD1
        // Three lines of 30 chars each
        if (mrz.Length == TD1_LENGTH * 3)
        {
            return MRZType.TD1;
        }

        return MRZType.UNKNOWN;
    }

    /// <summary>
    /// Extract clear data from MachineReadableZone string
    /// </summary>
    /// <param name="mrz">The MachineReadableZone data</param>
    /// <returns>The extracted data</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IDDocument? ExtractMRZ(string mrz)
    {
        mrz = RemoveWhitespace(mrz);
        mrz = RecomposeString(mrz);
        if (string.IsNullOrWhiteSpace(mrz))
        {
            return null;
        }

        string[]? lines = null;
        if (mrz.Length == 88)
        {
            lines = new string[2];
            lines[0] = mrz.Substring(0, 44);
            lines[1] = mrz.Substring(44, 44);
        }
        else
        {
            // Split line
            lines = mrz.Split(' ');
        }

        // TD3 check 
        if (lines.Length != 2)
        {
            throw new InvalidOperationException("Invalid TD3 lines count");
        }

        foreach (var line in lines)
        {
            if (line.Length != 44)
            {
                throw new InvalidOperationException("Invalid TD3 line lenght");
            }
        }

        // Preliminary checks
        bool nameCouldBeTruncated = lines[0][43] != '<';

        string documentType = lines[0].Substring(0, 2).Replace('<', ' ').Trim();
        string emissionCountry = lines[0].Substring(2, 3).Replace('<', ' ').Trim();

        string trimNames = lines[0].Substring(5).TrimEnd('<');
        bool containsPrimaryAndSecondary = trimNames.Contains("<<");

        string primaryName = string.Empty;
        string secondaryName = string.Empty;

        if (containsPrimaryAndSecondary)
        {
            int splitChar = trimNames.IndexOf("<<");
            primaryName = trimNames.Substring(0, splitChar).Replace('<', ' ').Trim();
            secondaryName = trimNames.Substring(splitChar + 2).Replace('<', ' ').Trim();
        }
        else
        {
            primaryName = trimNames.Replace('<', ' ').Trim();
        }

        string documentNumber = lines[1].Substring(0, 9).Replace('<', ' ').Trim();
        string crcDocumentNumber = lines[1].Substring(9, 1).Replace('<', ' ').Trim();
        string nationality = lines[1].Substring(10, 3).Replace('<', ' ').Trim();

        string dateBirth = lines[1].Substring(13, 6).Replace('<', ' ').Trim();
        string crcDateOfBirth = lines[1].Substring(19, 1).Replace('<', ' ').Trim();
        string sex = lines[1].Substring(20, 1).Replace('<', ' ').Trim();
        string expirationDate = lines[1].Substring(21, 6).Replace('<', ' ').Trim();
        string crcExpirationDate = lines[1].Substring(27, 1).Replace('<', ' ').Trim();
        string personalNumber = lines[1].Substring(28, 14).Replace('<', ' ').Trim();
        string crc = lines[1].Substring(42, 2).Replace('<', ' ').Trim();

        return new IDDocument(documentType,
                                emissionCountry,
                                primaryName,
                                secondaryName,
                                documentNumber,
                                crcDocumentNumber,
                                nationality,
                                dateBirth,
                                crcDateOfBirth,
                                sex,
                                expirationDate,
                                crcExpirationDate,
                                personalNumber,
                                crc);

    }

    public static bool ContainsPrimaryAndSecondary(IEnumerable<string> names)
    {
        bool first = !string.IsNullOrEmpty(names.First());
        bool middle = false;

        foreach (var n in names.Skip(1))
        {
            if (!string.IsNullOrEmpty(n))
            {
                if (first && middle) return true;
            }
            else
            {
                middle = true;
            }

        }
        return false;
    }

    public static DateTime FromMRZString(string dateTime)
    {
        return DateTime.ParseExact(dateTime, "yyMMdd", CultureInfo.InvariantCulture);
    }

    public static string RecomposeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Split multiline
        string[] lines = input.Split(
            new[] { "\r\n", "\r", "\n", "\\n", " " },
            StringSplitOptions.None
        );

        return string.Join("", lines);
    }

    /// <summary>
    ///  Move to Core
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }
}

public enum MRZCheckResult
{
    Unknown = -1,
    OK = 0,
    Lenght = 1,
    DocumentNumber = 2,
    DateOfBirth = 3,
    ExpirationDate = 4,
    CRC = 5,
    Names = 6
}

public enum MRZType
{
    UNKNOWN,
    TD1,
    TD3
}