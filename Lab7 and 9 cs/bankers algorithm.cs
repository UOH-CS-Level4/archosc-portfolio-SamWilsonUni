using System;

class BankersAlgorithm
{
    // Number of processes and resource types
    static int numProcesses;
    static int numResources;

    // Matrices and arrays for the algorithm
    static int[,] allocation;    // Currently allocated resources
    static int[,] maximum;       // Maximum demand for each process
    static int[,] need;          // Remaining need (maximum - allocation)
    static int[] available;      // Currently available resources

    static void Main(string[] args)
    {
        Console.WriteLine("=== Banker's Algorithm Simulator ===\n");

        // Get number of processes from user
        Console.Write("Enter number of processes: ");
        numProcesses = ValidateInput();

        // Get number of resource types from user
        Console.Write("Enter number of resource types: ");
        numResources = ValidateInput();

        // Initialise matrices
        allocation = new int[numProcesses, numResources];
        maximum = new int[numProcesses, numResources];
        need = new int[numProcesses, numResources];
        available = new int[numResources];

        // Get allocation matrix from user
        Console.WriteLine("\nEnter allocation matrix:");
        for (int i = 0; i < numProcesses; i++)
        {
            Console.Write($"P{i + 1}: ");
            string[] inputs = Console.ReadLine().Trim().Split(' ');

            // Validate correct number of values entered
            while (inputs.Length != numResources)
            {
                Console.Write($"Invalid input. Please enter {numResources} values for P{i + 1}: ");
                inputs = Console.ReadLine().Trim().Split(' ');
            }

            for (int j = 0; j < numResources; j++)
            {
                allocation[i, j] = int.Parse(inputs[j]);
            }
        }

        // Get maximum demand matrix from user
        Console.WriteLine("\nEnter maximum demand matrix:");
        for (int i = 0; i < numProcesses; i++)
        {
            Console.Write($"P{i + 1}: ");
            string[] inputs = Console.ReadLine().Trim().Split(' ');

            // Validate correct number of values entered
            while (inputs.Length != numResources)
            {
                Console.Write($"Invalid input. Please enter {numResources} values for P{i + 1}: ");
                inputs = Console.ReadLine().Trim().Split(' ');
            }

            for (int j = 0; j < numResources; j++)
            {
                maximum[i, j] = int.Parse(inputs[j]);
            }
        }

        // Get available resources from user
        Console.Write("\nEnter available resources: ");
        string[] availInputs = Console.ReadLine().Trim().Split(' ');

        // Validate correct number of values entered
        while (availInputs.Length != numResources)
        {
            Console.Write($"Invalid input. Please enter {numResources} values: ");
            availInputs = Console.ReadLine().Trim().Split(' ');
        }

        for (int i = 0; i < numResources; i++)
        {
            available[i] = int.Parse(availInputs[i]);
        }

        // Calculate the need matrix (maximum - allocation)
        CalculateNeed();

        // Run the safety algorithm
        RunSafetyAlgorithm();
    }

    // Calculates the need matrix for each process
    static void CalculateNeed()
    {
        for (int i = 0; i < numProcesses; i++)
        {
            for (int j = 0; j < numResources; j++)
            {
                need[i, j] = maximum[i, j] - allocation[i, j];
            }
        }
    }

    // Validates that user input is a positive integer
    static int ValidateInput()
    {
        int value;
        while (!int.TryParse(Console.ReadLine(), out value) || value <= 0)
        {
            Console.Write("Invalid input. Please enter a positive integer: ");
        }
        return value;
    }

    // Checks if the need of a process can be satisfied by available resources
    static bool CanProcessRun(int processIndex, int[] work)
    {
        for (int j = 0; j < numResources; j++)
        {
            // If any resource need exceeds available, process cannot run
            if (need[processIndex, j] > work[j])
            {
                return false;
            }
        }
        return true;
    }

    // Runs the Banker's safety algorithm to find a safe sequence
    static void RunSafetyAlgorithm()
    {
        // Work array represents currently available resources
        int[] work = (int[])available.Clone();

        // Finish array tracks which processes have completed
        bool[] finish = new bool[numProcesses];

        // Safe sequence stores the order processes can safely execute
        int[] safeSequence = new int[numProcesses];
        int count = 0;

        Console.WriteLine("\n=== Running Safety Algorithm ===");

        // Keep looping until all processes are finished or no progress can be made
        while (count < numProcesses)
        {
            bool progressMade = false;

            for (int i = 0; i < numProcesses; i++)
            {
                // Only consider processes that haven't finished yet
                if (!finish[i] && CanProcessRun(i, work))
                {
                    // Process can run - release its allocation back to work
                    Console.Write($"\nP{i + 1} can run. Releasing allocation: (");
                    for (int j = 0; j < numResources; j++)
                    {
                        work[j] += allocation[i, j];
                        Console.Write(j < numResources - 1 ? $"{allocation[i, j]}," : $"{allocation[i, j]})");
                    }

                    // Print new available resources
                    Console.Write(" | New Available: (");
                    for (int j = 0; j < numResources; j++)
                    {
                        Console.Write(j < numResources - 1 ? $"{work[j]}," : $"{work[j]})");
                    }

                    // Mark process as finished and add to safe sequence
                    finish[i] = true;
                    safeSequence[count++] = i;
                    progressMade = true;
                }
            }

            // If no progress was made in this pass, system is unsafe
            if (!progressMade)
            {
                Console.WriteLine("\n\nSystem is in an UNSAFE state - deadlock possible!");
                return;
            }
        }

        // All processes completed - print safe sequence
        Console.Write("\n\nSafe Sequence: ");
        for (int i = 0; i < numProcesses; i++)
        {
            Console.Write(i < numProcesses - 1 
                ? $"P{safeSequence[i] + 1} -> " 
                : $"P{safeSequence[i] + 1}");
        }
        Console.WriteLine("\nSystem is in a SAFE state.");
    }
}