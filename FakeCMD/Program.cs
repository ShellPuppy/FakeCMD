using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FakeCMD
{
    class Program
    {
        //Keeps track of the current directory
        private static DirectoryInfo CurrentDirectory { get; set; }

        //String that is shown at the start of a new command i.e. C:\Windows>
        private static string NewCommandPrompt => CurrentDirectory.FullName + ">";

        //Reference to the process started by the user 
        private static Process RunningProcess { get; set; }

        //If the user types while a process is running the text is captured into this string
        private static string CapturedString { get; set; }

        //This is flagged when the user hits ctrl-c
        private static bool BreakCaptured { get; set; }

        private static void CommandLoop()
        {
            while (true)
            {
                //Show new command prompt
                Console.Write(NewCommandPrompt);

                //Uncomment this line if you want to see what the user typed in during a process (virus found!)
                //if (!string.IsNullOrEmpty(CapturedString)) Console.Write(CapturedString);



                //Get command from the user
                string Command = CaptureKeyStrokes(); //Console.ReadLine();

                //If the user hits ctlr-c then Command is null
                if (Command == null)
                {
                    Console.WriteLine();
                    continue;
                }

                //Cleanup the command string
                string lcommand = Command.Replace(" ", "").ToLower();


                //
                //Do something with the command
                //


                //if the user gave an empty string or hit ctrl-c while typing, then just send a new line and start the loop over
                if (string.IsNullOrEmpty(lcommand))
                {
                    Console.Write("\n");
                    continue;
                }

                //cd..
                if (lcommand.StartsWith("cd.."))
                {
                    //Perform the change directory command
                    CurrentDirectory = CurrentDirectory?.Parent ?? CurrentDirectory;
                    continue;
                }

                //cd\
                if (lcommand.StartsWith(@"cd\"))
                {
                    CurrentDirectory = CurrentDirectory.Root;
                    continue;
                }

                //cd
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

                //cls
                if (lcommand == "cls")
                {
                    Console.Clear();
                    Console.WriteLine();
                    continue;
                }

                //tree
                if (lcommand.StartsWith("tree"))
                {
                    StartProcess(@"c:\windows\system32\tree.com");
                    Console.WriteLine(Properties.Resources.Tree);
                    continue;
                }

                //syskey
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

                //dir 
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

                //netstat
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

                //Try to run the command (if none of them match above)
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

                //Get the command and the arguments
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
        /// Capture 
        /// </summary>
        private static string CaptureKeyStrokes()
        {


            ConsoleKeyInfo keypress;

            string result = string.Empty;
            string newresult = string.Empty;
            bool handled = false;

            //Remember where the start of the string is
            int MinCursorPositionLeft = Console.CursorLeft;
            int MinCursMinCursorTop = Console.CursorTop;

            //Keep track of where the cursor is in relation to the string 
            int stringpos = 0;

            try
            {
                Console.TreatControlCAsInput = true;    //We need to capture ctrl-c

                while (true)
                {
                    handled = false;

                    keypress = Console.ReadKey(true); // read next key stroke 

                    //user pressed enter - return the entire string
                    if (keypress.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return result;
                    }

                    //Capture ctrl-c
                    if (keypress.KeyChar == 3)
                    {
                        return string.Empty;
                    }

                    if (keypress.Key == ConsoleKey.Escape)
                    {
                        newresult = string.Empty;
                        stringpos = 0;
                        handled = true;
                    }

                    //Left arrow
                    if (keypress.Key == ConsoleKey.LeftArrow)
                    {
                        Console.SetCursorPosition(Math.Max(MinCursorPositionLeft, Console.CursorLeft - 1), MinCursMinCursorTop);
                        stringpos = Console.CursorLeft - MinCursorPositionLeft;
                        handled = true;
                        continue;
                    }

                    //Right arrow
                    if (keypress.Key == ConsoleKey.RightArrow)
                    {
                        Console.SetCursorPosition(Math.Min(MinCursorPositionLeft + result.Length, Console.CursorLeft + 1), MinCursMinCursorTop);
                        stringpos = Console.CursorLeft - MinCursorPositionLeft;
                        handled = true;
                        continue;
                    }

                    //Back space
                    if (keypress.Key == ConsoleKey.Backspace)
                    {
                        if (Console.CursorLeft - 1 >= MinCursorPositionLeft)
                        {
                            Console.SetCursorPosition(Math.Max(MinCursorPositionLeft, Console.CursorLeft - 1), MinCursMinCursorTop);
                            stringpos = Console.CursorLeft - MinCursorPositionLeft;
                            newresult = result.Remove(stringpos, 1);
                            handled = true;
                        }
                    }

                    //Delete key
                    if (keypress.Key == ConsoleKey.Delete)
                    {
                        stringpos = Console.CursorLeft - MinCursorPositionLeft;
                        if (stringpos < result.Length)
                        {
                            newresult = result.Remove(stringpos, 1);
                        }
                        handled = true;
                    }

                    if (!handled)
                    {
                        stringpos = Console.CursorLeft - MinCursorPositionLeft;
                        newresult = result.Insert(stringpos, keypress.KeyChar.ToString());
                        stringpos += 1;
                    }

                    //We can reject any numbers greater than 999

                    if (ExtractValue(newresult) > 999.99) continue;




                    //Rewrite the entire result string
                    Console.SetCursorPosition(MinCursorPositionLeft, MinCursMinCursorTop);
                    Console.Write(new string(' ', result.Length));
                    Console.SetCursorPosition(MinCursorPositionLeft, MinCursMinCursorTop);
                    result = newresult;
                    Console.Write(newresult);
                    Console.SetCursorPosition(MinCursorPositionLeft + stringpos, MinCursMinCursorTop);
                }
            }
            finally
            {
                Console.TreatControlCAsInput = false; ////We need to let the event handler capture ctrl-c
            }
        }

        private static double ExtractValue(string st)
        {
            double result = 0.0;

            if (string.IsNullOrEmpty(st)) return result;

            var mt = Regex.Matches(st.Replace(",",""),@"(?:\d+(?:\.\d*)?|\.\d+)");
            foreach(Match m in mt)
            {
                if(double.TryParse(m.Value, out double v))
                {
                    if (v > result) result = v;
                }
            }
            return result;
        }


        /// <summary>
        /// Captures the ctrl-break ctrl-c event.  
        /// Aborts a running a process. 
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
            Console.Title = @"C:\Windows\System32\cmd.exe";

            //Command prompt startup message
            Console.WriteLine("Microsoft Windows [Version 10.0.1337.420]\n(c) 2018 Microsoft Corporation.All rights reserved.\n");

            //Set current directory
            CurrentDirectory = new DirectoryInfo(@"C:\Windows\System32");

            //Capture ctrl-c ctrl-break 
            Console.CancelKeyPress += Console_CancelKeyPress;

            //Run the command loop
            CommandLoop();
        }
    }
}
