using System;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "MATCH#{" + nameof(MatchNumber) + "}")]
	public class LadderMatch : LadderBase {

		public int SeasonId;

		public int MatchNumber;

		public DateTime DateTimeStarted;

		public DateTime DateTimeEnded;

		public string MapSK;

		public MatchStatus MatchStatus;

		public string WinningTeam;

		[DynamoGSIKey(Format = "SEASON#{" + nameof(SeasonId) + "}#{" + nameof(MatchNumber) + "}", KeyType = DynamoKeyType.SK, IndexName = "GSI1")]
		[DynamoProperty(IsHidden = true)]
		public string GS1SK;

	}

	public enum MatchStatus {
		Unknown,
		Created,
		InProgress,
		Cancelled,
		Reported,
	}

}
