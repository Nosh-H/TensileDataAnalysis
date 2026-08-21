using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataAnalyzer
{
	/// <summary>
	/// A message box for unanticipated exceptions: a short, bold "what to check" line up top,
	/// separated by a divider from the raw exception details (message + stack trace) below in
	/// plain text.  MessageBox can't mix bold guidance with plain diagnostic detail in one box,
	/// so this fills that gap for MainForm's catch-all exception handler.
	/// </summary>
	public class ErrorDialog : Form
	{
		public static void Show(string guidance, string details)
		{
			using (ErrorDialog dialog = new ErrorDialog(guidance, details))
			{
				dialog.ShowDialog();
			}
		}

		private ErrorDialog(string guidance, string details)
		{
			Text = "Error";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowIcon = false;
			ShowInTaskbar = false;
			// CenterParent with no owner passed to ShowDialog() is undefined/inconsistent in
			// WinForms; CenterScreen guarantees this always renders somewhere visible.
			StartPosition = FormStartPosition.CenterScreen;
			ClientSize = new Size(520, 380);

			Label guidanceLabel = new Label
			{
				Text = guidance,
				Font = new Font(Font, FontStyle.Bold),
				AutoSize = false,
				Location = new Point(12, 12),
				Size = new Size(496, 60),
			};

			Panel divider = new Panel
			{
				BorderStyle = BorderStyle.Fixed3D,
				Location = new Point(12, guidanceLabel.Bottom + 8),
				Size = new Size(496, 2),
			};

			TextBox detailsBox = new TextBox
			{
				Text = details,
				Font = new Font(Font, FontStyle.Regular),
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Vertical,
				Location = new Point(12, divider.Bottom + 8),
				Size = new Size(496, 250),
			};

			Button okButton = new Button
			{
				Text = "OK",
				DialogResult = DialogResult.OK,
				Location = new Point(432, detailsBox.Bottom + 8),
				Size = new Size(76, 24),
			};

			Controls.Add(guidanceLabel);
			Controls.Add(divider);
			Controls.Add(detailsBox);
			Controls.Add(okButton);
			AcceptButton = okButton;
			CancelButton = okButton;
		}
	}
}
