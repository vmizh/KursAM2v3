#define NETFX
#if NETFX || (NETCORE && !NETSTANDARD2_0)
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace ServiceStack.Text;

public sealed class EmitReflectionOptimizer : ReflectionOptimizer
{
    private static EmitReflectionOptimizer provider;
    public static EmitReflectionOptimizer Provider => provider ??= new EmitReflectionOptimizer();
    private EmitReflectionOptimizer() { }

    public override Type UseType(Type type)
    {
        if (type.IsInterface || type.IsAbstract)
        {
            return DynamicProxy.GetInstanceFor(type).GetType();
        }

        return type;
    }

    internal static DynamicMethod CreateDynamicGetMethod<T>(MemberInfo memberInfo)
    {
        var memberType = memberInfo is FieldInfo ? "Field" : "Property";
        var name = $"_Get{memberType}[T]_{memberInfo.Name}_";
        var returnType = typeof(object);

        return !memberInfo.DeclaringType.IsInterface
            ? new DynamicMethod(name, returnType, new[] {typeof(T)}, memberInfo.DeclaringType, true)
            : new DynamicMethod(name, returnType, new[] {typeof(T)}, memberInfo.Module, true);
    }

    public override GetMemberDelegate CreateGetter(PropertyInfo propertyInfo)
    {
        var getter = CreateDynamicGetMethod(propertyInfo);

        var gen = getter.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);

        if (propertyInfo.DeclaringType.IsValueType)
        {
            gen.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        }

        var mi = propertyInfo.GetGetMethod(true);
        if (mi == null)
            return null;
        gen.Emit(mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);

        if (propertyInfo.PropertyType.IsValueType)
        {
            gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }

        gen.Emit(OpCodes.Ret);

