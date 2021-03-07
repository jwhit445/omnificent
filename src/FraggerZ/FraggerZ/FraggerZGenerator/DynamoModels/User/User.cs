using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {


	[DynamoKey(PK = PkFormat, SK = PkFormat)]
	public class User: UserBase {

		public string Username;

		public string StreamURL;

		public DateTime DateTimeSuspensionEnd;

		public DateTime DateTimePremiumExpire;

	}

}
