using FastMember;
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
        private readonly TypeAccessor _accessor;

        public ModelValidation(ModelComposition modelComposition, TypeAccessor accessor)
        {
            _modelComposition = modelComposition;
            _accessor = accessor;
        }

        private OneProperty ConfigureOneProperty(PropertyInfo property)
        {
            HeaderName headerName = property.GetCustomAttribute<HeaderName>();
            OneProperty oneProperty = new OneProperty()
            {
                Name = headerName == null ? property.Name : headerName.Name,
                Accesor = _accessor,
                PropertyName = property.Name,
                PropertyType = property.PropertyType, 
                ReflectedType = property.ReflectedType,
                AllowNullAttribute = property.GetCustomAttribute<AllowNull>(),
                DataLengthAttribute = property.GetCustomAttribute<DataLength>(),
                AutoPropertyAttribute = property.GetCustomAttribute<AutoProperty>(),
                DateCreatedAttibute = property.GetCustomAttribute<DateCreated>(),
                DateModifiedAttribute = property.GetCustomAttribute<DateModified>(),
                DefaultAttribute = property.GetCustomAttribute<Default>(),
                ForeignDataAttribute = property.GetCustomAttribute<ForeignData>(),
                ForeignKeyAttribute = property.GetCustomAttribute<ForeignKey>(),
                PrimaryKeyAttribute = property.GetCustomAttribute<PrimaryKey>(),
                UniqueAttribute = property.GetCustomAttribute<Unique>(),
                UnmanagedPropertyAttribute = property.GetCustomAttribute<UnmanagedProperty>()
            };

            return oneProperty;
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
            foreach (PropertyInfo property in type.GetProperties())
            {
                OneProperty oneProperty = ConfigureOneProperty(property);

                _modelComposition.ManagedProperties.Add(oneProperty.Name, oneProperty);
                _modelComposition.FilteredProperties.Add(oneProperty.Name, oneProperty);
                foreach (CustomAttributeData attribute in property.CustomAttributes)
                {
                    switch (attribute.AttributeType.Name)
                    {
                        case nameof(UnmanagedProperty):
                            _modelComposition.UnmanagedProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.ManagedProperties.Remove(oneProperty.Name);
                            _modelComposition.FilteredProperties.Remove(oneProperty.Name);
                            break;
                        case nameof(AutoProperty):
                            _modelComposition.AutoProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.AutoPropertyAttributes.Add(oneProperty.Name, property.GetCustomAttribute<AutoProperty>());
                            _modelComposition.FilteredProperties.Remove(oneProperty.Name);
                            break;
                        case nameof(PrimaryKey):
                            _modelComposition.PrimaryKeyProperty = oneProperty;
                            _modelComposition.PrimaryKeyAttribute = property.GetCustomAttribute<PrimaryKey>();
                            break;
                        case nameof(DateCreated):
                            _modelComposition.DateCreatedProperty = oneProperty;
                            _modelComposition.AutoProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.AutoPropertyAttributes.Add(oneProperty.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            _modelComposition.FilteredProperties.Remove(oneProperty.Name);
                            break;
                        case nameof(DateModified):
                            _modelComposition.DateModifiedProperty = oneProperty;
                            _modelComposition.AutoProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.AutoPropertyAttributes.Add(oneProperty.Name, new AutoProperty(AutoPropertyTypes.DateTime));
                            _modelComposition.FilteredProperties.Remove(oneProperty.Name);
                            break;
                        case nameof(ForeignKey):
                            _modelComposition.ForeignKeyProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.ForeignKeyAttributes.Add(oneProperty.Name, property.GetCustomAttribute<ForeignKey>());
                            break;
                        case nameof(Unique):
                            _modelComposition.UniqueKeyProperties.Add(oneProperty.Name, oneProperty);
                            break;
                        case nameof(AllowNull):
                            _modelComposition.AllowNullProperties.Add(oneProperty.Name, oneProperty);
                            break;
                        case nameof(Default):
                            _modelComposition.DefaultProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.DefaultAttributes.Add(oneProperty.Name, property.GetCustomAttribute<Default>());
                            break;
                        case nameof(DataLength):
                            _modelComposition.DataLengthProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.DataLengthAttributes.Add(oneProperty.Name, property.GetCustomAttribute<DataLength>());
                            break;
                        case nameof(ForeignData):
                            _modelComposition.ForeignDataProperties.Add(oneProperty.Name, oneProperty);
                            _modelComposition.ForeignDataAttributes.Add(oneProperty.Name, ConfigureForeignDataAttribute(property.GetCustomAttribute<ForeignData>(), oneProperty));
                            _modelComposition.ManagedProperties.Remove(oneProperty.Name);
                            _modelComposition.FilteredProperties.Remove(oneProperty.Name);
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
                if (!_modelComposition.PrimaryKeyProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(_modelComposition.PrimaryKeyProperty.PropertyType) != null || _modelComposition.PrimaryKeyProperty.AllowNullAttribute != null)
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

        private ForeignData ConfigureForeignDataAttribute(ForeignData foreignData, OneProperty oneProperty)
        {
            if (foreignData.ReferenceModel == null)
            {
                foreignData.ReferenceModel = oneProperty.ReflectedType;
                foreignData.ReferenceIdName = $"{foreignData.JoinModel.Name}Id";
            }
            foreignData.PropertyName = oneProperty.Name;

            return foreignData;
        }
    }
}
