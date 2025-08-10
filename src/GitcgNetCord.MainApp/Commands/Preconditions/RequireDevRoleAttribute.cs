using NetCord;
using NetCord.Services;

namespace GitcgNetCord.MainApp.Commands.Preconditions;

public class RequireDevRoleAttribute<TContext>
    : PreconditionAttribute<TContext>
    where TContext : IUserContext
{
    private const ulong GitcgvnDevRoleId = 1236343128336629780;

    public override ValueTask<PreconditionResult>
        EnsureCanExecuteAsync(
            TContext context,
            IServiceProvider? serviceProvider
        )
    {
        if (context.User is not GuildUser user)
            return new(PreconditionResult.Fail(
                message: "User is not a guild user."
            ));

        if (!user.RoleIds.Contains(GitcgvnDevRoleId))
            return new(PreconditionResult.Fail(
                message: "User does not have the required developer role."
            ));

        return new(PreconditionResult.Success);
    }
}