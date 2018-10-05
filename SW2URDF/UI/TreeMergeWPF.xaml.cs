﻿using log4net;
using SW2URDF.URDF;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SW2URDF.UI
{
    /// <summary>
    /// Interaction logic for TreeMergeWPF.xaml
    /// </summary>
    public partial class TreeMergeWPF : Window
    {
        private static readonly ILog logger = Logger.GetLogger();

        private static readonly int MAX_LABEL_CHARACTER_WIDTH = 40;
        private static readonly int MAX_BUTTON_CHARACTER_WIDTH = 20;

        private readonly string CSVFileName;
        private readonly string AssemblyName;

        public TreeMergeWPF(List<string> coordinateSystems, List<string> referenceAxes, string csvFileName, string assemblyName)
        {
            CSVFileName = csvFileName;
            AssemblyName = assemblyName;

            InitializeComponent();
            ConfigureMenus(coordinateSystems, referenceAxes);
            ConfigureLabels();
        }

        public void SetTrees(LinkNode existingNode, LinkNode loadedNode)
        {
            ExistingTreeView.Items.Clear();
            LoadedTreeView.Items.Clear();

            TreeViewItem existing = BuildTreeViewItem(existingNode);
            TreeViewItem loaded = BuildTreeViewItem(loadedNode);

            ExistingTreeView.MouseMove += TreeViewMouseMove;
            ExistingTreeView.Drop += TreeViewDrop;

            ExistingTreeView.Items.Add(existing);
            LoadedTreeView.Items.Add(loaded);

            ExistingTreeView.AllowDrop = true;
            LoadedTreeView.AllowDrop = true;
        }

        private string ShortenStringForLabel(string text, int numCharacters)
        {
            string result = text;
            if (text.Length > numCharacters)
            {
                string extension = Path.GetExtension(text);
                int numToKeep = numCharacters - "...".Length - extension.Length;
                result = text.Substring(0, numToKeep) + "..." + extension;
            }
            return result;
        }

        private TextBlock BuildTextBlock(string boldBit, string regularBit)
        {
            TextBlock block = new TextBlock();
            block.Inlines.Add(new Bold(new Run(boldBit)));
            block.Inlines.Add(regularBit);
            return block;
        }

        private void ConfigureLabels()
        {
            string longAssemblyName = ShortenStringForLabel(AssemblyName, MAX_LABEL_CHARACTER_WIDTH);
            string shortAssemblyName = ShortenStringForLabel(AssemblyName, MAX_BUTTON_CHARACTER_WIDTH);

            string longCSVFilename = ShortenStringForLabel(CSVFileName, MAX_LABEL_CHARACTER_WIDTH);
            string shortCSVFilename = ShortenStringForLabel(CSVFileName, MAX_BUTTON_CHARACTER_WIDTH);

            ExistingTreeLabel.Content = BuildTextBlock("Configuration from Assembly: ", longAssemblyName);
            ExistingTreeLabel.ToolTip =
                new TextBlock { Text = "Configuration from Assembly: " + AssemblyName };

            LoadedTreeLabel.Content = BuildTextBlock("Configuration from CSV: ", longCSVFilename);
            LoadedTreeLabel.ToolTip =
                new TextBlock { Text = "Configuration from CSV: " + CSVFileName };

            MassInertiaExistingButton.Content = new TextBlock { Text = shortAssemblyName };
            MassInertiaExistingButton.ToolTip =
                new TextBlock { Text = "Use Mass and Inertia properties loaded from: " + AssemblyName };

            VisualExistingButton.Content = new TextBlock { Text = shortAssemblyName };
            VisualExistingButton.ToolTip =
                new TextBlock { Text = "Use Mesh and Material properties loaded from: " + AssemblyName };

            JointKinematicsExistingButton.Content = new TextBlock { Text = shortAssemblyName };
            JointKinematicsExistingButton.ToolTip =
                new TextBlock { Text = "Use Joint Kinematic properties loaded from: " + AssemblyName };

            OtherJointExistingButton.Content = new TextBlock { Text = shortAssemblyName };
            OtherJointExistingButton.ToolTip = new TextBlock
            {
                Text =
                "Use Limits, Dynamics, Calibration and Safety Controller values loaded from: " +
                AssemblyName
            };

            MassInertiaLoadedButton.Content = new TextBlock { Text = shortCSVFilename };
            MassInertiaLoadedButton.ToolTip =
                new TextBlock { Text = "Use Mass and Inertia properties loaded from: " + CSVFileName };

            VisualLoadedButton.Content = new TextBlock { Text = shortCSVFilename };
            VisualLoadedButton.ToolTip =
                new TextBlock { Text = "Use Mesh and Material properties loaded from: " + CSVFileName };

            JointKinematicsLoadedButton.Content = new TextBlock { Text = shortCSVFilename };
            JointKinematicsLoadedButton.ToolTip =
                new TextBlock { Text = "Use Joint Kinematic properties loaded from: " + CSVFileName };

            OtherJointLoadedButton.Content = new TextBlock { Text = shortCSVFilename };
            OtherJointLoadedButton.ToolTip = new TextBlock
            {
                Text = "Use Limits, Dynamics, Calibration and Safety Controller values loaded from: " + CSVFileName
            };
        }

        private void ProcessDragDrop(TreeView treeView, TreeViewItem target, TreeViewItem package)
        {
            if (package.Parent == treeView)
            {
            }
            else if (package.Parent.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem packageParent = (TreeViewItem)package.Parent;

                packageParent.Items.Remove(package);

                target.Items.Add(package);
            }
            else
            {
                logger.Warn("Unhandled package parent " + package.Parent.GetType());
            }
        }

        private void TreeViewDrop(object sender, DragEventArgs e)
        {
            TreeViewItem package = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;
            if (package != null & package != e.Source)
            {
                if (e.Source.GetType() == typeof(TreeViewItem))
                {
                    ProcessDragDrop((TreeView)sender, (TreeViewItem)e.Source, package);
                }
                else if (e.Source.GetType() == typeof(TreeView))
                {
                }
                else
                {
                    logger.Warn("Unhandled drop target " + e.Source.GetType());
                }
            }
        }

        private void TreeViewMouseMove(object sender, MouseEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                DependencyObject dependencyObject = treeView.InputHitTest(e.GetPosition(treeView)) as DependencyObject;

                if (treeView.SelectedValue != null)
                {
                    DragDrop.DoDragDrop(treeView, treeView.SelectedValue, DragDropEffects.Move);
                    e.Handled = true;
                }
            }
        }

        private void TreeViewClick(object sender, MouseButtonEventArgs e)
        {
            TreeView treeView = sender as TreeView;
        }

        private TreeViewItem BuildTreeViewItem(LinkNode node)
        {
            TreeViewItem item = new TreeViewItem
            {
                Tag = node.Link,
                IsExpanded = true,
                AllowDrop = true,
                Name = node.Name,
                Header = node.Name,
            };

            foreach (LinkNode child in node.Nodes)
            {
                item.Items.Add(BuildTreeViewItem(child));
            }

            return item;
        }

        private void ConfigureMenus(List<string> coordinateSystems, List<string> referenceAxes)
        {
            SetMenu(ExistingCoordinatesMenu, coordinateSystems);
            SetMenu(ExistingAxisMenu, referenceAxes);
        }

        private void SetMenu(Button button, List<string> menuContents)
        {
            bool isFirst = true;
            foreach (string menuItemLabel in menuContents)
            {
                MenuItem menuItem = new MenuItem
                {
                    Header = new TextBlock { Text = menuItemLabel },
                    IsCheckable = true,
                    IsChecked = isFirst,
                };
                isFirst = false;

                menuItem.Checked += MenuItemChecked;
                button.ContextMenu.Items.Add(menuItem);
            }
        }

        private void MenuClick(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void MenuItemChecked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            logger.Info("Parent type " + menuItem.Parent.GetType());
            ContextMenu contextMenuParent = menuItem.Parent as ContextMenu;
            foreach (MenuItem item in contextMenuParent.Items)
            {
                if (item != sender)
                {
                    logger.Info("Unchecking " + item.Header);
                    item.IsChecked = false;
                }
            }
            Button button = contextMenuParent.PlacementTarget as Button;
            TextBlock menuItemText = menuItem.Header as TextBlock;
            if (menuItemText == null)
            {
                logger.Info("MenuItemText is null here");
                return;
            }
            button.Content = new TextBlock
            {
                Text = menuItemText.Text,
            };
        }
    }
}