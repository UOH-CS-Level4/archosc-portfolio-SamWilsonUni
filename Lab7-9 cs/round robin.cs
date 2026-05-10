using System;
using System.Collections.Generic;
using System.Linq;
 
// Represents a process being managed by the round-robin scheduler
class Process
{
    public string Name { get; set; }
    public int ArrivalTime { get; set; }             // When the process enters the ready queue
    public int ExecutionTime { get; set; }           // Total CPU time needed
    public int RemainingTime { get; set; }           // CPU time still left to run
    public int FinishTime { get; set; }              // Clock time when the process completes
    public int TurnaroundTime { get; set; }          // FinishTime - ArrivalTime
    public double NormalisedTurnaround { get; set; } // TurnaroundTime / ExecutionTime
}
 
class RoundRobin
{
    static void Main(string[] args)
    {
        // Test the scheduler across four different time quanta to observe the trade-offs:
        // small quanta → more context switches, fairer; large quanta → approaches FCFS
        int[] timeQuanta = { 1, 3, 4, 6 };
 
        foreach (int tq in timeQuanta)
        {
            // Re-create the process list fresh for each time quantum run
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
 
            // Set each process's remaining time to its full execution time
            foreach (var p in processes)
                p.RemainingTime = p.ExecutionTime;
 
            Console.WriteLine($"\n{'=',1} Round Robin (Time Quantum = {tq}) {'=',1}");
            RunRoundRobin(processes, tq);
        }
    }
 
    // Simulates Round Robin scheduling with the given time quantum.
    // Each process runs for at most timeQuantum units before being preempted
    // and cycled to the back of the ready queue.
    static void RunRoundRobin(List<Process> processes, int timeQuantum)
    {
        int currentTime = 0;
        var readyQueue = new Queue<Process>();  // FIFO queue of processes waiting for CPU
        var ganttChart = new List<(string Name, int Start, int End)>(); // Execution timeline
 
        // Clone the process list so original data is preserved for result display
        var remaining = processes.Select(p => new Process
        {
            Name = p.Name,
            ArrivalTime = p.ArrivalTime,
            ExecutionTime = p.ExecutionTime,
            RemainingTime = p.ExecutionTime
        }).ToList();
 
        var completed = new List<Process>();
        var arrived = new HashSet<string>(); // Prevents a process from being enqueued twice
 
        // Enqueue any processes ready at time 0
        foreach (var p in remaining.Where(p => p.ArrivalTime <= currentTime))
        {
            readyQueue.Enqueue(p);
            arrived.Add(p.Name);
        }
 
        // Keep scheduling until every process has finished
        while (completed.Count < remaining.Count)
        {
            if (readyQueue.Count == 0)
            {
                // CPU is idle — jump the clock forward to the next process arrival
                currentTime = remaining
                    .Where(p => !arrived.Contains(p.Name))
                    .Min(p => p.ArrivalTime);
 
                // Admit all processes that have arrived by the new time
                foreach (var p in remaining.Where(p =>
                    p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
                {
                    readyQueue.Enqueue(p);
                    arrived.Add(p.Name);
                }
                continue;
            }
 
            // Dequeue the next process in FIFO order
            var current = readyQueue.Dequeue();
            int startTime = currentTime;
 
            // Run for the time quantum, or less if the process finishes sooner
            int runTime = Math.Min(timeQuantum, current.RemainingTime);
            currentTime += runTime;
            current.RemainingTime -= runTime;
 
            // Enqueue any processes that arrived during this execution slice
            foreach (var p in remaining.Where(p =>
                p.ArrivalTime <= currentTime && !arrived.Contains(p.Name)))
            {
                readyQueue.Enqueue(p);
                arrived.Add(p.Name);
            }
 
            // Record this slice in the Gantt chart
            ganttChart.Add((current.Name, startTime, currentTime));
 
            if (current.RemainingTime == 0)
            {
                // Process has completed — record its metrics
                current.FinishTime = currentTime;
                current.TurnaroundTime = current.FinishTime - current.ArrivalTime;
                current.NormalisedTurnaround =
                    (double)current.TurnaroundTime / current.ExecutionTime;
                completed.Add(current);
            }
            else
            {
                // Process is not finished — return it to the back of the queue
                readyQueue.Enqueue(current);
            }
        }
 
        PrintResults(completed, processes);
        PrintGanttChart(ganttChart);
    }
 
    // Prints a results table ordered by original process submission order,
    // followed by average turnaround and normalised turnaround values
    static void PrintResults(List<Process> completed, List<Process> original)
    {
        // Sort completed processes back into their original submission order
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
 
    // Renders a text-based Gantt chart showing the execution timeline
    static void PrintGanttChart(List<(string Name, int Start, int End)> ganttChart)
    {
        Console.WriteLine("\nGantt Chart:");
 
        int blockWidth = 4; // Character width per unit of time
 
        // Top border — segment width is proportional to execution duration
        Console.Write("+");
        foreach (var seg in ganttChart)
            Console.Write(new string('-', (seg.End - seg.Start) * blockWidth - 1) + "+");
        Console.WriteLine();
 
        // Process name row
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
 
        // Build the time axis using a fixed-width character array so time labels
        // appear at their exact character positions rather than being formatted inline
        int totalWidth = ganttChart.Last().End * blockWidth + 5;
        char[] axis = new string(' ', totalWidth).ToCharArray();
 
        // Place the start label (time 0) at the leftmost position
        int pos = 0;
        string firstLabel = ganttChart[0].Start.ToString();
        foreach (char c in firstLabel)
            axis[pos++] = c;
 
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
 