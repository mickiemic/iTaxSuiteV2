using ScintillaNET;
using System.Drawing.Design;

namespace iTaxSuite.WinForms.Extensions
{
    internal class PromptDialog
    {
        public static string ShowMultilineDialog(string title, string promptText, string defaultJson = "")
        {
            using (Form form = new Form())
            {
                form.Width = 650;
                form.Height = 450;

                // 1. ALLOW RESIZING: Change from FixedDialog to Sizable
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = true; // Allow maximizing for heavy JSON data
                form.MinimizeBox = false;

                // Set a minimum threshold so layout elements don't overlap or break
                form.MinimumSize = new Size(400, 300);

                Label lblPrompt = new Label()
                {
                    Left = 20,
                    Top = 15,
                    Width = 590,
                    Height = 25,
                    Text = promptText,
                    Font = new Font(form.Font.FontFamily, 10, FontStyle.Regular),

                    // Anchor label to top-left and stretch horizontally
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                Scintilla jsonEditor = new Scintilla()
                {
                    Left = 20,
                    Top = 45,
                    Width = 594,
                    Height = 280, // Dynamic height calculation base
                    Text = defaultJson,

                    // 2. STRETCH EDITOR: Anchor to all 4 sides so it grows with the window
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                //ConfigureJsonLexer(jsonEditor);
                EditorHelper.initCodeFolding(jsonEditor);
                EditorHelper.initSyntaxColoring(jsonEditor);

                Button btnOk = new Button()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Left = 405,
                    Top = 350,
                    Width = 100,
                    Height = 35,
                    UseVisualStyleBackColor = true,

                    // 3. LOCK BUTTONS TO BOTTOM RIGHT
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                Button btnCancel = new Button()
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Left = 515,
                    Top = 350,
                    Width = 100,
                    Height = 35,
                    UseVisualStyleBackColor = true,

                    // 3. LOCK BUTTONS TO BOTTOM RIGHT
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                form.CancelButton = btnCancel;

                form.Controls.Add(lblPrompt);
                form.Controls.Add(jsonEditor);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    return jsonEditor.Text;
                }

                return null;
            }
        }

        private static void ConfigureJsonLexer(Scintilla scintilla)
        {
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.LexerLanguage = Lexer.SCLEX_JSON.ToString();
            scintilla.Styles[Style.Json.String].ForeColor = Color.Maroon;
            scintilla.Styles[Style.Json.PropertyName].ForeColor = Color.Blue;
            scintilla.Styles[Style.Json.Number].ForeColor = Color.DarkCyan;
            scintilla.Styles[Style.Json.Operator].ForeColor = Color.DarkGray;
            scintilla.Styles[Style.Json.Keyword].ForeColor = Color.MediumPurple;

            //scintilla.Margins.Width = 35;
            scintilla.Styles[Style.LineNumber].ForeColor = Color.DimGray;
            scintilla.Styles[Style.LineNumber].BackColor = Color.WhiteSmoke;
        }
    }
}
