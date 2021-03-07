using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {

	public class DynamoKeyAttribute : Attribute {

		public string PK { get; set; }

		public string SK { get; set; }

	}

}
