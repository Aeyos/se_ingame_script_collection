if ($null -eq $args[0]) {
    Write-Output "Usage:`n> $$ ""filename.cs"""
}
else {
    Get-Content $args[0] | Out-String | ForEach-Object { [Regex]::Matches($_, "(?<=// \*\* START OF SE CODE \*\* //)((.|\n)*?)(?=// \*\* END OF SE CODE \*\* //)") } | ForEach-Object { $_.Value } | Set-Clipboard
}