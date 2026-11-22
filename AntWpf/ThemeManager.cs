namespace AntWpf
{
    using System;
    using System.Windows;

    /// <summary>
    /// Switches the AntWpf color theme of the running application.
    /// </summary>
    /// <remarks>
    /// The theme dictionary (<c>Themes/Light.xaml</c> or <c>Themes/Dark.xaml</c>) is kept as a top-level
    /// entry of <see cref="Application.Resources"/>'s merged dictionaries. If one is already there it is
    /// replaced in place, so dictionaries merged after it keep overriding it; otherwise it is appended,
    /// so it overrides the Light colors that <c>Styles/Theme.xaml</c> merges. Other dictionaries are not
    /// touched. Styles reference colors through DynamicResource, so controls update immediately.
    /// </remarks>
    public static class ThemeManager
    {
        #region Fields

        private const string ThemePathPrefix = "AntWpf;component/Themes/";

        private static AntTheme? current;

        #endregion

        #region Events

        /// <summary>
        /// Raised on the UI thread after the theme dictionary has been switched.
        /// </summary>
        public static event EventHandler? ThemeChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the active theme. Before <see cref="SetTheme"/> is called it reflects the theme
        /// dictionary merged into the application resources, or <see cref="AntTheme.Light"/>.
        /// </summary>
        public static AntTheme Current
        {
            get
            {
                if (current == null)
                {
                    current = DetectTheme();
                }

                return current.Value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies <paramref name="theme"/> to <see cref="Application.Current"/>. Can be called from any
        /// thread; off the UI thread the call is marshalled synchronously through the dispatcher.
        /// </summary>
        public static void SetTheme(AntTheme theme)
        {
            var application = Application.Current
                ?? throw new InvalidOperationException("ThemeManager requires an Application instance.");

            if (!application.Dispatcher.CheckAccess())
            {
                application.Dispatcher.Invoke(() => SetTheme(theme));
                return;
            }

            var dictionaries = application.Resources.MergedDictionaries;
            int index = -1;

            // Keep a single theme dictionary: the last one is the effective one, drop earlier duplicates.
            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                if (!TryGetTheme(dictionaries[i], out _))
                {
                    continue;
                }

                if (index < 0)
                {
                    index = i;
                }
                else
                {
                    dictionaries.RemoveAt(i);
                    index--;
                }
            }

            if (theme == Current && (index >= 0 || theme == AntTheme.Light))
            {
                return;
            }

            var dictionary = new ResourceDictionary { Source = GetThemeUri(theme) };

            if (index >= 0)
            {
                dictionaries[index] = dictionary;
            }
            else
            {
                dictionaries.Add(dictionary);
            }

            current = theme;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private static AntTheme DetectTheme()
        {
            var application = Application.Current;

            if (application != null)
            {
                var dictionaries = application.Resources.MergedDictionaries;

                for (int i = dictionaries.Count - 1; i >= 0; i--)
                {
                    if (TryGetTheme(dictionaries[i], out var theme))
                    {
                        return theme;
                    }
                }
            }

            return AntTheme.Light;
        }

        private static bool TryGetTheme(ResourceDictionary dictionary, out AntTheme theme)
        {
            var source = dictionary.Source?.OriginalString;

            foreach (AntTheme candidate in Enum.GetValues(typeof(AntTheme)))
            {
                if (source != null && source.EndsWith(ThemePathPrefix + candidate + ".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    theme = candidate;
                    return true;
                }
            }

            theme = default;
            return false;
        }

        private static Uri GetThemeUri(AntTheme theme)
        {
            return new Uri("pack://application:,,,/" + ThemePathPrefix + theme + ".xaml", UriKind.Absolute);
        }

        #endregion
    }
}
