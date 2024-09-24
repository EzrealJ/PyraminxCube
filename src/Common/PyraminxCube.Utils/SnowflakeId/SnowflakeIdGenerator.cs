using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.SnowflakeId
{
    public class SnowflakeIdGenerator(ISnowflakeIdSettings settings)
    {
        public const long TWEPOCH = 1288834974657L;

        private const int WORKER_ID_BITS = 5;
        private const int DATACENTER_ID_BITS = 5;
        private const int SEQUENCE_BITS = 12;
        internal const long MAX_WORKER_ID = -1L ^ (-1L << WORKER_ID_BITS);
        internal const long MAX_DATACENTER_ID = -1L ^ (-1L << DATACENTER_ID_BITS);

        private const int WORKER_ID_SHIFT = SEQUENCE_BITS;
        private const int DATACENTER_ID_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS;
        public const int TIMESTAMP_LEFT_SHIFT = SEQUENCE_BITS + WORKER_ID_BITS + DATACENTER_ID_BITS;
        private const long SEQUENCE_MASK = -1L ^ (-1L << SEQUENCE_BITS);

        private readonly object _lock = new();
        private long _lastTimestamp = -1L;
        /// <summary>
        /// 
        /// </summary>
        public long WorkerId { get; } = settings.WorkerId;
        /// <summary>
        /// 
        /// </summary>
        public long DatacenterId { get; } = settings.DataCenterId;
        /// <summary>
        /// 序列号
        /// </summary>
        public long Sequence { get; internal set; } = 0L;

        /// <summary>
        /// 生成下一个Id
        /// </summary>
        /// <returns>返回雪花Id</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public long NextId()
        {
            // sanity check for workerId
            if (WorkerId > MAX_WORKER_ID || WorkerId < 0)
            {
                throw new ArgumentException($"worker Id can't be greater than {MAX_WORKER_ID} or less than 0");
            }

            if (DatacenterId > MAX_DATACENTER_ID || DatacenterId < 0)
            {
                throw new ArgumentException($"datacenter Id can't be greater than {MAX_DATACENTER_ID} or less than 0");
            }

            lock (_lock)
            {
                long timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    throw new Exception($"InvalidSystemClock: Clock moved backwards, Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
                }

                if (_lastTimestamp == timestamp)
                {
                    Sequence = (Sequence + 1) & SEQUENCE_MASK;
                    if (Sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    Sequence = 0;
                }

                _lastTimestamp = timestamp;
                long id = ((timestamp - TWEPOCH) << TIMESTAMP_LEFT_SHIFT) |
                         (DatacenterId << DATACENTER_ID_SHIFT) |
                         (WorkerId << WORKER_ID_SHIFT) | Sequence;

                return id;
            }
        }

        private static long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }

            return timestamp;
        }

        private static long TimeGen() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
