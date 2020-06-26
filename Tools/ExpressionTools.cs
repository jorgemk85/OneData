using OneData.DAO;
using OneData.Enums;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace OneData.Tools
{
    public class ExpressionTools
    {
        internal static string ConvertExpressionToSQL<T>(Expression<Func<T, bool>> expression, ref DbCommand command) where T : IManageable, new()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                string qualifiedTableName = Manager.ConnectionType == ConnectionTypes.MySQL ? $"`{Manager.TablePrefix}{Manager<T>.Composition.TableName}`" : $"[{Manager<T>.Composition.Schema}].[{Manager.TablePrefix}{Manager<T>.Composition.TableName}]";

                switch (expression.Body)
                {
                    case BinaryExpression binaryExpression:
                        BuildQueryFromBinaryExpressionBody(binaryExpression, ref builder, qualifiedTableName, ref command);
                        break;
                    default:
                        builder.Append(QueryCreation.GetStringFromNodeType(expression.Body, qualifiedTableName, ref command));
                        break;
                }

                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void BuildQueryFromBinaryExpressionBody(BinaryExpression body, ref StringBuilder builder, string tableName, ref DbCommand command)
        {
            string logicalString = string.Empty;

            builder.Append("(");

            if (GetNodeGroup(body) == NodeGroupTypes.Comparison)
            {
                builder.Append(QueryCreation.GetStringFromNodeType(body, tableName, ref command));
                builder.Append(")");
                return;
            }
            else
            {
                logicalString = QueryCreation.GetLogicalStringFromNodeTypeOperation(body);
            }

            if (GetNodeGroup(body.Left) == NodeGroupTypes.Comparison)
            {
                builder.Append(QueryCreation.GetStringFromNodeType(body.Left, tableName, ref command));
            }
            else
            {
                if (body.Left.NodeType == ExpressionType.Call)
                {
                    builder.Append(QueryCreation.GetStringFromNodeType((MethodCallExpression)body.Left, tableName, ref command));
                }
                else
                {
                    BuildQueryFromBinaryExpressionBody((BinaryExpression)body.Left, ref builder, tableName, ref command);
                }
            }

            if (!string.IsNullOrWhiteSpace(logicalString))
            {
                builder.Append(logicalString);
            }

            if (GetNodeGroup(body.Right) == NodeGroupTypes.Comparison)
            {
                builder.Append(QueryCreation.GetStringFromNodeType(body.Right, tableName, ref command));
            }
            else
            {
                if (body.Right.NodeType == ExpressionType.Call)
                {
                    builder.Append(QueryCreation.GetStringFromNodeType((MethodCallExpression)body.Right, tableName, ref command));
                }
                else
                {
                    BuildQueryFromBinaryExpressionBody((BinaryExpression)body.Right, ref builder, tableName, ref command);
                }
            }
            builder.Append(")");
        }

        internal static BinaryObjectRepresentation GetPairFromComparison(BinaryExpression body, string tableName)
        {
            return new BinaryObjectRepresentation(GetExpressionValue(body.Left, tableName), GetExpressionValue(body.Right, tableName));
        }

        private static object GetExpressionValue(Expression body, string tableName)
        {
            object result = new object();
            bool isMsSQL = Manager.ConnectionType == ConnectionTypes.MSSQL ? true : false;

            switch (body)
            {
                case ConstantExpression constantExpression:
                    result = GetResultFromConstantExpression(constantExpression, tableName);
                    break;
                case MemberExpression memberExpression:
                    result = GetResultFromMemberExpression(memberExpression, tableName);
                    break;
                case MethodCallExpression methodCallExpression:
                    result = GetResultFromMethodCallExpression(methodCallExpression, tableName);
                    break;
                case UnaryExpression unaryExpression:
                    result = GetResultFromUnaryExpression(unaryExpression, tableName);
                    break;
                default:
                    break;
            }

            if (result == new object())
            {
                throw new NotSupportedException($"La instruccion '{body.ToString()}' no es comprendida por el analizador de consultas. Intente colocar una expresion diferente.");
            }

            return result;
        }

        private static object GetResultFromUnaryExpression(UnaryExpression unaryExpression, string tableName)
        {
            switch (unaryExpression.Operand)
            {
                case MemberExpression memberExpression:
                    if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        return GetMemberExpressionName(memberExpression, tableName);
                    }
                    break;
                default:
                    break;
            }

            object result = Expression.Lambda(unaryExpression).Compile().DynamicInvoke();
            if (result is bool)
            {
                result = (bool)result == true ? 1 : 0;
            }

            return result;
        }

        private static object GetResultFromMethodCallExpression(MethodCallExpression methodCallExpression, string tableName)
        {
            object result = Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();
            if (result is bool)
            {
                result = (bool)result == true ? 1 : 0;
            }

            return result;
        }

        private static object GetResultFromMemberExpression(MemberExpression memberExpression, string tableName)
        {
            if (memberExpression.Expression != null)
            {
                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    return GetMemberExpressionName(memberExpression, tableName);
                }
            }

            return Expression.Lambda(memberExpression).Compile().DynamicInvoke();
        }

        private static object GetResultFromConstantExpression(ConstantExpression constantExpression, string tableName)
        {
            return constantExpression.Value;
        }

        private static string GetMemberExpressionName(MemberExpression memberExpression, string tableName)
        {
            bool isMsSQL = Manager.ConnectionType == ConnectionTypes.MSSQL ? true : false;

            // Si contiene un valor en Expression es por que  hace referencia a una propiedad interna y por ello
            // no debe obtener el valor contenido (ya que no existe), sino solo el nombre de la misma.
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return isMsSQL == true ? $"[{memberExpression.Member.Name}]" : $"`{memberExpression.Member.Name}`";
            }
            else
            {
                return isMsSQL == true ? $"{tableName}.[{memberExpression.Member.Name}]" : $"{tableName}.`{memberExpression.Member.Name}`";
            }
        }

        internal static NodeGroupTypes GetNodeGroup(Expression body)
        {
            switch (body.NodeType)
            {
                case ExpressionType.Add:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.AddChecked:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.And:
                    return NodeGroupTypes.Logical;
                case ExpressionType.AndAlso:
                    return NodeGroupTypes.Logical;
                case ExpressionType.ArrayLength:
                    return NodeGroupTypes.Value;
                case ExpressionType.ArrayIndex:
                    return NodeGroupTypes.Value;
                case ExpressionType.Call:
                    return NodeGroupTypes.Method;
                case ExpressionType.Coalesce:
                    return NodeGroupTypes.Method;
                case ExpressionType.Conditional:
                    return NodeGroupTypes.Method;
                case ExpressionType.Constant:
                    return NodeGroupTypes.Value;
                case ExpressionType.Convert:
                    return NodeGroupTypes.Method;
                case ExpressionType.ConvertChecked:
                    return NodeGroupTypes.Method;
                case ExpressionType.Divide:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.Equal:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.ExclusiveOr:
                    return NodeGroupTypes.Logical;
                case ExpressionType.GreaterThan:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.GreaterThanOrEqual:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.Invoke:
                    return NodeGroupTypes.Method;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    return NodeGroupTypes.Bitwise;
                case ExpressionType.LessThan:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.LessThanOrEqual:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.ListInit:
                    return NodeGroupTypes.Method;
                case ExpressionType.MemberAccess:
                    return NodeGroupTypes.Value;
                case ExpressionType.MemberInit:
                    return NodeGroupTypes.Method;
                case ExpressionType.Modulo:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.Multiply:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.MultiplyChecked:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.Negate:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.UnaryPlus:
                    return NodeGroupTypes.Method;
                case ExpressionType.NegateChecked:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.New:
                    return NodeGroupTypes.Method;
                case ExpressionType.NewArrayInit:
                    return NodeGroupTypes.Method;
                case ExpressionType.NewArrayBounds:
                    return NodeGroupTypes.Method;
                case ExpressionType.Not:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.NotEqual:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.Or:
                    return NodeGroupTypes.Logical;
                case ExpressionType.OrElse:
                    return NodeGroupTypes.Logical;
                case ExpressionType.Parameter:
                    return NodeGroupTypes.Method;
                case ExpressionType.Power:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.RightShift:
                    return NodeGroupTypes.Bitwise;
                case ExpressionType.Subtract:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.SubtractChecked:
                    return NodeGroupTypes.Arithmetic;
                case ExpressionType.TypeAs:
                    return NodeGroupTypes.Method;
                case ExpressionType.TypeIs:
                    return NodeGroupTypes.Method;
                case ExpressionType.Assign:
                    return NodeGroupTypes.Method;
                case ExpressionType.Block:
                    break;
                case ExpressionType.DebugInfo:
                    break;
                case ExpressionType.Decrement:
                    return NodeGroupTypes.Method;
                case ExpressionType.Dynamic:
                    break;
                case ExpressionType.Default:
                    return NodeGroupTypes.Method;
                case ExpressionType.Extension:
                    break;
                case ExpressionType.Goto:
                    return NodeGroupTypes.Method;
                case ExpressionType.Increment:
                    return NodeGroupTypes.Method;
                case ExpressionType.Index:
                    return NodeGroupTypes.Method;
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
                    return NodeGroupTypes.Method;
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
                    return NodeGroupTypes.Method;
                case ExpressionType.PreDecrementAssign:
                    return NodeGroupTypes.Method;
                case ExpressionType.PostIncrementAssign:
                    return NodeGroupTypes.Method;
                case ExpressionType.PostDecrementAssign:
                    return NodeGroupTypes.Method;
                case ExpressionType.TypeEqual:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.OnesComplement:
                    break;
                case ExpressionType.IsTrue:
                    return NodeGroupTypes.Comparison;
                case ExpressionType.IsFalse:
                    return NodeGroupTypes.Comparison;
                default:
                    break;
            }

            return NodeGroupTypes.Unknown;
        }
    }
}
