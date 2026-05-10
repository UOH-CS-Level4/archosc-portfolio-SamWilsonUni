using System;
using System.Collections.Generic;
using System.Linq;
 
// Represents a single process with scheduling attributes
class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }      // Time the process enters the system
    public int ExecutionTime { get; set; }    // Total CPU time required
    public int Priority { get; set; }         // Higher value = higher priority
    public int RemainingTime { get; set; }    // CPU time still needed (decreases as it runs)
    public int FinishTime { get; set; }       // Time the process completes
    public int TurnaroundTime { get; set; }   // FinishTime - ArrivalTime
    public double NormalisedTurnaround { get; set; } // TurnaroundTime / ExecutionTime
}
 
class ContextSwitching
{
    static void Main(string[] args)
    {
        // Run the simulation with two different time quanta to compare behaviour
        int[] timeQuanta = { 1, 6 };
 
        foreach (int tq in timeQuanta)
        {
            // Define the process set (same for both runs)
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
 
            // Initialise remaining time equal to execution time before each run
            foreach (var p in processes)
                p.RemainingTime = p.ExecutionTime;
 
            Console.WriteLine($"\n{'=',1} Priority Round Robin (Time Quantum = {tq}) {'=',1}");
            RunPriorityRoundRobin(processes, tq);
        }
    }
 
    // Simulates Priority Round Robin scheduling.
    // At each scheduling decision, the highest-priority process in the ready queue
    // is selected and runs for up to timeQuantum units before being preempted.
    static void RunPriorityRoundRobin(List<Process> processes, int timeQuantum)
    {
        int currentTime = 0;
        var ganttChart = new List<(string Name, int Start, int End)>(); // Execution timeline
        var completed = new List<Process>();
        var arrived = new HashSet<string>(); // Tracks which processes have joined the ready queue
 
        // Clone the process list so the originals are preserved for result display
        var remaining = processes.Select(p => new Process
        {
            Name = p.Name,
            ArrivalTime = p.ArrivalTime,
            ExecutionTime = p.ExecutionTime,
            Priority = p.Priority,
            RemainingTime = p.ExecutionTime
        }).ToList();
 
        // Ready queue holds processes that have arrived and are waiting for CPU time
        var readyQueue = new List<Process>();
 
        // Enqueue any processes that are already available at time 0
        foreach (var p in remaining.Where(p => p.ArrivalTime <= currentTime))
        {
            readyQueue.Add(p);
            arrived.Add(p.Name);
        }
 
        // Continue until every process has finished
        while (completed.Count < remaining.Count)
        {
            if (readyQueue.Count == 0)
            {
                // No process is ready — advance the clock to the next arrival
                currentTime = remaining
                    .Where(p => !arrived.Contains(p.Name))
                    .Min(p => p.ArrivalTime);
 
                // Admit all processes that have now arrived
                foreach (var p in remaining.Where(p =>
                    p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
                {
                    readyQueue.Add(p);
                    arrived.Add(p.Name);
                }
                continue;
            }
 
            // Select the highest-priority process from the ready queue (preemptive by priority)
            var current = readyQueue.OrderByDescending(p => p.Priority).First();
            readyQueue.Remove(current);
 
            int startTime = currentTime;
 
            // Run for the time quantum, or finish early if less time remains
            int runTime = Math.Min(timeQuantum, current.RemainingTime);
            currentTime += runTime;
            current.RemainingTime -= runTime;
 
            // Check for new arrivals during this execution slice
            foreach (var p in remaining.Where(p =>
                p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
            {
                readyQueue.Add(p);
                arrived.Add(p.Name);
            }
 
            // Record this execution slice in the Gantt chart
            ganttChart.Add((current.Name, startTime, currentTime));
 
            if (current.RemainingTime == 0)
            {
                // Process is done — compute its completion metrics
                current.FinishTime = currentTime;
                current.TurnaroundTime = current.FinishTime - current.ArrivalTime;
                current.NormalisedTurnaround =
                    (double)current.TurnaroundTime / current.ExecutionTime;
                completed.Add(current);
            }
            else
            {
                // Process still has work to do — put it back in the ready queue
                readyQueue.Add(current);
            }
        }
 
        PrintResults(completed, processes);
        PrintGanttChart(ganttChart);
    }
 
    // Prints a table of per-process results along with average turnaround metrics
    static void PrintResults(List<Process> completed, List<Process> original)
    {
        // Restore the original submission order for consistent display
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
 
    // Renders a text-based Gantt chart showing when each process ran
    static void PrintGanttChart(List<(string Name, int Start, int End)> ganttChart)
    {
        Console.WriteLine("\nGantt Chart:");
 
        int blockWidth = 4; // Character width per unit of time
 
        // Top border — each segment's width is proportional to its duration
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();
 
        // Process name row
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
 
        // Build the time axis as a character array so labels land at exact positions
        int totalWidth = ganttChart.Last().End * blockWidth + 5;
        char[] axis = new string(' ', totalWidth).ToCharArray();
 
        // Place the start-time label at position 0
        string firstLabel = ganttChart[0].Start.ToString();
        for (int i = 0; i < firstLabel.Length; i++)
            axis[i] = firstLabel[i];
 
        // Place each segment's end-time label at the correct character offset
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