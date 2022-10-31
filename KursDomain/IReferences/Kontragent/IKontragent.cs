using System;

namespace KursDomain.IReferences.Kontragent;

/// <summary>
///     Контрагент
/// </summary>
public interface IKontragent
{
    string ShortName { get; set; }
    string FullName { get; set; }
    IKontragentCategory Category { set; get; }
    string INN { get; set; }
    string KPP { get; set; }
    string Director { get; set; }
    string GlavBuh { get; set; }
    bool IsDeleted { get; set; }
    string Address { get; set; }
    string Phone { get; set; }
    string OKPO { get; set; }
    string OKONH { get; set; }
    bool IsPersona { get; set; }
    string Passport { get; set; }
    IEmployee Employee { get; set; }

    IClientCategory ClientCategory { get; set; }

    bool IsBalans { get; set; }
    ICurrency Currency { set; get; }
    DateTime StartBalans { get; set; }
    decimal StartSumma { get; set; }
    string EMail { get; set; }

    IEmployee ResponsibleTabelNumber { get; set; }
    IRegion Region { get; set; }
}
