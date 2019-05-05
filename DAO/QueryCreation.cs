using OneData.Enums;
using OneData.Models;
using OneData.Tools;
using System;
using System.Linq.Expressions;

namespace OneData.DAO
{
    public class QueryCreation
    {
        public static string GetStringFromNodeType(Expression body, string tableName)
        {
            NameValueObject pair = null;

            if (ExpressionTools.GetNodeGroup(body) == NodeGroupTypes.Comparison)
            {
                pair = ExpressionTools.GetPairFromComparison((BinaryExpression)body, tableName);
            }
            else if (ExpressionTools.GetNodeGroup(body) == NodeGroupTypes.Method)
            {
                pair = GetPairFromMethod(body, tableName);
            }

            switch (body.NodeType)
            {
                case ExpressionType.Add:
                    break;
                case ExpressionType.AddChecked:
                    break;
                case ExpressionType.And:
                    break;
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.Call:
                    return $"{tableName}.{pair.Name} {GetSqlTranslationFromMethodName((MethodCallExpression)body, pair.Value)}";
                case ExpressionType.Coalesce:
                    break;
                case ExpressionType.Conditional:
                    break;
                case ExpressionType.Constant:
                    break;
                case ExpressionType.Convert:
                    break;
                case ExpressionType.ConvertChecked:
                    break;
                case ExpressionType.Divide:
                    break;
                case ExpressionType.Equal:
                    return string.Format("{0} {1}", pair.Name, pair.Value == null ? "is null" : $" = {pair.Value}");
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    return string.Format("{0} > {1}", pair.Name, pair.Value ?? "null");
                case ExpressionType.GreaterThanOrEqual:
                    return string.Format("{0} >= {1}", pair.Name, pair.Value ?? "null");
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LessThan:
                    return string.Format("{0} < {1}", pair.Name, pair.Value ?? "null");
                case ExpressionType.LessThanOrEqual:
                    return string.Format("{0} <= {1}", pair.Name, pair.Value ?? "null");
                case ExpressionType.ListInit:
                    break;
                case ExpressionType.MemberAccess:
                    break;
                case ExpressionType.MemberInit:
                    break;
                case ExpressionType.Modulo:
                    break;
                case ExpressionType.Multiply:
                    break;
                case ExpressionType.MultiplyChecked:
                    break;
                case ExpressionType.Negate:
                    break;
                case ExpressionType.UnaryPlus:
                    break;
                case ExpressionType.NegateChecked:
                    break;
                case ExpressionType.New:
                    break;
                case ExpressionType.NewArrayInit:
                    break;
                case ExpressionType.NewArrayBounds:
                    break;
                case ExpressionType.Not:
                    break;
                case ExpressionType.NotEqual:
                    return string.Format("{0} {1}", pair.Name, pair.Value == null ? "is not null" : $" != {pair.Value }");
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Parameter:
                    break;
                case ExpressionType.Power:
                    break;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.RightShift:
                    break;
                case ExpressionType.Subtract:
                    break;
                case ExpressionType.SubtractChecked:
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.TypeIs:
                    break;
                case ExpressionType.Assign:
                    break;
                case ExpressionType.Block:
                    break;
                case ExpressionType.DebugInfo:
                    break;
                case ExpressionType.Decrement:
                    break;
                case ExpressionType.Dynamic:
                    break;
                case ExpressionType.Default:
                    break;
                case ExpressionType.Extension:
                    break;
                case ExpressionType.Goto:
                    break;
                case ExpressionType.Increment:
                    break;
                case ExpressionType.Index:
                    break;
                case ExpressionType.Label:
                    break;
                case ExpressionType.RuntimeVariables:
                    break;
                case ExpressionType.Loop:
                    break;
                case ExpressionType.Switch:
                    break;
                case ExpressionType.Throw:
                    break;
                case ExpressionType.Try:
                    break;
                case ExpressionType.Unbox:
                    break;
                case ExpressionType.AddAssign:
                    break;
                case ExpressionType.AndAssign:
                    break;
                case ExpressionType.DivideAssign:
                    break;
                case ExpressionType.ExclusiveOrAssign:
                    break;
                case ExpressionType.LeftShiftAssign:
                    break;
                case ExpressionType.ModuloAssign:
                    break;
                case ExpressionType.MultiplyAssign:
                    break;
                case ExpressionType.OrAssign:
                    break;
                case ExpressionType.PowerAssign:
                    break;
                case ExpressionType.RightShiftAssign:
                    break;
                case ExpressionType.SubtractAssign:
                    break;
                case ExpressionType.AddAssignChecked:
                    break;
                case ExpressionType.MultiplyAssignChecked:
                    break;
                case ExpressionType.SubtractAssignChecked:
                    break;
                case ExpressionType.PreIncrementAssign:
                    break;
                case ExpressionType.PreDecrementAssign:
                    break;
                case ExpressionType.PostIncrementAssign:
                    break;
                case ExpressionType.PostDecrementAssign:
                    break;
                case ExpressionType.TypeEqual:
                    break;
                case ExpressionType.OnesComplement:
                    break;
                case ExpressionType.IsTrue:
                    break;
                case ExpressionType.IsFalse:
                    break;
                default:
                    break;
            }

            return string.Empty;
        }

        private static NameValueObject GetPairFromMethod(Expression body, string tableName)
        {
            MemberExpression member = (MemberExpression)((MethodCallExpression)body).Object;
            MethodCallExpression method = (MethodCallExpression)body;
            object value = null;
            if (method.Arguments[0] is ConstantExpression)
            {
                value = ((ConstantExpression)method.Arguments[0]).Value;
            }
            else if (method.Arguments[0] is MemberExpression)
            {
                var objectMember = Expression.Convert(method.Arguments[0], typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                value = getter();
            }

            return new NameValueObject(member.Member.Name, value);
        }

        private static string GetSqlTranslationFromMethodName(MethodCallExpression method, object value)
        {
            switch (method.Method.Name)
            {
                case "Contains":
                    return $"like '%{value}%'";
                case "StartsWith":
                    return $"like '{value}%'";
                case "EndsWith":
                    return $"like '%{value}'";
                default:
                    throw new NotSupportedException($"El metodo '{method.Method.Name}' no es comprendido por el analizador de consultas. Intente colocar una expresion diferente.");
            }
        }
    }
}
