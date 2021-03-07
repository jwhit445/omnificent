using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "PLATFORM#{" + nameof(GameCode) + "}#{" + nameof(TeamName) + "}")]
	public class UserTeam : UserBase {

		public GameCode GameCode;

		public string TeamName;

		public PlayerType PlayerType;

	}

	public enum PlayerType {
		Unknown,
		Captain,
		Primary,
		Substitute,
	}

}
