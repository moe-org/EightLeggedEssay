
<#
    .SYNOPSIS 
    URL转换

    .Parameter RootUrl
    顶级URL，输出将会基于此URL进行输出。默认是(Get-EleVariable -Name RootUrl)的结果

    .Parameter RelativeTo
    顶级目录

    .Parameter Target
    目标文件

    .Example
    Convert-URL -RootUrl "https://github.com/blog" -RelativeTo "/a/" -Target "/a/b/c"

    # 结果是https://github.com/blog/b/c
#>
function Convert-URL{
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $RootUrl,

        [Parameter(Mandatory=$true)]
        [string]
        $RelativeTo,

        [Parameter(Mandatory=$true)]
        [string]
        $Target)

    
    $rePath = [System.IO.Path]::GetRelativePath([System.IO.Path]::GetFullPath($RelativeTo),[System.IO.Path]::GetFullPath($Target))

    $builder = [System.UriBuilder]::new($RootUrl)

    $builder.Path += "/" + $rePath

    return $builder.ToString()
}


<#
    .SYNOPSIS
    重定向路径。将源目录相对于父目录的地址转换到另一个父目录。
    即从root/parent/some/children转换到anotherRoot/parent/some/children。

    .Parameter OriginPath
    源路径

    .Parameter OriginDirectory 
    源路径的父目录

    .Parameter TargetDirectory
    目标目录

    .Example
    ConvertTo-RedirectPath -OriginPath \c\d\file -OriginDirectory \c\ -TargetDirectory \e\f\

    # 结果:\e\f\d\file
#>
function ConvertTo-RedirectPath{
    [OutputType([string])]
    Param(
        [Parameter(Mandatory=$true,Position=0)]
        [string]
        $OriginPath,

        [Parameter(Mandatory=$true,Position=1)]
        [string]
        $OriginDirectory,

        [Parameter(Mandatory=$true,Position=2)]
        [string]
        $TargetDirectory)

    $rePath = [System.IO.Path]::GetRelativePath($OriginDirectory,$OriginPath)
    
    return [System.IO.Path]::Join($TargetDirectory,$rePath)
}

<#
    .SYNOPSIS
    对hashtable进行转换

    .Parameter InputObject
    要进行转换的对象。如果为空，则创建一个新的同步对象。

#>
function ConvertTo-ThreadSafeHashtable{
    [OutputType([hashtable])]
    Param(
        [Parameter(Mandatory=$false,Position = 0)]
        [hashtable]
        $InputObject = @{}
    )
    return [hashtable]::Synchronized($InputObject)
}

<#
    .SYNOPSIS
    对markdown文章进行编译的便捷工具函数。
    这个函数将会递归搜索文件夹下所有的文件，使用多线程编译，通过增量编译输出到另一个文件夹。

    .Parameter Path
    输入的文件的目录

    .Parameter OutPath
    输出的文件的目录
#>
function Convert-MarkdownPosterHelper{
    [OutputType([EightLeggedEssay.Poster[]])]
    Param(
        [Parameter(Mandatory = $true)]
        [System.IO.DirectoryInfo]
        $Path,

        [Parameter(Mandatory = $true)]
        [System.IO.DirectoryInfo]
        $OutPath
    )

    $result = [System.Collections.Concurrent.ConcurrentBag[EightLeggedEssay.Poster]]::new()

    $data = ConvertTo-ThreadSafeHashtable @{
        Posters = [System.Collections.Concurrent.ConcurrentBag[object]]::new()
        Result = $result
        OriginPath = $Path
        OutPath = $OutPath
    }

    foreach($item in (Get-ChildItem -Path $Path -Recurse -File)){
        $data["Posters"].Add($item)
    }

    $_ = Invoke-ParallelScriptBlock -ScriptBlock {
        $poster = $null
        $OutPath = $PassedVariable["OutPath"]
        $OriginPath = $PassedVariable["OriginPath"]
        $result = $PassedVariable["Result"]

        while($PassedVariable["Posters"].TryTake([ref] $poster)){
            $outputPath = ConvertTo-RedirectPath $poster $OriginPath $OutPath

            $outputPath += ".compiled"

            $compiled = Convert-MarkdownPoster -SourcePath $poster -OutputPath $outputPath

            $result.Add($compiled)
        }

    } -PassedVariable $data

    $result
}

