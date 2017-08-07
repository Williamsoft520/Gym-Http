using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>
    /// 为程序提供快速、简单的 Http 请求。
    /// </summary>
    /// <remarks>可通过扩展方法对其他请求类型进行扩展，例如 DELETE/PUT 等。</remarks>
    public static class HttpExtension
    {
        /// <summary>
        /// 获取 multipart/form-data 的 content-type 字符串。
        /// </summary>
        public const string FORM_DATA = "multipart/form-data";

        /// <summary>
        /// 获取 application/x-www-form-urlencoded 的 content-type 字符串。
        /// </summary>
        public const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";

        /// <summary>
        /// 获取 application/json 的 content-type 字符串。
        /// </summary>
        public const string JSON = "application/json";

        /// <summary>
        /// 获取 application/xml 的 content-type 字符串。
        /// </summary>
        public const string XML = "application/xml";

        /// <summary>
        /// 获取 application/html 的 content-type 字符串。
        /// </summary>
        public const string HTML = "application/html";

        /// <summary>
        /// 获取 application/javascript 的 content-type 字符串。
        /// </summary>
        public const string JAVASCRIPT = "application/javascript";

        /// <summary>
        /// 获取 Http 响应的字符串。
        /// </summary>
        /// <param name="response">一个 <see cref="WebResponse"/> 实例的扩展。</param>
        /// <returns></returns>
        public static string GetResponseString(this WebResponse response)
        {
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return stream.ReadToEnd();
            }
        }

        /// <summary>
        /// 将当前 <see cref="WebResponse"/> 对象转换为 <see cref="HttpWebResponse"/> 实例。
        /// </summary>
        /// <param name="response">一个 <see cref="WebResponse"/> 实例的扩展。</param>
        /// <returns></returns>
        public static HttpWebResponse ToHttpResponse(this WebResponse response)
        {
            if (response is HttpWebResponse res)
                return res;
            throw new InvalidCastException("当前的 WebResponse 对象不支持对 HttpWebResponse 类型的转换");
        }

        /// <summary>
        /// 设置当前 <see cref="WebRequest"/> 实例的 ContentType 字符串。
        /// </summary>
        /// <param name="request">当前 <see cref="WebRequest"/> 实例扩展。</param>
        /// <param name="contentType">发送请求时的内容类型字符串。可以使用 <see cref="HttpExtension"/> 定义好的常量。</param>
        /// <returns>当前 <see cref="WebRequest"/> 实例。</returns>
        public static WebRequest SetContentType(this WebRequest request, string contentType,Encoding encoding=null)
        {
            request.ContentType = $"{contentType}; charset={encoding.Utf8IfNull().WebName}";
            return request;
        }
        /// <summary>
        /// 设置当前 <see cref="WebRequest"/> 实例的 请求方式类型（GET,POST,PUT,DELETE ...）。
        /// </summary>
        /// <param name="request">当前 <see cref="WebRequest"/> 实例扩展。</param>
        /// <param name="method">当前请求的方式。</param>
        /// <returns>当前 <see cref="WebRequest"/> 实例。</returns>
        public static WebRequest SetMethod(this WebRequest request, string method)
        {
            request.Method = new Http.HttpMethod(method).Method;
            return request;
        }
        
        /// <summary>
        /// 设置当前 <see cref="WebRequest"/> 实例请求的头内容。
        /// </summary>
        /// <param name="request">当前 <see cref="WebRequest"/> 实例扩展。</param>
        /// <param name="header">这是一个 <see cref="HttpRequestHeader"/> 枚举类型。</param>
        /// <param name="value">对应头的值。</param>
        /// <returns>当前 <see cref="WebRequest"/> 实例。</returns>
        public static WebRequest SetRequestHeader(this WebRequest request, HttpRequestHeader header, string value)
        {
            request.Headers[header] = value;
            return request;
        }

        /// <summary>
        /// 设置当前 <see cref="WebRequest"/> 实例响应的头内容。
        /// </summary>
        /// <param name="request">当前 <see cref="WebRequest"/> 实例扩展。</param>
        /// <param name="header">这是一个 <see cref="HttpResponseHeader"/> 枚举类型。</param>
        /// <param name="value">对应头的值。</param>
        /// <returns>当前 <see cref="WebRequest"/> 实例。</returns>
        public static WebRequest SetResponseHeader(this WebRequest request, HttpResponseHeader header, string value)
        {
            request.Headers[header] = value;
            return request;
        }

        /// <summary>
        /// 发送指定的 form data 数据。
        /// </summary>
        /// <param name="request">WebRequest 扩展实例</param>
        /// <param name="data">提交数据时所需要的数据。可使用 <code>new { name = "xxx" }</code> 匿名对象的形式提供。</param>
        /// <param name="encoding">对于提交的数据的编码格式。null表示使用默认的 UTF-8 编码格式。</param>    
        /// <returns>当前 <see cref="WebRequest"/> 实例。</returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        public static async Task<WebResponse> PushDataAsync(this WebRequest request, object data,Encoding encoding=null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }


            var form_data = string.Empty;
            if (data.GetType() != typeof(string))
            {
                var properties = new Dictionary<string, string>();

                foreach (var propInfo in data.GetType().GetRuntimeProperties())
                {
                    properties.Add(propInfo.Name, propInfo.GetValue(data).ToString());
                }

                form_data = properties.Select(kvp => $"{kvp.Key}={kvp.Value}").Aggregate((prev, next) => $"{prev}&{next}");
            }
            else
            {
                form_data = data.ToString();
            }

            var buffer = form_data.ToBytes(encoding);

            using (var stream = await request.GetRequestStreamAsync())
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            return await request.GetResponseAsync();
        }
    }
}
