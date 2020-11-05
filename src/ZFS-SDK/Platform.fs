module Platform

open System

open Infrastructure

open System.Runtime.InteropServices
[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )>]
extern uint16 GetShortPathName
    ( string lpszLongPath
    , Text.StringBuilder lpszShortPath
    , uint16 cchBuffer
    )



let platform : PlatformID =
    if begin
        true
        && Environment.OSVersion.Platform = PlatformID.Unix
        && IO.Directory.Exists "/Applications"
        && IO.Directory.Exists "/System"
        && IO.Directory.Exists "/Users"
        && IO.Directory.Exists "/Volumes"
        end
    then PlatformID.MacOSX
    else Environment.OSVersion.Platform


let isUnix : bool =
    match platform with
    | PlatformID.Unix
    | PlatformID.MacOSX ->
        true
    | _ ->
        false


let normalizeNameToFileSystem (filename : string) : string =
    if isUnix then
        filename
    else
        let bufferSize = uint16 256
        let shortNameBuffer = Text.StringBuilder(int bufferSize)
        GetShortPathName(filename, shortNameBuffer, bufferSize) |> ignore
        shortNameBuffer.ToString()


let workingDirectory : string =
    IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    |> normalizeNameToFileSystem


let runNative (exe : String) (args : seq<string>) : Result<unit , string> =
    
    let proc =
        new Diagnostics.Process();
    
    proc.StartInfo <-
        Diagnostics.ProcessStartInfo
            ( FileName               = exe
            , Arguments              = String.concat " " args
            , WorkingDirectory       = workingDirectory
            , RedirectStandardOutput = true
            , RedirectStandardError  = true
            , UseShellExecute        = false
            )
    
    let appender (string_builder : Text.StringBuilder) (args : Diagnostics.DataReceivedEventArgs) : unit =
        if not <| String.IsNullOrWhiteSpace args.Data then
            if string_builder.Length <> 0 then
                string_builder.Append Environment.NewLine |> ignore
            string_builder.Append args.Data |> ignore
    
    let error =
        Text.StringBuilder()
    
    proc.ErrorDataReceived.Add(appender error)
    
    let output =
        Text.StringBuilder()
    
    proc.OutputDataReceived.Add(appender output)
    
    try
        if proc.Start() then
            proc.BeginErrorReadLine()
            proc.BeginOutputReadLine()
            proc.WaitForExit()
            let output =
                output.ToString()
            if output.Length > 0 then
                printfn "%s" output
            let error =
                error.ToString()
            if error.Length > 0 then
                eprintfn "%s" error
            if proc.ExitCode = 0 then
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
            List.tryFind IO.File.Exists //TODO: prioritize
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