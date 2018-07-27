using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DataAccess
{
    public class ClassBuilder
    {
        public static void CreateNewObject(string className, DataTable dataTable)
        {
            var myType = CompileResultType(className, dataTable);
            var myObject = Activator.CreateInstance(myType);
        }

        private static Type CompileResultType(string className, DataTable dataTable)
        {
            TypeBuilder typeBuilder = GetTypeBuilder(className);
            SetConstructors(typeBuilder);
            SetProperties(typeBuilder, dataTable);

            return typeBuilder.CreateType();
        }

        private static void SetProperties(TypeBuilder typeBuilder, DataTable dataTable)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                CreateProperty(typeBuilder, column.ColumnName, column.DataType);
            }
        }

        private static void SetConstructors(TypeBuilder typeBuilder)
        {
            ParameterInfo[] parameters;
            ConstructorBuilder constructorBuilder;

            FieldBuilder xField = typeBuilder.DefineField("x", typeof(int),
                                                          FieldAttributes.Public);
            FieldBuilder yField = typeBuilder.DefineField("y", typeof(int),
                                                               FieldAttributes.Public);
            FieldBuilder zField = typeBuilder.DefineField("z", typeof(int),
                                                               FieldAttributes.Public);

            //typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            //ConstructorInfo ctr = typeof(Object).GetConstructor(new Type[0]);

            foreach (ConstructorInfo constructor in typeof(Object).GetConstructors())
            {
                parameters = constructor.GetParameters();
                Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                Type[][] requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                Type[][] optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
                ILGenerator ctorIL = constructorBuilder.GetILGenerator();

                ctorIL.Emit(OpCodes.Ldarg_0);

                // Here, we wish to create an instance of System.Object by invoking its
                // constructor, as specified above.

                ctorIL.Emit(OpCodes.Call, constructor);

                // Now, we'll load the current instance ref in arg 0, along
                // with the value of parameter "x" stored in arg 1, into stfld.

                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_1);
                ctorIL.Emit(OpCodes.Stfld, xField);

                // Now, we store arg 2 "y" in the current instance with stfld.

                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_2);
                ctorIL.Emit(OpCodes.Stfld, yField);

                // Last of all, arg 3 "z" gets stored in the current instance.

                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_3);
                ctorIL.Emit(OpCodes.Stfld, zField);

                // Our work complete, we return.

                ctorIL.Emit(OpCodes.Ret);


                //for (var i = 0; i < parameters.Length; ++i)
                //{
                //    var parameter = parameters[i];
                //    var parameterBuilder = constructorBuilder.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
                //    if (((int)parameter.Attributes & (int)ParameterAttributes.HasDefault) != 0)
                //    {
                //        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                //    }
                //}

                // Este break es temporal para depurar con mayor facilidad y solo tener 1 constructor.
                break;
            }
        }


        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMethodBuilder.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMethodBuilder =
                typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMethodBuilder.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMethodBuilder);
            propertyBuilder.SetSetMethod(setPropMethodBuilder);
        }

        private static TypeBuilder GetTypeBuilder(string className)
        {
            var assemblyName = new AssemblyName(className);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(className,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return typeBuilder;
        }
    }
}
