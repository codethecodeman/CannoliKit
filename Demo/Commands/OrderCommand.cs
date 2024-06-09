using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Demo.Modules.Cart;
using Discord;

namespace Demo.Commands
{
    internal class OrderCommand : ICannoliCommand
    {
        public string Name => "order";
        public DeferralType DeferralType => DeferralType.Ephemeral;

        private readonly ICannoliModuleFactory _moduleFactory;

        public OrderCommand(
            ICannoliModuleFactory moduleFactory)
        {
            _moduleFactory = moduleFactory;
        }

        public async Task RespondAsync(CannoliCommandContext context)
        {
            var cartModule = _moduleFactory.CreateModule<CartModule>(context.Command.User);
            await context.Command.FollowupAsync(cartModule);
        }

        public async Task<ApplicationCommandProperties> BuildAsync()
        {
            var builder = new SlashCommandBuilder()
            {
                Name = Name,
                Description = "Start a new grocery order",
                ContextTypes = [
                    InteractionContextType.Guild,
                ],
                DefaultMemberPermissions = GuildPermission.Administrator,
            };

            await Task.CompletedTask;

            return builder.Build();
        }
    }
}
