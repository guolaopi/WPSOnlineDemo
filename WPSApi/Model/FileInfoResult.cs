namespace WPSApi.Model
{
    /// <summary>
    /// 获取文件元数据 出参
    /// </summary>
    public class FileInfoResult : WPSBaseModel
    {
        /// <summary>
        /// 文件
        /// </summary>
        public WPSFile file { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserForFile user { get; set; }
    }

    /// <summary>
    /// 用户权限控制
    /// </summary>
    public class User_acl
    {
        /// <summary>
        /// 是否允许重命名，1：是 0：否
        /// </summary>
        public int rename { get; set; }

        /// <summary>
        /// 是否允许查看历史记录，1：是 0：否
        /// </summary>
        public int history { get; set; }

        /// <summary>
        /// 是否允许复制，1：是 0：否
        /// </summary>
        public int copy { get; set; }
    }

    /// <summary>
    /// 水印，只实现了简单的水印显示，如需配置水印的字体透明度等样式需要自己添加属性，参见：https://open.wps.cn/docs/wwo/access/api-list#des2 watermark部分
    /// </summary>
    public class Watermark
    {
        /// <summary>
        /// 是否有水印， 1：有 0：无
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 水印字符串
        /// </summary>
        public string value { get; set; }
    }

    /// <summary>
    /// 文件model
    /// </summary>
    public class WPSFile
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// 文件大小，单位B
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 创建者用户名
        /// </summary>
        public string creator { get; set; }

        /// <summary>
        /// 创建时间的时间戳，单位秒
        /// </summary>
        public int create_time { get; set; }

        /// <summary>
        /// 修改者的用户名
        /// </summary>
        public string modifier { get; set; }

        /// <summary>
        /// 修改时间的时间戳，单位秒
        /// </summary>
        public int modify_time { get; set; }

        /// <summary>
        /// 文件下载url
        /// </summary>
        public string download_url { get; set; }

        /// <summary>
        /// 用户权限控制配置
        /// </summary>
        public User_acl user_acl { get; set; }

        /// <summary>
        /// 水印配置
        /// </summary>
        public Watermark watermark { get; set; }
    }

    /// <summary>
    /// 用户对于此文件的信息
    /// </summary>
    public class UserForFile
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 用户对文件的权限，只能取 “read” 和 “write” 两个字符串，表示只读和可修改
        /// </summary>
        public string permission { get; set; }

        /// <summary>
        /// 用户头像url
        /// </summary>
        public string avatar_url { get; set; }
    }
}
