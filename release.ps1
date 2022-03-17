if ($null -eq $args[0]) {
    Write-Output "Usage:`n> $$ ""filename.cs"""
}
else {
    $filename = [System.IO.Path]::GetFileNameWithoutExtension($args[0])
    Write-Output "Parsing ""$filename"""
    Get-Content $args[0] | Out-String | ForEach-Object { [Regex]::Matches($_, "(?<=// \*\* START OF SE CODE \*\* //)((.|\n)*?)(?=// \*\* END OF SE CODE \*\* //)") } | ForEach-Object { $_.Value } > $filename.temp.cs
    Write-Output "Wrote to: $filename.temp.cs"
    Write-Output "Minifying: $filename.temp.cs"
    .\csharp-min\CSharpMinifierConsole.exe min .\$filename.temp.cs > $filename.min.cs
    Write-Output "Minified $filename.min.cs"
}

