using System;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Represents the current state of template installation
    /// </summary>
    public class InstallationProgress
    {
        public string CurrentStep { get; set; }
        public float Progress { get; set; }
        public bool IsComplete { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        public event Action<InstallationProgress> OnProgressChanged;

        public void UpdateProgress(string step, float progress)
        {
            CurrentStep = step;
            Progress = progress;
            OnProgressChanged?.Invoke(this);
        }

        public void Complete()
        {
            IsComplete = true;
            Progress = 1f;
            OnProgressChanged?.Invoke(this);
        }

        public void ReportError(string error)
        {
            HasError = true;
            ErrorMessage = error;
            OnProgressChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Result of template installation operation
    /// </summary>
    public class InstallationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[] CreatedFolders { get; set; }
        public string[] CreatedScenes { get; set; }
        public string[] CreatedAddressableGroups { get; set; }
        public string[] CreatedLocalizationTables { get; set; }
        public string ErrorDetails { get; set; }

        public static InstallationResult Failure(string message, string details = "")
        {
            return new InstallationResult
            {
                Success = false,
                Message = message,
                ErrorDetails = details
            };
        }

        public static InstallationResult SuccessResult(string message)
        {
            return new InstallationResult
            {
                Success = true,
                Message = message
            };
        }
    }
}
