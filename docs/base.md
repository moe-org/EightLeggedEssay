## 基础使用
如果你什么都不干，直接运行`EightLeggedEssay`，大概率会得到一个异常，或者类似于`command not found`之类的东西。

这是设计之内的，我们没有计划打印帮助页面，不过你也可以通过`EightLeggedEssay --help`手动调用出来。

画风类似于
```
usage:EightLeggedEssay [--options] -- [command options]
options:
        --server path  :start a http server in path,default in output path
        --config path  :set the path to load config file
        --system path  :set the path of EightLeggedEssay system module
        --repl         :entry the repl mode
        --help         :print help then exit with success
        --debug        :entry debug mode
        --new    path  :create a new site in path then exit
        --run  command :execute a command that defined by configuration file
        the arguments after `--` will send to the `--run command`
```

祝你已经顺利地看到了输出 *:-)*

使用模板创建一个站点来让我们学习的开端更加轻松:
```shell
> EightLeggedEssay --new example
> cd exmplae
> tree
C:.
│  build-EightLeggedEssay.ps1
│  EightLeggedEssay.json
│  new.ps1
│
├─content
├─site
├─source
└─theme
```
好了，你就得到这么多东西！

接下来一个个看吧

 - `build-EightLeggedEssay.ps1`
 - - 这是你的构建脚本，你的工作大部分可能都会围绕这个展开
 - - 不喜欢这个名字？当然可以换一个，我们会在后文提到
 - `EightLeggedEssay.json`
 - - 这是`EightLeggedEssay`的配置文件，你个人使用的配置在这个文件里也有一席之地。
 - - 如果你同样不喜欢这个名字，可以在参数中指定配置文件，看看上面 :-)
 - `new.ps1`
 - - 这个文件是额外的。这个`额外`指的是，你随时可以删除或者添加更多这种文件，只要**在配置文件中指定**就行了。
 - - 比如：你执行了`EightLeggedEssay --run new -- a`那么，`EightLeggedEssay`就会在配置文件中寻找`new`这个命令所映射的`.ps1`文件，然后将后面的参数`a`传递过去（你也可以不传递任何参数，参数的数量也没有上限）
 - `content`
 - `site`
 - `source`
 - `theme`
 - - 这四个文件夹纯属方便之用，不用也行。`EightLeggedEssay`不会用到这些文件夹里的任何东西，里面的任何东西都属于你自己。

让我们先看看`build-EightLeggedEssay.ps1`，好的，他是空的，我们接下去会用到的。

再看看我们的配置文件:
```json
{
  "RootUrl": "",
  "OutputDirectory": "site",
  "BuildScript": "build-EightLeggedEssay.ps1",
  "ContentDirectory": "content",
  "SourceDirectory": "source",
  "ThemeDirectory": "theme",
  "UserConfiguration": {},
  "Commands": {
    "new": "new.ps1"
  }
}
```
慢慢来，同志:
 - `RootUrl` 
 - - 你项目站点的RootUrl，你也许有兴趣在本地调试站点时把他设置成`localhost`，但是没必要在配置文件里设置，因为你可以把他设置成任何值在运行时。
 - `OutputDirectory`
 - `ContentDirectory`
 - `ThemeDirectory`
 - `SourceDirectory`
 - - 这四个变量指定了类似于`output`文件夹的名称，就像上面四个文件夹一样。
 - - 这四个变量没什么用，即使变量所代表的文件夹不存在也不会报告任何错误。
 - `UserConfiguration`
 - - 属于你自己的配置空间，你可以在这里面编写任何合法的json
 - - 你的脚本可以读取到这里面的信息
 - `Commands`
 - - 命令的映射。`"new": "new.ps1"`代表把`new`这个命令映射到`new.ps1`这个文件里。上文中有例子。


值得注意的是，`build-EightLeggedEssay.ps1`文件并不能传递参数，只有命令才能传递参数。

如果你什么都不干，只是运行`EightLeggedEssay`，那么`EightLeggedEssay`就会寻找`build-EightLeggedEssay.ps1`文件，然后执行。

所以如果你对我们的示例项目执行`EightLeggedEssay`，你什么也看不到。

让我们来点有趣的:
```shell
> EightLeggedEssay --run new -- "Hello World"
```
这条shell命令会让EightLeggedEssay使用一个参数`Hello World`来调用`new`命令。

不出意外，你的`content`目录下面应该已经有一个`Hello World.md`文件了。让我们看看里面有什么：
```markdown
<!--INFOS--
{
  "CreateTime": "2022-07-06T22:27:31.9607081+08:00",
  "Title": "Hello World"
}
--INFOS-->

#Hello World!


```
 - 里面有一个文件头，使用html注释包裹起来

   你可能已经在其他的框架中见到了类似的`header`了

   我们有值得注意的几点:
   - 开头`<!--INFOS--`和结尾`--INFOS-->`必须在单独一行，不能写作:
   ```markdown
   <!--INFOS--{
   "CreateTime": "2022-07-06T22:27:31.9607081+08:00",
   "Title": "Hello World"}--INFOS-->
   ```
   或者类似的形式。

   - 文件头必须是合法的json文件。
 - 接下来就是你所熟悉的markdown（如果还不熟悉，`EightLeggedEssay`可能不适合你）
   
   `EightLeggedEssay`使用[markdig](https://github.com/xoofx/markdig)作为**内置**的markdown编译器。它符合`CommonMark`标准，并且有许多扩展，而且非常快速。

   同样的，你可以随时更换markdown编译器（如果你能找到一个更适合你的）。事实上，`EightLeggedEssay`并不依赖于任何一个markdown编译器或者什么模板引擎工作，这些东西可以随时更换。即使从源代码中删去对`EightLeggedEssay`也没有任何影响。



现在你已经了解了关于`EightLeggedEssay`的基础知识，如果你想要更进一步，那么`EightLeggedEssay`还允许你：
 - 自定义你的`build-EightLeggedEssay.ps1`文件，并生成你的站点

   如你所见，我们目前没有进行任何编译操作，也没有生成任何文件，仅仅是创建了一篇文章。

 - 添加或者修改你的命令
   
   我们的`new`命令是来自官方的，如果有一天你觉得这个命令不够完美，你可以自定义`new`命令。

   同样，你可以删去这个命令。你也可以添加任意数量的你所喜欢的命令。


对于命令的编写，与`build-EightLeggedEssay.ps1`是相似的（除了命令支持使用参数，要获取参数，使用powershell内置的`$args`变量即可，于正常pwsh脚本中的命令行参数一样）

我们将在[深入了解工作原理](advantage.md)中讨论关于`build-EightLeggedEssay.ps1`的编写操作。

