using System.ComponentModel.DataAnnotations;

namespace CommonUtil
{
    public enum Permission
    {
        [Display(GroupName = "IAM & Admin", Name = "Viewer", Description = "User Account Viewer")]
        UserAccountViewer = 1,
        [Display(GroupName = "IAM & Admin", Name = "Creator", Description = "User Account Creator")]
        UserAccountCreator = 2,
        [Display(GroupName = "IAM & Admin", Name = "Editor", Description = "User Account Editor")]
        UserAccountEditor = 3,
        [Display(GroupName = "IAM & Admin", Name = "Admin", Description = "User Account Admin")]
        UserAccountAdmin = 4,
        [Display(GroupName = "Sample Module", Name = "Viewer", Description = "Sample Module Viewer")]
        MotorCovernoteViewer = 11,
        [Display(GroupName = "Sample Module", Name = "Creator", Description = "Sample Module Creator")]
        MotorCovernoteCreator = 12,
        [Display(GroupName = "Sample Module", Name = "Editor", Description = "Sample Module Editor")]
        MotorCovernoteEditor = 13,
        [Display(GroupName = "Sample Module", Name = "Admin", Description = "Sample Module Admin")]
        MotorCovernoteAdmin = 14
    }
}
