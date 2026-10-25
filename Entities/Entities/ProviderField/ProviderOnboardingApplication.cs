using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    public class ProviderOnboardingApplication : Id_Field
    {
        public long UserId { get; set; }
        public long? ProviderId { get; set; }
        public string Kind { get; set; } = "Provider";
        public string RoleLabel { get; set; }
        public string Status { get; set; } = "Draft";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public string BirthDate { get; set; }
        public string ApplicantType { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNationalId { get; set; }
        public string City { get; set; }
        public string BusinessName { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessPhone { get; set; }
        public string DetailsJson { get; set; }
        public string ReviewerNote { get; set; }
        public long? ReviewedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public ICollection<ProviderOnboardingDocument> Documents { get; set; }
        public ICollection<ProviderOnboardingEvent> Events { get; set; }
    }

    public class ProviderOnboardingDocument : Id_Field
    {
        public long ApplicationId { get; set; }
        public string Kind { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        // AES-GCM protected base64. Never expose this through list/detail endpoints.
        public string ProtectedContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProviderOnboardingApplication Application { get; set; }
    }

    public class ProviderOnboardingEvent : Id_Field
    {
        public long ApplicationId { get; set; }
        public long ActorUserId { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProviderOnboardingApplication Application { get; set; }
    }
}
