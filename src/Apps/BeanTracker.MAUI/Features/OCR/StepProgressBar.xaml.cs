namespace BeanTracker.MAUI.Features.OCR;

/// <summary>
/// A step progress bar component that uses a hidden Shiny Slider underneath
/// and renders visual segmented bars with divider strokes between them.
/// The number of steps is dynamic (set via <see cref="TotalSteps"/>),
/// and the current step is tracked via <see cref="CurrentStep"/>.
/// </summary>
public sealed partial class StepProgressBar : ContentView
{
    public StepProgressBar()
    {
        InitializeComponent();
        SegmentsLayout.SizeChanged += OnSegmentsLayoutSizeChanged;
    }

    #region Bindable Properties

    public static readonly BindableProperty TotalStepsProperty = BindableProperty.Create(
        nameof(TotalSteps),
        typeof(int),
        typeof(StepProgressBar),
        defaultValue: 4,
        propertyChanged: OnStepPropertyChanged);

    public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(
        nameof(CurrentStep),
        typeof(int),
        typeof(StepProgressBar),
        defaultValue: 0,
        propertyChanged: OnStepPropertyChanged);

    public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create(
        nameof(ActiveColor),
        typeof(Color),
        typeof(StepProgressBar),
        defaultValue: Color.FromArgb("#3B82F6"),
        propertyChanged: OnStepPropertyChanged);

    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(StepProgressBar),
        defaultValue: Color.FromArgb("#D1E3FF"),
        propertyChanged: OnStepPropertyChanged);

    public static readonly BindableProperty SegmentHeightProperty = BindableProperty.Create(
        nameof(SegmentHeight),
        typeof(double),
        typeof(StepProgressBar),
        defaultValue: 4.0,
        propertyChanged: OnStepPropertyChanged);

    /// <summary>Total number of step segments to display.</summary>
    public int TotalSteps
    {
        get => (int)GetValue(TotalStepsProperty);
        set => SetValue(TotalStepsProperty, value);
    }

    /// <summary>The current active step (0-based). Segments 0 .. CurrentStep-1 are "active".</summary>
    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    /// <summary>Color for completed/active segments.</summary>
    public Color ActiveColor
    {
        get => (Color)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    /// <summary>Color for remaining/inactive segments.</summary>
    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    /// <summary>Height of each segment bar.</summary>
    public double SegmentHeight
    {
        get => (double)GetValue(SegmentHeightProperty);
        set => SetValue(SegmentHeightProperty, value);
    }

    #endregion

    private static void OnStepPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StepProgressBar bar)
            bar.RebuildSegments();
    }

    private void RebuildSegments()
    {
        SegmentsLayout.Children.Clear();

        var total = Math.Max(1, TotalSteps);
        var current = Math.Clamp(CurrentStep, 0, total);

        // Also sync the hidden Shiny Slider value
        HiddenSlider.Maximum = total;
        HiddenSlider.Value = current;

        for (int i = 0; i < total; i++)
        {
            var isActive = i < current;

            var segment = new BoxView
            {
                Color = isActive ? ActiveColor : InactiveColor,
                HeightRequest = SegmentHeight,
                CornerRadius = SegmentHeight / 2,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            SegmentsLayout.Children.Add(segment);
        }

        // Recalculate widths if layout size is already known
        DistributeSegmentWidths(SegmentsLayout.Width);
    }

    /// <summary>
    /// Recalculates segment widths when the layout container resizes,
    /// distributing available width equally across all segments.
    /// </summary>
    private void OnSegmentsLayoutSizeChanged(object? sender, EventArgs e)
    {
        DistributeSegmentWidths(SegmentsLayout.Width);
    }

    private void DistributeSegmentWidths(double availableWidth)
    {
        if (SegmentsLayout.Children.Count == 0 || availableWidth <= 0)
            return;

        var totalSteps = SegmentsLayout.Children.Count;
        var spacing = SegmentsLayout.Spacing;
        var totalSpacing = spacing * (totalSteps - 1);
        var segmentWidth = (availableWidth - totalSpacing) / totalSteps;

        if (segmentWidth > 0)
        {
            foreach (var child in SegmentsLayout.Children)
            {
                if (child is BoxView box)
                    box.WidthRequest = segmentWidth;
            }
        }
    }
}

