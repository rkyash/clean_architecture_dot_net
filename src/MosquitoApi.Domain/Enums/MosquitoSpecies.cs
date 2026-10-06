namespace MosquitoApi.Domain.Enums;

/// <summary>
/// Enumeration of common mosquito species
/// </summary>
public enum MosquitoSpecies
{
    /// <summary>
    /// Aedes aegypti - Yellow fever mosquito
    /// </summary>
    AedesAegypti = 1,

    /// <summary>
    /// Aedes albopictus - Asian tiger mosquito
    /// </summary>
    AedesAlbopictus = 2,

    /// <summary>
    /// Anopheles gambiae - Principal malaria vector in Africa
    /// </summary>
    AnophelesGambiae = 3,

    /// <summary>
    /// Anopheles stephensi - Asian malaria mosquito
    /// </summary>
    AnophelesStephensi = 4,

    /// <summary>
    /// Culex pipiens - Common house mosquito
    /// </summary>
    CulexPipiens = 5,

    /// <summary>
    /// Culex quinquefasciatus - Southern house mosquito
    /// </summary>
    CulexQuinquefasciatus = 6,

    /// <summary>
    /// Unknown or unidentified species
    /// </summary>
    Unknown = 999
}