using DataManagement.Enums;
using DataManagement.Exceptions;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;

namespace DataManagement.Tools
{
    public class ConsolidationTools
    {
        /// <summary>
        /// Valida que no exista una sola propiedad con valor nulo.
        /// </summary>
        /// <param name="obj">El objeto que sera validado.</param>
        /// <param name="throwError">Especifica si se debe de arrojar error o regresar false cuando se encuentren valores nulos.</param>
        /// <returns>Regresa True cuando el objeto tiene todas las propiedades asignadas, o error, cuando es lo contrario.</returns>
        public static bool PerformNullValidation(object obj, bool throwError)
        {
            PropertyInfo[] typeProperties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in typeProperties)
            {
                if (property.GetValue(obj) == null)
                {
                    if (throwError)
                    {
                        throw new FoundNullException(string.Format("Se encontró un valor nulo en una propiedad del objeto de tipo {0} al crear una nueva instancia.", obj.GetType().ToString()));
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Asigna los valores proporcionados en las propiedades correspondientes a la instancia del objeto de tipo <typeparamref name="T"/>. Solo se aceptan valores originados desde un objeto anonimo o predefinidos del mismo tipo enviado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a asignar.</typeparam>
        /// <param name="obj">El objeto a alimentarle los valores proporcionados.</param>
        /// <param name="values">Los valores usados en la asignacion de las propiedades. Se admiten objetos anonimos o predefinidos del mismo tipo enviado.</param>
        /// <returns>Regresa el objeto ya alimentado de los valores.</returns>
        public static T SetValuesIntoObjectOfType<T>(T obj, dynamic values)
        {
            PropertyInfo[] typeProperties = typeof(T).GetProperties();
            PropertyInfo[] anonymousProperties = values.GetType().GetProperties();

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                foreach (PropertyInfo anonymousProperty in anonymousProperties)
                {
                    if (typeProperty.Name.Equals(anonymousProperty.Name))
                    {
                        if (typeProperty.CanWrite)
                        {
                            typeProperty.SetValue(obj, SimpleConverter.ConvertStringToType(anonymousProperty.GetValue(values).ToString(), typeProperty.PropertyType));
                        }
                        else
                        {
                            throw new SetAccessorNotFoundException(typeProperty.Name);
                        }

                        break;
                    }
                }
            }

            return obj;
        }

        /// <summary>
        /// Obtiene el valor colocado bajo la llave proporcionada en el archivo de Configuracion del proyecto.
        /// </summary>
        /// <param name="key">Llave a localizar.</param>
        /// <param name="type">Especifica el tipo de configuracion al que pertenece la llave.</param>
        /// <returns>Regresa el valor obtenido del archivo de configuracion, si la llave fue encontrada.</returns>
        public static string GetValueFromConfiguration(string key, ConfigurationTypes type)
        {
            switch (type)
            {
                case ConfigurationTypes.ConnectionString:
                    if (ConfigurationManager.ConnectionStrings[key] == null) throw new ConfigurationNotFoundException(key);
                    return ConfigurationManager.ConnectionStrings[key].ConnectionString;
                case ConfigurationTypes.AppSetting:
                    if (ConfigurationManager.AppSettings[key] == null) throw new ConfigurationNotFoundException(key);
                    return ConfigurationManager.AppSettings[key];
                default:
                    throw new ConfigurationNotFoundException(key);
            }
        }

        public static TRequired GetExpressionBodyType<TParameter, TResult, TRequired>(Expression<Func<TParameter, TResult>> expression, Expression<Func<TParameter, TRequired>> expressionType, bool throwErrorIfFalse = true) where TRequired : class
        {
            TRequired currentExpressionType = expression.Body as TRequired;

            if (currentExpressionType == null && throwErrorIfFalse)
            {
                throw new NotMatchingExpressionTypeException(expression.Body.GetType().FullName, expressionType.ToString());
            }

            return currentExpressionType;
        }

        public static Parameter[] GetParametersFromExpression<T>(Expression<Func<T, bool>> expression)
        {
            List<Parameter> parameters = new List<Parameter>();

            GetParametersFromExpression((BinaryExpression)expression.Body, ref parameters);

            return parameters.ToArray();
        }

        private static void GetParametersFromExpression(BinaryExpression body, ref List<Parameter> parameters)
        {
            Type bodyType = body.GetType();
            Type leftType = body.Left.GetType();
            Type rightType = body.Right.GetType();

            if (bodyType.Name == "MethodBinaryExpression")
            {
                parameters.Add(GetNewParameter(body));
                return;
            }

            if (leftType.Name == "MethodBinaryExpression")
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Left));
            }
            else
            {
                GetParametersFromExpression((BinaryExpression)body.Left, ref parameters);
            }

            if (rightType.Name == "MethodBinaryExpression")
            {
                parameters.Add(GetNewParameter((BinaryExpression)body.Right));
            }
            else
            {
                GetParametersFromExpression((BinaryExpression)body.Right, ref parameters);
            }
        }

        private static Parameter GetNewParameter(BinaryExpression body)
        {
            MemberExpression leftExpression = (MemberExpression)body.Left;
            dynamic rightValue = GetRightValue(body.Right);

            return new Parameter(leftExpression.Member.Name, rightValue);
        }


        private static dynamic GetRightValue(Expression body)
        {
            dynamic right;
            Type bodyType = body.GetType();

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
