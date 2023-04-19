using SmartHome.Data.DTO;
using System.Collections.Concurrent;

namespace SmartHome.Api.Utilities
{
    public class ScenariosQueue
    {
        private readonly ConcurrentQueue<ScenarioDto> _queue = new ConcurrentQueue<ScenarioDto>();

        public void Enqueue(ScenarioDto scenario)
        {
            _queue.Enqueue(scenario);
        }

        public bool TryDequeue(out ScenarioDto scenario)
        {
            return _queue.TryDequeue(out scenario);
        }
    }
}
