using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FakeCMD
{
    class Program
    {
        private static DirectoryInfo CurrentDirectory { get; set; }

        private static string NewCommandPrompt => CurrentDirectory.FullName + ">";

        private static Process RunningProcess { get; set; }

        private static string CapturedString { get; set; } //This will contain the string the user typed in while a process was running

        private static bool BreakCaptured { get; set; } //This is triggered when the user hits ctrl-c

        private static void CommandLoop()
        {
            while (true)
            {
                //Show prompt
                Console.Write(NewCommandPrompt);

                //Uncomment this line if you want to see what the user typed in during a process (virus found!)
                //if (!string.IsNullOrEmpty(CapturedString)) Console.Write(CapturedString);

                //Get command from the user
                string Command = Console.ReadLine();

                //If the user hits the ctlr-c then Command is null
                if (Command == null)
                {
                    Console.WriteLine();
                    continue;
                }

                //format the command string
                string lcommand = Command.Replace(" ", "").ToLower();

                //Do something with the command

                //Change Directory (required)
                if (lcommand.StartsWith("cd.."))
                {
                    if (CurrentDirectory.Parent != null) CurrentDirectory = CurrentDirectory.Parent;
                    continue;
                }

                //Change Directory (required)
                if (lcommand.StartsWith(@"cd\"))
                {
                    CurrentDirectory = CurrentDirectory.Root;
                    continue;
                }

                //Change Directory (required)
                if (lcommand.StartsWith("cd"))
                {
                    //get new directory name
                    var newdir = Path.Combine(CurrentDirectory.FullName, Command.Replace("cd ", ""));
                    if (Directory.Exists(newdir))
                    {
                        CurrentDirectory = new DirectoryInfo(newdir);
                    }
                    else
                    {
                        Console.WriteLine("The system cannot find the specified path.\n");
                    }

                    continue;
                }

                if (lcommand.StartsWith("tree"))
                {
                    StartProcess(@"c:\windows\system32\tree.com");
                    Console.WriteLine(Properties.Resources.Tree);
                    continue;
                }

                if (lcommand.StartsWith("syskey"))
                {

                    Process.Start(@"https://en.wikipedia.org/wiki/Technical_support_scam");
                    continue;
                }

                //ascii art examples
                if (lcommand.StartsWith("art"))
                {
                    Console.WriteLine(Properties.Resources.Tree);
                    Console.WriteLine(Properties.Resources.Brain);
                    Console.WriteLine(Properties.Resources.Donkey);
                    continue;
                }

                if (lcommand.StartsWith("dir"))
                {
                    //Loop until user hits break
                    while (!BreakCaptured)
                    {
                        Console.WriteLine(Path.GetRandomFileName());
                    }

                    //Reset flag
                    BreakCaptured = false;

                    continue;
                }


                if (lcommand.StartsWith("netstat"))
                {
                    //hacker mode
                    var r = new Random();

                    while (true)
                    {
                        Console.Write((char)r.Next(12, 255));

                        if (r.Next(0, 200) == 123) Console.ForegroundColor = ConsoleColor.DarkGreen;
                        if (r.Next(0, 300) == 123) Console.ForegroundColor = ConsoleColor.Green;
                        if (r.Next(0, 500) == 123) Console.ForegroundColor = ConsoleColor.Red;

                        if (r.Next(0, 20) == 1) Console.Beep(r.Next(200, 2700), 100);

                        if (r.Next(0, 40) == 2)
                        {
                            Console.SetCursorPosition(r.Next() % Console.BufferWidth, r.Next() % Console.BufferWidth);
                        }
                    }

                }

                //Run the command (if none of them match above)
                StartProcess(Command);
            }
        }

        /// <summary>
        /// Attempts to start the process the user typed in.
        /// </summary>
        /// <param name="Command"></param>
        private static void StartProcess(string Command, string Arguments = "")
        {
            try
            {
                var so = new ProcessStartInfo(Command);

                so.UseShellExecute = false;

                so.WorkingDirectory = CurrentDirectory.FullName;

                if (Command.Contains(" "))
                {
                    so.FileName = Command.Substring(0, Command.IndexOf(' '));
                    so.Arguments = Command.Substring(Command.IndexOf(' '));
                }

                RunningProcess = Process.Start(so);

                RunningProcess.WaitForExit();

            }
            catch (Exception)
            {
                Console.WriteLine("'{0}' is not recognized as an internal or external command,\noperable program or batch file.\n", Command.Split(' ')[0]);
            }
            finally
            {
                //Make sure console buffer is clear before returning
                while (Console.KeyAvailable) Console.ReadKey(true);
            }

        }

        /// <summary>
        /// Captures the ctrl-break ctrl-c event.  Aborts a running a process. 
        /// </summary>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {

            //If a process is running then kill it
            if (RunningProcess != null && !RunningProcess.HasExited)
            {
                RunningProcess.Kill();
                RunningProcess = null;
            }

            //Capture keys pressed while the process is running (don't display them in the console)
            CapturedString = string.Empty;
            while (Console.KeyAvailable)
            {
                CapturedString += Console.ReadKey(true).KeyChar;
            }

            BreakCaptured = true;

            e.Cancel = true;
        }

        static void Main(string[] args)
        {
            //Set title to match a real command prompt
            Console.Title = "Command Prompt";

            //Command prompt startup message
            Console.WriteLine("Microsoft Windows [Version 7.0.16299.492]\n(c) 2016 Microsoft Corporation.All rights reserved.\n");

            //Set current directory
            CurrentDirectory = new DirectoryInfo(@"C:\Windows\System32"); 

            //Capture ctrl-c ctrl-break 
            Console.CancelKeyPress += Console_CancelKeyPress;

            //Run the command loop
            CommandLoop();
        }
    }
}
