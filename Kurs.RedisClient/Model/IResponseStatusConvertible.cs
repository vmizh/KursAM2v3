namespace ServiceStack.Model;

//Allow Exceptions to Customize ResponseStatus returned
public interface IResponseStatusConvertible
{
    ResponseStatus ToResponseStatus();
}
