using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	public class DynamoModelBase {

		[DynamoProperty(IsHidden = true)]
		public string PK;

		[DynamoProperty(IsHidden = true)]
		public string SK;

	}
}
