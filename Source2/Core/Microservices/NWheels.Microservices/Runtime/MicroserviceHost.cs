﻿using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Primitives;
using NWheels.Kernel.Runtime.Injection;
using NWheels.Microservices.Api;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceHost : IDisposable
    {
        private readonly IBootConfiguration _bootConfig;
        private readonly IModuleLoader _moduleLoader;
        private readonly IMicroserviceHostLogger _logger;
        private readonly List<ILifecycleListenerComponent> _lifecycleComponents;
        private readonly RevertableSequence _configureSequence;
        private readonly RevertableSequence _loadSequence;
        private readonly RevertableSequence _activateSequence;
        private int _initializationCount = 0;
        private IInternalComponentContainer _container;
        private StateMachine<MicroserviceState, MicroserviceTrigger> _stateMachine;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost(IBootConfiguration bootConfig)
            : this(bootConfig, new DefaultModuleLoader(), new Mocks.MicroserviceHostLoggerMock())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost(IBootConfiguration bootConfig, IModuleLoader loader)
            : this(bootConfig, loader, new Mocks.MicroserviceHostLoggerMock())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost(IBootConfiguration bootConfig, IModuleLoader moduleLoader, IMicroserviceHostLogger logger)
        {
            _bootConfig = bootConfig;
            _logger = logger;
            _moduleLoader = moduleLoader;
            _configureSequence = new RevertableSequence(new ConfigureSequenceCodeBehind(this));
            _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
            _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
            _lifecycleComponents = new List<ILifecycleListenerComponent>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDisposable.Dispose()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure()
        {
            InitializeBeforeConfigure();

            using (_logger.NodeConfiguring())
            {
                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Configure);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToConfigure(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Configured)
                {
                    throw _logger.NodeHasFailedToConfigure();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Load()
        {
            using (_logger.NodeLoading())
            {
                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Load);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToLoad(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Standby)
                {
                    throw _logger.NodeHasFailedToLoad();
                }

                _logger.NodeSuccessfullyLoaded();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Activate()
        {
            using (_logger.NodeActivating())
            {
                try
                {
                    _stateMachine.ReceiveTrigger(MicroserviceTrigger.Activate);
                }
                catch (Exception e)
                {
                    _logger.NodeHasFailedToActivate(e);
                    throw;
                }

                if (_stateMachine.CurrentState != MicroserviceState.Active)
                {
                    throw _logger.NodeHasFailedToActivate();
                }

                _logger.NodeSuccessfullyActivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadAndActivate()
        {
            InitializeBeforeConfigure();

            using (_logger.NodeStartingUp(
                /*_bootConfig.ApplicationName,
                _bootConfig.EnvironmentType,
                _bootConfig.EnvironmentName,
                _bootConfig.NodeName,
                _bootConfig.InstanceId*/))
            {
                Load();
                Activate();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Deactivate()
        {
            using (_logger.NodeDeactivating())
            {
                _stateMachine.ReceiveTrigger(MicroserviceTrigger.Deactivate);

                if (_stateMachine.CurrentState != MicroserviceState.Standby)
                {
                    throw _logger.NodeHasFailedToDeactivate();
                }

                _logger.NodeDeactivated();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DeactivateAndUnload()
        {
            using (_logger.NodeShuttingDown())
            {
                if (_stateMachine.CurrentState == MicroserviceState.Active)
                {
                    Deactivate();
                }

                if (_stateMachine.CurrentState == MicroserviceState.Standby)
                {
                    Unload();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Unload()
        {
            using (_logger.NodeUnloading())
            {
                _stateMachine.ReceiveTrigger(MicroserviceTrigger.Unload);

                if (_stateMachine.CurrentState != MicroserviceState.Down)
                {
                    throw _logger.NodeHasFailedToUnload();
                }

                _logger.NodeUnloaded();
            }

            FinalizeAfterUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainer GetContainer()
        {
            return _container;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler StateChanged;
        public event EventHandler<AssemblyScanLoadEventArgs> AssemblyLoad;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IAssemblyLocationMap GetAssemblyLocationMap()
        {
            if (_bootConfig.AssemblyLocationMap != null)
            {
                return _bootConfig.AssemblyLocationMap;
            }

            var defaultMap = new AssemblyLocationMap();
            //defaultMap.AddDirectory(_bootConfig.ConfigsDirectory);
            defaultMap.AddDirectory(AppContext.BaseDirectory);
            return defaultMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteConfigurePhase()
        {
            return ExecutePhase(_configureSequence.Perform, _logger.NodeConfigureError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteLoadPhase()
        {
            return ExecutePhase(_loadSequence.Perform, _logger.NodeLoadError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteActivatePhase()
        {
            return ExecutePhase(_activateSequence.Perform, _logger.NodeActivationError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteDeactivatePhase()
        {
            return ExecutePhase(_activateSequence.Revert, _logger.NodeDeactivationError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecuteUnloadPhase()
        {
            return ExecutePhase(
                () => {
                    _loadSequence.Revert();
                    _configureSequence.Revert();
                }, 
                _logger.NodeUnloadError);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ExecutePhase(Action phaseAction, Action<Exception> loggerAction)
        {
            try
            {
                phaseAction();
                return true;
            }
            catch(Exception e)
            {
                loggerAction(e);
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeBeforeConfigure()
        {
            if (Interlocked.Increment(ref _initializationCount) > 1)
            {
                return;
            }

            //_container = BuildBaseContainer(_registerHostComponents);

            _stateMachine = new StateMachine<MicroserviceState, MicroserviceTrigger>(
                new StateMachineCodeBehind(this),
                new Mocks.TransientStateMachineLoggerMock<MicroserviceState, MicroserviceTrigger>());
                //_container.Resolve<TransientStateMachine<MicroserviceState, MicroserviceTrigger>.ILogger>());
            _stateMachine.CurrentStateChanged += OnStateChanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FinalizeAfterUnload()
        {
            if (Interlocked.Decrement(ref _initializationCount) > 0)
            {
                return;
            }

            _container.Dispose();
            _container = null;
            _stateMachine = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnStateChanged(object sender, EventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<Type> LoadAssembly(Type implementedInterface, string assemblyName)
        {
            var loadHandler = this.AssemblyLoad;
            List<Type> types;

            if (loadHandler != null)
            {
                var args = new AssemblyScanLoadEventArgs(implementedInterface, assemblyName);
                loadHandler(this, args);

                types = args.Destination;
            }
            else
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
                types = assembly.GetTypes()
                    .Where(x => x.GetTypeInfo().ImplementedInterfaces.Any(i => i == implementedInterface))
                    .ToList();
            }

            return types;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IMicroserviceHostLogger Logger
        {
            get
            {
                return _logger;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IBootConfiguration BootConfig
        {
            get
            {
                return _bootConfig;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<ILifecycleListenerComponent> LifecycleComponents
        {
            get
            {
                return _lifecycleComponents;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IInternalComponentContainer Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class StateMachineCodeBehind : IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>
        {
            private readonly MicroserviceHost _owner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateMachineCodeBehind(MicroserviceHost owner)
            {
                _owner = owner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<MicroserviceState, MicroserviceTrigger> machine)
            {
                machine.State(MicroserviceState.Down)
                    .SetAsInitial()
                    .OnTrigger(MicroserviceTrigger.Configure).TransitionTo(MicroserviceState.Configuring);

                machine.State(MicroserviceState.Configuring)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Configured)
                    .OnEntered(ConfiguringEntered);

                machine.State(MicroserviceState.Configured)
                    .OnTrigger(MicroserviceTrigger.Unload).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.Load).TransitionTo(MicroserviceState.Loading);

                machine.State(MicroserviceState.Loading)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Down)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                    .OnEntered(LoadingEntered);

                machine.State(MicroserviceState.Standby)
                    .OnTrigger(MicroserviceTrigger.Unload).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.Activate).TransitionTo(MicroserviceState.Activating);

                machine.State(MicroserviceState.Activating)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Active)
                    .OnEntered(ActivatingEntered);

                machine.State(MicroserviceState.Active)
                    .OnTrigger(MicroserviceTrigger.Deactivate).TransitionTo(MicroserviceState.Deactivating);

                machine.State(MicroserviceState.Deactivating)
                    .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                    .OnEntered(DeactivatingEntered);

                machine.State(MicroserviceState.Unloading)
                    .OnTrigger(MicroserviceTrigger.Done).TransitionTo(MicroserviceState.Down)
                    .OnEntered(UnloadingEntered);
            }            

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ConfiguringEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteConfigurePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteLoadPhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteActivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                var success = _owner.ExecuteDeactivatePhase();
                e.ReceiveFeedback(success ? MicroserviceTrigger.OK : MicroserviceTrigger.Failed);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
            {
                _owner.ExecuteUnloadPhase();
                e.ReceiveFeedback(MicroserviceTrigger.Done);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class MicroserviceLifecycleSequenceBase
        {
            private readonly MicroserviceHost _ownerHost;
            private IDisposable _systemSession;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceLifecycleSequenceBase(MicroserviceHost ownerHost)
            {
                _ownerHost = ownerHost;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void CallComponentsLifecycleStep(
                ILifecycleListenerComponent component,
                Func<string, IExecutionPathActivity> loggerFunc,
                Action componentLifecycleStep)
            {
                using (var activity = loggerFunc(component.GetType().FullName))
                {
                    try
                    {
                        componentLifecycleStep();
                    }
                    catch (Exception e)
                    {
                        activity.Fail(e);
                        OwnerHost.Logger.ComponentsEventFailed(component.GetType(), nameof(componentLifecycleStep), e);
                        throw;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void JoinSystemSession()
            {
                //_systemSession = _ownerLifetime.LifetimeContainer.Resolve<ISessionManager>().JoinGlobalSystem();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected void LeaveSystemSession()
            {
                _systemSession.Dispose();
                _systemSession = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected MicroserviceHost OwnerHost
            {
                get { return _ownerHost; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: we don't use any features of revertable sequence here; 
        //      why not just convert it into a plain logic class?
        //      or instead, we can convert ForEach loops over feature loaders into ForEach'es of the revertable sequence.
        private class ConfigureSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            public ConfigureSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(LoadFeatures);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadFeatures()
            {
                var rootBuilder = new ComponentContainerBuilder(rootContainer: null);
                var rootContainer = rootBuilder.CreateComponentContainer(isRootContainer: true);
                var featureLoaders = GetConfiguredFeatureLoaders();

                executeFeatureLoaderStep((feature, newComponents) => feature.ContributeConfigSections(newComponents));

                //TODO: compile & register contributed configuration objects

                executeFeatureLoaderStep((feature, newComponents) => feature.ContributeConfiguration(rootContainer));
                executeFeatureLoaderStep((feature, newComponents) => feature.ContributeComponents(rootContainer, newComponents));
                executeFeatureLoaderStep((feature, newComponents) => feature.ContributeAdapterComponents(rootContainer, newComponents));

                if (rootContainer.TryResolve<ICompilationFeature>(out ICompilationFeature compilationFeature))
                {
                    executeFeatureLoaderStep((feature, newComponents) => feature.CompileComponents(rootContainer));
                    compilationFeature.CompileGeneratedComponents();
                    executeFeatureLoaderStep((feature, newComponents) => feature.ContributeCompiledComponents(rootContainer, newComponents));
                }

                OwnerHost.Container = rootContainer;

                void executeFeatureLoaderStep(Action<IFeatureLoader, IComponentContainerBuilder> step)
                {
                    var newComponents = new ComponentContainerBuilder(rootContainer);

                    foreach (var feature in featureLoaders) {
                        step(feature, newComponents);
                    };

                    rootContainer.Merge(newComponents);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private List<IFeatureLoader> GetConfiguredFeatureLoaders()
            {
                return OwnerHost._moduleLoader.GetBootFeatureLoaders(OwnerHost._bootConfig).ToList();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IInternalComponentContainerBuilder CreateComponentContainerBuilder(IInternalComponentContainer rootContainer)
            {
                return new ComponentContainerBuilder(rootContainer);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            public LoadSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.Once().OnPerform(FindLifecycleComponents);

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) => 
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceLoading, component.MicroserviceLoading))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceUnloaded, component.MicroserviceMaybeUnloaded));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceLoad, component.Load))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceUnload, component.MayUnload));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceLoaded, component.MicroserviceLoaded))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceUnloading, component.MicroserviceMaybeUnloading));
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                ...
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void FindLifecycleComponents()
            {
                using (OwnerHost.Logger.LookingForLifecycleComponents())
                {
                    try
                    {
                        var foundComponents = OwnerHost.Container.ResolveAll<ILifecycleListenerComponent>();
                        OwnerHost.LifecycleComponents.AddRange(foundComponents);
                        OwnerHost.LifecycleComponents.ForEach(x => OwnerHost.Logger.FoundLifecycleComponent(x.GetType().FriendlyName()));
                        
                        if (OwnerHost.LifecycleComponents.Count == 0)
                        {
                            OwnerHost.Logger.NoLifecycleComponentsFound();
                        }
                    }
                    catch (Exception e)
                    {
                        OwnerHost.Logger.FailedToLoadLifecycleComponents(e);
                        throw;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : MicroserviceLifecycleSequenceBase, IRevertableSequenceCodeBehind
        {
            public ActivateSequenceCodeBehind(MicroserviceHost ownerHost)
                : base(ownerHost)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceActivating, component.MicroserviceActivating))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceDeactivated, component.MicroserviceMaybeDeactivated));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceActivate, component.Activate))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceDeactivate, component.MayDeactivate));

                sequence.ForEach(() => OwnerHost.LifecycleComponents)
                    .OnPerform((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceActivated, component.MicroserviceActivated))
                    .OnRevert((component, index, isLast) =>
                        CallComponentsLifecycleStep(component, OwnerHost.Logger.MicroserviceDeactivating, component.MicroserviceMaybeDeactivating));
                /*sequence.Once().OnPerform(JoinSystemSession).OnRevert(LeaveSystemSession);
                ...
                sequence.Once().OnPerform(LeaveSystemSession).OnRevert(JoinSystemSession);*/
            }
        }
    }
}
