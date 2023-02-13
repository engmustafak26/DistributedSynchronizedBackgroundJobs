namespace DistributedSynchronizedBackgroundJobs.NumericRandomGenerator
{
    public class NumericRandomGenerator : INumericRandomGenerator
    {
        public int Generate(int maxNumberThreshold)
        {
          return new Random().Next(maxNumberThreshold); // we can use only instance of Random variable instead of new instance each time to generate a unique random number
                                                        // but instead I simulate scenario when multiple background instances run at same time
        }
    }
}
