using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Interfaces;
using Discord;

namespace Sample.Commands
{
    internal class OrderMealCommand : ICannoliCommand
    {
        public string Name => "order-meal";
        public DeferralType DeferralType => DeferralType.Ephemeral;

        public Task RespondAsync(CannoliCommandContext context)
        {
            throw new NotImplementedException();
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
