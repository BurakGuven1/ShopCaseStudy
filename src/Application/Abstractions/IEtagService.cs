namespace Application.Abstractions;

public interface IEtagService
{
    string ComputeForProductsList(string filterKey, DateTimeOffset? latestUpdated, int totalCount);
}
