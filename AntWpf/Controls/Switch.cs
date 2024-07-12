﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;


namespace AntWpf.Controls
{
    [TemplatePart(Name = PART_Dot, Type = typeof(FrameworkElement))]
    public class Switch : ToggleButton
    {
        #region Fields

        private const string PART_Dot = "PART_Dot";

        private FrameworkElement dot;

        private VisualState pressedState;

        private Storyboard pressedStoryboard;

        #endregion

        #region Properties

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Switch), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets loading state of switch.
        /// </summary>
        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Sizes?), typeof(Switch), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the size of the switch.
        /// </summary>
        public Sizes? Size
        {
            get { return (Sizes?)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedContentProperty =
            DependencyProperty.Register("UnCheckedContent", typeof(object), typeof(Switch), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets content to be shown when the state is unchecked.
        /// </summary>
        public object UnCheckedContent
        {
            get { return GetValue(UnCheckedContentProperty); }
            set { SetValue(UnCheckedContentProperty, value); }
        }

        #endregion

        #region Constructors

        static Switch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Switch), new FrameworkPropertyMetadata(typeof(Switch)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            dot = GetTemplateChild(PART_Dot) as FrameworkElement;
            pressedState = GetTemplateChild("Pressed") as VisualState;

            if (pressedState != null && pressedState.Storyboard != null)
            {
                pressedStoryboard = pressedState.Storyboard.Clone();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SetDotAnimation();
        }
        private bool DotAnimated = false;
        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            //if (!DotAnimated)
            //{
            if (dot != null /*&& IsChecked.HasValue && IsChecked.Value*/ && IsPressed)
            {
                var to = dot.Margin;
                // to.Left -= dot.ActualWidth * 1.3333 - dot.ActualWidth;
                to.Right -= dot.ActualWidth * 1.3333 - dot.ActualWidth;
                var ease = new CircleEase() { EasingMode = EasingMode.EaseInOut };
                var animation = new ThicknessAnimation(to, TimeSpan.FromSeconds(0.36)) { EasingFunction = ease };

                dot.BeginAnimation(MarginProperty, animation);
            }

            //else if (!IsPressed) DotAnimated = false;
            //}
        }

        private void SetDotAnimation()
        {
            if (dot == null || pressedState == null || DotAnimated) return;

            DotAnimated = true;
            var storyboard = pressedStoryboard != null ? pressedStoryboard.Clone() : new Storyboard();
            var ease = new CircleEase() { EasingMode = EasingMode.EaseInOut };
            var animation = new DoubleAnimation(dot.ActualWidth * 1.3333, TimeSpan.FromSeconds(0.36)) { EasingFunction = ease };

            Storyboard.SetTargetName(animation, PART_Dot);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));

            storyboard.Children.Add(animation);
            pressedState.Storyboard = storyboard;
        }

        #endregion
    }
}
