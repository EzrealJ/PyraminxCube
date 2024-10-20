using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.ConnectionString
{
    public interface IConnectionStringResolver
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns></returns>
        string GetConnectionString(string name);
        //异步获取数据库链接字符串
        Task<string> GetConnectionStringAsync(string name);
    }
}
