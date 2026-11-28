namespace AntWpf
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Switches the AntWpf color theme of the running application.
    /// </summary>
    /// <remarks>
    /// The theme dictionary (<c>Themes/Light.xaml</c> or <c>Themes/Dark.xaml</c>) is kept as a top-level
    /// entry of <see cref="Application.Resources"/>'s merged dictionaries. If one is already there it is
    /// replaced in place, so dictionaries merged after it keep overriding it; otherwise it is appended,
    /// so it overrides the Light colors that <c>Styles/Theme.xaml</c> merges. Other dictionaries are not
    /// touched. Styles reference colors through DynamicResource, so controls update immediately.
    /// A custom primary color (<see cref="SetPrimaryColor"/>) is a generated dictionary kept right after
    /// the theme dictionary.
    /// </remarks>
    public static class ThemeManager
    {
        #region Fields

        private const string ThemePathPrefix = "AntWpf;component/Themes/";

        private static AntTheme? current;

        private static Color? primaryColor;

        // Overrides the primary tokens of the theme dictionary; kept right after it in the merged dictionaries.
        private static ResourceDictionary? primaryDictionary;

        // Theme keys that follow the primary color -> color token (1..10). Keep in sync with Themes/*.xaml.
        // Info, Processing, Link and the Blue3/Blue4 palette keys keep their own colors, as in Ant Design.
        private static readonly (string Key, int Token)[] primaryColorKeys =
        {
            ("PrimaryColor", 6), ("PrimaryHoverColor", 5), ("PrimaryActiveColor", 7), ("BackgroundColorLight", 1)
        };

        private static readonly (string Key, int Token)[] primaryBrushKeys =
        {
            ("PrimaryBrush", 6), ("PrimaryHoverBrush", 5), ("PrimaryActiveBrush", 7), ("OutlineBrush", 6),
            ("BackgroundBrushLight", 1), ("WindowTilteBarBackground", 6), ("ButtonPrimaryBackground", 6),
            ("CheckBoxBrush", 6), ("RadioDotBrush", 6), ("InputMouseOverBorderBrush", 6), ("SwitchBrush", 6),
            ("SliderBackgroundBrush", 3), ("SliderHoverBrush", 4)
        };

        #endregion

        #region Events

        /// <summary>
        /// Raised on the UI thread after the theme dictionary or the primary color has been switched.
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

        /// <summary>
        /// Gets the custom primary seed color set with <see cref="SetPrimaryColor"/>, or
        /// <see langword="null"/> when the theme's own primary palette is used.
        /// </summary>
        public static Color? PrimaryColor => primaryColor;

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

            // The palette of a custom primary color depends on the theme.
            ApplyPrimaryColor(application);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Replaces the primary palette (Primary1..10 tokens and the keys derived from them) of
        /// <see cref="Application.Current"/> with the Ant Design v5 palette of <paramref name="color"/>,
        /// generated for the current theme and regenerated whenever <see cref="SetTheme"/> switches it.
        /// Pass <see langword="null"/> to restore the theme's own primary palette. Can be called from any
        /// thread; off the UI thread the call is marshalled synchronously through the dispatcher.
        /// </summary>
        /// <remarks>
        /// Only the primary color changes: Info, Processing, Success, Warning, Error and Link keep the
        /// theme's colors. In the dark theme <paramref name="color"/> is the seed, as in Ant Design, so
        /// the applied primary color is the seed mixed over the dark background (#1677FF gives #1668DC).
        /// </remarks>
        public static void SetPrimaryColor(Color? color)
        {
            var application = Application.Current
                ?? throw new InvalidOperationException("ThemeManager requires an Application instance.");

            if (!application.Dispatcher.CheckAccess())
            {
                application.Dispatcher.Invoke(() => SetPrimaryColor(color));
                return;
            }

            // Only the RGB of the seed is used: the theme colors are opaque.
            if (color is Color seed)
            {
                color = Color.FromRgb(seed.R, seed.G, seed.B);
            }

            if (color == primaryColor)
            {
                return;
            }

            primaryColor = color;
            ApplyPrimaryColor(application);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Removes the primary dictionary and, for a custom primary color, inserts a new one generated for
        /// <see cref="Current"/> right after the theme dictionary (or last, when none is merged), so the
        /// dictionaries the application merges after the theme still override it.
        /// </summary>
        private static void ApplyPrimaryColor(Application application)
        {
            var dictionaries = application.Resources.MergedDictionaries;

            if (primaryDictionary != null)
            {
                dictionaries.Remove(primaryDictionary);
                primaryDictionary = null;
            }

            if (primaryColor is not Color seed)
            {
                return;
            }

            var dictionary = CreatePrimaryDictionary(seed, Current == AntTheme.Dark);
            int index = dictionaries.Count;

            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                if (TryGetTheme(dictionaries[i], out _))
                {
                    index = i + 1;
                    break;
                }
            }

            dictionaries.Insert(index, dictionary);
            primaryDictionary = dictionary;
        }

        private static ResourceDictionary CreatePrimaryDictionary(Color seed, bool dark)
        {
            var tokens     = ColorPalette.Generate(seed, dark);
            var brushes    = new SolidColorBrush[tokens.Length];
            var dictionary = new ResourceDictionary();

            for (int i = 0; i < tokens.Length; i++)
            {
                // Frozen like the theme brushes: state storyboards rely on WPF cloning frozen brushes.
                brushes[i] = new SolidColorBrush(tokens[i]);
                brushes[i].Freeze();

                dictionary[$"Primary{i + 1}Color"] = tokens[i];
                dictionary[$"Primary{i + 1}Brush"] = brushes[i];
            }

            foreach (var (key, token) in primaryColorKeys)
            {
                dictionary[key] = tokens[token - 1];
            }

            foreach (var (key, token) in primaryBrushKeys)
            {
                dictionary[key] = brushes[token - 1];
            }

            return dictionary;
        }

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
