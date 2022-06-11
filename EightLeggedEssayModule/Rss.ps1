
<#
    .SYNOPSIS
    获取一个新的RSS对象。使用Rss 2.0标准

    .Parameter Title
    对应channel中的title。此为rss必选属性。

    .Parameter Link
    对应channel中的link。此为rss必选属性。

    .Parameter Description
    对应channel中的description。此为rss必选属性。

    .Example
    $rss = New-Rss -Title "This is Title" -Link "http://localhost/" -Description "Test Rss"

    # we can also access rss object directly
    $rss.Auother = "me@kawayi.moe (MingMoe)"

    $rssPoster = Add-RssPoster -Rss $rss -Poster $poster

    # rss item can access directly, too
    $rssPoster.Auother = "me@kawayi.moe (MingMoe)"

    # print result
    Wirte-Host (rss.GetString())

#>
function New-Rss{
    [OutputType([EightLeggedEssay.Rss])]
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $Title,

        [Parameter(Mandatory=$true)]
        $Link,

        [Parameter(Mandatory=$true)]
        [string]
        $Description
    )
    return [EightLeggedEssay.Rss]::new($Title,[System.Uri]::new($Link),$Description)
}

<#
    .SYNOPSIS 
    添加一篇文章到rss。返回添加的文章。这是一个helper函数。

    .Parameter Rss
    要添加到的Rss

    .Parameter Poster
    要使用的文章。将会使用文章的Title和CreateTime填充信息。

    .Parameter Url
    可选项，文章的url地址。对应rss item中的link元素。

    .Example
    $rss = New-Rss -Title "This is Title" -Link "http://localhost/" -Description "Test Rss"

    # we can also access rss object directly
    $rss.Auother = "me@kawayi.moe (MingMoe)"

    $rssPoster = Add-RssPoster -Rss $rss -Poster $poster

    # rss item can access directly, too
    $rssPoster.Auother = "me@kawayi.moe (MingMoe)"

    # print result
    Wirte-Host (rss.GetString())
#>
function Add-RssPoster{
    [OutputType([EightLeggedEssay.RssItem])]
    Param(
        [Parameter(Mandatory=$true,ValueFromPipeline = $true)]
        $Rss,

        [Parameter(Mandatory=$true,ValueFromPipeline = $true)]
        $Poster,

        [Parameter(Mandatory=$false)]
        $Url = $null
    )

    $item = [EightLeggedEssay.RssItem]::new()

    $item.Title = $Poster.Title
    $item.PublishTime = $Poster.CreateTime
    $item.Link = $Url

    $Rss.Items.Add($item)

    return $item
}


Export-ModuleMember -Function "New-Rss"
Export-ModuleMember -Function "Add-RssPoster"
