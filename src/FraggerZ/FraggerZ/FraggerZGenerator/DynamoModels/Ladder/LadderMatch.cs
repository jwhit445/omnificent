using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "MATCH#{" + nameof(MatchNumber) + "}")]
	public class LadderMatch : LadderBase {

		public int SeasonId;

		public int MatchNumber;

		public DateTime DateTimeStarted;

		public DateTime DateTimeEnded;

		public string Map_SK;

		public MatchStatus MatchStatus;

		public string WinningTeam;

		[DynamoGSIKey(Format = "SEASON#{" + nameof(SeasonId) + "}#MATCH#{" + nameof(MatchNumber) + "}", KeyType = DynamoKeyType.SK)]
		public string GS1_SK;

	}

	public enum MatchStatus {
		Unknown,
		Created,
		InProgress,
		Cancelled,
		Reported,
	}

}
