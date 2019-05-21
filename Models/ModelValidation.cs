using OneData.Attributes;
using OneData.DAO;
using OneData.Enums;
using OneData.Exceptions;
using System;
using System.Reflection;

namespace OneData.Models
{
    internal sealed class ModelValidation
    {
        private readonly ModelComposition _modelComposition;

        public ModelValidation(ModelComposition modelComposition)
        {
            _modelComposition = modelComposition;
        }

        internal void ValidateAndConfigureClass(Type type)
        {
            DataTable dataTableAttribute;
            CacheEnabled cacheEnabledAttribute;

            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                switch (attribute.AttributeType.Name)
                {
                    case nameof(DataTable):
                        dataTableAttribute = type.GetCustomAttribute<DataTable>();
                        _modelComposition.TableName = dataTableAttribute.TableName;
                        _modelComposition.Schema = string.IsNullOrWhiteSpace(dataTableAttribute.Schema) ? Manager.DefaultSchema : dataTableAttribute.Schema;
                        _modelComposition.FullyQualifiedTableName = Manager.ConnectionType == ConnectionTypes.MSSQL ? $"[{Manager.DefaultSchema}.{Manager.TablePrefix}{_modelComposition.TableName}]" : $"`{Manager.TablePrefix}{_modelComposition.TableName}`";
                        break;
                    case nameof(CacheEnabled):
                        cacheEnabledAttribute = type.GetCustomAttribute<CacheEnabled>();
                        _modelComposition.IsCacheEnabled = true;
                        _modelComposition.CacheExpiration = cacheEnabledAttribute.Expiration * TimeSpan.TicksPerSecond;
                        break;
                    default:
                        break;
                }
            }

            PerformClassValidation(type);
        }

        internal void ValidateAndConfigureProperties(Type type)
        {
            foreach (PropertyInfo property in _modelComposition.Properties)
            {
                _modelComposition.ManagedProperties.Add(property.Name, property);
                _modelComposition.FilteredProperties.Add(property.Name, property);
                foreach (CustomAttributeData attribute in property.CustomAttributes)
                {
                    switch (attribute.AttributeType.Name)
                    {
                        case nameof(UnmanagedProperty):
                            _modelComposition.UnmanagedProperties.Add(property.Name, property);
                            _modelComposition.ManagedProperties.Remove(property.Name);
                            _modelComposition.FilteredProperties.Remove(property.Name);
                            break;
                        case nameof(AutoProperty):
                            _modelComposition.AutoProperties.Add(property.Name, property);
                            _modelComposition.AutoPropertyAttributes.Add(property.Name, property.GetCustomAttribute<AutoProperty>());
                            _modelComposition.FilteredProperties.Remove(property.Name);
                            break;
                        case nameof(PrimaryKey):
                            _modelComposition.PrimaryKeyProperty = property;
                            _modelComposition.PrimaryKeyAttribute = property.GetCustomAttribute<PrimaryKey>();
                            break;
                        case nameof(DateCreated):
                            _modelComposition.DateCreatedProperty = property;
                            _modelComposition.AutoProperties.Add(property.Name, property);
                            _modelComposition.AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            _modelComposition.FilteredProperties.Remove(property.Name);
                            break;
                        case nameof(DateModified):
                            _modelComposition.DateModifiedProperty = property;
                            _modelComposition.AutoProperties.Add(property.Name, property);
                            _modelComposition.AutoPropertyAttributes.Add(property.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            _modelComposition.FilteredProperties.Remove(property.Name);
                            break;
                        case nameof(ForeignKey):
                            _modelComposition.ForeignKeyProperties.Add(property.Name, property);
                            _modelComposition.ForeignKeyAttributes.Add(property.Name, property.GetCustomAttribute<ForeignKey>());
                            break;
                        case nameof(Unique):
                            _modelComposition.UniqueKeyProperties.Add(property.Name, property);
                            break;
                        case nameof(AllowNull):
                            _modelComposition.AllowNullProperties.Add(property.Name, property);
                            break;
                        case nameof(Default):
                            _modelComposition.DefaultProperties.Add(property.Name, property);
                            _modelComposition.DefaultAttributes.Add(property.Name, property.GetCustomAttribute<Default>());
                            break;
                        case nameof(DataLength):
                            _modelComposition.DataLengthProperties.Add(property.Name, property);
                            _modelComposition.DataLengthAttributes.Add(property.Name, property.GetCustomAttribute<DataLength>());
                            break;
                        case nameof(ForeignData):
                            _modelComposition.ForeignDataProperties.Add(property.Name, property);
                            _modelComposition.ForeignDataAttributes.Add(property.Name, ConfigureForeignDataAttribute(property.GetCustomAttribute<ForeignData>(), property));
                            _modelComposition.ManagedProperties.Remove(property.Name);
                            _modelComposition.FilteredProperties.Remove(property.Name);
                            break;
                        default:
                            break;
                    }
                }
            }
            PerformPropertiesValidation(type);
        }

        private void PerformClassValidation(Type type)
        {
            if (string.IsNullOrWhiteSpace(_modelComposition.TableName))
            {
                throw new RequiredAttributeNotFound(nameof(DataTable), type.FullName);
            }
        }

        private void PerformPropertiesValidation(Type type)
        {
            if (_modelComposition.PrimaryKeyProperty == null)
            {
                throw new RequiredAttributeNotFound(nameof(PrimaryKey), type.FullName);
            }
            else
            {
                if (!_modelComposition.PrimaryKeyProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(_modelComposition.PrimaryKeyProperty.PropertyType) != null || _modelComposition.PrimaryKeyProperty.GetCustomAttribute<AllowNull>() != null)
                {
                    throw new InvalidDataType(_modelComposition.PrimaryKeyProperty.Name, type.FullName, "not nullable struct");
                }
                if (_modelComposition.PrimaryKeyAttribute.IsAutoIncrement && !_modelComposition.PrimaryKeyProperty.PropertyType.Equals(typeof(int)))
                {
                    throw new NotSupportedException($"PrimaryKey inside {type.FullName} is set to AutoIncrement but it's value type is not 'int'.");
                }
            }

            if (_modelComposition.DateCreatedProperty == null)
            {
                throw new RequiredAttributeNotFound(nameof(DateCreated), type.FullName);
            }
            else if (!_modelComposition.DateCreatedProperty.PropertyType.Equals(typeof(DateTime)) && !Nullable.GetUnderlyingType(_modelComposition.DateCreatedProperty.PropertyType).Equals(typeof(DateTime)))
            {
                throw new InvalidDataType(_modelComposition.DateCreatedProperty.Name, type.FullName, "DateTime");
            }

            if (_modelComposition.DateModifiedProperty == null)
            {
                throw new RequiredAttributeNotFound(nameof(DateModified), type.FullName);
            }
            else if (!_modelComposition.DateModifiedProperty.PropertyType.Equals(typeof(DateTime)) && !Nullable.GetUnderlyingType(_modelComposition.DateCreatedProperty.PropertyType).Equals(typeof(DateTime)))
            {
                throw new InvalidDataType(_modelComposition.DateModifiedProperty.Name, type.FullName, "DateTime");
            }
        }

        private ForeignData ConfigureForeignDataAttribute(ForeignData foreignData, PropertyInfo property)
        {
            if (foreignData.ReferenceModel == null)
            {
                foreignData.ReferenceModel = property.ReflectedType;
                foreignData.ReferenceIdName = $"{foreignData.JoinModel.Name}Id";
            }
            foreignData.PropertyName = property.Name;

            return foreignData;
        }
    }
}
