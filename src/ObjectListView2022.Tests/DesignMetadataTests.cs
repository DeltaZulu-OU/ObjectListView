using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BrightIdeasSoftware;
using BrightIdeasSoftware.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [TestClass]
    public class DesignMetadataTests
    {
        [TestMethod]
        public void ColumnsEditorTypeResolves()
        {
            var property = TypeDescriptor.GetProperties(typeof(ObjectListView))[nameof(ObjectListView.Columns)];
            Assert.IsNotNull(property);

            var attribute = property.Attributes[typeof(EditorAttribute)] as EditorAttribute;
            Assert.IsNotNull(attribute);

            AssertObjectListViewTypeResolves(attribute.EditorTypeName);
        }

        [TestMethod]
        public void ImageOverlayConverterTypeResolves()
        {
            AssertConverterTypeResolves(typeof(ImageOverlay));
        }

        [TestMethod]
        public void TextOverlayConverterTypeResolves()
        {
            AssertConverterTypeResolves(typeof(TextOverlay));
        }

        [TestMethod]
        public void ObjectListViewMetadataDoesNotReferenceMissingBrightIdeasSoftwareTypes()
        {
            var assembly = typeof(ObjectListView).Assembly;
            var unresolved = new List<string>();

            foreach (var type in assembly.GetTypes())
            {
                InspectAttributes(assembly, type.CustomAttributes, type.FullName, unresolved);

                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.Instance | BindingFlags.Static))
                {
                    InspectAttributes(assembly, member.CustomAttributes,
                        type.FullName + "." + member.Name, unresolved);
                }
            }

            Assert.AreEqual(0, unresolved.Count,
                "Unresolved ObjectListView metadata type references:" + Environment.NewLine +
                string.Join(Environment.NewLine, unresolved));
        }

        private static void AssertConverterTypeResolves(Type decoratedType)
        {
            var attribute = TypeDescriptor.GetAttributes(decoratedType)[typeof(TypeConverterAttribute)] as TypeConverterAttribute;
            Assert.IsNotNull(attribute);
            AssertObjectListViewTypeResolves(attribute.ConverterTypeName);
        }

        private static void InspectAttributes(Assembly assembly,
            IEnumerable<CustomAttributeData> attributes,
            string owner,
            ICollection<string> unresolved)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeType != typeof(EditorAttribute) &&
                    attribute.AttributeType != typeof(TypeConverterAttribute))
                {
                    continue;
                }

                foreach (var argument in attribute.ConstructorArguments.Where(x => x.ArgumentType == typeof(string)))
                {
                    var typeName = argument.Value as string;
                    if (string.IsNullOrWhiteSpace(typeName) ||
                        !typeName.StartsWith("BrightIdeasSoftware.", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (ResolveType(assembly, typeName) == null)
                    {
                        unresolved.Add(owner + " -> " + typeName);
                    }
                }
            }
        }

        private static void AssertObjectListViewTypeResolves(string typeName)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(typeName));
            Assert.IsNotNull(ResolveType(typeof(ObjectListView).Assembly, typeName),
                "Could not resolve metadata type '" + typeName + "'.");
        }

        private static Type ResolveType(Assembly assembly, string typeName)
        {
            var unqualifiedName = typeName.Split(',')[0].Trim();
            return assembly.GetType(unqualifiedName, false);
        }
    }
}
