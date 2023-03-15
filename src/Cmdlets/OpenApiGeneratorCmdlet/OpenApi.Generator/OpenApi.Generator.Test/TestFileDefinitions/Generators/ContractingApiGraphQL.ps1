function Format-Json {
    <#
    .SYNOPSIS
        Prettifies JSON output.
    .DESCRIPTION
        Reformats a JSON string so the output looks better than what ConvertTo-Json outputs.
    .PARAMETER Json
        Required: [string] The JSON text to prettify.
    .PARAMETER Minify
        Optional: Returns the json string compressed.
    .PARAMETER Indentation
        Optional: The number of spaces (1..1024) to use for indentation. Defaults to 4.
    .PARAMETER AsArray
        Optional: If set, the output will be in the form of a string array, otherwise a single string is output.
    .EXAMPLE
        $json | ConvertTo-Json  | Format-Json -Indentation 2
    #>
    [CmdletBinding(DefaultParameterSetName = 'Prettify')]
    Param(
        [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true)]
        [string]$Json,

        [Parameter(ParameterSetName = 'Minify')]
        [switch]$Minify,

        [Parameter(ParameterSetName = 'Prettify')]
        [ValidateRange(1, 1024)]
        [int]$Indentation = 4,

        [Parameter(ParameterSetName = 'Prettify')]
        [switch]$AsArray
    )

    if ($PSCmdlet.ParameterSetName -eq 'Minify') {
        return ($Json | ConvertFrom-Json) | ConvertTo-Json -Depth 100 -Compress
    }

    # If the input JSON text has been created with ConvertTo-Json -Compress
    # then we first need to reconvert it without compression
    if ($Json -notmatch '\r?\n') {
        $Json = ($Json | ConvertFrom-Json) | ConvertTo-Json -Depth 100
    }

    $indent = 0
    $regexUnlessQuoted = '(?=([^"]*"[^"]*")*[^"]*$)'

    $result = $Json -split '\r?\n' |
        ForEach-Object {
            # If the line contains a ] or } character, 
            # we need to decrement the indentation level unless it is inside quotes.
            if ($_ -match "[}\]]$regexUnlessQuoted") {
                $indent = [Math]::Max($indent - $Indentation, 0)
            }

            # Replace all colon-space combinations by ": " unless it is inside quotes.
            $line = (' ' * $indent) + ($_.TrimStart() -replace ":\s+$regexUnlessQuoted", ': ')

            # If the line contains a [ or { character, 
            # we need to increment the indentation level unless it is inside quotes.
            if ($_ -match "[\{\[]$regexUnlessQuoted") {
                $indent += $Indentation
            }

            $line
        }

    if ($AsArray) { return $result }
    return $result -Join [Environment]::NewLine
}


try
{
    "Get openapi.json documentation..."
    $swagger = Invoke-WebRequest -Uri "https://contracting-api-graphql-test.azurewebsites.net/swagger/v1/swagger.json" -Headers @{"Accept"="application/json,*/*"; "Accept-Encoding"="gzip, deflate, br"} -ContentType "application/json" -UseBasicParsing
    $swaggerJson20 = $swagger.Content -replace "odpcustomerID", "_odpcustomerID" -replace "odpcompanyID", "_odpcompanyID"
    $swaggerJson = $swaggerJson20 | ConvertFrom-Json
    if($swaggerJson.openapi -lt "3")
    {
        "Saving received specification"
        $swaggerJson20 |Format-Json | Out-File ../swaggerGraphQl.json -Encoding ASCII -Force

        "Specification received was v2, converting to v3"
        $swagger3 = Invoke-WebRequest -Uri "https://converter.swagger.io/api/convert" -Method "POST" -Headers @{"accept"="application/json"; "Accept-Encoding"="gzip, deflate, br"; } -ContentType "application/json" -Body ([System.Text.Encoding]::UTF8.GetBytes($swaggerJson20)) -UseBasicParsing
        $swaggerJson30 = $swagger3.Content
    }
    else
    {
        "Specification received was v3, no conversion"
        $swaggerJson30 = $swagger.Content
	}
}
catch
{
	$err=$_.Exception
    $err
    return
}

"Save documentation..."
$swaggerJson30 |Format-Json | Out-File ../openApiGraphQL30.json -Encoding ASCII -Force
