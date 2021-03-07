using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class Game
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Maps { get; set; }
        public List<string> MapImageURLs { get; set; }
        public int TeamSize { get; set; }
        
        public Game(string name, List<string> maps, List<string> mapImageURLs, int teamSize)
        {
            Name = name;
            Maps = maps;
            TeamSize = teamSize;
            MapImageURLs = mapImageURLs;
        }
    }
}
