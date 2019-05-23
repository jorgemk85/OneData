using OneData.Attributes;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneData.DAO.MySql
{
    internal class MySqlValidation : IValidatable
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

        public bool IsColumnRemoved(Dictionary<string, PropertyInfo> properties, string columnName)
        {
            return !properties.ContainsKey(columnName);
        }

        public bool IsNowNullable(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return (Nullable.GetUnderlyingType(property.PropertyType) != null || property.GetCustomAttribute<AllowNull>() != null) && (columnDefinition.Is_Nullable == "NO" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNowUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(uniqueConstraintName) && property.GetCustomAttribute<Unique>() != null;
        }

        public bool IsNowDefault(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) && property.GetCustomAttribute<Default>() != null;
        }

        public bool IsNowPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string primaryKeyConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(primaryKeyConstraintName) && property.GetCustomAttribute<PrimaryKey>() != null;
        }

        public bool IsNowForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property)
        {
            return !constraints.ContainsKey(foreignKeyConstraintName) && property.GetCustomAttribute<ForeignKey>() != null;
        }

        public bool IsNoLongerNullable(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return (Nullable.GetUnderlyingType(property.PropertyType) == null && property.GetCustomAttribute<AllowNull>() == null) && (columnDefinition.Is_Nullable == "YES" || columnDefinition.Is_Nullable == null);
        }

        public bool IsNoLongerUnique(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(uniqueConstraintName) && (property == null || property.GetCustomAttribute<Unique>() == null);
        }

        public bool IsNoLongerDefault(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) && (property == null || property.GetCustomAttribute<Default>() == null);
        }

        public bool IsNoLongerPrimaryKey(Dictionary<string, ConstraintDefinition> constraints, string uniqueConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(uniqueConstraintName) && property.GetCustomAttribute<PrimaryKey>() == null;
        }

        public bool IsNoLongerForeignKey(Dictionary<string, ConstraintDefinition> constraints, string foreignKeyConstraintName, PropertyInfo property)
        {
            return constraints.ContainsKey(foreignKeyConstraintName) && property.GetCustomAttribute<ForeignKey>() == null;
        }

        public bool IsDefaultChanged(ColumnDefinition columnDefinition, PropertyInfo property)
        {
            Default defaultValueAttribute = property.GetCustomAttribute<Default>();
            string currentDefaultValue = columnDefinition.Column_Default?.ToString().Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty);

            return !string.IsNullOrWhiteSpace(columnDefinition.Column_Default?.ToString()) ? defaultValueAttribute != null ? !currentDefaultValue.Equals($"{defaultValueAttribute.Value}") : false : false;
        }

        public bool IsNullable(PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null || property.GetCustomAttribute<AllowNull>() != null;
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
