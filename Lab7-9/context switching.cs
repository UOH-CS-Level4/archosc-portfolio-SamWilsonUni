using System;
using System.Collections.Generic;
using System.Linq;

class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }
    public int ExecutionTime { get; set; }
    public int Priority { get; set; }
    public int RemainingTime { get; set; }
    public int FinishTime { get; set; }
    public int TurnaroundTime { get; set; }
    public double NormalisedTurnaround { get; set; }
}

class ContextSwitching
{
    static void Main(string[] args)
    {
        int[] timeQuanta = { 1, 6 };

        foreach (int tq in timeQuanta)
        {
            // Table 1 processes with priority
            var processes = new List<Process>
            {
                new Process { Name = "A", ArrivalTime = 0, ExecutionTime = 3, Priority = 5 },
                new Process { Name = "B", ArrivalTime = 2, ExecutionTime = 6, Priority = 4 },
                new Process { Name = "C", ArrivalTime = 5, ExecutionTime = 5, Priority = 8 },
                new Process { Name = "D", ArrivalTime = 6, ExecutionTime = 3, Priority = 6 },
                new Process { Name = "E", ArrivalTime = 8, ExecutionTime = 6, Priority = 10 },
                new Process { Name = "F", ArrivalTime = 9, ExecutionTime = 2, Priority = 3 },
                new Process { Name = "G", ArrivalTime = 10, ExecutionTime = 6, Priority = 7 }
            };

            foreach (var p in processes)
                p.RemainingTime = p.ExecutionTime;

            Console.WriteLine($"\n{'=',1} Priority Round Robin (Time Quantum = {tq}) {'=',1}");
            RunPriorityRoundRobin(processes, tq);
        }
    }

    static void RunPriorityRoundRobin(List<Process> processes, int timeQuantum)
    {
        int currentTime = 0;
        var ganttChart = new List<(string Name, int Start, int End)>();
        var completed = new List<Process>();
        var arrived = new HashSet<string>();

        var remaining = processes.Select(p => new Process
        {
            Name = p.Name,
            ArrivalTime = p.ArrivalTime,
            ExecutionTime = p.ExecutionTime,
            Priority = p.Priority,
            RemainingTime = p.ExecutionTime
        }).ToList();

        // Ready queue - sorted by priority (highest first)
        var readyQueue = new List<Process>();

        // Add processes arriving at time 0
        foreach (var p in remaining.Where(p => p.ArrivalTime <= currentTime))
        {
            readyQueue.Add(p);
            arrived.Add(p.Name);
        }

        while (completed.Count < remaining.Count)
        {
            if (readyQueue.Count == 0)
            {
                // CPU idle, jump to next arrival
                currentTime = remaining
                    .Where(p => !arrived.Contains(p.Name))
                    .Min(p => p.ArrivalTime);

                foreach (var p in remaining.Where(p =>
                    p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
                {
                    readyQueue.Add(p);
                    arrived.Add(p.Name);
                }
                continue;
            }

            // Always pick highest priority process from ready queue
            var current = readyQueue.OrderByDescending(p => p.Priority).First();
            readyQueue.Remove(current);

            int startTime = currentTime;
            int runTime = Math.Min(timeQuantum, current.RemainingTime);
            currentTime += runTime;
            current.RemainingTime -= runTime;

            // Add newly arrived processes
            foreach (var p in remaining.Where(p =>
                p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
            {
                readyQueue.Add(p);
                arrived.Add(p.Name);
            }

            ganttChart.Add((current.Name, startTime, currentTime));

            if (current.RemainingTime == 0)
            {
                current.FinishTime = currentTime;
                current.TurnaroundTime = current.FinishTime - current.ArrivalTime;
                current.NormalisedTurnaround =
                    (double)current.TurnaroundTime / current.ExecutionTime;
                completed.Add(current);
            }
            else
            {
                // Re-add to ready queue for next round
                readyQueue.Add(current);
            }
        }

        PrintResults(completed, processes);
        PrintGanttChart(ganttChart);
    }

    static void PrintResults(List<Process> completed, List<Process> original)
    {
        var ordered = completed.OrderBy(p =>
            original.FindIndex(o => o.Name == p.Name)).ToList();

        Console.WriteLine($"\n{"Process",-10} {"Arrival",-10} {"Exec",-8} {"Priority",-10} {"Finish",-10} {"Turnaround",-14} {"Norm. Turnaround"}");
        Console.WriteLine(new string('-', 75));

        double avgTa = 0, avgNorm = 0;
        foreach (var p in ordered)
        {
            Console.WriteLine($"{p.Name,-10} {p.ArrivalTime,-10} {p.ExecutionTime,-8} {p.Priority,-10} {p.FinishTime,-10} {p.TurnaroundTime,-14} {p.NormalisedTurnaround:F2}");
            avgTa += p.TurnaroundTime;
            avgNorm += p.NormalisedTurnaround;
        }

        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"{"Averages",-10} {"",-10} {"",-8} {"",-10} {"",-10} {avgTa / ordered.Count,-14:F2} {avgNorm / ordered.Count:F2}");
    }

    static void PrintGanttChart(List<(string Name, int Start, int End)> ganttChart)
    {
        Console.WriteLine("\nGantt Chart:");

        int blockWidth = 4;

        // Top border
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();

        // Process names
        Console.Write("|");
        foreach (var seg in ganttChart)
        {
            int width = (seg.End - seg.Start) * blockWidth - 1;
            Console.Write(seg.Name.PadRight(width) + "|");
        }
        Console.WriteLine();

        // Bottom border
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();

        // Time axis as character array
        int totalWidth = ganttChart.Last().End * blockWidth + 5;
        char[] axis = new string(' ', totalWidth).ToCharArray();

        string firstLabel = ganttChart[0].Start.ToString();
        for (int i = 0; i < firstLabel.Length; i++)
            axis[i] = firstLabel[i];

        foreach (var seg in ganttChart)
        {
            int charPos = seg.End * blockWidth;
            string label = seg.End.ToString();
            for (int i = 0; i < label.Length; i++)
                if (charPos + i < axis.Length)
                    axis[charPos + i] = label[i];
        }

        Console.WriteLine(new string(axis));
    }
}