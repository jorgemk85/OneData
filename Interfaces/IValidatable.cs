using OneData.Models;
using System.Collections.Generic;
using System.Reflection;

namespace OneData.Interfaces
{
    internal interface IValidatable
    {
        bool IsNewColumn(ColumnDefinition columnDefinition);
        bool IsColumnDataTypeChanged(ColumnDefinition columnDefinition, string sqlDataType);
        bool IsColumnRemoved(Dictionary<string, PropertyInfo> properties, string columnName);
        bool IsDefaultChanged(ColumnDefinition columnDefinition, PropertyInfo property);

        bool IsNullable(PropertyInfo property);
        bool IsUnique(IManageable model, string propertyName);
        bool IsDefault(IManageable model, string propertyName);
        bool IsPrimaryKey(IManageable model, string propertyName);

        bool IsNowNullable(ColumnDefinition columnDefinition, PropertyInfo property);
        bool IsNowUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property);
        bool IsNowDefault(ColumnDefinition columnDefinition, PropertyInfo property);
        bool IsNowPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryKeyConstraintName, PropertyInfo property);
        bool IsNowForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property);

        bool IsNoLongerNullable(ColumnDefinition columnDefinition, PropertyInfo property);
        bool IsNoLongerUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property);
        bool IsNoLongerDefault(ColumnDefinition columnDefinition, PropertyInfo property);
        bool IsNoLongerPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property);
        bool IsNoLongerForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property);
    }
}
