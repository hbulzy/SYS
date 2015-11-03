using Autofac;
using Autofac.Core;
using System;
using System.Web.Mvc;
namespace Tunynet
{
	public class DIContainer
	{
		private static IContainer _container;
		public static void RegisterContainer(IContainer container)
		{
			DIContainer._container = container;
		}
		public static TService Resolve<TService>()
		{
			return ResolutionExtensions.Resolve<TService>(DIContainer._container);
		}
		public static TService ResolveNamed<TService>(string serviceName)
		{
			return ResolutionExtensions.ResolveNamed<TService>(DIContainer._container, serviceName);
		}
		public static TService Resolve<TService>(params Parameter[] parameters)
		{
			return ResolutionExtensions.Resolve<TService>(DIContainer._container, parameters);
		}
		public static TService ResolveKeyed<TService>(object serviceKey)
		{
			return ResolutionExtensions.ResolveKeyed<TService>(DIContainer._container, serviceKey);
		}
		public static TService ResolvePerHttpRequest<TService>()
		{
			IDependencyResolver current = DependencyResolver.get_Current();
			if (current != null)
			{
				TService tService = (TService)((object)current.GetService(typeof(TService)));
				if (tService != null)
				{
					return tService;
				}
			}
			return ResolutionExtensions.Resolve<TService>(DIContainer._container);
		}
	}
}
