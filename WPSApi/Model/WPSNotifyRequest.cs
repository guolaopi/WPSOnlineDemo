namespace WPSApi.Model
{
    /// <summary>
    /// 回调通知 入参
    /// </summary>
    public class WPSNotifyRequest
    {
        /// <summary>
        /// 回调命令的参数
        /// </summary>
        public string cmd { get; set; }

        /// <summary>
        /// 回调命令的内容 由于官方给的示例中body内容不固定，所以此处使用了object，可参考文档根据自己需求进行修改
        /// </summary>
        public object body { get; set; }
    }
}
