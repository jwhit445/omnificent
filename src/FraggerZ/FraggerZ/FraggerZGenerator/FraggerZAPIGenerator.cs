using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Generator;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace FraggerZGenerator {

	public static class FraggerZAPIGenerator {

		public static void Generate() {
			DynamoGenerator.Generate();
			ApiGenerator.Generate(@"..\..\..\..\..\assets\swagger.json", @"..\..\..\API");
		}

	}
}
