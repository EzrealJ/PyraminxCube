using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.SnowflakeId
{
    public class SnowflakeIdGenerator(ISnowflakeIdSettings settings)
    {
        // 获取2024年的utc时间  
        private static readonly DateTime _utc2024 = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // 获取1970年的utc时间
        private static readonly DateTime _utc1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly long Epoch = (long)(_utc2024 - _utc1970).TotalMilliseconds;
        

        private const int WORKER_ID_BITS = 5;
        private const int DATACENTER_ID_BITS = 5;
        private const int SEQUENCE_BITS = 12;
        internal const long MAX_WORKER_ID = -1L ^ (-1L << WORKER_ID_BITS);
        internal const long MAX_DATACENTER_ID = -1L ^ (-1L << DATACENTER_ID_BITS);

        private const int WORKER_ID_SHIFT = SEQUENCE_BITS;
        private const int DATACENTER_ID_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS;
        public const int TIMESTAMP_LEFT_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS + DATACENTER_ID_BITS;
        private const long SEQUENCE_MASK = -1L ^ (-1L << SEQUENCE_BITS);

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;
        /// <summary>
        /// 
        /// </summary>
        public long WorkerId { get; } = settings.WorkerId;
        /// <summary>
        /// 
        /// </summary>
        public long DatacenterId { get; } = settings.DataCenterId;


        /// <summary>
        /// 生成下一个Id
        /// </summary>
        /// <returns>返回雪花Id</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public long NextId()
        {
            long timestamp = TimeGen();

            if (timestamp < _lastTimestamp)
            {
                // Handle clock moving backwards
                throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
            }

            if (_lastTimestamp == timestamp)
            {
                // Same millisecond; increment sequence using Interlocked
                _sequence = Interlocked.Increment(ref _sequence) & SEQUENCE_MASK;
                if (_sequence == 0)
                {
                    // Sequence exhausted; wait until next millisecond
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                // New millisecond; reset sequence
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            long id = ((timestamp - Epoch) << TIMESTAMP_LEFT_SHIFT) |
                      (DatacenterId << DATACENTER_ID_SHIFT) |
                      (WorkerId << WORKER_ID_SHIFT) |
                      _sequence;

            return id;
        }

        private static long TilNextMillis(long lastTimestamp)
        {
            long timestamp;
            do
            {
                timestamp = TimeGen();
            } while (timestamp <= lastTimestamp);
            return timestamp;
        }

        private static long TimeGen() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
