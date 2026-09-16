using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectListView2022.Tests
{
    [STATestClass]
    public class DesignerBehaviorRegressionTests
    {
        [TestMethod]
        public void ObjectListView_DesignSurfaceUsesObjectListViewDesigner()
        {
            using var surface = new DesignSurface(typeof(ObjectListView));
            var host = GetDesignerHost(surface);
            var control = (ObjectListView)host.RootComponent;

            var designer = host.GetDesigner(control);

            Assert.IsNotNull(designer);
            Assert.IsInstanceOfType<ObjectListViewDesigner>(designer);
        }

        [TestMethod]
        public void ObjectListViewDesigner_HidesUnsupportedListViewProperties()
        {
            using var surface = new DesignSurface(typeof(ObjectListView));
            var host = GetDesignerHost(surface);
            var control = (ObjectListView)host.RootComponent;
            var properties = TypeDescriptor.GetProperties(control);

            AssertHidden(properties, "BackgroundImage");
            AssertHidden(properties, "BackgroundImageTiled");
            AssertHidden(properties, "HotTracking");
            AssertHidden(properties, "HoverSelection");
            AssertHidden(properties, "LabelEdit");
            AssertHidden(properties, "VirtualListSize");
            AssertHidden(properties, "VirtualMode");
        }

        [TestMethod]
        public void ObjectListViewDesigner_HidesUnsupportedListViewEvents()
        {
            using var surface = new DesignSurface(typeof(ObjectListView));
            var host = GetDesignerHost(surface);
            var control = (ObjectListView)host.RootComponent;
            var events = TypeDescriptor.GetEvents(control);

            AssertHidden(events, "AfterLabelEdit");
            AssertHidden(events, "BeforeLabelEdit");
            AssertHidden(events, "DrawColumnHeader");
            AssertHidden(events, "DrawItem");
            AssertHidden(events, "DrawSubItem");
            AssertHidden(events, "RetrieveVirtualItem");
            AssertHidden(events, "SearchForVirtualItem");
            AssertHidden(events, "VirtualItemsSelectionRangeChanged");
        }

        [TestMethod]
        public void TreeListViewDesigner_HidesGroupingPropertiesAndEvents()
        {
            using var surface = new DesignSurface(typeof(TreeListView));
            var host = GetDesignerHost(surface);
            var control = (TreeListView)host.RootComponent;

            var properties = TypeDescriptor.GetProperties(control);
            AssertHidden(properties, "GroupImageList");
            AssertHidden(properties, "GroupWithItemCountFormat");
            AssertHidden(properties, "GroupWithItemCountSingularFormat");
            AssertHidden(properties, "HasCollapsibleGroups");
            AssertHidden(properties, "SpaceBetweenGroups");
            AssertHidden(properties, "ShowGroups");
            AssertHidden(properties, "SortGroupItemsByPrimaryColumn");
            AssertHidden(properties, "ShowItemCountOnGroups");

            var events = TypeDescriptor.GetEvents(control);
            AssertHidden(events, "AboutToCreateGroups");
            AssertHidden(events, "AfterCreatingGroups");
            AssertHidden(events, "BeforeCreatingGroups");
            AssertHidden(events, "GroupTaskClicked");
            AssertHidden(events, "GroupExpandingCollapsing");
            AssertHidden(events, "GroupStateChanged");
        }

        [TestMethod]
        public void ObjectListViewDesigner_PreservesColumnsEditorAndContentSerialization()
        {
            using var surface = new DesignSurface(typeof(ObjectListView));
            var host = GetDesignerHost(surface);
            var control = (ObjectListView)host.RootComponent;
            var columns = TypeDescriptor.GetProperties(control)[nameof(ObjectListView.Columns)];

            Assert.IsNotNull(columns);
            Assert.IsTrue(columns.IsBrowsable);

            var serialization = columns.Attributes[typeof(DesignerSerializationVisibilityAttribute)]
                as DesignerSerializationVisibilityAttribute;
            Assert.IsNotNull(serialization);
            Assert.AreEqual(DesignerSerializationVisibility.Content, serialization.Visibility);

            var editor = columns.Attributes[typeof(EditorAttribute)] as EditorAttribute;
            Assert.IsNotNull(editor);
            Assert.IsNotNull(ResolveObjectListViewType(editor.EditorTypeName));
        }

        [TestMethod]
        public void ObjectListViewDesigner_LargeIconViewRemainsSerializable()
        {
            using var surface = new DesignSurface(typeof(ObjectListView));
            var host = GetDesignerHost(surface);
            var control = (ObjectListView)host.RootComponent;
            var view = TypeDescriptor.GetProperties(control)[nameof(ObjectListView.View)];

            Assert.IsNotNull(view);

            control.View = View.LargeIcon;

            Assert.IsTrue(view.ShouldSerializeValue(control));
        }

        [TestMethod]
        public void LegacySelectionColorPropertiesRemainHiddenFromDesignerSerialization()
        {
            AssertLegacyPropertyHidden(nameof(ObjectListView.HighlightBackgroundColor));
            AssertLegacyPropertyHidden(nameof(ObjectListView.HighlightForegroundColor));
            AssertLegacyPropertyHidden(nameof(ObjectListView.UnfocusedHighlightBackgroundColor));
            AssertLegacyPropertyHidden(nameof(ObjectListView.UnfocusedHighlightForegroundColor));
        }

        private static IDesignerHost GetDesignerHost(DesignSurface surface)
        {
            var host = surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
            Assert.IsNotNull(host);
            Assert.IsNotNull(host.RootComponent);
            return host;
        }

        private static void AssertHidden(PropertyDescriptorCollection properties, string propertyName)
        {
            var property = properties[propertyName];
            Assert.IsNotNull(property, $"Expected design-time property '{propertyName}' to remain present.");
            Assert.IsFalse(property.IsBrowsable, $"Expected design-time property '{propertyName}' to be hidden.");
        }

        private static void AssertHidden(EventDescriptorCollection events, string eventName)
        {
            var eventDescriptor = events[eventName];
            Assert.IsNotNull(eventDescriptor, $"Expected design-time event '{eventName}' to remain present.");
            Assert.IsFalse(eventDescriptor.IsBrowsable, $"Expected design-time event '{eventName}' to be hidden.");
        }

        private static void AssertLegacyPropertyHidden(string propertyName)
        {
            var property = TypeDescriptor.GetProperties(typeof(ObjectListView))[propertyName];
            Assert.IsNotNull(property);
            Assert.IsFalse(property.IsBrowsable);

            var serialization = property.Attributes[typeof(DesignerSerializationVisibilityAttribute)]
                as DesignerSerializationVisibilityAttribute;
            Assert.IsNotNull(serialization);
            Assert.AreEqual(DesignerSerializationVisibility.Hidden, serialization.Visibility);
        }

        private static Type ResolveObjectListViewType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            var unqualifiedName = typeName.Split(',')[0].Trim();
            return typeof(ObjectListView).Assembly.GetType(unqualifiedName, false);
        }
    }
}
