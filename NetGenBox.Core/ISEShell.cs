using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NetGenBox.Core;

public class ISEShell
{
    public string ISEPath { get; set; }

    public ISEShell(string isePath)
    {
        ISEPath = isePath;
    }


    public async Task<int> CreateProject(string topModule, string workDir, List<string> files,
        Action<string>? stdOut = null, Action<string>? stdErr = null)
    {
        string command;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = ISEPath + "/bin/lin64/xtclsh";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = ISEPath + "/bin/lin64/xtclsh";
        }
        else
        {
            command = ISEPath + "/bin/lin64/xtclsh";
        }
        
        string arguments = "/tmp/Project.tcl";

        string generateXfiles = "";
        files.ForEach(file => { generateXfiles += $"xfile add {file}\n"; });

        string args = @$"
            #Module 0
            project new {topModule}.ise
            project set family CoolRunner2
            project set device XC2C512
            project set package PQ208
            project set speed -7
            {generateXfiles}
            set top_name {topModule}
            process run ""Synthesize - XST""
            project close
        ";
        File.Delete("/tmp/*.ise");
        await File.WriteAllTextAsync("/tmp/Project.tcl", args);
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new Process { StartInfo = startInfo };
        process.Start();

        // Reading output stream
        Task.Run(async () =>
        {
            using StreamReader reader = process.StandardOutput;
            char[] buffer = new char[2048];
           
            if (stdOut is not null)
            {
                int len;
                while((len = await reader.ReadAsync(buffer)) != 0)
                {
                    stdOut(new string(buffer, 0, len));
                }
              
            }
        }).ConfigureAwait(false);


        // Reading error stream
        Task.Run(async () =>
        {
            using StreamReader reader = process.StandardError;
            char[] buffer = new char[2048];
            
            if (stdErr is not null)
            {
                int len;
                while((len = await reader.ReadAsync(buffer)) != 0)
                {
                    stdErr(new string(buffer, 0, len));
                }
            }
        }).ConfigureAwait(false);


        await process.WaitForExitAsync(); // Wait for the process to finish
        return process.ExitCode;
    }

    public async Task<int> WriteEdif(string edifName, string outputName, string workDir, bool overwrite = true, Action<string>? stdOut = null, Action<string>? stdErr = null)
    {
        string command;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = ISEPath + "/bin/lin64/ngc2edif";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = ISEPath + "/bin/lin64/ngc2edif.exe";
        }
        else
        {
            command = ISEPath + "/bin/lin64/ngc2edif";
        }
        Console.WriteLine(command);
        string arguments =
            $"{(overwrite ? "-w" : "")} {edifName} {outputName.Replace(".ndf", "")}.ndf"; // Example command

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new Process { StartInfo = startInfo };
        process.Start();

        // Reading output stream
        Task.Run(async () =>
        {
            using StreamReader reader = process.StandardOutput;
            char[] buffer = new char[2048];
           
            if (stdOut is not null)
            {
                int len;
                while((len = await reader.ReadAsync(buffer)) != 0)
                {
                    stdOut(new string(buffer, 0, len));
                }
              
            }
        }).ConfigureAwait(false);

        // Reading error stream
        Task.Run(async () =>
        {
            using StreamReader reader = process.StandardError;
            char[] buffer = new char[2048];
            
            if (stdErr is not null)
            {
                int len;
                while((len = await reader.ReadAsync(buffer)) != 0)
                {
                    stdErr(new string(buffer, 0, len));
                }
            }
        }).ConfigureAwait(false);

        await process.WaitForExitAsync(); // Wait for the process to finish
        return process.ExitCode;
    }
}