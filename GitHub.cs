using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace APSIM.Builds.Migrator
{
    /// <summary>
    /// Encapsulates API calls made to github.
    /// </summary>
    public class GitHub
    {
        /// <summary>
        /// Name of an environment variable which holds the github API token used
        /// to authenticate requests to github's REST API.
        /// </summary>
        private const string githubTokenVariableName = "GITHUB_PAT";

        /// <summary>
        /// List of keywords which may be used to indicate progress on an issue.
        /// </summary>
        private static readonly string[] workingOnSyntax = { "working on", "'working on'" };

        /// <summary>
        /// List of keywords which may be used to close an issue.
        /// </summary>
        /// <remarks>
        /// Taken from here: https://help.github.com/articles/closing-issues-using-keywords/
        /// </remarks>
        private static readonly string[] resolvesSyntax = { "close", "closes", "closed", "fix", "fixes", "fixed", "resolve", "resolves", "resolved", "address" };

        // /// <summary>
        // /// Get metadata for a pull request on the given repository.
        // /// </summary>
        // /// <param name="pullRequest">Pull request ID.</param>
        // /// <param name="owner">Pull request repository owner.</param>
        // /// <param name="repo">Pull request repository name.</param>
        // public async Task<PullRequestMetadata> GetMetadataAsync(uint pullRequestNumber, string owner, string repo)
        // {
        //     PullRequest pullRequest = await GetPullRequestAsync((int)pullRequestNumber, owner, repo);
        //     return await GetMetadataAsync(pullRequest, owner, repo);
        // }

        /// <summary>
        /// Fetches information about a GitHub pull request.
        /// </summary>
        /// <param name="id">ID of the pull request.</param>
        /// <param name="owner">Owner of the GitHub repository on which the pull request was created.</param>
        /// <param name="repo">Name of the GitHub repository on which the pull request was created.</param>
        private PullRequest GetPullRequest(int id, string owner, string repo)
        {
            IGitHubClient client = CreateClient();
            Task<PullRequest> task = client.PullRequest.Get(owner, repo, id);
            task.Wait();
            return task.Result;
        }

        internal string GetIssueTitle(string owner, string repo, int issueNumber)
        {
            Issue issue = GetIssue(issueNumber, owner, repo);
            return issue.Title;
        }

        /// <summary>
        /// Fetches information about a GitHub issue.
        /// </summary>
        /// <param name="id">ID of the issue.</param>
        /// <param name="owner">Owner of the GitHub repository on which the issue was created.</param>
        /// <param name="repo">Name of the GitHub repository on which the issue was created.</param>
        public Issue GetIssue(int id, string owner, string repo)
        {
            IGitHubClient client = CreateClient();
            Task<Issue> task = client.Issue.Get(owner, repo, id);
            task.Wait();
            return task.Result;
        }

        public int GetReferencedIssue(int pullRequestId, string owner, string repo)
        {
            if (repo == "APSIMClassic")
            {
                if (pullRequestId == 1775)
                    return 1741;
                if (pullRequestId == 1782)
                    return 1786;
                if (pullRequestId == 1809)
                    return 1770;
                if (pullRequestId == 1836)
                    return 1736;
                if (pullRequestId == 1872)
                    return 1642;
                if (pullRequestId == 1907)
                    return 1883;
                if (pullRequestId == 1942)
                    return 2121;
                if (pullRequestId == 2123)
                    return 2123;
            }
            PullRequest pullRequest = GetPullRequest(pullRequestId, owner, repo);
            GetDetails(pullRequest, owner, repo, out int issueId, out _);
            if (issueId < 0)
                throw new InvalidOperationException($"Pull request #{pullRequestId} on {owner}/{repo} does not reference an issue");
            return issueId;
        }

        /// <summary>
        /// Get the date on which tests were run for the given pull request.
        /// </summary>
        /// <param name="pull">The pull request.</param>
        /// <param name="owner">Owner of the pull request's repo.</param>
        /// <param name="repo">Name of the pull request's repo.</param>
        private async Task<DateTime> GetTestDateAsync(PullRequest pull, string owner, string repo)
        {
            IGitHubClient client = CreateClient();

            IReadOnlyList<PullRequestCommit> commits = await client.PullRequest.Commits(owner, repo, pull.Number);
            if (commits.Any())
            {
                CombinedCommitStatus combinedStatus = await client.Repository.Status.GetCombined(owner, repo, commits.Last().Sha);
                if (combinedStatus.Statuses.Any())
                    return combinedStatus.Statuses.Last().UpdatedAt.LocalDateTime;
            }
            return pull.UpdatedAt.DateTime;
        }

        // /// <summary>
        // /// Get metadata for a pull request on the given repository.
        // /// </summary>
        // /// <param name="pullRequest">Pull request.</param>
        // /// <param name="owner">Pull request repository owner.</param>
        // /// <param name="repo">Pull request repository name.</param>
        // private async Task<PullRequestMetadata> GetMetadataAsync(PullRequest pullRequest, string owner, string repo)
        // {
        //     GetDetails(pullRequest, out int issueNumber, out bool resolves);
        //     Issue issue = await GetIssueAsync(issueNumber, owner, repo);
        //     IssueMetadata issueMetadata = new IssueMetadata((uint)issueNumber, issue.Title, issue.Url);
        //     return new PullRequestMetadata(issueMetadata, resolves, pullRequest.Title, pullRequest.User.Login);
        // }

        /// <summary>
        /// Gets details about the issue addressed by a pull request.
        /// </summary>
        /// <param name="pullRequest">The pull request.</param>
        /// <param name="issueID">
        /// ID of the first issue referenced using one of the keywords in
        /// <see cref="resolvesSyntax"/> and <see cref="workingOnSyntax"/>.
        /// </param>
        /// <param name="resolves">True iff the pull request resolves an issue.</param>
        private void GetDetails(PullRequest pullRequest, string owner, string repo, out int issueID, out bool resolves)
        {
            issueID = -1;
            resolves = false;
            int pos = int.MaxValue;

            for (int i = 0; i < resolvesSyntax.Length + workingOnSyntax.Length; i++)
            {
                string syntax = i < resolvesSyntax.Length ? resolvesSyntax[i] : workingOnSyntax[i - resolvesSyntax.Length];
                string[] syntaces = new string[2]
                {
                    $@"{syntax}\s+#(\d+)",
                    $@"{syntax}\s+https?://github.com/{owner}/{repo}/issues/(\d+)"
                };
                foreach (string pattern in syntaces)
                {
                    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = regex.Matches(pullRequest.Body);
                    foreach (Match match in matches)
                    {
                        if (match.Index < pos)
                        {
                            pos = match.Index;
                            issueID = int.Parse(match.Groups[1].Value);
                            if (i < resolvesSyntax.Length)
                                resolves = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a github client to use for API requests.
        /// </summary>
        /// <param name="username">Github organisation/username to use for requests.</param>
        private IGitHubClient CreateClient()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("hol430"));
            client.Credentials = GetGitHubCredentials();
            return client;
        }

        /// <summary>
        /// Gets credentials which will be passed to octokit to make API requests.
        /// </summary>
        private static Credentials GetGitHubCredentials()
        {
            return new Credentials(EnvironmentVariable.Read(githubTokenVariableName, "GitHub API token"));
        }
    }
}
