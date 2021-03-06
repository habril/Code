[assembly: WebActivator.PostApplicationStartMethod(typeof(SaferPasswordProject.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace SaferPasswordProject.App_Start
{
    using System.Reflection;
    using System.Web.Mvc;

    using SimpleInjector;
    using SimpleInjector.Extensions;
	using SimpleInjector.Integration.Web;
    using SimpleInjector.Integration.Web.Mvc;
    using Identity.Services;
    
    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as MVC3 Dependency Resolver.</summary>
        public static void Initialize()
        {
            // Did you know the container can diagnose your configuration? Go to: http://bit.ly/YE8OJj.
            var container = new Container();
            
            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            
            container.RegisterMvcAttributeFilterProvider();
       
            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
     
        private static void InitializeContainer(Container container)
        {
            container.Register<ItmUserManager, tmUserManager>();
        }
    }
}