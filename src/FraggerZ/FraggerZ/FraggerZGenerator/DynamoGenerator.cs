using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FraggerZGenerator.DynamoModels;

namespace FraggerZGenerator {

	public static class DynamoGenerator {

		public static void Generate() {
			List<Type> listFacetTypes = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(x => x.GetCustomAttribute<DynamoKeyAttribute>() != null)
				.ToList();
            string modelsFolder = @"..\..\..\..\..\api\src\models";
            Directory.CreateDirectory(modelsFolder);
            foreach (Type facetType in listFacetTypes) {
                List<FieldInfo> listFields = facetType
                    .GetFields()
                    .OrderByDescending(x => x.Name.ToLower() == "pk")
                    .ThenByDescending(x => x.Name.ToLower() == "sk")
                    .ToList();
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine($"export class {facetType.Name} {{");
                foreach(FieldInfo field in listFields) {
                    classBuilder.AppendLine($"\t{field.Name}?: {GetTypeScriptTypeFromField(field.FieldType)};");
				}
                classBuilder.AppendLine("}");
                File.WriteAllText($"{modelsFolder}\\{facetType.Name}.ts", classBuilder.ToString());
			}
		}

        private static string GetTypeScriptTypeFromField(Type fieldType) {
            string type;
            if(fieldType == typeof(string)) {
                type = "string";
			}
            else if(fieldType == typeof(int) || fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(decimal)) {
                type = "number";
			}
            else if(fieldType == typeof(DateTime)) {
                type = "Date";
			}
            else if(fieldType == typeof(bool)) {
                type = "boolean";
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
                type = $"{GetTypeScriptTypeFromField(fieldType.GetGenericArguments()[0])}[]";
            }
            else {
                type = fieldType.Name;
			}
            return $"{type}";
		}
	}

}
