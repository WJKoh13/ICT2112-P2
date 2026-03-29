namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class RouteResolutionException : Exception
{
    public RouteResolutionException(string message)
        : base(message)
    {
    }

    public RouteResolutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
