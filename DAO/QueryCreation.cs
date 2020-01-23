using MySql.Data.MySqlClient;
using OneData.Enums;
using OneData.Models;
using OneData.Tools;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace OneData.DAO
{
    public class QueryCreation
    {
        public static string GetLogicalStringFromNodeTypeOperation(Expression body)
        {
            string stringFromNode = string.Empty;

            switch (body.NodeType)
            {
                case ExpressionType.Add:
                    break;
                case ExpressionType.AddChecked:
                    break;
                case ExpressionType.And:
                    break;
                case ExpressionType.AndAlso:
                    stringFromNode = " AND ";
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.Call:
                    break;
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
                    break;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    break;
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LessThan:
                    break;
                case ExpressionType.LessThanOrEqual:
                    break;
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
                    break;
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrElse:
                    stringFromNode = " OR ";
                    break;
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

            return stringFromNode;
        }

        public static string GetStringFromNodeType(Expression body, string tableName, ref DbCommand command)
        {
            BinaryObjectRepresentation binaryRepresentation = null;
            string stringFromNode = string.Empty;

            if (ExpressionTools.GetNodeGroup(body) == NodeGroupTypes.Comparison)
            {
                binaryRepresentation = ExpressionTools.GetPairFromComparison((BinaryExpression)body, tableName);
            }
            else if (ExpressionTools.GetNodeGroup(body) == NodeGroupTypes.Method)
            {
                binaryRepresentation = GetPairFromMethod(body, tableName);
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
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.Call:
                    stringFromNode = $"{tableName}.{binaryRepresentation.Name} LIKE {binaryRepresentation.ParameterName}";
                    binaryRepresentation.Value = GetSqlTranslationFromMethodName(((MethodCallExpression)body).Method.Name, binaryRepresentation.Value);
                    break;
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
                    stringFromNode = string.Format("{0} {1}", binaryRepresentation.Name, binaryRepresentation.Value == null ? "is null" : $" = {binaryRepresentation.ParameterName}");
                    break;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    stringFromNode = string.Format("{0} > {1}", binaryRepresentation.Name, binaryRepresentation.ParameterName ?? "null");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    stringFromNode = string.Format("{0} >= {1}", binaryRepresentation.Name, binaryRepresentation.ParameterName ?? "null");
                    break;
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LessThan:
                    stringFromNode = string.Format("{0} < {1}", binaryRepresentation.Name, binaryRepresentation.ParameterName ?? "null");
                    break;
                case ExpressionType.LessThanOrEqual:
                    stringFromNode = string.Format("{0} <= {1}", binaryRepresentation.Name, binaryRepresentation.ParameterName ?? "null");
                    break;
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
                    stringFromNode = string.Format("{0} {1}", binaryRepresentation.Name, binaryRepresentation.Value == null ? "is not null" : $" != {binaryRepresentation.ParameterName }");
                    break;
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrElse:
                    break;
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

            if (binaryRepresentation.Value != null)
            {
                switch (Manager.ConnectionType)
                {
                    case ConnectionTypes.MySQL:
                        ((MySqlCommand)command).Parameters.AddWithValue(binaryRepresentation.ParameterName, binaryRepresentation.Value);
                        break;
                    case ConnectionTypes.MSSQL:
                        ((SqlCommand)command).Parameters.AddWithValue(binaryRepresentation.ParameterName, binaryRepresentation.Value);
                        break;
                    default:
                        break;
                }
            }

            return stringFromNode;
        }

        private static BinaryObjectRepresentation GetPairFromMethod(Expression body, string tableName)
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
            bool isMsSQL = Manager.ConnectionType == ConnectionTypes.MSSQL ? true : false;
            return new BinaryObjectRepresentation(isMsSQL == true ? $"[{member.Member.Name}]" : $"`{member.Member.Name}`", value);
        }

        private static string GetSqlTranslationFromMethodName(string name, object value)
        {
            switch (name)
            {
                case "Contains":
                    return $"%{value}%";
                case "StartsWith":
                    return $"{value}%";
                case "EndsWith":
                    return $"%{value}";
                default:
                    throw new NotSupportedException($"El metodo '{name}' no es comprendido por el analizador de consultas. Intente colocar una expresion diferente.");
            }
        }
    }
}
