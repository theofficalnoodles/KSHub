using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KSHub
{
    public partial class MainWindow : Window
    {
        private List<TabInfo> tabs = new();
        private int activeIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCef();

            BtnNewTab.Click += BtnNewTab_Click;
            BtnLoadExtension.Click += BtnLoadExtension_Click;
            BtnMakeDefault.Click += BtnMakeDefault_Click;
            BtnBack.Click += BtnBack_Click;
            BtnForward.Click += BtnForward_Click;
            Omnibox.KeyDown += Omnibox_KeyDown;

            OpenNewTab("https://www.google.com");
        }

        private void InitializeCef()
        {
            var settings = new CefSettings();
            settings.Locale = "en-US";
            // experimental flags for extension support (if available in your Cef/CefSharp)
            settings.CefCommandLineArgs.Add("disable-features", "ExtensionsToolbarMenu"); // example tweak
            // Initialize Cef (performDependencyCheck true helps ensure native libs are present)
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        private void BtnNewTab_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTab("about:blank");
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (activeIndex >= 0)
            {
                var b = tabs[activeIndex].Browser;
                if (b.CanGoBack) b.Back();
            }
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            if (activeIndex >= 0)
            {
                var b = tabs[activeIndex].Browser;
                if (b.CanGoForward) b.Forward();
            }
        }

        private void Omnibox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && activeIndex >= 0)
            {
                var url = ResolveInput(Omnibox.Text.Trim());
                tabs[activeIndex].Browser.Load(url);
            }
        }

        private string ResolveInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "https://www.google.com";
            if (Uri.TryCreate(input, UriKind.Absolute, out var u) &&
                (u.Scheme == "http" || u.Scheme == "https"))
                return u.ToString();
            if (input.Contains(".") && !input.Contains(" ")) return "http://" + input;
            return "https://www.google.com/search?q=" + Uri.EscapeDataString(input);
        }

        private void OpenNewTab(string url)
        {
            var browser = new ChromiumWebBrowser(url);
            browser.AddressChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (activeIndex >= 0 && tabs[activeIndex].Browser == browser)
                        Omnibox.Text = e.Address;
                    // update tab title if needed
                });
            };

            // Tab button
            var tabButton = new Button
            {
                Content = "New Tab",
                Margin = new Thickness(4, 0, 4, 0),
                MinWidth = 160,
                Height = 28,
                Background = System.Windows.Media.Brushes.Transparent
            };

            int index = tabs.Count;
            tabButton.Click += (s, e) => ActivateTab(index);

            // Add to UI
            TabStrip.Children.Add(tabButton);
            tabs.Add(new TabInfo { Browser = browser, TabButton = tabButton });

            ActivateTab(index);
        }

        private void ActivateTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;
            activeIndex = index;
            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(tabs[index].Browser);
            Omnibox.Text = tabs[index].Browser.Address;
            // update tab visuals: bold active
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].TabButton.FontWeight = i == index ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        // Load an unpacked extension directory (simple implementation)
        private void BtnLoadExtension_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            var res = dlg.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                string extPath = dlg.SelectedPath;
                try
                {
                    var ctx = Cef.GetGlobalRequestContext();
                    // Many CefSharp versions provide RequestContext.LoadExtension/LoadExtensionAsync - check your version.
                    bool loaded = ctx.LoadExtension(extPath, null);
                    MessageBox.Show("LoadExtension called. result: " + loaded, "Extension loader");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Extension load error: " + ex.Message, "Extension loader");
                }
            }
        }

        // Open Windows Default Apps settings to let user set KSHub as default
        private void BtnMakeDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // On Windows 10/11, open the "Default apps" settings page
                Process.Start(new ProcessStartInfo("ms-settings:defaultapps") { UseShellExecute = true });
            }
            catch
            {
                // fallback: open control panel default programs (older)
                Process.Start(new ProcessStartInfo("control", "/name Microsoft.DefaultPrograms") { UseShellExecute = true });
            }
        }
    }

    public class TabInfo
    {
        public ChromiumWebBrowser Browser { get; set; }
        public Button TabButton { get; set; }
    }
}