        return (GetMemberDelegate) getter.CreateDelegate(typeof(GetMemberDelegate));
    }

    public override GetMemberDelegate<T> CreateGetter<T>(PropertyInfo propertyInfo)
    {
        var getter = CreateDynamicGetMethod<T>(propertyInfo);

        var gen = getter.GetILGenerator();
        var mi = propertyInfo.GetGetMethod(true);
        if (mi == null)
            return null;

        if (typeof(T).IsValueType)
        {
            gen.Emit(OpCodes.Ldarga_S, 0);

            if (typeof(T) != propertyInfo.DeclaringType)
            {
                gen.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }
        }
        else
        {
            gen.Emit(OpCodes.Ldarg_0);

            if (typeof(T) != propertyInfo.DeclaringType)
            {
                gen.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            }
        }

        gen.Emit(mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);

        if (propertyInfo.PropertyType.IsValueType)
        {
            gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }

        gen.Emit(OpCodes.Isinst, typeof(object));

        gen.Emit(OpCodes.Ret);

        return (GetMemberDelegate<T>) getter.CreateDelegate(typeof(GetMemberDelegate<T>));
    }

    public override SetMemberDelegate CreateSetter(PropertyInfo propertyInfo)
    {
        var mi = propertyInfo.GetSetMethod(true);
        if (mi == null)
            return null;

        var setter = CreateDynamicSetMethod(propertyInfo);

        var gen = setter.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);

        if (propertyInfo.DeclaringType.IsValueType)
        {
            gen.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        }

        gen.Emit(OpCodes.Ldarg_1);

        if (propertyInfo.PropertyType.IsValueType)
        {
            gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        }

        gen.EmitCall(mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, (Type[]) null);

        gen.Emit(OpCodes.Ret);

        return (SetMemberDelegate) setter.CreateDelegate(typeof(SetMemberDelegate));
    }

    public override SetMemberDelegate<T> CreateSetter<T>(PropertyInfo propertyInfo) =>
        ExpressionReflectionOptimizer.Provider.CreateSetter<T>(propertyInfo);


    public override GetMemberDelegate CreateGetter(FieldInfo fieldInfo)
    {
        var getter = CreateDynamicGetMethod(fieldInfo);

        var gen = getter.GetILGenerator();

        gen.Emit(OpCodes.Ldarg_0);

        if (fieldInfo.DeclaringType.IsValueType)
        {
            gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
        }

        gen.Emit(OpCodes.Ldfld, fieldInfo);

        if (fieldInfo.FieldType.IsValueType)
        {
            gen.Emit(OpCodes.Box, fieldInfo.FieldType);
        }

        gen.Emit(OpCodes.Ret);

        return (GetMemberDelegate) getter.CreateDelegate(typeof(GetMemberDelegate));
    }

    public override GetMemberDelegate<T> CreateGetter<T>(FieldInfo fieldInfo)
    {
        var getter = CreateDynamicGetMethod<T>(fieldInfo);

        var gen = getter.GetILGenerator();

        gen.Emit(OpCodes.Ldarg_0);

        gen.Emit(OpCodes.Ldfld, fieldInfo);

        if (fieldInfo.FieldType.IsValueType)
        {
            gen.Emit(OpCodes.Box, fieldInfo.FieldType);
        }

        gen.Emit(OpCodes.Ret);

        return (GetMemberDelegate<T>) getter.CreateDelegate(typeof(GetMemberDelegate<T>));
    }

    public override SetMemberDelegate CreateSetter(FieldInfo fieldInfo)
    {
        var setter = CreateDynamicSetMethod(fieldInfo);

        var gen = setter.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);

        if (fieldInfo.DeclaringType.IsValueType)
        {
            gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
        }

        gen.Emit(OpCodes.Ldarg_1);

        gen.Emit(fieldInfo.FieldType.IsClass
                ? OpCodes.Castclass
                : OpCodes.Unbox_Any,
            fieldInfo.FieldType);

        gen.Emit(OpCodes.Stfld, fieldInfo);
        gen.Emit(OpCodes.Ret);

        return (SetMemberDelegate) setter.CreateDelegate(typeof(SetMemberDelegate));
    }

    static readonly Type[] DynamicGetMethodArgs = {typeof(object)};

    internal static DynamicMethod CreateDynamicGetMethod(MemberInfo memberInfo)
    {
        var memberType = memberInfo is FieldInfo ? "Field" : "Property";
        var name = $"_Get{memberType}_{memberInfo.Name}_";
        var returnType = typeof(object);

        return !memberInfo.DeclaringType.IsInterface
            ? new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.DeclaringType, true)
            : new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.Module, true);
    }

    public override SetMemberDelegate<T> CreateSetter<T>(FieldInfo fieldInfo) =>
        ExpressionReflectionOptimizer.Provider.CreateSetter<T>(fieldInfo);

    public override SetMemberRefDelegate<T> CreateSetterRef<T>(FieldInfo fieldInfo) =>
        ExpressionReflectionOptimizer.Provider.CreateSetterRef<T>(fieldInfo);

    public override bool IsDynamic(Assembly assembly)
    {
        try
        {
            var isDynamic = assembly is AssemblyBuilder
                            || string.IsNullOrEmpty(assembly.Location);
            return isDynamic;
        }
        catch (NotSupportedException)
        {
            //Ignore assembly.Location not supported in a dynamic assembly.
            return true;
        }
    }

    public override EmptyCtorDelegate CreateConstructor(Type type)
    {
        var emptyCtor = type.GetConstructor(Type.EmptyTypes);
        if (emptyCtor != null)
        {
            var dm = new DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(ReflectionExtensions).Module, true);
            var ilgen = dm.GetILGenerator();
            ilgen.Emit(OpCodes.Nop);
            ilgen.Emit(OpCodes.Newobj, emptyCtor);
            ilgen.Emit(OpCodes.Ret);

            return (EmptyCtorDelegate) dm.CreateDelegate(typeof(EmptyCtorDelegate));
        }

        //Anonymous types don't have empty constructors
        return () => FormatterServices.GetUninitializedObject(type);
    }

    static readonly Type[] DynamicSetMethodArgs = {typeof(object), typeof(object)};

    internal static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
    {
        var memberType = memberInfo is FieldInfo ? "Field" : "Property";
        var name = $"_Set{memberType}_{memberInfo.Name}_";
        var returnType = typeof(void);

        return !memberInfo.DeclaringType.IsInterface
            ? new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.DeclaringType, true)
            : new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.Module, true);
    }
}
#endif
