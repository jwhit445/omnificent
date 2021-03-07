using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraggerZGenerator.DynamoModels {
	public class DynamoGSIKeyAttribute : Attribute {

		public string Format;

		public DynamoKeyType KeyType;
		
	}

	public enum DynamoKeyType {
		PK,
		SK,
	}
}
