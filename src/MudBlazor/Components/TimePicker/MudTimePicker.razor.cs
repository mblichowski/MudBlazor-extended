﻿using Microsoft.AspNetCore.Components;
using MudBlazor.Extensions;
using MudBlazor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace MudBlazor
{
    public partial class MudTimePicker : MudBasePicker
    {

        /// <summary>
        /// Reference to the Picker, initialized via @ref
        /// </summary>
        private MudPicker Picker;

        /// <summary>
        /// First view to show in the MudDatePicker.
        /// </summary>
        [Parameter] public OpenTo OpenTo { get; set; } = OpenTo.Hours;

        /// <summary>
        /// If true, sets 12 hour selection clock.
        /// </summary>
        [Parameter] public bool AmPm { get; set; }

        /// <summary>
        /// Sets the Input Icon.
        /// </summary>
        [Parameter] public string InputIcon { get; set; } = Icons.Material.Event;

        protected TimeSpan? _time;

        /// <summary>
        /// The currently selected time (two-way bindable). If null, then nothing was selected.
        /// </summary>
        [Parameter]
        public TimeSpan? Time
        {
            get => _time;
            set
            {
                if (value == _time)
                    return;
                _time = value;
                Value = AmPm ? _time.ToAmPmString() : _time.ToIsoString();
                InvokeAsync(StateHasChanged);
                TimeChanged.InvokeAsync(value);
            }
        }

        /// <summary>
        /// Fired when the date changes.
        /// </summary>
        [Parameter] public EventCallback<TimeSpan?> TimeChanged { get; set; }

        protected override void StringValueChanged(string value)
        {
               Time = ParseTimeValue(value);
        }

        private TimeSpan? ParseTimeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            bool pm = false;
            var value1 = value.Trim();
            var m = Regex.Match(value, "AM|PM", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                AmPm = true; // <-- this is kind of a hack, but we need to make sure it is set or else the string value might be converted to 24h format.
                pm = m.Value.ToLower() == "pm";
                value1 = Regex.Replace(value, "(AM|am|PM|pm)", "").Trim();
            }

            if (TimeSpan.TryParse(value1, out var time))
            {
                if (pm)
                    time = new TimeSpan(time.Hours + 12, time.Minutes, 0);
                return time;
            }
                
            return null;
        }

        private string getHourString()
        {
            if (_time == null)
                return "--";
            var h=AmPm ? _time.Value.ToAmPmHour() : _time.Value.Hours;
            return Math.Min(23, Math.Max(0, h)).ToString(CultureInfo.InvariantCulture);
        }

        private string getMinuteString()
        {
            if (_time == null)
                return "--";
            return $"{Math.Min(59, Math.Max(0, _time.Value.Minutes)):D2}";
        }

        private void UpdateTime()
        {
            Time = new TimeSpan(TimeSet.Hour, TimeSet.Minute, 0);
        }

        private async void OnHourClick()
        {
            OpenTo = OpenTo.Hours;
            StateHasChanged();
        }

        private async void OnMinutesClick()
        {
            OpenTo = OpenTo.Minutes;
            StateHasChanged();
        }

        private void OnAmClicked()
        {
            TimeSet.Hour = TimeSet.Hour % 12;
            UpdateTime();
            StateHasChanged();
        }

        private void OnPmClicked()
        {
            if (TimeSet.Hour < 12)
                TimeSet.Hour = TimeSet.Hour + 12;
            TimeSet.Hour = TimeSet.Hour % 12;
            UpdateTime();
            StateHasChanged();
        }

        protected string ToolbarClass =>
        new CssBuilder()
          .AddClass($"mud-picker-timepicker-toolbar")
          .AddClass($"mud-picker-timepicker-toolbar-landscape", Orientation == Orientation.Landscape && PickerVariant == PickerVariant.Static)
        .Build();

        private string GetClockPinColor()
        {
           return $"mud-picker-time-clock-pin mud-color-{Color.ToDescriptionString()}";
        }
        private string GetClockPointerColor()
        {
            return $"mud-picker-time-clock-pointer mud-picker-time-clock-pointer-animation mud-color-{Color.ToDescriptionString()}";
        }

        private string GetClockPointerThumbColor()
        {
            return $"mud-picker-time-clock-pointer-thumb mud-color-{Color.ToDescriptionString()}";
        }

        private string GetPointerRotation()
        {
            double deg = 0;
            if (OpenTo == OpenTo.Hours)
                deg = (TimeSet.Hour * 30) % 360;
            if (OpenTo == OpenTo.Minutes)
                deg = (TimeSet.Minute * 6) % 360;
            return $"rotateZ({deg}deg);";
        }

        private string GetPointerHeight()
        {
            int height = 40;
            if (OpenTo == OpenTo.Minutes)
                height = 40;
            if (OpenTo == OpenTo.Hours)
            {
                if (!AmPm && TimeSet.Hour > 0 && TimeSet.Hour < 13)
                    height = 26;
                else
                    height = 40;
            }
            return $"{height}%;";
        }

        private SetTime TimeSet = new SetTime();

        protected override void OnInitialized()
        {
            if (_time == null)
            {
                TimeSet.Hour = 0;
                TimeSet.Minute = 0;
                return;
            }
            if (AmPm)
                TimeSet.Hour = _time.Value.ToAmPmHour();
            else 
                TimeSet.Hour = _time.Value.Hours;
            TimeSet.Minute = _time.Value.Minutes;
        }

        public bool MouseDown { get; set; }

        private void OnMouseDown(MouseEventArgs e)
        {
            MouseDown = true;
        }

        private void OnMouseUp(MouseEventArgs e)
        {
            MouseDown = false;
        }

        private void OnMouseOverHour(SetTime setTime)
        {
            if(MouseDown)
            {
                TimeSet.Hour = setTime.Hour;
                UpdateTime();
            }
        }

        private void OnMouseOverMinute(SetTime setTime)
        {
            if(MouseDown)
            {
                TimeSet.Minute = setTime.Minute;
                UpdateTime();
            }
        }

        private class SetTime
        {
            [Obsolete("not needed, calculated on the fly")]
            public int Degrees { get; set; }

            [Obsolete("not needed, calculated on the fly")]
            public int Height { get; set; }

            public int Hour { get; set; }

            public int Minute { get; set; }
        }
    }
}
