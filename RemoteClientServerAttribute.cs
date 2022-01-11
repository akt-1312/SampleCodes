using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using TestSamplePractice.Controllers;

namespace TestSamplePractice.CustomValidation
{
    public class RemoteClientServerAttribute : RemoteAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            Type controller = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(type => type.Name.ToLower() == string.Format("{0}Controller",
                this.RouteData["controller"].ToString()).ToLower());

            if (controller != null)
            {
                MethodInfo action = controller.GetMethods().FirstOrDefault(method => method.Name.ToLower() ==
                this.RouteData["action"].ToString().ToLower());
                if (action != null)
                {
                    var actionParams = action.GetParameters()?.Select(x=> x.Name).ToArray();
                    object[] actionParamsArray = new object[actionParams == null ? 0 : actionParams.Length];
                    if(actionParams != null && actionParams.Length > 0)
                    {
                        for(int i = 0; i < actionParams.Length; i++)
                        {
                            string str = actionParams[i].ToString();
                            string paramKey = char.ToUpper(str[0]) + str.Substring(1); //actionParams[i].Name;
                            var paramValue = validationContext.ObjectType.GetProperty(paramKey).GetValue(validationContext.ObjectInstance, null);
                            actionParamsArray[i] = paramValue;
                        }
                    }
                    object instance = TryCreateController(validationContext, controller);
                    //object instance = Activator.CreateInstance(controller);                   
                    object response = action.Invoke(instance, actionParamsArray);
                    if (response is JsonResult)
                    {
                        object jsonData = ((JsonResult)response).Value;
                        if (jsonData is bool)
                        {
                            return (bool)jsonData ? ValidationResult.Success : new ValidationResult(this.ErrorMessage);
                        }
                        else
                        {
                            return new ValidationResult(jsonData.ToString());
                        }
                    }
                }
            }
            return new ValidationResult(this.ErrorMessage);
        }

        private object TryCreateController(ValidationContext context, Type controllerType)
        {
            if (controllerType == null)
            {
                return null;
            }
            foreach (var constructor in controllerType.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                var args = new dynamic[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = context.GetService(parameters[i].ParameterType);
                }

                try
                {
                    var instance = Activator.CreateInstance(controllerType, args);
                    if (instance != null)
                    {
                        return instance;
                    }
                }
                catch
                {
                    continue;
                }

            }

            return null;
        }



        public RemoteClientServerAttribute(string routeName) : base(routeName)
        {
        }

        public RemoteClientServerAttribute(string action, string controller) : base(action, controller)
        {
        }

        public RemoteClientServerAttribute(string action, string controller, string area) : base(action, controller, area)
        {
        }
    }
}
