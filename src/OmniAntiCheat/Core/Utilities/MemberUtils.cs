using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Core {

	public class MemberUtils {

		///<summary>Returns the MemberInfo for the given expression. Should look like () => Property.</summary>
		public static MemberInfo GetMemberInfo(Expression expression) {
			MemberExpression operand;
			LambdaExpression lambdaExpression=(LambdaExpression)expression;
			if(lambdaExpression.Body is UnaryExpression un) {
				operand=(MemberExpression)un.Operand;
			}
			else {
				operand=(MemberExpression)lambdaExpression.Body;
			}
			return operand.Member;
		}

	}
}
