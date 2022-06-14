using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TDMP_CGS_Launcher
{   
    /* WHY THIS PATCH FOR THE LAUNCHER
     * TDMP LAUNCHER WRITTEN IN C++ ITS KINDA MESSY, IT TRIES TO START THE GAME FROM THE APPID FOR TRYING TO "EVADE" THE PIRACY (Literally hex-editing a number is enough to bypass it)
     * THIS RUNS THE GAME FROM THE EXE, THAT MEANS THE PROCESS WILL START UP AND INJECT TDMP NO MATTERS IF ITS CRACKED OR NOT. 
     * ONCE YOU START THE GAME THE TDMP CONSOLE WILL APPEAR. AND AFTERALL IF YOU OWN THE GAME BY A LEGIT WAY YOU CAN OPEN UP THE STEAM OVERLAY OTHERWISE YOU COULD NOT, PURCHASE THE GAME OR DO SOME RESEARCH.
     * Security Tip: Dont implement anti-piracy on an injector, its stupid and easy to crack, do it on the dll, harder to find and edit if you dont want to break something.
    */


    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;

        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to inject tdmp? Y/N");
            var arg = Console.ReadLine();
            if (arg == "Y" || arg == "y")
            {
                if (File.Exists("teardown.exe") && File.Exists("tdmp.dll"))
                {
                    Console.WriteLine("[TDMP]: CGS LAUNCHER-FIX");
                    Console.WriteLine("Starting teardown process.\n Made by CGS1999");
                    Console.WriteLine("Make sure teardown is closed and run this.");
                    var process = Process.Start("teardown.exe");
                    var a = InjectDll(process);
                    if (a == 0)
                    {
                        return;
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("[TDMP]: SEEMS SOME OF YOUR FILES ARE MISSING, It may be teardown.exe or tdmp.dll. Make sure you got them!");
                }
            }
            else if(arg == "N" || arg == "n")
            {
                var process = Process.Start("teardown.exe");
            }
            else
            {
                Console.Clear();
                Console.WriteLine("[TDMP]: Input Not Recognized");
                Main(args);
            }
        }


        static int InjectDll(Process prc)
        {
            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, prc.Id);
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            string dllName = "tdmp.dll";

            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((dllName.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            // writing the name of the dll there
            UIntPtr bytesWritten;
            WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(dllName), (uint)((dllName.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);

            // creating a thread that will call LoadLibraryA with allocMemAddress as argument
            CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

            return 0;

        }
    }
}
