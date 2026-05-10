using System;
using System.Collections.Generic;
using System.Linq;
 
// Represents a job submitted to the system before it is admitted to memory
class Job
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }   // When the job arrives at the system
    public int ExecutionTime { get; set; } // CPU time the job requires
    public int Priority { get; set; }      // Used by the long-term scheduler to decide admission
}
 
class Program
{
    static void Main(string[] args)
    {
        // Full job pool — these represent all jobs submitted to the system
        var jobs = new List<Job>
        {
            new Job { Name = "A", ArrivalTime = 0, ExecutionTime = 3, Priority = 5 },
            new Job { Name = "B", ArrivalTime = 2, ExecutionTime = 6, Priority = 4 },
            new Job { Name = "C", ArrivalTime = 5, ExecutionTime = 5, Priority = 8 },
            new Job { Name = "D", ArrivalTime = 6, ExecutionTime = 3, Priority = 6 },
            new Job { Name = "E", ArrivalTime = 8, ExecutionTime = 6, Priority = 10 },
            new Job { Name = "F", ArrivalTime = 9, ExecutionTime = 2, Priority = 3 },
            new Job { Name = "G", ArrivalTime = 10, ExecutionTime = 6, Priority = 7 }
        };
 
        // Long-term scheduling decision: only admit jobs with Priority > 5.
        // This controls the degree of multiprogramming by limiting which jobs
        // move from the job queue into main memory (the ready queue).
        var admittedJobs = jobs.Where(job => job.Priority > 5).ToList();
 
        Console.WriteLine("Admitted Jobs (Priority > 5):");
        Console.WriteLine($"{"Job",-6} {"Priority",-10} {"Arrival Time",-15} {"Execution Time"}");
        Console.WriteLine(new string('-', 45));
 
        // Display each admitted job
        foreach (var job in admittedJobs)
        {
            Console.WriteLine($"{job.Name,-6} {job.Priority,-10} {job.ArrivalTime,-15} {job.ExecutionTime}");
        }
    }
}
 