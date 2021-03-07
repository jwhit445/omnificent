using Discord;
using System.Threading.Tasks;

namespace DiscordBot.Util
{
    public static class Extensions
    {
        /// <summary>
        /// Delete a message after a given number of milliseconds
        /// </summary>
        /// <param name="userMessage">The message to delete</param>
        /// <param name="delay">How long to wait before deleting it</param>
        public static async Task DeleteAfter(this IUserMessage userMessage, int delay)
        {
            await Task.Delay(delay);
            await userMessage.DeleteAsync();
        }
    }
}