<#
    .SYNOPSIS
    用于获取下一个页面名称的辅助函数。如果当前页面是最后一个页面则返回null。
    默认格式如：
    index.html      - 首页
    2_index.html    - 第二页
    3_index.html    - 第三页...

    .Parameter page
    一个包括页面信息的hashtable。要求有以下数据：
    IsLastPage: 当前页面是否是最后一个页面
    IsFirstPage: 当前页面是否是第一个页面
    CurrentPageNumber: 当前页面的页码
    通常这些数据已经包含在Convert-Paginations的返回值中。

    .Parameter ScriptBlock
    用于目标索引字符串的函数。参数为一个long类型的页面索引，返回值为字符串。
    默认返回$index + "_index.html"。
    注意，参数为“下一页”的索引值，如输入的Page的页面是第三页，那么函数将会获取第四页的索引值（即数字4）
#>
function Get-NextPageHelper{
    [OutputType([string])]
    Param(
        [Parameter(Mandatory = $true,Position = 0,ValueFromPipeline = $true)]
        [hashtable]
        $Page,

        [Parameter(Mandatory = $false)]
        [scriptblock]
        $ScriptBlock = {
            [OutputType([string])]
            param([long]$index)
            return ($index.ToString() + "_index.html")
        }
    )
    
    if(-not $Page["IsLastPage"]){
        return $ScriptBlock.Invoke($Page["CurrentPageNumber"] + 1)
    }
    else{
        return $null
    }
}

<#
    .SYNOPSIS
    用于获取上一个页面名称的辅助函数。如果当前页面是第一个页面则返回null。
    默认格式如：
    index.html      - 首页
    2_index.html    - 第二页
    3_index.html    - 第三页...

    .Parameter Page
    一个包括页面信息的hashtable。要求有以下数据：
    IsLastPage: 当前页面是否是最后一个页面
    IsFirstPage: 当前页面是否是第一个页面
    CurrentPageNumber: 当前页面的页码
    通常这些数据已经包含在Convert-Paginations的返回值中。

    .Parameter FirstPageIndex
    第一页的索引。
    默认为index.html。

    .Parameter ScriptBlock
    用于获取目标索引字符串的函数，参数为一个long类型的页面索引，返回值为字符串。
    默认返回$index + "_index.html"。
    注意，参数为“上一页”的索引值，如输入的Page的页面是第三页，那么函数将会获取第二页的索引值（即数字2）
#>
function Get-PreviousPageHelper{
    [OutputType([string])]
    Param(
        [Parameter(Mandatory = $true,Position = 0,ValueFromPipeline = $true)]
        [hashtable]
        $Page,

        [Parameter(Mandatory = $false)]
        [string]
        $FirstPageIndex = "index.html",

        [Parameter(Mandatory = $false)]
        [scriptblock]
        $ScriptBlock = {
            [OutputType([string])]
            param([long]$index)
            return ($index.ToString() + "_index.html")
        }
    )

    if(-not $Page["IsFirstPage"]){
        if(($Page["CurrentPageNumber"] - 1) -eq 1){
            return $FirstPageIndex
        }
        else{
            return $ScriptBlock.Invoke($Page["CurrentPageNumber"] - 1)
        }
    }
    else{
        return $null
    }
}

<#
    .SYNOPSIS
    这个函数用于获取当前页面的字符串索引。格式如
    index.html
    2_index.html
    3_index.html

    .Parameter Page
    一个包括页面信息的hashtable。要求有以下数据：
    IsLastPage: 当前页面是否是最后一个页面
    IsFirstPage: 当前页面是否是第一个页面
    CurrentPageNumber: 当前页面的页码
    通常这些数据已经包含在Convert-Paginations的返回值中。

    .Parameter FirstPageIndex
    第一个页面的索引值，默认为index.html

    .Parameter ScriptBlock
    用于获取目标索引字符串的函数，参数为一个long类型的页面索引，返回值为字符串。
    默认返回$index + "_index.html"。
