using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = PkFormat)]
	public class Ladder : LadderBase {

		public int CurrentSeasonId;

	}

}
