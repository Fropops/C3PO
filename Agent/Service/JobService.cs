using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Agent.Service
{
    internal interface IJobService
    {
        void RegisterJob(int processId, string name);
        bool RemoveJob(int processId);
        Job GetJob(int processId);
        Job GetJobById(int id);
        List<Job> GetJobs();
    }
    internal class JobService : IJobService
    {
        private int NextId = 0;
        private ConcurrentDictionary<int, Job> Jobs = new ConcurrentDictionary<int, Job>();

        public void RegisterJob(int processId, string name)
        {
            var job = new Job(this.NextId++, processId, name);
            this.Jobs.AddOrUpdate(job.ProcessId, job, (key, value) => value);
        }

        public bool RemoveJob(int processId)
        {
            return this.Jobs.TryRemove(processId, out Job _);
        }

        public Job GetJob(int processId)
        {
            this.Jobs.TryGetValue(processId, out Job job);
            return job;
        }

        public Job GetJobById(int id)
        {
            return this.Jobs.Values.FirstOrDefault(j => j.Id == id);
        }

        public List<Job> GetJobs()
        {
            return this.Jobs.Values.ToList();
        }
    }
}
