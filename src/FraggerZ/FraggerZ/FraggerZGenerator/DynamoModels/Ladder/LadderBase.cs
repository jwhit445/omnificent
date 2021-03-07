using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	public class LadderBase : DynamoModelBase {

		public const string PkFormat = "#LADDER#{" + nameof(ServerId) + "}#{" + nameof(GameCode) + "}#{" + nameof(LadderName) + "}";

		public string ServerId;

		public GameCode GameCode;

		public string LadderName;

	}
}
