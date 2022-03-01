namespace IsBookingOpenedAPI.Helpers
{
    public sealed class PollingSingleton
    {
        PollingSingleton() { }
        private static readonly object lockObj = new object();
        private static PollingSingleton instance = null;
        public static Task? pollingListener = null;
        public static Task? pollingListenerHeroku = null;
        public static PollingSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new PollingSingleton();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
