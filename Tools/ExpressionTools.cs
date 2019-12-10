using OneData.DAO;
using OneData.Enums;
using OneData.Exceptions;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace OneData.Tools
{
    public class ExpressionTools
    {
        internal static TRequired GetExpressionBodyType<TParameter, TResult, TRequired>(Expression<Func<TParameter, TResult>> expression, Expression<Func<TParameter, TRequired>> expressionType, bool throwErrorIfFalse = true) where TRequired : class
        {
            TRequired currentExpressionType = expression.Body as TRequired;

            if (currentExpressionType == null && throwErrorIfFalse)
            {
                throw new NotMatchingExpressionTypeException(expression.Body.GetType().FullName, expressionType.ToString());
            }

            return currentExpressionType;
        }

        internal static Parameter[] ConvertExpressionToParameters<T>(Expression<Func<T, bool>> expression)
        {
            List<Parameter> parameters = new List<Parameter>();

            try
            {
                SetParametersFromExpressionBody((BinaryExpression)expression.Body, ref parameters);
            }
            catch
            {
                throw new NotSupportedException($"La instruccion '{expression.ToString()}' no es comprendida por el analizador de consultas. Intente colocar una expresion diferente.");
            }

            return parameters.ToArray();
        }

        internal static string ConvertExpressionToSQL<T>(Expression<Func<T, bool>> expression, ref DbCommand command) where T : Cope<T>, IManageable, new()
        {
            StringBuilder builder = new StringBuilder();
            string qualifiedTableName = Manager.ConnectionType == ConnectionTypes.MySQL ? $"`{Manager.TablePrefix}{Cope<T>.ModelComposition.TableName}`" : $"[{Cope<T>.ModelComposition.Schema}].[{Manager.TablePrefix}{Cope<T>.ModelComposition.TableName}]";

            try
            {
                if (expression.Body.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression body = (MethodCallExpression)expression.Body;
                    BuildQueryFromMethodCallExpressionBody(body, ref builder, qualifiedTableName, ref command);
                }
                else
                {
                    BinaryExpression body = (BinaryExpression)expression.Body;
                    BuildQueryFromBinaryExpressionBody(body, ref builder, qualifiedTableName, ref command);
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"La instruccion '{expression.ToString()}' no es comprendida por el analizador de consultas. Intente colocar una expresion diferente.", ex);
            }

            return builder.ToString();
        }

        private static void BuildQueryFromMethodCallExpressionBody(MethodCallExpression body, ref StringBuilder builder, string qualifiedTableName, ref DbCommand command)
        {
            builder.Append(QueryCreation.GetStringFromNodeType(body, qualifiedTableName, ref command));
        }

        private static void BuildQueryFromBinaryExpressionBody(BinaryExpression body, ref StringBuilder builder, string tableName, ref DbCommand command)
        {
            string logicalString = string.Empty;
            if (GetNodeGroup(body) == NodeGroupTypes.Comparison)
            {
                builder.Append(QueryCreation.GetStringFromNodeType(body, tableName, ref command));
                return;
            }
            else
            {
                logicalString = QueryCreation.GetStringFromNodeType(body, tableName, ref command);
            }

            if (GetNodeGroup(body.Left) == NodeGroupTypes.Comparison)
            {
                builder.Append(QueryCreation.GetStringFromNodeType(body.Left, tableName, ref command));
            }
            else
            {
                if (body.Left.NodeType == ExpressionType.Call)
                {
                    BuildQueryFromMethodCallExpressionBody((MethodCallExpression)body.Left, ref builder, tableName, ref command);
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
                    BuildQueryFromMethodCallExpressionBody((MethodCallExpression)body.Right, ref builder, tableName, ref command);
                }
                else
                {
                    BuildQueryFromBinaryExpressionBody((BinaryExpression)body.Right, ref builder, tableName, ref command);
                }
            }
        }

        private static void SetParametersFromExpressionBody(BinaryExpression body, ref List<Parameter> parameters)
        {
            if (GetNodeGroup(body) == NodeGroupTypes.Comparison)
            {
                parameters.Add(GetNewParameter(body));
                return;
            }

            if (GetNodeGroup(body.Left) == NodeGroupTypes.Comparison)
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Left));
            }
            else
            {
                SetParametersFromExpressionBody((BinaryExpression)body.Left, ref parameters);
            }

            if (GetNodeGroup(body.Right) == NodeGroupTypes.Comparison)
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Right));
            }
            else
            {
                SetParametersFromExpressionBody((BinaryExpression)body.Right, ref parameters);
            }
        }

        private static Parameter GetNewParameter(BinaryExpression body)
        {
            BinaryObjectRepresentation pair = GetPairFromComparison(body, "");
            return new Parameter(pair.Name.ToString(), pair.Value);
        }

        internal static BinaryObjectRepresentation GetPairFromComparison(BinaryExpression body, string tableName)
        {
            return new BinaryObjectRepresentation(GetExpressionValue(body.Left, tableName), GetExpressionValue(body.Right, tableName));
        }

        private static object GetExpressionValue(Expression body, string tableName)
        {
            object result = null;
            bool checkAnsciiType = true;
            bool isMsSQL = Manager.ConnectionType == ConnectionTypes.MSSQL ? true : false;

            if (body is ConstantExpression)
            {
                result = ((ConstantExpression)body).Value;
            }

            if (body is MemberExpression)
            {
                if (((MemberExpression)body).Expression != null)
                {
                    if (((MemberExpression)body).Expression.NodeType == ExpressionType.Parameter)
                    {
                        // Si contiene un valor en Expression es por que  hace referencia a una propiedad interna y por ello
                        // no debe obtener el valor contenido (ya que no existe), sino solo el nombre de la misma.
                        checkAnsciiType = false;
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            result = isMsSQL == true ? $"[{((MemberExpression)body).Member.Name}]" : $"`{((MemberExpression)body).Member.Name}`";
                        }
                        else
                        {
                            result = isMsSQL == true ? $"{tableName}.[{((MemberExpression)body).Member.Name}]" : $"{tableName}.`{((MemberExpression)body).Member.Name}`";
                        }
                    }
                    else
                    {
                        // Caso contrario, significa que tiene que traer el valor contenido en la variable o propiedad.
                        result = Expression.Lambda(body).Compile().DynamicInvoke();
                    }
                }
                else
                {
                    // Caso contrario, significa que tiene que traer el valor contenido en la variable o propiedad.
                    result = Expression.Lambda(body).Compile().DynamicInvoke();
                }
            }

            if (body is UnaryExpression || body is MethodCallExpression)
            {
                result = Expression.Lambda(body).Compile().DynamicInvoke();
                if (result is bool)
                {
                    checkAnsciiType = false;
                    result = (bool)result == true ? 1 : 0;
                }
            }

            //if (checkAnsciiType && result != null)
            //{
            //    // Si el tipo del resultado tiene seleccionado internamente que es un formato de un string, entonces agrega dos comillas simples 
            //    // alrededor del mismo.
            //    AppendSingleQuotes(ref result);
            //}

            if (result == null)
            {
                throw new NotSupportedException($"La instruccion '{body.ToString()}' no es comprendida por el analizador de consultas. Intente colocar una expresion diferente.");
            }

            return result;
        }

        private static void AppendSingleQuotes(ref object result)
        {
            if (result.GetType().IsAnsiClass)
            {
                result = $"'{result}'";
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
