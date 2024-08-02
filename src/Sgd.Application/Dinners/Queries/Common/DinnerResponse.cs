using Sgd.Domain.DinnerAggregate;
using Sgd.Domain.UserAggregate;

namespace Sgd.Application.Dinners.Queries.Common;

public sealed class DinnerResponse
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime Date { get; init; }
    public int Capacity { get; init; }
    public string SignUpMethod { get; init; }
    public DateTime? RandomSelectionTime { get; init; }
    public IReadOnlyList<UserResponse> Hosts { get; init; }
    public IReadOnlyList<SignUpResponse> SignUps { get; init; }
    public IReadOnlyList<SignUpResponse> WaitList { get; init; }

    public static DinnerResponse FromDomain(Dinner dinner, IEnumerable<User> users)
    {
        var userDictionary = users.ToDictionary(user => user.Id.ToString());

        return new DinnerResponse
        {
            Id = dinner.Id.ToString(),
            Name = dinner.Name,
            Description = dinner.Description,
            ImageUrl = dinner.ImageUrl,
            Date = dinner.Date,
            Capacity = dinner.Capacity,
            SignUpMethod = dinner.SignUpMethod.GetType().Name,
            RandomSelectionTime = dinner.RandomSelectionTime,
            Hosts = dinner
                .Hosts.Select(hostId => UserResponse.FromDomain(userDictionary[hostId.ToString()]))
                .ToList()
                .AsReadOnly(),
            SignUps = dinner
                .SignUps.Select(signUp =>
                    SignUpResponse.FromDomain(signUp, userDictionary[signUp.UserId.ToString()])
                )
                .ToList()
                .AsReadOnly(),
            WaitList = dinner
                .WaitList.Select(signUp =>
                    SignUpResponse.FromDomain(signUp, userDictionary[signUp.UserId.ToString()])
                )
                .ToList()
                .AsReadOnly()
        };
    }
}
