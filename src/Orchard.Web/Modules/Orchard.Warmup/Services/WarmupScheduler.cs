using System;
using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Warmup.Services {
    public interface IWarmupEventHandler : IEventHandler {
        void Generate(bool force);
    }
    
    public interface IJobsQueueService : IEventHandler {
        void Enqueue(string message, object parameters, int priority);
    }

    public class WarmupScheduler : IWarmupScheduler, IWarmupEventHandler {
        private readonly IJobsQueueService _jobsQueueService;
        private readonly Lazy<IWarmupUpdater> _warmupUpdater;

        public WarmupScheduler(
            IJobsQueueService jobsQueueService,
            Lazy<IWarmupUpdater> warmupUpdater ) {
            _jobsQueueService = jobsQueueService;
            _warmupUpdater = warmupUpdater;
        }

        public void Schedule(bool force) {
            _jobsQueueService.Enqueue(
                "IWarmupEventHandler.Generate", 
                new Dictionary<string, object> { { "force", force } }, 
                10);
        }

        public void Generate(bool force) {
            if(force) {
                _warmupUpdater.Value.Generate();
            }
            else {
                _warmupUpdater.Value.EnsureGenerate();
            }
        }
    }
}