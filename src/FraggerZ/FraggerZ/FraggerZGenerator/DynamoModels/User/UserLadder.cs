using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	[DynamoKey(PK = PkFormat, SK = "LADDER#{" + nameof(GameCode) + "}#{" + nameof(LadderName) + "}#{" + nameof(SeasonId) + "}")]
	public class UserLadder : UserBase {

		public GameCode GameCode;

		public string LadderName;

		public int SeasonId;

		public double MMR;

		public double Sigma;

		public int Wins;

		public int Losses;

	}
}
