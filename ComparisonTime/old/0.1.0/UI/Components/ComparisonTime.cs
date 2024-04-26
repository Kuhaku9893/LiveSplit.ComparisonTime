using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class ComparisonTime : IComponent
    {
        protected InfoTimeComponent InternalComponent { get; set; }
        public ComparisonTimeSettings Settings { get; set; }
        private RegularTimeFormatter Formatter { get; set; }
        private string PreviousInformationName { get; set; }

        public string ComponentName => "Comparison Time";

        public float HorizontalWidth => InternalComponent.HorizontalWidth;
        public float MinimumHeight   => InternalComponent.MinimumHeight;
        public float VerticalHeight  => InternalComponent.VerticalHeight;
        public float MinimumWidth    => InternalComponent.MinimumWidth;

        public float PaddingTop     => InternalComponent.PaddingTop;
        public float PaddingBottom  => InternalComponent.PaddingBottom;
        public float PaddingLeft    => InternalComponent.PaddingLeft;
        public float PaddingRight   => InternalComponent.PaddingRight;

        public IDictionary<string, Action> ContextMenuControls => null;

        public ComparisonTime(LiveSplitState state)
        {
            Settings = new ComparisonTimeSettings()
            {
                CurrentState = state
            };
            Formatter = new RegularTimeFormatter(Settings.Accuracy)
            {
                NullFormat = NullFormat.Dash,
            };
            InternalComponent = new InfoTimeComponent(null, null, Formatter);
            state.ComparisonRenamed += state_ComparisonRenamed;
        }

        void state_ComparisonRenamed(object sender, EventArgs e)
        {
            var args = (RenameEventArgs)e;
            if (Settings.Comparison == args.OldName)
            {
                Settings.Comparison = args.NewName;
                ((LiveSplitState)sender).Layout.HasChanged = true;
            }
        }

        private void PrepareDraw(LiveSplitState state)
        {
            InternalComponent.DisplayTwoRows = Settings.Display2Rows;

            InternalComponent.NameLabel.HasShadow 
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            Formatter.Accuracy = Settings.Accuracy;

            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = Settings.OverrideTimeColor ? Settings.TimeColor : state.LayoutSettings.TextColor;
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Settings.BackgroundColor.A > 0
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.A > 0)
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(g, state, width, VerticalHeight);
            PrepareDraw(state);
            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(g, state, HorizontalWidth, height);
            PrepareDraw(state);
            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        protected void SetAlternateText(string comparison)
        {
            switch (comparison)
            {
                case Run.PersonalBestComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        "PB",
                    };
                    break;
                case AverageSegmentsComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        AverageSegmentsComparisonGenerator.ShortComparisonName,
                    };
                    break;
                case BestSegmentsComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        BestSegmentsComparisonGenerator.ShortComparisonName,
                    };
                    break;
                case LatestRunComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        LatestRunComparisonGenerator.ShortComparisonName,
                    };
                    break;
                case MedianSegmentsComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        MedianSegmentsComparisonGenerator.ShortComparisonName,
                    };
                    break;
                case PercentileComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        PercentileComparisonGenerator.ShortComparisonName,
                    };
                    break;
                case WorstSegmentsComparisonGenerator.ComparisonName:
                    InternalComponent.AlternateNameText = new[]
                    {
                        WorstSegmentsComparisonGenerator.ShortComparisonName,
                    };
                    break;
                default:
                    // ShortComaprisonNameがない
                    // BestSplitTimesComparisonGenerator
                    break;
            }
        }
        protected TimeSpan? GetTimeValue(LiveSplitState state, string comparison)
        {
            if ((state.CurrentPhase == TimerPhase.Ended) || (state.CurrentPhase == TimerPhase.NotRunning) ||
                (Settings.Type == TimeType.GoalTime))
                return state.Run.Last().Comparisons[comparison][state.CurrentTimingMethod];

            if (Settings.Type == TimeType.SplitTime)
                return state.Run[state.CurrentSplitIndex].Comparisons[comparison][state.CurrentTimingMethod];

            // Settings.Type == TimeType.SegmentTime
            var currentComparisonSplitTime = state.Run[state.CurrentSplitIndex].Comparisons[comparison][state.CurrentTimingMethod];
            if (state.CurrentSplitIndex == 0)
                return currentComparisonSplitTime;
            var previousComparisonSplitTime = state.Run[state.CurrentSplitIndex - 1].Comparisons[comparison][state.CurrentTimingMethod];
            return currentComparisonSplitTime - previousComparisonSplitTime;
        }
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            var comparison = Settings.Comparison == "Current Comparison" ? state.CurrentComparison : Settings.Comparison;
            if (!state.Run.Comparisons.Contains(comparison))
                comparison = state.CurrentComparison;

            InternalComponent.InformationName = comparison;
            if (InternalComponent.InformationName != PreviousInformationName)
            {
                SetAlternateText(comparison);
                PreviousInformationName = InternalComponent.InformationName;
            }

            InternalComponent.TimeValue = GetTimeValue(state, comparison);
            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose(){ }

        public int GetSettingsHashCode() => Settings.GetSettingsHashCode();
    }
}
