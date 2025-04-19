using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using GTCMemory;

namespace AimbotTest
{
    class Program
    {
    
        
        public static GTCMemory.GTCMemory MemLib = new GTCMemory.GTCMemory();
        private static Dictionary<long, byte[]> originalValues = new Dictionary<long, byte[]>();
        private static Dictionary<long, int> originalValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> originalValuesWrite2 = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite2 = new Dictionary<long, int>();
        
        // Cache for addresses to avoid repeated scanning
        private static List<long> cachedAddresses = null;
        private static bool aimActive = false;
        
        // Async method to find addresses only once and cache them
        private async static Task<List<long>> FindAddresses()
        {
            if (cachedAddresses != null && cachedAddresses.Count > 0)
            {
                return cachedAddresses;
            }
            
            Int32 proc = GetProcIdFromName("HD-Player");
            if (proc <= 0)
            {
                Console.WriteLine("HD-Player process not found!");
                return new List<long>();
            }
            
            MemLib.OpenProcess(proc);
            
            // The AOB pattern to search for
            string pattern = "FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
            
            // Use optimized scan function that caches results
            IEnumerable<long> addresses = await MemLib.OptimizedAoBScan(pattern, true);
            
            if (addresses == null || !addresses.Any())
            {
                Console.WriteLine("No addresses found!");
                return new List<long>();
            }
            
            cachedAddresses = addresses.ToList();
            Console.WriteLine($"Found {cachedAddresses.Count} addresses");
            return cachedAddresses;
        }
        
        public async static void Aimbot()
        {
            try
            {
                var addresses = await FindAddresses();
                if (addresses.Count == 0) return;

                foreach (long num in addresses)
                {
                    string str = num.ToString("X");
                    
                    // Use fast memory reads and writes
                    int originalValue = MemLib.FastReadInt((num + 0x9E).ToString("X"));
                    int valueToWrite = MemLib.FastReadInt((num + 0xA2).ToString("X"));
                    
                    MemLib.FastWriteInt((num + 0x9E).ToString("X"), valueToWrite);
                }
                
                Console.WriteLine("Aimbot Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aimbot error: {ex.Message}");
            }
        }
        
