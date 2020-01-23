using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FotoShoutUtils.Service {
    public abstract class Executor
    {
        static ILog _logger = LogManager.GetLogger(typeof(Executor));
        
        const string APP_SETTINGS_TIMERINTERVAL = "TimerInterval";

        Timer _timer;

        public int Interval { get; set; }

        protected int MinimumSecond = 0;

        public string Error { get; set; }


        public virtual bool Initialize(bool timerUsed) {
            
            if (timerUsed)
                InitTimer();

            return true;

        }

        public virtual bool DeInitialize() {
            return true;
        }

        protected Executor() {
        }

        void LoadIntervalSetting() {
            Interval = MinimumSecond;

            try {
                string intervalStr = System.Configuration.ConfigurationManager.AppSettings[APP_SETTINGS_TIMERINTERVAL];

                if (!string.IsNullOrEmpty(intervalStr)) {
                    int setinterval;
                    if (int.TryParse(intervalStr.Trim(), out setinterval)) {
                        if (setinterval >= MinimumSecond) {
                            Interval = setinterval;
                        }
                    }
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }
        }

        void InitTimer() {
            LoadIntervalSetting();

            _timer = new Timer();


            _timer.Interval = this.Interval * 1000;
            _timer.Elapsed += new ElapsedEventHandler(Timer_OnElapsed);
            _timer.Enabled = true;
        }

        /// <summary>
        /// the procedure called when timer triggered.
        /// </summary>
        public abstract void Execute();

        protected void Timer_OnElapsed(object sender, ElapsedEventArgs e) {
            _timer.Stop();

            try {
                Execute();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }
            _timer.Start();
        }

    }
}
