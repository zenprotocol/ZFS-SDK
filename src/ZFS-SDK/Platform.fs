module Platform

open System.IO
open System.Text
open System.Diagnostics
open System

open Infrastructure

open System.Runtime.InteropServices
[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )>]
extern uint16 GetShortPathName(
    string lpszLongPath,
    StringBuilder lpszShortPath,
    uint16 cchBuffer)



let platform =
    if (Environment.OSVersion.Platform = PlatformID.Unix
        && Directory.Exists "/Applications"
        && Directory.Exists "/System"
        && Directory.Exists "/Users"
        && Directory.Exists "/Volumes")
            then PlatformID.MacOSX
    else Environment.OSVersion.Platform


let isUnix =
    match platform with
    | PlatformID.Unix
    | PlatformID.MacOSX ->
        true
    | _ ->
        false


let normalizeNameToFileSystem =
    if isUnix then
        id
    else
        fun fileName ->
            let bufferSize = uint16 256
            let shortNameBuffer = StringBuilder((int)bufferSize)
            GetShortPathName(fileName, shortNameBuffer, bufferSize) |> ignore
            shortNameBuffer.ToString()


let workingDirectory =
    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    |> normalizeNameToFileSystem


let runNative exe args =
    
    let p =
        new Process();
    
    p.StartInfo <-
        ProcessStartInfo(
            FileName = exe,
            Arguments = String.concat " " args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false)
    
    let appender (sb : StringBuilder) =
        let (+) (sb : StringBuilder) (s : string) = sb.Append s |> ignore
        fun (args : DataReceivedEventArgs) ->
            let data = args.Data
            if not (String.IsNullOrWhiteSpace data) then
                if sb.Length <> 0 then
                    sb + Environment.NewLine
                sb + data
    
    let error =
        StringBuilder()
    
    p.ErrorDataReceived.Add(appender error)
    
    let output =
        StringBuilder()
    
    p.OutputDataReceived.Add(appender output)
    
    try
        if p.Start() then
            p.BeginErrorReadLine()
            p.BeginOutputReadLine()
            p.WaitForExit()
            let output =
                output.ToString()
            if output.Length > 0 then
                printfn "%s" output
            let error =
                error.ToString()
            if error.Length > 0 then
                eprintfn "%s" error
            if p.ExitCode = 0 then
                Ok ()
            else
                Error (if String.IsNullOrEmpty error then output else error)
        else
            Error "failed to start process"
    with _ as ex ->
        Exception.toError "run" ex


let run exe args =
    
    let monoM =
        if isUnix then
            List.tryFind File.Exists //TODO: prioritize
                ["/usr/bin/mono"
                 "/usr/local/bin/mono"
                 "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono"]
        else
            Some "unused"

    let mono =
        match monoM with
        | Some mono -> mono
        | _ -> "mono"
    
    let exe, args =
        if isUnix then mono, exe :: args else exe, args
    
    runNative exe args