using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "GAMEPROFILE#{" + nameof(GameCode) + "}")]
	public class UserGameProfile : UserBase {

		public GameCode GameCode;

		public string IGN;

		public DateTime DateTimeLastStatReset;

	}

	public enum GameCode {
		Unknown,
		RogueCompany,
	}
}
