using System;
using System.Collections.Generic;
using System.Linq;

class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }
    public int ExecutionTime { get; set; }
    public int RemainingTime { get; set; }
    public int FinishTime { get; set; }
    public int TurnaroundTime { get; set; }
    public double NormalisedTurnaround { get; set; }
}

class RoundRobin
{
    static void Main(string[] args)
    {
        int[] timeQuanta = { 1, 3, 4, 6 };

        foreach (int tq in timeQuanta)
        {
            // Reset processes for each time quantum
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

            // Set remaining time for each process
            foreach (var p in processes)
                p.RemainingTime = p.ExecutionTime;

            Console.WriteLine($"\n{'=',1} Round Robin (Time Quantum = {tq}) {'=',1}");
            RunRoundRobin(processes, tq);
        }
    }

    static void RunRoundRobin(List<Process> processes, int timeQuantum)
    {
        int currentTime = 0;
        var readyQueue = new Queue<Process>();
        var ganttChart = new List<(string Name, int Start, int End)>();
        var remaining = processes.Select(p => new Process
        {
            Name = p.Name,
            ArrivalTime = p.ArrivalTime,
            ExecutionTime = p.ExecutionTime,
            RemainingTime = p.ExecutionTime
        }).ToList();

        var completed = new List<Process>();
        var arrived = new HashSet<string>();

        // Add processes that arrive at time 0
        foreach (var p in remaining.Where(p => p.ArrivalTime <= currentTime))
        {
            readyQueue.Enqueue(p);
            arrived.Add(p.Name);
        }

        while (completed.Count < remaining.Count)
        {
            if (readyQueue.Count == 0)
            {
                // CPU is idle, jump to next arrival
                currentTime = remaining
                    .Where(p => !arrived.Contains(p.Name))
                    .Min(p => p.ArrivalTime);

                foreach (var p in remaining.Where(p => 
                    p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
                {
                    readyQueue.Enqueue(p);
                    arrived.Add(p.Name);
                }
                continue;
            }

            // Get next process from queue
            var current = readyQueue.Dequeue();
            int startTime = currentTime;

            // Run for time quantum or remaining time, whichever is smaller
            int runTime = Math.Min(timeQuantum, current.RemainingTime);
            currentTime += runTime;
            current.RemainingTime -= runTime;

            // Add newly arrived processes to ready queue
            foreach (var p in remaining.Where(p => 
                p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
            {
                readyQueue.Enqueue(p);
                arrived.Add(p.Name);
            }

            ganttChart.Add((current.Name, startTime, currentTime));

            if (current.RemainingTime == 0)
            {
                // Process finished
                current.FinishTime = currentTime;
                current.TurnaroundTime = current.FinishTime - current.ArrivalTime;
                current.NormalisedTurnaround = 
                    (double)current.TurnaroundTime / current.ExecutionTime;
                completed.Add(current);
            }
            else
            {
                // Process not finished, re-add to queue
                readyQueue.Enqueue(current);
            }
        }

        PrintResults(completed, processes);
        PrintGanttChart(ganttChart);
    }

    static void PrintResults(List<Process> completed, List<Process> original)
    {
        // Sort by original process order
        var ordered = completed.OrderBy(p => 
            original.FindIndex(o => o.Name == p.Name)).ToList();

        Console.WriteLine($"\n{"Process",-10} {"Arrival",-10} {"Exec",-8} {"Finish",-10} {"Turnaround",-14} {"Norm. Turnaround"}");
        Console.WriteLine(new string('-', 65));

        double avgTa = 0, avgNorm = 0;
        foreach (var p in ordered)
        {
            Console.WriteLine($"{p.Name,-10} {p.ArrivalTime,-10} {p.ExecutionTime,-8} {p.FinishTime,-10} {p.TurnaroundTime,-14} {p.NormalisedTurnaround:F2}");
            avgTa += p.TurnaroundTime;
            avgNorm += p.NormalisedTurnaround;
        }

        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"Averages",-10} {"",-10} {"",-8} {"",-10} {avgTa / ordered.Count,-14:F2} {avgNorm / ordered.Count:F2}");
    }

    static void PrintGanttChart(List<(string Name, int Start, int End)> ganttChart)
    {
        Console.WriteLine("\nGantt Chart:");
        
        int blockWidth = 4; // width per time unit

        // Top border
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();

        // Process names row
        Console.Write("|");
        foreach (var seg in ganttChart)
        {
            int width = (seg.End - seg.Start) * blockWidth - 1;
            string label = seg.Name.PadRight(width);
            Console.Write(label + "|");
        }
        Console.WriteLine();

        // Bottom border
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();

        // Build time axis as a fixed-width character array
        int totalWidth = ganttChart.Last().End * blockWidth + 5;
        char[] axis = new string(' ', totalWidth).ToCharArray();

        // Place each time marker at exact character position
        int pos = 0;
        // Mark start of first segment
        string firstLabel = ganttChart[0].Start.ToString();
        foreach (char c in firstLabel)
            axis[pos++] = c;

        // Mark end of each segment
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