#>
function Get-CurrentPageHelper{
    [OutputType([string])]
    Param(
        [Parameter(Mandatory = $true,Position = 0,ValueFromPipeline = $true)]
        [hashtable]
        $Page,

        [Parameter(Mandatory = $false)]
        [string]
        $FirstPageIndex = "index.html",

        [Parameter(Mandatory = $false)]
        [scriptblock]
        $ScriptBlock = {
            [OutputType([string])]
            param([long]$index)
            return ($index.ToString() + "_index.html")
        }
    )
    if($Page["IsFirstPage"]){
        return $FirstPageIndex
    }
    else{
        return $ScriptBlock.Invoke($Page["CurrentPageNumber"])
    }
}

<#
    .SYNOPSIS
    创建一个新的html checker。checker不是线程安全的。

    .PARAMETER CheckerItems
    要添加的检查器的类型名称列表
#>
function New-HtmlChecker{
    [OutputType([EightLeggedEssay.Html.IHtmlChecker])]
    Param(
        [string[]]
        [Parameter(Mandatory = $true,ValueFromPipeline = $true)]
        $CheckerItems
    )

    $types = [System.Collections.Generic.List[EightLeggedEssay.Html.IHtmlCheckerItem]]::new()

    foreach($item in $CheckerItems){
        $types.Add([System.Activator]::CreateInstance($null,$item,$false,0,$null,$null,$null,$null).Unwrap())
    }

    return [EightLeggedEssay.Html.HtmlChecker]::new($types)
}

<#
    .SYNOPSIS
    测试html文章，同时把错误输出给用户

    .PARAMETER Posters
    要测试的文章

    .PARAMETER Checker
    要使用的检查器

#>
function Test-HtmlPostersHelper{
    Param(
        [array]
        [Parameter(Mandatory = $true,ValueFromPipeline = $true)]
        $Posters,

        [EightLeggedEssay.Html.IHtmlChecker]
        [Parameter(Mandatory = $false, ValueFromPipeline = $false)]
        $Checker = [EightLeggedEssay.Html.HtmlChecker]::new()
    )

    foreach($post in $Posters){
        $errors = $null;

        if(([EightLeggedEssay.Html.IHtmlChecker]$Checker).TryGetError($post.Text,([ref] $errors))){
            Write-Host -Object ("found html post error at " + ($post.SourcePath)) -ForegroundColor Red

            foreach($err in $errors){
                Write-Host -Object $err -ForegroundColor Red
            }
        }
    }
}

<#
    .SYNOPSIS
    创建一个文件的父目录（如果不存在的话）

    .PARAMETER Path
    文件的路径
#>
function New-ParentDirectories{
    Param(
        [Parameter(Mandatory = $true,ValueFromPipeline = $true,Position = 1)]
        $Path
    )

    $file = [System.IO.FIleInfo]::new($Path.ToString())
    
    if($null -ne $file.Directory -and $null -ne $file.Directory.FullName){
        if(-not(Test-Path -LiteralPath $file.Directory.FullName -PathType Container)){
            New-Item -ItemType Directory -Force -Path $file.Directory.FullName
        }
    }
}

Export-ModuleMember -Function "Convert-URL"
Export-ModuleMember -Function "ConvertTo-ThreadSafeHashtable"
Export-ModuleMember -Function "ConvertTo-RedirectPath"
Export-ModuleMember -Function "Convert-MarkdownPosterHelper"
Export-ModuleMember -Function "Get-NextPageHelper"
Export-ModuleMember -Function "Get-PreviousPageHelper"
Export-ModuleMember -Function "Get-CurrentPageHelper"
Export-ModuleMember -Function "New-HtmlChecker"
Export-ModuleMember -Function "Test-HtmlPostersHelper"
Export-ModuleMember -Function "New-ParentDirectories"
