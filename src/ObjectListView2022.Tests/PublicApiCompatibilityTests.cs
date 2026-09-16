using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class PublicApiCompatibilityTests
    {
        [TestMethod]
        public void CurrentAssembly_PreservesObjectListView291ConsumerApi()
        {
            var baselinePath = Path.Combine(
                AppContext.BaseDirectory,
                "Baseline",
                "ObjectListView.2.9.1.dll");

            Assert.IsTrue(
                File.Exists(baselinePath),
                $"The ObjectListView 2.9.1 API baseline was not copied to '{baselinePath}'.");

            using var baseline = AssemblyDefinition.ReadAssembly(baselinePath);
            using var current = AssemblyDefinition.ReadAssembly(typeof(ObjectListView).Assembly.Location);

            var failures = ComparePublicApi(baseline.MainModule, current.MainModule);
            if (failures.Count == 0)
            {
                return;
            }

            const int maximumReportedFailures = 100;
            var details = string.Join(
                Environment.NewLine,
                failures.Take(maximumReportedFailures).Select(x => " - " + x));
            var suffix = failures.Count > maximumReportedFailures
                ? $"{Environment.NewLine} - ... {failures.Count - maximumReportedFailures} additional incompatibilities omitted."
                : string.Empty;

            Assert.Fail(
                $"The current assembly is not source-compatible with the ObjectListView 2.9.1 consumer API. " +
                $"Found {failures.Count} incompatibility/incompatibilities:{Environment.NewLine}{details}{suffix}");
        }

        private static List<string> ComparePublicApi(ModuleDefinition baseline, ModuleDefinition current)
        {
            var failures = new List<string>();
            var currentTypes = GetConsumerVisibleTypes(current)
                .ToDictionary(x => x.FullName, StringComparer.Ordinal);

            foreach (var baselineType in GetConsumerVisibleTypes(baseline).OrderBy(x => x.FullName, StringComparer.Ordinal))
            {
                if (!currentTypes.TryGetValue(baselineType.FullName, out var currentType))
                {
                    failures.Add($"Missing type: {baselineType.FullName}");
                    continue;
                }

                CompareType(baselineType, currentType, failures);
            }

            return failures;
        }

        private static void CompareType(TypeDefinition baseline, TypeDefinition current, ICollection<string> failures)
        {
            var typeName = baseline.FullName;

            if (!VisibilitySatisfies(baseline, current))
            {
                failures.Add($"Narrowed type accessibility: {typeName}");
            }

            if (baseline.IsInterface != current.IsInterface ||
                baseline.IsEnum != current.IsEnum ||
                baseline.IsValueType != current.IsValueType)
            {
                failures.Add($"Changed type kind: {typeName}");
            }

            if (!baseline.IsSealed && current.IsSealed)
            {
                failures.Add($"Type became sealed: {typeName}");
            }

            if (!baseline.IsAbstract && current.IsAbstract && !baseline.IsInterface)
            {
                failures.Add($"Type became abstract: {typeName}");
            }

            if (!BaseTypeSatisfies(baseline.BaseType, current))
            {
                failures.Add(
                    $"Changed base type incompatibly: {typeName} ({TypeName(baseline.BaseType)} -> {TypeName(current.BaseType)})");
            }

            var currentInterfaces = new HashSet<string>(GetAllInterfaceNames(current), StringComparer.Ordinal);
            foreach (var baselineInterface in baseline.Interfaces)
            {
                if (!currentInterfaces.Contains(TypeName(baselineInterface.InterfaceType)))
                {
                    failures.Add($"Removed interface from {typeName}: {TypeName(baselineInterface.InterfaceType)}");
                }
            }

            CompareGenericParameters(typeName, baseline.GenericParameters, current.GenericParameters, failures);
            CompareFields(baseline, current, failures);
            CompareMethods(baseline, current, failures);
            CompareProperties(baseline, current, failures);
            CompareEvents(baseline, current, failures);
            CompareDefaultMemberAttribute(baseline, current, failures);
        }

        private static void CompareFields(TypeDefinition baseline, TypeDefinition current, ICollection<string> failures)
        {
            var currentFields = GetTypeHierarchy(current)
                .SelectMany(x => x.Fields)
                .Where(IsConsumerVisible)
                .GroupBy(FieldKey, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

            foreach (var field in baseline.Fields.Where(IsConsumerVisible))
            {
                var key = FieldKey(field);
                if (!currentFields.TryGetValue(key, out var currentField))
                {
                    failures.Add($"Missing field: {baseline.FullName}.{field.Name} : {TypeName(field.FieldType)}");
                    continue;
                }

                if (!VisibilitySatisfies(field, currentField))
                {
                    failures.Add($"Narrowed field accessibility: {baseline.FullName}.{field.Name}");
                }

                if (field.IsStatic != currentField.IsStatic)
                {
                    failures.Add($"Changed field staticness: {baseline.FullName}.{field.Name}");
                }

                if (!field.IsInitOnly && currentField.IsInitOnly)
                {
                    failures.Add($"Field became readonly: {baseline.FullName}.{field.Name}");
                }

                if (field.IsLiteral)
                {
                    if (!currentField.IsLiteral || !Equals(field.Constant, currentField.Constant))
                    {
                        failures.Add($"Changed constant value: {baseline.FullName}.{field.Name}");
                    }
                }
            }
        }

        private static void CompareMethods(TypeDefinition baseline, TypeDefinition current, ICollection<string> failures)
        {
            var inheritedMethods = GetTypeHierarchy(current)
                .SelectMany(x => x.Methods)
                .Where(IsConsumerVisible)
                .Where(x => !x.IsConstructor);
            var constructorMethods = current.Methods
                .Where(IsConsumerVisible)
                .Where(x => x.IsConstructor);
            var currentMethods = inheritedMethods
                .Concat(constructorMethods)
                .GroupBy(MethodKey, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.Ordinal);

            foreach (var method in baseline.Methods.Where(IsConsumerVisible))
            {
                var key = MethodKey(method);
                if (!currentMethods.TryGetValue(key, out var candidates) || candidates.Count == 0)
                {
                    failures.Add($"Missing method: {baseline.FullName}.{DisplayMethod(method)}");
                    continue;
                }

                var currentMethod = candidates[0];
                if (!VisibilitySatisfies(method, currentMethod))
                {
                    failures.Add($"Narrowed method accessibility: {baseline.FullName}.{DisplayMethod(method)}");
                }

                if (method.IsStatic != currentMethod.IsStatic)
                {
                    failures.Add($"Changed method staticness: {baseline.FullName}.{DisplayMethod(method)}");
                }

                if (method.IsVirtual && !currentMethod.IsVirtual)
                {
                    failures.Add($"Virtual method became non-virtual: {baseline.FullName}.{DisplayMethod(method)}");
                }

                if (!method.IsAbstract && currentMethod.IsAbstract)
                {
                    failures.Add($"Method became abstract: {baseline.FullName}.{DisplayMethod(method)}");
                }

                CompareGenericParameters(
                    $"{baseline.FullName}.{DisplayMethod(method)}",
                    method.GenericParameters,
                    currentMethod.GenericParameters,
                    failures);

                CompareParameters(baseline.FullName, method, currentMethod, failures);

                if (HasAttribute(method, "System.Runtime.CompilerServices.ExtensionAttribute") &&
                    !HasAttribute(currentMethod, "System.Runtime.CompilerServices.ExtensionAttribute"))
                {
                    failures.Add($"Method is no longer an extension method: {baseline.FullName}.{DisplayMethod(method)}");
                }

                if (!HasErrorObsoleteAttribute(method) && HasErrorObsoleteAttribute(currentMethod))
                {
                    failures.Add($"Method became compile-time obsolete error: {baseline.FullName}.{DisplayMethod(method)}");
                }
            }
        }

        private static void CompareParameters(
            string declaringType,
            MethodDefinition baseline,
            MethodDefinition current,
            ICollection<string> failures)
        {
            for (var i = 0; i < baseline.Parameters.Count; i++)
            {
                var baselineParameter = baseline.Parameters[i];
                var currentParameter = current.Parameters[i];
                var parameterName = $"{declaringType}.{baseline.Name} parameter {i + 1}";

                if (baselineParameter.IsOut != currentParameter.IsOut || baselineParameter.IsIn != currentParameter.IsIn)
                {
                    failures.Add($"Changed ref/in/out contract: {parameterName}");
                }

                if (baselineParameter.IsOptional && !currentParameter.IsOptional)
                {
                    failures.Add($"Optional parameter became required: {parameterName}");
                }

                if (baselineParameter.HasConstant &&
                    (!currentParameter.HasConstant || !Equals(baselineParameter.Constant, currentParameter.Constant)))
                {
                    failures.Add($"Changed default parameter value: {parameterName}");
                }

                if (HasAttribute(baselineParameter, "System.ParamArrayAttribute") &&
                    !HasAttribute(currentParameter, "System.ParamArrayAttribute"))
                {
                    failures.Add($"params parameter lost ParamArrayAttribute: {parameterName}");
                }
            }
        }

        private static void CompareProperties(TypeDefinition baseline, TypeDefinition current, ICollection<string> failures)
        {
            var currentProperties = GetTypeHierarchy(current)
                .SelectMany(x => x.Properties)
                .Where(IsConsumerVisible)
                .GroupBy(PropertyKey, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
            foreach (var property in baseline.Properties.Where(IsConsumerVisible))
            {
                if (!currentProperties.ContainsKey(PropertyKey(property)))
                {
                    failures.Add($"Missing property: {baseline.FullName}.{DisplayProperty(property)}");
                }
            }
        }

        private static void CompareEvents(TypeDefinition baseline, TypeDefinition current, ICollection<string> failures)
        {
            var currentEvents = GetTypeHierarchy(current)
                .SelectMany(x => x.Events)
                .Where(IsConsumerVisible)
                .GroupBy(EventKey, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
            foreach (var eventDefinition in baseline.Events.Where(IsConsumerVisible))
            {
                if (!currentEvents.ContainsKey(EventKey(eventDefinition)))
                {
                    failures.Add(
                        $"Missing event: {baseline.FullName}.{eventDefinition.Name} : {TypeName(eventDefinition.EventType)}");
                }
            }
        }

        private static void CompareGenericParameters(
            string owner,
            IList<GenericParameter> baseline,
            IList<GenericParameter> current,
            ICollection<string> failures)
        {
            if (baseline.Count != current.Count)
            {
                failures.Add($"Changed generic arity: {owner}");
                return;
            }

            const GenericParameterAttributes specialConstraints =
                GenericParameterAttributes.ReferenceTypeConstraint |
                GenericParameterAttributes.NotNullableValueTypeConstraint |
                GenericParameterAttributes.DefaultConstructorConstraint;

            for (var i = 0; i < baseline.Count; i++)
            {
                var baselineParameter = baseline[i];
                var currentParameter = current[i];
                var addedSpecialConstraints =
                    (currentParameter.Attributes & specialConstraints) &
                    ~(baselineParameter.Attributes & specialConstraints);
                if (addedSpecialConstraints != 0)
                {
                    failures.Add($"Tightened generic constraints: {owner} generic parameter {i + 1}");
                }

                var baselineConstraints = new HashSet<string>(
                    baselineParameter.Constraints.Select(x => TypeName(x.ConstraintType)),
                    StringComparer.Ordinal);
                foreach (var currentConstraint in currentParameter.Constraints)
                {
                    if (!baselineConstraints.Contains(TypeName(currentConstraint.ConstraintType)))
                    {
                        failures.Add(
                            $"Added generic type constraint: {owner} generic parameter {i + 1} -> " +
                            TypeName(currentConstraint.ConstraintType));
                    }
                }
            }
        }

        private static void CompareDefaultMemberAttribute(
            TypeDefinition baseline,
            TypeDefinition current,
            ICollection<string> failures)
        {
            var baselineDefaultMember = GetStringAttributeArgument(baseline, "System.Reflection.DefaultMemberAttribute");
            if (baselineDefaultMember == null)
            {
                return;
            }

            var currentDefaultMember = GetStringAttributeArgument(current, "System.Reflection.DefaultMemberAttribute");
            if (!string.Equals(baselineDefaultMember, currentDefaultMember, StringComparison.Ordinal))
            {
                failures.Add(
                    $"Changed default member: {baseline.FullName} ({baselineDefaultMember} -> {currentDefaultMember ?? "<none>"})");
            }
        }

        private static bool BaseTypeSatisfies(TypeReference baselineBaseType, TypeDefinition current)
        {
            if (baselineBaseType == null)
            {
                return current.BaseType == null;
            }

            foreach (var type in GetTypeHierarchy(current).Skip(1))
            {
                if (string.Equals(type.FullName, TypeName(baselineBaseType), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            var baseType = current.BaseType;
            while (baseType != null)
            {
                if (SameType(baselineBaseType, baseType))
                {
                    return true;
                }

                if (!TryResolve(baseType, out var resolved))
                {
                    break;
                }

                baseType = resolved.BaseType;
            }

            return false;
        }

        private static IEnumerable<string> GetAllInterfaceNames(TypeDefinition type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var hierarchyType in GetTypeHierarchy(type))
            {
                foreach (var interfaceImplementation in hierarchyType.Interfaces)
                {
                    foreach (var interfaceName in GetInterfaceNames(interfaceImplementation.InterfaceType, seen))
                    {
                        yield return interfaceName;
                    }
                }
            }
        }

        private static IEnumerable<string> GetInterfaceNames(TypeReference interfaceType, ISet<string> seen)
        {
            var name = TypeName(interfaceType);
            if (!seen.Add(name))
            {
                yield break;
            }

            yield return name;
            if (!TryResolve(interfaceType, out var resolved))
            {
                yield break;
            }

            foreach (var parent in resolved.Interfaces)
            {
                foreach (var parentName in GetInterfaceNames(parent.InterfaceType, seen))
                {
                    yield return parentName;
                }
            }
        }

        private static IEnumerable<TypeDefinition> GetTypeHierarchy(TypeDefinition type)
        {
            for (var current = type; current != null;)
            {
                yield return current;
                if (current.BaseType == null || !TryResolve(current.BaseType, out var resolved))
                {
                    yield break;
                }

                current = resolved;
            }
        }

        private static bool TryResolve(TypeReference type, out TypeDefinition definition)
        {
            try
            {
                definition = type.Resolve();
                return definition != null;
            }
            catch (AssemblyResolutionException)
            {
                definition = null;
                return false;
            }
            catch (ResolutionException)
            {
                definition = null;
                return false;
            }
        }

        private static IEnumerable<TypeDefinition> GetConsumerVisibleTypes(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                foreach (var visibleType in GetConsumerVisibleTypes(type))
                {
                    yield return visibleType;
                }
            }
        }

        private static IEnumerable<TypeDefinition> GetConsumerVisibleTypes(TypeDefinition type)
        {
            if (IsConsumerVisible(type))
            {
                yield return type;
            }

            foreach (var nestedType in type.NestedTypes)
            {
                foreach (var visibleType in GetConsumerVisibleTypes(nestedType))
                {
                    yield return visibleType;
                }
            }
        }

        private static bool IsConsumerVisible(TypeDefinition type) =>
            type.IsPublic || type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamilyOrAssembly;

        private static bool IsConsumerVisible(FieldDefinition field) =>
            field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly;

        private static bool IsConsumerVisible(MethodDefinition method) =>
            method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

        private static bool IsConsumerVisible(PropertyDefinition property) =>
            (property.GetMethod != null && IsConsumerVisible(property.GetMethod)) ||
            (property.SetMethod != null && IsConsumerVisible(property.SetMethod));

        private static bool IsConsumerVisible(EventDefinition eventDefinition) =>
            (eventDefinition.AddMethod != null && IsConsumerVisible(eventDefinition.AddMethod)) ||
            (eventDefinition.RemoveMethod != null && IsConsumerVisible(eventDefinition.RemoveMethod));

        private static bool VisibilitySatisfies(TypeDefinition baseline, TypeDefinition current)
        {
            if (baseline.IsPublic)
            {
                return current.IsPublic;
            }

            if (baseline.IsNestedPublic)
            {
                return current.IsNestedPublic;
            }

            return current.IsNestedPublic || current.IsNestedFamily || current.IsNestedFamilyOrAssembly;
        }

        private static bool VisibilitySatisfies(FieldDefinition baseline, FieldDefinition current)
        {
            if (baseline.IsPublic)
            {
                return current.IsPublic;
            }

            return current.IsPublic || current.IsFamily || current.IsFamilyOrAssembly;
        }

        private static bool VisibilitySatisfies(MethodDefinition baseline, MethodDefinition current)
        {
            if (baseline.IsPublic)
            {
                return current.IsPublic;
            }

            return current.IsPublic || current.IsFamily || current.IsFamilyOrAssembly;
        }

        private static string FieldKey(FieldDefinition field) =>
            field.Name + ":" + TypeName(field.FieldType);

        private static string MethodKey(MethodDefinition method) =>
            method.Name + "`" + method.GenericParameters.Count + "(" +
            string.Join(",", method.Parameters.Select(x => TypeName(x.ParameterType))) + ")->" +
            TypeName(method.ReturnType);

        private static string PropertyKey(PropertyDefinition property) =>
            property.Name + "(" +
            string.Join(",", property.Parameters.Select(x => TypeName(x.ParameterType))) + "):" +
            TypeName(property.PropertyType);

        private static string EventKey(EventDefinition eventDefinition) =>
            eventDefinition.Name + ":" + TypeName(eventDefinition.EventType);

        private static string DisplayMethod(MethodDefinition method) =>
            method.Name + "(" + string.Join(", ", method.Parameters.Select(x => TypeName(x.ParameterType))) + ")";

        private static string DisplayProperty(PropertyDefinition property) =>
            property.Name +
            (property.Parameters.Count == 0
                ? string.Empty
                : "[" + string.Join(", ", property.Parameters.Select(x => TypeName(x.ParameterType))) + "]") +
            " : " + TypeName(property.PropertyType);

        private static bool SameType(TypeReference left, TypeReference right) =>
            string.Equals(TypeName(left), TypeName(right), StringComparison.Ordinal);

        private static string TypeName(TypeReference type) => type?.FullName ?? "<none>";

        private static bool HasAttribute(ICustomAttributeProvider provider, string attributeType) =>
            provider.HasCustomAttributes &&
            provider.CustomAttributes.Any(x => x.AttributeType.FullName == attributeType);

        private static bool HasErrorObsoleteAttribute(ICustomAttributeProvider provider)
        {
            if (!provider.HasCustomAttributes)
            {
                return false;
            }

            var attribute = provider.CustomAttributes.FirstOrDefault(
                x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            if (attribute == null || attribute.ConstructorArguments.Count < 2)
            {
                return false;
            }

            return attribute.ConstructorArguments[1].Value is bool isError && isError;
        }

        private static string GetStringAttributeArgument(ICustomAttributeProvider provider, string attributeType)
        {
            if (!provider.HasCustomAttributes)
            {
                return null;
            }

            var attribute = provider.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == attributeType);
            if (attribute == null || attribute.ConstructorArguments.Count == 0)
            {
                return null;
            }

            return attribute.ConstructorArguments[0].Value as string;
        }
    }
}
