﻿namespace AntWpf.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;
    using ButtonBase = System.Windows.Controls.Button;

    /// <summary>
    /// To trigger an operation.
    /// </summary>
    [TemplatePart(Name = PART_Border, Type = typeof(FrameworkElement))]
    [TemplateVisualState(Name = "Loaded", GroupName = "LoadStates")]
    [TemplateVisualState(Name = "Unloaded", GroupName = "LoadStates")]
    public class Button : ButtonBase
    {
        #region Fields

        private const string PART_Border = "PART_Border";

        private FrameworkElement border;

        private VisualState mouseOverState;

        private VisualState pressedState;

        private VisualState focusedState;

        #endregion

        #region Properties

        public static readonly DependencyProperty GhostProperty =
            DependencyProperty.Register("Ghost", typeof(bool), typeof(Button), new PropertyMetadata(false, OnEffectBrushChanged));

        /// <summary>
        /// Gets/sets whether to make the background transparent and invert text and border colors.
        /// </summary>
        public bool Ghost
        {
            get { return (bool)GetValue(GhostProperty); }
            set { SetValue(GhostProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(Button), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the icon type of the button.
        /// </summary>
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Button), new PropertyMetadata(false, OnLoadingChanged));

        /// <summary>
        /// Gets/sets the loading state of the button.
        /// </summary>
        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        private static void OnLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Button).SetLoadVisualState();
        }

        private void SetLoadVisualState()
        {
            VisualStateManager.GoToState(this, (Loading ? "Loaded" : "Unloaded"), true);
        }


        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register("Shape", typeof(Shapes), typeof(Button), new PropertyMetadata(Shapes.Square));

        /// <summary>
        /// Gets/sets the shape of button.
        /// </summary>
        public Shapes Shape
        {
            get { return (Shapes)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Sizes?), typeof(Button), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the size of the button.
        /// </summary>
        public Sizes? Size
        {
            get { return (Sizes?)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty = 
            DependencyProperty.Register("Type", typeof(ButtonType?), typeof(Button), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the type of the button.
        /// </summary>
        public ButtonType? Type
        {
            get { return (ButtonType?)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty EffectBrushProperty = DependencyProperty.Register(
            "EffectBrush", 
            typeof(Brush), 
            typeof(Button), 
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits,
                OnEffectBrushChanged));

        /// <summary>
        /// Gets/sets the border effect brush of the button.
        /// </summary>
        public Brush EffectBrush
        {
            get { return (Brush)GetValue(EffectBrushProperty); }
            set { SetValue(EffectBrushProperty, value); }
        }

        private static void OnEffectBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Button).SetVisualStateAnimation();
        }

        /// <summary>
        /// Force background transparent in Ghost state.
        /// </summary>
        private static object OnBackgroundCoerceValue(DependencyObject d, object baseValue)
        {
            var button = d as Button;

            if (button.Ghost)
            {
                return Brushes.Transparent;
            }

            return baseValue;
        }

        #endregion

        #region Constructors

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
            BackgroundProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata { CoerceValueCallback = OnBackgroundCoerceValue });
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            border         = GetTemplateChild(PART_Border) as FrameworkElement;
            mouseOverState = GetTemplateChild("MouseOver") as VisualState;

            focusedState   = GetTemplateChild("Focused") as VisualState;
            pressedState   = GetTemplateChild("Pressed") as VisualState;

            SetVisualStateAnimation();
            SetLoadVisualState();
        }

        #endregion

        #region Private Methods

        private void SetVisualStateAnimation()
        {
            // No initialization or no need for me to handle.
            if (border == null || mouseOverState == null && focusedState == null && pressedState == null) return;

            // Unable to extract color.
            if (EffectBrush is SolidColorBrush brush)
            {
                var isShape = border is Shape;
                Func<Button, Color, int, bool, bool, Duration?, Storyboard> func;

                if (!Type.HasValue || Type.Value == ButtonType.Dashed)
                {
                    func = CreateDefaultStoryboard;
                }
                else if (Type.Value == ButtonType.Primary)
                {
                    func = CreatePrimaryStoryboard;
                }
                else
                {
                    // Danger
                    func = CreateDangerStoryboard;
                }

                if (mouseOverState != null)
                {
                    mouseOverState.Storyboard = func(this, brush.Color, 5, isShape, false, null);
                }

                if (focusedState != null)
                {
                    focusedState.Storyboard = func(this, brush.Color, 5, isShape, true, null);
                }

                if (pressedState != null)
                {
                    pressedState.Storyboard = func(this, brush.Color, 7, isShape, false, TimeSpan.FromSeconds(0));
                }
            }
        }

        private static Storyboard CreateDefaultStoryboard(Button button, Color color, int index, bool IsShape, bool focused, Duration? duration = null)
        {
            var storyboard = new Storyboard();
            var children = storyboard.Children;

            color = ColorPalette.Toning(color, index);
            children.Add(CreateForegroundAnimation(button, color, duration));
            children.Add(CreateBorderAnimation(PART_Border, color, IsShape, duration));

            return storyboard;
        }

        private static Storyboard CreatePrimaryStoryboard(Button button, Color color, int index, bool IsShape, bool focused, Duration? duration = null)
        {
            var storyboard = new Storyboard();
            var children   = storyboard.Children;

            color = ColorPalette.Toning(color, index);

            if (button.Ghost)
            {
                children.Add(CreateForegroundAnimation(button, color, duration));
            } else
            {
                children.Add(CreateBackgroundAnimation(PART_Border, color, IsShape, duration));
            }

            children.Add(CreateBorderAnimation(PART_Border, color, IsShape, duration));

            return storyboard;
        }

        private static Storyboard CreateDangerStoryboard(Button button, Color color, int index, bool IsShape, bool focused, Duration? duration = null)
        {
            var storyboard = new Storyboard();
            var children   = storyboard.Children;

            Color foreground;
            color = ColorPalette.Toning(color, index);

            if (button.Ghost)
            {
                foreground = color;
            } else
            {
                Color background;

                if (focused)
                {
                    foreground = color;
                    background = Colors.White;
                }
                else
                {
                    foreground = Colors.White;
                    background = color;
                }

                children.Add(CreateBackgroundAnimation(PART_Border, background, IsShape, duration));
            }

            children.Add(CreateForegroundAnimation(button, foreground, duration));
            children.Add(CreateBorderAnimation(PART_Border, color, IsShape, duration));

            return storyboard;
        }

        private static Timeline CreateForegroundAnimation(DependencyObject target, Color color, Duration? duration = null)
        {
            return CreateColorAnimation(target, "(Control.Foreground).(SolidColorBrush.Color)", color, duration);
        }

        private static Timeline CreateBackgroundAnimation(string target, Color color, bool IsShape, Duration? duration = null)
        {
            return CreateColorAnimation(target, (IsShape ? "Fill" : "Background") + ".Color", color, duration);
        }

        private static Timeline CreateBorderAnimation(string target, Color color, bool IsShape, Duration? duration = null)
        {
            return CreateColorAnimation(target, (IsShape ? "Stroke" : "BorderBrush") + ".Color", color, duration);
        }

        private static Timeline CreateColorAnimation(object target, string path, Color color, Duration? duration)
        {
            var animation = new ColorAnimation() { To = color };

            if (duration.HasValue)
            {
                animation.Duration = duration.Value;
            }

            if (target is DependencyObject)
            {
                Storyboard.SetTarget(animation, (DependencyObject)target);
            } else
            {
                Storyboard.SetTargetName(animation, (string)target);
            }
  
            Storyboard.SetTargetProperty(animation, new PropertyPath(path));

            return animation;
        }

        #endregion
    }

    public enum ButtonType : byte
    {
        Primary, Dashed, Danger
    }
}
