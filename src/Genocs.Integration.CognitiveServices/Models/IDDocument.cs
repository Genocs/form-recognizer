
namespace Genocs.Integration.CognitiveServices.Models;


/// <summary>
/// 
/// </summary>
public class IdentityDocumentData
{
    /// <summary>
    /// DocumentType
    /// </summary>
    public string DocumentType { get; private set; } = default!;

    /// <summary>
    /// EmissionCountry
    /// </summary>
    public string EmissionCountry { get; private set; } = default!;

    /// <summary>
    /// PrimaryName
    /// </summary>
    public string PrimaryName { get; private set; } = default!;

    /// <summary>
    /// SecondaryName
    /// </summary>
    public string SecondaryName { get; private set; } = default!;

    /// <summary>
    /// DocumentNumber
    /// </summary>
    public string DocumentNumber { get; private set; } = default!;

    /// <summary>
    /// Nationality
    /// </summary>
    public string Nationality { get; private set; } = default!;

    /// <summary>
    /// DateOfBirth
    /// </summary>
    public string DateOfBirth { get; private set; } = default!;

    /// <summary>
    /// Sex
    /// </summary>
    public string Sex { get; private set; } = default!;

    /// <summary>
    /// ExpirationDate
    /// </summary>
    public string ExpirationDate { get; private set; } = default!;

    /// <summary>
    /// PersonalNumber
    /// </summary>
    public string? PersonalNumber { get; private set; } = default!;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="documentType"></param>
    /// <param name="emissionCountry"></param>
    /// <param name="primaryName"></param>
    /// <param name="secondaryName"></param>
    /// <param name="documentNumber"></param>
    /// <param name="nationality"></param>
    /// <param name="dateOfBirth"></param>
    /// <param name="sex"></param>
    /// <param name="expirationDate"></param>
    /// <param name="personalNumber"></param>
    public IdentityDocumentData(string documentType,
                                string emissionCountry,
                                string primaryName,
                                string secondaryName,
                                string documentNumber,
                                string nationality,
                                string dateOfBirth,
                                string sex,
                                string expirationDate,
                                string? personalNumber)
    {
        DocumentType = documentType;
        EmissionCountry = emissionCountry;
        PrimaryName = primaryName;
        SecondaryName = secondaryName;
        DocumentNumber = documentNumber;
        Nationality = nationality;
        DateOfBirth = dateOfBirth;
        Sex = sex;
        ExpirationDate = expirationDate;
        PersonalNumber = personalNumber;
    }
}


/// <summary>
/// Identity Document data
/// see ICAO 9303 Specification:
/// https://www.icao.int/publications/Documents/9303_p1_cons_en.pdf
/// https://www.icao.int/publications/Documents/9303_p2_cons_en.pdf
/// https://www.icao.int/publications/Documents/9303_p3_cons_en.pdf
/// </summary>
public class IDDocument : IdentityDocumentData
{

    //protected override IEnumerable<object> GetEqualityComponents()
    //{
    //    // Using a yield return statement to return each element one at a time
    //    yield return Id;
    //}

    /// <summary>
    /// CrcDocumentNumber
    /// </summary>
    public string CrcDocumentNumber { get; private set; }

    /// <summary>
    /// CrcDateOfBirth
    /// </summary>
    public string CrcDateOfBirth { get; private set; }

    /// <summary>
    /// CrcExpirationDate
    /// </summary>
    public string CrcExpirationDate { get; private set; }

    /// <summary>
    /// Crc
    /// </summary>
    public string Crc { get; private set; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="documentType"></param>
    /// <param name="emissionCountry"></param>
    /// <param name="primaryName"></param>
    /// <param name="secondaryName"></param>
    /// <param name="documentNumber"></param>
    /// <param name="crcDocumentNumber"></param>
    /// <param name="nationality"></param>
    /// <param name="dateOfBirth"></param>
    /// <param name="crcDateOfBirth"></param>
    /// <param name="sex"></param>
    /// <param name="expirationDate"></param>
    /// <param name="crcExpirationDate"></param>
    /// <param name="personalNumber"></param>
    /// <param name="crc"></param>
    public IDDocument(string documentType,
                        string emissionCountry,
                        string primaryName,
                        string secondaryName,
                        string documentNumber,
                        string crcDocumentNumber,
                        string nationality,
                        string dateOfBirth,
                        string crcDateOfBirth,
                        string sex,
                        string expirationDate,
                        string crcExpirationDate,
                        string? personalNumber,
                        string crc) : base(documentType,
                                            emissionCountry,
                                            primaryName,
                                            secondaryName,
                                            documentNumber,
                                            nationality,
                                            dateOfBirth,
                                            sex,
                                            expirationDate,
                                            personalNumber)
    {
        CrcDocumentNumber = crcDocumentNumber;
        CrcDateOfBirth = crcDateOfBirth;

        CrcExpirationDate = crcExpirationDate;
        Crc = crc;
    }
}
