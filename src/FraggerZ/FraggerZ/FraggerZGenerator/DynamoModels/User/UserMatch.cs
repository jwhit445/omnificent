using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "MATCH#{" + nameof(MatchNumber) + "}")]
	public class UserMatch : UserBase {

		public int MatchNumber;

		public string PlayerTeam;

		public UserMatchStatus MatchStatus;

	}

	public enum UserMatchStatus {
		Unknown,
		InProgress,
		Cancelled,
		Won,
		Lost,
	}

}
