using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WPSApi.Helper;
using WPSApi.Model;

namespace WPSApi.Controllers
{
    [ApiController]
    public class WPSController : ControllerBase
    {
        private ILog _logger = LogManager.GetLogger("wps", "");

        /// <summary>
        /// 简单过滤一下HttpRequest参数
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private RequestParam FilterRequestForWPS(HttpRequest Request)
        {
            var result = new RequestParam();
            result.FileId = Request.Headers["x-weboffice-file-id"].ToString();
            var queryStr = Request.QueryString.ToString();
            queryStr = queryStr.StartsWith("?") ? queryStr.Substring(1) : queryStr;
            if (string.IsNullOrEmpty(queryStr) || string.IsNullOrEmpty(result.FileId))
            {
                return new RequestParam
                {
                    code = 403,
                    msg = "参数错误，无法打开文件",
                    Status = false
                };
            }

            // url参数序列化成Dictionary
            result.Params = queryStr.Split("&", StringSplitOptions.RemoveEmptyEntries).ToDictionary(p => p.Split("=")[0], p => p.Split("=")[1]);

            // 此处判断是否传递了自定义的 _w_userId 参数，如果不需要此参数的话可以注释该判断
            if (!result.Params.ContainsKey("_w_userId"))
            {
                return new RequestParam
                {
                    code = 403,
                    msg = "用户异常",
                    Status = false
                };
            }
            return result;
        }

        /// <summary>
        /// 生成iframe用的url（此方法非WPS官方的，主要是为了签名，也可以自己实现）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("api/wps/genarate")]
        [HttpPost]
        public Task<GenarateResult> GenarateWPSUrl(GenarateRequest request)
        {
            return Task.Run(() =>
            {
                var url = WPSSignatureHelper.GenarateUrl(request.FileId,
                                                request.FileType,
                                                new Dictionary<string, string> {
                                                    { "_w_userId", request.UserId },
                                                    { "_w_fileName", request.FileName }
                                                });
                // 上面的写法是在生成的url中带了两个自定义参数 _w_userId 和 _w_fileName，可以根据业务自己扩展，生成url是这样的：
                // https://wwo.wps.cn/office/w/123?_w_appid=123456&_w_fileName=x.docx&_w_userId=5024&_w_signature=xxxxx


                // 也可以不写自定义参数，这样生成的url会只有 _w_appId 和 _w_ signatrue，例如：https://wwo.wps.cn/office/w/123?_w_appid=123456&_w_signature=xxxxx
                //var url = WPSHelper.GenarateUrl(request.FileId,request.FileType);

                return new GenarateResult { Url = url };
            });
        }

        #region WPS官方要求实现的回调接口

        /// <summary>
        /// 获取文件元数据
        /// </summary>
        /// <returns></returns>
        [Route("v1/3rd/file/info")]
        [HttpGet]
        public Task<FileInfoResult> FileInfo()
        {
            return Task.Run(() =>
            {
                // 简单的过滤下不合理的请求，可注释，以下接口基本上都有此过滤
                var request = FilterRequestForWPS(Request);
                if (!request.Status)
                {
                    return new FileInfoResult { code = request.code, msg = request.msg };
                }

                // 获取自定义参数
                var userId = request.Params["_w_userId"].ToString();

                // 从数据库查询用户名、文件 等信息......

                // 创建时间和修改时间默认全是现在，可更改，但是注意时间戳是11位的（秒）
                var now = TimestampHelper.GetCurrentTimestamp();

                var result = new FileInfoResult
                {
                    file = new Model.WPSFile
                    {
                        id = request.FileId,
                        name = "文件名",
                        version = 1,
                        size = 1024, // WPS单位是B
                        create_time = now,
                        creator = "创建者用户名",
                        modify_time = now,
                        modifier = "修改者用户名",
                        download_url = "文件下载链接",
                        user_acl = new User_acl
                        {
                            history = 1, // 允许查看历史版本
                            rename = 1, // 允许重命名
                            copy = 1 // 允许复制
                        },
                        watermark = new Watermark
                        {
                            type = 0, // 1为有水印
                            value = "水印文字"
                        }
                    },
                    user = new UserForFile()
                    {
                        id = userId,
                        name = "用户名",
                        //permission = "read",
                        permission = "write", // write为允许编辑，read为只能查看
                        avatar_url = "用户头像url",
                    }
                };
                return result;
            });
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="body">包含一个名为ids的字符串数组，里面是用户id</param>
        /// <returns></returns>
        [Route("v1/3rd/user/info")]
        [HttpPost]
        public Task<UserModel> GetUserInfo(GetUserInfoRequest body)
        {
            return Task.Run(() =>
            {
                var request = FilterRequestForWPS(Request);
                if (!request.Status)
                {
                    return new UserModel();
                }

                var result = new UserModel
                {
                    id = "用户ID",
                    name = "用户名",
                    avatar_url = "用户头像url"
                };
                return result;
            });
        }

        /// <summary>
        /// 通知此文件目前有哪些人正在协作
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("v1/3rd/file/online")]
        [HttpPost]
        public Task<WPSBaseModel> Online(GetUserInfoRequest body)
        {
            return Task.Run(() =>
            {
                var result = new WPSBaseModel
                {
                    code = 200,
                    msg = "success"
                };
                return result;
            });
        }

