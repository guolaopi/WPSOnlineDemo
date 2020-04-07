## 这是个 WPS 在线编辑服务的.NET CORE 版的 demo

## 注意如果部署在 IIS 上，需要在 IIS-网站-模块中移除 WebDAVModule 来允许 put 请求

## WPS 开放平台文档 [链接](https://open.wps.cn/docs/wwo/access/api-list)

在 appsettings.json 中配置 WPSConfig 的 AppId 以及 AppSecret 即可使用

简单测试方式：
1. 配置好AppId以及AppSecret
2. 在 WPSApi/Controllers/WPSController 的 FileInfo 方法返回值中将“download_url”属性设为你要在线预览的文件的**外网下载地址**
3. 生成并部署WPSApi
4. 在 HTML/index.html中调用WPSApi的GenarateWPSUrl接口即可看到效果