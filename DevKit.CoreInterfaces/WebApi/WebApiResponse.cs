namespace CoreInterfaces.WebApi;

/// <inheritdoc />
public class WebApiResponse<T> : IWebApiResponse<T>
{
    /// <inheritdoc />
    public object[] Metadata { get; set; }
    /// <inheritdoc />
    public T Data { get; set; }
    /// <inheritdoc />
    public bool Successful { get; set; }
    /// <inheritdoc />
    public string SuccessMessage { get; set; }
    /// <inheritdoc />
    public string ErrorMessage { get; set; }
}