        // Ultra-fast AimNeck with direct memory manipulation
        public async static void AimNeckOptimized()
        {
            try
            {
                // Toggle the state
                aimActive = !aimActive;
                
                // Get cached addresses or find them if not cached
                var addresses = await FindAddresses();
                if (addresses.Count == 0) return;
                
                // Only show sound feedback if toggling from off to on
                if (aimActive)
                {
                    // Minimize console output - it causes slowdowns
                    Console.WriteLine("AimNeck activated");
                    
                    // Prepare all UIntPtr addresses in advance to avoid string conversions during loop
                    Dictionary<long, UIntPtr> sourceAddresses = new Dictionary<long, UIntPtr>();
                    Dictionary<long, UIntPtr> targetAddresses = new Dictionary<long, UIntPtr>();
                    
                    foreach (long addr in addresses)
                    {
                        sourceAddresses[addr] = MemLib.GetCachedAddress((addr + 0xA2).ToString("X"));
                        targetAddresses[addr] = MemLib.GetCachedAddress((addr + 0x9E).ToString("X"));
                    }
                    
                    // Batch read all values first
                    Dictionary<long, int> sourceValues = new Dictionary<long, int>();
                    Dictionary<long, int> targetOriginalValues = new Dictionary<long, int>();
                    
                    foreach (long addr in addresses)
                    {
                        sourceValues[addr] = MemLib.FastReadInt(sourceAddresses[addr]);
                        targetOriginalValues[addr] = MemLib.FastReadInt(targetAddresses[addr]);
                        originalValuesWrite[addr + 0x9E] = targetOriginalValues[addr];
                    }
                    
                    // Now batch write all values at once
                    foreach (long addr in addresses)
                    {
                        // Direct write of int value without string conversions
                        MemLib.FastWriteInt(targetAddresses[addr], sourceValues[addr]);
                    }
                    
                    // Short sound to indicate completion
                    Console.Beep(4500, 50);
                }
                else
                {
                    // Disable with minimal overhead
                    Console.WriteLine("AimNeck deactivated");
                    
                    // Prepare all UIntPtr addresses in advance
                    Dictionary<long, UIntPtr> targetAddresses = new Dictionary<long, UIntPtr>();
                    foreach (long addr in addresses)
                    {
                        targetAddresses[addr] = MemLib.GetCachedAddress((addr + 0x9E).ToString("X"));
                    }
                    
                    // Batch restore all original values directly 
                    foreach (long addr in addresses)
                    {
                        long addressrep = addr + 0x9E;
                        if (originalValuesWrite.ContainsKey(addressrep))
                        {
                            MemLib.FastWriteInt(targetAddresses[addr], originalValuesWrite[addressrep]);
                        }
                    }
                    
                    // Clear collections
                    originalValuesWrite.Clear();
                    modifiedValuesWrite.Clear();
                    
                    // Short sound to indicate completion
                    Console.Beep(3500, 50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AimNeckOptimized error: {ex.Message}");
                aimActive = false;
            }
        }
        
        // Original AimNeckv1 method (keeping for reference)
        public async static void AimNeckv1()
        {
            originalValuesWrite.Clear();
            modifiedValuesWrite.Clear();
            originalValuesWrite2.Clear();
            modifiedValuesWrite2.Clear();
            
            Console.Beep(3500, 500);
            
            try
            {
                Int32 proc = Process.GetProcessesByName("HD-Player")[0].Id;
                MemLib.OpenProcess(proc);
                
                IEnumerable<long> addresses = await MemLib.AoBScan2(("FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43"), true);
                
                if (addresses == null || !addresses.Any())
                {
                    Console.WriteLine("No AimNeck Found!");
                    return;
                }
                
                foreach (long addr in addresses)
                {
                    long addressscan = addr + 0xA2; // 162 in decimal
                    long addressrep = addr + 0x9E;  // 158 in decimal
                    
                    // Read original values
                    byte[] bytesRep = MemLib.AhReadMeFucker(addressrep.ToString("X"), 4);
                    int bufferWrite = BitConverter.ToInt32(bytesRep, 0);
                    originalValuesWrite[addressrep] = bufferWrite;
                    
                    byte[] bytesScan = MemLib.AhReadMeFucker(addressscan.ToString("X"), 4);
                    int bufferRead = BitConverter.ToInt32(bytesScan, 0);
                    originalValuesWrite2[addressscan] = bufferRead;
                    
                    // Swap values
                    MemLib.WriteMemory(addressrep.ToString("X"), "int", bufferRead.ToString());
                    modifiedValuesWrite[addressrep] = bufferRead;
                    
                    MemLib.WriteMemory(addressscan.ToString("X"), "int", bufferWrite.ToString());
                    modifiedValuesWrite2[addressscan] = bufferWrite;
                }
                
                Console.WriteLine("AimNeck Load Success");
                Console.Beep(4500, 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Inject, Error: {ex.Message}");
            }
        }
        
        // TurboAimNeck - Maximum performance implementation
        public async static void TurboAimNeck()
        {
            try
            {
                // Toggle the state
                aimActive = !aimActive;
                
                // Get cached addresses
                var addresses = await FindAddresses();
                if (addresses.Count == 0) return;
                
                if (aimActive)
                {
                    Console.WriteLine("TurboAimNeck activated");
                    
                    // Create lists of addresses to batch process
                    List<UIntPtr> sourceAddrs = new List<UIntPtr>();
                    List<UIntPtr> targetAddrs = new List<UIntPtr>();
                    Dictionary<UIntPtr, UIntPtr> addrPairs = new Dictionary<UIntPtr, UIntPtr>();
                    
                    // Pre-cache all addresses and prepare batches
                    foreach (long addr in addresses)
                    {
                        UIntPtr sourceAddr = MemLib.GetCachedAddress((addr + 0xA2).ToString("X"));
                        UIntPtr targetAddr = MemLib.GetCachedAddress((addr + 0x9E).ToString("X"));
                        
                        sourceAddrs.Add(sourceAddr);
                        targetAddrs.Add(targetAddr);
                        addrPairs[targetAddr] = sourceAddr;
                    }
                    
                    // Preallocate buffers for all addresses
                    MemLib.PreallocateBuffers(sourceAddrs);
                    MemLib.PreallocateBuffers(targetAddrs);
                    
                    // Batch read all source and target values
                    Dictionary<UIntPtr, int> sourceValues = MemLib.BatchReadInts(sourceAddrs);
                    Dictionary<UIntPtr, int> targetValues = MemLib.BatchReadInts(targetAddrs);
                    
                    // Store original values for restoration
                    foreach (long addr in addresses)
                    {
                        UIntPtr targetAddr = MemLib.GetCachedAddress((addr + 0x9E).ToString("X"));
                        if (targetValues.ContainsKey(targetAddr))
                        {
                            originalValuesWrite[addr + 0x9E] = targetValues[targetAddr];
                        }
                    }
                    
                    // Prepare the new values to write
                    Dictionary<UIntPtr, int> valuesToWrite = new Dictionary<UIntPtr, int>();
                    foreach (var targetAddr in targetAddrs)
                    {
                        if (addrPairs.ContainsKey(targetAddr) && sourceValues.ContainsKey(addrPairs[targetAddr]))
                        {
                            valuesToWrite[targetAddr] = sourceValues[addrPairs[targetAddr]];
                        }
                    }
                    
                    // Batch write all values
                    MemLib.BatchWriteInts(valuesToWrite);
                    
                    // Very short beep
                    Console.Beep(4500, 30);
                }
                else
                {
                    Console.WriteLine("TurboAimNeck deactivated");
                    
                    // Prepare the restoration values
                    Dictionary<UIntPtr, int> restoreValues = new Dictionary<UIntPtr, int>();
                    
                    foreach (long addr in addresses)
                    {
                        long addressKey = addr + 0x9E;
                        if (originalValuesWrite.ContainsKey(addressKey))
                        {
                            UIntPtr targetAddr = MemLib.GetCachedAddress(addressKey.ToString("X"));
                            restoreValues[targetAddr] = originalValuesWrite[addressKey];
                        }
                    }
                    
                    // Batch restore all values
                    MemLib.BatchWriteInts(restoreValues);
                    
                    // Clear dictionaries
                    originalValuesWrite.Clear();
                    modifiedValuesWrite.Clear();
                    
                    // Very short beep
                    Console.Beep(3500, 30);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TurboAimNeck error: {ex.Message}");
                aimActive = false;
            }
        }
        
        // InstantAimNeck - Absolute minimum latency implementation with zero overhead
        public async static void InstantAimNeck()
        {
            try
            {
                // Toggle state without any console output
                aimActive = !aimActive;
                
                // Fast retrieval of cached addresses - but only do this once and cache them statically
                if (cachedAddresses == null || cachedAddresses.Count == 0)
                {
                    var addresses = await FindAddresses();
                    if (addresses.Count == 0) return;
                    cachedAddresses = addresses;
                }
                
                // Direct raw memory access for maximum speed
                if (aimActive)
                {
                    foreach (long addr in cachedAddresses)
                    {
                        // Direct math - avoid all conversions
                        long sourceAddr = addr + 0xA2;
                        long targetAddr = addr + 0x9E;
                        
                        // Raw direct memory read
                        int sourceValue = MemLib.DirectReadMemory(sourceAddr);
                        
                        // Raw direct memory write
                        MemLib.DirectWriteMemory(targetAddr, sourceValue);
                    }
                }
            }
            catch
            {
                // Minimal exception handling
                aimActive = false;
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("AimbotTest - HD-Player Memory Modifier");
            Console.WriteLine("Press 1 to activate AimNeck (Original)");
            Console.WriteLine("Press 2 to activate/deactivate AimNeckv1 (New Version - Safe)");
            Console.WriteLine("Press 3 to toggle optimized AimNeck (Fastest - RISK)");
            Console.WriteLine("Press 4 to toggle TurboAimNeck (Ultra-Fast - RISK)");
            Console.WriteLine("Press 5 to toggle InstantAimNeck (Zero Overhead - RISK)");
            Console.WriteLine("Press ESC to exit");
            
            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    
                    switch (key.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            Console.WriteLine("Activating AimNeck (Original)...");
                            Aimbot();
                            break;
                            
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            Console.WriteLine("Activating AimNeckv1 (New Version)...");
                            AimNeckv1();
                            break;
                            
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            Console.WriteLine("Toggling optimized AimNeck...");
                            AimNeckOptimized();
                            break;
                            
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            Console.WriteLine("Toggling TurboAimNeck...");
                            TurboAimNeck();
                            break;
                            
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            // No console output to prevent lag
                            InstantAimNeck();
                            break;
                            
                        case ConsoleKey.Escape:
                            running = false;
                            break;
                    }
                }
                
                // Reduced sleep time for faster response
                System.Threading.Thread.Sleep(50);
            }
        }
        
        // Helper method to get process ID from name
        private static int GetProcIdFromName(string name)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(name.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : -1;
        }
    }
}
