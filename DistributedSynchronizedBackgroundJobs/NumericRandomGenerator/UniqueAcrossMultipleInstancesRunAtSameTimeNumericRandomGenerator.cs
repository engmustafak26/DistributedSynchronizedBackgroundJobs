using System.Text;

namespace DistributedSynchronizedBackgroundJobs.NumericRandomGenerator
{
    public class UniqueAcrossMultipleInstancesRunAtSameTimeNumericRandomGenerator : INumericRandomGenerator
    {
        const int MaxGuidValue = 3444; // ASCII Sum for Guid "ffffffff-ffff-ffff-ffff-ffffffffffff"
        const string guidSeperatorCharcter = "-";

        public int Generate(int maxNumberThreshold)
        {
            var guidString = GenerateGuidString(maxNumberThreshold);

            int sum = 0;
            foreach (int guidCharcterASCIICode in guidString)
            {
                if (guidCharcterASCIICode.ToString() == guidSeperatorCharcter)
                    continue;

                sum += guidCharcterASCIICode;
            }

            return (sum % maxNumberThreshold);

        }


        private string GenerateGuidString(int maxNumberThreshold)
        {
            StringBuilder stringBuilder = new();
            var guidCount = Math.Ceiling((double)maxNumberThreshold / MaxGuidValue);
            for (int i = 0; i < guidCount; i++)
            {
                stringBuilder.Append(Guid.NewGuid().ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
