using OneData.Attributes;
using OneData.Models;
using System.Collections.Generic;

namespace OneData.Interfaces
{
    internal interface IValidatable
    {
        bool IsNewColumn(ColumnDefinition columnDefinition);
        bool IsColumnDataTypeChanged(ColumnDefinition columnDefinition, string sqlDataType);
        bool IsColumnRemoved(Dictionary<string, OneProperty> properties, string columnName);
        bool IsDefaultChanged(ColumnDefinition columnDefinition, OneProperty property);
        bool IsForeignKeyRulesChanged(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyName, ForeignKey foreignKeyAttribute);

        bool IsNullable(OneProperty property);
        bool IsUnique(IManageable model, string propertyName);
        bool IsUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName);
        bool IsDefault(IManageable model, string propertyName);
        bool IsDefault(ColumnDefinition columnDefinition);
        bool IsPrimaryKey(IManageable model, string propertyName);

        bool IsNowNullable(ColumnDefinition columnDefinition, OneProperty property);
        bool IsNowUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, OneProperty property);
        bool IsNowDefault(ColumnDefinition columnDefinition, OneProperty property);
        bool IsNowPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryKeyConstraintName, OneProperty property);
        bool IsNowForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, OneProperty property);

        bool IsNoLongerNullable(ColumnDefinition columnDefinition, OneProperty property);
        bool IsNoLongerUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, OneProperty property);
        bool IsNoLongerDefault(ColumnDefinition columnDefinition, OneProperty property);
        bool IsNoLongerPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, OneProperty property);
        bool IsNoLongerForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, OneProperty property);
    }
}
