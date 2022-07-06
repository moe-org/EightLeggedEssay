
. "$PSScriptRoot/Thread.ps1"
. "$PSScriptRoot/Utility.ps1"
. "$PSScriptRoot/Rss.ps1"

<#
    .SYNOPSIS
    对markdown进行编译，附带增量编译功能。

    .Parameter SourcePath
    源文件路径

    .Parameter OutputPath
    输出文件路径

    .Parameter NoIncrementalCompilation
    设置此选项来关闭增量编译

    .Parameter EnableAdvancedExpansion
    设置此选项来开启markdown高级扩展.高级扩展见:https://github.com/xoofx/markdig
  
    .Example
    Convert-MarkdownPoster -SourcePath "./content/hello world.md" -OutputPath "./content/output.binary"
#>
function Convert-MarkdownPoster{
    [OutputType([EightLeggedEssay.Poster])]
    Param(
        [Parameter(Mandatory=$true)]
        [System.IO.FileInfo]
        $SourcePath,

        [Parameter(Mandatory=$true)]
        [System.IO.FileInfo]
        $OutputPath,

        [Parameter(Mandatory=$false)]
        [switch]
        $NoIncrementalCompilation,

        [Parameter(Mandatory=$false)]
        [switch]
        $EnableAdvancedExpansion)
    
    # 增量编译检查
    # 输出文件存在，源文件修改时间早于输出文件时间，开启增量编译
    if((Test-Path $OutputPath) -and ($SourcePath.LastWriteTime.CompareTo($OutputPath.LastWriteTime) -le 0) -and -not $NoIncrementalCompilation.IsPresent){
        return [EightLeggedEssay.Poster]::Parse(([System.IO.File]::ReadAllBytes(($OutputPath.FullName))))
    }

    # 编译
    $parsed = Convert-PriMarkdownPoster -Source (Get-Content -Raw -Path $SourcePath)

    $compiled = $null

    if($EnableAdvancedExpansion.IsPresent){
        $compiled = Convert-Markdown -Source $parsed.Markdown -EnableAdvancedExpansion
    }
    else{
        $compiled = Convert-Markdown -Source $parsed.Markdown
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

<#
    .SYNOPSIS
    转换Scriban模板

    .Parameter TemplateFile
    模板文件

    .Parameter OutputFile
    输出文件

    .Parameter Attributes
    转换中使用的属性

#>
function Convert-ScribanTemplate{
    Param(
        [Parameter(Mandatory=$true)]
        [System.IO.FileInfo]
        $TemplateFile,

        [Parameter(Mandatory=$true)]
        [System.IO.FileInfo]
        $OutputFile,

        [Parameter(Mandatory=$false)]
        [hashtable]
        $Attributes = @{})

    New-ParentDirectories -Path $OutputFile

    Get-Content -Path $TemplateFile -Raw | 
    Convert-Scriban -Property ($Attributes | Get-ScribanTable) -IncludePath $TemplateFile.DirectoryName | 
    Out-File -LiteralPath $OutputFile -Encoding "UTF-8" -NoNewline
}

<#
    .SYNOPSIS
    给文章数组分类

    .Parameter PostersPerPage
    每一页可以分配的文章数量

    .Parameter Posters
    所有的文章

    .Outputs
    将会输出一个List<hashtable>，List的每一项代表一个页面，按顺序排序。hashtable内容为：
    [long]CurrentPageNumber:当前页面是第几页。这个值从1开始计算。
    [long]PostersIndexStart:当前页面的第一篇文章在所有文章中索引。
    [long]PosterCount:当前页面一共有多少文章。
    [array]Posters:当前页面的文章。是一个数组。
    [bool]IsFirstPage:当前页面是否是第一页。
    [bool]IsLastPage:当前页面是否是最后一页。

    .NOTES
    文章参数中的Posters是一个数组，但数组的内容不一定得是[Poster]类型的，理论上任何类型的都可以进行分页。
    因为此和文章内容无关。 :-)
#>
function Convert-Paginations{
    [OutputType([System.Collections.Generic.List[hashtable]])]
    Param(
        [Parameter(Mandatory=$true)]
        [long]
        $PostersPerPage,
        
        [Parameter(Mandatory=$true)]
        [array]
        $Posters)

    # 分页
    $pages = [System.Collections.Generic.List[hashtable]]::new()

    $pageIndex = [long]1
    $allocedPosters = [long]0
    $restPosts = [long]$Posters.LongLength

    $isFirstPage = [bool]$true
    $isLastPage = [bool]$false

    while($restPosts -ne 0){

        $alloc = $PostersPerPage

        if($alloc -ge $restPosts){
            # 此次分配将耗尽最后的文章
            $alloc =  $restPosts
            $isLastPage = $true
        }

        $data = @{
            CurrentPageNumber = $pageIndex
            PostersIndexStart = $allocedPosters
            PosterCount = $alloc
            Posters = $Posters[$allocedPosters..($allocedPosters + $alloc - 1)]
            IsFirstPage = $isFirstPage
            IsLastPage = $isLastPage
        }

        $pages.Add($data)

        $restPosts -= $alloc
        $allocedPosters += $alloc
        $pageIndex += 1
        $isFirstPage = $false
    }

    return $pages
}

Export-ModuleMember -Function "Convert-MarkdownPoster"
Export-ModuleMember -Function "Convert-ScribanTemplate"
Export-ModuleMember -Function "Convert-Paginations"
