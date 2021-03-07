using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = PkFormat)]
	public class Game : GameBase {

		public List<string> ListGameModes;

		public List<string> ListCharacterNames;

		public List<Map> ListMaps;

	}

	public class Map {

		public string Name;

		public string ImageURL;

	}
}
