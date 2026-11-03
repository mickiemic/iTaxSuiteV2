using Microsoft.Toolkit.Uwp.Notifications;

namespace iTaxSuite.WinForms
{
    public class MApplicationContext : ApplicationContext
    {
        // private NotifyIcon _trayIcon;

        public MApplicationContext()
        {
            // Initialize the tray icon
            /*_trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Required: assign an icon
                Visible = true,                 // Required: default is false
                Text = "My App"                 // Tooltip text
            };*/

            // Show a simple balloon notification
            /*_trayIcon.ShowBalloonTip(
                timeout: 5000, // Duration in milliseconds
                tipTitle: "Notification",
                tipIcon: "This is a simple system tray notification.",
                tipIcon: ToolTipIcon.Info
            );*/

            //_trayIcon.ShowBalloonTip(5000, "Tax Notification", $"It is now {DateTime.Now.ToString("s")}", ToolTipIcon.Info);

            /*new ToastContentBuilder()
                .AddText("Hello Windows 11")
                .AddText("Modern toast via Community Toolkit")
                .AddAppLogoOverride(new Uri("file:///C:/path/to/image.png"))
                .Show();*/

            // Handle cleanup on exit
            /*Application.ApplicationExit += (sender, e) =>
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            };*/
        }
    }
}
