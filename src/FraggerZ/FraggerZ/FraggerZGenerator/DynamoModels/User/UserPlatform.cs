using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	[DynamoKey(PK = PkFormat, SK = "PLATFORM#{" + nameof(PlatformCode) + "}#{" + nameof(PlatformUsername) + "}")]
	public class UserPlatform : UserBase {

		public PlatformType PlatformCode;

		public string PlatformId;

		public string PlatformUsername;

		[DynamoGSIKey(Format = "PLATFORM#{" + nameof(PlatformCode) + "}#{" + nameof(PlatformId) + "}", KeyType = DynamoKeyType.SK, IndexName = "GSI1")]
		public string GSI1SK;

	}

	public enum PlatformType {
		Unknown,
		Epic,
		BattleNet,
	}
}
