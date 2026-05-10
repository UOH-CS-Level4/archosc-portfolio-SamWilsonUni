using System;
using System.Collections.Generic;
 
// Represents a single entry in the Translation Lookaside Buffer
class TLBEntry
{
    public int VPN; // Virtual Page Number — the key used for lookups
    public int PPN; // Physical Page Number — the translated address stored here
}
 
// Simulates a hardware TLB with a fixed capacity and FIFO eviction policy.
// The TLB caches recent VPN→PPN translations to avoid expensive page table walks.
class TLB
{
    private List<TLBEntry> entries = new List<TLBEntry>();
    private const int capacity = 4; // Maximum number of entries the TLB can hold
    private int hits = 0;           // Count of successful TLB lookups
    private int misses = 0;         // Count of failed TLB lookups
 
    // Searches the TLB for a given VPN.
    // Returns the corresponding PPN on a hit, or null on a miss.
    public int? Lookup(int vpn)
    {
        foreach (var entry in entries)
        {
            if (entry.VPN == vpn)
            {
                hits++;
                Console.WriteLine($"TLB HIT  - VPN {vpn} → PPN {entry.PPN}");
                return entry.PPN;
            }
        }
 
        // VPN not found — the caller must consult the page table
        misses++;
        Console.WriteLine($"TLB MISS - VPN {vpn} not found in TLB");
        return null;
    }
 
    // Adds a new VPN→PPN mapping to the TLB.
    // If the TLB is full, the oldest entry is evicted (FIFO replacement policy).
    public void Insert(int vpn, int ppn)
    {
        if (entries.Count >= capacity)
        {
            // Evict the entry at index 0 (the oldest / first-in)
            Console.WriteLine($"TLB FULL - Evicting VPN {entries[0].VPN}");
            entries.RemoveAt(0);
        }
        entries.Add(new TLBEntry { VPN = vpn, PPN = ppn });
    }
 
    // Prints a summary of TLB performance for the simulation run
    public void PrintHitRatio()
    {
        int total = hits + misses;
        double ratio = (double)hits / total * 100;
        Console.WriteLine($"\n--- TLB Statistics ---");
        Console.WriteLine($"Total lookups : {total}");
        Console.WriteLine($"Hits          : {hits}");
        Console.WriteLine($"Misses        : {misses}");
        Console.WriteLine($"Hit Ratio     : {ratio:F1}%");
    }
}
 
// Simulates a simple page table that maps virtual page numbers to physical page numbers.
// In a real system this would be stored in main memory and maintained by the OS.
class PageTable
{
    // Static mapping of VPN → PPN for this simulation
    private Dictionary<int, int> table = new Dictionary<int, int>()
    {
        { 0, 7 }, { 1, 9 }, { 2, 0 }, { 3, 5 },
        { 4, 5 }, { 5, 3 }, { 6, 2 }, { 7, 4 }
    };
 
    // Looks up a VPN in the page table.
    // Returns the PPN if the page is in memory, or null to signal a page fault.
    public int? Lookup(int vpn)
    {
        if (table.ContainsKey(vpn)) return table[vpn];
        return null; // Page fault — the page is not currently in physical memory
    }
}
 
class Program
{
    static void Main()
    {
        TLB tlb = new TLB();
        PageTable pageTable = new PageTable();
 
        // The sequence of virtual page numbers that the CPU requests.
        // Repeated accesses (e.g. VPN 1, 6, 3) should produce TLB hits after
        // the first miss, demonstrating temporal locality.
        int[] vpnSequence = { 1, 6, 3, 1, 6, 2, 0, 1, 6, 3 };
 
        Console.WriteLine("=== TLB Simulation ===\n");
 
        foreach (int vpn in vpnSequence)
        {
            Console.Write($"Looking up VPN {vpn}: ");
 
            // Step 1: Check the TLB first (fast path)
            int? ppn = tlb.Lookup(vpn);
 
            if (ppn == null) // TLB miss — fall back to the page table (slow path)
            {
                ppn = pageTable.Lookup(vpn);
 
                if (ppn != null)
                {
                    // Load the translation into the TLB so future accesses hit
                    tlb.Insert(vpn, ppn.Value);
                    Console.WriteLine($"  → Loaded from page table: PPN {ppn}");
                }
                else
                {
                    // The page is not in memory at all — a page fault would occur here
                    Console.WriteLine($"  → PAGE FAULT! VPN {vpn} not in memory");
                }
            }
        }
 
        // Display overall hit/miss statistics for the simulation
        tlb.PrintHitRatio();
    }
}
