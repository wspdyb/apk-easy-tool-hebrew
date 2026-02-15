using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKEasyTool.Utils
{
    class CMD
    {
        private static MainForm main;

        public CMD(MainForm Main)
        {
            main = Main;
        }

        // פונקציה 1: משמשת לקריאת פרטי ה-APK (שם, גרסה וכו')
        public static string ProcessStartWithOutput(string FileName, string Arguments)
        {
            string result = string.Empty;
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = FileName;
                    process.StartInfo.Arguments = Arguments;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;

                    // --- תיקון עברית: הגדרת קידוד UTF-8 ---
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    // ----------------------------------------

                    process.Start();
                    // קריאת הפלט עם הקידוד הנכון
                    result = process.StandardOutput.ReadToEnd().Trim() + process.StandardError.ReadToEnd().Trim();
                    process.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return result;
        }

        public static void ProcessStartWithArgs(string FileName, string Arguments)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = FileName;
                    process.StartInfo.Arguments = Arguments;
                    process.Start();
                    process.WaitForExit(5000);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Start", e);
            }
        }

        // פונקציה 2: משמשת ללוגים בזמן אמת (Decompile/Compile)
        public static void StartProgram(string filename, string commandLine, bool logoutput, out int ExitCode)
        {
            // בדיקה ש-main אותחל (למניעת קריסות נדירות)
            if (main != null)
                main.LogOutput("Command: " + filename + " " + commandLine + "\n");

            var info = new ProcessStartInfo();
            info.FileName = filename;
            info.Arguments = commandLine;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;

            // --- תיקון עברית: הגדרת קידוד UTF-8 ---
            info.StandardOutputEncoding = Encoding.UTF8;
            info.StandardErrorEncoding = Encoding.UTF8;
            // ----------------------------------------

            using (var p = new Process())
            {
                p.StartInfo = info;

                p.OutputDataReceived += (s, o) =>
                {
                    if (logoutput && o.Data != null && main != null)
                        main.LogOutput(o.Data);
                };
                p.ErrorDataReceived += (s, o) =>
                {
                    if (o.Data != null && main != null)
                        main.LogOutput(o.Data);
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                ExitCode = p.ExitCode;
            }
        }
    }
}
