using Sgd.Domain.Common;
using Sgd.Domain.GroupAggregate.Events;
using Sgd.Domain.UserAggregate;

namespace Sgd.Domain.GroupAggregate;

public class Group : AggregateRoot<ObjectId>
{
    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public bool IsOpen { get; private set; }

    public string? ImageUrl { get; private set; }

    private readonly List<ObjectId> _memberIds = [];
    public IReadOnlyList<ObjectId> MemberIds => _memberIds.AsReadOnly();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsDeleted { get; private set; } = false;

    private Group(
        string name,
        string description,
        bool isOpen,
        string? imageUrl,
        ObjectId? id = null
    )
        : base(id ?? ObjectId.GenerateNewId())
    {
        Name = name;
        Description = description;
        IsOpen = isOpen;
        ImageUrl = imageUrl;
    }

    // Add a factory method to create a group
    public static ErrorOr<Group> CreateGroup(
        string name,
        string description,
        bool isOpen,
        string? imageUrl,
        User creator,
        ObjectId? id = null
    )
    {
        var group = new Group(name, description, isOpen, imageUrl, id);
        var assignAdminResult = group.AddMember(creator, GroupRole.Admin);
        if (assignAdminResult.IsError)
        {
            return assignAdminResult.Errors;
        }

        return group;
    }

    public ErrorOr<Success> AddMember(User user, GroupRole role)
    {
        if (_memberIds.Contains(user.Id))
        {
            return GroupErrors.UserAlreadyAssigned;
        }

        var profile = new GroupProfile(Id, user.Id, role);
        _memberIds.Add(user.Id);

        _domainEvents.Add(new GroupProfileCreatedEvent(profile));
        return Result.Success;
    }
}
