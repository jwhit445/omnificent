using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	[DynamoKey(PK = PkFormat, SK = "SETTINGS#{" + nameof(SeasonId) + "}")]
	public class LadderSettings : LadderBase {

		public int SeasonId;

		public int TeamSize;

		public int NumberOfTeams;

		public TeamSelectionType TeamSelectionType;

		public RatingSystem RatingSystem;

		public string GameMode;

		public bool CanBanCharacters;

		public bool CanProtectCharacters;

	}

	public enum RatingSystem {
		Unknown,
		TrueSkill,
		Constant,
	}

	public enum TeamSelectionType {
		Unknown,
		Automatic,
		Captains,
	}
}
