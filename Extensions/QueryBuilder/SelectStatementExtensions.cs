using OneData.Models.QueryBuilder;
using System;
using System.Linq.Expressions;

namespace OneData.Extensions.QueryBuilder
{
    public static class SelectStatementExtensions
    {
        public static InnerJoinKeyword InnerJoin<T, J>(this SelectStatement<T> selectStatement, Expression<Func<T, dynamic>> parameters)
        {
            return new InnerJoinKeyword();
        }

        public static LeftJoinKeyword<T> LeftJoin<T>(this SelectStatement<T> selectStatement, Expression<Func<T, dynamic>> parameters)
        {
            return new LeftJoinKeyword<T>();
        }

        public static RightJoinKeyword<T> RightJoin<T>(this SelectStatement<T> selectStatement, Expression<Func<T, dynamic>> parameters)
        {
            return new RightJoinKeyword<T>();
        }

        public static WhereClause<T> Where<T>(this SelectStatement<T> selectStatement, Expression<Func<T, bool>> parameters)
        {
            return new WhereClause<T>();
        }
    }
}
