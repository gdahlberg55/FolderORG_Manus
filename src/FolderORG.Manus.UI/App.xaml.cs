using FolderORG.Manus.Core.Interfaces;
using FolderORG.Manus.Domain.Classification.Classifiers;
using FolderORG.Manus.Domain.Classification.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FolderORG.Manus.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register the classification engine as a singleton
            services.AddSingleton<IClassificationEngine, ClassificationEngine>(provider =>
            {
                var engine = new ClassificationEngine();
                
                // Register classifiers with the engine
                engine.RegisterClassifier(new ExtensionClassifier());
                engine.RegisterClassifier(new SizeClassifier());
                engine.RegisterClassifier(new DateClassifier());
                engine.RegisterClassifier(new DateClassifier(true) { Name = "ModificationDateClassifier" });
                
                return engine;
            });

            // Register main window
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
} 