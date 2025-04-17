using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FolderORG.Manus.Domain.Rules.Models;
using FolderORG.Manus.Domain.Rules.Repository;
using FolderORG.Manus.Domain.Rules.Services;

namespace FolderORG.Manus.Application.Rules
{
    /// <summary>
    /// Coordinates rule evaluation and action execution for file organization
    /// </summary>
    public class RuleEngine
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly IRuleEvaluationService _ruleEvaluationService;
        
        /// <summary>
        /// Creates a new rule engine
        /// </summary>
        /// <param name="ruleRepository">Repository for rule storage</param>
        /// <param name="ruleEvaluationService">Service for rule evaluation</param>
        public RuleEngine(IRuleRepository ruleRepository, IRuleEvaluationService ruleEvaluationService)
        {
            _ruleRepository = ruleRepository;
            _ruleEvaluationService = ruleEvaluationService;
        }
        
        /// <summary>
        /// Processes a single file according to enabled rules
        /// </summary>
        /// <param name="filePath">The file to process</param>
        /// <returns>The actions that will be performed</returns>
        public async Task<IEnumerable<FolderAction>> ProcessFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);
                
            var enabledRules = await _ruleRepository.GetEnabledRulesAsync();
            return await _ruleEvaluationService.GenerateActionsAsync(filePath, enabledRules);
        }
        
        /// <summary>
        /// Processes a directory and its files according to enabled rules
        /// </summary>
        /// <param name="directoryPath">The directory to process</param>
        /// <param name="recursive">Whether to process subdirectories</param>
        /// <param name="filePattern">Optional file pattern for filtering</param>
        /// <returns>Dictionary mapping files to their actions</returns>
        public async Task<Dictionary<string, IEnumerable<FolderAction>>> ProcessDirectoryAsync(
            string directoryPath, bool recursive = true, string filePattern = "*")
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException("Directory not found: " + directoryPath);
                
            var enabledRules = await _ruleRepository.GetEnabledRulesAsync();
            var results = new Dictionary<string, IEnumerable<FolderAction>>();
            
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, filePattern, searchOption);
            
            foreach (var file in files)
            {
                var actions = await _ruleEvaluationService.GenerateActionsAsync(file, enabledRules);
                results.Add(file, actions);
            }
            
            return results;
        }
        
        /// <summary>
        /// Checks if a file matches any enabled rules
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>Collection of matching rules</returns>
        public async Task<IEnumerable<RuleDefinition>> GetMatchingRulesAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);
                
            var enabledRules = await _ruleRepository.GetEnabledRulesAsync();
            return await _ruleEvaluationService.GetMatchingRulesAsync(filePath, enabledRules);
        }
        
        /// <summary>
        /// Creates a rule from a template
        /// </summary>
        /// <param name="templateName">Name of the template to use</param>
        /// <returns>The created rule</returns>
        public async Task<RuleDefinition> CreateRuleFromTemplateAsync(string templateName)
        {
            var rule = templateName.ToLowerInvariant() switch
            {
                "documents" => CreateDocumentsTemplate(),
                "images" => CreateImagesTemplate(),
                "music" => CreateMusicTemplate(),
                "videos" => CreateVideosTemplate(),
                "downloads" => CreateDownloadsTemplate(),
                _ => throw new ArgumentException($"Unknown template: {templateName}")
            };
            
            return await _ruleRepository.AddRuleAsync(rule);
        }
        
        /// <summary>
        /// Creates a template for organizing documents
        /// </summary>
        private RuleDefinition CreateDocumentsTemplate()
        {
            var builder = new RuleBuilder()
                .WithName("Organize Documents")
                .WithDescription("Organizes document files into a Documents folder")
                .WithPriority(100)
                .WithCategory("Templates")
                .WithFileExtensionCondition("doc,docx,pdf,txt,rtf,odt,xls,xlsx,ppt,pptx", false)
                .WithMoveAction(@"{UserDocuments}\Documents\{FileExtension}", true);
                
            return builder.Build();
        }
        
        /// <summary>
        /// Creates a template for organizing images
        /// </summary>
        private RuleDefinition CreateImagesTemplate()
        {
            var builder = new RuleBuilder()
                .WithName("Organize Images")
                .WithDescription("Organizes image files into an Images folder by year/month")
                .WithPriority(100)
                .WithCategory("Templates")
                .WithFileExtensionCondition("jpg,jpeg,png,gif,bmp,tiff,tif,svg,webp", false)
                .WithMoveAction(@"{UserPictures}\{CreationYear}\{CreationMonth}", true);
                
            return builder.Build();
        }
        
        /// <summary>
        /// Creates a template for organizing music
        /// </summary>
        private RuleDefinition CreateMusicTemplate()
        {
            var builder = new RuleBuilder()
                .WithName("Organize Music")
                .WithDescription("Organizes music files into a Music folder")
                .WithPriority(100)
                .WithCategory("Templates")
                .WithFileExtensionCondition("mp3,wav,flac,aac,ogg,wma,m4a", false)
                .WithMoveAction(@"{UserMusic}\{Artist|Unknown}\{Album|Unsorted}", true);
                
            return builder.Build();
        }
        
        /// <summary>
        /// Creates a template for organizing videos
        /// </summary>
        private RuleDefinition CreateVideosTemplate()
        {
            var builder = new RuleBuilder()
                .WithName("Organize Videos")
                .WithDescription("Organizes video files into a Videos folder")
                .WithPriority(100)
                .WithCategory("Templates")
                .WithFileExtensionCondition("mp4,avi,mkv,mov,wmv,flv,webm", false)
                .WithMoveAction(@"{UserVideos}\{CreationYear}", true);
                
            return builder.Build();
        }
        
        /// <summary>
        /// Creates a template for organizing downloads
        /// </summary>
        private RuleDefinition CreateDownloadsTemplate()
        {
            var builder = new RuleBuilder()
                .WithName("Organize Downloads")
                .WithDescription("Organizes files in the Downloads folder by type")
                .WithPriority(100)
                .WithCategory("Templates")
                .WithCopyAction(@"{UserDownloads}\Organized\{FileExtension}", true);
                
            return builder.Build();
        }
    }
} 