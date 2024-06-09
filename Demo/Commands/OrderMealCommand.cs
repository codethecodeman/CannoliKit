using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Demo.Modules.Cart;
using Discord;

namespace Demo.Commands
{
    internal class OrderMealCommand : ICannoliCommand
    {
        public string Name => "order-meal";
        public DeferralType DeferralType => DeferralType.Ephemeral;

        private readonly ICannoliModuleFactory _moduleFactory;

        public OrderMealCommand(
            ICannoliModuleFactory moduleFactory)
        {
            _moduleFactory = moduleFactory;
        }

        public async Task RespondAsync(CannoliCommandContext context)
        {
            var cartModule = _moduleFactory.CreateModule<CartModule>();
            await context.Command.FollowupAsync(cartModule);
        }

        public async Task<ApplicationCommandProperties> BuildAsync()
        {
            var builder = new SlashCommandBuilder()
            {
                Name = Name,
                Description = Name,
                ContextTypes = [
                    InteractionContextType.Guild,
                ],
                DefaultMemberPermissions = GuildPermission.Administrator,
            };

            return builder.Build();
        }
    }
}
