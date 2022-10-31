using System.Net.Mime;

namespace KursDomain.IReferences;

/// <summary>
/// Страны
/// </summary>
public interface ICountry
{
    string Alpha2 { get; set; }
    string Alpha3 { get; set; }
    string ISO { set; get; }
    string ForeignName { get; set; }
    string RussianName { get; set; }
    string Location { set; get; }
    string LocationPrecise { set; get; } 
    byte[] Flag { get; set; }
}
