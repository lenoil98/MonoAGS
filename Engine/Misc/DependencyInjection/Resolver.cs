﻿using System;
using Autofac;
using System.Reflection;
using AGS.API;
using System.Collections.Generic;
using Autofac.Features.ResolveAnything;

namespace AGS.Engine
{
	public class Resolver
	{
		public Resolver(IEngineConfigFile configFile)
		{
			Builder = new ContainerBuilder ();

			if (configFile.DebugResolves)
			{
				Builder.RegisterModule(new AutofacResolveLoggingModule ());
			}

			Builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).
				Except<SpatialAStarPathFinder>().AsImplementedInterfaces();

			Builder.RegisterType<GLImageRenderer>().As<IImageRenderer>();
			Builder.RegisterType<AGSObject>().As<IObject>();
			Builder.RegisterType<GLImage>().As<IImage>();

			Builder.RegisterType<AGSGameState>().SingleInstance().As<IGameState>();
			Builder.RegisterType<AGSGame>().SingleInstance().As<IGame>();
			Builder.RegisterType<AGSGameEvents>().SingleInstance().As<IGameEvents>();
			Builder.RegisterType<BitmapPool>().SingleInstance();
			Builder.RegisterType<GLViewportMatrix>().SingleInstance().As<IGLViewportMatrix>();
			Builder.RegisterType<AGSPlayer>().SingleInstance().As<IPlayer>();
			Builder.RegisterType<ResourceLoader>().SingleInstance().As<IResourceLoader>();
			Builder.RegisterType<AGSCutscene>().SingleInstance().As<ICutscene>();

			registerComponents();

			Builder.RegisterType<AGSSprite>().As<ISprite>();
			Builder.RegisterGeneric(typeof(AGSEvent<>)).As(typeof(IEvent<>));

			Dictionary<string, GLImage> textures = new Dictionary<string, GLImage> (1024);
			Builder.RegisterInstance(textures);
			Builder.RegisterInstance(textures).As(typeof(IDictionary<string, GLImage>));

			FastFingerChecker checker = new FastFingerChecker ();
			Builder.RegisterInstance(checker);

			Builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
		}

		public ContainerBuilder Builder { get; private set; }

		public IContainer Container { get; private set; }

		public void Build()
		{
			Container = Builder.Build();

			var updater = new ContainerBuilder ();
			updater.RegisterInstance(Container);
			updater.RegisterInstance(this);
			updater.Update(Container);
		}

		private void registerComponents()
		{
			var assembly = Assembly.GetCallingAssembly();
			foreach (var type in assembly.GetTypes())
			{
				if (!isComponent(type)) continue;
				registerComponent(type);
			}
			Builder.RegisterType<VisibleProperty>().As<IVisibleComponent>();
			Builder.RegisterType<EnabledProperty>().As<IEnabledComponent>();
		}

		private bool isComponent(Type type)
		{
			return (type.BaseType == typeof(AGSComponent));
		}

		private void registerComponent(Type type)
		{
			foreach (var compInterface in type.GetInterfaces())
			{
				if (compInterface == typeof(IComponent) || compInterface == typeof(IDisposable)) continue;
				Builder.RegisterType(type).As(compInterface);
			}
		}
	}
}

