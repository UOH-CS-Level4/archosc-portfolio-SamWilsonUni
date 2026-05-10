using System;
using System.Collections.Generic;

class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }
    public int ExecutionTime { get; set; }
    public int FinishTime { get; set; }
    public int TurnaroundTime { get; set; }
    public double NormalisedTurnaround { get; set; }
}

class FCFS
{
    static void Main(string[] args)
    {
        // Table 2: FCFS process table
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

    static void RunFCFS(List<Process> processes)
    {
        int currentTime = 0;

        foreach (var process in processes)
        {
            // If CPU is idle (process hasn't arrived yet), skip forward to its arrival
            if (currentTime < process.ArrivalTime)
                currentTime = process.ArrivalTime;

            // Process runs to completion (non-preemptive)
            process.FinishTime = currentTime + process.ExecutionTime;
            process.TurnaroundTime = process.FinishTime - process.ArrivalTime;
            process.NormalisedTurnaround = (double)process.TurnaroundTime / process.ExecutionTime;

            // Advance the clock
            currentTime = process.FinishTime;
        }
    }

    static void PrintResults(List<Process> processes)
    {
        Console.WriteLine("FCFS Scheduling Results:");
        Console.WriteLine($"{"Process",-10} {"Arrival",-10} {"Exec",-8} {"Finish",-10} {"Turnaround",-14} {"Norm. Turnaround"}");
        Console.WriteLine(new string('-', 65));

        foreach (var p in processes)
        {
            Console.WriteLine($"{p.Name,-10} {p.ArrivalTime,-10} {p.ExecutionTime,-8} {p.FinishTime,-10} {p.TurnaroundTime,-14} {p.NormalisedTurnaround:F2}");
        }

        // Averages help compare
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

    static void PrintGanttChart(List<Process> processes)
    {
        Console.WriteLine("\nGantt Chart:");
        Console.Write("|");

        // Top row: process names
        foreach (var p in processes)
            Console.Write($" {p.Name.PadRight(p.ExecutionTime * 2 - 1)}|");

        Console.WriteLine();
        Console.Write("0");

        // Bottom row: time markers
        foreach (var p in processes)
            Console.Write($"{p.FinishTime.ToString().PadLeft(p.ExecutionTime * 2 + 1)}");

        Console.WriteLine();
    }
}