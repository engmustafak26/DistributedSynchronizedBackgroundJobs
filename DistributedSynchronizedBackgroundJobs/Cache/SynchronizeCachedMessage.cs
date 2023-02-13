namespace DistributedSynchronizedBackgroundJobs.Cache
{
    public class SynchronizeCachedMessage
    {
        public bool IsStarted { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CompleteDate { get; set; }
        public string ServerName { get; set; }
        public int ProcessId { get; set; }

    }
}
