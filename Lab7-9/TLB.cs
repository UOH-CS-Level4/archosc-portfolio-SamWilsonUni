using System;
using System.Collections.Generic;

class TLBEntry 
{ 
    public int VPN; 
    public int PPN; 
}

class TLB 
{ 
    private List<TLBEntry> entries = new List<TLBEntry>(); 
    private const int capacity = 4;
    private int hits = 0;
    private int misses = 0;

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
        misses++;
        Console.WriteLine($"TLB MISS - VPN {vpn} not found in TLB");
        return null;
    }

    public void Insert(int vpn, int ppn) 
    { 
        if (entries.Count >= capacity)
        {
            Console.WriteLine($"TLB FULL - Evicting VPN {entries[0].VPN}");
            entries.RemoveAt(0); // Remove oldest entry (FIFO)
        }
        entries.Add(new TLBEntry { VPN = vpn, PPN = ppn }); 
    }

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

// Simulated page table (VPN → PPN)
class PageTable
{
    private Dictionary<int, int> table = new Dictionary<int, int>()
    {
        { 0, 7 }, { 1, 9 }, { 2, 0 }, { 3, 5 },
        { 4, 5 }, { 5, 3 }, { 6, 2 }, { 7, 4 }
    };

    public int? Lookup(int vpn)
    {
        if (table.ContainsKey(vpn)) return table[vpn];
        return null; // Page fault
    }
}

class Program
{
    static void Main()
    {
        TLB tlb = new TLB();
        PageTable pageTable = new PageTable();

        // Sequence of virtual page number lookups to simulate
        int[] vpnSequence = { 1, 6, 3, 1, 6, 2, 0, 1, 6, 3 };

        Console.WriteLine("=== TLB Simulation ===\n");

        foreach (int vpn in vpnSequence)
        {
            Console.Write($"Looking up VPN {vpn}: ");
            int? ppn = tlb.Lookup(vpn);

            if (ppn == null) // TLB miss - go to page table
            {
                ppn = pageTable.Lookup(vpn);
                if (ppn != null)
                {
                    tlb.Insert(vpn, ppn.Value);
                    Console.WriteLine($"  → Loaded from page table: PPN {ppn}");
                }
                else
                {
                    Console.WriteLine($"  → PAGE FAULT! VPN {vpn} not in memory");
                }
            }
        }

        tlb.PrintHitRatio();
    }
}