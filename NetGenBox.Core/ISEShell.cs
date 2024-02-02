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

        string command;
        string tmpPath = "/tmp/";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = ISEPath + "/bin/lin64/xtclsh";
            //File.Delete("/tmp/*.ise");
           
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = ISEPath + "/bin/nt64/xtclsh.exe";
            tmpPath = Path.GetTempPath();
        }
        else
        {
            command = ISEPath + "/bin/lin64/xtclsh";
            tmpPath = Path.GetTempPath();
        }

        string tmpFile = Path.Combine(tmpPath, "Project.tcl");
        await File.WriteAllTextAsync(tmpFile, args);
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = tmpFile,
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
            command = ISEPath + "/bin/nt64/ngc2edif.exe";
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

    public async Task<bool> GenerateXilinxFormatNetList(string ngdPath, string netgenPath,  bool overwrite, bool includeTestBench
    , bool insertGlobalModule, bool dontEscapeName, bool flattenOutput)
    {
        string command;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = ISEPath + "/bin/lin64/netgen";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = ISEPath + "/bin/nt64/netgen.exe";
        }
        else
        {
            command = ISEPath + "/bin/lin64/ngc2edif";
        }

        string args = $"-sim -ism -ofmt verilog {(overwrite ? "-w" : "")} " +
                      $"{(includeTestBench ? "-tb" : "")} " +
                      $"{(flattenOutput ? "-fn" : "")} " +
                      $"{(dontEscapeName ? "-ne" : "")} " +
                      $"-insert_glbl {(insertGlobalModule ? "true" : "false")} " +
                      $"{ngdPath} {netgenPath}";

        string workDir = Path.GetDirectoryName(ngdPath);
        
        //Console.WriteLine(args);
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new Process { StartInfo = startInfo };
        process.Start();
        //using StreamReader reader = process.StandardOutput;
        await process.WaitForExitAsync();
        //Console.WriteLine(await reader.ReadToEndAsync());
        return process.ExitCode == 0;
    }

    public async Task<bool> NgdBuild(string ngcFile)
    {
        string workDir = Path.GetDirectoryName(ngcFile) ?? String.Empty;
        string command;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = ISEPath + "/bin/lin64/ngdbuild";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = ISEPath + "/bin/nt64/ngdbuild.exe";
        }
        else
        {
            command = ISEPath + "/bin/lin64/ngdbuild";
        }

        string outputFileName = Path.GetFileNameWithoutExtension(ngcFile) + ".ngd";
        string outputFilePath = Path.Combine(workDir, outputFileName);
        string args = $"-sd {workDir} {ngcFile} {outputFilePath}";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using Process process = new Process { StartInfo = startInfo };
        process.Start();
        using StreamReader reader = process.StandardOutput;
        await process.WaitForExitAsync();
        Console.WriteLine(await reader.ReadToEndAsync());
        return process.ExitCode == 0;
    }
}