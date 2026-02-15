using APKEasyTool.Enums;
using APKEasyTool.Forms;
using APKEasyTool.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APKEasyTool
{
    public class Apktool
    {
        #region fields
        private static MainForm main;
        public static ProcessType processType;
        public Process p = new Process();
        #endregion

        public Apktool(MainForm Main)
        {
            main = Main;
        }

        static string GetXmx()
        {
            if (main.useJaxaXmxChkBox.Checked)
                return "-Xmx" + main.javaHeapSizeNum.Text + "m ";
            else
                return "";
        }

        static string GetJavaPath()
        {
            if (main.useExtJava.Checked)
                return "\"" + main.extJavaTxtBox.Text + "\"";
            else
                return "java.exe";
        }

        public static async Task Decompile(string input, string output)
        {
            // --- FIX: Decompile Hebrew Support ---
            string tempPath = Path.Combine(Path.GetTempPath(), "AET_Dec_" + Guid.NewGuid().ToString().Substring(0,6) + ".apk");
            bool isTemp = false;
            if (Regex.IsMatch(input, @"[\u0590-\u05FF]") && File.Exists(input))
            {
                try { File.Copy(input, tempPath, true); input = tempPath; isTemp = true; } catch {}
            }
            // -------------------------------------

            processType = ProcessType.Java;
            string flag = "";
            if (main.decBcheckBox.Checked) flag += " -b";
            if (main.decFcheckBox.Checked) flag += " -f";
            if (main.decKcheckBox.Checked) flag += " -k";
            if (main.decMcheckBox.Checked) flag += " -m";
            if (main.decRcheckBox.Checked) flag += " -r";
            if (main.decScheckBox.Checked) flag += " -s";
            if (main.decNoAssetsCheckBox.Checked) flag += " --no-assets";
            if (!String.IsNullOrEmpty(main.decodeApiLvl.Text)) flag += " -api " + main.decodeApiLvl.Text;
            if (main.decOnlyMainClassesChkBox.Checked) flag += " --only-main-classes";
            if (main.useTagChkBox.Checked) flag += " -t \"" + main.tagTxtBox.Text + "\"";
            if (!String.IsNullOrEmpty(main.fwDirTxtBox.Text)) flag += " -p \"" + main.fwDirTxtBox.Text + "\"";
            if (main.decForceManifestChkBox.Checked) flag += " --force-manifest";

            main.DisableForm();
            main.LogOutput("---------------------------");
            main.LogOutput("\n" + (Path.GetExtension(input) == ".apk" ? Lang.DEC_APK_FILE_NOTICE : Lang.DEC_JAR_FILE_NOTICE) + " " + Lang.CANCEL_ESC, MainForm.Type.Info);

            string command = GetXmx() + " -jar " + Variables.Apktool + " d" + flag + " -o \"" + output + "\" \"" + input + "\"";

            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });

            if (exitCode == 0)
            {
                main.LogOutput(Lang.DEC_SUCCESS_MBOX, MainForm.Type.Info);
                if (Environment.GetCommandLineArgs().Length > 2) Environment.Exit(0);
            }
            else
            {
                main.LogOutput(Lang.DEC_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            
            if (isTemp) { try { File.Delete(tempPath); } catch {} }
            main.EnableForm();
        }

        // --- הפונקציה המתוקנת לבנייה ---
        public static async Task Compile(string input, string output)
        {
            // --- FIX: Force English Folder Name ---
            string originalInput = input;
            string originalOutput = output;
            string uniqueId = Guid.NewGuid().ToString().Substring(0, 6);
            string safeInput = Path.Combine(Path.GetDirectoryName(input), "AET_Build_" + uniqueId);
            string safeOutput = Path.Combine(Path.GetDirectoryName(output), "AET_Out_" + uniqueId + ".apk");
            bool renamed = false;
            bool tempOut = false;

            try
            {
                // תמיד מנסים לשנות שם לאנגלית לפני העבודה
                if (Directory.Exists(input))
                {
                    if (Directory.Exists(safeInput)) Directory.Delete(safeInput, true);
                    Directory.Move(input, safeInput);
                    input = safeInput;
                    renamed = true;
                }
                // תמיד שומרים לקובץ זמני באנגלית
                output = safeOutput;
                tempOut = true;
            }
            catch (Exception ex)
            {
                main.LogOutput("Warning: Rename failed (" + ex.Message + ")", MainForm.Type.Error);
            }
            // --------------------------------------

            processType = ProcessType.Java;
            string flag = "";
            if (main.comCcheckBox.Checked) flag += " -c";
            if (main.comDcheckBox.Checked) flag += " -d";
            if (main.comFcheckBox.Checked) flag += " -f";
            if (main.chkBoxUseAapt2.Checked) flag += " --use-aapt2";
            if (!String.IsNullOrEmpty(main.rebuildApiLvl.Text)) flag += " -api " + main.rebuildApiLvl.Text;
            if (main.comNCcheckBox.Checked) flag += " -nc";
            if (main.useAaptPathChkBox.Checked) flag += " --aapt \"" + main.useAaptPathTxtBox.Text + "\"";
            if (!String.IsNullOrEmpty(main.fwDirTxtBox.Text)) flag += " -p \"" + main.fwDirTxtBox.Text + "\"";

            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput("Compiling Project: " + Path.GetFileName(originalInput), MainForm.Type.Info); // לוג נקי

            string command = GetXmx() + "-jar " + Variables.Apktool + " b" + flag + " -o \"" + output + "\" \"" + input + "\"";

            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });

            if (exitCode == 0)
            {
                // שחזור שם הקובץ הסופי
                if (tempOut && File.Exists(output))
                {
                    try 
                    {
                        if (File.Exists(originalOutput)) File.Delete(originalOutput);
                        File.Move(output, originalOutput);
                        output = originalOutput;
                        main.LogOutput("I: Built apk into: " + originalOutput, MainForm.Type.Info);
                    }
                    catch { main.LogOutput("Warning: Output at " + output, MainForm.Type.Warning); }
                }
                else 
                {
                    main.LogOutput(Lang.COM_SUCCESS_MBOX, MainForm.Type.Info);
                }

                string checkPath = renamed ? originalInput : input;
                if (File.Exists(Path.Combine(checkPath, "apktool.yml")) && File.Exists(Path.Combine(checkPath, "AndroidManifest.xml")))
                {
                    if (main.zipAfterSignChkBox.Checked) await ZipAlign(output, output);
                    else if (main.signApkCheckBox.Checked) await Sign(output, output);
                    else if (Environment.GetCommandLineArgs().Length > 2) Environment.Exit(0);
                }
            }
            else
            {
                main.LogOutput(Lang.COM_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }

            // שחזור שם התיקייה המקורית
            if (renamed)
            {
                try 
                {
                    if (Directory.Exists(input)) 
                    {
                        if (Directory.Exists(originalInput)) Directory.Delete(originalInput, true);
                        Directory.Move(input, originalInput);
                    }
                }
                catch { main.LogOutput("CRITICAL: Restore folder failed. Data is in: " + input, MainForm.Type.Error); }
            }
            main.EnableForm();
        }

        public static async Task Sign(string input, string output)
        {
            processType = ProcessType.Java;
            string flag = "";
            if (main.v2signComboBox.SelectedIndex == 1) flag += " --v2-signing-enabled true";
            else if (main.v2signComboBox.SelectedIndex == 2) flag += " --v2-signing-enabled false";
            if (main.v3signComboBox.SelectedIndex == 1) flag += " --v3-signing-enabled true";
            else if (main.v3signComboBox.SelectedIndex == 2) flag += " --v3-signing-enabled false";
            if (main.v4signComboBox.SelectedIndex == 1) flag += " --v4-signing-enabled true";
            else if (main.v4signComboBox.SelectedIndex == 2) flag += " --v4-signing-enabled false";
            else if (main.v4signComboBox.SelectedIndex == 3) flag += " --v4-signing-enabled only";

            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.SIGN_APK_FILE_NOTICE + " " + Lang.CANCEL_ESC, MainForm.Type.Info);

            string command = null;
            if (main.useJksCheckBox.Checked)
            {
                if (main.jksFileTxtBox.Text == "") { MessageBox.Show(Lang.KEY_NOT_SEL_MBOX, Program.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                command = GetXmx() + "-jar " + Variables.ApkSignerPath + " --ks \"" + main.jksFileTxtBox.Text + "\" --ks-pass pass:" + main.jksPass.Text + " " + flag + " --out \"" + output + "\" \"" + input + "\"";
            }
            else
            {
                command = GetXmx() + "-jar " + Variables.ApkSignerPath + " --key \"" + main.pk8FileTxtBox.Text + "\" --cert \"" + main.pemFileTxtBox.Text + "\" " + flag + " --out \"" + output + "\" \"" + input + "\"";
            }

            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });

            if (exitCode == 0)
            {
                main.LogOutput(Lang.SIGN_SUCCESS_MBOX, MainForm.Type.Info);
                if (main.installApkChkBox.Checked) await InstallApk(input);
                else if (Environment.GetCommandLineArgs().Length > 2) Environment.Exit(0);
            }
            else
            {
                main.LogOutput(Lang.SIGN_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task InstallApk(string input)
        {
            processType = ProcessType.Adb;
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.INS_APK_FILE_LOG + " " + Lang.CANCEL_ESC, MainForm.Type.Info);
            string command = "install -r \"" + input + "\"";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(Variables.RealPath("Resources\\adb.exe"), command, true, out exitCode); });
            if (exitCode == 0)
            {
                main.LogOutput("\n" + Lang.INS_SUCCESS_MBOX, MainForm.Type.Info);
                if (Environment.GetCommandLineArgs().Length > 2) Environment.Exit(0);
            }
            else
            {
                main.LogOutput("\n" + Lang.INS_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task ZipAlign(string input, string output)
        {
            processType = ProcessType.Zipalign;
            string flag = "";
            if (main.zipFcheckBox.Checked) flag += " -f";
            if (main.zipPcheckBox.Checked) flag += " -p";
            if (main.zipZcheckBox.Checked) flag += " -z";
            if (main.zipVcheckBox.Checked) flag += " -v";

            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.ZIPALIGN_APK_FILE_NOTICE + " " + Lang.CANCEL_ESC, MainForm.Type.Info);

            string s = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input) + " zipalign temp.apk");
            if (File.Exists(s)) File.Delete(s);
            string command = flag + " 4 \"" + input + "\" \"" + s + "\"";

            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(Variables.RealPath("Resources\\zipalign.exe"), command, true, out exitCode); });

            if (exitCode == 0)
            {
                if (File.Exists(output)) File.Delete(output);
                if (File.Exists(s)) File.Move(s, output);
                main.LogOutput(Lang.ZIPALIGN_SUCCESS_MBOX, MainForm.Type.Info);
                if (main.signApkCheckBox.Checked) await Sign(output, output);
                else if (Environment.GetCommandLineArgs().Length > 2) Environment.Exit(0);
            }
            else
            {
                main.LogOutput(Lang.ZIPALIGN_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task CheckAlign(string input)
        {
            processType = ProcessType.Zipalign;
            string flag = "";
            if (main.zipFcheckBox.Checked) flag += " -f";
            if (main.zipPcheckBox.Checked) flag += " -p";
            if (main.zipZcheckBox.Checked) flag += " -z";
            if (main.zipVcheckBox.Checked) flag += " -v";
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.VERI_APK_FILE_LOG + " " + Lang.CANCEL_ESC, MainForm.Type.Info);
            if (!main.zipVcheckBox.Checked) main.LogOutput("\n\n" + Lang.ZIPALIGN_OUTPUT_DIS_LOG);
            string command = "-c" + flag + " 4 \"" + input + "\"";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(Variables.RealPath("Resources\\zipalign.exe"), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.VERI_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.VERI_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task Framework(string input)
        {
            processType = ProcessType.Java;
            string fwtag = main.tagFwTxtBox.Text != "" ? "-t \"" + main.tagFwTxtBox.Text + "\"" : "";
            string fwpath = main.installFwDirTxtBox.Text != "" ? "-p \"" + main.installFwDirTxtBox.Text + "\"" : "";
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.FW_APK_FILE_LOG, MainForm.Type.Info);
            string command = GetXmx() + "-jar " + Variables.Apktool + " if \"" + input + "\" " + fwtag + " " + fwpath + " ";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.FW_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.FW_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task FrameworkClear()
        {
            processType = ProcessType.Java;
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.FW_CLR_APK_FILE_LOG, MainForm.Type.Info);
            string command = GetXmx() + " -jar " + Variables.Apktool + " empty-framework-dir --force \"";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.FW_CLR_SUCCESS_MBOX, MainForm.Type.Info);
            else main.LogOutput(Lang.FW_CLR_FAIL_MBOX, MainForm.Type.Error);
            main.EnableForm();
        }

        public static async Task ZipApk(string input, string output)
        {
            processType = ProcessType.SevenZip;
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.ZIP_APK_FILE_LOG, MainForm.Type.Info);
            string command = "a -tzip \"" + output + "\" \"" + input + "\\*\" -mx" + main.sevenzComUpDown.Text + " -aoa";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(Variables.RealPath("Resources\\7z.exe"), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.ZIP_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.ZIP_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task ExtractApk(string input, string output)
        {
            processType = ProcessType.SevenZip;
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.EXT_APK_FILE_LOG, MainForm.Type.Info);
            string command = "x \"" + input + "\" -o\"" + output + "\" -aoa";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(Variables.RealPath("Resources\\7z.exe"), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.EXT_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.EXT_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task Baksmali(string input, string output)
        {
            processType = ProcessType.Java;
            string de = main.deodexChkBox.Checked ? "x" : "d";
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.BAKSMALI_LOG, MainForm.Type.Info);
            string command = GetXmx() + " -jar " + "\"" + Variables.RealPath("Resources\\baksmali.jar") + "\" " + de + " \"" + input + "\" -o \"" + output + "\"";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.BAKSMALI_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.BAKSMALI_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }

        public static async Task Smali(string input, string output)
        {
            processType = ProcessType.Java;
            main.DisableForm();
            main.LogOutput("---------------------------\n");
            main.LogOutput(Lang.SMALI_LOG, MainForm.Type.Info);
            string command = GetXmx() + " -jar " + "\"" + Variables.RealPath("Resources\\smali.jar") + "\" a \"" + input + "\" -o \"" + output + "\"";
            int exitCode = 0;
            await Task.Factory.StartNew(() => { CMD.StartProgram(GetJavaPath(), command, true, out exitCode); });
            if (exitCode == 0) main.LogOutput(Lang.SMALI_SUCCESS_MBOX, MainForm.Type.Info);
            else
            {
                main.LogOutput(Lang.SMALI_FAIL_MBOX, MainForm.Type.Error);
                if (!MainClass.isSidedLogOpened) main.tMain.SelectedIndex = 2;
            }
            main.EnableForm();
        }
    }
}
