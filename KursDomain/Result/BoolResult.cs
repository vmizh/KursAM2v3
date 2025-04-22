namespace KursDomain.Result;

public class BoolResult : IBoolResult
{
    public string ErrorText { get; set; }
    public bool Result { get; set; }
}
