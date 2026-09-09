using System.Drawing;
using System.Windows.Forms;

namespace NuvyntraLabs.WinUIMVVMExpress.VisualStudio;

internal static class Prompts
{
    public static string? AskText(string title, string label, string defaultValue, Func<string, string?> validate)
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterScreen,
            ClientSize = new Size(440, 128),
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
        };
        var caption = new Label { Left = 16, Top = 12, Width = 408, Text = label };
        var box = new TextBox { Left = 16, Top = 36, Width = 408, Text = defaultValue };
        var ok = new Button { Text = "OK", Left = 268, Top = 84, Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 349, Top = 84, Width = 75 };
        ok.Click += (_, _) =>
        {
            var error = validate(box.Text.Trim());
            if (error is not null)
            {
                MessageBox.Show(form, error, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            form.DialogResult = DialogResult.OK;
        };
        form.Controls.Add(caption);
        form.Controls.Add(box);
        form.Controls.Add(ok);
        form.Controls.Add(cancel);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return form.ShowDialog() == DialogResult.OK ? box.Text.Trim() : null;
    }

    public static string? AskFolder(string description, string? selectedPath = null)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = description,
            ShowNewFolderButton = true,
        };
        if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
        {
            dialog.SelectedPath = selectedPath;
        }

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
