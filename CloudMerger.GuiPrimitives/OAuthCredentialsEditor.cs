using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;

namespace CloudMerger.GuiPrimitives
{
    public static class OAuthCredentialsEditor
    {
        public static Task<Result<OAuthCredentials>> ShowNew(IHostingManager[] hostingManagers)
        {
            return ShowNew(new OAuthCredentials(), hostingManagers);
        }

        public static async Task<Result<OAuthCredentials>> ShowNew(OAuthCredentials input, IHostingManager[] hostingManagers)
        {
            var result = new Result<OAuthCredentials>(new OAuthCredentials());
            StaApplication.StartNew(() => new OAuthCredentialsForm(input, hostingManagers, result)).Join();
            return result;
        }
    }

    public class OAuthCredentialsForm : Form
    {
        private readonly OAuthCredentials input;
        private readonly IHostingManager[] hostingManagers;
        private readonly Result<OAuthCredentials> result;

        private ComboBox serviceSelector;
        private ComboTextInput login;
        private ComboTextInput token;

        private Button ok;
        private Button reset;
        private Button cancel;

        public OAuthCredentialsForm(OAuthCredentials input, IHostingManager[] hostingManagers, Result<OAuthCredentials> result)
        {
            this.input = input;
            this.hostingManagers = hostingManagers;
            this.result = result;

            InitializeControls();
            Reset();
            login.ButtonClick += _ => Authorize();
            token.ButtonClick += _ => Authorize();

            reset.Click += (_, __) => Reset();
            cancel.Click += (_, __) => Cancel();
            ok.Click += (_, __) => End();
        }

        private void Cancel()
        {
            Reset();
            result.HasBeenCanceled = true;
            End();
        }

        private void End()
        {
            result.Value.Service = CurrentHostingManager?.Name?.ToLower();
            result.Value.Login = login.Text == "" ? null : login.Text;
            result.Value.Token = token.Text == "" ? null : token.Text;
            Close();
        }

        private IHostingManager CurrentHostingManager => serviceSelector.SelectedItem as IHostingManager;

        private async void Authorize()
        {
            if (CurrentHostingManager == null)
            {
                MessageBox.Show("Please, select hostingManager", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Enabled = false;
                Hide();
                try
                {
                    var result = await CurrentHostingManager.AuthorizeAsync();
                    if (result != null)
                    {
                        login.Text = result.Login ?? "";
                        token.Text = result.Token ?? "";
                    }
                }
                catch (Exception)
                { }
                finally
                {
                    Enabled = true;
                    Show();
                    Focus();
                }
            }
        }

        private void Reset()
        {
            var srv = (object)hostingManagers.FirstOrDefault(s => s.Name.ToLower() == input.Service?.ToLower()) ?? "?";
            serviceSelector.SelectedIndex = serviceSelector.Items.IndexOf(srv);
            login.Text = input.Login ?? "";
            token.Text = input.Token ?? "";
        }

        private void InitializeControls()
        {
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            //table.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 1f });
            Controls.Add(table);

            serviceSelector = new ComboBox { Dock = DockStyle.Fill };
            serviceSelector.Items.Add("?");
            serviceSelector.Items.AddRange(hostingManagers);
            serviceSelector.SelectedIndex = 0;
            serviceSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            table.Controls.Add(serviceSelector, 0, 0);
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            table.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });

            login = new ComboTextInput("Login: ", "...") { Dock = DockStyle.Fill };
            table.Controls.Add(login, 0, 1);

            token = new ComboTextInput("Token: ", "...") { Dock = DockStyle.Fill };
            table.Controls.Add(token, 0, 2);

            var btable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 3
            };
            ok = new Button {Text = "Save"};
            reset = new Button {Text = "Reset"};
            cancel = new Button {Text = "Cancel"};
            btable.Controls.Add(ok);
            btable.Controls.Add(reset);
            btable.Controls.Add(cancel);
            table.Controls.Add(btable, 0, 3);

            Height = btable.MinimumSize.Height + login.Height + token.Height;
            MaximumSize = new Size(1920, Height);
            MinimumSize = new Size(Width, Height);

            Text = "Credentials editor";
        }
    }
}