        /// <summary>
        /// 上传文件新版本（保存文件）
        /// </summary>
        /// <param name="file">传来的文件流</param>
        /// <returns></returns>
        [Route("v1/3rd/file/save")]
        [HttpPost]
        public Task<SaveFileResult> SaveFile(IFormFile file)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var request = FilterRequestForWPS(Request);
                    if (!request.Status)
                    {
                        return new SaveFileResult { code = request.code, msg = request.msg };
                    }

                    using (var stream = System.IO.File.Create("保存的文件名"))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var result = new SaveFileResult
                    {
                        file = new WPSFileModel
                        {
                            download_url = "新的文件下载链接",
                            id = request.FileId,
                            name = request.Params["_w_fileName"].ToString()
                        }
                    };
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error("save file failed: ", ex);
                    return new SaveFileResult { code = 403, msg = "保存出现异常" };
                }
            });
        }

        /// <summary>
        /// 获取特定版本的文件信息
        /// </summary>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        [Route("/v1/3rd/file/version")]
        [HttpGet]
        public Task<GetFileByVersionResult> Version(int version)
        {
            return Task.Run(() =>
            {
                var request = FilterRequestForWPS(Request);
                if (!request.Status)
                {
                    return new GetFileByVersionResult { code = request.code, msg = request.msg };
                }

                // 从数据库查询文件信息......

                // 创建时间和修改时间默认全是现在
                var now = TimestampHelper.GetCurrentTimestamp();
                var result = new GetFileByVersionResult
                {
                    id = request.FileId,
                    name = "文件名",
                    version = 1,
                    size = 1024,
                    create_time = now,
                    creator = "创建者用户名",
                    modify_time = now,
                    modifier = "修改者用户名",
                    download_url = "文件下载url"
                };
                return result;
            });
        }

        /// <summary>
        /// 文件重命名
        /// </summary>
        /// <param name="body">包含一个name的字符串属性，值为保存的新文件名</param>
        /// <returns></returns>
        [Route("v1/3rd/file/rename")]
        [HttpPut]
        public Task<WPSBaseModel> RenameFile(RenameFileRequest body)
        {
            return Task.Run(() =>
            {
                var request = FilterRequestForWPS(Request);
                if (!request.Status)
                {
                    return new WPSBaseModel { code = request.code, msg = request.msg };
                }

                var result = new WPSBaseModel
                {
                    code = 200,
                    msg = "success"
                };
                return result;
            });
        }

        /// <summary>
        /// 获取所有历史版本文件信息
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("v1/3rd/file/history")]
        [HttpPost]
        public Task<GetHistoryResult> GetHistory(GetHistoryRequest body)
        {
            return Task.Run(() =>
            {
                var request = FilterRequestForWPS(Request);
                if (!request.Status)
                {
                    return new GetHistoryResult { code = request.code, msg = request.msg };
                }
                // 从数据库查询用户、文件信息等......

                // 创建时间和修改时间默认全是现在
                var now = TimestampHelper.GetCurrentTimestamp();

                // 不需要使用历史版本功能的此处也请返回，如果此接口不通时，文档加载会报错：“GetFileInfoFailed”
                var result = new GetHistoryResult
                {
                    histories = new List<HistroyModel>
                    {
                        new HistroyModel
                        {
                         id=request.FileId,
                         name="文件名",
                         size=1024, // 单位B
                         version=1,
                         download_url="文件下载链接",
                         create_time=now,
                         modify_time=now,
                         creator=new UserModel
                         {
                             id="创建者ID",
                             name="创建者名",
                             avatar_url = "创建者头像url"
                         },
                         modifier=new UserModel
                         {
                             id="修改者ID",
                             name="修改者名",
                             avatar_url = "修改者头像url"
                         }
                        }
                    }
                };
                return result;
            });
        }

        /// <summary>
        /// 新建文件
        /// </summary>
        /// <returns></returns>
        [Route("v1/3rd/file/new")]
        [HttpPost]
        public Task<CreateWPSFileResult> NewFile(CreateWPSFileRequest request)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var filterRequest = FilterRequestForWPS(Request);
                    if (!filterRequest.Status)
                    {
                        return new CreateWPSFileResult { code = filterRequest.code, msg = filterRequest.msg };
                    }

                    using (var stream = System.IO.File.Create("保存的文件名"))
                    {
                        await request.file.CopyToAsync(stream);
                    }

                    var result = new CreateWPSFileResult
                    {
                        redirect_url = "新的文件访问链接",
                        user_id = "创建人id"
                    };
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error("save file failed: ", ex);
                    return new CreateWPSFileResult { code = 403, msg = "新建文件出现异常" };
                }
            });
        }

        /// <summary>
        /// 回调通知
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Route("v1/3rd/onnotify")]
        [HttpPost]
        public Task<WPSBaseModel> WPSNotify(WPSNotifyRequest body)
        {
            return Task.Run(() =>
            {
                var result = new WPSBaseModel
                {
                    code = 200,
                    msg = "success"
                };
                return result;
            });
        }

        #endregion
    }
}
