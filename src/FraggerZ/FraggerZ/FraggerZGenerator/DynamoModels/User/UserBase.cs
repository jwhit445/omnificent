using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	public abstract class UserBase : DynamoModelBase {

		public const string PkFormat = "#USER#{" + nameof(ServerId) + "}#{" + nameof(UserId) + "}";

		public string ServerId;

		public string UserId;

	}
}
