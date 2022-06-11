
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

Export-ModuleMember -Function "Convert-URL"
Export-ModuleMember -Function "ConvertTo-ThreadSafeHashtable"
Export-ModuleMember -Function "ConvertTo-RedirectPath"
