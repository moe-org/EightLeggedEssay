
<# 
 .SYNOPSIS
 start threaded task

 .Parameter ScriptBlock
 the script block that you want to execute

 .Parameter PassedVariable
 the variable that will be passed to the scripg block

 .Example
 $manager = New-ThreadJobManager -Count 2 "MyThreadJobs"

 Start-ThreadJob -Manager $manager -ScriptBlock { Write-Host $PassedVariable } -PassedVariable "Hello World"

 Wait-ThreadJob $manager
 # will output twice:Hello World
#>
function Start-ThreadJob{
    Param([EightLeggedEssay.ThreadWorker.WorkerManager]$Manager,[ScriptBlock]$ScriptBlock,[object]$PassedVariable)

    $strs = [System.Text.StringBuilder]::new()
    $stacks = [object[]](Get-PSCallstack)

    foreach($item in $($stacks[1..-1])){
        $strs.Append($item.ToString())
        $strs.Append("`n")
    }

    Start-PriThreadJob -Manager $Manager -ScriptBlock $ScriptBlock -CallStack ($strs.ToString()) -PassedVariable $PassedVariable
}


<# 
 .SYNOPSIS
 a helper function to execute script block parallel

 .Parameter ScriptBlock
 the script block that you want to execute

 .Parameter PassedVariable
 the variable that will be passed to the scripg block

 .Parameter Count
 how many cpu that you want to use

 .Example
 Invoke-ParallelScriptBlock -Count 1 -ScriptBlock {Write-Host $PassedVariable} $PassedVariable "Hello World"
 # will output twice:Hello World
#>
function Invoke-ParallelScriptBlock{
    Param([long]$Count,[ScriptBlock]$ScriptBlock,[object]$PassedVariable)

    $manager = New-ThreadJobManager -Count $Count "ParallelScriptBlock"

    Start-ThreadJob -Manager $manager -ScriptBlock $ScriptBlock -PassedVariable $PassedVariable
   
    return Wait-ThreadJob $manager
}


function Compile-MarkdownPoster{
    [OutputType([EightLeggedEssay.Poster])]
    Param([System.IO.FileInfo]$SourcePath,[System.IO.FileInfo]$OutputPath,[switch]$NoIncrementalCompilation,[switch]$EnableAdvancedExpansion)
    
    # 检查访问时间
    if((Test-Path $OutputPath) -and ($SourcePath.LastWriteTime -gt $OutputPath.LastWriteTime) -and -not $NoIncrementalCompilation.IsPresent){
        # 不需要再次编译，直接返回
        return [EightLeggedEssay.Poster]::Parse(([System.IO.File]::ReadAllBytes(($OutputPath.FullName))))
    }

    # 编译
    $parsed = Compile-PriMarkdownPoster -Source (Get-Content -Raw -Path $SourcePath)

    $compiled = $null

    if($EnableAdvancedExpansion.IsPresent){
        $compiled = Compile-Markdown -Source $parsed.Markdown -EnableAdvancedExpansion
    }
    else{
        $compiled = Compile-Markdown -Source $parsed.Markdown
    }

    return [EightLeggedEssay.Poster]::Create(
        $compiled,
        $parsed.Head.Title,
        $parsed.Head.CreateTime,
        $parsed.Head.Strict,
        $parsed.Head.Attributes,
        $SourcePath.FullName,
        $OutputPath.FullName)
}

Export-ModuleMember -Function "Start-ThreadJob"
Export-ModuleMember -Function "Invoke-ParallelScriptBlock"
Export-ModuleMember -Function "Compile-MarkdownPoster"
