using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Gradera.ObjectChangeFilter
{
    public class ObjectChangeFilter : ActionFilterAttribute
    {
        public string IdentifierProperty { get; set; }
        public ChangeType ChangeType { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            if (ChangeType == ChangeType.Save)
            {
                string objectHash = actionContext.Request.Headers.GetValues("objectHash").FirstOrDefault();
                string assemblyHash = actionContext.Request.Headers.GetValues("assemblyHash").FirstOrDefault();
                DateTime getDateTime = JsonConvert.DeserializeObject<DateTime>(actionContext.Request.Headers.GetValues("getDateTime").FirstOrDefault());
                bool forceSave = false;

                try
                {
                    forceSave = bool.Parse(actionContext.Request.Headers.GetValues("forceSave").FirstOrDefault());
                }
                catch (Exception) { }

                if (string.IsNullOrEmpty(objectHash) || string.IsNullOrEmpty(assemblyHash) || getDateTime <= DateTime.MinValue) return;

                string assembly = actionContext.ActionArguments.First().Value.GetType().AssemblyQualifiedName;
                string uniqueId = GetPropertyValue(actionContext.ActionArguments.First().Value, IdentifierProperty).ToString();

                string hashedObject = string.Empty;
                string hashedAssembly = string.Empty;

                using (MD5 md5Hash = MD5.Create())
                {
                    hashedObject = GetMd5Hash(md5Hash, JsonConvert.SerializeObject(actionContext.ActionArguments.First().Value));
                    hashedAssembly = GetMd5Hash(md5Hash, string.Concat(assembly, uniqueId));
                }

                if (!forceSave && assemblyHash == hashedAssembly
                    && ObjectChangeHandler.Instance.SaveList.Any(o => o.AssemblyHash == assemblyHash
                    && o.GetDate > getDateTime && o.ObjectHash != objectHash))
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Conflict);
                    return;
                }
                else if (assemblyHash != hashedAssembly)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.ResetContent);
                    return;
                }
                else
                {
                    ObjectChangeHandler.Instance.SaveList.Add(new ObjectChangeModel()
                    {
                        ObjectHash = hashedObject,
                        AssemblyHash = hashedAssembly,
                        GetDate = DateTime.UtcNow
                    });
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (ChangeType == ChangeType.Get)
            {
                string assembly = ((ObjectContent)actionExecutedContext.Response.Content).Value.GetType().AssemblyQualifiedName;
                string uniqueId = GetPropertyValue(((ObjectContent)actionExecutedContext.Response.Content).Value,
                    IdentifierProperty).ToString();
                string hashedObject = string.Empty;
                string hashedAssembly = string.Empty;

                using (MD5 md5Hash = MD5.Create())
                {
                    hashedObject = GetMd5Hash(md5Hash, JsonConvert.SerializeObject(((ObjectContent)actionExecutedContext.Response.Content).Value));
                    hashedAssembly = GetMd5Hash(md5Hash, string.Concat(assembly, uniqueId));
                }

                actionExecutedContext.Response.Headers.Add("objectHash", hashedObject);
                actionExecutedContext.Response.Headers.Add("assemblyHash", hashedAssembly);
                actionExecutedContext.Response.Headers.Add("getDateTime", JsonConvert.SerializeObject(DateTime.UtcNow));
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            string hashOfInput = GetMd5Hash(md5Hash, input);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetPropertyValue(object obj, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) propertyName = "Id";
            return obj.GetType().GetProperties()
               .Single(pi => pi.Name == propertyName)
               .GetValue(obj, null);
        }
    }
}
