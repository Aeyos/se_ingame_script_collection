if ($null -eq $args[0]) {
    Write-Output "Usage:`n> $$ ""filename.cs"""
}
else {
    $file_target = Get-Item $args[0]
    $filename_noextension = "$($file_target.DirectoryName)\$($file_target.BaseName)"
    $filename_temp = "$($filename_noextension).temp.cs"
    $filename_compiled = "$($filename_noextension).min.cs"
    $filename_header = "$($filename_noextension)_Header.cs"
    $has_header = Test-Path -Path $filename_header -PathType Leaf

    if (Test-Path $filename_temp) {
        Remove-Item $filename_temp
    }
    if (Test-Path $filename_compiled) {
        Remove-Item $filename_compiled
    }

    Write-Output "Parsing ""$($args[0])"""
    
    # GET FILE CONTENT, CROP, OUTPUT TO TEMP
    Get-Content $args[0] | Out-String | ForEach-Object { [Regex]::Matches($_, "(?<=// \*\* START OF SE CODE \*\* //)((.|\n)*?)(?=// \*\* END OF SE CODE \*\* //)") } | ForEach-Object { $_.Value } > "$filename_temp"
    Write-Output "Wrote to: $filename_temp"

    # APPEND HEADER
    if ($has_header) {
        Write-Output "$filename_header detected, appending content to file"
        Get-Content $filename_header | Out-String | ForEach-Object { [Regex]::Matches($_, "(?<=// \*\* START OF SE CODE \*\* //)((.|\n)*?)(?=// \*\* END OF SE CODE \*\* //)") } | ForEach-Object { $_.Value } >> $filename_compiled
    }
    Write-Output "Minifying: $($args[0])"
    .\csharp-min\CSharpMinifierConsole.exe min $filename_temp >> "$filename_compiled"
    Write-Output "Minified $filename_compiled"
}

