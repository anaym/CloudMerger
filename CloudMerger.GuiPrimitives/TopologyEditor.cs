using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;
using CloudMerger.HostingsManager;

namespace CloudMerger.GuiPrimitives
{
    public class TopologyEditor
    {
        public static Task<Node<OAuthCredentials>> ShowNew(ServicesCollection services)
        {
            return ShowNew(null, services);
        }

        public static async Task<Node<OAuthCredentials>> ShowNew(Node<OAuthCredentials> input, ServicesCollection services)
        {
            var result = new Mutable<Node<OAuthCredentials>>(null);
            StaApplication.StartNew(() => new TopologyEditorForm(services, input, result)).Join();
            return result.Value;
        }
    }

    public class TopologyEditorForm : Form
    {
        private readonly ServicesCollection services;
        private readonly Node<OAuthCredentials> input;
        private readonly Mutable<Node<OAuthCredentials>> result;

        public TopologyEditorForm(ServicesCollection services, Node<OAuthCredentials> input, Mutable<Node<OAuthCredentials>> result)
        {
            this.services = services;
            this.input = input;
            this.result = result;
            result.Value = input?.Clone();

            InitializeControls();
            ok.Click += (_, __) => Save();
            reset.Click += (_, __) => Reset();
            cancel.Click += (_, __) => Cancel();

            ControlBox = false;

            Reset();
        }

        private void Save()
        {
            Close();
        }

        private void Reset()
        {
            result.Value = input?.Clone();
            Update();
            tree.ExpandAll();
            tree.Font = new Font(tree.Font.FontFamily, 12);
        }

        private void Cancel()
        {
            Reset();
            Save();
        }

        private async void NewHosting()
        {
            var curent = tree.SelectedNode;
            var node = ((Node<OAuthCredentials>) tree.SelectedNode?.Tag);
            if (curent == null)
            {
                var credentials = await OAuthCredentialsEditor.ShowNew(services.Managers.ToArray());
                result.Value = new Node<OAuthCredentials>(credentials);
                Update();
            }
            else if (node?.Value.Service != null)
            {
                if (services.IsContainsMultiHostingManager(node.Value.Service))
                {
                    var credentials = await OAuthCredentialsEditor.ShowNew(services.Managers.ToArray());
                    var n = new Node<OAuthCredentials>(credentials);
                    node.Nested.Add(n);
                    var tn = new TreeNode {Tag = n};
                    curent.Nodes.Add(tn);
                    UpdateNode(tn);
                }
            }
        }

        private void NewMultiHosting(string name)
        {
            var curent = tree.SelectedNode;
            var node = ((Node<OAuthCredentials>)tree.SelectedNode?.Tag);
            if (curent == null)
            {
                result.Value = new Node<OAuthCredentials>(new OAuthCredentials {Service = name});
                Update();
            }
            else if (node?.Value.Service != null)
            {
                if (services.IsContainsMultiHostingManager(node.Value.Service))
                {
                    var n = new Node<OAuthCredentials>(new OAuthCredentials {Service = name});
                    node.Nested.Add(n);
                    var tn = new TreeNode {Tag = n};
                    curent.Nodes.Add(tn);
                    UpdateNode(tn);
                }
            }
        }

        private void RemoveHosting()
        {
            var current = tree.SelectedNode;
            if (current == null)
                return;

            if (current.Parent == null)
            {
                result.Value = null;
                Update();
            }
            else
            {
                var parent = current.Parent;
                parent.Nodes.Remove(current);
                ((Node<OAuthCredentials>) parent.Tag).Nested.Remove((Node<OAuthCredentials>) current.Tag);
                UpdateNode(parent);
            }
        }

        private async void EditHosting()
        {
            if (tree.SelectedNode == null)
                return;

            var current = (Node<OAuthCredentials>)tree.SelectedNode.Tag;
            if (current.Value.Service == null || services.IsContainsManager(current.Value.Service))
            {
                var credentials = await OAuthCredentialsEditor.ShowNew(current.Value, services.Managers.ToArray());
                current.Value = credentials;
                UpdateNode(tree.SelectedNode);
            }
        }

        private void Update()
        {
            tree.Nodes.Clear();
            if (result.Value == null)
                return;

            tree.Nodes.Add(new TreeNode());
            LoadNode(tree.Nodes[0], result.Value);
        }

        private void LoadNode(TreeNode to, Node<OAuthCredentials> from)
        {
            to.Tag = from;
            UpdateNode(to);
            foreach (var node in from.Nested)
            {
                var n = new TreeNode();
                LoadNode(n, node);
                to.Nodes.Add(n);
            }
        }

        private void UpdateNode(TreeNode node)
        {
            var from = (Node<OAuthCredentials>) node.Tag;
            var title = from.Value?.Login ?? from.Value?.Token?.Substring(0, 8) ?? "";
            node.Text = $"[{from.Value?.Service?.ToUpper() ?? "?"}] {title}";
            node.Expand();
            if (ok != null)
                ok.Enabled = tree.Nodes.Count > 0 && Validate(tree.Nodes[0]);
        }

        private bool Validate(TreeNode node)
        {
            var v = Validate((Node<OAuthCredentials>) node.Tag);
            foreach (var n in node.Nodes)
            {
                Validate((TreeNode)n);
            }
            if (v)
            {
                node.ForeColor = Color.Black;
                return true;
            }
            node.ForeColor = Color.Red;
            return false;
        }

        private bool Validate(Node<OAuthCredentials> node)
        {
            if (node.Value?.Service == null)
                return false;
            if (services.IsContainsManager(node.Value.Service))
            {
                if (node.IsNode)
                    return false;
                return true;
            }
            else if (services.IsContainsMultiHostingManager(node.Value.Service))
            {
                if (node.IsLeaf)
                    return false;
                return node.Nested.All(Validate);
            }
            else
            {
                return false;
            }
        }

        private void InitializeControls()
        {
            ClientSize = new Size(1080, 720);
            Text = "Accounts manager";

            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize});
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 1f });
            Controls.Add(table);

            var menu = new ToolStrip();
            menu.Items.Add(new ToolStripButton("&New hosting", null, (_, __) => NewHosting()));
            var multihosting = new ToolStripMenuItem("New &multihosting");
            menu.Items.Add(multihosting);
            menu.Items.Add(new ToolStripButton("&Remove", null, (_, __) => RemoveHosting()));
            menu.Items.Add(new ToolStripButton("&Edit", null, (_, __) => EditHosting()));
            table.Controls.Add(menu);

            foreach (var manager in services.MultiHostingManagerNames)
            {
                multihosting.DropDownItems.Add
                (
                    manager, 
                    null,
                    (_, __) => NewMultiHosting(manager + "")
                );
            }

            tree = new TreeView {Dock = DockStyle.Fill, AutoSize = true};
            table.Controls.Add(tree);

            var btable = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                ColumnCount = 3
            };
            ok = new Button { Text = "Save"};
            reset = new Button { Text = "Reset"};
            cancel = new Button { Text = "Cancel"};
            btable.Controls.Add(ok);
            btable.Controls.Add(reset);
            btable.Controls.Add(cancel);
            table.Controls.Add(btable, 0, 2);
        }

        private TreeView tree;
        private Button ok;
        private Button reset;
        private Button cancel;
    }
}