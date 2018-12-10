using System.Diagnostics;
using Xunit;

namespace Hashids.net.core.Tests
{
    public class HashidsPerfTests
    {
        [Fact]
        private void Encode_single()
        {
            var hashids = new Hashids();
            var stopWatch = Stopwatch.StartNew();
            for (var i = 1; i < 10001; i++)
            {
                hashids.Encode(i);
            }
            stopWatch.Stop();
            Trace.WriteLine($"10 000 encodes: {stopWatch.ElapsedMilliseconds}");
        }
    }
}
