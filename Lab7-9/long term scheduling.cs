using System;
using System.Collections.Generic;
using System.Linq;

class Job
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }
    public int ExecutionTime { get; set; }
    public int Priority { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Table 1: Long-term Scheduling Table
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

        // Admit only jobs with Priority > 5
        var admittedJobs = jobs.Where(job => job.Priority > 5).ToList();

        Console.WriteLine("Admitted Jobs (Priority > 5):");
        Console.WriteLine($"{"Job",-6} {"Priority",-10} {"Arrival Time",-15} {"Execution Time"}");
        Console.WriteLine(new string('-', 45));

        foreach (var job in admittedJobs)
        {
            Console.WriteLine($"{job.Name,-6} {job.Priority,-10} {job.ArrivalTime,-15} {job.ExecutionTime}");
        }
    }
}