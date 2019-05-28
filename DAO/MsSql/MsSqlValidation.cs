using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections.Generic;

namespace OneData.DAO.MsSql
{
    internal class MsSqlValidation : IValidatable
    {
        public bool IsNewColumn(ColumnDefinition columnDefinition)
        {
            return columnDefinition == null;
        }

        public bool IsColumnDataTypeChanged(ColumnDefinition columnDefinition, string sqlDataType)
        {
            string columnMax = columnDefinition.Character_Maximum_Length != null ? $"({columnDefinition.Character_Maximum_Length})" : string.Empty;
            return columnDefinition.Data_Type == null ? false : $"{columnDefinition.Data_Type}{columnMax}" != sqlDataType;
        }

        public bool IsColumnRemoved(Dictionary<string, OneProperty> properties, string columnName)
        {
            return !properties.ContainsKey(columnName);
        }

        public bool IsNowNullable(ColumnDefinition columnDefinition, OneProperty property)
        {
            return (Nullable.GetUnderlyingType(property.PropertyType) != null || property.AllowNullAttribute != null) && (columnDefinition.Is_Nullable == "NO" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNowUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, OneProperty property)
        {
            return !constraints.ContainsKey(uniqueConstraintName) && property.UniqueAttribute != null;
        }

        public bool IsNowDefault(ColumnDefinition columnDefinition, OneProperty property)
        {
            return string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) && property.DefaultAttribute != null;
        }

        public bool IsNowPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryKeyConstraintName, OneProperty property)
        {
            return !constraints.ContainsKey(primaryKeyConstraintName) && property.PrimaryKeyAttribute != null;
        }

        public bool IsNowForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, OneProperty property)
        {
            return !constraints.ContainsKey(foreignKeyConstraintName) && property.ForeignKeyAttribute != null;
        }

        public bool IsNoLongerNullable(ColumnDefinition columnDefinition, OneProperty property)
        {
            return (Nullable.GetUnderlyingType(property.PropertyType) == null && property.AllowNullAttribute == null) && (columnDefinition.Is_Nullable == "YES" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNoLongerUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, OneProperty property)
        {
            return constraints.ContainsKey(uniqueConstraintName) && (property == null || property.UniqueAttribute == null);
        }

        public bool IsNoLongerDefault(ColumnDefinition columnDefinition, OneProperty property)
        {
            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) && (property == null || property.DefaultAttribute == null);
        }

        public bool IsNoLongerPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryConstraintName, OneProperty property)
        {
            return constraints.ContainsKey(primaryConstraintName) && property.PrimaryKeyAttribute == null;
        }

        public bool IsNoLongerForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, OneProperty property)
        {
            return constraints.ContainsKey(foreignKeyConstraintName) && property.ForeignKeyAttribute == null;
        }

        public bool IsDefaultChanged(ColumnDefinition columnDefinition, OneProperty property)
        {
            string currentDefaultValue = columnDefinition.Column_Default?.ToString().Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty);

            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) ? property.DefaultAttribute != null ? !currentDefaultValue.Equals($"{property.DefaultAttribute.Value}") : false : false;
        }

        public bool IsForeignKeyRulesChanged(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyName, ForeignKey foreignKeyAttribute)
        {
            if (constraints.TryGetValue(foreignKeyName, out ConstraintDefinition constraintDefinition) && foreignKeyAttribute != null)
            {
                return constraintDefinition.Update_Rule != foreignKeyAttribute.OnUpdate.ToString().Replace("_", " ") || constraintDefinition.Delete_Rule != foreignKeyAttribute.OnDelete.ToString().Replace("_", " ");
            }

            return false;
        }

        public bool IsNullable(OneProperty property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null || property.AllowNullAttribute != null;
        }

        public bool IsUnique(IManageable model, string propertyName)
        {
            return model.Composition.UniqueKeyProperties.ContainsKey(propertyName);
        }

        public bool IsUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName)
        {
            return constraints.ContainsKey(uniqueConstraintName);
        }

        public bool IsDefault(IManageable model, string propertyName)
        {
            return model.Composition.DefaultProperties.ContainsKey(propertyName);
        }

        public bool IsDefault(ColumnDefinition columnDefinition)
        {
            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString());
        }

        public bool IsPrimaryKey(IManageable model, string propertyName)
        {
            return model.Composition.PrimaryKeyProperty.Name.Equals(propertyName);
        }
    }
}
