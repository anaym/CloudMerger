﻿using System;
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
        public static Task<OAuthCredentials> ShowNew(IService[] services)
        {
            return ShowNew(new OAuthCredentials(), services);
        }

        public static async Task<OAuthCredentials> ShowNew(OAuthCredentials input, IService[] services)
        {
            var result = new OAuthCredentials();
            StaApplication.StartNew(() => new OAuthCredentialsForm(input, services, result)).Join();
            return result;
        }
    }

    public class OAuthCredentialsForm : Form
    {
        private readonly OAuthCredentials input;
        private readonly IService[] services;
        private readonly OAuthCredentials result;

        private ComboBox serviceSelector;
        private ComboTextInput login;
        private ComboTextInput token;

        private Button ok;
        private Button reset;
        private Button cancel;

        public OAuthCredentialsForm(OAuthCredentials input, IService[] services, OAuthCredentials result)
        {
            this.input = input;
            this.services = services;
            this.result = result;

            InitializeControls();
            Reset();
            login.ButtonClick += _ => Authorize();
            token.ButtonClick += _ => Authorize();

            reset.Click += (_, __) => Reset();
            cancel.Click += (_, __) => { Reset(); End(); };
            ok.Click += (_, __) => End();
        }

        private void End()
        {
            result.Service = currentService?.Name?.ToLower();
            result.Login = login.Text == "" ? null : login.Text;
            result.Token = token.Text == "" ? null : token.Text;
            Close();
        }

        private IService currentService => serviceSelector.SelectedItem as IService;

        private async void Authorize()
        {
            if (currentService == null)
            {
                MessageBox.Show("Please, select service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Enabled = false;
                Hide();
                try
                {
                    var result = await currentService.AuthorizeAsync();
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
            var srv = (object)services.FirstOrDefault(s => s.Name.ToLower() == input.Service?.ToLower()) ?? "?";
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
            serviceSelector.Items.AddRange(services);
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