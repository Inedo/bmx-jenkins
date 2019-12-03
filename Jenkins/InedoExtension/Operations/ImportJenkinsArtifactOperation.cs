﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Operations;
using Inedo.Web;

namespace Inedo.Extensions.Jenkins.Operations
{
    [DisplayName("Import Artifact from Jenkins")]
    [Description("Downloads an artifact from the specified Jenkins server and saves it to the artifact library.")]
    [ScriptAlias("Import-Artifact")]
    [Tag("artifacts")]
    [Tag("jenkins")]
    [AppliesTo(InedoProduct.BuildMaster)]
    public sealed class ImportJenkinsArtifactOperation : JenkinsOperation
    {
        [ScriptAlias("Credentials")]
        [DisplayName("Credentials")]
        public override string CredentialName { get; set; }

        [Required]
        [ScriptAlias("Job")]
        [DisplayName("Job name")]
        [SuggestableValue(typeof(JobNameSuggestionProvider))]
        public string JobName { get; set; }

        [ScriptAlias("Branch")]
        [DisplayName("Branch name")]
        [SuggestableValue(typeof(BranchNameSuggestionProvider))]
        [Description("The branch name is required for a Jenkins multi-branch project, otherwise should be left empty.")]
        public string BranchName { get; set; }

        [ScriptAlias("BuildNumber")]
        [DisplayName("Build number")]
        [DefaultValue("lastSuccessfulBuild")]
        [PlaceholderText("lastSuccessfulBuild")]
        [Description("The build number may be a specific build number, or a special value such as \"lastSuccessfulBuild\", \"lastStableBuild\", \"lastBuild\", or \"lastCompletedBuild\".")]
        [SuggestableValue(typeof(BuildNumberSuggestionProvider))]
        public string BuildNumber { get; set; }

        [Required]
        [ScriptAlias("Artifact")]
        [DisplayName("Artifact name")]
        [DefaultValue("archive.zip")]
        [Description("The name of the artifact in BuildMaster once it is captured from the {jenkinsUrl}/job/{jobName}/{buildNumber}/artifact/*zip*/archive.zip endpoint.")]
        public string ArtifactName { get; set; }

        [Output]
        [ScriptAlias("JenkinsBuildNumber")]
        [DisplayName("Set build number to variable")]
        [Description("The Jenkins build number can be output into a runtime variable.")]
        [PlaceholderText("e.g. $JenkinsBuildNumber")]
        public string JenkinsBuildNumber { get; set; }

        public async override Task ExecuteAsync(IOperationExecutionContext context)
        {
            var importer = new JenkinsArtifactImporter((IJenkinsConnectionInfo)this, this, context)
            {
                ArtifactName = this.ArtifactName,
                BuildNumber = this.BuildNumber,
                BranchName = this.BranchName,
                JobName = this.JobName
            };

            this.JenkinsBuildNumber = await importer.ImportAsync().ConfigureAwait(false);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            string buildNumber = config[nameof(this.BuildNumber)];

            var desc = new List<object>();
            desc.Add("of build ");
            desc.Add(AH.ParseInt(buildNumber) != null ? "#" : "");
            desc.Add(new Hilite(buildNumber));
            if (!string.IsNullOrEmpty(this.BranchName))
            {
                desc.Add(" on  branch ");
                desc.Add(new Hilite(this.BranchName));
            }
            desc.Add(" for job ");
            desc.Add(new Hilite(config[nameof(this.JobName)]));
            
            return new ExtendedRichDescription(
                new RichDescription("Import Jenkins ", new Hilite(config[nameof(this.ArtifactName)]), " Artifact "),
                new RichDescription(desc.ToArray())
            );
        }
    }
}
