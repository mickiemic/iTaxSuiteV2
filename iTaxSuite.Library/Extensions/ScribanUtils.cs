using Scriban;
using Scriban.Runtime;

namespace iTaxSuite.Library.Extensions
{
    public class ScribanHelper : ScriptObject
    {

    }

    public class ScriptHelper
    {
        public static async Task<string> renderString(string strTemplate, TemplateContext context)
        {
            Template template = Template.Parse(strTemplate);
            string result = await template.RenderAsync(context);
            return result ?? string.Empty;
        }
        public static async Task<string> evalToString(string expression, TemplateContext context)
        {
            var result = await Template.EvaluateAsync(expression, context);
            return (result == null) ? string.Empty : result.ToString();
        }
        public static async Task<bool> evalToBool(string expression, TemplateContext context)
        {
            var result = await Template.EvaluateAsync(expression, context);
            return result != null && result is bool && (bool)result;
        }

        public static async Task<bool> strToBool(string expression, TemplateContext context)
        {
            if (expression.StartsWith("EVAL"))
            {
                string[] parts = expression.Split(":", 2);
                return await evalToBool(parts[1], context);
            }
            else
            {
                return Convert.ToBoolean(await renderString(expression, context));
            }
        }

    }

}
