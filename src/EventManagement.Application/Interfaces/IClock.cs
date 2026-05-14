namespace EventManagement.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
