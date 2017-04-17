using System;
using System.Windows.Forms;

namespace CloudMerger.GuiPrimitives
{
    public class ComboTextInput : TableLayoutPanel
    {
        private readonly TextBox editor;

        public string Text
        {
            get { return editor.Text; }
            set { editor.Text = value; }
        }

        public event Action<string> ButtonClick;

        public ComboTextInput(string label, string button = null)
        {
            ColumnCount = 1;
            var lbl = new Label {Text = label ?? "", Dock = DockStyle.Left, AutoSize = true};
            var btn = new Button {Text = button ?? "", Dock = DockStyle.Right};
            btn.Click += (sender, args) => ButtonClick?.Invoke(Text);
            editor = new TextBox {Dock = DockStyle.Fill, AutoSize = true};
            RowStyles.Add(new RowStyle {SizeType = SizeType.AutoSize});
            if (label != null)
            {
                ColumnStyles.Add(new ColumnStyle {SizeType = SizeType.AutoSize});
                ColumnCount++;
                Controls.Add(lbl, 0, 0);
            }
            ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 1f });
            Controls.Add(editor, 1, 0);
            if (button != null)
            {
                ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
                ColumnCount++;
                Controls.Add(btn, 2, 0);
                btn.Width = editor.Width;
            }
            AutoSize = true;
        }
    }
}