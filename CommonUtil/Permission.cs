using System.ComponentModel.DataAnnotations;
using CommonUtil.Attributes;

namespace CommonUtil
{
    public enum Permission
    {
        [Display(GroupName = "IAM & Admin", Name = "Viewer", Description = "User Account Viewer"), Audience(audience: "http://identity.api")]
        UserAccountViewer = 1,
        [Display(GroupName = "IAM & Admin", Name = "Creator", Description = "User Account Creator"), Audience(audience: "http://identity.api")]
        UserAccountCreator = 2,
        [Display(GroupName = "IAM & Admin", Name = "Editor", Description = "User Account Editor"), Audience(audience: "http://identity.api")]
        UserAccountEditor = 3,
        [Display(GroupName = "IAM & Admin", Name = "Admin", Description = "User Account Admin"), Audience(audience: "http://identity.api")]
        UserAccountAdmin = 4,
        [Display(GroupName = "Sample Module", Name = "Viewer", Description = "Sample Module Viewer"), Audience(audience: "http://sample.api")]
        SampleModuleViewer = 11,
        [Display(GroupName = "Sample Module", Name = "Creator", Description = "Sample Module Creator"), Audience(audience: "http://sample.api")]
        SampleModuleCreator = 12,
        [Display(GroupName = "Sample Module", Name = "Editor", Description = "Sample Module Editor"), Audience(audience: "http://sample.api")]
        SampleModuleEditor = 13,
        [Display(GroupName = "Sample Module", Name = "Admin", Description = "Sample Module Admin"), Audience(audience: "http://sample.api")]
        SampleModuleAdmin = 14
    }
}
