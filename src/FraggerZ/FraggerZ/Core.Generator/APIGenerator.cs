using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Core.Generator {
	public static class ApiGenerator {

		private static string _cSharpApiFolderPath;

		public static void Generate(string jsonPath, string cSharpApiFolderPath) {
			_cSharpApiFolderPath = cSharpApiFolderPath;
			if(!File.Exists(jsonPath)) {
				throw new ApplicationException($"{jsonPath} does not exist.");
			}
			OpenApiStringReader apiReader = new OpenApiStringReader();
			OpenApiDocument apiDocument = apiReader.Read(File.ReadAllText(jsonPath), out OpenApiDiagnostic diagnostic);
			if(!diagnostic.Errors.IsNullOrEmpty()) {
				throw new ApplicationException($"Errors when parsing json file: {string.Join("\r\n", diagnostic.Errors)}");
			}
			Directory.CreateDirectory(cSharpApiFolderPath);
			foreach(KeyValuePair<string, OpenApiPathItem> pathPair in apiDocument.Paths) {
				foreach(KeyValuePair<OperationType, OpenApiOperation> operationPair in pathPair.Value.Operations) {
					CSharp.GenerateAPIMethod(pathPair.Key, operationPair.Key, operationPair.Value);
				}
			}
		}

		private static class CSharp {

			public static void GenerateAPIMethod(string path, OperationType operationType, OpenApiOperation operation) {
				GenerateRequestClass(operation);
				GenerateResponseClass(operation);
			}

			private static void GenerateRequestClass(OpenApiOperation operation) {
				string className = $"{operation.OperationId[0].ToString().ToUpper()}{operation.OperationId.Substring(1)}Request";
				StringBuilder strBuilder = new StringBuilder();
				strBuilder.AppendLine($"public class {className} {{\r\n");
				foreach(OpenApiParameter parameter in operation.Parameters) {
					if(!string.IsNullOrWhiteSpace(parameter.Description)) {
						strBuilder.AppendLine($"///<summary>{parameter.Description}</summary>");
					}
					strBuilder.AppendLine($"public {parameter}");
				}
				strBuilder.AppendLine("}");
				Directory.CreateDirectory($@"{_cSharpApiFolderPath}\Models");
				File.WriteAllText($@"{_cSharpApiFolderPath}\Models\{className}.cs", strBuilder.ToString());
			}

			private static void GenerateResponseClass(OpenApiOperation operation) {
				
			}

		}

		private static string ToFirstUpper(string str) {

		}

	}
}
