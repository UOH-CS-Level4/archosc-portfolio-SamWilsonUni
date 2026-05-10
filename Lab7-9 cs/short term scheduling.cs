using System;
using System.Collections.Generic;
 
// Represents a process managed by the short-term (CPU) scheduler
class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }             // When the process enters the ready queue
    public int ExecutionTime { get; set; }           // Total CPU burst time required
    public int FinishTime { get; set; }              // Clock time when execution completes
    public int TurnaroundTime { get; set; }          // FinishTime - ArrivalTime
    public double NormalisedTurnaround { get; set; } // TurnaroundTime / ExecutionTime
}
 
class FCFS
{
    static void Main(string[] args)
    {
        // Process table — ordered by arrival time as FCFS requires
        var processes = new List<Process>
        {
            new Process { Name = "A", ArrivalTime = 0, ExecutionTime = 3 },
            new Process { Name = "B", ArrivalTime = 2, ExecutionTime = 6 },
            new Process { Name = "C", ArrivalTime = 5, ExecutionTime = 5 },
            new Process { Name = "D", ArrivalTime = 6, ExecutionTime = 3 },
            new Process { Name = "E", ArrivalTime = 8, ExecutionTime = 6 },
            new Process { Name = "F", ArrivalTime = 9, ExecutionTime = 2 },
            new Process { Name = "G", ArrivalTime = 10, ExecutionTime = 6 }
        };
 
        RunFCFS(processes);
        PrintResults(processes);
        PrintGanttChart(processes);
    }
 
    // Simulates First Come First Served (FCFS) scheduling.
    // FCFS is non-preemptive: once a process starts it runs to completion.
    // Processes are served in arrival order with no priority consideration.
    static void RunFCFS(List<Process> processes)
    {
        int currentTime = 0;
 
        foreach (var process in processes)
        {
            // If the CPU finishes before the next process arrives, advance the clock
            // to that process's arrival (CPU sits idle in the gap)
            if (currentTime < process.ArrivalTime)
                currentTime = process.ArrivalTime;
 
            // Run the process to completion — FCFS is non-preemptive
            process.FinishTime = currentTime + process.ExecutionTime;
            process.TurnaroundTime = process.FinishTime - process.ArrivalTime;
            process.NormalisedTurnaround = (double)process.TurnaroundTime / process.ExecutionTime;
 
            // Advance the clock past this process's finish time
            currentTime = process.FinishTime;
        }
    }
 
    // Prints a per-process results table including turnaround metrics and averages
    static void PrintResults(List<Process> processes)
    {
        Console.WriteLine("FCFS Scheduling Results:");
        Console.WriteLine($"{"Process",-10} {"Arrival",-10} {"Exec",-8} {"Finish",-10} {"Turnaround",-14} {"Norm. Turnaround"}");
        Console.WriteLine(new string('-', 65));
 
        foreach (var p in processes)
        {
            Console.WriteLine($"{p.Name,-10} {p.ArrivalTime,-10} {p.ExecutionTime,-8} {p.FinishTime,-10} {p.TurnaroundTime,-14} {p.NormalisedTurnaround:F2}");
        }
 
        // Compute averages to allow comparison with other scheduling algorithms
        double avgTurnaround = 0;
        double avgNorm = 0;
        foreach (var p in processes)
        {
            avgTurnaround += p.TurnaroundTime;
            avgNorm += p.NormalisedTurnaround;
        }
 
        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"Averages",-10} {"",-10} {"",-8} {"",-10} {avgTurnaround / processes.Count,-14:F2} {avgNorm / processes.Count:F2}");
    }
 
    // Renders a simple text-based Gantt chart.
    // Each process block is scaled by its execution time, with finish times on the axis below.
    static void PrintGanttChart(List<Process> processes)
    {
        Console.WriteLine("\nGantt Chart:");
        Console.Write("|");
 
        // Top row: process name, padded to be proportional to its execution time
        foreach (var p in processes)
            Console.Write($" {p.Name.PadRight(p.ExecutionTime * 2 - 1)}|");
 
        Console.WriteLine();
        Console.Write("0");
 
        // Bottom row: finish-time markers aligned to each process block's right edge
        foreach (var p in processes)
            Console.Write($"{p.FinishTime.ToString().PadLeft(p.ExecutionTime * 2 + 1)}");
 
        Console.WriteLine();
    }
}
 