﻿using BenLib.Standard;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BenLib.WPF
{
    /// <summary>
    /// <para>Enables the selection inside of a ListBox using a seleciton rectangle.</para>
    /// <para>Authors : Samuel Cragg (https://www.codeproject.com/Articles/209560/ListBox-drag-selection) and BenNat</para>
    /// </summary>
    public sealed class ListBoxSelector
    {
        /// <summary>Identifies the IsEnabled attached property.</summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(ListBoxSelector), new UIPropertyMetadata(false, IsEnabledChangedCallback));

        // This stores the ListBoxSelector for each ListBox so we can unregister it.
        private static readonly Dictionary<ListBox, ListBoxSelector> attachedControls = new Dictionary<ListBox, ListBoxSelector>();

        private readonly ListBox listBox;
        private ScrollContentPresenter scrollContent;

        private SelectionAdorner selectionRect;
        private AutoScroller autoScroller;
        private ItemsControlSelector selector;

        private bool selecting;
        private bool clicked;
        private bool mouseCaptured;
        private Point start;
        private Point end;
        private Point clickpoint;
        private Point originalclickpoint;
        private MouseButtonEventArgs clickargs;

        private ListBoxSelector(ListBox listBox)
        {
            this.listBox = listBox;
            if (this.listBox.IsLoaded) Register();
            else
            {
                // We need to wait for it to be loaded so we can find the
                // child controls.
                this.listBox.Loaded += OnListBoxLoaded;
            }
        }

        /// <summary>
        /// Gets the value of the IsEnabled attached property that indicates
        /// whether a selection rectangle can be used to select items or not.
        /// </summary>
        /// <param name="obj">Object on which to get the property.</param>
        /// <returns>
        /// true if items can be selected by a selection rectangle; otherwise, false.
        /// </returns>
        public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);

        /// <summary>
        /// Sets the value of the IsEnabled attached property that indicates
        /// whether a selection rectangle can be used to select items or not.
        /// </summary>
        /// <param name="obj">Object on which to set the property.</param>
        /// <param name="value">Value to set.</param>
        public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

        private static void IsEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                {
                    // If we're enabling selection by a rectangle we can assume
                    // this means we want to be able to select more than one item.
                    if (listBox.SelectionMode == SelectionMode.Single) listBox.SelectionMode = SelectionMode.Extended;

                    attachedControls.Add(listBox, new ListBoxSelector(listBox));
                }
                else // Unregister the selector
                {
                    if (attachedControls.TryGetValue(listBox, out var selector))
                    {
                        attachedControls.Remove(listBox);
                        selector.UnRegister();
                    }
                }
            }
        }

        // Finds the nearest child of the specified type, or null if one wasn't found.
        private static T FindChild<T>(DependencyObject reference) where T : class
        {
            // Do a breadth first search.
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(reference);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (child is T obj) return obj;

                // Add the children to the queue to search through later.
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
                {
                    queue.Enqueue(VisualTreeHelper.GetChild(child, i));
                }
            }
            return null; // Not found.
        }

        private bool Register()
        {
            if ((scrollContent = FindChild<ScrollContentPresenter>(listBox)) != null)
            {
                autoScroller = new AutoScroller(listBox);
                autoScroller.OffsetChanged += OnOffsetChanged;

                selectionRect = new SelectionAdorner(scrollContent);
                scrollContent.AdornerLayer.Add(selectionRect);

                selector = new ItemsControlSelector(listBox);

                // The ListBox intercepts the regular MouseLeftButtonDown event
                // to do its selection processing, so we need to handle the
                // PreviewMouseLeftButtonDown. The scroll content won't receive
                // the message if we click on a blank area so use the ListBox.
                listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                listBox.PreviewKeyDown += OnPreviewKeyDown;
                listBox.MouseLeftButtonUp += OnMouseLeftButtonUp;
                listBox.MouseMove += OnMouseMove;
            }

            // Return success if we found the ScrollContentPresenter
            return (scrollContent != null);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    OnMouseLeftButtonUp(sender, null);
                    break;
            }
        }

        private void UnRegister()
        {
            StopSelection();

            // Remove all the event handlers so this instance can be reclaimed by the GC.
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.PreviewKeyDown -= OnPreviewKeyDown;
            listBox.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            listBox.MouseMove -= OnMouseMove;

            autoScroller.UnRegister();
        }

        private void OnListBoxLoaded(object sender, EventArgs e)
        {
            if (Register()) listBox.Loaded -= OnListBoxLoaded;
        }

        private void OnOffsetChanged(object sender, OffsetChangedEventArgs e)
        {
            selector.Scroll(e.HorizontalChange, e.VerticalChange);
            UpdateSelection();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (start == end)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item && item.IsMouseOver) goto End;
                }
                listBox.SelectedItems.Clear();
            }

        End:
            clicked = false;
            clickpoint = originalclickpoint = start = end = default;
            clickargs = null;

            if (mouseCaptured)
            {
                mouseCaptured = false;
                scrollContent.ReleaseMouseCapture();
                StopSelection();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (clicked && listBox.IsKeyboardFocusWithin)
            {
                if (!selecting)
                {
                    var mouse = e.GetPosition(scrollContent);
                    if ((clickpoint.X >= 0) && (clickpoint.X < scrollContent.ActualWidth) && (clickpoint.Y >= 0) && (clickpoint.Y < scrollContent.ActualHeight))
                    {
                        if (Math.Sqrt((clickpoint.X - mouse.X).Pow(2) + (clickpoint.Y - mouse.Y).Pow(2)) >= 1)
                        {
                            if (mouseCaptured = TryCaptureMouse()) StartSelection(originalclickpoint);
                            else clicked = false;
                        }
                    }
                }

                if (selecting && mouseCaptured)
                {
                    // Get the position relative to the content of the ScrollViewer.
                    end = e.GetPosition(scrollContent);
                    autoScroller.Update(end);
                    UpdateSelection();
                }
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check that the mouse is inside the scroll content (could be on the
            // scroll bars for example).
            if (!clicked) originalclickpoint = e.GetPosition(scrollContent);
            clickpoint = e.GetPosition(scrollContent);
            if ((clickpoint.X >= 0) && (clickpoint.X < scrollContent.ActualWidth) && (clickpoint.Y >= 0) && (clickpoint.Y < scrollContent.ActualHeight))
            {
                clickargs = e;
                clicked = true;
                listBox.Focus();
            }
        }

        private bool TryCaptureMouse()
        {
            // Check if there is anything under the mouse.
            if (scrollContent.InputHitTest(originalclickpoint) is UIElement element)
            {
                // Simulate a mouse click by sending it the MouseButtonDown
                // event based on the data we received.
                //var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left, e.StylusDevice) { RoutedEvent = Mouse.MouseDownEvent, Source = e.Source };
                element.RaiseEvent(clickargs);

                // The ListBox will try to capture the mouse unless something
                // else captures it.
                if (Mouse.Captured != listBox) return false; // Something else wanted the mouse, let it keep it.
            }

            // Either there's nothing under the mouse or the element doesn't want the mouse.
            return scrollContent.CaptureMouse();
        }

        /// <summary>
        /// Hide the selection rectangle and stop the auto scrolling.
        /// </summary>
        private void StopSelection() => selectionRect.IsEnabled = autoScroller.IsEnabled = selecting = false;

        private void StartSelection(Point location)
        {
            // We've stolen the MouseLeftButtonDown event from the ListBox
            // so we need to manually give it focus.
            listBox.Focus();

            start = location;
            end = location;

            // Do we need to start a new selection?
            if (((Keyboard.Modifiers & ModifierKeys.Control) == 0) && ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)) listBox.SelectedItems.Clear(); // Neither the shift key or control key is pressed, so clear the selection.

            selector.Reset();
            UpdateSelection();

            selectionRect.IsEnabled = autoScroller.IsEnabled = selecting = true;
        }

        private void UpdateSelection()
        {
            // Offset the start point based on the scroll offset.
            var start = autoScroller.TranslatePoint(this.start);

            // Draw the selecion rectangle.
            // Rect can't have a negative width/height...
            double x = Math.Min(start.X, end.X);
            double y = Math.Min(start.Y, end.Y);
            double width = Math.Abs(end.X - start.X);
            double height = Math.Abs(end.Y - start.Y);
            var area = new Rect(x, y, width, height);
            selectionRect.SelectionArea = area;

            // Select the items.
            // Transform the points to be relative to the ListBox.
            var topLeft = scrollContent.TranslatePoint(area.TopLeft, listBox);
            var bottomRight = scrollContent.TranslatePoint(area.BottomRight, listBox);

            // And select the items.
            selector.UpdateSelection(new Rect(topLeft, bottomRight));
        }

        /// <summary>
        /// Automatically scrolls an ItemsControl when the mouse is dragged outside
        /// of the control.
        /// </summary>
        private sealed class AutoScroller
        {
            private readonly DispatcherTimer autoScroll = new DispatcherTimer();
            private readonly ItemsControl itemsControl;
            private readonly ScrollViewer scrollViewer;
            private readonly ScrollContentPresenter scrollContent;
            private bool isEnabled;
            private Point offset;
            private Point mouse;

            /// <summary>
            /// Initializes a new instance of the AutoScroller class.
            /// </summary>
            /// <param name="itemsControl">The ItemsControl that is scrolled.</param>
            /// <exception cref="ArgumentNullException">itemsControl is null.</exception>
            public AutoScroller(ItemsControl itemsControl)
            {
                this.itemsControl = itemsControl ?? throw new ArgumentNullException("itemsControl");
                scrollViewer = FindChild<ScrollViewer>(itemsControl);
                scrollViewer.ScrollChanged += OnScrollChanged;
                scrollContent = FindChild<ScrollContentPresenter>(scrollViewer);

                autoScroll.Tick += delegate { PreformScroll(); };
                autoScroll.Interval = TimeSpan.FromMilliseconds(GetRepeatRate());
            }

            /// <summary>Occurs when the scroll offset has changed.</summary>
            public event EventHandler<OffsetChangedEventArgs> OffsetChanged;

            /// <summary>
            /// Gets or sets a value indicating whether the auto-scroller is enabled
            /// or not.
            /// </summary>
            public bool IsEnabled
            {
                get => isEnabled;
                set
                {
                    if (isEnabled != value)
                    {
                        isEnabled = value;

                        // Reset the auto-scroller and offset.
                        autoScroll.IsEnabled = false;
                        offset = new Point();
                    }
                }
            }

            /// <summary>
            /// Translates the specified point by the current scroll offset.
            /// </summary>
            /// <param name="point">The point to translate.</param>
            /// <returns>A new point offset by the current scroll amount.</returns>
            public Point TranslatePoint(Point point) => new Point(point.X - offset.X, point.Y - offset.Y);

            /// <summary>
            /// Removes all the event handlers registered on the control.
            /// </summary>
            public void UnRegister() => scrollViewer.ScrollChanged -= OnScrollChanged;

            /// <summary>
            /// Updates the location of the mouse and automatically scrolls if required.
            /// </summary>
            /// <param name="mouse">
            /// The location of the mouse, relative to the ScrollViewer's content.
            /// </param>
            public void Update(Point mouse)
            {
                this.mouse = mouse;

                // If scrolling isn't enabled then see if it needs to be.
                if (!autoScroll.IsEnabled) PreformScroll();
            }

            // Returns the default repeat rate in milliseconds.
            private static int GetRepeatRate()
            {
                // The RepeatButton uses the SystemParameters.KeyboardSpeed as the
                // default value for the Interval property. KeyboardSpeed returns
                // a value between 0 (400ms) and 31 (33ms).
                const double Ratio = (400.0 - 33.0) / 31.0;
                return 400 - (int)(SystemParameters.KeyboardSpeed * Ratio);
            }

            private double CalculateOffset(int startIndex, int endIndex)
            {
                double sum = 0;
                for (int i = startIndex; i != endIndex; i++)
                {
                    if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
                    {
                        // Height = Actual height + margin
                        sum += container.ActualHeight;
                        sum += container.Margin.Top + container.Margin.Bottom;
                    }
                }
                return sum;
            }

            private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                // Do we need to update the offset?
                if (IsEnabled)
                {
                    double horizontal = e.HorizontalChange;
                    double vertical = e.VerticalChange;

                    // VerticalOffset means two seperate things based on the CanContentScroll
                    // property. If this property is true then the offset is the number of
                    // items to scroll; false then it's in Device Independant Pixels (DIPs).
                    if (scrollViewer.CanContentScroll)
                    {
                        // We need to either increase the offset or decrease it.
                        if (e.VerticalChange < 0)
                        {
                            int start = (int)e.VerticalOffset;
                            int end = (int)(e.VerticalOffset - e.VerticalChange);
                            vertical = -CalculateOffset(start, end);
                        }
                        else
                        {
                            int start = (int)(e.VerticalOffset - e.VerticalChange);
                            int end = (int)e.VerticalOffset;
                            vertical = CalculateOffset(start, end);
                        }
                    }

                    offset.X += horizontal;
                    offset.Y += vertical;

                    OffsetChanged?.Invoke(this, new OffsetChangedEventArgs(horizontal, vertical));
                }
            }

            private void PreformScroll()
            {
                bool scrolled = false;

                if (mouse.X > scrollContent.ActualWidth)
                {
                    scrollViewer.LineRight();
                    scrolled = true;
                }
                else if (mouse.X < 0)
                {
                    scrollViewer.LineLeft();
                    scrolled = true;
                }

                if (mouse.Y > scrollContent.ActualHeight)
                {
                    scrollViewer.LineDown();
                    scrolled = true;
                }
                else if (mouse.Y < 0)
                {
                    scrollViewer.LineUp();
                    scrolled = true;
                }

                // It's important to disable scrolling if we're inside the bounds of
                // the control so that when the user does leave the bounds we can
                // re-enable scrolling and it will have the correct initial delay.
                autoScroll.IsEnabled = scrolled;
            }
        }

        /// <summary>Enables the selection of items by a specified rectangle.</summary>
        private sealed class ItemsControlSelector
        {
            private readonly ItemsControl itemsControl;
            private Rect previousArea;

            /// <summary>
            /// Initializes a new instance of the ItemsControlSelector class.
            /// </summary>
            /// <param name="itemsControl">
            /// The control that contains the items to select.
            /// </param>
            /// <exception cref="ArgumentNullException">itemsControl is null.</exception>
            public ItemsControlSelector(ItemsControl itemsControl) => this.itemsControl = itemsControl ?? throw new ArgumentNullException("itemsControl");

            /// <summary>
            /// Resets the cached information, allowing a new selection to begin.
            /// </summary>
            public void Reset() => previousArea = new Rect();

            /// <summary>
            /// Scrolls the selection area by the specified amount.
            /// </summary>
            /// <param name="x">The horizontal scroll amount.</param>
            /// <param name="y">The vertical scroll amount.</param>
            public void Scroll(double x, double y) => previousArea.Offset(-x, -y);

            /// <summary>
            /// Updates the controls selection based on the specified area.
            /// </summary>
            /// <param name="area">
            /// The selection area, relative to the control passed in the contructor.
            /// </param>
            public void UpdateSelection(Rect area)
            {
                // Check eack item to see if it intersects with the area.
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {
                    if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item)
                    {
                        // Get the bounds in the parent's co-ordinates.
                        var topLeft = item.TranslatePoint(new Point(0, 0), itemsControl);
                        var itemBounds = new Rect(topLeft.X, topLeft.Y, item.ActualWidth, item.ActualHeight);

                        // Only change the selection if it intersects with the area
                        // (or intersected i.e. we changed the value last time).
                        if (itemBounds.IntersectsWith(area))
                        {
                            Selector.SetIsSelected(item, true);
                        }
                        else if (itemBounds.IntersectsWith(previousArea))
                        {
                            // We previously changed the selection to true but it no
                            // longer intersects with the area so clear the selection.
                            Selector.SetIsSelected(item, false);
                        }
                    }
                }
                previousArea = area;
            }
        }

        /// <summary>The event data for the AutoScroller.OffsetChanged event.</summary>
        private sealed class OffsetChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the OffsetChangedEventArgs class.
            /// </summary>
            /// <param name="horizontal">The change in horizontal scroll.</param>
            /// <param name="vertical">The change in vertical scroll.</param>
            internal OffsetChangedEventArgs(double horizontal, double vertical)
            {
                HorizontalChange = horizontal;
                VerticalChange = vertical;
            }

            /// <summary>Gets the change in horizontal scroll position.</summary>
            public double HorizontalChange { get; }

            /// <summary>Gets the change in vertical scroll position.</summary>
            public double VerticalChange { get; }
        }

        /// <summary>Draws a selection rectangle on an AdornerLayer.</summary>
        private sealed class SelectionAdorner : Adorner
        {
            private Rect selectionRect;

            /// <summary>
            /// Initializes a new instance of the SelectionAdorner class.
            /// </summary>
            /// <param name="parent">
            /// The UIElement which this instance will overlay.
            /// </param>
            /// <exception cref="ArgumentNullException">parent is null.</exception>
            public SelectionAdorner(UIElement parent) : base(parent)
            {
                // Make sure the mouse doesn't see us.
                IsHitTestVisible = false;

                // We only draw a rectangle when we're enabled.
                IsEnabledChanged += delegate { InvalidateVisual(); };
            }

            /// <summary>Gets or sets the area of the selection rectangle.</summary>
            public Rect SelectionArea
            {
                get => selectionRect;
                set
                {
                    selectionRect = value;
                    InvalidateVisual();
                }
            }

            /// <summary>
            /// Participates in rendering operations that are directed by the layout system.
            /// </summary>
            /// <param name="drawingContext">The drawing instructions.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (IsEnabled)
                {
                    // Make the lines snap to pixels (add half the pen width [0.5])
                    double[] x = { SelectionArea.Left + 0.5, SelectionArea.Right + 0.5 };
                    double[] y = { SelectionArea.Top + 0.5, SelectionArea.Bottom + 0.5 };
                    drawingContext.PushGuidelineSet(new GuidelineSet(x, y));

                    Brush fill = SystemColors.HighlightBrush.Clone();
                    fill.Opacity = 0.4;
                    drawingContext.DrawRectangle(fill, new Pen(SystemColors.HighlightBrush, 1.0), SelectionArea);
                }
            }
        }
    }
}