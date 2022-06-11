
<# 
    .SYNOPSIS
    开启一个多线程任务

    .Parameter ScriptBlock
    每个线程要执行脚本块

    .Parameter PassedVariable
    每个线程从主线程那里获取到的变量

    .Example
    $manager = New-ThreadJobManager -Count 2 "MyThreadJobs"

    Start-ThreadJob -Manager $manager -ScriptBlock { Write-Host $PassedVariable } -PassedVariable "Hello World"

    Wait-ThreadJob $manager
    # 会输出两次:Hello World
#>
function Start-ThreadJob{
    Param(
        [Parameter(Mandatory=$true)]
        [EightLeggedEssay.WorkerManager]
        $Manager,
        
        [Parameter(Mandatory=$true)]
        [ScriptBlock]
        $ScriptBlock,

        [Parameter(Mandatory=$false)]
        [object]
        $PassedVariable = $null)

    $strs = [System.Text.StringBuilder]::new()
    $stacks = [object[]](Get-PSCallstack)

    foreach($item in $stacks){
        $strs.Append($item.ToString())
        $strs.Append("`n")
    }

    Start-PriThreadJob -Manager $Manager -ScriptBlock $ScriptBlock -CallStack ($strs.ToString()) -PassedVariable $PassedVariable
}


<# 
    .SYNOPSIS
    一个辅助函数来一次性从多个线程执行脚本块并获取结果

    .Parameter ScriptBlock
    每个线程要执行的脚本块

    .Parameter PassedVariable
    每个线程的脚本块可以获取到的变量

    .Parameter Count
    有多少线程可供使用

    .Example
    Invoke-ParallelScriptBlock -Count 2 -ScriptBlock {Write-Host $PassedVariable} $PassedVariable "Hello World"
    # 将会输出两次:Hello World
#>
function Invoke-ParallelScriptBlock{
    [OutputType([object[]])]
    Param(
        # 使用0将会使用所有cpu
        # 见C#的Start-PriThreadJob实现
        [Parameter(Mandatory=$false)]
        [long]
        $Count = 0,

        [Parameter(Mandatory=$true)]
        [ScriptBlock]
        $ScriptBlock,

        [Parameter(Mandatory=$false)]
        [object]
        $PassedVariable = $null)

    $manager = New-ThreadJobManager -Count $Count "ParallelScriptBlock"

    Start-ThreadJob -Manager $manager -ScriptBlock $ScriptBlock -PassedVariable $PassedVariable
   
    return Wait-ThreadJob $manager
}

Export-ModuleMember -Function "Start-ThreadJob"
Export-ModuleMember -Function "Invoke-ParallelScriptBlock"
