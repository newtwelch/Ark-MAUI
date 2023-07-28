using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ark.Models.Toast
{
    internal class ToastService : IDisposable
    {
        public event Action<string, string, ToastLevel> OnShow;
        public event Action OnHide;
        public System.Timers.Timer Countdown;

        public void ShowToast(string message, string heading, ToastLevel level)
        {
            OnShow?.Invoke(message, heading, level);
            StartCountdown();
        }
        private void StartCountdown()
        {
            SetCountdown();

            if (Countdown!.Enabled)
            {
                Countdown.Stop();
                Countdown.Start();
            }
            else
            {
                Countdown!.Start();
            }
        }

        private void SetCountdown()
        {
            if (Countdown != null) return;

            Countdown = new System.Timers.Timer(2000);
            Countdown.Elapsed += HideToast;
            Countdown.AutoReset = false;
        }

        private void HideToast(object source, ElapsedEventArgs args) => OnHide?.Invoke();
        public void Dispose() => Countdown?.Dispose();
    }

    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
}
