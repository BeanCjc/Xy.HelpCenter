using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Profiling;
using Xy.HelpCenter.log4net;

namespace Xy.HelpCenter.Filter
{
    public class GlobalExceptionsFilter : IExceptionFilter
    {

        private readonly IHostingEnvironment _env;
        private readonly ILoggerHelper _loggerHelper;
        public GlobalExceptionsFilter(IHostingEnvironment env, ILoggerHelper loggerHelper)
        {
            _env = env;
            _loggerHelper = loggerHelper;
        }
        public void OnException(ExceptionContext context)
        {
            var json = new JsonErrorResponse { Message = context.Exception.Message };

            //错误信息
            if (_env.IsDevelopment())
            {
                json.DevelopmentMessage = context.Exception.StackTrace;//堆栈信息
            }
            context.Result = new InternalServerErrorObjectResult(json);

            //MiniProfiler.Current.CustomTiming("Errors：", json.Message);


            //采用log4net 进行错误日志记录
            _loggerHelper.Error(json.Message, WriteLog(json.Message, context.Exception));
        }

        /// <summary>
        /// 自定义返回格式
        /// </summary>
        /// <param name="throwMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string WriteLog(string throwMsg, Exception ex)
        {
            return $"【自定义错误】：{throwMsg}\r\n【异常类型】：{ex.GetType().Name}\r\n【异常信息】：{ex.Message}\r\n【堆栈调用】：{ex.StackTrace}";
        }
    }
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
    //返回错误信息
    public class JsonErrorResponse
    {
        /// <summary>
        /// 生产环境的消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 开发环境的消息
        /// </summary>
        public string DevelopmentMessage { get; set; }
    }
}
