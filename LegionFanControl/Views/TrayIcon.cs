using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace LegionFanControl.Views;

public class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MainWindow _mainWindow;

    public TrayIcon(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        _notifyIcon = new NotifyIcon
        {
            Text = "Legion Fan Control",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => ShowWindow();

        mainWindow.Closing += OnWindowClosing;
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.BackColor = Color.FromArgb(34, 34, 34);
        menu.ForeColor = Color.White;
        menu.RenderMode = ToolStripRenderMode.System;

        var showItem = new ToolStripMenuItem("Show") { Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold) };
        showItem.Click += (_, _) => ShowWindow();

        var separator = new ToolStripSeparator();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            _notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        };

        menu.Items.Add(showItem);
        menu.Items.Add(separator);
        menu.Items.Add(exitItem);
        return menu;
    }

    private void ShowWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        _mainWindow.Hide();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
