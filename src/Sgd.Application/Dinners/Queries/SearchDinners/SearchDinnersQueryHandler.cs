using Sgd.Application.Common.Interfaces;
using Sgd.Application.Common.Messaging;
using Sgd.Application.Dinners.Queries.Common;

namespace Sgd.Application.Dinners.Queries.SearchDinners;

internal sealed class SearchDinnersQueryHandler(
    IDinnerRepository dinnerRepository,
    IUserRepository userRepository
) : IQueryHandler<SearchDinnersQuery, IReadOnlyList<DinnerResponse>>
{
    public async Task<ErrorOr<IReadOnlyList<DinnerResponse>>> Handle(
        SearchDinnersQuery request,
        CancellationToken cancellationToken
    )
    {
        var dinners = await dinnerRepository.SearchDinners(request.Name, cancellationToken);

        // Collect all user IDs from the dinners
        var userIds = dinners
            .SelectMany(dinner =>
                dinner
                    .Hosts.Concat(dinner.SignUps.Select(s => s.UserId))
                    .Concat(dinner.WaitList.Select(w => w.UserId))
            )
            .Distinct()
            .ToList();

        var users = await userRepository.GetUsers(userIds);
        return dinners.Select(dinner => DinnerResponse.FromDomain(dinner, users)).ToList();
    }
}
