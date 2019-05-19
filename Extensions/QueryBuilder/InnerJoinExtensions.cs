using OneData.Models.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OneData.Extensions.QueryBuilder
{
    public static class InnerJoinExtensions
    {
        public static WhereClause<T> Where<T>(this InnerJoinKeyword innerJoin, params Expression<Func<T, bool>>[] conditions)
        {
            return new WhereClause<T>();
        }

        public static InnerJoinKeyword InnerJoin<T, J>(this InnerJoinKeyword selectStatement, Expression<Func<T, dynamic>> id)
        {
            return new InnerJoinKeyword();
        }
    }
}
