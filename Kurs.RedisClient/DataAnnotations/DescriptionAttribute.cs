namespace ServiceStack.DataAnnotations;

/// <summary>
/// Annotate any Type, Property or Enum with a textual description
/// </summary>
public class DescriptionAttribute : AttributeBase
{
    public string Description { get; set; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}
