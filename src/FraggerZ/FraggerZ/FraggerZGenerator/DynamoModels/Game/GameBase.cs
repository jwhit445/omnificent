using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	public class GameBase : DynamoModelBase {

		public const string PkFormat = "#GAME#{" + nameof(ServerId) + "}#{" + nameof(GameCode) + "}";

		public string ServerId;

		public GameCode GameCode;

	}
}
