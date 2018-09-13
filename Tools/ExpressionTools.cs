using DataManagement.Exceptions;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataManagement.Tools
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

        internal static Parameter[] SetParametersFromExpression<T>(Expression<Func<T, bool>> expression)
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

        private static void SetParametersFromExpressionBody(BinaryExpression body, ref List<Parameter> parameters)
        {
            if (IsComparisonExpression(body))
            {
                parameters.Add(GetNewParameter(body));
                return;
            }

            if (IsComparisonExpression(body.Left))
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Left));
            }
            else
            {
                SetParametersFromExpressionBody((BinaryExpression)body.Left, ref parameters);
            }

            if (IsComparisonExpression(body.Right))
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Right));
            }
            else
            {
                SetParametersFromExpressionBody((BinaryExpression)body.Right, ref parameters);
            }
        }

        private static bool IsComparisonExpression(Expression body)
        {
            bool isComparison = false;

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
                    isComparison = true;
                    break;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    isComparison = true;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    isComparison = true;
                    break;
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LessThan:
                    isComparison = true;
                    break;
                case ExpressionType.LessThanOrEqual:
                    isComparison = true;
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
                    isComparison = true;
                    break;
                case ExpressionType.NotEqual:
                    isComparison = true;
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
                    isComparison = true;
                    break;
                case ExpressionType.OnesComplement:
                    break;
                case ExpressionType.IsTrue:
                    isComparison = true;
                    break;
                case ExpressionType.IsFalse:
                    isComparison = true;
                    break;
                default:
                    break;
            }

            return isComparison;
        }

        private static Parameter GetNewParameter(BinaryExpression body)
        {
            return new Parameter(((MemberExpression)body.Left).Member.Name, GetExpressionValue(body.Right));
        }

        private static dynamic GetExpressionValue(Expression body)
        {
            dynamic right;

            right = body as ConstantExpression;
            if (right != null) return ((ConstantExpression)right).Value;

            right = body as MemberExpression;
            if (right != null)
            {
                return Expression.Lambda(right).Compile().DynamicInvoke();
            }

            right = body as UnaryExpression;
            if (right != null)
            {
                MethodCallExpression call = (MethodCallExpression)((UnaryExpression)right).Operand;
                return Expression.Lambda(call).Compile().DynamicInvoke();
            }

            throw new NotSupportedException($"La instruccion '{body.ToString()}' no es comprendida por el analizador de consultas. Intente colocar una expresion diferente.");
        }
    }
}
