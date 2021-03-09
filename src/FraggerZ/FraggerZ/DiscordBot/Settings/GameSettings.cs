using System;
using System.Collections.Generic;
using System.Text;
using DiscordBot.Models;

namespace DiscordBot.Settings
{
    public static class GameSettings
    {
        public static List<Game> Games { get; } = new List<Game>()
        {
            new Game("Rogue Company", new List<string>()
            {
                "Favelas",
                //"Icarus",
                "Vice",
                "High Castle",
                "Lockdown",
                "Windward",
                "Factory",
                "Canals",
                "Skyfell"
                //"Glacier",
                //"The Arena"
            }, new List<string>()
            {
                "https://static.wikia.nocookie.net/roguecompany/images/a/ae/Favelas.jpeg/revision/latest/scale-to-width-down/310?cb=20200725034307",
                //"https://static.wikia.nocookie.net/roguecompany/images/5/54/Icarus.jpeg/revision/latest/scale-to-width-down/310?cb=20200725031304",
                "https://static.wikia.nocookie.net/roguecompany/images/9/98/Vice_Concept_Art_1.jpeg/revision/latest/scale-to-width-down/310?cb=20200724215431",
                "https://static.wikia.nocookie.net/roguecompany/images/3/3f/High_Castle_Loading_Screen.jpeg/revision/latest/scale-to-width-down/310?cb=20200812220704",
                "https://static.wikia.nocookie.net/roguecompany/images/9/99/Lockdown_Loading_Screen.png/revision/latest?cb=20200922162735",
                "https://static.wikia.nocookie.net/roguecompany/images/a/ae/Windward.jpeg/revision/latest/scale-to-width-down/310?cb=20200725003153",
                "https://static.wikia.nocookie.net/roguecompany/images/e/e7/Factory.jpeg/revision/latest/scale-to-width-down/310?cb=20200725042450",
                "https://static.wikia.nocookie.net/roguecompany/images/b/b4/Canals.jpeg/revision/latest/scale-to-width-down/310?cb=20200730035022",
                "https://static.wikia.nocookie.net/roguecompany/images/9/96/Skyfell.jpeg/revision/latest/scale-to-width-down/310?cb=20200725003918"
                //"https://www.pcinvasion.com/wp-content/uploads/2020/11/Rogue-Company-new-map-Glacier.jpg",
                //"https://cdn2.unrealengine.com/roco-egs-ob-51-featuredimage-pose-1920x1080-gt-1920x1080-228503862.jpg"
            }, 4),

            new Game("CrossFire", new List<string>()
            {
                "Compound",
                "Black Widow",
                "Port",
                "Sub Base",
                "Ankara"
            }, new List<string>()
            {
                "https://vignette.wikia.nocookie.net/crossfirelegends/images/7/76/Sattelite.jpg/revision/latest?cb=20180720152755",
                "https://static.wikia.nocookie.net/crossfirefps/images/2/2f/Black_Widow_2.0.jpg/revision/latest/scale-to-width-down/340?cb=20150426121858",
                "https://static.wikia.nocookie.net/crossfirefps/images/c/c1/Mapport.png/revision/latest/top-crop/width/450/height/450?cb=20131016093456",
                "https://static.wikia.nocookie.net/crossfirefps/images/a/a3/Comp_SubBase.jpg/revision/latest/top-crop/width/220/height/220?cb=20161025163905",
                "https://static.wikia.nocookie.net/crossfirefps/images/e/e8/Ankara.jpg/revision/latest/top-crop/width/300/height/300?cb=20130903020439"
            },5),

            new Game("IronSight", new List<string>()
            {
                "Discovery",
                "Titan",
                "Airport",
                "Cloud 9",
                "Island",
                "Mart"
            }, new List<string>() 
            {
                "https://www.f2p.com/wp-content/uploads/2019/12/Ironsight-New-Map-Discovery_2-screenshoots.jpg",
                "https://www.f2p.com/wp-content/uploads/2017/11/ironsight_screenshott_7.jpg",
                "https://cdna.artstation.com/p/assets/images/images/002/488/282/large/hyunwook-chun-12698292-1122278757816759-2740477705052801305-o.jpg?1462339281",
                "https://cms-content.s.aeriastatic.com/5ccdf33b86f4645f172cb506f9d0aa1e/files/is/image/C/Cloud9.png",
                "https://cms-content.s.aeriastatic.com/fa9d2a12aa05e76f9c1aafc3c61a21ae/files/is/image/I/Island.png",
                "https://ironsight.wiki/dl1979?display&x=auto"
            }, 5)
        };
    }
}
